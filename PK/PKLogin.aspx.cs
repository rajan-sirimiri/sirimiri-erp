using System;
using System.Configuration;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using MySql.Data.MySqlClient;
using PKApp.DAL;

namespace PKApp
{
    public partial class PKLogin : Page
    {
        protected System.Web.UI.WebControls.TextBox txtUser, txtPass;
        protected System.Web.UI.WebControls.Button btnLogin;
        protected System.Web.UI.WebControls.Panel pnlErr;
        protected System.Web.UI.WebControls.Label lblErr;

        protected void Page_Load(object s, EventArgs e)
        {
            if (!IsPostBack && Session["PK_UserID"] != null)
                Response.Redirect(GetReturnUrl());

            // ── SSO: check for token in query string ──
            if (!IsPostBack && Session["PK_UserID"] == null)
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

                        Session["PK_UserID"]   = userId;
                        Session["PK_FullName"] = fullName;
                        Session["PK_Role"]     = role;

                        Response.Redirect(GetReturnUrl());
                        return;
                    }
                }
            }
        }

        protected void btnLogin_Click(object s, EventArgs e)
        {
            string u = txtUser.Text.Trim(), p = txtPass.Text;
            if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p))
            { ShowErr("Enter username and password."); return; }

            string hash;
            using (var sha = SHA256.Create())
            {
                hash = BitConverter.ToString(
                    sha.ComputeHash(Encoding.UTF8.GetBytes(p)))
                    .Replace("-", "").ToLower();
            }

            var row = PKDatabaseHelper.ValidateUser(u, hash);
            if (row == null) { ShowErr("Invalid credentials."); return; }

            Session["PK_UserID"]   = Convert.ToInt32(row["UserID"]);
            Session["PK_FullName"] = row["FullName"].ToString();
            Session["PK_Role"]     = row["Role"].ToString();
            Response.Redirect(GetReturnUrl());
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

        void ShowErr(string m) { lblErr.Text = m; pnlErr.Visible = true; }

        private string GetReturnUrl()
        {
            string returnUrl = Request.QueryString["ReturnUrl"];
            if (!string.IsNullOrEmpty(returnUrl))
            {
                if (returnUrl.StartsWith("/") || returnUrl.StartsWith("PK"))
                    return returnUrl;
            }
            return "PKHome.aspx";
        }
    }
}
