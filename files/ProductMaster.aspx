<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ProductMaster.aspx.cs" Inherits="StockApp.ProductMaster" ResponseEncoding="UTF-8" ContentType="text/html; charset=utf-8" %>
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <title>Sirimiri - Product Master</title>
    <link rel="preconnect" href="https://fonts.googleapis.com"/>
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin/>
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
    <style>
        :root{--accent:#C0392B;--accent-dark:#a93226;--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--text:#1a1a1a;--muted:#666;--radius:14px}
        *{box-sizing:border-box;margin:0;padding:0}
        body{background:var(--bg);font-family:'DM Sans',sans-serif;min-height:100vh}
        nav{background:var(--accent);display:flex;align-items:center;padding:0 24px;height:52px;gap:12px}
        .nav-group{position:relative}
        .nav-item{color:#fff;font-size:13px;font-weight:600;padding:8px 14px;border-radius:6px;cursor:pointer;display:flex;align-items:center;gap:6px;text-decoration:none}
        .nav-item:hover{background:rgba(255,255,255,.15)}
        .nav-dropdown{display:none;position:absolute;top:100%;left:0;background:#fff;border-radius:8px;min-width:220px;box-shadow:0 4px 20px rgba(0,0,0,.15);z-index:999;overflow:hidden}
        .nav-group:hover .nav-dropdown{display:block}
        .nav-dropdown a{display:block;padding:10px 16px;font-size:13px;color:var(--text);text-decoration:none}
        .nav-dropdown a:hover{background:var(--bg);color:var(--accent)}
        .nav-right{margin-left:auto;display:flex;align-items:center;gap:20px;font-size:13px}
        .nav-right a{color:#fff;font-weight:700;text-decoration:none;opacity:.9}
        .btn-signout{border:1.5px solid rgba(255,255,255,.6);padding:5px 14px;border-radius:6px}
        .user-label{color:#fff;opacity:.9;font-weight:500}
        .logo-area{background:#fff;display:flex;align-items:center;justify-content:space-between;padding:16px 24px 0}
        .logo-area img{height:72px;object-fit:contain;filter:drop-shadow(0 2px 8px rgba(204,30,30,.20))}
        .bis-label{font-family:'Bebas Neue',cursive;font-size:22px;letter-spacing:.12em;color:var(--text);text-align:center;line-height:1.25}
        .accent-bar{height:4px;background:linear-gradient(90deg,var(--accent-dark),#e63030,var(--accent-dark))}
        .page-wrap{max-width:1000px;margin:32px auto;padding:0 20px 60px}
        .page-title{font-family:'Bebas Neue',cursive;font-size:32px;letter-spacing:.08em;color:var(--text);margin-bottom:4px}
        .page-sub{font-size:13px;color:var(--muted);margin-bottom:28px}
        .card{background:var(--surface);border-radius:var(--radius);box-shadow:0 2px 16px rgba(0,0,0,.08);margin-bottom:24px;overflow:hidden}
        .card-head{background:var(--accent);padding:14px 24px}
        .card-head h2{font-family:'Bebas Neue',cursive;font-size:20px;letter-spacing:.08em;color:#fff}
        .card-body{padding:24px}
        .form-grid{display:grid;grid-template-columns:2fr 1fr 1fr 1fr 1fr;gap:12px;align-items:end}
        .field label{display:block;font-size:11px;font-weight:700;letter-spacing:.1em;text-transform:uppercase;color:var(--muted);margin-bottom:6px}
        .field input,.field select{width:100%;padding:10px 14px;border:1.5px solid var(--border);border-radius:8px;font-size:14px;font-family:inherit;outline:none;background:#fff}
        .field input:focus,.field select:focus{border-color:var(--accent)}
        .prefix-wrap{position:relative}
        .prefix-wrap .pre{position:absolute;left:12px;top:50%;transform:translateY(-50%);color:var(--muted);font-size:13px;pointer-events:none}
        .prefix-wrap input{padding-left:34px}
        .btn-add{margin-top:14px;padding:11px 32px;background:var(--accent);color:#fff;border:none;border-radius:8px;font-size:13px;font-weight:700;font-family:inherit;cursor:pointer}
        .btn-add:hover{background:var(--accent-dark)}
        .msg-ok{background:#f0faf4;border:1.5px solid #27ae60;color:#27ae60;padding:10px 14px;border-radius:8px;font-size:13px;margin-bottom:16px}
        .msg-err{background:#fdf0f0;border:1.5px solid var(--accent);color:var(--accent);padding:10px 14px;border-radius:8px;font-size:13px;margin-bottom:16px}
        table{width:100%;border-collapse:collapse;font-size:13px}
        th{text-align:left;padding:10px 14px;background:var(--bg);font-size:11px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:var(--muted);white-space:nowrap}
        td{padding:11px 14px;border-bottom:1px solid var(--border);vertical-align:middle}
        tr:last-child td{border-bottom:none}
        tr:hover td{background:#fafafa}
        .num{text-align:right}
        .badge-active{background:#e8fdf0;color:#27ae60;padding:3px 10px;border-radius:20px;font-size:11px;font-weight:700}
        .badge-inactive{background:#f5f5f5;color:#999;padding:3px 10px;border-radius:20px;font-size:11px;font-weight:700}
        .gst-badge{background:#e8f4fd;color:#2980b9;padding:2px 8px;border-radius:12px;font-size:11px;font-weight:700}
        .btn-toggle{padding:5px 14px;border-radius:6px;font-size:12px;font-weight:600;font-family:inherit;cursor:pointer;border:1.5px solid;background:#fff;text-decoration:none;display:inline-block}
        .btn-deactivate{border-color:#e0e0e0;color:#999}
        .btn-deactivate:hover{border-color:var(--accent);color:var(--accent)}
        .btn-activate{border-color:#27ae60;color:#27ae60}
        .btn-activate:hover{background:#27ae60;color:#fff}
        .empty-state{text-align:center;padding:40px;color:var(--muted);font-size:14px}
        @media(max-width:768px){.form-grid{grid-template-columns:1fr 1fr}}
        .upload-hint  { color:#555; font-size:13px; margin-bottom:14px; }
        .bulk-actions { margin-bottom:12px; }
        .btn-template { display:inline-block; padding:9px 18px; background:#1a7a4a; color:#fff;
                        border-radius:6px; text-decoration:none; font-size:13px; font-weight:600; cursor:pointer; border:none; }
        .btn-template:hover { opacity:.88; color:#fff; }
        .upload-row   { display:flex; gap:12px; align-items:center; flex-wrap:wrap; margin-bottom:14px; }
        .file-input   { flex:1; min-width:200px; font-size:13px; }
        .bulk-result  { display:flex; gap:20px; flex-wrap:wrap; background:#f4fdf8;
                        border:1px solid #b3dfc9; border-radius:6px; padding:12px 16px; margin-bottom:10px; }
        .bulk-stat    { font-size:13px; color:#333; }
        .bulk-detail  { font-size:12px; color:#c0392b; white-space:pre-wrap; display:block; margin-top:6px; }
        .btn-edit     { background:#2979c9; color:#fff; border:none; padding:5px 12px; border-radius:5px;
                        font-size:12px; cursor:pointer; text-decoration:none; }
        .btn-edit:hover { opacity:.85; color:#fff; }
        .btn-cancel   { padding:10px 22px; background:#fff; color:var(--text); border:1.5px solid var(--border);
                        border-radius:6px; font-size:14px; font-weight:600; cursor:pointer; }
        .btn-cancel:hover { background:var(--bg); }
        .btn-row      { display:flex; gap:12px; }
        .edit-card    { border: 2px solid #2979c9; }
        .edit-grid    { display:grid; grid-template-columns:1fr 1fr 1fr; gap:16px; }
        @media(max-width:640px){ .edit-grid{ grid-template-columns:1fr; } }
    </style>
</head>
<body>
<form id="form1" runat="server">
    <nav>
        <div class="nav-group">
            <span class="nav-item">Home &#9660;</span>
            <div class="nav-dropdown">
                <a href="StockEntry.aspx">Distributor Stock Position Entry</a>
                <a href="DailySales.aspx">Daily Sales Entry</a>
            </div>
        </div>
        <div class="nav-group">
            <span class="nav-item">Admin &#9660;</span>
            <div class="nav-dropdown">
                <a href="UserAdmin.aspx">User Management</a>
                <a href="ProductMaster.aspx">Product Master</a>
            </div>
        </div>
        <div class="nav-right">
            <asp:Label ID="lblUserInfo" runat="server" CssClass="user-label"/>
            <a href="Logout.aspx" class="btn-signout" onclick="return confirm('Sign out?')">Sign Out</a>
        </div>
    </nav>

    <div class="logo-area">
        <img src="Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri"/>
        <div class="bis-label">SIRIMIRI Nutrition Food Products<br/>
            <span style="font-size:14px;color:var(--muted);">Business Intelligence System</span>
        </div>
        <div style="width:80px;"></div>
    </div>
    <div class="accent-bar"></div>

    <div class="page-wrap">
        <div class="page-title">Product Master</div>
        <p class="page-sub">Manage the product list used in Daily Sales Entry. Only active products appear in the entry form.</p>

        <!-- ADD PRODUCT -->
        <div class="card">
            <div class="card-head"><h2>Add New Product</h2></div>
            <div class="card-body">
                <asp:Panel ID="pnlMsg" runat="server" Visible="false">
                    <div class="msg-ok"><asp:Label ID="lblMsg" runat="server"/></div>
                </asp:Panel>
                <asp:Panel ID="pnlErr" runat="server" Visible="false">
                    <div class="msg-err"><asp:Label ID="lblErr" runat="server"/></div>
                </asp:Panel>

                <div class="form-grid">
                    <div class="field">
                        <label>Product Name *</label>
                        <asp:TextBox ID="txtProductName" runat="server" placeholder="e.g. Sirimiri Classic Mix"/>
                    </div>
                    <div class="field">
                        <label>Product Code</label>
                        <asp:TextBox ID="txtProductCode" runat="server" placeholder="e.g. SMC-001"/>
                    </div>
                    <div class="field">
                        <label>MRP (Rs.) *</label>
                        <div class="prefix-wrap">
                            <span class="pre">Rs.</span>
                            <asp:TextBox ID="txtMRP" runat="server" placeholder="0.00"/>
                        </div>
                    </div>
                    <div class="field">
                        <label>HSN Code</label>
                        <asp:TextBox ID="txtHSN" runat="server" placeholder="e.g. 2106"/>
                    </div>
                    <div class="field">
                        <label>GST Rate *</label>
                        <asp:DropDownList ID="ddlGST" runat="server">
                            <asp:ListItem Value="0">0%  - Exempt</asp:ListItem>
                            <asp:ListItem Value="5">5%</asp:ListItem>
                            <asp:ListItem Value="12">12%</asp:ListItem>
                            <asp:ListItem Value="18" Selected="True">18%</asp:ListItem>
                            <asp:ListItem Value="28">28%</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <asp:Button ID="btnAdd" runat="server" Text="Add Product"
                    CssClass="btn-add" OnClick="btnAdd_Click"/>
            </div>
        </div>

        <!-- BULK UPLOAD -->
        <div class="card">
            <div class="card-head"><h2>Bulk Upload Products</h2></div>
            <div class="card-body">
                <p class="upload-hint">Download the template, fill in product details, and upload.</p>
                <div class="bulk-actions">
                    <asp:LinkButton ID="btnDownloadTemplate" runat="server"
                        CssClass="btn-template" OnClick="btnDownloadTemplate_Click">
                        &#8595; Download Excel Template
                    </asp:LinkButton>
                </div>
                <div class="upload-row">
                    <asp:FileUpload ID="fileProducts" runat="server" CssClass="file-input" Accept=".xlsx" />
                    <asp:Button ID="btnBulkUpload" runat="server" Text="Upload &amp; Add Products"
                        CssClass="btn-add" OnClick="btnBulkUpload_Click" />
                </div>
                <asp:Panel ID="pnlBulkResult" runat="server" Visible="false">
                    <div class="bulk-result">
                        <span class="bulk-stat">&#10003; Added: <strong><asp:Label ID="lblAdded" runat="server" /></strong></span>
                        <span class="bulk-stat">&#8212; Skipped (duplicate): <strong><asp:Label ID="lblBulkSkipped" runat="server" /></strong></span>
                        <span class="bulk-stat">&#9888; Errors: <strong><asp:Label ID="lblBulkErrors" runat="server" /></strong></span>
                    </div>
                    <asp:Label ID="lblBulkDetail" runat="server" CssClass="bulk-detail" />
                </asp:Panel>
            </div>
        </div>

        <!-- EDIT PRODUCT PANEL -->
        <asp:Panel ID="pnlEditProduct" runat="server" Visible="false">
        <div class="card edit-card">
            <div class="card-head"><h2>Edit Product</h2></div>
            <div class="card-body">
                <asp:HiddenField ID="hfEditProductId" runat="server" />
                <div class="edit-grid">
                    <div class="field">
                        <label>Product Name <span class="req">*</span></label>
                        <asp:TextBox ID="txtEditProductName" runat="server" />
                    </div>
                    <div class="field">
                        <label>Product Code</label>
                        <asp:TextBox ID="txtEditProductCode" runat="server" />
                    </div>
                    <div class="field">
                        <label>MRP (Rs.)</label>
                        <asp:TextBox ID="txtEditMRP" runat="server" />
                    </div>
                    <div class="field">
                        <label>HSN Code</label>
                        <asp:TextBox ID="txtEditHSN" runat="server" />
                    </div>
                    <div class="field">
                        <label>GST Rate (%)</label>
                        <asp:DropDownList ID="ddlEditGST" runat="server">
                            <asp:ListItem Text="0%"  Value="0" />
                            <asp:ListItem Text="5%"  Value="5" />
                            <asp:ListItem Text="12%" Value="12" />
                            <asp:ListItem Text="18%" Value="18" Selected="True" />
                            <asp:ListItem Text="28%" Value="28" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="btn-row" style="margin-top:16px;">
                    <asp:Button ID="btnEditSave" runat="server" Text="Save Changes"
                        CssClass="btn-add" OnClick="btnEditSave_Click" />
                    <asp:Button ID="btnEditCancel" runat="server" Text="Cancel"
                        CssClass="btn-cancel" OnClick="btnEditCancel_Click" CausesValidation="false" />
                </div>
            </div>
        </div>
        </asp:Panel>

        <!-- PRODUCT LIST -->
        <div class="card">
            <div class="card-head"><h2>All Products</h2></div>
            <div class="card-body" style="padding:0;">
                <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
                    <div class="empty-state">No products yet. Add your first product above.</div>
                </asp:Panel>
                <asp:GridView ID="gvProducts" runat="server" AutoGenerateColumns="false"
                    OnRowCommand="gvProducts_RowCommand" GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="ProductCode" HeaderText="Code"         NullDisplayText="-"/>
                        <asp:BoundField DataField="ProductName" HeaderText="Product Name"/>
                        <asp:TemplateField HeaderText="MRP" ItemStyle-CssClass="num">
                            <ItemTemplate>Rs. <%# string.Format("{0:N2}", Eval("MRP")) %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="HSNCode"     HeaderText="HSN Code"     NullDisplayText="-"/>
                        <asp:TemplateField HeaderText="GST">
                            <ItemTemplate>
                                <span class="gst-badge"><%# Eval("GSTRate") %>%</span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge-active" : "badge-inactive" %>'>
                                    <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="CreatedAt" HeaderText="Added On" DataFormatString="{0:dd MMM yyyy}"/>
                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:LinkButton ID="btnEdit" runat="server"
                                    CommandName="EditProduct"
                                    CommandArgument='<%# Eval("ProductID") %>'
                                    CssClass="btn-toggle btn-edit">Edit</asp:LinkButton>
                                &nbsp;
                                <asp:LinkButton ID="btnToggle" runat="server"
                                    CommandName="ToggleActive"
                                    CommandArgument='<%# Eval("ProductID") + "|" + Eval("IsActive") %>'
                                    CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "btn-toggle btn-deactivate" : "btn-toggle btn-activate" %>'
                                    Text='<%# Convert.ToBoolean(Eval("IsActive")) ? "Deactivate" : "Activate" %>'
                                    OnClientClick="return confirm('Are you sure?');"/>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>

    </div>
</form>
</body>
</html>
