using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using OfficeOpenXml;
using StockApp.DAL;

namespace DataImport
{
    public partial class Import : Page
    {
        private string UserRole => Session["Role"]?.ToString();
        private int    UserID   => Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;

        // Control references — names match Import.aspx exactly
        private FileUpload FileSales    => (FileUpload)FindControl("fileSales");
        private FileUpload FileReceipts => (FileUpload)FindControl("fileReceipts");

        // Sales result controls
        private Panel  PnlSalesOK      => (Panel)FindControl("pnlSalesOK");
        private Panel  PnlSalesFail    => (Panel)FindControl("pnlSalesFail");
        private Label  LblSalesNew     => (Label)FindControl("lblSalesNew");
        private Label  LblSalesSkip    => (Label)FindControl("lblSalesSkip");
        private Label  LblSalesErr     => (Label)FindControl("lblSalesErr");
        private Label  LblSalesFailMsg => (Label)FindControl("lblSalesFailMsg");

        // Receipts result controls
        private Panel  PnlReceiptsOK      => (Panel)FindControl("pnlReceiptsOK");
        private Panel  PnlReceiptsFail    => (Panel)FindControl("pnlReceiptsFail");
        private Label  LblReceiptsNew     => (Label)FindControl("lblReceiptsNew");
        private Label  LblReceiptsSkip    => (Label)FindControl("lblReceiptsSkip");
        private Label  LblReceiptsErr     => (Label)FindControl("lblReceiptsErr");
        private Label  LblReceiptsFailMsg => (Label)FindControl("lblReceiptsFailMsg");

        // Column mapping selects
        private HtmlSelect SelSalesDate    => (HtmlSelect)FindControl("salesColDate");
        private HtmlSelect SelSalesName    => (HtmlSelect)FindControl("salesColName");
        private HtmlSelect SelSalesInvoice => (HtmlSelect)FindControl("salesColInvoice");
        private HtmlSelect SelSalesQty     => (HtmlSelect)FindControl("salesColQty");
        private HtmlSelect SelSalesValue   => (HtmlSelect)FindControl("salesColValue");

        private HtmlSelect SelReceiptsDate   => (HtmlSelect)FindControl("receiptsColDate");
        private HtmlSelect SelReceiptsName   => (HtmlSelect)FindControl("receiptsColName");
        private HtmlSelect SelReceiptsVch    => (HtmlSelect)FindControl("receiptsColVch");
        private HtmlSelect SelReceiptsCredit => (HtmlSelect)FindControl("receiptsColCredit");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Login.aspx"); return; }
            if (UserRole == "FieldUser")   { Response.Redirect("~/Login.aspx"); return; }

            var lbl = (Label)FindControl("lblUserInfo");
            if (lbl != null)
                lbl.Text = "👤 " + Session["FullName"] + " (" + UserRole + ")";


            if (!IsPostBack)
            {
                PnlSalesOK.Visible      = false;
                PnlSalesFail.Visible    = false;
                PnlReceiptsOK.Visible   = false;
                PnlReceiptsFail.Visible = false;
            }
        }

        // ── SALES IMPORT ─────────────────────────────────────────────
        protected void btnImportSales_Click(object sender, EventArgs e)
        {
            PnlSalesOK.Visible   = false;
            PnlSalesFail.Visible = false;

            if (FileSales == null || !FileSales.HasFile)
            { ShowSalesError("Please select an Excel file first."); return; }

            try
            {
                int inserted = 0, skipped = 0, errors = 0;

                using (var stream = new MemoryStream(FileSales.FileBytes))
                using (var pkg   = new ExcelPackage(stream))
                {
                    var ws      = pkg.Workbook.Worksheets[1];
                    var headers = GetHeaders(ws);

                    int iDate    = ResolveCol(headers, SelSalesDate?.Value,    "date", "order date");
                    int iName    = ResolveCol(headers, SelSalesName?.Value,    "customer name", "distributor", "name");
                    int iInvoice = ResolveCol(headers, SelSalesInvoice?.Value, "voucher no", "invoice no", "vch no", "voucher", "voucher no.");
                    int iProduct = ResolveCol(headers, "",                     "product name", "product", "item name", "item");
                    int iQty     = ResolveCol(headers, SelSalesQty?.Value,     "quantity", "qty", "units");
                    int iValue   = ResolveCol(headers, SelSalesValue?.Value,   "value", "amount", "total");

                    if (iDate < 0 || iName < 0 || iInvoice < 0)
                    { ShowSalesError("Could not detect required columns (Date, Customer Name, Voucher No). " +
                        "Check that row 1 of your file contains column headers."); return; }

                    int lastRow = ws.Dimension.End.Row;
                    for (int r = 2; r <= lastRow; r++)
                    {
                        try
                        {
                            string invoiceNo = ws.Cells[r, iInvoice].Text.Trim();
                            if (string.IsNullOrWhiteSpace(invoiceNo)) { skipped++; continue; }

                            string productName = iProduct > 0 ? ws.Cells[r, iProduct].Text.Trim() : "";
                            if (DatabaseHelper.SalesOrderExists(invoiceNo, productName)) { skipped++; continue; }

                            DateTime orderDate = DateTime.Today;
                            var dv = ws.Cells[r, iDate].Value;
                            if (dv is DateTime dt) orderDate = dt;
                            else if (dv != null) DateTime.TryParse(dv.ToString(), out orderDate);

                            string name = ws.Cells[r, iName].Text.Trim();
                            if (string.IsNullOrWhiteSpace(name)) { skipped++; continue; }

                            int     qty = iQty   > 0 ? ToInt(ws.Cells[r, iQty].Value)      : 0;
                            decimal val = iValue > 0 ? ToDecimal(ws.Cells[r, iValue].Value) : 0;

                            DatabaseHelper.InsertSalesOrder(orderDate, name, invoiceNo, productName, qty, val);
                            inserted++;
                        }
                        catch { errors++; }
                    }
                }

                LblSalesNew.Text   = inserted.ToString();
                LblSalesSkip.Text  = skipped.ToString();
                LblSalesErr.Text   = errors.ToString();
                PnlSalesOK.Visible = true;

                DatabaseHelper.LogAudit(UserID, "SalesImport_" + inserted + "_rows",
                    null, null, Request.UserHostAddress);
            }
            catch (Exception ex)
            { ShowSalesError("Import failed: " + ex.Message); }
        }

