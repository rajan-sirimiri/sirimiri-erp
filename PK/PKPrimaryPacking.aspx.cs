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
        protected Label    lblOutputBatch, lblContainerSizeHdr, lblContainerName;
        protected Panel    pnlAlert, pnlInfo, pnlExecution, pnlOutput, pnlHistory;
        protected Panel    pnlHistEmpty, pnlHistTable;
        protected DropDownList ddlProduct, ddlJarSize;
        protected HiddenField  hfOrderId, hfProductId, hfPackingId;
        protected HiddenField  hfJarsPerCase, hfJarSizes, hfContainerType, hfPackLevels;
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
                    if (ddlJarSize.Items.Count == 0 && !string.IsNullOrEmpty(hfJarSizes.Value))
                    {
                        string ct = string.IsNullOrEmpty(hfContainerType.Value) ? "Container" : hfContainerType.Value;
                        BindJarSizes(hfJarSizes.Value, ct);
                        lblContainerName.Text    = ct + "s";
                        lblContainerSizeHdr.Text = "Units per " + ct;
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
                    + "|" + (r["JarsPerCase"]    == DBNull.Value ? "12"                   : r["JarsPerCase"].ToString())
                    + "|" + (r["JarSizes"]       == DBNull.Value ? ""                     : r["JarSizes"].ToString())
                    + "|" + (r["ContainerType"]  == DBNull.Value ? ""                     : r["ContainerType"].ToString())
                    + "|" + (r["PackLevels"]     == DBNull.Value ? "Case+Container+Unit"  : r["PackLevels"].ToString())));
        }

        protected void btnLoad_Click(object s, EventArgs e)
        {
            string val = ddlProduct.SelectedValue;
            if (val == "0") { ShowAlert("Please select a product.", false); return; }

            string[] parts = val.Split('|');
            int productId      = int.Parse(parts[0]);
            string jarsPerCase = parts.Length > 1 ? parts[1] : "12";
            string jarSizes    = parts.Length > 2 ? parts[2] : "";
            string containerType = parts.Length > 3 ? parts[3] : "";
            string packLevels    = parts.Length > 4 ? parts[4] : "Case+Container+Unit";

            hfProductId.Value    = productId.ToString();
            hfJarsPerCase.Value  = jarsPerCase;
            hfJarSizes.Value     = jarSizes;
            hfContainerType.Value = containerType;
            hfPackLevels.Value   = packLevels;

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
            string ct = string.IsNullOrEmpty(containerType) ? "Container" : containerType;
            lblContainerName.Text     = ct + "s";
            lblContainerSizeHdr.Text  = "Units per " + ct;

            // Jar size dropdown
            BindJarSizes(jarSizes, ct);

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
                else // Ended — show output panel
                {
                    lblOutputBatch.Text = batchNo.ToString();
                    SetState("ended", batchNo, total);
                }
            }
            else if (packed >= total)
            {
                ShowAlert("All " + total + " batches packed! ✓", true);
                SetState("done", total, total);
            }
            else
                SetState("ready", nextBatch, total);

            BindHistory(orderId);
        }

        void SetState(string state, int batchNo, int total)
        {
            // pnlOutput visibility controlled by applyState() JS — not server side
            // Build startup script — runs after window.load, sets definitive state
            string js =
                "window.batchNum='" + batchNo + "';" +
                "window.totalBat='" + total + "';" +
                (state == "running" ? "startWheel();" : "stopWheel(" + (state == "ready" ? "true" : "false") + ");") +
                "applyState();" +
                "setButtonStates('" + (state == "running" ? "running" : "ready") + "');" +
                "updateBatchDisplay();";
            ClientScript.RegisterStartupScript(GetType(), "pkstate", js, true);
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
            PKDatabaseHelper.EndPackingBatch(packingId);

            var ended = PKDatabaseHelper.GetEndedPacking(orderId);
            var order = PKDatabaseHelper.GetPackingOrderForProduct(productId);
            int total = order != null ? Convert.ToInt32(order["TotalBatches"]) : 1;

            if (ended != null)
            {
                lblOutputBatch.Text = ended["BatchNo"].ToString();
                SetState("ended", Convert.ToInt32(ended["BatchNo"]), total);
            }
            BindHistory(orderId);
        }

        protected void btnSave_Click(object s, EventArgs e)
        {
            int packingId = Convert.ToInt32(hfPackingId.Value);
            int orderId   = Convert.ToInt32(hfOrderId.Value);
            int productId = Convert.ToInt32(hfProductId.Value);
            if (packingId == 0) { ShowAlert("No batch to save.", false); return; }

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
                PKDatabaseHelper.SavePackingOutput(packingId, orderId, productId,
                    cases, jars, units, jarSize, UserID);

                // Reset inputs
                txtCases.Value = "0"; txtJars.Value = "0"; txtUnits.Value = "0";
                hfPackingId.Value = "0";

                var order  = PKDatabaseHelper.GetPackingOrderForProduct(productId);
                int total  = order != null ? Convert.ToInt32(order["TotalBatches"])  : 1;
                int packed = order != null ? Convert.ToInt32(order["PackedBatches"]) : 0;

                int jarsPerCase = int.Parse(hfJarsPerCase.Value);
                string pl = hfPackLevels.Value;
                int totalUnits;
                if      (pl == "Case+Unit")       totalUnits = (cases * jarSize) + units;
                else if (pl == "Container+Unit")  totalUnits = (jars  * jarSize) + units;
                else                               totalUnits = (cases * jarsPerCase * jarSize) + (jars * jarSize) + units;

                ShowAlert("Batch saved — " + totalUnits.ToString("N0") + " units added to FG stock.", true);
                UpdateInfoLabels(packed, total);
                RenderState(orderId, total, packed);
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
