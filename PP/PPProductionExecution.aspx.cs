using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using PPApp.DAL;

namespace PPApp
{
    public partial class PPProductionExecution : Page
    {
        private int    UserID   => Convert.ToInt32(Session["PP_UserID"]);
        private string FullName => Session["PP_FullName"]?.ToString() ?? "";

        // All control declarations
        protected Label         lblNavUser;
        protected Label         lblTodayDate;
        protected Label         lblAlert;
        protected Panel         pnlAlert;
        protected DropDownList  ddlShift;
        protected DropDownList  ddlProduct;
        protected Button        btnLoad;
        protected Panel         pnlInfo;
        protected Label         lblInfoProduct;
        protected Label         lblInfoCode;
        protected Label         lblInfoBatches;
        protected Label         lblInfoOutput;
        protected Label         lblInfoStatus;
        protected Label         lblInfoDate;
        protected Label         lblInfoCompleted;
        protected Panel         pnlExecution;
        protected Panel         pnlNoOrder;
        protected HiddenField   hfOrderID;
        protected HiddenField   hfExecutionID;
        protected HiddenField   hfTotalBatches;
        protected HiddenField   hfCurrentBatch;
        protected HiddenField   hfShowOutput;
        protected HiddenField   hfState;  // "ready" | "running" | "ended"
        protected Button        btnStart;
        protected Button        btnEnd;
        protected Panel         pnlOutput;
        protected System.Web.UI.WebControls.DropDownList ddlRemarks;
        protected Panel         pnlDynParams;
        protected Repeater      rptDynParams;
        protected Button        btnSaveOutput;
        protected Repeater      rptHistory;
        protected Panel         pnlHistoryEmpty;

        // ── PAGE LOAD ─────────────────────────────────────────────────────────
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PP_UserID"] == null) { Response.Redirect("PPLogin.aspx"); return; }
            lblNavUser.Text   = FullName;
            lblTodayDate.Text = PPDatabaseHelper.TodayIST().ToString("dddd, dd MMM yyyy").ToUpper();

            if (!IsPostBack)
            {
                pnlInfo.Visible = false;
                pnlExecution.Style["display"] = "none";
                LoadProductDropdown(1);
            }
            else
            {
                // Preserve dropdown selection
                string selVal = Request.Form[ddlProduct.UniqueID];
                LoadProductDropdown(Convert.ToInt32(ddlShift.SelectedValue));
                if (!string.IsNullOrEmpty(selVal) && selVal != "0")
                {
                    var item = ddlProduct.Items.FindByValue(selVal);
                    if (item != null) item.Selected = true;
                }
            }
        }

        // ── LOAD ORDER ────────────────────────────────────────────────────────
        protected void btnLoad_Click(object sender, EventArgs e)
        {
            pnlAlert.Visible = false;
            string selVal = Request.Form[ddlProduct.UniqueID];
            if (string.IsNullOrEmpty(selVal) || selVal == "0")
            { ShowAlert("Please select a product.", false); return; }

            int orderId = Convert.ToInt32(selVal);
            // Store in session — most reliable across postbacks
            Session["PE_OrderID"] = orderId;
            Session["PE_Shift"]   = Convert.ToInt32(ddlShift.SelectedValue);
            RenderOrder(orderId);
        }

        protected void ddlShift_Changed(object sender, EventArgs e)
        {
            LoadProductDropdown(Convert.ToInt32(ddlShift.SelectedValue));
            pnlInfo.Visible = false;
            pnlExecution.Style["display"] = "none";
            Session.Remove("PE_OrderID");
        }

        private void LoadProductDropdown(int shift)
        {
            var dt = PPDatabaseHelper.GetInitiatedOrdersForShift(shift, PPDatabaseHelper.TodayIST());
            ddlProduct.Items.Clear();
            ddlProduct.Items.Add(new System.Web.UI.WebControls.ListItem("-- Select Product --", "0"));
            foreach (DataRow r in dt.Rows)
                ddlProduct.Items.Add(new System.Web.UI.WebControls.ListItem(
                    r["ProductName"].ToString() + " (" + r["EffectiveBatches"] + " batches)",
                    r["OrderID"].ToString()));
        }

