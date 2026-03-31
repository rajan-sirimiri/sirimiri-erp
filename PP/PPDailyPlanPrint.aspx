<%@ Page Language="C#" AutoEventWireup="true" Inherits="PPApp.PPDailyPlanPrint" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
<meta charset="utf-8"/>
<title>Production Plan — <asp:Literal ID="litTitle" runat="server"/></title>
<style>
* { box-sizing:border-box; margin:0; padding:0; }
body { font-family: Arial, sans-serif; font-size: 11pt; color: #000; background: #fff; }

/* HEADER */
.report-header { display:flex; justify-content:space-between; align-items:flex-start;
    border-bottom: 3px solid #000; padding-bottom: 10px; margin-bottom: 16px; }
.company-name { font-size: 18pt; font-weight: 700; letter-spacing: .04em; }
.report-title  { font-size: 13pt; font-weight: 600; color: #333; margin-top: 3px; }
.report-meta   { font-size: 9pt; color: #555; margin-top: 4px; }
.report-date-box { text-align: right; }
.plan-date  { font-size: 14pt; font-weight: 700; }
.plan-status { display:inline-block; font-size: 9pt; font-weight: 700;
    padding: 2px 10px; border-radius: 3px; margin-top: 4px; }
.status-draft     { background: #fff3cd; color: #664d00; border: 1px solid #ffc107; }
.status-confirmed { background: #d1fae5; color: #065f46; border: 1px solid #6ee7b7; }

/* SECTION TITLE */
.section-title { font-size: 11pt; font-weight: 700; letter-spacing: .06em;
    text-transform: uppercase; border-left: 4px solid #000;
    padding-left: 8px; margin: 18px 0 8px; }
.section-title.shift1 { border-color: #27ae60; color: #27ae60; }
.section-title.shift2 { border-color: #2980b9; color: #2980b9; }
.section-title.rm     { border-color: #e74c3c; color: #333; }

/* SHIFT SUMMARY */
.shift-summary { font-size: 9pt; color: #555; margin-bottom: 6px; }

/* TABLES */
table { width: 100%; border-collapse: collapse; font-size: 10pt; margin-bottom: 6px; }
th { background: #1a1a1a; color: #fff; font-size: 9pt; font-weight: 700;
    letter-spacing: .05em; text-transform: uppercase;
    padding: 6px 8px; text-align: left; }
th.num { text-align: right; }
td { padding: 6px 8px; border-bottom: 1px solid #e0e0e0; vertical-align: middle; }
td.num { text-align: right; font-variant-numeric: tabular-nums; }
tr:nth-child(even) td { background: #f9f9f9; }
.prod-name { font-weight: 600; }
.prod-code { font-size: 8pt; color: #666; }
.batch-big { font-size: 13pt; font-weight: 700; color: #27ae60; }
.output-val { font-weight: 600; color: #2980b9; }
.shortfall  { color: #e74c3c; font-weight: 700; }
.surplus    { color: #27ae60; font-weight: 600; }
.empty-msg  { font-size: 10pt; color: #999; padding: 10px 0; font-style: italic; }

/* TWO COLUMN SHIFTS */
.shifts-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }

/* FOOTER */
.report-footer { margin-top: 24px; border-top: 1px solid #ccc; padding-top: 8px;
    font-size: 8pt; color: #999; display: flex; justify-content: space-between; }

/* PRINT */
@media print {
    @page { size: A4 landscape; margin: 15mm; }
    body { font-size: 10pt; }
    .no-print { display: none !important; }
    .shifts-grid { break-inside: avoid; }
    .rm-section { break-before: auto; }
}
@media screen {
    body { max-width: 1100px; margin: 20px auto; padding: 20px; }
    .print-bar { background: #1a1a1a; color: #fff; padding: 10px 20px;
        display: flex; align-items: center; gap: 12px;
        position: fixed; top: 0; left: 0; right: 0; z-index: 100; }
    .print-btn { background: #27ae60; color: #fff; border: none; border-radius: 6px;
        padding: 7px 18px; font-size: 13px; font-weight: 600; cursor: pointer; }
    .print-btn:hover { background: #219a52; }
    .close-btn { background: transparent; color: #ccc; border: 1px solid #555;
        border-radius: 6px; padding: 7px 14px; font-size: 13px; cursor: pointer; }
    .close-btn:hover { color: #fff; border-color: #fff; }
    body { padding-top: 60px; }
}
</style>
    <link rel="stylesheet" href="/StockApp/erp-tablet.css"/>
</head>
<body>

<!-- PRINT BAR (screen only) -->
<div class="print-bar no-print">
    <span style="font-size:14px; font-weight:600;">Production Plan Report</span>
    <button class="print-btn" onclick="window.print()">&#128438; Save as PDF / Print</button>
    <button class="close-btn" onclick="window.close()">&#10005; Close</button>
</div>

<form id="form1" runat="server">

<!-- REPORT HEADER -->
<div class="report-header">
    <div>
        <div class="company-name">Sirimiri Nutrition Food Products</div>
        <div class="report-title">Daily Production Plan</div>
        <div class="report-meta">Generated: <asp:Literal ID="litGenerated" runat="server"/></div>
    </div>
    <div class="report-date-box">
        <div class="plan-date"><asp:Literal ID="litPlanDate" runat="server"/></div>
        <asp:Literal ID="litStatus" runat="server"/>
    </div>
</div>

<!-- SHIFTS GRID -->
<div class="shifts-grid">

    <!-- SHIFT 1 -->
    <div>
        <div class="section-title shift1">Shift 1 — Morning</div>
        <div class="shift-summary"><asp:Literal ID="litS1Summary" runat="server"/></div>
        <asp:Literal ID="litShift1Table" runat="server"/>
    </div>

    <!-- SHIFT 2 -->
    <div>
        <div class="section-title shift2">Shift 2 — Evening</div>
        <div class="shift-summary"><asp:Literal ID="litS2Summary" runat="server"/></div>
        <asp:Literal ID="litShift2Table" runat="server"/>
    </div>

</div>

<!-- RM STATUS -->
<div class="rm-section">
    <div class="section-title rm">Raw Material Requirement vs Stock</div>
    <asp:Literal ID="litRMTable" runat="server"/>
</div>

<!-- FOOTER -->
<div class="report-footer">
    <span>Sirimiri Nutrition Food Products — Confidential</span>
    <span>Printed by: <asp:Literal ID="litUser" runat="server"/></span>
</div>

</form>

<script>
// Auto-trigger print dialog when page loads (optional — remove if not wanted)
// window.addEventListener('load', function() { window.print(); });
</script>
</body>
</html>
