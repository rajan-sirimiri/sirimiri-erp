using System;
using System.Web.UI;

namespace FINApp
{
    public partial class FINSalesAnalytics : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["FIN_UserID"] == null)
            {
                Response.Redirect("FINLogin.aspx");
                return;
            }
        }
    }
}
