using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKApp.DAL;

namespace PKApp
{
    public partial class PKProductMRP : Page
    {
        protected Panel pnlAlert;
        protected Label lblAlert;
        protected Repeater rptProducts;
        protected Button btnSave;
        protected HiddenField hfMRPData;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PK_UserID"] == null) { Response.Redirect("PKLogin.aspx"); return; }

            string role = Session["PK_Role"]?.ToString() ?? "";
            if (!PKDatabaseHelper.RoleHasModuleAccess(role, "PK", "PK_MASTER"))
            { Response.Redirect("PKHome.aspx"); return; }

            if (!IsPostBack) LoadData();
        }

        private void LoadData()
        {
            var dt = PKDatabaseHelper.GetProductMRPList();
            rptProducts.DataSource = dt;
            rptProducts.DataBind();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string raw = hfMRPData != null ? hfMRPData.Value : "";
            if (string.IsNullOrEmpty(raw)) { ShowAlert("No MRP data to save.", false); return; }

            int saved = 0;
            string[] entries = raw.Split(',');
            foreach (string entry in entries)
            {
                string[] parts = entry.Split(':');
                if (parts.Length != 3) continue;

                int pid;
                if (!int.TryParse(parts[0], out pid) || pid <= 0) continue;
                string form = parts[1].Trim().ToUpper();
                if (form != "PCS" && form != "JAR" && form != "BOX" && form != "CASE") continue;

                decimal mrp;
                if (!decimal.TryParse(parts[2], out mrp)) continue;

                PKDatabaseHelper.SaveProductMRP(pid, form, mrp);
                saved++;
            }

            ShowAlert("Saved MRP for " + saved + " entries.", true);
            LoadData();
        }

        protected string GetJarBoxMRP(object mrpJar, object mrpBox, object containerType)
        {
            string ct = containerType != null && containerType != DBNull.Value ? containerType.ToString().ToUpper() : "JAR";
            object mrpVal = ct == "BOX" ? mrpBox : mrpJar;
            if (mrpVal == null || mrpVal == DBNull.Value) return "";
            decimal val = Convert.ToDecimal(mrpVal);
            return val > 0 ? val.ToString("0.##") : "";
        }

        private void ShowAlert(string msg, bool success)
        {
            pnlAlert.Visible = true;
            lblAlert.Text = msg;
            pnlAlert.CssClass = success ? "alert alert-success" : "alert alert-danger";
        }
    }
}
