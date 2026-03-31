<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Import.aspx.cs" Inherits="DataImport.Import" ResponseEncoding="UTF-8" ContentType="text/html; charset=utf-8" %>
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <title>Sirimiri - Data Import</title>
    <link rel="preconnect" href="https://fonts.googleapis.com"/>
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin/>
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
    <style>
        :root { --accent:#C0392B; --accent-dark:#a93226; --bg:#f0f0f0; --surface:#fff;
                --border:#e0e0e0; --text:#1a1a1a; --text-muted:#666; --radius:14px; }
        * { box-sizing:border-box; margin:0; padding:0; }
        body { background:var(--bg); font-family:'DM Sans',sans-serif; min-height:100vh; }

        nav { background:var(--accent); display:flex; align-items:center;
              padding:0 24px; height:52px; }
        .nav-title { font-family:'Bebas Neue',cursive; font-size:20px;
                     letter-spacing:.1em; color:#fff; }
        .nav-right { margin-left:auto; display:flex; align-items:center; gap:20px; font-size:13px; }
        .nav-right a { color:#fff; font-weight:700; text-decoration:none; opacity:.9; }
        .user-label { color:#fff; opacity:.9; font-weight:500; font-size:13px; }
        .btn-signout { border:1.5px solid rgba(255,255,255,.6); padding:5px 14px; border-radius:6px; }
        .btn-back    { background:rgba(255,255,255,.15); padding:5px 14px; border-radius:6px; }

        .logo-area { background:#fff; display:flex; align-items:center;
                     justify-content:space-between; padding:16px 24px 0; }
        .logo-area img { height:72px; object-fit:contain;
                         filter:drop-shadow(0 2px 8px rgba(204,30,30,.20)); }
        .bis-label { font-family:'Bebas Neue',cursive; font-size:22px;
                     letter-spacing:.12em; color:var(--text); text-align:center; line-height:1.25; }
        .accent-bar { height:4px; background:linear-gradient(90deg,var(--accent-dark),#e63030,var(--accent-dark)); }

        .page-wrap { max-width:860px; margin:32px auto; padding:0 20px 60px; }
        .page-title { font-family:'Bebas Neue',cursive; font-size:32px;
                      letter-spacing:.08em; color:var(--text); margin-bottom:4px; }
        .page-sub { font-size:13px; color:var(--text-muted); margin-bottom:28px; }

        .import-card { background:var(--surface); border-radius:var(--radius);
                       box-shadow:0 2px 16px rgba(0,0,0,.08); margin-bottom:28px; overflow:hidden; }
        .card-head { background:var(--accent); padding:16px 24px;
                     display:flex; align-items:center; gap:12px; }
        .card-head .card-icon { font-size:20px; background:rgba(255,255,255,.2);
                                 width:36px; height:36px; border-radius:8px;
                                 display:flex; align-items:center; justify-content:center;
                                 font-weight:700; color:#fff; }
        .card-head h2 { font-family:'Bebas Neue',cursive; font-size:22px;
                        letter-spacing:.08em; color:#fff; }
        .card-head .badge { margin-left:auto; background:rgba(255,255,255,.2);
                            color:#fff; font-size:11px; font-weight:600;
                            padding:3px 10px; border-radius:20px; }
        .card-body { padding:28px; }

        .upload-zone { border:2px dashed var(--border); border-radius:10px;
                       padding:32px 24px; text-align:center; margin-bottom:20px;
                       cursor:pointer; transition:all .2s; background:var(--bg); }
        .upload-zone:hover { border-color:var(--accent); background:#fdf5f5; }
        .upload-zone .upload-icon { width:56px; height:56px; background:#fff;
                                     border:2px solid var(--border); border-radius:12px;
                                     display:flex; align-items:center; justify-content:center;
                                     margin:0 auto 12px; font-size:24px; }
        .upload-zone strong { font-size:15px; color:var(--text); display:block; margin-bottom:6px; }
        .upload-zone .hint { font-size:13px; color:var(--text-muted); }
        .upload-zone .selected-file { margin-top:10px; font-size:13px;
                                       color:var(--accent); font-weight:600; }
        input[type=file] { display:none; }

        .col-map-panel { background:#fafafa; border:1px solid var(--border);
                         border-radius:10px; padding:20px; margin-bottom:20px; display:none; }
        .col-map-panel h4 { font-size:11px; font-weight:700; letter-spacing:.12em;
                            text-transform:uppercase; color:var(--text-muted); margin-bottom:14px; }
        .col-grid { display:grid; grid-template-columns:1fr 1fr; gap:12px; }
        .col-row { display:flex; flex-direction:column; gap:4px; }
        .col-row label { font-size:11px; font-weight:700; letter-spacing:.08em;
                         text-transform:uppercase; color:var(--text-muted); }
        .col-row select { padding:8px 10px; border:1.5px solid var(--border);
                          border-radius:7px; font-size:13px; font-family:inherit; background:#fff; }
        .col-row select:focus { outline:none; border-color:var(--accent); }

        .btn-import { width:100%; padding:14px; background:var(--accent); color:#fff;
                      border:none; border-radius:9px; font-size:15px; font-weight:700;
                      font-family:'Bebas Neue',cursive; letter-spacing:.1em;
                      cursor:pointer; transition:background .2s; }
        .btn-import:hover { background:var(--accent-dark); }

        .spinner { display:none; text-align:center; padding:20px; }
        .spinner-ring { width:40px; height:40px; border:4px solid var(--border);
                        border-top-color:var(--accent); border-radius:50%;
                        animation:spin .8s linear infinite; display:inline-block; }
        @keyframes spin { to { transform:rotate(360deg); } }

        .result-panel { border-radius:10px; padding:22px 26px; margin-top:20px; }
        .result-panel.ok  { background:#f0faf4; border:1.5px solid #27ae60; }
        .result-panel.err { background:#fdf0f0; border:1.5px solid var(--accent); }
        .result-panel h3 { font-size:15px; font-weight:700; margin-bottom:14px; }
        .result-panel.ok  h3 { color:#27ae60; }
        .result-panel.err h3 { color:var(--accent); }
        .stats { display:flex; gap:32px; flex-wrap:wrap; }
        .stat { text-align:center; }
        .stat .num { font-family:'Bebas Neue',cursive; font-size:42px; line-height:1; }
        .stat .lbl { font-size:11px; font-weight:600; letter-spacing:.1em;
                     text-transform:uppercase; color:var(--text-muted); margin-top:2px; }
        .stat.green  .num { color:#27ae60; }
        .stat.orange .num { color:#e67e22; }
        .stat.red    .num { color:var(--accent); }

        @media(max-width:600px) { .col-grid { grid-template-columns:1fr; } .stats { gap:20px; } }
    </style>
</head>
<body>
<form id="form1" runat="server">

    <nav>
        <span class="nav-title">Data Import Portal</span>
        <div class="nav-right">
            <asp:Label ID="lblUserInfo" runat="server" CssClass="user-label"/>
            <a href="https://vimarsa.in/StockApp/" class="btn-back">Back to StockApp</a>
            <a href="Logout.aspx" class="btn-signout"
               onclick="return confirm('Sign out?')">Sign Out</a>
        </div>
    </nav>

    <div class="logo-area">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri Efficient Nutrition"/>
        <div class="bis-label">SIRIMIRI Nutrition Food Products<br/>
            <span style="font-size:14px;color:var(--text-muted);">Business Intelligence System</span>
        </div>
        <div style="width:80px;"></div>
    </div>
    <div class="accent-bar"></div>

    <div class="page-wrap">
        <div class="page-title">Data Import</div>
        <p class="page-sub">Upload Excel files to append new records. Existing records are never overwritten - duplicates are automatically skipped.</p>

        <!-- SALES REPORT -->
        <div class="import-card">
            <div class="card-head">
                <div class="card-icon">S</div>
                <h2>Sales Report</h2>
                <span class="badge">SalesOrders Table</span>
            </div>
            <div class="card-body">
                <div class="upload-zone" id="salesZone"
                     onclick="document.getElementById('fileSales').click()">
                    <div class="upload-icon">XLS</div>
                    <strong>Click to select Sales Report Excel file</strong>
                    <p class="hint">Columns auto-detected: Date &middot; Customer Name &middot; Voucher No &middot; Quantity &middot; Value</p>
                    <asp:FileUpload ID="fileSales" runat="server" ClientIDMode="Static"
                        onchange="onFileSelected(this,'salesZone','salesColPanel','salesCols')"/>
                    <div id="salesFileLabel" class="selected-file"></div>
                </div>

                <div id="salesColPanel" class="col-map-panel">
                    <h4>Column Mapping - Select which column contains each field</h4>
                    <div class="col-grid">
                        <div class="col-row">
                            <label>Order Date *</label>
                            <select id="salesColDate" runat="server"><option value="">-- Auto Detect --</option></select>
                        </div>
                        <div class="col-row">
                            <label>Distributor / Customer Name *</label>
                            <select id="salesColName" runat="server"><option value="">-- Auto Detect --</option></select>
                        </div>
                        <div class="col-row">
                            <label>Invoice / Voucher No *</label>
                            <select id="salesColInvoice" runat="server"><option value="">-- Auto Detect --</option></select>
                        </div>
                        <div class="col-row">
                            <label>Quantity (Units)</label>
                            <select id="salesColQty" runat="server"><option value="">-- Auto Detect --</option></select>
                        </div>
                        <div class="col-row">
                            <label>Total Value (Rs.)</label>
                            <select id="salesColValue" runat="server"><option value="">-- Auto Detect --</option></select>
                        </div>
                    </div>
                </div>

                <div id="salesSpinner" class="spinner">
                    <div class="spinner-ring"></div>
                    <p style="margin-top:10px;color:var(--text-muted);font-size:13px;">Processing... please wait</p>
                </div>

                <asp:Button ID="btnImportSales" runat="server" Text="IMPORT SALES DATA"
                    CssClass="btn-import" OnClick="btnImportSales_Click"
                    OnClientClick="document.getElementById('salesSpinner').style.display='block';"/>

                <asp:Panel ID="pnlSalesOK" runat="server" Visible="false">
                    <div class="result-panel ok">
                        <h3>Sales Import Complete</h3>
                        <div class="stats">
                            <div class="stat green">
                                <div class="num"><asp:Label ID="lblSalesNew" runat="server"/></div>
                                <div class="lbl">New Records Added</div>
                            </div>
                            <div class="stat orange">
                                <div class="num"><asp:Label ID="lblSalesSkip" runat="server"/></div>
                                <div class="lbl">Skipped (Duplicate)</div>
                            </div>
                            <div class="stat red">
                                <div class="num"><asp:Label ID="lblSalesErr" runat="server"/></div>
                                <div class="lbl">Errors</div>
                            </div>
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlSalesFail" runat="server" Visible="false">
                    <div class="result-panel err">
                        <h3>Sales Import Failed</h3>
                        <asp:Label ID="lblSalesFailMsg" runat="server"/>
                    </div>
                </asp:Panel>
            </div>
        </div>

        <!-- RECEIPTS -->
        <div class="import-card">
            <div class="card-head">
                <div class="card-icon">R</div>
                <h2>Sales Receipts</h2>
                <span class="badge">ReceiptRegister Table</span>
            </div>
            <div class="card-body">
                <div class="upload-zone" id="receiptsZone"
                     onclick="document.getElementById('fileReceipts').click()">
                    <div class="upload-icon">XLS</div>
                    <strong>Click to select Sales Receipts Excel file</strong>
                    <p class="hint">Columns auto-detected: Date &middot; Particulars &middot; Vch No &middot; Credit</p>
                    <asp:FileUpload ID="fileReceipts" runat="server" ClientIDMode="Static"
                        onchange="onFileSelected(this,'receiptsZone','receiptsColPanel','receiptsCols')"/>
                    <div id="receiptsFileLabel" class="selected-file"></div>
                </div>

                <div id="receiptsColPanel" class="col-map-panel">
                    <h4>Column Mapping - Select which column contains each field</h4>
                    <div class="col-grid">
                        <div class="col-row">
                            <label>Receipt Date *</label>
                            <select id="receiptsColDate" runat="server"><option value="">-- Auto Detect --</option></select>
                        </div>
                        <div class="col-row">
                            <label>Distributor / Particulars *</label>
                            <select id="receiptsColName" runat="server"><option value="">-- Auto Detect --</option></select>
                        </div>
                        <div class="col-row">
                            <label>Voucher No *</label>
                            <select id="receiptsColVch" runat="server"><option value="">-- Auto Detect --</option></select>
                        </div>
                        <div class="col-row">
                            <label>Credit Amount (Rs.)</label>
                            <select id="receiptsColCredit" runat="server"><option value="">-- Auto Detect --</option></select>
                        </div>
                    </div>
                </div>

                <div id="receiptsSpinner" class="spinner">
                    <div class="spinner-ring"></div>
                    <p style="margin-top:10px;color:var(--text-muted);font-size:13px;">Processing... please wait</p>
                </div>

                <asp:Button ID="btnImportReceipts" runat="server" Text="IMPORT RECEIPTS DATA"
                    CssClass="btn-import" OnClick="btnImportReceipts_Click"
                    OnClientClick="document.getElementById('receiptsSpinner').style.display='block';"/>

                <asp:Panel ID="pnlReceiptsOK" runat="server" Visible="false">
                    <div class="result-panel ok">
                        <h3>Receipts Import Complete</h3>
                        <div class="stats">
                            <div class="stat green">
                                <div class="num"><asp:Label ID="lblReceiptsNew" runat="server"/></div>
                                <div class="lbl">New Records Added</div>
                            </div>
                            <div class="stat orange">
                                <div class="num"><asp:Label ID="lblReceiptsSkip" runat="server"/></div>
                                <div class="lbl">Skipped (Duplicate)</div>
                            </div>
                            <div class="stat red">
                                <div class="num"><asp:Label ID="lblReceiptsErr" runat="server"/></div>
                                <div class="lbl">Errors</div>
                            </div>
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlReceiptsFail" runat="server" Visible="false">
                    <div class="result-panel err">
                        <h3>Receipts Import Failed</h3>
                        <asp:Label ID="lblReceiptsFailMsg" runat="server"/>
                    </div>
                </asp:Panel>
            </div>
        </div>

    </div>
</form>
<script>
function onFileSelected(input, zoneId, panelId, colsId) {
    if (!input.files || !input.files[0]) return;
    var zone  = document.getElementById(zoneId);
    var label = zone.querySelector('.selected-file');
    label.textContent = 'Selected: ' + input.files[0].name;
    document.getElementById(panelId).style.display = 'block';
    zone.style.borderColor = '#C0392B';
    zone.style.background  = '#fdf5f5';
}
</script>
</body>
</html>
