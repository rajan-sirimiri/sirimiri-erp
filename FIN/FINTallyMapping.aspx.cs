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
    public partial class FINTallyMapping : Page
    {
        protected System.Web.UI.WebControls.Label        lblNavUser, lblAlert;
        protected System.Web.UI.WebControls.Panel        pnlAlert, pnlResults;
        protected System.Web.UI.WebControls.FileUpload   fileUpload;
        protected System.Web.UI.WebControls.Button       btnUpload;
        protected System.Web.UI.WebControls.Button       btnTabProducts, btnTabScrap, btnTabCustomers, btnTabMapped;
        protected System.Web.UI.WebControls.HiddenField  hfTab;

        // Unmapped lists
        protected System.Web.UI.WebControls.Repeater     rptUnmappedProducts, rptUnmappedScrap, rptUnmappedCustomers;
        protected System.Web.UI.WebControls.Panel        pnlProducts, pnlScrap, pnlCustomers;
        protected System.Web.UI.WebControls.Label        lblProductCount, lblScrapCount, lblCustomerCount;
        protected System.Web.UI.WebControls.Label        lblProductMapped, lblScrapMapped, lblCustomerMapped;

        // Mapped items
        protected System.Web.UI.WebControls.Panel        pnlMapped;
        protected System.Web.UI.WebControls.Repeater     rptMappedProducts, rptMappedScrap, rptMappedCustomers;
        protected System.Web.UI.WebControls.Label        lblMappedProductCount, lblMappedScrapCount, lblMappedCustomerCount;

        // Saved files
        protected System.Web.UI.WebControls.Repeater     rptSavedFiles;
        protected System.Web.UI.WebControls.Panel        pnlNoSavedFiles;

        protected int UserID => Session["FIN_UserID"] != null ? Convert.ToInt32(Session["FIN_UserID"]) : 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["FIN_UserID"] == null) { Response.Redirect("FINLogin.aspx"); return; }
            lblNavUser.Text = Session["FIN_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
            {
                hfTab.Value = "PRODUCTS";
                SetActiveTab();
                BindSavedFiles();

                // If session already has data, show results
                if (Session["TallyProducts"] != null)
                {
                    BindUnmappedProducts();
                    BindUnmappedScrap();
                    BindUnmappedCustomers();
                    pnlResults.Visible = true;
                }
            }
            else
            {
                SetActiveTab();
            }
        }

        private string GetUploadFolder()
        {
            string folder = Server.MapPath("~/App_Data/TallyUploads");
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
            { ShowAlert("Please select the Tally Mapping Template file.", false); return; }

            try
            {
                // Save file to server
                string folder = GetUploadFolder();
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string safeFileName = Path.GetFileNameWithoutExtension(fileUpload.FileName).Replace(" ", "_");
                string savedName = timestamp + "_" + safeFileName + ".xlsx";
                string savedPath = Path.Combine(folder, savedName);
                fileUpload.SaveAs(savedPath);

                // Load from saved file
                LoadMappingFile(savedPath);
                BindSavedFiles();
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message, false);
            }
        }

        protected void rptSavedFiles_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "LoadFile") return;
            string fileName = Path.GetFileName(e.CommandArgument.ToString());
            if (string.IsNullOrEmpty(fileName)) return;

            string folder = GetUploadFolder();
            string filePath = Path.Combine(folder, fileName);
            if (!File.Exists(filePath))
            { ShowAlert("File not found: " + fileName, false); return; }

            try
            {
                LoadMappingFile(filePath);
                BindSavedFiles();
            }
            catch (Exception ex)
            {
                ShowAlert("Error loading file: " + ex.Message, false);
            }
        }

        private void LoadMappingFile(string filePath)
        {
            var tallyProducts = new List<string>();
            var tallyScrap = new List<string>();
            var tallyCustomers = new List<string>();

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var wb = new XLWorkbook(stream))
            {
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

            Session["TallyProducts"] = tallyProducts;
            Session["TallyScrap"] = tallyScrap;
            Session["TallyCustomers"] = tallyCustomers;

            int autoMatched = FINDatabaseHelper.AutoMatchCustomers(tallyCustomers, null);

            BindUnmappedProducts();
            BindUnmappedScrap();
            BindUnmappedCustomers();
            pnlResults.Visible = true;

            string fileName = Path.GetFileName(filePath);
            ShowAlert("Loaded " + tallyProducts.Count + " products, " +
                tallyScrap.Count + " scrap items, " +
                tallyCustomers.Count + " customers from " + fileName + ". " +
                "Auto-matched " + autoMatched + " customers.", true);
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

            // Populate dropdowns after DataBind
            var options = FINDatabaseHelper.GetProductsWithFGOptions();
            foreach (System.Web.UI.WebControls.RepeaterItem item in rptUnmappedProducts.Items)
            {
                var ddl = item.FindControl("ddlProductFG") as System.Web.UI.WebControls.DropDownList;
                if (ddl != null)
                {
                    ddl.Items.Clear();
                    ddl.Items.Add(new System.Web.UI.WebControls.ListItem("-- Select Product + Packing --", ""));
                    foreach (DataRow r in options.Rows)
                    {
                        string val = r["ProductID"] + "|" + r["PackForm"] + "|" + r["UnitsPerPack"];
                        ddl.Items.Add(new System.Web.UI.WebControls.ListItem(r["DisplayLabel"].ToString(), val));
                    }
                }
            }

            if (lblProductCount != null) lblProductCount.Text = unmapped.Rows.Count.ToString();
            if (lblProductMapped != null) lblProductMapped.Text = mapped.ToString();
        }

        protected void rptUnmappedProducts_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "SaveProduct") return;
            string tallyName = e.CommandArgument.ToString();

            var ddl = e.Item.FindControl("ddlProductFG") as System.Web.UI.WebControls.DropDownList;
            var txtMrp = e.Item.FindControl("txtMRP") as System.Web.UI.WebControls.TextBox;

            if (ddl == null || string.IsNullOrEmpty(ddl.SelectedValue))
            { ShowAlert("Please select a product + packing option.", false); return; }

            string[] parts = ddl.SelectedValue.Split('|');
            if (parts.Length < 3) return;

            int productId = Convert.ToInt32(parts[0]);
            string form = parts[1];
            int ppu = Convert.ToInt32(parts[2]);

            decimal? mrp = null;
            decimal m;
            if (txtMrp != null && decimal.TryParse(txtMrp.Text, out m) && m > 0) mrp = m;

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

            var scraps = FINDatabaseHelper.GetAllScrapMaterials();
            foreach (System.Web.UI.WebControls.RepeaterItem item in rptUnmappedScrap.Items)
            {
                var ddl = item.FindControl("ddlScrap") as System.Web.UI.WebControls.DropDownList;
                if (ddl != null)
                {
                    ddl.Items.Clear();
                    ddl.Items.Add(new System.Web.UI.WebControls.ListItem("-- Select Scrap --", "0"));
                    foreach (DataRow r in scraps.Rows)
                        ddl.Items.Add(new System.Web.UI.WebControls.ListItem(
                            r["ScrapName"] + " (" + r["ScrapCode"] + ")", r["ScrapID"].ToString()));
                }
            }

            if (lblScrapCount != null) lblScrapCount.Text = unmapped.Rows.Count.ToString();
            if (lblScrapMapped != null) lblScrapMapped.Text = mapped.ToString();
        }

        protected void rptUnmappedScrap_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "SaveScrap") return;
            string tallyName = e.CommandArgument.ToString();

            var ddl = e.Item.FindControl("ddlScrap") as System.Web.UI.WebControls.DropDownList;
            if (ddl == null || ddl.SelectedValue == "0")
            { ShowAlert("Please select a scrap material.", false); return; }

            int scrapId = Convert.ToInt32(ddl.SelectedValue);
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

            var customers = FINDatabaseHelper.GetAllCustomers();
            foreach (System.Web.UI.WebControls.RepeaterItem item in rptUnmappedCustomers.Items)
            {
                var ddl = item.FindControl("ddlCustomer") as System.Web.UI.WebControls.DropDownList;
                if (ddl != null)
                {
                    ddl.Items.Clear();
                    ddl.Items.Add(new System.Web.UI.WebControls.ListItem("-- Select Customer --", "0"));
                    foreach (DataRow r in customers.Rows)
                    {
                        string type = r["CustomerType"] != DBNull.Value ? " [" + r["CustomerType"] + "]" : "";
                        ddl.Items.Add(new System.Web.UI.WebControls.ListItem(
                            r["CustomerName"].ToString() + type, r["CustomerID"].ToString()));
                    }
                }
            }

            if (lblCustomerCount != null) lblCustomerCount.Text = unmapped.Rows.Count.ToString();
            if (lblCustomerMapped != null) lblCustomerMapped.Text = mapped.ToString();
        }

        protected void rptUnmappedCustomers_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "SaveCustomer") return;
            string tallyName = e.CommandArgument.ToString();

            var ddl = e.Item.FindControl("ddlCustomer") as System.Web.UI.WebControls.DropDownList;
            if (ddl == null || ddl.SelectedValue == "0")
            { ShowAlert("Please select a customer.", false); return; }

            int customerId = Convert.ToInt32(ddl.SelectedValue);
            FINDatabaseHelper.SaveCustomerMapping(tallyName, customerId);
            BindUnmappedCustomers();
            pnlResults.Visible = true;
            ShowAlert("Saved: " + tallyName, true);
        }

        // ── HELPERS ──

        protected string RenderProductFGDropdown(object tallyNameObj)
        {
            var options = FINDatabaseHelper.GetProductsWithFGOptions();
            var sb = new System.Text.StringBuilder();
            sb.Append("<select class='map-select'>");
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
            return "<input type='number' step='0.01' min='0' placeholder='MRP' class='mrp-input'/>";
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
