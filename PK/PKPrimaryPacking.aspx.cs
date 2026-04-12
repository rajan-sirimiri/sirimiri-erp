using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKApp.DAL;

namespace PKApp
{
    public partial class PKPrimaryPacking : Page
    {
        protected Label    lblUser, lblAlert, lblDate;
        protected System.Web.UI.WebControls.DropDownList ddlShift;
        protected Label    lblProduct, lblContainerType, lblTotalBatches, lblPackedBatches, lblRemaining;
        protected Label    lblContainerName, lblCaseQtyLbl, lblJarOutName, lblOutputSummary;
        protected Panel    pnlAlert, pnlInfo, pnlRunConfig, pnlExecution, pnlOutput, pnlHistory;
        protected Panel    pnlOrderSelect, pnlMainContent;
        protected Repeater rptOrders;
        protected HiddenField hfProductId2;
        protected Panel    pnlHistEmpty, pnlHistTable;
        protected System.Web.UI.HtmlControls.HtmlGenericControl rowCaseQty;
        protected DropDownList ddlProduct, ddlUnitSize, ddlCaseQty;
        protected HiddenField  hfOrderId, hfProductId, hfPackingId, hfMachineId;
        protected HiddenField  hfState, hfBatchNo, hfTotalBat;
        protected HiddenField  hfContainerType, hfUnitSizes, hfContainersPerCase;
        protected HiddenField  hfSelectedUnitSize, hfSelectedCaseQty;
        protected HiddenField  hfHasLanguageLabels;
        protected HiddenField  hfLangSplit;
        protected Button   btnLoad, btnStart, btnEnd, btnSave;
        protected Repeater rptHistory;
        protected System.Web.UI.HtmlControls.HtmlInputGenericControl txtJars, txtUnits;
        protected System.Web.UI.HtmlControls.HtmlGenericControl rowLabelLanguage;
        protected DropDownList ddlLabelLanguage;
        protected Panel    pnlPMConsumption;
        protected Repeater rptPMConsumption;

        // Machine selection — inline edit/display panels
        protected Panel    pnlMachineSelect;
        protected Panel    pnlMachineEdit;
        protected Panel    pnlMachineDisplay;
        protected DropDownList ddlMachine;
        protected Button   btnSetMachine;
        protected Label    lblMachineName;
        protected LinkButton lnkChangeMachine;

        // Batch completion
        protected Panel    pnlBatchCompletion, pnlMachineSummary, pnlCompPM;
        protected Literal  litMachineSummary;
        protected Label    lblCompJarName;
        protected System.Web.UI.HtmlControls.HtmlInputGenericControl txtCompJars, txtCompLoose;
        protected Button   btnCompleteBatch;
        protected Repeater rptCompPM;

        protected int UserID => Convert.ToInt32(Session["PK_UserID"]);
        protected int MachineID => Session["PK_MachineID"] != null ? Convert.ToInt32(Session["PK_MachineID"]) : 0;

        protected void Page_Load(object s, EventArgs e)
        {
            if (Session["PK_UserID"] == null) { Response.Redirect("PKLogin.aspx?ReturnUrl=" + Server.UrlEncode(Request.Url.PathAndQuery)); return; }

            // Module access check
            string __role = Session["PK_Role"]?.ToString() ?? "";
            if (!PKDatabaseHelper.RoleHasModuleAccess(__role, "PK", "PK_PRIMARY"))
            { Response.Redirect("PKHome.aspx"); return; }
            lblUser.Text = Session["PK_FullName"] as string ?? "";
            lblDate.Text = PKDatabaseHelper.TodayIST().ToString("dddd, dd MMM yyyy").ToUpper() + " — PRIMARY PACKING";

            if (!IsPostBack)
            {
                // Check if machine is set in session
                if (MachineID == 0)
                {
                    ShowMachineSelector();
                    BindProductDropdown();
                    return;
                }
                SetMachineLabel();
                BindProductDropdown();
            }
            else
            {
                // On postback, restore machine from hidden field if session lost
                if (MachineID == 0 && hfMachineId.Value != "0" && !string.IsNullOrEmpty(hfMachineId.Value))
                    Session["PK_MachineID"] = Convert.ToInt32(hfMachineId.Value);

                if (hfOrderId.Value != "0")
                {
                    pnlMainContent.Visible = true;
                    pnlOrderSelect.Visible = false;
                    pnlInfo.Visible      = true;
                    pnlRunConfig.Visible = true;
                    pnlExecution.Visible = true;
                    pnlHistory.Visible   = true;
                    RestoreConfigDropdowns();
                }
            }
        }

