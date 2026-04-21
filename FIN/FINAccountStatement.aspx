<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FINAccountStatement.aspx.cs"
    Inherits="Sirimiri.FIN.FINAccountStatement" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Account Statement — FIN</title>
    <link rel="stylesheet" href="FINStyles.css" />
    <style>
        .filter-bar { padding:10px; background:#f3f3f3; border:1px solid #ddd; margin-bottom:12px; }
        .filter-bar label { margin-right:6px; font-weight:600; }
        .filter-bar .ctrl { margin-right:16px; }
        .party-header { padding:10px 12px; border:1px solid #ccc; background:#fafafa; margin-bottom:8px; }
        .party-header strong { font-size:15px; }
        .stmt-table { width:100%; border-collapse:collapse; font-size:13px; }
        .stmt-table th, .stmt-table td { border:1px solid #ccc; padding:6px 8px; vertical-align:top; }
        .stmt-table th { background:#eee; text-align:left; }
        .stmt-table td.amt { text-align:right; font-family:Consolas,monospace; white-space:nowrap; }
        .stmt-table tr.opening { background:#fff9e0; font-weight:600; }
        .stmt-table tr.totals { background:#f0f0f0; font-weight:600; }
        .stmt-table tr.closing { background:#e8f4ff; font-weight:700; }
        .err { color:#b00; }
        .hint { color:#888; font-size:12px; margin-left:8px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:PlaceHolder ID="phNav" runat="server" />

        <div class="page-container">
            <h2>FIN › Account Statement</h2>

            <asp:Label ID="lblMsg" runat="server" CssClass="err" />

            <div class="filter-bar">
                <span class="ctrl">
                    <asp:RadioButtonList ID="rblPartyType" runat="server" RepeatDirection="Horizontal"
                        AutoPostBack="true" OnSelectedIndexChanged="rblPartyType_Changed">
                        <asp:ListItem Value="CUS" Text="Customer" Selected="True" />
                        <asp:ListItem Value="SUP" Text="Supplier" />
                    </asp:RadioButtonList>
                </span>

                <span class="ctrl">
                    <label>Party:</label>
                    <asp:DropDownList ID="ddlParty" runat="server" AutoPostBack="true"
                        OnSelectedIndexChanged="ddlParty_Changed" Width="280px" />
                </span>

                <span class="ctrl">
                    <label>From:</label>
                    <asp:TextBox ID="txtFrom" runat="server" Width="110px" />
                </span>

                <span class="ctrl">
                    <label>To:</label>
                    <asp:TextBox ID="txtTo" runat="server" Width="110px" />
                </span>

                <asp:Button ID="btnRefresh" runat="server" Text="Refresh"
                    OnClick="btnRefresh_Click" CssClass="btn-secondary" />
                <asp:Button ID="btnPrint" runat="server" Text="Print PDF"
                    OnClick="btnPrint_Click" CssClass="btn-primary" />
                <span class="hint">Date format: dd-MMM-yyyy (e.g. 01-Apr-2026)</span>
            </div>

            <asp:Panel ID="pnlStatement" runat="server" Visible="false">
                <div class="party-header">
                    <strong><asp:Label ID="lblPartyName" runat="server" /></strong>
                    <asp:Label ID="lblPartyGSTIN" runat="server" />
                    <br />
                    <small>
                        Statement: <asp:Label ID="lblPeriod" runat="server" />
                    </small>
                </div>

                <table class="stmt-table">
                    <thead>
                        <tr>
                            <th style="width:90px;">Date</th>
                            <th style="width:110px;">Voucher</th>
                            <th>Particulars</th>
                            <th style="width:110px;text-align:right;">Debit</th>
                            <th style="width:110px;text-align:right;">Credit</th>
                            <th style="width:130px;text-align:right;">Balance</th>
                        </tr>
                    </thead>
                    <tbody>
                        <%-- Opening balance row --%>
                        <asp:PlaceHolder ID="phOpening" runat="server" />

                        <%-- Transaction rows --%>
                        <asp:Repeater ID="rptLines" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("TxnDate", "{0:dd-MMM-yyyy}") %></td>
                                    <td><%# Eval("VoucherNo") %></td>
                                    <td><%# Eval("Particulars") %></td>
                                    <td class="amt"><%# FormatAmt((decimal)Eval("Debit")) %></td>
                                    <td class="amt"><%# FormatAmt((decimal)Eval("Credit")) %></td>
                                    <td class="amt"><%# FormatBalance(Container.DataItem) %></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>

                        <%-- Totals and closing --%>
                        <asp:PlaceHolder ID="phFooter" runat="server" />
                    </tbody>
                </table>
            </asp:Panel>

            <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
                <p><em>No transactions in the selected range.</em></p>
            </asp:Panel>
        </div>
    </form>
</body>
</html>
