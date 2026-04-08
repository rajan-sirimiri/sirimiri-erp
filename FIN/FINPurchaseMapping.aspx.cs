using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using FINApp.DAL;
using ClosedXML.Excel;

namespace FINApp
{
    public partial class FINPurchaseMapping : Page
    {
        protected Label        lblNavUser, lblAlert;
        protected Panel        pnlAlert, pnlResults, pnlNoSavedFiles;
        protected FileUpload   fileUpload;
        protected Button       btnUpload, btnSaveAllItems, btnSaveAllSuppliers;
        protected Button       btnTabItems, btnTabSuppliers, btnTabMapped;
        protected HiddenField  hfTab;

        protected Repeater     rptSavedFiles;
        protected Repeater     rptUnmappedItems, rptUnmappedSuppliers;
        protected Panel        pnlItems, pnlSuppliers, pnlMapped;
        protected Label        lblItemCount, lblItemMapped, lblSupplierCount, lblSupplierMapped;

        protected Repeater     rptMappedItems, rptMappedSuppliers;
        protected Label        lblMappedItemCount, lblMappedSupplierCount;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["FIN_UserID"] == null) { Response.Redirect("FINLogin.aspx"); return; }
            lblNavUser.Text = Session["FIN_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
            {
                hfTab.Value = "ITEMS";
                BindSavedFiles();
            }

            // Always set tab visibility and re-bind data if session has it
            SetActiveTab();
            if (Session["TallyPurchaseItems"] != null)
            {
                pnlResults.Visible = true;
                if (!IsPostBack)
                {
                    BindUnmappedItems();
                    BindUnmappedSuppliers();
                }
            }
        }

        // ── FILE MANAGEMENT ──

        private string GetUploadFolder()
        {
            string folder = Server.MapPath("~/App_Data/PurchaseUploads");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return folder;
        }

        private void BindSavedFiles()
        {
            string folder = GetUploadFolder();
            var files = new DirectoryInfo(folder).GetFiles("*.xlsx")
                .OrderByDescending(f => f.CreationTime).Take(10).ToList();
            if (rptSavedFiles != null) { rptSavedFiles.DataSource = files; rptSavedFiles.DataBind(); }
            if (pnlNoSavedFiles != null) pnlNoSavedFiles.Visible = files.Count == 0;
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            pnlAlert.Visible = false;
            if (fileUpload == null || !fileUpload.HasFile)
            { ShowAlert("Please select a Purchase Report file.", false); return; }

            try
            {
                string folder = GetUploadFolder();
                string savedName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" +
                    Path.GetFileNameWithoutExtension(fileUpload.FileName).Replace(" ", "_") + ".xlsx";
                string savedPath = Path.Combine(folder, savedName);
                fileUpload.SaveAs(savedPath);

                LoadPurchaseFile(savedPath);
                BindSavedFiles();
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        protected void rptSavedFiles_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "LoadFile") return;
            string fileName = Path.GetFileName(e.CommandArgument.ToString());
            string filePath = Path.Combine(GetUploadFolder(), fileName);
            if (!File.Exists(filePath)) { ShowAlert("File not found: " + fileName, false); return; }

            try { LoadPurchaseFile(filePath); }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        private void LoadPurchaseFile(string filePath)
        {
            var items = new List<string>();
            var suppliers = new List<string>();

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var wb = new XLWorkbook(stream))
            {
                var ws = wb.Worksheet(1);
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;
                for (int r = 2; r <= lastRow; r++)
                {
                    string supplier = ws.Cell(r, 2).GetString().Trim();
                    string item = ws.Cell(r, 3).GetString().Trim();
                    if (!string.IsNullOrEmpty(supplier) && !suppliers.Contains(supplier))
                        suppliers.Add(supplier);
                    if (!string.IsNullOrEmpty(item) && !items.Contains(item))
                        items.Add(item);
                }
            }

            Session["TallyPurchaseItems"] = items;
            Session["TallyPurchaseSuppliers"] = suppliers;

            int autoMatched = FINDatabaseHelper.AutoMatchSuppliers(suppliers);

            BindUnmappedItems();
            BindUnmappedSuppliers();
            pnlResults.Visible = true;

            string fileName = Path.GetFileName(filePath);
            ShowAlert("Loaded " + items.Count + " items, " + suppliers.Count + " suppliers from " + fileName +
                ". Auto-matched " + autoMatched + " suppliers.", true);
        }

