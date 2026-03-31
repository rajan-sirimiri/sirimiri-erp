<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ERPHome.aspx.cs" Inherits="StockApp.ERPHome" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Sirimiri Nutrition — ERP</title>
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:ital,wght@0,300;0,400;0,500;0,600;1,300&display=swap" rel="stylesheet" />
    <style>
        :root {
            --bg:           #f5f5f5;
            --surface:      #ffffff;
            --surface2:     #f9f9f9;
            --border:       #e0e0e0;
            --accent:       #cc1e1e;
            --accent-dark:  #a81818;
            --accent-glow:  rgba(204,30,30,0.12);
            --accent-soft:  rgba(204,30,30,0.06);
            --text:         #1a1a1a;
            --text-muted:   #666666;
            --text-dim:     #999999;
            --gold:         #b8860b;
            --radius:       14px;
        }

        *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

        body {
            background: var(--bg);
            color: var(--text);
            font-family: 'DM Sans', sans-serif;
            min-height: 100vh;
            overflow-x: hidden;
        }

        /* ── BACKGROUND TEXTURE ── */
        body::before {
            content: '';
            position: fixed; inset: 0; z-index: 0;
            background-image:
                linear-gradient(var(--border) 1px, transparent 1px),
                linear-gradient(90deg, var(--border) 1px, transparent 1px);
            background-size: 60px 60px;
            opacity: 0.06;
            pointer-events: none;
        }

        /* Radial glow behind modules */
        body::after {
            content: '';
            position: fixed;
            top: 50%; left: 50%;
            transform: translate(-50%, -50%);
            width: 900px; height: 600px;
            background: radial-gradient(ellipse, rgba(204,30,30,0.04) 0%, transparent 70%);
            pointer-events: none; z-index: 0;
        }

        /* ── HEADER ── */
        header {
            position: relative; z-index: 10;
            display: flex; align-items: center;
            padding: 0 40px;
            height: 80px;
            background: #ffffff;
            border-bottom: 2px solid var(--accent);
        }

        .header-logo {
            display: flex; align-items: center; gap: 14px;
            flex-shrink: 0;
        }
        .header-logo img {
            height: 52px; width: auto;
            object-fit: contain;
            filter: drop-shadow(0 2px 12px rgba(204,30,30,0.35));
        }
        .header-logo-fallback {
            width: 52px; height: 52px;
            background: linear-gradient(135deg, var(--accent-dark), var(--accent));
            border-radius: 10px;
            display: flex; align-items: center; justify-content: center;
            font-family: 'Bebas Neue', sans-serif;
            font-size: 22px; color: #fff;
            letter-spacing: .05em;
        }

        .header-center {
            flex: 1; text-align: center;
        }
        .header-brand {
            font-family: 'Bebas Neue', sans-serif;
            font-size: 26px;
            letter-spacing: .10em;
            color: var(--text);
            line-height: 1;
        }
        .header-brand span {
            color: var(--accent);
        }
        .header-tagline {
            font-size: 10px;
            letter-spacing: .22em;
            text-transform: uppercase;
            color: var(--text-muted);
            margin-top: 3px;
        }

        .header-right {
            flex-shrink: 0;
            display: flex; align-items: center; gap: 20px;
        }
        .header-user {
            text-align: right;
        }
        .header-user-name {
            font-size: 13px; font-weight: 600; color: var(--text);
        }
        .header-user-role {
            font-size: 11px; color: var(--text-muted);
            text-transform: uppercase; letter-spacing: .06em;
        }
        .btn-signout {
            padding: 7px 16px;
            border: 1.5px solid var(--border);
            border-radius: 8px;
            color: var(--text-muted);
            font-size: 12px; font-weight: 700;
            text-decoration: none;
            letter-spacing: .04em;
            text-transform: uppercase;
            transition: all .2s;
            white-space: nowrap;
        }
        .btn-signout:hover {
            border-color: var(--accent);
            color: var(--accent);
        }

        /* ── RED ACCENT LINE ── */
        .accent-bar {
            height: 3px;
            background: linear-gradient(90deg, transparent, var(--accent), var(--accent-dark), transparent);
            position: relative; z-index: 10;
        }

        /* ── MAIN CONTENT ── */
        main {
            position: relative; z-index: 1;
            max-width: 1100px;
            margin: 0 auto;
            padding: 60px 32px 80px;
        }

        .section-label {
            font-size: 11px; font-weight: 600;
            text-transform: uppercase; letter-spacing: .22em;
            color: var(--text-dim);
            text-align: center;
            margin-bottom: 10px;
        }
        .section-title {
            font-family: 'Bebas Neue', sans-serif;
            font-size: 36px; letter-spacing: .08em;
            text-align: center;
            color: var(--text);
            margin-bottom: 6px;
        }
        .section-title span { color: var(--accent); }
        .section-sub {
            text-align: center;
            font-size: 14px; color: var(--text-muted);
            font-style: italic; font-weight: 300;
            margin-bottom: 56px;
        }

        /* ── MODULE GRID ── */
        .module-grid {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 20px;
        }

        .module-card {
            position: relative;
            background: var(--surface);
            border: 1px solid var(--border);
            border-radius: var(--radius);
            padding: 36px 28px 32px;
            cursor: pointer;
            text-decoration: none;
            display: flex;
            flex-direction: column;
            align-items: flex-start;
            gap: 16px;
            overflow: hidden;
            transition: border-color .3s, transform .3s, box-shadow .3s;
            animation: cardIn .5s ease both;
            box-shadow: 0 2px 8px rgba(0,0,0,0.06);
        }

        .module-card:nth-child(1) { animation-delay: .05s; }
        .module-card:nth-child(2) { animation-delay: .10s; }
        .module-card:nth-child(3) { animation-delay: .15s; }
        .module-card:nth-child(4) { animation-delay: .20s; }
        .module-card:nth-child(5) { animation-delay: .25s; }
        .module-card:nth-child(6) { animation-delay: .30s; }

        @keyframes cardIn {
            from { opacity: 0; transform: translateY(24px); }
            to   { opacity: 1; transform: translateY(0); }
        }

        /* Top corner glow */
        .module-card::before {
            content: '';
            position: absolute; top: 0; right: 0;
            width: 120px; height: 120px;
            border-radius: 0 var(--radius) 0 120px;
            opacity: 0;
            transition: opacity .3s;
        }

        .module-card:hover {
            border-color: var(--accent);
            transform: translateY(-4px);
            box-shadow: 0 12px 32px rgba(0,0,0,0.12), 0 0 0 1px var(--accent);
        }
        .module-card:hover::before { opacity: 1; }

        /* Bottom accent line on hover */
        .module-card::after {
            content: '';
            position: absolute; bottom: 0; left: 0; right: 0;
            height: 2px;
            transform: scaleX(0);
            transform-origin: left;
            transition: transform .3s ease;
        }
        .module-card:hover::after { transform: scaleX(1); }

        /* ── MODULE ICON ── */
        .module-icon-wrap {
            width: 58px; height: 58px;
            border-radius: 14px;
            display: flex; align-items: center; justify-content: center;
            font-size: 26px;
            position: relative; z-index: 1;
            transition: transform .3s;
            flex-shrink: 0;
        }
        .module-card:hover .module-icon-wrap {
            transform: scale(1.08);
        }

        .module-info { position: relative; z-index: 1; }
        .module-name {
            font-family: 'Bebas Neue', sans-serif;
            font-size: 22px; letter-spacing: .07em;
            color: var(--text); line-height: 1.1;
            margin-bottom: 6px;
            transition: color .3s;
        }
        .module-desc {
            font-size: 12px; color: var(--text-muted);
            font-weight: 400; line-height: 1.5;
        }

        .module-arrow {
            position: absolute; bottom: 20px; right: 22px;
            font-size: 18px; color: #cccccc;
            transition: color .3s, transform .3s;
        }
        .module-card:hover .module-arrow {
            color: var(--accent);
            transform: translate(3px, -3px);
        }

        /* ── MODULE COLOUR THEMES ── */
        /* 1. Materials Management — amber */
        .mod-materials .module-icon-wrap { background: rgba(201,168,76,0.12); color: var(--gold); }
        .mod-materials::before           { background: radial-gradient(circle, rgba(201,168,76,0.08), transparent); }
        .mod-materials::after            { background: var(--gold); }

        /* 2. Production — blue */
        .mod-production .module-icon-wrap { background: rgba(30,120,204,0.12); color: #4a9eff; }
        .mod-production::before           { background: radial-gradient(circle, rgba(30,120,204,0.08), transparent); }
        .mod-production::after            { background: #1e78cc; }

        /* 3. Packing — teal */
        .mod-packing .module-icon-wrap { background: rgba(26,158,106,0.12); color: #2dcb82; }
        .mod-packing::before           { background: radial-gradient(circle, rgba(26,158,106,0.08), transparent); }
        .mod-packing::after            { background: #1a9e6a; }

        /* 4. Sales BI — red (brand) */
        .mod-sales .module-icon-wrap { background: rgba(204,30,30,0.12); color: #ff5555; }
        .mod-sales::before           { background: radial-gradient(circle, rgba(204,30,30,0.10), transparent); }
        .mod-sales::after            { background: var(--accent); }

        /* 5. BI & Analytics — purple */
        .mod-bi .module-icon-wrap { background: rgba(140,80,210,0.12); color: #b07aff; }
        .mod-bi::before           { background: radial-gradient(circle, rgba(140,80,210,0.08), transparent); }
        .mod-bi::after            { background: #8c50d2; }

        /* 6. Finance — emerald */
        .mod-finance .module-icon-wrap { background: rgba(16,185,129,0.12); color: #34d399; }
        .mod-finance::before           { background: radial-gradient(circle, rgba(16,185,129,0.08), transparent); }
        .mod-finance::after            { background: #10b981; }

        /* ── STATUS PILL ── */
        .module-status {
            position: absolute; top: 16px; right: 16px;
            font-size: 10px; font-weight: 700;
            letter-spacing: .08em; text-transform: uppercase;
            padding: 3px 10px; border-radius: 20px;
        }
        .status-live    { background: rgba(26,158,106,0.15); color: #2dcb82; }
        .status-soon    { background: rgba(201,168,76,0.12); color: var(--gold); }
        .status-planned { background: rgba(0,0,0,0.05); color: var(--text-dim); }

        /* ── FOOTER ── */
        footer {
            position: relative; z-index: 1;
            text-align: center;
            padding: 24px;
            border-top: 1px solid var(--border);
            font-size: 12px; color: var(--text-muted);
            letter-spacing: .06em;
        }

        @media (max-width: 768px) {
            .module-grid { grid-template-columns: repeat(2, 1fr); }
            main { padding: 40px 20px 60px; }
            header { padding: 0 20px; }
            .header-brand { font-size: 22px; }
        }
        @media (max-width: 480px) {
            .module-grid { grid-template-columns: 1fr; }
        }
    </style>
</head>
<body>

    <form id="form1" runat="server">
    <asp:HiddenField ID="hfSSOToken" runat="server" Value=""/>

    <!-- HEADER -->
    <header>
        <div class="header-logo">
            <img src="Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri Nutrition"
                 onerror="this.style.display='none';document.getElementById('logoFallback').style.display='flex';" />
            <div id="logoFallback" class="header-logo-fallback" style="display:none;">SN</div>
        </div>

        <div class="header-center">
            <div class="header-brand">Sirimiri Nutrition Food Products</div>
            <div class="header-tagline">Enterprise Resource Planning</div>
        </div>

        <div class="header-right">
            <div class="header-user">
                <div class="header-user-name">
                    <asp:Label ID="lblUserName" runat="server" />
                </div>
                <div class="header-user-role">
                    <asp:Label ID="lblUserRole" runat="server" />
                </div>
            </div>
            <a href="Logout.aspx" class="btn-signout"
               onclick="return confirm('Sign out?')">&#x2192; Sign Out</a>
        </div>
    </header>


    <!-- MAIN -->
    <main>
        <div class="section-label">Enterprise Resource Planning</div>
        <div class="section-title">End To End <span>Operations Management</span> System</div>
        <div class="section-sub">All operations in one place</div>

        <div class="module-grid">

            <!-- 1. Materials Management -->
            <a href="/MM/MMLogin.aspx" data-sso-module="MM" class="module-card mod-materials">
                <span class="module-status status-live">Live</span>
                <div class="module-icon-wrap">&#x1F4E6;</div>
                <div class="module-info">
                    <div class="module-name">Materials<br/>Management</div>
                    <div class="module-desc">Raw material procurement, inventory tracking, supplier management &amp; GRN</div>
                </div>
                <div class="module-arrow">&#x2197;</div>
            </a>

            <!-- 2. Production -->
            <a href="/PP/PPLogin.aspx" data-sso-module="PP" class="module-card mod-production">
                <span class="module-status status-live">Live</span>
                <div class="module-icon-wrap">&#x2699;&#xFE0F;</div>
                <div class="module-info">
                    <div class="module-name">Production</div>
                    <div class="module-desc">Batch planning, work orders, BOM management &amp; production tracking</div>
                </div>
                <div class="module-arrow">&#x2197;</div>
            </a>

            <!-- 3. Packing -->
            <a href="/PK/PKLogin.aspx" data-sso-module="PK" class="module-card mod-packing">
                <span class="module-status status-live">Live</span>
                <div class="module-icon-wrap">&#x1F3F7;&#xFE0F;</div>
                <div class="module-info">
                    <div class="module-name">Packing &amp;<br/>Shipments</div>
                    <div class="module-desc">Primary &amp; secondary packing, customer orders and dispatch</div>
                </div>
                <div class="module-arrow">&#x2197;</div>
            </a>

            <!-- 4. Sales BI -->
            <a href="SAHome.aspx" class="module-card mod-sales">
                <span class="module-status status-live">Live</span>
                <div class="module-icon-wrap">&#x1F4C8;</div>
                <div class="module-info">
                    <div class="module-name">Sales &amp;<br/>Distribution</div>
                    <div class="module-desc">Distributor stock positions, daily sales entry &amp; movement reports</div>
                </div>
                <div class="module-arrow">&#x2197;</div>
            </a>

            <!-- 5. BI & Analytics -->
            <a href="Reports.aspx" class="module-card mod-bi">
                <span class="module-status status-live">Live</span>
                <div class="module-icon-wrap">&#x1F9E0;</div>
                <div class="module-info">
                    <div class="module-name">BI &amp;<br/>Analytics</div>
                    <div class="module-desc">Stock movement reports, daily sales analytics &amp; performance dashboards</div>
                </div>
                <div class="module-arrow">&#x2197;</div>
            </a>

            <!-- 6. Finance -->
            <div class="module-card mod-finance" style="opacity:.5;cursor:default;pointer-events:none;">
                <div class="module-icon-wrap">&#x1F4B0;</div>
                <div class="module-info">
                    <div class="module-name">Finance</div>
                    <div class="module-desc">Accounts receivable, credit tracking, payment reconciliation &amp; P&amp;L</div>
                </div>
                <div class="module-arrow">&#x2197;</div>
            </div>

        </div>
    </main>

    <footer>
        &copy; <%= DateTime.Now.Year %> Sirimiri Nutrition &nbsp;&mdash;&nbsp; ERP v2.0
    </footer>

    <script type="text/javascript">
        // SSO: append token to module links on page load
        (function () {
            var tokenField = document.getElementById('<%= hfSSOToken.ClientID %>');
            var token = tokenField ? tokenField.value : '';
            if (!token) return;

            var links = document.querySelectorAll('a[data-sso-module]');
            for (var i = 0; i < links.length; i++) {
                var href = links[i].getAttribute('href');
                var sep = href.indexOf('?') >= 0 ? '&' : '?';
                links[i].setAttribute('href', href + sep + 'sso=' + token);
            }
        })();
    </script>

    </form>
</body>
</html>
