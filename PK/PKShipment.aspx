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
.btn-zoho{background:#0078d4;color:#fff;margin-left:6px;}.btn-zoho:hover{background:#005a9e;}
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
<asp:HiddenField ID="hfCustomerData" runat="server" Value="{}"/>
<asp:HiddenField ID="hfOrgState" runat="server" Value="Tamil Nadu"/>
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

    <!-- ══════ CONSIGNMENT TAB BAR (browser-style) ══════ -->
    <div style="display:flex;justify-content:space-between;align-items:flex-end;margin-bottom:0;">
        <div class="ship-tab-bar" id="consigTabBar" style="flex:1;margin-bottom:0;">
            <asp:Button ID="btnTabRetail" runat="server" Text="Retail Orders" CssClass="ship-tab active"
                OnClick="btnTabRetail_Click" CausesValidation="false" />
            <asp:Repeater ID="rptConsigTabs" runat="server">
                <ItemTemplate>
                    <asp:Button runat="server" Text='<%# Eval("ConsignmentCode") %>'
                        CssClass='<%# "ship-tab" + (Eval("ConsignmentID").ToString() == hfActiveConsig.Value ? " active" : "") %>'
                        CommandName="SelectConsig" CommandArgument='<%# Eval("ConsignmentID") %>'
                        OnCommand="rptConsigTabs_Command" CausesValidation="false" />
                </ItemTemplate>
            </asp:Repeater>
            <button type="button" class="ship-tab" onclick="document.getElementById('pnlNewConsig').style.display=document.getElementById('pnlNewConsig').style.display==='none'?'block':'none';"
                style="color:#fff;background:var(--teal);font-size:11px;font-weight:700;padding:8px 16px;border-radius:8px;margin-left:8px;margin-bottom:4px;" title="New Consignment">+ Create Consignment</button>
        </div>
        <div style="display:flex;flex-direction:column;gap:4px;margin-left:12px;margin-bottom:2px;">
            <asp:DropDownList ID="ddlDispatched" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlDispatched_Changed"
                style="font-size:10px;padding:4px 8px;border:1px solid #cce5ff;border-radius:6px;color:#004085;background:#f0f7ff;" />
            <asp:DropDownList ID="ddlArchived" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlArchived_Changed"
                style="font-size:10px;padding:4px 8px;border:1px solid #e2e3e5;border-radius:6px;color:#383d41;background:#f5f5f5;" />
        </div>
    </div>
    <asp:HiddenField ID="hfActiveConsig" runat="server" Value="0" />
    <asp:HiddenField ID="hfActiveTab" runat="server" Value="retail" />

    <!-- Inline new consignment form -->
    <div id="pnlNewConsig" style="display:none;background:#f8f7f5;border:1px solid var(--border);border-radius:0 0 var(--radius) var(--radius);padding:14px;margin-bottom:16px;">
        <div style="font-size:11px;font-weight:700;color:var(--text-muted);margin-bottom:8px;">CREATE NEW CONSIGNMENT</div>
        <div class="form-row" style="align-items:flex-end;">
            <div class="form-group" style="flex:1;"><label>Date</label>
                <asp:TextBox ID="txtConsigDate" runat="server" TextMode="Date" /></div>
            <div class="form-group" style="flex:1;"><label>Identifier (e.g. ROTN, BLORE)</label>
                <asp:TextBox ID="txtConsigText" runat="server" placeholder="ROTN" MaxLength="30" style="text-transform:uppercase;"/></div>
            <div class="form-group" style="flex:0 0 auto;">
                <asp:Button ID="btnCreateConsignment" runat="server" Text="Create" CssClass="btn btn-primary"
                    OnClick="btnCreateConsignment_Click" CausesValidation="false" style="font-size:11px;padding:8px 16px;"/></div>
        </div>
    </div>

    <!-- ══════ CONSIGNMENT CONTENT PANEL ══════ -->
    <asp:Panel ID="pnlConsigContent" runat="server" Visible="false">

    <!-- Consignment Status Bar -->
    <div class="card" style="padding:12px 18px;margin-bottom:12px;">
        <div style="display:flex;justify-content:space-between;align-items:center;">
            <div>
                <span style="font-size:14px;font-weight:700;"><asp:Label ID="lblConsigDCTitle" runat="server" /></span>
                <asp:Label ID="lblConsigStatus" runat="server" style="margin-left:10px;font-size:11px;font-weight:700;padding:3px 10px;border-radius:12px;" />
            </div>
            <div style="display:flex;gap:6px;align-items:center;">
                <asp:Button ID="btnSyncFromZoho" runat="server" Text="Sync from Zoho" CssClass="btn btn-secondary"
                    OnClick="btnSyncFromZoho_Click" CausesValidation="false" Visible="false" style="font-size:11px;padding:6px 14px;border-color:#0078d4;color:#0078d4;" />
                <asp:Button ID="btnBulkInvoice" runat="server" Text="Create All Invoices" CssClass="btn btn-zoho"
                    OnClick="btnBulkInvoice_Click" CausesValidation="false" Visible="false" style="font-size:11px;padding:6px 14px;" />
                <asp:Button ID="btnDispatchConsig" runat="server" Text="Dispatch" CssClass="btn btn-primary"
                    OnClick="btnDispatchConsig_Click" CausesValidation="false" Visible="false" style="font-size:11px;padding:6px 14px;" />
                <asp:Button ID="btnArchiveConsig" runat="server" Text="Archive" CssClass="btn btn-secondary"
                    OnClick="btnArchiveConsig_Click" CausesValidation="false" Visible="false" style="font-size:11px;padding:6px 14px;" />
            </div>
        </div>
        <!-- Dispatch form -->
        <asp:Panel ID="pnlDispatchForm" runat="server" Visible="false" style="margin-top:10px;background:#fffdf5;border:1px solid #f0e6c0;border-radius:8px;padding:12px;">
            <div class="form-row" style="align-items:flex-end;">
                <div class="form-group" style="flex:1;"><label>Vehicle Number</label>
                    <asp:TextBox ID="txtVehicleNo" runat="server" placeholder="TN 01 AB 1234" style="text-transform:uppercase;" /></div>
                <div class="form-group" style="flex:0 0 auto;">
                    <asp:Button ID="btnConfirmDispatch" runat="server" Text="Confirm Dispatch" CssClass="btn btn-primary"
                        OnClick="btnConfirmDispatch_Click" CausesValidation="false" style="font-size:11px;padding:8px 14px;" /></div>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlBulkResult" runat="server" Visible="false" style="margin-top:10px;">
            <asp:Literal ID="litBulkResult" runat="server" />
        </asp:Panel>
    </div>

    <!-- Consignment DC List -->
    <asp:Panel ID="pnlConsigDCs" runat="server" Visible="false">
    <div class="card" style="margin-bottom:16px;">
        <table class="data-table" style="font-size:11px;">
            <thead><tr><th>DC No.</th><th>Customer</th><th>Status</th><th class="num">Amount</th><th>Invoice</th><th>Transport</th><th>Tracking#</th><th></th></tr></thead>
            <tbody>
                <asp:Repeater ID="rptConsigDCs" runat="server" OnItemCommand="rptDCs_ItemCommand">
                    <ItemTemplate><tr>
                        <td style="font-weight:600;"><%# Eval("DCNumber") %></td>
                        <td><%# Eval("CustomerName") %><div style="font-size:9px;color:var(--text-dim);"><%# Eval("CustomerCode") %></div></td>
                        <td><%# Eval("Status").ToString()=="FINALISED" ? "<span class='badge-final'>Finalised</span>" : "<span class='badge-draft'>Draft</span>" %></td>
                        <td class="num" style="font-weight:600;"><%# Eval("GrandTotal") != DBNull.Value ? string.Format("₹{0:N0}", Eval("GrandTotal")) : "—" %></td>
                        <td><%# GetInvoiceStatusBadge(Eval("DCID")) %></td>
                        <td style="font-size:10px;"><%# GetTransportLabel(Eval("TransportMode"), Eval("CourierName")) %></td>
                        <td style="font-size:10px;color:var(--text-muted);"><%# Eval("TrackingNumber") != DBNull.Value && Eval("TrackingNumber").ToString() != "" ? Eval("TrackingNumber") : "—" %></td>
                        <td><asp:LinkButton runat="server" CommandName="EditDC" CommandArgument='<%# Eval("DCID") %>' CssClass="act-link" CausesValidation="false"><%# Eval("Status").ToString()=="DRAFT" ? "Edit" : "View" %></asp:LinkButton></td>
                    </tr></ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
        <asp:Panel ID="pnlConsigEmpty" runat="server"><div class="empty-note">No DCs in this consignment yet</div></asp:Panel>
    </div>
    </asp:Panel>

    <!-- SUB-TABS: DCs | Sales Force Orders -->
    <div class="ship-tab-bar">
        <button type="button" class="ship-tab active" onclick="switchShipTab('dc')">Delivery Challans</button>
        <button type="button" class="ship-tab" onclick="switchShipTab('sa')">Sales Force Orders</button>
    </div>

    <div id="tabDC" class="ship-tab-panel active">

    <!-- ══════ DC FORM ══════ -->
    <asp:Panel ID="pnlForm" runat="server">
    <div class="card">
        <div class="card-title"><asp:Label ID="lblFormTitle" runat="server">New Delivery Challan</asp:Label></div>
        <div class="form-row">
            <div class="form-group"><label>DC Number</label>
                <asp:TextBox ID="txtDCNumber" runat="server" ReadOnly="true" placeholder="Auto-generated"/></div>
            <div class="form-group" style="position:relative;"><label>Customer <span class="req">*</span></label>
                <input type="text" id="txtCustomerSearch" placeholder="Click to select customer..."
                    onfocus="this.blur();openCustomerModal();"
                    readonly="readonly"
                    style="margin-bottom:4px;padding:8px 12px;border:1.5px solid #e0e0e0;border-radius:8px;font-size:12px;background:#fffdf5;outline:none;width:100%;cursor:pointer;" autocomplete="off"/>
                <asp:DropDownList ID="ddlCustomer" runat="server" style="display:none;"/></div>
            <div class="form-group"><label>DC Date <span class="req">*</span></label>
                <asp:TextBox ID="txtDCDate" runat="server" TextMode="Date"/></div>
            <div class="form-group"><label>Channel <span class="req">*</span></label>
                <asp:DropDownList ID="ddlChannel" runat="server" onchange="onChannelChange();">
                    <asp:ListItem Text="General Trade" Value="GT"/>
                    <asp:ListItem Text="Super Market" Value="SM"/>
                </asp:DropDownList></div>
        </div>
        <div class="form-row" style="margin-top:10px;">
            <div class="form-group"><label>Transport Mode</label>
                <asp:DropDownList ID="ddlTransport" runat="server" onchange="onTransportChange();">
                    <asp:ListItem Text="-- Select --" Value=""/>
                    <asp:ListItem Text="Full Load - Own Vehicle" Value="FULL_LOAD"/>
                    <asp:ListItem Text="Courier" Value="COURIER"/>
                </asp:DropDownList></div>
            <div class="form-group" id="divCourier" style="display:none;"><label>Courier Name</label>
                <asp:TextBox ID="txtCourierName" runat="server" placeholder="e.g. DTDC, BlueDart" /></div>
            <div class="form-group" id="divTracking" style="display:none;"><label>Tracking / LR Number</label>
                <asp:TextBox ID="txtTrackingNo" runat="server" placeholder="Optional" /></div>
        </div>
        <div id="divCustGST" style="display:none;padding:8px 14px;background:#f8f7f5;border-radius:8px;margin-bottom:12px;font-size:12px;">
            <span style="color:var(--text-muted);font-weight:600;">GSTIN:</span> <span id="spnCustGSTIN" style="font-weight:700;">—</span>
            &nbsp;&bull;&nbsp; <span id="spnCustState" style="color:var(--text-muted);">—</span>
            &nbsp;&bull;&nbsp; Margin: <span id="spnMarginPct" style="font-weight:700;color:#0078d4;">—</span>
        </div>

        <!-- ADD PRODUCT LINE -->
        <div style="margin-top:20px;padding-top:16px;border-top:1px solid var(--border);">
            <div style="font-size:12px;font-weight:700;color:var(--text-muted);margin-bottom:10px;">ADD PRODUCTS TO THIS DC</div>
            <div class="form-row" style="align-items:flex-end;">
                <div class="form-group" style="flex:3;"><label>Product</label>
                    <select id="selProduct" onchange="onProductSelect();"><option value="0">-- Select Product --</option></select></div>
                <div class="form-group" style="flex:1;"><label>Selling Form</label>
                    <select id="selForm" onchange="onProductSelect();">
                        <option value="JAR">JAR</option>
                        <option value="BOX">BOX</option>
                        <option value="PCS">PCS</option>
                        <option value="CASE">CASE</option>
                    </select></div>
                <div class="form-group" style="flex:1;"><label>Source</label>
                    <select id="selSource" onchange="onProductSelect();">
                        <option value="CASE">From Cases</option>
                        <option value="LOOSE">From Loose</option>
                    </select></div>
                <div class="form-group" style="flex:1;"><label>Quantity</label>
                    <input type="number" id="txtLineQty" min="1" step="1" placeholder="0"/></div>
            </div>
            <div id="stockInfo" style="font-size:12px;color:var(--text-dim);margin-top:4px;"></div>
            <button type="button" class="btn-add" onclick="addLine();" style="margin-top:10px;">+ Add Product Line</button>
        </div>

        <!-- LINE ITEMS TABLE -->
        <table class="line-table" id="lineTable" style="display:none;">
            <thead><tr><th>Product</th><th>HSN</th><th class="num">Form</th><th class="num">Source</th><th class="num">Qty</th><th class="num">MRP</th><th class="num">Rate</th><th class="num">Taxable</th><th class="num">GST</th><th class="num">Total</th><th></th></tr></thead>
            <tbody id="lineBody"></tbody>
            <tfoot><tr style="font-weight:700;"><td colspan="7">TOTAL</td><td class="num" id="ftTaxable">0</td><td class="num" id="ftGST">0</td><td class="num" id="ftTotal">0</td><td></td></tr></tfoot>
        </table>

        <div class="form-group" style="margin-top:14px;"><label>Remarks</label>
            <asp:TextBox ID="txtRemarks" runat="server" TextMode="MultiLine" Rows="2" MaxLength="500" placeholder="Optional notes"/></div>

        <div class="btn-row">
            <asp:Button ID="btnDraftSave" runat="server" Text="&#x1F4BE; Save as Draft" CssClass="btn btn-primary" OnClick="btnDraftSave_Click" OnClientClick="syncLines();" CausesValidation="false"/>
            <asp:Button ID="btnFinalise" runat="server" Text="&#x2705; Finalise Shipment" CssClass="btn btn-success" OnClick="btnFinalise_Click" CausesValidation="false"/>
            <asp:Button ID="btnCreateInvoiceDraft" runat="server" Text="&#x1F4E8; Create Invoice in Zoho" CssClass="btn btn-zoho"
                OnClientClick="doCreateInvoiceDraftConfirm(); return false;" CausesValidation="false" Visible="false"/>
            <asp:Button ID="btnCreateInvoiceDraftHidden" runat="server" OnClick="btnCreateInvoiceDraft_Click" CausesValidation="false" style="display:none;"/>
            <asp:Button ID="btnNew" runat="server" Text="+ New DC" CssClass="btn btn-secondary" OnClick="btnNew_Click" OnClientClick="document.getElementById('txtCustomerSearch').value='';" CausesValidation="false"/>
            <asp:Button ID="btnPrintDC" runat="server" Text="&#x1F4C4; Download DC" CssClass="btn btn-secondary" OnClick="btnPrintDC_Click" CausesValidation="false"/>
            <asp:Button ID="btnDeleteDC" runat="server" Text="&#x1F5D1; Delete DC" CssClass="btn btn-danger" OnClick="btnDeleteDC_Click" OnClientClick="return doDeleteDCConfirm();" CausesValidation="false"/>
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
            <HeaderTemplate><table class="data-table"><thead><tr><th>Product</th><th>HSN</th><th class="num">Form</th><th class="num">Source</th><th class="num">Qty</th><th class="num">MRP</th><th class="num">Rate</th><th class="num">Taxable</th><th class="num">GST</th><th class="num">Total</th></tr></thead><tbody></HeaderTemplate>
            <ItemTemplate><tr>
                <td><strong><%# Eval("ProductName") %></strong><div style="font-size:10px;color:var(--text-dim);"><%# Eval("ProductCode") %></div></td>
                <td style="font-size:10px;color:var(--text-muted);"><%# Eval("HSNCode") %></td>
                <td class="num" style="font-weight:600;color:#0078d4;"><%# Eval("SellingForm") %></td>
                <td class="num" style="font-size:10px;"><%# Eval("Source") != DBNull.Value && Eval("Source").ToString() == "LOOSE" ? "<span style='color:#27ae60'>Loose</span>" : "<span style='color:#e67e22'>Cases</span>" %></td>
                <td class="num"><%# string.Format("{0:N0}", Eval("TotalPcs")) %></td>
                <td class="num" style="color:var(--text-dim);"><%# Eval("MRP") != DBNull.Value ? string.Format("₹{0:N2}", Eval("MRP")) : "—" %></td>
                <td class="num" style="font-weight:700;"><%# Eval("UnitRate") != DBNull.Value ? string.Format("₹{0:N2}", Eval("UnitRate")) : "—" %></td>
                <td class="num"><%# Eval("TaxableValue") != DBNull.Value ? string.Format("₹{0:N2}", Eval("TaxableValue")) : "—" %></td>
                <td class="num" style="font-size:10px;"><%# GetGSTDisplay(Eval("CGSTAmt"), Eval("SGSTAmt"), Eval("IGSTAmt"), Eval("GSTRate")) %></td>
                <td class="num" style="font-weight:700;color:var(--teal);"><%# Eval("LineTotal") != DBNull.Value ? string.Format("₹{0:N2}", Eval("LineTotal")) : "—" %></td>
            </tr></ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>

        <!-- Invoice Status — shown after invoice is created -->
        <asp:Panel ID="pnlInvoiceStatus" runat="server" Visible="false">
            <div style="margin-top:12px;padding:16px 20px;border-radius:10px;border:1px solid #b3d7ff;background:#e6f2ff;">
                <div style="display:flex;align-items:center;justify-content:space-between;flex-wrap:wrap;gap:12px;">
                    <div>
                        <div style="font-size:11px;font-weight:700;color:#0078d4;text-transform:uppercase;letter-spacing:.06em;">Zoho Books Invoice</div>
                        <div style="font-size:16px;font-weight:800;margin-top:2px;"><asp:Label ID="lblInvoiceNo" runat="server"/></div>
                        <div style="font-size:12px;color:#555;margin-top:4px;">
                            Status: <asp:Label ID="lblInvoiceZohoStatus" runat="server" style="font-weight:700;"/>
                            &nbsp;&bull;&nbsp; <asp:Label ID="lblInvoiceAmount" runat="server" style="font-weight:600;"/>
                        </div>
                    </div>
                    <div style="display:flex;gap:8px;flex-wrap:wrap;">
                        <asp:HyperLink ID="lnkViewInZoho" runat="server" Target="_blank"
                            CssClass="btn btn-zoho" style="font-size:11px;padding:7px 14px;text-decoration:none;">
                            &#x1F517; View / Edit in Zoho
                        </asp:HyperLink>
                        <asp:Button ID="btnDownloadInvoicePDF" runat="server" Text="&#x1F4C4; Download Invoice PDF"
                            CssClass="btn btn-primary" style="font-size:11px;padding:7px 14px;"
                            OnClick="btnDownloadInvoicePDF_Click" CausesValidation="false"/>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- Invoice Error -->
        <asp:Panel ID="pnlInvoiceError" runat="server" Visible="false">
            <div style="margin-top:12px;padding:12px 18px;border-radius:10px;border:1px solid #f5c6cb;background:#fdf3f2;font-size:12px;">
                <strong style="color:#e74c3c;">&#x26A0; Invoice Error:</strong> <asp:Label ID="lblInvoiceError" runat="server" style="color:#c0392b;"/>
            </div>
        </asp:Panel>

        <div class="btn-row" style="margin-top:14px;">
            <asp:Button ID="btnDownloadFromView" runat="server" Text="&#x1F4C4; Download DC" CssClass="btn btn-primary" OnClick="btnPrintDC_Click" CausesValidation="false"/>
            <asp:Button ID="btnNewFromLocked" runat="server" Text="+ Create New DC" CssClass="btn btn-secondary" OnClick="btnNew_Click" OnClientClick="document.getElementById('txtCustomerSearch').value='';" CausesValidation="false"/>

            <!-- Zoho Invoice Creation — visible when no invoice exists yet -->
            <asp:Panel ID="pnlCreateInvoice" runat="server" Visible="false">
                <asp:Button ID="btnCreateInvoice" runat="server" Text="&#x1F4E8; Create Invoice in Zoho" CssClass="btn btn-zoho"
                    OnClientClick="doCreateInvoiceConfirm(); return false;" CausesValidation="false"/>
                <asp:Button ID="btnCreateInvoiceHidden" runat="server" OnClick="btnCreateInvoice_Click" CausesValidation="false" style="display:none;"/>
            </asp:Panel>
        </div>
    </div>
    </asp:Panel>

    <!-- ══════ RECENT DCs LIST (legacy - remove old standalone list) ══════ -->
    </div><!-- end tabDC -->

    <!-- ══════ SUB-TAB: SALES FORCE ORDERS ══════ -->
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

    </div><!-- end tabSA -->
    </asp:Panel><!-- end pnlConsigContent -->

    <!-- ══════ SALES PROJECTIONS (always visible, outside consignment) ══════ -->
    <div class="card" style="margin-top:20px;">
        <div class="card-title">Sales Projections (Read Only)</div>
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
</div>
</form>
<script>
function onTransportChange(){
    var mode=document.getElementById('<%= ddlTransport.ClientID %>').value;
    document.getElementById('divCourier').style.display=mode==='COURIER'?'flex':'none';
    document.getElementById('divTracking').style.display=(mode==='COURIER')?'flex':'none';
}
function switchShipTab(tab) {
    var panels = document.querySelectorAll('.ship-tab-panel');
    var tabs = document.querySelectorAll('.ship-tab');
    for (var i = 0; i < panels.length; i++) panels[i].classList.remove('active');
    for (var i = 0; i < tabs.length; i++) tabs[i].classList.remove('active');
    var map = {dc:'tabDC', sa:'tabSA'};
    var p = document.getElementById(map[tab]);
    if (p) p.classList.add('active');
    var idx = {dc:0, sa:1};
    if (tabs[idx[tab]]) tabs[idx[tab]].classList.add('active');
}
function toggleProjDetail(el) {
    var detail = el.parentElement.querySelector('.proj-detail');
    if (detail) detail.classList.toggle('open');
    var arrow = el.querySelector('span:last-child');
    if (arrow) arrow.innerHTML = detail.classList.contains('open') ? '&#x25B2;' : '&#x25BC;';
}
// ── Customer Modal Search (GRN-style) ──
var _custModalOverlay = null;
function openCustomerModal() {
    var ddl = document.getElementById('<%= ddlCustomer.ClientID %>');
    if (!ddl) return;
    var items = [];
    for (var i = 0; i < ddl.options.length; i++) {
        if (ddl.options[i].value === '0') continue;
        items.push({ value: ddl.options[i].value, text: ddl.options[i].text, idx: i });
    }
    if (_custModalOverlay) _custModalOverlay.remove();

    var ov = document.createElement('div');
    ov.style.cssText = 'position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,.5);z-index:9999;display:flex;align-items:flex-start;justify-content:center;padding:40px 16px 0;';

    var box = document.createElement('div');
    box.style.cssText = 'background:#fff;border-radius:14px;width:100%;max-width:540px;max-height:80vh;display:flex;flex-direction:column;box-shadow:0 8px 40px rgba(0,0,0,.25);overflow:hidden;';

    // Header
    var hdr = document.createElement('div');
    hdr.style.cssText = 'padding:16px 20px 12px;border-bottom:2px solid #f0ede8;display:flex;align-items:center;justify-content:space-between;';
    hdr.innerHTML = '<span style="font-family:\'Bebas Neue\',sans-serif;font-size:18px;letter-spacing:.06em;">Select Customer</span>';
    var closeBtn = document.createElement('button'); closeBtn.type = 'button'; closeBtn.innerHTML = '\u2715';
    closeBtn.style.cssText = 'border:none;background:none;font-size:20px;cursor:pointer;color:#999;padding:4px 8px;';
    closeBtn.onclick = function() { ov.remove(); _custModalOverlay = null; };
    hdr.appendChild(closeBtn); box.appendChild(hdr);

    // Search input
    var sWrap = document.createElement('div'); sWrap.style.cssText = 'padding:12px 20px;';
    var sInput = document.createElement('input'); sInput.type = 'text'; sInput.placeholder = 'Search customer name, code...';
    sInput.style.cssText = 'width:100%;padding:12px 16px;border:2px solid #e0e0e0;border-radius:10px;font-size:16px;font-family:\'DM Sans\',sans-serif;outline:none;background:#fafafa;';
    sInput.setAttribute('autocomplete', 'off');
    sWrap.appendChild(sInput); box.appendChild(sWrap);

    // Results list
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
                row.innerHTML = escH(it.text.substring(0, idx)) +
                    '<strong style="color:var(--teal,#1a9e6a);">' + escH(it.text.substring(idx, idx + q.length)) + '</strong>' +
                    escH(it.text.substring(idx + q.length));
            } else {
                row.textContent = it.text;
            }
            row.onclick = function() {
                ddl.selectedIndex = it.idx;
                document.getElementById('txtCustomerSearch').value = it.text;
                ov.remove(); _custModalOverlay = null;
                showCustInfo();
                renderLines();
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
    _custModalOverlay = ov;
    ov.onclick = function(e) { if (e.target === ov) { ov.remove(); _custModalOverlay = null; } };
    renderList('');
    setTimeout(function() { sInput.focus(); }, 150);
}
function escH(s) { var d = document.createElement('div'); d.textContent = s; return d.innerHTML; }

var productData={};
try{productData=JSON.parse(document.getElementById('<%= hfProductData.ClientID %>').value||'{}');}catch(e){}
var customerData={};
try{customerData=JSON.parse(document.getElementById('<%= hfCustomerData.ClientID %>').value||'{}');}catch(e){}
var orgState='<%= hfOrgState.Value %>';
var lines=[];

function getMarginPct(){
    var ch=document.getElementById('<%= ddlChannel.ClientID %>').value;
    var ddlC=document.getElementById('<%= ddlCustomer.ClientID %>');
    if(!ddlC||ddlC.selectedIndex<=0)return 0;
    var cid=ddlC.value;
    var c=customerData[cid];
    if(!c)return 0;
    return ch==='SM'?parseFloat(c.sm)||0:parseFloat(c.gt)||0;
}
function isInterState(){
    var ddlC=document.getElementById('<%= ddlCustomer.ClientID %>');
    if(!ddlC||ddlC.selectedIndex<=0)return false;
    var cid=ddlC.value;var c=customerData[cid];
    if(!c||!c.state)return false;
    return c.state.toLowerCase().trim()!==orgState.toLowerCase().trim();
}
function showCustInfo(){
    var ddlC=document.getElementById('<%= ddlCustomer.ClientID %>');
    var div=document.getElementById('divCustGST');
    if(!ddlC||ddlC.selectedIndex<=0){div.style.display='none';return;}
    var cid=ddlC.value;var c=customerData[cid];
    if(!c){div.style.display='none';return;}
    div.style.display='block';
    document.getElementById('spnCustGSTIN').innerText=c.gstin||'—';
    document.getElementById('spnCustState').innerText=c.state||'—';
    document.getElementById('spnMarginPct').innerText=getMarginPct().toFixed(1)+'%';
}
function onChannelChange(){showCustInfo();renderLines();}
// Override customer modal selection callback
var _origSelectCust = null;
window.addEventListener('load',function(){
    // Restore customer search text from dropdown selection
    var ddlCust = document.getElementById('<%= ddlCustomer.ClientID %>');
    if (ddlCust && ddlCust.selectedIndex > 0) {
        document.getElementById('txtCustomerSearch').value = ddlCust.options[ddlCust.selectedIndex].text;
    }
    showCustInfo();
    var sel=document.getElementById('selProduct');
    for(var pid in productData){
        var p=productData[pid];
        var avCases=parseInt(p.availCases)||0;
        var avLoose=parseInt(p.availLoose)||0;
        var jpc=parseInt(p.jarsPerCase)||12;
        var opt=document.createElement('option');
        opt.value=pid;
        var looseLabel=p.containerType==='BOX'?'boxes':'jars';
        opt.text=p.name+' ('+p.code+') — '+avCases+' cases'+(avLoose>0?' + '+avLoose+' '+looseLabel:'');
        opt.setAttribute('data-avcases',avCases);
        opt.setAttribute('data-avloose',avLoose);
        opt.setAttribute('data-avjars',parseInt(p.availJars)||0);
        opt.setAttribute('data-jpc',jpc);
        opt.setAttribute('data-unitsize',parseInt(p.unitSize)||1);
        opt.setAttribute('data-ct',p.containerType||'JAR');
        sel.appendChild(opt);
    }
    var raw=document.getElementById('<%= hfLines.ClientID %>').value;
    if(raw){try{lines=JSON.parse(raw);}catch(e){lines=[];}renderLines();}
});

function onProductSelect(){
    var sel=document.getElementById('selProduct');
    var info=document.getElementById('stockInfo');
    if(sel.value==='0'){info.innerHTML='';return;}
    var p=productData[sel.value];
    if(!p){info.innerHTML='';return;}
    var jpc=parseInt(p.jarsPerCase)||12;
    var avCases=parseInt(p.availCases)||0;
    var avLoose=parseInt(p.availLoose)||0;

    // Subtract already-added lines (for unsaved DCs)
    var dcId=document.getElementById('<%= hfDCID.ClientID %>').value;
    if(!dcId||dcId==='0'){
        lines.forEach(function(l){
            if(l.pid!==sel.value) return;
            if(l.source==='CASE') avCases-=Math.ceil(l.qty/jpc);
            else avLoose-=l.qty;
        });
    }
    if(avCases<0)avCases=0;if(avLoose<0)avLoose=0;

    var source=document.getElementById('selSource').value;
    var form=document.getElementById('selForm').value;
    // Auto-set selling form based on container type
    var ct=p.containerType||'JAR';
    var selFormEl=document.getElementById('selForm');
    if(ct==='BOX'){selFormEl.value='BOX';form='BOX';}
    else if(ct==='DIRECT'){selFormEl.value='PCS';form='PCS';}
    else {selFormEl.value='JAR';form='JAR';}
    var mrp=0;
    if(p){
        if(form==='PCS') mrp=parseFloat(p.mrpPcs)||0;
        else if(form==='JAR') mrp=parseFloat(p.mrpJar)||0;
        else if(form==='BOX') mrp=parseFloat(p.mrpBox)||0;
        else if(form==='CASE') mrp=parseFloat(p.mrpCase)||0;
    }
    var mrpInfo=mrp>0?' | MRP: ₹'+mrp:'';
    if(source==='CASE'){
        var maxJars=avCases*jpc;
        info.innerHTML='<span class="stock-badge'+(avCases<=0?' stock-zero':'')+'">From Cases: '+avCases+' cases ('+maxJars+' '+form+'s)'+mrpInfo+'</span>';
    } else {
        var looseUnit=ct==='BOX'?'boxes':'jars';
        info.innerHTML='<span class="stock-badge'+(avLoose<=0?' stock-zero':'')+'">From Loose: '+avLoose+' '+looseUnit+' available'+mrpInfo+'</span>';
    }
}

function addLine(){
    var sel=document.getElementById('selProduct');
    var pid=sel.value;
    if(pid==='0'){erpAlert('Please select a product before adding.', {title:'Selection Required', type:'warn'});return;}
    var qty=parseInt(document.getElementById('txtLineQty').value)||0;
    if(qty<=0){erpAlert('Please enter quantity.', {title:'Quantity Required', type:'warn'});return;}
    var form=document.getElementById('selForm').value;
    var source=document.getElementById('selSource').value; // CASE or LOOSE

    var p=productData[pid];
    if(!p){erpAlert('Product data not found.', {title:'Error', type:'danger'});return;}

    var jpc=parseInt(p.jarsPerCase)||12;
    var us=parseInt(p.unitSize)||1;

    // Calculate available stock after existing lines
    var avCases=parseInt(p.availCases)||0;
    var avLoose=parseInt(p.availLoose)||0;
    var dcId=document.getElementById('<%= hfDCID.ClientID %>').value;
    if(!dcId||dcId==='0'){
        lines.forEach(function(l){
            if(l.pid!==pid) return;
            if((l.source||'CASE')==='CASE') avCases-=Math.ceil(l.qty/jpc);
            else avLoose-=l.qty;
        });
    }
    if(avCases<0)avCases=0;if(avLoose<0)avLoose=0;

    // Validate based on source
    if(source==='CASE'){
        if(form!=='CASE' && qty%jpc!==0){
            erpAlert(form+' quantity must be in multiples of '+jpc+' when sourcing from Cases.', {title:'Invalid Quantity', type:'warn'});return;}
        var casesNeeded=form==='CASE'?qty:Math.ceil(qty/jpc);
        if(casesNeeded>avCases){
            erpAlert('Insufficient CASES for '+p.name+'. Need '+casesNeeded+' cases, available '+avCases+'.', {title:'Stock Insufficient', type:'danger'});return;}
    } else {
        var looseNeeded=qty;
        if(form==='PCS') looseNeeded=Math.ceil(qty/us);
        if(looseNeeded>avLoose){
            erpAlert('Insufficient loose stock for '+p.name+'. Need '+looseNeeded+', available '+avLoose+' loose.', {title:'Stock Insufficient', type:'danger'});return;}
    }

    // Get MRP for selected selling form
    var mrp=0;
    if(form==='PCS') mrp=parseFloat(p.mrpPcs)||0;
    else if(form==='JAR') mrp=parseFloat(p.mrpJar)||0;
    else if(form==='BOX') mrp=parseFloat(p.mrpBox)||0;
    else if(form==='CASE') mrp=parseFloat(p.mrpCase)||0;
    if(mrp<=0){erpAlert('MRP not set for '+p.name+' ('+form+'). Configure in Product MRP page.', {title:'MRP Missing', type:'warn'});return;}

    var marginPct=getMarginPct();
    var rate=Math.round(mrp*(1-marginPct/100)*100)/100;
    var gstRate=parseFloat(p.gstRate)||0;
    var hsn=p.hsn||'';
    var rateExGST=gstRate>0?rate/(1+gstRate/100):rate;
    var taxableVal=Math.round(rateExGST*qty*100)/100;
    var gstAmt=Math.round((rate*qty-taxableVal)*100)/100;
    var lineTotal=Math.round(rate*qty*100)/100;

    lines.push({pid:pid,name:p.name,code:p.code,form:form,source:source,qty:qty,
        hsn:hsn,gstRate:gstRate,mrp:mrp,marginPct:marginPct,rate:rate,
        taxableVal:taxableVal,gstAmt:gstAmt,lineTotal:lineTotal});
    renderLines();
    document.getElementById('txtLineQty').value='';
    sel.selectedIndex=0;
    document.getElementById('stockInfo').innerHTML='';
}

function removeLine(idx){lines.splice(idx,1);renderLines();}

function updateDropdownStock(){
    var sel=document.getElementById('selProduct');
    var dcId=document.getElementById('<%= hfDCID.ClientID %>').value;
    var isSaved=dcId&&dcId!=='0';
    for(var i=1;i<sel.options.length;i++){
        var opt=sel.options[i];var pid=opt.value;
        var p=productData[pid];
        if(!p) continue;
        var avCases=parseInt(p.availCases)||0;
        var avLoose=parseInt(p.availLoose)||0;
        // For unsaved DCs, subtract lines already added
        if(!isSaved){
            var jpc=parseInt(p.jarsPerCase)||12;
            lines.forEach(function(l){
                if(l.pid!==pid) return;
                if((l.source||'CASE')==='CASE') avCases-=Math.ceil(l.qty/jpc);
                else avLoose-=l.qty;
            });
        }
        if(avCases<0)avCases=0;if(avLoose<0)avLoose=0;
        var ll=p.containerType==='BOX'?'boxes':'jars';
        opt.text=p.name+' ('+p.code+') — '+avCases+' cases'+(avLoose>0?' + '+avLoose+' '+ll:'');
        opt.setAttribute('data-avcases',avCases);
        opt.setAttribute('data-avloose',avLoose);
    }
}

function fmt(n){return '₹'+n.toLocaleString('en-IN',{minimumFractionDigits:2,maximumFractionDigits:2});}

function renderLines(){
    var body=document.getElementById('lineBody');
    var tbl=document.getElementById('lineTable');
    body.innerHTML='';
    if(lines.length===0){tbl.style.display='none';syncLines();updateDropdownStock();return;}
    tbl.style.display='table';
    var sumTax=0,sumGST=0,sumTotal=0;
    var inter=isInterState();
    var marginPct=getMarginPct();
    lines.forEach(function(l,i){
        var p=productData[l.pid];
        // Get MRP for the line's selling form
        var mrp=l.mrp;
        if(p){
            if(l.form==='PCS') mrp=parseFloat(p.mrpPcs)||mrp;
            else if(l.form==='JAR') mrp=parseFloat(p.mrpJar)||mrp;
            else if(l.form==='BOX') mrp=parseFloat(p.mrpBox)||mrp;
            else if(l.form==='CASE') mrp=parseFloat(p.mrpCase)||mrp;
        }
        var gstRate=parseFloat(p?p.gstRate:l.gstRate)||l.gstRate||0;
        var rate=Math.round(mrp*(1-marginPct/100)*100)/100;
        var rateExGST=gstRate>0?rate/(1+gstRate/100):rate;
        var taxableVal=Math.round(rateExGST*l.qty*100)/100;
        var gstAmt=Math.round((rate*l.qty-taxableVal)*100)/100;
        var lineTotal=Math.round(rate*l.qty*100)/100;
        l.mrp=mrp;l.marginPct=marginPct;l.rate=rate;l.taxableVal=taxableVal;l.gstAmt=gstAmt;l.lineTotal=lineTotal;

        var gstLabel=inter?'IGST '+gstRate+'%':'C+S '+(gstRate/2)+'%+'+(gstRate/2)+'%';
        var tr=document.createElement('tr');
        tr.innerHTML='<td><strong>'+l.name+'</strong><div style="font-size:10px;color:var(--text-dim);">'+l.code+'</div></td>'
            +'<td style="font-size:10px;color:var(--text-muted);">'+l.hsn+'</td>'
            +'<td class="num" style="font-weight:600;color:#0078d4;">'+l.form+'</td>'
            +'<td class="num" style="font-size:10px;color:'+((l.source||'CASE')==='CASE'?'#e67e22':'#27ae60')+';">'+((l.source||'CASE')==='CASE'?'Cases':'Loose')+'</td>'
            +'<td class="num">'+l.qty+'</td>'
            +'<td class="num" style="color:var(--text-dim);">'+fmt(mrp)+'</td>'
            +'<td class="num" style="font-weight:700;">'+fmt(rate)+'</td>'
            +'<td class="num">'+fmt(taxableVal)+'</td>'
            +'<td class="num" style="font-size:10px;">'+fmt(gstAmt)+'<div style="font-size:9px;color:#999;">'+gstLabel+'</div></td>'
            +'<td class="num" style="font-weight:700;color:var(--teal);">'+fmt(lineTotal)+'</td>'
            +'<td><button type="button" class="btn-remove" onclick="removeLine('+i+')">Remove</button></td>';
        body.appendChild(tr);
        sumTax+=taxableVal;sumGST+=gstAmt;sumTotal+=lineTotal;
    });
    document.getElementById('ftTaxable').innerText=fmt(sumTax);
    document.getElementById('ftGST').innerText=fmt(sumGST);
    document.getElementById('ftTotal').innerText=fmt(sumTotal);
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
<script>
function doCreateInvoiceDraftConfirm(){
    syncLines();
    var btn = document.getElementById('<%= btnCreateInvoiceDraft.ClientID %>');
    var isUpdate = btn && btn.value.indexOf('Update') >= 0;
    erpConfirm(isUpdate
        ? 'Save this DC and update the existing Zoho Books invoice?'
        : 'Save this DC and create a new Zoho Books invoice?', {
        title: isUpdate ? 'Update Invoice' : 'Create Invoice',
        type: 'info',
        okText: isUpdate ? 'Update Invoice' : 'Create Invoice',
        onOk: function(){ setTimeout(function(){ document.getElementById('<%= btnCreateInvoiceDraftHidden.ClientID %>').click(); }, 300); }
    });
}
function doCreateInvoiceConfirm(){
    var btn = document.getElementById('<%= btnCreateInvoice.ClientID %>');
    var isUpdate = btn && btn.value.indexOf('Update') >= 0;
    erpConfirm(isUpdate
        ? 'Update the Zoho Books invoice for this DC?'
        : 'Create a Zoho Books invoice for this DC?', {
        title: isUpdate ? 'Update Invoice' : 'Create Invoice',
        type: 'info',
        okText: isUpdate ? 'Update Invoice' : 'Create Invoice',
        onOk: function(){ setTimeout(function(){ document.getElementById('<%= btnCreateInvoiceHidden.ClientID %>').click(); }, 300); }
    });
}
function doDeleteDCConfirm(){
    erpConfirm('Are you sure you want to DELETE this entire Delivery Challan? All reserved stock will be freed. This cannot be undone.', {
        title: 'Delete Delivery Challan',
        type: 'danger',
        okText: 'Delete',
        onOk: function(){ __doPostBack('<%= btnDeleteDC.UniqueID %>', ''); }
    });
    return false;
}
</script>
<script src="/StockApp/erp-modal.js"></script>
<script src="/StockApp/erp-keepalive.js"></script>
</body></html>
