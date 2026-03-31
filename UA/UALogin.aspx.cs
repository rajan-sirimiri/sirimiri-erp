using System;
using System.Data;
using System.Web.UI;
using UAApp.DAL;

namespace UAApp
{
    public partial class UALogin : Page
    {
        protected System.Web.UI.WebControls.Label lblAlert;
        protected System.Web.UI.WebControls.Panel pnlAlert;
        protected System.Web.UI.WebControls.TextBox txtUsername, txtPassword;
        protected System.Web.UI.WebControls.Button btnLogin;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Session["UA_UserID"] != null)
                Response.Redirect("UAHome.aspx");
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            { ShowAlert("Please enter username and password."); return; }

            string hash = UADatabaseHelper.HashPassword(password);
            DataRow user = UADatabaseHelper.ValidateUser(username, hash);
            if (user == null) { ShowAlert("Invalid username or password."); return; }

            int userId = Convert.ToInt32(user["UserID"]);
            string role = user["Role"].ToString();
            if (role != "Super" && !UADatabaseHelper.RoleHasAppAccess(role, "UA"))
            { ShowAlert("You do not have access to User Administration."); return; }

            Session["UA_UserID"] = userId;
            Session["UA_FullName"] = user["FullName"].ToString();
            Session["UA_Role"] = role;
            Response.Redirect("UAHome.aspx");
        }

        private void ShowAlert(string msg) { pnlAlert.Visible = true; lblAlert.Text = msg; }
    }
}
