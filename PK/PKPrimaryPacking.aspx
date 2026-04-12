<%@ Page Language="C#" AutoEventWireup="true" Inherits="PKApp.PKPrimaryPacking" EnableEventValidation="false" %>
<!DOCTYPE html><html lang="en"><head runat="server">
<meta charset="utf-8"/><meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Primary Packing — Sirimiri</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{
    --bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;
    --accent:#27ae60;--accent-dark:#219a52;--accent-light:#eafaf1;
    --red:#e74c3c;--orange:#e67e22;--amber:#f39c12;
    --text:#1a1a1a;--text-muted:#666;--text-dim:#999;
    --radius:14px;--nav-h:52px;
}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);color:var(--text);min-height:100vh;}
nav{background:#1a1a1a;height:var(--nav-h);display:flex;align-items:center;padding:0 20px;gap:12px;position:sticky;top:0;z-index:100;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;}
.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.08em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:12px;}
.nav-user{color:rgba(255,255,255,.8);font-size:12px;}
.nav-link{color:#aaa;text-decoration:none;font-size:11px;font-weight:700;text-transform:uppercase;padding:5px 10px;border-radius:5px;}
.nav-link:hover{color:#fff;background:rgba(255,255,255,.08);}

/* SELECT BAR */
.date-bar{background:var(--surface);border-bottom:2px solid #1a1a1a;padding:10px 24px;display:flex;align-items:center;justify-content:space-between;gap:16px;}
.date-bar-left{display:flex;flex-direction:column;gap:2px;}
.page-title-bar{font-family:"Bebas Neue",sans-serif;font-size:22px;letter-spacing:.08em;color:var(--text);}
.page-title-bar span{color:var(--orange);}
.date-str{font-size:11px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-muted);}
.date-bar-right{display:flex;align-items:center;gap:8px;}
.shift-label{font-size:11px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;color:var(--text-muted);}
.shift-sel{padding:6px 10px;border:1.5px solid var(--border);border-radius:7px;font-family:inherit;font-size:12px;font-weight:700;background:#fafafa;outline:none;color:var(--text);}
.shift-sel:focus{border-color:var(--orange);}
.select-bar{background:var(--surface);border-bottom:1px solid var(--border);padding:14px 24px;display:flex;align-items:flex-end;justify-content:center;gap:14px;flex-wrap:wrap;}
.select-bar label{font-size:11px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;color:var(--text-muted);display:block;margin-bottom:5px;}
.select-bar select{padding:9px 13px;border:1.5px solid var(--border);border-radius:9px;font-family:inherit;font-size:13px;color:var(--text);background:#fafafa;outline:none;min-width:300px;}
.select-bar select:focus{border-color:var(--accent);}
.btn-load{background:#1a1a1a;color:#fff;border:none;border-radius:9px;padding:10px 24px;font-size:12px;font-weight:700;cursor:pointer;letter-spacing:.04em;}
.btn-load:hover{background:#333;}

/* ALERT */
.alert-wrap{padding:0 20px;margin-top:14px;}
.alert{padding:12px 18px;border-radius:10px;font-size:13px;font-weight:600;}
.alert-success{background:var(--accent-light);color:var(--accent-dark);border:1px solid #a9dfbf;}
.alert-danger{background:#fdf3f2;color:var(--red);border:1px solid #f5c6cb;}

/* PAGE */
.page-body{max-width:860px;margin:0 auto;padding:20px 20px 60px;}

/* INFO PANEL */
.info-panel{background:var(--surface);border-radius:var(--radius);box-shadow:0 2px 12px rgba(0,0,0,.07);padding:18px 24px;margin-bottom:20px;display:flex;gap:32px;flex-wrap:wrap;align-items:center;}
.info-item{display:flex;flex-direction:column;gap:3px;}
.info-label{font-size:10px;font-weight:700;letter-spacing:.1em;text-transform:uppercase;color:var(--text-dim);}
.info-val{font-family:'Bebas Neue',sans-serif;font-size:22px;letter-spacing:.04em;}
.info-val.green{color:var(--accent);}
.info-val.orange{color:var(--orange);}

/* ── RUN CONFIG PANEL ─────────────────────────────────────────────────── */
.run-config{background:#fff8e1;border:1px solid #ffe082;border-radius:var(--radius);padding:20px 24px;margin-bottom:20px;}
.run-config-title{font-family:'Bebas Neue',sans-serif;font-size:15px;letter-spacing:.07em;color:#f57f17;margin-bottom:14px;}
.run-config-grid{display:grid;grid-template-columns:1fr 1fr;gap:14px;}
.form-group{display:flex;flex-direction:column;gap:5px;}
label{font-size:11px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;color:var(--text-muted);}
.req{color:var(--red);}
select.cfg-sel,input.cfg-inp{width:100%;padding:10px 13px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:14px;background:#fff;outline:none;}
select.cfg-sel:focus,input.cfg-inp:focus{border-color:var(--amber);}

/* EXEC PANEL */
.exec-panel{background:var(--surface);border-radius:var(--radius);box-shadow:0 2px 12px rgba(0,0,0,.07);padding:28px 32px;}
.exec-panel-inner{display:flex;align-items:center;justify-content:center;gap:24px;}
.gear-wrap{width:200px;height:200px;flex-shrink:0;}
.gear-wrap img{width:100%;height:100%;object-fit:contain;}
.gear-buttons-row{display:flex;flex-direction:column;gap:16px;align-items:center;}
.batch-side{display:flex;flex-direction:column;align-items:center;justify-content:center;min-width:100px;}
.btn-start{background:var(--accent);color:#fff;border:none;border-radius:50%;width:90px;height:90px;font-size:11px;font-weight:700;cursor:pointer;letter-spacing:.06em;text-transform:uppercase;line-height:1.3;box-shadow:0 4px 16px rgba(39,174,96,.4);transition:all .2s;}
.btn-start:hover{transform:scale(1.05);}
.btn-start:disabled{background:#ccc;box-shadow:none;cursor:not-allowed;transform:none;}
.btn-end{background:var(--red);color:#fff;border:none;border-radius:50%;width:90px;height:90px;font-size:11px;font-weight:700;cursor:pointer;letter-spacing:.06em;text-transform:uppercase;line-height:1.3;box-shadow:0 4px 16px rgba(231,76,60,.4);transition:all .2s;}
.btn-end:hover{transform:scale(1.05);}
.btn-end:disabled{background:#ccc;box-shadow:none;cursor:not-allowed;transform:none;}
.batch-info-row{display:none;}/* replaced by side layout */
.batch-num{font-family:'Bebas Neue',sans-serif;font-size:108px;letter-spacing:.04em;line-height:1;color:var(--text);}
.batch-of{font-family:'Bebas Neue',sans-serif;font-size:108px;letter-spacing:.04em;line-height:1;color:var(--text-muted);}
.status-label{font-size:12px;font-weight:700;letter-spacing:.1em;text-transform:uppercase;padding:6px 18px;border-radius:20px;display:inline-block;margin-bottom:8px;}
.status-label.running{background:var(--accent-light);color:var(--accent-dark);}
.status-label.stopped{background:#f0f0f0;color:var(--text-muted);}

/* ── OUTPUT PANEL ─────────────────────────────────────────────────────── */
.output-panel{background:#f0faf4;border:1.5px solid #a9dfbf;border-radius:12px;padding:24px;margin-top:20px;text-align:left;}
.output-title{font-family:'Bebas Neue',sans-serif;font-size:16px;letter-spacing:.07em;color:var(--accent-dark);margin-bottom:6px;}
.output-sub{font-size:12px;color:var(--text-muted);margin-bottom:18px;}
.output-grid{display:grid;grid-template-columns:1fr 1fr 1fr;gap:14px;}
label.out-lbl{font-size:11px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;color:var(--text-muted);}
input.out-inp{width:100%;padding:11px 12px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:16px;font-weight:600;background:#fff;outline:none;text-align:center;}
input.out-inp:focus{border-color:var(--accent);}
.total-bar{background:#1a1a1a;border-radius:10px;padding:16px 20px;display:flex;justify-content:space-between;align-items:center;margin-top:16px;}
.total-bar-left .total-bar-label{color:rgba(255,255,255,.55);font-size:11px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;}
.total-bar-left .total-bar-formula{color:rgba(255,255,255,.3);font-size:11px;margin-top:4px;}
.total-bar-val{font-family:'Bebas Neue',sans-serif;font-size:40px;color:#2ecc71;letter-spacing:.04em;line-height:1;}
.total-bar-unit{color:rgba(255,255,255,.4);font-size:11px;font-weight:600;margin-left:4px;}
.btn-save{background:var(--accent);color:#fff;border:none;border-radius:9px;padding:13px;font-size:14px;font-weight:700;cursor:pointer;letter-spacing:.05em;margin-top:16px;width:100%;}
.btn-save:hover{background:var(--accent-dark);}

/* HISTORY */
.history-panel{background:var(--surface);border-radius:var(--radius);box-shadow:0 2px 12px rgba(0,0,0,.07);padding:20px 24px;margin-top:20px;}
.history-title{font-family:'Bebas Neue',sans-serif;font-size:15px;letter-spacing:.07em;color:var(--text-muted);margin-bottom:14px;padding-bottom:10px;border-bottom:1px solid var(--border);}
.hist-table{width:100%;border-collapse:collapse;font-size:13px;}
.hist-table th{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-muted);padding:8px 12px;border-bottom:2px solid var(--border);text-align:left;}
.hist-table th.num{text-align:right;}
.hist-table td{padding:10px 12px;border-bottom:1px solid var(--border);}
.hist-table td.num{text-align:right;font-variant-numeric:tabular-nums;}
.hist-table tr:last-child td{border-bottom:none;}
.badge-done{background:var(--accent-light);color:var(--accent-dark);font-size:10px;font-weight:700;padding:2px 8px;border-radius:10px;}
.badge-prog{background:#fff3e0;color:var(--orange);font-size:10px;font-weight:700;padding:2px 8px;border-radius:10px;}
.empty-note{text-align:center;padding:24px;color:var(--text-dim);font-size:13px;}

@media(max-width:600px){
    .batch-num{font-size:72px;}
    .batch-of{font-size:28px;}
    .output-grid{grid-template-columns:1fr 1fr;}
    .run-config-grid{grid-template-columns:1fr;}
    .gear-wrap{width:160px;height:160px;}
    .exec-panel-inner{gap:12px;}
}
</style>
</head>
<body>
<form id="form1" runat="server">

<asp:HiddenField ID="hfOrderId"          runat="server" Value="0"/>
<asp:HiddenField ID="hfProductId"        runat="server" Value="0"/>
<asp:HiddenField ID="hfPackingId"        runat="server" Value="0"/>
<asp:HiddenField ID="hfState"            runat="server" Value="ready"/>
<asp:HiddenField ID="hfBatchNo"          runat="server" Value="0"/>
<asp:HiddenField ID="hfTotalBat"         runat="server" Value="0"/>
<asp:HiddenField ID="hfContainerType"    runat="server" Value="DIRECT"/>
<asp:HiddenField ID="hfUnitSizes"        runat="server" Value=""/>
<asp:HiddenField ID="hfContainersPerCase" runat="server" Value="12"/>
<asp:HiddenField ID="hfSelectedUnitSize" runat="server" Value="0"/>
<asp:HiddenField ID="hfSelectedCaseQty"  runat="server" Value="0"/>
<asp:HiddenField ID="hfHasLanguageLabels" runat="server" Value="0"/>
<asp:HiddenField ID="hfLangSplit" runat="server" Value=""/>

<asp:HiddenField ID="hfMachineId" runat="server" Value="0"/>

<nav>
    <a class="nav-logo" href="PKHome.aspx"><img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="" onerror="this.style.display='none'"/></a>
    <span class="nav-title">Primary Packing</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblUser" runat="server"/></span>
        <a href="/StockApp/ERPHome.aspx" class="nav-link">&#x2302; ERP Home</a>
        <a href="PKHome.aspx" class="nav-link">&#8592; Home</a>
        <a href="PKLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>

<!-- MACHINE SELECT — inline in product select bar -->
<asp:Panel ID="pnlMachineSelect" runat="server" Visible="false"/>

<asp:Panel ID="pnlMainContent" runat="server" Visible="true">

<!-- DATE BAR -->
<div class="date-bar">
    <div class="date-bar-left">
        <div class="page-title-bar">Primary <span>Packing</span></div>
        <div class="date-str"><asp:Label ID="lblDate" runat="server"/></div>
    </div>
    <div class="date-bar-right">
        <span class="shift-label">Shift</span>
        <asp:DropDownList ID="ddlShift" runat="server" CssClass="shift-sel">
            <asp:ListItem Value="1">Shift 1 — Morning</asp:ListItem>
            <asp:ListItem Value="2">Shift 2 — Evening</asp:ListItem>
        </asp:DropDownList>
    </div>
</div>

<!-- MACHINE + PRODUCT SELECT BAR -->
<div class="select-bar" style="flex-wrap:wrap;gap:14px;">
    <div style="display:flex;align-items:flex-end;gap:10px;">
        <div>
            <label style="font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:#999;">Machine</label>
            <!-- EDIT mode: dropdown + SET button -->
            <asp:Panel ID="pnlMachineEdit" runat="server">
                <div style="display:flex;gap:6px;align-items:center;">
                    <asp:DropDownList ID="ddlMachine" runat="server"
                        style="padding:9px 14px;border:2px solid #e67e22;border-radius:8px;font-size:14px;font-family:inherit;min-width:200px;background:#fff8f0;"/>
                    <asp:Button ID="btnSetMachine" runat="server" Text="SET"
                        style="padding:9px 18px;background:#e67e22;color:#fff;border:none;border-radius:8px;font-size:13px;font-weight:700;cursor:pointer;font-family:inherit;letter-spacing:.04em;"
                        OnClick="btnSetMachine_Click" CausesValidation="false"/>
                </div>
            </asp:Panel>
            <!-- DISPLAY mode: bold machine name + EDIT button -->
            <asp:Panel ID="pnlMachineDisplay" runat="server" Visible="false">
                <div style="display:flex;gap:10px;align-items:center;">
                    <asp:Label ID="lblMachineName" runat="server"
                        style="font-family:'Bebas Neue',sans-serif;font-size:26px;letter-spacing:.06em;color:#e67e22;line-height:1;"/>
                    <asp:LinkButton ID="lnkChangeMachine" runat="server" Text="EDIT" OnClick="lnkChangeMachine_Click" CausesValidation="false"
                        style="padding:5px 14px;border:1.5px solid #e67e22;border-radius:6px;font-size:11px;font-weight:700;color:#e67e22;text-decoration:none;cursor:pointer;letter-spacing:.04em;"/>
                </div>
            </asp:Panel>
        </div>
    </div>
    <div style="display:flex;align-items:flex-end;gap:10px;flex:1;min-width:200px;">
        <div style="flex:1;">
            <label>Product</label>
            <asp:DropDownList ID="ddlProduct" runat="server"/>
        </div>
        <asp:Button ID="btnLoad" runat="server" Text="Load" CssClass="btn-load"
            OnClick="btnLoad_Click" CausesValidation="false"/>
    </div>
</div>

<div class="alert-wrap">
    <asp:Panel ID="pnlAlert" runat="server" Visible="false">
        <div class="alert"><asp:Label ID="lblAlert" runat="server"/></div>
    </asp:Panel>
</div>

<div class="page-body">

    <!-- ORDER SELECTION -->
    <asp:Panel ID="pnlOrderSelect" runat="server" Visible="false">
    <div class="order-list">
        <div class="order-list-title">&#x1F4CB; Select Order to Pack</div>
        <asp:Repeater ID="rptOrders" runat="server" OnItemCommand="rptOrders_ItemCommand">
            <ItemTemplate>
                <div class="order-item">
                    <div class="order-item-left">
                        <div class="order-item-id">Order #<%# Eval("OrderID") %></div>
                        <div class="order-item-date"><%# Convert.ToDateTime(Eval("OrderDate")).ToString("dd MMM yyyy") %> &nbsp;|&nbsp; Shift <%# Eval("Shift") %></div>
                    </div>
                    <div class="order-item-right">
                        <div class="order-item-stat">
                            <div class="order-item-stat-val"><%# Eval("ProductionDone") %>/<%# Eval("TotalBatches") %></div>
                            <div class="order-item-stat-lbl">Produced</div>
                        </div>
                        <div class="order-item-stat">
                            <div class="order-item-stat-val"><%# Eval("PackedBatches") %></div>
                            <div class="order-item-stat-lbl">Packed</div>
                        </div>
                        <asp:Button runat="server" Text="Select &#x2192;" CssClass="btn-select-order"
                            CommandName="SelectOrder"
                            CommandArgument='<%# Eval("OrderID") %>'
                            CausesValidation="false"/>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>
    </asp:Panel>

    <!-- INFO -->
    <asp:Panel ID="pnlInfo" runat="server" Visible="false">
    <div class="info-panel">
        <div class="info-item">
            <span class="info-label">Product</span>
            <span class="info-val"><asp:Label ID="lblProduct" runat="server"/></span>
        </div>
        <div class="info-item">
            <span class="info-label">Container</span>
            <span class="info-val"><asp:Label ID="lblContainerType" runat="server"/></span>
        </div>
        <div class="info-item">
            <span class="info-label">Total Batches</span>
            <span class="info-val"><asp:Label ID="lblTotalBatches" runat="server"/></span>
        </div>
        <div class="info-item">
            <span class="info-label">Packed</span>
            <span class="info-val green"><asp:Label ID="lblPackedBatches" runat="server"/></span>
        </div>
        <div class="info-item">
            <span class="info-label">Remaining</span>
            <span class="info-val orange"><asp:Label ID="lblRemaining" runat="server"/></span>
        </div>
    </div>
    </asp:Panel>

    <!-- RUN CONFIG — shown before packing starts -->
    <asp:Panel ID="pnlRunConfig" runat="server" Visible="false">
    <div class="run-config">
        <div class="run-config-title">&#9881; Configure This Packing Run</div>
        <div class="run-config-grid">
            <div class="form-group">
                <label>Units per <asp:Label ID="lblContainerName" runat="server">Container</asp:Label> <span class="req">*</span></label>
                <asp:DropDownList ID="ddlUnitSize" runat="server" CssClass="cfg-sel"
                    onchange="onUnitSizeChange(this.value);"/>
            </div>
            <div class="form-group" id="rowCaseQty" runat="server">
                <label><asp:Label ID="lblCaseQtyLbl" runat="server">Containers</asp:Label> per Case <span class="req">*</span></label>
                <asp:DropDownList ID="ddlCaseQty" runat="server" CssClass="cfg-sel"
                    onchange="onCaseQtyChange(this.value);"/>
            </div>
        </div>
        <div id="rowLabelLanguage" runat="server" style="margin-top:14px;display:none;">
            <div class="form-group" style="max-width:320px;">
                <label>Label Language <span class="req">*</span></label>
                <asp:DropDownList ID="ddlLabelLanguage" runat="server" CssClass="cfg-sel">
                    <asp:ListItem Value="Tamil">Tamil</asp:ListItem>
                    <asp:ListItem Value="Kannada">Kannada</asp:ListItem>
                    <asp:ListItem Value="Telugu">Telugu</asp:ListItem>
                </asp:DropDownList>
                <span style="font-size:11px;color:var(--text-dim);margin-top:4px;">Can be changed between batches</span>
            </div>
        </div>
    </div>
    </asp:Panel>

    <!-- EXECUTION PANEL -->
    <asp:Panel ID="pnlExecution" runat="server" Visible="false">
    <div class="exec-panel">
        <div class="exec-panel-inner">

            <!-- LEFT: Batch Number -->
            <div class="batch-side">
                <span class="batch-num" id="batchNum">—</span>
            </div>

            <!-- CENTRE: Wheel + Buttons -->
            <div style="display:flex;flex-direction:column;align-items:center;gap:16px;">
                <div class="gear-wrap">
                    <img id="gearImg" src="/PP/progress_wheel.png" alt=""
                        onerror="this.style.opacity='.1'"/>
                </div>
                <div class="gear-buttons-row">
                    <asp:Button ID="btnStart" runat="server" CssClass="btn-start"
                        OnClick="btnStart_Click" CausesValidation="false"
                        UseSubmitBehavior="false" OnClientClick="startWheelAnim();"
                        Text="&#9654;&#xA;START"/>
                    <asp:Button ID="btnEnd" runat="server" CssClass="btn-end"
                        OnClick="btnEnd_Click" CausesValidation="false"
                        UseSubmitBehavior="false" OnClientClick="stopWheelAnim();"
                        Text="&#9646;&#9646;&#xA;END"/>
                </div>
                <div class="status-label stopped" id="statusLabel">READY TO START</div>
            </div>

            <!-- RIGHT: X of Y -->
            <div class="batch-side">
                <span class="batch-of" id="batchSub">—</span>
            </div>

        </div>
    </div>
    </asp:Panel>

    <!-- OUTPUT PANEL — shown after ALL batches done -->
    <asp:Panel ID="pnlOutput" runat="server" Visible="true" style="display:none;">
    <div class="output-panel">
        <div class="output-title">&#x2713; All Batches Complete — Record Packed Output</div>
        <div class="output-sub">
            <asp:Label ID="lblOutputSummary" runat="server"/>
        </div>
        <div class="output-grid" style="grid-template-columns:1fr 1fr;">
            <div class="form-group" id="rowJarsOut">
                <label class="out-lbl">No. of <asp:Label ID="lblJarOutName" runat="server">Jars</asp:Label></label>
                <input type="number" id="txtJars" runat="server" class="out-inp" min="0" step="1" placeholder="0" value="0"/>
            </div>
            <div class="form-group">
                <label class="out-lbl">Individual Pieces (loose)</label>
                <input type="number" id="txtUnits" runat="server" class="out-inp" min="0" step="1" placeholder="0" value="0"/>
            </div>
        </div>
        <div class="total-bar">
            <div class="total-bar-left">
                <div class="total-bar-label">Total Individual Pieces</div>
                <div class="total-bar-formula" id="totalFormula">—</div>
            </div>
            <div>
                <span class="total-bar-val" id="totalVal">0</span>
                <span class="total-bar-unit">pcs</span>
            </div>
        </div>

        <!-- PM CONSUMPTION GRID -->
        <asp:Panel ID="pnlPMConsumption" runat="server" Visible="false">
        <div style="margin-top:20px;border-top:1px solid #a9dfbf;padding-top:16px;">
            <div style="font-family:'Bebas Neue',sans-serif;font-size:14px;letter-spacing:.07em;color:var(--accent-dark);margin-bottom:10px;">Packing Material Consumption</div>
            <div style="font-size:11px;color:var(--text-muted);margin-bottom:12px;">Auto-calculated from output. Edit actual qty if needed before saving.</div>
            <table class="hist-table" style="font-size:13px;">
                <thead><tr>
                    <th>Packing Material</th>
                    <th>Level</th>
                    <th>Language</th>
                    <th style="text-align:right;">Calculated</th>
                    <th style="text-align:right;width:120px;">Actual Qty</th>
                    <th>Unit</th>
                </tr></thead>
                <tbody id="pmGridBody">
                    <asp:Repeater ID="rptPMConsumption" runat="server">
                        <ItemTemplate>
                            <tr class="pm-calc-row"
                                data-pmid="<%# Eval("PMID") %>"
                                data-qtyper="<%# Eval("QtyPerUnit") %>"
                                data-level="<%# Eval("ApplyLevel") %>"
                                data-lang="<%# Eval("Language") == DBNull.Value ? "" : Eval("Language") %>">
                                <td><strong><%# Eval("PMName") %></strong><div style="font-size:10px;color:var(--text-dim);"><%# Eval("PMCode") %></div></td>
                                <td><span class='level-badge level-<%# Eval("ApplyLevel") %>'><%# Eval("ApplyLevel") %></span></td>
                                <td style="font-size:12px;"><%# Eval("Language") == DBNull.Value ? "All" : Eval("Language").ToString() %></td>
                                <td class="num pm-calc-val" style="color:var(--text-muted);font-weight:600;">0</td>
                                <td style="text-align:right;">
                                    <input type="number" name="pmQty_<%# Eval("PMID") %>" class="pm-actual-qty" value="0"
                                        min="0" step="0.0001" style="width:100%;padding:6px 8px;border:1.5px solid var(--border);border-radius:6px;font-size:13px;text-align:right;font-weight:600;"/>
                                </td>
                                <td style="font-size:12px;color:var(--text-muted);"><%# Eval("Abbreviation") %></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </div>
        </asp:Panel>

        <asp:Button ID="btnSave" runat="server" Text="&#x2713; Save Packing Output &amp; PM Consumption"
            CssClass="btn-save" OnClick="btnSave_Click" CausesValidation="false"/>
    </div>
    </asp:Panel>

    <!-- HISTORY -->
    <asp:Panel ID="pnlHistory" runat="server" Visible="false">
    <div class="history-panel">
        <div class="history-title">Batch History</div>
        <asp:Panel ID="pnlHistEmpty" runat="server"><div class="empty-note">No batches packed yet</div></asp:Panel>
        <asp:Panel ID="pnlHistTable" runat="server" Visible="false">
        <table class="hist-table">
            <thead><tr>
                <th>Batch</th><th>Start</th><th>End</th><th>Language</th><th>Status</th>
            </tr></thead>
            <tbody>
                <asp:Repeater ID="rptHistory" runat="server">
                    <ItemTemplate><tr>
                        <td><strong>Batch <%# Eval("BatchNo") %></strong></td>
                        <td style="color:var(--text-muted);font-size:11px;"><%# Eval("StartTime") == DBNull.Value ? "—" : Convert.ToDateTime(Eval("StartTime")).ToString("hh:mm tt") %></td>
                        <td style="color:var(--text-muted);font-size:11px;"><%# Eval("EndTime")   == DBNull.Value ? "—" : Convert.ToDateTime(Eval("EndTime")).ToString("hh:mm tt")   %></td>
                        <td style="font-size:12px;font-weight:600;"><%# Eval("LabelLanguage") == DBNull.Value ? "—" : Eval("LabelLanguage") %></td>
                        <td><%# Eval("Status").ToString() == "Completed" ? "<span class='badge-done'>Done</span>" : "<span class='badge-prog'>In Progress</span>" %></td>
                    </tr></ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
        </asp:Panel>
    </div>
    </asp:Panel>

    <!-- BATCH COMPLETION PANEL (shown when all batches packed but completion not done) -->
    <asp:Panel ID="pnlBatchCompletion" runat="server" Visible="false">
    <div style="background:#fff;border-radius:14px;padding:28px 32px;box-shadow:0 2px 12px rgba(0,0,0,.07);margin-top:20px;border:2px solid var(--orange);">
        <div style="font-family:'Bebas Neue',sans-serif;font-size:22px;letter-spacing:.07em;color:var(--orange);margin-bottom:6px;">&#x2705; All Batches Complete — Final Verification</div>
        <div style="font-size:12px;color:#999;margin-bottom:20px;">All production batches have been packed. Please enter the final JAR/BOX count and verify packing material consumption to complete this order.</div>

        <!-- Machine Summary -->
        <asp:Panel ID="pnlMachineSummary" runat="server" Visible="false">
        <div style="margin-bottom:18px;padding:14px;background:#fef5ec;border-radius:10px;border:1px solid #ffd6a0;">
            <div style="font-size:11px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--orange);margin-bottom:8px;">Packing Summary by Machine</div>
            <asp:Literal ID="litMachineSummary" runat="server"/>
        </div>
        </asp:Panel>

        <div style="display:grid;grid-template-columns:1fr 1fr;gap:16px;margin-bottom:16px;">
            <div>
                <label style="font-size:11px;font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:#666;display:block;margin-bottom:4px;">Total <asp:Label ID="lblCompJarName" runat="server" Text="Jars"/>*</label>
                <input type="number" id="txtCompJars" runat="server" min="0" step="1" value="0"
                    style="width:100%;padding:12px 16px;border:2px solid #e0e0e0;border-radius:10px;font-size:18px;font-weight:700;font-family:inherit;"
                    oninput="validateCompletion();"/>
            </div>
            <div>
                <label style="font-size:11px;font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:#666;display:block;margin-bottom:4px;">Loose Pieces</label>
                <input type="number" id="txtCompLoose" runat="server" min="0" step="1" value="0"
                    style="width:100%;padding:12px 16px;border:2px solid #e0e0e0;border-radius:10px;font-size:18px;font-weight:700;font-family:inherit;"
                    oninput="validateCompletion();"/>
            </div>
        </div>

        <!-- PM Consumption for Completion -->
        <asp:Panel ID="pnlCompPM" runat="server" Visible="false">
        <div style="margin-top:16px;border-top:1px solid #ffd6a0;padding-top:16px;">
            <div style="font-family:'Bebas Neue',sans-serif;font-size:14px;letter-spacing:.07em;color:var(--orange);margin-bottom:10px;">Packing Material Consumption</div>
            <div style="font-size:11px;color:#999;margin-bottom:12px;">Auto-calculated from final output. Edit actual qty if needed.</div>
            <table class="hist-table" style="font-size:13px;">
                <thead><tr>
                    <th>Packing Material</th>
                    <th>Level</th>
                    <th>Language</th>
                    <th style="text-align:right;">Calculated</th>
                    <th style="text-align:right;width:120px;">Actual Qty</th>
                    <th>Unit</th>
                </tr></thead>
                <tbody id="compPmGridBody">
                    <asp:Repeater ID="rptCompPM" runat="server">
                        <ItemTemplate>
                            <tr class="comp-pm-row"
                                data-pmid="<%# Eval("PMID") %>"
                                data-qtyper="<%# Eval("QtyPerUnit") %>"
                                data-level="<%# Eval("ApplyLevel") %>"
                                data-lang="<%# Eval("Language") == DBNull.Value ? "" : Eval("Language") %>">
                                <td><strong><%# Eval("PMName") %></strong><div style="font-size:10px;color:var(--text-dim);"><%# Eval("PMCode") %></div></td>
                                <td><span class='level-badge level-<%# Eval("ApplyLevel") %>'><%# Eval("ApplyLevel") %></span></td>
                                <td style="font-size:12px;"><%# Eval("Language") == DBNull.Value ? "All" : Eval("Language").ToString() %></td>
                                <td class="num comp-pm-calc" style="color:var(--text-muted);font-weight:600;">0</td>
                                <td style="text-align:right;">
                                    <input type="number" name="compPmQty_<%# Eval("PMID") %>" class="comp-pm-actual" value="0"
                                        min="0" step="0.0001" style="width:100%;padding:6px 8px;border:1.5px solid var(--border);border-radius:6px;font-size:13px;text-align:right;font-weight:600;"/>
                                </td>
                                <td style="font-size:12px;color:var(--text-muted);"><%# Eval("Abbreviation") %></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </div>
        </asp:Panel>

        <asp:Button ID="btnCompleteBatch" runat="server" Text="&#x2713; Save Packing Output &amp; PM Consumption"
            CssClass="btn-save" OnClick="btnCompleteBatch_Click" CausesValidation="false"
            Enabled="false" style="opacity:0.5;margin-top:16px;"/>
        <div id="compValidationMsg" style="font-size:12px;color:var(--red);margin-top:6px;display:none;">Please enter JAR/BOX count before saving.</div>
    </div>
    </asp:Panel>

</div>

</asp:Panel><!-- /pnlMainContent -->

<script>
// ── Batch Completion Validation ──
function validateCompletion() {
    var jars  = parseInt(document.getElementById('<%= txtCompJars.ClientID %>').value)  || 0;
    var loose = parseInt(document.getElementById('<%= txtCompLoose.ClientID %>').value) || 0;
    var btn   = document.getElementById('<%= btnCompleteBatch.ClientID %>');
    var msg   = document.getElementById('compValidationMsg');
    if (jars > 0 || loose > 0) {
        btn.disabled = false; btn.style.opacity = '1';
        if (msg) msg.style.display = 'none';
    } else {
        btn.disabled = true; btn.style.opacity = '0.5';
        if (msg) msg.style.display = 'block';
    }
}

</script>

<script>
// ── WHEEL ──────────────────────────────────────────────────────────────────
var gearAngle=0, gearSpeed=0, targetSpeed=0;
function animateGear(){
    gearSpeed += (targetSpeed-gearSpeed)*0.03;
    gearAngle  = (gearAngle+gearSpeed)%360;
    var img=document.getElementById('gearImg');
    if(img) img.style.transform='rotate('+gearAngle+'deg)';
    requestAnimationFrame(animateGear);
}
function startWheel(){ window.serverState='running'; }
function stopWheel(r){ window.serverState=r?'ready':'ended'; }

function applyState(){
    var s=window.serverState||'ready';
    var lbl=document.getElementById('statusLabel');
    if(s==='running'){
        targetSpeed=0.9;
        if(lbl){lbl.innerText='PACKING IN PROGRESS...';lbl.className='status-label running';}
    } else {
        targetSpeed=0;
        if(lbl){lbl.innerText='READY TO START';lbl.className='status-label stopped';}
    }
    updateBatchDisplay();
}

function updateBatchDisplay(){
    var n=document.getElementById('batchNum');
    var s=document.getElementById('batchSub');
    if(!n||!s) return;
    if(window.batchNum&&window.batchNum!=='0'){
        n.innerText='B'+window.batchNum;
        s.innerText=window.batchNum+'/'+window.totalBat;
    } else { n.innerText='—'; s.innerText='—'; }
}

function setButtonStates(s){
    var bS=document.getElementById('<%= btnStart.ClientID %>');
    var bE=document.getElementById('<%= btnEnd.ClientID %>');
    if(!bS||!bE) return;
    if(s==='running'){
        bS.disabled=true;  bS.style.background='#ccc'; bS.style.boxShadow='none'; bS.style.cursor='not-allowed'; bS.style.transform='none';
        bE.disabled=false; bE.style.background=''; bE.style.cursor='';
    } else {
        bS.disabled=false; bS.style.background=''; bS.style.cursor='';
        bE.disabled=true;  bE.style.background='#ccc'; bE.style.boxShadow='none'; bE.style.cursor='not-allowed'; bE.style.transform='none';
    }
}

function playClick(){
    try{
        var c=new(window.AudioContext||window.webkitAudioContext)();
        var b=c.createBuffer(1,c.sampleRate*0.08,c.sampleRate);
        var d=b.getChannelData(0);
        for(var i=0;i<d.length;i++) d[i]=(Math.random()*2-1)*Math.pow(1-i/d.length,8);
        var s=c.createBufferSource(); s.buffer=b;
        var f=c.createBiquadFilter(); f.type='bandpass'; f.frequency.value=800; f.Q.value=0.8;
        s.connect(f); f.connect(c.destination); s.start();
    }catch(e){}
}
function startWheelAnim(){ playClick(); targetSpeed=0.9; var l=document.getElementById('statusLabel'); if(l){l.innerText='PACKING IN PROGRESS...';l.className='status-label running';} setButtonStates('running'); return true; }
function stopWheelAnim(){  playClick(); targetSpeed=0;   var l=document.getElementById('statusLabel'); if(l){l.innerText='ENDING BATCH...';l.className='status-label stopped';} return true; }

// ── CONFIG CHANGE HANDLERS ────────────────────────────────────────────────
function onUnitSizeChange(val){
    document.getElementById('<%= hfSelectedUnitSize.ClientID %>').value = val;
    calcTotal();
}
function onCaseQtyChange(val){
    document.getElementById('<%= hfSelectedCaseQty.ClientID %>').value = val;
    calcTotal();
}

// ── TOTAL CALC ────────────────────────────────────────────────────────────
function calcTotal(){
    var ct   = document.getElementById('<%= hfContainerType.ClientID %>').value || 'DIRECT';
    var sz   = parseInt(document.getElementById('<%= hfSelectedUnitSize.ClientID %>').value) || 0;
    var jars = parseInt(document.getElementById('txtJars').value)  || 0;
    var units= parseInt(document.getElementById('txtUnits').value) || 0;
    var total, formula;
    if(ct==='DIRECT'){
        total   = (jars * sz) + units;
        formula = jars+' containers × '+sz+' units + '+units+' loose';
    } else {
        total   = (jars * sz) + units;
        formula = jars+' '+ct.toLowerCase()+'s × '+sz+' units/'+ct.toLowerCase()+' + '+units+' loose';
    }
    document.getElementById('totalVal').innerText    = total.toLocaleString();
    document.getElementById('totalFormula').innerText = formula;
    calcPMConsumption(jars, units, sz, ct);
}

// ── PM CONSUMPTION CALC ──────────────────────────────────────────────────
function calcPMConsumption(jars, loosePcs, unitsPerContainer, containerType){
    var rows = document.querySelectorAll('.pm-calc-row');
    if(!rows.length) return;

    // Parse language split from hidden field: "Tamil:20,Kannada:30" or ""
    var langSplitStr = document.getElementById('<%= hfLangSplit.ClientID %>').value || '';
    var langCounts = {};
    var totalBatches = 0;
    var langOrder = [];
    if(langSplitStr){
        langSplitStr.split(',').forEach(function(pair){
            var parts = pair.split(':');
            if(parts.length===2){
                var lang = parts[0];
                var cnt  = parseInt(parts[1]) || 0;
                langCounts[lang] = cnt;
                totalBatches += cnt;
                langOrder.push(lang);
            }
        });
    }
    if(totalBatches===0) totalBatches=1;

    // Group language-specific PMs by PMID+Level to distribute whole numbers
    // First pass: calculate universal PMs directly
    // Second pass: distribute language PMs with whole-number rounding
    var langGroups = {};

    rows.forEach(function(row){
        var pmid   = row.getAttribute('data-pmid');
        var qtyPer = parseFloat(row.getAttribute('data-qtyper')) || 0;
        var level  = row.getAttribute('data-level') || 'UNIT';
        var lang   = row.getAttribute('data-lang') || '';

        var baseQty = 0;
        if(level==='UNIT'){
            baseQty = (jars * unitsPerContainer + loosePcs) * qtyPer;
        } else if(level==='CONTAINER'){
            baseQty = jars * qtyPer;
        } else if(level==='CASE'){
            baseQty = 0;
        }

        if(!lang){
            // Universal PM — use full quantity, round to 4 decimals
            var rounded = Math.round(baseQty * 10000) / 10000;
            setRowValues(row, rounded);
        } else {
            // Language-specific — collect for distribution
            var groupKey = level + '_' + qtyPer;
            if(!langGroups[groupKey]){
                langGroups[groupKey] = { total: baseQty, level: level, rows: {} };
            }
            langGroups[groupKey].rows[lang] = row;
        }
    });

    // Distribute language PMs: whole numbers for CONTAINER, decimals for UNIT
    for(var key in langGroups){
        var group = langGroups[key];
        var totalQty = group.total;
        var isDiscrete = (group.level === 'CONTAINER');
        var allocated = 0;
        var lastLang = null;

        for(var i = 0; i < langOrder.length; i++){
            var lang = langOrder[i];
            var row = group.rows[lang];
            if(!row) continue;

            var ratio = langCounts[lang] / totalBatches;
            var qty;

            if(isDiscrete){
                // Whole numbers for container-level items (labels, lids)
                if(i < langOrder.length - 1){
                    qty = Math.floor(totalQty * ratio);
                } else {
                    // Last language gets remainder
                    qty = Math.round(totalQty - allocated);
                }
            } else {
                // Decimals OK for unit-level items (kg, litres)
                qty = Math.round(totalQty * ratio * 10000) / 10000;
            }

            allocated += qty;
            lastLang = lang;
            setRowValues(row, qty);
        }

        // If there were languages with no batches, set to 0
        for(var lang in group.rows){
            if(!langCounts[lang]){
                setRowValues(group.rows[lang], 0);
            }
        }
    }
}

function setRowValues(row, qty){
    var calcCell = row.querySelector('.pm-calc-val');
    if(calcCell) calcCell.innerText = qty;
    var actualInput = row.querySelector('.pm-actual-qty');
    if(actualInput && !actualInput.dataset.edited){
        actualInput.value = qty;
    }
}

window.addEventListener('load',function(){
    // Read state from hidden fields
    var stateEl = document.getElementById('<%= hfState.ClientID %>');
    var batchEl = document.getElementById('<%= hfBatchNo.ClientID %>');
    var totalEl = document.getElementById('<%= hfTotalBat.ClientID %>');
    window.serverState = stateEl ? stateEl.value : 'ready';
    window.batchNum    = batchEl ? batchEl.value  : '0';
    window.totalBat    = totalEl ? totalEl.value  : '0';

    animateGear();
    applyState();
    updateBatchDisplay();
    setButtonStates(window.serverState||'ready');

    ['txtJars','txtUnits'].forEach(function(id){
        var el=document.getElementById(id); if(el) el.addEventListener('input',calcTotal);
    });

    // Mark PM actual qty inputs as edited when user manually changes them
    document.querySelectorAll('.pm-actual-qty').forEach(function(inp){
        inp.addEventListener('input', function(){ this.dataset.edited='1'; });
    });

    calcTotal();
});
</script>
</form>
<script src="/StockApp/erp-keepalive.js"></script>
</body></html>
