using System;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKApp.DAL;

namespace PKApp
{
    public partial class PKChangePassword : Page
    {
        protected TextBox txtCurrent, txtNew, txtConfirm;
        protected Panel pnlError;
        protected Label lblError;
        protected Button btnChange;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PK_UserID"] == null) Response.Redirect("PKLogin.aspx");
        }

        protected void btnChange_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["PK_UserID"]);
            string current = txtCurrent.Text;
            string newPwd = txtNew.Text;
            string confirm = txtConfirm.Text;

            string currentHash = PKLogin.ComputeSHA256(current);
            if (!PKDatabaseHelper.VerifyPassword(userId, currentHash))
            { ShowError("Current password is incorrect."); return; }

            if (newPwd.Length < 8 || !Regex.IsMatch(newPwd, "[A-Z]") || !Regex.IsMatch(newPwd, "[0-9]"))
            { ShowError("Password must be at least 8 characters with one uppercase letter and one number."); return; }

            if (newPwd != confirm)
            { ShowError("Passwords do not match."); return; }

            if (current == newPwd)
            { ShowError("New password must be different from the current password."); return; }

            PKDatabaseHelper.ChangePassword(userId, PKLogin.ComputeSHA256(newPwd));
            Response.Redirect("PKHome.aspx");
        }

        private void ShowError(string msg)
        {
            if (lblError != null) lblError.Text = msg;
            if (pnlError != null) pnlError.Visible = true;
        }
    }
}