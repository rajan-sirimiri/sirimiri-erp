<%@ Page Language="C#" AutoEventWireup="true" Inherits="FINApp.FINHome" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri — Finance</title>
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
.menu-grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(240px,1fr));gap:16px;}
.menu-card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:24px 20px;text-decoration:none;display:flex;gap:14px;align-items:flex-start;transition:all .25s;box-shadow:0 2px 6px rgba(0,0,0,.05);}
.menu-card:hover{border-color:var(--accent);transform:translateY(-3px);box-shadow:0 8px 24px rgba(0,0,0,.1);}
.menu-icon{font-size:28px;flex-shrink:0;margin-top:2px;}
.menu-title{font-family:'Bebas Neue',sans-serif;font-size:16px;letter-spacing:.05em;line-height:1.2;margin-bottom:4px;}
.menu-desc{font-size:11px;color:var(--text-muted);line-height:1.4;}
</style>
</head>
<body>
<form id="form1" runat="server">
<nav>
    <a class="nav-logo" href="/StockApp/ERPHome.aspx">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    </a>
    <span class="nav-title">FINANCE</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblNavUser" runat="server"/></span>
        <a href="/StockApp/ERPHome.aspx" class="nav-link">&#8592; ERP Home</a>
        <a href="FINLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>

<div class="page-header">
    <div class="page-icon">&#x1F4B0;</div>
    <div class="page-title">FINANCE <span>MODULE</span></div>
    <div class="page-sub">Tally data import, mapping, reconciliation and financial reports</div>
</div>

<div class="container">

    <div class="section-head">Tally Integration</div>
    <div class="menu-grid">
        <a id="lnkMapping" runat="server" href="FINTallyMapping.aspx" class="menu-card">
            <div class="menu-icon">&#x1F517;</div>
            <div>
                <div class="menu-title">Tally Data<br/>Mapping</div>
                <div class="menu-desc">Map Tally product, scrap and customer names to ERP master data — one-time setup per new item</div>
            </div>
        </a>
        <a id="lnkSalesImport" runat="server" href="FINSalesImport.aspx" class="menu-card">
            <div class="menu-icon">&#x1F4C8;</div>
            <div>
                <div class="menu-title">Sales Invoice<br/>Import</div>
                <div class="menu-desc">Import Tally sales invoices into ERP — auto-matches mapped products and customers</div>
            </div>
        </a>
        <a id="lnkPurchaseMapping" runat="server" href="FINPurchaseMapping.aspx" class="menu-card">
            <div class="menu-icon">&#x1F517;</div>
            <div>
                <div class="menu-title">Purchase Data<br/>Mapping</div>
                <div class="menu-desc">Map Tally purchase items and suppliers to ERP materials and supplier master</div>
            </div>
        </a>
        <a id="lnkPurchaseImport" runat="server" href="FINPurchaseImport.aspx" class="menu-card">
            <div class="menu-icon">&#x1F6D2;</div>
            <div>
                <div class="menu-title">Purchase Invoice<br/>Import</div>
                <div class="menu-desc">Import Tally purchase invoices — auto-matches mapped items and suppliers</div>
            </div>
        </a>
        <a id="lnkReceiptImport" runat="server" href="FINReceiptImport.aspx" class="menu-card">
            <div class="menu-icon">&#x1F4B0;</div>
            <div>
                <div class="menu-title">Receipt Register<br/>Import</div>
                <div class="menu-desc">Import customer receipts — auto-classified as Customer, Bank, Internal, or Other</div>
            </div>
        </a>
        <a id="lnkSalesAnalytics" runat="server" href="FINSalesAnalytics.aspx" class="menu-card">
            <div class="menu-icon">&#x1F4CA;</div>
            <div>
                <div class="menu-title">Sales Analytics<br/>Dashboard</div>
                <div class="menu-desc">State &amp; district growth, product performance, distributor analysis with charts</div>
            </div>
        </a>
        <a id="lnkOutstanding" runat="server" href="FINOutstandingReport.aspx" class="menu-card">
            <div class="menu-icon">&#x1F4B3;</div>
            <div>
                <div class="menu-title">Invoice<br/>Outstanding</div>
                <div class="menu-desc">Invoice-level payment tracking — FIFO receipt allocation, aging analysis</div>
            </div>
        </a>
    </div>

    <div class="section-head">Zoho Integration</div>
    <div class="menu-grid">
        <a id="lnkConsignments" runat="server" href="FINConsignments.aspx" class="menu-card" style="border-color:#d7bde2;">
            <div class="menu-icon">&#x1F69A;</div>
            <div>
                <div class="menu-title">Consignments</div>
                <div class="menu-desc">Review outbound consignments — invoice verification, DC approval, dispatch release, and delivery tracking</div>
            </div>
        </a>
        <a id="lnkGRNToZoho" runat="server" href="FINGRNToZoho.aspx" class="menu-card" style="border-color:#f5c9a7;">
            <div class="menu-icon">&#x1F4E6;</div>
            <div>
                <div class="menu-title">GRN to Zoho</div>
                <div class="menu-desc">Push vendor-purchase GRNs to Zoho Books as Bills — Raw &amp; Packing materials (Phase 1)</div>
            </div>
        </a>
    </div>

    <div class="section-head">Accounting</div>
    <div class="menu-grid">
        <a id="lnkJournal" runat="server" href="FINJournal.aspx" class="menu-card" style="border-color:#aed6f1;">
            <div class="menu-icon">&#x1F4D3;</div>
            <div>
                <div class="menu-title">Journal Entries</div>
                <div class="menu-desc">Manual double-entry bookings — expenses, accruals, reclassifications, reversals</div>
            </div>
        </a>
        <a id="lnkChartOfAccounts" runat="server" href="FINChartOfAccounts.aspx" class="menu-card" style="border-color:#a9dfbf;">
            <div class="menu-icon">&#x1F4D2;</div>
            <div>
                <div class="menu-title">Chart of Accounts</div>
                <div class="menu-desc">Ledger master mirrored from Zoho Books — sync when accounts change in Zoho</div>
            </div>
        </a>
        <a id="lnkAccountStatement" runat="server" href="FINAccountStatement.aspx" class="menu-card" style="border-color:#f5cba7;">
            <div class="menu-icon">&#x1F4C4;</div>
            <div>
                <div class="menu-title">Account Statement</div>
                <div class="menu-desc">Party ledger with opening balance, transactions, and running Dr/Cr balance</div>
            </div>
        </a>
        <a id="lnkPartyOpeningBalance" runat="server" href="FINPartyOpeningBalance.aspx" class="menu-card" style="border-color:#d7bde2;">
            <div class="menu-icon">&#x2699;</div>
            <div>
                <div class="menu-title">Party Opening Balance</div>
                <div class="menu-desc">Set as-of-FY-start opening balances for customers and suppliers (Super role)</div>
            </div>
        </a>
        <a id="lnkServiceProviderReg" runat="server" href="FINServiceProviderReg.aspx" class="menu-card" style="border-color:#c8b6e2;">
            <div class="menu-icon">&#x1F6E0;</div>
            <div>
                <div class="menu-title">Service Providers</div>
                <div class="menu-desc">Register vendors that provide services (Pest Control, Security, Maintenance) &mdash; billed via JV</div>
            </div>
        </a>
    </div>

</div>
</form>
<script src="/StockApp/erp-keepalive.js"></script>
</body>
</html>
