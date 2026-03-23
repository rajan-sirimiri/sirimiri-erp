using System; using System.Data; using System.Web.UI; using System.Web.UI.WebControls; using PKApp.DAL;
namespace PKApp {
    public partial class PKSecondaryPacking : Page {
        protected Label lblUser, lblAlert, lblAvail;
        protected Panel pnlAlert, pnlEmpty, pnlTable;
        protected DropDownList ddlProduct, ddlCartonPM;
        protected System.Web.UI.HtmlControls.HtmlInputGenericControl txtCartons, txtUnitsPerCarton, txtCartonPMQty;
        protected System.Web.UI.HtmlControls.HtmlTextArea txtRemarks;
        protected Repeater rptLog;
        protected Button btnPack;
        protected int UserID => Convert.ToInt32(Session["PK_UserID"]);

        protected void Page_Load(object s, EventArgs e) {
            if (Session["PK_UserID"] == null) { Response.Redirect("PKLogin.aspx"); return; }
            lblUser.Text = Session["PK_FullName"] as string ?? "";
            if (!IsPostBack) { BindProductDropdown(); BindPMDropdown(); BindLog(); }
        }

        void BindProductDropdown() {
            var dt = PKDatabaseHelper.GetFGReadyForSecondary();
            ddlProduct.Items.Clear();
            ddlProduct.Items.Add(new ListItem("-- Select Product --", "0"));
            foreach (DataRow r in dt.Rows)
                ddlProduct.Items.Add(new ListItem(
                    r["ProductName"] + " (" + string.Format("{0:0.###}", r["AvailableQty"]) + " " + r["Unit"] + " available)",
                    r["ProductID"].ToString()));
        }

        void BindPMDropdown() {
            var dt = PKDatabaseHelper.GetActivePackingMaterials();
            ddlCartonPM.Items.Clear();
            ddlCartonPM.Items.Add(new ListItem("-- None --", "0"));
            foreach (DataRow r in dt.Rows)
                ddlCartonPM.Items.Add(new ListItem(r["PMName"] + " (" + r["Abbreviation"] + ")", r["PMID"].ToString()));
        }

        protected void ddlProduct_Changed(object s, EventArgs e) {
            int productId = Convert.ToInt32(ddlProduct.SelectedValue);
            if (productId == 0) { lblAvail.Text = "—"; return; }
            decimal avail = PKDatabaseHelper.GetFGAvailable(productId);
            lblAvail.Text = avail.ToString("0.###");
        }

        void BindLog() {
            var dt = PKDatabaseHelper.GetSecondaryPackingToday();
            pnlEmpty.Visible = dt.Rows.Count == 0;
            pnlTable.Visible = dt.Rows.Count > 0;
            rptLog.DataSource = dt; rptLog.DataBind();
        }

        protected void btnPack_Click(object s, EventArgs e) {
            int productId = Convert.ToInt32(ddlProduct.SelectedValue);
            if (productId == 0) { ShowAlert("Please select a product.", false); return; }
            decimal cartons, upc;
            if (!decimal.TryParse(txtCartons.Value, out cartons) || cartons <= 0) { ShowAlert("Enter valid carton count.", false); return; }
            if (!decimal.TryParse(txtUnitsPerCarton.Value, out upc) || upc <= 0) { ShowAlert("Enter valid units per carton.", false); return; }
            decimal totalUnits = cartons * upc;
            decimal avail = PKDatabaseHelper.GetFGAvailable(productId);
            if (totalUnits > avail) { ShowAlert("Total units (" + totalUnits.ToString("0.###") + ") exceeds FG available (" + avail.ToString("0.###") + ").", false); return; }
            int pmId; decimal pmQty;
            int.TryParse(ddlCartonPM.SelectedValue, out pmId);
            decimal.TryParse(txtCartonPMQty.Value, out pmQty);
            try {
                PKDatabaseHelper.AddSecondaryPacking(productId, cartons, (int)upc, pmId, pmQty, txtRemarks.Value, UserID);
                txtCartons.Value = ""; txtUnitsPerCarton.Value = ""; txtCartonPMQty.Value = ""; txtRemarks.Value = "";
                ShowAlert("Secondary packing recorded — " + totalUnits.ToString("0.###") + " units in " + cartons + " cartons.", true);
                BindProductDropdown(); BindLog();
            } catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        void ShowAlert(string m, bool ok) { lblAlert.Text = m; pnlAlert.CssClass = "alert " + (ok ? "alert-success" : "alert-danger"); pnlAlert.Visible = true; }
    }
}
