using System;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using StockApp.DAL;

namespace StockApp
{
    public partial class ChangePassword : Page
    {
        private TextBox TxtCurrent => (TextBox)FindControl("txtCurrent");
        private TextBox TxtNew     => (TextBox)FindControl("txtNew");
        private TextBox TxtConfirm => (TextBox)FindControl("txtConfirm");
        private Panel   PnlError   => (Panel)FindControl("pnlError");
        private Label   LblError   => (Label)FindControl("lblError");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null) Response.Redirect("~/Login.aspx");
        }

        protected void btnChange_Click(object sender, EventArgs e)
        {
            int    userId  = Convert.ToInt32(Session["UserID"]);
            string current = TxtCurrent.Text;
            string newPwd  = TxtNew.Text;
            string confirm = TxtConfirm.Text;

            string currentHash = Login.ComputeSHA256(current);
            if (!DatabaseHelper.VerifyPassword(userId, currentHash))
            { ShowError("Current password is incorrect."); return; }

            if (newPwd.Length < 8 || !Regex.IsMatch(newPwd, "[A-Z]") || !Regex.IsMatch(newPwd, "[0-9]"))
            { ShowError("Password must be at least 8 characters with one uppercase letter and one number."); return; }

            if (newPwd != confirm)
            { ShowError("Passwords do not match."); return; }

            if (current == newPwd)
            { ShowError("New password must be different from the current password."); return; }

            DatabaseHelper.ChangePassword(userId, Login.ComputeSHA256(newPwd));
            DatabaseHelper.LogAudit(userId, "ChangePassword", null, null, Request.UserHostAddress);
            Response.Redirect("~/StockEntry.aspx");
        }

        private void ShowError(string msg) { LblError.Text = msg; PnlError.Visible = true; }
    }
}
