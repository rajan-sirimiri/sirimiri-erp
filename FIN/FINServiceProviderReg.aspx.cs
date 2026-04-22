using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using FINApp.DAL;

namespace FINApp
{
    /// <summary>
    /// Service Provider registration — vendors that supply SERVICES rather than materials.
    /// Stored on mm_suppliers with PartyType='SERVICE' so the code generator,
    /// Zoho vendor sync and party-ledger infrastructure can be reused.
    /// These parties never appear on GRN dropdowns (all GRN queries filter PartyType='SUPPLIER').
    /// Their transactions flow through Journal Entries (FINJournal) instead.
    /// </summary>
    public partial class FINServiceProviderReg : System.Web.UI.Page
    {
        // Nav + alerts
        protected Label lblNavUser;
        protected Label lblFormTitle;
        protected Label lblAlert;
        protected Label lblCount;
        protected HtmlGenericControl alertBox;
        protected Panel pnlAlert, pnlEmpty;

        // Form state
        protected HiddenField hfProviderID;

        // Form fields
        protected TextBox txtCode, txtName, txtContact, txtPhone, txtEmail, txtGST, txtPAN;
        protected TextBox txtAddress, txtCity, txtState, txtPinCode, txtOtherCategory;
        protected DropDownList ddlCategory;

        // Buttons
        protected Button btnSave, btnClear, btnToggleActive;

        // List
        protected Repeater rptProviders;

        // ── Role helpers (reuse FIN conventions) ──
        private string UserRole   => Session["FIN_Role"]?.ToString() ?? "";
        private bool   IsFinance  => FINConsignments.IsFinanceRole(UserRole);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["FIN_UserID"] == null)
            {
                Response.Redirect("FINLogin.aspx?returnUrl=" + Server.UrlEncode(Request.RawUrl));
                return;
            }

            if (!IsFinance)
            {
                // Finance / Super roles only — no silent fallback
                Response.Redirect("FINHome.aspx");
                return;
            }

            if (lblNavUser != null)
                lblNavUser.Text = Session["FIN_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
            {
                LoadProviders();
                SetFormMode(false);
            }
        }

        // ══════════════════════════════════════════════════════════════
        // List
        // ══════════════════════════════════════════════════════════════

        private void LoadProviders()
        {
            DataTable dt = FINDatabaseHelper.GetAllServiceProviders();
            if (dt.Rows.Count > 0)
            {
                rptProviders.DataSource = dt;
                rptProviders.DataBind();
                pnlEmpty.Visible = false;
                lblCount.Text = dt.Rows.Count + " record" + (dt.Rows.Count == 1 ? "" : "s");
            }
            else
            {
                rptProviders.DataSource = null;
                rptProviders.DataBind();
                pnlEmpty.Visible = true;
                lblCount.Text = "0 records";
            }
        }

        // ══════════════════════════════════════════════════════════════
        // Form helpers
        // ══════════════════════════════════════════════════════════════

        private void SetFormMode(bool isEdit)
        {
            lblFormTitle.Text = isEdit ? "Edit Service Provider" : "New Service Provider";
            btnToggleActive.Visible = isEdit;
        }

        private void ShowAlert(string message, bool success)
        {
            pnlAlert.Visible = true;
            lblAlert.Text = message;
            alertBox.Attributes["class"] = "alert " + (success ? "alert-success" : "alert-danger");
        }

        private void ClearForm()
        {
            hfProviderID.Value = "0";
            txtCode.Text = "";
            txtName.Text = "";
            txtContact.Text = "";
            txtPhone.Text = "";
            txtEmail.Text = "";
            txtGST.Text = "";
            txtPAN.Text = "";
            txtAddress.Text = "";
            txtCity.Text = "";
            txtState.Text = "";
            txtPinCode.Text = "";
            ddlCategory.SelectedValue = "";
            txtOtherCategory.Text = "";
            SetFormMode(false);
        }

        /// <summary>Resolve the category value to save. If "Other" is picked, use the free-text field.</summary>
        private string ResolveCategory()
        {
            string pick = ddlCategory.SelectedValue ?? "";
            if (pick == "Other")
            {
                string custom = txtOtherCategory.Text.Trim();
                return string.IsNullOrEmpty(custom) ? "Other" : custom;
            }
            return pick;
        }

        // ══════════════════════════════════════════════════════════════
        // Save / clear / toggle
        // ══════════════════════════════════════════════════════════════

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string category = ResolveCategory();

            if (string.IsNullOrEmpty(name))
            {
                ShowAlert("Name is required.", false);
                return;
            }
            if (string.IsNullOrEmpty(category))
            {
                ShowAlert("Service Category is required.", false);
                return;
            }

