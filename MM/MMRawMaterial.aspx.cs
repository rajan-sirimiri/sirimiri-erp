using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using MMApp.DAL;

namespace MMApp
{
    public partial class MMRawMaterial : Page
    {
        protected Label       lblNavUser;
        protected Label       lblFormTitle;
        protected Label       lblAlert;
        protected Label       lblCount;
        protected HiddenField hfRMID;
        protected Panel       pnlAlert;
        protected Panel       pnlEmpty;
        protected TextBox     txtCode;
        protected TextBox     txtName;
        protected TextBox     txtDescription;
        protected TextBox     txtHSN;
        protected TextBox     txtGSTRate;
        protected TextBox     txtReorder;
        protected DropDownList ddlUOM;
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
            if (!IsPostBack) { LoadUOM(); LoadMaterials(); SetFormMode(false); }
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

        private void LoadMaterials()
        {
            DataTable dt = MMDatabaseHelper.GetAllRawMaterials();
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
            lblFormTitle.Text       = isEdit ? "Edit Raw Material" : "New Raw Material";
            btnToggleActive.Visible = isEdit;
            pnlOpeningStock.Visible = isEdit;
        }

        private void ShowAlert(string message, bool success)
        {
            pnlAlert.Visible  = true;
            lblAlert.Text     = message;
            pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger");
        }

        // ── MATERIAL SAVE ────────────────────────────────────────────────────
        protected void btnSave_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name)) { ShowAlert("Material Name is required.", false); return; }
            if (ddlUOM.SelectedValue == "0") { ShowAlert("Please select a Unit of Measure.", false); return; }

            int     rmId    = Convert.ToInt32(hfRMID.Value);
            int     uomId   = Convert.ToInt32(ddlUOM.SelectedValue);
            decimal reorder = 0;
            decimal.TryParse(txtReorder.Text.Trim(), out reorder);
            string  desc    = txtDescription.Text.Trim();
            string  hsn     = txtHSN.Text.Trim();
            decimal? gstRate = null;
            decimal gstParsed;
            if (decimal.TryParse(txtGSTRate.Text.Trim(), out gstParsed)) gstRate = gstParsed;

            try
            {
                if (rmId == 0)
                {
                    MMDatabaseHelper.AddRawMaterial(name, desc, hsn, gstRate, uomId, reorder);
                    ShowAlert("Raw material '" + name + "' added successfully.", true);
                    ClearForm();
                }
                else
                {
                    MMDatabaseHelper.UpdateRawMaterial(rmId, txtCode.Text.Trim(), name, desc, hsn, gstRate, uomId, reorder);
                    ShowAlert("Raw material '" + name + "' updated successfully.", true);
                    LoadOpeningStock(rmId);
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
            int rmId = Convert.ToInt32(hfRMID.Value);
            if (rmId == 0) return;
            DataRow dr = MMDatabaseHelper.GetRawMaterialById(rmId);
            if (dr == null) return;
            bool active = Convert.ToBoolean(dr["IsActive"]);
            MMDatabaseHelper.ToggleRawMaterialActive(rmId, !active);
            ShowAlert("Material " + (active ? "deactivated" : "activated") + " successfully.", true);
            ClearForm();
            LoadMaterials();
        }

        protected void rptMaterials_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Edit")
            {
                int rmId = Convert.ToInt32(e.CommandArgument);
                DataRow dr = MMDatabaseHelper.GetRawMaterialById(rmId);
                if (dr == null) return;
                hfRMID.Value          = rmId.ToString();
                txtCode.Text          = dr["RMCode"].ToString();
                txtName.Text          = dr["RMName"].ToString();
                txtDescription.Text   = dr["Description"].ToString();
                txtHSN.Text           = dr["HSNCode"]     == DBNull.Value ? "" : dr["HSNCode"].ToString();
                txtGSTRate.Text       = dr["GSTRate"]     == DBNull.Value ? "" : dr["GSTRate"].ToString();
                txtReorder.Text       = dr["ReorderLevel"].ToString();
                ddlUOM.SelectedValue  = dr["UOMID"].ToString();
                bool isActive         = Convert.ToBoolean(dr["IsActive"]);
                btnToggleActive.Text  = isActive ? "Deactivate" : "Activate";
                SetFormMode(true);
                pnlAlert.Visible      = false;
                LoadOpeningStock(rmId);
                lblOSMaterialName.Text = dr["RMName"].ToString();
            }
        }

        // ── OPENING STOCK ────────────────────────────────────────────────────
        private void LoadOpeningStock(int rmId)
        {
            DataRow dr = MMDatabaseHelper.GetOpeningStock("RM", rmId);
            if (dr != null)
            {
                txtOSQty.Text     = dr["Quantity"].ToString();
                txtOSRate.Text    = dr["Rate"].ToString();
                txtOSDate.Text    = Convert.ToDateTime(dr["AsOfDate"]).ToString("yyyy-MM-dd");
                txtOSRemarks.Text = dr["Remarks"] == DBNull.Value ? "" : dr["Remarks"].ToString();
                decimal val       = dr["Value"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["Value"]);
                lblOSValue.Text   = "₹ " + val.ToString("N2");
                lblOSLastSaved.Text = "Last saved: " + Convert.ToDateTime(dr["AsOfDate"]).ToString("dd-MMM-yyyy");
            }
            else
            {
                txtOSQty.Text      = "";
                txtOSRate.Text     = "";
                txtOSDate.Text     = DateTime.Today.ToString("yyyy-MM-dd");
                txtOSRemarks.Text  = "";
                lblOSValue.Text    = "₹ 0.00";
                lblOSLastSaved.Text = "Not yet recorded";
            }
        }

        protected void btnSaveOS_Click(object sender, EventArgs e)
        {
            int rmId = Convert.ToInt32(hfRMID.Value);
            if (rmId == 0) { ShowAlert("Please select a material first.", false); return; }

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
                MMDatabaseHelper.SaveOpeningStock("RM", rmId, qty, rate, asOfDate, txtOSRemarks.Text.Trim(), userId);
                ShowAlert("Opening stock saved successfully.", true);
                LoadOpeningStock(rmId);
            }
            catch (Exception ex) { ShowAlert("Error saving opening stock: " + ex.Message, false); }
        }

        private void ClearForm()
        {
            hfRMID.Value        = "0";
            txtCode.Text        = txtName.Text = txtDescription.Text = txtReorder.Text = "";
            txtHSN.Text         = txtGSTRate.Text = "";
            ddlUOM.SelectedIndex = 0;
            SetFormMode(false);
        }
    }
}
