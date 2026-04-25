using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using FINApp.DAL;

namespace FINApp
{
    /// <summary>
    /// Service Catalog master-data CRUD screen. Services are the things a
    /// Service Provider (vendor) offers. Many-to-many with providers via
    /// fin_serviceprovider_services. Finance role required.
    /// </summary>
    public partial class FINServiceCatalog : System.Web.UI.Page
    {
        protected Label lblNavUser, lblFormTitle, lblAlert, lblCount;
        protected HtmlGenericControl alertBox;
        protected Panel pnlAlert, pnlEmpty;

        protected HiddenField hfServiceID;
        protected TextBox txtCode, txtName, txtDescription, txtHSN, txtGSTRate;
        protected Button btnSave, btnClear, btnToggleActive;
        protected Repeater rptServices;

        private string UserRole   => Session["FIN_Role"]?.ToString() ?? "";
        private bool   IsFinance  => FINConsignments.IsFinanceRole(UserRole);
        private int    CurrentUserID => Convert.ToInt32(Session["FIN_UserID"] ?? 0);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["FIN_UserID"] == null)
            {
                Response.Redirect("FINLogin.aspx?returnUrl=" + Server.UrlEncode(Request.RawUrl));
                return;
            }
            if (!IsFinance)
            {
                Response.Redirect("FINHome.aspx");
                return;
            }

            if (lblNavUser != null)
                lblNavUser.Text = Session["FIN_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
            {
                LoadServices();
                SetFormMode(false);
            }
        }

        // ── List ──────────────────────────────────────────────────────

        private void LoadServices()
        {
            DataTable dt = FINDatabaseHelper.GetServicesWithUsage();
            if (dt.Rows.Count > 0)
            {
                rptServices.DataSource = dt;
                rptServices.DataBind();
                pnlEmpty.Visible = false;
                lblCount.Text = dt.Rows.Count + " service" + (dt.Rows.Count == 1 ? "" : "s");
            }
            else
            {
                rptServices.DataSource = null;
                rptServices.DataBind();
                pnlEmpty.Visible = true;
                lblCount.Text = "0 services";
            }
        }

        // ── Form helpers ──────────────────────────────────────────────

        private void SetFormMode(bool isEdit)
        {
            lblFormTitle.Text = isEdit ? "Edit Service" : "New Service";
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
            hfServiceID.Value = "0";
            txtCode.Text = "";
            txtName.Text = "";
            txtDescription.Text = "";
            txtHSN.Text = "";
            txtGSTRate.Text = "18.00";
            SetFormMode(false);
        }

        // ── Save / clear / toggle ─────────────────────────────────────

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                ShowAlert("Service Name is required.", false);
                return;
            }

            decimal gst;
            if (!decimal.TryParse(txtGSTRate.Text.Trim(), out gst) || gst < 0 || gst > 100)
            {
                ShowAlert("GST Rate must be a number between 0 and 100.", false);
                return;
            }

            int serviceId = Convert.ToInt32(hfServiceID.Value);

            try
            {
                if (serviceId == 0)
                {
                    FINDatabaseHelper.AddService(name, txtDescription.Text.Trim(),
                                                 txtHSN.Text.Trim(), gst, CurrentUserID);
                    ShowAlert("Service '" + name + "' added.", true);
                }
                else
                {
                    FINDatabaseHelper.UpdateService(serviceId, name, txtDescription.Text.Trim(),
                                                    txtHSN.Text.Trim(), gst);
                    ShowAlert("Service '" + name + "' updated.", true);
                }
                ClearForm();
                LoadServices();
            }
            catch (Exception ex)
            {
                // MySQL unique-key on ServiceName will error out on duplicates
                string msg = ex.Message;
                if (msg.Contains("UQ_Service_Name") || msg.Contains("Duplicate entry"))
                    msg = "A service with this name already exists.";
                ShowAlert("Error: " + msg, false);
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            pnlAlert.Visible = false;
        }

        protected void btnToggleActive_Click(object sender, EventArgs e)
        {
            int serviceId = Convert.ToInt32(hfServiceID.Value);
            if (serviceId == 0) return;

            DataRow dr = FINDatabaseHelper.GetServiceById(serviceId);
            if (dr == null) return;

            bool currentlyActive = Convert.ToBoolean(dr["IsActive"]);

            // Block deactivation if any provider uses this service (safety)
            if (currentlyActive)
            {
                int used = FINDatabaseHelper.GetProviderCountForService(serviceId);
                if (used > 0)
                {
                    ShowAlert("Cannot deactivate — " + used + " service provider" + (used == 1 ? "" : "s")
                        + " still linked to this service. Remove links first.", false);
                    return;
                }
            }

            FINDatabaseHelper.ToggleServiceActive(serviceId, !currentlyActive);
            ShowAlert("Service " + (currentlyActive ? "deactivated" : "activated") + ".", true);
            ClearForm();
            LoadServices();
        }

        // ── Edit from list ────────────────────────────────────────────

        protected void rptServices_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Edit")
            {
                int serviceId = Convert.ToInt32(e.CommandArgument);
                DataRow dr = FINDatabaseHelper.GetServiceById(serviceId);
                if (dr == null) return;

                hfServiceID.Value   = serviceId.ToString();
                txtCode.Text        = dr["ServiceCode"].ToString();
                txtName.Text        = dr["ServiceName"].ToString();
                txtDescription.Text = dr["Description"] == DBNull.Value ? "" : dr["Description"].ToString();
                txtHSN.Text         = dr["HSNCode"]     == DBNull.Value ? "" : dr["HSNCode"].ToString();
                txtGSTRate.Text     = dr["GSTRate"]     == DBNull.Value ? "18.00" : Convert.ToDecimal(dr["GSTRate"]).ToString("0.##");

                bool isActive = Convert.ToBoolean(dr["IsActive"]);
                btnToggleActive.Text = isActive ? "Deactivate" : "Activate";
                SetFormMode(true);
                pnlAlert.Visible = false;
            }
        }
    }
}
