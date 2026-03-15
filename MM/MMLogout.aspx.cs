using System;
using System.Web.UI;

namespace MMApp
{
    public partial class MMLogout : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session.Remove("MM_UserID");
            Session.Remove("MM_Username");
            Session.Remove("MM_FullName");
            Session.Remove("MM_Role");
            Response.Redirect("~/MMLogin.aspx");
        }
    }
}