            int providerId = Convert.ToInt32(hfProviderID.Value);

            try
            {
                if (providerId == 0)
                {
                    FINDatabaseHelper.AddServiceProvider(
                        name,
                        txtContact.Text.Trim(), txtPhone.Text.Trim(), txtEmail.Text.Trim(),
                        txtGST.Text.Trim(), txtPAN.Text.Trim(),
                        txtAddress.Text.Trim(), txtCity.Text.Trim(),
                        txtState.Text.Trim(), txtPinCode.Text.Trim(),
                        category);
                    ShowAlert("Service Provider '" + name + "' added.", true);
                }
                else
                {
                    FINDatabaseHelper.UpdateServiceProvider(
                        providerId, name,
                        txtContact.Text.Trim(), txtPhone.Text.Trim(), txtEmail.Text.Trim(),
                        txtGST.Text.Trim(), txtPAN.Text.Trim(),
                        txtAddress.Text.Trim(), txtCity.Text.Trim(),
                        txtState.Text.Trim(), txtPinCode.Text.Trim(),
                        category);
                    ShowAlert("Service Provider '" + name + "' updated.", true);
                }

                ClearForm();
                LoadProviders();
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message, false);
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            pnlAlert.Visible = false;
        }

        protected void btnToggleActive_Click(object sender, EventArgs e)
        {
            int providerId = Convert.ToInt32(hfProviderID.Value);
            if (providerId == 0) return;

            DataRow dr = FINDatabaseHelper.GetServiceProviderById(providerId);
            if (dr == null) return;

            bool currentlyActive = Convert.ToBoolean(dr["IsActive"]);
            FINDatabaseHelper.ToggleServiceProviderActive(providerId, !currentlyActive);
            ShowAlert("Service Provider " + (currentlyActive ? "deactivated" : "activated") + ".", true);
            ClearForm();
            LoadProviders();
        }

        // ══════════════════════════════════════════════════════════════
        // Edit from list
        // ══════════════════════════════════════════════════════════════

        protected void rptProviders_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Edit")
            {
                int providerId = Convert.ToInt32(e.CommandArgument);
                DataRow dr = FINDatabaseHelper.GetServiceProviderById(providerId);
                if (dr == null) return;

                hfProviderID.Value = providerId.ToString();
                txtCode.Text     = dr["SupplierCode"].ToString();
                txtName.Text     = dr["SupplierName"].ToString();
                txtContact.Text  = dr["ContactPerson"] == DBNull.Value ? "" : dr["ContactPerson"].ToString();
                txtPhone.Text    = dr["Phone"]         == DBNull.Value ? "" : dr["Phone"].ToString();
                txtEmail.Text    = dr["Email"]         == DBNull.Value ? "" : dr["Email"].ToString();
                txtGST.Text      = dr["GSTNo"]         == DBNull.Value ? "" : dr["GSTNo"].ToString();
                txtPAN.Text      = dr["PAN"]           == DBNull.Value ? "" : dr["PAN"].ToString();
                txtAddress.Text  = dr["Address"]       == DBNull.Value ? "" : dr["Address"].ToString();
                txtCity.Text     = dr["City"]          == DBNull.Value ? "" : dr["City"].ToString();
                txtState.Text    = dr["State"]         == DBNull.Value ? "" : dr["State"].ToString();
                txtPinCode.Text  = dr["PinCode"]       == DBNull.Value ? "" : dr["PinCode"].ToString();

                // Set category; if not in the fixed list, fall back to "Other" + free-text
                string cat = dr["ServiceCategory"] == DBNull.Value ? "" : dr["ServiceCategory"].ToString();
                bool inFixedList = false;
                foreach (ListItem li in ddlCategory.Items)
                {
                    if (li.Value == cat) { inFixedList = true; break; }
                }
                if (inFixedList)
                {
                    ddlCategory.SelectedValue = cat;
                    txtOtherCategory.Text = "";
                }
                else if (!string.IsNullOrEmpty(cat))
                {
                    ddlCategory.SelectedValue = "Other";
                    txtOtherCategory.Text = cat;
                }
                else
                {
                    ddlCategory.SelectedValue = "";
                    txtOtherCategory.Text = "";
                }

                bool isActive = Convert.ToBoolean(dr["IsActive"]);
                btnToggleActive.Text = isActive ? "Deactivate" : "Activate";
                SetFormMode(true);
                pnlAlert.Visible = false;

                // Force the JS toggle to match the loaded category
                ClientScript.RegisterStartupScript(GetType(), "toggleOtherCat",
                    "if (window.toggleOtherCat) toggleOtherCat();", true);
            }
        }
    }
}
