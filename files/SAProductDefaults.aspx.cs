using System;
using System.Web.UI;
using StockApp.DAL;

namespace StockApp
{
    public partial class SAProductDefaults : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Login.aspx"); return; }

            // Only Admin / Super can change defaults
            string role = Session["Role"]?.ToString() ?? "";
            if (role != "Admin" && role != "Super")
            { Response.Redirect("SAHome.aspx"); return; }
        }
    }
}
