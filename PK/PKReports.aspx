<%@ Page Language="C#" AutoEventWireup="true" Inherits="PKApp.PKReports" %>
<!DOCTYPE html><html lang="en"><head runat="server">
<meta charset="utf-8"/><meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Packing Reports</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--accent:#e67e22;--accent-dark:#d35400;--teal:#1a9e6a;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--radius:12px;}
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
.main{max-width:1100px;margin:24px auto;padding:0 28px;}
.card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:20px 24px;margin-bottom:20px;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:14px;letter-spacing:.08em;color:var(--text-muted);margin-bottom:14px;padding-bottom:10px;border-bottom:1px solid var(--border);display:flex;justify-content:space-between;align-items:center;}
.btn-pdf{background:var(--accent);color:#fff;border:none;border-radius:8px;padding:7px 16px;font-size:11px;font-weight:700;cursor:pointer;}
.btn-pdf:hover{background:var(--accent-dark);}
.btn-refresh{background:#f0f0f0;color:#333;border:1px solid var(--border);border-radius:8px;padding:7px 14px;font-size:11px;font-weight:600;cursor:pointer;}
.data-table{width:100%;border-collapse:collapse;font-size:13px;}
.data-table th{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-muted);padding:9px 12px;border-bottom:2px solid var(--border);text-align:left;background:#fafafa;}
.data-table th.num{text-align:right;}
.data-table td{padding:10px 12px;border-bottom:1px solid var(--border);vertical-align:middle;}
.data-table td.num{text-align:right;font-variant-numeric:tabular-nums;}
.data-table tr:last-child td{border-bottom:none;}
.stock-ok{color:var(--teal);font-weight:700;}
.stock-zero{color:#e74c3c;font-weight:700;}
.summary-bar{display:flex;gap:24px;padding:12px 16px;background:#f9f9f9;border-top:1px solid var(--border);border-radius:0 0 var(--radius) var(--radius);font-size:12px;color:var(--text-muted);}
.summary-bar strong{color:var(--text);}
.report-as-of{font-size:11px;color:var(--text-dim);}
@media print{nav,.btn-pdf,.btn-refresh{display:none!important;}body{background:#fff;}.main{margin:0;padding:0;}}
</style></head><body>
<form id="form1" runat="server">
<nav>
    <a class="nav-logo" href="PKHome.aspx"><img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="" onerror="this.style.display='none'"/></a>
    <span class="nav-title">Packing &amp; Shipments</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblUser" runat="server"/></span>
        <a href="/StockApp/ERPHome.aspx" class="nav-link">&#x2302; ERP Home</a>
        <a href="PKLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>
<div class="page-header">
    <div>
        <div class="page-title">Packing <span>Reports</span></div>
        <div class="page-sub">SFG &amp; FG stock position, packing summary and shipment history</div>
    </div>
    <div style="display:flex;gap:8px;">
        <asp:Button ID="btnRefresh" runat="server" Text="&#8635; Refresh" CssClass="btn-refresh" OnClick="btnRefresh_Click" CausesValidation="false"/>
        <button type="button" class="btn-pdf" onclick="window.print()">&#x1F4C4; Print / PDF</button>
    </div>
</div>
<div class="main">
    <!-- FG STOCK SUMMARY -->
    <div class="card">
        <div class="card-title">
            <span>&#x1F4E6; Semi-Finished Goods (SFG) Stock Position</span>
            <span class="report-as-of">As of <asp:Label ID="lblDate" runat="server"/></span>
        </div>
        <table class="data-table">
            <thead><tr>
                <th>Code</th>
                <th>Product</th>
                <th class="num">Total Production Units</th>
                <th>Container</th>
                <th class="num">Cases Packed</th>
                <th class="num">Jars / Boxes (loose)</th>
                <th class="num">Individual Pcs (loose)</th>
            </tr></thead>
            <tbody>
                <asp:Repeater ID="rptFG" runat="server">
                    <ItemTemplate><tr>
                        <td style="font-size:11px;color:var(--text-dim)"><%# Eval("ProductCode") %></td>
                        <td><strong><%# Eval("ProductName") %></strong></td>
                        <td class="num">
                            <span class='<%# Convert.ToDecimal(Eval("FGAvailable")) > 0 ? "stock-ok" : "stock-zero" %>' style="font-size:16px;font-weight:700;">
                                <%# string.Format("{0:N0}", Eval("FGAvailable")) %>
                            </span>
                        </td>
                        <td style="font-size:12px;color:var(--text-muted)">
                            <%# Eval("ContainerType") == DBNull.Value || string.IsNullOrEmpty(Eval("ContainerType").ToString()) ? "—" : Eval("ContainerType") %>
                            <%# (Eval("ContainerType") != DBNull.Value && Eval("ContainerType").ToString() != "DIRECT" && Convert.ToInt32(Eval("ContainersPerCase")) > 0) ? "<br/><small>" + Eval("ContainersPerCase") + " per case</small>" : "" %>
                        </td>
                        <td class="num" style="font-weight:700"><%# Convert.ToInt32(Eval("TotalCases")) > 0 ? string.Format("{0:N0}", Eval("TotalCases")) : "—" %></td>
                        <td class="num"><%# Convert.ToInt32(Eval("TotalJars"))  > 0 ? string.Format("{0:N0}", Eval("TotalJars"))  : "—" %></td>
                        <td class="num"><%# Convert.ToInt32(Eval("TotalPcs"))   > 0 ? string.Format("{0:N0}", Eval("TotalPcs"))   : "—" %></td>
                    </tr></ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
        <div class="summary-bar">
            <div>Total Products: <strong><asp:Label ID="lblTotal" runat="server"/></strong></div>
            <div style="color:var(--teal)">With Stock: <strong><asp:Label ID="lblWithStock" runat="server"/></strong></div>
            <div style="color:#e74c3c">Zero Stock: <strong><asp:Label ID="lblZero" runat="server"/></strong></div>
        </div>
    </div>
</div>
</form><script src="/StockApp/erp-keepalive.js"></script>
</body></html>