        void ShowMachineSelector()
        {
            // Load machine dropdown
            var machines = PKDatabaseHelper.GetActiveMachines();
            ddlMachine.Items.Clear();
            ddlMachine.Items.Add(new ListItem("-- Select Machine --", "0"));
            foreach (DataRow r in machines.Rows)
                ddlMachine.Items.Add(new ListItem(
                    r["MachineName"] + " (" + r["MachineCode"] + ")" +
                    (r["Location"] != DBNull.Value ? " — " + r["Location"] : ""),
                    r["MachineID"].ToString()));
            // Show edit panel, hide display panel
            pnlMachineEdit.Visible   = true;
            pnlMachineDisplay.Visible = false;
        }

        protected void btnSetMachine_Click(object s, EventArgs e)
        {
            int mid = Convert.ToInt32(ddlMachine.SelectedValue);
            if (mid == 0) return;
            Session["PK_MachineID"] = mid;
            hfMachineId.Value = mid.ToString();
            SetMachineLabel();
            BindProductDropdown();
        }

        protected void lnkChangeMachine_Click(object s, EventArgs e)
        {
            // Switch back to edit mode (keep session — don't clear until a new one is SET)
            ShowMachineSelector();
            // Pre-select the current machine in dropdown
            if (MachineID > 0 && ddlMachine.Items.FindByValue(MachineID.ToString()) != null)
                ddlMachine.SelectedValue = MachineID.ToString();
        }

        void SetMachineLabel()
        {
            var machines = PKDatabaseHelper.GetActiveMachines();
            foreach (DataRow r in machines.Rows)
            {
                if (Convert.ToInt32(r["MachineID"]) == MachineID)
                {
                    lblMachineName.Text = r["MachineCode"] + " — " + r["MachineName"];
                    hfMachineId.Value = MachineID.ToString();
                    break;
                }
            }
            // Show display panel, hide edit panel
            pnlMachineEdit.Visible   = false;
            pnlMachineDisplay.Visible = true;
        }

        void BindProductDropdown()
        {
            var dt = PKDatabaseHelper.GetProductsInProduction();
            ddlProduct.Items.Clear();
            ddlProduct.Items.Add(new ListItem("-- Select Product --", "0"));
            foreach (DataRow r in dt.Rows)
                ddlProduct.Items.Add(new ListItem(
                    r["ProductName"] + " (" + r["ProductCode"] + ")",
                    r["ProductID"].ToString()
                    + "|" + (r["ContainerType"]     == DBNull.Value ? "DIRECT" : r["ContainerType"].ToString())
                    + "|" + (r["UnitsPerContainer"] == DBNull.Value ? ""       : r["UnitsPerContainer"].ToString())
                    + "|" + (r["ContainersPerCase"] == DBNull.Value ? "12"     : r["ContainersPerCase"].ToString())
                    + "|" + (r["HasLanguageLabels"] == DBNull.Value ? "0"      : r["HasLanguageLabels"].ToString())));
        }

        protected void btnLoad_Click(object s, EventArgs e)
        {
            string val = ddlProduct.SelectedValue;
            if (val == "0") { ShowAlert("Please select a product.", false); return; }

            string[] parts = val.Split('|');
            int productId  = int.Parse(parts[0]);

            hfProductId.Value         = productId.ToString();
            hfContainerType.Value     = parts.Length > 1 ? parts[1] : "DIRECT";
            hfUnitSizes.Value         = parts.Length > 2 ? parts[2] : "";
            hfContainersPerCase.Value = parts.Length > 3 ? parts[3] : "12";
            hfHasLanguageLabels.Value = parts.Length > 4 ? parts[4] : "0";

            pnlAlert.Visible = false;
            ClearPanels();

            var orders = PKDatabaseHelper.GetPendingPackingOrdersWithCompletion(productId);
            if (orders.Rows.Count == 0)
            { ShowAlert("No orders with produced batches ready to pack.", false); return; }

            if (orders.Rows.Count == 1)
                LoadOrder(Convert.ToInt32(orders.Rows[0]["OrderID"]));
            else
            {
                pnlOrderSelect.Visible = true;
                rptOrders.DataSource   = orders;
                rptOrders.DataBind();
            }
        }

