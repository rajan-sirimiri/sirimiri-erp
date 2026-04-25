<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MMConsumptionModeBulk.aspx.cs" Inherits="MMApp.MMConsumptionModeBulk" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Consumption Mode &mdash; Bulk Edit</title>
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600;700&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
    <style>
        :root { --bg:#f5f5f5; --surface:#ffffff; --border:#e0e0e0; --accent:#cc1e1e; --blue:#1e78cc; --teal:#1a9e6a; --text:#1a1a1a; --text-muted:#666; --text-dim:#999; --radius:12px; }
        *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
        body { background: var(--bg); color: var(--text); font-family: 'DM Sans', sans-serif; min-height: 100vh; }
        nav { background: #1a1a1a; display: flex; align-items: center; padding: 0 28px; height: 52px; gap: 6px; position: sticky; top: 0; z-index: 100; }
        .nav-brand { font-family: 'Bebas Neue', sans-serif; font-size: 18px; color: #fff; letter-spacing: .1em; margin-right: 20px; }
        .nav-item { color: #aaa; text-decoration: none; font-size: 12px; font-weight: 600; letter-spacing: .06em; text-transform: uppercase; padding: 6px 12px; border-radius: 6px; transition: all .2s; }
        .nav-item:hover, .nav-item.active { color: #fff; background: rgba(255,255,255,0.08); }
        .nav-sep { color: #444; margin: 0 4px; }
        .nav-right { margin-left: auto; display: flex; align-items: center; gap: 12px; }
        .nav-user { font-size: 12px; color: #888; }
        .page-header { background: var(--surface); border-bottom: 3px solid var(--accent); padding: 24px 40px; }
        .page-title { font-family: 'Bebas Neue', sans-serif; font-size: 28px; letter-spacing: .07em; }
        .page-title span { color: var(--accent); }
        .page-sub { font-size: 12px; color: var(--text-muted); margin-top: 2px; }
        .content { max-width: 1400px; margin: 24px auto; padding: 0 32px; }
        .card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius); padding: 20px 0; }
        .toolbar { display: flex; align-items: center; justify-content: space-between; gap: 16px; padding: 0 28px 20px; flex-wrap: wrap; }
        .toolbar-left { display: flex; align-items: center; gap: 16px; flex-wrap: wrap; }
        .filter-group { display: flex; gap: 6px; flex-wrap: wrap; }
        .filter-btn { padding: 7px 14px; border: 1.5px solid var(--border); border-radius: 6px; background: #fff; cursor: pointer; font-size: 12px; font-weight: 600; color: var(--text-muted); font-family: inherit; transition: all .15s; }
        .filter-btn:hover { border-color: var(--blue); color: var(--blue); }
        .filter-btn.active { background: var(--blue); color: #fff; border-color: var(--blue); }
        .search-wrap { position: relative; }
        .search-wrap input { padding: 8px 12px 8px 32px; border: 1.5px solid var(--border); border-radius: 6px; font-size: 13px; font-family: inherit; outline: none; min-width: 240px; }
        .search-wrap input:focus { border-color: var(--blue); }
        .search-wrap::before { content: '🔍'; position: absolute; left: 10px; top: 50%; transform: translateY(-50%); font-size: 13px; pointer-events: none; }
        .toolbar-right { display: flex; gap: 8px; }
        .btn { padding: 9px 20px; border-radius: 8px; font-family: inherit; font-size: 13px; font-weight: 700; letter-spacing: .04em; cursor: pointer; border: none; transition: all .15s; }
        .btn-primary { background: var(--teal); color: #fff; }
        .btn-primary:hover { background: #157a52; }
        .btn-secondary { background: transparent; border: 1.5px solid var(--border); color: var(--text-muted); }
        .btn-secondary:hover { border-color: var(--text-muted); color: var(--text); }
        .alert { margin: 0 28px 16px; padding: 12px 16px; border-radius: 8px; font-size: 13px; }
        .alert-success { background: rgba(26,158,106,0.1); color: var(--teal); border: 1px solid rgba(26,158,106,0.3); }
        .alert-danger  { background: rgba(204,30,30,0.08); color: var(--accent); border: 1px solid rgba(204,30,30,0.2); }
        .stats-bar { display: flex; gap: 24px; padding: 0 28px 16px; font-size: 12px; color: var(--text-muted); }
        .stats-bar strong { color: var(--text); font-weight: 700; }
        table.bulk { width: 100%; border-collapse: collapse; }
        table.bulk th { padding: 12px 20px; text-align: left; font-size: 10px; font-weight: 700; letter-spacing: .1em; text-transform: uppercase; color: var(--text-dim); background: #fafafa; border-top: 1px solid var(--border); border-bottom: 1px solid var(--border); position: sticky; top: 52px; z-index: 5; }
        table.bulk td { padding: 10px 20px; font-size: 13px; border-bottom: 1px solid #f0f0f0; vertical-align: middle; }
        table.bulk tr:hover td { background: #fafafa; }
        .type-pill { display: inline-block; padding: 2px 10px; border-radius: 20px; font-size: 10px; font-weight: 700; letter-spacing: .06em; }
        .type-PM { background: rgba(204,168,76,0.15); color: #b8860b; }
        .type-CN { background: rgba(26,158,106,0.12); color: var(--teal); }
        .type-ST { background: rgba(140,80,210,0.12); color: #8c50d2; }
        .row-changed td { background: #fff8e1 !important; }
        .row-changed td:first-child { border-left: 3px solid var(--accent); padding-left: 17px; }
        .mode-select { padding: 6px 10px; border: 1.5px solid var(--border); border-radius: 6px; font-size: 12px; font-family: inherit; cursor: pointer; background: #fff; outline: none; min-width: 200px; }
        .mode-select:focus { border-color: var(--blue); }
        .empty-state { text-align: center; padding: 60px 20px; color: var(--text-dim); font-size: 13px; }
        .legend { font-size: 11px; color: var(--text-muted); padding: 12px 28px; background: #fafafa; border-top: 1px solid var(--border); }
        .legend strong { color: var(--text); }
        @media(max-width:900px) { .toolbar { flex-direction: column; align-items: flex-start; } .search-wrap input { min-width: 0; width: 100%; } }
    </style>
</head>
<body>
<form id="form1" runat="server">
    <asp:ScriptManager ID="sm1" runat="server" />

    <nav>
        <a href="MMHome.aspx" style="display:flex;align-items:center;margin-right:16px;flex-shrink:0;background:#fff;border-radius:6px;padding:3px 8px;">
            <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" style="height:28px;width:auto;object-fit:contain;" onerror="this.style.display='none'" />
        </a>
        <a href="/StockApp/ERPHome.aspx" class="nav-item">&#x2302; ERP Home</a>
        <span class="nav-sep">›</span>
        <a href="MMHome.aspx" class="nav-item">Home</a>
        <span class="nav-sep">›</span>
        <span class="nav-item active">Consumption Mode</span>
        <div class="nav-right">
            <span class="nav-user"><asp:Label ID="lblNavUser" runat="server" /></span>
        </div>
    </nav>

    <div class="page-header">
        <div class="page-title">CONSUMPTION <span>MODE</span></div>
        <div class="page-sub">Set how each Packing Material, Consumable and Stationery is consumed when issued from Stores to Floor</div>
    </div>

    <div class="content">
        <asp:Panel ID="pnlAlert" runat="server" Visible="false">
            <div id="divAlert" class="alert">
                <asp:Label ID="lblAlert" runat="server" />
            </div>
        </asp:Panel>

        <div class="card">
            <div class="toolbar">
                <div class="toolbar-left">
                    <div class="filter-group">
                        <button type="button" class="filter-btn active" onclick="filterByType(this, 'ALL');">All Types</button>
                        <button type="button" class="filter-btn"        onclick="filterByType(this, 'PM');">Packing Materials</button>
                        <button type="button" class="filter-btn"        onclick="filterByType(this, 'CN');">Consumables</button>
                        <button type="button" class="filter-btn"        onclick="filterByType(this, 'ST');">Stationery</button>
                    </div>
                    <div class="search-wrap">
                        <input type="text" id="txtSearch" oninput="filterBySearch();" placeholder="Search by name or code..." />
                    </div>
                </div>
                <div class="toolbar-right">
                    <asp:Button ID="btnSaveAll" runat="server" Text="Save All Changes" CssClass="btn btn-primary" OnClick="btnSaveAll_Click" />
                    <button type="button" class="btn btn-secondary" onclick="resetForm();">Reset</button>
                </div>
            </div>

            <div class="stats-bar">
                <span>Total: <strong id="statTotal">0</strong></span>
                <span>Pending changes: <strong id="statChanges" style="color:var(--accent);">0</strong></span>
            </div>

            <asp:HiddenField ID="hfChanges" runat="server" Value="{}" />

            <table class="bulk">
                <thead>
                    <tr>
                        <th style="width:80px;">Type</th>
                        <th style="width:120px;">Code</th>
                        <th>Name</th>
                        <th style="width:80px;">UOM</th>
                        <th style="width:240px;">Consumption Mode</th>
                    </tr>
                </thead>
                <tbody id="tblBody">
                    <asp:Repeater ID="rptMaterials" runat="server">
                        <ItemTemplate>
                            <tr id='row_<%# Eval("MaterialType") %>_<%# Eval("MaterialID") %>'
                                data-type='<%# Eval("MaterialType") %>'
                                data-id='<%# Eval("MaterialID") %>'
                                data-name='<%# Eval("Name") %>'
                                data-code='<%# Eval("Code") %>'
                                data-orig='<%# Eval("ConsumptionMode") %>'>
                                <td><span class='type-pill type-<%# Eval("MaterialType") %>'><%# Eval("MaterialType") %></span></td>
                                <td style="color:var(--text-muted);font-weight:600;font-size:12px;"><%# Eval("Code") %></td>
                                <td style="font-weight:500;"><%# Eval("Name") %></td>
                                <td style="color:var(--text-muted);font-size:12px;"><%# Eval("UOM") %></td>
                                <td>
                                    <select class="mode-select"
                                            data-type='<%# Eval("MaterialType") %>'
                                            data-id='<%# Eval("MaterialID") %>'
                                            onchange="onModeChange(this);">
                                        <option value="IN_PRODUCTION" <%# Eval("ConsumptionMode").ToString()=="IN_PRODUCTION" ? "selected=\"selected\"" : "" %>>In Production (Floor inventory)</option>
                                        <option value="AT_ISSUE"      <%# Eval("ConsumptionMode").ToString()=="AT_ISSUE"      ? "selected=\"selected\"" : "" %>>At Issue (consumed when issued)</option>
                                    </select>
                                </td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
            <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
                <div class="empty-state">No active materials found.</div>
            </asp:Panel>

            <div class="legend">
                <strong>In Production:</strong> qty stays on Floor as inventory after issue (typical for bulk PM, batched materials).
                &nbsp;&middot;&nbsp;
                <strong>At Issue:</strong> qty is treated as consumed at the moment of issue (typical for cleaning consumables, single-use stationery).
            </div>
        </div>
    </div>

<script>
    // Track per-row changes in a JS object: { "PM_12": "AT_ISSUE", ... }
    var pendingChanges = {};

    function refreshStats() {
        var rows = document.querySelectorAll('#tblBody tr');
        document.getElementById('statTotal').textContent = rows.length;
        document.getElementById('statChanges').textContent = Object.keys(pendingChanges).length;
    }

    function onModeChange(sel) {
        var key = sel.dataset.type + '_' + sel.dataset.id;
        var row = sel.closest('tr');
        var origMode = row.dataset.orig;
        if (sel.value === origMode) {
            delete pendingChanges[key];
            row.classList.remove('row-changed');
        } else {
            pendingChanges[key] = sel.value;
            row.classList.add('row-changed');
        }
        // Update hidden field for postback
        document.getElementById('<%= hfChanges.ClientID %>').value = JSON.stringify(pendingChanges);
        refreshStats();
    }

    function filterByType(btn, type) {
        document.querySelectorAll('.filter-btn').forEach(function(b){ b.classList.remove('active'); });
        btn.classList.add('active');
        var rows = document.querySelectorAll('#tblBody tr');
        rows.forEach(function(r){
            r.dataset._typeShow = (type === 'ALL' || r.dataset.type === type) ? '1' : '0';
            applyVisibility(r);
        });
    }

    function filterBySearch() {
        var q = (document.getElementById('txtSearch').value || '').trim().toLowerCase();
        var rows = document.querySelectorAll('#tblBody tr');
        rows.forEach(function(r){
            var name = (r.dataset.name || '').toLowerCase();
            var code = (r.dataset.code || '').toLowerCase();
            r.dataset._searchShow = (!q || name.indexOf(q) !== -1 || code.indexOf(q) !== -1) ? '1' : '0';
            applyVisibility(r);
        });
    }

    function applyVisibility(r) {
        var t = r.dataset._typeShow;   if (t === undefined) t = '1';
        var s = r.dataset._searchShow; if (s === undefined) s = '1';
        r.style.display = (t === '1' && s === '1') ? '' : 'none';
    }

    function resetForm() {
        if (!confirm('Discard all pending changes?')) return;
        pendingChanges = {};
        document.getElementById('<%= hfChanges.ClientID %>').value = '{}';
        var rows = document.querySelectorAll('#tblBody tr');
        rows.forEach(function(r){
            r.classList.remove('row-changed');
            var sel = r.querySelector('.mode-select');
            if (sel) sel.value = r.dataset.orig;
        });
        refreshStats();
    }

    window.addEventListener('load', refreshStats);
</script>
</form>
</body>
</html>
