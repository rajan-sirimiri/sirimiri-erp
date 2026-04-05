<%@ Page Language="C#" AutoEventWireup="true" Inherits="PPApp.PPProductCatalog" EnableEventValidation="false" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri PP — Product Catalog</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600;700&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{
    --accent:#2ecc71; --accent-dark:#27ae60; --accent-light:#eafaf1;
    --orange:#e67e22; --blue:#2980b9; --red:#e74c3c;
    --text:#1a1a1a; --text-muted:#666; --text-dim:#999;
    --bg:#f0f0f0; --surface:#fff; --border:#e0e0e0;
    --radius:14px; --nav-h:52px;
}
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

.page-layout{display:grid;grid-template-columns:380px 1fr;height:calc(100vh - var(--nav-h));overflow:hidden;}

/* LEFT PANEL - Product List */
.list-panel{background:var(--surface);border-right:1px solid var(--border);display:flex;flex-direction:column;overflow:hidden;}
.list-header{padding:16px 18px 12px;border-bottom:1px solid var(--border);}
.list-title{font-family:'Bebas Neue',sans-serif;font-size:20px;letter-spacing:.06em;}
.list-count{font-size:11px;color:var(--text-dim);margin-left:8px;}
.list-search{padding:8px 18px;}
.list-search input{width:100%;padding:9px 13px;border:1.5px solid var(--border);border-radius:9px;font-family:inherit;font-size:12px;outline:none;background:#fafafa;}
.list-search input:focus{border-color:var(--accent);background:#fff;}
.list-body{flex:1;overflow-y:auto;padding:4px 10px 20px;}

.prod-item{padding:12px 14px;border-radius:10px;cursor:pointer;margin-bottom:2px;transition:all .15s;display:flex;align-items:center;gap:12px;border:1.5px solid transparent;}
.prod-item:hover{background:#f5f5f5;border-color:var(--border);}
.prod-item.active{background:var(--accent-light);border-color:var(--accent);}
.prod-icon{width:36px;height:36px;border-radius:8px;display:flex;align-items:center;justify-content:center;font-size:16px;flex-shrink:0;}
.prod-icon.core{background:#e8f5e9;} .prod-icon.conversion{background:#e3f2fd;} .prod-icon.prefilled{background:#fff3e0;} .prod-icon.preprocess{background:#fce4ec;}
.prod-info{flex:1;min-width:0;}
.prod-name{font-size:13px;font-weight:600;word-wrap:break-word;}
.prod-meta{font-size:10px;color:var(--text-dim);margin-top:2px;}
.prod-type{font-size:9px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;padding:2px 7px;border-radius:4px;white-space:nowrap;flex-shrink:0;}
.type-core{background:#e8f5e9;color:#2e7d32;} .type-conversion{background:#e3f2fd;color:#1565c0;}
.type-prefilled{background:#fff3e0;color:#e65100;} .type-preprocess{background:#fce4ec;color:#c62828;}

/* RIGHT PANEL - Product Details */
.detail-panel{overflow-y:auto;padding:24px 30px 40px;background:var(--bg);}
.detail-empty{display:flex;align-items:center;justify-content:center;height:100%;color:var(--text-dim);font-size:14px;}
.detail-header{margin-bottom:24px;}
.detail-product-name{font-family:'Bebas Neue',sans-serif;font-size:32px;letter-spacing:.04em;line-height:1.1;}
.detail-product-code{font-size:12px;color:var(--text-dim);margin-top:4px;}

.detail-card{background:var(--surface);border-radius:var(--radius);box-shadow:0 2px 12px rgba(0,0,0,.06);padding:20px 22px;margin-bottom:16px;}
.detail-card-title{font-family:'Bebas Neue',sans-serif;font-size:14px;letter-spacing:.08em;color:var(--text-muted);margin-bottom:14px;display:flex;align-items:center;gap:8px;}
.detail-grid{display:grid;grid-template-columns:1fr 1fr;gap:12px 24px;}
.detail-field{display:flex;flex-direction:column;gap:3px;}
.detail-label{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);}
.detail-value{font-size:14px;font-weight:500;}
.detail-value.highlight{color:var(--accent-dark);font-weight:700;}

.bom-table{width:100%;border-collapse:collapse;font-size:12px;}
.bom-table th{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);padding:8px 12px;border-bottom:2px solid var(--border);text-align:left;}
.bom-table th.num{text-align:right;}
.bom-table td{padding:9px 12px;border-bottom:1px solid #f0f0f0;vertical-align:middle;}
.bom-table td.num{text-align:right;font-variant-numeric:tabular-nums;}
.bom-table tr:last-child td{border-bottom:none;}
.bom-type-badge{font-size:9px;font-weight:700;letter-spacing:.06em;text-transform:uppercase;padding:2px 6px;border-radius:3px;}
.bom-rm{background:#e8f5e9;color:#2e7d32;} .bom-pm{background:#e3f2fd;color:#1565c0;} .bom-cn{background:#fff3e0;color:#e65100;}

.stage-flow{display:flex;align-items:center;gap:6px;flex-wrap:wrap;}
.stage-pill{background:#f5f5f5;border:1px solid var(--border);border-radius:8px;padding:8px 14px;font-size:12px;font-weight:600;text-align:center;min-width:80px;}
.stage-arrow{color:var(--text-dim);font-size:16px;}

.param-list{list-style:none;padding:0;}
.param-list li{padding:6px 0;border-bottom:1px solid #f5f5f5;font-size:12px;display:flex;justify-content:space-between;}
.param-list li:last-child{border-bottom:none;}
.param-label{font-weight:600;} .param-type{color:var(--text-dim);font-size:11px;}

.tag{display:inline-block;font-size:10px;font-weight:700;letter-spacing:.05em;text-transform:uppercase;padding:3px 8px;border-radius:4px;}
.tag-yes{background:#e8f5e9;color:#2e7d32;} .tag-no{background:#f5f5f5;color:#999;}

@media(max-width:768px){
    .page-layout{grid-template-columns:1fr;height:auto;}
    .list-panel{max-height:40vh;} .detail-panel{padding:16px;}
}
</style>
</head>
<body>
<form id="form1" runat="server">

<asp:HiddenField ID="hfSelectedProductId" runat="server" Value="0"/>
<asp:Button ID="btnSelect" runat="server" OnClick="btnSelect_Click" style="display:none" CausesValidation="false"/>

<nav>
    <a class="nav-logo" href="PPHome.aspx">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    </a>
    <span class="nav-title">PP — Product Catalog</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblNavUser" runat="server"/></span>
        <a href="PPHome.aspx" class="nav-link">&#8592; Home</a>
        <a href="PPLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>

<div class="page-layout">

    <!-- LEFT: Product List -->
    <div class="list-panel">
        <div class="list-header">
            <span class="list-title">Products</span>
            <span class="list-count"><asp:Label ID="lblCount" runat="server"/></span>
        </div>
        <div class="list-search">
            <input type="text" id="txtSearch" placeholder="Search products..." onkeyup="filterProducts(this.value)"/>
        </div>
        <div class="list-body" id="productList">
            <asp:Repeater ID="rptProducts" runat="server">
                <ItemTemplate>
                    <div class='prod-item <%# Convert.ToInt32(Eval("ProductID")).ToString() == hfSelectedProductId.Value ? "active" : "" %>'
                         data-name='<%# Eval("ProductName") %>' data-type='<%# Eval("ProductType") %>'
                         onclick='selectProduct(<%# Eval("ProductID") %>)'>
                        <div class='prod-icon <%# GetTypeClass(Eval("ProductType")) %>'><%# GetTypeIcon(Eval("ProductType")) %></div>
                        <div class="prod-info">
                            <div class="prod-name"><%# Eval("ProductName") %></div>
                            <div class="prod-meta"><%# Eval("ProductCode") %> &bull; Batch: <%# Eval("BatchSize") %> <%# Eval("ProdAbbreviation") %></div>
                        </div>
                        <span class='prod-type <%# "type-" + Eval("ProductType").ToString().ToLower().Replace(" ","") %>'><%# Eval("ProductType") %></span>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </div>

    <!-- RIGHT: Product Details -->
    <div class="detail-panel">
        <asp:Panel ID="pnlEmpty" runat="server">
            <div class="detail-empty">&#x1F4CB; Select a product to view details</div>
        </asp:Panel>

        <asp:Panel ID="pnlDetail" runat="server" Visible="false">

            <!-- HEADER -->
            <div class="detail-header">
                <div class="detail-product-name"><asp:Label ID="lblProductName" runat="server"/></div>
                <div class="detail-product-code"><asp:Label ID="lblProductCode" runat="server"/> &bull; <asp:Label ID="lblProductType" runat="server"/></div>
            </div>

            <!-- GENERAL INFO -->
            <div class="detail-card">
                <div class="detail-card-title">&#x2699; General Information</div>
                <div class="detail-grid">
                    <div class="detail-field">
                        <span class="detail-label">Batch Size</span>
                        <span class="detail-value highlight"><asp:Label ID="lblBatchSize" runat="server"/></span>
                    </div>
                    <div class="detail-field">
                        <span class="detail-label">Production UOM</span>
                        <span class="detail-value"><asp:Label ID="lblProdUOM" runat="server"/></span>
                    </div>
                    <div class="detail-field">
                        <span class="detail-label">Output UOM</span>
                        <span class="detail-value"><asp:Label ID="lblOutputUOM" runat="server"/></span>
                    </div>
                    <div class="detail-field">
                        <span class="detail-label">Unit Weight</span>
                        <span class="detail-value"><asp:Label ID="lblUnitWeight" runat="server"/></span>
                    </div>
                    <div class="detail-field">
                        <span class="detail-label">Production Line</span>
                        <span class="detail-value"><asp:Label ID="lblProductionLine" runat="server"/></span>
                    </div>
                    <div class="detail-field">
                        <span class="detail-label">HSN Code</span>
                        <span class="detail-value"><asp:Label ID="lblHSN" runat="server"/></span>
                    </div>
                    <div class="detail-field">
                        <span class="detail-label">GST Rate</span>
                        <span class="detail-value"><asp:Label ID="lblGST" runat="server"/></span>
                    </div>
                    <div class="detail-field">
                        <span class="detail-label">Price Calculation</span>
                        <span class="detail-value"><asp:Label ID="lblPriceCalc" runat="server"/></span>
                    </div>
                </div>
            </div>

            <!-- BOM / RECIPE -->
            <asp:Panel ID="pnlBOM" runat="server" Visible="false">
            <div class="detail-card">
                <div class="detail-card-title">&#x1F4DC; Recipe / Bill of Materials</div>
                <asp:Panel ID="pnlBOMEmpty" runat="server"><div style="color:var(--text-dim);font-size:12px;">No BOM defined</div></asp:Panel>
                <asp:Panel ID="pnlBOMTable" runat="server" Visible="false">
                <table class="bom-table">
                    <thead><tr>
                        <th>Type</th><th>Code</th><th>Material</th><th>UOM</th>
                        <th class="num">Qty / Batch</th><th class="num">Unit Rate (₹)</th>
                        <th class="num">Cost / Batch (₹)</th>
                    </tr></thead>
                    <tbody>
                        <asp:Repeater ID="rptBOM" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td><span class='bom-type-badge <%# "bom-" + Eval("MaterialType").ToString().ToLower() %>'><%# Eval("MaterialType") %></span></td>
                                    <td style="font-size:11px;color:var(--text-muted);"><%# Eval("MaterialCode") %></td>
                                    <td style="font-weight:600;"><%# Eval("MaterialName") %></td>
                                    <td><%# Eval("Abbreviation") %></td>
                                    <td class="num"><%# Convert.ToDecimal(Eval("Quantity")).ToString("0.###") %></td>
                                    <td class="num" style="color:var(--text-muted);"><%# FormatRate(Eval("UnitRate")) %></td>
                                    <td class="num" style="font-weight:600;"><%# FormatBOMCost(Eval("Quantity"), Eval("UnitRate")) %></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
                <div style="text-align:right;margin-top:10px;font-size:13px;">
                    Total BOM Cost / Batch: <strong style="color:var(--accent-dark);font-size:16px;">₹<asp:Label ID="lblBOMTotal" runat="server"/></strong>
                </div>
                </asp:Panel>
            </div>
            </asp:Panel>

            <!-- PREPROCESS STAGES -->
            <asp:Panel ID="pnlStages" runat="server" Visible="false">
            <div class="detail-card">
                <div class="detail-card-title">&#x1F504; Pre-Processing Stages</div>
                <div class="detail-field" style="margin-bottom:14px;">
                    <span class="detail-label">Input Raw Material</span>
                    <span class="detail-value highlight"><asp:Label ID="lblInputRM" runat="server"/></span>
                </div>
                <div class="stage-flow">
                    <asp:Label ID="lblStageFlow" runat="server"/>
                </div>
            </div>
            </asp:Panel>

            <!-- BATCH PARAMETERS -->
            <asp:Panel ID="pnlParams" runat="server" Visible="false">
            <div class="detail-card">
                <div class="detail-card-title">&#x1F4CB; Batch Parameters</div>
                <ul class="param-list">
                    <asp:Repeater ID="rptParams" runat="server">
                        <ItemTemplate>
                            <li>
                                <span class="param-label"><%# Eval("ParamLabel") %></span>
                                <span class="param-type"><%# Eval("ParamType") %><%# Eval("ParamOptions") != DBNull.Value && Eval("ParamOptions").ToString() != "" ? " — " + Eval("ParamOptions").ToString() : "" %></span>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>
            </div>
            </asp:Panel>

        </asp:Panel>
    </div>
</div>

</form>
<script>
function selectProduct(id) {
    document.getElementById('<%= hfSelectedProductId.ClientID %>').value = id;
    document.getElementById('<%= btnSelect.ClientID %>').click();
}
function filterProducts(q) {
    q = q.toLowerCase();
    var items = document.querySelectorAll('.prod-item');
    items.forEach(function(el) {
        var name = (el.getAttribute('data-name') || '').toLowerCase();
        var type = (el.getAttribute('data-type') || '').toLowerCase();
        el.style.display = (name.indexOf(q) >= 0 || type.indexOf(q) >= 0) ? '' : 'none';
    });
}
</script>
<script src="/StockApp/erp-keepalive.js"></script>
</body>
</html>
