<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FINServiceCatalog.aspx.cs" Inherits="FINApp.FINServiceCatalog" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Service Catalog &mdash; FIN</title>
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

        .page-header { background:var(--surface); border-bottom:1px solid var(--border); padding:24px 40px; display:flex; align-items:center; justify-content:space-between; }
        .page-title { font-family:'Bebas Neue',sans-serif; font-size:28px; letter-spacing:.07em; color:var(--text); }
        .page-title span { color:var(--accent); }
        .page-sub { font-size:12px; color:var(--text-muted); margin-top:2px; }

        .content { max-width:1200px; margin:32px auto; padding:0 32px; display:grid; grid-template-columns:1fr 420px; gap:24px; align-items:start; }

        /* FORM CARD */
        .card { background:var(--surface); border:1px solid var(--border); border-radius:var(--radius); padding:28px; }
        .card-title { font-family:'Bebas Neue',sans-serif; font-size:18px; letter-spacing:.07em; color:var(--text); margin-bottom:24px; padding-bottom:14px; border-bottom:2px solid var(--accent); display:flex; align-items:center; gap:10px; }

        .form-grid { display:grid; grid-template-columns:1fr 1fr; gap:16px; }
        .form-group { display:flex; flex-direction:column; gap:6px; }
        .form-group.full { grid-column:1 / -1; }
        label { font-size:11px; font-weight:700; letter-spacing:.07em; text-transform:uppercase; color:var(--text-muted); }
        label .req { color:var(--danger); margin-left:2px; }
        input[type=text], input[type=number], textarea {
            width:100%; padding:10px 14px; border:1.5px solid var(--border); border-radius:8px;
            font-family:'DM Sans',sans-serif; font-size:14px; color:var(--text);
            background:#fafafa; transition:border-color .2s, background .2s; outline:none;
        }
        input:focus, textarea:focus { border-color:var(--accent); background:#fff; }
        textarea { resize:vertical; min-height:72px; }
        .field-hint { font-size:11px; color:var(--text-dim); }

        .form-section { margin:20px 0 16px; font-size:11px; font-weight:700; letter-spacing:.12em; text-transform:uppercase; color:var(--text-dim); display:flex; align-items:center; gap:10px; }
        .form-section::after { content:''; flex:1; height:1px; background:var(--border); }

        .btn-row { display:flex; gap:10px; margin-top:24px; }
        .btn { padding:11px 24px; border-radius:8px; font-family:'DM Sans',sans-serif; font-size:13px; font-weight:700; letter-spacing:.04em; cursor:pointer; border:none; transition:all .2s; }
        .btn-primary { background:var(--accent); color:#fff; }
        .btn-primary:hover { background:var(--accent-dark); }
        .btn-secondary { background:transparent; border:1.5px solid var(--border); color:var(--text-muted); }
        .btn-secondary:hover { border-color:var(--text-muted); color:var(--text); }
        .btn-danger { background:transparent; border:1.5px solid #ffcccc; color:var(--danger); }
        .btn-danger:hover { background:#fff5f5; }

        .alert { padding:12px 16px; border-radius:8px; font-size:13px; margin-bottom:20px; display:flex; align-items:center; gap:10px; }
        .alert-success { background:rgba(26,158,106,0.10); color:var(--success); border:1px solid rgba(26,158,106,0.25); }
        .alert-danger  { background:rgba(192,57,43,0.08); color:var(--danger); border:1px solid rgba(192,57,43,0.2); }

        /* LIST */
        .list-card { background:var(--surface); border:1px solid var(--border); border-radius:var(--radius); overflow:hidden; }
        .list-header { padding:16px 20px; border-bottom:1px solid var(--border); display:flex; align-items:center; justify-content:space-between; }
        .list-title { font-family:'Bebas Neue',sans-serif; font-size:18px; letter-spacing:.07em; }
        .list-count { font-size:11px; color:var(--text-muted); background:var(--bg); padding:3px 10px; border-radius:20px; }
        .list-search { padding:12px 16px; border-bottom:1px solid var(--border); }
        .list-search input { width:100%; padding:8px 12px; border:1.5px solid var(--border); border-radius:8px; font-size:13px; outline:none; }
        .list-search input:focus { border-color:var(--accent); }

        .svc-table { width:100%; border-collapse:collapse; }
        .svc-table th { padding:10px 14px; text-align:left; font-size:10px; font-weight:700; letter-spacing:.1em; text-transform:uppercase; color:var(--text-dim); background:#fafafa; border-bottom:1px solid var(--border); }
        .svc-table td { padding:12px 14px; font-size:13px; border-bottom:1px solid #f0f0f0; vertical-align:middle; }
        .svc-table tr:last-child td { border-bottom:none; }
        .svc-table tr:hover td { background:#fafafa; }

        .code-cell { font-family:'Roboto Mono',monospace; font-size:12px; color:var(--text-muted); font-weight:600; }
        .badge-active   { display:inline-block; padding:2px 8px; border-radius:20px; font-size:10px; font-weight:700; letter-spacing:.06em; background:rgba(26,158,106,0.12); color:var(--success); }
        .badge-inactive { display:inline-block; padding:2px 8px; border-radius:20px; font-size:10px; font-weight:700; letter-spacing:.06em; background:rgba(0,0,0,0.06); color:var(--text-dim); }
        .usage-count { font-size:12px; color:var(--text-muted); }
        .usage-count.zero { color:var(--text-dim); }

        .act-link { font-size:12px; color:var(--accent); cursor:pointer; text-decoration:none; font-weight:600; margin-right:8px; background:none; border:0; font-family:inherit; padding:0; }
        .act-link:hover { text-decoration:underline; }

        .empty-state { text-align:center; padding:40px 20px; color:var(--text-dim); font-size:13px; }

        @media(max-width:900px) { .content { grid-template-columns:1fr; } }
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
        <span class="nav-item active">Service Catalog</span>
        <div class="nav-right">
            <span class="nav-user"><asp:Label ID="lblNavUser" runat="server" /></span>
            <a href="FINLogout.aspx" class="nav-logout">Sign Out</a>
        </div>
    </nav>

    <div class="page-header">
        <div>
            <div class="page-title">Service <span>Catalog</span></div>
            <div class="page-sub">Master list of services offered by vendors &mdash; Pest Control, Security, Maintenance, etc. Providers link to one or more services during registration.</div>
        </div>
    </div>

    <div class="content">

        <!-- LEFT: FORM -->
        <div>
            <asp:Panel ID="pnlAlert" runat="server" Visible="false">
                <div class="alert" id="alertBox" runat="server">
                    <asp:Label ID="lblAlert" runat="server" />
                </div>
            </asp:Panel>

            <div class="card">
                <div class="card-title">
                    <asp:Label ID="lblFormTitle" runat="server" Text="New Service" />
                    <asp:HiddenField ID="hfServiceID" runat="server" Value="0" />
                </div>

                <div class="form-section">Details</div>
                <div class="form-grid">
                    <div class="form-group">
                        <label>Code</label>
                        <asp:TextBox ID="txtCode" runat="server" ReadOnly="true" style="background:#f0f0f0;color:var(--text-muted);cursor:not-allowed;" placeholder="Auto (SVC-0001)" />
                        <span class="field-hint">Auto-generated on save</span>
                    </div>
                    <div class="form-group">
                        <label>Service Name <span class="req">*</span></label>
                        <asp:TextBox ID="txtName" runat="server" MaxLength="150" placeholder="e.g. Pest Control" />
                    </div>
                    <div class="form-group full">
                        <label>Description</label>
                        <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" MaxLength="500" placeholder="Brief description of the service (optional)" />
                    </div>
                </div>

                <div class="form-section">Billing Defaults <span style="font-weight:400;text-transform:none;letter-spacing:0;color:var(--text-dim);font-size:10px;">(used when billing via JV)</span></div>
                <div class="form-grid">
                    <div class="form-group">
                        <label>HSN / SAC Code</label>
                        <asp:TextBox ID="txtHSN" runat="server" MaxLength="20" placeholder="e.g. 998521" />
                        <span class="field-hint">SAC for services, HSN if classified as goods</span>
                    </div>
                    <div class="form-group">
                        <label>Default GST Rate (%)</label>
                        <asp:TextBox ID="txtGSTRate" runat="server" MaxLength="6" placeholder="18.00" />
                        <span class="field-hint">Typically 18% for most services</span>
                    </div>
                </div>

                <div class="btn-row">
                    <asp:Button ID="btnSave" runat="server" Text="Save Service" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="btnClear_Click" CausesValidation="false" />
                    <asp:Button ID="btnToggleActive" runat="server" Text="Deactivate" CssClass="btn btn-danger" OnClick="btnToggleActive_Click" CausesValidation="false" Visible="false" />
                </div>
            </div>
        </div>

        <!-- RIGHT: LIST -->
        <div class="list-card">
            <div class="list-header">
                <span class="list-title">Services</span>
                <asp:Label ID="lblCount" runat="server" CssClass="list-count" Text="0 services" />
            </div>
            <div class="list-search">
                <input type="text" id="searchBox" placeholder="Search services..." onkeyup="filterTable(this.value)" />
            </div>
            <div style="overflow-x:auto; max-height:650px; overflow-y:auto;">
                <asp:Repeater ID="rptServices" runat="server" OnItemCommand="rptServices_ItemCommand">
                    <HeaderTemplate>
                        <table class="svc-table" id="svcTable">
                        <thead><tr>
                            <th>Code</th>
                            <th>Name</th>
                            <th style="text-align:right;">Used By</th>
                            <th>Status</th>
                            <th>Action</th>
                        </tr></thead><tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr data-id='<%# Eval("ServiceID") %>'>
                            <td class="code-cell"><%# Eval("ServiceCode") %></td>
                            <td>
                                <div style="font-weight:500;"><%# Eval("ServiceName") %></div>
                                <div style="font-size:11px;color:var(--text-dim);margin-top:2px;"><%# Eval("HSNCode") %><%# Eval("HSNCode") != null && Eval("HSNCode").ToString() != "" ? " &middot; " : "" %>GST <%# Eval("GSTRate") %>%</div>
                            </td>
                            <td style="text-align:right;">
                                <span class='<%# Convert.ToInt32(Eval("ProviderCount")) == 0 ? "usage-count zero" : "usage-count" %>'>
                                    <%# Eval("ProviderCount") %>
                                </span>
                            </td>
                            <td>
                                <span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge-active" : "badge-inactive" %>'>
                                    <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
                                </span>
                            </td>
                            <td>
                                <asp:LinkButton ID="lbEdit" runat="server" CommandName="Edit" CommandArgument='<%# Eval("ServiceID") %>' CssClass="act-link" CausesValidation="false">Edit</asp:LinkButton>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate></tbody></table></FooterTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
                    <div class="empty-state">No services yet. Add your first one on the left.</div>
                </asp:Panel>
            </div>
        </div>

    </div>

</form>
<script>
    function filterTable(val) {
        val = val.toLowerCase();
        var rows = document.querySelectorAll('#svcTable tbody tr');
        rows.forEach(function(r) {
            r.style.display = r.innerText.toLowerCase().includes(val) ? '' : 'none';
        });
    }
</script>
</body>
</html>
