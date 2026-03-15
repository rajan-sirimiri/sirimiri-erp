<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UserAdmin.aspx.cs" Inherits="StockApp.UserAdmin" %>
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width, initial-scale=1.0"/>
<title>User Management — Sirimiri Stock App</title>
<style>
:root { --accent:#C0392B; --surface:#fff; --bg:#f5f5f5; --border:#e0e0e0; --text:#1a1a1a; --muted:#666; }
* { box-sizing:border-box; margin:0; padding:0; }
body { background:var(--bg); font-family:'Segoe UI',Arial,sans-serif; color:var(--text); }
.topbar { background:var(--accent); color:#fff; padding:14px 32px;
          display:flex; align-items:center; justify-content:space-between; }
.topbar .title { font-size:16px; font-weight:700; letter-spacing:.04em; }
.topbar .nav a { color:#fff; text-decoration:none; font-size:13px; margin-left:20px;
                 opacity:.85; } .topbar .nav a:hover { opacity:1; }
.container { max-width:1000px; margin:32px auto; padding:0 16px; }
h1 { font-size:22px; font-weight:700; margin-bottom:4px; }
.sub { color:var(--muted); font-size:13px; margin-bottom:24px; }

/* Card */
.card { background:var(--surface); border-radius:12px; box-shadow:0 2px 12px rgba(0,0,0,.07);
        padding:28px; margin-bottom:24px; }
.card h2 { font-size:15px; font-weight:700; margin-bottom:18px;
           padding-bottom:10px; border-bottom:1.5px solid var(--border); }

/* Form grid */
.form-grid { display:grid; grid-template-columns:1fr 1fr; gap:14px 20px; }
.form-grid .full { grid-column:1/-1; }
.field label { display:block; font-size:11px; font-weight:600; text-transform:uppercase;
               letter-spacing:.05em; color:var(--muted); margin-bottom:5px; }
.field input, .field select { width:100%; padding:9px 12px; border:1.5px solid var(--border);
    border-radius:7px; font-size:13px; outline:none; }
.field input:focus, .field select:focus { border-color:var(--accent); }
.btn-add { padding:10px 24px; background:var(--accent); color:#fff; border:none;
           border-radius:7px; font-size:13px; font-weight:700; cursor:pointer; }
.btn-add:hover { opacity:.88; }

/* Table */
table { width:100%; border-collapse:collapse; font-size:13px; }
thead th { background:#f8f8f8; padding:10px 12px; text-align:left; font-size:11px;
           font-weight:700; text-transform:uppercase; letter-spacing:.05em;
           color:var(--muted); border-bottom:1.5px solid var(--border); }
tbody td { padding:10px 12px; border-bottom:1px solid #f0f0f0; vertical-align:middle; }
tbody tr:hover { background:#fafafa; }
.badge { display:inline-block; padding:2px 10px; border-radius:20px; font-size:11px; font-weight:700; }
.badge-admin   { background:#fdecea; color:var(--accent); }
.badge-manager { background:#e8f4fd; color:#1a73e8; }
.badge-field   { background:#e8f8f0; color:#27ae60; }
.badge-active  { background:#e8f8f0; color:#27ae60; }
.badge-inactive{ background:#f5f5f5; color:#999; }
.btn-sm { padding:4px 12px; border-radius:5px; font-size:12px; font-weight:600;
          cursor:pointer; border:1.5px solid; }
.btn-reset  { color:#1a73e8; border-color:#1a73e8; background:#fff; }
.btn-toggle { color:var(--accent); border-color:var(--accent); background:#fff; }
.btn-reset:hover  { background:#e8f4fd; }
.btn-toggle:hover { background:#fdecea; }
.msg-ok  { background:#e8f8f0; border:1px solid #a8e6c0; color:#27ae60;
           border-radius:8px; padding:10px 14px; font-size:13px; margin-bottom:16px; }
.msg-err { background:#fdecea; border:1px solid #f5c6c2; color:var(--accent);
           border-radius:8px; padding:10px 14px; font-size:13px; margin-bottom:16px; }
@media(max-width:600px){ .form-grid{ grid-template-columns:1fr; } }
    .upload-hint   { color:#555; font-size:13px; margin-bottom:14px; }
    .bulk-actions  { margin-bottom:12px; }
    .btn-template  { display:inline-block; padding:9px 18px; background:#1a7a4a; color:#fff;
                     border-radius:6px; text-decoration:none; font-size:13px; font-weight:600; cursor:pointer; border:none; }
    .btn-template:hover { opacity:.88; color:#fff; }
    .upload-row    { display:flex; gap:12px; align-items:center; flex-wrap:wrap; margin-bottom:14px; }
    .file-input    { flex:1; min-width:200px; font-size:13px; }
    .bulk-result   { display:flex; gap:20px; flex-wrap:wrap; background:#f4fdf8;
                     border:1px solid #b3dfc9; border-radius:6px; padding:12px 16px; margin-bottom:10px; }
    .bulk-stat     { font-size:13px; color:#333; }
    .bulk-detail   { font-size:12px; color:#c0392b; white-space:pre-wrap; display:block; margin-top:6px; }
</style>
</head>
<body>
<form id="form1" runat="server">
<div class="topbar">
    <div class="title">&#9881; User Management</div>
    <div class="nav">
        <a href="StockEntry.aspx">&#8592; Stock Entry</a>
        <a href="Login.aspx" onclick="return confirm('Sign out?')">Sign Out</a>
    </div>
</div>

<div class="container">
    <h1>User Management</h1>
    <p class="sub">Manage user accounts. Admins can create users and change active status.</p>

    <asp:Panel ID="pnlMsg" runat="server" Visible="false">
        <div class="<%=MsgCssClass %>"><asp:Label ID="lblMsg" runat="server" /></div>
    </asp:Panel>

    <!-- Add User (Admin only) -->
    <asp:Panel ID="pnlAddUser" runat="server">
    <div class="card">
        <h2>Add New User</h2>
        <div class="form-grid">
            <div class="field">
                <label>Full Name</label>
                <asp:TextBox ID="txtFullName" runat="server" placeholder="e.g. Rajan Kumar" />
            </div>
            <div class="field">
                <label>Username</label>
                <asp:TextBox ID="txtUsername" runat="server" placeholder="e.g. rajan.kumar" />
            </div>
            <div class="field">
                <label>Temporary Password</label>
                <asp:TextBox ID="txtTempPwd" runat="server" TextMode="Password" placeholder="Min 8 chars" />
            </div>
            <div class="field">
                <label>Role</label>
                <asp:DropDownList ID="ddlRole" runat="server" AutoPostBack="true"
                    OnSelectedIndexChanged="ddlRole_Changed">
                    <asp:ListItem Text="Field User" Value="FieldUser" />
                    <asp:ListItem Text="Manager"    Value="Manager" />
                    <asp:ListItem Text="Admin"      Value="Admin" />
                </asp:DropDownList>
            </div>
            <div class="field" id="divState" runat="server">
                <label>Assigned State <small>(Field User only)</small></label>
                <asp:DropDownList ID="ddlState" runat="server">
                    <asp:ListItem Text="— Select State —" Value="0" />
                </asp:DropDownList>
            </div>
        </div>
        <br/>
        <asp:Button ID="btnAdd" runat="server" Text="Create User"
            CssClass="btn-add" OnClick="btnAdd_Click" />
    </div>
    </asp:Panel>

    <!-- Bulk Upload (Admin only) -->
    <asp:Panel ID="pnlBulkUpload" runat="server">
    <div class="card">
        <h2>Bulk Upload Users</h2>
        <p class="upload-hint">Download the template, fill in user details, and upload. All users will be created with <strong>MustChangePwd = Yes</strong>.</p>

        <div class="bulk-actions">
            <asp:LinkButton ID="btnDownloadTemplate" runat="server"
                CssClass="btn-template" OnClick="btnDownloadTemplate_Click">
                &#8595; Download Excel Template
            </asp:LinkButton>
        </div>

        <div class="upload-row">
            <asp:FileUpload ID="fileUsers" runat="server" CssClass="file-input" Accept=".xlsx" />
            <asp:Button ID="btnBulkUpload" runat="server" Text="Upload &amp; Create Users"
                CssClass="btn-add" OnClick="btnBulkUpload_Click" />
        </div>

        <asp:Panel ID="pnlBulkResult" runat="server" Visible="false">
            <div class="bulk-result">
                <span class="bulk-stat">&#10003; Created: <strong><asp:Label ID="lblCreated" runat="server" /></strong></span>
                <span class="bulk-stat">&#8212; Skipped (duplicate): <strong><asp:Label ID="lblSkipped" runat="server" /></strong></span>
                <span class="bulk-stat">&#9888; Errors: <strong><asp:Label ID="lblBulkErrors" runat="server" /></strong></span>
            </div>
            <asp:Label ID="lblBulkDetail" runat="server" CssClass="bulk-detail" />
        </asp:Panel>
    </div>
    </asp:Panel>

    <!-- EDIT USER PANEL -->
    <asp:Panel ID="pnlEditUser" runat="server" Visible="false">
    <div class="card" style="border:2px solid #2979c9;">
        <h2 style="padding:12px 20px;background:#2979c9;color:#fff;font-family:'Bebas Neue',cursive;letter-spacing:.08em;">Edit User</h2>
        <div style="padding:20px;">
            <asp:HiddenField ID="hfEditUserId" runat="server" />
            <div class="form-grid">
                <div class="field">
                    <label>Full Name</label>
                    <asp:TextBox ID="txtEditFullName" runat="server" />
                </div>
                <div class="field">
                    <label>Username</label>
                    <asp:TextBox ID="txtEditUsername" runat="server" />
                </div>
                <div class="field">
                    <label>Role</label>
                    <asp:DropDownList ID="ddlEditRole" runat="server" AutoPostBack="true"
                        OnSelectedIndexChanged="ddlEditRole_Changed">
                        <asp:ListItem Text="Field User" Value="FieldUser" />
                        <asp:ListItem Text="Manager"    Value="Manager" />
                        <asp:ListItem Text="Admin"      Value="Admin" />
                    </asp:DropDownList>
                </div>
                <div class="field" id="divEditState" runat="server">
                    <label>Assigned State</label>
                    <asp:DropDownList ID="ddlEditState" runat="server">
                        <asp:ListItem Text="— Select State —" Value="0" />
                    </asp:DropDownList>
                </div>
                <div class="field">
                    <label>Reporting Manager</label>
                    <asp:DropDownList ID="ddlEditManager" runat="server">
                        <asp:ListItem Text="— None —" Value="0" />
                    </asp:DropDownList>
                </div>
            </div>
            <br/>
            <div style="display:flex;gap:12px;">
                <asp:Button ID="btnEditUserSave" runat="server" Text="Save Changes"
                    CssClass="btn-add" OnClick="btnEditUserSave_Click" />
                <asp:Button ID="btnEditUserCancel" runat="server" Text="Cancel"
                    CssClass="btn-add" style="background:#888;"
                    OnClick="btnEditUserCancel_Click" CausesValidation="false" />
            </div>
        </div>
    </div>
    </asp:Panel>

    <!-- User List -->
    <div class="card">
        <h2>All Users</h2>
        <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="false"
            CssClass="" OnRowCommand="gvUsers_RowCommand" OnRowDataBound="gvUsers_RowDataBound">
            <Columns>
                <asp:BoundField  DataField="FullName"  HeaderText="Full Name" />
                <asp:BoundField  DataField="Username"  HeaderText="Username" />
                <asp:TemplateField HeaderText="Role">
                    <ItemTemplate>
                        <span class='badge <%# GetRoleBadge(Eval("Role").ToString()) %>'>
                            <%# Eval("Role") %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField  DataField="StateName" HeaderText="State" NullDisplayText="All" />
                <asp:TemplateField HeaderText="Status">
                    <ItemTemplate>
                        <span class='badge <%# Convert.ToBoolean(Eval("IsActive")) ? "badge-active" : "badge-inactive" %>'>
                            <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField  DataField="LastLogin"  HeaderText="Last Login" DataFormatString="{0:dd MMM yyyy HH:mm}" NullDisplayText="Never" />
                <asp:TemplateField HeaderText="Reporting Manager">
                    <ItemTemplate>
                        <%# Eval("ManagerName") != DBNull.Value && Eval("ManagerName") != null && Eval("ManagerName").ToString() != "" ? Eval("ManagerName").ToString() : "<span style='color:#aaa'>&mdash;</span>" %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Actions">
                    <ItemTemplate>
                        <asp:LinkButton CommandName="EditUser" CommandArgument='<%# Eval("UserID") %>'
                            CssClass="btn-sm btn-edit" runat="server">Edit</asp:LinkButton>
                        &nbsp;
                        <asp:LinkButton CommandName="ResetPwd" CommandArgument='<%# Eval("UserID") %>'
                            CssClass="btn-sm btn-reset" runat="server"
                            OnClientClick="return confirm('Reset password to Temp@1234?')">Reset Pwd</asp:LinkButton>
                        &nbsp;
                        <asp:LinkButton ID="btnToggle" CommandName="ToggleActive" CommandArgument='<%# Eval("UserID") %>'
                            CssClass="btn-sm btn-toggle" runat="server"
                            OnClientClick="return confirm('Toggle active status?')">
                            <%# Convert.ToBoolean(Eval("IsActive")) ? "Deactivate" : "Activate" %>
                        </asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>
</div>
</form>
</body>
</html>
