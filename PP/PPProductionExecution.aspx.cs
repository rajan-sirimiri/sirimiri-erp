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
        protected HiddenField    hfShowOutput;

        // Gear animation area (labels driven by JS via ClientScript)
        protected Button         btnStart;
        protected Button         btnEnd;

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
            lblNavUser.Text   = FullName;
            lblTodayDate.Text = PPDatabaseHelper.TodayIST().ToString("dddd, dd MMM yyyy").ToUpper();

            int shift = Convert.ToInt32(ddlShift.SelectedValue);

            if (!IsPostBack)
            {
                pnlInfo.Visible      = false;
                pnlExecution.Visible = false;
                pnlNoOrder.Visible   = false;
                LoadProductDropdown(shift);
            }
            else
            {
                // Reload dropdown but preserve the selected value
                string selectedOrderId = Request.Form[ddlProduct.UniqueID];
                LoadProductDropdown(shift);
                if (!string.IsNullOrEmpty(selectedOrderId) && selectedOrderId != "0")
                {
                    var item = ddlProduct.Items.FindByValue(selectedOrderId);
                    if (item != null) item.Selected = true;
                }

                // Restore order panel if an order was previously loaded
                int orderId = Convert.ToInt32(hfOrderID.Value);
                if (orderId > 0)
                {
                    LoadOrder(orderId, shift);
                    // If END was just pressed, keep output panel visible
                    if (hfShowOutput.Value == "1")
                    {
                        pnlOutput.Visible    = true;
                        hfShowOutput.Value   = "0";
                        txtActualOutput.Text = "";
                        txtRemarks.Text      = "";
                    }
                }
            }
        }

        // ── LOAD ORDER ────────────────────────────────────────────────────────
        protected void btnLoad_Click(object sender, EventArgs e)
        {
            pnlAlert.Visible = false;
            int shift = Convert.ToInt32(ddlShift.SelectedValue);

            // Read directly from form post — dropdown may have been cleared by Page_Load
            string selectedVal = Request.Form[ddlProduct.UniqueID];
            if (string.IsNullOrEmpty(selectedVal) || selectedVal == "0")
            { ShowAlert("Please select a product.", false); return; }

            int orderId = Convert.ToInt32(selectedVal);
            hfOrderID.Value = orderId.ToString(); // persist for subsequent postbacks
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
            // Direct DB lookup — works regardless of order status or shift
            DataRow order = PPDatabaseHelper.GetProductionOrderById(orderId);

            if (order == null)
            { ShowAlert("Order not found.", false); return; }

            int    totalBatches = Convert.ToInt32(Convert.ToDecimal(order["EffectiveBatches"]));
            string status       = order["Status"].ToString();

            // Info panel
            pnlInfo.Visible       = true;
            lblInfoProduct.Text   = order["ProductName"].ToString();
            lblInfoCode.Text      = order["ProductCode"].ToString();
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
                    pnlOutput.Visible = false;
                    ClientScript.RegisterStartupScript(GetType(), "startWheel",
                        "window.batchRunning=true; window.batchNum='" + batchNo +
                        "'; window.totalBat='" + totalBatches + "'; startWheel();", true);
                }
                else
                {
                    // No active batch — ready to start next
                    int nextBatch = completedBatches + 1;
                    hfCurrentBatch.Value = nextBatch.ToString();
                    hfExecutionID.Value  = "0";
                    pnlOutput.Visible = false;
                    ClientScript.RegisterStartupScript(GetType(), "stopWheel",
                        "window.batchRunning=false; window.batchNum='" + nextBatch +
                        "'; window.totalBat='" + totalBatches + "'; stopWheel();", true);
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
            // Read from Request.Form — hidden fields may be reset by Page_Load before handler runs
            int orderId      = ReadIntFromForm(hfOrderID);
            int batchNo      = ReadIntFromForm(hfCurrentBatch);
            int totalBatches = ReadIntFromForm(hfTotalBatches);

            if (orderId == 0) { ShowAlert("No product loaded. Please select and Load a product first.", false); return; }
            if (batchNo  == 0) batchNo = 1;

            // Guard: no batch already in progress
            if (PPDatabaseHelper.GetActiveBatch(orderId) != null)
            { ShowAlert("A batch is already in progress.", false); return; }

            int execId = PPDatabaseHelper.StartBatch(orderId, batchNo, UserID);
            hfExecutionID.Value = execId.ToString();
            hfOrderID.Value     = orderId.ToString();

            pnlOutput.Visible = false;
            ClientScript.RegisterStartupScript(GetType(), "startWheel",
                "window.batchRunning=true; window.batchNum='" + batchNo +
                "'; window.totalBat='" + totalBatches + "'; startWheel();", true);
        }

        // ── END BATCH ─────────────────────────────────────────────────────────
        protected void btnEnd_Click(object sender, EventArgs e)
        {
            int orderId = ReadIntFromForm(hfOrderID);
            if (orderId == 0) { ShowAlert("Please load a product first.", false); return; }

            // Always look up active batch from DB — don't rely on hidden field
            DataRow activeBatch = PPDatabaseHelper.GetActiveBatch(orderId);
            if (activeBatch == null)
            { ShowAlert("No batch found for OrderID " + orderId + ". Please press START first.", false); return; }

            int execId = Convert.ToInt32(activeBatch["ExecutionID"]);
            hfExecutionID.Value = execId.ToString();

            PPDatabaseHelper.EndBatch(execId, orderId);

            // Stop wheel, unlock output panel
            ClientScript.RegisterStartupScript(GetType(), "stopWheel",
                "window.batchRunning=false; stopWheel();", true);

            // Signal Page_Load to show output panel
            hfShowOutput.Value   = "1";
            txtActualOutput.Text = "";
            txtRemarks.Text      = "";
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

        // Read integer from hidden field — falls back to raw form post value
        // Needed because Page_Load runs before button handlers and can reset fields
        private int ReadIntFromForm(HiddenField field)
        {
            // Try the field value first (set by Page_Load)
            int val = Convert.ToInt32(field.Value);
            if (val > 0) return val;
            // Fall back to raw posted form value
            string raw = Request.Form[field.UniqueID];
            return string.IsNullOrEmpty(raw) ? 0 : Convert.ToInt32(raw);
        }
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
