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
    --bg: #0f1117;
    --surface: #181a22;
    --surface-alt: #1e2130;
    --border: #2a2d3a;
    --border-light: #353849;
    --text: #e4e6ef;
    --text-muted: #8b8fa5;
    --text-dim: #5c6078;
    --accent: #2980b9;
    --accent-glow: rgba(41,128,185,0.15);

    --stock-bg: #1a2744;
    --stock-border: #2a4a7a;
    --stock-text: #5b9cf5;
    --stock-head: #3b7ddb;

    --payment-bg: #1a3a2a;
    --payment-border: #2a6a4a;
    --payment-text: #4ecb71;
    --payment-head: #34a853;

    --closing-bg: #3a2a1a;
    --closing-border: #6a4a2a;
    --closing-text: #f5a623;
    --closing-head: #e09422;

    --state-bg: #12141d;
    --state-border: #2980b9;

    --frozen-width: 420px;
    --row-height: 40px;
    --state-row-height: 36px;
    --header-height: 72px; /* date + sub combined */
    --date-col-width: 270px;
    --cell-width: 90px;
}
* { margin:0; padding:0; box-sizing:border-box; }
body {
    font-family:'DM Sans',sans-serif;
    background:var(--bg);
    color:var(--text);
    height:100vh;
    display:flex;
    flex-direction:column;
    overflow:hidden;
}

/* ── Top Bar ── */
.topbar {
    background:#000;
    height:48px;
    display:flex;
    align-items:center;
    padding:0 20px;
    gap:16px;
    flex-shrink:0;
    border-bottom:1px solid var(--border);
}
.topbar .logo-pill {
    background:#fff;
    border-radius:6px;
    padding:3px 10px;
    display:flex;
    align-items:center;
    height:32px;
}
.topbar .logo-pill img { height:24px; object-fit:contain; }
.topbar .sep { color:var(--text-dim); }
.topbar .page-title { font-weight:600; font-size:14px; color:var(--text); }
.topbar .date-range {
    margin-left:auto;
    font-size:12px;
    color:var(--text-muted);
    font-family:'JetBrains Mono',monospace;
}
.topbar .nav-links { display:flex; gap:12px; margin-left:12px; }
.topbar .nav-links a {
    color:var(--text-muted); text-decoration:none; font-size:12px;
    font-weight:500; padding:4px 10px; border-radius:4px; transition:all 0.2s;
}
.topbar .nav-links a:hover { background:var(--surface-alt); color:var(--text); }

/* ── Controls Bar ── */
.controls-bar {
    background:var(--surface);
    border-bottom:1px solid var(--border);
    padding:10px 20px;
    display:flex;
    align-items:center;
    gap:24px;
    flex-shrink:0;
    flex-wrap:wrap;
}
.legend { display:flex; gap:18px; align-items:center; }
.legend-item { display:flex; align-items:center; gap:6px; font-size:12px; font-weight:500; }
.legend-swatch { width:14px; height:14px; border-radius:3px; }
.legend-swatch.stock { background:var(--stock-bg); border:1px solid var(--stock-border); }
.legend-swatch.payment { background:var(--payment-bg); border:1px solid var(--payment-border); }
.legend-swatch.closing { background:var(--closing-bg); border:1px solid var(--closing-border); }
.label-stock { color:var(--stock-text); }
.label-payment { color:var(--payment-text); }
.label-closing { color:var(--closing-text); }
.controls-bar .info { margin-left:auto; font-size:11px; color:var(--text-dim); }
.scroll-hint { font-size:11px; color:var(--text-dim); animation:pulse 2s ease-in-out infinite; }
@keyframes pulse { 0%,100%{opacity:.5} 50%{opacity:1} }

/* ── Loading ── */
.loading-overlay {
    position:absolute; top:0; left:0; right:0; bottom:0;
    background:rgba(15,17,23,0.85);
    display:flex; align-items:center; justify-content:center;
    z-index:100; font-size:16px; color:var(--text-muted);
    flex-direction:column; gap:12px;
}
.loading-overlay .spinner {
    width:36px; height:36px;
    border:3px solid var(--border);
    border-top-color:var(--accent);
    border-radius:50%;
    animation:spin 0.8s linear infinite;
}
@keyframes spin { to{transform:rotate(360deg)} }

