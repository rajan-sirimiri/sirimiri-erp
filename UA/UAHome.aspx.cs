using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using UAApp.DAL;

namespace UAApp
{
    public partial class UAHome : Page
    {
        protected Label        lblNavUser, lblNavRole, lblAlert, lblFormTitle, lblCount, lblRoleName;
        protected Panel        pnlAlert, pnlPassword, pnlUsers, pnlRoles, pnlNoRole, pnlRoleDetail, pnlOrg;
        protected HiddenField  hfEditUserId, hfTab, hfSelectedRole, hfRoleClick, hfEditPosId;
        protected TextBox      txtFullName, txtUsername, txtPassword;
        protected TextBox      txtZoneName, txtRegionName, txtPosName, txtPosEmpId, txtAreaName;
        protected DropDownList ddlRole;
        protected DropDownList ddlRegionZone, ddlAreaRegion, ddlPosDesig, ddlPosUser, ddlPosZone, ddlPosRegion, ddlPosReportsTo;
        protected Button       btnSave, btnCancel, btnTabUsers, btnTabRoles, btnTabOrg, btnSelectRole, btnSaveRoleAccess;
        protected Button       btnAddZone, btnAddRegion, btnAddArea, btnSavePos;
        protected Repeater     rptUsers, rptRoleList, rptRoleApps, rptZones, rptRegions, rptAreas, rptPositions;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UA_UserID"] == null) { Response.Redirect("UALogin.aspx"); return; }
            lblNavUser.Text = Session["UA_FullName"]?.ToString() ?? "";
            if (lblNavRole != null) lblNavRole.Text = Session["UA_Role"]?.ToString() ?? "";
            if (!IsPostBack) { LoadRoleDropdown(); BindUsers(); BindRoleList(); LoadOrgDropdowns(); SetActiveTab(); }
            else
            {
                SetActiveTab();
                string tab = hfTab.Value ?? "users";
                if (tab == "org") { LoadOrgDropdowns(); }
            }
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
            else if (hfTab.Value == "roles") { BindRoleList(); LoadSelectedRole(); }
            else if (hfTab.Value == "org") { LoadOrgDropdowns(); BindOrgData(); }
            pnlAlert.Visible = false;
        }

