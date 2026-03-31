<%@ Page Language="C#" AutoEventWireup="true" Inherits="PKApp.PKProductPM" %>
<!DOCTYPE html><html lang="en"><head runat="server">
<meta charset="utf-8"/><meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Product PM Mapping — Packing</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
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
.layout{max-width:1100px;margin:24px auto;padding:0 28px;display:grid;grid-template-columns:340px 1fr;gap:20px;align-items:start;}
.card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:20px 22px;margin-bottom:16px;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:14px;letter-spacing:.08em;color:var(--text-muted);margin-bottom:14px;padding-bottom:10px;border-bottom:1px solid var(--border);}
.form-group{display:flex;flex-direction:column;gap:4px;margin-bottom:12px;}
label{font-size:11px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;color:var(--text-muted);}
input,select{width:100%;padding:9px 12px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:13px;color:var(--text);background:#fafafa;outline:none;}
input:focus,select:focus{border-color:var(--accent);background:#fff;}
.form-row{display:grid;grid-template-columns:1fr 1fr;gap:10px;}
.form-actions{display:flex;gap:8px;margin-top:14px;}
.btn-primary{background:var(--accent);color:#fff;border:none;border-radius:8px;padding:9px 20px;font-size:12px;font-weight:700;cursor:pointer;}
.btn-primary:hover{background:var(--accent-dark);}
.btn-secondary{background:#f0f0f0;color:#333;border:1px solid var(--border);border-radius:8px;padding:8px 14px;font-size:12px;font-weight:600;cursor:pointer;}
.btn-danger{background:transparent;color:#c0392b;border:1px solid #c0392b;border-radius:8px;padding:5px 10px;font-size:11px;font-weight:700;cursor:pointer;}
.btn-danger:hover{background:#c0392b;color:#fff;}
.alert{padding:10px 14px;border-radius:8px;font-size:13px;font-weight:600;margin-bottom:12px;}
.alert-success{background:#eafaf1;color:var(--teal);border:1px solid #a9dfbf;}
.alert-danger{background:#fdf3f2;color:#e74c3c;border:1px solid #f5c6cb;}
/* ── Product selector cards ── */
.prod-list{display:flex;flex-direction:column;gap:8px;max-height:520px;overflow-y:auto;}
.prod-row{display:flex;align-items:center;justify-content:space-between;padding:10px 14px;background:#fafafa;border:1px solid var(--border);border-radius:9px;cursor:pointer;transition:border-color .2s;}
.prod-row:hover{border-color:var(--accent);}
.prod-row.selected{border-color:var(--accent);background:#fef9f3;}
.prod-name{font-weight:700;font-size:13px;}
.prod-meta{font-size:11px;color:var(--text-dim);}
.pm-count{display:inline-block;padding:2px 8px;border-radius:10px;font-size:10px;font-weight:700;background:#eafaf1;color:var(--teal);}
.pm-count.zero{background:#f0f0f0;color:var(--text-dim);}
/* ── Mapping table ── */
table.mapping{width:100%;border-collapse:collapse;font-size:13px;}
table.mapping th{font-size:11px;font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:var(--text-muted);text-align:left;padding:8px 10px;border-bottom:2px solid var(--border);}
table.mapping td{padding:8px 10px;border-bottom:1px solid var(--border);vertical-align:middle;}
table.mapping tr:last-child td{border-bottom:none;}
.level-badge{display:inline-block;padding:2px 8px;border-radius:10px;font-size:10px;font-weight:700;text-transform:uppercase;}
.level-UNIT{background:#e8f4fd;color:#2980b9;}
.level-CONTAINER{background:#fef3e2;color:#e67e22;}
.level-CASE{background:#eafaf1;color:#1a9e6a;}
.empty-note{text-align:center;padding:28px;color:var(--text-dim);font-size:13px;}
.search-box{margin-bottom:12px;}
.search-box input{padding:8px 12px;font-size:12px;}
</style></head><body>
<form id="form1" runat="server">
<asp:HiddenField ID="hfProductID" runat="server" Value="0"/>
<asp:HiddenField ID="hfMappingID" runat="server" Value="0"/>
<nav>
    <a class="nav-logo" href="PKHome.aspx"><img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="" onerror="this.style.display='none'"/></a>
    <span class="nav-title">Packing &amp; Shipments</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblUser" runat="server"/></span>
        <a href="/StockApp/ERPHome.aspx" class="nav-link">&#x2302; ERP Home</a>
        <a href="PKHome.aspx" class="nav-link">PK Home</a>
        <a href="PKLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>
<div class="page-header">
    <div class="page-title">Product <span>PM Mapping</span></div>
    <div class="page-sub">Define which packing materials are consumed for each product — per unit, per container, or per case</div>
</div>
<div class="layout">
    <!-- ── LEFT: Product List ── -->
    <div>
        <div class="card">
            <div class="card-title">Products (<asp:Label ID="lblProdCount" runat="server"/>)</div>
            <asp:Panel ID="pnlProdEmpty" runat="server"><div class="empty-note">No active products found</div></asp:Panel>
            <div class="prod-list">
                <asp:Repeater ID="rptProducts" runat="server" OnItemCommand="rptProducts_Cmd">
                    <ItemTemplate>
                        <asp:LinkButton runat="server" CommandName="SelectProduct" CommandArgument='<%# Eval("ProductID") %>'
                            CausesValidation="false" style="text-decoration:none;color:inherit;display:block;">
                            <div class='prod-row <%# Eval("ProductID").ToString() == hfProductID.Value ? "selected" : "" %>'>
                                <div>
                                    <div class="prod-name"><%# Eval("ProductName") %></div>
                                    <div class="prod-meta"><%# Eval("ProductCode") %> &nbsp;·&nbsp; <%# Eval("ContainerType") %></div>
                                </div>
                                <span class='pm-count <%# Convert.ToInt32(Eval("PMCount")) == 0 ? "zero" : "" %>'><%# Eval("PMCount") %> PMs</span>
                            </div>
                        </asp:LinkButton>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>

    <!-- ── RIGHT: Mapping Form + Table ── -->
    <div>
        <asp:Panel ID="pnlAlert" runat="server" Visible="false">
            <div class="alert"><asp:Label ID="lblAlert" runat="server"/></div>
        </asp:Panel>

        <!-- No product selected state -->
        <asp:Panel ID="pnlNoProduct" runat="server">
            <div class="card"><div class="empty-note">Select a product from the list to manage its packing material mapping</div></div>
        </asp:Panel>

        <!-- Product selected: form + existing mappings -->
        <asp:Panel ID="pnlMapping" runat="server" Visible="false">
            <!-- Product info bar -->
            <div class="card" style="padding:14px 22px;">
                <div style="display:flex;align-items:center;justify-content:space-between;">
                    <div>
                        <span style="font-weight:700;font-size:15px;"><asp:Label ID="lblProductName" runat="server"/></span>
                        <span style="color:var(--text-dim);font-size:12px;margin-left:8px;"><asp:Label ID="lblProductMeta" runat="server"/></span>
                    </div>
                    <span style="font-size:12px;color:var(--text-muted);">Container: <strong><asp:Label ID="lblContainerType" runat="server"/></strong></span>
                </div>
            </div>

            <!-- Add/Edit PM mapping form -->
            <div class="card">
                <div class="card-title"><asp:Label ID="lblFormTitle" runat="server">Add Packing Material</asp:Label></div>
                <div class="form-group">
                    <label>Packing Material <span style="color:var(--accent)">*</span></label>
                    <asp:DropDownList ID="ddlPM" runat="server" onchange="onPMChange(this);"/>
                </div>
                <div class="form-row">
                    <div class="form-group">
                        <label>Quantity <span style="color:var(--accent)">*</span> <span id="spanPMUnit" style="font-weight:400;text-transform:none;color:var(--accent);font-size:12px;"></span></label>
                        <asp:TextBox ID="txtQtyPerUnit" runat="server" Text="1" style="text-align:right;"/>
                    </div>
                    <div class="form-group">
                        <label>Apply Level <span style="color:var(--accent)">*</span></label>
                        <asp:DropDownList ID="ddlApplyLevel" runat="server">
                            <asp:ListItem Text="Per Individual Piece" Value="UNIT"/>
                            <asp:ListItem Text="Per Container (Jar/Bottle)" Value="CONTAINER"/>
                            <asp:ListItem Text="Per Case" Value="CASE"/>
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="form-group" id="rowLanguage" runat="server" style="display:none;">
                    <label>Language <span style="color:var(--accent)">*</span></label>
                    <asp:DropDownList ID="ddlLanguage" runat="server">
                        <asp:ListItem Text="All Languages (universal)" Value=""/>
                        <asp:ListItem Text="Tamil" Value="Tamil"/>
                        <asp:ListItem Text="Kannada" Value="Kannada"/>
                        <asp:ListItem Text="Telugu" Value="Telugu"/>
                    </asp:DropDownList>
                    <span style="font-size:11px;color:var(--text-dim);margin-top:3px;">Select a language if this PM is language-specific (e.g. a label). Leave as "universal" for jars, lids, rolls etc.</span>
                </div>
                <div class="form-actions">
                    <asp:Button ID="btnAddPM" runat="server" Text="Add PM" CssClass="btn-primary" OnClick="btnAddPM_Click" CausesValidation="false"/>
                    <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn-secondary" OnClick="btnClear_Click" CausesValidation="false"/>
                </div>
            </div>

            <!-- Existing mappings table -->
            <div class="card">
                <div class="card-title">Assigned Packing Materials (<asp:Label ID="lblMappingCount" runat="server" Text="0"/>)</div>
                <asp:Panel ID="pnlMappingEmpty" runat="server">
                    <div class="empty-note">No packing materials assigned to this product yet</div>
                </asp:Panel>
                <asp:Panel ID="pnlMappingTable" runat="server" Visible="false">
                    <table class="mapping">
                        <thead>
                            <tr>
                                <th>Packing Material</th>
                                <th>Qty / Unit</th>
                                <th>Apply Level</th>
                                <th>Language</th>
                                <th>Current Stock</th>
                                <th style="width:100px;text-align:center;">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptMappings" runat="server" OnItemCommand="rptMappings_Cmd">
                                <ItemTemplate>
                                    <tr>
                                        <td>
                                            <strong><%# Eval("PMName") %></strong>
                                            <div style="font-size:11px;color:var(--text-dim);"><%# Eval("PMCode") %></div>
                                        </td>
                                        <td style="text-align:right;font-weight:600;"><%# Eval("QtyPerUnit", "{0:0.####}") %></td>
                                        <td>
                                            <span class='level-badge level-<%# Eval("ApplyLevel") %>'><%# Eval("ApplyLevel") %></span>
                                        </td>
                                        <td style="font-size:12px;">
                                            <%# Eval("Language") == DBNull.Value ? "<span style='color:var(--text-dim);'>All</span>" : "<strong>" + Eval("Language") + "</strong>" %>
                                        </td>
                                        <td style="text-align:right;font-size:12px;">
                                            <%# Eval("CurrentStock", "{0:N2}") %> <%# Eval("Abbreviation") %>
                                        </td>
                                        <td style="text-align:center;">
                                            <asp:LinkButton runat="server" CommandName="EditMapping" CommandArgument='<%# Eval("MappingID") %>'
                                                CausesValidation="false" style="font-size:11px;color:var(--accent);font-weight:700;text-decoration:none;margin-right:6px;">Edit</asp:LinkButton>
                                            <asp:LinkButton runat="server" CommandName="DeleteMapping" CommandArgument='<%# Eval("MappingID") %>'
                                                CausesValidation="false" style="font-size:11px;color:#c0392b;font-weight:700;text-decoration:none;"
                                                OnClientClick="return erpConfirmLink(this, 'Remove this packing material from product?', {title:'Remove PM', okText:'Yes, Remove', btnClass:'danger'});">Remove</asp:LinkButton>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </asp:Panel>
            </div>
        </asp:Panel>
    </div>
</div>
<script type="text/javascript">
function onPMChange(sel) {
    var span = document.getElementById('spanPMUnit');
    if (!span) return;
    var val = sel.value;
    if (!val || val === '0') { span.innerText = ''; return; }
    var parts = val.split('|');
    if (parts.length > 1 && parts[1]) {
        span.innerText = '(in ' + parts[1] + ')';
    } else {
        span.innerText = '';
    }
}
window.addEventListener('load', function() {
    var sel = document.getElementById('<%= ddlPM.ClientID %>');
    if (sel) onPMChange(sel);
});
</script>
</form><script src="/StockApp/erp-modal.js"></script>
<script src="/StockApp/erp-keepalive.js"></script>
</body></html>
