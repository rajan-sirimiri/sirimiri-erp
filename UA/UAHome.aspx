<%@ Page Language="C#" AutoEventWireup="true" EnableEventValidation="false" Inherits="UAApp.UAHome" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="UTF-8"/>
<meta name="viewport" content="width=device-width, initial-scale=1.0"/>
<title>User Administration — Sirimiri ERP</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:ital,wght@0,300;0,400;0,500;0,600;1,300&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--bg:#f5f5f5;--surface:#fff;--surface2:#f9f9f9;--border:#e0e0e0;--accent:#cc1e1e;--accent-dark:#a81818;--teal:#1a9e6a;--gold:#b8860b;--orange:#e07b00;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--radius:12px;}
*,*::before,*::after{box-sizing:border-box;margin:0;padding:0;}
body{background:var(--bg);color:var(--text);font-family:'DM Sans',sans-serif;min-height:100vh;}

/* Nav — MM style gradient */
nav.ua-nav{position:relative;z-index:9;background:linear-gradient(135deg,#1a1a1a 0%,#cc1e1e 100%);display:flex;align-items:center;padding:0 32px;gap:4px;box-shadow:0 2px 8px rgba(0,0,0,.2);}
.nav-item{display:block;padding:12px 16px;color:#fff;font-size:12px;font-weight:600;cursor:pointer;letter-spacing:.05em;text-transform:uppercase;white-space:nowrap;transition:background .2s;text-decoration:none;}
.nav-item:hover,.nav-item.active{background:rgba(255,255,255,.18);}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:12px;}
.nav-user{font-size:12px;color:rgba(255,255,255,.7);}
.nav-logout{font-size:11px;color:rgba(255,255,255,.5);text-decoration:none;padding:4px 10px;border:1px solid rgba(255,255,255,.2);border-radius:5px;}
.nav-logout:hover{color:#fff;border-color:rgba(255,255,255,.5);}

/* Page header */
.page-header{background:var(--surface);border-bottom:1px solid var(--border);padding:20px 40px;}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:28px;letter-spacing:.07em;}
.page-title span{color:var(--accent);}
.page-sub{font-size:12px;color:var(--text-muted);margin-top:2px;}

.container{max-width:1100px;margin:24px auto;padding:0 24px;}

/* Cards — MM style with teal left border on titles */
.card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:22px 26px;margin-bottom:20px;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:15px;letter-spacing:.08em;color:var(--text-muted);margin-bottom:16px;padding-bottom:10px;border-bottom:1px solid var(--border);display:flex;align-items:center;gap:8px;}
.card-title::before{content:'';display:inline-block;width:3px;height:14px;background:var(--teal);border-radius:2px;}

/* Form */
.form-row{display:flex;gap:14px;align-items:flex-end;flex-wrap:wrap;margin-bottom:16px;}
.form-group{display:flex;flex-direction:column;gap:4px;flex:1;min-width:180px;}
.form-group label{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);}
.form-group input,.form-group select{padding:9px 12px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:13px;background:#fafafa;outline:none;}
.form-group input:focus,.form-group select:focus{border-color:var(--teal);}
.req{color:var(--accent);}

.btn{border:none;border-radius:8px;padding:10px 20px;font-size:12px;font-weight:700;letter-spacing:.06em;cursor:pointer;font-family:inherit;}
.btn-primary{background:var(--teal);color:#fff;}.btn-primary:hover{background:#148a5b;}
.btn-accent{background:var(--accent);color:#fff;}.btn-accent:hover{background:var(--accent-dark);}
.btn-cancel{background:#f0f0f0;color:#333;border:1px solid #ddd;}

.alert{padding:12px 16px;border-radius:8px;font-size:13px;margin-bottom:16px;}
.alert-success{background:#eafaf1;color:var(--teal);border:1px solid #b2dfdb;}
.alert-danger{background:#fdf3f2;color:var(--accent);border:1px solid #f5c6cb;}

/* Tabs — MM style */
.tab-bar{display:flex;gap:0;background:var(--surface);border:1px solid var(--border);border-radius:var(--radius) var(--radius) 0 0;overflow:hidden;}
.tab-btn{padding:14px 24px;font-size:12px;font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:var(--text-muted);cursor:pointer;border:none;background:none;border-bottom:3px solid transparent;font-family:inherit;transition:all .2s;}
.tab-btn:hover{color:var(--text);background:var(--surface2);}
.tab-btn.active{color:var(--accent);border-bottom-color:var(--accent);background:var(--surface);}

/* Table */
table{width:100%;border-collapse:collapse;font-size:13px;}
th{background:var(--surface2);font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);padding:10px 12px;text-align:left;border-bottom:2px solid var(--border);}
td{padding:9px 12px;border-bottom:1px solid #f0f0f0;vertical-align:middle;}
tr:hover{background:rgba(26,158,106,0.04);}

.badge{display:inline-block;padding:2px 8px;border-radius:10px;font-size:10px;font-weight:700;}
.badge-active{background:#eafaf1;color:var(--teal);}
.badge-inactive{background:#f5f5f5;color:var(--text-dim);}
.badge-super{background:rgba(204,30,30,0.08);color:var(--accent);}
.badge-app{background:rgba(26,158,106,0.1);color:var(--teal);margin:1px 2px;padding:2px 6px;border-radius:4px;font-size:9px;font-weight:600;display:inline-block;}

.link-btn{font-size:11px;font-weight:700;text-decoration:none;cursor:pointer;border:none;background:none;padding:0;}
.link-edit{color:var(--teal);}
.link-danger{color:var(--accent);}
.link-orange{color:var(--orange);}

/* Role access config — split panel */
.role-grid{display:grid;grid-template-columns:220px 1fr;gap:0;border:1px solid var(--border);border-radius:var(--radius);overflow:hidden;min-height:400px;}
.role-list{background:var(--surface2);border-right:1px solid var(--border);}
.role-item{padding:12px 16px;cursor:pointer;font-size:13px;font-weight:500;border-bottom:1px solid #eee;transition:background .15s;}
.role-item:hover{background:rgba(26,158,106,0.06);}
.role-item.active{background:linear-gradient(135deg,#1a1a1a,var(--accent));color:#fff;font-weight:700;}
.role-item .role-desc{font-size:10px;color:var(--text-dim);margin-top:2px;}
.role-item.active .role-desc{color:rgba(255,255,255,.7);}
.role-detail{padding:20px;background:var(--surface);}
.role-detail-title{font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.06em;margin-bottom:16px;display:flex;align-items:center;gap:8px;}
.role-detail-title::before{content:'';display:inline-block;width:3px;height:16px;background:var(--accent);border-radius:2px;}

.app-access-row{display:flex;align-items:center;gap:12px;padding:10px 0;border-bottom:1px solid #f0f0f0;}
.app-access-row label{font-size:13px;font-weight:600;display:flex;align-items:center;gap:8px;cursor:pointer;min-width:200px;}
.app-access-row input[type="checkbox"]{width:18px;height:18px;accent-color:var(--teal);}
.module-list{margin-left:36px;display:flex;flex-wrap:wrap;gap:6px;margin-top:6px;margin-bottom:8px;}
.module-list label{font-size:11px;font-weight:500;display:flex;align-items:center;gap:4px;cursor:pointer;padding:4px 10px;background:var(--surface2);border:1px solid #eee;border-radius:6px;transition:all .15s;}
.module-list label:hover{background:rgba(26,158,106,0.06);border-color:var(--teal);}
.module-list input[type="checkbox"]{width:14px;height:14px;accent-color:var(--teal);}
.module-list.hidden{display:none;}
</style>
</head>
<body>
<form id="form1" runat="server">

<nav class="ua-nav">
    <a href="/StockApp/ERPHome.aspx" class="nav-item" title="ERP Home">&#x2302; ERP Home</a>
    <span class="nav-item active">&#x1F511; User Administration</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblNavUser" runat="server"/></span>
        <a href="#" class="nav-logout" onclick="erpConfirm('Sign out?',{title:'Sign Out',type:'warn',okText:'Sign Out',onOk:function(){window.location='UALogout.aspx';}});return false;">Sign Out</a>
    </div>
</nav>

<div class="page-header">
    <div class="page-title">User <span>Administration</span></div>
    <div class="page-sub">Manage users, roles, and access control</div>
</div>

<div class="container">

<asp:Panel ID="pnlAlert" runat="server" Visible="false" CssClass="alert"><asp:Label ID="lblAlert" runat="server"/></asp:Panel>

<asp:HiddenField ID="hfTab" runat="server" Value="users"/>

<!-- Tabs -->
<div class="tab-bar">
    <asp:Button ID="btnTabUsers" runat="server" Text="&#x1F465; Users" CssClass="tab-btn active" OnClick="btnTab_Click" CommandArgument="users"/>
    <asp:Button ID="btnTabRoles" runat="server" Text="&#x1F511; Role Access Config" CssClass="tab-btn" OnClick="btnTab_Click" CommandArgument="roles"/>
</div>

<!-- ═══════ TAB 1: USERS ═══════ -->
<asp:Panel ID="pnlUsers" runat="server">

<div class="card" style="border-top-left-radius:0;border-top-right-radius:0;">
    <div class="card-title"><asp:Label ID="lblFormTitle" runat="server" Text="Create New User"/></div>
    <asp:HiddenField ID="hfEditUserId" runat="server" Value="0"/>
    <div class="form-row">
        <div class="form-group"><label>Full Name <span class="req">*</span></label>
            <asp:TextBox ID="txtFullName" runat="server" placeholder="e.g. Rajan S"/></div>
        <div class="form-group"><label>Username <span class="req">*</span></label>
            <asp:TextBox ID="txtUsername" runat="server" placeholder="e.g. rajan"/></div>
        <div class="form-group"><label>Role <span class="req">*</span></label>
            <asp:DropDownList ID="ddlRole" runat="server"/></div>
    </div>
    <asp:Panel ID="pnlPassword" runat="server">
    <div class="form-row">
        <div class="form-group" style="max-width:300px;"><label>Temporary Password <span class="req">*</span></label>
            <asp:TextBox ID="txtPassword" runat="server" placeholder="Min 6 characters"/></div>
    </div>
    </asp:Panel>
    <div style="display:flex;gap:10px;">
        <asp:Button ID="btnSave" runat="server" Text="Create User" CssClass="btn btn-primary" OnClick="btnSave_Click"/>
        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-cancel" OnClick="btnCancel_Click" Visible="false"/>
    </div>
</div>

<div class="card">
    <div class="card-title">All Users (<asp:Label ID="lblCount" runat="server" Text="0"/>)</div>
    <asp:Repeater ID="rptUsers" runat="server">
        <HeaderTemplate><table><tr><th>Name</th><th>Username</th><th>Role</th><th>Apps</th><th>Status</th><th>Last Login</th><th>Actions</th></tr></HeaderTemplate>
        <ItemTemplate>
            <tr>
                <td style="font-weight:500;"><%# Eval("FullName") %></td>
                <td style="font-size:12px;color:var(--text-muted);"><%# Eval("Username") %></td>
                <td><span class='badge <%# Eval("Role").ToString()=="Super" ? "badge-super" : "" %>'><%# Eval("RoleName") %></span></td>
                <td><%# RenderAppBadges(Eval("Role").ToString()) %></td>
                <td><span class='badge <%# Convert.ToBoolean(Eval("IsActive")) ? "badge-active" : "badge-inactive" %>'><%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %></span></td>
                <td style="font-size:11px;color:var(--text-dim);"><%# Eval("LastLogin") == DBNull.Value ? "Never" : Convert.ToDateTime(Eval("LastLogin")).ToString("dd-MMM HH:mm") %></td>
                <td style="white-space:nowrap;">
                    <asp:LinkButton ID="lnkEdit" runat="server" Text="Edit" CommandName="EditUser" CommandArgument='<%# Eval("UserID") %>' OnCommand="UserAction_Command" CssClass="link-btn link-edit"/>
                    <asp:LinkButton ID="lnkToggle" runat="server"
                        Text='<%# Convert.ToBoolean(Eval("IsActive")) ? "Deactivate" : "Activate" %>'
                        CommandName='<%# Convert.ToBoolean(Eval("IsActive")) ? "Deactivate" : "Activate" %>'
                        CommandArgument='<%# Eval("UserID") %>' OnCommand="UserAction_Command"
                        OnClientClick="return erpConfirmLink(this,'Change user status?',{title:'Confirm',okText:'Yes',btnClass:'danger'});"
                        CssClass="link-btn link-danger" style="margin-left:8px;"/>
                    <asp:LinkButton ID="lnkReset" runat="server" Text="Reset Pwd" CommandName="ResetPwd" CommandArgument='<%# Eval("UserID") %>' OnCommand="UserAction_Command"
                        OnClientClick="return erpConfirmLink(this,'Reset password to sirimiri123?',{title:'Reset Password',okText:'Yes, Reset',btnClass:'danger'});"
                        CssClass="link-btn link-orange" style="margin-left:8px;"/>
                </td>
            </tr>
        </ItemTemplate>
        <FooterTemplate></table></FooterTemplate>
    </asp:Repeater>
</div>

</asp:Panel>

<!-- ═══════ TAB 2: ROLE ACCESS CONFIG ═══════ -->
<asp:Panel ID="pnlRoles" runat="server" Visible="false">

<asp:HiddenField ID="hfSelectedRole" runat="server" Value=""/>

<div class="role-grid" style="border-top-left-radius:0;border-top-right-radius:0;">
    <div class="role-list">
        <asp:Repeater ID="rptRoleList" runat="server">
            <ItemTemplate>
                <div class='role-item <%# Eval("RoleCode").ToString() == GetSelectedRole() ? "active" : "" %>'
                     onclick="__doPostBack('<%= btnSelectRole.UniqueID %>', '<%# Eval("RoleCode") %>');">
                    <div><%# Eval("RoleName") %></div>
                    <div class="role-desc"><%# Eval("Description") %></div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>
    <div class="role-detail">
        <asp:Panel ID="pnlNoRole" runat="server">
            <div style="text-align:center;padding:60px 20px;color:var(--text-dim);font-size:13px;">
                &#x1F511; Select a role from the left to configure access
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlRoleDetail" runat="server" Visible="false">
            <div class="role-detail-title">Access for: <asp:Label ID="lblRoleName" runat="server" style="color:var(--accent);"/></div>
            <asp:Repeater ID="rptRoleApps" runat="server">
                <ItemTemplate>
                    <div class="app-access-row">
                        <label>
                            <input type="checkbox" name="role_app" value='<%# Eval("AppCode") %>'
                                <%# Convert.ToInt32(Eval("CanAccess")) == 1 ? "checked" : "" %>
                                onchange="toggleRoleModules(this, '<%# Eval("AppCode") %>');" />
                            <%# Eval("AppName") %>
                        </label>
                    </div>
                    <div class="module-list <%# Convert.ToInt32(Eval("CanAccess")) == 1 ? "" : "hidden" %>" id="role_modules_<%# Eval("AppCode") %>">
                        <asp:Repeater ID="rptAppModules" runat="server" DataSource='<%# GetRoleModules(Eval("AppCode").ToString()) %>'>
                            <ItemTemplate>
                                <label>
                                    <input type="checkbox" name='role_mod_<%# Eval("AppCode") %>' value='<%# Eval("ModuleCode") %>'
                                        <%# Convert.ToInt32(Eval("CanAccess")) == 1 ? "checked" : "" %> />
                                    <%# Eval("ModuleName") %>
                                </label>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
            <div style="margin-top:20px;">
                <asp:Button ID="btnSaveRoleAccess" runat="server" Text="&#x1F4BE; Save Role Access" CssClass="btn btn-primary" OnClick="btnSaveRoleAccess_Click"/>
            </div>
        </asp:Panel>
    </div>
</div>

<asp:Button ID="btnSelectRole" runat="server" OnClick="btnSelectRole_Click" style="display:none;"/>

</asp:Panel>

</div>

<script>
function toggleRoleModules(chk, appCode) {
    var panel = document.getElementById('role_modules_' + appCode);
    if (!panel) return;
    if (chk.checked) {
        panel.classList.remove('hidden');
        panel.querySelectorAll('input[type=checkbox]').forEach(function(cb) { cb.checked = true; });
    } else {
        panel.classList.add('hidden');
        panel.querySelectorAll('input[type=checkbox]').forEach(function(cb) { cb.checked = false; });
    }
}
</script>
<script src="/StockApp/erp-modal.js"></script>
<script src="/StockApp/erp-keepalive.js"></script>
</form>
</body>
</html>
