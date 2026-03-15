using System;
using System.Web.UI;

namespace PPApp
{
    public partial class PPHome : Page
    {
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
        }
    }
}
