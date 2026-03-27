using System;
using System.Configuration;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using StockApp.DAL;

namespace DataImport
{
    public partial class Login : Page
    {
        private TextBox TxtUsername => (TextBox)FindControl("txtUsername");
        private TextBox TxtPassword => (TextBox)FindControl("txtPassword");
        private Panel   PnlError    => (Panel)FindControl("pnlError");
        private Label   LblError    => (Label)FindControl("lblError");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Session["UserID"] != null)
                Response.Redirect("~/Import.aspx");

            // ── SSO: check for token in query string ──
            if (!IsPostBack && Session["UserID"] == null)
            {
                string ssoToken = Request.QueryString["sso"];
                if (!string.IsNullOrEmpty(ssoToken))
                {
                    var ssoUser = ValidateSSOTokenDirect(ssoToken);
                    if (ssoUser != null)
                    {
                        string role = ssoUser["Role"].ToString();
                        if (role == "FieldUser")
                        {
                            ShowError("Access denied. Manager or Admin access required.");
                            return;
                        }

                        int    userId   = Convert.ToInt32(ssoUser["UserID"]);
                        string fullName = ssoUser["FullName"].ToString();

                        Session["UserID"]   = userId;
                        Session["Username"] = fullName;
                        Session["FullName"] = fullName;
                        Session["Role"]     = role;

                        DatabaseHelper.UpdateLastLogin(userId);
                        DatabaseHelper.LogAudit(userId, "DataImport_SSO_Login", null, null, Request.UserHostAddress);

                        Response.Redirect("~/Import.aspx");
                        return;
                    }
                }
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = TxtUsername.Text.Trim();
            string password = TxtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            { ShowError("Please enter username and password."); return; }

            DataRow user = null;
            try
            {
                string hash = ComputeSHA256(password);
                user = DatabaseHelper.ValidateUser(username, hash);

                if (user == null)
                { ShowError("Invalid username or password."); return; }

                if (!Convert.ToBoolean(user["IsActive"]))
                { ShowError("Your account is deactivated. Contact Admin."); return; }

                string role = user["Role"].ToString();
                if (role == "FieldUser")
                { ShowError("Access denied. Manager or Admin access required."); return; }
            }
            catch (Exception ex)
            {
                ShowError("Connection error: " + ex.Message);
                return;
            }

            Session["UserID"]   = Convert.ToInt32(user["UserID"]);
            Session["Username"] = user["Username"].ToString();
            Session["FullName"] = user["FullName"].ToString();
            Session["Role"]     = user["Role"].ToString();

            int uid = Convert.ToInt32(user["UserID"]);
            DatabaseHelper.UpdateLastLogin(uid);
            DatabaseHelper.LogAudit(uid, "DataImport_Login", null, null, Request.UserHostAddress);

            if (Convert.ToBoolean(user["MustChangePwd"]))
                Response.Redirect("~/ChangePassword.aspx");
            else
                Response.Redirect("~/Import.aspx");
        }

        // ── SSO token validation (direct DB call) ──
        private DataRow ValidateSSOTokenDirect(string token)
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["StockDBConnection"].ConnectionString;
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
            LblError.Text    = msg;
            PnlError.Visible = true;
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
