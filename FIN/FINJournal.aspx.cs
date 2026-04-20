using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using FINApp.DAL;

namespace FINApp
{
    public partial class FINJournal : System.Web.UI.Page
    {
        // Server controls declared in the .aspx markup
        protected Label lblNavUser;
        // List mode
        protected Panel pnlList, pnlDetail;
        protected Literal litDraftCount, litPostedCount, litReversedCount;
        protected TextBox txtFromDate, txtToDate, txtNumber;
        protected DropDownList ddlStatus;
        protected LinkButton btnNewJournal, btnFilter;
        protected PlaceHolder phBanner, phList;
        // Detail mode
        protected Literal litHeadLabel, litJournalNumber, litStatusBadge, litMeta;
        protected PlaceHolder phReversalNotice, phLines;
        protected TextBox txtJournalDate, txtNarration, txtReference;
        protected LinkButton btnAddLine, btnCancel, btnSaveDraft, btnPost, btnDelete, btnReverse;
        protected Literal litTotalDebit, litTotalCredit, litBalance;
        // Zoho push (detail mode)
        protected Panel pnlZohoSection;
        protected Literal litZohoStatus;
        protected LinkButton btnPushToZoho, btnRepushToZoho;
        // Print (detail mode)
        protected HyperLink lnkPrintVoucher;

        // ── mode / state ──
        // Mode is derived from query string: no id → list, id=NEW → new entry, id=<int> → edit/view existing.
        // ViewState["LineCount"] tracks the dynamic-row count in new/draft mode so we can
        // rebuild controls on every postback (required for event dispatch on ASP.NET dynamic controls).

        int JournalID
        {
            get
            {
                int id;
                return int.TryParse(Request.QueryString["id"] ?? "", out id) ? id : 0;
            }
        }
        bool IsNewMode => string.Equals(Request.QueryString["id"], "NEW", StringComparison.OrdinalIgnoreCase);
        bool IsDetailMode => IsNewMode || JournalID > 0;

        bool IsFinance => FINConsignments.IsFinanceRole(Session["FIN_Role"]?.ToString() ?? "");
        int CurrentUserId => Convert.ToInt32(Session["FIN_UserID"]);

        int LineCount
        {
            get { return ViewState["LineCount"] is int n ? n : 0; }
            set { ViewState["LineCount"] = value; }
        }

        // We cache chart of accounts for a single request to avoid re-querying for every row
        DataTable _chartCache;
        DataTable Chart
        {
            get
            {
                if (_chartCache == null)
                    _chartCache = FINDatabaseHelper.GetChartOfAccounts(activeOnly: true);
                return _chartCache;
            }
        }

