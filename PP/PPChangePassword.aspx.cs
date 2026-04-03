using System;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using PPApp.DAL;

namespace PPApp
{
    public partial class PPChangePassword : Page
    {
        protected TextBox txtCurrent, txtNew, txtConfirm;
        protected Panel pnlError;
        protected Label lblError;
        protected Button btnChange;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PP_UserID"] == null) Response.Redirect("PPLogin.aspx");
        }

        protected void btnChange_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["PP_UserID"]);
            string current = txtCurrent.Text;
            string newPwd = txtNew.Text;
            string confirm = txtConfirm.Text;

            string currentHash = PPLogin.ComputeSHA256(current);
            if (!PPDatabaseHelper.VerifyPassword(userId, currentHash))
            { ShowError("Current password is incorrect."); return; }

            if (newPwd.Length < 8 || !Regex.IsMatch(newPwd, "[A-Z]") || !Regex.IsMatch(newPwd, "[0-9]"))
            { ShowError("Password must be at least 8 characters with one uppercase letter and one number."); return; }

            if (newPwd != confirm)
            { ShowError("Passwords do not match."); return; }

            if (current == newPwd)
            { ShowError("New password must be different from the current password."); return; }

            PPDatabaseHelper.ChangePassword(userId, PPLogin.ComputeSHA256(newPwd));
            Response.Redirect("PPHome.aspx");
        }

        private void ShowError(string msg)
        {
            if (lblError != null) lblError.Text = msg;
            if (pnlError != null) pnlError.Visible = true;
        }
    }
}