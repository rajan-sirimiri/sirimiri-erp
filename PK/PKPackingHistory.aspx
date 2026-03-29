<%@ Page Language="C#" AutoEventWireup="true" Inherits="PKApp.PKPackingHistory" %>
<!DOCTYPE html><html lang="en"><head runat="server">
<meta charset="utf-8"/><meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Packing History &mdash; PK</title>
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
.filter-bar{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:16px 24px;margin-bottom:20px;display:flex;align-items:flex-end;gap:14px;flex-wrap:wrap;}
.filter-bar label{font-size:11px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;color:var(--text-muted);display:block;margin-bottom:5px;}
.filter-bar select,.filter-bar input{padding:9px 13px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:13px;color:var(--text);background:#fafafa;outline:none;}
.filter-bar select:focus,.filter-bar input:focus{border-color:var(--accent);}
.btn-search{background:#1a1a1a;color:#fff;border:none;border-radius:8px;padding:10px 22px;font-size:12px;font-weight:700;cursor:pointer;letter-spacing:.04em;}
.btn-search:hover{background:#333;}
.btn-pdf{background:var(--accent);color:#fff;border:none;border-radius:8px;padding:9px 20px;font-size:12px;font-weight:700;cursor:pointer;}
.btn-pdf:hover{background:var(--accent-dark);}
.card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);}
.card-header{padding:16px 24px;border-bottom:1px solid var(--border);display:flex;align-items:center;justify-content:space-between;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:15px;letter-spacing:.08em;color:var(--text-muted);}
.hist-table{width:100%;border-collapse:collapse;font-size:13px;}
.hist-table th{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-muted);padding:10px 14px;border-bottom:2px solid var(--border);text-align:left;background:#fafafa;}
.hist-table th.num{text-align:right;}
.hist-table td{padding:10px 14px;border-bottom:1px solid var(--border);vertical-align:middle;}
.hist-table td.num{text-align:right;font-variant-numeric:tabular-nums;}
.hist-table tr:last-child td{border-bottom:none;}
.hist-table tr:hover td{background:#f9f9f9;}
.badge-done{background:#eafaf1;color:var(--teal);font-size:10px;font-weight:700;padding:2px 8px;border-radius:10px;}
.badge-prog{background:#fff3e0;color:var(--orange);font-size:10px;font-weight:700;padding:2px 8px;border-radius:10px;}
.summary-bar{display:flex;gap:24px;padding:12px 24px;background:#f9f9f9;border-top:1px solid var(--border);border-radius:0 0 var(--radius) var(--radius);font-size:12px;color:var(--text-muted);}
.summary-bar strong{color:var(--text);}
.empty-note{text-align:center;padding:32px;color:var(--text-dim);font-size:13px;}
@media print{nav,.filter-bar,.btn-pdf{display:none!important;}body{background:#fff;}.main{margin:0;padding:0;}.card{border:none;border-radius:0;}.print-header{display:block!important;}}
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
        <div class="page-title">Packing <span>History</span></div>
        <div class="page-sub">Batch-wise packing history with production details</div>
    </div>
    <button type="button" class="btn-pdf" onclick="printReport()">&#x1F4C4; Download PDF</button>
</div>
<div class="main">
    <!-- FILTER BAR -->
    <div class="filter-bar">
        <div>
            <label>Product</label>
            <asp:DropDownList ID="ddlProduct" runat="server" style="min-width:280px;"/>
        </div>
        <div>
            <label>From Date</label>
            <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date"/>
        </div>
        <div>
            <label>To Date</label>
            <asp:TextBox ID="txtToDate" runat="server" TextMode="Date"/>
        </div>
        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn-search" OnClick="btnSearch_Click" CausesValidation="false"/>
    </div>

    <!-- RESULTS -->
    <asp:Panel ID="pnlResults" runat="server" Visible="false">
    <div class="card">
        <div class="card-header">
            <span class="card-title">&#x1F4CB; Packing History</span>
            <span style="font-size:11px;color:var(--text-dim);">
                <asp:Label ID="lblDateRange" runat="server"/>
            </span>
        </div>
        <div class="print-header">Sirimiri Nutrition Food Products &nbsp;|&nbsp; Packing History &nbsp;|&nbsp; <asp:Label ID="lblPrintRange" runat="server"/></div>

        <asp:Panel ID="pnlEmpty" runat="server"><div class="empty-note">No packing records found for the selected criteria</div></asp:Panel>

        <asp:Panel ID="pnlTable" runat="server" Visible="false">
        <table class="hist-table">
            <thead><tr>
                <th>Sr</th>
                <th>Product</th>
                <th>Order</th>
                <th>Batch</th>
                <th>Date</th>
                <th>Start</th>
                <th>End</th>
                <th>Duration</th>
                <th>Language</th>
                <th class="num">Jars</th>
                <th class="num">Pcs</th>
                <th class="num">Total Units</th>
                <th>Status</th>
            </tr></thead>
            <tbody>
                <asp:Repeater ID="rptHistory" runat="server">
                    <ItemTemplate><tr>
                        <td style="font-size:11px;color:var(--text-dim);"><%# Container.ItemIndex + 1 %></td>
                        <td>
                            <strong><%# Eval("ProductName") %></strong>
                            <div style="font-size:10px;color:var(--text-dim);"><%# Eval("ProductCode") %></div>
                        </td>
                        <td style="font-size:12px;">#<%# Eval("OrderID") %><div style="font-size:10px;color:var(--text-dim);">Shift <%# Eval("Shift") %></div></td>
                        <td style="font-weight:700;">B<%# Eval("BatchNo") %></td>
                        <td style="font-size:12px;"><%# Eval("StartTime") == DBNull.Value ? "—" : Convert.ToDateTime(Eval("StartTime")).ToString("dd MMM yyyy") %></td>
                        <td style="font-size:12px;color:var(--text-muted);"><%# Eval("StartTime") == DBNull.Value ? "—" : Convert.ToDateTime(Eval("StartTime")).ToString("hh:mm tt") %></td>
                        <td style="font-size:12px;color:var(--text-muted);"><%# Eval("EndTime") == DBNull.Value ? "—" : Convert.ToDateTime(Eval("EndTime")).ToString("hh:mm tt") %></td>
                        <td style="font-size:12px;color:var(--text-muted);"><%# FormatDuration(Eval("StartTime"), Eval("EndTime")) %></td>
                        <td style="font-size:12px;font-weight:600;"><%# Eval("LabelLanguage") == DBNull.Value ? "—" : Eval("LabelLanguage") %></td>
                        <td class="num"><%# Eval("Jars") == DBNull.Value ? "—" : string.Format("{0:N0}", Eval("Jars")) %></td>
                        <td class="num"><%# Eval("Units") == DBNull.Value ? "—" : string.Format("{0:N0}", Eval("Units")) %></td>
                        <td class="num" style="font-weight:700;"><%# Eval("TotalUnits") == DBNull.Value ? "—" : string.Format("{0:N0}", Eval("TotalUnits")) %></td>
                        <td><%# Eval("Status").ToString() == "Completed" ? "<span class='badge-done'>Done</span>" : "<span class='badge-prog'>In Progress</span>" %></td>
                    </tr></ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
        <div class="summary-bar">
            <div>Total Batches: <strong><asp:Label ID="lblTotalBatches" runat="server"/></strong></div>
            <div style="color:var(--teal);">Total Jars: <strong><asp:Label ID="lblTotalJars" runat="server"/></strong></div>
            <div>Total Units: <strong><asp:Label ID="lblTotalUnits" runat="server"/></strong></div>
        </div>
        </asp:Panel>
    </div>
    </asp:Panel>
</div>
</form>
<script>
function printReport(){
    var now=new Date();var d=String(now.getDate()).padStart(2,'0');var m=String(now.getMonth()+1).padStart(2,'0');
    var y=String(now.getFullYear()).slice(-2);var h=String(now.getHours()).padStart(2,'0');var mn=String(now.getMinutes()).padStart(2,'0');
    var orig=document.title;document.title='Packing History '+d+'-'+m+'-'+y+' '+h+mn;window.print();setTimeout(function(){document.title=orig;},2000);
}
</script></body></html>
