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
.page-title{font-family:'Bebas Neue',sans-serif;font-size:26px;letter-spacing:.07em;}.page-title span{color:var(--accent);}
.page-sub{font-size:12px;color:var(--text-muted);margin-top:2px;}
.main{max-width:1100px;margin:24px auto;padding:0 28px;}
.card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:20px 24px;margin-bottom:20px;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:14px;letter-spacing:.08em;color:var(--text-muted);margin-bottom:14px;padding-bottom:10px;border-bottom:1px solid var(--border);}
.form-grid-3{display:grid;grid-template-columns:1fr 1fr 1fr;gap:14px;}
.form-group{display:flex;flex-direction:column;gap:5px;}
label{font-size:11px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;color:var(--text-muted);}
select,input[type=number],input[type=text],textarea{width:100%;padding:9px 12px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:13px;background:#fafafa;outline:none;}
select:focus,input:focus,textarea:focus{border-color:var(--accent);background:#fff;}
.btn-primary{background:var(--accent);color:#fff;border:none;border-radius:8px;padding:11px 28px;font-size:13px;font-weight:700;cursor:pointer;margin-top:14px;}
.btn-primary:hover{background:var(--accent-dark);}
.btn-add{background:#1a1a1a;color:#fff;border:none;border-radius:8px;padding:9px 18px;font-size:12px;font-weight:700;cursor:pointer;}
.btn-add:hover{background:#333;}
.btn-remove{background:none;border:none;color:#e74c3c;font-size:11px;font-weight:700;cursor:pointer;text-decoration:underline;}
.alert{padding:10px 14px;border-radius:8px;font-size:13px;font-weight:600;margin-bottom:14px;}
.alert-success{background:#eafaf1;color:var(--teal);border:1px solid #a9dfbf;}
.alert-danger{background:#fdf3f2;color:#e74c3c;border:1px solid #f5c6cb;}
.data-table{width:100%;border-collapse:collapse;font-size:13px;}
.data-table th{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-muted);padding:9px 12px;border-bottom:2px solid var(--border);text-align:left;background:#fafafa;}
.data-table th.num{text-align:right;}.data-table td{padding:10px 12px;border-bottom:1px solid var(--border);}
.data-table td.num{text-align:right;font-variant-numeric:tabular-nums;}
.data-table tr:last-child td{border-bottom:none;}.data-table tr:hover td{background:#f9f9f9;}
.empty-note{text-align:center;padding:28px;color:var(--text-dim);font-size:13px;}
.badge-case{background:#fef9f3;color:var(--accent);font-size:10px;font-weight:700;padding:2px 8px;border-radius:10px;}
.badge-online{background:#e8f4fd;color:#2980b9;font-size:10px;font-weight:700;padding:2px 8px;border-radius:10px;}
.tabs{display:flex;gap:0;margin-bottom:0;}
.tab{padding:12px 28px;font-size:13px;font-weight:700;cursor:pointer;border:1px solid var(--border);border-bottom:none;border-radius:var(--radius) var(--radius) 0 0;background:#f5f5f5;color:var(--text-muted);position:relative;top:1px;}
.tab.active{background:var(--surface);color:var(--text);border-bottom:1px solid var(--surface);}
.tab-content{display:none;}.tab-content.active{display:block;}
.product-info{display:none;background:#fef9f3;border:1px solid #fde3c8;border-radius:10px;padding:16px 20px;margin-bottom:16px;}
.product-info.show{display:block;}
.product-info-row{display:flex;gap:28px;align-items:center;flex-wrap:wrap;}
.pi-stat{text-align:center;}.pi-stat-val{font-family:'Bebas Neue',sans-serif;font-size:22px;}.pi-stat-val.green{color:var(--teal);}.pi-stat-val.orange{color:var(--accent);}
.pi-stat-lbl{font-size:9px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);}
.pi-name{font-weight:700;font-size:15px;}.pi-code{font-size:11px;color:var(--text-dim);}
.calc-bar{background:#f0faf5;border:1px solid #a9dfbf;border-radius:10px;padding:12px 18px;margin-top:12px;display:flex;align-items:center;justify-content:space-between;}
.calc-formula{font-size:12px;color:var(--text-muted);}.calc-total{font-family:'Bebas Neue',sans-serif;font-size:24px;color:var(--teal);}
.warn-bar{background:#fdf3f2;border:1px solid #f5c6cb;border-radius:8px;padding:10px 14px;margin-top:10px;font-size:12px;font-weight:600;color:#c0392b;display:none;}
.ol-table{width:100%;border-collapse:collapse;font-size:13px;margin-top:12px;}
.ol-table th{font-size:10px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;color:var(--text-dim);padding:8px 10px;border-bottom:1px solid var(--border);text-align:left;}
.ol-table td{padding:8px 10px;border-bottom:1px solid #f0f0f0;}.ol-table th.num,.ol-table td.num{text-align:right;}
</style></head><body>
<form id="form1" runat="server">
<asp:HiddenField ID="hfProductData" runat="server" Value="{}"/>
<asp:HiddenField ID="hfOnlineLines" runat="server" Value=""/>
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
    <div class="page-sub">Pack jars/boxes into master cartons — SFG (jars/boxes) into cases — becomes FG (Finished Goods) ready for dispatch</div>
</div>
<div class="main">
    <asp:Panel ID="pnlAlert" runat="server" Visible="false"><div class="alert"><asp:Label ID="lblAlert" runat="server"/></div></asp:Panel>

    <div class="tabs">
        <div class="tab active" onclick="switchTab('case')">&#x1F4E6; Case Packing</div>
        <div class="tab" onclick="switchTab('online')">&#x1F6D2; Online Orders</div>
    </div>

    <!-- ══════ TAB 1: CASE PACKING ══════ -->
    <div id="tabCase" class="tab-content active">
    <div class="card" style="border-top-left-radius:0;">
        <div class="card-title">Pack Jars into Master Cartons</div>
        <div class="form-group" style="max-width:500px;margin-bottom:14px;">
            <label>Select Product <span style="color:var(--accent)">*</span></label>
            <asp:DropDownList ID="ddlProduct" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlProduct_Changed" onchange="onProductChange(this);"/>
        </div>
        <div id="productInfo" class="product-info">
            <div class="product-info-row">
                <div><div class="pi-name" id="piName"></div><div class="pi-code" id="piCode"></div></div>
                <div class="pi-stat"><div class="pi-stat-val green" id="piAvailJars">0</div><div class="pi-stat-lbl" id="piContainerLabel">Jars Available</div></div>
                <div class="pi-stat"><div class="pi-stat-val" id="piAvailPcs">0</div><div class="pi-stat-lbl">Pieces Available</div></div>
                <div class="pi-stat"><div class="pi-stat-val orange" id="piPerCase">0</div><div class="pi-stat-lbl" id="piPerCaseLabel">Jars per Case</div></div>
                <div class="pi-stat"><div class="pi-stat-val" id="piMaxCases">0</div><div class="pi-stat-lbl">Max Cases</div></div>
            </div>
        </div>
        <div class="form-grid-3">
            <div class="form-group"><label>No. of Cases <span style="color:var(--accent)">*</span></label>
                <input type="number" id="txtCartons" runat="server" step="1" min="1" placeholder="0" oninput="calcSecondary(); calcCasePMs();"/>
                <div id="maxCasesHint" style="font-size:11px;font-weight:700;color:var(--teal);margin-top:2px;display:none;">Max: <span id="maxCasesVal">0</span> cases possible</div></div>
            <div class="form-group"><label>Jars per Case</label>
                <input type="number" id="txtUnitsPerCarton" runat="server" step="1" min="1" placeholder="0" oninput="calcSecondary();"/></div>
        </div>
        <div class="calc-bar" id="calcBar" style="display:none;">
            <div class="calc-formula" id="calcFormula">—</div>
            <div><span class="calc-total" id="calcTotal">0</span> <span style="font-size:12px;color:var(--text-muted);">jars into cases</span></div>
        </div>
        <div class="warn-bar" id="warnBar">&#9888; <span id="warnText"></span></div>
        <!-- PM CONSUMPTION GRID FOR CASE PACKING -->
        <asp:Panel ID="pnlCasePM" runat="server" Visible="false">
        <div style="margin-top:16px;border-top:1px solid var(--border);padding-top:14px;">
            <div style="font-family:'Bebas Neue',sans-serif;font-size:13px;letter-spacing:.07em;color:var(--accent-dark);margin-bottom:6px;">Packing Material Consumption</div>
            <div style="font-size:11px;color:var(--text-muted);margin-bottom:10px;">Auto-calculated from cases. Edit actual qty if needed.</div>
            <div id="pmShortageBar" style="display:none;background:#fdf3f2;border:1px solid #f5c6cb;border-radius:8px;padding:8px 12px;margin-bottom:10px;font-size:12px;font-weight:600;color:#c0392b;">&#9888; <span id="pmShortageText"></span></div>
            <table class="data-table" style="font-size:13px;">
                <thead><tr><th>Packing Material</th><th style="text-align:right;">Per Case</th><th style="text-align:right;">Calculated</th><th style="text-align:right;">Available</th><th style="text-align:right;width:110px;">Actual Qty</th><th>Unit</th></tr></thead>
                <tbody id="casePMBody">
                    <asp:Repeater ID="rptCasePM" runat="server">
                        <ItemTemplate>
                            <tr class="case-pm-row" data-pmid="<%# Eval("PMID") %>" data-qtyper="<%# Eval("QtyPerUnit") %>" data-stock="<%# Eval("CurrentStock") %>">
                                <td><strong><%# Eval("PMName") %></strong><div style="font-size:10px;color:var(--text-dim);"><%# Eval("PMCode") %></div></td>
                                <td class="num" style="color:var(--text-muted);"><%# Eval("QtyPerUnit") %></td>
                                <td class="num case-pm-calc" style="font-weight:600;">0</td>
                                <td class="num case-pm-avail" style="font-weight:600;"><%# string.Format("{0:0.##}", Eval("CurrentStock")) %></td>
                                <td style="text-align:right;">
                                    <input type="number" name="casePmQty_<%# Eval("PMID") %>" class="case-pm-actual" value="0"
                                        min="0" step="0.01" data-edited="0"
                                        style="width:100%;padding:6px 8px;border:1.5px solid var(--border);border-radius:6px;font-size:13px;text-align:right;font-weight:600;"
                                        oninput="this.setAttribute('data-edited','1');checkCasePMStock();"/>
                                </td>
                                <td style="font-size:12px;color:var(--text-muted);"><%# Eval("Abbreviation") %></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </div>
        </asp:Panel>
        <div class="form-group" style="margin-top:12px;"><label>Remarks</label>
            <textarea id="txtRemarks" runat="server" rows="2" placeholder="Optional notes"></textarea></div>
        <asp:Button ID="btnPack" runat="server" Text="&#x2713; Pack Cases &amp; Move to FG" CssClass="btn-primary" OnClick="btnPack_Click" CausesValidation="false"/>
    </div>
    </div>

    <!-- ══════ TAB 2: ONLINE ORDERS ══════ -->
    <div id="tabOnline" class="tab-content">
    <div class="card" style="border-top-left-radius:0;">
        <div class="card-title">&#x1F6D2; Pack Online Order</div>
        <div class="form-grid-3">
            <div class="form-group"><label>Order ID <span style="color:var(--accent)">*</span></label>
                <input type="text" id="txtOnlineOrderId" runat="server" placeholder="e.g. AMZ-12345"/></div>
            <div class="form-group"><label>Customer Name <span style="color:var(--accent)">*</span></label>
                <input type="text" id="txtCustomerName" runat="server" placeholder="Customer name"/></div>
            <div class="form-group"><label>Shipping Carton</label>
                <asp:DropDownList ID="ddlOnlineCarton" runat="server"/></div>
        </div>
        <div style="margin-top:18px;padding-top:14px;border-top:1px solid var(--border);">
            <div style="font-size:12px;font-weight:700;color:var(--text-muted);margin-bottom:10px;">ADD PRODUCTS TO THIS ORDER</div>
            <div class="form-grid-3" style="align-items:flex-end;">
                <div class="form-group"><label>Product</label><select id="selOLProduct" onchange="onOLProductChange();"><option value="0">-- Select --</option></select></div>
                <div class="form-group"><label>Qty (Jars/Boxes)</label><input type="number" id="txtOLQty" min="1" step="1" placeholder="0"/></div>
                <div><button type="button" class="btn-add" onclick="addOLLine();">+ Add Product</button></div>
            </div>
            <div id="olAvailInfo" style="font-size:11px;color:var(--text-dim);margin-top:4px;"></div>
        </div>
        <table class="ol-table" id="olTable" style="display:none;">
            <thead><tr><th>Product</th><th class="num">Qty</th><th class="num">Pcs</th><th></th></tr></thead>
            <tbody id="olBody"></tbody>
            <tfoot><tr style="font-weight:700;border-top:2px solid var(--border);"><td>Total</td><td class="num" id="olTotalQty">0</td><td class="num" id="olTotalPcs">0</td><td></td></tr></tfoot>
        </table>
        <div class="form-group" style="margin-top:12px;"><label>Remarks</label>
            <textarea id="txtOnlineRemarks" runat="server" rows="2" placeholder="Optional notes"></textarea></div>
        <asp:Button ID="btnPackOnline" runat="server" Text="&#x2713; Pack Online Order" CssClass="btn-primary" OnClick="btnPackOnline_Click" CausesValidation="false"/>
    </div>
    </div>

    <!-- ══════ TODAY'S LOG ══════ -->
    <div class="card">
        <div class="card-title">&#x1F4CB; Today's Packing Log</div>
        <asp:Panel ID="pnlEmpty" runat="server"><div class="empty-note">No packing recorded today</div></asp:Panel>
        <asp:Panel ID="pnlTable" runat="server" Visible="false">
        <table class="data-table">
            <thead><tr><th>Time</th><th>Type</th><th>Product</th><th>Order / Customer</th><th class="num">Cases/Qty</th><th class="num">Per Case</th><th class="num">Total</th><th>Carton</th></tr></thead>
            <tbody>
                <asp:Repeater ID="rptLog" runat="server">
                    <ItemTemplate><tr>
                        <td style="font-size:12px;color:var(--text-muted);"><%# Convert.ToDateTime(Eval("PackedAt")).ToString("hh:mm tt") %></td>
                        <td><%# Eval("PackingType").ToString()=="ONLINE" ? "<span class='badge-online'>Online</span>" : "<span class='badge-case'>Case</span>" %></td>
                        <td><strong><%# Eval("ProductName") %></strong></td>
                        <td style="font-size:12px;"><%# Eval("OnlineOrderID")==DBNull.Value ? "" : Eval("OnlineOrderID").ToString() %><%# Eval("CustomerName")==DBNull.Value ? "" : "<br/><span style='color:var(--text-dim);'>"+Eval("CustomerName")+"</span>" %></td>
                        <td class="num" style="font-weight:700;"><%# string.Format("{0:N0}", Eval("QtyCartons")) %></td>
                        <td class="num"><%# Eval("UnitsPerCarton") %></td>
                        <td class="num" style="font-weight:700;color:var(--teal);"><%# string.Format("{0:N0}", Eval("TotalUnits")) %></td>
                        <td style="font-size:12px;"><%# Eval("PMName")==DBNull.Value ? "—" : Eval("PMName").ToString() %></td>
                    </tr></ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
        </asp:Panel>
    </div>
</div>
</form>
<script>
var productData={};
try{productData=JSON.parse(document.getElementById('<%= hfProductData.ClientID %>').value||'{}');}catch(e){}
var olLines=[];
function switchTab(t){document.querySelectorAll('.tab').forEach(function(el,i){el.className='tab'+(i===(t==='case'?0:1)?' active':'');});document.getElementById('tabCase').className='tab-content'+(t==='case'?' active':'');document.getElementById('tabOnline').className='tab-content'+(t==='online'?' active':'');}
function onProductChange(sel){var v=sel.value;var info=document.getElementById('productInfo');var btn=document.getElementById('<%= btnPack.ClientID %>');var mch=document.getElementById('maxCasesHint');if(!v||v==='0'){info.className='product-info';document.getElementById('calcBar').style.display='none';if(btn)btn.disabled=false;if(mch)mch.style.display='none';return;}var p=productData[v];if(!p){info.className='product-info';return;}var js=parseInt(p.unitSizes)||1,ap=parseInt(p.availPcs)||0,aj=Math.floor(ap/js),pc=parseInt(p.containersPerCase)||12,mx=Math.floor(aj/pc),ct=p.containerType||'JAR',cl=ct==='DIRECT'?'Containers':ct+'s';document.getElementById('piName').innerText=p.name;document.getElementById('piCode').innerText=p.code;document.getElementById('piAvailJars').innerText=aj.toLocaleString();document.getElementById('piAvailPcs').innerText=ap.toLocaleString();document.getElementById('piContainerLabel').innerText=cl+' Available';document.getElementById('piPerCase').innerText=pc;document.getElementById('piPerCaseLabel').innerText=cl+' per Case';document.getElementById('piMaxCases').innerText=mx;document.getElementById('txtUnitsPerCarton').value=pc;document.getElementById('txtCartons').setAttribute('max',mx);if(mch){document.getElementById('maxCasesVal').innerText=mx;mch.style.display=mx>0?'block':'none';}info.className='product-info show';calcSecondary();}
function calcSecondary(){var sel=document.getElementById('<%= ddlProduct.ClientID %>');var v=sel?sel.value:'0';var p=productData[v];var cb=document.getElementById('calcBar');var wb=document.getElementById('warnBar');var btn=document.getElementById('<%= btnPack.ClientID %>');if(!p||v==='0'){cb.style.display='none';wb.style.display='none';if(btn){btn.disabled=false;btn.style.opacity='1';}return;}var cs=parseInt(document.getElementById('txtCartons').value)||0,pc=parseInt(document.getElementById('txtUnitsPerCarton').value)||0,tj=cs*pc,js=parseInt(p.unitSizes)||1,aj=Math.floor((parseInt(p.availPcs)||0)/js),ct=(p.containerType||'JAR'),cl=ct==='DIRECT'?'containers':ct.toLowerCase()+'s';if(cs>0&&pc>0){cb.style.display='flex';document.getElementById('calcFormula').innerText=cs+' cases x '+pc+' '+cl+'/case';document.getElementById('calcTotal').innerText=tj.toLocaleString();if(tj>aj){wb.style.display='block';document.getElementById('warnText').innerText='Cannot proceed — need '+tj+' '+cl+' but only '+aj+' available (max '+Math.floor(aj/pc)+' cases)';if(btn){btn.disabled=true;btn.style.opacity='0.4';}}else{wb.style.display='none';checkCasePMStock();}}else{cb.style.display='none';wb.style.display='none';if(btn){btn.disabled=false;btn.style.opacity='1';}}}
function calcCasePMs(){var cs=parseInt(document.getElementById('txtCartons').value)||0;var rows=document.querySelectorAll('.case-pm-row');rows.forEach(function(r){var qtyPer=parseFloat(r.getAttribute('data-qtyper'))||0;var calc=cs*qtyPer;r.querySelector('.case-pm-calc').innerText=calc>0?calc.toFixed(calc%1===0?0:2):'0';var inp=r.querySelector('.case-pm-actual');if(inp.getAttribute('data-edited')!=='1'){inp.value=calc>0?calc.toFixed(calc%1===0?0:2):'0';}});checkCasePMStock();}
function checkCasePMStock(){var rows=document.querySelectorAll('.case-pm-row');var shortages=[];var btn=document.getElementById('<%= btnPack.ClientID %>');rows.forEach(function(r){var stock=parseFloat(r.getAttribute('data-stock'))||0;var inp=r.querySelector('.case-pm-actual');var needed=parseFloat(inp.value)||0;var availCell=r.querySelector('.case-pm-avail');if(needed>stock&&needed>0){shortages.push(r.querySelector('strong').innerText+' (need '+needed+', have '+stock+')');inp.style.borderColor='#e74c3c';inp.style.background='#fdf3f2';if(availCell)availCell.style.color='#e74c3c';}else{inp.style.borderColor='';inp.style.background='';if(availCell)availCell.style.color='';}});var bar=document.getElementById('pmShortageBar');if(shortages.length>0){bar.style.display='block';document.getElementById('pmShortageText').innerText='Insufficient PM stock: '+shortages.join('; ');if(btn){btn.disabled=true;btn.style.opacity='0.4';}}else{bar.style.display='none';var wb=document.getElementById('warnBar');if(wb.style.display==='none'&&btn){btn.disabled=false;btn.style.opacity='1';}}}
window.addEventListener('load',function(){var sel=document.getElementById('selOLProduct');for(var pid in productData){var p=productData[pid];var js=parseInt(p.unitSizes)||1,aj=Math.floor((parseInt(p.availPcs)||0)/js),ct=p.containerType||'JAR',cl=ct==='DIRECT'?'containers':ct.toLowerCase()+'s';var opt=document.createElement('option');opt.value=pid;opt.text=p.name+' ('+aj+' '+cl+')';opt.setAttribute('data-jarsize',js);opt.setAttribute('data-avail',aj);opt.setAttribute('data-ct',ct);sel.appendChild(opt);}});
function onOLProductChange(){var sel=document.getElementById('selOLProduct');var opt=sel.options[sel.selectedIndex];var info=document.getElementById('olAvailInfo');if(sel.value==='0'){info.innerText='';return;}var av=parseInt(opt.getAttribute('data-avail'))||0,ct=opt.getAttribute('data-ct')||'JAR',cl=ct==='DIRECT'?'containers':ct.toLowerCase()+'s';info.innerText=av+' '+cl+' available';}
function addOLLine(){var sel=document.getElementById('selOLProduct');var qi=document.getElementById('txtOLQty');var pid=sel.value;if(pid==='0'){alert('Select a product');return;}var qty=parseInt(qi.value)||0;if(qty<=0){alert('Enter a quantity');return;}var opt=sel.options[sel.selectedIndex];var nm=productData[pid].name;var js=parseInt(opt.getAttribute('data-jarsize'))||1;var av=parseInt(opt.getAttribute('data-avail'))||0;for(var i=0;i<olLines.length;i++){if(olLines[i].pid===pid){alert('Product already added. Remove first.');return;}}if(qty>av){alert('Only '+av+' available');return;}olLines.push({pid:pid,name:nm,qty:qty,jarSize:js,pcs:qty*js});renderOLTable();qi.value='';sel.selectedIndex=0;document.getElementById('olAvailInfo').innerText='';}
function removeOLLine(idx){olLines.splice(idx,1);renderOLTable();}
function renderOLTable(){var body=document.getElementById('olBody');var tbl=document.getElementById('olTable');body.innerHTML='';var tq=0,tp=0;if(olLines.length===0){tbl.style.display='none';syncOL();return;}tbl.style.display='table';olLines.forEach(function(l,i){var tr=document.createElement('tr');tr.innerHTML='<td><strong>'+l.name+'</strong></td><td class="num">'+l.qty+'</td><td class="num">'+l.pcs.toLocaleString()+'</td><td><button type="button" class="btn-remove" onclick="removeOLLine('+i+')">Remove</button></td>';body.appendChild(tr);tq+=l.qty;tp+=l.pcs;});document.getElementById('olTotalQty').innerText=tq;document.getElementById('olTotalPcs').innerText=tp.toLocaleString();syncOL();}
function syncOL(){var parts=[];olLines.forEach(function(l){parts.push(l.pid+':'+l.qty+':'+l.jarSize);});document.getElementById('<%= hfOnlineLines.ClientID %>').value=parts.join(',');}
</script></body></html>
