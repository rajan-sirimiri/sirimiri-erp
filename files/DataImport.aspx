<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DataImport.aspx.cs" Inherits="StockApp.DataImport" %>
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <title>Sirimiri — Data Import</title>
    <link rel="preconnect" href="https://fonts.googleapis.com"/>
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin/>
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
    <style>
        :root { --accent:#C0392B; --accent-dark:#a93226; --bg:#f0f0f0; --surface:#fff;
                --border:#e0e0e0; --text:#1a1a1a; --text-muted:#666; --radius:14px; }
        * { box-sizing:border-box; margin:0; padding:0; }
        body { background:var(--bg); font-family:'DM Sans',sans-serif; min-height:100vh; }

        /* ── NAV ── */
        nav { background:var(--accent); display:flex; align-items:center;
              padding:0 24px; height:52px; gap:12px; }
        .nav-group { position:relative; }
        .nav-item { color:#fff; font-size:13px; font-weight:600; padding:8px 14px;
                    border-radius:6px; cursor:pointer; display:flex; align-items:center; gap:6px;
                    text-decoration:none; }
        .nav-item:hover { background:rgba(255,255,255,.15); }
        .nav-dropdown { display:none; position:absolute; top:100%; left:0;
                        background:#fff; border-radius:8px; min-width:220px;
                        box-shadow:0 4px 20px rgba(0,0,0,.15); z-index:999; overflow:hidden; }
        .nav-group:hover .nav-dropdown { display:block; }
        .nav-dropdown a { display:block; padding:10px 16px; font-size:13px;
                          color:var(--text); text-decoration:none; }
        .nav-dropdown a:hover { background:var(--bg); color:var(--accent); }
        .nav-right { margin-left:auto; display:flex; align-items:center; gap:20px; font-size:13px; }
        .nav-right a { color:#fff; font-weight:700; text-decoration:none; opacity:.9; }
        .nav-right .btn-signout { border:1.5px solid rgba(255,255,255,.6);
                                   padding:5px 14px; border-radius:6px; }
        .user-label { color:#fff; opacity:.9; font-weight:500; }

        /* ── LOGO BAR ── */
        .logo-area { background:#fff; display:flex; align-items:center;
                     justify-content:space-between; padding:16px 24px 0;
                     border-bottom:none; }
        .logo-area img { height:72px; object-fit:contain;
                         filter:drop-shadow(0 2px 8px rgba(204,30,30,.20)); }
        .bis-label { font-family:'Bebas Neue',cursive; font-size:22px;
                     letter-spacing:.12em; color:var(--text); text-align:center; line-height:1.25; }
        .accent-bar { height:4px; background:linear-gradient(90deg,var(--accent-dark),#e63030,var(--accent-dark)); }

        /* ── PAGE LAYOUT ── */
        .page-wrap { max-width:900px; margin:32px auto; padding:0 20px 60px; }
        .page-title { font-family:'Bebas Neue',cursive; font-size:32px;
                      letter-spacing:.08em; color:var(--text); margin-bottom:6px; }
        .page-sub { font-size:13px; color:var(--text-muted); margin-bottom:28px; }

        /* ── CARDS ── */
        .import-card { background:var(--surface); border-radius:var(--radius);
                       box-shadow:0 2px 16px rgba(0,0,0,.08); margin-bottom:24px;
                       overflow:hidden; }
        .import-card-header { background:var(--accent); padding:14px 24px;
                              display:flex; align-items:center; gap:12px; }
        .import-card-header .icon { font-size:22px; }
        .import-card-header h2 { font-family:'Bebas Neue',cursive; font-size:20px;
                                  letter-spacing:.08em; color:#fff; }
        .import-card-header .badge { margin-left:auto; background:rgba(255,255,255,.2);
                                      color:#fff; font-size:11px; font-weight:600;
                                      padding:3px 10px; border-radius:20px; }
        .import-card-body { padding:24px; }

        /* ── UPLOAD ZONE ── */
        .upload-zone { border:2px dashed var(--border); border-radius:10px;
                       padding:28px; text-align:center; margin-bottom:16px;
                       transition:border-color .2s; cursor:pointer; }
        .upload-zone:hover { border-color:var(--accent); }
        .upload-zone .upload-icon { font-size:36px; margin-bottom:8px; }
        .upload-zone p { font-size:13px; color:var(--text-muted); margin-bottom:12px; }
        .upload-zone strong { color:var(--text); }

        /* ── FILE INPUT ── */
        .file-input-wrap { display:inline-block; }
        .file-input-wrap input[type=file] { display:none; }
        .file-label { display:inline-block; padding:9px 20px; background:var(--bg);
                      border:1.5px solid var(--border); border-radius:8px;
                      font-size:13px; font-weight:600; cursor:pointer; color:var(--text); }
        .file-label:hover { border-color:var(--accent); color:var(--accent); }
        .file-selected { margin-top:8px; font-size:12px; color:var(--accent);
                         font-weight:600; display:none; }

        /* ── COLUMN MAP ── */
        .col-map { background:var(--bg); border-radius:8px; padding:16px;
                   margin-bottom:16px; display:none; }
        .col-map h4 { font-size:12px; font-weight:700; letter-spacing:.1em;
                      text-transform:uppercase; color:var(--text-muted); margin-bottom:12px; }
        .col-map-grid { display:grid; grid-template-columns:1fr 1fr; gap:10px; }
        .col-map-row { display:flex; align-items:center; gap:8px; font-size:13px; }
        .col-map-row label { color:var(--text-muted); min-width:100px; font-size:12px; }
        .col-map-row select { flex:1; padding:6px 10px; border:1.5px solid var(--border);
                               border-radius:6px; font-size:12px; font-family:inherit; }

        /* ── BUTTONS ── */
        .btn-import { width:100%; padding:13px; background:var(--accent); color:#fff;
                      border:none; border-radius:9px; font-size:14px; font-weight:700;
                      font-family:inherit; cursor:pointer; letter-spacing:.05em;
                      transition:background .2s; }
        .btn-import:hover { background:var(--accent-dark); }
        .btn-import:disabled { background:#ccc; cursor:not-allowed; }

        /* ── RESULTS ── */
        .result-box { border-radius:10px; padding:20px 24px; margin-top:16px; display:none; }
        .result-box.success { background:#f0faf4; border:1.5px solid #27ae60; }
        .result-box.error   { background:#fdf0f0; border:1.5px solid var(--accent); }
        .result-box h3 { font-size:15px; font-weight:700; margin-bottom:12px; }
        .result-box.success h3 { color:#27ae60; }
        .result-box.error   h3 { color:var(--accent); }
        .stat-row { display:flex; gap:24px; flex-wrap:wrap; }
        .stat-item { text-align:center; }
        .stat-item .num { font-family:'Bebas Neue',cursive; font-size:36px;
                           line-height:1; }
        .stat-item .lbl { font-size:11px; font-weight:600; letter-spacing:.1em;
                           text-transform:uppercase; color:var(--text-muted); }
        .stat-item.green .num { color:#27ae60; }
        .stat-item.red   .num { color:var(--accent); }
        .stat-item.grey  .num { color:#999; }

        /* ── SPINNER ── */
        .spinner { display:none; text-align:center; padding:16px; }
        .spinner-ring { width:36px; height:36px; border:4px solid var(--border);
                        border-top-color:var(--accent); border-radius:50%;
                        animation:spin .8s linear infinite; display:inline-block; }
        @keyframes spin { to { transform:rotate(360deg); } }

        /* ── HISTORY ── */
        .history-table { width:100%; border-collapse:collapse; font-size:13px; }
        .history-table th { text-align:left; padding:8px 12px; background:var(--bg);
                            font-size:11px; font-weight:700; letter-spacing:.08em;
                            text-transform:uppercase; color:var(--text-muted); }
        .history-table td { padding:10px 12px; border-bottom:1px solid var(--border); }
        .badge-type { padding:3px 10px; border-radius:20px; font-size:11px; font-weight:700; }
        .badge-sales    { background:#e8f4fd; color:#2980b9; }
        .badge-receipts { background:#e8fdf0; color:#27ae60; }

        @media(max-width:600px) {
            .col-map-grid { grid-template-columns:1fr; }
            .stat-row { gap:16px; }
        }
    </style>
</head>
<body>
<form id="form1" runat="server">

    <!-- NAV -->
    <nav>
        <div class="nav-group">
            <span class="nav-item">Home ▾</span>
            <div class="nav-dropdown">
                <a href="StockEntry.aspx">Distributor Stock Position Entry</a>
                <a href="#">Store Visit Entry</a>
            </div>
        </div>
        <div class="nav-right">
            <asp:Label ID="lblUserInfo" runat="server" CssClass="user-label"/>
            <a href="UserAdmin.aspx">⚙ Users</a>
            <a href="DataImport.aspx" style="border-bottom:2px solid #fff;">⬆ Import</a>
            <a href="Logout.aspx" class="btn-signout"
               onclick="return confirm('Sign out?')">→ Sign Out</a>
        </div>
    </nav>

    <!-- LOGO BAR -->
    <div class="logo-area">
        <img src="Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri Efficient Nutrition"/>
        <div class="bis-label">SIRIMIRI Nutrition Food Products<br/>
            <span style="font-size:14px;color:var(--text-muted);">Business Intelligence System</span>
        </div>
        <div style="width:80px;"></div>
    </div>
    <div class="accent-bar"></div>

    <div class="page-wrap">
        <div class="page-title">Data Import</div>
        <p class="page-sub">Upload Excel files to append new records to the database. Existing records are never overwritten.</p>

        <!-- SALES REPORT CARD -->
        <div class="import-card">
            <div class="import-card-header">
                <span class="icon">📊</span>
                <h2>Sales Report</h2>
                <span class="badge">SalesOrders Table</span>
            </div>
            <div class="import-card-body">
                <div class="upload-zone" onclick="document.getElementById('fileSales').click()">
                    <div class="upload-icon">📁</div>
                    <p><strong>Click to select</strong> your Sales Report Excel file</p>
                    <p>Expected columns: Date, Customer Name, Voucher No., Quantity, Value</p>
                    <div class="file-input-wrap">
                        <asp:FileUpload ID="fileSales" runat="server" CssClass="file-input"
                            onchange="showFileName(this,'salesFileName','salesColMap','salesCols')"/>
                    </div>
                </div>
                <div id="salesFileName" class="file-selected"></div>

                <!-- Column Mapping -->
                <div id="salesColMap" class="col-map">
                    <h4>Map Columns — Select which column contains each field</h4>
                    <div class="col-map-grid">
                        <div class="col-map-row">
                            <label>Order Date</label>
                            <select id="salesColDate" runat="server"></select>
                        </div>
                        <div class="col-map-row">
                            <label>Distributor Name</label>
                            <select id="salesColName" runat="server"></select>
                        </div>
                        <div class="col-map-row">
                            <label>Invoice / Voucher No</label>
                            <select id="salesColInvoice" runat="server"></select>
                        </div>
                        <div class="col-map-row">
                            <label>Quantity (Units)</label>
                            <select id="salesColQty" runat="server"></select>
                        </div>
                        <div class="col-map-row">
                            <label>Total Value (₹)</label>
                            <select id="salesColValue" runat="server"></select>
                        </div>
                    </div>
                </div>

                <div id="salesSpinner" class="spinner"><div class="spinner-ring"></div></div>

                <asp:Button ID="btnImportSales" runat="server" Text="⬆ IMPORT SALES DATA"
                    CssClass="btn-import" OnClick="btnImportSales_Click"
                    OnClientClick="showSpinner('salesSpinner')"/>

                <!-- Results -->
                <asp:Panel ID="pnlSalesResult" runat="server" Visible="false">
                    <div class="result-box success" style="display:block;">
                        <h3>✅ Import Complete — Sales Report</h3>
                        <div class="stat-row">
                            <div class="stat-item green">
                                <div class="num"><asp:Label ID="lblSalesInserted" runat="server"/></div>
                                <div class="lbl">New Records Added</div>
                            </div>
                            <div class="stat-item grey">
                                <div class="num"><asp:Label ID="lblSalesSkipped" runat="server"/></div>
                                <div class="lbl">Skipped (Already Exist)</div>
                            </div>
                            <div class="stat-item red">
                                <div class="num"><asp:Label ID="lblSalesErrors" runat="server"/></div>
                                <div class="lbl">Errors</div>
                            </div>
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlSalesError" runat="server" Visible="false">
                    <div class="result-box error" style="display:block;">
                        <h3>❌ Import Failed</h3>
                        <asp:Label ID="lblSalesErrMsg" runat="server"/>
                    </div>
                </asp:Panel>
            </div>
        </div>

        <!-- RECEIPTS CARD -->
        <div class="import-card">
            <div class="import-card-header">
                <span class="icon">💳</span>
                <h2>Sales Receipts</h2>
                <span class="badge">ReceiptRegister Table</span>
            </div>
            <div class="import-card-body">
                <div class="upload-zone" onclick="document.getElementById('fileReceipts').click()">
                    <div class="upload-icon">📁</div>
                    <p><strong>Click to select</strong> your Sales Receipts Excel file</p>
                    <p>Expected columns: Date, Particulars, Vch No., Credit</p>
                    <div class="file-input-wrap">
                        <asp:FileUpload ID="fileReceipts" runat="server" CssClass="file-input"
                            onchange="showFileName(this,'receiptsFileName','receiptsColMap','receiptsCols')"/>
                    </div>
                </div>
                <div id="receiptsFileName" class="file-selected"></div>

                <!-- Column Mapping -->
                <div id="receiptsColMap" class="col-map">
                    <h4>Map Columns — Select which column contains each field</h4>
                    <div class="col-map-grid">
                        <div class="col-map-row">
                            <label>Receipt Date</label>
                            <select id="receiptsColDate" runat="server"></select>
                        </div>
                        <div class="col-map-row">
                            <label>Distributor Name</label>
                            <select id="receiptsColName" runat="server"></select>
                        </div>
                        <div class="col-map-row">
                            <label>Voucher No</label>
                            <select id="receiptsColVch" runat="server"></select>
                        </div>
                        <div class="col-map-row">
                            <label>Credit Amount (₹)</label>
                            <select id="receiptsColCredit" runat="server"></select>
                        </div>
                    </div>
                </div>

                <div id="receiptsSpinner" class="spinner"><div class="spinner-ring"></div></div>

                <asp:Button ID="btnImportReceipts" runat="server" Text="⬆ IMPORT RECEIPTS DATA"
                    CssClass="btn-import" OnClick="btnImportReceipts_Click"
                    OnClientClick="showSpinner('receiptsSpinner')"/>

                <!-- Results -->
                <asp:Panel ID="pnlReceiptsResult" runat="server" Visible="false">
                    <div class="result-box success" style="display:block;">
                        <h3>✅ Import Complete — Sales Receipts</h3>
                        <div class="stat-row">
                            <div class="stat-item green">
                                <div class="num"><asp:Label ID="lblReceiptsInserted" runat="server"/></div>
                                <div class="lbl">New Records Added</div>
                            </div>
                            <div class="stat-item grey">
                                <div class="num"><asp:Label ID="lblReceiptsSkipped" runat="server"/></div>
                                <div class="lbl">Skipped (Already Exist)</div>
                            </div>
                            <div class="stat-item red">
                                <div class="num"><asp:Label ID="lblReceiptsErrors" runat="server"/></div>
                                <div class="lbl">Errors</div>
                            </div>
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlReceiptsError" runat="server" Visible="false">
                    <div class="result-box error" style="display:block;">
                        <h3>❌ Import Failed</h3>
                        <asp:Label ID="lblReceiptsErrMsg" runat="server"/>
                    </div>
                </asp:Panel>
            </div>
        </div>
    </div>

</form>
<script>
function showFileName(input, labelId, mapId, colsId) {
    if (input.files && input.files[0]) {
        var label = document.getElementById(labelId);
        label.textContent = '📎 ' + input.files[0].name;
        label.style.display = 'block';
        document.getElementById(mapId).style.display = 'block';
    }
}
function showSpinner(id) {
    document.getElementById(id).style.display = 'block';
}
</script>
</body>
</html>
