using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using PPApp.DAL;

namespace PPApp
{
    public partial class PPPrefilledEntry : Page
    {
        protected Label        lblNavUser;
        protected Label        lblDate;
        protected Panel        pnlAlert;
        protected Label        lblAlert;
        protected DropDownList ddlProduct;
        protected Panel        pnlEntry;
        protected TextBox      txtQty;
        protected Label        lblOutputUnit;
        protected Button       btnAdd;
        protected Panel        pnlTallyEmpty;
        protected Panel        pnlTallyTable;
        protected Repeater     rptTally;
        protected Label        lblTallyTotal;
        protected Label        lblTallyUnit;
        protected DropDownList ddlRawMaterial;
        protected TextBox      txtRMQty;
        protected Label        lblRMUnit;
        protected Button       btnClose;
        protected Panel        pnlClosureEmpty;
        protected Panel        pnlClosureTable;
        protected Repeater     rptClosure;
        protected Label        lblClosureTotal;
        protected Label        lblClosureUnit;
        protected HiddenField  hfProductId;
        protected HiddenField  hfShiftClosed;
        protected HiddenField  hfRMStockQty, hfRMStockUnit, hfRMDisplayName;
        protected Panel        pnlRMStock;
        protected Repeater     rptRMStock;
        protected Label        lblRMName, lblRMStockQty, lblRMStockUnit;
        protected Panel        pnlScrapEntry;
        protected Repeater     rptScrapInputs;
        protected Button       btnCheckScrap;
        protected Button       btnCloseShift;
        protected Panel        pnlShiftClosedMsg;
        protected Panel        pnlRightCard;
        protected HiddenField  hfOutputUnit;
        protected HiddenField  hfRMId;
        protected HiddenField  hfRMUnit;

        protected int UserID =>
            Session["PP_UserID"] != null ? Convert.ToInt32(Session["PP_UserID"]) : 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PP_UserID"] == null) { Response.Redirect("PPLogin.aspx"); return; }

            // Module access check
            string __role = Session["PP_Role"]?.ToString() ?? "";
            if (!PPDatabaseHelper.RoleHasModuleAccess(__role, "PP", "PP_PREFILLED"))
            { Response.Redirect("PPHome.aspx"); return; }
            lblNavUser.Text = Session["PP_FullName"] as string ?? "";
            lblDate.Text    = PPDatabaseHelper.TodayIST().ToString("dddd, dd MMM yyyy").ToUpper();

            if (!IsPostBack)
            {
                BindProductList();
                pnlEntry.Visible = false;
            }
            else
            {
                // Restore shift closed visual state on every postback
                bool closed = hfShiftClosed.Value == "1";
                SetShiftClosedState(closed);
                // NOTE: Do NOT reload scrap inputs here — that would clear user-entered values.
                // Scrap inputs are loaded only when RM is selected (ddlRM_Changed).
                // On postback the repeater rebinds only if LoadScrapInputs is explicitly called.
            }
        }

        private void BindProductList()
        {
            var dt = PPDatabaseHelper.GetPrefilledConversionProducts();
            ddlProduct.Items.Clear();
            ddlProduct.Items.Add(new ListItem("-- Select Product --", "0"));
            foreach (DataRow row in dt.Rows)
                ddlProduct.Items.Add(new ListItem(
                    row["ProductName"].ToString() + " (" + row["ProductCode"].ToString() + ")",
                    row["ProductID"].ToString()));
        }

        protected void ddlProduct_Changed(object sender, EventArgs e)
        {
            pnlAlert.Visible = false;
            int productId = Convert.ToInt32(ddlProduct.SelectedValue);
            if (productId == 0) { pnlEntry.Visible = false; return; }

            hfProductId.Value  = productId.ToString();
            hfShiftClosed.Value  = "0";
            if (pnlScrapEntry != null) pnlScrapEntry.Visible = false;
            SetShiftClosedState(false);

            // Get product output unit
            var products = PPDatabaseHelper.GetPrefilledConversionProducts();
            foreach (DataRow row in products.Rows)
            {
                if (Convert.ToInt32(row["ProductID"]) == productId)
                {
                    lblOutputUnit.Text  = row["OutputUnit"].ToString();
                    hfOutputUnit.Value  = row["OutputUnit"].ToString();
                    break;
                }
            }

            // Load RM dropdown — RMs used in this product's BOM
            LoadRMDropdown(productId);

            pnlEntry.Visible = true;
            RefreshTally(productId);
            RefreshRMStock(productId);  // Show all BOM RM stocks immediately
        }

        private void LoadRMDropdown(int productId)
        {
            // Get RMs from this product's BOM
            var bom = PPDatabaseHelper.ExecuteQueryPublic(
                "SELECT DISTINCT r.RMID, r.RMName, u.Abbreviation AS Unit" +
                " FROM PP_BOM b" +
                " JOIN MM_RawMaterials r ON r.RMID = b.MaterialID" +
                " JOIN MM_UOM u ON u.UOMID = r.UOMID" +
                " WHERE b.ProductID = ?pid AND b.MaterialType = 'RM'" +
                " ORDER BY r.RMName;",
                new MySql.Data.MySqlClient.MySqlParameter("?pid", productId));

            ddlRawMaterial.Items.Clear();
            ddlRawMaterial.Items.Add(new ListItem("-- Select Raw Material --", "0"));
            foreach (DataRow row in bom.Rows)
                ddlRawMaterial.Items.Add(new ListItem(
                    row["RMName"].ToString(), row["RMID"].ToString()));

            lblRMUnit.Text  = "—";
            hfRMId.Value    = "0";
            hfRMUnit.Value  = "";
        }

