<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HREmployeeImport.aspx.cs" Inherits="HRModule.HREmployeeImport" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri &mdash; Import Employees</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--accent:#0d9488;--accent-dark:#0f766e;--accent-light:#ccfbf1;--teal:#0f6e56;--warn:#f39c12;--danger:#c0392b;--success:#0f6e56;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--radius:12px;}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);color:var(--text);min-height:100vh;}

nav{background:#1a1a1a;display:flex;align-items:center;padding:0 28px;height:52px;gap:6px;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;margin-right:10px;}
.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.1em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:14px;}
.nav-user{font-size:12px;color:#999;}
.nav-link{color:#fff;font-size:12px;font-weight:600;text-decoration:none;opacity:.8;}
.nav-link:hover{opacity:1;}
.nav-link.active{opacity:1;color:var(--accent-light);}

.page-header{background:var(--surface);border-bottom:2px solid var(--accent);padding:24px 40px;}
.page-icon{font-size:28px;margin-bottom:4px;}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:30px;letter-spacing:.06em;}
.page-title span{color:var(--accent);}
.page-sub{font-size:12px;color:var(--text-muted);margin-top:2px;}

.container{max-width:1500px;margin:0 auto;padding:22px 24px 60px;}
.card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:20px 22px;margin-bottom:18px;box-shadow:0 2px 6px rgba(0,0,0,.04);}

