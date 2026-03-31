<%@ Page Language="C#" AutoEventWireup="true" EnableEventValidation="false" Inherits="MMApp.MMStockReconciliation" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="UTF-8"/>
<meta name="viewport" content="width=device-width, initial-scale=1.0"/>
<title>Stock Reconciliation &mdash; MM</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--bg:#f5f5f5;--surface:#fff;--border:#e0e0e0;--accent:#e07b00;--teal:#1a9e6a;--red:#c0392b;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--radius:12px;}
*,*::before,*::after{box-sizing:border-box;margin:0;padding:0;}
body{background:var(--bg);color:var(--text);font-family:'DM Sans',sans-serif;min-height:100vh;}
nav{background:#1a1a1a;display:flex;align-items:center;padding:0 28px;height:52px;gap:6px;position:sticky;top:0;z-index:100;}
.nav-brand{font-family:'Bebas Neue',sans-serif;font-size:18px;color:#fff;letter-spacing:.1em;margin-right:20px;}
.nav-item{color:#aaa;text-decoration:none;font-size:12px;font-weight:600;letter-spacing:.06em;text-transform:uppercase;padding:6px 12px;border-radius:6px;}
.nav-item:hover,.nav-item.active{color:#fff;background:rgba(255,255,255,0.08);}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:12px;}
.nav-user{font-size:12px;color:#888;}
.nav-logout{font-size:11px;color:#666;text-decoration:none;padding:4px 10px;border:1px solid #333;border-radius:5px;}
.nav-logout:hover{color:var(--accent);border-color:var(--accent);}

.page-header{background:var(--surface);border-bottom:1px solid var(--border);padding:20px 40px;}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:28px;letter-spacing:.07em;}
.page-title span{color:var(--teal);}
.page-sub{font-size:12px;color:var(--text-muted);margin-top:2px;}

.container{max-width:1000px;margin:24px auto;padding:0 24px;}
.card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:20px;margin-bottom:20px;}

/* Tabs */
.tab-bar{display:flex;gap:0;border-bottom:2px solid var(--border);margin-bottom:0;}
.tab-btn{padding:12px 24px;font-size:12px;font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:var(--text-muted);cursor:pointer;border:none;background:none;border-bottom:3px solid transparent;margin-bottom:-2px;font-family:inherit;}
.tab-btn:hover{color:var(--text);}
.tab-btn.active{color:var(--accent);border-bottom-color:var(--accent);}

/* Alert */
.alert{padding:12px 16px;border-radius:8px;font-size:13px;margin-bottom:16px;}
.alert-success{background:#eafaf1;color:var(--teal);border:1px solid #b2dfdb;}
.alert-danger{background:#fdf3f2;color:var(--red);border:1px solid #f5c6cb;}

/* Table */
table{width:100%;border-collapse:collapse;font-size:13px;}
th{background:#fafafa;font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);padding:10px 12px;text-align:left;border-bottom:2px solid var(--border);}
td{padding:8px 12px;border-bottom:1px solid #f0f0f0;vertical-align:middle;}
tr:hover{background:#fef9f3;}
.code{font-size:10px;color:var(--text-dim);}
.num{text-align:right;font-weight:600;}

/* Physical qty input */
.phys-input{width:100px;padding:7px 10px;border:1.5px solid var(--border);border-radius:6px;font-size:13px;text-align:right;font-weight:600;font-family:inherit;}
.phys-input:focus{border-color:var(--accent);outline:none;}
.phys-input.saved{border-color:var(--teal);background:#f0faf6;}

/* Per-row save button */
.btn-save-row{background:var(--teal);color:#fff;border:none;border-radius:6px;padding:6px 12px;font-size:11px;font-weight:700;cursor:pointer;font-family:inherit;letter-spacing:.04em;}
.btn-save-row:hover{background:#148a5b;}

/* Saved row styling */
tr.row-saved{background:#f0faf6;}
tr.row-saved:hover{background:#e5f5ec;}
.phys-saved{font-weight:700;font-size:14px;color:var(--teal);text-align:right;display:block;}
.save-done{font-size:11px;color:var(--teal);font-weight:700;}

/* Reconcile columns (hidden initially) */
.recon-col{display:none;}
.recon-col.show{display:table-cell;}
th.recon-col.show{display:table-cell;}
.var-ok{color:var(--teal);}
.var-warn{color:var(--red);font-weight:700;background:rgba(192,57,43,0.08);}

/* Reconcile button */
.btn{border:none;border-radius:8px;padding:10px 24px;font-size:13px;font-weight:700;letter-spacing:.06em;cursor:pointer;font-family:inherit;}
.btn-primary{background:var(--accent);color:#fff;}
.btn-primary:hover{background:#c56a00;}
.btn-teal{background:var(--teal);color:#fff;}
.btn-teal:hover{background:#148a5b;}

.date-bar{display:flex;align-items:center;gap:12px;margin-bottom:16px;}
.date-bar label{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);}
.date-bar input{padding:8px 12px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:13px;}

.summary-bar{display:flex;gap:16px;margin:16px 0;flex-wrap:wrap;}
.summary-stat{background:#fef9f3;border:1px solid #fde3c8;border-radius:8px;padding:8px 16px;text-align:center;}
.summary-stat .val{font-family:'Bebas Neue',sans-serif;font-size:20px;}
.summary-stat .lbl{font-size:9px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);}

.empty{text-align:center;padding:40px;color:var(--text-dim);font-size:13px;}
.footer-bar{display:flex;gap:12px;margin-top:16px;align-items:center;}
</style>
</head>
<body>
<form id="form1" runat="server">

<nav>
    <span class="nav-brand">SIRIMIRI</span>
    <a href="MMHome.aspx" class="nav-item">&#x2190; MM Home</a>
    <span class="nav-item active">Stock Reconciliation</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblNavUser" runat="server"/></span>
        <a href="#" class="nav-logout" onclick="erpConfirm('Sign out?',{title:'Sign Out',type:'warn',okText:'Sign Out',onOk:function(){window.location='MMLogout.aspx';}});return false;">Sign Out</a>
    </div>
</nav>

<div class="page-header">
    <div class="page-title">Stock <span>Reconciliation</span></div>
    <div class="page-sub">Enter physical stock counts, then reconcile against system stock</div>
</div>

<div class="container">

<asp:Panel ID="pnlAlert" runat="server" Visible="false" CssClass="alert"><asp:Label ID="lblAlert" runat="server"/></asp:Panel>

<!-- Date picker -->
<div class="date-bar">
    <label>Session Date</label>
    <asp:TextBox ID="txtReconDate" runat="server" TextMode="Date"/>
    <asp:Button ID="btnLoadDate" runat="server" Text="Load" CssClass="btn btn-primary" OnClick="btnLoadDate_Click" style="padding:8px 16px;font-size:12px;"/>
</div>

<!-- Tabs -->
<div class="tab-bar">
    <asp:Button ID="btnTabRM" runat="server" Text="Raw Materials" CssClass="tab-btn active" OnClick="btnTab_Click" CommandArgument="RM"/>
    <asp:Button ID="btnTabPM" runat="server" Text="Packing Materials" CssClass="tab-btn" OnClick="btnTab_Click" CommandArgument="PM"/>
    <asp:Button ID="btnTabCM" runat="server" Text="Consumables" CssClass="tab-btn" OnClick="btnTab_Click" CommandArgument="CM"/>
    <asp:Button ID="btnTabST" runat="server" Text="Stationary" CssClass="tab-btn" OnClick="btnTab_Click" CommandArgument="ST"/>
</div>

<asp:HiddenField ID="hfTab" runat="server" Value="RM"/>
<asp:HiddenField ID="hfReconciled" runat="server" Value="0"/>
<asp:Button ID="btnSaveRow" runat="server" OnClick="btnSaveRow_Click" style="display:none;"/>
<asp:HiddenField ID="hfSaveData" runat="server" Value=""/>
<asp:Button ID="btnSaveRow" runat="server" OnClick="btnSaveRow_Click" style="display:none;"/>

<div class="card" style="border-top-left-radius:0;border-top-right-radius:0;">

    <div class="summary-bar">
        <div class="summary-stat"><div class="val"><asp:Label ID="lblTotalItems" runat="server" Text="0"/></div><div class="lbl">Total Items</div></div>
        <div class="summary-stat"><div class="val" style="color:var(--teal);"><asp:Label ID="lblEntered" runat="server" Text="0"/></div><div class="lbl">Counts Entered</div></div>
        <div class="summary-stat"><div class="val" style="color:var(--accent);"><asp:Label ID="lblPending" runat="server" Text="0"/></div><div class="lbl">Pending</div></div>
    </div>

    <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
        <div class="empty">No active materials found for this type.</div>
    </asp:Panel>

    <asp:Repeater ID="rptStock" runat="server">
        <HeaderTemplate>
            <table id="tblStock">
            <tr>
                <th style="width:30px;">#</th>
                <th>Material</th>
                <th>UOM</th>
                <th>Physical Count</th>
                <th style="width:80px;">Action</th>
                <th class="recon-col">System Stock</th>
                <th class="recon-col">Variance</th>
                <th class="recon-col">%</th>
            </tr>
        </HeaderTemplate>
        <ItemTemplate>
            <tr data-mid='<%# Eval("MaterialID") %>' class='<%# IsPhysicalSaved(Eval("MaterialID")) ? "row-saved" : "" %>'>
                <td style="color:var(--text-dim);font-size:11px;"><%# Container.ItemIndex + 1 %></td>
                <td>
                    <div style="font-weight:500;"><%# Eval("Name") %></div>
                    <div class="code"><%# Eval("Code") %></div>
                </td>
                <td style="font-size:12px;color:var(--text-muted);"><%# Eval("UOM") %></td>
                <td><%# RenderPhysicalCell(Eval("MaterialID")) %></td>
                <td><%# RenderActionCell(Eval("MaterialID")) %></td>
                <td class="recon-col num" data-sys='<%# Eval("SystemStock") %>'><%# Convert.ToDecimal(Eval("SystemStock")).ToString("N2") %></td>
                <td class="recon-col num" data-var="1"></td>
                <td class="recon-col num" data-pct="1"></td>
            </tr>
        </ItemTemplate>
        <FooterTemplate></table></FooterTemplate>
    </asp:Repeater>

    <div class="footer-bar">
        <asp:Button ID="btnReconcile" runat="server" Text="&#x2696; Reconcile — Show System Stock" CssClass="btn btn-primary" OnClick="btnReconcile_Click"/>
        <asp:Button ID="btnDownload" runat="server" Text="&#x1F4E5; Download Report" CssClass="btn btn-teal" OnClick="btnDownload_Click" Visible="false"/>
        <asp:Label ID="lblReconStatus" runat="server" style="font-size:12px;color:var(--text-muted);"/>
    </div>
</div>

</div>

<script>
// Save a single row via AJAX-style postback
function saveRow(btn) {
    var row = btn.closest('tr');
    var mid = row.getAttribute('data-mid');
    var input = row.querySelector('.phys-input');
    var qty = input.value.trim();
    if (qty === '' || isNaN(parseFloat(qty))) {
        erpAlert('Please enter a valid quantity.', {title:'Invalid', type:'warn'});
        return;
    }
    document.getElementById('<%= hfSaveData.ClientID %>').value = mid + ':' + qty;
    document.getElementById('<%= btnSaveRow.ClientID %>').click();
}

// After reconcile, calculate variances client-side
function showVariances() {
    var rows = document.querySelectorAll('#tblStock tr[data-mid]');
    rows.forEach(function(row) {
        var input = row.querySelector('.phys-input');
        var saved = row.querySelector('.phys-saved');
        var phys = 0;
        if (input) phys = parseFloat(input.value) || 0;
        else if (saved) phys = parseFloat(saved.innerText) || 0;
        var sysTd = row.querySelector('[data-sys]');
        var varTd = row.querySelector('[data-var]');
        var pctTd = row.querySelector('[data-pct]');
        if (!sysTd || !varTd) return;
        var sys = parseFloat(sysTd.getAttribute('data-sys')) || 0;
        var diff = phys - sys;
        var pct = sys !== 0 ? (diff / sys) * 100 : (phys !== 0 ? 100 : 0);
        varTd.innerText = diff.toFixed(2);
        pctTd.innerText = pct.toFixed(1) + '%';
        if (Math.abs(pct) > 1) {
            varTd.className = 'recon-col num show var-warn';
            pctTd.className = 'recon-col num show var-warn';
        } else {
            varTd.className = 'recon-col num show var-ok';
            pctTd.className = 'recon-col num show var-ok';
        }
    });
}

// Show recon columns if already reconciled
window.addEventListener('load', function() {
    if ('<%= hfReconciled.Value %>' === '1') {
        document.querySelectorAll('.recon-col').forEach(function(el) { el.classList.add('show'); });
        showVariances();
    }
});
</script>
<script src="/StockApp/erp-modal.js"></script>
<script src="/StockApp/erp-keepalive.js"></script>
</form>
</body>
</html>
