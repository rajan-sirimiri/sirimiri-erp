using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClosedXML.Excel;
using MySql.Data.MySqlClient;

namespace HRModule
{
    /// <summary>
    /// Employee Excel bulk import.
    /// Follows Session 9/10 pattern:
    ///   - pnlResults.Visible = true restored on every postback via hfFilePath check
    ///   - Server-side parse, show preview grid, confirm via browser confirm()
    ///   - Auto-create missing department (mirrors AutoMatchCustomers pattern)
    ///
    /// Session 13 optimization (this file):
    ///   - btnConfirm_Click now uses a single connection + single transaction
    ///   - All employee codes pre-allocated via GenerateEmployeeCodeBatch
    ///     (1 query for the prefix table, 1 grouped query for max suffix per
    ///     prefix — vs. ~2 queries per row before)
    ///   - Department lookups resolved up-front against the cached
    ///     GetDepartments() result, not per-row DB hits
    ///   - Server-side single-use token in Session prevents duplicate
    ///     submissions (browser timeout retry, double-click, back button)
    ///   - Server.ScriptTimeout bumped to 600s for the confirm postback
    ///
    /// Net: 57 rows go from ~230 round-trips on 230 connections to
    /// ~60 round-trips on 1 connection in 1 transaction.
    /// </summary>
    public partial class HREmployeeImport : System.Web.UI.Page
    {
        // Session keys for the double-submit guard. The token is created
        // on first preview, consumed atomically on confirm. If a second
        // confirm postback arrives (refresh, back button, retry), the
        // token has already been cleared and the second insert is rejected.
        private const string SK_ImportToken = "HR_ImportToken";

        protected void Page_Load(object sender, EventArgs e)
        {
            // --- Auth gate ---
            if (Session["HR_UserID"] == null && Session["UserID"] == null)
            {
                Response.Redirect("HRLogin.aspx", true);
                return;
            }

            // --- Role gate: Super / Admin only ---
            string role = (Session["HR_Role"] as string) ?? (Session["UserRole"] as string) ?? (Session["Role"] as string);
            if (role != "Super" && role != "Admin")
            {
                Response.Redirect("HRLogin.aspx", true);
                return;
            }

            // Show user name in top nav (only if the new control exists in markup)
            string navName = (Session["HR_FullName"] as string) ?? (Session["FullName"] as string)
                          ?? (Session["UserName"] as string) ?? "";
            if (!string.IsNullOrEmpty(navName) && lblNavUser != null) lblNavUser.Text = navName;

            // --- Postback: if a file path is pinned, rebuild the preview panel ---
            // Otherwise controls inside pnlResults won't render on postback
            // (lesson from Session 9: panels with Visible=false don't instantiate controls)
            if (IsPostBack && !string.IsNullOrEmpty(hfFilePath.Value) && File.Exists(hfFilePath.Value))
            {
                pnlResults.Visible = true;
            }
        }

