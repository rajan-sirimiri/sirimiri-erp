<%@ Page Language="C#" AutoEventWireup="true" Inherits="PKApp.PKCustomer" EnableEventValidation="false" %>
<!DOCTYPE html><html lang="en"><head runat="server">
<meta charset="utf-8"/><meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Customer Master &mdash; PK</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<style>
:root{--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--accent:#e67e22;--accent-dark:#d35400;--teal:#1a9e6a;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--radius:12px;}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);min-height:100vh;color:var(--text);}
nav{background:#1a1a1a;height:52px;display:flex;align-items:center;padding:0 24px;gap:12px;position:sticky;top:0;z-index:100;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;}.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:17px;letter-spacing:.08em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:12px;}
.nav-user{color:rgba(255,255,255,.8);font-size:12px;}
.nav-link{color:#aaa;text-decoration:none;font-size:11px;font-weight:600;text-transform:uppercase;padding:5px 10px;border-radius:5px;}
.nav-link:hover{color:#fff;background:rgba(255,255,255,.08);}
.page-header{background:var(--surface);border-bottom:1px solid var(--border);padding:18px 32px;display:flex;justify-content:space-between;align-items:center;}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:26px;letter-spacing:.07em;}.page-title span{color:var(--accent);}
.page-sub{font-size:12px;color:var(--text-muted);margin-top:2px;}
.main{display:grid;grid-template-columns:1fr 1fr;gap:24px;max-width:1300px;margin:24px auto;padding:0 28px;}
@media(max-width:900px){.main{grid-template-columns:1fr;}}
.card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:20px 24px;margin-bottom:20px;}
.card-title{font-family:'Bebas Neue',sans-serif;font-size:14px;letter-spacing:.08em;color:var(--text-muted);margin-bottom:14px;padding-bottom:10px;border-bottom:1px solid var(--border);}
.form-grid{display:grid;grid-template-columns:1fr 1fr;gap:12px;}
.form-group{display:flex;flex-direction:column;gap:5px;}.form-group.full{grid-column:1/-1;}
label{font-size:11px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;color:var(--text-muted);}
.req{color:var(--accent);}
select,input[type=text],textarea{width:100%;padding:9px 12px;border:1.5px solid var(--border);border-radius:8px;font-family:inherit;font-size:13px;background:#fafafa;outline:none;}
select:focus,input:focus,textarea:focus{border-color:var(--accent);background:#fff;}
.field-hint{font-size:10px;color:var(--text-dim);}
.btn-row{display:flex;gap:8px;margin-top:14px;}
.btn{border:none;border-radius:8px;padding:10px 22px;font-size:12px;font-weight:700;cursor:pointer;}
.btn-primary{background:var(--accent);color:#fff;}.btn-primary:hover{background:var(--accent-dark);}
.btn-secondary{background:#f0f0f0;color:#333;border:1px solid var(--border);}
.btn-danger{background:#fdf3f2;color:#e74c3c;border:1px solid #f5c6cb;}
.alert{padding:10px 14px;border-radius:8px;font-size:13px;font-weight:600;margin-bottom:14px;}
.alert-success{background:#eafaf1;color:var(--teal);border:1px solid #a9dfbf;}
.alert-danger{background:#fdf3f2;color:#e74c3c;border:1px solid #f5c6cb;}
.cust-table{width:100%;border-collapse:collapse;font-size:12px;}
.cust-table th{font-size:9px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--text-dim);padding:8px 10px;border-bottom:2px solid var(--border);text-align:left;background:#fafafa;}
.cust-table td{padding:9px 10px;border-bottom:1px solid var(--border);}
.cust-table tr:last-child td{border-bottom:none;}.cust-table tr:hover td{background:#f9f9f9;}
.badge-active{background:#eafaf1;color:var(--teal);font-size:9px;font-weight:700;padding:2px 6px;border-radius:8px;}
.badge-inactive{background:#f5f5f5;color:#999;font-size:9px;font-weight:700;padding:2px 6px;border-radius:8px;}
.badge-type{font-size:9px;font-weight:700;padding:2px 6px;border-radius:8px;background:#e8f4fd;color:#2980b9;}
.act-link{color:var(--accent);font-size:11px;font-weight:600;text-decoration:none;}.act-link:hover{text-decoration:underline;}
.search-box{width:100%;padding:8px 12px;border:1.5px solid var(--border);border-radius:8px;font-size:12px;margin-bottom:12px;outline:none;}.search-box:focus{border-color:var(--accent);}
.empty-note{text-align:center;padding:28px;color:var(--text-dim);font-size:13px;}
.gst-status{font-size:11px;font-weight:600;margin-top:4px;}.gst-valid{color:var(--teal);}.gst-invalid{color:#e74c3c;}
.upload-section{background:#f9f9f9;border:1px dashed var(--border);border-radius:10px;padding:16px;margin-top:4px;}
.upload-hint{font-size:11px;color:var(--text-dim);margin-top:6px;}
</style></head><body>
<form id="form1" runat="server">
<asp:HiddenField ID="hfCustID" runat="server" Value="0"/>
<nav>
    <a class="nav-logo" href="PKHome.aspx"><img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="" onerror="this.style.display='none'"/></a>
    <span class="nav-title">Packing &amp; Shipments</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblUser" runat="server"/></span>
        <a href="/StockApp/ERPHome.aspx" class="nav-link">ERP Home</a>
        <a href="PKHome.aspx" class="nav-link">PK Home</a>
        <a href="PKLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>
<div class="page-header">
    <div><div class="page-title">Customer <span>Master</span></div><div class="page-sub">Manage stockists, distributors and retail customers</div></div>
    <div style="font-size:12px;color:var(--text-muted);">Total: <strong><asp:Label ID="lblCount" runat="server">0</asp:Label></strong></div>
</div>
<div class="main">
    <!-- LEFT: FORM -->
    <div>
        <asp:Panel ID="pnlAlert" runat="server" Visible="false"><div class="alert"><asp:Label ID="lblAlert" runat="server"/></div></asp:Panel>
        <div class="card">
            <div class="card-title"><asp:Label ID="lblFormTitle" runat="server">New Customer</asp:Label></div>
            <div class="form-grid">
                <div class="form-group"><label>Customer Type <span class="req">*</span></label>
                    <asp:DropDownList ID="ddlCustomerType" runat="server" onchange="onTypeChange(this);"/></div>
                <div class="form-group"><label>Customer Code</label>
                    <asp:TextBox ID="txtCode" runat="server" ReadOnly="true" placeholder="Auto-generated"/>
                    <span class="field-hint">Auto-generated based on type</span></div>
                <div class="form-group full"><label><span id="lblNameLabel">Name</span> <span class="req">*</span></label>
                    <asp:TextBox ID="txtName" runat="server" MaxLength="200" placeholder="Enter name"/></div>
                <div class="form-group"><label>Contact Person</label>
                    <asp:TextBox ID="txtContact" runat="server" MaxLength="100" placeholder="Contact person"/></div>
                <div class="form-group"><label>Phone</label>
                    <asp:TextBox ID="txtPhone" runat="server" MaxLength="20" placeholder="e.g. 9876543210"/></div>
                <div class="form-group full"><label>Email</label>
                    <asp:TextBox ID="txtEmail" runat="server" MaxLength="200" placeholder="email@example.com"/></div>
                <div class="form-group full"><label>Address</label>
                    <asp:TextBox ID="txtAddress" runat="server" TextMode="MultiLine" Rows="2" MaxLength="500" placeholder="Full address"/></div>
                <div class="form-group"><label>City</label><asp:TextBox ID="txtCity" runat="server" MaxLength="100" placeholder="City"/></div>
                <div class="form-group"><label>State</label><asp:TextBox ID="txtState" runat="server" MaxLength="100" placeholder="State"/></div>
                <div class="form-group"><label>PIN Code</label><asp:TextBox ID="txtPinCode" runat="server" MaxLength="10" placeholder="e.g. 560001"/></div>
                <div class="form-group"><label>GSTIN</label>
                    <asp:TextBox ID="txtGSTIN" runat="server" MaxLength="15" placeholder="15-digit GSTIN" onblur="validateGST(this.value);"/>
                    <div class="gst-status" id="gstStatus"></div>
                    <span class="field-hint">15-character GST Identification Number</span></div>
            </div>
            <div class="btn-row">
                <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" CausesValidation="false"/>
                <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="btnClear_Click" CausesValidation="false"/>
                <asp:Button ID="btnToggle" runat="server" Text="Deactivate" CssClass="btn btn-danger" OnClick="btnToggle_Click" CausesValidation="false" Visible="false"/>
            </div>
        </div>
        <!-- EXCEL UPLOAD -->
        <div class="card">
            <div class="card-title">&#x1F4E5; Bulk Import from Excel</div>
            <div class="upload-section">
                <asp:FileUpload ID="fuExcel" runat="server"/>
                <asp:Button ID="btnUpload" runat="server" Text="&#x2B06; Import Customers" CssClass="btn btn-primary" OnClick="btnUpload_Click" CausesValidation="false" style="margin-top:10px;"/>
                <asp:LinkButton ID="lnkTemplate" runat="server" OnClick="lnkTemplate_Click" CausesValidation="false"
                    style="display:inline-block;margin-top:10px;margin-left:10px;font-size:12px;color:var(--accent);font-weight:600;text-decoration:underline;cursor:pointer;">
                    &#x1F4E5; Download Template
                </asp:LinkButton>
                <div class="upload-hint">
                    Upload .xlsx file with columns: <strong>CustomerType</strong> (Stockist/Distributor/Retail or ST/DI/RT),
                    <strong>Name</strong>, ContactPerson, Phone, Email, Address, City, State, PinCode, GSTIN
                </div>
            </div>
        </div>
    </div>
    <!-- RIGHT: LIST -->
    <div>
        <div class="card">
            <div class="card-title">&#x1F4CB; Customer List</div>
            <input type="text" class="search-box" placeholder="Search customers..." onkeyup="filterTable(this.value);"/>
            <div style="overflow-x:auto;max-height:600px;overflow-y:auto;">
                <asp:Panel ID="pnlEmpty" runat="server" Visible="false"><div class="empty-note">No customers yet</div></asp:Panel>
                <asp:Repeater ID="rptList" runat="server" OnItemCommand="rptList_Cmd">
                    <HeaderTemplate><table class="cust-table" id="custTable"><thead><tr>
                        <th>Code</th><th>Type</th><th>Name</th><th>City</th><th>Status</th><th></th>
                    </tr></thead><tbody></HeaderTemplate>
                    <ItemTemplate><tr>
                        <td style="font-size:11px;font-weight:600;color:var(--text-muted);"><%# Eval("CustomerCode") %></td>
                        <td><%# Eval("TypeName").ToString() != "" ? "<span class='badge-type'>" + Eval("TypeName") + "</span>" : "" %></td>
                        <td><div style="font-weight:600;"><%# Eval("CustomerName") %></div>
                            <div style="font-size:10px;color:var(--text-dim);"><%# Eval("ContactPerson") == DBNull.Value ? "" : Eval("ContactPerson") %></div></td>
                        <td style="font-size:12px;"><%# Eval("City") == DBNull.Value ? "" : Eval("City") %></td>
                        <td><span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge-active" : "badge-inactive" %>'><%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %></span></td>
                        <td><asp:LinkButton runat="server" CommandName="Edit" CommandArgument='<%# Eval("CustomerID") %>' CssClass="act-link" CausesValidation="false">Edit</asp:LinkButton></td>
                    </tr></ItemTemplate>
                    <FooterTemplate></tbody></table></FooterTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>
</div>
</form>
<script>
function onTypeChange(sel){
    var lbl=document.getElementById('lblNameLabel');if(!lbl)return;
    if(sel.value==='ST')lbl.innerText='Stockist Name';
    else if(sel.value==='DI')lbl.innerText='Distributor Name';
    else if(sel.value==='RT')lbl.innerText='Customer Name';
    else lbl.innerText='Name';
}
function validateGST(g){
    var el=document.getElementById('gstStatus');if(!el)return;
    if(!g||g.length===0){el.innerHTML='';return;}
    g=g.toUpperCase().trim();
    if(g.length!==15){el.innerHTML='<span class="gst-invalid">&#10008; Must be 15 characters</span>';return;}
    var pat=/^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$/;
    if(!pat.test(g)){el.innerHTML='<span class="gst-invalid">&#10008; Invalid GSTIN format</span>';return;}
    var chars='0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ',sum=0;
    for(var i=0;i<14;i++){var idx=chars.indexOf(g[i]);var f=(i%2===0)?1:2;var p=idx*f;sum+=Math.floor(p/36)+(p%36);}
    var cd=(36-(sum%36))%36;
    if(g[14]!==chars[cd]){el.innerHTML='<span class="gst-invalid">&#10008; Checksum invalid</span>';return;}
    el.innerHTML='<span class="gst-valid">&#10004; Valid GSTIN format</span>';
}
function filterTable(q){q=q.toLowerCase();document.querySelectorAll('#custTable tbody tr').forEach(function(r){r.style.display=r.innerText.toLowerCase().indexOf(q)>=0?'':'none';});}
window.addEventListener('load',function(){var sel=document.getElementById('<%= ddlCustomerType.ClientID %>');if(sel)onTypeChange(sel);});
</script><script src="/StockApp/erp-keepalive.js"></script>
</body></html>
