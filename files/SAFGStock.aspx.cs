using System;
using System.Web.UI;
using StockApp.DAL;

namespace StockApp
{
    public partial class SAFGStock : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Login.aspx"); return; }

            string role = Session["Role"]?.ToString() ?? "";
            if (!DatabaseHelper.RoleHasModuleAccess(role, "SA", "SA_FG_STOCK"))
            { Response.Redirect("SAHome.aspx"); return; }
        }
    }
}
