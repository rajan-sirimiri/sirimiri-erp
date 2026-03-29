<%@ Page Language="C#" AutoEventWireup="true" Inherits="PKApp.PKPMStockReport" %>
<!DOCTYPE html><html lang="en"><head runat="server">
<meta charset="utf-8"/><meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Packing Material Stock Report &mdash; PK</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<style>
:root{--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--accent:#e67e22;--accent-dark:#d35400;--teal:#1a9e6a;--orange:#e07b00;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--radius:12px;}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);min-height:100vh;color:var(--text);}
nav{background:#1a1a1a;height:52px;display:flex;align-items:center;padding:0 24px;gap:12px;position:sticky;top:0;z-index:100;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;}
.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:17px;letter-spacing:.08em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:12px;}
.nav-user{color:rgba(255,255,255,.8);font-size:12px;}
.nav-link{color:#aaa;text-decoration:none;font-size:11px;font-weight:600;text-transform:uppercase;padding:5px 10px;border-radius:5px;}
.nav-link:hover{color:#fff;background:rgba(255,255,255,.08);}
.page-header{background:var(--surface);border-bottom:1px solid var(--border);padding:18px 32px;display:flex;justify-content:space-between;align-items:center;}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:26px;letter-spacing:.07em;}
.page-title span{color:var(--accent);}
.page-sub{font-size:12px;color:var(--text-muted);margin-top:2px;}
.main{max-width:1200px;margin:24px auto;padding:0 28px;}
.card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);}
.card-header{padding:16px 24px;border-bottom:1px solid var(--border);display:flex;align-items:center;justify-content:space-between;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:15px;letter-spacing:.08em;color:var(--text-muted);}
.report-date{font-size:11px;color:var(--text-dim);}
.stock-table{width:100%;border-collapse:collapse;font-size:13px;}
.stock-table th{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-muted);padding:10px 16px;border-bottom:2px solid var(--border);text-align:left;background:#fafafa;}
.stock-table th.num{text-align:right;}
.stock-table td{padding:11px 16px;border-bottom:1px solid var(--border);vertical-align:middle;}
.stock-table td.num{text-align:right;font-variant-numeric:tabular-nums;}
.stock-table tr:last-child td{border-bottom:none;}
.stock-table tr:hover td{background:#f9f9f9;}
.pm-name{font-weight:600;}
.pm-code{font-size:11px;color:var(--text-dim);}
.stock-ok{color:var(--teal);font-weight:700;}
.stock-low{color:var(--orange);font-weight:700;}
.stock-zero{color:#e74c3c;font-weight:700;}
.movement-dim{color:var(--text-dim);font-size:12px;}
.summary-bar{display:flex;gap:24px;padding:12px 24px;background:#f9f9f9;border-top:1px solid var(--border);border-radius:0 0 var(--radius) var(--radius);font-size:12px;color:var(--text-muted);}
.summary-bar strong{color:var(--text);}
.btn-pdf{background:var(--accent);color:#fff;border:none;border-radius:8px;padding:9px 20px;font-size:12px;font-weight:700;cursor:pointer;}
.btn-pdf:hover{background:var(--accent-dark);}
.btn-refresh{background:#f0f0f0;color:#333;border:1px solid var(--border);border-radius:8px;padding:9px 16px;font-size:12px;font-weight:600;cursor:pointer;}
@media print{nav,.page-header .btn-pdf,.btn-refresh{display:none!important;}body{background:#fff;}.main{margin:0;padding:0;}.card{border:none;border-radius:0;}.print-header{display:block!important;}}
.print-header{display:none;font-size:11px;color:var(--text-muted);margin-bottom:8px;}
</style></head><body>
<form id="form1" runat="server">
<nav>
    <a class="nav-logo" href="PKHome.aspx"><img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="" onerror="this.style.display='none'"/></a>
    <span class="nav-title">Packing &amp; Shipments</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblUser" runat="server"/></span>
        <a href="/StockApp/ERPHome.aspx" class="nav-link">ERP Home</a>
        <a href="PKHome.aspx" class="nav-link">PK Home</a>
        <a href="PKLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>
<div class="page-header">
    <div>
        <div class="page-title">Packing Material <span>Stock Report</span></div>
        <div class="page-sub">Current stock position — Opening + GRN Received &minus; Packing Consumed</div>
    </div>
    <div style="display:flex;gap:8px;">
        <asp:Button ID="btnRefresh" runat="server" CssClass="btn-refresh" Text="&#8635; Refresh" OnClick="btnRefresh_Click" CausesValidation="false"/>
        <button type="button" class="btn-pdf" onclick="printReport()">&#x1F4C4; Download PDF</button>
    </div>
</div>
<div class="main">
    <div class="card">
        <div class="card-header">
            <span class="card-title">&#x1F4E6; Packing Materials — Stock Position</span>
            <span class="report-date">As of <asp:Label ID="lblReportDate" runat="server"/></span>
        </div>
        <div class="print-header">Sirimiri Nutrition Food Products &nbsp;|&nbsp; Packing Material Stock Report &nbsp;|&nbsp; As of <asp:Label ID="lblPrintDate" runat="server"/></div>
        <table class="stock-table">
            <thead><tr>
                <th>Sr</th><th>Code</th><th>Material Name</th><th>UOM</th>
                <th class="num">Opening</th><th class="num">Received</th><th class="num">Consumed</th>
                <th class="num">Current Stock</th><th class="num">Reorder Level</th>
            </tr></thead>
            <tbody>
                <asp:Repeater ID="rptStock" runat="server">
                    <ItemTemplate><tr>
                        <td class="pm-code"><%# Container.ItemIndex + 1 %></td>
                        <td><span class="pm-code"><%# Eval("PMCode") %></span></td>
                        <td><span class="pm-name"><%# Eval("PMName") %></span></td>
                        <td><%# Eval("UOM") %></td>
                        <td class="num movement-dim"><%# FormatQty(Eval("OpeningStock")) %></td>
                        <td class="num movement-dim"><%# FormatQty(Eval("TotalReceived")) %></td>
                        <td class="num movement-dim"><%# FormatQty(Eval("TotalConsumed")) %></td>
                        <td class="num"><span class='<%# GetStockClass(Eval("CurrentStock"), Eval("ReorderLevel")) %>'><%# FormatQty(Eval("CurrentStock")) %></span></td>
                        <td class="num"><%# FormatQty(Eval("ReorderLevel")) %></td>
                    </tr></ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
        <div class="summary-bar">
            <div>Total Materials: <strong><asp:Label ID="lblTotal" runat="server"/></strong></div>
            <div style="color:var(--teal);">In Stock: <strong><asp:Label ID="lblInStock" runat="server"/></strong></div>
            <div style="color:var(--orange);">Low Stock: <strong><asp:Label ID="lblLow" runat="server"/></strong></div>
            <div style="color:#e74c3c;">Zero Stock: <strong><asp:Label ID="lblZero" runat="server"/></strong></div>
        </div>
    </div>
</div>
</form>
<script>
function printReport(){
    var now=new Date();var d=String(now.getDate()).padStart(2,'0');var m=String(now.getMonth()+1).padStart(2,'0');
    var y=String(now.getFullYear()).slice(-2);var h=String(now.getHours()).padStart(2,'0');var mn=String(now.getMinutes()).padStart(2,'0');
    var orig=document.title;document.title='PM Stock Report '+d+'-'+m+'-'+y+' '+h+mn;window.print();setTimeout(function(){document.title=orig;},2000);
}
</script></body></html>
