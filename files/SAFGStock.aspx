<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SAFGStock.aspx.cs" Inherits="StockApp.SAFGStock" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width, initial-scale=1.0"/>
<title>FG Stock Level — Sirimiri ERP</title>
<link rel="preconnect" href="https://fonts.googleapis.com"/>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600;700&family=JetBrains+Mono:wght@400;500&display=swap" rel="stylesheet"/>
<style>
:root{--accent:#2980b9;--accent-dark:#2471a3;--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--text:#1a1a1a;--muted:#666;--radius:12px}
*{box-sizing:border-box;margin:0;padding:0}
body{background:var(--bg);font-family:'DM Sans',sans-serif;min-height:100vh}
nav{background:#1a1a1a;display:flex;align-items:center;padding:0 24px;height:52px;gap:12px}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center}
.nav-logo img{height:26px;object-fit:contain}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.08em}
.nav-right{margin-left:auto;display:flex;gap:16px}
.nav-link{color:#fff;font-size:12px;font-weight:600;text-decoration:none;opacity:.85;padding:6px 12px;border-radius:4px}
.nav-link:hover{opacity:1;background:rgba(255,255,255,.1)}
.page-wrap{max-width:900px;margin:0 auto;padding:24px 16px 60px}
.page-header{display:flex;align-items:center;justify-content:space-between;margin-bottom:20px;flex-wrap:wrap;gap:12px}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:28px;letter-spacing:.06em;color:var(--text)}
.btn-pdf{
    padding:8px 20px;background:#e74c3c;color:#fff;border:none;border-radius:8px;
    font-size:13px;font-weight:600;cursor:pointer;text-decoration:none;display:inline-flex;align-items:center;gap:6px;
}
.btn-pdf:hover{background:#c0392b}
.summary-strip{display:flex;gap:16px;margin-bottom:20px;flex-wrap:wrap}
.summary-card{background:var(--surface);border-radius:var(--radius);padding:14px 20px;flex:1;min-width:150px;box-shadow:0 2px 8px rgba(0,0,0,.06);border-left:4px solid var(--accent)}
.summary-card .label{font-size:10px;font-weight:600;text-transform:uppercase;letter-spacing:.08em;color:var(--muted);margin-bottom:4px}
.summary-card .val{font-family:'JetBrains Mono',monospace;font-size:22px;font-weight:700;color:var(--accent)}
.card{background:var(--surface);border-radius:var(--radius);box-shadow:0 2px 12px rgba(0,0,0,.06);overflow:hidden}
table{width:100%;border-collapse:collapse}
th{font-size:10px;font-weight:600;letter-spacing:.06em;text-transform:uppercase;color:var(--muted);padding:10px 14px;text-align:left;border-bottom:2px solid var(--border);background:#fafafa}
td{padding:10px 14px;border-bottom:1px solid var(--border);font-size:13px}
tr:last-child td{border-bottom:none}
tr:hover td{background:#f8f9fb}
.code{font-family:'JetBrains Mono',monospace;font-size:11px;color:var(--muted)}
.product-name{font-weight:500;color:var(--text)}
.num{text-align:right;font-family:'JetBrains Mono',monospace;font-weight:600}
.avail{color:var(--accent);font-size:15px}
.avail.negative{color:#e74c3c}
.avail.zero{color:#ccc}
.loading{text-align:center;padding:40px;color:var(--muted)}
.no-data{text-align:center;padding:40px;color:var(--muted);font-size:14px}
.timestamp{font-size:11px;color:var(--muted);margin-top:12px;text-align:right}
</style>
</head>
<body>
<form id="form1" runat="server">
<nav>
    <div class="nav-logo">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri"
             onerror="this.parentElement.innerHTML='<span style=color:#2980b9;font-weight:700>SIRIMIRI</span>'"/>
    </div>
    <span class="nav-title">Finished Goods Stock Level</span>
    <div class="nav-right">
        <a href="SAHome.aspx" class="nav-link">&#x2302; SA Home</a>
        <a href="ERPHome.aspx" class="nav-link">&#x2302; ERP Home</a>
    </div>
</nav>

<div class="page-wrap">
    <div class="page-header">
        <div class="page-title">FINISHED GOODS STOCK LEVEL</div>
        <a class="btn-pdf" href="SAFGStockAPI.ashx?action=pdf" target="_blank">&#x1F4C4; Download PDF</a>
    </div>

    <div class="summary-strip" id="summaryStrip" style="display:none;">
        <div class="summary-card"><div class="label">Products</div><div class="val" id="sumProducts">0</div></div>
        <div class="summary-card" style="border-left-color:#2980b9"><div class="label">FG Loose JARs</div><div class="val" id="sumLooseJars" style="color:#2980b9">0</div></div>
        <div class="summary-card" style="border-left-color:#2980b9"><div class="label">FG Cases</div><div class="val" id="sumFGCases" style="color:#2980b9">0</div></div>
        <div class="summary-card" style="border-left-color:#e67e22"><div class="label">Reserved (Draft DC)</div><div class="val" id="sumReserved" style="color:#e67e22">0</div></div>
        <div class="summary-card" style="border-left-color:#27ae60"><div class="label">Available for DC</div><div class="val" id="sumAvailDC" style="color:#27ae60">0</div></div>
    </div>

    <div class="card">
        <div id="tableArea">
            <div class="loading">Loading stock data...</div>
        </div>
    </div>
    <div class="timestamp" id="timestamp"></div>
</div>

</form>

<script>
(function(){
    var xhr = new XMLHttpRequest();
    xhr.open('GET', 'SAFGStockAPI.ashx?action=data', true);
    xhr.onreadystatechange = function() {
        if (xhr.readyState === 4) {
            if (xhr.status === 200) {
                try {
                    var data = JSON.parse(xhr.responseText);
                    render(data);
                } catch(e) {
                    document.getElementById('tableArea').innerHTML = '<div class="no-data">Error loading data.</div>';
                }
            } else {
                document.getElementById('tableArea').innerHTML = '<div class="no-data">Failed to load. Please refresh.</div>';
            }
        }
    };
    xhr.send();

    function render(data) {
        if (!data || data.length === 0) {
            document.getElementById('tableArea').innerHTML = '<div class="no-data">No case-packed products found.</div>';
            return;
        }

        var totalLooseJars = 0, totalFGCases = 0, totalRes = 0, totalAvailDC = 0;

        var html = '<table><thead><tr>';
        html += '<th style="width:30px">#</th>';
        html += '<th style="width:80px">Product Code</th>';
        html += '<th>Product Name</th>';
        html += '<th style="width:80px;text-align:right">FG Loose<br/>JARs</th>';
        html += '<th style="width:80px;text-align:right">Cases<br/>Packed</th>';
        html += '<th style="width:80px;text-align:right">Dispatched</th>';
        html += '<th style="width:70px;text-align:right">FG<br/>Cases</th>';
        html += '<th style="width:70px;text-align:right">Reserved</th>';
        html += '<th style="width:80px;text-align:right">Avail<br/>for DC</th>';
        html += '</tr></thead><tbody>';

        for (var i = 0; i < data.length; i++) {
            var d = data[i];
            totalLooseJars += d.looseJars;
            totalFGCases += d.fgCases;
            totalRes += d.reserved;
            totalAvailDC += d.availDC;

            var availClass = d.availDC > 0 ? 'avail' : (d.availDC < 0 ? 'avail negative' : 'avail zero');

            html += '<tr>';
            html += '<td style="color:var(--muted);font-size:11px">' + (i + 1) + '</td>';
            html += '<td class="code">' + esc(d.code) + '</td>';
            html += '<td class="product-name">' + esc(d.name) + '</td>';
            html += '<td class="num" style="color:#2980b9;font-weight:700">' + (d.looseJars > 0 ? fmt(d.looseJars) : '<span style="color:#ccc">0</span>') + '</td>';
            html += '<td class="num">' + (d.casesPacked > 0 ? fmt(d.casesPacked) : '<span style="color:#ccc">—</span>') + '</td>';
            html += '<td class="num">' + (d.dispatched > 0 ? fmt(d.dispatched) : '<span style="color:#ccc">—</span>') + '</td>';
            html += '<td class="num" style="color:#2980b9;font-weight:700">' + (d.fgCases > 0 ? fmt(d.fgCases) : '<span style="color:#ccc">0</span>') + '</td>';
            html += '<td class="num" style="color:#e67e22">' + (d.reserved > 0 ? fmt(d.reserved) : '<span style="color:#ccc">—</span>') + '</td>';
            html += '<td class="num ' + availClass + '">' + fmt(d.availDC) + '</td>';
            html += '</tr>';
        }
        html += '</tbody></table>';

        document.getElementById('tableArea').innerHTML = html;
        document.getElementById('summaryStrip').style.display = '';
        document.getElementById('sumProducts').textContent = data.length;
        document.getElementById('sumLooseJars').textContent = fmt(totalLooseJars);
        document.getElementById('sumFGCases').textContent = fmt(totalFGCases);
        document.getElementById('sumReserved').textContent = fmt(totalRes);
        document.getElementById('sumAvailDC').textContent = fmt(totalAvailDC);
        document.getElementById('timestamp').textContent = 'Last refreshed: ' + new Date().toLocaleString();
    }

    function fmt(n) { return n.toLocaleString(); }
    function esc(s) { var d = document.createElement('div'); d.textContent = s; return d.innerHTML; }
})();
</script>
</body>
</html>
