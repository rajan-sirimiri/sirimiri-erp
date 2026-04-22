using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using FINApp.DAL;

namespace FINApp
{
    /// <summary>
    /// Service Provider registration — vendors that supply SERVICES rather than materials.
    /// Stored on mm_suppliers with PartyType='SERVICE'. Each provider can offer
    /// MULTIPLE services from the fin_services catalog (many-to-many via
    /// fin_serviceprovider_services junction table).
    ///
    /// The legacy mm_suppliers.ServiceCategory column is repurposed as a
    /// free-text Notes field in this UI.
    /// </summary>
    public partial class FINServiceProviderReg : System.Web.UI.Page
    {
        // Nav + alerts
        protected Label lblNavUser, lblFormTitle, lblAlert, lblCount;
        protected HtmlGenericControl alertBox;
        protected Panel pnlAlert, pnlEmpty, pnlNoServices;

        // Form state
        protected HiddenField hfProviderID;

        // Form fields
        protected TextBox txtCode, txtName, txtContact, txtPhone, txtEmail, txtGST, txtPAN;
        protected TextBox txtAddress, txtCity, txtState, txtPinCode, txtNotes;

        // Services chip-picker — rendered manually via Literal
        protected Literal litServices;
        // The set of currently-checked service IDs. Tracked between postbacks via ViewState
        // because Request.Form only tells us what's checked right now — we need to know what
        // was checked last time too (for re-render with correct state).
        private System.Collections.Generic.HashSet<int> CheckedServiceIds
        {
            get { return ViewState["_svcChecked"] as System.Collections.Generic.HashSet<int> ?? new System.Collections.Generic.HashSet<int>(); }
            set { ViewState["_svcChecked"] = value; }
        }

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
                Response.Redirect("FINHome.aspx");
                return;
            }

            if (lblNavUser != null)
                lblNavUser.Text = Session["FIN_FullName"]?.ToString() ?? "";

            // ViewState doesn't preserve CheckBoxList items populated programmatically unless
            // we re-bind on EVERY request (before event dispatch). Otherwise checkbox state
            // is lost on postback.
            LoadServicesPicker(preserveSelection: true);

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
            DataTable dt = FINDatabaseHelper.GetProvidersWithServiceSummary();
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

