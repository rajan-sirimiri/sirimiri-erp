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
            // --- Role gate: Super / Admin only ---
            string role = Session["UserRole"] as string;
            if (role != "Super" && role != "Admin")
            {
                Response.Redirect("/Login.aspx", true);
                return;
            }

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
            pnlMsg.CssClass = ok ? "msg msg-ok" : "msg msg-err";
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

            string user = (Session["UserName"] as string) ?? "SYSTEM";
            int id = int.Parse(hfDeptID.Value);

            try
            {
                if (id == 0)
                {
                    HR_DatabaseHelper.InsertDepartment(code, name, user);
                    ShowMsg("Department created.", true);
                }
                else
                {
                    HR_DatabaseHelper.UpdateDepartment(id, code, name, chkActive.Checked, user);
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
                        chkActive.Checked = Convert.ToInt32(r["IsActive"]) == 1;
                        litFormHeading.Text = "Edit Department";
                        break;
                    }
                }
            }
        }
    }
}
