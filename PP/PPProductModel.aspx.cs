using System;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PPApp.DAL;

namespace PPApp
{
    public partial class PPProductModel : Page
    {
        // Exposed to ASPX for JS batch-size calculation
        protected string BatchSizeForCost = "1";

        // ── Control Declarations ─────────────────────────────────────────────
        protected global::System.Web.UI.WebControls.Label          lblNavUser;
        protected global::System.Web.UI.WebControls.Label          lblAlert;
        protected global::System.Web.UI.WebControls.Panel          pnlAlert;
        protected global::System.Web.UI.WebControls.TextBox        txtCode;
        protected global::System.Web.UI.WebControls.TextBox        txtName;
        protected global::System.Web.UI.WebControls.TextBox        txtHSN;
        protected global::System.Web.UI.WebControls.TextBox        txtGSTRate;
        protected global::System.Web.UI.WebControls.TextBox        txtBatchSize;
        protected global::System.Web.UI.WebControls.DropDownList   ddlProductType;
        protected global::System.Web.UI.WebControls.DropDownList   ddlOutputUOM;
        protected global::System.Web.UI.WebControls.DropDownList   ddlProdUOM;
        protected global::System.Web.UI.WebControls.HiddenField    hfProductID;
        protected global::System.Web.UI.WebControls.HiddenField    hfImagePath;
        protected global::System.Web.UI.WebControls.Button         btnSave;
        protected global::System.Web.UI.WebControls.Button         btnClear;
        protected global::System.Web.UI.WebControls.Button         btnToggleActive;
        protected global::System.Web.UI.WebControls.Label          lblCount;
        protected global::System.Web.UI.WebControls.Panel          pnlEmpty;
        protected global::System.Web.UI.WebControls.Repeater       rptProducts;
        protected global::System.Web.UI.WebControls.Panel          pnlBOM;
        protected global::System.Web.UI.WebControls.Panel          pnlBOMEmpty;
        protected global::System.Web.UI.WebControls.Panel          pnlBOMTable;
        protected global::System.Web.UI.WebControls.Panel          pnlNoBOM;
        protected global::System.Web.UI.WebControls.Label          lblBOMProductCode;
        protected global::System.Web.UI.WebControls.Label          lblBOMProductName;
        protected global::System.Web.UI.WebControls.Label          lblBOMProductType;
        protected global::System.Web.UI.WebControls.Label          lblBOMCount;
        protected global::System.Web.UI.WebControls.Repeater       rptBOM;
        protected global::System.Web.UI.WebControls.DropDownList   ddlMatType;
        protected global::System.Web.UI.WebControls.DropDownList   ddlMaterial;
        protected global::System.Web.UI.WebControls.DropDownList   ddlIngUOM;
        protected global::System.Web.UI.WebControls.TextBox        txtIngQty;
        protected global::System.Web.UI.WebControls.Button         btnAddIng;
        protected global::System.Web.UI.WebControls.Button         btnSaveBOM;
        protected global::System.Web.UI.WebControls.Panel          pnlCost;
        protected global::System.Web.UI.WebControls.Panel          pnlNoCost;
        protected global::System.Web.UI.WebControls.Label          lblCostBatchSize;
        protected global::System.Web.UI.WebControls.Repeater       rptCostRates;
        protected global::System.Web.UI.WebControls.Label          lblCostBOMLines;
        protected global::System.Web.UI.WebControls.Label          lblCostExpectedOutput;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PP_UserID"] == null) { Response.Redirect("PPLogin.aspx"); return; }

            lblNavUser.Text = Session["PP_FullName"] as string ?? "";

            // LoadIngUOM must run on every postback so ddlOutputUOM/ddlProdUOM
            // always have items — otherwise SelectedValue cannot be restored
            LoadIngUOM();
            if (!IsPostBack)
            {
                BindProductList();
            }
        }

        // ── UOM ──────────────────────────────────────────────────────────────

        private int GetBatchesUOMID()
        {
            var dt = PPDatabaseHelper.GetActiveUOM();
            foreach (DataRow row in dt.Rows)
            {
                string name = row["UOMName"].ToString().Trim().ToLower();
                string abbr = row["Abbreviation"].ToString().Trim().ToLower();
                if (name == "batches" || name == "batch" || abbr == "batch" || abbr == "batches")
                    return Convert.ToInt32(Convert.ToInt64(row["UOMID"]));
            }
            if (dt.Rows.Count > 0) return Convert.ToInt32(Convert.ToInt64(dt.Rows[0]["UOMID"]));
            return 1;
        }

