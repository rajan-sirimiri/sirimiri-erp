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
        protected Label    lblTotalOrders, lblTotalBatches, lblTotalJars, lblTotalUnits;
        protected DropDownList ddlProduct;
        protected TextBox  txtFromDate, txtToDate;
        protected Button   btnSearch;
        protected Panel    pnlResults, pnlEmpty, pnlTable;
        protected Repeater rptOrders;

        protected void Page_Load(object s, EventArgs e)
        {
            if (Session["PK_UserID"] == null) { Response.Redirect("PKLogin.aspx"); return; }
            lblUser.Text = Session["PK_FullName"] as string ?? "";

            if (!IsPostBack)
            {
                BindProductDropdown();
                DateTime now = PKDatabaseHelper.NowIST();
                txtFromDate.Text = now.AddDays(-30).ToString("yyyy-MM-dd");
                txtToDate.Text   = now.ToString("yyyy-MM-dd");
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
            rptOrders.DataSource = dt;
            rptOrders.DataBind();

            // Summaries
            int totalOrders = dt.Rows.Count;
            long totalBatches = 0, totalJars = 0, totalUnits = 0;
            foreach (DataRow row in dt.Rows)
            {
                totalBatches += Convert.ToInt64(row["BatchCount"]);
                totalJars    += Convert.ToInt64(row["OrderJars"]);
                totalUnits   += Convert.ToInt64(row["OrderTotalUnits"]);
            }

            lblTotalOrders.Text  = totalOrders.ToString("N0");
            lblTotalBatches.Text = totalBatches.ToString("N0");
            lblTotalJars.Text    = totalJars.ToString("N0");
            lblTotalUnits.Text   = totalUnits.ToString("N0");
        }

        protected void rptOrders_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            var orderRow = (DataRowView)e.Item.DataItem;
            int orderId  = Convert.ToInt32(orderRow["OrderID"]);

            // Find the inner repeater and bind batch data
            var rptBatches = (Repeater)e.Item.FindControl("rptBatches");
            if (rptBatches != null)
            {
                var batches = PKDatabaseHelper.GetPackingBatchesByOrder(orderId);
                rptBatches.DataSource = batches;
                rptBatches.DataBind();
            }
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
