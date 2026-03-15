using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using StockApp.DAL;

namespace DataImport
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
            int userId = Convert.ToInt32(Session["UserID"]);
            string curHash = Login.ComputeSHA256(TxtCurrent.Text);
            string newPwd  = TxtNew.Text;
            string conPwd  = TxtConfirm.Text;

            if (!DatabaseHelper.VerifyPassword(userId, curHash))
            { ShowError("Current password is incorrect."); return; }

            if (newPwd.Length < 8 || !System.Text.RegularExpressions.Regex.IsMatch(newPwd, @"[A-Z]")
                || !System.Text.RegularExpressions.Regex.IsMatch(newPwd, @"[0-9]"))
            { ShowError("Password must be at least 8 characters with 1 uppercase and 1 number."); return; }

            if (newPwd != conPwd)
            { ShowError("Passwords do not match."); return; }

            if (Login.ComputeSHA256(newPwd) == curHash)
            { ShowError("New password must be different from current password."); return; }

            DatabaseHelper.ChangePassword(userId, Login.ComputeSHA256(newPwd));
            Response.Redirect("~/Import.aspx");
        }

        private void ShowError(string msg) { LblError.Text = msg; PnlError.Visible = true; }
    }
}
