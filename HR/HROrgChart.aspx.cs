using System;
using System.Web.UI;

namespace HRModule
{
    /// <summary>
    /// Org chart page. The server only handles auth + nav rendering; all the
    /// graph drawing and detail-fetching happens in the browser via
    /// HROrgChart.ashx (?action=tree | ?action=detail&id=N).
    ///
    /// This keeps the page postback-free (one-page-app feel) and makes pan/zoom
    /// snappy — no full-page reloads when clicking around.
    /// </summary>
    public partial class HROrgChart : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // --- Auth gate (mirrors HREmployee.aspx) ---
            if (Session["HR_UserID"] == null && Session["UserID"] == null)
            {
                Response.Redirect("HRLogin.aspx", true);
                return;
            }

            string role = (Session["HR_Role"] as string)
                       ?? (Session["UserRole"] as string)
                       ?? (Session["Role"] as string);
            if (role != "Super" && role != "Admin")
            {
                Response.Redirect("HRLogin.aspx", true);
                return;
            }

            // Show user name in top nav
            string navName = (Session["HR_FullName"] as string)
                          ?? (Session["FullName"] as string)
                          ?? (Session["UserName"] as string)
                          ?? "";
            if (!string.IsNullOrEmpty(navName) && lblNavUser != null)
                lblNavUser.Text = navName;
        }
    }
}
