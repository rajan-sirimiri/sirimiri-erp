<%@ Page Language="C#" AutoEventWireup="true" Inherits="PPApp.PPProductModel" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Product Modelling &mdash; PP</title>
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
    <style>
        :root {
            --bg:#f4f5f7; --surface:#ffffff; --border:#e2e4e8; --border-light:#f0f1f3;
            --accent:#1a7a4a; --accent-dark:#145e38; --accent-light:rgba(26,122,74,0.10);
            --blue:#1e78cc; --blue-light:rgba(30,120,204,0.10);
            --amber:#b8860b; --amber-light:rgba(184,134,11,0.10);
            --red:#cc1e1e; --red-light:rgba(204,30,30,0.08);
            --text:#1a1a1a; --text-muted:#606878; --text-dim:#9aa0ac;
            --radius:10px; --radius-sm:6px;
        }
        *, *::before, *::after { box-sizing:border-box; margin:0; padding:0; }
        body { background:var(--bg); color:var(--text); font-family:'DM Sans',sans-serif; min-height:100vh; font-size:14px; }

        /* ── NAV ── */
        nav { background:#1a1a1a; display:flex; align-items:center; padding:0 24px; height:50px; gap:4px; position:sticky; top:0; z-index:200; }
        .nav-brand { font-family:'Bebas Neue',sans-serif; font-size:17px; color:#fff; letter-spacing:.1em; margin-right:16px; }
        .nav-item { color:#999; text-decoration:none; font-size:11px; font-weight:700; letter-spacing:.07em; text-transform:uppercase; padding:5px 10px; border-radius:5px; transition:all .2s; }
        .nav-item:hover,.nav-item.active { color:#fff; background:rgba(255,255,255,.08); }
        .nav-sep { color:#333; margin:0 2px; font-size:13px; }
        .nav-right { margin-left:auto; display:flex; align-items:center; gap:10px; }
        .nav-user { font-size:11px; color:#777; }
        .nav-logout { font-size:11px; color:#666; text-decoration:none; padding:4px 10px; border:1px solid #333; border-radius:5px; transition:all .2s; }
        .nav-logout:hover { color:var(--accent); border-color:var(--accent); }

        /* ── PAGE HEADER ── */
        .page-header { background:var(--surface); border-bottom:2px solid var(--accent); padding:18px 28px; display:flex; align-items:center; gap:14px; }
        .page-header-icon { width:40px; height:40px; border-radius:9px; background:var(--accent-light); display:flex; align-items:center; justify-content:center; font-size:18px; flex-shrink:0; }
        .page-title { font-family:'Bebas Neue',sans-serif; font-size:24px; letter-spacing:.07em; line-height:1; }
        .page-title span { color:var(--accent); }
        .page-sub { font-size:11px; color:var(--text-muted); margin-top:2px; }

        /* ── THREE PANEL LAYOUT ── */
        .workspace {
            display:grid;
            grid-template-columns: 420px 1fr 300px;
            gap:0;
            height:calc(100vh - 114px);
            overflow:hidden;
        }

        /* ── PANEL SHARED ── */
        .panel { display:flex; flex-direction:column; overflow:hidden; border-right:1px solid var(--border); }
        .panel:last-child { border-right:none; }
        .panel-head {
            padding:14px 18px 12px;
            border-bottom:1px solid var(--border);
            background:var(--surface);
            flex-shrink:0;
        }
        .panel-head-row { display:flex; align-items:center; justify-content:space-between; }
        .panel-label { font-size:9px; font-weight:700; letter-spacing:.14em; text-transform:uppercase; color:var(--text-dim); margin-bottom:2px; }
        .panel-title { font-family:'Bebas Neue',sans-serif; font-size:19px; letter-spacing:.06em; }
        .panel-title span { color:var(--accent); }
        .panel-body { flex:1; overflow-y:auto; padding:16px 18px; background:var(--bg); }
        .panel-body::-webkit-scrollbar { width:4px; }
        .panel-body::-webkit-scrollbar-thumb { background:#ddd; border-radius:4px; }

        /* ── ALERT ── */
        .alert { padding:10px 14px; border-radius:var(--radius-sm); font-size:12px; margin-bottom:14px; }
        .alert-success { background:rgba(26,122,74,.10); color:var(--accent); border:1px solid rgba(26,122,74,.25); }
        .alert-danger  { background:var(--red-light); color:var(--red); border:1px solid rgba(204,30,30,.2); }

        /* ── FORM ELEMENTS ── */
        .form-group { margin-bottom:13px; }
        .form-group-row { display:grid; grid-template-columns:1fr 1fr; gap:10px; margin-bottom:13px; }
        label { display:block; font-size:10px; font-weight:700; letter-spacing:.07em; text-transform:uppercase; color:var(--text-muted); margin-bottom:4px; }
        label .req { color:var(--red); margin-left:2px; }
        input[type=text], input[type=number], select, textarea {
            width:100%; padding:8px 11px; border:1.5px solid var(--border);
            border-radius:var(--radius-sm); font-family:'DM Sans',sans-serif;
            font-size:13px; color:var(--text); background:#fafafa;
            transition:border-color .2s, background .2s; outline:none;
        }
        input:focus, select:focus, textarea:focus { border-color:var(--accent); background:#fff; box-shadow:0 0 0 3px var(--accent-light); }
        input[readonly] { background:#f0f1f3; color:var(--text-muted); cursor:not-allowed; }
        input[readonly]:focus { border-color:var(--border); box-shadow:none; }
        textarea { resize:vertical; min-height:60px; }
        select option { padding:6px; }
        .field-hint { font-size:10px; color:var(--text-dim); margin-top:2px; }

        /* ── SECTION DIVIDER ── */
        .section-div { font-size:9px; font-weight:700; letter-spacing:.13em; text-transform:uppercase; color:var(--text-dim); margin:16px 0 10px; display:flex; align-items:center; gap:8px; }
        .section-div::after { content:''; flex:1; height:1px; background:var(--border); }

        /* ── BUTTONS ── */
        .btn { padding:9px 18px; border-radius:var(--radius-sm); font-family:'DM Sans',sans-serif; font-size:12px; font-weight:700; letter-spacing:.04em; cursor:pointer; border:none; transition:all .2s; }
        .btn-sm { padding:6px 12px; font-size:11px; }
        .btn-primary  { background:var(--accent); color:#fff; }
        .btn-primary:hover  { background:var(--accent-dark); }
        .btn-secondary { background:transparent; border:1.5px solid var(--border); color:var(--text-muted); }
        .btn-secondary:hover { border-color:var(--text-muted); color:var(--text); }
        .btn-danger   { background:transparent; border:1.5px solid #ffcccc; color:var(--red); }
        .btn-danger:hover { background:#fff5f5; }
        .btn-blue     { background:var(--blue); color:#fff; }
        .btn-blue:hover { background:#1560a8; }
        .btn-amber    { background:var(--amber); color:#fff; }
        .btn-amber:hover { background:#8a6408; }
        .btn-row { display:flex; gap:8px; margin-top:16px; flex-wrap:wrap; }

        /* ── PRODUCT IMAGE UPLOAD ── */
        .img-upload-box {
            border:2px dashed var(--border); border-radius:var(--radius);
            padding:16px; text-align:center; cursor:pointer;
            transition:border-color .2s, background .2s; background:#fafafa;
            position:relative; overflow:hidden;
        }
        .img-upload-box:hover { border-color:var(--accent); background:#f0faf4; }
        .img-upload-box input[type=file] { position:absolute; inset:0; opacity:0; cursor:pointer; width:100%; height:100%; }
        .img-preview { width:100%; max-height:140px; object-fit:contain; border-radius:6px; display:none; }
        .img-placeholder { font-size:28px; margin-bottom:4px; }
        .img-placeholder-text { font-size:11px; color:var(--text-dim); }

        /* ── PRODUCT LIST (panel 1 bottom) ── */
        .prod-list { margin-top:0; }
        .prod-list-header { display:flex; align-items:center; justify-content:space-between; padding:10px 18px; background:var(--surface); border-bottom:1px solid var(--border); border-top:2px solid var(--border); flex-shrink:0; }
        .prod-list-title { font-family:'Bebas Neue',sans-serif; font-size:16px; letter-spacing:.06em; }
        .prod-count { font-size:10px; color:var(--text-dim); background:var(--bg); padding:2px 8px; border-radius:20px; border:1px solid var(--border); }
        .search-box { padding:8px 12px; border-bottom:1px solid var(--border); background:var(--surface); flex-shrink:0; }
        .search-box input { padding:7px 10px; font-size:12px; border-radius:var(--radius-sm); }
        .prod-list-scroll { flex:1; overflow-y:auto; }
        .prod-list-scroll::-webkit-scrollbar { width:3px; }
        .prod-list-scroll::-webkit-scrollbar-thumb { background:#ddd; }
        .prod-row { padding:10px 18px; border-bottom:1px solid var(--border-light); cursor:pointer; transition:background .15s; display:flex; align-items:center; gap:10px; }
        .prod-row:hover { background:#f0faf4; }
        .prod-row.selected { background:var(--accent-light); border-left:3px solid var(--accent); }
        .prod-row-img { width:36px; height:36px; border-radius:6px; object-fit:cover; background:#f0f0f0; display:flex; align-items:center; justify-content:center; font-size:16px; flex-shrink:0; overflow:hidden; }
        .prod-row-img img { width:100%; height:100%; object-fit:cover; }
        .prod-code { font-size:10px; font-weight:700; color:var(--text-dim); }
        .prod-name { font-size:13px; font-weight:500; color:var(--text); }
        .prod-type-badge { font-size:9px; font-weight:700; padding:2px 6px; border-radius:10px; }
        .type-core     { background:var(--accent-light); color:var(--accent); }
        .type-assembled { background:var(--blue-light); color:var(--blue); }
        .type-conversion { background:#fff3e0; color:#e65100; }
        .badge-active   { font-size:9px; font-weight:700; padding:2px 7px; border-radius:10px; background:var(--accent-light); color:var(--accent); }
        .badge-inactive { font-size:9px; font-weight:700; padding:2px 7px; border-radius:10px; background:#f0f0f0; color:var(--text-dim); }

        /* ── BOM PANEL (panel 2) ── */
        .bom-product-banner {
            background:var(--surface); border:1px solid var(--border);
            border-radius:var(--radius); padding:12px 14px; margin-bottom:14px;
            display:flex; align-items:center; gap:12px;
        }
        .bom-product-banner-icon { font-size:22px; }
        .bom-product-name { font-family:'Bebas Neue',sans-serif; font-size:17px; letter-spacing:.05em; }
        .bom-product-meta { font-size:11px; color:var(--text-muted); }
        .bom-uom-note { font-size:11px; color:var(--blue); font-weight:600; margin-top:2px; }

        /* Add ingredient row */
        .add-ing-grid { display:grid; grid-template-columns:1fr 80px 100px auto; gap:8px; align-items:end; margin-bottom:10px; }
        .add-ing-grid .btn { height:36px; padding:0 14px; align-self:end; white-space:nowrap; }

        /* BOM table */
        .bom-table { width:100%; border-collapse:collapse; }
        .bom-table th { padding:8px 10px; text-align:left; font-size:9px; font-weight:700; letter-spacing:.10em; text-transform:uppercase; color:var(--text-dim); background:#fafafa; border-bottom:1px solid var(--border); }
        .bom-table td { padding:9px 10px; font-size:12px; border-bottom:1px solid var(--border-light); vertical-align:middle; }
        .bom-table tr:last-child td { border-bottom:none; }
        .bom-table tr:hover td { background:#fafafa; }
        .mat-type-pill { font-size:9px; font-weight:700; padding:2px 6px; border-radius:8px; }
        .pill-rm { background:var(--accent-light); color:var(--accent); }
        .pill-pm { background:var(--blue-light); color:var(--blue); }
        .pill-cn { background:var(--amber-light); color:var(--amber); }
        .del-btn { background:none; border:none; cursor:pointer; color:#ccc; font-size:14px; padding:2px 6px; transition:color .2s; }
        .del-btn:hover { color:var(--red); }
        .bom-empty { text-align:center; padding:32px 16px; color:var(--text-dim); font-size:12px; }

        /* BOM save strip */
        .bom-save-strip { padding:12px 18px; background:var(--surface); border-top:1px solid var(--border); flex-shrink:0; display:flex; align-items:center; justify-content:space-between; }
        .bom-count-note { font-size:11px; color:var(--text-muted); }

        /* ── COST PANEL (panel 3) ── */
        .cost-card { background:var(--surface); border:1px solid var(--border); border-radius:var(--radius); padding:14px; margin-bottom:14px; }
        .cost-card-title { font-size:10px; font-weight:700; letter-spacing:.10em; text-transform:uppercase; color:var(--text-dim); margin-bottom:12px; padding-bottom:8px; border-bottom:1px solid var(--border); }
        .cost-row { display:flex; align-items:center; justify-content:space-between; margin-bottom:10px; }
        .cost-label { font-size:12px; color:var(--text-muted); }
        .cost-value { font-size:14px; font-weight:600; color:var(--text); font-variant-numeric:tabular-nums; }
        .cost-value.big { font-family:'Bebas Neue',sans-serif; font-size:22px; letter-spacing:.04em; color:var(--accent); }
        .cost-value.highlight { color:var(--blue); }
        .cost-divider { height:1px; background:var(--border); margin:10px 0; }
        .cost-input-group { margin-bottom:10px; }

        .no-product-state { text-align:center; padding:40px 16px; color:var(--text-dim); }
        .no-product-state .big-icon { font-size:40px; margin-bottom:10px; }
        .no-product-state p { font-size:12px; line-height:1.6; }

        /* ── HIDDEN FILE INPUT WORKAROUND ── */
        .file-input-hidden { display:none; }

        @media(max-width:1100px) {
            .workspace { grid-template-columns:360px 1fr 260px; }
        }
        @media(max-width:860px) {
            .workspace { grid-template-columns:1fr; height:auto; overflow:visible; }
            .panel { border-right:none; border-bottom:1px solid var(--border); }
        }
        .packing-spec-panel{margin-top:18px;background:#fff8e1;border:1px solid #ffe082;border-radius:10px;padding:14px 16px;}
        .packing-spec-title{font-size:11px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:#f57f17;margin-bottom:12px;}
        #divContainerRows{display:none;}
        .params-panel{margin-top:18px;background:#e8f5e9;border:1px solid #a5d6a7;border-radius:10px;padding:14px 16px;}
        .params-title{font-size:11px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:#2e7d32;margin-bottom:12px;}
        .param-row{display:grid;grid-template-columns:140px 1fr 1fr 28px;gap:8px;align-items:center;margin-bottom:8px;}
        .param-row select,.param-row input{padding:7px 10px;border:1.5px solid var(--border);border-radius:7px;font-family:inherit;font-size:12px;background:#fff;outline:none;}
        .param-row select:focus,.param-row input:focus{border-color:#4caf50;}
        .btn-remove-param{background:none;border:none;color:#e74c3c;font-size:16px;cursor:pointer;padding:0;line-height:1;}
        .btn-add-param{background:none;border:1.5px dashed #a5d6a7;color:#2e7d32;font-size:12px;font-weight:700;padding:6px 14px;border-radius:7px;cursor:pointer;margin-top:4px;}
        .btn-add-param:hover{background:#e8f5e9;}
        .global-settings-bar{background:var(--surface);border-bottom:1px solid var(--border);padding:8px 24px;display:flex;align-items:center;gap:10px;}
        .btn-global-settings{background:none;border:1.5px solid var(--border);border-radius:7px;padding:5px 14px;font-size:11px;font-weight:700;letter-spacing:.05em;text-transform:uppercase;cursor:pointer;color:var(--text-muted);transition:all .2s;}
        .btn-global-settings:hover,.btn-global-settings.active{background:#1a1a1a;color:#fff;border-color:#1a1a1a;}
        .global-settings-panel{display:none;background:var(--bg);border-bottom:2px solid var(--accent);padding:20px 28px;}
        .global-settings-panel.open{display:block;}
        .gs-title{font-family:"Bebas Neue",sans-serif;font-size:16px;letter-spacing:.07em;margin-bottom:14px;color:var(--text);}
        .gs-title span{color:var(--accent);}
        .remark-list{display:flex;flex-direction:column;gap:6px;max-width:480px;margin-bottom:12px;}
        .remark-item{display:flex;gap:8px;align-items:center;}
        .remark-item input{flex:1;padding:7px 11px;border:1.5px solid var(--border);border-radius:7px;font-family:inherit;font-size:13px;background:#fff;outline:none;}
        .remark-item input:focus{border-color:var(--accent);}
        .btn-remove-remark{background:none;border:none;color:#e74c3c;font-size:16px;cursor:pointer;line-height:1;}
        .btn-add-remark{background:none;border:1.5px dashed #ccc;color:var(--text-muted);font-size:12px;font-weight:700;padding:5px 14px;border-radius:7px;cursor:pointer;}
        .btn-add-remark:hover{border-color:var(--accent);color:var(--accent);}
        .btn-save-remarks{background:var(--accent);color:#fff;border:none;border-radius:7px;padding:8px 20px;font-size:12px;font-weight:700;cursor:pointer;letter-spacing:.04em;}
        .btn-save-remarks:hover{background:var(--accent-dark);}
    </style>
</head>
<body>
<form id="form1" runat="server" enctype="multipart/form-data">

    <nav>
        <a href="PPHome.aspx" style="display:flex;align-items:center;margin-right:16px;flex-shrink:0;background:#fff;border-radius:6px;padding:3px 8px;"><img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" style="height:28px;width:auto;object-fit:contain;" onerror="this.style.display='none'" /></a>
        <a href="/StockApp/ERPHome.aspx" class="nav-item">&#x2302; ERP</a>
        <span class="nav-sep">›</span>
        <a href="PPHome.aspx" class="nav-item">Home</a>
        <span class="nav-sep">›</span>
        <span class="nav-item active">Product Modelling</span>
        <div class="nav-right">
            <span class="nav-user"><asp:Label ID="lblNavUser" runat="server" /></span>
            <a href="#" class="nav-logout" onclick="erpConfirm('Sign out?',{title:'Sign Out',type:'warn',okText:'Sign Out',onOk:function(){window.location='PPLogout.aspx';}});return false;">Sign Out</a>
        </div>
    </nav>

    <!-- GLOBAL SETTINGS BAR -->
    <div class="global-settings-bar">
        <button type="button" class="btn-global-settings" id="btnGlobalSettings"
            onclick="toggleGlobalSettings()">&#9881; Global Settings</button>
        <span style="font-size:11px;color:var(--text-dim);">Configure Remarks options and other global settings</span>
    </div>

    <!-- GLOBAL SETTINGS PANEL -->
    <div class="global-settings-panel" id="globalSettingsPanel">
        <div style="display:flex;gap:48px;flex-wrap:wrap;">
            <div>
                <div class="gs-title">Batch Remarks <span>Options</span></div>
                <div style="font-size:11px;color:var(--text-muted);margin-bottom:12px;">Shown as dropdown in Batch Execution after each batch</div>
                <asp:Panel ID="pnlRemarksAlert" runat="server" Visible="false">
                    <div class="alert" style="margin-bottom:10px;"><asp:Label ID="lblRemarksAlert" runat="server"/></div>
                </asp:Panel>
                <div class="remark-list" id="remarkList"></div>
                <div style="display:flex;gap:8px;align-items:center;margin-top:8px;">
                    <button type="button" class="btn-add-remark" onclick="addRemarkRow()">+ Add Option</button>
                    <asp:Button ID="btnSaveRemarks" runat="server" Text="&#x2713; Save Remarks"
                        CssClass="btn-save-remarks" OnClick="btnSaveRemarks_Click"
                        CausesValidation="false" OnClientClick="collectRemarks();"/>
                </div>
                <asp:HiddenField ID="hfRemarksJson" runat="server" Value="[]"/>
            </div>
        </div>
    </div>

    <div class="page-header">
        <div class="page-header-icon">&#x1F9EA;</div>
        <div>
            <div class="page-title">Product <span>Modelling</span></div>
            <div class="page-sub">Define finished goods, bill of materials and cost structure</div>
        </div>
    </div>

    <div class="workspace">

        <!-- ═══════════════════════════════════════════════
             PANEL 1 — PRODUCT FORM + LIST
        ════════════════════════════════════════════════ -->
        <div class="panel" style="background:var(--bg);">

            <!-- Form area -->
            <div class="panel-head">
                <div class="panel-label">Section 1</div>
                <div class="panel-title">Product <span>Master</span></div>
            </div>
            <div class="panel-body" style="flex:0 0 auto; max-height:calc(100% - 380px); overflow-y:auto;">

                <asp:Panel ID="pnlAlert" runat="server" Visible="false">
                    <div class="alert">
                        <asp:Label ID="lblAlert" runat="server" />
                    </div>
                </asp:Panel>

                <asp:HiddenField ID="hfProductID" runat="server" Value="0" />
                <asp:HiddenField ID="hfImagePath" runat="server" Value="" />

                <div class="form-group">
                    <label>Product Code</label>
                    <asp:TextBox ID="txtCode" runat="server" ReadOnly="true" placeholder="Auto-generated" />
                </div>
                <div class="form-group">
                    <label>Product Name <span class="req">*</span></label>
                    <asp:TextBox ID="txtName" runat="server" MaxLength="200" placeholder="e.g. Millet Energy Bar" />
                </div>
                <div class="form-group-row">
                    <div class="form-group" style="margin-bottom:0">
                        <label>Product Type <span class="req">*</span></label>
                        <asp:DropDownList ID="ddlProductType" runat="server" onchange="onProductTypeChange(this.value);">
                            <asp:ListItem Value="">-- Select --</asp:ListItem>
                            <asp:ListItem Value="Core">Core</asp:ListItem>
                            <asp:ListItem Value="Assembled">Assembled</asp:ListItem>
                            <asp:ListItem Value="Conversion">Conversion</asp:ListItem>
                            <asp:ListItem Value="Prefilled Conversion">Prefilled Conversion</asp:ListItem>
                            <asp:ListItem Value="Pre processed RM">Pre processed RM</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="form-group" style="margin-bottom:0">
                        <label>Production UOM</label>
                        <asp:DropDownList ID="ddlProdUOM" runat="server" EnableViewState="false" style="display:none;" />
                        <div style="display:flex;align-items:center;height:38px;padding:0 12px;background:#f0faf5;border:1px solid #c3ece0;border-radius:8px;font-weight:700;color:var(--green);font-size:13px;letter-spacing:.04em;">
                            Batches
                        </div>
                    </div>
                </div>

                <div class="form-group-row" style="margin-top:13px;">
                    <div class="form-group" style="margin-bottom:0">
                        <label>Production Line</label>
                        <asp:DropDownList ID="ddlProductionLine" runat="server">
                            <asp:ListItem Value="0">-- Select Line --</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="form-group" style="margin-bottom:0">
                        <label>Unit Weight (grams)</label>
                        <asp:TextBox ID="txtUnitWeightGrams" runat="server" placeholder="e.g. 30" type="number" step="0.01" min="0"/>
                        <div class="field-hint">Weight of one finished unit in grams (for dough-based calculation)</div>
                    </div>
                </div>

                <!-- PRE PROCESSED RM FIELDS — shown only for Pre processed RM type -->
                <div id="divPreprocessFields" style="display:none;margin-top:13px;background:#e3f2fd;border:1px solid #90caf9;border-radius:10px;padding:14px 16px;">
                    <div style="font-size:11px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;color:#1565c0;margin-bottom:12px;">&#9881; Pre processing Stage Labels</div>
                    <div class="form-group-row">
                        <div class="form-group" style="margin-bottom:0">
                            <label>Input Raw Material Name <span class="req">*</span></label>
                            <asp:TextBox ID="txtInputRMName" runat="server" MaxLength="150" placeholder="e.g. Raw Peanut"/>
                        </div>
                        <div class="form-group" style="margin-bottom:0">
                            <label>Stage 1 Label <span class="req">*</span></label>
                            <asp:TextBox ID="txtStage1Label" runat="server" MaxLength="100" placeholder="e.g. Dispensed for Roasting"/>
                        </div>
                    </div>
                    <div class="form-group-row" style="margin-top:10px;">
                        <div class="form-group" style="margin-bottom:0">
                            <label>Stage 2 Label <span class="req">*</span></label>
                            <asp:TextBox ID="txtStage2Label" runat="server" MaxLength="100" placeholder="e.g. Roasted Peanuts"/>
                        </div>
                        <div class="form-group" style="margin-bottom:0">
                            <label>Stage 3 Label <span class="req">*</span></label>
                            <asp:TextBox ID="txtStage3Label" runat="server" MaxLength="100" placeholder="e.g. Sorted Roasted Peanuts"/>
                        </div>
                    </div>
                </div>

                <div class="form-group-row" style="margin-top:13px;">
                    <div class="form-group" style="margin-bottom:0">
                        <label>HSN Code</label>
                        <asp:TextBox ID="txtHSN" runat="server" MaxLength="20" placeholder="e.g. 2106" />
                    </div>
                    <div class="form-group" style="margin-bottom:0">
                        <label>GST Rate (%)</label>
                        <asp:TextBox ID="txtGSTRate" runat="server" MaxLength="6" placeholder="e.g. 12" />
                    </div>
                </div>

                <!-- PACKING SPECIFICATION -->
                <div class="packing-spec-panel" style="margin-top:13px;">
                    <div class="packing-spec-title">&#x1F4E6; Packing Specification</div>
                    <div class="form-group-row">
                        <div class="form-group" style="margin-bottom:0">
                            <label>Container Type</label>
                            <asp:DropDownList ID="ddlContainerType" runat="server"
                                onchange="onContainerTypeChange(this.value);">
                                <asp:ListItem Value="">-- Not Set --</asp:ListItem>
                                <asp:ListItem Value="JAR">JAR</asp:ListItem>
                                <asp:ListItem Value="BOX">BOX</asp:ListItem>
                                <asp:ListItem Value="DIRECT">DIRECT (Case only)</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="form-group" style="margin-bottom:0">
                            <label>Unit Sizes (comma-separated)</label>
                            <asp:TextBox ID="txtUnitSizes" runat="server" MaxLength="200"
                                placeholder="e.g. 100,200,500 (units per JAR/BOX/Case)"/>
                        </div>
                    </div>
                    <div class="form-group-row" style="margin-top:10px;" id="divContainerRows">
                        <div class="form-group" style="margin-bottom:0">
                            <label>Containers per Case (JARs/BOXes per Case)</label>
                            <asp:TextBox ID="txtContainersPerCase" runat="server" MaxLength="10"
                                placeholder="e.g. 12"/>
                        </div>
                    </div>
                    <div style="margin-top:12px;display:flex;align-items:center;gap:8px;">
                        <asp:CheckBox ID="chkLanguageLabels" runat="server"/>
                        <label style="margin:0;cursor:pointer;font-size:12px;font-weight:600;letter-spacing:.03em;color:#333;text-transform:none;"
                            onclick="document.getElementById('<%= chkLanguageLabels.ClientID %>').click();">
                            This product has language-specific labels (Tamil / Kannada / Telugu)
                        </label>
                    </div>
                </div>

                <div class="form-group" style="margin-top:13px;">
                    <label>Expected Qty Output (per batch) <span class="req">*</span></label>
                    <div style="display:flex;align-items:center;gap:10px;">
                        <asp:TextBox ID="txtBatchSize" runat="server" MaxLength="12" placeholder="e.g. 100" style="flex:1;" />
                        <asp:DropDownList ID="ddlOutputUOM" runat="server" style="width:130px;" />
                    </div>
                    <span class="field-hint" id="batchHint">Quantity and UOM of finished product per one batch</span>
                </div>

                <!-- BATCH OUTPUT PARAMETERS -->
                <div class="params-panel" style="margin-top:13px;">
                    <div class="params-title">&#x1F4CF; Batch Output Parameters</div>
                    <div style="font-size:11px;color:#555;margin-bottom:10px;">Configure what data to capture after each batch (up to 4 fields)</div>
                    <div id="paramsContainer">
                        <!-- rows injected by JS / server -->
                    </div>
                    <button type="button" class="btn-add-param" onclick="addParamRow()">+ Add Parameter</button>
                    <asp:HiddenField ID="hfParamsJson" runat="server" Value="[]"/>
                </div>

                <div class="btn-row">
                    <asp:Button ID="btnSave" runat="server" Text="Save Product" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="btnClear_Click" CausesValidation="false" />
                    <asp:Button ID="btnToggleActive" runat="server" Text="Deactivate" CssClass="btn btn-danger" OnClick="btnToggleActive_Click" CausesValidation="false" Visible="false" />
                </div>
            </div>

            <!-- Product List -->
            <div class="prod-list-header" style="flex-shrink:0;">
                <span class="prod-list-title">Products</span>
                <asp:Label ID="lblCount" runat="server" CssClass="prod-count" Text="0" />
            </div>
            <div class="search-box" style="flex-shrink:0;">
                <input type="text" id="prodSearch" placeholder="&#x1F50D; Search products..." onkeyup="filterProdList(this.value)" />
            </div>
            <div class="prod-list-scroll" style="flex:1; overflow-y:auto;">
                <asp:Repeater ID="rptProducts" runat="server" OnItemCommand="rptProducts_ItemCommand">
                    <ItemTemplate>
                        <asp:LinkButton runat="server" CommandName="Select" CommandArgument='<%# Eval("ProductID") %>'
                            CausesValidation="false"
                            CssClass='prod-row <%# GetSelectedClass(Eval("ProductID")) %>'
                            style="display:flex; text-decoration:none; color:inherit; width:100%;">
                            <div class="prod-row-img">
                                <%# string.IsNullOrEmpty(Eval("ImagePath") as string) ? "&#x1F9EA;" : "<img src='" + Eval("ImagePath") + "' />" %>
                            </div>
                            <div style="flex:1; min-width:0;">
                                <div class="prod-code"><%# Eval("ProductCode") %></div>
                                <div class="prod-name" style="white-space:nowrap;overflow:hidden;text-overflow:ellipsis;"><%# Eval("ProductName") %></div>
                                <span class='prod-type-badge <%# Eval("ProductType").ToString() == "Core" ? "type-core" : Eval("ProductType").ToString() == "Conversion" ? "type-conversion" : Eval("ProductType").ToString() == "Prefilled Conversion" ? "type-prefilled" : Eval("ProductType").ToString() == "Pre processed RM" ? "type-preproc" : "type-assembled" %>'><%# Eval("ProductType") %></span>
                            </div>
                        </asp:LinkButton>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
                    <div class="bom-empty">No products yet. Add your first product.</div>
                </asp:Panel>
            </div>
        </div>

        <!-- ═══════════════════════════════════════════════
             PANEL 2 — BOM / INGREDIENTS
        ════════════════════════════════════════════════ -->
        <div class="panel">
            <div class="panel-head">
                <div class="panel-label">Section 2 &mdash; Bill of Materials</div>
                <div class="panel-title">Ingredients &amp; <span>Components</span></div>
                <div style="font-size:11px; color:var(--text-muted); margin-top:3px;">BOM quantities are always per one Production UOM</div>
            </div>

            <div class="panel-body" style="flex:1; overflow-y:auto;">
                <asp:Panel ID="pnlNoBOM" runat="server" Visible="true">
                    <div class="no-product-state">
                        <div class="big-icon">&#x1F4CB;</div>
                        <p>Select a product from the left panel<br/>to view or edit its Bill of Materials.</p>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlBOM" runat="server" Visible="false">

                    <!-- Product banner -->
                    <div class="bom-product-banner">
                        <div class="bom-product-banner-icon">&#x1F9EA;</div>
                        <div>
                            <div class="bom-product-name"><asp:Label ID="lblBOMProductName" runat="server" /></div>
                            <div class="bom-product-meta">
                                <asp:Label ID="lblBOMProductCode" runat="server" />
                                &nbsp;&bull;&nbsp;
                                <asp:Label ID="lblBOMProductType" runat="server" />
                            </div>
                            <div class="bom-uom-note">BOM per 1 Batch</div>
                        </div>
                    </div>

                    <!-- Add ingredient row -->
                    <div class="section-div">Add Ingredient</div>
                    <div class="add-ing-grid">
                        <div class="form-group" style="margin-bottom:0;">
                            <label>Material Type</label>
                            <asp:DropDownList ID="ddlMatType" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlMatType_Changed">
                                <asp:ListItem Value="">-- Type --</asp:ListItem>
                                <asp:ListItem Value="RM">Raw Material</asp:ListItem>
                                <asp:ListItem Value="PM">Packing Material</asp:ListItem>
                                <asp:ListItem Value="CN">Consumable</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="form-group" style="margin-bottom:0;">
                            <label>Qty</label>
                            <asp:TextBox ID="txtIngQty" runat="server" MaxLength="10" placeholder="0.000" />
                        </div>
                        <div class="form-group" style="margin-bottom:0;">
                            <label>UOM</label>
                            <asp:DropDownList ID="ddlIngUOM" runat="server" EnableViewState="false" onchange="syncConversionUOM();" />
                        </div>
                        <asp:Button ID="btnAddIng" runat="server" Text="+ Add" CssClass="btn btn-blue btn-sm" OnClick="btnAddIng_Click" CausesValidation="false" />
                    </div>
                    <div class="form-group">
                        <label>Material <span class="req">*</span></label>
                        <asp:DropDownList ID="ddlMaterial" runat="server" />
                    </div>

                    <!-- BOM table -->
                    <div class="section-div">Current BOM</div>
                    <asp:Panel ID="pnlBOMEmpty" runat="server" Visible="true">
                        <div class="bom-empty">&#x1F4CB; No ingredients added yet.</div>
                    </asp:Panel>
                    <asp:Panel ID="pnlBOMTable" runat="server" Visible="false">
                        <table class="bom-table" id="bomTable">
                            <thead>
                                <tr>
                                    <th>Type</th>
                                    <th>Material</th>
                                    <th>Qty</th>
                                    <th>UOM</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptBOM" runat="server" OnItemCommand="rptBOM_ItemCommand">
                                    <ItemTemplate>
                                        <tr>
                                            <td><span class='mat-type-pill pill-<%# Eval("MaterialType").ToString().ToLower() %>'><%# Eval("MaterialType") %></span></td>
                                            <td>
                                                <div style="font-weight:500;font-size:12px;"><%# Eval("MaterialName") %></div>
                                                <div style="font-size:10px;color:var(--text-dim);"><%# Eval("MaterialCode") %></div>
                                            </td>
                                            <td style="font-variant-numeric:tabular-nums;"><%# string.Format("{0:N3}", Eval("Quantity")) %></td>
                                            <td style="font-size:11px;color:var(--text-muted);"><%# Eval("Abbreviation") %></td>
                                            <td>
                                                <asp:LinkButton runat="server" CommandName="DeleteBOM" CommandArgument='<%# Eval("BOMID") %>'
                                                    CssClass="del-btn" CausesValidation="false"
                                                    OnClientClick="return erpConfirmLink(this,'Remove this ingredient from the BOM?',{title:'Remove Ingredient',okText:'Yes, Remove',btnClass:'danger'})">&#x2715;</asp:LinkButton>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </asp:Panel>
                </asp:Panel>
            </div>

            <!-- BOM save strip -->
            <div class="bom-save-strip">
                <asp:Label ID="lblBOMCount" runat="server" CssClass="bom-count-note" Text="" />
                <asp:Button ID="btnSaveBOM" runat="server" Text="Save BOM" CssClass="btn btn-blue" OnClick="btnSaveBOM_Click" CausesValidation="false" Visible="false" />
            </div>
        </div>

        <!-- ═══════════════════════════════════════════════
             PANEL 3 — COST ELEMENTS
        ════════════════════════════════════════════════ -->
        <div class="panel">
            <div class="panel-head">
                <div class="panel-label">Section 3</div>
                <div class="panel-title">Cost <span>Elements</span></div>
            </div>
            <div class="panel-body">

                <!-- Product Image -->
                <div style="margin-bottom:14px;">
                    <div style="font-size:10px; font-weight:700; letter-spacing:.08em; text-transform:uppercase; color:var(--text-dim); margin-bottom:8px;">Product Image</div>
                    <div class="img-upload-box" onclick="document.getElementById('fileImage').click()" style="margin-bottom:0;">
                        <input type="file" id="fileImage" name="fileImage" accept="image/*" onchange="previewImage(this)" class="file-input-hidden" />
                        <img id="imgPreview" class="img-preview" src="#" alt="Preview" />
                        <asp:Image ID="imgSaved" runat="server" CssClass="img-preview"
                            Visible="false" AlternateText="Product Image"
                            style="width:100%;max-height:140px;object-fit:contain;border-radius:6px;"/>
                        <div id="imgPlaceholder">
                            <div class="img-placeholder">&#x1F5BC;&#xFE0F;</div>
                            <div class="img-placeholder-text">Click to upload product image<br/><span style="font-size:9px;">PNG, JPG up to 2MB</span></div>
                        </div>
                    </div>
                </div>

                <asp:Panel ID="pnlNoCost" runat="server" Visible="true">
                    <div class="no-product-state">
                        <div class="big-icon">&#x1F4B0;</div>
                        <p>Select a product and complete its BOM<br/>to see cost calculations.</p>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlCost" runat="server" Visible="false">

                    <!-- Batch summary -->
                    <div class="cost-card">
                        <div class="cost-card-title">Batch Summary</div>
                        <div class="cost-row">
                            <span class="cost-label" style="font-weight:600;">Input Batch Size</span>
                            <span class="cost-value"><asp:Label ID="lblCostBatchSize" runat="server" Text="—" /></span>
                        </div>
                        <div style="font-size:10px; color:var(--text-dim); margin:-6px 0 10px 0;">
                            Total quantity of all ingredients in one batch
                        </div>
                        <div class="cost-divider"></div>
                        <div class="cost-row" style="margin-top:10px;">
                            <span class="cost-label" style="font-weight:600;">Expected Output per Batch</span>
                            <span class="cost-value highlight"><asp:Label ID="lblCostExpectedOutput" runat="server" Text="—" /></span>
                        </div>
                        <div style="font-size:10px; color:var(--text-dim); margin:-6px 0 10px 0;">
                            Finished goods quantity produced per batch
                        </div>
                        <div class="cost-divider"></div>
                        <div class="cost-row" style="margin-top:10px;">
                            <span class="cost-label">Ingredients</span>
                            <span class="cost-value"><asp:Label ID="lblCostBOMLines" runat="server" Text="0" /></span>
                        </div>
                    </div>

                    <!-- Material Rates from Opening Stock -->
                    <div class="cost-card">
                        <div class="cost-card-title">Material Rates</div>
                        <div style="font-size:10px; color:var(--text-dim); margin-bottom:10px;">
                            Rates auto-calculated from last 3 GRNs (weighted avg). Falls back to Opening Stock if no GRNs exist.
                            <a href="../MM/MMRawMaterial.aspx" target="_blank" style="color:var(--blue);">Update in MM &#8599;</a>
                        </div>

                        <asp:Repeater ID="rptCostRates" runat="server">
                            <ItemTemplate>
                                <div class="cost-input-group">
                                    <div style="display:flex; justify-content:space-between; align-items:center; gap:8px;">
                                        <label style="margin-bottom:0; flex:1;">
                                            <span class='mat-type-pill pill-<%# Eval("MaterialType").ToString().ToLower() %>' style="margin-right:4px;"><%# Eval("MaterialType") %></span>
                                            <%# Eval("MaterialName") %>
                                            <span style="color:var(--text-dim); font-weight:400;">(<%# Eval("Abbreviation") %>)</span>
                                        </label>
                                        <span style="font-size:12px; font-weight:600; color:<%# Convert.ToInt32(Eval("HasRate")) >= 1 ? "var(--blue)" : "#e74c3c" %>; white-space:nowrap;">
                                            <%# Convert.ToInt32(Eval("HasRate")) >= 1
                                                ? "&#x20B9; " + string.Format("{0:N2}", Eval("UnitRate")) + " / " + Eval("Abbreviation")
                                                : "&#9888; No rate" %>
                                        </span>
                                    </div>
                                    <div style="font-size:10px; margin-top:3px; color:var(--text-dim);">
                                        <%# Convert.ToInt32(Eval("HasRate")) == 1
                                            ? "Weighted avg of last " + Eval("GRNCount") + " GRN" + (Convert.ToInt32(Eval("GRNCount")) == 1 ? "" : "s")
                                            : Convert.ToInt32(Eval("HasRate")) == 2
                                                ? "From Opening Stock (no GRNs yet)"
                                                : "<span style=\"color:#e74c3c;\">No GRNs or Opening Stock found — go to MM to add</span>" %>
                                    </div>
                                    <!-- Hidden inputs carry rate+qty for JS calculation -->
                                    <input type="hidden"
                                        data-qty='<%# Eval("Quantity") %>'
                                        data-rate='<%# Eval("UnitRate") %>'
                                        data-type='<%# Eval("MaterialType") %>'
                                        data-bomid='<%# Eval("BOMID") %>' />
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <!-- Missing rates warning -->
                    <div id="costMissingWarn" style="display:none; background:#fff5f5; border:1px solid #ffcccc; border-radius:8px; padding:10px 14px; margin-bottom:10px; font-size:12px; color:#c0392b;">
                        &#9888; Some materials are missing opening stock rates. Costs shown may be incomplete.
                        Go to <a href="../MM/MMRawMaterial.aspx" target="_blank" style="color:#c0392b; font-weight:600;">MM &rarr; Raw Material</a> and record opening stock with rates.
                    </div>

                    <!-- Computed costs -->
                    <div class="cost-card">
                        <div class="cost-card-title">Computed Costs</div>
                        <div class="cost-row">
                            <span class="cost-label">RM Cost / Batch</span>
                            <span class="cost-value highlight" id="lblRMCostBatch">&#x20B9; 0.00</span>
                        </div>
                        <div class="cost-row">
                            <span class="cost-label">PM Cost / Batch</span>
                            <span class="cost-value highlight" id="lblPMCostBatch">&#x20B9; 0.00</span>
                        </div>
                        <div class="cost-row">
                            <span class="cost-label">Consumable Cost / Batch</span>
                            <span class="cost-value highlight" id="lblCNCostBatch">&#x20B9; 0.00</span>
                        </div>
                        <div class="cost-divider"></div>
                        <div class="cost-row">
                            <span class="cost-label" style="font-weight:600;">Total Material Cost / Batch</span>
                            <span class="cost-value big" id="lblTotalBatch">&#x20B9; 0.00</span>
                        </div>
                        <div class="cost-divider"></div>
                        <div class="cost-row">
                            <span class="cost-label">RM Cost / kg</span>
                            <span class="cost-value" id="lblRMPerKg">&#x20B9; 0.00</span>
                        </div>
                        <div class="cost-row">
                            <span class="cost-label">Expected Unit Cost</span>
                            <span class="cost-value" id="lblUnitCost">&#x20B9; 0.00</span>
                        </div>
                    </div>

                    <div style="font-size:10px; color:var(--text-dim); line-height:1.6; padding:0 2px;">
                        * Rates = weighted average of last 3 GRNs (Qty &times; Rate). Falls back to Opening Stock if no GRNs.<br/>
                        * RM Cost/UOM = Total RM cost &divide; total RM quantity in BOM.<br/>
                        * Unit Cost = Total Batch Cost &divide; Batch Size.
                    </div>

                </asp:Panel>
            </div>
        </div>

    </div><!-- /workspace -->


<script>
// ── PRODUCT TYPE CHANGE ────────────────────────────────────
// ── GLOBAL SETTINGS ────────────────────────────────────────────────────
function toggleGlobalSettings() {
    var panel = document.getElementById("globalSettingsPanel");
    var btn   = document.getElementById("btnGlobalSettings");
    if (!panel) return;
    var isOpen = panel.classList.contains("open");
    panel.classList.toggle("open", !isOpen);
    if (btn) btn.classList.toggle("active", !isOpen);
}
function addRemarkRow(text) {
    var list = document.getElementById("remarkList");
    if (!list) return;
    var row = document.createElement("div");
    row.className = "remark-item";
    row.innerHTML = '<input type="text" placeholder="e.g. No Issues" value="' + (text||"") + '"/>'
        + '<button type="button" class="btn-remove-remark" onclick="this.parentElement.remove()">&#x2715;</button>';
    list.appendChild(row);
}
function collectRemarks() {
    var list = document.getElementById("remarkList");
    var hf   = document.getElementById("<%= hfRemarksJson.ClientID %>");
    if (!list || !hf) return;
    var items = [];
    list.querySelectorAll("input").forEach(function(inp) {
        var v = inp.value.trim(); if (v) items.push(v);
    });
    hf.value = JSON.stringify(items);
}
function loadRemarks(json) {
    var list = document.getElementById("remarkList");
    if (!list) return;
    list.innerHTML = "";
    try { JSON.parse(json||"[]").forEach(function(t){ addRemarkRow(t); }); } catch(e) {}
}
// ── BATCH PARAMS ────────────────────────────────────────────────────────
var PARAM_TYPES = [
    {v:"",       l:"-- Select Type --"},
    {v:"TRAYS",  l:"Number of Trays"},
    {v:"MOLDS",  l:"Number of Molds"},
    {v:"WEIGHT", l:"Weight (kg)"},
    {v:"UNITS",  l:"Total Units Produced"},
    {v:"COUNT",  l:"Count"},
    {v:"CUSTOM", l:"Custom..."}
];

function addParamRow(type, label, options) {
    var container = document.getElementById("paramsContainer");
    if (!container) return;
    var rows = container.querySelectorAll(".param-row");
    if (rows.length >= 4) { erpAlert('Maximum 4 parameters allowed.', {title:'Limit Reached', type:'warn'}); return; }
    var sel = PARAM_TYPES.map(function(p){
        return '<option value="'+p.v+'"'+((p.v===type)?' selected':'')+'>'+p.l+'</option>';
    }).join("");
    var lbl = label || "";
    var row = document.createElement("div");
    row.className = "param-row";
    var opts = options || "";
    row.innerHTML = '<select onchange="onParamTypeChange(this)">'+sel+'</select>'
        + '<input type="text" placeholder="Label" value="'+lbl+'"/>'
        + '<input type="text" placeholder="Options (comma-sep, e.g. OK,Not OK)" value="'+opts+'"/>'
        + '<button type="button" class="btn-remove-param" onclick="removeParamRow(this)">&#x2715;</button>';
    container.appendChild(row);
    // Set default label from type if none provided
    if (!lbl && type) setDefaultLabel(row.querySelector("select"));
    saveParamsToHidden();
}

function onParamTypeChange(sel) {
    setDefaultLabel(sel);
    saveParamsToHidden();
}

function setDefaultLabel(sel) {
    var defaults = {TRAYS:"Number of Trays",MOLDS:"Number of Molds",WEIGHT:"Weight of Dough (kg)",UNITS:"Total Units Produced",COUNT:"Count"};
    var inp = sel.parentElement.querySelector("input");
    if (inp && (inp.value === "" || Object.values(defaults).indexOf(inp.value) >= 0))
        inp.value = defaults[sel.value] || "";
    inp.addEventListener("input", saveParamsToHidden);
}

function removeParamRow(btn) {
    btn.parentElement.remove();
    saveParamsToHidden();
}

function saveParamsToHidden() {
    var container = document.getElementById("paramsContainer");
    if (!container) return;
    var params = [];
    container.querySelectorAll(".param-row").forEach(function(row) {
        var inputs = row.querySelectorAll("input");
        var type  = row.querySelector("select").value;
        var label = inputs[0] ? inputs[0].value.trim() : "";
        var opts  = inputs[1] ? inputs[1].value.trim() : "";
        if (type) params.push({type:type, label:label||type, options:opts});
    });
    var hf = document.getElementById("<%= hfParamsJson.ClientID %>");
    if (hf) hf.value = JSON.stringify(params);
}

function loadParamsFromJson(json) {
    var container = document.getElementById("paramsContainer");
    if (!container) return;
    container.innerHTML = "";
    try {
        var params = JSON.parse(json || "[]");
        params.forEach(function(p){ addParamRow(p.type, p.label, p.options||""); });
    } catch(e) {}
}

function onContainerTypeChange(type) {
    var rows = document.getElementById("divContainerRows");
    if (rows) rows.style.display = type ? "grid" : "none";
}
function onProductTypeChange(type) {
    var hint = document.getElementById('batchHint');
    if (hint) hint.innerText = 'Quantity and UOM of finished product per one batch';
    var prep = document.getElementById('divPreprocessFields');
    if (prep) prep.style.display = (type === 'Pre processed RM') ? 'block' : 'none';
}

// ── CONVERSION UOM SYNC ────────────────────────────────────
function syncConversionUOM() {
    var type = document.getElementById('<%= ddlProductType.ClientID %>').value;
    if (type !== 'Conversion') return;
    var ingUOM = document.getElementById('<%= ddlIngUOM.ClientID %>');
    var outUOM = document.getElementById('<%= ddlOutputUOM.ClientID %>');
    if (ingUOM && outUOM && ingUOM.value !== '0') {
        outUOM.value = ingUOM.value;
    }
}

// ── IMAGE PREVIEW ──────────────────────────────────────────
function previewImage(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function(e) {
            var img = document.getElementById('imgPreview');
            var ph  = document.getElementById('imgPlaceholder');
            img.src = e.target.result;
            img.style.display = 'block';
            ph.style.display  = 'none';
        };
        reader.readAsDataURL(input.files[0]);
    }
}

// ── PRODUCT LIST FILTER ────────────────────────────────────
// ── INIT ON LOAD ──────────────────────────────────────────
window.addEventListener('load', function() {
    var ddlType = document.getElementById('<%= ddlProductType.ClientID %>');
    if (ddlType) onProductTypeChange(ddlType.value);
});

function filterProdList(val) {
    val = val.toLowerCase();
    document.querySelectorAll('.prod-row').forEach(function(r) {
        r.style.display = r.innerText.toLowerCase().includes(val) ? 'flex' : 'none';
    });
}

// ── COST RECALCULATION ─────────────────────────────────────
var batchSize = parseFloat('<%# BatchSizeForCost %>') || 1;

function recalcCosts() {
    var rmCost = 0, pmCost = 0, cnCost = 0, rmTotalQty = 0;
    var missingRates = [];

    document.querySelectorAll('input[data-bomid]').forEach(function(inp) {
        var rate = parseFloat(inp.getAttribute('data-rate')) || 0;
        var qty  = parseFloat(inp.getAttribute('data-qty'))  || 0;
        var type = (inp.getAttribute('data-type') || '').toUpperCase();
        var lineCost = rate * qty;
        if      (type === 'RM') { rmCost += lineCost; rmTotalQty += qty; }
        else if (type === 'PM') { pmCost += lineCost; }
        else if (type === 'CN') { cnCost += lineCost; }
        if (rate === 0) missingRates.push(type);
    });

    var total    = rmCost + pmCost + cnCost;
    var rmPerKg  = rmTotalQty > 0 ? rmCost / rmTotalQty : 0;
    var unitCost = batchSize  > 0 ? total  / batchSize  : 0;

    setText('lblRMCostBatch', '\u20B9 ' + rmCost.toFixed(2));
    setText('lblPMCostBatch', '\u20B9 ' + pmCost.toFixed(2));
    setText('lblCNCostBatch', '\u20B9 ' + cnCost.toFixed(2));
    setText('lblTotalBatch',  '\u20B9 ' + total.toFixed(2));
    setText('lblRMPerKg',     '\u20B9 ' + rmPerKg.toFixed(2));
    setText('lblUnitCost',    '\u20B9 ' + unitCost.toFixed(2));

    // Show warning if any rates are missing
    var warn = document.getElementById('costMissingWarn');
    if (warn) {
        warn.style.display = missingRates.length > 0 ? 'block' : 'none';
    }
}

function setText(id, val) {
    var el = document.getElementById(id);
    if (el) el.innerText = val;
}

// Auto-calculate on page load
window.addEventListener('load', function() { recalcCosts(); });
</script>
</form>


<script src="/StockApp/erp-modal.js"></script>
<script src="/StockApp/erp-keepalive.js"></script>
</body>
</html>
