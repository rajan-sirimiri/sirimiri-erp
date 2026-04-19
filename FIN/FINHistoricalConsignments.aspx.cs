using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using FINApp.DAL;

namespace FINApp
{
    public partial class FINHistoricalConsignments : Page
    {
        protected Label lblNavUser;
        protected Repeater rptHistorical;
        protected Panel pnlEmpty;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["FIN_UserID"] == null) { Response.Redirect("FINLogin.aspx"); return; }
            lblNavUser.Text = Session["FIN_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
            {
                var dt = FINDatabaseHelper.GetHistoricalConsignments();
                rptHistorical.DataSource = dt;
                rptHistorical.DataBind();
                pnlEmpty.Visible = dt.Rows.Count == 0;
            }
        }
    }
}
