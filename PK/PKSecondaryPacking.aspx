<%@ Page Language="C#" AutoEventWireup="true" Inherits="PKApp.PKSecondaryPacking" EnableEventValidation="false" %>
<!DOCTYPE html><html lang="en"><head runat="server">
<meta charset="utf-8"/><meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Secondary Packing &amp; FG</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<style>
:root{--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--accent:#e67e22;--accent-dark:#d35400;--teal:#1a9e6a;--orange:#e07b00;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--radius:12px;}
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
.main{max-width:1100px;margin:24px auto;padding:0 28px;}
.card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:20px 24px;margin-bottom:20px;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:14px;letter-spacing:.08em;color:var(--text-muted);margin-bottom:14px;padding-bottom:10px;border-bottom:1px solid var(--border);}
.form-grid{display:grid;grid-template-columns:1fr 1fr;gap:14px;}
.form-grid-3{display:grid;grid-template-columns:1fr 1fr 1fr;gap:14px;}
.form-group{display:flex;flex-direction:column;gap:5px;}
label{font-size:11px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;color:var(--text-muted);}
select,input[type=number],textarea{width:100%;padding:9px 12px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:13px;background:#fafafa;outline:none;}
select:focus,input:focus,textarea:focus{border-color:var(--accent);background:#fff;}
.btn-primary{background:var(--accent);color:#fff;border:none;border-radius:8px;padding:11px 28px;font-size:13px;font-weight:700;cursor:pointer;margin-top:14px;letter-spacing:.03em;}
.btn-primary:hover{background:var(--accent-dark);}
.alert{padding:10px 14px;border-radius:8px;font-size:13px;font-weight:600;margin-bottom:14px;}
.alert-success{background:#eafaf1;color:var(--teal);border:1px solid #a9dfbf;}
.alert-danger{background:#fdf3f2;color:#e74c3c;border:1px solid #f5c6cb;}
.data-table{width:100%;border-collapse:collapse;font-size:13px;}
.data-table th{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-muted);padding:9px 12px;border-bottom:2px solid var(--border);text-align:left;background:#fafafa;}
.data-table th.num{text-align:right;}
.data-table td{padding:10px 12px;border-bottom:1px solid var(--border);}
.data-table td.num{text-align:right;font-variant-numeric:tabular-nums;}
.data-table tr:last-child td{border-bottom:none;}
.data-table tr:hover td{background:#f9f9f9;}
.empty-note{text-align:center;padding:28px;color:var(--text-dim);font-size:13px;}

/* Product info panel */
.product-info{display:none;background:#fef9f3;border:1px solid #fde3c8;border-radius:10px;padding:16px 20px;margin-bottom:16px;}
.product-info.show{display:block;}
.product-info-row{display:flex;gap:28px;align-items:center;flex-wrap:wrap;}
.pi-stat{text-align:center;}
.pi-stat-val{font-family:'Bebas Neue',sans-serif;font-size:22px;letter-spacing:.03em;}
.pi-stat-val.green{color:var(--teal);}
.pi-stat-val.orange{color:var(--accent);}
.pi-stat-lbl{font-size:9px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);}
.pi-name{font-weight:700;font-size:15px;}
.pi-code{font-size:11px;color:var(--text-dim);}

/* Calc bar */
.calc-bar{background:#f0faf5;border:1px solid #a9dfbf;border-radius:10px;padding:12px 18px;margin-top:12px;display:flex;align-items:center;justify-content:space-between;}
.calc-formula{font-size:12px;color:var(--text-muted);}
.calc-total{font-family:'Bebas Neue',sans-serif;font-size:24px;color:var(--teal);letter-spacing:.03em;}

/* Warning bar */
.warn-bar{background:#fdf3f2;border:1px solid #f5c6cb;border-radius:8px;padding:10px 14px;margin-top:10px;font-size:12px;font-weight:600;color:#c0392b;display:none;}
</style></head><body>
<form id="form1" runat="server">

<!-- Hidden fields for JS -->
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
    <div class="page-title">Secondary Packing &amp; <span>FG</span></div>
    <div class="page-sub">Pack jars/boxes into master cartons — becomes Finished Goods ready for shipping</div>
</div>
<div class="main">
    <asp:Panel ID="pnlAlert" runat="server" Visible="false">
        <div class="alert"><asp:Label ID="lblAlert" runat="server"/></div>
    </asp:Panel>

    <div class="card">
        <div class="card-title">&#x1F4E6; Record Case Packing</div>

        <!-- Step 1: Product selection -->
        <div class="form-group" style="max-width:500px;margin-bottom:14px;">
            <label>Select Product <span style="color:var(--accent)">*</span></label>
            <asp:DropDownList ID="ddlProduct" runat="server" onchange="onProductChange(this);"/>
        </div>

        <!-- Product info panel - shows after selection via JS -->
        <div id="productInfo" class="product-info">
            <div class="product-info-row">
                <div>
                    <div class="pi-name" id="piName"></div>
                    <div class="pi-code" id="piCode"></div>
                </div>
                <div class="pi-stat">
                    <div class="pi-stat-val green" id="piAvailJars">0</div>
                    <div class="pi-stat-lbl" id="piContainerLabel">Jars Available</div>
                </div>
                <div class="pi-stat">
                    <div class="pi-stat-val" id="piAvailPcs">0</div>
                    <div class="pi-stat-lbl">Pieces Available</div>
                </div>
                <div class="pi-stat">
                    <div class="pi-stat-val orange" id="piPerCase">0</div>
                    <div class="pi-stat-lbl" id="piPerCaseLabel">Jars per Case</div>
                </div>
                <div class="pi-stat">
                    <div class="pi-stat-val" id="piMaxCases">0</div>
                    <div class="pi-stat-lbl">Max Cases</div>
                </div>
            </div>
        </div>

        <!-- Step 2: Packing input -->
        <div class="form-grid-3">
            <div class="form-group">
                <label>No. of Cases <span style="color:var(--accent)">*</span></label>
                <input type="number" id="txtCartons" runat="server" step="1" min="1" placeholder="0" oninput="calcSecondary();"/>
            </div>
            <div class="form-group">
                <label>Jars per Case</label>
                <input type="number" id="txtUnitsPerCarton" runat="server" step="1" min="1" placeholder="0" oninput="calcSecondary();"/>
            </div>
            <div class="form-group">
                <label>Carton Material</label>
                <div id="casePMDisplay" style="padding:9px 12px;border:1.5px solid var(--border);border-radius:8px;background:#f5f5f5;font-size:13px;color:var(--text-muted);min-height:38px;">—</div>
                <asp:HiddenField ID="hfCasePMID" runat="server" Value="0"/>
            </div>
        </div>

        <!-- Calc bar -->
        <div class="calc-bar" id="calcBar" style="display:none;">
            <div class="calc-formula" id="calcFormula">—</div>
            <div><span class="calc-total" id="calcTotal">0</span> <span style="font-size:12px;color:var(--text-muted);">jars into cases</span></div>
        </div>

        <!-- Warning -->
        <div class="warn-bar" id="warnBar">&#9888; <span id="warnText"></span></div>

        <div class="form-group" style="margin-top:12px;">
            <label>Remarks</label>
            <textarea id="txtRemarks" runat="server" rows="2" placeholder="Optional notes"></textarea>
        </div>

        <asp:Button ID="btnPack" runat="server" Text="&#x2713; Pack Cases &amp; Add to FG Stock"
            CssClass="btn-primary" OnClick="btnPack_Click" CausesValidation="false"/>
    </div>

    <!-- Today's log -->
    <div class="card">
        <div class="card-title">&#x1F4CB; Today's Secondary Packing Log</div>
        <asp:Panel ID="pnlEmpty" runat="server"><div class="empty-note">No secondary packing recorded today</div></asp:Panel>
        <asp:Panel ID="pnlTable" runat="server" Visible="false">
        <table class="data-table">
            <thead><tr>
                <th>Time</th><th>Product</th><th class="num">Cases</th>
                <th class="num">Jars/Case</th><th class="num">Total Jars</th>
                <th>Carton PM</th><th>Remarks</th>
            </tr></thead>
            <tbody>
                <asp:Repeater ID="rptLog" runat="server">
                    <ItemTemplate><tr>
                        <td style="font-size:12px;color:var(--text-muted);"><%# Convert.ToDateTime(Eval("PackedAt")).ToString("hh:mm tt") %></td>
                        <td><strong><%# Eval("ProductName") %></strong></td>
                        <td class="num" style="font-weight:700;"><%# Eval("QtyCartons") %></td>
                        <td class="num"><%# Eval("UnitsPerCarton") %></td>
                        <td class="num" style="font-weight:700;color:var(--teal);"><%# string.Format("{0:N0}", Eval("TotalUnits")) %></td>
                        <td style="font-size:12px;"><%# Eval("PMName") == DBNull.Value ? "—" : Eval("PMName").ToString() %></td>
                        <td style="font-size:12px;color:var(--text-dim);"><%# Eval("Remarks") == DBNull.Value ? "" : Eval("Remarks") %></td>
                    </tr></ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
        </asp:Panel>
    </div>
</div>
</form>
<script>
var productData = {};
try { productData = JSON.parse(document.getElementById('<%= hfProductData.ClientID %>').value || '{}'); } catch(e){}

function onProductChange(sel){
    var val = sel.value;
    var info = document.getElementById('productInfo');
    var calcBar = document.getElementById('calcBar');
    if(!val || val === '0'){
        info.className = 'product-info';
        calcBar.style.display = 'none';
        return;
    }
    var p = productData[val];
    if(!p){ info.className='product-info'; return; }

    var jarSize = parseInt(p.unitSizes) || 1;
    var availPcs = parseInt(p.availPcs) || 0;
    var availJars = Math.floor(availPcs / jarSize);
    var perCase = parseInt(p.containersPerCase) || 12;
    var maxCases = Math.floor(availJars / perCase);
    var ct = p.containerType || 'JAR';
    var ctLabel = ct === 'DIRECT' ? 'Containers' : ct + 's';

    document.getElementById('piName').innerText = p.name;
    document.getElementById('piCode').innerText = p.code;
    document.getElementById('piAvailJars').innerText = availJars.toLocaleString();
    document.getElementById('piAvailPcs').innerText = availPcs.toLocaleString();
    document.getElementById('piContainerLabel').innerText = ctLabel + ' Available';
    document.getElementById('piPerCase').innerText = perCase;
    document.getElementById('piPerCaseLabel').innerText = ctLabel + ' per Case';
    document.getElementById('piMaxCases').innerText = maxCases;

    // Auto-fill jars per case
    document.getElementById('txtUnitsPerCarton').value = perCase;

    // Show carton PM
    var casePMDisplay = document.getElementById('casePMDisplay');
    var hfPMID = document.getElementById('<%= hfCasePMID.ClientID %>');
    if(p.casePMID && parseInt(p.casePMID) > 0){
        casePMDisplay.innerText = p.casePMName;
        casePMDisplay.style.color = 'var(--text)';
        casePMDisplay.style.fontWeight = '600';
        hfPMID.value = p.casePMID;
    } else {
        casePMDisplay.innerText = 'No carton PM mapped';
        casePMDisplay.style.color = 'var(--text-dim)';
        casePMDisplay.style.fontWeight = '400';
        hfPMID.value = '0';
    }

    info.className = 'product-info show';
    calcSecondary();
}

function calcSecondary(){
    var sel = document.getElementById('<%= ddlProduct.ClientID %>');
    var val = sel ? sel.value : '0';
    var p = productData[val];
    var calcBar = document.getElementById('calcBar');
    var warnBar = document.getElementById('warnBar');
    if(!p || val === '0'){ calcBar.style.display='none'; warnBar.style.display='none'; return; }

    var cases = parseInt(document.getElementById('txtCartons').value) || 0;
    var perCase = parseInt(document.getElementById('txtUnitsPerCarton').value) || 0;
    var totalJars = cases * perCase;

    var jarSize = parseInt(p.unitSizes) || 1;
    var availPcs = parseInt(p.availPcs) || 0;
    var availJars = Math.floor(availPcs / jarSize);
    var ct = (p.containerType || 'JAR');
    var ctLabel = ct === 'DIRECT' ? 'containers' : ct.toLowerCase() + 's';

    if(cases > 0 && perCase > 0){
        calcBar.style.display = 'flex';
        document.getElementById('calcFormula').innerText = cases + ' cases × ' + perCase + ' ' + ctLabel + '/case';
        document.getElementById('calcTotal').innerText = totalJars.toLocaleString();

        if(totalJars > availJars){
            warnBar.style.display = 'block';
            document.getElementById('warnText').innerText = 'Need ' + totalJars + ' ' + ctLabel + ' but only ' + availJars + ' available';
        } else {
            warnBar.style.display = 'none';
        }
    } else {
        calcBar.style.display = 'none';
        warnBar.style.display = 'none';
    }
}
</script></body></html>
