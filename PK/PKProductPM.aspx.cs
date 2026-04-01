using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKApp.DAL;

namespace PKApp
{
    public partial class PKProductPM : Page
    {
        protected Label    lblUser, lblAlert, lblProdCount, lblFormTitle;
        protected Label    lblProductName, lblProductMeta, lblContainerType, lblMappingCount;
        protected Panel    pnlAlert, pnlProdEmpty, pnlNoProduct, pnlMapping;
        protected Panel    pnlMappingEmpty, pnlMappingTable;
        protected HiddenField hfProductID, hfMappingID;
        protected DropDownList ddlPM, ddlApplyLevel, ddlLanguage;
        protected TextBox  txtQtyPerUnit;
        protected Button   btnAddPM, btnClear;
        protected Repeater rptProducts, rptMappings;
        protected System.Web.UI.HtmlControls.HtmlGenericControl rowLanguage;

        protected int UserID => Convert.ToInt32(Session["PK_UserID"]);

        protected void Page_Load(object s, EventArgs e)
        {
            if (Session["PK_UserID"] == null) { Response.Redirect("PKLogin.aspx?ReturnUrl=" + Server.UrlEncode(Request.Url.PathAndQuery)); return; }

            // Module access check
            string __role = Session["PK_Role"]?.ToString() ?? "";
            if (!PKDatabaseHelper.RoleHasModuleAccess(__role, "PK", "PK_PM_MAPPING"))
            { Response.Redirect("PKHome.aspx"); return; }
            lblUser.Text = Session["PK_FullName"] as string ?? "";
            if (!IsPostBack)
            {
                BindProductList();
                BindPMDropdown();
            }
        }

        // ── Product list (left panel) ─────────────────────────────────────
        void BindProductList()
        {
            var dt = PKDatabaseHelper.GetProductsWithPMCount();
            pnlProdEmpty.Visible = dt.Rows.Count == 0;
            rptProducts.DataSource = dt;
            rptProducts.DataBind();
            lblProdCount.Text = dt.Rows.Count.ToString();
        }

        protected void rptProducts_Cmd(object src, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "SelectProduct") return;
            int productId = Convert.ToInt32(e.CommandArgument);
            hfProductID.Value = productId.ToString();
            LoadProduct(productId);
            BindProductList(); // re-bind to update selected highlight
        }

        void LoadProduct(int productId)
        {
            var prod = PKDatabaseHelper.GetProductForPMMapping(productId);
            if (prod == null)
            {
                ShowAlert("Product not found.", false);
                return;
            }

            lblProductName.Text    = prod["ProductName"].ToString();
            lblProductMeta.Text    = prod["ProductCode"].ToString();
            lblContainerType.Text  = prod["ContainerType"] == DBNull.Value ? "DIRECT" : prod["ContainerType"].ToString();

            // Show language dropdown only for products with language-specific labels
            bool hasLangLabels = prod.Table.Columns.Contains("HasLanguageLabels")
                && prod["HasLanguageLabels"] != DBNull.Value
                && Convert.ToInt32(prod["HasLanguageLabels"]) == 1;
            if (rowLanguage != null)
                rowLanguage.Style["display"] = hasLangLabels ? "block" : "none";

            pnlNoProduct.Visible = false;
            pnlMapping.Visible   = true;

            ClearForm();
            BindMappings(productId);
        }

        // ── PM dropdown ───────────────────────────────────────────────────
        void BindPMDropdown()
        {
            var dt = PKDatabaseHelper.GetActivePackingMaterials();
            ddlPM.Items.Clear();
            ddlPM.Items.Add(new ListItem("-- Select Packing Material --", "0"));
            foreach (DataRow r in dt.Rows)
                ddlPM.Items.Add(new ListItem(
                    r["PMName"] + " (" + r["PMCode"] + ") — " + r["Abbreviation"],
                    r["PMID"].ToString() + "|" + r["Abbreviation"].ToString()));
        }

        // ── Mappings table (right panel) ──────────────────────────────────
        void BindMappings(int productId)
        {
            var dt = PKDatabaseHelper.GetProductPMMappings(productId);
            pnlMappingEmpty.Visible = dt.Rows.Count == 0;
            pnlMappingTable.Visible = dt.Rows.Count > 0;
            rptMappings.DataSource  = dt;
            rptMappings.DataBind();
            lblMappingCount.Text = dt.Rows.Count.ToString();
        }

