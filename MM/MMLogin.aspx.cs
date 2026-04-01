using System;
using System.Configuration;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using MySql.Data.MySqlClient;
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

            // ── SSO: check for token in query string ──
            if (!IsPostBack && Session["MM_UserID"] == null)
            {
                string ssoToken = Request.QueryString["sso"];
                if (!string.IsNullOrEmpty(ssoToken))
                {
                    var ssoUser = ValidateSSOTokenDirect(ssoToken);
                    if (ssoUser != null)
                    {
                        int    userId   = Convert.ToInt32(ssoUser["UserID"]);
                        string fullName = ssoUser["FullName"].ToString();
                        string role     = ssoUser["Role"].ToString();

                        // Check MM access — Super and Admin always allowed
                        if (role != "Admin" && role != "Super")
                        {
                            var access = MMDatabaseHelper.GetUserAccessList(userId);
                            if (access.Rows.Count == 0)
                            {
                                ShowError("You do not have access to the Materials Management module.");
                                return;
                            }
                        }

                        Session["MM_UserID"]   = userId;
                        Session["MM_Username"] = fullName;
                        Session["MM_FullName"] = fullName;
                        Session["MM_Role"]     = role;

                        MMDatabaseHelper.UpdateLastLogin(userId);
                        Response.Redirect("~/MMHome.aspx");
                        return;
                    }
                    // Token invalid/expired — fall through to normal login
                }
            }
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

            // Check MM access — Super and Admin always allowed
            int userId = Convert.ToInt32(user["UserID"]);
            string role = user["Role"].ToString();

            if (role != "Admin" && role != "Super")
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

        // ── SSO token validation (direct DB call — avoids cross-project DAL dependency) ──
        private DataRow ValidateSSOTokenDirect(string token)
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["StockDB"].ConnectionString;
                DataTable dt = new DataTable();
                using (var conn = new MySqlConnection(connStr))
                using (var cmd = new MySqlCommand(
                    "SELECT Token, UserID, FullName, Role FROM ERP_SSOTokens" +
                    " WHERE Token=?tok AND IsUsed=0 AND ExpiresAt > NOW() LIMIT 1;", conn))
                {
                    cmd.Parameters.AddWithValue("?tok", token);
                    conn.Open();
                    new MySqlDataAdapter(cmd).Fill(dt);
                }
                if (dt.Rows.Count == 0) return null;
                // Mark as used
                using (var conn = new MySqlConnection(connStr))
                using (var cmd = new MySqlCommand(
                    "UPDATE ERP_SSOTokens SET IsUsed=1 WHERE Token=?tok;", conn))
                {
                    cmd.Parameters.AddWithValue("?tok", token);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                return dt.Rows[0];
            }
            catch { return null; }
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
