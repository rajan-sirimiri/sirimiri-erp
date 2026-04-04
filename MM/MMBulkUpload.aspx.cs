using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClosedXML.Excel;
using MMApp.DAL;

namespace MMApp
{
    public partial class MMBulkUpload : Page
    {
        protected Label lblNavUser, lblAlert, lblPreviewSummary;
        protected Panel pnlAlert, pnlPreview;
        protected FileUpload fuExcel;
        protected Button btnDownloadTemplate, btnPreview, btnConfirmUpload;
        protected Button btnTabSup, btnTabRM, btnTabPM, btnTabCN, btnTabST;
        protected Repeater rptPreview;
        protected HiddenField hfActiveTab;

        private int UserID => Convert.ToInt32(Session["MM_UserID"]);

        private Dictionary<string, List<BulkRow>> ParsedData
        {
            get { return Session["Bulk_ParsedData"] as Dictionary<string, List<BulkRow>>; }
            set { Session["Bulk_ParsedData"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["MM_UserID"] == null) { Response.Redirect("MMLogin.aspx"); return; }
            string __role = Session["MM_Role"]?.ToString() ?? "";
            if (!MMDatabaseHelper.RoleHasModuleAccess(__role, "MM", "MM_BULK"))
            { Response.Redirect("MMHome.aspx"); return; }
            lblNavUser.Text = Session["MM_FullName"] as string ?? "";
        }

        // ══════════════════════════════════════════════════════════════════
        //  STEP 1 — DOWNLOAD TEMPLATE (all 5 sheets, with UOM reference)
        // ══════════════════════════════════════════════════════════════════

        protected void btnDownloadTemplate_Click(object sender, EventArgs e)
        {
            var uomLookup = BuildUOMLookup();
            // Reverse lookup: UOMID → abbreviation
            var uomReverse = new Dictionary<int, string>();
            foreach (var kv in uomLookup) uomReverse[kv.Value] = kv.Key;

            using (var wb = new XLWorkbook())
            {
                // ── Suppliers ──
                var wsSup = wb.AddWorksheet("Suppliers");
                WriteHeader(wsSup, "SupplierName *", "ContactPerson", "Phone", "Email", "GSTNo", "PAN", "Address", "City", "State", "PinCode");
                int row = 3;
                DataTable supData = MMDatabaseHelper.GetAllSuppliers();
                foreach (DataRow r in supData.Rows)
                {
                    if (r.Table.Columns.Contains("IsActive") && Convert.ToInt32(r["IsActive"]) == 0) continue;
                    wsSup.Cell(row, 1).Value = r["SupplierName"].ToString();
                    wsSup.Cell(row, 2).Value = r.Table.Columns.Contains("ContactPerson") && r["ContactPerson"] != DBNull.Value ? r["ContactPerson"].ToString() : "";
                    wsSup.Cell(row, 3).Value = r.Table.Columns.Contains("Phone") && r["Phone"] != DBNull.Value ? r["Phone"].ToString() : "";
                    wsSup.Cell(row, 4).Value = r.Table.Columns.Contains("Email") && r["Email"] != DBNull.Value ? r["Email"].ToString() : "";
                    wsSup.Cell(row, 5).Value = r.Table.Columns.Contains("GSTNo") && r["GSTNo"] != DBNull.Value ? r["GSTNo"].ToString() : "";
                    wsSup.Cell(row, 6).Value = r.Table.Columns.Contains("PAN") && r["PAN"] != DBNull.Value ? r["PAN"].ToString() : "";
                    wsSup.Cell(row, 7).Value = r.Table.Columns.Contains("Address") && r["Address"] != DBNull.Value ? r["Address"].ToString() : "";
                    wsSup.Cell(row, 8).Value = r.Table.Columns.Contains("City") && r["City"] != DBNull.Value ? r["City"].ToString() : "";
                    wsSup.Cell(row, 9).Value = r.Table.Columns.Contains("State") && r["State"] != DBNull.Value ? r["State"].ToString() : "";
                    wsSup.Cell(row, 10).Value = r.Table.Columns.Contains("PinCode") && r["PinCode"] != DBNull.Value ? r["PinCode"].ToString() : "";
                    StyleExistingRow(wsSup, row, 10);
                    row++;
                }
                FinalizeSheet(wsSup, 10);

                // ── Raw Materials ──
                var wsRM = wb.AddWorksheet("Raw Materials");
                WriteHeader(wsRM, "RawMaterialName *", "UOM *", "HSNCode", "GSTRate", "ReorderLevel", "Description");
                row = 3;
                DataTable rmData = MMDatabaseHelper.GetAllRawMaterials();
                foreach (DataRow r in rmData.Rows)
                {
                    if (r.Table.Columns.Contains("IsActive") && Convert.ToInt32(r["IsActive"]) == 0) continue;
                    wsRM.Cell(row, 1).Value = r["RMName"].ToString();
                    int uomId = Convert.ToInt32(r["UOMID"]);
                    wsRM.Cell(row, 2).Value = uomReverse.ContainsKey(uomId) ? uomReverse[uomId] : "";
                    wsRM.Cell(row, 3).Value = r["HSNCode"] != DBNull.Value ? r["HSNCode"].ToString() : "";
                    wsRM.Cell(row, 4).Value = r["GSTRate"] != DBNull.Value ? r["GSTRate"].ToString() : "";
                    wsRM.Cell(row, 5).Value = r["ReorderLevel"] != DBNull.Value ? r["ReorderLevel"].ToString() : "";
                    wsRM.Cell(row, 6).Value = r["Description"] != DBNull.Value ? r["Description"].ToString() : "";
                    StyleExistingRow(wsRM, row, 6);
                    row++;
                }
                WriteUOMRef(wsRM, uomLookup, 8);
                FinalizeSheet(wsRM, 6);

                // ── Packing Materials ──
                var wsPM = wb.AddWorksheet("Packing Materials");
                WriteHeader(wsPM, "PackingMaterialName *", "UOM *", "Category", "HSNCode", "GSTRate", "ReorderLevel", "Description");
                row = 3;
                DataTable pmData = MMDatabaseHelper.GetAllPackingMaterials();
                foreach (DataRow r in pmData.Rows)
                {
                    if (r.Table.Columns.Contains("IsActive") && Convert.ToInt32(r["IsActive"]) == 0) continue;
                    wsPM.Cell(row, 1).Value = r["PMName"].ToString();
                    int uomId = Convert.ToInt32(r["UOMID"]);
                    wsPM.Cell(row, 2).Value = uomReverse.ContainsKey(uomId) ? uomReverse[uomId] : "";
                    wsPM.Cell(row, 3).Value = r.Table.Columns.Contains("PMCategory") && r["PMCategory"] != DBNull.Value ? r["PMCategory"].ToString() : "";
                    wsPM.Cell(row, 4).Value = r["HSNCode"] != DBNull.Value ? r["HSNCode"].ToString() : "";
                    wsPM.Cell(row, 5).Value = r["GSTRate"] != DBNull.Value ? r["GSTRate"].ToString() : "";
                    wsPM.Cell(row, 6).Value = r["ReorderLevel"] != DBNull.Value ? r["ReorderLevel"].ToString() : "";
                    wsPM.Cell(row, 7).Value = r["Description"] != DBNull.Value ? r["Description"].ToString() : "";
                    StyleExistingRow(wsPM, row, 7);
                    row++;
                }
                WriteUOMRef(wsPM, uomLookup, 9);
                FinalizeSheet(wsPM, 7);

                // ── Consumables ──
                var wsCN = wb.AddWorksheet("Consumables");
                WriteHeader(wsCN, "ConsumableName *", "UOM *", "HSNCode", "GSTRate", "ReorderLevel", "Description");
                row = 3;
                DataTable cnData = MMDatabaseHelper.GetAllConsumables();
                foreach (DataRow r in cnData.Rows)
                {
                    if (r.Table.Columns.Contains("IsActive") && Convert.ToInt32(r["IsActive"]) == 0) continue;
                    wsCN.Cell(row, 1).Value = r["ConsumableName"].ToString();
                    int uomId = Convert.ToInt32(r["UOMID"]);
                    wsCN.Cell(row, 2).Value = uomReverse.ContainsKey(uomId) ? uomReverse[uomId] : "";
                    wsCN.Cell(row, 3).Value = r["HSNCode"] != DBNull.Value ? r["HSNCode"].ToString() : "";
                    wsCN.Cell(row, 4).Value = r["GSTRate"] != DBNull.Value ? r["GSTRate"].ToString() : "";
                    wsCN.Cell(row, 5).Value = r["ReorderLevel"] != DBNull.Value ? r["ReorderLevel"].ToString() : "";
                    wsCN.Cell(row, 6).Value = r["Description"] != DBNull.Value ? r["Description"].ToString() : "";
                    StyleExistingRow(wsCN, row, 6);
                    row++;
                }
                WriteUOMRef(wsCN, uomLookup, 8);
                FinalizeSheet(wsCN, 6);

                // ── Stationaries ──
                var wsST = wb.AddWorksheet("Stationaries");
                WriteHeader(wsST, "StationaryName *", "UOM *", "HSNCode", "GSTRate", "ReorderLevel", "Description");
                row = 3;
                DataTable stData = MMDatabaseHelper.GetAllStationaries();
                foreach (DataRow r in stData.Rows)
                {
                    if (r.Table.Columns.Contains("IsActive") && Convert.ToInt32(r["IsActive"]) == 0) continue;
                    wsST.Cell(row, 1).Value = r["StationaryName"].ToString();
                    int uomId = Convert.ToInt32(r["UOMID"]);
                    wsST.Cell(row, 2).Value = uomReverse.ContainsKey(uomId) ? uomReverse[uomId] : "";
                    wsST.Cell(row, 3).Value = r["HSNCode"] != DBNull.Value ? r["HSNCode"].ToString() : "";
                    wsST.Cell(row, 4).Value = r["GSTRate"] != DBNull.Value ? r["GSTRate"].ToString() : "";
                    wsST.Cell(row, 5).Value = r["ReorderLevel"] != DBNull.Value ? r["ReorderLevel"].ToString() : "";
                    wsST.Cell(row, 6).Value = r["Description"] != DBNull.Value ? r["Description"].ToString() : "";
                    StyleExistingRow(wsST, row, 6);
                    row++;
                }
                WriteUOMRef(wsST, uomLookup, 8);
                FinalizeSheet(wsST, 6);

                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("Content-Disposition", "attachment; filename=MM_BulkUpload_Template.xlsx");
                using (var ms = new MemoryStream()) { wb.SaveAs(ms); ms.WriteTo(Response.OutputStream); }
                Response.End();
            }
        }

        void WriteHeader(IXLWorksheet ws, params string[] headers)
        {
            ws.Cell(1, 1).Value = "Existing data shown below (gray). Add new items at the bottom. Duplicates are skipped on import.";
            ws.Range("A1:" + GetColLetter(headers.Length) + "1").Merge().Style.Font.SetItalic(true).Font.SetFontColor(XLColor.Gray);
            for (int c = 0; c < headers.Length; c++)
            {
                ws.Cell(2, c + 1).Value = headers[c];
                ws.Cell(2, c + 1).Style.Font.SetBold(true).Font.SetFontColor(XLColor.White)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#2C3E50"))
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            }
        }

        void StyleExistingRow(IXLWorksheet ws, int row, int cols)
        {
            for (int c = 1; c <= cols; c++)
            {
                ws.Cell(row, c).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F5F5F5"));
                ws.Cell(row, c).Style.Font.SetFontColor(XLColor.FromHtml("#666666"));
            }
        }
        void WriteSample(IXLWorksheet ws, int row, params string[] vals)
        {
            for (int c = 0; c < vals.Length; c++)
            {
                ws.Cell(row, c + 1).Value = vals[c];
                ws.Cell(row, c + 1).Style.Font.SetFontColor(XLColor.FromHtml("#0000FF"));
            }
        }
        void WriteUOMRef(IXLWorksheet ws, Dictionary<string, int> uomLookup, int startCol)
        {
            ws.Cell(2, startCol).Value = "Valid UOM";
            ws.Cell(2, startCol).Style.Font.SetBold(true).Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromHtml("#7f8c8d"));
            int r = 3;
            foreach (var kv in uomLookup) { ws.Cell(r, startCol).Value = kv.Key; r++; }
        }
        void FinalizeSheet(IXLWorksheet ws, int numCols)
        {
            ws.Columns().AdjustToContents();
            if (ws.Column(1).Width < 25) ws.Column(1).Width = 25;
            ws.SheetView.FreezeRows(2);
        }
        string GetColLetter(int col) { return ((char)(64 + col)).ToString(); }

