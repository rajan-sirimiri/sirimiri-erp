using System;
using System.Web.UI;
using PKApp.DAL;

namespace PKApp
{
    public partial class PKFGOpeningStock : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PK_UserID"] == null)
            {
                Response.Redirect("PKLogin.aspx?ReturnUrl=" + Server.UrlEncode(Request.Url.PathAndQuery));
                return;
            }

            string role = Session["PK_Role"]?.ToString() ?? "";
            if (!PKDatabaseHelper.RoleHasModuleAccess(role, "PK", "PK_MASTER"))
            {
                Response.Redirect("PKHome.aspx");
                return;
            }
        }
    }
}