        // =================================================================
        // Upload / Preview
        // =================================================================
        protected void btnUpload_Click(object sender, EventArgs e)
        {
            if (!fuFile.HasFile) { ShowMsg("Please choose an Excel file.", "err"); return; }

            string ext = Path.GetExtension(fuFile.FileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
            { ShowMsg("File must be .xlsx or .xls", "err"); return; }

            try
            {
                string tempDir = Server.MapPath("~/App_Data/ImportTemp");
                if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);
                string saveTo = Path.Combine(tempDir,
                    "EmpImport_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + ext);
                fuFile.SaveAs(saveTo);
                hfFilePath.Value = saveTo;

                List<ImportRow> rows = ParseExcel(saveTo, out string parseError);
                if (parseError != null) { ShowMsg("Parse error: " + parseError, "err"); return; }

                ValidateRows(rows);
                ViewState["ImportRows"] = rows;

                // Mint a fresh single-use token for this preview. Replaces any
                // prior token (e.g. user uploaded a different file mid-flow).
                Session[SK_ImportToken] = Guid.NewGuid().ToString("N");

                BindPreview(rows);
                pnlResults.Visible = true;
            }
            catch (Exception ex)
            {
                ShowMsg("Upload failed: " + ex.Message, "err");
            }
        }

        // =================================================================
        // Confirm import — Session 13 batched path
        // =================================================================
        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            // ---- Double-submit guard (atomic claim of the token) ----
            // Browser-timeout retries, refreshes, double-clicks, back-button
            // all hit this same handler. Only the first one through wins.
            string claimedToken = null;
            lock (((System.Collections.IDictionary)Session).SyncRoot)
            {
                claimedToken = Session[SK_ImportToken] as string;
                if (string.IsNullOrEmpty(claimedToken))
                {
                    ShowMsg("This import has already been submitted (or the page was reloaded). " +
                            "Please upload the file again to start a new import.", "err");
                    pnlResults.Visible = false;
                    return;
                }
                Session.Remove(SK_ImportToken);
            }

            // Don't let .NET kill the request mid-batch on slow VPS / first-time JIT.
            Server.ScriptTimeout = 600;

            List<ImportRow> rows = ViewState["ImportRows"] as List<ImportRow>;
            if (rows == null) { ShowMsg("Nothing to import. Please upload again.", "err"); return; }

            string user = (Session["UserName"] as string) ?? "SYSTEM";

            int inserted = 0, failed = 0;
            List<string> errors = new List<string>();
            List<ImportRow> readyRows = rows.Where(x => x.Status == "READY").ToList();

            // Pre-flight sanity: nothing to do?
            if (readyRows.Count == 0)
            {
                ShowMsg("No READY rows to import.", "warn");
                pnlResults.Visible = false;
                return;
            }

            // ---- Phase 1: build dept-name -> deptId map (1 query) ----
            // Auto-create any missing depts in their own short transactions
            // BEFORE the main insert transaction opens, so we don't hold the
            // big transaction across dept creation. New depts created this way
            // get NULL CodePrefix — those rows will fail allocation in Phase 2
            // (with a clear message), which is the safe behaviour.
            Dictionary<string, int> deptByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            try
            {
                DataTable depts = HR_DatabaseHelper.GetDepartments(false);
                foreach (DataRow dr in depts.Rows)
                {
                    string n = dr["DeptName"].ToString();
                    int id = Convert.ToInt32(Convert.ToInt64(dr["DeptID"]));
                    deptByName[n] = id;
                }

                // Auto-create missing departments if checkbox is on.
                foreach (ImportRow r in readyRows)
                {
                    if (string.IsNullOrWhiteSpace(r.Department)) continue;
                    string dn = r.Department.Trim();
                    if (deptByName.ContainsKey(dn)) continue;

                    if (chkAutoCreateDept.Checked)
                    {
                        int newId = HR_DatabaseHelper.GetOrCreateDepartment(dn, user);
                        if (newId > 0) deptByName[dn] = newId;
                    }
                    // If auto-create is off, leave the row to fail in Phase 3 with
                    // a clear "department not found" message.
                }
            }
            catch (Exception ex)
            {
                ShowMsg("Setup failed (department resolution): " + ex.Message, "err");
                pnlResults.Visible = false;
                return;
            }

            // ---- Phase 2 + 3: open ONE connection, ONE transaction ----
            // All inserts share it. If any single row fails, we record the
            // error and continue (savepoint/rollback per row), so one bad row
            // doesn't lose the rest of the batch — this matches the old
            // per-row try/catch behaviour, just much faster.
            using (MySqlConnection con = HR_DatabaseHelper.GetConnection())
            {
                con.Open();

                Dictionary<int, HR_DatabaseHelper.DeptCodeAllocator> allocators;
                MySqlTransaction tx = con.BeginTransaction();
                bool committed = false;

                try
                {
                    // Pre-allocate code counters for every dept that has a CodePrefix.
                    // Single round-trip (a UNION ALL of one MAX-per-prefix subquery).
                    allocators = HR_DatabaseHelper.GenerateEmployeeCodeBatch(con, tx);

                    // ---- Phase 3: per-row insert loop ----
                    foreach (ImportRow r in readyRows)
                    {
                        // Sub-transaction per row via savepoint, so a single
                        // failing row doesn't poison the whole batch.
                        string sp = "sp_" + r.RowNum.ToString();
                        using (MySqlCommand spCmd = new MySqlCommand("SAVEPOINT " + sp, con, tx))
                            spCmd.ExecuteNonQuery();

                        try
                        {
                            // Resolve dept (lookup, no DB hit)
                            int deptId;
                            if (string.IsNullOrWhiteSpace(r.Department) ||
                                !deptByName.TryGetValue(r.Department.Trim(), out deptId))
                            {
                                throw new InvalidOperationException(
                                    "Department '" + (r.Department ?? "") + "' not found.");
                            }

                            // Resolve / allocate EmployeeCode
                            if (!IsRealEmployeeCode(r.EmployeeCode))
                            {
                                if (!allocators.TryGetValue(deptId, out var alloc))
                                {
                                    throw new InvalidOperationException(
                                        "Department '" + r.Department + "' has no CodePrefix configured. " +
                                        "Set HR_Department.CodePrefix before importing employees for this dept.");
                                }
                                r.EmployeeCode = alloc.Allocate();
                            }

                            EmployeeRecord emp = new EmployeeRecord
                            {
                                EmployeeCode   = r.EmployeeCode,
                                FullName       = r.FullName,
                                FatherName     = r.FatherName,
                                Gender         = string.IsNullOrEmpty(r.Gender) ? "M" : r.Gender,
                                DOB            = r.DOB,
                                DOJ            = r.DOJ ?? DateTime.Today,
                                DeptID         = deptId,
                                Designation    = r.Designation,
                                ReportingManager = r.ReportingManager,
                                Zone           = r.Zone,
                                Region         = r.Region,
                                Area           = r.Area,
                                WorkLocation   = r.WorkLocation,
                                EmploymentType = string.IsNullOrEmpty(r.EmploymentType) ? "Permanent" : r.EmploymentType,
                                MobileNo       = r.MobileNo,
                                AltMobileNo    = r.AltMobileNo,
                                Email          = r.Email,
                                AddressLine    = r.AddressLine,
                                City           = r.City,
                                StateName      = r.StateName,
                                Pincode        = r.Pincode,
                                AadhaarNo      = r.AadhaarNo,
                                PANNo          = r.PANNo,
                                UANNo          = r.UANNo,
                                PFNo           = r.PFNo,
                                ESINo          = r.ESINo,
                                BankAccountNo  = r.BankAccountNo,
                                BankName       = r.BankName,
                                IFSCCode       = r.IFSCCode,
                                BasicSalary    = r.BasicSalary,
                                HRA            = r.HRA,
                                ConveyanceAllow= r.ConveyanceAllow,
                                OtherAllow     = r.OtherAllow,
                                GrossSalary    = r.GrossSalary,
                                IsActive       = true
                            };

                            HR_DatabaseHelper.InsertEmployee(emp, user, con, tx);

                            // Row inserted cleanly — drop the savepoint so it merges into the parent tx.
                            using (MySqlCommand rel = new MySqlCommand("RELEASE SAVEPOINT " + sp, con, tx))
                                rel.ExecuteNonQuery();

                            inserted++;
                        }
                        catch (Exception rowEx)
                        {
                            // Roll back just this row — keep going for the rest.
                            try
                            {
                                using (MySqlCommand rb = new MySqlCommand("ROLLBACK TO SAVEPOINT " + sp, con, tx))
                                    rb.ExecuteNonQuery();
                                using (MySqlCommand rel = new MySqlCommand("RELEASE SAVEPOINT " + sp, con, tx))
                                    rel.ExecuteNonQuery();
                            }
                            catch { /* savepoint already gone — ignore */ }

                            failed++;
                            errors.Add("Row " + r.RowNum + ": " + rowEx.Message);
                        }
                    }

                    tx.Commit();
                    committed = true;
                }
                finally
                {
                    if (!committed)
                    {
                        try { tx.Rollback(); } catch { }
                    }
                    tx.Dispose();
                }
            }

            // Clean up temp file and viewstate
            try { if (!string.IsNullOrEmpty(hfFilePath.Value) && File.Exists(hfFilePath.Value)) File.Delete(hfFilePath.Value); } catch { }
            hfFilePath.Value = "";
            ViewState.Remove("ImportRows");

            string msg = "Imported " + inserted + " employee(s).";
            if (failed > 0) msg += " Failed: " + failed + ". First errors: " + string.Join("; ", errors.Take(3));
            ShowMsg(msg, failed > 0 ? "warn" : "ok");

            pnlResults.Visible = false;
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            try { if (File.Exists(hfFilePath.Value)) File.Delete(hfFilePath.Value); } catch { }
            hfFilePath.Value = "";
            ViewState.Remove("ImportRows");
            Session.Remove(SK_ImportToken);
            pnlResults.Visible = false;
            pnlMsg.Visible = false;
        }

