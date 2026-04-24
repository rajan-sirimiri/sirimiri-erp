using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using PPApp.DAL;

namespace PPApp
{
    public partial class PPProductionExecution : Page
    {
        private int UserID => Convert.ToInt32(Session["PP_UserID"]);
        private string FullName => Session["PP_FullName"]?.ToString() ?? "";

        // All control declarations
        protected Label lblNavUser;
        protected Label lblTodayDate;
        protected Label lblAlert;
        protected Panel pnlAlert;
        protected DropDownList ddlShift;
        protected DropDownList ddlProduct;
        protected DropDownList ddlLine;
        protected Button btnLoad;

        protected Panel pnlInfo;
        protected Label lblInfoProduct;
        protected Label lblInfoCode;
        protected Label lblGrammage;
        protected Label lblInfoBatches;
        protected Label lblInfoOutput;
        protected Label lblInfoStatus;
        protected Label lblInfoDate;
        protected Label lblInfoCompleted;

        protected Panel pnlExecution;
        protected Panel pnlNoOrder;
        protected HiddenField hfOrderID;
        protected HiddenField hfExecutionID;
        protected HiddenField hfTotalBatches;
        protected HiddenField hfCurrentBatch;
        protected HiddenField hfShowOutput;
        protected HiddenField hfState; // "ready" | "running" | "ended"

        protected Button btnStart;
        protected Button btnEnd;
        protected Panel pnlOutput;
        protected System.Web.UI.WebControls.DropDownList ddlRemarks;
        protected Panel pnlDynParams;
        protected Repeater rptDynParams;
        protected Button btnSaveOutput;
        protected Panel pnlDoughWeight;
        protected TextBox txtDoughWeight;
        protected Label lblUnitWeight;
        protected HiddenField hfUnitWeightGrams;
        protected HiddenField hfCalcUnits;
        protected Panel pnlTrays;
        protected TextBox txtNoOfTrays;
        protected TextBox txtPartialUnits;
        protected Label lblUnitsPerTray;
        protected HiddenField hfUnitsPerTray;
        protected HiddenField hfCalcTrayUnits;
        protected Repeater rptHistory;
        protected Panel pnlHistoryEmpty;

        // ═══════════ SHIFT CONTROLS (NEW) ═══════════
        protected HiddenField hfShiftID;        // 0 = no active shift
        protected HiddenField hfShiftActive;    // "1" if active
        protected Panel pnlShiftStart;          // red banner with START SHIFT button
        protected Panel pnlShiftActive;         // green banner with shift info + END SHIFT button
        protected Button btnStartShift;
        protected Button btnEndShift;
        protected Label lblShiftStartShiftLbl;  // "Shift 1"
        protected Label lblShiftStartLineLbl;   // "All Lines" / "<Line name>"
        protected Label lblShiftInfo;           // "Shift 1"
        protected Label lblShiftStartTime;      // "09:12 AM"
        protected Label lblShiftStartedBy;      // "Rajan"
        protected Label lblShiftDuration;       // "01:45"
        protected Label lblShiftTargetBatches;
        protected Label lblShiftCompletedBatches;
        protected Label lblShiftLine;

        // ── PAGE LOAD ─────────────────────────────────────────────────────────
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PP_UserID"] == null) { Response.Redirect("PPLogin.aspx"); return; }

            string __role = Session["PP_Role"]?.ToString() ?? "";
            if (!PPDatabaseHelper.RoleHasModuleAccess(__role, "PP", "PP_EXECUTION"))
            { Response.Redirect("PPHome.aspx"); return; }

            lblNavUser.Text = FullName;
            lblTodayDate.Text = PPDatabaseHelper.TodayIST().ToString("dddd, dd MMM yyyy").ToUpper();

