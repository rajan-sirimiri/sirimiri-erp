using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FINApp
{
    public partial class FINConsignments : Page
    {
        protected Label lblNavUser;
        protected Panel pnlReadOnlyBanner;

        /// <summary>Roles allowed to perform write actions (approve, edit, dispatch) in the
        /// FIN Consignments flow. "Super" is the admin role; "Finance" is the functional role.
        /// Other roles see the pages in view-only mode.</summary>
        public static readonly string[] FinanceRoles = new[] { "Finance", "Super" };

        public static bool IsFinanceRole(string roleCode)
        {
            if (string.IsNullOrEmpty(roleCode)) return false;
            foreach (var r in FinanceRoles)
                if (string.Equals(r, roleCode, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["FIN_UserID"] == null) { Response.Redirect("FINLogin.aspx"); return; }
            lblNavUser.Text = Session["FIN_FullName"]?.ToString() ?? "";

            // Show the read-only banner when the user's role isn't in the finance allowlist.
            // Sub-pages (Invoice Processing etc.) enforce the actual write restrictions; the
            // banner here is an advance notice so users aren't surprised when buttons are disabled.
            string role = Session["FIN_Role"]?.ToString() ?? "";
            pnlReadOnlyBanner.Visible = !IsFinanceRole(role);
        }
    }
}
