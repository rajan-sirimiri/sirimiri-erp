<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FINServiceProviderReg.aspx.cs" Inherits="FINApp.FINServiceProviderReg" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Service Provider Registration &mdash; FIN</title>
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
    <style>
        :root {
            --bg:          #f0f0f0;
            --surface:     #ffffff;
            --border:      #e0e0e0;
            --accent:      #8e44ad;
            --accent-dark: #6c3483;
            --accent-light:#f4ecf7;
            --text:        #1a1a1a;
            --text-muted:  #666;
            --text-dim:    #999;
            --success:     #1a9e6a;
            --danger:      #c0392b;
            --radius:      12px;
        }
        *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
        body { background: var(--bg); color: var(--text); font-family: 'DM Sans', sans-serif; min-height: 100vh; }

        /* NAV */
        nav { background: #1a1a1a; display: flex; align-items: center; padding: 0 28px; height: 52px; gap: 6px; position: sticky; top: 0; z-index: 100; }
        .nav-brand { font-family: 'Bebas Neue', sans-serif; font-size: 18px; color: #fff; letter-spacing: .1em; margin-right: 20px; }
        .nav-item { color: #aaa; text-decoration: none; font-size: 12px; font-weight: 600; letter-spacing: .06em; text-transform: uppercase; padding: 6px 12px; border-radius: 6px; transition: all .2s; }
        .nav-item:hover, .nav-item.active { color: #fff; background: rgba(255,255,255,0.08); }
        .nav-sep { color: #444; margin: 0 4px; }
        .nav-right { margin-left: auto; display: flex; align-items: center; gap: 12px; }
        .nav-user { font-size: 12px; color: #888; }
        .nav-logout { font-size: 11px; color: #666; text-decoration: none; padding: 4px 10px; border: 1px solid #333; border-radius: 5px; transition: all .2s; }
        .nav-logout:hover { color: var(--accent); border-color: var(--accent); }

        /* PAGE HEADER */
        .page-header { background: var(--surface); border-bottom: 1px solid var(--border); padding: 24px 40px; display: flex; align-items: center; justify-content: space-between; }
        .page-title { font-family: 'Bebas Neue', sans-serif; font-size: 28px; letter-spacing: .07em; color: var(--text); }
        .page-title span { color: var(--accent); }
        .page-sub { font-size: 12px; color: var(--text-muted); margin-top: 2px; }

        /* LAYOUT */
        .content { max-width: 1200px; margin: 32px auto; padding: 0 32px; display: grid; grid-template-columns: 1fr 400px; gap: 24px; align-items: start; }

        /* FORM CARD */
        .card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius); padding: 28px; }
        .card-title { font-family: 'Bebas Neue', sans-serif; font-size: 18px; letter-spacing: .07em; color: var(--text); margin-bottom: 24px; padding-bottom: 14px; border-bottom: 2px solid var(--accent); display: flex; align-items: center; gap: 10px; }

        /* FORM GRID */
        .form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
        .form-group { display: flex; flex-direction: column; gap: 6px; }
        .form-group.full { grid-column: 1 / -1; }
        label { font-size: 11px; font-weight: 700; letter-spacing: .07em; text-transform: uppercase; color: var(--text-muted); }
        label .req { color: var(--danger); margin-left: 2px; }
        input[type=text], input[type=email], input[type=tel], select, textarea {
            width: 100%; padding: 10px 14px; border: 1.5px solid var(--border); border-radius: 8px;
            font-family: 'DM Sans', sans-serif; font-size: 14px; color: var(--text);
            background: #fafafa; transition: border-color .2s, background .2s; outline: none;
        }
        input:focus, select:focus, textarea:focus { border-color: var(--accent); background: #fff; }
        textarea { resize: vertical; min-height: 72px; }
        .field-hint { font-size: 11px; color: var(--text-dim); }

        /* SECTION DIVIDER */
        .form-section { margin: 20px 0 16px; font-size: 11px; font-weight: 700; letter-spacing: .12em; text-transform: uppercase; color: var(--text-dim); display: flex; align-items: center; gap: 10px; }
        .form-section::after { content: ''; flex: 1; height: 1px; background: var(--border); }

        /* BUTTONS */
        .btn-row { display: flex; gap: 10px; margin-top: 24px; }
        .btn { padding: 11px 24px; border-radius: 8px; font-family: 'DM Sans', sans-serif; font-size: 13px; font-weight: 700; letter-spacing: .04em; cursor: pointer; border: none; transition: all .2s; }
        .btn-primary { background: var(--accent); color: #fff; }
        .btn-primary:hover { background: var(--accent-dark); }
        .btn-secondary { background: transparent; border: 1.5px solid var(--border); color: var(--text-muted); }
        .btn-secondary:hover { border-color: var(--text-muted); color: var(--text); }
        .btn-danger { background: transparent; border: 1.5px solid #ffcccc; color: var(--danger); }
        .btn-danger:hover { background: #fff5f5; }

        /* ALERT */
        .alert { padding: 12px 16px; border-radius: 8px; font-size: 13px; margin-bottom: 20px; display: flex; align-items: center; gap: 10px; }
        .alert-success { background: rgba(26,158,106,0.10); color: var(--success); border: 1px solid rgba(26,158,106,0.25); }
        .alert-danger  { background: rgba(192,57,43,0.08); color: var(--danger); border: 1px solid rgba(192,57,43,0.2); }

        /* LIST */
        .list-card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius); overflow: hidden; }
        .list-header { padding: 16px 20px; border-bottom: 1px solid var(--border); display: flex; align-items: center; justify-content: space-between; }
        .list-title { font-family: 'Bebas Neue', sans-serif; font-size: 18px; letter-spacing: .07em; }
        .list-count { font-size: 11px; color: var(--text-muted); background: var(--bg); padding: 3px 10px; border-radius: 20px; }
        .list-search { padding: 12px 16px; border-bottom: 1px solid var(--border); }
        .list-search input { width: 100%; padding: 8px 12px; border: 1.5px solid var(--border); border-radius: 8px; font-size: 13px; font-family: 'DM Sans', sans-serif; outline: none; }
        .list-search input:focus { border-color: var(--accent); }

        .sp-table { width: 100%; border-collapse: collapse; }
        .sp-table th { padding: 10px 14px; text-align: left; font-size: 10px; font-weight: 700; letter-spacing: .1em; text-transform: uppercase; color: var(--text-dim); background: #fafafa; border-bottom: 1px solid var(--border); }
        .sp-table td { padding: 12px 14px; font-size: 13px; border-bottom: 1px solid #f0f0f0; vertical-align: middle; }
        .sp-table tr:last-child td { border-bottom: none; }
        .sp-table tr:hover td { background: #fafafa; }

        .badge-active   { display: inline-block; padding: 2px 8px; border-radius: 20px; font-size: 10px; font-weight: 700; letter-spacing: .06em; background: rgba(26,158,106,0.12); color: var(--success); }
        .badge-inactive { display: inline-block; padding: 2px 8px; border-radius: 20px; font-size: 10px; font-weight: 700; letter-spacing: .06em; background: rgba(0,0,0,0.06); color: var(--text-dim); }
        .badge-cat      { display: inline-block; padding: 2px 8px; border-radius: 20px; font-size: 10px; font-weight: 600; background: var(--accent-light); color: var(--accent-dark); }

        .act-link { font-size: 12px; color: var(--accent); cursor: pointer; text-decoration: none; font-weight: 600; margin-right: 8px; }
        .act-link:hover { text-decoration: underline; }

        .empty-state { text-align: center; padding: 40px 20px; color: var(--text-dim); font-size: 13px; }

        @media(max-width:900px) { .content { grid-template-columns: 1fr; } }

        /* ── Multi-select service chip list ── */
        .svc-list ul.svc-cbl { list-style:none; margin:0; padding:4px; }
        .svc-list ul.svc-cbl li {
            display:flex; align-items:flex-start; gap:8px;
            padding:6px 10px; border-radius:6px; margin-bottom:2px;
            transition:background .12s;
        }
        .svc-list ul.svc-cbl li:hover { background:var(--accent-light); }
        .svc-list ul.svc-cbl li input[type=checkbox] {
            margin-top:3px; width:14px; height:14px;
            accent-color:var(--accent); flex-shrink:0;
        }
        .svc-list ul.svc-cbl li label { font-size:13px; color:var(--text); cursor:pointer; flex:1; line-height:1.35; font-weight:400; text-transform:none; letter-spacing:0; }
        .svc-list ul.svc-cbl li label .svc-code { font-family:'Roboto Mono',monospace; font-size:11px; color:var(--text-muted); font-weight:600; margin-right:6px; }
        .svc-list ul.svc-cbl li label .svc-name { font-weight:500; }
        .svc-list ul.svc-cbl li label .svc-meta { font-size:11px; color:var(--text-dim); margin-left:4px; }
        .svc-list ul.svc-cbl li.svc-hidden { display:none; }

        /* provider list chip pills */
        .svc-chips { display:flex; flex-wrap:wrap; gap:4px; margin-top:4px; }
        .svc-chip { display:inline-block; padding:2px 8px; border-radius:20px; font-size:10px; font-weight:600; background:var(--accent-light); color:var(--accent-dark); }
    </style>
</head>
<body>
<form id="form1" runat="server">

    <!-- NAV -->
    <nav>
        <a href="FINHome.aspx" style="display:flex;align-items:center;margin-right:16px;flex-shrink:0;background:#fff;border-radius:6px;padding:3px 8px;"><img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" style="height:28px;width:auto;object-fit:contain;" onerror="this.style.display='none'" /></a>
        <a href="/StockApp/ERPHome.aspx" class="nav-item">&#x2302; ERP Home</a>
        <span class="nav-sep">›</span>
        <a href="FINHome.aspx" class="nav-item">Finance</a>
        <span class="nav-sep">›</span>
        <span class="nav-item active">Service Providers</span>
        <div class="nav-right">
            <span class="nav-user"><asp:Label ID="lblNavUser" runat="server" /></span>
            <a href="FINLogout.aspx" class="nav-logout">Sign Out</a>
        </div>
    </nav>

    <!-- PAGE HEADER -->
    <div class="page-header">
        <div>
            <div class="page-title">Service Provider <span>Registration</span></div>
            <div class="page-sub">Register vendors that provide services (Pest Control, Security, Maintenance, etc.) &mdash; billed via Journal Entries, not GRNs</div>
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
                <div class="card-title" style="justify-content:space-between;">
                    <div style="display:flex;align-items:center;gap:10px;">
                        <asp:Label ID="lblFormTitle" runat="server" Text="New Service Provider" />
                        <asp:HiddenField ID="hfProviderID" runat="server" Value="0" />
                    </div>
                </div>

                <div class="form-section">Identification</div>
                <div class="form-grid">
                    <div class="form-group">
                        <label>Code</label>
                        <asp:TextBox ID="txtCode" runat="server" ReadOnly="true" style="background:#f0f0f0;color:var(--text-muted);cursor:not-allowed;" placeholder="Auto-generated (SRV-0001)" />
                        <span class="field-hint">Auto-generated on save</span>
                    </div>
                    <div class="form-group">
                        <label>Name <span class="req">*</span></label>
                        <asp:TextBox ID="txtName" runat="server" MaxLength="200" placeholder="Full legal name" />
                    </div>
                    <div class="form-group">
                        <label>GST Number</label>
                        <asp:TextBox ID="txtGST" runat="server" MaxLength="20" placeholder="e.g. 33XXXXX0000X1ZX" />
                    </div>
                    <div class="form-group">
                        <label>PAN</label>
                        <asp:TextBox ID="txtPAN" runat="server" MaxLength="10" placeholder="e.g. ABCDE1234F" />
                    </div>
                </div>

                <div class="form-section" style="display:flex;align-items:center;">
                    <span>Services Offered</span>
                    <span style="margin-left:auto;">
                        <a href="FINServiceCatalog.aspx"
                           style="font-size:11px;font-weight:600;color:var(--accent);text-decoration:none;padding:4px 10px;border:1.5px solid var(--accent-light);border-radius:6px;letter-spacing:.03em;text-transform:uppercase;"
                           onmouseover="this.style.background='var(--accent-light)'" onmouseout="this.style.background=''">
                            Manage Catalog &rarr;
                        </a>
                    </span>
                </div>
                <div>
                    <div class="svc-search-wrap" style="margin-bottom:10px;">
                        <input type="text" id="svcSearch" placeholder="Search services by name or code..." onkeyup="filterSvcList(this.value)"
                               style="width:100%;padding:9px 12px;border:1.5px solid var(--border);border-radius:8px;font-size:13px;font-family:inherit;background:#fafafa;outline:none;" />
                    </div>
                    <asp:Panel ID="pnlNoServices" runat="server" Visible="false"
                        style="padding:20px;border:1.5px dashed var(--border);border-radius:8px;text-align:center;color:var(--text-muted);font-size:13px;">
                        No services defined yet.
                        <a href="FINServiceCatalog.aspx" style="color:var(--accent);font-weight:600;">Create your first service</a>
                        in the catalog.
                    </asp:Panel>
                    <div id="svcListWrap" class="svc-list" style="max-height:240px;overflow-y:auto;border:1.5px solid var(--border);border-radius:8px;background:#fff;">
                        <asp:Literal ID="litServices" runat="server" />
                    </div>
                    <span class="field-hint" style="margin-top:6px;display:block;">
                        Pick all services this vendor offers. Use the catalog to add new services.
                    </span>
                </div>

                <div class="form-section">Contact</div>
                <div class="form-grid">
                    <div class="form-group">
                        <label>Contact Person</label>
                        <asp:TextBox ID="txtContact" runat="server" MaxLength="100" placeholder="Name" />
                    </div>
                    <div class="form-group">
                        <label>Phone</label>
                        <asp:TextBox ID="txtPhone" runat="server" MaxLength="20" placeholder="+91 XXXXX XXXXX" />
                    </div>
                    <div class="form-group full">
                        <label>Email</label>
                        <asp:TextBox ID="txtEmail" runat="server" MaxLength="100" placeholder="contact@example.com" />
                    </div>
                </div>

                <div class="form-section">Address</div>
                <div class="form-grid">
                    <div class="form-group full">
                        <label>Address</label>
                        <asp:TextBox ID="txtAddress" runat="server" TextMode="MultiLine" MaxLength="500" placeholder="Street address, area..." />
                    </div>
                    <div class="form-group">
                        <label>City</label>
                        <asp:TextBox ID="txtCity" runat="server" MaxLength="100" />
                    </div>
                    <div class="form-group">
                        <label>State</label>
                        <asp:TextBox ID="txtState" runat="server" MaxLength="100" />
                    </div>
                    <div class="form-group">
                        <label>Pin Code</label>
                        <asp:TextBox ID="txtPinCode" runat="server" MaxLength="10" />
                    </div>
                </div>

                <div class="form-section">Notes</div>
                <div class="form-grid">
                    <div class="form-group full">
                        <label>Free-text notes</label>
                        <asp:TextBox ID="txtNotes" runat="server" TextMode="MultiLine" MaxLength="60" placeholder="Any additional notes about this vendor (optional)" />
                        <span class="field-hint">Short internal remarks &mdash; not shown on invoices or statements.</span>
                    </div>
                </div>

                <div class="btn-row">
                    <asp:Button ID="btnSave" runat="server" Text="Save Service Provider" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="btnClear_Click" CausesValidation="false" />
                    <asp:Button ID="btnToggleActive" runat="server" Text="Deactivate" CssClass="btn btn-danger" OnClick="btnToggleActive_Click" CausesValidation="false" Visible="false" />
                </div>
            </div>
        </div>

        <!-- RIGHT: LIST -->
        <div class="list-card">
            <div class="list-header">
                <span class="list-title">Service Providers</span>
                <asp:Label ID="lblCount" runat="server" CssClass="list-count" Text="0 records" />
            </div>
            <div class="list-search">
                <input type="text" id="searchBox" placeholder="Search providers..." onkeyup="filterTable(this.value)" />
            </div>
            <div style="overflow-x:auto; max-height:650px; overflow-y:auto;">
                <asp:Repeater ID="rptProviders" runat="server" OnItemCommand="rptProviders_ItemCommand">
                    <HeaderTemplate>
                        <table class="sp-table" id="spTable">
                        <thead><tr>
                            <th>Code</th>
                            <th>Name / Category</th>
                            <th>Status</th>
                            <th>Action</th>
                        </tr></thead><tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr data-id='<%# Eval("SupplierID") %>'>
                            <td style="font-weight:600;font-size:12px;color:var(--text-muted);"><%# Eval("SupplierCode") %></td>
                            <td>
                                <div style="font-weight:500;"><%# Eval("SupplierName") %></div>
                                <div class="svc-chips-cell" style="margin-top:4px;">
                                    <%# RenderServiceChips(Eval("ServicesText")) %>
                                </div>
                            </td>
                            <td>
                                <span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge-active" : "badge-inactive" %>'>
                                    <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
                                </span>
                            </td>
                            <td>
                                <asp:LinkButton ID="lbEdit" runat="server" CommandName="Edit" CommandArgument='<%# Eval("SupplierID") %>' CssClass="act-link" CausesValidation="false">Edit</asp:LinkButton>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate></tbody></table></FooterTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
                    <div class="empty-state">No service providers yet. Add your first one.</div>
                </asp:Panel>
            </div>
        </div>

    </div>

</form>
<script>
    function filterTable(val) {
        val = val.toLowerCase();
        var rows = document.querySelectorAll('#spTable tbody tr');
        rows.forEach(function(r) {
            r.style.display = r.innerText.toLowerCase().includes(val) ? '' : 'none';
        });
    }

    function filterSvcList(val) {
        val = val.toLowerCase().trim();
        var items = document.querySelectorAll('.svc-list ul.svc-cbl li');
        items.forEach(function(li) {
            var txt = li.innerText.toLowerCase();
            li.classList.toggle('svc-hidden', val !== '' && txt.indexOf(val) === -1);
        });
    }
</script>
</body>
</html>
