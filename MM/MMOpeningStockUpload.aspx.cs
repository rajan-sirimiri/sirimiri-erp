using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClosedXML.Excel;
using MMApp.DAL;

namespace MMApp
{
    public partial class MMOpeningStockUpload : Page
    {
        protected Label lblUser, lblAlert, lblPreviewSummary;
        protected Panel pnlAlert, pnlPreview;
        protected FileUpload fuExcel;
        protected Button btnDownloadTemplate, btnPreview, btnConfirmUpload;
        protected Button btnTabRM, btnTabPM, btnTabCN, btnTabST;
        protected Repeater rptPreview;
        protected HiddenField hfActiveTab;

        private int UserID => Convert.ToInt32(Session["MM_UserID"]);

        // Store parsed data across postbacks
        private Dictionary<string, List<StockRow>> ParsedData
        {
            get { return Session["OS_ParsedData"] as Dictionary<string, List<StockRow>>; }
            set { Session["OS_ParsedData"] = value; }
        }

        protected void Page_Load(object s, EventArgs e)
        {
            if (Session["MM_UserID"] == null)
            { Response.Redirect("MMLogin.aspx"); return; }
            if (lblUser != null) lblUser.Text = Session["MM_FullName"] as string ?? "";
        }

        // ── DOWNLOAD TEMPLATE ────────────────────────────────────────────

        protected void btnDownloadTemplate_Click(object s, EventArgs e)
        {
            using (var wb = new XLWorkbook())
            {
                string today = DateTime.Now.ToString("dd-MM-yyyy");

                // ── Raw Materials ──
                var wsRM = wb.AddWorksheet("Raw Materials");
                WriteSheetHeader(wsRM);
                DataTable rmData = MMDatabaseHelper.GetActiveRawMaterials();
                int row = 3;
                foreach (DataRow r in rmData.Rows)
                {
                    wsRM.Cell(row, 1).Value = r["RMCode"].ToString();
                    wsRM.Cell(row, 2).Value = r["RMName"].ToString();
                    wsRM.Cell(row, 1).Style.Font.SetFontColor(XLColor.FromHtml("#333333")).Font.SetBold(true);
                    wsRM.Cell(row, 2).Style.Font.SetFontColor(XLColor.FromHtml("#666666"));
                    wsRM.Cell(row, 5).Value = today;
                    row++;
                }
                FinalizeSheet(wsRM);

                // ── Packing Materials ──
                var wsPM = wb.AddWorksheet("Packing Materials");
                WriteSheetHeader(wsPM);
                DataTable pmData = MMDatabaseHelper.GetAllPackingMaterials();
                row = 3;
                foreach (DataRow r in pmData.Rows)
                {
                    if (Convert.ToBoolean(r["IsActive"]) == false) continue;
                    wsPM.Cell(row, 1).Value = r["PMCode"].ToString();
                    wsPM.Cell(row, 2).Value = r["PMName"].ToString();
                    wsPM.Cell(row, 1).Style.Font.SetFontColor(XLColor.FromHtml("#333333")).Font.SetBold(true);
                    wsPM.Cell(row, 2).Style.Font.SetFontColor(XLColor.FromHtml("#666666"));
                    wsPM.Cell(row, 5).Value = today;
                    row++;
                }
                FinalizeSheet(wsPM);

                // ── Consumables ──
                var wsCN = wb.AddWorksheet("Consumables");
                WriteSheetHeader(wsCN);
                DataTable cnData = MMDatabaseHelper.GetAllConsumables();
                row = 3;
                foreach (DataRow r in cnData.Rows)
                {
                    if (Convert.ToBoolean(r["IsActive"]) == false) continue;
                    wsCN.Cell(row, 1).Value = r["ConsumableCode"].ToString();
                    wsCN.Cell(row, 2).Value = r["ConsumableName"].ToString();
                    wsCN.Cell(row, 1).Style.Font.SetFontColor(XLColor.FromHtml("#333333")).Font.SetBold(true);
                    wsCN.Cell(row, 2).Style.Font.SetFontColor(XLColor.FromHtml("#666666"));
                    wsCN.Cell(row, 5).Value = today;
                    row++;
                }
                FinalizeSheet(wsCN);

                // ── Stationaries ──
                var wsST = wb.AddWorksheet("Stationaries");
                WriteSheetHeader(wsST);
                DataTable stData = MMDatabaseHelper.GetAllStationaries();
                row = 3;
                foreach (DataRow r in stData.Rows)
                {
                    if (Convert.ToBoolean(r["IsActive"]) == false) continue;
                    wsST.Cell(row, 1).Value = r["StationaryCode"].ToString();
                    wsST.Cell(row, 2).Value = r["StationaryName"].ToString();
                    wsST.Cell(row, 1).Style.Font.SetFontColor(XLColor.FromHtml("#333333")).Font.SetBold(true);
                    wsST.Cell(row, 2).Style.Font.SetFontColor(XLColor.FromHtml("#666666"));
                    wsST.Cell(row, 5).Value = today;
                    row++;
                }
                FinalizeSheet(wsST);

                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("Content-Disposition", "attachment; filename=OpeningStock_Template.xlsx");
                using (var ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    ms.WriteTo(Response.OutputStream);
                }
                Response.End();
            }
        }

