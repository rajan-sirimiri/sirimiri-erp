using System;
using System.Configuration;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using MySql.Data.MySqlClient;

namespace HRModule
{
    public partial class HRLogin : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Already logged in to HR -> straight to Employee list
            if (!IsPostBack && Session["HR_UserID"] != null)
            {
                Response.Redirect("~/HREmployee.aspx");
                return;
            }

            // SSO bridge from ERPHome — if a token is supplied, validate and auto-login
            if (!IsPostBack && Session["HR_UserID"] == null)
            {
                string ssoToken = Request.QueryString["sso"];
                if (!string.IsNullOrEmpty(ssoToken))
                {
                    DataRow ssoUser = ValidateSSOTokenDirect(ssoToken);
                    if (ssoUser != null)
                    {
                        int userId = Convert.ToInt32(ssoUser["UserID"]);
                        string fullName = ssoUser["FullName"].ToString();
                        string role = ssoUser["Role"].ToString();

                        if (!IsHRAllowedRole(role))
                        {
                            ShowError("You do not have access to the HR module.");
                            return;
                        }

                        SetHRSession(userId, fullName, fullName, role);
                        Response.Redirect("~/HREmployee.aspx");
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
            {
                ShowError("Please enter username and password.");
                return;
            }

            DataRow user;
            try
            {
                string hash = ComputeSHA256(password);
                user = ValidateUser(username, hash);
                if (user == null)
                {
                    ShowError("Invalid username or password.");
                    return;
                }
                if (!Convert.ToBoolean(user["IsActive"]))
                {
                    ShowError("Account is deactivated.");
                    return;
                }
            }
            catch (Exception ex)
            {
                ShowError("Connection error: " + ex.Message);
                return;
            }

            int userId = Convert.ToInt32(user["UserID"]);
            string role = user["Role"].ToString();

            if (!IsHRAllowedRole(role))
            {
                ShowError("Your role does not have access to the HR module.");
                return;
            }

            SetHRSession(
                userId,
                user["Username"].ToString(),
                user["FullName"].ToString(),
                role);

            UpdateLastLogin(userId);

            Response.Redirect("~/HREmployee.aspx");
        }

        // =====================================================================
        // Role gate — HR is restricted to Super and Admin only.
        // =====================================================================
        private static bool IsHRAllowedRole(string role)
        {
            if (string.IsNullOrEmpty(role)) return false;
            return role == "Super" || role == "Admin";
        }

        // =====================================================================
        // Session keys mirror MM/PP/PK/FIN convention: HR_ prefix.
        // =====================================================================
        private void SetHRSession(int userId, string username, string fullName, string role)
        {
            Session["HR_UserID"]   = userId;
            Session["HR_Username"] = username;
            Session["HR_FullName"] = fullName;
            Session["HR_Role"]     = role;
            // Mirror the unprefixed keys too — some pages from Session 12
            // were written assuming Session["UserID"] / Session["UserRole"].
            // Setting both means either pattern works.
            Session["UserID"]   = userId;
            Session["UserName"] = fullName;
            Session["UserRole"] = role;
            Session["FullName"] = fullName;
            Session["Role"]     = role;
        }

        // =====================================================================
        // DB helpers — direct MySQL calls so HRLogin doesn't depend on the
        // shape of HR_DatabaseHelper. Connection string name is "StockDB".
        // =====================================================================
        private static string ConnStr
        {
            get { return ConfigurationManager.ConnectionStrings["StockDB"].ConnectionString; }
        }

        private static DataRow ValidateUser(string username, string passwordHash)
        {
            DataTable dt = new DataTable();
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "SELECT UserID, Username, FullName, Role, IsActive " +
                "FROM Users " +
                "WHERE Username = ?u AND PasswordHash = ?p LIMIT 1;", conn))
            {
                cmd.Parameters.AddWithValue("?u", username);
                cmd.Parameters.AddWithValue("?p", passwordHash);
                conn.Open();
                new MySqlDataAdapter(cmd).Fill(dt);
            }
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        private static void UpdateLastLogin(int userId)
        {
            try
            {
                using (var conn = new MySqlConnection(ConnStr))
                using (var cmd = new MySqlCommand(
                    "UPDATE Users SET LastLogin = NOW() WHERE UserID = ?id;", conn))
                {
                    cmd.Parameters.AddWithValue("?id", userId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // Non-fatal — login already succeeded.
            }
        }

        // =====================================================================
        // SSO token validation — single-use tokens generated by ERPHome.
        // Same logic as FINLogin.ValidateSSOTokenDirect.
        // =====================================================================
        private static DataRow ValidateSSOTokenDirect(string token)
        {
            try
            {
                DataTable dt = new DataTable();
                using (var conn = new MySqlConnection(ConnStr))
                using (var cmd = new MySqlCommand(
                    "SELECT Token, UserID, FullName, Role FROM ERP_SSOTokens " +
                    "WHERE Token = ?tok AND IsUsed = 0 AND ExpiresAt > NOW() LIMIT 1;", conn))
                {
                    cmd.Parameters.AddWithValue("?tok", token);
                    conn.Open();
                    new MySqlDataAdapter(cmd).Fill(dt);
                }
                if (dt.Rows.Count == 0) return null;

                using (var conn = new MySqlConnection(ConnStr))
                using (var cmd = new MySqlCommand(
                    "UPDATE ERP_SSOTokens SET IsUsed = 1 WHERE Token = ?tok;", conn))
                {
                    cmd.Parameters.AddWithValue("?tok", token);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                return dt.Rows[0];
            }
            catch
            {
                return null;
            }
        }

        // =====================================================================
        // Helpers
        // =====================================================================
        private void ShowError(string msg)
        {
            lblError.Text = msg;
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