        private void LoadIngUOM()
        {
            var dt = PPDatabaseHelper.GetActiveUOM();

            // Preserve posted values before rebinding so selections survive postback
            string selOutput = ddlOutputUOM.SelectedValue;
            string selProd   = ddlProdUOM.SelectedValue;
            string selIng    = ddlIngUOM.SelectedValue;

            ddlIngUOM.DataSource     = dt;
            ddlIngUOM.DataTextField  = "Abbreviation";
            ddlIngUOM.DataValueField = "UOMID";
            ddlIngUOM.DataBind();
            ddlIngUOM.Items.Insert(0, new ListItem("-- UOM --", "0"));

            // Conversion Production UOM uses same list
            ddlProdUOM.DataSource     = dt;
            ddlProdUOM.DataTextField  = "Abbreviation";
            ddlProdUOM.DataValueField = "UOMID";
            ddlProdUOM.DataBind();
            ddlProdUOM.Items.Insert(0, new ListItem("-- UOM --", "0"));

            // Output UOM uses same list
            ddlOutputUOM.DataSource     = dt;
            ddlOutputUOM.DataTextField  = "Abbreviation";
            ddlOutputUOM.DataValueField = "UOMID";
            ddlOutputUOM.DataBind();
            ddlOutputUOM.Items.Insert(0, new ListItem("-- UOM --", "0"));

            // Restore selections after rebind
            if (IsPostBack)
            {
                if (ddlOutputUOM.Items.FindByValue(selOutput) != null)
                    ddlOutputUOM.SelectedValue = selOutput;
                if (ddlProdUOM.Items.FindByValue(selProd) != null)
                    ddlProdUOM.SelectedValue = selProd;
                if (ddlIngUOM.Items.FindByValue(selIng) != null)
                    ddlIngUOM.SelectedValue = selIng;
            }
        }

        // ── PRODUCT LIST ─────────────────────────────────────────────────────
        private void BindProductList()
        {
            var dt = PPDatabaseHelper.GetAllProducts();
            int selectedId = GetSelectedProductId();

            if (dt.Rows.Count == 0)
            {
                rptProducts.Visible = false;
                pnlEmpty.Visible    = true;
            }
            else
            {
                rptProducts.Visible    = true;
                pnlEmpty.Visible       = false;
                rptProducts.DataSource = dt;
                rptProducts.DataBind();
            }
            lblCount.Text = dt.Rows.Count + " product" + (dt.Rows.Count == 1 ? "" : "s");
        }

        protected string GetSelectedClass(object productId)
        {
            return GetSelectedProductId() == Convert.ToInt32(Convert.ToInt64(productId)) ? "selected" : "";
        }

        private int GetSelectedProductId()
        {
            int id;
            int.TryParse(hfProductID.Value, out id);
            return id;
        }

