using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using StockApp.DAL;

namespace StockApp
{
    public partial class SASalesForce : Page
    {
        protected Label lblUserName, lblUserRole, lblAlert, lblMonthYear, lblProjContext, lblProjMonth, lblShipMonth;
        protected Panel pnlAlert, pnlProjection, pnlShipments, pnlProjLines, pnlProjEmpty;
        protected Panel pnlShipLines, pnlNoProjection, pnlShipEmpty;
        protected DropDownList ddlMonth, ddlYear, ddlProjState, ddlProjChannel;
        protected DropDownList ddlShipState, ddlShipChannel, ddlTransport;
        protected TextBox txtShipDate, txtVehicleNo, txtShipRemarks;
        protected Button btnTabProjection, btnTabShipments, btnLoadProjection, btnSaveProjection, btnCreateShipment;
        protected Repeater rptProjLines, rptProjections, rptShipLines, rptShipments;
        protected HiddenField hfTab, hfProductOptionsHtml, hfEditProjId;

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
                LoadDropdowns();
                BuildProductOptionsHtml();
                SetActiveTab();
                RefreshData();
            }
        }

        private void LoadYears()
        {
            ddlYear.Items.Clear();
            int current = DateTime.Now.Year;
            for (int y = current - 1; y <= current + 2; y++)
                ddlYear.Items.Add(new ListItem(y.ToString(), y.ToString()));
        }

        private void LoadDropdowns()
        {
            // States
            DataTable states = DatabaseHelper.GetStatesForAdmin();
            ddlProjState.Items.Clear(); ddlShipState.Items.Clear();
            ddlProjState.Items.Add(new ListItem("-- Select State --", "0"));
            ddlShipState.Items.Add(new ListItem("-- Select State --", "0"));
            foreach (DataRow r in states.Rows)
            {
                ddlProjState.Items.Add(new ListItem(r["StateName"].ToString(), r["StateID"].ToString()));
                ddlShipState.Items.Add(new ListItem(r["StateName"].ToString(), r["StateID"].ToString()));
            }

            // Channels
            DataTable channels = DatabaseHelper.ExecuteQueryPublic(
                "SELECT ChannelID, ChannelName FROM SA_Channels WHERE IsActive=1 ORDER BY SortOrder;");
            ddlProjChannel.Items.Clear(); ddlShipChannel.Items.Clear();
            ddlProjChannel.Items.Add(new ListItem("-- Select Channel --", "0"));
            ddlShipChannel.Items.Add(new ListItem("-- Select Channel --", "0"));
            foreach (DataRow r in channels.Rows)
            {
                ddlProjChannel.Items.Add(new ListItem(r["ChannelName"].ToString(), r["ChannelID"].ToString()));
                ddlShipChannel.Items.Add(new ListItem(r["ChannelName"].ToString(), r["ChannelID"].ToString()));
            }

            // Transport modes
            DataTable modes = DatabaseHelper.ExecuteQueryPublic(
                "SELECT ModeID, ModeName FROM SA_TransportModes WHERE IsActive=1 ORDER BY SortOrder;");
            ddlTransport.Items.Clear();
            ddlTransport.Items.Add(new ListItem("-- Select --", "0"));
            foreach (DataRow r in modes.Rows)
                ddlTransport.Items.Add(new ListItem(r["ModeName"].ToString(), r["ModeID"].ToString()));
        }

        private void BuildProductOptionsHtml()
        {
            DataTable products = DatabaseHelper.ExecuteQueryPublic(
                "SELECT ProductID, ProductName, ProductCode FROM PP_Products WHERE IsActive=1 AND ProductType IN ('Core','Conversion') ORDER BY ProductName;");
            var sb = new System.Text.StringBuilder();
            foreach (DataRow r in products.Rows)
                sb.Append("<option value='" + r["ProductID"] + "'>" + r["ProductName"] + " (" + r["ProductCode"] + ")</option>");
            hfProductOptionsHtml.Value = sb.ToString();
        }

        // ── TAB SWITCHING ─────────────────────────────────────────────────

        protected void btnTab_Click(object sender, EventArgs e)
        {
            hfTab.Value = ((Button)sender).CommandArgument;
            SetActiveTab();
            RefreshData();
        }

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
            string monthName = ddlMonth.SelectedItem.Text + " " + SelYear;
            lblMonthYear.Text = monthName;
            lblProjMonth.Text = monthName;
            lblShipMonth.Text = monthName;
            LoadProjectionsList();
            LoadShipmentsList();
        }

        // ── PROJECTION ────────────────────────────────────────────────────

        protected void btnLoadProjection_Click(object sender, EventArgs e)
        {
            int stateId = Convert.ToInt32(ddlProjState.SelectedValue);
            int channelId = Convert.ToInt32(ddlProjChannel.SelectedValue);
            if (stateId == 0 || channelId == 0)
            { ShowAlert("Please select State and Channel.", false); return; }

            lblProjContext.Text = ddlProjState.SelectedItem.Text + " / " + ddlProjChannel.SelectedItem.Text;

            // Check if projection exists
            DataRow proj = DatabaseHelper.ExecuteQueryRowPublic(
                "SELECT ProjectionID FROM SA_Projections WHERE ProjectionMonth=?m AND ProjectionYear=?y AND StateID=?s AND ChannelID=?c;",
                new MySql.Data.MySqlClient.MySqlParameter("?m", SelMonth),
                new MySql.Data.MySqlClient.MySqlParameter("?y", SelYear),
                new MySql.Data.MySqlClient.MySqlParameter("?s", stateId),
                new MySql.Data.MySqlClient.MySqlParameter("?c", channelId));

            if (proj != null)
            {
                int projId = Convert.ToInt32(proj["ProjectionID"]);
                hfEditProjId.Value = projId.ToString();
                LoadProjLines(projId);
            }
            else
            {
                hfEditProjId.Value = "0";
                // Show empty line
                var dt = new DataTable();
                dt.Columns.Add("LineID", typeof(int));
                dt.Columns.Add("ProductID", typeof(int));
                dt.Columns.Add("Quantity", typeof(int));
                dt.Rows.Add(0, 0, 0);
                rptProjLines.DataSource = dt;
                rptProjLines.DataBind();
                SetProductOptions();
            }
            pnlProjLines.Visible = true;
            BuildProductOptionsHtml();
            RefreshData();
        }

        private void LoadProjLines(int projId)
        {
            DataTable lines = DatabaseHelper.ExecuteQueryPublic(
                "SELECT pl.LineID, pl.ProductID, pl.Quantity, p.ProductName, p.ProductCode" +
                " FROM SA_ProjectionLines pl JOIN PP_Products p ON p.ProductID=pl.ProductID" +
                " WHERE pl.ProjectionID=?pid ORDER BY p.ProductName;",
                new MySql.Data.MySqlClient.MySqlParameter("?pid", projId));

            if (lines.Rows.Count == 0)
            {
                lines.Rows.Add(0, 0, 0, "", "");
            }
            rptProjLines.DataSource = lines;
            rptProjLines.DataBind();
            SetProductOptions();
        }

        private void SetProductOptions()
        {
            foreach (RepeaterItem item in rptProjLines.Items)
            {
                var lit = item.FindControl("litProductOptions") as Literal;
                if (lit != null) lit.Text = hfProductOptionsHtml.Value;
            }
        }

        protected void btnSaveProjection_Click(object sender, EventArgs e)
        {
            int stateId = Convert.ToInt32(ddlProjState.SelectedValue);
            int channelId = Convert.ToInt32(ddlProjChannel.SelectedValue);
            if (stateId == 0 || channelId == 0) { ShowAlert("Select State and Channel.", false); return; }

            int projId = Convert.ToInt32(hfEditProjId.Value);

            // Create projection if new
            if (projId == 0)
            {
                DatabaseHelper.ExecuteNonQueryPublic(
                    "INSERT INTO SA_Projections (ProjectionMonth, ProjectionYear, StateID, ChannelID, CreatedBy)" +
                    " VALUES (?m,?y,?s,?c,?u)" +
                    " ON DUPLICATE KEY UPDATE CreatedBy=?u2;",
                    new MySql.Data.MySqlClient.MySqlParameter("?m", SelMonth),
                    new MySql.Data.MySqlClient.MySqlParameter("?y", SelYear),
                    new MySql.Data.MySqlClient.MySqlParameter("?s", stateId),
                    new MySql.Data.MySqlClient.MySqlParameter("?c", channelId),
                    new MySql.Data.MySqlClient.MySqlParameter("?u", UserID),
                    new MySql.Data.MySqlClient.MySqlParameter("?u2", UserID));

                DataRow newProj = DatabaseHelper.ExecuteQueryRowPublic(
                    "SELECT ProjectionID FROM SA_Projections WHERE ProjectionMonth=?m AND ProjectionYear=?y AND StateID=?s AND ChannelID=?c;",
                    new MySql.Data.MySqlClient.MySqlParameter("?m", SelMonth),
                    new MySql.Data.MySqlClient.MySqlParameter("?y", SelYear),
                    new MySql.Data.MySqlClient.MySqlParameter("?s", stateId),
                    new MySql.Data.MySqlClient.MySqlParameter("?c", channelId));
                if (newProj != null) projId = Convert.ToInt32(newProj["ProjectionID"]);
            }

            if (projId == 0) { ShowAlert("Error creating projection.", false); return; }

            // Clear existing lines and re-insert
            DatabaseHelper.ExecuteNonQueryPublic("DELETE FROM SA_ProjectionLines WHERE ProjectionID=?pid;",
                new MySql.Data.MySqlClient.MySqlParameter("?pid", projId));

            string[] products = Request.Form.GetValues("proj_product");
            string[] qtys = Request.Form.GetValues("proj_qty");
            if (products != null && qtys != null)
            {
                for (int i = 0; i < products.Length; i++)
                {
                    int productId = 0; int.TryParse(products[i], out productId);
                    int qty = 0; int.TryParse(qtys[i], out qty);
                    if (productId > 0 && qty > 0)
                    {
                        DatabaseHelper.ExecuteNonQueryPublic(
                            "INSERT INTO SA_ProjectionLines (ProjectionID, ProductID, Quantity) VALUES (?pid,?prod,?qty)" +
                            " ON DUPLICATE KEY UPDATE Quantity=?qty2;",
                            new MySql.Data.MySqlClient.MySqlParameter("?pid", projId),
                            new MySql.Data.MySqlClient.MySqlParameter("?prod", productId),
                            new MySql.Data.MySqlClient.MySqlParameter("?qty", qty),
                            new MySql.Data.MySqlClient.MySqlParameter("?qty2", qty));
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
                    "SELECT * FROM SA_Projections WHERE ProjectionID=?pid;",
                    new MySql.Data.MySqlClient.MySqlParameter("?pid", projId));
                if (proj == null) return;
                ddlProjState.SelectedValue = proj["StateID"].ToString();
                ddlProjChannel.SelectedValue = proj["ChannelID"].ToString();
                hfEditProjId.Value = projId.ToString();
                lblProjContext.Text = ddlProjState.SelectedItem.Text + " / " + ddlProjChannel.SelectedItem.Text;
                LoadProjLines(projId);
                pnlProjLines.Visible = true;
                BuildProductOptionsHtml();
            }
            else if (e.CommandName == "ConfirmProj")
            {
                DatabaseHelper.ExecuteNonQueryPublic(
                    "UPDATE SA_Projections SET Status='Confirmed' WHERE ProjectionID=?pid;",
                    new MySql.Data.MySqlClient.MySqlParameter("?pid", projId));
                ShowAlert("Projection confirmed.", true);
            }
            RefreshData();
        }

        private void LoadProjectionsList()
        {
            DataTable dt = DatabaseHelper.ExecuteQueryPublic(
                "SELECT p.ProjectionID, s.StateName, c.ChannelName, p.Status," +
                " COUNT(pl.LineID) AS ProductCount, IFNULL(SUM(pl.Quantity),0) AS TotalQty" +
                " FROM SA_Projections p" +
                " JOIN States s ON s.StateID=p.StateID" +
                " JOIN SA_Channels c ON c.ChannelID=p.ChannelID" +
                " LEFT JOIN SA_ProjectionLines pl ON pl.ProjectionID=p.ProjectionID" +
                " WHERE p.ProjectionMonth=?m AND p.ProjectionYear=?y" +
                " GROUP BY p.ProjectionID, s.StateName, c.ChannelName, p.Status" +
                " ORDER BY s.StateName, c.ChannelName;",
                new MySql.Data.MySqlClient.MySqlParameter("?m", SelMonth),
                new MySql.Data.MySqlClient.MySqlParameter("?y", SelYear));
            rptProjections.DataSource = dt;
            rptProjections.DataBind();
            pnlProjEmpty.Visible = dt.Rows.Count == 0;
        }

        // ── SHIPMENTS ─────────────────────────────────────────────────────

        protected void ddlShipState_Changed(object sender, EventArgs e) { LoadShipmentProjection(); }
        protected void ddlShipChannel_Changed(object sender, EventArgs e) { LoadShipmentProjection(); }

        private void LoadShipmentProjection()
        {
            int stateId = Convert.ToInt32(ddlShipState.SelectedValue);
            int channelId = Convert.ToInt32(ddlShipChannel.SelectedValue);
            if (stateId == 0 || channelId == 0) { pnlShipLines.Visible = false; pnlNoProjection.Visible = false; return; }

            DataRow proj = DatabaseHelper.ExecuteQueryRowPublic(
                "SELECT ProjectionID FROM SA_Projections WHERE ProjectionMonth=?m AND ProjectionYear=?y AND StateID=?s AND ChannelID=?c;",
                new MySql.Data.MySqlClient.MySqlParameter("?m", SelMonth),
                new MySql.Data.MySqlClient.MySqlParameter("?y", SelYear),
                new MySql.Data.MySqlClient.MySqlParameter("?s", stateId),
                new MySql.Data.MySqlClient.MySqlParameter("?c", channelId));

            if (proj == null)
            { pnlShipLines.Visible = false; pnlNoProjection.Visible = true; return; }

            int projId = Convert.ToInt32(proj["ProjectionID"]);
            DataTable lines = DatabaseHelper.ExecuteQueryPublic(
                "SELECT pl.ProductID, p.ProductName, p.ProductCode, pl.Quantity" +
                " FROM SA_ProjectionLines pl JOIN PP_Products p ON p.ProductID=pl.ProductID" +
                " WHERE pl.ProjectionID=?pid ORDER BY p.ProductName;",
                new MySql.Data.MySqlClient.MySqlParameter("?pid", projId));

            rptShipLines.DataSource = lines;
            rptShipLines.DataBind();
            pnlShipLines.Visible = lines.Rows.Count > 0;
            pnlNoProjection.Visible = lines.Rows.Count == 0;
        }

        protected void btnCreateShipment_Click(object sender, EventArgs e)
        {
            int stateId = Convert.ToInt32(ddlShipState.SelectedValue);
            int channelId = Convert.ToInt32(ddlShipChannel.SelectedValue);
            string shipDate = txtShipDate.Text.Trim();
            if (stateId == 0 || channelId == 0 || string.IsNullOrEmpty(shipDate))
            { ShowAlert("Please fill Date, State, and Channel.", false); return; }

            int transportId = Convert.ToInt32(ddlTransport.SelectedValue);

            // Get projection ID
            DataRow proj = DatabaseHelper.ExecuteQueryRowPublic(
                "SELECT ProjectionID FROM SA_Projections WHERE ProjectionMonth=?m AND ProjectionYear=?y AND StateID=?s AND ChannelID=?c;",
                new MySql.Data.MySqlClient.MySqlParameter("?m", SelMonth),
                new MySql.Data.MySqlClient.MySqlParameter("?y", SelYear),
                new MySql.Data.MySqlClient.MySqlParameter("?s", stateId),
                new MySql.Data.MySqlClient.MySqlParameter("?c", channelId));
            int projId = proj != null ? Convert.ToInt32(proj["ProjectionID"]) : 0;

            // Create shipment
            DatabaseHelper.ExecuteNonQueryPublic(
                "INSERT INTO SA_Shipments (ProjectionID, ShipmentDate, StateID, ChannelID, TransportModeID, VehicleNo, Status, Remarks, CreatedBy)" +
                " VALUES (?pid,?dt,?s,?c,?tm,?vn,'Draft',?rem,?u);",
                new MySql.Data.MySqlClient.MySqlParameter("?pid", projId > 0 ? (object)projId : DBNull.Value),
                new MySql.Data.MySqlClient.MySqlParameter("?dt", DateTime.Parse(shipDate)),
                new MySql.Data.MySqlClient.MySqlParameter("?s", stateId),
                new MySql.Data.MySqlClient.MySqlParameter("?c", channelId),
                new MySql.Data.MySqlClient.MySqlParameter("?tm", transportId > 0 ? (object)transportId : DBNull.Value),
                new MySql.Data.MySqlClient.MySqlParameter("?vn", txtVehicleNo.Text.Trim()),
                new MySql.Data.MySqlClient.MySqlParameter("?rem", txtShipRemarks.Text.Trim()),
                new MySql.Data.MySqlClient.MySqlParameter("?u", UserID));

            object shipIdObj = DatabaseHelper.ExecuteScalarPublic("SELECT LAST_INSERT_ID();");
            int shipId = Convert.ToInt32(shipIdObj);

            // Save line items
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
                            new MySql.Data.MySqlClient.MySqlParameter("?sid", shipId),
                            new MySql.Data.MySqlClient.MySqlParameter("?pid", pid),
                            new MySql.Data.MySqlClient.MySqlParameter("?pq", pq),
                            new MySql.Data.MySqlClient.MySqlParameter("?sq", sq));
                    }
                }
            }

            ShowAlert("Shipment created.", true);
            pnlShipLines.Visible = false;
            txtShipDate.Text = ""; txtVehicleNo.Text = ""; txtShipRemarks.Text = "";
            RefreshData();
        }

        private void LoadShipmentsList()
        {
            DataTable dt = DatabaseHelper.ExecuteQueryPublic(
                "SELECT sh.ShipmentID, sh.ShipmentDate, s.StateName, c.ChannelName," +
                " IFNULL(tm.ModeName,'—') AS TransportMode, sh.Status," +
                " COUNT(sl.LineID) AS ProductCount" +
                " FROM SA_Shipments sh" +
                " JOIN States s ON s.StateID=sh.StateID" +
                " JOIN SA_Channels c ON c.ChannelID=sh.ChannelID" +
                " LEFT JOIN SA_TransportModes tm ON tm.ModeID=sh.TransportModeID" +
                " LEFT JOIN SA_ShipmentLines sl ON sl.ShipmentID=sh.ShipmentID" +
                " WHERE MONTH(sh.ShipmentDate)=?m AND YEAR(sh.ShipmentDate)=?y" +
                " GROUP BY sh.ShipmentID, sh.ShipmentDate, s.StateName, c.ChannelName, tm.ModeName, sh.Status" +
                " ORDER BY sh.ShipmentDate DESC;",
                new MySql.Data.MySqlClient.MySqlParameter("?m", SelMonth),
                new MySql.Data.MySqlClient.MySqlParameter("?y", SelYear));
            rptShipments.DataSource = dt;
            rptShipments.DataBind();
            pnlShipEmpty.Visible = dt.Rows.Count == 0;
        }

        private void ShowAlert(string msg, bool success)
        {
            pnlAlert.Visible = true; lblAlert.Text = msg;
            pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger");
        }
    }
}
