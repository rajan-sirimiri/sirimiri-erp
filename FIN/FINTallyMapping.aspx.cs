using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using FINApp.DAL;
using ClosedXML.Excel;

namespace FINApp
{
    public partial class FINTallyMapping : Page
    {
        protected System.Web.UI.WebControls.Label        lblNavUser, lblAlert;
        protected System.Web.UI.WebControls.Panel        pnlAlert, pnlResults;
        protected System.Web.UI.WebControls.FileUpload   fileUpload;
        protected System.Web.UI.WebControls.Button       btnUpload, btnSaveProducts, btnSaveScrap, btnSaveCustomers;
        protected System.Web.UI.WebControls.Button       btnTabProducts, btnTabScrap, btnTabCustomers;
        protected System.Web.UI.WebControls.HiddenField  hfTab;

        // Unmapped lists
        protected System.Web.UI.WebControls.Repeater     rptUnmappedProducts, rptUnmappedScrap, rptUnmappedCustomers;
        protected System.Web.UI.WebControls.Panel        pnlProducts, pnlScrap, pnlCustomers;
        protected System.Web.UI.WebControls.Label        lblProductCount, lblScrapCount, lblCustomerCount;
        protected System.Web.UI.WebControls.Label        lblProductMapped, lblScrapMapped, lblCustomerMapped;

        protected int UserID => Session["FIN_UserID"] != null ? Convert.ToInt32(Session["FIN_UserID"]) : 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["FIN_UserID"] == null) { Response.Redirect("FINLogin.aspx"); return; }
            lblNavUser.Text = Session["FIN_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
            {
                hfTab.Value = "PRODUCTS";
                SetActiveTab();
            }
            else
            {
                SetActiveTab();
            }
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            pnlAlert.Visible = false;
            if (fileUpload == null || !fileUpload.HasFile)
            { ShowAlert("Please select a Tally Excel file.", false); return; }

            try
            {
                // Parse Excel
                var tallyProducts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var tallyCustomers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                using (var stream = new MemoryStream(fileUpload.FileBytes))
                using (var wb = new XLWorkbook(stream))
                {
                    var ws = wb.Worksheet(1);
                    int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;

                    // Find columns by header
                    int colProduct = 0, colCustomer = 0;
                    int lastCol = ws.LastColumnUsed()?.ColumnNumber() ?? 0;
                    for (int c = 1; c <= lastCol; c++)
                    {
                        string h = ws.Cell(1, c).GetString().Trim().ToLower();
                        if (h.Contains("product")) colProduct = c;
                        if (h.Contains("customer")) colCustomer = c;
                    }

                    if (colProduct == 0 || colCustomer == 0)
                    { ShowAlert("Could not find 'Product Name' and 'Customer Name' columns in the Excel header.", false); return; }

                    for (int r = 2; r <= lastRow; r++)
                    {
                        string prod = ws.Cell(r, colProduct).GetString().Trim();
                        string cust = ws.Cell(r, colCustomer).GetString().Trim();
                        if (!string.IsNullOrEmpty(prod)) tallyProducts.Add(prod);
                        if (!string.IsNullOrEmpty(cust)) tallyCustomers.Add(cust);
                    }
                }

                // Store in session for mapping
                Session["TallyProducts"] = new List<string>(tallyProducts);
                Session["TallyCustomers"] = new List<string>(tallyCustomers);

                // Bind all tabs
                BindUnmappedProducts();
                BindUnmappedScrap();
                BindUnmappedCustomers();
                pnlResults.Visible = true;

                int totalNames = tallyProducts.Count + tallyCustomers.Count;
                ShowAlert("Parsed " + tallyProducts.Count + " product names and " +
                    tallyCustomers.Count + " customer names from the file.", true);
            }
            catch (Exception ex)
            {
                ShowAlert("Error reading file: " + ex.Message, false);
            }
        }

        // ── TAB SWITCHING ──
        protected void btnTab_Click(object sender, EventArgs e)
        {
            hfTab.Value = ((System.Web.UI.WebControls.Button)sender).CommandArgument;
            SetActiveTab();
            BindCurrentTab();
        }

        private void SetActiveTab()
        {
            string tab = hfTab.Value;
            if (btnTabProducts != null) btnTabProducts.CssClass = tab == "PRODUCTS" ? "tab-btn active" : "tab-btn";
            if (btnTabScrap != null) btnTabScrap.CssClass = tab == "SCRAP" ? "tab-btn active" : "tab-btn";
            if (btnTabCustomers != null) btnTabCustomers.CssClass = tab == "CUSTOMERS" ? "tab-btn active" : "tab-btn";
            if (pnlProducts != null) pnlProducts.Visible = tab == "PRODUCTS";
            if (pnlScrap != null) pnlScrap.Visible = tab == "SCRAP";
            if (pnlCustomers != null) pnlCustomers.Visible = tab == "CUSTOMERS";
        }

