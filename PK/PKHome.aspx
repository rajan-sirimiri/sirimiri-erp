<%@ Page Language="C#" AutoEventWireup="true" Inherits="PKApp.PKHome" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="UTF-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1.0"/>
<title>Sirimiri — Packing &amp; Shipments</title>
<link rel="preconnect" href="https://fonts.googleapis.com"/>
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin/>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:ital,wght@0,300;0,400;0,500;0,600;1,300&display=swap" rel="stylesheet"/>
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root {
    --bg:#f5f5f5; --surface:#ffffff; --surface2:#f9f9f9;
    --border:#e0e0e0; --accent:#e67e22; --accent-dark:#cf6d17;
    --text:#1a1a1a; --text-muted:#666666; --text-dim:#999999;
    --radius:12px;
}
*,*::before,*::after{box-sizing:border-box;margin:0;padding:0;}
body{background:var(--bg);color:var(--text);font-family:'DM Sans',sans-serif;min-height:100vh;overflow-x:hidden;}
body::before{
    content:'';position:fixed;inset:0;z-index:0;
    background-image:linear-gradient(var(--border) 1px,transparent 1px),linear-gradient(90deg,var(--border) 1px,transparent 1px);
    background-size:48px 48px;opacity:.06;pointer-events:none;
}

/* ── HEADER ── */
header{
    position:relative;z-index:10;background:#fff;
    border-bottom:2px solid var(--accent);
    display:flex;align-items:center;
    padding:0 32px;height:76px;
    box-shadow:0 2px 8px rgba(0,0,0,.06);
}
.header-logo img{height:52px;width:auto;object-fit:contain;filter:drop-shadow(0 2px 8px rgba(230,126,34,.2));}
.header-center{flex:1;text-align:center;}
.header-brand{font-family:'Bebas Neue',sans-serif;font-size:24px;letter-spacing:.10em;color:var(--text);line-height:1;}
.header-tagline{font-size:10px;letter-spacing:.18em;text-transform:uppercase;color:var(--text-muted);margin-top:3px;}
.header-right{display:flex;align-items:center;gap:16px;}
.header-user-name{font-size:13px;font-weight:600;color:var(--text);text-align:right;}
.header-user-role{font-size:11px;color:var(--text-muted);text-transform:uppercase;letter-spacing:.06em;}
.btn-signout{padding:6px 14px;border:1.5px solid var(--border);border-radius:7px;color:var(--text-muted);font-size:12px;font-weight:700;text-decoration:none;letter-spacing:.04em;text-transform:uppercase;transition:all .2s;white-space:nowrap;}
.btn-signout:hover{border-color:var(--accent);color:var(--accent);}

