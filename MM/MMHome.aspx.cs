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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["MM_UserID"] == null) { Response.Redirect("MMLogin.aspx"); return; }

            lblUserName.Text = Session["MM_FullName"]?.ToString() ?? "";
            lblUserRole.Text = Session["MM_Role"]?.ToString() ?? "";
            lblNavUser.Text  = Session["MM_FullName"]?.ToString() ?? "";
        }
    }
}