        // ══════════════════════════════════════════════════════════════════
        //  STEP 2 — PREVIEW & VALIDATE
        // ══════════════════════════════════════════════════════════════════

        protected void btnPreview_Click(object sender, EventArgs e)
        {
            if (!fuExcel.HasFile) { ShowAlert("Please select an Excel file.", false); return; }
            if (!fuExcel.FileName.ToLower().EndsWith(".xlsx")) { ShowAlert("Only .xlsx files are supported.", false); return; }

            try
            {
                var parsed = new Dictionary<string, List<BulkRow>>();
                var uomLookup = BuildUOMLookup();
                var existingNames = new Dictionary<string, HashSet<string>>();
                existingNames["SUP"] = GetExistingNames("SUP");
                existingNames["RM"] = GetExistingNames("RM");
                existingNames["PM"] = GetExistingNames("PM");
                existingNames["CN"] = GetExistingNames("CN");
                existingNames["ST"] = GetExistingNames("ST");

                using (var ms = new MemoryStream(fuExcel.FileBytes))
                using (var wb = new XLWorkbook(ms))
                {
                    var sheetMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                        {"Suppliers","SUP"}, {"Raw Materials","RM"}, {"Packing Materials","PM"},
                        {"Consumables","CN"}, {"Stationaries","ST"}
                    };

                    foreach (var ws in wb.Worksheets)
                    {
                        string typeCode = null;
                        foreach (var kv in sheetMap)
                            if (ws.Name.IndexOf(kv.Key, StringComparison.OrdinalIgnoreCase) >= 0) { typeCode = kv.Value; break; }
                        if (typeCode == null) continue;

                        var rows = new List<BulkRow>();
                        int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;
                        var existing = existingNames.ContainsKey(typeCode) ? existingNames[typeCode] : new HashSet<string>();

                        for (int r = 3; r <= lastRow; r++)
                        {
                            string c1 = ws.Cell(r, 1).GetString().Trim();
                            if (string.IsNullOrEmpty(c1)) continue;

                            var row = new BulkRow
                            {
                                RowNum = r,
                                Col1 = c1,
                                Col2 = ws.Cell(r, 2).GetString().Trim(),
                                Col3 = ws.Cell(r, 3).GetString().Trim(),
                                Col4 = ws.Cell(r, 4).GetString().Trim(),
                                Col5 = ws.Cell(r, 5).GetString().Trim(),
                                Col6 = ws.Cell(r, 6).GetString().Trim(),
                                Col7 = ws.Cell(r, 7).GetString().Trim(),
                                Col8 = ws.Cell(r, 8).GetString().Trim(),
                                Col9 = ws.Cell(r, 9).GetString().Trim(),
                                Col10 = ws.Cell(r, 10).GetString().Trim(),
                                TypeCode = typeCode
                            };

                            row.Validate(existing, uomLookup);
                            rows.Add(row);
                        }
                        parsed[typeCode] = rows;
                    }
                }

                if (parsed.Count == 0)
                { ShowAlert("No valid sheets found.", false); return; }

                ParsedData = parsed;
                hfActiveTab.Value = "SUP";
                BindPreview("SUP");
                pnlPreview.Visible = true;
                SetActiveTabStyle("SUP");

                int totalRows = 0, validRows = 0;
                foreach (var kv in parsed) { totalRows += kv.Value.Count; validRows += kv.Value.FindAll(r => r.IsValid).Count; }
                ShowAlert("File parsed: " + totalRows + " rows found, " + validRows + " ready to import.", validRows == totalRows);
            }
            catch (Exception ex) { ShowAlert("Error reading file: " + ex.Message, false); }
        }

