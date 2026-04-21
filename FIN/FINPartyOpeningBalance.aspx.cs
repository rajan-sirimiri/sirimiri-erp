using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using FINApp.DAL;

namespace FINApp
{
    public partial class FINPartyOpeningBalance : System.Web.UI.Page
    {
        // Controls declared in the aspx markup
        protected Label lblNavUser;
        protected Panel pnlDenied, pnlMain, pnlCurrent, pnlNone;
        protected Label lblMsg, lblAsOfDate, lblCurrentAmount, lblCreatedOn,
                        lblCreatedBy, lblLastModified, lblFYEcho;
        protected RadioButtonList rblPartyType, rblDrCr;
        protected DropDownList ddlParty, ddlFY;
        protected TextBox txtAmount, txtReason;
        protected Button btnSave;
        protected PlaceHolder phAudit;

        private static readonly CultureInfo _ci = new CultureInfo("en-IN");

        protected void Page_Load(object sender, EventArgs e)
        {
            // Auth
            if (Session["FIN_UserID"] == null)
            {
                Response.Redirect("FINLogin.aspx");
                return;
            }
            lblNavUser.Text = Session["FIN_FullName"]?.ToString() ?? "";

            // Super-role gate
            string role = Session["FIN_Role"]?.ToString() ?? "";
            if (!string.Equals(role, "Super", StringComparison.OrdinalIgnoreCase))
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

        // ── Dropdown loaders ─────────────────────────────────────────
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
            ddlFY.SelectedValue = currentFYStartYear + "-" +
                ((currentFYStartYear + 1) % 100).ToString("D2");
        }

        private void LoadPartyDropdown()
        {
            string partyType = rblPartyType.SelectedValue;
            DataTable dt = FINDatabaseHelper.ListAllParties(partyType);

            ddlParty.Items.Clear();
            ddlParty.Items.Add(new ListItem("-- Select --", ""));
            foreach (DataRow r in dt.Rows)
            {
                string name = r["Name"].ToString();
                bool isActive = r["IsActive"] != DBNull.Value && Convert.ToBoolean(r["IsActive"]);
                string text = name + (isActive ? "" : " (inactive)");
                ddlParty.Items.Add(new ListItem(text, r["PartyID"].ToString()));
            }
        }

        private void UpdateAsOfDate()
        {
            string fy = ddlFY.SelectedValue;
            if (string.IsNullOrEmpty(fy)) { lblAsOfDate.Text = ""; return; }
            int startYear = int.Parse(fy.Substring(0, 4));
            lblAsOfDate.Text = new DateTime(startYear, 4, 1).ToString("dd-MMM-yyyy");
        }

        // ── Events ───────────────────────────────────────────────────
        protected void rblPartyType_Changed(object sender, EventArgs e)
        {
            LoadPartyDropdown();
            ClearDisplay();
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
            if (!decimal.TryParse(txtAmount.Text.Trim(), NumberStyles.Any,
                    CultureInfo.InvariantCulture, out amount) || amount < 0)
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
            string user = Session["FIN_FullName"]?.ToString() ?? ("UID:" + Session["FIN_UserID"]);

            // Is this an update? (row already exists) → Reason required
            DataRow existing = FINDatabaseHelper.GetOpeningBalance(partyType, partyID, fy);
            if (existing != null && string.IsNullOrEmpty(reason))
            {
                lblMsg.Text = "Reason is required when editing an existing opening balance.";
                return;
            }

            try
            {
                FINDatabaseHelper.SaveOpeningBalance(partyType, partyID, fy,
                    asOf, amount, drcr, string.IsNullOrEmpty(reason) ? null : reason, user);
                lblMsg.CssClass = "ok";
                lblMsg.Text = "Opening balance saved.";
                LoadCurrentAndAudit();
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Save failed: " + ex.Message;
            }
        }

        // ── Display refresh ──────────────────────────────────────────
        private void ClearDisplay()
        {
            pnlCurrent.Visible = false;
            pnlNone.Visible = false;
            phAudit.Controls.Clear();
            txtAmount.Text = "0.00";
            txtReason.Text = "";
            rblDrCr.SelectedValue = "Dr";
        }

        private void LoadCurrentAndAudit()
        {
            ClearDisplay();

            if (string.IsNullOrEmpty(ddlParty.SelectedValue)) return;

            string partyType = rblPartyType.SelectedValue;
            int partyID = int.Parse(ddlParty.SelectedValue);
            string fy = ddlFY.SelectedValue;

            DataRow existing = FINDatabaseHelper.GetOpeningBalance(partyType, partyID, fy);
            if (existing != null)
            {
                pnlCurrent.Visible = true;
                decimal amt = Convert.ToDecimal(existing["Amount"]);
                string drcr = existing["DrCr"].ToString();
                lblCurrentAmount.Text = FormatIndian(amt) + " " + drcr;
                lblCreatedOn.Text = Convert.ToDateTime(existing["CreatedOn"])
                    .ToString("dd-MMM-yyyy HH:mm");
                lblCreatedBy.Text = existing["CreatedBy"].ToString();

                if (existing["LastModifiedOn"] != DBNull.Value)
                {
                    lblLastModified.Text = " | Last edited " +
                        Convert.ToDateTime(existing["LastModifiedOn"]).ToString("dd-MMM-yyyy HH:mm") +
                        " by " + existing["LastModifiedBy"];
                }
                else
                {
                    lblLastModified.Text = "";
                }

                // Prefill edit form
                txtAmount.Text = amt.ToString("0.00", CultureInfo.InvariantCulture);
                rblDrCr.SelectedValue = drcr;
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

            RenderAudit(partyType, partyID, fy);
        }

        private void RenderAudit(string partyType, int partyID, string fy)
        {
            DataTable dt = FINDatabaseHelper.GetOpeningBalanceAudit(partyType, partyID, fy);
            if (dt.Rows.Count == 0)
            {
                phAudit.Controls.Add(new LiteralControl("<p><em>No history yet.</em></p>"));
                return;
            }

            var sb = new StringBuilder();
            sb.Append("<table class='audit-table'>");
            sb.Append("<tr><th>Changed On</th><th>Action</th><th>Old</th><th>New</th>" +
                      "<th>By</th><th>Reason</th></tr>");
            foreach (DataRow r in dt.Rows)
            {
                string oldStr = r["OldAmount"] == DBNull.Value
                    ? "—"
                    : FormatIndian(Convert.ToDecimal(r["OldAmount"])) + " " + r["OldDrCr"];
                string newStr = FormatIndian(Convert.ToDecimal(r["NewAmount"])) +
                    " " + r["NewDrCr"];
                string reason = r["Reason"] == DBNull.Value ? "" : Server.HtmlEncode(r["Reason"].ToString());

                sb.Append("<tr>")
                  .Append("<td>").Append(Convert.ToDateTime(r["ChangedOn"]).ToString("dd-MMM-yyyy HH:mm")).Append("</td>")
                  .Append("<td>").Append(r["ActionType"]).Append("</td>")
                  .Append("<td class='amt'>").Append(oldStr).Append("</td>")
                  .Append("<td class='amt'>").Append(newStr).Append("</td>")
                  .Append("<td>").Append(Server.HtmlEncode(r["ChangedBy"].ToString())).Append("</td>")
                  .Append("<td>").Append(reason).Append("</td>")
                  .Append("</tr>");
            }
            sb.Append("</table>");
            phAudit.Controls.Add(new LiteralControl(sb.ToString()));
        }

        private static string FormatIndian(decimal amount)
        {
            return amount.ToString("N2", _ci);
        }
    }
}
