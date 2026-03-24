using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKApp.DAL;

namespace PKApp
{
    public partial class PKPrimaryPacking : Page
    {
        protected Label    lblUser, lblAlert;
        protected Label    lblProduct, lblContainerType, lblTotalBatches, lblPackedBatches, lblRemaining;
        protected Label    lblContainerName, lblCaseQtyLbl, lblJarOutName, lblOutputSummary;
        protected Panel    pnlAlert, pnlInfo, pnlRunConfig, pnlExecution, pnlOutput, pnlHistory;
        protected Panel    pnlHistEmpty, pnlHistTable;
        protected System.Web.UI.HtmlControls.HtmlGenericControl rowCaseQty;
        protected DropDownList ddlProduct, ddlUnitSize, ddlCaseQty;
        protected HiddenField  hfOrderId, hfProductId, hfPackingId;
        protected HiddenField  hfState, hfBatchNo, hfTotalBat;
        protected HiddenField  hfContainerType, hfUnitSizes, hfContainersPerCase;
        protected HiddenField  hfSelectedUnitSize, hfSelectedCaseQty;
        protected Button   btnLoad, btnStart, btnEnd, btnSave;
        protected Repeater rptHistory;
        protected System.Web.UI.HtmlControls.HtmlInputGenericControl txtCases, txtJars, txtUnits;
        protected int UserID => Convert.ToInt32(Session["PK_UserID"]);

        protected void Page_Load(object s, EventArgs e)
        {
            if (Session["PK_UserID"] == null) { Response.Redirect("PKLogin.aspx"); return; }
            lblUser.Text = Session["PK_FullName"] as string ?? "";
            if (!IsPostBack)
            {
                BindProductDropdown();
            }
            else
            {
                if (hfOrderId.Value != "0")
                {
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
                    + "|" + (r["ContainersPerCase"] == DBNull.Value ? "12"     : r["ContainersPerCase"].ToString())));
        }

        protected void btnLoad_Click(object s, EventArgs e)
        {
            string val = ddlProduct.SelectedValue;
            if (val == "0") { ShowAlert("Please select a product.", false); return; }

            string[] parts       = val.Split('|');
            int    productId     = int.Parse(parts[0]);
            string containerType = parts.Length > 1 ? parts[1] : "DIRECT";
            string unitSizes     = parts.Length > 2 ? parts[2] : "";
            string ctrsPerCase   = parts.Length > 3 ? parts[3] : "12";

            hfProductId.Value        = productId.ToString();
            hfContainerType.Value    = containerType;
            hfUnitSizes.Value        = unitSizes;
            hfContainersPerCase.Value = ctrsPerCase;

            var order = PKDatabaseHelper.GetPackingOrderForProduct(productId);
            if (order == null) { ShowAlert("No active production order found.", false); return; }

            int orderId = Convert.ToInt32(order["OrderID"]);
            int total   = Convert.ToInt32(order["TotalBatches"]);
            int packed  = Convert.ToInt32(order["PackedBatches"]);
            hfOrderId.Value = orderId.ToString();

            // Product name
            string productName = ddlProduct.SelectedItem.Text;
            int bracket = productName.IndexOf(" (");
            if (bracket > 0) productName = productName.Substring(0, bracket);

            lblProduct.Text       = productName;
            lblContainerType.Text = containerType;
            lblTotalBatches.Text  = total.ToString();
            lblPackedBatches.Text = packed.ToString();
            lblRemaining.Text     = (total - packed).ToString();

            // Container label
            string ctLabel = containerType == "DIRECT" ? "Case" : containerType;
            lblContainerName.Text = ctLabel;
            lblJarOutName.Text    = ctLabel + "s";
            lblCaseQtyLbl.Text    = ctLabel + "s";
            lblOutputSummary.Text = string.IsNullOrEmpty(unitSizes) ? "" :
                "Unit sizes available: " + unitSizes + " units per " + ctLabel;

            // Config dropdowns
            BindUnitSizes(unitSizes, ctLabel);
            BindCaseQty(ctrsPerCase, containerType);

            pnlInfo.Visible      = true;
            pnlRunConfig.Visible = true;
            pnlExecution.Visible = true;
            pnlHistory.Visible   = true;
            pnlAlert.Visible     = false;

            RenderState(orderId, total, packed);
        }

