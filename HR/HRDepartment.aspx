<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HRDepartment.aspx.cs" Inherits="HRModule.HRDepartment" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Departments — Sirimiri ERP</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <style>
        * { box-sizing: border-box; }
        body { margin:0; font-family: 'Segoe UI', Arial, sans-serif; background:#f4f5f7; color:#222; }
        .navbar { background:#111; color:#fff; padding:12px 22px; display:flex; gap:18px; align-items:center; }
        .navbar a { color:#fff; text-decoration:none; font-size:14px; }
        .navbar a:hover { color:#ff6a3d; }
        .brand { font-weight:600; letter-spacing:.5px; }
        .page-header { padding:18px 22px; background:#fff; border-bottom:3px solid #d93025; }
        .page-header h1 { margin:0; font-size:22px; }
        .container { max-width:1100px; margin:20px auto; padding:0 20px; }
        .card { background:#fff; border:1px solid #e1e3e7; border-radius:6px; padding:18px; margin-bottom:18px; }
        .grid { display:grid; grid-template-columns: 160px 1fr 160px 1fr; gap:10px 14px; align-items:center; }
        input[type=text] { width:100%; padding:7px 9px; border:1px solid #c4c7cc; border-radius:4px; font-size:14px; }
        .btn { padding:8px 16px; border:none; border-radius:4px; cursor:pointer; font-size:14px; }
        .btn-primary { background:#1a73e8; color:#fff; }
        .btn-danger  { background:#d93025; color:#fff; }
        .btn-secondary { background:#5f6368; color:#fff; }
        table { width:100%; border-collapse:collapse; font-size:14px; }
        th { background:#f1f3f4; text-align:left; padding:9px 8px; border-bottom:1px solid #dadce0; }
        td { padding:8px; border-bottom:1px solid #eee; }
        tr:hover { background:#fafbfc; }
        .msg { padding:9px 12px; border-radius:4px; margin-bottom:12px; font-size:14px; }
        .msg-ok  { background:#e6f4ea; color:#137333; border:1px solid #ceead6; }
        .msg-err { background:#fce8e6; color:#c5221f; border:1px solid #f6c2bd; }
        .actions { display:flex; gap:8px; }
        .pill { display:inline-block; padding:2px 10px; border-radius:10px; font-size:12px; }
        .pill-on  { background:#e6f4ea; color:#137333; }
        .pill-off { background:#fce8e6; color:#c5221f; }
    </style>
</head>
<body>
<form id="form1" runat="server">
    <div class="navbar">
        <span class="brand">SIRIMIRI ERP</span>
        <a href="/">Home</a>
        <a href="/MM/">MM</a>
        <a href="/PP/">PP</a>
        <a href="/PK/">PK</a>
        <a href="/FIN/">FIN</a>
        <a href="HREmployee.aspx">HR · Employees</a>
        <a href="HRDepartment.aspx"><b>Departments</b></a>
        <a href="HREmployeeImport.aspx">Import</a>
    </div>
    <div class="page-header"><h1>Departments</h1></div>

    <div class="container">
        <asp:Panel ID="pnlMsg" runat="server" Visible="false" CssClass="msg"></asp:Panel>

        <div class="card">
            <h3 style="margin-top:0">
                <asp:Literal ID="litFormHeading" runat="server" Text="Add Department" />
            </h3>
            <asp:HiddenField ID="hfDeptID" runat="server" Value="0" />
            <div class="grid">
                <label>Dept Code</label>
                <asp:TextBox ID="txtCode" runat="server" placeholder="Auto if blank" />
                <label>Dept Name *</label>
                <asp:TextBox ID="txtName" runat="server" />
                <label>Active</label>
                <asp:CheckBox ID="chkActive" runat="server" Checked="true" />
                <label></label>
                <div class="actions">
                    <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="btnClear_Click" CausesValidation="false" />
                </div>
            </div>
        </div>

        <div class="card">
            <h3 style="margin-top:0">Existing Departments</h3>
            <asp:GridView ID="gvDepts" runat="server" AutoGenerateColumns="false"
                          GridLines="None" CssClass="gv" DataKeyNames="DeptID"
                          OnRowCommand="gvDepts_RowCommand">
                <Columns>
                    <asp:BoundField DataField="DeptCode" HeaderText="Code" />
                    <asp:BoundField DataField="DeptName" HeaderText="Name" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <span class='<%# (Convert.ToInt32(Eval("IsActive"))==1) ? "pill pill-on" : "pill pill-off" %>'>
                                <%# (Convert.ToInt32(Eval("IsActive"))==1) ? "Active" : "Inactive" %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:LinkButton runat="server" Text="Edit" CommandName="EditDept"
                                CommandArgument='<%# Eval("DeptID") %>' CssClass="btn btn-secondary"
                                Style="padding:3px 10px" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>No departments yet.</EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>
</form>
</body>
</html>
