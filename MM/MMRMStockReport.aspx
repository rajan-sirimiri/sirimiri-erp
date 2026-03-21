<%@ Page Language="C#" AutoEventWireup="true" Inherits="MMApp.MMRMStockReport" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Raw Material Stock Report &mdash; MM</title>
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet" />
    <style>
        :root { --bg:#f5f5f5; --surface:#fff; --border:#e0e0e0; --accent:#cc1e1e; --teal:#1a9e6a; --gold:#b8860b; --orange:#e07b00; --text:#1a1a1a; --text-muted:#666; --text-dim:#999; --radius:12px; }
        *, *::before, *::after { box-sizing:border-box; margin:0; padding:0; }
        body { background:var(--bg); color:var(--text); font-family:'DM Sans',sans-serif; min-height:100vh; }
        nav { background:#1a1a1a; display:flex; align-items:center; padding:0 28px; height:52px; gap:6px; position:sticky; top:0; z-index:100; }
        .nav-brand { font-family:'Bebas Neue',sans-serif; font-size:18px; color:#fff; letter-spacing:.1em; margin-right:20px; }
        .nav-item { color:#aaa; text-decoration:none; font-size:12px; font-weight:600; letter-spacing:.06em; text-transform:uppercase; padding:6px 12px; border-radius:6px; }
        .nav-item:hover,.nav-item.active { color:#fff; background:rgba(255,255,255,0.08); }
        .nav-right { margin-left:auto; display:flex; align-items:center; gap:12px; }
        .nav-user { font-size:12px; color:#888; }
        .nav-logout { font-size:11px; color:#666; text-decoration:none; padding:4px 10px; border:1px solid #333; border-radius:5px; }
        .nav-logout:hover { color:var(--accent); border-color:var(--accent); }

        .page-header { background:var(--surface); border-bottom:1px solid var(--border); padding:20px 40px; display:flex; align-items:center; justify-content:space-between; }
        .page-title { font-family:'Bebas Neue',sans-serif; font-size:28px; letter-spacing:.07em; }
        .page-title span { color:var(--teal); }
        .page-sub { font-size:12px; color:var(--text-muted); margin-top:2px; }
        .header-actions { display:flex; gap:10px; align-items:center; }

        .btn-pdf { background:var(--accent); color:#fff; border:none; border-radius:8px; padding:9px 20px; font-size:12px; font-weight:700; letter-spacing:.06em; cursor:pointer; display:flex; align-items:center; gap:6px; text-decoration:none; }
        .btn-pdf:hover { background:#a81515; }
        .btn-refresh { background:#f0f0f0; color:#333; border:1px solid var(--border); border-radius:8px; padding:9px 16px; font-size:12px; font-weight:600; cursor:pointer; }

        .main { max-width:1200px; margin:28px auto; padding:0 32px; }
        .card { background:var(--surface); border:1px solid var(--border); border-radius:var(--radius); }
        .card-header { padding:16px 24px; border-bottom:1px solid var(--border); display:flex; align-items:center; justify-content:space-between; }
        .card-title { font-family:'Bebas Neue',sans-serif; font-size:15px; letter-spacing:.08em; color:var(--text-muted); }
        .report-date { font-size:11px; color:var(--text-dim); }

        .stock-table { width:100%; border-collapse:collapse; font-size:13px; }
        .stock-table th { font-size:10px; font-weight:700; letter-spacing:.08em; text-transform:uppercase; color:var(--text-muted); padding:10px 16px; border-bottom:2px solid var(--border); text-align:left; background:#fafafa; }
        .stock-table th.num { text-align:right; }
        .stock-table td { padding:11px 16px; border-bottom:1px solid var(--border); vertical-align:middle; }
        .stock-table td.num { text-align:right; font-variant-numeric:tabular-nums; }
        .stock-table tr:last-child td { border-bottom:none; }
        .stock-table tr:hover td { background:#f9f9f9; }

        .rm-name { font-weight:600; color:var(--text); }
        .rm-code { font-size:11px; color:var(--text-dim); margin-top:1px; }
        .sr-num { font-size:11px; color:var(--text-dim); }

        .stock-ok    { color:var(--teal); font-weight:700; }
        .stock-low   { color:var(--orange); font-weight:700; }
        .stock-zero  { color:var(--accent); font-weight:700; }

        .recon-blank { color:var(--text-dim); font-size:11px; font-style:italic; }
        .summary-bar { display:flex; gap:24px; padding:12px 24px; background:#f9f9f9; border-top:1px solid var(--border); border-radius:0 0 var(--radius) var(--radius); }
        .summary-item { font-size:12px; color:var(--text-muted); }
        .summary-item strong { color:var(--text); }

        /* PDF print styles */
        @media print {
            nav, .header-actions, .btn-pdf, .btn-refresh { display:none !important; }
            body { background:#fff; }
            .main { margin:0; padding:0; }
            .card { border:none; border-radius:0; }
            .page-header { padding:10px 0; }
            .stock-table th, .stock-table td { padding:6px 10px; font-size:11px; }
            .print-header { display:block !important; }
        }
        .print-header { display:none; font-size:11px; color:var(--text-muted); margin-bottom:8px; }
    </style>
</head>
<body>
<form id="form1" runat="server">

<nav>
    <a href="MMHome.aspx" style="display:flex;align-items:center;margin-right:16px;flex-shrink:0;background:#fff;border-radius:6px;padding:3px 8px;"><img src="Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" style="height:28px;width:auto;object-fit:contain;" onerror="this.style.display='none'" /></a>
    <a href="MMHome.aspx" class="nav-item">Home</a>
    <a href="MMRawInward.aspx" class="nav-item">Raw GRN</a>
    <a href="MMRMStockReport.aspx" class="nav-item active">Stock Report</a>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblNavUser" runat="server"/></span>
        <a href="MMLogout.aspx" class="nav-logout">Logout</a>
    </div>
</nav>

<div class="page-header">
    <div>
        <div class="page-title">Raw Material <span>Stock Report</span></div>
        <div class="page-sub">Current stock position — Opening Stock + GRN Received − Production Consumed</div>
    </div>
    <div class="header-actions">
        <asp:Button ID="btnRefresh" runat="server" CssClass="btn-refresh"
            Text="&#8635; Refresh" OnClick="btnRefresh_Click" CausesValidation="false"/>
        <button type="button" class="btn-pdf" onclick="printReport()">
            &#x1F4C4; Download PDF
        </button>
    </div>
</div>

<div class="main">
    <div class="card">
        <div class="card-header">
            <span class="card-title">&#128230; Raw Materials — Stock Position</span>
            <span class="report-date">As of <asp:Label ID="lblReportDate" runat="server"/></span>
        </div>

        <div class="print-header">
            Sirimiri Nutrition Food Products &nbsp;|&nbsp; Raw Material Stock Report &nbsp;|&nbsp;
            As of <asp:Label ID="lblPrintDate" runat="server"/>
        </div>

        <table class="stock-table">
            <thead>
                <tr>
                    <th>Sr</th>
                    <th>Code</th>
                    <th>Material Name</th>
                    <th>UOM</th>
                    <th class="num">Current Stock</th>
                    <th class="num">Reorder Level</th>
                    <th>Recon Status</th>
                    <th>Recon Date</th>
                </tr>
            </thead>
            <tbody>
                <asp:Repeater ID="rptStock" runat="server">
                    <ItemTemplate>
                        <tr>
                            <td class="sr-num"><%# Container.ItemIndex + 1 %></td>
                            <td><span class="rm-code"><%# Eval("RMCode") %></span></td>
                            <td><span class="rm-name"><%# Eval("RMName") %></span></td>
                            <td><%# Eval("UOM") %></td>
                            <td class="num">
                                <span class='<%# GetStockClass(Eval("CurrentStock"), Eval("ReorderLevel")) %>'>
                                    <%# FormatQty(Eval("CurrentStock")) %>
                                </span>
                            </td>
                            <td class="num"><%# FormatQty(Eval("ReorderLevel")) %></td>
                            <td><span class="recon-blank">Not reconciled</span></td>
                            <td><span class="recon-blank">—</span></td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>

        <div class="summary-bar">
            <div class="summary-item">Total Materials: <strong><asp:Label ID="lblTotal" runat="server"/></strong></div>
            <div class="summary-item" style="color:var(--teal);">In Stock: <strong><asp:Label ID="lblInStock" runat="server"/></strong></div>
            <div class="summary-item" style="color:var(--orange);">Low Stock: <strong><asp:Label ID="lblLow" runat="server"/></strong></div>
            <div class="summary-item" style="color:var(--accent);">Zero Stock: <strong><asp:Label ID="lblZero" runat="server"/></strong></div>
        </div>
    </div>
</div>

</form>
<script>
function printReport() {
    // Set document title to desired filename — browsers use this as the PDF filename
    var now = new Date();
    var dd   = String(now.getDate()).padStart(2,'0');
    var mm   = String(now.getMonth()+1).padStart(2,'0');
    var yy   = String(now.getFullYear()).slice(-2);
    var hh   = String(now.getHours()).padStart(2,'0');
    var min  = String(now.getMinutes()).padStart(2,'0');
    var originalTitle = document.title;
    document.title = 'RM Stock Report ' + dd + '-' + mm + '-' + yy + ' ' + hh + '' + min;
    window.print();
    // Restore original title after print dialog closes
    setTimeout(function() { document.title = originalTitle; }, 2000);
}
</script>
</body>
</html>
