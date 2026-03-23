using System; using System.Security.Cryptography; using System.Text;
using System.Web.UI; using PKApp.DAL;
namespace PKApp {
    public partial class PKLogin : Page {
        protected System.Web.UI.WebControls.TextBox txtUser, txtPass;
        protected System.Web.UI.WebControls.Button btnLogin;
        protected System.Web.UI.WebControls.Panel pnlErr;
        protected System.Web.UI.WebControls.Label lblErr;
        protected void Page_Load(object s, EventArgs e) {
            if (!IsPostBack && Session["PK_UserID"] != null) Response.Redirect("PKHome.aspx");
        }
        protected void btnLogin_Click(object s, EventArgs e) {
            string u = txtUser.Text.Trim(), p = txtPass.Text;
            if (string.IsNullOrEmpty(u)||string.IsNullOrEmpty(p)){ShowErr("Enter username and password.");return;}
            string hash; using(var sha=SHA256.Create()){hash=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(p))).Replace("-","").ToLower();}
            var row = PKDatabaseHelper.ValidateUser(u, hash);
            if (row == null){ShowErr("Invalid credentials.");return;}
            Session["PK_UserID"]   = Convert.ToInt32(row["UserID"]);
            Session["PK_FullName"] = row["FullName"].ToString();
            Session["PK_Role"]     = row["Role"].ToString();
            Response.Redirect("PKHome.aspx");
        }
        void ShowErr(string m){lblErr.Text=m;pnlErr.Visible=true;}
    }
}
