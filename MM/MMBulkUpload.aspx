<%@ Page Language="C#" AutoEventWireup="true" Inherits="MMApp.MMBulkUpload" EnableEventValidation="false" %>
<!DOCTYPE html><html lang="en"><head runat="server">
<meta charset="utf-8"/><meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Bulk Upload &mdash; MM</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--accent:#2980b9;--accent-dark:#1a6a9e;--teal:#1a9e6a;--red:#e74c3c;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--radius:12px;}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);min-height:100vh;color:var(--text);}
nav{background:#1a1a1a;height:52px;display:flex;align-items:center;padding:0 24px;gap:12px;position:sticky;top:0;z-index:100;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;}.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:17px;letter-spacing:.08em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:12px;}
.nav-user{color:rgba(255,255,255,.8);font-size:12px;}
.nav-link{color:#aaa;text-decoration:none;font-size:11px;font-weight:600;text-transform:uppercase;padding:5px 10px;border-radius:5px;}
.nav-link:hover{color:#fff;background:rgba(255,255,255,.08);}
.page-header{background:var(--surface);border-bottom:1px solid var(--border);padding:18px 32px;}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:26px;letter-spacing:.07em;}.page-title span{color:var(--accent);}
.page-sub{font-size:12px;color:var(--text-muted);margin-top:2px;}
.main{max-width:1100px;margin:24px auto;padding:0 28px;}
.card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:20px 24px;margin-bottom:20px;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:14px;letter-spacing:.08em;color:var(--text-muted);margin-bottom:14px;padding-bottom:10px;border-bottom:1px solid var(--border);}
.btn{border:none;border-radius:8px;padding:10px 22px;font-size:12px;font-weight:700;cursor:pointer;display:inline-block;}
.btn-primary{background:var(--accent);color:#fff;}.btn-primary:hover{background:var(--accent-dark);}
.btn-success{background:var(--teal);color:#fff;}
.btn-secondary{background:#f0f0f0;color:#333;border:1px solid var(--border);}
.btn-row{display:flex;gap:10px;margin-top:16px;flex-wrap:wrap;}
.alert{padding:10px 14px;border-radius:8px;font-size:13px;font-weight:600;margin-bottom:14px;}
.alert-success{background:#eafaf1;color:var(--teal);border:1px solid #a9dfbf;}
.alert-danger{background:#fdf3f2;color:var(--red);border:1px solid #f5c6cb;}
.upload-area{border:2px dashed var(--border);border-radius:12px;padding:30px;text-align:center;margin:16px 0;background:#fafafa;}
.upload-area:hover{border-color:var(--accent);background:#f5faff;}
table{width:100%;border-collapse:collapse;font-size:13px;margin-top:12px;}
th{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);padding:9px 10px;border-bottom:2px solid var(--border);text-align:left;background:#fafafa;}
td{padding:8px 10px;border-bottom:1px solid #f0f0f0;}
th.num,td.num{text-align:right;}
.row-ok{color:var(--teal);}.row-err{color:var(--red);}.row-skip{color:var(--accent);}
.step-num{display:inline-flex;align-items:center;justify-content:center;width:28px;height:28px;border-radius:50%;background:var(--accent);color:#fff;font-size:13px;font-weight:700;margin-right:10px;}
.tab-row{display:flex;gap:0;border-bottom:2px solid var(--border);margin-bottom:16px;}
.tab-btn{padding:10px 20px;font-size:12px;font-weight:700;letter-spacing:.04em;border:none;background:transparent;cursor:pointer;color:var(--text-muted);border-bottom:3px solid transparent;margin-bottom:-2px;}
.tab-btn.active{color:var(--accent);border-bottom-color:var(--accent);}
</style></head><body>
<form id="form1" runat="server">
<nav>
    <a class="nav-logo" href="MMHome.aspx"><img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="" onerror="this.style.display='none'"/></a>
    <span class="nav-title">Materials Management</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblNavUser" runat="server"/></span>
        <a href="/StockApp/ERPHome.aspx" class="nav-link">ERP Home</a>
        <a href="MMHome.aspx" class="nav-link">MM Home</a>
        <a href="MMLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>
<div class="page-header">
    <div class="page-title">Bulk <span>Upload</span></div>
    <div class="page-sub">Import Suppliers, Raw Materials, Packing Materials, Consumables, and Stationaries from Excel</div>
</div>
<div class="main">
    <asp:Panel ID="pnlAlert" runat="server" Visible="false"><div class="alert"><asp:Label ID="lblAlert" runat="server"/></div></asp:Panel>

    <!-- STEP 1: Download Template -->
    <div class="card">
        <div class="card-title"><span class="step-num">1</span> Download Template</div>
        <p style="font-size:13px;color:var(--text-muted);margin-bottom:12px;">
            Download the Excel template with 5 sheets — one per master type. Each sheet is pre-populated with
            column headers and a sample row. Fill your data starting from row 3.
        </p>
        <p style="font-size:12px;color:var(--text-dim);margin-bottom:14px;">
            <strong>Sheets:</strong> Suppliers, Raw Materials, Packing Materials, Consumables, Stationaries.
            Duplicate names are automatically skipped. UOM abbreviations must match the system (see reference column in template).
        </p>
        <asp:Button ID="btnDownloadTemplate" runat="server" Text="&#x1F4E5; Download Template" CssClass="btn btn-secondary" OnClick="btnDownloadTemplate_Click" CausesValidation="false"/>
    </div>

    <!-- STEP 2: Upload -->
    <div class="card">
        <div class="card-title"><span class="step-num">2</span> Upload Filled Excel</div>
        <div class="upload-area">
            <asp:FileUpload ID="fuExcel" runat="server" accept=".xlsx" style="font-size:13px;"/>
            <div style="margin-top:8px;font-size:11px;color:var(--text-dim);">Accepts .xlsx files only</div>
        </div>
        <div class="btn-row">
            <asp:Button ID="btnPreview" runat="server" Text="&#x1F50D; Preview &amp; Validate" CssClass="btn btn-primary" OnClick="btnPreview_Click" CausesValidation="false"/>
        </div>
    </div>

    <!-- STEP 3: Preview & Confirm -->
    <asp:Panel ID="pnlPreview" runat="server" Visible="false">
    <div class="card">
        <div class="card-title"><span class="step-num">3</span> Preview &amp; Confirm</div>

        <div class="tab-row">
            <asp:Button ID="btnTabSup" runat="server" Text="Suppliers" CssClass="tab-btn active" OnClick="btnTab_Click" CommandArgument="SUP" CausesValidation="false"/>
            <asp:Button ID="btnTabRM" runat="server" Text="Raw Materials" CssClass="tab-btn" OnClick="btnTab_Click" CommandArgument="RM" CausesValidation="false"/>
            <asp:Button ID="btnTabPM" runat="server" Text="Packing Materials" CssClass="tab-btn" OnClick="btnTab_Click" CommandArgument="PM" CausesValidation="false"/>
            <asp:Button ID="btnTabCN" runat="server" Text="Consumables" CssClass="tab-btn" OnClick="btnTab_Click" CommandArgument="CN" CausesValidation="false"/>
            <asp:Button ID="btnTabST" runat="server" Text="Stationaries" CssClass="tab-btn" OnClick="btnTab_Click" CommandArgument="ST" CausesValidation="false"/>
        </div>
        <asp:HiddenField ID="hfActiveTab" runat="server" Value="SUP"/>

        <asp:Label ID="lblPreviewSummary" runat="server" style="font-size:13px;font-weight:600;margin-bottom:10px;display:block;"/>

        <asp:Repeater ID="rptPreview" runat="server">
            <HeaderTemplate><table><thead><tr><th>Row</th>
                <asp:Literal ID="litHeaders" runat="server"/>
                <th>Status</th></tr></thead><tbody></HeaderTemplate>
            <ItemTemplate><tr>
                <td><%# Eval("RowNum") %></td>
                <td><%# Eval("Col1") %></td><td><%# Eval("Col2") %></td><td><%# Eval("Col3") %></td>
                <td><%# Eval("Col4") %></td><td><%# Eval("Col5") %></td><td><%# Eval("Col6") %></td>
                <td class='<%# Eval("IsValid").ToString()=="True" ? "row-ok" : (Eval("StatusMsg").ToString().StartsWith("Skip") ? "row-skip" : "row-err") %>' style="font-weight:700;font-size:11px;"><%# Eval("StatusMsg") %></td>
            </tr></ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>

        <div class="btn-row">
            <asp:Button ID="btnConfirmUpload" runat="server" Text="&#x2705; Confirm &amp; Import All" CssClass="btn btn-success" OnClick="btnConfirmUpload_Click" CausesValidation="false"/>
        </div>
    </div>
    </asp:Panel>
</div>
</form>
<script src="/StockApp/erp-modal.js"></script>
<script src="/StockApp/erp-keepalive.js"></script>
</body></html>
