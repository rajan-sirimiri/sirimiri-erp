<%@ Page Language="C#" AutoEventWireup="true" Inherits="FINApp.FINSalesAnalytics" EnableEventValidation="false" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri FIN — Sales Analytics</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600;700&display=swap" rel="stylesheet"/>
<script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/4.4.1/chart.umd.min.js"></script>
<style>
:root{
    --accent:#8e44ad; --accent-dark:#6c3483; --accent-light:#f4ecf7;
    --teal:#1a9e6a; --orange:#e67e22; --red:#e74c3c; --blue:#2980b9;
    --text:#1a1a1a; --text-muted:#666; --text-dim:#999;
    --bg:#f0f0f0; --surface:#fff; --border:#e0e0e0; --radius:14px;
}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);color:var(--text);min-height:100vh;}
nav{background:#1a1a1a;height:52px;display:flex;align-items:center;padding:0 20px;gap:12px;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;}
.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.08em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:16px;}
.nav-user{color:rgba(255,255,255,.8);font-size:12px;}
.nav-link{color:#fff;font-size:12px;font-weight:600;text-decoration:none;opacity:.8;} .nav-link:hover{opacity:1;}
.container{max-width:1200px;margin:0 auto;padding:24px 20px 60px;}

/* Section tabs */
.section-tabs{display:flex;gap:0;margin-bottom:24px;border-radius:10px;overflow:hidden;border:2px solid var(--accent);}
.section-tab{flex:1;padding:14px 20px;text-align:center;font-family:'Bebas Neue',sans-serif;font-size:16px;letter-spacing:.06em;cursor:pointer;background:#fff;color:var(--accent);border:none;transition:all .2s;}
.section-tab.active{background:var(--accent);color:#fff;}
.section-tab:hover:not(.active){background:var(--accent-light);}

.card{background:var(--surface);border-radius:var(--radius);box-shadow:0 2px 16px rgba(0,0,0,.08);padding:24px;margin-bottom:20px;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:20px;letter-spacing:.06em;margin-bottom:4px;}
.card-sub{font-size:12px;color:var(--text-muted);margin-bottom:16px;}

.filter-row{display:flex;gap:12px;align-items:center;flex-wrap:wrap;margin-bottom:18px;}
.filter-row label{font-size:11px;font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:var(--text-dim);}
.filter-select{padding:8px 14px;border:1.5px solid var(--border);border-radius:8px;font-size:13px;font-family:inherit;min-width:200px;}

/* Data tables */
.data-table{width:100%;border-collapse:collapse;font-size:11px;margin-bottom:16px;}
.data-table th{font-size:9px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:#fff;background:var(--accent);padding:8px 6px;text-align:left;position:sticky;top:0;}
.data-table td{padding:6px;border-bottom:1px solid #f0f0f0;vertical-align:middle;}
.data-table tr:nth-child(even){background:#fafafa;}
.data-table tr:hover{background:#f5f0f8;}
.data-table .num{text-align:right;font-family:monospace;font-weight:600;}
.data-table .growth-pos{color:var(--teal);font-weight:700;}
.data-table .growth-neg{color:var(--red);font-weight:700;}
.table-scroll{max-height:500px;overflow:auto;border:1px solid var(--border);border-radius:8px;}

/* Chart containers */
.chart-row{display:grid;grid-template-columns:1fr 1fr;gap:20px;margin-bottom:20px;}
.chart-box{background:#fff;border:1px solid var(--border);border-radius:10px;padding:16px;}
.chart-title{font-family:'Bebas Neue',sans-serif;font-size:14px;letter-spacing:.05em;color:var(--text-muted);margin-bottom:10px;}

/* KPI cards */
.kpi-row{display:flex;gap:14px;margin-bottom:20px;flex-wrap:wrap;}
.kpi{background:#fff;border:1px solid var(--border);border-radius:10px;padding:14px 20px;flex:1;min-width:140px;}
.kpi-val{font-family:'Bebas Neue',sans-serif;font-size:26px;}
.kpi-lbl{font-size:9px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);}

/* Distributor cards */
.dist-card{background:#fff;border:1px solid var(--border);border-radius:10px;padding:14px;margin-bottom:10px;display:grid;grid-template-columns:1fr auto;gap:12px;}
.dist-name{font-weight:700;font-size:13px;}
.dist-city{font-size:11px;color:var(--text-muted);}
.dist-tag{display:inline-block;padding:2px 6px;border-radius:4px;font-size:9px;font-weight:700;letter-spacing:.04em;margin-left:6px;}
.dist-tag-di{background:#eafaf1;color:var(--teal);}
.dist-tag-st{background:#ebf5fb;color:var(--blue);}
.dist-metrics{text-align:right;}
.dist-val{font-family:'Bebas Neue',sans-serif;font-size:20px;color:var(--accent);}
.dist-orders{font-size:10px;color:var(--text-dim);}

.section{display:none;}
.section.active{display:block;}
.loading{text-align:center;padding:40px;color:var(--text-dim);font-size:14px;}

@media(max-width:768px){
    .chart-row{grid-template-columns:1fr;}
    .kpi-row{flex-wrap:wrap;}
    .kpi{min-width:calc(50% - 10px);}
    .filter-select{min-width:100%;}
}
</style>
</head>
<body>
<form id="form1" runat="server">

<!-- Hidden fields for chart data -->
<asp:HiddenField ID="hfSection" runat="server" Value="1"/>
<asp:HiddenField ID="hfStateMonthlyData" runat="server"/>
<asp:HiddenField ID="hfCityMonthlyData" runat="server"/>
<asp:HiddenField ID="hfProductMonthlyData" runat="server"/>
<asp:HiddenField ID="hfDistributorChartData" runat="server"/>

<nav>
    <a class="nav-logo" href="FINHome.aspx">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    </a>
    <span class="nav-title">FIN — Sales Analytics Dashboard</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblNavUser" runat="server"/></span>
        <a href="FINHome.aspx" class="nav-link">&#8592; Home</a>
        <a href="FINLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>

<div class="container">

<!-- SECTION TABS -->
<div class="section-tabs">
    <asp:LinkButton ID="btnTab1" runat="server" CssClass="section-tab active" OnClick="btnTab1_Click" CausesValidation="false">&#x1F4C8; Overall Sales Growth</asp:LinkButton>
    <asp:LinkButton ID="btnTab2" runat="server" CssClass="section-tab" OnClick="btnTab2_Click" CausesValidation="false">&#x1F4E6; Product Sales Analysis</asp:LinkButton>
    <asp:LinkButton ID="btnTab3" runat="server" CssClass="section-tab" OnClick="btnTab3_Click" CausesValidation="false">&#x1F465; Distributor Performance</asp:LinkButton>
</div>

<!-- ═══════════════════════════════════════════════════ -->
<!-- SECTION 1: OVERALL SALES GROWTH                    -->
<!-- ═══════════════════════════════════════════════════ -->
<asp:Panel ID="pnlSection1" runat="server">

<!-- State-level KPIs -->
<div class="kpi-row">
    <div class="kpi"><div class="kpi-val" style="color:var(--accent);"><asp:Label ID="lblTotalSales" runat="server"/></div><div class="kpi-lbl">Total Sales</div></div>
    <div class="kpi"><div class="kpi-val" style="color:var(--teal);"><asp:Label ID="lblTotalInvoices" runat="server"/></div><div class="kpi-lbl">Invoices</div></div>
    <div class="kpi"><div class="kpi-val" style="color:var(--blue);"><asp:Label ID="lblTotalCustomers" runat="server"/></div><div class="kpi-lbl">Customers</div></div>
    <div class="kpi"><div class="kpi-val" style="color:var(--orange);"><asp:Label ID="lblTotalStates" runat="server"/></div><div class="kpi-lbl">States</div></div>
    <div class="kpi"><div class="kpi-val"><asp:Label ID="lblDataRange" runat="server"/></div><div class="kpi-lbl">Date Range</div></div>
</div>

<!-- State monthly chart -->
<div class="card">
    <div class="card-title">State-Level Monthly Sales</div>
    <div class="card-sub">Month-over-month sales by state</div>
    <div style="height:360px;"><canvas id="chartStateMonthly"></canvas></div>
</div>

<!-- State monthly table -->
<div class="card">
    <div class="card-title">Monthly Sales by State (Table)</div>
    <div class="table-scroll">
        <asp:Literal ID="litStateTable" runat="server"/>
    </div>
</div>

<!-- District drill-down -->
<div class="card">
    <div class="card-title">District-Level Growth</div>
    <div class="card-sub">Select a state to see city/district-level performance</div>
    <div class="filter-row">
        <label>State</label>
        <asp:DropDownList ID="ddlS1State" runat="server" CssClass="filter-select" AutoPostBack="true" OnSelectedIndexChanged="ddlS1State_Changed"/>
    </div>
    <asp:Panel ID="pnlCityChart" runat="server" Visible="false">
        <div style="height:360px;margin-bottom:16px;"><canvas id="chartCityMonthly"></canvas></div>
        <div class="table-scroll">
            <asp:Literal ID="litCityTable" runat="server"/>
        </div>
    </asp:Panel>
</div>
</asp:Panel>

<!-- ═══════════════════════════════════════════════════ -->
<!-- SECTION 2: PRODUCT SALES ANALYSIS                  -->
<!-- ═══════════════════════════════════════════════════ -->
<asp:Panel ID="pnlSection2" runat="server" Visible="false">
<div class="card">
    <div class="card-title">Product Sales Analysis</div>
    <div class="card-sub">Month-over-month product performance by state and district</div>
    <div class="filter-row">
        <label>State</label>
        <asp:DropDownList ID="ddlS2State" runat="server" CssClass="filter-select" AutoPostBack="true" OnSelectedIndexChanged="ddlS2State_Changed"/>
        <label>City/District</label>
        <asp:DropDownList ID="ddlS2City" runat="server" CssClass="filter-select" AutoPostBack="true" OnSelectedIndexChanged="ddlS2City_Changed">
            <asp:ListItem Text="— All Cities —" Value=""/>
        </asp:DropDownList>
    </div>
</div>

<asp:Panel ID="pnlProductResults" runat="server" Visible="false">
<div class="card">
    <div class="card-title">Product Performance — <asp:Label ID="lblProductScope" runat="server"/></div>
    <div style="height:400px;margin-bottom:16px;"><canvas id="chartProductMonthly"></canvas></div>
    <div class="table-scroll">
        <asp:Literal ID="litProductTable" runat="server"/>
    </div>
</div>

<div class="card">
    <div class="card-title">Product Summary</div>
    <div class="table-scroll">
        <asp:Literal ID="litProductSummary" runat="server"/>
    </div>
</div>
</asp:Panel>
</asp:Panel>

<!-- ═══════════════════════════════════════════════════ -->
<!-- SECTION 3: DISTRIBUTOR PERFORMANCE                 -->
<!-- ═══════════════════════════════════════════════════ -->
<asp:Panel ID="pnlSection3" runat="server" Visible="false">
<div class="card">
    <div class="card-title">Distributor Performance</div>
    <div class="card-sub">Sales performance, product mix, and repeat order metrics by state</div>
    <div class="filter-row">
        <label>State</label>
        <asp:DropDownList ID="ddlS3State" runat="server" CssClass="filter-select" AutoPostBack="true" OnSelectedIndexChanged="ddlS3State_Changed"/>
    </div>
</div>

<asp:Panel ID="pnlDistResults" runat="server" Visible="false">
<div class="card">
    <div class="card-title">Distributor Ranking — <asp:Label ID="lblDistScope" runat="server"/></div>
    <div style="height:400px;margin-bottom:16px;"><canvas id="chartDistributors"></canvas></div>
</div>

<div class="card">
    <div class="card-title">Distributor Details</div>
    <div class="table-scroll">
        <asp:Literal ID="litDistTable" runat="server"/>
    </div>
</div>

<div class="card">
    <div class="card-title">&#x1F50D; Distributor Deep-Dive</div>
    <div class="card-sub">Select a distributor to see their product mix and order history</div>
    <div class="filter-row">
        <label>Distributor</label>
        <asp:DropDownList ID="ddlDistributor" runat="server" CssClass="filter-select" AutoPostBack="true" OnSelectedIndexChanged="ddlDistributor_Changed"/>
    </div>
    <asp:Panel ID="pnlDistDetail" runat="server" Visible="false">
        <div class="chart-row">
            <div class="chart-box">
                <div class="chart-title">Monthly Sales Trend</div>
                <canvas id="chartDistMonthly"></canvas>
            </div>
            <div class="chart-box">
                <div class="chart-title">Product Mix</div>
                <canvas id="chartDistProducts"></canvas>
            </div>
        </div>
        <div class="table-scroll">
            <asp:Literal ID="litDistProducts" runat="server"/>
        </div>
    </asp:Panel>
</div>
</asp:Panel>
</asp:Panel>

</div><!-- container -->

<script>
// Chart.js rendering — reads data from hidden fields
var chartColors = ['#8e44ad','#1a9e6a','#e67e22','#2980b9','#e74c3c','#2c3e50','#f39c12','#27ae60','#8e44ad','#16a085','#d35400','#c0392b','#7f8c8d','#2ecc71','#3498db'];
Chart.defaults.font.family = "'DM Sans', sans-serif";
Chart.defaults.font.size = 11;

function formatLakh(v) {
    if (v >= 10000000) return '₹' + (v/10000000).toFixed(1) + 'Cr';
    if (v >= 100000) return '₹' + (v/100000).toFixed(1) + 'L';
    if (v >= 1000) return '₹' + (v/1000).toFixed(1) + 'K';
    return '₹' + v.toFixed(0);
}

function renderChart(canvasId, type, labels, datasets, opts) {
    var ctx = document.getElementById(canvasId);
    if (!ctx) return;
    if (ctx._chartInstance) ctx._chartInstance.destroy();
    var cfg = {
        type: type,
        data: { labels: labels, datasets: datasets },
        options: Object.assign({
            responsive: true, maintainAspectRatio: false,
            plugins: { legend: { position: 'bottom', labels: { boxWidth: 12, padding: 10, font: { size: 10 } } } },
            scales: { y: { ticks: { callback: function(v) { return formatLakh(v); } } } }
        }, opts || {})
    };
    ctx._chartInstance = new Chart(ctx, cfg);
}

function buildCharts() {
    // Section 1: State monthly
    var d1 = document.getElementById('<%= hfStateMonthlyData.ClientID %>');
    if (d1 && d1.value) {
        try {
            var sd = JSON.parse(d1.value);
            renderChart('chartStateMonthly', 'bar', sd.labels, sd.datasets.map(function(ds, i) {
                return { label: ds.label, data: ds.data, backgroundColor: chartColors[i % chartColors.length] + 'cc', borderColor: chartColors[i % chartColors.length], borderWidth: 1 };
            }));
        } catch(e) { console.error('State chart error:', e); }
    }

    // Section 1: City monthly
    var d2 = document.getElementById('<%= hfCityMonthlyData.ClientID %>');
    if (d2 && d2.value) {
        try {
            var cd = JSON.parse(d2.value);
            renderChart('chartCityMonthly', 'bar', cd.labels, cd.datasets.map(function(ds, i) {
                return { label: ds.label, data: ds.data, backgroundColor: chartColors[i % chartColors.length] + 'cc', borderColor: chartColors[i % chartColors.length], borderWidth: 1 };
            }), { plugins: { legend: { display: cd.datasets.length <= 15 } } });
        } catch(e) { console.error('City chart error:', e); }
    }

    // Section 2: Product monthly
    var d3 = document.getElementById('<%= hfProductMonthlyData.ClientID %>');
    if (d3 && d3.value) {
        try {
            var pd = JSON.parse(d3.value);
            renderChart('chartProductMonthly', 'line', pd.labels, pd.datasets.map(function(ds, i) {
                return { label: ds.label, data: ds.data, borderColor: chartColors[i % chartColors.length], backgroundColor: chartColors[i % chartColors.length] + '33', fill: false, tension: .3, pointRadius: 3 };
            }));
        } catch(e) { console.error('Product chart error:', e); }
    }

    // Section 3: Distributor bar + deep-dive
    var d4 = document.getElementById('<%= hfDistributorChartData.ClientID %>');
    if (d4 && d4.value) {
        try {
            var dd = JSON.parse(d4.value);
            if (dd.ranking) {
                renderChart('chartDistributors', 'bar', dd.ranking.labels, [{
                    label: 'Total Sales', data: dd.ranking.values,
                    backgroundColor: dd.ranking.values.map(function(v, i) { return chartColors[i % chartColors.length] + 'cc'; })
                }], { indexAxis: 'y', plugins: { legend: { display: false } },
                     scales: { x: { ticks: { callback: function(v) { return formatLakh(v); } } } } });
            }
            if (dd.monthly) {
                renderChart('chartDistMonthly', 'bar', dd.monthly.labels, [{
                    label: 'Sales', data: dd.monthly.values,
                    backgroundColor: '#8e44adcc', borderColor: '#8e44ad', borderWidth: 1
                }]);
            }
            if (dd.products) {
                renderChart('chartDistProducts', 'doughnut', dd.products.labels, [{
                    data: dd.products.values,
                    backgroundColor: dd.products.labels.map(function(l, i) { return chartColors[i % chartColors.length]; })
                }], { scales: {} });
            }
        } catch(e) { console.error('Dist chart error:', e); }
    }
}

// Render after page load (including async postback)
if (typeof Sys !== 'undefined' && Sys.WebForms) {
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(buildCharts);
}
window.addEventListener('load', buildCharts);
</script>
<script src="/StockApp/erp-keepalive.js"></script>
</form>
</body>
</html>
