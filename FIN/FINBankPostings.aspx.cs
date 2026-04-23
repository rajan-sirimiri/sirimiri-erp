using System;
using System.Collections.Generic;
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
        protected Panel pnlAlert, pnlListView, pnlDetailView, pnlNoLayout, pnlEmpty, pnlManualBank;
        protected Panel pnlPostModal;
        protected HiddenField hfPostLineID;
        protected Label lblPostDate, lblPostAmount, lblPostDirection, lblPostDesc;
        protected Label lblJvLine1Account, lblJvLine1Party, lblJvLine1Debit, lblJvLine1Credit;
        protected Label lblJvLine2Account, lblJvLine2Party, lblJvLine2Debit, lblJvLine2Credit;
        protected DropDownList ddlPostParty;
        protected Label lblSuggestHint;
        protected Button btnConfirmPost, btnCancelPost;
        protected LinkButton lnkCloseModal;
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
            else if (ViewState["StatementID"] != null)
            {
                // On postback keep the detail view visible. DO NOT re-DataBind
                // the rptLines repeater here — that wipes and recreates the
                // LinkButton child controls AFTER ViewState has already been
                // used to raise the ItemCommand event, so the event never fires.
                // ASP.NET will restore the repeater's child controls from
                // ViewState automatically.
                pnlListView.Visible = false;
                pnlDetailView.Visible = true;
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

            if (!fuStatement.HasFile)
            {
                ShowAlert("Choose an XLSX or PDF file to upload.", "danger");
                return;
            }
            string fnLower = fuStatement.FileName.ToLowerInvariant();
            if (fnLower.EndsWith(".xls") && !fnLower.EndsWith(".xlsx"))
            {
                ShowAlert("Older .xls (Excel 97-2003) format is not supported. Open the file in Excel and use <i>Save As &rarr; Excel Workbook (.xlsx)</i>, then upload the .xlsx.", "danger");
                return;
            }
            if (!fnLower.EndsWith(".xlsx") && !fnLower.EndsWith(".pdf"))
            {
                ShowAlert("Only .xlsx and .pdf files are supported.", "danger");
                return;
            }
            bool isPdf = fnLower.EndsWith(".pdf");

            byte[] fileBytes = fuStatement.FileBytes;
            string fileName  = fuStatement.FileName;

            // 1) If the user explicitly picked a bank in the manual-override dropdown, use that.
            int bankId = 0;
            if (pnlManualBank.Visible && !string.IsNullOrEmpty(ddlBank.SelectedValue))
                bankId = Convert.ToInt32(ddlBank.SelectedValue);

            string detectedName = null;

            // 2) Otherwise try auto-detect from signatures.
            if (bankId == 0)
            {
                bankId = DetectBankFromFile(fileBytes, isPdf, out detectedName);
            }

            if (bankId == 0)
            {
                // No match — show the fallback dropdown.
                pnlManualBank.Visible = true;
                LoadBankDropdown();
                ShowAlert("Couldn't auto-detect the bank from this file. Pick one manually and try again (you'll need to re-select the file too).", "danger");
                return;
            }

            DataRow layout = FINDatabaseHelper.GetBankLayout(bankId);
            // Check the matching layout is configured for this file format.
            string cfgKey = isPdf ? "PdfIsConfigured" : "IsConfigured";
            bool configured = layout != null
                && layout.Table.Columns.Contains(cfgKey)
                && layout[cfgKey] != DBNull.Value
                && Convert.ToInt32(layout[cfgKey]) == 1;
            if (!configured)
            {
                pnlNoLayout.Visible = true;
                ShowAlert("The matched bank has no "
                    + (isPdf ? "PDF" : "XLSX")
                    + " layout configured. Open Manage Bank Accounts and set it for this format.",
                    "danger");
                return;
            }

            try
            {
                ParseAndStore(bankId, layout, fileName, fileBytes, isPdf, detectedName);
                pnlManualBank.Visible = false; // success — clear the fallback
            }
            catch (Exception ex)
            {
                ShowAlert("Upload failed: " + ex.Message, "danger");
            }
        }

        /// <summary>Scan the uploaded file for each configured bank's signature
        /// text. Works for both XLSX (top N rows) and PDF (full text &mdash; the
        /// signature is hopefully on page 1 but search all pages to be safe).
        /// Returns the first match, or 0 if none.</summary>
        private int DetectBankFromFile(byte[] fileBytes, bool isPdf, out string detectedBankName)
        {
            detectedBankName = null;
            DataTable layouts = FINDatabaseHelper.ListConfiguredLayoutsForDetection();
            if (layouts.Rows.Count == 0) return 0;

            // Collect a big blob of text from the file &mdash; approach differs by format:
            //   XLSX: join cells in the top N rows (N per-bank, but use max across all banks).
            //   PDF:  full extracted text (iText7).
            string haystack;
            if (isPdf)
            {
                haystack = PdfStatementExtractor.ExtractPlainText(fileBytes);
            }
            else
            {
                // For XLSX we want to respect per-bank SignatureScanRows, so we can't just
                // flatten once; but the cost of collecting top 30 rows is trivial, and that
                // will satisfy any realistic scanRows setting.
                using (var stream = new MemoryStream(fileBytes))
                using (var wb = new XLWorkbook(stream))
                {
                    var ws = wb.Worksheet(1);
                    int lastRow = ws.LastRowUsed() != null ? ws.LastRowUsed().RowNumber() : 0;
                    int lastCol = ws.LastColumnUsed() != null ? ws.LastColumnUsed().ColumnNumber() : 0;
                    int scanUpTo = Math.Min(30, lastRow);
                    var sb = new System.Text.StringBuilder();
                    for (int r = 1; r <= scanUpTo; r++)
                        for (int col = 1; col <= lastCol; col++)
                            sb.Append(ws.Cell(r, col).GetString() ?? "").Append(' ');
                    haystack = sb.ToString();
                }
            }

            string haystackUpper = haystack.ToUpperInvariant();
            foreach (DataRow layout in layouts.Rows)
            {
                string sig = layout["SignatureText"] == DBNull.Value ? "" : layout["SignatureText"].ToString().Trim();
                if (string.IsNullOrEmpty(sig)) continue;

                if (haystackUpper.Contains(sig.ToUpperInvariant()))
                {
                    detectedBankName = layout["BankCode"] + "  " + layout["BankName"];
                    return Convert.ToInt32(layout["BankID"]);
                }
            }
            return 0;
        }

        /// <summary>Parse the uploaded statement using the bank's saved column
        /// layout and insert one fin_bankstatementlines row per transaction.
        /// Works for both XLSX (direct) and PDF (via PdfStatementExtractor).
        /// Exact-duplicate rows are silently skipped via INSERT IGNORE.</summary>
        private void ParseAndStore(int bankId, DataRow layout, string fileName,
                                    byte[] fileBytes, bool isPdf, string detectedBankName)
        {
            // Pick the column set matching the file format. Both sets live in fin_banklayouts,
            // named *Col for XLSX and Pdf*Col for PDF.
            string prefix = isPdf ? "Pdf" : "";
            int headerRow    = layout[prefix + "HeaderRow"]    == DBNull.Value ? 1 : Convert.ToInt32(layout[prefix + "HeaderRow"]);
            int firstDataRow = layout[prefix + "FirstDataRow"] == DBNull.Value ? headerRow + 1 : Convert.ToInt32(layout[prefix + "FirstDataRow"]);
            string dateCol   = GetLayoutCol(layout, prefix + "DateCol");
            string descCol   = GetLayoutCol(layout, prefix + "DescCol");
            string refCol    = GetLayoutCol(layout, prefix + "RefCol");
            string mode      = layout[prefix + "AmountMode"] == DBNull.Value ? "TWO_COL" : layout[prefix + "AmountMode"].ToString();
            string debCol    = GetLayoutCol(layout, prefix + "DebitCol");
            string crdCol    = GetLayoutCol(layout, prefix + "CreditCol");
            string amtCol    = GetLayoutCol(layout, prefix + "AmountCol");
            string flagCol   = GetLayoutCol(layout, prefix + "FlagCol");
            string balCol    = GetLayoutCol(layout, prefix + "BalanceCol");
            string dateFmt   = layout[prefix + "DateFormat"] == DBNull.Value ? "dd/MM/yyyy" : layout[prefix + "DateFormat"].ToString();

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

            // Build a unified string grid for either file type. First-index = row
            // (1-based), second-index = column (1-based). A cell that doesn't
            // exist is treated as empty string.
            List<string[]> grid;
            if (isPdf)
            {
                // PDF path: iText7 extracts text spans, cluster by Y (rows) and X
                // (columns), producing left-to-right column letters A, B, C...
                var pdfGrid = PdfStatementExtractor.Extract(fileBytes);
                grid = new List<string[]>();
                foreach (var row in pdfGrid) grid.Add(row.ToArray());
            }
            else
            {
                // XLSX path: read via ClosedXML as before.
                grid = new List<string[]>();
                using (var stream = new MemoryStream(fileBytes))
                using (var wb = new XLWorkbook(stream))
                {
                    var ws = wb.Worksheet(1);
                    int lastRow = ws.LastRowUsed() != null ? ws.LastRowUsed().RowNumber() : 0;
                    int lastCol = ws.LastColumnUsed() != null ? ws.LastColumnUsed().ColumnNumber() : 0;
                    for (int r = 1; r <= lastRow; r++)
                    {
                        var rowArr = new string[lastCol];
                        for (int col = 1; col <= lastCol; col++)
                        {
                            var cell = ws.Cell(r, col);
                            // For dates, preserve formatted value so TryReadDateText() works
                            if (cell.DataType == XLDataType.DateTime)
                                rowArr[col - 1] = cell.GetDateTime().ToString("yyyy-MM-dd");
                            else
                                rowArr[col - 1] = cell.GetString() ?? "";
                        }
                        grid.Add(rowArr);
                    }
                }
            }

            // First pass: walk the grid, collect rows so we can compute period + balances first.
            var rows = new System.Collections.Generic.List<ParsedLine>();
            DateTime? periodStart = null, periodEnd = null;
            decimal? openBal = null, closeBal = null;

            int seq = 0;
            // Grid is 0-indexed internally; layout rows are 1-based. Start at firstDataRow - 1.
            for (int r = firstDataRow - 1; r < grid.Count; r++)
            {
                DateTime txnDate;
                if (!TryReadDateText(CellAt(grid, r, dateIdx), dateFmt, out txnDate)) continue;

                decimal debit = 0m, credit = 0m;
                if (mode == "TWO_COL")
                {
                    debit  = ParseDecimalText(CellAt(grid, r, debIdx));
                    credit = ParseDecimalText(CellAt(grid, r, crdIdx));
                }
                else if (mode == "FLAG")
                {
                    decimal amt = ParseDecimalText(CellAt(grid, r, amtIdx));
                    string flag = (CellAt(grid, r, flagIdx) ?? "").Trim().ToUpperInvariant();
                    if (flag.StartsWith("DR") || flag.Contains("DEBIT"))      debit = Math.Abs(amt);
                    else if (flag.StartsWith("CR") || flag.Contains("CREDIT")) credit = Math.Abs(amt);
                    else continue;
                }
                else if (mode == "SIGNED")
                {
                    decimal amt = ParseDecimalText(CellAt(grid, r, amtIdx));
                    if (amt < 0) debit = -amt;
                    else if (amt > 0) credit = amt;
                    else continue;
                }

                if (debit == 0 && credit == 0) continue;

                string description = CellAt(grid, r, descIdx);
                string reference   = CellAt(grid, r, refIdx);
                decimal? balance   = balIdx > 0 ? (decimal?)ParseDecimalText(CellAt(grid, r, balIdx)) : null;

                seq++;
                rows.Add(new ParsedLine {
                    Seq = seq, Date = txnDate,
                    Description = description == null ? "" : description.Trim(),
                    Reference   = reference   == null ? "" : reference.Trim(),
                    Debit = debit, Credit = credit, Balance = balance
                });

                if (periodStart == null || txnDate < periodStart) periodStart = txnDate;
                if (periodEnd   == null || txnDate > periodEnd)   periodEnd   = txnDate;
            }

            if (rows.Count == 0)
            {
                ShowAlert("No data rows recognised. The parser could not match your layout to rows in the file. For PDFs, verify the file is native-text (not scanned). For XLSX, check the column letters in the bank layout.", "danger");
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

            string msg = "";
            if (!string.IsNullOrEmpty(detectedBankName))
                msg = "Auto-detected as <b>" + detectedBankName + "</b>. ";
            msg += "Inserted " + inserted + " new rows";
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

        /// <summary>Fetch a cell from the grid (1-based col). Returns empty
        /// if the cell is out of bounds, which can happen when PDFs are sparse.</summary>
        private static string CellAt(List<string[]> grid, int rowZeroBased, int colOneBased)
        {
            if (colOneBased <= 0) return "";
            if (rowZeroBased < 0 || rowZeroBased >= grid.Count) return "";
            var row = grid[rowZeroBased];
            if (colOneBased > row.Length) return "";
            return row[colOneBased - 1] ?? "";
        }

        /// <summary>Parse a decimal from a cell string. Handles Indian currency
        /// formats (with comma separators, rupee symbols), CR/DR suffixes, and
        /// parentheses (accounting negative).</summary>
        private static decimal ParseDecimalText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0m;
            string s = text.Trim();
            // Strip CR / DR suffixes &mdash; caller handles sign separately
            s = System.Text.RegularExpressions.Regex.Replace(s, @"\s*(CR|DR|Cr|Dr|cr|dr)\s*$", "").Trim();
            // Accounting negative: (1,234.56) -> -1234.56
            bool negParen = s.StartsWith("(") && s.EndsWith(")");
            if (negParen) s = s.Substring(1, s.Length - 2);
            s = s.Replace(",", "").Replace("\u20B9", "").Replace("Rs.", "").Replace("Rs", "").Trim();
            if (string.IsNullOrEmpty(s)) return 0m;
            decimal d;
            if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d))
                return negParen ? -d : d;
            return 0m;
        }

        /// <summary>Text-based date reader used by the unified grid parser.
        /// Tries the layout's configured format first, then a list of common
        /// Indian bank formats.</summary>
        private static bool TryReadDateText(string text, string format, out DateTime result)
        {
            result = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(text)) return false;
            string s = text.Trim();
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            if (DateTime.TryParseExact(s, format, inv, System.Globalization.DateTimeStyles.None, out result)) return true;
            string[] tryFormats = new[] {
                "yyyy-MM-dd", "dd/MM/yyyy", "dd-MM-yyyy", "dd-MMM-yyyy", "dd/MMM/yyyy",
                "dd/MM/yy", "dd-MM-yy", "d/M/yyyy", "d-MMM-yyyy", "dd.MM.yyyy",
                "dd MMM yyyy", "dd-MMM-yy"
            };
            foreach (var f in tryFormats)
                if (DateTime.TryParseExact(s, f, inv, System.Globalization.DateTimeStyles.None, out result)) return true;
            return DateTime.TryParse(s, out result);
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

        // ══════════════════════════════════════════════════════════════
        //  POST BANK LINE AS JV — inline modal flow
        // ══════════════════════════════════════════════════════════════

        /// <summary>Format a JournalID as "JV-2627-0013" text for the HyperLink on Posted lines.</summary>
        protected string FormatJvLink(object journalIdObj)
        {
            if (journalIdObj == null || journalIdObj == DBNull.Value) return "";
            int jid = Convert.ToInt32(journalIdObj);
            if (jid <= 0) return "";
            string num = FINDatabaseHelper.GetJournalNumberById(jid);
            return string.IsNullOrEmpty(num) ? ("JV-" + jid) : num;
        }

        /// <summary>Repeater item-command handler for Post link clicks.</summary>
        protected void rptLines_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "PostLine")
            {
                ShowAlert("Got unexpected CommandName: " + e.CommandName, "warning");
                return;
            }
            string argStr = Convert.ToString(e.CommandArgument);
            long lineId;
            if (!long.TryParse(argStr, out lineId))
            {
                ShowAlert("Bank line ID missing from the Post click (raw value: '" + argStr + "'). Please refresh and try again.", "danger");
                return;
            }
            OpenPostModal(lineId);
        }

        /// <summary>Open the Post modal for a given bank line. Populates summary,
        /// JV preview, and party dropdown with auto-suggestions.</summary>
        private void OpenPostModal(long lineId)
        {
            DataRow line = FINDatabaseHelper.GetBankLineById(lineId);
            if (line == null)
            {
                ShowAlert("Bank line not found.", "danger");
                return;
            }

            if (line["Status"] != DBNull.Value && line["Status"].ToString() == "Posted")
            {
                ShowAlert("This line has already been posted.", "danger");
                return;
            }

            hfPostLineID.Value = lineId.ToString();

            // Summary
            DateTime txnDate = Convert.ToDateTime(line["TxnDate"]);
            decimal debit   = line["Debit"]  == DBNull.Value ? 0m : Convert.ToDecimal(line["Debit"]);
            decimal credit  = line["Credit"] == DBNull.Value ? 0m : Convert.ToDecimal(line["Credit"]);
            string desc     = line["Description"] == DBNull.Value ? "" : line["Description"].ToString();

            lblPostDate.Text = txnDate.ToString("dd MMM yyyy");
            lblPostDesc.Text = System.Web.HttpUtility.HtmlEncode(desc);

            bool isWithdrawal = debit > 0;
            decimal amount = isWithdrawal ? debit : credit;
            lblPostAmount.Text    = amount.ToString("N2");
            lblPostDirection.Text = isWithdrawal ? "Withdrawal (money out)" : "Deposit (money in)";

            // JV preview — show the account labels
            string apAcc = FINDatabaseHelper.GetAccountsPayableZohoId();
            string arAcc = FINDatabaseHelper.GetAccountsReceivableZohoId();
            string bankAccName = FINDatabaseHelper.GetBankAccountName(Convert.ToInt32(line["BankID"]));
            string apName = string.IsNullOrEmpty(apAcc) ? "Accounts Payable (not in COA)"    : "Accounts Payable";
            string arName = string.IsNullOrEmpty(arAcc) ? "Accounts Receivable (not in COA)" : "Accounts Receivable";

            if (isWithdrawal)
            {
                lblJvLine1Account.Text = apName;
                lblJvLine1Party.Text   = "(select below)";
                lblJvLine1Debit.Text   = amount.ToString("N2");
                lblJvLine1Credit.Text  = "";
                lblJvLine2Account.Text = bankAccName;
                lblJvLine2Party.Text   = "";
                lblJvLine2Debit.Text   = "";
                lblJvLine2Credit.Text  = amount.ToString("N2");
            }
            else
            {
                lblJvLine1Account.Text = bankAccName;
                lblJvLine1Party.Text   = "";
                lblJvLine1Debit.Text   = amount.ToString("N2");
                lblJvLine1Credit.Text  = "";
                lblJvLine2Account.Text = arName;
                lblJvLine2Party.Text   = "(select below)";
                lblJvLine2Debit.Text   = "";
                lblJvLine2Credit.Text  = amount.ToString("N2");
            }

            PopulatePartyDropdown(desc, isWithdrawal);
            pnlPostModal.Visible = true;
        }

        /// <summary>Populate ddlPostParty with filtered list and preselect auto-suggested party.</summary>
        private void PopulatePartyDropdown(string description, bool isWithdrawal)
        {
            // Suppliers for withdrawals, Customers for deposits
            string targetType = isWithdrawal ? "SUP" : "CUS";
            DataTable allParties = FINDatabaseHelper.GetPartyList();

            ddlPostParty.Items.Clear();
            ddlPostParty.Items.Add(new ListItem("-- Select " + (isWithdrawal ? "Supplier" : "Customer") + " --", ""));

            foreach (DataRow r in allParties.Rows)
            {
                if (r["PartyType"].ToString() != targetType) continue;
                string pkey  = r["PartyKey"].ToString();
                string pname = r["PartyName"].ToString();
                ddlPostParty.Items.Add(new ListItem(pname, pkey));
            }

            // Auto-suggest
            DataTable suggestions = FINDatabaseHelper.SuggestPartiesForDescription(description, 3);
            string suggestedKey = null;
            foreach (DataRow s in suggestions.Rows)
            {
                if (s["PartyType"].ToString() == targetType)
                {
                    suggestedKey = s["PartyKey"].ToString();
                    break;
                }
            }
            if (suggestedKey != null)
            {
                var it = ddlPostParty.Items.FindByValue(suggestedKey);
                if (it != null)
                {
                    ddlPostParty.SelectedValue = suggestedKey;
                    lblSuggestHint.Text = "Auto-suggested from description. Confirm or choose differently.";
                }
                else
                {
                    lblSuggestHint.Text = "";
                }
            }
            else
            {
                lblSuggestHint.Text = "";
            }
        }

        protected void lnkCloseModal_Click(object sender, EventArgs e)
        {
            pnlPostModal.Visible = false;
        }

        protected void btnConfirmPost_Click(object sender, EventArgs e)
        {
            long lineId;
            if (!long.TryParse(hfPostLineID.Value, out lineId))
            {
                ShowAlert("Missing bank line reference.", "danger");
                return;
            }

            string partyKey = ddlPostParty.SelectedValue;
            if (string.IsNullOrEmpty(partyKey))
            {
                ShowAlert("Please select the party before posting.", "danger");
                return;
            }

            int userId = Convert.ToInt32(Session["FIN_UserID"] ?? 0);
            if (userId == 0)
            {
                ShowAlert("Session expired — please sign in again.", "danger");
                return;
            }

            try
            {
                int journalId = FINDatabaseHelper.SaveJournalFromBankLine(lineId, partyKey, userId);
                pnlPostModal.Visible = false;
                ShowAlert("Journal created successfully. Status updated to Posted.", "success");
                // Refresh the current statement's lines
                if (ViewState["StatementID"] != null)
                {
                    int stmtId = Convert.ToInt32(ViewState["StatementID"]);
                    rptLines.DataSource = FINDatabaseHelper.GetStatementLines(stmtId);
                    rptLines.DataBind();
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Could not post: " + ex.Message, "danger");
            }
        }

    }
}
