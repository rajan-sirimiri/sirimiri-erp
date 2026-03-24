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
        protected Label    lblProduct, lblTotalBatches, lblPackedBatches, lblRemaining;
        protected Label    lblContainerSizeHdr, lblContainerName;
        protected Panel    pnlAlert, pnlInfo, pnlExecution, pnlOutput, pnlHistory;
        protected Panel    pnlHistEmpty, pnlHistTable;
        protected DropDownList ddlProduct, ddlJarSize;
        protected HiddenField  hfOrderId, hfProductId, hfPackingId;
        protected HiddenField  hfState, hfBatchNo, hfTotalBat;
        protected HiddenField  hfContainersPerCase, hfUnitSizes, hfContainerType;
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
                // Restore panel visibility on postback
                if (hfOrderId.Value != "0")
                {
                    pnlInfo.Visible      = true;
                    pnlExecution.Visible = true;
                    pnlHistory.Visible   = true;
                    // Restore jar size dropdown if needed
                    if (ddlJarSize.Items.Count == 0 && !string.IsNullOrEmpty(hfUnitSizes.Value))
                    {
                        string ct = string.IsNullOrEmpty(hfContainerType.Value) ? "Case" : hfContainerType.Value;
                        string ctLabel = ct == "DIRECT" ? "Case" : ct;
                        BindJarSizes(hfUnitSizes.Value, ctLabel);
                        lblContainerName.Text    = ctLabel + "s";
                        lblContainerSizeHdr.Text = "Units per " + ctLabel;
                    }
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

            string[] parts = val.Split('|');
            int productId      = int.Parse(parts[0]);
            string containerType    = parts.Length > 1 ? parts[1] : "DIRECT";
            string unitSizes        = parts.Length > 2 ? parts[2] : "";
            string containersPerCase = parts.Length > 3 ? parts[3] : "12";

            hfProductId.Value         = productId.ToString();
            hfContainerType.Value     = containerType;
            hfUnitSizes.Value         = unitSizes;
            hfContainersPerCase.Value = containersPerCase;

            var order = PKDatabaseHelper.GetPackingOrderForProduct(productId);
            if (order == null) { ShowAlert("No active production order found for this product.", false); return; }

            int orderId     = Convert.ToInt32(order["OrderID"]);
            int total       = Convert.ToInt32(order["TotalBatches"]);
            int packed      = Convert.ToInt32(order["PackedBatches"]);
            hfOrderId.Value = orderId.ToString();

            // Product name
            string productName = ddlProduct.SelectedItem.Text;
            int bracket = productName.IndexOf(" (");
            if (bracket > 0) productName = productName.Substring(0, bracket);

            lblProduct.Text       = productName;
            lblTotalBatches.Text  = total.ToString();
            lblPackedBatches.Text = packed.ToString();
            lblRemaining.Text     = (total - packed).ToString();

            // Container labels
            string ctLabel = containerType == "DIRECT" ? "Case" : containerType;
            lblContainerName.Text    = ctLabel + "s";
            lblContainerSizeHdr.Text = "Units per " + ctLabel;

            // Unit size dropdown
            BindJarSizes(unitSizes, ctLabel);

            pnlInfo.Visible      = true;
            pnlExecution.Visible = true;
            pnlHistory.Visible   = true;
            pnlAlert.Visible     = false;

            RenderState(orderId, total, packed);
        }

        void BindJarSizes(string jarSizes, string containerType)
        {
            ddlJarSize.Items.Clear();
            if (!string.IsNullOrEmpty(jarSizes))
            {
                foreach (string sz in jarSizes.Split(','))
                {
                    string t = sz.Trim();
                    if (!string.IsNullOrEmpty(t))
                        ddlJarSize.Items.Add(new ListItem(t + " units per " + containerType, t));
                }
            }
            if (ddlJarSize.Items.Count == 0)
                ddlJarSize.Items.Add(new ListItem("1 unit per " + containerType, "1"));
        }

        void RenderState(int orderId, int total, int packed)
        {
            var active = PKDatabaseHelper.GetActivePacking(orderId);
            int nextBatch = packed + 1;

            if (active != null)
            {
                int packingId = Convert.ToInt32(active["PackingID"]);
                int batchNo   = Convert.ToInt32(active["BatchNo"]);
                hfPackingId.Value = packingId.ToString();
                string status = active["Status"].ToString();

                if (status == "InProgress")
                    SetState("running", batchNo, total);
                else // Ended — show ready for next (no output yet)
                    SetState("ready", batchNo, total);
            }
            else if (packed >= total)
            {
                // All batches done — show output panel to record qty
                SetState("alldone", total, total);
                ShowAlert("All " + total + " batches packed! Enter total qty below.", true);
            }
            else
                SetState("ready", nextBatch, total);

            BindHistory(orderId);
        }

        void SetState(string state, int batchNo, int total)
        {
            btnStart.Enabled = true;
            btnEnd.Enabled   = true;

            // Store state in hidden fields — read by JS on window.load
            // Avoids any RegisterStartupScript timing issues
            hfState.Value = (state == "running") ? "running" : "ready";
            hfBatchNo.Value    = batchNo.ToString();
            hfTotalBat.Value   = total.ToString();

            // pnlOutput controlled server-side
            pnlOutput.Style["display"] = (state == "alldone") ? "block" : "none";
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
            BindHistory(orderId);
            UpdateInfoLabels(packed, total);
        }

        protected void btnEnd_Click(object s, EventArgs e)
        {
            int packingId = Convert.ToInt32(hfPackingId.Value);
            if (packingId == 0) { ShowAlert("No active batch to end.", false); return; }

            int orderId   = Convert.ToInt32(hfOrderId.Value);
            int productId = Convert.ToInt32(hfProductId.Value);

            // Mark batch as Completed directly (no separate Ended state needed)
            PKDatabaseHelper.CompletePackingBatch(packingId);

            var order  = PKDatabaseHelper.GetPackingOrderForProduct(productId);
            int total  = order != null ? Convert.ToInt32(order["TotalBatches"])  : 1;
            int packed = order != null ? Convert.ToInt32(order["PackedBatches"]) : 0;

            UpdateInfoLabels(packed, total);
            RenderState(orderId, total, packed);
            BindHistory(orderId);
        }

        protected void btnSave_Click(object s, EventArgs e)
        {
            int orderId   = Convert.ToInt32(hfOrderId.Value);
            int productId = Convert.ToInt32(hfProductId.Value);
            if (orderId == 0) { ShowAlert("No order loaded.", false); return; }

            int jarSize;
            if (!int.TryParse(ddlJarSize.SelectedValue, out jarSize))
                jarSize = 1;

            int cases, jars, units;
            int.TryParse(txtCases.Value, out cases);
            int.TryParse(txtJars.Value,  out jars);
            int.TryParse(txtUnits.Value, out units);

            if (cases == 0 && jars == 0 && units == 0)
            { ShowAlert("Please enter at least one quantity.", false); return; }

            try
            {
                // Save total output for the entire order
                int containersPerCase = int.Parse(string.IsNullOrEmpty(hfContainersPerCase.Value) ? "12" : hfContainersPerCase.Value);
                string ct = hfContainerType.Value;
                int totalUnits;
                if (ct == "DIRECT")
                    totalUnits = cases * jarSize;
                else
                    totalUnits = (cases * containersPerCase * jarSize) + (jars * jarSize);

                // Add to FG Stock for the entire order
                PKDatabaseHelper.AddFGStock(productId, totalUnits, 0, orderId, 0, UserID);

                // Reset inputs
                txtCases.Value = "0"; txtJars.Value = "0"; txtUnits.Value = "0";

                ShowAlert("Packing complete — " + totalUnits.ToString("N0") + " units added to FG stock.", true);

                // Reset to ready state
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
            lblAlert.Text      = m;
            pnlAlert.CssClass  = "alert " + (ok ? "alert-success" : "alert-danger");
            pnlAlert.Visible   = true;
        }
    }
}
