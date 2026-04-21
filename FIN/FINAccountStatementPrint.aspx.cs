using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using FINApp.DAL;

namespace FINApp
{
    public partial class FINAccountStatementPrint : System.Web.UI.Page
    {
        // Controls declared in markup
        protected Label lblAddress, lblPartyName, lblPartyGSTIN, lblPeriod, lblGeneratedOn;
        protected PlaceHolder phBody;

        private static readonly CultureInfo _ci = new CultureInfo("en-IN");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["FIN_UserID"] == null)
            {
                Response.Redirect("FINLogin.aspx");
                return;
            }

            // TODO: pull from company master / config if available
            lblAddress.Text = "South India  •  www.sirimiri.in";

            string partyType = Request.QueryString["type"];
            string pidStr = Request.QueryString["pid"];
            string fromStr = Request.QueryString["from"];
            string toStr = Request.QueryString["to"];

            if (string.IsNullOrEmpty(partyType) || string.IsNullOrEmpty(pidStr))
            {
                Response.Write("<p>Missing parameters.</p>");
                return;
            }

            int partyID = int.Parse(pidStr);
            DateTime fromDate = DateTime.ParseExact(fromStr, "dd-MMM-yyyy", _ci);
            DateTime toDate = DateTime.ParseExact(toStr, "dd-MMM-yyyy", _ci);

            DataRow party = FINDatabaseHelper.GetParty(partyType, partyID);
            if (party == null) { Response.Write("<p>Party not found.</p>"); return; }

            int fyStartYear = fromDate.Month >= 4 ? fromDate.Year : fromDate.Year - 1;
            string fy = fyStartYear + "-" + ((fyStartYear + 1) % 100).ToString("D2");

            DataRow opening = FINDatabaseHelper.GetOpeningBalance(partyType, partyID, fy);
            decimal openingSigned = 0m;
            decimal openingAmt = 0m;
            string openingDrCr = "Dr";
            DateTime? openingDate = null;
            if (opening != null)
            {
                openingAmt = Convert.ToDecimal(opening["Amount"]);
                openingDrCr = opening["DrCr"].ToString();
                openingDate = Convert.ToDateTime(opening["AsOfDate"]);
                openingSigned = openingDrCr == "Dr" ? openingAmt : -openingAmt;
            }

            DataTable lines = FINDatabaseHelper.GetPartyStatement(partyType, partyID, fromDate, toDate);

            lblPartyName.Text = party["Name"].ToString() + " (" + partyType + ":" + party["PartyID"] + ")";
            string gstin = party["GSTIN"] == DBNull.Value ? "" : party["GSTIN"].ToString();
            lblPartyGSTIN.Text = string.IsNullOrEmpty(gstin) ? "" : "GSTIN: " + gstin;
            lblPeriod.Text = fromDate.ToString("dd-MMM-yyyy") + " to " + toDate.ToString("dd-MMM-yyyy");
            lblGeneratedOn.Text = DateTime.Now.ToString("dd-MMM-yyyy HH:mm");

            var sb = new StringBuilder();

            // Opening
            string openingDisplay = opening == null
                ? "0.00 (no opening set)"
                : FormatIndian(openingAmt) + " " + openingDrCr;
            sb.Append("<tr class='opening'>")
              .Append("<td>").Append(openingDate.HasValue ? openingDate.Value.ToString("dd-MMM-yyyy") : "").Append("</td>")
              .Append("<td></td><td>Opening Balance</td>")
              .Append("<td class='amt'></td><td class='amt'></td>")
              .Append("<td class='amt'>").Append(openingDisplay).Append("</td>")
              .Append("</tr>");

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
        }

        private static string FormatIndian(decimal amount)
        {
            return amount.ToString("N2", _ci);
        }
    }
}
