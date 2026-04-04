using System;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using PPApp.DAL;

namespace PPApp
{
    public partial class PPProductionOrder : Page
    {
        private int    UserID   => Convert.ToInt32(Session["PP_UserID"]);
        private string FullName => Session["PP_FullName"]?.ToString() ?? "";

        protected Label     lblNavUser;
        protected Label     lblTodayDate;
        protected Label     lblPlanStatus;
        protected Label     lblAlert;
        protected Panel     pnlAlert;
        protected HiddenField hfPlanID;
        protected HiddenField hfActiveShift;

        // Shift tabs
        protected LinkButton btnShift1Tab;
        protected LinkButton btnShift2Tab;

        // Order repeaters
        protected Repeater  rptShift1Orders;
        protected Repeater  rptShift2Orders;
        protected Panel     pnlShift1Empty;
        protected Panel     pnlShift2Empty;
        protected Panel     pnlNoplan;

        // Progress panel
        protected Repeater  rptProgress;
        protected Panel     pnlProgressEmpty;

        // Priority buttons
        protected global::System.Web.UI.WebControls.Button btnClearPriority1, btnClearPriority2;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PP_UserID"] == null) { Response.Redirect("PPLogin.aspx"); return; }

            // Module access check
            string __role = Session["PP_Role"]?.ToString() ?? "";
            if (!PPDatabaseHelper.RoleHasModuleAccess(__role, "PP", "PP_ORDER"))
            { Response.Redirect("PPHome.aspx"); return; }
            lblNavUser.Text = FullName;
            lblTodayDate.Text = PPDatabaseHelper.TodayIST().ToString("dddd, dd MMM yyyy").ToUpper();

            if (!IsPostBack)
            {
                hfActiveShift.Value = "1";
                LoadPage();
            }
        }

        private void LoadPage()
        {
            DateTime today = PPDatabaseHelper.TodayIST();

            // Check if plan exists for today
            DataRow plan = PPDatabaseHelper.GetDailyPlan(today);
            if (plan == null)
            {
                pnlNoplan.Visible = true;
                lblPlanStatus.Text = "No Plan";
                lblPlanStatus.CssClass = "status-badge draft";
                hfPlanID.Value = "0";
                return;
            }

            pnlNoplan.Visible = false;
            int planId = Convert.ToInt32(plan["PlanID"]);
            hfPlanID.Value = planId.ToString();

            string status = plan["Status"].ToString();
            lblPlanStatus.Text = status;
            lblPlanStatus.CssClass = status == "Confirmed"
                ? "status-badge confirmed" : "status-badge draft";

            // Block production orders until plan is confirmed
            if (status != "Confirmed")
            {
                pnlNoplan.Visible = true;
                pnlNoplan.Controls.Clear();
                pnlNoplan.Controls.Add(new System.Web.UI.LiteralControl(
                    "<div class='no-plan-warn'>" +
                    "&#9888;&nbsp; The production plan for today is currently in <strong>Draft</strong> status. " +
                    "Production orders can only be created after the plan is <strong>Confirmed</strong>. " +
                    "Please go to <a href='PPDailyPlan.aspx'>Production Planning</a> and confirm the plan first." +
                    "</div>"));
                return;
            }

            pnlNoplan.Visible = false;

            // Load orders for both shifts
            BindShiftOrders(planId, today);

            // Load progress panel
            BindProgress(today);

            // Set active tab highlight
            SetActiveTab(hfActiveShift.Value == "2" ? 2 : 1);
        }

        private void BindShiftOrders(int planId, DateTime today)
        {
            var s1 = PPDatabaseHelper.GetOrCreateProductionOrders(planId, 1, today, UserID);
            var s2 = PPDatabaseHelper.GetOrCreateProductionOrders(planId, 2, today, UserID);

            rptShift1Orders.DataSource = s1; rptShift1Orders.DataBind();
            rptShift2Orders.DataSource = s2; rptShift2Orders.DataBind();

            pnlShift1Empty.Visible = s1.Rows.Count == 0;
            pnlShift2Empty.Visible = s2.Rows.Count == 0;
        }

        private void BindProgress(DateTime today)
        {
            var dt = PPDatabaseHelper.GetTodayOrderProgress(today);
            pnlProgressEmpty.Visible = dt.Rows.Count == 0;
            rptProgress.DataSource   = dt;
            rptProgress.DataBind();
        }

        private void SetActiveTab(int shift)
        {
            hfActiveShift.Value = shift.ToString();
            if (btnShift1Tab != null)
                btnShift1Tab.CssClass = shift == 1 ? "shift-tab active" : "shift-tab";
            if (btnShift2Tab != null)
                btnShift2Tab.CssClass = shift == 2 ? "shift-tab active s2" : "shift-tab s2";
        }