        private void SetActiveTab()
        {
            string tab = hfTab.Value ?? "users";
            pnlUsers.Visible = tab == "users";
            pnlRoles.Visible = tab == "roles";
            if (pnlOrg != null) pnlOrg.Visible = tab == "org";
            btnTabUsers.CssClass = tab == "users" ? "tab-btn active" : "tab-btn";
            btnTabRoles.CssClass = tab == "roles" ? "tab-btn active" : "tab-btn";
            if (btnTabOrg != null) btnTabOrg.CssClass = tab == "org" ? "tab-btn active" : "tab-btn";
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

        protected void btnSelectRole_Click(object sender, EventArgs e)
        {
            string roleCode = hfRoleClick.Value;
            if (!string.IsNullOrEmpty(roleCode))
            {
                hfSelectedRole.Value = roleCode;
                BindRoleList();
                LoadSelectedRole();
            }
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

        // ── ORG STRUCTURE ─────────────────────────────────────────────────

        private void LoadOrgDropdowns()
        {
            // Zone dropdowns
            DataTable zones = UADatabaseHelper.GetAllZones();
            BindZoneDropdown(ddlRegionZone, zones, true);
            BindZoneDropdown(ddlPosZone, zones, true);

            // Designations
            if (ddlPosDesig != null)
            {
                ddlPosDesig.Items.Clear();
                ddlPosDesig.Items.Add(new ListItem("-- Select --", "0"));
                foreach (DataRow r in UADatabaseHelper.GetAllDesignations().Rows)
                    ddlPosDesig.Items.Add(new ListItem(r["DesignName"].ToString(), r["DesignationID"].ToString()));
            }

            // Users
            if (ddlPosUser != null)
            {
                ddlPosUser.Items.Clear();
                ddlPosUser.Items.Add(new ListItem("-- None (Vacant) --", "0"));
                foreach (DataRow r in UADatabaseHelper.GetAllUsers().Rows)
                    ddlPosUser.Items.Add(new ListItem(r["FullName"].ToString() + " (" + r["Username"] + ")", r["UserID"].ToString()));
            }

            // Regions (all) — for position form and area form
            if (ddlPosRegion != null)
            {
                ddlPosRegion.Items.Clear();
                ddlPosRegion.Items.Add(new ListItem("-- None --", "0"));
                foreach (DataRow r in UADatabaseHelper.GetAllRegions().Rows)
                    ddlPosRegion.Items.Add(new ListItem(r["RegionName"].ToString() + " (" + r["ZoneName"] + ")", r["RegionID"].ToString()));
            }

            // Area region dropdown
            if (ddlAreaRegion != null)
            {
                ddlAreaRegion.Items.Clear();
                ddlAreaRegion.Items.Add(new ListItem("-- Select Region --", "0"));
                foreach (DataRow r in UADatabaseHelper.GetAllRegions().Rows)
                    ddlAreaRegion.Items.Add(new ListItem(r["RegionName"].ToString() + " (" + r["ZoneName"] + ")", r["RegionID"].ToString()));
            }

            // Reports To (all positions)
            if (ddlPosReportsTo != null)
            {
                ddlPosReportsTo.Items.Clear();
                ddlPosReportsTo.Items.Add(new ListItem("-- None --", "0"));
                foreach (DataRow r in UADatabaseHelper.GetAllOrgPositions().Rows)
                    ddlPosReportsTo.Items.Add(new ListItem(r["EmployeeName"]?.ToString() + " (" + r["DesignName"] + ")", r["PositionID"].ToString()));
            }

            BindOrgData();
        }

        private void BindZoneDropdown(DropDownList ddl, DataTable zones, bool addEmpty)
        {
            if (ddl == null) return;
            ddl.Items.Clear();
            if (addEmpty) ddl.Items.Add(new ListItem("-- Select Zone --", "0"));
            foreach (DataRow r in zones.Rows)
                ddl.Items.Add(new ListItem(r["ZoneName"].ToString(), r["ZoneID"].ToString()));
        }

        private void BindOrgData()
        {
            if (rptZones != null) { rptZones.DataSource = UADatabaseHelper.GetAllZones(); rptZones.DataBind(); }
            if (rptRegions != null) { rptRegions.DataSource = UADatabaseHelper.GetAllRegions(); rptRegions.DataBind(); }
            if (rptAreas != null) { rptAreas.DataSource = UADatabaseHelper.GetAllAreas(); rptAreas.DataBind(); }
            if (rptPositions != null) { rptPositions.DataSource = UADatabaseHelper.GetAllOrgPositions(); rptPositions.DataBind(); }
        }

        protected void btnAddZone_Click(object sender, EventArgs e)
        {
            string name = txtZoneName != null ? txtZoneName.Text.Trim() : "";
            if (string.IsNullOrEmpty(name)) { ShowAlert("Enter Zone name.", false); return; }
            try
            {
                UADatabaseHelper.SaveZone(0, name);
                txtZoneName.Text = "";
                ShowAlert("Zone '" + name + "' added.", true);
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message, false);
            }
            LoadOrgDropdowns();
        }

        protected void btnAddRegion_Click(object sender, EventArgs e)
        {
            int zoneId = ddlRegionZone != null ? Convert.ToInt32(ddlRegionZone.SelectedValue) : 0;
            string name = txtRegionName != null ? txtRegionName.Text.Trim() : "";
            if (zoneId == 0 || string.IsNullOrEmpty(name)) { ShowAlert("Select Zone and enter Region name.", false); return; }
            try
            {
                UADatabaseHelper.SaveRegion(0, zoneId, name);
                txtRegionName.Text = "";
                ShowAlert("Region '" + name + "' added.", true);
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message, false);
            }
            LoadOrgDropdowns();
        }

