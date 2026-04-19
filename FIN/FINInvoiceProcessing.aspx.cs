using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using FINApp.DAL;

namespace FINApp
{
    public partial class FINInvoiceProcessing : Page
    {
        protected Label lblNavUser;
        protected Panel pnlAlert, pnlReadOnly, pnlDCs, pnlEmpty, pnlDispatchForm;
        protected Literal litAlert, litConsigSummary, litBottomSummary;
        protected DropDownList ddlConsig;
        protected Repeater rptDCs;
        protected Button btnDownloadAllInvoices, btnMarkReady, btnOpenDispatch, btnConfirmDispatch;
        protected TextBox txtVehicleNo;
        protected HiddenField hfActiveDCID;

        // Record IRN modal
        protected Panel pnlRecordIRN;
        protected HiddenField hfRecordIRN_DCID;
        protected Label lblRecordIRN_DCInfo;
        protected TextBox txtIRN, txtAckNo, txtAckDate;
        protected Button btnSaveIRN, btnCancelIRN;

        // Cancel E-Invoice modal
        protected Panel pnlCancelEInv;
        protected HiddenField hfCancelEInv_DCID;
        protected Label lblCancelEInv_Info;
        protected DropDownList ddlCancelReason;
        protected TextBox txtCancelNotes;
        protected Button btnConfirmCancelEInv, btnCancelEInv_Close;

        // Record EWB modal
        protected Panel pnlRecordEWB;
        protected HiddenField hfRecordEWB_DCID;
        protected Label lblRecordEWB_Info;
        protected TextBox txtEWBNo, txtEWBDate, txtEWBValid;
        protected Button btnSaveEWB, btnCancelEWB;

        private int UserID => Convert.ToInt32(Session["FIN_UserID"]);
        private string UserRole => Session["FIN_Role"]?.ToString() ?? "";
        private bool IsFinance => FINConsignments.IsFinanceRole(UserRole);

        /// <summary>DCID the user has expanded for review; 0 when collapsed.</summary>
        protected int ActiveDCID
        {
            get { int v; int.TryParse(hfActiveDCID.Value, out v); return v; }
            set { hfActiveDCID.Value = value.ToString(); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["FIN_UserID"] == null) { Response.Redirect("FINLogin.aspx"); return; }
            lblNavUser.Text = Session["FIN_FullName"]?.ToString() ?? "";
            pnlReadOnly.Visible = !IsFinance;

            if (!IsPostBack)
            {
                BindConsignmentDropdown();
                if (ddlConsig.Items.Count > 1)
                {
                    ddlConsig.SelectedIndex = 1;
                    LoadConsignment(Convert.ToInt32(ddlConsig.SelectedValue));
                }
                else
                {
                    pnlEmpty.Visible = true;
                }
            }
            else
            {
                // On postback, rebind ONLY when the user has a DC's detail panel open.
                // The detail panel contains dynamically-added LinkButtons (per-line Save/Delete)
                // that aren't preserved across postbacks via ViewState; they must be re-created
                // in Page_Load so ASP.NET can dispatch their command events.
                //
                // For other postbacks (approval checkbox, dropdown change, modal buttons), the
                // declarative controls in the repeater are restored from ViewState. Rebinding
                // here would CLOBBER posted form values (e.g. checkbox state) with the OLD DB
                // state before the event handler runs — breaking AutoPostBack controls inside
                // the repeater. The event handlers themselves call RefreshCurrentConsignment
                // after their work, so the UI catches up to the new DB state.
                if (ActiveDCID > 0)
                {
                    int csgId = 0; int.TryParse(ddlConsig.SelectedValue, out csgId);
                    if (csgId > 0) LoadConsignment(csgId);
                }
            }
        }

        // ══════════════════════════════════════════════════════════════
        // CONSIGNMENT DROPDOWN
        // ══════════════════════════════════════════════════════════════

        void BindConsignmentDropdown()
        {
            ddlConsig.Items.Clear();
            ddlConsig.Items.Add(new ListItem("— Select a consignment —", "0"));
            var dt = FINDatabaseHelper.GetActiveConsignmentsForFIN();
            foreach (DataRow r in dt.Rows)
            {
                string label = r["ConsignmentCode"].ToString() + " — " + r["Status"];
                ddlConsig.Items.Add(new ListItem(label, r["ConsignmentID"].ToString()));
            }
        }

        protected void ddlConsig_Changed(object sender, EventArgs e)
        {
            int csgId = 0; int.TryParse(ddlConsig.SelectedValue, out csgId);
            ActiveDCID = 0; // collapse any open detail
            if (csgId > 0) LoadConsignment(csgId);
            else
            {
                pnlDCs.Visible = false;
                litConsigSummary.Text = "";
            }
        }

        void LoadConsignment(int csgId)
        {
            // Summary strip at top
            var consigs = FINDatabaseHelper.GetActiveConsignmentsForFIN();
            DataRow csg = null;
            foreach (DataRow r in consigs.Rows)
                if (Convert.ToInt32(r["ConsignmentID"]) == csgId) { csg = r; break; }

            if (csg == null)
            {
                pnlDCs.Visible = false;
                litConsigSummary.Text = "<span style='color:var(--danger);'>Consignment no longer active — may have been dispatched.</span>";
                return;
            }

            string status = csg["Status"].ToString();
            string statusPillCss = status == "READY" ? "s-ready" : "s-open";
            string transport = "";
            string tm = csg["TransportMode"].ToString();
            if (tm == "FULL_LOAD") transport = "Own Vehicle";
            else if (tm == "COURIER")
            {
                transport = "Courier";
                string cn = csg["CourierName"].ToString();
                if (!string.IsNullOrEmpty(cn)) transport += " (" + cn + ")";
            }
            else transport = "—";

            litConsigSummary.Text =
                "<span><b>" + csg["ConsignmentCode"] + "</b></span>" +
                "<span>Status: <span class='status-pill " + statusPillCss + "'>" + status + "</span></span>" +
                "<span>Date: <b>" + Convert.ToDateTime(csg["ConsignmentDate"]).ToString("dd-MMM-yyyy") + "</b></span>" +
                "<span>Transport: <b>" + Server.HtmlEncode(transport) + "</b></span>";

            // DC list
            var dcs = FINDatabaseHelper.GetDCsByConsignmentForFIN(csgId);
            rptDCs.DataSource = dcs;
            rptDCs.DataBind();
            pnlDCs.Visible = true;
            pnlEmpty.Visible = false;

            // Bottom-bar summary + action button visibility
            int total = dcs.Rows.Count;
            int approved = 0, finalised = 0, draft = 0;
            decimal grand = 0;
            foreach (DataRow r in dcs.Rows)
            {
                if (r["ApprovedAt"] != DBNull.Value) approved++;
                string dcStatus = r["Status"].ToString();
                if (dcStatus == "FINALISED") finalised++;
                else if (dcStatus == "DRAFT") draft++;
                grand += r["GrandTotal"] != DBNull.Value ? Convert.ToDecimal(r["GrandTotal"]) : 0;
            }

            litBottomSummary.Text = string.Format(
                "<b>{0}</b> DCs · <b>{1}</b> finalised · <b>{2}</b> approved · Total <b>&#x20B9;{3:N2}</b>",
                total, finalised, approved, grand);

            // Mark READY: visible when status=OPEN AND every DC is FINALISED
            btnMarkReady.Visible = IsFinance && status == "OPEN" && finalised == total && total > 0;
            // Dispatch: visible when status=READY (no DRAFT DCs, implied by the Mark READY precondition)
            btnOpenDispatch.Visible = IsFinance && status == "READY";
            pnlDispatchForm.Visible = false;

            // Disable bulk download if no invoices exist
            btnDownloadAllInvoices.Enabled = total > 0;
        }

