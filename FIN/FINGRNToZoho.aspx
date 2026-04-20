<%@ Page Language="C#" AutoEventWireup="true" Inherits="FINApp.FINGRNToZoho" %>
<%@ Import Namespace="System.Data" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri — GRN to Zoho</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--accent:#d35400;--accent-dark:#a04000;--accent-light:#fef5eb;--teal:#0f6e56;--warn:#f39c12;--danger:#c0392b;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--radius:12px;}
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
.ro-banner{background:#fffbea;border:1px solid #f0e6c0;border-radius:8px;padding:10px 14px;font-size:12px;color:#7c6b20;margin-bottom:18px;}

/* Filter bar */
.filter-bar{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:14px 18px;display:flex;gap:14px;align-items:center;flex-wrap:wrap;margin-bottom:14px;box-shadow:0 1px 3px rgba(0,0,0,.04);}
.filter-bar label{font-size:11px;font-weight:600;letter-spacing:.04em;color:var(--text-muted);text-transform:uppercase;}
.filter-bar input,.filter-bar select{border:1px solid var(--border);border-radius:6px;padding:7px 10px;font-size:13px;font-family:inherit;background:#fff;color:var(--text);}
.filter-bar input:focus,.filter-bar select:focus{outline:none;border-color:var(--accent);}
.filter-bar .filter-group{display:flex;flex-direction:column;gap:3px;}
.filter-bar .flex-spacer{flex:1;}

/* Tabs */
.tabs{display:flex;gap:2px;background:var(--surface);border:1px solid var(--border);border-bottom:none;border-radius:var(--radius) var(--radius) 0 0;overflow:hidden;}
.tab{flex:1;padding:14px 16px;text-align:center;font-size:13px;font-weight:500;color:var(--text-muted);cursor:pointer;border:none;background:#fafafa;border-bottom:3px solid transparent;font-family:inherit;transition:all .15s;}
.tab:hover:not(.tab-active){background:#f0f0f0;color:var(--text);}
.tab.tab-active{background:var(--surface);color:var(--accent);border-bottom-color:var(--accent);font-weight:600;}
.tab .count{display:inline-block;margin-left:6px;background:var(--accent-light);color:var(--accent);padding:1px 7px;border-radius:10px;font-size:11px;font-weight:600;}
.tab.tab-active .count{background:var(--accent);color:#fff;}
.tab.tab-disabled{color:var(--text-dim);cursor:not-allowed;background:#f8f8f8;}
.tab.tab-disabled:hover{background:#f8f8f8;color:var(--text-dim);}

/* Table body */
.tab-content{background:var(--surface);border:1px solid var(--border);border-top:none;border-radius:0 0 var(--radius) var(--radius);padding:0;box-shadow:0 1px 3px rgba(0,0,0,.04);overflow:hidden;}
.toolbar{padding:12px 18px;border-bottom:1px solid var(--border);background:#fafafa;display:flex;gap:10px;align-items:center;}
.toolbar .info{flex:1;font-size:12px;color:var(--text-muted);}
.toolbar .info b{color:var(--text);}

table.grn-table{width:100%;border-collapse:collapse;font-size:13px;}
table.grn-table thead th{background:#fafafa;text-align:left;padding:10px 14px;font-size:11px;font-weight:600;letter-spacing:.06em;text-transform:uppercase;color:var(--text-muted);border-bottom:1px solid var(--border);}
table.grn-table tbody td{padding:12px 14px;border-bottom:1px solid #f0f0f0;vertical-align:middle;}
table.grn-table tbody tr:hover{background:#fcfbfa;}
.col-check{width:32px;}
.col-num{text-align:right;font-variant-numeric:tabular-nums;white-space:nowrap;}
.col-status{width:140px;text-align:center;}
.col-action{width:110px;text-align:center;}
.grn-no{font-weight:600;color:var(--text);font-family:'Roboto Mono','Courier New',monospace;font-size:12px;}
.supplier{font-weight:500;}
.material{font-size:11px;color:var(--text-muted);margin-top:2px;}
.invoice{font-size:12px;}
.invoice-date{font-size:11px;color:var(--text-dim);}
.amount{font-weight:600;}

/* Status badges */
.badge{display:inline-block;padding:3px 10px;border-radius:10px;font-size:10px;font-weight:600;text-transform:uppercase;letter-spacing:.03em;white-space:nowrap;}
.badge-pending{background:#f0f0f0;color:#666;}
.badge-pushed{background:#d4edda;color:#1e7e34;}
.badge-error{background:#f8d7da;color:#721c24;}

/* Action buttons */
.btn{font-family:inherit;border:none;cursor:pointer;border-radius:6px;padding:6px 14px;font-size:12px;font-weight:600;transition:all .15s;white-space:nowrap;}
.btn-push{background:var(--accent-light);color:var(--accent);border:1px solid transparent;}
.btn-push:hover{background:var(--accent);color:#fff;}
.btn-primary{background:var(--accent);color:#fff;}
.btn-primary:hover{background:var(--accent-dark);}
.btn-secondary{background:#fff;color:var(--text);border:1px solid var(--border);}
.btn-secondary:hover{border-color:var(--accent);color:var(--accent);}
.btn-link-view{font-size:11px;color:var(--text-muted);text-decoration:none;}
.btn-link-view:hover{color:var(--accent);text-decoration:underline;}

.empty{padding:60px 20px;text-align:center;color:var(--text-muted);font-size:13px;}
.empty-icon{font-size:36px;margin-bottom:10px;color:var(--text-dim);}

.placeholder-tab{padding:60px 20px;text-align:center;}
.placeholder-tab .big-icon{font-size:44px;margin-bottom:10px;color:var(--text-dim);}
.placeholder-tab .note{font-size:14px;color:var(--text-muted);max-width:460px;margin:0 auto;line-height:1.5;}

/* Alerts */
.alert{border-radius:8px;padding:12px 14px;font-size:12px;margin-bottom:14px;}
.alert-success{background:#d4edda;color:#155724;border:1px solid #c3e6cb;}
.alert-danger{background:#f8d7da;color:#721c24;border:1px solid #f5c6cb;}
.alert-info{background:#e6f2ff;color:#004085;border:1px solid #b3d7ff;}
.alert-warn{background:#fff3cd;color:#856404;border:1px solid #ffeeba;}

/* Error message in row */
.err-line{font-size:11px;color:var(--danger);margin-top:3px;font-style:italic;}

/* Mobile-ish tweaks (not the primary target but avoid breakage) */
@media(max-width:900px){
    .page-header{padding:18px 20px;}
    .container{padding:16px 12px;}
    .filter-bar{padding:12px;gap:10px;}
    table.grn-table{font-size:12px;}
    table.grn-table thead th, table.grn-table tbody td{padding:8px 10px;}
}
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
    <div class="page-icon">&#x1F4E6;</div>
    <div class="page-title">GRN <span>TO ZOHO</span></div>
    <div class="page-sub">Push vendor-purchase GRNs to Zoho Books as Bills. Internal production / preprocess / prefilled GRNs are excluded automatically.</div>
</div>

<div class="container">

    <asp:Panel ID="pnlReadOnly" runat="server" Visible="false" CssClass="ro-banner">
        <b>&#x1F512; Read-only:</b> Only Finance and Admin roles can push GRNs to Zoho. Displaying history in view-only mode.
    </asp:Panel>

    <asp:Panel ID="pnlAlert" runat="server" Visible="false">
        <asp:Literal ID="litAlert" runat="server" />
    </asp:Panel>

    <!-- ── Filter bar (shared across tabs) ── -->
    <div class="filter-bar">
        <div class="filter-group">
            <label>Supplier</label>
            <asp:DropDownList ID="ddlSupplier" runat="server" AutoPostBack="true"
                OnSelectedIndexChanged="ddlSupplier_Changed" />
        </div>
        <div class="filter-group">
            <label>From Date</label>
            <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date" />
        </div>
        <div class="filter-group">
            <label>To Date</label>
            <asp:TextBox ID="txtToDate" runat="server" TextMode="Date" />
        </div>
        <div class="filter-group">
            <label>Status</label>
            <asp:DropDownList ID="ddlStatusFilter" runat="server" AutoPostBack="true"
                OnSelectedIndexChanged="ddlStatusFilter_Changed">
                <asp:ListItem Value="" Text="All" />
                <asp:ListItem Value="Pending" Text="Pending Push" Selected="True" />
                <asp:ListItem Value="Pushed" Text="Pushed" />
                <asp:ListItem Value="Error" Text="Error" />
            </asp:DropDownList>
        </div>
        <asp:Button ID="btnApplyFilter" runat="server" Text="Apply"
            CssClass="btn btn-secondary" OnClick="btnApplyFilter_Click" style="align-self:flex-end;" />
        <div class="flex-spacer"></div>
        <asp:Button ID="btnPushSelected" runat="server" Text="&#x26A1; Push Selected to Zoho"
            CssClass="btn btn-primary" OnClick="btnPushSelected_Click" CausesValidation="false"
            OnClientClick="return confirmPushSelected(this);" style="align-self:flex-end;" />
    </div>

    <!-- ── Tabs ── -->
    <div class="tabs">
        <asp:LinkButton ID="tabRaw" runat="server" OnClick="tabRaw_Click" CssClass="tab" CausesValidation="false">
            <span>Raw Materials</span> <asp:Literal ID="litRawCount" runat="server" />
        </asp:LinkButton>
        <asp:LinkButton ID="tabPacking" runat="server" OnClick="tabPacking_Click" CssClass="tab" CausesValidation="false">
            <span>Packing Materials</span> <asp:Literal ID="litPackingCount" runat="server" />
        </asp:LinkButton>
        <asp:LinkButton ID="tabConsumable" runat="server" OnClick="tabConsumable_Click" CssClass="tab tab-disabled" CausesValidation="false">
            <span>Consumables</span>
        </asp:LinkButton>
        <asp:LinkButton ID="tabStationery" runat="server" OnClick="tabStationery_Click" CssClass="tab tab-disabled" CausesValidation="false">
            <span>Stationery</span>
        </asp:LinkButton>
    </div>

    <!-- ── Tab content ── -->
    <div class="tab-content">

        <!-- RAW / PACKING active list (shared markup, rebuilt from code-behind) -->
        <asp:Panel ID="pnlActiveList" runat="server">
            <div class="toolbar">
                <div class="info">
                    <b><asp:Literal ID="litResultInfo" runat="server" /></b>
                </div>
            </div>

            <asp:PlaceHolder ID="phTable" runat="server" />
            <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="empty">
                <div class="empty-icon">&#x1F4CB;</div>
                No GRNs match the current filters.<br/>
                <span style="font-size:11px;color:var(--text-dim);">Internal production / preprocess / prefilled entries are excluded automatically.</span>
            </asp:Panel>
        </asp:Panel>

        <!-- CONSUMABLE / STATIONERY placeholder (for future) -->
        <asp:Panel ID="pnlPlaceholder" runat="server" Visible="false" CssClass="placeholder-tab">
            <div class="big-icon">&#x1F6A7;</div>
            <div class="note">
                This tab is reserved for <asp:Literal ID="litPlaceholderLabel" runat="server" />.<br/>
                Push-to-Zoho for this GRN type is not yet implemented. Raw Materials and Packing Materials are live.
            </div>
        </asp:Panel>

    </div>

</div>

<script src="/StockApp/erp-modal.js"></script>
<script type="text/javascript">
    /* Uses the shared erp-modal.js API (erpConfirmLink / erpAlert).
       The count-of-selected is computed at click time and woven into
       the message; erpConfirmLink handles the postback on OK. */
    function confirmPushSelected(btn) {
        var boxes = document.querySelectorAll('input[type=checkbox][name*="chkPush"]:checked');
        if (boxes.length === 0) {
            erpAlert('Select at least one GRN to push.', { title: 'No GRNs selected', type: 'warn' });
            return false;
        }
        var msg = 'Push ' + boxes.length + ' GRN(s) to Zoho Books as Bills?<br><br>'
                + '&bull; Auto-create vendors in Zoho if not already mapped<br>'
                + '&bull; Auto-create items in Zoho for any new raw/packing material<br>'
                + '&bull; Create one Bill per selected GRN';
        return erpConfirmLink(btn, msg, {
            title: 'Push to Zoho Books',
            okText: 'Push ' + boxes.length + ' GRN(s)',
            btnClass: 'primary',
            type: 'info'
        });
    }
</script>

</form>
</body>
</html>
