using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using MMApp.DAL;

namespace MMApp
{
    public partial class MMUOMMaster : Page
    {
        protected Label lblNavUser;
        protected Label lblFormTitle;
        protected Label lblAlert;
        protected Label lblCount;
        protected HiddenField hfUOMID;
        protected Panel pnlAlert;
        protected Panel pnlEmpty;
        protected TextBox txtName;
        protected TextBox txtAbbr;
        protected Button btnSave;
        protected Button btnClear;
        protected Button btnToggleActive;
        protected Repeater rptUOM;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["MM_UserID"] == null) { Response.Redirect("MMLogin.aspx"); return; }

            // Module access check
            string __role = Session["MM_Role"]?.ToString() ?? "";
            if (!MMDatabaseHelper.RoleHasModuleAccess(__role, "MM", "MM_UOM"))
            { Response.Redirect("MMHome.aspx"); return; }
            lblNavUser.Text = Session["MM_FullName"]?.ToString() ?? "";
            if (!IsPostBack) { LoadUOM(); SetFormMode(false); }
        }

        private void LoadUOM()
        {
            DataTable dt = MMDatabaseHelper.GetAllUOM();
            if (dt.Rows.Count > 0)
            {
                rptUOM.DataSource = dt;
                rptUOM.DataBind();
                pnlEmpty.Visible = false;
                lblCount.Text = dt.Rows.Count + " record" + (dt.Rows.Count == 1 ? "" : "s");
            }
            else
            {
                rptUOM.DataSource = null;
                rptUOM.DataBind();
                pnlEmpty.Visible = true;
                lblCount.Text = "0 records";
            }
        }

        private void SetFormMode(bool isEdit)
        {
            lblFormTitle.Text = isEdit ? "Edit UOM" : "New UOM";
            btnToggleActive.Visible = isEdit;
        }

        private void ShowAlert(string message, bool success)
        {
            pnlAlert.Visible = true;
            lblAlert.Text = message;
            pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger");
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string abbr = txtAbbr.Text.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(abbr))
            {
                ShowAlert("UOM Name and Abbreviation are required.", false);
                return;
            }

            int uomId = Convert.ToInt32(hfUOMID.Value);

            try
            {
                if (uomId == 0)
                {
                    MMDatabaseHelper.AddUOM(name, abbr);
                    ShowAlert("UOM '" + name + " (" + abbr + ")' added successfully.", true);
                }
                else
                {
                    MMDatabaseHelper.UpdateUOM(uomId, name, abbr);
                    ShowAlert("UOM '" + name + " (" + abbr + ")' updated successfully.", true);
                }
                ClearForm();
                LoadUOM();
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        protected void btnClear_Click(object sender, EventArgs e) { ClearForm(); pnlAlert.Visible = false; }

        protected void btnToggleActive_Click(object sender, EventArgs e)
        {
            int uomId = Convert.ToInt32(hfUOMID.Value);
            if (uomId == 0) return;
            DataRow dr = MMDatabaseHelper.GetUOMById(uomId);
            if (dr == null) return;
            bool active = Convert.ToBoolean(dr["IsActive"]);
            MMDatabaseHelper.ToggleUOMActive(uomId, !active);
            ShowAlert("UOM " + (active ? "deactivated" : "activated") + " successfully.", true);
            ClearForm(); LoadUOM();
        }

        protected void rptUOM_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Edit")
            {
                int uomId = Convert.ToInt32(e.CommandArgument);
                DataRow dr = MMDatabaseHelper.GetUOMById(uomId);
                if (dr == null) return;
                hfUOMID.Value  = uomId.ToString();
                txtName.Text   = dr["UOMName"].ToString();
                txtAbbr.Text   = dr["Abbreviation"].ToString();
                bool isActive  = Convert.ToBoolean(dr["IsActive"]);
                btnToggleActive.Text = isActive ? "Deactivate" : "Activate";
                SetFormMode(true);
                pnlAlert.Visible = false;
            }
        }

        private void ClearForm()
        {
            hfUOMID.Value = "0";
            txtName.Text = txtAbbr.Text = "";
            SetFormMode(false);
        }
    }
}
