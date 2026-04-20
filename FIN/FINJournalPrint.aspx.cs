using System;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using FINApp.DAL;

namespace FINApp
{
    /// <summary>
    /// Tally-style journal voucher print page.  Renders a single journal
    /// (looked up by ?id=) in a printable Indian-accounting format:
    ///   - Party name is shown as the Particulars for AP/AR lines (not "Accounts Payable")
    ///   - "via Accounts Payable" appears as small subtext for traceability
    ///   - Amount in words uses Indian numbering (lakh / crore)
    ///
    /// No postbacks beyond initial GET.  Screen has Back / Print / Download PDF
    /// buttons; these are hidden from the printed output via @media print CSS.
    /// </summary>
    public partial class FINJournalPrint : Page
    {
        protected Literal litTbNumber, litStatusTag, litVoucherNo, litVoucherDate,
                         litTotalDebit, litTotalCredit, litNarration, litReference,
                         litAmountWords, litEnteredBy, litErrorMsg;
        protected PlaceHolder phLines, phReversalNote;
        protected Panel pnlVoucher, pnlError;
        protected HyperLink lnkBack;

        // ── state / query ──────────────────────────────────────────────
        int JournalID
        {
            get
            {
                int id;
                return int.TryParse(Request.QueryString["id"] ?? "", out id) ? id : 0;
            }
        }

        // Exposed to the .aspx for the download filename (voucher number,
        // sanitised for filesystem safety).
        public string VoucherNumberSafe { get; private set; }

        // Cached CoA / parties — same pattern as FINJournal.aspx.cs
        DataTable _chartCache;
        DataTable Chart
        {
            get { return _chartCache ?? (_chartCache = FINDatabaseHelper.GetChartOfAccounts(activeOnly: false)); }
        }

        DataTable _partyCache;
        DataTable Parties
        {
            get { return _partyCache ?? (_partyCache = FINDatabaseHelper.GetPartyList()); }
        }

        // ══════════════════════════════════════════════════════════════
        //  PAGE LOAD
        // ══════════════════════════════════════════════════════════════
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["FIN_UserID"] == null)
            {
                Response.Redirect("FINLogin.aspx?returnUrl=" + Server.UrlEncode(Request.RawUrl));
                return;
            }

            VoucherNumberSafe = "draft";  // default so the .aspx inline <%= %> never NREs

            if (JournalID <= 0)
            {
                ShowError("No journal ID supplied in the URL (expected ?id=<number>).");
                return;
            }

            var ds = FINDatabaseHelper.GetJournalDetail(JournalID);
            if (ds.Tables["Header"].Rows.Count == 0)
            {
                ShowError("Journal #" + JournalID + " was not found.");
                return;
            }

            DataRow h  = ds.Tables["Header"].Rows[0];
            DataTable lines = ds.Tables["Lines"];
            string status = h["Status"].ToString();

            // Only POSTED and REVERSED are printable.  DRAFTs are not
            // considered legal vouchers yet.
            if (status != "POSTED" && status != "REVERSED")
            {
                ShowError("Only POSTED or REVERSED journals can be printed. This one is " + status + ".");
                return;
            }

            RenderHeader(h);
            RenderLines(lines);
            RenderFooter(h, lines);

