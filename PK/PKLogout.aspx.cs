using System; using System.Web.UI;
namespace PKApp {
    public partial class PKLogout : Page {
        protected void Page_Load(object s, EventArgs e) { Session.Clear(); Session.Abandon(); Response.Redirect("PKLogin.aspx"); }
    }
}
