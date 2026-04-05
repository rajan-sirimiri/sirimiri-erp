using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using PPApp.DAL;

namespace PPApp
{
    public partial class PPPreprocessEntry : Page
    {
        protected Label        lblNavUser, lblDate, lblAlert;
        protected Panel        pnlAlert, pnlStages, pnlS1Empty, pnlS1Table;
        protected Panel        pnlS2Empty, pnlS2Table, pnlS3Empty, pnlS3Table;
        protected Panel        pnlScrapInputs, pnlNoScrap;
        protected DropDownList ddlProduct;
        protected HiddenField  hfProductId, hfInputRMName, hfStage1Label, hfStage2Label, hfStage3Label, hfStage4Label, hfOutputUnit;
        protected HiddenField  hfBatchSize, hfIsPriceCalc;
        protected Label        lblStage1, lblStage2, lblStage3, lblStage4;
        protected Label        lblS1Unit, lblS2Unit, lblS3Unit, lblS4Unit;
        protected Label        lblS1Total, lblS2Total, lblS3Total, lblS4Total;
        protected Repeater     rptS1, rptS2, rptS3, rptS4, rptScrapItems;
        protected Button       btnS1, btnS2, btnS3, btnS4, btnCloseShift;
        protected Label        lblRawPeanutStock, lblSortedStock, lblRoastedPending, lblStage4Stock;
        protected Label        lblInputRMTitle, lblStage2Title, lblStage3Title, lblStage4Title;
        protected Panel        pnlStage4Card, pnlS4Summary, pnlS4Empty, pnlS4Table;
        protected System.Web.UI.HtmlControls.HtmlInputGenericControl txtS1, txtS2, txtS3, txtS4;

        protected int UserID => Session["PP_UserID"] != null ? Convert.ToInt32(Session["PP_UserID"]) : 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PP_UserID"] == null) { Response.Redirect("PPLogin.aspx"); return; }

