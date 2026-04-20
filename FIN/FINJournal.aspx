<%@ Page Language="C#" AutoEventWireup="true" Inherits="FINApp.FINJournal" %>
<%@ Import Namespace="System.Data" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri — Journal Entries</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--accent:#2980b9;--accent-dark:#1f618d;--accent-light:#eaf3fb;--teal:#0f6e56;--warn:#f39c12;--danger:#c0392b;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--radius:12px;}
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

.btn{border:none;border-radius:8px;padding:10px 18px;font-size:13px;font-weight:600;cursor:pointer;font-family:inherit;text-decoration:none;display:inline-flex;align-items:center;gap:6px;}
.btn-primary{background:var(--accent);color:#fff;}
.btn-primary:hover{background:var(--accent-dark);}
.btn-teal{background:var(--teal);color:#fff;}
.btn-teal:hover{background:#0a5541;}
.btn-ghost{background:transparent;color:var(--text);border:1px solid var(--border);}
.btn-ghost:hover{background:#fafafa;}
.btn-danger{background:transparent;color:var(--danger);border:1px solid #f5b7b1;}
.btn-danger:hover{background:#fdecea;}
.btn-warn{background:transparent;color:#b9770e;border:1px solid #f5cba7;}
.btn-warn:hover{background:#fef5eb;}
.btn:disabled{opacity:.5;cursor:not-allowed;}

.banner{border-radius:8px;padding:12px 16px;font-size:13px;margin-bottom:16px;}
.banner-success{background:#e8f7f1;color:#0f6e56;border:1px solid #a7dbc7;}
.banner-error{background:#fdecea;color:#c0392b;border:1px solid #f5b7b1;}
.banner-info{background:#eef6fb;color:#2471a3;border:1px solid #aed6f1;}
.banner-warn{background:#fef5eb;color:#b9770e;border:1px solid #f5cba7;}

/* ========= LIST MODE ========= */
.filter-bar{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:14px 18px;display:flex;align-items:center;gap:14px;margin-bottom:14px;flex-wrap:wrap;}
.filter-bar label{font-size:11px;color:var(--text-muted);text-transform:uppercase;letter-spacing:.06em;}
.filter-bar input, .filter-bar select{border:1px solid var(--border);border-radius:6px;padding:7px 10px;font-size:13px;font-family:inherit;}
.filter-bar input[type=text]{min-width:180px;}
.filter-bar .spacer{flex:1;}

.status-bar{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:14px 18px;display:flex;align-items:center;gap:28px;margin-bottom:14px;flex-wrap:wrap;}
.status-chip{display:flex;align-items:center;gap:8px;}
.status-chip .dot{width:10px;height:10px;border-radius:50%;}
.status-chip .dot.draft{background:#bdc3c7;}
.status-chip .dot.posted{background:#0f6e56;}
.status-chip .dot.reversed{background:#b9770e;}
.status-chip .lbl{font-size:11px;text-transform:uppercase;letter-spacing:.06em;color:var(--text-muted);}
.status-chip .val{font-size:16px;font-weight:600;}

.tbl{width:100%;border-collapse:collapse;background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);overflow:hidden;}
.tbl th{background:#fafafa;font-size:10px;text-transform:uppercase;letter-spacing:.08em;color:var(--text-dim);padding:10px 14px;text-align:left;font-weight:600;border-bottom:1px solid var(--border);}
.tbl td{padding:12px 14px;font-size:13px;border-bottom:1px solid #f0f0f0;}
.tbl tr:last-child td{border-bottom:none;}
.tbl tr:hover td{background:#fafafa;}
.col-num{width:140px;font-family:'Courier New',monospace;}
.col-date{width:110px;color:var(--text-muted);font-size:12px;}
.col-total{width:130px;text-align:right;font-family:'Courier New',monospace;font-weight:600;}
.col-status{width:110px;text-align:center;}
.col-act{width:130px;text-align:right;}

.badge{font-size:10px;font-weight:600;padding:4px 10px;border-radius:10px;text-transform:uppercase;letter-spacing:.05em;}
.badge-draft{background:#f0f0f0;color:#666;}
.badge-posted{background:#e8f7f1;color:#0f6e56;}
.badge-reversed{background:#fef5eb;color:#b9770e;}

/* Zoho push status chips (used in list + detail) */
.badge-zoho-notpushed{background:#f0f0f0;color:#666;}
.badge-zoho-pushed{background:#d4edda;color:#1e7e34;}
.badge-zoho-error{background:#f8d7da;color:#721c24;}
.col-zoho{width:110px;text-align:center;}

/* Zoho section inside detail card */
.zoho-section{padding:16px 24px;border-top:1px solid var(--border);background:#fafafa;display:flex;align-items:center;gap:14px;flex-wrap:wrap;}
.zoho-section .zoho-label{font-size:10px;text-transform:uppercase;letter-spacing:.08em;color:var(--text-dim);font-weight:600;}
.zoho-section .zoho-info{font-size:12px;color:var(--text-muted);flex:1;}
.zoho-section .zoho-info b{color:var(--text);font-family:'Courier New',monospace;}
.zoho-section .zoho-error{color:var(--danger);font-size:11px;font-style:italic;margin-top:4px;display:block;}
.btn-zoho{background:#d35400;color:#fff;}
.btn-zoho:hover{background:#a04000;}
.btn-zoho-retry{background:#fef5eb;color:#d35400;border:1px solid #f5cba7;}
.btn-zoho-retry:hover{background:#d35400;color:#fff;}

.empty-state{text-align:center;padding:48px 20px;color:var(--text-muted);font-size:13px;}
.empty-state strong{display:block;font-size:16px;color:var(--text);margin-bottom:6px;}

.link{color:var(--accent);text-decoration:none;font-weight:500;}
.link:hover{text-decoration:underline;}
.link-danger{color:var(--danger);}
.link-warn{color:#b9770e;}

/* ========= DETAIL MODE ========= */
.detail-card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);overflow:hidden;}
.detail-head{padding:20px 24px;border-bottom:1px solid var(--border);display:flex;align-items:center;justify-content:space-between;gap:18px;}
.detail-head-left{}
.detail-head-label{font-size:10px;text-transform:uppercase;letter-spacing:.08em;color:var(--text-dim);}
.detail-head-number{font-size:20px;font-weight:600;margin-top:2px;font-family:'Courier New',monospace;}
.detail-head-meta{font-size:11px;color:var(--text-muted);margin-top:6px;}
.detail-head-status{}

.detail-form{padding:18px 24px;border-bottom:1px solid var(--border);display:grid;grid-template-columns:180px 1fr 1fr;gap:16px;}
.detail-form label{display:block;font-size:10px;text-transform:uppercase;letter-spacing:.08em;color:var(--text-muted);margin-bottom:4px;font-weight:600;}
.detail-form input{width:100%;height:36px;padding:0 10px;border:1px solid var(--border);border-radius:8px;font-size:13px;font-family:inherit;}
.detail-form input:focus{outline:none;border-color:var(--accent);}
.detail-form input[readonly]{background:#fafafa;color:var(--text-muted);}

.lines-hdr{display:grid;grid-template-columns:30px 1fr 130px 130px 1fr 40px;gap:10px;padding:10px 24px;background:#fafafa;font-size:10px;text-transform:uppercase;letter-spacing:.08em;color:var(--text-muted);font-weight:600;border-bottom:1px solid var(--border);}
.lines-hdr>div:nth-child(3),.lines-hdr>div:nth-child(4){text-align:right;}

/* Each line has two rows: row1 has account/debit/credit/party, row2 has indented description spanning wide */
.line-row{padding:10px 24px 12px;border-bottom:1px solid #f0f0f0;}
.line-row .line-main{display:grid;grid-template-columns:30px 1fr 130px 130px 1fr 40px;gap:10px;align-items:center;}
.line-row .line-desc{display:grid;grid-template-columns:30px 1fr;gap:10px;align-items:center;margin-top:6px;}
.line-row .line-desc .desc-lbl{font-size:10px;color:var(--text-dim);text-align:right;padding-right:4px;text-transform:uppercase;letter-spacing:.05em;}

/* Natural-language caption under a line ("↳ Credits Highland Valley via Accounts Payable") */
.line-row .line-caption{display:grid;grid-template-columns:30px 1fr;gap:10px;margin-top:4px;}
.line-row .line-caption .cap-body{font-size:11px;color:var(--text-muted);line-height:1.4;}
.line-row .line-caption .cap-body .cap-party{font-weight:600;color:var(--text);}
.line-row .line-caption .cap-body .cap-via{color:var(--text-dim);}
.line-row .line-caption .cap-body .cap-diag{display:block;font-size:10px;color:var(--text-dim);font-family:'Courier New',monospace;margin-top:1px;letter-spacing:.02em;}
.line-row .idx{color:var(--text-dim);font-size:13px;text-align:center;}
.line-row select, .line-row input{height:36px;padding:0 10px;border:1px solid var(--border);border-radius:8px;font-size:13px;font-family:inherit;}
.line-row select{width:100%;}
.line-row input{width:100%;}
.line-row input.num{text-align:right;font-family:'Courier New',monospace;font-weight:600;}
.line-row input.num-empty{background:#fafafa;color:var(--text-dim);}
.line-row input:focus, .line-row select:focus{outline:none;border-color:var(--accent);}
.line-row input[readonly], .line-row select[disabled]{background:#fafafa;color:var(--text-muted);}
.line-row .del-btn{background:transparent;border:none;color:var(--danger);font-size:16px;cursor:pointer;padding:0;font-weight:600;}
.line-row .del-btn:hover{color:#922b21;}

/* Prompt when account type is payable/receivable but party is still blank */
.line-row select.party-prompt{border-color:var(--warn);background:#fef9ec;}
.line-row .party-hint{font-size:10px;color:#b9770e;margin-left:2px;font-style:italic;}

.lines-add{padding:12px 24px;border-bottom:1px solid var(--border);}
.btn-add-line{background:transparent;border:1px dashed var(--border);padding:8px 14px;border-radius:8px;font-size:12px;color:var(--text-muted);font-weight:600;cursor:pointer;font-family:inherit;}
.btn-add-line:hover{border-color:var(--accent);color:var(--accent);}

.totals-row{display:grid;grid-template-columns:30px 1fr 130px 130px 1fr 40px;gap:10px;padding:14px 24px;background:#f5f6fa;}
.totals-row .lbl{grid-column:2;font-size:12px;font-weight:600;color:var(--text-muted);text-transform:uppercase;letter-spacing:.06em;align-self:center;}
.totals-row .val{text-align:right;font-size:14px;font-weight:700;font-family:'Courier New',monospace;align-self:center;}
.totals-row .bal{grid-column:5;font-size:12px;font-weight:600;align-self:center;}
.totals-row .bal-ok{color:var(--teal);}
.totals-row .bal-off{color:var(--danger);}

.detail-actions{padding:18px 24px;display:flex;justify-content:flex-end;gap:10px;flex-wrap:wrap;}

.reversal-notice{background:#fef5eb;border:1px solid #f5cba7;border-radius:8px;padding:10px 14px;margin:16px 24px;font-size:12px;color:#b9770e;}
.reversal-notice a{color:#b9770e;font-weight:600;text-decoration:underline;}
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
        <a href="FINChartOfAccounts.aspx" class="nav-link">Chart of Accounts</a>
        <a href="FINHome.aspx" class="nav-link">&#8592; FIN Home</a>
        <a href="FINLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>

<div class="page-header">
    <div class="page-icon">&#x1F4D3;</div>
    <div class="page-title">JOURNAL <span>ENTRIES</span></div>
    <div class="page-sub">Manual double-entry accounting bookings. Expenses, accruals, reclassifications, reversals.</div>
</div>

<div class="container">

    <asp:PlaceHolder ID="phBanner" runat="server"></asp:PlaceHolder>

    <!-- LIST MODE -->
    <asp:Panel ID="pnlList" runat="server">

        <div class="status-bar">
            <div class="status-chip">
                <span class="dot draft"></span>
                <div><div class="lbl">Draft</div><div class="val"><asp:Literal ID="litDraftCount" runat="server" Text="0"/></div></div>
            </div>
            <div class="status-chip">
                <span class="dot posted"></span>
                <div><div class="lbl">Posted</div><div class="val"><asp:Literal ID="litPostedCount" runat="server" Text="0"/></div></div>
            </div>
            <div class="status-chip">
                <span class="dot reversed"></span>
                <div><div class="lbl">Reversed</div><div class="val"><asp:Literal ID="litReversedCount" runat="server" Text="0"/></div></div>
            </div>
            <div style="flex:1"></div>
            <asp:LinkButton ID="btnNewJournal" runat="server" CssClass="btn btn-primary" OnClick="btnNewJournal_Click">
                + New Journal Entry
            </asp:LinkButton>
        </div>

        <div class="filter-bar">
            <label>From</label>
            <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date"/>
            <label>To</label>
            <asp:TextBox ID="txtToDate" runat="server" TextMode="Date"/>
            <label>Status</label>
            <asp:DropDownList ID="ddlStatus" runat="server">
                <asp:ListItem Value=""         Text="All" Selected="True"/>
                <asp:ListItem Value="DRAFT"    Text="Draft"/>
                <asp:ListItem Value="POSTED"   Text="Posted"/>
                <asp:ListItem Value="REVERSED" Text="Reversed"/>
            </asp:DropDownList>
            <label>Number</label>
            <asp:TextBox ID="txtNumber" runat="server" placeholder="JV-..."/>
            <asp:LinkButton ID="btnFilter" runat="server" CssClass="btn btn-ghost" OnClick="btnFilter_Click">Apply</asp:LinkButton>
        </div>

        <asp:PlaceHolder ID="phList" runat="server"/>
    </asp:Panel>

    <!-- DETAIL MODE -->
    <asp:Panel ID="pnlDetail" runat="server" Visible="false">

        <div class="detail-card">

            <div class="detail-head">
                <div class="detail-head-left">
                    <div class="detail-head-label"><asp:Literal ID="litHeadLabel" runat="server" Text="New Journal Entry"/></div>
                    <div class="detail-head-number"><asp:Literal ID="litJournalNumber" runat="server" Text="(new)"/></div>
                    <div class="detail-head-meta"><asp:Literal ID="litMeta" runat="server"/></div>
                </div>
                <div class="detail-head-status">
                    <asp:Literal ID="litStatusBadge" runat="server"/>
                </div>
            </div>

            <asp:PlaceHolder ID="phReversalNotice" runat="server"/>

            <div class="detail-form">
                <div>
                    <label>Date</label>
                    <asp:TextBox ID="txtJournalDate" runat="server" TextMode="Date"/>
                </div>
                <div>
                    <label>Narration</label>
                    <asp:TextBox ID="txtNarration" runat="server" placeholder="e.g. Monthly rent — Apr 2026"/>
                </div>
                <div>
                    <label>Reference</label>
                    <asp:TextBox ID="txtReference" runat="server" placeholder="cheque no, voucher no, etc."/>
                </div>
            </div>

            <div class="lines-hdr">
                <div>#</div>
                <div>Account</div>
                <div>Debit</div>
                <div>Credit</div>
                <div>Party <span style="text-transform:none;font-weight:400;color:var(--text-dim);font-size:9px;">(optional)</span></div>
                <div></div>
            </div>

            <asp:PlaceHolder ID="phLines" runat="server"/>

            <div class="lines-add">
                <asp:LinkButton ID="btnAddLine" runat="server" CssClass="btn-add-line" OnClick="btnAddLine_Click">+ Add line</asp:LinkButton>
            </div>

            <div class="totals-row">
                <div class="lbl">Total</div>
                <div class="val"><asp:Literal ID="litTotalDebit" runat="server" Text="0.00"/></div>
                <div class="val"><asp:Literal ID="litTotalCredit" runat="server" Text="0.00"/></div>
                <div class="bal"><asp:Literal ID="litBalance" runat="server"/></div>
            </div>

            <asp:Panel ID="pnlZohoSection" runat="server" CssClass="zoho-section" Visible="false">
                <div class="zoho-label">Zoho Books</div>
                <div class="zoho-info">
                    <asp:Literal ID="litZohoStatus" runat="server"/>
                </div>
            </asp:Panel>

            <div class="detail-actions">
                <asp:LinkButton ID="btnCancel"    runat="server" CssClass="btn btn-ghost"  OnClick="btnCancel_Click">Cancel</asp:LinkButton>
                <asp:LinkButton ID="btnSaveDraft" runat="server" CssClass="btn btn-ghost"  OnClick="btnSaveDraft_Click">Save draft</asp:LinkButton>
                <asp:LinkButton ID="btnPost"      runat="server" CssClass="btn btn-teal"   OnClick="btnPost_Click"
                    OnClientClick="return erpConfirmLink(this,'Post this entry? Once posted it cannot be edited — only reversed.',{title:'Post Entry',okText:'Post',btnClass:'success',type:'warn'});">Post entry</asp:LinkButton>
                <asp:LinkButton ID="btnDelete"    runat="server" CssClass="btn btn-danger" OnClick="btnDelete_Click"
                    OnClientClick="return erpConfirmLink(this,'Delete this draft? This cannot be undone.',{title:'Delete Draft',okText:'Delete',btnClass:'danger',type:'danger'});">Delete draft</asp:LinkButton>
                <asp:LinkButton ID="btnReverse"   runat="server" CssClass="btn btn-warn"   OnClick="btnReverse_Click"
                    OnClientClick="return erpConfirmLink(this,'Reverse this posted entry? This will create a contra journal that flips every debit/credit.',{title:'Reverse Entry',okText:'Reverse',btnClass:'primary',type:'warn'});">Reverse entry</asp:LinkButton>
                <asp:LinkButton ID="btnPushToZoho" runat="server" CssClass="btn btn-zoho" OnClick="btnPushToZoho_Click" Visible="false"
                    OnClientClick="return erpConfirmLink(this,'Push this journal to Zoho Books? This is idempotent — if already pushed, nothing new will be created.',{title:'Push to Zoho',okText:'Push',btnClass:'primary',type:'info'});">&#x2197; Push to Zoho</asp:LinkButton>
                <asp:LinkButton ID="btnRepushToZoho" runat="server" CssClass="btn btn-zoho-retry" OnClick="btnPushToZoho_Click" Visible="false"
                    OnClientClick="return erpConfirmLink(this,'Retry push to Zoho Books? The previous attempt failed.',{title:'Retry Zoho Push',okText:'Retry',btnClass:'primary',type:'info'});">&#x21BB; Retry Zoho push</asp:LinkButton>
            </div>
        </div>

    </asp:Panel>

</div>

</form>
<script src="/StockApp/erp-modal.js"></script>
<script src="/StockApp/erp-keepalive.js"></script>
<script>
/*  wireJournalPartyPrompt — nudge the user to pick a Party when an AP/AR account is selected.
 *  Called (a) once on initial page load below, (b) again by server-side emitted script after every postback
 *  so newly-added / re-rendered rows get wired up.
 *  Data flow: window._acctTypes is an object mapping ZohoAccountID → lowercased account type name
 *  (emitted by FINJournal.aspx.cs RenderLineRows).
 *  If the chosen account's type contains "payable" or "receivable" AND party is empty, highlight the party select.
 */
window.wireJournalPartyPrompt = function(){
    var types = window._acctTypes || {};
    // Find all account dropdowns — their IDs end with "_ln_acc_<i>"
    var accs = document.querySelectorAll('select[id*="_ln_acc_"], select[id$="ln_acc_0"], select[id$="ln_acc_1"], select[id$="ln_acc_2"], select[id$="ln_acc_3"], select[id$="ln_acc_4"], select[id$="ln_acc_5"], select[id$="ln_acc_6"], select[id$="ln_acc_7"], select[id$="ln_acc_8"], select[id$="ln_acc_9"]');
    // Simpler: grab every select whose id contains "ln_acc_"
    var all = document.querySelectorAll('select');
    for (var i = 0; i < all.length; i++) {
        var el = all[i];
        if (!el.id || el.id.indexOf('ln_acc_') === -1) continue;
        // Derive the matching party select id by replacing ln_acc_ with ln_party_
        var partyId = el.id.replace('ln_acc_', 'ln_party_');
        var partySel = document.getElementById(partyId);
        if (!partySel) continue;

        var check = (function(accEl, partyEl){
            return function(){
                var val = accEl.value;
                var typeName = (types[val] || '').toLowerCase();
                var isPartyAccount = typeName.indexOf('payable') !== -1 || typeName.indexOf('receivable') !== -1;
                if (isPartyAccount && !partyEl.value) {
                    partyEl.classList.add('party-prompt');
                    partyEl.title = 'This looks like a vendor/customer account — consider picking a party.';
                } else {
                    partyEl.classList.remove('party-prompt');
                    partyEl.title = '';
                }
            };
        })(el, partySel);

        // Avoid double-binding on re-render — remove old handler reference first
        el.removeEventListener('change', el._partyCheck || function(){});
        partySel.removeEventListener('change', partySel._partyCheck || function(){});
        el._partyCheck = check;
        partySel._partyCheck = check;
        el.addEventListener('change', check);
        partySel.addEventListener('change', check);
        // Initial evaluation
        check();
    }
};
// First-run invocation (the server-side script runs after this DOM block only on initial GET;
// on postbacks the server-emitted script at the end of phLines re-wires).
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', window.wireJournalPartyPrompt);
} else {
    window.wireJournalPartyPrompt();
}
</script>
</body>
</html>
