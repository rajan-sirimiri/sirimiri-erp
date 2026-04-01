using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using OfficeOpenXml;
using StockApp.DAL;

namespace StockApp
{
    public partial class DataImport : Page
    {
        private string UserRole   => Session["Role"]?.ToString();
        private int    UserID     => Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;
        private Label  LblUserInfo => (Label)FindControl("lblUserInfo");

        protected void Page_Load(object sender, EventArgs e)
        {
            // Auth check
            if (Session["UserID"] == null) { Response.Redirect("~/Login.aspx"); return; }

            // Module access check
            string __role = Session["Role"]?.ToString() ?? "";
            if (!DatabaseHelper.RoleHasModuleAccess(__role, "SA", "SA_IMPORT"))
            { Response.Redirect("SAHome.aspx"); return; }
            if (UserRole == "FieldUser")   { Response.Redirect("~/StockEntry.aspx"); return; }

            if (LblUserInfo != null)
                LblUserInfo.Text = "👤 " + Session["FullName"] + " (" + UserRole + ")";

            if (!IsPostBack)
            {
                pnlSalesResult.Visible   = false;
                pnlSalesError.Visible    = false;
                pnlReceiptsResult.Visible = false;
                pnlReceiptsError.Visible  = false;
            }

            // Set EPPlus license
        }

        // ── SALES IMPORT ────────────────────────────────────────────
        protected void btnImportSales_Click(object sender, EventArgs e)
        {
            pnlSalesResult.Visible = false;
            pnlSalesError.Visible  = false;

            if (!fileSales.HasFile)
            { ShowSalesError("Please select an Excel file before importing."); return; }

            try
            {
                // Read column mapping from dropdowns
                var colDate    = GetDropdownValue("salesColDate");
                var colName    = GetDropdownValue("salesColName");
                var colInvoice = GetDropdownValue("salesColInvoice");
                var colQty     = GetDropdownValue("salesColQty");
                var colValue   = GetDropdownValue("salesColValue");

                int inserted = 0, skipped = 0, errors = 0;

                using (var stream = new MemoryStream(fileSales.FileBytes))
                using (var pkg = new ExcelPackage(stream))
                {
                    var ws = pkg.Workbook.Worksheets[0];
                    // Build header map
                    var headers = GetHeaders(ws);

                    // Resolve column indices using mapping or auto-detect
                    int iDate    = ResolveCol(headers, colDate,    "date", "order date");
                    int iName    = ResolveCol(headers, colName,    "customer name", "distributor");
                    int iInvoice = ResolveCol(headers, colInvoice, "voucher no", "invoice", "vch no", "voucher no.");
                    int iProduct = ResolveCol(headers, "",         "product name", "product", "item name", "item");
                    int iQty     = ResolveCol(headers, colQty,     "quantity", "units", "qty");
                    int iValue   = ResolveCol(headers, colValue,   "value", "amount", "total");

                    if (iDate < 0 || iName < 0 || iInvoice < 0)
                    { ShowSalesError("Could not find required columns (Date, Name, Invoice No). Please use column mapping."); return; }

                    int lastRow = ws.Dimension.End.Row;
                    for (int r = 2; r <= lastRow; r++)
                    {
                        try
                        {
                            string invoiceNo  = ws.Cells[r, iInvoice].Text.Trim();
                            if (string.IsNullOrEmpty(invoiceNo)) { skipped++; continue; }

                            string productName = iProduct > 0 ? ws.Cells[r, iProduct].Text.Trim() : "";

                            // Skip if already exists (per invoice + product)
                            if (DatabaseHelper.SalesOrderExists(invoiceNo, productName)) { skipped++; continue; }

                            DateTime orderDate = DateTime.Today;
                            var dateVal = ws.Cells[r, iDate].Value;
                            if (dateVal is DateTime dt) orderDate = dt;
                            else if (dateVal != null) DateTime.TryParse(dateVal.ToString(), out orderDate);

                            string distName = ws.Cells[r, iName].Text.Trim();
                            int    qty      = iQty > 0 ? ParseInt(ws.Cells[r, iQty].Value) : 0;
                            decimal val     = iValue > 0 ? ParseDecimal(ws.Cells[r, iValue].Value) : 0;

                            DatabaseHelper.InsertSalesOrder(orderDate, distName, invoiceNo, productName, qty, val);
                            inserted++;
                        }
                        catch { errors++; }
                    }
                }

                lblSalesInserted.Text = inserted.ToString();
                lblSalesSkipped.Text  = skipped.ToString();
                lblSalesErrors.Text   = errors.ToString();
                pnlSalesResult.Visible = true;

                DatabaseHelper.LogAudit(UserID, "SalesImport_" + inserted + "rows", null, null, Request.UserHostAddress);
            }
            catch (Exception ex)
            { ShowSalesError("Import failed: " + ex.Message); }
        }

