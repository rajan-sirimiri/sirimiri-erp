using System;
using System.Web.UI;
using MMApp.DAL;

namespace MMApp
{
    public partial class MMHome : Page
    {
        protected global::System.Web.UI.WebControls.Label     lblUserName;
        protected global::System.Web.UI.WebControls.Label     lblUserRole;
        protected global::System.Web.UI.WebControls.Label     lblNavUser;
        protected System.Web.UI.HtmlControls.HtmlAnchor lnkSupplier, lnkRawMaterial, lnkPackingMaterial, lnkConsumable, lnkStationary, lnkScrapMaterial, lnkUOM;
        protected System.Web.UI.HtmlControls.HtmlAnchor lnkRawInward, lnkPackingInward, lnkConsumableInward, lnkStationaryInward;
        protected System.Web.UI.HtmlControls.HtmlAnchor lnkRMReport, lnkPMReport, lnkScrapReport, lnkRecon, lnkBulk, lnkOpeningStock, lnkCNReport, lnkSTReport;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["MM_UserID"] == null) { Response.Redirect("MMLogin.aspx"); return; }

            lblUserName.Text = Session["MM_FullName"]?.ToString() ?? "";
            lblUserRole.Text = Session["MM_Role"]?.ToString() ?? "";
            lblNavUser.Text  = Session["MM_FullName"]?.ToString() ?? "";

            // Hide menu cards based on role access
            string role = Session["MM_Role"]?.ToString() ?? "";
            if (lnkSupplier != null) lnkSupplier.Visible = MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_SUPPLIER");
            if (lnkRawMaterial != null) lnkRawMaterial.Visible = MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_RM_MASTER");
            if (lnkPackingMaterial != null) lnkPackingMaterial.Visible = MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_PM_MASTER");
            if (lnkConsumable != null) lnkConsumable.Visible = MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_CM_MASTER");
            if (lnkStationary != null) lnkStationary.Visible = MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_ST_MASTER");
            if (lnkScrapMaterial != null) lnkScrapMaterial.Visible = MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_SCRAP_MASTER");
            if (lnkUOM != null) lnkUOM.Visible = MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_UOM");
            if (lnkRawInward != null) lnkRawInward.Visible = MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_RM_GRN");
            if (lnkPackingInward != null) lnkPackingInward.Visible = MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_PM_GRN");
            if (lnkConsumableInward != null) lnkConsumableInward.Visible = MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_CM_GRN");
            if (lnkStationaryInward != null) lnkStationaryInward.Visible = MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_ST_GRN");
            if (lnkRMReport != null) lnkRMReport.Visible = MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_RM_REPORT");
            if (lnkPMReport != null) lnkPMReport.Visible = MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_PM_REPORT");
            if (lnkScrapReport != null) lnkScrapReport.Visible = MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_SCRAP_REPORT");
            if (lnkRecon != null) lnkRecon.Visible = MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_RECON");
            if (lnkBulk != null) lnkBulk.Visible = MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_BULK");
            if (lnkOpeningStock != null) lnkOpeningStock.Visible = MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_BULK");
            if (lnkCNReport != null) lnkCNReport.Visible = MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_CN_REPORT");
            if (lnkSTReport != null) lnkSTReport.Visible = MMDatabaseHelper.RoleHasModuleAccess(role, "MM", "MM_ST_REPORT");
        }
    }
}
