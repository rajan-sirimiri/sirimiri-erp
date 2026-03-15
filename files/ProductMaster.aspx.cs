using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Text;
using OfficeOpenXml;
using StockApp.DAL;

namespace StockApp
{
    public partial class ProductMaster : Page
    {
        private string UserRole => Session["Role"]?.ToString();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Login.aspx"); return; }
            if (UserRole != "Admin")       { Response.Redirect("~/StockEntry.aspx"); return; }

            var lbl = (Label)FindControl("lblUserInfo");
            if (lbl != null) lbl.Text = Session["FullName"] + " (" + UserRole + ")";

            if (!IsPostBack) BindProducts();
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            pnlMsg.Visible = false;
            pnlErr.Visible = false;

            string name = txtProductName.Text.Trim();
            string code = txtProductCode.Text.Trim();
            string hsn  = txtHSN.Text.Trim();

            if (string.IsNullOrEmpty(name))
            { lblErr.Text = "Product name is required."; pnlErr.Visible = true; return; }

            decimal mrp = 0;
            if (!string.IsNullOrEmpty(txtMRP.Text) && !decimal.TryParse(txtMRP.Text, out mrp))
            { lblErr.Text = "MRP must be a valid number (e.g. 250.00)."; pnlErr.Visible = true; return; }

            decimal gst = Convert.ToDecimal(ddlGST.SelectedValue);

            try
            {
                DatabaseHelper.AddProduct(name, code, mrp, hsn, gst);
                txtProductName.Text = "";
                txtProductCode.Text = "";
                txtMRP.Text         = "";
                txtHSN.Text         = "";
                ddlGST.SelectedValue = "18";
                lblMsg.Text = "Product \"" + name + "\" added successfully.";
                pnlMsg.Visible = true;
                BindProducts();
            }
            catch (Exception ex)
            {
                lblErr.Text = ex.Message.Contains("Duplicate") || ex.Message.Contains("uq_")
                    ? "A product with this name already exists."
                    : "Error: " + ex.Message;
                pnlErr.Visible = true;
            }
        }

