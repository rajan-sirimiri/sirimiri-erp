<%@ Page Language="C#" AutoEventWireup="true" EnableEventValidation="false" Inherits="MMApp.MMConsumableInward" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Consumable Inward &mdash; MM</title>
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
    <style>
        :root { --bg:#f5f5f5; --surface:#fff; --border:#e0e0e0; --accent:#cc1e1e; --teal:#1a9e6a; --primary:#e07b00; --gold:#b8860b; --orange:#e07b00; --text:#1a1a1a; --text-muted:#666; --text-dim:#999; --radius:12px; }
        *,*::before,*::after { box-sizing:border-box; margin:0; padding:0; }
        body { background:var(--bg); color:var(--text); font-family:'DM Sans',sans-serif; min-height:100vh; }
        nav { background:#1a1a1a; display:flex; align-items:center; padding:0 28px; height:52px; gap:6px; position:sticky; top:0; z-index:100; }
        .nav-brand { font-family:'Bebas Neue',sans-serif; font-size:18px; color:#fff; letter-spacing:.1em; margin-right:20px; }
        .nav-item { color:#aaa; text-decoration:none; font-size:12px; font-weight:600; letter-spacing:.06em; text-transform:uppercase; padding:6px 12px; border-radius:6px; transition:all .2s; }
        .nav-item:hover,.nav-item.active { color:#fff; background:rgba(255,255,255,0.08); }
        .nav-sep { color:#444; margin:0 4px; }
        .nav-right { margin-left:auto; display:flex; align-items:center; gap:12px; }
        .nav-user { font-size:12px; color:#888; }
        .nav-logout { font-size:11px; color:#666; text-decoration:none; padding:4px 10px; border:1px solid #333; border-radius:5px; transition:all .2s; }
        .nav-logout:hover { color:var(--accent); border-color:var(--accent); }
        .page-header { background:var(--surface); border-bottom:1px solid var(--border); padding:20px 40px; display:flex; align-items:center; justify-content:space-between; }
        .page-title { font-family:'Bebas Neue',sans-serif; font-size:28px; letter-spacing:.07em; }
        .page-title span { color:var(--primary); }
        .page-sub { font-size:12px; color:var(--text-muted); margin-top:2px; }
        .grn-badge { font-family:'Bebas Neue',sans-serif; font-size:22px; letter-spacing:.1em; color:var(--gold); background:rgba(184,134,11,0.08); border:1px solid rgba(184,134,11,0.2); padding:6px 20px; border-radius:8px; }
        .main-layout { max-width:1280px; margin:28px auto; padding:0 32px; display:grid; grid-template-columns:1fr 340px; gap:24px; align-items:start; }
        .card { background:var(--surface); border:1px solid var(--border); border-radius:var(--radius); padding:22px 26px; margin-bottom:20px; }
        .card:last-child { margin-bottom:0; }
        .card-title { font-family:'Bebas Neue',sans-serif; font-size:15px; letter-spacing:.08em; color:var(--text-muted); margin-bottom:16px; padding-bottom:10px; border-bottom:1px solid var(--border); display:flex; align-items:center; gap:8px; }
        .card-title::before { content:''; display:inline-block; width:3px; height:14px; background:var(--primary); border-radius:2px; }
        .form-grid { display:grid; grid-template-columns:repeat(3,1fr); gap:14px; }
        .form-grid-2 { display:grid; grid-template-columns:1fr 1fr; gap:14px; }
        .form-group { display:flex; flex-direction:column; gap:5px; }
        .form-group.span2 { grid-column:span 2; }
        label { font-size:11px; font-weight:700; letter-spacing:.07em; text-transform:uppercase; color:var(--text-muted); }
        label .req { color:var(--accent); margin-left:2px; }
        input[type=text],input[type=date],select,textarea { width:100%; padding:9px 12px; border:1.5px solid var(--border); border-radius:8px; font-family:'DM Sans',sans-serif; font-size:13px; color:var(--text); background:#fafafa; transition:border-color .2s; outline:none; }
        input:focus,select:focus,textarea:focus { border-color:var(--primary); background:#fff; }
        .field-hint { font-size:11px; color:var(--text-dim); }
        textarea { resize:vertical; min-height:58px; }
        .shortage-row { background:rgba(224,123,0,0.06); border:1.5px solid rgba(224,123,0,0.25); border-radius:8px; padding:10px 14px; margin-top:8px; display:flex; align-items:center; justify-content:space-between; }
        .shortage-label { font-size:11px; font-weight:700; letter-spacing:.06em; text-transform:uppercase; color:var(--orange); }
        .shortage-vals { display:flex; gap:20px; }
        .shortage-val { text-align:right; }
        .shortage-val .num { font-family:'Bebas Neue',sans-serif; font-size:20px; color:var(--orange); }
        .shortage-val .sub { font-size:10px; color:var(--text-dim); }
        .amount-box { background:#fafafa; border:1px solid var(--border); border-radius:10px; padding:14px 18px; margin-top:18px; }
        .amt-row { display:flex; justify-content:space-between; align-items:center; padding:5px 0; font-size:13px; border-bottom:1px solid #f0f0f0; }
        .amt-row:last-child { border-bottom:none; }
        .amt-row label { font-size:11px; font-weight:600; color:var(--text-muted); text-transform:uppercase; letter-spacing:.05em; }
        .amt-row .val { font-weight:600; color:var(--text); }
        .amt-row.total { margin-top:6px; padding-top:10px; border-top:2px solid var(--border); border-bottom:none; }
        .amt-row.total label { font-size:13px; color:var(--text); font-weight:700; }
        .amt-row.total .val { font-family:'Bebas Neue',sans-serif; font-size:22px; color:var(--primary); }
        .check-row { display:flex; align-items:center; gap:10px; padding:8px 0; }
        .check-row input[type=checkbox] { width:15px; height:15px; accent-color:var(--primary); cursor:pointer; flex-shrink:0; }
        .check-row label { font-size:13px; font-weight:500; color:var(--text); text-transform:none; letter-spacing:0; cursor:pointer; }
        .qc-box { background:rgba(224,123,0,0.04); border:1.5px solid rgba(224,123,0,0.2); border-radius:8px; padding:12px 16px; }
        .qc-box .check-row label { font-size:13px; font-weight:600; color:var(--primary); }
        .btn-row { display:flex; gap:10px; margin-top:18px; flex-wrap:wrap; }
        .btn { padding:11px 24px; border-radius:8px; font-family:'DM Sans',sans-serif; font-size:13px; font-weight:700; cursor:pointer; border:none; transition:all .2s; }
        .btn-receive { background:var(--primary); color:#fff; }
        .btn-receive:hover { background:#b56200; box-shadow:0 4px 12px rgba(224,123,0,0.3); }
        .btn-reject { background:transparent; border:2px solid var(--accent); color:var(--accent); }
        .btn-reject:hover { background:#fff5f5; }
        .btn-clear { background:transparent; border:1.5px solid var(--border); color:var(--text-muted); }
        .btn-clear:hover { border-color:var(--text-muted); color:var(--text); }
        .alert { padding:12px 16px; border-radius:8px; font-size:13px; margin-bottom:18px; }
        .alert-success { background:rgba(224,123,0,0.10); color:var(--primary); border:1px solid rgba(224,123,0,0.25); }
        .alert-danger  { background:rgba(204,30,30,0.08); color:var(--accent); border:1px solid rgba(204,30,30,0.2); }
        .rec-panel { background:var(--surface); border:1.5px solid rgba(224,123,0,0.3); border-radius:var(--radius); overflow:hidden; position:sticky; top:68px; }
        .rec-header { background:rgba(224,123,0,0.08); padding:14px 18px; border-bottom:1px solid rgba(224,123,0,0.2); }
        .rec-title { font-family:'Bebas Neue',sans-serif; font-size:17px; letter-spacing:.07em; color:var(--orange); }
        .rec-sub { font-size:11px; color:var(--text-muted); margin-top:2px; }
        .rec-supplier { font-size:12px; font-weight:600; color:var(--text); margin-top:6px; }
        .rec-empty { padding:32px 18px; text-align:center; color:var(--text-dim); font-size:13px; }
        .rec-list { max-height:480px; overflow-y:auto; }
        .rec-item { padding:12px 18px; border-bottom:1px solid #f5f5f5; }
        .rec-item:last-child { border-bottom:none; }
        .rec-item-grn  { font-size:10px; font-weight:700; color:var(--gold); letter-spacing:.05em; }
        .rec-item-name { font-size:13px; font-weight:500; color:var(--text); margin:2px 0; }
        .rec-item-date { font-size:11px; color:var(--text-dim); }
        .rec-item-amounts { display:flex; justify-content:space-between; margin-top:6px; }
        .rec-chip { padding:3px 10px; border-radius:20px; font-size:11px; font-weight:700; }
        .rec-chip-qty   { background:rgba(224,123,0,0.10); color:var(--orange); }
        .rec-chip-value { background:rgba(204,30,30,0.08); color:var(--accent); }
        .rec-total { padding:12px 18px; border-top:2px solid rgba(224,123,0,0.2); background:rgba(224,123,0,0.05); display:flex; justify-content:space-between; align-items:center; }
        .rec-total-label { font-size:11px; font-weight:700; text-transform:uppercase; letter-spacing:.06em; color:var(--orange); }
        .rec-total-val { font-family:'Bebas Neue',sans-serif; font-size:20px; color:var(--orange); }
        .list-card { background:var(--surface); border:1px solid var(--border); border-radius:var(--radius); overflow:hidden; margin-top:24px; }
        .list-header { padding:14px 20px; border-bottom:1px solid var(--border); display:flex; align-items:center; flex-wrap:wrap; gap:12px; }
        .list-title { font-family:'Bebas Neue',sans-serif; font-size:18px; letter-spacing:.07em; }
        .list-controls { display:flex; align-items:center; gap:8px; margin-left:auto; }
        .list-controls input[type=date] { padding:6px 10px; font-size:12px; border:1.5px solid var(--border); border-radius:6px; outline:none; }
        .btn-filter { padding:6px 14px; background:var(--primary); color:#fff; border:none; border-radius:6px; font-size:12px; font-weight:700; cursor:pointer; }
        .list-count { font-size:11px; color:var(--text-muted); background:var(--bg); padding:3px 10px; border-radius:20px; }
        .grn-table { width:100%; border-collapse:collapse; }
        .grn-table th { padding:9px 12px; text-align:left; font-size:10px; font-weight:700; letter-spacing:.1em; text-transform:uppercase; color:var(--text-dim); background:#fafafa; border-bottom:1px solid var(--border); white-space:nowrap; }
        .grn-table td { padding:10px 12px; font-size:12px; border-bottom:1px solid #f0f0f0; vertical-align:middle; }
        .grn-table tr:last-child td { border-bottom:none; }
        .grn-table tr:hover td { background:#fafafa; }
        .grn-no { font-weight:700; font-size:11px; color:var(--gold); letter-spacing:.04em; }
        .badge-shortage { display:inline-block; padding:2px 8px; border-radius:20px; font-size:10px; font-weight:700; background:rgba(224,123,0,0.12); color:var(--orange); }
        .badge-qc-pass  { display:inline-block; padding:2px 6px; border-radius:20px; font-size:10px; font-weight:700; background:rgba(224,123,0,0.08); color:var(--primary); }
        .badge-qc-fail  { display:inline-block; padding:2px 6px; border-radius:20px; font-size:10px; font-weight:700; background:rgba(0,0,0,0.05); color:var(--text-dim); }
        .empty-state { text-align:center; padding:40px; color:var(--text-dim); font-size:13px; }

        /* Expandable GRN detail rows */
        .grn-no-link { display:inline-flex; align-items:center; gap:6px; text-decoration:none; color:inherit; cursor:pointer; transition:color .15s; }
        .grn-no-link:hover { color:var(--teal); }
        .grn-no-link .chev { font-size:9px; color:var(--text-dim); transition:transform .15s; display:inline-block; }
        .grn-no-link.open .chev { transform:rotate(90deg); color:var(--teal); }
        .grn-detail-row td.grn-detail-cell { background:#fafafa; padding:0; border-top:2px solid var(--teal); border-bottom:2px solid var(--border); }
        .grn-detail-content { padding:16px 20px; }
        .grn-detail-loading { padding:16px 20px; color:var(--text-dim); font-size:12px; font-style:italic; }
        .grn-detail-header { display:flex; gap:24px; flex-wrap:wrap; padding:12px 20px; background:#fff; border:1px solid var(--border); border-radius:8px; margin-bottom:12px; font-size:12px; }
        .grn-detail-header div { display:flex; flex-direction:column; gap:2px; }
        .grn-detail-header .label { font-size:10px; font-weight:700; letter-spacing:.06em; text-transform:uppercase; color:var(--text-dim); }
        .grn-detail-header .value { font-weight:600; color:var(--text); }
        .grn-lines-table { width:100%; border-collapse:collapse; background:#fff; border-radius:8px; overflow:hidden; border:1px solid var(--border); }
        .grn-lines-table th { padding:8px 10px; text-align:left; font-size:10px; font-weight:700; letter-spacing:.08em; text-transform:uppercase; color:var(--text-dim); background:#f5f5f5; border-bottom:1px solid var(--border); }
        .grn-lines-table td { padding:9px 10px; font-size:12px; border-bottom:1px solid #f5f5f5; }
        .grn-lines-table tr:last-child td { border-bottom:none; }
        .grn-single-grid { display:grid; grid-template-columns:repeat(4, 1fr); gap:10px 18px; padding:14px 18px; background:#fff; border:1px solid var(--border); border-radius:8px; }
        .grn-single-grid > div { display:flex; flex-direction:column; gap:2px; font-size:12px; }
        .grn-single-grid .label { font-size:10px; font-weight:700; letter-spacing:.06em; text-transform:uppercase; color:var(--text-dim); }
        .grn-single-grid .value { font-weight:600; color:var(--text); }
        .grn-detail-err { padding:16px 20px; color:var(--accent); font-size:12px; }
        @media(max-width:1000px) { .main-layout { grid-template-columns:1fr; } .rec-panel { position:static; } }
    </style>
</head>
<body>
<form id="form1" runat="server">

    <nav>
        <a href="MMHome.aspx" style="display:flex;align-items:center;margin-right:16px;flex-shrink:0;background:#fff;border-radius:6px;padding:3px 8px;"><img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" style="height:28px;width:auto;object-fit:contain;" onerror="this.style.display='none'" /></a>
        <a href="/StockApp/ERPHome.aspx" class="nav-item">&#x2302; ERP Home</a>
        <span class="nav-sep">&rsaquo;</span>
        <a href="MMHome.aspx" class="nav-item">Home</a>
        <span class="nav-sep">&rsaquo;</span>
        <span class="nav-item active">Consumable Inward</span>
        <div class="nav-right">
            <span class="nav-user"><asp:Label ID="lblNavUser" runat="server" /></span>
            <a href="#" class="nav-logout" onclick="erpConfirm('Sign out?',{title:'Sign Out',type:'warn',okText:'Sign Out',onOk:function(){window.location='MMLogout.aspx';}});return false;">Sign Out</a>
        </div>
    </nav>

    <div class="page-header">
        <div>
            <div class="page-title">Consumable <span>Inward</span></div>
            <div class="page-sub">Record consumable receipts &mdash; generate GRN</div>
        </div>
        <div class="grn-badge"><asp:Label ID="lblGRNNo" runat="server" Text="GRN-CN-#####" /></div>
    </div>

    <asp:HiddenField ID="hfInwardID"   runat="server" Value="0" />
    <asp:HiddenField ID="hfSupplierID" runat="server" Value="0" />
    <asp:HiddenField ID="hfTaxable"    runat="server" Value="0" />
    <asp:HiddenField ID="hfGSTAmount"  runat="server" Value="0" />
    <asp:HiddenField ID="hfTotal"      runat="server" Value="0" />
    <asp:HiddenField ID="hfLoading"   runat="server" Value="0" />
    <asp:HiddenField ID="hfUnloading" runat="server" Value="0" />
    <asp:HiddenField ID="hfQtyVerified" runat="server" Value="0" />
    <asp:Button ID="btnSupplierTrigger" runat="server" style="display:none;" OnClick="btnSupplierTrigger_Click" CausesValidation="false" />

    <div class="main-layout">
        <div>
            <asp:Panel ID="pnlAlert" runat="server" Visible="false">
                <div class="alert" id="alertBox" runat="server">
                    <asp:Label ID="lblAlert" runat="server" />
                </div>
            </asp:Panel>

            <div class="card">
                <div class="card-title">Item &amp; Supplier</div>
                <!-- Invoice mode: radio group -->
                <div style="margin-bottom:14px;padding:10px 14px;background:#fff8f0;border:1.5px solid #ffe0b2;border-radius:8px;">
                    <div style="display:flex;align-items:center;gap:24px;flex-wrap:wrap;">
                        <span style="font-size:11px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;color:#e67e22;">Invoice Mode:</span>
                        <label style="display:flex;align-items:center;gap:6px;cursor:pointer;font-size:13px;font-weight:600;color:#333;">
                            <input type="radio" name="rblInvoiceMode" id="rbInvNormal" value="normal" checked
                                   onclick="setInvoiceMode('normal');"
                                   style="width:16px;height:16px;accent-color:#e67e22;cursor:pointer;"/>
                            Normal Invoice
                        </label>
                        <label style="display:flex;align-items:center;gap:6px;cursor:pointer;font-size:13px;font-weight:600;color:#e67e22;">
                            <input type="radio" name="rblInvoiceMode" id="rbInvManual" value="manual"
                                   onclick="setInvoiceMode('manual');"
                                   style="width:16px;height:16px;accent-color:#e67e22;cursor:pointer;"/>
                            Manual Invoice
                        </label>
                        <label style="display:flex;align-items:center;gap:6px;cursor:pointer;font-size:13px;font-weight:600;color:#999;">
                            <input type="radio" name="rblInvoiceMode" id="rbInvNone" value="none"
                                   onclick="setInvoiceMode('none');"
                                   style="width:16px;height:16px;accent-color:#999;cursor:pointer;"/>
                            No Invoice
                        </label>
                    </div>
                    <asp:HiddenField ID="hfInvoiceMode" runat="server" Value="normal"/>
                </div>
                <div class="form-grid">
                    <div class="form-group" style="position:relative;">
                        <label>Consumable Item <span class="req">*</span></label>
                        <input type="text" id="txtItemSearch" placeholder="&#128269; Tap to search item..." readonly
                            onfocus="openSearchModal(this, '<%= ddlItem.ClientID %>', 'txtItemSearch', 'Item');"
                            style="margin-bottom:4px;padding:8px 12px;border:1.5px solid #e0e0e0;border-radius:8px;font-size:12px;background:#fffdf5 !important;color:#0f0f0f !important;outline:none;width:100%;cursor:pointer !important;" autocomplete="off"/>
                        <asp:DropDownList ID="ddlItem" runat="server" onchange="onItemChange(this)" />
                    </div>
                    <div class="form-group" style="position:relative;">
                        <label>Supplier <span class="req">*</span></label>
                        <input type="text" id="txtSupplierSearch" placeholder="&#128269; Tap to search supplier..." readonly
                            onfocus="openSearchModal(this, '<%= ddlSupplier.ClientID %>', 'txtSupplierSearch', 'Supplier');"
                            style="margin-bottom:4px;padding:8px 12px;border:1.5px solid #e0e0e0;border-radius:8px;font-size:12px;background:#fffdf5 !important;color:#0f0f0f !important;outline:none;width:100%;cursor:pointer !important;" autocomplete="off"/>
                        <asp:DropDownList ID="ddlSupplier" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlSupplier_Changed" />
                    </div>
                    <div class="form-group">
                        <label>GRN Date <span class="req">*</span></label>
                        <asp:TextBox ID="txtGRNDate" runat="server" TextMode="Date" />
                    </div>
                    <div class="form-group">
                        <label>Invoice No</label>
                        <asp:TextBox ID="txtInvoiceNo" runat="server" MaxLength="50" placeholder="e.g. INV-2024-001" />
                    </div>
                    <div class="form-group">
                        <label>Invoice Date</label>
                        <asp:TextBox ID="txtInvoiceDate" runat="server" TextMode="Date" />
                    </div>
                    <div class="form-group">
                        <label>PO Number</label>
                        <asp:TextBox ID="txtPONo" runat="server" MaxLength="50" placeholder="Optional" />
                    </div>
                </div>
            </div>

            <div class="card">
                <div class="card-title">Quantity &amp; Pricing</div>
                <div class="form-grid">
                    <div class="form-group">
                        <label>Qty as per Invoice <span class="req">*</span></label>
                        <div style="display:flex;gap:6px;align-items:center;">
                            <asp:TextBox ID="txtQtyInvoice" runat="server" placeholder="0" onchange="calcAll()" onkeyup="calcAll()" style="flex:1;" />
                            <asp:DropDownList ID="ddlInvoiceUOM" runat="server" onchange="syncReceivedUOM(this)" style="width:90px;padding:9px 6px;border:1.5px solid #e0e0e0;border-radius:8px;font-family:'DM Sans',sans-serif;font-size:13px;background:#fafafa;outline:none;" />
                        </div>
                        <span class="field-hint">Qty billed by supplier</span>
                    </div>
                    <div class="form-group">
                        <label>Qty Actually Received <span class="req">*</span></label>
                        <div style="display:flex;gap:6px;align-items:center;">
                            <asp:TextBox ID="txtQtyReceived" runat="server" placeholder="0" onchange="calcAll()" onkeyup="calcAll()" style="flex:1;" />
                            <asp:DropDownList ID="ddlReceivedUOM" runat="server" style="width:90px;padding:9px 6px;border:1.5px solid #e0e0e0;border-radius:8px;font-family:'DM Sans',sans-serif;font-size:13px;background:#fafafa;outline:none;" />
                        </div>
                        <span class="field-hint">Qty physically counted</span>
                    </div>
                    <div class="form-group" style="background:rgba(224,123,0,0.06);border:1.5px solid rgba(224,123,0,0.3);border-radius:10px;padding:10px 12px;">
                        <label style="color:var(--primary);">Qty in Stock UOM <span class="req">*</span></label>
                        <div style="display:flex;gap:6px;align-items:center;">
                            <asp:TextBox ID="txtQtyUOM" runat="server" placeholder="0" onchange="calcAll()" onkeyup="calcAll()" style="flex:1;border-color:rgba(224,123,0,0.4);background:#fff8f0;" />
                            <asp:DropDownList ID="ddlStdUOM" runat="server" style="width:90px;padding:9px 6px;border:1.5px solid rgba(224,123,0,0.4);border-radius:8px;font-family:'DM Sans',sans-serif;font-size:13px;background:#fff8f0;outline:none;" />
                        </div>
                        <span class="field-hint" id="uomHint" style="color:var(--primary);">Select item to auto-fill UOM</span>
                    </div>
                    <div class="form-group">
                        <label>Unit Price <span class="req">*</span></label>
                        <asp:TextBox ID="txtRate" runat="server" placeholder="0.00" onchange="calcAll()" onkeyup="calcAll()" />
                    </div>
                    <div class="form-group">
                        <label>HSN Code</label>
                        <asp:TextBox ID="txtHSN" runat="server" MaxLength="20" placeholder="Auto-filled from item" />
                    </div>
                    <div class="form-group">
                        <label>GST Rate (%)</label>
                        <asp:TextBox ID="txtGSTRate" runat="server" placeholder="0" onchange="calcAll()" onkeyup="calcAll()" />
                    </div>
                    <div class="form-group">
                        <label>Transportation Cost</label>
                        <asp:TextBox ID="txtTransport" runat="server" placeholder="0.00" onchange="calcAll()" onkeyup="calcAll()" />
                    </div>
                    <div class="form-group">
                        <label>Loading Charges</label>
                        <asp:TextBox ID="txtLoading" runat="server" placeholder="0.00" />
                    </div>
                    <div class="form-group">
                        <label>Unloading Charges</label>
                        <asp:TextBox ID="txtUnloading" runat="server" placeholder="0.00" />
                    </div>
                    <div class="form-group span2">
                        <div class="check-row">
                            <asp:CheckBox ID="chkTransportInInvoice" runat="server" onclick="calcAll()" />
                            <label for="<%= chkTransportInInvoice.ClientID %>">Transport cost is part of invoice (add to taxable amount)</label>
                        </div>
                        <div class="check-row">
                            <asp:CheckBox ID="chkTransportInGST" runat="server" onclick="calcAll()" />
                            <label for="<%= chkTransportInGST.ClientID %>">Include transport in GST calculation</label>
                        </div>
                    </div>
                </div>

                <div class="shortage-row" id="shortageRow" style="display:none;">
                    <span class="shortage-label">Shortage &mdash; Recoverable from Supplier</span>
                    <div class="shortage-vals">
                        <div class="shortage-val">
                            <div class="num" id="dispShortageQty">0</div>
                            <div class="sub">Shortage Qty</div>
                        </div>
                        <div class="shortage-val">
                            <div class="num" id="dispShortageVal">Rs. 0</div>
                            <div class="sub">Value</div>
                        </div>
                    </div>
                </div>

                <div class="amount-box">
                    <div class="amt-row"><label>Invoice Amount (Qty x Rate)</label><span class="val" id="dispInvoiceAmt">Rs. 0.00</span></div>
                    <div class="amt-row"><label>Taxable Amount</label><span class="val" id="dispTaxable">Rs. 0.00</span></div>
                    <div class="amt-row"><label>GST Amount</label><span class="val" id="dispGST">Rs. 0.00</span></div>
                    <div class="amt-row"><label>Transport Cost</label><span class="val" id="dispTransport">Rs. 0.00</span></div>
                    <div class="amt-row total"><label>Total Amount</label><span class="val" id="dispTotal">Rs. 0.00</span></div>
                </div>
            </div>

            <div class="card">
                <div class="card-title">Quality &amp; Remarks</div>
                <div class="form-grid-2">
                    <div class="form-group">
                        <label>Remarks</label>
                        <asp:TextBox ID="txtRemarks" runat="server" TextMode="MultiLine" placeholder="Notes about this shipment..." />
                    </div>
                    <div class="form-group">
                        <label>Quality Check</label>
                        <div class="qc-box" style="margin-top:4px;">
                            <div class="check-row">
                                <asp:CheckBox ID="chkQC" runat="server" />
                                <label for="<%= chkQC.ClientID %>">Quality Check Passed</label>
                            </div>
                            <div class="field-hint" style="margin-top:4px;">Check if goods passed quality inspection</div>
                        </div>
                    </div>
                </div>
                <div class="btn-row">
                    <button type="button" class="btn btn-receive" onclick="showGRNConfirm();">Receive Goods</button>
                    <asp:Button ID="btnReceive" runat="server" Text="Receive Goods" CssClass="btn btn-receive" OnClick="btnReceive_Click" style="display:none;" />
                    <asp:Button ID="btnReject"  runat="server" Text="Reject Goods"  CssClass="btn btn-reject"  OnClick="btnReject_Click"  CausesValidation="false" OnClientClick="return erpConfirmLink(this,'Reject and discard this GRN entry?',{title:'Reject Goods',okText:'Yes, Reject',btnClass:'danger'})" />
                    <asp:Button ID="btnClear"   runat="server" Text="Clear"          CssClass="btn btn-clear"   OnClick="btnClear_Click"   CausesValidation="false" />
                </div>
            </div>

            <div class="list-card">
                <div class="list-header">
                    <span class="list-title">GRN History</span>
                    <div class="list-controls">
                        <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date" />
                        <asp:TextBox ID="txtToDate"   runat="server" TextMode="Date" />
                        <asp:Button  ID="btnFilter"   runat="server" Text="Filter" CssClass="btn-filter" OnClick="btnFilter_Click" CausesValidation="false" />
                    </div>
                    <asp:Label ID="lblCount" runat="server" CssClass="list-count" Text="0 records" />
                </div>
                <div style="overflow-x:auto;max-height:480px;overflow-y:auto;">
                    <asp:Repeater ID="rptGRN" runat="server">
                        <HeaderTemplate>
                            <table class="grn-table"><thead><tr>
                                <th>GRN No</th><th>Date</th><th>Item</th><th>Supplier</th>
                                <th>Invoice No</th><th>Inv Qty</th><th>Act Qty</th><th>Std Qty</th>
                                <th>Rate</th><th>GST</th><th>Transport</th><th>Total</th>
                                <th>Shortage</th><th>QC</th>
                            </tr></thead><tbody>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr class="grn-row" data-grn='<%# Eval("GRNNo") %>'>
                                <td>
                                    <a href="javascript:void(0)" class="grn-no-link" onclick="toggleGRNDetail(this,'<%# Eval("GRNNo") %>','CN');">
                                        <span class="chev">&#9654;</span>
                                        <span class="grn-no"><%# Eval("GRNNo") %></span>
                                    </a>
                                </td>
                                <td><%# Eval("InwardDate","{0:dd-MMM-yy}") %></td>
                                <td>
                                    <div style="font-weight:500;font-size:12px;"><%# Eval("ConsumableName") %></div>
                                    <div style="font-size:10px;color:var(--text-dim);"><%# Eval("ConsumableCode") %></div>
                                </td>
                                <td style="font-size:11px;"><%# Eval("SupplierName") %></td>
                                <td style="font-size:11px;"><%# Eval("InvoiceNo") %></td>
                                <td style="text-align:right;"><%# Eval("Quantity") %></td>
                                <td style="text-align:right;"><%# Eval("QtyActualReceived") %></td>
                                <td style="text-align:right;"><%# Eval("QtyInUOM") %> <span style="font-size:10px;color:var(--text-dim);"><%# Eval("Abbreviation") %></span></td>
                                <td style="text-align:right;"><%# Eval("Rate","Rs.{0:N2}") %></td>
                                <td style="text-align:right;"><%# Eval("GSTAmount","Rs.{0:N2}") %></td>
                                <td style="text-align:right;"><%# Eval("TransportCost","Rs.{0:N2}") %></td>
                                <td style="text-align:right;font-weight:600;"><%# Eval("Amount","Rs.{0:N2}") %></td>
                                <td><%# Convert.ToDecimal(Eval("ShortageQty")) > 0 ? "<span class='badge-shortage'>" + Eval("ShortageQty") + "</span>" : "<span style='color:var(--text-dim);'>--</span>" %></td>
                                <td><span class='<%# Convert.ToBoolean(Eval("QualityCheck")) ? "badge-qc-pass" : "badge-qc-fail" %>'><%# Convert.ToBoolean(Eval("QualityCheck")) ? "Pass" : "--" %></span></td>
                            </tr>
                            <tr class="grn-detail-row" data-grn='<%# Eval("GRNNo") %>' style="display:none;">
                                <td colspan="14" class="grn-detail-cell">
                                    <div class="grn-detail-loading">Loading details&hellip;</div>
                                </td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate></tbody></table></FooterTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
                        <div class="empty-state">No GRN records found for the selected period.</div>
                    </asp:Panel>
                </div>
            </div>
        </div>

        <div>
            <div class="rec-panel">
                <div class="rec-header">
                    <div class="rec-title">Supplier Recoverables</div>
                    <div class="rec-sub">Pending shortage recovery from selected supplier</div>
                    <div class="rec-supplier"><asp:Label ID="lblRecSupplier" runat="server" Text="-- Select a supplier --" /></div>
                </div>
                <asp:Panel ID="pnlRecEmpty" runat="server">
                    <div class="rec-empty">
                        <div style="font-size:28px;margin-bottom:8px;">&#10003;</div>
                        <div>Select a supplier to view recoverables</div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlRecList" runat="server" Visible="false">
                    <div class="rec-list">
                        <asp:Repeater ID="rptRecoverables" runat="server">
                            <ItemTemplate>
                                <div class="rec-item">
                                    <div class="rec-item-grn"><%# Eval("GRNNo") %></div>
                                    <div class="rec-item-name"><%# Eval("ConsumableName") %> <span style="font-size:10px;color:var(--text-dim);">(<%# Eval("ConsumableCode") %>)</span></div>
                                    <div class="rec-item-date"><%# Eval("InwardDate","{0:dd-MMM-yyyy}") %></div>
                                    <div class="rec-item-amounts">
                                        <span class="rec-chip rec-chip-qty"><%# Eval("ShortageQty") %> <%# Eval("Abbreviation") %> short</span>
                                        <span class="rec-chip rec-chip-value">Rs. <%# Eval("ShortageValue","{0:N2}") %></span>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <div class="rec-total">
                        <span class="rec-total-label">Total Recoverable</span>
                        <span class="rec-total-val">Rs. <asp:Label ID="lblRecTotal" runat="server" Text="0.00" /></span>
                    </div>
                </asp:Panel>
            </div>
        </div>
    </div>

</form>
<script>
    var itemData = <%= ItemDataJson %>;

    function setInvoiceMode(mode) {
        // mode: 'normal' | 'manual' | 'none'
        var inv     = document.getElementById('<%= txtInvoiceNo.ClientID %>');
        var invDate = document.getElementById('<%= txtInvoiceDate.ClientID %>');
        var gst     = document.getElementById('<%= txtGSTRate.ClientID %>');
        var hfMode  = document.getElementById('<%= hfInvoiceMode.ClientID %>');
        if (hfMode) hfMode.value = mode;

        if (mode === 'none') {
            inv.value = 'NO-INVOICE';
            inv.readOnly = true; inv.style.background = '#f0f0f0'; inv.style.color = '#999';
            if (invDate) { invDate.value = ''; invDate.readOnly = true; invDate.style.background = '#f0f0f0'; }
            if (gst) { gst.value = '0'; gst.readOnly = true; gst.style.background = '#f0f0f0'; }
        } else if (mode === 'manual') {
            var current = (inv.value || '').trim();
            if (current === 'NO-INVOICE' || current === 'MANUAL INVOICE' || current === '') {
                inv.value = 'MN-';
            } else if (current.indexOf('MN-') !== 0) {
                inv.value = 'MN-' + current;
            }
            inv.readOnly = false; inv.style.background = ''; inv.style.color = '';
            if (invDate) { invDate.readOnly = false; invDate.style.background = ''; }
            if (gst) { gst.value = '0'; gst.readOnly = true; gst.style.background = '#f0f0f0'; }
        } else {
            var cur = (inv.value || '').trim();
            if (cur === 'NO-INVOICE' || cur === 'MANUAL INVOICE' || cur.indexOf('MN-') === 0) {
                inv.value = '';
            }
            inv.readOnly = false; inv.style.background = ''; inv.style.color = '';
            if (invDate) { invDate.readOnly = false; invDate.style.background = ''; }
            if (gst) { gst.readOnly = false; gst.style.background = ''; }
        }
        calcAll();
    }

    // Generic searchable dropdown filter
    // ── Modal Search Overlay (touch-friendly) ──
    var _modalOverlay = null;
    function openSearchModal(searchInput, ddlId, searchId, title) {
        searchInput.blur();
        var ddl = document.getElementById(ddlId);
        if (!ddl) return;
        var items = [];
        for (var i = 0; i < ddl.options.length; i++) {
            if (ddl.options[i].value === '0') continue;
            items.push({ value: ddl.options[i].value, text: ddl.options[i].text, idx: i });
        }
        if (_modalOverlay) _modalOverlay.remove();
        var ov = document.createElement('div');
        ov.id = 'searchOverlay';
        ov.style.cssText = 'position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,.5);z-index:9999;display:flex;align-items:flex-start;justify-content:center;padding:40px 16px 0;';
        var box = document.createElement('div');
        box.style.cssText = 'background:#fff;border-radius:14px;width:100%;max-width:540px;max-height:80vh;display:flex;flex-direction:column;box-shadow:0 8px 40px rgba(0,0,0,.25);overflow:hidden;';
        var hdr = document.createElement('div');
        hdr.style.cssText = 'padding:16px 20px 12px;border-bottom:2px solid #f0ede8;display:flex;align-items:center;justify-content:space-between;';
        hdr.innerHTML = '<span style="font-family:\'Bebas Neue\',sans-serif;font-size:18px;letter-spacing:.06em;">Select ' + title + '</span>';
        var closeBtn = document.createElement('button');
        closeBtn.type = 'button';
        closeBtn.innerHTML = '✕';
        closeBtn.style.cssText = 'border:none;background:none;font-size:20px;cursor:pointer;color:#999;padding:4px 8px;';
        closeBtn.onclick = function() { ov.remove(); _modalOverlay = null; };
        hdr.appendChild(closeBtn);
        box.appendChild(hdr);
        var sWrap = document.createElement('div');
        sWrap.style.cssText = 'padding:12px 20px;';
        var sInput = document.createElement('input');
        sInput.type = 'text';
        sInput.placeholder = 'Search ' + title.toLowerCase() + '...';
        sInput.style.cssText = 'width:100%;padding:12px 16px;border:2px solid #e0e0e0;border-radius:10px;font-size:16px;font-family:\'DM Sans\',sans-serif;outline:none;background:#fafafa;';
        sInput.setAttribute('autocomplete', 'off');
        sInput.setAttribute('autocorrect', 'off');
        sInput.setAttribute('autocapitalize', 'off');
        sWrap.appendChild(sInput);
        box.appendChild(sWrap);
        var list = document.createElement('div');
        list.style.cssText = 'flex:1;overflow-y:auto;padding:0 8px 12px;-webkit-overflow-scrolling:touch;';
        function renderList(query) {
            list.innerHTML = '';
            var q = (query || '').toLowerCase().trim();
            var count = 0;
            items.forEach(function(it) {
                if (q && it.text.toLowerCase().indexOf(q) < 0) return;
                count++;
                var row = document.createElement('div');
                row.style.cssText = 'padding:12px 14px;border-radius:8px;cursor:pointer;font-size:14px;margin:2px 0;transition:background .1s;';
                row.onmouseenter = function() { row.style.background = '#f5f5f0'; };
                row.onmouseleave = function() { row.style.background = 'transparent'; };
                if (q) {
                    var idx = it.text.toLowerCase().indexOf(q);
                    row.innerHTML = it.text.substring(0, idx) + '<strong style="color:var(--accent,#cc1e1e);">' + it.text.substring(idx, idx + q.length) + '</strong>' + it.text.substring(idx + q.length);
                } else { row.textContent = it.text; }
                row.onclick = function() {
                    ddl.selectedIndex = it.idx;
                    var sb = document.getElementById(searchId);
                    if (sb) sb.value = it.text;
                    ov.remove();
                    _modalOverlay = null;
                    var evt = document.createEvent('HTMLEvents');
                    evt.initEvent('change', true, false);
                    ddl.dispatchEvent(evt);
                    if (ddl.onchange) ddl.onchange.call(ddl, evt);
                };
                list.appendChild(row);
            });
            if (count === 0) {
                var empty = document.createElement('div');
                empty.style.cssText = 'padding:20px;text-align:center;color:#999;font-size:13px;';
                empty.textContent = 'No results found';
                list.appendChild(empty);
            }
        }
        box.appendChild(list);
        ov.appendChild(box);
        document.body.appendChild(ov);
        _modalOverlay = ov;
        ov.onclick = function(e) { if (e.target === ov) { ov.remove(); _modalOverlay = null; } };
        renderList('');
        setTimeout(function() { sInput.focus(); }, 150);
        sInput.oninput = function() { renderList(sInput.value); };
    }

    document.addEventListener('change', function(e) {
        var pairs = [['<%= ddlItem.ClientID %>', 'txtItemSearch'], ['<%= ddlSupplier.ClientID %>', 'txtSupplierSearch']];
        pairs.forEach(function(p) {
            if (e.target.id === p[0]) {
                e.target.size = 0; e.target.style.position = ''; e.target.style.zIndex = '';
                var sb = document.getElementById(p[1]);
                if (sb) sb.value = e.target.options[e.target.selectedIndex].text;
            }
        });
    });

    function syncAllUOMs(src) {
        var val = src.value;
        var ids = ['<%= ddlInvoiceUOM.ClientID %>', '<%= ddlReceivedUOM.ClientID %>', '<%= ddlStdUOM.ClientID %>'];
        ids.forEach(function(id) {
            var ddl = document.getElementById(id);
            if (ddl && ddl !== src) ddl.value = val;
        });
    }

    function onItemChange(sel) {
        var d = itemData[sel.value];
        if (d) {
            document.getElementById('<%= txtHSN.ClientID %>').value     = d.hsn || '';
            document.getElementById('<%= txtGSTRate.ClientID %>').value = d.gst || '';
            document.getElementById('uomHint').innerText = 'Stock UOM: ' + d.uom;
            var stdIds = ['<%= ddlInvoiceUOM.ClientID %>', '<%= ddlReceivedUOM.ClientID %>', '<%= ddlStdUOM.ClientID %>'];
            stdIds.forEach(function(id) {
                var ddl = document.getElementById(id);
                if (!ddl) return;
                for (var i = 0; i < ddl.options.length; i++) {
                    if (ddl.options[i].text === d.uom) { ddl.selectedIndex = i; break; }
                }
            });
        } else {
            document.getElementById('uomHint').innerText = 'Select item to auto-fill UOM';
        }
        calcAll();
    }

    function syncReceivedUOM(sel) {
        syncAllUOMs(sel);
    }

    function onSupplierChange(supId) {
        document.getElementById('<%= hfSupplierID.ClientID %>').value = supId;
        __doPostBack('<%= btnSupplierTrigger.UniqueID %>', '');
    }

    function n(id) { return parseFloat(document.getElementById(id).value) || 0; }
    function c(id) { return document.getElementById(id).checked; }
    function fmt(v) { return 'Rs. ' + parseFloat(v||0).toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g,','); }

    function calcAll() {
        var qtyInv    = n('<%= txtQtyInvoice.ClientID %>');
        var qtyAct    = n('<%= txtQtyReceived.ClientID %>');
        var qtyBilled = n('<%= txtQtyUOM.ClientID %>');
        var rate      = n('<%= txtRate.ClientID %>');
        var gstRate   = n('<%= txtGSTRate.ClientID %>');
        var transport = n('<%= txtTransport.ClientID %>');
        var transInv  = c('<%= chkTransportInInvoice.ClientID %>');
        var transGST  = c('<%= chkTransportInGST.ClientID %>');

        var shortage = Math.max(0, qtyInv - qtyAct);
        var shortRow = document.getElementById('shortageRow');
        if (shortage > 0 && qtyAct > 0) {
            document.getElementById('dispShortageQty').innerText = shortage.toFixed(3).replace(/\.?0+$/,'');
            document.getElementById('dispShortageVal').innerText = fmt(shortage * rate);
            shortRow.style.display = 'flex';
        } else { shortRow.style.display = 'none'; }

        // Invoice value now uses Qty Billed (not Standard Qty)
        var invoiceAmt = qtyBilled * rate;
        var taxable    = invoiceAmt + (transInv ? transport : 0);
        var gstBase    = transGST ? taxable : invoiceAmt;
        var gstAmt     = gstBase * (gstRate / 100);
        var total      = taxable + gstAmt + (transInv ? 0 : transport);

        document.getElementById('dispInvoiceAmt').innerText = fmt(invoiceAmt);
        document.getElementById('dispTaxable').innerText    = fmt(taxable);
        document.getElementById('dispGST').innerText        = fmt(gstAmt);
        document.getElementById('dispTransport').innerText  = fmt(transport);
        document.getElementById('dispTotal').innerText      = fmt(total);

        document.getElementById('<%= hfTaxable.ClientID %>').value   = taxable.toFixed(2);
        document.getElementById('<%= hfGSTAmount.ClientID %>').value = gstAmt.toFixed(2);
        document.getElementById('<%= hfTotal.ClientID %>').value     = total.toFixed(2);
    }

    function syncAmounts() { calcAll(); return true; }
    window.onload = function() { calcAll(); };
    function showGRNConfirm() {
        if (!syncAmounts()) return;
        var g = function(id) { var el = document.getElementById(id); return el ? (el.value || el.innerText || '') : ''; };
        var ddlText = function(id) { var el = document.getElementById(id); return el && el.selectedIndex >= 0 ? el.options[el.selectedIndex].text : ''; };
        var material = ddlText('<%= ddlItem.ClientID %>');
        var supplier = ddlText('<%= ddlSupplier.ClientID %>');
        var grnDate = g('<%= txtGRNDate.ClientID %>');
        var invoiceNo = g('<%= txtInvoiceNo.ClientID %>');
        var qtyInvoice = g('<%= txtQtyInvoice.ClientID %>');
        var qtyReceived = g('<%= txtQtyReceived.ClientID %>');
        var rate = g('<%= txtRate.ClientID %>');
        var transport = g('<%= txtTransport.ClientID %>') || '0';
        var loading = g('<%= txtLoading.ClientID %>') || '0';
        var unloading = g('<%= txtUnloading.ClientID %>') || '0';
        var total = g('dispTotal') || g('<%= hfTotal.ClientID %>');
        var html = '<table style="width:100%;border-collapse:collapse;">';
        html += '<tr><td style="padding:6px 0;color:#666;width:40%;">Item</td><td style="padding:6px 0;font-weight:600;">' + material + '</td></tr>';
        html += '<tr><td style="padding:6px 0;color:#666;">Supplier</td><td style="padding:6px 0;font-weight:600;">' + supplier + '</td></tr>';
        html += '<tr><td style="padding:6px 0;color:#666;">GRN Date</td><td style="padding:6px 0;">' + grnDate + '</td></tr>';
        html += '<tr><td style="padding:6px 0;color:#666;">Invoice No</td><td style="padding:6px 0;">' + invoiceNo + '</td></tr>';
        html += '<tr style="border-top:1px solid #eee;"><td style="padding:6px 0;color:#666;">Qty (Invoice)</td><td style="padding:6px 0;font-weight:600;">' + qtyInvoice + '</td></tr>';
        html += '<tr><td style="padding:6px 0;color:#666;">Qty (Received)</td><td style="padding:6px 0;font-weight:600;">' + qtyReceived + '</td></tr>';
        html += '<tr><td style="padding:6px 0;color:#666;">Rate</td><td style="padding:6px 0;">' + rate + '</td></tr>';
        html += '<tr style="border-top:1px solid #eee;"><td style="padding:6px 0;color:#666;">Transport Cost</td><td style="padding:6px 0;">Rs. ' + parseFloat(transport).toFixed(2) + '</td></tr>';
        html += '<tr><td style="padding:6px 0;color:#666;">Loading Charges</td><td style="padding:6px 0;">Rs. ' + parseFloat(loading).toFixed(2) + '</td></tr>';
        html += '<tr><td style="padding:6px 0;color:#666;">Unloading Charges</td><td style="padding:6px 0;">Rs. ' + parseFloat(unloading).toFixed(2) + '</td></tr>';
        html += '<tr style="border-top:2px solid #1a9e6a;"><td style="padding:8px 0;color:#1a9e6a;font-weight:700;">Total</td><td style="padding:8px 0;font-weight:700;font-size:16px;color:#1a9e6a;">' + total + '</td></tr>';
        html += '</table>';
        document.getElementById('grnSummary').innerHTML = html;
        document.getElementById('chkQtyVerified').checked = false;
        document.getElementById('grnConfirmOverlay').style.display = 'flex';
    }
    function confirmGRN() {
        document.getElementById('<%= hfQtyVerified.ClientID %>').value = document.getElementById('chkQtyVerified').checked ? '1' : '0';
        document.getElementById('<%= hfLoading.ClientID %>').value = document.getElementById('<%= txtLoading.ClientID %>').value || '0';
        document.getElementById('<%= hfUnloading.ClientID %>').value = document.getElementById('<%= txtUnloading.ClientID %>').value || '0';
        closeGRNConfirm();
        syncAmounts();
        document.getElementById('<%= btnReceive.ClientID %>').click();
    }
    function closeGRNConfirm() { document.getElementById('grnConfirmOverlay').style.display = 'none'; }

    // ── Expandable GRN detail (click GRN No to toggle) ──────────────
    function toggleGRNDetail(linkEl, grnNo, tab) {
        var trLink = linkEl.closest('tr');
        if (!trLink) return;
        var trDet  = trLink.nextElementSibling;
        if (!trDet || !trDet.classList.contains('grn-detail-row')) return;
        var isOpen = trDet.style.display !== 'none';
        if (isOpen) { trDet.style.display = 'none'; linkEl.classList.remove('open'); return; }
        trDet.style.display = 'table-row';
        linkEl.classList.add('open');
        var cell = trDet.querySelector('.grn-detail-cell');
        if (cell.getAttribute('data-loaded') === '1') return;
        cell.innerHTML = '<div class="grn-detail-loading">Loading details&hellip;</div>';
        var xhr = new XMLHttpRequest();
        xhr.open('GET', 'MMGRNDetailAPI.ashx?tab=' + encodeURIComponent(tab) + '&grn=' + encodeURIComponent(grnNo) + '&_=' + Date.now(), true);
        xhr.onreadystatechange = function() {
            if (xhr.readyState !== 4) return;
            if (xhr.status !== 200) { cell.innerHTML = '<div class="grn-detail-err">Failed to load (HTTP ' + xhr.status + ').</div>'; return; }
            try {
                var data = JSON.parse(xhr.responseText);
                if (data.error) { cell.innerHTML = '<div class="grn-detail-err">' + escapeHtml(data.error) + '</div>'; return; }
                cell.innerHTML = renderGRNDetail(data);
                cell.setAttribute('data-loaded', '1');
            } catch (e) { cell.innerHTML = '<div class="grn-detail-err">Error parsing response.</div>'; }
        };
        xhr.send();
    }
    function renderGRNDetail(d) {
        var isMulti = (d.lines || []).length > 1;
        var html = '<div class="grn-detail-content"><div class="grn-detail-header">';
        html += hdrCell('GRN Date', d.grnDate || '—');
        html += hdrCell('Supplier', d.supplier || '—');
        html += hdrCell('Invoice No', d.invoiceNo || '—');
        html += hdrCell('Invoice Date', d.invoiceDate || '—');
        html += hdrCell('PO No', d.poNo || '—');
        html += hdrCell('Status', d.status || '—');
        html += hdrCell('Recorded At', d.createdAt || '—');
        if (isMulti) html += hdrCell('Line Items', String(d.lineCount));
        html += '</div>';
        if (isMulti) {
            html += '<table class="grn-lines-table"><thead><tr><th>#</th><th>Material</th><th>HSN</th><th style="text-align:right;">Inv Qty</th><th style="text-align:right;">Actual</th><th style="text-align:right;">Billed</th><th style="text-align:right;">Rate</th><th style="text-align:right;">GST%</th><th style="text-align:right;">GST Amt</th><th style="text-align:right;">Shortage</th><th style="text-align:right;">Amount</th></tr></thead><tbody>';
            for (var i = 0; i < d.lines.length; i++) {
                var L = d.lines[i];
                html += '<tr><td>' + (i+1) + '</td>';
                html += '<td><div style="font-weight:500;">' + escapeHtml(L.matName) + '</div><div style="font-size:10px;color:#999;">' + escapeHtml(L.matCode) + '</div></td>';
                html += '<td>' + escapeHtml(L.hsn || '—') + '</td>';
                html += '<td style="text-align:right;">' + fmtNum(L.qtyInv) + ' ' + escapeHtml(L.uom) + '</td>';
                html += '<td style="text-align:right;">' + fmtNum(L.qtyActual) + '</td>';
                html += '<td style="text-align:right;">' + fmtNum(L.qtyBilled) + '</td>';
                html += '<td style="text-align:right;">' + fmtRs(L.rate) + '</td>';
                html += '<td style="text-align:right;">' + fmtNum(L.gstRate) + '%</td>';
                html += '<td style="text-align:right;">' + fmtRs(L.gstAmt) + '</td>';
                html += '<td style="text-align:right;">' + (parseFloat(L.shortageQty) > 0 ? '<span class="badge-shortage">' + fmtNum(L.shortageQty) + '</span>' : '—') + '</td>';
                html += '<td style="text-align:right;font-weight:600;">' + fmtRs(L.amount) + '</td></tr>';
            }
            html += '</tbody></table>';
        } else if (d.lines && d.lines.length === 1) {
            var L = d.lines[0];
            html += '<div class="grn-single-grid">';
            html += kv('Material', escapeHtml(L.matName) + ' <span style="color:#999;">(' + escapeHtml(L.matCode) + ')</span>');
            html += kv('HSN Code', escapeHtml(L.hsn || '—'));
            html += kv('Qty as per Invoice', fmtNum(L.qtyInv) + ' ' + escapeHtml(L.uom));
            html += kv('Qty Actually Received', fmtNum(L.qtyActual));
            html += kv('Qty Billed (Standard)', fmtNum(L.qtyBilled));
            html += kv('Unit Rate', fmtRs(L.rate));
            html += kv('GST Rate', fmtNum(L.gstRate) + '%');
            html += kv('GST Amount', fmtRs(L.gstAmt));
            html += kv('Transport Cost', fmtRs(L.transport));
            html += kv('Transport in Invoice', L.transInInv ? 'Yes' : 'No');
            html += kv('Transport in GST', L.transInGst ? 'Yes' : 'No');
            html += kv('Loading Charges', fmtRs(L.loading));
            html += kv('Unloading Charges', fmtRs(L.unloading));
            html += kv('Shortage Qty', parseFloat(L.shortageQty) > 0 ? fmtNum(L.shortageQty) : '—');
            html += kv('Shortage Value', parseFloat(L.shortageVal) > 0 ? fmtRs(L.shortageVal) : '—');
            html += kv('Quality Check', L.qc ? '<span style="color:var(--teal);font-weight:700;">Passed</span>' : '—');
            html += kv('Total Amount', '<span style="color:var(--teal);font-weight:700;">' + fmtRs(L.amount) + '</span>');
            html += '</div>';
            if (L.remarks) {
                html += '<div style="margin-top:10px;padding:10px 14px;background:#fff;border:1px solid var(--border);border-radius:8px;font-size:12px;"><span style="font-size:10px;font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:var(--text-dim);">Remarks</span><br/>' + escapeHtml(L.remarks) + '</div>';
            }
        } else { html += '<div class="grn-detail-err">No line items found for this GRN.</div>'; }
        html += '</div>';
        return html;
    }
    function hdrCell(lbl, val) { return '<div><span class="label">' + escapeHtml(lbl) + '</span><span class="value">' + val + '</span></div>'; }
    function kv(lbl, valHtml) { return '<div><span class="label">' + escapeHtml(lbl) + '</span><span class="value">' + valHtml + '</span></div>'; }
    function fmtNum(v) { var n = parseFloat(v); if (isNaN(n)) return '0'; return n.toFixed(3).replace(/\.?0+$/, ''); }
    function fmtRs(v) { var n = parseFloat(v); if (isNaN(n)) n = 0; return 'Rs. ' + n.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ','); }
    function escapeHtml(s) { if (s === null || s === undefined) return ''; return String(s).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;'); }

</script>

                <!-- GRN Confirmation Modal -->
                <div id="grnConfirmOverlay" style="display:none;position:fixed;inset:0;background:rgba(0,0,0,.5);z-index:9999;align-items:center;justify-content:center;">
                    <div style="background:#fff;border-radius:14px;max-width:520px;width:90%%;padding:28px;box-shadow:0 16px 48px rgba(0,0,0,.2);max-height:80vh;overflow-y:auto;">
                        <div style="font-family:'Bebas Neue',sans-serif;font-size:20px;letter-spacing:.06em;margin-bottom:16px;border-bottom:2px solid var(--teal);padding-bottom:8px;">Confirm GRN Submission</div>
                        <div id="grnSummary" style="font-size:13px;line-height:1.8;"></div>
                        <div style="margin-top:16px;padding-top:12px;border-top:1px solid #e0e0e0;">
                            <label style="display:flex;align-items:center;gap:8px;font-size:13px;font-weight:600;cursor:pointer;">
                                <input type="checkbox" id="chkQtyVerified" style="width:18px;height:18px;accent-color:var(--teal);" />
                                Quantity Verified
                            </label>
                        </div>
                        <div style="display:flex;gap:10px;margin-top:20px;">
                            <button type="button" onclick="confirmGRN();" style="flex:1;padding:12px;border:none;border-radius:8px;background:var(--teal);color:#fff;font-size:14px;font-weight:700;cursor:pointer;font-family:inherit;">Confirm</button>
                            <button type="button" onclick="closeGRNConfirm();" style="flex:1;padding:12px;border:1px solid #ddd;border-radius:8px;background:#f5f5f5;color:#333;font-size:14px;font-weight:700;cursor:pointer;font-family:inherit;">Cancel</button>
                        </div>
                    </div>
                </div>
<script src="/StockApp/erp-modal.js"></script>
<script src="/StockApp/erp-keepalive.js"></script>
</body>
</html>
