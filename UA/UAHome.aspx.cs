using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using UAApp.DAL;

namespace UAApp
{
    public partial class UAHome : Page
    {
        protected Label        lblNavUser, lblAlert, lblFormTitle, lblCount, lblRoleName;
        protected Panel        pnlAlert, pnlPassword, pnlUsers, pnlRoles, pnlNoRole, pnlRoleDetail;
        protected HiddenField  hfEditUserId, hfTab, hfSelectedRole;
        protected TextBox      txtFullName, txtUsername, txtPassword;
        protected DropDownList ddlRole;
        protected Button       btnSave, btnCancel, btnTabUsers, btnTabRoles, btnSelectRole, btnSaveRoleAccess;
        protected Repeater     rptUsers, rptRoleList, rptRoleApps;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UA_UserID"] == null) { Response.Redirect("UALogin.aspx"); return; }
            lblNavUser.Text = Session["UA_FullName"]?.ToString() ?? "";
            if (!IsPostBack) { LoadRoleDropdown(); BindUsers(); BindRoleList(); SetActiveTab(); }
        }

        private void LoadRoleDropdown()
        {
            ddlRole.Items.Clear();
            foreach (DataRow r in UADatabaseHelper.GetAllRoles().Rows)
                ddlRole.Items.Add(new ListItem(r["RoleName"].ToString(), r["RoleCode"].ToString()));
        }

        protected void btnTab_Click(object sender, EventArgs e)
        {
            hfTab.Value = ((Button)sender).CommandArgument;
            SetActiveTab();
            if (hfTab.Value == "users") { LoadRoleDropdown(); BindUsers(); }
            else { BindRoleList(); LoadSelectedRole(); }
            pnlAlert.Visible = false;
        }

        private void SetActiveTab()
        {
            bool isUsers = hfTab.Value != "roles";
            pnlUsers.Visible = isUsers; pnlRoles.Visible = !isUsers;
            btnTabUsers.CssClass = isUsers ? "tab-btn active" : "tab-btn";
            btnTabRoles.CssClass = !isUsers ? "tab-btn active" : "tab-btn";
        }

        private void BindUsers()
        {
            DataTable dt = UADatabaseHelper.GetAllUsers();
            rptUsers.DataSource = dt; rptUsers.DataBind();
            lblCount.Text = dt.Rows.Count.ToString();
        }

