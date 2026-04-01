using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using MMApp.DAL;

namespace MMApp
{
    public partial class MMScrapStockReport : Page
    {
        protected Label    lblNavUser;
        protected Label    lblReportDate;
        protected Label    lblPrintDate;
        protected Repeater rptStock;
        protected Label    lblTotal;
        protected Label    lblInStock;
        protected Label    lblZero;
        protected Button   btnRefresh;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["MM_UserID"] == null) { Response.Redirect("MMLogin.aspx"); return; }

            // Module access check
            string __role = Session["MM_Role"]?.ToString() ?? "";
            if (!MMDatabaseHelper.RoleHasModuleAccess(__role, "MM", "MM_SCRAP_REPORT"))
            { Response.Redirect("MMHome.aspx"); return; }
            lblNavUser.Text = Session["MM_FullName"] as string ?? "";
            if (!IsPostBack) BindReport();
        }

        protected void btnRefresh_Click(object sender, EventArgs e) { BindReport(); }

        private void BindReport()
        {
            string now = DateTime.Now.ToString("dd MMM yyyy, hh:mm tt");
            lblReportDate.Text = now;
            lblPrintDate.Text  = now;

            var dt = MMDatabaseHelper.GetScrapStockReport();
            rptStock.DataSource = dt;
            rptStock.DataBind();

            int total = dt.Rows.Count, inStock = 0, zero = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (Convert.ToDecimal(row["StockQty"]) > 0) inStock++;
                else zero++;
            }
            lblTotal.Text   = total.ToString();
            lblInStock.Text = inStock.ToString();
            lblZero.Text    = zero.ToString();
        }

        protected string FormatQty(object val)
        {
            if (val == null || val == DBNull.Value) return "0";
            return Convert.ToDecimal(val).ToString("0.###");
        }
    }
}
