using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HRModule
{
    public partial class HREmployee : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // --- Role gate: Super / Admin only ---
            string role = Session["UserRole"] as string;
            if (role != "Super" && role != "Admin")
            {
                Response.Redirect("/Login.aspx", true);
                return;
            }

            if (!IsPostBack)
            {
                LoadDepartmentDropdowns();
                BindGrid();
            }
        }

        private void LoadDepartmentDropdowns()
        {
            DataTable dt = HR_DatabaseHelper.GetDepartments(true);

            ddlDept.Items.Clear();
            foreach (DataRow r in dt.Rows)
            {
                ddlDept.Items.Add(new ListItem(
                    r["DeptName"].ToString(),
                    r["DeptID"].ToString()));
            }

            ddlFilterDept.Items.Clear();
            ddlFilterDept.Items.Add(new ListItem("-- All Departments --", "0"));
            foreach (DataRow r in dt.Rows)
            {
                ddlFilterDept.Items.Add(new ListItem(
                    r["DeptName"].ToString(),
                    r["DeptID"].ToString()));
            }
        }

        private void BindGrid()
        {
            string q = (txtSearch.Text ?? "").Trim();
            int dept = 0;
            int.TryParse(ddlFilterDept.SelectedValue, out dept);
            gvEmployees.DataSource = HR_DatabaseHelper.GetEmployees(
                q, dept > 0 ? (int?)dept : null, chkActiveOnly.Checked);
            gvEmployees.DataBind();
        }

        private void ShowMsg(string text, bool ok)
        {
            pnlMsg.Visible = true;
            pnlMsg.CssClass = ok ? "msg msg-ok" : "msg msg-err";
            pnlMsg.Controls.Clear();
            pnlMsg.Controls.Add(new LiteralControl(Server.HtmlEncode(text)));
        }

        protected void btnSearch_Click(object sender, EventArgs e) { BindGrid(); }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            ResetForm();
            txtCode.Text = HR_DatabaseHelper.GenerateEmployeeCode();
            txtDOJ.Text = DateTime.Today.ToString("yyyy-MM-dd");
            litFormHeading.Text = "New Employee";
            pnlList.Visible = false;
            pnlEdit.Visible = true;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ResetForm();
            pnlList.Visible = true;
            pnlEdit.Visible = false;
            BindGrid();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                EmployeeRecord r = CollectForm();

                // --- Validations ---
                if (string.IsNullOrWhiteSpace(r.EmployeeCode)) { ShowMsg("Employee Code required.", false); return; }
                if (string.IsNullOrWhiteSpace(r.FullName))     { ShowMsg("Full Name required.", false); return; }
                if (r.DeptID <= 0)                             { ShowMsg("Department required.", false); return; }

                if (!HR_DatabaseHelper.IsValidAadhaar(r.AadhaarNo))
                { ShowMsg("Aadhaar must be 12 digits.", false); return; }
                if (!HR_DatabaseHelper.IsValidPAN(r.PANNo))
                { ShowMsg("PAN format invalid (expected ABCDE1234F).", false); return; }
                if (!HR_DatabaseHelper.IsValidIFSC(r.IFSCCode))
                { ShowMsg("IFSC format invalid (4 letters + 0 + 6 chars).", false); return; }

                int existingId = int.Parse(hfEmployeeID.Value);
                if (HR_DatabaseHelper.EmployeeCodeExists(r.EmployeeCode,
                    existingId > 0 ? (int?)existingId : null))
                { ShowMsg("Employee Code already exists.", false); return; }

                string user = (Session["UserName"] as string) ?? "SYSTEM";

                if (existingId == 0)
                {
                    HR_DatabaseHelper.InsertEmployee(r, user);
                    ShowMsg("Employee created: " + r.EmployeeCode, true);
                }
                else
                {
                    r.EmployeeID = existingId;
                    HR_DatabaseHelper.UpdateEmployee(r, user);
                    ShowMsg("Employee updated: " + r.EmployeeCode, true);
                }

                ResetForm();
                pnlList.Visible = true;
                pnlEdit.Visible = false;
                BindGrid();
            }
            catch (Exception ex)
            {
                ShowMsg("Error: " + ex.Message, false);
            }
        }

        protected void gvEmployees_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditEmp")
            {
                int id = Convert.ToInt32(e.CommandArgument);
                DataRow row = HR_DatabaseHelper.GetEmployeeById(id);
                if (row == null) { ShowMsg("Employee not found.", false); return; }
                LoadEmployeeIntoForm(row);
                pnlList.Visible = false;
                pnlEdit.Visible = true;
            }
        }

        // ---------------------------------------------------------------
        // Form helpers
        // ---------------------------------------------------------------
        private EmployeeRecord CollectForm()
        {
            decimal basic  = ParseDec(txtBasic.Text);
            decimal hra    = ParseDec(txtHRA.Text);
            decimal conv   = ParseDec(txtConv.Text);
            decimal other  = ParseDec(txtOther.Text);

            return new EmployeeRecord
            {
                EmployeeCode   = (txtCode.Text ?? "").Trim().ToUpperInvariant(),
                FullName       = (txtName.Text ?? "").Trim(),
                FatherName     = NullIfEmpty(txtFatherName.Text),
                Gender         = ddlGender.SelectedValue,
                DOB            = ParseDate(txtDOB.Text),
                DOJ            = ParseDate(txtDOJ.Text) ?? DateTime.Today,
                DOL            = ParseDate(txtDOL.Text),
                DeptID         = int.Parse(ddlDept.SelectedValue),
                Designation    = NullIfEmpty(txtDesignation.Text),
                EmploymentType = ddlEmpType.SelectedValue,
                MobileNo       = NullIfEmpty(txtMobile.Text),
                AltMobileNo    = NullIfEmpty(txtAltMobile.Text),
                Email          = NullIfEmpty(txtEmail.Text),
                AddressLine    = NullIfEmpty(txtAddress.Text),
                City           = NullIfEmpty(txtCity.Text),
                StateName      = NullIfEmpty(txtState.Text),
                Pincode        = NullIfEmpty(txtPincode.Text),
                AadhaarNo      = NullIfEmpty((txtAadhaar.Text ?? "").Replace(" ", "").Replace("-", "")),
                PANNo          = NullIfEmpty((txtPAN.Text ?? "").ToUpperInvariant()),
                UANNo          = NullIfEmpty(txtUAN.Text),
                PFNo           = NullIfEmpty(txtPF.Text),
                ESINo          = NullIfEmpty(txtESI.Text),
                BankAccountNo  = NullIfEmpty(txtBankAc.Text),
                BankName       = NullIfEmpty(txtBankName.Text),
                IFSCCode       = NullIfEmpty((txtIFSC.Text ?? "").ToUpperInvariant()),
                BasicSalary    = basic,
                HRA            = hra,
                ConveyanceAllow= conv,
                OtherAllow     = other,
                GrossSalary    = basic + hra + conv + other,
                IsActive       = chkActive.Checked
            };
        }

        private void LoadEmployeeIntoForm(DataRow r)
        {
            hfEmployeeID.Value = r["EmployeeID"].ToString();
            txtCode.Text = r["EmployeeCode"].ToString();
            txtName.Text = r["FullName"].ToString();
            txtFatherName.Text = NullToEmpty(r["FatherName"]);
            ddlGender.SelectedValue = NullToEmpty(r["Gender"], "M");
            txtDOB.Text = FormatDate(r["DOB"]);
            txtDOJ.Text = FormatDate(r["DOJ"]);
            txtDOL.Text = FormatDate(r["DOL"]);
            TrySetDropDown(ddlDept, r["DeptID"].ToString());
            txtDesignation.Text = NullToEmpty(r["Designation"]);
            TrySetDropDown(ddlEmpType, NullToEmpty(r["EmploymentType"], "Permanent"));
            txtMobile.Text = NullToEmpty(r["MobileNo"]);
            txtAltMobile.Text = NullToEmpty(r["AltMobileNo"]);
            txtEmail.Text = NullToEmpty(r["Email"]);
            txtAddress.Text = NullToEmpty(r["AddressLine"]);
            txtCity.Text = NullToEmpty(r["City"]);
            txtState.Text = NullToEmpty(r["StateName"]);
            txtPincode.Text = NullToEmpty(r["Pincode"]);
            txtAadhaar.Text = NullToEmpty(r["AadhaarNo"]);
            txtPAN.Text = NullToEmpty(r["PANNo"]);
            txtUAN.Text = NullToEmpty(r["UANNo"]);
            txtPF.Text = NullToEmpty(r["PFNo"]);
            txtESI.Text = NullToEmpty(r["ESINo"]);
            txtBankAc.Text = NullToEmpty(r["BankAccountNo"]);
            txtBankName.Text = NullToEmpty(r["BankName"]);
            txtIFSC.Text = NullToEmpty(r["IFSCCode"]);
            txtBasic.Text = Convert.ToDecimal(r["BasicSalary"]).ToString("0.##");
            txtHRA.Text   = Convert.ToDecimal(r["HRA"]).ToString("0.##");
            txtConv.Text  = Convert.ToDecimal(r["ConveyanceAllow"]).ToString("0.##");
            txtOther.Text = Convert.ToDecimal(r["OtherAllow"]).ToString("0.##");
            txtGross.Text = Convert.ToDecimal(r["GrossSalary"]).ToString("0.##");
            chkActive.Checked = Convert.ToInt32(r["IsActive"]) == 1;
            litFormHeading.Text = "Edit Employee — " + r["EmployeeCode"];
        }

        private void ResetForm()
        {
            hfEmployeeID.Value = "0";
            txtCode.Text = ""; txtName.Text = ""; txtFatherName.Text = "";
            ddlGender.SelectedValue = "M";
            txtDOB.Text = ""; txtDOJ.Text = ""; txtDOL.Text = "";
            if (ddlDept.Items.Count > 0) ddlDept.SelectedIndex = 0;
            txtDesignation.Text = "";
            ddlEmpType.SelectedValue = "Permanent";
            txtMobile.Text = ""; txtAltMobile.Text = ""; txtEmail.Text = "";
            txtAddress.Text = ""; txtCity.Text = ""; txtState.Text = ""; txtPincode.Text = "";
            txtAadhaar.Text = ""; txtPAN.Text = ""; txtUAN.Text = ""; txtPF.Text = ""; txtESI.Text = "";
            txtBankAc.Text = ""; txtBankName.Text = ""; txtIFSC.Text = "";
            txtBasic.Text = "0"; txtHRA.Text = "0"; txtConv.Text = "0"; txtOther.Text = "0"; txtGross.Text = "0";
            chkActive.Checked = true;
        }

        // ---------------------------------------------------------------
        // Small utilities
        // ---------------------------------------------------------------
        private static string NullIfEmpty(string s)
            => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        private static string NullToEmpty(object o, string defaultVal = "")
            => (o == null || o == DBNull.Value) ? defaultVal : o.ToString();

        private static DateTime? ParseDate(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            DateTime d;
            return DateTime.TryParse(s, out d) ? (DateTime?)d : null;
        }

        private static string FormatDate(object o)
        {
            if (o == null || o == DBNull.Value) return "";
            DateTime d = Convert.ToDateTime(o);
            return d == DateTime.MinValue ? "" : d.ToString("yyyy-MM-dd");
        }

        private static decimal ParseDec(string s)
        {
            decimal d;
            return decimal.TryParse(s, out d) ? d : 0m;
        }

        private static void TrySetDropDown(DropDownList ddl, string value)
        {
            ListItem li = ddl.Items.FindByValue(value);
            if (li != null) ddl.ClearSelection();
            if (li != null) li.Selected = true;
        }
    }
}