        // ══════════════════════════════════════════════════════════════════
        //  TAB SWITCHING
        // ══════════════════════════════════════════════════════════════════

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
            if (btnTabSup != null) btnTabSup.CssClass = tab == "SUP" ? "tab-btn active" : "tab-btn";
            if (btnTabRM != null) btnTabRM.CssClass = tab == "RM" ? "tab-btn active" : "tab-btn";
            if (btnTabPM != null) btnTabPM.CssClass = tab == "PM" ? "tab-btn active" : "tab-btn";
            if (btnTabCN != null) btnTabCN.CssClass = tab == "CN" ? "tab-btn active" : "tab-btn";
            if (btnTabST != null) btnTabST.CssClass = tab == "ST" ? "tab-btn active" : "tab-btn";
        }

        void BindPreview(string typeCode)
        {
            var data = ParsedData;
            if (data == null || !data.ContainsKey(typeCode))
            {
                rptPreview.DataSource = CreateEmptyDT();
                rptPreview.DataBind();
                lblPreviewSummary.Text = "No data found for this type.";
                return;
            }

            var rows = data[typeCode];
            var dt = CreateEmptyDT();
            int valid = 0, skip = 0;
            foreach (var r in rows)
            {
                dt.Rows.Add(r.RowNum, r.Col1, r.Col2, r.Col3, r.Col4, r.Col5, r.Col6, r.IsValid, r.StatusMsg);
                if (r.IsValid) valid++;
                if (r.StatusMsg.StartsWith("Skip")) skip++;
            }

            rptPreview.DataSource = dt;
            rptPreview.DataBind();

            string typeName = typeCode == "SUP" ? "Suppliers" : typeCode == "RM" ? "Raw Materials" :
                typeCode == "PM" ? "Packing Materials" : typeCode == "CN" ? "Consumables" : "Stationaries";
            lblPreviewSummary.Text = typeName + ": " + rows.Count + " rows — " + valid + " new, " + skip + " duplicates";
        }

        DataTable CreateEmptyDT()
        {
            var dt = new DataTable();
            dt.Columns.Add("RowNum", typeof(int));
            dt.Columns.Add("Col1"); dt.Columns.Add("Col2"); dt.Columns.Add("Col3");
            dt.Columns.Add("Col4"); dt.Columns.Add("Col5"); dt.Columns.Add("Col6");
            dt.Columns.Add("IsValid", typeof(bool)); dt.Columns.Add("StatusMsg");
            return dt;
        }

        // ══════════════════════════════════════════════════════════════════
        //  STEP 3 — CONFIRM & IMPORT
        // ══════════════════════════════════════════════════════════════════

        protected void btnConfirmUpload_Click(object sender, EventArgs e)
        {
            var data = ParsedData;
            if (data == null) { ShowAlert("No data to import. Please upload first.", false); return; }

            var uomLookup = BuildUOMLookup();
            int saved = 0, skipped = 0;
            var errors = new List<string>();

            foreach (var kv in data)
            {
                string typeCode = kv.Key;
                foreach (var row in kv.Value)
                {
                    if (!row.IsValid) { skipped++; continue; }

                    try
                    {
                        switch (typeCode)
                        {
                            case "SUP":
                                MMDatabaseHelper.AddSupplier(row.Col1, row.Col2, row.Col3, row.Col4, row.Col5, row.Col6, row.Col7,
                                    row.GetVal(8), row.GetVal(9), row.GetVal(10));
                                break;
                            case "RM":
                                int rmUom = uomLookup.ContainsKey(row.Col2.ToLower()) ? uomLookup[row.Col2.ToLower()] : 0;
                                if (rmUom == 0) { errors.Add(row.Col1 + ": invalid UOM"); skipped++; continue; }
                                decimal? rmGst = ParseDecimal(row.Col4);
                                decimal rmReorder = 0; decimal.TryParse(row.Col5, out rmReorder);
                                MMDatabaseHelper.AddRawMaterial(row.Col1, row.Col6, row.Col3, rmGst, rmUom, rmReorder);
                                break;
                            case "PM":
                                int pmUom = uomLookup.ContainsKey(row.Col2.ToLower()) ? uomLookup[row.Col2.ToLower()] : 0;
                                if (pmUom == 0) { errors.Add(row.Col1 + ": invalid UOM"); skipped++; continue; }
                                decimal? pmGst = ParseDecimal(row.Col5);
                                decimal pmReorder = 0; decimal.TryParse(row.Col6, out pmReorder);
                                MMDatabaseHelper.AddPackingMaterial(row.Col1, row.Col7, row.Col4, pmGst, pmUom, pmReorder, row.Col3);
                                break;
                            case "CN":
                                int cnUom = uomLookup.ContainsKey(row.Col2.ToLower()) ? uomLookup[row.Col2.ToLower()] : 0;
                                if (cnUom == 0) { errors.Add(row.Col1 + ": invalid UOM"); skipped++; continue; }
                                decimal? cnGst = ParseDecimal(row.Col4);
                                decimal cnReorder = 0; decimal.TryParse(row.Col5, out cnReorder);
                                MMDatabaseHelper.AddConsumable(row.Col1, row.Col6, row.Col3, cnGst, cnUom, cnReorder);
                                break;
                            case "ST":
                                int stUom = uomLookup.ContainsKey(row.Col2.ToLower()) ? uomLookup[row.Col2.ToLower()] : 0;
                                if (stUom == 0) { errors.Add(row.Col1 + ": invalid UOM"); skipped++; continue; }
                                decimal? stGst = ParseDecimal(row.Col4);
                                decimal stReorder = 0; decimal.TryParse(row.Col5, out stReorder);
                                MMDatabaseHelper.AddStationary(row.Col1, row.Col6, row.Col3, stGst, stUom, stReorder);
                                break;
                        }
                        saved++;
                    }
                    catch (Exception ex) { errors.Add(row.Col1 + ": " + ex.Message); }
                }
            }

            ParsedData = null;
            pnlPreview.Visible = false;

            string msg = saved + " records imported.";
            if (skipped > 0) msg += " " + skipped + " skipped.";
            if (errors.Count > 0) msg += " Errors: " + string.Join("; ", errors.GetRange(0, Math.Min(5, errors.Count)));
            ShowAlert(msg, errors.Count == 0);
        }

        // ══════════════════════════════════════════════════════════════════
        //  HELPERS
        // ══════════════════════════════════════════════════════════════════

        Dictionary<string, int> BuildUOMLookup()
        {
            var lookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in MMDatabaseHelper.GetActiveUOM().Rows)
                lookup[row["Abbreviation"].ToString().ToLower()] = Convert.ToInt32(row["UOMID"]);
            return lookup;
        }

        HashSet<string> GetExistingNames(string typeCode)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            DataTable dt; string nameCol;
            switch (typeCode)
            {
                case "SUP": dt = MMDatabaseHelper.GetAllSuppliers(); nameCol = "SupplierName"; break;
                case "RM": dt = MMDatabaseHelper.GetAllRawMaterials(); nameCol = "RMName"; break;
                case "PM": dt = MMDatabaseHelper.GetAllPackingMaterials(); nameCol = "PMName"; break;
                case "CN": dt = MMDatabaseHelper.GetAllConsumables(); nameCol = "ConsumableName"; break;
                case "ST": dt = MMDatabaseHelper.GetAllStationaries(); nameCol = "StationaryName"; break;
                default: return set;
            }
            foreach (DataRow row in dt.Rows)
                if (dt.Columns.Contains(nameCol)) set.Add(row[nameCol].ToString());
            return set;
        }

        decimal? ParseDecimal(string s)
        {
            decimal v;
            return decimal.TryParse(s, out v) ? (decimal?)v : null;
        }

        void ShowAlert(string m, bool ok)
        {
            if (lblAlert != null) lblAlert.Text = m;
            if (pnlAlert != null) { pnlAlert.CssClass = "alert " + (ok ? "alert-success" : "alert-danger"); pnlAlert.Visible = true; }
        }

        // ══════════════════════════════════════════════════════════════════
        //  DATA CLASS
        // ══════════════════════════════════════════════════════════════════

        [Serializable]
        public class BulkRow
        {
            public int RowNum;
            public string Col1, Col2, Col3, Col4, Col5, Col6, Col7, Col8, Col9, Col10;
            public string TypeCode;
            public bool IsValid;
            public string StatusMsg;

            // For Suppliers which have 10 columns — store extras
            private string[] _extras;
            public string GetVal(int colNum)
            {
                switch (colNum)
                {
                    case 1: return Col1; case 2: return Col2; case 3: return Col3;
                    case 4: return Col4; case 5: return Col5; case 6: return Col6;
                    case 7: return Col7; case 8: return Col8 ?? "";
                    case 9: return Col9 ?? ""; case 10: return Col10 ?? "";
                    default: return "";
                }
            }

            public void Validate(HashSet<string> existingNames, Dictionary<string, int> uomLookup)
            {
                IsValid = false;
                string name = Col1;

                if (string.IsNullOrEmpty(name))
                { StatusMsg = "Missing name"; return; }

                if (existingNames.Contains(name))
                { StatusMsg = "Skip — already exists"; return; }

                if (TypeCode != "SUP")
                {
                    // Validate UOM
                    string uom = (Col2 ?? "").Trim().ToLower();
                    if (string.IsNullOrEmpty(uom) || !uomLookup.ContainsKey(uom))
                    { StatusMsg = "Invalid or missing UOM"; return; }
                }

                IsValid = true;
                StatusMsg = "✓ Ready";
            }
        }
    }
}
