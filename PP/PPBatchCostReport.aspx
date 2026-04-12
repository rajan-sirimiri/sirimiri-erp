<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PPBatchCostReport.aspx.cs" Inherits="PPApp.PPBatchCostReport" EnableEventValidation="false" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri PP — Batch Cost Report</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600;700&family=JetBrains+Mono:wght@400;700&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{
    --accent:#2ecc71; --accent-dark:#27ae60; --accent-light:#eafaf1;
    --orange:#e67e22; --blue:#2980b9; --red:#e74c3c;
    --text:#1a1a1a; --text-muted:#666; --text-dim:#999;
    --bg:#f0f0f0; --surface:#fff; --border:#e0e0e0;
    --radius:14px; --nav-h:52px;
}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);color:var(--text);min-height:100vh;}

/* NAV */
nav{background:var(--accent-dark);height:var(--nav-h);display:flex;align-items:center;padding:0 20px;gap:12px;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;}
.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.08em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:16px;}
.nav-user{color:rgba(255,255,255,.85);font-size:12px;}
.nav-link{color:#fff;font-size:12px;font-weight:600;text-decoration:none;opacity:.85;}
.nav-link:hover{opacity:1;}

/* HEADER */
.page-header{background:var(--surface);border-bottom:3px solid var(--accent);padding:20px 30px;display:flex;align-items:center;gap:14;}
.page-icon{font-size:28px;}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:28px;letter-spacing:.07em;}
.page-title span{color:var(--accent);}
.page-sub{font-size:12px;color:var(--text-dim);}

