<%@ Page Language="C#" AutoEventWireup="true" Inherits="FINApp.FINInvoiceProcessing" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri — Invoice Processing</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--accent:#8e44ad;--accent-dark:#6c3483;--accent-light:#f4ecf7;--teal:#1a9e6a;--warn:#e67e22;--danger:#e74c3c;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--radius:14px;}
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
.page-title{font-family:'Bebas Neue',sans-serif;font-size:28px;letter-spacing:.06em;}
.page-title span{color:var(--accent);}
.page-sub{font-size:13px;color:var(--text-muted);margin-top:2px;}
.container{max-width:1300px;margin:0 auto;padding:20px 24px 60px;}
.filter-bar{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:16px 20px;margin-bottom:16px;display:flex;align-items:center;gap:16px;flex-wrap:wrap;box-shadow:0 1px 3px rgba(0,0,0,.04);}
.filter-bar .filter-label{font-size:11px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-muted);}
.filter-bar select{font-size:14px;font-weight:600;padding:8px 12px;border:1px solid var(--border);border-radius:8px;background:#fff;min-width:320px;color:var(--text);}
.consig-summary{display:flex;gap:12px;flex:1;min-width:240px;font-size:12px;color:var(--text-muted);}
.consig-summary b{color:var(--text);}
.status-pill{display:inline-block;padding:3px 10px;border-radius:12px;font-size:11px;font-weight:700;}
.s-open{background:#fff3cd;color:#856404;}
.s-ready{background:#d4edda;color:#155724;}
.alert{padding:12px 16px;border-radius:8px;margin-bottom:14px;font-size:13px;}
.alert-success{background:#d4edda;color:#155724;border:1px solid #c3e6cb;}
.alert-danger{background:#fdf3f2;color:#721c24;border:1px solid #f5c6cb;}
.alert-info{background:#e6f2ff;color:#004085;border:1px solid #b3d7ff;}
.dc-card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);margin-bottom:12px;overflow:hidden;box-shadow:0 1px 3px rgba(0,0,0,.04);}
.dc-row{display:grid;grid-template-columns:140px 1fr 110px 130px 120px 90px 80px 80px;gap:14px;align-items:center;padding:14px 20px;border-bottom:1px solid var(--border);}
.dc-row:last-child{border-bottom:none;}
.dc-row.header{background:#fafafa;font-size:10px;font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:var(--text-muted);}
.dc-num{font-weight:700;color:var(--text);}
.dc-customer{font-size:13px;}
.dc-customer .code{font-size:11px;color:var(--text-dim);}
.dc-amount{text-align:right;font-weight:600;font-variant-numeric:tabular-nums;}
.dc-invoice{font-size:12px;}
.dc-status-badge{font-size:10px;font-weight:700;padding:3px 8px;border-radius:10px;text-align:center;display:inline-block;}
.b-draft{background:#fef9f3;color:var(--warn);}
.b-final{background:#eafaf1;color:var(--teal);}
.b-closed{background:#e2e3e5;color:#383d41;}
.approve-box{display:flex;align-items:center;justify-content:center;flex-direction:column;}
.approve-chk{width:20px;height:20px;accent-color:var(--accent);cursor:pointer;}
.approve-meta{font-size:9px;color:var(--text-dim);margin-top:3px;line-height:1.2;text-align:center;}
.btn-expand{background:var(--accent-light);color:var(--accent);border:none;padding:6px 12px;border-radius:6px;font-size:11px;font-weight:700;cursor:pointer;text-decoration:none;display:inline-block;}
.btn-expand:hover{background:var(--accent);color:#fff;}
.dc-detail{background:#fafafa;padding:20px 24px;border-top:1px solid var(--border);}
.dc-detail-title{font-family:'Bebas Neue',sans-serif;font-size:13px;letter-spacing:.08em;color:var(--text-muted);margin-bottom:12px;}
.lines-tbl{width:100%;background:var(--surface);border:1px solid var(--border);border-radius:8px;border-collapse:separate;border-spacing:0;overflow:hidden;}
.lines-tbl th{font-size:10px;font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:var(--text-muted);padding:10px 12px;border-bottom:1px solid var(--border);text-align:left;background:#fafafa;}
.lines-tbl th.num,.lines-tbl td.num{text-align:right;}
.lines-tbl td{padding:10px 12px;border-bottom:1px solid #f0f0f0;font-size:12px;}
.lines-tbl tr:last-child td{border-bottom:none;}
.lines-tbl input{font-size:12px;padding:4px 6px;border:1px solid var(--border);border-radius:4px;width:100%;}
.lines-tbl input[type=number]{text-align:right;}
.lines-tbl .num-input{max-width:90px;}
.totals-strip{display:flex;gap:20px;justify-content:flex-end;padding:12px 0;font-size:12px;}
.totals-strip span{color:var(--text-muted);}
.totals-strip b{color:var(--text);margin-left:4px;font-variant-numeric:tabular-nums;}
.totals-strip .grand{color:var(--accent);font-size:14px;}
.detail-actions{display:flex;gap:10px;margin-top:14px;}
.btn{padding:8px 16px;border-radius:8px;font-size:12px;font-weight:600;border:1px solid transparent;cursor:pointer;text-decoration:none;display:inline-flex;align-items:center;gap:6px;}
.btn-primary{background:var(--accent);color:#fff;}
.btn-primary:hover{background:var(--accent-dark);}
.btn-secondary{background:#f0f0f0;color:var(--text);border:1px solid var(--border);}
.btn-secondary:hover{background:#e0e0e0;}
.btn-success{background:var(--teal);color:#fff;}
.btn-success:hover{background:#148a5b;}
.btn-danger{background:var(--danger);color:#fff;}
.btn-danger:hover{background:#c0392b;}
.bottom-bar{position:sticky;bottom:0;background:var(--surface);border-top:1px solid var(--border);padding:16px 20px;display:flex;gap:12px;justify-content:flex-end;align-items:center;margin-top:20px;border-radius:0 0 var(--radius) var(--radius);box-shadow:0 -2px 8px rgba(0,0,0,.04);flex-wrap:wrap;}
.bottom-bar .summary{flex:1;font-size:12px;color:var(--text-muted);min-width:200px;}
.bottom-bar .summary b{color:var(--text);}
.empty{padding:60px 20px;text-align:center;color:var(--text-muted);font-size:13px;background:var(--surface);border:1px dashed var(--border);border-radius:var(--radius);}
.ro-banner{background:#fffbea;border:1px solid #f0e6c0;border-radius:8px;padding:10px 14px;font-size:12px;color:#7c6b20;margin-bottom:14px;}
.dispatch-form{background:#fffdf5;border:1px solid #f0e6c0;border-radius:8px;padding:14px;margin-top:12px;display:flex;gap:10px;align-items:flex-end;flex-wrap:wrap;}
.dispatch-form label{font-size:10px;font-weight:700;text-transform:uppercase;color:var(--text-muted);display:block;margin-bottom:3px;}
.dispatch-form input{font-size:13px;padding:6px 10px;border:1px solid var(--border);border-radius:6px;min-width:200px;text-transform:uppercase;}
.fin-edit-warn{background:#fdf3f2;border:1px solid #f5c6cb;border-radius:6px;padding:10px 14px;font-size:12px;color:#721c24;margin:0 0 12px 0;}
.fin-edit-warn b{display:block;margin-bottom:3px;font-weight:700;}
.header-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(140px,1fr));gap:10px;margin-bottom:12px;font-size:12px;}
.header-grid .hk{font-size:9px;font-weight:700;text-transform:uppercase;letter-spacing:.06em;color:var(--text-muted);margin-bottom:2px;}
.header-grid .hv{font-weight:600;color:var(--text);}
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
    <div class="page-title">INVOICE <span>PROCESSING</span></div>
    <div class="page-sub">Review DCs, verify invoices, approve for dispatch, and release consignment to delivery</div>
</div>

<div class="container">

    <asp:Panel ID="pnlAlert" runat="server" Visible="false" CssClass="alert">
        <asp:Literal ID="litAlert" runat="server" />
    </asp:Panel>

    <asp:Panel ID="pnlReadOnly" runat="server" Visible="false" CssClass="ro-banner">
        <strong>View-only access.</strong> Your role doesn't have finance permissions — approve, edit, and
        dispatch actions are disabled.
    </asp:Panel>

    <div class="filter-bar">
        <div>
            <div class="filter-label" style="margin-bottom:4px;">Consignment</div>
            <asp:DropDownList ID="ddlConsig" runat="server" AutoPostBack="true"
                OnSelectedIndexChanged="ddlConsig_Changed" />
        </div>
        <div class="consig-summary">
            <asp:Literal ID="litConsigSummary" runat="server" />
        </div>
    </div>

    <asp:Panel ID="pnlDCs" runat="server" Visible="false">

        <div class="dc-card">
            <div class="dc-row header">
                <div>DC #</div>
                <div>Customer</div>
                <div>Status</div>
                <div>Invoice</div>
                <div style="text-align:right;">Amount</div>
                <div style="text-align:center;">Approved</div>
                <div style="text-align:center;">PDF</div>
                <div style="text-align:center;">Review</div>
            </div>
            <asp:Repeater ID="rptDCs" runat="server" OnItemCommand="rptDCs_ItemCommand" OnItemDataBound="rptDCs_ItemDataBound">
                <ItemTemplate>
                    <div class="dc-row">
                        <div class="dc-num"><%# Eval("DCNumber") %></div>
                        <div class="dc-customer">
                            <%# Eval("CustomerName") %>
                            <div class="code"><%# Eval("CustomerCode") %></div>
                        </div>
                        <div>
                            <span class='dc-status-badge <%# GetStatusCss(Eval("Status").ToString()) %>'><%# GetStatusLabel(Eval("Status").ToString()) %></span>
                        </div>
                        <div class="dc-invoice">
                            <%# Eval("InvoiceNumber") == DBNull.Value || Eval("InvoiceNumber").ToString() == "" ? "—" : Eval("InvoiceNumber") %>
                        </div>
                        <div class="dc-amount">&#x20B9;<%# Eval("GrandTotal") != DBNull.Value ? Convert.ToDecimal(Eval("GrandTotal")).ToString("N2") : "0.00" %></div>
                        <div class="approve-box">
                            <asp:CheckBox runat="server" ID="chkApprove" CssClass="approve-chk"
                                AutoPostBack="true" OnCheckedChanged="chkApprove_Changed" />
                            <asp:HiddenField runat="server" ID="hfDcIdForChk" Value='<%# Eval("DCID") %>' />
                            <div class="approve-meta">
                                <asp:Literal runat="server" ID="litApproveMeta" />
                            </div>
                        </div>
                        <div style="text-align:center;">
                            <asp:LinkButton runat="server" ID="lnkDownloadInvoice" CssClass="btn-expand"
                                CommandName="DownloadInvoice" CommandArgument='<%# Eval("DCID") %>'>PDF</asp:LinkButton>
                        </div>
                        <div style="text-align:center;">
                            <asp:LinkButton runat="server" CssClass="btn-expand"
                                CommandName="ToggleDetail" CommandArgument='<%# Eval("DCID") %>'>View</asp:LinkButton>
                        </div>
                    </div>
                    <asp:Panel runat="server" ID="pnlInlineDetail" CssClass="dc-detail" Visible="false">
                        <asp:PlaceHolder runat="server" ID="phDetail" />
                    </asp:Panel>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <div class="bottom-bar">
            <div class="summary">
                <asp:Literal ID="litBottomSummary" runat="server" />
            </div>
            <asp:Button ID="btnDownloadAllInvoices" runat="server" Text="&#x1F4E5; Download All Invoices"
                CssClass="btn btn-secondary" OnClick="btnDownloadAllInvoices_Click" CausesValidation="false" />
            <asp:Button ID="btnMarkReady" runat="server" Text="Mark Consignment READY"
                CssClass="btn btn-success" OnClientClick="return confirmMarkReady();"
                OnClick="btnMarkReady_Click" CausesValidation="false" Visible="false" />
            <asp:Button ID="btnOpenDispatch" runat="server" Text="&#x1F69A; Dispatch Consignment"
                CssClass="btn btn-primary" OnClick="btnOpenDispatch_Click" CausesValidation="false" Visible="false" />
        </div>

        <asp:Panel ID="pnlDispatchForm" runat="server" Visible="false" CssClass="dispatch-form">
            <div>
                <label>Vehicle Number</label>
                <asp:TextBox ID="txtVehicleNo" runat="server" placeholder="TN 01 AB 1234" />
            </div>
            <asp:Button ID="btnConfirmDispatch" runat="server" Text="Confirm Dispatch"
                CssClass="btn btn-success" OnClientClick="return confirmDispatch();"
                OnClick="btnConfirmDispatch_Click" CausesValidation="false" />
        </asp:Panel>

    </asp:Panel>

    <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
        <div class="empty">
            <div style="font-size:40px;margin-bottom:10px;opacity:.4;">&#x1F4EC;</div>
            No OPEN or READY consignments to process.
        </div>
    </asp:Panel>

</div>

<asp:HiddenField ID="hfActiveDCID" runat="server" Value="0" />

<script>
function confirmMarkReady(){ return confirm('Mark this consignment READY for dispatch?\n\nAll DCs must be FINALISED. No more DCs can be added after this.'); }
function confirmDispatch(){
    var v=document.getElementById('<%= txtVehicleNo.ClientID %>').value.trim();
    if(!v){ alert('Please enter a vehicle number.'); return false; }
    return confirm('Dispatch this consignment with vehicle ' + v.toUpperCase() + '?\n\nAll contained DCs will be marked CLOSED and cannot be edited further from the PK module.');
}
function confirmDeleteLine(){ return confirm('Delete this line from the DC? This will recompute the invoice amount. Zoho invoice will need re-sync.'); }
</script>
</form>
<script src="/StockApp/erp-keepalive.js"></script>
</body>
</html>