        void WriteSheetHeader(IXLWorksheet ws)
        {
            ws.Cell(1, 1).Value = "Fill Quantity, Rate, Date. Code and Name are pre-populated. Do not change the Code column.";
            ws.Range("A1:F1").Merge().Style.Font.SetItalic(true).Font.SetFontColor(XLColor.Gray);

            string[] headers = { "Material Code *", "Material Name", "Quantity *", "Rate (₹) *", "As Of Date (DD-MM-YYYY) *", "Remarks" };
            for (int c = 0; c < headers.Length; c++)
            {
                ws.Cell(2, c + 1).Value = headers[c];
                ws.Cell(2, c + 1).Style.Font.SetBold(true).Font.SetFontColor(XLColor.White)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#2C3E50"))
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            }
        }

        void FinalizeSheet(IXLWorksheet ws)
        {
            ws.Column(1).Width = 18;
            ws.Column(2).Width = 40;
            ws.Column(3).Width = 14;
            ws.Column(4).Width = 14;
            ws.Column(5).Width = 24;
            ws.Column(6).Width = 28;
            ws.SheetView.FreezeRows(2);
            // Protect code and name columns (light gray background)
            int lastRow = ws.LastRowUsed()?.RowNumber() ?? 2;
            if (lastRow > 2)
            {
                ws.Range(3, 1, lastRow, 2).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F5F5F5"));
            }
        }

        // ── PREVIEW & VALIDATE ───────────────────────────────────────────

