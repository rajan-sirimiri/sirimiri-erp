using System;
using System.Configuration;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using MySql.Data.MySqlClient;
using PPApp.DAL;

namespace PPApp
{
    public partial class PPLogin : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Session["PP_UserID"] != null)
                Response.Redirect("~/PPHome.aspx");

            // ── SSO: check for token in query string ──
            if (!IsPostBack && Session["PP_UserID"] == null)
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

                        // Check PP access — role-based
                        if (!PPDatabaseHelper.RoleHasAppAccess(role, "PP"))
                        { ShowError("You do not have access to the Production Planning module."); return; }

                        Session["PP_UserID"]   = userId;
                        Session["PP_Username"] = fullName;
                        Session["PP_FullName"] = fullName;
                        Session["PP_Role"]     = role;

                        PPDatabaseHelper.UpdateLastLogin(userId);
                        Response.Redirect("~/PPHome.aspx");
                        return;
                    }
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

            // Check PP access — role-based
            if (!PPDatabaseHelper.RoleHasAppAccess(role, "PP"))
            { ShowError("You do not have access to the Production Planning module. Please contact Admin."); return; }

            Session["PP_UserID"]   = userId;
            Session["PP_Username"] = user["Username"].ToString();
            Session["PP_FullName"] = user["FullName"].ToString();
            Session["PP_Role"]     = role;

            PPDatabaseHelper.UpdateLastLogin(userId);

            if (user.Table.Columns.Contains("MustChangePwd") && Convert.ToInt32(user["MustChangePwd"]) == 1)
                Response.Redirect("~/PPChangePassword.aspx");
            else
                Response.Redirect("~/PPHome.aspx");
        }

        // ── SSO token validation (direct DB call) ──
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