        // ── TAB SWITCH ───────────────────────────────────────────────────────
        protected void btnShift1_Click(object sender, EventArgs e)
        {
            SetActiveTab(1);
            BindProgress(PPDatabaseHelper.TodayIST());
        }

        protected void btnShift2_Click(object sender, EventArgs e)
        {
            SetActiveTab(2);
            BindProgress(PPDatabaseHelper.TodayIST());
        }

        // ── INITIATE ORDER ───────────────────────────────────────────────────
        protected void rptShift1Orders_ItemCommand(object source, RepeaterCommandEventArgs e)
            => HandleItemCommand(e);

        protected void rptShift2Orders_ItemCommand(object source, RepeaterCommandEventArgs e)
            => HandleItemCommand(e);

        private void HandleItemCommand(RepeaterCommandEventArgs e)
        {
            int orderId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "Initiate")
            {
                // Save revised batches if provided
                var txtRevised = e.Item.FindControl("txtRevised") as TextBox;
                if (txtRevised != null && !string.IsNullOrEmpty(txtRevised.Text.Trim()))
                {
                    decimal revised;
                    if (decimal.TryParse(txtRevised.Text.Trim(), out revised) && revised > 0)
                        PPDatabaseHelper.UpdateRevisedBatches(orderId, revised);
                }

                // Server-side stock guard
                var sf = PPDatabaseHelper.CheckStockForOrder(orderId);
                if (sf.Rows.Count > 0)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.Append("<strong>Insufficient stock — cannot initiate.</strong><br/>");
                    sb.Append("<table style='width:100%;margin-top:8px;font-size:12px;border-collapse:collapse;'>"); 
                    sb.Append("<tr style='font-weight:700;'>"); 
                    sb.Append("<td style='padding:4px 6px;'>Raw Material</td>"); 
                    sb.Append("<td style='padding:4px 6px;text-align:right;'>Required</td>"); 
                    sb.Append("<td style='padding:4px 6px;text-align:right;'>In Stock</td>"); 
                    sb.Append("<td style='padding:4px 6px;text-align:right;color:#c0392b;'>Shortfall</td></tr>"); 
                    foreach (DataRow row in sf.Rows)
                    {
                        string u = row["RMUnit"].ToString();
                        decimal req = Convert.ToDecimal(row["Required"]);
                        decimal ins = Convert.ToDecimal(row["InStock"]);
                        decimal sht = Convert.ToDecimal(row["Shortfall"]);
                        sb.Append("<tr>");
                        sb.Append("<td style='padding:3px 6px;'>" + System.Web.HttpUtility.HtmlEncode(row["RMName"].ToString()) + "</td>");
                        sb.Append("<td style='padding:3px 6px;text-align:right;'>" + req.ToString("0.###") + " " + u + "</td>");
                        sb.Append("<td style='padding:3px 6px;text-align:right;'>" + ins.ToString("0.###") + " " + u + "</td>");
                        sb.Append("<td style='padding:3px 6px;text-align:right;color:#c0392b;font-weight:700;'>-" + sht.ToString("0.###") + " " + u + "</td>");
                        sb.Append("</tr>");
                    }
                    sb.Append("</table>");
                    ShowAlert(sb.ToString(), false);
                    LoadPage(); return;
                }