        protected void rptOrders_ItemCommand(object src, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "SelectOrder")
            {
                pnlOrderSelect.Visible = false;
                LoadOrder(Convert.ToInt32(e.CommandArgument));
            }
        }

        private void ClearPanels()
        {
            if (pnlOrderSelect != null) pnlOrderSelect.Visible = false;
            pnlInfo.Visible      = false;
            pnlRunConfig.Visible = false;
            pnlExecution.Visible = false;
            pnlHistory.Visible   = false;
        }

        private void LoadOrder(int orderId)
        {
            var order = PKDatabaseHelper.GetPackingOrderById(orderId);
            if (order == null) { ShowAlert("Order not found.", false); return; }

            int productId      = Convert.ToInt32(order["ProductID"]);
            int productionDone = Convert.ToInt32(order["ProductionDone"]);
            int totalOrdered   = Convert.ToInt32(order["TotalBatches"]);
            int packed         = Convert.ToInt32(order["PackedBatches"]);
            int total          = productionDone;

            hfProductId.Value         = productId.ToString();
            hfOrderId.Value           = orderId.ToString();
            hfContainerType.Value     = order["ContainerType"]    == DBNull.Value ? "DIRECT" : order["ContainerType"].ToString();
            hfUnitSizes.Value         = order["UnitsPerContainer"] == DBNull.Value ? ""       : order["UnitsPerContainer"].ToString();
            hfContainersPerCase.Value = order["ContainersPerCase"] == DBNull.Value ? "12"     : order["ContainersPerCase"].ToString();

            // Language labels flag
            bool hasLangLabels = order.Table.Columns.Contains("HasLanguageLabels")
                && order["HasLanguageLabels"] != DBNull.Value
                && Convert.ToInt32(order["HasLanguageLabels"]) == 1;
            hfHasLanguageLabels.Value = hasLangLabels ? "1" : "0";
            if (rowLabelLanguage != null)
                rowLabelLanguage.Style["display"] = hasLangLabels ? "block" : "none";

            string containerType = hfContainerType.Value;
            string unitSizes     = hfUnitSizes.Value;
            string ctrsPerCase   = hfContainersPerCase.Value;

            lblProduct.Text       = order["ProductName"].ToString();
            lblContainerType.Text = containerType;
            string effectiveLabel = totalOrdered + " ordered";
            lblTotalBatches.Text  = productionDone + " produced / " + effectiveLabel;
            lblPackedBatches.Text = packed.ToString();
            lblRemaining.Text     = Math.Max(0, total - packed).ToString();

            string ctLabel = containerType == "DIRECT" ? "Case" : containerType;
            lblContainerName.Text = ctLabel;
            lblJarOutName.Text    = ctLabel + "s";
            lblCaseQtyLbl.Text    = ctLabel + "s";
            if (lblCompJarName != null) lblCompJarName.Text = ctLabel + "s";
            lblOutputSummary.Text = string.IsNullOrEmpty(unitSizes) ? "" :
                "Unit sizes available: " + unitSizes + " units per " + ctLabel;

            BindUnitSizes(unitSizes, ctLabel);
            BindCaseQty(ctrsPerCase, containerType);

            pnlInfo.Visible      = true;
            pnlRunConfig.Visible = true;
            pnlHistory.Visible   = true;

            // Check if all batches are done — show PM consumption panel
            bool allBatchesDone = PKDatabaseHelper.AreAllBatchesPacked(orderId);
            if (allBatchesDone || PKDatabaseHelper.HasPendingBatchCompletion(orderId))
            {
                pnlExecution.Visible = true;
                SetState("alldone", packed, total);
                BindHistory(orderId);
            }
            else
            {
                pnlExecution.Visible = true;
                RenderState(orderId, total, packed);
            }
        }

