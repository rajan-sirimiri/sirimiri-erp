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
            if (Session["PK_UserID"] == null) { Response.Redirect("PKLogin.aspx?ReturnUrl=" + Server.UrlEncode(Request.Url.PathAndQuery)); return; }

            // Module access check
            string __role = Session["PK_Role"]?.ToString() ?? "";
            if (!PKDatabaseHelper.RoleHasModuleAccess(__role, "PK", "PK_HISTORY"))
            { Response.Redirect("PKHome.aspx"); return; }
            if (lblUser != null) lblUser.Text = Session["PK_FullName"] as string ?? "";

            if (!IsPostBack)
            {
                BindProductDropdown();
                DateTime now = PKDatabaseHelper.NowIST();
                if (txtFromDate != null) txtFromDate.Text = now.AddDays(-30).ToString("yyyy-MM-dd");
                if (txtToDate != null)   txtToDate.Text   = now.ToString("yyyy-MM-dd");
                LoadHistory();
            }
        }

        void BindProductDropdown()
        {
            if (ddlProduct == null) return;
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
            try
            {
                int productId = 0;
                if (ddlProduct != null && ddlProduct.SelectedValue != null)
                    int.TryParse(ddlProduct.SelectedValue, out productId);

                DateTime now = PKDatabaseHelper.NowIST();
                DateTime fromDate = now.AddDays(-30);
                DateTime toDate   = now;
                if (txtFromDate != null && !string.IsNullOrEmpty(txtFromDate.Text))
                    DateTime.TryParse(txtFromDate.Text, out fromDate);
                if (txtToDate != null && !string.IsNullOrEmpty(txtToDate.Text))
                    DateTime.TryParse(txtToDate.Text, out toDate);

                string rangeText = fromDate.ToString("dd MMM yyyy") + " — " + toDate.ToString("dd MMM yyyy");
                if (productId > 0 && ddlProduct != null && ddlProduct.SelectedItem != null)
                    rangeText += " | " + ddlProduct.SelectedItem.Text;
                if (lblDateRange != null)  lblDateRange.Text  = rangeText;
                if (lblPrintRange != null) lblPrintRange.Text = rangeText;

                var dt = PKDatabaseHelper.GetPackingHistoryReport(productId, fromDate, toDate);

                if (pnlResults != null) pnlResults.Visible = true;

                if (dt == null || dt.Rows.Count == 0)
                {
                    if (pnlEmpty != null) pnlEmpty.Visible = true;
                    if (pnlTable != null) pnlTable.Visible = false;
                    return;
                }

                if (pnlEmpty != null) pnlEmpty.Visible = false;
                if (pnlTable != null) pnlTable.Visible = true;

                if (rptOrders != null)
                {
                    rptOrders.DataSource = dt;
                    rptOrders.DataBind();
                }

                // Summaries
                int totalOrders = dt.Rows.Count;
                long totalBatches = 0, totalJars = 0, totalUnits = 0;
                foreach (DataRow row in dt.Rows)
                {
                    if (row.Table.Columns.Contains("BatchCount") && row["BatchCount"] != DBNull.Value)
                        totalBatches += Convert.ToInt64(row["BatchCount"]);
                    if (row.Table.Columns.Contains("OrderJars") && row["OrderJars"] != DBNull.Value)
                        totalJars += Convert.ToInt64(row["OrderJars"]);
                    if (row.Table.Columns.Contains("OrderTotalUnits") && row["OrderTotalUnits"] != DBNull.Value)
                        totalUnits += Convert.ToInt64(row["OrderTotalUnits"]);
                }

                if (lblTotalOrders != null)  lblTotalOrders.Text  = totalOrders.ToString("N0");
                if (lblTotalBatches != null) lblTotalBatches.Text = totalBatches.ToString("N0");
                if (lblTotalJars != null)    lblTotalJars.Text    = totalJars.ToString("N0");
                if (lblTotalUnits != null)   lblTotalUnits.Text   = totalUnits.ToString("N0");
            }
            catch (Exception ex)
            {
                if (pnlResults != null) pnlResults.Visible = true;
                if (pnlEmpty != null) pnlEmpty.Visible = true;
                if (pnlTable != null) pnlTable.Visible = false;
                Response.Write("<div style='color:red;padding:20px;font-size:13px;background:#fff3f3;border:2px solid red;margin:20px;border-radius:8px;'>"
                    + "<strong>DEBUG ERROR:</strong> " + Server.HtmlEncode(ex.Message)
                    + "<br/><br/><strong>Type:</strong> " + ex.GetType().FullName
                    + "<br/><br/><strong>Stack:</strong><br/><pre>" + Server.HtmlEncode(ex.StackTrace) + "</pre></div>");
            }
        }

        protected void rptOrders_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            var orderRow = (DataRowView)e.Item.DataItem;
            int orderId  = Convert.ToInt32(orderRow["OrderID"]);

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
