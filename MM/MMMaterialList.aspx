<%@ Page Language="C#" AutoEventWireup="true" Inherits="MMApp.MMMaterialList" EnableEventValidation="false" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri — Material Master List</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600;700&display=swap" rel="stylesheet"/>
<style>
:root{ --accent:#cc1e1e; --teal:#1a9e6a; --text:#1a1a1a; --text-dim:#999; --border:#ddd; }
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;color:var(--text);background:#fff;padding:20px;}

.header{display:flex;align-items:center;gap:16px;margin-bottom:20px;padding-bottom:16px;border-bottom:3px solid var(--accent);}
.header img{height:40px;}
.header-text{flex:1;}
.header-title{font-family:'Bebas Neue',sans-serif;font-size:28px;letter-spacing:.06em;}
.header-sub{font-size:11px;color:var(--text-dim);}
.header-date{font-size:11px;color:var(--text-dim);text-align:right;}

.toolbar{display:flex;gap:12px;margin-bottom:20px;align-items:center;}
.btn{border:none;border-radius:8px;padding:10px 20px;font-size:13px;font-weight:700;cursor:pointer;font-family:inherit;}
.btn-print{background:var(--accent);color:#fff;}
.btn-back{background:#f0f0f0;color:#333;border:1px solid #ddd;}
.filter-label{font-size:12px;font-weight:700;color:var(--text-dim);}

.section-title{font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.06em;margin:24px 0 10px;padding:6px 0;border-bottom:2px solid var(--accent);color:var(--accent);}
.section-count{font-size:11px;color:var(--text-dim);font-weight:400;margin-left:8px;}

table{width:100%;border-collapse:collapse;font-size:11px;margin-bottom:20px;}
th{font-size:9px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:#fff;background:var(--accent);padding:8px 6px;text-align:left;}
td{padding:6px;border-bottom:1px solid #eee;vertical-align:top;}
tr:nth-child(even){background:#fafafa;}
.code{font-family:monospace;font-weight:700;color:var(--accent);}
.num{text-align:right;font-family:monospace;}
.inactive{color:#ccc;text-decoration:line-through;}

@media print {
    .toolbar{display:none !important;}
    .header{border-bottom:3px solid #000;}
    body{padding:10px;font-size:10px;}
    th{background:#333 !important;color:#fff !important;-webkit-print-color-adjust:exact;print-color-adjust:exact;}
    tr:nth-child(even){background:#f5f5f5 !important;-webkit-print-color-adjust:exact;print-color-adjust:exact;}
    table{page-break-inside:auto;}
    tr{page-break-inside:avoid;}
    .section-title{page-break-after:avoid;}
}
</style>
</head>
<body>
<form id="form1" runat="server">

<div class="header">
    <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    <div class="header-text">
        <div class="header-title">Material Master List</div>
        <div class="header-sub">Sirimiri Nutrition Food Products Pvt. Ltd.</div>
    </div>
    <div class="header-date">
        Generated: <asp:Label ID="lblDate" runat="server"/><br/>
        By: <asp:Label ID="lblUser" runat="server"/>
    </div>
</div>

<div class="toolbar">
    <button type="button" class="btn btn-print" onclick="window.print();">&#x1F5A8; Print / Save as PDF</button>
    <a href="MMHome.aspx" class="btn btn-back">&#8592; Back to MM Home</a>
    <span class="filter-label">Filter:</span>
    <asp:CheckBox ID="chkRM" runat="server" Text=" Raw Materials" Checked="true" AutoPostBack="true" OnCheckedChanged="chkFilter_Changed"/>
    <asp:CheckBox ID="chkPM" runat="server" Text=" Packing Materials" Checked="true" AutoPostBack="true" OnCheckedChanged="chkFilter_Changed"/>
    <asp:CheckBox ID="chkCN" runat="server" Text=" Consumables" Checked="true" AutoPostBack="true" OnCheckedChanged="chkFilter_Changed"/>
    <asp:CheckBox ID="chkST" runat="server" Text=" Stationaries" Checked="true" AutoPostBack="true" OnCheckedChanged="chkFilter_Changed"/>
    <asp:CheckBox ID="chkInactive" runat="server" Text=" Include Inactive" AutoPostBack="true" OnCheckedChanged="chkFilter_Changed"/>
</div>

<!-- RAW MATERIALS -->
<asp:Panel ID="pnlRM" runat="server">
<div class="section-title">Raw Materials<span class="section-count">(<asp:Label ID="lblRMCount" runat="server"/>)</span></div>
<asp:Repeater ID="rptRM" runat="server">
    <HeaderTemplate>
        <table>
        <thead><tr><th style="width:30px;">#</th><th style="width:70px;">Code</th><th>Name</th><th>Description</th><th style="width:70px;">HSN</th><th style="width:50px;">GST%</th><th style="width:50px;">UOM</th><th style="width:70px;">Reorder</th></tr></thead><tbody>
    </HeaderTemplate>
    <ItemTemplate>
        <tr class='<%# Convert.ToBoolean(Eval("IsActive")) ? "" : "inactive" %>'>
            <td><%# Container.ItemIndex + 1 %></td>
            <td class="code"><%# Eval("RMCode") %></td>
            <td style="font-weight:600;"><%# Eval("RMName") %></td>
            <td style="font-size:10px;color:#666;"><%# Eval("Description") %></td>
            <td><%# Eval("HSNCode") %></td>
            <td class="num"><%# Eval("GSTRate") != DBNull.Value ? string.Format("{0:0.##}", Eval("GSTRate")) : "" %></td>
            <td><%# Eval("Abbreviation") %></td>
            <td class="num"><%# Convert.ToDecimal(Eval("ReorderLevel")) > 0 ? string.Format("{0:N0}", Eval("ReorderLevel")) : "" %></td>
        </tr>
    </ItemTemplate>
    <FooterTemplate></tbody></table></FooterTemplate>
</asp:Repeater>
</asp:Panel>

<!-- PACKING MATERIALS -->
<asp:Panel ID="pnlPM" runat="server">
<div class="section-title">Packing Materials<span class="section-count">(<asp:Label ID="lblPMCount" runat="server"/>)</span></div>
<asp:Repeater ID="rptPM" runat="server">
    <HeaderTemplate>
        <table>
        <thead><tr><th style="width:30px;">#</th><th style="width:70px;">Code</th><th>Name</th><th style="width:80px;">Category</th><th>Description</th><th style="width:70px;">HSN</th><th style="width:50px;">GST%</th><th style="width:50px;">UOM</th><th style="width:70px;">Reorder</th></tr></thead><tbody>
    </HeaderTemplate>
    <ItemTemplate>
        <tr class='<%# Convert.ToBoolean(Eval("IsActive")) ? "" : "inactive" %>'>
            <td><%# Container.ItemIndex + 1 %></td>
            <td class="code"><%# Eval("PMCode") %></td>
            <td style="font-weight:600;"><%# Eval("PMName") %></td>
            <td style="font-size:10px;"><%# Eval("PMCategory") %></td>
            <td style="font-size:10px;color:#666;"><%# Eval("Description") %></td>
            <td><%# Eval("HSNCode") %></td>
            <td class="num"><%# Eval("GSTRate") != DBNull.Value ? string.Format("{0:0.##}", Eval("GSTRate")) : "" %></td>
            <td><%# Eval("Abbreviation") %></td>
            <td class="num"><%# Convert.ToDecimal(Eval("ReorderLevel")) > 0 ? string.Format("{0:N0}", Eval("ReorderLevel")) : "" %></td>
        </tr>
    </ItemTemplate>
    <FooterTemplate></tbody></table></FooterTemplate>
</asp:Repeater>
</asp:Panel>

<!-- CONSUMABLES -->
<asp:Panel ID="pnlCN" runat="server">
<div class="section-title">Consumables<span class="section-count">(<asp:Label ID="lblCNCount" runat="server"/>)</span></div>
<asp:Repeater ID="rptCN" runat="server">
    <HeaderTemplate>
        <table>
        <thead><tr><th style="width:30px;">#</th><th style="width:70px;">Code</th><th>Name</th><th>Description</th><th style="width:70px;">HSN</th><th style="width:50px;">GST%</th><th style="width:50px;">UOM</th><th style="width:70px;">Reorder</th></tr></thead><tbody>
    </HeaderTemplate>
    <ItemTemplate>
        <tr class='<%# Convert.ToBoolean(Eval("IsActive")) ? "" : "inactive" %>'>
            <td><%# Container.ItemIndex + 1 %></td>
            <td class="code"><%# Eval("ConsumableCode") %></td>
            <td style="font-weight:600;"><%# Eval("ConsumableName") %></td>
            <td style="font-size:10px;color:#666;"><%# Eval("Description") %></td>
            <td><%# Eval("HSNCode") %></td>
            <td class="num"><%# Eval("GSTRate") != DBNull.Value ? string.Format("{0:0.##}", Eval("GSTRate")) : "" %></td>
            <td><%# Eval("Abbreviation") %></td>
            <td class="num"><%# Convert.ToDecimal(Eval("ReorderLevel")) > 0 ? string.Format("{0:N0}", Eval("ReorderLevel")) : "" %></td>
        </tr>
    </ItemTemplate>
    <FooterTemplate></tbody></table></FooterTemplate>
</asp:Repeater>
</asp:Panel>

<!-- STATIONARIES -->
<asp:Panel ID="pnlST" runat="server">
<div class="section-title">Stationaries<span class="section-count">(<asp:Label ID="lblSTCount" runat="server"/>)</span></div>
<asp:Repeater ID="rptST" runat="server">
    <HeaderTemplate>
        <table>
        <thead><tr><th style="width:30px;">#</th><th style="width:70px;">Code</th><th>Name</th><th>Description</th><th style="width:70px;">HSN</th><th style="width:50px;">GST%</th><th style="width:50px;">UOM</th><th style="width:70px;">Reorder</th></tr></thead><tbody>
    </HeaderTemplate>
    <ItemTemplate>
        <tr class='<%# Convert.ToBoolean(Eval("IsActive")) ? "" : "inactive" %>'>
            <td><%# Container.ItemIndex + 1 %></td>
            <td class="code"><%# Eval("StationaryCode") %></td>
            <td style="font-weight:600;"><%# Eval("StationaryName") %></td>
            <td style="font-size:10px;color:#666;"><%# Eval("Description") %></td>
            <td><%# Eval("HSNCode") %></td>
            <td class="num"><%# Eval("GSTRate") != DBNull.Value ? string.Format("{0:0.##}", Eval("GSTRate")) : "" %></td>
            <td><%# Eval("Abbreviation") %></td>
            <td class="num"><%# Convert.ToDecimal(Eval("ReorderLevel")) > 0 ? string.Format("{0:N0}", Eval("ReorderLevel")) : "" %></td>
        </tr>
    </ItemTemplate>
    <FooterTemplate></tbody></table></FooterTemplate>
</asp:Repeater>
</asp:Panel>

</form>
</body>
</html>
