<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FINSalesAnalytics.aspx.cs" Inherits="FINApp.FINSalesAnalytics" EnableEventValidation="false" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri — Sales Analytics</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600;700&family=JetBrains+Mono:wght@400;700&display=swap" rel="stylesheet"/>
<script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/4.4.1/chart.umd.min.js"></script>
<style>
:root{--ink:#0f0f0f;--paper:#f7f5f2;--smoke:#e8e5e0;--dim:#9a9590;--ruby:#cc1e1e;--emerald:#1a9e6a;--sapphire:#1e5fcc;--amber:#d68b00;--plum:#7c3aed;--surface:#fff;--mono:'JetBrains Mono',monospace;--sans:'DM Sans',sans-serif;--display:'Bebas Neue',sans-serif;--radius:10px;}
*,*::before,*::after{box-sizing:border-box;margin:0;padding:0;}
body{font-family:var(--sans);background:var(--paper);color:var(--ink);-webkit-font-smoothing:antialiased;}
.top-bar{background:var(--ink);padding:12px 24px;display:flex;align-items:center;gap:16px;}
.top-bar img{height:28px;background:#fff;border-radius:4px;padding:2px 6px;} .top-bar h1{color:#fff;font-family:var(--display);font-size:18px;letter-spacing:.1em;flex:1;}
.top-bar a{color:rgba(255,255,255,.7);font-size:12px;font-weight:600;text-decoration:none;} .top-bar a:hover{color:#fff;}
.page{max-width:1280px;margin:0 auto;padding:28px 24px 80px;}
.main-tabs{display:flex;gap:0;margin-bottom:24px;border-radius:10px;overflow:hidden;border:2px solid var(--ink);}
.main-tab{flex:1;padding:13px 10px;text-align:center;font-family:var(--display);font-size:14px;letter-spacing:.08em;cursor:pointer;background:#fff;color:var(--ink);border:none;transition:all .15s;}
.main-tab.active{background:var(--ink);color:#fff;} .main-tab:hover:not(.active){background:#f0ede8;}
.kpi-strip{display:flex;gap:12px;margin-bottom:28px;flex-wrap:wrap;}
.kpi{flex:1;min-width:140px;background:var(--surface);border-radius:var(--radius);padding:16px 18px;border-left:4px solid var(--smoke);}
.kpi.accent{border-left-color:var(--ruby);} .kpi.green{border-left-color:var(--emerald);} .kpi.blue{border-left-color:var(--sapphire);} .kpi.amber{border-left-color:var(--amber);}
.kpi-val{font-family:var(--display);font-size:28px;line-height:1;letter-spacing:.03em;}
.kpi-label{font-size:10px;font-weight:700;letter-spacing:.1em;text-transform:uppercase;color:var(--dim);margin-top:4px;}
.kpi-delta{font-size:11px;font-weight:700;margin-top:4px;} .kpi-delta.up{color:var(--emerald);} .kpi-delta.down{color:var(--ruby);}
.section-head{font-family:var(--display);font-size:22px;letter-spacing:.08em;margin:32px 0 14px;padding-bottom:6px;border-bottom:3px solid var(--ink);display:flex;align-items:baseline;gap:12px;}
.section-head .badge{font-family:var(--sans);font-size:10px;font-weight:700;background:var(--ruby);color:#fff;padding:3px 10px;border-radius:20px;}
.card{background:var(--surface);border-radius:var(--radius);padding:22px;margin-bottom:18px;box-shadow:0 1px 4px rgba(0,0,0,.06);}
.card-head{font-family:var(--display);font-size:16px;letter-spacing:.06em;color:var(--dim);margin-bottom:12px;}
.card-row{display:grid;grid-template-columns:1fr 1fr;gap:18px;margin-bottom:18px;}
.filter-bar{display:flex;gap:10px;align-items:center;flex-wrap:wrap;margin-bottom:14px;}
.filter-bar label{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--dim);}
.filter-bar select{padding:8px 14px;border:1.5px solid var(--smoke);border-radius:8px;font-family:var(--sans);font-size:13px;background:#fff;min-width:160px;}
.chip{display:inline-block;padding:6px 14px;border:1.5px solid var(--smoke);border-radius:20px;font-size:11px;font-weight:600;cursor:pointer;background:#fff;transition:all .15s;margin:3px;}
.chip.active{background:var(--ink);color:#fff;border-color:var(--ink);} .chip:hover:not(.active){background:#f0ede8;}
.chip.all{border-color:var(--ruby);color:var(--ruby);} .chip.all.active{background:var(--ruby);color:#fff;}
.chip.reg{border-color:var(--emerald);} .chip.reg.active{background:var(--emerald);color:#fff;border-color:var(--emerald);}
.tbl-wrap{overflow:auto;max-height:520px;border:1px solid var(--smoke);border-radius:8px;}
table.dt{width:100%;border-collapse:collapse;font-size:11px;}
table.dt th{font-size:9px;font-weight:700;letter-spacing:.1em;text-transform:uppercase;color:var(--dim);padding:10px 8px;text-align:left;border-bottom:2px solid var(--smoke);background:#faf9f7;position:sticky;top:0;z-index:1;}
table.dt th.num,table.dt td.num{text-align:right;} table.dt td{padding:8px;border-bottom:1px solid #f2f0ed;} table.dt tr:hover{background:#f9f7f4;}
table.dt .mono{font-family:var(--mono);font-size:10px;}
table.dt .bar-cell{position:relative;} table.dt .bar-bg{position:absolute;left:0;top:0;bottom:0;background:var(--ruby);opacity:.08;border-radius:0 4px 4px 0;}
.growth-up{color:var(--emerald);font-weight:700;} .growth-down{color:var(--ruby);font-weight:700;}
.tag{display:inline-block;font-size:9px;font-weight:700;letter-spacing:.04em;padding:2px 7px;border-radius:4px;} .tag-di{background:#eafaf1;color:var(--emerald);} .tag-st{background:#ebf5fb;color:var(--sapphire);}
.alert-card{background:#fff8f0;border:1.5px solid #ffd6a0;border-radius:var(--radius);padding:16px 20px;margin-bottom:18px;}
.alert-title{font-family:var(--display);font-size:14px;letter-spacing:.06em;color:var(--amber);margin-bottom:8px;}
.alert-item{display:flex;align-items:center;gap:10px;padding:6px 0;border-bottom:1px solid #f5efe5;font-size:12px;} .alert-item:last-child{border:none;}
.alert-days{font-family:var(--mono);font-size:11px;font-weight:700;color:var(--ruby);min-width:50px;}
.drawer{background:#faf9f7;border:1.5px solid var(--smoke);border-radius:var(--radius);padding:20px;margin-top:14px;}
.drawer-title{font-family:var(--display);font-size:18px;letter-spacing:.06em;margin-bottom:12px;}
.multi-sel{min-width:240px;height:120px;font-size:12px;font-family:var(--sans);}
#loading{text-align:center;padding:60px;color:var(--dim);font-size:14px;}
@media(max-width:768px){.card-row{grid-template-columns:1fr;}.kpi-strip{flex-direction:column;}}
</style>
</head>
<body>
<form id="form1" runat="server">
<div class="top-bar">
    <a class="nav-logo" href="/StockApp/ERPHome.aspx">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    </a>
    <span class="nav-title">FINANCE</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblNavUser" runat="server"/></span>
        <a href="FINHome.aspx" class="nav-link">&#8592; FIN Home</a>
        <a href="/StockApp/ERPHome.aspx" class="nav-link">&#8592; ERP Home</a>
        <a href="FINLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</div>
<div class="page-header">
    <div class="page-icon">&#x1F4CA;</div>
    <div>
        <div class="page-title">SALES <span>ANALYTICS</span></div>
        <div class="page-sub">Revenue trends, product performance, distributor intelligence</div>
    </div>
</div>
<div class="page">
    <div class="main-tabs">
        <div class="main-tab active" onclick="switchTab(0)">All Data</div>
        <div class="main-tab" onclick="switchTab(1)">FY 2025-26</div>
        <div class="main-tab" onclick="switchTab(2)">Product View</div>
        <div class="main-tab" onclick="switchTab(3)">Distributor View</div>
    </div>
    <div id="app"><div id="loading">Loading dashboard...</div></div>
</div>
<script>
(function(){
var API='FINAnalyticsAPI.ashx',C=['#cc1e1e','#1a9e6a','#1e5fcc','#d68b00','#7c3aed','#e67e22','#16a085','#8e44ad','#2c3e50','#f39c12','#c0392b','#27ae60','#3498db','#d35400','#7f8c8d'];
var app=document.getElementById('app'),statesCache=null,allProducts=null;
function q(a,p){var u=API+'?action='+a;if(p)for(var k in p)if(p[k])u+='&'+k+'='+encodeURIComponent(p[k]);return fetch(u).then(function(r){return r.json();});}
function fmt(v){if(v>=1e7)return'\u20B9'+(v/1e7).toFixed(1)+'Cr';if(v>=1e5)return'\u20B9'+(v/1e5).toFixed(1)+'L';if(v>=1e3)return'\u20B9'+(v/1e3).toFixed(1)+'K';return'\u20B9'+Math.round(v).toLocaleString('en-IN');}
function fM(ym){var p=ym.split('-');var m=['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];return m[parseInt(p[1])-1]+' '+p[0].substring(2);}
function pct(c,p){if(!p||p===0)return'';var v=((c-p)/p*100).toFixed(0);return'<span class="'+(v>=0?'growth-up':'growth-down')+'">'+(v>=0?'\u25B2':'\u25BC')+' '+Math.abs(v)+'%</span>';}
function mc(id,type,labels,ds,opts){var el=document.getElementById(id);if(!el)return;if(el._ci)el._ci.destroy();var d={responsive:true,maintainAspectRatio:false,plugins:{legend:{position:'bottom',labels:{boxWidth:10,padding:8,font:{family:"'DM Sans'",size:10}}}},scales:type==='doughnut'||type==='pie'?{}:{y:{ticks:{callback:function(v){return fmt(v);},font:{family:"'JetBrains Mono'",size:9}},grid:{color:'#f0ede8'}},x:{ticks:{font:{size:9}},grid:{display:false}}}};el._ci=new Chart(el,{type:type,data:{labels:labels,datasets:ds},options:Object.assign(d,opts||{})});}
function pvt(items,months,label){var h='<table class="dt"><thead><tr><th>#</th><th>'+label+'</th><th class="num">Total</th>';months.forEach(function(m){h+='<th class="num">'+fM(m)+'</th>';});h+='<th class="num">Trend</th></tr></thead><tbody>';items.forEach(function(it,i){h+='<tr><td>'+(i+1)+'</td><td><strong>'+it.name+'</strong></td><td class="num mono" style="font-weight:700">'+fmt(it.total)+'</td>';it.monthly.forEach(function(v){h+='<td class="num mono">'+(v>0?fmt(v):'\u2014')+'</td>';});var l=it.monthly.length;h+='<td class="num">'+(l>=2?pct(it.monthly[l-1],it.monthly[l-2]):'')+'</td></tr>';});return h+'</tbody></table>';}

window.switchTab=function(i){document.querySelectorAll('.main-tab').forEach(function(t,j){t.className='main-tab'+(j===i?' active':'');});if(i===0)loadFull('','');else if(i===1)loadFull('2025-04-01','2026-03-31');else if(i===2)loadProductView();else if(i===3)loadDistView();};

// ═══ TAB 1 & 2 ═══
function loadFull(df,dt){app.innerHTML='<div id="loading">Loading...</div>';Promise.all([q('overview',{dateFrom:df,dateTo:dt}),q('monthlyTrend',{dateFrom:df,dateTo:dt}),q('stateBreakdown',{dateFrom:df,dateTo:dt}),q('topProducts',{dateFrom:df,dateTo:dt}),q('alerts',{})]).then(function(r){var ov=r[0],trend=Array.isArray(r[1])?r[1]:[],states=r[2]&&r[2].states?r[2]:{months:[],states:[]},products=Array.isArray(r[3])?r[3]:[],alerts=r[4]&&r[4].silentDistributors?r[4]:{silentDistributors:[]};renderFull(ov,trend,states,products,alerts,df,dt);}).catch(function(e){app.innerHTML='<div style="color:red;padding:40px;">Error: '+e.message+'</div>';});}
function renderFull(ov,trend,states,products,alerts,df,dt){statesCache=states;window._trendData=trend;var gc=ov.growthPct>=0?'up':'down',ga=ov.growthPct>=0?'\u25B2':'\u25BC',lbl=df?'FY 2025-26':'All Time',h='';h+='<div class="kpi-strip"><div class="kpi accent"><div class="kpi-val">'+fmt(ov.totalSales)+'</div><div class="kpi-label">'+lbl+' Revenue</div></div><div class="kpi green"><div class="kpi-val">'+fmt(ov.thisMonth)+'</div><div class="kpi-label">Latest Month</div><div class="kpi-delta '+gc+'">'+ga+' '+Math.abs(ov.growthPct).toFixed(0)+'% vs prev</div></div><div class="kpi blue"><div class="kpi-val">'+ov.totalInvoices.toLocaleString()+'</div><div class="kpi-label">Invoices</div></div><div class="kpi amber"><div class="kpi-val">'+ov.totalCustomers+'</div><div class="kpi-label">Customers</div></div><div class="kpi" style="border-left-color:var(--plum)"><div class="kpi-val">'+fmt(ov.totalReceipts||0)+'</div><div class="kpi-label">Receipts Collected</div></div><div class="kpi"><div class="kpi-val">'+ov.monthCount+'</div><div class="kpi-label">Months</div></div></div>';h+='<div class="section-head">Revenue Trend</div><div class="card"><div style="display:flex;align-items:center;gap:8px;margin-bottom:10px;"><label style="font-size:12px;font-weight:600;cursor:pointer;display:flex;align-items:center;gap:6px;"><input type="checkbox" id="chkReceipts" onchange="window._toggleReceipts()" style="width:16px;height:16px;accent-color:var(--emerald);cursor:pointer;"/> Show Receipts</label></div><div style="height:300px"><canvas id="cTrend"></canvas></div></div>';h+='<div class="section-head">State Performance</div><div class="card-row"><div class="card"><div class="card-head">Revenue Share</div><div style="height:280px"><canvas id="cSP"></canvas></div></div><div class="card"><div class="card-head">Monthly by State</div><div style="height:280px"><canvas id="cSB"></canvas></div></div></div>';h+='<div class="card"><div class="card-head">State x Month</div><div class="tbl-wrap">'+pvt(states.states,states.months,'State')+'</div></div>';h+='<div class="card"><div class="card-head">Drill Down: State \u2192 City</div><div class="filter-bar"><label>State</label><select id="selS1" onchange="window._lc()"><option value="">-- Select --</option>';states.states.forEach(function(s){h+='<option value="'+s.name+'">'+s.name+' ('+fmt(s.total)+')</option>';});h+='</select></div><div id="cityC"></div></div>';h+='<div class="section-head">Product Performance</div><div class="card-row"><div class="card"><div class="card-head">Top Products</div><div style="height:320px"><canvas id="cPB"></canvas></div></div><div class="card"><div class="card-head">Product Mix</div><div style="height:320px"><canvas id="cPP"></canvas></div></div></div>';h+='<div class="card"><div class="card-head">Product Details</div><div class="tbl-wrap"><table class="dt"><thead><tr><th>#</th><th>Product</th><th class="num">Revenue</th><th class="num">Qty</th><th class="num">Invoices</th><th class="num">Customers</th><th class="num">Share</th></tr></thead><tbody>';var pt=products.reduce(function(s,p){return s+p.sales;},0);products.forEach(function(p,i){var pc=pt>0?(p.sales/pt*100).toFixed(1):'0';h+='<tr><td>'+(i+1)+'</td><td class="bar-cell"><div class="bar-bg" style="width:'+pc+'%"></div><strong>'+p.name+'</strong></td><td class="num mono">'+fmt(p.sales)+'</td><td class="num mono">'+Math.round(p.qty).toLocaleString()+'</td><td class="num">'+p.invoices+'</td><td class="num">'+p.customers+'</td><td class="num">'+pc+'%</td></tr>';});h+='</tbody></table></div></div>';h+='<div class="card"><div class="card-head">Product Trends by State</div><div class="filter-bar"><label>State</label><select id="selS2" onchange="window._lpt()"><option value="">-- Select --</option>';states.states.forEach(function(s){h+='<option value="'+s.name+'">'+s.name+'</option>';});h+='</select></div><div id="ptC"></div></div>';h+='<div class="section-head">Distributor Intelligence</div>';if(alerts.silentDistributors&&alerts.silentDistributors.length>0){h+='<div class="alert-card"><div class="alert-title">\u26A0 Silent Distributors \u2014 45+ Days</div>';alerts.silentDistributors.forEach(function(d){h+='<div class="alert-item"><span class="alert-days">'+d.daysSilent+'d</span><span style="flex:1;font-weight:600">'+d.name+'</span><span style="color:var(--dim);font-size:11px">'+d.city+', '+d.state+'</span><span class="mono" style="font-size:10px">'+fmt(d.totalSales)+'</span></div>';});h+='</div>';}h+='<div class="card"><div class="card-head">Distributor Performance</div><div class="filter-bar"><label>State</label><select id="selS3" onchange="window._ld()"><option value="">-- Select --</option>';states.states.forEach(function(s){h+='<option value="'+s.name+'">'+s.name+'</option>';});h+='</select></div><div id="distC"></div></div>';app.innerHTML=h;app.dataset.df=df;app.dataset.dt=dt;window._renderTrend(false);mc('cSP','doughnut',states.states.map(function(s){return s.name;}),[{data:states.states.map(function(s){return s.total;}),backgroundColor:C.slice(0,states.states.length),borderWidth:0}],{cutout:'55%'});mc('cSB','bar',states.months.map(fM),states.states.map(function(s,i){return{label:s.name,data:s.monthly,backgroundColor:C[i%C.length]+'cc'};}),{scales:{x:{stacked:true},y:{stacked:true,ticks:{callback:function(v){return fmt(v);}}}}});mc('cPB','bar',products.slice(0,10).map(function(p){return p.name.length>25?p.name.substring(0,22)+'...':p.name;}),[{data:products.slice(0,10).map(function(p){return p.sales;}),backgroundColor:C.slice(0,10).map(function(c){return c+'cc';})}],{indexAxis:'y',plugins:{legend:{display:false}}});var t8=products.slice(0,8),oS=products.slice(8).reduce(function(s,p){return s+p.sales;},0),pL=t8.map(function(p){return p.name;}),pD=t8.map(function(p){return p.sales;});if(oS>0){pL.push('Others');pD.push(oS);}mc('cPP','doughnut',pL,[{data:pD,backgroundColor:C.slice(0,pL.length),borderWidth:0}],{cutout:'50%'});}

// Drill-downs for Tab 1/2
window._lc=function(){var s=document.getElementById('selS1').value,d=document.getElementById('cityC'),df=app.dataset.df,dt=app.dataset.dt;if(!s){d.innerHTML='';return;}d.innerHTML='<div style="padding:20px;color:var(--dim)">Loading...</div>';q('cityBreakdown',{state:s,dateFrom:df,dateTo:dt}).then(function(data){d.innerHTML='<div style="height:320px;margin-bottom:16px"><canvas id="cCB"></canvas></div><div class="tbl-wrap">'+pvt(data.cities,data.months,'City')+'</div>';mc('cCB','bar',data.months.map(fM),data.cities.slice(0,12).map(function(c,i){return{label:c.name,data:c.monthly,backgroundColor:C[i%C.length]+'cc'};}),{scales:{x:{stacked:true},y:{stacked:true,ticks:{callback:function(v){return fmt(v);}}}}});});};
window._lpt=function(){var s=document.getElementById('selS2').value,d=document.getElementById('ptC'),df=app.dataset.df,dt=app.dataset.dt;if(!s){d.innerHTML='';return;}d.innerHTML='<div style="padding:20px;color:var(--dim)">Loading...</div>';q('productMix',{state:s,dateFrom:df,dateTo:dt}).then(function(data){d.innerHTML='<div style="height:340px;margin-bottom:16px"><canvas id="cPT"></canvas></div><div class="tbl-wrap">'+pvt(data.products,data.months,'Product')+'</div>';mc('cPT','line',data.months.map(fM),data.products.slice(0,10).map(function(p,i){return{label:p.name,data:p.monthly,borderColor:C[i],backgroundColor:'transparent',tension:.3,pointRadius:3,borderWidth:2};}));});};
window._ld=function(){var s=document.getElementById('selS3').value,d=document.getElementById('distC'),df=app.dataset.df,dt=app.dataset.dt;if(!s){d.innerHTML='';return;}d.innerHTML='<div style="padding:20px;color:var(--dim)">Loading...</div>';q('distributors',{state:s,dateFrom:df,dateTo:dt}).then(function(data){var h='<div style="height:'+Math.max(300,Math.min(data.length,20)*28)+'px;margin-bottom:18px"><canvas id="cDB"></canvas></div><div class="tbl-wrap"><table class="dt"><thead><tr><th>#</th><th>Distributor</th><th>Type</th><th>City</th><th class="num">Revenue</th><th class="num">Orders</th><th class="num">Active Mo</th><th class="num">Last Order</th><th class="num">Days Ago</th><th class="num">Repeat</th></tr></thead><tbody>';data.forEach(function(x,i){var rp=(x.activeMonths/13*100).toFixed(0),rc=rp>=70?'growth-up':rp<40?'growth-down':'',dc=x.daysSinceLast>45?'growth-down':x.daysSinceLast>30?'':'growth-up';h+='<tr><td>'+(i+1)+'</td><td><a href="#" class="dl" data-id="'+x.id+'" style="font-weight:700;color:var(--ink);text-decoration:none;border-bottom:1px dashed var(--dim)">'+x.name+'</a></td><td><span class="tag tag-'+(x.type==='DI'?'di':'st')+'">'+x.type+'</span></td><td>'+x.city+'</td><td class="num mono">'+fmt(x.sales)+'</td><td class="num">'+x.orders+'</td><td class="num">'+x.activeMonths+'</td><td class="num">'+(x.lastOrder||'\u2014')+'</td><td class="num '+dc+'">'+x.daysSinceLast+'d</td><td class="num '+rc+'">'+rp+'%</td></tr>';});h+='</tbody></table></div><div id="distDD"></div>';d.innerHTML=h;d.querySelectorAll('.dl').forEach(function(a){a.addEventListener('click',function(e){e.preventDefault();window._ldd(parseInt(a.dataset.id),a.textContent);});});var t20=data.filter(function(x){return x.sales>0;}).slice(0,20);mc('cDB','bar',t20.map(function(x){return x.name.length>30?x.name.substring(0,27)+'...':x.name;}),[{data:t20.map(function(x){return x.sales;}),backgroundColor:t20.map(function(x,i){return C[i%C.length]+'cc';})}],{indexAxis:'y',plugins:{legend:{display:false}}});});};
window._ldd=function(id,name){var d=document.getElementById('distDD'),df=app.dataset.df,dt=app.dataset.dt;d.innerHTML='<div class="drawer"><div style="color:var(--dim)">Loading...</div></div>';d.scrollIntoView({behavior:'smooth',block:'nearest'});q('distDetail',{customerId:id,dateFrom:df,dateTo:dt}).then(function(data){var h='<div class="drawer"><div class="drawer-title">'+name+'</div><div class="card-row"><div><div class="card-head">Monthly Sales</div><div style="height:220px"><canvas id="cDM"></canvas></div></div><div><div class="card-head">Product Mix</div><div style="height:220px"><canvas id="cDP"></canvas></div></div></div><div class="tbl-wrap" style="margin-top:12px"><table class="dt"><thead><tr><th>#</th><th>Product</th><th class="num">Revenue</th><th class="num">Qty</th><th class="num">Orders</th></tr></thead><tbody>';data.products.forEach(function(p,i){h+='<tr><td>'+(i+1)+'</td><td><strong>'+p.name+'</strong></td><td class="num mono">'+fmt(p.sales)+'</td><td class="num">'+Math.round(p.qty)+'</td><td class="num">'+p.orders+'</td></tr>';});h+='</tbody></table></div></div>';d.innerHTML=h;mc('cDM','bar',data.monthly.map(function(m){return fM(m.month);}),[{label:'Sales',data:data.monthly.map(function(m){return m.sales;}),backgroundColor:'#cc1e1ecc',borderRadius:4}],{plugins:{legend:{display:false}}});var t6=data.products.slice(0,6);mc('cDP','doughnut',t6.map(function(p){return p.name;}),[{data:t6.map(function(p){return p.sales;}),backgroundColor:C.slice(0,6),borderWidth:0}],{cutout:'45%'});});};

// ═══ TREND CHART WITH RECEIPTS TOGGLE ═══
window._renderTrend=function(showReceipts){
    var trend=window._trendData||[];
    var datasets=[{label:'Sales Revenue',data:trend.map(function(t){return t.sales;}),borderColor:'#cc1e1e',backgroundColor:'rgba(204,30,30,.08)',fill:true,tension:.35,pointRadius:4,pointBackgroundColor:'#cc1e1e',borderWidth:2.5}];
    if(showReceipts){datasets.push({label:'Receipts',data:trend.map(function(t){return t.receipts||0;}),borderColor:'#1a9e6a',backgroundColor:'rgba(26,158,106,.08)',fill:true,tension:.35,pointRadius:4,pointBackgroundColor:'#1a9e6a',borderWidth:2.5,borderDash:[6,3]});}
    mc('cTrend','line',trend.map(function(t){return fM(t.month);}),datasets);
};
window._toggleReceipts=function(){var chk=document.getElementById('chkReceipts');window._renderTrend(chk&&chk.checked);};

// ═══ TAB 3: PRODUCT VIEW ═══
function loadProductView(){app.innerHTML='<div id="loading">Loading...</div>';var pr=allProducts?Promise.resolve(allProducts):q('productList',{});pr.then(function(prods){allProducts=prods;var h='<div class="section-head">Product Performance View</div><div class="card"><div class="filter-bar"><label>State</label><select id="pvS"><option value="ALL">All States</option></select><label>Period</label><select id="pvP"><option value="FY">FY 2025-26</option><option value="ALL">All Data</option></select></div><div style="font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--dim);margin-bottom:8px">Select Products</div><div id="pvChips"><div class="chip all active" data-prod="ALL" onclick="window._pvT(this)">All Products</div>';prods.forEach(function(p){h+='<div class="chip" data-prod="'+p+'" onclick="window._pvT(this)">'+p+'</div>';});h+='</div></div><div id="pvC"><div style="padding:20px;color:var(--dim)">Loading...</div></div>';app.innerHTML=h;var sel=document.getElementById('pvS');if(statesCache){statesCache.states.forEach(function(s){var o=document.createElement('option');o.value=s.name;o.text=s.name;sel.appendChild(o);});}else{q('stateBreakdown',{}).then(function(d){statesCache=d;d.states.forEach(function(s){var o=document.createElement('option');o.value=s.name;o.text=s.name;sel.appendChild(o);});});}document.getElementById('pvS').onchange=function(){window._pvR();};document.getElementById('pvP').onchange=function(){window._pvR();};window._pvR();});}
window._pvT=function(chip){if(chip.dataset.prod==='ALL'){document.querySelectorAll('#pvChips .chip').forEach(function(c){c.classList.remove('active');});chip.classList.add('active');}else{document.querySelector('#pvChips .chip.all').classList.remove('active');chip.classList.toggle('active');if(!document.querySelector('#pvChips .chip.active'))document.querySelector('#pvChips .chip.all').classList.add('active');}window._pvR();};
window._pvR=function(){var state=document.getElementById('pvS').value,period=document.getElementById('pvP').value,df='',dt='';if(period==='FY'){df='2025-04-01';dt='2026-03-31';}var selected=[];if(document.querySelector('#pvChips .chip.all.active'))selected=['ALL'];else document.querySelectorAll('#pvChips .chip.active').forEach(function(c){if(c.dataset.prod!=='ALL')selected.push(c.dataset.prod);});var div=document.getElementById('pvC');div.innerHTML='<div style="padding:20px;color:var(--dim)">Loading...</div>';q('productView',{state:state,dateFrom:df,dateTo:dt}).then(function(data){if(!data||!data.products){div.innerHTML='<div style="padding:20px;color:var(--ruby)">'+(data&&data.error?data.error:'Error')+'</div>';return;}var fp=data.products,fs=data.summary||[];if(selected[0]!=='ALL'){fp=data.products.filter(function(p){return selected.indexOf(p.name)>=0;});fs=(data.summary||[]).filter(function(p){return selected.indexOf(p.name)>=0;});}renderPV(data.months,fp,fs);}).catch(function(e){div.innerHTML='<div style="padding:20px;color:var(--ruby)">Error: '+e.message+'</div>';});};
function renderPV(months,products,summary){var div=document.getElementById('pvC');if(!products.length){div.innerHTML='<div style="padding:20px;color:var(--dim)">No data for selected products.</div>';return;}var h='<div style="height:400px;margin-bottom:18px"><canvas id="cPV"></canvas></div>';var total=summary.reduce(function(s,p){return s+p.sales;},0);h+='<div class="tbl-wrap"><table class="dt"><thead><tr><th>#</th><th>Product</th><th class="num">Revenue</th><th class="num">Qty</th><th class="num">Invoices</th><th class="num">Customers</th><th class="num">Share</th>';months.forEach(function(m){h+='<th class="num">'+fM(m)+'</th>';});h+='<th class="num">Trend</th></tr></thead><tbody>';products.forEach(function(p,i){var s=summary.find(function(x){return x.name===p.name;})||{};var pc=total>0?(p.total/total*100).toFixed(1):'0';h+='<tr><td>'+(i+1)+'</td><td><strong>'+p.name+'</strong></td><td class="num mono" style="font-weight:700">'+fmt(p.total)+'</td><td class="num mono">'+(s.qty?Math.round(s.qty).toLocaleString():'')+'</td><td class="num">'+(s.invoices||'')+'</td><td class="num">'+(s.customers||'')+'</td><td class="num">'+pc+'%</td>';p.monthly.forEach(function(v){h+='<td class="num mono">'+(v>0?fmt(v):'\u2014')+'</td>';});var l=p.monthly.length;h+='<td class="num">'+(l>=2?pct(p.monthly[l-1],p.monthly[l-2]):'')+'</td></tr>';});h+='</tbody></table></div>';div.innerHTML=h;mc('cPV','line',months.map(fM),products.slice(0,15).map(function(p,i){return{label:p.name,data:p.monthly,borderColor:C[i%C.length],backgroundColor:'transparent',tension:.3,pointRadius:3,borderWidth:2.5};}));}

// ═══ TAB 4: DISTRIBUTOR VIEW ═══
function loadDistView(){
    app.innerHTML='<div id="loading">Loading...</div>';
    var pr=allProducts?Promise.resolve(allProducts):q('productList',{});
    var sr=statesCache?Promise.resolve(statesCache):q('stateBreakdown',{});
    Promise.all([pr,sr]).then(function(r){
        allProducts=r[0]; statesCache=r[1];
        var h='<div class="section-head">Distributor View</div>';
        h+='<div class="card"><div class="filter-bar">';
        h+='<label>State</label><select id="dvS"><option value="ALL">All States</option>';
        statesCache.states.forEach(function(s){h+='<option value="'+s.name+'">'+s.name+'</option>';});
        h+='</select>';
        h+='<label>Period</label><select id="dvP"><option value="FY">FY 2025-26</option><option value="ALL">All Data</option></select>';
        h+='</div></div>';

        // Section 1: Monthly sales
        h+='<div class="card"><div class="card-head">1. Distributor Sales — Month over Month</div><div id="dvSec1"><div style="padding:20px;color:var(--dim)">Select filters above and click a section...</div></div></div>';

        // Section 2: Orders & Value
        h+='<div class="card"><div class="card-head">2. Orders &amp; Value + Order Gap Analysis</div><div id="dvSec2"></div></div>';

        // Section 3: Regularity
        h+='<div class="card"><div class="card-head">3. Regular Ordering Distributors</div><div id="dvSec3"></div></div>';

        // Section 4: Product filter
        h+='<div class="card"><div class="card-head">4. Distributor Orders by Product</div>';
        h+='<div class="filter-bar"><label>Products</label><select id="dvProdSel" multiple class="multi-sel">';
        allProducts.forEach(function(p){h+='<option value="'+p+'">'+p+'</option>';});
        h+='</select><button type="button" onclick="window._dvProd()" style="padding:8px 18px;border:none;border-radius:8px;background:var(--ink);color:#fff;font-weight:700;cursor:pointer;font-family:var(--sans);font-size:12px;">Apply</button></div>';
        h+='<div id="dvSec4"></div></div>';

        app.innerHTML=h;
        document.getElementById('dvS').onchange=function(){window._dvRefresh();};
        document.getElementById('dvP').onchange=function(){window._dvRefresh();};
        window._dvRefresh();
    });
}

window._dvRefresh=function(){
    var state=document.getElementById('dvS').value;
    var period=document.getElementById('dvP').value;
    var df='',dt='';if(period==='FY'){df='2025-04-01';dt='2026-03-31';}

    // Load distributor view data
    var d1=document.getElementById('dvSec1'),d2=document.getElementById('dvSec2'),d3=document.getElementById('dvSec3');
    d1.innerHTML=d2.innerHTML=d3.innerHTML='<div style="padding:16px;color:var(--dim)">Loading...</div>';

    q('distView',{state:state,dateFrom:df,dateTo:dt}).then(function(data){
        if(!data||!data.distributors){d1.innerHTML='<div style="color:var(--ruby)">'+(data&&data.error||'Error')+'</div>';return;}

        // Section 1: Monthly chart + table
        var h1='<div style="height:360px;margin-bottom:16px"><canvas id="cDV1"></canvas></div>';
        h1+='<div class="tbl-wrap">'+pvt(data.distributors.slice(0,30),data.months,'Distributor')+'</div>';
        d1.innerHTML=h1;
        mc('cDV1','bar',data.months.map(fM),data.distributors.slice(0,8).map(function(d,i){return{label:d.name,data:d.monthly,backgroundColor:C[i%C.length]+'cc'};}),{scales:{x:{stacked:true},y:{stacked:true,ticks:{callback:function(v){return fmt(v);}}}}});

        // Section 2: Orders table with gap analysis
        var summ=data.summary||[];
        var h2='<div class="tbl-wrap"><table class="dt"><thead><tr><th>#</th><th>Distributor</th><th>Type</th><th>City</th><th class="num">Orders</th><th class="num">Revenue</th><th class="num">Avg Order</th><th class="num">Avg Gap (days)</th><th class="num">Min Gap</th><th class="num">Max Gap</th><th class="num">Days Since Last</th></tr></thead><tbody>';
        summ.forEach(function(d,i){
            var avgOrd=d.orders>0?(d.sales/d.orders):0;
            var gapCls=d.avgGap>0&&d.avgGap<=30?'growth-up':d.avgGap>90?'growth-down':'';
            var dslCls=d.daysSinceLast>45?'growth-down':d.daysSinceLast<=30?'growth-up':'';
            h2+='<tr><td>'+(i+1)+'</td><td><strong>'+d.name+'</strong></td>';
            h2+='<td><span class="tag tag-'+(d.type==='DI'?'di':'st')+'">'+d.type+'</span></td>';
            h2+='<td>'+d.city+'</td>';
            h2+='<td class="num">'+d.orders+'</td>';
            h2+='<td class="num mono">'+fmt(d.sales)+'</td>';
            h2+='<td class="num mono">'+fmt(avgOrd)+'</td>';
            h2+='<td class="num '+gapCls+'">'+(d.avgGap>0?d.avgGap.toFixed(0):'\u2014')+'</td>';
            h2+='<td class="num">'+(d.minGap>0?d.minGap:'\u2014')+'</td>';
            h2+='<td class="num">'+(d.maxGap>0?d.maxGap:'\u2014')+'</td>';
            h2+='<td class="num '+dslCls+'">'+d.daysSinceLast+'d</td></tr>';
        });
        h2+='</tbody></table></div>';
        d2.innerHTML=h2;

        // Section 3: Regularity buckets
        var reg=data.regularity||{};
        var h3='<div style="display:flex;gap:12px;flex-wrap:wrap;margin-bottom:16px;">';
        [60,90,120,180,270,360].forEach(function(b){
            var count=reg['d'+b]||0;
            var cls=count>0?'reg active':'reg';
            h3+='<div class="chip '+cls+'" style="cursor:default;font-size:13px;padding:10px 20px;">';
            h3+='<strong>'+count+'</strong> distributors order within <strong>'+b+'</strong> days';
            h3+='</div>';
        });
        h3+='</div>';
        h3+='<div style="font-size:11px;color:var(--dim);">Based on average gap between orders. Distributors with 2+ orders and average gap \u2264 threshold are counted.</div>';
        d3.innerHTML=h3;
    }).catch(function(e){d1.innerHTML='<div style="color:var(--ruby)">Error: '+e.message+'</div>';console.error(e);});
};

window._dvProd=function(){
    var sel=document.getElementById('dvProdSel');
    var selected=[];
    for(var i=0;i<sel.options.length;i++){if(sel.options[i].selected)selected.push(sel.options[i].value);}
    if(selected.length===0){document.getElementById('dvSec4').innerHTML='<div style="padding:16px;color:var(--dim)">Select one or more products.</div>';return;}
    var state=document.getElementById('dvS').value;
    var period=document.getElementById('dvP').value;
    var df='',dt='';if(period==='FY'){df='2025-04-01';dt='2026-03-31';}
    var div=document.getElementById('dvSec4');
    div.innerHTML='<div style="padding:16px;color:var(--dim)">Loading...</div>';
    q('distOrdersByProduct',{state:state,dateFrom:df,dateTo:dt,products:selected.join('|')}).then(function(data){
        if(!data||!data.distributors){div.innerHTML='<div style="color:var(--ruby)">'+(data&&data.error||'No data')+'</div>';return;}
        var h='<div style="height:360px;margin-bottom:16px"><canvas id="cDV4"></canvas></div>';
        h+='<div class="tbl-wrap">'+pvt(data.distributors.slice(0,30),data.months,'Distributor')+'</div>';
        div.innerHTML=h;
        mc('cDV4','bar',data.months.map(fM),data.distributors.slice(0,10).map(function(d,i){return{label:d.name,data:d.monthly,backgroundColor:C[i%C.length]+'cc'};}),{scales:{x:{stacked:true},y:{stacked:true,ticks:{callback:function(v){return fmt(v);}}}}});
    }).catch(function(e){div.innerHTML='<div style="color:var(--ruby)">Error: '+e.message+'</div>';});
};

// ═══ INIT ═══
loadFull('','');
})();
</script>
<script src="/StockApp/erp-keepalive.js"></script>
</form>
</body>
</html>
