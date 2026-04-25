using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClosedXML.Excel;

namespace HRModule
{
    /// <summary>
    /// Employee Excel bulk import.
    /// Follows Session 9/10 pattern:
    ///   - pnlResults.Visible = true restored on every postback via hfFilePath check
    ///   - Server-side parse, show preview grid, confirm via browser confirm()
    ///   - Auto-create missing department (mirrors AutoMatchCustomers pattern)
    /// </summary>
    public partial class HREmployeeImport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // --- Role gate ---
            string role = Session["UserRole"] as string;
            if (role != "Super" && role != "Admin")
            {
                Response.Redirect("/Login.aspx", true);
                return;
            }

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

                BindPreview(rows);
                pnlResults.Visible = true;
            }
            catch (Exception ex)
            {
                ShowMsg("Upload failed: " + ex.Message, "err");
            }
        }

        // =================================================================
        // Confirm import
        // =================================================================
        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            List<ImportRow> rows = ViewState["ImportRows"] as List<ImportRow>;
            if (rows == null) { ShowMsg("Nothing to import. Please upload again.", "err"); return; }

            string user = (Session["UserName"] as string) ?? "SYSTEM";
            int inserted = 0, failed = 0;
            List<string> errors = new List<string>();

            foreach (ImportRow r in rows.Where(x => x.Status == "READY"))
            {
                try
                {
                    // Auto-generate code if blank
                    if (string.IsNullOrWhiteSpace(r.EmployeeCode))
                        r.EmployeeCode = HR_DatabaseHelper.GenerateEmployeeCode();

                    // Resolve / create department
                    int deptId = HR_DatabaseHelper.GetOrCreateDepartment(r.Department, user);
                    if (deptId <= 0) { failed++; errors.Add("Row " + r.RowNum + ": department resolution failed"); continue; }

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

                    HR_DatabaseHelper.InsertEmployee(emp, user);
                    inserted++;
                }
                catch (Exception ex)
                {
                    failed++;
                    errors.Add("Row " + r.RowNum + ": " + ex.Message);
                }
            }

            // Clean up temp file and state
            try { if (File.Exists(hfFilePath.Value)) File.Delete(hfFilePath.Value); } catch { }
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
            if (status == "ERROR") e.Row.CssClass = "row-err";
            else if (status == "WARN") e.Row.CssClass = "row-warn";
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
                IXLRow hdr = used.FirstRow().AsRow();
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
                    if (string.IsNullOrWhiteSpace(ir.FullName) && string.IsNullOrWhiteSpace(ir.EmployeeCode))
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

                if (!string.IsNullOrWhiteSpace(r.EmployeeCode))
                {
                    if (existingCodes.Contains(r.EmployeeCode)) errs.Add("Code already exists in DB");
                    else if (!seenInFile.Add(r.EmployeeCode)) errs.Add("Duplicate code in file");
                }
                // blank code OK -> auto-generate at import time

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
                dr["EmployeeCode"] = r.EmployeeCode ?? "(auto)";
                dr["FullName"] = r.FullName;
                dr["Department"] = r.Department;
                dr["Designation"] = r.Designation;
                dr["EmploymentType"] = r.EmploymentType;
                dr["DOJ"] = (object)r.DOJ ?? DBNull.Value;
                dr["MobileNo"] = r.MobileNo;
                dr["AadhaarNo"] = r.AadhaarNo;
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
            pnlMsg.CssClass = "msg msg-" + kind;
            pnlMsg.Controls.Clear();
            pnlMsg.Controls.Add(new LiteralControl(Server.HtmlEncode(text)));
        }

        // =================================================================
        // Excel cell helpers (tolerate header aliases)
        // =================================================================
        private static readonly Dictionary<string, string[]> Aliases = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "EmployeeCode", new[]{ "EmployeeCode", "EmpCode", "Code" } },
            { "FullName",     new[]{ "FullName", "Name", "Employee Name" } },
            { "FatherName",   new[]{ "FatherName", "Father's Name", "Father" } },
            { "DOB",          new[]{ "DOB", "Date of Birth", "Birth Date" } },
            { "DOJ",          new[]{ "DOJ", "Date of Joining", "Joining Date" } },
            { "Department",   new[]{ "Department", "Dept" } },
            { "EmploymentType", new[]{ "EmploymentType", "Type", "Employment Type" } },
            { "Mobile",       new[]{ "Mobile", "Phone", "MobileNo" } },
            { "AltMobile",    new[]{ "AltMobile", "Alt Mobile", "Alternate Mobile" } },
            { "Pincode",      new[]{ "Pincode", "Pin", "Zip" } },
            { "Aadhaar",      new[]{ "Aadhaar", "Aadhar", "AadhaarNo", "UID" } },
            { "PAN",          new[]{ "PAN", "PANNo" } },
            { "BankAcNo",     new[]{ "BankAcNo", "Bank A/c", "Account Number" } },
            { "Basic",        new[]{ "Basic", "Basic Salary" } },
        };

        private static string GetStr(IXLWorksheet ws, int row, Dictionary<string, int> col, string logical)
        {
            int c = ResolveCol(col, logical);
            if (c == 0) return "";
            string s = ws.Cell(row, c).GetString();
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
            DateTime d;
            // Try common Indian formats first
            string[] fmts = { "dd-MM-yyyy", "dd/MM/yyyy", "d-M-yyyy", "d/M/yyyy",
                              "yyyy-MM-dd", "dd-MMM-yyyy", "dd-MMM-yy" };
            if (DateTime.TryParseExact(s, fmts, System.Globalization.CultureInfo.InvariantCulture,
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
            return a.Replace(" ", "").Replace("-", "").Trim();
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
