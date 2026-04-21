using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using FINApp.DAL;

namespace FINApp
{
    /// <summary>
    /// GRN-to-Zoho dashboard. Four tabs (Raw / Packing / Consumables / Stationery).
    /// Phase 2: all four tabs are live. Each row in the active tab has a checkbox
    /// and a per-row Push button. Bulk "Push Selected" pushes all checked rows one
    /// by one. Stationary is stored under the DB spelling "Stationary" but shown
    /// in UI as "Stationery".
    /// </summary>
    public partial class FINGRNToZoho : Page
    {
        protected Label lblNavUser;
        protected Panel pnlReadOnly, pnlAlert, pnlActiveList, pnlPlaceholder, pnlEmpty;
        protected Literal litAlert, litResultInfo, litPlaceholderLabel;
        protected LinkButton tabRaw, tabPacking, tabConsumable, tabStationery;
        protected DropDownList ddlSupplier, ddlStatusFilter;
        protected TextBox txtFromDate, txtToDate;
        protected Button btnApplyFilter, btnPushSelected;
        protected PlaceHolder phTable;

        private int UserID => Convert.ToInt32(Session["FIN_UserID"]);
        private string UserRole => Session["FIN_Role"]?.ToString() ?? "";
        private bool IsFinance => FINConsignments.IsFinanceRole(UserRole);

        /// <summary>Which tab is currently active — "RAW", "PACKING", "CONSUMABLE", "STATIONARY".
        /// Stored in ViewState so it survives postbacks without needing a hidden field.
        /// Note: uses "STATIONARY" (DB spelling) not "STATIONERY" — matches zoho_billlog.GRNType.</summary>
        protected string ActiveTab
        {
            get { return (ViewState["ActiveTab"] as string) ?? "RAW"; }
            set { ViewState["ActiveTab"] = value; }
        }

        // ══════════════════════════════════════════════════════════════
        // Page lifecycle
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

            pnlReadOnly.Visible = !IsFinance;
            btnPushSelected.Visible = IsFinance;

            if (!IsPostBack)
            {
                // Default date range: last 30 days through today. Finance can widen it.
                txtFromDate.Text = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
                txtToDate.Text = DateTime.Today.ToString("yyyy-MM-dd");

                BindSupplierFilter();
                ActiveTab = "RAW";
            }

            // Tab counts come from the summary query — keep current even across
            // tab switches so finance sees inbound volume at a glance.
            RefreshTabCounts();

            // Always rebuild tab visuals (active underline + placeholder visibility)
            RenderTabState();

            // IMPORTANT: Always rebuild the list, even on postback. The per-row "Push"
            // LinkButtons and the per-row checkboxes are dynamic controls built inside
            // phTable; if we skip rebuild on postback, ASP.NET can't find the button
            // that was clicked and silently drops the command event.
            //
            // Page_Load runs BEFORE event handlers, so we rebuild here with the
            // pre-click data; the event handler (PushOne_Command / btnPushSelected_Click)
            // does its work, and then calls RenderActiveList() itself at the end to
            // refresh the display with post-push state.
            RenderActiveList();
        }

        // ══════════════════════════════════════════════════════════════
        // Tab click handlers
        // ══════════════════════════════════════════════════════════════

        protected void tabRaw_Click(object s, EventArgs e)       { ActiveTab = "RAW";       RenderTabState(); RenderActiveList(); }
        protected void tabPacking_Click(object s, EventArgs e)   { ActiveTab = "PACKING";   RenderTabState(); RenderActiveList(); }
        protected void tabConsumable_Click(object s, EventArgs e){ ActiveTab = "CONSUMABLE";RenderTabState(); RenderActiveList(); }
        protected void tabStationery_Click(object s, EventArgs e){ ActiveTab = "STATIONARY";RenderTabState(); RenderActiveList(); }

        /// <summary>Paint each tab: set CssClass (active underline) and Text (label + count badge).
        /// Text is set programmatically — declarative inner markup on LinkButtons does not
        /// round-trip through ViewState reliably on postbacks.
        /// Phase 2: all 4 tabs are live, so the placeholder panel is never shown.</summary>
        void RenderTabState()
        {
            tabRaw.CssClass        = "tab" + (ActiveTab == "RAW"        ? " tab-active" : "");
            tabPacking.CssClass    = "tab" + (ActiveTab == "PACKING"    ? " tab-active" : "");
            tabConsumable.CssClass = "tab" + (ActiveTab == "CONSUMABLE" ? " tab-active" : "");
            tabStationery.CssClass = "tab" + (ActiveTab == "STATIONARY" ? " tab-active" : "");

            tabRaw.Text        = TabLabel("Raw Materials",     _cntRaw);
            tabPacking.Text    = TabLabel("Packing Materials", _cntPacking);
            tabConsumable.Text = TabLabel("Consumables",       _cntConsumable);
            tabStationery.Text = TabLabel("Stationery",        _cntStationary);

            pnlActiveList.Visible = true;
            pnlPlaceholder.Visible = false;
            btnPushSelected.Visible = IsFinance;
        }

        static string TabLabel(string name, int count)
        {
            return "<span>" + name + "</span> <span class='count'>" + count + "</span>";
        }

