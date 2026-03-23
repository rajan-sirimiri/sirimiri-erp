using System; using System.Data; using System.Web.UI; using System.Web.UI.WebControls; using PKApp.DAL;
namespace PKApp {
    public partial class PKCustomer : Page {
        protected Label lblUser, lblAlert, lblCount, lblFormTitle;
        protected Panel pnlAlert, pnlEmpty;
        protected HiddenField hfCustID;
        protected TextBox txtCode, txtName, txtContact, txtPhone, txtEmail, txtGSTIN, txtCity, txtState, txtAddress;
        protected Button btnSave, btnClear, btnToggle;
        protected Repeater rptList;
        protected int UserID => Convert.ToInt32(Session["PK_UserID"]);
        protected void Page_Load(object s, EventArgs e) {
            if (Session["PK_UserID"] == null) { Response.Redirect("PKLogin.aspx"); return; }
            lblUser.Text = Session["PK_FullName"] as string ?? "";
            if (!IsPostBack) BindList();
        }
        void BindList() {
            var dt = PKDatabaseHelper.GetAllCustomers();
            pnlEmpty.Visible = dt.Rows.Count == 0;
            rptList.DataSource = dt; rptList.DataBind();
            lblCount.Text = dt.Rows.Count.ToString();
        }
        protected void btnSave_Click(object s, EventArgs e) {
            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name)) { ShowAlert("Customer name is required.", false); return; }
            int id = Convert.ToInt32(hfCustID.Value);
            if (id == 0) {
                PKDatabaseHelper.AddCustomer(name, txtContact.Text.Trim(), txtPhone.Text.Trim(),
                    txtEmail.Text.Trim(), txtAddress.Text.Trim(), txtCity.Text.Trim(),
                    txtState.Text.Trim(), txtGSTIN.Text.Trim());
                ShowAlert("Customer '" + name + "' added.", true);
            } else {
                PKDatabaseHelper.UpdateCustomer(id, txtCode.Text.Trim(), name,
                    txtContact.Text.Trim(), txtPhone.Text.Trim(), txtEmail.Text.Trim(),
                    txtAddress.Text.Trim(), txtCity.Text.Trim(), txtState.Text.Trim(), txtGSTIN.Text.Trim());
                ShowAlert("Customer updated.", true);
            }
            ClearForm(); BindList();
        }
        protected void btnClear_Click(object s, EventArgs e) { ClearForm(); pnlAlert.Visible = false; }
        protected void btnToggle_Click(object s, EventArgs e) {
            int id = Convert.ToInt32(hfCustID.Value); if (id == 0) return;
            bool cur = btnToggle.Text == "Deactivate";
            PKDatabaseHelper.ToggleCustomer(id, !cur);
            ShowAlert("Customer " + (!cur ? "activated" : "deactivated") + ".", true);
            ClearForm(); BindList();
        }
        protected void rptList_Cmd(object src, RepeaterCommandEventArgs e) {
            if (e.CommandName != "Edit") return;
            int id = Convert.ToInt32(e.CommandArgument);
            var row = PKDatabaseHelper.GetCustomerById(id); if (row == null) return;
            hfCustID.Value    = id.ToString();
            txtCode.Text      = row["CustomerCode"].ToString();
            txtName.Text      = row["CustomerName"].ToString();
            txtContact.Text   = row["ContactPerson"] == DBNull.Value ? "" : row["ContactPerson"].ToString();
            txtPhone.Text     = row["Phone"]    == DBNull.Value ? "" : row["Phone"].ToString();
            txtEmail.Text     = row["Email"]    == DBNull.Value ? "" : row["Email"].ToString();
            txtGSTIN.Text     = row["GSTIN"]    == DBNull.Value ? "" : row["GSTIN"].ToString();
            txtCity.Text      = row["City"]     == DBNull.Value ? "" : row["City"].ToString();
            txtState.Text     = row["State"]    == DBNull.Value ? "" : row["State"].ToString();
            txtAddress.Text   = row["Address"]  == DBNull.Value ? "" : row["Address"].ToString();
            lblFormTitle.Text = "Edit Customer";
            btnToggle.Text    = Convert.ToBoolean(row["IsActive"]) ? "Deactivate" : "Activate";
            btnToggle.Visible = true;
            pnlAlert.Visible  = false;
        }
        void ClearForm() {
            hfCustID.Value = "0"; txtCode.Text = txtName.Text = txtContact.Text = "";
            txtPhone.Text = txtEmail.Text = txtGSTIN.Text = txtCity.Text = "";
            txtState.Text = txtAddress.Text = ""; btnToggle.Visible = false;
            lblFormTitle.Text = "New Customer";
        }
        void ShowAlert(string m, bool ok) { lblAlert.Text = m; pnlAlert.CssClass = "alert " + (ok ? "alert-success" : "alert-danger"); pnlAlert.Visible = true; }
    }
}
