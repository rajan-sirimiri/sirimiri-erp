using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using StockApp.DAL;
using MySql.Data.MySqlClient;

namespace StockApp
{
    public partial class SASalesForce : Page
    {
        protected Label lblUserName, lblUserRole, lblAlert, lblMonthYear, lblProjMonth, lblShipMonth;
        protected Label lblShipZone, lblShipRegion, lblProjZone, lblProjRegion, lblEditingShipId;
        protected Panel pnlAlert, pnlProjection, pnlShipments, pnlProjLines, pnlProjEmpty, pnlProjForm;
        protected Panel pnlShipLines, pnlShipEmpty, pnlZoneRegionInfo, pnlProjZoneRegion, pnlShipForm;
        protected Panel pnlConsignments, pnlSAConsigDetail, pnlConsigEmpty, pnlSAConsigOrdersEmpty;
        protected Panel pnlSAShipForm, pnlSAConsigLocked;
        protected Label lblSALockedStatus;
        // Set by LoadSAConsigDetail — consulted by rptSAConsigOrders_ItemDataBound to hide
        // per-row Edit/Delete buttons when the whole consignment is locked (DISPATCHED/ARCHIVED).
        private bool _saConsigIsLocked = false;
        protected DropDownList ddlMonth, ddlYear;
        protected DropDownList ddlProjArea, ddlProjChannel;
        protected DropDownList ddlShipArea, ddlShipChannel, ddlTransport, ddlCustomer;
        protected DropDownList ddlSACustomer, ddlSAChannel;
        protected DropDownList ddlSADispatched, ddlSAArchived;
        protected TextBox txtShipDate, txtVehicleNo, txtSAConsigDate, txtSAConsigText, txtSAShipDate;
        protected Button btnTabProjection, btnTabShipments, btnTabConsignments, btnLoadProjection, btnSaveProjection, btnCreateShipment, btnSaveShipment, btnNewProjection, btnNewShipment;
        protected Button btnSACreateConsig, btnSASaveDraft, btnSACreateOrder, btnSASendToPK;
        protected LinkButton btnSACancelEdit;
        protected Label lblSAConsigTitle, lblSAConsigStatus, lblSAEditShipId, lblSAShipFormTitle;
        protected Repeater rptProjLines, rptProjections, rptShipLines, rptShipments, rptSAConsigOrders, rptSAConsigTabs, rptSAShipLines;
        protected HiddenField hfTab, hfProductOptionsHtml, hfUOMOptionsHtml, hfEditProjId;
        protected HiddenField hfShipZoneID, hfShipRegionID, hfProjZoneID, hfProjRegionID, hfEditShipId;
        protected HiddenField hfSAConsigId, hfSAProductData, hfSAProductOptionsHtml;

        private int SelMonth => Convert.ToInt32(ddlMonth.SelectedValue);
        private int SelYear => Convert.ToInt32(ddlYear.SelectedValue);
        private int UserID => Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Login.aspx"); return; }
            string __role = Session["Role"]?.ToString() ?? "";
            if (!DatabaseHelper.RoleHasModuleAccess(__role, "SA", "SA_SALESFORCE"))
            { Response.Redirect("SAHome.aspx"); return; }

            lblUserName.Text = Session["FullName"]?.ToString() ?? "";
            lblUserRole.Text = Session["Role"]?.ToString() ?? "";

            if (!IsPostBack)
            {
                LoadYears();
                ddlMonth.SelectedValue = DateTime.Now.Month.ToString();
                ddlYear.SelectedValue = DateTime.Now.Year.ToString();
                LoadStaticDropdowns();
                LoadZoneDropdowns();
                BuildOptionHtml();
                SetActiveTab();
                RefreshData();
            }
        }

        private void LoadYears()
        {
            ddlYear.Items.Clear();
            int cur = DateTime.Now.Year;
            for (int y = cur - 1; y <= cur + 2; y++)
                ddlYear.Items.Add(new ListItem(y.ToString(), y.ToString()));
        }

        private void LoadStaticDropdowns()
        {
            // Channels
            DataTable ch = DatabaseHelper.ExecuteQueryPublic(
                "SELECT ChannelID, ChannelName FROM SA_Channels WHERE IsActive=1 ORDER BY SortOrder;");
            BindDDL(ddlProjChannel, ch, "ChannelName", "ChannelID", "-- Select Channel --");
            BindDDL(ddlShipChannel, ch, "ChannelName", "ChannelID", "-- Select Channel --");

            // Transport modes
            DataTable tm = DatabaseHelper.ExecuteQueryPublic(
                "SELECT ModeID, ModeName FROM SA_TransportModes WHERE IsActive=1 ORDER BY SortOrder;");
            BindDDL(ddlTransport, tm, "ModeName", "ModeID", "-- Select --");
        }

        private void LoadZoneDropdowns()
        {
            // All areas with zone/region context — shared query
            DataTable areas = DatabaseHelper.ExecuteQueryPublic(
                "SELECT a.AreaID, a.AreaName, a.AreaCode, r.RegionName, z.ZoneName" +
                " FROM SA_Areas a JOIN SA_Regions r ON r.RegionID=a.RegionID" +
                " JOIN SA_Zones z ON z.ZoneID=r.ZoneID" +
                " WHERE a.IsActive=1 ORDER BY z.SortOrder, r.SortOrder, a.SortOrder, a.AreaName;");

            // Projection Area dropdown
            if (ddlProjArea != null)
            {
                ddlProjArea.Items.Clear();
                ddlProjArea.Items.Add(new ListItem("-- Select Area --", "0"));
                foreach (DataRow r in areas.Rows)
                    ddlProjArea.Items.Add(new ListItem(
                        r["AreaName"].ToString() + " (" + r["RegionName"] + " / " + r["ZoneName"] + ")",
                        r["AreaID"].ToString()));
            }

            // Ship Area dropdown
            if (ddlShipArea != null)
            {
                ddlShipArea.Items.Clear();
                ddlShipArea.Items.Add(new ListItem("-- Select Area --", "0"));
                foreach (DataRow r in areas.Rows)
                    ddlShipArea.Items.Add(new ListItem(
                        r["AreaName"].ToString() + " (" + r["RegionName"] + " / " + r["ZoneName"] + ")",
                        r["AreaID"].ToString()));
            }

            // Customers
            if (ddlCustomer != null)
            {
                ddlCustomer.Items.Clear();
                ddlCustomer.Items.Add(new ListItem("-- Select Customer --", "0"));
                DataTable cust = DatabaseHelper.ExecuteQueryPublic(
                    "SELECT c.CustomerID, c.CustomerName, c.CustomerCode, c.City," +
                    " IFNULL(ct.TypeName,'') AS TypeName" +
                    " FROM PK_Customers c" +
                    " LEFT JOIN PK_CustomerTypes ct ON ct.TypeCode=c.CustomerType" +
                    " WHERE c.IsActive=1 ORDER BY c.CustomerName;");
                foreach (DataRow r in cust.Rows)
                {
                    string city = r["City"] != DBNull.Value ? r["City"].ToString() : "";
                    string typeName = r["TypeName"].ToString();
                    string label = r["CustomerName"].ToString();
                    if (!string.IsNullOrEmpty(typeName)) label += " — " + typeName;
                    if (!string.IsNullOrEmpty(city)) label += " — " + city;
                    ddlCustomer.Items.Add(new ListItem(label, r["CustomerID"].ToString()));
                }
            }
        }

        private void BuildOptionHtml()
        {
            // Core products only
            DataTable products = DatabaseHelper.ExecuteQueryPublic(
                "SELECT ProductID, ProductName, ProductCode FROM PP_Products WHERE IsActive=1 AND ProductType='Core' ORDER BY ProductName;");
            var sb = new System.Text.StringBuilder();
            foreach (DataRow r in products.Rows)
                sb.Append("<option value='" + r["ProductID"] + "'>" + r["ProductName"] + " (" + r["ProductCode"] + ")</option>");
            hfProductOptionsHtml.Value = sb.ToString();

            // UOMs — only CASE, JAR, BOX
            DataTable uoms = DatabaseHelper.ExecuteQueryPublic(
                "SELECT UOMID, Abbreviation, UOMName FROM MM_UOM WHERE IsActive=1 AND UPPER(Abbreviation) IN ('CASE','JAR','BOX') ORDER BY UOMName;");
            var ub = new System.Text.StringBuilder();
            foreach (DataRow r in uoms.Rows)
            {
                string abbr = r["Abbreviation"].ToString();
                string sel = abbr.Equals("JAR", StringComparison.OrdinalIgnoreCase) ? " selected" : "";
                ub.Append("<option value='" + r["UOMID"] + "'" + sel + ">" + abbr + "</option>");
            }
            hfUOMOptionsHtml.Value = ub.ToString();
        }

        // ── HELPERS ───────────────────────────────────────────────────────

