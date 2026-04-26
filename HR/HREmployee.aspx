<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HREmployee.aspx.cs" Inherits="HRModule.HREmployee" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri &mdash; Employees</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--accent:#0d9488;--accent-dark:#0f766e;--accent-light:#ccfbf1;--teal:#0f6e56;--warn:#f39c12;--danger:#c0392b;--success:#0f6e56;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--radius:12px;}
*{box-sizing:border-box;margin:0;padding:0;}
body{font-family:'DM Sans',sans-serif;background:var(--bg);color:var(--text);min-height:100vh;}

/* ---------- Top nav (matches FIN) ---------- */
nav{background:#1a1a1a;display:flex;align-items:center;padding:0 28px;height:52px;gap:6px;}
.nav-logo{background:#fff;border-radius:6px;padding:3px 8px;display:flex;align-items:center;margin-right:10px;}
.nav-logo img{height:26px;object-fit:contain;}
.nav-title{color:#fff;font-family:'Bebas Neue',sans-serif;font-size:18px;letter-spacing:.1em;}
.nav-right{margin-left:auto;display:flex;align-items:center;gap:14px;}
.nav-user{font-size:12px;color:#999;}
.nav-link{color:#fff;font-size:12px;font-weight:600;text-decoration:none;opacity:.8;}
.nav-link:hover{opacity:1;}
.nav-link.active{opacity:1;color:var(--accent-light);}

/* ---------- Page header ---------- */
.page-header{background:var(--surface);border-bottom:2px solid var(--accent);padding:24px 40px;display:flex;align-items:flex-end;justify-content:space-between;gap:20px;flex-wrap:wrap;}
.page-header-left{flex:1;min-width:260px;}
.page-icon{font-size:28px;margin-bottom:4px;}
.page-title{font-family:'Bebas Neue',sans-serif;font-size:30px;letter-spacing:.06em;}
.page-title span{color:var(--accent);}
.page-sub{font-size:12px;color:var(--text-muted);margin-top:2px;}
.page-header-actions{display:flex;gap:10px;align-items:center;}

/* ---------- Layout ---------- */
.container{max-width:1300px;margin:0 auto;padding:22px 24px 60px;}

/* ---------- Buttons ---------- */
.btn{border:none;border-radius:8px;padding:10px 18px;font-size:13px;font-weight:600;cursor:pointer;font-family:inherit;text-decoration:none;display:inline-flex;align-items:center;gap:6px;}
.btn-primary{background:var(--accent);color:#fff;}
.btn-primary:hover{background:var(--accent-dark);}
.btn-success{background:#0f6e56;color:#fff;}
.btn-success:hover{background:#0c5a47;}
.btn-secondary{background:#5f6368;color:#fff;}
.btn-secondary:hover{background:#4a4d51;}
.btn-ghost{background:transparent;color:var(--text);border:1px solid var(--border);}
.btn-ghost:hover{background:#fafafa;}
.btn-sm{padding:5px 12px;font-size:12px;}
.btn:disabled{opacity:.5;cursor:not-allowed;}

/* ---------- Card / panels ---------- */
.card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:20px 22px;margin-bottom:18px;box-shadow:0 2px 6px rgba(0,0,0,.04);}

/* ---------- Filter bar ---------- */
.filter-bar{display:flex;gap:12px;align-items:center;flex-wrap:wrap;}
.filter-bar input[type=text],.filter-bar select{border:1px solid var(--border);border-radius:6px;padding:8px 12px;font-size:13px;font-family:inherit;}
.filter-bar input[type=text]{flex:1;min-width:240px;}
.filter-bar select{min-width:200px;}
.filter-bar label{font-size:12px;color:var(--text-muted);display:flex;align-items:center;gap:6px;}

/* ---------- Banner / messages ---------- */
.banner{border-radius:8px;padding:12px 16px;font-size:13px;margin-bottom:16px;}
.banner-success{background:#e8f7f1;color:#0f6e56;border:1px solid #a7dbc7;}
.banner-error{background:#fdecea;color:#c0392b;border:1px solid #f5b7b1;}
.banner-info{background:#eef6fb;color:#2471a3;border:1px solid #aed6f1;}

/* ---------- Table ---------- */
.tbl{width:100%;border-collapse:collapse;background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);overflow:hidden;}
.tbl th{background:#fafafa;font-size:10px;text-transform:uppercase;letter-spacing:.08em;color:var(--text-dim);padding:10px 14px;text-align:left;font-weight:600;border-bottom:1px solid var(--border);}
.tbl td{padding:11px 14px;font-size:13px;border-bottom:1px solid #f0f0f0;}
.tbl tr:last-child td{border-bottom:none;}
.tbl tr:hover td{background:#fafafa;}
.col-code{font-family:'Courier New',monospace;color:var(--text-muted);width:90px;}
.col-num{text-align:right;font-variant-numeric:tabular-nums;}

/* ---------- Status badges ---------- */
.badge{font-size:10px;font-weight:600;padding:3px 9px;border-radius:10px;text-transform:uppercase;letter-spacing:.05em;display:inline-block;}
.badge-active{background:#e8f7f1;color:#0f6e56;}
.badge-inactive{background:#fdecea;color:#c0392b;}

/* ---------- Empty state ---------- */
.empty-state{text-align:center;padding:48px 20px;color:var(--text-muted);font-size:13px;}
.empty-state strong{display:block;font-size:16px;color:var(--text);margin-bottom:6px;}

/* ---------- Edit form ---------- */
.form-section{margin-bottom:22px;}
.form-section-title{font-family:'Bebas Neue',sans-serif;font-size:13px;letter-spacing:.12em;color:var(--accent);margin-bottom:14px;padding-bottom:8px;border-bottom:1px solid var(--border);}
.form-grid-4{display:grid;grid-template-columns:repeat(4,1fr);gap:14px 18px;}
.form-grid-2{display:grid;grid-template-columns:repeat(2,1fr);gap:14px 18px;}
.form-field{display:flex;flex-direction:column;gap:4px;}
.form-field label{font-size:10px;text-transform:uppercase;letter-spacing:.07em;color:var(--text-muted);font-weight:600;}
.form-field input[type=text],.form-field input[type=date],.form-field input[type=number],.form-field select,.form-field textarea{
    border:1px solid var(--border);border-radius:6px;padding:8px 11px;font-size:13px;font-family:inherit;background:#fff;color:var(--text);
}
.form-field input:focus,.form-field select:focus,.form-field textarea:focus{outline:none;border-color:var(--accent);box-shadow:0 0 0 3px rgba(13,148,136,.1);}
.form-field input[readonly]{background:#f7f7f7;color:var(--text-muted);}
.form-field textarea{resize:vertical;min-height:60px;}
.form-field-checkbox{display:flex;align-items:center;gap:8px;font-size:13px;}
.form-field-checkbox input{width:16px;height:16px;}
.form-hint{color:var(--text-muted);font-size:11px;margin-top:6px;}
.actions-row{display:flex;gap:10px;margin-top:24px;padding-top:18px;border-top:1px solid var(--border);}

@media (max-width:900px){
    .form-grid-4{grid-template-columns:repeat(2,1fr);}
}
@media (max-width:600px){
    .form-grid-4,.form-grid-2{grid-template-columns:1fr;}
}
</style>
</head>
<body>
<form id="form1" runat="server">

<nav>
    <a class="nav-logo" href="HREmployee.aspx">
        <img src="/StockApp/Sirimiri_Logo-16_9-72ppi-01.png" alt="Sirimiri" onerror="this.style.display='none'"/>
    </a>
    <span class="nav-title">HUMAN RESOURCES</span>
    <div class="nav-right">
        <span class="nav-user"><asp:Label ID="lblNavUser" runat="server"/></span>
        <a href="HREmployee.aspx" class="nav-link active">Employees</a>
        <a href="HRDepartment.aspx" class="nav-link">Departments</a>
        <a href="HREmployeeImport.aspx" class="nav-link">Import</a>
        <a href="/StockApp/ERPHome.aspx" class="nav-link">&#8592; ERP Home</a>
        <a href="HRLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>

<div class="page-header">
    <div class="page-header-left">
        <div class="page-icon">&#x1F465;</div>
        <div class="page-title">EMPLOYEE <span>MASTER</span></div>
        <div class="page-sub">Manage employee records, departments, KYC details and salary structure</div>
    </div>
    <div class="page-header-actions">
        <asp:Button ID="btnNew" runat="server" Text="+ New Employee" CssClass="btn btn-success"
            OnClick="btnNew_Click" CausesValidation="false" />
    </div>
</div>

<div class="container">

    <asp:Panel ID="pnlMsg" runat="server" Visible="false" CssClass="banner banner-info"></asp:Panel>

    <!-- ================= LIST VIEW ================= -->
    <asp:Panel ID="pnlList" runat="server">
        <div class="card">
            <div class="filter-bar">
                <asp:TextBox ID="txtSearch" runat="server" placeholder="Search by name, code, mobile..." />
                <asp:DropDownList ID="ddlFilterDept" runat="server" />
                <label><asp:CheckBox ID="chkActiveOnly" runat="server" Checked="true" /> Active only</label>
                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
            </div>
        </div>

        <asp:GridView ID="gvEmployees" runat="server" AutoGenerateColumns="false"
                      GridLines="None" CssClass="tbl" DataKeyNames="EmployeeID"
                      OnRowCommand="gvEmployees_RowCommand">
            <Columns>
                <asp:BoundField DataField="EmployeeCode" HeaderText="Code"
                                ItemStyle-CssClass="col-code" />
                <asp:BoundField DataField="FullName" HeaderText="Name" />
                <asp:BoundField DataField="DeptName" HeaderText="Department" />
                <asp:BoundField DataField="Designation" HeaderText="Designation" />
                <asp:BoundField DataField="EmploymentType" HeaderText="Type" />
                <asp:BoundField DataField="DOJ" HeaderText="Joined" DataFormatString="{0:dd-MMM-yy}" />
                <asp:BoundField DataField="MobileNo" HeaderText="Mobile" />
                <asp:BoundField DataField="GrossSalary" HeaderText="Gross" DataFormatString="{0:N0}"
                                ItemStyle-CssClass="col-num" />
                <asp:TemplateField HeaderText="Status">
                    <ItemTemplate>
                        <span class='<%# (Convert.ToInt32(Eval("IsActive"))==1) ? "badge badge-active" : "badge badge-inactive" %>'>
                            <%# (Convert.ToInt32(Eval("IsActive"))==1) ? "Active" : "Left" %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="">
                    <ItemTemplate>
                        <asp:LinkButton runat="server" Text="Edit" CommandName="EditEmp"
                            CommandArgument='<%# Eval("EmployeeID") %>' CssClass="btn btn-secondary btn-sm" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <div class="empty-state">
                    <strong>No employees yet</strong>
                    Click <em>+ New Employee</em> to add one, or use the Import page for bulk upload.
                </div>
            </EmptyDataTemplate>
        </asp:GridView>
    </asp:Panel>

    <!-- ================= EDIT VIEW ================= -->
    <asp:Panel ID="pnlEdit" runat="server" Visible="false">
        <div class="card">
            <div class="form-section-title" style="font-size:16px;border:none;margin-bottom:18px;">
                <asp:Literal ID="litFormHeading" runat="server" Text="New Employee" />
            </div>
            <asp:HiddenField ID="hfEmployeeID" runat="server" Value="0" />

            <div class="form-section">
                <div class="form-section-title">Identity</div>
                <div class="form-grid-4">
                    <div class="form-field">
                        <label>Employee Code *</label>
                        <asp:TextBox ID="txtCode" runat="server" />
                    </div>
                    <div class="form-field">
                        <label>Full Name *</label>
                        <asp:TextBox ID="txtName" runat="server" />
                    </div>
                    <div class="form-field">
                        <label>Father's Name</label>
                        <asp:TextBox ID="txtFatherName" runat="server" />
                    </div>
                    <div class="form-field">
                        <label>Gender</label>
                        <asp:DropDownList ID="ddlGender" runat="server">
                            <asp:ListItem Value="M" Text="Male" />
                            <asp:ListItem Value="F" Text="Female" />
                            <asp:ListItem Value="O" Text="Other" />
                        </asp:DropDownList>
                    </div>
                    <div class="form-field">
                        <label>Date of Birth</label>
                        <asp:TextBox ID="txtDOB" runat="server" TextMode="Date" />
                    </div>
                    <div class="form-field">
                        <label>Date of Joining *</label>
                        <asp:TextBox ID="txtDOJ" runat="server" TextMode="Date" />
                    </div>
                    <div class="form-field" id="dolFieldWrap" runat="server">
                        <label>Date of Leaving</label>
                        <asp:TextBox ID="txtDOL" runat="server" TextMode="Date" />
                    </div>
                    <div class="form-field-checkbox" style="align-self:end;padding-bottom:8px;">
                        <asp:CheckBox ID="chkActive" runat="server" Checked="true" ClientIDMode="Static" /> Active
                    </div>
                </div>
            </div>

            <div class="form-section">
                <div class="form-section-title">Role</div>
                <div class="form-grid-4">
                    <div class="form-field">
                        <label>Department *</label>
                        <asp:DropDownList ID="ddlDept" runat="server" />
                    </div>
                    <div class="form-field">
                        <label>Designation</label>
                        <asp:TextBox ID="txtDesignation" runat="server" />
                    </div>
                    <div class="form-field">
                        <label>Employment Type</label>
                        <asp:DropDownList ID="ddlEmpType" runat="server">
                            <asp:ListItem>Permanent</asp:ListItem>
                            <asp:ListItem>Contract</asp:ListItem>
                            <asp:ListItem>Trainee</asp:ListItem>
                            <asp:ListItem>Apprentice</asp:ListItem>
                            <asp:ListItem>Temporary</asp:ListItem>
                            <asp:ListItem>Director</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div></div>
                </div>
            </div>

            <div class="form-section">
                <div class="form-section-title">Organization</div>
                <div class="form-grid-4">
                    <div class="form-field">
                        <label>Reporting Manager</label>
                        <asp:TextBox ID="txtReportingManager" runat="server" />
                    </div>
                    <div class="form-field">
                        <label>State / Zone</label>
                        <asp:TextBox ID="txtZone" runat="server" />
                    </div>
                    <div class="form-field">
                        <label>Region</label>
                        <asp:TextBox ID="txtRegion" runat="server" />
                    </div>
                    <div class="form-field">
                        <label>Area</label>
                        <asp:TextBox ID="txtArea" runat="server" />
                    </div>
                    <div class="form-field">
                        <label>Work Location</label>
                        <asp:TextBox ID="txtWorkLocation" runat="server" />
                    </div>
                    <div></div><div></div><div></div>
                </div>
                <div class="form-hint">Used primarily for Sales territory hierarchy. Optional for other departments.</div>
            </div>

            <div class="form-section">
                <div class="form-section-title">Contact</div>
                <div class="form-grid-4">
                    <div class="form-field">
                        <label>Mobile</label>
                        <asp:TextBox ID="txtMobile" runat="server" />
                    </div>
                    <div class="form-field">
                        <label>Alt Mobile</label>
                        <asp:TextBox ID="txtAltMobile" runat="server" />
                    </div>
                    <div class="form-field">
                        <label>Email</label>
                        <asp:TextBox ID="txtEmail" runat="server" />
                    </div>
                    <div class="form-field">
                        <label>Pincode</label>
                        <asp:TextBox ID="txtPincode" runat="server" />
                    </div>
                    <div class="form-field">
                        <label>City</label>
                        <asp:TextBox ID="txtCity" runat="server" />
                    </div>
                    <div class="form-field">
                        <label>State</label>
                        <asp:TextBox ID="txtState" runat="server" />
                    </div>
                    <div class="form-field" style="grid-column:span 2;">
                        <label>Address</label>
                        <asp:TextBox ID="txtAddress" runat="server" TextMode="MultiLine" Rows="2" />
                    </div>
                </div>
            </div>

            <div class="form-section">
                <div class="form-section-title">KYC / Statutory</div>
                <div class="form-grid-4">
                    <div class="form-field">
                        <label>Aadhaar No</label>
                        <asp:TextBox ID="txtAadhaar" runat="server" MaxLength="12" />
                    </div>
                    <div class="form-field">
                        <label>PAN</label>
                        <asp:TextBox ID="txtPAN" runat="server" MaxLength="10" />
                    </div>
                    <div class="form-field">
                        <label>UAN</label>
                        <asp:TextBox ID="txtUAN" runat="server" MaxLength="12" />
                    </div>
                    <div class="form-field">
                        <label>PF No</label>
                        <asp:TextBox ID="txtPF" runat="server" />
                    </div>
                    <div class="form-field">
                        <label>ESI No</label>
                        <asp:TextBox ID="txtESI" runat="server" />
                    </div>
                    <div></div><div></div><div></div>
                </div>
            </div>

            <div class="form-section">
                <div class="form-section-title">Bank</div>
                <div class="form-grid-4">
                    <div class="form-field">
                        <label>Bank A/c No</label>
                        <asp:TextBox ID="txtBankAc" runat="server" />
                    </div>
                    <div class="form-field">
                        <label>Bank Name</label>
                        <asp:TextBox ID="txtBankName" runat="server" />
                    </div>
                    <div class="form-field">
                        <label>IFSC</label>
                        <asp:TextBox ID="txtIFSC" runat="server" MaxLength="11" />
                    </div>
                    <div></div>
                </div>
            </div>

            <div class="form-section">
                <div class="form-section-title">Salary</div>
                <div class="form-grid-4">
                    <div class="form-field">
                        <label>Basic</label>
                        <asp:TextBox ID="txtBasic" runat="server" TextMode="Number" Text="0" />
                    </div>
                    <div class="form-field">
                        <label>HRA</label>
                        <asp:TextBox ID="txtHRA" runat="server" TextMode="Number" Text="0" />
                    </div>
                    <div class="form-field">
                        <label>Conveyance</label>
                        <asp:TextBox ID="txtConv" runat="server" TextMode="Number" Text="0" />
                    </div>
                    <div class="form-field">
                        <label>Other Allow.</label>
                        <asp:TextBox ID="txtOther" runat="server" TextMode="Number" Text="0" />
                    </div>
                    <div class="form-field">
                        <label>Gross (computed)</label>
                        <asp:TextBox ID="txtGross" runat="server" TextMode="Number" Text="0" ReadOnly="true" />
                    </div>
                    <div></div><div></div><div></div>
                </div>
                <div class="form-hint">
                    Gross is Basic + HRA + Conveyance + Other on save. Statutory deductions (PF/ESI) will compute in the payroll module.
                </div>
            </div>

            <div class="actions-row">
                <asp:Button ID="btnSave" runat="server" Text="Save Employee" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-ghost"
                    OnClick="btnCancel_Click" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>
</div>
</form>
<script src="/StockApp/erp-keepalive.js"></script>
<script>
// Show "Date of Leaving" only when employee is marked Inactive (= has left).
// Also stays visible if a DOL value is already filled (pre-existing leaver record).
(function () {
    function syncDOLVisibility() {
        var wrap = document.getElementById('dolFieldWrap');
        var chk  = document.getElementById('chkActive');
        if (!wrap || !chk) return;
        var dol  = wrap.querySelector('input[type="date"]');
        var hasDOL = dol && dol.value && dol.value.length > 0;
        var leftOrg = !chk.checked;
        wrap.style.display = (leftOrg || hasDOL) ? '' : 'none';
    }
    // Run after DOM ready and on every postback (UpdatePanel-friendly)
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            syncDOLVisibility();
            var chk = document.getElementById('chkActive');
            if (chk) chk.addEventListener('change', syncDOLVisibility);
        });
    } else {
        syncDOLVisibility();
        var chk = document.getElementById('chkActive');
        if (chk) chk.addEventListener('change', syncDOLVisibility);
    }
})();
</script>
</body>
</html>
