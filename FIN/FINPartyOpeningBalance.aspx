<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FINPartyOpeningBalance.aspx.cs"
    Inherits="FINApp.FINPartyOpeningBalance" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Party Opening Balance — FIN</title>
    <link rel="stylesheet" href="../erp-tablet.css" />
    <style>
        .page-container { max-width:900px; margin:16px auto; padding:12px; font-family:Segoe UI,Arial,sans-serif; }
        .opening-card { border:1px solid #ccc; padding:12px; margin:12px 0; background:#f9f9f9; border-radius:4px; }
        .opening-card.exists { background:#fff7e6; border-color:#d4a017; }
        .form-row { margin:10px 0; }
        .form-row label { display:inline-block; min-width:140px; font-weight:600; }
        .audit-table { width:100%; border-collapse:collapse; margin-top:16px; font-size:13px; }
        .audit-table th, .audit-table td { border:1px solid #ddd; padding:6px 8px; text-align:left; }
        .audit-table th { background:#eee; }
        .amt { text-align:right; font-family:Consolas,monospace; }
        .err { color:#b00; font-weight:600; }
        .ok  { color:#060; font-weight:600; }
        .access-denied { padding:30px; text-align:center; color:#b00; font-size:16px; }
        .btn-primary { padding:6px 18px; background:#1a6e1a; color:#fff; border:0; border-radius:3px; cursor:pointer; }
        .btn-primary:hover { background:#125012; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="page-container">
            <div style="margin-bottom:8px;">
                <a href="FINHome.aspx">← FIN Home</a>
                &nbsp;|&nbsp;
                <span>Logged in: <asp:Label ID="lblNavUser" runat="server" /></span>
            </div>
            <h2>FIN › Party Opening Balance</h2>

            <asp:Panel ID="pnlDenied" runat="server" Visible="false" CssClass="access-denied">
                <p>This page requires the <strong>Super</strong> role.</p>
            </asp:Panel>

            <asp:Panel ID="pnlMain" runat="server">
                <asp:Label ID="lblMsg" runat="server" />

                <div class="form-row">
                    <label>Party Type:</label>
                    <asp:RadioButtonList ID="rblPartyType" runat="server" RepeatDirection="Horizontal"
                        AutoPostBack="true" OnSelectedIndexChanged="rblPartyType_Changed">
                        <asp:ListItem Value="CUS" Text="Customer" Selected="True" />
                        <asp:ListItem Value="SUP" Text="Supplier" />
                    </asp:RadioButtonList>
                </div>

                <div class="form-row">
                    <label>Party:</label>
                    <asp:DropDownList ID="ddlParty" runat="server" AutoPostBack="true"
                        OnSelectedIndexChanged="ddlParty_Changed" Width="320px" />
                </div>

                <div class="form-row">
                    <label>Financial Year:</label>
                    <asp:DropDownList ID="ddlFY" runat="server" AutoPostBack="true"
                        OnSelectedIndexChanged="ddlFY_Changed" Width="120px" />
                    <span style="margin-left:20px;">As of:</span>
                    <asp:Label ID="lblAsOfDate" runat="server" Font-Bold="true" />
                </div>

                <asp:Panel ID="pnlCurrent" runat="server" CssClass="opening-card exists" Visible="false">
                    <strong>Current Opening:</strong>
                    <asp:Label ID="lblCurrentAmount" runat="server" />
                    <br />
                    <small>
                        Set on <asp:Label ID="lblCreatedOn" runat="server" /> by
                        <asp:Label ID="lblCreatedBy" runat="server" />
                        <asp:Label ID="lblLastModified" runat="server" />
                    </small>
                </asp:Panel>

                <asp:Panel ID="pnlNone" runat="server" CssClass="opening-card" Visible="false">
                    <em>No opening balance set for this party in FY <asp:Label ID="lblFYEcho" runat="server" />.</em>
                </asp:Panel>

                <h3>New / Edit Opening</h3>
                <div class="form-row">
                    <label>Amount:</label>
                    <asp:TextBox ID="txtAmount" runat="server" Width="140px" CssClass="amt"
                        onfocus="this.select();" />
                    <asp:RadioButtonList ID="rblDrCr" runat="server" RepeatDirection="Horizontal"
                        style="display:inline-block;margin-left:12px;">
                        <asp:ListItem Value="Dr" Text="Dr" Selected="True" />
                        <asp:ListItem Value="Cr" Text="Cr" />
                    </asp:RadioButtonList>
                </div>

                <div class="form-row">
                    <label>Reason:</label>
                    <asp:TextBox ID="txtReason" runat="server" Width="400px" MaxLength="200" />
                    <span style="margin-left:8px;color:#888;font-size:12px;">
                        (required when editing an existing opening)
                    </span>
                </div>

                <div class="form-row">
                    <asp:Button ID="btnSave" runat="server" Text="Save Opening Balance"
                        OnClick="btnSave_Click" CssClass="btn-primary" />
                </div>

                <h3>Audit History</h3>
                <asp:PlaceHolder ID="phAudit" runat="server" />
            </asp:Panel>
        </div>
    </form>
</body>
</html>
