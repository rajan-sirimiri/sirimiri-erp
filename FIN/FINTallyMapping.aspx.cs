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
        protected System.Web.UI.WebControls.Button       btnUpload;
        protected System.Web.UI.WebControls.Button       btnSaveOneProduct, btnSaveOneScrap, btnSaveOneCustomer;
        protected System.Web.UI.WebControls.Button       btnTabProducts, btnTabScrap, btnTabCustomers, btnTabMapped;
        protected System.Web.UI.WebControls.HiddenField  hfTab;
        protected System.Web.UI.WebControls.HiddenField  hfSaveProductData, hfSaveScrapData, hfSaveCustomerData;

        // Unmapped lists
        protected System.Web.UI.WebControls.Repeater     rptUnmappedProducts, rptUnmappedScrap, rptUnmappedCustomers;
        protected System.Web.UI.WebControls.Panel        pnlProducts, pnlScrap, pnlCustomers;
        protected System.Web.UI.WebControls.Label        lblProductCount, lblScrapCount, lblCustomerCount;
        protected System.Web.UI.WebControls.Label        lblProductMapped, lblScrapMapped, lblCustomerMapped;

        // Mapped items
        protected System.Web.UI.WebControls.Panel        pnlMapped;
        protected System.Web.UI.WebControls.Repeater     rptMappedProducts, rptMappedScrap, rptMappedCustomers;
        protected System.Web.UI.WebControls.Label        lblMappedProductCount, lblMappedScrapCount, lblMappedCustomerCount;

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
            { ShowAlert("Please select the Tally Mapping Template file.", false); return; }

            try
            {
                var tallyProducts = new List<string>();
                var tallyScrap = new List<string>();
                var tallyCustomers = new List<string>();

                using (var stream = new MemoryStream(fileUpload.FileBytes))
                using (var wb = new XLWorkbook(stream))
                {
                    // Sheet 1: Products — read column B (Tally Product Name)
                    if (wb.Worksheets.Count >= 1)
                    {
                        var ws = wb.Worksheet(1);
                        int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;
                        for (int r = 2; r <= lastRow; r++)
                        {
                            string name = ws.Cell(r, 2).GetString().Trim();
                            if (!string.IsNullOrEmpty(name)) tallyProducts.Add(name);
                        }
                    }

                    // Sheet 2: Scrap Items — read column B (Tally Scrap Name)
                    if (wb.Worksheets.Count >= 2)
                    {
                        var ws = wb.Worksheet(2);
                        int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;
                        for (int r = 2; r <= lastRow; r++)
                        {
                            string name = ws.Cell(r, 2).GetString().Trim();
                            if (!string.IsNullOrEmpty(name)) tallyScrap.Add(name);
                        }
                    }

                    // Sheet 3: Customers — read column B (Tally Customer Name)
                    if (wb.Worksheets.Count >= 3)
                    {
                        var ws = wb.Worksheet(3);
                        int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;
                        for (int r = 2; r <= lastRow; r++)
                        {
                            string name = ws.Cell(r, 2).GetString().Trim();
                            if (!string.IsNullOrEmpty(name)) tallyCustomers.Add(name);
                        }
                    }
                }

                // Store in session for mapping
                Session["TallyProducts"] = tallyProducts;
                Session["TallyScrap"] = tallyScrap;
                Session["TallyCustomers"] = tallyCustomers;

                // Auto-match customers against ERP customer master
                int autoMatched = FINDatabaseHelper.AutoMatchCustomers(tallyCustomers, null);

                // Bind all tabs
                BindUnmappedProducts();
                BindUnmappedScrap();
                BindUnmappedCustomers();
                pnlResults.Visible = true;

                ShowAlert("Loaded " + tallyProducts.Count + " products, " +
                    tallyScrap.Count + " scrap items, and " +
                    tallyCustomers.Count + " customers. " +
                    "Auto-matched " + autoMatched + " customers to ERP.", true);
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
            if (btnTabMapped != null) btnTabMapped.CssClass = tab == "MAPPED" ? "tab-btn active" : "tab-btn";
            if (pnlProducts != null) pnlProducts.Visible = tab == "PRODUCTS";
            if (pnlScrap != null) pnlScrap.Visible = tab == "SCRAP";
            if (pnlCustomers != null) pnlCustomers.Visible = tab == "CUSTOMERS";
            if (pnlMapped != null) pnlMapped.Visible = tab == "MAPPED";
        }

        private void BindCurrentTab()
        {
            string tab = hfTab.Value;
            if (tab == "PRODUCTS") BindUnmappedProducts();
            else if (tab == "SCRAP") BindUnmappedScrap();
            else if (tab == "CUSTOMERS") BindUnmappedCustomers();
            else if (tab == "MAPPED") BindMappedItems();
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
                if (FINDatabaseHelper.ProductMappingExists(name))
                    mapped++;
                else
                    unmapped.Rows.Add(name);
            }

            rptUnmappedProducts.DataSource = unmapped;
            rptUnmappedProducts.DataBind();
            if (lblProductCount != null) lblProductCount.Text = unmapped.Rows.Count.ToString();
            if (lblProductMapped != null) lblProductMapped.Text = mapped.ToString();
        }

        protected void btnSaveOneProduct_Click(object sender, EventArgs e)
        {
            string data = hfSaveProductData.Value ?? "";
            hfSaveProductData.Value = "";
            if (string.IsNullOrEmpty(data)) return;

            // Format: tallyName||ProductID|PackForm|UnitsPerPack||MRP
            string[] parts = data.Split(new string[] { "||" }, StringSplitOptions.None);
            if (parts.Length < 2) return;

            string tallyName = parts[0];
            string combo = parts[1];
            string mrpStr = parts.Length > 2 ? parts[2] : "";

            string[] comboParts = combo.Split('|');
            if (comboParts.Length < 3) return;

            int productId;
            if (!int.TryParse(comboParts[0], out productId) || productId <= 0) return;
            string form = comboParts[1];
            int ppu = 1;
            int.TryParse(comboParts[2], out ppu);
            if (ppu < 1) ppu = 1;

            decimal? mrp = null;
            decimal m;
            if (decimal.TryParse(mrpStr, out m) && m > 0) mrp = m;

            FINDatabaseHelper.SaveProductMapping(tallyName, productId, form, ppu, mrp);
            BindUnmappedProducts();
            pnlResults.Visible = true;
            ShowAlert("Saved: " + tallyName, true);
        }

        // ── SCRAP MAPPING ──
        private void BindUnmappedScrap()
        {
            var tallyNames = Session["TallyScrap"] as List<string>;
            if (tallyNames == null) return;

            var unmapped = new DataTable();
            unmapped.Columns.Add("TallyName", typeof(string));
            int mapped = 0;

            foreach (string name in tallyNames)
            {
                if (FINDatabaseHelper.ScrapMappingExists(name))
                    mapped++;
                else
                    unmapped.Rows.Add(name);
            }

            rptUnmappedScrap.DataSource = unmapped;
            rptUnmappedScrap.DataBind();
            if (lblScrapCount != null) lblScrapCount.Text = unmapped.Rows.Count.ToString();
            if (lblScrapMapped != null) lblScrapMapped.Text = mapped.ToString();
        }

        protected void btnSaveOneScrap_Click(object sender, EventArgs e)
        {
            string data = hfSaveScrapData.Value ?? "";
            hfSaveScrapData.Value = "";
            if (string.IsNullOrEmpty(data)) return;

            string[] parts = data.Split(new string[] { "||" }, StringSplitOptions.None);
            if (parts.Length < 2) return;

            string tallyName = parts[0];
            int scrapId;
            if (!int.TryParse(parts[1], out scrapId) || scrapId <= 0) return;

            FINDatabaseHelper.SaveScrapMapping(tallyName, scrapId);
            BindUnmappedScrap();
            pnlResults.Visible = true;
            ShowAlert("Saved: " + tallyName, true);
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

        protected void btnSaveOneCustomer_Click(object sender, EventArgs e)
        {
            string data = hfSaveCustomerData.Value ?? "";
            hfSaveCustomerData.Value = "";
            if (string.IsNullOrEmpty(data)) return;

            string[] parts = data.Split(new string[] { "||" }, StringSplitOptions.None);
            if (parts.Length < 2) return;

            string tallyName = parts[0];
            int customerId;
            if (!int.TryParse(parts[1], out customerId) || customerId <= 0) return;

            FINDatabaseHelper.SaveCustomerMapping(tallyName, customerId);
            BindUnmappedCustomers();
            pnlResults.Visible = true;
            ShowAlert("Saved: " + tallyName, true);
        }

        // ── HELPERS ──

        protected string RenderProductFGDropdown(object tallyNameObj)
        {
            string tallyName = tallyNameObj.ToString();
            string safeName = System.Web.HttpUtility.HtmlAttributeEncode(tallyName);
            var options = FINDatabaseHelper.GetProductsWithFGOptions();
            var sb = new System.Text.StringBuilder();
            sb.Append("<select name='prodfg_" + safeName + "' class='map-select'>");
            sb.Append("<option value=''>-- Select Product + Packing --</option>");
            foreach (DataRow r in options.Rows)
            {
                string val = r["ProductID"] + "|" + r["PackForm"] + "|" + r["UnitsPerPack"];
                sb.Append("<option value='" + System.Web.HttpUtility.HtmlAttributeEncode(val) + "'>" +
                    System.Web.HttpUtility.HtmlEncode(r["DisplayLabel"]) + "</option>");
            }
            sb.Append("</select>");
            return sb.ToString();
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
            sb.Append("<select class='map-select'>");
            sb.Append("<option value='0'>-- Select Scrap --</option>");
            foreach (DataRow r in scraps.Rows)
                sb.Append("<option value='" + r["ScrapID"] + "'>" +
                    System.Web.HttpUtility.HtmlEncode(r["ScrapName"] + " (" + r["ScrapCode"] + ")") + "</option>");
            sb.Append("</select>");
            return sb.ToString();
        }

        protected string RenderCustomerDropdown(object tallyNameObj)
        {
            string tallyName = tallyNameObj.ToString();
            string safeTally = System.Web.HttpUtility.HtmlAttributeEncode(tallyName);
            // Render a text input for search + hidden select for value
            return "<div class='cust-search-wrap' data-tally='" + safeTally + "'>" +
                "<input type='text' class='cust-search-input map-select' placeholder='Type to search...' autocomplete='off'/>" +
                "<input type='hidden' class='cust-search-val' value='0'/>" +
                "<div class='cust-search-list'></div>" +
                "</div>";
        }

        // Called once from ASPX to build the JS customer array
        protected string GetCustomerJsonArray()
        {
            var customers = FINDatabaseHelper.GetAllCustomers();
            var sb = new System.Text.StringBuilder("[");
            bool first = true;
            foreach (DataRow r in customers.Rows)
            {
                if (!first) sb.Append(",");
                string name = r["CustomerName"].ToString().Replace("\\", "\\\\").Replace("\"", "\\\"");
                string type = r["CustomerType"] != DBNull.Value ? r["CustomerType"].ToString() : "";
                sb.Append("{\"id\":" + r["CustomerID"] + ",\"n\":\"" + name + "\",\"t\":\"" + type + "\"}");
                first = false;
            }
            sb.Append("]");
            return sb.ToString();
        }

        private void BindMappedItems()
        {
            // Products
            var prodMap = FINDatabaseHelper.GetAllProductMappings();
            if (rptMappedProducts != null) { rptMappedProducts.DataSource = prodMap; rptMappedProducts.DataBind(); }
            if (lblMappedProductCount != null) lblMappedProductCount.Text = prodMap.Rows.Count + " products mapped";

            // Scrap
            var scrapMap = FINDatabaseHelper.GetAllScrapMappings();
            if (rptMappedScrap != null) { rptMappedScrap.DataSource = scrapMap; rptMappedScrap.DataBind(); }
            if (lblMappedScrapCount != null) lblMappedScrapCount.Text = scrapMap.Rows.Count + " scrap items mapped";

            // Customers
            var custMap = FINDatabaseHelper.GetAllCustomerMappings();
            if (rptMappedCustomers != null) { rptMappedCustomers.DataSource = custMap; rptMappedCustomers.DataBind(); }
            if (lblMappedCustomerCount != null) lblMappedCustomerCount.Text = custMap.Rows.Count + " customers mapped";
        }

        private void ShowAlert(string msg, bool success)
        {
            pnlAlert.Visible = true;
            lblAlert.Text = msg;
            pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger");
        }
    }
}
