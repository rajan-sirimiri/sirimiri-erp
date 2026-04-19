using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FINApp
{
    public partial class FINGoodsDelivery : Page
    {
        protected Label lblNavUser;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["FIN_UserID"] == null) { Response.Redirect("FINLogin.aspx"); return; }
            lblNavUser.Text = Session["FIN_FullName"]?.ToString() ?? "";
        }
    }
}
