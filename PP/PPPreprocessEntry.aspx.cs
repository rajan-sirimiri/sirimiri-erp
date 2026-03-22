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
        protected HiddenField  hfProductId, hfInputRMName, hfStage1Label, hfStage2Label, hfStage3Label, hfOutputUnit;
        protected Label        lblStage1, lblStage2, lblStage3;
        protected Label        lblS1Unit, lblS2Unit, lblS3Unit;
        protected Label        lblS1Total, lblS2Total, lblS3Total;
        protected Repeater     rptS1, rptS2, rptS3, rptScrapItems;
        protected Button       btnS1, btnS2, btnS3, btnCloseShift;
        protected System.Web.UI.HtmlControls.HtmlInputText txtS1, txtS2, txtS3;

        protected int UserID => Session["PP_UserID"] != null ? Convert.ToInt32(Session["PP_UserID"]) : 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PP_UserID"] == null) { Response.Redirect("PPLogin.aspx"); return; }
            lblNavUser.Text = Session["PP_FullName"] as string ?? "";
            lblDate.Text    = PPDatabaseHelper.TodayIST().ToString("dddd, dd MMM yyyy").ToUpper();
            if (!IsPostBack)
            {
                BindProductList();
                pnlStages.Visible = false;
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
            hfOutputUnit.Value  = row["OutputUnit"].ToString();

            lblStage1.Text = row["Stage1Label"].ToString();
            lblStage2.Text = row["Stage2Label"].ToString();
            lblStage3.Text = row["Stage3Label"].ToString();

            string unit = row["OutputUnit"].ToString();
            lblS1Unit.Text = unit; lblS2Unit.Text = unit; lblS3Unit.Text = unit;

            // Load scrap for input RM
            LoadScrapItems(row["InputRMName"].ToString());

            pnlStages.Visible = true;
            RefreshAllStages(productId);
        }

        private void LoadScrapItems(string inputRMName)
        {
            // Find RMID for input RM
            var rmRow = PPDatabaseHelper.ExecuteQueryPublic(
                "SELECT RMID FROM MM_RawMaterials WHERE LOWER(TRIM(RMName))=LOWER(TRIM(?name)) AND IsActive=1 LIMIT 1;",
                new MySql.Data.MySqlClient.MySqlParameter("?name", inputRMName));

            if (rmRow.Rows.Count == 0) { pnlNoScrap.Visible = true; pnlScrapInputs.Visible = false; return; }
            int rmId = Convert.ToInt32(rmRow.Rows[0]["RMID"]);

            var scraps = PPDatabaseHelper.GetScrapMaterialsForRM(rmId);
            pnlNoScrap.Visible     = scraps.Rows.Count == 0;
            pnlScrapInputs.Visible = scraps.Rows.Count > 0;
            rptScrapItems.DataSource = scraps;
            rptScrapItems.DataBind();
        }

        private void RefreshAllStages(int productId)
        {
            // Stage 1
            var s1 = PPDatabaseHelper.GetPreprocessStage1LogToday(productId);
            BindStageLog(s1, rptS1, pnlS1Empty, pnlS1Table, lblS1Total, hfOutputUnit.Value);

            // Stages 2 & 3
            var log = PPDatabaseHelper.GetPreprocessLogToday(productId);
            var s2 = new DataTable(); s2.Columns.Add("Qty", typeof(decimal)); s2.Columns.Add("CreatedAt", typeof(DateTime));
            var s3 = new DataTable(); s3.Columns.Add("Qty", typeof(decimal)); s3.Columns.Add("CreatedAt", typeof(DateTime));
            foreach (DataRow r in log.Rows)
            {
                int stage = Convert.ToInt32(r["Stage"]);
                if (stage == 2) s2.Rows.Add(r["Qty"], r["CreatedAt"]);
                else if (stage == 3) s3.Rows.Add(r["Qty"], r["CreatedAt"]);
            }
            BindStageLog(s2, rptS2, pnlS2Empty, pnlS2Table, lblS2Total, hfOutputUnit.Value);
            BindStageLog(s3, rptS3, pnlS3Empty, pnlS3Table, lblS3Total, hfOutputUnit.Value);
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

        private void RecordStage(int stage, string valStr)
        {
            int productId = Convert.ToInt32(hfProductId.Value);
            if (productId == 0) { ShowAlert("Please select a product.", false); return; }

            decimal qty;
            if (!decimal.TryParse(valStr, out qty) || qty <= 0)
            { ShowAlert("Please enter a valid quantity for Stage " + stage + ".", false); return; }

            string productName = ddlProduct.SelectedItem?.Text ?? "";
            int bracket = productName.IndexOf(" (");
            if (bracket > 0) productName = productName.Substring(0, bracket).Trim();

            string stageLabel = stage == 1 ? hfStage1Label.Value
                              : stage == 2 ? hfStage2Label.Value
                              : hfStage3Label.Value;

            try
            {
                PPDatabaseHelper.AddPreprocessEntry(productId, stage, qty,
                    productName, hfInputRMName.Value, stageLabel, UserID);
                ShowAlert("Stage " + stage + " — " + qty.ToString("0.###") + " " +
                    hfOutputUnit.Value + " recorded.", true);
                RefreshAllStages(productId);
                // Reload stage labels
                lblStage1.Text = hfStage1Label.Value;
                lblStage2.Text = hfStage2Label.Value;
                lblStage3.Text = hfStage3Label.Value;
                string u = hfOutputUnit.Value;
                lblS1Unit.Text = u; lblS2Unit.Text = u; lblS3Unit.Text = u;
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

        private void ShowAlert(string msg, bool success)
        {
            lblAlert.Text     = msg;
            pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger");
            pnlAlert.Visible  = true;
        }
    }
}