        // ══════════════════════════════════════════════════════════════
        // Filter dropdowns / apply
        // ══════════════════════════════════════════════════════════════

        protected void ddlSupplier_Changed(object s, EventArgs e)      { RenderActiveList(); }
        protected void ddlStatusFilter_Changed(object s, EventArgs e)  { RenderActiveList(); }
        protected void btnApplyFilter_Click(object s, EventArgs e)     { RenderActiveList(); }

        void BindSupplierFilter()
        {
            var dt = FINDatabaseHelper.GetSuppliersWithGRNs();
            ddlSupplier.Items.Clear();
            ddlSupplier.Items.Add(new ListItem("All Suppliers", ""));
            foreach (DataRow r in dt.Rows)
                ddlSupplier.Items.Add(new ListItem(r["SupplierName"].ToString(), r["SupplierID"].ToString()));
        }

        DateTime? ParseDateOrNull(string s)
        {
            DateTime d;
            return DateTime.TryParse(s, out d) ? d : (DateTime?)null;
        }

        int SelectedSupplierId
        {
            get { int v; return int.TryParse(ddlSupplier.SelectedValue, out v) ? v : 0; }
        }

        // Tab counts, populated by RefreshTabCounts — used by RenderTabState to
        // render tab labels with inline count badges. We store instead of pushing
        // to Literals because LinkButton inner content (span + literal) does not
        // survive ASP.NET round-tripping reliably on postbacks.
        int _cntRaw, _cntPacking, _cntConsumable, _cntStationary;

        void RefreshTabCounts()
        {
            _cntRaw = _cntPacking = _cntConsumable = _cntStationary = 0;
            try
            {
                var dt = FINDatabaseHelper.GetGRNBillingTabSummary(
                    ParseDateOrNull(txtFromDate.Text),
                    ParseDateOrNull(txtToDate.Text));
                foreach (DataRow r in dt.Rows)
                {
                    string type = r["GRNType"].ToString();
                    int total = r["TotalCount"] != DBNull.Value ? Convert.ToInt32(r["TotalCount"]) : 0;
                    if (type == "RAW")         _cntRaw         = total;
                    if (type == "PACKING")     _cntPacking     = total;
                    if (type == "CONSUMABLE")  _cntConsumable  = total;
                    if (type == "STATIONARY")  _cntStationary  = total;
                }
            }
            catch { /* tab counts are decoration; don't fail the page on count errors */ }
        }

        // ══════════════════════════════════════════════════════════════
        // List rendering — builds a data table for the active tab
        // ══════════════════════════════════════════════════════════════

        void RenderActiveList()
        {
            phTable.Controls.Clear();
            pnlEmpty.Visible = false;

            if (ActiveTab != "RAW" && ActiveTab != "PACKING" &&
                ActiveTab != "CONSUMABLE" && ActiveTab != "STATIONARY")
            {
                litResultInfo.Text = "";
                return;
            }

            DateTime? fromDt = ParseDateOrNull(txtFromDate.Text);
            DateTime? toDt   = ParseDateOrNull(txtToDate.Text);
            string statusF   = ddlStatusFilter.SelectedValue;
            int supId        = SelectedSupplierId;

            DataTable dt;
            if (ActiveTab == "RAW")
                dt = FINDatabaseHelper.GetRawGRNsForBilling(fromDt, toDt, statusF);
            else if (ActiveTab == "PACKING")
                dt = FINDatabaseHelper.GetPackingGRNsForBilling(fromDt, toDt, statusF);
            else if (ActiveTab == "CONSUMABLE")
                dt = FINDatabaseHelper.GetConsumableGRNsForBilling(fromDt, toDt, statusF);
            else // STATIONARY
                dt = FINDatabaseHelper.GetStationaryGRNsForBilling(fromDt, toDt, statusF);

            // In-memory supplier filter — avoids a second SQL variant per tab
            if (supId > 0)
            {
                var rows = dt.Select("SupplierID=" + supId);
                var filtered = dt.Clone();
                foreach (var r in rows) filtered.ImportRow(r);
                dt = filtered;
            }

            int total = dt.Rows.Count;
            int pendingCount = 0;
            decimal pendingValue = 0m;
            decimal pushedValue = 0m;
            foreach (DataRow r in dt.Rows)
            {
                bool hasBill = r["ZohoBillID"] != DBNull.Value && !string.IsNullOrEmpty(r["ZohoBillID"].ToString());
                decimal amt = r["Amount"] != DBNull.Value ? Convert.ToDecimal(r["Amount"]) : 0m;
                if (!hasBill) { pendingCount++; pendingValue += amt; }
                else { pushedValue += amt; }
            }

            litResultInfo.Text = total + " GRN(s) — "
                + pendingCount + " pending (" + pendingValue.ToString("N0") + " ₹)"
                + (total > pendingCount ? ", " + (total - pendingCount) + " pushed (" + pushedValue.ToString("N0") + " ₹)" : "");

            if (total == 0)
            {
                pnlEmpty.Visible = true;
                return;
            }

            phTable.Controls.Add(BuildTable(dt, ActiveTab));
        }

