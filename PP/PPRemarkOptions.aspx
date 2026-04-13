<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PPRemarkOptions.aspx.cs" Inherits="PPApp.PPRemarkOptions" EnableEventValidation="false" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri PP — Remark Options</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600;700&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--accent:#cc1e1e;--accent-dark:#a01818;--accent-light:#fdf0f0;--txn:#0d6efd;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--bg:#f0f0f0;--surface:#fff;--surface2:#faf9f7;--border:#e0e0e0;--radius:14px;--nav-h:52px;}
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
.content{max-width:960px;margin:0 auto;padding:24px;}
.card{background:var(--surface);border-radius:var(--radius);padding:24px;margin-bottom:20px;box-shadow:0 1px 4px rgba(0,0,0,.05);}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.06em;margin-bottom:16px;padding-bottom:8px;border-bottom:2px solid var(--border);}
.form-row{display:flex;gap:12px;align-items:flex-end;flex-wrap:wrap;margin-bottom:16px;}
.form-group{display:flex;flex-direction:column;gap:4px;}
.form-group label{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);}
.form-group input,.form-group select{padding:9px 12px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:13px;outline:none;}
.form-group input:focus,.form-group select:focus{border-color:var(--accent);}
.btn-add{padding:9px 20px;background:var(--accent);color:#fff;border:none;border-radius:8px;font-size:13px;font-weight:700;cursor:pointer;white-space:nowrap;min-height:38px;}
.btn-add:hover{background:var(--accent-dark);}
.btn-cancel{padding:9px 20px;background:#f5f5f5;color:#333;border:1px solid var(--border);border-radius:8px;font-size:13px;font-weight:700;cursor:pointer;}
table{width:100%;border-collapse:collapse;font-size:13px;}
thead th{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);padding:10px;text-align:left;border-bottom:2px solid var(--border);background:var(--surface2);}
tbody td{padding:10px;border-bottom:1px solid #f2f0ed;vertical-align:middle;}
tbody tr:nth-child(even){background:var(--surface2);}
.line-badge{font-size:11px;font-weight:700;padding:3px 10px;border-radius:6px;display:inline-block;}
.line-BARFI{background:#fde8e8;color:#c0392b;}
.line-LADDU{background:#fef5ec;color:#e67e22;}
.line-GINGER{background:#e8f5e9;color:#27ae60;}
.line-BANANA{background:#fff9c4;color:#f9a825;}
.status-active{color:#2ecc71;font-weight:700;font-size:12px;}
.status-inactive{color:#e74c3c;font-weight:700;font-size:12px;}
.btn-sm{padding:4px 12px;border-radius:6px;font-size:11px;font-weight:600;cursor:pointer;border:1.5px solid var(--border);background:#fff;}
.btn-sm:hover{background:var(--surface2);}
.btn-edit{border-color:var(--accent);color:var(--accent);}
.btn-edit:hover{background:var(--accent-light);}
.filter-bar{display:flex;gap:12px;align-items:center;margin-bottom:16px;flex-wrap:wrap;}
.filter-pill{padding:6px 14px;border-radius:20px;font-size:12px;font-weight:600;cursor:pointer;border:1.5px solid var(--border);background:#fff;transition:all .15s;}
.filter-pill:hover{background:var(--surface2);}
.filter-pill.active{background:var(--accent);color:#fff;border-color:var(--accent);}
.alert-panel{margin-bottom:16px;}
.empty-msg{text-align:center;padding:30px;color:var(--text-dim);font-size:13px;}
</style>
</head>
<body>
<form id="form1" runat="server">
<asp:HiddenField ID="hfEditId" runat="server" Value="0"/>

<nav>
    <div class="nav-logo"><img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="" onerror="this.style.display='none'"/></div>
    <span class="nav-title">PRODUCTION</span>
    <div class="nav-right">
        <asp:Label ID="lblNavUser" runat="server" CssClass="nav-user"/>
        <a href="PPHome.aspx" class="nav-link">&#x2190; PP Home</a>
        <a href="/StockApp/ERPHome.aspx" class="nav-link">&#x2190; ERP</a>
    </div>
</nav>

<div class="page-header">
    <span class="page-icon">&#x1F4DD;</span>
    <div>
        <div class="page-title">Remark <span>Options</span></div>
        <div class="page-sub">Configure remark options per production line for batch execution</div>
    </div>
</div>

<div class="content">

    <asp:Panel ID="pnlAlert" runat="server" Visible="false" CssClass="alert-panel">
        <asp:Label ID="lblAlert" runat="server"/>
    </asp:Panel>

    <!-- ADD / EDIT FORM -->
    <div class="card">
        <div class="card-title"><asp:Label ID="lblFormTitle" runat="server" Text="Add Remark Option"/></div>
        <div class="form-row">
            <div class="form-group" style="min-width:180px;">
                <label>Production Line *</label>
                <asp:DropDownList ID="ddlLine" runat="server" style="padding:9px 12px;border:1.5px solid #e0e0e0;border-radius:8px;font-family:inherit;font-size:13px;"/>
            </div>
            <div class="form-group" style="flex:2;min-width:250px;">
                <label>Remark Text *</label>
                <asp:TextBox ID="txtRemarkText" runat="server" MaxLength="200" placeholder="e.g. Good Quality, Slightly Burnt, Rework Required"/>
            </div>
            <div class="form-group" style="min-width:80px;">
                <label>Sort Order</label>
                <asp:TextBox ID="txtSortOrder" runat="server" Text="1" MaxLength="3" style="text-align:center;width:70px;"/>
            </div>
            <asp:Button ID="btnSave" runat="server" Text="+ Add" CssClass="btn-add"
                OnClick="btnSave_Click" CausesValidation="false"/>
            <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn-cancel" Visible="false"
                OnClick="btnCancel_Click" CausesValidation="false"/>
        </div>
    </div>

    <!-- FILTER + LIST -->
    <div class="card">
        <div class="card-title">Remark Options (<asp:Label ID="lblCount" runat="server" Text="0"/>)</div>

        <!-- Line filter pills -->
        <div class="filter-bar">
            <asp:LinkButton ID="lnkAll" runat="server" CssClass="filter-pill active" Text="All" OnClick="lnkFilter_Click" CommandArgument="0" CausesValidation="false"/>
            <asp:Repeater ID="rptLinePills" runat="server">
                <ItemTemplate>
                    <asp:LinkButton runat="server" CssClass="filter-pill" Text='<%# Eval("LineName") %>'
                        OnClick="lnkFilter_Click" CommandArgument='<%# Eval("LineID") %>' CausesValidation="false"/>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <asp:Panel ID="pnlEmpty" runat="server">
            <div class="empty-msg">No remark options configured. Add one above.</div>
        </asp:Panel>
        <asp:Panel ID="pnlTable" runat="server" Visible="false">
        <table>
            <thead><tr>
                <th>Production Line</th>
                <th>Remark Text</th>
                <th style="text-align:center;">Order</th>
                <th>Status</th>
                <th style="text-align:center;">Actions</th>
            </tr></thead>
            <tbody>
                <asp:Repeater ID="rptRemarks" runat="server" OnItemCommand="rptRemarks_ItemCommand">
                    <ItemTemplate>
                    <tr style='<%# Convert.ToInt32(Eval("IsActive")) == 0 ? "opacity:.5;" : "" %>'>
                        <td><span class='line-badge line-<%# GetLineCode(Eval("LineID")) %>'><%# Eval("LineName") %></span></td>
                        <td style="font-weight:600;"><%# Eval("OptionText") %></td>
                        <td style="text-align:center;color:var(--text-muted);"><%# Eval("SortOrder") %></td>
                        <td><%# Convert.ToInt32(Eval("IsActive")) == 1 ? "<span class='status-active'>Active</span>" : "<span class='status-inactive'>Inactive</span>" %></td>
                        <td style="text-align:center;white-space:nowrap;">
                            <asp:LinkButton runat="server" CommandName="EditOption" CommandArgument='<%# Eval("OptionID") %>'
                                CssClass="btn-sm btn-edit" CausesValidation="false">Edit</asp:LinkButton>
                            <asp:LinkButton runat="server" CommandName="ToggleOption" CommandArgument='<%# Eval("OptionID") %>'
                                CssClass="btn-sm" CausesValidation="false"><%# Convert.ToInt32(Eval("IsActive")) == 1 ? "Deactivate" : "Activate" %></asp:LinkButton>
                        </td>
                    </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
        </asp:Panel>
    </div>

</div>

<script src="/StockApp/erp-modal.js"></script>
<script src="/StockApp/erp-keepalive.js"></script>
</form>
</body>
</html>