        // =================================================================
        // Row-level styling
        // =================================================================
        protected void gvPreview_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            DataRowView drv = e.Row.DataItem as DataRowView;
            if (drv == null) return;
            string status = drv["Status"] as string;
            string msg = drv["Message"] as string;
            if (status == "ERROR") e.Row.CssClass = "row-err";
            else if (!string.IsNullOrEmpty(msg)) e.Row.CssClass = "row-warn";
        }

        // =================================================================
        // Excel parsing (ClosedXML 0.95.0)
        // =================================================================
        private List<ImportRow> ParseExcel(string path, out string error)
        {
            error = null;
            List<ImportRow> rows = new List<ImportRow>();

            using (XLWorkbook wb = new XLWorkbook(path))
            {
                IXLWorksheet ws = wb.Worksheet(1);
                IXLRange used = ws.RangeUsed();
                if (used == null) { error = "Sheet is empty."; return rows; }

                // Header row -> column index map
                Dictionary<string, int> col = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                IXLRow hdr = ws.FirstRowUsed();
                int c = 1;
                foreach (IXLCell cell in hdr.Cells())
                {
                    string h = (cell.GetString() ?? "").Trim();
                    if (h.Length > 0 && !col.ContainsKey(h)) col[h] = c;
                    c++;
                }

                int firstDataRow = used.FirstRow().RowNumber() + 1;
                int lastRow = used.LastRow().RowNumber();
                int rowNum = 0;

                for (int r = firstDataRow; r <= lastRow; r++)
                {
                    rowNum++;
                    IXLRow xlRow = ws.Row(r);
                    if (xlRow.IsEmpty()) { rowNum--; continue; }

                    ImportRow ir = new ImportRow
                    {
                        RowNum = rowNum,
                        EmployeeCode  = GetStr(ws, r, col, "EmployeeCode"),
                        FullName      = GetStr(ws, r, col, "FullName"),
                        FatherName    = GetStr(ws, r, col, "FatherName"),
                        Gender        = NormGender(GetStr(ws, r, col, "Gender")),
                        DOB           = GetDate(ws, r, col, "DOB"),
                        DOJ           = GetDate(ws, r, col, "DOJ"),
                        Department    = GetStr(ws, r, col, "Department"),
                        Designation   = GetStr(ws, r, col, "Designation"),
                        ReportingManager = GetStr(ws, r, col, "ReportingManager"),
                        Zone          = GetStr(ws, r, col, "Zone"),
                        Region        = GetStr(ws, r, col, "Region"),
                        Area          = GetStr(ws, r, col, "Area"),
                        WorkLocation  = GetStr(ws, r, col, "WorkLocation"),
                        EmploymentType= NormEmpType(GetStr(ws, r, col, "EmploymentType")),
                        MobileNo      = GetStr(ws, r, col, "Mobile"),
                        AltMobileNo   = GetStr(ws, r, col, "AltMobile"),
                        Email         = GetStr(ws, r, col, "Email"),
                        AddressLine   = GetStr(ws, r, col, "Address"),
                        City          = GetStr(ws, r, col, "City"),
                        StateName     = GetStr(ws, r, col, "State"),
                        Pincode       = GetStr(ws, r, col, "Pincode"),
                        AadhaarNo     = CleanAadhaar(GetStr(ws, r, col, "Aadhaar")),
                        PANNo         = (GetStr(ws, r, col, "PAN") ?? "").ToUpperInvariant(),
                        UANNo         = GetStr(ws, r, col, "UAN"),
                        PFNo          = GetStr(ws, r, col, "PFNo"),
                        ESINo         = GetStr(ws, r, col, "ESINo"),
                        BankAccountNo = GetStr(ws, r, col, "BankAcNo"),
                        BankName      = GetStr(ws, r, col, "BankName"),
                        IFSCCode      = (GetStr(ws, r, col, "IFSC") ?? "").ToUpperInvariant(),
                        BasicSalary   = GetDec(ws, r, col, "Basic"),
                        HRA           = GetDec(ws, r, col, "HRA"),
                        ConveyanceAllow = GetDec(ws, r, col, "Conveyance"),
                        OtherAllow    = GetDec(ws, r, col, "Other")
                    };
                    ir.GrossSalary = ir.BasicSalary + ir.HRA + ir.ConveyanceAllow + ir.OtherAllow;

                    // Skip fully-blank trailing rows
                    if (string.IsNullOrWhiteSpace(ir.FullName) && !IsRealEmployeeCode(ir.EmployeeCode))
                    { rowNum--; continue; }

                    rows.Add(ir);
                }
            }

            return rows;
        }

