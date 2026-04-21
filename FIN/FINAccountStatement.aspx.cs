using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using FINApp.DAL;

namespace FINApp
{
    public partial class FINAccountStatement : System.Web.UI.Page
    {
        // Controls declared in markup
        protected Label lblNavUser, lblMsg, lblPartyName, lblPartyGSTIN, lblPeriod;
        protected RadioButtonList rblPartyType;
        protected DropDownList ddlParty;
        protected TextBox txtFrom, txtTo;
        protected Button btnRefresh, btnPrint;
        protected Panel pnlStatement, pnlEmpty;
        protected PlaceHolder phBody;

        private static readonly CultureInfo _ci = new CultureInfo("en-IN");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["FIN_UserID"] == null)
            {
                Response.Redirect("FINLogin.aspx");
                return;
            }
            lblNavUser.Text = Session["FIN_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
            {
                // Default: From = 1 Apr current FY, To = today
                DateTime today = DateTime.Today;
                int fyStartYear = today.Month >= 4 ? today.Year : today.Year - 1;
                txtFrom.Text = new DateTime(fyStartYear, 4, 1).ToString("dd-MMM-yyyy");
                txtTo.Text = today.ToString("dd-MMM-yyyy");

                LoadPartyDropdown();
            }
        }

        // ── Dropdown ─────────────────────────────────────────────────
        private void LoadPartyDropdown()
        {
            string partyType = rblPartyType.SelectedValue;
            DateTime fromDate, toDate;
            if (!TryParseDates(out fromDate, out toDate)) return;

            DataTable dt = FINDatabaseHelper.ListPartiesWithActivity(partyType, fromDate, toDate);
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

        private bool TryParseDates(out DateTime fromDate, out DateTime toDate)
        {
            fromDate = DateTime.MinValue;
            toDate = DateTime.MinValue;
            if (!DateTime.TryParseExact(txtFrom.Text.Trim(), "dd-MMM-yyyy", _ci,
                    DateTimeStyles.None, out fromDate))
            {
                lblMsg.Text = "Invalid From date. Use dd-MMM-yyyy.";
                return false;
            }
            if (!DateTime.TryParseExact(txtTo.Text.Trim(), "dd-MMM-yyyy", _ci,
                    DateTimeStyles.None, out toDate))
            {
                lblMsg.Text = "Invalid To date. Use dd-MMM-yyyy.";
                return false;
            }
            if (toDate < fromDate)
            {
                lblMsg.Text = "To date must be on or after From date.";
                return false;
            }
            lblMsg.Text = "";
            return true;
        }

        // ── Events ───────────────────────────────────────────────────
        protected void rblPartyType_Changed(object sender, EventArgs e)
        {
            LoadPartyDropdown();
            pnlStatement.Visible = false;
            pnlEmpty.Visible = false;
        }

        protected void ddlParty_Changed(object sender, EventArgs e)
        {
            LoadStatement();
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadPartyDropdown();
            LoadStatement();
        }

        protected void btnPrint_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlParty.SelectedValue))
            {
                lblMsg.Text = "Select a party first.";
                return;
            }
            string url = string.Format(
                "FINAccountStatementPrint.aspx?type={0}&pid={1}&from={2}&to={3}",
                rblPartyType.SelectedValue,
                ddlParty.SelectedValue,
                Server.UrlEncode(txtFrom.Text.Trim()),
                Server.UrlEncode(txtTo.Text.Trim()));
            ClientScript.RegisterStartupScript(GetType(), "openPrint",
                "window.open('" + url + "', '_blank');", true);
        }

        // ── Render statement ─────────────────────────────────────────
        private void LoadStatement()
        {
            pnlStatement.Visible = false;
            pnlEmpty.Visible = false;
            phBody.Controls.Clear();

            if (string.IsNullOrEmpty(ddlParty.SelectedValue)) return;

            DateTime fromDate, toDate;
            if (!TryParseDates(out fromDate, out toDate)) return;

            string partyType = rblPartyType.SelectedValue;
            int partyID = int.Parse(ddlParty.SelectedValue);

            DataRow party = FINDatabaseHelper.GetParty(partyType, partyID);
            if (party == null) { lblMsg.Text = "Party not found."; return; }

            // FY derived from fromDate
            int fyStartYear = fromDate.Month >= 4 ? fromDate.Year : fromDate.Year - 1;
            string fy = fyStartYear + "-" + ((fyStartYear + 1) % 100).ToString("D2");

            DataRow opening = FINDatabaseHelper.GetOpeningBalance(partyType, partyID, fy);
            decimal openingSigned = 0m;  // positive=Dr, negative=Cr
            DateTime? openingDate = null;
            decimal openingAmt = 0m;
            string openingDrCr = "Dr";
            if (opening != null)
            {
                openingAmt = Convert.ToDecimal(opening["Amount"]);
                openingDrCr = opening["DrCr"].ToString();
                openingDate = Convert.ToDateTime(opening["AsOfDate"]);
                openingSigned = openingDrCr == "Dr" ? openingAmt : -openingAmt;
            }

            DataTable lines = FINDatabaseHelper.GetPartyStatement(partyType, partyID, fromDate, toDate);

            // Header
            lblPartyName.Text = party["Name"].ToString() + " (" + partyType + ":" + party["PartyID"] + ")";
            string gstin = party["GSTIN"] == DBNull.Value ? "" : party["GSTIN"].ToString();
            lblPartyGSTIN.Text = string.IsNullOrEmpty(gstin) ? "" : "  •  GSTIN " + gstin;
            lblPeriod.Text = fromDate.ToString("dd-MMM-yyyy") + " to " + toDate.ToString("dd-MMM-yyyy");

            // Build body
            var sb = new StringBuilder();

            // Opening row
            string openingDisplay = opening == null
                ? "0.00 (no opening set)"
                : FormatIndian(openingAmt) + " " + openingDrCr;
            sb.Append("<tr class='opening'>")
              .Append("<td>").Append(openingDate.HasValue ? openingDate.Value.ToString("dd-MMM-yyyy") : "").Append("</td>")
              .Append("<td></td><td>Opening Balance</td>")
              .Append("<td class='amt'></td><td class='amt'></td>")
              .Append("<td class='amt'>").Append(openingDisplay).Append("</td>")
              .Append("</tr>");

            // Transaction rows
            decimal running = openingSigned;
            decimal totalDebit = 0m, totalCredit = 0m;
            foreach (DataRow r in lines.Rows)
            {
                decimal debit = Convert.ToDecimal(r["Debit"]);
                decimal credit = Convert.ToDecimal(r["Credit"]);
                running += debit - credit;
                totalDebit += debit;
                totalCredit += credit;
                string bal = FormatIndian(Math.Abs(running)) + " " + (running >= 0 ? "Dr" : "Cr");

                sb.Append("<tr>")
                  .Append("<td>").Append(Convert.ToDateTime(r["TxnDate"]).ToString("dd-MMM-yyyy")).Append("</td>")
                  .Append("<td>").Append(Server.HtmlEncode(r["VoucherNo"].ToString())).Append("</td>")
                  .Append("<td>").Append(Server.HtmlEncode(r["Particulars"].ToString())).Append("</td>")
                  .Append("<td class='amt'>").Append(debit == 0 ? "" : FormatIndian(debit)).Append("</td>")
                  .Append("<td class='amt'>").Append(credit == 0 ? "" : FormatIndian(credit)).Append("</td>")
                  .Append("<td class='amt'>").Append(bal).Append("</td>")
                  .Append("</tr>");
            }

            // Totals + closing
            string closingDisplay = FormatIndian(Math.Abs(running)) + " " + (running >= 0 ? "Dr" : "Cr");
            sb.Append("<tr class='totals'>")
              .Append("<td></td><td></td><td>Period Totals</td>")
              .Append("<td class='amt'>").Append(FormatIndian(totalDebit)).Append("</td>")
              .Append("<td class='amt'>").Append(FormatIndian(totalCredit)).Append("</td>")
              .Append("<td class='amt'></td></tr>");
            sb.Append("<tr class='closing'>")
              .Append("<td></td><td></td><td>Closing Balance</td>")
              .Append("<td class='amt'></td><td class='amt'></td>")
              .Append("<td class='amt'>").Append(closingDisplay).Append("</td></tr>");

            phBody.Controls.Add(new LiteralControl(sb.ToString()));
            pnlStatement.Visible = true;

            if (lines.Rows.Count == 0 && opening == null)
            {
                pnlEmpty.Visible = true;
            }
        }

        private static string FormatIndian(decimal amount)
        {
            return amount.ToString("N2", _ci);
        }
    }
}
