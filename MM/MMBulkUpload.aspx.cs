using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using MMApp.DAL;
using ClosedXML.Excel;

namespace MMApp
{
    public partial class MMBulkUpload : Page
    {
        protected global::System.Web.UI.WebControls.Label        lblNavUser;
        protected global::System.Web.UI.WebControls.HiddenField  hfMasterType;
        protected global::System.Web.UI.WebControls.Panel        pnlAlert;
        protected global::System.Web.UI.WebControls.Label        lblAlert;
        protected global::System.Web.UI.WebControls.Panel        pnlTemplateDownload;
        protected global::System.Web.UI.WebControls.Label        lblTemplateName;
        protected global::System.Web.UI.WebControls.Label        lblTemplateDesc;
        protected global::System.Web.UI.WebControls.Button       btnSelectMaster;
        protected global::System.Web.UI.WebControls.Button       btnDownloadTemplate;
        protected global::System.Web.UI.WebControls.Panel        pnlUploadSection;
        protected global::System.Web.UI.WebControls.FileUpload   fuExcel;
        protected global::System.Web.UI.WebControls.Button       btnPreview;
        protected global::System.Web.UI.WebControls.Panel        pnlPreview;
        protected global::System.Web.UI.WebControls.Label        lblTotalRows;
        protected global::System.Web.UI.WebControls.Label        lblNewRows;
        protected global::System.Web.UI.WebControls.Label        lblSkipRows;
        protected global::System.Web.UI.WebControls.Label        lblErrRows;
        protected global::System.Web.UI.WebControls.Literal      litPreviewTable;
        protected global::System.Web.UI.WebControls.Button       btnImport;
        protected global::System.Web.UI.WebControls.Button       btnReset;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["MM_UserID"] == null) { Response.Redirect("MMLogin.aspx"); return; }
            lblNavUser.Text = Session["MM_FullName"] as string ?? "";
        }

        protected void btnSelectMaster_Click(object sender, EventArgs e)
        {
            pnlPreview.Visible = false;
            pnlAlert.Visible   = false;
            ShowTemplateSection();
        }

        private void ShowTemplateSection()
        {
            string type = hfMasterType.Value;
            if (string.IsNullOrEmpty(type)) return;
            pnlTemplateDownload.Visible = true;
            pnlUploadSection.Visible    = true;
            switch (type)
            {
                case "Supplier":
                    lblTemplateName.Text = "Supplier Registration Template";
                    lblTemplateDesc.Text = "Columns: SupplierName*, ContactPerson, Phone, Email, GSTNo, PAN, Address, City, State, PinCode";
                    break;
                case "RawMaterial":
                    lblTemplateName.Text = "Raw Material Template";
                    lblTemplateDesc.Text = "Columns: RawMaterialName*, UOM* (Abbreviation), HSNCode, GSTRate, ReorderLevel, Description";
                    break;
                case "PackingMaterial":
                    lblTemplateName.Text = "Packing Material Template";
                    lblTemplateDesc.Text = "Columns: PackingMaterialName*, UOM* (Abbreviation), HSNCode, GSTRate, ReorderLevel, Description";
                    break;
                case "Consumable":
                    lblTemplateName.Text = "Consumable Template";
                    lblTemplateDesc.Text = "Columns: ConsumableName*, UOM* (Abbreviation), HSNCode, GSTRate, ReorderLevel, Description";
                    break;
                case "Stationary":
                    lblTemplateName.Text = "Stationary & Other Items Template";
                    lblTemplateDesc.Text = "Columns: StationaryName*, UOM* (Abbreviation), HSNCode, GSTRate, ReorderLevel, Description";
                    break;
            }
        }

        protected void btnDownloadTemplate_Click(object sender, EventArgs e)
        {
            string type = hfMasterType.Value;
            if (string.IsNullOrEmpty(type)) { ShowAlert("Please select a master type first.", false); return; }
            try
            {
                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Data");
                    switch (type)
                    {
                        case "Supplier":
                            WriteHeaders(ws, "SupplierName*", "ContactPerson", "Phone", "Email", "GSTNo", "PAN", "Address", "City", "State", "PinCode");
                            WriteSampleRow(ws, 2, "ABC Traders", "Ramesh Kumar", "9876543210", "abc@example.com", "33AABCA1234A1ZB", "AABCA1234A", "123 Main St", "Chennai", "Tamil Nadu", "600001");
                            break;
                        case "RawMaterial":
                            WriteHeaders(ws, "RawMaterialName*", "UOM*", "HSNCode", "GSTRate", "ReorderLevel", "Description");
                            WriteSampleRow(ws, 2, "Wheat Flour", "kg", "1101", "5", "500", "Fine wheat flour");
                            WriteUOMReference(ws, MMDatabaseHelper.GetActiveUOM(), 8);
                            break;
                        case "PackingMaterial":
                            WriteHeaders(ws, "PackingMaterialName*", "UOM*", "HSNCode", "GSTRate", "ReorderLevel", "Description");
                            WriteSampleRow(ws, 2, "200g Pouch", "nos", "3923", "18", "1000", "");
                            WriteUOMReference(ws, MMDatabaseHelper.GetActiveUOM(), 8);
                            break;
                        case "Consumable":
                            WriteHeaders(ws, "ConsumableName*", "UOM*", "HSNCode", "GSTRate", "ReorderLevel", "Description");
                            WriteSampleRow(ws, 2, "Gloves (Box)", "box", "3926", "18", "50", "");
                            WriteUOMReference(ws, MMDatabaseHelper.GetActiveUOM(), 8);
                            break;
                        case "Stationary":
                            WriteHeaders(ws, "StationaryName*", "UOM*", "HSNCode", "GSTRate", "ReorderLevel", "Description");
                            WriteSampleRow(ws, 2, "A4 Paper Ream", "pkt", "4802", "12", "20", "");
                            WriteUOMReference(ws, MMDatabaseHelper.GetActiveUOM(), 8);
                            break;
                    }
                    ws.Columns().AdjustToContents();
                    using (var ms = new MemoryStream())
                    {
                        wb.SaveAs(ms);
                        ms.Position = 0;
                        byte[] bytes = ms.ToArray();
                        Response.Clear();
                        Response.Buffer = true;
                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        Response.AddHeader("Content-Disposition", "attachment; filename=MM_" + type + "_Template.xlsx");
                        Response.AddHeader("Content-Length", bytes.Length.ToString());
                        Response.BinaryWrite(bytes);
                        Response.Flush();
                        Context.ApplicationInstance.CompleteRequest();
                    }
                }
            }
            catch (Exception ex) { ShowAlert("Error generating template: " + ex.Message, false); }
        }

        private void WriteHeaders(IXLWorksheet ws, params string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value          = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
            }
        }

        private void WriteSampleRow(IXLWorksheet ws, int row, params string[] values)
        {
            for (int i = 0; i < values.Length; i++)
                ws.Cell(row, i + 1).Value = values[i];
        }

        private void WriteUOMReference(IXLWorksheet ws, DataTable uoms, int startCol)
        {
            ws.Cell(1, startCol).Value          = "Valid UOM Abbreviations (reference)";
            ws.Cell(1, startCol).Style.Font.Bold = true;
            int r = 2;
            foreach (DataRow row in uoms.Rows)
            {
                ws.Cell(r, startCol).Value     = row["Abbreviation"].ToString();
                ws.Cell(r, startCol + 1).Value = row["UOMName"].ToString();
                r++;
            }
        }

        protected void btnPreview_Click(object sender, EventArgs e)
        {
            string type = hfMasterType.Value;
            if (string.IsNullOrEmpty(type)) { ShowAlert("Please select a master type first.", false); return; }
            if (!fuExcel.HasFile)           { ShowAlert("Please select an Excel file to upload.", false); return; }
            if (!fuExcel.FileName.ToLower().EndsWith(".xlsx")) { ShowAlert("Only .xlsx files are supported.", false); return; }
            try
            {
                byte[] fileBytes;
                using (var ms = new MemoryStream())
                {
                    fuExcel.FileContent.CopyTo(ms);
                    fileBytes = ms.ToArray();
                }
                if (fileBytes.Length == 0) { ShowAlert("Uploaded file is empty.", false); return; }
                using (var ms2 = new MemoryStream(fileBytes))
                using (var wb  = new XLWorkbook(ms2))
                {
                    var ws   = wb.Worksheet(1);
                    var rows = ParseRows(ws);
                    StorePreviewRows(rows);
                    RenderPreview(rows, type);
                    ShowTemplateSection();
                }
            }
            catch (Exception ex) { ShowAlert("Error reading file: " + ex.GetType().Name + " - " + ex.Message, false); }
        }

        private List<Dictionary<string, string>> ParseRows(IXLWorksheet ws)
        {
            var result = new List<Dictionary<string, string>>();
            var range  = ws.RangeUsed();
            if (range == null) return result;
            int lastRow = range.LastRow().RowNumber();
            int lastCol = range.LastColumn().ColumnNumber();
            var headers = new List<string>();
            for (int c = 1; c <= lastCol; c++)
            {
                string h = ws.Cell(1, c).GetString().Trim().TrimEnd('*');
                headers.Add(h.ToLower().Replace(" ", ""));
            }
            for (int r = 2; r <= lastRow; r++)
            {
                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                bool hasData = false;
                for (int c = 1; c <= headers.Count; c++)
                {
                    string val = ws.Cell(r, c).GetString().Trim();
                    dict[headers[c - 1]] = val;
                    if (!string.IsNullOrEmpty(val)) hasData = true;
                }
                if (hasData) result.Add(dict);
            }
            return result;
        }

        private void StorePreviewRows(List<Dictionary<string, string>> rows)
        {
            var dt = new DataTable();
            if (rows.Count == 0) { Session["BulkPreview"] = dt; return; }
            foreach (var key in rows[0].Keys) dt.Columns.Add(key);
            foreach (var row in rows)
            {
                var dr = dt.NewRow();
                foreach (var key in row.Keys) dr[key] = row[key];
                dt.Rows.Add(dr);
            }
            Session["BulkPreview"] = dt;
        }

        private void RenderPreview(List<Dictionary<string, string>> rows, string type)
        {
            if (rows.Count == 0) { ShowAlert("No data rows found. Make sure data starts from row 2.", false); return; }
            var uomLookup     = BuildUOMLookup();
            var existingNames = GetExistingNames(type);
            var statuses      = new List<string>();
            var reasons       = new List<string>();
            int total = rows.Count, newCount = 0, skipCount = 0, errCount = 0;
            foreach (var row in rows)
            {
                string status = "ok", reason = "";
                string name = "";
                row.TryGetValue(GetNameKey(type), out name);
                name = (name ?? "").Trim();
                if (string.IsNullOrEmpty(name))
                { status = "err"; reason = "Name is required"; errCount++; }
                else if (existingNames.Contains(name.ToLower()))
                { status = "skip"; reason = "Duplicate - already exists"; skipCount++; }
                else if (type != "Supplier")
                {
                    string uom = "";
                    row.TryGetValue("uom", out uom);
                    if (string.IsNullOrEmpty(uom) || !uomLookup.ContainsKey(uom.Trim().ToLower()))
                    { status = "err"; reason = "Invalid or missing UOM"; errCount++; }
                    else newCount++;
                }
                else newCount++;
                statuses.Add(status); reasons.Add(reason);
            }
            lblTotalRows.Text = total.ToString();
            lblNewRows.Text   = newCount.ToString();
            lblSkipRows.Text  = skipCount.ToString();
            lblErrRows.Text   = errCount.ToString();
            var sb = new StringBuilder();
            sb.Append("<table class='preview-table'><thead><tr><th>#</th><th>Status</th>");
            foreach (var key in rows[0].Keys)
            {
                if (key.StartsWith("validuom")) continue;
                sb.Append("<th>" + System.Web.HttpUtility.HtmlEncode(key) + "</th>");
            }
            sb.Append("<th>Note</th></tr></thead><tbody>");
            for (int i = 0; i < rows.Count; i++)
            {
                string rc    = statuses[i] == "ok" ? "row-ok" : (statuses[i] == "skip" ? "row-skip" : "");
                string badge = statuses[i] == "ok"   ? "<span class='badge-ok'>New</span>"
                             : statuses[i] == "skip" ? "<span class='badge-skip'>Skip</span>"
                                                     : "<span class='badge-err'>Error</span>";
                sb.Append("<tr class='" + rc + "'>");
                sb.Append("<td>" + (i + 1) + "</td><td>" + badge + "</td>");
                foreach (var key in rows[i].Keys)
                {
                    if (key.StartsWith("validuom")) continue;
                    sb.Append("<td>" + System.Web.HttpUtility.HtmlEncode(rows[i][key]) + "</td>");
                }
                sb.Append("<td style='font-size:11px;color:#888'>" + reasons[i] + "</td></tr>");
            }
            sb.Append("</tbody></table>");
            litPreviewTable.Text = sb.ToString();
            pnlPreview.Visible   = true;
        }

        protected void btnImport_Click(object sender, EventArgs e)
        {
            string type = hfMasterType.Value;
            if (string.IsNullOrEmpty(type)) { ShowAlert("Session expired. Please start over.", false); return; }
            var dt = Session["BulkPreview"] as DataTable;
            if (dt == null || dt.Rows.Count == 0) { ShowAlert("No preview data found. Please upload again.", false); return; }
            var uomLookup     = BuildUOMLookup();
            var existingNames = GetExistingNames(type);
            string nameKey    = GetNameKey(type);
            int imported = 0, skipped = 0, errors = 0;
            var errorList = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                try
                {
                    string name = row.Table.Columns.Contains(nameKey) ? row[nameKey].ToString().Trim() : "";
                    if (string.IsNullOrEmpty(name)) { errors++; continue; }
                    if (existingNames.Contains(name.ToLower())) { skipped++; continue; }
                    string uomAbbr = GetCol(row, "uom");
                    string hsnCode = GetCol(row, "hsncode");
                    string gstStr  = GetCol(row, "gstrate");
                    string reorderStr = GetCol(row, "reorderlevel");
                    string desc    = GetCol(row, "description");
                    decimal? gstRate = null; decimal g;
                    if (decimal.TryParse(gstStr, out g)) gstRate = g;
                    decimal reorder = 0;
                    decimal.TryParse(reorderStr, out reorder);
                    switch (type)
                    {
                        case "Supplier":
                            MMDatabaseHelper.AddSupplier(name, GetCol(row,"contactperson"), GetCol(row,"phone"),
                                GetCol(row,"email"), GetCol(row,"gstno"), GetCol(row,"pan"),
                                GetCol(row,"address"), GetCol(row,"city"), GetCol(row,"state"), GetCol(row,"pincode"));
                            break;
                        case "RawMaterial":
                            if (!uomLookup.ContainsKey(uomAbbr.ToLower())) { errors++; errorList.Add(name+": invalid UOM"); continue; }
                            MMDatabaseHelper.AddRawMaterial(name, desc, hsnCode, gstRate, uomLookup[uomAbbr.ToLower()], reorder);
                            break;
                        case "PackingMaterial":
                            if (!uomLookup.ContainsKey(uomAbbr.ToLower())) { errors++; errorList.Add(name+": invalid UOM"); continue; }
                            MMDatabaseHelper.AddPackingMaterial(name, desc, hsnCode, gstRate, uomLookup[uomAbbr.ToLower()], reorder);
                            break;
                        case "Consumable":
                            if (!uomLookup.ContainsKey(uomAbbr.ToLower())) { errors++; errorList.Add(name+": invalid UOM"); continue; }
                            MMDatabaseHelper.AddConsumable(name, desc, hsnCode, gstRate, uomLookup[uomAbbr.ToLower()], reorder);
                            break;
                        case "Stationary":
                            if (!uomLookup.ContainsKey(uomAbbr.ToLower())) { errors++; errorList.Add(name+": invalid UOM"); continue; }
                            MMDatabaseHelper.AddStationary(name, desc, hsnCode, gstRate, uomLookup[uomAbbr.ToLower()], reorder);
                            break;
                    }
                    imported++;
                }
                catch (Exception ex) { errors++; errorList.Add(ex.Message); }
            }
            Session.Remove("BulkPreview");
            pnlPreview.Visible = false;
            string msg = imported + " record(s) imported successfully.";
            if (skipped > 0) msg += " " + skipped + " skipped (duplicates).";
            if (errors  > 0) msg += " " + errors + " error(s): " + string.Join("; ", errorList.ToArray());
            ShowAlert(msg, errors == 0);
            ShowTemplateSection();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            Session.Remove("BulkPreview");
            hfMasterType.Value          = "";
            pnlPreview.Visible          = false;
            pnlUploadSection.Visible    = false;
            pnlTemplateDownload.Visible = false;
            pnlAlert.Visible            = false;
        }

        private Dictionary<string, int> BuildUOMLookup()
        {
            var lookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in MMDatabaseHelper.GetActiveUOM().Rows)
                lookup[row["Abbreviation"].ToString().ToLower()] = Convert.ToInt32(row["UOMID"]);
            return lookup;
        }

        private HashSet<string> GetExistingNames(string type)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            DataTable dt; string nameCol;
            switch (type)
            {
                case "Supplier":        dt = MMDatabaseHelper.GetAllSuppliers();        nameCol = "SupplierName";   break;
                case "RawMaterial":     dt = MMDatabaseHelper.GetAllRawMaterials();     nameCol = "RMName";         break;
                case "PackingMaterial": dt = MMDatabaseHelper.GetAllPackingMaterials(); nameCol = "PMName";         break;
                case "Consumable":      dt = MMDatabaseHelper.GetAllConsumables();      nameCol = "ConsumableName"; break;
                case "Stationary":      dt = MMDatabaseHelper.GetAllStationaries();     nameCol = "StationaryName"; break;
                default: return set;
            }
            foreach (DataRow row in dt.Rows)
                if (dt.Columns.Contains(nameCol)) set.Add(row[nameCol].ToString());
            return set;
        }

        private string GetNameKey(string type)
        {
            switch (type)
            {
                case "Supplier":        return "suppliername";
                case "RawMaterial":     return "rawmaterialname";
                case "PackingMaterial": return "packingmaterialname";
                case "Consumable":      return "consumablename";
                case "Stationary":      return "stationaryname";
                default:                return "name";
            }
        }

        private string GetCol(DataRow row, string key)
        {
            foreach (DataColumn col in row.Table.Columns)
                if (col.ColumnName.Equals(key, StringComparison.OrdinalIgnoreCase))
                    return row[col].ToString().Trim();
            return "";
        }

        private void ShowAlert(string msg, bool success)
        {
            lblAlert.Text     = msg;
            pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger");
            pnlAlert.Visible  = true;
        }
    }
}
