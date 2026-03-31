<%@ Page Language="C#" AutoEventWireup="true" Inherits="StockApp.SAHome" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="UTF-8"/>
<meta name="viewport" content="width=device-width, initial-scale=1.0"/>
<title>Sales & Distribution — Sirimiri ERP</title>
<link rel="preconnect" href="https://fonts.googleapis.com"/>
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin/>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:ital,wght@0,300;0,400;0,500;0,600;1,300&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
    :root {
        --bg:#f5f5f5; --surface:#ffffff; --surface2:#f9f9f9;
        --border:#e0e0e0; --accent:#2980b9; --accent-dark:#2471a3;
        --teal:#1a9e6a; --gold:#b8860b; --orange:#e07b00;
        --text:#1a1a1a; --text-muted:#666666; --text-dim:#999999;
        --radius:12px;
    }
    *, *::before, *::after { box-sizing:border-box; margin:0; padding:0; }
    body { background:var(--bg); font-family:'DM Sans',sans-serif; min-height:100vh; }

    /* Header */
    header {
        display:flex; align-items:center; padding:0 40px; height:72px;
        background:var(--surface); border-bottom:2px solid var(--accent);
        box-shadow:0 2px 8px rgba(0,0,0,.06);
    }
    .header-logo img { height:52px; width:auto; object-fit:contain; filter:drop-shadow(0 2px 8px rgba(41,128,185,.2)); }
    .header-center { flex:1; text-align:center; }
    .header-brand { font-family:'Bebas Neue',sans-serif; font-size:24px; letter-spacing:.10em; color:var(--text); line-height:1; }
    .header-tagline { font-size:10px; letter-spacing:.18em; text-transform:uppercase; color:var(--text-muted); margin-top:3px; }
    .header-right { display:flex; align-items:center; gap:16px; }
    .header-user-name { font-size:13px; font-weight:600; color:var(--text); text-align:right; }
    .header-user-role { font-size:11px; color:var(--text-muted); text-transform:uppercase; letter-spacing:.06em; }
    .btn-signout { padding:6px 14px; border:1.5px solid var(--border); border-radius:7px; color:var(--text-muted); font-size:12px; font-weight:700; text-decoration:none; letter-spacing:.04em; text-transform:uppercase; transition:all .2s; white-space:nowrap; }
    .btn-signout:hover { border-color:var(--accent); color:var(--accent); }

    /* Nav */
    nav.sa-nav {
        position:relative; z-index:9;
        background:linear-gradient(135deg,#1a1a1a 0%,#2980b9 100%);
        display:flex; align-items:center; padding:0 32px; gap:4px;
        box-shadow:0 2px 8px rgba(0,0,0,.2);
    }
    .nav-item { display:block; padding:12px 16px; color:#fff; font-size:12px; font-weight:600; cursor:pointer; letter-spacing:.05em; text-transform:uppercase; white-space:nowrap; transition:background .2s; text-decoration:none; }
    .nav-item:hover, .nav-item.active { background:rgba(255,255,255,.18); }
    .nav-user { margin-left:auto; font-size:12px; color:rgba(255,255,255,.6); }

    /* Module strip */
    .module-strip { padding:28px 40px 10px; }
    .module-strip-title { font-family:'Bebas Neue',sans-serif; font-size:30px; letter-spacing:.07em; color:var(--text); }
    .module-strip-title span { color:var(--accent); }
    .module-strip-sub { font-size:12px; color:var(--text-muted); margin-top:4px; }
    .module-strip-divider { height:3px; margin-top:14px; background:linear-gradient(90deg,var(--accent) 0%,transparent 60%); border-radius:2px; }

    /* Menu grid */
    main { max-width:1100px; margin:0 auto; padding:0 40px 40px; }
    .section-head { font-size:10px; font-weight:700; text-transform:uppercase; letter-spacing:.16em; color:var(--text-dim); margin-bottom:12px; padding-bottom:8px; border-bottom:1px solid var(--border); }
    .menu-grid { display:grid; grid-template-columns:repeat(3,1fr); gap:16px; margin-bottom:36px; }
    .menu-card {
        background:var(--surface); border:1px solid var(--border); border-radius:var(--radius);
        padding:24px 20px 20px; text-decoration:none; display:flex; flex-direction:column; gap:12px;
        transition:border-color .25s,transform .25s,box-shadow .25s;
        box-shadow:0 2px 6px rgba(0,0,0,.05); position:relative; overflow:hidden;
        animation:cardIn .45s ease both;
    }
    .menu-card:nth-child(1){animation-delay:.05s} .menu-card:nth-child(2){animation-delay:.10s}
    .menu-card:nth-child(3){animation-delay:.15s} .menu-card:nth-child(4){animation-delay:.20s}
    .menu-card:nth-child(5){animation-delay:.25s} .menu-card:nth-child(6){animation-delay:.30s}
    @keyframes cardIn { from{opacity:0;transform:translateY(20px)} to{opacity:1;transform:translateY(0)} }
    .menu-card::after { content:''; position:absolute; bottom:0; left:0; right:0; height:2px; background:var(--accent); transform:scaleX(0); transform-origin:left; transition:transform .25s; }
    .menu-card:hover { border-color:var(--accent); transform:translateY(-3px); box-shadow:0 8px 24px rgba(0,0,0,.10); }
    .menu-card:hover::after { transform:scaleX(1); }
    .menu-icon { width:44px; height:44px; border-radius:10px; display:flex; align-items:center; justify-content:center; font-size:20px; transition:transform .25s; }
    .menu-card:hover .menu-icon { transform:scale(1.1); }
    .menu-title { font-family:'Bebas Neue',sans-serif; font-size:17px; letter-spacing:.06em; color:var(--text); line-height:1.15; margin-bottom:3px; }
    .menu-desc { font-size:12px; color:var(--text-muted); line-height:1.5; }
    .menu-arrow { position:absolute; top:18px; right:16px; font-size:14px; color:#ddd; transition:color .25s,transform .25s; }
    .menu-card:hover .menu-arrow { color:var(--accent); transform:translate(2px,-2px); }

    .cat-entry .menu-icon { background:rgba(41,128,185,0.12); color:#2980b9; }
    .cat-stock .menu-icon { background:rgba(26,158,106,0.12); color:#1a9e6a; }
    .cat-report .menu-icon { background:rgba(184,134,11,0.12); color:#b8860b; }

    @media(max-width:680px){.menu-grid{grid-template-columns:repeat(2,1fr)}}
    @media(max-width:420px){.menu-grid{grid-template-columns:1fr}}
    @media(max-width:768px){
        header{padding:0 16px;height:60px;}
        .header-brand{font-size:18px;}
        .header-logo img{height:40px;}
        .module-strip{padding:20px 16px 8px;}
        main{padding:0 16px 24px;}
        nav.sa-nav{padding:0 16px;}
    }
</style>
</head>
<body>
<form id="form1" runat="server">

<header>
    <div class="header-logo">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'" />
    </div>
    <div class="header-center">
        <div class="header-brand">Sirimiri Nutrition Food Products</div>
        <div class="header-tagline">Enterprise Resource Planning</div>
    </div>
    <div class="header-right">
        <div>
            <div class="header-user-name"><asp:Label ID="lblUserName" runat="server" /></div>
            <div class="header-user-role"><asp:Label ID="lblUserRole" runat="server" /></div>
        </div>
        <a href="#" class="btn-signout" onclick="erpConfirm('Sign out?',{title:'Sign Out',type:'warn',okText:'Sign Out',onOk:function(){window.location='Logout.aspx';}});return false;">&#x2192; Sign Out</a>
    </div>
</header>

<nav class="sa-nav">
    <a href="/StockApp/ERPHome.aspx" class="nav-item" title="ERP Home">&#x2302; ERP Home</a>
    <span class="nav-item active">&#x1F4CA; Sales & Distribution</span>
</nav>

<div class="module-strip">
    <div class="module-strip-title">Sales & <span>Distribution</span></div>
    <div class="module-strip-sub">Distributor stock, daily sales, hub management, FG stock levels and reporting</div>
    <div class="module-strip-divider"></div>
</div>

<main>

    <div class="section-head">Stock & Inventory</div>
    <div class="menu-grid">
        <a href="StockEntry.aspx" class="menu-card cat-stock">
            <div class="menu-icon">&#x1F4E6;</div>
            <div>
                <div class="menu-title">Distributor Stock<br/>Position</div>
                <div class="menu-desc">View and update distributor-wise stock levels across all products and locations</div>
            </div>
            <div class="menu-arrow">&#x2197;</div>
        </a>
        <a href="SAHubStock.aspx" class="menu-card cat-stock">
            <div class="menu-icon">&#x1F3ED;</div>
            <div>
                <div class="menu-title">HUB Stock</div>
                <div class="menu-desc">Track finished goods stock at distribution hubs and transit points</div>
            </div>
            <div class="menu-arrow">&#x2197;</div>
        </a>
        <a href="SAFGStock.aspx" class="menu-card cat-stock">
            <div class="menu-icon">&#x1F4CB;</div>
            <div>
                <div class="menu-title">Finished Goods<br/>Stock Level</div>
                <div class="menu-desc">Current FG stock at factory — product-wise quantity, cases and loose jars</div>
            </div>
            <div class="menu-arrow">&#x2197;</div>
        </a>
    </div>

    <div class="section-head">Sales Operations</div>
    <div class="menu-grid">
        <a href="DailySales.aspx" class="menu-card cat-entry">
            <div class="menu-icon">&#x1F4DD;</div>
            <div>
                <div class="menu-title">Daily Sales<br/>Entry — SO</div>
                <div class="menu-desc">Record daily sales orders from field teams — distributor-wise product quantities</div>
            </div>
            <div class="menu-arrow">&#x2197;</div>
        </a>
        <a href="SASalesForce.aspx" class="menu-card cat-entry">
            <div class="menu-icon">&#x1F4F1;</div>
            <div>
                <div class="menu-title">Sales Force<br/>Order Platform</div>
                <div class="menu-desc">Mobile-first order placement for field sales team — real-time stock visibility</div>
            </div>
            <div class="menu-arrow">&#x2197;</div>
        </a>
    </div>

    <div class="section-head">Reports & Analytics</div>
    <div class="menu-grid">
        <a href="Reports.aspx" class="menu-card cat-report">
            <div class="menu-icon">&#x1F4CA;</div>
            <div>
                <div class="menu-title">Reports</div>
                <div class="menu-desc">Sales reports, distributor performance, stock movement analytics and trend analysis</div>
            </div>
            <div class="menu-arrow">&#x2197;</div>
        </a>
    </div>

</main>

<script src="/StockApp/erp-modal.js"></script>
<script src="/StockApp/erp-keepalive.js"></script>
</form>
</body>
</html>
