using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using PPApp.DAL;

namespace PPApp
{
    public partial class PPLogin : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Session["PP_UserID"] != null)
                Response.Redirect("~/PPHome.aspx");
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
                user = PPDatabaseHelper.ValidateUser(username, hash);

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

            int    userId = Convert.ToInt32(user["UserID"]);
            string role   = user["Role"].ToString();

            Session["PP_UserID"]   = userId;
            Session["PP_Username"] = user["Username"].ToString();
            Session["PP_FullName"] = user["FullName"].ToString();
            Session["PP_Role"]     = role;

            PPDatabaseHelper.UpdateLastLogin(userId);

            Response.Redirect("~/PPHome.aspx");
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