/* FILTERS */
.filter-bar{background:var(--surface);padding:16px 30px;border-bottom:1px solid var(--border);display:flex;gap:14px;align-items:flex-end;flex-wrap:wrap;}
.filter-group{display:flex;flex-direction:column;gap:4px;}
.filter-label{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);}
.filter-input{padding:8px 12px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:13px;outline:none;min-height:38px;}
.filter-input:focus{border-color:var(--accent);}
.filter-select{min-width:200px;}
.btn-apply{padding:8px 20px;background:var(--accent);color:#fff;border:none;border-radius:8px;font-size:13px;font-weight:700;cursor:pointer;min-height:38px;white-space:nowrap;}
.btn-apply:hover{background:var(--accent-dark);}

/* CONTENT */
.content{max-width:1400px;margin:0 auto;padding:20px 24px 80px;}

/* SUMMARY CARDS */
.summary-row{display:flex;gap:14px;margin-bottom:20px;flex-wrap:wrap;}
.summary-card{flex:1;min-width:140px;background:var(--surface);border-radius:10px;padding:16px 18px;border-left:4px solid var(--accent);box-shadow:0 1px 4px rgba(0,0,0,.05);}
.summary-val{font-family:'Bebas Neue',sans-serif;font-size:26px;line-height:1;letter-spacing:.03em;}
.summary-lbl{font-size:9px;font-weight:700;letter-spacing:.1em;text-transform:uppercase;color:var(--text-dim);margin-top:4px;}

/* TABLE */
.rpt-card{background:var(--surface);border-radius:var(--radius);padding:20px;margin-bottom:18px;box-shadow:0 1px 4px rgba(0,0,0,.05);}
.rpt-title{font-family:'Bebas Neue',sans-serif;font-size:16px;color:var(--text-dim);letter-spacing:.06em;margin-bottom:12px;}
.tbl-wrap{overflow-x:auto;border:1px solid var(--border);border-radius:8px;}
table{width:100%;border-collapse:collapse;font-size:12px;}
thead th{font-size:9px;font-weight:700;letter-spacing:.1em;text-transform:uppercase;color:var(--text-dim);
    padding:10px 10px;text-align:left;border-bottom:2px solid var(--border);background:#faf9f7;position:sticky;top:0;z-index:1;}
thead th.num{text-align:right;}
tbody td{padding:8px 10px;border-bottom:1px solid #f2f0ed;vertical-align:top;}
tbody td.num{text-align:right;font-family:'JetBrains Mono',monospace;font-size:11px;}
tbody td.bold{font-weight:700;}
tbody tr:nth-child(even){background:#faf9f7;}
tbody tr.batch-header{background:#eafaf1;font-weight:600;}
tbody tr.batch-header td{border-top:2px solid var(--accent);padding-top:10px;}
tbody tr.batch-total{background:#f0faf5;font-weight:700;}
tbody tr.batch-total td{border-top:1.5px solid var(--accent);font-family:'JetBrains Mono',monospace;font-size:11px;}

.empty-msg{text-align:center;padding:40px;color:var(--text-dim);font-size:14px;}

/* ALERT */
.alert-panel{margin:10px 30px 0;padding:12px 18px;border-radius:8px;}
</style>
</head>
<body>
<form id="form1" runat="server">

<nav>
    <div class="nav-logo"><img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="" onerror="this.style.display='none'"/></div>
    <span class="nav-title">PRODUCTION &amp; PLANNING</span>
    <div class="nav-right">
        <asp:Label ID="lblNavUser" runat="server" CssClass="nav-user"/>
        <a href="PPHome.aspx" class="nav-link">← PP Home</a>
        <a href="/StockApp/ERPHome.aspx" class="nav-link">← ERP</a>
        <a href="PPLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>

<div class="page-header">
    <span class="page-icon">📊</span>
    <div>
        <div class="page-title">BATCH <span>COST REPORT</span></div>
        <div class="page-sub">Production batch costs — BOM vs actual consumption with material pricing</div>
    </div>
</div>

<!-- FILTERS -->
<div class="filter-bar">
    <div class="filter-group">
        <span class="filter-label">Date From</span>
        <asp:TextBox ID="txtDateFrom" runat="server" TextMode="Date" CssClass="filter-input"/>
    </div>
    <div class="filter-group">
        <span class="filter-label">Date To</span>
        <asp:TextBox ID="txtDateTo" runat="server" TextMode="Date" CssClass="filter-input"/>
    </div>
    <div class="filter-group">
        <span class="filter-label">Product</span>
        <asp:DropDownList ID="ddlProduct" runat="server" CssClass="filter-input filter-select"/>
    </div>
    <asp:Button ID="btnApply" runat="server" Text="Apply" CssClass="btn-apply"
        OnClick="btnApply_Click" CausesValidation="false"/>
</div>

<asp:Panel ID="pnlAlert" runat="server" Visible="false" CssClass="alert-panel">
    <asp:Label ID="lblAlert" runat="server"/>
</asp:Panel>

<div class="content">

    <!-- SUMMARY -->
    <asp:Panel ID="pnlSummary" runat="server" Visible="false">
        <div class="summary-row">
            <div class="summary-card">
                <div class="summary-val"><asp:Label ID="lblTotalBatches" runat="server" Text="0"/></div>
                <div class="summary-lbl">Total Batches</div>
            </div>
            <div class="summary-card" style="border-color:var(--blue);">
                <div class="summary-val"><asp:Label ID="lblTotalProducts" runat="server" Text="0"/></div>
                <div class="summary-lbl">Products</div>
            </div>
            <div class="summary-card" style="border-color:var(--orange);">
                <div class="summary-val"><asp:Label ID="lblBOMCost" runat="server" Text="₹0"/></div>
                <div class="summary-lbl">Total BOM Cost</div>
            </div>
            <div class="summary-card" style="border-color:var(--red);">
                <div class="summary-val"><asp:Label ID="lblActualCost" runat="server" Text="₹0"/></div>
                <div class="summary-lbl">Total Actual Cost</div>
            </div>
        </div>
    </asp:Panel>

    <!-- REPORT TABLE -->
    <asp:Panel ID="pnlReport" runat="server" Visible="false">
        <div class="rpt-card">
            <div class="rpt-title">Batch-wise Cost Breakdown</div>
            <div class="tbl-wrap">
                <asp:Literal ID="litTable" runat="server"/>
            </div>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlEmpty" runat="server" Visible="true">
        <div class="empty-msg">Select date range and click Apply to generate the report.</div>
    </asp:Panel>

</div>

<script src="/StockApp/erp-keepalive.js"></script>
</form>
</body>
</html>
