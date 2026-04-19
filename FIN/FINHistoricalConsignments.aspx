<%@ Page Language="C#" AutoEventWireup="true" Inherits="FINApp.FINHistoricalConsignments" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri — Historical Consignments</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--accent:#8e44ad;--accent-dark:#6c3483;--text:#1a1a1a;--text-muted:#666;--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--radius:14px;}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);color:var(--text);min-height:100vh;}
nav{background:#1a1a1a;display:flex;align-items:center;padding:0 28px;height:52px;gap:6px;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;margin-right:10px;}
.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.1em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:14px;}
.nav-user{font-size:12px;color:#999;}
.nav-link{color:#fff;font-size:12px;font-weight:600;text-decoration:none;opacity:.8;}
.nav-link:hover{opacity:1;}
.page-header{background:var(--surface);border-bottom:2px solid var(--accent);padding:30px 40px;}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:34px;letter-spacing:.06em;}
.page-title span{color:var(--accent);}
.page-sub{font-size:13px;color:var(--text-muted);margin-top:2px;}
.container{max-width:1100px;margin:0 auto;padding:30px 24px;}
.tbl{width:100%;background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);border-collapse:separate;border-spacing:0;overflow:hidden;}
.tbl th{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-muted);padding:12px 14px;border-bottom:1px solid var(--border);text-align:left;background:#fafafa;}
.tbl td{padding:12px 14px;border-bottom:1px solid #f0f0f0;font-size:13px;}
.tbl tr:last-child td{border-bottom:none;}
.tbl tr:hover td{background:#fafafa;}
.badge{font-size:10px;font-weight:700;padding:3px 8px;border-radius:10px;}
.b-dispatched{background:#e6f2ff;color:#004085;}
.b-archived{background:#e2e3e5;color:#383d41;}
.empty{padding:60px 20px;text-align:center;color:var(--text-muted);font-size:13px;}
</style>
</head>
<body>
<form id="form1" runat="server">
<nav>
    <a class="nav-logo" href="FINHome.aspx">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    </a>
    <span class="nav-title">FINANCE</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblNavUser" runat="server"/></span>
        <a href="FINConsignments.aspx" class="nav-link">&#8592; Consignments</a>
        <a href="FINLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>
<div class="page-header">
    <div class="page-title">HISTORICAL <span>CONSIGNMENTS</span></div>
    <div class="page-sub">Dispatched and archived consignments — read-only audit view</div>
</div>
<div class="container">
    <asp:Repeater ID="rptHistorical" runat="server">
        <HeaderTemplate>
            <table class="tbl">
                <thead><tr>
                    <th>Consignment</th><th>Date</th><th>Status</th><th>Vehicle</th>
                    <th>Dispatched At</th><th>Archived At</th>
                </tr></thead><tbody>
        </HeaderTemplate>
        <ItemTemplate>
            <tr>
                <td style="font-weight:600;"><%# Eval("ConsignmentCode") %></td>
                <td><%# Eval("ConsignmentDate", "{0:dd-MMM-yyyy}") %></td>
                <td><span class='badge <%# Eval("Status").ToString() == "DISPATCHED" ? "b-dispatched" : "b-archived" %>'><%# Eval("Status") %></span></td>
                <td style="font-size:12px;color:var(--text-muted);"><%# Eval("VehicleNumber").ToString() == "" ? "—" : Eval("VehicleNumber") %></td>
                <td style="font-size:12px;color:var(--text-muted);"><%# Eval("DispatchedAt") != DBNull.Value ? Convert.ToDateTime(Eval("DispatchedAt")).ToString("dd-MMM-yyyy HH:mm") : "—" %></td>
                <td style="font-size:12px;color:var(--text-muted);"><%# Eval("ArchivedAt") != DBNull.Value ? Convert.ToDateTime(Eval("ArchivedAt")).ToString("dd-MMM-yyyy HH:mm") : "—" %></td>
            </tr>
        </ItemTemplate>
        <FooterTemplate></tbody></table></FooterTemplate>
    </asp:Repeater>
    <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
        <div class="empty">No dispatched or archived consignments yet.</div>
    </asp:Panel>
</div>
</form>
</body>
</html>
