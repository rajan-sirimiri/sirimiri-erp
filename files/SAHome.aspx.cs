using System;
using System.Web.UI;
using StockApp.DAL;

namespace StockApp
{
    public partial class SAHome : Page
    {
        protected System.Web.UI.WebControls.Label lblUserName, lblUserRole;
        protected System.Web.UI.HtmlControls.HtmlAnchor lnkDistStock, lnkHubStock, lnkFGStock, lnkDailySales, lnkSalesForce, lnkSuperMarkets, lnkReports;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                lblUserName.Text = Session["FullName"]?.ToString() ?? "";
                lblUserRole.Text = Session["Role"]?.ToString() ?? "";
            }

            // Hide menu cards based on role access
            string role = Session["Role"]?.ToString() ?? "";
            if (lnkDistStock != null) lnkDistStock.Visible = DatabaseHelper.RoleHasModuleAccess(role, "SA", "SA_DIST_STOCK");
            if (lnkHubStock != null) lnkHubStock.Visible = DatabaseHelper.RoleHasModuleAccess(role, "SA", "SA_HUB_STOCK");
            if (lnkFGStock != null) lnkFGStock.Visible = DatabaseHelper.RoleHasModuleAccess(role, "SA", "SA_FG_STOCK");
            if (lnkDailySales != null) lnkDailySales.Visible = DatabaseHelper.RoleHasModuleAccess(role, "SA", "SA_DAILY_SALES");
            if (lnkSalesForce != null) lnkSalesForce.Visible = DatabaseHelper.RoleHasModuleAccess(role, "SA", "SA_SALESFORCE");
            if (lnkSuperMarkets != null) lnkSuperMarkets.Visible = DatabaseHelper.RoleHasModuleAccess(role, "SA", "SA_SUPERMARKET");
            if (lnkReports != null) lnkReports.Visible = DatabaseHelper.RoleHasModuleAccess(role, "SA", "SA_REPORTS");
        }
    }
}