/* ════════════════════════════════════════════
   GRID LAYOUT — 4 quadrants
   ════════════════════════════════════════════
   ┌──────────────┬────────────────────────┐
   │ frozen-hdr   │ scroll-hdr (h-scroll)  │ <- fixed height, no v-scroll
   ├──────────────┼────────────────────────┤
   │ frozen-body  │ scroll-body (h+v)      │ <- fills remaining, v-scrolls
   └──────────────┴────────────────────────┘
*/
.report-wrapper {
    flex:1;
    display:grid;
    grid-template-columns: var(--frozen-width) 1fr;
    grid-template-rows: var(--header-height) 1fr;
    position:relative;
    overflow:hidden;
}

/* ── Top-Left: Frozen Header ── */
.frozen-hdr {
    grid-column:1; grid-row:1;
    background:#000;
    border-right:2px solid var(--accent);
    border-bottom:2px solid var(--border);
    display:flex;
    flex-direction:column;
    z-index:30;
    box-shadow:4px 0 16px rgba(0,0,0,0.4);
}
.frozen-hdr-top {
    height:38px;
    display:flex;
    align-items:center;
    padding:0 12px;
    font-size:11px;
    font-weight:600;
    color:var(--text-muted);
    text-transform:uppercase;
    letter-spacing:1px;
    border-bottom:1px solid var(--border);
}
.frozen-hdr-sub {
    height:34px;
    display:grid;
    grid-template-columns:100px 1fr 80px;
    align-items:center;
    padding:0 12px;
    font-size:11px;
    font-weight:600;
    color:var(--text-dim);
}

/* ── Top-Right: Scrollable Date Headers ── */
.scroll-hdr {
    grid-column:2; grid-row:1;
    overflow:hidden; /* controlled by JS sync */
    display:flex;
    flex-direction:column;
    border-bottom:2px solid var(--border);
    z-index:20;
}
.date-header-row {
    display:flex;
    height:38px;
    background:#000;
    flex-shrink:0;
    border-bottom:1px solid var(--border);
}
.date-header-cell {
    width:var(--date-col-width); min-width:var(--date-col-width);
    display:flex; align-items:center; justify-content:center;
    font-size:11px; font-weight:600;
    font-family:'JetBrains Mono',monospace;
    color:var(--text);
    border-right:1px solid var(--border);
    letter-spacing:.3px;
}
.date-header-cell.today { background:var(--accent-glow); color:var(--accent); }
.date-header-cell.sunday { color:var(--text-dim); background:rgba(255,255,255,0.02); }
.sub-header-row {
    display:flex;
    height:34px;
    background:#0a0b10;
    flex-shrink:0;
}
.sub-header-group {
    width:var(--date-col-width); min-width:var(--date-col-width);
    display:flex;
    border-right:1px solid var(--border);
}
.sub-header-cell {
    width:var(--cell-width);
    display:flex; align-items:center; justify-content:center;
    font-size:9px; font-weight:700;
    text-transform:uppercase; letter-spacing:.8px;
}
.sub-header-cell.stock { color:var(--stock-head); border-bottom:2px solid var(--stock-head); }
.sub-header-cell.payment { color:var(--payment-head); border-bottom:2px solid var(--payment-head); }
.sub-header-cell.closing { color:var(--closing-head); border-bottom:2px solid var(--closing-head); }

/* ── Bottom-Left: Frozen Body ── */
.frozen-body {
    grid-column:1; grid-row:2;
    overflow:hidden; /* v-scroll synced with scroll-body */
    background:var(--surface);
    border-right:2px solid var(--accent);
    z-index:15;
    box-shadow:4px 0 16px rgba(0,0,0,0.4);
}
.frozen-body-inner { /* sized by content */ }