        protected void ddlRM_Changed(object sender, EventArgs e)
        {
            int rmId = Convert.ToInt32(ddlRawMaterial.SelectedValue);
            if (rmId == 0) { lblRMUnit.Text = "—"; hfRMId.Value = "0"; return; }
            hfRMId.Value = rmId.ToString();

            // Get RM unit
            var bom = PPDatabaseHelper.ExecuteQueryPublic(
                "SELECT u.Abbreviation AS Unit FROM MM_RawMaterials r" +
                " JOIN MM_UOM u ON u.UOMID = r.UOMID WHERE r.RMID = ?id;",
                new MySql.Data.MySqlClient.MySqlParameter("?id", rmId));
            if (bom.Rows.Count > 0)
            {
                string unit = bom.Rows[0]["Unit"].ToString();
                lblRMUnit.Text = unit;
                hfRMUnit.Value = unit;
            }

            // Auto-load scrap items for this RM
            LoadScrapInputs(rmId);
            RefreshClosures(rmId);
            RefreshRMStock();
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            int productId = Convert.ToInt32(hfProductId.Value);
            if (productId == 0) { ShowAlert("Please select a product.", false); return; }

            decimal qty;
            if (!decimal.TryParse(txtQty.Text.Trim(), out qty) || qty <= 0)
            { ShowAlert("Please enter a valid quantity.", false); return; }

            // Get the linked RM (matched by product name)
            string productName = ddlProduct.SelectedItem.Text;
            // Strip "(code)" suffix if present
            int bracket = productName.IndexOf(" (");
            if (bracket > 0) productName = productName.Substring(0, bracket).Trim();

            DataRow rm = PPDatabaseHelper.GetRMByName(productName);
            if (rm == null)
            {
                ShowAlert("No Raw Material named '" + productName +
                    "' found. Add it in MM first.", false);
                return;
            }

            try
            {
                PPDatabaseHelper.AddPrefilledEntry(
                    Convert.ToInt32(rm["RMID"]), qty, productName, productId, UserID);
                txtQty.Text = "";
                ShowAlert("Added " + qty.ToString("0.###") + " " + hfOutputUnit.Value +
                    " of " + productName + " to stock.", true);
                RefreshTally(productId);
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message, false);
            }
        }

        protected void btnCloseShift_Click(object sender, EventArgs e)
        {
            hfShiftClosed.Value = "1";
            SetShiftClosedState(true);
            ShowAlert("Shift closed. You can now record Raw Material consumed.", true);
        }

        private void SetShiftClosedState(bool closed)
        {
            // Close Shift button — hide once clicked
            btnCloseShift.Visible     = !closed;
            pnlShiftClosedMsg.Visible = closed;
            // Right card — enabled only after shift is closed
            if (pnlRightCard != null)
                pnlRightCard.CssClass = closed ? "" : "right-card-disabled";
            // Disable/enable Close Shift Consumption button
            btnClose.Enabled = closed;
        }

        private void LoadScrapInputs(int rmId)
        {
            var scraps = PPDatabaseHelper.GetScrapMaterialsForRM(rmId);
            pnlScrapEntry.Visible     = scraps.Rows.Count > 0;
            rptScrapInputs.DataSource = scraps;
            rptScrapInputs.DataBind();
        }

        protected void btnClose_Click(object sender, EventArgs e)
        {
            int rmId = Convert.ToInt32(hfRMId.Value);
            if (rmId == 0) { ShowAlert("Please select the raw material consumed.", false); return; }

            decimal qty;
            if (!decimal.TryParse(txtRMQty.Text.Trim(), out qty) || qty <= 0)
            { ShowAlert("Please enter qty consumed.", false); return; }

            string rmName = ddlRawMaterial.SelectedItem?.Text ?? "";
            string productName = ddlProduct.SelectedItem?.Text ?? "";

            // Check available stock before recording consumption
            decimal available = PPDatabaseHelper.GetAvailableStock(rmId);
            if (qty > available)
            {
                ShowAlert("Insufficient stock — " + rmName + " available: " +
                    available.ToString("0.###") + " " + hfRMUnit.Value +
                    ", requested: " + qty.ToString("0.###") + " " + hfRMUnit.Value + ".", false);
                return;
            }
            try
            {
                PPDatabaseHelper.RecordShiftConsumption(rmId, qty, rmName, productName, UserID);

                // Process scrap quantities — read directly from Request.Form
                // Scrap inputs are named "scrap_<ScrapID>" in the repeater
                foreach (string key in Request.Form.AllKeys)
                {
                    if (key == null || !key.StartsWith("scrap_")) continue;
                    string scrapIdStr = key.Substring(6); // strip "scrap_"
                    int scrapId;
                    if (!int.TryParse(scrapIdStr, out scrapId)) continue;
                    decimal scrapQty;
                    if (!decimal.TryParse(Request.Form[key], out scrapQty) || scrapQty <= 0) continue;
                    // Get scrap name
                    var scrapRow = PPDatabaseHelper.ExecuteQueryPublic(
                        "SELECT ScrapName FROM MM_ScrapMaterials WHERE ScrapID=?id;",
                        new MySql.Data.MySqlClient.MySqlParameter("?id", scrapId));
                    string scrapName = scrapRow.Rows.Count > 0 ? scrapRow.Rows[0]["ScrapName"].ToString() : "Scrap#" + scrapId;
                    PPDatabaseHelper.RecordScrapGenerated(scrapId, scrapQty, scrapName, rmName, UserID);
                }

                txtRMQty.Text         = "";
                pnlScrapEntry.Visible = false;
                ShowAlert("Recorded " + qty.ToString("0.###") + " " + hfRMUnit.Value +
                    " consumption of " + rmName + ". Scrap quantities logged.", true);
                // Refresh closures table — keep right panel visible
                RefreshClosures(rmId);
                // Reload scrap inputs for next entry (cleared now)
                LoadScrapInputs(rmId);
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message, false);
            }
        }

        private void RefreshTally(int productId)
        {
            // Get entries via InvoiceNo = 'PREFILLED' and remarks containing productId
            var dt = PPDatabaseHelper.GetPrefilledEntriesToday(productId);
            if (dt.Rows.Count == 0)
            {
                pnlTallyEmpty.Visible = true;
                pnlTallyTable.Visible = false;
            }
            else
            {
                pnlTallyEmpty.Visible = false;
                pnlTallyTable.Visible = true;
                rptTally.DataSource   = dt;
                rptTally.DataBind();
                decimal total = 0;
                foreach (DataRow row in dt.Rows)
                    total += Convert.ToDecimal(row["Qty"]);
                lblTallyTotal.Text = total.ToString("0.###");
                lblTallyUnit.Text  = dt.Rows[0]["Unit"].ToString();
            }
        }

        private void RefreshClosures(int rmId)
        {
            var dt = PPDatabaseHelper.GetShiftConsumptionToday(rmId);
            if (dt.Rows.Count == 0)
            {
                pnlClosureEmpty.Visible = true;
                pnlClosureTable.Visible = false;
            }
            else
            {
                pnlClosureEmpty.Visible = false;
                pnlClosureTable.Visible = true;
                rptClosure.DataSource   = dt;
                rptClosure.DataBind();
                decimal total = 0;
                foreach (DataRow row in dt.Rows)
                    total += Convert.ToDecimal(row["QtyConsumed"]);
                lblClosureTotal.Text = total.ToString("0.###");
                lblClosureUnit.Text  = hfRMUnit.Value;
            }
        }

        private void RefreshRMStock(int productId = 0)
        {
            if (pnlRMStock == null) return;
            if (productId == 0) productId = Convert.ToInt32(hfProductId.Value);
            if (productId == 0) { pnlRMStock.Visible = false; return; }

            // Get ALL RMs from this product's BOM with their stock
            var bom = PPDatabaseHelper.ExecuteQueryPublic(
                "SELECT r.RMID, r.RMName, u.Abbreviation AS Unit," +
                " ROUND(IFNULL(os.Quantity,0) + IFNULL(grn.TotalGRN,0) - IFNULL(con.TotalConsumed,0), 3) AS Stock" +
                " FROM PP_BOM b" +
                " JOIN MM_RawMaterials r ON r.RMID = b.MaterialID" +
                " JOIN MM_UOM u ON u.UOMID = r.UOMID" +
                " LEFT JOIN MM_OpeningStock os ON os.MaterialType='RM' AND os.MaterialID=r.RMID" +
                " LEFT JOIN (SELECT RMID, SUM(QtyActualReceived) AS TotalGRN FROM MM_RawInward GROUP BY RMID) grn ON grn.RMID=r.RMID" +
                " LEFT JOIN (SELECT RMID, SUM(QtyConsumed) AS TotalConsumed FROM MM_StockConsumption GROUP BY RMID) con ON con.RMID=r.RMID" +
                " WHERE b.ProductID=?pid AND b.MaterialType='RM'" +
                " GROUP BY r.RMID, r.RMName, u.Abbreviation, os.Quantity, grn.TotalGRN, con.TotalConsumed" +
                " ORDER BY r.RMName;",
                new MySql.Data.MySqlClient.MySqlParameter("?pid", productId));

            if (bom.Rows.Count == 0) { pnlRMStock.Visible = false; return; }

            rptRMStock.DataSource = bom;
            rptRMStock.DataBind();
            pnlRMStock.Visible = true;
        }

        private void ShowAlert(string msg, bool success)
        {
            lblAlert.Text     = msg;
            pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger");
            pnlAlert.Visible  = true;
        }
    }
}
