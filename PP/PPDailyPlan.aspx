<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PPDailyPlan.aspx.cs" Inherits="PPApp.PPDailyPlan" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width, initial-scale=1.0"/>
<title>Sirimiri PP — Daily Production Plan</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root {
    --accent:#2ecc71; --accent-dark:#27ae60; --accent-light:#eafaf1;
    --red:#e74c3c;    --red-light:#fdf3f2;
    --blue:#2980b9;   --blue-light:#eaf4fb;
    --orange:#e67e22; --orange-light:#fef5ec;
    --text:#1a1a1a;   --text-muted:#666;    --text-dim:#999;
    --bg:#f0f0f0;     --surface:#fff;        --border:#e0e0e0;
    --radius:12px;    --nav-h:52px;
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

/* DATE BAR */
.date-bar{background:var(--surface);border-bottom:2px solid var(--accent);padding:12px 20px;
    display:flex;align-items:center;gap:12px;flex-wrap:wrap;}
.date-nav-btn{border:1.5px solid var(--border);background:#fff;border-radius:8px;
    padding:6px 14px;font-size:13px;font-weight:600;cursor:pointer;color:var(--text);transition:all .2s;}
.date-nav-btn:hover{border-color:var(--accent);color:var(--accent);}
.date-label{font-family:'Bebas Neue',sans-serif;font-size:22px;letter-spacing:.06em;color:var(--text);}
.status-badge{font-size:11px;font-weight:700;padding:3px 10px;border-radius:20px;letter-spacing:.05em;}
.status-badge.draft{background:#fff3cd;color:#856404;}
.status-badge.confirmed{background:#d1f5e0;color:#155724;}
.date-bar-actions{margin-left:auto;display:flex;gap:8px;}
.btn{padding:8px 18px;border-radius:8px;font-size:13px;font-weight:600;cursor:pointer;border:none;transition:all .2s;}
.btn-confirm{background:var(--accent);color:#fff;}
.btn-confirm:hover{background:var(--accent-dark);}
.btn-draft{background:#fff3cd;color:#856404;border:1.5px solid #ffc107;}
.btn-today{background:var(--blue);color:#fff;}
.btn-today:hover{background:#1f6fa3;}
.btn-pdf{background:#e74c3c;color:#fff;}
.btn-pdf:hover{background:#c0392b;}

/* ALERT */
.alert{padding:10px 16px;border-radius:8px;font-size:13px;margin:10px 20px 0;}
.alert-success{background:#d1f5e0;color:#155724;}
.alert-danger{background:#fdf3f2;color:#842029;}

/* FOUR QUADRANT GRID */
.quad-grid{display:grid;grid-template-columns:1fr 1fr;grid-template-rows:auto auto;gap:0;
    min-height:calc(100vh - var(--nav-h) - 62px);}

/* SHIFT PANELS (top row) */
.shift-panel{background:var(--surface);border:1px solid var(--border);padding:0;display:flex;flex-direction:column;}
.shift-panel:first-child{border-right:none;}
.shift-head{padding:14px 18px;border-bottom:2px solid var(--accent);display:flex;align-items:center;gap:10px;}
.shift-badge{background:var(--accent);color:#fff;font-family:'Bebas Neue',sans-serif;
    font-size:15px;letter-spacing:.06em;padding:2px 10px;border-radius:6px;}
.shift-title{font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.06em;color:var(--text);}
.shift-total{margin-left:auto;font-size:12px;font-weight:600;color:var(--accent-dark);}
.shift-body{flex:1;padding:14px 18px;overflow-y:auto;}

/* PLAN ROWS TABLE */
.plan-table{width:100%;border-collapse:collapse;font-size:13px;}
.plan-table th{text-align:left;font-size:10px;font-weight:700;letter-spacing:.08em;
    text-transform:uppercase;color:var(--text-dim);padding:0 8px 8px 0;border-bottom:1px solid var(--border);}
.plan-table td{padding:8px 8px 8px 0;border-bottom:1px solid var(--border);vertical-align:middle;}
.plan-table tr:last-child td{border-bottom:none;}
.prod-name{font-weight:500;}
.prod-code{font-size:10px;color:var(--text-dim);}
.batch-count{font-family:'Bebas Neue',sans-serif;font-size:20px;letter-spacing:.04em;color:var(--accent-dark);}
.batch-label{font-size:10px;color:var(--text-dim);}
.output-qty{font-size:12px;color:var(--blue);font-weight:600;}
.del-btn{background:none;border:none;color:var(--red);cursor:pointer;font-size:16px;padding:2px 6px;
    border-radius:4px;transition:background .2s;}
.del-btn:hover{background:var(--red-light);}

/* ADD ROW FORM */
.add-row-form{display:flex;gap:8px;align-items:flex-end;margin-top:14px;padding-top:12px;
    border-top:1px dashed var(--border);}
.add-row-form select, .add-row-form input{border:1.5px solid var(--border);border-radius:8px;
    padding:7px 10px;font-size:13px;background:#fff;font-family:inherit;}
.add-row-form select{flex:1;}
.add-row-form input{width:80px;text-align:center;}
.add-row-form select:focus, .add-row-form input:focus{outline:none;border-color:var(--accent);}
.btn-add-row{background:var(--accent);color:#fff;border:none;border-radius:8px;
    padding:7px 14px;font-size:13px;font-weight:600;cursor:pointer;white-space:nowrap;}
.btn-add-row:hover{background:var(--accent-dark);}
.empty-shift{text-align:center;padding:24px;color:var(--text-dim);font-size:13px;}

/* RM STATUS PANELS (bottom row) */
.rm-panel{grid-column:1/-1;background:var(--surface);border-top:2px solid var(--border);}
.rm-head{padding:14px 18px;border-bottom:2px solid var(--border);display:flex;align-items:center;gap:12px;}
.rm-title{font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.06em;color:var(--text);}
.rm-summary{font-size:12px;font-weight:600;padding:3px 10px;border-radius:20px;}
.rm-summary.ok{background:#d1f5e0;color:#155724;}
.rm-summary.warn{background:#fdf3f2;color:#842029;}
.rm-body{padding:14px 18px;overflow-x:auto;}

/* RM TABLE */
.rm-table{width:100%;border-collapse:collapse;font-size:13px;}
.rm-table th{text-align:left;font-size:10px;font-weight:700;letter-spacing:.08em;
    text-transform:uppercase;color:var(--text-dim);padding:0 12px 8px 0;border-bottom:2px solid var(--border);}
.rm-table th:not(:first-child){text-align:right;}
.rm-table td{padding:9px 12px 9px 0;border-bottom:1px solid var(--border);vertical-align:middle;}
.rm-table td:not(:first-child){text-align:right;font-variant-numeric:tabular-nums;}
.rm-table tr:last-child td{border-bottom:none;}
.rm-name{font-weight:500;}
.rm-code{font-size:10px;color:var(--text-dim);}
.rm-uom{font-size:11px;color:var(--text-dim);}
.shortfall{color:var(--red);font-weight:700;}
.surplus{color:var(--accent-dark);font-weight:600;}
.ok-val{color:var(--accent-dark);}
.empty-rm{text-align:center;padding:32px;color:var(--text-dim);font-size:13px;}

/* RESPONSIVE */
@media(max-width:768px){
    .quad-grid{grid-template-columns:1fr;}
    .shift-panel:first-child{border-right:1px solid var(--border);}
    .rm-panel{grid-column:1;}
}
</style>
</head>
<body>
<form id="form1" runat="server">

<asp:HiddenField ID="hfPlanID"   runat="server" Value="0"/>
<asp:HiddenField ID="hfPlanDate" runat="server" Value=""/>

<nav>
    <a href="PPHome.aspx" class="nav-logo">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    </a>
    <span class="nav-title">Production Planning</span>
    <div class="nav-right">
        <asp:Label ID="lblNavUser" runat="server" CssClass="nav-user"/>
        <a href="PPHome.aspx" class="nav-link">&#8592; PP Home</a>
        <a href="PPPrefilledEntry.aspx" class="nav-link" style="background:#7b1fa2;opacity:1;border-radius:6px;padding:5px 12px;">&#x1F9C3; Prefilled Entry</a>
        <a href="#" class="nav-link" onclick="erpConfirm('Sign out?',{title:'Sign Out',type:'warn',okText:'Sign Out',onOk:function(){window.location='PPLogout.aspx';}});return false;">Sign Out</a>
    </div>
</nav>

<!-- DATE BAR -->
<div class="date-bar">
    <asp:Button ID="btnPrevDay" runat="server" Text="&#9664; Prev" CssClass="date-nav-btn"
        OnClick="btnPrevDay_Click" CausesValidation="false"/>
    <asp:Button ID="btnToday" runat="server" Text="Today" CssClass="btn btn-today"
        OnClick="btnToday_Click" CausesValidation="false"/>
    <asp:Button ID="btnNextDay" runat="server" Text="Next &#9654;" CssClass="date-nav-btn"
        OnClick="btnNextDay_Click" CausesValidation="false"/>
    <asp:Label ID="lblPlanDate" runat="server" CssClass="date-label"/>
    <asp:Label ID="lblPlanStatus" runat="server" CssClass="status-badge draft" Text="Draft"/>
    <div class="date-bar-actions">
        <asp:Button ID="btnConfirm" runat="server" Text="Confirm Plan" CssClass="btn btn-confirm"
            OnClick="btnConfirm_Click" CausesValidation="false"/>
        <asp:Button ID="btnDraft" runat="server" Text="Revert to Draft" CssClass="btn btn-draft"
            OnClick="btnDraft_Click" CausesValidation="false" Visible="false"/>
        <asp:HyperLink ID="lnkPDF" runat="server" CssClass="btn btn-pdf"
            Target="_blank" Text="&#128438; Download PDF"/>
    </div>
</div>

<asp:Panel ID="pnlAlert" runat="server" Visible="false" style="margin:10px 20px 0;">
    <asp:Label ID="lblAlert" runat="server"/>
</asp:Panel>

<!-- FOUR QUADRANTS -->
<div class="quad-grid">

    <!-- SHIFT 1 -->
    <div class="shift-panel">
        <div class="shift-head">
            <span class="shift-badge">SHIFT 1</span>
            <span class="shift-title">Morning Shift</span>
            <asp:Label ID="lblS1Total" runat="server" CssClass="shift-total" Text="0 batches"/>
        </div>
        <div class="shift-body">
            <asp:Panel ID="pnlShift1Empty" runat="server" Visible="true">
                <div class="empty-shift">No products scheduled yet. Add below.</div>
            </asp:Panel>

            <asp:Repeater ID="rptShift1" runat="server" OnItemCommand="rptShift1_ItemCommand">
                <HeaderTemplate>
                    <table class="plan-table">
                    <tr>
                        <th>Product</th>
                        <th>Batches</th>
                        <th>Output</th>
                        <th></th>
                    </tr>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td>
                            <div class="prod-name"><%# Eval("ProductName") %></div>
                            <div class="prod-code"><%# Eval("ProductCode") %></div>
                        </td>
                        <td>
                            <div class="batch-count"><%# FormatDecimal(Eval("Batches")) %></div>
                            <div class="batch-label"><%# Eval("ProdAbbr") %></div>
                        </td>
                        <td>
                            <div class="output-qty">
                                <%# FormatDecimal(Convert.ToDecimal(Eval("Batches")) * Convert.ToDecimal(Eval("BatchSize"))) %>
                                <%# Eval("OutputAbbr") %>
                            </div>
                        </td>
                        <td>
                            <asp:LinkButton runat="server" CommandName="Delete"
                                CommandArgument='<%# Eval("RowID") %>'
                                CssClass="del-btn" CausesValidation="false">&#x2715;</asp:LinkButton>
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></table></FooterTemplate>
            </asp:Repeater>

            <!-- Add Row -->
            <div class="add-row-form">
                <asp:DropDownList ID="ddlS1Product" runat="server" onchange="updateBatchPlaceholder(this, 'txtS1Batches')"/>
                <asp:TextBox ID="txtS1Batches" runat="server" placeholder="Batches" MaxLength="6"/>
                <asp:Button ID="btnAddS1" runat="server" Text="+ Add" CssClass="btn-add-row"
                    OnClick="btnAddS1_Click"/>
            </div>
        </div>
    </div>

    <!-- SHIFT 2 -->
    <div class="shift-panel">
        <div class="shift-head">
            <span class="shift-badge" style="background:var(--blue);">SHIFT 2</span>
            <span class="shift-title">Evening Shift</span>
            <asp:Label ID="lblS2Total" runat="server" CssClass="shift-total" Text="0 batches"/>
        </div>
        <div class="shift-body">
            <asp:Panel ID="pnlShift2Empty" runat="server" Visible="true">
                <div class="empty-shift">No products scheduled yet. Add below.</div>
            </asp:Panel>

            <asp:Repeater ID="rptShift2" runat="server" OnItemCommand="rptShift2_ItemCommand">
                <HeaderTemplate>
                    <table class="plan-table">
                    <tr>
                        <th>Product</th>
                        <th>Batches</th>
                        <th>Output</th>
                        <th></th>
                    </tr>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td>
                            <div class="prod-name"><%# Eval("ProductName") %></div>
                            <div class="prod-code"><%# Eval("ProductCode") %></div>
                        </td>
                        <td>
                            <div class="batch-count"><%# FormatDecimal(Eval("Batches")) %></div>
                            <div class="batch-label"><%# Eval("ProdAbbr") %></div>
                        </td>
                        <td>
                            <div class="output-qty">
                                <%# FormatDecimal(Convert.ToDecimal(Eval("Batches")) * Convert.ToDecimal(Eval("BatchSize"))) %>
                                <%# Eval("OutputAbbr") %>
                            </div>
                        </td>
                        <td>
                            <asp:LinkButton runat="server" CommandName="Delete"
                                CommandArgument='<%# Eval("RowID") %>'
                                CssClass="del-btn" CausesValidation="false">&#x2715;</asp:LinkButton>
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></table></FooterTemplate>
            </asp:Repeater>

            <!-- Add Row -->
            <div class="add-row-form">
                <asp:DropDownList ID="ddlS2Product" runat="server" onchange="updateBatchPlaceholder(this, 'txtS2Batches')"/>
                <asp:TextBox ID="txtS2Batches" runat="server" placeholder="Batches" MaxLength="6"/>
                <asp:Button ID="btnAddS2" runat="server" Text="+ Add" CssClass="btn-add-row"
                    OnClick="btnAddS2_Click"/>
            </div>
        </div>
    </div>

    <!-- RM STATUS (full width bottom) -->
    <div class="rm-panel">
        <div class="rm-head">
            <span class="rm-title">Raw Material Requirement vs Stock</span>
            <asp:Label ID="lblRMSummary" runat="server" CssClass="rm-summary ok" Text=""/>
        </div>
        <div class="rm-body">
            <asp:Panel ID="pnlRMEmpty" runat="server" Visible="true">
                <div class="empty-rm">No shortages — all raw materials are sufficient.</div>
            </asp:Panel>

            <asp:Repeater ID="rptRM" runat="server">
                <HeaderTemplate>
                    <table class="rm-table">
                    <tr>
                        <th>Raw Material</th>
                        <th>Required</th>
                        <th>In Stock</th>
                        <th>Shortfall</th>
                    </tr>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td>
                            <div class="rm-name"><%# Eval("RMName") %></div>
                            <div class="rm-code"><%# Eval("RMCode") %></div>
                        </td>
                        <td><%# FormatQtyWithUOM(Eval("Required"), Eval("Abbreviation")) %></td>
                        <td><%# FormatQtyWithUOM(Eval("InStock"),  Eval("Abbreviation")) %></td>
                        <td class='<%# ShortfallClass(Eval("Shortfall")) %>'>
                            <%# ShortfallDisplay(Eval("Shortfall"), Eval("Abbreviation")) %>
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></table></FooterTemplate>
            </asp:Repeater>
        </div>
    </div>

</div><!-- /quad-grid -->

</form>

<script>
function updateBatchPlaceholder(ddl, inputId) {
    var selected = ddl.options[ddl.selectedIndex];
    var uom = selected ? (selected.getAttribute('data-produom') || 'Qty') : 'Qty';
    if (!uom || uom === '') uom = 'Qty';
    // Find the input — it's a server control so we search by ending ID
    var inputs = document.querySelectorAll('input[type=text]');
    inputs.forEach(function(inp) {
        if (inp.id && inp.id.indexOf(inputId) >= 0) {
            inp.placeholder = uom;
            inp.title = 'Enter quantity in ' + uom;
        }
    });
}
</script>
<script src="/StockApp/erp-modal.js"></script>
<script src="/StockApp/erp-keepalive.js"></script>
</body>
</html>
