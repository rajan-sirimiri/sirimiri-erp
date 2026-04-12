using System;
using System.Web.UI;
using PPApp.DAL;

namespace PPApp
{
    public partial class PPHome : Page
    {
        protected System.Web.UI.HtmlControls.HtmlAnchor lnkProduct, lnkCatalog, lnkDailyPlan, lnkOrder, lnkExecution, lnkPrefilled, lnkPreprocess, lnkBatchCost;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PP_UserID"] == null)
            {
                Response.Redirect("PPLogin.aspx");
                return;
            }

            if (!IsPostBack)
            {
                string fullName = Session["PP_FullName"] as string ?? "";
                string role     = Session["PP_Role"]     as string ?? "";

                lblUserName.Text = fullName;
                lblUserRole.Text = role;
                lblNavUser.Text  = fullName;
            }

            // Hide menu cards based on role access
            string ppRole = Session["PP_Role"]?.ToString() ?? "";
            if (lnkProduct != null) lnkProduct.Visible = PPDatabaseHelper.RoleHasModuleAccess(ppRole, "PP", "PP_PRODUCT");
            if (lnkCatalog != null) lnkCatalog.Visible = PPDatabaseHelper.RoleHasModuleAccess(ppRole, "PP", "PP_PRODUCT");
            if (lnkDailyPlan != null) lnkDailyPlan.Visible = PPDatabaseHelper.RoleHasModuleAccess(ppRole, "PP", "PP_DAILY_PLAN");
            if (lnkOrder != null) lnkOrder.Visible = PPDatabaseHelper.RoleHasModuleAccess(ppRole, "PP", "PP_ORDER");
            if (lnkExecution != null) lnkExecution.Visible = PPDatabaseHelper.RoleHasModuleAccess(ppRole, "PP", "PP_EXECUTION");
            if (lnkPrefilled != null) lnkPrefilled.Visible = PPDatabaseHelper.RoleHasModuleAccess(ppRole, "PP", "PP_PREFILLED");
            if (lnkPreprocess != null) lnkPreprocess.Visible = PPDatabaseHelper.RoleHasModuleAccess(ppRole, "PP", "PP_PREPROCESS");
        }
    }
}
