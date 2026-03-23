<%@ Page Language="C#" AutoEventWireup="true" Inherits="PKApp.PKCustomer" %>
<!DOCTYPE html><html lang="en"><head runat="server">
<meta charset="utf-8"/><meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Customer Master — Packing</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<style>
:root{--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--accent:#e67e22;--accent-dark:#d35400;--teal:#1a9e6a;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--radius:12px;}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);min-height:100vh;color:var(--text);}
nav{background:#1a1a1a;height:52px;display:flex;align-items:center;padding:0 24px;gap:12px;position:sticky;top:0;z-index:100;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;}
.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:17px;letter-spacing:.08em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:12px;}
.nav-user{color:rgba(255,255,255,.8);font-size:12px;}
.nav-link{color:#aaa;text-decoration:none;font-size:11px;font-weight:600;text-transform:uppercase;padding:5px 10px;border-radius:5px;}
.nav-link:hover{color:#fff;background:rgba(255,255,255,.08);}
.page-header{background:var(--surface);border-bottom:1px solid var(--border);padding:18px 32px;}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:26px;letter-spacing:.07em;}
.page-title span{color:var(--accent);}
.page-sub{font-size:12px;color:var(--text-muted);margin-top:2px;}
.layout{max-width:1100px;margin:24px auto;padding:0 28px;display:grid;grid-template-columns:380px 1fr;gap:20px;align-items:start;}
.card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:20px 22px;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:14px;letter-spacing:.08em;color:var(--text-muted);margin-bottom:14px;padding-bottom:10px;border-bottom:1px solid var(--border);}
.form-grid{display:grid;grid-template-columns:1fr 1fr;gap:10px;}
.form-group{display:flex;flex-direction:column;gap:4px;margin-bottom:10px;}
.form-group.full{grid-column:1/-1;}
label{font-size:11px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;color:var(--text-muted);}
input,select{width:100%;padding:9px 12px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:13px;color:var(--text);background:#fafafa;outline:none;}
input:focus,select:focus{border-color:var(--accent);background:#fff;}
input[readonly]{background:#f0f0f0;color:var(--text-dim);}
.form-actions{display:flex;gap:8px;margin-top:14px;}
.btn-primary{background:var(--accent);color:#fff;border:none;border-radius:8px;padding:9px 20px;font-size:12px;font-weight:700;cursor:pointer;}
.btn-primary:hover{background:var(--accent-dark);}
.btn-secondary{background:#f0f0f0;color:#333;border:1px solid var(--border);border-radius:8px;padding:8px 14px;font-size:12px;font-weight:600;cursor:pointer;}
.btn-danger{background:transparent;color:var(--accent);border:1px solid var(--accent);border-radius:8px;padding:5px 10px;font-size:11px;font-weight:700;cursor:pointer;}
.btn-danger:hover{background:var(--accent);color:#fff;}
.alert{padding:10px 14px;border-radius:8px;font-size:13px;font-weight:600;margin-bottom:12px;}
.alert-success{background:#eafaf1;color:var(--teal);border:1px solid #a9dfbf;}
.alert-danger{background:#fdf3f2;color:#e74c3c;border:1px solid #f5c6cb;}
.cust-list{display:flex;flex-direction:column;gap:8px;}
.cust-row{display:flex;align-items:center;justify-content:space-between;padding:10px 14px;background:#fafafa;border:1px solid var(--border);border-radius:9px;cursor:pointer;transition:border-color .2s;}
.cust-row:hover{border-color:var(--accent);}
.cust-row.inactive{opacity:.5;}
.cust-name{font-weight:700;font-size:13px;}
.cust-meta{font-size:11px;color:var(--text-dim);}
.badge{display:inline-block;padding:2px 8px;border-radius:10px;font-size:10px;font-weight:700;}
.badge-active{background:#eafaf1;color:var(--teal);}
.badge-inactive{background:#f0f0f0;color:var(--text-dim);}
.empty-note{text-align:center;padding:28px;color:var(--text-dim);font-size:13px;}
</style></head><body>
<form id="form1" runat="server">
<asp:HiddenField ID="hfCustID" runat="server" Value="0"/>
<nav>
    <a class="nav-logo" href="PKHome.aspx"><img src="Sirimiri_Logo-16_9-72ppi-01.png" alt="" onerror="this.style.display='none'"/></a>
    <span class="nav-title">Packing &amp; Shipments</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblUser" runat="server"/></span>
        <a href="/StockApp/ERPHome.aspx" class="nav-link">&#x2302; ERP Home</a>
        <a href="PKLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>
<div class="page-header">
    <div class="page-title">Customer <span>Master</span></div>
    <div class="page-sub">Add and manage customers and distributors for shipments</div>
</div>
<div class="layout">
    <div>
        <asp:Panel ID="pnlAlert" runat="server" Visible="false">
            <div class="alert"><asp:Label ID="lblAlert" runat="server"/></div>
        </asp:Panel>
        <div class="card">
            <div class="card-title"><asp:Label ID="lblFormTitle" runat="server">New Customer</asp:Label></div>
            <div class="form-group full">
                <label>Customer Code</label>
                <asp:TextBox ID="txtCode" runat="server" ReadOnly="true" placeholder="Auto-generated"/>
            </div>
            <div class="form-group full">
                <label>Customer Name <span style="color:var(--accent)">*</span></label>
                <asp:TextBox ID="txtName" runat="server" MaxLength="150"/>
            </div>
            <div class="form-grid">
                <div class="form-group"><label>Contact Person</label><asp:TextBox ID="txtContact" runat="server" MaxLength="100"/></div>
                <div class="form-group"><label>Phone</label><asp:TextBox ID="txtPhone" runat="server" MaxLength="20"/></div>
                <div class="form-group"><label>Email</label><asp:TextBox ID="txtEmail" runat="server" MaxLength="100"/></div>
                <div class="form-group"><label>GSTIN</label><asp:TextBox ID="txtGSTIN" runat="server" MaxLength="20"/></div>
                <div class="form-group"><label>City</label><asp:TextBox ID="txtCity" runat="server" MaxLength="80"/></div>
                <div class="form-group"><label>State</label><asp:TextBox ID="txtState" runat="server" MaxLength="80"/></div>
            </div>
            <div class="form-group full"><label>Address</label><asp:TextBox ID="txtAddress" runat="server" TextMode="MultiLine" Rows="2" MaxLength="300"/></div>
            <div class="form-actions">
                <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn-primary" OnClick="btnSave_Click" CausesValidation="false"/>
                <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn-secondary" OnClick="btnClear_Click" CausesValidation="false"/>
                <asp:Button ID="btnToggle" runat="server" Text="Deactivate" CssClass="btn-danger" OnClick="btnToggle_Click" CausesValidation="false" Visible="false"/>
            </div>
        </div>
    </div>
    <div>
        <div class="card">
            <div class="card-title">Customers (<asp:Label ID="lblCount" runat="server"/>)</div>
            <asp:Panel ID="pnlEmpty" runat="server"><div class="empty-note">No customers yet</div></asp:Panel>
            <div class="cust-list">
                <asp:Repeater ID="rptList" runat="server" OnItemCommand="rptList_Cmd">
                    <ItemTemplate>
                        <div class='cust-row <%# !(bool)Eval("IsActive") ? "inactive" : "" %>'>
                            <div>
                                <div class="cust-name"><%# Eval("CustomerName") %></div>
                                <div class="cust-meta"><%# Eval("CustomerCode") %> &nbsp;·&nbsp; <%# Eval("City") %></div>
                            </div>
                            <div style="display:flex;gap:8px;align-items:center;">
                                <span class='badge <%# (bool)Eval("IsActive") ? "badge-active" : "badge-inactive" %>'><%# (bool)Eval("IsActive") ? "Active" : "Inactive" %></span>
                                <asp:LinkButton runat="server" CommandName="Edit" CommandArgument='<%# Eval("CustomerID") %>' CssClass="btn-secondary" style="font-size:11px;padding:4px 10px;" CausesValidation="false">Edit</asp:LinkButton>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>
</div>
</form></body></html>
