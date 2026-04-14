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

    --frozen-width: 420px;
    --row-height: 42px;
    --header-date-height: 38px;
    --header-sub-height: 34px;
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
.topbar .logo-pill img {
    height:24px;
    object-fit:contain;
}
.topbar .sep { color:var(--text-dim); }
.topbar .page-title {
    font-weight:600;
    font-size:14px;
    color:var(--text);
}
.topbar .date-range {
    margin-left:auto;
    font-size:12px;
    color:var(--text-muted);
    font-family:'JetBrains Mono',monospace;
}
.topbar .nav-links { display:flex; gap:12px; margin-left:12px; }
.topbar .nav-links a {
    color:var(--text-muted);
    text-decoration:none;
    font-size:12px;
    font-weight:500;
    padding:4px 10px;
    border-radius:4px;
    transition:all 0.2s;
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

/* ── Report Container ── */
.report-wrapper {
    flex:1;
    display:flex;
    position:relative;
    overflow:hidden;
}

/* ── Frozen Left Panel ── */
.frozen-panel {
    width:var(--frozen-width);
    flex-shrink:0;
    display:flex;
    flex-direction:column;
    background:var(--surface);
    z-index:10;
    border-right:2px solid var(--accent);
    box-shadow:4px 0 16px rgba(0,0,0,0.4);
}
.frozen-header {
    height:calc(var(--header-date-height) + var(--header-sub-height));
    background:#000;
    display:flex;
    flex-direction:column;
    border-bottom:1px solid var(--border);
    flex-shrink:0;
}
.frozen-header-top {
    height:var(--header-date-height);
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
.frozen-header-sub {
    height:var(--header-sub-height);
    display:grid;
    grid-template-columns:80px 100px 1fr;
    align-items:center;
    padding:0 12px;
    font-size:11px;
    font-weight:600;
    color:var(--text-dim);
}
.frozen-body { flex:1; overflow-y:auto; overflow-x:hidden; }
.frozen-body::-webkit-scrollbar { display:none; }
.frozen-row {
    height:var(--row-height);
    display:grid;
    grid-template-columns:80px 100px 1fr;
    align-items:center;
    padding:0 12px;
    border-bottom:1px solid var(--border);
    font-size:12px;
    transition:background 0.15s;
}
.frozen-row:hover { background:var(--surface-alt); }
.frozen-row .state {
    font-weight:600; color:var(--text-muted); font-size:10px;
    text-transform:uppercase; letter-spacing:.5px;
    white-space:nowrap; overflow:hidden; text-overflow:ellipsis;
}
.frozen-row .city {
    color:var(--text-muted); font-size:11px;
    white-space:nowrap; overflow:hidden; text-overflow:ellipsis;
}
.frozen-row .distributor {
    font-weight:500; color:var(--text);
    white-space:nowrap; overflow:hidden; text-overflow:ellipsis;
}

/* ── Scrollable Data Panel ── */
.scroll-panel {
    flex:1;
    overflow-x:auto;
    overflow-y:auto;
    display:flex;
    flex-direction:column;
}
.scroll-panel::-webkit-scrollbar { height:10px; width:10px; }
.scroll-panel::-webkit-scrollbar-track { background:var(--surface); }
.scroll-panel::-webkit-scrollbar-thumb { background:var(--border-light); border-radius:5px; }
.scroll-panel::-webkit-scrollbar-thumb:hover { background:var(--text-dim); }
.scroll-panel::-webkit-scrollbar-corner { background:var(--surface); }

/* ── Date Headers ── */
.date-header-row {
    display:flex;
    height:var(--header-date-height);
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

/* ── Sub-headers ── */
.sub-header-row {
    display:flex;
    height:var(--header-sub-height);
    background:#0a0b10;
    flex-shrink:0;
    border-bottom:2px solid var(--border);
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

/* ── Data Rows ── */
.data-body { flex:1; }
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

/* ── No data ── */
.no-data {
    display:flex; align-items:center; justify-content:center;
    flex:1; color:var(--text-dim); font-size:14px;
}
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
    <span class="scroll-hint">← Scroll horizontally to view all 90 days →</span>
    <span class="info" id="distributorCount"></span>
</div>

<!-- Report -->
<div class="report-wrapper" id="reportWrapper">
    <!-- Loading -->
    <div class="loading-overlay" id="loadingOverlay">
        <div class="spinner"></div>
        <span>Loading distributor data...</span>
    </div>

    <!-- Frozen Left -->
    <div class="frozen-panel" id="frozenPanel" style="display:none;">
        <div class="frozen-header">
            <div class="frozen-header-top">Distributor Details</div>
            <div class="frozen-header-sub">
                <span>State</span>
                <span>City</span>
                <span>Distributor Name</span>
            </div>
        </div>
        <div class="frozen-body" id="frozenBody"></div>
    </div>

    <!-- Scrollable Right -->
    <div class="scroll-panel" id="scrollPanel" style="display:none;">
        <div class="date-header-row" id="dateHeaderRow"></div>
        <div class="sub-header-row" id="subHeaderRow"></div>
        <div class="data-body" id="dataBody"></div>
    </div>
</div>

</form>

<script>
(function() {
    var DAYS = 90;
    var months = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
    var dayNames = ['Sun','Mon','Tue','Wed','Thu','Fri','Sat'];

    // ── Generate date array ──
    var today = new Date();
    today.setHours(0,0,0,0);
    var dates = [];
    for (var i = DAYS - 1; i >= 0; i--) {
        var d = new Date(today);
        d.setDate(d.getDate() - i);
        dates.push(d);
    }

    function fmtKey(d) {
        var m = d.getMonth() + 1;
        var dy = d.getDate();
        return d.getFullYear() + '-' + (m < 10 ? '0' : '') + m + '-' + (dy < 10 ? '0' : '') + dy;
    }
    function fmtDate(d) { return d.getDate() + ' ' + months[d.getMonth()]; }
    function fmtDateFull(d) { return dayNames[d.getDay()] + ', ' + d.getDate() + ' ' + months[d.getMonth()] + ' ' + d.getFullYear(); }
    function isToday(d) { return d.getTime() === today.getTime(); }
    function isSunday(d) { return d.getDay() === 0; }
    function fmtAmount(n) {
        if (n >= 100000) return '\u20B9' + (n / 100000).toFixed(1) + 'L';
        if (n >= 1000) return '\u20B9' + (n / 1000).toFixed(0) + 'K';
        return '\u20B9' + n;
    }

    // ── Set date range label ──
    document.getElementById('dateRangeLabel').textContent =
        fmtDateFull(dates[0]) + '  \u2192  ' + fmtDateFull(dates[dates.length - 1]);

    // ── Fetch data ──
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

    // Load distributors, then stock data
    fetchJSON('SADistStockAPI.ashx?action=distributors', function(err, data) {
        if (err || !data) {
            document.getElementById('loadingOverlay').innerHTML =
                '<span style="color:#e63946;">Failed to load distributors. Please refresh.</span>';
            return;
        }
        distributors = data;

        fetchJSON('SADistStockAPI.ashx?action=stockData&days=' + DAYS, function(err2, data2) {
            if (err2) {
                stockData = {};
            } else {
                stockData = data2 || {};
            }
            renderReport();
        });
    });

    function renderReport() {
        if (distributors.length === 0) {
            document.getElementById('loadingOverlay').innerHTML =
                '<span>No distributors found.</span>';
            return;
        }

        document.getElementById('loadingOverlay').style.display = 'none';
        document.getElementById('frozenPanel').style.display = '';
        document.getElementById('scrollPanel').style.display = '';
        document.getElementById('distributorCount').textContent = distributors.length + ' distributors';

        // ── Build frozen rows ──
        var frozenBody = document.getElementById('frozenBody');
        var frag = document.createDocumentFragment();
        for (var di = 0; di < distributors.length; di++) {
            var dist = distributors[di];
            var row = document.createElement('div');
            row.className = 'frozen-row';
            row.dataset.idx = di;
            row.innerHTML =
                '<span class="state">' + esc(dist.state) + '</span>' +
                '<span class="city">' + esc(dist.city) + '</span>' +
                '<span class="distributor" title="' + esc(dist.name) + '">' + esc(dist.name) + '</span>';
            frag.appendChild(row);
        }
        frozenBody.appendChild(frag);

        // ── Build date headers ──
        var dateHeaderRow = document.getElementById('dateHeaderRow');
        var subHeaderRow = document.getElementById('subHeaderRow');
        var dhFrag = document.createDocumentFragment();
        var shFrag = document.createDocumentFragment();

        for (var i = 0; i < dates.length; i++) {
            var d = dates[i];
            var cell = document.createElement('div');
            cell.className = 'date-header-cell';
            if (isToday(d)) cell.classList.add('today');
            if (isSunday(d)) cell.classList.add('sunday');
            cell.textContent = dayNames[d.getDay()] + ' \u00B7 ' + d.getDate() + ' ' + months[d.getMonth()];
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

        // ── Build data rows ──
        var dataBody = document.getElementById('dataBody');
        var dFrag = document.createDocumentFragment();

        for (var di = 0; di < distributors.length; di++) {
            var dist = distributors[di];
            var distData = stockData[dist.id] || {};
            var row = document.createElement('div');
            row.className = 'data-row';
            row.dataset.idx = di;

            for (var i = 0; i < dates.length; i++) {
                var d = dates[i];
                var key = fmtKey(d);
                var entry = distData[key] || {};

                var group = document.createElement('div');
                group.className = 'day-group';
                if (isToday(d)) group.classList.add('today');
                if (isSunday(d)) group.classList.add('sunday');

                var sent = entry.s || 0;
                var pay = entry.p || 0;
                var close = entry.c || 0;

                group.innerHTML =
                    '<div class="data-cell stock ' + (sent ? 'has-value' : 'empty') + '">' + (sent || '') + '</div>' +
                    '<div class="data-cell payment ' + (pay ? 'has-value' : 'empty') + '">' + (pay ? fmtAmount(pay) : '') + '</div>' +
                    '<div class="data-cell closing ' + (close ? 'has-value' : 'empty') + '">' + (close || '') + '</div>';
                row.appendChild(group);
            }
            dFrag.appendChild(row);
        }
        dataBody.appendChild(dFrag);

        // ── Sync vertical scroll ──
        var scrollPanel = document.getElementById('scrollPanel');
        frozenBody.addEventListener('scroll', function() { scrollPanel.scrollTop = frozenBody.scrollTop; });
        scrollPanel.addEventListener('scroll', function() { frozenBody.scrollTop = scrollPanel.scrollTop; });

        // ── Hover sync ──
        var frozenRows = frozenBody.querySelectorAll('.frozen-row');
        var dataRows = dataBody.querySelectorAll('.data-row');
        frozenRows.forEach(function(fRow, i) {
            fRow.addEventListener('mouseenter', function() { if (dataRows[i]) dataRows[i].style.background = 'var(--surface-alt)'; });
            fRow.addEventListener('mouseleave', function() { if (dataRows[i]) dataRows[i].style.background = ''; });
        });
        dataRows.forEach(function(dRow, i) {
            dRow.addEventListener('mouseenter', function() { if (frozenRows[i]) frozenRows[i].style.background = 'var(--surface-alt)'; });
            dRow.addEventListener('mouseleave', function() { if (frozenRows[i]) frozenRows[i].style.background = ''; });
        });

        // ── Scroll to today (most recent) ──
        setTimeout(function() { scrollPanel.scrollLeft = scrollPanel.scrollWidth; }, 100);
    }

    function esc(s) { var d = document.createElement('div'); d.textContent = s; return d.innerHTML; }
})();
</script>
</body>
</html>
