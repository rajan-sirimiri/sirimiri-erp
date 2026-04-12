using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKApp.DAL;

namespace PKApp
{
    public partial class PKMachineMaster : Page
    {
        protected Label lblNavUser, lblAlert, lblCount, lblFormTitle;
        protected Panel pnlAlert, pnlEmpty, pnlTable;
        protected TextBox txtMachineCode, txtMachineName, txtLocation;
        protected Button btnSave, btnCancel;
        protected Repeater rptMachines;
        protected HiddenField hfEditId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PK_UserID"] == null) { Response.Redirect("PKLogin.aspx"); return; }

            // Only Super Admin can manage machines
            string role = Session["PK_Role"]?.ToString() ?? "";
            if (role != "Super Admin")
            { Response.Redirect("PKHome.aspx"); return; }

            lblNavUser.Text = Session["PK_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
                BindMachines();
        }

        void BindMachines()
        {
            DataTable dt = PKDatabaseHelper.GetAllMachines();
            int count = dt != null ? dt.Rows.Count : 0;
            lblCount.Text = count.ToString();
            pnlEmpty.Visible = count == 0;
            pnlTable.Visible = count > 0;
            if (count > 0)
            {
                rptMachines.DataSource = dt;
                rptMachines.DataBind();
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string code = txtMachineCode.Text.Trim().ToUpper();
            string name = txtMachineName.Text.Trim();
            string location = txtLocation.Text.Trim();

            if (string.IsNullOrEmpty(code))
            { ShowAlert("Machine Code is required.", false); return; }
            if (string.IsNullOrEmpty(name))
            { ShowAlert("Machine Name is required.", false); return; }

            int editId = Convert.ToInt32(hfEditId.Value);

            try
            {
                if (editId > 0)
                {
                    PKDatabaseHelper.UpdateMachine(editId, code, name, location);
                    ShowAlert("Machine updated successfully.", true);
                    ResetForm();
                }
                else
                {
                    // Check duplicate code
                    if (PKDatabaseHelper.MachineCodeExists(code))
                    { ShowAlert("Machine Code '" + code + "' already exists.", false); return; }

                    PKDatabaseHelper.AddMachine(code, name, location);
                    ShowAlert("Machine '" + code + "' added successfully.", true);
                    txtMachineCode.Text = "";
                    txtMachineName.Text = "";
                    txtLocation.Text = "";
                }
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }

            BindMachines();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ResetForm();
            BindMachines();
        }

        protected void rptMachines_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int machineId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "ToggleActive")
            {
                PKDatabaseHelper.ToggleMachineActive(machineId);
                ShowAlert("Machine status updated.", true);
                BindMachines();
            }
            else if (e.CommandName == "EditMachine")
            {
                DataRow machine = PKDatabaseHelper.GetMachineById(machineId);
                if (machine != null)
                {
                    hfEditId.Value = machineId.ToString();
                    txtMachineCode.Text = machine["MachineCode"].ToString();
                    txtMachineName.Text = machine["MachineName"].ToString();
                    txtLocation.Text = machine["Location"] != DBNull.Value ? machine["Location"].ToString() : "";
                    lblFormTitle.Text = "Edit Machine — " + machine["MachineCode"];
                    btnSave.Text = "Update Machine";
                    btnCancel.Visible = true;
                }
                BindMachines();
            }
        }

        void ResetForm()
        {
            hfEditId.Value = "0";
            txtMachineCode.Text = "";
            txtMachineName.Text = "";
            txtLocation.Text = "";
            lblFormTitle.Text = "Add New Machine";
            btnSave.Text = "+ Add Machine";
            btnCancel.Visible = false;
        }

        void ShowAlert(string msg, bool success)
        {
            string bg = success ? "#d1f5e0" : "#fdf3f2";
            string color = success ? "#155724" : "#842029";
            string border = success ? "#a3d9b1" : "#f5c2c7";
            string icon = success ? "&#10003;" : "&#9888;";
            lblAlert.Text = string.Format(
                "<div style='background:{0};color:{1};border:1px solid {2};padding:12px 18px;border-radius:8px;font-size:13px;'>" +
                "<strong>{3}</strong> {4}</div>", bg, color, border, icon, msg);
            pnlAlert.Visible = true;
        }
    }
}
