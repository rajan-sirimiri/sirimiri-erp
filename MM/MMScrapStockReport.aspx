<%@ Page Language="C#" AutoEventWireup="true" Inherits="MMApp.MMScrapStockReport" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Scrap Material Stock Report &mdash; MM</title>
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet" />
    <style>
        :root { --bg:#f5f5f5; --surface:#fff; --border:#e0e0e0; --accent:#cc1e1e; --teal:#1a9e6a; --gold:#b8860b; --orange:#e07b00; --text:#1a1a1a; --text-muted:#666; --text-dim:#999; --radius:12px; }
        *, *::before, *::after { box-sizing:border-box; margin:0; padding:0; }
        body { background:var(--bg); color:var(--text); font-family:'DM Sans',sans-serif; min-height:100vh; }
        nav { background:#1a1a1a; display:flex; align-items:center; padding:0 28px; height:52px; gap:6px; position:sticky; top:0; z-index:100; }
        .nav-item { color:#aaa; text-decoration:none; font-size:12px; font-weight:600; letter-spacing:.06em; text-transform:uppercase; padding:6px 12px; border-radius:6px; }
        .nav-item:hover,.nav-item.active { color:#fff; background:rgba(255,255,255,0.08); }
        .nav-right { margin-left:auto; display:flex; align-items:center; gap:12px; }
        .nav-user { font-size:12px; color:#888; }
        .nav-logout { font-size:11px; color:#666; text-decoration:none; padding:4px 10px; border:1px solid #333; border-radius:5px; }
        .nav-logout:hover { color:var(--accent); border-color:var(--accent); }
        .page-header { background:var(--surface); border-bottom:1px solid var(--border); padding:20px 40px; display:flex; align-items:center; justify-content:space-between; }
        .page-title { font-family:'Bebas Neue',sans-serif; font-size:28px; letter-spacing:.07em; }
        .page-title span { color:var(--orange); }
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
        .mat-name { font-weight:600; color:var(--text); }
        .mat-code { font-size:11px; color:var(--text-dim); }
        .sr-num { font-size:11px; color:var(--text-dim); }
        .stock-ok   { color:var(--teal);   font-weight:700; }
        .stock-zero { color:var(--accent); font-weight:700; }
        .summary-bar { display:flex; gap:24px; padding:12px 24px; background:#f9f9f9; border-top:1px solid var(--border); border-radius:0 0 var(--radius) var(--radius); }
        .summary-item { font-size:12px; color:var(--text-muted); }
        .summary-item strong { color:var(--text); }
        @media print {
            nav, .header-actions { display:none !important; }
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
    <a href="MMHome.aspx" style="display:flex;align-items:center;margin-right:16px;flex-shrink:0;background:#fff;border-radius:6px;padding:3px 8px;">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" style="height:28px;width:auto;object-fit:contain;" onerror="this.style.display='none'" />
    </a>
    <a href="MMHome.aspx" class="nav-item">Home</a>
    <a href="MMRMStockReport.aspx" class="nav-item">RM Stock Report</a>
    <a href="MMScrapStockReport.aspx" class="nav-item active">Scrap Report</a>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblNavUser" runat="server"/></span>
        <a href="MMLogout.aspx" class="nav-logout">Logout</a>
    </div>
</nav>

<div class="page-header">
    <div>
        <div class="page-title">Scrap Material <span>Stock Report</span></div>
        <div class="page-sub">Current scrap stock — generated as by-products during raw material processing</div>
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
            <span class="card-title">&#9851; Scrap Materials — Stock Position</span>
            <span class="report-date">As of <asp:Label ID="lblReportDate" runat="server"/></span>
        </div>

        <div class="print-header">
            Sirimiri Nutrition Food Products &nbsp;|&nbsp; Scrap Material Stock Report &nbsp;|&nbsp;
            As of <asp:Label ID="lblPrintDate" runat="server"/>
        </div>

        <table class="stock-table">
            <thead>
                <tr>
                    <th>Sr</th>
                    <th>Code</th>
                    <th>Scrap Name</th>
                    <th>UOM</th>
                    <th class="num">Stock Qty</th>
                    <th>Linked Raw Material(s)</th>
                </tr>
            </thead>
            <tbody>
                <asp:Repeater ID="rptStock" runat="server">
                    <ItemTemplate>
                        <tr>
                            <td class="sr-num"><%# Container.ItemIndex + 1 %></td>
                            <td><span class="mat-code"><%# Eval("ScrapCode") %></span></td>
                            <td><span class="mat-name"><%# Eval("ScrapName") %></span></td>
                            <td><%# Eval("UOM") %></td>
                            <td class="num">
                                <span class='<%# Convert.ToDecimal(Eval("StockQty")) > 0 ? "stock-ok" : "stock-zero" %>'>
                                    <%# FormatQty(Eval("StockQty")) %>
                                </span>
                            </td>
                            <td style="font-size:11px;color:var(--text-muted);"><%# Eval("LinkedRMs") %></td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>

        <div class="summary-bar">
            <div class="summary-item">Total Scrap Materials: <strong><asp:Label ID="lblTotal" runat="server"/></strong></div>
            <div class="summary-item" style="color:var(--teal);">With Stock: <strong><asp:Label ID="lblInStock" runat="server"/></strong></div>
            <div class="summary-item" style="color:var(--accent);">Zero Stock: <strong><asp:Label ID="lblZero" runat="server"/></strong></div>
        </div>
    </div>
</div>

</form>
<script>
function printReport() {
    var now = new Date();
    var dd  = String(now.getDate()).padStart(2,'0');
    var mm  = String(now.getMonth()+1).padStart(2,'0');
    var yy  = String(now.getFullYear()).slice(-2);
    var hh  = String(now.getHours()).padStart(2,'0');
    var min = String(now.getMinutes()).padStart(2,'0');
    var originalTitle = document.title;
    document.title = 'Scrap Stock Report ' + dd + '-' + mm + '-' + yy + ' ' + hh + min;
    window.print();
    setTimeout(function() { document.title = originalTitle; }, 2000);
}
</script>
</body>
</html>
