<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SADistStockPosition.aspx.cs" Inherits="StockApp.SADistStockPosition" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width, initial-scale=1.0"/>
<title>Distributor Stock Position — Sirimiri ERP</title>
<link rel="preconnect" href="https://fonts.googleapis.com"/>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600;700&family=JetBrains+Mono:wght@400;500&display=swap" rel="stylesheet"/>
<style>
:root {
    --bg:#0f1117; --surface:#181a22; --surface-alt:#1e2130;
    --border:#2a2d3a; --border-light:#353849;
    --text:#e4e6ef; --text-muted:#8b8fa5; --text-dim:#5c6078;
    --accent:#2980b9; --accent-glow:rgba(41,128,185,0.15);
    --stock-bg:#1a2744; --stock-border:#2a4a7a; --stock-text:#5b9cf5; --stock-head:#3b7ddb;
    --payment-bg:#1a3a2a; --payment-border:#2a6a4a; --payment-text:#4ecb71; --payment-head:#34a853;
    --closing-bg:#3a2a1a; --closing-border:#6a4a2a; --closing-text:#f5a623; --closing-head:#e09422;
    --state-bg:#12141d; --state-border:#2980b9;
    --frozen-w:420px; --row-h:40px; --state-h:36px; --hdr-h:72px; --date-w:270px; --cell-w:90px;
}
*{margin:0;padding:0;box-sizing:border-box;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);color:var(--text);height:100vh;display:flex;flex-direction:column;overflow:hidden;}
#form1{display:flex;flex-direction:column;flex:1;min-height:0;overflow:hidden;}

