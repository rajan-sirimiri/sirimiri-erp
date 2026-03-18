<%@ Page Language="C#" AutoEventWireup="true" Inherits="PPApp.PPProductionOrder" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri PP — Production Order</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<style>
:root{
    --accent:#2ecc71; --accent-dark:#27ae60; --accent-light:#eafaf1;
    --blue:#2980b9;   --blue-light:#eaf4fb;
    --red:#e74c3c;    --red-light:#fdf3f2;
    --orange:#e67e22; --orange-light:#fef5ec;
    --text:#1a1a1a;   --text-muted:#666; --text-dim:#999;
    --bg:#f0f0f0;     --surface:#fff;    --border:#e0e0e0;
    --radius:12px;    --nav-h:52px;
}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);color:var(--text);min-height:100vh;}

/* NAV */
nav{background:#1a1a1a;height:var(--nav-h);display:flex;align-items:center;padding:0 20px;gap:12px;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;}
.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.08em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:16px;}
.nav-user{color:rgba(255,255,255,.8);font-size:12px;}
.nav-link{color:#fff;font-size:12px;font-weight:600;text-decoration:none;opacity:.8;}
.nav-link:hover{opacity:1;}

/* DATE BAR */
.date-bar{background:var(--surface);border-bottom:3px solid #1a1a1a;
    padding:12px 20px;display:flex;align-items:center;gap:12px;}
.date-label{font-family:'Bebas Neue',sans-serif;font-size:22px;letter-spacing:.06em;}
.status-badge{font-size:11px;font-weight:700;padding:3px 10px;border-radius:20px;letter-spacing:.05em;}
.status-badge.draft{background:#fff3cd;color:#856404;}
.status-badge.confirmed{background:#d1f5e0;color:#155724;}

/* ALERT */
.alert{padding:10px 16px;border-radius:8px;font-size:13px;margin:10px 20px 0;}
.alert-success{background:#d1f5e0;color:#155724;}
.alert-danger{background:#fdf3f2;color:#842029;}

/* SPLIT LAYOUT */
.split{display:grid;grid-template-columns:1fr 1fr;height:calc(100vh - var(--nav-h) - 56px);}
.pane{display:flex;flex-direction:column;overflow:hidden;border-right:1px solid var(--border);}
.pane:last-child{border-right:none;}
.pane-head{padding:14px 20px;background:var(--surface);border-bottom:2px solid var(--border);flex-shrink:0;}
.pane-title{font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.06em;}
.pane-body{flex:1;overflow-y:auto;padding:16px 20px;}

/* SHIFT TABS */
.shift-tabs{display:flex;gap:0;margin-bottom:0;}
.shift-tab{padding:8px 20px;font-size:12px;font-weight:700;letter-spacing:.05em;
    cursor:pointer;border:none;background:var(--bg);color:var(--text-muted);
    border-bottom:3px solid transparent;transition:all .2s;}
.shift-tab.active{background:var(--surface);color:var(--accent-dark);border-bottom-color:var(--accent);}
.shift-tab.s2.active{color:var(--blue);border-bottom-color:var(--blue);}
.shift-tab:hover{color:var(--text);}
.shift-content{display:none;}
.shift-content.visible{display:block;}

/* ORDER TABLE */
.order-table{width:100%;border-collapse:collapse;font-size:13px;}
.order-table th{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;
    color:var(--text-dim);padding:0 8px 10px 0;border-bottom:2px solid var(--border);text-align:left;}
.order-table th:not(:first-child){text-align:center;}
.order-table td{padding:10px 8px 10px 0;border-bottom:1px solid var(--border);vertical-align:middle;}
.order-table td:not(:first-child){text-align:center;}
.sr-num{font-family:'Bebas Neue',sans-serif;font-size:18px;color:var(--text-dim);}
.prod-name{font-weight:600;font-size:13px;}
.prod-code{font-size:10px;color:var(--text-dim);}
.batch-num{font-family:'Bebas Neue',sans-serif;font-size:22px;letter-spacing:.04em;color:var(--accent-dark);}
.batch-uom{font-size:10px;color:var(--text-dim);}
.revised-input{width:70px;border:1.5px solid var(--border);border-radius:6px;
    padding:5px 8px;font-size:13px;text-align:center;font-family:inherit;}
.revised-input:focus{outline:none;border-color:var(--accent);}
.revised-input:disabled{background:var(--bg);color:var(--text-muted);}

/* INITIATE BUTTON */
.btn-initiate{background:var(--accent);color:#fff;border:none;border-radius:8px;
    padding:7px 14px;font-size:12px;font-weight:700;cursor:pointer;
    white-space:nowrap;transition:all .2s;letter-spacing:.03em;}
.btn-initiate:hover{background:var(--accent-dark);}
.btn-initiate:disabled{background:#ccc;cursor:not-allowed;}
.btn-inprogress{background:var(--orange);color:#fff;border:none;border-radius:8px;
    padding:7px 14px;font-size:12px;font-weight:700;cursor:default;letter-spacing:.03em;}
.btn-completed{background:var(--accent-dark);color:#fff;border:none;border-radius:8px;
    padding:7px 14px;font-size:12px;font-weight:700;cursor:default;letter-spacing:.03em;}

/* STATUS BADGES */
.badge-pending{background:#f8f9fa;color:#666;font-size:10px;font-weight:700;
    padding:2px 8px;border-radius:10px;border:1px solid #dee2e6;}
.badge-initiated{background:var(--orange-light);color:var(--orange);font-size:10px;
    font-weight:700;padding:2px 8px;border-radius:10px;}
.badge-inprogress{background:var(--blue-light);color:var(--blue);font-size:10px;
    font-weight:700;padding:2px 8px;border-radius:10px;}
.badge-completed{background:var(--accent-light);color:var(--accent-dark);font-size:10px;
    font-weight:700;padding:2px 8px;border-radius:10px;}

/* EMPTY STATES */
.empty-state{text-align:center;padding:40px 20px;color:var(--text-dim);font-size:13px;}
.empty-icon{font-size:36px;margin-bottom:12px;}
.no-plan-warn{background:#fff3cd;border:1px solid #ffc107;border-radius:10px;
    padding:16px 20px;margin:16px 0;font-size:13px;color:#664d00;}

/* PROGRESS PANEL */
.progress-card{background:var(--surface);border:1px solid var(--border);
    border-radius:var(--radius);padding:16px;margin-bottom:14px;
    box-shadow:0 2px 8px rgba(0,0,0,.04);}
.progress-card-head{display:flex;align-items:flex-start;
    justify-content:space-between;margin-bottom:12px;}
.progress-prod-name{font-weight:600;font-size:13px;}
.progress-prod-code{font-size:10px;color:var(--text-dim);}
.progress-shift-badge{font-size:10px;font-weight:700;padding:2px 8px;border-radius:10px;}
.shift1-badge{background:var(--accent-light);color:var(--accent-dark);}
.shift2-badge{background:var(--blue-light);color:var(--blue);}

/* PROGRESS BAR */
.prog-bar-wrap{background:#f0f0f0;border-radius:6px;height:8px;margin:8px 0;overflow:hidden;}
.prog-bar-fill{height:100%;border-radius:6px;background:var(--accent);transition:width .3s;}
.prog-label{display:flex;justify-content:space-between;font-size:11px;color:var(--text-muted);margin-bottom:4px;}

/* OUTPUT & RM */
.stat-row{display:flex;justify-content:space-between;font-size:12px;
    padding:4px 0;border-bottom:1px solid var(--border);}
.stat-row:last-child{border-bottom:none;}
.stat-label{color:var(--text-muted);}
.stat-value{font-weight:600;color:var(--text);}
.rm-est-section{margin-top:10px;}
.rm-est-title{font-size:10px;font-weight:700;letter-spacing:.06em;text-transform:uppercase;
    color:var(--text-dim);margin-bottom:6px;}
.rm-est-row{display:flex;justify-content:space-between;font-size:11px;padding:3px 0;
    border-bottom:1px dashed var(--border);}
.rm-est-row:last-child{border-bottom:none;}
.rm-est-name{color:var(--text-muted);}
.rm-est-qty{font-weight:600;}

@media(max-width:900px){
    .split{grid-template-columns:1fr;height:auto;}
    .pane{height:auto;}
}
</style>
</head>
<body>
<form id="form1" runat="server">

<asp:HiddenField ID="hfPlanID"     runat="server" Value="0"/>
<asp:HiddenField ID="hfActiveShift" runat="server" Value="1"/>

<nav>
    <a href="PPHome.aspx" class="nav-logo">
        <img src="Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    </a>
    <span class="nav-title">Production Order</span>
    <div class="nav-right">
        <asp:Label ID="lblNavUser" runat="server" CssClass="nav-user"/>
        <a href="PPHome.aspx" class="nav-link">&#8592; PP Home</a>
        <a href="PPLogout.aspx" class="nav-link" onclick="return confirm('Sign out?')">Sign Out</a>
    </div>
</nav>

<!-- DATE BAR -->
<div class="date-bar">
    <asp:Label ID="lblTodayDate" runat="server" CssClass="date-label"/>
    <asp:Label ID="lblPlanStatus" runat="server" CssClass="status-badge draft" Text="—"/>
    <span style="font-size:12px;color:var(--text-muted);margin-left:4px;">Plan Status</span>
</div>

<asp:Panel ID="pnlAlert" runat="server" Visible="false">
    <asp:Label ID="lblAlert" runat="server"/>
</asp:Panel>

<!-- SPLIT LAYOUT -->
<div class="split">

    <!-- LEFT PANE — ORDERS -->
    <div class="pane">
        <div class="pane-head">
            <div class="pane-title">Production Orders</div>
            <div class="shift-tabs">
                <asp:LinkButton ID="btnShift1Tab" runat="server" CssClass="shift-tab active"
                    OnClick="btnShift1_Click" CausesValidation="false">SHIFT 1 — MORNING</asp:LinkButton>
                <asp:LinkButton ID="btnShift2Tab" runat="server" CssClass="shift-tab s2"
                    OnClick="btnShift2_Click" CausesValidation="false">SHIFT 2 — EVENING</asp:LinkButton>
            </div>
        </div>
        <div class="pane-body">

            <!-- No Plan Warning -->
            <asp:Panel ID="pnlNoplan" runat="server" Visible="false">
                <div class="no-plan-warn">
                    &#9888; No production plan found for today.
                    Please create a plan in <a href="PPDailyPlan.aspx">Production Planning</a> first.
                </div>
            </asp:Panel>

            <!-- SHIFT 1 ORDERS -->
            <div id="divShift1" runat="server">
                <asp:Panel ID="pnlShift1Empty" runat="server" Visible="false">
                    <div class="empty-state">
                        <div class="empty-icon">&#128203;</div>
                        <div>No products scheduled in Shift 1 today.</div>
                    </div>
                </asp:Panel>

                <asp:Repeater ID="rptShift1Orders" runat="server"
                    OnItemCommand="rptShift1Orders_ItemCommand">
                    <HeaderTemplate>
                        <table class="order-table">
                        <tr>
                            <th>Sr</th>
                            <th>Product</th>
                            <th>Ordered</th>
                            <th>Revised</th>
                            <th>Status</th>
                            <th>Action</th>
                        </tr>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td><span class="sr-num"><%# Container.ItemIndex + 1 %></span></td>
                            <td>
                                <div class="prod-name"><%# Eval("ProductName") %></div>
                                <div class="prod-code"><%# Eval("ProductCode") %></div>
                            </td>
                            <td>
                                <div class="batch-num"><%# Eval("OrderedBatches") %></div>
                                <div class="batch-uom"><%# Eval("ProdAbbr") %></div>
                            </td>
                            <td>
                                <asp:TextBox ID="txtRevised" runat="server"
                                    CssClass="revised-input"
                                    Text='<%# Eval("RevisedBatches") %>'
                                    Enabled='<%# CanInitiate(Eval("Status")) %>'
                                    placeholder="—"/>
                            </td>
                            <td>
                                <span class='<%# StatusClass(Eval("Status")) %>'>
                                    <%# Eval("Status") %>
                                </span>
                            </td>
                            <td>
                                <%# CanInitiate(Eval("Status"))
                                    ? "" : "" %>
                                <asp:LinkButton runat="server"
                                    CommandName="Initiate"
                                    CommandArgument='<%# Eval("OrderID") %>'
                                    CssClass="btn-initiate"
                                    Visible='<%# CanInitiate(Eval("Status")) %>'
                                    OnClientClick="return confirmInitiate(this)"
                                    CausesValidation="false">Initiate Production</asp:LinkButton>
                                <%# !CanInitiate(Eval("Status"))
                                    ? "<span class='btn-" + Eval("Status").ToString().ToLower() + "'>" + ButtonLabel(Eval("Status")) + "</span>"
                                    : "" %>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate></table></FooterTemplate>
                </asp:Repeater>
            </div>

            <!-- SHIFT 2 ORDERS -->
            <div id="divShift2" runat="server" style="display:none;">
                <asp:Panel ID="pnlShift2Empty" runat="server" Visible="false">
                    <div class="empty-state">
                        <div class="empty-icon">&#128203;</div>
                        <div>No products scheduled in Shift 2 today.</div>
                    </div>
                </asp:Panel>

                <asp:Repeater ID="rptShift2Orders" runat="server"
                    OnItemCommand="rptShift2Orders_ItemCommand">
                    <HeaderTemplate>
                        <table class="order-table">
                        <tr>
                            <th>Sr</th>
                            <th>Product</th>
                            <th>Ordered</th>
                            <th>Revised</th>
                            <th>Status</th>
                            <th>Action</th>
                        </tr>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td><span class="sr-num"><%# Container.ItemIndex + 1 %></span></td>
                            <td>
                                <div class="prod-name"><%# Eval("ProductName") %></div>
                                <div class="prod-code"><%# Eval("ProductCode") %></div>
                            </td>
                            <td>
                                <div class="batch-num"><%# Eval("OrderedBatches") %></div>
                                <div class="batch-uom"><%# Eval("ProdAbbr") %></div>
                            </td>
                            <td>
                                <asp:TextBox ID="txtRevised" runat="server"
                                    CssClass="revised-input"
                                    Text='<%# Eval("RevisedBatches") %>'
                                    Enabled='<%# CanInitiate(Eval("Status")) %>'
                                    placeholder="—"/>
                            </td>
                            <td>
                                <span class='<%# StatusClass(Eval("Status")) %>'>
                                    <%# Eval("Status") %>
                                </span>
                            </td>
                            <td>
                                <asp:LinkButton runat="server"
                                    CommandName="Initiate"
                                    CommandArgument='<%# Eval("OrderID") %>'
                                    CssClass="btn-initiate"
                                    Visible='<%# CanInitiate(Eval("Status")) %>'
                                    OnClientClick="return confirmInitiate(this)"
                                    CausesValidation="false">Initiate Production</asp:LinkButton>
                                <%# !CanInitiate(Eval("Status"))
                                    ? "<span class='btn-" + Eval("Status").ToString().ToLower() + "'>" + ButtonLabel(Eval("Status")) + "</span>"
                                    : "" %>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate></table></FooterTemplate>
                </asp:Repeater>
            </div>

        </div>
    </div><!-- /left pane -->

    <!-- RIGHT PANE — PROGRESS -->
    <div class="pane">
        <div class="pane-head">
            <div class="pane-title">Production Progress</div>
            <div style="font-size:11px;color:var(--text-dim);margin-top:2px;">
                Orders initiated today — estimated RM consumption based on BOM
            </div>
        </div>
        <div class="pane-body">

            <asp:Panel ID="pnlProgressEmpty" runat="server" Visible="true">
                <div class="empty-state">
                    <div class="empty-icon">&#9200;</div>
                    <div>No orders initiated yet today.<br/>
                    Initiate a production order to see progress here.</div>
                </div>
            </asp:Panel>

            <asp:Repeater ID="rptProgress" runat="server">
                <ItemTemplate>
                    <div class="progress-card">
                        <div class="progress-card-head">
                            <div>
                                <div class="progress-prod-name"><%# Eval("ProductName") %></div>
                                <div class="progress-prod-code"><%# Eval("ProductCode") %></div>
                            </div>
                            <span class='progress-shift-badge <%# Convert.ToInt32(Eval("Shift"))==1 ? "shift1-badge" : "shift2-badge" %>'>
                                SHIFT <%# Eval("Shift") %>
                            </span>
                        </div>

                        <!-- Progress bar — driven by Execution module later -->
                        <div class="prog-label">
                            <span>Progress</span>
                            <span class='<%# StatusClass(Eval("Status")) %>'><%# Eval("Status") %></span>
                        </div>
                        <div class="prog-bar-wrap">
                            <div class="prog-bar-fill" style="width:<%# Eval("Status").ToString()=="Completed" ? "100" : Eval("Status").ToString()=="InProgress" ? "50" : "10" %>%"></div>
                        </div>

                        <!-- Stats -->
                        <div class="stat-row">
                            <span class="stat-label">Batches Ordered</span>
                            <span class="stat-value">
                                <%# Eval("EffectiveBatches") %> <%# Eval("ProdAbbr") %>
                            </span>
                        </div>
                        <div class="stat-row">
                            <span class="stat-label">Expected Output</span>
                            <span class="stat-value">
                                <%# FormatOutput(Eval("EffectiveBatches"), Eval("BatchSize"), Eval("OutputAbbr")) %>
                            </span>
                        </div>
                        <div class="stat-row">
                            <span class="stat-label">Initiated At</span>
                            <span class="stat-value">
                                <%# Eval("InitiatedAt") != DBNull.Value
                                    ? Convert.ToDateTime(Eval("InitiatedAt")).ToString("hh:mm tt")
                                    : "—" %>
                            </span>
                        </div>

                        <!-- RM Estimate -->
                        <div class="rm-est-section">
                            <div class="rm-est-title">Estimated RM Consumption</div>
                            <%# BuildRMEstimate(Eval("OrderID")) %>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>

        </div>
    </div><!-- /right pane -->

</div><!-- /split -->

</form>

<script>
function confirmInitiate(btn) {
    var row  = btn.closest('tr');
    var name = row ? row.querySelector('.prod-name') : null;
    var prod = name ? name.innerText : 'this product';
    var rev  = row ? row.querySelector('.revised-input') : null;
    var batches = rev && rev.value ? rev.value : 
                  row ? (row.querySelector('.batch-num') || {}).innerText : '?';
    return confirm('Initiate production of ' + prod + '?\n\nBatches: ' + batches + 
                   '\n\nThis will start the production order and redirect to Execution.');
}

// Tab switching — show/hide shift divs
document.addEventListener('DOMContentLoaded', function() {
    var activeShift = document.getElementById('<%= hfActiveShift.ClientID %>').value || '1';
    showShift(activeShift);
});

function showShift(n) {
    var d1 = document.getElementById('<%= divShift1.ClientID %>');
    var d2 = document.getElementById('<%= divShift2.ClientID %>');
    if (d1) d1.style.display = n == '2' ? 'none' : 'block';
    if (d2) d2.style.display = n == '2' ? 'block' : 'none';
}
</script>
</body>
</html>
