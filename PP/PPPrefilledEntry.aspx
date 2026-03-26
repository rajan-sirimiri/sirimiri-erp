<%@ Page Language="C#" AutoEventWireup="true" Inherits="PPApp.PPPrefilledEntry" EnableEventValidation="false" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri PP — Prefilled Conversion Entry</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<style>
:root{
    --accent:#2ecc71; --accent-dark:#27ae60; --accent-light:#eafaf1;
    --orange:#e67e22; --orange-light:#fef5ec;
    --red:#e74c3c; --red-light:#fdf3f2;
    --blue:#2980b9; --blue-light:#eaf4fb;
    --text:#1a1a1a; --text-muted:#666; --text-dim:#999;
    --bg:#f0f0f0; --surface:#fff; --border:#e0e0e0;
    --radius:14px; --nav-h:52px;
}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);color:var(--text);min-height:100vh;}
nav{background:#1a1a1a;height:var(--nav-h);display:flex;align-items:center;padding:0 20px;gap:12px;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;}
.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.08em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:16px;}
.nav-user{color:rgba(255,255,255,.8);font-size:12px;}
.nav-link{color:#fff;font-size:12px;font-weight:600;text-decoration:none;opacity:.8;}
.nav-link:hover{opacity:1;}
.date-bar{background:var(--surface);border-bottom:2px solid #1a1a1a;padding:10px 20px;
    font-family:'Bebas Neue',sans-serif;font-size:16px;letter-spacing:.06em;color:var(--text-muted);}
.alert-wrap{padding:0 20px;margin-top:12px;}
.alert{padding:12px 18px;border-radius:10px;font-size:13px;font-weight:600;}
.alert-success{background:var(--accent-light);color:var(--accent-dark);border:1px solid #a9dfbf;}
.alert-danger{background:var(--red-light);color:var(--red);border:1px solid #f5c6cb;}

.page-body{max-width:960px;margin:0 auto;padding:24px 20px 60px;}
.two-col{display:grid;grid-template-columns:1fr 1fr;gap:24px;align-items:start;}
.rm-stock-banner{background:#1a1a1a;border-radius:12px;padding:16px 24px;
    margin-bottom:20px;display:flex;flex-wrap:wrap;gap:16px;align-items:flex-start;}
.rm-stock-label{color:rgba(255,255,255,.5);font-size:10px;font-weight:700;
    letter-spacing:.12em;text-transform:uppercase;}
.rm-stock-item{display:flex;align-items:center;justify-content:space-between;
    background:rgba(255,255,255,.06);border-radius:9px;padding:12px 18px;
    min-width:220px;flex:1;gap:16px;}
.rm-stock-name{color:#fff;font-size:13px;font-weight:700;}
.rm-stock-value{text-align:right;white-space:nowrap;}
.rm-stock-qty{font-family:"Bebas Neue",sans-serif;font-size:32px;
    letter-spacing:.04em;line-height:1;}
.rm-stock-qty.ok{color:#2ecc71;}
.rm-stock-qty.low{color:#f39c12;}
.rm-stock-qty.zero{color:#e74c3c;}
.rm-stock-unit{color:rgba(255,255,255,.5);font-size:12px;font-weight:600;
    margin-left:4px;vertical-align:middle;}

.card{background:var(--surface);border-radius:var(--radius);
    box-shadow:0 2px 16px rgba(0,0,0,.08);padding:28px 26px;margin-bottom:24px;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:20px;letter-spacing:.06em;
    margin-bottom:6px;}
.card-sub{font-size:12px;color:var(--text-muted);margin-bottom:22px;}
.section-badge{display:inline-block;font-size:10px;font-weight:700;letter-spacing:.1em;
    text-transform:uppercase;padding:3px 10px;border-radius:20px;margin-bottom:14px;}
.badge-tally{background:var(--accent-light);color:var(--accent-dark);}
.badge-closure{background:var(--orange-light);color:var(--orange);}

.form-group{margin-bottom:16px;}
.form-group label{display:block;font-size:11px;font-weight:700;letter-spacing:.07em;
    text-transform:uppercase;color:var(--text-muted);margin-bottom:6px;}
.form-group label .req{color:var(--red);margin-left:2px;}
select,input[type=number],input[type=text]{width:100%;padding:10px 13px;
    border:1.5px solid var(--border);border-radius:9px;font-family:inherit;
    font-size:13px;color:var(--text);background:#fafafa;transition:border-color .2s;outline:none;}
select:focus,input:focus{border-color:var(--accent);background:#fff;}
.qty-row{display:flex;gap:10px;align-items:flex-end;}
.qty-row .form-group{flex:1;margin-bottom:0;}
.unit-badge{background:var(--blue-light);color:var(--blue);font-size:12px;font-weight:700;
    padding:10px 14px;border-radius:9px;border:1.5px solid #bee3f8;white-space:nowrap;
    display:flex;align-items:center;}

.btn-add{background:var(--accent-dark);color:#fff;border:none;border-radius:9px;
    padding:11px 28px;font-size:13px;font-weight:700;cursor:pointer;letter-spacing:.04em;
    width:100%;margin-top:6px;transition:background .2s;}
.btn-add:hover{background:var(--accent);}
.btn-close{background:var(--orange);color:#fff;border:none;border-radius:9px;
    padding:11px 28px;font-size:13px;font-weight:700;cursor:pointer;letter-spacing:.04em;
    width:100%;margin-top:6px;transition:background .2s;}
.btn-close:hover{background:#d35400;}
.btn-close-shift{background:#1a1a1a;color:#fff;border:none;border-radius:9px;
    padding:11px 28px;font-size:13px;font-weight:700;cursor:pointer;letter-spacing:.04em;
    width:100%;margin-top:14px;transition:background .2s;}
.btn-close-shift:hover{background:#333;}
.right-card-disabled{opacity:0.45;pointer-events:none;}
.scrap-entry-section{background:#fff8e1;border:1px solid #ffe082;border-radius:10px;
    padding:14px 16px;margin-top:14px;}
.scrap-entry-title{font-size:11px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;
    color:#f57f17;margin-bottom:12px;display:flex;align-items:center;gap:6px;}
.scrap-row{display:flex;align-items:center;gap:10px;margin-bottom:10px;}
.scrap-row:last-child{margin-bottom:0;}
.scrap-row label{font-size:12px;font-weight:600;color:#555;min-width:140px;flex-shrink:0;}
.scrap-row .unit-sm{font-size:11px;color:#888;white-space:nowrap;}
.scrap-row input{flex:1;padding:8px 10px;border:1.5px solid #ffe082;border-radius:7px;
    font-size:13px;font-family:inherit;background:#fffde7;}
.scrap-row input:focus{outline:none;border-color:#f9a825;}

.tally-table{width:100%;border-collapse:collapse;font-size:12px;margin-top:16px;}
.tally-table th{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;
    color:var(--text-dim);padding:8px 12px;border-bottom:2px solid var(--border);text-align:left;}
.tally-table th.num{text-align:right;}
.tally-table td{padding:9px 12px;border-bottom:1px solid var(--border);vertical-align:middle;}
.tally-table td.num{text-align:right;font-variant-numeric:tabular-nums;font-weight:600;}
.tally-table tr:last-child td{border-bottom:none;}
.tally-total{background:#f8fff8;font-weight:700;color:var(--accent-dark);}

.empty-note{text-align:center;padding:28px;color:var(--text-dim);font-size:12px;}
.divider{border:none;border-top:1px solid var(--border);margin:20px 0;}
.total-row{display:flex;justify-content:space-between;align-items:center;
    background:var(--accent-light);border-radius:9px;padding:12px 16px;margin-top:14px;}
.total-label{font-size:12px;font-weight:700;color:var(--accent-dark);letter-spacing:.04em;text-transform:uppercase;}
.total-val{font-family:'Bebas Neue',sans-serif;font-size:28px;color:var(--accent-dark);letter-spacing:.04em;}
.closure-total{background:var(--orange-light);}
.closure-total .total-label,.closure-total .total-val{color:var(--orange);}

@media(max-width:720px){.two-col{grid-template-columns:1fr;}}
</style>
</head>
<body>
<form id="form1" runat="server">

<asp:HiddenField ID="hfProductId"      runat="server" Value="0"/>
<asp:HiddenField ID="hfShiftClosed"    runat="server" Value="0"/>
<asp:HiddenField ID="hfRMStockQty"     runat="server" Value="0"/>
<asp:HiddenField ID="hfRMStockUnit"    runat="server" Value=""/>
<asp:HiddenField ID="hfRMDisplayName"  runat="server" Value=""/>
<asp:HiddenField ID="hfOutputUnit"  runat="server" Value=""/>
<asp:HiddenField ID="hfRMId"        runat="server" Value="0"/>
<asp:HiddenField ID="hfRMUnit"      runat="server" Value=""/>

<nav>
    <a class="nav-logo" href="PPHome.aspx">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri"
             onerror="this.style.display='none'"/>
    </a>
    <span class="nav-title">PP — Prefilled Entry</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblNavUser" runat="server"/></span>
        <a href="PPHome.aspx" class="nav-link">&#8592; Home</a>
        <a href="PPLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>

<div class="date-bar"><asp:Label ID="lblDate" runat="server"/></div>

<div class="alert-wrap">
    <asp:Panel ID="pnlAlert" runat="server" Visible="false">
        <div class="alert"><asp:Label ID="lblAlert" runat="server"/></div>
    </asp:Panel>
</div>

<div class="page-body">

    <!-- PRODUCT SELECTOR -->
    <div class="card">
        <div class="card-title">Prefilled Conversion Entry</div>
        <div class="card-sub">Select a Prefilled Conversion product to record quantities and close shift consumption</div>
        <div style="display:flex;gap:12px;align-items:flex-end;flex-wrap:wrap;">
            <div class="form-group" style="flex:1;min-width:240px;margin-bottom:0;">
                <label>Product</label>
                <asp:DropDownList ID="ddlProduct" runat="server" AutoPostBack="true"
                    OnSelectedIndexChanged="ddlProduct_Changed"/>
            </div>
        </div>
    </div>

    <asp:Panel ID="pnlRMStock" runat="server" Visible="false">
    <div class="rm-stock-banner">
        <div class="rm-stock-label" style="margin-bottom:10px;width:100%;">
            &#x1F4E6; Raw Material Stock on Hand
        </div>
        <asp:Repeater ID="rptRMStock" runat="server">
            <ItemTemplate>
                <div class="rm-stock-item">
                    <div class="rm-stock-name"><%# Eval("RMName") %></div>
                    <div class="rm-stock-value">
                        <span class='<%# Convert.ToDecimal(Eval("Stock")) <= 0 ? "rm-stock-qty zero" : Convert.ToDecimal(Eval("Stock")) < 50 ? "rm-stock-qty low" : "rm-stock-qty ok" %>'>
                            <%# Convert.ToDecimal(Eval("Stock")).ToString("0.###") %>
                        </span>
                        <span class="rm-stock-unit"><%# Eval("Unit") %></span>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>
    </asp:Panel>

    <asp:Panel ID="pnlEntry" runat="server" Visible="false">
    <div class="two-col">

        <!-- LEFT: RUNNING TALLY -->
        <div>
            <div class="card">
                <span class="section-badge badge-tally">&#9654; Running Tally</span>
                <div class="card-title">Add to Stock</div>
                <div class="card-sub">Enter qty just produced and click Add — stock increases immediately</div>

                <div class="qty-row">
                    <div class="form-group">
                        <label>Qty Produced <span class="req">*</span></label>
                        <asp:TextBox ID="txtQty" runat="server" type="number"
                            placeholder="0.000" step="0.001" min="0.001"/>
                    </div>
                    <div class="unit-badge"><asp:Label ID="lblOutputUnit" runat="server"/></div>
                </div>

                <asp:Button ID="btnAdd" runat="server" CssClass="btn-add"
                    Text="+ Add to Stock" OnClick="btnAdd_Click" CausesValidation="false"/>

                <asp:Button ID="btnCloseShift" runat="server" CssClass="btn-close-shift"
                    Text="&#9654; CLOSE THE SHIFT"
                    OnClick="btnCloseShift_Click" CausesValidation="false"/>

                <asp:Panel ID="pnlShiftClosedMsg" runat="server" Visible="false">
                    <div style="background:#eafaf1;border:1px solid #a9dfbf;border-radius:9px;
                        padding:10px 14px;margin-top:10px;font-size:12px;font-weight:700;
                        color:var(--accent-dark);">&#10003; Shift closed — Raw Material entry enabled</div>
                </asp:Panel>

                <!-- Today's tally -->
                <hr class="divider"/>
                <div style="font-size:11px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;color:var(--text-muted);margin-bottom:10px;">Today's Entries</div>
                <asp:Panel ID="pnlTallyEmpty" runat="server">
                    <div class="empty-note">No entries yet today</div>
                </asp:Panel>
                <asp:Panel ID="pnlTallyTable" runat="server" Visible="false">
                    <table class="tally-table">
                        <thead><tr>
                            <th>Time</th>
                            <th class="num">Qty Added</th>
                        </tr></thead>
                        <tbody>
                            <asp:Repeater ID="rptTally" runat="server">
                                <ItemTemplate>
                                    <tr>
                                        <td><%# Convert.ToDateTime(Eval("CreatedAt")).ToString("hh:mm tt") %></td>
                                        <td class="num"><%# Convert.ToDecimal(Eval("Qty")).ToString("0.###") %> <%# Eval("Unit") %></td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                    <div class="total-row">
                        <span class="total-label">Total Added Today</span>
                        <span class="total-val"><asp:Label ID="lblTallyTotal" runat="server"/> <asp:Label ID="lblTallyUnit" runat="server"/></span>
                    </div>
                </asp:Panel>
            </div>
        </div>

        <!-- RIGHT: SHIFT CLOSURE -->
        <div>
            <asp:Panel ID="pnlRightCard" runat="server">
            <div class="card">
                <span class="section-badge badge-closure">&#9632; Shift Closure</span>
                <div class="card-title">Record Raw Material Used</div>
                <div class="card-sub">At end of shift — enter actual raw material consumed. This closes the loop and deducts from stock.</div>

                <div class="form-group">
                    <label>Raw Material Consumed</label>
                    <asp:DropDownList ID="ddlRawMaterial" runat="server"
                        OnSelectedIndexChanged="ddlRM_Changed" AutoPostBack="true"/>
                </div>

                <div class="qty-row">
                    <div class="form-group">
                        <label>Qty Consumed <span class="req">*</span></label>
                        <asp:TextBox ID="txtRMQty" runat="server" type="number"
                            placeholder="0.000" step="0.001" min="0.001"/>
                    </div>
                    <div class="unit-badge"><asp:Label ID="lblRMUnit" runat="server">—</asp:Label></div>
                </div>

                <!-- SCRAP QTY ENTRY — auto-loads when RM selected -->
                <asp:Panel ID="pnlScrapEntry" runat="server" Visible="false">
                <div class="scrap-entry-section">
                    <div class="scrap-entry-title">&#9851; Scrap Generated from this RM</div>
                    <asp:Repeater ID="rptScrapInputs" runat="server">
                        <ItemTemplate>
                            <div class="scrap-row">
                                <label><%# Eval("ScrapName") %> <span class="unit-sm"><%# Eval("Unit") %></span></label>
                                <input type="number" step="0.001" min="0"
                                    name='<%# "scrap_" + Eval("ScrapID") %>'
                                    placeholder="0.000" class="scrap-input"
                                    data-scrapid='<%# Eval("ScrapID") %>'
                                    data-scrapname='<%# Eval("ScrapName") %>' />
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                </asp:Panel>

                <asp:Button ID="btnClose" runat="server" CssClass="btn-close"
                    Text="&#9632; Close Shift Consumption"
                    OnClick="btnClose_Click" CausesValidation="false"/>

                <!-- Today's closures -->
                <hr class="divider"/>
                <div style="font-size:11px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;color:var(--text-muted);margin-bottom:10px;">Today's Closures</div>
                <asp:Panel ID="pnlClosureEmpty" runat="server">
                    <div class="empty-note">No closures recorded today</div>
                </asp:Panel>
                <asp:Panel ID="pnlClosureTable" runat="server" Visible="false">
                    <table class="tally-table">
                        <thead><tr>
                            <th>Time</th>
                            <th class="num">Qty Consumed</th>
                        </tr></thead>
                        <tbody>
                            <asp:Repeater ID="rptClosure" runat="server">
                                <ItemTemplate>
                                    <tr>
                                        <td><%# Convert.ToDateTime(Eval("ConsumedAt")).ToString("hh:mm tt") %></td>
                                        <td class="num"><%# Convert.ToDecimal(Eval("QtyConsumed")).ToString("0.###") %></td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                    <div class="total-row closure-total">
                        <span class="total-label">Total Consumed Today</span>
                        <span class="total-val"><asp:Label ID="lblClosureTotal" runat="server"/> <asp:Label ID="lblClosureUnit" runat="server"/></span>
                    </div>
                </asp:Panel>
            </div>
            </asp:Panel>
        </div>

    </div>
    </asp:Panel>

</div>
</form>
</body>
</html>
