<%@ Page Language="C#" AutoEventWireup="true" EnableEventValidation="false" ValidateRequest="false"
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
:root{--bg:#f5f5f5;--surface:#fff;--surface2:#f9f9f9;--border:#e0e0e0;--accent:#2980b9;--accent-dark:#2471a3;--teal:#1a9e6a;--red:#c0392b;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--radius:12px;}
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
nav.sa-nav{background:linear-gradient(135deg,#1a1a1a 0%,#2980b9 100%);display:flex;align-items:center;padding:0 32px;gap:4px;box-shadow:0 2px 8px rgba(0,0,0,.2);}
.nav-item{display:block;padding:12px 16px;color:#fff;font-size:12px;font-weight:600;letter-spacing:.05em;text-transform:uppercase;text-decoration:none;transition:background .2s;}
.nav-item:hover,.nav-item.active{background:rgba(255,255,255,.18);}
.container{max-width:1100px;margin:24px auto;padding:0 24px;}
.card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:22px 26px;margin-bottom:20px;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:15px;letter-spacing:.08em;color:var(--text-muted);margin-bottom:16px;padding-bottom:10px;border-bottom:1px solid var(--border);display:flex;align-items:center;gap:8px;}
.card-title::before{content:'';display:inline-block;width:3px;height:14px;background:var(--accent);border-radius:2px;}
.form-row{display:flex;gap:14px;align-items:flex-end;flex-wrap:wrap;margin-bottom:16px;}
.form-group{display:flex;flex-direction:column;gap:4px;flex:1;min-width:140px;}
.form-group label{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);}
.form-group input,.form-group select{padding:9px 12px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:13px;background:#fafafa;outline:none;}
.form-group input:focus,.form-group select:focus{border-color:var(--accent);}
.req{color:var(--red);}
.btn{border:none;border-radius:8px;padding:10px 20px;font-size:12px;font-weight:700;letter-spacing:.06em;cursor:pointer;font-family:inherit;}
.btn-primary{background:var(--accent);color:#fff;}.btn-primary:hover{background:var(--accent-dark);}
.btn-teal{background:var(--teal);color:#fff;}.btn-teal:hover{background:#148a5b;}
.btn-sm{padding:6px 14px;font-size:11px;}
.alert{padding:12px 16px;border-radius:8px;font-size:13px;margin-bottom:16px;}
.alert-success{background:#eafaf1;color:var(--teal);border:1px solid #b2dfdb;}
.alert-danger{background:#fdf3f2;color:var(--red);border:1px solid #f5c6cb;}
.tab-bar{display:flex;gap:0;background:var(--surface);border:1px solid var(--border);border-radius:var(--radius) var(--radius) 0 0;overflow:hidden;}
.tab-btn{padding:14px 24px;font-size:12px;font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:var(--text-muted);cursor:pointer;border:none;background:none;border-bottom:3px solid transparent;font-family:inherit;}
.tab-btn:hover{color:var(--text);background:var(--surface2);}.tab-btn.active{color:var(--accent);border-bottom-color:var(--accent);}
.month-bar{display:flex;align-items:center;gap:12px;margin-bottom:20px;padding:14px 20px;background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);}
.month-bar label{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);}
.month-display{font-family:'Bebas Neue',sans-serif;font-size:22px;letter-spacing:.06em;color:var(--accent);flex:1;}
table{width:100%;border-collapse:collapse;font-size:13px;}
th{background:var(--surface2);font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);padding:10px 12px;text-align:left;border-bottom:2px solid var(--border);}
td{padding:9px 12px;border-bottom:1px solid #f0f0f0;vertical-align:middle;}
tr:hover{background:rgba(41,128,185,0.04);}
.badge{display:inline-block;padding:2px 8px;border-radius:10px;font-size:10px;font-weight:700;}
.badge-draft{background:#fff3cd;color:#856404;}.badge-confirmed{background:#eafaf1;color:var(--teal);}.badge-shipped{background:#e8f4fd;color:var(--accent);}
.badge-saved{background:#f5f5f5;color:#666;}.badge-order{background:#d4edda;color:#155724;}
.line-row{display:flex;gap:10px;align-items:center;margin-bottom:8px;padding:8px 12px;background:var(--surface2);border-radius:8px;}
.line-row select,.line-row input{padding:7px 10px;border:1px solid var(--border);border-radius:6px;font-size:12px;font-family:inherit;}
.line-row .prod-sel{flex:3;min-width:200px;}.line-row .qty-inp{flex:1;max-width:100px;}.line-row .uom-sel{flex:1;max-width:120px;}
.line-remove{background:none;border:none;color:var(--red);cursor:pointer;font-size:16px;font-weight:700;padding:4px 8px;}
.sel-path{display:flex;gap:6px;align-items:center;flex-wrap:wrap;margin-bottom:14px;}
.sel-path .path-tag{padding:4px 10px;background:#e8f4fd;color:var(--accent);border-radius:6px;font-size:11px;font-weight:600;}
.sel-path .path-arrow{color:var(--text-dim);font-size:14px;}
@media(max-width:768px){header{padding:0 16px;height:60px;}nav.sa-nav{padding:0 16px;}.container{padding:0 12px;}.form-row{flex-direction:column;}.line-row{flex-wrap:wrap;}}
</style>
</head>
<body>
<form id="form1" runat="server">
<header>
    <div class="header-logo"><img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/></div>
    <div class="header-center"><div class="header-brand">Sirimiri Nutrition Food Products</div><div class="header-tagline">Enterprise Resource Planning</div></div>
    <div class="header-right"><div><div class="header-user-name"><asp:Label ID="lblUserName" runat="server"/></div><div class="header-user-role"><asp:Label ID="lblUserRole" runat="server"/></div></div>
    <a href="/StockApp/ChangePassword.aspx" class="btn-signout" style="background:transparent;border:1px solid rgba(255,255,255,.25);margin-right:6px;">&#x1F512; Change Password</a>
    <a href="#" class="btn-signout" onclick="erpConfirm('Sign out?',{title:'Sign Out',type:'warn',okText:'Sign Out',onOk:function(){window.location='Logout.aspx';}});return false;">&#x2192; Sign Out</a></div>
</header>
<nav class="sa-nav"><a href="SAHome.aspx" class="nav-item">&#x2302; SA Home</a><span class="nav-item active">&#x1F4F1; Sales Force Order Platform</span></nav>
<div class="container">
<asp:Panel ID="pnlAlert" runat="server" Visible="false" CssClass="alert"><asp:Label ID="lblAlert" runat="server"/></asp:Panel>
<div class="month-bar">
    <label>Month</label>
    <asp:DropDownList ID="ddlMonth" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlMonth_Changed">
        <asp:ListItem Value="1">January</asp:ListItem><asp:ListItem Value="2">February</asp:ListItem><asp:ListItem Value="3">March</asp:ListItem><asp:ListItem Value="4">April</asp:ListItem>
        <asp:ListItem Value="5">May</asp:ListItem><asp:ListItem Value="6">June</asp:ListItem><asp:ListItem Value="7">July</asp:ListItem><asp:ListItem Value="8">August</asp:ListItem>
        <asp:ListItem Value="9">September</asp:ListItem><asp:ListItem Value="10">October</asp:ListItem><asp:ListItem Value="11">November</asp:ListItem><asp:ListItem Value="12">December</asp:ListItem>
    </asp:DropDownList>
    <label>Year</label><asp:DropDownList ID="ddlYear" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlYear_Changed"/>
    <div class="month-display"><asp:Label ID="lblMonthYear" runat="server"/></div>
</div>
<asp:HiddenField ID="hfTab" runat="server" Value="projection"/>
<div class="tab-bar">
    <asp:Button ID="btnTabProjection" runat="server" Text="&#x1F4CA; Projection" CssClass="tab-btn active" OnClick="btnTab_Click" CommandArgument="projection"/>
    <asp:Button ID="btnTabConsignments" runat="server" Text="&#x1F4E6; Consignments" CssClass="tab-btn" OnClick="btnTab_Click" CommandArgument="consignments"/>
</div>

<!-- ═══════ TAB 1: PROJECTION ═══════ -->
<asp:Panel ID="pnlProjection" runat="server">
<div class="card" style="border-top-left-radius:0;border-top-right-radius:0;">
    <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:16px;">
        <div class="card-title" style="margin-bottom:0;padding-bottom:0;border-bottom:none;">Projection</div>
        <asp:Button ID="btnNewProjection" runat="server" Text="+ Create New Projection" CssClass="btn btn-primary btn-sm" OnClick="btnNewProjection_Click"/>
    </div>

    <asp:Panel ID="pnlProjForm" runat="server" Visible="false">
    <!-- ROW 1: Area + Channel -->
    <div class="form-row">
        <div class="form-group"><label>Area <span class="req">*</span></label>
            <asp:DropDownList ID="ddlProjArea" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlProjArea_Changed"/></div>
        <div class="form-group"><label>Channel <span class="req">*</span></label><asp:DropDownList ID="ddlProjChannel" runat="server"/></div>
    </div>

    <!-- Zone & Region auto-resolved from Area -->
    <asp:Panel ID="pnlProjZoneRegion" runat="server" Visible="false">
    <div style="display:flex;gap:16px;margin-bottom:16px;padding:10px 16px;background:#f0f8ff;border:1px solid #c2ddf5;border-radius:8px;">
        <div style="font-size:11px;"><span style="font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:var(--text-dim);">Zone:</span>
            <span style="font-weight:600;color:var(--accent);margin-left:4px;"><asp:Label ID="lblProjZone" runat="server" Text="—"/></span></div>
        <div style="font-size:11px;"><span style="font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:var(--text-dim);">Region:</span>
            <span style="font-weight:600;color:var(--accent);margin-left:4px;"><asp:Label ID="lblProjRegion" runat="server" Text="—"/></span></div>
    </div>
    </asp:Panel>
    <asp:HiddenField ID="hfProjZoneID" runat="server" Value="0"/>
    <asp:HiddenField ID="hfProjRegionID" runat="server" Value="0"/>

    <!-- Products - always visible when form is open -->
    <asp:Panel ID="pnlProjLines" runat="server" Visible="true">
        <div class="card-title">Products</div>
        <div id="divProjLines">
            <asp:Repeater ID="rptProjLines" runat="server">
                <ItemTemplate>
                    <div class="line-row">
                        <select name="proj_product" class="prod-sel"><option value="0">-- Select Product --</option><asp:Literal ID="litProductOptions" runat="server"/></select>
                        <input type="number" name="proj_qty" class="qty-inp" min="0" step="1" value='<%# Eval("Quantity") %>' placeholder="Qty"/>
                        <select name="proj_uom" class="uom-sel"><asp:Literal ID="litUOMOptions" runat="server"/></select>
                        <input type="hidden" name="proj_lineid" value='<%# Eval("LineID") %>'/>
                        <button type="button" class="line-remove" onclick="this.parentNode.remove();">&#x2715;</button>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
        <div style="margin-top:8px;"><button type="button" class="btn btn-sm" style="background:#f0f0f0;color:#333;border:1px solid #ddd;" onclick="addProjLine();">+ Add Product</button></div>
        <div style="margin-top:16px;display:flex;gap:10px;"><asp:Button ID="btnSaveProjection" runat="server" Text="&#x1F4BE; Save Projection" CssClass="btn btn-teal" OnClick="btnSaveProjection_Click"/></div>
    </asp:Panel>
    </asp:Panel>
</div>
<div class="card">
    <div class="card-title">Projections for <asp:Label ID="lblProjMonth" runat="server"/></div>
    <asp:Panel ID="pnlProjEmpty" runat="server"><div style="text-align:center;padding:20px;color:var(--text-dim);font-size:13px;">No projections yet for this month.</div></asp:Panel>
    <asp:Repeater ID="rptProjections" runat="server">
        <HeaderTemplate><table><tr><th>Zone</th><th>Region</th><th>Area (ASM)</th><th>Channel</th><th>Products</th><th>Total Units</th><th>Status</th><th>Actions</th></tr></HeaderTemplate>
        <ItemTemplate><tr>
            <td style="font-size:11px;"><%# Eval("ZoneName") %></td><td style="font-size:11px;"><%# Eval("RegionName") %></td>
            <td style="font-weight:500;"><%# Eval("AreaName") %></td><td><%# Eval("ChannelName") %></td>
            <td><%# Eval("ProductCount") %></td><td style="font-weight:600;"><%# Eval("TotalQty") %></td>
            <td><span class='badge <%# Eval("Status").ToString()=="Confirmed"?"badge-confirmed":"badge-draft" %>'><%# Eval("Status") %></span></td>
            <td><asp:LinkButton runat="server" Text="Edit" CommandName="EditProj" CommandArgument='<%# Eval("ProjectionID") %>' OnCommand="ProjAction_Command" style="color:var(--accent);font-size:11px;font-weight:700;text-decoration:none;margin-right:8px;"/>
                <asp:LinkButton runat="server" Text="Confirm" CommandName="ConfirmProj" CommandArgument='<%# Eval("ProjectionID") %>' OnCommand="ProjAction_Command" Visible='<%# Eval("Status").ToString()=="Draft" %>' style="color:var(--teal);font-size:11px;font-weight:700;text-decoration:none;margin-right:8px;"/>
                <asp:LinkButton runat="server" Text="Delete" CommandName="DeleteProj" CommandArgument='<%# Eval("ProjectionID") %>' OnCommand="ProjAction_Command"
                    OnClientClick="var href=this.getAttribute('href'); erpConfirm('Delete this projection?',{title:'Delete Projection',type:'warn',okText:'Delete',onOk:function(){eval(href);}});return false;"
                    style="color:var(--red);font-size:11px;font-weight:700;text-decoration:none;"/></td>
        </tr></ItemTemplate>
        <FooterTemplate></table></FooterTemplate>
    </asp:Repeater>
</div>
</asp:Panel>

<!-- ═══════ TAB 2: SHIPMENTS ═══════ -->
<asp:Panel ID="pnlShipments" runat="server" Visible="false">
<div class="card" style="border-top-left-radius:0;border-top-right-radius:0;">
    <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:16px;">
        <div class="card-title" style="margin-bottom:0;padding-bottom:0;border-bottom:none;">Shipment <asp:Label ID="lblEditingShipId" runat="server" style="color:var(--accent);"/></div>
        <asp:Button ID="btnNewShipment" runat="server" Text="+ Create New Shipment" CssClass="btn btn-primary btn-sm" OnClick="btnNewShipment_Click"/>
    </div>

    <asp:Panel ID="pnlShipForm" runat="server" Visible="false">

    <!-- ROW 1: Date, Area, Channel -->
    <div class="form-row">
        <div class="form-group"><label>Shipment Date <span class="req">*</span></label><asp:TextBox ID="txtShipDate" runat="server" TextMode="Date"/></div>
        <div class="form-group"><label>Area <span class="req">*</span></label><asp:DropDownList ID="ddlShipArea" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlShipArea_Changed"/></div>
        <div class="form-group"><label>Channel <span class="req">*</span></label><asp:DropDownList ID="ddlShipChannel" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlShipChannel_Changed"/></div>
    </div>

    <!-- ROW 2: Zone & Region (auto-resolved, read-only display) -->
    <asp:Panel ID="pnlZoneRegionInfo" runat="server" Visible="false">
    <div style="display:flex;gap:16px;margin-bottom:16px;padding:10px 16px;background:#f0f8ff;border:1px solid #c2ddf5;border-radius:8px;">
        <div style="font-size:11px;"><span style="font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:var(--text-dim);">Zone:</span>
            <span style="font-weight:600;color:var(--accent);margin-left:4px;"><asp:Label ID="lblShipZone" runat="server" Text="—"/></span></div>
        <div style="font-size:11px;"><span style="font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:var(--text-dim);">Region:</span>
            <span style="font-weight:600;color:var(--accent);margin-left:4px;"><asp:Label ID="lblShipRegion" runat="server" Text="—"/></span></div>
    </div>
    </asp:Panel>

    <!-- ROW 3: Transport -->
    <div class="form-row">
        <div class="form-group"><label>Mode of Transport</label><asp:DropDownList ID="ddlTransport" runat="server"/></div>
        <div class="form-group"><label>Vehicle No</label><asp:TextBox ID="txtVehicleNo" runat="server" placeholder="e.g. TN 01 AB 1234"/></div>
    </div>

    <!-- ROW 4: Customer -->
    <div class="form-row">
        <div class="form-group" style="flex:2;">
            <label>Customer</label>
            <div style="position:relative;">
                <input type="text" id="txtCustSearch" placeholder="Click to select customer..." autocomplete="off"
                    onfocus="this.blur();openCustModal();"
                    readonly="readonly"
                    style="width:100%;padding:9px 12px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:13px;background:#fafafa;outline:none;cursor:pointer;"/>
                <asp:DropDownList ID="ddlCustomer" runat="server" style="display:none;"/>
            </div>
        </div>
    </div>

    <!-- Hidden fields for resolved Zone/Region IDs -->
    <asp:HiddenField ID="hfShipZoneID" runat="server" Value="0"/>
    <asp:HiddenField ID="hfShipRegionID" runat="server" Value="0"/>

    <asp:Panel ID="pnlShipLines" runat="server" Visible="true">
        <div class="card-title">Products</div>
        <div id="divShipLines">
            <asp:Repeater ID="rptShipLines" runat="server" OnItemDataBound="rptShipLines_ItemDataBound">
                <ItemTemplate>
                    <div class="line-row">
                        <select name="ship_product" class="prod-sel">
                            <option value="0">-- Select Product --</option>
                            <asp:Literal ID="litShipProductOptions" runat="server"/>
                        </select>
                        <input type="number" name="ship_qty" class="qty-inp" min="0" step="1" value='<%# Eval("Quantity") %>' placeholder="Qty"/>
                        <select name="ship_uom" class="uom-sel"><asp:Literal ID="litShipUOMOptions" runat="server"/></select>
                        <input type="hidden" name="ship_productid" value='<%# Eval("ProductID") %>'/>
                        <button type="button" class="line-remove" onclick="this.parentNode.remove();">&#x2715;</button>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
        <div style="margin-top:8px;"><button type="button" class="btn btn-sm" style="background:#f0f0f0;color:#333;border:1px solid #ddd;" onclick="addShipLine();">+ Add Product</button></div>
        <div style="margin-top:16px;display:flex;gap:10px;">
            <asp:Button ID="btnSaveShipment" runat="server" Text="&#x1F4BE; Save Shipment" CssClass="btn btn-sm" style="background:#f0f0f0;color:#333;border:1px solid #ddd;" OnClick="btnSaveShipment_Click"/>
            <asp:Button ID="btnCreateShipment" runat="server" Text="&#x1F69A; Create Shipment Order" CssClass="btn btn-primary" OnClick="btnCreateShipment_Click"/>
        </div>
    </asp:Panel>
    </asp:Panel>
</div>
<div class="card">
    <div class="card-title">Shipments for <asp:Label ID="lblShipMonth" runat="server"/></div>
    <asp:Panel ID="pnlShipEmpty" runat="server"><div style="text-align:center;padding:20px;color:var(--text-dim);font-size:13px;">No shipments yet.</div></asp:Panel>
    <asp:Repeater ID="rptShipments" runat="server">
        <HeaderTemplate><table><tr><th>Ship #</th><th>Date</th><th>Customer</th><th>Area</th><th>Zone</th><th>Region</th><th>Channel</th><th>Transport</th><th>Items</th><th>Status</th><th></th></tr></HeaderTemplate>
        <ItemTemplate><tr><td style="font-family:monospace;font-weight:600;color:var(--accent);">SH-<%# Eval("ShipmentID").ToString().PadLeft(5,'0') %></td>
            <td><%# Convert.ToDateTime(Eval("ShipmentDate")).ToString("dd-MMM") %></td>
            <td style="font-weight:500;"><%# Eval("CustomerName") %></td>
            <td style="font-weight:500;"><%# Eval("AreaName") %></td>
            <td style="font-size:11px;"><%# Eval("ZoneName") %></td><td style="font-size:11px;"><%# Eval("RegionName") %></td>
            <td><%# Eval("ChannelName") %></td><td style="font-size:11px;"><%# Eval("TransportMode") %></td>
            <td><%# Eval("ProductCount") %></td>
            <td><span class='badge <%# GetShipStatusBadge(Eval("Status").ToString()) %>'><%# Eval("Status") %></span></td>
            <td><asp:LinkButton runat="server" Text="Edit" CommandName="EditShip" CommandArgument='<%# Eval("ShipmentID") %>' OnCommand="ShipAction_Command"
                Visible='<%# Eval("Status").ToString() == "Saved" || Eval("Status").ToString() == "Order" %>'
                style="color:var(--accent);font-size:11px;font-weight:700;text-decoration:none;margin-right:8px;"/>
                <asp:LinkButton runat="server" Text="Delete" CommandName="DeleteShip" CommandArgument='<%# Eval("ShipmentID") %>' OnCommand="ShipAction_Command"
                Visible='<%# Eval("Status").ToString() == "Saved" || Eval("Status").ToString() == "Order" %>'
                OnClientClick="var href=this.getAttribute('href'); erpConfirm('Delete this shipment?',{title:'Delete Shipment',type:'warn',okText:'Delete',onOk:function(){eval(href);}});return false;"
                style="color:var(--red);font-size:11px;font-weight:700;text-decoration:none;"/></td>
        </tr></ItemTemplate>
        <FooterTemplate></table></FooterTemplate>
    </asp:Repeater>
</div>
</asp:Panel>

<!-- ══════ TAB: CONSIGNMENTS ══════ -->
<asp:Panel ID="pnlConsignments" runat="server" Visible="false">

<!-- Action bar with visible Create Consignment button -->
<div style="display:flex;justify-content:flex-end;align-items:center;margin-bottom:10px;">
    <button type="button" class="btn btn-primary btn-sm"
        onclick="var d=document.getElementById('divNewConsig');d.style.display=(d.style.display==='none'||d.style.display==='')?'block':'none';"
        style="padding:8px 14px;">+ Create Consignment</button>
</div>

<!-- Consignment Tab Bar -->
<div style="display:flex;justify-content:space-between;align-items:flex-end;margin-bottom:0;">
    <div class="tab-bar" style="display:flex;gap:0;border-bottom:2px solid var(--border);flex:1;overflow-x:auto;">
        <asp:Repeater ID="rptSAConsigTabs" runat="server">
            <ItemTemplate>
                <asp:Button runat="server" Text='<%# Eval("ConsignmentCode") %>'
                    CssClass='<%# "tab-btn" + (Eval("ConsignmentID").ToString() == hfSAConsigId.Value ? " active" : "") %>'
                    CommandName="OpenConsig" CommandArgument='<%# Eval("ConsignmentID") %>'
                    OnCommand="SAConsig_Command" CausesValidation="false" style="white-space:nowrap;font-size:11px;padding:10px 16px;" />
            </ItemTemplate>
        </asp:Repeater>
        <button type="button" class="tab-btn" onclick="var d=document.getElementById('divNewConsig');d.style.display=(d.style.display==='none'||d.style.display==='')?'block':'none';"
            style="color:var(--teal);font-size:16px;padding:8px 14px;" title="New Consignment">+</button>
    </div>
</div>

<!-- New Consignment Form (hidden) -->
<div id="divNewConsig" style="display:none;background:var(--surface2);border:1px solid var(--border);border-radius:0 0 8px 8px;padding:14px;margin-bottom:14px;">
    <div style="font-size:11px;font-weight:700;color:var(--text-muted);margin-bottom:8px;">CREATE CONSIGNMENT</div>
    <div style="display:grid;grid-template-columns:1fr 1fr auto;gap:10px;align-items:end;">
        <div><label style="font-size:10px;font-weight:700;text-transform:uppercase;color:var(--text-muted);">Date</label>
            <asp:TextBox ID="txtSAConsigDate" runat="server" TextMode="Date" style="width:100%;padding:8px;border:1px solid var(--border);border-radius:6px;font-size:12px;"/></div>
        <div><label style="font-size:10px;font-weight:700;text-transform:uppercase;color:var(--text-muted);">Identifier (e.g. ROTN)</label>
            <asp:TextBox ID="txtSAConsigText" runat="server" placeholder="ROTN" MaxLength="30" style="width:100%;padding:8px;border:1px solid var(--border);border-radius:6px;font-size:12px;text-transform:uppercase;"/></div>
        <div><asp:Button ID="btnSACreateConsig" runat="server" Text="Create" CssClass="btn btn-primary" OnClick="btnSACreateConsig_Click" CausesValidation="false" style="font-size:11px;padding:8px 16px;"/></div>
    </div>
</div>

<!-- Consignment Content (when a tab is selected) -->
<asp:Panel ID="pnlSAConsigDetail" runat="server" Visible="false">

<!-- Consignment Header + Status -->
<div class="sa-card" style="padding:12px 16px;margin-bottom:10px;">
    <div style="display:flex;justify-content:space-between;align-items:center;">
        <div>
            <span style="font-size:14px;font-weight:700;"><asp:Label ID="lblSAConsigTitle" runat="server"/></span>
            <asp:Label ID="lblSAConsigStatus" runat="server" style="margin-left:10px;font-size:10px;font-weight:700;padding:2px 8px;border-radius:10px;" />
        </div>
        <asp:Button ID="btnSASendToPK" runat="server" Text="Send to Shipment Team" CssClass="btn btn-primary btn-sm"
            OnClick="btnSASendToPK_Click" CausesValidation="false" Visible="false" />
    </div>
</div>

<!-- Shipment Orders List -->
<div class="sa-card" style="margin-bottom:14px;">
    <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:10px;">
        <div class="card-title" style="margin:0;padding:0;border:none;">Shipment Orders</div>
    </div>
    <asp:Repeater ID="rptSAConsigOrders" runat="server">
        <HeaderTemplate><table class="sa-table"><tr><th>Order#</th><th>Customer</th><th>Date</th><th>Items</th><th>Qty</th><th>Status</th><th></th></tr></HeaderTemplate>
        <ItemTemplate><tr>
            <td style="font-family:monospace;font-weight:600;color:var(--accent);">SH-<%# Eval("ShipmentID").ToString().PadLeft(5,'0') %></td>
            <td><%# Eval("CustomerName") %><div style="font-size:9px;color:var(--text-dim);"><%# Eval("CustomerCode") %></div></td>
            <td><%# Convert.ToDateTime(Eval("ShipmentDate")).ToString("dd-MMM") %></td>
            <td class="num"><%# Eval("LineCount") %></td>
            <td class="num" style="font-weight:600;"><%# string.Format("{0:N0}", Eval("TotalQty")) %></td>
            <td><%# GetStatusBadge(Eval("Status").ToString()) %></td>
            <td><asp:LinkButton runat="server" Text="Edit" CommandName="EditSAConsigShip" CommandArgument='<%# Eval("ShipmentID") %>' OnCommand="SAConsigOrder_Command"
                Visible='<%# Eval("Status").ToString() == "Saved" || Eval("Status").ToString() == "Order" %>'
                style="color:var(--accent);font-size:11px;font-weight:600;text-decoration:none;cursor:pointer;margin-right:8px;" CausesValidation="false"/>
                <asp:LinkButton runat="server" Text="Delete" CommandName="DeleteSAConsigShip" CommandArgument='<%# Eval("ShipmentID") %>' OnCommand="SAConsigOrder_Command"
                Visible='<%# Eval("Status").ToString() == "Saved" || Eval("Status").ToString() == "Order" %>'
                OnClientClick="var href=this.getAttribute('href'); erpConfirm('Delete this shipment order?',{title:'Delete Order',type:'warn',okText:'Delete',onOk:function(){eval(href);}});return false;"
                style="color:var(--red);font-size:11px;font-weight:600;text-decoration:none;cursor:pointer;" CausesValidation="false"/></td>
        </tr></ItemTemplate>
        <FooterTemplate></table></FooterTemplate>
    </asp:Repeater>
    <asp:Panel ID="pnlSAConsigOrdersEmpty" runat="server"><div style="text-align:center;padding:14px;color:var(--text-dim);font-size:12px;">No shipment orders yet. Create one below.</div></asp:Panel>
</div>

<!-- Shipment Order Form (embedded in consignment) -->
<div class="sa-card">
    <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:0;">
        <div class="card-title" style="margin-bottom:0;padding-bottom:0;border-bottom:none;">
            <asp:Label ID="lblSAShipFormTitle" runat="server" Text="New Shipment Order"/>
            <asp:Label ID="lblSAEditShipId" runat="server" style="color:var(--accent);"/>
        </div>
        <asp:LinkButton ID="btnSACancelEdit" runat="server" Text="&#x2715; Cancel Edit" CssClass="btn btn-sm"
            OnClick="btnSACancelEdit_Click" CausesValidation="false" Visible="false"
            style="background:#f0f0f0;color:#666;border:1px solid #ddd;text-decoration:none;padding:6px 12px;font-size:11px;"/>
    </div>
    <div style="display:grid;grid-template-columns:1fr 1fr 1fr;gap:10px;margin-top:12px;">
        <div><label style="font-size:10px;font-weight:700;text-transform:uppercase;color:var(--text-muted);">Customer <span style="color:var(--accent);">*</span></label>
            <asp:DropDownList ID="ddlSACustomer" runat="server" style="width:100%;padding:8px;border:1px solid var(--border);border-radius:6px;font-size:12px;"/></div>
        <div><label style="font-size:10px;font-weight:700;text-transform:uppercase;color:var(--text-muted);">Date <span style="color:var(--accent);">*</span></label>
            <asp:TextBox ID="txtSAShipDate" runat="server" TextMode="Date" style="width:100%;padding:8px;border:1px solid var(--border);border-radius:6px;font-size:12px;"/></div>
        <div><label style="font-size:10px;font-weight:700;text-transform:uppercase;color:var(--text-muted);">Channel <span style="color:var(--accent);">*</span></label>
            <asp:DropDownList ID="ddlSAChannel" runat="server" style="width:100%;padding:8px;border:1px solid var(--border);border-radius:6px;font-size:12px;"/></div>
    </div>

    <!-- Product Lines -->
    <div style="margin-top:14px;">
        <div class="card-title" style="font-size:12px;">Products</div>
        <div id="divSAShipLines">
            <asp:Repeater ID="rptSAShipLines" runat="server" OnItemDataBound="rptSAShipLines_ItemDataBound">
                <ItemTemplate>
                    <div class="line-row" style="display:grid;grid-template-columns:3fr 1fr 1fr 140px 30px;gap:8px;margin-bottom:6px;align-items:center;">
                        <select name="sa_ship_product" class="sa-prod-sel" onchange="onSAProductChange(this);"
                            style="padding:7px;border:1px solid var(--border);border-radius:6px;font-size:12px;">
                            <option value="0">-- Select Product --</option>
                            <asp:Literal ID="litSAProductOptions" runat="server"/>
                        </select>
                        <select name="sa_ship_form" class="sa-form-sel" style="padding:7px;border:1px solid var(--border);border-radius:6px;font-size:12px;">
                            <asp:Literal ID="litSAFormOptions" runat="server"/>
                        </select>
                        <input type="number" name="sa_ship_qty" min="0" step="1" value='<%# Eval("Quantity") %>' placeholder="Qty"
                            onblur="roundToCase(this);"
                            style="padding:7px;border:1px solid var(--border);border-radius:6px;font-size:12px;"/>
                        <span class="sa-stock-info" style="font-size:10px;color:#666;"></span>
                        <button type="button" style="background:none;border:none;color:#e74c3c;font-size:14px;cursor:pointer;" onclick="this.parentNode.remove();">&#x2715;</button>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
        <button type="button" class="btn btn-sm" style="font-size:11px;background:#f0f0f0;color:#333;border:1px solid #ddd;padding:6px 12px;border-radius:6px;margin-top:4px;" onclick="addSAShipLine()">+ Add Product</button>
    </div>

    <div style="display:flex;gap:8px;margin-top:14px;">
        <asp:Button ID="btnSASaveDraft" runat="server" Text="Save Draft" CssClass="btn btn-sm" style="background:#f0f0f0;color:#333;border:1px solid #ddd;"
            OnClick="btnSASaveDraft_Click" CausesValidation="false"/>
        <asp:Button ID="btnSACreateOrder" runat="server" Text="Create Shipment Order" CssClass="btn btn-primary"
            OnClick="btnSACreateOrder_Click" CausesValidation="false"/>
    </div>
</div>
</asp:Panel>

<!-- Empty state when no consignment selected -->
<asp:Panel ID="pnlConsigEmpty" runat="server">
    <div style="text-align:center;padding:40px;color:var(--text-dim);font-size:13px;">
        Select a consignment tab or click <strong>+</strong> to create one.
    </div>
</asp:Panel>

</asp:Panel>

</div>
<asp:HiddenField ID="hfProductOptionsHtml" runat="server"/><asp:HiddenField ID="hfUOMOptionsHtml" runat="server"/><asp:HiddenField ID="hfEditProjId" runat="server" Value="0"/><asp:HiddenField ID="hfEditShipId" runat="server" Value="0"/><asp:HiddenField ID="hfSAConsigId" runat="server" Value="0"/><asp:HiddenField ID="hfSAProductData" runat="server" Value="{}"/><asp:HiddenField ID="hfSAProductOptionsHtml" runat="server"/>
<script>
function addProjLine() {
    var p = document.getElementById('<%= hfProductOptionsHtml.ClientID %>').value;
    var u = document.getElementById('<%= hfUOMOptionsHtml.ClientID %>').value;
    var d = document.getElementById('divProjLines'), r = document.createElement('div');
    r.className = 'line-row';
    r.innerHTML = '<select name="proj_product" class="prod-sel"><option value="0">-- Select Product --</option>' + p + '</select>'
        + '<input type="number" name="proj_qty" class="qty-inp" min="0" step="1" placeholder="Qty"/>'
        + '<select name="proj_uom" class="uom-sel">' + u + '</select>'
        + '<input type="hidden" name="proj_lineid" value="0"/>'
        + '<button type="button" class="line-remove" onclick="this.parentNode.remove();">&#x2715;</button>';
    d.appendChild(r);
}
function addShipLine() {
    var p = document.getElementById('<%= hfProductOptionsHtml.ClientID %>').value;
    var u = document.getElementById('<%= hfUOMOptionsHtml.ClientID %>').value;
    var d = document.getElementById('divShipLines'), r = document.createElement('div');
    r.className = 'line-row';
    r.innerHTML = '<select name="ship_product" class="prod-sel"><option value="0">-- Select Product --</option>' + p + '</select>'
        + '<input type="number" name="ship_qty" class="qty-inp" min="0" step="1" placeholder="Qty"/>'
        + '<select name="ship_uom" class="uom-sel">' + u + '</select>'
        + '<input type="hidden" name="ship_productid" value="0"/>'
        + '<button type="button" class="line-remove" onclick="this.parentNode.remove();">&#x2715;</button>';
    d.appendChild(r);
}
// ── Customer Modal Search (GRN-style) ──
var _custOv = null;
function openCustModal() {
    var ddl = document.getElementById('<%= ddlCustomer.ClientID %>');
    if (!ddl) return;
    var items = [];
    for (var i = 0; i < ddl.options.length; i++) {
        if (ddl.options[i].value === '0') continue;
        items.push({ value: ddl.options[i].value, text: ddl.options[i].text, idx: i });
    }
    if (_custOv) _custOv.remove();

    var ov = document.createElement('div');
    ov.style.cssText = 'position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,.5);z-index:9999;display:flex;align-items:flex-start;justify-content:center;padding:40px 16px 0;';

    var box = document.createElement('div');
    box.style.cssText = 'background:#fff;border-radius:14px;width:100%;max-width:540px;max-height:80vh;display:flex;flex-direction:column;box-shadow:0 8px 40px rgba(0,0,0,.25);overflow:hidden;';

    var hdr = document.createElement('div');
    hdr.style.cssText = 'padding:16px 20px 12px;border-bottom:2px solid #f0ede8;display:flex;align-items:center;justify-content:space-between;';
    hdr.innerHTML = '<span style="font-family:\'Bebas Neue\',sans-serif;font-size:18px;letter-spacing:.06em;">Select Customer</span>';
    var closeBtn = document.createElement('button'); closeBtn.type = 'button'; closeBtn.innerHTML = '\u2715';
    closeBtn.style.cssText = 'border:none;background:none;font-size:20px;cursor:pointer;color:#999;padding:4px 8px;';
    closeBtn.onclick = function() { ov.remove(); _custOv = null; };
    hdr.appendChild(closeBtn); box.appendChild(hdr);

    var sWrap = document.createElement('div'); sWrap.style.cssText = 'padding:12px 20px;';
    var sInput = document.createElement('input'); sInput.type = 'text'; sInput.placeholder = 'Search customer name, code...';
    sInput.style.cssText = 'width:100%;padding:12px 16px;border:2px solid #e0e0e0;border-radius:10px;font-size:16px;font-family:\'DM Sans\',sans-serif;outline:none;background:#fafafa;';
    sInput.setAttribute('autocomplete', 'off');
    sWrap.appendChild(sInput); box.appendChild(sWrap);

    var list = document.createElement('div');
    list.style.cssText = 'flex:1;overflow-y:auto;padding:0 8px 12px;-webkit-overflow-scrolling:touch;';

    function renderList(query) {
        list.innerHTML = '';
        var q = (query || '').toLowerCase().trim();
        var count = 0;
        items.forEach(function(it) {
            if (q && it.text.toLowerCase().indexOf(q) < 0) return;
            count++;
            var row = document.createElement('div');
            row.style.cssText = 'padding:12px 14px;border-radius:8px;cursor:pointer;font-size:14px;margin:2px 0;transition:background 0.1s;';
            row.onmouseenter = function() { row.style.background = '#f5f5f0'; };
            row.onmouseleave = function() { row.style.background = ''; };
            if (q) {
                var idx = it.text.toLowerCase().indexOf(q);
                row.innerHTML = escM(it.text.substring(0, idx)) +
                    '<strong style="color:var(--accent,#2980b9);">' + escM(it.text.substring(idx, idx + q.length)) + '</strong>' +
                    escM(it.text.substring(idx + q.length));
            } else { row.textContent = it.text; }
            row.onclick = function() {
                ddl.selectedIndex = it.idx;
                document.getElementById('txtCustSearch').value = it.text;
                ov.remove(); _custOv = null;
            };
            list.appendChild(row);
        });
        if (count === 0) {
            var empty = document.createElement('div');
            empty.style.cssText = 'padding:20px;text-align:center;color:#999;font-size:13px;';
            empty.textContent = 'No customers found';
            list.appendChild(empty);
        }
    }

    sInput.addEventListener('input', function() { renderList(sInput.value); });
    box.appendChild(list); ov.appendChild(box); document.body.appendChild(ov);
    _custOv = ov;
    ov.onclick = function(e) { if (e.target === ov) { ov.remove(); _custOv = null; } };
    renderList('');
    setTimeout(function() { sInput.focus(); }, 150);
}
function escM(s) { var d = document.createElement('div'); d.textContent = s; return d.innerHTML; }

// Sync customer name on page load (for edit mode)
window.addEventListener('load', function() {
    var ddl = document.getElementById('<%= ddlCustomer.ClientID %>');
    if (ddl && ddl.selectedIndex > 0) {
        document.getElementById('txtCustSearch').value = ddl.options[ddl.selectedIndex].text;
    }
});
var _saProductData = {};
try { _saProductData = JSON.parse(document.getElementById('<%= hfSAProductData.ClientID %>').value || '{}'); } catch(e){}

function onSAProductChange(sel) {
    var pid = sel.value;
    var row = sel.closest('.line-row');
    var info = row ? row.querySelector('.sa-stock-info') : null;
    var formSel = row ? row.querySelector('.sa-form-sel') : null;
    var pd = _saProductData[pid];

    // Auto-pick selling form based on container type (user can still override)
    if (formSel && pd && pd.containerType) {
        var ct = pd.containerType;
        var auto = ct === 'BOX' ? 'BOX' : ct === 'DIRECT' ? 'PCS' : 'JAR';
        // Only auto-set if user hasn't touched it (value is still the default JAR or matches auto)
        if (!formSel.dataset.userTouched) formSel.value = auto;
    }

    if (!info) return;
    if (pd) {
        info.innerHTML = '<span style="color:#1a9e6a;font-weight:700;">FG: ' + pd.stock + '</span> | Case: ' + pd.caseQty;
    } else {
        info.innerHTML = '';
    }
}

// Track manual form-selector changes so auto-pick doesn't override user choice
document.addEventListener('change', function(e) {
    if (e.target && e.target.classList && e.target.classList.contains('sa-form-sel')) {
        e.target.dataset.userTouched = '1';
    }
});

function roundToCase(inp) {
    var qty = parseInt(inp.value) || 0;
    if (qty <= 0) return;
    var row = inp.closest('.line-row');
    if (!row) return;
    var sel = row.querySelector('select.sa-prod-sel') || row.querySelector('select');
    if (!sel) return;
    var pid = sel.value;
    var pd = _saProductData[pid];
    if (!pd || pd.caseQty <= 1) return;
    var cq = pd.caseQty;
    var rounded = Math.ceil(qty / cq) * cq;
    if (rounded !== qty) {
        inp.value = rounded;
        inp.style.background = '#fffde7';
        setTimeout(function(){ inp.style.background = ''; }, 1500);
    }
}

function addSAShipLine() {
    var p = document.getElementById('<%= hfSAProductOptionsHtml.ClientID %>').value
         || document.getElementById('<%= hfProductOptionsHtml.ClientID %>').value;
    var d = document.getElementById('divSAShipLines'), r = document.createElement('div');
    r.className = 'line-row';
    r.style.cssText = 'display:grid;grid-template-columns:3fr 1fr 1fr 140px 30px;gap:8px;margin-bottom:6px;align-items:center;';
    r.innerHTML =
        '<select name="sa_ship_product" class="sa-prod-sel" onchange="onSAProductChange(this);" style="padding:7px;border:1px solid var(--border);border-radius:6px;font-size:12px;"><option value="0">-- Select Product --</option>' + p + '</select>'
      + '<select name="sa_ship_form" class="sa-form-sel" style="padding:7px;border:1px solid var(--border);border-radius:6px;font-size:12px;"><option value="JAR">JAR</option><option value="BOX">BOX</option><option value="PCS">PCS</option><option value="CASE">CASE</option></select>'
      + '<input type="number" name="sa_ship_qty" min="0" step="1" placeholder="Qty" onblur="roundToCase(this);" style="padding:7px;border:1px solid var(--border);border-radius:6px;font-size:12px;"/>'
      + '<span class="sa-stock-info" style="font-size:10px;color:#666;"></span>'
      + '<button type="button" style="background:none;border:none;color:#e74c3c;font-size:14px;cursor:pointer;" onclick="this.parentNode.remove();">&#x2715;</button>';
    d.appendChild(r);
}

// On page load, trigger stock hint for any pre-populated lines (edit mode)
window.addEventListener('load', function() {
    var sels = document.querySelectorAll('#divSAShipLines select.sa-prod-sel');
    for (var i = 0; i < sels.length; i++) {
        if (sels[i].value !== '0') {
            // Mark user-touched so auto-pick doesn't clobber the saved form selection
            var row = sels[i].closest('.line-row');
            var fs = row ? row.querySelector('.sa-form-sel') : null;
            if (fs) fs.dataset.userTouched = '1';
            onSAProductChange(sels[i]);
        }
    }
});
</script>
<script src="/StockApp/erp-modal.js"></script><script src="/StockApp/erp-keepalive.js"></script>
</form></body></html>
