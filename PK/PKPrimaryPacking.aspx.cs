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
        protected Panel    pnlOrderSelect;
        protected Repeater rptOrders;
        protected HiddenField hfProductId2;
        protected Panel    pnlHistEmpty, pnlHistTable;
        protected System.Web.UI.HtmlControls.HtmlGenericControl rowCaseQty;
        protected DropDownList ddlProduct, ddlUnitSize, ddlCaseQty;
        protected HiddenField  hfOrderId, hfProductId, hfPackingId;
        protected HiddenField  hfState, hfBatchNo, hfTotalBat;
        protected HiddenField  hfContainerType, hfUnitSizes, hfContainersPerCase;
        protected HiddenField  hfSelectedUnitSize, hfSelectedCaseQty;
        protected HiddenField  hfHasLanguageLabels;
        protected Button   btnLoad, btnStart, btnEnd, btnSave;
        protected Repeater rptHistory;
        protected System.Web.UI.HtmlControls.HtmlInputGenericControl txtJars, txtUnits;
        protected System.Web.UI.HtmlControls.HtmlGenericControl rowLabelLanguage;
        protected DropDownList ddlLabelLanguage;
        protected int UserID => Convert.ToInt32(Session["PK_UserID"]);

        protected void Page_Load(object s, EventArgs e)
        {
            if (Session["PK_UserID"] == null) { Response.Redirect("PKLogin.aspx"); return; }
            lblUser.Text = Session["PK_FullName"] as string ?? "";
            lblDate.Text = PKDatabaseHelper.TodayIST().ToString("dddd, dd MMM yyyy").ToUpper() + " — PRIMARY PACKING";
            if (!IsPostBack)
            {
                BindProductDropdown();
            }
            else
            {
                if (hfOrderId.Value != "0")
                {
                    pnlOrderSelect.Visible = false;
                    if (pnlOrderSelect != null) pnlOrderSelect.Visible = false;
                    pnlInfo.Visible      = true;
                    pnlRunConfig.Visible = true;
                    pnlExecution.Visible = true;
                    pnlHistory.Visible   = true;
                    RestoreConfigDropdowns();
                }
            }
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

            var orders = PKDatabaseHelper.GetPendingPackingOrders(productId);
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
            // Show: X produced / Y effective (target to pack)
            string effectiveLabel = totalOrdered + " ordered";
            lblTotalBatches.Text  = productionDone + " produced / " + effectiveLabel;
            lblPackedBatches.Text = packed.ToString();
            lblRemaining.Text     = Math.Max(0, total - packed).ToString();

            string ctLabel = containerType == "DIRECT" ? "Case" : containerType;
            lblContainerName.Text = ctLabel;
            lblJarOutName.Text    = ctLabel + "s";
            lblCaseQtyLbl.Text    = ctLabel + "s";
            lblOutputSummary.Text = string.IsNullOrEmpty(unitSizes) ? "" :
                "Unit sizes available: " + unitSizes + " units per " + ctLabel;

            BindUnitSizes(unitSizes, ctLabel);
            BindCaseQty(ctrsPerCase, containerType);

            pnlInfo.Visible      = true;
            pnlRunConfig.Visible = true;
            pnlExecution.Visible = true;
            pnlHistory.Visible   = true;

            RenderState(orderId, total, packed);
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
            // total = productionDone (packing ceiling per session)
            // effectiveBatches = Revised or Ordered (threshold to unlock output panel)
            var orderRow = PKDatabaseHelper.GetPackingOrderById(orderId);
            int effectiveBatches = total; // fallback
            string orderStatus   = "";
            if (orderRow != null)
            {
                effectiveBatches = Convert.ToInt32(orderRow["TotalBatches"]); // RevisedBatches ?? OrderedBatches
                orderStatus      = orderRow["Status"].ToString();
            }

            var active = PKDatabaseHelper.GetActivePacking(orderId);

            if (active != null)
            {
                int packingId = Convert.ToInt32(active["PackingID"]);
                int batchNo   = Convert.ToInt32(active["BatchNo"]);
                hfPackingId.Value = packingId.ToString();
                SetState("running", batchNo, total);
            }
            else if (packed >= effectiveBatches || orderStatus == "Stopped")
            {
                // All effective batches packed, or order stopped — unlock output panel
                SetState("alldone", packed, total);
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
        }

        protected void btnStart_Click(object s, EventArgs e)
        {
            int orderId   = Convert.ToInt32(hfOrderId.Value);
            int productId = Convert.ToInt32(hfProductId.Value);
            if (orderId == 0) { ShowAlert("No order loaded.", false); return; }

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

            int packingId = PKDatabaseHelper.StartPackingBatch(orderId, next, UserID, labelLang);
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

                // Primary packing records containers (jars/boxes) + loose pcs only.
                // Cases = 0 here; case packing is done in Secondary Packing.
                PKDatabaseHelper.SaveOrderPackingOutput(orderId, productId,
                    0, jars, units, unitSize, caseQty, ct, UserID);

                int totalPcs = (jars * unitSize) + units;

                txtJars.Value = "0"; txtUnits.Value = "0";
                ShowAlert("Packing saved — " + totalPcs.ToString("N0") + " individual pieces added to FG stock.", true);

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
    }
}