        /// <summary>Build the data table as a Literal-wrapped HTML block so that per-row
        /// action LinkButtons can still be wired into the ASP.NET event system. We build
        /// the table via a Table control with Checkbox + LinkButton children rather than
        /// raw HTML — that way CommandName / CommandArgument dispatch works without JS.</summary>
        Control BuildTable(DataTable dt, string grnType)
        {
            var wrap = new Panel();
            var tbl = new Table();
            tbl.CssClass = "grn-table";
            tbl.CellPadding = 0; tbl.CellSpacing = 0;

            // Header
            var thead = new TableHeaderRow();
            string[] heads = new[] { "", "GRN No", "Supplier · Material", "Invoice · Date", "Qty", "Rate", "Amount", "Zoho Bill", "Action" };
            foreach (string h in heads)
            {
                var th = new TableHeaderCell { Text = h };
                if (h == "Qty" || h == "Rate" || h == "Amount") th.CssClass = "col-num";
                if (h == "Zoho Bill") th.CssClass = "col-status";
                if (h == "Action") th.CssClass = "col-action";
                if (h == "") th.CssClass = "col-check";
                thead.Cells.Add(th);
            }
            tbl.Rows.Add(thead);

            // Body
            foreach (DataRow r in dt.Rows)
            {
                int grnId = Convert.ToInt32(r["InwardID"]);
                string grnNo = r["GRNNo"].ToString();
                string supName = r["SupplierName"].ToString();
                string matName, matCode;
                if (grnType == "RAW")            { matName = r["RMName"].ToString();          matCode = r["RMCode"].ToString(); }
                else if (grnType == "PACKING")   { matName = r["PMName"].ToString();          matCode = r["PMCode"].ToString(); }
                else if (grnType == "CONSUMABLE"){ matName = r["ConsumableName"].ToString();  matCode = r["ConsumableCode"].ToString(); }
                else /* STATIONARY */            { matName = r["StationaryName"].ToString();  matCode = r["StationaryCode"].ToString(); }
                string invNo = r["InvoiceNo"] != DBNull.Value ? r["InvoiceNo"].ToString() : "";
                DateTime? invDt = r["InvoiceDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(r["InvoiceDate"]) : null;
                DateTime inwDt = Convert.ToDateTime(r["InwardDate"]);
                decimal qty = r["Quantity"] != DBNull.Value ? Convert.ToDecimal(r["Quantity"]) : 0m;
                decimal rate = r["Rate"] != DBNull.Value ? Convert.ToDecimal(r["Rate"]) : 0m;
                decimal amt = r["Amount"] != DBNull.Value ? Convert.ToDecimal(r["Amount"]) : 0m;
                bool hasBill = r["ZohoBillID"] != DBNull.Value && !string.IsNullOrEmpty(r["ZohoBillID"].ToString());
                string billNo = r["ZohoBillNo"] != DBNull.Value ? r["ZohoBillNo"].ToString() : "";
                string pushStatus = r["PushStatus"] != DBNull.Value ? r["PushStatus"].ToString() : "";
                string err = r["ErrorMessage"] != DBNull.Value ? r["ErrorMessage"].ToString() : "";

                // Validation — determine if row has Error-level issues that should block push
                var issues = ComputeValidation(r, grnType);
                bool hasErrors = false;
                foreach (var issue in issues) { if (issue.Severity == "err") { hasErrors = true; break; } }

                // ── Main GRN row ──
                var tr = new TableRow();

                // Checkbox — disabled if already pushed, not finance, or row has validation errors
                var cCheck = new TableCell { CssClass = "col-check" };
                var chk = new CheckBox { ID = "chkPush_" + grnId };
                chk.Enabled = IsFinance && !hasBill && !hasErrors;
                chk.Attributes["data-grnid"] = grnId.ToString();
                if (hasErrors && !hasBill) chk.ToolTip = "Cannot push — resolve validation errors first";
                cCheck.Controls.Add(chk);
                tr.Cells.Add(cCheck);

                // GRN no — now a clickable toggle for the expand panel
                var cGrn = new TableCell();
                cGrn.Controls.Add(new LiteralControl(
                    "<a href='#' id='lnkGrn_" + grnId + "' class='grn-no-link' " +
                    "onclick=\"return toggleGrnDetail(" + grnId + ");\">" +
                    "<span class='caret'>&#x25B8;</span>" +
                    Server.HtmlEncode(grnNo) + "</a>"));
                tr.Cells.Add(cGrn);

                // Supplier / Material
                var cSup = new TableCell();
                cSup.Controls.Add(new LiteralControl(
                    "<div class='supplier'>" + Server.HtmlEncode(supName) + "</div>" +
                    "<div class='material'>" + Server.HtmlEncode(matName) + " · " + Server.HtmlEncode(matCode) + "</div>"));
                tr.Cells.Add(cSup);

                // Invoice / Date
                var cInv = new TableCell();
                string invText = string.IsNullOrEmpty(invNo) ? "<span style='color:var(--text-dim);'>(no invoice no)</span>" : Server.HtmlEncode(invNo);
                string dateText = (invDt.HasValue ? invDt.Value : inwDt).ToString("dd-MMM-yy");
                cInv.Controls.Add(new LiteralControl(
                    "<div class='invoice'>" + invText + "</div>" +
                    "<div class='invoice-date'>" + dateText + "</div>"));
                tr.Cells.Add(cInv);

                tr.Cells.Add(new TableCell { CssClass = "col-num", Text = qty.ToString("N2") });
                tr.Cells.Add(new TableCell { CssClass = "col-num", Text = rate.ToString("N2") });
                tr.Cells.Add(new TableCell { CssClass = "col-num amount", Text = "₹ " + amt.ToString("N2") });

                // Zoho Bill status
                var cStatus = new TableCell { CssClass = "col-status" };
                if (hasBill)
                {
                    string zohoOrgId = FINDatabaseHelper.GetZohoOrgID();
                    string billLink = string.IsNullOrEmpty(zohoOrgId)
                        ? ("<span class='badge badge-pushed'>" + Server.HtmlEncode(billNo) + "</span>")
                        : ("<a class='btn-link-view' target='_blank' href='https://books.zoho.in/app/" + zohoOrgId + "#/bills/" + r["ZohoBillID"] + "' title='Open in Zoho Books'>" +
                           "<span class='badge badge-pushed'>&#x2713; " + Server.HtmlEncode(billNo) + " &#x2197;</span></a>");
                    cStatus.Controls.Add(new LiteralControl(billLink));
                }
                else if (pushStatus == "Error")
                {
                    cStatus.Controls.Add(new LiteralControl(
                        "<span class='badge badge-error'>Error</span>" +
                        (string.IsNullOrEmpty(err) ? "" : "<div class='err-line' title='" + Server.HtmlEncode(err) + "'>" +
                            Server.HtmlEncode(err.Length > 50 ? err.Substring(0, 50) + "…" : err) + "</div>")));
                }
                else
                {
                    cStatus.Controls.Add(new LiteralControl("<span class='badge badge-pending'>Pending</span>"));
                }
                tr.Cells.Add(cStatus);

                // Action — Push disabled if hasErrors
                var cAction = new TableCell { CssClass = "col-action" };
                if (IsFinance && !hasBill && !hasErrors)
                {
                    var btn = new LinkButton
                    {
                        ID = "btnPushRow_" + grnId,
                        CssClass = "btn btn-push",
                        Text = "Push",
                        CommandName = "PushOne",
                        CommandArgument = grnId.ToString() + "|" + grnType,
                        CausesValidation = false
                    };
                    btn.Command += PushOne_Command;
                    cAction.Controls.Add(btn);
                }
                else if (IsFinance && !hasBill && hasErrors)
                {
                    // Errors block push — show disabled button with tooltip
                    cAction.Controls.Add(new LiteralControl(
                        "<span class='btn btn-push disabled' title='Resolve validation errors first'>Push</span>"));
                }
                else if (hasBill)
                {
                    cAction.Controls.Add(new LiteralControl(
                        "<span style='font-size:11px;color:var(--text-muted);'>pushed</span>"));
                }
                else
                {
                    cAction.Controls.Add(new LiteralControl(
                        "<span style='font-size:11px;color:var(--text-dim);'>—</span>"));
                }
                tr.Cells.Add(cAction);

                tbl.Rows.Add(tr);

                // ── Detail (expand) row — hidden by default, toggled by JS ──
                var trDetail = new TableRow();
                trDetail.Attributes["id"] = "detail_" + grnId;
                trDetail.CssClass = "grn-detail";
                var cDetail = new TableCell();
                cDetail.ColumnSpan = heads.Length;
                cDetail.Controls.Add(new LiteralControl(BuildDetailPanel(r, grnType, grnId, issues)));
                trDetail.Cells.Add(cDetail);
                tbl.Rows.Add(trDetail);
            }

            wrap.Controls.Add(tbl);
            return wrap;
        }

        // ══════════════════════════════════════════════════════════════
        // Expand-row detail panel + validation
        // ══════════════════════════════════════════════════════════════

        class ValidationIssue
        {
            public string Severity; // err | warn | info
            public string Message;
        }

        /// <summary>Compute validation issues for a GRN row. "err" blocks the push;
        /// "warn" and "info" are advisory. Rules mirror the Zoho push logic in FINZohoHelper.</summary>
        System.Collections.Generic.List<ValidationIssue> ComputeValidation(DataRow r, string grnType)
        {
            var list = new System.Collections.Generic.List<ValidationIssue>();

            decimal qty = r["Quantity"] != DBNull.Value ? Convert.ToDecimal(r["Quantity"]) : 0m;
            decimal rate = r["Rate"] != DBNull.Value ? Convert.ToDecimal(r["Rate"]) : 0m;
            decimal amt = r["Amount"] != DBNull.Value ? Convert.ToDecimal(r["Amount"]) : 0m;
            decimal gstRate = r["GSTRate"] != DBNull.Value ? Convert.ToDecimal(r["GSTRate"]) : 0m;
            string grnHsn = r["HSNCode"] != DBNull.Value ? r["HSNCode"].ToString().Trim() : "";
            string supGstin = r["SupGSTIN"] != DBNull.Value ? r["SupGSTIN"].ToString().Trim() : "";
            string supState = r["SupState"] != DBNull.Value ? r["SupState"].ToString().Trim() : "";

            // Material-master HSN alias varies by type
            string masterHsn = "";
            if (grnType == "RAW" && r.Table.Columns.Contains("RMHSN"))
                masterHsn = r["RMHSN"] != DBNull.Value ? r["RMHSN"].ToString().Trim() : "";
            else if (grnType == "PACKING" && r.Table.Columns.Contains("PMHSN"))
                masterHsn = r["PMHSN"] != DBNull.Value ? r["PMHSN"].ToString().Trim() : "";
            else if (grnType == "CONSUMABLE" && r.Table.Columns.Contains("ConsumableHSN"))
                masterHsn = r["ConsumableHSN"] != DBNull.Value ? r["ConsumableHSN"].ToString().Trim() : "";
            else if (grnType == "STATIONARY" && r.Table.Columns.Contains("StationaryHSN"))
                masterHsn = r["StationaryHSN"] != DBNull.Value ? r["StationaryHSN"].ToString().Trim() : "";

            // ── Errors (block push) ──
            if (qty <= 0)  list.Add(new ValidationIssue { Severity = "err", Message = "Quantity is zero or negative" });
            if (rate <= 0) list.Add(new ValidationIssue { Severity = "err", Message = "Rate is zero or negative" });
            if (amt <= 0)  list.Add(new ValidationIssue { Severity = "err", Message = "Amount is zero or negative" });
            if (string.IsNullOrEmpty(grnHsn) && string.IsNullOrEmpty(masterHsn))
                list.Add(new ValidationIssue { Severity = "err", Message = "HSN code is missing on both GRN and item master" });

            // ── Warnings (advisory) ──
            if (string.IsNullOrEmpty(supGstin) || supGstin.Length < 15)
                list.Add(new ValidationIssue { Severity = "warn", Message = "Supplier has no GSTIN — will push as reverse-charge" });
            if (gstRate == 0m)
                list.Add(new ValidationIssue { Severity = "warn", Message = "GST rate is 0% — verify if exempt or data missing" });
            if (!string.IsNullOrEmpty(grnHsn) && !string.IsNullOrEmpty(masterHsn) && grnHsn != masterHsn)
                list.Add(new ValidationIssue { Severity = "warn", Message = "GRN HSN (" + grnHsn + ") differs from item master HSN (" + masterHsn + ")" });

            decimal shortQty = r.Table.Columns.Contains("ShortageQty") && r["ShortageQty"] != DBNull.Value ? Convert.ToDecimal(r["ShortageQty"]) : 0m;
            decimal shortVal = r.Table.Columns.Contains("ShortageValue") && r["ShortageValue"] != DBNull.Value ? Convert.ToDecimal(r["ShortageValue"]) : 0m;
            if ((shortQty > 0 && shortVal == 0) || (shortQty == 0 && shortVal > 0))
                list.Add(new ValidationIssue { Severity = "warn", Message = "Shortage qty/value mismatch" });

            // ── Info (noteworthy) ──
            bool interState = !string.IsNullOrEmpty(supState) && supState.Trim().ToLower() != "tamil nadu";
            if (interState)
                list.Add(new ValidationIssue { Severity = "info", Message = "Interstate (" + supState + ") — IGST applies" });

            decimal transport = r.Table.Columns.Contains("TransportCost") && r["TransportCost"] != DBNull.Value ? Convert.ToDecimal(r["TransportCost"]) : 0m;
            bool transportInGst = r.Table.Columns.Contains("TransportInGST") && r["TransportInGST"] != DBNull.Value && Convert.ToBoolean(r["TransportInGST"]);
            if (transport > 0 && !transportInGst)
                list.Add(new ValidationIssue { Severity = "info", Message = "Transport cost ₹" + transport.ToString("N2") + " present but not in GST" });

            return list;
        }

        /// <summary>Render the HTML block shown when the user clicks the GRN No link.
        /// Contains a "Going to Zoho" prominent section + collapsed "Other GRN fields" section.</summary>
        string BuildDetailPanel(DataRow r, string grnType, int grnId, System.Collections.Generic.List<ValidationIssue> issues)
        {
            // ── Gather everything once ──
            string matName, matCode, masterHsn;
            if (grnType == "RAW")            { matName = r["RMName"].ToString();          matCode = r["RMCode"].ToString();         masterHsn = Safe(r, "RMHSN"); }
            else if (grnType == "PACKING")   { matName = r["PMName"].ToString();          matCode = r["PMCode"].ToString();         masterHsn = Safe(r, "PMHSN"); }
            else if (grnType == "CONSUMABLE"){ matName = r["ConsumableName"].ToString();  matCode = r["ConsumableCode"].ToString(); masterHsn = Safe(r, "ConsumableHSN"); }
            else                             { matName = r["StationaryName"].ToString();  matCode = r["StationaryCode"].ToString(); masterHsn = Safe(r, "StationaryHSN"); }

            string supName  = r["SupplierName"].ToString();
            string supGstin = Safe(r, "SupGSTIN");
            string supState = Safe(r, "SupState");
            string grnNo    = r["GRNNo"].ToString();
            string invNo    = Safe(r, "InvoiceNo");
            DateTime? invDt = r["InvoiceDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(r["InvoiceDate"]) : null;
            DateTime inwDt  = Convert.ToDateTime(r["InwardDate"]);
            DateTime billDate = invDt ?? inwDt;
            string unit     = Safe(r, "Unit");

            decimal qty   = Convert.ToDecimal(r["Quantity"]);
            decimal rate  = Convert.ToDecimal(r["Rate"]);
            decimal amt   = Convert.ToDecimal(r["Amount"]);
            decimal subtotal = qty * rate;
            decimal gstRate  = r["GSTRate"] != DBNull.Value ? Convert.ToDecimal(r["GSTRate"]) : 0m;
            decimal gstAmt   = r["GSTAmount"] != DBNull.Value ? Convert.ToDecimal(r["GSTAmount"]) : 0m;
            string  grnHsn   = Safe(r, "HSNCode");

            bool interState    = !string.IsNullOrEmpty(supState) && supState.Trim().ToLower() != "tamil nadu";
            bool reverseCharge = string.IsNullOrEmpty(supGstin) || supGstin.Length < 15;

            string taxMode;
            string gstBreakdown;
            if (reverseCharge)
            {
                taxMode = "Reverse Charge (unregistered vendor)";
                gstBreakdown = "RCM — buyer pays " + gstRate.ToString("0.##") + "% directly to government";
            }
            else if (interState)
            {
                taxMode = "Regular (interstate)";
                gstBreakdown = "IGST " + gstRate.ToString("0.##") + "%  —  ₹ " + gstAmt.ToString("N2");
            }
            else
            {
                taxMode = "Regular (intrastate)";
                decimal half = gstRate / 2m;
                gstBreakdown = "CGST " + half.ToString("0.##") + "% + SGST " + half.ToString("0.##") + "%  —  ₹ " + gstAmt.ToString("N2");
            }

            // ── Build HTML ──
            var sb = new System.Text.StringBuilder();
            sb.Append("<div class='grn-detail-wrap'>");

            // Validation chips
            sb.Append("<div class='vchips'>");
            if (issues.Count == 0)
            {
                sb.Append("<span class='vchip ok'>&#x2713; No validation issues</span>");
            }
            else
            {
                foreach (var iss in issues)
                {
                    string icon = iss.Severity == "err" ? "&#x26D4;" : iss.Severity == "warn" ? "&#x26A0;" : "&#x2139;";
                    sb.Append("<span class='vchip ").Append(iss.Severity).Append("'>")
                      .Append(icon).Append(" ").Append(Server.HtmlEncode(iss.Message))
                      .Append("</span>");
                }
            }
            sb.Append("</div>");

            // ── Going to Zoho section ──
            sb.Append("<div class='grn-detail-section zoho'>");
            sb.Append("<h4>⚡ Going to Zoho Books</h4>");
            sb.Append("<div class='dl-grid'>");
            sb.Append(Row("Vendor",    "<strong>" + Server.HtmlEncode(supName) + "</strong>" +
                                        (string.IsNullOrEmpty(supGstin) ? " <span style='color:#b22222;'>(no GSTIN)</span>"
                                                                         : " &middot; <span class='mono'>" + Server.HtmlEncode(supGstin) + "</span>") +
                                        (string.IsNullOrEmpty(supState) ? "" : " &middot; " + Server.HtmlEncode(supState))));
            sb.Append(Row("Bill Date", billDate.ToString("dd-MMM-yyyy") +
                                         "<span style='color:var(--text-muted);font-size:11px;'> (from " +
                                         (invDt.HasValue ? "InvoiceDate" : "InwardDate") + ")</span>"));
            sb.Append(Row("Reference", string.IsNullOrEmpty(invNo) ? "<em>none</em>" : Server.HtmlEncode(invNo) +
                                         " (GRN: <span class='mono'>" + Server.HtmlEncode(grnNo) + "</span>)"));
            sb.Append("</div>");

            sb.Append("<div style='height:10px;'></div>");
            sb.Append("<div class='dl-grid'>");
            sb.Append(Row("Item",     "<strong>" + Server.HtmlEncode(matName) + "</strong> &middot; <span class='mono'>" + Server.HtmlEncode(matCode) + "</span>"));
            string hsnDisp = string.IsNullOrEmpty(grnHsn) ? (string.IsNullOrEmpty(masterHsn) ? "<span style='color:#b22222;'>MISSING</span>" : "<span class='mono'>" + Server.HtmlEncode(masterHsn) + "</span> <span style='color:var(--text-muted);font-size:11px;'>(from item master)</span>") : "<span class='mono'>" + Server.HtmlEncode(grnHsn) + "</span>";
            sb.Append(Row("HSN",      hsnDisp));
            sb.Append(Row("Quantity", "<span class='amt'>" + qty.ToString("N2") + "</span>" + (string.IsNullOrEmpty(unit) ? "" : " " + Server.HtmlEncode(unit))));
            sb.Append(Row("Rate",     "<span class='amt'>₹ " + rate.ToString("N2") + "</span>"));
            sb.Append(Row("Subtotal", "<span class='amt'>₹ " + subtotal.ToString("N2") + "</span>"));
            sb.Append(Row("GST",      gstBreakdown));
            sb.Append(Row("Tax Mode", taxMode));
            sb.Append("</div>");

            // Bill total highlight
            sb.Append("<div class='bill-total-row'>");
            sb.Append("<span class='label'>Bill Total → Zoho</span>");
            sb.Append("<span class='val'>₹ ").Append(amt.ToString("N2")).Append("</span>");
            sb.Append("</div>");
            sb.Append("</div>"); // zoho section

            // ── Other GRN fields (collapsible) ──
            sb.Append("<button type='button' class='other-toggle' id='otherBtn_").Append(grnId).Append("' ");
            sb.Append("onclick="return toggleOther(").Append(grnId).Append(");">");
            sb.Append("<span class='caret'>▸</span> Other GRN fields (PO, transport, shortage, remarks, audit)");
            sb.Append("</button>");
            sb.Append("<div id='other_").Append(grnId).Append("' style='display:none;margin-top:10px;'>");
            sb.Append("<div class='grn-detail-section other'>");
            sb.Append("<h4>Operational Data</h4>");
            sb.Append("<div class='dl-grid'>");

            string po        = Safe(r, "PONo");
            string remarks   = Safe(r, "Remarks");
            string status    = Safe(r, "Status");
            string createdBy = Safe(r, "CreatedByName");
            DateTime createdAt = r["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(r["CreatedAt"]) : DateTime.MinValue;
            decimal qtyActual = r.Table.Columns.Contains("QtyActualReceived") && r["QtyActualReceived"] != DBNull.Value ? Convert.ToDecimal(r["QtyActualReceived"]) : 0m;
            decimal shortQty  = r.Table.Columns.Contains("ShortageQty") && r["ShortageQty"] != DBNull.Value ? Convert.ToDecimal(r["ShortageQty"]) : 0m;
            decimal shortVal  = r.Table.Columns.Contains("ShortageValue") && r["ShortageValue"] != DBNull.Value ? Convert.ToDecimal(r["ShortageValue"]) : 0m;
            decimal transport = r.Table.Columns.Contains("TransportCost") && r["TransportCost"] != DBNull.Value ? Convert.ToDecimal(r["TransportCost"]) : 0m;
            bool transInInv   = r.Table.Columns.Contains("TransportInInvoice") && r["TransportInInvoice"] != DBNull.Value && Convert.ToBoolean(r["TransportInInvoice"]);
            bool transInGst   = r.Table.Columns.Contains("TransportInGST") && r["TransportInGST"] != DBNull.Value && Convert.ToBoolean(r["TransportInGST"]);
            decimal loadCh    = r.Table.Columns.Contains("LoadingCharges") && r["LoadingCharges"] != DBNull.Value ? Convert.ToDecimal(r["LoadingCharges"]) : 0m;
            decimal unloadCh  = r.Table.Columns.Contains("UnloadingCharges") && r["UnloadingCharges"] != DBNull.Value ? Convert.ToDecimal(r["UnloadingCharges"]) : 0m;
            bool qtyVerified  = r.Table.Columns.Contains("QtyVerified") && r["QtyVerified"] != DBNull.Value && Convert.ToBoolean(r["QtyVerified"]);
            bool qc           = r.Table.Columns.Contains("QualityCheck") && r["QualityCheck"] != DBNull.Value && Convert.ToBoolean(r["QualityCheck"]);

            sb.Append(Row("PO No",           string.IsNullOrEmpty(po) ? "<em style='color:var(--text-muted);'>none</em>" : Server.HtmlEncode(po)));
            sb.Append(Row("Status",          Server.HtmlEncode(status)));
            sb.Append(Row("Qty Actual",      qtyActual > 0 ? qtyActual.ToString("N2") + (string.IsNullOrEmpty(unit) ? "" : " " + Server.HtmlEncode(unit)) : "<em style='color:var(--text-muted);'>not recorded</em>"));
            sb.Append(Row("Shortage",        (shortQty > 0 || shortVal > 0) ? shortQty.ToString("N2") + " × ₹ " + shortVal.ToString("N2") : "<em style='color:var(--text-muted);'>none</em>"));
            sb.Append(Row("Transport Cost",  transport > 0 ? "₹ " + transport.ToString("N2") + (transInInv ? " (in invoice)" : " (separate)") + (transInGst ? ", GST applied" : ", no GST") : "<em style='color:var(--text-muted);'>none</em>"));
            sb.Append(Row("Loading / Unload",(loadCh > 0 || unloadCh > 0) ? "₹ " + loadCh.ToString("N2") + " / ₹ " + unloadCh.ToString("N2") : "<em style='color:var(--text-muted);'>none</em>"));
            sb.Append(Row("Qty Verified",    qtyVerified ? "<span style='color:#2d7a2d;'>Yes</span>" : "<span style='color:var(--text-muted);'>No</span>"));
            sb.Append(Row("Quality Check",   qc ? "<span style='color:#2d7a2d;'>Yes</span>" : "<span style='color:var(--text-muted);'>No</span>"));
            sb.Append(Row("Created",         Server.HtmlEncode(string.IsNullOrEmpty(createdBy) ? "(unknown)" : createdBy) + (createdAt == DateTime.MinValue ? "" : " on " + createdAt.ToString("dd-MMM-yy HH:mm"))));
            if (!string.IsNullOrEmpty(remarks))
                sb.Append("<div class='dt'>Remarks</div><div class='dd dd-full'>" + Server.HtmlEncode(remarks) + "</div>");
            sb.Append("</div>"); // dl-grid
            sb.Append("</div>"); // other section
            sb.Append("</div>"); // other container

            sb.Append("</div>"); // wrap
            return sb.ToString();
        }

        static string Row(string label, string valueHtml)
        {
            return "<div class='dt'>" + System.Web.HttpUtility.HtmlEncode(label) + "</div><div class='dd'>" + valueHtml + "</div>";
        }

        static string Safe(DataRow r, string col)
        {
            if (!r.Table.Columns.Contains(col)) return "";
            return r[col] != DBNull.Value ? r[col].ToString() : "";
        }

        // ══════════════════════════════════════════════════════════════
        // Push handlers
        // ══════════════════════════════════════════════════════════════

        protected void PushOne_Command(object sender, CommandEventArgs e)
        {
            if (!IsFinance) { ShowAlert("Not permitted for your role.", "alert-danger"); return; }

            string arg = e.CommandArgument?.ToString() ?? "";
            var parts = arg.Split('|');
            if (parts.Length != 2) return;
            int grnId; if (!int.TryParse(parts[0], out grnId)) return;
            string grnType = parts[1];

            var results = new System.Collections.Generic.List<FINApp.DAL.ZohoBillPushResult>();
            results.Add(SafePush(grnId, grnType));
            RenderPushResults(results);

            // Refresh the list so the pushed row flips to green
            RefreshTabCounts();
            RenderActiveList();
        }

        protected void btnPushSelected_Click(object sender, EventArgs e)
        {
            if (!IsFinance) { ShowAlert("Not permitted for your role.", "alert-danger"); return; }

            // Read which checkboxes were checked from the posted form — the checkboxes
            // live inside a dynamically-built table so ViewState can't find them by ID.
            var selected = new System.Collections.Generic.List<int>();
            foreach (string key in Request.Form.AllKeys)
            {
                if (key == null) continue;
                if (!key.Contains("chkPush_")) continue;
                // key format ctl00$...$chkPush_123
                int idx = key.LastIndexOf("chkPush_");
                if (idx < 0) continue;
                string tail = key.Substring(idx + "chkPush_".Length);
                int grnId;
                if (int.TryParse(tail, out grnId)) selected.Add(grnId);
            }

            if (selected.Count == 0)
            {
                ShowAlert("No GRNs selected. Tick the checkboxes to push multiple at once.", "alert-warn");
                return;
            }

            var results = new System.Collections.Generic.List<FINApp.DAL.ZohoBillPushResult>();
            foreach (int grnId in selected)
                results.Add(SafePush(grnId, ActiveTab));

            RenderPushResults(results);
            RefreshTabCounts();
            RenderActiveList();
        }

        FINApp.DAL.ZohoBillPushResult SafePush(int grnId, string grnType)
        {
            try { return FINDatabaseHelper.PushGRNToZoho(grnId, grnType, UserID); }
            catch (Exception ex) { return new FINApp.DAL.ZohoBillPushResult { GRNID = grnId, GRNType = grnType, Success = false, Message = ex.Message }; }
        }

        void RenderPushResults(System.Collections.Generic.List<FINApp.DAL.ZohoBillPushResult> results)
        {
            int ok = 0, already = 0, err = 0;
            var sb = new System.Text.StringBuilder();
            sb.Append("<div style='font-size:12px;'>");
            foreach (var r in results)
            {
                if (r.AlreadyPushed) { already++; continue; }
                if (r.Success) ok++; else err++;

                string icon = r.Success ? "&#x2705;" : "&#x274C;";
                string color = r.Success ? "var(--teal)" : "var(--danger)";
                sb.Append("<div style='color:").Append(color).Append(";margin-bottom:3px;'>")
                  .Append(icon).Append(" GRN #").Append(r.GRNID).Append(" — ")
                  .Append(Server.HtmlEncode(r.Message ?? ""));
                if (!string.IsNullOrEmpty(r.ZohoBillNo))
                    sb.Append(" <strong>(").Append(Server.HtmlEncode(r.ZohoBillNo)).Append(")</strong>");
                sb.Append("</div>");
            }

            // Summary line at top
            string summary = "Pushed " + ok + " bill(s)";
            if (err > 0) summary += ", " + err + " error(s)";
            if (already > 0) summary += ", " + already + " already pushed";

            string cls = (err > 0 && ok == 0) ? "alert-danger" : (err > 0 ? "alert-warn" : "alert-success");
            sb.Append("</div>");

            pnlAlert.Visible = true;
            pnlAlert.CssClass = "alert " + cls;
            litAlert.Text = "<b>" + summary + "</b>" + (results.Count > 0 ? "<div style='margin-top:6px;'>" + sb.ToString() + "</div>" : "");
        }

        void ShowAlert(string msg, string cls)
        {
            pnlAlert.Visible = true;
            pnlAlert.CssClass = "alert " + cls;
            litAlert.Text = msg;
        }
    }
}
