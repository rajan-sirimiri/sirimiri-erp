using System;
using System.Web.UI;
using StockApp.DAL;

namespace StockApp
{
    public partial class ERPHome : Page
    {
        protected System.Web.UI.WebControls.Panel pnlUACard;
        protected System.Web.UI.WebControls.Panel pnlZohoCard;
        protected System.Web.UI.WebControls.Panel pnlHRCard;
        protected System.Web.UI.HtmlControls.HtmlAnchor lnkMM, lnkPP, lnkPK, lnkSA, lnkBI;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            lblUserName.Text = Session["FullName"]?.ToString() ?? "";
            lblUserRole.Text = Session["Role"]?.ToString() ?? "";

            // Show/hide module cards based on role access
            string userRole = Session["Role"]?.ToString() ?? "";
            pnlUACard.Visible = (userRole == "Super");
            if (pnlZohoCard != null) pnlZohoCard.Visible = (userRole == "Super");
            if (pnlHRCard != null) pnlHRCard.Visible = (userRole == "Super" || userRole == "Admin");
            if (lnkMM != null) lnkMM.Visible = DatabaseHelper.RoleHasAppAccess(userRole, "MM");
            if (lnkPP != null) lnkPP.Visible = DatabaseHelper.RoleHasAppAccess(userRole, "PP");
            if (lnkPK != null) lnkPK.Visible = DatabaseHelper.RoleHasAppAccess(userRole, "PK");
            if (lnkSA != null) lnkSA.Visible = DatabaseHelper.RoleHasAppAccess(userRole, "SA");
            if (lnkBI != null) lnkBI.Visible = DatabaseHelper.RoleHasAppAccess(userRole, "SA"); // BI uses SA access

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