            // Module access check
            string __role = Session["PP_Role"]?.ToString() ?? "";
            if (!PPDatabaseHelper.RoleHasModuleAccess(__role, "PP", "PP_PREPROCESS"))
            { Response.Redirect("PPHome.aspx"); return; }
            lblNavUser.Text = Session["PP_FullName"] as string ?? "";
            lblDate.Text    = PPDatabaseHelper.TodayIST().ToString("dddd, dd MMM yyyy").ToUpper();
            if (!IsPostBack)
            {
                BindProductList();
                pnlStages.Visible = false;
            }
            else
            {
                // Re-apply stage 4 visibility on postback
                int pid = 0;
                if (hfProductId != null && int.TryParse(hfProductId.Value, out pid) && pid > 0)
                {
                    var dt2 = PPDatabaseHelper.GetPreprocessProducts();
                    foreach (DataRow r in dt2.Rows)
                    {
                        if (Convert.ToInt32(r["ProductID"]) == pid)
                        {
                            string s4 = r.Table.Columns.Contains("Stage4Label") && r["Stage4Label"] != DBNull.Value
                                ? r["Stage4Label"].ToString() : "";
                            bool hasS4 = !string.IsNullOrEmpty(s4);
                            if (pnlStage4Card != null) pnlStage4Card.Visible = hasS4;
                            if (pnlS4Summary != null) pnlS4Summary.Visible = hasS4;

                            // Re-apply Stage 1 lock for IsPriceCalcProduct
                            bool isPCalc = r.Table.Columns.Contains("IsPriceCalcProduct") && r["IsPriceCalcProduct"] != DBNull.Value
                                && Convert.ToInt32(r["IsPriceCalcProduct"]) == 1;
                            decimal bs = r.Table.Columns.Contains("BatchSize") && r["BatchSize"] != DBNull.Value
                                ? Convert.ToDecimal(r["BatchSize"]) : 0;
                            if (isPCalc && bs > 0 && txtS1 != null)
                            {
                                txtS1.Value = bs.ToString("0.###");
                                txtS1.Disabled = true;
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void BindProductList()
        {
            var dt = PPDatabaseHelper.GetPreprocessProducts();
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
            if (productId == 0) { pnlStages.Visible = false; return; }

            var dt = PPDatabaseHelper.GetPreprocessProducts();
            DataRow row = null;
            foreach (DataRow r in dt.Rows)
                if (Convert.ToInt32(r["ProductID"]) == productId) { row = r; break; }
            if (row == null) return;

            hfProductId.Value   = productId.ToString();
            hfInputRMName.Value = row["InputRMName"].ToString();
            hfStage1Label.Value = row["Stage1Label"].ToString();
            hfStage2Label.Value = row["Stage2Label"].ToString();
            hfStage3Label.Value = row["Stage3Label"].ToString();
            string stage4 = row.Table.Columns.Contains("Stage4Label") && row["Stage4Label"] != DBNull.Value
                ? row["Stage4Label"].ToString() : "";
            if (hfStage4Label != null) hfStage4Label.Value = stage4;
            hfOutputUnit.Value  = row["OutputUnit"].ToString();

            // Store BatchSize and IsPriceCalcProduct
            decimal batchSize = row.Table.Columns.Contains("BatchSize") && row["BatchSize"] != DBNull.Value
                ? Convert.ToDecimal(row["BatchSize"]) : 0;
            bool isPriceCalc = row.Table.Columns.Contains("IsPriceCalcProduct") && row["IsPriceCalcProduct"] != DBNull.Value
                && Convert.ToInt32(row["IsPriceCalcProduct"]) == 1;
            if (hfBatchSize != null) hfBatchSize.Value = batchSize.ToString("0.###");
            if (hfIsPriceCalc != null) hfIsPriceCalc.Value = isPriceCalc ? "1" : "0";

            // Pre-populate Stage 1 with BatchSize if IsPriceCalcProduct
            if (isPriceCalc && batchSize > 0 && txtS1 != null)
            {
                txtS1.Value = batchSize.ToString("0.###");
                txtS1.Disabled = true;
            }
            else if (txtS1 != null)
            {
                txtS1.Value = "";
                txtS1.Disabled = false;
            }

            lblStage1.Text = row["Stage1Label"].ToString();
            lblStage2.Text = row["Stage2Label"].ToString();
            lblStage3.Text = row["Stage3Label"].ToString();
            if (lblStage4 != null) lblStage4.Text = stage4;

            bool hasStage4 = !string.IsNullOrEmpty(stage4);
            if (pnlStage4Card != null) pnlStage4Card.Visible = hasStage4;
            if (pnlS4Summary != null) pnlS4Summary.Visible = hasStage4;
            if (lblStage4Title != null && hasStage4) lblStage4Title.Text = stage4;

            string unit = row["OutputUnit"].ToString();
            lblS1Unit.Text = unit; lblS2Unit.Text = unit; lblS3Unit.Text = unit;
            if (lblS4Unit != null) lblS4Unit.Text = unit;

            // Load scrap for input RM
            LoadScrapItems(row["InputRMName"].ToString());

            pnlStages.Visible = true;
            RefreshAllStages(productId);
            RefreshStockSummary();
        }

        private void LoadScrapItems(string inputRMName)
        {
            // Collect scraps for ALL RMs involved in this process:
            // - Input RM (e.g. Raw Peanut)
            // - Stage 2 RM (e.g. Roasted Peanuts) — matches Stage 2 label name
            // - Stage 3/Output RM (e.g. Sorted Roasted Peanuts) — matches product name
            var rmNames = new System.Collections.Generic.List<string>();
            rmNames.Add(inputRMName);
            if (!string.IsNullOrEmpty(hfStage2Label.Value)) rmNames.Add(hfStage2Label.Value);
            if (!string.IsNullOrEmpty(hfStage3Label.Value)) rmNames.Add(hfStage3Label.Value);
            if (hfStage4Label != null && !string.IsNullOrEmpty(hfStage4Label.Value)) rmNames.Add(hfStage4Label.Value);

            // Get unique RMIDs for all matching names
            var allRMIds = new System.Collections.Generic.List<int>();
            foreach (string name in rmNames)
            {
                var rmRow = PPDatabaseHelper.ExecuteQueryPublic(
                    "SELECT RMID FROM MM_RawMaterials WHERE LOWER(TRIM(RMName))=LOWER(TRIM(?name)) AND IsActive=1 LIMIT 1;",
                    new MySql.Data.MySqlClient.MySqlParameter("?name", name));
                if (rmRow.Rows.Count > 0)
                    allRMIds.Add(Convert.ToInt32(rmRow.Rows[0]["RMID"]));
            }

            if (allRMIds.Count == 0) { pnlNoScrap.Visible = true; pnlScrapInputs.Visible = false; return; }

            // Merge scrap lists — avoid duplicates by ScrapID
            var merged = new DataTable();
            merged.Columns.Add("LinkID",    typeof(int));
            merged.Columns.Add("ScrapID",   typeof(int));
            merged.Columns.Add("ScrapName", typeof(string));
            merged.Columns.Add("Unit",      typeof(string));
            var seen = new System.Collections.Generic.HashSet<int>();
            foreach (int rmId in allRMIds)
            {
                var scraps = PPDatabaseHelper.GetScrapMaterialsForRM(rmId);
                foreach (DataRow r in scraps.Rows)
                {
                    int sid = Convert.ToInt32(r["ScrapID"]);
                    if (seen.Add(sid))
                        merged.Rows.Add(r["LinkID"], sid, r["ScrapName"], r["Unit"]);
                }
            }

            pnlNoScrap.Visible       = merged.Rows.Count == 0;
            pnlScrapInputs.Visible   = merged.Rows.Count > 0;
            rptScrapItems.DataSource = merged;
            rptScrapItems.DataBind();
        }

        private void RefreshAllStages(int productId)
        {
            // Stage 1
            var s1 = PPDatabaseHelper.GetPreprocessStage1LogToday(productId);
            BindStageLog(s1, rptS1, pnlS1Empty, pnlS1Table, lblS1Total, hfOutputUnit.Value);

            // Stages 2, 3 & 4
            var log = PPDatabaseHelper.GetPreprocessLogToday(productId);
            var s2 = new DataTable(); s2.Columns.Add("Qty", typeof(decimal)); s2.Columns.Add("CreatedAt", typeof(DateTime));
            var s3 = new DataTable(); s3.Columns.Add("Qty", typeof(decimal)); s3.Columns.Add("CreatedAt", typeof(DateTime));
            var s4 = new DataTable(); s4.Columns.Add("Qty", typeof(decimal)); s4.Columns.Add("CreatedAt", typeof(DateTime));
            foreach (DataRow r in log.Rows)
            {
                int stage = Convert.ToInt32(r["Stage"]);
                if (stage == 2) s2.Rows.Add(r["Qty"], r["CreatedAt"]);
                else if (stage == 3) s3.Rows.Add(r["Qty"], r["CreatedAt"]);
                else if (stage == 4) s4.Rows.Add(r["Qty"], r["CreatedAt"]);
            }
            BindStageLog(s2, rptS2, pnlS2Empty, pnlS2Table, lblS2Total, hfOutputUnit.Value);
            BindStageLog(s3, rptS3, pnlS3Empty, pnlS3Table, lblS3Total, hfOutputUnit.Value);
            if (rptS4 != null && pnlS4Empty != null && pnlS4Table != null && lblS4Total != null)
                BindStageLog(s4, rptS4, pnlS4Empty, pnlS4Table, lblS4Total, hfOutputUnit.Value);
        }

        private void BindStageLog(DataTable dt, Repeater rpt, Panel empty, Panel table, Label total, string unit)
        {
            empty.Visible = dt.Rows.Count == 0;
            table.Visible = dt.Rows.Count > 0;
            rpt.DataSource = dt; rpt.DataBind();
            decimal sum = 0;
            foreach (DataRow r in dt.Rows) sum += Convert.ToDecimal(r["Qty"]);
            total.Text = sum.ToString("0.###") + " " + unit;
        }

        protected void btnS1_Click(object sender, EventArgs e) => RecordStage(1, txtS1?.Value);
        protected void btnS2_Click(object sender, EventArgs e) => RecordStage(2, txtS2?.Value);
        protected void btnS3_Click(object sender, EventArgs e) => RecordStage(3, txtS3?.Value);
        protected void btnS4_Click(object sender, EventArgs e) => RecordStage(4, txtS4?.Value);

        private void RecordStage(int stage, string valStr)
        {
            int productId = Convert.ToInt32(hfProductId.Value);
            if (productId == 0) { ShowAlert("Please select a product.", false); return; }

            // For IsPriceCalcProduct: Stage 1 is fixed to BatchSize, Stages 2-4 cannot exceed BatchSize
            bool isPCalc = hfIsPriceCalc != null && hfIsPriceCalc.Value == "1";
            decimal batchSz = 0;
            if (hfBatchSize != null) decimal.TryParse(hfBatchSize.Value, out batchSz);

            decimal qty;
            if (isPCalc && stage == 1 && batchSz > 0)
            {
                // Stage 1 is always the batch size — use it directly
                qty = batchSz;
            }
            else
            {
                if (!decimal.TryParse(valStr, out qty) || qty <= 0)
                { ShowAlert("Please enter a valid quantity for Stage " + stage + ".", false); return; }
            }

            // Validate: Stages 2-4 cannot exceed BatchSize for IsPriceCalcProduct
            if (isPCalc && batchSz > 0 && stage > 1 && qty > batchSz)
            {
                ShowAlert("Quantity (" + qty.ToString("0.###") + ") cannot exceed batch size (" + batchSz.ToString("0.###") + ").", false);
                return;
            }

            string productName = ddlProduct.SelectedItem?.Text ?? "";
            int bracket = productName.IndexOf(" (");
            if (bracket > 0) productName = productName.Substring(0, bracket).Trim();

            string stageLabel = stage == 1 ? hfStage1Label.Value
                              : stage == 2 ? hfStage2Label.Value
                              : stage == 3 ? hfStage3Label.Value
                              : (hfStage4Label != null ? hfStage4Label.Value : hfStage3Label.Value);

            try
            {
                PPDatabaseHelper.AddPreprocessEntry(productId, stage, qty,
                    productName, hfInputRMName.Value, stageLabel, UserID);
                // Clear the input box that was just submitted
                if (stage == 1 && txtS1 != null) txtS1.Value = "";
                if (stage == 2 && txtS2 != null) txtS2.Value = "";
                if (stage == 3 && txtS3 != null) txtS3.Value = "";
                if (stage == 4 && txtS4 != null) txtS4.Value = "";
                ShowAlert("Stage " + stage + " — " + qty.ToString("0.###") + " " +
                    hfOutputUnit.Value + " recorded.", true);
                RefreshAllStages(productId);
                RefreshStockSummary();
                // Reload stage labels
                lblStage1.Text = hfStage1Label.Value;
                lblStage2.Text = hfStage2Label.Value;
                lblStage3.Text = hfStage3Label.Value;
                if (lblStage4 != null && hfStage4Label != null) lblStage4.Text = hfStage4Label.Value;
                string u = hfOutputUnit.Value;
                lblS1Unit.Text = u; lblS2Unit.Text = u; lblS3Unit.Text = u;
                if (lblS4Unit != null) lblS4Unit.Text = u;
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("STOCK_SHORTFALL:"))
                    ShowAlert("Insufficient stock — " + ex.Message.Substring(16), false);
                else
                    ShowAlert("Error: " + ex.Message, false);
            }
        }

        protected void btnCloseShift_Click(object sender, EventArgs e)
        {
            int productId = Convert.ToInt32(hfProductId.Value);
            if (productId == 0) { ShowAlert("Please select a product.", false); return; }

            string rmName = hfInputRMName.Value;
            int scrapCount = 0;

            foreach (string key in Request.Form.AllKeys)
            {
                if (key == null || !key.StartsWith("scrap_")) continue;
                int scrapId;
                if (!int.TryParse(key.Substring(6), out scrapId)) continue;
                decimal scrapQty;
                if (!decimal.TryParse(Request.Form[key], out scrapQty) || scrapQty <= 0) continue;

                var scrapRow = PPDatabaseHelper.ExecuteQueryPublic(
                    "SELECT ScrapName FROM MM_ScrapMaterials WHERE ScrapID=?id;",
                    new MySql.Data.MySqlClient.MySqlParameter("?id", scrapId));
                string scrapName = scrapRow.Rows.Count > 0 ? scrapRow.Rows[0]["ScrapName"].ToString() : "Scrap#" + scrapId;
                PPDatabaseHelper.RecordScrapGenerated(scrapId, scrapQty, scrapName, rmName, UserID);
                scrapCount++;
            }

            ShowAlert("Shift closed." + (scrapCount > 0 ? " " + scrapCount + " scrap entries recorded." : ""), true);
        }

        private void RefreshStockSummary()
        {
            if (lblRawPeanutStock == null) return;
            string inputRM   = hfInputRMName.Value;
            string stage2RM  = hfStage2Label.Value;
            string stage3RM  = hfStage3Label.Value;
            string stage4RM  = hfStage4Label != null ? hfStage4Label.Value : "";
            string unit      = hfOutputUnit.Value;

            decimal rawStock     = GetRMStock(inputRM);
            decimal sortedStock  = GetRMStock(stage3RM);
            decimal roastedStock = GetRMStock(stage2RM);
            decimal stage4Stock  = !string.IsNullOrEmpty(stage4RM) ? GetRMStock(stage4RM) : 0;

            if (lblInputRMTitle != null) lblInputRMTitle.Text = inputRM;
            if (lblStage2Title  != null) lblStage2Title.Text  = stage2RM;
            if (lblStage3Title  != null) lblStage3Title.Text  = stage3RM;
            if (lblStage4Title  != null && !string.IsNullOrEmpty(stage4RM)) lblStage4Title.Text = stage4RM;
            lblRawPeanutStock.Text   = rawStock.ToString("0.###")     + " " + unit;
            lblSortedStock.Text      = sortedStock.ToString("0.###")  + " " + unit;
            lblRoastedPending.Text   = roastedStock.ToString("0.###") + " " + unit;
            if (lblStage4Stock != null) lblStage4Stock.Text = stage4Stock.ToString("0.###") + " " + unit;
            if (pnlS4Summary != null) pnlS4Summary.Visible = !string.IsNullOrEmpty(stage4RM);
        }

        private decimal GetRMStock(string rmName)
        {
            if (string.IsNullOrEmpty(rmName)) return 0;
            var row = PPDatabaseHelper.ExecuteQueryPublic(
                "SELECT ROUND(IFNULL(os.Quantity,0) + IFNULL(grn.TotalGRN,0) - IFNULL(con.TotalConsumed,0), 4) AS Stock" +
                " FROM MM_RawMaterials r" +
                " LEFT JOIN MM_OpeningStock os ON os.MaterialType='RM' AND os.MaterialID=r.RMID" +
                " LEFT JOIN (SELECT RMID, SUM(QtyActualReceived) AS TotalGRN FROM MM_RawInward GROUP BY RMID) grn ON grn.RMID=r.RMID" +
                " LEFT JOIN (SELECT RMID, SUM(QtyConsumed) AS TotalConsumed FROM MM_StockConsumption GROUP BY RMID) con ON con.RMID=r.RMID" +
                " WHERE LOWER(TRIM(r.RMName))=LOWER(TRIM(?name)) AND r.IsActive=1;",
                new MySql.Data.MySqlClient.MySqlParameter("?name", rmName));
            if (row == null || row.Rows.Count == 0) return 0;
            return Convert.ToDecimal(row.Rows[0]["Stock"]);
        }

        private void ShowAlert(string msg, bool success)
        {
            lblAlert.Text     = msg;
            pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger");
            pnlAlert.Visible  = true;
        }
    }
}
