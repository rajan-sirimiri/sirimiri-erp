using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HRModule
{
    public partial class HRDepartment : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // --- Auth gate ---
            if (Session["HR_UserID"] == null && Session["UserID"] == null)
            {
                Response.Redirect("HRLogin.aspx", true);
                return;
            }

            // --- Role gate: Super / Admin only ---
            string role = (Session["HR_Role"] as string) ?? (Session["UserRole"] as string) ?? (Session["Role"] as string);
            if (role != "Super" && role != "Admin")
            {
                Response.Redirect("HRLogin.aspx", true);
                return;
            }

            // Show user name in top nav (only if the new control exists in markup)
            string navName = (Session["HR_FullName"] as string) ?? (Session["FullName"] as string)
                          ?? (Session["UserName"] as string) ?? "";
            if (!string.IsNullOrEmpty(navName) && lblNavUser != null) lblNavUser.Text = navName;

            if (!IsPostBack)
            {
                BindGrid();
            }
        }

        private void BindGrid()
        {
            gvDepts.DataSource = HR_DatabaseHelper.GetDepartments(false);
            gvDepts.DataBind();
        }

        private void ShowMsg(string text, bool ok)
        {
            pnlMsg.Visible = true;
            pnlMsg.CssClass = ok ? "banner banner-success" : "banner banner-error";
            pnlMsg.Controls.Clear();
            pnlMsg.Controls.Add(new LiteralControl(Server.HtmlEncode(text)));
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string name = (txtName.Text ?? "").Trim();
            if (name.Length == 0)
            {
                ShowMsg("Department name is required.", false);
                return;
            }

            string code = (txtCode.Text ?? "").Trim();
            if (code.Length == 0) code = HR_DatabaseHelper.GenerateDeptCode(name);

            string codePrefix = (txtCodePrefix.Text ?? "").Trim().ToUpperInvariant();
            // Optional but if provided, must be 1-10 chars, alphanumeric only
            if (!string.IsNullOrEmpty(codePrefix))
            {
                if (codePrefix.Length > 10) { ShowMsg("Code Prefix must be 10 characters or fewer.", false); return; }
                foreach (char c in codePrefix)
                {
                    if (!char.IsLetterOrDigit(c)) { ShowMsg("Code Prefix can only contain letters and digits.", false); return; }
                }
            }

            string user = (Session["UserName"] as string) ?? "SYSTEM";
            int id = int.Parse(hfDeptID.Value);

            try
            {
                if (id == 0)
                {
                    HR_DatabaseHelper.InsertDepartment(code, name,
                        string.IsNullOrEmpty(codePrefix) ? null : codePrefix,
                        user);
                    ShowMsg("Department created.", true);
                }
                else
                {
                    // updatePrefix=true so blank explicitly clears it
                    HR_DatabaseHelper.UpdateDepartment(id, code, name,
                        string.IsNullOrEmpty(codePrefix) ? null : codePrefix,
                        chkActive.Checked, user, /*updatePrefix*/ true);
                    ShowMsg("Department updated.", true);
                }
                ResetForm();
                BindGrid();
            }
            catch (Exception ex)
            {
                ShowMsg("Error: " + ex.Message, false);
            }
        }

        protected void btnClear_Click(object sender, EventArgs e) { ResetForm(); }

        private void ResetForm()
        {
            hfDeptID.Value = "0";
            txtCode.Text = "";
            txtName.Text = "";
            txtCodePrefix.Text = "";
            chkActive.Checked = true;
            litFormHeading.Text = "Add Department";
        }

        protected void gvDepts_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditDept")
            {
                int id = Convert.ToInt32(e.CommandArgument);
                DataTable dt = HR_DatabaseHelper.GetDepartments(false);
                foreach (DataRow r in dt.Rows)
                {
                    if (Convert.ToInt32(r["DeptID"]) == id)
                    {
                        hfDeptID.Value = id.ToString();
                        txtCode.Text = r["DeptCode"].ToString();
                        txtName.Text = r["DeptName"].ToString();
                        txtCodePrefix.Text = (r.Table.Columns.Contains("CodePrefix") && r["CodePrefix"] != DBNull.Value)
                            ? r["CodePrefix"].ToString() : "";
                        chkActive.Checked = Convert.ToInt32(r["IsActive"]) == 1;
                        litFormHeading.Text = "Edit Department";
                        break;
                    }
                }
            }
        }
    }
}
