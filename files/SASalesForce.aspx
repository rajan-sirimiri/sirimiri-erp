<%@ Page Language="C#" AutoEventWireup="true" EnableEventValidation="false"
    CodeBehind="SASalesForce.aspx.cs" Inherits="StockApp.SASalesForce" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="UTF-8"/>
<meta name="viewport" content="width=device-width, initial-scale=1.0"/>
<title>Sales Force Order Platform — Sirimiri ERP</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:ital,wght@0,300;0,400;0,500;0,600;1,300&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--bg:#f5f5f5;--surface:#fff;--surface2:#f9f9f9;--border:#e0e0e0;--accent:#2980b9;--accent-dark:#2471a3;--teal:#1a9e6a;--red:#c0392b;--orange:#e07b00;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--radius:12px;}
*,*::before,*::after{box-sizing:border-box;margin:0;padding:0;}
body{background:var(--bg);color:var(--text);font-family:'DM Sans',sans-serif;min-height:100vh;}
header{display:flex;align-items:center;padding:0 40px;height:72px;background:var(--surface);border-bottom:2px solid var(--accent);box-shadow:0 2px 8px rgba(0,0,0,.06);}
.header-logo img{height:48px;width:auto;object-fit:contain;}
.header-center{flex:1;text-align:center;}
.header-brand{font-family:'Bebas Neue',sans-serif;font-size:24px;letter-spacing:.10em;}
.header-tagline{font-size:10px;letter-spacing:.18em;text-transform:uppercase;color:var(--text-muted);margin-top:3px;}
.header-right{display:flex;align-items:center;gap:16px;}
.header-user-name{font-size:13px;font-weight:600;text-align:right;}
.header-user-role{font-size:11px;color:var(--text-muted);text-transform:uppercase;letter-spacing:.06em;text-align:right;}
.btn-signout{padding:6px 14px;border:1.5px solid var(--border);border-radius:7px;color:var(--text-muted);font-size:12px;font-weight:700;text-decoration:none;letter-spacing:.04em;text-transform:uppercase;}
.btn-signout:hover{border-color:var(--accent);color:var(--accent);}
nav.sa-nav{background:linear-gradient(135deg,#1a1a1a 0%,#2980b9 100%);display:flex;align-items:center;padding:0 32px;gap:4px;box-shadow:0 2px 8px rgba(0,0,0,.2);}
.nav-item{display:block;padding:12px 16px;color:#fff;font-size:12px;font-weight:600;letter-spacing:.05em;text-transform:uppercase;text-decoration:none;transition:background .2s;}
.nav-item:hover,.nav-item.active{background:rgba(255,255,255,.18);}

.container{max-width:1100px;margin:24px auto;padding:0 24px;}
.card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:22px 26px;margin-bottom:20px;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:15px;letter-spacing:.08em;color:var(--text-muted);margin-bottom:16px;padding-bottom:10px;border-bottom:1px solid var(--border);display:flex;align-items:center;gap:8px;}
.card-title::before{content:'';display:inline-block;width:3px;height:14px;background:var(--accent);border-radius:2px;}

.form-row{display:flex;gap:14px;align-items:flex-end;flex-wrap:wrap;margin-bottom:16px;}
.form-group{display:flex;flex-direction:column;gap:4px;flex:1;min-width:150px;}
.form-group label{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);}
.form-group input,.form-group select{padding:9px 12px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:13px;background:#fafafa;outline:none;}
.form-group input:focus,.form-group select:focus{border-color:var(--accent);}
.req{color:var(--red);}
.btn{border:none;border-radius:8px;padding:10px 20px;font-size:12px;font-weight:700;letter-spacing:.06em;cursor:pointer;font-family:inherit;}
.btn-primary{background:var(--accent);color:#fff;}.btn-primary:hover{background:var(--accent-dark);}
.btn-teal{background:var(--teal);color:#fff;}.btn-teal:hover{background:#148a5b;}
.btn-cancel{background:#f0f0f0;color:#333;border:1px solid #ddd;}
.btn-sm{padding:6px 14px;font-size:11px;}
.alert{padding:12px 16px;border-radius:8px;font-size:13px;margin-bottom:16px;}
.alert-success{background:#eafaf1;color:var(--teal);border:1px solid #b2dfdb;}
.alert-danger{background:#fdf3f2;color:var(--red);border:1px solid #f5c6cb;}

/* Tabs */
.tab-bar{display:flex;gap:0;background:var(--surface);border:1px solid var(--border);border-radius:var(--radius) var(--radius) 0 0;overflow:hidden;}
.tab-btn{padding:14px 24px;font-size:12px;font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:var(--text-muted);cursor:pointer;border:none;background:none;border-bottom:3px solid transparent;font-family:inherit;}
.tab-btn:hover{color:var(--text);background:var(--surface2);}
.tab-btn.active{color:var(--accent);border-bottom-color:var(--accent);}

/* Month/Year bar */
.month-bar{display:flex;align-items:center;gap:12px;margin-bottom:20px;padding:14px 20px;background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);}
.month-bar label{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);}
.month-display{font-family:'Bebas Neue',sans-serif;font-size:22px;letter-spacing:.06em;color:var(--accent);flex:1;}

table{width:100%;border-collapse:collapse;font-size:13px;}
th{background:var(--surface2);font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);padding:10px 12px;text-align:left;border-bottom:2px solid var(--border);}
td{padding:9px 12px;border-bottom:1px solid #f0f0f0;vertical-align:middle;}
tr:hover{background:rgba(41,128,185,0.04);}
.badge{display:inline-block;padding:2px 8px;border-radius:10px;font-size:10px;font-weight:700;}
.badge-draft{background:#fff3cd;color:#856404;}
.badge-confirmed{background:#eafaf1;color:var(--teal);}
.badge-shipped{background:#e8f4fd;color:var(--accent);}

/* Line items */
.line-items{margin-top:12px;}
.line-row{display:flex;gap:10px;align-items:center;margin-bottom:8px;padding:8px 12px;background:var(--surface2);border-radius:8px;}
.line-row select,.line-row input{padding:7px 10px;border:1px solid var(--border);border-radius:6px;font-size:12px;font-family:inherit;}
.line-row select{flex:2;}
.line-row input[type="number"]{flex:1;max-width:100px;}
.line-remove{background:none;border:none;color:var(--red);cursor:pointer;font-size:16px;font-weight:700;padding:4px 8px;}

@media(max-width:768px){
    header{padding:0 16px;height:60px;}
    .header-brand{font-size:18px;}
    nav.sa-nav{padding:0 16px;}
    .container{padding:0 12px;}
    .form-row{flex-direction:column;}
    .line-row{flex-wrap:wrap;}
}
</style>
</head>
<body>
<form id="form1" runat="server">

<header>
    <div class="header-logo"><img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/></div>
    <div class="header-center">
        <div class="header-brand">Sirimiri Nutrition Food Products</div>
        <div class="header-tagline">Enterprise Resource Planning</div>
    </div>
    <div class="header-right">
        <div>
            <div class="header-user-name"><asp:Label ID="lblUserName" runat="server"/></div>
            <div class="header-user-role"><asp:Label ID="lblUserRole" runat="server"/></div>
        </div>
        <a href="#" class="btn-signout" onclick="erpConfirm('Sign out?',{title:'Sign Out',type:'warn',okText:'Sign Out',onOk:function(){window.location='Logout.aspx';}});return false;">&#x2192; Sign Out</a>
    </div>
</header>

<nav class="sa-nav">
    <a href="SAHome.aspx" class="nav-item">&#x2302; SA Home</a>
    <span class="nav-item active">&#x1F4F1; Sales Force Order Platform</span>
</nav>

<div class="container">

<asp:Panel ID="pnlAlert" runat="server" Visible="false" CssClass="alert"><asp:Label ID="lblAlert" runat="server"/></asp:Panel>

<!-- MONTH / YEAR SELECTOR (common for both tabs) -->
<div class="month-bar">
    <label>Month</label>
    <asp:DropDownList ID="ddlMonth" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlMonth_Changed">
        <asp:ListItem Value="1">January</asp:ListItem><asp:ListItem Value="2">February</asp:ListItem>
        <asp:ListItem Value="3">March</asp:ListItem><asp:ListItem Value="4">April</asp:ListItem>
        <asp:ListItem Value="5">May</asp:ListItem><asp:ListItem Value="6">June</asp:ListItem>
        <asp:ListItem Value="7">July</asp:ListItem><asp:ListItem Value="8">August</asp:ListItem>
        <asp:ListItem Value="9">September</asp:ListItem><asp:ListItem Value="10">October</asp:ListItem>
        <asp:ListItem Value="11">November</asp:ListItem><asp:ListItem Value="12">December</asp:ListItem>
    </asp:DropDownList>
    <label>Year</label>
    <asp:DropDownList ID="ddlYear" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlYear_Changed"/>
    <div class="month-display"><asp:Label ID="lblMonthYear" runat="server"/></div>
</div>

<asp:HiddenField ID="hfTab" runat="server" Value="projection"/>

<!-- TABS -->
<div class="tab-bar">
    <asp:Button ID="btnTabProjection" runat="server" Text="&#x1F4CA; Projection" CssClass="tab-btn active" OnClick="btnTab_Click" CommandArgument="projection"/>
    <asp:Button ID="btnTabShipments" runat="server" Text="&#x1F69A; Shipments & Ordering" CssClass="tab-btn" OnClick="btnTab_Click" CommandArgument="shipments"/>
</div>

<!-- ═══════ TAB 1: PROJECTION ═══════ -->
<asp:Panel ID="pnlProjection" runat="server">

<div class="card" style="border-top-left-radius:0;border-top-right-radius:0;">
    <div class="card-title">Add / Edit Projection</div>
    <div class="form-row">
        <div class="form-group"><label>State <span class="req">*</span></label>
            <asp:DropDownList ID="ddlProjState" runat="server"/></div>
        <div class="form-group"><label>Channel <span class="req">*</span></label>
            <asp:DropDownList ID="ddlProjChannel" runat="server"/></div>
        <div class="form-group" style="flex:0;">
            <label>&nbsp;</label>
            <asp:Button ID="btnLoadProjection" runat="server" Text="Load" CssClass="btn btn-primary btn-sm" OnClick="btnLoadProjection_Click"/>
        </div>
    </div>

    <asp:Panel ID="pnlProjLines" runat="server" Visible="false">
        <div class="card-title">Products — <asp:Label ID="lblProjContext" runat="server"/></div>
        <div class="line-items" id="divProjLines">
            <asp:Repeater ID="rptProjLines" runat="server">
                <ItemTemplate>
                    <div class="line-row">
                        <select name="proj_product">
                            <option value="0">-- Select Product --</option>
                            <asp:Literal ID="litProductOptions" runat="server"/>
                        </select>
                        <input type="number" name="proj_qty" min="0" step="1" value='<%# Eval("Quantity") %>' placeholder="Qty"/>
                        <input type="hidden" name="proj_lineid" value='<%# Eval("LineID") %>'/>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
        <div style="margin-top:8px;">
            <button type="button" class="btn btn-sm" style="background:#f0f0f0;color:#333;border:1px solid #ddd;" onclick="addProjLine();">+ Add Product</button>
        </div>
        <div style="margin-top:16px;display:flex;gap:10px;">
            <asp:Button ID="btnSaveProjection" runat="server" Text="&#x1F4BE; Save Projection" CssClass="btn btn-teal" OnClick="btnSaveProjection_Click"/>
        </div>
    </asp:Panel>
</div>

<!-- Existing projections for this month -->
<div class="card">
    <div class="card-title">Projections for <asp:Label ID="lblProjMonth" runat="server"/></div>
    <asp:Panel ID="pnlProjEmpty" runat="server"><div style="text-align:center;padding:20px;color:var(--text-dim);font-size:13px;">No projections yet for this month.</div></asp:Panel>
    <asp:Repeater ID="rptProjections" runat="server">
        <HeaderTemplate><table><tr><th>State</th><th>Channel</th><th>Products</th><th>Total Units</th><th>Status</th><th>Actions</th></tr></HeaderTemplate>
        <ItemTemplate>
            <tr>
                <td style="font-weight:500;"><%# Eval("StateName") %></td>
                <td><%# Eval("ChannelName") %></td>
                <td><%# Eval("ProductCount") %></td>
                <td style="font-weight:600;"><%# Eval("TotalQty") %></td>
                <td><span class='badge <%# Eval("Status").ToString()=="Confirmed" ? "badge-confirmed" : "badge-draft" %>'><%# Eval("Status") %></span></td>
                <td>
                    <asp:LinkButton runat="server" Text="Edit" CommandName="EditProj" CommandArgument='<%# Eval("ProjectionID") %>' OnCommand="ProjAction_Command"
                        style="color:var(--accent);font-size:11px;font-weight:700;text-decoration:none;margin-right:8px;"/>
                    <asp:LinkButton runat="server" Text="Confirm" CommandName="ConfirmProj" CommandArgument='<%# Eval("ProjectionID") %>' OnCommand="ProjAction_Command"
                        Visible='<%# Eval("Status").ToString()=="Draft" %>'
                        style="color:var(--teal);font-size:11px;font-weight:700;text-decoration:none;"/>
                </td>
            </tr>
        </ItemTemplate>
        <FooterTemplate></table></FooterTemplate>
    </asp:Repeater>
</div>

</asp:Panel>

<!-- ═══════ TAB 2: SHIPMENTS & ORDERING ═══════ -->
<asp:Panel ID="pnlShipments" runat="server" Visible="false">

<div class="card" style="border-top-left-radius:0;border-top-right-radius:0;">
    <div class="card-title">Create Shipment</div>
    <div class="form-row">
        <div class="form-group"><label>Date of Shipment <span class="req">*</span></label>
            <asp:TextBox ID="txtShipDate" runat="server" TextMode="Date"/></div>
        <div class="form-group"><label>State <span class="req">*</span></label>
            <asp:DropDownList ID="ddlShipState" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlShipState_Changed"/></div>
        <div class="form-group"><label>Channel <span class="req">*</span></label>
            <asp:DropDownList ID="ddlShipChannel" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlShipChannel_Changed"/></div>
    </div>
    <div class="form-row">
        <div class="form-group"><label>Mode of Transport</label>
            <asp:DropDownList ID="ddlTransport" runat="server"/></div>
        <div class="form-group"><label>Vehicle No</label>
            <asp:TextBox ID="txtVehicleNo" runat="server" placeholder="e.g. TN 01 AB 1234"/></div>
        <div class="form-group"><label>Remarks</label>
            <asp:TextBox ID="txtShipRemarks" runat="server" placeholder="Optional notes"/></div>
    </div>

    <!-- Products pulled from projection -->
    <asp:Panel ID="pnlShipLines" runat="server" Visible="false">
        <div class="card-title">Products (from Projection)</div>
        <table>
            <tr><th>Product</th><th>Projected Qty</th><th>Ship Qty</th></tr>
            <asp:Repeater ID="rptShipLines" runat="server">
                <ItemTemplate>
                    <tr>
                        <td style="font-weight:500;"><%# Eval("ProductName") %></td>
                        <td><%# Eval("Quantity") %></td>
                        <td><input type="number" name="ship_qty" min="0" step="1" value='<%# Eval("Quantity") %>' style="width:80px;padding:6px 8px;border:1px solid var(--border);border-radius:6px;font-size:12px;"/>
                            <input type="hidden" name="ship_productid" value='<%# Eval("ProductID") %>'/>
                            <input type="hidden" name="ship_projqty" value='<%# Eval("Quantity") %>'/>
                        </td>
                    </tr>
                </ItemTemplate>
            </asp:Repeater>
        </table>
        <div style="margin-top:16px;">
            <asp:Button ID="btnCreateShipment" runat="server" Text="&#x1F69A; Create Shipment" CssClass="btn btn-primary" OnClick="btnCreateShipment_Click"/>
        </div>
    </asp:Panel>
    <asp:Panel ID="pnlNoProjection" runat="server" Visible="false">
        <div style="padding:16px;color:var(--text-dim);font-size:13px;text-align:center;">
            No projection found for selected State + Channel this month. Create a projection first.
        </div>
    </asp:Panel>
</div>

<!-- Recent Shipments -->
<div class="card">
    <div class="card-title">Shipments for <asp:Label ID="lblShipMonth" runat="server"/></div>
    <asp:Panel ID="pnlShipEmpty" runat="server"><div style="text-align:center;padding:20px;color:var(--text-dim);font-size:13px;">No shipments yet.</div></asp:Panel>
    <asp:Repeater ID="rptShipments" runat="server">
        <HeaderTemplate><table><tr><th>Date</th><th>State</th><th>Channel</th><th>Transport</th><th>Products</th><th>Status</th></tr></HeaderTemplate>
        <ItemTemplate>
            <tr>
                <td><%# Convert.ToDateTime(Eval("ShipmentDate")).ToString("dd-MMM") %></td>
                <td><%# Eval("StateName") %></td>
                <td><%# Eval("ChannelName") %></td>
                <td><%# Eval("TransportMode") %></td>
                <td><%# Eval("ProductCount") %> items</td>
                <td><span class='badge <%# Eval("Status").ToString()=="Shipped" ? "badge-shipped" : "badge-draft" %>'><%# Eval("Status") %></span></td>
            </tr>
        </ItemTemplate>
        <FooterTemplate></table></FooterTemplate>
    </asp:Repeater>
</div>

</asp:Panel>

</div>

<asp:HiddenField ID="hfProductOptionsHtml" runat="server"/>
<asp:HiddenField ID="hfEditProjId" runat="server" Value="0"/>

<script>
function addProjLine() {
    var opts = document.getElementById('<%= hfProductOptionsHtml.ClientID %>').value;
    var div = document.getElementById('divProjLines');
    var row = document.createElement('div');
    row.className = 'line-row';
    row.innerHTML = '<select name="proj_product"><option value="0">-- Select Product --</option>' + opts + '</select>'
        + '<input type="number" name="proj_qty" min="0" step="1" placeholder="Qty"/>'
        + '<input type="hidden" name="proj_lineid" value="0"/>'
        + '<button type="button" class="line-remove" onclick="this.parentNode.remove();">&#x2715;</button>';
    div.appendChild(row);
}
</script>
<script src="/StockApp/erp-modal.js"></script>
<script src="/StockApp/erp-keepalive.js"></script>
</form>
</body>
</html>
