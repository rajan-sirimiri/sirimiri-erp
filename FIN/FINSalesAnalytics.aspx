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
.top-bar img{height:28px;background:#fff;border-radius:4px;padding:2px 6px;}
.top-bar h1{color:#fff;font-family:var(--display);font-size:18px;letter-spacing:.1em;flex:1;}
.top-bar a{color:rgba(255,255,255,.7);font-size:12px;font-weight:600;text-decoration:none;} .top-bar a:hover{color:#fff;}
.page{max-width:1280px;margin:0 auto;padding:28px 24px 80px;}
.main-tabs{display:flex;gap:0;margin-bottom:24px;border-radius:10px;overflow:hidden;border:2px solid var(--ink);}
.main-tab{flex:1;padding:13px 16px;text-align:center;font-family:var(--display);font-size:15px;letter-spacing:.08em;cursor:pointer;background:#fff;color:var(--ink);border:none;transition:all .15s;}
.main-tab.active{background:var(--ink);color:#fff;} .main-tab:hover:not(.active){background:#f0ede8;}
.kpi-strip{display:flex;gap:12px;margin-bottom:28px;flex-wrap:wrap;}
.kpi{flex:1;min-width:150px;background:var(--surface);border-radius:var(--radius);padding:18px 20px;border-left:4px solid var(--smoke);}
.kpi.accent{border-left-color:var(--ruby);} .kpi.green{border-left-color:var(--emerald);} .kpi.blue{border-left-color:var(--sapphire);} .kpi.amber{border-left-color:var(--amber);}
.kpi-val{font-family:var(--display);font-size:30px;line-height:1;letter-spacing:.03em;}
.kpi-label{font-size:10px;font-weight:700;letter-spacing:.1em;text-transform:uppercase;color:var(--dim);margin-top:4px;}
.kpi-delta{font-size:11px;font-weight:700;margin-top:4px;} .kpi-delta.up{color:var(--emerald);} .kpi-delta.down{color:var(--ruby);}
.section-head{font-family:var(--display);font-size:22px;letter-spacing:.08em;margin:32px 0 14px;padding-bottom:6px;border-bottom:3px solid var(--ink);display:flex;align-items:baseline;gap:12px;}
.section-head .badge{font-family:var(--sans);font-size:10px;font-weight:700;background:var(--ruby);color:#fff;padding:3px 10px;border-radius:20px;letter-spacing:.04em;}
.card{background:var(--surface);border-radius:var(--radius);padding:22px;margin-bottom:18px;box-shadow:0 1px 4px rgba(0,0,0,.06);}
.card-head{font-family:var(--display);font-size:16px;letter-spacing:.06em;color:var(--dim);margin-bottom:12px;}
.card-row{display:grid;grid-template-columns:1fr 1fr;gap:18px;margin-bottom:18px;}
.filter-bar{display:flex;gap:10px;align-items:center;flex-wrap:wrap;margin-bottom:18px;}
.filter-bar label{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--dim);}
.filter-bar select{padding:8px 14px;border:1.5px solid var(--smoke);border-radius:8px;font-family:var(--sans);font-size:13px;background:#fff;min-width:160px;cursor:pointer;}
.filter-bar select:focus{border-color:var(--ruby);outline:none;}
.prod-chips{display:flex;flex-wrap:wrap;gap:6px;margin-bottom:14px;}
.prod-chip{padding:6px 14px;border:1.5px solid var(--smoke);border-radius:20px;font-size:11px;font-weight:600;cursor:pointer;background:#fff;transition:all .15s;}
.prod-chip.active{background:var(--ink);color:#fff;border-color:var(--ink);} .prod-chip:hover:not(.active){background:#f0ede8;}
.prod-chip.all{border-color:var(--ruby);color:var(--ruby);} .prod-chip.all.active{background:var(--ruby);color:#fff;}
.tbl-wrap{overflow:auto;max-height:520px;border:1px solid var(--smoke);border-radius:8px;}
table.dt{width:100%;border-collapse:collapse;font-size:11px;}
table.dt th{font-size:9px;font-weight:700;letter-spacing:.1em;text-transform:uppercase;color:var(--dim);padding:10px 8px;text-align:left;border-bottom:2px solid var(--smoke);background:#faf9f7;position:sticky;top:0;z-index:1;}
table.dt th.num,table.dt td.num{text-align:right;}
table.dt td{padding:8px;border-bottom:1px solid #f2f0ed;}
table.dt tr:hover{background:#f9f7f4;}
table.dt .mono{font-family:var(--mono);font-size:10px;}
table.dt .bar-cell{position:relative;} table.dt .bar-bg{position:absolute;left:0;top:0;bottom:0;background:var(--ruby);opacity:.08;border-radius:0 4px 4px 0;}
.growth-up{color:var(--emerald);font-weight:700;} .growth-down{color:var(--ruby);font-weight:700;}
.tag{display:inline-block;font-size:9px;font-weight:700;letter-spacing:.04em;padding:2px 7px;border-radius:4px;}
.tag-di{background:#eafaf1;color:var(--emerald);} .tag-st{background:#ebf5fb;color:var(--sapphire);}
.alert-card{background:#fff8f0;border:1.5px solid #ffd6a0;border-radius:var(--radius);padding:16px 20px;margin-bottom:18px;}
.alert-title{font-family:var(--display);font-size:14px;letter-spacing:.06em;color:var(--amber);margin-bottom:8px;}
.alert-item{display:flex;align-items:center;gap:10px;padding:6px 0;border-bottom:1px solid #f5efe5;font-size:12px;} .alert-item:last-child{border:none;}
.alert-days{font-family:var(--mono);font-size:11px;font-weight:700;color:var(--ruby);min-width:50px;}
.drawer{background:#faf9f7;border:1.5px solid var(--smoke);border-radius:var(--radius);padding:20px;margin-top:14px;}
.drawer-title{font-family:var(--display);font-size:18px;letter-spacing:.06em;margin-bottom:12px;}
#loading{text-align:center;padding:60px;color:var(--dim);font-size:14px;}
@media(max-width:768px){.card-row{grid-template-columns:1fr;}.kpi-strip{flex-direction:column;}.kpi{min-width:100%;}}
</style>
</head>
<body>
<form id="form1" runat="server">
<div class="top-bar">
    <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    <h1>Sales Analytics</h1>
    <a href="FINHome.aspx">&#8592; FIN Home</a>
    <a href="FINLogout.aspx">Sign Out</a>
</div>
<div class="page">
    <div class="main-tabs">
        <div class="main-tab active" onclick="switchTab(0)">All Data</div>
        <div class="main-tab" onclick="switchTab(1)">FY 2025-26</div>
        <div class="main-tab" onclick="switchTab(2)">Product View</div>
    </div>
    <div id="app"><div id="loading">Loading dashboard...</div></div>
</div>
<script>
(function(){
var API='FINAnalyticsAPI.ashx', C=['#cc1e1e','#1a9e6a','#1e5fcc','#d68b00','#7c3aed','#e67e22','#16a085','#8e44ad','#2c3e50','#f39c12','#c0392b','#27ae60','#3498db','#d35400','#7f8c8d'];
var app=document.getElementById('app'), statesCache=null, allProducts=null;

function q(a,p){var u=API+'?action='+a;if(p)for(var k in p)if(p[k])u+='&'+k+'='+encodeURIComponent(p[k]);return fetch(u).then(function(r){return r.json();});}
function fmt(v){if(v>=1e7)return'\u20B9'+(v/1e7).toFixed(1)+'Cr';if(v>=1e5)return'\u20B9'+(v/1e5).toFixed(1)+'L';if(v>=1e3)return'\u20B9'+(v/1e3).toFixed(1)+'K';return'\u20B9'+Math.round(v).toLocaleString('en-IN');}
function fM(ym){var p=ym.split('-');var m=['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];return m[parseInt(p[1])-1]+' '+p[0].substring(2);}
function pct(c,p){if(!p||p===0)return'';var v=((c-p)/p*100).toFixed(0);return'<span class="'+(v>=0?'growth-up':'growth-down')+'">'+(v>=0?'\u25B2':'\u25BC')+' '+Math.abs(v)+'%</span>';}
function mc(id,type,labels,datasets,opts){var el=document.getElementById(id);if(!el)return;if(el._ci)el._ci.destroy();var d={responsive:true,maintainAspectRatio:false,plugins:{legend:{position:'bottom',labels:{boxWidth:10,padding:8,font:{family:"'DM Sans'",size:10}}}},scales:type==='doughnut'||type==='pie'?{}:{y:{ticks:{callback:function(v){return fmt(v);},font:{family:"'JetBrains Mono'",size:9}},grid:{color:'#f0ede8'}},x:{ticks:{font:{size:9}},grid:{display:false}}}};el._ci=new Chart(el,{type:type,data:{labels:labels,datasets:datasets},options:Object.assign(d,opts||{})});}
function pvt(items,months,label){var h='<table class="dt"><thead><tr><th>#</th><th>'+label+'</th><th class="num">Total</th>';months.forEach(function(m){h+='<th class="num">'+fM(m)+'</th>';});h+='<th class="num">Trend</th></tr></thead><tbody>';items.forEach(function(it,i){h+='<tr><td>'+(i+1)+'</td><td><strong>'+it.name+'</strong></td><td class="num mono" style="font-weight:700">'+fmt(it.total)+'</td>';it.monthly.forEach(function(v){h+='<td class="num mono">'+(v>0?fmt(v):'\u2014')+'</td>';});var l=it.monthly.length;h+='<td class="num">'+(l>=2?pct(it.monthly[l-1],it.monthly[l-2]):'')+'</td></tr>';});return h+'</tbody></table>';}

// ═══ TAB SWITCHING ═══
window.switchTab=function(i){
    document.querySelectorAll('.main-tab').forEach(function(t,j){t.className='main-tab'+(j===i?' active':'');});
    if(i===0)loadFull('','');
    else if(i===1)loadFull('2025-04-01','2026-03-31');
    else loadProductView();
};

// ═══ TAB 1 & 2: FULL VIEW ═══
function loadFull(df,dt){
    app.innerHTML='<div id="loading">Loading...</div>';
    Promise.all([q('overview',{dateFrom:df,dateTo:dt}),q('monthlyTrend',{dateFrom:df,dateTo:dt}),q('stateBreakdown',{dateFrom:df,dateTo:dt}),q('topProducts',{dateFrom:df,dateTo:dt}),q('alerts',{})])
    .then(function(r){
        // Ensure arrays (API might return error objects)
        var ov=r[0], trend=Array.isArray(r[1])?r[1]:[], states=r[2]&&r[2].states?r[2]:{months:[],states:[]}, products=Array.isArray(r[3])?r[3]:[], alerts=r[4]&&r[4].silentDistributors?r[4]:{silentDistributors:[]};
        renderFull(ov,trend,states,products,alerts,df,dt);
    }).catch(function(e){app.innerHTML='<div style="color:red;padding:40px;">Error: '+e.message+'<br><small>Check browser console for details.</small></div>';console.error('Dashboard load error:',e);});
}
function renderFull(ov,trend,states,products,alerts,df,dt){
    statesCache=states;
    var gc=ov.growthPct>=0?'up':'down',ga=ov.growthPct>=0?'\u25B2':'\u25BC',lbl=df?'FY 2025-26':'All Time',h='';
    h+='<div class="kpi-strip"><div class="kpi accent"><div class="kpi-val">'+fmt(ov.totalSales)+'</div><div class="kpi-label">'+lbl+' Revenue</div></div>';
    h+='<div class="kpi green"><div class="kpi-val">'+fmt(ov.thisMonth)+'</div><div class="kpi-label">Latest Month</div><div class="kpi-delta '+gc+'">'+ga+' '+Math.abs(ov.growthPct).toFixed(0)+'% vs prev</div></div>';
    h+='<div class="kpi blue"><div class="kpi-val">'+ov.totalInvoices.toLocaleString()+'</div><div class="kpi-label">Invoices</div></div>';
    h+='<div class="kpi amber"><div class="kpi-val">'+ov.totalCustomers+'</div><div class="kpi-label">Customers</div></div>';
    h+='<div class="kpi"><div class="kpi-val">'+ov.monthCount+'</div><div class="kpi-label">Months</div></div></div>';
    h+='<div class="section-head">Revenue Trend</div><div class="card"><div style="height:300px"><canvas id="cTrend"></canvas></div></div>';
    h+='<div class="section-head">State Performance</div><div class="card-row"><div class="card"><div class="card-head">Revenue Share</div><div style="height:280px"><canvas id="cSP"></canvas></div></div><div class="card"><div class="card-head">Monthly by State</div><div style="height:280px"><canvas id="cSB"></canvas></div></div></div>';
    h+='<div class="card"><div class="card-head">State x Month</div><div class="tbl-wrap">'+pvt(states.states,states.months,'State')+'</div></div>';
    h+='<div class="card"><div class="card-head">Drill Down: State \u2192 City</div><div class="filter-bar"><label>State</label><select id="selS1" onchange="window._lc()"><option value="">-- Select --</option>';
    states.states.forEach(function(s){h+='<option value="'+s.name+'">'+s.name+' ('+fmt(s.total)+')</option>';});
    h+='</select></div><div id="cityC"></div></div>';
    h+='<div class="section-head">Product Performance</div><div class="card-row"><div class="card"><div class="card-head">Top Products</div><div style="height:320px"><canvas id="cPB"></canvas></div></div><div class="card"><div class="card-head">Product Mix</div><div style="height:320px"><canvas id="cPP"></canvas></div></div></div>';
    h+='<div class="card"><div class="card-head">Product Details</div><div class="tbl-wrap"><table class="dt"><thead><tr><th>#</th><th>Product</th><th class="num">Revenue</th><th class="num">Qty</th><th class="num">Invoices</th><th class="num">Customers</th><th class="num">Share</th></tr></thead><tbody>';
    var pt=products.reduce(function(s,p){return s+p.sales;},0);
    products.forEach(function(p,i){var pc=pt>0?(p.sales/pt*100).toFixed(1):'0';h+='<tr><td>'+(i+1)+'</td><td class="bar-cell"><div class="bar-bg" style="width:'+pc+'%"></div><strong>'+p.name+'</strong></td><td class="num mono">'+fmt(p.sales)+'</td><td class="num mono">'+Math.round(p.qty).toLocaleString()+'</td><td class="num">'+p.invoices+'</td><td class="num">'+p.customers+'</td><td class="num">'+pc+'%</td></tr>';});
    h+='</tbody></table></div></div>';
    h+='<div class="card"><div class="card-head">Product Trends by State</div><div class="filter-bar"><label>State</label><select id="selS2" onchange="window._lpt()"><option value="">-- Select --</option>';
    states.states.forEach(function(s){h+='<option value="'+s.name+'">'+s.name+'</option>';});
    h+='</select></div><div id="ptC"></div></div>';
    h+='<div class="section-head">Distributor Intelligence</div>';
    if(alerts.silentDistributors&&alerts.silentDistributors.length>0){h+='<div class="alert-card"><div class="alert-title">\u26A0 Silent Distributors \u2014 45+ Days</div>';alerts.silentDistributors.forEach(function(d){h+='<div class="alert-item"><span class="alert-days">'+d.daysSilent+'d</span><span style="flex:1;font-weight:600">'+d.name+'</span><span style="color:var(--dim);font-size:11px">'+d.city+', '+d.state+'</span><span class="mono" style="font-size:10px">'+fmt(d.totalSales)+'</span></div>';});h+='</div>';}
    h+='<div class="card"><div class="card-head">Distributor Performance</div><div class="filter-bar"><label>State</label><select id="selS3" onchange="window._ld()"><option value="">-- Select --</option>';
    states.states.forEach(function(s){h+='<option value="'+s.name+'">'+s.name+'</option>';});
    h+='</select></div><div id="distC"></div></div>';
    app.innerHTML=h;
    // Store date params for drill-downs
    app.dataset.df=df; app.dataset.dt=dt;
    // Charts
    mc('cTrend','line',trend.map(function(t){return fM(t.month);}),[{label:'Revenue',data:trend.map(function(t){return t.sales;}),borderColor:'#cc1e1e',backgroundColor:'rgba(204,30,30,.08)',fill:true,tension:.35,pointRadius:4,pointBackgroundColor:'#cc1e1e',borderWidth:2.5}]);
    mc('cSP','doughnut',states.states.map(function(s){return s.name;}),[{data:states.states.map(function(s){return s.total;}),backgroundColor:C.slice(0,states.states.length),borderWidth:0}],{cutout:'55%'});
    mc('cSB','bar',states.months.map(fM),states.states.map(function(s,i){return{label:s.name,data:s.monthly,backgroundColor:C[i%C.length]+'cc'};}),{scales:{x:{stacked:true},y:{stacked:true,ticks:{callback:function(v){return fmt(v);}}}}});
    mc('cPB','bar',products.slice(0,10).map(function(p){return p.name.length>25?p.name.substring(0,22)+'...':p.name;}),[{data:products.slice(0,10).map(function(p){return p.sales;}),backgroundColor:C.slice(0,10).map(function(c){return c+'cc';})}],{indexAxis:'y',plugins:{legend:{display:false}}});
    var t8=products.slice(0,8),oS=products.slice(8).reduce(function(s,p){return s+p.sales;},0),pL=t8.map(function(p){return p.name;}),pD=t8.map(function(p){return p.sales;});if(oS>0){pL.push('Others');pD.push(oS);}
    mc('cPP','doughnut',pL,[{data:pD,backgroundColor:C.slice(0,pL.length),borderWidth:0}],{cutout:'50%'});
}

// Drill-down handlers
window._lc=function(){var s=document.getElementById('selS1').value,d=document.getElementById('cityC'),df=app.dataset.df,dt=app.dataset.dt;if(!s){d.innerHTML='';return;}d.innerHTML='<div style="padding:20px;color:var(--dim)">Loading...</div>';q('cityBreakdown',{state:s,dateFrom:df,dateTo:dt}).then(function(data){d.innerHTML='<div style="height:320px;margin-bottom:16px"><canvas id="cCB"></canvas></div><div class="tbl-wrap">'+pvt(data.cities,data.months,'City')+'</div>';mc('cCB','bar',data.months.map(fM),data.cities.slice(0,12).map(function(c,i){return{label:c.name,data:c.monthly,backgroundColor:C[i%C.length]+'cc'};}),{scales:{x:{stacked:true},y:{stacked:true,ticks:{callback:function(v){return fmt(v);}}}}});});};
window._lpt=function(){var s=document.getElementById('selS2').value,d=document.getElementById('ptC'),df=app.dataset.df,dt=app.dataset.dt;if(!s){d.innerHTML='';return;}d.innerHTML='<div style="padding:20px;color:var(--dim)">Loading...</div>';q('productMix',{state:s,dateFrom:df,dateTo:dt}).then(function(data){d.innerHTML='<div style="height:340px;margin-bottom:16px"><canvas id="cPT"></canvas></div><div class="tbl-wrap">'+pvt(data.products,data.months,'Product')+'</div>';mc('cPT','line',data.months.map(fM),data.products.slice(0,10).map(function(p,i){return{label:p.name,data:p.monthly,borderColor:C[i],backgroundColor:'transparent',tension:.3,pointRadius:3,borderWidth:2};}));});};
window._ld=function(){var s=document.getElementById('selS3').value,d=document.getElementById('distC'),df=app.dataset.df,dt=app.dataset.dt;if(!s){d.innerHTML='';return;}d.innerHTML='<div style="padding:20px;color:var(--dim)">Loading...</div>';q('distributors',{state:s,dateFrom:df,dateTo:dt}).then(function(data){var h='<div style="height:'+Math.max(300,Math.min(data.length,20)*28)+'px;margin-bottom:18px"><canvas id="cDB"></canvas></div><div class="tbl-wrap"><table class="dt"><thead><tr><th>#</th><th>Distributor</th><th>Type</th><th>City</th><th class="num">Revenue</th><th class="num">Orders</th><th class="num">Active Mo</th><th class="num">Last Order</th><th class="num">Days Ago</th><th class="num">Repeat</th></tr></thead><tbody>';data.forEach(function(x,i){var rp=(x.activeMonths/13*100).toFixed(0),rc=rp>=70?'growth-up':rp<40?'growth-down':'',dc=x.daysSinceLast>45?'growth-down':x.daysSinceLast>30?'':'growth-up';h+='<tr><td>'+(i+1)+'</td><td><a href="#" class="dl" data-id="'+x.id+'" style="font-weight:700;color:var(--ink);text-decoration:none;border-bottom:1px dashed var(--dim)">'+x.name+'</a></td><td><span class="tag tag-'+(x.type==='DI'?'di':'st')+'">'+x.type+'</span></td><td>'+x.city+'</td><td class="num mono">'+fmt(x.sales)+'</td><td class="num">'+x.orders+'</td><td class="num">'+x.activeMonths+'</td><td class="num">'+(x.lastOrder||'\u2014')+'</td><td class="num '+dc+'">'+x.daysSinceLast+'d</td><td class="num '+rc+'">'+rp+'%</td></tr>';});h+='</tbody></table></div><div id="distDD"></div>';d.innerHTML=h;d.querySelectorAll('.dl').forEach(function(a){a.addEventListener('click',function(e){e.preventDefault();window._ldd(parseInt(a.dataset.id),a.textContent);});});var t20=data.filter(function(x){return x.sales>0;}).slice(0,20);mc('cDB','bar',t20.map(function(x){return x.name.length>30?x.name.substring(0,27)+'...':x.name;}),[{data:t20.map(function(x){return x.sales;}),backgroundColor:t20.map(function(x,i){return C[i%C.length]+'cc';})}],{indexAxis:'y',plugins:{legend:{display:false}}});});};
window._ldd=function(id,name){var d=document.getElementById('distDD'),df=app.dataset.df,dt=app.dataset.dt;d.innerHTML='<div class="drawer"><div style="color:var(--dim)">Loading...</div></div>';d.scrollIntoView({behavior:'smooth',block:'nearest'});q('distDetail',{customerId:id,dateFrom:df,dateTo:dt}).then(function(data){var h='<div class="drawer"><div class="drawer-title">'+name+'</div><div class="card-row"><div><div class="card-head">Monthly Sales</div><div style="height:220px"><canvas id="cDM"></canvas></div></div><div><div class="card-head">Product Mix</div><div style="height:220px"><canvas id="cDP"></canvas></div></div></div><div class="tbl-wrap" style="margin-top:12px"><table class="dt"><thead><tr><th>#</th><th>Product</th><th class="num">Revenue</th><th class="num">Qty</th><th class="num">Orders</th></tr></thead><tbody>';data.products.forEach(function(p,i){h+='<tr><td>'+(i+1)+'</td><td><strong>'+p.name+'</strong></td><td class="num mono">'+fmt(p.sales)+'</td><td class="num">'+Math.round(p.qty)+'</td><td class="num">'+p.orders+'</td></tr>';});h+='</tbody></table></div></div>';d.innerHTML=h;mc('cDM','bar',data.monthly.map(function(m){return fM(m.month);}),[{label:'Sales',data:data.monthly.map(function(m){return m.sales;}),backgroundColor:'#cc1e1ecc',borderRadius:4}],{plugins:{legend:{display:false}}});var t6=data.products.slice(0,6);mc('cDP','doughnut',t6.map(function(p){return p.name;}),[{data:t6.map(function(p){return p.sales;}),backgroundColor:C.slice(0,6),borderWidth:0}],{cutout:'45%'});});};

// ═══ TAB 3: PRODUCT VIEW ═══
function loadProductView(){
    app.innerHTML='<div id="loading">Loading product list...</div>';
    var prom = allProducts ? Promise.resolve(allProducts) : q('productList',{});
    prom.then(function(prods){
        allProducts = prods;
        var h='<div class="section-head">Product Performance View</div><div class="card">';
        h+='<div class="filter-bar"><label>State</label><select id="pvS"><option value="ALL">All States</option></select>';
        h+='<label>Period</label><select id="pvP"><option value="FY">FY 2025-26</option><option value="ALL">All Data</option></select></div>';
        h+='<div style="font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--dim);margin-bottom:8px;">Select Products</div>';
        h+='<div class="prod-chips"><div class="prod-chip all active" data-prod="ALL" onclick="window._pvToggle(this)">All Products</div>';
        prods.forEach(function(p){h+='<div class="prod-chip" data-prod="'+p+'" onclick="window._pvToggle(this)">'+p+'</div>';});
        h+='</div></div><div id="pvC"><div style="padding:20px;color:var(--dim)">Loading...</div></div>';
        app.innerHTML=h;
        // Populate states
        var sel=document.getElementById('pvS');
        if(statesCache){statesCache.states.forEach(function(s){var o=document.createElement('option');o.value=s.name;o.text=s.name;sel.appendChild(o);});}
        else{q('stateBreakdown',{}).then(function(d){statesCache=d;d.states.forEach(function(s){var o=document.createElement('option');o.value=s.name;o.text=s.name;sel.appendChild(o);});});}
        document.getElementById('pvS').onchange=function(){window._pvRefresh();};
        document.getElementById('pvP').onchange=function(){window._pvRefresh();};
        window._pvRefresh();
    });
}

window._pvToggle=function(chip){
    var prod=chip.dataset.prod;
    if(prod==='ALL'){
        // Toggle all off, set ALL active
        document.querySelectorAll('.prod-chip').forEach(function(c){c.classList.remove('active');});
        chip.classList.add('active');
    } else {
        // Deselect ALL chip
        document.querySelector('.prod-chip.all').classList.remove('active');
        chip.classList.toggle('active');
        // If nothing selected, reselect ALL
        if(!document.querySelector('.prod-chip.active')){
            document.querySelector('.prod-chip.all').classList.add('active');
        }
    }
    window._pvRefresh();
};

window._pvRefresh=function(){
    var state=document.getElementById('pvS').value;
    var period=document.getElementById('pvP').value;
    var df='',dt='';
    if(period==='FY'){df='2025-04-01';dt='2026-03-31';}
    // Get selected products
    var selected=[];
    if(document.querySelector('.prod-chip.all.active')){selected=['ALL'];}
    else{document.querySelectorAll('.prod-chip.active').forEach(function(c){if(c.dataset.prod!=='ALL')selected.push(c.dataset.prod);});}
    var div=document.getElementById('pvC');
    div.innerHTML='<div style="padding:20px;color:var(--dim)">Loading...</div>';
    q('productView',{state:state,dateFrom:df,dateTo:dt}).then(function(data){
        if(!data||!data.products){div.innerHTML='<div style="padding:20px;color:var(--ruby)">Error loading product data: '+(data&&data.error?data.error:'Unknown error')+'</div>';return;}
        // Filter products if not ALL
        var filteredProducts=data.products;
        var filteredSummary=data.summary||[];
        if(selected[0]!=='ALL'){
            filteredProducts=data.products.filter(function(p){return selected.indexOf(p.name)>=0;});
            filteredSummary=(data.summary||[]).filter(function(p){return selected.indexOf(p.name)>=0;});
        }
        renderPV(data.months,filteredProducts,filteredSummary);
    }).catch(function(e){div.innerHTML='<div style="padding:20px;color:var(--ruby)">Error: '+e.message+'</div>';console.error('ProductView error:',e);});
};

function renderPV(months,products,summary){
    var div=document.getElementById('pvC');
    if(!products.length){div.innerHTML='<div style="padding:20px;color:var(--dim)">No data for selected products.</div>';return;}
    var h='<div style="height:400px;margin-bottom:18px"><canvas id="cPV"></canvas></div>';
    var total=summary.reduce(function(s,p){return s+p.sales;},0);
    h+='<div class="tbl-wrap"><table class="dt"><thead><tr><th>#</th><th>Product</th><th class="num">Revenue</th><th class="num">Qty</th><th class="num">Invoices</th><th class="num">Customers</th><th class="num">Share</th>';
    months.forEach(function(m){h+='<th class="num">'+fM(m)+'</th>';});
    h+='<th class="num">Trend</th></tr></thead><tbody>';
    products.forEach(function(p,i){
        var s=summary.find(function(x){return x.name===p.name;})||{};
        var pc=total>0?(p.total/total*100).toFixed(1):'0';
        h+='<tr><td>'+(i+1)+'</td><td><strong>'+p.name+'</strong></td>';
        h+='<td class="num mono" style="font-weight:700">'+fmt(p.total)+'</td>';
        h+='<td class="num mono">'+(s.qty?Math.round(s.qty).toLocaleString():'')+'</td>';
        h+='<td class="num">'+(s.invoices||'')+'</td><td class="num">'+(s.customers||'')+'</td>';
        h+='<td class="num">'+pc+'%</td>';
        p.monthly.forEach(function(v){h+='<td class="num mono">'+(v>0?fmt(v):'\u2014')+'</td>';});
        var l=p.monthly.length;h+='<td class="num">'+(l>=2?pct(p.monthly[l-1],p.monthly[l-2]):'')+'</td>';
        h+='</tr>';
    });
    h+='</tbody></table></div>';
    div.innerHTML=h;
    mc('cPV','line',months.map(fM),products.slice(0,15).map(function(p,i){return{label:p.name,data:p.monthly,borderColor:C[i%C.length],backgroundColor:'transparent',tension:.3,pointRadius:3,borderWidth:2.5};}));
}

// ═══ INIT ═══
loadFull('','');
})();
</script>
<script src="/StockApp/erp-keepalive.js"></script>
</form>
</body>
</html>
