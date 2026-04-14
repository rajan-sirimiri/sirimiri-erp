<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SAProductDefaults.aspx.cs" Inherits="StockApp.SAProductDefaults" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width, initial-scale=1.0"/>
<title>Product Default Pack — Sirimiri ERP</title>
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
.page-wrap{max-width:800px;margin:0 auto;padding:24px 16px 60px}
.page-header{display:flex;align-items:center;justify-content:space-between;margin-bottom:20px;flex-wrap:wrap;gap:12px}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:28px;letter-spacing:.06em;color:var(--text)}
.page-desc{font-size:13px;color:var(--muted);margin-bottom:20px;line-height:1.6}
.card{background:var(--surface);border-radius:var(--radius);box-shadow:0 2px 12px rgba(0,0,0,.06);overflow:hidden}
.card-head{background:var(--accent);padding:12px 20px}
.card-head h2{font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.08em;color:#fff}
table{width:100%;border-collapse:collapse}
th{font-size:11px;font-weight:600;letter-spacing:.06em;text-transform:uppercase;color:var(--muted);padding:10px 16px;text-align:left;border-bottom:2px solid var(--border);background:#fafafa}
td{padding:10px 16px;border-bottom:1px solid var(--border);vertical-align:middle;font-size:13px}
tr:last-child td{border-bottom:none}
tr:hover td{background:#f8f9fb}
.product-name{font-weight:500;color:var(--text)}
.product-meta{font-size:11px;color:var(--muted);margin-top:2px}
.pack-select{padding:7px 10px;border:1.5px solid var(--border);border-radius:6px;font-size:13px;color:var(--text);background:#fff;min-width:120px;font-family:'DM Sans',sans-serif}
.pack-select:focus{outline:none;border-color:var(--accent)}
.pack-select.changed{border-color:var(--accent);background:#eef6fb}
.btn-row{padding:16px 20px;display:flex;gap:12px;border-top:1px solid var(--border)}
.btn-save{padding:10px 28px;background:var(--accent);color:#fff;border:none;border-radius:8px;font-size:14px;font-weight:600;cursor:pointer}
.btn-save:hover{background:var(--accent-dark)}
.btn-save:disabled{opacity:.5;cursor:not-allowed}
.msg{padding:12px 20px;font-size:13px;display:none}
.msg.ok{background:#f0fdf4;color:#166534;border-bottom:1px solid #86efac}
.msg.err{background:#fff8f8;color:#991b1b;border-bottom:1px solid #fca5a5}
.change-count{font-size:12px;color:var(--accent);font-weight:600}
</style>
</head>
<body>
<form id="form1" runat="server">
<nav>
    <div class="nav-logo">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri"
             onerror="this.parentElement.innerHTML='<span style=color:#2980b9;font-weight:700>SIRIMIRI</span>'"/>
    </div>
    <span class="nav-title">Product Default Pack Settings</span>
    <div class="nav-right">
        <a href="DailySales.aspx" class="nav-link">&#x270F; Sales Entry</a>
        <a href="SAHome.aspx" class="nav-link">&#x2302; SA Home</a>
        <a href="ERPHome.aspx" class="nav-link">&#x2302; ERP Home</a>
    </div>
</nav>

<div class="page-wrap">
    <div class="page-header">
        <div class="page-title">DEFAULT SELLING UNIT PER PRODUCT</div>
        <span class="change-count" id="changeCount"></span>
    </div>
    <p class="page-desc">
        Set the default pack form (PCS, JAR, BOX, CASE) for each product. When a sales person opens the Daily Sales Entry page,
        the pack form dropdown will be pre-selected to this default. They can still change it per entry if needed.
    </p>

    <div class="card">
        <div class="card-head"><h2>CORE PRODUCTS</h2></div>
        <div class="msg ok" id="msgOk"></div>
        <div class="msg err" id="msgErr"></div>
        <table>
            <thead>
                <tr>
                    <th style="width:50px">#</th>
                    <th>Product</th>
                    <th style="width:200px">Default Selling Unit</th>
                </tr>
            </thead>
            <tbody id="tbody"></tbody>
        </table>
        <div class="btn-row">
            <button type="button" class="btn-save" id="btnSave" onclick="saveDefaults()" disabled>SAVE CHANGES</button>
        </div>
    </div>
</div>

</form>

<script>
(function(){
    var products = [];
    var origDefaults = {}; // pid -> original default form

    function fetchJSON(url, cb) {
        var xhr = new XMLHttpRequest(); xhr.open('GET', url, true);
        xhr.onreadystatechange = function() {
            if (xhr.readyState === 4 && xhr.status === 200) {
                try { cb(JSON.parse(xhr.responseText)); } catch(e) { cb(null); }
            }
        };
        xhr.send();
    }

    fetchJSON('SADailySalesAPI.ashx?action=products', function(data) {
        if (!data || data.error) return;
        products = data;
        render();
    });

    function render() {
        var tbody = document.getElementById('tbody');
        var html = '';
        for (var i = 0; i < products.length; i++) {
            var p = products[i];
            origDefaults[p.id] = p.df || 'PCS';

            var meta = [];
            if (p.wt > 0) meta.push(p.wt + 'g');
            if (p.ct) meta.push(p.ct);

            // Build options from available packing forms
            var forms = {};
            for (var j = 0; j < p.packs.length; j++) {
                var pk = p.packs[j];
                if (!forms[pk.form]) {
                    var label = pk.form;
                    if (pk.units > 1) label += ' (of ' + pk.units + ')';
                    forms[pk.form] = label;
                }
            }
            // Always ensure PCS is available
            if (!forms['PCS']) forms['PCS'] = 'PCS';

            var opts = '';
            var formKeys = ['PCS', 'JAR', 'BOX', 'CASE'];
            for (var k = 0; k < formKeys.length; k++) {
                var f = formKeys[k];
                if (forms[f]) {
                    var sel = (f === p.df) ? ' selected' : '';
                    opts += '<option value="' + f + '"' + sel + '>' + forms[f] + '</option>';
                }
            }

            html += '<tr>' +
                '<td style="color:var(--muted);font-size:12px">' + (i + 1) + '</td>' +
                '<td><div class="product-name">' + esc(p.name) + '</div>' +
                (meta.length ? '<div class="product-meta">' + esc(meta.join(' | ')) + '</div>' : '') +
                '</td>' +
                '<td><select class="pack-select" id="df_' + p.id + '" onchange="trackChange(' + p.id + ')">' + opts + '</select></td>' +
                '</tr>';
        }
        tbody.innerHTML = html;
    }

    window.trackChange = function(pid) {
        var sel = document.getElementById('df_' + pid);
        var changed = sel.value !== origDefaults[pid];
        sel.classList.toggle('changed', changed);

        // Count total changes
        var count = 0;
        for (var i = 0; i < products.length; i++) {
            var s = document.getElementById('df_' + products[i].id);
            if (s && s.value !== origDefaults[products[i].id]) count++;
        }
        document.getElementById('changeCount').textContent = count > 0 ? count + ' change(s)' : '';
        document.getElementById('btnSave').disabled = count === 0;
    };

    window.saveDefaults = function() {
        var pairs = [];
        for (var i = 0; i < products.length; i++) {
            var pid = products[i].id;
            var sel = document.getElementById('df_' + pid);
            if (sel && sel.value !== origDefaults[pid]) {
                pairs.push(pid + ':' + sel.value);
            }
        }
        if (pairs.length === 0) return;

        document.getElementById('btnSave').disabled = true;

        var xhr = new XMLHttpRequest();
        xhr.open('POST', 'SADailySalesAPI.ashx', true);
        xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
        xhr.onreadystatechange = function() {
            if (xhr.readyState === 4) {
                try {
                    var resp = JSON.parse(xhr.responseText);
                    if (resp.ok) {
                        // Update origDefaults
                        for (var i = 0; i < products.length; i++) {
                            var pid = products[i].id;
                            var sel = document.getElementById('df_' + pid);
                            if (sel) {
                                origDefaults[pid] = sel.value;
                                sel.classList.remove('changed');
                            }
                        }
                        document.getElementById('changeCount').textContent = '';
                        showMsg('ok', 'Saved ' + resp.updated + ' default(s) successfully.');
                    } else {
                        showMsg('err', resp.error || 'Save failed.');
                        document.getElementById('btnSave').disabled = false;
                    }
                } catch(e) {
                    showMsg('err', 'Unexpected error.');
                    document.getElementById('btnSave').disabled = false;
                }
            }
        };
        xhr.send('action=saveDefaults&defaults=' + encodeURIComponent(pairs.join(';')));
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
</body>
</html>
