<%@ Page Language="C#" AutoEventWireup="true" Inherits="PKApp.PKHome" %>
<!DOCTYPE html><html lang="en"><head runat="server">
<meta charset="utf-8"/><meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Packing &amp; Shipments — Home</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<style>
:root{--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--accent:#e67e22;--text:#1a1a1a;--text-muted:#666;--radius:14px;}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);min-height:100vh;}
nav{background:#1a1a1a;height:52px;display:flex;align-items:center;padding:0 24px;gap:12px;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;}
.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.08em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:16px;}
.nav-user{color:rgba(255,255,255,.8);font-size:12px;}
.nav-link{color:#fff;font-size:11px;font-weight:600;text-decoration:none;opacity:.8;padding:4px 10px;border:1px solid #444;border-radius:5px;}
.nav-link:hover{opacity:1;border-color:#888;}
.page-header{background:var(--surface);border-bottom:2px solid #1a1a1a;padding:24px 32px;}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:32px;letter-spacing:.07em;}
.page-title span{color:var(--accent);}
.page-sub{font-size:12px;color:var(--text-muted);margin-top:4px;}
main{max-width:960px;margin:32px auto;padding:0 24px;}
.section-head{font-size:10px;font-weight:700;letter-spacing:.16em;text-transform:uppercase;color:var(--text-muted);margin-bottom:14px;padding-bottom:8px;border-bottom:1px solid var(--border);}
.menu-grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(200px,1fr));gap:16px;margin-bottom:32px;}
.menu-card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:22px 20px;text-decoration:none;display:flex;flex-direction:column;gap:10px;transition:all .25s;position:relative;overflow:hidden;}
.menu-card::after{content:'';position:absolute;bottom:0;left:0;right:0;height:3px;background:var(--accent);transform:scaleX(0);transform-origin:left;transition:transform .25s;}
.menu-card:hover{border-color:var(--accent);transform:translateY(-3px);box-shadow:0 8px 24px rgba(0,0,0,.1);}
.menu-card:hover::after{transform:scaleX(1);}
.menu-icon{font-size:26px;}
.menu-title{font-family:'Bebas Neue',sans-serif;font-size:17px;letter-spacing:.06em;color:var(--text);}
.menu-desc{font-size:11px;color:var(--text-muted);line-height:1.5;}
.menu-arrow{color:var(--text-muted);font-size:14px;margin-top:auto;}
.menu-card:hover .menu-arrow{color:var(--accent);}
.cat-masters{} .cat-txn{} .cat-report{}
</style></head><body>
<nav>
    <a class="nav-logo" href="#"><img src="Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/></a>
    <span class="nav-title">Packing &amp; Shipments</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblUser" runat="server"/></span>
        <a href="/StockApp/ERPHome.aspx" class="nav-link"><a href="PKLogout.aspx" class="nav-link">Sign Out</a>#x2302; ERP Home</a>
        <a href="PKLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>
<div class="page-header">
    <div class="page-title">Packing &amp; <span>Shipments</span></div>
    <div class="page-sub">Manage primary packing, secondary packing, customer orders and dispatches</div>
</div>
<main>
    <div class="section-head">Masters</div>
    <div class="menu-grid">
        <a href="PKCustomer.aspx" class="menu-card cat-masters">
            <div class="menu-icon">&#x1F3E2;</div>
            <div><div class="menu-title">Customer<br/>Master</div><div class="menu-desc">Add and manage customers and distributors</div></div>
            <div class="menu-arrow">&#x2197;</div>
        </a>
    </div>
    <div class="section-head">Transactions</div>
    <div class="menu-grid">
        <a href="PKPrimaryPacking.aspx" class="menu-card cat-txn">
            <div class="menu-icon">&#x1F4E6;</div>
            <div><div class="menu-title">Primary<br/>Packing</div><div class="menu-desc">Fill finished product into primary containers — pouches, bottles, jars</div></div>
            <div class="menu-arrow">&#x2197;</div>
        </a>
        <a href="PKSecondaryPacking.aspx" class="menu-card cat-txn">
            <div class="menu-icon">&#x1F4CB;</div>
            <div><div class="menu-title">Secondary<br/>Packing</div><div class="menu-desc">Pack into cartons with labelling — shipment ready</div></div>
            <div class="menu-arrow">&#x2197;</div>
        </a>
        <a href="PKShipment.aspx" class="menu-card cat-txn">
            <div class="menu-icon">&#x1F69A;</div>
            <div><div class="menu-title">Shipments</div><div class="menu-desc">Dispatch cartons against customer PO with delivery challan</div></div>
            <div class="menu-arrow">&#x2197;</div>
        </a>
    </div>
    <div class="section-head">Reports</div>
    <div class="menu-grid">
        <a href="PKReports.aspx" class="menu-card cat-report">
            <div class="menu-icon">&#x1F4CA;</div>
            <div><div class="menu-title">Packing<br/>Reports</div><div class="menu-desc">FG stock position, packing summary and shipment history</div></div>
            <div class="menu-arrow">&#x2197;</div>
        </a>
    </div>
</main>
</body></html>
