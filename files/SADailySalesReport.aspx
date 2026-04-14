<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SADailySalesReport.aspx.cs" Inherits="StockApp.SADailySalesReport" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width, initial-scale=1.0"/>
<title>Daily Sales Report — Sirimiri ERP</title>
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
.page-wrap{max-width:1100px;margin:0 auto;padding:24px 16px 60px}
.page-header{display:flex;align-items:center;justify-content:space-between;margin-bottom:20px;flex-wrap:wrap;gap:12px}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:28px;letter-spacing:.06em;color:var(--text)}
/* Filters */
.filter-card{background:var(--surface);border-radius:var(--radius);padding:16px 20px;margin-bottom:20px;display:flex;align-items:center;gap:16px;flex-wrap:wrap;box-shadow:0 2px 8px rgba(0,0,0,.06);border-left:4px solid var(--accent)}
.filter-card label{font-size:11px;font-weight:600;text-transform:uppercase;letter-spacing:.06em;color:var(--muted)}
.filter-card input[type=date]{padding:8px 12px;border:1.5px solid var(--border);border-radius:8px;font-size:14px;color:var(--text);font-family:'DM Sans',sans-serif}
.filter-card input[type=date]:focus{outline:none;border-color:var(--accent)}
.btn-go{padding:8px 24px;background:var(--accent);color:#fff;border:none;border-radius:8px;font-size:14px;font-weight:600;cursor:pointer}
.btn-go:hover{background:var(--accent-dark)}
/* User cards */
.user-card{background:var(--surface);border-radius:var(--radius);margin-bottom:20px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,.06)}
.user-hdr{background:#1a1a1a;padding:14px 20px;display:flex;align-items:center;gap:16px;flex-wrap:wrap}
.user-name{font-family:'Bebas Neue',sans-serif;font-size:20px;color:#fff;letter-spacing:.06em}
.user-desig{font-size:12px;color:rgba(255,255,255,.6);font-weight:500}
.user-stat{display:flex;align-items:center;gap:6px;background:rgba(255,255,255,.1);padding:5px 12px;border-radius:6px;font-size:12px;color:#fff;font-weight:600}
.user-stat .val{color:var(--accent);font-family:'JetBrains Mono',monospace;font-size:14px}
.user-stat.new .val{color:#4ecb71}
.user-stat.repeat .val{color:#f5a623}
.user-body{padding:0}
.detail-table{width:100%;border-collapse:collapse}
.detail-table th{font-size:11px;font-weight:600;letter-spacing:.06em;text-transform:uppercase;color:var(--muted);padding:10px 16px;text-align:left;border-bottom:2px solid var(--border);background:#fafafa}
.detail-table td{padding:10px 16px;border-bottom:1px solid var(--border);font-size:13px}
.detail-table tr:last-child td{border-bottom:none}
.detail-table tr:hover td{background:#f8f9fb}
.detail-table .dist-name{font-weight:600;color:var(--text)}
.detail-table .pack-badge{display:inline-block;padding:2px 8px;border-radius:4px;font-size:11px;font-weight:600;background:#eef2f7;color:#555}
.detail-table .units-col{font-family:'JetBrains Mono',monospace;font-weight:600;color:var(--accent)}
.no-data{text-align:center;padding:40px;color:var(--muted);font-size:14px}
.loading{text-align:center;padding:40px;color:var(--muted)}
</style>
</head>
<body>
<form id="form1" runat="server">
<nav>
    <div class="nav-logo">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri"
             onerror="this.parentElement.innerHTML='<span style=color:#2980b9;font-weight:700>SIRIMIRI</span>'"/>
    </div>
    <span class="nav-title">Daily Sales Report</span>
    <div class="nav-right">
        <a href="DailySales.aspx" class="nav-link">&#x270F; Sales Entry</a>
        <a href="SAHome.aspx" class="nav-link">&#x2302; SA Home</a>
        <a href="ERPHome.aspx" class="nav-link">&#x2302; ERP Home</a>
    </div>
</nav>

<div class="page-wrap">
    <div class="page-header">
        <div class="page-title">DAILY SALES REPORT — SO / SR WISE</div>
    </div>

    <!-- Date Filter -->
    <div class="filter-card">
        <div>
            <label>From</label><br/>
            <input type="date" id="dtFrom"/>
        </div>
        <div>
            <label>To</label><br/>
            <input type="date" id="dtTo"/>
        </div>
        <button type="button" class="btn-go" onclick="loadReport()">LOAD REPORT</button>
    </div>

    <div id="reportArea">
        <div class="no-data">Select a date range and click LOAD REPORT</div>
    </div>
</div>

</form>

<script>
(function(){
    // Default dates: today
    var today = new Date().toISOString().split('T')[0];
    document.getElementById('dtFrom').value = today;
    document.getElementById('dtTo').value = today;

    window.loadReport = function() {
        var df = document.getElementById('dtFrom').value;
        var dt = document.getElementById('dtTo').value;
        if (!df || !dt) { alert('Please select dates.'); return; }

        var area = document.getElementById('reportArea');
        area.innerHTML = '<div class="loading">Loading report...</div>';

        var xhr = new XMLHttpRequest();
        xhr.open('GET', 'SADailySalesAPI.ashx?action=report&dateFrom=' + df + '&dateTo=' + dt, true);
        xhr.onreadystatechange = function() {
            if (xhr.readyState === 4) {
                if (xhr.status === 200) {
                    try {
                        var data = JSON.parse(xhr.responseText);
                        renderReport(data);
                    } catch(e) {
                        area.innerHTML = '<div class="no-data">Error loading report.</div>';
                    }
                } else {
                    area.innerHTML = '<div class="no-data">Error loading report.</div>';
                }
            }
        };
        xhr.send();
    };

    function renderReport(data) {
        var area = document.getElementById('reportArea');
        if (!data.users || data.users.length === 0) {
            area.innerHTML = '<div class="no-data">No sales entries found for this date range.</div>';
            return;
        }

        var html = '';
        for (var u = 0; u < data.users.length; u++) {
            var user = data.users[u];
            html += '<div class="user-card">';
            html += '<div class="user-hdr">';
            html += '<span class="user-name">' + esc(user.name) + '</span>';
            if (user.desig) html += '<span class="user-desig">' + esc(user.desig) + '</span>';
            html += '<div class="user-stat"><span>Distributors:</span> <span class="val">' + user.distCount + '</span></div>';
            html += '<div class="user-stat new"><span>New Shops:</span> <span class="val">' + user.newShops + '</span></div>';
            html += '<div class="user-stat repeat"><span>Repeat Shops:</span> <span class="val">' + user.repeatShops + '</span></div>';
            html += '<div class="user-stat"><span>Total Units:</span> <span class="val">' + user.totalUnits + '</span></div>';
            html += '</div>';

            if (user.lines.length > 0) {
                html += '<div class="user-body"><table class="detail-table">';
                html += '<thead><tr><th>Distributor</th><th>Product</th><th>Pack Form</th><th style="text-align:right">Packs</th><th style="text-align:right">Total Units</th></tr></thead>';
                html += '<tbody>';
                var prevDist = '';
                for (var l = 0; l < user.lines.length; l++) {
                    var ln = user.lines[l];
                    var showDist = ln.dist !== prevDist;
                    prevDist = ln.dist;
                    html += '<tr>';
                    html += '<td class="dist-name">' + (showDist ? esc(ln.dist) : '') + '</td>';
                    html += '<td>' + esc(ln.product) + '</td>';
                    html += '<td><span class="pack-badge">' + esc(ln.form) + '</span></td>';
                    html += '<td style="text-align:right;font-family:JetBrains Mono,monospace">' + ln.packs + '</td>';
                    html += '<td style="text-align:right" class="units-col">' + ln.units + '</td>';
                    html += '</tr>';
                }
                html += '</tbody></table></div>';
            }
            html += '</div>';
        }
        area.innerHTML = html;
    }

    function esc(s) { var d = document.createElement('div'); d.textContent = s; return d.innerHTML; }
})();
</script>
</body>
</html>
