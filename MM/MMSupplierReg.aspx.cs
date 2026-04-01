using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using MMApp.DAL;

namespace MMApp
{
    public partial class MMSupplierReg : Page
    {
        protected Label lblNavUser;
        protected Label lblFormTitle;
        protected Label lblAlert;
        protected Label lblCount;
        protected HiddenField hfSupplierID;
        protected Panel pnlAlert;
        protected Panel pnlEmpty;
        protected TextBox txtCode;
        protected TextBox txtName;
        protected TextBox txtContact;
        protected TextBox txtPhone;
        protected TextBox txtEmail;
        protected TextBox txtGST;
        protected TextBox txtPAN;
        protected TextBox txtAddress;
        protected TextBox txtCity;
        protected TextBox txtState;
        protected TextBox txtPinCode;
        protected Button btnSave;
        protected Button btnClear;
        protected Button btnToggleActive;
        protected Repeater rptSuppliers;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["MM_UserID"] == null) { Response.Redirect("MMLogin.aspx"); return; }

            // Module access check
            string __role = Session["MM_Role"]?.ToString() ?? "";
            if (!MMDatabaseHelper.RoleHasModuleAccess(__role, "MM", "MM_SUPPLIER"))
            { Response.Redirect("MMHome.aspx"); return; }
            lblNavUser.Text = Session["MM_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
            {
                LoadSuppliers();
                SetFormMode(false);
            }
        }

        private void LoadSuppliers()
        {
            DataTable dt = MMDatabaseHelper.GetAllSuppliers();
            if (dt.Rows.Count > 0)
            {
                rptSuppliers.DataSource = dt;
                rptSuppliers.DataBind();
                pnlEmpty.Visible = false;
                lblCount.Text = dt.Rows.Count + " record" + (dt.Rows.Count == 1 ? "" : "s");
            }
            else
            {
                rptSuppliers.DataSource = null;
                rptSuppliers.DataBind();
                pnlEmpty.Visible = true;
                lblCount.Text = "0 records";
            }
        }

        private void SetFormMode(bool isEdit)
        {
            lblFormTitle.Text = isEdit ? "Edit Supplier" : "New Supplier";
            btnToggleActive.Visible = isEdit;
        }

        private void ShowAlert(string message, bool success)
        {
            pnlAlert.Visible = true;
            lblAlert.Text = message;
            pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger");
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string code = txtCode.Text.Trim();
            string name = txtName.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                ShowAlert("Supplier Name is required.", false);
                return;
            }

            int supplierId = Convert.ToInt32(hfSupplierID.Value);

            try
            {
                if (supplierId == 0)
                {
                    MMDatabaseHelper.AddSupplier(name, txtContact.Text.Trim(),
                        txtPhone.Text.Trim(), txtEmail.Text.Trim(), txtGST.Text.Trim(),
                        txtPAN.Text.Trim(), txtAddress.Text.Trim(), txtCity.Text.Trim(),
                        txtState.Text.Trim(), txtPinCode.Text.Trim());
                    ShowAlert("Supplier '" + name + "' added successfully.", true);
                }
                else
                {
                    MMDatabaseHelper.UpdateSupplier(supplierId, code, name, txtContact.Text.Trim(),
                        txtPhone.Text.Trim(), txtEmail.Text.Trim(), txtGST.Text.Trim(),
                        txtPAN.Text.Trim(), txtAddress.Text.Trim(), txtCity.Text.Trim(),
                        txtState.Text.Trim(), txtPinCode.Text.Trim());
                    ShowAlert("Supplier '" + name + "' updated successfully.", true);
                }

                ClearForm();
                LoadSuppliers();
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message, false);
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            pnlAlert.Visible = false;
        }

        protected void btnToggleActive_Click(object sender, EventArgs e)
        {
            int supplierId = Convert.ToInt32(hfSupplierID.Value);
            if (supplierId == 0) return;

            DataRow dr = MMDatabaseHelper.GetSupplierById(supplierId);
            if (dr == null) return;

            bool currentlyActive = Convert.ToBoolean(dr["IsActive"]);
            MMDatabaseHelper.ToggleSupplierActive(supplierId, !currentlyActive);
            ShowAlert("Supplier " + (currentlyActive ? "deactivated" : "activated") + " successfully.", true);
            ClearForm();
            LoadSuppliers();
        }

        protected void rptSuppliers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Edit")
            {
                int supplierId = Convert.ToInt32(e.CommandArgument);
                DataRow dr = MMDatabaseHelper.GetSupplierById(supplierId);
                if (dr == null) return;

                hfSupplierID.Value  = supplierId.ToString();
                txtCode.Text        = dr["SupplierCode"].ToString();
                txtName.Text        = dr["SupplierName"].ToString();
                txtContact.Text     = dr["ContactPerson"].ToString();
                txtPhone.Text       = dr["Phone"].ToString();
                txtEmail.Text       = dr["Email"].ToString();
                txtGST.Text         = dr["GSTNo"].ToString();
                txtPAN.Text         = dr["PAN"].ToString();
                txtAddress.Text     = dr["Address"].ToString();
                txtCity.Text        = dr["City"].ToString();
                txtState.Text       = dr["State"].ToString();
                txtPinCode.Text     = dr["PinCode"].ToString();

                bool isActive = Convert.ToBoolean(dr["IsActive"]);
                btnToggleActive.Text = isActive ? "Deactivate" : "Activate";
                SetFormMode(true);
                pnlAlert.Visible = false;
            }
        }

        private void ClearForm()
        {
            hfSupplierID.Value = "0";
            txtCode.Text = txtName.Text = txtContact.Text = txtPhone.Text = "";
            txtEmail.Text = txtGST.Text = txtPAN.Text = txtAddress.Text = "";
            txtCity.Text = txtState.Text = txtPinCode.Text = "";
            SetFormMode(false);
        }
    }
}
