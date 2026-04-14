<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DailySales.aspx.cs" Inherits="StockApp.DailySales" ResponseEncoding="UTF-8" ContentType="text/html; charset=utf-8" %>
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <title>Sirimiri - Daily Sales Entry</title>
    <link rel="preconnect" href="https://fonts.googleapis.com"/>
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin/>
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
    <style>
        :root{--accent:#C0392B;--accent-dark:#a93226;--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--text:#1a1a1a;--muted:#666;--radius:14px}
        *{box-sizing:border-box;margin:0;padding:0}
        body{background:var(--bg);font-family:'DM Sans',sans-serif;min-height:100vh}
        nav{background:var(--accent);display:flex;align-items:center;padding:0 24px;height:52px;gap:12px}
        .nav-group{position:relative}
        .nav-item{color:#fff;font-size:13px;font-weight:600;padding:8px 14px;border-radius:6px;cursor:pointer;display:flex;align-items:center;gap:6px;text-decoration:none}
        .nav-item:hover{background:rgba(255,255,255,.15)}
        .nav-dropdown{display:none;position:absolute;top:100%;left:0;background:#fff;border-radius:8px;min-width:220px;box-shadow:0 4px 20px rgba(0,0,0,.15);z-index:999;overflow:hidden}
        .nav-group:hover .nav-dropdown{display:block}
        .nav-dropdown a{display:block;padding:10px 16px;font-size:13px;color:var(--text);text-decoration:none}
        .nav-dropdown a:hover{background:var(--bg);color:var(--accent)}
        .nav-right{margin-left:auto;display:flex;align-items:center;gap:20px;font-size:13px}
        .nav-right a{color:#fff;font-weight:700;text-decoration:none;opacity:.9}
        .btn-signout{border:1.5px solid rgba(255,255,255,.6);padding:5px 14px;border-radius:6px}
        .user-label{color:#fff;opacity:.9;font-weight:500}
        .logo-area{background:#fff;display:flex;align-items:center;justify-content:space-between;padding:16px 24px 0}
        .logo-area img{height:72px;object-fit:contain;filter:drop-shadow(0 2px 8px rgba(204,30,30,.20))}
        .bis-label{font-family:'Bebas Neue',cursive;font-size:22px;letter-spacing:.12em;color:var(--text);text-align:center;line-height:1.25}
        .accent-bar{height:4px;background:linear-gradient(90deg,var(--accent-dark),#e63030,var(--accent-dark))}
        .page-wrap{max-width:860px;margin:0 auto;padding:28px 16px 60px}
        .page-header{display:flex;align-items:center;justify-content:space-between;margin-bottom:22px}
        .page-title{font-family:'Bebas Neue',cursive;font-size:32px;letter-spacing:.08em;color:var(--text)}
        .date-badge{background:var(--accent);color:#fff;font-family:'Bebas Neue',cursive;font-size:14px;letter-spacing:.08em;padding:6px 16px;border-radius:20px}
        .card{background:var(--surface);border-radius:var(--radius);box-shadow:0 2px 16px rgba(0,0,0,.08);margin-bottom:22px;overflow:hidden}
        .card-head{background:var(--accent);padding:12px 20px}
        .card-head h2{font-family:'Bebas Neue',cursive;font-size:18px;letter-spacing:.08em;color:#fff}
        .card-body{padding:20px}
        .field-row{display:grid;grid-template-columns:1fr 1fr 1fr;gap:16px}
        @media(max-width:640px){.field-row{grid-template-columns:1fr}}
        .field label{display:block;font-size:11px;font-weight:600;letter-spacing:.06em;text-transform:uppercase;color:var(--muted);margin-bottom:6px}
        .field select{width:100%;padding:10px 12px;border:1.5px solid var(--border);border-radius:8px;font-size:14px;background:#fff;color:var(--text)}
        .field select:focus{outline:none;border-color:var(--accent)}
        /* Multi-select distributor */
        .field select[multiple]{height:140px;padding:6px}
        .field select[multiple] option{padding:6px 8px;border-radius:4px;margin-bottom:2px}
        .field select[multiple] option:checked{background:var(--accent);color:#fff}
        .multi-hint{font-size:11px;color:var(--muted);margin-top:4px}
        /* Product table */
        .product-table{width:100%;border-collapse:collapse}
        .product-table th{font-size:11px;font-weight:600;letter-spacing:.06em;text-transform:uppercase;color:var(--muted);padding:10px 12px;text-align:left;border-bottom:2px solid var(--border)}
        .product-table td{padding:10px 12px;border-bottom:1px solid var(--border);vertical-align:middle}
        .product-table tr:last-child td{border-bottom:none}
        .product-table tr:hover td{background:#fafafa}
        .product-name{font-size:14px;font-weight:500;color:var(--text)}
        .product-meta{font-size:11px;color:var(--muted);margin-top:2px}
        .qty-wrap{display:flex;align-items:center;gap:8px}
        .pack-select{padding:8px 10px;border:1.5px solid var(--border);border-radius:8px;font-size:13px;color:var(--text);background:#fff;min-width:130px}
        .pack-select:focus{outline:none;border-color:var(--accent)}
        .qty-input{width:80px;padding:8px 10px;border:1.5px solid var(--border);border-radius:8px;font-size:15px;font-weight:600;text-align:center;color:var(--text)}
        .qty-input:focus{outline:none;border-color:var(--accent);background:#fff8f8}
        .qty-input:invalid{border-color:#e74c3c}
        .units-label{font-size:12px;color:var(--muted);font-weight:500;min-width:50px}
        /* Buttons */
        .btn-row{display:flex;gap:12px;margin-top:8px}
        .btn-save{padding:12px 32px;background:var(--accent);color:#fff;border:none;border-radius:8px;font-size:15px;font-weight:600;cursor:pointer;letter-spacing:.04em}
        .btn-save:hover{background:var(--accent-dark)}
        .btn-cancel{padding:12px 28px;background:#fff;color:var(--text);border:1.5px solid var(--border);border-radius:8px;font-size:15px;font-weight:600;cursor:pointer}
        .btn-cancel:hover{background:var(--bg)}
        /* Messages */
        .msg-ok{background:#f0fdf4;border:1px solid #86efac;border-radius:8px;padding:12px 16px;color:#166534;font-size:14px;margin-bottom:16px}
        .msg-err{background:#fff8f8;border:1px solid #fca5a5;border-radius:8px;padding:12px 16px;color:#991b1b;font-size:14px;margin-bottom:16px}
        .empty-products{text-align:center;padding:40px;color:var(--muted);font-size:14px}
    </style>
</head>
<body>
<form id="form1" runat="server">

    <!-- NAV -->
    <nav>
        <a href="ERPHome.aspx" style="text-decoration:none;color:#fff;font-size:13px;font-weight:600;padding:14px 18px;letter-spacing:.04em;text-transform:uppercase;">&#x2302; ERP</a>
        <div class="nav-group">
            <span class="nav-item">&#9776; Home</span>
            <div class="nav-dropdown">
                <a href="StockEntry.aspx">Distributor Stock Position Entry</a>
                <a href="DailySales.aspx" style="color:var(--accent);font-weight:600;">Daily Sales Entry</a>
            </div>
        </div>
        <asp:Panel ID="pnlAdminMenu" runat="server" Visible="false" CssClass="nav-group">
            <span class="nav-item">&#9881; Admin</span>
            <div class="nav-dropdown">
                <a href="UserAdmin.aspx">User Management</a>
                <a href="ProductMaster.aspx">Product Master</a>
            </div>
        </asp:Panel>
        <div class="nav-right">
            <span class="user-label"><asp:Label ID="lblUserInfo" runat="server" /></span>
            <a href="Logout.aspx" class="btn-signout">Sign Out</a>
        </div>
    </nav>

    <!-- LOGO -->
    <div class="logo-area">
        <img src="https://vimarsa.in/StockApp/sirimiri-logo.png" alt="Sirimiri" onerror="this.style.display='none'" />
        <div class="bis-label">SIRIMIRI NUTRITION FOOD PRODUCTS<br/><span style="font-size:14px;letter-spacing:.14em;">BUSINESS INTELLIGENCE SYSTEM</span></div>
        <div></div>
    </div>
    <div class="accent-bar"></div>

    <div class="page-wrap">
        <div class="page-header">
            <div class="page-title">DAILY SALES ENTRY</div>
            <div class="date-badge"><%= DateTime.Now.ToString("dddd, d MMM yyyy").ToUpper() %></div>
        </div>

        <!-- Messages -->
        <div class="msg-ok" id="msgOk" style="display:none;"></div>
        <div class="msg-err" id="msgErr" style="display:none;"></div>

        <!-- DISTRIBUTOR SELECTION -->
        <div class="card">
            <div class="card-head"><h2>SELECT DISTRIBUTOR</h2></div>
            <div class="card-body">
                <div class="field-row">
                    <div class="field">
                        <label>State</label>
                        <asp:DropDownList ID="ddlState" runat="server" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlState_SelectedIndexChanged" />
                    </div>
                    <div class="field">
                        <label>City</label>
                        <asp:DropDownList ID="ddlCity" runat="server" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlCity_SelectedIndexChanged" />
                    </div>
                    <div class="field">
                        <label>Distributor</label>
                        <asp:ListBox ID="lstDistributor" runat="server" SelectionMode="Multiple" Rows="6" />
                        <div class="multi-hint">Hold Ctrl / Cmd to select multiple</div>
                    </div>
                </div>
            </div>
        </div>

        <!-- PRODUCT ENTRY (JS-rendered with pack form selector) -->
        <div class="card" id="cardProducts" style="display:none;">
            <div class="card-head"><h2>ENTER QUANTITIES SOLD</h2></div>
            <div class="card-body">
                <table class="product-table">
                    <thead>
                        <tr>
                            <th>Product</th>
                            <th style="width:160px;">Pack Form</th>
                            <th style="width:160px;">Quantity</th>
                        </tr>
                    </thead>
                    <tbody id="tbodyProducts"></tbody>
                </table>

                <br/>
                <div class="btn-row">
                    <button type="button" class="btn-save" onclick="saveEntries()">SAVE</button>
                    <button type="button" class="btn-cancel" onclick="resetForm()">CANCEL</button>
                </div>
            </div>
        </div>

    </div>
</form>

<script>
(function(){
    // Load products with packing options
    var products = [];

    function fetchJSON(url, cb) {
        var xhr = new XMLHttpRequest();
        xhr.open('GET', url, true);
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
        renderProducts();
    });

    function renderProducts() {
        var tbody = document.getElementById('tbodyProducts');
        if (!tbody) return;
        var html = '';
        for (var i = 0; i < products.length; i++) {
            var p = products[i];
            var meta = [];
            if (p.wt > 0) meta.push(p.wt + 'g');
            if (p.ct) meta.push(p.ct);
            if (p.hsn) meta.push('HSN: ' + p.hsn);

            // Build pack form dropdown
            var opts = '';
            if (p.packs.length === 0) {
                opts = '<option value="PCS|1">PCS</option>';
            } else {
                for (var j = 0; j < p.packs.length; j++) {
                    var pk = p.packs[j];
                    var label = pk.form;
                    if (pk.units > 1) label += ' of ' + pk.units;
                    if (pk.desc && pk.desc !== pk.form && pk.desc !== 'PCS') label = pk.desc;
                    opts += '<option value="' + pk.form + '|' + pk.units + '">' + esc(label) + '</option>';
                }
            }

            html += '<tr data-pid="' + p.id + '">' +
                '<td><div class="product-name">' + esc(p.name) + '</div>' +
                (meta.length ? '<div class="product-meta">' + esc(meta.join(' | ')) + '</div>' : '') +
                '</td>' +
                '<td><select class="pack-select" id="pack_' + p.id + '">' + opts + '</select></td>' +
                '<td><div class="qty-wrap">' +
                '<input type="number" class="qty-input" id="qty_' + p.id + '" value="0" min="0"/>' +
                '<span class="units-label" id="lbl_' + p.id + '"></span>' +
                '</div></td>' +
                '</tr>';
        }
        tbody.innerHTML = html;
        document.getElementById('cardProducts').style.display = '';

        // Attach change listeners to update unit labels
        for (var i = 0; i < products.length; i++) {
            (function(pid) {
                var sel = document.getElementById('pack_' + pid);
                var qty = document.getElementById('qty_' + pid);
                var lbl = document.getElementById('lbl_' + pid);
                function updateLabel() {
                    var parts = sel.value.split('|');
                    var units = parseInt(parts[1]) || 1;
                    var q = parseInt(qty.value) || 0;
                    if (q > 0 && units > 1) {
                        lbl.textContent = '= ' + (q * units) + ' pcs';
                    } else {
                        lbl.textContent = '';
                    }
                }
                sel.addEventListener('change', updateLabel);
                qty.addEventListener('input', updateLabel);
            })(products[i].id);
        }
    }

    // Save
    window.saveEntries = function() {
        // Get selected distributor IDs from the ASP ListBox
        var lst = document.getElementById('<%= lstDistributor.ClientID %>');
        if (!lst) { alert('Distributor list not found.'); return; }
        var custIds = [];
        for (var i = 0; i < lst.options.length; i++) {
            if (lst.options[i].selected) custIds.push(lst.options[i].value);
        }
        if (custIds.length === 0) { alert('Please select at least one distributor.'); return; }

        // Collect entries: pid|form|unitsPerPack|qty
        var entries = [];
        for (var i = 0; i < products.length; i++) {
            var pid = products[i].id;
            var qty = parseInt(document.getElementById('qty_' + pid).value) || 0;
            if (qty <= 0) continue;
            var packVal = document.getElementById('pack_' + pid).value; // "JAR|50"
            entries.push(pid + '|' + packVal + '|' + qty);
        }
        if (entries.length === 0) { alert('Please enter quantity for at least one product.'); return; }

        // POST to API
        var xhr = new XMLHttpRequest();
        xhr.open('POST', 'SADailySalesAPI.ashx', true);
        xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
        xhr.onreadystatechange = function() {
            if (xhr.readyState === 4) {
                try {
                    var resp = JSON.parse(xhr.responseText);
                    if (resp.ok) {
                        // Show success
                        var distNames = [];
                        for (var i = 0; i < lst.options.length; i++)
                            if (lst.options[i].selected) distNames.push(lst.options[i].text);
                        showMsg('ok', 'Saved ' + entries.length + ' product(s) across ' + custIds.length +
                            ' distributor(s): ' + distNames.join(', ') +
                            '<br/><small>Entered by: <strong>' + resp.user + '</strong> at ' + new Date().toLocaleTimeString() + '</small>');
                        resetQty();
                    } else {
                        showMsg('err', resp.error || 'Save failed.');
                    }
                } catch(e) {
                    showMsg('err', 'Unexpected error. Please try again.');
                }
            }
        };
        xhr.send('action=save&customerIds=' + encodeURIComponent(custIds.join(',')) +
                 '&entries=' + encodeURIComponent(entries.join(';')) +
                 '&date=' + new Date().toISOString().split('T')[0]);
    };

    window.resetForm = function() {
        // Reset ASP dropdowns via postback would lose JS state. Just reset qty.
        resetQty();
        showMsg('', '');
    };

    function resetQty() {
        for (var i = 0; i < products.length; i++) {
            var pid = products[i].id;
            var qty = document.getElementById('qty_' + pid);
            if (qty) qty.value = '0';
            var lbl = document.getElementById('lbl_' + pid);
            if (lbl) lbl.textContent = '';
        }
    }

    function showMsg(type, msg) {
        var okEl = document.getElementById('msgOk');
        var errEl = document.getElementById('msgErr');
        if (okEl) { okEl.style.display = type === 'ok' ? '' : 'none'; okEl.innerHTML = msg; }
        if (errEl) { errEl.style.display = type === 'err' ? '' : 'none'; errEl.innerHTML = msg; }
    }

    function esc(s) { var d = document.createElement('div'); d.textContent = s; return d.innerHTML; }
})();
</script>
</body>
</html>
