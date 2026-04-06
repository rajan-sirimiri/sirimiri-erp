using System;
using System.Configuration;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using MySql.Data.MySqlClient;
using FINApp.DAL;

namespace FINApp
{
    public partial class FINLogin : Page
    {
        protected System.Web.UI.WebControls.Label lblError;
        protected System.Web.UI.WebControls.Panel pnlError;
        protected System.Web.UI.WebControls.TextBox txtUsername, txtPassword;
        protected System.Web.UI.WebControls.Button btnLogin;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Session["FIN_UserID"] != null)
                Response.Redirect("~/FINHome.aspx");

            if (!IsPostBack && Session["FIN_UserID"] == null)
            {
                string ssoToken = Request.QueryString["sso"];
                if (!string.IsNullOrEmpty(ssoToken))
                {
                    var ssoUser = ValidateSSOTokenDirect(ssoToken);
                    if (ssoUser != null)
                    {
                        int userId = Convert.ToInt32(ssoUser["UserID"]);
                        string fullName = ssoUser["FullName"].ToString();
                        string role = ssoUser["Role"].ToString();

                        if (!FINDatabaseHelper.RoleHasAppAccess(role, "FIN"))
                        { ShowError("You do not have access to the Finance module."); return; }

                        Session["FIN_UserID"] = userId;
                        Session["FIN_Username"] = fullName;
                        Session["FIN_FullName"] = fullName;
                        Session["FIN_Role"] = role;
                        FINDatabaseHelper.UpdateLastLogin(userId);
                        Response.Redirect("~/FINHome.aspx");
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
                user = FINDatabaseHelper.ValidateUser(username, hash);
                if (user == null) { ShowError("Invalid username or password."); return; }
                if (!Convert.ToBoolean(user["IsActive"])) { ShowError("Account deactivated."); return; }
            }
            catch (Exception ex) { ShowError("Connection error: " + ex.Message); return; }

            int userId = Convert.ToInt32(user["UserID"]);
            string role = user["Role"].ToString();

            if (!FINDatabaseHelper.RoleHasAppAccess(role, "FIN"))
            { ShowError("No access to Finance module."); return; }

            Session["FIN_UserID"] = userId;
            Session["FIN_Username"] = user["Username"].ToString();
            Session["FIN_FullName"] = user["FullName"].ToString();
            Session["FIN_Role"] = role;
            FINDatabaseHelper.UpdateLastLogin(userId);

            if (user.Table.Columns.Contains("MustChangePwd") && Convert.ToInt32(user["MustChangePwd"]) == 1)
                Response.Redirect("~/FINChangePassword.aspx");
            else
                Response.Redirect("~/FINHome.aspx");
        }

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
                using (var cmd = new MySqlCommand("UPDATE ERP_SSOTokens SET IsUsed=1 WHERE Token=?tok;", conn))
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