/* ── Bottom-Right: Scrollable Body ── */
.scroll-body {
    grid-column:2; grid-row:2;
    overflow:auto; /* both h and v scroll */
}
.scroll-body::-webkit-scrollbar { height:10px; width:10px; }
.scroll-body::-webkit-scrollbar-track { background:var(--surface); }
.scroll-body::-webkit-scrollbar-thumb { background:var(--border-light); border-radius:5px; }
.scroll-body::-webkit-scrollbar-thumb:hover { background:var(--text-dim); }
.scroll-body::-webkit-scrollbar-corner { background:var(--surface); }

/* ── Frozen Rows ── */
.frozen-row {
    height:var(--row-height);
    display:grid;
    grid-template-columns:100px 1fr 80px;
    align-items:center;
    padding:0 12px;
    border-bottom:1px solid var(--border);
    font-size:12px;
    transition:background 0.15s;
}
.frozen-row:hover { background:var(--surface-alt); }
.frozen-row .city {
    color:var(--text-muted); font-size:11px;
    white-space:nowrap; overflow:hidden; text-overflow:ellipsis;
}
.frozen-row .distributor {
    font-weight:500; color:var(--text);
    white-space:nowrap; overflow:hidden; text-overflow:ellipsis;
}
.frozen-row .mv {
    font-family:'JetBrains Mono',monospace;
    font-size:10px; color:var(--text-dim);
    text-align:right;
}

/* ── State Group Header (frozen side) ── */
.frozen-state-row {
    height:var(--state-row-height);
    display:flex;
    align-items:center;
    padding:0 12px;
    background:var(--state-bg);
    border-bottom:2px solid var(--state-border);
    border-top:1px solid var(--border-light);
    gap:10px;
}
.frozen-state-row .state-name {
    font-family:'Bebas Neue',sans-serif;
    font-size:16px;
    letter-spacing:1px;
    color:var(--accent);
}
.frozen-state-row .state-count {
    font-size:11px;
    color:var(--text-dim);
}
.frozen-state-row .state-total {
    margin-left:auto;
    font-family:'JetBrains Mono',monospace;
    font-size:11px;
    color:var(--payment-text);
}

/* ── State Group Header (data side) ── */
.data-state-row {
    height:var(--state-row-height);
    display:flex;
    background:var(--state-bg);
    border-bottom:2px solid var(--state-border);
    border-top:1px solid var(--border-light);
}

/* ── Data Rows ── */
.data-row {
    display:flex;
    height:var(--row-height);
    border-bottom:1px solid var(--border);
    transition:background 0.15s;
}
.data-row:hover { background:rgba(255,255,255,0.02); }
.day-group {
    width:var(--date-col-width); min-width:var(--date-col-width);
    display:flex;
    border-right:1px solid var(--border);
}
.data-cell {
    width:var(--cell-width);
    display:flex; align-items:center; justify-content:center;
    font-size:12px;
    font-family:'JetBrains Mono',monospace;
    font-weight:500;
    border-right:1px solid rgba(255,255,255,0.03);
}
.data-cell.stock { background:var(--stock-bg); color:var(--stock-text); border-right-color:var(--stock-border); }
.data-cell.payment { background:var(--payment-bg); color:var(--payment-text); border-right-color:var(--payment-border); }
.data-cell.closing { background:var(--closing-bg); color:var(--closing-text); border-right-color:var(--closing-border); }
.data-cell.empty { background:transparent; color:transparent; }
.data-cell.stock.has-value { text-shadow:0 0 8px rgba(91,156,245,0.3); }
.data-cell.payment.has-value { text-shadow:0 0 8px rgba(78,203,113,0.3); }
.data-cell.closing.has-value { text-shadow:0 0 8px rgba(245,166,35,0.3); }
.day-group.sunday { opacity:.5; }
.day-group.today .data-cell { box-shadow:inset 0 0 0 1px var(--accent); }
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
        <div class="legend-item">
            <div class="legend-swatch stock"></div>
            <span class="label-stock">Stock Sent (Units)</span>
        </div>
        <div class="legend-item">
            <div class="legend-swatch payment"></div>
            <span class="label-payment">Payment Made (₹)</span>
        </div>
        <div class="legend-item">
            <div class="legend-swatch closing"></div>
            <span class="label-closing">Closing Stock</span>
        </div>
    </div>
    <span class="scroll-hint">← Scroll horizontally · Scroll vertically to see all distributors →</span>
    <span class="info" id="distributorCount"></span>