        // ── RECEIPTS IMPORT ─────────────────────────────────────────
        protected void btnImportReceipts_Click(object sender, EventArgs e)
        {
            pnlReceiptsResult.Visible = false;
            pnlReceiptsError.Visible  = false;

            if (!fileReceipts.HasFile)
            { ShowReceiptsError("Please select an Excel file before importing."); return; }

            try
            {
                var colDate   = GetDropdownValue("receiptsColDate");
                var colName   = GetDropdownValue("receiptsColName");
                var colVch    = GetDropdownValue("receiptsColVch");
                var colCredit = GetDropdownValue("receiptsColCredit");

                int inserted = 0, skipped = 0, errors = 0;

                using (var stream = new MemoryStream(fileReceipts.FileBytes))
                using (var pkg = new ExcelPackage(stream))
                {
                    var ws = pkg.Workbook.Worksheets[0];
                    var headers = GetHeaders(ws);

                    int iDate   = ResolveCol(headers, colDate,   "date");
                    int iName   = ResolveCol(headers, colName,   "particulars", "customer", "distributor");
                    int iVch    = ResolveCol(headers, colVch,    "vch no", "voucher", "receipt no");
                    int iCredit = ResolveCol(headers, colCredit, "credit", "amount");

                    if (iDate < 0 || iName < 0 || iVch < 0)
                    { ShowReceiptsError("Could not find required columns (Date, Particulars, Vch No). Please use column mapping."); return; }

                    int lastRow = ws.Dimension.End.Row;
                    for (int r = 2; r <= lastRow; r++)
                    {
                        try
                        {
                            string vchNo = ws.Cells[r, iVch].Text.Trim();
                            string name  = ws.Cells[r, iName].Text.Trim();
                            if (string.IsNullOrEmpty(vchNo) || string.IsNullOrEmpty(name)) { skipped++; continue; }

                            // Skip if already exists
                            if (DatabaseHelper.ReceiptExists(vchNo, name)) { skipped++; continue; }

                            DateTime receiptDate = DateTime.Today;
                            var dateVal = ws.Cells[r, iDate].Value;
                            if (dateVal is DateTime dt) receiptDate = dt;
                            else if (dateVal != null) DateTime.TryParse(dateVal.ToString(), out receiptDate);

                            decimal credit = iCredit > 0 ? ParseDecimal(ws.Cells[r, iCredit].Value) : 0;

                            DatabaseHelper.InsertReceipt(receiptDate, name, vchNo, credit);
                            inserted++;
                        }
                        catch { errors++; }
                    }
                }

                lblReceiptsInserted.Text = inserted.ToString();
                lblReceiptsSkipped.Text  = skipped.ToString();
                lblReceiptsErrors.Text   = errors.ToString();
                pnlReceiptsResult.Visible = true;

                DatabaseHelper.LogAudit(UserID, "ReceiptsImport_" + inserted + "rows", null, null, Request.UserHostAddress);
            }
            catch (Exception ex)
            { ShowReceiptsError("Import failed: " + ex.Message); }
        }

        // ── HELPERS ─────────────────────────────────────────────────
        private Dictionary<string, int> GetHeaders(OfficeOpenXml.ExcelWorksheet ws)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int lastCol = ws.Dimension.End.Column;
            for (int c = 1; c <= lastCol; c++)
            {
                string h = ws.Cells[1, c].Text.Trim().ToLower();
                if (!string.IsNullOrEmpty(h) && !map.ContainsKey(h))
                    map[h] = c;
            }
            return map;
        }

        private int ResolveCol(Dictionary<string, int> headers, string selected, params string[] fallbacks)
        {
            // Try user-selected value first
            if (!string.IsNullOrEmpty(selected))
                foreach (var kv in headers)
                    if (kv.Key.Equals(selected, StringComparison.OrdinalIgnoreCase)) return kv.Value;

            // Auto-detect by keyword match
            foreach (var fb in fallbacks)
                foreach (var kv in headers)
                    if (kv.Key.Contains(fb.ToLower())) return kv.Value;

            return -1;
        }

        private string GetDropdownValue(string id)
        {
            var ctl = FindControl(id) as HtmlSelect;
            return ctl?.Value ?? "";
        }

        private int     ParseInt(object v)     { int i; return (v != null && int.TryParse(v.ToString(), out i)) ? i : 0; }
        private decimal ParseDecimal(object v) { decimal d; return (v != null && decimal.TryParse(v.ToString(), out d)) ? d : 0; }

        private void ShowSalesError(string msg)    { lblSalesErrMsg.Text = msg;    pnlSalesError.Visible = true; }
        private void ShowReceiptsError(string msg) { lblReceiptsErrMsg.Text = msg; pnlReceiptsError.Visible = true; }
    }
}