        // ── TAB SWITCHING ──

        protected void btnTab_Click(object sender, EventArgs e)
        {
            hfTab.Value = ((Button)sender).CommandArgument;
            SetActiveTab();
            BindCurrentTab();
        }

        private void SetActiveTab()
        {
            string tab = hfTab.Value;
            if (btnTabItems != null) btnTabItems.CssClass = tab == "ITEMS" ? "tab-btn active" : "tab-btn";
            if (btnTabSuppliers != null) btnTabSuppliers.CssClass = tab == "SUPPLIERS" ? "tab-btn active" : "tab-btn";
            if (btnTabMapped != null) btnTabMapped.CssClass = tab == "MAPPED" ? "tab-btn active" : "tab-btn";
            if (pnlItems != null) pnlItems.Visible = tab == "ITEMS";
            if (pnlSuppliers != null) pnlSuppliers.Visible = tab == "SUPPLIERS";
            if (pnlMapped != null) pnlMapped.Visible = tab == "MAPPED";
        }

        private void BindCurrentTab()
        {
            string tab = hfTab.Value;
            if (tab == "ITEMS") BindUnmappedItems();
            else if (tab == "SUPPLIERS") BindUnmappedSuppliers();
            else if (tab == "MAPPED") BindMappedItems();
        }

        // ── ITEM MAPPING ──

        private void BindUnmappedItems()
        {
            var tallyNames = Session["TallyPurchaseItems"] as List<string>;
            if (tallyNames == null) return;

            var unmapped = new DataTable();
            unmapped.Columns.Add("TallyName", typeof(string));
            int mapped = 0;

            foreach (string name in tallyNames)
            {
                if (FINDatabaseHelper.ItemMappingExists(name)) mapped++;
                else unmapped.Rows.Add(name);
            }

            rptUnmappedItems.DataSource = unmapped;
            rptUnmappedItems.DataBind();

            // Populate material dropdowns with ALL materials combined
            var allMaterials = GetCombinedMaterials();
            foreach (RepeaterItem item in rptUnmappedItems.Items)
            {
                var ddl = item.FindControl("ddlMaterial") as DropDownList;
                if (ddl != null)
                {
                    ddl.Items.Clear();
                    ddl.Items.Add(new ListItem("-- Select Material --", ""));
                    foreach (DataRow r in allMaterials.Rows)
                        ddl.Items.Add(new ListItem(r["Label"].ToString(), r["Type"] + "|" + r["ID"]));
                }
            }

            if (lblItemCount != null) lblItemCount.Text = unmapped.Rows.Count.ToString();
            if (lblItemMapped != null) lblItemMapped.Text = mapped.ToString();
        }

        private DataTable GetCombinedMaterials()
        {
            var dt = new DataTable();
            dt.Columns.Add("Type"); dt.Columns.Add("ID"); dt.Columns.Add("Label");

            var rm = FINDatabaseHelper.GetAllRawMaterials();
            foreach (DataRow r in rm.Rows)
                dt.Rows.Add("RM", r["RMID"], "[RM] " + r["RMName"] + " (" + r["RMCode"] + ")");

            var pm = FINDatabaseHelper.GetAllPackingMaterials();
            foreach (DataRow r in pm.Rows)
                dt.Rows.Add("PM", r["PMID"], "[PM] " + r["PMName"] + " (" + r["PMCode"] + ")");

            var cn = FINDatabaseHelper.GetAllConsumables();
            foreach (DataRow r in cn.Rows)
                dt.Rows.Add("CN", r["ConsumableID"], "[CN] " + r["ConsumableName"] + " (" + r["ConsumableCode"] + ")");

            var st = FINDatabaseHelper.GetAllStationaries();
            foreach (DataRow r in st.Rows)
                dt.Rows.Add("ST", r["StationaryID"], "[ST] " + r["StationaryName"] + " (" + r["StationaryCode"] + ")");

            var sc = FINDatabaseHelper.GetAllScrapMaterials();
            foreach (DataRow r in sc.Rows)
                dt.Rows.Add("SCRAP", r["ScrapID"], "[SCRAP] " + r["ScrapName"] + " (" + r["ScrapCode"] + ")");

            return dt;
        }

