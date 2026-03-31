<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Reports.aspx.cs" Inherits="StockApp.Reports" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Sirimiri — Reports</title>
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/html2pdf.js/0.10.1/html2pdf.bundle.min.js"></script>
    <style>
        :root {
            --bg:          #f5f5f5;
            --surface:     #ffffff;
            --surface2:    #f9f9f9;
            --border:      #e0e0e0;
            --accent:      #cc1e1e;
            --accent-dark: #a81818;
            --accent-glow: rgba(204,30,30,0.15);
            --text:        #1a1a1a;
            --text-muted:  #666666;
            --success:     #1a9e6a;
            --warning:     #e07c00;
            --radius:      10px;
        }
        *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
        body {
            background: var(--bg);
            color: var(--text);
            font-family: 'DM Sans', sans-serif;
            font-weight: 400;
            min-height: 100vh;
            padding: 0 0 48px;
            overflow-x: hidden;
        }
        body::before {
            content: '';
            position: fixed;
            inset: 0;
            background-image: linear-gradient(var(--border) 1px, transparent 1px), linear-gradient(90deg, var(--border) 1px, transparent 1px);
            background-size: 48px 48px;
            opacity: 0.06;
            pointer-events: none;
            z-index: 0;
        }

        /* ── NAVBAR ── */
        .navbar {
            position: relative; z-index: 100;
            background: linear-gradient(135deg, #1a1a1a 0%, #cc1e1e 100%);
            display: flex; align-items: center;
            padding: 0 24px; gap: 4px;
            box-shadow: 0 2px 12px rgba(0,0,0,0.25);
        }
        .nav-group { position: relative; }
        .nav-item {
            display: block; padding: 14px 18px;
            color: #fff; font-size: 13px; font-weight: 600;
            cursor: pointer; letter-spacing: .04em;
            text-transform: uppercase; white-space: nowrap;
            transition: background .2s;
        }
        .nav-item:hover, .nav-group:hover > .nav-item { background: rgba(255,255,255,.12); }
        .nav-item.active { background: rgba(255,255,255,.18); }
        .nav-item .chevron { font-size: 9px; margin-left: 5px; }
        .nav-dropdown {
            display: none; position: absolute; top: 100%; left: 0;
            background: #fff; border: 1px solid var(--border);
            border-radius: 0 0 8px 8px; min-width: 220px;
            box-shadow: 0 8px 24px rgba(0,0,0,.12); z-index: 200;
        }
        .nav-group:hover .nav-dropdown { display: block; }
        .nav-dropdown a {
            display: block; padding: 11px 18px;
            color: var(--text); font-size: 13px; text-decoration: none;
            border-bottom: 1px solid var(--border); transition: background .15s;
        }
        .nav-dropdown a:last-child { border-bottom: none; }
        .nav-dropdown a:hover { background: var(--surface2); }
        .nav-dropdown a.active { color: var(--accent); font-weight: 600; }

        /* ── PAGE LAYOUT ── */
        .page-wrapper {
            position: relative; z-index: 1;
            max-width: 1100px; margin: 32px auto; padding: 0 24px;
        }
        .page-title {
            font-family: 'Bebas Neue', sans-serif;
            font-size: 32px; letter-spacing: .06em;
            color: var(--text); margin-bottom: 6px;
        }
        .page-subtitle { color: var(--text-muted); font-size: 14px; margin-bottom: 24px; }

        /* ── REPORT TABS ── */
        .report-tabs {
            display: flex; gap: 4px; margin-bottom: 24px;
            border-bottom: 2px solid var(--border);
        }
        .report-tab {
            padding: 10px 22px; font-size: 13px; font-weight: 600;
            cursor: pointer; border-radius: 8px 8px 0 0;
            border: 1px solid transparent; border-bottom: none;
            color: var(--text-muted); background: transparent;
            transition: all .2s; letter-spacing: .03em;
            text-transform: uppercase;
        }
        .report-tab:hover { background: var(--surface); color: var(--text); }
        .report-tab.active {
            background: var(--surface); color: var(--accent);
            border-color: var(--border); border-bottom-color: var(--surface);
            margin-bottom: -2px;
        }

        /* ── FILTER CARD ── */
        .filter-card {
            background: var(--surface); border: 1px solid var(--border);
            border-radius: var(--radius); padding: 20px 24px;
            margin-bottom: 20px;
            box-shadow: 0 2px 8px rgba(0,0,0,.04);
        }
        .filter-row {
            display: flex; flex-wrap: wrap; gap: 14px;
            align-items: flex-end;
        }
        .filter-group { display: flex; flex-direction: column; gap: 5px; }
        .filter-group label {
            font-size: 11px; font-weight: 600; text-transform: uppercase;
            letter-spacing: .05em; color: var(--text-muted);
        }
        .filter-group select,
        .filter-group input[type=text] {
            padding: 8px 12px; border: 1px solid var(--border);
            border-radius: 6px; font-size: 13px; font-family: inherit;
            background: var(--surface2); color: var(--text);
            min-width: 160px;
        }
        .filter-group select:focus,
        .filter-group input:focus {
            outline: none; border-color: var(--accent);
            box-shadow: 0 0 0 3px var(--accent-glow);
        }

        /* ── PERIOD PILLS ── */
        .period-pills { display: flex; gap: 6px; }
        .period-pill {
            padding: 7px 16px; border-radius: 20px;
            font-size: 12px; font-weight: 600; cursor: pointer;
            border: 1.5px solid var(--border); background: var(--surface2);
            color: var(--text-muted); transition: all .2s;
            letter-spacing: .03em;
        }
        .period-pill:hover { border-color: var(--accent); color: var(--accent); }
        .period-pill.active {
            background: var(--accent); color: #fff;
            border-color: var(--accent);
        }

        /* ── BUTTONS ── */
        .btn {
            padding: 9px 22px; border-radius: 6px; font-size: 13px;
            font-weight: 600; cursor: pointer; border: none;
            font-family: inherit; letter-spacing: .03em;
            transition: all .2s; text-transform: uppercase;
        }
        .btn-primary { background: var(--accent); color: #fff; }
        .btn-primary:hover { background: var(--accent-dark); }
        .btn-outline {
            background: transparent; color: var(--text);
            border: 1.5px solid var(--border);
        }
        .btn-outline:hover { border-color: var(--accent); color: var(--accent); }
        .btn-pdf { background: #1a6acc; color: #fff; }
        .btn-pdf:hover { background: #155aaa; }

        /* ── REPORT RESULTS ── */
        .report-card {
            background: var(--surface); border: 1px solid var(--border);
            border-radius: var(--radius); overflow: hidden;
            box-shadow: 0 2px 8px rgba(0,0,0,.04);
        }
        .report-header {
            padding: 16px 24px; border-bottom: 1px solid var(--border);
            display: flex; align-items: center; justify-content: space-between;
            background: var(--surface2);
        }
        .report-header-title {
            font-family: 'Bebas Neue', sans-serif;
            font-size: 20px; letter-spacing: .05em;
        }
        .report-meta { font-size: 12px; color: var(--text-muted); }

        /* ── TABLE ── */
        .report-table-wrap { overflow-x: auto; }
        table.report-tbl {
            width: 100%; border-collapse: collapse; font-size: 13px;
        }
        table.report-tbl th {
            background: #1a1a1a; color: #fff;
            padding: 10px 14px; text-align: left;
            font-size: 11px; font-weight: 600;
            text-transform: uppercase; letter-spacing: .05em;
            white-space: nowrap;
        }
        table.report-tbl td {
            padding: 9px 14px; border-bottom: 1px solid var(--border);
            vertical-align: middle;
        }
        table.report-tbl tr:last-child td { border-bottom: none; }
        table.report-tbl tr:hover td { background: #fafafa; }

        /* ── ROW TYPE HIGHLIGHTS ── */
        tr.row-purchase td { background: #fff; }
        tr.row-stock td {
            background: #fff8e1;
            border-left: 4px solid var(--warning);
        }
        tr.row-stock td:first-child { padding-left: 10px; }

        .badge {
            display: inline-block; padding: 2px 10px;
            border-radius: 12px; font-size: 11px; font-weight: 700;
            letter-spacing: .03em; text-transform: uppercase;
        }
        .badge-purchase { background: #e8f5e9; color: #2e7d32; }
        .badge-stock    { background: #fff3e0; color: #e07c00; }

        /* ── TOTALS ROW ── */
        tr.row-total td {
            background: #f5f5f5; font-weight: 700;
            border-top: 2px solid var(--border);
        }

        /* ── EMPTY STATE ── */
        .empty-state {
            padding: 48px; text-align: center; color: var(--text-muted);
        }
        .empty-state .icon { font-size: 40px; margin-bottom: 12px; }
        .empty-state p { font-size: 14px; }

        /* ── DISTRIBUTOR SECTION (Report 1) ── */
        .dist-section { margin-bottom: 0; }
        .dist-section-header {
            padding: 12px 20px;
            background: linear-gradient(135deg, #1a1a1a, #333);
            color: #fff; font-weight: 700; font-size: 14px;
            display: flex; justify-content: space-between; align-items: center;
        }
        .dist-section-header span { font-size: 12px; opacity: .75; font-weight: 400; }

        @media print, (max-width: 640px) {
            .filter-card, .report-tabs, .btn-pdf, .navbar { display: none !important; }
            .report-card { box-shadow: none; border: none; }
        }
    </style>
</head>
<body>

    <!-- NAVBAR -->
    <nav class="navbar">
            <a href="ERPHome.aspx" class="nav-item" style="text-decoration:none;" title="Back to ERP Home">&#x2302; Home</a>

        <div class="nav-group">
            <span class="nav-item">Home <span class="chevron">&#9660;</span></span>
            <div class="nav-dropdown">
                <a href="StockEntry.aspx">Distributor Stock Position Entry</a>
                <a href="DailySales.aspx">Daily Sales Entry</a>
            </div>
        </div>
        <asp:Panel ID="pnlAdminMenu" runat="server" Visible="false" style="position:relative;">
        <div class="nav-group">
            <span class="nav-item">Admin <span class="chevron">&#9660;</span></span>
            <div class="nav-dropdown">
                <a href="UserAdmin.aspx">User Management</a>
                <a href="ProductMaster.aspx">Product Master</a>
            </div>
        </div>
        </asp:Panel>
        <div class="nav-group">
            <span class="nav-item active">Reports <span class="chevron">&#9660;</span></span>
            <div class="nav-dropdown">
                <a href="Reports.aspx" class="active">Stock Movement</a>
                <a href="Reports.aspx?tab=daily">Daily Sales</a>
            </div>
        </div>
        <div style="margin-left:auto;display:flex;align-items:center;gap:20px;font-size:13px;">
            <asp:Label ID="lblUserInfo" runat="server" style="color:#fff;opacity:.9;font-weight:500;" />
            <a href="Logout.aspx" style="color:#fff;font-weight:700;text-decoration:none;border:1.5px solid rgba(255,255,255,.6);padding:5px 14px;border-radius:6px;opacity:.9;" onclick="return confirm('Sign out?')">&#x2192; Sign Out</a>
        </div>
    </nav>

    <form id="form1" runat="server">
    <div class="page-wrapper">

        <div class="page-title">Reports</div>
        <div class="page-subtitle">Distributor Stock Movement &amp; Daily Sales Analysis</div>

        <!-- REPORT TABS -->
        <div class="report-tabs">
            <div class="report-tab <%= ActiveTab=="stock" ? "active" : "" %>"
                 onclick="switchTab('stock')">&#x1F4E6; Stock Movement</div>
            <div class="report-tab <%= ActiveTab=="daily" ? "active" : "" %>"
                 onclick="switchTab('daily')">&#x1F4CA; Daily Sales</div>
        </div>

        <!-- ════════════════════════════════════════════════
             REPORT 1 — DISTRIBUTOR STOCK MOVEMENT
        ════════════════════════════════════════════════ -->
        <div id="divStockReport" runat="server">

            <!-- Filters -->
            <div class="filter-card">
                <div class="filter-row">

                    <!-- Period pills -->
                    <div class="filter-group">
                        <label>Period</label>
                        <div class="period-pills">
                            <asp:HiddenField ID="hfStockDays" runat="server" Value="30" />
                            <div class="period-pill <%= StockDays==30?"active":"" %>" onclick="setPeriod('stock',30,this)">30 Days</div>
                            <div class="period-pill <%= StockDays==60?"active":"" %>" onclick="setPeriod('stock',60,this)">60 Days</div>
                            <div class="period-pill <%= StockDays==90?"active":"" %>" onclick="setPeriod('stock',90,this)">90 Days</div>
                        </div>
                    </div>

                    <!-- Date range -->
                    <div class="filter-group">
                        <label>From</label>
                        <asp:TextBox ID="txtStockFrom" runat="server" TextMode="Date" CssClass="filter-input" onchange="onDateChange('stock')" />
                    </div>
                    <div class="filter-group">
                        <label>To</label>
                        <asp:TextBox ID="txtStockTo" runat="server" TextMode="Date" CssClass="filter-input" onchange="onDateChange('stock')" />
                    </div>

                    <!-- State -->
                    <div class="filter-group">
                        <label>State</label>
                        <asp:DropDownList ID="ddlStockState" runat="server" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlStockState_Changed" CssClass="filter-select">
                            <asp:ListItem Text="All States" Value="0" />
                        </asp:DropDownList>
                    </div>

                    <!-- City -->
                    <div class="filter-group">
                        <label>City</label>
                        <asp:DropDownList ID="ddlStockCity" runat="server" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlStockCity_Changed" CssClass="filter-select">
                            <asp:ListItem Text="All Cities" Value="0" />
                        </asp:DropDownList>
                    </div>

                    <!-- Distributor -->
                    <div class="filter-group">
                        <label>Distributor</label>
                        <asp:DropDownList ID="ddlStockDist" runat="server" CssClass="filter-select">
                            <asp:ListItem Text="All Distributors" Value="0" />
                        </asp:DropDownList>
                    </div>

                    <div class="filter-group">
                        <label>&nbsp;</label>
                        <div style="display:flex;gap:8px;">
                            <asp:Button ID="btnStockRun" runat="server" Text="Run Report"
                                CssClass="btn btn-primary" OnClick="btnStockRun_Click" />
                            <button type="button" class="btn btn-pdf" onclick="downloadPDF('divStockResult','Stock_Movement_Report')">&#x2B07; PDF</button>
                        </div>
                    </div>

                </div>
            </div>

            <!-- Results -->
            <asp:Panel ID="pnlStockResult" runat="server" Visible="false">
                <div id="divStockResult">
                    <asp:PlaceHolder ID="phStockReport" runat="server" />
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlStockEmpty" runat="server" Visible="false">
                <div class="report-card">
                    <div class="empty-state">
                        <div class="icon">&#x1F4ED;</div>
                        <p>No data found for the selected filters.</p>
                    </div>
                </div>
            </asp:Panel>

        </div>

        <!-- ════════════════════════════════════════════════
             REPORT 2 — DAILY SALES REPORT
        ════════════════════════════════════════════════ -->
        <div id="divDailyReport" runat="server" visible="false">

            <!-- Filters -->
            <div class="filter-card">
                <div class="filter-row">

                    <div class="filter-group">
                        <label>Period</label>
                        <div class="period-pills">
                            <asp:HiddenField ID="hfDailyDays" runat="server" Value="30" />
                            <div class="period-pill <%= DailyDays==30?"active":"" %>" onclick="setPeriod('daily',30,this)">30 Days</div>
                            <div class="period-pill <%= DailyDays==60?"active":"" %>" onclick="setPeriod('daily',60,this)">60 Days</div>
                            <div class="period-pill <%= DailyDays==90?"active":"" %>" onclick="setPeriod('daily',90,this)">90 Days</div>
                        </div>
                    </div>

                    <div class="filter-group">
                        <label>From</label>
                        <asp:TextBox ID="txtDailyFrom" runat="server" TextMode="Date" onchange="onDateChange('daily')" />
                    </div>
                    <div class="filter-group">
                        <label>To</label>
                        <asp:TextBox ID="txtDailyTo" runat="server" TextMode="Date" onchange="onDateChange('daily')" />
                    </div>

                    <div class="filter-group">
                        <label>&nbsp;</label>
                        <div style="display:flex;gap:8px;">
                            <asp:Button ID="btnDailyRun" runat="server" Text="Run Report"
                                CssClass="btn btn-primary" OnClick="btnDailyRun_Click" />
                            <button type="button" class="btn btn-pdf" onclick="downloadPDF('divDailyResult','Daily_Sales_Report')">&#x2B07; PDF</button>
                        </div>
                    </div>

                </div>
            </div>

            <!-- Results -->
            <asp:Panel ID="pnlDailyResult" runat="server" Visible="false">
                <div id="divDailyResult">
                    <asp:PlaceHolder ID="phDailyReport" runat="server" />
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlDailyEmpty" runat="server" Visible="false">
                <div class="report-card">
                    <div class="empty-state">
                        <div class="icon">&#x1F4ED;</div>
                        <p>No data found for the selected filters.</p>
                    </div>
                </div>
            </asp:Panel>

        </div>

    </div><!-- page-wrapper -->
    </form>

    <script>
        function switchTab(tab) {
            window.location.href = 'Reports.aspx?tab=' + tab;
        }

        function setPeriod(report, days, el) {
            if (report === 'stock') {
                document.getElementById('<%= hfStockDays.ClientID %>').value = days;
                // Clear date range
                document.getElementById('<%= txtStockFrom.ClientID %>').value = '';
                document.getElementById('<%= txtStockTo.ClientID %>').value = '';
                // Update pills
                document.querySelectorAll('#divStockReport .period-pill').forEach(function(p) { p.classList.remove('active'); });
            } else {
                document.getElementById('<%= hfDailyDays.ClientID %>').value = days;
                // Clear date range
                document.getElementById('<%= txtDailyFrom.ClientID %>').value = '';
                document.getElementById('<%= txtDailyTo.ClientID %>').value = '';
                // Update pills
                document.querySelectorAll('#divDailyReport .period-pill').forEach(function(p) { p.classList.remove('active'); });
            }
            el.classList.add('active');
        }

        function onDateChange(report) {
            if (report === 'stock') {
                var f = document.getElementById('<%= txtStockFrom.ClientID %>').value;
                var t = document.getElementById('<%= txtStockTo.ClientID %>').value;
                if (f || t) {
                    document.getElementById('<%= hfStockDays.ClientID %>').value = '0';
                    document.querySelectorAll('#divStockReport .period-pill').forEach(function(p) { p.classList.remove('active'); });
                }
            } else {
                var f = document.getElementById('<%= txtDailyFrom.ClientID %>').value;
                var t = document.getElementById('<%= txtDailyTo.ClientID %>').value;
                if (f || t) {
                    document.getElementById('<%= hfDailyDays.ClientID %>').value = '0';
                    document.querySelectorAll('#divDailyReport .period-pill').forEach(function(p) { p.classList.remove('active'); });
                }
            }
        }

        function downloadPDF(divId, filename) {
            var el = document.getElementById(divId);
            var opt = {
                margin: 10,
                filename: filename + '.pdf',
                image: { type: 'jpeg', quality: 0.98 },
                html2canvas: { scale: 2 },
                jsPDF: { unit: 'mm', format: 'a4', orientation: 'landscape' }
            };
            html2pdf().set(opt).from(el).save();
        }
    </script>
</body>
</html>
