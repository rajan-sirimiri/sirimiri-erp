<%@ Page Language="C#" AutoEventWireup="true" Inherits="FINApp.FINPurchaseMapping" EnableEventValidation="false" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri FIN — Purchase Mapping</title>
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
.container{max-width:1200px;margin:0 auto;padding:24px 20px 60px;}
.card{background:var(--surface);border-radius:var(--radius);box-shadow:0 2px 16px rgba(0,0,0,.08);padding:24px;margin-bottom:20px;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:20px;letter-spacing:.06em;margin-bottom:6px;}
.card-sub{font-size:12px;color:var(--text-muted);margin-bottom:18px;}
.alert{padding:12px 18px;border-radius:10px;font-size:13px;font-weight:600;margin-bottom:16px;}
.alert-success{background:#eafaf1;color:var(--teal);border:1px solid #a9dfbf;}
.alert-danger{background:#fdf3f2;color:var(--red);border:1px solid #f5c6cb;}
.upload-row{display:flex;gap:12px;align-items:center;flex-wrap:wrap;}
.btn{border:none;border-radius:9px;padding:10px 24px;font-size:13px;font-weight:700;cursor:pointer;font-family:inherit;letter-spacing:.04em;}
.btn-primary{background:var(--accent);color:#fff;} .btn-primary:hover{background:var(--accent-dark);}
.tab-bar{display:flex;gap:0;border-bottom:2px solid var(--border);margin-bottom:20px;}
.tab-btn{padding:12px 24px;font-size:12px;font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:var(--text-muted);cursor:pointer;border:none;background:none;border-bottom:3px solid transparent;margin-bottom:-2px;font-family:inherit;}
.tab-btn:hover{color:var(--text);} .tab-btn.active{color:var(--accent);border-bottom-color:var(--accent);}
.summary-bar{display:flex;gap:16px;margin-bottom:16px;flex-wrap:wrap;}
.summary-stat{background:#fafafa;border:1px solid var(--border);border-radius:8px;padding:8px 16px;text-align:center;}
.summary-stat .val{font-family:'Bebas Neue',sans-serif;font-size:22px;}
.summary-stat .lbl{font-size:9px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);}
.map-table{width:100%;border-collapse:collapse;font-size:12px;}
.map-table th{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);padding:10px 8px;border-bottom:2px solid var(--border);text-align:left;position:sticky;top:0;background:var(--surface);}
.map-table td{padding:8px;border-bottom:1px solid #f0f0f0;vertical-align:middle;}
.tally-name{font-weight:600;font-size:12px;max-width:300px;word-wrap:break-word;}
.map-select{width:100%;padding:7px 10px;border:1.5px solid var(--border);border-radius:6px;font-size:12px;font-family:inherit;}
.btn-save-row{background:var(--teal);color:#fff;border:none;border-radius:6px;padding:6px 12px;font-size:11px;font-weight:700;cursor:pointer;font-family:inherit;text-decoration:none;display:inline-block;}
.btn-save-row:hover{background:#148a5b;}
</style>
</head>
<body>
<form id="form1" runat="server">

<asp:HiddenField ID="hfTab" runat="server" Value="ITEMS"/>

<nav>
    <a class="nav-logo" href="FINHome.aspx">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    </a>
    <span class="nav-title">FIN — Purchase Data Mapping</span>
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

<!-- UPLOAD -->
<div class="card">
    <div class="card-title">&#x1F4C2; Purchase Report Template</div>
    <div class="card-sub">Upload the Tally Purchase_Report.xlsx to scan items and suppliers for mapping.</div>
    <div class="upload-row">
        <asp:FileUpload ID="fileUpload" runat="server"/>
        <asp:Button ID="btnUpload" runat="server" Text="&#x1F4E4; Upload &amp; Scan" CssClass="btn btn-primary" OnClick="btnUpload_Click"/>
    </div>
    <div style="margin-top:18px;border-top:1px solid var(--border);padding-top:14px;">
        <div style="font-size:11px;font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:var(--text-dim);margin-bottom:8px;">Previously Uploaded Files</div>
        <asp:Panel ID="pnlNoSavedFiles" runat="server"><div style="font-size:12px;color:#999;">No files uploaded yet.</div></asp:Panel>
        <asp:Repeater ID="rptSavedFiles" runat="server" OnItemCommand="rptSavedFiles_ItemCommand">
            <ItemTemplate>
                <div style="display:flex;align-items:center;gap:10px;padding:6px 0;border-bottom:1px solid #f0f0f0;">
                    <span style="font-size:12px;color:var(--text);flex:1;">&#x1F4C4; <%# Eval("Name") %></span>
                    <span style="font-size:10px;color:var(--text-dim);"><%# ((DateTime)Eval("CreationTime")).ToString("dd-MMM-yyyy HH:mm") %></span>
                    <asp:LinkButton ID="btnLoad" runat="server" CommandName="LoadFile" CommandArgument='<%# Eval("Name") %>' CssClass="btn-save-row" CausesValidation="false">Load</asp:LinkButton>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>
</div>

<!-- RESULTS -->
<asp:Panel ID="pnlResults" runat="server" Visible="false">

<div class="tab-bar">
    <asp:Button ID="btnTabItems" runat="server" CssClass="tab-btn active" Text="Items" OnClick="btnTab_Click" CommandArgument="ITEMS"/>
    <asp:Button ID="btnTabSuppliers" runat="server" CssClass="tab-btn" Text="Suppliers" OnClick="btnTab_Click" CommandArgument="SUPPLIERS"/>
    <asp:Button ID="btnTabMapped" runat="server" CssClass="tab-btn" Text="&#x2705; Mapped Items" OnClick="btnTab_Click" CommandArgument="MAPPED"/>
</div>

<!-- ITEMS TAB -->
<asp:Panel ID="pnlItems" runat="server">
<div class="card">
    <div class="card-title">&#x1F4E6; Item Mapping</div>
    <div class="card-sub">Map each Tally item to an ERP material type and specific material.</div>
    <div class="summary-bar">
        <div class="summary-stat"><div class="val" style="color:var(--orange);"><asp:Label ID="lblItemCount" runat="server" Text="0"/></div><div class="lbl">Unmapped</div></div>
        <div class="summary-stat"><div class="val" style="color:var(--teal);"><asp:Label ID="lblItemMapped" runat="server" Text="0"/></div><div class="lbl">Mapped</div></div>
    </div>
    <asp:Repeater ID="rptUnmappedItems" runat="server" OnItemCommand="rptUnmappedItems_ItemCommand">
        <HeaderTemplate>
            <div style="max-height:600px;overflow-y:auto;">
            <table class="map-table">
            <thead><tr>
                <th style="width:30px;">#</th>
                <th>Tally Item Name</th>
                <th style="width:120px;">Material Type</th>
                <th>ERP Material</th>
                <th style="width:70px;"></th>
            </tr></thead><tbody>
        </HeaderTemplate>
        <ItemTemplate>
            <tr>
                <td style="color:var(--text-dim);"><%# Container.ItemIndex + 1 %></td>
                <td class="tally-name"><%# Eval("TallyName") %></td>
                <td><asp:DropDownList ID="ddlMatType" runat="server" CssClass="map-select" style="width:110px;" AutoPostBack="false">
                    <asp:ListItem Text="-- Type --" Value=""/>
                    <asp:ListItem Text="RM" Value="RM"/>
                    <asp:ListItem Text="PM" Value="PM"/>
                    <asp:ListItem Text="CN" Value="CN"/>
                    <asp:ListItem Text="ST" Value="ST"/>
                    <asp:ListItem Text="SCRAP" Value="SCRAP"/>
                    <asp:ListItem Text="CAPEX" Value="CAPEX"/>
                    <asp:ListItem Text="OTHER" Value="OTHER"/>
                </asp:DropDownList></td>
                <td><asp:DropDownList ID="ddlMaterial" runat="server" CssClass="map-select"/></td>
                <td><asp:LinkButton ID="btnSaveItem" runat="server" CommandName="SaveItem" CommandArgument='<%# Eval("TallyName") %>' CssClass="btn-save-row" CausesValidation="false">Save</asp:LinkButton></td>
            </tr>
        </ItemTemplate>
        <FooterTemplate></tbody></table></div></FooterTemplate>
    </asp:Repeater>
</div>
</asp:Panel>

<!-- SUPPLIERS TAB -->
<asp:Panel ID="pnlSuppliers" runat="server" Visible="false">
<div class="card">
    <div class="card-title">&#x1F465; Supplier Mapping</div>
    <div class="card-sub">Suppliers auto-matched on upload. Remaining shown below.</div>
    <div class="summary-bar">
        <div class="summary-stat"><div class="val" style="color:var(--orange);"><asp:Label ID="lblSupplierCount" runat="server" Text="0"/></div><div class="lbl">Unmapped</div></div>
        <div class="summary-stat"><div class="val" style="color:var(--teal);"><asp:Label ID="lblSupplierMapped" runat="server" Text="0"/></div><div class="lbl">Mapped</div></div>
    </div>
    <asp:Repeater ID="rptUnmappedSuppliers" runat="server" OnItemCommand="rptUnmappedSuppliers_ItemCommand">
        <HeaderTemplate>
            <div style="max-height:600px;overflow-y:auto;">
            <table class="map-table">
            <thead><tr>
                <th style="width:30px;">#</th>
                <th>Tally Supplier Name</th>
                <th>ERP Supplier</th>
                <th style="width:70px;"></th>
            </tr></thead><tbody>
        </HeaderTemplate>
        <ItemTemplate>
            <tr>
                <td style="color:var(--text-dim);"><%# Container.ItemIndex + 1 %></td>
                <td class="tally-name"><%# Eval("TallyName") %></td>
                <td><asp:DropDownList ID="ddlSupplier" runat="server" CssClass="map-select"/></td>
                <td><asp:LinkButton ID="btnSaveSupplier" runat="server" CommandName="SaveSupplier" CommandArgument='<%# Eval("TallyName") %>' CssClass="btn-save-row" CausesValidation="false">Save</asp:LinkButton></td>
            </tr>
        </ItemTemplate>
        <FooterTemplate></tbody></table></div></FooterTemplate>
    </asp:Repeater>
</div>
</asp:Panel>

<!-- MAPPED TAB -->
<asp:Panel ID="pnlMapped" runat="server" Visible="false">
<div class="card">
    <div class="card-title">&#x2705; Mapped Items</div>
    <asp:Label ID="lblMappedItemCount" runat="server" style="font-size:12px;color:var(--teal);font-weight:700;"/>
    <asp:Repeater ID="rptMappedItems" runat="server">
        <HeaderTemplate>
            <div style="max-height:400px;overflow-y:auto;margin-top:10px;">
            <table class="map-table"><thead><tr><th>#</th><th>Tally Name</th><th>Type</th><th>ERP Material</th></tr></thead><tbody>
        </HeaderTemplate>
        <ItemTemplate>
            <tr>
                <td style="color:var(--text-dim);"><%# Container.ItemIndex + 1 %></td>
                <td class="tally-name"><%# Eval("TallyName") %></td>
                <td style="font-weight:700;"><%# Eval("MaterialType") %></td>
                <td><%# Eval("MaterialLabel") %></td>
            </tr>
        </ItemTemplate>
        <FooterTemplate></tbody></table></div></FooterTemplate>
    </asp:Repeater>
</div>
<div class="card" style="margin-top:16px;">
    <div class="card-title">&#x2705; Mapped Suppliers</div>
    <asp:Label ID="lblMappedSupplierCount" runat="server" style="font-size:12px;color:var(--teal);font-weight:700;"/>
    <asp:Repeater ID="rptMappedSuppliers" runat="server">
        <HeaderTemplate>
            <div style="max-height:400px;overflow-y:auto;margin-top:10px;">
            <table class="map-table"><thead><tr><th>#</th><th>Tally Name</th><th>ERP Supplier</th></tr></thead><tbody>
        </HeaderTemplate>
        <ItemTemplate>
            <tr>
                <td style="color:var(--text-dim);"><%# Container.ItemIndex + 1 %></td>
                <td class="tally-name"><%# Eval("TallyName") %></td>
                <td style="font-weight:600;"><%# Eval("SupplierName") %> <span style="color:var(--text-dim);">(<%# Eval("SupplierCode") %>)</span></td>
            </tr>
        </ItemTemplate>
        <FooterTemplate></tbody></table></div></FooterTemplate>
    </asp:Repeater>
</div>
</asp:Panel>

</asp:Panel>
</div>
<script src="/StockApp/erp-keepalive.js"></script>
</form>
</body>
</html>