        protected void btnAddArea_Click(object sender, EventArgs e)
        {
            int regionId = ddlAreaRegion != null ? Convert.ToInt32(ddlAreaRegion.SelectedValue) : 0;
            string name = txtAreaName != null ? txtAreaName.Text.Trim() : "";
            if (regionId == 0 || string.IsNullOrEmpty(name)) { ShowAlert("Select Region and enter Area name.", false); return; }
            try
            {
                UADatabaseHelper.SaveArea(0, regionId, name);
                txtAreaName.Text = "";
                ShowAlert("Area '" + name + "' added.", true);
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message, false);
            }
            LoadOrgDropdowns();
        }

        protected void ddlPosZone_Changed(object sender, EventArgs e)
        {
            if (ddlPosRegion == null || ddlPosZone == null) return;
            int zoneId = Convert.ToInt32(ddlPosZone.SelectedValue);
            ddlPosRegion.Items.Clear();
            ddlPosRegion.Items.Add(new ListItem("-- None --", "0"));
            if (zoneId > 0)
            {
                foreach (DataRow r in UADatabaseHelper.GetRegionsByZone(zoneId).Rows)
                    ddlPosRegion.Items.Add(new ListItem(r["RegionName"].ToString(), r["RegionID"].ToString()));
            }
        }

        protected void btnSavePos_Click(object sender, EventArgs e)
        {
            int designId = ddlPosDesig != null ? Convert.ToInt32(ddlPosDesig.SelectedValue) : 0;
            if (designId == 0) { ShowAlert("Select a designation.", false); return; }

            string empName = txtPosName != null ? txtPosName.Text.Trim() : "";
            string empId = txtPosEmpId != null ? txtPosEmpId.Text.Trim() : "";
            int? userId = ddlPosUser != null && ddlPosUser.SelectedValue != "0" ? (int?)Convert.ToInt32(ddlPosUser.SelectedValue) : null;
            int? zoneId = ddlPosZone != null && ddlPosZone.SelectedValue != "0" ? (int?)Convert.ToInt32(ddlPosZone.SelectedValue) : null;
            int? regionId = ddlPosRegion != null && ddlPosRegion.SelectedValue != "0" ? (int?)Convert.ToInt32(ddlPosRegion.SelectedValue) : null;
            int? reportsTo = ddlPosReportsTo != null && ddlPosReportsTo.SelectedValue != "0" ? (int?)Convert.ToInt32(ddlPosReportsTo.SelectedValue) : null;

            int posId = hfEditPosId != null ? Convert.ToInt32(hfEditPosId.Value) : 0;
            UADatabaseHelper.SaveOrgPosition(posId, userId, designId, empId, empName, zoneId, regionId, reportsTo);

            if (txtPosName != null) txtPosName.Text = "";
            if (txtPosEmpId != null) txtPosEmpId.Text = "";
            if (hfEditPosId != null) hfEditPosId.Value = "0";
            ShowAlert(posId == 0 ? "Position created." : "Position updated.", true);
            LoadOrgDropdowns();
        }

        protected void OrgAction_Command(object sender, CommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            switch (e.CommandName)
            {
                case "DelZone": UADatabaseHelper.ToggleZoneActive(id); ShowAlert("Zone removed.", true); break;
                case "DelRegion":
                    UADatabaseHelper.ExecuteNonQueryDirect("UPDATE SA_Regions SET IsActive=0 WHERE RegionID=?id;",
                        new MySql.Data.MySqlClient.MySqlParameter("?id", id));
                    ShowAlert("Region removed.", true); break;
                case "DelArea":
                    UADatabaseHelper.ExecuteNonQueryDirect("UPDATE SA_Areas SET IsActive=0 WHERE AreaID=?id;",
                        new MySql.Data.MySqlClient.MySqlParameter("?id", id));
                    ShowAlert("Area removed.", true); break;
                case "EditPos":
                    DataRow pos = UADatabaseHelper.GetOrgPositionById(id);
                    if (pos != null)
                    {
                        hfEditPosId.Value = id.ToString();
                        if (ddlPosDesig != null) { var li = ddlPosDesig.Items.FindByValue(pos["DesignationID"].ToString()); if (li != null) ddlPosDesig.SelectedValue = pos["DesignationID"].ToString(); }
                        if (txtPosName != null) txtPosName.Text = pos["EmployeeName"]?.ToString() ?? "";
                        if (txtPosEmpId != null) txtPosEmpId.Text = pos["EmployeeID"]?.ToString() ?? "";
                        if (ddlPosUser != null && pos["UserID"] != DBNull.Value) { var li = ddlPosUser.Items.FindByValue(pos["UserID"].ToString()); if (li != null) ddlPosUser.SelectedValue = pos["UserID"].ToString(); }
                        if (ddlPosZone != null && pos["ZoneID"] != DBNull.Value) { var li = ddlPosZone.Items.FindByValue(pos["ZoneID"].ToString()); if (li != null) ddlPosZone.SelectedValue = pos["ZoneID"].ToString(); }
                        if (ddlPosRegion != null && pos["RegionID"] != DBNull.Value) { var li = ddlPosRegion.Items.FindByValue(pos["RegionID"].ToString()); if (li != null) ddlPosRegion.SelectedValue = pos["RegionID"].ToString(); }
                        if (ddlPosReportsTo != null && pos["ReportsToID"] != DBNull.Value) { var li = ddlPosReportsTo.Items.FindByValue(pos["ReportsToID"].ToString()); if (li != null) ddlPosReportsTo.SelectedValue = pos["ReportsToID"].ToString(); }
                    }
                    break;
                case "DelPos": UADatabaseHelper.TogglePositionActive(id); ShowAlert("Position removed.", true); break;
            }
            LoadOrgDropdowns();
        }

        protected string GetDesigBadgeClass(object hierarchyLevel)
        {
            int lvl = Convert.ToInt32(hierarchyLevel);
            switch (lvl)
            {
                case 1: return "badge-md";
                case 2: return "badge-zsm";
                case 3: return "badge-rsm";
                case 4: return "badge-asm";
                case 5: return "badge-so";
                default: return "";
            }
        }

        private void ShowAlert(string msg, bool success)
        { pnlAlert.Visible = true; lblAlert.Text = msg; pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger"); }
    }
}