        void BindUnitSizes(string unitSizes, string ctLabel)
        {
            ddlUnitSize.Items.Clear();
            string firstVal = "";
            if (!string.IsNullOrEmpty(unitSizes))
                foreach (string sz in unitSizes.Split(','))
                {
                    string t = sz.Trim();
                    if (!string.IsNullOrEmpty(t))
                    {
                        ddlUnitSize.Items.Add(new ListItem(t + " units per " + ctLabel, t));
                        if (string.IsNullOrEmpty(firstVal)) firstVal = t;
                    }
                }
            // Auto-select first value — sets default without user interaction
            if (!string.IsNullOrEmpty(firstVal))
            {
                ddlUnitSize.SelectedValue   = firstVal;
                hfSelectedUnitSize.Value    = firstVal;
            }
        }

        void BindCaseQty(string ctrsPerCase, string containerType)
        {
            ddlCaseQty.Items.Clear();
            if (containerType == "DIRECT")
            {
                if (rowCaseQty != null) rowCaseQty.Style["display"] = "none";
                hfSelectedCaseQty.Value = "0";
                return;
            }
            if (rowCaseQty != null) rowCaseQty.Style["display"] = "";
            string firstVal = "";
            if (!string.IsNullOrEmpty(ctrsPerCase))
                foreach (string cq in ctrsPerCase.Split(','))
                {
                    string t = cq.Trim();
                    if (!string.IsNullOrEmpty(t))
                    {
                        ddlCaseQty.Items.Add(new ListItem(t + " per case", t));
                        if (string.IsNullOrEmpty(firstVal)) firstVal = t;
                    }
                }
            if (ddlCaseQty.Items.Count == 0)
            {
                ddlCaseQty.Items.Add(new ListItem("12 per case", "12"));
                firstVal = "12";
            }
            // Auto-select first value
            ddlCaseQty.SelectedValue = firstVal;
            hfSelectedCaseQty.Value  = firstVal;
        }

        void RestoreConfigDropdowns()
        {
            if (ddlUnitSize.Items.Count == 0)
            {
                string ct      = hfContainerType.Value;
                string ctLabel = ct == "DIRECT" ? "Case" : ct;
                BindUnitSizes(hfUnitSizes.Value, ctLabel);
                BindCaseQty(hfContainersPerCase.Value, ct);
                // Restore selections
                // Restore user-selected values if set, otherwise keep auto-selected defaults
                if (!string.IsNullOrEmpty(hfSelectedUnitSize.Value) && hfSelectedUnitSize.Value != "0")
                    try { ddlUnitSize.SelectedValue = hfSelectedUnitSize.Value; } catch { }
                else
                    hfSelectedUnitSize.Value = ddlUnitSize.Items.Count > 0 ? ddlUnitSize.Items[0].Value : "0";

                if (!string.IsNullOrEmpty(hfSelectedCaseQty.Value) && hfSelectedCaseQty.Value != "0")
                    try { ddlCaseQty.SelectedValue = hfSelectedCaseQty.Value; } catch { }
                else
                    hfSelectedCaseQty.Value = ddlCaseQty.Items.Count > 0 ? ddlCaseQty.Items[0].Value : "0";

                // Restore language row visibility
                if (rowLabelLanguage != null)
                    rowLabelLanguage.Style["display"] = hfHasLanguageLabels.Value == "1" ? "block" : "none";
            }
        }

