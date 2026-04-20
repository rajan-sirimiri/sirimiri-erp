<%@ Page Language="C#" AutoEventWireup="true" Inherits="FINApp.FINChartOfAccounts" %>
<%@ Import Namespace="System.Data" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri — Chart of Accounts</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--accent:#16a085;--accent-dark:#0e7762;--accent-light:#e7f6f2;--teal:#0f6e56;--warn:#f39c12;--danger:#c0392b;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--radius:12px;}
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

.page-header{background:var(--surface);border-bottom:2px solid var(--accent);padding:24px 40px;}
.page-icon{font-size:28px;margin-bottom:4px;}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:30px;letter-spacing:.06em;}
.page-title span{color:var(--accent);}
.page-sub{font-size:12px;color:var(--text-muted);margin-top:2px;}

.container{max-width:1300px;margin:0 auto;padding:22px 24px 60px;}

.meta-bar{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:18px 22px;display:flex;align-items:center;gap:20px;margin-bottom:18px;flex-wrap:wrap;}
.meta-item{display:flex;flex-direction:column;gap:2px;}
.meta-label{font-size:10px;text-transform:uppercase;letter-spacing:.08em;color:var(--text-dim);}
.meta-value{font-size:14px;font-weight:500;}
.meta-stale{color:var(--warn);}
.meta-ok{color:var(--teal);}
.meta-empty{color:var(--danger);}
.meta-action{margin-left:auto;}

.btn{border:none;border-radius:8px;padding:10px 18px;font-size:13px;font-weight:600;cursor:pointer;font-family:inherit;text-decoration:none;display:inline-flex;align-items:center;gap:6px;}
.btn-primary{background:var(--accent);color:#fff;}
.btn-primary:hover{background:var(--accent-dark);}
.btn-ghost{background:transparent;color:var(--text);border:1px solid var(--border);}
.btn-ghost:hover{background:#fafafa;}
.btn:disabled{opacity:.5;cursor:not-allowed;}

.filter-bar{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:14px 18px;display:flex;align-items:center;gap:14px;margin-bottom:14px;flex-wrap:wrap;}
.filter-bar label{font-size:11px;color:var(--text-muted);text-transform:uppercase;letter-spacing:.06em;}
.filter-bar select, .filter-bar input[type=text]{border:1px solid var(--border);border-radius:6px;padding:7px 10px;font-size:13px;font-family:inherit;}
.filter-bar input[type=text]{min-width:220px;}
.filter-bar .spacer{flex:1;}
.count-pill{background:var(--accent-light);color:var(--accent-dark);font-size:11px;font-weight:600;padding:4px 10px;border-radius:10px;}

.banner{border-radius:8px;padding:12px 16px;font-size:13px;margin-bottom:16px;}
.banner-success{background:#e8f7f1;color:#0f6e56;border:1px solid #a7dbc7;}
.banner-error{background:#fdecea;color:#c0392b;border:1px solid #f5b7b1;}
.banner-info{background:#eef6fb;color:#2471a3;border:1px solid #aed6f1;}

.tbl{width:100%;border-collapse:collapse;background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);overflow:hidden;}
.tbl th{background:#fafafa;font-size:10px;text-transform:uppercase;letter-spacing:.08em;color:var(--text-dim);padding:10px 14px;text-align:left;font-weight:600;border-bottom:1px solid var(--border);}
.tbl td{padding:12px 14px;font-size:13px;border-bottom:1px solid #f0f0f0;}
.tbl tr:last-child td{border-bottom:none;}
.tbl tr:hover td{background:#fafafa;}
.col-code{font-family:'Courier New',monospace;color:var(--text-muted);width:90px;}
.col-type{width:140px;color:var(--text-muted);font-size:12px;}
.col-status{width:80px;text-align:center;}
.col-date{width:140px;color:var(--text-muted);font-size:12px;}

.badge{font-size:10px;font-weight:600;padding:3px 9px;border-radius:10px;text-transform:uppercase;letter-spacing:.05em;}
.badge-active{background:#e8f7f1;color:#0f6e56;}
.badge-inactive{background:#f0f0f0;color:#888;}

.empty-state{text-align:center;padding:48px 20px;color:var(--text-muted);font-size:13px;}
.empty-state strong{display:block;font-size:16px;color:var(--text);margin-bottom:6px;}
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
        <a href="FINJournal.aspx" class="nav-link">Journals</a>
        <a href="FINHome.aspx" class="nav-link">&#8592; FIN Home</a>
        <a href="FINLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>

<div class="page-header">
    <div class="page-icon">&#x1F4D2;</div>
    <div class="page-title">CHART OF <span>ACCOUNTS</span></div>
    <div class="page-sub">Local mirror of Zoho Books' chart of accounts — used by journal entries. Sync when you add or deactivate accounts in Zoho.</div>
</div>

<div class="container">

    <asp:PlaceHolder ID="phBanner" runat="server"></asp:PlaceHolder>

    <div class="meta-bar">
        <div class="meta-item">
            <span class="meta-label">Accounts Cached</span>
            <span class="meta-value"><asp:Literal ID="litTotalCount" runat="server" Text="0"/></span>
        </div>
        <div class="meta-item">
            <span class="meta-label">Active</span>
            <span class="meta-value"><asp:Literal ID="litActiveCount" runat="server" Text="0"/></span>
        </div>
        <div class="meta-item">
            <span class="meta-label">Last Sync</span>
            <span class="meta-value"><asp:Literal ID="litLastSync" runat="server" Text="—"/></span>
        </div>
        <div class="meta-action">
            <asp:LinkButton ID="btnSync" runat="server" CssClass="btn btn-primary"
                OnClick="btnSync_Click" OnClientClick="return erpConfirmLink(this,'Pull latest chart of accounts from Zoho? This may take 5-15 seconds.',{title:'Sync Chart of Accounts',okText:'Sync',btnClass:'primary',type:'info'});">
                &#x1F504; Sync from Zoho
            </asp:LinkButton>
        </div>
    </div>

    <div class="filter-bar">
        <label for="<%= ddlType.ClientID %>">Type</label>
        <asp:DropDownList ID="ddlType" runat="server" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
            <asp:ListItem Value="" Text="All types"/>
            <asp:ListItem Value="asset"     Text="Asset"/>
            <asp:ListItem Value="liability" Text="Liability"/>
            <asp:ListItem Value="equity"    Text="Equity"/>
            <asp:ListItem Value="income"    Text="Income"/>
            <asp:ListItem Value="expense"   Text="Expense"/>
        </asp:DropDownList>

        <label for="<%= ddlStatus.ClientID %>">Status</label>
        <asp:DropDownList ID="ddlStatus" runat="server" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
            <asp:ListItem Value="active" Text="Active only" Selected="True"/>
            <asp:ListItem Value="all"    Text="Active + inactive"/>
        </asp:DropDownList>

        <label for="<%= txtSearch.ClientID %>">Search</label>
        <asp:TextBox ID="txtSearch" runat="server" placeholder="name or code..." AutoPostBack="true" OnTextChanged="Filter_Changed"/>

        <div class="spacer"></div>
        <span class="count-pill"><asp:Literal ID="litListCount" runat="server" Text="0"/> shown</span>
    </div>

    <asp:PlaceHolder ID="phList" runat="server"/>

</div>

</form>
<script src="/StockApp/erp-modal.js"></script>
<script src="/StockApp/erp-keepalive.js"></script>
</body>
</html>
