using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKApp.DAL;

namespace PKApp
{
    public partial class PKPackingHistory : Page
    {
        protected Label    lblUser, lblDateRange, lblPrintRange;
        protected Label    lblTotalBatches, lblTotalJars, lblTotalUnits;
        protected DropDownList ddlProduct;
        protected TextBox  txtFromDate, txtToDate;
        protected Button   btnSearch;
        protected Panel    pnlResults, pnlEmpty, pnlTable;
        protected Repeater rptHistory;

        protected void Page_Load(object s, EventArgs e)
        {
            if (Session["PK_UserID"] == null) { Response.Redirect("PKLogin.aspx"); return; }
            lblUser.Text = Session["PK_FullName"] as string ?? "";

            if (!IsPostBack)
            {
                BindProductDropdown();

                // Default date range: last 30 days
                DateTime now = PKDatabaseHelper.NowIST();
                txtFromDate.Text = now.AddDays(-30).ToString("yyyy-MM-dd");
                txtToDate.Text   = now.ToString("yyyy-MM-dd");

                // Auto-load on first visit
                LoadHistory();
            }
        }

        void BindProductDropdown()
        {
            var dt = PKDatabaseHelper.GetProductsWithPackingHistory();
            ddlProduct.Items.Clear();
            ddlProduct.Items.Add(new ListItem("-- All Products --", "0"));
            foreach (DataRow r in dt.Rows)
                ddlProduct.Items.Add(new ListItem(
                    r["ProductName"] + " (" + r["ProductCode"] + ")",
                    r["ProductID"].ToString()));
        }

        protected void btnSearch_Click(object s, EventArgs e)
        {
            LoadHistory();
        }

        void LoadHistory()
        {
            int productId = Convert.ToInt32(ddlProduct.SelectedValue);

            DateTime fromDate, toDate;
            if (!DateTime.TryParse(txtFromDate.Text, out fromDate))
                fromDate = PKDatabaseHelper.NowIST().AddDays(-30);
            if (!DateTime.TryParse(txtToDate.Text, out toDate))
                toDate = PKDatabaseHelper.NowIST();

            string rangeText = fromDate.ToString("dd MMM yyyy") + " — " + toDate.ToString("dd MMM yyyy");
            if (productId > 0)
                rangeText += " | " + ddlProduct.SelectedItem.Text;
            lblDateRange.Text  = rangeText;
            lblPrintRange.Text = rangeText;

            var dt = PKDatabaseHelper.GetPackingHistoryReport(productId, fromDate, toDate);

            pnlResults.Visible = true;

            if (dt.Rows.Count == 0)
            {
                pnlEmpty.Visible = true;
                pnlTable.Visible = false;
                return;
            }

            pnlEmpty.Visible = false;
            pnlTable.Visible = true;
            rptHistory.DataSource = dt;
            rptHistory.DataBind();

            // Summaries — deduplicate order totals (same OrderJars/Pcs on every batch row of an order)
            int totalBatches = dt.Rows.Count;
            long totalJars = 0, totalUnits = 0;
            var seenOrders = new System.Collections.Generic.HashSet<int>();
            foreach (DataRow row in dt.Rows)
            {
                int orderId = Convert.ToInt32(row["OrderID"]);
                if (!seenOrders.Contains(orderId))
                {
                    seenOrders.Add(orderId);
                    if (row["OrderJars"] != DBNull.Value)       totalJars  += Convert.ToInt64(row["OrderJars"]);
                    if (row["OrderTotalUnits"] != DBNull.Value) totalUnits += Convert.ToInt64(row["OrderTotalUnits"]);
                }
            }

            lblTotalBatches.Text = totalBatches.ToString("N0");
            lblTotalJars.Text    = totalJars.ToString("N0");
            lblTotalUnits.Text   = totalUnits.ToString("N0");
        }

        protected string FormatDuration(object startVal, object endVal)
        {
            if (startVal == null || startVal == DBNull.Value ||
                endVal == null || endVal == DBNull.Value)
                return "—";
            try
            {
                var start = Convert.ToDateTime(startVal);
                var end   = Convert.ToDateTime(endVal);
                var span  = end - start;
                if (span.TotalMinutes < 1) return "< 1m";
                if (span.TotalHours < 1) return ((int)span.TotalMinutes) + "m";
                return ((int)span.TotalHours) + "h " + span.Minutes + "m";
            }
            catch { return "—"; }
        }
    }
}