        // ══════════════════════════════════════════════════════════════
        // REPEATER: PER-ROW DATABINDING + COMMANDS
        // ══════════════════════════════════════════════════════════════

        protected void rptDCs_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            var row = (DataRowView)e.Item.DataItem;
            int dcId = Convert.ToInt32(row["DCID"]);

            // Approval checkbox state + metadata
            var chk = e.Item.FindControl("chkApprove") as CheckBox;
            var litMeta = e.Item.FindControl("litApproveMeta") as Literal;
            bool isApproved = row["ApprovedAt"] != DBNull.Value;
            if (chk != null)
            {
                chk.Checked = isApproved;
                chk.Enabled = IsFinance;
            }
            if (litMeta != null)
            {
                if (isApproved)
                {
                    string who = row["ApprovedByName"] != DBNull.Value ? row["ApprovedByName"].ToString() : "—";
                    DateTime when = Convert.ToDateTime(row["ApprovedAt"]);
                    litMeta.Text = Server.HtmlEncode(who) + "<br/>" + when.ToString("dd-MMM HH:mm");
                }
                else
                {
                    litMeta.Text = "";
                }
            }

            // Invoice cell: invoice number + small "open in Zoho" deep link if Zoho invoice exists
            var litInv = e.Item.FindControl("litInvoice") as Literal;
            if (litInv != null)
            {
                string invNo = row["InvoiceNumber"] != DBNull.Value ? row["InvoiceNumber"].ToString() : "";
                string zohoInvId = FINDatabaseHelper.GetZohoInvoiceID(dcId);
                string zohoOrgId = FINDatabaseHelper.GetZohoOrgID();
                if (string.IsNullOrEmpty(invNo))
                {
                    litInv.Text = "<span style='color:var(--text-dim);'>—</span>";
                }
                else if (!string.IsNullOrEmpty(zohoInvId) && !string.IsNullOrEmpty(zohoOrgId))
                {
                    // Zoho Books frontend is a hash-routed SPA — without the # before /invoices,
                    // the browser hits the server, which serves the dashboard. With #/invoices/{id},
                    // the SPA router opens the specific invoice.
                    string url = "https://books.zoho.in/app/" + zohoOrgId + "#/invoices/" + zohoInvId;
                    litInv.Text = Server.HtmlEncode(invNo) +
                        " <a href='" + url + "' target='_blank' title='Open in Zoho Books' " +
                        "style='color:var(--accent);text-decoration:none;font-size:11px;'>&#x2197;</a>";
                }
                else
                {
                    litInv.Text = Server.HtmlEncode(invNo);
                }
            }

            // E-Invoice cell: status badge + action button(s) per state
            var phEi = e.Item.FindControl("phEInvoice") as PlaceHolder;
            if (phEi != null)
                BuildEInvoiceCell(phEi, dcId, row);

            // Render the inline detail panel if this is the active DC
            if (dcId == ActiveDCID)
            {
                var pnl = e.Item.FindControl("pnlInlineDetail") as Panel;
                var ph = e.Item.FindControl("phDetail") as PlaceHolder;
                if (pnl != null) pnl.Visible = true;
                if (ph != null) BuildDCDetailInto(ph, dcId);
            }
        }

        protected void rptDCs_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int dcId;
            if (!int.TryParse(e.CommandArgument?.ToString(), out dcId)) return;

