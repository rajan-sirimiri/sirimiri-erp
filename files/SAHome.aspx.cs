using System;
using System.Web.UI;

namespace StockApp
{
    public partial class SAHome : Page
    {
        protected System.Web.UI.WebControls.Label lblUserName, lblUserRole;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                lblUserName.Text = Session["FullName"]?.ToString() ?? "";
                lblUserRole.Text = Session["Role"]?.ToString() ?? "";
            }
        }
    }
}
