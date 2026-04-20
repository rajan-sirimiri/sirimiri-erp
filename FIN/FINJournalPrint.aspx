<%@ Page Language="C#" AutoEventWireup="true" Inherits="FINApp.FINJournalPrint" %>
<%@ Import Namespace="System.Data" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sirimiri — Journal Voucher Print</title>
<link href="https://fonts.googleapis.com/css2?family=DM+Sans:wght@300;400;500;600;700&display=swap" rel="stylesheet"/>
<!--
    Tally-style Journal Voucher print page.
    Purpose: provide an Indian-accounting-friendly printable voucher for any
    POSTED or REVERSED journal.  Party-backed AP/AR lines display the party
    name as the particulars (Tally convention), with "Accounts Payable" /
    "Accounts Receivable" shown as small "via ..." subtext.

    The page has two action buttons (hidden during print):
      - Print       → window.print()
      - Download PDF → html2pdf.js

    No postbacks beyond initial load.  Closing-tab behaviour is the user's
    expectation; there's no Save / Cancel.
-->
<script src="https://cdnjs.cloudflare.com/ajax/libs/html2pdf.js/0.10.1/html2pdf.bundle.min.js"></script>
<style>
:root{
    --ink:#1a1a1a;
    --muted:#555;
    --line:#000;     /* print pages want hard black borders */
    --accent:#2980b9;
}
*{box-sizing:border-box;margin:0;padding:0;}
html,body{background:#e9ebee;font-family:'DM Sans',sans-serif;color:var(--ink);}

/* Screen-only action bar — does not print */
.toolbar{
    max-width:800px;margin:20px auto 0;padding:12px 16px;background:#1a1a1a;
    color:#fff;border-radius:10px 10px 0 0;display:flex;gap:10px;align-items:center;
}
.toolbar .title{flex:1;font-size:13px;font-weight:600;letter-spacing:.04em;}
.toolbar a.btn, .toolbar button.btn{
    background:transparent;border:1px solid #555;color:#fff;padding:7px 14px;
    border-radius:6px;font-size:12px;font-weight:600;cursor:pointer;text-decoration:none;
    font-family:inherit;
}
.toolbar a.btn:hover, .toolbar button.btn:hover{background:#333;border-color:#888;}
.toolbar .btn-primary{background:var(--accent);border-color:var(--accent);}
.toolbar .btn-primary:hover{background:#1f618d;border-color:#1f618d;}

/* The voucher paper — this is what prints */
.voucher-page{
    max-width:800px;margin:0 auto 40px;background:#fff;padding:28px 32px 24px;
    border:1px solid #ddd;
    box-shadow:0 4px 16px rgba(0,0,0,.08);
    font-size:12.5px;line-height:1.4;color:var(--ink);
}

/* Letterhead */
.vh-head{border-bottom:2px solid var(--line);padding-bottom:12px;margin-bottom:14px;text-align:center;}
.vh-head .co-name{font-size:16px;font-weight:700;letter-spacing:.02em;margin-bottom:4px;}
.vh-head .co-addr{font-size:11px;color:var(--muted);line-height:1.45;}
.vh-head .co-meta{font-size:11px;color:var(--muted);margin-top:4px;}
.vh-head .co-meta b{color:var(--ink);font-weight:600;}

/* Voucher title */
.vh-title{text-align:center;font-size:14px;font-weight:700;letter-spacing:.12em;
    text-transform:uppercase;margin:10px 0 6px;padding-bottom:4px;border-bottom:1px dashed #aaa;}

/* Meta row (No + Date) */
.vh-meta{display:flex;justify-content:space-between;font-size:12px;margin:10px 2px 14px;}
.vh-meta .vm-item{}
.vh-meta .vm-lbl{font-weight:600;color:var(--muted);margin-right:6px;}
.vh-meta .vm-val{font-weight:600;color:var(--ink);font-family:'Courier New',monospace;}

/* Particulars table */
table.vh-lines{width:100%;border-collapse:collapse;margin-bottom:12px;}
table.vh-lines th, table.vh-lines td{
    border:1px solid var(--line);padding:7px 10px;vertical-align:top;
}
table.vh-lines thead th{
    background:#f5f5f5;font-size:11px;text-transform:uppercase;letter-spacing:.04em;
    font-weight:700;text-align:left;
}
table.vh-lines td.cell-drcr{width:60px;text-align:center;font-weight:700;}
table.vh-lines td.cell-particulars{}
table.vh-lines td.cell-amount{width:120px;text-align:right;font-family:'Courier New',monospace;font-weight:600;}
table.vh-lines td.cell-amount-empty{font-family:'Courier New',monospace;color:#999;text-align:right;}

.cell-particulars .party-line{font-weight:600;}
.cell-particulars .via-line{font-size:10.5px;color:var(--muted);margin-top:2px;padding-left:10px;}
.cell-particulars .desc-line{font-size:10.5px;color:var(--muted);margin-top:3px;padding-left:10px;font-style:italic;}

/* Totals row */
tr.vh-total td{background:#f9f9f9;font-weight:700;}
tr.vh-total td.cell-particulars{text-align:right;padding-right:14px;}

/* Narration block */
.vh-narration{
    border:1px solid var(--line);border-top:none;padding:10px 12px;margin-bottom:10px;
    font-size:12px;display:flex;gap:20px;
}
.vh-narration .narr-body{flex:1;}
.vh-narration .narr-lbl{font-weight:600;margin-right:6px;color:var(--muted);text-transform:uppercase;font-size:10px;letter-spacing:.05em;}
.vh-narration .ref-block{min-width:180px;text-align:right;}

/* Amount in words */
.vh-words{
    border:1px solid var(--line);padding:8px 12px;margin-bottom:14px;
    font-size:12px;font-style:italic;
}
.vh-words .w-lbl{font-style:normal;font-weight:600;color:var(--muted);text-transform:uppercase;font-size:10px;letter-spacing:.05em;margin-right:6px;}

/* Signatures footer */
.vh-sig{display:grid;grid-template-columns:1fr 1fr 1fr;gap:20px;margin-top:30px;padding-top:16px;}
.vh-sig .sig-box{border-top:1px solid #666;padding-top:6px;font-size:11px;color:var(--muted);text-align:center;}
.vh-sig .sig-box b{display:block;font-size:10px;text-transform:uppercase;letter-spacing:.06em;color:var(--ink);margin-bottom:4px;font-weight:600;}

/* Reversed / status tag */
.vh-status-tag{display:inline-block;padding:2px 8px;border-radius:4px;font-size:10px;font-weight:700;text-transform:uppercase;letter-spacing:.05em;margin-left:10px;}
.vh-status-tag.posted{background:#e8f7f1;color:#0f6e56;border:1px solid #a7dbc7;}
.vh-status-tag.reversed{background:#fef5eb;color:#b9770e;border:1px solid #f5cba7;}
.vh-reversal-note{background:#fef5eb;border:1px solid #f5cba7;padding:8px 12px;border-radius:6px;font-size:11px;color:#b9770e;margin-bottom:12px;}

/* Error (journal not found) */
.err-page{max-width:600px;margin:60px auto;padding:30px;background:#fff;border:1px solid #ddd;border-radius:10px;text-align:center;}
.err-page h1{font-size:18px;color:#c0392b;margin-bottom:10px;}
.err-page p{font-size:13px;color:var(--muted);margin-bottom:16px;}
.err-page a{color:var(--accent);text-decoration:none;font-weight:600;}

/* ═══ Print styles ═══ */
@media print {
    html, body { background:#fff; }
    .toolbar { display:none !important; }
    .voucher-page{
        max-width:none;margin:0;padding:14mm 14mm 10mm;
        box-shadow:none;border:none;
    }
    /* Force background colors to print */
    tr.vh-total td, table.vh-lines thead th{
        -webkit-print-color-adjust:exact;print-color-adjust:exact;
    }
    /* Avoid orphaned signature block */
    .vh-sig{page-break-inside:avoid;}
}

/* When html2pdf is rendering, it clones the node — hide the toolbar in the
   rendered copy too (in case CSS doesn't apply) */
.pdf-rendering .toolbar{display:none !important;}
</style>
</head>
<body>
<form id="form1" runat="server" style="all:unset;">

<!-- Screen-only toolbar -->
<div class="toolbar" id="printToolbar">
    <div class="title">Journal Voucher: <asp:Literal ID="litTbNumber" runat="server"/></div>
    <asp:HyperLink ID="lnkBack" runat="server" CssClass="btn" NavigateUrl="~/FINJournal.aspx">&#8592; Back</asp:HyperLink>
    <button type="button" class="btn" onclick="downloadPdf()">&#8659; Download PDF</button>
    <button type="button" class="btn btn-primary" onclick="window.print()">&#128424; Print</button>
</div>

<!-- Voucher paper -->
<asp:Panel ID="pnlVoucher" runat="server" CssClass="voucher-page">

    <!-- Letterhead (hardcoded per company request) -->
    <div class="vh-head">
        <div class="co-name">Sirimiri Nutrition Food Products Private Limited</div>
        <div class="co-addr">
            Door No: 2/23B7, Syon Nagar, Tenkasi to Ambai road,<br/>
            Kadayamperumpathu village 2, Tenkasi Taluk,<br/>
            Tenkasi, Tamil Nadu &mdash; 627415
        </div>
        <div class="co-meta">
            <b>GSTIN:</b> 33AAZCS8693P1ZW
            &nbsp;&bull;&nbsp;
            <b>Email:</b> customercare@sirimirihealth.com
            &nbsp;&bull;&nbsp;
            <b>Web:</b> www.SirimiriHealth.com
        </div>
    </div>

    <!-- Title + status -->
    <div class="vh-title">
        Journal Voucher
        <asp:Literal ID="litStatusTag" runat="server"/>
    </div>

    <!-- Reversal notice (if any) -->
    <asp:PlaceHolder ID="phReversalNote" runat="server"/>

    <!-- Voucher No + Date -->
    <div class="vh-meta">
        <div class="vm-item">
            <span class="vm-lbl">Voucher No.</span>
            <span class="vm-val"><asp:Literal ID="litVoucherNo" runat="server"/></span>
        </div>
        <div class="vm-item">
            <span class="vm-lbl">Date</span>
            <span class="vm-val"><asp:Literal ID="litVoucherDate" runat="server"/></span>
        </div>
    </div>

    <!-- Lines table -->
    <table class="vh-lines">
        <thead>
            <tr>
                <th class="cell-drcr" style="width:60px;">Dr/Cr</th>
                <th class="cell-particulars">Particulars</th>
                <th class="cell-amount" style="width:120px;">Debit</th>
                <th class="cell-amount" style="width:120px;">Credit</th>
            </tr>
        </thead>
        <tbody>
            <asp:PlaceHolder ID="phLines" runat="server"/>
            <tr class="vh-total">
                <td>&nbsp;</td>
                <td class="cell-particulars">Total</td>
                <td class="cell-amount"><asp:Literal ID="litTotalDebit" runat="server"/></td>
                <td class="cell-amount"><asp:Literal ID="litTotalCredit" runat="server"/></td>
            </tr>
        </tbody>
    </table>

    <!-- Narration + Reference -->
    <div class="vh-narration">
        <div class="narr-body">
            <span class="narr-lbl">Narration</span>
            <asp:Literal ID="litNarration" runat="server"/>
        </div>
        <div class="ref-block">
            <span class="narr-lbl">Ref</span>
            <asp:Literal ID="litReference" runat="server"/>
        </div>
    </div>

    <!-- Amount in words -->
    <div class="vh-words">
        <span class="w-lbl">Amount in Words</span>
        <asp:Literal ID="litAmountWords" runat="server"/>
    </div>

    <!-- Signature blocks -->
    <div class="vh-sig">
        <div class="sig-box"><b>Entered by</b><asp:Literal ID="litEnteredBy" runat="server"/></div>
        <div class="sig-box"><b>Checked by</b>&nbsp;</div>
        <div class="sig-box"><b>Authorised signatory</b>&nbsp;</div>
    </div>

</asp:Panel>

<!-- Error state (journal not found / not authorised) -->
<asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="err-page">
    <h1>Journal Not Found</h1>
    <p><asp:Literal ID="litErrorMsg" runat="server"/></p>
    <a href="FINJournal.aspx">&larr; Back to Journals</a>
</asp:Panel>

</form>

<script>
function downloadPdf(){
    var el = document.getElementById('<%= pnlVoucher.ClientID %>');
    var filename = 'Journal_<%= VoucherNumberSafe %>.pdf';
    var opt = {
        margin:       [10, 10, 10, 10],  // mm
        filename:     filename,
        image:        { type:'jpeg', quality:0.98 },
        html2canvas:  { scale:2, useCORS:true, letterRendering:true },
        jsPDF:        { unit:'mm', format:'a4', orientation:'portrait' }
    };
    // Add a marker class during render so any screen-only bits stay hidden in the clone
    document.body.classList.add('pdf-rendering');
    html2pdf().set(opt).from(el).save().then(function(){
        document.body.classList.remove('pdf-rendering');
    }).catch(function(){
        document.body.classList.remove('pdf-rendering');
    });
}
</script>
</body>
</html>
