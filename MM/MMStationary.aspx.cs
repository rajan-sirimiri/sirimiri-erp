using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using MMApp.DAL;

namespace MMApp
{
    public partial class MMStationary : Page
    {
        protected Label       lblNavUser;
        protected Label       lblFormTitle;
        protected Label       lblAlert;
        protected Label       lblCount;
        protected HiddenField hfStationaryID;
        protected Panel       pnlAlert;
        protected Panel       pnlEmpty;
        protected TextBox     txtCode;
        protected TextBox     txtName;
        protected TextBox     txtDescription;
        protected TextBox     txtHSN;
        protected TextBox     txtGSTRate;
        protected TextBox     txtReorder;
        protected DropDownList ddlUOM;
        protected DropDownList ddlConsumptionMode;
        protected Button      btnSave;
        protected Button      btnClear;
        protected Button      btnToggleActive;
        protected Repeater    rptItems;

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

            // Module access check
            string __role = Session["MM_Role"]?.ToString() ?? "";
            if (!MMDatabaseHelper.RoleHasModuleAccess(__role, "MM", "MM_ST_MASTER"))
            { Response.Redirect("MMHome.aspx"); return; }
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
            DataTable dt = MMDatabaseHelper.GetAllStationaries();
            if (dt.Rows.Count > 0)
            {
                rptItems.DataSource = dt;
                rptItems.DataBind();
                pnlEmpty.Visible = false;
                lblCount.Text = dt.Rows.Count + " record" + (dt.Rows.Count == 1 ? "" : "s");
            }
            else
            {
                rptItems.DataSource = null;
                rptItems.DataBind();
                pnlEmpty.Visible = true;
                lblCount.Text = "0 records";
            }
        }

        private void SetFormMode(bool isEdit)
        {
            lblFormTitle.Text       = isEdit ? "Edit Stationary" : "New Stationary";
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
            if (string.IsNullOrEmpty(name)) { ShowAlert("Stationary Name is required.", false); return; }
            if (ddlUOM.SelectedValue == "0") { ShowAlert("Please select a Unit of Measure.", false); return; }

            int     matId   = Convert.ToInt32(hfStationaryID.Value);
            int     uomId   = Convert.ToInt32(ddlUOM.SelectedValue);
            decimal reorder = 0;
            decimal.TryParse(txtReorder.Text.Trim(), out reorder);
            string  desc    = txtDescription.Text.Trim();
            string  hsn     = txtHSN.Text.Trim();
            decimal? gstRate = null;
            decimal gstParsed;
            if (decimal.TryParse(txtGSTRate.Text.Trim(), out gstParsed)) gstRate = gstParsed;
            string consumptionMode = ddlConsumptionMode != null
                ? ddlConsumptionMode.SelectedValue : "AT_ISSUE";

            try
            {
                if (matId == 0)
                {
                    MMDatabaseHelper.AddStationary(name, desc, hsn, gstRate, uomId, reorder);
                    DataTable allST = MMDatabaseHelper.GetAllStationaries();
                    foreach (DataRow rr in allST.Rows)
                    {
                        if (rr["StationaryName"].ToString() == name)
                        {
                            MMDatabaseHelper.SetConsumptionMode("ST", Convert.ToInt32(rr["StationaryID"]), consumptionMode);
                            break;
                        }
                    }
                    ShowAlert("Stationary '" + name + "' added successfully.", true);
                    ClearForm();
                }
                else
                {
                    MMDatabaseHelper.UpdateStationary(matId, txtCode.Text.Trim(), name, desc, hsn, gstRate, uomId, reorder);
                    MMDatabaseHelper.SetConsumptionMode("ST", matId, consumptionMode);
                    ShowAlert("Stationary '" + name + "' updated successfully.", true);
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
            int matId = Convert.ToInt32(hfStationaryID.Value);
            if (matId == 0) return;
            DataRow dr = MMDatabaseHelper.GetStationaryById(matId);
            if (dr == null) return;
            bool active = Convert.ToBoolean(dr["IsActive"]);
            MMDatabaseHelper.ToggleStationaryActive(matId, !active);
            ShowAlert("Stationary " + (active ? "deactivated" : "activated") + " successfully.", true);
            ClearForm();
            LoadMaterials();
        }

        protected void rptItems_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Edit")
            {
                int matId = Convert.ToInt32(e.CommandArgument);
                DataRow dr = MMDatabaseHelper.GetStationaryById(matId);
                if (dr == null) return;
                hfStationaryID.Value         = matId.ToString();
                txtCode.Text          = dr["StationaryCode"].ToString();
                txtName.Text          = dr["StationaryName"].ToString();
                txtDescription.Text   = dr["Description"].ToString();
                txtHSN.Text           = dr["HSNCode"]     == DBNull.Value ? "" : dr["HSNCode"].ToString();
                txtGSTRate.Text       = dr["GSTRate"]     == DBNull.Value ? "" : dr["GSTRate"].ToString();
                txtReorder.Text       = dr["ReorderLevel"].ToString();
                ddlUOM.SelectedValue  = dr["UOMID"].ToString();
                if (ddlConsumptionMode != null)
                {
                    string mode = MMDatabaseHelper.GetConsumptionMode("ST", matId);
                    try { ddlConsumptionMode.SelectedValue = mode; } catch { ddlConsumptionMode.SelectedIndex = 0; }
                }
                bool isActive         = Convert.ToBoolean(dr["IsActive"]);
                btnToggleActive.Text  = isActive ? "Deactivate" : "Activate";
                SetFormMode(true);
                pnlAlert.Visible      = false;
                LoadOpeningStock(matId);
                lblOSMaterialName.Text = dr["StationaryName"].ToString();
            }
        }

        // ── OPENING STOCK ────────────────────────────────────────────────────
        private void LoadOpeningStock(int matId)
        {
            DataRow dr = MMDatabaseHelper.GetOpeningStock("ST", matId);
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
            int matId = Convert.ToInt32(hfStationaryID.Value);
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
                MMDatabaseHelper.SaveOpeningStock("ST", matId, qty, rate, asOfDate, txtOSRemarks.Text.Trim(), userId);
                ShowAlert("Opening stock saved successfully.", true);
                LoadOpeningStock(matId);
            }
            catch (Exception ex) { ShowAlert("Error saving opening stock: " + ex.Message, false); }
        }

        private void ClearForm()
        {
            hfStationaryID.Value        = "0";
            txtCode.Text         = txtName.Text = txtDescription.Text = txtReorder.Text = "";
            txtHSN.Text          = txtGSTRate.Text = "";
            ddlUOM.SelectedIndex = 0;
            if (ddlConsumptionMode != null)
                try { ddlConsumptionMode.SelectedValue = "AT_ISSUE"; } catch { ddlConsumptionMode.SelectedIndex = 0; }
            SetFormMode(false);
        }
    }
}
