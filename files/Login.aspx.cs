using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using StockApp.DAL;

namespace StockApp
{
    public partial class Login : Page
    {
        private TextBox  TxtUsername => (TextBox)FindControl("txtUsername");
        private TextBox  TxtPassword => (TextBox)FindControl("txtPassword");
        private Panel    PnlError    => (Panel)FindControl("pnlError");
        private Label    LblError    => (Label)FindControl("lblError");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Session["UserID"] != null)
                Response.Redirect("~/ERPHome.aspx");
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
                {
                    ShowError("Invalid username or password. [Hash=" + hash.Substring(0, 8) + "...]");
                    return;
                }

                if (!Convert.ToBoolean(user["IsActive"]))
                { ShowError("Your account has been deactivated. Please contact Admin."); return; }
            }
            catch (Exception ex)
            {
                ShowError("Connection error: " + ex.Message +
                    (ex.InnerException != null ? " | " + ex.InnerException.Message : ""));
                return;
            }

            try
            {
                Session["UserID"]   = Convert.ToInt32(user["UserID"]);
                Session["Username"] = user["Username"].ToString();
                Session["FullName"] = user["FullName"].ToString();
                Session["Role"]     = user["Role"].ToString();
                Session["StateID"]  = (user["StateID"] == DBNull.Value || user["StateID"] == null)
                                        ? (object)null
                                        : Convert.ToInt32(user["StateID"]);

                int uid = Convert.ToInt32(user["UserID"]);
                bool mustChange = Convert.ToInt32(user["MustChangePwd"]) == 1;

                DatabaseHelper.UpdateLastLogin(uid);
                DatabaseHelper.LogAudit(uid, "Login", null, null, Request.UserHostAddress);

                if (mustChange)
                    Response.Redirect("~/ChangePassword.aspx");
                else
                    Response.Redirect("~/ERPHome.aspx");
            }
            catch (Exception ex)
            {
                ShowError("Session error: " + ex.Message);
            }
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
