<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FINOutstandingReport.aspx.cs" Inherits="FINApp.FINOutstandingReport" EnableEventValidation="false" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri FIN — Outstanding Report</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600;700&family=JetBrains+Mono:wght@400;700&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--ink:#0f0f0f;--paper:#f7f5f2;--smoke:#e8e5e0;--dim:#9a9590;--ruby:#cc1e1e;--emerald:#1a9e6a;--sapphire:#1e5fcc;--amber:#d68b00;--plum:#7c3aed;--surface:#fff;--mono:'JetBrains Mono',monospace;--sans:'DM Sans',sans-serif;--display:'Bebas Neue',sans-serif;--radius:10px;}
*,*::before,*::after{box-sizing:border-box;margin:0;padding:0;}
body{font-family:var(--sans);background:var(--paper);color:var(--ink);-webkit-font-smoothing:antialiased;}
.top-bar{background:#1a1a1a;display:flex;align-items:center;padding:0 28px;height:52px;gap:6px;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;margin-right:10px;}
.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:var(--display);font-size:18px;letter-spacing:.1em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:14px;}
.nav-user{font-size:12px;color:#999;}
.nav-link{color:#fff;font-size:12px;font-weight:600;text-decoration:none;opacity:.8;} .nav-link:hover{opacity:1;}
.page-header{background:var(--surface);border-bottom:3px solid var(--ruby);padding:24px 40px;display:flex;align-items:center;gap:16px;}
.page-icon{font-size:28px;}
.page-title{font-family:var(--display);font-size:30px;letter-spacing:.07em;} .page-title span{color:var(--ruby);}
.page-sub{font-size:12px;color:var(--dim);margin-top:2px;}
.page{max-width:1300px;margin:0 auto;padding:28px 24px 80px;}
.kpi-strip{display:flex;gap:12px;margin-bottom:24px;flex-wrap:wrap;}
.kpi{flex:1;min-width:150px;background:var(--surface);border-radius:var(--radius);padding:16px 18px;border-left:4px solid var(--smoke);}
.kpi.accent{border-left-color:var(--ruby);} .kpi.green{border-left-color:var(--emerald);} .kpi.blue{border-left-color:var(--sapphire);} .kpi.amber{border-left-color:var(--amber);}
.kpi-val{font-family:var(--display);font-size:28px;line-height:1;letter-spacing:.03em;}
.kpi-label{font-size:10px;font-weight:700;letter-spacing:.1em;text-transform:uppercase;color:var(--dim);margin-top:4px;}
.card{background:var(--surface);border-radius:var(--radius);padding:22px;margin-bottom:18px;box-shadow:0 1px 4px rgba(0,0,0,.06);}
.card-head{font-family:var(--display);font-size:16px;letter-spacing:.06em;color:var(--dim);margin-bottom:12px;}
.filter-bar{display:flex;gap:14px;align-items:center;flex-wrap:wrap;margin-bottom:18px;}
.filter-bar label{font-size:12px;font-weight:600;cursor:pointer;display:flex;align-items:center;gap:6px;}
.filter-bar input[type=checkbox]{width:16px;height:16px;accent-color:var(--ruby);cursor:pointer;}
.filter-bar select{padding:8px 14px;border:1.5px solid var(--smoke);border-radius:8px;font-family:var(--sans);font-size:13px;background:#fff;min-width:160px;}
.filter-bar input[type=text]{padding:8px 14px;border:1.5px solid var(--smoke);border-radius:8px;font-family:var(--sans);font-size:13px;width:220px;}
.tbl-wrap{overflow:auto;max-height:700px;border:1px solid var(--smoke);border-radius:8px;}
table.dt{width:100%;border-collapse:collapse;font-size:11px;}
table.dt th{font-size:9px;font-weight:700;letter-spacing:.1em;text-transform:uppercase;color:var(--dim);padding:10px 8px;text-align:left;border-bottom:2px solid var(--smoke);background:#faf9f7;position:sticky;top:0;z-index:1;}
table.dt th.num,table.dt td.num{text-align:right;} table.dt td{padding:8px;border-bottom:1px solid #f2f0ed;} table.dt tr:hover{background:#f9f7f4;}
table.dt .mono{font-family:var(--mono);font-size:10px;}
.tag{display:inline-block;font-size:9px;font-weight:700;letter-spacing:.04em;padding:2px 7px;border-radius:4px;}
.tag-di{background:#eafaf1;color:var(--emerald);} .tag-st{background:#ebf5fb;color:var(--sapphire);}
.tag-paid{background:#eafaf1;color:var(--emerald);} .tag-partial{background:#fef3cd;color:var(--amber);} .tag-unpaid{background:#fdf3f2;color:var(--ruby);}
.growth-up{color:var(--emerald);font-weight:700;} .growth-down{color:var(--ruby);font-weight:700;}
.radio-group{display:flex;gap:0;border:2px solid var(--ink);border-radius:8px;overflow:hidden;}
.radio-group label{padding:8px 16px;font-size:12px;font-weight:700;letter-spacing:.04em;cursor:pointer;background:#fff;color:var(--ink);transition:all .15s;}
.radio-group label.active{background:var(--ink);color:#fff;}
.radio-group label:hover:not(.active){background:#f0ede8;}
.radio-group input{display:none;}
#loading{text-align:center;padding:60px;color:var(--dim);font-size:14px;}
.summary-row{background:#faf9f7 !important;font-weight:700 !important;}
.summary-row td{border-top:2px solid var(--smoke) !important;}
@media(max-width:768px){.kpi-strip{flex-direction:column;}}
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
        <a href="/StockApp/ERPHome.aspx" class="nav-link">&#8592; ERP</a>
        <a href="FINLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</div>
<div class="page-header">
    <div class="page-icon">&#x1F4B3;</div>
    <div>
        <div class="page-title">INVOICE <span>OUTSTANDING</span></div>
        <div class="page-sub">Invoice-level payment tracking with FIFO receipt allocation</div>
    </div>
</div>

<div class="page" id="app">
    <div id="loading">Loading invoice data...</div>
</div>

<script>
(function(){
var app=document.getElementById('app');
var allData=null;

function fmt(v){if(v>=1e7)return'\u20B9'+(v/1e7).toFixed(1)+'Cr';if(v>=1e5)return'\u20B9'+(v/1e5).toFixed(1)+'L';if(v>=1e3)return'\u20B9'+(v/1e3).toFixed(1)+'K';return'\u20B9'+Math.round(v).toLocaleString('en-IN');}
function fmtExact(v){return'\u20B9'+v.toLocaleString('en-IN',{minimumFractionDigits:0,maximumFractionDigits:0});}

fetch('FINOutstandingAPI.ashx').then(function(r){return r.json();}).then(function(data){
    if(data.error){app.innerHTML='<div style="color:red;padding:40px;">Error: '+data.error+'</div>';return;}
    allData=data;
    renderPage();
}).catch(function(e){app.innerHTML='<div style="color:red;padding:40px;">Error: '+e.message+'</div>';});

function renderPage(){
    var s=allData.summary;
    var h='';

    // KPI strip
    h+='<div class="kpi-strip">';
    h+='<div class="kpi accent"><div class="kpi-val">'+fmt(s.totalInvoiced)+'</div><div class="kpi-label">Total Invoiced</div></div>';
    h+='<div class="kpi green"><div class="kpi-val">'+fmt(s.totalReceived)+'</div><div class="kpi-label">Total Received</div></div>';
    h+='<div class="kpi amber"><div class="kpi-val">'+fmt(s.totalOutstanding)+'</div><div class="kpi-label">Outstanding</div></div>';
    h+='<div class="kpi blue"><div class="kpi-val">'+s.totalInvoices.toLocaleString()+'</div><div class="kpi-label">Total Invoices</div></div>';
    h+='<div class="kpi" style="border-left-color:var(--ruby)"><div class="kpi-val">'+s.outstandingCount.toLocaleString()+'</div><div class="kpi-label">With Balance Due</div></div>';
    var pct=s.totalInvoiced>0?((s.totalReceived/s.totalInvoiced)*100).toFixed(1):'0';
    h+='<div class="kpi" style="border-left-color:var(--plum)"><div class="kpi-val">'+pct+'%</div><div class="kpi-label">Collection Rate</div></div>';
    h+='</div>';

    // Filters
    h+='<div class="card">';
    h+='<div class="filter-bar">';
    h+='<div class="radio-group">';
    h+='<label class="active" onclick="window._setFilter(\'all\',this)"><input type="radio" name="flt" checked/> All Invoices</label>';
    h+='<label onclick="window._setFilter(\'outstanding\',this)"><input type="radio" name="flt"/> Outstanding Only</label>';
    h+='<label onclick="window._setFilter(\'30\',this)"><input type="radio" name="flt"/> Outstanding 30+ Days</label>';
    h+='<label onclick="window._setFilter(\'60\',this)"><input type="radio" name="flt"/> Outstanding 60+ Days</label>';
    h+='<label onclick="window._setFilter(\'90\',this)"><input type="radio" name="flt"/> Outstanding 90+ Days</label>';
    h+='</div>';
    h+='</div>';
    h+='<div class="filter-bar">';
    h+='<label style="font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--dim);">Search</label>';
    h+='<input type="text" id="searchBox" placeholder="Customer name, invoice no..." oninput="window._applyFilters()"/>';
    h+='</div>';
    h+='</div>';

    // Table container
    h+='<div class="card"><div class="card-head">Invoice Details <span id="filteredCount" style="font-family:var(--sans);font-size:12px;color:var(--dim);font-weight:400;"></span></div>';
    h+='<div class="tbl-wrap" id="tblWrap"></div></div>';

    app.innerHTML=h;
    window._currentFilter='all';
    window._applyFilters();
}

window._setFilter=function(f,el){
    window._currentFilter=f;
    document.querySelectorAll('.radio-group label').forEach(function(l){l.classList.remove('active');});
    if(el)el.classList.add('active');
    window._applyFilters();
};

window._applyFilters=function(){
    var filter=window._currentFilter||'all';
    var search=(document.getElementById('searchBox')||{}).value||'';
    search=search.toLowerCase();

    var filtered=allData.invoices.filter(function(inv){
        // Filter
        if(filter==='outstanding'&&inv.balance<=0.01)return false;
        if(filter==='30'&&(inv.balance<=0.01||inv.days<30))return false;
        if(filter==='60'&&(inv.balance<=0.01||inv.days<60))return false;
        if(filter==='90'&&(inv.balance<=0.01||inv.days<90))return false;
        // Search
        if(search){
            var hay=(inv.custName+' '+inv.vch+' '+inv.city+' '+inv.state).toLowerCase();
            if(hay.indexOf(search)<0)return false;
        }
        return true;
    });

    // Group by customer
    var grouped={};
    var order=[];
    filtered.forEach(function(inv){
        var key=inv.custId||inv.custName;
        if(!grouped[key]){grouped[key]={name:inv.custName,type:inv.custType,city:inv.city,state:inv.state,invoices:[],totalInv:0,totalRec:0,totalBal:0};order.push(key);}
        grouped[key].invoices.push(inv);
        grouped[key].totalInv+=inv.invoiced;
        grouped[key].totalRec+=inv.received;
        grouped[key].totalBal+=inv.balance;
    });

    // Sort by outstanding desc
    order.sort(function(a,b){return grouped[b].totalBal-grouped[a].totalBal;});

    var h='<table class="dt"><thead><tr><th>#</th><th>Customer</th><th>Type</th><th>City</th><th>Invoice No</th><th>Date</th><th class="num">Days</th><th class="num">Invoice Amt</th><th class="num">Received</th><th class="num">Balance</th><th>Status</th></tr></thead><tbody>';

    var rowNum=0;
    var fTotalInv=0,fTotalRec=0,fTotalBal=0;
    order.forEach(function(key){
        var grp=grouped[key];
        grp.invoices.forEach(function(inv,j){
            rowNum++;
            var statusTag='';
            if(inv.balance<=0.01)statusTag='<span class="tag tag-paid">PAID</span>';
            else if(inv.received>0)statusTag='<span class="tag tag-partial">PARTIAL</span>';
            else statusTag='<span class="tag tag-unpaid">UNPAID</span>';

            var daysCls=inv.balance>0.01?(inv.days>90?'growth-down':inv.days>30?'':'growth-up'):'';
            var typeTag=inv.custType==='DI'?'<span class="tag tag-di">DI</span>':inv.custType==='ST'?'<span class="tag tag-st">ST</span>':'';

            h+='<tr>';
            h+='<td>'+rowNum+'</td>';
            h+='<td><strong>'+(j===0?inv.custName:'')+'</strong></td>';
            h+='<td>'+(j===0?typeTag:'')+'</td>';
            h+='<td>'+(j===0?inv.city:'')+'</td>';
            h+='<td class="mono">'+inv.vch+'</td>';
            h+='<td>'+inv.date+'</td>';
            h+='<td class="num '+daysCls+'">'+inv.days+'</td>';
            h+='<td class="num mono">'+fmtExact(inv.invoiced)+'</td>';
            h+='<td class="num mono" style="color:var(--emerald)">'+fmtExact(inv.received)+'</td>';
            h+='<td class="num mono" style="'+(inv.balance>0.01?'color:var(--ruby);font-weight:700':'color:var(--dim)')+'">'+fmtExact(inv.balance)+'</td>';
            h+='<td>'+statusTag+'</td>';
            h+='</tr>';

            fTotalInv+=inv.invoiced;
            fTotalRec+=inv.received;
            fTotalBal+=inv.balance;
        });

        // Customer subtotal if multiple invoices
        if(grp.invoices.length>1){
            h+='<tr class="summary-row"><td></td><td colspan="6" style="text-align:right;font-size:10px;letter-spacing:.06em;text-transform:uppercase;color:var(--dim);">'+grp.name+' Subtotal</td>';
            h+='<td class="num mono">'+fmtExact(grp.totalInv)+'</td>';
            h+='<td class="num mono" style="color:var(--emerald)">'+fmtExact(grp.totalRec)+'</td>';
            h+='<td class="num mono" style="color:var(--ruby);font-weight:700">'+fmtExact(grp.totalBal)+'</td>';
            h+='<td></td></tr>';
        }
    });

    // Grand total
    h+='<tr class="summary-row" style="background:#f0ede8 !important;"><td></td><td colspan="6" style="text-align:right;font-family:var(--display);font-size:14px;letter-spacing:.08em;">Grand Total</td>';
    h+='<td class="num mono" style="font-size:12px;font-weight:700;">'+fmtExact(fTotalInv)+'</td>';
    h+='<td class="num mono" style="font-size:12px;font-weight:700;color:var(--emerald)">'+fmtExact(fTotalRec)+'</td>';
    h+='<td class="num mono" style="font-size:12px;font-weight:700;color:var(--ruby)">'+fmtExact(fTotalBal)+'</td>';
    h+='<td></td></tr>';

    h+='</tbody></table>';

    document.getElementById('tblWrap').innerHTML=h;
    document.getElementById('filteredCount').textContent='Showing '+rowNum+' invoices across '+order.length+' customers';
};

})();
</script>
<script src="/StockApp/erp-keepalive.js"></script>
</form>
</body>
</html>
