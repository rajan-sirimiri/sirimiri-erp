<%@ Page Language="C#" AutoEventWireup="true" Inherits="PPApp.PPPreprocessEntry" EnableEventValidation="false" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri PP — Pre processed RM Entry</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{
    --accent:#2ecc71; --accent-dark:#27ae60; --accent-light:#eafaf1;
    --blue:#2980b9;   --blue-light:#eaf4fb;
    --orange:#e67e22; --orange-light:#fef5ec;
    --red:#e74c3c;    --red-light:#fdf3f2;
    --purple:#7b1fa2; --purple-light:#f3e5f5;
    --text:#1a1a1a;   --text-muted:#666; --text-dim:#999;
    --bg:#f0f0f0;     --surface:#fff;    --border:#e0e0e0;
    --radius:14px;    --nav-h:52px;
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

.page-body{max-width:1200px;margin:0 auto;padding:20px 20px 60px;}

/* PRODUCT SELECTOR */
.card{background:var(--surface);border-radius:var(--radius);box-shadow:0 2px 12px rgba(0,0,0,.07);padding:20px 24px;margin-bottom:20px;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.06em;margin-bottom:4px;}
.card-sub{font-size:12px;color:var(--text-muted);margin-bottom:16px;}
.form-group label{display:block;font-size:11px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;color:var(--text-muted);margin-bottom:6px;}
select,input[type=number]{width:100%;padding:10px 13px;border:1.5px solid var(--border);border-radius:9px;
    font-family:inherit;font-size:13px;color:var(--text);background:#fafafa;outline:none;}
select:focus,input:focus{border-color:var(--accent-dark);background:#fff;}

/* STAGE GRID */
.stage-grid{display:grid;grid-template-columns:1fr 1fr 1fr 1fr;gap:16px;margin-bottom:20px;}
.stage-card{background:var(--surface);border-radius:var(--radius);box-shadow:0 2px 12px rgba(0,0,0,.07);
    padding:20px 20px;border-top:4px solid var(--border);}
.stage-card.s1{border-top-color:#e74c3c;}
.stage-card.s2{border-top-color:#e67e22;}
.stage-card.s3{border-top-color:#27ae60;}
.stage-card.s4{border-top-color:#2980b9;}
.stage-num{font-size:10px;font-weight:700;letter-spacing:.1em;text-transform:uppercase;
    color:var(--text-dim);margin-bottom:6px;}
.stage-label{font-family:'Bebas Neue',sans-serif;font-size:17px;letter-spacing:.05em;margin-bottom:14px;}
.qty-row{display:flex;gap:8px;align-items:flex-end;margin-bottom:10px;}
.qty-row input{flex:1;}
.unit-tag{background:var(--blue-light);color:var(--blue);font-size:11px;font-weight:700;
    padding:10px 10px;border-radius:8px;white-space:nowrap;}
.btn-stage{width:100%;border:none;border-radius:8px;padding:10px;font-size:12px;
    font-weight:700;cursor:pointer;letter-spacing:.04em;transition:background .2s;}
.btn-s1{background:#e74c3c;color:#fff;} .btn-s1:hover{background:#c0392b;}
.btn-s2{background:#e67e22;color:#fff;} .btn-s2:hover{background:#d35400;}
.btn-s3{background:#27ae60;color:#fff;} .btn-s3:hover{background:#219a52;}

/* LOG TABLE */
.log-section{margin-top:14px;border-top:1px solid var(--border);padding-top:12px;}
.log-title{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;
    color:var(--text-dim);margin-bottom:8px;}
.log-table{width:100%;border-collapse:collapse;font-size:11px;}
.log-table td{padding:5px 4px;border-bottom:1px solid #f0f0f0;color:var(--text-muted);}
.log-table td.qty{text-align:right;font-weight:700;color:var(--text);}
.log-table tr:last-child td{border-bottom:none;}
.log-total{display:flex;justify-content:space-between;margin-top:8px;padding:8px 10px;
    border-radius:8px;font-size:13px;font-weight:700;}
.tot-s1{background:#fdf3f2;color:#e74c3c;}
.tot-s2{background:var(--orange-light);color:var(--orange);}
.tot-s3{background:var(--accent-light);color:var(--accent-dark);}

/* SCRAP SECTION */
.scrap-card{background:var(--surface);border-radius:var(--radius);box-shadow:0 2px 12px rgba(0,0,0,.07);
    padding:20px 24px;margin-bottom:20px;border-left:4px solid #f57f17;}
.scrap-title{font-family:'Bebas Neue',sans-serif;font-size:16px;letter-spacing:.06em;
    color:#f57f17;margin-bottom:6px;}
.scrap-sub{font-size:12px;color:var(--text-muted);margin-bottom:16px;}
.scrap-grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(200px,1fr));gap:12px;margin-bottom:14px;}
.scrap-item label{font-size:11px;font-weight:700;letter-spacing:.06em;text-transform:uppercase;
    color:var(--text-muted);display:block;margin-bottom:5px;}
.scrap-item input{width:100%;padding:9px 11px;border:1.5px solid #ffe082;border-radius:8px;
    font-size:13px;font-family:inherit;background:#fffde7;}
.scrap-item input:focus{outline:none;border-color:#f9a825;}
.btn-close-shift{background:#1a1a1a;color:#fff;border:none;border-radius:9px;
    padding:11px 28px;font-size:13px;font-weight:700;cursor:pointer;letter-spacing:.04em;}
.btn-close-shift:hover{background:#333;}

@media(max-width:900px){.stage-grid{grid-template-columns:1fr 1fr;} .stock-summary{grid-template-columns:1fr 1fr;}}
.stock-summary{display:grid;grid-template-columns:repeat(4,1fr);gap:12px;margin-bottom:16px;}
.ss-card{background:var(--surface);border-radius:12px;padding:14px 18px;
    box-shadow:0 2px 8px rgba(0,0,0,.06);border-left:4px solid var(--border);}
.ss-card.raw{border-left-color:#e74c3c;}
.ss-card.roasted{border-left-color:#e67e22;}
.ss-card.sorted{border-left-color:#27ae60;}
.ss-card.stage4{border-left-color:#2980b9;}
.ss-label{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;
    color:var(--text-dim);margin-bottom:6px;}
.ss-value{font-family:"Bebas Neue",sans-serif;font-size:24px;letter-spacing:.04em;}
.ss-card.raw .ss-value{color:#e74c3c;}
.ss-card.roasted .ss-value{color:#e67e22;}
.ss-card.sorted .ss-value{color:#27ae60;}
.ss-card.stage4 .ss-value{color:#2980b9;}
</style>
</head>
<body>
<form id="form1" runat="server">

<asp:HiddenField ID="hfProductId"   runat="server" Value="0"/>
<asp:HiddenField ID="hfInputRMName" runat="server" Value=""/>
<asp:HiddenField ID="hfStage1Label" runat="server" Value=""/>
<asp:HiddenField ID="hfStage2Label" runat="server" Value=""/>
<asp:HiddenField ID="hfStage3Label" runat="server" Value=""/>
<asp:HiddenField ID="hfStage4Label" runat="server" Value=""/>
<asp:HiddenField ID="hfOutputUnit"  runat="server" Value=""/>
<asp:HiddenField ID="hfBatchSize"   runat="server" Value="0"/>
<asp:HiddenField ID="hfIsPriceCalc" runat="server" Value="0"/>

<nav>
    <a class="nav-logo" href="PPHome.aspx">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    </a>
    <span class="nav-title">PP — Pre processed RM Entry</span>
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
        <div class="card-title">Pre processed RM Entry</div>
        <div class="card-sub">Select a Pre processed RM product to record stage quantities</div>
        <div class="form-group">
            <label>Product</label>
            <asp:DropDownList ID="ddlProduct" runat="server" AutoPostBack="true"
                OnSelectedIndexChanged="ddlProduct_Changed"/>
        </div>
    </div>

    <asp:Panel ID="pnlStages" runat="server" Visible="false">

    <!-- STOCK SUMMARY -->
    <div class="stock-summary">
        <div class="ss-card raw">
            <div class="ss-label"><asp:Label ID="lblInputRMTitle" runat="server">Raw Peanut</asp:Label> in Stock</div>
            <div class="ss-value"><asp:Label ID="lblRawPeanutStock" runat="server">—</asp:Label></div>
        </div>
        <div class="ss-card roasted">
            <div class="ss-label"><asp:Label ID="lblStage2Title" runat="server">Roasted</asp:Label> — Pending Sorting</div>
            <div class="ss-value"><asp:Label ID="lblRoastedPending" runat="server">—</asp:Label></div>
        </div>
        <div class="ss-card sorted">
            <div class="ss-label"><asp:Label ID="lblStage3Title" runat="server">Sorted</asp:Label> — Stage 3 Output</div>
            <div class="ss-value"><asp:Label ID="lblSortedStock" runat="server">—</asp:Label></div>
        </div>
        <asp:Panel ID="pnlS4Summary" runat="server" Visible="false">
        <div class="ss-card stage4">
            <div class="ss-label"><asp:Label ID="lblStage4Title" runat="server">Stage 4</asp:Label> — Available for Production</div>
            <div class="ss-value"><asp:Label ID="lblStage4Stock" runat="server">—</asp:Label></div>
        </div>
        </asp:Panel>
    </div>

    <!-- THREE STAGE CARDS -->
    <div class="stage-grid">

        <!-- STAGE 1 -->
        <div class="stage-card s1">
            <div class="stage-num">Stage 1</div>
            <div class="stage-label"><asp:Label ID="lblStage1" runat="server"/></div>
            <div class="qty-row">
                <input type="number" step="0.001" min="0.001" id="txtS1" runat="server" placeholder="0.000"/>
                <span class="unit-tag"><asp:Label ID="lblS1Unit" runat="server"/></span>
            </div>
            <asp:Button ID="btnS1" runat="server" CssClass="btn-stage btn-s1"
                Text="Record Stage 1" OnClick="btnS1_Click" CausesValidation="false"/>
            <div class="log-section">
                <div class="log-title">Today's Entries</div>
                <asp:Panel ID="pnlS1Empty" runat="server"><div style="font-size:11px;color:var(--text-dim);font-style:italic;">No entries yet</div></asp:Panel>
                <asp:Panel ID="pnlS1Table" runat="server" Visible="false">
                    <table class="log-table">
                        <asp:Repeater ID="rptS1" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Convert.ToDateTime(Eval("CreatedAt")).ToString("hh:mm tt") %></td>
                                    <td class="qty"><%# Convert.ToDecimal(Eval("Qty")).ToString("0.###") %></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </table>
                    <div class="log-total tot-s1">
                        <span>Total Dispensed</span>
                        <span><asp:Label ID="lblS1Total" runat="server"/></span>
                    </div>
                </asp:Panel>
            </div>
        </div>

        <!-- STAGE 2 -->
        <div class="stage-card s2">
            <div class="stage-num">Stage 2</div>
            <div class="stage-label"><asp:Label ID="lblStage2" runat="server"/></div>
            <div class="qty-row">
                <input type="number" step="0.001" min="0.001" id="txtS2" runat="server" placeholder="0.000"/>
                <span class="unit-tag"><asp:Label ID="lblS2Unit" runat="server"/></span>
            </div>
            <asp:Button ID="btnS2" runat="server" CssClass="btn-stage btn-s2"
                Text="Record Stage 2" OnClick="btnS2_Click" CausesValidation="false"/>
            <div class="log-section">
                <div class="log-title">Today's Entries</div>
                <asp:Panel ID="pnlS2Empty" runat="server"><div style="font-size:11px;color:var(--text-dim);font-style:italic;">No entries yet</div></asp:Panel>
                <asp:Panel ID="pnlS2Table" runat="server" Visible="false">
                    <table class="log-table">
                        <asp:Repeater ID="rptS2" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Convert.ToDateTime(Eval("CreatedAt")).ToString("hh:mm tt") %></td>
                                    <td class="qty"><%# Convert.ToDecimal(Eval("Qty")).ToString("0.###") %></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </table>
                    <div class="log-total tot-s2">
                        <span>Total Processed</span>
                        <span><asp:Label ID="lblS2Total" runat="server"/></span>
                    </div>
                </asp:Panel>
            </div>
        </div>

        <!-- STAGE 3 -->
        <div class="stage-card s3">
            <div class="stage-num">Stage 3 — Available for Production</div>
            <div class="stage-label"><asp:Label ID="lblStage3" runat="server"/></div>
            <div class="qty-row">
                <input type="number" step="0.001" min="0.001" id="txtS3" runat="server" placeholder="0.000"/>
                <span class="unit-tag"><asp:Label ID="lblS3Unit" runat="server"/></span>
            </div>
            <asp:Button ID="btnS3" runat="server" CssClass="btn-stage btn-s3"
                Text="Record Stage 3" OnClick="btnS3_Click" CausesValidation="false"/>
            <div class="log-section">
                <div class="log-title">Today's Entries</div>
                <asp:Panel ID="pnlS3Empty" runat="server"><div style="font-size:11px;color:var(--text-dim);font-style:italic;">No entries yet</div></asp:Panel>
                <asp:Panel ID="pnlS3Table" runat="server" Visible="false">
                    <table class="log-table">
                        <asp:Repeater ID="rptS3" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Convert.ToDateTime(Eval("CreatedAt")).ToString("hh:mm tt") %></td>
                                    <td class="qty"><%# Convert.ToDecimal(Eval("Qty")).ToString("0.###") %></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </table>
                    <div class="log-total tot-s3">
                        <span>Total Available</span>
                        <span><asp:Label ID="lblS3Total" runat="server"/></span>
                    </div>
                </asp:Panel>
            </div>
        </div>

        <!-- STAGE 4 (optional — shown only if Stage4Label is set) -->
        <asp:Panel ID="pnlStage4Card" runat="server" Visible="false">
        <div class="stage-card s4">
            <div class="stage-num">Stage 4 — Available for Production</div>
            <div class="stage-label"><asp:Label ID="lblStage4" runat="server"/></div>
            <div class="qty-row">
                <input type="number" step="0.001" min="0.001" id="txtS4" runat="server" placeholder="0.000"/>
                <span class="unit-tag"><asp:Label ID="lblS4Unit" runat="server"/></span>
            </div>
            <asp:Button ID="btnS4" runat="server" CssClass="btn-stage btn-s3"
                Text="Record Stage 4" OnClick="btnS4_Click" CausesValidation="false"/>
            <div class="log-section">
                <div class="log-title">Today's Entries</div>
                <asp:Panel ID="pnlS4Empty" runat="server"><div style="font-size:11px;color:var(--text-dim);font-style:italic;">No entries yet</div></asp:Panel>
                <asp:Panel ID="pnlS4Table" runat="server" Visible="false">
                    <table class="log-table">
                        <asp:Repeater ID="rptS4" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Convert.ToDateTime(Eval("CreatedAt")).ToString("hh:mm tt") %></td>
                                    <td class="qty"><%# Convert.ToDecimal(Eval("Qty")).ToString("0.###") %></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </table>
                    <div class="log-total tot-s3">
                        <span>Total Stage 4</span>
                        <span><asp:Label ID="lblS4Total" runat="server"/></span>
                    </div>
                </asp:Panel>
            </div>
        </div>
        </asp:Panel>

    </div>

    <!-- SCRAP SECTION -->
    <div class="scrap-card">
        <div class="scrap-title">&#9851; End of Shift — Scrap Generated</div>
        <div class="scrap-sub">Enter scrap quantities from all stages, then close the shift</div>
        <asp:Panel ID="pnlScrapInputs" runat="server">
            <div class="scrap-grid">
                <asp:Repeater ID="rptScrapItems" runat="server">
                    <ItemTemplate>
                        <div class="scrap-item">
                            <label><%# Eval("ScrapName") %> <span style="color:#bbb;font-weight:400;">(<%# Eval("Unit") %>)</span></label>
                            <input type="number" step="0.001" min="0" placeholder="0.000"
                                name='<%# "scrap_" + Eval("ScrapID") %>'
                                data-scrapid='<%# Eval("ScrapID") %>'
                                data-scrapname='<%# Eval("ScrapName") %>'/>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlNoScrap" runat="server" Visible="false">
            <div style="font-size:12px;color:var(--text-dim);font-style:italic;margin-bottom:14px;">No scrap materials linked to this product's input RM</div>
        </asp:Panel>
        <asp:Button ID="btnCloseShift" runat="server" CssClass="btn-close-shift"
            Text="&#9654; Close Shift" OnClick="btnCloseShift_Click" CausesValidation="false"/>
    </div>

    </asp:Panel>

</div>
</form>
<script src="/StockApp/erp-keepalive.js"></script>
</body>
</html>
