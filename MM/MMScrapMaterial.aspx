<%@ Page Language="C#" AutoEventWireup="true" Inherits="MMApp.MMScrapMaterial" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Scrap Materials &mdash; MM</title>
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet" />
    <style>
        :root { --bg:#f5f5f5; --surface:#fff; --border:#e0e0e0; --accent:#cc1e1e; --teal:#1a9e6a; --gold:#b8860b; --orange:#e07b00; --text:#1a1a1a; --text-muted:#666; --text-dim:#999; --radius:12px; }
        *, *::before, *::after { box-sizing:border-box; margin:0; padding:0; }
        body { background:var(--bg); color:var(--text); font-family:'DM Sans',sans-serif; min-height:100vh; }
        nav { background:#1a1a1a; display:flex; align-items:center; padding:0 28px; height:52px; gap:6px; position:sticky; top:0; z-index:100; }
        .nav-logo { background:#fff; border-radius:6px; padding:3px 8px; display:flex; align-items:center; }
        .nav-logo img { height:28px; object-fit:contain; }
        .nav-item { color:#aaa; text-decoration:none; font-size:12px; font-weight:600; letter-spacing:.06em; text-transform:uppercase; padding:6px 12px; border-radius:6px; transition:all .2s; }
        .nav-item:hover,.nav-item.active { color:#fff; background:rgba(255,255,255,.08); }
        .nav-right { margin-left:auto; display:flex; align-items:center; gap:12px; }
        .nav-user { font-size:12px; color:#888; }
        .nav-logout { font-size:11px; color:#666; text-decoration:none; padding:4px 10px; border:1px solid #333; border-radius:5px; transition:all .2s; }
        .nav-logout:hover { color:var(--accent); border-color:var(--accent); }
        .page-header { background:var(--surface); border-bottom:1px solid var(--border); padding:20px 40px; display:flex; align-items:center; justify-content:space-between; }
        .page-title { font-family:'Bebas Neue',sans-serif; font-size:28px; letter-spacing:.07em; }
        .page-title span { color:var(--orange); }
        .page-sub { font-size:12px; color:var(--text-muted); margin-top:2px; }
        .main-layout { max-width:1100px; margin:28px auto; padding:0 32px; display:grid; grid-template-columns:1fr 360px; gap:24px; align-items:start; }
        .card { background:var(--surface); border:1px solid var(--border); border-radius:var(--radius); padding:22px 26px; margin-bottom:20px; }
        .card:last-child { margin-bottom:0; }
        .card-title { font-family:'Bebas Neue',sans-serif; font-size:15px; letter-spacing:.08em; color:var(--text-muted); margin-bottom:16px; padding-bottom:10px; border-bottom:1px solid var(--border); display:flex; align-items:center; gap:8px; }
        .card-title::before { content:''; display:inline-block; width:3px; height:14px; background:var(--orange); border-radius:2px; }
        .form-grid { display:grid; grid-template-columns:1fr 1fr; gap:14px; }
        .form-group { display:flex; flex-direction:column; gap:5px; }
        .form-group.full { grid-column:1/-1; }
        label { font-size:11px; font-weight:700; letter-spacing:.07em; text-transform:uppercase; color:var(--text-muted); }
        label .req { color:var(--accent); margin-left:2px; }
        input[type=text],select,textarea { width:100%; padding:9px 12px; border:1.5px solid var(--border); border-radius:8px; font-family:'DM Sans',sans-serif; font-size:13px; color:var(--text); background:#fafafa; transition:border-color .2s; outline:none; }
        input:focus,select:focus,textarea:focus { border-color:var(--teal); background:#fff; }
        input[readonly] { background:#f0f0f0; color:var(--text-muted); cursor:not-allowed; }
        .btn-primary { background:var(--teal); color:#fff; border:none; border-radius:8px; padding:10px 24px; font-size:13px; font-weight:700; cursor:pointer; letter-spacing:.04em; }
        .btn-primary:hover { background:#148a5a; }
        .btn-secondary { background:#f0f0f0; color:#333; border:1px solid var(--border); border-radius:8px; padding:9px 18px; font-size:12px; font-weight:600; cursor:pointer; }
        .btn-secondary:hover { background:#e0e0e0; }
        .btn-danger { background:transparent; color:var(--accent); border:1px solid var(--accent); border-radius:8px; padding:5px 12px; font-size:11px; font-weight:700; cursor:pointer; }
        .btn-danger:hover { background:var(--accent); color:#fff; }
        .form-actions { display:flex; gap:10px; margin-top:18px; }
        .alert { padding:11px 16px; border-radius:8px; font-size:13px; font-weight:600; margin-bottom:16px; }
        .alert-success { background:#eafaf1; color:var(--teal); border:1px solid #a9dfbf; }
        .alert-danger { background:#fdf3f2; color:var(--accent); border:1px solid #f5c6cb; }
        .material-list { display:flex; flex-direction:column; gap:8px; }
        .material-row { display:flex; align-items:center; justify-content:space-between; padding:10px 14px; background:#fafafa; border:1px solid var(--border); border-radius:9px; transition:border-color .2s; }
        .material-row:hover { border-color:var(--orange); }
        .material-row.inactive { opacity:.5; }
        .mat-info { display:flex; flex-direction:column; gap:2px; }
        .mat-name { font-weight:700; font-size:13px; }
        .mat-code { font-size:11px; color:var(--text-dim); }
        .mat-uom { font-size:11px; color:var(--text-muted); background:#f0f0f0; padding:2px 8px; border-radius:10px; }
        .mat-actions { display:flex; gap:8px; align-items:center; }
        .selected-row { border-color:var(--orange) !important; background:#fff8f0 !important; }
        .badge-active { background:#eafaf1; color:var(--teal); font-size:10px; font-weight:700; padding:2px 8px; border-radius:10px; }
        .badge-inactive { background:#f0f0f0; color:var(--text-dim); font-size:10px; font-weight:700; padding:2px 8px; border-radius:10px; }
        .empty-note { text-align:center; padding:28px; color:var(--text-dim); font-size:13px; }
    </style>
</head>
<body>
<form id="form1" runat="server">
<asp:HiddenField ID="hfScrapID" runat="server" Value="0"/>

<nav>
    <a href="MMHome.aspx" class="nav-logo">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    </a>
    <a href="MMHome.aspx" class="nav-item">Home</a>
    <a href="MMScrapMaterial.aspx" class="nav-item active">Scrap Materials</a>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblNavUser" runat="server"/></span>
        <a href="MMLogout.aspx" class="nav-logout">Logout</a>
    </div>
</nav>

<div class="page-header">
    <div>
        <div class="page-title">Scrap <span>Materials</span></div>
        <div class="page-sub">Define scrap materials produced as by-products during raw material processing</div>
    </div>
</div>

<div class="main-layout">

    <!-- LEFT: FORM -->
    <div>
        <asp:Panel ID="pnlAlert" runat="server" Visible="false">
            <div class="alert"><asp:Label ID="lblAlert" runat="server"/></div>
        </asp:Panel>

        <div class="card">
            <div class="card-title">Scrap Material Details</div>

            <div class="form-grid">
                <div class="form-group">
                    <label>Scrap Code</label>
                    <asp:TextBox ID="txtCode" runat="server" placeholder="Auto-generated" ReadOnly="true"/>
                </div>
                <div class="form-group">
                    <label>UOM <span class="req">*</span></label>
                    <asp:DropDownList ID="ddlUOM" runat="server"/>
                </div>
                <div class="form-group full">
                    <label>Scrap Name <span class="req">*</span></label>
                    <asp:TextBox ID="txtName" runat="server" MaxLength="150" placeholder="e.g. Coconut Shell"/>
                </div>
                <div class="form-group full">
                    <label>Description</label>
                    <asp:TextBox ID="txtDesc" runat="server" TextMode="MultiLine" Rows="2" placeholder="Optional description"/>
                </div>
            </div>

            <div class="form-actions">
                <asp:Button ID="btnSave" runat="server" Text="Save Scrap Material" CssClass="btn-primary"
                    OnClick="btnSave_Click" CausesValidation="false"/>
                <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn-secondary"
                    OnClick="btnClear_Click" CausesValidation="false"/>
                <asp:Button ID="btnToggle" runat="server" Text="Deactivate" CssClass="btn-danger"
                    OnClick="btnToggle_Click" CausesValidation="false" Visible="false"/>
            </div>
        </div>
    </div>

    <!-- RIGHT: LIST -->
    <div>
        <div class="card">
            <div class="card-title">Scrap Materials <asp:Label ID="lblCount" runat="server" style="font-size:11px;color:var(--text-dim);font-weight:400;font-family:'DM Sans',sans-serif;"/></div>
            <asp:Panel ID="pnlEmpty" runat="server">
                <div class="empty-note">No scrap materials defined yet</div>
            </asp:Panel>
            <div class="material-list">
                <asp:Repeater ID="rptList" runat="server" OnItemCommand="rptList_ItemCommand">
                    <ItemTemplate>
                        <div class='material-row <%# !(bool)Eval("IsActive") ? "inactive" : "" %>'>
                            <div class="mat-info">
                                <span class="mat-name"><%# Eval("ScrapName") %></span>
                                <span class="mat-code"><%# Eval("ScrapCode") %></span>
                            </div>
                            <div class="mat-actions">
                                <span class="mat-uom"><%# Eval("Abbreviation") %></span>
                                <span class='<%# (bool)Eval("IsActive") ? "badge-active" : "badge-inactive" %>'>
                                    <%# (bool)Eval("IsActive") ? "Active" : "Inactive" %>
                                </span>
                                <asp:LinkButton runat="server" CommandName="Edit"
                                    CommandArgument='<%# Eval("ScrapID") %>'
                                    CssClass="btn-secondary" style="font-size:11px;padding:4px 10px;"
                                    CausesValidation="false">Edit</asp:LinkButton>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>

</div>
</form>
</body>
</html>
