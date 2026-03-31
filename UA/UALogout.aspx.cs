using System;
using System.Web.UI;

namespace UAApp
{
    public partial class UALogout : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session.Remove("UA_UserID");
            Session.Remove("UA_FullName");
            Session.Remove("UA_Role");
            Response.Redirect("UALogin.aspx");
        }
    }
}
