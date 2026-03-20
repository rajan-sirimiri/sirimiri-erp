<%@ Page Language="C#" AutoEventWireup="true" Inherits="PPApp.PPProductionExecution" EnableEventValidation="false" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri PP — Production Execution</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<style>
:root{
    --accent:#2ecc71; --accent-dark:#27ae60; --accent-light:#eafaf1;
    --blue:#2980b9;   --blue-light:#eaf4fb;
    --red:#e74c3c;    --red-light:#fdf3f2;
    --orange:#e67e22; --orange-light:#fef5ec;
    --gear:#455a64;   --gear-dark:#263238;   --gear-shine:#78909c;
    --text:#1a1a1a;   --text-muted:#666;     --text-dim:#999;
    --bg:#f0f0f0;     --surface:#fff;        --border:#e0e0e0;
    --radius:14px;    --nav-h:52px;
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
.date-bar{background:var(--surface);border-bottom:2px solid #1a1a1a;
    padding:10px 20px;font-family:'Bebas Neue',sans-serif;
    font-size:16px;letter-spacing:.06em;color:var(--text-muted);}

/* ALERT */
.alert-wrap{padding:0 20px;margin-top:10px;}

/* SELECTION BAR */
.select-bar{background:var(--surface);border-bottom:1px solid var(--border);
    padding:14px 20px;display:flex;align-items:center;gap:14px;flex-wrap:wrap;}
.select-bar label{font-size:12px;font-weight:700;letter-spacing:.05em;
    text-transform:uppercase;color:var(--text-muted);}
.select-bar select{border:1.5px solid var(--border);border-radius:8px;
    padding:8px 12px;font-size:13px;font-family:inherit;background:#fff;min-width:200px;}
.select-bar select:focus{outline:none;border-color:var(--accent);}
.btn-load{background:#1a1a1a;color:#fff;border:none;border-radius:8px;
    padding:9px 22px;font-size:13px;font-weight:700;cursor:pointer;letter-spacing:.04em;}
.btn-load:hover{background:#333;}

/* PAGE BODY */
.page-body{max-width:900px;margin:0 auto;padding:24px 20px 60px;}

/* INFO PANEL */
.info-panel{background:var(--surface);border-radius:var(--radius);
    box-shadow:0 2px 12px rgba(0,0,0,.07);padding:20px 24px;
    margin-bottom:40px;display:grid;
    grid-template-columns:1fr auto;gap:16px;align-items:center;}
.info-left{}
.info-product{font-family:'Bebas Neue',sans-serif;font-size:26px;
    letter-spacing:.06em;color:var(--text);}
.info-code{font-size:11px;color:var(--text-dim);margin-bottom:12px;}
.info-stats{display:flex;gap:24px;flex-wrap:wrap;}
.info-stat{font-size:12px;}
.info-stat-val{font-weight:700;font-size:14px;color:var(--text);}
.info-stat-lbl{color:var(--text-muted);}
.info-right{text-align:right;}
.info-batches{font-family:'Bebas Neue',sans-serif;font-size:48px;
    letter-spacing:.04em;line-height:1;color:var(--accent-dark);}
.info-batches-lbl{font-size:11px;color:var(--text-muted);text-transform:uppercase;letter-spacing:.05em;}
.info-completed{font-size:12px;color:var(--text-muted);margin-top:4px;}
.status-initiated{background:var(--orange-light);color:var(--orange);
    font-size:10px;font-weight:700;padding:3px 10px;border-radius:10px;}
.status-inprogress{background:var(--blue-light);color:var(--blue);
    font-size:10px;font-weight:700;padding:3px 10px;border-radius:10px;}
.status-completed{background:var(--accent-light);color:var(--accent-dark);
    font-size:10px;font-weight:700;padding:3px 10px;border-radius:10px;}

/* EXECUTION PANEL */
.exec-panel{background:var(--surface);border-radius:var(--radius);
    box-shadow:0 2px 16px rgba(0,0,0,.08);padding:32px 24px;margin-bottom:32px;}
.exec-title{font-family:'Bebas Neue',sans-serif;font-size:20px;letter-spacing:.06em;
    color:var(--text);margin-bottom:32px;text-align:center;}

/* GEAR AREA */
.gear-area{display:flex;align-items:center;justify-content:center;gap:40px;margin-bottom:40px;}

/* START / END BUTTONS */
.btn-start{background:var(--accent);color:#fff;border:none;border-radius:50%;
    width:80px;height:80px;font-size:13px;font-weight:700;cursor:pointer;
    letter-spacing:.04em;box-shadow:0 4px 16px rgba(46,204,113,.4);
    transition:all .2s;display:flex;align-items:center;justify-content:center;
    flex-direction:column;gap:4px;}
.btn-start:hover{transform:scale(1.05);box-shadow:0 6px 20px rgba(46,204,113,.5);}
.btn-start:disabled{background:#ccc;box-shadow:none;cursor:not-allowed;transform:none;}
.btn-end{background:var(--red);color:#fff;border:none;border-radius:50%;
    width:80px;height:80px;font-size:13px;font-weight:700;cursor:pointer;
    letter-spacing:.04em;box-shadow:0 4px 16px rgba(231,76,60,.4);
    transition:all .2s;display:flex;align-items:center;justify-content:center;
    flex-direction:column;gap:4px;}
.btn-end:hover{transform:scale(1.05);box-shadow:0 6px 20px rgba(231,76,60,.5);}
.btn-end:disabled{background:#ccc;box-shadow:none;cursor:not-allowed;transform:none;}
.btn-icon{font-size:22px;line-height:1;}
.btn-label{font-size:10px;letter-spacing:.06em;}

/* GEAR WHEEL */
.gear-wrap{position:relative;width:364px;height:364px;flex-shrink:0;display:flex;align-items:center;justify-content:center;}
#gearSvg{width:364px;height:364px;object-fit:contain;
    transform-origin:center center;
    transition:filter .3s;}
#gearSvg.spinning{filter:drop-shadow(0 0 16px rgba(180,120,30,.7));}
.batch-info-row{display:flex;align-items:baseline;justify-content:center;
    gap:6px;margin-top:10px;}
.gear-batch-num{font-family:'Bebas Neue',sans-serif;font-size:28px;
    letter-spacing:.04em;color:var(--accent-dark);line-height:1;}
.gear-batch-sep{font-size:18px;color:var(--text-dim);font-weight:300;}
.gear-batch-sub{font-size:13px;color:var(--text-muted);letter-spacing:.06em;
    text-transform:uppercase;font-weight:600;}
.gear-status-label{font-size:11px;font-weight:700;text-align:center;
    margin-top:10px;letter-spacing:.06em;text-transform:uppercase;
    color:var(--text-muted);}
.gear-status-label.running{color:var(--accent-dark);}
.gear-status-label.stopped{color:var(--text-dim);}

/* OUTPUT PANEL */
.output-panel{background:#f8fffe;border:2px solid var(--accent);border-radius:var(--radius);
    padding:20px 24px;margin-top:8px;}
.output-title{font-family:'Bebas Neue',sans-serif;font-size:16px;letter-spacing:.06em;
    color:var(--accent-dark);margin-bottom:16px;}
.output-grid{display:grid;grid-template-columns:1fr 1fr;gap:14px;}
.form-group label{font-size:11px;font-weight:700;letter-spacing:.06em;
    text-transform:uppercase;color:var(--text-muted);display:block;margin-bottom:6px;}
.form-group input, .form-group textarea{width:100%;border:1.5px solid var(--border);
    border-radius:8px;padding:9px 12px;font-size:13px;font-family:inherit;background:#fff;}
.form-group input:focus, .form-group textarea:focus{outline:none;border-color:var(--accent);}
.output-unit{font-size:11px;color:var(--text-muted);margin-top:4px;}
.btn-save-output{background:var(--accent-dark);color:#fff;border:none;border-radius:8px;
    padding:10px 28px;font-size:13px;font-weight:700;cursor:pointer;margin-top:16px;
    letter-spacing:.04em;}
.btn-save-output:hover{background:var(--accent);}

/* BATCH HISTORY */
.history-section{background:var(--surface);border-radius:var(--radius);
    box-shadow:0 2px 12px rgba(0,0,0,.06);overflow:hidden;}
.history-head{padding:14px 20px;border-bottom:2px solid var(--border);
    display:flex;align-items:center;gap:10px;}
.history-title{font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.06em;}
.history-table{width:100%;border-collapse:collapse;font-size:13px;}
.history-table th{font-size:10px;font-weight:700;letter-spacing:.08em;
    text-transform:uppercase;color:var(--text-dim);padding:10px 16px;
    border-bottom:1px solid var(--border);text-align:left;}
.history-table td{padding:10px 16px;border-bottom:1px solid var(--border);}
.history-table tr:last-child td{border-bottom:none;}
.batch-no{font-family:'Bebas Neue',sans-serif;font-size:18px;color:var(--text-dim);}
.badge-done{background:var(--accent-light);color:var(--accent-dark);
    font-size:10px;font-weight:700;padding:2px 8px;border-radius:10px;}
.badge-running{background:var(--orange-light);color:var(--orange);
    font-size:10px;font-weight:700;padding:2px 8px;border-radius:10px;}
.empty-history{text-align:center;padding:32px;color:var(--text-dim);font-size:13px;}
.no-order-msg{text-align:center;padding:40px;color:var(--text-dim);font-size:13px;}
.no-order-icon{font-size:40px;margin-bottom:12px;}
</style>
</head>
<body>
<form id="form1" runat="server">

<asp:HiddenField ID="hfOrderID"      runat="server" Value="0"/>
<asp:HiddenField ID="hfExecutionID"  runat="server" Value="0"/>
<asp:HiddenField ID="hfTotalBatches" runat="server" Value="0"/>
<asp:HiddenField ID="hfCurrentBatch" runat="server" Value="1"/>
<asp:HiddenField ID="hfShowOutput"   runat="server" Value="0"/>
<asp:HiddenField ID="hfState"        runat="server" Value="ready"/>

<nav>
    <a href="PPHome.aspx" class="nav-logo">
        <img src="Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    </a>
    <span class="nav-title">Production Execution</span>
    <div class="nav-right">
        <asp:Label ID="lblNavUser" runat="server" CssClass="nav-user"/>
        <a href="PPHome.aspx" class="nav-link">&#8592; PP Home</a>
        <a href="PPLogout.aspx" class="nav-link" onclick="return confirm('Sign out?')">Sign Out</a>
    </div>
</nav>

<div class="date-bar">
    <asp:Label ID="lblTodayDate" runat="server"/>
    &nbsp;&mdash;&nbsp; TODAY'S EXECUTION
</div>

<!-- ALERT -->
<div class="alert-wrap">
    <asp:Panel ID="pnlAlert" runat="server" Visible="false">
        <asp:Label ID="lblAlert" runat="server"/>
    </asp:Panel>
</div>

<!-- SELECTION BAR -->
<div class="select-bar">
    <label>Shift</label>
    <asp:DropDownList ID="ddlShift" runat="server" AutoPostBack="true"
        OnSelectedIndexChanged="ddlShift_Changed">
        <asp:ListItem Value="1">Shift 1 — Morning</asp:ListItem>
        <asp:ListItem Value="2">Shift 2 — Evening</asp:ListItem>
    </asp:DropDownList>

    <label>Product</label>
    <asp:DropDownList ID="ddlProduct" runat="server">
        <asp:ListItem Value="0">-- Select Product --</asp:ListItem>
    </asp:DropDownList>

    <asp:Button ID="btnLoad" runat="server" Text="Load" CssClass="btn-load"
        OnClick="btnLoad_Click" CausesValidation="false"/>
</div>

<div class="page-body">

    <!-- INFO PANEL -->
    <asp:Panel ID="pnlInfo" runat="server" Visible="false">
        <div class="info-panel">
            <div class="info-left">
                <div class="info-product">
                    <asp:Label ID="lblInfoProduct" runat="server"/>
                </div>
                <div class="info-code">
                    <asp:Label ID="lblInfoCode" runat="server"/>
                    &nbsp;|&nbsp;
                    <asp:Label ID="lblInfoDate" runat="server"/>
                    &nbsp;|&nbsp;
                    <asp:Label ID="lblInfoStatus" runat="server"/>
                </div>
                <div class="info-stats">
                    <div class="info-stat">
                        <div class="info-stat-val"><asp:Label ID="lblInfoOutput" runat="server"/></div>
                        <div class="info-stat-lbl">Expected Output</div>
                    </div>
                </div>
            </div>
            <div class="info-right">
                <div class="info-batches"><asp:Label ID="lblInfoBatches" runat="server"/></div>
                <div class="info-batches-lbl">Total Batches</div>
                <div class="info-completed"><asp:Label ID="lblInfoCompleted" runat="server"/></div>
            </div>
        </div>
    </asp:Panel>

    <!-- NO ORDER STATE -->
    <asp:Panel ID="pnlNoOrder" runat="server" Visible="false">
        <div class="no-order-msg">
            <div class="no-order-icon">&#x23F3;</div>
            <div>Select a Shift and Product above to begin execution.</div>
        </div>
    </asp:Panel>

    <!-- EXECUTION PANEL -->
    <asp:Panel ID="pnlExecution" runat="server" Visible="true" style="display:none">
        <div class="exec-panel">
            <div class="exec-title">Batch Execution</div>

            <!-- GEAR AREA -->
            <div class="gear-area">

                <!-- START BUTTON -->
                <asp:Button ID="btnStart" runat="server" CssClass="btn-start"
                    OnClick="btnStart_Click" CausesValidation="false"
                    UseSubmitBehavior="false"
                    OnClientClick="return startWheelAnim();"
                    Text="&#9654;&#xA;START"/>

                <!-- GEAR WHEEL SVG -->
                <div class="gear-wrap">
                    <!-- Ship wheel image rotates on batch execution -->
                    <img id="gearSvg" src="progress_wheel.png" alt="Production Wheel"/>
                </div>

                <!-- END BUTTON -->
                <asp:Button ID="btnEnd" runat="server" CssClass="btn-end"
                    OnClick="btnEnd_Click" CausesValidation="false"
                    UseSubmitBehavior="false"
                    OnClientClick="return stopWheelAnim();"
                    Text="&#9646;&#9646;&#xA;END"/>

            </div>

            <!-- Batch number display — sits above status label -->
            <div class="batch-info-row">
                <span class="gear-batch-num" id="gearBatchNum">—</span>
                <span class="gear-batch-sep">·</span>
                <span class="gear-batch-sub" id="gearBatchSub">READY</span>
            </div>
            <div class="gear-status-label stopped" id="gearStatusLabel">
                READY TO START
            </div>

            <!-- OUTPUT PANEL — unlocks after END -->
            <asp:Panel ID="pnlOutput" runat="server" Visible="false">
                <div class="output-panel" style="margin-top:28px;">
                    <div class="output-title">Record Batch Output</div>
                    <div class="output-grid">
                        <div class="form-group">
                            <label>Actual Output <span style="color:#e74c3c">*</span></label>
                            <asp:TextBox ID="txtActualOutput" runat="server"
                                placeholder="e.g. 115.5" MaxLength="10"/>
                            <div class="output-unit">
                                Unit: <asp:Label ID="lblOutputUnit" runat="server"/>
                            </div>
                        </div>
                        <div class="form-group">
                            <label>Remarks (if any issue)</label>
                            <asp:TextBox ID="txtRemarks" runat="server"
                                placeholder="Optional — note any issues"
                                MaxLength="300"/>
                        </div>
                    </div>
                    <asp:Button ID="btnSaveOutput" runat="server"
                        Text="Save &amp; Move to Packing"
                        CssClass="btn-save-output"
                        OnClick="btnSaveOutput_Click"
                        CausesValidation="false"/>
                </div>
            </asp:Panel>

        </div>
    </asp:Panel>

    <!-- BATCH HISTORY -->
    <asp:Panel ID="pnlInfo2" runat="server">
        <div class="history-section" style="display:<%# pnlInfo.Visible ? "block" : "none" %>">
            <div class="history-head">
                <span style="font-size:20px;">&#128203;</span>
                <span class="history-title">Batch History</span>
            </div>

            <asp:Panel ID="pnlHistoryEmpty" runat="server" Visible="true">
                <div class="empty-history">No batches started yet for this order.</div>
            </asp:Panel>

            <asp:Repeater ID="rptHistory" runat="server">
                <HeaderTemplate>
                    <table class="history-table">
                    <tr>
                        <th>#</th>
                        <th>Started</th>
                        <th>Completed</th>
                        <th>Duration</th>
                        <th>Actual Output</th>
                        <th>Status</th>
                        <th>Remarks</th>
                    </tr>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><span class="batch-no"><%# Eval("BatchNo") %></span></td>
                        <td><%# FormatTime(Eval("StartTime")) %></td>
                        <td><%# FormatTime(Eval("EndTime")) %></td>
                        <td><%# Eval("EndTime") != DBNull.Value
                            ? Math.Round((Convert.ToDateTime(Eval("EndTime")) - Convert.ToDateTime(Eval("StartTime"))).TotalMinutes, 0) + " min"
                            : "—" %></td>
                        <td><%# FormatOutput(Eval("ActualOutput"), "") %></td>
                        <td>
                            <span class='<%# Eval("Status").ToString()=="Completed" ? "badge-done" : "badge-running" %>'>
                                <%# Eval("Status") %>
                            </span>
                        </td>
                        <td style="color:var(--text-muted);font-size:12px;">
                            <%# Eval("Remarks") == DBNull.Value ? "—" : Eval("Remarks") %>
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></table></FooterTemplate>
            </asp:Repeater>
        </div>
    </asp:Panel>

</div><!-- /page-body -->


<script>
// ── GEAR ANIMATION ──────────────────────────────────────────────────────────
var gearAngle = 0, gearSpeed = 0, targetSpeed = 0;

function animateGear() {
    gearSpeed += (targetSpeed - gearSpeed) * 0.03;
    gearAngle  = (gearAngle + gearSpeed) % 360;
    var img = document.getElementById('gearSvg');
    if (img) img.style.transform = 'rotate(' + gearAngle + 'deg)';
    requestAnimationFrame(animateGear);
}

function updateGearText() {
    var n = document.getElementById('gearBatchNum');
    var s = document.getElementById('gearBatchSub');
    if (!n || !s) return;
    if (window.totalBat && window.totalBat !== '0') {
        n.innerText = 'B' + window.batchNum;
        s.innerText = window.batchNum + ' OF ' + window.totalBat;
    } else {
        n.innerText = '—'; s.innerText = 'READY';
    }
}

// Set by RegisterStartupScript before DOM ready — just store state
function startWheel() { window.serverState = 'running'; }
function stopWheel(r)  { window.serverState = r ? 'ready' : 'ended'; }

function applyState() {
    var img = document.getElementById('gearSvg');
    var lbl = document.getElementById('gearStatusLabel');
    var out = document.getElementById('pnlOutput');
    var s   = window.serverState || 'ready';

    if (s === 'running') {
        targetSpeed = 0.9;
        if (lbl) { lbl.innerText = 'IN PROGRESS...'; lbl.className = 'gear-status-label running'; }
        if (out) out.style.display = 'none';
    } else if (s === 'ended') {
        targetSpeed = 0;
        if (lbl) { lbl.innerText = 'BATCH ENDED — ENTER OUTPUT BELOW'; lbl.className = 'gear-status-label stopped'; }
        if (out) out.style.display = 'block';
    } else if (s === 'stopped') {
        targetSpeed = 0;
        if (lbl) { lbl.innerText = 'PRODUCTION STOPPED'; lbl.className = 'gear-status-label stopped'; }
        if (out) out.style.display = 'none';
    } else {
        targetSpeed = 0;
        if (lbl) { lbl.innerText = 'READY TO START'; lbl.className = 'gear-status-label stopped'; }
        if (out) out.style.display = 'none';
    }
    updateGearText();
}

// OnClientClick — pure visual animation only, NO button state changes
// Server controls which buttons are enabled via Enabled property
function startWheelAnim() {
    targetSpeed = 0.9;
    var lbl = document.getElementById('gearStatusLabel');
    if (lbl) { lbl.innerText = 'IN PROGRESS...'; lbl.className = 'gear-status-label running'; }
    return true;  // allow postback
}
function stopWheelAnim() {
    targetSpeed = 0;
    var out = document.getElementById('pnlOutput');
    if (out) out.style.display = 'block';
    var lbl = document.getElementById('gearStatusLabel');
    if (lbl) { lbl.innerText = 'BATCH ENDED — ENTER OUTPUT BELOW'; lbl.className = 'gear-status-label stopped'; }
    return true;  // allow postback
}

window.addEventListener('load', function() {
    animateGear();   // start loop
    applyState();    // apply server state (RegisterStartupScript already ran)
    updateGearText();
});
</script>
</form>
</body>
</html>