        // ── RENDER ORDER ──────────────────────────────────────────────────────
        // Single method that reads DB state and renders everything correctly
        private void RenderOrder(int orderId)
        {
            DataRow order = PPDatabaseHelper.GetProductionOrderById(orderId);
            if (order == null) { ShowAlert("Order not found.", false); return; }

            int    total  = Convert.ToInt32(Convert.ToDecimal(order["EffectiveBatches"]));
            string status = order["Status"].ToString();
            string outAbbr = order["OutputAbbr"].ToString();
            string prodType = order["ProductType"] != DBNull.Value ? order["ProductType"].ToString() : "Core";
            bool isConversion = (prodType == "Conversion");
            btnSaveOutput.Text = isConversion ? "Save & Store as Raw Material" : "Save & Move to Packing";
            Session["PE_ProductName"]  = order["ProductName"].ToString();
            Session["PE_IsConversion"] = isConversion;

            // Info panel
            pnlInfo.Visible        = true;
            lblInfoProduct.Text    = order["ProductName"].ToString();
            lblInfoCode.Text       = order["ProductCode"].ToString();
            if (lblInfoBatches != null) lblInfoBatches.Text = total.ToString();
            lblInfoOutput.Text     = Convert.ToDecimal(order["BatchSize"]).ToString("0.###") + " " + outAbbr + " per batch";
            lblInfoStatus.Text     = status;
            lblInfoStatus.CssClass = status == "Completed" ? "status-completed" :
                                     status == "InProgress" ? "status-inprogress" : "status-initiated";
            lblInfoDate.Text       = PPDatabaseHelper.TodayIST().ToString("dd MMM yyyy");
            // lblOutputUnit removed — Actual Output field removed from UI

            // Count completed
            var history = PPDatabaseHelper.GetBatchHistory(orderId);
            int done = 0;
            foreach (DataRow r in history.Rows)
                if (r["Status"].ToString() == "Completed") done++;
            lblInfoCompleted.Text = done + " of " + total + " batches done";

            // Bind history
            rptHistory.DataSource = history;
            rptHistory.DataBind();
            pnlHistoryEmpty.Visible = history.Rows.Count == 0;

            if (status == "Completed")
            {
                pnlExecution.Style["display"] = "none";
                ShowAlert("All " + total + " batches completed!", true);
                return;
            }

            if (status == "Stopped")
            {
                pnlExecution.Style["display"] = "block";
                SetState("ready", orderId, done + 1, total, 0);
                btnStart.Enabled = true;
                btnEnd.Enabled   = true;
                ClientScript.RegisterStartupScript(GetType(), "pkwheel_stopped",
                    "window.serverState='stopped';applyState();", true);
                ShowAlert("&#9654; This production order is currently <strong>Stopped</strong>. " +
                    "Go to Production Order to resume.", false);
                return;
            }

            pnlExecution.Style["display"] = "block";

            // Determine current batch state from DB
            DataRow active = PPDatabaseHelper.GetActiveBatch(orderId);
            DataRow ended  = PPDatabaseHelper.GetEndedBatch(orderId);
            int nextBatch  = done + 1;

            if (active != null)
            {
                // Batch running
                int bno = Convert.ToInt32(active["BatchNo"]);
                SetState("running", orderId, bno, total, Convert.ToInt32(active["ExecutionID"]));
            }
            else if (ended != null)
            {
                // Batch ended — awaiting output
                int bno = Convert.ToInt32(ended["BatchNo"]);
                SetState("ended", orderId, bno, total, Convert.ToInt32(ended["ExecutionID"]));
            }
            else
            {
                // Ready for next batch
                SetState("ready", orderId, nextBatch, total, 0);
            }
        }

        private void SetState(string state, int orderId, int batchNo, int total, int execId)
        {
            hfState.Value        = state;
            hfOrderID.Value      = orderId.ToString();
            hfCurrentBatch.Value = batchNo.ToString();
            hfTotalBatches.Value = total.ToString();
            hfExecutionID.Value  = execId.ToString();

            // Buttons always enabled — guards are in C# handlers
            btnStart.Enabled = true;
            btnEnd.Enabled   = true;

            // Output panel — show only when ended; panel is always in DOM
            pnlOutput.Style["display"] = (state == "ended") ? "block" : "none";

            // Dynamic params + remarks — load when output panel appears
            if (state == "ended")
            {
                try
                {
                    // Remarks dropdown
                    if (ddlRemarks != null)
                    {
                        ddlRemarks.Items.Clear();
                        ddlRemarks.Items.Add(new System.Web.UI.WebControls.ListItem("-- Select --",""));
                        foreach (DataRow rr in PPDatabaseHelper.GetRemarkOptions().Rows)
                            ddlRemarks.Items.Add(new System.Web.UI.WebControls.ListItem(
                                rr["OptionText"].ToString(), rr["OptionText"].ToString()));
                    }
                    // Dynamic params
                    if (pnlDynParams != null && orderId > 0)
                    {
                        var orderRow2 = PPDatabaseHelper.GetProductionOrderById(orderId);
                        if (orderRow2 != null)
                        {
                            var paramsDt = PPDatabaseHelper.GetProductParams(Convert.ToInt32(orderRow2["ProductID"]));
                            pnlDynParams.Visible    = paramsDt.Rows.Count > 0;
                            rptDynParams.DataSource = paramsDt;
                            rptDynParams.DataBind();
                        }
                    }
                }
                catch { /* non-fatal */ }
            }

            // JS wheel state — call applyState inline so it fires regardless of load order
            string js = state == "running"
                ? "window.batchNum='" + batchNo + "';window.totalBat='" + total + "';startWheel();if(typeof applyState==='function'){applyState();}if(typeof setButtonStates==='function'){setButtonStates('running');}"
                : "window.batchNum='" + batchNo + "';window.totalBat='" + total + "';stopWheel(" + (state == "ready" ? "true" : "false") + ");if(typeof applyState==='function'){applyState();}if(typeof setButtonStates==='function'){setButtonStates('ready');}";
            ClientScript.RegisterStartupScript(GetType(), "pkwheel_" + state, js, true);
        }

