<%@ Page Language="C#" AutoEventWireup="true" Inherits="PKApp.PKPrimaryPacking" EnableEventValidation="false" %>
<!DOCTYPE html><html lang="en"><head runat="server">
<meta charset="utf-8"/><meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Primary Packing</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<style>
:root{--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--accent:#e67e22;--accent-dark:#d35400;--teal:#1a9e6a;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--radius:12px;}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);min-height:100vh;color:var(--text);}
nav{background:#1a1a1a;height:52px;display:flex;align-items:center;padding:0 24px;gap:12px;position:sticky;top:0;z-index:100;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;}
.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:17px;letter-spacing:.08em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:12px;}
.nav-user{color:rgba(255,255,255,.8);font-size:12px;}
.nav-link{color:#aaa;text-decoration:none;font-size:11px;font-weight:600;text-transform:uppercase;padding:5px 10px;border-radius:5px;}
.nav-link:hover{color:#fff;background:rgba(255,255,255,.08);}
.page-header{background:var(--surface);border-bottom:1px solid var(--border);padding:18px 32px;}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:26px;letter-spacing:.07em;}
.page-title span{color:var(--accent);}
.page-sub{font-size:12px;color:var(--text-muted);margin-top:2px;}
.main{max-width:1100px;margin:24px auto;padding:0 28px;}
.card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:20px 24px;margin-bottom:20px;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:14px;letter-spacing:.08em;color:var(--text-muted);margin-bottom:14px;padding-bottom:10px;border-bottom:1px solid var(--border);}
.form-grid{display:grid;grid-template-columns:1fr 1fr 1fr;gap:12px;}
.form-group{display:flex;flex-direction:column;gap:5px;}
label{font-size:11px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;color:var(--text-muted);}
select,input[type=number],input[type=text]{width:100%;padding:9px 12px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:13px;background:#fafafa;outline:none;}
select:focus,input:focus{border-color:var(--accent);background:#fff;}
.btn-primary{background:var(--accent);color:#fff;border:none;border-radius:8px;padding:10px 24px;font-size:12px;font-weight:700;cursor:pointer;margin-top:16px;}
.btn-primary:hover{background:var(--accent-dark);}
.alert{padding:10px 14px;border-radius:8px;font-size:13px;font-weight:600;margin-bottom:14px;}
.alert-success{background:#eafaf1;color:var(--teal);border:1px solid #a9dfbf;}
.alert-danger{background:#fdf3f2;color:#e74c3c;border:1px solid #f5c6cb;}
.batch-table,.log-table{width:100%;border-collapse:collapse;font-size:13px;}
.batch-table th,.log-table th{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-muted);padding:9px 12px;border-bottom:2px solid var(--border);text-align:left;background:#fafafa;}
.batch-table td,.log-table td{padding:10px 12px;border-bottom:1px solid var(--border);vertical-align:middle;}
.batch-table tr:last-child td,.log-table tr:last-child td{border-bottom:none;}
.badge-pending{background:#fff3e0;color:var(--accent);font-size:10px;font-weight:700;padding:2px 8px;border-radius:10px;}
.qty-highlight{font-weight:700;color:var(--accent);}
.empty-note{text-align:center;padding:28px;color:var(--text-dim);font-size:13px;}
</style></head><body>
<form id="form1" runat="server">
<nav>
    <a class="nav-logo" href="PKHome.aspx"><img src="Sirimiri_Logo-16_9-72ppi-01.png" alt="" onerror="this.style.display='none'"/></a>
    <span class="nav-title">Packing &amp; Shipments</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblUser" runat="server"/></span>
        <a href="PKHome.aspx" class="nav-link">&#8592; Home</a>
        
                <a href="/StockApp/ERPHome.aspx" class="nav-link">&#x2302; ERP Home</a>
        <a href="PKLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>
<div class="page-header">
    <div class="page-title">Primary <span>Packing</span></div>
    <div class="page-sub">Fill finished product from completed production batches into primary containers</div>
</div>
<div class="main">
    <asp:Panel ID="pnlAlert" runat="server" Visible="false">
        <div class="alert"><asp:Label ID="lblAlert" runat="server"/></div>
    </asp:Panel>

    <!-- ENTRY FORM -->
    <div class="card">
        <div class="card-title">Record Primary Packing</div>
        <div class="form-grid">
            <div class="form-group">
                <label>Production Batch <span style="color:var(--accent)">*</span></label>
                <asp:DropDownList ID="ddlBatch" runat="server" OnSelectedIndexChanged="ddlBatch_Changed" AutoPostBack="true"/>
            </div>
            <div class="form-group">
                <label>Qty to Pack <span style="color:var(--accent)">*</span></label>
                <input type="number" id="txtQtyPack" runat="server" step="0.001" min="0.001" placeholder="0.000"/>
            </div>
            <div class="form-group">
                <label>Packing Material Used</label>
                <asp:DropDownList ID="ddlPM" runat="server"/>
            </div>
            <div class="form-group">
                <label>PM Qty Used</label>
                <input type="number" id="txtPMQty" runat="server" step="0.001" min="0" placeholder="0.000"/>
            </div>
        </div>
        <div style="margin-top:6px;font-size:12px;color:var(--text-muted);">
            Pending from batch: <strong><asp:Label ID="lblPending" runat="server">—</asp:Label></strong>
        </div>
        <asp:Button ID="btnPack" runat="server" Text="&#x2713; Record Packing" CssClass="btn-primary"
            OnClick="btnPack_Click" CausesValidation="false"/>
    </div>

    <!-- PENDING BATCHES -->
    <div class="card">
        <div class="card-title">Completed Batches — Pending Primary Packing</div>
        <asp:Panel ID="pnlBatchEmpty" runat="server"><div class="empty-note">All batches are packed</div></asp:Panel>
        <asp:Panel ID="pnlBatchTable" runat="server" Visible="false">
        <table class="batch-table">
            <thead><tr>
                <th>Product</th><th>Batch</th><th>Order Date</th>
                <th class="num">Actual Output</th><th class="num">Packed</th><th class="num">Pending</th>
            </tr></thead>
            <tbody>
                <asp:Repeater ID="rptBatches" runat="server">
                    <ItemTemplate>
                        <tr>
                            <td><strong><%# Eval("ProductName") %></strong><br/><span style="font-size:11px;color:var(--text-dim)"><%# Eval("ProductCode") %></span></td>
                            <td>Batch <%# Eval("BatchNo") %></td>
                            <td><%# Convert.ToDateTime(Eval("OrderDate")).ToString("dd MMM yyyy") %></td>
                            <td class="num"><%# string.Format("{0:0.###}", Eval("ActualOutput")) %> <%# Eval("OutputUnit") %></td>
                            <td class="num"><%# string.Format("{0:0.###}", Eval("PackedQty")) %></td>
                            <td class="num"><span class="qty-highlight"><%# string.Format("{0:0.###}", Eval("PendingQty")) %></span></td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
        </asp:Panel>
    </div>

    <!-- TODAY'S LOG -->
    <div class="card">
        <div class="card-title">Today's Primary Packing Log</div>
        <asp:Panel ID="pnlLogEmpty" runat="server"><div class="empty-note">No packing recorded today</div></asp:Panel>
        <asp:Panel ID="pnlLogTable" runat="server" Visible="false">
        <table class="log-table">
            <thead><tr><th>Time</th><th>Product</th><th>Batch</th><th class="num">Qty Packed</th><th>Packing Material</th><th class="num">PM Used</th></tr></thead>
            <tbody>
                <asp:Repeater ID="rptLog" runat="server">
                    <ItemTemplate>
                        <tr>
                            <td><%# Convert.ToDateTime(Eval("PackedAt")).ToString("hh:mm tt") %></td>
                            <td><%# Eval("ProductName") %></td>
                            <td><%# Eval("BatchNo") %></td>
                            <td class="num"><strong><%# string.Format("{0:0.###}", Eval("QtyPacked")) %> <%# Eval("Unit") %></strong></td>
                            <td><%# Eval("PMName") == DBNull.Value ? "—" : Eval("PMName") %></td>
                            <td class="num"><%# Eval("QtyUsed") == DBNull.Value ? "—" : string.Format("{0:0.###}", Eval("QtyUsed")) + " " + Eval("PMUnit") %></td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
        </asp:Panel>
    </div>
</div>
</form></body></html>
