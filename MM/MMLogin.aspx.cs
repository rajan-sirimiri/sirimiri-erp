using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using MMApp.DAL;

namespace MMApp
{
    public partial class MMLogin : Page
    {
        protected global::System.Web.UI.WebControls.TextBox   txtUsername;
        protected global::System.Web.UI.WebControls.TextBox   txtPassword;
        protected global::System.Web.UI.WebControls.Button    btnLogin;
        protected global::System.Web.UI.WebControls.Panel     pnlError;
        protected global::System.Web.UI.WebControls.Label     lblError;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Session["MM_UserID"] != null)
                Response.Redirect("~/MMHome.aspx");
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            { ShowError("Please enter username and password."); return; }

            DataRow user = null;
            try
            {
                string hash = ComputeSHA256(password);
                user = MMDatabaseHelper.ValidateUser(username, hash);

                if (user == null)
                { ShowError("Invalid username or password."); return; }

                if (!Convert.ToBoolean(user["IsActive"]))
                { ShowError("Your account has been deactivated. Please contact Admin."); return; }
            }
            catch (Exception ex)
            {
                ShowError("Connection error: " + ex.Message);
                return;
            }

            // Check MM access — Admin always allowed, others need at least one MM module grant
            int userId = Convert.ToInt32(user["UserID"]);
            string role = user["Role"].ToString();

            if (role != "Admin")
            {
                var access = MMDatabaseHelper.GetUserAccessList(userId);
                if (access.Rows.Count == 0)
                { ShowError("You do not have access to the Materials Management module. Please contact Admin."); return; }
            }

            Session["MM_UserID"]   = userId;
            Session["MM_Username"] = user["Username"].ToString();
            Session["MM_FullName"] = user["FullName"].ToString();
            Session["MM_Role"]     = role;

            MMDatabaseHelper.UpdateLastLogin(userId);

            if (Convert.ToInt32(user["MustChangePwd"]) == 1)
                Response.Redirect("~/MMChangePassword.aspx");
            else
                Response.Redirect("~/MMHome.aspx");
        }

        private void ShowError(string msg)
        {
            lblError.Text    = msg;
            pnlError.Visible = true;
        }

        public static string ComputeSHA256(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
