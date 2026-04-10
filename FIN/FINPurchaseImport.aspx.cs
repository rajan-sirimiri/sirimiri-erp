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
    public partial class FINPurchaseImport : Page
    {
        protected Label        lblNavUser, lblAlert;
        protected Panel        pnlAlert, pnlResults, pnlNoSavedFiles, pnlUnmapped;
        protected FileUpload   fileUpload;
        protected Button       btnUpload, btnImport;
        protected HiddenField  hfFilePath;
        protected Repeater     rptSavedFiles, rptImportHistory, rptUnmappedItems;
        protected Label        lblPreviewTotal, lblPreviewInvoices, lblPreviewSkipped, lblPreviewUnmapped;

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

            if (!string.IsNullOrEmpty(hfFilePath.Value))
                pnlResults.Visible = true;
        }

        // ── FILE MANAGEMENT ──

        private string GetUploadFolder()
        {
            string folder = Server.MapPath("~/App_Data/PurchaseUploads");
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
            { ShowAlert("Please select a Purchase Report file.", false); return; }

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
            var invoices = ParsePurchaseFile(filePath);
            int totalRows = 0, alreadyImported = 0;
            var unmappedList = new List<string>();

            foreach (var inv in invoices)
            {
                totalRows += inv.Value.Count;

                if (FINDatabaseHelper.PurchaseInvoiceExists(inv.Key))
                { alreadyImported++; continue; }

                string supName = inv.Value[0].SupplierName;
                if (!string.IsNullOrEmpty(supName) && FINDatabaseHelper.GetSupplierMapping(supName) == null)
                {
                    string key = "Supplier: " + supName;
                    if (!unmappedList.Contains(key)) unmappedList.Add(key);
                }

                foreach (var line in inv.Value)
                {
                    if (!string.IsNullOrEmpty(line.ItemName) && FINDatabaseHelper.GetItemMapping(line.ItemName) == null)
                    {
                        string key = "Item: " + line.ItemName;
                        if (!unmappedList.Contains(key)) unmappedList.Add(key);
                    }
                }
            }

            int newInvoices = invoices.Count - alreadyImported;
            lblPreviewTotal.Text = totalRows.ToString("N0");
            lblPreviewInvoices.Text = newInvoices.ToString("N0");
            lblPreviewSkipped.Text = alreadyImported.ToString("N0");
            lblPreviewUnmapped.Text = unmappedList.Count.ToString();

            if (unmappedList.Count > 0)
            {
                var dt = new DataTable();
                dt.Columns.Add("Item", typeof(string));
                foreach (var item in unmappedList.Take(50)) dt.Rows.Add(item);
                rptUnmappedItems.DataSource = dt;
                rptUnmappedItems.DataBind();
                pnlUnmapped.Visible = true;
            }
            else
            {
                pnlUnmapped.Visible = false;
            }

            pnlResults.Visible = true;
            string fileName = Path.GetFileName(filePath);
            ShowAlert("Preview: " + totalRows + " rows, " + invoices.Count + " invoices (" +
                newInvoices + " new, " + alreadyImported + " already imported). " +
                unmappedList.Count + " unmapped items.", unmappedList.Count == 0);
        }

        // ── IMPORT ──

        protected void btnImport_Click(object sender, EventArgs e)
        {
            string filePath = hfFilePath.Value;
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            { ShowAlert("No file loaded. Please upload or load a file first.", false); return; }

            try
            {
                var invoices = ParsePurchaseFile(filePath);
                string fileName = Path.GetFileName(filePath);
                int batchId = FINDatabaseHelper.CreateImportBatch("PURCHASE", fileName, UserID);

                int inserted = 0, skipped = 0, errors = 0;

                foreach (var inv in invoices)
                {
                    string invNo = inv.Key;
                    var lines = inv.Value;

                    if (FINDatabaseHelper.PurchaseInvoiceExists(invNo))
                    { skipped += lines.Count; continue; }

                    try
                    {
                        string supName = lines[0].SupplierName;
                        var supMap = FINDatabaseHelper.GetSupplierMapping(supName);
                        int? supplierId = supMap != null ? (int?)Convert.ToInt32(supMap["SupplierID"]) : null;

                        decimal totalQty = lines.Sum(l => l.Quantity);
                        decimal totalValue = lines.Sum(l => l.Value);

                        int invoiceId = FINDatabaseHelper.CreatePurchaseInvoice(
                            invNo, lines[0].InvoiceDate,
                            supplierId, supName,
                            totalQty, totalValue, batchId);

                        foreach (var line in lines)
                        {
                            string materialType = null;
                            int? materialId = null;

                            var itemMap = FINDatabaseHelper.GetItemMapping(line.ItemName);
                            if (itemMap != null)
                            {
                                materialType = itemMap["MaterialType"].ToString();
                                if (itemMap["MaterialID"] != DBNull.Value)
                                    materialId = Convert.ToInt32(itemMap["MaterialID"]);
                            }

                            FINDatabaseHelper.AddPurchaseInvoiceLine(
                                invoiceId, materialType, materialId,
                                line.ItemName, line.Quantity, line.Value);

                            inserted++;
                        }
                    }
                    catch
                    {
                        errors += lines.Count;
                    }
                }

                FINDatabaseHelper.UpdateImportBatch(batchId, inserted + skipped + errors, inserted, skipped, errors);
                BindImportHistory();
                ShowAlert("Import complete: " + inserted + " lines inserted, " +
                    skipped + " skipped, " + errors + " errors.", errors == 0);
            }
            catch (Exception ex)
            {
                ShowAlert("Import failed: " + ex.Message, false);
            }
        }

        // ── PARSING ──

        private class PurchaseLine
        {
            public DateTime InvoiceDate;
            public string SupplierName;
            public string ItemName;
            public string InvoiceNo;
            public decimal Quantity;
            public decimal Value;
        }

        private Dictionary<string, List<PurchaseLine>> ParsePurchaseFile(string filePath)
        {
            var result = new Dictionary<string, List<PurchaseLine>>(StringComparer.OrdinalIgnoreCase);

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var wb = new XLWorkbook(stream))
            {
                var ws = wb.Worksheet(1);
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;

                // Columns: Date, Supplier, Particulars, Supplier Invoice No., Quantity, Value
                for (int r = 2; r <= lastRow; r++)
                {
                    string supplier = ws.Cell(r, 2).GetString().Trim();
                    string item = ws.Cell(r, 3).GetString().Trim();
                    string invNo = ws.Cell(r, 4).GetString().Trim();

                    if (string.IsNullOrEmpty(invNo)) continue;

                    DateTime dt = DateTime.Today;
                    var dv = ws.Cell(r, 1).Value;
                    if (dv is DateTime d) dt = d;
                    else if (dv != null) DateTime.TryParse(dv.ToString(), out dt);

                    decimal qty = 0;
                    var qv = ws.Cell(r, 5).Value;
                    if (qv is double qd) qty = (decimal)qd;
                    else if (qv != null) decimal.TryParse(qv.ToString(), out qty);

                    decimal val = 0;
                    var vv = ws.Cell(r, 6).Value;
                    if (vv is double vd) val = (decimal)vd;
                    else if (vv != null) decimal.TryParse(vv.ToString(), out val);

                    var line = new PurchaseLine
                    {
                        InvoiceDate = dt,
                        SupplierName = supplier,
                        ItemName = item,
                        InvoiceNo = invNo,
                        Quantity = qty,
                        Value = val
                    };

                    if (!result.ContainsKey(invNo))
                        result[invNo] = new List<PurchaseLine>();
                    result[invNo].Add(line);
                }
            }

            return result;
        }

        // ── HISTORY ──

        private void BindImportHistory()
        {
            var dt = FINDatabaseHelper.GetImportBatches("PURCHASE");
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
