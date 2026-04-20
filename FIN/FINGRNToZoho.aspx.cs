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
    /// Phase 1: Raw + Packing are live; the other two show a placeholder.
    /// Each row in the active tab has a checkbox + per-row Push button.
    /// Bulk "Push Selected" pushes all checked rows one by one.
    /// </summary>
    public partial class FINGRNToZoho : Page
    {
        protected Label lblNavUser;
        protected Panel pnlReadOnly, pnlAlert, pnlActiveList, pnlPlaceholder, pnlEmpty;
        protected Literal litAlert, litResultInfo, litRawCount, litPackingCount, litPlaceholderLabel;
        protected LinkButton tabRaw, tabPacking, tabConsumable, tabStationery;
        protected DropDownList ddlSupplier, ddlStatusFilter;
        protected TextBox txtFromDate, txtToDate;
        protected Button btnApplyFilter, btnPushSelected;
        protected PlaceHolder phTable;

        private int UserID => Convert.ToInt32(Session["FIN_UserID"]);
        private string UserRole => Session["FIN_Role"]?.ToString() ?? "";
        private bool IsFinance => FINConsignments.IsFinanceRole(UserRole);

        /// <summary>Which tab is currently active — "RAW", "PACKING", "CONSUMABLE", "STATIONERY".
        /// Stored in ViewState so it survives postbacks without needing a hidden field.</summary>
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
        protected void tabStationery_Click(object s, EventArgs e){ ActiveTab = "STATIONERY";RenderTabState(); RenderActiveList(); }

        /// <summary>Paint the active tab's underline + count badge, and toggle placeholder vs. list panel.</summary>
        void RenderTabState()
        {
            tabRaw.CssClass        = "tab" + (ActiveTab == "RAW"        ? " tab-active" : "");
            tabPacking.CssClass    = "tab" + (ActiveTab == "PACKING"    ? " tab-active" : "");
            tabConsumable.CssClass = "tab tab-disabled" + (ActiveTab == "CONSUMABLE" ? " tab-active" : "");
            tabStationery.CssClass = "tab tab-disabled" + (ActiveTab == "STATIONERY" ? " tab-active" : "");

            bool isActiveType = (ActiveTab == "RAW" || ActiveTab == "PACKING");
            pnlActiveList.Visible = isActiveType;
            pnlPlaceholder.Visible = !isActiveType;
            btnPushSelected.Visible = IsFinance && isActiveType;

            if (!isActiveType)
                litPlaceholderLabel.Text = ActiveTab == "CONSUMABLE" ? "Consumables GRNs" : "Stationery GRNs";
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

        void RefreshTabCounts()
        {
            try
            {
                var dt = FINDatabaseHelper.GetGRNBillingTabSummary(
                    ParseDateOrNull(txtFromDate.Text),
                    ParseDateOrNull(txtToDate.Text));
                foreach (DataRow r in dt.Rows)
                {
                    string type = r["GRNType"].ToString();
                    int total = r["TotalCount"] != DBNull.Value ? Convert.ToInt32(r["TotalCount"]) : 0;
                    if (type == "RAW")     litRawCount.Text     = "<span class='count'>" + total + "</span>";
                    if (type == "PACKING") litPackingCount.Text = "<span class='count'>" + total + "</span>";
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

            if (ActiveTab != "RAW" && ActiveTab != "PACKING")
            {
                litResultInfo.Text = "";
                return;
            }

            DateTime? fromDt = ParseDateOrNull(txtFromDate.Text);
            DateTime? toDt   = ParseDateOrNull(txtToDate.Text);
            string statusF   = ddlStatusFilter.SelectedValue;
            int supId        = SelectedSupplierId;

            DataTable dt = (ActiveTab == "RAW")
                ? FINDatabaseHelper.GetRawGRNsForBilling(fromDt, toDt, statusF)
                : FINDatabaseHelper.GetPackingGRNsForBilling(fromDt, toDt, statusF);

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
            string[] heads;
            if (grnType == "RAW")
                heads = new[] { "", "GRN No", "Supplier · Material", "Invoice · Date", "Qty", "Rate", "Amount", "Zoho Bill", "Action" };
            else
                heads = new[] { "", "GRN No", "Supplier · Material", "Invoice · Date", "Qty", "Rate", "Amount", "Zoho Bill", "Action" };
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
                string matName = grnType == "RAW" ? r["RMName"].ToString() : r["PMName"].ToString();
                string matCode = grnType == "RAW" ? r["RMCode"].ToString() : r["PMCode"].ToString();
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

                var tr = new TableRow();

                // Checkbox — disabled if already pushed or not finance
                var cCheck = new TableCell { CssClass = "col-check" };
                var chk = new CheckBox { ID = "chkPush_" + grnId };
                chk.Enabled = IsFinance && !hasBill;
                // Attach DCID via Attributes so we can read it in btnPushSelected_Click
                chk.Attributes["data-grnid"] = grnId.ToString();
                cCheck.Controls.Add(chk);
                tr.Cells.Add(cCheck);
                // GRN no
                var cGrn = new TableCell();
                cGrn.Controls.Add(new LiteralControl(
                    "<span class='grn-no'>" + Server.HtmlEncode(grnNo) + "</span>"));
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

                // Qty
                var cQty = new TableCell { CssClass = "col-num", Text = qty.ToString("N2") };
                tr.Cells.Add(cQty);

                // Rate
                var cRate = new TableCell { CssClass = "col-num", Text = rate.ToString("N2") };
                tr.Cells.Add(cRate);

                // Amount
                var cAmt = new TableCell { CssClass = "col-num amount", Text = "₹ " + amt.ToString("N2") };
                tr.Cells.Add(cAmt);

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

                // Action
                var cAction = new TableCell { CssClass = "col-action" };
                if (IsFinance && !hasBill)
                {
                    var btn = new LinkButton
                    {
                        ID = "btnPushRow_" + grnId,  // explicit ID so event dispatch works on postback
                        CssClass = "btn btn-push",
                        Text = "Push",
                        CommandName = "PushOne",
                        CommandArgument = grnId.ToString() + "|" + grnType,
                        CausesValidation = false
                    };
                    btn.Command += PushOne_Command;
                    cAction.Controls.Add(btn);
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
            }

            wrap.Controls.Add(tbl);
            return wrap;
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

            var results = new System.Collections.Generic.List<StockApp.DAL.ZohoBillPushResult>();
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

            var results = new System.Collections.Generic.List<StockApp.DAL.ZohoBillPushResult>();
            foreach (int grnId in selected)
                results.Add(SafePush(grnId, ActiveTab));

            RenderPushResults(results);
            RefreshTabCounts();
            RenderActiveList();
        }

        StockApp.DAL.ZohoBillPushResult SafePush(int grnId, string grnType)
        {
            try { return FINDatabaseHelper.PushGRNToZoho(grnId, grnType, UserID); }
            catch (Exception ex) { return new StockApp.DAL.ZohoBillPushResult { GRNID = grnId, GRNType = grnType, Success = false, Message = ex.Message }; }
        }

        void RenderPushResults(System.Collections.Generic.List<StockApp.DAL.ZohoBillPushResult> results)
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