            if (!IsPostBack)
            {
                pnlInfo.Visible = false;
                pnlExecution.Style["display"] = "none";
                LoadProductionLines();
                LoadProductDropdown(1);
                RefreshShiftBanner();     // ── NEW: decide whether to show start or active banner
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

                RefreshShiftBanner();     // ── NEW: re-apply banner + lock state on every postback
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
            RefreshShiftBanner();
        }

        protected void ddlLine_Changed(object sender, EventArgs e)
        {
            LoadProductDropdown(Convert.ToInt32(ddlShift.SelectedValue));
            pnlInfo.Visible = false;
            pnlExecution.Style["display"] = "none";
            Session.Remove("PE_OrderID");
            RefreshShiftBanner();
        }

        private void LoadProductionLines()
        {
            if (ddlLine == null) return;
            ddlLine.Items.Clear();
            ddlLine.Items.Add(new System.Web.UI.WebControls.ListItem("-- All Lines --", "0"));
            var dt = PPDatabaseHelper.GetActiveProductionLines();
            foreach (DataRow r in dt.Rows)
                ddlLine.Items.Add(new System.Web.UI.WebControls.ListItem(r["LineName"].ToString(), r["LineID"].ToString()));
        }

        private void LoadProductDropdown(int shift)
        {
            int lineId = ddlLine != null && ddlLine.SelectedValue != "0"
                ? Convert.ToInt32(ddlLine.SelectedValue) : 0;
            var dt = lineId > 0
                ? PPDatabaseHelper.GetInitiatedOrdersForShiftAndLine(shift, PPDatabaseHelper.TodayIST(), lineId)
                : PPDatabaseHelper.GetInitiatedOrdersForShift(shift, PPDatabaseHelper.TodayIST());

            ddlProduct.Items.Clear();
            ddlProduct.Items.Add(new System.Web.UI.WebControls.ListItem("-- Select Product --", "0"));

            bool anyPrioritySet = false;
            foreach (DataRow r in dt.Rows)
                if (r["ExecutionPriority"] != DBNull.Value && Convert.ToInt32(r["ExecutionPriority"]) > 0)
                { anyPrioritySet = true; break; }

            var lineWithPriority1 = new System.Collections.Generic.HashSet<int>();
            if (anyPrioritySet)
            {
                foreach (DataRow r in dt.Rows)
                {
                    if (r["ExecutionPriority"] != DBNull.Value && Convert.ToInt32(r["ExecutionPriority"]) == 1)
                    {
                        DataRow orderDetail = PPDatabaseHelper.GetProductionOrder(Convert.ToInt32(r["OrderID"]));
                        if (orderDetail != null && orderDetail["ProductionLineID"] != DBNull.Value)
                            lineWithPriority1.Add(Convert.ToInt32(orderDetail["ProductionLineID"]));
                    }
                }
            }

            foreach (DataRow r in dt.Rows)
            {
                string priority = r["ExecutionPriority"] != DBNull.Value ? Convert.ToInt32(r["ExecutionPriority"]).ToString() : "";
                string prefix = !string.IsNullOrEmpty(priority) && Convert.ToInt32(priority) > 0
                    ? "[#" + priority + "] " : "";
                string status = r["Status"].ToString();

                bool isActive = !anyPrioritySet
                             || status == "InProgress"
                             || (!string.IsNullOrEmpty(priority) && Convert.ToInt32(priority) == 1);

                var item = new System.Web.UI.WebControls.ListItem(
                    prefix + r["ProductName"].ToString() + " (" + r["EffectiveBatches"] + " batches)",
                    r["OrderID"].ToString());

                if (!isActive) item.Attributes.Add("disabled", "disabled");
                ddlProduct.Items.Add(item);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // SHIFT BANNER LOGIC
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Figures out current shift state for (today, ddlShift, ddlLine) and
        /// renders either pnlShiftStart or pnlShiftActive accordingly. Also locks
        /// the shift/line dropdowns when a shift is active.
        /// </summary>
        private void RefreshShiftBanner()
        {
            int shift   = Convert.ToInt32(ddlShift.SelectedValue);
            int lineId  = ddlLine != null && ddlLine.SelectedValue != "0"
                              ? Convert.ToInt32(ddlLine.SelectedValue) : 0;
            DateTime today = PPDatabaseHelper.TodayIST();

            DataRow activeShift = PPDatabaseHelper.GetActiveProductionShift(today, shift, lineId);

            string lineLabel = lineId > 0
                ? (ddlLine.SelectedItem != null ? ddlLine.SelectedItem.Text : "Line " + lineId)
                : "All Lines";
            string shiftLabel = "Shift " + shift;

            if (activeShift == null)
            {
                // No active shift — show START banner, unlock dropdowns
                hfShiftID.Value      = "0";
                hfShiftActive.Value  = "0";
                pnlShiftStart.Visible  = true;
                pnlShiftActive.Visible = false;

                if (lblShiftStartShiftLbl != null) lblShiftStartShiftLbl.Text = shiftLabel;
                if (lblShiftStartLineLbl  != null) lblShiftStartLineLbl.Text  = lineLabel;

                // Dropdowns unlocked
                if (ddlShift != null) ddlShift.Enabled = true;
                if (ddlLine  != null) ddlLine.Enabled  = true;
            }
            else
            {
                // Active shift — show ACTIVE banner, lock dropdowns
                int shiftId = Convert.ToInt32(activeShift["ShiftID"]);
                hfShiftID.Value      = shiftId.ToString();
                hfShiftActive.Value  = "1";
                pnlShiftStart.Visible  = false;
                pnlShiftActive.Visible = true;

                DateTime startTime = Convert.ToDateTime(activeShift["StartTime"]);
                string startedBy   = activeShift["StartedByName"] != DBNull.Value
                                       ? activeShift["StartedByName"].ToString() : "—";

                if (lblShiftInfo         != null) lblShiftInfo.Text         = shiftLabel;
                if (lblShiftStartTime    != null) lblShiftStartTime.Text    = startTime.ToString("hh:mm tt");
                if (lblShiftStartedBy    != null) lblShiftStartedBy.Text    = startedBy;
                if (lblShiftLine         != null) lblShiftLine.Text         = lineLabel;

                TimeSpan elapsed = PPDatabaseHelper.NowIST() - startTime;
                if (lblShiftDuration != null)
                    lblShiftDuration.Text = ((int)elapsed.TotalHours).ToString("D2") + ":" +
                                            elapsed.Minutes.ToString("D2");

                // Live target / completed counts
                DataRow totals = PPDatabaseHelper.GetProductionShiftTotals(today, shift, lineId);
                int target = 0, done = 0;
                if (totals != null)
                {
                    target = totals["TargetBatches"]    != DBNull.Value ? Convert.ToInt32(totals["TargetBatches"])    : 0;
                    done   = totals["CompletedBatches"] != DBNull.Value ? Convert.ToInt32(totals["CompletedBatches"]) : 0;
                }
                if (lblShiftTargetBatches    != null) lblShiftTargetBatches.Text    = target.ToString();
                if (lblShiftCompletedBatches != null) lblShiftCompletedBatches.Text = done.ToString();

                // Lock shift / line dropdowns while shift is active
                if (ddlShift != null) ddlShift.Enabled = false;
                if (ddlLine  != null) ddlLine.Enabled  = false;
            }
        }

        /// <summary>Returns true if a shift is currently active for the filter.</summary>
        private bool IsShiftActive()
        {
            return hfShiftActive != null && hfShiftActive.Value == "1";
        }

        // ── START SHIFT ────────────────────────────────────────────────────────
        protected void btnStartShift_Click(object sender, EventArgs e)
        {
            int shift  = Convert.ToInt32(ddlShift.SelectedValue);
            int lineId = ddlLine != null && ddlLine.SelectedValue != "0"
                             ? Convert.ToInt32(ddlLine.SelectedValue) : 0;
            DateTime today = PPDatabaseHelper.TodayIST();

            try
            {
                int shiftId = PPDatabaseHelper.StartProductionShift(today, shift, lineId, UserID, FullName);
                ShowAlert("Shift " + shift + " started.", true);
                RefreshShiftBanner();
            }
            catch (Exception ex)
            {
                ShowAlert("Error starting shift: " + ex.Message, false);
            }
        }

        // ── END SHIFT ──────────────────────────────────────────────────────────
        protected void btnEndShift_Click(object sender, EventArgs e)
        {
            int shift  = Convert.ToInt32(ddlShift.SelectedValue);
            int lineId = ddlLine != null && ddlLine.SelectedValue != "0"
                             ? Convert.ToInt32(ddlLine.SelectedValue) : 0;
            DateTime today = PPDatabaseHelper.TodayIST();

            int shiftId = 0;
            int.TryParse(hfShiftID.Value, out shiftId);
            if (shiftId == 0)
            { ShowAlert("No active shift to end.", false); return; }

            DataRow totals = PPDatabaseHelper.GetProductionShiftTotals(today, shift, lineId);
            int target = 0, done = 0;
            if (totals != null)
            {
                target = totals["TargetBatches"]    != DBNull.Value ? Convert.ToInt32(totals["TargetBatches"])    : 0;
                done   = totals["CompletedBatches"] != DBNull.Value ? Convert.ToInt32(totals["CompletedBatches"]) : 0;
            }

            // Block if target not met
            if (done < target)
            {
                ShowAlert(
                    "<strong>Cannot end shift yet.</strong><br/>" +
                    "Only <strong>" + done + " of " + target + "</strong> batches have been completed. " +
                    "Please complete all allocated batches before ending the shift.",
                    false);
                return;
            }

            // Close the shift in DB
            try
            {
                PPDatabaseHelper.EndProductionShift(shiftId, target, done, UserID, FullName);
            }
            catch (Exception ex)
            {
                ShowAlert("Error ending shift: " + ex.Message, false);
                return;
            }

            // Build congratulatory message
            string shiftWord = "Shift " + shift;
            string msg;
            if (done > target)
            {
                msg = "&#127881; <strong>Exceeded the target!</strong> " +
                      shiftWord + " closed &mdash; you produced <strong>" + done + "</strong> batches " +
                      "against a target of <strong>" + target + "</strong>. Fantastic work!";
            }
            else
            {
                msg = "&#127881; <strong>Congratulations &mdash; target met!</strong> " +
                      shiftWord + " closed &mdash; all <strong>" + target + "</strong> batches completed.";
            }
            ShowAlert(msg, true);

            // Reset page UI
            Session.Remove("PE_OrderID");
            pnlInfo.Visible = false;
            pnlExecution.Style["display"] = "none";
            RefreshShiftBanner();
        }

        // ── RENDER ORDER ──────────────────────────────────────────────────────
        private void RenderOrder(int orderId)
        {
            DataRow order = PPDatabaseHelper.GetProductionOrderById(orderId);
            if (order == null) { ShowAlert("Order not found.", false); return; }

            int total     = Convert.ToInt32(Convert.ToDecimal(order["EffectiveBatches"]));
            string status = order["Status"].ToString();
            string outAbbr = order["OutputAbbr"].ToString();
            string prodType = order["ProductType"] != DBNull.Value ? order["ProductType"].ToString() : "Core";
            bool isConversion = (prodType == "Conversion");
            btnSaveOutput.Text = isConversion ? "Save & Store as Raw Material" : "Save & Move to Packing";
            Session["PE_ProductName"]  = order["ProductName"].ToString();
            Session["PE_IsConversion"] = isConversion;

            // Info panel
            pnlInfo.Visible = true;
            lblInfoProduct.Text = order["ProductName"].ToString();
            lblInfoCode.Text    = order["ProductCode"].ToString();
            if (lblInfoBatches != null) lblInfoBatches.Text = total.ToString();
            lblInfoOutput.Text  = Convert.ToDecimal(order["BatchSize"]).ToString("0.###") + " " + outAbbr + " per batch";
            lblInfoStatus.Text  = status;
            lblInfoStatus.CssClass = status == "Completed"  ? "status-completed" :
                                    status == "InProgress" ? "status-inprogress" : "status-initiated";
            lblInfoDate.Text    = PPDatabaseHelper.TodayIST().ToString("dd MMM yyyy");

            decimal uwg = order["UnitWeightGrams"] != DBNull.Value ? Convert.ToDecimal(order["UnitWeightGrams"]) : 0;
            string lineCode = order["LineCode"] != DBNull.Value ? order["LineCode"].ToString() : "";
            bool isLadduLine = lineCode.Equals("LADDU", StringComparison.OrdinalIgnoreCase);
            bool isBarfiLine = lineCode.Equals("BARFI", StringComparison.OrdinalIgnoreCase);
            if (pnlDoughWeight    != null) pnlDoughWeight.Visible = uwg > 0 && isLadduLine;
            if (hfUnitWeightGrams != null) hfUnitWeightGrams.Value = uwg.ToString("0.##");
            if (lblUnitWeight     != null) lblUnitWeight.Text = uwg.ToString("0.##");

            // BARFI line: tray-based unit calculation
            int unitsPerTray = 0;
            if (order.Table.Columns.Contains("UnitsPerTray") && order["UnitsPerTray"] != DBNull.Value)
                unitsPerTray = Convert.ToInt32(order["UnitsPerTray"]);
            if (pnlTrays        != null) pnlTrays.Visible       = isBarfiLine && unitsPerTray > 0;
            if (hfUnitsPerTray  != null) hfUnitsPerTray.Value   = unitsPerTray.ToString();
            if (lblUnitsPerTray != null) lblUnitsPerTray.Text   = unitsPerTray > 0 ? unitsPerTray.ToString() : "--";

            if (lblGrammage != null)
            {
                if (uwg > 0)
                {
                    lblGrammage.Text = uwg.ToString("0.##") + " G<span class='grammage-sub'>Per Unit</span>";
                    lblGrammage.Visible = true;
                }
                else
                {
                    lblGrammage.Visible = false;
                }
            }

            var history = PPDatabaseHelper.GetBatchHistory(orderId);
            int done = 0;
            foreach (DataRow r in history.Rows)
                if (r["Status"].ToString() == "Completed") done++;
            lblInfoCompleted.Text = done + " of " + total + " batches done";

            rptHistory.DataSource = history;
            rptHistory.DataBind();
            pnlHistoryEmpty.Visible = history.Rows.Count == 0;

            if (status == "Completed")
            {
                pnlExecution.Style["display"] = "none";
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

            DataRow active = PPDatabaseHelper.GetActiveBatch(orderId);
            DataRow ended  = PPDatabaseHelper.GetEndedBatch(orderId);
            int nextBatch = done + 1;

            if (active != null)
            {
                int bno = Convert.ToInt32(active["BatchNo"]);
                SetState("running", orderId, bno, total, Convert.ToInt32(active["ExecutionID"]));
            }
            else if (ended != null)
            {
                int bno = Convert.ToInt32(ended["BatchNo"]);
                SetState("ended", orderId, bno, total, Convert.ToInt32(ended["ExecutionID"]));
            }
            else
            {
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

            btnStart.Enabled = true;
            btnEnd.Enabled   = true;

            pnlOutput.Style["display"] = (state == "ended") ? "block" : "none";

            if (state == "ended")
            {
                try
                {
                    if (ddlRemarks != null)
                    {
                        ddlRemarks.Items.Clear();
                        ddlRemarks.Items.Add(new System.Web.UI.WebControls.ListItem("-- Select --", ""));
                        var orderForRemarks = PPDatabaseHelper.GetProductionOrderById(orderId);
                        int lineId = 0;
                        if (orderForRemarks != null && orderForRemarks["ProductionLineID"] != DBNull.Value)
                            lineId = Convert.ToInt32(orderForRemarks["ProductionLineID"]);
                        DataTable remarksDt = lineId > 0
                            ? PPDatabaseHelper.GetRemarkOptionsByLine(lineId)
                            : PPDatabaseHelper.GetRemarkOptions();
                        foreach (DataRow rr in remarksDt.Rows)
                            ddlRemarks.Items.Add(new System.Web.UI.WebControls.ListItem(
                                rr["OptionText"].ToString(), rr["OptionText"].ToString()));
                    }

                    if (pnlDynParams != null && orderId > 0)
                    {
                        var orderRow2 = PPDatabaseHelper.GetProductionOrderById(orderId);
                        if (orderRow2 != null)
                        {
                            var paramsDt = PPDatabaseHelper.GetProductParams(Convert.ToInt32(orderRow2["ProductID"]));
                            pnlDynParams.Visible = paramsDt.Rows.Count > 0;
                            rptDynParams.DataSource = paramsDt;
                            rptDynParams.DataBind();
                        }
                    }
                }
                catch { /* non-fatal */ }
            }

            string js = state == "running"
                ? "window.batchNum='" + batchNo + "';window.totalBat='" + total + "';startWheel();if(typeof applyState==='function'){applyState();}if(typeof setButtonStates==='function'){setButtonStates('running');}"
                : "window.batchNum='" + batchNo + "';window.totalBat='" + total + "';stopWheel(" + (state == "ready" ? "true" : "false") + ");if(typeof applyState==='function'){applyState();}if(typeof setButtonStates==='function'){setButtonStates('ready');}";
            ClientScript.RegisterStartupScript(GetType(), "pkwheel_" + state, js, true);
        }

        // ── START BATCH ───────────────────────────────────────────────────────
        protected void btnStart_Click(object sender, EventArgs e)
        {
            // ── SHIFT GATE ──
            if (!IsShiftActive())
            { ShowAlert("No active shift. Please <strong>START THE SHIFT</strong> first.", false); return; }

            int orderId = GetOrderId();
            if (orderId == 0) { ShowAlert("No product loaded.", false); return; }
            int batchNo = Convert.ToInt32(hfCurrentBatch.Value);
            if (batchNo == 0) batchNo = 1;

            if (PPDatabaseHelper.GetActiveBatch(orderId) != null)
            { ShowAlert("A batch is already in progress.", false); RenderOrder(orderId); return; }

            int execId = PPDatabaseHelper.StartBatch(orderId, batchNo, UserID);
            ShowAlert("Batch " + batchNo + " started.", true);
            RenderOrder(orderId);
        }

        // ── END BATCH ─────────────────────────────────────────────────────────
        protected void btnEnd_Click(object sender, EventArgs e)
        {
            // ── SHIFT GATE ──
            if (!IsShiftActive())
            { ShowAlert("No active shift. Please <strong>START THE SHIFT</strong> first.", false); return; }

            int orderId = GetOrderId();
            if (orderId == 0) { ShowAlert("No product loaded.", false); return; }

            DataRow active = PPDatabaseHelper.GetActiveBatch(orderId);
            if (active == null) { ShowAlert("No active batch found. Please press START first.", false); RenderOrder(orderId); return; }

            int execId = Convert.ToInt32(active["ExecutionID"]);
            PPDatabaseHelper.EndBatch(execId, orderId);

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
            // ── SHIFT GATE ──
            if (!IsShiftActive())
            { ShowAlert("No active shift. Please <strong>START THE SHIFT</strong> first.", false); return; }

            int orderId = GetOrderId();
            if (orderId == 0) { ShowAlert("No product loaded.", false); return; }

            DataRow ended = PPDatabaseHelper.GetEndedBatch(orderId);
            if (ended == null) { ShowAlert("No batch awaiting output. Please press END first.", false); return; }

            int execId  = Convert.ToInt32(ended["ExecutionID"]);
            int batchNo = Convert.ToInt32(ended["BatchNo"]);
            int total   = Convert.ToInt32(hfTotalBatches.Value);
            if (total == 0) total = Convert.ToInt32(
                Convert.ToDecimal(PPDatabaseHelper.GetProductionOrderById(orderId)["EffectiveBatches"]));

            decimal output = 1;
            var orderRowOut = PPDatabaseHelper.GetProductionOrderById(orderId);
            if (orderRowOut != null && orderRowOut["BatchSize"] != DBNull.Value)
                output = Convert.ToDecimal(orderRowOut["BatchSize"]);

            decimal calcUnits = 0;
            if (hfCalcUnits != null && decimal.TryParse(hfCalcUnits.Value, out calcUnits) && calcUnits > 0)
                output = calcUnits;

            // BARFI tray-based output: overrides ActualOutput when tray panel is in use
            decimal calcTrayUnits = 0;
            int trayCount = 0, partialUnits = 0;
            bool trayPanelUsed =
                hfCalcTrayUnits != null
                && decimal.TryParse(hfCalcTrayUnits.Value, out calcTrayUnits)
                && calcTrayUnits > 0;
            if (trayPanelUsed)
            {
                output = calcTrayUnits;
                int.TryParse(Request.Form[txtNoOfTrays.UniqueID],   out trayCount);
                int.TryParse(Request.Form[txtPartialUnits.UniqueID], out partialUnits);
            }

            int productId = Convert.ToInt32(PPDatabaseHelper.GetProductionOrderById(orderId)["ProductID"]);
            try
            {
                PPDatabaseHelper.DeductStockFIFO(execId, orderId, batchNo, productId, UserID);
            }
            catch (Exception stockEx)
            {
                if (stockEx.Message.StartsWith("STOCK_SHORTFALL:"))
                {
                    ShowAlert("<strong>Cannot save &mdash; insufficient stock for this batch:</strong><br/>" +
                              stockEx.Message.Substring(16).Replace("|", "<br/>"), false);
                    return;
                }
                throw;
            }

            PPDatabaseHelper.SaveBatchOutput(execId, output, ddlRemarks != null ? ddlRemarks.SelectedValue : "", orderId, total);

            // Persist BARFI tray panel inputs as reserved system params
            if (trayPanelUsed)
            {
                int trayParamId    = PPDatabaseHelper.GetOrCreateSystemParam(productId, "__SYS_NoOfTrays",     "number");
                int partialParamId = PPDatabaseHelper.GetOrCreateSystemParam(productId, "__SYS_PartialUnits",  "number");
                PPDatabaseHelper.SaveBatchParamValue(execId, trayParamId,    trayCount);
                PPDatabaseHelper.SaveBatchParamValue(execId, partialParamId, partialUnits);
            }

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
            if (txtDoughWeight != null) txtDoughWeight.Text = "";
            if (txtNoOfTrays   != null) txtNoOfTrays.Text   = "";
            if (txtPartialUnits!= null) txtPartialUnits.Text= "";
            RenderOrder(orderId);

            // Refresh shift counters so banner shows updated Completed count
            RefreshShiftBanner();
        }

        // ── HELPERS ──────────────────────────────────────────────────────────
        private int GetOrderId()
        {
            if (Session["PE_OrderID"] != null)
                return Convert.ToInt32(Session["PE_OrderID"]);
            string v = hfOrderID.Value;
            if (!string.IsNullOrEmpty(v) && v != "0") return Convert.ToInt32(v);
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
            string bg   = success ? "#d1f5e0"  : "#fdf3f2";
            string col  = success ? "#155724"  : "#842029";
            string bdr  = success ? "#a3d9b1"  : "#f5c2c7";
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
