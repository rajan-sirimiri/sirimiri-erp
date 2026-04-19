<%@ Page Language="C#" AutoEventWireup="true" Inherits="FINApp.FINConsignments" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri — FIN Consignments</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--accent:#8e44ad;--accent-dark:#6c3483;--accent-light:#f4ecf7;--text:#1a1a1a;--text-muted:#666;--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--radius:14px;}
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
.page-icon{font-size:32px;margin-bottom:6px;}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:34px;letter-spacing:.06em;}
.page-title span{color:var(--accent);}
.page-sub{font-size:13px;color:var(--text-muted);margin-top:2px;}

.container{max-width:900px;margin:0 auto;padding:30px 24px;}
.section-head{font-family:'Bebas Neue',sans-serif;font-size:13px;letter-spacing:.12em;color:var(--accent);margin:28px 0 14px;padding-left:4px;}
.menu-grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(260px,1fr));gap:16px;}
.menu-card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:26px 22px;text-decoration:none;display:flex;gap:14px;align-items:flex-start;transition:all .25s;box-shadow:0 2px 6px rgba(0,0,0,.05);color:inherit;}
.menu-card:hover{border-color:var(--accent);transform:translateY(-3px);box-shadow:0 8px 24px rgba(0,0,0,.1);}
.menu-icon{font-size:28px;flex-shrink:0;margin-top:2px;}
.menu-title{font-family:'Bebas Neue',sans-serif;font-size:17px;letter-spacing:.05em;line-height:1.2;margin-bottom:6px;}
.menu-desc{font-size:11px;color:var(--text-muted);line-height:1.45;}
.ro-banner{background:#fffbea;border:1px solid #f0e6c0;border-radius:8px;padding:10px 14px;font-size:12px;color:#7c6b20;margin-bottom:18px;}
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
        <a href="FINHome.aspx" class="nav-link">&#8592; FIN Home</a>
        <a href="FINLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>

<div class="page-header">
    <div class="page-icon">&#x1F69A;</div>
    <div class="page-title">CONSIG<span>NMENTS</span></div>
    <div class="page-sub">Review invoices, approve DCs, and track goods delivery for outbound consignments</div>
</div>

<div class="container">

    <asp:Panel ID="pnlReadOnlyBanner" runat="server" Visible="false" CssClass="ro-banner">
        <strong>View-only access.</strong> You're signed in with a non-finance role — you can browse this
        section but cannot approve, edit, or dispatch. Contact an admin if you need finance permissions.
    </asp:Panel>

    <div class="section-head">Consignment Review</div>
    <div class="menu-grid">
        <a href="FINInvoiceProcessing.aspx" class="menu-card">
            <div class="menu-icon">&#x1F4CB;</div>
            <div>
                <div class="menu-title">Invoice<br/>Processing</div>
                <div class="menu-desc">Review each DC, verify invoice details, approve for dispatch, and mark the
                    consignment as dispatched once complete</div>
            </div>
        </a>
        <a href="FINGoodsDelivery.aspx" class="menu-card">
            <div class="menu-icon">&#x1F4E6;</div>
            <div>
                <div class="menu-title">Goods Delivery<br/>Tracking</div>
                <div class="menu-desc">Follow dispatched consignments through delivery confirmation — track
                    status per DC and close out when all deliveries are confirmed</div>
            </div>
        </a>
        <a href="FINHistoricalConsignments.aspx" class="menu-card">
            <div class="menu-icon">&#x1F5C3;</div>
            <div>
                <div class="menu-title">Historical<br/>Consignments</div>
                <div class="menu-desc">Browse dispatched and archived consignments for audit trail, reporting,
                    and reference</div>
            </div>
        </a>
    </div>

</div>
</form>
<script src="/StockApp/erp-keepalive.js"></script>
</body>
</html>