        // ── START BATCH ───────────────────────────────────────────────────────
        protected void btnStart_Click(object sender, EventArgs e)
        {
            int orderId = GetOrderId();
            if (orderId == 0) { ShowAlert("No product loaded.", false); return; }
            int batchNo = Convert.ToInt32(hfCurrentBatch.Value);
            if (batchNo == 0) batchNo = 1;

            // Verify no batch already running
            if (PPDatabaseHelper.GetActiveBatch(orderId) != null)
            { ShowAlert("A batch is already in progress.", false); RenderOrder(orderId); return; }

            int execId = PPDatabaseHelper.StartBatch(orderId, batchNo, UserID);
            ShowAlert("Batch " + batchNo + " started.", true);
            RenderOrder(orderId);
        }

        // ── END BATCH ─────────────────────────────────────────────────────────
        protected void btnEnd_Click(object sender, EventArgs e)
        {
            int orderId = GetOrderId();
            if (orderId == 0) { ShowAlert("No product loaded.", false); return; }

            DataRow active = PPDatabaseHelper.GetActiveBatch(orderId);
            if (active == null) { ShowAlert("No active batch found. Please press START first.", false); RenderOrder(orderId); return; }

            int execId = Convert.ToInt32(active["ExecutionID"]);
            PPDatabaseHelper.EndBatch(execId, orderId);

            // Verify EndBatch worked
            DataRow ended = PPDatabaseHelper.GetEndedBatch(orderId);
            if (ended == null)
                ShowAlert("Batch ended. Please enter actual output and save.", false);
            else
                ShowAlert("Batch ended. Please enter actual output and save.", true);

            RenderOrder(orderId);
        }

