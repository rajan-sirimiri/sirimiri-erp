<%@ Page Language="C#" AutoEventWireup="true"
         CodeBehind="StockEntry.aspx.cs"
         Inherits="StockApp.StockEntry"
         EnableViewState="true" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Sirimiri — Distributor Stock Position Entry</title>

    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap"
          rel="stylesheet" />

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
            --error:       #cc1e1e;
            --radius:      10px;
        }

        *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

        body {
            background: var(--bg);
            color: var(--text);
            font-family: 'DM Sans', sans-serif;
            font-weight: 400;
            min-height: 100vh;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            padding: 24px;
            overflow-x: hidden;
        }

        body::before {
            content: '';
            position: fixed;
            inset: 0;
            background-image:
                linear-gradient(var(--border) 1px, transparent 1px),
                linear-gradient(90deg, var(--border) 1px, transparent 1px);
            background-size: 48px 48px;
            opacity: 0.06;
            pointer-events: none;
            z-index: 0;
        }

        .page-wrapper {
            position: relative;
            z-index: 1;
            width: 100%;
            max-width: 560px;
            display: flex;
            flex-direction: column;
            align-items: center;
            animation: fadeUp .55s ease both;
        }

        @keyframes fadeUp {
            from { opacity: 0; transform: translateY(28px); }
            to   { opacity: 1; transform: translateY(0); }
        }

        /* ====================================================
           LOGO AREA — with date badge
        ==================================================== */
        .logo-area {
            width: 100%;
            display: flex;
            align-items: center;
            justify-content: space-between;
            padding: 20px 24px 16px;
            background: #ffffff;
            border: 1px solid var(--border);
            border-bottom: none;
            border-radius: var(--radius) var(--radius) 0 0;
        }

        .logo-area img {
            height: 80px;
            width: auto;
            object-fit: contain;
            filter: drop-shadow(0 2px 8px rgba(204,30,30,0.20));
        }

        /* Centre text between logo and date */
        .center-text {
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .bis-label {
            font-family: 'Bebas Neue', cursive;
            font-size: 22px;
            letter-spacing: .12em;
            color: var(--text);
            text-transform: uppercase;
            text-align: center;
            line-height: 1.25;
        }

        /* Date badge on the right side of logo area */
        .date-badge {
            display: flex;
            flex-direction: column;
            align-items: flex-end;
            gap: 2px;
        }

        .date-badge .date-label {
            font-size: 10px;
            font-weight: 600;
            letter-spacing: .15em;
            text-transform: uppercase;
            color: var(--text-muted);
        }

        .date-badge .date-day {
            font-family: 'Bebas Neue', cursive;
            font-size: 32px;
            line-height: 1;
            color: var(--accent);
        }

        .date-badge .date-month-year {
            font-size: 12px;
            font-weight: 600;
            color: var(--text);
            text-transform: uppercase;
            letter-spacing: .08em;
        }

        .date-badge .date-weekday {
            font-size: 11px;
            color: var(--text-muted);
            font-weight: 400;
        }

        .accent-bar {
            width: 100%;
            height: 4px;
            background: linear-gradient(90deg, var(--accent-dark), #e63030, var(--accent-dark));
        }

        /* ====================================================
           NAVBAR  —  dropdown style
        ==================================================== */
        .navbar {
            width: 100%;
            background: var(--accent);
            border: none;
            display: flex;
            align-items: stretch;
            position: relative;
            z-index: 100;
        }

        /* Each top-level item wrapper */
        .nav-group {
            position: relative;
        }

        /* Top-level link */
        .nav-item {
            display: flex;
            align-items: center;
            gap: 6px;
            padding: 13px 20px;
            font-size: 13px;
            font-weight: 500;
            color: rgba(255,255,255,0.90);
            text-decoration: none;
            white-space: nowrap;
            transition: background .2s, color .2s;
            cursor: pointer;
            border: none;
            background: transparent;
            letter-spacing: .01em;
        }

        .nav-item:hover,
        .nav-group:hover > .nav-item {
            background: var(--accent-dark);
            color: #ffffff;
        }

        .nav-item.active {
            background: var(--accent-dark);
            color: #ffffff;
            font-weight: 600;
        }

        /* Chevron arrow for items with dropdown */
        .nav-item .chevron {
            font-size: 9px;
            transition: transform .2s;
            opacity: .75;
        }

        .nav-group:hover > .nav-item .chevron {
            transform: rotate(180deg);
        }

        /* Dropdown panel */
        .nav-dropdown {
            display: none;
            position: absolute;
            top: 100%;
            left: 0;
            min-width: 220px;
            background: #ffffff;
            border: 1px solid var(--border);
            border-top: 3px solid var(--accent);
            border-radius: 0 0 var(--radius) var(--radius);
            box-shadow: 0 8px 24px rgba(0,0,0,.12);
            z-index: 200;
        }

        .nav-group:hover .nav-dropdown {
            display: block;
        }

        /* Dropdown items */
        .nav-dropdown a {
            display: block;
            padding: 11px 18px;
            font-size: 13px;
            font-weight: 400;
            color: var(--text);
            text-decoration: none;
            transition: background .15s, color .15s, padding-left .15s;
            border-bottom: 1px solid #f0f0f0;
        }

        .nav-dropdown a:last-child {
            border-bottom: none;
        }

        .nav-dropdown a:hover {
            background: #fff5f5;
            color: var(--accent);
            padding-left: 24px;
        }

        .nav-dropdown a.active {
            color: var(--accent);
            font-weight: 600;
            background: #fff5f5;
        }

        /* ====================================================
           CARD
        ==================================================== */
        .card-wrapper { width: 100%; }

        .card {
            background: var(--surface);
            border: 1px solid var(--border);
            border-top: none;
            border-radius: 0 0 var(--radius) var(--radius);
            padding: 40px 44px 44px;
            box-shadow: 0 8px 40px rgba(0,0,0,0.10);
        }

        .card-header { margin-bottom: 36px; }

        .eyebrow {
            font-size: 11px;
            font-weight: 600;
            letter-spacing: .2em;
            text-transform: uppercase;
            color: var(--accent);
            margin-bottom: 8px;
            display: block;
        }

        .card-title {
            font-family: 'Bebas Neue', cursive;
            font-size: 38px;
            letter-spacing: .03em;
            line-height: 1;
            color: var(--text);
        }

        .card-subtitle {
            margin-top: 8px;
            font-size: 14px;
            color: var(--text-muted);
            font-weight: 300;
        }

        /* ====================================================
           FORM FIELDS
        ==================================================== */
        .field-group {
            display: flex;
            flex-direction: column;
            gap: 22px;
        }

        .field {
            display: flex;
            flex-direction: column;
            gap: 8px;
        }

        .period-field { margin-bottom: 8px; }
        .period-options { display: flex; gap: 0; }
        .period-rbl { display: flex !important; gap: 0; }
        .period-rbl span { display: inline-flex; align-items: center; }
        .period-rbl span:first-child label { border-radius: 8px 0 0 8px; }
        .period-rbl span:last-child  label { border-radius: 0 8px 8px 0; border-right: 1.5px solid var(--border); }
        .period-rbl input[type="radio"] { display: none; }
        .period-rbl label {
            display: inline-block;
            padding: 8px 22px;
            font-size: 13px;
            font-weight: 600;
            letter-spacing: .04em;
            text-transform: uppercase;
            color: var(--text-muted);
            background: var(--surface);
            border: 1.5px solid var(--border);
            border-right: none;
            cursor: pointer;
            transition: background .15s, color .15s;
            margin: 0;
        }
        .period-rbl input[type="radio"]:checked + label {
            background: var(--accent);
            color: #fff;
            border-color: var(--accent);
        }
        .period-rbl label:hover { background: #f5e6e6; color: var(--accent); }

        label {
            font-size: 13px;
            font-weight: 500;
            color: var(--text-muted);
            letter-spacing: .05em;
            text-transform: uppercase;
        }

        label .req { color: var(--accent); margin-left: 3px; }

        select,
        input[type="number"] {
            width: 100%;
            background: var(--surface2);
            border: 1px solid var(--border);
            color: var(--text);
            font-family: 'DM Sans', sans-serif;
            font-size: 16px;
            font-weight: 400;
            padding: 14px 16px;
            border-radius: var(--radius);
            outline: none;
            appearance: none;
            -webkit-appearance: none;
            transition: border-color .2s, box-shadow .2s, background .2s;
        }

        .select-wrapper { position: relative; }

        .select-wrapper::after {
            content: '';
            position: absolute;
            right: 16px;
            top: 50%;
            transform: translateY(-50%);
            width: 0; height: 0;
            border-left: 5px solid transparent;
            border-right: 5px solid transparent;
            border-top: 6px solid var(--text-muted);
            pointer-events: none;
            transition: border-top-color .2s;
        }

        .select-wrapper:focus-within::after { border-top-color: var(--accent); }

        select:focus,
        input[type="number"]:focus {
            border-color: var(--accent);
            box-shadow: 0 0 0 3px var(--accent-glow);
            background: #fff5f5;
        }

        select:disabled { opacity: 0.4; cursor: not-allowed; }

        input[type="number"]::-webkit-inner-spin-button,
        input[type="number"]::-webkit-outer-spin-button { -webkit-appearance: none; }
        input[type="number"] { -moz-appearance: textfield; }

        /* ====================================================
           ADDRESS PANEL
        ==================================================== */
        .addr-footer {
            display: flex;
            align-items: center;
            justify-content: space-between;
            margin-top: 6px;
            flex-wrap: wrap;
            gap: 8px;
        }
        .map-link {
            display: inline-flex;
            align-items: center;
            gap: 5px;
            font-size: 12px;
            font-weight: 600;
            color: #1a73e8;
            text-decoration: none;
            padding: 4px 10px;
            border: 1.5px solid #1a73e8;
            border-radius: 6px;
            transition: background .15s, color .15s;
        }
        .map-link:hover {
            background: #1a73e8;
            color: #fff;
        }
        .map-link svg { flex-shrink: 0; }

                .address-panel {
            background: #fff8f8;
            border: 1px solid rgba(204,30,30,0.20);
            border-left: 3px solid var(--accent);
            border-radius: var(--radius);
            padding: 14px 16px;
            animation: fadeUp .25s ease both;
        }

        .address-panel .addr-label {
            font-size: 10px;
            font-weight: 600;
            letter-spacing: .15em;
            text-transform: uppercase;
            color: var(--accent);
            margin-bottom: 6px;
            display: flex;
            align-items: center;
            gap: 6px;
        }

        .address-panel .addr-label::before {
            content: '\25C9';
            font-size: 12px;
        }

        .address-panel .addr-text {
            font-size: 13px;
            color: var(--text);
            line-height: 1.6;
            font-weight: 400;
        }

        .address-panel .addr-pin {
            display: inline-block;
            margin-top: 6px;
            font-size: 11px;
            font-weight: 600;
            background: var(--accent);
            color: #fff;
            padding: 2px 8px;
            border-radius: 20px;
            letter-spacing: .05em;
        }

        /* ====================================================
           SALES SUMMARY PANEL
        ==================================================== */
        .sales-summary {
            background: #f0f7ff;
            border: 1px solid #c0d8f0;
            border-left: 3px solid #2176ae;
            border-radius: var(--radius);
            padding: 16px;
            animation: fadeUp .25s ease both;
        }

        .sales-summary .summary-title {
            font-size: 10px;
            font-weight: 600;
            letter-spacing: .15em;
            text-transform: uppercase;
            color: #2176ae;
            margin-bottom: 12px;
            display: flex;
            align-items: center;
            gap: 6px;
        }

        .sales-summary .summary-title::before {
            content: '\25C9';
            font-size: 12px;
        }

        .summary-grid {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 12px;
        }

        .summary-stat {
            display: flex;
            flex-direction: column;
            align-items: center;
            background: #ffffff;
            border: 1px solid #d0e6f8;
            border-radius: 8px;
            padding: 12px 8px;
            text-align: center;
        }

        .summary-stat .stat-value {
            font-family: 'Bebas Neue', cursive;
            font-size: 28px;
            letter-spacing: .04em;
            color: #2176ae;
            line-height: 1;
        }

        .summary-stat .stat-label {
            font-size: 10px;
            font-weight: 600;
            letter-spacing: .10em;
            text-transform: uppercase;
            color: var(--text-muted);
            margin-top: 4px;
        }

        .summary-last-order {
            margin-top: 10px;
            font-size: 12px;
            color: var(--text-muted);
            text-align: center;
        }

        .summary-last-order strong {
            color: var(--text);
        }

        /* Credit divider inside summary panel */
        .summary-divider {
            border: none;
            border-top: 1px dashed #c0d8f0;
            margin: 14px 0 12px;
        }

        .summary-section-title {
            font-size: 10px;
            font-weight: 600;
            letter-spacing: .15em;
            text-transform: uppercase;
            color: #1a7a4a;
            margin-bottom: 10px;
            display: flex;
            align-items: center;
            gap: 6px;
        }

        .summary-section-title::before {
            content: '\25C9';
            font-size: 12px;
        }

        /* Credit stats — green theme */
        .credit-grid {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 10px;
            margin-bottom: 10px;
        }

        .credit-stat {
            display: flex;
            flex-direction: column;
            align-items: center;
            background: #f0fff8;
            border: 1px solid #b0e0c8;
            border-radius: 8px;
            padding: 10px 6px;
            text-align: center;
        }

        .credit-stat .stat-value {
            font-family: 'Bebas Neue', cursive;
            font-size: 22px;
            letter-spacing: .03em;
            color: #1a7a4a;
            line-height: 1;
        }

        .credit-stat .stat-label {
            font-size: 10px;
            font-weight: 600;
            letter-spacing: .08em;
            text-transform: uppercase;
            color: var(--text-muted);
            margin-top: 4px;
        }

        /* Outstanding balance row */
        .outstanding-row {
            display: flex;
            align-items: center;
            justify-content: space-between;
            background: #fff8f0;
            border: 1px solid #f0d0a0;
            border-left: 3px solid #e07800;
            border-radius: 8px;
            padding: 10px 14px;
            margin-top: 10px;
        }

        .outstanding-row .out-label {
            font-size: 11px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: .08em;
            color: #e07800;
        }

        .outstanding-row .out-value {
            font-family: 'Bebas Neue', cursive;
            font-size: 24px;
            color: #c05000;
            letter-spacing: .04em;
        }

        .outstanding-row .out-value.credit {
            color: #1a7a4a;
        }

        @media (max-width: 400px) {
            .summary-grid  { grid-template-columns: 1fr 1fr; }
            .credit-grid   { grid-template-columns: 1fr 1fr; }
        }

        /* ====================================================
           ORDER HISTORY CHART PANEL
        ==================================================== */
        .chart-panel {
            background: #ffffff;
            border: 1px solid var(--border);
            border-top: 3px solid #2176ae;
            border-radius: var(--radius);
            padding: 16px 16px 12px;
            animation: fadeUp .3s ease both;
        }

        .chart-title {
            font-size: 10px;
            font-weight: 600;
            letter-spacing: .15em;
            text-transform: uppercase;
            color: #2176ae;
            margin-bottom: 12px;
            display: flex;
            align-items: center;
            gap: 6px;
        }

        .chart-title::before {
            content: '\25C9';
            font-size: 12px;
        }

        .chart-container {
            position: relative;
            height: 200px;
            width: 100%;
        }

        .closing-stock-info {
            display: flex;
            align-items: center;
            gap: 8px;
            background: #f0f7ff;
            border: 1px solid #b3d4f5;
            border-left: 4px solid #2979c9;
            border-radius: 6px;
            padding: 10px 16px;
            margin-bottom: 16px;
            font-size: 14px;
            color: #1a3a5c;
        }
        .closing-stock-label { color: #555; }
        .closing-stock-date  { font-weight: 600; color: #2979c9; }
        .closing-stock-sep   { color: #aaa; }
        @keyframes bounce {
            0%, 100% { transform: translateY(0); }
            50%       { transform: translateY(-6px); }
        }
        .closing-stock-units { font-weight: 700; font-size: 22px; color: #2979c9; animation: bounce 1.2s ease-in-out infinite; display: inline-block; }
        .closing-stock-jars  { color: #555; font-size: 13px; }

        .divider {
            border: none;
            border-top: 1px solid var(--border);
            margin: 28px 0;
        }

        /* ====================================================
           SUBMIT BUTTON
        ==================================================== */
        .btn-submit {
            width: 100%;
            padding: 16px;
            background: var(--accent);
            color: #ffffff;
            font-family: 'Bebas Neue', cursive;
            font-size: 20px;
            letter-spacing: .12em;
            border: none;
            border-radius: var(--radius);
            cursor: pointer;
            transition: background .2s, transform .12s, box-shadow .2s;
            box-shadow: 0 4px 24px var(--accent-glow);
        }

        .btn-submit:hover {
            background: var(--accent-dark);
            box-shadow: 0 6px 32px rgba(204,30,30,.45);
            transform: translateY(-1px);
        }

        .btn-submit:active { transform: translateY(0); }

        .btn-submit:disabled {
            background: var(--border);
            color: var(--text-muted);
            cursor: not-allowed;
            box-shadow: none;
            transform: none;
        }

        /* ====================================================
           ALERTS
        ==================================================== */
        .alert {
            display: flex;
            align-items: flex-start;
            gap: 12px;
            padding: 14px 16px;
            border-radius: var(--radius);
            font-size: 14px;
            font-weight: 500;
            margin-bottom: 28px;
            animation: fadeUp .3s ease both;
        }

        .alert-success {
            background: rgba(26,158,106,0.08);
            border: 1px solid rgba(26,158,106,0.35);
            color: var(--success);
        }

        .alert-error {
            background: rgba(204,30,30,0.07);
            border: 1px solid rgba(204,30,30,0.30);
            color: var(--error);
        }

        .alert-icon { font-size: 18px; line-height: 1; flex-shrink: 0; }

        .validation-summary {
            background: rgba(204,30,30,0.07);
            border: 1px solid rgba(204,30,30,0.30);
            border-radius: var(--radius);
            padding: 14px 16px;
            color: var(--error);
            font-size: 13px;
            margin-bottom: 20px;
        }

        .validation-summary ul { padding-left: 18px; }
        .validation-summary ul li { margin-top: 4px; }

        .field-validator {
            font-size: 12px;
            color: var(--error);
            margin-top: -4px;
        }

        .spinner {
            display: inline-block;
            width: 16px; height: 16px;
            border: 2px solid rgba(255,255,255,.3);
            border-top-color: #fff;
            border-radius: 50%;
            animation: spin .7s linear infinite;
            vertical-align: middle;
            margin-right: 6px;
        }
        @keyframes spin { to { transform: rotate(360deg); } }

        /* ====================================================
           MOBILE
        ==================================================== */
        @media (max-width: 600px) {
            body { padding: 0; justify-content: flex-start; }
            .page-wrapper { max-width: 100%; border-radius: 0; }

            .logo-area {
                border-radius: 0;
                border-left: none;
                border-right: none;
                padding: 16px;
            }
            .logo-area img { height: 52px; }
            .bis-label { font-size: 15px; letter-spacing: .08em; }
            .navbar { flex-wrap: wrap; }
            .nav-item { padding: 11px 14px; font-size: 12px; }
            .nav-dropdown { min-width: 180px; }
            .date-badge .date-day { font-size: 26px; }
            .date-badge .date-month-year { font-size: 11px; }

            .card {
                padding: 24px 16px 32px;
                border-radius: 0;
                border-left: none;
                border-right: none;
                box-shadow: none;
            }

            .card-title { font-size: 28px; }
            .eyebrow    { font-size: 10px; }
            .card-subtitle { font-size: 13px; }
            label { font-size: 12px; }
            select, input[type="number"] { font-size: 16px; padding: 14px 12px; }
            .btn-submit { font-size: 18px; padding: 18px; }
            .field-group { gap: 18px; }
            .divider { margin: 20px 0; }
        }

        @media (max-width: 375px) {
            .card-title { font-size: 24px; }
            .logo-area img { height: 44px; }
            .date-badge .date-day { font-size: 22px; }
        }
    </style>
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
</head>
<body>

    <div class="page-wrapper">

        <!-- LOGO + DATE -->
        <div class="logo-area">
            <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri Efficient Nutrition" />

            <div class="center-text">
                <span class="bis-label">Business Intelligence<br />Systems</span>
            </div>

            <div class="date-badge">
                <span class="date-label">Today</span>
                <span class="date-day"   id="datDay"></span>
                <span class="date-month-year" id="datMonthYear"></span>
                <span class="date-weekday"    id="datWeekday"></span>
            </div>
        </div>

        <div class="accent-bar"></div>

        <!-- NAVBAR -->
        <nav class="navbar">

            <!-- ERP Home -->
            <a href="ERPHome.aspx" class="nav-item" style="text-decoration:none;" title="Back to ERP Home">
                &#x2302; ERP
            </a>

            <!-- S&D dropdown -->
            <div class="nav-group">
                <span class="nav-item active">
                    Sales &amp; Distribution <span class="chevron">&#9660;</span>
                </span>
                <div class="nav-dropdown">
                    <a href="StockEntry.aspx" class="active">Distributor Stock Position Entry</a>
                    <a href="DailySales.aspx">Daily Sales Entry</a>
                </div>
            </div>

            <!-- Admin dropdown - visible to Admin role only -->
            <asp:Panel ID="pnlAdminMenu" runat="server" Visible="false" style="position:relative;">
            <div class="nav-group">
                <span class="nav-item">
                    Admin <span class="chevron">&#9660;</span>
                </span>
                <div class="nav-dropdown">
                    <a href="UserAdmin.aspx">User Management</a>
                    <a href="ProductMaster.aspx">Product Master</a>
                </div>
            </div>
            </asp:Panel>

            <!-- Reports -->
            <div class="nav-group">
                <span class="nav-item">Reports <span class="chevron">&#9660;</span></span>
                <div class="nav-dropdown">
                    <a href="Reports.aspx">Stock Movement</a>
                    <a href="Reports.aspx?tab=daily">Daily Sales</a>
                </div>
            </div>

            <!-- Right side: user info + sign out -->
            <div style="margin-left:auto;display:flex;align-items:center;gap:20px;font-size:13px;">
                <asp:Label ID="lblUserInfo" runat="server"
                    style="color:#fff;opacity:.9;font-weight:500;" />
                <a href="Logout.aspx"
                   style="color:#fff;font-weight:700;text-decoration:none;
                          border:1.5px solid rgba(255,255,255,.6);
                          padding:5px 14px;border-radius:6px;opacity:.9;"
                   onclick="return confirm('Sign out?')">
                    &#x2192; Sign Out
                </a>
            </div>

        </nav>

        <form id="form1" runat="server" class="card-wrapper">
            <div class="card">

                <div class="card-header">
                    <span class="eyebrow">Distributor Stockholding</span>
                    <h1 class="card-title">Distributor Stock Position Entry</h1>
                    <p class="card-subtitle">Select location details and enter current stock count.</p>
                </div>

                <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
                    <div class="alert alert-success">
                        <span class="alert-icon">&#10004;</span>
                        <span><asp:Label ID="lblSuccess" runat="server" /></span>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlError" runat="server" Visible="false">
                    <div class="alert alert-error">
                        <span class="alert-icon">&#10006;</span>
                        <span><asp:Label ID="lblError" runat="server" /></span>
                    </div>
                </asp:Panel>

                <asp:ValidationSummary ID="valSummary" runat="server"
                    CssClass="validation-summary"
                    HeaderText="Please correct the following errors:"
                    DisplayMode="BulletList"
                    ShowMessageBox="false"
                    ShowSummary="true" />

                <div class="field-group">

                    <!-- PERIOD SELECTOR -->
                    <div class="field period-field">
                        <label>Report Period</label>
                        <div class="period-options">
                            <asp:RadioButtonList ID="rblPeriod" runat="server"
                                RepeatDirection="Horizontal"
                                RepeatLayout="Flow"
                                AutoPostBack="true"
                                OnSelectedIndexChanged="rblPeriod_SelectedIndexChanged"
                                CssClass="period-rbl">
                                <asp:ListItem Text="30 Days"  Value="30"  Selected="True" />
                                <asp:ListItem Text="60 Days"  Value="60"  />
                                <asp:ListItem Text="90 Days"  Value="90"  />
                            </asp:RadioButtonList>
                        </div>
                    </div>

                    <!-- STATE -->
                    <div class="field">
                        <label for="ddlState">State <span class="req">*</span></label>
                        <div class="select-wrapper">
                            <asp:DropDownList ID="ddlState" runat="server"
                                AutoPostBack="true"
                                OnSelectedIndexChanged="ddlState_SelectedIndexChanged">
                            </asp:DropDownList>
                        </div>
                        <asp:RequiredFieldValidator ID="rfvState" runat="server"
                            ControlToValidate="ddlState" InitialValue="0"
                            ErrorMessage="Please select a State."
                            Display="Dynamic" CssClass="field-validator"
                            Text="* Please select a State." />
                    </div>

                    <!-- CITY -->
                    <div class="field">
                        <label for="ddlCity">City <span class="req">*</span></label>
                        <div class="select-wrapper">
                            <asp:DropDownList ID="ddlCity" runat="server"
                                AutoPostBack="true"
                                OnSelectedIndexChanged="ddlCity_SelectedIndexChanged"
                                Enabled="false">
                            </asp:DropDownList>
                        </div>
                        <asp:RequiredFieldValidator ID="rfvCity" runat="server"
                            ControlToValidate="ddlCity" InitialValue="0"
                            ErrorMessage="Please select a City."
                            Display="Dynamic" CssClass="field-validator"
                            Text="* Please select a City." />
                    </div>

                    <!-- DISTRIBUTOR -->
                    <div class="field">
                        <label for="ddlDistributor">Distributor Name <span class="req">*</span></label>
                        <div class="select-wrapper">
                            <asp:DropDownList ID="ddlDistributor" runat="server"
                                Enabled="false"
                                AutoPostBack="true"
                                OnSelectedIndexChanged="ddlDistributor_SelectedIndexChanged">
                            </asp:DropDownList>
                        </div>
                        <asp:RequiredFieldValidator ID="rfvDistributor" runat="server"
                            ControlToValidate="ddlDistributor" InitialValue="0"
                            ErrorMessage="Please select a Distributor."
                            Display="Dynamic" CssClass="field-validator"
                            Text="* Please select a Distributor." />
                    </div>

                    <!-- ADDRESS PANEL (shown after distributor selected) -->
                    <asp:Panel ID="pnlAddress" runat="server" Visible="false">
                        <div class="address-panel">
                            <div class="addr-label">Distributor Address</div>
                            <div class="addr-text">
                                <asp:Label ID="lblAddress" runat="server" />
                            </div>
                            <div class="addr-footer">
                                <asp:Panel ID="pnlPin" runat="server" Visible="false">
                                    <span class="addr-pin">PIN: <asp:Label ID="lblPin" runat="server" /></span>
                                </asp:Panel>
                                <asp:HyperLink ID="lnkGoogleMap" runat="server"
                                    Target="_blank"
                                    CssClass="map-link"
                                    Visible="false">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></svg>
                                    View on Map
                                </asp:HyperLink>
                            </div>
                        </div>
                    </asp:Panel>

                    <!-- SUMMARY DIV — always rendered, shown/hidden via style -->
                    <div id="divSummary" runat="server" style="display:none;">
                        <div class="sales-summary">

                            <!-- Sales section -->
                            <div class="summary-title">Last <asp:Label ID="lblPeriodTitle" runat="server" Text="60" /> Days &mdash; Sales</div>
                            <div class="summary-grid">
                                <div class="summary-stat">
                                    <span class="stat-value"><asp:Label ID="lblSummaryOrders" runat="server" Text="0" /></span>
                                    <span class="stat-label">Orders</span>
                                </div>
                                <div class="summary-stat">
                                    <span class="stat-value"><asp:Label ID="lblSummaryUnits" runat="server" Text="0" /></span>
                                    <span class="stat-label">Units</span>
                                </div>
                                <div class="summary-stat">
                                    <span class="stat-value" style="font-size:18px;"><asp:Label ID="lblSummaryValue" runat="server" Text="&#8377;0" /></span>
                                    <span class="stat-label">Sales Value</span>
                                </div>
                            </div>
                            <div class="summary-last-order">
                                Last Order: <strong><asp:Label ID="lblLastOrder" runat="server" Text="&mdash;" /></strong>
                            </div>

                            <hr class="summary-divider" />

                            <!-- Credit/Receipt section -->
                            <div class="summary-section-title">Receipts &amp; Payments</div>
                            <div class="credit-grid">
                                <div class="credit-stat">
                                    <span class="stat-value" style="font-size:18px;"><asp:Label ID="lblCreditTotal" runat="server" Text="&#8377;0" /></span>
                                    <span class="stat-label">Total Received</span>
                                </div>
                                <div class="credit-stat">
                                    <span class="stat-value" style="font-size:18px;"><asp:Label ID="lblCredit60" runat="server" Text="&#8377;0" /></span>
                                    <span class="stat-label">Last <asp:Label ID="lblCreditDays" runat="server" Text="60" /> Days</span>
                                </div>
                                <div class="credit-stat">
                                    <span class="stat-value" style="font-size:16px;"><asp:Label ID="lblLastPayment" runat="server" Text="&mdash;" /></span>
                                    <span class="stat-label">Last Payment</span>
                                </div>
                            </div>
                            <div class="outstanding-row">
                                <span class="out-label">Outstanding Balance</span>
                                <asp:Label ID="lblOutstanding" runat="server" Text="&#8377;0" CssClass="out-value" />
                            </div>

                        </div>
                    </div>

                    <!-- Hidden fields always in DOM -->
                    <asp:HiddenField ID="hfChartData"    runat="server" Value="" />
                    <asp:HiddenField ID="hfPaymentData"  runat="server" Value="" />
                    <!-- Chart visibility flags (0=hidden, 1=show) -->
                    <asp:HiddenField ID="hfShowOrderChart"   runat="server" Value="0" />
                    <asp:HiddenField ID="hfShowPaymentChart" runat="server" Value="0" />

                    <!-- ORDER HISTORY CHART — always in DOM, shown via JS -->
                    <div id="divOrderChart" style="display:none;">
                        <div class="chart-panel">
                            <div class="chart-title">Units Ordered &ndash; Last <asp:Label ID="lblChartDays" runat="server" Text="60" /> Days</div>
                            <div class="chart-container">
                                <canvas id="orderChart"></canvas>
                            </div>
                        </div>
                    </div>

                    <!-- PAYMENT HISTORY CHART — always in DOM, shown via JS -->
                    <div id="divPaymentChart" style="display:none; margin-top:8px;">
                        <div class="chart-panel" style="border-top-color:#1a7a4a;">
                            <div class="chart-title" style="color:#1a7a4a;">Payment History &ndash; Last <asp:Label ID="lblPaymentDays" runat="server" Text="60" /> Days</div>
                            <div class="chart-container">
                                <canvas id="paymentChart"></canvas>
                            </div>
                        </div>
                    </div>

                    <hr class="divider" />

                    <!-- CLOSING STOCK - shown only if previous entry exists -->
                    <asp:Panel ID="pnlClosingStock" runat="server" Visible="false">
                        <div class="closing-stock-info">
                            <span class="closing-stock-label">Closing Stock as of</span>
                            <asp:Label ID="lblClosingDate" runat="server" CssClass="closing-stock-date" />
                            <span class="closing-stock-sep">&mdash;</span>
                            <asp:Label ID="lblClosingUnits" runat="server" CssClass="closing-stock-units" />
                            <span class="closing-stock-jars">Jars</span>
                        </div>
                    </asp:Panel>

                    <!-- CURRENT STOCK POSITION -->
                    <div class="field">
                        <label for="txtCurrentStock">Current Stock Position (No of Units) <span class="req">*</span></label>
                        <asp:TextBox ID="txtCurrentStock" runat="server"
                            TextMode="SingleLine"
                            placeholder="Enter Stock Count (JAR/Box: e.g. 500)" />
                        <asp:RequiredFieldValidator ID="rfvStock" runat="server"
                            ControlToValidate="txtCurrentStock"
                            ErrorMessage="Current Stock Position (No of Units) is required."
                            Display="Dynamic" CssClass="field-validator"
                            Text="* Current Stock Position (No of Units) is required." />
                        <asp:RangeValidator ID="rvStock" runat="server"
                            ControlToValidate="txtCurrentStock"
                            Type="Integer" MinimumValue="0" MaximumValue="9999999"
                            ErrorMessage="Current Stock must be a whole number between 0 and 9,999,999."
                            Display="Dynamic" CssClass="field-validator"
                            Text="* Enter a valid whole number (0 – 9,999,999)." />
                    </div>

                </div><!-- /field-group -->

                <hr class="divider" />

                <asp:Button ID="btnSubmit" runat="server"
                    Text="SAVE STOCK POSITION"
                    CssClass="btn-submit"
                    OnClick="btnSubmit_Click" />

            </div><!-- /card -->
        </form>

    </div><!-- /page-wrapper -->

    <script type="text/javascript">

        // Render current date into the badge
        (function () {
            var now  = new Date();
            var days = ['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'];
            var months = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];

            document.getElementById('datDay').textContent       = now.getDate();
            document.getElementById('datMonthYear').textContent = months[now.getMonth()] + ' ' + now.getFullYear();
            document.getElementById('datWeekday').textContent   = days[now.getDay()];
        })();

        // Make stock input numeric
        (function () {
            var txtStock = document.getElementById('<%= txtCurrentStock.ClientID %>');
            if (txtStock) {
                txtStock.setAttribute('type', 'number');
                txtStock.setAttribute('min', '0');
                txtStock.setAttribute('max', '9999999');
                txtStock.setAttribute('step', '1');
            }
        })();
    </script>

    <!-- Chart.js from CDN -->
    <script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/4.4.1/chart.umd.min.js"></script>
    <script type="text/javascript">

        function renderCharts() {
            var hfOrder   = document.getElementById('<%= hfChartData.ClientID %>');
            var hfPayment = document.getElementById('<%= hfPaymentData.ClientID %>');
            var hfShowOrder   = document.getElementById('<%= hfShowOrderChart.ClientID %>');
            var hfShowPayment = document.getElementById('<%= hfShowPaymentChart.ClientID %>');

            var divOrder   = document.getElementById('divOrderChart');
            var divPayment = document.getElementById('divPaymentChart');

            // ── Order chart ──────────────────────────────────────
            if (hfShowOrder && hfShowOrder.value === '1' && hfOrder && hfOrder.value) {
                divOrder.style.display = 'block';
                var data;
                try { data = JSON.parse(hfOrder.value); } catch(e) { data = null; }

                if (data && data.labels && data.labels.length > 0) {
                    if (window._orderChartInstance) { window._orderChartInstance.destroy(); }
                    var ctx1 = document.getElementById('orderChart').getContext('2d');
                    window._orderChartInstance = new Chart(ctx1, {
                        type: 'line',
                        data: {
                            labels: data.labels,
                            datasets: [{
                                label: 'Units Ordered',
                                data: data.values,
                                borderColor: '#2176ae',
                                backgroundColor: 'rgba(33,118,174,0.08)',
                                borderWidth: 2.5,
                                pointBackgroundColor: '#2176ae',
                                pointBorderColor: '#ffffff',
                                pointBorderWidth: 2,
                                pointRadius: 5,
                                pointHoverRadius: 7,
                                fill: true,
                                tension: 0.35
                            }]
                        },
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            plugins: {
                                legend: { display: false },
                                tooltip: { callbacks: {
                                    title: function(i) { return 'Date: ' + i[0].label; },
                                    label: function(i) { return 'Units: ' + Number(i.raw).toLocaleString(); }
                                }}
                            },
                            scales: {
                                x: { grid: { color: 'rgba(0,0,0,0.05)' }, ticks: { font: { size: 10 }, color: '#999', maxRotation: 45, autoSkip: true, maxTicksLimit: 12 }},
                                y: { beginAtZero: true, grid: { color: 'rgba(0,0,0,0.05)' }, ticks: { font: { size: 10 }, color: '#999' }}
                            }
                        }
                    });
                }
            } else if (divOrder) {
                divOrder.style.display = 'none';
            }

            // ── Payment chart ────────────────────────────────────
            if (hfShowPayment && hfShowPayment.value === '1' && hfPayment && hfPayment.value) {
                divPayment.style.display = 'block';
                var data2;
                try { data2 = JSON.parse(hfPayment.value); } catch(e) { data2 = null; }

                if (data2 && data2.labels && data2.labels.length > 0) {
                    if (window._paymentChartInstance) { window._paymentChartInstance.destroy(); }
                    var ctx2 = document.getElementById('paymentChart').getContext('2d');
                    window._paymentChartInstance = new Chart(ctx2, {
                        type: 'bar',
                        data: {
                            labels: data2.labels,
                            datasets: [{
                                label: 'Payment Amount',
                                data: data2.values,
                                backgroundColor: 'rgba(26,122,74,0.75)',
                                borderColor: '#1a7a4a',
                                borderWidth: 1.5,
                                borderRadius: 4,
                                hoverBackgroundColor: 'rgba(26,122,74,0.95)'
                            }]
                        },
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            plugins: {
                                legend: { display: false },
                                tooltip: { callbacks: {
                                    title: function(i) { return 'Date: ' + i[0].label; },
                                    label: function(i) { return '₹' + Number(i.raw).toLocaleString('en-IN', { minimumFractionDigits: 2 }); }
                                }}
                            },
                            scales: {
                                x: { grid: { color: 'rgba(0,0,0,0.05)' }, ticks: { font: { size: 10 }, color: '#999', maxRotation: 45, autoSkip: true, maxTicksLimit: 12 }},
                                y: { beginAtZero: true, grid: { color: 'rgba(0,0,0,0.05)' }, ticks: { font: { size: 10 }, color: '#999',
                                    callback: function(v) { return '₹' + Number(v).toLocaleString('en-IN'); }
                                }}
                            }
                        }
                    });
                }
            } else if (divPayment) {
                divPayment.style.display = 'none';
            }
        }

        window.addEventListener('load', function() { renderCharts(); });

    </script>
</body>
</html>