        // =================================================================
        // Row validation
        // =================================================================
        private void ValidateRows(List<ImportRow> rows)
        {
            HashSet<string> existingCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            DataTable existing = HR_DatabaseHelper.GetEmployees(null, null, false);
            foreach (DataRow er in existing.Rows)
                existingCodes.Add(er["EmployeeCode"].ToString());

            HashSet<string> seenInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            DataTable depts = HR_DatabaseHelper.GetDepartments(false);
            HashSet<string> deptNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow dr in depts.Rows) deptNames.Add(dr["DeptName"].ToString());

            foreach (ImportRow r in rows)
            {
                List<string> errs = new List<string>();
                List<string> warns = new List<string>();

                if (string.IsNullOrWhiteSpace(r.FullName)) errs.Add("Name missing");
                if (r.DOJ == null) errs.Add("DOJ missing");
                if (string.IsNullOrWhiteSpace(r.Department)) errs.Add("Department missing");
                else if (!deptNames.Contains(r.Department))
                {
                    if (chkAutoCreateDept.Checked) warns.Add("New dept '" + r.Department + "' will be created");
                    else errs.Add("Department '" + r.Department + "' not found");
                }

                if (IsRealEmployeeCode(r.EmployeeCode))
                {
                    if (existingCodes.Contains(r.EmployeeCode)) errs.Add("Code already exists in DB");
                    else if (!seenInFile.Add(r.EmployeeCode)) errs.Add("Duplicate code in file");
                }
                // non-real (blank or numeric like "1") -> auto-generate at import time

                if (!HR_DatabaseHelper.IsValidAadhaar(r.AadhaarNo)) errs.Add("Aadhaar not 12 digits");
                if (!HR_DatabaseHelper.IsValidPAN(r.PANNo))         warns.Add("PAN format unusual");
                if (!HR_DatabaseHelper.IsValidIFSC(r.IFSCCode))     warns.Add("IFSC format unusual");

                if (errs.Count > 0)
                {
                    r.Status = "ERROR";
                    r.Message = string.Join("; ", errs);
                }
                else if (warns.Count > 0)
                {
                    r.Status = "READY";   // still importable
                    r.Message = "⚠ " + string.Join("; ", warns);
                }
                else
                {
                    r.Status = "READY";
                    r.Message = "";
                }
            }
        }

        private void BindPreview(List<ImportRow> rows)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("RowNum", typeof(int));
            dt.Columns.Add("Status", typeof(string));
            dt.Columns.Add("Message", typeof(string));
            dt.Columns.Add("EmployeeCode", typeof(string));
            dt.Columns.Add("FullName", typeof(string));
            dt.Columns.Add("Department", typeof(string));
            dt.Columns.Add("Designation", typeof(string));
            dt.Columns.Add("ReportingManager", typeof(string));
            dt.Columns.Add("Zone", typeof(string));
            dt.Columns.Add("Region", typeof(string));
            dt.Columns.Add("Area", typeof(string));
            dt.Columns.Add("WorkLocation", typeof(string));
            dt.Columns.Add("EmploymentType", typeof(string));
            dt.Columns.Add("DOJ", typeof(DateTime));
            dt.Columns.Add("MobileNo", typeof(string));
            dt.Columns.Add("AadhaarNo", typeof(string));
            dt.Columns.Add("GrossSalary", typeof(decimal));

            foreach (ImportRow r in rows)
            {
                DataRow dr = dt.NewRow();
                dr["RowNum"] = r.RowNum;
                dr["Status"] = r.Status;
                dr["Message"] = r.Message;
                dr["EmployeeCode"] = IsRealEmployeeCode(r.EmployeeCode) ? r.EmployeeCode : "(auto)";
                dr["FullName"] = r.FullName ?? "";
                dr["Department"] = r.Department ?? "";
                dr["Designation"] = r.Designation ?? "";
                dr["ReportingManager"] = r.ReportingManager ?? "";
                dr["Zone"] = r.Zone ?? "";
                dr["Region"] = r.Region ?? "";
                dr["Area"] = r.Area ?? "";
                dr["WorkLocation"] = r.WorkLocation ?? "";
                dr["EmploymentType"] = r.EmploymentType ?? "";
                dr["DOJ"] = (object)r.DOJ ?? DBNull.Value;
                dr["MobileNo"] = r.MobileNo ?? "";
                dr["AadhaarNo"] = r.AadhaarNo ?? "";
                dr["GrossSalary"] = r.GrossSalary;
                dt.Rows.Add(dr);
            }

            gvPreview.DataSource = dt;
            gvPreview.DataBind();

            int total = rows.Count;
            int ok = rows.Count(x => x.Status == "READY" && string.IsNullOrEmpty(x.Message));
            int warn = rows.Count(x => x.Status == "READY" && !string.IsNullOrEmpty(x.Message));
            int err = rows.Count(x => x.Status == "ERROR");

            litTotal.Text = total.ToString();
            litOK.Text = ok.ToString();
            litWarn.Text = warn.ToString();
            litErr.Text = err.ToString();
        }

        private void ShowMsg(string text, string kind)
        {
            pnlMsg.Visible = true;
            // Map old keys to new banner classes
            string css = "banner banner-info";
            if (kind == "ok")   css = "banner banner-success";
            if (kind == "err")  css = "banner banner-error";
            if (kind == "warn") css = "banner banner-info";
            pnlMsg.CssClass = css;
            pnlMsg.Controls.Clear();
            pnlMsg.Controls.Add(new LiteralControl(Server.HtmlEncode(text)));
        }

        // =================================================================
        // Excel cell helpers (tolerate header aliases)
        // =================================================================
        private static readonly Dictionary<string, string[]> Aliases = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "EmployeeCode", new[]{ "EmployeeCode", "EmpCode", "Code", "S.No", "SNo", "Sl No", "Sr No", "Serial No" } },
            { "FullName",     new[]{ "FullName", "Name", "Employee Name", "Employee name" } },
            { "FatherName",   new[]{ "FatherName", "Father's Name", "Father" } },
            { "Gender",       new[]{ "Gender", "Sex" } },
            { "DOB",          new[]{ "DOB", "Date of Birth", "Birth Date", "Date Of Birth" } },
            { "DOJ",          new[]{ "DOJ", "Date of Joining", "Joining Date", "Date Of Joining", "DOJ" } },
            { "Department",   new[]{ "Department", "Dept" } },
            { "Designation",  new[]{ "Designation", "Position", "Role" } },
            { "ReportingManager", new[]{ "ReportingManager", "Reporting Manager", "Manager", "Reports To" } },
            { "Zone",         new[]{ "Zone", "State/Zone", "State Zone" } },
            { "Region",       new[]{ "Region", "REGION" } },
            { "Area",         new[]{ "Area", "AREA" } },
            { "WorkLocation", new[]{ "WorkLocation", "Work Location", "Location", "LOCATION", "Branch" } },
            { "EmploymentType", new[]{ "EmploymentType", "Type", "Employment Type" } },
            { "Mobile",       new[]{ "Mobile", "Phone", "MobileNo", "Contact", "Mobile No", "Mobile Number", "Contact No", "Contact Number" } },
            { "AltMobile",    new[]{ "AltMobile", "Alt Mobile", "Alternate Mobile" } },
            { "Email",        new[]{ "Email", "Email ID", "Email Id", "EmailAddress" } },
            { "Address",      new[]{ "Address", "Residential Address", "Permanent Address" } },
            { "City",         new[]{ "City" } },
            { "State",        new[]{ "State" } },
            { "Pincode",      new[]{ "Pincode", "Pin", "Zip", "Pin Code" } },
            { "Aadhaar",      new[]{ "Aadhaar", "Aadhar", "AadhaarNo", "Aadhar Number", "Aadhaar Number", "Aadhar No", "UID" } },
            { "PAN",          new[]{ "PAN", "PANNo", "PAN Card", "PAN No", "PAN Number" } },
            { "UAN",          new[]{ "UAN", "UANNo", "UAN No", "UAN NO", "UAN Number" } },
            { "PFNo",         new[]{ "PFNo", "PF No", "PF Number" } },
            { "ESINo",        new[]{ "ESINo", "ESI No", "ESI NO", "ESI Number" } },
            { "BankAcNo",     new[]{ "BankAcNo", "Bank A/c", "Account Number", "A/c No", "Bank A/c No", "Account No", "Bank Account" } },
            { "BankName",     new[]{ "BankName", "Bank Name", "Bank" } },
            { "IFSC",         new[]{ "IFSC", "IFSC Code", "IFSCCode" } },
            { "Basic",        new[]{ "Basic", "Basic Salary" } },
            { "HRA",          new[]{ "HRA" } },
            { "Conveyance",   new[]{ "Conveyance", "Conveyance Allow", "ConveyanceAllow" } },
            { "Other",        new[]{ "Other", "Other Allow", "OtherAllow", "Other Allowance" } },
        };

        private static string GetStr(IXLWorksheet ws, int row, Dictionary<string, int> col, string logical)
        {
            int c = ResolveCol(col, logical);
            if (c == 0) return "";
            IXLCell cell = ws.Cell(row, c);
            if (cell.IsEmpty()) return "";
            // For numeric cells, format without trailing zeros (mobile/aadhaar/account stored as int)
            if (cell.DataType == XLDataType.Number)
            {
                double n = cell.GetDouble();
                if (n == Math.Floor(n) && !double.IsInfinity(n))
                    return ((long)n).ToString();
                return n.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            string s = cell.GetString();
            return (s ?? "").Trim();
        }

        private static DateTime? GetDate(IXLWorksheet ws, int row, Dictionary<string, int> col, string logical)
        {
            int c = ResolveCol(col, logical);
            if (c == 0) return null;
            IXLCell cell = ws.Cell(row, c);
            if (cell.IsEmpty()) return null;
            if (cell.DataType == XLDataType.DateTime) return cell.GetDateTime();
            string s = cell.GetString();
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim();

            DateTime d;
            // Indian/dot/slash formats first (DD-MM-YYYY assumed)
            string[] fmts = {
                "dd-MM-yyyy", "dd/MM/yyyy", "dd.MM.yyyy",
                "d-M-yyyy",   "d/M/yyyy",   "d.M.yyyy",
                "yyyy-MM-dd", "yyyy/MM/dd",
                "dd-MMM-yyyy", "dd-MMM-yy", "dd MMM yyyy"
            };
            if (DateTime.TryParseExact(s, fmts,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out d))
                return d;

            // US-style fallback (MM/DD/YYYY) — only if first part > 12 disambiguates against DD/MM
            string[] usFmts = { "MM/dd/yyyy", "M/d/yyyy", "MM-dd-yyyy" };
            if (DateTime.TryParseExact(s, usFmts,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out d))
                return d;

            if (DateTime.TryParse(s, out d)) return d;
            return null;
        }

        private static decimal GetDec(IXLWorksheet ws, int row, Dictionary<string, int> col, string logical)
        {
            int c = ResolveCol(col, logical);
            if (c == 0) return 0m;
            IXLCell cell = ws.Cell(row, c);
            if (cell.IsEmpty()) return 0m;
            try
            {
                if (cell.DataType == XLDataType.Number) return (decimal)cell.GetDouble();
                decimal d;
                return decimal.TryParse(cell.GetString(), out d) ? d : 0m;
            }
            catch { return 0m; }
        }

        private static int ResolveCol(Dictionary<string, int> col, string logical)
        {
            if (Aliases.TryGetValue(logical, out string[] alts))
            {
                foreach (string a in alts)
                    if (col.TryGetValue(a, out int idx)) return idx;
            }
            return col.TryGetValue(logical, out int direct) ? direct : 0;
        }

        private static string CleanAadhaar(string a)
        {
            if (string.IsNullOrWhiteSpace(a)) return "";
            // Strip ALL whitespace (including double spaces in source data) plus dashes
            return System.Text.RegularExpressions.Regex.Replace(a, @"\s+", "").Replace("-", "").Trim();
        }

        // Excel sometimes provides S.No (1, 2, 3) which would alias to EmployeeCode.
        // We only treat the value as a real code if it looks like one (alpha-prefixed,
        // not a pure integer row number).
        private static bool IsRealEmployeeCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            string c = code.Trim();
            // pure-digits = row number, not a real code -> auto-generate
            int dummy;
            if (int.TryParse(c, out dummy)) return false;
            // requires at least one letter
            foreach (char ch in c) if (char.IsLetter(ch)) return true;
            return false;
        }

        private static string NormGender(string g)
        {
            if (string.IsNullOrWhiteSpace(g)) return "M";
            g = g.Trim().ToUpperInvariant();
            if (g.StartsWith("M")) return "M";
            if (g.StartsWith("F")) return "F";
            return "O";
        }

        private static string NormEmpType(string t)
        {
            if (string.IsNullOrWhiteSpace(t)) return "Permanent";
            string s = t.Trim();
            string[] valid = { "Permanent", "Contract", "Trainee", "Apprentice", "Temporary" };
            foreach (string v in valid)
                if (string.Equals(v, s, StringComparison.OrdinalIgnoreCase)) return v;
            return "Permanent";
        }

        // =================================================================
        // Import row DTO
        // =================================================================
        [Serializable]
        public class ImportRow
        {
            public int RowNum { get; set; }
            public string Status { get; set; } = "READY";
            public string Message { get; set; } = "";

            public string EmployeeCode { get; set; }
            public string FullName { get; set; }
            public string FatherName { get; set; }
            public string Gender { get; set; }
            public DateTime? DOB { get; set; }
            public DateTime? DOJ { get; set; }
            public string Department { get; set; }
            public string Designation { get; set; }
            public string ReportingManager { get; set; }
            public string Zone { get; set; }
            public string Region { get; set; }
            public string Area { get; set; }
            public string WorkLocation { get; set; }
            public string EmploymentType { get; set; }
            public string MobileNo { get; set; }
            public string AltMobileNo { get; set; }
            public string Email { get; set; }
            public string AddressLine { get; set; }
            public string City { get; set; }
            public string StateName { get; set; }
            public string Pincode { get; set; }
            public string AadhaarNo { get; set; }
            public string PANNo { get; set; }
            public string UANNo { get; set; }
            public string PFNo { get; set; }
            public string ESINo { get; set; }
            public string BankAccountNo { get; set; }
            public string BankName { get; set; }
            public string IFSCCode { get; set; }
            public decimal BasicSalary { get; set; }
            public decimal HRA { get; set; }
            public decimal ConveyanceAllow { get; set; }
            public decimal OtherAllow { get; set; }
            public decimal GrossSalary { get; set; }
        }
    }
}
