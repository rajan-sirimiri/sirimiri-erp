<%@ Page Language="C#" AutoEventWireup="true" Inherits="FINApp.FINSalesImport" EnableEventValidation="false" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri FIN — Sales Import</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600;700&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
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

.container{max-width:1100px;margin:0 auto;padding:24px 20px 60px;}
.card{background:var(--surface);border-radius:var(--radius);box-shadow:0 2px 16px rgba(0,0,0,.08);padding:24px;margin-bottom:20px;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:20px;letter-spacing:.06em;margin-bottom:6px;}
.card-sub{font-size:12px;color:var(--text-muted);margin-bottom:18px;}

.alert{padding:12px 18px;border-radius:10px;font-size:13px;font-weight:600;margin-bottom:16px;}
.alert-success{background:#eafaf1;color:var(--teal);border:1px solid #a9dfbf;}
.alert-danger{background:#fdf3f2;color:var(--red);border:1px solid #f5c6cb;}

.upload-row{display:flex;gap:12px;align-items:center;flex-wrap:wrap;}
.btn{border:none;border-radius:9px;padding:10px 24px;font-size:13px;font-weight:700;cursor:pointer;font-family:inherit;letter-spacing:.04em;}
.btn-primary{background:var(--accent);color:#fff;} .btn-primary:hover{background:var(--accent-dark);}
.btn-teal{background:var(--teal);color:#fff;} .btn-teal:hover{background:#148a5b;}
.btn-save-row{background:var(--teal);color:#fff;border:none;border-radius:6px;padding:6px 12px;font-size:11px;font-weight:700;cursor:pointer;font-family:inherit;}

.summary-bar{display:flex;gap:16px;margin:16px 0;flex-wrap:wrap;}
.summary-stat{background:#fafafa;border:1px solid var(--border);border-radius:8px;padding:10px 18px;text-align:center;}
.summary-stat .val{font-family:'Bebas Neue',sans-serif;font-size:24px;}
.summary-stat .lbl{font-size:9px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);}

.map-table{width:100%;border-collapse:collapse;font-size:12px;}
.map-table th{font-size:10px;font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:var(--text-dim);padding:8px;border-bottom:2px solid var(--border);text-align:left;}
.map-table td{padding:8px;border-bottom:1px solid #f0f0f0;}

.unmapped-item{padding:6px 10px;font-size:12px;border-bottom:1px solid #f5f5f5;color:var(--red);}
</style>
</head>
<body>
<form id="form1" runat="server">

<asp:HiddenField ID="hfFilePath" runat="server" Value=""/>
<asp:HiddenField ID="hfLoadFileName" runat="server" Value=""/>
<asp:Button ID="btnLoadSaved" runat="server" OnClick="btnLoadSaved_Click" style="display:none;"/>

<nav>
    <a class="nav-logo" href="FINHome.aspx">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    </a>
    <span class="nav-title">FIN — Sales Invoice Import</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblNavUser" runat="server"/></span>
        <a href="FINHome.aspx" class="nav-link">&#8592; Home</a>
        <a href="FINLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>

<div class="container">

<asp:Panel ID="pnlAlert" runat="server" Visible="false" CssClass="alert">
    <asp:Label ID="lblAlert" runat="server"/>
</asp:Panel>

<!-- UPLOAD CARD -->
<div class="card">
    <div class="card-title">&#x1F4C8; Upload Tally Sales Report</div>
    <div class="card-sub">Upload the full Tally Sales_Report.xlsx. The system will preview and validate before importing.</div>
    <div class="upload-row">
        <asp:FileUpload ID="fileUpload" runat="server"/>
        <asp:Button ID="btnUpload" runat="server" Text="&#x1F4E4; Upload &amp; Preview" CssClass="btn btn-primary" OnClick="btnUpload_Click"/>
    </div>

    <div style="margin-top:18px;border-top:1px solid var(--border);padding-top:14px;">
        <div style="font-size:11px;font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:var(--text-dim);margin-bottom:8px;">Previously Uploaded Files</div>
        <asp:Panel ID="pnlNoSavedFiles" runat="server">
            <div style="font-size:12px;color:#999;">No files uploaded yet.</div>
        </asp:Panel>
        <asp:Repeater ID="rptSavedFiles" runat="server">
            <ItemTemplate>
                <div style="display:flex;align-items:center;gap:10px;padding:6px 0;border-bottom:1px solid #f0f0f0;">
                    <span style="font-size:12px;color:var(--text);flex:1;">&#x1F4C4; <%# Eval("Name") %></span>
                    <span style="font-size:10px;color:var(--text-dim);"><%# ((DateTime)Eval("CreationTime")).ToString("dd-MMM-yyyy HH:mm") %></span>
                    <button type="button" class="btn-save-row" onclick="loadSavedFile('<%# Eval("Name") %>');">Load</button>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>
</div>

<!-- PREVIEW RESULTS -->
<asp:Panel ID="pnlResults" runat="server" Visible="false">
<div class="card">
    <div class="card-title">&#x1F50D; Preview</div>
    <div class="summary-bar">
        <div class="summary-stat"><div class="val"><asp:Label ID="lblPreviewTotal" runat="server" Text="0"/></div><div class="lbl">Total Rows</div></div>
        <div class="summary-stat"><div class="val" style="color:var(--teal);"><asp:Label ID="lblPreviewInvoices" runat="server" Text="0"/></div><div class="lbl">New Invoices</div></div>
        <div class="summary-stat"><div class="val" style="color:var(--blue);"><asp:Label ID="lblPreviewMapped" runat="server" Text="0"/></div><div class="lbl">Already Imported</div></div>
        <div class="summary-stat"><div class="val" style="color:var(--red);"><asp:Label ID="lblPreviewUnmapped" runat="server" Text="0"/></div><div class="lbl">Unmapped Items</div></div>
    </div>

    <!-- Unmapped items warning -->
    <asp:Panel ID="pnlUnmapped" runat="server" Visible="false">
        <div style="background:#fdf3f2;border:1px solid #f5c6cb;border-radius:10px;padding:14px;margin-bottom:14px;">
            <div style="font-size:12px;font-weight:700;color:var(--red);margin-bottom:8px;">&#x26A0; Unmapped Items — these will be imported without ERP mapping. Map them in Tally Mapping first for full traceability.</div>
            <div style="max-height:200px;overflow-y:auto;">
                <asp:Repeater ID="rptUnmappedItems" runat="server">
                    <ItemTemplate>
                        <div class="unmapped-item"><%# Eval("Item") %></div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </asp:Panel>

    <div style="display:flex;gap:12px;align-items:center;">
        <asp:Button ID="btnImportHidden" runat="server" OnClick="btnImport_Click" style="display:none;"/>
        <button type="button" class="btn btn-teal" onclick="doImportConfirm();">&#x1F4E5; Import Sales Data</button>
        <span style="font-size:11px;color:var(--text-dim);">Already imported invoices will be skipped automatically.</span>
    </div>
</div>
</asp:Panel>

<!-- IMPORT HISTORY -->
<div class="card">
    <div class="card-title">&#x1F4CB; Import History</div>
    <asp:Repeater ID="rptImportHistory" runat="server">
        <HeaderTemplate>
            <table class="map-table">
            <thead><tr>
                <th>Date</th>
                <th>File</th>
                <th>Inserted</th>
                <th>Skipped</th>
                <th>Errors</th>
                <th>By</th>
            </tr></thead><tbody>
        </HeaderTemplate>
        <ItemTemplate>
            <tr>
                <td><%# ((DateTime)Eval("ImportedAt")).ToString("dd-MMM-yyyy HH:mm") %></td>
                <td style="font-weight:600;"><%# Eval("FileName") %></td>
                <td style="color:var(--teal);font-weight:700;"><%# Eval("RowsInserted") %></td>
                <td style="color:var(--text-dim);"><%# Eval("RowsSkipped") %></td>
                <td style="color:var(--red);font-weight:<%# Convert.ToInt32(Eval("RowsError")) > 0 ? "700" : "400" %>;"><%# Eval("RowsError") %></td>
                <td><%# Eval("ImportedByName") %></td>
            </tr>
        </ItemTemplate>
        <FooterTemplate></tbody></table></FooterTemplate>
    </asp:Repeater>
</div>

</div>

</form>
<script src="/StockApp/erp-modal.js"></script>
<script>
function loadSavedFile(fileName) {
    document.getElementById('<%= hfLoadFileName.ClientID %>').value = fileName;
    document.getElementById('<%= btnLoadSaved.ClientID %>').click();
}
function doImportConfirm() {
    if (typeof erpConfirm === 'function') {
        erpConfirm('Import all new invoices? Already imported vouchers will be skipped.', {
            title: 'Confirm Import',
            type: 'info',
            okText: 'Import',
            onOk: function() { document.getElementById('<%= btnImportHidden.ClientID %>').click(); }
        });
    } else {
        if (confirm('Import all new invoices? Already imported vouchers will be skipped.')) {
            document.getElementById('<%= btnImportHidden.ClientID %>').click();
        }
    }
}
</script>
<script src="/StockApp/erp-keepalive.js"></script>
</body>
</html>
