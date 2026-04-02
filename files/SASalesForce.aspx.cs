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
        protected DropDownList ddlMonth, ddlYear;
        protected DropDownList ddlProjArea, ddlProjChannel;
        protected DropDownList ddlShipArea, ddlShipChannel, ddlTransport, ddlCustomer;
        protected TextBox txtShipDate, txtVehicleNo;
        protected Button btnTabProjection, btnTabShipments, btnLoadProjection, btnSaveProjection, btnCreateShipment, btnSaveShipment, btnNewProjection, btnNewShipment;
        protected Repeater rptProjLines, rptProjections, rptShipLines, rptShipments;
        protected HiddenField hfTab, hfProductOptionsHtml, hfUOMOptionsHtml, hfEditProjId;
        protected HiddenField hfShipZoneID, hfShipRegionID, hfProjZoneID, hfProjRegionID, hfEditShipId;

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
            bool isProj = hfTab.Value != "shipments";
            pnlProjection.Visible = isProj; pnlShipments.Visible = !isProj;
            btnTabProjection.CssClass = isProj ? "tab-btn active" : "tab-btn";
            btnTabShipments.CssClass = !isProj ? "tab-btn active" : "tab-btn";
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
            SetShipProductOptions();
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

        private void SetShipProductOptions()
        {
            foreach (RepeaterItem item in rptShipLines.Items)
            {
                var lit = item.FindControl("litShipProductOptions") as Literal;
                if (lit != null) lit.Text = hfProductOptionsHtml.Value;
                var litU = item.FindControl("litShipUOMOptions") as Literal;
                if (litU != null) litU.Text = hfUOMOptionsHtml.Value;
            }
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
                    "SELECT sl.ProductID, p.ProductName, sl.ProjectedQty AS Quantity" +
                    " FROM SA_ShipmentLines sl JOIN PP_Products p ON p.ProductID=sl.ProductID" +
                    " WHERE sl.ShipmentID=?sid ORDER BY p.ProductName;",
                    new MySqlParameter("?sid", shipId));
                rptShipLines.DataSource = lines;
                rptShipLines.DataBind();
                SetShipProductOptions();
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
                case "Shipped": return "badge-shipped";
                default: return "badge-draft";
            }
        }

        private void ShowAlert(string msg, bool success)
        { pnlAlert.Visible = true; lblAlert.Text = msg; pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger"); }
    }
}
