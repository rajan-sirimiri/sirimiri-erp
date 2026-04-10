using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.UI;
using FINApp.DAL;
using ClosedXML.Excel;

namespace FINApp
{
    public partial class FINSalesImport : Page
    {
        protected System.Web.UI.WebControls.Label        lblNavUser, lblAlert;
        protected System.Web.UI.WebControls.Panel        pnlAlert, pnlResults, pnlNoSavedFiles;
        protected System.Web.UI.WebControls.FileUpload   fileUpload;
        protected System.Web.UI.WebControls.Button       btnUpload, btnImport, btnLoadSaved;
        protected System.Web.UI.WebControls.HiddenField  hfLoadFileName, hfFilePath;
        protected System.Web.UI.WebControls.Repeater     rptSavedFiles, rptImportHistory;
        protected System.Web.UI.WebControls.Label        lblPreviewTotal, lblPreviewInvoices, lblPreviewMapped, lblPreviewUnmapped;
        protected System.Web.UI.WebControls.Repeater     rptUnmappedItems;
        protected System.Web.UI.WebControls.Panel        pnlUnmapped;

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
            string folder = Server.MapPath("~/App_Data/SalesUploads");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            return folder;
        }

        private void BindSavedFiles()
        {
            string folder = GetUploadFolder();
            var files = new DirectoryInfo(folder)
                .GetFiles("*.xlsx")
                .OrderByDescending(f => f.CreationTime)
                .Take(10)
                .ToList();

            if (rptSavedFiles != null)
            {
                rptSavedFiles.DataSource = files;
                rptSavedFiles.DataBind();
            }
            if (pnlNoSavedFiles != null)
                pnlNoSavedFiles.Visible = files.Count == 0;
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            pnlAlert.Visible = false;
            if (fileUpload == null || !fileUpload.HasFile)
            { ShowAlert("Please select a Tally Sales Report Excel file.", false); return; }

            try
            {
                string folder = GetUploadFolder();
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string safeFileName = Path.GetFileNameWithoutExtension(fileUpload.FileName).Replace(" ", "_");
                string savedName = timestamp + "_" + safeFileName + ".xlsx";
                string savedPath = Path.Combine(folder, savedName);
                fileUpload.SaveAs(savedPath);

                hfFilePath.Value = savedPath;
                PreviewFile(savedPath);
                BindSavedFiles();
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message, false);
            }
        }

        protected void btnLoadSaved_Click(object sender, EventArgs e)
        {
            string fileName = Path.GetFileName(hfLoadFileName.Value ?? "");
            if (string.IsNullOrEmpty(fileName)) return;

            string filePath = Path.Combine(GetUploadFolder(), fileName);
            if (!File.Exists(filePath))
            { ShowAlert("File not found: " + fileName, false); return; }

            try
            {
                hfFilePath.Value = filePath;
                PreviewFile(filePath);
            }
            catch (Exception ex)
            {
                ShowAlert("Error loading file: " + ex.Message, false);
            }
        }

        // ── PREVIEW ──

        private void PreviewFile(string filePath)
        {
            var invoices = ParseSalesFile(filePath);
            int totalRows = 0;
            int totalInvoices = invoices.Count;
            int unmappedProducts = 0;
            int unmappedCustomers = 0;
            int alreadyImported = 0;
            var unmappedItems = new List<string>();

            // Auto-match: collect all unique customer names and try to match against PK_Customers
            var allCustomerNames = new List<string>();
            var tallyPincodes = new Dictionary<string, string>();
            foreach (var inv in invoices)
            {
                string custName = inv.Value[0].CustomerName;
                if (!string.IsNullOrEmpty(custName) && !allCustomerNames.Contains(custName))
                {
                    allCustomerNames.Add(custName);
                    // Extract pincode from BuyerAddress if available
                    string addr = inv.Value[0].BuyerAddress ?? "";
                    var pinMatch = System.Text.RegularExpressions.Regex.Match(addr, @"\b(\d{6})\b");
                    if (pinMatch.Success && !tallyPincodes.ContainsKey(custName))
                        tallyPincodes[custName] = pinMatch.Groups[1].Value;
                }
            }
            int autoMatched = FINDatabaseHelper.AutoMatchCustomers(allCustomerNames, tallyPincodes);
            if (autoMatched > 0) FINDatabaseHelper.RepairNullCustomerLinks();

            foreach (var inv in invoices)
            {
                totalRows += inv.Value.Count;

                // Check if voucher already imported
                if (FINDatabaseHelper.SalesInvoiceExists(inv.Key))
                { alreadyImported++; continue; }

                // Check customer mapping
                string custName = inv.Value[0].CustomerName;
                if (FINDatabaseHelper.GetCustomerMapping(custName) == null)
                {
                    if (!unmappedItems.Contains("Customer: " + custName))
                        unmappedItems.Add("Customer: " + custName);
                    unmappedCustomers++;
                }

                // Check product mappings
                foreach (var line in inv.Value)
                {
                    if (FINDatabaseHelper.GetProductMapping(line.ProductName) == null &&
                        FINDatabaseHelper.GetScrapMapping(line.ProductName) == null)
                    {
                        string key = "Product: " + line.ProductName;
                        if (!unmappedItems.Contains(key))
                            unmappedItems.Add(key);
                        unmappedProducts++;
                    }
                }
            }

            int newInvoices = totalInvoices - alreadyImported;
            lblPreviewTotal.Text = totalRows.ToString("N0");
            lblPreviewInvoices.Text = newInvoices.ToString("N0");
            lblPreviewMapped.Text = alreadyImported.ToString("N0");

            if (unmappedItems.Count > 0)
            {
                lblPreviewUnmapped.Text = unmappedItems.Count.ToString();
                var dt = new DataTable();
                dt.Columns.Add("Item", typeof(string));
                foreach (var item in unmappedItems.Take(50))
                    dt.Rows.Add(item);
                rptUnmappedItems.DataSource = dt;
                rptUnmappedItems.DataBind();
                pnlUnmapped.Visible = true;
            }
            else
            {
                lblPreviewUnmapped.Text = "0";
                pnlUnmapped.Visible = false;
            }

            pnlResults.Visible = true;

            string fileName = Path.GetFileName(filePath);
            string autoMsg = autoMatched > 0 ? " Auto-matched " + autoMatched + " customer(s)." : "";
            ShowAlert("Preview: " + totalRows + " rows, " + totalInvoices + " invoices (" +
                newInvoices + " new, " + alreadyImported + " already imported). " +
                unmappedItems.Count + " unmapped items." + autoMsg, unmappedItems.Count == 0);
        }

        // ── IMPORT ──

        protected void btnImport_Click(object sender, EventArgs e)
        {
            string filePath = hfFilePath.Value;
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            { ShowAlert("No file loaded. Please upload or load a file first.", false); return; }

            try
            {
                var invoices = ParseSalesFile(filePath);
                string fileName = Path.GetFileName(filePath);
                int batchId = FINDatabaseHelper.CreateImportBatch("SALES", fileName, UserID);

                int inserted = 0, skipped = 0, errors = 0;

                foreach (var inv in invoices)
                {
                    string voucherNo = inv.Key;
                    var lines = inv.Value;

                    // Skip already imported
                    if (FINDatabaseHelper.SalesInvoiceExists(voucherNo))
                    { skipped += lines.Count; continue; }

                    try
                    {
                        // Get customer mapping
                        string custName = lines[0].CustomerName;
                        string buyerAddr = lines[0].BuyerAddress;
                        var custMap = FINDatabaseHelper.GetCustomerMapping(custName);
                        int? customerId = custMap != null ? (int?)Convert.ToInt32(custMap["CustomerID"]) : null;

                        // Calculate totals
                        decimal totalQty = lines.Sum(l => l.Quantity);
                        decimal totalValue = lines.Sum(l => l.Value);

                        // Create invoice header
                        int invoiceId = FINDatabaseHelper.CreateSalesInvoice(
                            voucherNo, lines[0].InvoiceDate,
                            customerId, custName, buyerAddr,
                            totalQty, totalValue, batchId);

                        // Create line items
                        foreach (var line in lines)
                        {
                            int? productId = null;
                            int? scrapId = null;
                            string sellingForm = "PCS";
                            int piecesPerUnit = 1;
                            string lineType = "PRODUCT";

                            // Try product mapping first
                            var prodMap = FINDatabaseHelper.GetProductMapping(line.ProductName);
                            if (prodMap != null)
                            {
                                productId = Convert.ToInt32(prodMap["ProductID"]);
                                sellingForm = prodMap["SellingForm"].ToString();
                                piecesPerUnit = Convert.ToInt32(prodMap["PiecesPerUnit"]);
                            }
                            else
                            {
                                // Try scrap mapping
                                var scrapMap = FINDatabaseHelper.GetScrapMapping(line.ProductName);
                                if (scrapMap != null)
                                {
                                    scrapId = Convert.ToInt32(scrapMap["ScrapID"]);
                                    lineType = "SCRAP";
                                }
                            }

                            FINDatabaseHelper.AddSalesInvoiceLine(
                                invoiceId, productId, scrapId,
                                line.ProductName, sellingForm, piecesPerUnit,
                                line.Quantity, line.Value, lineType);

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
                    skipped + " skipped (already imported), " + errors + " errors.", errors == 0);
            }
            catch (Exception ex)
            {
                ShowAlert("Import failed: " + ex.Message, false);
            }
        }

        // ── PARSING ──

        private class SalesLine
        {
            public DateTime InvoiceDate;
            public string CustomerName;
            public string ProductName;
            public string BuyerAddress;
            public string VoucherNo;
            public decimal Quantity;
            public decimal Value;
        }

        /// Parse Sales_Report.xlsx — returns Dictionary of VoucherNo → list of lines
        private Dictionary<string, List<SalesLine>> ParseSalesFile(string filePath)
        {
            var result = new Dictionary<string, List<SalesLine>>(StringComparer.OrdinalIgnoreCase);

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var wb = new XLWorkbook(stream))
            {
                var ws = wb.Worksheet(1);
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;

                // Find columns by header
                int colDate = 0, colCustomer = 0, colProduct = 0, colAddress = 0, colVoucher = 0, colQty = 0, colValue = 0;
                int lastCol = ws.LastColumnUsed()?.ColumnNumber() ?? 0;
                for (int c = 1; c <= lastCol; c++)
                {
                    string h = ws.Cell(1, c).GetString().Trim().ToLower();
                    if (h.Contains("date") && colDate == 0) colDate = c;
                    if (h.Contains("customer")) colCustomer = c;
                    if (h.Contains("product")) colProduct = c;
                    if (h.Contains("address") || h.Contains("buyer")) colAddress = c;
                    if (h.Contains("voucher")) colVoucher = c;
                    if (h.Contains("quantity") || h == "qty") colQty = c;
                    if (h.Contains("value") || h.Contains("amount")) colValue = c;
                }

                if (colVoucher == 0 || colCustomer == 0 || colProduct == 0)
                    throw new Exception("Could not find required columns (Voucher No, Customer Name, Product Name) in the header row.");

                for (int r = 2; r <= lastRow; r++)
                {
                    string voucherNo = ws.Cell(r, colVoucher).GetString().Trim();
                    if (string.IsNullOrEmpty(voucherNo)) continue;

                    DateTime dt = DateTime.Today;
                    if (colDate > 0)
                    {
                        var dv = ws.Cell(r, colDate).Value;
                        if (dv is DateTime d) dt = d;
                        else if (dv != null) DateTime.TryParse(dv.ToString(), out dt);
                    }

                    string custName = colCustomer > 0 ? ws.Cell(r, colCustomer).GetString().Trim() : "";
                    string prodName = ws.Cell(r, colProduct).GetString().Trim();
                    string address = colAddress > 0 ? ws.Cell(r, colAddress).GetString().Trim() : "";

                    decimal qty = 0;
                    if (colQty > 0)
                    {
                        var qv = ws.Cell(r, colQty).Value;
                        if (qv is double qd) qty = (decimal)qd;
                        else if (qv != null) decimal.TryParse(qv.ToString(), out qty);
                    }

                    decimal val = 0;
                    if (colValue > 0)
                    {
                        var vv = ws.Cell(r, colValue).Value;
                        if (vv is double vd) val = (decimal)vd;
                        else if (vv != null) decimal.TryParse(vv.ToString(), out val);
                    }

                    var line = new SalesLine
                    {
                        InvoiceDate = dt,
                        CustomerName = custName,
                        ProductName = prodName,
                        BuyerAddress = address,
                        VoucherNo = voucherNo,
                        Quantity = qty,
                        Value = val
                    };

                    if (!result.ContainsKey(voucherNo))
                        result[voucherNo] = new List<SalesLine>();
                    result[voucherNo].Add(line);
                }
            }

            return result;
        }

        // ── IMPORT HISTORY ──

        private void BindImportHistory()
        {
            var dt = FINDatabaseHelper.GetImportBatches("SALES");
            if (rptImportHistory != null)
            {
                rptImportHistory.DataSource = dt;
                rptImportHistory.DataBind();
            }
        }

        // ── HELPERS ──

        private void ShowAlert(string msg, bool success)
        {
            pnlAlert.Visible = true;
            lblAlert.Text = msg;
            pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger");
        }
    }
}