                bool ok = PPDatabaseHelper.InitiateOrder(orderId);
                if (ok)
                {
                    ShowAlert("Production initiated successfully.", true);
                    LoadPage();
                }
                else
                {
                    ShowAlert("Could not initiate — order may already be in progress.", false);
                    LoadPage();
                }
            }
            else if (e.CommandName == "SaveRevised")
            {
                var txtRevised = e.Item.FindControl("txtRevised") as TextBox;
                if (txtRevised == null) { LoadPage(); return; }
                decimal revised;
                if (!decimal.TryParse(txtRevised.Text.Trim(), out revised) || revised <= 0)
                { ShowAlert("Please enter a valid number of batches.", false); LoadPage(); return; }

                // Validate: revised cannot be less than completed batches
                int completed = GetCompletedBatches(orderId);
                if (revised < completed)
                {
                    ShowAlert("Revised batches cannot be less than completed batches (" + completed + ").", false);
                    LoadPage(); return;
                }
                PPDatabaseHelper.UpdateRevisedBatches(orderId, revised);
                ShowAlert("Revised batches updated to " + revised + ".", true);
                LoadPage();
            }
            else if (e.CommandName == "Stop")
            {
                PPDatabaseHelper.StopOrder(orderId);
                ShowAlert("Production stopped.", false);
                LoadPage();
            }
            else if (e.CommandName == "Resume")
            {
                PPDatabaseHelper.ResumeOrder(orderId);
                ShowAlert("Production resumed.", true);
                LoadPage();
            }
            else if (e.CommandName == "SetPriority")
            {
                int shift = Convert.ToInt32(hfActiveShift.Value);
                DateTime today = PPDatabaseHelper.TodayIST();
                var orders = PPDatabaseHelper.GetOrCreateProductionOrders(
                    GetCurrentPlanId(), shift, today, UserID);

                int maxPriority = 0;
                foreach (DataRow r in orders.Rows)
                    if (r["ExecutionPriority"] != DBNull.Value)
                    { int p = Convert.ToInt32(r["ExecutionPriority"]); if (p > maxPriority) maxPriority = p; }

                DataRow thisOrder = PPDatabaseHelper.GetProductionOrder(orderId);
                if (thisOrder != null && thisOrder["ExecutionPriority"] != DBNull.Value && Convert.ToInt32(thisOrder["ExecutionPriority"]) > 0)
                {
                    PPDatabaseHelper.SetExecutionPriority(orderId, 0);
                    ResequencePriorities(shift, today);
                }
                else
                {
                    PPDatabaseHelper.SetExecutionPriority(orderId, maxPriority + 1);
                }
                LoadPage();
            }
        }

        private void ResequencePriorities(int shift, DateTime date)
        {
            int planId = GetCurrentPlanId();
            var orders = PPDatabaseHelper.GetOrCreateProductionOrders(planId, shift, date, UserID);
            var prioritized = new System.Collections.Generic.List<int>();
            foreach (DataRow r in orders.Rows)
                if (r["ExecutionPriority"] != DBNull.Value && Convert.ToInt32(r["ExecutionPriority"]) > 0)
                    prioritized.Add(Convert.ToInt32(r["OrderID"]));
            for (int i = 0; i < prioritized.Count; i++)
                PPDatabaseHelper.SetExecutionPriority(prioritized[i], i + 1);
        }

        private int GetCurrentPlanId()
        {
            DateTime today = PPDatabaseHelper.TodayIST();
            var plan = PPDatabaseHelper.GetDailyPlan(today);
            return plan != null ? Convert.ToInt32(plan["PlanID"]) : 0;
        }

        protected void btnClearPriority1_Click(object s, EventArgs e)
        { PPDatabaseHelper.ClearExecutionPriorities(1, PPDatabaseHelper.TodayIST()); ShowAlert("Priority cleared for Shift 1.", true); LoadPage(); }

        protected void btnClearPriority2_Click(object s, EventArgs e)
        { PPDatabaseHelper.ClearExecutionPriorities(2, PPDatabaseHelper.TodayIST()); ShowAlert("Priority cleared for Shift 2.", true); LoadPage(); }

        protected string GetPriorityBtnClass(object status, object priority)
        {
            string st = status?.ToString() ?? "";
            if (st == "Completed") return "priority-btn completed";
            if (priority != null && priority != DBNull.Value && Convert.ToInt32(priority) > 0) return "priority-btn set";
            return "priority-btn";
        }

        private int GetCompletedBatches(int orderId)
        {
            var dt = PPDatabaseHelper.GetBatchHistory(orderId);
            int count = 0;
            foreach (DataRow r in dt.Rows)
                if (r["Status"].ToString() == "Completed") count++;
            return count;
        }

        // ── HELPERS ──────────────────────────────────────────────────────────
        protected string StatusClass(object status)
        {
            switch (status?.ToString() ?? "")
            {
                case "Initiated":  return "badge-initiated";
                case "InProgress": return "badge-inprogress";
                case "Completed":  return "badge-completed";
                case "Stopped":    return "badge-stopped";
                default:           return "badge-pending";
            }
        }

        protected string ButtonLabel(object status)
        {
            switch (status?.ToString() ?? "")
            {
                case "Initiated":  return "In Progress";
                case "InProgress": return "In Progress";
                case "Completed":  return "Completed";
                default:           return "Initiate Production";
            }
        }

        protected bool CanInitiate(object status)
            => status?.ToString() == "Pending";

        protected string FormatOutput(object batches, object batchSize, object abbr)
        {
            try
            {
                decimal b  = Convert.ToDecimal(batches);
                decimal bs = Convert.ToDecimal(batchSize);
                return (b * bs).ToString("0.###") + " " + abbr?.ToString();
            }
            catch { return "—"; }
        }

        protected string BuildRMEstimate(object orderIdObj)
        {
            try
            {
                int orderId = Convert.ToInt32(orderIdObj);
                var dt = PPDatabaseHelper.GetOrderRMEstimate(orderId);
                if (dt.Rows.Count == 0) return "<span style='color:var(--text-dim)'>No BOM defined</span>";

                var sb = new StringBuilder();
                foreach (DataRow row in dt.Rows)
                {
                    decimal qty   = Convert.ToDecimal(row["EstimatedQty"]);
                    string  rmUom = row["RM_UOM"].ToString().Trim().ToLower();

                    // Normalise units
                    if      (rmUom == "g")  { qty = qty / 1000m; rmUom = "kg"; }
                    else if (rmUom == "mg") { qty = qty / 1000000m; rmUom = "kg"; }
                    else if (rmUom == "ml") { qty = qty / 1000m; rmUom = "l"; }

                    sb.AppendFormat(
                        "<div class='rm-est-row'><span class='rm-est-name'>{0}</span>" +
                        "<span class='rm-est-qty'>{1} {2}</span></div>",
                        System.Web.HttpUtility.HtmlEncode(row["RMName"].ToString()),
                        qty.ToString("0.###"),
                        rmUom.ToUpper());
                }
                return sb.ToString();
            }
            catch { return "—"; }
        }

        protected string GetInitiateCssClass(object st, object oid)
        {
            return CanInitiateWithStock(st, oid) ? "btn-initiate" : "btn-stock-short";
        }

        protected string GetInitiateLabel(object st, object oid)
        {
            return CanInitiateWithStock(st, oid) ? "Initiate" : "Stock Short";
        }

        protected bool CanInitiateWithStock(object statusObj, object orderIdObj)
        {
            if ((statusObj == null ? "" : statusObj.ToString()) != "Pending") return false;
            try { return PPDatabaseHelper.CheckStockForOrder(Convert.ToInt32(orderIdObj)).Rows.Count == 0; }
            catch { return false; }
        }

        protected string StockTooltip(object statusObj, object orderIdObj)
        {
            if ((statusObj == null ? "" : statusObj.ToString()) != "Pending") return "";
            try
            {
                var sf = PPDatabaseHelper.CheckStockForOrder(Convert.ToInt32(orderIdObj));
                if (sf.Rows.Count == 0) return "";
                var parts = new System.Collections.Generic.List<string>();
                foreach (DataRow r in sf.Rows)
                    parts.Add(r["RMName"] + ": short by " + Convert.ToDecimal(r["Shortfall"]).ToString("0.###") + " " + r["RMUnit"]);
                return "Insufficient stock: " + string.Join(", ", parts);
            }
            catch { return ""; }
        }

        protected string FormatProgress(object completed, object effective, object ordered)
        {
            try {
                int c = Convert.ToInt32(completed);
                int e = Convert.ToInt32(Convert.ToDecimal(effective));
                int o = Convert.ToInt32(Convert.ToDecimal(ordered));
                if (e == o) return c + " / " + e;
                return c + " / " + e + " (" + o + " ordered)";
            } catch { return "—"; }
        }

        // Variation: Actual output vs Expected output for completed batches only
        // Expected = BatchSize × CompletedBatches (apples-to-apples comparison)
        protected string FormatVariation(object actualOut, object batchSize, object completed, object abbr)
        {
            try {
                decimal actual    = Convert.ToDecimal(actualOut);
                decimal bsize     = Convert.ToDecimal(batchSize);
                decimal done      = Convert.ToDecimal(completed);
                if (actual == 0 || done == 0) return "—";
                decimal expected  = bsize * done;  // expected from completed batches only
                if (expected == 0) return "—";
                decimal diff      = actual - expected;
                decimal pct       = Math.Round(diff / expected * 100, 1);
                string sign       = diff >= 0 ? "+" : "";
                string color      = diff >= 0 ? "var(--accent-dark)" : "var(--red)";
                return string.Format(
                    "<span style='color:{0}'>{1}{2} {3}<br/><small style='font-weight:normal;'>" +
                    "Actual {4} vs Expected {5} {3} ({6}{7}%)</small></span>",
                    color, sign, diff.ToString("0.###"), abbr,
                    actual.ToString("0.###"), expected.ToString("0.###"),
                    sign, pct);
            } catch { return "—"; }
        }

        protected string ProgressBarWidth(object completed, object effective)
        {
            try {
                decimal c = Convert.ToDecimal(completed);
                decimal e = Convert.ToDecimal(effective);
                if (e == 0) return "0";
                return Math.Min(100, Math.Round(c / e * 100)).ToString();
            } catch { return "0"; }
        }

        private void ShowAlert(string msg, bool success)
        {
            lblAlert.Text     = msg;
            pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger");
            pnlAlert.Visible  = true;
        }
    }
}
