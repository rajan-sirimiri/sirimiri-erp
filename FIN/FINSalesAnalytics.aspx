<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FINSalesAnalytics.aspx.cs" Inherits="FINApp.FINSalesAnalytics" EnableEventValidation="false" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri — Sales Analytics</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600;700&family=JetBrains+Mono:wght@400;700&display=swap" rel="stylesheet"/>
<script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/4.4.1/chart.umd.min.js"></script>
<style>
:root {
    --ink: #0f0f0f;
    --paper: #f7f5f2;
    --smoke: #e8e5e0;
    --dim: #9a9590;
    --ruby: #cc1e1e;
    --emerald: #1a9e6a;
    --sapphire: #1e5fcc;
    --amber: #d68b00;
    --plum: #7c3aed;
    --surface: #fff;
    --mono: 'JetBrains Mono', monospace;
    --sans: 'DM Sans', sans-serif;
    --display: 'Bebas Neue', sans-serif;
    --radius: 10px;
}
*, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
body { font-family: var(--sans); background: var(--paper); color: var(--ink); -webkit-font-smoothing: antialiased; }

/* ── NAV ── */
.top-bar { background: var(--ink); padding: 12px 24px; display: flex; align-items: center; gap: 16px; }
.top-bar img { height: 28px; background: #fff; border-radius: 4px; padding: 2px 6px; }
.top-bar h1 { color: #fff; font-family: var(--display); font-size: 18px; letter-spacing: .1em; flex: 1; }
.top-bar a { color: rgba(255,255,255,.7); font-size: 12px; font-weight: 600; text-decoration: none; }
.top-bar a:hover { color: #fff; }

/* ── LAYOUT ── */
.page { max-width: 1280px; margin: 0 auto; padding: 28px 24px 80px; }

/* ── KPI STRIP ── */
.kpi-strip { display: flex; gap: 12px; margin-bottom: 28px; flex-wrap: wrap; }
.kpi { flex: 1; min-width: 150px; background: var(--surface); border-radius: var(--radius); padding: 18px 20px; border-left: 4px solid var(--smoke); }
.kpi.accent { border-left-color: var(--ruby); }
.kpi.green { border-left-color: var(--emerald); }
.kpi.blue { border-left-color: var(--sapphire); }
.kpi.amber { border-left-color: var(--amber); }
.kpi-val { font-family: var(--display); font-size: 30px; line-height: 1; letter-spacing: .03em; }
.kpi-label { font-size: 10px; font-weight: 700; letter-spacing: .1em; text-transform: uppercase; color: var(--dim); margin-top: 4px; }
.kpi-delta { font-size: 11px; font-weight: 700; margin-top: 4px; }
.kpi-delta.up { color: var(--emerald); }
.kpi-delta.down { color: var(--ruby); }

/* ── SECTION HEADERS ── */
.section-head { font-family: var(--display); font-size: 22px; letter-spacing: .08em; margin: 32px 0 14px; padding-bottom: 6px; border-bottom: 3px solid var(--ink); display: flex; align-items: baseline; gap: 12px; }
.section-head .badge { font-family: var(--sans); font-size: 10px; font-weight: 700; background: var(--ruby); color: #fff; padding: 3px 10px; border-radius: 20px; letter-spacing: .04em; }

/* ── CARDS ── */
.card { background: var(--surface); border-radius: var(--radius); padding: 22px; margin-bottom: 18px; box-shadow: 0 1px 4px rgba(0,0,0,.06); }
.card-head { font-family: var(--display); font-size: 16px; letter-spacing: .06em; color: var(--dim); margin-bottom: 12px; }
.card-row { display: grid; grid-template-columns: 1fr 1fr; gap: 18px; margin-bottom: 18px; }

/* ── FILTER BAR ── */
.filter-bar { display: flex; gap: 10px; align-items: center; flex-wrap: wrap; margin-bottom: 18px; }
.filter-bar label { font-size: 10px; font-weight: 700; letter-spacing: .08em; text-transform: uppercase; color: var(--dim); }
.filter-bar select { padding: 8px 14px; border: 1.5px solid var(--smoke); border-radius: 8px; font-family: var(--sans); font-size: 13px; background: #fff; min-width: 180px; cursor: pointer; }
.filter-bar select:focus { border-color: var(--ruby); outline: none; }

/* ── TABLES ── */
.tbl-wrap { overflow: auto; max-height: 520px; border: 1px solid var(--smoke); border-radius: 8px; }
table.dt { width: 100%; border-collapse: collapse; font-size: 11px; }
table.dt th { font-size: 9px; font-weight: 700; letter-spacing: .1em; text-transform: uppercase; color: var(--dim); padding: 10px 8px; text-align: left; border-bottom: 2px solid var(--smoke); background: #faf9f7; position: sticky; top: 0; z-index: 1; }
table.dt th.num, table.dt td.num { text-align: right; }
table.dt td { padding: 8px; border-bottom: 1px solid #f2f0ed; }
table.dt tr:hover { background: #f9f7f4; }
table.dt .mono { font-family: var(--mono); font-size: 10px; }
table.dt .bar-cell { position: relative; }
table.dt .bar-bg { position: absolute; left: 0; top: 0; bottom: 0; background: var(--ruby); opacity: .08; border-radius: 0 4px 4px 0; }
.growth-up { color: var(--emerald); font-weight: 700; }
.growth-down { color: var(--ruby); font-weight: 700; }
.tag { display: inline-block; font-size: 9px; font-weight: 700; letter-spacing: .04em; padding: 2px 7px; border-radius: 4px; }
.tag-di { background: #eafaf1; color: var(--emerald); }
.tag-st { background: #ebf5fb; color: var(--sapphire); }

/* ── ALERTS ── */
.alert-card { background: #fff8f0; border: 1.5px solid #ffd6a0; border-radius: var(--radius); padding: 16px 20px; margin-bottom: 18px; }
.alert-title { font-family: var(--display); font-size: 14px; letter-spacing: .06em; color: var(--amber); margin-bottom: 8px; }
.alert-item { display: flex; align-items: center; gap: 10px; padding: 6px 0; border-bottom: 1px solid #f5efe5; font-size: 12px; }
.alert-item:last-child { border: none; }
.alert-days { font-family: var(--mono); font-size: 11px; font-weight: 700; color: var(--ruby); min-width: 50px; }

/* ── DETAIL DRAWER ── */
.drawer { background: #faf9f7; border: 1.5px solid var(--smoke); border-radius: var(--radius); padding: 20px; margin-top: 14px; }
.drawer-title { font-family: var(--display); font-size: 18px; letter-spacing: .06em; margin-bottom: 12px; }

#loading { text-align: center; padding: 60px; color: var(--dim); font-size: 14px; }

@media(max-width: 768px) {
    .card-row { grid-template-columns: 1fr; }
    .kpi-strip { flex-direction: column; }
    .kpi { min-width: 100%; }
}
</style>
</head>
<body>
<form id="form1" runat="server">
<div class="top-bar">
    <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    <h1>Sales Analytics</h1>
    <a href="FINHome.aspx">&#8592; FIN Home</a>
    <a href="FINLogout.aspx">Sign Out</a>
</div>

<div class="page" id="app">
    <div id="loading">Loading dashboard data...</div>
</div>

<script>
(function() {
    var API = 'FINAnalyticsAPI.ashx';
    var COLORS = ['#cc1e1e','#1a9e6a','#1e5fcc','#d68b00','#7c3aed','#e67e22','#16a085','#8e44ad','#2c3e50','#f39c12','#c0392b','#27ae60','#3498db','#d35400','#7f8c8d'];
    var app = document.getElementById('app');

    function q(action, params) {
        var url = API + '?action=' + action;
        if (params) for (var k in params) url += '&' + k + '=' + encodeURIComponent(params[k]);
        return fetch(url).then(function(r) { return r.json(); });
    }

    function fmt(v) {
        if (v >= 10000000) return '\u20B9' + (v/10000000).toFixed(1) + 'Cr';
        if (v >= 100000) return '\u20B9' + (v/100000).toFixed(1) + 'L';
        if (v >= 1000) return '\u20B9' + (v/1000).toFixed(1) + 'K';
        return '\u20B9' + Math.round(v).toLocaleString('en-IN');
    }

    function fmtMonth(ym) {
        var p = ym.split('-');
        var m = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
        return m[parseInt(p[1])-1] + ' ' + p[0].substring(2);
    }

    function pctChange(curr, prev) {
        if (!prev || prev === 0) return '';
        var pct = ((curr - prev) / prev * 100).toFixed(0);
        var cls = pct >= 0 ? 'growth-up' : 'growth-down';
        var arrow = pct >= 0 ? '\u25B2' : '\u25BC';
        return '<span class="' + cls + '">' + arrow + ' ' + Math.abs(pct) + '%</span>';
    }

    function makeChart(id, type, labels, datasets, opts) {
        var el = document.getElementById(id);
        if (!el) return;
        if (el._ci) el._ci.destroy();
        var cfg = {
            type: type,
            data: { labels: labels, datasets: datasets },
            options: Object.assign({
                responsive: true, maintainAspectRatio: false,
                plugins: { legend: { position: 'bottom', labels: { boxWidth: 10, padding: 8, font: { family: "'DM Sans'", size: 10 } } } },
                scales: type === 'doughnut' || type === 'pie' ? {} : { y: { ticks: { callback: function(v) { return fmt(v); }, font: { family: "'JetBrains Mono'", size: 9 } }, grid: { color: '#f0ede8' } }, x: { ticks: { font: { size: 9 } }, grid: { display: false } } }
            }, opts || {})
        };
        el._ci = new Chart(el, cfg);
    }

    // ── Load everything ──
    Promise.all([
        q('overview'), q('monthlyTrend'), q('stateBreakdown'),
        q('topProducts'), q('alerts')
    ]).then(function(results) {
        var ov = results[0], trend = results[1], states = results[2],
            products = results[3], alerts = results[4];
        render(ov, trend, states, products, alerts);
    }).catch(function(err) {
        app.innerHTML = '<div style="color:red;padding:40px;">Error loading data: ' + err.message + '</div>';
    });

    function render(ov, trend, states, products, alerts) {
        var growthCls = ov.growthPct >= 0 ? 'up' : 'down';
        var growthArrow = ov.growthPct >= 0 ? '\u25B2' : '\u25BC';

        var html = '';

        // ── KPI STRIP ──
        html += '<div class="kpi-strip">';
        html += '<div class="kpi accent"><div class="kpi-val">' + fmt(ov.totalSales) + '</div><div class="kpi-label">Total Revenue</div></div>';
        html += '<div class="kpi green"><div class="kpi-val">' + fmt(ov.thisMonth) + '</div><div class="kpi-label">This Month</div><div class="kpi-delta ' + growthCls + '">' + growthArrow + ' ' + Math.abs(ov.growthPct).toFixed(0) + '% vs prev month</div></div>';
        html += '<div class="kpi blue"><div class="kpi-val">' + ov.totalInvoices.toLocaleString() + '</div><div class="kpi-label">Invoices</div></div>';
        html += '<div class="kpi amber"><div class="kpi-val">' + ov.totalCustomers + '</div><div class="kpi-label">Customers</div></div>';
        html += '<div class="kpi"><div class="kpi-val">' + ov.totalStates + '</div><div class="kpi-label">States</div></div>';
        html += '</div>';

        // ── REVENUE TREND ──
        html += '<div class="section-head">Revenue Trend <span class="badge">' + ov.monthCount + ' MONTHS</span></div>';
        html += '<div class="card"><div style="height:300px;"><canvas id="cTrend"></canvas></div></div>';

        // ── STATE PERFORMANCE ──
        html += '<div class="section-head">State Performance</div>';
        html += '<div class="card-row">';
        html += '<div class="card"><div class="card-head">Revenue by State</div><div style="height:280px;"><canvas id="cStatePie"></canvas></div></div>';
        html += '<div class="card"><div class="card-head">Monthly by State</div><div style="height:280px;"><canvas id="cStateBar"></canvas></div></div>';
        html += '</div>';

        // State table
        html += '<div class="card"><div class="card-head">State × Month Breakdown</div><div class="tbl-wrap">';
        html += buildPivotTable(states.states, states.months, 'State');
        html += '</div></div>';

        // Drill-down selector
        html += '<div class="card"><div class="card-head">Drill Down by State → City</div>';
        html += '<div class="filter-bar"><label>State</label><select id="selState1" onchange="window._loadCities()"><option value="">— Select —</option>';
        states.states.forEach(function(s) { html += '<option value="' + s.name + '">' + s.name + ' (' + fmt(s.total) + ')</option>'; });
        html += '</select></div><div id="cityContent"></div></div>';

        // ── TOP PRODUCTS ──
        html += '<div class="section-head">Product Performance</div>';
        html += '<div class="card-row">';
        html += '<div class="card"><div class="card-head">Top Products by Revenue</div><div style="height:320px;"><canvas id="cProdBar"></canvas></div></div>';
        html += '<div class="card"><div class="card-head">Product Mix</div><div style="height:320px;"><canvas id="cProdPie"></canvas></div></div>';
        html += '</div>';

        // Product table
        html += '<div class="card"><div class="card-head">Product Details</div><div class="tbl-wrap"><table class="dt"><thead><tr>';
        html += '<th>#</th><th>Product</th><th class="num">Revenue</th><th class="num">Qty</th><th class="num">Invoices</th><th class="num">Customers</th><th class="num">Share</th>';
        html += '</tr></thead><tbody>';
        var prodTotal = products.reduce(function(s, p) { return s + p.sales; }, 0);
        products.forEach(function(p, i) {
            var pct = prodTotal > 0 ? (p.sales / prodTotal * 100).toFixed(1) : '0';
            var barW = prodTotal > 0 ? (p.sales / prodTotal * 100).toFixed(0) : 0;
            html += '<tr><td>' + (i+1) + '</td><td class="bar-cell"><div class="bar-bg" style="width:' + barW + '%"></div><strong>' + p.name + '</strong></td>';
            html += '<td class="num mono">' + fmt(p.sales) + '</td><td class="num mono">' + Math.round(p.qty).toLocaleString() + '</td>';
            html += '<td class="num">' + p.invoices + '</td><td class="num">' + p.customers + '</td><td class="num">' + pct + '%</td></tr>';
        });
        html += '</tbody></table></div></div>';

        // Product state drill-down
        html += '<div class="card"><div class="card-head">Product Trends by State</div>';
        html += '<div class="filter-bar"><label>State</label><select id="selState2" onchange="window._loadProductTrend()"><option value="">— Select —</option>';
        states.states.forEach(function(s) { html += '<option value="' + s.name + '">' + s.name + '</option>'; });
        html += '</select></div><div id="prodTrendContent"></div></div>';

        // ── DISTRIBUTOR ANALYSIS ──
        html += '<div class="section-head">Distributor Intelligence</div>';

        // Alerts
        if (alerts.silentDistributors && alerts.silentDistributors.length > 0) {
            html += '<div class="alert-card"><div class="alert-title">\u26A0 Silent Distributors — No Orders in 45+ Days</div>';
            alerts.silentDistributors.forEach(function(d) {
                html += '<div class="alert-item"><span class="alert-days">' + d.daysSilent + 'd</span>';
                html += '<span style="flex:1;font-weight:600;">' + d.name + '</span>';
                html += '<span style="color:var(--dim);font-size:11px;">' + d.city + ', ' + d.state + '</span>';
                html += '<span class="mono" style="font-size:10px;">' + fmt(d.totalSales) + ' lifetime</span></div>';
            });
            html += '</div>';
        }

        html += '<div class="card"><div class="card-head">Distributor Performance</div>';
        html += '<div class="filter-bar"><label>State</label><select id="selState3" onchange="window._loadDist()"><option value="">— Select —</option>';
        states.states.forEach(function(s) { html += '<option value="' + s.name + '">' + s.name + '</option>'; });
        html += '</select></div><div id="distContent"></div></div>';

        app.innerHTML = html;

        // ── Render charts ──
        // Trend
        makeChart('cTrend', 'line', trend.map(function(t) { return fmtMonth(t.month); }), [{
            label: 'Revenue', data: trend.map(function(t) { return t.sales; }),
            borderColor: '#cc1e1e', backgroundColor: 'rgba(204,30,30,.08)', fill: true,
            tension: .35, pointRadius: 4, pointBackgroundColor: '#cc1e1e', borderWidth: 2.5
        }]);

        // State pie
        makeChart('cStatePie', 'doughnut',
            states.states.map(function(s) { return s.name; }),
            [{ data: states.states.map(function(s) { return s.total; }),
               backgroundColor: COLORS.slice(0, states.states.length), borderWidth: 0 }],
            { cutout: '55%' });

        // State stacked bar
        makeChart('cStateBar', 'bar', states.months.map(fmtMonth),
            states.states.map(function(s, i) {
                return { label: s.name, data: s.monthly, backgroundColor: COLORS[i % COLORS.length] + 'cc' };
            }), { scales: { x: { stacked: true }, y: { stacked: true, ticks: { callback: function(v) { return fmt(v); } } } } });

        // Product bar
        makeChart('cProdBar', 'bar',
            products.slice(0, 10).map(function(p) { return p.name.length > 25 ? p.name.substring(0, 22) + '...' : p.name; }),
            [{ data: products.slice(0, 10).map(function(p) { return p.sales; }),
               backgroundColor: COLORS.slice(0, 10).map(function(c) { return c + 'cc'; }) }],
            { indexAxis: 'y', plugins: { legend: { display: false } } });

        // Product pie
        var top8 = products.slice(0, 8);
        var otherSales = products.slice(8).reduce(function(s, p) { return s + p.sales; }, 0);
        var pieLabels = top8.map(function(p) { return p.name; });
        var pieData = top8.map(function(p) { return p.sales; });
        if (otherSales > 0) { pieLabels.push('Others'); pieData.push(otherSales); }
        makeChart('cProdPie', 'doughnut', pieLabels,
            [{ data: pieData, backgroundColor: COLORS.slice(0, pieLabels.length), borderWidth: 0 }],
            { cutout: '50%' });
    }

    function buildPivotTable(items, months, label) {
        var h = '<table class="dt"><thead><tr><th>#</th><th>' + label + '</th><th class="num">Total</th>';
        months.forEach(function(m) { h += '<th class="num">' + fmtMonth(m) + '</th>'; });
        h += '<th class="num">Trend</th></tr></thead><tbody>';
        items.forEach(function(item, i) {
            h += '<tr><td>' + (i+1) + '</td><td><strong>' + item.name + '</strong></td>';
            h += '<td class="num mono" style="font-weight:700;">' + fmt(item.total) + '</td>';
            item.monthly.forEach(function(v) { h += '<td class="num mono">' + (v > 0 ? fmt(v) : '\u2014') + '</td>'; });
            var len = item.monthly.length;
            if (len >= 2) h += '<td class="num">' + pctChange(item.monthly[len-1], item.monthly[len-2]) + '</td>';
            else h += '<td></td>';
            h += '</tr>';
        });
        h += '</tbody></table>';
        return h;
    }

    // ── DRILL-DOWN: State → City ──
    window._loadCities = function() {
        var state = document.getElementById('selState1').value;
        var div = document.getElementById('cityContent');
        if (!state) { div.innerHTML = ''; return; }
        div.innerHTML = '<div style="padding:20px;color:var(--dim);">Loading...</div>';
        q('cityBreakdown', { state: state }).then(function(data) {
            var h = '<div style="height:320px;margin-bottom:16px;"><canvas id="cCityBar"></canvas></div>';
            h += '<div class="tbl-wrap">' + buildPivotTable(data.cities, data.months, 'City') + '</div>';
            div.innerHTML = h;
            makeChart('cCityBar', 'bar', data.months.map(fmtMonth),
                data.cities.slice(0, 12).map(function(c, i) {
                    return { label: c.name, data: c.monthly, backgroundColor: COLORS[i % COLORS.length] + 'cc' };
                }), { scales: { x: { stacked: true }, y: { stacked: true, ticks: { callback: function(v) { return fmt(v); } } } } });
        });
    };

    // ── DRILL-DOWN: Product trends by state ──
    window._loadProductTrend = function() {
        var state = document.getElementById('selState2').value;
        var div = document.getElementById('prodTrendContent');
        if (!state) { div.innerHTML = ''; return; }
        div.innerHTML = '<div style="padding:20px;color:var(--dim);">Loading...</div>';
        q('productMix', { state: state }).then(function(data) {
            var h = '<div style="height:340px;margin-bottom:16px;"><canvas id="cProdTrend"></canvas></div>';
            h += '<div class="tbl-wrap">' + buildPivotTable(data.products, data.months, 'Product') + '</div>';
            div.innerHTML = h;
            makeChart('cProdTrend', 'line', data.months.map(fmtMonth),
                data.products.slice(0, 10).map(function(p, i) {
                    return { label: p.name, data: p.monthly, borderColor: COLORS[i], backgroundColor: 'transparent', tension: .3, pointRadius: 3, borderWidth: 2 };
                }));
        });
    };

    // ── DRILL-DOWN: Distributors ──
    window._loadDist = function() {
        var state = document.getElementById('selState3').value;
        var div = document.getElementById('distContent');
        if (!state) { div.innerHTML = ''; return; }
        div.innerHTML = '<div style="padding:20px;color:var(--dim);">Loading...</div>';
        q('distributors', { state: state }).then(function(data) {
            var h = '<div style="height:' + Math.max(300, Math.min(data.length, 20) * 28) + 'px;margin-bottom:18px;"><canvas id="cDistBar"></canvas></div>';

            // Table
            h += '<div class="tbl-wrap"><table class="dt"><thead><tr>';
            h += '<th>#</th><th>Distributor</th><th>Type</th><th>City</th><th class="num">Revenue</th><th class="num">Orders</th><th class="num">Active Months</th><th class="num">Last Order</th><th class="num">Days Ago</th><th class="num">Repeat Rate</th>';
            h += '</tr></thead><tbody>';
            data.forEach(function(d, i) {
                var repeat = 13 > 0 ? (d.activeMonths / 13 * 100).toFixed(0) : 0;
                var repeatCls = repeat >= 70 ? 'growth-up' : repeat < 40 ? 'growth-down' : '';
                var daysCls = d.daysSinceLast > 45 ? 'growth-down' : d.daysSinceLast > 30 ? '' : 'growth-up';
                var typeTag = d.type === 'DI' ? '<span class="tag tag-di">DI</span>' : '<span class="tag tag-st">ST</span>';
                h += '<tr><td>' + (i+1) + '</td>';
                h += '<td><a href="#" onclick="window._loadDistDetail(' + d.id + ',\'' + d.name.replace(/'/g, "\\'") + '\');return false;" style="font-weight:700;color:var(--ink);text-decoration:none;border-bottom:1px dashed var(--dim);">' + d.name + '</a></td>';
                h += '<td>' + typeTag + '</td><td>' + d.city + '</td>';
                h += '<td class="num mono">' + fmt(d.sales) + '</td>';
                h += '<td class="num">' + d.orders + '</td>';
                h += '<td class="num">' + d.activeMonths + '</td>';
                h += '<td class="num">' + (d.lastOrder || '\u2014') + '</td>';
                h += '<td class="num ' + daysCls + '">' + d.daysSinceLast + 'd</td>';
                h += '<td class="num ' + repeatCls + '">' + repeat + '%</td></tr>';
            });
            h += '</tbody></table></div>';
            h += '<div id="distDetailDrawer"></div>';
            div.innerHTML = h;

            // Chart
            var top20 = data.filter(function(d) { return d.sales > 0; }).slice(0, 20);
            makeChart('cDistBar', 'bar',
                top20.map(function(d) { return d.name.length > 30 ? d.name.substring(0, 27) + '...' : d.name; }),
                [{ data: top20.map(function(d) { return d.sales; }),
                   backgroundColor: top20.map(function(d, i) { return COLORS[i % COLORS.length] + 'cc'; }) }],
                { indexAxis: 'y', plugins: { legend: { display: false } } });
        });
    };

    window._loadDistDetail = function(id, name) {
        var div = document.getElementById('distDetailDrawer');
        div.innerHTML = '<div class="drawer"><div style="color:var(--dim);">Loading ' + name + '...</div></div>';
        div.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        q('distDetail', { customerId: id }).then(function(data) {
            var h = '<div class="drawer"><div class="drawer-title">' + name + '</div>';
            h += '<div class="card-row">';
            h += '<div><div class="card-head">Monthly Sales</div><div style="height:220px;"><canvas id="cDistMonthly"></canvas></div></div>';
            h += '<div><div class="card-head">Product Mix</div><div style="height:220px;"><canvas id="cDistProd"></canvas></div></div>';
            h += '</div>';

            // Products table
            h += '<div class="tbl-wrap" style="margin-top:12px;"><table class="dt"><thead><tr>';
            h += '<th>#</th><th>Product</th><th class="num">Revenue</th><th class="num">Qty</th><th class="num">Orders</th>';
            h += '</tr></thead><tbody>';
            data.products.forEach(function(p, i) {
                h += '<tr><td>' + (i+1) + '</td><td><strong>' + p.name + '</strong></td>';
                h += '<td class="num mono">' + fmt(p.sales) + '</td>';
                h += '<td class="num">' + Math.round(p.qty) + '</td><td class="num">' + p.orders + '</td></tr>';
            });
            h += '</tbody></table></div></div>';
            div.innerHTML = h;

            // Charts
            makeChart('cDistMonthly', 'bar',
                data.monthly.map(function(m) { return fmtMonth(m.month); }),
                [{ label: 'Sales', data: data.monthly.map(function(m) { return m.sales; }),
                   backgroundColor: '#cc1e1ecc', borderRadius: 4 }],
                { plugins: { legend: { display: false } } });

            var top6 = data.products.slice(0, 6);
            makeChart('cDistProd', 'doughnut',
                top6.map(function(p) { return p.name; }),
                [{ data: top6.map(function(p) { return p.sales; }),
                   backgroundColor: COLORS.slice(0, 6), borderWidth: 0 }],
                { cutout: '45%' });
        });
    };

})();
</script>
<script src="/StockApp/erp-keepalive.js"></script>
</form>
</body>
</html>
