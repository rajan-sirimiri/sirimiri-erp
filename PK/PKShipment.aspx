<%@ Page Language="C#" AutoEventWireup="true" Inherits="PKApp.PKShipment" EnableEventValidation="false" %>
<!DOCTYPE html><html lang="en"><head runat="server">
<meta charset="utf-8"/><meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Shipments</title>
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
.page-header{background:var(--surface);border-bottom:1px solid var(--border);padding:18px 32px;display:flex;justify-content:space-between;align-items:center;}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:26px;letter-spacing:.07em;}
.page-title span{color:var(--accent);}
.page-sub{font-size:12px;color:var(--text-muted);margin-top:2px;}
.main{max-width:1100px;margin:24px auto;padding:0 28px;}
.tab-bar{display:flex;gap:4px;background:#f0f0f0;padding:4px;border-radius:10px;margin-bottom:20px;width:fit-content;}
.tab{padding:7px 18px;border-radius:8px;font-size:12px;font-weight:700;cursor:pointer;border:none;background:transparent;color:var(--text-muted);}
.tab.active{background:#fff;color:var(--text);box-shadow:0 1px 4px rgba(0,0,0,.1);}
.card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:20px 24px;margin-bottom:20px;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:14px;letter-spacing:.08em;color:var(--text-muted);margin-bottom:14px;padding-bottom:10px;border-bottom:1px solid var(--border);}
.form-grid{display:grid;grid-template-columns:1fr 1fr 1fr;gap:12px;}
.form-group{display:flex;flex-direction:column;gap:5px;}
.form-group.full{grid-column:1/-1;}
label{font-size:11px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;color:var(--text-muted);}
select,input[type=text],input[type=number],input[type=date],textarea{width:100%;padding:9px 12px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:13px;background:#fafafa;outline:none;}
select:focus,input:focus,textarea:focus{border-color:var(--accent);background:#fff;}
.btn-primary{background:var(--accent);color:#fff;border:none;border-radius:8px;padding:9px 20px;font-size:12px;font-weight:700;cursor:pointer;}
.btn-primary:hover{background:var(--accent-dark);}
.btn-secondary{background:#f0f0f0;color:#333;border:1px solid var(--border);border-radius:8px;padding:8px 14px;font-size:12px;font-weight:600;cursor:pointer;}
.btn-sm{background:var(--accent);color:#fff;border:none;border-radius:6px;padding:5px 12px;font-size:11px;font-weight:700;cursor:pointer;}
.form-actions{display:flex;gap:10px;margin-top:14px;}
.alert{padding:10px 14px;border-radius:8px;font-size:13px;font-weight:600;margin-bottom:14px;}
.alert-success{background:#eafaf1;color:var(--teal);border:1px solid #a9dfbf;}
.alert-danger{background:#fdf3f2;color:#e74c3c;border:1px solid #f5c6cb;}
.data-table{width:100%;border-collapse:collapse;font-size:13px;}
.data-table th{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-muted);padding:9px 12px;border-bottom:2px solid var(--border);text-align:left;background:#fafafa;}
.data-table th.num{text-align:right;}
.data-table td{padding:10px 12px;border-bottom:1px solid var(--border);vertical-align:middle;}
.data-table td.num{text-align:right;}
.data-table tr:last-child td{border-bottom:none;}
.badge{display:inline-block;padding:2px 8px;border-radius:10px;font-size:10px;font-weight:700;}
.badge-open{background:#eafaf1;color:var(--teal);}
.badge-dispatched{background:#e3f2fd;color:#1565c0;}
.line-row{display:flex;gap:8px;align-items:center;margin-bottom:8px;}
.line-row select{flex:1;}
.line-row input{width:120px;}
.empty-note{text-align:center;padding:28px;color:var(--text-dim);font-size:13px;}
</style></head><body>
<form id="form1" runat="server">
<asp:HiddenField ID="hfTab" runat="server" Value="po"/>
<asp:HiddenField ID="hfPOID" runat="server" Value="0"/>
<nav>
    <a class="nav-logo" href="PKHome.aspx"><img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="" onerror="this.style.display='none'"/></a>
    <span class="nav-title">Packing &amp; Shipments</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblUser" runat="server"/></span>
        <a href="/StockApp/ERPHome.aspx" class="nav-link">&#x2302; ERP Home</a>
        <a href="PKLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>
<div class="page-header">
    <div>
        <div class="page-title">Customer <span>Orders &amp; Shipments</span></div>
        <div class="page-sub">Manage customer POs and dispatch shipments with challan</div>
    </div>
</div>
<div class="main">
    <asp:Panel ID="pnlAlert" runat="server" Visible="false">
        <div class="alert"><asp:Label ID="lblAlert" runat="server"/></div>
    </asp:Panel>

    <div class="tab-bar">
        <asp:Button ID="btnTabPO" runat="server" Text="Customer POs" CssClass="tab active" OnClick="btnTabPO_Click" CausesValidation="false"/>
        <asp:Button ID="btnTabShip" runat="server" Text="Shipments" CssClass="tab" OnClick="btnTabShip_Click" CausesValidation="false"/>
    </div>

    <!-- PO TAB -->
    <asp:Panel ID="pnlPO" runat="server">
        <div class="card">
            <div class="card-title">New Customer PO</div>
            <div class="form-grid">
                <div class="form-group">
                    <label>Customer <span style="color:var(--accent)">*</span></label>
                    <asp:DropDownList ID="ddlPOCustomer" runat="server"/>
                </div>
                <div class="form-group">
                    <label>PO Date <span style="color:var(--accent)">*</span></label>
                    <input type="date" id="txtPODate" runat="server"/>
                </div>
                <div class="form-group">
                    <label>Delivery Date</label>
                    <input type="date" id="txtDeliveryDate" runat="server"/>
                </div>
                <div class="form-group full">
                    <label>Remarks</label>
                    <input type="text" id="txtPORemarks" runat="server" placeholder="Optional"/>
                </div>
            </div>
            <div style="margin-top:16px;">
                <div class="card-title" style="font-size:12px;margin-bottom:10px;">PO Line Items</div>
                <asp:Repeater ID="rptPOLines" runat="server">
                    <ItemTemplate>
                        <div class="line-row">
                            <asp:DropDownList runat="server" DataSourceID="" CssClass="line-product">
                            </asp:DropDownList>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <div id="poLines">
                    <div class="line-row">
                        <select name="po_product_1"><asp:Literal ID="litProducts" runat="server"/></select>
                        <input type="number" name="po_qty_1" step="0.001" min="0.001" placeholder="Qty" style="width:100px;padding:9px 10px;border:1.5px solid var(--border);border-radius:8px;font-size:13px;"/>
                        <input type="number" name="po_price_1" step="0.01" min="0" placeholder="Price/unit" style="width:110px;padding:9px 10px;border:1.5px solid var(--border);border-radius:8px;font-size:13px;"/>
                    </div>
                </div>
                <button type="button" onclick="addPOLine()" class="btn-secondary" style="margin-top:8px;font-size:11px;">+ Add Line</button>
            </div>
            <div class="form-actions">
                <asp:Button ID="btnSavePO" runat="server" Text="Save PO" CssClass="btn-primary" OnClick="btnSavePO_Click" CausesValidation="false"/>
            </div>
        </div>

        <div class="card">
            <div class="card-title">All Customer POs</div>
            <asp:Panel ID="pnlPOEmpty" runat="server"><div class="empty-note">No POs created yet</div></asp:Panel>
            <asp:Panel ID="pnlPOTable" runat="server" Visible="false">
            <table class="data-table">
                <thead><tr><th>PO Code</th><th>Customer</th><th>PO Date</th><th>Delivery Date</th><th>Lines</th><th>Status</th></tr></thead>
                <tbody>
                    <asp:Repeater ID="rptPOs" runat="server">
                        <ItemTemplate><tr>
                            <td><strong><%# Eval("POCode") %></strong></td>
                            <td><%# Eval("CustomerName") %></td>
                            <td><%# Convert.ToDateTime(Eval("PODate")).ToString("dd MMM yyyy") %></td>
                            <td><%# Eval("DeliveryDate") == DBNull.Value ? "—" : Convert.ToDateTime(Eval("DeliveryDate")).ToString("dd MMM yyyy") %></td>
                            <td><%# Eval("LineCount") %> products</td>
                            <td><span class='badge <%# Eval("Status").ToString() == "Open" ? "badge-open" : "badge-dispatched" %>'><%# Eval("Status") %></span></td>
                        </tr></ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
            </asp:Panel>
        </div>
    </asp:Panel>

    <!-- SHIPMENT TAB -->
    <asp:Panel ID="pnlShip" runat="server" Visible="false">
        <div class="card">
            <div class="card-title">Create Shipment</div>
            <div class="form-grid">
                <div class="form-group">
                    <label>Customer PO <span style="color:var(--accent)">*</span></label>
                    <asp:DropDownList ID="ddlShipPO" runat="server" OnSelectedIndexChanged="ddlShipPO_Changed" AutoPostBack="true"/>
                </div>
                <div class="form-group">
                    <label>Ship Date <span style="color:var(--accent)">*</span></label>
                    <input type="date" id="txtShipDate" runat="server"/>
                </div>
                <div class="form-group">
                    <label>Vehicle No</label>
                    <input type="text" id="txtVehicle" runat="server" placeholder="e.g. TN 01 AB 1234"/>
                </div>
                <div class="form-group">
                    <label>Driver Name</label>
                    <input type="text" id="txtDriver" runat="server" placeholder="Driver name"/>
                </div>
                <div class="form-group full">
                    <label>Remarks</label>
                    <input type="text" id="txtShipRemarks" runat="server" placeholder="Optional"/>
                </div>
            </div>
            <div style="margin-top:14px;">
                <div class="card-title" style="font-size:12px;margin-bottom:8px;">Products to Ship</div>
                <asp:Panel ID="pnlPOLines" runat="server">
                    <table class="data-table">
                        <thead><tr><th>Product</th><th class="num">Ordered</th><th class="num">Shipped</th><th class="num">FG Available</th><th class="num">Dispatch Qty</th></tr></thead>
                        <tbody>
                            <asp:Repeater ID="rptShipLines" runat="server">
                                <ItemTemplate><tr>
                                    <td><%# Eval("ProductName") %><input type="hidden" name='<%# "ship_pid_" + Eval("LineID") %>' value='<%# Eval("ProductID") %>'/></td>
                                    <td class="num"><%# string.Format("{0:0.###}", Eval("QtyOrdered")) %> <%# Eval("Unit") %></td>
                                    <td class="num"><%# string.Format("{0:0.###}", Eval("QtyShipped")) %></td>
                                    <td class="num"><asp:Label runat="server" Text='<%# PKApp.DAL.PKDatabaseHelper.ExecuteQueryPublic("SELECT ROUND(IFNULL(fg.p,0)-IFNULL(sh.s,0),3) AS A FROM (SELECT SUM(QtyPacked) p FROM PK_FGStock WHERE ProductID=?pid) fg, (SELECT IFNULL(SUM(sl.QtyShipped),0) s FROM PK_ShipmentLine sl JOIN PK_Shipment s ON s.ShipmentID=sl.ShipmentID WHERE sl.ProductID=?pid AND s.Status!=\"Cancelled\") sh;", new MySql.Data.MySqlClient.MySqlParameter("?pid", Eval("ProductID"))).Rows.Count > 0 ? PKApp.DAL.PKDatabaseHelper.ExecuteQueryPublic("SELECT ROUND(IFNULL(fg.p,0)-IFNULL(sh.s,0),3) AS A FROM (SELECT SUM(QtyPacked) p FROM PK_FGStock WHERE ProductID=?pid) fg, (SELECT IFNULL(SUM(sl.QtyShipped),0) s FROM PK_ShipmentLine sl JOIN PK_Shipment s ON s.ShipmentID=sl.ShipmentID WHERE sl.ProductID=?pid AND s.Status!=\"Cancelled\") sh;", new MySql.Data.MySqlClient.MySqlParameter("?pid", Eval("ProductID"))).Rows[0]["A"].ToString() : "0" %>' /></td>
                                    <td class="num"><input type="number" name='<%# "ship_qty_" + Eval("LineID") %>' step="0.001" min="0" placeholder="0.000" style="width:100px;padding:7px 9px;border:1.5px solid var(--border);border-radius:7px;font-size:12px;text-align:right;"/></td>
                                </tr></ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </asp:Panel>
                <asp:Panel ID="pnlSelectPO" runat="server"><div style="font-size:12px;color:var(--text-dim);padding:16px;">Select a PO above to see line items</div></asp:Panel>
            </div>
            <div class="form-actions">
                <asp:Button ID="btnCreateShipment" runat="server" Text="&#x1F69A; Create Shipment" CssClass="btn-primary" OnClick="btnCreateShipment_Click" CausesValidation="false"/>
            </div>
        </div>

        <div class="card">
            <div class="card-title">All Shipments</div>
            <asp:Panel ID="pnlShipEmpty" runat="server"><div class="empty-note">No shipments yet</div></asp:Panel>
            <asp:Panel ID="pnlShipTable" runat="server" Visible="false">
            <table class="data-table">
                <thead><tr><th>Shipment</th><th>Customer</th><th>PO</th><th>Ship Date</th><th>Vehicle</th><th>Lines</th><th>Status</th></tr></thead>
                <tbody>
                    <asp:Repeater ID="rptShipments" runat="server">
                        <ItemTemplate><tr>
                            <td><strong><%# Eval("ShipmentCode") %></strong></td>
                            <td><%# Eval("CustomerName") %></td>
                            <td><%# Eval("POCode") %></td>
                            <td><%# Convert.ToDateTime(Eval("ShipDate")).ToString("dd MMM yyyy") %></td>
                            <td><%# Eval("VehicleNo") == DBNull.Value ? "—" : Eval("VehicleNo") %></td>
                            <td><%# Eval("Lines") %></td>
                            <td><span class="badge badge-dispatched"><%# Eval("Status") %></span></td>
                        </tr></ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
            </asp:Panel>
        </div>
    </asp:Panel>
</div>
</form>
<script>
var lineCount = 1;
var productOptions = document.querySelector('#poLines select')?.innerHTML || '';
function addPOLine() {
    lineCount++;
    var div = document.createElement('div');
    div.className = 'line-row';
    div.innerHTML = '<select name="po_product_' + lineCount + '">' + productOptions + '</select>' +
        '<input type="number" name="po_qty_' + lineCount + '" step="0.001" min="0.001" placeholder="Qty" style="width:100px;padding:9px 10px;border:1.5px solid var(--border);border-radius:8px;font-size:13px;"/>' +
        '<input type="number" name="po_price_' + lineCount + '" step="0.01" min="0" placeholder="Price/unit" style="width:110px;padding:9px 10px;border:1.5px solid var(--border);border-radius:8px;font-size:13px;"/>';
    document.getElementById('poLines').appendChild(div);
}
</script>
</body></html>