        // ── Add / Update PM ──────────────────────────────────────────────
        protected void btnAddPM_Click(object s, EventArgs e)
        {
            int productId = Convert.ToInt32(hfProductID.Value);
            if (productId == 0) { ShowAlert("Please select a product first.", false); return; }

            // Parse PMID from pipe-delimited value (PMID|Abbreviation)
            string pmVal = ddlPM.SelectedValue;
            if (pmVal == "0") { ShowAlert("Please select a packing material.", false); return; }
            string[] pmParts = pmVal.Split('|');
            int pmId = Convert.ToInt32(pmParts[0]);
            if (pmId == 0) { ShowAlert("Please select a packing material.", false); return; }

            decimal qty;
            if (!decimal.TryParse(txtQtyPerUnit.Text.Trim(), out qty) || qty <= 0)
            { ShowAlert("Quantity must be a positive number.", false); return; }

            string level = ddlApplyLevel.SelectedValue;
            string language = (ddlLanguage != null && !string.IsNullOrEmpty(ddlLanguage.SelectedValue))
                ? ddlLanguage.SelectedValue : null;
            int mappingId = Convert.ToInt32(hfMappingID.Value);

            try
            {
                if (mappingId == 0)
                {
                    // Check for duplicate
                    if (PKDatabaseHelper.ProductPMMappingExists(productId, pmId, level, language))
                    {
                        string langLabel = language ?? "universal";
                        ShowAlert("This PM is already assigned at " + level + " level for " + langLabel + ". Use Edit to update.", false);
                        return;
                    }
                    PKDatabaseHelper.AddProductPMMapping(productId, pmId, qty, level, UserID, language);
                    ShowAlert("Packing material added to product.", true);
                }
                else
                {
                    PKDatabaseHelper.UpdateProductPMMapping(mappingId, pmId, qty, level, language);
                    ShowAlert("Mapping updated.", true);
                }

                ClearForm();
                BindMappings(productId);
                BindProductList(); // update PM count badges
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message, false);
            }
        }

        // ── Mapping actions (Edit / Delete) ──────────────────────────────
        protected void rptMappings_Cmd(object src, RepeaterCommandEventArgs e)
        {
            int productId = Convert.ToInt32(hfProductID.Value);

            if (e.CommandName == "EditMapping")
            {
                int mappingId = Convert.ToInt32(e.CommandArgument);
                var row = PKDatabaseHelper.GetProductPMMappingById(mappingId);
                if (row == null) return;

                hfMappingID.Value         = mappingId.ToString();
                // Find the dropdown item whose value starts with the PMID
                string targetPmId = row["PMID"].ToString();
                foreach (ListItem item in ddlPM.Items)
                {
                    if (item.Value.StartsWith(targetPmId + "|") || item.Value == targetPmId)
                    { ddlPM.SelectedValue = item.Value; break; }
                }
                txtQtyPerUnit.Text        = Convert.ToDecimal(row["QtyPerUnit"]).ToString("0.####");
                ddlApplyLevel.SelectedValue = row["ApplyLevel"].ToString();
                // Restore language selection
                if (ddlLanguage != null)
                {
                    string lang = row.Table.Columns.Contains("Language") && row["Language"] != DBNull.Value
                        ? row["Language"].ToString() : "";
                    try { ddlLanguage.SelectedValue = lang; } catch { ddlLanguage.SelectedIndex = 0; }
                }
                lblFormTitle.Text         = "Edit Packing Material";
                btnAddPM.Text             = "Update";
                pnlAlert.Visible          = false;
            }
            else if (e.CommandName == "DeleteMapping")
            {
                int mappingId = Convert.ToInt32(e.CommandArgument);
                PKDatabaseHelper.DeleteProductPMMapping(mappingId);
                ShowAlert("Packing material removed.", true);
                BindMappings(productId);
                BindProductList();
            }
        }

        // ── Clear / Alert ────────────────────────────────────────────────
        protected void btnClear_Click(object s, EventArgs e)
        {
            ClearForm();
            pnlAlert.Visible = false;
        }

        void ClearForm()
        {
            hfMappingID.Value = "0";
            if (ddlPM.Items.Count > 0) ddlPM.SelectedIndex = 0;
            txtQtyPerUnit.Text = "1";
            ddlApplyLevel.SelectedIndex = 0;
            if (ddlLanguage != null) ddlLanguage.SelectedIndex = 0;
            lblFormTitle.Text = "Add Packing Material";
            btnAddPM.Text     = "Add PM";
        }

        void ShowAlert(string m, bool ok)
        {
            lblAlert.Text     = m;
            pnlAlert.CssClass = "alert " + (ok ? "alert-success" : "alert-danger");
            pnlAlert.Visible  = true;
        }
    }
}