        void RenderState(int orderId, int total, int packed)
        {
            var orderRow = PKDatabaseHelper.GetPackingOrderById(orderId);
            int effectiveBatches = total;
            string orderStatus   = "";
            if (orderRow != null)
            {
                effectiveBatches = Convert.ToInt32(orderRow["TotalBatches"]);
                orderStatus      = orderRow["Status"].ToString();
            }

            // Check for active packing on THIS machine
            var active = MachineID > 0
                ? PKDatabaseHelper.GetActivePackingForMachine(orderId, MachineID)
                : PKDatabaseHelper.GetActivePacking(orderId);

            if (active != null)
            {
                int packingId = Convert.ToInt32(active["PackingID"]);
                int batchNo   = Convert.ToInt32(active["BatchNo"]);
                hfPackingId.Value = packingId.ToString();
                SetState("running", batchNo, total);
            }
            else if (packed >= effectiveBatches || orderStatus == "Stopped")
            {
                // All batches show as packed — verify with AreAllBatchesPacked (counts Completed across ALL machines)
                bool allDone = PKDatabaseHelper.AreAllBatchesPacked(orderId) || orderStatus == "Stopped";

                if (allDone)
                {
                    // Every batch across ALL machines is Completed — show the output panel (jars + PM consumption)
                    SetState("alldone", packed, total);
                }
                else
                {
                    // This machine sees all done but other machine(s) still have pending batches
                    SetState("waiting", packed, total);
                    ShowAlert("This machine has finished all its batches. Waiting for other machine(s) to complete before PM consumption.", true);
                }
            }
            else
                SetState("ready", packed + 1, total);

            BindHistory(orderId);
        }

        void SetState(string state, int batchNo, int total)
        {
            btnStart.Enabled = true;
            btnEnd.Enabled   = true;

            hfState.Value    = state == "running" ? "running" : "ready";
            hfBatchNo.Value  = batchNo.ToString();
            hfTotalBat.Value = total.ToString();

            pnlOutput.Style["display"] = state == "alldone" ? "block" : "none";

            // In waiting state, disable start/end — this machine has no more batches
            if (state == "waiting")
            {
                btnStart.Enabled = false;
                btnEnd.Enabled   = false;
                hfState.Value    = "ready";
            }

            // Populate PM consumption grid when output panel shows
            if (state == "alldone")
                BindPMConsumptionGrid();
            else if (pnlPMConsumption != null)
                pnlPMConsumption.Visible = false;
        }

        void BindPMConsumptionGrid()
        {
            int orderId   = Convert.ToInt32(hfOrderId.Value);
            int productId = Convert.ToInt32(hfProductId.Value);
            if (orderId == 0 || productId == 0) return;

            // Get raw PM mappings for this product — exclude CASE level (secondary packing only)
            var pmData = PKDatabaseHelper.GetProductPMMappings(productId, "CASE");

            if (pmData.Rows.Count > 0)
            {
                // Add CalculatedQty column (JS will populate, but server needs it for binding)
                if (!pmData.Columns.Contains("CalculatedQty"))
                    pmData.Columns.Add("CalculatedQty", typeof(decimal), "0");

                pnlPMConsumption.Visible   = true;
                rptPMConsumption.DataSource = pmData;
                rptPMConsumption.DataBind();

                // Populate language split for JS: "Tamil:20,Kannada:30"
                var langSplit = PKDatabaseHelper.GetBatchLanguageSplit(orderId);
                var parts = new System.Collections.Generic.List<string>();
                foreach (DataRow lr in langSplit.Rows)
                {
                    string lang = lr["Language"] == DBNull.Value ? "" : lr["Language"].ToString();
                    if (!string.IsNullOrEmpty(lang))
                        parts.Add(lang + ":" + lr["BatchCount"].ToString());
                }
                hfLangSplit.Value = string.Join(",", parts);
            }
            else
            {
                pnlPMConsumption.Visible = false;
            }
        }

        protected void btnStart_Click(object s, EventArgs e)
        {
            int orderId   = Convert.ToInt32(hfOrderId.Value);
            int productId = Convert.ToInt32(hfProductId.Value);
            if (orderId == 0) { ShowAlert("No order loaded.", false); return; }
            if (MachineID == 0) { ShowAlert("No machine selected. Please select a machine first.", false); return; }

            var order  = PKDatabaseHelper.GetPackingOrderById(orderId);
            int packed = order != null ? Convert.ToInt32(order["PackedBatches"]) : 0;
            int productionDone2 = order != null ? Convert.ToInt32(order["ProductionDone"]) : 0;
            int total  = productionDone2;
            int next   = packed + 1;
            if (total == 0) { ShowAlert("No production batches completed yet.", false); return; }
            if (next > total) { ShowAlert("All produced batches have been packed.", false); return; }

            // Pass label language if product has language labels
            string labelLang = null;
            if (hfHasLanguageLabels.Value == "1" && ddlLabelLanguage != null)
                labelLang = ddlLabelLanguage.SelectedValue;

            int packingId = PKDatabaseHelper.StartPackingBatchWithMachine(orderId, next, UserID, MachineID, labelLang);
            hfPackingId.Value = packingId.ToString();
            SetState("running", next, total);
            UpdateInfoLabels(packed, total);
            BindHistory(orderId);
        }

