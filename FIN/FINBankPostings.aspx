<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FINBankPostings.aspx.cs" Inherits="FINApp.FINBankPostings" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Bank Postings &mdash; FIN</title>
    <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&family=Roboto+Mono:wght@400;600&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
    <style>
        :root {
            --bg:#f0f0f0; --surface:#fff; --border:#e0e0e0;
            --accent:#117a65; --accent-dark:#0e6252; --accent-light:#e8f6f3;
            --text:#1a1a1a; --text-muted:#666; --text-dim:#999;
            --success:#1a9e6a; --danger:#c0392b; --radius:12px;
        }
        *, *::before, *::after { box-sizing:border-box; margin:0; padding:0; }
        body { background:var(--bg); color:var(--text); font-family:'DM Sans',sans-serif; }

        nav { background:#1a1a1a; display:flex; align-items:center; padding:0 28px; height:52px; gap:6px; position:sticky; top:0; z-index:100; }
        .nav-item { color:#aaa; text-decoration:none; font-size:12px; font-weight:600; letter-spacing:.06em; text-transform:uppercase; padding:6px 12px; border-radius:6px; }
        .nav-item:hover, .nav-item.active { color:#fff; background:rgba(255,255,255,0.08); }
        .nav-sep { color:#444; margin:0 4px; }
        .nav-right { margin-left:auto; display:flex; align-items:center; gap:12px; }
        .nav-user { font-size:12px; color:#888; }
        .nav-logout { font-size:11px; color:#666; text-decoration:none; padding:4px 10px; border:1px solid #333; border-radius:5px; }
        .nav-logout:hover { color:var(--accent); border-color:var(--accent); }

        .page-header { background:var(--surface); border-bottom:1px solid var(--border); padding:24px 40px; display:flex; justify-content:space-between; align-items:center; }
        .page-title { font-family:'Bebas Neue',sans-serif; font-size:28px; letter-spacing:.07em; }
        .page-title span { color:var(--accent); }
        .page-sub { font-size:12px; color:var(--text-muted); margin-top:2px; }
        .manage-btn { font-size:12px; font-weight:600; color:var(--accent); text-decoration:none; padding:8px 14px; border:1.5px solid var(--accent-light); border-radius:6px; letter-spacing:.03em; }
        .manage-btn:hover { background:var(--accent-light); }

        .content { max-width:1300px; margin:32px auto; padding:0 32px; }

        .card { background:var(--surface); border:1px solid var(--border); border-radius:var(--radius); padding:24px; margin-bottom:24px; }
        .card-title { font-family:'Bebas Neue',sans-serif; font-size:18px; letter-spacing:.07em; margin-bottom:18px; padding-bottom:10px; border-bottom:2px solid var(--accent); }

        .upload-grid { display:grid; grid-template-columns:280px 1fr auto; gap:14px; align-items:end; }
        .form-group { display:flex; flex-direction:column; gap:6px; }
        label { font-size:11px; font-weight:700; letter-spacing:.07em; text-transform:uppercase; color:var(--text-muted); }
        select, input[type=file] { width:100%; padding:10px 14px; border:1.5px solid var(--border); border-radius:8px; background:#fafafa; font-size:14px; font-family:inherit; outline:none; }
        select:focus, input[type=file]:focus { border-color:var(--accent); background:#fff; }
        .btn { padding:11px 22px; border-radius:8px; font-size:13px; font-weight:700; letter-spacing:.04em; cursor:pointer; border:none; font-family:inherit; }
        .btn-primary { background:var(--accent); color:#fff; }
        .btn-primary:hover { background:var(--accent-dark); }

        .alert { padding:12px 16px; border-radius:8px; font-size:13px; margin-bottom:18px; }
        .alert-success { background:rgba(26,158,106,0.10); color:var(--success); border:1px solid rgba(26,158,106,0.25); }
        .alert-info    { background:rgba(17,122,101,0.08); color:var(--accent); border:1px solid rgba(17,122,101,0.2); }
        .alert-danger  { background:rgba(192,57,43,0.08); color:var(--danger); border:1px solid rgba(192,57,43,0.2); }

        .stmt-table { width:100%; border-collapse:collapse; }
        .stmt-table th { padding:10px 14px; text-align:left; font-size:10px; font-weight:700; letter-spacing:.1em; text-transform:uppercase; color:var(--text-dim); background:#fafafa; border-bottom:1px solid var(--border); }
        .stmt-table td { padding:12px 14px; font-size:13px; border-bottom:1px solid #f0f0f0; vertical-align:middle; }
        .stmt-table tr:hover td { background:#fafafa; }
        .num { font-family:'Roboto Mono',monospace; text-align:right; font-size:12px; }
        .code-cell { font-family:'Roboto Mono',monospace; font-size:12px; color:var(--text-muted); font-weight:600; }

        .line-table { width:100%; border-collapse:collapse; font-size:12px; }
        .line-table th { padding:8px 10px; text-align:left; font-size:10px; font-weight:700; letter-spacing:.08em; text-transform:uppercase; color:var(--text-dim); background:#fafafa; border-bottom:1px solid var(--border); position:sticky; top:0; }
        .line-table td { padding:8px 10px; border-bottom:1px solid #f5f5f5; }
        .line-table .num { font-family:'Roboto Mono',monospace; text-align:right; }
        .badge-unposted { display:inline-block; padding:2px 8px; border-radius:20px; font-size:10px; font-weight:700; background:#f0f0f0; color:#666; }
        .badge-posted   { display:inline-block; padding:2px 8px; border-radius:20px; font-size:10px; font-weight:700; background:rgba(26,158,106,0.12); color:var(--success); }
        .badge-skipped  { display:inline-block; padding:2px 8px; border-radius:20px; font-size:10px; font-weight:700; background:rgba(192,57,43,0.08); color:var(--danger); }

        .stat-row { display:flex; gap:24px; margin-bottom:18px; padding:16px; background:var(--accent-light); border-radius:8px; }
        .stat { flex:1; }
        .stat-label { font-size:10px; font-weight:700; letter-spacing:.1em; text-transform:uppercase; color:var(--accent-dark); }
        .stat-value { font-size:18px; font-weight:600; color:var(--text); margin-top:4px; font-family:'Roboto Mono',monospace; }

        .drill-link { color:var(--accent); text-decoration:none; font-weight:600; font-size:12px; }
        .drill-link:hover { text-decoration:underline; }
        .empty-state { text-align:center; padding:40px 20px; color:var(--text-dim); font-size:13px; }
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
        <span class="nav-item active">Bank Postings</span>
        <div class="nav-right">
            <span class="nav-user"><asp:Label ID="lblNavUser" runat="server" /></span>
            <a href="FINLogout.aspx" class="nav-logout">Sign Out</a>
        </div>
    </nav>

    <div class="page-header">
        <div>
            <div class="page-title">Bank <span>Postings</span></div>
            <div class="page-sub">Upload bank statements and post each transaction as a journal &mdash; payments made, receipts received, all bank-side entries.</div>
        </div>
        <a href="FINBankAccounts.aspx" class="manage-btn">Manage Bank Accounts &rarr;</a>
    </div>

    <div class="content">

        <asp:Panel ID="pnlAlert" runat="server" Visible="false">
            <div class="alert" id="alertBox" runat="server">
                <asp:Label ID="lblAlert" runat="server" />
            </div>
        </asp:Panel>

        <!-- ═════════════════ UPLOAD VIEW (default) ═════════════════ -->
        <asp:Panel ID="pnlListView" runat="server">

            <!-- UPLOAD CARD -->
            <div class="card">
                <div class="card-title">Upload Statement</div>
                <div style="background:var(--accent-light);padding:10px 14px;border-radius:8px;font-size:12px;color:var(--accent-dark);margin-bottom:14px;">
                    <b>Auto-detect:</b> Pick an XLSX or PDF. The system scans the file for each configured bank's signature text and matches automatically.
                    <br/><br/>
                    <b>Notes:</b>
                    <ul style="margin:6px 0 0 18px;padding:0;font-size:12px;">
                        <li>Older <b>.xls</b> (Excel 97-2003) is not supported &mdash; open in Excel and <i>Save As &rarr; .xlsx</i> first.</li>
                        <li><b>Scanned PDFs</b> are not supported (no text to extract). Use the native-text PDF your net-banking exports.</li>
                    </ul>
                </div>
                <div class="upload-grid" style="grid-template-columns:1fr auto;">
                    <div class="form-group">
                        <label>Statement File (XLSX or PDF)</label>
                        <asp:FileUpload ID="fuStatement" runat="server" accept=".xlsx,.pdf" />
                    </div>
                    <div>
                        <asp:Button ID="btnUpload" runat="server" Text="Upload &amp; Parse" CssClass="btn btn-primary" OnClick="btnUpload_Click" />
                    </div>
                </div>

                <!-- Fallback: shown only when auto-detect fails or user cleared selection -->
                <asp:Panel ID="pnlManualBank" runat="server" Visible="false" style="margin-top:16px;padding-top:14px;border-top:1px dashed var(--border);">
                    <div class="alert alert-danger" style="margin-bottom:12px;">
                        Couldn't identify the bank automatically. Pick one below and click <b>Upload &amp; Parse</b> again.
                    </div>
                    <div class="form-group" style="max-width:400px;">
                        <label>Bank (manual override)</label>
                        <asp:DropDownList ID="ddlBank" runat="server" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlNoLayout" runat="server" Visible="false">
                    <div class="alert alert-danger" style="margin-top:14px;">
                        The matched bank has no XLSX layout configured yet.
                        <a href="FINBankAccounts.aspx" style="color:inherit;font-weight:700;">Configure it here</a> before uploading.
                    </div>
                </asp:Panel>
            </div>

            <!-- STATEMENTS LIST -->
            <div class="card">
                <div class="card-title">Uploaded Statements</div>
                <div style="overflow-x:auto;">
                    <asp:Repeater ID="rptStatements" runat="server" OnItemCommand="rptStatements_ItemCommand">
                        <HeaderTemplate>
                            <table class="stmt-table"><thead><tr>
                                <th>Bank</th>
                                <th>File</th>
                                <th>Period</th>
                                <th class="num">Opening</th>
                                <th class="num">Closing</th>
                                <th class="num">Rows</th>
                                <th>Uploaded</th>
                                <th></th>
                            </tr></thead><tbody>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td>
                                    <div class="code-cell"><%# Eval("BankCode") %></div>
                                    <div><%# Eval("BankName") %></div>
                                </td>
                                <td style="font-size:12px;color:var(--text-muted);"><%# Eval("FileName") %></td>
                                <td style="font-size:12px;">
                                    <%# FormatPeriod(Eval("PeriodStart"), Eval("PeriodEnd")) %>
                                </td>
                                <td class="num"><%# FormatMoney(Eval("OpeningBalance")) %></td>
                                <td class="num"><%# FormatMoney(Eval("ClosingBalance")) %></td>
                                <td class="num">
                                    <%# Eval("RowCount") %>
                                    <%# Convert.ToInt32(Eval("DuplicateCount")) > 0 ? " <span style='color:var(--danger);font-size:10px;'>(" + Eval("DuplicateCount") + " dup)</span>" : "" %>
                                </td>
                                <td style="font-size:11px;color:var(--text-dim);">
                                    <%# FormatDateTime(Eval("UploadedAt")) %><br/>
                                    <%# Eval("UploadedByName") %>
                                </td>
                                <td>
                                    <asp:LinkButton ID="lbView" runat="server" CommandName="View" CommandArgument='<%# Eval("StatementID") %>' CssClass="drill-link" CausesValidation="false">View rows &rarr;</asp:LinkButton>
                                </td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate></tbody></table></FooterTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
                        <div class="empty-state">No statements uploaded yet.</div>
                    </asp:Panel>
                </div>
            </div>
        </asp:Panel>

        <!-- ═════════════════ DETAIL VIEW (drill-down) ═════════════════ -->
        <asp:Panel ID="pnlDetailView" runat="server" Visible="false">

            <div class="card">
                <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:12px;">
                    <div>
                        <div class="card-title" style="border:none;padding:0;margin:0;">
                            <asp:Label ID="lblDetailHeader" runat="server" />
                        </div>
                    </div>
                    <asp:LinkButton ID="btnBack" runat="server" Text="&larr; Back to list" CssClass="drill-link" OnClick="btnBack_Click" CausesValidation="false" />
                </div>

                <div class="stat-row">
                    <div class="stat">
                        <div class="stat-label">Period</div>
                        <div class="stat-value" style="font-size:14px;"><asp:Label ID="lblStatPeriod" runat="server" /></div>
                    </div>
                    <div class="stat">
                        <div class="stat-label">Opening</div>
                        <div class="stat-value"><asp:Label ID="lblStatOpen" runat="server" /></div>
                    </div>
                    <div class="stat">
                        <div class="stat-label">Closing</div>
                        <div class="stat-value"><asp:Label ID="lblStatClose" runat="server" /></div>
                    </div>
                    <div class="stat">
                        <div class="stat-label">Rows</div>
                        <div class="stat-value"><asp:Label ID="lblStatRows" runat="server" /></div>
                    </div>
                </div>

                <div style="max-height:600px;overflow-y:auto;">
                    <asp:Repeater ID="rptLines" runat="server">
                        <HeaderTemplate>
                            <table class="line-table"><thead><tr>
                                <th>#</th>
                                <th>Date</th>
                                <th>Description</th>
                                <th>Reference</th>
                                <th class="num">Debit</th>
                                <th class="num">Credit</th>
                                <th class="num">Balance</th>
                                <th>Status</th>
                            </tr></thead><tbody>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td style="color:var(--text-dim);"><%# Eval("RowSeq") %></td>
                                <td><%# FormatDate(Eval("TxnDate")) %></td>
                                <td style="max-width:360px;"><%# Eval("Description") %></td>
                                <td style="font-family:'Roboto Mono',monospace;font-size:11px;color:var(--text-muted);"><%# Eval("Reference") %></td>
                                <td class="num"><%# FormatMoney(Eval("Debit")) %></td>
                                <td class="num"><%# FormatMoney(Eval("Credit")) %></td>
                                <td class="num"><%# FormatMoney(Eval("Balance")) %></td>
                                <td><span class='<%# "badge-" + Eval("Status").ToString().ToLower() %>'><%# Eval("Status") %></span></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate></tbody></table></FooterTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </asp:Panel>

    </div>

</form>
</body>
</html>
