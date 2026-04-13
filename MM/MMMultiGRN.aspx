<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MMMultiGRN.aspx.cs" Inherits="MMApp.MMMultiGRN" EnableEventValidation="false" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri MM — Multi-Item GRN</title>
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
.main-layout{max-width:1400px;margin:20px auto;padding:0 24px;display:grid;grid-template-columns:1fr 320px;gap:20px;align-items:start;}
@media(max-width:1100px){.main-layout{grid-template-columns:1fr;}}
.card{background:var(--surface);border-radius:var(--radius);padding:20px;margin-bottom:16px;box-shadow:0 1px 4px rgba(0,0,0,.05);}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:17px;letter-spacing:.06em;margin-bottom:14px;padding-bottom:8px;border-bottom:2px solid var(--border);}
.header-grid{display:grid;grid-template-columns:1fr 1fr 1fr 1fr;gap:12px;}
@media(max-width:800px){.header-grid{grid-template-columns:1fr 1fr;}}
.form-group{display:flex;flex-direction:column;gap:3px;}
.form-group label{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);}
.form-group input,.form-group select{padding:8px 10px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:12px;outline:none;}
.form-group input:focus,.form-group select:focus{border-color:var(--accent);}
.req{color:var(--accent);}
/* Line items table */
.items-table{width:100%;border-collapse:collapse;font-size:12px;}
.items-table thead th{font-size:9px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);padding:8px 6px;text-align:left;border-bottom:2px solid var(--border);background:var(--surface2);white-space:nowrap;}
.items-table tbody td{padding:6px 4px;border-bottom:1px solid #f2f0ed;vertical-align:top;}
.items-table tbody tr:nth-child(even){background:var(--surface2);}
.items-table input,.items-table select{padding:6px 8px;border:1.5px solid var(--border);border-radius:6px;font-family:inherit;font-size:12px;outline:none;width:100%;min-width:60px;}
.items-table input:focus,.items-table select:focus{border-color:var(--accent);}
.items-table .col-rm{min-width:200px;}
.items-table .col-num{width:90px;}
.items-table .col-sm{width:70px;}
.items-table .col-check{width:40px;text-align:center;}
.items-table .col-act{width:36px;text-align:center;}
.btn-remove{background:none;border:none;color:#e74c3c;font-size:18px;cursor:pointer;padding:2px 6px;border-radius:4px;}
.btn-remove:hover{background:#fdf3f2;}
.btn-add-row{padding:8px 20px;background:var(--teal);color:#fff;border:none;border-radius:8px;font-size:12px;font-weight:700;cursor:pointer;margin-top:10px;}
.btn-add-row:hover{opacity:.9;}
/* Footer */
.totals-bar{display:flex;gap:20px;align-items:center;justify-content:flex-end;padding:14px 0;border-top:2px solid var(--border);margin-top:12px;flex-wrap:wrap;}
.total-chip{font-size:12px;color:var(--text-dim);}
.total-chip strong{font-size:15px;color:var(--text);}
.total-grand{font-size:13px;font-weight:700;color:var(--teal);padding:8px 16px;background:#e8f5e9;border-radius:8px;}
.btn-save{padding:12px 32px;background:var(--teal);color:#fff;border:none;border-radius:10px;font-size:14px;font-weight:700;cursor:pointer;font-family:inherit;}
.btn-save:hover{opacity:.9;}
.btn-clear{padding:12px 20px;background:#f5f5f5;color:#333;border:1px solid var(--border);border-radius:10px;font-size:14px;font-weight:700;cursor:pointer;}
/* Right panel */
.rec-panel{background:var(--surface);border-radius:var(--radius);padding:16px;box-shadow:0 1px 4px rgba(0,0,0,.05);margin-bottom:16px;}
.rec-header{margin-bottom:12px;}
.rec-title{font-family:'Bebas Neue',sans-serif;font-size:16px;letter-spacing:.06em;}
.rec-sub{font-size:11px;color:var(--text-dim);}
.rec-item{padding:10px 0;border-bottom:1px solid #f2f0ed;}
.rec-empty{text-align:center;padding:20px;color:var(--text-dim);font-size:12px;}
/* Alert */
.alert-ok{background:#d1f5e0;color:#155724;border:1px solid #a3d9b1;padding:12px 18px;border-radius:8px;font-size:13px;margin-bottom:14px;}
.alert-err{background:#fdf3f2;color:#842029;border:1px solid #f5c2c7;padding:12px 18px;border-radius:8px;font-size:13px;margin-bottom:14px;}
.search-input{margin-bottom:4px;padding:8px 12px;border:1.5px solid #e0e0e0;border-radius:8px;font-size:12px;background:#fffdf5 !important;color:#0f0f0f !important;outline:none;width:100%;cursor:pointer !important;}
</style>
</head>
<body>
<form id="form1" runat="server">
<asp:HiddenField ID="hfLineItems" runat="server" Value="[]"/>
<asp:HiddenField ID="hfSupplierID" runat="server" Value="0"/>

<nav>
    <div class="nav-logo"><img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="" onerror="this.style.display='none'"/></div>
    <span class="nav-title">MATERIALS MANAGEMENT</span>
    <div class="nav-right">
        <asp:Label ID="lblNavUser" runat="server" CssClass="nav-user"/>
        <a href="MMHome.aspx" class="nav-link">&#x2190; MM Home</a>
        <a href="MMRawInward.aspx" class="nav-link">Single GRN</a>
    </div>
</nav>

<div class="page-header">
    <span class="page-icon">&#x1F4E6;</span>
    <div>
        <div class="page-title">Multi-Item <span>GRN</span> — Raw Materials</div>
        <div class="page-sub">Record multiple raw materials from a single invoice</div>
    </div>
</div>

<div class="main-layout">
<div>
    <!-- ALERT -->
    <asp:Panel ID="pnlAlert" runat="server" Visible="false"><asp:Literal ID="litAlert" runat="server"/></asp:Panel>

    <!-- INVOICE HEADER -->
    <div class="card">
        <div class="card-title">Invoice Details</div>
        <div class="header-grid">
            <div class="form-group" style="position:relative;">
                <label>Supplier <span class="req">*</span></label>
                <input type="text" id="txtSupSearch" placeholder="&#128269; Tap to search supplier..." readonly
                    onfocus="openSearchModal(this, '<%= ddlSupplier.ClientID %>', 'txtSupSearch', 'Supplier');"
                    class="search-input" autocomplete="off"/>
                <asp:DropDownList ID="ddlSupplier" runat="server" style="display:none;"/>
            </div>
            <div class="form-group">
                <label>Invoice No</label>
                <input type="text" id="txtInvoiceNo" maxlength="50" placeholder="e.g. INV-2024-001"/>
            </div>
            <div class="form-group">
                <label>Invoice Date</label>
                <input type="date" id="txtInvoiceDate"/>
            </div>
            <div class="form-group">
                <label>GRN Date <span class="req">*</span></label>
                <input type="date" id="txtGRNDate"/>
            </div>
        </div>
        <div class="header-grid" style="margin-top:10px;">
            <div class="form-group">
                <label>Transport Cost (Total)</label>
                <input type="number" id="txtTransport" step="0.01" min="0" placeholder="0.00" oninput="recalcAll();"/>
            </div>
            <div class="form-group" style="align-self:end;">
                <label style="display:flex;align-items:center;gap:6px;">
                    <input type="checkbox" id="chkTransInInvoice" onchange="recalcAll();" style="width:15px;height:15px;"/>
                    Transport in invoice
                </label>
            </div>
            <div class="form-group" style="align-self:end;">
                <label style="display:flex;align-items:center;gap:6px;">
                    <input type="checkbox" id="chkTransGST" onchange="recalcAll();" style="width:15px;height:15px;"/>
                    Transport attracts GST
                </label>
            </div>
            <div class="form-group" style="align-self:end;">
                <label style="display:flex;align-items:center;gap:6px;">
                    <input type="checkbox" id="chkManualInvoice" onchange="toggleManualInvoice();" style="width:15px;height:15px;accent-color:#e67e22;"/>
                    <span style="color:#e67e22;font-weight:600;">Manual Invoice</span>
                </label>
            </div>
        </div>
    </div>

    <!-- LINE ITEMS -->
    <div class="card">
        <div class="card-title">Line Items (<span id="itemCount">1</span>)</div>
        <div style="overflow-x:auto;">
            <table class="items-table" id="tblItems">
                <thead><tr>
                    <th class="col-rm">Raw Material *</th>
                    <th class="col-num">Inv Qty *</th>
                    <th class="col-sm">UOM</th>
                    <th class="col-num">Act Qty *</th>
                    <th class="col-num">Std Qty *</th>
                    <th class="col-sm">Std UOM</th>
                    <th class="col-num">Rate *</th>
                    <th class="col-sm">HSN</th>
                    <th class="col-sm">GST%</th>
                    <th class="col-num">Amount</th>
                    <th class="col-check">QC</th>
                    <th class="col-act"></th>
                </tr></thead>
                <tbody id="tbodyItems"></tbody>
            </table>
        </div>
        <button type="button" class="btn-add-row" onclick="addRow();">+ Add Material</button>

        <!-- Totals -->
        <div class="totals-bar">
            <div class="total-chip">Items: <strong id="dispItemCount">0</strong></div>
            <div class="total-chip">Subtotal: <strong id="dispSubtotal">Rs. 0.00</strong></div>
            <div class="total-chip">GST: <strong id="dispGST">Rs. 0.00</strong></div>
            <div class="total-chip">Transport: <strong id="dispTransport">Rs. 0.00</strong></div>
            <div class="total-grand">Grand Total: Rs. <span id="dispGrand">0.00</span></div>
        </div>

        <div style="display:flex;gap:12px;margin-top:16px;justify-content:flex-end;">
            <button type="button" class="btn-clear" onclick="clearAll();">Clear All</button>
            <asp:Button ID="btnSave" runat="server" Text="&#x2714; Save All GRNs" CssClass="btn-save"
                OnClick="btnSave_Click" OnClientClick="return prepareSubmit();" CausesValidation="false"/>
        </div>
    </div>
</div>

<!-- RIGHT PANEL -->
<div>
    <!-- Pending Invoices -->
    <asp:Panel ID="pnlPendingPanel" runat="server">
    <div class="rec-panel" style="border-left:3px solid #e67e22;">
        <div class="rec-header">
            <div class="rec-title" style="color:#e67e22;">&#x23F3; Pending Invoices</div>
            <div class="rec-sub">GRNs received without invoice</div>
        </div>
        <asp:Panel ID="pnlPendingEmpty" runat="server"><div class="rec-empty" style="color:#2ecc71;">&#10003; No pending invoices</div></asp:Panel>
        <asp:Panel ID="pnlPendingList" runat="server" Visible="false">
            <div style="max-height:250px;overflow-y:auto;">
                <asp:Repeater ID="rptPending" runat="server">
                    <ItemTemplate>
                        <div class="rec-item" style="border-left:3px solid #e67e22;padding-left:10px;">
                            <div style="font-weight:600;font-size:12px;"><%# Eval("SupplierName") %></div>
                            <div style="font-size:11px;color:var(--text-dim);"><%# Eval("RMName") %> — <%# Eval("GRNNo") %></div>
                            <div style="display:flex;justify-content:space-between;">
                                <span style="font-size:10px;color:var(--text-dim);"><%# Convert.ToDateTime(Eval("InwardDate")).ToString("dd-MMM-yyyy") %></span>
                                <span style="font-weight:700;font-size:12px;">Rs. <%# Convert.ToDecimal(Eval("Amount")).ToString("N2") %></span>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </asp:Panel>
    </div>
    </asp:Panel>

    <!-- Supplier Recoverables -->
    <div class="rec-panel">
        <div class="rec-header">
            <div class="rec-title">Supplier Recoverables</div>
            <div class="rec-sub">Pending shortage recovery</div>
            <div style="font-size:12px;font-weight:600;margin-top:4px;"><asp:Label ID="lblRecSupplier" runat="server" Text="— Select a supplier —"/></div>
        </div>
        <asp:Panel ID="pnlRecEmpty" runat="server"><div class="rec-empty">Select a supplier to view recoverables</div></asp:Panel>
        <asp:Panel ID="pnlRecList" runat="server" Visible="false">
            <div style="max-height:250px;overflow-y:auto;">
                <asp:Repeater ID="rptRecoverables" runat="server">
                    <ItemTemplate>
                        <div class="rec-item">
                            <div style="font-size:11px;font-weight:500;"><%# Eval("RMName") %></div>
                            <div style="display:flex;justify-content:space-between;font-size:11px;">
                                <span style="color:var(--text-dim);"><%# Eval("GRNNo") %> — <%# Eval("InwardDate","{0:dd-MMM-yy}") %></span>
                                <span style="color:#e74c3c;font-weight:600;"><%# Eval("ShortageQty") %> short</span>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <div style="display:flex;justify-content:space-between;padding:10px 0 0;border-top:2px solid var(--teal);font-size:12px;font-weight:700;">
                <span>Total Recoverable</span>
                <span style="color:var(--teal);">Rs. <asp:Label ID="lblRecTotal" runat="server" Text="0.00"/></span>
            </div>
        </asp:Panel>
    </div>
</div>
</div>

<asp:Button ID="btnSupplierTrigger" runat="server" style="display:none" OnClick="btnSupplierTrigger_Click"/>
</form>

<script>
    // ── RM data for auto-fill HSN/GST/UOM ──
    var rmData = <%= RMDataJson %>;

    // ── RM options for dropdown ──
    var rmOptions = <%= RMOptionsJson %>;

    // ── UOM options for dropdown ──
    var uomOptions = <%= UOMOptionsJson %>;

    // ── Modal Search (reused) ──
    var _modalOverlay = null;
    function openSearchModal(searchInput, ddlId, searchId, title) {
        searchInput.blur();
        var ddl = document.getElementById(ddlId);
        if (!ddl) return;
        var items = [];
        for (var i = 0; i < ddl.options.length; i++) {
            if (ddl.options[i].value === '0') continue;
            items.push({ value: ddl.options[i].value, text: ddl.options[i].text, idx: i });
        }
        if (_modalOverlay) _modalOverlay.remove();
        var ov = document.createElement('div');
        ov.id = 'searchOverlay';
        ov.style.cssText = 'position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,.5);z-index:9999;display:flex;align-items:flex-start;justify-content:center;padding:40px 16px 0;';
        var box = document.createElement('div');
        box.style.cssText = 'background:#fff;border-radius:14px;width:100%;max-width:540px;max-height:80vh;display:flex;flex-direction:column;box-shadow:0 8px 40px rgba(0,0,0,.25);overflow:hidden;';
        var hdr = document.createElement('div');
        hdr.style.cssText = 'padding:16px 20px 12px;border-bottom:2px solid #f0ede8;display:flex;align-items:center;justify-content:space-between;';
        hdr.innerHTML = '<span style="font-family:\'Bebas Neue\',sans-serif;font-size:18px;letter-spacing:.06em;">Select ' + title + '</span>';
        var closeBtn = document.createElement('button'); closeBtn.type='button'; closeBtn.innerHTML='✕';
        closeBtn.style.cssText = 'border:none;background:none;font-size:20px;cursor:pointer;color:#999;padding:4px 8px;';
        closeBtn.onclick = function() { ov.remove(); _modalOverlay = null; };
        hdr.appendChild(closeBtn); box.appendChild(hdr);
        var sWrap = document.createElement('div'); sWrap.style.cssText = 'padding:12px 20px;';
        var sInput = document.createElement('input'); sInput.type='text'; sInput.placeholder='Search '+title.toLowerCase()+'...';
        sInput.style.cssText = 'width:100%;padding:12px 16px;border:2px solid #e0e0e0;border-radius:10px;font-size:16px;font-family:\'DM Sans\',sans-serif;outline:none;background:#fafafa;';
        sInput.setAttribute('autocomplete','off');
        sWrap.appendChild(sInput); box.appendChild(sWrap);
        var list = document.createElement('div');
        list.style.cssText = 'flex:1;overflow-y:auto;padding:0 8px 12px;-webkit-overflow-scrolling:touch;';
        function renderList(query) {
            list.innerHTML = ''; var q = (query||'').toLowerCase().trim(); var count = 0;
            items.forEach(function(it) {
                if (q && it.text.toLowerCase().indexOf(q) < 0) return; count++;
                var row = document.createElement('div');
                row.style.cssText = 'padding:12px 14px;border-radius:8px;cursor:pointer;font-size:14px;margin:2px 0;';
                row.onmouseenter = function(){row.style.background='#f5f5f0';}; row.onmouseleave = function(){row.style.background='';};
                if (q) { var idx=it.text.toLowerCase().indexOf(q); row.innerHTML=it.text.substring(0,idx)+'<strong style="color:var(--accent);">'+it.text.substring(idx,idx+q.length)+'</strong>'+it.text.substring(idx+q.length); }
                else { row.textContent = it.text; }
                row.onclick = function() {
                    ddl.selectedIndex = it.idx; var sb=document.getElementById(searchId); if(sb)sb.value=it.text;
                    ov.remove(); _modalOverlay = null;
                    // Trigger supplier change
                    if (ddlId.indexOf('ddlSupplier') >= 0) onSupplierSelect(it.value);
                };
                list.appendChild(row);
            });
            if (count===0) { var empty=document.createElement('div'); empty.style.cssText='padding:20px;text-align:center;color:#999;font-size:13px;'; empty.textContent='No results found'; list.appendChild(empty); }
        }
        box.appendChild(list); ov.appendChild(box); document.body.appendChild(ov); _modalOverlay = ov;
        ov.onclick = function(e){if(e.target===ov){ov.remove();_modalOverlay=null;}};
        renderList('');
        setTimeout(function(){sInput.focus();},150);
        sInput.oninput = function(){renderList(sInput.value);};
    }

    function onSupplierSelect(supId) {
        document.getElementById('<%= hfSupplierID.ClientID %>').value = supId;
        __doPostBack('<%= btnSupplierTrigger.UniqueID %>', '');
    }

    // ── Manual Invoice ──
    function toggleManualInvoice() {
        var chk = document.getElementById('chkManualInvoice').checked;
        var inv = document.getElementById('txtInvoiceNo');
        var invDt = document.getElementById('txtInvoiceDate');
        if (chk) { inv.value='MANUAL INVOICE'; inv.readOnly=true; inv.style.background='#f0f0f0'; inv.style.color='#999';
            if(invDt){invDt.value='';invDt.readOnly=true;invDt.style.background='#f0f0f0';}
        } else { inv.value=''; inv.readOnly=false; inv.style.background=''; inv.style.color='';
            if(invDt){invDt.readOnly=false;invDt.style.background='';}
        }
    }

    // ── Line Item Management ──
    var rowIdx = 0;

    function buildRMSelect(idx) {
        var html = '<select id="rm_'+idx+'" onchange="onRMSelect('+idx+');" style="min-width:180px;">';
        html += '<option value="0">-- Select --</option>';
        for (var i = 0; i < rmOptions.length; i++)
            html += '<option value="'+rmOptions[i].id+'">'+rmOptions[i].name+'</option>';
        html += '</select>';
        return html;
    }

    function buildUOMSelect(idx, prefix) {
        var html = '<select id="'+prefix+'_'+idx+'" style="width:70px;">';
        for (var i = 0; i < uomOptions.length; i++)
            html += '<option value="'+uomOptions[i].id+'">'+uomOptions[i].name+'</option>';
        html += '</select>';
        return html;
    }

    function addRow() {
        rowIdx++;
        var tbody = document.getElementById('tbodyItems');
        var tr = document.createElement('tr'); tr.id = 'row_'+rowIdx; tr.dataset.idx = rowIdx;
        tr.innerHTML =
            '<td class="col-rm">'+buildRMSelect(rowIdx)+'</td>' +
            '<td class="col-num"><input type="number" id="qtyInv_'+rowIdx+'" step="0.001" min="0" placeholder="" oninput="calcRow('+rowIdx+');"/></td>' +
            '<td class="col-sm">'+buildUOMSelect(rowIdx, 'invUom')+'</td>' +
            '<td class="col-num"><input type="number" id="qtyAct_'+rowIdx+'" step="0.001" min="0" placeholder=""/></td>' +
            '<td class="col-num"><input type="number" id="qtyUom_'+rowIdx+'" step="0.001" min="0" placeholder=""/></td>' +
            '<td class="col-sm">'+buildUOMSelect(rowIdx, 'stdUom')+'</td>' +
            '<td class="col-num"><input type="number" id="rate_'+rowIdx+'" step="0.01" min="0" placeholder="" oninput="calcRow('+rowIdx+');"/></td>' +
            '<td class="col-sm"><input type="text" id="hsn_'+rowIdx+'" maxlength="10" style="width:60px;"/></td>' +
            '<td class="col-sm"><input type="number" id="gst_'+rowIdx+'" step="0.01" min="0" placeholder="0" oninput="calcRow('+rowIdx+');"/></td>' +
            '<td class="col-num"><span id="amt_'+rowIdx+'" style="font-weight:600;">0.00</span></td>' +
            '<td class="col-check"><input type="checkbox" id="qc_'+rowIdx+'" checked style="width:16px;height:16px;"/></td>' +
            '<td class="col-act"><button type="button" class="btn-remove" onclick="removeRow('+rowIdx+');">✕</button></td>';
        tbody.appendChild(tr);
        updateCount();
    }

    function removeRow(idx) {
        var row = document.getElementById('row_'+idx);
        if (row) row.remove();
        recalcAll();
        updateCount();
    }

    function selectUOMById(selectId, uomId) {
        var ddl = document.getElementById(selectId);
        if (!ddl || !uomId) return;
        for (var i = 0; i < ddl.options.length; i++) {
            if (ddl.options[i].value === uomId) { ddl.selectedIndex = i; break; }
        }
    }

    function onRMSelect(idx) {
        var sel = document.getElementById('rm_'+idx);
        var d = rmData[sel.value];
        if (d) {
            document.getElementById('hsn_'+idx).value = d.hsn || '';
            document.getElementById('gst_'+idx).value = d.gst || '';
            // Auto-select UOM dropdowns to match RM master UOM
            selectUOMById('invUom_'+idx, d.uomId);
            selectUOMById('stdUom_'+idx, d.uomId);
        }
        calcRow(idx);
    }

    function calcRow(idx) {
        var qtyInv = parseFloat(document.getElementById('qtyInv_'+idx).value) || 0;
        var rate = parseFloat(document.getElementById('rate_'+idx).value) || 0;
        var amt = qtyInv * rate;
        document.getElementById('amt_'+idx).innerText = amt.toFixed(2);
        recalcAll();
    }

    function recalcAll() {
        var rows = document.querySelectorAll('#tbodyItems tr');
        var subtotal = 0, totalGST = 0, count = 0;
        rows.forEach(function(tr) {
            var idx = tr.dataset.idx;
            var qtyInv = parseFloat(document.getElementById('qtyInv_'+idx)?.value) || 0;
            var rate = parseFloat(document.getElementById('rate_'+idx)?.value) || 0;
            var gstRate = parseFloat(document.getElementById('gst_'+idx)?.value) || 0;
            var lineAmt = qtyInv * rate;
            var lineGST = lineAmt * (gstRate / 100);
            subtotal += lineAmt;
            totalGST += lineGST;
            count++;
        });
        var transport = parseFloat(document.getElementById('txtTransport').value) || 0;
        var transInInv = document.getElementById('chkTransInInvoice').checked;
        var transGST = document.getElementById('chkTransGST').checked;

        var grand = subtotal + totalGST + transport;

        document.getElementById('dispItemCount').innerText = count;
        document.getElementById('dispSubtotal').innerText = 'Rs. ' + subtotal.toFixed(2);
        document.getElementById('dispGST').innerText = 'Rs. ' + totalGST.toFixed(2);
        document.getElementById('dispTransport').innerText = 'Rs. ' + transport.toFixed(2);
        document.getElementById('dispGrand').innerText = grand.toFixed(2);
    }

    function updateCount() {
        var count = document.querySelectorAll('#tbodyItems tr').length;
        document.getElementById('itemCount').innerText = count;
        document.getElementById('dispItemCount').innerText = count;
    }

    function clearAll() {
        document.getElementById('tbodyItems').innerHTML = '';
        document.getElementById('txtInvoiceNo').value = '';
        document.getElementById('txtInvoiceDate').value = '';
        document.getElementById('txtTransport').value = '';
        document.getElementById('chkTransInInvoice').checked = false;
        document.getElementById('chkTransGST').checked = false;
        document.getElementById('chkManualInvoice').checked = false;
        toggleManualInvoice();
        rowIdx = 0;
        addRow();
        recalcAll();
    }

    // ── Prepare for submit ──
    function prepareSubmit() {
        var rows = document.querySelectorAll('#tbodyItems tr');
        var items = [];
        var hasError = false;

        // Validate header
        var grnDate = document.getElementById('txtGRNDate').value;
        if (!grnDate) { alert('GRN Date is required.'); return false; }
        var supDdl = document.getElementById('<%= ddlSupplier.ClientID %>');
        if (!supDdl || supDdl.value === '0') { alert('Please select a supplier.'); return false; }

        rows.forEach(function(tr) {
            var idx = tr.dataset.idx;
            var rmId = document.getElementById('rm_'+idx)?.value || '0';
            if (rmId === '0') { hasError = true; return; }
            var qtyInv = document.getElementById('qtyInv_'+idx)?.value || '';
            var qtyAct = document.getElementById('qtyAct_'+idx)?.value || '';
            var qtyUom = document.getElementById('qtyUom_'+idx)?.value || '';
            var rate = document.getElementById('rate_'+idx)?.value || '';
            if (!qtyInv || !qtyAct || !qtyUom || !rate) { hasError = true; return; }

            items.push({
                rmId: rmId,
                qtyInv: qtyInv, qtyAct: qtyAct, qtyUom: qtyUom,
                rate: rate,
                hsn: document.getElementById('hsn_'+idx)?.value || '',
                gst: document.getElementById('gst_'+idx)?.value || '0',
                qc: document.getElementById('qc_'+idx)?.checked ? '1' : '0'
            });
        });

        if (hasError || items.length === 0) { alert('Please fill all required fields (Material, Qty, Rate) for each line item.'); return false; }

        // Pack header + items into hidden field
        var payload = {
            invoiceNo: document.getElementById('txtInvoiceNo').value,
            invoiceDate: document.getElementById('txtInvoiceDate').value,
            grnDate: grnDate,
            transport: document.getElementById('txtTransport').value || '0',
            transInInvoice: document.getElementById('chkTransInInvoice').checked ? '1' : '0',
            transGST: document.getElementById('chkTransGST').checked ? '1' : '0',
            items: items
        };
        document.getElementById('<%= hfLineItems.ClientID %>').value = JSON.stringify(payload);
        return true;
    }

    // ── Init ──
    window.onload = function() {
        document.getElementById('txtGRNDate').value = new Date().toISOString().split('T')[0];
        addRow();
    };
</script>
<script src="/StockApp/erp-modal.js"></script>
<script src="/StockApp/erp-keepalive.js"></script>
</body>
</html>
