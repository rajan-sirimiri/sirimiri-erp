<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PKMachineMaster.aspx.cs" Inherits="PKApp.PKMachineMaster" EnableEventValidation="false" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri PK — Machine Master</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600;700&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--accent:#e67e22;--accent-dark:#d35400;--accent-light:#fef5ec;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--bg:#f0f0f0;--surface:#fff;--surface2:#faf9f7;--border:#e0e0e0;--radius:14px;--nav-h:52px;}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);color:var(--text);min-height:100vh;}
nav{background:#1a1a1a;height:var(--nav-h);display:flex;align-items:center;padding:0 20px;gap:12px;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;}
.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.08em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:16px;}
.nav-user{color:rgba(255,255,255,.8);font-size:12px;}
.nav-link{color:#fff;font-size:12px;font-weight:600;text-decoration:none;opacity:.8;}
.nav-link:hover{opacity:1;}
.page-header{background:var(--surface);border-bottom:3px solid var(--accent);padding:20px 30px;display:flex;align-items:center;gap:14px;}
.page-icon{font-size:28px;}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:28px;letter-spacing:.07em;}
.page-title span{color:var(--accent);}
.page-sub{font-size:12px;color:var(--text-dim);}
.content{max-width:900px;margin:0 auto;padding:24px;}
.card{background:var(--surface);border-radius:var(--radius);padding:24px;margin-bottom:20px;box-shadow:0 1px 4px rgba(0,0,0,.05);}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.06em;margin-bottom:16px;padding-bottom:8px;border-bottom:2px solid var(--border);}
.form-row{display:flex;gap:12px;align-items:flex-end;flex-wrap:wrap;margin-bottom:16px;}
.form-group{display:flex;flex-direction:column;gap:4px;}
.form-group label{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);}
.form-group input,.form-group select{padding:9px 12px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:13px;outline:none;}
.form-group input:focus{border-color:var(--accent);}
.btn-add{padding:9px 20px;background:var(--accent);color:#fff;border:none;border-radius:8px;font-size:13px;font-weight:700;cursor:pointer;white-space:nowrap;min-height:38px;}
.btn-add:hover{background:var(--accent-dark);}
table{width:100%;border-collapse:collapse;font-size:13px;}
thead th{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);padding:10px;text-align:left;border-bottom:2px solid var(--border);background:var(--surface2);}
tbody td{padding:10px;border-bottom:1px solid #f2f0ed;vertical-align:middle;}
tbody tr:nth-child(even){background:var(--surface2);}
.code-badge{font-family:'JetBrains Mono',monospace;font-size:12px;font-weight:700;background:var(--accent-light);color:var(--accent-dark);padding:3px 10px;border-radius:6px;}
.status-active{color:#2ecc71;font-weight:700;font-size:12px;}
.status-inactive{color:#e74c3c;font-weight:700;font-size:12px;}
.btn-toggle{padding:4px 12px;border:1.5px solid var(--border);border-radius:6px;font-size:11px;font-weight:600;cursor:pointer;background:#fff;}
.btn-toggle:hover{background:var(--surface2);}
.btn-edit{padding:4px 12px;border:1.5px solid var(--accent);border-radius:6px;font-size:11px;font-weight:600;cursor:pointer;background:#fff;color:var(--accent);}
.btn-edit:hover{background:var(--accent-light);}
.alert-panel{margin-bottom:16px;}
.empty-msg{text-align:center;padding:30px;color:var(--text-dim);font-size:13px;}
</style>
</head>
<body>
<form id="form1" runat="server">
<asp:HiddenField ID="hfEditId" runat="server" Value="0"/>

<nav>
    <div class="nav-logo"><img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="" onerror="this.style.display='none'"/></div>
    <span class="nav-title">PACKING &amp; SHIPMENTS</span>
    <div class="nav-right">
        <asp:Label ID="lblNavUser" runat="server" CssClass="nav-user"/>
        <a href="PKHome.aspx" class="nav-link">← PK Home</a>
        <a href="/StockApp/ERPHome.aspx" class="nav-link">← ERP</a>
        <a href="PKLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>

<div class="page-header">
    <span class="page-icon">&#x2699;&#xFE0F;</span>
    <div>
        <div class="page-title">Machine <span>Master</span></div>
        <div class="page-sub">Register and manage packing machines used in primary packing</div>
    </div>
</div>

<div class="content">

    <asp:Panel ID="pnlAlert" runat="server" Visible="false" CssClass="alert-panel">
        <asp:Label ID="lblAlert" runat="server"/>
    </asp:Panel>

    <!-- ADD / EDIT FORM -->
    <div class="card">
        <div class="card-title"><asp:Label ID="lblFormTitle" runat="server" Text="Add New Machine"/></div>
        <div class="form-row">
            <div class="form-group" style="min-width:120px;">
                <label>Machine Code *</label>
                <asp:TextBox ID="txtMachineCode" runat="server" MaxLength="20" placeholder="e.g. M1"/>
            </div>
            <div class="form-group" style="flex:1;min-width:200px;">
                <label>Machine Name *</label>
                <asp:TextBox ID="txtMachineName" runat="server" MaxLength="50" placeholder="e.g. Packing Machine 1"/>
            </div>
            <div class="form-group" style="flex:1;min-width:180px;">
                <label>Location</label>
                <asp:TextBox ID="txtLocation" runat="server" MaxLength="100" placeholder="e.g. Production Line 1"/>
            </div>
            <asp:Button ID="btnSave" runat="server" Text="+ Add Machine" CssClass="btn-add"
                OnClick="btnSave_Click" CausesValidation="false"/>
            <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn-toggle" Visible="false"
                OnClick="btnCancel_Click" CausesValidation="false"/>
        </div>
    </div>

    <!-- MACHINE LIST -->
    <div class="card">
        <div class="card-title">Registered Machines (<asp:Label ID="lblCount" runat="server" Text="0"/>)</div>
        <asp:Panel ID="pnlEmpty" runat="server">
            <div class="empty-msg">No machines registered yet. Add one above.</div>
        </asp:Panel>
        <asp:Panel ID="pnlTable" runat="server" Visible="false">
        <table>
            <thead><tr>
                <th>Code</th>
                <th>Machine Name</th>
                <th>Location</th>
                <th>Status</th>
                <th style="text-align:center;">Actions</th>
            </tr></thead>
            <tbody>
                <asp:Repeater ID="rptMachines" runat="server" OnItemCommand="rptMachines_ItemCommand">
                    <ItemTemplate>
                    <tr>
                        <td><span class="code-badge"><%# Eval("MachineCode") %></span></td>
                        <td style="font-weight:600;"><%# Eval("MachineName") %></td>
                        <td style="color:var(--text-muted);"><%# Eval("Location") == DBNull.Value ? "—" : Eval("Location") %></td>
                        <td><%# Convert.ToBoolean(Eval("IsActive")) ? "<span class='status-active'>Active</span>" : "<span class='status-inactive'>Inactive</span>" %></td>
                        <td style="text-align:center;white-space:nowrap;">
                            <asp:LinkButton ID="lnkEdit" runat="server" CommandName="EditMachine" CommandArgument='<%# Eval("MachineID") %>'
                                CssClass="btn-edit" CausesValidation="false">Edit</asp:LinkButton>
                            <asp:LinkButton ID="lnkToggle" runat="server" CommandName="ToggleActive" CommandArgument='<%# Eval("MachineID") %>'
                                CssClass="btn-toggle" CausesValidation="false"><%# Convert.ToBoolean(Eval("IsActive")) ? "Deactivate" : "Activate" %></asp:LinkButton>
                        </td>
                    </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
        </asp:Panel>
    </div>

</div>

<script src="/StockApp/erp-keepalive.js"></script>
</form>
</body>
</html>
