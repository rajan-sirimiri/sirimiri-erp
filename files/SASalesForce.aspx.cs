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
        protected Label lblPathZone, lblPathRegion, lblPathArea, lblPathChannel;
        protected Panel pnlAlert, pnlProjection, pnlShipments, pnlProjLines, pnlProjEmpty;
        protected Panel pnlShipLines, pnlNoProjection, pnlShipEmpty;
        protected DropDownList ddlMonth, ddlYear;
        protected DropDownList ddlProjZone, ddlProjRegion, ddlProjArea, ddlProjChannel;
        protected DropDownList ddlShipZone, ddlShipRegion, ddlShipArea, ddlShipChannel, ddlTransport, ddlCustomer;
        protected TextBox txtShipDate, txtVehicleNo;
        protected Button btnTabProjection, btnTabShipments, btnLoadProjection, btnSaveProjection, btnCreateShipment;
        protected Repeater rptProjLines, rptProjections, rptShipLines, rptShipments;
        protected HiddenField hfTab, hfProductOptionsHtml, hfUOMOptionsHtml, hfEditProjId;

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

            // Customers — Distributor and Stockist only
            if (ddlCustomer != null)
            {
                DataTable cust = DatabaseHelper.ExecuteQueryPublic(
                    "SELECT c.CustomerID, c.CustomerName, c.CustomerCode," +
                    " IFNULL(ct.TypeName,'') AS TypeName" +
                    " FROM PK_Customers c" +
                    " LEFT JOIN PK_CustomerTypes ct ON ct.TypeCode=c.CustomerType" +
                    " WHERE c.IsActive=1 AND UPPER(c.CustomerType) IN ('DISTRIBUTOR','STOCKIST')" +
                    " ORDER BY c.CustomerName;");
                ddlCustomer.Items.Clear();
                ddlCustomer.Items.Add(new ListItem("-- Select Customer --", "0"));
                foreach (DataRow r in cust.Rows)
                    ddlCustomer.Items.Add(new ListItem(
                        r["CustomerName"].ToString() + " (" + r["TypeName"] + ")",
                        r["CustomerID"].ToString()));
            }
        }

        private void LoadZoneDropdowns()
        {
            DataTable zones = DatabaseHelper.ExecuteQueryPublic(
                "SELECT ZoneID, ZoneName FROM SA_Zones WHERE IsActive=1 ORDER BY SortOrder, ZoneName;");
            BindDDL(ddlProjZone, zones, "ZoneName", "ZoneID", "-- Select Zone --");
            BindDDL(ddlShipZone, zones, "ZoneName", "ZoneID", "-- Select Zone --");
            // Clear dependent
            ClearDDL(ddlProjRegion, "-- Select Region --");
            ClearDDL(ddlProjArea, "-- Select Area --");
            ClearDDL(ddlShipRegion, "-- Select Region --");
            ClearDDL(ddlShipArea, "-- Select Area --");
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
                ub.Append("<option value='" + r["UOMID"] + "'>" + r["Abbreviation"] + "</option>");
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

        // ── PROJECTION: CASCADING DROPDOWNS ───────────────────────────────

        protected void ddlProjZone_Changed(object sender, EventArgs e)
        {
            LoadRegions(ddlProjRegion, Convert.ToInt32(ddlProjZone.SelectedValue));
            ClearDDL(ddlProjArea, "-- Select Area --");
        }

        protected void ddlProjRegion_Changed(object sender, EventArgs e)
        { LoadAreas(ddlProjArea, Convert.ToInt32(ddlProjRegion.SelectedValue)); }

        // ── SHIPMENT: CASCADING DROPDOWNS ─────────────────────────────────

        protected void ddlShipZone_Changed(object sender, EventArgs e)
        {
            LoadRegions(ddlShipRegion, Convert.ToInt32(ddlShipZone.SelectedValue));
            ClearDDL(ddlShipArea, "-- Select Area --");
            pnlShipLines.Visible = false; pnlNoProjection.Visible = false;
        }

        protected void ddlShipRegion_Changed(object sender, EventArgs e)
        {
            LoadAreas(ddlShipArea, Convert.ToInt32(ddlShipRegion.SelectedValue));
            pnlShipLines.Visible = false; pnlNoProjection.Visible = false;
        }

        protected void ddlShipArea_Changed(object sender, EventArgs e) { LoadShipmentProjection(); }
        protected void ddlShipChannel_Changed(object sender, EventArgs e) { LoadShipmentProjection(); }

        // ── PROJECTION CRUD ───────────────────────────────────────────────

        protected void btnLoadProjection_Click(object sender, EventArgs e)
        {
            int zoneId = Convert.ToInt32(ddlProjZone.SelectedValue);
            int regionId = Convert.ToInt32(ddlProjRegion.SelectedValue);
            int areaId = Convert.ToInt32(ddlProjArea.SelectedValue);
            int channelId = Convert.ToInt32(ddlProjChannel.SelectedValue);
            if (zoneId == 0 || regionId == 0 || areaId == 0 || channelId == 0)
            { ShowAlert("Please select Zone, Region, Area, and Channel.", false); return; }

            lblPathZone.Text = ddlProjZone.SelectedItem.Text;
            lblPathRegion.Text = ddlProjRegion.SelectedItem.Text;
            lblPathArea.Text = ddlProjArea.SelectedItem.Text;
            lblPathChannel.Text = ddlProjChannel.SelectedItem.Text;

            DataRow proj = DatabaseHelper.ExecuteQueryRowPublic(
                "SELECT ProjectionID FROM SA_Projections WHERE ProjectionMonth=?m AND ProjectionYear=?y AND ChannelID=?c AND PositionID=?p;",
                new MySqlParameter("?m", SelMonth), new MySqlParameter("?y", SelYear),
                new MySqlParameter("?c", channelId), new MySqlParameter("?p", areaId));

            if (proj != null)
            {
                int projId = Convert.ToInt32(proj["ProjectionID"]);
                hfEditProjId.Value = projId.ToString();
                LoadProjLines(projId);
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

        protected void btnSaveProjection_Click(object sender, EventArgs e)
        {
            int zoneId = Convert.ToInt32(ddlProjZone.SelectedValue);
            int regionId = Convert.ToInt32(ddlProjRegion.SelectedValue);
            int areaId = Convert.ToInt32(ddlProjArea.SelectedValue);
            int channelId = Convert.ToInt32(ddlProjChannel.SelectedValue);
            if (zoneId == 0 || regionId == 0 || areaId == 0 || channelId == 0)
            { ShowAlert("Select Zone, Region, Area, and Channel.", false); return; }

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

                // Set cascading dropdowns
                if (proj["ZoneID"] != DBNull.Value)
                {
                    ddlProjZone.SelectedValue = proj["ZoneID"].ToString();
                    LoadRegions(ddlProjRegion, Convert.ToInt32(proj["ZoneID"]));
                }
                if (proj["RegionID"] != DBNull.Value)
                {
                    var li = ddlProjRegion.Items.FindByValue(proj["RegionID"].ToString());
                    if (li != null) ddlProjRegion.SelectedValue = proj["RegionID"].ToString();
                    LoadAreas(ddlProjArea, Convert.ToInt32(proj["RegionID"]));
                }
                if (proj["PositionID"] != DBNull.Value)
                {
                    var li = ddlProjArea.Items.FindByValue(proj["PositionID"].ToString());
                    if (li != null) ddlProjArea.SelectedValue = proj["PositionID"].ToString();
                }
                if (proj["ChannelID"] != DBNull.Value)
                    ddlProjChannel.SelectedValue = proj["ChannelID"].ToString();

                lblPathZone.Text = proj["ZoneName"]?.ToString() ?? "";
                lblPathRegion.Text = proj["RegionName"]?.ToString() ?? "";
                lblPathArea.Text = proj["AreaName"]?.ToString() ?? "";
                lblPathChannel.Text = proj["ChannelName"]?.ToString() ?? "";

                hfEditProjId.Value = projId.ToString();
                LoadProjLines(projId);
                pnlProjLines.Visible = true;
                BuildOptionHtml();
            }
            else if (e.CommandName == "ConfirmProj")
            {
                DatabaseHelper.ExecuteNonQueryPublic(
                    "UPDATE SA_Projections SET Status='Confirmed' WHERE ProjectionID=?pid;",
                    new MySqlParameter("?pid", projId));
                ShowAlert("Projection confirmed.", true);
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

        private void LoadShipmentProjection()
        {
            int areaId = Convert.ToInt32(ddlShipArea.SelectedValue);
            int channelId = Convert.ToInt32(ddlShipChannel.SelectedValue);
            if (areaId == 0 || channelId == 0) { pnlShipLines.Visible = false; pnlNoProjection.Visible = false; return; }

            DataRow proj = DatabaseHelper.ExecuteQueryRowPublic(
                "SELECT ProjectionID FROM SA_Projections WHERE ProjectionMonth=?m AND ProjectionYear=?y AND ChannelID=?c AND PositionID=?p;",
                new MySqlParameter("?m", SelMonth), new MySqlParameter("?y", SelYear),
                new MySqlParameter("?c", channelId), new MySqlParameter("?p", areaId));

            if (proj == null)
            { pnlShipLines.Visible = false; pnlNoProjection.Visible = true; return; }

            int projId = Convert.ToInt32(proj["ProjectionID"]);
            DataTable lines = DatabaseHelper.ExecuteQueryPublic(
                "SELECT pl.ProductID, p.ProductName, pl.Quantity" +
                " FROM SA_ProjectionLines pl JOIN PP_Products p ON p.ProductID=pl.ProductID" +
                " WHERE pl.ProjectionID=?pid ORDER BY p.ProductName;",
                new MySqlParameter("?pid", projId));
            rptShipLines.DataSource = lines;
            rptShipLines.DataBind();
            pnlShipLines.Visible = lines.Rows.Count > 0;
            pnlNoProjection.Visible = lines.Rows.Count == 0;
        }

        protected void btnCreateShipment_Click(object sender, EventArgs e)
        {
            int zoneId = Convert.ToInt32(ddlShipZone.SelectedValue);
            int regionId = Convert.ToInt32(ddlShipRegion.SelectedValue);
            int areaId = Convert.ToInt32(ddlShipArea.SelectedValue);
            int channelId = Convert.ToInt32(ddlShipChannel.SelectedValue);
            string shipDate = txtShipDate.Text.Trim();
            if (channelId == 0 || string.IsNullOrEmpty(shipDate))
            { ShowAlert("Please fill Date and Channel.", false); return; }

            int transportId = Convert.ToInt32(ddlTransport.SelectedValue);

            // Get projection ID
            DataRow proj = DatabaseHelper.ExecuteQueryRowPublic(
                "SELECT ProjectionID FROM SA_Projections WHERE ProjectionMonth=?m AND ProjectionYear=?y AND ChannelID=?c AND PositionID=?p;",
                new MySqlParameter("?m", SelMonth), new MySqlParameter("?y", SelYear),
                new MySqlParameter("?c", channelId), new MySqlParameter("?p", areaId));
            int projId = proj != null ? Convert.ToInt32(proj["ProjectionID"]) : 0;

            int custId = ddlCustomer != null ? Convert.ToInt32(ddlCustomer.SelectedValue) : 0;

            DatabaseHelper.ExecuteNonQueryPublic(
                "INSERT INTO SA_Shipments (ProjectionID, CustomerID, ShipmentDate, StateID, ChannelID, ZoneID, RegionID, PositionID, TransportModeID, VehicleNo, Status, CreatedBy)" +
                " VALUES (?pid,?cust,?dt,0,?c,?z,?r,?p,?tm,?vn,'Draft',?u);",
                new MySqlParameter("?pid", projId > 0 ? (object)projId : DBNull.Value),
                new MySqlParameter("?cust", custId > 0 ? (object)custId : DBNull.Value),
                new MySqlParameter("?dt", DateTime.Parse(shipDate)),
                new MySqlParameter("?c", channelId),
                new MySqlParameter("?z", zoneId > 0 ? (object)zoneId : DBNull.Value),
                new MySqlParameter("?r", regionId > 0 ? (object)regionId : DBNull.Value),
                new MySqlParameter("?p", areaId > 0 ? (object)areaId : DBNull.Value),
                new MySqlParameter("?tm", transportId > 0 ? (object)transportId : DBNull.Value),
                new MySqlParameter("?vn", txtVehicleNo.Text.Trim()),
                new MySqlParameter("?u", UserID));

            object shipIdObj = DatabaseHelper.ExecuteScalarPublic("SELECT LAST_INSERT_ID();");
            int shipId = Convert.ToInt32(shipIdObj);

            string[] productIds = Request.Form.GetValues("ship_productid");
            string[] shipQtys = Request.Form.GetValues("ship_qty");
            string[] projQtys = Request.Form.GetValues("ship_projqty");
            if (productIds != null && shipQtys != null)
            {
                for (int i = 0; i < productIds.Length; i++)
                {
                    int pid = 0; int.TryParse(productIds[i], out pid);
                    int sq = 0; int.TryParse(shipQtys[i], out sq);
                    int pq = 0; if (projQtys != null && i < projQtys.Length) int.TryParse(projQtys[i], out pq);
                    if (pid > 0 && sq > 0)
                    {
                        DatabaseHelper.ExecuteNonQueryPublic(
                            "INSERT INTO SA_ShipmentLines (ShipmentID, ProductID, ProjectedQty, ShippedQty) VALUES (?sid,?pid,?pq,?sq);",
                            new MySqlParameter("?sid", shipId), new MySqlParameter("?pid", pid),
                            new MySqlParameter("?pq", pq), new MySqlParameter("?sq", sq));
                    }
                }
            }

            ShowAlert("Shipment created.", true);
            pnlShipLines.Visible = false;
            txtShipDate.Text = ""; txtVehicleNo.Text = "";
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

        private void ShowAlert(string msg, bool success)
        { pnlAlert.Visible = true; lblAlert.Text = msg; pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger"); }
    }
}