        protected void gvProducts_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ToggleActive")
            {
                string[] parts = e.CommandArgument.ToString().Split('|');
                DatabaseHelper.ToggleProductActive(Convert.ToInt32(parts[0]), !Convert.ToBoolean(parts[1]));
                pnlMsg.Visible = false;
                pnlErr.Visible = false;
                BindProducts();
            }
            else if (e.CommandName == "EditProduct")
            {
                int productId = Convert.ToInt32(e.CommandArgument);
                DataRow row   = DatabaseHelper.GetProductById(productId);
                if (row == null) return;

                hfEditProductId.Value      = productId.ToString();
                txtEditProductName.Text    = row["ProductName"].ToString();
                txtEditProductCode.Text    = row["ProductCode"] != DBNull.Value ? row["ProductCode"].ToString() : "";
                txtEditMRP.Text            = row["MRP"] != DBNull.Value ? Convert.ToDecimal(row["MRP"]).ToString("0.##") : "";
                txtEditHSN.Text            = row["HSNCode"] != DBNull.Value ? row["HSNCode"].ToString() : "";
                ddlEditGST.SelectedValue   = Convert.ToDecimal(row["GSTRate"]).ToString("0.##");

                pnlEditProduct.Visible = true;
                pnlMsg.Visible = false;
                pnlErr.Visible = false;
            }
        }

        protected void btnEditSave_Click(object sender, EventArgs e)
        {
            string name = txtEditProductName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            { lblErr.Text = "Product name is required."; pnlErr.Visible = true; return; }

            decimal mrp = 0;
            if (!string.IsNullOrEmpty(txtEditMRP.Text) && !decimal.TryParse(txtEditMRP.Text, out mrp))
            { lblErr.Text = "MRP must be a valid number."; pnlErr.Visible = true; return; }

            DatabaseHelper.UpdateProduct(
                Convert.ToInt32(hfEditProductId.Value),
                name,
                txtEditProductCode.Text.Trim(),
                mrp,
                txtEditHSN.Text.Trim(),
                Convert.ToDecimal(ddlEditGST.SelectedValue));

            pnlEditProduct.Visible = false;
            lblMsg.Text = $"Product '{name}' updated successfully.";
            pnlMsg.Visible = true;
            BindProducts();
        }

        protected void btnEditCancel_Click(object sender, EventArgs e)
        {
            pnlEditProduct.Visible = false;
        }

        protected void btnDownloadTemplate_Click(object sender, EventArgs e)
        {
            using (var pkg = new ExcelPackage())
            {
                var ws = pkg.Workbook.Worksheets.Add("Products");
                // Headers
                ws.Cells[1, 1].Value = "ProductName";
                ws.Cells[1, 2].Value = "ProductCode";
                ws.Cells[1, 3].Value = "MRP";
                ws.Cells[1, 4].Value = "HSNCode";
                ws.Cells[1, 5].Value = "GSTRate";
                // Style headers
                using (var hdr = ws.Cells[1, 1, 1, 5])
                {
                    hdr.Style.Font.Bold = true;
                    hdr.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    hdr.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(41, 121, 201));
                    hdr.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }
                // Sample rows
                ws.Cells[2, 1].Value = "Sirimiri Classic 500g";
                ws.Cells[2, 2].Value = "SRM-001";
                ws.Cells[2, 3].Value = 250.00;
                ws.Cells[2, 4].Value = "21069099";
                ws.Cells[2, 5].Value = 18;
                ws.Cells[3, 1].Value = "Sirimiri Pro 1kg";
                ws.Cells[3, 2].Value = "SRM-002";
                ws.Cells[3, 3].Value = 450.00;
                ws.Cells[3, 4].Value = "21069099";
                ws.Cells[3, 5].Value = 12;
                ws.Cells.AutoFitColumns();
                // Notes
                ws.Cells[5, 1].Value = "Notes:";
                ws.Cells[5, 1].Style.Font.Bold = true;
                ws.Cells[6, 1].Value = "ProductName is required";
                ws.Cells[7, 1].Value = "GSTRate must be one of: 0, 5, 12, 18, 28";
                ws.Cells[8, 1].Value = "MRP should be a decimal number e.g. 250.00";
                ws.Cells[9, 1].Value = "ProductCode and HSNCode are optional";

                byte[] fileBytes = pkg.GetAsByteArray();
                Response.Clear();
                Response.Buffer = true;
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=ProductUploadTemplate.xlsx");
                Response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                Response.Flush();
                Response.SuppressContent = true;
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
        }

        protected void btnBulkUpload_Click(object sender, EventArgs e)
        {
            pnlMsg.Visible = false;
            pnlErr.Visible = false;
            pnlBulkResult.Visible = false;

            if (!fileProducts.HasFile)
            { lblErr.Text = "Please select an Excel file."; pnlErr.Visible = true; return; }

            if (!fileProducts.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            { lblErr.Text = "Only .xlsx files are supported."; pnlErr.Visible = true; return; }

            int added = 0, skipped = 0, errors = 0;
            var detail = new StringBuilder();
            var validGst = new System.Collections.Generic.HashSet<decimal> { 0, 5, 12, 18, 28 };

            try
            {
                using (var stream = new MemoryStream(fileProducts.FileBytes))
                using (var pkg = new ExcelPackage(stream))
                {
                    var ws   = pkg.Workbook.Worksheets[1];
                    int rows = ws.Dimension?.Rows ?? 0;

                    for (int r = 2; r <= rows; r++)
                    {
                        string name = (ws.Cells[r, 1].Value ?? "").ToString().Trim();
                        string code = (ws.Cells[r, 2].Value ?? "").ToString().Trim();
                        string mrpStr = (ws.Cells[r, 3].Value ?? "").ToString().Trim();
                        string hsn  = (ws.Cells[r, 4].Value ?? "").ToString().Trim();
                        string gstStr = (ws.Cells[r, 5].Value ?? "").ToString().Trim();

                        if (string.IsNullOrEmpty(name)) continue;

                        // MRP
                        decimal mrp = 0;
                        if (!string.IsNullOrEmpty(mrpStr) && !decimal.TryParse(mrpStr, out mrp))
                        { detail.AppendLine($"Row {r} ({name}): Invalid MRP '{mrpStr}' — skipped."); errors++; continue; }

                        // GST
                        decimal gst = 18;
                        if (!string.IsNullOrEmpty(gstStr))
                        {
                            if (!decimal.TryParse(gstStr, out gst) || !validGst.Contains(gst))
                            { detail.AppendLine($"Row {r} ({name}): Invalid GSTRate '{gstStr}' (must be 0,5,12,18,28) — skipped."); errors++; continue; }
                        }

                        try
                        {
                            DatabaseHelper.AddProduct(name, code, mrp, hsn, gst);
                            added++;
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("Duplicate") || ex.Message.Contains("uq_"))
                            { detail.AppendLine($"Row {r} ({name}): Already exists — skipped."); skipped++; }
                            else
                            { detail.AppendLine($"Row {r} ({name}): Error — {ex.Message}"); errors++; }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblErr.Text = "Error reading file: " + ex.Message;
                pnlErr.Visible = true;
                return;
            }

            lblAdded.Text      = added.ToString();
            lblBulkSkipped.Text = skipped.ToString();
            lblBulkErrors.Text = errors.ToString();
            lblBulkDetail.Text = detail.ToString();
            pnlBulkResult.Visible = true;

            if (added > 0)
            {
                lblMsg.Text = $"{added} product(s) added successfully.";
                pnlMsg.Visible = true;
                BindProducts();
            }
            else
            {
                lblErr.Text = "No new products were added.";
                pnlErr.Visible = true;
            }
        }

        private void BindProducts()
        {
            DataTable dt = DatabaseHelper.GetAllProducts();
            if (dt.Rows.Count == 0)
            { gvProducts.Visible = false; pnlEmpty.Visible = true; }
            else
            { gvProducts.DataSource = dt; gvProducts.DataBind(); gvProducts.Visible = true; pnlEmpty.Visible = false; }
        }
    }
}
