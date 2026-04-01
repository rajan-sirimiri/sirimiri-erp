using System;
using System.Web.UI;
using StockApp.DAL;

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

            // Show UA card only for Super role
            string userRole = Session["Role"]?.ToString() ?? "";
            pnlUACard.Visible = (userRole == "Super");

            if (!IsPostBack)
            {
                // Generate a fresh SSO token for module links
                int    userId   = Convert.ToInt32(Session["UserID"]);
                string fullName = Session["FullName"]?.ToString() ?? "";
                string role     = Session["Role"]?.ToString() ?? "";

                try
                {
                    string token = DatabaseHelper.CreateSSOToken(userId, fullName, role);
                    hfSSOToken.Value = token;

                    // Cleanup old tokens occasionally
                    DatabaseHelper.CleanupSSOTokens();
                }
                catch
                {
                    // If SSO table doesn't exist yet, links will fall back to normal login
                    hfSSOToken.Value = "";
                }
            }
        }
    }
}