        /// <summary>Bound via Eval in the repeater — renders the GROUP_CONCAT'd
        /// "SVC-0001 Pest Control, SVC-0003 Security" string as colored chip pills.</summary>
        protected string RenderServiceChips(object servicesTextObj)
        {
            if (servicesTextObj == null || servicesTextObj == DBNull.Value) return "<span style='color:var(--text-dim);font-size:11px;'>no services</span>";
            string txt = servicesTextObj.ToString();
            if (string.IsNullOrEmpty(txt)) return "<span style='color:var(--text-dim);font-size:11px;'>no services</span>";

            var sb = new System.Text.StringBuilder();
            sb.Append("<div class='svc-chips'>");
            foreach (string item in txt.Split(','))
            {
                string trimmed = item.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                sb.Append("<span class='svc-chip'>").Append(Server.HtmlEncode(trimmed)).Append("</span>");
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        // ══════════════════════════════════════════════════════════════
        // Services chip-picker
        // ══════════════════════════════════════════════════════════════

        /// <summary>Render the services chip picker. Each checkbox has name="svcChk_<id>".
        /// The "checked" state comes from:
        ///   - Request.Form["svcChk_<id>"] if this is a postback (user's current state)
        ///   - CheckedServiceIds ViewState otherwise (loaded from DB on Edit)
        /// </summary>
        private void LoadServicesPicker(bool preserveSelection)
        {
            // Defensive: surface clear diagnostics if any server control is missing
            if (litServices == null)
                throw new InvalidOperationException("litServices control is null — aspx/code-behind mismatch.");
            if (pnlNoServices == null)
                throw new InvalidOperationException("pnlNoServices control is null — aspx/code-behind mismatch.");

            DataTable dt = null;
            try
            {
                dt = FINDatabaseHelper.GetActiveServices();
            }
            catch (Exception ex)
            {
                throw new Exception("GetActiveServices() failed: " + ex.Message, ex);
            }

            if (dt == null || dt.Rows.Count == 0)
            {
                pnlNoServices.Visible = true;
                litServices.Text = "";
                return;
            }
            pnlNoServices.Visible = false;

            // Decide source of truth for which IDs are checked
            HashSet<int> checkedIds;
            if (preserveSelection && IsPostBack)
            {
                // Read current form state — user may have toggled checkboxes
                checkedIds = new HashSet<int>();
                foreach (DataRow r in dt.Rows)
                {
                    if (r["ServiceID"] == null || r["ServiceID"] == DBNull.Value) continue;
                    int sid = Convert.ToInt32(r["ServiceID"]);
                    if (Request != null && Request.Form != null && Request.Form["svcChk_" + sid] != null)
                        checkedIds.Add(sid);
                }
                CheckedServiceIds = checkedIds;
            }
            else
            {
                checkedIds = CheckedServiceIds ?? new HashSet<int>();
            }

            var sb = new System.Text.StringBuilder();
            sb.Append("<ul class=\"svc-cbl\">");
            foreach (DataRow r in dt.Rows)
            {
                if (r["ServiceID"] == null || r["ServiceID"] == DBNull.Value) continue;
                int sid   = Convert.ToInt32(r["ServiceID"]);
                string code = r["ServiceCode"] == DBNull.Value ? "" : (r["ServiceCode"] ?? "").ToString();
                string name = r["ServiceName"] == DBNull.Value ? "" : (r["ServiceName"] ?? "").ToString();
                decimal gst = r["GSTRate"] == DBNull.Value ? 0m : Convert.ToDecimal(r["GSTRate"]);

                string inputId = "svcChk_" + sid;
                bool isChecked = checkedIds.Contains(sid);

                sb.Append("<li>")
                  .Append("<input type=\"checkbox\" id=\"").Append(inputId).Append("\" name=\"").Append(inputId).Append("\" value=\"1\"")
                  .Append(isChecked ? " checked=\"checked\"" : "").Append(" />")
                  .Append("<label for=\"").Append(inputId).Append("\">")
                  .Append("<span class=\"svc-code\">").Append(Server.HtmlEncode(code)).Append("</span>")
                  .Append("<span class=\"svc-name\">").Append(Server.HtmlEncode(name)).Append("</span>")
                  .Append("<span class=\"svc-meta\">\u00b7 ").Append(gst.ToString("0.##")).Append("% GST</span>")
                  .Append("</label></li>");
            }
            sb.Append("</ul>");
            litServices.Text = sb.ToString();
        }

        /// <summary>Read selected service IDs directly from the form (Request.Form)
        /// so we get the latest user toggles, not stale ViewState.</summary>
        private List<int> GetSelectedServiceIds()
        {
            var ids = new List<int>();
            DataTable dt = FINDatabaseHelper.GetActiveServices();
            foreach (DataRow r in dt.Rows)
            {
                int sid = Convert.ToInt32(r["ServiceID"]);
                if (Request.Form["svcChk_" + sid] != null) ids.Add(sid);
            }
            return ids;
        }

        /// <summary>Set the picker's checked state (used when Edit loads a provider).
        /// Stores in ViewState and re-renders.</summary>
        private void SetSelectedServiceIds(IEnumerable<int> ids)
        {
            var set = new HashSet<int>();
            foreach (int id in ids) set.Add(id);
            CheckedServiceIds = set;
            LoadServicesPicker(preserveSelection: false);
        }

        private void ClearServicePicker()
        {
            CheckedServiceIds = new HashSet<int>();
            LoadServicesPicker(preserveSelection: false);
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
            if (txtNotes != null) txtNotes.Text = "";
            ClearServicePicker();
            SetFormMode(false);
        }

        // ══════════════════════════════════════════════════════════════
        // Save / clear / toggle
        // ══════════════════════════════════════════════════════════════

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                ShowAlert("Name is required.", false);
                return;
            }

            List<int> selectedSvc = GetSelectedServiceIds();
            if (selectedSvc.Count == 0)
            {
                ShowAlert("Pick at least one service this provider offers. If the service isn't in the list, add it in the catalog.", false);
                return;
            }

            // Notes (formerly ServiceCategory column — keep as free-text)
            string notesText = txtNotes != null ? txtNotes.Text.Trim() : "";

            int providerId = Convert.ToInt32(hfProviderID.Value);

            try
            {
                if (providerId == 0)
                {
                    providerId = FINDatabaseHelper.AddServiceProvider(
                        name,
                        txtContact.Text.Trim(), txtPhone.Text.Trim(), txtEmail.Text.Trim(),
                        txtGST.Text.Trim(), txtPAN.Text.Trim(),
                        txtAddress.Text.Trim(), txtCity.Text.Trim(),
                        txtState.Text.Trim(), txtPinCode.Text.Trim(),
                        notesText);   // ← repurposed column, now carries Notes
                    FINDatabaseHelper.SaveProviderServices(providerId, selectedSvc);
                    ShowAlert("Service Provider '" + name + "' added with " + selectedSvc.Count
                        + " service" + (selectedSvc.Count == 1 ? "" : "s") + ".", true);
                }
                else
                {
                    FINDatabaseHelper.UpdateServiceProvider(
                        providerId, name,
                        txtContact.Text.Trim(), txtPhone.Text.Trim(), txtEmail.Text.Trim(),
                        txtGST.Text.Trim(), txtPAN.Text.Trim(),
                        txtAddress.Text.Trim(), txtCity.Text.Trim(),
                        txtState.Text.Trim(), txtPinCode.Text.Trim(),
                        notesText);
                    FINDatabaseHelper.SaveProviderServices(providerId, selectedSvc);
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

                // Notes (kept in ServiceCategory column)
                if (txtNotes != null)
                    txtNotes.Text = dr["ServiceCategory"] == DBNull.Value ? "" : dr["ServiceCategory"].ToString();

                // Check the services this provider currently offers
                DataTable svcDt = FINDatabaseHelper.GetServicesForProvider(providerId);
                var ids = new List<int>();
                foreach (DataRow srow in svcDt.Rows) ids.Add(Convert.ToInt32(srow["ServiceID"]));
                SetSelectedServiceIds(ids);

                bool isActive = Convert.ToBoolean(dr["IsActive"]);
                btnToggleActive.Text = isActive ? "Deactivate" : "Activate";
                SetFormMode(true);
                pnlAlert.Visible = false;
            }
        }
    }
}
