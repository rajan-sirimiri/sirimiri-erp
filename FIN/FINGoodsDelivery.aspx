<%@ Page Language="C#" AutoEventWireup="true" Inherits="FINApp.FINGoodsDelivery" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri — Goods Delivery Tracking</title>
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
.container{max-width:900px;margin:0 auto;padding:40px 24px;}
.placeholder{background:var(--surface);border:1px dashed var(--border);border-radius:var(--radius);padding:60px 30px;text-align:center;}
.placeholder-icon{font-size:56px;margin-bottom:12px;opacity:.4;}
.placeholder-title{font-family:'Bebas Neue',sans-serif;font-size:22px;letter-spacing:.05em;margin-bottom:8px;}
.placeholder-sub{font-size:13px;color:var(--text-muted);max-width:500px;margin:0 auto;line-height:1.5;}
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
    <div class="page-title">GOODS <span>DELIVERY TRACKING</span></div>
    <div class="page-sub">Track dispatched consignments through delivery confirmation</div>
</div>
<div class="container">
    <div class="placeholder">
        <div class="placeholder-icon">&#x1F69B;</div>
        <div class="placeholder-title">Coming Soon</div>
        <div class="placeholder-sub">Delivery tracking requirements haven't been finalised yet. This dashboard
            will let finance confirm deliveries per DC and close out consignments once every order is
            received. Check back after the requirement for Item 7 is provided.</div>
    </div>
</div>
</form>
</body>
</html>
