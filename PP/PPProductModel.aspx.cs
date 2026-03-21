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
        protected global::System.Web.UI.WebControls.Image          imgSaved;
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

        private void LoadIngUOM(bool restoreFromForm = true)
        {
            var dt = PPDatabaseHelper.GetActiveUOM();

            BindDropdown(ddlIngUOM,    dt, "Abbreviation", "UOMID", "-- UOM --");
            BindDropdown(ddlProdUOM,   dt, "Abbreviation", "UOMID", "-- UOM --");
            BindDropdown(ddlOutputUOM, dt, "Abbreviation", "UOMID", "-- UOM --");

            // Only restore from form post when called from Page_Load
            // When called from LoadBOM, the caller (LoadSelectedProduct) sets values
            if (IsPostBack && restoreFromForm)
            {
                TrySetValue(ddlOutputUOM, Request.Form[ddlOutputUOM.UniqueID]);
                TrySetValue(ddlProdUOM,   Request.Form[ddlProdUOM.UniqueID]);
                TrySetValue(ddlIngUOM,    Request.Form[ddlIngUOM.UniqueID]);
            }
        }

        private void SetProdUOMVisibility(string productType)
        {
            // All product types use Batches — always show static label, hide dropdown
            ddlProdUOM.Style["display"] = "none";
        }

        private void BindDropdown(System.Web.UI.WebControls.DropDownList ddl,
            System.Data.DataTable dt, string textField, string valueField, string defaultText)
        {
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem(defaultText, "0"));
            foreach (System.Data.DataRow row in dt.Rows)
            {
                // Convert int64 MySQL IDs to plain int string to ensure SelectedValue match
                string val = Convert.ToInt32(Convert.ToInt64(row[valueField])).ToString();
                ddl.Items.Add(new ListItem(row[textField].ToString(), val));
            }
        }

        private void TrySetValue(System.Web.UI.WebControls.DropDownList ddl, string value)
        {
            if (!string.IsNullOrEmpty(value) && ddl.Items.FindByValue(value) != null)
                ddl.SelectedValue = value;
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

            // Production UOM is always Batches for all product types
            int prodUomId = GetBatchesUOMID();

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
                try
                {
                    string uploadDir = Server.MapPath("~/ProductImages/");
                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
                    string ext      = Path.GetExtension(fileImage.FileName).ToLower();
                    // Only allow image extensions
                    if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".gif" && ext != ".webp")
                    { ShowAlert("Invalid image format. Use JPG, PNG, GIF or WebP.", false); return; }
                    string fileName = "prod_" + PPDatabaseHelper.NowIST().Ticks + ext;
                    string fullPath = Path.Combine(uploadDir, fileName);
                    fileImage.SaveAs(fullPath);
                    imagePath = "ProductImages/" + fileName;
                }
                catch (Exception imgEx)
                {
                    ShowAlert("Image upload failed: " + imgEx.Message + " — product will be saved without image.", false);
                    imagePath = hfImagePath.Value; // keep existing image if any
                }
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
            string imgPath       = row["ImagePath"] == DBNull.Value ? "" : row["ImagePath"].ToString();
            hfImagePath.Value    = imgPath;
            if (!string.IsNullOrEmpty(imgPath))
            {
                imgSaved.ImageUrl = "~/" + imgPath;
                imgSaved.Visible  = true;
                ClientScript.RegisterStartupScript(GetType(), "hidePlaceholder",
                    "var ph=document.getElementById('imgPlaceholder'); if(ph) ph.style.display='none';" +
                    "var ip=document.getElementById('imgPreview'); if(ip) ip.style.display='none';", true);
            }
            else
            {
                imgSaved.Visible = false;
            }

            ddlProductType.SelectedValue = row["ProductType"].ToString();
            // Restore output UOM
            if (row["OutputUOMID"] != DBNull.Value)
            {
                string uomIdStr = Convert.ToInt32(Convert.ToInt64(row["OutputUOMID"])).ToString();
                TrySetValue(ddlOutputUOM, uomIdStr);
            }
            // Production UOM is always Batches for all product types
            // No need to restore ddlProdUOM — static "Batches" label always shows
            string prodType = row["ProductType"].ToString();

            bool isActive = Convert.ToBoolean(row["IsActive"]);
            btnToggleActive.Text    = isActive ? "Deactivate" : "Activate";
            btnToggleActive.Visible = true;
        }

        private void ClearForm()
        {
            hfProductID.Value    = "0";
            hfImagePath.Value    = "";
            if (imgSaved != null) imgSaved.Visible = false;
            txtCode.Text         = "";
            txtName.Text         = "";
            txtHSN.Text          = "";
            txtGSTRate.Text      = "";
            txtBatchSize.Text    = "";
            ddlProductType.SelectedIndex = 0;
            if (ddlOutputUOM.Items.Count > 0) ddlOutputUOM.SelectedIndex = 0;
            if (ddlProdUOM.Items.Count > 0) ddlProdUOM.SelectedIndex = 0;
            SetProdUOMVisibility("Core"); // default to Core on clear
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

            LoadIngUOM(false); // don't restore from form — values set by LoadSelectedProduct
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
