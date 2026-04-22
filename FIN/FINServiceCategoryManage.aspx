<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FINServiceCategoryManage.aspx.cs" Inherits="FINApp.FINServiceCategoryManage" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Manage Service Categories &mdash; FIN</title>
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
    <style>
        :root {
            --bg:#f0f0f0; --surface:#fff; --border:#e0e0e0;
            --accent:#8e44ad; --accent-dark:#6c3483; --accent-light:#f4ecf7;
            --text:#1a1a1a; --text-muted:#666; --text-dim:#999;
            --success:#1a9e6a; --danger:#c0392b; --radius:12px;
        }
        *, *::before, *::after { box-sizing:border-box; margin:0; padding:0; }
        body { background:var(--bg); color:var(--text); font-family:'DM Sans',sans-serif; min-height:100vh; }

        nav { background:#1a1a1a; display:flex; align-items:center; padding:0 28px; height:52px; gap:6px; position:sticky; top:0; z-index:100; }
        .nav-item { color:#aaa; text-decoration:none; font-size:12px; font-weight:600; letter-spacing:.06em; text-transform:uppercase; padding:6px 12px; border-radius:6px; transition:all .2s; }
        .nav-item:hover, .nav-item.active { color:#fff; background:rgba(255,255,255,0.08); }
        .nav-sep { color:#444; margin:0 4px; }
        .nav-right { margin-left:auto; display:flex; align-items:center; gap:12px; }
        .nav-user { font-size:12px; color:#888; }
        .nav-logout { font-size:11px; color:#666; text-decoration:none; padding:4px 10px; border:1px solid #333; border-radius:5px; }
        .nav-logout:hover { color:var(--accent); border-color:var(--accent); }

        .page-header { background:var(--surface); border-bottom:1px solid var(--border); padding:24px 40px; }
        .page-title { font-family:'Bebas Neue',sans-serif; font-size:28px; letter-spacing:.07em; color:var(--text); }
        .page-title span { color:var(--accent); }
        .page-sub { font-size:12px; color:var(--text-muted); margin-top:2px; }

        .content { max-width:900px; margin:32px auto; padding:0 32px; }
        .back-link { display:inline-block; font-size:12px; color:var(--text-muted); text-decoration:none; margin-bottom:16px; font-weight:500; }
        .back-link:hover { color:var(--accent); }

        .card { background:var(--surface); border:1px solid var(--border); border-radius:var(--radius); overflow:hidden; margin-bottom:20px; }
        .card-head { padding:16px 20px; border-bottom:1px solid var(--border); display:flex; align-items:center; justify-content:space-between; }
        .card-title { font-family:'Bebas Neue',sans-serif; font-size:18px; letter-spacing:.07em; }
        .card-count { font-size:11px; color:var(--text-muted); background:var(--bg); padding:3px 10px; border-radius:20px; }

        .cat-table { width:100%; border-collapse:collapse; }
        .cat-table th { padding:10px 14px; text-align:left; font-size:10px; font-weight:700; letter-spacing:.1em; text-transform:uppercase; color:var(--text-dim); background:#fafafa; border-bottom:1px solid var(--border); }
        .cat-table td { padding:12px 14px; font-size:13px; border-bottom:1px solid #f0f0f0; vertical-align:middle; }
        .cat-table tr:last-child td { border-bottom:none; }
        .cat-table tr:hover td { background:#fafafa; }
        .cat-name { font-weight:500; color:var(--text); }
        .badge-seed    { display:inline-block; padding:2px 8px; border-radius:20px; font-size:10px; font-weight:700; letter-spacing:.06em; background:var(--accent-light); color:var(--accent-dark); }
        .badge-unused  { display:inline-block; padding:2px 8px; border-radius:20px; font-size:10px; font-weight:700; letter-spacing:.06em; background:rgba(0,0,0,0.06); color:var(--text-dim); }
        .count-badge   { font-size:12px; font-weight:600; color:var(--text); }
        .count-badge.zero { color:var(--text-dim); font-weight:400; }

        .act-link { font-size:12px; color:var(--accent); cursor:pointer; text-decoration:none; font-weight:600; margin-right:10px; background:none; border:0; font-family:inherit; padding:0; }
        .act-link:hover { text-decoration:underline; }

        .alert { padding:12px 16px; border-radius:8px; font-size:13px; margin-bottom:20px; display:flex; align-items:center; gap:10px; }
        .alert-success { background:rgba(26,158,106,0.10); color:var(--success); border:1px solid rgba(26,158,106,0.25); }
        .alert-danger  { background:rgba(192,57,43,0.08); color:var(--danger); border:1px solid rgba(192,57,43,0.2); }

        .edit-panel { border:1.5px solid var(--accent); border-radius:var(--radius); background:var(--accent-light); padding:20px 24px; }
        .edit-panel-title { font-family:'Bebas Neue',sans-serif; font-size:15px; letter-spacing:.07em; color:var(--accent-dark); margin-bottom:12px; }
        .edit-panel label { font-size:11px; font-weight:700; letter-spacing:.07em; text-transform:uppercase; color:var(--text-muted); display:block; margin-bottom:4px; }
        .edit-panel input[type=text], .edit-panel select { width:100%; padding:10px 14px; border:1.5px solid var(--border); border-radius:8px; font-family:'DM Sans',sans-serif; font-size:14px; background:#fff; outline:none; }
        .edit-panel input:focus, .edit-panel select:focus { border-color:var(--accent); }
        .edit-row { display:grid; grid-template-columns:1fr auto auto; gap:10px; align-items:end; }
        .edit-hint { font-size:11px; color:var(--text-muted); margin-top:6px; }
        .btn { padding:10px 20px; border-radius:8px; font-family:'DM Sans',sans-serif; font-size:13px; font-weight:700; letter-spacing:.04em; cursor:pointer; border:none; transition:all .2s; }
        .btn-primary { background:var(--accent); color:#fff; }
        .btn-primary:hover { background:var(--accent-dark); }
        .btn-secondary { background:#fff; border:1.5px solid var(--border); color:var(--text-muted); }
        .btn-secondary:hover { border-color:var(--text-muted); color:var(--text); }
    </style>
</head>
<body>
<form id="form1" runat="server">

    <nav>
        <a href="FINHome.aspx" style="display:flex;align-items:center;margin-right:16px;flex-shrink:0;background:#fff;border-radius:6px;padding:3px 8px;"><img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" style="height:28px;width:auto;object-fit:contain;" onerror="this.style.display='none'" /></a>
        <a href="/StockApp/ERPHome.aspx" class="nav-item">&#x2302; ERP Home</a>
        <span class="nav-sep">›</span>
        <a href="FINHome.aspx" class="nav-item">Finance</a>
        <span class="nav-sep">›</span>
        <a href="FINServiceProviderReg.aspx" class="nav-item">Service Providers</a>
        <span class="nav-sep">›</span>
        <span class="nav-item active">Manage Categories</span>
        <div class="nav-right">
            <span class="nav-user"><asp:Label ID="lblNavUser" runat="server" /></span>
            <a href="FINLogout.aspx" class="nav-logout">Sign Out</a>
        </div>
    </nav>

    <div class="page-header">
        <div class="page-title">Service <span>Categories</span></div>
        <div class="page-sub">Rename or merge categories used by service providers. Actions update all providers using the source category.</div>
    </div>

    <div class="content">
        <a href="FINServiceProviderReg.aspx" class="back-link">&larr; Back to Service Providers</a>

        <asp:Panel ID="pnlAlert" runat="server" Visible="false">
            <div class="alert" id="alertBox" runat="server">
                <asp:Label ID="lblAlert" runat="server" />
            </div>
        </asp:Panel>

        <div class="card">
            <div class="card-head">
                <span class="card-title">Categories</span>
                <asp:Label ID="lblCount" runat="server" CssClass="card-count" Text="0 categories" />
            </div>
            <asp:Repeater ID="rptCategories" runat="server" OnItemCommand="rptCategories_ItemCommand">
                <HeaderTemplate>
                    <table class="cat-table">
                    <thead><tr>
                        <th>Category</th>
                        <th style="width:100px;text-align:right;">Providers</th>
                        <th style="width:90px;">Type</th>
                        <th style="width:160px;">Action</th>
                    </tr></thead><tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><span class="cat-name"><%# Eval("Category") %></span></td>
                        <td style="text-align:right;">
                            <span class='<%# Convert.ToInt32(Eval("ProviderCount")) == 0 ? "count-badge zero" : "count-badge" %>'>
                                <%# Eval("ProviderCount") %>
                            </span>
                        </td>
                        <td>
                            <%# Convert.ToBoolean(Eval("IsSeed")) ? "<span class='badge-seed'>Seed</span>" : (Convert.ToInt32(Eval("ProviderCount")) == 0 ? "<span class='badge-unused'>Unused</span>" : "<span class='badge-unused'>Custom</span>") %>
                        </td>
                        <td>
                            <asp:LinkButton ID="lbRename" runat="server" CommandName="Rename" CommandArgument='<%# Eval("Category") %>' CssClass="act-link" CausesValidation="false">Rename</asp:LinkButton>
                            <asp:LinkButton ID="lbMerge"  runat="server" CommandName="Merge"  CommandArgument='<%# Eval("Category") %>' CssClass="act-link" CausesValidation="false">Merge</asp:LinkButton>
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></tbody></table></FooterTemplate>
            </asp:Repeater>
        </div>

        <!-- Rename panel -->
        <asp:Panel ID="pnlRename" runat="server" Visible="false" CssClass="edit-panel" style="margin-bottom:20px;">
            <div class="edit-panel-title">Rename Category</div>
            <div class="edit-row">
                <div>
                    <label>Rename "<asp:Label ID="lblRenameFrom" runat="server" />" to</label>
                    <asp:TextBox ID="txtNewName" runat="server" MaxLength="60" />
                </div>
                <asp:Button ID="btnRenameSave"   runat="server" Text="Save"   CssClass="btn btn-primary"   OnClick="btnRenameSave_Click" />
                <asp:Button ID="btnRenameCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary" OnClick="btnCancel_Click" CausesValidation="false" />
            </div>
            <div class="edit-hint">
                All providers currently using this category will be updated. If the new name matches an existing category, the two will be merged.
            </div>
            <asp:HiddenField ID="hfRenameFrom" runat="server" />
        </asp:Panel>

        <!-- Merge panel -->
        <asp:Panel ID="pnlMerge" runat="server" Visible="false" CssClass="edit-panel" style="margin-bottom:20px;">
            <div class="edit-panel-title">Merge Category</div>
            <div class="edit-row">
                <div>
                    <label>Merge "<asp:Label ID="lblMergeFrom" runat="server" />" into</label>
                    <asp:DropDownList ID="ddlMergeTarget" runat="server" />
                </div>
                <asp:Button ID="btnMergeSave"   runat="server" Text="Merge"  CssClass="btn btn-primary"   OnClick="btnMergeSave_Click"
                    OnClientClick="return confirm('This will reassign all providers using the source category to the target category. Continue?');" />
                <asp:Button ID="btnMergeCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary" OnClick="btnCancel_Click" CausesValidation="false" />
            </div>
            <div class="edit-hint">
                After the merge, the source category will disappear from the list (unless it is a seed category, in which case it stays with zero providers).
            </div>
            <asp:HiddenField ID="hfMergeFrom" runat="server" />
        </asp:Panel>

    </div>

</form>
</body>
</html>
