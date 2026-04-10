using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using FINApp.DAL;
using ClosedXML.Excel;

namespace FINApp
{
    public partial class FINReceiptImport : Page
    {
        protected Label        lblNavUser, lblAlert;
        protected Panel        pnlAlert, pnlResults, pnlNoSavedFiles, pnlUnmapped;
        protected FileUpload   fileUpload;
        protected Button       btnUpload, btnImport;
        protected HiddenField  hfFilePath;
        protected Repeater     rptSavedFiles, rptImportHistory, rptUnmappedCustomers;
        protected Label        lblTotal, lblCustomer, lblBank, lblInternal, lblOther, lblSkipped;

        protected int UserID => Session["FIN_UserID"] != null ? Convert.ToInt32(Session["FIN_UserID"]) : 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["FIN_UserID"] == null) { Response.Redirect("FINLogin.aspx"); return; }
            lblNavUser.Text = Session["FIN_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
            {
                BindSavedFiles();
                BindImportHistory();
            }
        }

        // ── FILE MANAGEMENT ──

        private string GetUploadFolder()
        {
            string folder = Server.MapPath("~/App_Data/ReceiptUploads");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return folder;
        }

        private void BindSavedFiles()
        {
            string folder = GetUploadFolder();
            var files = new DirectoryInfo(folder).GetFiles("*.xlsx")
                .OrderByDescending(f => f.CreationTime).Take(10).ToList();
            if (rptSavedFiles != null) { rptSavedFiles.DataSource = files; rptSavedFiles.DataBind(); }
            if (pnlNoSavedFiles != null) pnlNoSavedFiles.Visible = files.Count == 0;
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            pnlAlert.Visible = false;
            if (fileUpload == null || !fileUpload.HasFile)
            { ShowAlert("Please select a Receipt Register file.", false); return; }

            try
            {
                string folder = GetUploadFolder();
                string savedName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" +
                    Path.GetFileNameWithoutExtension(fileUpload.FileName).Replace(" ", "_") + ".xlsx";
                string savedPath = Path.Combine(folder, savedName);
                fileUpload.SaveAs(savedPath);

                hfFilePath.Value = savedPath;
                PreviewFile(savedPath);
                BindSavedFiles();
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        protected void rptSavedFiles_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "LoadFile") return;
            string fileName = Path.GetFileName(e.CommandArgument.ToString());
            string filePath = Path.Combine(GetUploadFolder(), fileName);
            if (!File.Exists(filePath)) { ShowAlert("File not found.", false); return; }

            try
            {
                hfFilePath.Value = filePath;
                PreviewFile(filePath);
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        // ── PREVIEW ──

        private void PreviewFile(string filePath)
        {
            var rows = ParseReceiptFile(filePath);
            int total = rows.Count, skipped = 0;
            int custCount = 0, bankCount = 0, intCount = 0, otherCount = 0;
            var unmappedCustomers = new List<string>();

            // Auto-match: collect CUSTOMER-type names and try to match against PK_Customers
            var customerNames = new List<string>();
            foreach (var r in rows)
            {
                string rtype = FINDatabaseHelper.ClassifyReceiptType(r.TallyName);
                if (rtype == "CUSTOMER" && !string.IsNullOrEmpty(r.TallyName) && !customerNames.Contains(r.TallyName))
                    customerNames.Add(r.TallyName);
            }
            int autoMatched = FINDatabaseHelper.AutoMatchCustomers(customerNames, null);
            if (autoMatched > 0) FINDatabaseHelper.RepairNullCustomerLinks();

            foreach (var r in rows)
            {
                if (FINDatabaseHelper.ReceiptExists(r.VoucherNo, r.ReceiptDate))
                { skipped++; continue; }

                string rtype = FINDatabaseHelper.ClassifyReceiptType(r.TallyName);
                switch (rtype)
                {
                    case "CUSTOMER": custCount++; break;
                    case "BANK": bankCount++; break;
                    case "INTERNAL": intCount++; break;
                    default: otherCount++; break;
                }

                // Check customer mapping for CUSTOMER type
                if (rtype == "CUSTOMER")
                {
                    var mapping = FINDatabaseHelper.GetCustomerMapping(r.TallyName);
                    if (mapping == null && !unmappedCustomers.Contains(r.TallyName))
                        unmappedCustomers.Add(r.TallyName);
                }
            }

            lblTotal.Text = total.ToString("N0");
            lblCustomer.Text = custCount.ToString("N0");
            lblBank.Text = bankCount.ToString("N0");
            lblInternal.Text = intCount.ToString("N0");
            lblOther.Text = otherCount.ToString("N0");
            lblSkipped.Text = skipped.ToString("N0");

            if (unmappedCustomers.Count > 0)
            {
                var dt = new DataTable();
                dt.Columns.Add("Item", typeof(string));
                foreach (var item in unmappedCustomers.OrderBy(x => x).Take(50))
                    dt.Rows.Add(item);
                rptUnmappedCustomers.DataSource = dt;
                rptUnmappedCustomers.DataBind();
                pnlUnmapped.Visible = true;
            }
            else
            {
                pnlUnmapped.Visible = false;
            }

            pnlResults.Visible = true;
            string fileName = Path.GetFileName(filePath);
            string autoMsg = autoMatched > 0 ? " Auto-matched " + autoMatched + " customer(s)." : "";
            ShowAlert("Preview: " + total + " receipts from " + fileName + " — " +
                custCount + " customer, " + bankCount + " bank, " + intCount + " internal, " +
                otherCount + " other. " + skipped + " already imported. " +
                unmappedCustomers.Count + " unmapped customers." + autoMsg, unmappedCustomers.Count == 0);
        }

        // ── IMPORT ──

        protected void btnImport_Click(object sender, EventArgs e)
        {
            string filePath = hfFilePath.Value;
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            { ShowAlert("No file loaded. Please upload or load a file first.", false); return; }

            try
            {
                var rows = ParseReceiptFile(filePath);
                string fileName = Path.GetFileName(filePath);
                int batchId = FINDatabaseHelper.CreateImportBatch("RECEIPT", fileName, UserID);

                int inserted = 0, skipped = 0, errors = 0;

                foreach (var r in rows)
                {
                    if (FINDatabaseHelper.ReceiptExists(r.VoucherNo, r.ReceiptDate))
                    { skipped++; continue; }

                    try
                    {
                        string rtype = FINDatabaseHelper.ClassifyReceiptType(r.TallyName);
                        int? customerId = null;

                        // Try to map customer
                        if (rtype == "CUSTOMER")
                        {
                            var mapping = FINDatabaseHelper.GetCustomerMapping(r.TallyName);
                            if (mapping != null)
                                customerId = Convert.ToInt32(mapping["CustomerID"]);
                        }

                        FINDatabaseHelper.CreateReceipt(
                            r.VoucherNo, r.ReceiptDate,
                            r.TallyName, rtype, customerId,
                            r.Amount, batchId);

                        inserted++;
                    }
                    catch
                    {
                        errors++;
                    }
                }

                FINDatabaseHelper.UpdateImportBatch(batchId, inserted + skipped + errors, inserted, skipped, errors);
                BindImportHistory();
                ShowAlert("Import complete: " + inserted + " inserted, " +
                    skipped + " skipped, " + errors + " errors.", errors == 0);
            }
            catch (Exception ex)
            {
                ShowAlert("Import failed: " + ex.Message, false);
            }
        }

        // ── PARSING ──

        private class ReceiptRow
        {
            public DateTime ReceiptDate;
            public string TallyName;
            public string VoucherNo;
            public decimal Amount;
        }

        private List<ReceiptRow> ParseReceiptFile(string filePath)
        {
            var result = new List<ReceiptRow>();

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var wb = new XLWorkbook(stream))
            {
                var ws = wb.Worksheet(1);
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;

                // Columns: Date, Particulars, Vch Type, Vch No., Credit
                for (int r = 2; r <= lastRow; r++)
                {
                    string vchNo = ws.Cell(r, 4).GetString().Trim();
                    string name = ws.Cell(r, 2).GetString().Trim();
                    if (string.IsNullOrEmpty(vchNo)) continue;

                    DateTime dt = DateTime.Today;
                    var dv = ws.Cell(r, 1).Value;
                    if (dv is DateTime d) dt = d;
                    else if (dv != null) DateTime.TryParse(dv.ToString(), out dt);

                    decimal amount = 0;
                    var av = ws.Cell(r, 5).Value;
                    if (av is double ad) amount = (decimal)ad;
                    else if (av != null) decimal.TryParse(av.ToString(), out amount);

                    result.Add(new ReceiptRow
                    {
                        ReceiptDate = dt,
                        TallyName = name,
                        VoucherNo = vchNo,
                        Amount = amount
                    });
                }
            }

            return result;
        }

        // ── HISTORY ──

        private void BindImportHistory()
        {
            var dt = FINDatabaseHelper.GetImportBatches("RECEIPT");
            if (rptImportHistory != null) { rptImportHistory.DataSource = dt; rptImportHistory.DataBind(); }
        }

        private void ShowAlert(string msg, bool success)
        {
            pnlAlert.Visible = true;
            lblAlert.Text = msg;
            pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger");
        }
    }
}
