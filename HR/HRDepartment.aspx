<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HRDepartment.aspx.cs" Inherits="HRModule.HRDepartment" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri &mdash; Departments</title>
<link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=DM+Sans:wght@300;400;500;600&display=swap" rel="stylesheet"/>
<link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
<style>
:root{--accent:#0d9488;--accent-dark:#0f766e;--accent-light:#ccfbf1;--text:#1a1a1a;--text-muted:#666;--text-dim:#999;--bg:#f0f0f0;--surface:#fff;--border:#e0e0e0;--radius:12px;}
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

.container{max-width:1100px;margin:0 auto;padding:22px 24px 60px;}
.card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:20px 22px;margin-bottom:18px;box-shadow:0 2px 6px rgba(0,0,0,.04);}

.section-head{font-family:'Bebas Neue',sans-serif;font-size:13px;letter-spacing:.12em;color:var(--accent);margin-bottom:14px;padding-bottom:8px;border-bottom:1px solid var(--border);}

.form-grid{display:grid;grid-template-columns:repeat(4,1fr);gap:14px 18px;}
.form-field{display:flex;flex-direction:column;gap:4px;}
.form-field label{font-size:10px;text-transform:uppercase;letter-spacing:.07em;color:var(--text-muted);font-weight:600;}
.form-field input[type=text]{border:1px solid var(--border);border-radius:6px;padding:8px 11px;font-size:13px;font-family:inherit;background:#fff;color:var(--text);}
.form-field input:focus{outline:none;border-color:var(--accent);box-shadow:0 0 0 3px rgba(13,148,136,.1);}
.form-field-checkbox{display:flex;align-items:center;gap:8px;font-size:13px;align-self:end;padding-bottom:8px;}
.form-field-checkbox input{width:16px;height:16px;}
.form-hint{color:var(--text-muted);font-size:11px;margin-top:8px;}
.actions-row{display:flex;gap:10px;margin-top:18px;padding-top:14px;border-top:1px solid var(--border);}

.btn{border:none;border-radius:8px;padding:10px 18px;font-size:13px;font-weight:600;cursor:pointer;font-family:inherit;text-decoration:none;display:inline-flex;align-items:center;gap:6px;}
.btn-primary{background:var(--accent);color:#fff;}
.btn-primary:hover{background:var(--accent-dark);}
.btn-secondary{background:#5f6368;color:#fff;}
.btn-ghost{background:transparent;color:var(--text);border:1px solid var(--border);}
.btn-ghost:hover{background:#fafafa;}
.btn-sm{padding:5px 12px;font-size:12px;}

.banner{border-radius:8px;padding:12px 16px;font-size:13px;margin-bottom:16px;}
.banner-success{background:#e8f7f1;color:#0f6e56;border:1px solid #a7dbc7;}
.banner-error{background:#fdecea;color:#c0392b;border:1px solid #f5b7b1;}
.banner-info{background:#eef6fb;color:#2471a3;border:1px solid #aed6f1;}

.tbl{width:100%;border-collapse:collapse;background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);overflow:hidden;}
.tbl th{background:#fafafa;font-size:10px;text-transform:uppercase;letter-spacing:.08em;color:var(--text-dim);padding:10px 14px;text-align:left;font-weight:600;border-bottom:1px solid var(--border);}
.tbl td{padding:11px 14px;font-size:13px;border-bottom:1px solid #f0f0f0;}
.tbl tr:last-child td{border-bottom:none;}
.tbl tr:hover td{background:#fafafa;}
.col-code{font-family:'Courier New',monospace;color:var(--text-muted);width:90px;}
.col-prefix{font-family:'Courier New',monospace;font-weight:600;color:var(--accent);}

.badge{font-size:10px;font-weight:600;padding:3px 9px;border-radius:10px;text-transform:uppercase;letter-spacing:.05em;display:inline-block;}
.badge-active{background:#e8f7f1;color:#0f6e56;}
.badge-inactive{background:#fdecea;color:#c0392b;}
.badge-na{background:#f0f0f0;color:#888;font-style:italic;}

.empty-state{text-align:center;padding:36px 20px;color:var(--text-muted);font-size:13px;}

@media(max-width:700px){.form-grid{grid-template-columns:repeat(2,1fr);}}
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
        <a href="HREmployee.aspx" class="nav-link">Employees</a>
        <a href="HRDepartment.aspx" class="nav-link active">Departments</a>
        <a href="HREmployeeImport.aspx" class="nav-link">Import</a>
        <a href="/StockApp/ERPHome.aspx" class="nav-link">&#8592; ERP Home</a>
        <a href="HRLogout.aspx" class="nav-link">Sign Out</a>
    </div>
</nav>

<div class="page-header">
    <div class="page-icon">&#x1F3E2;</div>
    <div class="page-title">DEPARTMENT <span>MASTER</span></div>
    <div class="page-sub">Departments organize employees. Set the Code Prefix to enable auto-generated department-specific employee codes (e.g. EMPS001 for Sales).</div>
</div>

<div class="container">

    <asp:Panel ID="pnlMsg" runat="server" Visible="false" CssClass="banner banner-info"></asp:Panel>

    <!-- Add / Edit form -->
    <div class="card">
        <div class="section-head">
            <asp:Literal ID="litFormHeading" runat="server" Text="Add Department" />
        </div>
        <asp:HiddenField ID="hfDeptID" runat="server" Value="0" />

        <div class="form-grid">
            <div class="form-field">
                <label>Dept Code *</label>
                <asp:TextBox ID="txtCode" runat="server" placeholder="e.g. SALES" />
            </div>
            <div class="form-field">
                <label>Dept Name *</label>
                <asp:TextBox ID="txtName" runat="server" />
            </div>
            <div class="form-field">
                <label>Code Prefix</label>
                <asp:TextBox ID="txtCodePrefix" runat="server" placeholder="e.g. EMPS" MaxLength="10" />
            </div>
            <div class="form-field-checkbox">
                <asp:CheckBox ID="chkActive" runat="server" Checked="true" /> Active
            </div>
        </div>
        <div class="form-hint">
            <b>Code Prefix</b> drives employee code auto-generation for this department.
            Example: prefix <code style="background:#fff;padding:1px 5px;border-radius:3px;border:1px solid var(--border);">EMPS</code> &rarr; new hires get EMPS001, EMPS002, &hellip;
            Leave blank if you don't want auto-generated codes for this department yet.
        </div>

        <div class="actions-row">
            <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
            <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-ghost" OnClick="btnClear_Click" CausesValidation="false" />
        </div>
    </div>

    <!-- List -->
    <div class="card">
        <div class="section-head">All Departments</div>
        <asp:GridView ID="gvDepts" runat="server" AutoGenerateColumns="false"
                      GridLines="None" CssClass="tbl" DataKeyNames="DeptID"
                      OnRowCommand="gvDepts_RowCommand">
            <Columns>
                <asp:BoundField DataField="DeptCode" HeaderText="Code" ItemStyle-CssClass="col-code" />
                <asp:BoundField DataField="DeptName" HeaderText="Name" />
                <asp:TemplateField HeaderText="Code Prefix">
                    <ItemTemplate>
                        <%# string.IsNullOrWhiteSpace(Eval("CodePrefix") as string)
                              ? "<span class='badge badge-na'>not set</span>"
                              : "<span class='col-prefix'>" + Server.HtmlEncode(Eval("CodePrefix").ToString()) + "</span>" %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Status">
                    <ItemTemplate>
                        <span class='<%# (Convert.ToInt32(Eval("IsActive"))==1) ? "badge badge-active" : "badge badge-inactive" %>'>
                            <%# (Convert.ToInt32(Eval("IsActive"))==1) ? "Active" : "Inactive" %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="">
                    <ItemTemplate>
                        <asp:LinkButton runat="server" Text="Edit" CommandName="EditDept"
                            CommandArgument='<%# Eval("DeptID") %>' CssClass="btn btn-secondary btn-sm" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <div class="empty-state">No departments yet.</div>
            </EmptyDataTemplate>
        </asp:GridView>
    </div>

</div>
</form>
<script src="/StockApp/erp-keepalive.js"></script>
</body>
</html>
