using System;
using System.Web.UI;

namespace PPApp
{
    public partial class PPLogout : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session.Remove("PP_UserID");
            Session.Remove("PP_Username");
            Session.Remove("PP_FullName");
            Session.Remove("PP_Role");
            Response.Redirect("~/PPLogin.aspx");
        }
    }
}