</div>

<!-- Report Grid -->
<div class="report-wrapper" id="reportWrapper">
    <!-- Loading -->
    <div class="loading-overlay" id="loadingOverlay" style="grid-column:1/-1;grid-row:1/-1;">
        <div class="spinner"></div>
        <span>Loading distributor data...</span>
    </div>

    <!-- Top-Left: Frozen Header -->
    <div class="frozen-hdr" id="frozenHdr" style="display:none;">
        <div class="frozen-hdr-top">Distributor Details</div>
        <div class="frozen-hdr-sub">
            <span>City</span>
            <span>Distributor Name</span>
            <span style="text-align:right">30d Sales</span>
        </div>
    </div>

    <!-- Top-Right: Scrollable Date Headers -->
    <div class="scroll-hdr" id="scrollHdr" style="display:none;">
        <div class="date-header-row" id="dateHeaderRow"></div>
        <div class="sub-header-row" id="subHeaderRow"></div>
    </div>

    <!-- Bottom-Left: Frozen Body -->
    <div class="frozen-body" id="frozenBody" style="display:none;">
        <div class="frozen-body-inner" id="frozenBodyInner"></div>
    </div>

    <!-- Bottom-Right: Scrollable Body -->
    <div class="scroll-body" id="scrollBody" style="display:none;">
        <div id="dataBody"></div>
    </div>
</div>

</form>

