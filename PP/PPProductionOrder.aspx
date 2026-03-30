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
.badge-stopped{background:#fff3cd;color:#856404;font-size:10px;
    font-weight:700;padding:2px 8px;border-radius:10px;}
.btn-stop{background:var(--red);color:#fff;border:none;border-radius:8px;
    padding:5px 12px;font-size:11px;font-weight:700;cursor:pointer;letter-spacing:.03em;}
.btn-stop:hover{background:#c0392b;}
.btn-resume{background:var(--blue);color:#fff;border:none;border-radius:8px;
    padding:5px 12px;font-size:11px;font-weight:700;cursor:pointer;letter-spacing:.03em;}
.btn-resume:hover{background:#1f6fa3;}
.btn-save-rev{background:#f0f0f0;color:#333;border:1px solid #ccc;border-radius:6px;
    padding:5px 10px;font-size:11px;font-weight:600;cursor:pointer;}
.btn-save-rev:hover{background:#e0e0e0;}
.btn-stock-short{background:#e67e22;color:#fff;border:none;border-radius:8px;
    padding:5px 12px;font-size:11px;font-weight:700;cursor:not-allowed;}
.progress-text{font-size:12px;font-weight:700;color:var(--accent-dark);}
.prog-bar-wrap{background:#f0f0f0;border-radius:6px;height:6px;margin:4px 0 8px;overflow:hidden;}
.prog-bar-fill{height:100%;border-radius:6px;background:var(--accent);transition:width .3s;}
.variation-row{font-size:12px;margin-top:4px;}

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

/* STOP MODAL */
.modal-stop .modal-icon{ font-size:34px; }
.modal-btn-stop{background:var(--red);color:#fff;border:none;border-radius:8px;
    padding:10px 24px;font-size:13px;font-weight:700;cursor:pointer;letter-spacing:.03em;}
.modal-btn-stop:hover{background:#c0392b;}

/* CONFIRMATION MODAL */
.modal-overlay{display:none;position:fixed;inset:0;background:rgba(0,0,0,.55);
    z-index:1000;align-items:center;justify-content:center;}
.modal-overlay.visible{display:flex;}
.modal-box{background:#fff;border-radius:16px;padding:28px 28px 22px;
    max-width:420px;width:92%;box-shadow:0 8px 40px rgba(0,0,0,.2);animation:slideUp .2s ease;}
@keyframes slideUp{from{transform:translateY(20px);opacity:0}to{transform:translateY(0);opacity:1}}
.modal-icon{font-size:34px;margin-bottom:10px;}
.modal-title{font-family:'Bebas Neue',sans-serif;font-size:22px;letter-spacing:.06em;
    margin-bottom:8px;color:#1a1a1a;}
.modal-product{font-size:14px;font-weight:600;color:var(--accent-dark);margin-bottom:6px;}
.modal-detail{font-size:13px;color:var(--text-muted);line-height:1.6;margin-bottom:20px;}
.modal-actions{display:flex;gap:10px;justify-content:flex-end;}
.modal-btn-cancel{background:#f0f0f0;color:#555;border:none;border-radius:8px;
    padding:10px 20px;font-size:13px;font-weight:600;cursor:pointer;}
.modal-btn-cancel:hover{background:#e0e0e0;}
.modal-btn-confirm{background:var(--accent);color:#fff;border:none;border-radius:8px;
    padding:10px 24px;font-size:13px;font-weight:700;cursor:pointer;letter-spacing:.03em;}
.modal-btn-confirm:hover{background:var(--accent-dark);}
</style>
</head>
<body>
<form id="form1" runat="server">

<asp:HiddenField ID="hfPlanID"     runat="server" Value="0"/>
<asp:HiddenField ID="hfActiveShift" runat="server" Value="1"/>

<nav>
    <a href="PPHome.aspx" class="nav-logo">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    </a>
    <span class="nav-title">Production Order</span>
    <div class="nav-right">
        <asp:Label ID="lblNavUser" runat="server" CssClass="nav-user"/>
        <a href="PPHome.aspx" class="nav-link">&#8592; PP Home</a>
        <a href="#" class="nav-link" onclick="erpConfirm('Sign out?',{title:'Sign Out',type:'warn',okText:'Sign Out',onOk:function(){window.location='PPLogout.aspx';}});return false;">Sign Out</a>
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
                            <th>Revised ✓</th>
                            <th>Progress / Status</th>
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
                                <div style="display:flex;gap:4px;align-items:center;justify-content:center;">
                                    <asp:TextBox ID="txtRevised" runat="server"
                                        CssClass="revised-input"
                                        Text='<%# Eval("RevisedBatches") %>'
                                        Enabled='<%# Eval("Status").ToString() != "Completed" %>'
                                        placeholder="—"/>
                                    <asp:LinkButton runat="server"
                                        CommandName="SaveRevised"
                                        CommandArgument='<%# Eval("OrderID") %>'
                                        CssClass="btn-save-rev"
                                        Visible='<%# Eval("Status").ToString() != "Completed" %>'
                                        CausesValidation="false">✓</asp:LinkButton>
                                </div>
                            </td>
                            <td>
                                <div class="progress-text">
                                    <%# FormatProgress(Eval("CompletedBatches"), Eval("EffectiveBatches"), Eval("OrderedBatches")) %>
                                </div>
                                <div class="prog-bar-wrap">
                                    <div class="prog-bar-fill" style="width:<%# ProgressBarWidth(Eval("CompletedBatches"), Eval("EffectiveBatches")) %>%"></div>
                                </div>
                                <span class='<%# StatusClass(Eval("Status")) %>'><%# Eval("Status") %></span>
                            </td>
                            <td>
                                <asp:LinkButton runat="server"
                                    CommandName="Initiate"
                                    CommandArgument='<%# Eval("OrderID") %>'
                                    CssClass='<%# GetInitiateCssClass(Eval("Status"), Eval("OrderID")) %>'
                                    Visible='<%# CanInitiate(Eval("Status")) %>'
                                    Enabled='<%# CanInitiateWithStock(Eval("Status"), Eval("OrderID")) %>'
                                    ToolTip='<%# StockTooltip(Eval("Status"), Eval("OrderID")) %>'
                                    OnClientClick="return confirmInitiate(this);"
                                    CausesValidation="false"><%# GetInitiateLabel(Eval("Status"), Eval("OrderID")) %></asp:LinkButton>
                                <asp:LinkButton runat="server"
                                    CommandName="Stop"
                                    CommandArgument='<%# Eval("OrderID") %>'
                                    CssClass="btn-stop"
                                    Visible='<%# Eval("Status").ToString() == "InProgress" || Eval("Status").ToString() == "Initiated" %>'
                                    OnClientClick="return confirmStop(this);"
                                    CausesValidation="false">⏸ Stop</asp:LinkButton>
                                <asp:LinkButton runat="server"
                                    CommandName="Resume"
                                    CommandArgument='<%# Eval("OrderID") %>'
                                    CssClass="btn-resume"
                                    Visible='<%# Eval("Status").ToString() == "Stopped" %>'
                                    CausesValidation="false">▶ Resume</asp:LinkButton>
                                <%# Eval("Status").ToString() == "Completed"
                                    ? "<span class='badge-completed'>Completed</span>" : "" %>
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
                            <th>Revised ✓</th>
                            <th>Progress / Status</th>
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
                                <div style="display:flex;gap:4px;align-items:center;justify-content:center;">
                                    <asp:TextBox ID="txtRevised" runat="server"
                                        CssClass="revised-input"
                                        Text='<%# Eval("RevisedBatches") %>'
                                        Enabled='<%# Eval("Status").ToString() != "Completed" %>'
                                        placeholder="—"/>
                                    <asp:LinkButton runat="server"
                                        CommandName="SaveRevised"
                                        CommandArgument='<%# Eval("OrderID") %>'
                                        CssClass="btn-save-rev"
                                        Visible='<%# Eval("Status").ToString() != "Completed" %>'
                                        CausesValidation="false">✓</asp:LinkButton>
                                </div>
                            </td>
                            <td>
                                <div class="progress-text">
                                    <%# FormatProgress(Eval("CompletedBatches"), Eval("EffectiveBatches"), Eval("OrderedBatches")) %>
                                </div>
                                <div class="prog-bar-wrap">
                                    <div class="prog-bar-fill" style="width:<%# ProgressBarWidth(Eval("CompletedBatches"), Eval("EffectiveBatches")) %>%"></div>
                                </div>
                                <span class='<%# StatusClass(Eval("Status")) %>'><%# Eval("Status") %></span>
                            </td>
                            <td>
                                <asp:LinkButton runat="server"
                                    CommandName="Initiate"
                                    CommandArgument='<%# Eval("OrderID") %>'
                                    CssClass='<%# GetInitiateCssClass(Eval("Status"), Eval("OrderID")) %>'
                                    Visible='<%# CanInitiate(Eval("Status")) %>'
                                    Enabled='<%# CanInitiateWithStock(Eval("Status"), Eval("OrderID")) %>'
                                    ToolTip='<%# StockTooltip(Eval("Status"), Eval("OrderID")) %>'
                                    OnClientClick="return confirmInitiate(this);"
                                    CausesValidation="false"><%# GetInitiateLabel(Eval("Status"), Eval("OrderID")) %></asp:LinkButton>
                                <asp:LinkButton runat="server"
                                    CommandName="Stop"
                                    CommandArgument='<%# Eval("OrderID") %>'
                                    CssClass="btn-stop"
                                    Visible='<%# Eval("Status").ToString() == "InProgress" || Eval("Status").ToString() == "Initiated" %>'
                                    OnClientClick="return confirmStop(this);"
                                    CausesValidation="false">⏸ Stop</asp:LinkButton>
                                <asp:LinkButton runat="server"
                                    CommandName="Resume"
                                    CommandArgument='<%# Eval("OrderID") %>'
                                    CssClass="btn-resume"
                                    Visible='<%# Eval("Status").ToString() == "Stopped" %>'
                                    CausesValidation="false">▶ Resume</asp:LinkButton>
                                <%# Eval("Status").ToString() == "Completed"
                                    ? "<span class='badge-completed'>Completed</span>" : "" %>
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
                            <div style="text-align:right;">
                                <span class='progress-shift-badge <%# Convert.ToInt32(Eval("Shift"))==1 ? "shift1-badge" : "shift2-badge" %>'>
                                    SHIFT <%# Eval("Shift") %>
                                </span><br/>
                                <span class='<%# StatusClass(Eval("Status")) %>' style="margin-top:4px;display:inline-block;">
                                    <%# Eval("Status") %>
                                </span>
                            </div>
                        </div>

                        <!-- Batch Progress -->
                        <div class="prog-label">
                            <span style="font-weight:600;">
                                Batch <%# Convert.ToInt32(Eval("CompletedBatches")) %> 
                                of <%# Convert.ToInt32(Convert.ToDecimal(Eval("EffectiveBatches"))) %>
                                <%# Convert.ToDecimal(Eval("EffectiveBatches")) != Convert.ToDecimal(Eval("OrderedBatches"))
                                    ? "(" + Convert.ToInt32(Convert.ToDecimal(Eval("OrderedBatches"))) + " ordered)" : "" %>
                            </span>
                            <span style="font-size:11px;color:var(--text-muted);">
                                <%# ProgressBarWidth(Eval("CompletedBatches"), Eval("EffectiveBatches")) %>% done
                            </span>
                        </div>
                        <div class="prog-bar-wrap">
                            <div class="prog-bar-fill" style="width:<%# ProgressBarWidth(Eval("CompletedBatches"), Eval("EffectiveBatches")) %>%"></div>
                        </div>

                        <!-- Stats -->
                        <div class="stat-row">
                            <span class="stat-label">Expected Output</span>
                            <span class="stat-value">
                                <%# FormatOutput(Eval("EffectiveBatches"), Eval("BatchSize"), Eval("OutputAbbr")) %>
                            </span>
                        </div>
                        <div class="stat-row">
                            <span class="stat-label">Actual Output</span>
                            <span class="stat-value">
                                <%# Convert.ToDecimal(Eval("ActualOutput")) > 0
                                    ? Convert.ToDecimal(Eval("ActualOutput")).ToString("0.###") + " " + Eval("OutputAbbr")
                                    : "—" %>
                            </span>
                        </div>
                        <div class="stat-row">
                            <span class="stat-label">Variation</span>
                            <span class="stat-value variation-row">
                                <%# FormatVariation(Eval("ActualOutput"), Eval("BatchSize"), Eval("CompletedBatches"), Eval("OutputAbbr")) %>
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

<!-- INITIATE CONFIRMATION MODAL -->
<div class="modal-overlay" id="initiateModal">
    <div class="modal-box">
        <div class="modal-icon">&#x2699;&#xFE0F;</div>
        <div class="modal-title">Initiate Production</div>
        <div class="modal-product" id="modalProduct"></div>
        <div class="modal-detail" id="modalDetail"></div>
        <div class="modal-actions">
            <button type="button" class="modal-btn-cancel" onclick="closeInitiateModal()">Cancel</button>
            <button type="button" class="modal-btn-confirm" onclick="doInitiate()">Initiate Production</button>
        </div>
    </div>
</div>

<!-- STOP CONFIRMATION MODAL -->
<div class="modal-overlay" id="stopModal">
    <div class="modal-box modal-stop">
        <div class="modal-icon">⏸️</div>
        <div class="modal-title">Stop Production</div>
        <div class="modal-product" id="stopModalProduct"></div>
        <div class="modal-detail">This will pause the production order. You can resume it later from this screen.</div>
        <div class="modal-actions">
            <button type="button" class="modal-btn-cancel" onclick="closeStopModal()">Cancel</button>
            <button type="button" class="modal-btn-stop" onclick="doStop()">Stop Production</button>
        </div>
    </div>
</div>

<script>
var _pendingHref = null;

function confirmInitiate(btn) {
    var row     = btn.closest('tr');
    var nameEl  = row ? row.querySelector('.prod-name') : null;
    var codeEl  = row ? row.querySelector('.prod-code') : null;
    var revEl   = row ? row.querySelector('.revised-input') : null;
    var batchEl = row ? row.querySelector('.batch-num') : null;

    var prod    = nameEl ? nameEl.innerText.trim() : 'this product';
    var code    = codeEl ? codeEl.innerText.trim() : '';
    var batches = (revEl && revEl.value.trim()) ? revEl.value.trim()
                : (batchEl ? batchEl.innerText.trim() : '?');

    document.getElementById('modalProduct').innerText =
        prod + (code ? '  —  ' + code : '');
    document.getElementById('modalDetail').innerHTML =
        'You are about to initiate production of <strong>' + batches +
        ' batches</strong>.<br/>Once confirmed, you will be redirected to Production Execution.';

    // ASP.NET LinkButton renders as <a href="javascript:__doPostBack(...)">
    // Store the href so we can execute it after modal confirmation
    _pendingHref = btn.href;

    document.getElementById('initiateModal').classList.add('visible');
    return false;
}

function closeInitiateModal() {
    document.getElementById('initiateModal').classList.remove('visible');
    _pendingHref = null;
}

function doInitiate() {
    var href = _pendingHref;
    closeInitiateModal();
    if (href) {
        // Strip "javascript:" prefix and eval the postback call
        var js = href.replace(/^javascript:/i, '');
        eval(js);
    }
}

document.addEventListener('DOMContentLoaded', function() {
    document.getElementById('initiateModal').addEventListener('click', function(e) {
        if (e.target === this) closeInitiateModal();
    });
    document.getElementById('stopModal').addEventListener('click', function(e) {
        if (e.target === this) closeStopModal();
    });
});

var _pendingStopHref = null;
function confirmStop(btn) {
    var row    = btn.closest('tr');
    var nameEl = row ? row.querySelector('.prod-name') : null;
    document.getElementById('stopModalProduct').innerText = nameEl ? nameEl.innerText.trim() : '';
    _pendingStopHref = btn.href;
    document.getElementById('stopModal').classList.add('visible');
    return false;
}
function closeStopModal() {
    document.getElementById('stopModal').classList.remove('visible');
    _pendingStopHref = null;
}
function doStop() {
    var href = _pendingStopHref;
    closeStopModal();
    if (href) { var js = href.replace(/^javascript:/i, ''); eval(js); }
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
<script src="/StockApp/erp-modal.js"></script>
<script src="/StockApp/erp-keepalive.js"></script>
</body>
</html>
