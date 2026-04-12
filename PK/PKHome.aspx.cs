using System; using System.Web.UI;
using PKApp.DAL;
namespace PKApp {
    public partial class PKHome : Page {
        protected System.Web.UI.WebControls.Label lblUser, lblNavUser;
        protected System.Web.UI.HtmlControls.HtmlAnchor lnkCustomer, lnkPMMapping, lnkMachine, lnkPrimary, lnkSecondary, lnkShipment, lnkReports, lnkPMReport, lnkHistory;
        protected void Page_Load(object s, EventArgs e) {
            if (Session["PK_UserID"] == null) { Response.Redirect("PKLogin.aspx?ReturnUrl=" + Server.UrlEncode(Request.Url.PathAndQuery)); return; }
            lblUser.Text = Session["PK_FullName"] as string ?? "";
            if (lblNavUser != null) lblNavUser.Text = Session["PK_FullName"] as string ?? "";

            // Hide menu cards based on role access
            string role = Session["PK_Role"]?.ToString() ?? "";
            if (lnkCustomer != null) lnkCustomer.Visible = PKDatabaseHelper.RoleHasModuleAccess(role, "PK", "PK_CUSTOMER");
            if (lnkPMMapping != null) lnkPMMapping.Visible = PKDatabaseHelper.RoleHasModuleAccess(role, "PK", "PK_PM_MAPPING");
            if (lnkMachine != null) lnkMachine.Visible = role == "Super Admin";
            if (lnkPrimary != null) lnkPrimary.Visible = PKDatabaseHelper.RoleHasModuleAccess(role, "PK", "PK_PRIMARY");
            if (lnkSecondary != null) lnkSecondary.Visible = PKDatabaseHelper.RoleHasModuleAccess(role, "PK", "PK_SECONDARY");
            if (lnkShipment != null) lnkShipment.Visible = PKDatabaseHelper.RoleHasModuleAccess(role, "PK", "PK_SHIPMENT");
            if (lnkReports != null) lnkReports.Visible = PKDatabaseHelper.RoleHasModuleAccess(role, "PK", "PK_REPORTS");
            if (lnkPMReport != null) lnkPMReport.Visible = PKDatabaseHelper.RoleHasModuleAccess(role, "PK", "PK_PM_REPORT");
            if (lnkHistory != null) lnkHistory.Visible = PKDatabaseHelper.RoleHasModuleAccess(role, "PK", "PK_HISTORY");
        }
    }
}
