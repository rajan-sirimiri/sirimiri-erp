<%@ Page Language="C#" AutoEventWireup="true" Inherits="PKApp.PKShipment" EnableEventValidation="false" %>
<!DOCTYPE html><html lang="en"><head runat="server">
<meta charset="utf-8"/><meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Delivery Challans &mdash; PK</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--accent:#e67e22;--accent-dark:#d35400;--teal:#1a9e6a;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--radius:12px;}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);min-height:100vh;color:var(--text);}
nav{background:#1a1a1a;height:52px;display:flex;align-items:center;padding:0 24px;gap:12px;position:sticky;top:0;z-index:100;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;}.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:17px;letter-spacing:.08em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:12px;}
.nav-user{color:rgba(255,255,255,.8);font-size:12px;}
.nav-link{color:#aaa;text-decoration:none;font-size:11px;font-weight:600;text-transform:uppercase;padding:5px 10px;border-radius:5px;}
.nav-link:hover{color:#fff;background:rgba(255,255,255,.08);}
.page-header{background:var(--surface);border-bottom:1px solid var(--border);padding:18px 32px;}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:26px;letter-spacing:.07em;}.page-title span{color:var(--accent);}
.page-sub{font-size:12px;color:var(--text-muted);margin-top:2px;}
.main{max-width:1200px;margin:24px auto;padding:0 28px;}
.card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:20px 24px;margin-bottom:20px;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:14px;letter-spacing:.08em;color:var(--text-muted);margin-bottom:14px;padding-bottom:10px;border-bottom:1px solid var(--border);}
.form-row{display:grid;grid-template-columns:1fr 1fr 1fr;gap:14px;}
.form-group{display:flex;flex-direction:column;gap:5px;}.form-group.full{grid-column:1/-1;}
label{font-size:11px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;color:var(--text-muted);}
.req{color:var(--accent);}
select,input[type=text],input[type=date],input[type=number],textarea{width:100%;padding:9px 12px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:13px;background:#fafafa;outline:none;}
select:focus,input:focus,textarea:focus{border-color:var(--accent);background:#fff;}
.btn{border:none;border-radius:8px;padding:10px 22px;font-size:12px;font-weight:700;cursor:pointer;}
.btn-primary{background:var(--accent);color:#fff;}.btn-primary:hover{background:var(--accent-dark);}
.btn-success{background:var(--teal);color:#fff;}.btn-success:hover{background:#148a5b;}
.btn-secondary{background:#f0f0f0;color:#333;border:1px solid var(--border);}
.btn-danger{background:#fdf3f2;color:#e74c3c;border:1px solid #f5c6cb;}
.btn-add{background:#1a1a1a;color:#fff;border:none;border-radius:8px;padding:9px 18px;font-size:12px;font-weight:700;cursor:pointer;}
.btn-add:hover{background:#333;}
.btn-remove{background:none;border:none;color:#e74c3c;font-size:11px;font-weight:700;cursor:pointer;text-decoration:underline;}
.btn-row{display:flex;gap:8px;margin-top:16px;}
.alert{padding:10px 14px;border-radius:8px;font-size:13px;font-weight:600;margin-bottom:14px;}
.alert-success{background:#eafaf1;color:var(--teal);border:1px solid #a9dfbf;}
.alert-danger{background:#fdf3f2;color:#e74c3c;border:1px solid #f5c6cb;}
.data-table{width:100%;border-collapse:collapse;font-size:13px;}
.data-table th{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);padding:9px 12px;border-bottom:2px solid var(--border);text-align:left;background:#fafafa;}
.data-table th.num,.data-table td.num{text-align:right;}
.data-table td{padding:10px 12px;border-bottom:1px solid var(--border);}
.data-table tr:hover td{background:#f9f9f9;}
.badge-draft{background:#fef9f3;color:var(--accent);font-size:10px;font-weight:700;padding:3px 8px;border-radius:10px;}
.badge-final{background:#eafaf1;color:var(--teal);font-size:10px;font-weight:700;padding:3px 8px;border-radius:10px;}
.stock-badge{display:inline-block;background:#e8f8f0;color:var(--teal);font-size:11px;font-weight:700;padding:3px 10px;border-radius:8px;margin-left:6px;}
.stock-zero{background:#fdf3f2;color:#e74c3c;}
.line-table{width:100%;border-collapse:collapse;font-size:13px;margin-top:12px;}
.line-table th{font-size:9px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);padding:8px 10px;border-bottom:1px solid var(--border);text-align:left;}
.line-table th.num{text-align:right;}.line-table td{padding:8px 10px;border-bottom:1px solid #f0f0f0;}
.line-table td.num{text-align:right;font-variant-numeric:tabular-nums;}
.line-table tfoot td{border-top:2px solid var(--border);font-weight:700;}
.empty-note{text-align:center;padding:28px;color:var(--text-dim);font-size:13px;}
.ship-tab-bar{display:flex;gap:0;border-bottom:2px solid var(--border);margin-bottom:16px;background:var(--surface);border-radius:var(--radius) var(--radius) 0 0;overflow:hidden;}
.ship-tab{padding:12px 24px;font-size:12px;font-weight:700;letter-spacing:.04em;border:none;background:transparent;cursor:pointer;color:var(--text-muted);border-bottom:3px solid transparent;margin-bottom:-2px;}
.ship-tab.active{color:var(--accent);border-bottom-color:var(--accent);background:#f5faff;}
.ship-tab-panel{display:none;}.ship-tab-panel.active{display:block;}
.proj-card{background:#f9f9f9;border:1px solid var(--border);border-radius:10px;padding:14px 16px;margin-bottom:12px;}
.proj-header{display:flex;justify-content:space-between;align-items:center;cursor:pointer;}
.proj-title{font-weight:700;font-size:13px;}
.proj-meta{font-size:11px;color:var(--text-muted);}
.proj-detail{margin-top:12px;display:none;}.proj-detail.open{display:block;}
.act-link{color:var(--accent);font-size:11px;font-weight:600;text-decoration:none;cursor:pointer;}.act-link:hover{text-decoration:underline;}
.locked-msg{background:#f5f5f5;border:1px solid var(--border);border-radius:10px;padding:14px 18px;text-align:center;color:var(--text-muted);font-size:13px;font-weight:600;}
.badge-order{background:#d4edda;color:#155724;font-size:10px;font-weight:700;padding:3px 8px;border-radius:10px;}
.badge-dc{background:#cce5ff;color:#004085;font-size:10px;font-weight:700;padding:3px 8px;border-radius:10px;}
.badge-shipped{background:#e2e3e5;color:#383d41;font-size:10px;font-weight:700;padding:3px 8px;border-radius:10px;}
.sa-order-detail{background:#f9f9f9;border:1px solid var(--border);border-radius:8px;padding:14px 16px;margin-top:8px;}
</style></head><body>
<form id="form1" runat="server">
<asp:HiddenField ID="hfDCID" runat="server" Value="0"/>
<asp:HiddenField ID="hfLines" runat="server" Value=""/>
<asp:HiddenField ID="hfProductData" runat="server" Value="{}"/>
<nav>
    <a class="nav-logo" href="PKHome.aspx"><img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="" onerror="this.style.display='none'"/></a>
    <span class="nav-title">Packing &amp; Shipments</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblUser" runat="server"/></span>
        <a href="/StockApp/ERPHome.aspx" class="nav-link">ERP Home</a>
        <a href="PKHome.aspx" class="nav-link">PK Home</a>
        <a href="PKLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>
<div class="page-header">
    <div class="page-title">Delivery <span>Challans</span></div>
    <div class="page-sub">Create and manage delivery challans for FG shipments</div>
</div>
<div class="main">
    <asp:Panel ID="pnlAlert" runat="server" Visible="false"><div class="alert"><asp:Label ID="lblAlert" runat="server"/></div></asp:Panel>

    <!-- ══════ DC FORM ══════ -->
    <asp:Panel ID="pnlForm" runat="server">
    <div class="card">
        <div class="card-title"><asp:Label ID="lblFormTitle" runat="server">New Delivery Challan</asp:Label></div>
        <div class="form-row">
            <div class="form-group"><label>DC Number</label>
                <asp:TextBox ID="txtDCNumber" runat="server" ReadOnly="true" placeholder="Auto-generated"/></div>
            <div class="form-group" style="position:relative;"><label>Customer <span class="req">*</span></label>
                <input type="text" id="txtCustomerSearch" placeholder="Type to search customer..."
                    oninput="filterDropdown(this.value, '<%= ddlCustomer.ClientID %>', 'txtCustomerSearch');"
                    onfocus="filterDropdown(this.value, '<%= ddlCustomer.ClientID %>', 'txtCustomerSearch');"
                    style="margin-bottom:4px;padding:8px 12px;border:1.5px solid #e0e0e0;border-radius:8px;font-size:12px;background:#fffdf5;outline:none;width:100%;" autocomplete="off"/>
                <asp:DropDownList ID="ddlCustomer" runat="server"/></div>
            <div class="form-group"><label>DC Date <span class="req">*</span></label>
                <asp:TextBox ID="txtDCDate" runat="server" TextMode="Date"/></div>
        </div>

        <!-- ADD PRODUCT LINE -->
        <div style="margin-top:20px;padding-top:16px;border-top:1px solid var(--border);">
            <div style="font-size:12px;font-weight:700;color:var(--text-muted);margin-bottom:10px;">ADD PRODUCTS TO THIS DC</div>
            <div class="form-row" style="align-items:flex-end;">
                <div class="form-group"><label>Product</label>
                    <select id="selProduct" onchange="onProductSelect();"><option value="0">-- Select Product --</option></select></div>
                <div class="form-group"><label>Cases</label>
                    <input type="number" id="txtLineCases" min="0" step="1" placeholder="0"/></div>
                <div class="form-group"><label>Loose Jars</label>
                    <input type="number" id="txtLineLoose" min="0" step="1" placeholder="0"/></div>
            </div>
            <div id="stockInfo" style="font-size:12px;color:var(--text-dim);margin-top:4px;"></div>
            <button type="button" class="btn-add" onclick="addLine();" style="margin-top:10px;">+ Add Product Line</button>
        </div>

        <!-- LINE ITEMS TABLE -->
        <table class="line-table" id="lineTable" style="display:none;">
            <thead><tr><th>Product</th><th class="num">Cases</th><th class="num">Loose Jars</th><th class="num">Jars/Case</th><th class="num">Total Pcs</th><th></th></tr></thead>
            <tbody id="lineBody"></tbody>
            <tfoot><tr><td>Total</td><td class="num" id="ftCases">0</td><td class="num" id="ftLoose">0</td><td></td><td class="num" id="ftPcs">0</td><td></td></tr></tfoot>
        </table>

        <div class="form-group" style="margin-top:14px;"><label>Remarks</label>
            <asp:TextBox ID="txtRemarks" runat="server" TextMode="MultiLine" Rows="2" MaxLength="500" placeholder="Optional notes"/></div>

        <div class="btn-row">
            <asp:Button ID="btnDraftSave" runat="server" Text="&#x1F4BE; Save as Draft" CssClass="btn btn-primary" OnClick="btnDraftSave_Click" CausesValidation="false"/>
            <asp:Button ID="btnFinalise" runat="server" Text="&#x2705; Finalise Shipment" CssClass="btn btn-success" OnClick="btnFinalise_Click" CausesValidation="false"/>
            <asp:Button ID="btnNew" runat="server" Text="+ New DC" CssClass="btn btn-secondary" OnClick="btnNew_Click" CausesValidation="false"/>
            <asp:Button ID="btnPrintDC" runat="server" Text="&#x1F4C4; Download DC" CssClass="btn btn-secondary" OnClick="btnPrintDC_Click" CausesValidation="false"/>
        </div>
    </div>
    </asp:Panel>

    <!-- ══════ FINALISED VIEW ══════ -->
    <asp:Panel ID="pnlLocked" runat="server" Visible="false">
    <div class="card">
        <div class="card-title"><asp:Label ID="lblLockedTitle" runat="server">Delivery Challan</asp:Label></div>
        <div style="display:flex;gap:20px;align-items:center;margin-bottom:16px;padding:14px 18px;background:#f0faf5;border:1px solid #a9dfbf;border-radius:10px;">
            <div><span style="font-size:10px;font-weight:700;color:#888;text-transform:uppercase;">DC Number</span><div style="font-size:18px;font-weight:800;"><asp:Label ID="lblViewDCNum" runat="server"/></div></div>
            <div><span style="font-size:10px;font-weight:700;color:#888;text-transform:uppercase;">Date</span><div style="font-size:14px;font-weight:600;"><asp:Label ID="lblViewDate" runat="server"/></div></div>
            <div><span style="font-size:10px;font-weight:700;color:#888;text-transform:uppercase;">Customer</span><div style="font-size:14px;font-weight:600;"><asp:Label ID="lblViewCustomer" runat="server"/></div></div>
            <div><span class="badge-final">Finalised</span></div>
        </div>
        <asp:Panel ID="pnlViewRemarks" runat="server" Visible="false">
            <div style="font-size:12px;color:#555;margin-bottom:12px;padding:8px;background:#f9f9f9;border-radius:4px;"><strong>Remarks:</strong> <asp:Label ID="lblViewRemarks" runat="server"/></div>
        </asp:Panel>
        <asp:Repeater ID="rptViewLines" runat="server">
            <HeaderTemplate><table class="data-table"><thead><tr><th>Product</th><th class="num">Cases</th><th class="num">Loose Jars</th><th class="num">Jars/Case</th><th class="num">Total Pcs</th></tr></thead><tbody></HeaderTemplate>
            <ItemTemplate><tr>
                <td><strong><%# Eval("ProductName") %></strong><div style="font-size:10px;color:var(--text-dim);"><%# Eval("ProductCode") %></div></td>
                <td class="num"><%# Eval("Cases") %></td>
                <td class="num"><%# Eval("LooseJars") %></td>
                <td class="num" style="color:var(--text-dim);"><%# Eval("JarsPerCase") %></td>
                <td class="num" style="font-weight:700;color:var(--teal);"><%# string.Format("{0:N0}", Eval("TotalPcs")) %></td>
            </tr></ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>
        <div class="btn-row" style="margin-top:14px;">
            <asp:Button ID="btnDownloadFromView" runat="server" Text="&#x1F4C4; Download DC" CssClass="btn btn-primary" OnClick="btnPrintDC_Click" CausesValidation="false"/>
            <asp:Button ID="btnNewFromLocked" runat="server" Text="+ Create New DC" CssClass="btn btn-secondary" OnClick="btnNew_Click" CausesValidation="false"/>
        </div>
    </div>
    </asp:Panel>

    <!-- ══════ TAB BAR ══════ -->
    <div class="ship-tab-bar">
        <button type="button" class="ship-tab active" onclick="switchShipTab('dc')">&#x1F4CB; Delivery Challans</button>
        <button type="button" class="ship-tab" onclick="switchShipTab('sa')">&#x1F4E6; Sales Force Orders</button>
        <button type="button" class="ship-tab" onclick="switchShipTab('proj')">&#x1F4CA; Sales Projections</button>
    </div>

    <!-- ══════ TAB: SALES FORCE ORDERS ══════ -->
    <div id="tabSA" class="ship-tab-panel">
    <div class="card">
        <div class="card-title">&#x1F4E6; Sales Force Orders</div>
        <asp:Panel ID="pnlSAEmpty" runat="server"><div class="empty-note">No pending orders from Sales Force</div></asp:Panel>
        <asp:Panel ID="pnlSAList" runat="server" Visible="false">
        <table class="data-table">
            <thead><tr><th>Order #</th><th>Date</th><th>Customer</th><th>Area</th><th>Channel</th><th>Transport</th><th class="num">Items</th><th>Status</th><th></th></tr></thead>
            <tbody>
                <asp:Repeater ID="rptSAOrders" runat="server" OnItemCommand="rptSAOrders_ItemCommand">
                    <ItemTemplate><tr>
                        <td style="font-family:monospace;font-weight:700;color:var(--accent);">SH-<%# Eval("ShipmentID").ToString().PadLeft(5,'0') %></td>
                        <td style="font-size:12px;"><%# Convert.ToDateTime(Eval("ShipmentDate")).ToString("dd-MMM-yyyy") %></td>
                        <td style="font-weight:600;"><%# Eval("CustomerName") %></td>
                        <td style="font-size:11px;"><%# Eval("AreaName") %> <span style="color:var(--text-dim);font-size:10px;">(<%# Eval("ZoneName") %> / <%# Eval("RegionName") %>)</span></td>
                        <td><%# Eval("ChannelName") %></td>
                        <td style="font-size:11px;"><%# Eval("TransportMode") %></td>
                        <td class="num"><%# Eval("ProductCount") %></td>
                        <td><%# GetSAStatusBadge(Eval("Status").ToString()) %></td>
                        <td>
                            <asp:LinkButton runat="server" CommandName="EditSAOrder" CommandArgument='<%# Eval("ShipmentID") %>'
                                CssClass="act-link" CausesValidation="false"
                                Visible='<%# Eval("Status").ToString() != "Shipped" %>'>Edit</asp:LinkButton>
                            <asp:LinkButton runat="server" CommandName="ViewSAOrder" CommandArgument='<%# Eval("ShipmentID") %>'
                                CssClass="act-link" CausesValidation="false"
                                Visible='<%# Eval("Status").ToString() == "Shipped" %>'>View</asp:LinkButton>
                            <asp:LinkButton runat="server" CommandName="ConvertDC" CommandArgument='<%# Eval("ShipmentID") %>'
                                CssClass="act-link" CausesValidation="false" Visible='<%# Eval("Status").ToString()=="Order" %>'
                                style="margin-left:8px;color:var(--teal);">Convert to DC</asp:LinkButton>
                            <asp:LinkButton runat="server" CommandName="UnconvertDC" CommandArgument='<%# Eval("ShipmentID") %>'
                                CssClass="act-link" CausesValidation="false" Visible='<%# Eval("Status").ToString()=="DC" %>'
                                style="margin-left:8px;color:var(--accent);">Unconvert DC</asp:LinkButton>
                            <asp:LinkButton runat="server" CommandName="Dispatch" CommandArgument='<%# Eval("ShipmentID") %>'
                                CssClass="act-link" CausesValidation="false" Visible='<%# Eval("Status").ToString()=="DC" %>'
                                style="margin-left:8px;color:#6f42c1;">Finalize Shipment</asp:LinkButton>
                        </td>
                    </tr></ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
        </asp:Panel>
    </div>

    <!-- SA ORDER EDIT/VIEW -->
    <asp:Panel ID="pnlSADetail" runat="server" Visible="false">
    <div class="card">
        <div class="card-title">Order — <asp:Label ID="lblSAOrderId" runat="server"/> <asp:Label ID="lblSAStatus" runat="server" style="margin-left:8px;"/></div>

        <!-- Header info -->
        <div style="display:flex;gap:20px;flex-wrap:wrap;margin-bottom:14px;padding:12px 16px;background:#f0f8ff;border:1px solid #c2ddf5;border-radius:8px;">
            <div><span style="font-size:10px;font-weight:700;color:#888;text-transform:uppercase;">Customer</span><div style="font-weight:600;"><asp:Label ID="lblSACustomer" runat="server"/></div></div>
            <div><span style="font-size:10px;font-weight:700;color:#888;text-transform:uppercase;">Date</span><div><asp:Label ID="lblSADate" runat="server"/></div></div>
            <div><span style="font-size:10px;font-weight:700;color:#888;text-transform:uppercase;">Area</span><div><asp:Label ID="lblSAArea" runat="server"/></div></div>
            <div><span style="font-size:10px;font-weight:700;color:#888;text-transform:uppercase;">Channel</span><div><asp:Label ID="lblSAChannel" runat="server"/></div></div>
            <div><span style="font-size:10px;font-weight:700;color:#888;text-transform:uppercase;">Transport</span><div><asp:Label ID="lblSATransport" runat="server"/></div></div>
        </div>

        <!-- FG Stock Check table (always shown) -->
        <asp:Repeater ID="rptSALines" runat="server">
            <HeaderTemplate><table class="data-table"><thead><tr><th>Product</th><th class="num">Qty (Jars)</th><th class="num">FG Stock</th><th>Stock OK?</th></tr></thead><tbody></HeaderTemplate>
            <ItemTemplate><tr>
                <td><strong><%# Eval("ProductName") %></strong><div style="font-size:10px;color:var(--text-dim);"><%# Eval("ProductCode") %></div></td>
                <td class="num" style="font-weight:600;"><%# Eval("RequiredQty") %></td>
                <td class="num"><%# Eval("AvailableQty") %></td>
                <td><%# Convert.ToDecimal(Eval("AvailableQty")) >= Convert.ToDecimal(Eval("RequiredQty")) ? "<span style='color:var(--teal);font-weight:700;'>✓ OK</span>" : "<span style='color:#e74c3c;font-weight:700;'>✗ Short</span>" %></td>
            </tr></ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>

        <!-- Editable product lines (only for Order/DC status) -->
        <asp:Panel ID="pnlSAEditLines" runat="server" Visible="false">
        <div style="margin-top:16px;padding-top:14px;border-top:1px solid var(--border);">
            <div style="font-size:12px;font-weight:700;color:var(--text-muted);margin-bottom:10px;">EDIT PRODUCTS</div>
            <div id="divSAEditLines">
                <asp:Repeater ID="rptSAEditLines" runat="server" OnItemDataBound="rptSAEditLines_ItemDataBound">
                    <ItemTemplate>
                        <div class="form-row" style="margin-bottom:6px;align-items:center;">
                            <div class="form-group" style="flex:3;">
                                <select name="sa_edit_product" style="width:100%;padding:8px 10px;border:1.5px solid var(--border);border-radius:8px;font-size:12px;">
                                    <option value="0">-- Select Product --</option>
                                    <asp:Literal ID="litSAEditProduct" runat="server"/>
                                </select>
                            </div>
                            <div class="form-group" style="flex:1;">
                                <input type="number" name="sa_edit_qty" min="0" step="1" value='<%# Eval("Qty") %>'
                                    style="width:100%;padding:8px 10px;border:1.5px solid var(--border);border-radius:8px;font-size:12px;" placeholder="Qty"/>
                            </div>
                            <div style="flex:0;">
                                <button type="button" style="background:none;border:none;color:#e74c3c;font-size:16px;cursor:pointer;" onclick="this.closest('.form-row').remove();">&#x2715;</button>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <button type="button" class="btn btn-secondary" style="margin-top:8px;font-size:11px;" onclick="addSAEditLine();">+ Add Product</button>
        </div>
        </asp:Panel>

        <div class="btn-row" style="margin-top:14px;">
            <asp:Button ID="btnSaveSAEdit" runat="server" Text="&#x1F4BE; Save Changes" CssClass="btn btn-primary" OnClick="btnSaveSAEdit_Click" CausesValidation="false"/>
            <asp:Button ID="btnConvertDC" runat="server" Text="&#x2705; Convert to DC" CssClass="btn btn-success" OnClick="btnConvertDC_Click" CausesValidation="false"/>
            <asp:Button ID="btnUnconvertDC" runat="server" Text="&#x21A9; Unconvert DC" CssClass="btn btn-secondary" OnClick="btnUnconvertDC_Click" CausesValidation="false"/>
            <asp:Button ID="btnDispatch" runat="server" Text="&#x1F69A; Finalize Shipment" CssClass="btn btn-primary" OnClick="btnDispatch_Click" CausesValidation="false"/>
            <asp:Button ID="btnCloseSADetail" runat="server" Text="Close" CssClass="btn btn-secondary" OnClick="btnCloseSADetail_Click" CausesValidation="false"/>
        </div>
    </div>
    </asp:Panel>
    <asp:HiddenField ID="hfSAShipId" runat="server" Value="0"/>
    <asp:HiddenField ID="hfSAProductOptions" runat="server" Value="" ValidateRequestMode="Disabled"/>

    <!-- ══════ RECENT DCs LIST ══════ -->
    </div><!-- end tabSA -->

    <!-- ══════ TAB: SALES PROJECTIONS ══════ -->
    <div id="tabProj" class="ship-tab-panel">
    <div class="card">
        <div class="card-title">&#x1F4CA; Sales Projections (Read Only)</div>
        <div style="display:flex;gap:10px;align-items:center;margin-bottom:14px;">
            <asp:DropDownList ID="ddlProjMonth" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlProjMonth_Changed"
                style="padding:7px 10px;border:1px solid var(--border);border-radius:6px;font-size:13px;"/>
            <asp:DropDownList ID="ddlProjYear" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlProjYear_Changed"
                style="padding:7px 10px;border:1px solid var(--border);border-radius:6px;font-size:13px;"/>
        </div>
        <asp:Panel ID="pnlProjEmpty" runat="server"><div class="empty-note">No projections found for this period</div></asp:Panel>
        <asp:Repeater ID="rptProjections" runat="server">
            <ItemTemplate>
                <div class="proj-card">
                    <div class="proj-header" onclick="toggleProjDetail(this)">
                        <div>
                            <span class="proj-title"><%# Eval("AreaName") %></span>
                            <span class="proj-meta" style="margin-left:8px;"><%# Eval("ChannelName") %></span>
                            <span class='<%# Eval("Status").ToString()=="Confirmed" ? "badge-final" : "badge-draft" %>' style="margin-left:8px;"><%# Eval("Status") %></span>
                        </div>
                        <div>
                            <span class="proj-meta"><%# Eval("ProductCount") %> products &middot; <%# string.Format("{0:N0}", Eval("TotalQty")) %> units</span>
                            <span style="margin-left:6px;font-size:14px;">&#x25BC;</span>
                        </div>
                    </div>
                    <div class="proj-detail">
                        <div class="proj-meta" style="margin-bottom:8px;">Zone: <%# Eval("ZoneName") %> &middot; Region: <%# Eval("RegionName") %></div>
                        <asp:Repeater ID="rptProjLines" runat="server" DataSource='<%# GetProjectionLines(Eval("ProjectionID")) %>'>
                            <HeaderTemplate><table class="line-table"><thead><tr><th>Product</th><th>Code</th><th class="num">Quantity</th><th>UOM</th></tr></thead><tbody></HeaderTemplate>
                            <ItemTemplate><tr>
                                <td style="font-weight:600;"><%# Eval("ProductName") %></td>
                                <td style="font-size:11px;color:var(--text-dim);"><%# Eval("ProductCode") %></td>
                                <td class="num" style="font-weight:700;"><%# string.Format("{0:N0}", Eval("Quantity")) %></td>
                                <td><%# Eval("UOMAbbrv") %></td>
                            </tr></ItemTemplate>
                            <FooterTemplate></tbody></table></FooterTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>
    </div><!-- end tabProj -->

    <!-- ══════ TAB: DELIVERY CHALLANS ══════ -->
    <div id="tabDC" class="ship-tab-panel active">
    <div class="card">
        <div class="card-title">&#x1F4CB; Recent Delivery Challans</div>
        <asp:Panel ID="pnlEmpty" runat="server"><div class="empty-note">No delivery challans yet</div></asp:Panel>
        <asp:Panel ID="pnlList" runat="server" Visible="false">
        <table class="data-table">
            <thead><tr><th>DC No.</th><th>Date</th><th>Customer</th><th class="num">Lines</th><th class="num">Cases</th><th class="num">Total Pcs</th><th>Status</th><th></th></tr></thead>
            <tbody>
                <asp:Repeater ID="rptDCs" runat="server" OnItemCommand="rptDCs_ItemCommand">
                    <ItemTemplate><tr>
                        <td style="font-weight:700;"><%# Eval("DCNumber") %></td>
                        <td style="font-size:12px;"><%# Convert.ToDateTime(Eval("DCDate")).ToString("dd-MMM-yyyy") %></td>
                        <td><strong><%# Eval("CustomerName") %></strong><div style="font-size:10px;color:var(--text-dim);"><%# Eval("CustomerCode") %></div></td>
                        <td class="num"><%# Eval("LineCount") %></td>
                        <td class="num" style="font-weight:600;"><%# Eval("TotalCases") == DBNull.Value ? "0" : string.Format("{0:N0}", Eval("TotalCases")) %></td>
                        <td class="num" style="font-weight:700;color:var(--teal);"><%# Eval("TotalPcs") == DBNull.Value ? "0" : string.Format("{0:N0}", Eval("TotalPcs")) %></td>
                        <td><%# Eval("Status").ToString()=="FINALISED" ? "<span class='badge-final'>Finalised</span>" : "<span class='badge-draft'>Draft</span>" %></td>
                        <td><asp:LinkButton runat="server" CommandName="EditDC" CommandArgument='<%# Eval("DCID") %>' CssClass="act-link" CausesValidation="false"><%# Eval("Status").ToString()=="DRAFT" ? "Edit" : "View" %></asp:LinkButton></td>
                    </tr></ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
        </asp:Panel>
    </div>
    </div><!-- end tabDC -->
</div>
</form>
<script>
function switchShipTab(tab) {
    var panels = document.querySelectorAll('.ship-tab-panel');
    var tabs = document.querySelectorAll('.ship-tab');
    for (var i = 0; i < panels.length; i++) panels[i].classList.remove('active');
    for (var i = 0; i < tabs.length; i++) tabs[i].classList.remove('active');
    var map = {dc:'tabDC', sa:'tabSA', proj:'tabProj'};
    var p = document.getElementById(map[tab]);
    if (p) p.classList.add('active');
    // Highlight tab
    var idx = {dc:0, sa:1, proj:2};
    if (tabs[idx[tab]]) tabs[idx[tab]].classList.add('active');
}
function toggleProjDetail(el) {
    var detail = el.parentElement.querySelector('.proj-detail');
    if (detail) detail.classList.toggle('open');
    var arrow = el.querySelector('span:last-child');
    if (arrow) arrow.innerHTML = detail.classList.contains('open') ? '&#x25B2;' : '&#x25BC;';
}
// Generic searchable dropdown filter
function filterDropdown(q, ddlId, searchId) {
    var ddl = document.getElementById(ddlId);
    if (!ddl) return;
    q = q.toLowerCase().trim();
    var opts = ddl.options;
    var firstMatch = -1;
    for (var i = 0; i < opts.length; i++) {
        var txt = opts[i].text.toLowerCase();
        if (q === '' || txt.indexOf(q) >= 0 || opts[i].value === '0') {
            opts[i].style.display = ''; opts[i].disabled = false;
            if (firstMatch < 0 && i > 0 && txt.indexOf(q) >= 0) firstMatch = i;
        } else {
            opts[i].style.display = 'none'; opts[i].disabled = true;
        }
    }
    if (firstMatch >= 0 && q.length >= 2) ddl.selectedIndex = firstMatch;
    if (q.length >= 1) { ddl.size = Math.min(8, opts.length); ddl.style.position = 'absolute'; ddl.style.zIndex = '999'; ddl.style.width = '100%'; }
    else { ddl.size = 0; ddl.style.position = ''; ddl.style.zIndex = ''; }
}
document.addEventListener('change', function(e) {
    if (e.target.id === '<%= ddlCustomer.ClientID %>') {
        e.target.size = 0; e.target.style.position = ''; e.target.style.zIndex = '';
        var sb = document.getElementById('txtCustomerSearch');
        if (sb) sb.value = e.target.options[e.target.selectedIndex].text;
    }
});

var productData={};
try{productData=JSON.parse(document.getElementById('<%= hfProductData.ClientID %>').value||'{}');}catch(e){}
var lines=[];

// Restore lines from hidden field on load (for postback preservation)
window.addEventListener('load',function(){
    var sel=document.getElementById('selProduct');
    for(var pid in productData){
        var p=productData[pid];
        var jpc=parseInt(p.jarsPerCase)||12;
        var avJars=parseInt(p.availJars)||0;
        var avCases=Math.floor(avJars/jpc);
        var avLoose=avJars-(avCases*jpc);
        var opt=document.createElement('option');
        opt.value=pid;
        opt.text=p.name+' ('+p.code+') — '+avCases+' cases'+(avLoose>0?' + '+avLoose+' jars':'');
        opt.setAttribute('data-avjars',avJars);
        opt.setAttribute('data-jpc',jpc);
        opt.setAttribute('data-unitsize',parseInt(p.unitSize)||1);
        sel.appendChild(opt);
    }
    var raw=document.getElementById('<%= hfLines.ClientID %>').value;
    if(raw){try{lines=JSON.parse(raw);}catch(e){lines=[];}renderLines();}
});

function onProductSelect(){
    var sel=document.getElementById('selProduct');
    var info=document.getElementById('stockInfo');
    if(sel.value==='0'){info.innerHTML='';return;}
    var opt=sel.options[sel.selectedIndex];
    var avJars=parseInt(opt.getAttribute('data-avjars'))||0;
    var jpc=parseInt(opt.getAttribute('data-jpc'))||12;
    // Subtract already added lines for same product
    lines.forEach(function(l){if(l.pid===sel.value){avJars-=(l.cases*l.jpc+l.loose);}});
    var avCases=Math.floor(avJars/jpc);
    var avLoose=avJars-(avCases*jpc);
    info.innerHTML='<span class="stock-badge'+(avJars<=0?' stock-zero':'')+'">FG Stock: '+avCases+' cases'+(avLoose>0?' + '+avLoose+' loose jars':'')+' ('+avJars+' jars total)</span>';
}

function addLine(){
    var sel=document.getElementById('selProduct');
    var pid=sel.value;
    if(pid==='0'){erpAlert('Please select a product before adding.', {title:'Selection Required', type:'warn'});return;}
    var cs=parseInt(document.getElementById('txtLineCases').value)||0;
    var lj=parseInt(document.getElementById('txtLineLoose').value)||0;
    if(cs<=0&&lj<=0){erpAlert('Please enter number of cases or loose jars.', {title:'Quantity Required', type:'warn'});return;}
    var opt=sel.options[sel.selectedIndex];
    var jpc=parseInt(opt.getAttribute('data-jpc'))||12;
    var us=parseInt(opt.getAttribute('data-unitsize'))||1;
    var lineJars=(cs*jpc)+lj;
    var totalPcs=lineJars*us;
    var avJars=parseInt(opt.getAttribute('data-avjars'))||0;
    var usedJars=0;
    lines.forEach(function(l){if(l.pid===pid)usedJars+=(l.cases*l.jpc+l.loose);});
    if(lineJars>(avJars-usedJars)){erpAlert('Insufficient FG stock. Available: '+(avJars-usedJars)+' jars.', {title:'Stock Insufficient', type:'danger'});return;}
    var p=productData[pid];
    lines.push({pid:pid,name:p.name,code:p.code,cases:cs,loose:lj,jpc:jpc,unitSize:us,totalPcs:totalPcs});
    renderLines();
    document.getElementById('txtLineCases').value='';
    document.getElementById('txtLineLoose').value='';
    sel.selectedIndex=0;
    document.getElementById('stockInfo').innerHTML='';
}

function removeLine(idx){lines.splice(idx,1);renderLines();}

function updateDropdownStock(){
    var sel=document.getElementById('selProduct');
    for(var i=1;i<sel.options.length;i++){
        var opt=sel.options[i];
        var pid=opt.value;
        var avJars=parseInt(opt.getAttribute('data-avjars'))||0;
        var jpc=parseInt(opt.getAttribute('data-jpc'))||12;
        var usedJars=0;
        lines.forEach(function(l){if(l.pid===pid)usedJars+=(l.cases*l.jpc+l.loose);});
        var remJars=avJars-usedJars;
        var remCases=Math.floor(remJars/jpc);
        var remLoose=remJars-(remCases*jpc);
        var p=productData[pid];
        if(p) opt.text=p.name+' ('+p.code+') — '+remCases+' cases'+(remLoose>0?' + '+remLoose+' jars':'');
    }
}

function renderLines(){
    var body=document.getElementById('lineBody');
    var tbl=document.getElementById('lineTable');
    body.innerHTML='';
    if(lines.length===0){tbl.style.display='none';syncLines();updateDropdownStock();return;}
    tbl.style.display='table';
    var tc=0,tl=0,tp=0;
    lines.forEach(function(l,i){
        var tr=document.createElement('tr');
        tr.innerHTML='<td><strong>'+l.name+'</strong><div style="font-size:10px;color:var(--text-dim);">'+l.code+'</div></td>'
            +'<td class="num">'+l.cases+'</td>'
            +'<td class="num">'+l.loose+'</td>'
            +'<td class="num" style="color:var(--text-dim);">'+l.jpc+'</td>'
            +'<td class="num" style="font-weight:700;color:var(--teal);">'+l.totalPcs.toLocaleString()+'</td>'
            +'<td><button type="button" class="btn-remove" onclick="removeLine('+i+')">Remove</button></td>';
        body.appendChild(tr);
        tc+=l.cases;tl+=l.loose;tp+=l.totalPcs;
    });
    document.getElementById('ftCases').innerText=tc;
    document.getElementById('ftLoose').innerText=tl;
    document.getElementById('ftPcs').innerText=tp.toLocaleString();
    syncLines();
    updateDropdownStock();
}

function syncLines(){
    document.getElementById('<%= hfLines.ClientID %>').value=JSON.stringify(lines);
}
function addSAEditLine(){
    var p = document.getElementById('<%= hfSAProductOptions.ClientID %>').value;
    var d = document.getElementById('divSAEditLines');
    var r = document.createElement('div');
    r.className = 'form-row';
    r.style.cssText = 'margin-bottom:6px;align-items:center;';
    r.innerHTML = '<div class="form-group" style="flex:3;"><select name="sa_edit_product" style="width:100%;padding:8px 10px;border:1.5px solid var(--border);border-radius:8px;font-size:12px;"><option value="0">-- Select Product --</option>' + p + '</select></div>'
        + '<div class="form-group" style="flex:1;"><input type="number" name="sa_edit_qty" min="0" step="1" style="width:100%;padding:8px 10px;border:1.5px solid var(--border);border-radius:8px;font-size:12px;" placeholder="Qty"/></div>'
        + '<div style="flex:0;"><button type="button" style="background:none;border:none;color:#e74c3c;font-size:16px;cursor:pointer;" onclick="this.closest(\'.form-row\').remove();">&#x2715;</button></div>';
    d.appendChild(r);
}
</script>
<script src="/StockApp/erp-modal.js"></script>
<script src="/StockApp/erp-keepalive.js"></script>
</body></html>
