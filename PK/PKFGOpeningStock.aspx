<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PKFGOpeningStock.aspx.cs" Inherits="PKApp.PKFGOpeningStock" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width, initial-scale=1.0"/>
<title>FG Opening Stock — Sirimiri ERP</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600;700&family=JetBrains+Mono:wght@400;500&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--accent:#e67e22;--accent-dark:#d35400;--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--text:#1a1a1a;--muted:#666;--dim:#999;--radius:12px;--nav-h:52px;}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);min-height:100vh;}
nav{background:#1a1a1a;height:var(--nav-h);display:flex;align-items:center;padding:0 20px;gap:12px;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;}
.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.08em;}
.nav-right{margin-left:auto;display:flex;gap:16px;}
.nav-link{color:#fff;font-size:12px;font-weight:600;text-decoration:none;opacity:.85;padding:6px 12px;border-radius:4px;}
.nav-link:hover{opacity:1;background:rgba(255,255,255,.1);}
.page-wrap{max-width:1000px;margin:0 auto;padding:24px 16px 60px;}
.page-header{display:flex;align-items:center;justify-content:space-between;margin-bottom:16px;flex-wrap:wrap;gap:12px;}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:28px;letter-spacing:.06em;color:var(--text);}
.page-desc{font-size:13px;color:var(--muted);margin-bottom:20px;line-height:1.6;}
.card{background:var(--surface);border-radius:var(--radius);box-shadow:0 2px 12px rgba(0,0,0,.06);overflow:hidden;}
.card-head{background:var(--accent);padding:12px 20px;}
.card-head h2{font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.08em;color:#fff;}
table{width:100%;border-collapse:collapse;}
th{font-size:10px;font-weight:600;letter-spacing:.06em;text-transform:uppercase;color:var(--muted);padding:10px 14px;text-align:left;border-bottom:2px solid var(--border);background:#fafafa;}
td{padding:8px 14px;border-bottom:1px solid var(--border);vertical-align:middle;font-size:13px;}
tr:last-child td{border-bottom:none;}
tr:hover td{background:#f8f9fb;}
.product-name{font-weight:500;color:var(--text);}
.product-meta{font-size:11px;color:var(--muted);margin-top:1px;}
.stock-input{width:70px;padding:6px 8px;border:1.5px solid var(--border);border-radius:6px;font-size:14px;font-weight:600;text-align:center;color:var(--text);font-family:'JetBrains Mono',monospace;}
.stock-input:focus{outline:none;border-color:var(--accent);background:#fef5ec;}
.stock-input.changed{border-color:var(--accent);background:#fef5ec;}
.form-label{font-size:10px;color:var(--dim);text-align:center;margin-top:2px;}
.btn-row{padding:16px 20px;display:flex;gap:12px;border-top:1px solid var(--border);}
.btn-save{padding:10px 28px;background:var(--accent);color:#fff;border:none;border-radius:8px;font-size:14px;font-weight:600;cursor:pointer;}
.btn-save:hover{background:var(--accent-dark);}
.btn-save:disabled{opacity:.5;cursor:not-allowed;}
.msg{padding:12px 20px;font-size:13px;display:none;}
.msg.ok{background:#f0fdf4;color:#166534;border-bottom:1px solid #86efac;}
.msg.err{background:#fff8f8;color:#991b1b;border-bottom:1px solid #fca5a5;}
.change-count{font-size:12px;color:var(--accent);font-weight:600;}
</style>
</head>
<body>
<form id="form1" runat="server">
<nav>
    <div class="nav-logo">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri"
             onerror="this.parentElement.innerHTML='<span style=color:#e67e22;font-weight:700>SIRIMIRI</span>'"/>
    </div>
    <span class="nav-title">FG Opening Stock</span>
    <div class="nav-right">
        <a href="PKHome.aspx" class="nav-link">&#x2302; PK Home</a>
        <a href="/StockApp/ERPHome.aspx" class="nav-link">&#x2302; ERP Home</a>
    </div>
</nav>

<div class="page-wrap">
    <div class="page-header">
        <div class="page-title">FINISHED GOODS — OPENING STOCK</div>
        <span class="change-count" id="changeCount"></span>
    </div>
    <p class="page-desc">
        Enter opening stock for each Core product. JARs/BOXes are loose containers not yet case-packed.
        CASEs are already packed into shipping cases. These values are added to the FG stock calculations.
    </p>

    <div class="card">
        <div class="card-head"><h2>CORE PRODUCTS — OPENING STOCK</h2></div>
        <div class="msg ok" id="msgOk"></div>
        <div class="msg err" id="msgErr"></div>
        <table>
            <thead><tr>
                <th style="width:40px">#</th>
                <th style="width:80px">Code</th>
                <th>Product</th>
                <th style="width:80px">Container</th>
                <th style="width:100px;text-align:center">JARs / BOXes</th>
                <th style="width:100px;text-align:center">CASEs</th>
            </tr></thead>
            <tbody id="tbody"></tbody>
        </table>
        <div class="btn-row">
            <button type="button" class="btn-save" id="btnSave" onclick="saveAll()" disabled>SAVE OPENING STOCK</button>
        </div>
    </div>
</div>
</form>

<script>
(function(){
    var products = [];
    var origData = {}; // pid -> {jars, cases}

    function fetchJSON(url, cb) {
        var xhr = new XMLHttpRequest(); xhr.open('GET', url, true);
        xhr.onreadystatechange = function() {
            if (xhr.readyState === 4 && xhr.status === 200) {
                try { cb(JSON.parse(xhr.responseText)); } catch(e) { cb(null); }
            }
        };
        xhr.send();
    }

    fetchJSON('/StockApp/SAFGStockAPI.ashx?action=fgOpeningStock', function(data) {
        if (!data || data.error) {
            document.getElementById('tbody').innerHTML = '<tr><td colspan="6" style="text-align:center;padding:30px;color:#999">Failed to load data</td></tr>';
            return;
        }
        products = data.products || [];
        var existing = data.opening || {};
        render(existing);
    });

    function render(existing) {
        var tbody = document.getElementById('tbody');
        var html = '';
        for (var i = 0; i < products.length; i++) {
            var p = products[i];
            var jarQty = (existing[p.id] && existing[p.id].jar) || 0;
            var caseQty = (existing[p.id] && existing[p.id].cs) || 0;
            origData[p.id] = { jars: jarQty, cases: caseQty };

            var containerLabel = p.ct || 'JAR';
            html += '<tr>';
            html += '<td style="color:var(--dim);font-size:11px">' + (i+1) + '</td>';
            html += '<td style="font-family:JetBrains Mono,monospace;font-size:11px;color:var(--dim)">' + esc(p.code) + '</td>';
            html += '<td><div class="product-name">' + esc(p.name) + '</div></td>';
            html += '<td style="font-size:12px;color:var(--muted)">' + esc(containerLabel) + '<br/><small>' + p.cpc + ' per case</small></td>';
            html += '<td style="text-align:center"><input type="number" class="stock-input" id="jar_' + p.id + '" value="' + jarQty + '" min="0" onchange="track()"/><div class="form-label">' + esc(containerLabel) + 's</div></td>';
            html += '<td style="text-align:center"><input type="number" class="stock-input" id="case_' + p.id + '" value="' + caseQty + '" min="0" onchange="track()"/><div class="form-label">Cases</div></td>';
            html += '</tr>';
        }
        tbody.innerHTML = html;
    }

    window.track = function() {
        var count = 0;
        for (var i = 0; i < products.length; i++) {
            var pid = products[i].id;
            var j = parseInt(document.getElementById('jar_' + pid).value) || 0;
            var c = parseInt(document.getElementById('case_' + pid).value) || 0;
            var jEl = document.getElementById('jar_' + pid);
            var cEl = document.getElementById('case_' + pid);
            var jChanged = j !== origData[pid].jars;
            var cChanged = c !== origData[pid].cases;
            jEl.classList.toggle('changed', jChanged);
            cEl.classList.toggle('changed', cChanged);
            if (jChanged || cChanged) count++;
        }
        document.getElementById('changeCount').textContent = count > 0 ? count + ' change(s)' : '';
        document.getElementById('btnSave').disabled = count === 0;
    };

    window.saveAll = function() {
        var pairs = [];
        for (var i = 0; i < products.length; i++) {
            var pid = products[i].id;
            var j = parseInt(document.getElementById('jar_' + pid).value) || 0;
            var c = parseInt(document.getElementById('case_' + pid).value) || 0;
            if (j !== origData[pid].jars || c !== origData[pid].cases) {
                pairs.push(pid + ':' + j + ':' + c);
            }
        }
        if (pairs.length === 0) return;
        document.getElementById('btnSave').disabled = true;

        var xhr = new XMLHttpRequest();
        xhr.open('POST', '/StockApp/SAFGStockAPI.ashx', true);
        xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
        xhr.onreadystatechange = function() {
            if (xhr.readyState === 4) {
                try {
                    var resp = JSON.parse(xhr.responseText);
                    if (resp.ok) {
                        for (var i = 0; i < products.length; i++) {
                            var pid = products[i].id;
                            origData[pid].jars = parseInt(document.getElementById('jar_' + pid).value) || 0;
                            origData[pid].cases = parseInt(document.getElementById('case_' + pid).value) || 0;
                            document.getElementById('jar_' + pid).classList.remove('changed');
                            document.getElementById('case_' + pid).classList.remove('changed');
                        }
                        document.getElementById('changeCount').textContent = '';
                        showMsg('ok', 'Saved ' + resp.updated + ' product(s) successfully.');
                    } else {
                        showMsg('err', resp.error || 'Save failed.');
                        document.getElementById('btnSave').disabled = false;
                    }
                } catch(e) { showMsg('err', 'Error saving.'); document.getElementById('btnSave').disabled = false; }
            }
        };
        xhr.send('action=saveFGOpening&data=' + encodeURIComponent(pairs.join(';')));
    };

    function showMsg(type, msg) {
        var ok = document.getElementById('msgOk'), err = document.getElementById('msgErr');
        ok.style.display = 'none'; err.style.display = 'none';
        if (type === 'ok') { ok.textContent = msg; ok.style.display = ''; }
        if (type === 'err') { err.textContent = msg; err.style.display = ''; }
        setTimeout(function() { ok.style.display = 'none'; err.style.display = 'none'; }, 5000);
    }

    function esc(s) { var d = document.createElement('div'); d.textContent = s; return d.innerHTML; }
})();
</script>
<script src="/StockApp/erp-modal.js"></script><script src="/StockApp/erp-keepalive.js"></script>
</body>
</html>
