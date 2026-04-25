<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HREmployeeImport.aspx.cs" Inherits="HRModule.HREmployeeImport" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Import Employees — Sirimiri ERP</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <style>
        * { box-sizing: border-box; }
        body { margin:0; font-family: 'Segoe UI', Arial, sans-serif; background:#f4f5f7; color:#222; }
        .navbar { background:#111; color:#fff; padding:12px 22px; display:flex; gap:18px; align-items:center; }
        .navbar a { color:#fff; text-decoration:none; font-size:14px; }
        .navbar a:hover { color:#ff6a3d; }
        .brand { font-weight:600; letter-spacing:.5px; }
        .page-header { padding:18px 22px; background:#fff; border-bottom:3px solid #d93025; }
        .page-header h1 { margin:0; font-size:22px; }
        .container { max-width:1400px; margin:20px auto; padding:0 20px; }
        .card { background:#fff; border:1px solid #e1e3e7; border-radius:6px; padding:18px; margin-bottom:18px; }
        .btn { padding:8px 16px; border:none; border-radius:4px; cursor:pointer; font-size:14px; text-decoration:none; display:inline-block; }
        .btn-primary { background:#1a73e8; color:#fff; }
        .btn-success { background:#137333; color:#fff; }
        .btn-secondary { background:#5f6368; color:#fff; }
        .msg { padding:9px 12px; border-radius:4px; margin-bottom:12px; font-size:14px; }
        .msg-ok  { background:#e6f4ea; color:#137333; border:1px solid #ceead6; }
        .msg-err { background:#fce8e6; color:#c5221f; border:1px solid #f6c2bd; }
        .msg-warn{ background:#fef7e0; color:#b06000; border:1px solid #feefc3; }
        table.gv { width:100%; border-collapse:collapse; font-size:12px; }
        table.gv th { background:#f1f3f4; text-align:left; padding:7px; border-bottom:1px solid #dadce0; position:sticky; top:0; }
        table.gv td { padding:5px 7px; border-bottom:1px solid #eee; }
        .row-ok { }
        .row-err { background:#fce8e6; }
        .row-warn{ background:#fef7e0; }
        .toolbar { display:flex; gap:8px; align-items:center; flex-wrap:wrap; }
        .hint { color:#5f6368; font-size:12px; margin-top:6px; }
        .stat { display:inline-block; padding:6px 12px; background:#f1f3f4; border-radius:4px; margin-right:8px; }
        .stat b { color:#1a73e8; }
    </style>
</head>
<body>
<form id="form1" runat="server" enctype="multipart/form-data">
    <div class="navbar">
        <span class="brand">SIRIMIRI ERP</span>
        <a href="/">Home</a>
        <a href="HREmployee.aspx">Employees</a>
        <a href="HRDepartment.aspx">Departments</a>
        <a href="HREmployeeImport.aspx"><b>Import</b></a>
    </div>
    <div class="page-header"><h1>Import Employees from Excel</h1></div>

    <div class="container">
        <asp:Panel ID="pnlMsg" runat="server" Visible="false" CssClass="msg"></asp:Panel>

        <div class="card">
            <h3 style="margin-top:0">1. Upload</h3>
            <div class="toolbar">
                <asp:FileUpload ID="fuFile" runat="server" />
                <asp:Button ID="btnUpload" runat="server" Text="Preview" CssClass="btn btn-primary" OnClick="btnUpload_Click" />
                <asp:CheckBox ID="chkAutoCreateDept" runat="server" Checked="true" Text="Auto-create missing departments" />
                <a href="HREmployeeImport.ashx?action=template" class="btn btn-secondary">Download Template</a>
            </div>
            <div class="hint">
                Expected columns (header row required, order flexible):<br />
                <b>EmployeeCode, FullName, FatherName, Gender, DOB, DOJ, Department, Designation,
                EmploymentType, Mobile, AltMobile, Email, Address, City, State, Pincode,
                Aadhaar, PAN, UAN, PFNo, ESINo, BankAcNo, BankName, IFSC, Basic, HRA, Conveyance, Other</b>
                <br />Dates as dd-mm-yyyy or Excel date. Leave EmployeeCode blank to auto-generate EMP###.
            </div>
            <asp:HiddenField ID="hfFilePath" runat="server" />
        </div>

        <asp:Panel ID="pnlResults" runat="server" Visible="false">
            <div class="card">
                <h3 style="margin-top:0">2. Preview</h3>
                <div style="margin-bottom:10px;">
                    <span class="stat">Total: <b><asp:Literal ID="litTotal" runat="server" /></b></span>
                    <span class="stat">Ready: <b style="color:#137333;"><asp:Literal ID="litOK" runat="server" /></b></span>
                    <span class="stat">Warnings: <b style="color:#b06000;"><asp:Literal ID="litWarn" runat="server" /></b></span>
                    <span class="stat">Errors: <b style="color:#c5221f;"><asp:Literal ID="litErr" runat="server" /></b></span>
                </div>

                <div style="max-height:520px; overflow:auto; border:1px solid #e1e3e7;">
                    <asp:GridView ID="gvPreview" runat="server" AutoGenerateColumns="false"
                                  GridLines="None" CssClass="gv" OnRowDataBound="gvPreview_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="RowNum" HeaderText="#" />
                            <asp:BoundField DataField="Status" HeaderText="Status" />
                            <asp:BoundField DataField="Message" HeaderText="Issue" />
                            <asp:BoundField DataField="EmployeeCode" HeaderText="Code" />
                            <asp:BoundField DataField="FullName" HeaderText="Name" />
                            <asp:BoundField DataField="Department" HeaderText="Dept" />
                            <asp:BoundField DataField="Designation" HeaderText="Designation" />
                            <asp:BoundField DataField="EmploymentType" HeaderText="Type" />
                            <asp:BoundField DataField="DOJ" HeaderText="DOJ" DataFormatString="{0:dd-MMM-yy}" />
                            <asp:BoundField DataField="MobileNo" HeaderText="Mobile" />
                            <asp:BoundField DataField="AadhaarNo" HeaderText="Aadhaar" />
                            <asp:BoundField DataField="GrossSalary" HeaderText="Gross"
                                            DataFormatString="{0:N0}" ItemStyle-HorizontalAlign="Right" />
                        </Columns>
                    </asp:GridView>
                </div>

                <div class="toolbar" style="margin-top:14px;">
                    <asp:Button ID="btnConfirm" runat="server" Text="Import Ready Rows" CssClass="btn btn-success"
                        OnClick="btnConfirm_Click"
                        OnClientClick="return confirm('Import the rows marked READY into HR_Employee?');" />
                    <asp:Button ID="btnReset" runat="server" Text="Cancel" CssClass="btn btn-secondary"
                        OnClick="btnReset_Click" CausesValidation="false" />
                </div>
            </div>
        </asp:Panel>
    </div>
</form>
</body>
</html>
