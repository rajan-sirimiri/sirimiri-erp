using System; using System.Data; using System.Web.UI; using System.Web.UI.WebControls; using PKApp.DAL;
namespace PKApp {
    public partial class PKReports : Page {
        protected Label lblUser, lblDate, lblTotal, lblWithStock, lblZero;
        protected Repeater rptFG;
        protected Button btnRefresh;
        protected void Page_Load(object s, EventArgs e) {
            if (Session["PK_UserID"] == null) { Response.Redirect("PKLogin.aspx"); return; }
            lblUser.Text = Session["PK_FullName"] as string ?? "";
            if (!IsPostBack) BindReport();
        }
        protected void btnRefresh_Click(object s, EventArgs e) { BindReport(); }
        void BindReport() {
            lblDate.Text = PKDatabaseHelper.NowIST().ToString("dd MMM yyyy hh:mm tt");
            var dt = PKDatabaseHelper.GetFGStockSummary();
            rptFG.DataSource = dt; rptFG.DataBind();
            int total = dt.Rows.Count, withStock = 0, zero = 0;
            foreach (DataRow r in dt.Rows) { if (Convert.ToDecimal(r["FGAvailable"]) > 0) withStock++; else zero++; }
            lblTotal.Text = total.ToString(); lblWithStock.Text = withStock.ToString(); lblZero.Text = zero.ToString();
        }
    }
}
