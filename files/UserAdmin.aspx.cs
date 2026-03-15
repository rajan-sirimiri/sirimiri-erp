using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.IO;
using System.Text;
using OfficeOpenXml;
using StockApp.DAL;

namespace StockApp
{
    public partial class UserAdmin : Page
    {
        private Panel           PnlMsg    => (Panel)FindControl("pnlMsg");
        private Label           LblMsg    => (Label)FindControl("lblMsg");
        private Panel           PnlAddUser=> (Panel)FindControl("pnlAddUser");
        private HtmlGenericControl DivState => (HtmlGenericControl)FindControl("divState");
        private TextBox         TxtFullName=> (TextBox)FindControl("txtFullName");
        private TextBox         TxtUsername=> (TextBox)FindControl("txtUsername");
        private TextBox         TxtTempPwd => (TextBox)FindControl("txtTempPwd");
        private DropDownList    DdlRole   => (DropDownList)FindControl("ddlRole");
        private DropDownList    DdlState  => (DropDownList)FindControl("ddlState");
        private GridView        GvUsers   => (GridView)FindControl("gvUsers");

        public string MsgCssClass = "msg-ok";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null) Response.Redirect("~/Login.aspx");

            if (!IsPostBack)
            {
                BindStates();
                BindUsers();
                bool isAdmin = Session["Role"]?.ToString() == "Admin";
                DivState.Visible  = true;
                PnlAddUser.Visible   = isAdmin;
                pnlBulkUpload.Visible = isAdmin;
            }
        }

        private void BindStates()
        {
            DdlState.Items.Clear();
            DdlState.Items.Add(new ListItem("— Select State —", "0"));
            var dt = DatabaseHelper.GetStatesForAdmin();
            foreach (DataRow row in dt.Rows)
                DdlState.Items.Add(new ListItem(row["StateName"].ToString(), row["StateID"].ToString()));
        }

        private void BindUsers()
        {
            GvUsers.DataSource = DatabaseHelper.GetAllUsers();
            GvUsers.DataBind();
        }

        protected void ddlRole_Changed(object sender, EventArgs e)
        {
            DivState.Visible = DdlRole.SelectedValue == "FieldUser";
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            if (Session["Role"]?.ToString() != "Admin")
            { ShowMsg("Only Admins can create users.", false); return; }

            string fullName = TxtFullName.Text.Trim();
            string username = TxtUsername.Text.Trim();
            string tempPwd  = TxtTempPwd.Text;
            string role     = DdlRole.SelectedValue;
            int?   stateId  = null;

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(tempPwd))
            { ShowMsg("Please fill all required fields.", false); return; }

            if (tempPwd.Length < 8)
            { ShowMsg("Password must be at least 8 characters.", false); return; }

            if (role == "FieldUser")
            {
                if (!int.TryParse(DdlState.SelectedValue, out int sid) || sid == 0)
                { ShowMsg("Please select a state for Field User.", false); return; }
                stateId = sid;
            }

            bool ok = DatabaseHelper.CreateUser(fullName, username, Login.ComputeSHA256(tempPwd), role, stateId);
            if (ok)
            {
                ShowMsg($"User '{username}' created successfully.", true);
                TxtFullName.Text = TxtUsername.Text = TxtTempPwd.Text = "";
                BindUsers();
            }
            else
                ShowMsg("Username already exists. Please choose a different username.", false);
        }

        protected void gvUsers_RowDataBound(object sender, System.Web.UI.WebControls.GridViewRowEventArgs e)
        {
            if (e.Row.RowType == System.Web.UI.WebControls.DataControlRowType.DataRow)
            {
                bool isAdmin = Session["Role"]?.ToString() == "Admin";
                var btnToggle = e.Row.FindControl("btnToggle") as System.Web.UI.WebControls.LinkButton;
                if (btnToggle != null) btnToggle.Visible = isAdmin;
            }
        }

        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int  userId  = Convert.ToInt32(e.CommandArgument);
            bool isAdmin = Session["Role"]?.ToString() == "Admin";

            if (e.CommandName == "EditUser")
            {
                DataRow row = DatabaseHelper.GetUserById(userId);
                if (row == null) return;

                hfEditUserId.Value        = userId.ToString();
                txtEditFullName.Text      = row["FullName"].ToString();
                txtEditUsername.Text      = row["Username"].ToString();
                ddlEditRole.SelectedValue = row["Role"].ToString();

                // Load states for edit dropdown
                ddlEditState.Items.Clear();
                ddlEditState.Items.Add(new ListItem("— Select State —", "0"));
                var states = DatabaseHelper.GetStatesForAdmin();
                foreach (DataRow sr in states.Rows)
                    ddlEditState.Items.Add(new ListItem(sr["StateName"].ToString(), sr["StateID"].ToString()));

                if (row["StateID"] != DBNull.Value)
                    ddlEditState.SelectedValue = row["StateID"].ToString();

                // Load managers dropdown
                ddlEditManager.Items.Clear();
                ddlEditManager.Items.Add(new ListItem("— None —", "0"));
                var managers = DatabaseHelper.GetManagers();
                foreach (DataRow mr in managers.Rows)
                    if (Convert.ToInt32(mr["UserID"]) != userId) // exclude self
                        ddlEditManager.Items.Add(new ListItem(
                            mr["FullName"].ToString() + " (" + mr["Role"].ToString() + ")",
                            mr["UserID"].ToString()));

                if (row["ReportingManagerID"] != DBNull.Value)
                    ddlEditManager.SelectedValue = row["ReportingManagerID"].ToString();

                divEditState.Visible   = row["Role"].ToString() == "FieldUser";
                pnlEditUser.Visible    = true;
                return;
            }
            else if (e.CommandName == "ResetPwd")
            {
                DatabaseHelper.ResetPassword(userId, Login.ComputeSHA256("Temp@1234"));
                ShowMsg("Password reset to Temp@1234. User will be prompted to change on next login.", true);
            }
            else if (e.CommandName == "ToggleActive")
            {
                if (!isAdmin) { ShowMsg("Only Admins can activate or deactivate users.", false); return; }
                DatabaseHelper.ToggleUserActive(userId);
                ShowMsg("User status updated.", true);
            }
            BindUsers();
        }

        protected void ddlEditRole_Changed(object sender, EventArgs e)
        {
            divEditState.Visible = ddlEditRole.SelectedValue == "FieldUser";
        }

        protected void btnEditUserSave_Click(object sender, EventArgs e)
        {
            string fullName = txtEditFullName.Text.Trim();
            string username = txtEditUsername.Text.Trim();
            string role     = ddlEditRole.SelectedValue;

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username))
            { ShowMsg("Full Name and Username are required.", false); return; }

            int? stateId = null;
            if (role == "FieldUser")
            {
                if (!int.TryParse(ddlEditState.SelectedValue, out int sid) || sid == 0)
                { ShowMsg("Please select a state for Field User.", false); return; }
                stateId = sid;
            }

            int? managerId = null;
            if (int.TryParse(ddlEditManager.SelectedValue, out int mid) && mid > 0)
                managerId = mid;

            DatabaseHelper.UpdateUser(Convert.ToInt32(hfEditUserId.Value), fullName, username, role, stateId, managerId);
            pnlEditUser.Visible = false;
            ShowMsg($"User '{username}' updated successfully.", true);
            BindUsers();
        }

        protected void btnEditUserCancel_Click(object sender, EventArgs e)
        {
            pnlEditUser.Visible = false;
        }

        protected void btnDownloadTemplate_Click(object sender, EventArgs e)
        {
            // Build Excel template with EPPlus
            using (var pkg = new ExcelPackage())
            {
                var ws = pkg.Workbook.Worksheets.Add("Users");
                // Headers
                ws.Cells[1, 1].Value = "FullName";
                ws.Cells[1, 2].Value = "Username";
                ws.Cells[1, 3].Value = "TempPassword";
                ws.Cells[1, 4].Value = "Role";
                ws.Cells[1, 5].Value = "StateName";
                // Style headers
                using (var hdr = ws.Cells[1, 1, 1, 5])
                {
                    hdr.Style.Font.Bold = true;
                    hdr.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    hdr.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(41, 121, 201));
                    hdr.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }
                // Sample rows
                ws.Cells[2, 1].Value = "Rajan Kumar";
                ws.Cells[2, 2].Value = "rajan.kumar";
                ws.Cells[2, 3].Value = "Temp@1234";
                ws.Cells[2, 4].Value = "FieldUser";
                ws.Cells[2, 5].Value = "Tamil Nadu";
                ws.Cells[3, 1].Value = "Priya Sharma";
                ws.Cells[3, 2].Value = "priya.sharma";
                ws.Cells[3, 3].Value = "Temp@1234";
                ws.Cells[3, 4].Value = "Manager";
                ws.Cells[3, 5].Value = "";
                ws.Cells.AutoFitColumns();

                // Add notes row
                ws.Cells[5, 1].Value = "Notes:";
                ws.Cells[5, 1].Style.Font.Bold = true;
                ws.Cells[6, 1].Value = "Role must be: FieldUser, Manager, or Admin";
                ws.Cells[7, 1].Value = "StateName required only for FieldUser - must match exactly";
                ws.Cells[8, 1].Value = "TempPassword min 8 characters - user will be prompted to change on first login";

                byte[] fileBytes = pkg.GetAsByteArray();
                Response.Clear();
                Response.Buffer = true;
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=UserUploadTemplate.xlsx");
                Response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                Response.Flush();
                Response.SuppressContent = true;
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
        }

        protected void btnBulkUpload_Click(object sender, EventArgs e)
        {
            if (Session["Role"]?.ToString() != "Admin")
            { ShowMsg("Only Admins can upload users.", false); return; }

            if (!fileUsers.HasFile)
            { ShowMsg("Please select an Excel file to upload.", false); return; }

            if (!fileUsers.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            { ShowMsg("Only .xlsx files are supported.", false); return; }

            int created = 0, skipped = 0, errors = 0;
            var detail = new StringBuilder();

            // Load states for lookup
            var statesTable = DatabaseHelper.GetStatesForAdmin();
            var stateMap    = new System.Collections.Generic.Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow sr in statesTable.Rows)
                stateMap[sr["StateName"].ToString().Trim()] = Convert.ToInt32(sr["StateID"]);

            try
            {
                using (var stream = new MemoryStream(fileUsers.FileBytes))
                using (var pkg = new ExcelPackage(stream))
                {
                    var ws   = pkg.Workbook.Worksheets[1];
                    int rows = ws.Dimension?.Rows ?? 0;

                    for (int r = 2; r <= rows; r++)
                    {
                        string fullName = (ws.Cells[r, 1].Value ?? "").ToString().Trim();
                        string username = (ws.Cells[r, 2].Value ?? "").ToString().Trim();
                        string tempPwd  = (ws.Cells[r, 3].Value ?? "").ToString().Trim();
                        string role     = (ws.Cells[r, 4].Value ?? "").ToString().Trim();
                        string stateName= (ws.Cells[r, 5].Value ?? "").ToString().Trim();

                        if (string.IsNullOrEmpty(fullName) && string.IsNullOrEmpty(username)) continue;

                        // Validate
                        if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(tempPwd))
                        { detail.AppendLine($"Row {r}: Missing FullName, Username or Password — skipped."); errors++; continue; }

                        if (tempPwd.Length < 8)
                        { detail.AppendLine($"Row {r} ({username}): Password too short (min 8 chars) — skipped."); errors++; continue; }

                        if (role != "Admin" && role != "Manager" && role != "FieldUser")
                        { detail.AppendLine($"Row {r} ({username}): Invalid role '{role}' — skipped."); errors++; continue; }

                        int? stateId = null;
                        if (role == "FieldUser")
                        {
                            if (string.IsNullOrEmpty(stateName))
                            { detail.AppendLine($"Row {r} ({username}): FieldUser requires a StateName — skipped."); errors++; continue; }
                            if (!stateMap.TryGetValue(stateName, out int sid))
                            { detail.AppendLine($"Row {r} ({username}): State '{stateName}' not found — skipped."); errors++; continue; }
                            stateId = sid;
                        }

                        bool ok = DatabaseHelper.CreateUser(fullName, username, Login.ComputeSHA256(tempPwd), role, stateId);
                        if (ok) created++;
                        else  { skipped++; detail.AppendLine($"Row {r} ({username}): Username already exists — skipped."); }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMsg("Error reading file: " + ex.Message, false);
                return;
            }

            lblCreated.Text    = created.ToString();
            lblSkipped.Text    = skipped.ToString();
            lblBulkErrors.Text = errors.ToString();
            lblBulkDetail.Text = detail.ToString();
            pnlBulkResult.Visible = true;

            if (created > 0)
            {
                ShowMsg($"{created} user(s) created successfully.", true);
                BindUsers();
            }
            else
                ShowMsg("No new users were created.", false);
        }

        private void ShowMsg(string msg, bool ok)
        {
            MsgCssClass      = ok ? "msg-ok" : "msg-err";
            LblMsg.Text      = msg;
            PnlMsg.Visible   = true;
        }

        public string GetRoleBadge(string role)
        {
            switch (role)
            {
                case "Admin":   return "badge-admin";
                case "Manager": return "badge-manager";
                default:        return "badge-field";
            }
        }
    }
}
