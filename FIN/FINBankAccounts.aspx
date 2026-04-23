<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FINBankAccounts.aspx.cs" Inherits="FINApp.FINBankAccounts" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Bank Accounts &mdash; FIN</title>
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
        body { background:var(--bg); color:var(--text); font-family:'DM Sans',sans-serif; min-height:100vh; }

        nav { background:#1a1a1a; display:flex; align-items:center; padding:0 28px; height:52px; gap:6px; position:sticky; top:0; z-index:100; }
        .nav-item { color:#aaa; text-decoration:none; font-size:12px; font-weight:600; letter-spacing:.06em; text-transform:uppercase; padding:6px 12px; border-radius:6px; }
        .nav-item:hover, .nav-item.active { color:#fff; background:rgba(255,255,255,0.08); }
        .nav-sep { color:#444; margin:0 4px; }
        .nav-right { margin-left:auto; display:flex; align-items:center; gap:12px; }
        .nav-user { font-size:12px; color:#888; }
        .nav-logout { font-size:11px; color:#666; text-decoration:none; padding:4px 10px; border:1px solid #333; border-radius:5px; }
        .nav-logout:hover { color:var(--accent); border-color:var(--accent); }

        .page-header { background:var(--surface); border-bottom:1px solid var(--border); padding:24px 40px; }
        .page-title { font-family:'Bebas Neue',sans-serif; font-size:28px; letter-spacing:.07em; }
        .page-title span { color:var(--accent); }
        .page-sub { font-size:12px; color:var(--text-muted); margin-top:2px; }

        .content { max-width:1300px; margin:32px auto; padding:0 32px; display:grid; grid-template-columns:1fr 480px; gap:24px; align-items:start; }

        .card { background:var(--surface); border:1px solid var(--border); border-radius:var(--radius); padding:28px; }
        .card-title { font-family:'Bebas Neue',sans-serif; font-size:18px; letter-spacing:.07em; margin-bottom:20px; padding-bottom:12px; border-bottom:2px solid var(--accent); display:flex; align-items:center; gap:10px; }

        .form-grid { display:grid; grid-template-columns:1fr 1fr; gap:16px; }
        .form-group { display:flex; flex-direction:column; gap:6px; }
        .form-group.full { grid-column:1 / -1; }
        label { font-size:11px; font-weight:700; letter-spacing:.07em; text-transform:uppercase; color:var(--text-muted); }
        label .req { color:var(--danger); }
        input[type=text], input[type=number], select, textarea {
            width:100%; padding:10px 14px; border:1.5px solid var(--border); border-radius:8px;
            font-family:'DM Sans',sans-serif; font-size:14px; background:#fafafa; outline:none;
        }
        input:focus, select:focus, textarea:focus { border-color:var(--accent); background:#fff; }
        .field-hint { font-size:11px; color:var(--text-dim); }

        .form-section { margin:22px 0 14px; font-size:11px; font-weight:700; letter-spacing:.12em; text-transform:uppercase; color:var(--text-dim); display:flex; align-items:center; gap:10px; }
        .form-section::after { content:''; flex:1; height:1px; background:var(--border); }

        .btn-row { display:flex; gap:10px; margin-top:22px; }
        .btn { padding:11px 22px; border-radius:8px; font-size:13px; font-weight:700; letter-spacing:.04em; cursor:pointer; border:none; font-family:inherit; }
        .btn-primary { background:var(--accent); color:#fff; }
        .btn-primary:hover { background:var(--accent-dark); }
        .btn-secondary { background:transparent; border:1.5px solid var(--border); color:var(--text-muted); }
        .btn-secondary:hover { border-color:var(--text-muted); color:var(--text); }
        .btn-danger { background:transparent; border:1.5px solid #ffcccc; color:var(--danger); }

        .alert { padding:12px 16px; border-radius:8px; font-size:13px; margin-bottom:18px; }
        .alert-success { background:rgba(26,158,106,0.10); color:var(--success); border:1px solid rgba(26,158,106,0.25); }
        .alert-danger  { background:rgba(192,57,43,0.08); color:var(--danger);  border:1px solid rgba(192,57,43,0.2); }

        .list-card { background:var(--surface); border:1px solid var(--border); border-radius:var(--radius); overflow:hidden; }
        .list-header { padding:16px 20px; border-bottom:1px solid var(--border); display:flex; align-items:center; justify-content:space-between; }
        .list-title { font-family:'Bebas Neue',sans-serif; font-size:18px; letter-spacing:.07em; }
        .list-count { font-size:11px; color:var(--text-muted); background:var(--bg); padding:3px 10px; border-radius:20px; }

        .bank-table { width:100%; border-collapse:collapse; }
        .bank-table th { padding:10px 14px; text-align:left; font-size:10px; font-weight:700; letter-spacing:.1em; text-transform:uppercase; color:var(--text-dim); background:#fafafa; border-bottom:1px solid var(--border); }
        .bank-table td { padding:12px 14px; font-size:13px; border-bottom:1px solid #f0f0f0; }
        .code-cell { font-family:'Roboto Mono',monospace; font-size:12px; color:var(--text-muted); font-weight:600; }
        .badge-active   { display:inline-block; padding:2px 8px; border-radius:20px; font-size:10px; font-weight:700; background:rgba(26,158,106,0.12); color:var(--success); }
        .badge-inactive { display:inline-block; padding:2px 8px; border-radius:20px; font-size:10px; font-weight:700; background:rgba(0,0,0,0.06); color:var(--text-dim); }
        .badge-layout   { display:inline-block; padding:2px 8px; border-radius:20px; font-size:10px; font-weight:700; }
        .badge-layout.yes { background:rgba(17,122,101,0.12); color:var(--accent); }
        .badge-layout.no  { background:rgba(192,57,43,0.08); color:var(--danger); }
        .act-link { font-size:12px; color:var(--accent); cursor:pointer; font-weight:600; background:none; border:0; font-family:inherit; padding:0; text-decoration:none; }
        .act-link:hover { text-decoration:underline; }
        .act-link + .act-link { margin-left:10px; }

        .layout-section { margin-top:30px; padding-top:26px; border-top:2px solid var(--accent-light); display:none; }
        .layout-section.shown { display:block; }
        .layout-intro { background:var(--accent-light); padding:12px 16px; border-radius:8px; font-size:12px; color:var(--accent-dark); margin-bottom:18px; }
        .amt-modes { display:flex; gap:10px; margin-bottom:14px; }
        .amt-modes label { display:flex; align-items:center; gap:6px; padding:8px 12px; border:1.5px solid var(--border); border-radius:8px; cursor:pointer; font-size:12px; text-transform:none; letter-spacing:0; color:var(--text); font-weight:500; }
        .amt-modes input[type=radio]:checked + span { font-weight:700; color:var(--accent); }
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
        <span class="nav-item active">Bank Accounts</span>
        <div class="nav-right">
            <span class="nav-user"><asp:Label ID="lblNavUser" runat="server" /></span>
            <a href="FINLogout.aspx" class="nav-logout">Sign Out</a>
        </div>
    </nav>

    <div class="page-header">
        <div class="page-title">Bank <span>Accounts</span></div>
        <div class="page-sub">Define bank accounts used for postings &mdash; each bank also has an XLSX column layout used when uploading statements.</div>
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
                    <asp:Label ID="lblFormTitle" runat="server" Text="New Bank Account" />
                    <asp:HiddenField ID="hfBankID" runat="server" Value="0" />
                </div>

                <div class="form-section">Bank Details</div>
                <div class="form-grid">
                    <div class="form-group">
                        <label>Code</label>
                        <asp:TextBox ID="txtCode" runat="server" ReadOnly="true" style="background:#f0f0f0;color:var(--text-muted);cursor:not-allowed;" placeholder="Auto (BNK-001)" />
                    </div>
                    <div class="form-group">
                        <label>Bank Name <span class="req">*</span></label>
                        <asp:TextBox ID="txtName" runat="server" MaxLength="150" placeholder="e.g. HDFC Current Account" />
                    </div>
                    <div class="form-group">
                        <label>Account Number</label>
                        <asp:TextBox ID="txtAcctNo" runat="server" MaxLength="50" />
                    </div>
                    <div class="form-group">
                        <label>Branch</label>
                        <asp:TextBox ID="txtBranch" runat="server" MaxLength="100" />
                    </div>
                </div>

                <div class="form-section">Zoho Books Ledger</div>
                <div class="form-grid">
                    <div class="form-group full">
                        <label>Zoho Account (Chart of Accounts)</label>
                        <asp:DropDownList ID="ddlZohoAccount" runat="server" />
                        <span class="field-hint">Pick the bank's ledger in Zoho Books. Postings from this bank will be Dr/Cr against this account.</span>
                    </div>
                </div>

                <div class="btn-row">
                    <asp:Button ID="btnSave" runat="server" Text="Save Bank" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="btnClear_Click" CausesValidation="false" />
                    <asp:Button ID="btnToggleActive" runat="server" Text="Deactivate" CssClass="btn btn-danger" OnClick="btnToggleActive_Click" CausesValidation="false" Visible="false" />
                </div>

                <!-- XLSX Layout (shown only on edit) -->
                <asp:Panel ID="pnlLayout" runat="server" Visible="false" CssClass="layout-section shown">

                    <div class="layout-intro">
                        <b>Two layouts per bank &mdash; one for XLSX, one for PDF.</b>
                        Each statement format has different columns, so configure each independently.
                        Use column letters (<b>A, B, C</b>…) and 1-based row numbers.
                        <br/><br/>
                        <b>File format notes:</b>
                        <ul style="margin:6px 0 0 18px;padding:0;">
                            <li><b>XLSX</b> &mdash; column letters refer to the actual columns in your spreadsheet.</li>
                            <li><b>XLS (older Excel)</b> &mdash; not supported directly. Open in Excel, <i>Save As &rarr; Excel Workbook (.xlsx)</i>, then upload.</li>
                            <li><b>PDF</b> &mdash; the parser converts the PDF into a grid and auto-numbers columns left-to-right. Column <b>A</b> = leftmost text block, <b>B</b> = next, and so on. Header / first-data row are usually 1 / 2 for PDFs.</li>
                        </ul>
                    </div>

                    <!-- ═════════════════ XLSX LAYOUT ═════════════════ -->
                    <div class="form-section" style="color:var(--accent);">XLSX Column Layout</div>

                    <div class="form-grid">
                        <div class="form-group">
                            <label>Header Row</label>
                            <asp:TextBox ID="txtHeaderRow" runat="server" MaxLength="4" placeholder="1" />
                            <span class="field-hint">Row with column titles (e.g. "Date", "Description")</span>
                        </div>
                        <div class="form-group">
                            <label>First Data Row</label>
                            <asp:TextBox ID="txtFirstData" runat="server" MaxLength="4" placeholder="2" />
                        </div>
                        <div class="form-group">
                            <label>Date Column</label>
                            <asp:TextBox ID="txtDateCol" runat="server" MaxLength="3" placeholder="A" />
                        </div>
                        <div class="form-group">
                            <label>Date Format</label>
                            <asp:TextBox ID="txtDateFormat" runat="server" MaxLength="30" placeholder="dd/MM/yyyy" />
                            <span class="field-hint">e.g. dd/MM/yyyy, dd-MMM-yyyy, yyyy-MM-dd</span>
                        </div>
                        <div class="form-group">
                            <label>Description Column</label>
                            <asp:TextBox ID="txtDescCol" runat="server" MaxLength="3" placeholder="B" />
                        </div>
                        <div class="form-group">
                            <label>Reference Column</label>
                            <asp:TextBox ID="txtRefCol" runat="server" MaxLength="3" placeholder="C (optional)" />
                        </div>
                    </div>

                    <div class="form-section" style="margin-top:12px;">XLSX Amount Structure</div>
                    <div class="amt-modes">
                        <label>
                            <asp:RadioButton ID="rbModeTwoCol" runat="server" GroupName="amtMode" />
                            <span>Two columns (Debit &amp; Credit)</span>
                        </label>
                        <label>
                            <asp:RadioButton ID="rbModeFlag" runat="server" GroupName="amtMode" />
                            <span>Amount + DR/CR flag</span>
                        </label>
                        <label>
                            <asp:RadioButton ID="rbModeSigned" runat="server" GroupName="amtMode" />
                            <span>Signed amount (&minus; = debit)</span>
                        </label>
                    </div>

                    <div class="form-grid">
                        <div class="form-group">
                            <label>Debit Column</label>
                            <asp:TextBox ID="txtDebitCol" runat="server" MaxLength="3" placeholder="D" />
                            <span class="field-hint">Used in TWO_COL mode</span>
                        </div>
                        <div class="form-group">
                            <label>Credit Column</label>
                            <asp:TextBox ID="txtCreditCol" runat="server" MaxLength="3" placeholder="E" />
                            <span class="field-hint">Used in TWO_COL mode</span>
                        </div>
                        <div class="form-group">
                            <label>Amount Column</label>
                            <asp:TextBox ID="txtAmountCol" runat="server" MaxLength="3" placeholder="D" />
                            <span class="field-hint">Used in FLAG or SIGNED modes</span>
                        </div>
                        <div class="form-group">
                            <label>Flag Column (DR/CR)</label>
                            <asp:TextBox ID="txtFlagCol" runat="server" MaxLength="3" placeholder="E" />
                            <span class="field-hint">Used in FLAG mode only</span>
                        </div>
                        <div class="form-group">
                            <label>Balance Column</label>
                            <asp:TextBox ID="txtBalanceCol" runat="server" MaxLength="3" placeholder="F" />
                        </div>
                    </div>

                    <div class="btn-row">
                        <asp:Button ID="btnSaveLayoutXlsx" runat="server" Text="Save XLSX Layout" CssClass="btn btn-primary" OnClick="btnSaveLayoutXlsx_Click" CausesValidation="false" />
                    </div>

                    <!-- ═════════════════ PDF LAYOUT ═════════════════ -->
                    <div class="form-section" style="color:var(--accent);margin-top:32px;">PDF Column Layout</div>
                    <div class="layout-intro" style="background:#fef5ef;color:#a04000;border:1px solid rgba(204,100,50,0.2);">
                        The PDF parser auto-discovers columns left-to-right, then your layout mapping applies.
                        Upload a sample PDF first (the detail view will show you what columns landed where),
                        then come back here and set the letters accordingly.
                    </div>

                    <div class="form-grid">
                        <div class="form-group">
                            <label>Header Row</label>
                            <asp:TextBox ID="txtPdfHeaderRow" runat="server" MaxLength="4" placeholder="1" />
                        </div>
                        <div class="form-group">
                            <label>First Data Row</label>
                            <asp:TextBox ID="txtPdfFirstData" runat="server" MaxLength="4" placeholder="2" />
                        </div>
                        <div class="form-group">
                            <label>Date Column</label>
                            <asp:TextBox ID="txtPdfDateCol" runat="server" MaxLength="3" placeholder="A" />
                        </div>
                        <div class="form-group">
                            <label>Date Format</label>
                            <asp:TextBox ID="txtPdfDateFormat" runat="server" MaxLength="30" placeholder="dd/MM/yyyy" />
                            <span class="field-hint">Date pattern in the PDF (e.g. dd/MM/yyyy)</span>
                        </div>
                        <div class="form-group">
                            <label>Description Column</label>
                            <asp:TextBox ID="txtPdfDescCol" runat="server" MaxLength="3" placeholder="B" />
                        </div>
                        <div class="form-group">
                            <label>Reference Column</label>
                            <asp:TextBox ID="txtPdfRefCol" runat="server" MaxLength="3" placeholder="C (optional)" />
                        </div>
                    </div>

                    <div class="form-section" style="margin-top:12px;">PDF Amount Structure</div>
                    <div class="amt-modes">
                        <label>
                            <asp:RadioButton ID="rbPdfModeTwoCol" runat="server" GroupName="pdfAmtMode" />
                            <span>Two columns (Debit &amp; Credit)</span>
                        </label>
                        <label>
                            <asp:RadioButton ID="rbPdfModeFlag" runat="server" GroupName="pdfAmtMode" />
                            <span>Amount + DR/CR flag</span>
                        </label>
                        <label>
                            <asp:RadioButton ID="rbPdfModeSigned" runat="server" GroupName="pdfAmtMode" />
                            <span>Signed amount (&minus; = debit)</span>
                        </label>
                    </div>

                    <div class="form-grid">
                        <div class="form-group">
                            <label>Debit Column</label>
                            <asp:TextBox ID="txtPdfDebitCol" runat="server" MaxLength="3" placeholder="D" />
                        </div>
                        <div class="form-group">
                            <label>Credit Column</label>
                            <asp:TextBox ID="txtPdfCreditCol" runat="server" MaxLength="3" placeholder="E" />
                        </div>
                        <div class="form-group">
                            <label>Amount Column</label>
                            <asp:TextBox ID="txtPdfAmountCol" runat="server" MaxLength="3" placeholder="D" />
                        </div>
                        <div class="form-group">
                            <label>Flag Column (DR/CR)</label>
                            <asp:TextBox ID="txtPdfFlagCol" runat="server" MaxLength="3" placeholder="E" />
                        </div>
                        <div class="form-group">
                            <label>Balance Column</label>
                            <asp:TextBox ID="txtPdfBalanceCol" runat="server" MaxLength="3" placeholder="F" />
                        </div>
                    </div>

                    <div class="btn-row">
                        <asp:Button ID="btnSaveLayoutPdf" runat="server" Text="Save PDF Layout" CssClass="btn btn-primary" OnClick="btnSaveLayoutPdf_Click" CausesValidation="false" />
                    </div>

                    <!-- ═════════════════ SHARED SIGNATURE ═════════════════ -->
                    <div class="form-section" style="margin-top:32px;">Auto-Detection Signature</div>
                    <div class="layout-intro" style="background:#fff8e1;color:#8a6d00;border:1px solid rgba(220,180,0,0.2);">
                        Shared between XLSX and PDF. When a user uploads a statement without picking a bank, the system scans the top rows for a text that uniquely identifies this bank. Keep it short and distinctive &mdash; e.g. <b>HDFC BANK</b>, <b>ICICI Bank Ltd</b>, <b>Kotak Mahindra</b>.
                    </div>
                    <div class="form-grid">
                        <div class="form-group full">
                            <label>Signature Text</label>
                            <asp:TextBox ID="txtSignatureText" runat="server" MaxLength="200" placeholder="e.g. HDFC BANK" />
                            <span class="field-hint">Case-insensitive substring match. Leave blank to skip auto-detect for this bank.</span>
                        </div>
                        <div class="form-group">
                            <label>Scan Top N Rows</label>
                            <asp:TextBox ID="txtSignatureRows" runat="server" MaxLength="3" placeholder="15" />
                            <span class="field-hint">How many rows from the top to search</span>
                        </div>
                    </div>

                    <div class="btn-row">
                        <asp:Button ID="btnSaveSignature" runat="server" Text="Save Signature" CssClass="btn btn-secondary" OnClick="btnSaveSignature_Click" CausesValidation="false" />
                    </div>

                </asp:Panel>

            </div>
        </div>

        <!-- RIGHT: LIST -->
        <div class="list-card">
            <div class="list-header">
                <span class="list-title">Banks</span>
                <asp:Label ID="lblCount" runat="server" CssClass="list-count" Text="0 banks" />
            </div>
            <div style="overflow-x:auto;max-height:700px;overflow-y:auto;">
                <asp:Repeater ID="rptBanks" runat="server" OnItemCommand="rptBanks_ItemCommand">
                    <HeaderTemplate>
                        <table class="bank-table"><thead><tr>
                            <th>Code</th>
                            <th>Bank</th>
                            <th>Layout</th>
                            <th>Status</th>
                            <th>Action</th>
                        </tr></thead><tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td class="code-cell"><%# Eval("BankCode") %></td>
                            <td>
                                <div style="font-weight:500;"><%# Eval("BankName") %></div>
                                <div style="font-size:11px;color:var(--text-dim);margin-top:2px;"><%# Eval("AccountNumber") %></div>
                            </td>
                            <td><%# RenderLayoutBadge(Eval("BankID")) %></td>
                            <td>
                                <span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge-active" : "badge-inactive" %>'>
                                    <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
                                </span>
                            </td>
                            <td>
                                <asp:LinkButton ID="lbEdit" runat="server" CommandName="Edit" CommandArgument='<%# Eval("BankID") %>' CssClass="act-link" CausesValidation="false">Edit</asp:LinkButton>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate></tbody></table></FooterTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
                    <div style="text-align:center;padding:40px 20px;color:var(--text-dim);font-size:13px;">No bank accounts yet. Add your first one on the left.</div>
                </asp:Panel>
            </div>
        </div>

    </div>

</form>
</body>
</html>