        private void BindCurrentTab()
        {
            string tab = hfTab.Value;
            if (tab == "PRODUCTS") BindUnmappedProducts();
            else if (tab == "SCRAP") BindUnmappedScrap();
            else if (tab == "CUSTOMERS") BindUnmappedCustomers();
        }

        // ── PRODUCT MAPPING ──
        private void BindUnmappedProducts()
        {
            var tallyNames = Session["TallyProducts"] as List<string>;
            if (tallyNames == null) return;

            var unmapped = new DataTable();
            unmapped.Columns.Add("TallyName", typeof(string));
            int mapped = 0;

            foreach (string name in tallyNames)
            {
                // Check if already mapped as product OR scrap
                if (FINDatabaseHelper.ProductMappingExists(name) || FINDatabaseHelper.ScrapMappingExists(name))
                    mapped++;
                else
                    unmapped.Rows.Add(name);
            }

            rptUnmappedProducts.DataSource = unmapped;
            rptUnmappedProducts.DataBind();
            if (lblProductCount != null) lblProductCount.Text = unmapped.Rows.Count.ToString();
            if (lblProductMapped != null) lblProductMapped.Text = mapped.ToString();
        }

        protected void btnSaveProducts_Click(object sender, EventArgs e)
        {
            int saved = 0;
            foreach (string key in Request.Form.AllKeys)
            {
                if (key == null || !key.StartsWith("prod_")) continue;
                string tallyName = key.Substring(5); // strip "prod_"
                string pidStr = Request.Form[key];
                if (string.IsNullOrEmpty(pidStr) || pidStr == "0") continue;

                int productId = Convert.ToInt32(pidStr);
                string form = Request.Form["form_" + tallyName] ?? "PCS";
                int ppu = 1;
                int.TryParse(Request.Form["ppu_" + tallyName], out ppu);
                if (ppu < 1) ppu = 1;
                decimal? mrp = null;
                decimal m;
                if (decimal.TryParse(Request.Form["mrp_" + tallyName], out m) && m > 0) mrp = m;

                FINDatabaseHelper.SaveProductMapping(tallyName, productId, form, ppu, mrp);
                saved++;
            }

            BindUnmappedProducts();
            ShowAlert("Saved " + saved + " product mapping(s).", saved > 0);
        }

        // ── SCRAP MAPPING ──
        private void BindUnmappedScrap()
        {
            var tallyNames = Session["TallyProducts"] as List<string>;
            if (tallyNames == null) return;

            var unmapped = new DataTable();
            unmapped.Columns.Add("TallyName", typeof(string));
            int mapped = 0;

            // Only show items that are not mapped as product either
            foreach (string name in tallyNames)
            {
                if (FINDatabaseHelper.ScrapMappingExists(name))
                    mapped++;
                // Don't show items already mapped as products
                else if (!FINDatabaseHelper.ProductMappingExists(name))
                {
                    // This could be scrap — show in scrap tab too for user to decide
                }
            }

            // Actually, scrap tab shows ALL unmapped items — user picks which are scrap
            // But to keep it clean, let's show the same unmapped list as products tab
            // The user will map from either tab
            var allUnmapped = new DataTable();
            allUnmapped.Columns.Add("TallyName", typeof(string));
            mapped = 0;
            foreach (string name in tallyNames)
            {
                if (FINDatabaseHelper.ProductMappingExists(name) || FINDatabaseHelper.ScrapMappingExists(name))
                    mapped++;
                else
                    allUnmapped.Rows.Add(name);
            }

            rptUnmappedScrap.DataSource = allUnmapped;
            rptUnmappedScrap.DataBind();
            if (lblScrapCount != null) lblScrapCount.Text = allUnmapped.Rows.Count.ToString();
            if (lblScrapMapped != null) lblScrapMapped.Text = mapped.ToString();
        }

        protected void btnSaveScrap_Click(object sender, EventArgs e)
        {
            int saved = 0;
            foreach (string key in Request.Form.AllKeys)
            {
                if (key == null || !key.StartsWith("scrap_")) continue;
                string tallyName = key.Substring(6);
                string sidStr = Request.Form[key];
                if (string.IsNullOrEmpty(sidStr) || sidStr == "0") continue;

                int scrapId = Convert.ToInt32(sidStr);
                FINDatabaseHelper.SaveScrapMapping(tallyName, scrapId);
                saved++;
            }

            BindUnmappedScrap();
            BindUnmappedProducts(); // refresh product count too
            ShowAlert("Saved " + saved + " scrap mapping(s).", saved > 0);
        }