<script>
(function() {
    var DAYS = 90;
    var months = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
    var dayNames = ['Sun','Mon','Tue','Wed','Thu','Fri','Sat'];

    var today = new Date();
    today.setHours(0,0,0,0);
    var dates = [];
    for (var i = DAYS - 1; i >= 0; i--) {
        var d = new Date(today);
        d.setDate(d.getDate() - i);
        dates.push(d);
    }

    function fmtKey(d) {
        var m = d.getMonth() + 1, dy = d.getDate();
        return d.getFullYear() + '-' + (m < 10 ? '0' : '') + m + '-' + (dy < 10 ? '0' : '') + dy;
    }
    function fmtDateFull(d) { return dayNames[d.getDay()] + ', ' + d.getDate() + ' ' + months[d.getMonth()] + ' ' + d.getFullYear(); }
    function isToday(d) { return d.getTime() === today.getTime(); }
    function isSunday(d) { return d.getDay() === 0; }
    function fmtAmount(n) {
        if (n >= 10000000) return '\u20B9' + (n / 10000000).toFixed(1) + 'Cr';
        if (n >= 100000) return '\u20B9' + (n / 100000).toFixed(1) + 'L';
        if (n >= 1000) return '\u20B9' + (n / 1000).toFixed(0) + 'K';
        return '\u20B9' + n;
    }
    function fmtLakh(n) {
        if (n >= 10000000) return '\u20B9' + (n / 10000000).toFixed(1) + 'Cr';
        if (n >= 100000) return '\u20B9' + (n / 100000).toFixed(1) + 'L';
        if (n >= 1000) return '\u20B9' + (n / 1000).toFixed(0) + 'K';
        if (n > 0) return '\u20B9' + Math.round(n);
        return '';
    }

    document.getElementById('dateRangeLabel').textContent =
        fmtDateFull(dates[0]) + '  \u2192  ' + fmtDateFull(dates[dates.length - 1]);

    var distributors = [];
    var stockData = {};

    function fetchJSON(url, cb) {
        var xhr = new XMLHttpRequest();
        xhr.open('GET', url, true);
        xhr.onreadystatechange = function() {
            if (xhr.readyState === 4) {
                if (xhr.status === 200) {
                    try { cb(null, JSON.parse(xhr.responseText)); }
                    catch(e) { cb(e, null); }
                } else { cb(new Error('HTTP ' + xhr.status), null); }
            }
        };
        xhr.send();
    }

    fetchJSON('SADistStockAPI.ashx?action=distributors', function(err, data) {
        if (err || !data) {
            document.getElementById('loadingOverlay').innerHTML =
                '<span style="color:#e63946;">Failed to load distributors. Please refresh.</span>';
            return;
        }
        distributors = data;

        fetchJSON('SADistStockAPI.ashx?action=stockData&days=' + DAYS, function(err2, data2) {
            stockData = (!err2 && data2) ? data2 : {};
            renderReport();
        });
    });

    function renderReport() {
        if (distributors.length === 0) {
            document.getElementById('loadingOverlay').innerHTML = '<span>No distributors found.</span>';
            return;
        }

        // ── Group by state, sort by monthly sales desc within each state ──
        // API already returns sorted by state then MonthlySales DESC,
        // but let's group explicitly for state headers
        var stateMap = {};  // state -> [distributors]
        var stateOrder = [];
        for (var i = 0; i < distributors.length; i++) {
            var d = distributors[i];
            if (!stateMap[d.state]) {
                stateMap[d.state] = [];
                stateOrder.push(d.state);
            }
            stateMap[d.state].push(d);
        }

        // Show UI
        document.getElementById('loadingOverlay').style.display = 'none';
        document.getElementById('frozenHdr').style.display = '';
        document.getElementById('scrollHdr').style.display = '';
        document.getElementById('frozenBody').style.display = '';
        document.getElementById('scrollBody').style.display = '';
        document.getElementById('distributorCount').textContent =
            distributors.length + ' distributors \u00B7 ' + stateOrder.length + ' states';

        // ── Build date headers (top-right) ──
        var dateHeaderRow = document.getElementById('dateHeaderRow');
        var subHeaderRow = document.getElementById('subHeaderRow');
        var dhFrag = document.createDocumentFragment();
        var shFrag = document.createDocumentFragment();

        for (var i = 0; i < dates.length; i++) {
            var dt = dates[i];
            var cell = document.createElement('div');
            cell.className = 'date-header-cell';
            if (isToday(dt)) cell.classList.add('today');
            if (isSunday(dt)) cell.classList.add('sunday');
            cell.textContent = dayNames[dt.getDay()] + ' \u00B7 ' + dt.getDate() + ' ' + months[dt.getMonth()];
            dhFrag.appendChild(cell);

            var group = document.createElement('div');
            group.className = 'sub-header-group';
            group.innerHTML =
                '<div class="sub-header-cell stock">Sent</div>' +
                '<div class="sub-header-cell payment">Paid</div>' +
                '<div class="sub-header-cell closing">Close</div>';
            shFrag.appendChild(group);
        }
        dateHeaderRow.appendChild(dhFrag);
        subHeaderRow.appendChild(shFrag);

        // ── Build frozen body + data body (synced rows) ──
        var frozenInner = document.getElementById('frozenBodyInner');
        var dataBody = document.getElementById('dataBody');
        var fFrag = document.createDocumentFragment();
        var dFrag = document.createDocumentFragment();
        var rowIndex = 0;
        var totalDataWidth = dates.length * 270; // for state row spanning

        for (var si = 0; si < stateOrder.length; si++) {
            var stateName = stateOrder[si];
            var stateDists = stateMap[stateName];
            var stateTotal = 0;
            for (var j = 0; j < stateDists.length; j++) stateTotal += (stateDists[j].mv || 0);

            // ── State header row (frozen side) ──
            var stateRowF = document.createElement('div');
            stateRowF.className = 'frozen-state-row';
            stateRowF.innerHTML =
                '<span class="state-name">' + esc(stateName) + '</span>' +
                '<span class="state-count">' + stateDists.length + ' distributors</span>' +
                '<span class="state-total">' + (stateTotal > 0 ? fmtLakh(stateTotal) + ' /30d' : '') + '</span>';
            fFrag.appendChild(stateRowF);

            // ── State header row (data side) — empty colored bar ──
            var stateRowD = document.createElement('div');
            stateRowD.className = 'data-state-row';
            stateRowD.style.width = totalDataWidth + 'px';
            dFrag.appendChild(stateRowD);

            // ── Distributor rows ──
            for (var di = 0; di < stateDists.length; di++) {
                var dist = stateDists[di];
                var distData = stockData[dist.id] || {};

                // Frozen row
                var fRow = document.createElement('div');
                fRow.className = 'frozen-row';
                fRow.dataset.idx = rowIndex;
                fRow.innerHTML =
                    '<span class="city">' + esc(dist.city) + '</span>' +
                    '<span class="distributor" title="' + esc(dist.name) + '">' + esc(dist.name) + '</span>' +
                    '<span class="mv">' + fmtLakh(dist.mv || 0) + '</span>';
                fFrag.appendChild(fRow);

                // Data row
                var dRow = document.createElement('div');
                dRow.className = 'data-row';
                dRow.dataset.idx = rowIndex;

                for (var di2 = 0; di2 < dates.length; di2++) {
                    var dt = dates[di2];
                    var key = fmtKey(dt);
                    var entry = distData[key] || {};

                    var group = document.createElement('div');
                    group.className = 'day-group';
                    if (isToday(dt)) group.classList.add('today');
                    if (isSunday(dt)) group.classList.add('sunday');

                    var sent = entry.s || 0;
                    var pay = entry.p || 0;
                    var close = entry.c || 0;

                    group.innerHTML =
                        '<div class="data-cell stock ' + (sent ? 'has-value' : 'empty') + '">' + (sent || '') + '</div>' +
                        '<div class="data-cell payment ' + (pay ? 'has-value' : 'empty') + '">' + (pay ? fmtAmount(pay) : '') + '</div>' +
                        '<div class="data-cell closing ' + (close ? 'has-value' : 'empty') + '">' + (close || '') + '</div>';
                    dRow.appendChild(group);
                }
                dFrag.appendChild(dRow);
                rowIndex++;
            }
        }

        frozenInner.appendChild(fFrag);
        dataBody.appendChild(dFrag);

        // ── Scroll Sync ──
        var scrollBody = document.getElementById('scrollBody');
        var scrollHdr = document.getElementById('scrollHdr');
        var frozenBody = document.getElementById('frozenBody');

        // Horizontal: scrollBody drives scrollHdr
        scrollBody.addEventListener('scroll', function() {
            scrollHdr.scrollLeft = scrollBody.scrollLeft;
            frozenBody.scrollTop = scrollBody.scrollTop;
        });

        // Vertical: frozenBody mouse-wheel should scroll scrollBody
        frozenBody.addEventListener('wheel', function(e) {
            scrollBody.scrollTop += e.deltaY;
            scrollBody.scrollLeft += e.deltaX;
            e.preventDefault();
        }, { passive: false });

        // ── Hover sync ──
        var frozenRows = frozenInner.querySelectorAll('.frozen-row');
        var dataRows = dataBody.querySelectorAll('.data-row');
        frozenRows.forEach(function(fRow, i) {
            fRow.addEventListener('mouseenter', function() { if (dataRows[i]) dataRows[i].style.background = 'var(--surface-alt)'; });
            fRow.addEventListener('mouseleave', function() { if (dataRows[i]) dataRows[i].style.background = ''; });
        });
        dataRows.forEach(function(dRow, i) {
            dRow.addEventListener('mouseenter', function() { if (frozenRows[i]) frozenRows[i].style.background = 'var(--surface-alt)'; });
            dRow.addEventListener('mouseleave', function() { if (frozenRows[i]) frozenRows[i].style.background = ''; });
        });

        // ── Scroll to today (rightmost) ──
        setTimeout(function() { scrollBody.scrollLeft = scrollBody.scrollWidth; }, 100);
    }

    function esc(s) { var d = document.createElement('div'); d.textContent = s; return d.innerHTML; }
})();
</script>
</body>
</html>
