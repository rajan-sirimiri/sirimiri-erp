<%@ Page Language="C#" AutoEventWireup="true" Inherits="MMApp.MMBulkUpload" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Bulk Upload &mdash; MM</title>
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet" />
    <style>
        :root { --bg:#f5f5f5; --surface:#fff; --border:#e0e0e0; --accent:#cc1e1e;
                --teal:#1a9e6a; --blue:#1e78cc; --amber:#b8860b;
                --text:#1a1a1a; --text-muted:#666; --text-dim:#999; --radius:12px; }
        *, *::before, *::after { box-sizing:border-box; margin:0; padding:0; }
        body { background:var(--bg); color:var(--text); font-family:'DM Sans',sans-serif; min-height:100vh; }

        nav { background:#1a1a1a; display:flex; align-items:center; padding:0 28px; height:52px; gap:6px; position:sticky; top:0; z-index:100; }
        .nav-brand { font-family:'Bebas Neue',sans-serif; font-size:18px; color:#fff; letter-spacing:.1em; margin-right:20px; }
        .nav-item { color:#aaa; text-decoration:none; font-size:12px; font-weight:600; letter-spacing:.06em; text-transform:uppercase; padding:6px 12px; border-radius:6px; transition:all .2s; }
        .nav-item:hover,.nav-item.active { color:#fff; background:rgba(255,255,255,.08); }
        .nav-sep { color:#444; margin:0 4px; }
        .nav-right { margin-left:auto; display:flex; align-items:center; gap:12px; }
        .nav-user { font-size:12px; color:#888; }
        .nav-logout { font-size:11px; color:#666; text-decoration:none; padding:4px 10px; border:1px solid #333; border-radius:5px; transition:all .2s; }
        .nav-logout:hover { color:var(--accent); border-color:var(--accent); }

        .page-header { background:var(--surface); border-bottom:1px solid var(--border); padding:24px 40px; }
        .page-title { font-family:'Bebas Neue',sans-serif; font-size:28px; letter-spacing:.07em; }
        .page-title span { color:var(--teal); }
        .page-sub { font-size:12px; color:var(--text-muted); margin-top:2px; }

        .content { max-width:1000px; margin:32px auto; padding:0 32px 80px; }

        /* Step cards */
        .steps { display:grid; grid-template-columns:repeat(3,1fr); gap:16px; margin-bottom:32px; }
        .step-card { background:var(--surface); border:1px solid var(--border); border-radius:var(--radius); padding:20px; position:relative; }
        .step-num { font-family:'Bebas Neue',sans-serif; font-size:36px; color:var(--border); line-height:1; margin-bottom:6px; }
        .step-title { font-size:13px; font-weight:700; color:var(--text); margin-bottom:4px; }
        .step-desc { font-size:12px; color:var(--text-muted); line-height:1.5; }

        .card { background:var(--surface); border:1px solid var(--border); border-radius:var(--radius); padding:28px; margin-bottom:24px; }
        .card-title { font-family:'Bebas Neue',sans-serif; font-size:18px; letter-spacing:.07em; color:var(--text); margin-bottom:20px; padding-bottom:12px; border-bottom:2px solid var(--teal); }

        /* Master type selector */
        .master-grid { display:grid; grid-template-columns:repeat(5,1fr); gap:10px; margin-bottom:4px; }
        .master-btn { border:2px solid var(--border); border-radius:10px; padding:14px 8px; text-align:center; cursor:pointer; transition:all .2s; background:var(--bg); }
        .master-btn:hover { border-color:var(--teal); background:#f0faf5; }
        .master-btn.selected { border-color:var(--teal); background:#f0faf5; }
        .master-btn .m-icon { font-size:22px; margin-bottom:6px; }
        .master-btn .m-label { font-size:11px; font-weight:700; color:var(--text-muted); line-height:1.3; }
        .master-btn.selected .m-label { color:var(--teal); }
        .master-hidden { display:none; }

        /* Template download */
        .template-section { display:flex; align-items:center; justify-content:space-between; padding:16px 20px; background:#f9fffe; border:1px solid #c3ece0; border-radius:10px; margin-top:16px; }
        .template-info { font-size:13px; color:var(--text-muted); }
        .template-info strong { color:var(--text); display:block; font-size:14px; margin-bottom:2px; }

        /* Upload zone */
        .upload-zone { border:2px dashed var(--border); border-radius:var(--radius); padding:32px; text-align:center; transition:all .2s; background:#fafafa; position:relative; margin-top:16px; }
        .upload-zone:hover { border-color:var(--teal); background:#f0faf5; }
        .upload-zone input[type=file] { position:absolute; inset:0; opacity:0; cursor:pointer; width:100%; height:100%; }
        .upload-icon { font-size:36px; margin-bottom:8px; }
        .upload-text { font-size:13px; color:var(--text-muted); }
        .upload-text strong { color:var(--text); }
        .upload-hint { font-size:11px; color:var(--text-dim); margin-top:4px; }
        .file-chosen { font-size:12px; color:var(--teal); font-weight:600; margin-top:8px; }

        /* Buttons */
        .btn { padding:10px 22px; border-radius:8px; font-family:'DM Sans',sans-serif; font-size:13px; font-weight:700; letter-spacing:.04em; cursor:pointer; border:none; transition:all .2s; }
        .btn-primary  { background:var(--teal); color:#fff; }
        .btn-primary:hover  { background:#147a52; }
        .btn-secondary { background:transparent; border:1.5px solid var(--border); color:var(--text-muted); }
        .btn-secondary:hover { border-color:var(--text-muted); }
        .btn-download { background:var(--blue); color:#fff; font-size:12px; padding:8px 16px; }
        .btn-download:hover { background:#1560a8; }
        .btn-import { background:var(--teal); color:#fff; font-size:13px; }
        .btn-import:hover { background:#147a52; }
        .btn-row { display:flex; gap:10px; margin-top:20px; }

        /* Alert */
        .alert { padding:12px 16px; border-radius:8px; font-size:13px; margin-bottom:20px; }
        .alert-success { background:rgba(26,158,106,.10); color:var(--teal); border:1px solid rgba(26,158,106,.25); }
        .alert-danger  { background:rgba(204,30,30,.08); color:var(--accent); border:1px solid rgba(204,30,30,.2); }
        .alert-info    { background:rgba(30,120,204,.08); color:var(--blue); border:1px solid rgba(30,120,204,.2); }

        /* Preview table */
        .preview-wrap { overflow-x:auto; max-height:400px; overflow-y:auto; margin-top:16px; border:1px solid var(--border); border-radius:10px; }
        .preview-table { width:100%; border-collapse:collapse; font-size:12px; }
        .preview-table th { padding:9px 12px; text-align:left; font-size:10px; font-weight:700; letter-spacing:.09em; text-transform:uppercase; color:var(--text-dim); background:#fafafa; border-bottom:1px solid var(--border); position:sticky; top:0; }
        .preview-table td { padding:9px 12px; border-bottom:1px solid #f0f0f0; vertical-align:top; }
        .preview-table tr:last-child td { border-bottom:none; }
        .preview-table tr:hover td { background:#fafafa; }
        .row-ok   { background:rgba(26,158,106,.04); }
        .row-skip { background:rgba(204,30,30,.04); }
        .row-skip td { color:#999; text-decoration:line-through; }
        .badge-ok   { display:inline-block; padding:2px 8px; border-radius:20px; font-size:10px; font-weight:700; background:rgba(26,158,106,.12); color:var(--teal); }
        .badge-skip { display:inline-block; padding:2px 8px; border-radius:20px; font-size:10px; font-weight:700; background:rgba(204,30,30,.10); color:var(--accent); }
        .badge-err  { display:inline-block; padding:2px 8px; border-radius:20px; font-size:10px; font-weight:700; background:rgba(184,134,11,.12); color:var(--amber); }

        /* Summary bar */
        .summary-bar { display:flex; gap:24px; padding:14px 20px; background:#f9fffe; border:1px solid #c3ece0; border-radius:10px; margin-bottom:16px; }
        .summary-item { text-align:center; }
        .summary-num   { font-family:'Bebas Neue',sans-serif; font-size:28px; line-height:1; }
        .summary-label { font-size:10px; font-weight:700; text-transform:uppercase; letter-spacing:.08em; color:var(--text-muted); }
        .num-total  { color:var(--text); }
        .num-ok     { color:var(--teal); }
        .num-skip   { color:var(--accent); }
        .num-err    { color:var(--amber); }

        @media(max-width:700px) { .steps{grid-template-columns:1fr} .master-grid{grid-template-columns:repeat(3,1fr)} }
    </style>
</head>
<body>
<form id="form1" runat="server" enctype="multipart/form-data">

    <nav>
        <a href="MMHome.aspx" style="display:flex;align-items:center;margin-right:16px;flex-shrink:0;background:#fff;border-radius:6px;padding:3px 8px;"><img src="Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" style="height:28px;width:auto;object-fit:contain;" onerror="this.style.display='none'" /></a>
        <a href="/ERPHome.aspx" class="nav-item">&#x2302; ERP Home</a>
        <span class="nav-sep">›</span>
        <a href="MMHome.aspx" class="nav-item">Home</a>
        <span class="nav-sep">›</span>
        <span class="nav-item active">Bulk Upload</span>
        <div class="nav-right">
            <span class="nav-user"><asp:Label ID="lblNavUser" runat="server" /></span>
            <a href="MMLogout.aspx" class="nav-logout" onclick="return confirm('Sign out?')">Sign Out</a>
        </div>
    </nav>

    <div class="page-header">
        <div class="page-title">Bulk <span>Upload</span></div>
        <div class="page-sub">Import master data from Excel templates &mdash; Suppliers, Raw Materials, Packing Materials, Consumables, Stationaries</div>
    </div>

    <div class="content">

        <!-- HOW IT WORKS -->
        <div class="steps">
            <div class="step-card">
                <div class="step-num">01</div>
                <div class="step-title">Select Master Type</div>
                <div class="step-desc">Choose which master you want to upload &mdash; Suppliers, Raw Materials, etc.</div>
            </div>
            <div class="step-card">
                <div class="step-num">02</div>
                <div class="step-title">Download Template</div>
                <div class="step-desc">Download the Excel template, fill in your data and save.</div>
            </div>
            <div class="step-card">
                <div class="step-num">03</div>
                <div class="step-title">Upload &amp; Import</div>
                <div class="step-desc">Upload the filled file, preview rows, then click Import to save.</div>
            </div>
        </div>

        <!-- ALERTS -->
        <asp:Panel ID="pnlAlert" runat="server" Visible="false">
            <div class="alert">
                <asp:Label ID="lblAlert" runat="server" />
            </div>
        </asp:Panel>

        <!-- STEP 1 + 2: SELECT MASTER & DOWNLOAD TEMPLATE -->
        <div class="card">
            <div class="card-title">Step 1 &mdash; Select Master Type</div>

            <asp:HiddenField ID="hfMasterType" runat="server" Value="" />
            <asp:Button ID="btnSelectMaster" runat="server" Text="" CssClass="master-hidden"
                OnClick="btnSelectMaster_Click" CausesValidation="false" />

            <div class="master-grid">
                <div class="master-btn" id="mbtn_Supplier" onclick="selectMaster('Supplier',this)">
                    <div class="m-icon">&#x1F3ED;</div>
                    <div class="m-label">Supplier<br/>Registration</div>
                </div>
                <div class="master-btn" id="mbtn_RawMaterial" onclick="selectMaster('RawMaterial',this)">
                    <div class="m-icon">&#x1F33F;</div>
                    <div class="m-label">Raw<br/>Materials</div>
                </div>
                <div class="master-btn" id="mBtn_PackingMaterial" onclick="selectMaster('PackingMaterial',this)">
                    <div class="m-icon">&#x1F4E6;</div>
                    <div class="m-label">Packing<br/>Materials</div>
                </div>
                <div class="master-btn" id="mBtn_Consumable" onclick="selectMaster('Consumable',this)">
                    <div class="m-icon">&#x1F9F9;</div>
                    <div class="m-label">Consumables</div>
                </div>
                <div class="master-btn" id="mBtn_Stationary" onclick="selectMaster('Stationary',this)">
                    <div class="m-icon">&#x1F4DD;</div>
                    <div class="m-label">Stationaries<br/>&amp; Other</div>
                </div>
            </div>

            <asp:Panel ID="pnlTemplateDownload" runat="server" Visible="false">
                <div class="template-section">
                    <div class="template-info">
                        <strong><asp:Label ID="lblTemplateName" runat="server" /></strong>
                        <asp:Label ID="lblTemplateDesc" runat="server" />
                    </div>
                    <asp:Button ID="btnDownloadTemplate" runat="server" Text="&#x2B07; Download Template"
                        CssClass="btn btn-download" OnClick="btnDownloadTemplate_Click" CausesValidation="false" />
                </div>
            </asp:Panel>
        </div>

        <!-- STEP 3: UPLOAD FILE -->
        <asp:Panel ID="pnlUploadSection" runat="server" Visible="false">
            <div class="card">
                <div class="card-title">Step 2 &mdash; Upload Filled Template</div>

                <div class="upload-zone" id="uploadZone">
                    <asp:FileUpload ID="fuExcel" runat="server" accept=".xlsx,.xls" onchange="showFileName(this)" />
                    <div class="upload-icon">&#x1F4C2;</div>
                    <div class="upload-text"><strong>Click or drag &amp; drop</strong> your Excel file here</div>
                    <div class="upload-hint">.xlsx files only &mdash; use the downloaded template</div>
                    <div class="file-chosen" id="fileChosen"></div>
                </div>

                <div class="btn-row">
                    <asp:Button ID="btnPreview" runat="server" Text="Preview Data" CssClass="btn btn-secondary" OnClick="btnPreview_Click" CausesValidation="false" />
                </div>
            </div>
        </asp:Panel>

        <!-- STEP 4: PREVIEW + IMPORT -->
        <asp:Panel ID="pnlPreview" runat="server" Visible="false">
            <div class="card">
                <div class="card-title">Step 3 &mdash; Preview &amp; Import</div>

                <div class="summary-bar">
                    <div class="summary-item">
                        <div class="summary-num num-total"><asp:Label ID="lblTotalRows" runat="server" Text="0" /></div>
                        <div class="summary-label">Total Rows</div>
                    </div>
                    <div class="summary-item">
                        <div class="summary-num num-ok"><asp:Label ID="lblNewRows" runat="server" Text="0" /></div>
                        <div class="summary-label">New (will import)</div>
                    </div>
                    <div class="summary-item">
                        <div class="summary-num num-skip"><asp:Label ID="lblSkipRows" runat="server" Text="0" /></div>
                        <div class="summary-label">Skipped (duplicate)</div>
                    </div>
                    <div class="summary-item">
                        <div class="summary-num num-err"><asp:Label ID="lblErrRows" runat="server" Text="0" /></div>
                        <div class="summary-label">Errors</div>
                    </div>
                </div>

                <div class="preview-wrap">
                    <asp:Literal ID="litPreviewTable" runat="server" />
                </div>

                <div class="btn-row">
                    <asp:Button ID="btnImport" runat="server" Text="Import Records" CssClass="btn btn-import" OnClick="btnImport_Click" CausesValidation="false"
                        OnClientClick="return confirm('Import all valid new rows into the database?')" />
                    <asp:Button ID="btnReset" runat="server" Text="Start Over" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false" />
                </div>
            </div>
        </asp:Panel>

    </div>
</form>

<script>
function selectMaster(type, el) {
    document.querySelectorAll('.master-btn').forEach(function(b){ b.classList.remove('selected'); });
    el.classList.add('selected');
    document.getElementById('hfMasterType').value = type;
    document.getElementById('btnSelectMaster').click();
}

// Restore selected state after postback
window.addEventListener('load', function() {
    var val = document.getElementById('hfMasterType').value;
    if (val) {
        var btn = document.querySelector('[onclick*="' + val + '"]');
        if (btn) btn.classList.add('selected');
    }
});

function showFileName(input) {
    var el = document.getElementById('fileChosen');
    if (input.files && input.files[0]) {
        el.innerText = '✔ ' + input.files[0].name;
    }
}
</script>
</body>
</html>
