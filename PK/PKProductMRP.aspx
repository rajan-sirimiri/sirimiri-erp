<%@ Page Language="C#" AutoEventWireup="true" Inherits="PKApp.PKProductMRP" EnableEventValidation="false" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri PK — Product MRP</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--accent:#1a9a6c;--accent-dark:#148a5b;--text:#1a1a1a;--text-muted:#666;
    --bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--radius:14px;--nav-h:52px;}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);color:var(--text);min-height:100vh;}
nav{background:#1a1a1a;height:var(--nav-h);display:flex;align-items:center;padding:0 20px;gap:12px;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;}
.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.08em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:16px;}
.nav-link{color:#fff;font-size:12px;font-weight:600;text-decoration:none;opacity:.8;}.nav-link:hover{opacity:1;}
.page-body{max-width:1100px;margin:0 auto;padding:20px 20px 60px;}
.card{background:var(--surface);border-radius:var(--radius);box-shadow:0 2px 12px rgba(0,0,0,.07);padding:20px 24px;margin-bottom:20px;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.06em;margin-bottom:4px;}
.card-sub{font-size:12px;color:var(--text-muted);margin-bottom:16px;}
.alert{padding:12px 18px;border-radius:10px;font-size:13px;font-weight:600;margin-bottom:16px;}
.alert-success{background:#eafaf1;color:#27ae60;border:1px solid #a9dfbf;}
.alert-danger{background:#fdf3f2;color:#e74c3c;border:1px solid #f5c6cb;}
table.data{width:100%;border-collapse:collapse;font-size:12px;}
table.data th{text-align:left;padding:8px 10px;background:#f8f7f5;border-bottom:2px solid var(--border);
    font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-muted);}
table.data td{padding:6px 10px;border-bottom:1px solid #f0f0f0;}
table.data tr:hover{background:#fafafa;}
.mrp-input{width:80px;padding:6px 8px;border:1.5px solid var(--border);border-radius:6px;
    font-size:13px;font-weight:600;text-align:right;font-family:'DM Sans',sans-serif;}
.mrp-input:focus{outline:none;border-color:var(--accent);background:#eafaf1;}
.btn{border:none;border-radius:9px;padding:10px 24px;font-size:12px;font-weight:700;cursor:pointer;letter-spacing:.04em;}
.btn-green{background:#27ae60;color:#fff;}.btn-green:hover{background:#219a52;}
.prod-code{font-size:10px;color:var(--text-muted);}
</style>
</head>
<body>
<form id="form1" runat="server">
<asp:HiddenField ID="hfMRPData" runat="server" Value=""/>

<nav>
    <a class="nav-logo" href="PKHome.aspx">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    </a>
    <span class="nav-title">PK — Product MRP</span>
    <div class="nav-right">
        <a href="PKHome.aspx" class="nav-link">&#8592; PK Home</a>
        <a href="PKLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>

<div class="page-body">

    <asp:Panel ID="pnlAlert" runat="server" Visible="false">
        <div class="alert"><asp:Label ID="lblAlert" runat="server"/></div>
    </asp:Panel>

    <div class="card">
        <div class="card-title">&#x1F4B0; Product MRP Configuration</div>
        <div class="card-sub">Set MRP (inclusive of GST) per selling form. Leave blank if not applicable.</div>
        <div style="margin-bottom:14px;">
            <asp:Button ID="btnSave" runat="server" CssClass="btn btn-green"
                Text="&#x1F4BE; Save All MRP" OnClick="btnSave_Click" CausesValidation="false"
                OnClientClick="return collectMRP();"/>
        </div>

        <asp:Repeater ID="rptProducts" runat="server">
            <HeaderTemplate>
                <table class="data">
                <tr>
                    <th>Product</th>
                    <th>HSN</th>
                    <th>GST %</th>
                    <th style="text-align:right;">MRP — PCS (₹)</th>
                    <th style="text-align:right;">MRP — JAR/BOX (₹)</th>
                    <th style="text-align:right;">MRP — CASE (₹)</th>
                </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td>
                        <strong><%# Eval("ProductName") %></strong>
                        <div class="prod-code"><%# Eval("ProductCode") %></div>
                    </td>
                    <td><%# Eval("HSNCode") %></td>
                    <td><%# Eval("GSTRate") != DBNull.Value ? Convert.ToDecimal(Eval("GSTRate")).ToString("0.#") + "%" : "—" %></td>
                    <td style="text-align:right;">
                        <input type="text" class="mrp-input" data-pid='<%# Eval("ProductID") %>' data-form="PCS"
                            value='<%# Eval("MRP_PCS") != DBNull.Value && Convert.ToDecimal(Eval("MRP_PCS")) > 0 ? Convert.ToDecimal(Eval("MRP_PCS")).ToString("0.##") : "" %>'
                            placeholder="—"/>
                    </td>
                    <td style="text-align:right;">
                        <input type="text" class="mrp-input" data-pid='<%# Eval("ProductID") %>' data-form='<%# Eval("ContainerType") != DBNull.Value ? Eval("ContainerType").ToString() : "JAR" %>'
                            value='<%# GetJarBoxMRP(Eval("MRP_JAR"), Eval("MRP_BOX"), Eval("ContainerType")) %>'
                            placeholder="—"/>
                    </td>
                    <td style="text-align:right;">
                        <input type="text" class="mrp-input" data-pid='<%# Eval("ProductID") %>' data-form="CASE"
                            value='<%# Eval("MRP_CASE") != DBNull.Value && Convert.ToDecimal(Eval("MRP_CASE")) > 0 ? Convert.ToDecimal(Eval("MRP_CASE")).ToString("0.##") : "" %>'
                            placeholder="—"/>
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></table></FooterTemplate>
        </asp:Repeater>
    </div>
</div>

</form>
<script>
function collectMRP() {
    var inputs = document.querySelectorAll('.mrp-input');
    var pairs = [];
    for (var i = 0; i < inputs.length; i++) {
        var pid = inputs[i].getAttribute('data-pid');
        var form = inputs[i].getAttribute('data-form');
        var val = inputs[i].value.trim();
        if (val) pairs.push(pid + ':' + form + ':' + val);
    }
    document.getElementById('<%= hfMRPData.ClientID %>').value = pairs.join(',');
    return true;
}
</script>
<script src="/StockApp/erp-keepalive.js"></script>
</body>
</html>