        protected void btnPreview_Click(object s, EventArgs e)
        {
            if (!fuExcel.HasFile)
            { ShowAlert("Please select an Excel file.", false); return; }

            string ext = Path.GetExtension(fuExcel.FileName).ToLower();
            if (ext != ".xlsx")
            { ShowAlert("Only .xlsx files are supported.", false); return; }

            try
            {
                var parsed = new Dictionary<string, List<StockRow>>();

                using (var ms = new MemoryStream(fuExcel.FileBytes))
                using (var wb = new XLWorkbook(ms))
                {
                    // Map sheet names to material types
                    var sheetMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                        { "Raw Materials", "RM" }, { "Packing Materials", "PM" },
                        { "Consumables", "CN" }, { "Stationaries", "ST" }
                    };

                    foreach (var ws in wb.Worksheets)
                    {
                        string matType = null;
                        foreach (var kv in sheetMap)
                            if (ws.Name.IndexOf(kv.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                            { matType = kv.Value; break; }
                        if (matType == null) continue;

                        var rows = new List<StockRow>();
                        int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;

                        for (int r = 3; r <= lastRow; r++) // Skip header rows 1-2
                        {
                            string code = ws.Cell(r, 1).GetString().Trim();
                            if (string.IsNullOrEmpty(code)) continue;

                            string name = ws.Cell(r, 2).GetString().Trim();
                            string qtyStr = ws.Cell(r, 3).GetString().Trim();
                            string rateStr = ws.Cell(r, 4).GetString().Trim();
                            string dateStr = ws.Cell(r, 5).GetString().Trim();
                            string remarks = ws.Cell(r, 6).GetString().Trim();

                            var row = new StockRow
                            {
                                RowNum = r,
                                Code = code,
                                Name = name,
                                QtyStr = qtyStr,
                                RateStr = rateStr,
                                DateStr = dateStr,
                                Remarks = remarks,
                                MaterialType = matType
                            };

                            // Validate
                            row.Validate();
                            rows.Add(row);
                        }
                        parsed[matType] = rows;
                    }
                }

                if (parsed.Count == 0)
                { ShowAlert("No valid sheets found. Expected sheets: Raw Materials, Packing Materials, Consumables, Stationaries.", false); return; }

                ParsedData = parsed;
                hfActiveTab.Value = "RM";
                BindPreview("RM");
                pnlPreview.Visible = true;
                SetActiveTabStyle("RM");

                int totalRows = 0, validRows = 0;
                foreach (var kv in parsed) { totalRows += kv.Value.Count; validRows += kv.Value.FindAll(r => r.IsValid).Count; }
                ShowAlert("File parsed: " + totalRows + " rows found, " + validRows + " valid.", validRows == totalRows);
            }
            catch (Exception ex)
            {
                ShowAlert("Error reading file: " + ex.Message, false);
            }
        }

        // ── TAB SWITCHING ────────────────────────────────────────────────

        protected void btnTab_Click(object s, EventArgs e)
        {
            string tab = ((Button)s).CommandArgument;
            hfActiveTab.Value = tab;
            BindPreview(tab);
            SetActiveTabStyle(tab);
            pnlPreview.Visible = true;
        }

        void SetActiveTabStyle(string tab)
        {
            if (btnTabRM != null) btnTabRM.CssClass = tab == "RM" ? "tab-btn active" : "tab-btn";
            if (btnTabPM != null) btnTabPM.CssClass = tab == "PM" ? "tab-btn active" : "tab-btn";
            if (btnTabCN != null) btnTabCN.CssClass = tab == "CN" ? "tab-btn active" : "tab-btn";
            if (btnTabST != null) btnTabST.CssClass = tab == "ST" ? "tab-btn active" : "tab-btn";
        }

        void BindPreview(string matType)
        {
            var data = ParsedData;
            if (data == null || !data.ContainsKey(matType))
            {
                var empty = new DataTable();
                empty.Columns.Add("RowNum"); empty.Columns.Add("Code"); empty.Columns.Add("Name");
                empty.Columns.Add("Qty"); empty.Columns.Add("Rate"); empty.Columns.Add("DateStr");
                empty.Columns.Add("Remarks"); empty.Columns.Add("IsValid"); empty.Columns.Add("StatusMsg");
                rptPreview.DataSource = empty;
                rptPreview.DataBind();
                lblPreviewSummary.Text = "No data found for this type.";
                return;
            }

            var rows = data[matType];
            var dt = new DataTable();
            dt.Columns.Add("RowNum", typeof(int));
            dt.Columns.Add("Code"); dt.Columns.Add("Name");
            dt.Columns.Add("Qty"); dt.Columns.Add("Rate");
            dt.Columns.Add("DateStr"); dt.Columns.Add("Remarks");
            dt.Columns.Add("IsValid", typeof(bool)); dt.Columns.Add("StatusMsg");

            int valid = 0;
            foreach (var r in rows)
            {
                dt.Rows.Add(r.RowNum, r.Code, r.Name,
                    r.Qty.ToString("0.###"), r.Rate.ToString("0.00"),
                    r.DateStr, r.Remarks, r.IsValid, r.StatusMsg);
                if (r.IsValid) valid++;
            }

            rptPreview.DataSource = dt;
            rptPreview.DataBind();

            string typeName = matType == "RM" ? "Raw Materials" : matType == "PM" ? "Packing Materials" :
                matType == "CN" ? "Consumables" : "Stationaries";
            lblPreviewSummary.Text = typeName + ": " + rows.Count + " rows, " + valid + " valid";
        }

        // ── CONFIRM & SAVE ───────────────────────────────────────────────

        protected void btnConfirmUpload_Click(object s, EventArgs e)
        {
            var data = ParsedData;
            if (data == null) { ShowAlert("No data to save. Please upload first.", false); return; }

            int saved = 0, skipped = 0;
            var errors = new List<string>();

            foreach (var kv in data)
            {
                string matType = kv.Key;
                foreach (var row in kv.Value)
                {
                    if (!row.IsValid) { skipped++; continue; }

                    // Resolve MaterialID from code
                    int materialId = ResolveMaterialId(matType, row.Code);
                    if (materialId == 0)
                    {
                        errors.Add(matType + ": " + row.Code + " not found in system");
                        skipped++;
                        continue;
                    }

                    try
                    {
                        MMDatabaseHelper.SaveOpeningStock(matType, materialId, row.Qty, row.Rate, row.AsOfDate, row.Remarks, UserID);
                        saved++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add(matType + " " + row.Code + ": " + ex.Message);
                        skipped++;
                    }
                }
            }

            string msg = saved + " records saved successfully.";
            if (skipped > 0) msg += " " + skipped + " skipped.";
            if (errors.Count > 0) msg += " Errors: " + string.Join("; ", errors.GetRange(0, Math.Min(5, errors.Count)));
            ShowAlert(msg, errors.Count == 0);

            ParsedData = null;
            pnlPreview.Visible = false;
        }

        int ResolveMaterialId(string matType, string code)
        {
            string sql = "";
            switch (matType)
            {
                case "RM": sql = "SELECT RMID FROM MM_RawMaterials WHERE RMCode=?c;"; break;
                case "PM": sql = "SELECT PMID FROM MM_PackingMaterials WHERE PMCode=?c;"; break;
                case "CN": sql = "SELECT ConsumableID FROM MM_Consumables WHERE ConsumableCode=?c;"; break;
                case "ST": sql = "SELECT StationaryID FROM MM_Stationaries WHERE StationaryCode=?c;"; break;
                default: return 0;
            }
            var row = MMDatabaseHelper.ExecuteQuerySingleRowPublic(sql,
                new MySql.Data.MySqlClient.MySqlParameter("?c", code));
            if (row == null) return 0;
            return Convert.ToInt32(row[0]);
        }

        // ── HELPERS ──────────────────────────────────────────────────────

        void ShowAlert(string m, bool ok)
        {
            if (lblAlert != null) lblAlert.Text = m;
            if (pnlAlert != null) { pnlAlert.CssClass = "alert " + (ok ? "alert-success" : "alert-danger"); pnlAlert.Visible = true; }
        }

        // ── DATA CLASS ───────────────────────────────────────────────────

        [Serializable]
        public class StockRow
        {
            public int RowNum;
            public string Code, Name, QtyStr, RateStr, DateStr, Remarks, MaterialType;
            public decimal Qty, Rate;
            public DateTime AsOfDate;
            public bool IsValid;
            public string StatusMsg;

            public void Validate()
            {
                var issues = new List<string>();

                if (string.IsNullOrEmpty(Code)) issues.Add("Missing code");

                if (!decimal.TryParse(QtyStr, out Qty) || Qty < 0)
                    issues.Add("Invalid quantity");

                if (!decimal.TryParse(RateStr, out Rate) || Rate < 0)
                    issues.Add("Invalid rate");

                // Parse date — try multiple formats
                bool dateParsed = false;
                string[] formats = { "dd-MM-yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy", "d-M-yyyy", "d/M/yyyy" };
                foreach (string fmt in formats)
                {
                    if (DateTime.TryParseExact(DateStr, fmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out AsOfDate))
                    { dateParsed = true; break; }
                }
                if (!dateParsed)
                {
                    // Try general parse
                    if (DateTime.TryParse(DateStr, out AsOfDate))
                        dateParsed = true;
                    else
                        issues.Add("Invalid date");
                }

                IsValid = issues.Count == 0;
                StatusMsg = IsValid ? "✓ Valid" : string.Join(", ", issues);
            }
        }
    }
}