            // Set the back-link to go to this specific journal's detail page
            lnkBack.NavigateUrl = "FINJournal.aspx?id=" + JournalID;
        }

        // ══════════════════════════════════════════════════════════════
        //  HEADER (top meta + status + reversal notice)
        // ══════════════════════════════════════════════════════════════
        void RenderHeader(DataRow h)
        {
            string voucherNo = h["JournalNumber"].ToString();
            VoucherNumberSafe = SanitiseForFilename(voucherNo);

            litTbNumber.Text     = Server.HtmlEncode(voucherNo);
            litVoucherNo.Text    = Server.HtmlEncode(voucherNo);
            litVoucherDate.Text  = Convert.ToDateTime(h["JournalDate"]).ToString("dd-MMM-yyyy");

            string narr = h["Narration"] == DBNull.Value ? "" : h["Narration"].ToString();
            string refNo = h["Reference"] == DBNull.Value ? "" : h["Reference"].ToString();
            litNarration.Text = string.IsNullOrEmpty(narr) ? "&mdash;" : Server.HtmlEncode(narr);
            litReference.Text = string.IsNullOrEmpty(refNo) ? "&mdash;" : Server.HtmlEncode(refNo);

            string enteredBy = h["CreatedByName"] == DBNull.Value ? "" : h["CreatedByName"].ToString();
            litEnteredBy.Text = string.IsNullOrEmpty(enteredBy) ? "&mdash;" : Server.HtmlEncode(enteredBy);

            // Status tag next to the title
            string status = h["Status"].ToString();
            if (status == "REVERSED")
            {
                litStatusTag.Text = "<span class='vh-status-tag reversed'>Reversed</span>";
                // Link the contra if available
                if (h["ReversedByJournalID"] != DBNull.Value)
                {
                    int byId = Convert.ToInt32(h["ReversedByJournalID"]);
                    phReversalNote.Controls.Add(new LiteralControl(
                        "<div class='vh-reversal-note'>This voucher has been reversed. " +
                        "Contra journal: <a href='FINJournalPrint.aspx?id=" + byId + "'>#" + byId + "</a></div>"));
                }
            }
            else
            {
                // POSTED — small confirmation tag
                litStatusTag.Text = "<span class='vh-status-tag posted'>Posted</span>";
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  LINES (the main particulars table)
        // ══════════════════════════════════════════════════════════════
        void RenderLines(DataTable lines)
        {
            var sb = new StringBuilder();
            foreach (DataRow ln in lines.Rows)
            {
                string accountId   = ln["ZohoAccountID"].ToString();
                string accountName = GetAccountDisplayName(accountId);
                string accountType = GetAccountTypeName(accountId);
                string contactKey  = ln["ContactID"] == DBNull.Value ? "" : ln["ContactID"].ToString();
                string lineDesc    = ln["LineDescription"] == DBNull.Value ? "" : ln["LineDescription"].ToString();

                decimal dr = Convert.ToDecimal(ln["Debit"]);
                decimal cr = Convert.ToDecimal(ln["Credit"]);
                bool isDebit = dr > 0;

                // ── Particulars column construction (Tally-style swap) ──
                // For AP/AR-with-party lines, the PARTY NAME is the primary
                // text; the account becomes "via ..." subtext.  For everything
                // else, the account name is primary.
                bool isPartyAccount =
                    accountType.Contains("payable") || accountType.Contains("receivable");
                string primary, secondaryVia;
                if (isPartyAccount && !string.IsNullOrEmpty(contactKey))
                {
                    primary      = ResolvePartyName(contactKey);
                    secondaryVia = accountName;
                    // Defensive — if party name lookup failed for any reason,
                    // fall back so the voucher is never blank.
                    if (string.IsNullOrEmpty(primary))
                    {
                        primary      = accountName;
                        secondaryVia = "";
                    }
                }
                else
                {
                    primary      = accountName;
                    secondaryVia = "";
                }

                // Dr/Cr label — styled bold, abbreviated as "Dr" / "Cr"
                string drCrLabel = isDebit ? "Dr" : "Cr";

                // Particulars cell HTML
                var part = new StringBuilder();
                part.Append("<div class='party-line'>").Append(Server.HtmlEncode(primary)).Append("</div>");
                if (!string.IsNullOrEmpty(secondaryVia))
                    part.Append("<div class='via-line'>via ").Append(Server.HtmlEncode(secondaryVia)).Append("</div>");
                if (!string.IsNullOrEmpty(lineDesc))
                    part.Append("<div class='desc-line'>").Append(Server.HtmlEncode(lineDesc)).Append("</div>");

                // Row — always show both Debit and Credit columns so the
                // table lines up; empty cell on the unused side.
                sb.Append("<tr>");
                sb.Append("<td class='cell-drcr'>").Append(drCrLabel).Append("</td>");
                sb.Append("<td class='cell-particulars'>").Append(part.ToString()).Append("</td>");
                if (isDebit)
                {
                    sb.Append("<td class='cell-amount'>").Append(dr.ToString("N2")).Append("</td>");
                    sb.Append("<td class='cell-amount-empty'>&nbsp;</td>");
                }
                else
                {
                    sb.Append("<td class='cell-amount-empty'>&nbsp;</td>");
                    sb.Append("<td class='cell-amount'>").Append(cr.ToString("N2")).Append("</td>");
                }
                sb.Append("</tr>");
            }
            phLines.Controls.Add(new LiteralControl(sb.ToString()));
        }

        // ══════════════════════════════════════════════════════════════
        //  FOOTER (totals + amount in words)
        // ══════════════════════════════════════════════════════════════
        void RenderFooter(DataRow h, DataTable lines)
        {
            decimal td = Convert.ToDecimal(h["TotalDebit"]);
            decimal tc = Convert.ToDecimal(h["TotalCredit"]);
            litTotalDebit.Text  = td.ToString("N2");
            litTotalCredit.Text = tc.ToString("N2");

            // For the "in words" line, use the larger of Dr / Cr (they
            // should be equal for a balanced journal — defensive in case).
            decimal forWords = Math.Max(td, tc);
            litAmountWords.Text = "Rupees " + AmountInWordsIndian(forWords) + " Only";
        }

        // ══════════════════════════════════════════════════════════════
        //  HELPERS — chart / party lookup (mirror FINJournal.aspx.cs)
        // ══════════════════════════════════════════════════════════════
        string GetAccountDisplayName(string zohoAccountId)
        {
            if (string.IsNullOrEmpty(zohoAccountId)) return "";
            foreach (DataRow r in Chart.Rows)
            {
                if (r["ZohoAccountID"].ToString() == zohoAccountId)
                    return (r["AccountName"] ?? "").ToString();
            }
            return "";
        }

        string GetAccountTypeName(string zohoAccountId)
        {
            if (string.IsNullOrEmpty(zohoAccountId)) return "";
            foreach (DataRow r in Chart.Rows)
            {
                if (r["ZohoAccountID"].ToString() == zohoAccountId)
                    return ((r["AccountTypeName"] ?? r["AccountType"] ?? "").ToString()).ToLowerInvariant();
            }
            return "";
        }

        string ResolvePartyName(string partyKey)
        {
            if (string.IsNullOrEmpty(partyKey)) return "";
            var found = Parties.Select("PartyKey = '" + partyKey.Replace("'", "''") + "'");
            if (found.Length > 0) return (found[0]["PartyName"] ?? "").ToString();
            return FINDatabaseHelper.GetPartyDisplayName(partyKey);
        }

        // ══════════════════════════════════════════════════════════════
        //  HELPERS — amount-in-words (Indian lakh/crore convention)
        //
        //  Handles values up to 99,99,99,999.99 (99 crore and change).
        //  Output examples:
        //    315000.00   → "Three Lakh Fifteen Thousand"
        //    12050.50    → "Twelve Thousand Fifty and Paise Fifty"
        //    100.00      → "One Hundred"
        //    0.75        → "Zero and Paise Seventy Five"
        //    1.00        → "One"
        // ══════════════════════════════════════════════════════════════
        static readonly string[] Ones = {
            "Zero","One","Two","Three","Four","Five","Six","Seven","Eight","Nine",
            "Ten","Eleven","Twelve","Thirteen","Fourteen","Fifteen","Sixteen",
            "Seventeen","Eighteen","Nineteen"
        };
        static readonly string[] Tens = {
            "","","Twenty","Thirty","Forty","Fifty","Sixty","Seventy","Eighty","Ninety"
        };

        static string TwoDigitWords(int n)
        {
            if (n < 20) return Ones[n];
            int t = n / 10, o = n % 10;
            return Tens[t] + (o > 0 ? " " + Ones[o] : "");
        }

        static string ThreeDigitWords(int n)
        {
            if (n == 0) return "";
            if (n < 100) return TwoDigitWords(n);
            int h = n / 100, rest = n % 100;
            string s = Ones[h] + " Hundred";
            if (rest > 0) s += " " + TwoDigitWords(rest);
            return s;
        }

        /// <summary>
        /// Convert a rupee amount to Indian-numbered words.  Handles crore,
        /// lakh, thousand, hundred, and paise.  Returns the body only
        /// (e.g. "Three Lakh Fifteen Thousand") — the caller adds "Rupees"
        /// and "Only" wrappers.
        /// </summary>
        public static string AmountInWordsIndian(decimal amount)
        {
            if (amount < 0)
                return "Minus " + AmountInWordsIndian(-amount);

            // Split rupees and paise
            long rupees = (long)Math.Floor(amount);
            int paise = (int)Math.Round((amount - rupees) * 100m);
            // Rounding edge case — 99.999 could yield 100 paise
            if (paise == 100) { rupees += 1; paise = 0; }

            var sb = new StringBuilder();

            if (rupees == 0)
            {
                sb.Append("Zero");
            }
            else
            {
                long crore = rupees / 10000000L;                    // 1,00,00,000
                long afterCrore = rupees % 10000000L;
                long lakh = afterCrore / 100000L;                   // 1,00,000
                long afterLakh = afterCrore % 100000L;
                long thousand = afterLakh / 1000L;                  // 1,000
                int afterThousand = (int)(afterLakh % 1000L);

                var parts = new System.Collections.Generic.List<string>();
                if (crore > 0)    parts.Add(ThreeDigitWords((int)crore) + " Crore");
                if (lakh > 0)     parts.Add(ThreeDigitWords((int)lakh) + " Lakh");
                if (thousand > 0) parts.Add(ThreeDigitWords((int)thousand) + " Thousand");
                if (afterThousand > 0) parts.Add(ThreeDigitWords(afterThousand));

                sb.Append(string.Join(" ", parts));
            }

            if (paise > 0)
                sb.Append(" and Paise ").Append(TwoDigitWords(paise));

            return sb.ToString();
        }

        // ══════════════════════════════════════════════════════════════
        //  MISC
        // ══════════════════════════════════════════════════════════════
        void ShowError(string msg)
        {
            pnlVoucher.Visible = false;
            pnlError.Visible = true;
            litErrorMsg.Text = Server.HtmlEncode(msg);
        }

        /// <summary>Strip characters that would be invalid in a filename.</summary>
        static string SanitiseForFilename(string s)
        {
            if (string.IsNullOrEmpty(s)) return "voucher";
            var invalid = System.IO.Path.GetInvalidFileNameChars();
            var sb = new StringBuilder();
            foreach (char c in s)
            {
                if (Array.IndexOf(invalid, c) < 0 && c != ' ') sb.Append(c);
                else sb.Append('_');
            }
            return sb.ToString();
        }
    }
}
