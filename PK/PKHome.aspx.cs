using System; using System.Web.UI;
namespace PKApp {
    public partial class PKHome : Page {
        protected System.Web.UI.WebControls.Label lblUser, lblNavUser;
        protected void Page_Load(object s, EventArgs e) {
            if (Session["PK_UserID"] == null) { Response.Redirect("PKLogin.aspx"); return; }
            lblUser.Text = Session["PK_FullName"] as string ?? "";
            if (lblNavUser != null) lblNavUser.Text = Session["PK_FullName"] as string ?? "";
        }
    }
}
