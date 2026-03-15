using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
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