        protected void rptUnmappedItems_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "SaveItem") return;
            string tallyName = e.CommandArgument.ToString();

            var ddlType = e.Item.FindControl("ddlMatType") as DropDownList;
            var ddlMat = e.Item.FindControl("ddlMaterial") as DropDownList;

            string matType = ddlType?.SelectedValue ?? "";

            // For CAPEX/OTHER, no material selection needed
            if (matType == "CAPEX" || matType == "OTHER")
            {
                FINDatabaseHelper.SaveItemMapping(tallyName, matType, null);
                BindUnmappedItems();
                pnlResults.Visible = true;
                ShowAlert("Saved: " + tallyName + " as " + matType, true);
                return;
            }

            // If material dropdown has a value, use it (overrides type dropdown)
            if (ddlMat != null && !string.IsNullOrEmpty(ddlMat.SelectedValue))
            {
                string[] parts = ddlMat.SelectedValue.Split('|');
                if (parts.Length == 2)
                {
                    matType = parts[0];
                    int matId = Convert.ToInt32(parts[1]);
                    FINDatabaseHelper.SaveItemMapping(tallyName, matType, matId);
                    BindUnmappedItems();
                    pnlResults.Visible = true;
                    ShowAlert("Saved: " + tallyName, true);
                    return;
                }
            }

            if (string.IsNullOrEmpty(matType))
            { ShowAlert("Please select a material type or pick from the material dropdown.", false); return; }

            // Type selected but no specific material
            FINDatabaseHelper.SaveItemMapping(tallyName, matType, null);
            BindUnmappedItems();
            pnlResults.Visible = true;
            ShowAlert("Saved: " + tallyName + " as " + matType + " (no specific material selected)", true);
        }

        // ── SUPPLIER MAPPING ──

        private void BindUnmappedSuppliers()
        {
            var tallyNames = Session["TallyPurchaseSuppliers"] as List<string>;
            if (tallyNames == null) return;

            var unmapped = new DataTable();
            unmapped.Columns.Add("TallyName", typeof(string));
            int mapped = 0;

            foreach (string name in tallyNames)
            {
                if (FINDatabaseHelper.SupplierMappingExists(name)) mapped++;
                else unmapped.Rows.Add(name);
            }

            rptUnmappedSuppliers.DataSource = unmapped;
            rptUnmappedSuppliers.DataBind();

            var suppliers = FINDatabaseHelper.GetAllSuppliers();
            foreach (RepeaterItem item in rptUnmappedSuppliers.Items)
            {
                var ddl = item.FindControl("ddlSupplier") as DropDownList;
                if (ddl != null)
                {
                    ddl.Items.Clear();
                    ddl.Items.Add(new ListItem("-- Select Supplier --", "0"));
                    foreach (DataRow r in suppliers.Rows)
                        ddl.Items.Add(new ListItem(r["SupplierName"] + " (" + r["SupplierCode"] + ")", r["SupplierID"].ToString()));
                }
            }

            if (lblSupplierCount != null) lblSupplierCount.Text = unmapped.Rows.Count.ToString();
            if (lblSupplierMapped != null) lblSupplierMapped.Text = mapped.ToString();
        }

        protected void rptUnmappedSuppliers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "SaveSupplier") return;
            string tallyName = e.CommandArgument.ToString();

            var ddl = e.Item.FindControl("ddlSupplier") as DropDownList;
            if (ddl == null || ddl.SelectedValue == "0")
            { ShowAlert("Please select a supplier.", false); return; }

            int supplierId = Convert.ToInt32(ddl.SelectedValue);
            FINDatabaseHelper.SaveSupplierMapping(tallyName, supplierId);
            BindUnmappedSuppliers();
            pnlResults.Visible = true;
            ShowAlert("Saved: " + tallyName, true);
        }

        // ── SAVE ALL ──

        protected void btnSaveAllItems_Click(object sender, EventArgs e)
        {
            int saved = 0;
            foreach (RepeaterItem item in rptUnmappedItems.Items)
            {
                if (item.ItemType != ListItemType.Item && item.ItemType != ListItemType.AlternatingItem) continue;

                var ddlType = item.FindControl("ddlMatType") as DropDownList;
                var ddlMat = item.FindControl("ddlMaterial") as DropDownList;
                var btnSave = item.FindControl("btnSaveItem") as LinkButton;

                if (btnSave == null) continue;
                string tallyName = btnSave.CommandArgument;

                // Check if material dropdown has a value
                if (ddlMat != null && !string.IsNullOrEmpty(ddlMat.SelectedValue))
                {
                    string[] parts = ddlMat.SelectedValue.Split('|');
                    if (parts.Length == 2)
                    {
                        FINDatabaseHelper.SaveItemMapping(tallyName, parts[0], Convert.ToInt32(parts[1]));
                        saved++;
                        continue;
                    }
                }

                // Check if type dropdown has CAPEX/OTHER
                string matType = ddlType?.SelectedValue ?? "";
                if (matType == "CAPEX" || matType == "OTHER")
                {
                    FINDatabaseHelper.SaveItemMapping(tallyName, matType, null);
                    saved++;
                }
            }

            if (saved > 0)
            {
                BindUnmappedItems();
                ShowAlert("Saved " + saved + " item mapping(s).", true);
            }
            else
            {
                ShowAlert("No items to save. Please select a material for at least one row.", false);
            }
        }

        protected void btnSaveAllSuppliers_Click(object sender, EventArgs e)
        {
            int saved = 0;
            foreach (RepeaterItem item in rptUnmappedSuppliers.Items)
            {
                if (item.ItemType != ListItemType.Item && item.ItemType != ListItemType.AlternatingItem) continue;

                var ddl = item.FindControl("ddlSupplier") as DropDownList;
                var btnSave = item.FindControl("btnSaveSupplier") as LinkButton;

                if (ddl == null || ddl.SelectedValue == "0" || btnSave == null) continue;

                string tallyName = btnSave.CommandArgument;
                int supplierId = Convert.ToInt32(ddl.SelectedValue);
                FINDatabaseHelper.SaveSupplierMapping(tallyName, supplierId);
                saved++;
            }

            if (saved > 0)
            {
                BindUnmappedSuppliers();
                ShowAlert("Saved " + saved + " supplier mapping(s).", true);
            }
            else
            {
                ShowAlert("No suppliers to save. Please select a supplier for at least one row.", false);
            }
        }

        // ── MAPPED ITEMS ──

        private void BindMappedItems()
        {
            var itemMap = FINDatabaseHelper.GetAllItemMappings();
            if (rptMappedItems != null) { rptMappedItems.DataSource = itemMap; rptMappedItems.DataBind(); }
            if (lblMappedItemCount != null) lblMappedItemCount.Text = itemMap.Rows.Count + " items mapped";

            var supMap = FINDatabaseHelper.GetAllSupplierMappings();
            if (rptMappedSuppliers != null) { rptMappedSuppliers.DataSource = supMap; rptMappedSuppliers.DataBind(); }
            if (lblMappedSupplierCount != null) lblMappedSupplierCount.Text = supMap.Rows.Count + " suppliers mapped";
        }

        // ── HELPERS ──

        private void ShowAlert(string msg, bool success)
        {
            pnlAlert.Visible = true;
            lblAlert.Text = msg;
            pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger");
        }
    }
}