        protected string RenderAppBadges(string roleCode)
        {
            string apps = UADatabaseHelper.GetRoleAppsString(roleCode);
            if (string.IsNullOrEmpty(apps)) return "<span style='font-size:10px;color:#999;'>None</span>";
            if (apps == "ALL") return "<span class='badge-app' style='background:rgba(204,30,30,0.08);color:#cc1e1e;'>ALL</span>";
            var sb = new System.Text.StringBuilder();
            foreach (string a in apps.Split(','))
            { string t = a.Trim(); if (!string.IsNullOrEmpty(t)) sb.Append("<span class='badge-app'>" + t + "</span>"); }
            return sb.ToString();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string roleCode = ddlRole.SelectedValue;
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username))
            { ShowAlert("Full name and username are required.", false); BindUsers(); return; }

            int editId = 0; int.TryParse(hfEditUserId.Value, out editId);
            if (UADatabaseHelper.UsernameExists(username, editId))
            { ShowAlert("Username '" + username + "' already taken.", false); BindUsers(); return; }

            if (editId > 0)
            {
                UADatabaseHelper.UpdateUser(editId, fullName, username, roleCode);
                ShowAlert("User '" + fullName + "' updated.", true); ClearForm();
            }
            else
            {
                string password = txtPassword.Text.Trim();
                if (string.IsNullOrEmpty(password) || password.Length < 6)
                { ShowAlert("Password must be at least 6 characters.", false); BindUsers(); return; }
                UADatabaseHelper.CreateUser(fullName, username, UADatabaseHelper.HashPassword(password), roleCode);
                ShowAlert("User '" + fullName + "' created.", true); ClearForm();
            }
            BindUsers();
        }

        protected void UserAction_Command(object sender, CommandEventArgs e)
        {
            int userId = Convert.ToInt32(e.CommandArgument);
            switch (e.CommandName)
            {
                case "EditUser":
                    DataRow user = UADatabaseHelper.GetUserById(userId);
                    if (user == null) break;
                    hfEditUserId.Value = userId.ToString();
                    txtFullName.Text = user["FullName"].ToString();
                    txtUsername.Text = user["Username"].ToString();
                    LoadRoleDropdown();
                    ListItem li = ddlRole.Items.FindByValue(user["Role"].ToString());
                    if (li != null) ddlRole.SelectedValue = user["Role"].ToString();
                    lblFormTitle.Text = "Edit User — " + user["FullName"];
                    btnSave.Text = "Update User"; pnlPassword.Visible = false; btnCancel.Visible = true;
                    BindUsers(); return;
                case "Activate": case "Deactivate":
                    UADatabaseHelper.ToggleUserActive(userId);
                    ShowAlert("User status updated.", true); break;
                case "ResetPwd":
                    UADatabaseHelper.ResetPassword(userId, UADatabaseHelper.HashPassword("sirimiri123"));
                    ShowAlert("Password reset to 'sirimiri123'.", true); break;
            }
            BindUsers();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        { ClearForm(); BindUsers(); pnlAlert.Visible = false; }

        private void ClearForm()
        {
            hfEditUserId.Value = "0"; txtFullName.Text = ""; txtUsername.Text = ""; txtPassword.Text = "";
            LoadRoleDropdown(); lblFormTitle.Text = "Create New User"; btnSave.Text = "Create User";
            pnlPassword.Visible = true; btnCancel.Visible = false;
        }

        // ── ROLE ACCESS TAB ───────────────────────────────────────────────

        private void BindRoleList()
        { rptRoleList.DataSource = UADatabaseHelper.GetAllRoles(); rptRoleList.DataBind(); }

        protected string GetSelectedRole() { return hfSelectedRole.Value ?? ""; }

        protected void btnSelectRole_Click(object sender, EventArgs e)
        {
            string roleCode = Request["__EVENTARGUMENT"] ?? "";
            if (!string.IsNullOrEmpty(roleCode))
            { hfSelectedRole.Value = roleCode; BindRoleList(); LoadSelectedRole(); }
        }

        private void LoadSelectedRole()
        {
            string roleCode = hfSelectedRole.Value;
            if (string.IsNullOrEmpty(roleCode)) { pnlNoRole.Visible = true; pnlRoleDetail.Visible = false; return; }
            DataRow role = UADatabaseHelper.GetRoleByCode(roleCode);
            if (role == null) return;
            lblRoleName.Text = role["RoleName"].ToString();
            pnlNoRole.Visible = false; pnlRoleDetail.Visible = true;
            rptRoleApps.DataSource = UADatabaseHelper.GetRoleAppAccess(roleCode);
            rptRoleApps.DataBind();
        }

        protected DataTable GetRoleModules(string appCode)
        {
            string roleCode = hfSelectedRole.Value;
            if (string.IsNullOrEmpty(roleCode)) return new DataTable();
            return UADatabaseHelper.GetRoleModuleAccess(roleCode, appCode);
        }

        protected void btnSaveRoleAccess_Click(object sender, EventArgs e)
        {
            string roleCode = hfSelectedRole.Value;
            if (string.IsNullOrEmpty(roleCode)) return;
            UADatabaseHelper.ClearRoleAccess(roleCode);
            string[] appAccess = Request.Form.GetValues("role_app");
            if (appAccess != null)
            {
                foreach (string appCode in appAccess)
                {
                    UADatabaseHelper.SaveRoleAppAccess(roleCode, appCode, true);
                    string[] modAccess = Request.Form.GetValues("role_mod_" + appCode);
                    if (modAccess != null)
                        foreach (string modCode in modAccess)
                            UADatabaseHelper.SaveRoleModuleAccess(roleCode, appCode, modCode, true);
                }
            }
            ShowAlert("Access saved for role '" + roleCode + "'.", true);
            BindRoleList(); LoadSelectedRole();
        }

        private void ShowAlert(string msg, bool success)
        { pnlAlert.Visible = true; lblAlert.Text = msg; pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger"); }
    }
}