        void BindUnitSizes(string unitSizes, string ctLabel)
        {
            ddlUnitSize.Items.Clear();
            ddlUnitSize.Items.Add(new ListItem("-- Select --", "0"));
            if (!string.IsNullOrEmpty(unitSizes))
                foreach (string sz in unitSizes.Split(','))
                {
                    string t = sz.Trim();
                    if (!string.IsNullOrEmpty(t))
                        ddlUnitSize.Items.Add(new ListItem(t + " units per " + ctLabel, t));
                }
        }

        void BindCaseQty(string ctrsPerCase, string containerType)
        {
            ddlCaseQty.Items.Clear();
            if (containerType == "DIRECT")
            {
                // For DIRECT, case qty = unit size — hide this dropdown
                if (rowCaseQty != null) rowCaseQty.Style["display"] = "none";
                return;
            }
            if (rowCaseQty != null) rowCaseQty.Style["display"] = "";
            // Parse possible values — e.g. "6,12" or just "12"
            if (!string.IsNullOrEmpty(ctrsPerCase))
                foreach (string cq in ctrsPerCase.Split(','))
                {
                    string t = cq.Trim();
                    if (!string.IsNullOrEmpty(t))
                        ddlCaseQty.Items.Add(new ListItem(t + " per case", t));
                }
            if (ddlCaseQty.Items.Count == 0)
                ddlCaseQty.Items.Add(new ListItem("12 per case", "12"));
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
                try { ddlUnitSize.SelectedValue = hfSelectedUnitSize.Value; } catch { }
                try { ddlCaseQty.SelectedValue  = hfSelectedCaseQty.Value;  } catch { }
            }
        }

        void RenderState(int orderId, int total, int packed)
        {
            var active = PKDatabaseHelper.GetActivePacking(orderId);

            if (active != null)
            {
                int packingId = Convert.ToInt32(active["PackingID"]);
                int batchNo   = Convert.ToInt32(active["BatchNo"]);
                hfPackingId.Value = packingId.ToString();
                SetState("running", batchNo, total);
            }
            else if (packed >= total)
            {
                SetState("alldone", total, total);
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

            var order  = PKDatabaseHelper.GetPackingOrderForProduct(productId);
            int packed = order != null ? Convert.ToInt32(order["PackedBatches"]) : 0;
            int total  = order != null ? Convert.ToInt32(order["TotalBatches"])  : 1;
            int next   = packed + 1;
            if (next > total) { ShowAlert("All batches already packed.", false); return; }

            int packingId = PKDatabaseHelper.StartPackingBatch(orderId, next, UserID);
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

            var order  = PKDatabaseHelper.GetPackingOrderForProduct(productId);
            int total  = order != null ? Convert.ToInt32(order["TotalBatches"])  : 1;
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

            int cases, jars, units;
            int.TryParse(txtCases.Value, out cases);
            int.TryParse(txtJars.Value,  out jars);
            int.TryParse(txtUnits.Value, out units);
            if (cases == 0 && jars == 0 && units == 0)
            { ShowAlert("Please enter at least one quantity.", false); return; }

            try
            {
                string ct = hfContainerType.Value;
                int totalPcs;
                if (ct == "DIRECT")
                    totalPcs = (cases * unitSize) + units;
                else
                    totalPcs = (cases * caseQty * unitSize) + (jars * unitSize) + units;

                PKDatabaseHelper.AddFGStock(productId, totalPcs, 0, orderId, 0, UserID);

                txtCases.Value = "0"; txtJars.Value = "0"; txtUnits.Value = "0";
                ShowAlert("Packing saved — " + totalPcs.ToString("N0") + " individual pieces added to FG stock.", true);

                var order  = PKDatabaseHelper.GetPackingOrderForProduct(productId);
                int total  = order != null ? Convert.ToInt32(order["TotalBatches"])  : 1;
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
