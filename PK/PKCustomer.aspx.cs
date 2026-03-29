using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKApp.DAL;

namespace PKApp
{
    public partial class PKCustomer : Page
    {
        protected Label lblUser, lblAlert, lblCount, lblFormTitle;
        protected Panel pnlAlert, pnlEmpty;
        protected HiddenField hfCustID;
        protected DropDownList ddlCustomerType;
        protected TextBox txtCode, txtName, txtContact, txtPhone, txtEmail;
        protected TextBox txtGSTIN, txtCity, txtState, txtPinCode, txtAddress;
        protected Button btnSave, btnClear, btnToggle;
        protected Repeater rptList;
        protected int UserID => Convert.ToInt32(Session["PK_UserID"]);

        protected void Page_Load(object s, EventArgs e)
        {
            if (Session["PK_UserID"] == null) { Response.Redirect("PKLogin.aspx"); return; }
            if (lblUser != null) lblUser.Text = Session["PK_FullName"] as string ?? "";
            if (!IsPostBack) { LoadCustomerTypes(); BindList(); }
        }

        void LoadCustomerTypes()
        {
            if (ddlCustomerType == null) return;
            var dt = PKDatabaseHelper.GetCustomerTypes();
            ddlCustomerType.Items.Clear();
            ddlCustomerType.Items.Add(new ListItem("-- Select Type --", ""));
            foreach (DataRow r in dt.Rows)
                ddlCustomerType.Items.Add(new ListItem(r["TypeName"].ToString(), r["TypeCode"].ToString()));
        }

        void BindList()
        {
            var dt = PKDatabaseHelper.GetAllCustomers();
            if (pnlEmpty != null) pnlEmpty.Visible = dt.Rows.Count == 0;
            if (rptList != null) { rptList.DataSource = dt; rptList.DataBind(); }
            if (lblCount != null) lblCount.Text = dt.Rows.Count.ToString();
        }

        protected void btnSave_Click(object s, EventArgs e)
        {
            string customerType = ddlCustomerType != null ? ddlCustomerType.SelectedValue : "";
            if (string.IsNullOrEmpty(customerType))
            { ShowAlert("Please select a Customer Type.", false); return; }

            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name)) { ShowAlert("Name is required.", false); return; }

            int id = Convert.ToInt32(hfCustID.Value);
            string contact = txtContact.Text.Trim();
            string phone   = txtPhone.Text.Trim();
            string email   = txtEmail.Text.Trim();
            string address = txtAddress.Text.Trim();
            string city    = txtCity.Text.Trim();
            string state   = txtState.Text.Trim();
            string pinCode = txtPinCode != null ? txtPinCode.Text.Trim() : "";
            string gstin   = txtGSTIN.Text.Trim().ToUpper();

            try
            {
                if (id == 0)
                {
                    PKDatabaseHelper.AddCustomer(customerType, name, contact, phone,
                        email, address, city, state, pinCode, gstin);
                    ShowAlert("Customer '" + name + "' added.", true);
                }
                else
                {
                    PKDatabaseHelper.UpdateCustomer(id, txtCode.Text.Trim(), customerType,
                        name, contact, phone, email, address, city, state, pinCode, gstin);
                    ShowAlert("Customer updated.", true);
                }
                ClearForm(); BindList();
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        protected void btnClear_Click(object s, EventArgs e) { ClearForm(); if (pnlAlert != null) pnlAlert.Visible = false; }

        protected void btnToggle_Click(object s, EventArgs e)
        {
            int id = Convert.ToInt32(hfCustID.Value); if (id == 0) return;
            bool cur = btnToggle.Text == "Deactivate";
            PKDatabaseHelper.ToggleCustomer(id, !cur);
            ShowAlert("Customer " + (!cur ? "activated" : "deactivated") + ".", true);
            ClearForm(); BindList();
        }

        protected void rptList_Cmd(object src, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Edit") return;
            int id = Convert.ToInt32(e.CommandArgument);
            var row = PKDatabaseHelper.GetCustomerById(id);
            if (row == null) return;

            hfCustID.Value = id.ToString();
            txtCode.Text   = row["CustomerCode"].ToString();
            txtName.Text   = row["CustomerName"].ToString();
            txtContact.Text = row["ContactPerson"] == DBNull.Value ? "" : row["ContactPerson"].ToString();
            txtPhone.Text   = row["Phone"]    == DBNull.Value ? "" : row["Phone"].ToString();
            txtEmail.Text   = row["Email"]    == DBNull.Value ? "" : row["Email"].ToString();
            txtGSTIN.Text   = row["GSTIN"]    == DBNull.Value ? "" : row["GSTIN"].ToString();
            txtCity.Text    = row["City"]     == DBNull.Value ? "" : row["City"].ToString();
            txtState.Text   = row["State"]    == DBNull.Value ? "" : row["State"].ToString();
            txtAddress.Text = row["Address"]  == DBNull.Value ? "" : row["Address"].ToString();

            if (txtPinCode != null)
                txtPinCode.Text = row.Table.Columns.Contains("PinCode") && row["PinCode"] != DBNull.Value
                    ? row["PinCode"].ToString() : "";

            if (ddlCustomerType != null)
            {
                string custType = row.Table.Columns.Contains("CustomerType") && row["CustomerType"] != DBNull.Value
                    ? row["CustomerType"].ToString() : "";
                try { ddlCustomerType.SelectedValue = custType; } catch { ddlCustomerType.SelectedIndex = 0; }
            }

            lblFormTitle.Text = "Edit Customer";
            bool isActive = Convert.ToBoolean(row["IsActive"]);
            btnToggle.Text = isActive ? "Deactivate" : "Activate";
            btnToggle.Visible = true;
            if (pnlAlert != null) pnlAlert.Visible = false;
        }

        void ClearForm()
        {
            hfCustID.Value = "0";
            txtCode.Text = txtName.Text = txtContact.Text = "";
            txtPhone.Text = txtEmail.Text = txtGSTIN.Text = txtCity.Text = "";
            txtState.Text = txtAddress.Text = "";
            if (txtPinCode != null) txtPinCode.Text = "";
            if (ddlCustomerType != null) ddlCustomerType.SelectedIndex = 0;
            btnToggle.Visible = false;
            lblFormTitle.Text = "New Customer";
        }

        void ShowAlert(string m, bool ok)
        {
            if (lblAlert != null) lblAlert.Text = m;
            if (pnlAlert != null)
            {
                pnlAlert.CssClass = "alert " + (ok ? "alert-success" : "alert-danger");
                pnlAlert.Visible = true;
            }
        }
    }
}
