<%@ Page Language="C#" AutoEventWireup="true" Inherits="FINApp.FINTallyMapping" EnableEventValidation="false" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri FIN — Tally Mapping</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600;700&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{
    --accent:#8e44ad; --accent-dark:#6c3483; --accent-light:#f4ecf7;
    --teal:#1a9e6a; --orange:#e67e22; --red:#e74c3c; --blue:#2980b9;
    --text:#1a1a1a; --text-muted:#666; --text-dim:#999;
    --bg:#f0f0f0; --surface:#fff; --border:#e0e0e0;
    --radius:14px; --nav-h:52px;
}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);color:var(--text);min-height:100vh;}
nav{background:#1a1a1a;height:var(--nav-h);display:flex;align-items:center;padding:0 20px;gap:12px;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;}
.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.08em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:16px;}
.nav-user{color:rgba(255,255,255,.8);font-size:12px;}
.nav-link{color:#fff;font-size:12px;font-weight:600;text-decoration:none;opacity:.8;}
.nav-link:hover{opacity:1;}

.container{max-width:1200px;margin:0 auto;padding:24px 20px 60px;}
.card{background:var(--surface);border-radius:var(--radius);box-shadow:0 2px 16px rgba(0,0,0,.08);padding:24px;margin-bottom:20px;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:20px;letter-spacing:.06em;margin-bottom:6px;}
.card-sub{font-size:12px;color:var(--text-muted);margin-bottom:18px;}

.alert{padding:12px 18px;border-radius:10px;font-size:13px;font-weight:600;margin-bottom:16px;}
.alert-success{background:#eafaf1;color:var(--teal);border:1px solid #a9dfbf;}
.alert-danger{background:#fdf3f2;color:var(--red);border:1px solid #f5c6cb;}

.upload-row{display:flex;gap:12px;align-items:center;flex-wrap:wrap;}
.upload-row input[type=file]{font-size:13px;}
.btn{border:none;border-radius:9px;padding:10px 24px;font-size:13px;font-weight:700;cursor:pointer;font-family:inherit;letter-spacing:.04em;}
.btn-primary{background:var(--accent);color:#fff;} .btn-primary:hover{background:var(--accent-dark);}
.btn-teal{background:var(--teal);color:#fff;} .btn-teal:hover{background:#148a5b;}

.tab-bar{display:flex;gap:0;border-bottom:2px solid var(--border);margin-bottom:20px;}
.tab-btn{padding:12px 24px;font-size:12px;font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:var(--text-muted);cursor:pointer;border:none;background:none;border-bottom:3px solid transparent;margin-bottom:-2px;font-family:inherit;}
.tab-btn:hover{color:var(--text);}
.tab-btn.active{color:var(--accent);border-bottom-color:var(--accent);}
.tab-count{background:var(--accent-light);color:var(--accent-dark);font-size:10px;font-weight:700;padding:2px 7px;border-radius:10px;margin-left:6px;}

.summary-bar{display:flex;gap:16px;margin-bottom:16px;flex-wrap:wrap;}
.summary-stat{background:#fafafa;border:1px solid var(--border);border-radius:8px;padding:8px 16px;text-align:center;}
.summary-stat .val{font-family:'Bebas Neue',sans-serif;font-size:22px;}
.summary-stat .lbl{font-size:9px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);}

.map-table{width:100%;border-collapse:collapse;font-size:12px;}
.map-table th{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);padding:10px 8px;border-bottom:2px solid var(--border);text-align:left;position:sticky;top:0;background:var(--surface);}
.map-table td{padding:8px;border-bottom:1px solid #f0f0f0;vertical-align:middle;}
.map-table tr:hover{background:#fafafa;}
.tally-name{font-weight:600;font-size:12px;max-width:300px;word-wrap:break-word;}

.map-select{width:100%;padding:7px 10px;border:1.5px solid var(--border);border-radius:6px;font-size:12px;font-family:inherit;}
.map-select:focus{border-color:var(--accent);outline:none;}
.form-select{width:80px;padding:7px 6px;border:1.5px solid var(--border);border-radius:6px;font-size:11px;font-family:inherit;font-weight:700;}
.ppu-input,.mrp-input{width:70px;padding:7px 8px;border:1.5px solid var(--border);border-radius:6px;font-size:12px;text-align:right;font-family:inherit;}

.empty-note{text-align:center;padding:30px;color:var(--text-dim);font-size:13px;}
.save-bar{margin-top:16px;display:flex;gap:12px;align-items:center;}
.btn-save-row{background:var(--teal);color:#fff;border:none;border-radius:6px;padding:6px 12px;font-size:11px;font-weight:700;cursor:pointer;font-family:inherit;}
.btn-save-row:hover{background:#148a5b;}
</style>
</head>
<body>
<form id="form1" runat="server">

<asp:HiddenField ID="hfTab" runat="server" Value="PRODUCTS"/>

<nav>
    <a class="nav-logo" href="FINHome.aspx">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    </a>
    <span class="nav-title">FIN — Tally Data Mapping</span>
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
    <div class="card-title">&#x1F4C2; Upload Tally Mapping Template</div>
    <div class="card-sub">Upload the mapping template file (3 sheets: Products, Scrap Items, Customers). The system will show unmapped items for you to map to ERP master data.</div>
    <div class="upload-row">
        <asp:FileUpload ID="fileUpload" runat="server"/>
        <asp:Button ID="btnUpload" runat="server" Text="&#x1F50D; Scan File" CssClass="btn btn-primary" OnClick="btnUpload_Click"/>
    </div>
</div>

<!-- RESULTS -->
<asp:Panel ID="pnlResults" runat="server" Visible="false">

<!-- TABS -->
<div class="tab-bar">
    <asp:Button ID="btnTabProducts" runat="server" CssClass="tab-btn active"
        Text="Products" OnClick="btnTab_Click" CommandArgument="PRODUCTS"/>
    <asp:Button ID="btnTabScrap" runat="server" CssClass="tab-btn"
        Text="Scrap / Misc" OnClick="btnTab_Click" CommandArgument="SCRAP"/>
    <asp:Button ID="btnTabCustomers" runat="server" CssClass="tab-btn"
        Text="Customers" OnClick="btnTab_Click" CommandArgument="CUSTOMERS"/>
</div>

<!-- ═══ PRODUCTS TAB ═══ -->
<asp:Panel ID="pnlProducts" runat="server">
<div class="card">
    <div class="card-title">&#x1F4E6; Product Mapping</div>
    <div class="card-sub">Map each Tally product name to an ERP product + packing option. Click Save on each row.</div>
    <div class="summary-bar">
        <div class="summary-stat"><div class="val" style="color:var(--orange);"><asp:Label ID="lblProductCount" runat="server" Text="0"/></div><div class="lbl">Unmapped</div></div>
        <div class="summary-stat"><div class="val" style="color:var(--teal);"><asp:Label ID="lblProductMapped" runat="server" Text="0"/></div><div class="lbl">Mapped</div></div>
    </div>

    <asp:HiddenField ID="hfSaveProductData" runat="server" Value=""/>
    <asp:Button ID="btnSaveOneProduct" runat="server" OnClick="btnSaveOneProduct_Click" style="display:none;"/>

    <asp:Repeater ID="rptUnmappedProducts" runat="server">
        <HeaderTemplate>
            <div style="max-height:600px;overflow-y:auto;">
            <table class="map-table" id="tblProducts">
            <thead><tr>
                <th style="width:30px;">#</th>
                <th>Tally Product Name</th>
                <th>ERP Product — Packing Option</th>
                <th style="width:80px;">MRP</th>
                <th style="width:70px;"></th>
            </tr></thead><tbody>
        </HeaderTemplate>
        <ItemTemplate>
            <tr data-tally='<%# System.Web.HttpUtility.HtmlAttributeEncode(Eval("TallyName").ToString()) %>'>
                <td style="color:var(--text-dim);"><%# Container.ItemIndex + 1 %></td>
                <td class="tally-name"><%# Eval("TallyName") %></td>
                <td><%# RenderProductFGDropdown(Eval("TallyName")) %></td>
                <td><%# RenderMRPInput(Eval("TallyName")) %></td>
                <td><button type="button" class="btn-save-row" onclick="saveProductRow(this);">Save</button></td>
            </tr>
        </ItemTemplate>
        <FooterTemplate></tbody></table></div></FooterTemplate>
    </asp:Repeater>
</div>
</asp:Panel>

<!-- ═══ SCRAP TAB ═══ -->
<asp:Panel ID="pnlScrap" runat="server" Visible="false">
<div class="card">
    <div class="card-title">&#9851; Scrap / Misc Item Mapping</div>
    <div class="card-sub">Map scrap items to ERP scrap materials. Click Save on each row.</div>
    <div class="summary-bar">
        <div class="summary-stat"><div class="val" style="color:var(--orange);"><asp:Label ID="lblScrapCount" runat="server" Text="0"/></div><div class="lbl">Unmapped</div></div>
        <div class="summary-stat"><div class="val" style="color:var(--teal);"><asp:Label ID="lblScrapMapped" runat="server" Text="0"/></div><div class="lbl">Mapped</div></div>
    </div>

    <asp:HiddenField ID="hfSaveScrapData" runat="server" Value=""/>
    <asp:Button ID="btnSaveOneScrap" runat="server" OnClick="btnSaveOneScrap_Click" style="display:none;"/>

    <asp:Repeater ID="rptUnmappedScrap" runat="server">
        <HeaderTemplate>
            <div style="max-height:500px;overflow-y:auto;">
            <table class="map-table" id="tblScrap">
            <thead><tr>
                <th style="width:30px;">#</th>
                <th>Tally Item Name</th>
                <th>ERP Scrap Material</th>
                <th style="width:70px;"></th>
            </tr></thead><tbody>
        </HeaderTemplate>
        <ItemTemplate>
            <tr data-tally='<%# System.Web.HttpUtility.HtmlAttributeEncode(Eval("TallyName").ToString()) %>'>
                <td style="color:var(--text-dim);"><%# Container.ItemIndex + 1 %></td>
                <td class="tally-name"><%# Eval("TallyName") %></td>
                <td><%# RenderScrapDropdown(Eval("TallyName")) %></td>
                <td><button type="button" class="btn-save-row" onclick="saveScrapRow(this);">Save</button></td>
            </tr>
        </ItemTemplate>
        <FooterTemplate></tbody></table></div></FooterTemplate>
    </asp:Repeater>
</div>
</asp:Panel>

<!-- ═══ CUSTOMERS TAB ═══ -->
<asp:Panel ID="pnlCustomers" runat="server" Visible="false">
<div class="card">
    <div class="card-title">&#x1F465; Customer Mapping</div>
    <div class="card-sub">Customers auto-matched on upload. Remaining unmatched shown below — click Save on each row.</div>
    <div class="summary-bar">
        <div class="summary-stat"><div class="val" style="color:var(--orange);"><asp:Label ID="lblCustomerCount" runat="server" Text="0"/></div><div class="lbl">Unmapped</div></div>
        <div class="summary-stat"><div class="val" style="color:var(--teal);"><asp:Label ID="lblCustomerMapped" runat="server" Text="0"/></div><div class="lbl">Mapped</div></div>
    </div>

    <asp:HiddenField ID="hfSaveCustomerData" runat="server" Value=""/>
    <asp:Button ID="btnSaveOneCustomer" runat="server" OnClick="btnSaveOneCustomer_Click" style="display:none;"/>

    <asp:Repeater ID="rptUnmappedCustomers" runat="server">
        <HeaderTemplate>
            <div style="max-height:600px;overflow-y:auto;">
            <table class="map-table" id="tblCustomers">
            <thead><tr>
                <th style="width:30px;">#</th>
                <th>Tally Customer Name</th>
                <th>ERP Customer</th>
                <th style="width:70px;"></th>
            </tr></thead><tbody>
        </HeaderTemplate>
        <ItemTemplate>
            <tr data-tally='<%# System.Web.HttpUtility.HtmlAttributeEncode(Eval("TallyName").ToString()) %>'>
                <td style="color:var(--text-dim);"><%# Container.ItemIndex + 1 %></td>
                <td class="tally-name"><%# Eval("TallyName") %></td>
                <td><%# RenderCustomerDropdown(Eval("TallyName")) %></td>
                <td><button type="button" class="btn-save-row" onclick="saveCustomerRow(this);">Save</button></td>
            </tr>
        </ItemTemplate>
        <FooterTemplate></tbody></table></div></FooterTemplate>
    </asp:Repeater>
</div>
</asp:Panel>

</asp:Panel>

</div>
</form>
<script>
function saveProductRow(btn) {
    var row = btn.closest('tr');
    var tally = row.getAttribute('data-tally');
    var sel = row.querySelector('select');
    var mrpInput = row.querySelector('input[name^="mrp_"]');
    if (!sel || !sel.value) { alert('Please select a product + packing option.'); return; }
    var mrp = mrpInput ? mrpInput.value : '';
    document.getElementById('<%= hfSaveProductData.ClientID %>').value = tally + '||' + sel.value + '||' + mrp;
    document.getElementById('<%= btnSaveOneProduct.ClientID %>').click();
}
function saveScrapRow(btn) {
    var row = btn.closest('tr');
    var tally = row.getAttribute('data-tally');
    var sel = row.querySelector('select');
    if (!sel || !sel.value || sel.value === '0') { alert('Please select a scrap material.'); return; }
    document.getElementById('<%= hfSaveScrapData.ClientID %>').value = tally + '||' + sel.value;
    document.getElementById('<%= btnSaveOneScrap.ClientID %>').click();
}
function saveCustomerRow(btn) {
    var row = btn.closest('tr');
    var tally = row.getAttribute('data-tally');
    var sel = row.querySelector('select');
    if (!sel || !sel.value || sel.value === '0') { alert('Please select a customer.'); return; }
    document.getElementById('<%= hfSaveCustomerData.ClientID %>').value = tally + '||' + sel.value;
    document.getElementById('<%= btnSaveOneCustomer.ClientID %>').click();
}
</script>
<script src="/StockApp/erp-keepalive.js"></script>
</body>
</html>
