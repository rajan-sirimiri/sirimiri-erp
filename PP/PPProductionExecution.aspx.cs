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

        protected Label          lblNavUser;
        protected Label          lblTodayDate;
        protected Label          lblAlert;
        protected Panel          pnlAlert;

        // Selection bar
        protected DropDownList   ddlShift;
        protected DropDownList   ddlProduct;
        protected Button         btnLoad;

        // Info panel
        protected Panel          pnlInfo;
        protected Label          lblInfoProduct;
        protected Label          lblInfoCode;
        protected Label          lblInfoBatches;
        protected Label          lblInfoOutput;
        protected Label          lblInfoStatus;
        protected Label          lblInfoDate;
        protected Label          lblInfoCompleted;

        // Execution panel
        protected Panel          pnlExecution;
        protected Panel          pnlNoOrder;
        protected HiddenField    hfOrderID;
        protected HiddenField    hfExecutionID;
        protected HiddenField    hfTotalBatches;
        protected HiddenField    hfCurrentBatch;

        // Gear animation area
        protected Label          lblBatchProgress;
        protected Panel          pnlStartBtn;
        protected Panel          pnlEndBtn;
        protected Panel          pnlGearStatus;

        // Output panel
        protected Panel          pnlOutput;
        protected TextBox        txtActualOutput;
        protected Label          lblOutputUnit;
        protected TextBox        txtRemarks;
        protected Button         btnSaveOutput;

        // Batch history
        protected Repeater       rptHistory;
        protected Panel          pnlHistoryEmpty;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PP_UserID"] == null) { Response.Redirect("PPLogin.aspx"); return; }
            lblNavUser.Text  = FullName;
            lblTodayDate.Text = PPDatabaseHelper.TodayIST().ToString("dddd, dd MMM yyyy").ToUpper();

            if (!IsPostBack)
            {
                pnlInfo.Visible      = false;
                pnlExecution.Visible = false;
                pnlNoOrder.Visible   = false;
            }
        }

        // ── LOAD ORDER ────────────────────────────────────────────────────────
        protected void btnLoad_Click(object sender, EventArgs e)
        {
            pnlAlert.Visible = false;
            int shift = Convert.ToInt32(ddlShift.SelectedValue);

            if (ddlProduct.SelectedValue == "0")
            { ShowAlert("Please select a product.", false); return; }

            int orderId = Convert.ToInt32(ddlProduct.SelectedValue);
            LoadOrder(orderId, shift);
        }

        protected void ddlShift_Changed(object sender, EventArgs e)
        {
            // Reload product dropdown for selected shift
            LoadProductDropdown(Convert.ToInt32(ddlShift.SelectedValue));
            pnlInfo.Visible      = false;
            pnlExecution.Visible = false;
        }

        private void LoadProductDropdown(int shift)
        {
            var dt = PPDatabaseHelper.GetInitiatedOrdersForShift(shift, PPDatabaseHelper.TodayIST());
            ddlProduct.Items.Clear();
            ddlProduct.Items.Add(new ListItem("-- Select Product --", "0"));
            foreach (DataRow row in dt.Rows)
            {
                string label = row["ProductName"].ToString() +
                    " (" + row["EffectiveBatches"].ToString() + " batches)";
                ddlProduct.Items.Add(new ListItem(label, row["OrderID"].ToString()));
            }
        }

        private void LoadOrder(int orderId, int shift)
        {
            // Get order details from product dropdown data
            var orders = PPDatabaseHelper.GetInitiatedOrdersForShift(shift, PPDatabaseHelper.TodayIST());
            DataRow order = null;
            foreach (DataRow r in orders.Rows)
                if (Convert.ToInt32(r["OrderID"]) == orderId) { order = r; break; }

            if (order == null)
            { ShowAlert("Order not found or already completed.", false); return; }

            int    totalBatches = Convert.ToInt32(Convert.ToDecimal(order["EffectiveBatches"]));
            string status       = order["Status"].ToString();

            // Info panel
            pnlInfo.Visible       = true;
            lblInfoProduct.Text   = order["ProductName"].ToString();
            lblInfoCode.Text      = order["ProductCode"].ToString();
            lblInfoBatches.Text   = totalBatches.ToString();
            decimal batchSize     = Convert.ToDecimal(order["BatchSize"]);
            string outAbbr        = order["OutputAbbr"].ToString();
            lblInfoOutput.Text    = batchSize.ToString("0.###") + " " + outAbbr + " per batch";
            lblInfoStatus.Text    = status;
            lblInfoStatus.CssClass = status == "Completed" ? "status-completed" :
                                     status == "InProgress" ? "status-inprogress" : "status-initiated";
            lblInfoDate.Text      = PPDatabaseHelper.TodayIST().ToString("dd MMM yyyy");

            // Store state
            hfOrderID.Value      = orderId.ToString();
            hfTotalBatches.Value = totalBatches.ToString();
            lblOutputUnit.Text   = outAbbr;

            // Count completed batches
            var history = PPDatabaseHelper.GetBatchHistory(orderId);
            int completedBatches = 0;
            foreach (DataRow r in history.Rows)
                if (r["Status"].ToString() == "Completed") completedBatches++;

            lblInfoCompleted.Text = completedBatches + " of " + totalBatches + " batches done";

            // Check for active batch
            DataRow activeBatch = PPDatabaseHelper.GetActiveBatch(orderId);

            if (status == "Completed")
            {
                pnlExecution.Visible = false;
                pnlNoOrder.Visible   = false;
                ShowAlert("All " + totalBatches + " batches completed for this product.", true);
            }
            else
            {
                pnlExecution.Visible = true;
                pnlNoOrder.Visible   = false;

                if (activeBatch != null)
                {
                    // Batch in progress — wheel should be spinning
                    int batchNo = Convert.ToInt32(activeBatch["BatchNo"]);
                    hfExecutionID.Value = activeBatch["ExecutionID"].ToString();
                    hfCurrentBatch.Value = batchNo.ToString();
                    lblBatchProgress.Text = "BATCH " + batchNo + " OF " + totalBatches;
                    pnlOutput.Visible = false;
                    // JS will start the wheel via startup script
                    ClientScript.RegisterStartupScript(GetType(), "startWheel",
                        "window.batchRunning=true; startWheel();", true);
                }
                else
                {
                    // No active batch — ready to start next
                    int nextBatch = completedBatches + 1;
                    hfCurrentBatch.Value = nextBatch.ToString();
                    hfExecutionID.Value  = "0";
                    lblBatchProgress.Text = "BATCH " + nextBatch + " OF " + totalBatches;
                    pnlOutput.Visible = false;
                    ClientScript.RegisterStartupScript(GetType(), "stopWheel",
                        "window.batchRunning=false; stopWheel();", true);
                }
            }

            // Bind history
            rptHistory.DataSource = history;
            rptHistory.DataBind();
            pnlHistoryEmpty.Visible = history.Rows.Count == 0;
        }

        // ── START BATCH ───────────────────────────────────────────────────────
        protected void btnStart_Click(object sender, EventArgs e)
        {
            int orderId    = Convert.ToInt32(hfOrderID.Value);
            int batchNo    = Convert.ToInt32(hfCurrentBatch.Value);
            int totalBatches = Convert.ToInt32(hfTotalBatches.Value);

            // Guard: no batch already in progress
            if (PPDatabaseHelper.GetActiveBatch(orderId) != null)
            { ShowAlert("A batch is already in progress.", false); return; }

            int execId = PPDatabaseHelper.StartBatch(orderId, batchNo, UserID);
            hfExecutionID.Value = execId.ToString();

            lblBatchProgress.Text = "BATCH " + batchNo + " OF " + totalBatches;
            pnlOutput.Visible     = false;

            // Tell JS to spin the wheel
            ClientScript.RegisterStartupScript(GetType(), "startWheel",
                "window.batchRunning=true; startWheel();", true);

            ReloadHistory(orderId);
        }

        // ── END BATCH ─────────────────────────────────────────────────────────
        protected void btnEnd_Click(object sender, EventArgs e)
        {
            int execId  = Convert.ToInt32(hfExecutionID.Value);
            int orderId = Convert.ToInt32(hfOrderID.Value);

            if (execId == 0) { ShowAlert("No batch is currently running.", false); return; }

            PPDatabaseHelper.EndBatch(execId, orderId);

            // Stop wheel, unlock output panel
            pnlOutput.Visible     = true;
            txtActualOutput.Text  = "";
            txtRemarks.Text       = "";

            ClientScript.RegisterStartupScript(GetType(), "stopWheel",
                "window.batchRunning=false; stopWheel();", true);

            ReloadHistory(orderId);
            // Reload order info to update status
            LoadOrder(orderId, Convert.ToInt32(ddlShift.SelectedValue));
            pnlOutput.Visible = true; // keep visible after reload
        }

        // ── SAVE OUTPUT ───────────────────────────────────────────────────────
        protected void btnSaveOutput_Click(object sender, EventArgs e)
        {
            int execId       = Convert.ToInt32(hfExecutionID.Value);
            int orderId      = Convert.ToInt32(hfOrderID.Value);
            int totalBatches = Convert.ToInt32(hfTotalBatches.Value);

            decimal actualOutput;
            if (!decimal.TryParse(txtActualOutput.Text.Trim(), out actualOutput) || actualOutput <= 0)
            { ShowAlert("Please enter a valid actual output quantity.", false); return; }

            PPDatabaseHelper.SaveBatchOutput(execId, actualOutput,
                txtRemarks.Text.Trim(), orderId, totalBatches);

            hfExecutionID.Value = "0";
            pnlOutput.Visible   = false;
            ShowAlert("Batch completed and saved successfully.", true);

            // Reload
            LoadOrder(orderId, Convert.ToInt32(ddlShift.SelectedValue));
        }

        // ── HELPERS ──────────────────────────────────────────────────────────
        private void ReloadHistory(int orderId)
        {
            var history = PPDatabaseHelper.GetBatchHistory(orderId);
            rptHistory.DataSource = history;
            rptHistory.DataBind();
            pnlHistoryEmpty.Visible = history.Rows.Count == 0;
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

        private void ShowAlert(string msg, bool success)
        {
            string icon   = success ? "&#10003;" : "&#9888;";
            string bg     = success ? "#d1f5e0" : "#fdf3f2";
            string color  = success ? "#155724" : "#842029";
            string border = success ? "#a3d9b1" : "#f5c2c7";

            lblAlert.Text = string.Format(
                "<div style='display:flex;align-items:flex-start;gap:10px;padding:12px 16px;" +
                "border-radius:10px;font-size:13px;line-height:1.6;" +
                "background:{0};color:{1};border:1px solid {2};'>" +
                "<span style='font-size:16px;flex-shrink:0;'>{3}</span>" +
                "<span>{4}</span></div>",
                bg, color, border, icon, msg);
            pnlAlert.Visible = true;
        }
    }
}