        // Same idea for party list — 2400+ rows but we only query once per request
        DataTable _partyCache;
        DataTable Parties
        {
            get
            {
                if (_partyCache == null)
                    _partyCache = FINDatabaseHelper.GetPartyList();
                return _partyCache;
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  PAGE LIFECYCLE
        // ══════════════════════════════════════════════════════════════
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["FIN_UserID"] == null)
            {
                Response.Redirect("FINLogin.aspx?returnUrl=" + Server.UrlEncode(Request.RawUrl));
                return;
            }

            if (lblNavUser != null)
                lblNavUser.Text = Session["FIN_FullName"]?.ToString() ?? "";

            if (IsDetailMode)
            {
                pnlList.Visible = false;
                pnlDetail.Visible = true;

                if (!IsPostBack)
                    InitDetail();

                // Rebuild dynamic line rows on every postback so the controls exist for event dispatch
                RenderLineRows();
                RefreshTotals();
                WireDetailActionButtons();
                RefreshZohoSection();
            }
            else
            {
                pnlList.Visible = true;
                pnlDetail.Visible = false;

                if (!IsPostBack)
                {
                    txtFromDate.Text = DateTime.Today.AddDays(-60).ToString("yyyy-MM-dd");
                    txtToDate.Text   = DateTime.Today.ToString("yyyy-MM-dd");
                }
                RefreshStatusCounts();
                RenderList();
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  LIST MODE
        // ══════════════════════════════════════════════════════════════
        void RefreshStatusCounts()
        {
            var dt = FINDatabaseHelper.GetJournalStatusCounts();
            int d = 0, p = 0, r = 0;
            foreach (DataRow row in dt.Rows)
            {
                int c = Convert.ToInt32(row["Cnt"]);
                switch (row["Status"].ToString())
                {
                    case "DRAFT": d = c; break;
                    case "POSTED": p = c; break;
                    case "REVERSED": r = c; break;
                }
            }
            litDraftCount.Text = d.ToString();
            litPostedCount.Text = p.ToString();
            litReversedCount.Text = r.ToString();
        }

        protected void btnFilter_Click(object s, EventArgs e) { RenderList(); }

        protected void btnNewJournal_Click(object s, EventArgs e)
        {
            if (!IsFinance)
            {
                ShowBanner("Only Finance / Super users can create journals.", "error");
                return;
            }
            Response.Redirect("FINJournal.aspx?id=NEW");
        }

        void RenderList()
        {
            phList.Controls.Clear();

            DateTime? from = null, to = null;
            DateTime tmp;
            if (DateTime.TryParse(txtFromDate.Text, out tmp)) from = tmp;
            if (DateTime.TryParse(txtToDate.Text, out tmp))   to = tmp;
            string status = ddlStatus.SelectedValue;
            string numberQ = (txtNumber.Text ?? "").Trim();

            var dt = FINDatabaseHelper.GetJournalList(from, to, status, numberQ);

            if (dt.Rows.Count == 0)
            {
                var empty = new Panel { CssClass = "tbl" };
                empty.Controls.Add(new LiteralControl(
                    "<div class='empty-state'>" +
                    "<strong>No journal entries found</strong>" +
                    (FINDatabaseHelper.GetChartOfAccounts(activeOnly: false).Rows.Count == 0
                        ? "Sync the <a class='link' href='FINChartOfAccounts.aspx'>chart of accounts</a> first, then create your first entry."
                        : "Click <b>+ New Journal Entry</b> above to create one.") +
                    "</div>"));
                phList.Controls.Add(empty);
                return;
            }

            var tbl = new Table { CssClass = "tbl" };
            var hdr = new TableHeaderRow();
            foreach (var h in new[] { "Number", "Date", "Narration", "Debit", "Credit", "Status", "Zoho", "Action" })
            {
                var cell = new TableHeaderCell();
                cell.Controls.Add(new LiteralControl(h));
                if (h == "Number") cell.CssClass = "col-num";
                else if (h == "Date") cell.CssClass = "col-date";
                else if (h == "Debit" || h == "Credit") cell.CssClass = "col-total";
                else if (h == "Status") cell.CssClass = "col-status";
                else if (h == "Zoho") cell.CssClass = "col-zoho";
                else if (h == "Action") cell.CssClass = "col-act";
                hdr.Cells.Add(cell);
            }
            tbl.Rows.Add(hdr);

            // Batch-fetch Zoho push status for every journal on this page so we can
            // decorate rows without doing an extra query per row.
            var jidsOnPage = new System.Collections.Generic.List<int>();
            foreach (DataRow r in dt.Rows)
                jidsOnPage.Add(Convert.ToInt32(r["JournalID"]));
            var zohoLogs = FINDatabaseHelper.GetJournalLogsForList(jidsOnPage);

            foreach (DataRow r in dt.Rows)
            {
                int jid = Convert.ToInt32(r["JournalID"]);
                string num = r["JournalNumber"].ToString();
                DateTime jdate = Convert.ToDateTime(r["JournalDate"]);
                string narr = r["Narration"] == DBNull.Value ? "" : r["Narration"].ToString();
                decimal td = Convert.ToDecimal(r["TotalDebit"]);
                decimal tc = Convert.ToDecimal(r["TotalCredit"]);
                string st = r["Status"].ToString();

                var tr = new TableRow();

                var cNum = new TableCell { CssClass = "col-num" };
                cNum.Text = "<a class='link' href='FINJournal.aspx?id=" + jid + "'>" + Server.HtmlEncode(num) + "</a>";
                tr.Cells.Add(cNum);

                var cDate = new TableCell { CssClass = "col-date" };
                cDate.Text = jdate.ToString("dd MMM yyyy");
                tr.Cells.Add(cDate);

                var cNarr = new TableCell();
                cNarr.Text = Server.HtmlEncode(narr.Length > 80 ? narr.Substring(0, 80) + "…" : narr);
                tr.Cells.Add(cNarr);

                var cDr = new TableCell { CssClass = "col-total" };
                cDr.Text = td.ToString("N2");
                tr.Cells.Add(cDr);

                var cCr = new TableCell { CssClass = "col-total" };
                cCr.Text = tc.ToString("N2");
                tr.Cells.Add(cCr);

                var cSt = new TableCell { CssClass = "col-status" };
                cSt.Text = StatusBadge(st);
                tr.Cells.Add(cSt);

                var cZoho = new TableCell { CssClass = "col-zoho" };
                cZoho.Text = ZohoChip(zohoLogs.ContainsKey(jid) ? zohoLogs[jid] : null);
                tr.Cells.Add(cZoho);

                var cAct = new TableCell { CssClass = "col-act" };
                string verb = st == "DRAFT" ? "Edit" : "View";
                cAct.Text = "<a class='link' href='FINJournal.aspx?id=" + jid + "'>" + verb + " &rarr;</a>";
                tr.Cells.Add(cAct);

                tbl.Rows.Add(tr);
            }
            phList.Controls.Add(tbl);
        }

        string StatusBadge(string status)
        {
            switch (status)
            {
                case "DRAFT":    return "<span class='badge badge-draft'>Draft</span>";
                case "POSTED":   return "<span class='badge badge-posted'>Posted</span>";
                case "REVERSED": return "<span class='badge badge-reversed'>Reversed</span>";
                default:         return "<span class='badge badge-draft'>" + Server.HtmlEncode(status) + "</span>";
            }
        }

        /// <summary>
        /// Render a Zoho push status chip from a zoho_journallog row (may be null).
        /// States:
        ///   (no log row)                  → "—" (not pushed)
        ///   PushStatus='Pushed' + ZohoID  → green "Zoho: <entry#>"
        ///   PushStatus='Error'            → red "Error"
        ///   (other / pending)             → grey "—"
        /// </summary>
        string ZohoChip(DataRow logRow)
        {
            if (logRow == null)
                return "<span class='badge badge-zoho-notpushed'>—</span>";

            string pushStatus = logRow["PushStatus"].ToString();
            string zohoId = logRow["ZohoJournalID"] == DBNull.Value ? "" : logRow["ZohoJournalID"].ToString();

            if (pushStatus == "Pushed" && !string.IsNullOrEmpty(zohoId))
            {
                string zno = logRow["ZohoJournalNo"] == DBNull.Value ? "" : logRow["ZohoJournalNo"].ToString();
                string label = string.IsNullOrEmpty(zno) ? "Pushed" : Server.HtmlEncode(zno);
                return "<span class='badge badge-zoho-pushed' title='Pushed to Zoho Books'>" + label + "</span>";
            }
            if (pushStatus == "Error")
                return "<span class='badge badge-zoho-error' title='Push failed — open the journal for details'>Error</span>";

            return "<span class='badge badge-zoho-notpushed'>—</span>";
        }

        /// <summary>
        /// Populate the "Zoho Books" section inside the detail card.
        /// Shows the current push state (never pushed / pushed / error),
        /// exposes the Push / Retry buttons via WireDetailActionButtons.
        /// Only meaningful for POSTED journals — hidden for DRAFT/NEW/REVERSED.
        /// </summary>
        void RefreshZohoSection()
        {
            string st = CurrentStatus();
            // Only POSTED journals can be pushed. DRAFT: no point. REVERSED:
            // the contra is its own journal and is pushed separately; we
            // still show the section so the user sees this one was already
            // handled before reversal (if it was).
            if (st != "POSTED" && st != "REVERSED")
            {
                pnlZohoSection.Visible = false;
                return;
            }

            DataRow log = FINDatabaseHelper.GetJournalLog(JournalID);

            if (log == null)
            {
                if (st == "POSTED")
                {
                    pnlZohoSection.Visible = true;
                    litZohoStatus.Text = "<span class='badge badge-zoho-notpushed'>Not pushed</span> "
                        + "&nbsp; This journal has not been sent to Zoho Books yet.";
                }
                else
                {
                    pnlZohoSection.Visible = false;
                }
                return;
            }

            string pushStatus = log["PushStatus"].ToString();
            string zohoId = log["ZohoJournalID"] == DBNull.Value ? "" : log["ZohoJournalID"].ToString();
            string zohoNo = log["ZohoJournalNo"] == DBNull.Value ? "" : log["ZohoJournalNo"].ToString();
            string pushedAt = log["PushedAt"] == DBNull.Value
                ? "" : Convert.ToDateTime(log["PushedAt"]).ToString("dd MMM yyyy HH:mm");

            pnlZohoSection.Visible = true;
            var sb = new System.Text.StringBuilder();

            if (pushStatus == "Pushed" && !string.IsNullOrEmpty(zohoId))
            {
                sb.Append("<span class='badge badge-zoho-pushed'>Pushed</span> &nbsp; ");
                sb.Append("Zoho entry <b>").Append(Server.HtmlEncode(zohoNo)).Append("</b> ");
                if (!string.IsNullOrEmpty(pushedAt))
                    sb.Append("on ").Append(pushedAt).Append(". ");
                sb.Append("<span style='color:var(--text-dim);'>(id: ").Append(Server.HtmlEncode(zohoId)).Append(")</span>");
            }
            else if (pushStatus == "Error")
            {
                sb.Append("<span class='badge badge-zoho-error'>Error</span> &nbsp; ");
                sb.Append("Last attempt ");
                if (!string.IsNullOrEmpty(pushedAt)) sb.Append("at ").Append(pushedAt).Append(" ");
                sb.Append("failed.");
                string errMsg = log["ErrorMessage"] == DBNull.Value ? "" : log["ErrorMessage"].ToString();
                if (!string.IsNullOrEmpty(errMsg))
                    sb.Append("<span class='zoho-error'>").Append(Server.HtmlEncode(errMsg)).Append("</span>");
            }
            else
            {
                sb.Append("<span class='badge badge-zoho-notpushed'>").Append(Server.HtmlEncode(pushStatus)).Append("</span>");
            }

            litZohoStatus.Text = sb.ToString();
        }

        // ══════════════════════════════════════════════════════════════
        //  DETAIL MODE — initial render
        // ══════════════════════════════════════════════════════════════
        void InitDetail()
        {
            if (FINDatabaseHelper.GetChartOfAccounts(activeOnly: false).Rows.Count == 0)
            {
                ShowBanner(
                    "Chart of accounts is empty. Please sync it from Zoho first at the Chart of Accounts page.",
                    "error");
            }

            if (IsNewMode)
            {
                litHeadLabel.Text = "New Journal Entry";
                litJournalNumber.Text = "(auto-generated on save)";
                litStatusBadge.Text = StatusBadge("DRAFT");
                txtJournalDate.Text = DateTime.Today.ToString("yyyy-MM-dd");

                // Start with 2 empty lines — minimum for a double-entry
                LineCount = 2;
                ClearViewStateLines();
                return;
            }

            // Existing journal — load header + lines
            var ds = FINDatabaseHelper.GetJournalDetail(JournalID);
            if (ds.Tables["Header"].Rows.Count == 0)
            {
                ShowBanner("Journal #" + JournalID + " not found.", "error");
                pnlDetail.Visible = false;
                pnlList.Visible = true;
                return;
            }

            var h = ds.Tables["Header"].Rows[0];
            string status = h["Status"].ToString();

            litHeadLabel.Text = status == "DRAFT" ? "Draft Journal Entry"
                              : status == "POSTED" ? "Posted Journal Entry"
                              : "Reversed Journal Entry";
            litJournalNumber.Text = h["JournalNumber"].ToString();
            litStatusBadge.Text = StatusBadge(status);
            txtJournalDate.Text = Convert.ToDateTime(h["JournalDate"]).ToString("yyyy-MM-dd");
            txtNarration.Text = h["Narration"] == DBNull.Value ? "" : h["Narration"].ToString();
            txtReference.Text = h["Reference"] == DBNull.Value ? "" : h["Reference"].ToString();

            var metaParts = new List<string>();
            metaParts.Add("Created by " + (h["CreatedByName"] == DBNull.Value ? "—" : h["CreatedByName"].ToString())
                          + " on " + Convert.ToDateTime(h["CreatedAt"]).ToString("dd MMM yyyy HH:mm"));
            if (h["PostedAt"] != DBNull.Value)
                metaParts.Add("Posted by " + (h["PostedByName"] == DBNull.Value ? "—" : h["PostedByName"].ToString())
                              + " on " + Convert.ToDateTime(h["PostedAt"]).ToString("dd MMM yyyy HH:mm"));
            if (h["ReversedAt"] != DBNull.Value)
                metaParts.Add("Reversed by " + (h["ReversedByName"] == DBNull.Value ? "—" : h["ReversedByName"].ToString())
                              + " on " + Convert.ToDateTime(h["ReversedAt"]).ToString("dd MMM yyyy HH:mm"));
            litMeta.Text = string.Join(" · ", metaParts);

            if (status == "REVERSED" && h["ReversedByJournalID"] != DBNull.Value)
            {
                int byId = Convert.ToInt32(h["ReversedByJournalID"]);
                phReversalNotice.Controls.Add(new LiteralControl(
                    "<div class='reversal-notice'>This entry was reversed by " +
                    "<a href='FINJournal.aspx?id=" + byId + "'>contra journal #" + byId + "</a>. " +
                    "The contra flips every debit/credit and leaves the net effect on the ledger at zero.</div>"));
            }

            var lines = ds.Tables["Lines"];
            LineCount = lines.Rows.Count;
            ClearViewStateLines();
            for (int i = 0; i < lines.Rows.Count; i++)
            {
                var ln = lines.Rows[i];
                StoreLineViewState(i,
                    ln["ZohoAccountID"].ToString(),
                    Convert.ToDecimal(ln["Debit"]),
                    Convert.ToDecimal(ln["Credit"]),
                    ln["LineDescription"] == DBNull.Value ? "" : ln["LineDescription"].ToString(),
                    ln["ContactID"] == DBNull.Value ? "" : ln["ContactID"].ToString());
            }
        }

        // ── view state for line values (persists across postbacks, loaded into controls each render) ──
        void ClearViewStateLines()
        {
            var keys = ViewState.Keys.Cast<string>().Where(k => k.StartsWith("ln_")).ToList();
            foreach (var k in keys) ViewState.Remove(k);
        }
        void StoreLineViewState(int idx, string acc, decimal dr, decimal cr, string desc, string party)
        {
            ViewState["ln_acc_" + idx] = acc;
            ViewState["ln_dr_" + idx]  = dr;
            ViewState["ln_cr_" + idx]  = cr;
            ViewState["ln_desc_" + idx] = desc;
            ViewState["ln_party_" + idx] = party;
        }

        // ══════════════════════════════════════════════════════════════
        //  DETAIL MODE — render line rows
        // ══════════════════════════════════════════════════════════════
        void RenderLineRows()
        {
            phLines.Controls.Clear();
            bool editable = IsEditable();

            for (int i = 0; i < LineCount; i++)
            {
                // Pull current values: from form if posted, else from view state (first render / add-line)
                string accVal = GetPostedOrViewState("acc", i);
                string drVal  = GetPostedOrViewState("dr",  i);
                string crVal  = GetPostedOrViewState("cr",  i);
                string descVal = GetPostedOrViewState("desc", i);
                string partyVal = GetPostedOrViewState("party", i);

                // Outer container — contains the main row (with party) and description row below
                var row = new Panel { CssClass = "line-row" };

                // ── Main row: # | Account | Debit | Credit | Party | × ──
                var main = new Panel { CssClass = "line-main" };
                main.Controls.Add(new LiteralControl("<div class='idx'>" + (i + 1) + "</div>"));

                // Account dropdown
                var ddl = new DropDownList { ID = "ln_acc_" + i };
                ddl.Enabled = editable;
                ddl.Items.Add(new ListItem("— select account —", ""));
                foreach (DataRow r in Chart.Rows)
                {
                    string code = (r["AccountCode"] ?? "").ToString();
                    string name = (r["AccountName"] ?? "").ToString();
                    string label = string.IsNullOrEmpty(code) ? name : (name + " (" + code + ")");
                    ddl.Items.Add(new ListItem(label, r["ZohoAccountID"].ToString()));
                }
                if (!string.IsNullOrEmpty(accVal) && ddl.Items.FindByValue(accVal) != null)
                    ddl.SelectedValue = accVal;
                main.Controls.Add(ddl);

                var txtDr = new TextBox { ID = "ln_dr_" + i, CssClass = "num" };
                txtDr.Text = FormatMoney(drVal);
                txtDr.ReadOnly = !editable;
                main.Controls.Add(txtDr);

                var txtCr = new TextBox { ID = "ln_cr_" + i, CssClass = "num" };
                txtCr.Text = FormatMoney(crVal);
                txtCr.ReadOnly = !editable;
                main.Controls.Add(txtCr);

                // ── Party dropdown: combined Suppliers + Customers list ──
                if (editable)
                {
                    var ddlParty = new DropDownList { ID = "ln_party_" + i };
                    ddlParty.Items.Add(new ListItem("— none —", ""));
                    foreach (DataRow r in Parties.Rows)
                    {
                        string pkey = r["PartyKey"].ToString();
                        string ptype = r["PartyType"].ToString();      // SUP or CUS
                        string pname = (r["PartyName"] ?? "").ToString();
                        string label = "[" + ptype + "] " + pname;
                        ddlParty.Items.Add(new ListItem(label, pkey));
                    }
                    if (!string.IsNullOrEmpty(partyVal) && ddlParty.Items.FindByValue(partyVal) != null)
                        ddlParty.SelectedValue = partyVal;
                    main.Controls.Add(ddlParty);
                }
                else
                {
                    // Read-only: show resolved party name as plain text.
                    // Resolve via the in-memory Parties cache (already loaded for this request).
                    string partyName = "";
                    if (!string.IsNullOrEmpty(partyVal))
                    {
                        var found = Parties.Select("PartyKey = '" + partyVal.Replace("'", "''") + "'");
                        if (found.Length > 0)
                            partyName = "[" + found[0]["PartyType"] + "] " + found[0]["PartyName"];
                        else
                            partyName = FINDatabaseHelper.GetPartyDisplayName(partyVal);  // fallback for inactive parties
                    }
                    var readOnlyParty = new TextBox { ID = "ln_party_" + i };
                    readOnlyParty.Text = partyName;
                    readOnlyParty.ReadOnly = true;
                    main.Controls.Add(readOnlyParty);
                }

                if (editable)
                {
                    var del = new LinkButton
                    {
                        ID = "ln_del_" + i,
                        CssClass = "del-btn",
                        Text = "×",
                        CommandArgument = i.ToString(),
                        CausesValidation = false
                    };
                    del.Command += LineDelete_Command;
                    main.Controls.Add(del);
                }
                else
                {
                    main.Controls.Add(new LiteralControl("<div></div>"));
                }

                row.Controls.Add(main);

                // ── Second row: indented Description spanning wide ──
                var descWrap = new Panel { CssClass = "line-desc" };
                descWrap.Controls.Add(new LiteralControl("<div class='desc-lbl'>Description</div>"));
                var txtDesc = new TextBox { ID = "ln_desc_" + i };
                txtDesc.Text = descVal;
                txtDesc.ReadOnly = !editable;
                txtDesc.Attributes["placeholder"] = editable ? "optional — e.g. Monthly rent payment for Apr 2026" : "";
                descWrap.Controls.Add(txtDesc);
                row.Controls.Add(descWrap);

                phLines.Controls.Add(row);
            }

            // Emit a JS map of ZohoAccountID → AccountType so the client-side prompt
            // can highlight the Party field when a payable/receivable account is picked.
            // The script itself is defined once in the aspx; we only provide the data.
            if (editable)
            {
                var sb = new System.Text.StringBuilder();
                sb.Append("<script>window._acctTypes = {");
                bool first = true;
                foreach (DataRow r in Chart.Rows)
                {
                    if (!first) sb.Append(",");
                    first = false;
                    string zid = (r["ZohoAccountID"] ?? "").ToString().Replace("'", "\\'");
                    string atype = ((r["AccountTypeName"] ?? r["AccountType"] ?? "").ToString()).ToLowerInvariant().Replace("'", "\\'");
                    sb.Append("'").Append(zid).Append("':'").Append(atype).Append("'");
                }
                sb.Append("};if(window.wireJournalPartyPrompt)window.wireJournalPartyPrompt();</script>");
                phLines.Controls.Add(new LiteralControl(sb.ToString()));
            }
        }

        string GetPostedOrViewState(string field, int idx)
        {
            if (IsPostBack)
            {
                string key = "ln_" + field + "_" + idx;
                // ASP.NET naming may add prefixes — try direct match first, then scan
                if (Request.Form[key] != null) return Request.Form[key];
                foreach (string k in Request.Form.AllKeys)
                {
                    if (k != null && k.EndsWith("$" + key)) return Request.Form[k];
                    if (k != null && k.EndsWith("_" + key)) return Request.Form[k];
                }
            }
            object v = ViewState["ln_" + field + "_" + idx];
            return v == null ? "" : v.ToString();
        }

        string FormatMoney(string s)
        {
            decimal d;
            if (!decimal.TryParse(s ?? "", out d)) return "";
            if (d == 0) return "";
            return d.ToString("F2");
        }

        bool IsEditable()
        {
            if (!IsFinance) return false;
            if (IsNewMode) return true;
            return CurrentStatus() == "DRAFT";
        }

        string _cachedStatus;
        string CurrentStatus()
        {
            if (IsNewMode) return "NEW";
            if (JournalID == 0) return "NEW";
            if (_cachedStatus != null) return _cachedStatus;
            var ds = FINDatabaseHelper.GetJournalDetail(JournalID);
            if (ds.Tables["Header"].Rows.Count == 0) { _cachedStatus = "MISSING"; return _cachedStatus; }
            _cachedStatus = ds.Tables["Header"].Rows[0]["Status"].ToString();
            return _cachedStatus;
        }

        // ══════════════════════════════════════════════════════════════
        //  DETAIL MODE — totals + button visibility
        // ══════════════════════════════════════════════════════════════
        void RefreshTotals()
        {
            decimal td = 0, tc = 0;
            for (int i = 0; i < LineCount; i++)
            {
                decimal dr, cr;
                decimal.TryParse(GetPostedOrViewState("dr", i), out dr);
                decimal.TryParse(GetPostedOrViewState("cr", i), out cr);
                td += dr;
                tc += cr;
            }
            litTotalDebit.Text = td.ToString("N2");
            litTotalCredit.Text = tc.ToString("N2");

            decimal diff = td - tc;
            if (td == 0 && tc == 0)
                litBalance.Text = "<span class='bal'>—</span>";
            else if (diff == 0)
                litBalance.Text = "<span class='bal-ok'>&#x2713; Balanced</span>";
            else
                litBalance.Text = "<span class='bal-off'>&#x2717; Off by &#8377;" + Math.Abs(diff).ToString("N2") + "</span>";
        }

        void WireDetailActionButtons()
        {
            string st = CurrentStatus();

            // Visibility matrix:
            //   NEW      → Cancel, Save draft, Post
            //   DRAFT    → Cancel, Save draft, Post, Delete
            //   POSTED   → Cancel (= back to list), Reverse, [Push to Zoho | Retry Zoho push]
            //   REVERSED → Cancel (= back to list) — everything else hidden
            bool isNew = (st == "NEW");
            bool isDraft = (st == "DRAFT");
            bool isPosted = (st == "POSTED");
            bool isReversed = (st == "REVERSED");

            btnCancel.Visible    = true;
            btnSaveDraft.Visible = IsFinance && (isNew || isDraft);
            btnPost.Visible      = IsFinance && (isNew || isDraft);
            btnDelete.Visible    = IsFinance && isDraft;
            btnReverse.Visible   = IsFinance && isPosted;
            btnAddLine.Visible   = IsFinance && (isNew || isDraft);

            // Zoho push buttons — only for POSTED, only for Finance role
            btnPushToZoho.Visible = false;
            btnRepushToZoho.Visible = false;
            if (IsFinance && isPosted)
            {
                DataRow log = FINDatabaseHelper.GetJournalLog(JournalID);
                bool alreadyPushed = log != null
                    && log["ZohoJournalID"] != DBNull.Value
                    && !string.IsNullOrEmpty(log["ZohoJournalID"].ToString());
                bool priorError = log != null && log["PushStatus"].ToString() == "Error";

                if (alreadyPushed)
                {
                    // Already in Zoho — neither button. Chip in RefreshZohoSection tells the story.
                }
                else if (priorError)
                {
                    btnRepushToZoho.Visible = true;
                }
                else
                {
                    btnPushToZoho.Visible = true;
                }
            }

            // Print voucher — available for any POSTED or REVERSED journal, to any role
            // (it's a read-only print; no permissions beyond the base page auth).
            if (isPosted || isReversed)
            {
                lnkPrintVoucher.Visible = true;
                lnkPrintVoucher.NavigateUrl = "FINJournalPrint.aspx?id=" + JournalID;
            }
            else
            {
                lnkPrintVoucher.Visible = false;
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  DETAIL MODE — button handlers
        // ══════════════════════════════════════════════════════════════
        protected void btnAddLine_Click(object s, EventArgs e)
        {
            // Snapshot what's currently posted into view state before adding a new row,
            // otherwise the newly added row count would clobber current unsaved input.
            SnapshotPostedLinesToViewState();
            LineCount = LineCount + 1;
            // Fall through — Page_Load will call RenderLineRows with the new count
            RenderLineRows();
            RefreshTotals();
            WireDetailActionButtons();
        }

        protected void LineDelete_Command(object sender, CommandEventArgs e)
        {
            int idx;
            if (!int.TryParse(e.CommandArgument.ToString(), out idx)) return;
            if (LineCount <= 1) // keep at least one row
            {
                ShowBanner("A journal must have at least one line.", "warn");
                return;
            }

            // Snapshot current posted values, then shift rows up past the deleted index
            SnapshotPostedLinesToViewState();
            for (int i = idx; i < LineCount - 1; i++)
            {
                ViewState["ln_acc_" + i]   = ViewState["ln_acc_" + (i + 1)];
                ViewState["ln_dr_" + i]    = ViewState["ln_dr_" + (i + 1)];
                ViewState["ln_cr_" + i]    = ViewState["ln_cr_" + (i + 1)];
                ViewState["ln_desc_" + i]  = ViewState["ln_desc_" + (i + 1)];
                ViewState["ln_party_" + i] = ViewState["ln_party_" + (i + 1)];
            }
            // Remove the tail
            ViewState.Remove("ln_acc_" + (LineCount - 1));
            ViewState.Remove("ln_dr_" + (LineCount - 1));
            ViewState.Remove("ln_cr_" + (LineCount - 1));
            ViewState.Remove("ln_desc_" + (LineCount - 1));
            ViewState.Remove("ln_party_" + (LineCount - 1));
            LineCount = LineCount - 1;

            RenderLineRows();
            RefreshTotals();
            WireDetailActionButtons();
        }

        void SnapshotPostedLinesToViewState()
        {
            for (int i = 0; i < LineCount; i++)
            {
                ViewState["ln_acc_" + i]  = GetPostedOrViewState("acc", i);
                decimal dr, cr;
                decimal.TryParse(GetPostedOrViewState("dr", i), out dr);
                decimal.TryParse(GetPostedOrViewState("cr", i), out cr);
                ViewState["ln_dr_" + i]    = dr;
                ViewState["ln_cr_" + i]    = cr;
                ViewState["ln_desc_" + i]  = GetPostedOrViewState("desc", i);
                ViewState["ln_party_" + i] = GetPostedOrViewState("party", i);
            }
        }

        protected void btnCancel_Click(object s, EventArgs e)
        {
            Response.Redirect("FINJournal.aspx");
        }

        protected void btnSaveDraft_Click(object s, EventArgs e)
        {
            if (!IsFinance) { ShowBanner("Only Finance users can save journals.", "error"); return; }

            List<FINDatabaseHelper.JournalLineInput> lines;
            try { lines = CollectPostedLines(requireAllValid: false); }
            catch (Exception ex) { ShowBanner(ex.Message, "error"); return; }

            DateTime jdate;
            if (!DateTime.TryParse(txtJournalDate.Text, out jdate))
            { ShowBanner("Please enter a valid date.", "error"); return; }

            try
            {
                if (IsNewMode)
                {
                    int newId = FINDatabaseHelper.SaveJournalAsDraft(
                        jdate, txtNarration.Text?.Trim(), txtReference.Text?.Trim(),
                        lines, CurrentUserId);
                    Response.Redirect("FINJournal.aspx?id=" + newId + "&saved=1");
                }
                else
                {
                    FINDatabaseHelper.UpdateJournalDraft(
                        JournalID, jdate, txtNarration.Text?.Trim(), txtReference.Text?.Trim(),
                        lines, CurrentUserId);
                    Response.Redirect("FINJournal.aspx?id=" + JournalID + "&saved=1");
                }
            }
            catch (Exception ex)
            {
                ShowBanner("Save failed: " + ex.Message, "error");
            }
        }

        protected void btnPost_Click(object s, EventArgs e)
        {
            if (!IsFinance) { ShowBanner("Only Finance users can post journals.", "error"); return; }

            List<FINDatabaseHelper.JournalLineInput> lines;
            try { lines = CollectPostedLines(requireAllValid: true); }
            catch (Exception ex) { ShowBanner(ex.Message, "error"); return; }

            // Verify balance before hitting the DB
            decimal td = lines.Sum(l => l.Debit);
            decimal tc = lines.Sum(l => l.Credit);
            if (td != tc)
            { ShowBanner("Cannot post — journal is off by ₹" + Math.Abs(td - tc).ToString("N2") + ".", "error"); return; }

            DateTime jdate;
            if (!DateTime.TryParse(txtJournalDate.Text, out jdate))
            { ShowBanner("Please enter a valid date.", "error"); return; }

            try
            {
                int idToPost;
                if (IsNewMode)
                {
                    idToPost = FINDatabaseHelper.SaveJournalAsDraft(
                        jdate, txtNarration.Text?.Trim(), txtReference.Text?.Trim(),
                        lines, CurrentUserId);
                }
                else
                {
                    FINDatabaseHelper.UpdateJournalDraft(
                        JournalID, jdate, txtNarration.Text?.Trim(), txtReference.Text?.Trim(),
                        lines, CurrentUserId);
                    idToPost = JournalID;
                }
                FINDatabaseHelper.PostJournal(idToPost, CurrentUserId);
                Response.Redirect("FINJournal.aspx?id=" + idToPost + "&posted=1");
            }
            catch (Exception ex)
            {
                ShowBanner("Post failed: " + ex.Message, "error");
            }
        }

        protected void btnDelete_Click(object s, EventArgs e)
        {
            if (!IsFinance) { ShowBanner("Only Finance users can delete journals.", "error"); return; }
            if (JournalID == 0) { ShowBanner("Nothing to delete.", "error"); return; }
            try
            {
                FINDatabaseHelper.DeleteJournalDraft(JournalID);
                Response.Redirect("FINJournal.aspx?deleted=1");
            }
            catch (Exception ex)
            {
                ShowBanner("Delete failed: " + ex.Message, "error");
            }
        }

        protected void btnReverse_Click(object s, EventArgs e)
        {
            if (!IsFinance) { ShowBanner("Only Finance users can reverse journals.", "error"); return; }
            if (JournalID == 0) { ShowBanner("Nothing to reverse.", "error"); return; }
            try
            {
                int contraId = FINDatabaseHelper.ReverseJournal(
                    JournalID, DateTime.Today, "User-initiated reversal", CurrentUserId);
                Response.Redirect("FINJournal.aspx?id=" + contraId + "&reversed=1");
            }
            catch (Exception ex)
            {
                ShowBanner("Reversal failed: " + ex.Message, "error");
            }
        }

        /// <summary>
        /// Shared handler for both btnPushToZoho (first attempt) and
        /// btnRepushToZoho (retry after prior error).  The underlying
        /// PushJournalToZoho is idempotent, so the same code path handles
        /// both cases safely.
        /// </summary>
        protected void btnPushToZoho_Click(object s, EventArgs e)
        {
            if (!IsFinance) { ShowBanner("Only Finance users can push journals to Zoho.", "error"); return; }
            if (JournalID == 0) { ShowBanner("Save and post the journal first.", "error"); return; }

            try
            {
                var result = FINDatabaseHelper.PushJournalToZoho(JournalID, CurrentUserId);
                if (result.Success)
                {
                    string msg = result.AlreadyPushed
                        ? "Already in Zoho as " + (result.ZohoJournalNo ?? "") + "."
                        : "Pushed to Zoho as " + (result.ZohoJournalNo ?? "(draft)") + ".";
                    Response.Redirect("FINJournal.aspx?id=" + JournalID + "&zpushed=1");
                }
                else
                {
                    // Stay on the page so the user sees the error context (and can retry).
                    ShowBanner("Push to Zoho failed: " + result.Message, "error");
                    RefreshZohoSection();
                    WireDetailActionButtons();
                }
            }
            catch (Exception ex)
            {
                ShowBanner("Push to Zoho failed: " + ex.Message, "error");
                RefreshZohoSection();
                WireDetailActionButtons();
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  HELPERS
        // ══════════════════════════════════════════════════════════════
        List<FINDatabaseHelper.JournalLineInput> CollectPostedLines(bool requireAllValid)
        {
            var lines = new List<FINDatabaseHelper.JournalLineInput>();
            for (int i = 0; i < LineCount; i++)
            {
                string acc = GetPostedOrViewState("acc", i);
                decimal dr, cr;
                decimal.TryParse(GetPostedOrViewState("dr", i), out dr);
                decimal.TryParse(GetPostedOrViewState("cr", i), out cr);
                string desc = GetPostedOrViewState("desc", i);
                string party = GetPostedOrViewState("party", i);

                // Skip completely empty rows (user added line but never filled it in)
                if (string.IsNullOrEmpty(acc) && dr == 0 && cr == 0 && string.IsNullOrEmpty(desc) && string.IsNullOrEmpty(party))
                    continue;

                if (string.IsNullOrEmpty(acc))
                    throw new Exception("Line " + (i + 1) + ": please select an account.");
                if (dr == 0 && cr == 0)
                    throw new Exception("Line " + (i + 1) + ": enter either a debit or credit amount.");
                if (dr > 0 && cr > 0)
                    throw new Exception("Line " + (i + 1) + ": a line can be Debit OR Credit, not both.");
                if (dr < 0 || cr < 0)
                    throw new Exception("Line " + (i + 1) + ": amounts cannot be negative.");

                lines.Add(new FINDatabaseHelper.JournalLineInput
                {
                    ZohoAccountID = acc,
                    Debit = dr,
                    Credit = cr,
                    LineDescription = string.IsNullOrEmpty(desc) ? null : desc,
                    ContactID = string.IsNullOrEmpty(party) ? null : party
                });
            }

            if (requireAllValid && lines.Count < 2)
                throw new Exception("A journal entry must have at least 2 lines.");
            if (lines.Count == 0)
                throw new Exception("Add at least one non-empty line.");

            return lines;
        }

        void ShowBanner(string msg, string kind)
        {
            phBanner.Controls.Clear();
            string cls = "banner banner-" + (kind == "success" ? "success" : kind == "error" ? "error" : kind == "warn" ? "warn" : "info");
            phBanner.Controls.Add(new LiteralControl(
                "<div class='" + cls + "'>" + Server.HtmlEncode(msg) + "</div>"));
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            // Show post-redirect banners
            if (!string.IsNullOrEmpty(Request.QueryString["saved"]))
                ShowBanner("Draft saved.", "success");
            else if (!string.IsNullOrEmpty(Request.QueryString["posted"]))
                ShowBanner("Journal posted.", "success");
            else if (!string.IsNullOrEmpty(Request.QueryString["reversed"]))
                ShowBanner("Original entry reversed. This is the contra journal.", "success");
            else if (!string.IsNullOrEmpty(Request.QueryString["deleted"]))
                ShowBanner("Draft deleted.", "success");
            else if (!string.IsNullOrEmpty(Request.QueryString["zpushed"]))
                ShowBanner("Journal pushed to Zoho Books.", "success");
        }
    }
}