        // ── CUSTOMER MAPPING ──
        private void BindUnmappedCustomers()
        {
            var tallyNames = Session["TallyCustomers"] as List<string>;
            if (tallyNames == null) return;

            var unmapped = new DataTable();
            unmapped.Columns.Add("TallyName", typeof(string));
            int mapped = 0;

            foreach (string name in tallyNames)
            {
                if (FINDatabaseHelper.CustomerMappingExists(name))
                    mapped++;
                else
                    unmapped.Rows.Add(name);
            }

            rptUnmappedCustomers.DataSource = unmapped;
            rptUnmappedCustomers.DataBind();
            if (lblCustomerCount != null) lblCustomerCount.Text = unmapped.Rows.Count.ToString();
            if (lblCustomerMapped != null) lblCustomerMapped.Text = mapped.ToString();
        }

        protected void btnSaveCustomers_Click(object sender, EventArgs e)
        {
            int saved = 0;
            foreach (string key in Request.Form.AllKeys)
            {
                if (key == null || !key.StartsWith("cust_")) continue;
                string tallyName = key.Substring(5);
                string cidStr = Request.Form[key];
                if (string.IsNullOrEmpty(cidStr) || cidStr == "0") continue;

                int customerId = Convert.ToInt32(cidStr);
                FINDatabaseHelper.SaveCustomerMapping(tallyName, customerId);
                saved++;
            }

            BindUnmappedCustomers();
            ShowAlert("Saved " + saved + " customer mapping(s).", saved > 0);
        }

        // ── HELPERS ──

        protected string RenderProductDropdown(object tallyNameObj)
        {
            string tallyName = tallyNameObj.ToString();
            string safeName = System.Web.HttpUtility.HtmlAttributeEncode(tallyName);
            var products = FINDatabaseHelper.GetAllProducts();
            var sb = new System.Text.StringBuilder();
            sb.Append("<select name='prod_" + safeName + "' class='map-select'>");
            sb.Append("<option value='0'>-- Select Product --</option>");
            foreach (DataRow r in products.Rows)
                sb.Append("<option value='" + r["ProductID"] + "'>" +
                    System.Web.HttpUtility.HtmlEncode(r["ProductName"] + " (" + r["ProductCode"] + ")") + "</option>");
            sb.Append("</select>");
            return sb.ToString();
        }

        protected string RenderFormDropdown(object tallyNameObj)
        {
            string safeName = System.Web.HttpUtility.HtmlAttributeEncode(tallyNameObj.ToString());
            return "<select name='form_" + safeName + "' class='form-select'>" +
                "<option value='PCS'>PCS</option>" +
                "<option value='JAR'>JAR</option>" +
                "<option value='CASE'>CASE</option>" +
                "<option value='TRAY'>TRAY</option>" +
                "</select>";
        }

        protected string RenderPPUInput(object tallyNameObj)
        {
            string safeName = System.Web.HttpUtility.HtmlAttributeEncode(tallyNameObj.ToString());
            return "<input type='number' name='ppu_" + safeName + "' value='1' min='1' class='ppu-input'/>";
        }

        protected string RenderMRPInput(object tallyNameObj)
        {
            string safeName = System.Web.HttpUtility.HtmlAttributeEncode(tallyNameObj.ToString());
            return "<input type='number' name='mrp_" + safeName + "' step='0.01' min='0' placeholder='MRP' class='mrp-input'/>";
        }

        protected string RenderScrapDropdown(object tallyNameObj)
        {
            string safeName = System.Web.HttpUtility.HtmlAttributeEncode(tallyNameObj.ToString());
            var scraps = FINDatabaseHelper.GetAllScrapMaterials();
            var sb = new System.Text.StringBuilder();
            sb.Append("<select name='scrap_" + safeName + "' class='map-select'>");
            sb.Append("<option value='0'>-- Not Scrap --</option>");
            foreach (DataRow r in scraps.Rows)
                sb.Append("<option value='" + r["ScrapID"] + "'>" +
                    System.Web.HttpUtility.HtmlEncode(r["ScrapName"] + " (" + r["ScrapCode"] + ")") + "</option>");
            sb.Append("</select>");
            return sb.ToString();
        }

        protected string RenderCustomerDropdown(object tallyNameObj)
        {
            string safeName = System.Web.HttpUtility.HtmlAttributeEncode(tallyNameObj.ToString());
            var customers = FINDatabaseHelper.GetAllCustomers();
            var sb = new System.Text.StringBuilder();
            sb.Append("<select name='cust_" + safeName + "' class='map-select'>");
            sb.Append("<option value='0'>-- Select Customer --</option>");
            foreach (DataRow r in customers.Rows)
            {
                string type = r["CustomerType"] != DBNull.Value ? " [" + r["CustomerType"] + "]" : "";
                sb.Append("<option value='" + r["CustomerID"] + "'>" +
                    System.Web.HttpUtility.HtmlEncode(r["CustomerName"] + type) + "</option>");
            }
            sb.Append("</select>");
            return sb.ToString();
        }

        private void ShowAlert(string msg, bool success)
        {
            pnlAlert.Visible = true;
            lblAlert.Text = msg;
            pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger");
        }
    }
}
