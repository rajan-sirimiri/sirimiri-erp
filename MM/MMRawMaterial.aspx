<%@ Page Language="C#" AutoEventWireup="true" Inherits="MMApp.MMRawMaterial" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Raw Materials &mdash; MM</title>
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
    <style>
        :root { --bg:#f5f5f5; --surface:#ffffff; --border:#e0e0e0; --accent:#cc1e1e; --gold:#b8860b; --teal:#1a9e6a; --text:#1a1a1a; --text-muted:#666; --text-dim:#999; --radius:12px; }
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
        .page-title span { color: var(--teal); }
        .page-sub { font-size: 12px; color: var(--text-muted); margin-top: 2px; }
        .content { max-width: 1200px; margin: 32px auto; padding: 0 32px; display: grid; grid-template-columns: 1fr 400px; gap: 24px; align-items: start; }
        .card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius); padding: 28px; }
        .card-title { font-family: 'Bebas Neue', sans-serif; font-size: 18px; letter-spacing: .07em; color: var(--text); margin-bottom: 24px; padding-bottom: 14px; border-bottom: 2px solid var(--teal); }
        .form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
        .form-group { display: flex; flex-direction: column; gap: 6px; }
        .form-group.full { grid-column: 1 / -1; }
        label { font-size: 11px; font-weight: 700; letter-spacing: .07em; text-transform: uppercase; color: var(--text-muted); }
        label .req { color: var(--accent); margin-left: 2px; }
        input[type=text], select, textarea { width: 100%; padding: 10px 14px; border: 1.5px solid var(--border); border-radius: 8px; font-family: 'DM Sans', sans-serif; font-size: 14px; color: var(--text); background: #fafafa; transition: border-color .2s; outline: none; }
        input:focus, select:focus, textarea:focus { border-color: var(--teal); background: #fff; }
        input[readonly] { background: #f0f0f0; color: var(--text-muted); cursor: not-allowed; }
        textarea { resize: vertical; min-height: 72px; }
        .field-hint { font-size: 11px; color: var(--text-dim); }
        .form-section { margin: 20px 0 16px; font-size: 11px; font-weight: 700; letter-spacing: .12em; text-transform: uppercase; color: var(--text-dim); display: flex; align-items: center; gap: 10px; }
        .form-section::after { content: ''; flex: 1; height: 1px; background: var(--border); }
        .btn-row { display: flex; gap: 10px; margin-top: 24px; }
        .btn { padding: 11px 24px; border-radius: 8px; font-family: 'DM Sans', sans-serif; font-size: 13px; font-weight: 700; letter-spacing: .04em; cursor: pointer; border: none; transition: all .2s; }
        .btn-primary { background: var(--teal); color: #fff; }
        .btn-primary:hover { background: #147a52; }
        .btn-secondary { background: transparent; border: 1.5px solid var(--border); color: var(--text-muted); }
        .btn-secondary:hover { border-color: var(--text-muted); color: var(--text); }
        .btn-danger { background: transparent; border: 1.5px solid #ffcccc; color: var(--accent); }
        .btn-danger:hover { background: #fff5f5; }
        .alert { padding: 12px 16px; border-radius: 8px; font-size: 13px; margin-bottom: 20px; }
        .alert-success { background: rgba(26,158,106,0.10); color: var(--teal); border: 1px solid rgba(26,158,106,0.25); }
        .alert-danger  { background: rgba(204,30,30,0.08); color: var(--accent); border: 1px solid rgba(204,30,30,0.2); }
        .list-card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius); overflow: hidden; }
        .list-header { padding: 16px 20px; border-bottom: 1px solid var(--border); display: flex; align-items: center; justify-content: space-between; }
        .list-title { font-family: 'Bebas Neue', sans-serif; font-size: 18px; letter-spacing: .07em; }
        .list-count { font-size: 11px; color: var(--text-muted); background: var(--bg); padding: 3px 10px; border-radius: 20px; }
        .list-search { padding: 12px 16px; border-bottom: 1px solid var(--border); }
        .list-search input { width: 100%; padding: 8px 12px; border: 1.5px solid var(--border); border-radius: 8px; font-size: 13px; font-family: 'DM Sans', sans-serif; outline: none; }
        .list-search input:focus { border-color: var(--teal); }
        .rm-table { width: 100%; border-collapse: collapse; }
        .rm-table th { padding: 10px 14px; text-align: left; font-size: 10px; font-weight: 700; letter-spacing: .1em; text-transform: uppercase; color: var(--text-dim); background: #fafafa; border-bottom: 1px solid var(--border); }
        .rm-table td { padding: 11px 14px; font-size: 13px; border-bottom: 1px solid #f0f0f0; vertical-align: middle; }
        .rm-table tr:last-child td { border-bottom: none; }
        .rm-table tr:hover td { background: #fafafa; }
        .badge-active { display: inline-block; padding: 2px 8px; border-radius: 20px; font-size: 10px; font-weight: 700; background: rgba(26,158,106,0.12); color: var(--teal); }
        .badge-inactive { display: inline-block; padding: 2px 8px; border-radius: 20px; font-size: 10px; font-weight: 700; background: rgba(0,0,0,0.06); color: var(--text-dim); }
        .act-link { font-size: 12px; color: var(--teal); cursor: pointer; font-weight: 600; }
        .act-link:hover { text-decoration: underline; }
        .empty-state { text-align: center; padding: 40px 20px; color: var(--text-dim); font-size: 13px; }
        @media(max-width:900px) { .content { grid-template-columns: 1fr; } }

        /* ── Opening Stock Panel ── */
        .os-panel { margin-top: 24px; border-top: 2px solid var(--teal); padding-top: 20px; }
        .os-title { font-family: 'Bebas Neue', sans-serif; font-size: 16px; letter-spacing: .07em; color: var(--teal); margin-bottom: 16px; display:flex; align-items:center; gap:10px; }
        .os-grid { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 14px; }
        .os-value-box { background: #f0faf5; border: 1px solid #c3ece0; border-radius: 8px; padding: 10px 16px; font-size: 13px; color: var(--text-muted); }
        .os-value-box strong { display:block; font-size: 18px; font-weight: 700; color: var(--teal); }
        .os-last-saved { font-size: 11px; color: var(--text-muted); margin-top: 4px; }

        /* Scrap section */
        .scrap-panel { margin-top:18px; border-top:1px solid var(--border); padding-top:16px; }
        .scrap-title { font-size:11px; font-weight:700; letter-spacing:.08em; text-transform:uppercase; color:var(--text-muted); margin-bottom:12px; }
        .scrap-add-row { display:flex; gap:8px; align-items:flex-end; margin-bottom:12px; }
        .scrap-add-row select { flex:1; padding:8px 10px; border:1.5px solid var(--border); border-radius:7px; font-size:12px; font-family:inherit; }
        .btn-add-scrap { background:var(--teal); color:#fff; border:none; border-radius:7px; padding:8px 14px; font-size:12px; font-weight:700; cursor:pointer; white-space:nowrap; }
        .btn-add-scrap:hover { background:#148a5a; }
        .scrap-tag { display:inline-flex; align-items:center; gap:6px; background:#fff3e0; border:1px solid #ffe0b2; border-radius:20px; padding:4px 10px; font-size:12px; font-weight:600; color:#e65100; margin:3px; }
        .scrap-tag .del-scrap { background:none; border:none; color:#e65100; cursor:pointer; font-size:14px; line-height:1; padding:0 2px; font-weight:700; }
        .scrap-tag .del-scrap:hover { color:var(--accent); }
        .scrap-empty { font-size:12px; color:var(--text-dim); font-style:italic; }
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
        <span class="nav-item active">Raw Materials</span>
        <div class="nav-right">
            <span class="nav-user"><asp:Label ID="lblNavUser" runat="server" /></span>
            <a href="#" class="nav-logout" onclick="erpConfirm('Sign out?',{title:'Sign Out',type:'warn',okText:'Sign Out',onOk:function(){window.location='MMLogout.aspx';}});return false;">Sign Out</a>
        </div>
    </nav>

    <div class="page-header">
        <div class="page-title">Raw <span>Materials</span></div>
        <div class="page-sub">Manage raw material master &mdash; add, edit and activate/deactivate</div>
    </div>

    <div class="content">
        <div>
            <asp:Panel ID="pnlAlert" runat="server" Visible="false">
                <div class="alert" id="alertBox" runat="server">
                    <asp:Label ID="lblAlert" runat="server" />
                </div>
            </asp:Panel>
            <div class="card">
                <div class="card-title"><asp:Label ID="lblFormTitle" runat="server" Text="New Raw Material" /></div>
                <asp:HiddenField ID="hfRMID" runat="server" Value="0" />

                <div class="form-grid">
                    <div class="form-group">
                        <label>Material Code</label>
                        <asp:TextBox ID="txtCode" runat="server" ReadOnly="true" placeholder="Auto-generated" />
                        <span class="field-hint">Auto-generated on save</span>
                    </div>
                    <div class="form-group">
                        <label>Material Name <span class="req">*</span></label>
                        <asp:TextBox ID="txtName" runat="server" MaxLength="200" placeholder="e.g. Wheat Flour" />
                    </div>
                    <div class="form-group">
                        <label>Unit of Measure <span class="req">*</span></label>
                        <asp:DropDownList ID="ddlUOM" runat="server" />
                    </div>
                    <div class="form-group">
                        <label>Reorder Level</label>
                        <asp:TextBox ID="txtReorder" runat="server" MaxLength="10" placeholder="0" />
                        <span class="field-hint">Minimum stock before reorder alert</span>
                    </div>
                    <div class="form-group">
                        <label>HSN Code</label>
                        <asp:TextBox ID="txtHSN" runat="server" MaxLength="20" placeholder="e.g. 1101" />
                        <span class="field-hint">Optional &mdash; for GST invoicing</span>
                    </div>
                    <div class="form-group">
                        <label>GST Rate (%)</label>
                        <asp:TextBox ID="txtGSTRate" runat="server" MaxLength="6" placeholder="e.g. 18" />
                        <span class="field-hint">Optional &mdash; applicable GST %</span>
                    </div>
                    <div class="form-group full">
                        <label>Description</label>
                        <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" MaxLength="500" placeholder="Optional notes about this material..." />
                    </div>
                </div>

                <div class="btn-row">
                    <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="btnClear_Click" CausesValidation="false" />
                    <asp:Button ID="btnToggleActive" runat="server" Text="Deactivate" CssClass="btn btn-danger" OnClick="btnToggleActive_Click" CausesValidation="false" Visible="false" />
                </div>
            <!-- SCRAP MATERIALS SECTION -->
            <asp:Panel ID="pnlScrap" runat="server" Visible="false">
            <div class="scrap-panel">
                <div class="scrap-title">&#9851; Scrap Materials Produced</div>
                <div class="scrap-add-row">
                    <asp:DropDownList ID="ddlScrap" runat="server"/>
                    <asp:Button ID="btnAddScrap" runat="server" Text="+ Add" CssClass="btn-add-scrap"
                        OnClick="btnAddScrap_Click" CausesValidation="false"/>
                </div>
                <asp:Panel ID="pnlScrapEmpty" runat="server">
                    <span class="scrap-empty">No scrap materials linked yet</span>
                </asp:Panel>
                <asp:Panel ID="pnlScrapTags" runat="server" Visible="false">
                    <asp:Repeater ID="rptScrap" runat="server" OnItemCommand="rptScrap_ItemCommand">
                        <ItemTemplate>
                            <span class="scrap-tag">
                                <%# Eval("ScrapName") %> <span style="color:#bbb;font-size:10px;"><%# Eval("Unit") %></span>
                                <asp:LinkButton runat="server" CommandName="Del"
                                    CommandArgument='<%# Eval("LinkID") %>'
                                    CssClass="del-scrap" CausesValidation="false">&#x2715;</asp:LinkButton>
                            </span>
                        </ItemTemplate>
                    </asp:Repeater>
                </asp:Panel>
            </div>
            </asp:Panel>

            <asp:Panel ID="pnlOpeningStock" runat="server" Visible="false" CssClass="os-panel">
                <div class="os-title">
                    Opening Stock &mdash; <asp:Label ID="lblOSMaterialName" runat="server" />
                </div>
                <div class="os-grid">
                    <div class="form-group">
                        <label>Opening Quantity <span class="req">*</span></label>
                        <asp:TextBox ID="txtOSQty" runat="server" MaxLength="12" placeholder="e.g. 500" />
                    </div>
                    <div class="form-group">
                        <label>Rate per Unit (₹)</label>
                        <asp:TextBox ID="txtOSRate" runat="server" MaxLength="12" placeholder="e.g. 45.50" />
                    </div>
                    <div class="form-group">
                        <label>As of Date <span class="req">*</span></label>
                        <asp:TextBox ID="txtOSDate" runat="server" MaxLength="10" placeholder="YYYY-MM-DD" />
                    </div>
                    <div class="form-group" style="grid-column:1/-1;">
                        <label>Remarks</label>
                        <asp:TextBox ID="txtOSRemarks" runat="server" MaxLength="300" placeholder="Optional notes..." />
                    </div>
                </div>
                <div style="display:flex; align-items:center; gap:20px; margin-top:14px;">
                    <asp:Button ID="btnSaveOS" runat="server" Text="Save Opening Stock"
                        CssClass="btn btn-primary" OnClick="btnSaveOS_Click" CausesValidation="false" />
                    <div class="os-value-box">
                        Stock Value: <strong><asp:Label ID="lblOSValue" runat="server" Text="₹ 0.00" /></strong>
                        <div class="os-last-saved"><asp:Label ID="lblOSLastSaved" runat="server" Text="Not yet recorded" /></div>
                    </div>
                </div>

                <!-- CONVERSION LOSS PRICING -->
                <div style="margin-top:18px;background:#fff8e1;border:1px solid #ffe082;border-radius:10px;padding:14px 16px;">
                    <div style="font-size:11px;font-weight:700;letter-spacing:.07em;text-transform:uppercase;color:#f57f17;margin-bottom:10px;">&#x1F504; Conversion Loss Pricing</div>
                    <div style="font-size:11px;color:#666;margin-bottom:10px;">If this RM's price is derived from another RM with a conversion loss (e.g. Roasted Black Sesame from Black Sesame), set the source RM and loss %.</div>
                    <div class="os-grid">
                        <div class="form-group">
                            <label>Source RM (Derived From)</label>
                            <asp:DropDownList ID="ddlDerivedFromRM" runat="server" />
                        </div>
                        <div class="form-group">
                            <label>Conversion Loss %</label>
                            <asp:TextBox ID="txtConversionLoss" runat="server" MaxLength="5" placeholder="e.g. 3.00" />
                        </div>
                    </div>
                    <asp:Button ID="btnSaveConversion" runat="server" Text="Save Conversion Settings"
                        CssClass="btn btn-primary" style="margin-top:8px;background:#f57f17;" 
                        OnClick="btnSaveConversion_Click" CausesValidation="false" />
                </div>
            </asp:Panel>
            </div>
        </div>

        <div class="list-card">
            <div class="list-header">
                <span class="list-title">Raw Materials</span>
                <asp:Label ID="lblCount" runat="server" CssClass="list-count" Text="0 records" />
            </div>
            <div class="list-search">
                <input type="text" placeholder="Search..." onkeyup="filterTable(this.value,'rmTable')" />
            </div>
            <div style="overflow-x:auto; max-height:600px; overflow-y:auto;">
                <asp:Repeater ID="rptMaterials" runat="server" OnItemCommand="rptMaterials_ItemCommand">
                    <HeaderTemplate>
                        <table class="rm-table" id="rmTable">
                        <thead><tr><th>Code</th><th>Name</th><th>UOM</th><th>Status</th><th></th></tr></thead><tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td style="font-weight:600;font-size:12px;color:var(--text-muted);"><%# Eval("RMCode") %></td>
                            <td>
                                <div style="font-weight:500;"><%# Eval("RMName") %></div>
                                <div style="font-size:11px;color:var(--text-dim);"><%# Eval("Description") %></div>
                            </td>
                            <td style="font-size:12px;"><%# Eval("Abbreviation") %></td>
                            <td><span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge-active" : "badge-inactive" %>'><%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %></span></td>
                            <td><asp:LinkButton runat="server" CommandName="Edit" CommandArgument='<%# Eval("RMID") %>' CssClass="act-link" CausesValidation="false">Edit</asp:LinkButton></td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate></tbody></table></FooterTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
                    <div class="empty-state">No raw materials found. Add your first material.</div>
                </asp:Panel>
            </div>
        </div>
    </div>
</form>
<script>
function filterTable(val, id) {
    val = val.toLowerCase();
    document.querySelectorAll('#' + id + ' tbody tr').forEach(function(r) {
        r.style.display = r.innerText.toLowerCase().includes(val) ? '' : 'none';
    });
}
</script>
<script src="/StockApp/erp-modal.js"></script>
<script src="/StockApp/erp-keepalive.js"></script>
</body>
</html>