        private void BindDDL(DropDownList ddl, DataTable dt, string textCol, string valueCol, string emptyText)
        {
            if (ddl == null) return;
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem(emptyText, "0"));
            foreach (DataRow r in dt.Rows)
                ddl.Items.Add(new ListItem(r[textCol].ToString(), r[valueCol].ToString()));
        }

        private void ClearDDL(DropDownList ddl, string emptyText)
        {
            if (ddl == null) return;
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem(emptyText, "0"));
        }

        private void LoadRegions(DropDownList ddl, int zoneId)
        {
            ClearDDL(ddl, "-- Select Region --");
            if (zoneId == 0) return;
            DataTable dt = DatabaseHelper.ExecuteQueryPublic(
                "SELECT RegionID, RegionName FROM SA_Regions WHERE ZoneID=?z AND IsActive=1 ORDER BY SortOrder, RegionName;",
                new MySqlParameter("?z", zoneId));
            foreach (DataRow r in dt.Rows)
                ddl.Items.Add(new ListItem(r["RegionName"].ToString(), r["RegionID"].ToString()));
        }

        private void LoadAreas(DropDownList ddl, int regionId)
        {
            ClearDDL(ddl, "-- Select Area --");
            if (regionId == 0) return;
            DataTable dt = DatabaseHelper.ExecuteQueryPublic(
                "SELECT AreaID, AreaName, AreaCode FROM SA_Areas WHERE RegionID=?rid AND IsActive=1 ORDER BY SortOrder, AreaName;",
                new MySqlParameter("?rid", regionId));
            foreach (DataRow r in dt.Rows)
                ddl.Items.Add(new ListItem(r["AreaName"].ToString() + " (" + r["AreaCode"] + ")", r["AreaID"].ToString()));
        }

        // ── TAB SWITCHING ─────────────────────────────────────────────────

        protected void btnTab_Click(object sender, EventArgs e)
        { hfTab.Value = ((Button)sender).CommandArgument; SetActiveTab(); RefreshData(); }

        private void SetActiveTab()
        {
            // Default landing tab is Consignments — that's where day-to-day SA work happens.
            // Projections are a read-only reference and shouldn't be the first thing users see.
            string tab = string.IsNullOrEmpty(hfTab.Value) ? "consignments" : hfTab.Value;
            pnlProjection.Visible = tab == "projection";
            if (pnlShipments != null) pnlShipments.Visible = false; // Legacy — hidden
            if (pnlConsignments != null) pnlConsignments.Visible = tab == "consignments";
            btnTabProjection.CssClass = tab == "projection" ? "tab-btn active" : "tab-btn";
            if (btnTabConsignments != null) btnTabConsignments.CssClass = tab == "consignments" ? "tab-btn active" : "tab-btn";
            if (tab == "consignments") BindSAConsigTabs();
        }

        protected void ddlMonth_Changed(object sender, EventArgs e) { RefreshData(); }
        protected void ddlYear_Changed(object sender, EventArgs e) { RefreshData(); }

        private void RefreshData()
        {
            string mn = ddlMonth.SelectedItem.Text + " " + SelYear;
            lblMonthYear.Text = mn; lblProjMonth.Text = mn; lblShipMonth.Text = mn;
            LoadProjectionsList();
            LoadShipmentsList();
        }

        // ── CREATE NEW BUTTONS ─────────────────────────────────────────────

        protected void btnNewProjection_Click(object sender, EventArgs e)
        {
            if (pnlProjForm != null) pnlProjForm.Visible = true;
            if (pnlProjLines != null) pnlProjLines.Visible = true;
            if (pnlProjZoneRegion != null) pnlProjZoneRegion.Visible = false;
            if (ddlProjArea != null) ddlProjArea.SelectedIndex = 0;
            if (ddlProjChannel != null) ddlProjChannel.SelectedIndex = 0;
            if (hfEditProjId != null) hfEditProjId.Value = "0";
            // Show blank product line
            var dt = new DataTable();
            dt.Columns.Add("LineID", typeof(int));
            dt.Columns.Add("ProductID", typeof(int));
            dt.Columns.Add("Quantity", typeof(int));
            dt.Columns.Add("UOMID", typeof(int));
            dt.Rows.Add(0, 0, 0, 0);
            rptProjLines.DataSource = dt;
            rptProjLines.DataBind();
            SetProjOptions();
            BuildOptionHtml();
            pnlAlert.Visible = false;
        }

        protected void btnNewShipment_Click(object sender, EventArgs e)
        {
            if (pnlShipForm != null) pnlShipForm.Visible = true;
            if (pnlShipLines != null) pnlShipLines.Visible = true;
            if (pnlZoneRegionInfo != null) pnlZoneRegionInfo.Visible = false;
            if (ddlShipArea != null) ddlShipArea.SelectedIndex = 0;
            if (ddlShipChannel != null) ddlShipChannel.SelectedIndex = 0;
            if (ddlTransport != null) ddlTransport.SelectedIndex = 0;
            if (ddlCustomer != null) ddlCustomer.SelectedIndex = 0;
            if (txtShipDate != null) txtShipDate.Text = "";
            if (txtVehicleNo != null) txtVehicleNo.Text = "";
            if (hfEditShipId != null) hfEditShipId.Value = "0";
            if (lblEditingShipId != null) lblEditingShipId.Text = "";
            // Show blank product line
            var dt = new DataTable();
            dt.Columns.Add("ProductID", typeof(int));
            dt.Columns.Add("Quantity", typeof(int));
            dt.Rows.Add(0, 0);
            rptShipLines.DataSource = dt;
            rptShipLines.DataBind();
            BuildOptionHtml();
            pnlAlert.Visible = false;
        }

        // ── PROJECTION: AREA SELECTION (auto-resolves Zone/Region) ────────

        protected void ddlProjArea_Changed(object sender, EventArgs e)
        {
            int areaId = Convert.ToInt32(ddlProjArea.SelectedValue);
            if (areaId > 0)
            {
                DataRow area = DatabaseHelper.ExecuteQueryRowPublic(
                    "SELECT a.RegionID, r.RegionName, r.ZoneID, z.ZoneName" +
                    " FROM SA_Areas a JOIN SA_Regions r ON r.RegionID=a.RegionID" +
                    " JOIN SA_Zones z ON z.ZoneID=r.ZoneID WHERE a.AreaID=?aid;",
                    new MySqlParameter("?aid", areaId));
                if (area != null)
                {
                    if (lblProjZone != null) lblProjZone.Text = area["ZoneName"].ToString();
                    if (lblProjRegion != null) lblProjRegion.Text = area["RegionName"].ToString();
                    if (hfProjZoneID != null) hfProjZoneID.Value = area["ZoneID"].ToString();
                    if (hfProjRegionID != null) hfProjRegionID.Value = area["RegionID"].ToString();
                    if (pnlProjZoneRegion != null) pnlProjZoneRegion.Visible = true;
                }
            }
            else
            {
                if (pnlProjZoneRegion != null) pnlProjZoneRegion.Visible = false;
            }
        }

        // ── SHIPMENT: AREA SELECTION (auto-resolves Zone/Region) ──────────

        protected void ddlShipArea_Changed(object sender, EventArgs e)
        {
            int areaId = Convert.ToInt32(ddlShipArea.SelectedValue);
            if (areaId > 0)
            {
                DataRow area = DatabaseHelper.ExecuteQueryRowPublic(
                    "SELECT a.AreaID, a.RegionID, r.RegionName, r.ZoneID, z.ZoneName" +
                    " FROM SA_Areas a JOIN SA_Regions r ON r.RegionID=a.RegionID" +
                    " JOIN SA_Zones z ON z.ZoneID=r.ZoneID WHERE a.AreaID=?aid;",
                    new MySqlParameter("?aid", areaId));
                if (area != null)
                {
                    if (lblShipZone != null) lblShipZone.Text = area["ZoneName"].ToString();
                    if (lblShipRegion != null) lblShipRegion.Text = area["RegionName"].ToString();
                    if (hfShipZoneID != null) hfShipZoneID.Value = area["ZoneID"].ToString();
                    if (hfShipRegionID != null) hfShipRegionID.Value = area["RegionID"].ToString();
                    if (pnlZoneRegionInfo != null) pnlZoneRegionInfo.Visible = true;
                }
            }
            else
            {
                if (pnlZoneRegionInfo != null) pnlZoneRegionInfo.Visible = false;
            }
        }

        protected void ddlShipChannel_Changed(object sender, EventArgs e) { }

        // ── PROJECTION CRUD ───────────────────────────────────────────────

        protected void btnLoadProjection_Click(object sender, EventArgs e)
        {
            int areaId = Convert.ToInt32(ddlProjArea.SelectedValue);
            int channelId = Convert.ToInt32(ddlProjChannel.SelectedValue);
            if (areaId == 0 || channelId == 0)
            { ShowAlert("Please select Area and Channel.", false); return; }

            // Resolve zone/region from area
            ddlProjArea_Changed(null, null);

            DataRow proj = DatabaseHelper.ExecuteQueryRowPublic(
                "SELECT ProjectionID FROM SA_Projections WHERE ProjectionMonth=?m AND ProjectionYear=?y AND ChannelID=?c AND PositionID=?p;",
                new MySqlParameter("?m", SelMonth), new MySqlParameter("?y", SelYear),
                new MySqlParameter("?c", channelId), new MySqlParameter("?p", areaId));

            if (proj != null)
            {
                int projId = Convert.ToInt32(proj["ProjectionID"]);
                hfEditProjId.Value = projId.ToString();
                LoadProjLines(projId);
                ShowAlert("Projection already exists for this Area + Channel. Loaded for editing.", true);
            }
            else
            {
                hfEditProjId.Value = "0";
                var dt = new DataTable();
                dt.Columns.Add("LineID", typeof(int));
                dt.Columns.Add("ProductID", typeof(int));
                dt.Columns.Add("Quantity", typeof(int));
                dt.Columns.Add("UOMID", typeof(int));
                dt.Rows.Add(0, 0, 0, 0);
                rptProjLines.DataSource = dt;
                rptProjLines.DataBind();
                SetProjOptions();
            }
            pnlProjLines.Visible = true;
            if (pnlProjForm != null) pnlProjForm.Visible = true;
            BuildOptionHtml();
            RefreshData();
        }

        private void LoadProjLines(int projId)
        {
            DataTable lines = DatabaseHelper.ExecuteQueryPublic(
                "SELECT pl.LineID, pl.ProductID, pl.Quantity, pl.UOMID, p.ProductName, p.ProductCode" +
                " FROM SA_ProjectionLines pl JOIN PP_Products p ON p.ProductID=pl.ProductID" +
                " WHERE pl.ProjectionID=?pid ORDER BY p.ProductName;",
                new MySqlParameter("?pid", projId));
            if (lines.Rows.Count == 0)
            {
                if (!lines.Columns.Contains("UOMID")) lines.Columns.Add("UOMID", typeof(int));
                lines.Rows.Add(0, 0, 0, 0, "", "");
            }
            rptProjLines.DataSource = lines;
            rptProjLines.DataBind();
            SetProjOptions();
        }

        private void SetProjOptions()
        {
            foreach (RepeaterItem item in rptProjLines.Items)
            {
                var litP = item.FindControl("litProductOptions") as Literal;
                if (litP != null) litP.Text = hfProductOptionsHtml.Value;
                var litU = item.FindControl("litUOMOptions") as Literal;
                if (litU != null) litU.Text = hfUOMOptionsHtml.Value;
            }
        }

        protected void rptShipLines_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var row = (DataRowView)e.Item.DataItem;
            int savedPid = row["ProductID"] != DBNull.Value ? Convert.ToInt32(row["ProductID"]) : 0;

            // Product options with correct one selected
            var lit = e.Item.FindControl("litShipProductOptions") as Literal;
            if (lit != null)
            {
                DataTable products = DatabaseHelper.ExecuteQueryPublic(
                    "SELECT ProductID, ProductCode, ProductName FROM PP_Products WHERE IsActive=1 AND ProductType='Core' ORDER BY ProductName;");
                var sb = new System.Text.StringBuilder();
                foreach (DataRow r in products.Rows)
                {
                    int pid = Convert.ToInt32(r["ProductID"]);
                    string sel = (pid == savedPid) ? " selected" : "";
                    sb.Append("<option value='" + pid + "'" + sel + ">" +
                        r["ProductName"] + " (" + r["ProductCode"] + ")</option>");
                }
                lit.Text = sb.ToString();
            }

            // UOM options
            var litU = e.Item.FindControl("litShipUOMOptions") as Literal;
            if (litU != null) litU.Text = hfUOMOptionsHtml.Value;
        }

        protected void btnSaveProjection_Click(object sender, EventArgs e)
        {
            int areaId = Convert.ToInt32(ddlProjArea.SelectedValue);
            int channelId = Convert.ToInt32(ddlProjChannel.SelectedValue);
            if (areaId == 0 || channelId == 0)
            { ShowAlert("Select Area and Channel.", false); return; }

            int zoneId = hfProjZoneID != null ? Convert.ToInt32(hfProjZoneID.Value) : 0;
            int regionId = hfProjRegionID != null ? Convert.ToInt32(hfProjRegionID.Value) : 0;

            int projId = Convert.ToInt32(hfEditProjId.Value);

            if (projId == 0)
            {
                try
                {
                    DatabaseHelper.ExecuteNonQueryPublic(
                        "INSERT INTO SA_Projections (ProjectionMonth, ProjectionYear, StateID, ChannelID, ZoneID, RegionID, PositionID, CreatedBy)" +
                        " VALUES (?m,?y,0,?c,?z,?r,?p,?u);",
                        new MySqlParameter("?m", SelMonth), new MySqlParameter("?y", SelYear),
                        new MySqlParameter("?c", channelId), new MySqlParameter("?z", zoneId),
                        new MySqlParameter("?r", regionId), new MySqlParameter("?p", areaId),
                        new MySqlParameter("?u", UserID));
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Duplicate"))
                    { ShowAlert("Projection already exists for this Area + Channel this month.", false); return; }
                    throw;
                }

                DataRow newProj = DatabaseHelper.ExecuteQueryRowPublic(
                    "SELECT ProjectionID FROM SA_Projections WHERE ProjectionMonth=?m AND ProjectionYear=?y AND ChannelID=?c AND PositionID=?p;",
                    new MySqlParameter("?m", SelMonth), new MySqlParameter("?y", SelYear),
                    new MySqlParameter("?c", channelId), new MySqlParameter("?p", areaId));
                if (newProj != null) projId = Convert.ToInt32(newProj["ProjectionID"]);
            }

            if (projId == 0) { ShowAlert("Error creating projection.", false); return; }

            // Clear and re-insert lines
            DatabaseHelper.ExecuteNonQueryPublic("DELETE FROM SA_ProjectionLines WHERE ProjectionID=?pid;",
                new MySqlParameter("?pid", projId));

            string[] products = Request.Form.GetValues("proj_product");
            string[] qtys = Request.Form.GetValues("proj_qty");
            string[] uoms = Request.Form.GetValues("proj_uom");
            if (products != null && qtys != null)
            {
                for (int i = 0; i < products.Length; i++)
                {
                    int pid = 0; int.TryParse(products[i], out pid);
                    int qty = 0; int.TryParse(qtys[i], out qty);
                    int uid = 0; if (uoms != null && i < uoms.Length) int.TryParse(uoms[i], out uid);
                    if (pid > 0 && qty > 0)
                    {
                        DatabaseHelper.ExecuteNonQueryPublic(
                            "INSERT INTO SA_ProjectionLines (ProjectionID, ProductID, Quantity, UOMID) VALUES (?pid,?prod,?qty,?uom)" +
                            " ON DUPLICATE KEY UPDATE Quantity=?qty2, UOMID=?uom2;",
                            new MySqlParameter("?pid", projId), new MySqlParameter("?prod", pid),
                            new MySqlParameter("?qty", qty), new MySqlParameter("?uom", uid > 0 ? (object)uid : DBNull.Value),
                            new MySqlParameter("?qty2", qty), new MySqlParameter("?uom2", uid > 0 ? (object)uid : DBNull.Value));
                    }
                }
            }

            ShowAlert("Projection saved.", true);
            pnlProjLines.Visible = false;
            hfEditProjId.Value = "0";
            RefreshData();
        }

        protected void ProjAction_Command(object sender, CommandEventArgs e)
        {
            int projId = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "EditProj")
            {
                DataRow proj = DatabaseHelper.ExecuteQueryRowPublic(
                    "SELECT p.*, z.ZoneName, r.RegionName, ar.AreaName AS AreaName, c.ChannelName" +
                    " FROM SA_Projections p" +
                    " LEFT JOIN SA_Zones z ON z.ZoneID=p.ZoneID" +
                    " LEFT JOIN SA_Regions r ON r.RegionID=p.RegionID" +
                    " LEFT JOIN SA_Areas ar ON ar.AreaID=p.PositionID" +
                    " LEFT JOIN SA_Channels c ON c.ChannelID=p.ChannelID" +
                    " WHERE p.ProjectionID=?pid;",
                    new MySqlParameter("?pid", projId));
                if (proj == null) return;

                // Set Area dropdown
                if (proj["PositionID"] != DBNull.Value)
                {
                    var li = ddlProjArea.Items.FindByValue(proj["PositionID"].ToString());
                    if (li != null) ddlProjArea.SelectedValue = proj["PositionID"].ToString();
                }
                if (proj["ChannelID"] != DBNull.Value)
                    ddlProjChannel.SelectedValue = proj["ChannelID"].ToString();

                // Display resolved zone/region
                if (lblProjZone != null) lblProjZone.Text = proj["ZoneName"]?.ToString() ?? "—";
                if (lblProjRegion != null) lblProjRegion.Text = proj["RegionName"]?.ToString() ?? "—";
                if (hfProjZoneID != null && proj["ZoneID"] != DBNull.Value) hfProjZoneID.Value = proj["ZoneID"].ToString();
                if (hfProjRegionID != null && proj["RegionID"] != DBNull.Value) hfProjRegionID.Value = proj["RegionID"].ToString();
                if (pnlProjZoneRegion != null) pnlProjZoneRegion.Visible = true;

                hfEditProjId.Value = projId.ToString();
                LoadProjLines(projId);
                pnlProjLines.Visible = true;
                if (pnlProjForm != null) pnlProjForm.Visible = true;
                BuildOptionHtml();
            }
            else if (e.CommandName == "ConfirmProj")
            {
                DatabaseHelper.ExecuteNonQueryPublic(
                    "UPDATE SA_Projections SET Status='Confirmed' WHERE ProjectionID=?pid;",
                    new MySqlParameter("?pid", projId));
                ShowAlert("Projection confirmed.", true);
            }
            else if (e.CommandName == "DeleteProj")
            {
                DatabaseHelper.ExecuteNonQueryPublic("DELETE FROM SA_ProjectionLines WHERE ProjectionID=?pid;",
                    new MySqlParameter("?pid", projId));
                DatabaseHelper.ExecuteNonQueryPublic("DELETE FROM SA_Projections WHERE ProjectionID=?pid;",
                    new MySqlParameter("?pid", projId));
                ShowAlert("Projection deleted.", true);
            }
            RefreshData();
        }

        private void LoadProjectionsList()
        {
            DataTable dt = DatabaseHelper.ExecuteQueryPublic(
                "SELECT p.ProjectionID, IFNULL(z.ZoneName,'—') AS ZoneName, IFNULL(r.RegionName,'—') AS RegionName," +
                " IFNULL(ar.AreaName,'—') AS AreaName, c.ChannelName, p.Status," +
                " COUNT(pl.LineID) AS ProductCount, IFNULL(SUM(pl.Quantity),0) AS TotalQty" +
                " FROM SA_Projections p" +
                " LEFT JOIN SA_Zones z ON z.ZoneID=p.ZoneID" +
                " LEFT JOIN SA_Regions r ON r.RegionID=p.RegionID" +
                " LEFT JOIN SA_Areas ar ON ar.AreaID=p.PositionID" +
                " JOIN SA_Channels c ON c.ChannelID=p.ChannelID" +
                " LEFT JOIN SA_ProjectionLines pl ON pl.ProjectionID=p.ProjectionID" +
                " WHERE p.ProjectionMonth=?m AND p.ProjectionYear=?y" +
                " GROUP BY p.ProjectionID, z.ZoneName, r.RegionName, ar.AreaName, c.ChannelName, p.Status" +
                " ORDER BY z.ZoneName, r.RegionName, ar.AreaName;",
                new MySqlParameter("?m", SelMonth), new MySqlParameter("?y", SelYear));
            rptProjections.DataSource = dt;
            rptProjections.DataBind();
            pnlProjEmpty.Visible = dt.Rows.Count == 0;
        }

        // ── SHIPMENTS ─────────────────────────────────────────────────────

        // Save as "Saved" — not visible to Shipment team
        protected void btnSaveShipment_Click(object sender, EventArgs e)
        { SaveOrCreateShipment("Saved"); }

        // Create Order — visible to Shipment team
        protected void btnCreateShipment_Click(object sender, EventArgs e)
        { SaveOrCreateShipment("Order"); }

        private void SaveOrCreateShipment(string status)
        {
            int zoneId = hfShipZoneID != null ? Convert.ToInt32(hfShipZoneID.Value) : 0;
            int regionId = hfShipRegionID != null ? Convert.ToInt32(hfShipRegionID.Value) : 0;
            int areaId = Convert.ToInt32(ddlShipArea.SelectedValue);
            int channelId = Convert.ToInt32(ddlShipChannel.SelectedValue);
            int custId = ddlCustomer != null ? Convert.ToInt32(ddlCustomer.SelectedValue) : 0;
            string shipDate = txtShipDate.Text.Trim();
            if (channelId == 0 || string.IsNullOrEmpty(shipDate))
            { ShowAlert("Please fill Date and Channel.", false); return; }

            int transportId = Convert.ToInt32(ddlTransport.SelectedValue);
            int editShipId = hfEditShipId != null ? Convert.ToInt32(hfEditShipId.Value) : 0;

            // Get projection ID
            DataRow proj = DatabaseHelper.ExecuteQueryRowPublic(
                "SELECT ProjectionID FROM SA_Projections WHERE ProjectionMonth=?m AND ProjectionYear=?y AND ChannelID=?c AND PositionID=?p;",
                new MySqlParameter("?m", SelMonth), new MySqlParameter("?y", SelYear),
                new MySqlParameter("?c", channelId), new MySqlParameter("?p", areaId));
            int projId = proj != null ? Convert.ToInt32(proj["ProjectionID"]) : 0;

            int shipId;
            if (editShipId > 0)
            {
                // Update existing shipment
                DatabaseHelper.ExecuteNonQueryPublic(
                    "UPDATE SA_Shipments SET CustomerID=?cust, ShipmentDate=?dt, ChannelID=?c, ZoneID=?z, RegionID=?r, PositionID=?p," +
                    " TransportModeID=?tm, VehicleNo=?vn, Status=?st WHERE ShipmentID=?sid;",
                    new MySqlParameter("?cust", custId > 0 ? (object)custId : DBNull.Value),
                    new MySqlParameter("?dt", DateTime.Parse(shipDate)),
                    new MySqlParameter("?c", channelId),
                    new MySqlParameter("?z", zoneId > 0 ? (object)zoneId : DBNull.Value),
                    new MySqlParameter("?r", regionId > 0 ? (object)regionId : DBNull.Value),
                    new MySqlParameter("?p", areaId > 0 ? (object)areaId : DBNull.Value),
                    new MySqlParameter("?tm", transportId > 0 ? (object)transportId : DBNull.Value),
                    new MySqlParameter("?vn", txtVehicleNo.Text.Trim()),
                    new MySqlParameter("?st", status),
                    new MySqlParameter("?sid", editShipId));
                shipId = editShipId;
                // Delete old lines
                DatabaseHelper.ExecuteNonQueryPublic("DELETE FROM SA_ShipmentLines WHERE ShipmentID=?sid;",
                    new MySqlParameter("?sid", shipId));
            }
            else
            {
                // Insert new
                DatabaseHelper.ExecuteNonQueryPublic(
                    "INSERT INTO SA_Shipments (ProjectionID, CustomerID, ShipmentDate, StateID, ChannelID, ZoneID, RegionID, PositionID, TransportModeID, VehicleNo, Status, CreatedBy)" +
                    " VALUES (?pid,?cust,?dt,0,?c,?z,?r,?p,?tm,?vn,?st,?u);",
                    new MySqlParameter("?pid", projId > 0 ? (object)projId : DBNull.Value),
                    new MySqlParameter("?cust", custId > 0 ? (object)custId : DBNull.Value),
                    new MySqlParameter("?dt", DateTime.Parse(shipDate)),
                    new MySqlParameter("?c", channelId),
                    new MySqlParameter("?z", zoneId > 0 ? (object)zoneId : DBNull.Value),
                    new MySqlParameter("?r", regionId > 0 ? (object)regionId : DBNull.Value),
                    new MySqlParameter("?p", areaId > 0 ? (object)areaId : DBNull.Value),
                    new MySqlParameter("?tm", transportId > 0 ? (object)transportId : DBNull.Value),
                    new MySqlParameter("?vn", txtVehicleNo.Text.Trim()),
                    new MySqlParameter("?st", status),
                    new MySqlParameter("?u", UserID));
                object shipIdObj = DatabaseHelper.ExecuteScalarPublic("SELECT LAST_INSERT_ID();");
                shipId = Convert.ToInt32(shipIdObj);

                // Link to consignment if one is active
                int consigId = hfSAConsigId != null ? Convert.ToInt32(hfSAConsigId.Value) : 0;
                if (consigId > 0)
                    DatabaseHelper.LinkShipmentToConsignment(shipId, consigId);
            }

            // Save line items — read from ship_product dropdowns
            string[] productIds = Request.Form.GetValues("ship_product");
            string[] shipQtys = Request.Form.GetValues("ship_qty");
            if (productIds != null && shipQtys != null)
            {
                for (int i = 0; i < productIds.Length; i++)
                {
                    int pid = 0; int.TryParse(productIds[i], out pid);
                    int sq = 0; int.TryParse(shipQtys[i], out sq);
                    if (pid > 0 && sq > 0)
                    {
                        DatabaseHelper.ExecuteNonQueryPublic(
                            "INSERT INTO SA_ShipmentLines (ShipmentID, ProductID, ProjectedQty, ShippedQty) VALUES (?sid,?pid,0,?sq);",
                            new MySqlParameter("?sid", shipId), new MySqlParameter("?pid", pid),
                            new MySqlParameter("?sq", sq));
                    }
                }
            }

            string shipNo = "SH-" + shipId.ToString("D5");
            string msg = status == "Saved" ? "Shipment " + shipNo + " saved as draft." : "Shipment Order " + shipNo + " created — visible to Shipment team.";
            ShowAlert(msg, true);
            pnlShipLines.Visible = false;
            txtShipDate.Text = ""; txtVehicleNo.Text = "";
            if (hfEditShipId != null) hfEditShipId.Value = "0";
            if (lblEditingShipId != null) lblEditingShipId.Text = "";
            RefreshData();
        }

        private void LoadShipmentsList()
        {
            DataTable dt = DatabaseHelper.ExecuteQueryPublic(
                "SELECT sh.ShipmentID, sh.ShipmentDate," +
                " IFNULL(cust.CustomerName,'—') AS CustomerName," +
                " IFNULL(z.ZoneName,'—') AS ZoneName, IFNULL(r.RegionName,'—') AS RegionName," +
                " IFNULL(ar.AreaName,'—') AS AreaName," +
                " c.ChannelName, IFNULL(tm.ModeName,'—') AS TransportMode, sh.Status," +
                " COUNT(sl.LineID) AS ProductCount" +
                " FROM SA_Shipments sh" +
                " LEFT JOIN PK_Customers cust ON cust.CustomerID=sh.CustomerID" +
                " LEFT JOIN SA_Zones z ON z.ZoneID=sh.ZoneID" +
                " LEFT JOIN SA_Regions r ON r.RegionID=sh.RegionID" +
                " LEFT JOIN SA_Areas ar ON ar.AreaID=sh.PositionID" +
                " JOIN SA_Channels c ON c.ChannelID=sh.ChannelID" +
                " LEFT JOIN SA_TransportModes tm ON tm.ModeID=sh.TransportModeID" +
                " LEFT JOIN SA_ShipmentLines sl ON sl.ShipmentID=sh.ShipmentID" +
                " WHERE MONTH(sh.ShipmentDate)=?m AND YEAR(sh.ShipmentDate)=?y" +
                " GROUP BY sh.ShipmentID ORDER BY sh.ShipmentDate DESC;",
                new MySqlParameter("?m", SelMonth), new MySqlParameter("?y", SelYear));
            rptShipments.DataSource = dt;
            rptShipments.DataBind();
            pnlShipEmpty.Visible = dt.Rows.Count == 0;
        }

        protected void ShipAction_Command(object sender, CommandEventArgs e)
        {
            int shipId = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "EditShip")
            {
                DataRow ship = DatabaseHelper.ExecuteQueryRowPublic(
                    "SELECT sh.*, IFNULL(z.ZoneName,'—') AS ZoneName, IFNULL(r.RegionName,'—') AS RegionName" +
                    " FROM SA_Shipments sh" +
                    " LEFT JOIN SA_Zones z ON z.ZoneID=sh.ZoneID" +
                    " LEFT JOIN SA_Regions r ON r.RegionID=sh.RegionID" +
                    " WHERE sh.ShipmentID=?sid;",
                    new MySqlParameter("?sid", shipId));
                if (ship == null) return;

                hfEditShipId.Value = shipId.ToString();
                if (lblEditingShipId != null) lblEditingShipId.Text = "— Editing SH-" + shipId.ToString("D5");
                txtShipDate.Text = Convert.ToDateTime(ship["ShipmentDate"]).ToString("yyyy-MM-dd");

                // Set Area
                if (ship["PositionID"] != DBNull.Value)
                {
                    var li = ddlShipArea.Items.FindByValue(ship["PositionID"].ToString());
                    if (li != null) ddlShipArea.SelectedValue = ship["PositionID"].ToString();
                }
                // Set Channel
                if (ship["ChannelID"] != DBNull.Value)
                    ddlShipChannel.SelectedValue = ship["ChannelID"].ToString();
                // Set Transport
                if (ship["TransportModeID"] != DBNull.Value)
                {
                    var li = ddlTransport.Items.FindByValue(ship["TransportModeID"].ToString());
                    if (li != null) ddlTransport.SelectedValue = ship["TransportModeID"].ToString();
                }
                // Set Vehicle
                txtVehicleNo.Text = ship["VehicleNo"]?.ToString() ?? "";
                // Set Customer
                if (ddlCustomer != null && ship["CustomerID"] != DBNull.Value)
                {
                    var li = ddlCustomer.Items.FindByValue(ship["CustomerID"].ToString());
                    if (li != null) ddlCustomer.SelectedValue = ship["CustomerID"].ToString();
                }
                // Zone/Region display
                if (lblShipZone != null) lblShipZone.Text = ship["ZoneName"].ToString();
                if (lblShipRegion != null) lblShipRegion.Text = ship["RegionName"].ToString();
                if (hfShipZoneID != null && ship["ZoneID"] != DBNull.Value) hfShipZoneID.Value = ship["ZoneID"].ToString();
                if (hfShipRegionID != null && ship["RegionID"] != DBNull.Value) hfShipRegionID.Value = ship["RegionID"].ToString();
                if (pnlZoneRegionInfo != null) pnlZoneRegionInfo.Visible = true;

                // Load existing line items
                DataTable lines = DatabaseHelper.ExecuteQueryPublic(
                    "SELECT sl.ProductID, p.ProductName, sl.ShippedQty AS Quantity" +
                    " FROM SA_ShipmentLines sl JOIN PP_Products p ON p.ProductID=sl.ProductID" +
                    " WHERE sl.ShipmentID=?sid ORDER BY p.ProductName;",
                    new MySqlParameter("?sid", shipId));
                rptShipLines.DataSource = lines;
                rptShipLines.DataBind();
                BuildOptionHtml();
                pnlShipLines.Visible = lines.Rows.Count > 0;
                if (pnlShipForm != null) pnlShipForm.Visible = true;
            }
            else if (e.CommandName == "DeleteShip")
            {
                // Check status - only allow delete for non-Shipped
                DataRow ship = DatabaseHelper.ExecuteQueryRowPublic(
                    "SELECT Status FROM SA_Shipments WHERE ShipmentID=?sid;",
                    new MySqlParameter("?sid", shipId));
                if (ship != null && ship["Status"].ToString() == "Shipped")
                { ShowAlert("Cannot delete — shipment already shipped.", false); }
                else
                {
                    DatabaseHelper.ExecuteNonQueryPublic("DELETE FROM SA_ShipmentLines WHERE ShipmentID=?sid;",
                        new MySqlParameter("?sid", shipId));
                    DatabaseHelper.ExecuteNonQueryPublic("DELETE FROM SA_Shipments WHERE ShipmentID=?sid;",
                        new MySqlParameter("?sid", shipId));
                    ShowAlert("Shipment deleted.", true);
                }
            }
            RefreshData();
        }

        protected string GetShipStatusBadge(string status)
        {
            switch (status)
            {
                case "Saved": return "badge-saved";
                case "Order": return "badge-order";
                case "DC": return "badge-order";
                case "Shipped": return "badge-shipped";
                default: return "badge-draft";
            }
        }

        private void ShowAlert(string msg, bool success)
        { pnlAlert.Visible = true; lblAlert.Text = msg; pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger"); }

        // ══════════════════════════════════════════════════════════════
        // SALES FORCE CONSIGNMENT HANDLERS
        // ══════════════════════════════════════════════════════════════

        void BindSAConsigTabs()
        {
            var dt = DatabaseHelper.GetAllConsignments(50);
            if (rptSAConsigTabs != null) { rptSAConsigTabs.DataSource = dt; rptSAConsigTabs.DataBind(); }
            int csgId = 0;
            int.TryParse(hfSAConsigId.Value, out csgId);
            bool hasSelection = csgId > 0;
            if (pnlConsigEmpty != null) pnlConsigEmpty.Visible = !hasSelection;

            // Dropdowns for DISPATCHED + ARCHIVED — consignments that have left the active tab strip
            BindSADispatchedDropdown();
            BindSAArchivedDropdown();
        }

        /// <summary>Populate the Dispatched dropdown with all consignments in DISPATCHED status.
        /// Mirrors the pattern used by the PK Shipment page.</summary>
        void BindSADispatchedDropdown()
        {
            if (ddlSADispatched == null) return;
            var dt = DatabaseHelper.GetDispatchedConsignmentsSA();
            ddlSADispatched.Items.Clear();
            ddlSADispatched.Items.Add(new ListItem("Dispatched (" + dt.Rows.Count + ")", "0"));
            foreach (DataRow r in dt.Rows)
                ddlSADispatched.Items.Add(new ListItem(r["ConsignmentCode"].ToString(), r["ConsignmentID"].ToString()));

            // Keep the currently viewed dispatched consignment selected across postbacks
            int csgId = 0;
            int.TryParse(hfSAConsigId.Value, out csgId);
            if (csgId > 0)
            {
                var li = ddlSADispatched.Items.FindByValue(csgId.ToString());
                if (li != null) ddlSADispatched.SelectedValue = csgId.ToString();
            }
        }

        /// <summary>Populate the Archived dropdown.</summary>
        void BindSAArchivedDropdown()
        {
            if (ddlSAArchived == null) return;
            var dt = DatabaseHelper.GetArchivedConsignmentsSA();
            ddlSAArchived.Items.Clear();
            ddlSAArchived.Items.Add(new ListItem("Archived (" + dt.Rows.Count + ")", "0"));
            foreach (DataRow r in dt.Rows)
                ddlSAArchived.Items.Add(new ListItem(r["ConsignmentCode"].ToString(), r["ConsignmentID"].ToString()));

            int csgId = 0;
            int.TryParse(hfSAConsigId.Value, out csgId);
            if (csgId > 0)
            {
                var li = ddlSAArchived.Items.FindByValue(csgId.ToString());
                if (li != null) ddlSAArchived.SelectedValue = csgId.ToString();
            }
        }

        /// <summary>Open a dispatched consignment for viewing (same load path as clicking an active tab).</summary>
        protected void ddlSADispatched_Changed(object s, EventArgs e)
        {
            int csgId = Convert.ToInt32(ddlSADispatched.SelectedValue);
            if (csgId > 0)
            {
                hfSAConsigId.Value = csgId.ToString();
                if (hfEditShipId != null) hfEditShipId.Value = "0";
                BindSAConsigTabs();
                LoadSAConsigDetail(csgId);
            }
        }

        protected void ddlSAArchived_Changed(object s, EventArgs e)
        {
            int csgId = Convert.ToInt32(ddlSAArchived.SelectedValue);
            if (csgId > 0)
            {
                hfSAConsigId.Value = csgId.ToString();
                if (hfEditShipId != null) hfEditShipId.Value = "0";
                BindSAConsigTabs();
                LoadSAConsigDetail(csgId);
            }
        }

        void BindSAConsigCustomers()
        {
            if (ddlSACustomer == null) return;
            var dt = DatabaseHelper.ExecuteQueryPublic(
                "SELECT c.CustomerID, c.CustomerCode, c.CustomerName, IFNULL(ct.TypeName,'') AS TypeName" +
                " FROM PK_Customers c LEFT JOIN PK_CustomerTypes ct ON ct.TypeCode=c.CustomerType" +
                " WHERE c.IsActive=1 AND c.CustomerType IN ('DI','ST') ORDER BY c.CustomerName;");
            ddlSACustomer.Items.Clear();
            ddlSACustomer.Items.Add(new ListItem("-- Select Customer --", "0"));
            foreach (DataRow r in dt.Rows)
                ddlSACustomer.Items.Add(new ListItem(r["CustomerName"] + " (" + r["CustomerCode"] + ") — " + r["TypeName"], r["CustomerID"].ToString()));
        }

        void BindSAConsigChannels()
        {
            if (ddlSAChannel == null) return;
            var dt = DatabaseHelper.ExecuteQueryPublic("SELECT ChannelID, ChannelName FROM SA_Channels ORDER BY ChannelName;");
            ddlSAChannel.Items.Clear();
            ddlSAChannel.Items.Add(new ListItem("-- Select Channel --", "0"));
            foreach (DataRow r in dt.Rows)
                ddlSAChannel.Items.Add(new ListItem(r["ChannelName"].ToString(), r["ChannelID"].ToString()));
        }

        protected void btnSACreateConsig_Click(object s, EventArgs e)
        {
            try
            {
                DateTime dt = DateTime.Parse(txtSAConsigDate.Text);
                string userText = txtSAConsigText.Text.Trim();
                if (string.IsNullOrEmpty(userText))
                { ShowAlert("Please enter a consignment identifier (e.g. ROTN, BLORE).", false); return; }

                int newId = DatabaseHelper.CreateConsignment(dt, userText, "", UserID);
                txtSAConsigText.Text = "";
                hfSAConsigId.Value = newId.ToString();
                BindSAConsigTabs();
                LoadSAConsigDetail(newId);

                var csg = DatabaseHelper.GetConsignmentById(newId);
                ShowAlert("Consignment created: " + (csg != null ? csg["ConsignmentCode"].ToString() : ""), true);
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        protected void SAConsig_Command(object s, CommandEventArgs e)
        {
            if (e.CommandName == "OpenConsig")
            {
                int csgId = Convert.ToInt32(e.CommandArgument);
                hfSAConsigId.Value = csgId.ToString();
                BindSAConsigTabs();
                LoadSAConsigDetail(csgId);
            }
        }

        void LoadSAConsigDetail(int consignmentId)
        {
            if (pnlSAConsigDetail == null) return;
            var csg = DatabaseHelper.GetConsignmentById(consignmentId);
            if (csg == null) { pnlSAConsigDetail.Visible = false; return; }

            pnlSAConsigDetail.Visible = true;
            if (pnlConsigEmpty != null) pnlConsigEmpty.Visible = false;
            if (lblSAConsigTitle != null)
                lblSAConsigTitle.Text = csg["ConsignmentCode"].ToString();

            string status = csg["Status"].ToString();
            if (lblSAConsigStatus != null)
            {
                lblSAConsigStatus.Text = status;
                switch (status)
                {
                    case "OPEN": lblSAConsigStatus.Style["background"] = "#fff3cd"; lblSAConsigStatus.Style["color"] = "#856404"; break;
                    case "READY": lblSAConsigStatus.Style["background"] = "#d4edda"; lblSAConsigStatus.Style["color"] = "#155724"; break;
                    case "DISPATCHED": lblSAConsigStatus.Style["background"] = "#cce5ff"; lblSAConsigStatus.Style["color"] = "#004085"; break;
                    case "ARCHIVED": lblSAConsigStatus.Style["background"] = "#e2e3e5"; lblSAConsigStatus.Style["color"] = "#383d41"; break;
                    default: lblSAConsigStatus.Style["background"] = "#e2e3e5"; lblSAConsigStatus.Style["color"] = "#383d41"; break;
                }
            }

            // Lock rule: only OPEN + READY allow writes. DISPATCHED and ARCHIVED are view-only.
            _saConsigIsLocked = (status != "OPEN" && status != "READY");
            if (pnlSAShipForm != null) pnlSAShipForm.Visible = !_saConsigIsLocked;
            if (pnlSAConsigLocked != null) pnlSAConsigLocked.Visible = _saConsigIsLocked;
            if (lblSALockedStatus != null) lblSALockedStatus.Text = status.ToLower();
            // If user was mid-edit when something moved the consignment (unlikely but possible), bail out of edit mode
            if (_saConsigIsLocked && hfEditShipId != null) hfEditShipId.Value = "0";

            var orders = DatabaseHelper.GetShipmentsByConsignment(consignmentId);
            if (rptSAConsigOrders != null) { rptSAConsigOrders.DataSource = orders; rptSAConsigOrders.DataBind(); }
            if (pnlSAConsigOrdersEmpty != null) pnlSAConsigOrdersEmpty.Visible = orders.Rows.Count == 0;

            // Show "Send to Shipment Team" only for active consignments with drafts
            bool hasSaved = false;
            foreach (DataRow r in orders.Rows)
                if (r["Status"].ToString() == "Saved") { hasSaved = true; break; }
            if (btnSASendToPK != null) btnSASendToPK.Visible = hasSaved && !_saConsigIsLocked;

            // Bind form dropdowns + product data (only needed when form is visible)
            if (!_saConsigIsLocked)
            {
                BindSAConsigCustomers();
                BindSAConsigChannels();
                BuildSAProductData();

                // If not in edit mode, reset the form and render a single blank row
                int editShipId = hfEditShipId != null ? Convert.ToInt32(hfEditShipId.Value) : 0;
                if (editShipId <= 0)
                {
                    if (txtSAShipDate != null) txtSAShipDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    if (lblSAEditShipId != null) lblSAEditShipId.Text = "";
                    if (lblSAShipFormTitle != null) lblSAShipFormTitle.Text = "New Shipment Order";
                    if (btnSACancelEdit != null) btnSACancelEdit.Visible = false;
                    if (btnSACreateOrder != null) btnSACreateOrder.Text = "Create Shipment Order";
                    BindSAShipLines(CreateBlankLinesTable());
                }
            }
        }

        /// <summary>Per-row visibility for Edit/Delete — belt-and-braces with the ItemTemplate's Status-based Visible expression.
        /// Hides both buttons if the whole consignment is locked (DISPATCHED/ARCHIVED).</summary>
        protected void rptSAConsigOrders_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;
            if (!_saConsigIsLocked) return;
            var edit = e.Item.FindControl("lnkEditOrder") as LinkButton;
            var del  = e.Item.FindControl("lnkDeleteOrder") as LinkButton;
            if (edit != null) edit.Visible = false;
            if (del != null) del.Visible = false;
        }

        /// <summary>Server-side guard used by all write handlers. Returns true if writes are allowed.
        /// Shows an alert and refreshes the view if not.</summary>
        private bool AssertSAConsigWritable(int consignmentId)
        {
            var csg = DatabaseHelper.GetConsignmentById(consignmentId);
            if (csg == null) { ShowAlert("Consignment not found.", false); return false; }
            string st = csg["Status"].ToString();
            if (st == "OPEN" || st == "READY") return true;
            ShowAlert("Cannot modify — consignment is " + st + " (view only).", false);
            LoadSAConsigDetail(consignmentId);
            return false;
        }

        /// <summary>
        /// Pull product list + FG stock + case qty + container type. Populates:
        /// - hfSAProductData with JSON {pid:{stock,caseQty,containerType}}
        /// - hfSAProductOptionsHtml with raw &lt;option&gt; list for JS line-addition
        /// Stock query mirrors the PK DC form's accuracy: opening stock + production
        /// - secondary packing reservations - already-issued DC lines.
        /// </summary>
        private void BuildSAProductData()
        {
            var fgStock = DatabaseHelper.ExecuteQueryPublic(
                "SELECT p.ProductID, p.ProductName, p.ProductCode," +
                " IFNULL(p.ContainerType,'JAR') AS ContainerType," +
                " IFNULL(p.ContainersPerCase, 12) AS ContainersPerCase," +
                " (IFNULL(osJ.Jars,0)" +
                "  + FLOOR(IFNULL(fg.TotalPCS,0)/GREATEST(CAST(SUBSTRING_INDEX(IFNULL(p.UnitsPerContainer,'1'),',',1) AS UNSIGNED),1))" +
                "  - IFNULL(sp.JarsInCases,0) - IFNULL(dcA.LooseUsed,0))" +
                " + (IFNULL(osC.Cases,0) + IFNULL(sp.CasesPacked,0) - IFNULL(dcA.CasesUsed,0))" +
                "   * IFNULL(p.ContainersPerCase,12) AS AvailableFGJars" +
                " FROM PP_Products p" +
                " LEFT JOIN (SELECT ProductID, SUM(Quantity) AS Jars FROM PK_FGOpeningStock WHERE StockForm IN ('JAR','BOX') GROUP BY ProductID) osJ ON osJ.ProductID=p.ProductID" +
                " LEFT JOIN (SELECT ProductID, SUM(Quantity) AS Cases FROM PK_FGOpeningStock WHERE StockForm='CASE' GROUP BY ProductID) osC ON osC.ProductID=p.ProductID" +
                " LEFT JOIN (SELECT ProductID, SUM(QtyPacked) AS TotalPCS FROM PK_FGStock GROUP BY ProductID) fg ON fg.ProductID=p.ProductID" +
                " LEFT JOIN (SELECT ProductID, SUM(TotalUnits) AS JarsInCases, SUM(CASE WHEN PackingType='CASE' THEN QtyCartons ELSE 0 END) AS CasesPacked FROM PK_SecondaryPacking GROUP BY ProductID) sp ON sp.ProductID=p.ProductID" +
                " LEFT JOIN (SELECT dl.ProductID," +
                "   SUM(CASE WHEN IFNULL(dl.Source,'CASE')='CASE' THEN CEIL(dl.TotalPcs/GREATEST(IFNULL(pp.ContainersPerCase,12),1)) ELSE 0 END) AS CasesUsed," +
                "   SUM(CASE WHEN IFNULL(dl.Source,'CASE')='LOOSE' THEN dl.TotalPcs ELSE 0 END) AS LooseUsed" +
                "   FROM PK_DCLines dl JOIN PK_DeliveryChallans dch ON dch.DCID=dl.DCID JOIN PP_Products pp ON pp.ProductID=dl.ProductID" +
                "   WHERE dch.Status IN ('DRAFT','FINALISED') GROUP BY dl.ProductID) dcA ON dcA.ProductID=p.ProductID" +
                " WHERE p.IsActive=1 AND p.ProductType IN ('Core','Conversion','Prefilled Conversion')" +
                " ORDER BY p.ProductName;");

            // Cache for rptSAShipLines_ItemDataBound so we don't re-query per row
            ViewState["SAProductList"] = fgStock;

            var jsonSb = new System.Text.StringBuilder("{");
            var optsSb = new System.Text.StringBuilder();
            bool first = true;
            foreach (DataRow fg in fgStock.Rows)
            {
                string pid = fg["ProductID"].ToString();
                string name = fg["ProductName"].ToString();
                string code = fg["ProductCode"].ToString();
                int stock = Convert.ToInt32(fg["AvailableFGJars"]);
                int caseQty = Convert.ToInt32(fg["ContainersPerCase"]);
                string ct = fg["ContainerType"].ToString();

                // Raw options HTML for JS line-addition — escape quotes just in case
                string nameEsc = name.Replace("\"", "&quot;");
                string codeEsc = code.Replace("\"", "&quot;");
                optsSb.Append("<option value=\"").Append(pid).Append("\">")
                      .Append(nameEsc).Append(" (FG:").Append(stock).Append(")</option>");

                if (!first) jsonSb.Append(",");
                jsonSb.Append("\"").Append(pid).Append("\":{")
                      .Append("\"stock\":").Append(stock)
                      .Append(",\"caseQty\":").Append(caseQty)
                      .Append(",\"containerType\":\"").Append(ct).Append("\"")
                      .Append("}");
                first = false;
            }
            jsonSb.Append("}");

            if (hfSAProductData != null) hfSAProductData.Value = jsonSb.ToString();
            if (hfSAProductOptionsHtml != null) hfSAProductOptionsHtml.Value = optsSb.ToString();
        }

        /// <summary>Bind the repeater that renders shipment order product lines. Pass CreateBlankLinesTable() for a fresh form.</summary>
        private void BindSAShipLines(DataTable lines)
        {
            if (rptSAShipLines == null) return;
            rptSAShipLines.DataSource = lines;
            rptSAShipLines.DataBind();
        }

        /// <summary>Per-row repeater binding — builds the product &lt;option&gt; list with the saved
        /// product pre-selected, and the selling form &lt;option&gt; list with the saved form pre-selected.</summary>
        protected void rptSAShipLines_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;
            var row = (DataRowView)e.Item.DataItem;
            int savedPid = row["ProductID"] != DBNull.Value ? Convert.ToInt32(row["ProductID"]) : 0;
            string savedForm = row.Row.Table.Columns.Contains("SellingForm") && row["SellingForm"] != DBNull.Value
                ? row["SellingForm"].ToString() : "JAR";

            // Products
            var lit = e.Item.FindControl("litSAProductOptions") as Literal;
            if (lit != null)
            {
                var products = ViewState["SAProductList"] as DataTable;
                if (products == null)
                {
                    // Fallback — should have been built by BuildSAProductData() but guard anyway
                    BuildSAProductData();
                    products = ViewState["SAProductList"] as DataTable;
                }
                var sb = new System.Text.StringBuilder();
                if (products != null)
                {
                    foreach (DataRow p in products.Rows)
                    {
                        int pid = Convert.ToInt32(p["ProductID"]);
                        int stock = Convert.ToInt32(p["AvailableFGJars"]);
                        string sel = (pid == savedPid) ? " selected" : "";
                        string name = p["ProductName"].ToString().Replace("\"", "&quot;");
                        sb.Append("<option value=\"").Append(pid).Append("\"").Append(sel).Append(">")
                          .Append(name).Append(" (FG:").Append(stock).Append(")</option>");
                    }
                }
                lit.Text = sb.ToString();
            }

            // Selling form — build with savedForm selected
            var litForm = e.Item.FindControl("litSAFormOptions") as Literal;
            if (litForm != null)
            {
                var fsb = new System.Text.StringBuilder();
                string[] forms = new[] { "JAR", "BOX", "PCS", "CASE" };
                foreach (var f in forms)
                {
                    string sel = f == savedForm ? " selected" : "";
                    fsb.Append("<option value=\"").Append(f).Append("\"").Append(sel).Append(">").Append(f).Append("</option>");
                }
                litForm.Text = fsb.ToString();
            }
        }

        /// <summary>Single blank row for a fresh New Shipment Order form.</summary>
        private DataTable CreateBlankLinesTable()
        {
            var t = new DataTable();
            t.Columns.Add("LineID", typeof(int));
            t.Columns.Add("ProductID", typeof(int));
            t.Columns.Add("Quantity", typeof(int));
            t.Columns.Add("SellingForm", typeof(string));
            t.Rows.Add(0, 0, 0, "JAR");
            return t;
        }

        protected void btnSASaveDraft_Click(object s, EventArgs e)
        { SaveSAConsigShipment("Saved"); }

        protected void btnSACreateOrder_Click(object s, EventArgs e)
        { SaveSAConsigShipment("Order"); }

        void SaveSAConsigShipment(string status)
        {
            int csgId = 0;
            int.TryParse(hfSAConsigId.Value, out csgId);
            if (csgId <= 0) { ShowAlert("No consignment selected.", false); return; }
            if (!AssertSAConsigWritable(csgId)) return;

            int custId = ddlSACustomer != null ? Convert.ToInt32(ddlSACustomer.SelectedValue) : 0;
            if (custId <= 0) { ShowAlert("Please select a customer.", false); return; }

            string shipDate = txtSAShipDate != null ? txtSAShipDate.Text.Trim() : "";
            if (string.IsNullOrEmpty(shipDate)) { ShowAlert("Please enter a date.", false); return; }

            int channelId = ddlSAChannel != null ? Convert.ToInt32(ddlSAChannel.SelectedValue) : 0;
            if (channelId <= 0) { ShowAlert("Please select a channel.", false); return; }

            // Read lines from form — all three arrays are index-aligned
            string[] productIds = Request.Form.GetValues("sa_ship_product");
            string[] shipQtys   = Request.Form.GetValues("sa_ship_qty");
            string[] shipForms  = Request.Form.GetValues("sa_ship_form");
            if (productIds == null || shipQtys == null)
            { ShowAlert("Please add at least one product with a quantity.", false); return; }

            bool hasLine = false;
            for (int i = 0; i < productIds.Length && i < shipQtys.Length; i++)
            {
                int tpid = 0; int.TryParse(productIds[i], out tpid);
                int tqty = 0; int.TryParse(shipQtys[i], out tqty);
                if (tpid > 0 && tqty > 0) { hasLine = true; break; }
            }
            if (!hasLine) { ShowAlert("Please add at least one product with a quantity.", false); return; }

            int editShipId = hfEditShipId != null ? Convert.ToInt32(hfEditShipId.Value) : 0;
            int shipId;

            if (editShipId > 0)
            {
                // UPDATE existing shipment header
                DatabaseHelper.ExecuteNonQueryPublic(
                    "UPDATE SA_Shipments SET CustomerID=?cust, ShipmentDate=?dt, ChannelID=?ch, Status=?st WHERE ShipmentID=?sid;",
                    new MySqlParameter("?cust", custId),
                    new MySqlParameter("?dt", DateTime.Parse(shipDate)),
                    new MySqlParameter("?ch", channelId),
                    new MySqlParameter("?st", status),
                    new MySqlParameter("?sid", editShipId));
                shipId = editShipId;
                // Delete existing lines — re-insert below
                DatabaseHelper.ExecuteNonQueryPublic("DELETE FROM SA_ShipmentLines WHERE ShipmentID=?sid;",
                    new MySqlParameter("?sid", shipId));
            }
            else
            {
                // INSERT new shipment
                shipId = DatabaseHelper.CreateShipmentInConsignment(csgId, custId, DateTime.Parse(shipDate), 0, channelId, UserID);
                if (status == "Order")
                    DatabaseHelper.ExecuteNonQueryPublic("UPDATE SA_Shipments SET Status='Order' WHERE ShipmentID=?sid;",
                        new MySqlParameter("?sid", shipId));
            }

            // Insert lines (includes SellingForm — schema migration adds the column)
            for (int i = 0; i < productIds.Length; i++)
            {
                int pid = 0; int.TryParse(productIds[i], out pid);
                int sq = 0;
                if (i < shipQtys.Length) int.TryParse(shipQtys[i], out sq);
                string form = (shipForms != null && i < shipForms.Length && !string.IsNullOrEmpty(shipForms[i]))
                    ? shipForms[i] : "JAR";
                if (pid > 0 && sq > 0)
                {
                    DatabaseHelper.ExecuteNonQueryPublic(
                        "INSERT INTO SA_ShipmentLines (ShipmentID, ProductID, ProjectedQty, ShippedQty, SellingForm) VALUES (?sid,?pid,0,?sq,?frm);",
                        new MySqlParameter("?sid", shipId),
                        new MySqlParameter("?pid", pid),
                        new MySqlParameter("?sq", sq),
                        new MySqlParameter("?frm", form));
                }
            }

            string shipNo = "SH-" + shipId.ToString("D5");
            string msg;
            if (editShipId > 0)
                msg = status == "Order" ? "Order " + shipNo + " updated and sent to Shipment team." : "Draft " + shipNo + " updated.";
            else
                msg = status == "Order" ? "Shipment Order " + shipNo + " created — visible to Shipment team." : "Shipment " + shipNo + " saved as draft.";
            ShowAlert(msg, true);

            // Exit edit mode; LoadSAConsigDetail will reset the form
            if (hfEditShipId != null) hfEditShipId.Value = "0";
            LoadSAConsigDetail(csgId);
        }

        /// <summary>Fix 2: Handle Edit / Delete on a shipment order from inside the consignment view.</summary>
        protected void SAConsigOrder_Command(object sender, CommandEventArgs e)
        {
            int shipId = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "EditSAConsigShip")
            {
                DataRow ship = DatabaseHelper.ExecuteQueryRowPublic(
                    "SELECT ShipmentID, CustomerID, ShipmentDate, ChannelID, ConsignmentID, Status" +
                    " FROM SA_Shipments WHERE ShipmentID=?sid;",
                    new MySqlParameter("?sid", shipId));
                if (ship == null) { ShowAlert("Shipment not found.", false); return; }

                // Activate the right consignment
                int csgId = ship["ConsignmentID"] != DBNull.Value ? Convert.ToInt32(ship["ConsignmentID"]) : 0;
                if (csgId > 0) hfSAConsigId.Value = csgId.ToString();

                // Server-side guard: reject edit if consignment is locked
                if (csgId > 0 && !AssertSAConsigWritable(csgId)) return;

                // Set edit flag BEFORE LoadSAConsigDetail so it skips the form reset
                if (hfEditShipId != null) hfEditShipId.Value = shipId.ToString();

                BindSAConsigTabs();
                LoadSAConsigDetail(csgId);

                // Populate header fields
                if (ddlSACustomer != null && ship["CustomerID"] != DBNull.Value)
                {
                    var li = ddlSACustomer.Items.FindByValue(ship["CustomerID"].ToString());
                    if (li != null) ddlSACustomer.SelectedValue = ship["CustomerID"].ToString();
                }
                if (txtSAShipDate != null) txtSAShipDate.Text = Convert.ToDateTime(ship["ShipmentDate"]).ToString("yyyy-MM-dd");
                if (ddlSAChannel != null && ship["ChannelID"] != DBNull.Value)
                {
                    var li = ddlSAChannel.Items.FindByValue(ship["ChannelID"].ToString());
                    if (li != null) ddlSAChannel.SelectedValue = ship["ChannelID"].ToString();
                }

                // Load existing lines
                DataTable lines = DatabaseHelper.ExecuteQueryPublic(
                    "SELECT sl.LineID, sl.ProductID, sl.ShippedQty AS Quantity," +
                    " IFNULL(sl.SellingForm,'JAR') AS SellingForm" +
                    " FROM SA_ShipmentLines sl WHERE sl.ShipmentID=?sid ORDER BY sl.LineID;",
                    new MySqlParameter("?sid", shipId));
                BindSAShipLines(lines.Rows.Count > 0 ? lines : CreateBlankLinesTable());

                // Flip UI into edit mode
                if (lblSAEditShipId != null) lblSAEditShipId.Text = "— Editing SH-" + shipId.ToString("D5");
                if (lblSAShipFormTitle != null) lblSAShipFormTitle.Text = "Edit Shipment Order";
                if (btnSACancelEdit != null) btnSACancelEdit.Visible = true;
                if (btnSACreateOrder != null) btnSACreateOrder.Text = "Save & Send Order";
            }
            else if (e.CommandName == "DeleteSAConsigShip")
            {
                DataRow ship = DatabaseHelper.ExecuteQueryRowPublic(
                    "SELECT Status, ConsignmentID FROM SA_Shipments WHERE ShipmentID=?sid;",
                    new MySqlParameter("?sid", shipId));
                if (ship == null) return;
                int csgId = ship["ConsignmentID"] != DBNull.Value ? Convert.ToInt32(ship["ConsignmentID"]) : 0;

                // Server-side guard: reject delete if consignment is locked
                if (csgId > 0 && !AssertSAConsigWritable(csgId)) return;

                string st = ship["Status"].ToString();
                if (st == "DC" || st == "Shipped")
                { ShowAlert("Cannot delete — order already " + st + ".", false); return; }

                DatabaseHelper.ExecuteNonQueryPublic("DELETE FROM SA_ShipmentLines WHERE ShipmentID=?sid;",
                    new MySqlParameter("?sid", shipId));
                DatabaseHelper.ExecuteNonQueryPublic("DELETE FROM SA_Shipments WHERE ShipmentID=?sid;",
                    new MySqlParameter("?sid", shipId));
                ShowAlert("Shipment order deleted.", true);

                if (csgId > 0) LoadSAConsigDetail(csgId);
            }
        }

        /// <summary>Fix 2: exit edit mode without saving.</summary>
        protected void btnSACancelEdit_Click(object s, EventArgs e)
        {
            if (hfEditShipId != null) hfEditShipId.Value = "0";
            int csgId = 0;
            int.TryParse(hfSAConsigId.Value, out csgId);
            if (csgId > 0) LoadSAConsigDetail(csgId);
        }

        protected void btnSASendToPK_Click(object s, EventArgs e)
        {
            int csgId = 0;
            int.TryParse(hfSAConsigId.Value, out csgId);
            if (csgId <= 0) return;
            if (!AssertSAConsigWritable(csgId)) return;

            // Change all "Saved" shipments to "Order" status
            DatabaseHelper.ExecuteNonQueryPublic(
                "UPDATE SA_Shipments SET Status='Order' WHERE ConsignmentID=?cid AND Status='Saved';",
                new MySqlParameter("?cid", csgId));

            ShowAlert("All draft orders sent to Shipment team.", true);
            LoadSAConsigDetail(csgId);
        }

        protected string GetStatusBadge(string status)
        {
            switch (status)
            {
                case "Order": return "<span style='background:#d4edda;color:#155724;font-size:10px;font-weight:700;padding:2px 8px;border-radius:10px;'>Order</span>";
                case "Saved": return "<span style='background:#fff3cd;color:#856404;font-size:10px;font-weight:700;padding:2px 8px;border-radius:10px;'>Draft</span>";
                case "DC": return "<span style='background:#cce5ff;color:#004085;font-size:10px;font-weight:700;padding:2px 8px;border-radius:10px;'>DC Created</span>";
                default: return "<span style='font-size:10px;'>" + status + "</span>";
            }
        }
    }
}
