using System;
using System.Globalization;
using System.Web.UI;
using Sirimiri.FIN.DAL;

namespace Sirimiri.FIN
{
    public partial class FINPartyOpeningBalance : Page
    {
        private readonly FINDatabaseHelper _db = new FINDatabaseHelper();

        protected void Page_Load(object sender, EventArgs e)
        {
            // Access control
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/FIN/FINLogin.aspx");
                return;
            }

            bool isAdmin = Session["IsAdmin"] != null && Convert.ToBoolean(Session["IsAdmin"]);
            if (!isAdmin)
            {
                pnlMain.Visible = false;
                pnlDenied.Visible = true;
                return;
            }

            if (!IsPostBack)
            {
                LoadFYDropdown();
                LoadPartyDropdown();
                UpdateAsOfDate();
                LoadCurrentAndAudit();
            }
        }

        // -------------------------------------------------------------
        // Dropdown loaders
        // -------------------------------------------------------------
        private void LoadFYDropdown()
        {
            ddlFY.Items.Clear();
            int currentFYStartYear = DateTime.Today.Month >= 4
                ? DateTime.Today.Year
                : DateTime.Today.Year - 1;

            for (int offset = -2; offset <= 1; offset++)
            {
                int y = currentFYStartYear + offset;
                string fy = y + "-" + ((y + 1) % 100).ToString("D2");
                ddlFY.Items.Add(fy);
            }
            ddlFY.SelectedValue = currentFYStartYear + "-" + ((currentFYStartYear + 1) % 100).ToString("D2");
        }

        private void LoadPartyDropdown()
        {
            string partyType = rblPartyType.SelectedValue;
            var parties = _db.ListAllParties(partyType);

            ddlParty.Items.Clear();
            ddlParty.Items.Add(new System.Web.UI.WebControls.ListItem("-- Select --", ""));
            foreach (var p in parties)
            {
                string text = p.Name + (p.IsActive ? "" : " (inactive)");
                ddlParty.Items.Add(new System.Web.UI.WebControls.ListItem(text, p.PartyID.ToString()));
            }
        }

        private void UpdateAsOfDate()
        {
            // FY "2026-27" → AsOfDate = 01-Apr-2026
            string fy = ddlFY.SelectedValue;
            if (string.IsNullOrEmpty(fy)) { lblAsOfDate.Text = ""; return; }
            int startYear = int.Parse(fy.Substring(0, 4));
            DateTime asOf = new DateTime(startYear, 4, 1);
            lblAsOfDate.Text = asOf.ToString("dd-MMM-yyyy");
        }

        // -------------------------------------------------------------
        // Events
        // -------------------------------------------------------------
        protected void rblPartyType_Changed(object sender, EventArgs e)
        {
            LoadPartyDropdown();
            ClearCurrentDisplay();
        }

        protected void ddlParty_Changed(object sender, EventArgs e)
        {
            LoadCurrentAndAudit();
        }

        protected void ddlFY_Changed(object sender, EventArgs e)
        {
            UpdateAsOfDate();
            LoadCurrentAndAudit();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            lblMsg.Text = "";
            lblMsg.CssClass = "err";

            if (string.IsNullOrEmpty(ddlParty.SelectedValue))
            {
                lblMsg.Text = "Select a party first.";
                return;
            }

            decimal amount;
            if (!decimal.TryParse(txtAmount.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out amount)
                || amount < 0)
            {
                lblMsg.Text = "Enter a valid non-negative amount.";
                return;
            }

            string partyType = rblPartyType.SelectedValue;
            int partyID = int.Parse(ddlParty.SelectedValue);
            string fy = ddlFY.SelectedValue;
            int startYear = int.Parse(fy.Substring(0, 4));
            DateTime asOf = new DateTime(startYear, 4, 1);
            string drcr = rblDrCr.SelectedValue;
            string reason = txtReason.Text.Trim();
            string user = Session["UserID"].ToString();

            // Is this an update? (row already exists)
            var existing = _db.GetOpeningBalance(partyType, partyID, fy);
            if (existing != null && string.IsNullOrEmpty(reason))
            {
                lblMsg.Text = "Reason is required when editing an existing opening balance.";
                return;
            }

            try
            {
                _db.SaveOpeningBalance(partyType, partyID, fy, asOf, amount, drcr, reason, user);
                lblMsg.CssClass = "ok";
                lblMsg.Text = "Opening balance saved.";
                LoadCurrentAndAudit();
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Save failed: " + ex.Message;
            }
        }

        // -------------------------------------------------------------
        // Display refresh
        // -------------------------------------------------------------
        private void ClearCurrentDisplay()
        {
            pnlCurrent.Visible = false;
            pnlNone.Visible = false;
            rptAudit.Visible = false;
            pnlAuditEmpty.Visible = false;
            txtAmount.Text = "0.00";
            txtReason.Text = "";
            rblDrCr.SelectedValue = "Dr";
        }

        private void LoadCurrentAndAudit()
        {
            ClearCurrentDisplay();

            if (string.IsNullOrEmpty(ddlParty.SelectedValue)) return;

            string partyType = rblPartyType.SelectedValue;
            int partyID = int.Parse(ddlParty.SelectedValue);
            string fy = ddlFY.SelectedValue;

            var existing = _db.GetOpeningBalance(partyType, partyID, fy);
            if (existing != null)
            {
                pnlCurrent.Visible = true;
                lblCurrentAmount.Text = FormatIndian(existing.Amount) + " " + existing.DrCr;
                lblCreatedOn.Text = existing.CreatedOn.ToString("dd-MMM-yyyy HH:mm");
                lblCreatedBy.Text = existing.CreatedBy;
                if (existing.LastModifiedOn.HasValue)
                {
                    lblLastModified.Text = " | Last edited " +
                        existing.LastModifiedOn.Value.ToString("dd-MMM-yyyy HH:mm") +
                        " by " + existing.LastModifiedBy;
                }
                else
                {
                    lblLastModified.Text = "";
                }

                // Prefill edit form with current values
                txtAmount.Text = existing.Amount.ToString("0.00", CultureInfo.InvariantCulture);
                rblDrCr.SelectedValue = existing.DrCr;
                txtReason.Text = "";
            }
            else
            {
                pnlNone.Visible = true;
                lblFYEcho.Text = fy;
                txtAmount.Text = "0.00";
                rblDrCr.SelectedValue = "Dr";
                txtReason.Text = "";
            }

            // Audit
            var audit = _db.GetOpeningBalanceAudit(partyType, partyID, fy);
            if (audit.Count == 0)
            {
                pnlAuditEmpty.Visible = true;
            }
            else
            {
                rptAudit.Visible = true;
                rptAudit.DataSource = audit;
                rptAudit.DataBind();
            }
        }

        // -------------------------------------------------------------
        // Formatting helpers (used by Repeater templates)
        // -------------------------------------------------------------
        public string FormatOld(object row)
        {
            var a = (FINDatabaseHelper.PartyOpeningAudit)row;
            if (!a.OldAmount.HasValue) return "—";
            return FormatIndian(a.OldAmount.Value) + " " + a.OldDrCr;
        }

        public string FormatNew(object row)
        {
            var a = (FINDatabaseHelper.PartyOpeningAudit)row;
            return FormatIndian(a.NewAmount) + " " + a.NewDrCr;
        }

        public static string FormatIndian(decimal amount)
        {
            // Indian grouping: 1,05,432.50
            var ci = new CultureInfo("en-IN");
            return amount.ToString("N2", ci);
        }
    }
}