        protected void btnEnd_Click(object s, EventArgs e)
        {
            int packingId = Convert.ToInt32(hfPackingId.Value);
            if (packingId == 0) { ShowAlert("No active batch to end.", false); return; }

            int orderId   = Convert.ToInt32(hfOrderId.Value);
            int productId = Convert.ToInt32(hfProductId.Value);

            PKDatabaseHelper.CompletePackingBatch(packingId);
            hfPackingId.Value = "0";

            var order  = PKDatabaseHelper.GetPackingOrderById(orderId);
            int productionDone3 = order != null ? Convert.ToInt32(order["ProductionDone"]) : 1;
            int total  = productionDone3;
            int packed = order != null ? Convert.ToInt32(order["PackedBatches"]) : 0;

            UpdateInfoLabels(packed, total);
            RenderState(orderId, total, packed);
        }

        protected void btnSave_Click(object s, EventArgs e)
        {
            int orderId   = Convert.ToInt32(hfOrderId.Value);
            int productId = Convert.ToInt32(hfProductId.Value);
            if (orderId == 0) { ShowAlert("No order loaded.", false); return; }

            int unitSize, caseQty;
            int.TryParse(hfSelectedUnitSize.Value, out unitSize);
            int.TryParse(hfSelectedCaseQty.Value,  out caseQty);
            if (unitSize == 0) { ShowAlert("Please select unit size before saving.", false); return; }

            int jars, units;
            int.TryParse(txtJars.Value,  out jars);
            int.TryParse(txtUnits.Value, out units);
            if (jars == 0 && units == 0)
            { ShowAlert("Please enter at least one quantity.", false); return; }

            try
            {
                string ct = hfContainerType.Value;

                // 2. Calculate PM consumption with actual output values
                var pmData = PKDatabaseHelper.CalculatePMConsumptionWithLanguage(
                    productId, orderId, jars, units, unitSize, caseQty, ct);

                // 3. Read overridden quantities from form inputs (pmQty_PMID)
                if (!pmData.Columns.Contains("ActualQty"))
                    pmData.Columns.Add("ActualQty", typeof(decimal));

                foreach (DataRow row in pmData.Rows)
                {
                    int pmId = Convert.ToInt32(row["PMID"]);
                    string formKey = "pmQty_" + pmId;
                    string formVal = Request.Form[formKey];
                    decimal actualQty;
                    if (!string.IsNullOrEmpty(formVal) && decimal.TryParse(formVal, out actualQty))
                        row["ActualQty"] = actualQty;
                    else
                        row["ActualQty"] = row["CalculatedQty"];
                }

                // 3a. VALIDATE PM STOCK — block if any PM has insufficient stock
                var shortages = new System.Collections.Generic.List<string>();
                foreach (DataRow row in pmData.Rows)
                {
                    decimal needed = Convert.ToDecimal(row["ActualQty"]);
                    if (needed <= 0) continue;
                    int pmId = Convert.ToInt32(row["PMID"]);
                    decimal available = PKDatabaseHelper.GetPMCurrentStock(pmId);
                    if (needed > available)
                    {
                        string pmName = row["PMName"].ToString();
                        shortages.Add(pmName + " (need " + needed.ToString("0.##") + ", have " + available.ToString("0.##") + ")");
                    }
                }
                if (shortages.Count > 0)
                {
                    ShowAlert("Cannot save — insufficient PM stock: " + string.Join("; ", shortages) + ". Please do PM GRN first.", false);
                    return;
                }

                // 1. Save packing output (jars + loose pcs, cases=0)
                PKDatabaseHelper.SaveOrderPackingOutput(orderId, productId,
                    0, jars, units, unitSize, caseQty, ct, UserID);

                int totalPcs = (jars * unitSize) + units;

                // 4. Record PM consumption
                PKDatabaseHelper.RecordPMConsumptionBatch(orderId, productId, pmData, UserID);

                int pmCount = 0;
                foreach (DataRow row in pmData.Rows)
                    if (Convert.ToDecimal(row["ActualQty"]) > 0) pmCount++;

                txtJars.Value = "0"; txtUnits.Value = "0";
                string msg = "Packing saved — " + totalPcs.ToString("N0") + " individual pieces added to SFG stock.";
                if (pmCount > 0)
                    msg += " " + pmCount + " packing material(s) consumption recorded.";
                ShowAlert(msg, true);

                var order  = PKDatabaseHelper.GetPackingOrderById(orderId);
                int productionDone4 = order != null ? Convert.ToInt32(order["ProductionDone"]) : 1;
                int total  = productionDone4;
                int packed = order != null ? Convert.ToInt32(order["PackedBatches"]) : 0;
                UpdateInfoLabels(packed, total);
                SetState("ready", 0, total);
                BindHistory(orderId);
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        void UpdateInfoLabels(int packed, int total)
        {
            lblPackedBatches.Text = packed.ToString();
            lblRemaining.Text     = Math.Max(0, total - packed).ToString();
        }

        void BindHistory(int orderId)
        {
            var dt = PKDatabaseHelper.GetPackingHistory(orderId);
            pnlHistEmpty.Visible = dt.Rows.Count == 0;
            pnlHistTable.Visible = dt.Rows.Count > 0;
            rptHistory.DataSource = dt; rptHistory.DataBind();
        }

        void ShowAlert(string m, bool ok)
        {
            lblAlert.Text     = m;
            pnlAlert.CssClass = "alert " + (ok ? "alert-success" : "alert-danger");
            pnlAlert.Visible  = true;
        }

        // ── BATCH COMPLETION ─────────────────────────────────────────────────
        void ShowBatchCompletionPanel(int orderId, int productId)
        {
            pnlBatchCompletion.Visible = true;
            pnlExecution.Visible = false;
            pnlOutput.Style["display"] = "none";

            // Show machine summary
            var machineSummary = PKDatabaseHelper.GetPackingSummaryByMachine(orderId);
            if (machineSummary.Rows.Count > 0)
            {
                var sb = new System.Text.StringBuilder();
                sb.Append("<table style='width:100%;font-size:12px;border-collapse:collapse;'>");
                sb.Append("<tr style='font-size:10px;font-weight:700;color:#999;text-transform:uppercase;letter-spacing:.06em;'>");
                sb.Append("<td style='padding:4px 8px;'>Machine</td><td style='padding:4px 8px;'>Batches</td>");
                sb.Append("<td style='padding:4px 8px;'>First Start</td><td style='padding:4px 8px;'>Last End</td></tr>");
                foreach (DataRow r in machineSummary.Rows)
                {
                    sb.AppendFormat("<tr><td style='padding:4px 8px;font-weight:600;'>{0} ({1})</td>",
                        r["MachineName"], r["MachineCode"]);
                    sb.AppendFormat("<td style='padding:4px 8px;'>{0}</td>", r["BatchesPacked"]);
                    sb.AppendFormat("<td style='padding:4px 8px;font-size:11px;color:#666;'>{0}</td>",
                        r["FirstStart"] != DBNull.Value ? Convert.ToDateTime(r["FirstStart"]).ToString("hh:mm tt") : "—");
                    sb.AppendFormat("<td style='padding:4px 8px;font-size:11px;color:#666;'>{0}</td></tr>",
                        r["LastEnd"] != DBNull.Value ? Convert.ToDateTime(r["LastEnd"]).ToString("hh:mm tt") : "—");
                }
                sb.Append("</table>");
                litMachineSummary.Text = sb.ToString();
                pnlMachineSummary.Visible = true;
            }

            // Bind PM consumption grid for completion — exclude CASE level (secondary packing only)
            var pmData = PKDatabaseHelper.GetProductPMMappings(productId, "CASE");
            if (pmData.Rows.Count > 0)
            {
                if (!pmData.Columns.Contains("CalculatedQty"))
                    pmData.Columns.Add("CalculatedQty", typeof(decimal), "0");
                pnlCompPM.Visible = true;
                rptCompPM.DataSource = pmData;
                rptCompPM.DataBind();
            }
            else
            {
                pnlCompPM.Visible = false;
            }

            BindHistory(orderId);
        }

        protected void btnCompleteBatch_Click(object s, EventArgs e)
        {
            int orderId   = Convert.ToInt32(hfOrderId.Value);
            int productId = Convert.ToInt32(hfProductId.Value);
            if (orderId == 0) { ShowAlert("No order loaded.", false); return; }

            int unitSize, caseQty;
            int.TryParse(hfSelectedUnitSize.Value, out unitSize);
            int.TryParse(hfSelectedCaseQty.Value,  out caseQty);
            if (unitSize == 0) { ShowAlert("Please select unit size before saving.", false); return; }

            int jars, loose;
            int.TryParse(txtCompJars.Value,  out jars);
            int.TryParse(txtCompLoose.Value, out loose);
            if (jars == 0 && loose == 0)
            { ShowAlert("Please enter JAR/BOX count before saving.", false); return; }

            try
            {
                string ct = hfContainerType.Value;
                int totalPcs;
                if (ct == "DIRECT")
                    totalPcs = (jars * unitSize) + loose;
                else
                    totalPcs = (jars * unitSize) + loose;

                // Validate PM stock
                var pmData = PKDatabaseHelper.CalculatePMConsumptionWithLanguage(
                    productId, orderId, jars, loose, unitSize, caseQty, ct);

                if (!pmData.Columns.Contains("ActualQty"))
                    pmData.Columns.Add("ActualQty", typeof(decimal));

                foreach (DataRow row in pmData.Rows)
                {
                    int pmId = Convert.ToInt32(row["PMID"]);
                    string formKey = "compPmQty_" + pmId;
                    string formVal = Request.Form[formKey];
                    decimal actualQty;
                    if (!string.IsNullOrEmpty(formVal) && decimal.TryParse(formVal, out actualQty))
                        row["ActualQty"] = actualQty;
                    else
                        row["ActualQty"] = row["CalculatedQty"];
                }

                // Check PM stock
                var shortages = new System.Collections.Generic.List<string>();
                foreach (DataRow row in pmData.Rows)
                {
                    decimal needed = Convert.ToDecimal(row["ActualQty"]);
                    if (needed <= 0) continue;
                    int pmId = Convert.ToInt32(row["PMID"]);
                    decimal available = PKDatabaseHelper.GetPMCurrentStock(pmId);
                    if (needed > available)
                        shortages.Add(row["PMName"] + " (need " + needed.ToString("0.##") + ", have " + available.ToString("0.##") + ")");
                }
                if (shortages.Count > 0)
                {
                    ShowAlert("Cannot save — insufficient PM stock: " + string.Join("; ", shortages), false);
                    return;
                }

                // Save packing output (summary row with BatchNo=0)
                PKDatabaseHelper.SaveOrderPackingOutput(orderId, productId,
                    0, jars, loose, unitSize, caseQty, ct, UserID);

                // Record PM consumption
                PKDatabaseHelper.RecordPMConsumptionBatch(orderId, productId, pmData, UserID);

                // Complete the batch completion record
                PKDatabaseHelper.CompleteBatchCompletion(orderId, jars, loose, unitSize, totalPcs, UserID);

                int pmCount = 0;
                foreach (DataRow row in pmData.Rows)
                    if (Convert.ToDecimal(row["ActualQty"]) > 0) pmCount++;

                string msg = "Batch completion saved — " + totalPcs.ToString("N0") + " individual pieces added to SFG stock.";
                if (pmCount > 0)
                    msg += " " + pmCount + " packing material(s) consumption recorded.";
                ShowAlert(msg, true);

                pnlBatchCompletion.Visible = false;
                pnlExecution.Visible = false;
                pnlOutput.Style["display"] = "none";

                // Refresh info
                var order = PKDatabaseHelper.GetPackingOrderById(orderId);
                int packed = order != null ? Convert.ToInt32(order["PackedBatches"]) : 0;
                int total  = order != null ? Convert.ToInt32(order["ProductionDone"]) : 0;
                UpdateInfoLabels(packed, total);
                BindHistory(orderId);
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }
    }
}
