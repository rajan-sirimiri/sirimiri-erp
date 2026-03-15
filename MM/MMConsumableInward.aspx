<%@ Page Language="C#" AutoEventWireup="true" Inherits="MMApp.MMConsumableInward" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Consumable Inward &mdash; MM</title>
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet" />
    <style>
        :root { --bg:#f5f5f5; --surface:#fff; --border:#e0e0e0; --accent:#cc1e1e; --primary:#e07b00; --gold:#b8860b; --orange:#e07b00; --text:#1a1a1a; --text-muted:#666; --text-dim:#999; --radius:12px; }
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
        @media(max-width:1000px) { .main-layout { grid-template-columns:1fr; } .rec-panel { position:static; } }
    </style>
</head>
<body>
<form id="form1" runat="server">

    <nav>
        <a href="MMHome.aspx" style="display:flex;align-items:center;margin-right:16px;flex-shrink:0;background:#fff;border-radius:6px;padding:3px 8px;"><img src="Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" style="height:28px;width:auto;object-fit:contain;" onerror="this.style.display='none'" /></a>
        <a href="/ERPHome.aspx" class="nav-item">&#x2302; ERP Home</a>
        <span class="nav-sep">&rsaquo;</span>
        <a href="MMHome.aspx" class="nav-item">Home</a>
        <span class="nav-sep">&rsaquo;</span>
        <span class="nav-item active">Consumable Inward</span>
        <div class="nav-right">
            <span class="nav-user"><asp:Label ID="lblNavUser" runat="server" /></span>
            <a href="MMLogout.aspx" class="nav-logout" onclick="return confirm('Sign out?')">Sign Out</a>
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
                <div class="form-grid">
                    <div class="form-group">
                        <label>Consumable Item <span class="req">*</span></label>
                        <asp:DropDownList ID="ddlItem" runat="server" onchange="onItemChange(this)" />
                    </div>
                    <div class="form-group">
                        <label>Supplier <span class="req">*</span></label>
                        <asp:DropDownList ID="ddlSupplier" runat="server" onchange="onSupplierChange(this.value)" />
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
                            <asp:TextBox ID="txtQtyUOM" runat="server" placeholder="0" style="flex:1;border-color:rgba(224,123,0,0.4);background:#fff8f0;" />
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
                    <asp:Button ID="btnReceive" runat="server" Text="Receive Goods" CssClass="btn btn-receive" OnClick="btnReceive_Click" OnClientClick="return syncAmounts()" />
                    <asp:Button ID="btnReject"  runat="server" Text="Reject Goods"  CssClass="btn btn-reject"  OnClick="btnReject_Click"  CausesValidation="false" OnClientClick="return confirm('Reject and discard this GRN entry?')" />
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
                            <tr>
                                <td><span class="grn-no"><%# Eval("GRNNo") %></span></td>
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

    function onItemChange(sel) {
        var d = itemData[sel.value];
        if (d) {
            document.getElementById('<%= txtHSN.ClientID %>').value     = d.hsn || '';
            document.getElementById('<%= txtGSTRate.ClientID %>').value = d.gst || '';
            document.getElementById('uomHint').innerText = 'Stock UOM: ' + d.uom;
            var stdDdl = document.getElementById('<%= ddlStdUOM.ClientID %>');
            for (var i = 0; i < stdDdl.options.length; i++) {
                if (stdDdl.options[i].text === d.uom) { stdDdl.selectedIndex = i; break; }
            }
        } else {
            document.getElementById('uomHint').innerText = 'Select item to auto-fill UOM';
        }
        calcAll();
    }

    function syncReceivedUOM(sel) {
        document.getElementById('<%= ddlReceivedUOM.ClientID %>').value = sel.value;
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

        var invoiceAmt = qtyInv * rate;
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
</script>
</body>
</html>
