<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HREmployee.aspx.cs" Inherits="HRModule.HREmployee" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Employees — Sirimiri ERP</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <style>
        * { box-sizing: border-box; }
        body { margin:0; font-family: 'Segoe UI', Arial, sans-serif; background:#f4f5f7; color:#222; }
        .navbar { background:#111; color:#fff; padding:12px 22px; display:flex; gap:18px; align-items:center; }
        .navbar a { color:#fff; text-decoration:none; font-size:14px; }
        .navbar a:hover { color:#ff6a3d; }
        .brand { font-weight:600; letter-spacing:.5px; }
        .page-header { padding:18px 22px; background:#fff; border-bottom:3px solid #d93025; display:flex; justify-content:space-between; align-items:center; }
        .page-header h1 { margin:0; font-size:22px; }
        .container { max-width:1300px; margin:20px auto; padding:0 20px; }
        .card { background:#fff; border:1px solid #e1e3e7; border-radius:6px; padding:18px; margin-bottom:18px; }
        .card h3 { margin-top:0; padding-bottom:8px; border-bottom:1px solid #eee; }
        .section { margin-top:16px; }
        .section-title { font-weight:600; color:#d93025; margin-bottom:8px; font-size:13px; text-transform:uppercase; letter-spacing:.5px; }
        .grid-4 { display:grid; grid-template-columns: 140px 1fr 140px 1fr; gap:10px 14px; align-items:center; }
        .grid-2 { display:grid; grid-template-columns: 140px 1fr; gap:10px 14px; align-items:center; }
        input[type=text], input[type=date], input[type=number], select, textarea {
            width:100%; padding:7px 9px; border:1px solid #c4c7cc; border-radius:4px; font-size:14px;
            font-family: inherit;
        }
        textarea { resize:vertical; min-height:60px; }
        .btn { padding:8px 16px; border:none; border-radius:4px; cursor:pointer; font-size:14px; text-decoration:none; display:inline-block; }
        .btn-primary { background:#1a73e8; color:#fff; }
        .btn-danger  { background:#d93025; color:#fff; }
        .btn-secondary { background:#5f6368; color:#fff; }
        .btn-success { background:#137333; color:#fff; }
        table.gv { width:100%; border-collapse:collapse; font-size:13px; }
        table.gv th { background:#f1f3f4; text-align:left; padding:8px; border-bottom:1px solid #dadce0; }
        table.gv td { padding:7px 8px; border-bottom:1px solid #eee; }
        table.gv tr:hover { background:#fafbfc; }
        .msg { padding:9px 12px; border-radius:4px; margin-bottom:12px; font-size:14px; }
        .msg-ok  { background:#e6f4ea; color:#137333; border:1px solid #ceead6; }
        .msg-err { background:#fce8e6; color:#c5221f; border:1px solid #f6c2bd; }
        .search-bar { display:flex; gap:8px; align-items:center; margin-bottom:12px; }
        .search-bar input { flex:1; }
        .search-bar select { width:180px; }
        .pill { display:inline-block; padding:2px 10px; border-radius:10px; font-size:11px; }
        .pill-on  { background:#e6f4ea; color:#137333; }
        .pill-off { background:#fce8e6; color:#c5221f; }
        .actions-row { display:flex; gap:8px; margin-top:14px; }
        .hint { color:#5f6368; font-size:12px; }
    </style>
</head>
<body>
<form id="form1" runat="server">
    <div class="navbar">
        <span class="brand">SIRIMIRI ERP</span>
        <a href="/">Home</a>
        <a href="/MM/">MM</a>
        <a href="/PP/">PP</a>
        <a href="/PK/">PK</a>
        <a href="/FIN/">FIN</a>
        <a href="HREmployee.aspx"><b>HR · Employees</b></a>
        <a href="HRDepartment.aspx">Departments</a>
        <a href="HREmployeeImport.aspx">Import</a>
    </div>
    <div class="page-header">
        <h1>Employees</h1>
        <div>
            <asp:Button ID="btnNew" runat="server" Text="+ New Employee" CssClass="btn btn-success"
                OnClick="btnNew_Click" CausesValidation="false" />
        </div>
    </div>

    <div class="container">
        <asp:Panel ID="pnlMsg" runat="server" Visible="false" CssClass="msg"></asp:Panel>

        <!-- ================= LIST VIEW ================= -->
        <asp:Panel ID="pnlList" runat="server">
            <div class="card">
                <div class="search-bar">
                    <asp:TextBox ID="txtSearch" runat="server" placeholder="Search by name, code, mobile..." />
                    <asp:DropDownList ID="ddlFilterDept" runat="server" />
                    <asp:CheckBox ID="chkActiveOnly" runat="server" Checked="true" Text="Active only" />
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
                </div>

                <asp:GridView ID="gvEmployees" runat="server" AutoGenerateColumns="false"
                              GridLines="None" CssClass="gv" DataKeyNames="EmployeeID"
                              OnRowCommand="gvEmployees_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="EmployeeCode" HeaderText="Code" />
                        <asp:BoundField DataField="FullName" HeaderText="Name" />
                        <asp:BoundField DataField="DeptName" HeaderText="Department" />
                        <asp:BoundField DataField="Designation" HeaderText="Designation" />
                        <asp:BoundField DataField="EmploymentType" HeaderText="Type" />
                        <asp:BoundField DataField="DOJ" HeaderText="Joined" DataFormatString="{0:dd-MMM-yy}" />
                        <asp:BoundField DataField="MobileNo" HeaderText="Mobile" />
                        <asp:BoundField DataField="GrossSalary" HeaderText="Gross" DataFormatString="{0:N0}"
                                        ItemStyle-HorizontalAlign="Right" />
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='<%# (Convert.ToInt32(Eval("IsActive"))==1) ? "pill pill-on" : "pill pill-off" %>'>
                                    <%# (Convert.ToInt32(Eval("IsActive"))==1) ? "Active" : "Left" %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="">
                            <ItemTemplate>
                                <asp:LinkButton runat="server" Text="Edit" CommandName="EditEmp"
                                    CommandArgument='<%# Eval("EmployeeID") %>' CssClass="btn btn-secondary"
                                    Style="padding:3px 10px" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div style="padding:20px; text-align:center; color:#5f6368;">
                            No employees yet. Click "+ New Employee" to add one, or use the Import page.
                        </div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </asp:Panel>

        <!-- ================= EDIT VIEW ================= -->
        <asp:Panel ID="pnlEdit" runat="server" Visible="false">
            <div class="card">
                <h3>
                    <asp:Literal ID="litFormHeading" runat="server" Text="New Employee" />
                </h3>
                <asp:HiddenField ID="hfEmployeeID" runat="server" Value="0" />

                <div class="section">
                    <div class="section-title">Identity</div>
                    <div class="grid-4">
                        <label>Employee Code *</label>
                        <asp:TextBox ID="txtCode" runat="server" />
                        <label>Full Name *</label>
                        <asp:TextBox ID="txtName" runat="server" />

                        <label>Father's Name</label>
                        <asp:TextBox ID="txtFatherName" runat="server" />
                        <label>Gender</label>
                        <asp:DropDownList ID="ddlGender" runat="server">
                            <asp:ListItem Value="M" Text="Male" />
                            <asp:ListItem Value="F" Text="Female" />
                            <asp:ListItem Value="O" Text="Other" />
                        </asp:DropDownList>

                        <label>Date of Birth</label>
                        <asp:TextBox ID="txtDOB" runat="server" TextMode="Date" />
                        <label>Date of Joining *</label>
                        <asp:TextBox ID="txtDOJ" runat="server" TextMode="Date" />

                        <label>Date of Leaving</label>
                        <asp:TextBox ID="txtDOL" runat="server" TextMode="Date" />
                        <label>Active</label>
                        <asp:CheckBox ID="chkActive" runat="server" Checked="true" />
                    </div>
                </div>

                <div class="section">
                    <div class="section-title">Role</div>
                    <div class="grid-4">
                        <label>Department *</label>
                        <asp:DropDownList ID="ddlDept" runat="server" />
                        <label>Designation</label>
                        <asp:TextBox ID="txtDesignation" runat="server" />

                        <label>Employment Type</label>
                        <asp:DropDownList ID="ddlEmpType" runat="server">
                            <asp:ListItem>Permanent</asp:ListItem>
                            <asp:ListItem>Contract</asp:ListItem>
                            <asp:ListItem>Trainee</asp:ListItem>
                            <asp:ListItem>Apprentice</asp:ListItem>
                            <asp:ListItem>Temporary</asp:ListItem>
                        </asp:DropDownList>
                        <label></label><label></label>
                    </div>
                </div>

                <div class="section">
                    <div class="section-title">Contact</div>
                    <div class="grid-4">
                        <label>Mobile</label>
                        <asp:TextBox ID="txtMobile" runat="server" />
                        <label>Alt Mobile</label>
                        <asp:TextBox ID="txtAltMobile" runat="server" />

                        <label>Email</label>
                        <asp:TextBox ID="txtEmail" runat="server" />
                        <label>Pincode</label>
                        <asp:TextBox ID="txtPincode" runat="server" />

                        <label>City</label>
                        <asp:TextBox ID="txtCity" runat="server" />
                        <label>State</label>
                        <asp:TextBox ID="txtState" runat="server" />
                    </div>
                    <div class="grid-2" style="margin-top:10px;">
                        <label>Address</label>
                        <asp:TextBox ID="txtAddress" runat="server" TextMode="MultiLine" Rows="2" />
                    </div>
                </div>

                <div class="section">
                    <div class="section-title">KYC / Statutory</div>
                    <div class="grid-4">
                        <label>Aadhaar No</label>
                        <asp:TextBox ID="txtAadhaar" runat="server" MaxLength="12" />
                        <label>PAN</label>
                        <asp:TextBox ID="txtPAN" runat="server" MaxLength="10" />

                        <label>UAN</label>
                        <asp:TextBox ID="txtUAN" runat="server" MaxLength="12" />
                        <label>PF No</label>
                        <asp:TextBox ID="txtPF" runat="server" />

                        <label>ESI No</label>
                        <asp:TextBox ID="txtESI" runat="server" />
                        <label></label><label></label>
                    </div>
                </div>

                <div class="section">
                    <div class="section-title">Bank</div>
                    <div class="grid-4">
                        <label>Bank A/c No</label>
                        <asp:TextBox ID="txtBankAc" runat="server" />
                        <label>Bank Name</label>
                        <asp:TextBox ID="txtBankName" runat="server" />

                        <label>IFSC</label>
                        <asp:TextBox ID="txtIFSC" runat="server" MaxLength="11" />
                        <label></label><label></label>
                    </div>
                </div>

                <div class="section">
                    <div class="section-title">Salary</div>
                    <div class="grid-4">
                        <label>Basic</label>
                        <asp:TextBox ID="txtBasic" runat="server" TextMode="Number" Text="0" />
                        <label>HRA</label>
                        <asp:TextBox ID="txtHRA" runat="server" TextMode="Number" Text="0" />

                        <label>Conveyance</label>
                        <asp:TextBox ID="txtConv" runat="server" TextMode="Number" Text="0" />
                        <label>Other Allow.</label>
                        <asp:TextBox ID="txtOther" runat="server" TextMode="Number" Text="0" />

                        <label>Gross (computed)</label>
                        <asp:TextBox ID="txtGross" runat="server" TextMode="Number" Text="0" ReadOnly="true" />
                        <label></label><label></label>
                    </div>
                    <div class="hint" style="margin-top:6px;">
                        Gross is Basic + HRA + Conveyance + Other on save. Statutory deductions (PF/ESI) will compute in the payroll module.
                    </div>
                </div>

                <div class="actions-row">
                    <asp:Button ID="btnSave" runat="server" Text="Save Employee" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary"
                        OnClick="btnCancel_Click" CausesValidation="false" />
                </div>
            </div>
        </asp:Panel>
    </div>
</form>
</body>
</html>
