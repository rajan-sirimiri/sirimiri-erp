using System;
using System.Web.UI;

namespace StockApp
{
    public partial class ERPHome : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            lblUserName.Text = Session["FullName"]?.ToString() ?? "";
            lblUserRole.Text = Session["Role"]?.ToString() ?? "";
        }
    }
}