        // ── RECEIPTS IMPORT ──────────────────────────────────────────
        protected void btnImportReceipts_Click(object sender, EventArgs e)
        {
            PnlReceiptsOK.Visible   = false;
            PnlReceiptsFail.Visible = false;

            if (FileReceipts == null || !FileReceipts.HasFile)
            { ShowReceiptsError("Please select an Excel file first."); return; }

            try
            {
                int inserted = 0, skipped = 0, errors = 0;

                using (var stream = new MemoryStream(FileReceipts.FileBytes))
                using (var pkg   = new ExcelPackage(stream))
                {
                    var ws      = pkg.Workbook.Worksheets[1];
                    var headers = GetHeaders(ws);

                    int iDate   = ResolveCol(headers, SelReceiptsDate?.Value,   "date", "receipt date");
                    int iName   = ResolveCol(headers, SelReceiptsName?.Value,   "particulars", "customer", "distributor", "name");
                    int iVch    = ResolveCol(headers, SelReceiptsVch?.Value,    "vch no", "voucher no", "receipt no", "vch");
                    int iCredit = ResolveCol(headers, SelReceiptsCredit?.Value, "credit", "amount", "value");

                    if (iDate < 0 || iName < 0 || iVch < 0)
                    { ShowReceiptsError("Could not detect required columns (Date, Particulars, Vch No). " +
                        "Check that row 1 of your file contains column headers."); return; }

                    int lastRow = ws.Dimension.End.Row;
                    for (int r = 2; r <= lastRow; r++)
                    {
                        try
                        {
                            string vchNo = ws.Cells[r, iVch].Text.Trim();
                            string name  = ws.Cells[r, iName].Text.Trim();
                            if (string.IsNullOrWhiteSpace(vchNo) || string.IsNullOrWhiteSpace(name))
                            { skipped++; continue; }

                            if (DatabaseHelper.ReceiptExists(vchNo, name)) { skipped++; continue; }

                            DateTime receiptDate = DateTime.Today;
                            var dv = ws.Cells[r, iDate].Value;
                            if (dv is DateTime dt) receiptDate = dt;
                            else if (dv != null) DateTime.TryParse(dv.ToString(), out receiptDate);

                            decimal credit = iCredit > 0 ? ToDecimal(ws.Cells[r, iCredit].Value) : 0;

                            DatabaseHelper.InsertReceipt(receiptDate, name, vchNo, credit);
                            inserted++;
                        }
                        catch { errors++; }
                    }
                }

                LblReceiptsNew.Text   = inserted.ToString();
                LblReceiptsSkip.Text  = skipped.ToString();
                LblReceiptsErr.Text   = errors.ToString();
                PnlReceiptsOK.Visible = true;

                DatabaseHelper.LogAudit(UserID, "ReceiptsImport_" + inserted + "_rows",
                    null, null, Request.UserHostAddress);
            }
            catch (Exception ex)
            { ShowReceiptsError("Import failed: " + ex.Message); }
        }

        // ── HELPERS ──────────────────────────────────────────────────
        private Dictionary<string, int> GetHeaders(ExcelWorksheet ws)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int cols = ws.Dimension.End.Column;
            for (int c = 1; c <= cols; c++)
            {
                string h = ws.Cells[1, c].Text.Trim().ToLower();
                if (!string.IsNullOrEmpty(h) && !map.ContainsKey(h))
                    map[h] = c;
            }
            return map;
        }

        private int ResolveCol(Dictionary<string, int> headers, string selected,
                               params string[] fallbacks)
        {
            if (!string.IsNullOrEmpty(selected))
                foreach (var kv in headers)
                    if (kv.Key.Equals(selected, StringComparison.OrdinalIgnoreCase))
                        return kv.Value;

            foreach (var fb in fallbacks)
                foreach (var kv in headers)
                    if (kv.Key.ToLower().Contains(fb.ToLower()))
                        return kv.Value;

            return -1;
        }

        private int     ToInt(object v)     { int i;     return v != null && int.TryParse(v.ToString(),     out i) ? i : 0; }
        private decimal ToDecimal(object v) { decimal d; return v != null && decimal.TryParse(v.ToString(), out d) ? d : 0; }

        private void ShowSalesError(string msg)    { LblSalesFailMsg.Text    = msg; PnlSalesFail.Visible    = true; }
        private void ShowReceiptsError(string msg) { LblReceiptsFailMsg.Text = msg; PnlReceiptsFail.Visible = true; }
    }
}