.btn{border:none;border-radius:8px;padding:10px 18px;font-size:13px;font-weight:600;cursor:pointer;font-family:inherit;text-decoration:none;display:inline-flex;align-items:center;gap:6px;}
.btn-primary{background:var(--accent);color:#fff;}
.btn-primary:hover{background:var(--accent-dark);}
.btn-success{background:#0f6e56;color:#fff;}
.btn-success:hover{background:#0c5a47;}
.btn-secondary{background:#5f6368;color:#fff;}
.btn-secondary:hover{background:#4a4d51;}
.btn-ghost{background:transparent;color:var(--text);border:1px solid var(--border);}
.btn-ghost:hover{background:#fafafa;}
.btn:disabled{opacity:.5;cursor:not-allowed;}

.banner{border-radius:8px;padding:12px 16px;font-size:13px;margin-bottom:16px;}
.banner-success{background:#e8f7f1;color:#0f6e56;border:1px solid #a7dbc7;}
.banner-error{background:#fdecea;color:#c0392b;border:1px solid #f5b7b1;}
.banner-info{background:#eef6fb;color:#2471a3;border:1px solid #aed6f1;}

.section-head{font-family:'Bebas Neue',sans-serif;font-size:13px;letter-spacing:.12em;color:var(--accent);margin-bottom:12px;padding-bottom:6px;border-bottom:1px solid var(--border);}

.upload-toolbar{display:flex;gap:14px;align-items:center;flex-wrap:wrap;}
.upload-toolbar input[type=file]{font-family:inherit;font-size:13px;padding:6px;border:1px solid var(--border);border-radius:6px;background:#fafafa;}
.upload-toolbar label{font-size:12px;color:var(--text-muted);display:flex;align-items:center;gap:6px;}

.expected-cols{margin-top:14px;padding:12px 16px;background:#f7fafa;border-radius:8px;border:1px solid var(--border);font-size:12px;color:var(--text-muted);line-height:1.6;}
.expected-cols b{color:var(--text);font-weight:500;}
.expected-cols code{background:#fff;border:1px solid var(--border);padding:1px 6px;border-radius:4px;font-family:'Courier New',monospace;font-size:11px;color:var(--accent-dark);}

.stats-row{display:flex;gap:10px;flex-wrap:wrap;margin-bottom:14px;}
.stat{padding:10px 16px;background:#fafafa;border:1px solid var(--border);border-radius:8px;font-size:12px;color:var(--text-muted);min-width:110px;}
.stat b{display:block;font-size:18px;font-weight:600;color:var(--text);margin-top:2px;}
.stat-ok b{color:#0f6e56;}
.stat-warn b{color:#b06000;}
.stat-err b{color:#c0392b;}

.preview-wrap{max-height:560px;overflow:auto;border:1px solid var(--border);border-radius:var(--radius);background:#fff;}

.tbl{width:100%;border-collapse:collapse;font-size:12px;background:var(--surface);}
.tbl th{background:#fafafa;font-size:10px;text-transform:uppercase;letter-spacing:.07em;color:var(--text-dim);padding:8px 10px;text-align:left;font-weight:600;border-bottom:1px solid var(--border);position:sticky;top:0;z-index:1;}
.tbl td{padding:7px 10px;border-bottom:1px solid #f0f0f0;white-space:nowrap;}
.tbl tr:hover td{background:#fafafa;}
.row-err  td{background:#fdecea;}
.row-warn td{background:#fef7e0;}
.col-num{text-align:right;font-variant-numeric:tabular-nums;}

.badge{font-size:9px;font-weight:600;padding:3px 8px;border-radius:9px;text-transform:uppercase;letter-spacing:.05em;display:inline-block;}
.badge-ready{background:#e8f7f1;color:#0f6e56;}
.badge-error{background:#fdecea;color:#c0392b;}

.actions-row{display:flex;gap:10px;margin-top:18px;}

/* Importing-overlay shown while the confirm postback is in flight, so a
   distracted user can't double-click and the page-back doesn't replay. */
#importingOverlay{display:none;position:fixed;inset:0;background:rgba(255,255,255,.65);z-index:9999;align-items:center;justify-content:center;font-family:'Bebas Neue',sans-serif;letter-spacing:.1em;font-size:18px;color:var(--accent-dark);}
#importingOverlay.on{display:flex;}
</style>
</head>
<body>
<form id="form1" runat="server" enctype="multipart/form-data">

<nav>
    <a class="nav-logo" href="HREmployee.aspx">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    </a>
    <span class="nav-title">HUMAN RESOURCES</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblNavUser" runat="server"/></span>
        <a href="HREmployee.aspx" class="nav-link">Employees</a>
        <a href="HRDepartment.aspx" class="nav-link">Departments</a>
        <a href="HREmployeeImport.aspx" class="nav-link active">Import</a>
        <a href="/StockApp/ERPHome.aspx" class="nav-link">&#8592; ERP Home</a>
        <a href="HRLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>

<div class="page-header">
    <div class="page-icon">&#x1F4E5;</div>
    <div class="page-title">EMPLOYEE <span>IMPORT</span></div>
    <div class="page-sub">Bulk-load employees from Excel. Preview before commit, fix issues, then import the ready rows.</div>
</div>

<div class="container">

    <asp:Panel ID="pnlMsg" runat="server" Visible="false" CssClass="banner banner-info"></asp:Panel>

    <!-- Step 1: Upload -->
    <div class="card">
        <div class="section-head">Step 1 &mdash; Upload Excel</div>
        <div class="upload-toolbar">
            <asp:FileUpload ID="fuFile" runat="server" />
            <asp:Button ID="btnUpload" runat="server" Text="Preview" CssClass="btn btn-primary"
                OnClick="btnUpload_Click"
                UseSubmitBehavior="false"
                OnClientClick="return hrLockUpload(this);" />
            <label><asp:CheckBox ID="chkAutoCreateDept" runat="server" Checked="true" /> Auto-create missing departments</label>
            <a href="HREmployeeImport.ashx?action=template" class="btn btn-ghost">&#x1F4C4; Template</a>
        </div>
        <div class="expected-cols">
            <b>Expected columns (header row required, order flexible, name aliases tolerated).</b><br/>
            <b>Identity:</b> <code>EmployeeCode</code> <code>FullName</code> <code>FatherName</code> <code>Gender</code> <code>DOB</code> <code>DOJ</code><br/>
            <b>Role:</b> <code>Department</code> <code>Designation</code> <code>EmploymentType</code><br/>
            <b>Organization:</b> <code>Reporting Manager</code> <code>State/Zone</code> <code>Region</code> <code>Area</code> <code>Location</code><br/>
            <b>Contact:</b> <code>Mobile</code> <code>Alt Mobile</code> <code>Email</code> <code>Address</code> <code>City</code> <code>State</code> <code>Pincode</code><br/>
            <b>KYC:</b> <code>Aadhar</code> <code>PAN</code> <code>UAN</code> <code>PF No</code> <code>ESI No</code><br/>
            <b>Bank:</b> <code>A/c No</code> <code>Bank Name</code> <code>IFSC</code><br/>
            <b>Salary:</b> <code>Basic</code> <code>HRA</code> <code>Conveyance</code> <code>Other</code><br/>
            Dates: <code>dd-mm-yyyy</code>, <code>dd/mm/yyyy</code>, <code>dd.mm.yyyy</code>, or Excel date.
            Leave EmployeeCode blank (or use <code>S.No</code>) to auto-generate <code>EMP###</code>.
        </div>
        <asp:HiddenField ID="hfFilePath" runat="server" />
    </div>

    <!-- Step 2: Preview -->
    <asp:Panel ID="pnlResults" runat="server" Visible="false">
        <div class="card">
            <div class="section-head">Step 2 &mdash; Preview &amp; Confirm</div>

            <div class="stats-row">
                <div class="stat">Total<b><asp:Literal ID="litTotal" runat="server" /></b></div>
                <div class="stat stat-ok">Ready<b><asp:Literal ID="litOK" runat="server" /></b></div>
                <div class="stat stat-warn">Warnings<b><asp:Literal ID="litWarn" runat="server" /></b></div>
                <div class="stat stat-err">Errors<b><asp:Literal ID="litErr" runat="server" /></b></div>
            </div>

            <div class="preview-wrap">
                <asp:GridView ID="gvPreview" runat="server" AutoGenerateColumns="false"
                              GridLines="None" CssClass="tbl" OnRowDataBound="gvPreview_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="RowNum" HeaderText="#" />
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='<%# Eval("Status").ToString() == "ERROR" ? "badge badge-error" : "badge badge-ready" %>'>
                                    <%# Eval("Status") %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Message" HeaderText="Issue" />
                        <asp:BoundField DataField="EmployeeCode" HeaderText="Code" />
                        <asp:BoundField DataField="FullName" HeaderText="Name" />
                        <asp:BoundField DataField="Department" HeaderText="Dept" />
                        <asp:BoundField DataField="Designation" HeaderText="Designation" />
                        <asp:BoundField DataField="ReportingManager" HeaderText="Mgr" />
                        <asp:BoundField DataField="Zone" HeaderText="Zone" />
                        <asp:BoundField DataField="Region" HeaderText="Region" />
                        <asp:BoundField DataField="Area" HeaderText="Area" />
                        <asp:BoundField DataField="WorkLocation" HeaderText="Location" />
                        <asp:BoundField DataField="EmploymentType" HeaderText="Type" />
                        <asp:BoundField DataField="DOJ" HeaderText="DOJ" DataFormatString="{0:dd-MMM-yy}" />
                        <asp:BoundField DataField="MobileNo" HeaderText="Mobile" />
                        <asp:BoundField DataField="AadhaarNo" HeaderText="Aadhaar" />
                        <asp:BoundField DataField="GrossSalary" HeaderText="Gross"
                                        DataFormatString="{0:N0}" ItemStyle-CssClass="col-num" />
                    </Columns>
                </asp:GridView>
            </div>

            <div class="actions-row">
                <asp:Button ID="btnConfirm" runat="server" Text="&#x2713; Import Ready Rows" CssClass="btn btn-success"
                    OnClick="btnConfirm_Click"
                    UseSubmitBehavior="false"
                    OnClientClick="return hrLockConfirm(this);" />
                <asp:Button ID="btnReset" runat="server" Text="Cancel" CssClass="btn btn-ghost"
                    OnClick="btnReset_Click" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>

</div>

<!-- Full-screen overlay shown during import postback so user can't interact -->
<div id="importingOverlay">Importing &mdash; please don&rsquo;t reload &hellip;</div>

</form>

<script>
// ============================================================================
// Double-click / refresh / back-button guard for the import buttons.
//
// The pattern is:
//   1. UseSubmitBehavior="false" on the asp:Button -> ASP.NET emits a regular
//      <input type=button> whose onclick runs __doPostBack(...). That means
//      we can disable the button after click and the postback still fires
//      (a true submit button would NOT submit if disabled).
//   2. OnClientClick returns true to allow __doPostBack, false to abort.
//   3. After the user confirms, we set a window flag so a second click in
//      a quick double-click is rejected even before the disabled state takes
//      effect.
//   4. The overlay covers the page so accidental Enter / clicks elsewhere
//      can't trigger anything during the in-flight postback.
//   5. The server side has an additional double-submit token so even if
//      somehow a second postback gets through (e.g. browser back-and-resubmit),
//      it will be rejected with a clear "already submitted" message.
// ============================================================================

window.__hrImportInFlight = false;

function hrLockUpload(btn) {
    if (window.__hrImportInFlight) return false;
    window.__hrImportInFlight = true;
    try {
        btn.value = 'Uploading...';
        btn.disabled = true;
    } catch (e) {}
    document.getElementById('importingOverlay').classList.add('on');
    document.getElementById('importingOverlay').textContent = 'Uploading & previewing — please wait …';
    return true;
}

function hrLockConfirm(btn) {
    if (window.__hrImportInFlight) return false;
    if (!confirm('Import the rows marked READY into HR_Employee?\n\nThis will run as a single transaction. Please do NOT reload or close this tab.')) {
        return false;
    }
    window.__hrImportInFlight = true;
    try {
        btn.value = 'Importing...';
        btn.disabled = true;
    } catch (e) {}
    // Disable the Cancel button too so a fast double-click can't fire it.
    var cancelBtn = document.getElementById('<%= btnReset.ClientID %>');
    if (cancelBtn) {
        try { cancelBtn.disabled = true; } catch (e) {}
    }
    document.getElementById('importingOverlay').textContent = 'Importing — please don\u2019t reload \u2026';
    document.getElementById('importingOverlay').classList.add('on');
    // Warn if user tries to navigate away while the import is in flight.
    window.onbeforeunload = function () {
        return 'Import is in progress. Leaving now may interrupt it.';
    };
    return true;
}

// Clear the in-flight flag when the page is fully loaded after a postback,
// so the next operation can proceed normally.
window.addEventListener('pageshow', function () {
    window.__hrImportInFlight = false;
    window.onbeforeunload = null;
    document.getElementById('importingOverlay').classList.remove('on');
});
</script>

<script src="/StockApp/erp-keepalive.js"></script>
</body>
</html>