/* ── NAV ── */
nav.pk-nav{
    position:relative;z-index:9;
    background:linear-gradient(135deg,#1a1a1a 0%,#e67e22 100%);
    display:flex;align-items:center;padding:0 32px;gap:4px;
    box-shadow:0 2px 8px rgba(0,0,0,.2);
}
.nav-group{position:relative;}
.nav-item{display:block;padding:12px 16px;color:#fff;font-size:12px;font-weight:600;cursor:pointer;letter-spacing:.05em;text-transform:uppercase;white-space:nowrap;transition:background .2s;text-decoration:none;}
.nav-item:hover,.nav-group:hover>.nav-item{background:rgba(255,255,255,.12);}
.nav-item.active{background:rgba(255,255,255,.18);}
.chevron{font-size:9px;margin-left:4px;}
.nav-dropdown{display:none;position:absolute;top:100%;left:0;background:#fff;border:1px solid var(--border);border-radius:0 0 8px 8px;min-width:210px;box-shadow:0 8px 24px rgba(0,0,0,.12);z-index:200;}
.nav-group:hover .nav-dropdown{display:block;}
.nav-dropdown a{display:block;padding:10px 16px;color:var(--text);font-size:13px;text-decoration:none;border-bottom:1px solid var(--border);transition:background .15s;}
.nav-dropdown a:last-child{border-bottom:none;}
.nav-dropdown a:hover{background:var(--surface2);}
.nav-user{margin-left:auto;color:rgba(255,255,255,.85);font-size:12px;padding:0 4px;}

/* ── MAIN ── */
main{position:relative;z-index:1;max-width:1000px;margin:0 auto;padding:48px 28px 80px;}
.module-strip{display:flex;align-items:center;gap:14px;margin-bottom:36px;}
.module-strip-icon{width:52px;height:52px;border-radius:12px;background:rgba(230,126,34,0.12);display:flex;align-items:center;justify-content:center;font-size:24px;flex-shrink:0;}
.module-strip-label{font-size:10px;font-weight:700;text-transform:uppercase;letter-spacing:.16em;color:var(--text-dim);margin-bottom:2px;}
.module-strip-title{font-family:'Bebas Neue',sans-serif;font-size:30px;letter-spacing:.07em;color:var(--text);}
.module-strip-title span{color:var(--accent);}

.section-head{font-size:10px;font-weight:700;text-transform:uppercase;letter-spacing:.16em;color:var(--text-dim);margin-bottom:12px;padding-bottom:8px;border-bottom:1px solid var(--border);}
.menu-grid{display:grid;grid-template-columns:repeat(3,1fr);gap:16px;margin-bottom:36px;}
.menu-card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:24px 20px 20px;text-decoration:none;display:flex;flex-direction:column;gap:12px;transition:border-color .25s,transform .25s,box-shadow .25s;box-shadow:0 2px 6px rgba(0,0,0,.05);position:relative;overflow:hidden;animation:cardIn .45s ease both;}
.menu-card:nth-child(1){animation-delay:.05s} .menu-card:nth-child(2){animation-delay:.10s} .menu-card:nth-child(3){animation-delay:.15s} .menu-card:nth-child(4){animation-delay:.20s}
@keyframes cardIn{from{opacity:0;transform:translateY(20px)}to{opacity:1;transform:translateY(0)}}
.menu-card::after{content:'';position:absolute;bottom:0;left:0;right:0;height:2px;background:var(--accent);transform:scaleX(0);transform-origin:left;transition:transform .25s;}
.menu-card:hover{border-color:var(--accent);transform:translateY(-3px);box-shadow:0 8px 24px rgba(0,0,0,.10);}
.menu-card:hover::after{transform:scaleX(1);}
.menu-icon{width:44px;height:44px;border-radius:10px;display:flex;align-items:center;justify-content:center;font-size:20px;transition:transform .25s;}
.menu-card:hover .menu-icon{transform:scale(1.1);}
.menu-title{font-family:'Bebas Neue',sans-serif;font-size:17px;letter-spacing:.06em;color:var(--text);line-height:1.15;margin-bottom:3px;}
.menu-desc{font-size:12px;color:var(--text-muted);line-height:1.5;}
.menu-arrow{position:absolute;top:18px;right:16px;font-size:14px;color:#ddd;transition:color .25s,transform .25s;}
.menu-card:hover .menu-arrow{color:var(--accent);transform:translate(2px,-2px);}
.cat-master .menu-icon{background:rgba(201,168,76,0.12);color:#b8860b;}
.cat-txn    .menu-icon{background:rgba(230,126,34,0.12);color:var(--accent);}
.cat-report .menu-icon{background:rgba(30,120,204,0.12);color:#1e78cc;}

@media(max-width:680px){.menu-grid{grid-template-columns:repeat(2,1fr)}}
@media(max-width:420px){.menu-grid{grid-template-columns:1fr}}
</style>
</head>
<body>
<form id="form1" runat="server">

<header>
    <div class="header-logo">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    </div>
    <div class="header-center">
        <div class="header-brand">Sirimiri Nutrition Food Products</div>
        <div class="header-tagline">Enterprise Resource Planning</div>
    </div>
    <div class="header-right">
        <div>
            <div class="header-user-name"><asp:Label ID="lblUser" runat="server"/></div>
            <div class="header-user-role">ADMIN</div>
        </div>
        <a href="PKChangePassword.aspx" class="btn-signout" style="background:transparent;border:1px solid rgba(255,255,255,.25);margin-right:6px;">&#x1F512; Change Password</a>
        <a href="#" class="btn-signout" onclick="erpConfirm('Sign out?',{title:'Sign Out',type:'warn',okText:'Sign Out',onOk:function(){window.location='PKLogout.aspx';}});return false;">&#x2192; Sign Out</a>
    </div>
</header>

<nav class="pk-nav">
    <a href="/StockApp/ERPHome.aspx" class="nav-item">&#x2302; ERP Home</a>
    <div class="nav-group">
        <span class="nav-item active">&#x1F4E6; Packing &amp; Shipments <span class="chevron">&#9660;</span></span>
        <div class="nav-dropdown">
            <a href="PKHome.aspx">PK Home</a>
        </div>
    </div>
    <div class="nav-group">
        <span class="nav-item">Masters <span class="chevron">&#9660;</span></span>
        <div class="nav-dropdown">
            <a href="PKCustomer.aspx">Customer Master</a>
            <a href="PKProductPM.aspx">Product PM Mapping</a>
            <a href="PKMachineMaster.aspx">Machine Master</a>
        </div>
    </div>
    <div class="nav-group">
        <span class="nav-item">Transactions <span class="chevron">&#9660;</span></span>
        <div class="nav-dropdown">
            <a href="PKPrimaryPacking.aspx">Primary Packing</a>
            <a href="PKSecondaryPacking.aspx">Secondary Packing &amp; FG</a>
            <a href="PKShipment.aspx">Shipments</a>
        </div>
    </div>
    <div class="nav-group">
        <span class="nav-item">Reports <span class="chevron">&#9660;</span></span>
        <div class="nav-dropdown">
            <a href="PKReports.aspx">SFG Stock Report</a>
            <a href="PKPMStockReport.aspx">PM Stock Report</a>
            <a href="PKPackingHistory.aspx">Packing History</a>
        </div>
    </div>
    <span class="nav-user"><asp:Label ID="lblNavUser" runat="server"/></span>
</nav>

<main>
    <div class="module-strip">
        <div class="module-strip-icon">&#x1F4E6;</div>
        <div>
            <div class="module-strip-label">Module 5 of 6</div>
            <div class="module-strip-title">Packing &amp; <span>Shipments</span></div>
        </div>
    </div>

    <div class="section-head">Masters</div>
    <div class="menu-grid">
        <a id="lnkCustomer" runat="server" href="PKCustomer.aspx" class="menu-card cat-master">
            <div class="menu-icon">&#x1F3E2;</div>
            <div>
                <div class="menu-title">Customer Master</div>
                <div class="menu-desc">Add and manage customers and distributors</div>
            </div>
            <div class="menu-arrow">&#x2197;</div>
        </a>
        <a id="lnkPMMapping" runat="server" href="PKProductPM.aspx" class="menu-card cat-master">
            <div class="menu-icon">&#x1F517;</div>
            <div>
                <div class="menu-title">Product PM Mapping</div>
                <div class="menu-desc">Define packing materials consumed per product</div>
            </div>
            <div class="menu-arrow">&#x2197;</div>
        </a>
        <a id="lnkMachine" runat="server" href="PKMachineMaster.aspx" class="menu-card cat-master">
            <div class="menu-icon">&#x2699;&#xFE0F;</div>
            <div>
                <div class="menu-title">Machine Master</div>
                <div class="menu-desc">Register and manage packing machines for primary packing</div>
            </div>
            <div class="menu-arrow">&#x2197;</div>
        </a>
    </div>

    <div class="section-head">Transactions</div>
    <div class="menu-grid">
        <a id="lnkPrimary" runat="server" href="PKPrimaryPacking.aspx" class="menu-card cat-txn">
            <div class="menu-icon">&#x1F6E2;</div>
            <div>
                <div class="menu-title">Primary Packing</div>
                <div class="menu-desc">Pack finished product into jars, boxes or cases</div>
            </div>
            <div class="menu-arrow">&#x2197;</div>
        </a>
        <a id="lnkSecondary" runat="server" href="PKSecondaryPacking.aspx" class="menu-card cat-txn">
            <div class="menu-icon">&#x1F4CB;</div>
            <div>
                <div class="menu-title">Secondary Packing &amp; FG</div>
                <div class="menu-desc">Pack into master cartons — shipment ready</div>
            </div>
            <div class="menu-arrow">&#x2197;</div>
        </a>
        <a id="lnkShipment" runat="server" href="PKShipment.aspx" class="menu-card cat-txn">
            <div class="menu-icon">&#x1F69A;</div>
            <div>
                <div class="menu-title">Shipments</div>
                <div class="menu-desc">Dispatch against customer PO with delivery challan</div>
            </div>
            <div class="menu-arrow">&#x2197;</div>
        </a>
    </div>

    <div class="section-head">Reports</div>
    <div class="menu-grid">
        <a id="lnkReports" runat="server" href="PKReports.aspx" class="menu-card cat-report">
            <div class="menu-icon">&#x1F4CA;</div>
            <div>
                <div class="menu-title">SFG Stock Report</div>
                <div class="menu-desc">Semi-Finished Goods — packed jars/boxes by product with FG cases breakdown</div>
            </div>
            <div class="menu-arrow">&#x2197;</div>
        </a>
        <a id="lnkFGStock" runat="server" href="/StockApp/SAFGStock.aspx" class="menu-card cat-report">
            <div class="menu-icon">&#x1F4E6;</div>
            <div>
                <div class="menu-title">FG Stock Level</div>
                <div class="menu-desc">Finished Goods — loose jars, cases, reserved (draft DC), available for DC with PDF download</div>
            </div>
            <div class="menu-arrow">&#x2197;</div>
        </a>
        <a id="lnkPMReport" runat="server" href="PKPMStockReport.aspx" class="menu-card cat-report">
            <div class="menu-icon">&#x1F4E6;</div>
            <div>
                <div class="menu-title">PM Stock Report</div>
                <div class="menu-desc">Packing material stock — opening, received, consumed, current</div>
            </div>
            <div class="menu-arrow">&#x2197;</div>
        </a>
        <a id="lnkHistory" runat="server" href="PKPackingHistory.aspx" class="menu-card cat-report">
            <div class="menu-icon">&#x1F4CB;</div>
            <div>
                <div class="menu-title">Packing History</div>
                <div class="menu-desc">Batch-wise packing history with production time and language details</div>
            </div>
            <div class="menu-arrow">&#x2197;</div>
        </a>
    </div>
</main>
</form>
<script src="/StockApp/erp-modal.js"></script>
<script src="/StockApp/erp-keepalive.js"></script>
</body>
</html>
