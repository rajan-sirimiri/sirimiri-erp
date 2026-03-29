using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using MMApp.DAL;

namespace MMApp
{
    public partial class MMPackingMaterial : Page
    {
        protected Label       lblNavUser;
        protected Label       lblFormTitle;
        protected Label       lblAlert;
        protected Label       lblCount;
        protected HiddenField hfPMID;
        protected Panel       pnlAlert;
        protected Panel       pnlEmpty;
        protected TextBox     txtCode;
        protected TextBox     txtName;
        protected TextBox     txtDescription;
        protected TextBox     txtHSN;
        protected TextBox     txtGSTRate;
        protected TextBox     txtReorder;
        protected DropDownList ddlUOM;
        protected DropDownList ddlCategory;
        protected TextBox     txtNewCategory;
        protected Button      btnAddCategory;
        protected Button      btnSave;
        protected Button      btnClear;
        protected Button      btnToggleActive;
        protected Repeater    rptMaterials;

        // ── Opening Stock controls ──
        protected Panel   pnlOpeningStock;
        protected Label   lblOSMaterialName;
        protected TextBox txtOSQty;
        protected TextBox txtOSRate;
        protected TextBox txtOSDate;
        protected TextBox txtOSRemarks;
        protected Label   lblOSValue;
        protected Label   lblOSLastSaved;
        protected Button  btnSaveOS;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["MM_UserID"] == null) { Response.Redirect("MMLogin.aspx"); return; }
            lblNavUser.Text = Session["MM_FullName"]?.ToString() ?? "";
            if (!IsPostBack) { LoadUOM(); LoadCategories(); LoadMaterials(); SetFormMode(false); }
        }

        private void LoadUOM()
        {
            DataTable dt = MMDatabaseHelper.GetActiveUOM();
            ddlUOM.DataSource     = dt;
            ddlUOM.DataTextField  = "UOMName";
            ddlUOM.DataValueField = "UOMID";
            ddlUOM.DataBind();
            ddlUOM.Items.Insert(0, new ListItem("-- Select UOM --", "0"));
        }

        private void LoadCategories()
        {
            if (ddlCategory == null) return;
            string selectedVal = ddlCategory.SelectedValue;
            var dt = MMDatabaseHelper.GetPMCategories();
            ddlCategory.Items.Clear();
            ddlCategory.Items.Add(new ListItem("-- Select Category --", ""));
            foreach (DataRow r in dt.Rows)
                ddlCategory.Items.Add(new ListItem(r["CategoryName"].ToString(), r["CategoryName"].ToString()));
            // Restore selection
            if (!string.IsNullOrEmpty(selectedVal))
                try { ddlCategory.SelectedValue = selectedVal; } catch { }
        }

        private void LoadMaterials()
        {
            DataTable dt = MMDatabaseHelper.GetAllPackingMaterials();
            if (dt.Rows.Count > 0)
            {
                rptMaterials.DataSource = dt;
                rptMaterials.DataBind();
                pnlEmpty.Visible = false;
                lblCount.Text = dt.Rows.Count + " record" + (dt.Rows.Count == 1 ? "" : "s");
            }
            else
            {
                rptMaterials.DataSource = null;
                rptMaterials.DataBind();
                pnlEmpty.Visible = true;
                lblCount.Text = "0 records";
            }
        }

        private void SetFormMode(bool isEdit)
        {
            lblFormTitle.Text       = isEdit ? "Edit Packing Material" : "New Packing Material";
            btnToggleActive.Visible = isEdit;
            pnlOpeningStock.Visible = isEdit;
        }

        private void ShowAlert(string message, bool success)
        {
            pnlAlert.Visible  = true;
            lblAlert.Text     = message;
            pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger");
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name)) { ShowAlert("Packing Material Name is required.", false); return; }
            if (ddlUOM.SelectedValue == "0") { ShowAlert("Please select a Unit of Measure.", false); return; }

            int     matId   = Convert.ToInt32(hfPMID.Value);
            int     uomId   = Convert.ToInt32(ddlUOM.SelectedValue);
            decimal reorder = 0;
            decimal.TryParse(txtReorder.Text.Trim(), out reorder);
            string  desc    = txtDescription.Text.Trim();
            string  hsn     = txtHSN.Text.Trim();
            decimal? gstRate = null;
            decimal gstParsed;
            if (decimal.TryParse(txtGSTRate.Text.Trim(), out gstParsed)) gstRate = gstParsed;
            string category = ddlCategory != null && !string.IsNullOrEmpty(ddlCategory.SelectedValue)
                ? ddlCategory.SelectedValue : null;

            try
            {
                if (matId == 0)
                {
                    MMDatabaseHelper.AddPackingMaterial(name, desc, hsn, gstRate, uomId, reorder, category);
                    ShowAlert("Packing Material '" + name + "' added successfully.", true);
                    ClearForm();
                }
                else
                {
                    MMDatabaseHelper.UpdatePackingMaterial(matId, txtCode.Text.Trim(), name, desc, hsn, gstRate, uomId, reorder, category);
                    ShowAlert("Packing Material '" + name + "' updated successfully.", true);
                    LoadOpeningStock(matId);
                }
                LoadMaterials();
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            pnlAlert.Visible = false;
        }

        protected void btnToggleActive_Click(object sender, EventArgs e)
        {
            int matId = Convert.ToInt32(hfPMID.Value);
            if (matId == 0) return;
            DataRow dr = MMDatabaseHelper.GetPackingMaterialById(matId);
            if (dr == null) return;
            bool active = Convert.ToBoolean(dr["IsActive"]);
            MMDatabaseHelper.TogglePackingMaterialActive(matId, !active);
            ShowAlert("Packing Material " + (active ? "deactivated" : "activated") + " successfully.", true);
            ClearForm();
            LoadMaterials();
        }

        protected void rptMaterials_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Edit")
            {
                int matId = Convert.ToInt32(e.CommandArgument);
                DataRow dr = MMDatabaseHelper.GetPackingMaterialById(matId);
                if (dr == null) return;
                hfPMID.Value         = matId.ToString();
                txtCode.Text          = dr["PMCode"].ToString();
                txtName.Text          = dr["PMName"].ToString();
                txtDescription.Text   = dr["Description"].ToString();
                txtHSN.Text           = dr["HSNCode"]     == DBNull.Value ? "" : dr["HSNCode"].ToString();
                txtGSTRate.Text       = dr["GSTRate"]     == DBNull.Value ? "" : dr["GSTRate"].ToString();
                txtReorder.Text       = dr["ReorderLevel"].ToString();
                ddlUOM.SelectedValue  = dr["UOMID"].ToString();
                if (ddlCategory != null)
                {
                    string cat = dr.Table.Columns.Contains("PMCategory") && dr["PMCategory"] != DBNull.Value
                        ? dr["PMCategory"].ToString() : "";
                    try { ddlCategory.SelectedValue = cat; } catch { ddlCategory.SelectedIndex = 0; }
                }
                bool isActive         = Convert.ToBoolean(dr["IsActive"]);
                btnToggleActive.Text  = isActive ? "Deactivate" : "Activate";
                SetFormMode(true);
                pnlAlert.Visible      = false;
                LoadOpeningStock(matId);
                lblOSMaterialName.Text = dr["PMName"].ToString();
            }
        }

        // ── OPENING STOCK ────────────────────────────────────────────────────
        private void LoadOpeningStock(int matId)
        {
            DataRow dr = MMDatabaseHelper.GetOpeningStock("PM", matId);
            if (dr != null)
            {
                txtOSQty.Text      = dr["Quantity"].ToString();
                txtOSRate.Text     = dr["Rate"].ToString();
                txtOSDate.Text     = Convert.ToDateTime(dr["AsOfDate"]).ToString("yyyy-MM-dd");
                txtOSRemarks.Text  = dr["Remarks"] == DBNull.Value ? "" : dr["Remarks"].ToString();
                decimal val        = dr["Value"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["Value"]);
                lblOSValue.Text    = "₹ " + val.ToString("N2");
                lblOSLastSaved.Text = "Last saved: " + Convert.ToDateTime(dr["AsOfDate"]).ToString("dd-MMM-yyyy");
            }
            else
            {
                txtOSQty.Text       = "";
                txtOSRate.Text      = "";
                txtOSDate.Text      = DateTime.Today.ToString("yyyy-MM-dd");
                txtOSRemarks.Text   = "";
                lblOSValue.Text     = "₹ 0.00";
                lblOSLastSaved.Text = "Not yet recorded";
            }
        }

        protected void btnSaveOS_Click(object sender, EventArgs e)
        {
            int matId = Convert.ToInt32(hfPMID.Value);
            if (matId == 0) { ShowAlert("Please select a material first.", false); return; }

            decimal qty = 0, rate = 0;
            if (!decimal.TryParse(txtOSQty.Text.Trim(), out qty) || qty < 0)
            { ShowAlert("Opening Stock: please enter a valid quantity.", false); return; }
            decimal.TryParse(txtOSRate.Text.Trim(), out rate);

            DateTime asOfDate = DateTime.Today;
            if (!DateTime.TryParse(txtOSDate.Text.Trim(), out asOfDate))
            { ShowAlert("Opening Stock: please enter a valid date.", false); return; }

            int userId = Convert.ToInt32(Session["MM_UserID"]);
            try
            {
                MMDatabaseHelper.SaveOpeningStock("PM", matId, qty, rate, asOfDate, txtOSRemarks.Text.Trim(), userId);
                ShowAlert("Opening stock saved successfully.", true);
                LoadOpeningStock(matId);
            }
            catch (Exception ex) { ShowAlert("Error saving opening stock: " + ex.Message, false); }
        }

        private void ClearForm()
        {
            hfPMID.Value        = "0";
            txtCode.Text         = txtName.Text = txtDescription.Text = txtReorder.Text = "";
            txtHSN.Text          = txtGSTRate.Text = "";
            ddlUOM.SelectedIndex = 0;
            if (ddlCategory != null) ddlCategory.SelectedIndex = 0;
            SetFormMode(false);
        }

        protected void btnAddCategory_Click(object sender, EventArgs e)
        {
            if (txtNewCategory == null) return;
            string name = txtNewCategory.Text.Trim();
            if (string.IsNullOrEmpty(name))
            { ShowAlert("Enter a category name.", false); return; }
            try
            {
                MMDatabaseHelper.AddPMCategory(name);
                txtNewCategory.Text = "";
                LoadCategories();
                // Auto-select the newly added category
                try { ddlCategory.SelectedValue = name; } catch { }
                ShowAlert("Category '" + name + "' added.", true);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Duplicate"))
                    ShowAlert("Category '" + name + "' already exists.", false);
                else
                    ShowAlert("Error: " + ex.Message, false);
            }
        }
    }
}
