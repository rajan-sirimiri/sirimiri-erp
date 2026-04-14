using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using StockApp.DAL;

namespace StockApp
{
    public partial class DailySales : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Login.aspx"); return; }

            string __role = Session["Role"]?.ToString() ?? "";
            if (!DatabaseHelper.RoleHasModuleAccess(__role, "SA", "SA_DAILY_SALES"))
            { Response.Redirect("SAHome.aspx"); return; }

            lblUserInfo.Text     = Session["FullName"] + " (" + Session["Role"] + ")";
            pnlAdminMenu.Visible = Session["Role"]?.ToString() == "Admin";

            if (!IsPostBack)
                LoadStates();
        }

        private void LoadStates()
        {
            ddlState.Items.Clear();
            ddlState.Items.Add(new ListItem("— Select State —", "0"));
            var dt = DatabaseHelper.GetStates(90);
            foreach (DataRow r in dt.Rows)
                ddlState.Items.Add(new ListItem(r["StateName"].ToString(), r["StateID"].ToString()));
        }

        protected void ddlState_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlCity.Items.Clear();
            lstDistributor.Items.Clear();
            ddlCity.Items.Add(new ListItem("— Select City —", "0"));

            string state = ddlState.SelectedValue;
            if (string.IsNullOrEmpty(state) || state == "0") return;

            var dt = DatabaseHelper.GetCitiesByState(state, 90);
            foreach (DataRow r in dt.Rows)
                ddlCity.Items.Add(new ListItem(r["CityName"].ToString(), r["CityID"].ToString()));
        }

        protected void ddlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstDistributor.Items.Clear();

            string city = ddlCity.SelectedValue;
            string state = ddlState.SelectedValue;
            if (string.IsNullOrEmpty(city) || city == "0") return;

            var dt = DatabaseHelper.GetDistributorsByCity(city, state, 90);
            foreach (DataRow r in dt.Rows)
                lstDistributor.Items.Add(new ListItem(
                    r["DistributorName"].ToString(), r["DistributorID"].ToString()));
        }
    }
}
