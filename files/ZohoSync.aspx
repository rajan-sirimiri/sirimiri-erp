<%@ Page Language="C#" AutoEventWireup="true" Inherits="StockApp.ZohoSync" EnableEventValidation="false" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri ERP — Zoho Books Integration</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--accent:#0078d4;--accent-dark:#005a9e;--accent-light:#e6f2ff;
    --green:#27ae60;--green-light:#eafaf1;--red:#e74c3c;--red-light:#fdf3f2;
    --orange:#e67e22;--orange-light:#fef5ec;
    --text:#1a1a1a;--text-muted:#666;--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;
    --radius:14px;--nav-h:52px;}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);color:var(--text);min-height:100vh;}
nav{background:#1a1a1a;height:var(--nav-h);display:flex;align-items:center;padding:0 20px;gap:12px;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;}
.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.08em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:16px;}
.nav-link{color:#fff;font-size:12px;font-weight:600;text-decoration:none;opacity:.8;}.nav-link:hover{opacity:1;}
.page-body{max-width:1200px;margin:0 auto;padding:20px 20px 60px;}
.card{background:var(--surface);border-radius:var(--radius);box-shadow:0 2px 12px rgba(0,0,0,.07);padding:20px 24px;margin-bottom:20px;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.06em;margin-bottom:4px;}
.card-sub{font-size:12px;color:var(--text-muted);margin-bottom:16px;}
.alert{padding:12px 18px;border-radius:10px;font-size:13px;font-weight:600;margin-bottom:16px;}
.alert-success{background:var(--green-light);color:var(--green);border:1px solid #a9dfbf;}
.alert-danger{background:var(--red-light);color:var(--red);border:1px solid #f5c6cb;}
.alert-info{background:var(--accent-light);color:var(--accent);border:1px solid #b3d7ff;}
.btn{border:none;border-radius:9px;padding:10px 22px;font-size:12px;font-weight:700;cursor:pointer;letter-spacing:.04em;transition:background .2s;}
.btn-blue{background:var(--accent);color:#fff;}.btn-blue:hover{background:var(--accent-dark);}
.btn-green{background:var(--green);color:#fff;}.btn-green:hover{background:#219a52;}
.btn-orange{background:var(--orange);color:#fff;}.btn-orange:hover{background:#d35400;}
.btn-sm{padding:6px 14px;font-size:11px;}
.status-badge{display:inline-block;padding:3px 10px;border-radius:20px;font-size:10px;font-weight:700;letter-spacing:.04em;}
.badge-synced{background:var(--green-light);color:var(--green);}
.badge-pending{background:var(--orange-light);color:var(--orange);}
.badge-error{background:var(--red-light);color:var(--red);}
.badge-new{background:#e8e5e0;color:#999;}
table.data{width:100%;border-collapse:collapse;font-size:12px;}
table.data th{text-align:left;padding:8px 10px;background:#f8f7f5;border-bottom:2px solid var(--border);
    font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-muted);}
table.data td{padding:8px 10px;border-bottom:1px solid #f0f0f0;}
table.data tr:hover{background:#fafafa;}
.conn-status{display:flex;align-items:center;gap:12px;}
.conn-dot{width:12px;height:12px;border-radius:50%;display:inline-block;}
.conn-dot.ok{background:var(--green);box-shadow:0 0 6px rgba(39,174,96,.4);}
.conn-dot.err{background:var(--red);box-shadow:0 0 6px rgba(231,76,60,.4);}
.conn-dot.unknown{background:#ccc;}
.section-tabs{display:flex;gap:4px;margin-bottom:16px;border-bottom:2px solid var(--border);padding-bottom:0;}
.section-tab{padding:8px 18px;font-size:12px;font-weight:600;color:var(--text-muted);cursor:pointer;
    border-bottom:2px solid transparent;margin-bottom:-2px;background:none;border-top:none;border-left:none;border-right:none;}
.section-tab.active{color:var(--accent);border-bottom-color:var(--accent);}
.log-detail{max-width:300px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;font-size:11px;color:var(--text-muted);}
</style>
</head>
<body>
<form id="form1" runat="server">

<asp:HiddenField ID="hfActiveTab" runat="server" Value="connection"/>

<nav>
    <a class="nav-logo" href="ERPHome.aspx">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    </a>
    <span class="nav-title">Zoho Books Integration</span>
    <div class="nav-right">
        <a href="ERPHome.aspx" class="nav-link">&#8592; ERP Home</a>
    </div>
</nav>

<div class="page-body">

    <!-- ALERT -->
    <asp:Panel ID="pnlAlert" runat="server" Visible="false">
        <div class="alert"><asp:Label ID="lblAlert" runat="server"/></div>
    </asp:Panel>

    <!-- TABS -->
    <div class="section-tabs">
        <button type="button" class="section-tab active" onclick="showTab('connection')">Connection</button>
        <button type="button" class="section-tab" onclick="showTab('products')">Products</button>
        <button type="button" class="section-tab" onclick="showTab('customers')">Customers</button>
        <button type="button" class="section-tab" onclick="showTab('suppliers')">Suppliers</button>
        <button type="button" class="section-tab" onclick="showTab('log')">Sync Log</button>
    </div>

    <!-- ═══ CONNECTION TAB ═══ -->
    <div id="tab-connection" class="tab-panel">
        <div class="card">
            <div class="card-title">Zoho Books Connection</div>
            <div class="card-sub">Test and verify your Zoho Books API connection</div>
            <div class="conn-status" style="margin-bottom:16px;">
                <span class='conn-dot <asp:Literal ID="litConnClass" runat="server" Text="unknown"/>'></span>
                <span style="font-weight:600;"><asp:Label ID="lblConnStatus" runat="server" Text="Not tested yet"/></span>
            </div>
            <table style="font-size:12px;margin-bottom:16px;" cellpadding="6">
                <tr><td style="color:var(--text-muted);font-weight:600;">Organization ID</td><td><asp:Label ID="lblOrgId" runat="server"/></td></tr>
                <tr><td style="color:var(--text-muted);font-weight:600;">API Domain</td><td><asp:Label ID="lblDomain" runat="server"/></td></tr>
                <tr><td style="color:var(--text-muted);font-weight:600;">Refresh Token</td><td><asp:Label ID="lblRefreshToken" runat="server"/></td></tr>
                <tr><td style="color:var(--text-muted);font-weight:600;">Access Token Expiry</td><td><asp:Label ID="lblTokenExpiry" runat="server"/></td></tr>
            </table>
            <asp:Button ID="btnTestConnection" runat="server" CssClass="btn btn-blue"
                Text="&#x1F50C; Test Connection" OnClick="btnTestConnection_Click" CausesValidation="false"/>
        </div>
    </div>

    <!-- ═══ PRODUCTS TAB ═══ -->
    <div id="tab-products" class="tab-panel" style="display:none;">
        <div class="card">
            <div class="card-title">Product → Zoho Item Sync</div>
            <div class="card-sub">Push ERP products (Core, Conversion) to Zoho Books as Items</div>
            <div style="display:flex;gap:10px;margin-bottom:16px;">
                <asp:Button ID="btnSyncAllProducts" runat="server" CssClass="btn btn-green"
                    Text="&#x1F504; Sync All Products" OnClick="btnSyncAllProducts_Click" CausesValidation="false"/>
            </div>
            <asp:Panel ID="pnlProductList" runat="server">
                <asp:Repeater ID="rptProducts" runat="server" OnItemCommand="rptProducts_ItemCommand">
                    <HeaderTemplate><table class="data"><tr>
                        <th>Code</th><th>Product Name</th><th>HSN</th><th>GST</th><th>Zoho Status</th><th>Zoho ID</th><th>Action</th>
                    </tr></HeaderTemplate>
                    <ItemTemplate><tr>
                        <td><%# Eval("ProductCode") %></td>
                        <td style="font-weight:600;"><%# Eval("ProductName") %></td>
                        <td><%# Eval("HSNCode") %></td>
                        <td><%# Eval("GSTRate") != DBNull.Value ? Convert.ToDecimal(Eval("GSTRate")).ToString("0.#") + "%" : "—" %></td>
                        <td><%# GetSyncBadge(Eval("ZohoItemID"), Eval("SyncStatus")) %></td>
                        <td style="font-size:10px;color:var(--text-muted);"><%# Eval("ZohoItemID") != DBNull.Value ? Eval("ZohoItemID").ToString() : "" %></td>
                        <td><asp:LinkButton runat="server" CommandName="SyncOne" CommandArgument='<%# Eval("ProductID") %>'
                            CssClass="btn btn-blue btn-sm" CausesValidation="false">Sync</asp:LinkButton></td>
                    </tr></ItemTemplate>
                    <FooterTemplate></table></FooterTemplate>
                </asp:Repeater>
            </asp:Panel>
        </div>
    </div>

    <!-- ═══ CUSTOMERS TAB ═══ -->
    <div id="tab-customers" class="tab-panel" style="display:none;">
        <div class="card">
            <div class="card-title">Customer → Zoho Contact Sync</div>
            <div class="card-sub">Push ERP customers (distributors) to Zoho Books as Contacts</div>
            <div style="display:flex;gap:10px;margin-bottom:16px;">
                <asp:Button ID="btnSyncAllCustomers" runat="server" CssClass="btn btn-green"
                    Text="&#x1F504; Sync All Customers" OnClick="btnSyncAllCustomers_Click" CausesValidation="false"/>
            </div>
            <asp:Repeater ID="rptCustomers" runat="server" OnItemCommand="rptCustomers_ItemCommand">
                <HeaderTemplate><table class="data"><tr>
                    <th>Code</th><th>Customer Name</th><th>GSTIN</th><th>City</th><th>Zoho Status</th><th>Zoho ID</th><th>Action</th>
                </tr></HeaderTemplate>
                <ItemTemplate><tr>
                    <td><%# Eval("CustomerCode") %></td>
                    <td style="font-weight:600;"><%# Eval("CustomerName") %></td>
                    <td style="font-size:10px;"><%# Eval("GSTIN") %></td>
                    <td><%# Eval("City") %></td>
                    <td><%# GetSyncBadge(Eval("ZohoContactID"), Eval("SyncStatus")) %></td>
                    <td style="font-size:10px;color:var(--text-muted);"><%# Eval("ZohoContactID") != DBNull.Value ? Eval("ZohoContactID").ToString() : "" %></td>
                    <td><asp:LinkButton runat="server" CommandName="SyncOne" CommandArgument='<%# Eval("CustomerID") %>'
                        CssClass="btn btn-blue btn-sm" CausesValidation="false">Sync</asp:LinkButton></td>
                </tr></ItemTemplate>
                <FooterTemplate></table></FooterTemplate>
            </asp:Repeater>
        </div>
    </div>

    <!-- ═══ SUPPLIERS TAB ═══ -->
    <div id="tab-suppliers" class="tab-panel" style="display:none;">
        <div class="card">
            <div class="card-title">Supplier → Zoho Vendor Sync</div>
            <div class="card-sub">Push ERP suppliers to Zoho Books as Vendor contacts</div>
            <div style="display:flex;gap:10px;margin-bottom:16px;">
                <asp:Button ID="btnSyncAllSuppliers" runat="server" CssClass="btn btn-orange"
                    Text="&#x1F504; Sync All Suppliers" OnClick="btnSyncAllSuppliers_Click" CausesValidation="false"/>
            </div>
            <asp:Repeater ID="rptSuppliers" runat="server" OnItemCommand="rptSuppliers_ItemCommand">
                <HeaderTemplate><table class="data"><tr>
                    <th>Code</th><th>Supplier Name</th><th>GST No</th><th>City</th><th>Zoho Status</th><th>Zoho ID</th><th>Action</th>
                </tr></HeaderTemplate>
                <ItemTemplate><tr>
                    <td><%# Eval("SupplierCode") %></td>
                    <td style="font-weight:600;"><%# Eval("SupplierName") %></td>
                    <td style="font-size:10px;"><%# Eval("GSTNo") %></td>
                    <td><%# Eval("City") %></td>
                    <td><%# GetSyncBadge(Eval("ZohoContactID"), Eval("SyncStatus")) %></td>
                    <td style="font-size:10px;color:var(--text-muted);"><%# Eval("ZohoContactID") != DBNull.Value ? Eval("ZohoContactID").ToString() : "" %></td>
                    <td><asp:LinkButton runat="server" CommandName="SyncOne" CommandArgument='<%# Eval("SupplierID") %>'
                        CssClass="btn btn-blue btn-sm" CausesValidation="false">Sync</asp:LinkButton></td>
                </tr></ItemTemplate>
                <FooterTemplate></table></FooterTemplate>
            </asp:Repeater>
        </div>
    </div>

    <!-- ═══ LOG TAB ═══ -->
    <div id="tab-log" class="tab-panel" style="display:none;">
        <div class="card">
            <div class="card-title">Sync Activity Log</div>
            <div class="card-sub">Recent API calls and sync operations</div>
            <asp:Repeater ID="rptLog" runat="server">
                <HeaderTemplate><table class="data"><tr>
                    <th>Time</th><th>Action</th><th>Type</th><th>ERP ID</th><th>Zoho ID</th><th>Status</th><th>Details</th>
                </tr></HeaderTemplate>
                <ItemTemplate><tr>
                    <td style="white-space:nowrap;font-size:11px;"><%# Convert.ToDateTime(Eval("CreatedAt")).ToString("dd-MMM HH:mm:ss") %></td>
                    <td style="font-weight:600;"><%# Eval("Action") %></td>
                    <td><%# Eval("EntityType") %></td>
                    <td><%# Eval("EntityID") %></td>
                    <td style="font-size:10px;"><%# Eval("ZohoID") %></td>
                    <td><%# Eval("Status").ToString() == "Success"
                        ? "<span class='status-badge badge-synced'>Success</span>"
                        : "<span class='status-badge badge-error'>Error</span>" %></td>
                    <td class="log-detail" title='<%# Eval("Details") %>'><%# Eval("Details") %></td>
                </tr></ItemTemplate>
                <FooterTemplate></table></FooterTemplate>
            </asp:Repeater>
        </div>
    </div>

</div>
</form>
<script>
function showTab(name) {
    var panels = document.querySelectorAll('.tab-panel');
    for (var i = 0; i < panels.length; i++) panels[i].style.display = 'none';
    document.getElementById('tab-' + name).style.display = 'block';
    var tabs = document.querySelectorAll('.section-tab');
    for (var i = 0; i < tabs.length; i++) tabs[i].className = 'section-tab';
    event.target.className = 'section-tab active';
    document.getElementById('<%= hfActiveTab.ClientID %>').value = name;
}
// Restore tab on postback
window.addEventListener('DOMContentLoaded', function(){
    var active = document.getElementById('<%= hfActiveTab.ClientID %>').value || 'connection';
    var panels = document.querySelectorAll('.tab-panel');
    for (var i = 0; i < panels.length; i++) panels[i].style.display = 'none';
    document.getElementById('tab-' + active).style.display = 'block';
    var tabs = document.querySelectorAll('.section-tab');
    for (var i = 0; i < tabs.length; i++) {
        if (tabs[i].textContent.toLowerCase().replace(/\s/g,'').indexOf(active) >= 0
            || (active === 'connection' && i === 0)
            || (active === 'products' && i === 1)
            || (active === 'customers' && i === 2)
            || (active === 'suppliers' && i === 3)
            || (active === 'log' && i === 4))
        { tabs[i].className = 'section-tab active'; } else { tabs[i].className = 'section-tab'; }
    }
});
</script>
<script src="/StockApp/erp-keepalive.js"></script>
</body>
</html>