        // ── PRODUCT FORM SAVE ─────────────────────────────────────────────────
        protected void btnSave_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name)) { ShowAlert("Product Name is required.", false); return; }

            string type = ddlProductType.SelectedValue;
            if (string.IsNullOrEmpty(type)) { ShowAlert("Product Type is required.", false); return; }

            int uomId;
            if (!int.TryParse(ddlOutputUOM.SelectedValue, out uomId) || uomId == 0)
            { ShowAlert("Output UOM is required.", false); return; }

            // Production UOM: Conversion uses selected dropdown, others use Batches
            int prodUomId;
            if (type == "Conversion")
            {
                if (!int.TryParse(ddlProdUOM.SelectedValue, out prodUomId) || prodUomId == 0)
                { ShowAlert("Production UOM is required for Conversion type.", false); return; }
            }
            else
            {
                prodUomId = GetBatchesUOMID();
            }

            decimal batchSize = 1;
            decimal.TryParse(txtBatchSize.Text.Trim(), out batchSize);
            if (batchSize <= 0) batchSize = 1;

            decimal? gstRate = null;
            decimal g;
            if (decimal.TryParse(txtGSTRate.Text.Trim(), out g)) gstRate = g;

            string hsnCode = txtHSN.Text.Trim();
            string imagePath = hfImagePath.Value;

            // Handle image upload
            var fileImage = Request.Files["fileImage"];
            if (fileImage != null && fileImage.ContentLength > 0)
            {
                string uploadDir = Server.MapPath("~/ProductImages/");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
                string ext      = Path.GetExtension(fileImage.FileName);
                string fileName = "prod_" + DateTime.Now.Ticks + ext;
                fileImage.SaveAs(Path.Combine(uploadDir, fileName));
                imagePath = "ProductImages/" + fileName;
            }

            int productId = GetSelectedProductId();
            try
            {
                if (productId == 0)
                {
                    productId = PPDatabaseHelper.AddProduct(name, null, hsnCode, gstRate, prodUomId, uomId, batchSize, true, type, imagePath);
                    hfProductID.Value = productId.ToString();
                    ShowAlert("Product '" + name + "' saved successfully.", true);
                }
                else
                {
                    string code = txtCode.Text.Trim();
                    PPDatabaseHelper.UpdateProduct(productId, code, name, null, hsnCode, gstRate, prodUomId, uomId, batchSize, true, type, imagePath);
                    ShowAlert("Product updated successfully.", true);
                }

                hfImagePath.Value = imagePath;
                BindProductList();
                LoadSelectedProduct(productId);
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message + " | Source: " + ex.Source + " | " + ex.StackTrace, false);
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            BindProductList();
            HideBOM();
            HideCost();
        }

        protected void btnToggleActive_Click(object sender, EventArgs e)
        {
            int productId = GetSelectedProductId();
            if (productId == 0) return;
            bool current = btnToggleActive.Text == "Deactivate";
            PPDatabaseHelper.ToggleProductActive(productId, !current);
            ShowAlert("Product " + (!current ? "activated" : "deactivated") + ".", true);
            BindProductList();
            LoadSelectedProduct(productId);
        }

        // ── SELECT PRODUCT FROM LIST ──────────────────────────────────────────
        protected void rptProducts_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Select")
            {
                int productId = Convert.ToInt32(Convert.ToInt64(e.CommandArgument));
                hfProductID.Value = productId.ToString();
                LoadSelectedProduct(productId);
                LoadBOM(productId);
                BindProductList();
            }
        }

        private void LoadSelectedProduct(int productId)
        {
            var row = PPDatabaseHelper.GetProductById(productId);
            if (row == null) return;

            txtCode.Text         = row["ProductCode"].ToString();
            txtName.Text         = row["ProductName"].ToString();
            txtHSN.Text          = row["HSNCode"] == DBNull.Value ? "" : row["HSNCode"].ToString();
            txtGSTRate.Text      = row["GSTRate"] == DBNull.Value ? "" : row["GSTRate"].ToString();
            txtBatchSize.Text    = row["BatchSize"].ToString();
            hfImagePath.Value    = row["ImagePath"] == DBNull.Value ? "" : row["ImagePath"].ToString();

            ddlProductType.SelectedValue = row["ProductType"].ToString();
            // Restore output UOM
            try { ddlOutputUOM.SelectedValue = row["OutputUOMID"].ToString(); } catch { }
            // Restore production UOM for Conversion
            try { ddlProdUOM.SelectedValue = row["ProdUOMID"].ToString(); } catch { }
            // Toggle visibility via JS on page load
            string prodType = row["ProductType"].ToString();
            if (prodType == "Conversion")
            {
                ddlProdUOM.Style["display"]     = "";
                ClientScript.RegisterStartupScript(this.GetType(), "prodUOMToggle",
                    "document.getElementById('divProdUOMStatic').style.display='none';", true);
            }

            bool isActive = Convert.ToBoolean(row["IsActive"]);
            btnToggleActive.Text    = isActive ? "Deactivate" : "Activate";
            btnToggleActive.Visible = true;
        }

        private void ClearForm()
        {
            hfProductID.Value    = "0";
            hfImagePath.Value    = "";
            txtCode.Text         = "";
            txtName.Text         = "";
            txtHSN.Text          = "";
            txtGSTRate.Text      = "";
            txtBatchSize.Text    = "";
            ddlProductType.SelectedIndex = 0;
            if (ddlOutputUOM.Items.Count > 0) ddlOutputUOM.SelectedIndex = 0;
            if (ddlProdUOM.Items.Count > 0) ddlProdUOM.SelectedIndex = 0;
            ddlProdUOM.Style["display"] = "none";
            ClientScript.RegisterStartupScript(this.GetType(), "prodUOMReset",
                "document.getElementById('divProdUOMStatic').style.display='';", true);
            btnToggleActive.Visible = false;
            pnlAlert.Visible = false;
        }

        // ── BOM ───────────────────────────────────────────────────────────────
        protected void ddlMatType_Changed(object sender, EventArgs e)
        {
            LoadMaterialDropdown(ddlMatType.SelectedValue);
        }

        private void LoadMaterialDropdown(string type)
        {
            ddlMaterial.Items.Clear();
            if (string.IsNullOrEmpty(type)) { ddlMaterial.Items.Add(new ListItem("-- Select type first --", "0")); return; }

            DataTable dt;
            string valField, textField;
            switch (type)
            {
                case "RM": dt = PPDatabaseHelper.GetActiveRawMaterials();    valField = "RMID";         textField = "RMName";         break;
                case "PM": dt = PPDatabaseHelper.GetActivePackingMaterials(); valField = "PMID";         textField = "PMName";         break;
                case "CN": dt = PPDatabaseHelper.GetActiveConsumables();     valField = "ConsumableID"; textField = "ConsumableName"; break;
                default:   ddlMaterial.Items.Add(new ListItem("-- Select type first --", "0")); return;
            }
            ddlMaterial.DataSource     = dt;
            ddlMaterial.DataTextField  = textField;
            ddlMaterial.DataValueField = valField;
            ddlMaterial.DataBind();
            ddlMaterial.Items.Insert(0, new ListItem("-- Select material --", "0"));
        }

        protected void btnAddIng_Click(object sender, EventArgs e)
        {
            int productId = GetSelectedProductId();
            if (productId == 0) { ShowAlert("Please select or save a product first.", false); return; }

            string matType = ddlMatType.SelectedValue;
            if (string.IsNullOrEmpty(matType)) { ShowAlert("Please select a material type.", false); return; }

            int matId;
            if (!int.TryParse(ddlMaterial.SelectedValue, out matId) || matId == 0)
            { ShowAlert("Please select a material.", false); return; }

            decimal qty;
            if (!decimal.TryParse(txtIngQty.Text.Trim(), out qty) || qty <= 0)
            { ShowAlert("Please enter a valid quantity.", false); return; }

            int uomId;
            if (!int.TryParse(ddlIngUOM.SelectedValue, out uomId) || uomId == 0)
            { ShowAlert("Please select a UOM.", false); return; }

            try
            {
                PPDatabaseHelper.AddBOMLine(productId, matType, matId, qty, uomId);
                txtIngQty.Text = "";
                ddlMatType.SelectedIndex  = 0;
                ddlMaterial.Items.Clear();
                ddlMaterial.Items.Add(new ListItem("-- Select type first --", "0"));
                LoadBOM(productId);
                ShowAlert("Ingredient added.", true);
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        protected void rptBOM_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "DeleteBOM")
            {
                int bomId = Convert.ToInt32(e.CommandArgument);
                PPDatabaseHelper.DeleteBOMLine(bomId);
                int productId = GetSelectedProductId();
                LoadBOM(productId);
                ShowAlert("Ingredient removed.", true);
            }
        }

        protected void btnSaveBOM_Click(object sender, EventArgs e)
        {
            // BOM lines are saved individually on Add; this is a confirmation trigger
            int productId = GetSelectedProductId();
            LoadBOM(productId);
            ShowAlert("BOM saved successfully.", true);
        }

        private void LoadBOM(int productId)
        {
            var row = PPDatabaseHelper.GetProductById(productId);
            if (row == null) { HideBOM(); return; }

            pnlNoBOM.Visible = false;
            pnlBOM.Visible   = true;

            lblBOMProductName.Text = row["ProductName"].ToString();
            lblBOMProductCode.Text = row["ProductCode"].ToString();
            lblBOMProductType.Text = row["ProductType"].ToString();
            // BOM UOM fixed as Batch — shown statically in ASPX

            BatchSizeForCost = row["BatchSize"].ToString();

            var bom = PPDatabaseHelper.GetBOMByProduct(productId);
            if (bom.Rows.Count == 0)
            {
                pnlBOMEmpty.Visible  = true;
                pnlBOMTable.Visible  = false;
                lblBOMCount.Text     = "No ingredients";
                btnSaveBOM.Visible   = false;
                HideCost();
            }
            else
            {
                pnlBOMEmpty.Visible    = false;
                pnlBOMTable.Visible    = true;
                rptBOM.DataSource      = bom;
                rptBOM.DataBind();
                lblBOMCount.Text       = bom.Rows.Count + " ingredient" + (bom.Rows.Count == 1 ? "" : "s");
                btnSaveBOM.Visible     = true;
                LoadCost(row, bom);
            }

            LoadIngUOM();
            if (!string.IsNullOrEmpty(ddlMatType.SelectedValue))
                LoadMaterialDropdown(ddlMatType.SelectedValue);
        }

        private void HideBOM()
        {
            pnlNoBOM.Visible = true;
            pnlBOM.Visible   = false;
            btnSaveBOM.Visible = false;
            lblBOMCount.Text = "";
            HideCost();
        }

        // ── COST PANEL ────────────────────────────────────────────────────────
        private void LoadCost(DataRow product, DataTable bom)
        {
            pnlNoCost.Visible = false;
            pnlCost.Visible   = true;

            // Expected output per batch (from product record)
            decimal batchSize;
            decimal.TryParse(product["BatchSize"].ToString(), out batchSize);
            string outAbbr = ddlOutputUOM.SelectedItem != null ? ddlOutputUOM.SelectedItem.Text : "units";
            lblCostExpectedOutput.Text = batchSize.ToString("N2").TrimEnd('0').TrimEnd('.') + " " + outAbbr;

            // Input batch size = sum of BOM quantities grouped by UOM
            // Normalise quantities to canonical units within each family
            // Family map: abbr (lower) -> (family, multiplier to canonical unit)
            var unitFamilies = new System.Collections.Generic.Dictionary<string,
                System.Tuple<string, decimal>>(System.StringComparer.OrdinalIgnoreCase)
            {
                // Weight family -> kg
                { "kg",   System.Tuple.Create("kg",    1m)        },
                { "g",    System.Tuple.Create("kg",    0.001m)    },
                { "mg",   System.Tuple.Create("kg",    0.000001m) },
                { "ton",  System.Tuple.Create("kg",    1000m)     },
                { "tonne",System.Tuple.Create("kg",    1000m)     },
                { "lb",   System.Tuple.Create("kg",    0.453592m) },
                // Volume family -> l
                { "l",    System.Tuple.Create("l",     1m)        },
                { "litre",System.Tuple.Create("l",     1m)        },
                { "ltr",  System.Tuple.Create("l",     1m)        },
                { "ml",   System.Tuple.Create("l",     0.001m)    },
                // Length family -> m
                { "m",    System.Tuple.Create("m",     1m)        },
                { "cm",   System.Tuple.Create("m",     0.01m)     },
                { "mm",   System.Tuple.Create("m",     0.001m)    },
            };

            // Accumulate totals in canonical units per family; unknown units kept separate
            var canonicalTotals = new System.Collections.Generic.Dictionary<string, decimal>(
                System.StringComparer.OrdinalIgnoreCase);
            var unknownTotals = new System.Collections.Generic.Dictionary<string, decimal>(
                System.StringComparer.OrdinalIgnoreCase);

            foreach (DataRow row in bom.Rows)
            {
                decimal qty;
                string abbr = row["Abbreviation"].ToString().Trim();
                if (string.IsNullOrEmpty(abbr)) abbr = "units";
                if (!decimal.TryParse(row["Quantity"].ToString(), out qty)) continue;

                System.Tuple<string, decimal> mapping;
                if (unitFamilies.TryGetValue(abbr, out mapping))
                {
                    string canonical = mapping.Item1;
                    decimal multiplier = mapping.Item2;
                    if (canonicalTotals.ContainsKey(canonical))
                        canonicalTotals[canonical] += qty * multiplier;
                    else
                        canonicalTotals[canonical] = qty * multiplier;
                }
                else
                {
                    if (unknownTotals.ContainsKey(abbr)) unknownTotals[abbr] += qty;
                    else unknownTotals[abbr] = qty;
                }
            }

            var parts = new System.Collections.Generic.List<string>();
            foreach (var kv in canonicalTotals)
                parts.Add(kv.Value.ToString("N3").TrimEnd('0').TrimEnd('.') + " " + kv.Key);
            foreach (var kv in unknownTotals)
                parts.Add(kv.Value.ToString("N3").TrimEnd('0').TrimEnd('.') + " " + kv.Key);

            lblCostBatchSize.Text = parts.Count > 0 ? string.Join(" + ", parts) : "—";

            lblCostBOMLines.Text = bom.Rows.Count + " ingredient" + (bom.Rows.Count == 1 ? "" : "s");
            BatchSizeForCost     = batchSize.ToString();

            rptCostRates.DataSource = bom;
            rptCostRates.DataBind();
        }

        private void HideCost()
        {
            pnlNoCost.Visible = true;
            pnlCost.Visible   = false;
        }

        // ── ALERT ─────────────────────────────────────────────────────────────
        private void ShowAlert(string msg, bool success)
        {
            lblAlert.Text      = msg;
            pnlAlert.CssClass  = "alert " + (success ? "alert-success" : "alert-danger");
            pnlAlert.Visible   = true;
        }
    }
}