            switch (e.CommandName)
            {
                case "ToggleDetail":
                    ActiveDCID = (ActiveDCID == dcId) ? 0 : dcId;
                    RefreshCurrentConsignment();
                    break;

                case "DownloadInvoice":
                    DownloadSingleInvoice(dcId);
                    break;

                case "SaveLine":
                    SaveLineFromDetail(e.Item, dcId);
                    break;

                case "DeleteLine":
                    DeleteLineFromDetail(e.Item, dcId);
                    break;

                // ── e-invoice / EWB modal triggers ──
                case "OpenIRPInZoho":
                    OpenZohoInvoiceInNewTab(dcId);
                    break;

                case "RecordIRN":
                    OpenRecordIRNModal(dcId);
                    break;

                case "CancelEInvoice":
                    OpenCancelEInvoiceModal(dcId);
                    break;

                case "RecordEWB":
                    OpenRecordEWBModal(dcId);
                    break;

                case "CancelEWB":
                    CancelEWBNow(dcId);
                    break;
            }
        }

        void RefreshCurrentConsignment()
        {
            int csgId = 0; int.TryParse(ddlConsig.SelectedValue, out csgId);
            if (csgId > 0) LoadConsignment(csgId);
        }

        // ══════════════════════════════════════════════════════════════
        // DC DETAIL PANEL — built programmatically so we can render
        // edit controls for each line within the repeater item
        // ══════════════════════════════════════════════════════════════

        /// <summary>Build the expanded DC detail (header block + editable lines table + Zoho warning).
        /// Injected into the repeater item's PlaceHolder so each DC's detail is isolated.</summary>
        void BuildDCDetailInto(PlaceHolder ph, int dcId)
        {
            ph.Controls.Clear();
            var ds = FINDatabaseHelper.GetDCDetailForFIN(dcId);
            if (ds.Tables["Header"].Rows.Count == 0)
            {
                ph.Controls.Add(new LiteralControl("<div class='alert alert-danger'>DC not found.</div>"));
                return;
            }
            var hdr = ds.Tables["Header"].Rows[0];
            var lines = ds.Tables["Lines"];

            // Header title
            string title = "<div class='dc-detail-title'>DC Detail — " + Server.HtmlEncode(hdr["DCNumber"].ToString())
                + (hdr["InvoiceNumber"] != DBNull.Value && hdr["InvoiceNumber"].ToString() != ""
                    ? " &nbsp;·&nbsp; Invoice: <strong>" + Server.HtmlEncode(hdr["InvoiceNumber"].ToString()) + "</strong>"
                    : "")
                + "</div>";
            ph.Controls.Add(new LiteralControl(title));

            // Zoho sync warning when DC is FINALISED or CLOSED — edits here will desync
            string st = hdr["Status"].ToString();
            if (IsFinance && (st == "FINALISED" || st == "CLOSED"))
            {
                ph.Controls.Add(new LiteralControl(
                    "<div class='fin-edit-warn'>" +
                    "<b>&#x26A0; Zoho sync warning</b>" +
                    "This DC is " + st + " — its invoice is already in Zoho Books. Any line edit here will " +
                    "desync the Zoho invoice. You'll need to update the invoice in Zoho manually or use " +
                    "Sync-from-Zoho after saving." +
                    "</div>"));
            }

            // E-invoice lock warning — much stronger than the Zoho warning above. Edits to a DC
            // that has an active IRN will be REFUSED at the DAL level, so warn finance up-front
            // that they need to cancel the e-invoice first.
            string activeIRN = FINDatabaseHelper.GetActiveIRN(dcId);
            if (!string.IsNullOrEmpty(activeIRN))
            {
                ph.Controls.Add(new LiteralControl(
                    "<div class='fin-edit-warn' style='background:#fff3cd;border-color:#ffeeba;color:#856404;'>" +
                    "<b>&#x1F512; E-Invoice locked</b>" +
                    "This DC has an active e-invoice (IRN " +
                    Server.HtmlEncode(activeIRN.Substring(0, Math.Min(16, activeIRN.Length))) + "...). " +
                    "Line edits and deletes will be refused. To modify, first cancel the e-invoice in Zoho " +
                    "Books (within 24 hrs of generation), record the cancellation in ERP, then edit here." +
                    "</div>"));
            }

            // Header info grid
            var hg = new StringBuilder();
            hg.Append("<div class='header-grid'>");
            hg.Append("<div><div class='hk'>Customer</div><div class='hv'>").Append(Server.HtmlEncode(hdr["CustomerName"].ToString())).Append("</div></div>");
            hg.Append("<div><div class='hk'>Code</div><div class='hv'>").Append(Server.HtmlEncode(hdr["CustomerCode"].ToString())).Append("</div></div>");
            hg.Append("<div><div class='hk'>State</div><div class='hv'>").Append(Server.HtmlEncode(hdr["State"]?.ToString() ?? "—")).Append("</div></div>");
            hg.Append("<div><div class='hk'>Channel</div><div class='hv'>").Append(hdr["Channel"]?.ToString() == "SM" ? "Super Market" : "General Trade").Append("</div></div>");
            hg.Append("<div><div class='hk'>DC Date</div><div class='hv'>").Append(Convert.ToDateTime(hdr["DCDate"]).ToString("dd-MMM-yyyy")).Append("</div></div>");
            hg.Append("<div><div class='hk'>Status</div><div class='hv'>").Append(st).Append("</div></div>");
            hg.Append("</div>");
            ph.Controls.Add(new LiteralControl(hg.ToString()));

            // Lines table — render as a nested Repeater so we can place Save/Delete buttons
            // that fire the parent rptDCs's ItemCommand via bubbled events.
            var linesRpt = new Repeater();
            linesRpt.ID = "rptLines_" + dcId;
            // Header row is a literal; the Repeater renders the tbody
            var tblSb = new StringBuilder();
            tblSb.Append("<table class='lines-tbl'>");
            tblSb.Append("<thead><tr>");
            tblSb.Append("<th>Product</th><th>HSN</th><th class='num'>GST%</th>");
            tblSb.Append("<th class='num'>Qty</th><th class='num'>MRP</th><th class='num'>Margin%</th>");
            tblSb.Append("<th class='num'>Rate</th><th class='num'>Taxable</th><th class='num'>GST</th>");
            tblSb.Append("<th class='num'>Total</th><th style='text-align:center;'>Actions</th>");
            tblSb.Append("</tr></thead>");
            ph.Controls.Add(new LiteralControl(tblSb.ToString()));

            // Render lines as plain HTML + buttons (LinkButtons bubble via rptDCs.ItemCommand since
            // they live inside rptDCs items). Each row uses stable IDs with the LineID suffix.
            var body = new StringBuilder("<tbody>");
            decimal sumTax = 0, sumGst = 0, sumTotal = 0;
            foreach (DataRow lr in lines.Rows)
            {
                int lineId = Convert.ToInt32(lr["LineID"]);
                decimal gstR  = lr["GSTRate"]    != DBNull.Value ? Convert.ToDecimal(lr["GSTRate"])    : 0;
                int     qty   = lr["Qty"]        != DBNull.Value ? Convert.ToInt32(lr["Qty"])          : 0;
                decimal mrp   = lr["MRP"]        != DBNull.Value ? Convert.ToDecimal(lr["MRP"])        : 0;
                decimal mgn   = lr["MarginPct"]  != DBNull.Value ? Convert.ToDecimal(lr["MarginPct"])  : 0;
                decimal rate  = lr["UnitRate"]   != DBNull.Value ? Convert.ToDecimal(lr["UnitRate"])   : 0;
                decimal tax   = lr["TaxableValue"] != DBNull.Value ? Convert.ToDecimal(lr["TaxableValue"]) : 0;
                decimal cgst  = lr["CGSTAmt"]    != DBNull.Value ? Convert.ToDecimal(lr["CGSTAmt"])    : 0;
                decimal sgst  = lr["SGSTAmt"]    != DBNull.Value ? Convert.ToDecimal(lr["SGSTAmt"])    : 0;
                decimal igst  = lr["IGSTAmt"]    != DBNull.Value ? Convert.ToDecimal(lr["IGSTAmt"])    : 0;
                decimal total = lr["LineTotal"]  != DBNull.Value ? Convert.ToDecimal(lr["LineTotal"])  : 0;
                string hsn = lr["HSNCode"]?.ToString() ?? "";
                string form = lr["SellingForm"]?.ToString() ?? "";
                string source = lr["Source"]?.ToString() ?? "";

                sumTax += tax;
                sumGst += (cgst + sgst + igst);
                sumTotal += total;

                // Row with plain inputs named with lineId suffix so postback values can be retrieved
                // by the Save handler via Request.Form.
                body.Append("<tr>");
                body.Append("<td><strong>").Append(Server.HtmlEncode(lr["ProductName"].ToString()))
                    .Append("</strong><div style='font-size:10px;color:var(--text-dim);'>")
                    .Append(Server.HtmlEncode(lr["ProductCode"].ToString())).Append(" · ")
                    .Append(form).Append(" · ").Append(source).Append("</div></td>");

                if (IsFinance)
                {
                    body.Append("<td><input type='text' name='ln_hsn_").Append(lineId).Append("' value='")
                        .Append(Server.HtmlEncode(hsn)).Append("' style='max-width:70px;' /></td>");
                    body.Append("<td class='num'><input type='number' step='0.01' name='ln_gst_").Append(lineId).Append("' value='")
                        .Append(gstR.ToString("0.##")).Append("' class='num-input' style='max-width:65px;' /></td>");
                    body.Append("<td class='num'><input type='number' step='1' min='1' name='ln_qty_").Append(lineId).Append("' value='")
                        .Append(qty).Append("' class='num-input' /></td>");
                    body.Append("<td class='num'><input type='number' step='0.01' name='ln_mrp_").Append(lineId).Append("' value='")
                        .Append(mrp.ToString("0.00")).Append("' class='num-input' /></td>");
                    body.Append("<td class='num'><input type='number' step='0.01' name='ln_mgn_").Append(lineId).Append("' value='")
                        .Append(mgn.ToString("0.##")).Append("' class='num-input' style='max-width:70px;' /></td>");
                }
                else
                {
                    // Read-only renderings for non-finance users
                    body.Append("<td>").Append(Server.HtmlEncode(hsn)).Append("</td>");
                    body.Append("<td class='num'>").Append(gstR.ToString("0.##")).Append("%</td>");
                    body.Append("<td class='num'>").Append(qty).Append("</td>");
                    body.Append("<td class='num'>&#x20B9;").Append(mrp.ToString("N2")).Append("</td>");
                    body.Append("<td class='num'>").Append(mgn.ToString("0.##")).Append("%</td>");
                }
                body.Append("<td class='num'>&#x20B9;").Append(rate.ToString("N2")).Append("</td>");
                body.Append("<td class='num'>&#x20B9;").Append(tax.ToString("N2")).Append("</td>");
                body.Append("<td class='num' style='font-size:11px;'>&#x20B9;")
                    .Append((cgst + sgst + igst).ToString("N2"));
                if (igst > 0)
                    body.Append("<div style='font-size:9px;color:var(--text-dim);'>IGST ").Append(gstR.ToString("0.#")).Append("%</div>");
                else
                    body.Append("<div style='font-size:9px;color:var(--text-dim);'>C+S ").Append((gstR/2).ToString("0.#")).Append("%+").Append((gstR/2).ToString("0.#")).Append("%</div>");
                body.Append("</td>");
                body.Append("<td class='num'><strong>&#x20B9;").Append(total.ToString("N2")).Append("</strong></td>");

                // Action cell — Save + Delete link buttons (placeholder; actual controls added below)
                body.Append("<td style='text-align:center;min-width:110px;' data-line='").Append(lineId).Append("'></td>");
                body.Append("</tr>");
            }
            body.Append("</tbody></table>");
            ph.Controls.Add(new LiteralControl(body.ToString()));

            // Totals strip
            string totalsHtml = "<div class='totals-strip'>" +
                "<span>Subtotal: <b>&#x20B9;" + sumTax.ToString("N2") + "</b></span>" +
                "<span>GST: <b>&#x20B9;" + sumGst.ToString("N2") + "</b></span>" +
                "<span class='grand'>Grand Total: <b>&#x20B9;" + sumTotal.ToString("N2") + "</b></span>" +
                "</div>";
            ph.Controls.Add(new LiteralControl(totalsHtml));

            // Save/Delete buttons for each line — inserted as real controls so they can fire commands
            // that bubble up to rptDCs.ItemCommand. Since the table-cell placeholder is raw HTML,
            // we render the buttons inline as a row of action-strips below the table instead.
            if (IsFinance && lines.Rows.Count > 0)
            {
                var actionsPnl = new Panel();
                actionsPnl.CssClass = "detail-actions";
                actionsPnl.Style["flex-direction"] = "column";
                actionsPnl.Style["align-items"] = "stretch";
                actionsPnl.Style["gap"] = "6px";
                actionsPnl.Style["margin-top"] = "0";

                var heading = new LiteralControl("<div style='font-size:11px;font-weight:700;text-transform:uppercase;color:var(--text-muted);letter-spacing:.06em;margin:14px 0 6px 0;'>Line Actions (finance override)</div>");
                ph.Controls.Add(heading);

                foreach (DataRow lr in lines.Rows)
                {
                    int lineId = Convert.ToInt32(lr["LineID"]);
                    var lineStrip = new Panel();
                    lineStrip.Style["display"] = "flex";
                    lineStrip.Style["gap"] = "8px";
                    lineStrip.Style["align-items"] = "center";
                    lineStrip.Style["padding"] = "6px 10px";
                    lineStrip.Style["background"] = "#fff";
                    lineStrip.Style["border"] = "1px solid var(--border)";
                    lineStrip.Style["border-radius"] = "6px";
                    lineStrip.Style["font-size"] = "12px";

                    var lbl = new LiteralControl(
                        "<span style='flex:1;font-size:11px;color:var(--text-muted);'>" +
                        "<strong style='color:var(--text);'>" + Server.HtmlEncode(lr["ProductName"].ToString()) + "</strong>" +
                        " — Line #" + lineId + "</span>");
                    lineStrip.Controls.Add(lbl);

                    var btnSave = new LinkButton();
                    btnSave.ID = "btnSaveLine_" + lineId;
                    btnSave.CommandName = "SaveLine";
                    btnSave.CommandArgument = lineId.ToString();
                    btnSave.CssClass = "btn btn-primary";
                    btnSave.Text = "💾 Save";
                    btnSave.CausesValidation = false;
                    lineStrip.Controls.Add(btnSave);

                    var btnDel = new LinkButton();
                    btnDel.ID = "btnDelLine_" + lineId;
                    btnDel.CommandName = "DeleteLine";
                    btnDel.CommandArgument = lineId.ToString();
                    btnDel.CssClass = "btn btn-danger";
                    btnDel.Text = "🗑 Delete";
                    btnDel.CausesValidation = false;
                    btnDel.OnClientClick = "return confirmDeleteLine();";
                    lineStrip.Controls.Add(btnDel);

                    actionsPnl.Controls.Add(lineStrip);
                }
                ph.Controls.Add(actionsPnl);
            }
        }

        // ══════════════════════════════════════════════════════════════
        // LINE EDIT/DELETE HANDLERS
        // ══════════════════════════════════════════════════════════════

        /// <summary>Read the edited line fields from Request.Form and push through the FIN edit path.
        /// Fields are named ln_{field}_{lineId} in the posted form (see BuildDCDetailInto).</summary>
        void SaveLineFromDetail(RepeaterItem parent, int lineId)
        {
            if (!IsFinance) { ShowAlert("Not permitted for your role.", "alert-danger"); return; }

            string hsn  = Request.Form["ln_hsn_" + lineId] ?? "";
            string gstS = Request.Form["ln_gst_" + lineId] ?? "0";
            string qtyS = Request.Form["ln_qty_" + lineId] ?? "0";
            string mrpS = Request.Form["ln_mrp_" + lineId] ?? "0";
            string mgnS = Request.Form["ln_mgn_" + lineId] ?? "0";

            int qty = 0; int.TryParse(qtyS, out qty);
            decimal gst = 0; decimal.TryParse(gstS, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out gst);
            decimal mrp = 0; decimal.TryParse(mrpS, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out mrp);
            decimal mgn = 0; decimal.TryParse(mgnS, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out mgn);

            if (qty <= 0) { ShowAlert("Quantity must be greater than zero.", "alert-danger"); return; }
            if (mrp <= 0) { ShowAlert("MRP must be greater than zero.", "alert-danger"); return; }
            if (mgn < 0 || mgn >= 100) { ShowAlert("Margin must be between 0 and 100.", "alert-danger"); return; }
            if (gst < 0 || gst > 50) { ShowAlert("GST rate looks wrong — must be 0 to 50.", "alert-danger"); return; }

            try
            {
                FINDatabaseHelper.UpdateDCLineFromFIN(lineId, qty, mrp, mgn, hsn, gst, UserID);
                ShowAlert("Line updated. Approval cleared — re-review the DC.", "alert-success");
                RefreshCurrentConsignment();
            }
            catch (Exception ex) { ShowAlert("Save error: " + ex.Message, "alert-danger"); }
        }

        void DeleteLineFromDetail(RepeaterItem parent, int lineId)
        {
            if (!IsFinance) { ShowAlert("Not permitted for your role.", "alert-danger"); return; }
            try
            {
                FINDatabaseHelper.DeleteDCLineFromFIN(lineId, UserID);
                ShowAlert("Line deleted.", "alert-success");
                RefreshCurrentConsignment();
            }
            catch (Exception ex) { ShowAlert("Delete error: " + ex.Message, "alert-danger"); }
        }

        // ══════════════════════════════════════════════════════════════
        // APPROVAL CHECKBOX
        // ══════════════════════════════════════════════════════════════

        protected void chkApprove_Changed(object sender, EventArgs e)
        {
            if (!IsFinance) { ShowAlert("Not permitted for your role.", "alert-danger"); RefreshCurrentConsignment(); return; }

            var chk = sender as CheckBox;
            if (chk == null) return;
            var item = chk.NamingContainer as RepeaterItem;
            if (item == null) return;
            var hf = item.FindControl("hfDcIdForChk") as HiddenField;
            if (hf == null) return;
            int dcId = 0; int.TryParse(hf.Value, out dcId);
            if (dcId <= 0) return;

            // Determine the user's intent. Two cases:
            //   - ActiveDCID == 0: Page_Load skipped the rebind, ViewState handled the repeater,
            //     and chk.Checked reflects the user's posted value correctly.
            //   - ActiveDCID > 0: Page_Load rebound the repeater (needed for dynamic line buttons),
            //     which clobbered chk.Checked with the OLD DB value. Fall back to Request.Form
            //     to recover the posted value.
            bool wantApproved;
            if (ActiveDCID > 0)
            {
                string posted = Request.Form[chk.UniqueID];
                wantApproved = posted != null && posted.IndexOf("on", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            else
            {
                wantApproved = chk.Checked;
            }

            try
            {
                if (wantApproved) FINDatabaseHelper.ApproveDC(dcId, UserID);
                else              FINDatabaseHelper.UnapproveDC(dcId);
                RefreshCurrentConsignment();
            }
            catch (Exception ex) { ShowAlert("Approval error: " + ex.Message, "alert-danger"); }
        }

        // ══════════════════════════════════════════════════════════════
        // BULK ACTIONS
        // ══════════════════════════════════════════════════════════════

        protected void btnDownloadAllInvoices_Click(object sender, EventArgs e)
        {
            // Placeholder — we'd normally generate a zip of Zoho invoice PDFs here. For now, alert
            // that this requires a Zoho API bulk-download stub that hasn't been wired.
            ShowAlert("Bulk invoice download is a pending enhancement — please use the per-DC PDF links for now.", "alert-info");
        }

        protected void btnMarkReady_Click(object sender, EventArgs e)
        {
            if (!IsFinance) { ShowAlert("Not permitted for your role.", "alert-danger"); return; }
            int csgId = 0; int.TryParse(ddlConsig.SelectedValue, out csgId);
            if (csgId <= 0) return;

            try
            {
                // Use the direct UPDATE — finance doesn't need the PK-side eligibility ritual, but we
                // still refuse if any DC is in DRAFT since dispatch would be blocked anyway.
                var dcs = FINDatabaseHelper.GetDCsByConsignmentForFIN(csgId);
                var drafts = new List<string>();
                foreach (DataRow r in dcs.Rows)
                    if (r["Status"].ToString() == "DRAFT") drafts.Add(r["DCNumber"].ToString());
                if (drafts.Count > 0)
                {
                    ShowAlert("Cannot mark READY — DCs still in DRAFT: " + string.Join(", ", drafts.ToArray()), "alert-danger");
                    return;
                }
                // No dedicated FIN helper for MarkReady; issue the state change via the shared table
                FINDatabaseHelper.MarkConsignmentReadyFromFIN(csgId);
                ShowAlert("Consignment marked READY. Dispatch now available.", "alert-success");
                // Refresh dropdown to reflect new status label
                BindConsignmentDropdown();
                ddlConsig.SelectedValue = csgId.ToString();
                LoadConsignment(csgId);
            }
            catch (Exception ex) { ShowAlert("Mark READY failed: " + ex.Message, "alert-danger"); }
        }

        protected void btnOpenDispatch_Click(object sender, EventArgs e)
        {
            if (!IsFinance) { ShowAlert("Not permitted for your role.", "alert-danger"); return; }
            pnlDispatchForm.Visible = true;
            if (txtVehicleNo != null) txtVehicleNo.Focus();
        }

        protected void btnConfirmDispatch_Click(object sender, EventArgs e)
        {
            if (!IsFinance) { ShowAlert("Not permitted for your role.", "alert-danger"); return; }
            int csgId = 0; int.TryParse(ddlConsig.SelectedValue, out csgId);
            if (csgId <= 0) return;
            string vehicle = txtVehicleNo?.Text?.Trim().ToUpper() ?? "";
            if (string.IsNullOrEmpty(vehicle))
            { ShowAlert("Vehicle number is required.", "alert-danger"); return; }

            try
            {
                FINDatabaseHelper.DispatchConsignmentFromFIN(csgId, vehicle);
                ShowAlert("Consignment dispatched with vehicle " + vehicle + ". All DCs marked CLOSED.", "alert-success");
                // Remove it from dropdown (DISPATCHED consignments move to Historical)
                BindConsignmentDropdown();
                if (ddlConsig.Items.Count > 1)
                {
                    ddlConsig.SelectedIndex = 1;
                    LoadConsignment(Convert.ToInt32(ddlConsig.SelectedValue));
                }
                else
                {
                    pnlDCs.Visible = false;
                    pnlEmpty.Visible = true;
                    litConsigSummary.Text = "";
                }
            }
            catch (Exception ex) { ShowAlert("Dispatch failed: " + ex.Message, "alert-danger"); }
        }

        void DownloadSingleInvoice(int dcId)
        {
            // Placeholder — same as bulk. Zoho PDF URL lookup would go here; the PK side uses
            // StockApp.DAL.ZohoHelper but it's a separate assembly we don't reference from FIN.
            ShowAlert("Per-DC invoice download is pending — view the invoice directly in Zoho for now.", "alert-info");
        }

        // ══════════════════════════════════════════════════════════════
        // E-INVOICE / EWB CELL RENDERING + DEEP LINK
        // ══════════════════════════════════════════════════════════════
        // The e-invoice column shows one of these states per DC:
        //   - B2C (no GSTIN): grey "B2C" badge, no actions
        //   - B2B no IRN yet: "Push to IRP" button (deep link to Zoho) +
        //     "Record IRN" button to enter IRN after pushing
        //   - B2B with IRN: green badge with truncated IRN +
        //     "Cancel" link + "EWB" sub-action
        //   - Cancelled: red "Cancelled" badge + "Re-push" action
        // Buttons are disabled when user isn't in a finance role.

        void BuildEInvoiceCell(PlaceHolder ph, int dcId, DataRowView row)
        {
            ph.Controls.Clear();

            // B2C check first — no GSTIN means no e-invoice required
            string gstin = row["GSTIN"] != DBNull.Value ? row["GSTIN"].ToString().Trim() : "";
            if (gstin.Length < 15)
            {
                ph.Controls.Add(new LiteralControl(
                    "<span class='einv-badge einv-b-b2c' title='No GSTIN — e-invoicing not applicable for B2C invoices'>B2C</span>"));
                return;
            }

            // Has IRN? Read from the joined columns on the row
            string irn = row["IRN"] != DBNull.Value ? row["IRN"].ToString() : "";
            string status = row["EInvoiceStatus"] != DBNull.Value ? row["EInvoiceStatus"].ToString() : "";

            if (status == "GENERATED" && !string.IsNullOrEmpty(irn))
            {
                // Active e-invoice — show badge + IRN preview + actions
                string irnPreview = irn.Length > 10 ? irn.Substring(0, 10) + "…" : irn;
                ph.Controls.Add(new LiteralControl(
                    "<span class='einv-badge einv-b-irp' title='" + Server.HtmlEncode(irn) + "'>IRN ✓</span>" +
                    "<div class='einv-irn' title='" + Server.HtmlEncode(irn) + "'>" + Server.HtmlEncode(irnPreview) + "</div>"));

                if (IsFinance)
                {
                    // EWB sub-action: record or show
                    string ewbNo = row["EWBNumber"] != DBNull.Value ? row["EWBNumber"].ToString() : "";
                    string ewbStatus = row["EWBStatus"] != DBNull.Value ? row["EWBStatus"].ToString() : "";
                    if (string.IsNullOrEmpty(ewbNo))
                    {
                        var btnEWB = new LinkButton
                        {
                            CommandName = "RecordEWB", CommandArgument = dcId.ToString(),
                            CssClass = "einv-link", Text = "+ EWB", CausesValidation = false
                        };
                        ph.Controls.Add(btnEWB);
                    }
                    else if (ewbStatus == "CANCELLED")
                    {
                        ph.Controls.Add(new LiteralControl(
                            "<div class='einv-irn' style='color:var(--danger);'>EWB ✗</div>"));
                    }
                    else
                    {
                        ph.Controls.Add(new LiteralControl(
                            "<div class='einv-irn' style='color:var(--teal);' title='EWB " + Server.HtmlEncode(ewbNo) + "'>EWB ✓ " + Server.HtmlEncode(ewbNo.Length > 8 ? ewbNo.Substring(0, 8) + "…" : ewbNo) + "</div>"));
                        var btnEWBCancel = new LinkButton
                        {
                            CommandName = "CancelEWB", CommandArgument = dcId.ToString(),
                            CssClass = "einv-link", Text = "✗ EWB", CausesValidation = false,
                            OnClientClick = "return confirm('Mark EWB as cancelled? Cancel it in Zoho/IRP separately.');"
                        };
                        ph.Controls.Add(btnEWBCancel);
                    }

                    // Cancel e-invoice link
                    var btnCancel = new LinkButton
                    {
                        CommandName = "CancelEInvoice", CommandArgument = dcId.ToString(),
                        CssClass = "einv-link", Text = "Cancel", CausesValidation = false
                    };
                    btnCancel.Style["color"] = "var(--danger)";
                    ph.Controls.Add(btnCancel);
                }
                return;
            }

            // No active e-invoice — show "Push to IRP in Zoho" + "Record IRN"
            ph.Controls.Add(new LiteralControl(
                "<span class='einv-badge einv-b-none'>Pending</span>"));

            if (!IsFinance) return;

            // Push to IRP — deep link to Zoho (only useful if Zoho invoice id is known)
            string zohoInvId = FINDatabaseHelper.GetZohoInvoiceID(dcId);
            string zohoOrgId = FINDatabaseHelper.GetZohoOrgID();
            if (!string.IsNullOrEmpty(zohoInvId) && !string.IsNullOrEmpty(zohoOrgId))
            {
                // Hash-routed SPA — see note in BuildEInvoiceCell comment for litInvoice URL.
                string zohoUrl = "https://books.zoho.in/app/" + zohoOrgId + "#/invoices/" + zohoInvId;
                ph.Controls.Add(new LiteralControl(
                    "<a href='" + zohoUrl + "' target='_blank' class='einv-link' " +
                    "title='Open invoice in Zoho Books, then click Push to IRP'>↗ Push in Zoho</a>"));

                var btnRecord = new LinkButton
                {
                    CommandName = "RecordIRN", CommandArgument = dcId.ToString(),
                    CssClass = "einv-link", Text = "Record IRN", CausesValidation = false
                };
                ph.Controls.Add(btnRecord);
            }
            else
            {
                ph.Controls.Add(new LiteralControl(
                    "<div class='einv-irn' style='color:var(--warn);'>No Zoho invoice yet</div>"));
            }
        }

        // ══════════════════════════════════════════════════════════════
        // MODAL OPENERS
        // ══════════════════════════════════════════════════════════════

        void OpenZohoInvoiceInNewTab(int dcId)
        {
            // Unused — handled directly via anchor tag in the cell. Kept for symmetry.
        }

        /// <summary>Open the Record-IRN modal pre-populated with DC info. Finance has just
        /// returned from Zoho where they pushed the invoice to IRP.</summary>
        void OpenRecordIRNModal(int dcId)
        {
            if (!IsFinance) { ShowAlert("Not permitted for your role.", "alert-danger"); return; }

            // Refuse if an active e-invoice already exists
            string existingIrn = FINDatabaseHelper.GetActiveIRN(dcId);
            if (!string.IsNullOrEmpty(existingIrn))
            { ShowAlert("This DC already has an active e-invoice (IRN " + existingIrn.Substring(0, Math.Min(12, existingIrn.Length)) + "...). Cancel it first if you need to re-issue.", "alert-warn"); return; }

            hfRecordIRN_DCID.Value = dcId.ToString();
            txtIRN.Text = "";
            txtAckNo.Text = "";
            // Default ACK date to "now" since most users will record IRN immediately after pushing
            txtAckDate.Text = FINDatabaseHelper.NowIST().ToString("yyyy-MM-dd HH:mm");

            // Show DC context
            string dcInfo = GetDCSummaryLine(dcId);
            lblRecordIRN_DCInfo.Text = dcInfo;
            pnlRecordIRN.Visible = true;
        }

        /// <summary>Open the cancel-e-invoice modal. Will refuse if no active IRN exists.</summary>
        void OpenCancelEInvoiceModal(int dcId)
        {
            if (!IsFinance) { ShowAlert("Not permitted for your role.", "alert-danger"); return; }

            string irn = FINDatabaseHelper.GetActiveIRN(dcId);
            if (string.IsNullOrEmpty(irn))
            { ShowAlert("No active e-invoice on this DC to cancel.", "alert-warn"); return; }

            hfCancelEInv_DCID.Value = dcId.ToString();
            ddlCancelReason.SelectedIndex = 1; // default to "Data entry mistake"
            txtCancelNotes.Text = "";
            lblCancelEInv_Info.Text = GetDCSummaryLine(dcId) + " · IRN: " +
                Server.HtmlEncode(irn.Substring(0, Math.Min(20, irn.Length))) + "...";
            pnlCancelEInv.Visible = true;
        }

        void OpenRecordEWBModal(int dcId)
        {
            if (!IsFinance) { ShowAlert("Not permitted for your role.", "alert-danger"); return; }

            // EWB requires an active IRN
            string irn = FINDatabaseHelper.GetActiveIRN(dcId);
            if (string.IsNullOrEmpty(irn))
            { ShowAlert("Generate the e-invoice first before recording an EWB.", "alert-warn"); return; }

            hfRecordEWB_DCID.Value = dcId.ToString();
            txtEWBNo.Text = "";
            txtEWBDate.Text = FINDatabaseHelper.NowIST().ToString("yyyy-MM-dd HH:mm");
            txtEWBValid.Text = "";
            lblRecordEWB_Info.Text = GetDCSummaryLine(dcId);
            pnlRecordEWB.Visible = true;
        }

        void CancelEWBNow(int dcId)
        {
            if (!IsFinance) { ShowAlert("Not permitted for your role.", "alert-danger"); return; }
            try
            {
                FINDatabaseHelper.CancelEWayBillManual(dcId, "Cancelled from FIN dashboard", UserID);
                ShowAlert("E-way bill marked as cancelled in ERP. Cancel it in Zoho/IRP separately.", "alert-success");
                RefreshCurrentConsignment();
            }
            catch (Exception ex) { ShowAlert("EWB cancel failed: " + ex.Message, "alert-danger"); }
        }

        // ══════════════════════════════════════════════════════════════
        // MODAL HANDLERS
        // ══════════════════════════════════════════════════════════════

        protected void btnSaveIRN_Click(object sender, EventArgs e)
        {
            if (!IsFinance) { ShowAlert("Not permitted for your role.", "alert-danger"); return; }
            int dcId = 0; int.TryParse(hfRecordIRN_DCID.Value, out dcId);
            if (dcId <= 0) { ShowAlert("DC reference lost — try again.", "alert-danger"); return; }

            string irn = (txtIRN.Text ?? "").Trim();
            string ackNo = (txtAckNo.Text ?? "").Trim();
            string ackDtStr = (txtAckDate.Text ?? "").Trim();

            if (string.IsNullOrEmpty(irn)) { ShowAlert("IRN is required.", "alert-danger"); return; }
            if (irn.Length < 32) { ShowAlert("IRN looks too short — paste the full 64-char value from Zoho.", "alert-danger"); return; }
            if (string.IsNullOrEmpty(ackNo)) { ShowAlert("ACK Number is required.", "alert-danger"); return; }

            DateTime? ackDt = null;
            if (!string.IsNullOrEmpty(ackDtStr))
            {
                DateTime parsed;
                if (!DateTime.TryParse(ackDtStr, out parsed))
                { ShowAlert("ACK Date is not a valid date — use yyyy-mm-dd HH:mm.", "alert-danger"); return; }
                ackDt = parsed;
            }

            try
            {
                string zohoInvId = FINDatabaseHelper.GetZohoInvoiceID(dcId);
                FINDatabaseHelper.RecordEInvoiceManual(dcId, irn, ackNo, ackDt, zohoInvId, UserID);
                pnlRecordIRN.Visible = false;
                ShowAlert("IRN recorded for DC. The e-invoice is now tracked in ERP.", "alert-success");
                RefreshCurrentConsignment();
            }
            catch (Exception ex) { ShowAlert("Save IRN failed: " + ex.Message, "alert-danger"); }
        }

        protected void btnCancelIRN_Click(object sender, EventArgs e)
        {
            pnlRecordIRN.Visible = false;
        }

        protected void btnConfirmCancelEInv_Click(object sender, EventArgs e)
        {
            if (!IsFinance) { ShowAlert("Not permitted for your role.", "alert-danger"); return; }
            int dcId = 0; int.TryParse(hfCancelEInv_DCID.Value, out dcId);
            if (dcId <= 0) return;

            string reasonCode = ddlCancelReason.SelectedValue;
            string reasonText = ddlCancelReason.SelectedItem != null ? ddlCancelReason.SelectedItem.Text : reasonCode;
            string notes = (txtCancelNotes.Text ?? "").Trim();
            string composed = reasonText + (string.IsNullOrEmpty(notes) ? "" : " — " + notes);

            try
            {
                FINDatabaseHelper.CancelEInvoiceManual(dcId, composed, UserID);
                pnlCancelEInv.Visible = false;
                ShowAlert("E-invoice marked as cancelled. Remember to also cancel it in Zoho Books / IRP.", "alert-success");
                RefreshCurrentConsignment();
            }
            catch (Exception ex) { ShowAlert("Cancel failed: " + ex.Message, "alert-danger"); }
        }

        protected void btnCancelEInvClose_Click(object sender, EventArgs e)
        {
            pnlCancelEInv.Visible = false;
        }

        protected void btnSaveEWB_Click(object sender, EventArgs e)
        {
            if (!IsFinance) { ShowAlert("Not permitted for your role.", "alert-danger"); return; }
            int dcId = 0; int.TryParse(hfRecordEWB_DCID.Value, out dcId);
            if (dcId <= 0) return;

            string ewb = (txtEWBNo.Text ?? "").Trim();
            string ewbDtStr = (txtEWBDate.Text ?? "").Trim();
            string validStr = (txtEWBValid.Text ?? "").Trim();

            if (string.IsNullOrEmpty(ewb)) { ShowAlert("EWB Number is required.", "alert-danger"); return; }
            DateTime ewbDt;
            if (!DateTime.TryParse(ewbDtStr, out ewbDt))
            { ShowAlert("EWB Date is required and must be a valid date.", "alert-danger"); return; }

            DateTime? validUpto = null;
            if (!string.IsNullOrEmpty(validStr))
            {
                DateTime parsedV;
                if (!DateTime.TryParse(validStr, out parsedV))
                { ShowAlert("Valid Up To is not a valid date.", "alert-danger"); return; }
                validUpto = parsedV;
            }

            try
            {
                FINDatabaseHelper.RecordEWayBillManual(dcId, ewb, ewbDt, validUpto, UserID);
                pnlRecordEWB.Visible = false;
                ShowAlert("E-way bill recorded.", "alert-success");
                RefreshCurrentConsignment();
            }
            catch (Exception ex) { ShowAlert("Save EWB failed: " + ex.Message, "alert-danger"); }
        }

        protected void btnCancelEWB_Click(object sender, EventArgs e)
        {
            pnlRecordEWB.Visible = false;
        }

        /// <summary>Build a one-line "DC-XXX-NNN — Customer Name" for modal headers.</summary>
        string GetDCSummaryLine(int dcId)
        {
            try
            {
                var ds = FINDatabaseHelper.GetDCDetailForFIN(dcId);
                if (ds.Tables["Header"].Rows.Count == 0) return "DC #" + dcId;
                var h = ds.Tables["Header"].Rows[0];
                return Server.HtmlEncode(h["DCNumber"].ToString()) + " — " +
                       Server.HtmlEncode(h["CustomerName"].ToString());
            }
            catch { return "DC #" + dcId; }
        }

        // ══════════════════════════════════════════════════════════════
        // STATUS RENDERING HELPERS
        // ══════════════════════════════════════════════════════════════

        protected string GetStatusCss(string status)
        {
            switch (status)
            {
                case "DRAFT":     return "b-draft";
                case "FINALISED": return "b-final";
                case "CLOSED":    return "b-closed";
                default:          return "b-draft";
            }
        }

        protected string GetStatusLabel(string status)
        {
            switch (status)
            {
                case "DRAFT":     return "Draft";
                case "FINALISED": return "Finalised";
                case "CLOSED":    return "Closed";
                default:          return status;
            }
        }

        void ShowAlert(string msg, string cssClass)
        {
            litAlert.Text = msg;
            pnlAlert.CssClass = "alert " + cssClass;
            pnlAlert.Visible = true;
        }
    }
}
