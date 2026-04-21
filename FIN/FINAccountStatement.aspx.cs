using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Sirimiri.FIN.DAL;

namespace Sirimiri.FIN
{
    public partial class FINAccountStatement : Page
    {
        private readonly FINDatabaseHelper _db = new FINDatabaseHelper();
        private static readonly CultureInfo _ci = new CultureInfo("en-IN");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/FIN/FINLogin.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // Default From = 1 Apr current FY, To = today
                DateTime today = DateTime.Today;
                int fyStartYear = today.Month >= 4 ? today.Year : today.Year - 1;
                txtFrom.Text = new DateTime(fyStartYear, 4, 1).ToString("dd-MMM-yyyy");
                txtTo.Text = today.ToString("dd-MMM-yyyy");

                LoadPartyDropdown();
            }
        }

        // -------------------------------------------------------------
        // Dropdown
        // -------------------------------------------------------------
        private void LoadPartyDropdown()
        {
            string partyType = rblPartyType.SelectedValue;
            DateTime fromDate, toDate;
            if (!TryParseDates(out fromDate, out toDate)) return;

            var parties = _db.ListPartiesWithActivity(partyType, fromDate, toDate);
            ddlParty.Items.Clear();
            ddlParty.Items.Add(new ListItem("-- Select --", ""));
            foreach (var p in parties)
            {
                string text = p.Name + (p.IsActive ? "" : " (inactive)");
                ddlParty.Items.Add(new ListItem(text, p.PartyID.ToString()));
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

        // -------------------------------------------------------------
        // Events
        // -------------------------------------------------------------
        protected void rblPartyType_Changed(object sender, EventArgs e)
        {
            LoadPartyDropdown();
            pnlStatement.Visible = false;
            pnlEmpty.Visible = false;
        }

        protected void ddlParty_Changed(object sender, EventArgs e)
        {
            LoadStatement();  // auto-load on party select
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

        // -------------------------------------------------------------
        // Render statement
        // -------------------------------------------------------------
        private void LoadStatement()
        {
            pnlStatement.Visible = false;
            pnlEmpty.Visible = false;
            phOpening.Controls.Clear();
            phFooter.Controls.Clear();

            if (string.IsNullOrEmpty(ddlParty.SelectedValue)) return;

            DateTime fromDate, toDate;
            if (!TryParseDates(out fromDate, out toDate)) return;

            string partyType = rblPartyType.SelectedValue;
            int partyID = int.Parse(ddlParty.SelectedValue);

            var party = _db.GetParty(partyType, partyID);
            if (party == null) { lblMsg.Text = "Party not found."; return; }

            // Determine FY from fromDate (used to fetch opening balance)
            int fyStartYear = fromDate.Month >= 4 ? fromDate.Year : fromDate.Year - 1;
            string fy = fyStartYear + "-" + ((fyStartYear + 1) % 100).ToString("D2");

            var opening = _db.GetOpeningBalance(partyType, partyID, fy);
            decimal openingSigned = 0m;  // positive = Dr, negative = Cr
            if (opening != null)
            {
                openingSigned = opening.DrCr == "Dr" ? opening.Amount : -opening.Amount;
            }

            var lines = _db.GetPartyStatement(partyType, partyID, fromDate, toDate);

            // Compute running balance (Dr positive, Cr negative)
            decimal running = openingSigned;
            decimal totalDebit = 0m, totalCredit = 0m;
            foreach (var ln in lines)
            {
                running += ln.Debit - ln.Credit;
                totalDebit += ln.Debit;
                totalCredit += ln.Credit;
                ln.RunningBalance = Math.Abs(running);
                ln.RunningDrCr = running >= 0 ? "Dr" : "Cr";
            }

            // Header
            lblPartyName.Text = party.Name + " (" + partyType + ":" + party.PartyID + ")";
            lblPartyGSTIN.Text = string.IsNullOrEmpty(party.GSTIN) ? "" : "  •  GSTIN " + party.GSTIN;
            lblPeriod.Text = fromDate.ToString("dd-MMM-yyyy") + " to " + toDate.ToString("dd-MMM-yyyy");

            // Opening row
            string openingDisplay = opening == null
                ? FormatAmt(0m) + " (no opening set)"
                : FormatAmt(opening.Amount) + " " + opening.DrCr;
            phOpening.Controls.Add(new LiteralControl(
                "<tr class='opening'>" +
                "<td>" + (opening != null ? opening.AsOfDate.ToString("dd-MMM-yyyy") : "") + "</td>" +
                "<td></td>" +
                "<td>Opening Balance</td>" +
                "<td class='amt'></td>" +
                "<td class='amt'></td>" +
                "<td class='amt'>" + openingDisplay + "</td>" +
                "</tr>"));

            // Lines
            rptLines.DataSource = lines;
            rptLines.DataBind();

            // Footer (totals + closing)
            string closingDisplay = FormatAmt(Math.Abs(running)) + " " + (running >= 0 ? "Dr" : "Cr");
            phFooter.Controls.Add(new LiteralControl(
                "<tr class='totals'>" +
                "<td></td><td></td><td>Period Totals</td>" +
                "<td class='amt'>" + FormatAmt(totalDebit) + "</td>" +
                "<td class='amt'>" + FormatAmt(totalCredit) + "</td>" +
                "<td class='amt'></td>" +
                "</tr>" +
                "<tr class='closing'>" +
                "<td></td><td></td><td>Closing Balance</td>" +
                "<td class='amt'></td><td class='amt'></td>" +
                "<td class='amt'>" + closingDisplay + "</td>" +
                "</tr>"));

            pnlStatement.Visible = true;
            if (lines.Count == 0 && opening == null)
            {
                pnlEmpty.Visible = true;
            }
        }

        // -------------------------------------------------------------
        // Formatting helpers (used by Repeater templates)
        // -------------------------------------------------------------
        public static string FormatAmt(decimal amt)
        {
            if (amt == 0) return "";
            return amt.ToString("N2", _ci);
        }

        public string FormatBalance(object row)
        {
            var ln = (FINDatabaseHelper.StatementLine)row;
            return ln.RunningBalance.ToString("N2", _ci) + " " + ln.RunningDrCr;
        }
    }
}