/* ── Top Bar ── */
.topbar{background:#000;height:48px;display:flex;align-items:center;padding:0 20px;gap:16px;flex-shrink:0;border-bottom:1px solid var(--border);}
.topbar .logo-pill{background:#fff;border-radius:6px;padding:3px 10px;display:flex;align-items:center;height:32px;}
.topbar .logo-pill img{height:24px;object-fit:contain;}
.topbar .sep{color:var(--text-dim);}
.topbar .page-title{font-weight:600;font-size:14px;color:var(--text);}
.topbar .date-range{margin-left:auto;font-size:12px;color:var(--text-muted);font-family:'JetBrains Mono',monospace;}
.topbar .nav-links{display:flex;gap:12px;margin-left:12px;}
.topbar .nav-links a{color:var(--text-muted);text-decoration:none;font-size:12px;font-weight:500;padding:4px 10px;border-radius:4px;transition:all 0.2s;}
.topbar .nav-links a:hover{background:var(--surface-alt);color:var(--text);}

/* ── Controls Bar ── */
.controls-bar{background:var(--surface);border-bottom:1px solid var(--border);padding:8px 20px;display:flex;align-items:center;gap:16px;flex-shrink:0;flex-wrap:wrap;}
.legend{display:flex;gap:14px;align-items:center;}
.legend-item{display:flex;align-items:center;gap:5px;font-size:11px;font-weight:500;}
.legend-swatch{width:12px;height:12px;border-radius:3px;}
.legend-swatch.stock{background:var(--stock-bg);border:1px solid var(--stock-border);}
.legend-swatch.payment{background:var(--payment-bg);border:1px solid var(--payment-border);}
.legend-swatch.closing{background:var(--closing-bg);border:1px solid var(--closing-border);}
.label-stock{color:var(--stock-text);}
.label-payment{color:var(--payment-text);}
.label-closing{color:var(--closing-text);}
.controls-bar .info{font-size:11px;color:var(--text-dim);}

/* ── Filter Bar ── */
.filter-bar{background:var(--surface);border-bottom:1px solid var(--border);padding:8px 20px;display:flex;align-items:center;gap:16px;flex-shrink:0;flex-wrap:wrap;}
.filter-label{font-size:11px;color:var(--text-dim);font-weight:600;text-transform:uppercase;letter-spacing:.5px;}
.state-filters{display:flex;gap:6px;align-items:center;}
.state-btn{
    padding:5px 14px;border-radius:4px;font-size:11px;font-weight:600;
    border:1px solid var(--border);background:transparent;color:var(--text-muted);
    cursor:pointer;transition:all 0.15s;font-family:'DM Sans',sans-serif;
}
.state-btn:hover{border-color:var(--accent);color:var(--text);}
.state-btn.active{background:var(--accent);border-color:var(--accent);color:#fff;}
.filter-sep{width:1px;height:20px;background:var(--border);margin:0 4px;}
.filter-check{display:flex;align-items:center;gap:6px;cursor:pointer;font-size:11px;color:var(--text-muted);user-select:none;}
.filter-check input[type=checkbox]{width:14px;height:14px;accent-color:var(--accent);cursor:pointer;}
.filter-check:hover{color:var(--text);}

/* ── Loading ── */
.loading-overlay{position:absolute;top:0;left:0;right:0;bottom:0;background:rgba(15,17,23,0.85);display:flex;align-items:center;justify-content:center;z-index:100;font-size:16px;color:var(--text-muted);flex-direction:column;gap:12px;}
.loading-overlay .spinner{width:36px;height:36px;border:3px solid var(--border);border-top-color:var(--accent);border-radius:50%;animation:spin 0.8s linear infinite;}
@keyframes spin{to{transform:rotate(360deg)}}

/* ════════════════════════════════════════════
   LAYOUT: flex column fills viewport.
   Header row = fixed height. Body row = flex:1.
   Inside body: frozen-col (fixed width) + data-col (flex:1, overflow:auto).
   data-col is the SINGLE scroll master for both H and V.
   ════════════════════════════════════════════ */
.report-outer{flex:1;display:flex;flex-direction:column;overflow:hidden;position:relative;}

/* Header row */
.hdr-row{display:flex;flex-shrink:0;height:var(--hdr-h);border-bottom:2px solid var(--border);}
.hdr-frozen{width:var(--frozen-w);flex-shrink:0;background:#000;border-right:2px solid var(--accent);display:flex;flex-direction:column;z-index:30;box-shadow:4px 0 12px rgba(0,0,0,0.3);}
.hdr-frozen-top{height:38px;display:flex;align-items:center;padding:0 12px;font-size:11px;font-weight:600;color:var(--text-muted);text-transform:uppercase;letter-spacing:1px;border-bottom:1px solid var(--border);}
.hdr-frozen-sub{height:34px;display:grid;grid-template-columns:100px 1fr 80px;align-items:center;padding:0 12px;font-size:11px;font-weight:600;color:var(--text-dim);}
.hdr-scroll{flex:1;overflow:hidden;display:flex;flex-direction:column;}
.date-header-row{display:flex;height:38px;background:#000;flex-shrink:0;border-bottom:1px solid var(--border);}
.date-header-cell{width:var(--date-w);min-width:var(--date-w);display:flex;align-items:center;justify-content:center;font-size:11px;font-weight:600;font-family:'JetBrains Mono',monospace;color:var(--text);border-right:1px solid var(--border);letter-spacing:.3px;}
.date-header-cell.today{background:var(--accent-glow);color:var(--accent);}
.date-header-cell.sunday{color:var(--text-dim);background:rgba(255,255,255,0.02);}
.sub-header-row{display:flex;height:34px;background:#0a0b10;flex-shrink:0;}
.sub-header-group{width:var(--date-w);min-width:var(--date-w);display:flex;border-right:1px solid var(--border);}
.sub-header-cell{width:var(--cell-w);display:flex;align-items:center;justify-content:center;font-size:9px;font-weight:700;text-transform:uppercase;letter-spacing:.8px;}
.sub-header-cell.stock{color:var(--stock-head);border-bottom:2px solid var(--stock-head);}
.sub-header-cell.payment{color:var(--payment-head);border-bottom:2px solid var(--payment-head);}
.sub-header-cell.closing{color:var(--closing-head);border-bottom:2px solid var(--closing-head);}

/* Body row */
.body-row{flex:1;display:flex;overflow:hidden;min-height:0;} /* min-height:0 is KEY for flex child to allow shrinking */

.frozen-col{width:var(--frozen-w);flex-shrink:0;overflow:hidden;background:var(--surface);border-right:2px solid var(--accent);z-index:15;box-shadow:4px 0 12px rgba(0,0,0,0.3);}

.data-col{flex:1;overflow:auto;min-width:0;} /* min-width:0 allows flex child to shrink below content */
.data-col::-webkit-scrollbar{height:10px;width:10px;}
.data-col::-webkit-scrollbar-track{background:var(--surface);}
.data-col::-webkit-scrollbar-thumb{background:var(--border-light);border-radius:5px;}
.data-col::-webkit-scrollbar-thumb:hover{background:var(--text-dim);}
.data-col::-webkit-scrollbar-corner{background:var(--surface);}

/* dataInner gets explicit width via JS so horizontal scroll works */
#dataInner{display:block;}

/* ── Frozen Rows ── */
.frozen-row{height:var(--row-h);display:grid;grid-template-columns:100px 1fr 80px;align-items:center;padding:0 12px;border-bottom:1px solid var(--border);font-size:12px;transition:background 0.12s;}
.frozen-row:hover{background:var(--surface-alt);}
.frozen-row .city{color:var(--text-muted);font-size:11px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;}
.frozen-row .distributor{font-weight:500;color:var(--text);white-space:nowrap;overflow:hidden;text-overflow:ellipsis;}
.frozen-row .mv{font-family:'JetBrains Mono',monospace;font-size:10px;color:var(--text-dim);text-align:right;}

/* ── State Group Headers ── */
.frozen-state-row{height:var(--state-h);display:flex;align-items:center;padding:0 12px;background:var(--state-bg);border-bottom:2px solid var(--state-border);border-top:1px solid var(--border-light);gap:10px;}
.frozen-state-row .state-name{font-family:'Bebas Neue',sans-serif;font-size:16px;letter-spacing:1px;color:var(--accent);}
.frozen-state-row .state-count{font-size:11px;color:var(--text-dim);}
.frozen-state-row .state-total{margin-left:auto;font-family:'JetBrains Mono',monospace;font-size:11px;color:var(--payment-text);}
.data-state-row{height:var(--state-h);background:var(--state-bg);border-bottom:2px solid var(--state-border);border-top:1px solid var(--border-light);}

/* ── Data Rows ── */
.data-row{display:flex;height:var(--row-h);border-bottom:1px solid var(--border);transition:background 0.12s;}
.data-row:hover{background:rgba(255,255,255,0.02);}
.day-group{width:var(--date-w);min-width:var(--date-w);display:flex;border-right:1px solid var(--border);flex-shrink:0;}
.data-cell{width:var(--cell-w);min-width:var(--cell-w);display:flex;align-items:center;justify-content:center;font-size:12px;font-family:'JetBrains Mono',monospace;font-weight:500;border-right:1px solid rgba(255,255,255,0.03);flex-shrink:0;}
.data-cell.stock{background:var(--stock-bg);color:var(--stock-text);border-right-color:var(--stock-border);}
.data-cell.payment{background:var(--payment-bg);color:var(--payment-text);border-right-color:var(--payment-border);}
.data-cell.closing{background:var(--closing-bg);color:var(--closing-text);border-right-color:var(--closing-border);}
.data-cell.empty{background:transparent;color:transparent;}
.data-cell.stock.has-value{text-shadow:0 0 8px rgba(91,156,245,0.3);}
.data-cell.payment.has-value{text-shadow:0 0 8px rgba(78,203,113,0.3);}
.data-cell.closing.has-value{text-shadow:0 0 8px rgba(245,166,35,0.3);}
.day-group.sunday{opacity:.5;}
.day-group.today .data-cell{box-shadow:inset 0 0 0 1px var(--accent);}

.row-hidden{display:none !important;}
</style>
</head>
<body>
<form id="form1" runat="server">

<!-- Top Bar -->
<div class="topbar">
    <div class="logo-pill">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri"
             onerror="this.parentElement.innerHTML='<span style=color:#2980b9;font-weight:700;font-size:14px;letter-spacing:.5px>SIRIMIRI ERP</span>'"/>
    </div>
    <span class="sep">|</span>
    <span class="page-title">Sales &amp; Distribution — Distributor Stock Position</span>
    <div class="nav-links">
        <a href="SAHome.aspx">&#x2302; SA Home</a>
        <a href="ERPHome.aspx">&#x2302; ERP Home</a>
    </div>
    <span class="date-range" id="dateRangeLabel"></span>
</div>

<!-- Controls Bar -->
<div class="controls-bar">
    <div class="legend">
        <div class="legend-item"><div class="legend-swatch stock"></div><span class="label-stock">Stock Sent (Units)</span></div>
        <div class="legend-item"><div class="legend-swatch payment"></div><span class="label-payment">Payment Made (₹)</span></div>
        <div class="legend-item"><div class="legend-swatch closing"></div><span class="label-closing">Closing Stock</span></div>
    </div>
    <span class="info" id="distributorCount"></span>
</div>

<!-- Filter Bar -->
<div class="filter-bar">
    <span class="filter-label">State</span>
    <div class="state-filters" id="stateFilters">
        <button type="button" class="state-btn active" data-state="ALL" onclick="filterState(this)">All States</button>
        <button type="button" class="state-btn" data-state="Tamil Nadu" onclick="filterState(this)">Tamil Nadu</button>
        <button type="button" class="state-btn" data-state="Karnataka" onclick="filterState(this)">Karnataka</button>
        <button type="button" class="state-btn" data-state="Andhra Pradesh" onclick="filterState(this)">Andhra Pradesh</button>
        <button type="button" class="state-btn" data-state="Telangana" onclick="filterState(this)">Telangana</button>
    </div>
    <div class="filter-sep"></div>
    <label class="filter-check">
        <input type="checkbox" id="chkActiveOnly" onchange="applyFilters()"/>
        Show only Distributors / Stockists to whom Stock sent in 90 days
    </label>
</div>

<!-- Report -->
<div class="report-outer" id="reportOuter">
    <div class="loading-overlay" id="loadingOverlay">
        <div class="spinner"></div>
        <span>Loading distributor data...</span>
    </div>

    <!-- HEADER: frozen left + scrollable date headers -->
    <div class="hdr-row" id="hdrRow" style="display:none;">
        <div class="hdr-frozen">
            <div class="hdr-frozen-top">Distributor Details</div>
            <div class="hdr-frozen-sub">
                <span>City</span>
                <span>Distributor Name</span>
                <span style="text-align:right">30d Sales</span>
            </div>
        </div>
        <div class="hdr-scroll" id="hdrScroll">
            <div class="date-header-row" id="dateHeaderRow"></div>
            <div class="sub-header-row" id="subHeaderRow"></div>
        </div>
    </div>

    <!-- BODY: frozen left + scrollable data (THIS is the scroll master) -->
    <div class="body-row" id="bodyRow" style="display:none;">
        <div class="frozen-col" id="frozenCol">
            <div id="frozenInner"></div>
        </div>
        <div class="data-col" id="dataCol">
            <div id="dataInner"></div>
        </div>
    </div>
</div>

</form>

<script>
(function() {
    var DAYS = 90;
    var months = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
    var dayNames = ['Sun','Mon','Tue','Wed','Thu','Fri','Sat'];
    var today = new Date(); today.setHours(0,0,0,0);
    var dates = [];
    for (var i=DAYS-1;i>=0;i--){ var d=new Date(today); d.setDate(d.getDate()-i); dates.push(d); }

    var TOTAL_DATA_WIDTH = dates.length * 270; // 90 * 270 = 24300px

    function fmtKey(d){var m=d.getMonth()+1,dy=d.getDate();return d.getFullYear()+'-'+(m<10?'0':'')+m+'-'+(dy<10?'0':'')+dy;}
    function fmtDateFull(d){return dayNames[d.getDay()]+', '+d.getDate()+' '+months[d.getMonth()]+' '+d.getFullYear();}
    function isToday(d){return d.getTime()===today.getTime();}
    function isSunday(d){return d.getDay()===0;}
    function fmtAmount(n){
        if(n>=10000000)return'\u20B9'+(n/10000000).toFixed(1)+'Cr';
        if(n>=100000)return'\u20B9'+(n/100000).toFixed(1)+'L';
        if(n>=1000)return'\u20B9'+(n/1000).toFixed(0)+'K';
        return'\u20B9'+n;
    }
    function fmtLakh(n){
        if(n>=10000000)return'\u20B9'+(n/10000000).toFixed(1)+'Cr';
        if(n>=100000)return'\u20B9'+(n/100000).toFixed(1)+'L';
        if(n>=1000)return'\u20B9'+(n/1000).toFixed(0)+'K';
        if(n>0)return'\u20B9'+Math.round(n);
        return'';
    }

    document.getElementById('dateRangeLabel').textContent =
        fmtDateFull(dates[0])+'  \u2192  '+fmtDateFull(dates[dates.length-1]);

    var distributors=[], stockData={}, activeDistIds={};
    var currentStateFilter = 'ALL';

    function fetchJSON(url,cb){
        var xhr=new XMLHttpRequest();xhr.open('GET',url,true);
        xhr.onreadystatechange=function(){if(xhr.readyState===4){if(xhr.status===200){try{cb(null,JSON.parse(xhr.responseText));}catch(e){cb(e,null);}}else cb(new Error('HTTP '+xhr.status),null);}};
        xhr.send();
    }

    fetchJSON('SADistStockAPI.ashx?action=distributors',function(err,data){
        if(err||!data){document.getElementById('loadingOverlay').innerHTML='<span style="color:#e63946;">Failed to load distributors. Please refresh.</span>';return;}
        distributors=data;
        fetchJSON('SADistStockAPI.ashx?action=stockData&days='+DAYS,function(err2,data2){
            stockData=(!err2&&data2)?data2:{};
            for(var did in stockData){var dd=stockData[did];for(var dk in dd){if(dd[dk].s){activeDistIds[did]=true;break;}}}
            renderReport();
        });
    });

    // ── State filter ──
    window.filterState = function(btn) {
        var btns = document.querySelectorAll('.state-btn');
        btns.forEach(function(b){ b.classList.remove('active'); });
        btn.classList.add('active');
        currentStateFilter = btn.getAttribute('data-state');
        applyFilters();
    };

    // ── Combined filter (state + active-only) ──
    window.applyFilters = function() {
        var activeOnly = document.getElementById('chkActiveOnly').checked;
        var allEls = document.querySelectorAll('[data-did]');
        var stateEls = document.querySelectorAll('[data-state]');
        var stateVisible = {};
        var visibleCount = 0;

        allEls.forEach(function(el) {
            var did = el.getAttribute('data-did');
            var state = el.getAttribute('data-state-parent');
            var hideState = (currentStateFilter !== 'ALL' && state !== currentStateFilter);
            var hideActive = (activeOnly && !activeDistIds[did]);
            var hide = hideState || hideActive;
            el.classList.toggle('row-hidden', hide);
            if (!hide) {
                stateVisible[state] = (stateVisible[state]||0) + 1;
                // Count only frozen-rows (not data-rows) to avoid double counting
                if (el.classList.contains('frozen-row')) visibleCount++;
            }
        });

        stateEls.forEach(function(el) {
            var state = el.getAttribute('data-state');
            var hideState = (currentStateFilter !== 'ALL' && state !== currentStateFilter);
            var cnt = stateVisible[state] || 0;
            el.classList.toggle('row-hidden', hideState || cnt === 0);
            var cntEl = el.querySelector('.state-count');
            if (cntEl) cntEl.textContent = cnt + ' distributors';
        });

        document.getElementById('distributorCount').textContent = visibleCount + ' distributors shown';
    };

    function renderReport() {
        if(distributors.length===0){document.getElementById('loadingOverlay').innerHTML='<span>No distributors found.</span>';return;}

        document.getElementById('loadingOverlay').style.display='none';
        document.getElementById('hdrRow').style.display='';
        document.getElementById('bodyRow').style.display='';
        document.getElementById('distributorCount').textContent=distributors.length+' distributors shown';

        // ── Group by state ──
        var stateMap={}, stateOrder=[];
        for(var i=0;i<distributors.length;i++){
            var d=distributors[i];
            if(!stateMap[d.state]){stateMap[d.state]=[];stateOrder.push(d.state);}
            stateMap[d.state].push(d);
        }

        // ── Date headers ──
        var dateHeaderRow=document.getElementById('dateHeaderRow');
        var subHeaderRow=document.getElementById('subHeaderRow');
        var dhFrag=document.createDocumentFragment();
        var shFrag=document.createDocumentFragment();
        for(var i=0;i<dates.length;i++){
            var dt=dates[i];
            var cell=document.createElement('div');
            cell.className='date-header-cell';
            if(isToday(dt))cell.classList.add('today');
            if(isSunday(dt))cell.classList.add('sunday');
            cell.textContent=dayNames[dt.getDay()]+' \u00B7 '+dt.getDate()+' '+months[dt.getMonth()];
            dhFrag.appendChild(cell);
            var grp=document.createElement('div');
            grp.className='sub-header-group';
            grp.innerHTML='<div class="sub-header-cell stock">Sent</div><div class="sub-header-cell payment">Paid</div><div class="sub-header-cell closing">Close</div>';
            shFrag.appendChild(grp);
        }
        dateHeaderRow.appendChild(dhFrag);
        subHeaderRow.appendChild(shFrag);

        // ── Body rows ──
        var frozenInner=document.getElementById('frozenInner');
        var dataInner=document.getElementById('dataInner');

        // KEY FIX: set explicit width on dataInner so horizontal scroll triggers
        dataInner.style.width = TOTAL_DATA_WIDTH + 'px';

        var fFrag=document.createDocumentFragment();
        var dFrag=document.createDocumentFragment();
        var rowIdx=0;

        for(var si=0;si<stateOrder.length;si++){
            var stateName=stateOrder[si];
            var stateDists=stateMap[stateName];
            var stateTotal=0;
            for(var j=0;j<stateDists.length;j++) stateTotal+=(stateDists[j].mv||0);

            // State header — frozen
            var srf=document.createElement('div');
            srf.className='frozen-state-row';
            srf.setAttribute('data-state',stateName);
            srf.innerHTML='<span class="state-name">'+esc(stateName)+'</span><span class="state-count">'+stateDists.length+' distributors</span><span class="state-total">'+(stateTotal>0?fmtLakh(stateTotal)+' /30d':'')+'</span>';
            fFrag.appendChild(srf);

            // State header — data
            var srd=document.createElement('div');
            srd.className='data-state-row';
            srd.setAttribute('data-state',stateName);
            dFrag.appendChild(srd);

            // Distributor rows
            for(var di=0;di<stateDists.length;di++){
                var dist=stateDists[di];
                var distData=stockData[dist.id]||{};

                var fr=document.createElement('div');
                fr.className='frozen-row';
                fr.setAttribute('data-did',dist.id);
                fr.setAttribute('data-state-parent',stateName);
                fr.setAttribute('data-idx',rowIdx);
                fr.innerHTML='<span class="city">'+esc(dist.city)+'</span><span class="distributor" title="'+esc(dist.name)+'">'+esc(dist.name)+'</span><span class="mv">'+fmtLakh(dist.mv||0)+'</span>';
                fFrag.appendChild(fr);

                var dr=document.createElement('div');
                dr.className='data-row';
                dr.setAttribute('data-did',dist.id);
                dr.setAttribute('data-state-parent',stateName);
                dr.setAttribute('data-idx',rowIdx);

                for(var di2=0;di2<dates.length;di2++){
                    var dt=dates[di2];
                    var key=fmtKey(dt);
                    var entry=distData[key]||{};
                    var grp=document.createElement('div');
                    grp.className='day-group';
                    if(isToday(dt))grp.classList.add('today');
                    if(isSunday(dt))grp.classList.add('sunday');
                    var sent=entry.s||0,pay=entry.p||0,close=entry.c||0;
                    grp.innerHTML='<div class="data-cell stock '+(sent?'has-value':'empty')+'">'+(sent||'')+'</div><div class="data-cell payment '+(pay?'has-value':'empty')+'">'+(pay?fmtAmount(pay):'')+'</div><div class="data-cell closing '+(close?'has-value':'empty')+'">'+(close||'')+'</div>';
                    dr.appendChild(grp);
                }
                dFrag.appendChild(dr);
                rowIdx++;
            }
        }
        frozenInner.appendChild(fFrag);
        dataInner.appendChild(dFrag);

        // ════════════════════════════════════════════
        // SCROLL SYNC
        // dataCol is the MASTER — it has both scrollbars
        // ════════════════════════════════════════════
        var dataCol=document.getElementById('dataCol');
        var frozenCol=document.getElementById('frozenCol');
        var hdrScroll=document.getElementById('hdrScroll');

        dataCol.addEventListener('scroll',function(){
            frozenCol.scrollTop=dataCol.scrollTop;   // vertical sync
            hdrScroll.scrollLeft=dataCol.scrollLeft;  // horizontal sync
        });

        // Mouse wheel on frozen column → forward to dataCol
        frozenCol.addEventListener('wheel',function(e){
            dataCol.scrollTop+=e.deltaY;
            dataCol.scrollLeft+=e.deltaX;
            e.preventDefault();
        },{passive:false});

        // Mouse wheel on header → forward horizontal to dataCol
        hdrScroll.addEventListener('wheel',function(e){
            dataCol.scrollLeft+=e.deltaX||e.deltaY;
            e.preventDefault();
        },{passive:false});

        // ── Hover sync ──
        var fRows=frozenInner.querySelectorAll('.frozen-row');
        var dRows=dataInner.querySelectorAll('.data-row');
        fRows.forEach(function(fr,i){
            fr.addEventListener('mouseenter',function(){if(dRows[i])dRows[i].style.background='var(--surface-alt)';});
            fr.addEventListener('mouseleave',function(){if(dRows[i])dRows[i].style.background='';});
        });
        dRows.forEach(function(dr,i){
            dr.addEventListener('mouseenter',function(){if(fRows[i])fRows[i].style.background='var(--surface-alt)';});
            dr.addEventListener('mouseleave',function(){if(fRows[i])fRows[i].style.background='';});
        });

        // ── Scroll to today (rightmost) ──
        setTimeout(function(){dataCol.scrollLeft=dataCol.scrollWidth;},150);
    }

    function esc(s){var d=document.createElement('div');d.textContent=s;return d.innerHTML;}
})();
</script>
</body>
</html>
