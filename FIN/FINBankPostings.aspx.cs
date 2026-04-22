using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using ClosedXML.Excel;
using FINApp.DAL;

namespace FINApp
{
    /// <summary>
    /// Bank Postings — upload XLSX bank statements, parse using the bank's
    /// saved column layout, and store rows for later posting.
    ///
    /// Phase 1 (this release): upload + parse + store + view.
    /// Phase 2: post each row to fin_journal and push to Zoho Books.
    /// </summary>
    public partial class FINBankPostings : System.Web.UI.Page
    {
        protected Label lblNavUser, lblAlert, lblDetailHeader;
        protected Label lblStatPeriod, lblStatOpen, lblStatClose, lblStatRows;
        protected HtmlGenericControl alertBox;
        protected Panel pnlAlert, pnlListView, pnlDetailView, pnlNoLayout, pnlEmpty;
        protected DropDownList ddlBank;
        protected FileUpload fuStatement;
        protected Button btnUpload;
        protected Repeater rptStatements, rptLines;

        private string UserRole  => Session["FIN_Role"]?.ToString() ?? "";
        private bool   IsFinance => FINConsignments.IsFinanceRole(UserRole);
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
                LoadBankDropdown();
                LoadStatementList();
            }
        }

        private void LoadBankDropdown()
        {
            DataTable dt = FINDatabaseHelper.GetActiveBankAccounts();
            ddlBank.Items.Clear();
            ddlBank.Items.Add(new ListItem("-- Select bank --", ""));
            foreach (DataRow r in dt.Rows)
            {
                string label = r["BankCode"] + "  " + r["BankName"];
                if (r["AccountNumber"] != DBNull.Value && !string.IsNullOrEmpty(r["AccountNumber"].ToString()))
                    label += "  (" + r["AccountNumber"] + ")";
                ddlBank.Items.Add(new ListItem(label, r["BankID"].ToString()));
            }
        }

        private void LoadStatementList()
        {
            DataTable dt = FINDatabaseHelper.ListBankStatements(null);
            if (dt.Rows.Count > 0)
            {
                rptStatements.DataSource = dt;
                rptStatements.DataBind();
                pnlEmpty.Visible = false;
            }
            else
            {
                rptStatements.DataSource = null;
                rptStatements.DataBind();
                pnlEmpty.Visible = true;
            }
        }

        private void ShowAlert(string message, string kind)
        {
            // kind: "success" | "info" | "danger"
            pnlAlert.Visible = true;
            lblAlert.Text = message;
            alertBox.Attributes["class"] = "alert alert-" + kind;
        }

        // ══════════════════════════════════════════════════════════════
        //  Upload + parse
        // ══════════════════════════════════════════════════════════════

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            pnlNoLayout.Visible = false;

            if (string.IsNullOrEmpty(ddlBank.SelectedValue))
            {
                ShowAlert("Pick a bank first.", "danger");
                return;
            }
            if (!fuStatement.HasFile)
            {
                ShowAlert("Choose an XLSX file to upload.", "danger");
                return;
            }
            if (!fuStatement.FileName.ToLowerInvariant().EndsWith(".xlsx"))
            {
                ShowAlert("Only .xlsx files are supported. Re-save your file as XLSX if it's XLS or CSV.", "danger");
                return;
            }

            int bankId = Convert.ToInt32(ddlBank.SelectedValue);
            DataRow layout = FINDatabaseHelper.GetBankLayout(bankId);
            if (layout == null || layout["IsConfigured"] == DBNull.Value || Convert.ToInt32(layout["IsConfigured"]) != 1)
            {
                pnlNoLayout.Visible = true;
                return;
            }

            try
            {
                ParseAndStore(bankId, layout, fuStatement.FileName, fuStatement.FileBytes);
            }
            catch (Exception ex)
            {
                ShowAlert("Upload failed: " + ex.Message, "danger");
            }
        }

        /// <summary>Parse the uploaded XLSX using the bank's saved column layout and
        /// insert one fin_bankstatementlines row per statement row. Exact-duplicate
        /// rows (already present from a prior upload) are silently skipped.</summary>
        private void ParseAndStore(int bankId, DataRow layout, string fileName, byte[] fileBytes)
        {
            int headerRow    = layout["HeaderRow"]   == DBNull.Value ? 1 : Convert.ToInt32(layout["HeaderRow"]);
            int firstDataRow = layout["FirstDataRow"]== DBNull.Value ? headerRow + 1 : Convert.ToInt32(layout["FirstDataRow"]);
            string dateCol   = GetLayoutCol(layout, "DateCol");
            string descCol   = GetLayoutCol(layout, "DescCol");
            string refCol    = GetLayoutCol(layout, "RefCol");
            string mode      = layout["AmountMode"] == DBNull.Value ? "TWO_COL" : layout["AmountMode"].ToString();
            string debCol    = GetLayoutCol(layout, "DebitCol");
            string crdCol    = GetLayoutCol(layout, "CreditCol");
            string amtCol    = GetLayoutCol(layout, "AmountCol");
            string flagCol   = GetLayoutCol(layout, "FlagCol");
            string balCol    = GetLayoutCol(layout, "BalanceCol");
            string dateFmt   = layout["DateFormat"] == DBNull.Value ? "dd/MM/yyyy" : layout["DateFormat"].ToString();

            if (string.IsNullOrEmpty(dateCol))
                throw new Exception("Layout is missing the Date column. Edit the bank layout and set it.");

            int dateIdx = ColLetterToIndex(dateCol);
            int descIdx = ColLetterToIndex(descCol);
            int refIdx  = ColLetterToIndex(refCol);
            int debIdx  = ColLetterToIndex(debCol);
            int crdIdx  = ColLetterToIndex(crdCol);
            int amtIdx  = ColLetterToIndex(amtCol);
            int flagIdx = ColLetterToIndex(flagCol);
            int balIdx  = ColLetterToIndex(balCol);

            // First pass: walk the worksheet, collect rows into memory so we can compute
            // period and balances before creating the statement header. Using a temp list
            // avoids needing a multi-statement transaction.
            var rows = new System.Collections.Generic.List<ParsedLine>();
            DateTime? periodStart = null, periodEnd = null;
            decimal? openBal = null, closeBal = null;

            using (var stream = new MemoryStream(fileBytes))
            using (var wb = new XLWorkbook(stream))
            {
                var ws = wb.Worksheet(1);
                int lastRow = ws.LastRowUsed() != null ? ws.LastRowUsed().RowNumber() : 0;
                int seq = 0;
                for (int r = firstDataRow; r <= lastRow; r++)
                {
                    // Date — if blank, skip the row (might be a blank separator)
                    var dateCell = ws.Cell(r, dateIdx);
                    DateTime txnDate;
                    if (!TryReadDate(dateCell, dateFmt, out txnDate)) continue;

                    decimal debit = 0m, credit = 0m;
                    if (mode == "TWO_COL")
                    {
                        debit  = ReadDecimalSafe(ws, r, debIdx);
                        credit = ReadDecimalSafe(ws, r, crdIdx);
                    }
                    else if (mode == "FLAG")
                    {
                        decimal amt = ReadDecimalSafe(ws, r, amtIdx);
                        string flag = flagIdx > 0 ? (ws.Cell(r, flagIdx).GetString() ?? "").Trim().ToUpperInvariant() : "";
                        if (flag.StartsWith("DR") || flag.Contains("DEBIT"))      debit = Math.Abs(amt);
                        else if (flag.StartsWith("CR") || flag.Contains("CREDIT")) credit = Math.Abs(amt);
                        else continue;  // ambiguous — skip with no loss (user can re-check layout)
                    }
                    else if (mode == "SIGNED")
                    {
                        decimal amt = ReadDecimalSafe(ws, r, amtIdx);
                        if (amt < 0) debit = -amt;
                        else if (amt > 0) credit = amt;
                        else continue;
                    }

                    if (debit == 0 && credit == 0) continue;  // non-transaction row

                    string description = descIdx > 0 ? (ws.Cell(r, descIdx).GetString() ?? "").Trim() : "";
                    string reference   = refIdx  > 0 ? (ws.Cell(r, refIdx).GetString()  ?? "").Trim() : "";
                    decimal? balance   = balIdx  > 0 ? (decimal?)ReadDecimalSafe(ws, r, balIdx) : null;

                    seq++;
                    rows.Add(new ParsedLine {
                        Seq = seq, Date = txnDate, Description = description,
                        Reference = reference, Debit = debit, Credit = credit, Balance = balance
                    });

                    if (periodStart == null || txnDate < periodStart) periodStart = txnDate;
                    if (periodEnd   == null || txnDate > periodEnd)   periodEnd   = txnDate;
                }
            }

            if (rows.Count == 0)
            {
                ShowAlert("No data rows recognised. Check the XLSX layout for this bank — the parser might be looking at the wrong columns.", "danger");
                return;
            }

            // Opening/closing balance = balance on first/last row if present
            openBal  = rows[0].Balance;
            closeBal = rows[rows.Count - 1].Balance;

            // Create the statement header
            int statementId = FINDatabaseHelper.CreateBankStatement(
                bankId, fileName, periodStart, periodEnd, openBal, closeBal, CurrentUserID);

            // Insert lines. Count dedup skips.
            int inserted = 0, duplicates = 0;
            foreach (var line in rows)
            {
                int affected = FINDatabaseHelper.InsertBankLine(
                    statementId, bankId, line.Seq,
                    line.Date, line.Description, line.Reference,
                    line.Debit, line.Credit, line.Balance);
                if (affected == 1) inserted++;
                else duplicates++;
            }

            FINDatabaseHelper.UpdateStatementCounts(statementId, inserted, duplicates);

            string msg = "Uploaded. Inserted " + inserted + " new rows";
            if (duplicates > 0) msg += ", skipped " + duplicates + " exact duplicate(s)";
            msg += ".";
            ShowAlert(msg, "success");
            LoadStatementList();
        }

        private class ParsedLine
        {
            public int Seq;
            public DateTime Date;
            public string Description;
            public string Reference;
            public decimal Debit;
            public decimal Credit;
            public decimal? Balance;
        }

        // ── Helpers ───────────────────────────────────────────────────

        private static string GetLayoutCol(DataRow layout, string col)
        {
            object v = layout[col];
            return v == DBNull.Value ? "" : v.ToString();
        }

        /// <summary>Convert a spreadsheet column letter like "A", "B", "AA" into a 1-based
        /// column index. Returns 0 for empty/invalid input (used as "not set").</summary>
        private static int ColLetterToIndex(string letter)
        {
            if (string.IsNullOrEmpty(letter)) return 0;
            letter = letter.Trim().ToUpperInvariant();
            int idx = 0;
            foreach (char c in letter)
            {
                if (c < 'A' || c > 'Z') return 0;
                idx = idx * 26 + (c - 'A' + 1);
            }
            return idx;
        }

        private static decimal ReadDecimalSafe(IXLWorksheet ws, int row, int colIdx)
        {
            if (colIdx <= 0) return 0m;
            var cell = ws.Cell(row, colIdx);
            if (cell.IsEmpty()) return 0m;
            try { return cell.GetValue<decimal>(); }
            catch {
                string s = (cell.GetString() ?? "").Trim().Replace(",", "").Replace("\u20B9", "").Replace("Rs.", "");
                decimal d;
                return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out d) ? d : 0m;
            }
        }

        private static bool TryReadDate(IXLCell cell, string format, out DateTime result)
        {
            result = DateTime.MinValue;
            if (cell.IsEmpty()) return false;
            try
            {
                if (cell.DataType == XLDataType.DateTime) { result = cell.GetDateTime(); return true; }
                // Some XLSX files store dates as numbers (Excel serial dates) even when DataType says Number
                if (cell.DataType == XLDataType.Number)
                {
                    double d = cell.GetDouble();
                    if (d > 1 && d < 2958466) // valid Excel date range
                    {
                        result = DateTime.FromOADate(d);
                        return true;
                    }
                }
                string s = (cell.GetString() ?? "").Trim();
                if (string.IsNullOrEmpty(s)) return false;
                if (DateTime.TryParseExact(s, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result)) return true;
                // Also try common alternatives — Indian banks frequently use these:
                string[] tryFormats = new[] {
                    "dd/MM/yyyy", "dd-MM-yyyy", "dd-MMM-yyyy", "yyyy-MM-dd",
                    "dd/MM/yy", "dd-MM-yy", "d/M/yyyy", "d-MMM-yyyy"
                };
                foreach (var f in tryFormats)
                    if (DateTime.TryParseExact(s, f, CultureInfo.InvariantCulture, DateTimeStyles.None, out result)) return true;
                if (DateTime.TryParse(s, out result)) return true;
            }
            catch { }
            return false;
        }

        // ── Detail view ───────────────────────────────────────────────

        protected void rptStatements_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "View")
            {
                int stmtId = Convert.ToInt32(e.CommandArgument);
                ShowDetail(stmtId);
            }
        }

        private void ShowDetail(int statementId)
        {
            DataRow hdr = FINDatabaseHelper.GetBankStatementHeader(statementId);
            if (hdr == null)
            {
                ShowAlert("Statement not found.", "danger");
                return;
            }

            pnlListView.Visible = false;
            pnlDetailView.Visible = true;
            ViewState["StatementID"] = statementId;

            lblDetailHeader.Text = hdr["BankCode"] + " — " + hdr["BankName"] + "  ·  " + hdr["FileName"];
            lblStatPeriod.Text = FormatPeriod(hdr["PeriodStart"], hdr["PeriodEnd"]);
            lblStatOpen.Text   = FormatMoney(hdr["OpeningBalance"]);
            lblStatClose.Text  = FormatMoney(hdr["ClosingBalance"]);
            lblStatRows.Text   = hdr["RowCount"].ToString();

            DataTable lines = FINDatabaseHelper.GetStatementLines(statementId);
            rptLines.DataSource = lines;
            rptLines.DataBind();
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            pnlDetailView.Visible = false;
            pnlListView.Visible = true;
            ViewState.Remove("StatementID");
            LoadStatementList();
        }

        // ── Databinding formatters ────────────────────────────────────

        protected string FormatMoney(object v)
        {
            if (v == null || v == DBNull.Value) return "";
            decimal d = Convert.ToDecimal(v);
            if (d == 0m) return "";
            return d.ToString("N2", CultureInfo.GetCultureInfo("en-IN"));
        }

        protected string FormatDate(object v)
        {
            if (v == null || v == DBNull.Value) return "";
            return Convert.ToDateTime(v).ToString("dd MMM yyyy");
        }

        protected string FormatDateTime(object v)
        {
            if (v == null || v == DBNull.Value) return "";
            return Convert.ToDateTime(v).ToString("dd MMM yyyy HH:mm");
        }

        protected string FormatPeriod(object start, object end)
        {
            string s = start == null || start == DBNull.Value ? "" : Convert.ToDateTime(start).ToString("dd MMM yyyy");
            string e = end   == null || end   == DBNull.Value ? "" : Convert.ToDateTime(end).ToString("dd MMM yyyy");
            if (s == "" && e == "") return "—";
            if (s == e) return s;
            return s + "  →  " + e;
        }
    }
}
