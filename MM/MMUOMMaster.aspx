<%@ Page Language="C#" AutoEventWireup="true" Inherits="MMApp.MMUOMMaster" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>UOM Master &mdash; MM</title>
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet" />
    <style>
        :root { --bg:#f5f5f5; --surface:#ffffff; --border:#e0e0e0; --accent:#cc1e1e; --purple:#8c50d2; --text:#1a1a1a; --text-muted:#666; --text-dim:#999; --radius:12px; }
        *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
        body { background: var(--bg); color: var(--text); font-family: 'DM Sans', sans-serif; min-height: 100vh; }
        nav { background: #1a1a1a; display: flex; align-items: center; padding: 0 28px; height: 52px; gap: 6px; position: sticky; top: 0; z-index: 100; }
        .nav-brand { font-family: 'Bebas Neue', sans-serif; font-size: 18px; color: #fff; letter-spacing: .1em; margin-right: 20px; }
        .nav-item { color: #aaa; text-decoration: none; font-size: 12px; font-weight: 600; letter-spacing: .06em; text-transform: uppercase; padding: 6px 12px; border-radius: 6px; transition: all .2s; }
        .nav-item:hover, .nav-item.active { color: #fff; background: rgba(255,255,255,0.08); }
        .nav-sep { color: #444; margin: 0 4px; }
        .nav-right { margin-left: auto; display: flex; align-items: center; gap: 12px; }
        .nav-user { font-size: 12px; color: #888; }
        .nav-logout { font-size: 11px; color: #666; text-decoration: none; padding: 4px 10px; border: 1px solid #333; border-radius: 5px; transition: all .2s; }
        .nav-logout:hover { color: var(--accent); border-color: var(--accent); }
        .page-header { background: var(--surface); border-bottom: 1px solid var(--border); padding: 24px 40px; }
        .page-title { font-family: 'Bebas Neue', sans-serif; font-size: 28px; letter-spacing: .07em; }
        .page-title span { color: var(--purple); }
        .page-sub { font-size: 12px; color: var(--text-muted); margin-top: 2px; }

        /* Compact two-column layout &mdash; form is narrow for UOM */
        .content { max-width: 900px; margin: 32px auto; padding: 0 32px; display: grid; grid-template-columns: 320px 1fr; gap: 24px; align-items: start; }

        .card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius); padding: 28px; }
        .card-title { font-family: 'Bebas Neue', sans-serif; font-size: 18px; letter-spacing: .07em; color: var(--text); margin-bottom: 24px; padding-bottom: 14px; border-bottom: 2px solid var(--purple); }
        .form-group { display: flex; flex-direction: column; gap: 6px; margin-bottom: 16px; }
        label { font-size: 11px; font-weight: 700; letter-spacing: .07em; text-transform: uppercase; color: var(--text-muted); }
        label .req { color: var(--accent); margin-left: 2px; }
        input[type=text] { width: 100%; padding: 10px 14px; border: 1.5px solid var(--border); border-radius: 8px; font-family: 'DM Sans', sans-serif; font-size: 14px; color: var(--text); background: #fafafa; transition: border-color .2s; outline: none; }
        input:focus { border-color: var(--purple); background: #fff; }
        .field-hint { font-size: 11px; color: var(--text-dim); }
        .btn-row { display: flex; gap: 10px; margin-top: 8px; }
        .btn { padding: 11px 20px; border-radius: 8px; font-family: 'DM Sans', sans-serif; font-size: 13px; font-weight: 700; letter-spacing: .04em; cursor: pointer; border: none; transition: all .2s; }
        .btn-primary { background: var(--purple); color: #fff; }
        .btn-primary:hover { background: #6e3aab; }
        .btn-secondary { background: transparent; border: 1.5px solid var(--border); color: var(--text-muted); }
        .btn-secondary:hover { border-color: var(--text-muted); color: var(--text); }
        .btn-danger { background: transparent; border: 1.5px solid #ffcccc; color: var(--accent); }
        .btn-danger:hover { background: #fff5f5; }
        .alert { padding: 12px 16px; border-radius: 8px; font-size: 13px; margin-bottom: 20px; }
        .alert-success { background: rgba(140,80,210,0.08); color: var(--purple); border: 1px solid rgba(140,80,210,0.25); }
        .alert-danger  { background: rgba(204,30,30,0.08); color: var(--accent); border: 1px solid rgba(204,30,30,0.2); }

        .list-card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius); overflow: hidden; }
        .list-header { padding: 16px 20px; border-bottom: 1px solid var(--border); display: flex; align-items: center; justify-content: space-between; }
        .list-title { font-family: 'Bebas Neue', sans-serif; font-size: 18px; letter-spacing: .07em; }
        .list-count { font-size: 11px; color: var(--text-muted); background: var(--bg); padding: 3px 10px; border-radius: 20px; }
        .list-search { padding: 12px 16px; border-bottom: 1px solid var(--border); }
        .list-search input { width: 100%; padding: 8px 12px; border: 1.5px solid var(--border); border-radius: 8px; font-size: 13px; font-family: 'DM Sans', sans-serif; outline: none; }
        .list-search input:focus { border-color: var(--purple); }

        .uom-table { width: 100%; border-collapse: collapse; }
        .uom-table th { padding: 10px 16px; text-align: left; font-size: 10px; font-weight: 700; letter-spacing: .1em; text-transform: uppercase; color: var(--text-dim); background: #fafafa; border-bottom: 1px solid var(--border); }
        .uom-table td { padding: 12px 16px; font-size: 13px; border-bottom: 1px solid #f0f0f0; vertical-align: middle; }
        .uom-table tr:last-child td { border-bottom: none; }
        .uom-table tr:hover td { background: #fafafa; }

        .abbr-pill { display: inline-block; padding: 3px 10px; border-radius: 20px; font-size: 11px; font-weight: 700; background: rgba(140,80,210,0.10); color: var(--purple); letter-spacing: .04em; }
        .badge-active   { display: inline-block; padding: 2px 8px; border-radius: 20px; font-size: 10px; font-weight: 700; background: rgba(140,80,210,0.12); color: var(--purple); }
        .badge-inactive { display: inline-block; padding: 2px 8px; border-radius: 20px; font-size: 10px; font-weight: 700; background: rgba(0,0,0,0.06); color: var(--text-dim); }
        .act-link { font-size: 12px; color: var(--purple); cursor: pointer; font-weight: 600; }
        .act-link:hover { text-decoration: underline; }
        .empty-state { text-align: center; padding: 40px 20px; color: var(--text-dim); font-size: 13px; }
        @media(max-width:700px) { .content { grid-template-columns: 1fr; } }
    </style>
</head>
<body>
<form id="form1" runat="server">
    <nav>
        <a href="MMHome.aspx" style="display:flex;align-items:center;margin-right:16px;flex-shrink:0;background:#fff;border-radius:6px;padding:3px 8px;"><img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" style="height:28px;width:auto;object-fit:contain;" onerror="this.style.display='none'" /></a>
        <a href="/StockApp/ERPHome.aspx" class="nav-item">&#x2302; ERP Home</a>
        <span class="nav-sep">›</span>
        <a href="MMHome.aspx" class="nav-item">Home</a>
        <span class="nav-sep">›</span>
        <span class="nav-item active">UOM Master</span>
        <div class="nav-right">
            <span class="nav-user"><asp:Label ID="lblNavUser" runat="server" /></span>
            <a href="MMLogout.aspx" class="nav-logout" onclick="return confirm('Sign out?')">Sign Out</a>
        </div>
    </nav>

    <div class="page-header">
        <div class="page-title">UOM <span>Master</span></div>
        <div class="page-sub">Manage units of measure used across raw materials and packing materials</div>
    </div>

    <div class="content">

        <!-- FORM -->
        <div>
            <asp:Panel ID="pnlAlert" runat="server" Visible="false">
                <div class="alert" id="alertBox" runat="server">
                    <asp:Label ID="lblAlert" runat="server" />
                </div>
            </asp:Panel>
            <div class="card">
                <div class="card-title"><asp:Label ID="lblFormTitle" runat="server" Text="New UOM" /></div>
                <asp:HiddenField ID="hfUOMID" runat="server" Value="0" />

                <div class="form-group">
                    <label>UOM Name <span class="req">*</span></label>
                    <asp:TextBox ID="txtName" runat="server" MaxLength="50" placeholder="e.g. Kilogram" />
                </div>
                <div class="form-group">
                    <label>Abbreviation <span class="req">*</span></label>
                    <asp:TextBox ID="txtAbbr" runat="server" MaxLength="10" placeholder="e.g. kg" />
                    <span class="field-hint">Short form shown in forms and reports</span>
                </div>

                <div class="btn-row">
                    <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="btnClear_Click" CausesValidation="false" />
                    <asp:Button ID="btnToggleActive" runat="server" Text="Deactivate" CssClass="btn btn-danger" OnClick="btnToggleActive_Click" CausesValidation="false" Visible="false" />
                </div>
            </div>
        </div>

        <!-- LIST -->
        <div class="list-card">
            <div class="list-header">
                <span class="list-title">Units of Measure</span>
                <asp:Label ID="lblCount" runat="server" CssClass="list-count" Text="0 records" />
            </div>
            <div class="list-search">
                <input type="text" placeholder="Search..." onkeyup="filterTable(this.value)" />
            </div>
            <div style="overflow-x:auto; max-height:560px; overflow-y:auto;">
                <asp:Repeater ID="rptUOM" runat="server" OnItemCommand="rptUOM_ItemCommand">
                    <HeaderTemplate>
                        <table class="uom-table" id="uomTable">
                        <thead><tr><th>Name</th><th>Abbr</th><th>Status</th><th></th></tr></thead><tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td style="font-weight:500;"><%# Eval("UOMName") %></td>
                            <td><span class="abbr-pill"><%# Eval("Abbreviation") %></span></td>
                            <td><span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge-active" : "badge-inactive" %>'><%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %></span></td>
                            <td><asp:LinkButton runat="server" CommandName="Edit" CommandArgument='<%# Eval("UOMID") %>' CssClass="act-link" CausesValidation="false">Edit</asp:LinkButton></td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate></tbody></table></FooterTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
                    <div class="empty-state">No UOM records found.</div>
                </asp:Panel>
            </div>
        </div>

    </div>
</form>
<script>
function filterTable(val) {
    val = val.toLowerCase();
    document.querySelectorAll('#uomTable tbody tr').forEach(function(r) {
        r.style.display = r.innerText.toLowerCase().includes(val) ? '' : 'none';
    });
}
</script>
</body>
</html>