        // ── SAVE OUTPUT ───────────────────────────────────────────────────────
        protected void btnSaveOutput_Click(object sender, EventArgs e)
        {
            int orderId = GetOrderId();
            if (orderId == 0) { ShowAlert("No product loaded.", false); return; }

            DataRow ended = PPDatabaseHelper.GetEndedBatch(orderId);
            if (ended == null) { ShowAlert("No batch awaiting output. Please press END first.", false); return; }

            int execId  = Convert.ToInt32(ended["ExecutionID"]);
            int batchNo = Convert.ToInt32(ended["BatchNo"]);
            int total   = Convert.ToInt32(hfTotalBatches.Value);
            if (total == 0) total = Convert.ToInt32(
                Convert.ToDecimal(PPDatabaseHelper.GetProductionOrderById(orderId)["EffectiveBatches"]));
            // Use batch size as actual output (field removed from UI)
            decimal output = 1;
            var orderRowOut = PPDatabaseHelper.GetProductionOrderById(orderId);
            if (orderRowOut != null && orderRowOut["BatchSize"] != DBNull.Value)
                output = Convert.ToDecimal(orderRowOut["BatchSize"]);

            // ── FIFO STOCK DEDUCTION ──────────────────────────────────────────
            int productId = Convert.ToInt32(PPDatabaseHelper.GetProductionOrderById(orderId)["ProductID"]);
            try
            {
                PPDatabaseHelper.DeductStockFIFO(execId, orderId, batchNo, productId, UserID);
            }
            catch (Exception stockEx)
            {
                if (stockEx.Message.StartsWith("STOCK_SHORTFALL:"))
                {
                    ShowAlert("<strong>Cannot save — insufficient stock for this batch:</strong><br/>" +
                        stockEx.Message.Substring(16).Replace("|", "<br/>"), false);
                    return;
                }
                throw;
            }

            PPDatabaseHelper.SaveBatchOutput(execId, output, ddlRemarks != null ? ddlRemarks.SelectedValue : "", orderId, total);

            // Save dynamic batch params
            var paramsDt2 = PPDatabaseHelper.GetProductParams(
                Convert.ToInt32(PPDatabaseHelper.GetProductionOrderById(orderId)["ProductID"]));
            if (paramsDt2.Rows.Count > 0)
            {
                var pidList = new System.Collections.Generic.List<int>();
                var valList = new System.Collections.Generic.List<decimal?>();
                foreach (System.Data.DataRow pr in paramsDt2.Rows)
                {
                    int pid = Convert.ToInt32(pr["ParamID"]);
                    string fieldVal = Request.Form["dynparam_" + pid];
                    decimal v;
                    pidList.Add(pid);
                    valList.Add(decimal.TryParse(fieldVal, out v) ? v : (decimal?)null);
                }
                PPDatabaseHelper.SaveBatchParams(execId, pidList.ToArray(), valList.ToArray());
            }

            bool isConversion = Session["PE_IsConversion"] != null && (bool)Session["PE_IsConversion"];
            if (isConversion)
            {
                string productName = Session["PE_ProductName"]?.ToString() ?? "";
                DataRow rm = PPDatabaseHelper.GetRMByName(productName);
                if (rm != null)
                {
                    PPDatabaseHelper.AddInternalGRN(Convert.ToInt32(rm["RMID"]), output, productName, orderId, batchNo, UserID);
                    ShowAlert("Batch " + batchNo + " of " + total + " saved. "
                        + output.ToString("0.###") + " added to " + productName + " stock.", true);
                }
                else
                    ShowAlert("Batch saved, but no Raw Material named '" + productName + "' found. Update stock manually.", false);
            }
            else
                ShowAlert("Batch " + batchNo + " of " + total + " saved successfully.", true);

            if (ddlRemarks != null) ddlRemarks.SelectedIndex = 0;
            RenderOrder(orderId);
        }

        // ── HELPERS ──────────────────────────────────────────────────────────
        private int GetOrderId()
        {
            // Session is the most reliable store across postbacks
            if (Session["PE_OrderID"] != null)
                return Convert.ToInt32(Session["PE_OrderID"]);
            // Fallback to hidden field
            string v = hfOrderID.Value;
            if (!string.IsNullOrEmpty(v) && v != "0") return Convert.ToInt32(v);
            // Fallback to form post
            string f = Request.Form[hfOrderID.UniqueID];
            if (!string.IsNullOrEmpty(f) && f != "0") return Convert.ToInt32(f);
            return 0;
        }

        protected string FormatTime(object val)
        {
            if (val == null || val == DBNull.Value) return "—";
            return Convert.ToDateTime(val).ToString("hh:mm tt");
        }

        protected string FormatOutput(object val, object abbr)
        {
            if (val == null || val == DBNull.Value) return "—";
            return Convert.ToDecimal(val).ToString("0.###") + " " + abbr?.ToString();
        }

        protected string BuildParamDropdown(string paramId, string options)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("<select name=\"dynparam_" + paramId + "\" id=\"dynparam_" + paramId + "\" class=\"dyn-param-sel\">");
            sb.Append("<option value=\"\">-- Select --</option>");
            foreach (string opt in options.Split(','))
            {
                string o = opt.Trim();
                if (!string.IsNullOrEmpty(o))
                    sb.Append("<option value=\"" + System.Web.HttpUtility.HtmlEncode(o) + "\">" + System.Web.HttpUtility.HtmlEncode(o) + "</option>");
            }
            sb.Append("</select>");
            return sb.ToString();
        }

        private void ShowAlert(string msg, bool success)
        {
            string icon = success ? "&#10003;" : "&#9888;";
            string bg   = success ? "#d1f5e0" : "#fdf3f2";
            string col  = success ? "#155724" : "#842029";
            string bdr  = success ? "#a3d9b1" : "#f5c2c7";
            lblAlert.Text = string.Format(
                "<div style='display:flex;align-items:flex-start;gap:10px;padding:12px 16px;" +
                "border-radius:10px;font-size:13px;line-height:1.6;" +
                "background:{0};color:{1};border:1px solid {2};'>" +
                "<span style='font-size:16px;flex-shrink:0;'>{3}</span>" +
                "<span>{4}</span></div>",
                bg, col, bdr, icon, msg);
            pnlAlert.Visible = true;
        }
    }
}
