<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MMStockTransfer.aspx.cs" Inherits="MMApp.MMStockTransfer" EnableEventValidation="false" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri MM — Stock Transfer (Store → Floor)</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600;700&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--accent:#cc1e1e;--teal:#1a9e6a;--warn:#e67e22;--text:#1a1a1a;--text-dim:#999;--bg:#f0f0f0;--surface:#fff;--surface2:#faf9f7;--border:#e0e0e0;--radius:14px;}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);color:var(--text);min-height:100vh;}
nav{background:#1a1a1a;height:52px;display:flex;align-items:center;padding:0 20px;gap:12px;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;}
.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.08em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:16px;}
.nav-user{color:rgba(255,255,255,.8);font-size:12px;}
.nav-link{color:#fff;font-size:12px;font-weight:600;text-decoration:none;opacity:.8;}
.nav-link:hover{opacity:1;}
.page-header{background:var(--surface);border-bottom:3px solid var(--accent);padding:20px 30px;display:flex;align-items:center;gap:14px;}
.page-icon{font-size:28px;}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:28px;letter-spacing:.07em;}
.page-title span{color:var(--accent);}
.page-sub{font-size:12px;color:var(--text-dim);}
.main-layout{max-width:1400px;margin:20px auto;padding:0 24px;}
@media(max-width:1100px){.main-layout{padding:0 16px;}}
.card{background:var(--surface);border-radius:var(--radius);padding:20px;margin-bottom:16px;box-shadow:0 1px 4px rgba(0,0,0,.05);}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:17px;letter-spacing:.06em;margin-bottom:14px;padding-bottom:8px;border-bottom:2px solid var(--border);}
.form-grid{display:grid;grid-template-columns:repeat(4,1fr);gap:14px;}
@media(max-width:800px){.form-grid{grid-template-columns:1fr 1fr;}}
.form-group{display:flex;flex-direction:column;gap:4px;}
.form-group label{font-size:11px;font-weight:600;color:#666;letter-spacing:.05em;text-transform:uppercase;}
.form-group input,.form-group select,.form-group textarea{padding:8px 12px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:13px;outline:none;background:#fff;}
.form-group input:focus,.form-group select:focus{border-color:var(--teal);}
.req{color:var(--accent);}
.items-table{width:100%;border-collapse:collapse;font-size:12px;}
.items-table th{padding:8px;text-align:left;font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:#666;background:#fafafa;border-bottom:1.5px solid var(--border);}
.items-table td{padding:6px;border-bottom:1px solid #f0f0f0;vertical-align:middle;}
.items-table tr:hover td{background:#fafafa;}
.items-table .col-type{width:70px;}
.items-table .col-mat{min-width:200px;}
.items-table .col-num{width:90px;}
.items-table .col-uom{width:60px;}
.items-table .col-check{width:60px;text-align:center;}
.items-table .col-act{width:36px;text-align:center;}
.items-table input,.items-table select{padding:6px 8px;border:1.5px solid var(--border);border-radius:6px;font-family:inherit;font-size:12px;outline:none;width:100%;min-width:60px;}
.btn-add{background:var(--teal);color:#fff;border:none;padding:8px 16px;border-radius:8px;font-size:12px;font-weight:600;cursor:pointer;font-family:inherit;}
.btn-remove{background:#fff;color:var(--accent);border:1.5px solid var(--accent);width:28px;height:28px;border-radius:50%;font-size:14px;cursor:pointer;font-family:inherit;display:inline-flex;align-items:center;justify-content:center;padding:0;}
.btn-save{background:var(--teal);color:#fff;border:none;padding:14px 24px;border-radius:10px;font-size:14px;font-weight:700;letter-spacing:.04em;cursor:pointer;font-family:inherit;}
.btn-save:hover{background:#178154;}
.btn-cancel{background:#f5f5f5;color:#333;border:1px solid #ddd;padding:14px 24px;border-radius:10px;font-size:14px;font-weight:600;cursor:pointer;font-family:inherit;}
.summary-bar{display:flex;justify-content:space-between;align-items:center;padding:14px 0;font-size:13px;}
.summary-bar .lbl{color:#666;}
.summary-bar .val{font-weight:700;color:var(--text);}
.alert{padding:10px 14px;border-radius:8px;margin-bottom:14px;font-size:13px;display:none;}
.alert.show{display:block;}
.alert.success{background:#e8f5e9;color:#1e7d3a;border:1px solid #a5d6a7;}
.alert.error{background:#ffebee;color:#b71c1c;border:1px solid #ef9a9a;}
/* Material Picker (text input + 🔍) */
.rm-picker{position:relative;}
.rm-picker-input{position:relative;display:flex;align-items:stretch;}
.rm-picker-input input[type="text"]{flex:1;padding:6px 32px 6px 8px !important;border:1.5px solid var(--border);border-radius:6px;font-family:inherit;font-size:12px;outline:none;min-width:0;width:100%;background:#fff;}
.rm-picker-input input[type="text"]:focus{border-color:var(--teal);}
.rm-picker-btn{position:absolute;right:2px;top:50%;transform:translateY(-50%);width:28px;height:28px;padding:0;border:none;background:transparent;font-size:14px;cursor:pointer;border-radius:4px;line-height:1;}
.rm-picker-btn:hover,.rm-picker-btn:active{background:#f0f0f0;}
.mat-row{padding:12px 14px;border-bottom:1px solid #f0f0f0;cursor:pointer;font-size:14px;border-radius:6px;}
.mat-row:hover,.mat-row:active{background:var(--teal);color:#fff;}
@media(pointer:coarse){.mat-row{padding:14px;font-size:15px;min-height:44px;display:flex;align-items:center;}}
/* History */
.list-card{background:var(--surface);border-radius:var(--radius);padding:20px;margin-bottom:16px;box-shadow:0 1px 4px rgba(0,0,0,.05);}
.transfer-table{width:100%;border-collapse:collapse;font-size:12px;}
.transfer-table th{padding:9px 12px;text-align:left;font-size:10px;font-weight:700;letter-spacing:.1em;text-transform:uppercase;color:var(--text-dim);background:#fafafa;border-bottom:1px solid var(--border);}
.transfer-table td{padding:10px 12px;font-size:12px;border-bottom:1px solid #f0f0f0;vertical-align:middle;}
.transfer-table tr:hover td{background:#fafafa;}
.empty-state{padding:40px;text-align:center;color:#888;font-size:13px;}
</style>
</head>
<body>
<form id="form1" runat="server">
<asp:ScriptManager ID="sm1" runat="server" />

<!-- NAV -->
<nav>
    <div class="nav-logo"><img src="/StockApp/Sirimiri-Logo.png" alt="Sirimiri"/></div>
    <div class="nav-title">SIRIMIRI ERP — MATERIALS MGMT</div>
    <div class="nav-right">
        <span class="nav-user">User: <asp:Label ID="lblUser" runat="server" Text="—"/></span>
        <a href="MMHome.aspx" class="nav-link">Home</a>
        <a href="../StockApp/ERPHome.aspx" class="nav-link">ERP</a>
    </div>
</nav>

<!-- HEADER -->
<div class="page-header">
    <div class="page-icon">📤</div>
    <div>
        <div class="page-title">STOCK <span>TRANSFER</span></div>
        <div class="page-sub">Issue Packing / Consumable / Stationery materials from Stores to Floor</div>
    </div>
</div>

<div class="main-layout">
    <!-- ALERTS -->
    <div id="alertBox" class="alert"></div>

    <!-- HEADER CARD -->
    <div class="card">
        <div class="card-title">Transfer Header</div>
        <div class="form-grid">
            <div class="form-group">
                <label>Transfer Date <span class="req">*</span></label>
                <asp:TextBox ID="txtTransferDate" runat="server" TextMode="Date" />
            </div>
            <div class="form-group">
                <label>From <span class="req">*</span></label>
                <asp:DropDownList ID="ddlFromLocation" runat="server" />
            </div>
            <div class="form-group">
                <label>To <span class="req">*</span></label>
                <asp:DropDownList ID="ddlToLocation" runat="server" />
            </div>
            <div class="form-group">
                <label>Requested By (Floor person)</label>
                <asp:TextBox ID="txtRequestedBy" runat="server" placeholder="e.g. Murali / Shift A" MaxLength="100" />
            </div>
            <div class="form-group" style="grid-column: 1 / -1;">
                <label>Remarks</label>
                <asp:TextBox ID="txtRemarks" runat="server" MaxLength="300" placeholder="Optional notes about this transfer" />
            </div>
        </div>
    </div>

    <!-- LINE ITEMS -->
    <div class="card">
        <div class="card-title">
            Line Items
            <span style="font-size:11px;color:#888;font-weight:400;margin-left:8px;">
                (<span id="lineCount">0</span> items)
            </span>
        </div>
        <div style="overflow-x:auto;">
            <table class="items-table">
                <thead>
                    <tr>
                        <th class="col-type">Type</th>
                        <th class="col-mat">Material</th>
                        <th class="col-num">Quantity</th>
                        <th class="col-uom">UOM</th>
                        <th class="col-act"></th>
                    </tr>
                </thead>
                <tbody id="tbodyItems"></tbody>
            </table>
        </div>
        <div style="margin-top:12px;">
            <button type="button" class="btn-add" onclick="addRow();">+ Add Line</button>
        </div>
    </div>

    <!-- ACTIONS -->
    <div class="card" style="display:flex;gap:12px;justify-content:flex-end;">
        <button type="button" class="btn-cancel" onclick="clearAll();">Clear</button>
        <button type="button" class="btn-save" onclick="saveTransfer();">📤 Save Transfer</button>
    </div>

    <!-- HIDDEN FIELDS -->
    <asp:HiddenField ID="hfLineItems" runat="server" Value="[]" />

    <!-- HISTORY -->
    <div class="list-card">
        <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:14px;">
            <div class="card-title" style="margin-bottom:0;border-bottom:none;padding-bottom:0;">Recent Transfers</div>
            <div style="display:flex;gap:8px;align-items:center;">
                <asp:TextBox ID="txtFilterFrom" runat="server" TextMode="Date" />
                <asp:TextBox ID="txtFilterTo"   runat="server" TextMode="Date" />
                <asp:Button  ID="btnFilter"     runat="server" Text="Filter" OnClick="btnFilter_Click"
                             CssClass="btn-add" Style="padding:8px 16px;"/>
            </div>
        </div>
        <div style="overflow-x:auto;">
            <asp:Repeater ID="rptTransfers" runat="server">
                <HeaderTemplate>
                    <table class="transfer-table">
                        <thead><tr>
                            <th>Transfer No</th>
                            <th>Date</th>
                            <th>From → To</th>
                            <th>Requested By</th>
                            <th>Issued By</th>
                            <th style="text-align:center;">Lines</th>
                            <th>Status</th>
                        </tr></thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td style="font-weight:600;color:var(--teal);"><%# Eval("TransferNo") %></td>
                        <td><%# Eval("TransferDate","{0:dd-MMM-yyyy}") %></td>
                        <td><%# Eval("FromLocation") %> → <%# Eval("ToLocation") %></td>
                        <td><%# Eval("RequestedBy") %></td>
                        <td><%# Eval("IssuedByName") %></td>
                        <td style="text-align:center;font-weight:600;"><%# Eval("LineCount") %></td>
                        <td><%# Eval("Status") %></td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                        </tbody>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
        </div>
        <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
            <div class="empty-state">No transfers found for the selected period.</div>
        </asp:Panel>
    </div>
</div>

<!-- Material Search Modal (shared across rows) -->
<div id="matSearchOverlay" style="display:none;position:fixed;inset:0;background:rgba(0,0,0,.5);z-index:10000;align-items:flex-start;justify-content:center;padding-top:40px;">
    <div style="background:#fff;border-radius:14px;max-width:560px;width:95%;max-height:85vh;display:flex;flex-direction:column;box-shadow:0 16px 48px rgba(0,0,0,.25);overflow:hidden;">
        <div style="padding:20px 24px 12px;border-bottom:2px solid var(--teal);">
            <div style="font-family:'Bebas Neue',sans-serif;font-size:20px;letter-spacing:.06em;margin-bottom:12px;">
                Search Material — <span id="matSearchType">PM</span>
            </div>
            <input type="text" id="matSearchInput" placeholder="🔍 Type to filter…" oninput="filterMaterialList();"
                   style="width:100%;padding:12px 14px;font-size:16px;border:1.5px solid #ddd;border-radius:8px;outline:none;font-family:inherit;" autocomplete="off"/>
        </div>
        <div id="matSearchList" style="flex:1;overflow-y:auto;padding:8px;"></div>
        <div style="padding:12px 24px;border-top:1px solid #eee;text-align:right;">
            <button type="button" onclick="closeMaterialSearch();"
                    style="padding:10px 20px;border:1px solid #ddd;border-radius:8px;background:#f5f5f5;color:#333;font-size:13px;font-weight:600;cursor:pointer;font-family:inherit;">Cancel</button>
        </div>
    </div>
</div>

<script>
    // Material catalogs injected from server
    var pmOptions = <%= PMOptionsJson %>;
    var cnOptions = <%= CNOptionsJson %>;
    var stOptions = <%= STOptionsJson %>;

    function getCatalog(type) {
        if (type === 'PM') return pmOptions;
        if (type === 'CN') return cnOptions;
        if (type === 'ST') return stOptions;
        return [];
    }

    // ── Line item management ──
    var rowIdx = 0;

    function addRow() {
        rowIdx++;
        var tbody = document.getElementById('tbodyItems');
        var tr = document.createElement('tr');
        tr.id = 'row_' + rowIdx;
        tr.dataset.idx = rowIdx;
        tr.innerHTML =
            '<td class="col-type">'
              + '<select id="type_' + rowIdx + '" onchange="onTypeChange(' + rowIdx + ');">'
              + '<option value="PM">PM</option>'
              + '<option value="CN">CN</option>'
              + '<option value="ST">ST</option>'
              + '</select>'
            + '</td>'
            + '<td class="col-mat" id="matCell_' + rowIdx + '"></td>'
            + '<td class="col-num"><input type="number" id="qty_' + rowIdx + '" step="0.001" min="0" placeholder="0"/></td>'
            + '<td class="col-uom"><span id="uom_' + rowIdx + '" style="color:#666;font-size:11px;">—</span></td>'
            + '<td class="col-act"><button type="button" class="btn-remove" onclick="removeRow(' + rowIdx + ');">✕</button></td>';
        tbody.appendChild(tr);
        // Build initial picker for default type (PM)
        rebuildPicker(rowIdx, 'PM');
        updateCount();
    }

    function removeRow(idx) {
        var row = document.getElementById('row_' + idx);
        if (row) row.remove();
        updateCount();
    }

    function onTypeChange(idx) {
        var sel = document.getElementById('type_' + idx);
        rebuildPicker(idx, sel.value);
        // Clear UOM display
        document.getElementById('uom_' + idx).innerText = '—';
    }

    function rebuildPicker(idx, type) {
        var cell = document.getElementById('matCell_' + idx);
        if (!cell) return;
        var listId = 'matList_' + idx;
        var dispId = 'matDisplay_' + idx;
        var hidId  = 'mat_' + idx;
        var catalog = getCatalog(type);
        var html = ''
            + '<div class="rm-picker">'
            +   '<div class="rm-picker-input">'
            +     '<input type="text" id="' + dispId + '" list="' + listId + '" '
            +            'placeholder="🔍 Tap to search ' + type + '…" autocomplete="off" '
            +            'oninput="onMatTypeahead(' + idx + ');" onfocus="this.select();" />'
            +     '<button type="button" class="rm-picker-btn" '
            +            'onclick="openMaterialSearch(' + idx + ');" title="Search">🔍</button>'
            +   '</div>'
            +   '<datalist id="' + listId + '">';
        for (var i = 0; i < catalog.length; i++) {
            html += '<option value="' + (catalog[i].name || '').replace(/"/g, '&quot;') + '"></option>';
        }
        html +=   '</datalist>'
            +   '<input type="hidden" id="' + hidId + '" value="0"/>'
            + '</div>';
        cell.innerHTML = html;
    }

    function onMatTypeahead(idx) {
        var disp = document.getElementById('matDisplay_' + idx);
        var hid  = document.getElementById('mat_' + idx);
        if (!disp || !hid) return;
        var typed = (disp.value || '').trim().toLowerCase();
        var typeSel = document.getElementById('type_' + idx);
        var catalog = getCatalog(typeSel.value);
        if (!typed) { hid.value = '0'; document.getElementById('uom_' + idx).innerText = '—'; return; }
        for (var i = 0; i < catalog.length; i++) {
            if ((catalog[i].name || '').toLowerCase() === typed) {
                hid.value = String(catalog[i].id);
                document.getElementById('uom_' + idx).innerText = catalog[i].uom || '—';
                return;
            }
        }
    }

    function updateCount() {
        document.getElementById('lineCount').innerText = document.querySelectorAll('#tbodyItems tr').length;
    }

    // ── Material Search Modal ──
    var _matSearchActiveIdx = null;
    function openMaterialSearch(idx) {
        _matSearchActiveIdx = idx;
        var typeSel = document.getElementById('type_' + idx);
        var typeLbl = document.getElementById('matSearchType');
        if (typeLbl) typeLbl.textContent = typeSel ? typeSel.value : '';
        var ov = document.getElementById('matSearchOverlay');
        var inp = document.getElementById('matSearchInput');
        if (!ov || !inp) return;
        inp.value = '';
        renderMaterialList('');
        ov.style.display = 'flex';
        setTimeout(function(){ try { inp.focus(); } catch(e){} }, 60);
    }
    function closeMaterialSearch() {
        var ov = document.getElementById('matSearchOverlay');
        if (ov) ov.style.display = 'none';
        _matSearchActiveIdx = null;
    }
    function filterMaterialList() {
        var inp = document.getElementById('matSearchInput');
        renderMaterialList(inp ? inp.value : '');
    }
    function renderMaterialList(filter) {
        var listEl = document.getElementById('matSearchList');
        if (!listEl) return;
        var f = (filter || '').trim().toLowerCase();
        var typeSel = document.getElementById('type_' + _matSearchActiveIdx);
        var catalog = typeSel ? getCatalog(typeSel.value) : [];
        var html = '';
        var shown = 0;
        for (var i = 0; i < catalog.length; i++) {
            var nm = catalog[i].name || '';
            if (f && nm.toLowerCase().indexOf(f) === -1) continue;
            html += '<div class="mat-row" onclick="applyMaterialPick(' + catalog[i].id + ');">'
                  + nm.replace(/</g, '&lt;') + '</div>';
            shown++;
            if (shown > 200) break;
        }
        if (shown === 0) html = '<div style="padding:24px;text-align:center;color:#888;font-size:13px;">No materials match.</div>';
        listEl.innerHTML = html;
    }
    function applyMaterialPick(materialId) {
        var idx = _matSearchActiveIdx;
        if (idx === null) { closeMaterialSearch(); return; }
        var typeSel = document.getElementById('type_' + idx);
        var catalog = getCatalog(typeSel.value);
        var picked = null;
        for (var i = 0; i < catalog.length; i++) {
            if (catalog[i].id === materialId) { picked = catalog[i]; break; }
        }
        if (picked) {
            var hid = document.getElementById('mat_' + idx);
            var disp = document.getElementById('matDisplay_' + idx);
            if (hid) hid.value = String(materialId);
            if (disp) disp.value = picked.name;
            document.getElementById('uom_' + idx).innerText = picked.uom || '—';
        }
        closeMaterialSearch();
    }

    // ── Save / Cancel ──
    function showAlert(msg, isSuccess) {
        var el = document.getElementById('alertBox');
        el.textContent = msg;
        el.className = 'alert show ' + (isSuccess ? 'success' : 'error');
        if (isSuccess) setTimeout(function(){ el.classList.remove('show'); }, 5000);
        window.scrollTo(0, 0);
    }

    function clearAll() {
        if (!confirm('Clear all line items?')) return;
        document.getElementById('tbodyItems').innerHTML = '';
        rowIdx = 0;
        updateCount();
    }

    function saveTransfer() {
        // Validate rows
        var rows = document.querySelectorAll('#tbodyItems tr');
        if (rows.length === 0) { showAlert('Add at least one line item.', false); return; }
        var lines = [];
        var problems = [];
        rows.forEach(function(tr) {
            var idx = tr.dataset.idx;
            var type = (document.getElementById('type_' + idx).value || '').trim();
            var matId = parseInt(document.getElementById('mat_' + idx).value || '0', 10);
            var qty   = parseFloat(document.getElementById('qty_' + idx).value || '0');
            if (!matId)         problems.push('Row ' + idx + ': pick a material');
            else if (!(qty > 0)) problems.push('Row ' + idx + ': quantity must be > 0');
            else lines.push({ type: type, matId: matId, qty: qty });
        });
        if (problems.length > 0) { showAlert(problems.join('; '), false); return; }

        // Validate header
        var fromId = document.getElementById('<%= ddlFromLocation.ClientID %>').value;
        var toId   = document.getElementById('<%= ddlToLocation.ClientID %>').value;
        if (!fromId || !toId)   { showAlert('From and To locations are required.', false); return; }
        if (fromId === toId)    { showAlert('From and To locations cannot be the same.', false); return; }

        // Stuff payload into hidden field, postback
        document.getElementById('<%= hfLineItems.ClientID %>').value = JSON.stringify(lines);
        __doPostBack('SAVE_TRANSFER', '');
    }
</script>

<asp:Button ID="btnSavePostback" runat="server" Text="" Style="display:none;" OnClick="btnSavePostback_Click" />
<script>
    // Hook the JS-driven "SAVE_TRANSFER" event up to our hidden postback button
    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_initializeRequest(function(){});
    }
    // ASP.NET WebForms: __doPostBack with custom argument; intercept in code-behind via Request["__EVENTTARGET"]
</script>
</form>
</body>
</html>
