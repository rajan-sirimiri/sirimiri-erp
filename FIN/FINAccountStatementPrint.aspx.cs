using System;
using System.Globalization;
using System.Text;
using System.Web.UI;
using Sirimiri.FIN.DAL;

namespace Sirimiri.FIN
{
    public partial class FINAccountStatementPrint : Page
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

            // TODO: pull address from config / company master
            lblAddress.Text = "South India  •  GSTIN 33XXXXXXXXXXXX  •  www.sirimiri.in";

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

            var party = _db.GetParty(partyType, partyID);
            if (party == null) { Response.Write("<p>Party not found.</p>"); return; }

            int fyStartYear = fromDate.Month >= 4 ? fromDate.Year : fromDate.Year - 1;
            string fy = fyStartYear + "-" + ((fyStartYear + 1) % 100).ToString("D2");

            var opening = _db.GetOpeningBalance(partyType, partyID, fy);
            decimal openingSigned = 0m;
            if (opening != null)
                openingSigned = opening.DrCr == "Dr" ? opening.Amount : -opening.Amount;

            var lines = _db.GetPartyStatement(partyType, partyID, fromDate, toDate);

            lblPartyName.Text = party.Name + " (" + partyType + ":" + party.PartyID + ")";
            lblPartyGSTIN.Text = string.IsNullOrEmpty(party.GSTIN) ? "" : "GSTIN: " + party.GSTIN;
            lblPeriod.Text = fromDate.ToString("dd-MMM-yyyy") + " to " + toDate.ToString("dd-MMM-yyyy");
            lblGeneratedOn.Text = DateTime.Now.ToString("dd-MMM-yyyy HH:mm");

            var sb = new StringBuilder();

            // Opening row
            string openingDisplay = opening == null
                ? "0.00 (no opening set)"
                : opening.Amount.ToString("N2", _ci) + " " + opening.DrCr;
            sb.Append("<tr class='opening'>");
            sb.Append("<td>").Append(opening != null ? opening.AsOfDate.ToString("dd-MMM-yyyy") : "").Append("</td>");
            sb.Append("<td></td>");
            sb.Append("<td>Opening Balance</td>");
            sb.Append("<td class='amt'></td><td class='amt'></td>");
            sb.Append("<td class='amt'>").Append(openingDisplay).Append("</td>");
            sb.Append("</tr>");

            decimal running = openingSigned;
            decimal totalDebit = 0m, totalCredit = 0m;
            foreach (var ln in lines)
            {
                running += ln.Debit - ln.Credit;
                totalDebit += ln.Debit;
                totalCredit += ln.Credit;
                string bal = Math.Abs(running).ToString("N2", _ci) + " " + (running >= 0 ? "Dr" : "Cr");

                sb.Append("<tr>");
                sb.Append("<td>").Append(ln.TxnDate.ToString("dd-MMM-yyyy")).Append("</td>");
                sb.Append("<td>").Append(Server.HtmlEncode(ln.VoucherNo ?? "")).Append("</td>");
                sb.Append("<td>").Append(Server.HtmlEncode(ln.Particulars ?? "")).Append("</td>");
                sb.Append("<td class='amt'>").Append(ln.Debit == 0 ? "" : ln.Debit.ToString("N2", _ci)).Append("</td>");
                sb.Append("<td class='amt'>").Append(ln.Credit == 0 ? "" : ln.Credit.ToString("N2", _ci)).Append("</td>");
                sb.Append("<td class='amt'>").Append(bal).Append("</td>");
                sb.Append("</tr>");
            }

            // Totals + closing
            string closingDisplay = Math.Abs(running).ToString("N2", _ci) + " " + (running >= 0 ? "Dr" : "Cr");
            sb.Append("<tr class='totals'>");
            sb.Append("<td></td><td></td><td>Period Totals</td>");
            sb.Append("<td class='amt'>").Append(totalDebit.ToString("N2", _ci)).Append("</td>");
            sb.Append("<td class='amt'>").Append(totalCredit.ToString("N2", _ci)).Append("</td>");
            sb.Append("<td class='amt'></td></tr>");
            sb.Append("<tr class='closing'>");
            sb.Append("<td></td><td></td><td>Closing Balance</td>");
            sb.Append("<td class='amt'></td><td class='amt'></td>");
            sb.Append("<td class='amt'>").Append(closingDisplay).Append("</td></tr>");

            phBody.Controls.Add(new LiteralControl(sb.ToString()));
        }
    }
}
