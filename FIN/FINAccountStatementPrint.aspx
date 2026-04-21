<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FINAccountStatementPrint.aspx.cs"
    Inherits="FINApp.FINAccountStatementPrint" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Account Statement — Print</title>
    <style>
        @page { size: A4; margin: 12mm; }
        body {
            font-family: 'Calibri', 'Segoe UI', sans-serif;
            font-size: 11pt; color: #000; margin: 0;
        }
        .letterhead {
            text-align: center; border-bottom: 2px solid #000;
            padding-bottom: 8px; margin-bottom: 10px;
        }
        .letterhead h1 { margin: 0; font-size: 18pt; letter-spacing: 1px; }
        .letterhead .sub { font-size: 9pt; color: #333; }
        .title-bar {
            text-align: center; font-weight: bold; font-size: 13pt;
            margin: 8px 0 4px 0; text-transform: uppercase;
        }
        .party-block {
            display: table; width: 100%; margin-bottom: 8px;
            border: 1px solid #000;
        }
        .party-block .cell {
            display: table-cell; padding: 4px 8px; vertical-align: top;
        }
        .party-block .cell.right { text-align: right; width: 40%; }
        .party-block .label { font-size: 9pt; color: #555; }
        table.stmt { width: 100%; border-collapse: collapse; font-size: 10pt; }
        table.stmt th, table.stmt td {
            border: 1px solid #000; padding: 4px 6px; vertical-align: top;
        }
        table.stmt th { background: #eee; text-align: left; }
        table.stmt td.amt {
            text-align: right; font-family: 'Consolas', monospace; white-space: nowrap;
        }
        tr.opening, tr.totals, tr.closing { font-weight: bold; }
        tr.opening { background: #fffbe0; }
        tr.totals  { background: #f0f0f0; }
        tr.closing { background: #e8f4ff; }
        .signature { margin-top: 40px; display: table; width: 100%; }
        .signature .sig-cell {
            display: table-cell; width: 50%; vertical-align: bottom;
            padding-top: 30px; text-align: center; font-size: 10pt;
        }
        .signature .sig-line {
            border-top: 1px solid #000; margin: 0 auto; width: 70%; padding-top: 4px;
        }
        .print-btn {
            position: fixed; top: 10px; right: 10px;
            padding: 6px 14px; font-size: 11pt;
        }
        @media print { .print-btn { display: none; } }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <button type="button" class="print-btn" onclick="window.print();">Print</button>

        <div class="letterhead">
            <h1>SIRIMIRI NUTRITION FOOD PRODUCTS PVT. LTD.</h1>
            <div class="sub">
                <asp:Label ID="lblAddress" runat="server" />
            </div>
        </div>

        <div class="title-bar">Account Statement</div>

        <div class="party-block">
            <div class="cell">
                <div class="label">Party</div>
                <strong><asp:Label ID="lblPartyName" runat="server" /></strong>
                <br />
                <asp:Label ID="lblPartyGSTIN" runat="server" />
            </div>
            <div class="cell right">
                <div class="label">Statement Period</div>
                <strong><asp:Label ID="lblPeriod" runat="server" /></strong>
                <br />
                <span style="font-size:9pt;">
                    Generated on <asp:Label ID="lblGeneratedOn" runat="server" />
                </span>
            </div>
        </div>

        <table class="stmt">
            <thead>
                <tr>
                    <th style="width:85px;">Date</th>
                    <th style="width:110px;">Voucher</th>
                    <th>Particulars</th>
                    <th style="width:90px;text-align:right;">Debit</th>
                    <th style="width:90px;text-align:right;">Credit</th>
                    <th style="width:120px;text-align:right;">Balance</th>
                </tr>
            </thead>
            <tbody>
                <asp:PlaceHolder ID="phBody" runat="server" />
            </tbody>
        </table>

        <div class="signature">
            <div class="sig-cell">
                <div class="sig-line">Prepared By</div>
            </div>
            <div class="sig-cell">
                <div class="sig-line">Authorised Signatory</div>
            </div>
        </div>
    </form>
</body>
</html>
