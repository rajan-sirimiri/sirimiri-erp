using System;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using StockApp.DAL;

namespace StockApp
{
    public partial class Reports : Page
    {
        public string ActiveTab  { get; private set; } = "stock";
        public int    StockDays  { get; private set; } = 30;
        public int    DailyDays  { get; private set; } = 30;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null) { Response.Redirect("Login.aspx"); return; }

            // Module access check
            string __role = Session["Role"]?.ToString() ?? "";
            if (!DatabaseHelper.RoleHasModuleAccess(__role, "SA", "SA_REPORTS"))
            { Response.Redirect("SAHome.aspx"); return; }

            // Role
            string role = Session["Role"]?.ToString() ?? "";
            pnlAdminMenu.Visible = (role == "Admin");
            lblUserInfo.Text     = Session["FullName"]?.ToString() ?? "";

            // Tab
            ActiveTab = (Request.QueryString["tab"] == "daily") ? "daily" : "stock";
            divStockReport.Visible = (ActiveTab == "stock");
            divDailyReport.Visible = (ActiveTab == "daily");

            if (!IsPostBack)
            {
                // Set default date range
                txtStockFrom.Text = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
                txtStockTo.Text   = DateTime.Today.ToString("yyyy-MM-dd");
                txtDailyFrom.Text = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
                txtDailyTo.Text   = DateTime.Today.ToString("yyyy-MM-dd");

                LoadStates();
            }

            StockDays = int.TryParse(hfStockDays.Value, out int sd) ? sd : 30;
            DailyDays = int.TryParse(hfDailyDays.Value, out int dd) ? dd : 30;
        }

        // ── Load States ────────────────────────────────────────────
        private void LoadStates()
        {
            DataTable dt = DatabaseHelper.GetStates();
            ddlStockState.Items.Clear();
            ddlStockState.Items.Add(new ListItem("All States", "0"));
            foreach (DataRow r in dt.Rows)
                ddlStockState.Items.Add(new ListItem(r["StateName"].ToString(), r["StateID"].ToString()));
        }

        protected void ddlStockState_Changed(object sender, EventArgs e)
        {
            ddlStockCity.Items.Clear();
            ddlStockCity.Items.Add(new ListItem("All Cities", "0"));
            ddlStockDist.Items.Clear();
            ddlStockDist.Items.Add(new ListItem("All Distributors", "0"));

            int stateId = int.Parse(ddlStockState.SelectedValue);
            if (stateId > 0)
            {
                DataTable dt = DatabaseHelper.GetCitiesByState(stateId);
                foreach (DataRow r in dt.Rows)
                    ddlStockCity.Items.Add(new ListItem(r["CityName"].ToString(), r["CityID"].ToString()));
            }
        }

        protected void ddlStockCity_Changed(object sender, EventArgs e)
        {
            ddlStockDist.Items.Clear();
            ddlStockDist.Items.Add(new ListItem("All Distributors", "0"));

            int cityId = int.Parse(ddlStockCity.SelectedValue);
            if (cityId > 0)
            {
                DataTable dt = DatabaseHelper.GetDistributorsByCity(cityId);
                foreach (DataRow r in dt.Rows)
                    ddlStockDist.Items.Add(new ListItem(r["DistributorName"].ToString(), r["DistributorID"].ToString()));
            }
        }

        // ── REPORT 1: Stock Movement ───────────────────────────────
        protected void btnStockRun_Click(object sender, EventArgs e)
        {
            DateTime from, to;
            int days = int.TryParse(hfStockDays.Value, out int sd) ? sd : 30;
            if (days > 0)
            {
                to   = DateTime.Today;
                from = DateTime.Today.AddDays(-days);
            }
            else
            {
                from = DateTime.TryParse(txtStockFrom.Text, out DateTime f) ? f : DateTime.Today.AddDays(-30);
                to   = DateTime.TryParse(txtStockTo.Text,   out DateTime t) ? t : DateTime.Today;
            }

            int stateId = int.Parse(ddlStockState.SelectedValue);
            int cityId  = int.Parse(ddlStockCity.SelectedValue);
            int distId  = int.Parse(ddlStockDist.SelectedValue);

            DataTable dtPurchases = DatabaseHelper.GetStockMovementPurchases(from, to, stateId, cityId, distId);
            DataTable dtStock     = DatabaseHelper.GetStockMovementClosing(from, to, stateId, cityId, distId);

            if ((dtPurchases == null || dtPurchases.Rows.Count == 0) &&
                (dtStock     == null || dtStock.Rows.Count == 0))
            {
                pnlStockResult.Visible = false;
                pnlStockEmpty.Visible  = true;
                return;
            }

            pnlStockEmpty.Visible  = false;
            pnlStockResult.Visible = true;
            phStockReport.Controls.Clear();

            // Merge and group by distributor
            var html = BuildStockMovementHtml(dtPurchases, dtStock, from, to);
            phStockReport.Controls.Add(new LiteralControl(html));
        }

        private string BuildStockMovementHtml(DataTable dtPurchases, DataTable dtStock, DateTime from, DateTime to)
        {
            var sb = new StringBuilder();

            // Group purchases by DistributorID
            var distIds   = new System.Collections.Generic.HashSet<int>();
            var distNames = new System.Collections.Generic.Dictionary<int, string>();

            if (dtPurchases != null)
                foreach (DataRow r in dtPurchases.Rows)
                {
                    int id = Convert.ToInt32(r["DistributorID"]);
                    distIds.Add(id);
                    distNames[id] = r["DistributorName"].ToString();
                }
            if (dtStock != null)
                foreach (DataRow r in dtStock.Rows)
                {
                    int id = Convert.ToInt32(r["DistributorID"]);
                    distIds.Add(id);
                    if (!distNames.ContainsKey(id)) distNames[id] = r["DistributorName"].ToString();
                }

            sb.Append("<div class='report-card'>");
            sb.AppendFormat(
                "<div class='report-header'>" +
                "<div class='report-header-title'>Distributor Stock Movement Report</div>" +
                "<div class='report-meta'>{0} to {1} &nbsp;|&nbsp; {2} Distributor(s)</div>" +
                "</div>",
                from.ToString("dd MMM yyyy"), to.ToString("dd MMM yyyy"), distIds.Count);

            foreach (int distId in distIds)
            {
                string distName = distNames[distId];

                // Aggregate purchases by date
                var purchByDate = new System.Collections.Generic.SortedDictionary<DateTime, decimal>();
                if (dtPurchases != null)
                    foreach (DataRow r in dtPurchases.Rows)
                        if (Convert.ToInt32(r["DistributorID"]) == distId)
                        {
                            DateTime d = Convert.ToDateTime(r["OrderDate"]).Date;
                            decimal  u = Convert.ToDecimal(r["NoOfUnits"]);
                            if (!purchByDate.ContainsKey(d)) purchByDate[d] = 0;
                            purchByDate[d] += u;
                        }

                // Closing stock entries by date
                var stockByDate = new System.Collections.Generic.SortedDictionary<DateTime, decimal>();
                if (dtStock != null)
                    foreach (DataRow r in dtStock.Rows)
                        if (Convert.ToInt32(r["DistributorID"]) == distId)
                        {
                            DateTime d = Convert.ToDateTime(r["EntryDate"]).Date;
                            stockByDate[d] = Convert.ToDecimal(r["CurrentStock"]);
                        }

                // Merge all dates
                var allDates = new System.Collections.Generic.SortedSet<DateTime>(purchByDate.Keys);
                foreach (var d in stockByDate.Keys) allDates.Add(d);

                decimal totalPurchased = 0;
                foreach (var v in purchByDate.Values) totalPurchased += v;
                DateTime lastStockDate = DateTime.MinValue;
                foreach (var k in stockByDate.Keys) if (k > lastStockDate) lastStockDate = k;
                decimal lastClosing = lastStockDate > DateTime.MinValue ? stockByDate[lastStockDate] : 0;

                sb.Append("<div class='dist-section'>");
                sb.AppendFormat(
                    "<div class='dist-section-header'>{0} <span>Total Purchased: {1:N0} units &nbsp;|&nbsp; Last Closing Stock: {2:N0}</span></div>",
                    System.Web.HttpUtility.HtmlEncode(distName), totalPurchased, lastClosing);

                sb.Append("<div class='report-table-wrap'><table class='report-tbl'>");
                sb.Append("<thead><tr>" +
                          "<th>Date</th><th>Type</th>" +
                          "<th style='text-align:right;'>Units</th>" +
                          "</tr></thead><tbody>");

                foreach (DateTime d in allDates)
                {
                    if (purchByDate.ContainsKey(d))
                        sb.AppendFormat(
                            "<tr class='row-purchase'><td>{0}</td>" +
                            "<td><span class='badge badge-purchase'>&#x1F6D2; Purchase</span></td>" +
                            "<td style='text-align:right;font-weight:600;'>{1:N0}</td></tr>",
                            d.ToString("dd MMM yyyy"), purchByDate[d]);

                    if (stockByDate.ContainsKey(d))
                        sb.AppendFormat(
                            "<tr class='row-stock'><td>{0}</td>" +
                            "<td><span class='badge badge-stock'>&#x1F4CB; Closing Stock</span></td>" +
                            "<td style='text-align:right;font-weight:600;'>{1:N0}</td></tr>",
                            d.ToString("dd MMM yyyy"), stockByDate[d]);
                }

                sb.AppendFormat(
                    "<tr class='row-total'><td colspan='2'>Total Purchased</td>" +
                    "<td style='text-align:right;'>{0:N0}</td></tr>",
                    totalPurchased);

                sb.Append("</tbody></table></div></div>");
            }

            sb.Append("</div>");
            return sb.ToString();
        }

        // ── REPORT 2: Daily Sales ──────────────────────────────────
        protected void btnDailyRun_Click(object sender, EventArgs e)
        {
            DateTime from, to;
            int days = int.TryParse(hfDailyDays.Value, out int dd) ? dd : 30;
            if (days > 0)
            {
                to   = DateTime.Today;
                from = DateTime.Today.AddDays(-days);
            }
            else
            {
                from = DateTime.TryParse(txtDailyFrom.Text, out DateTime f) ? f : DateTime.Today.AddDays(-30);
                to   = DateTime.TryParse(txtDailyTo.Text,   out DateTime t) ? t : DateTime.Today;
            }

            int    userId = Convert.ToInt32(Session["UserID"]);
            string role   = Session["Role"]?.ToString() ?? "FieldUser";

            DataTable dt = DatabaseHelper.GetDailySalesReportByUser(from, to, userId, role);

            if (dt == null || dt.Rows.Count == 0)
            {
                pnlDailyResult.Visible = false;
                pnlDailyEmpty.Visible  = true;
                return;
            }

            pnlDailyEmpty.Visible  = false;
            pnlDailyResult.Visible = true;
            phDailyReport.Controls.Clear();

            var html = BuildDailySalesHtml(dt, from, to, role);
            phDailyReport.Controls.Add(new LiteralControl(html));
        }

        private string BuildDailySalesHtml(DataTable dt, DateTime from, DateTime to, string role)
        {
            var sb = new StringBuilder();

            sb.Append("<div class='report-card'>");
            sb.AppendFormat(
                "<div class='report-header'>" +
                "<div class='report-header-title'>Daily Sales Report</div>" +
                "<div class='report-meta'>{0} to {1}</div>" +
                "</div>",
                from.ToString("dd MMM yyyy"), to.ToString("dd MMM yyyy"));

            // Group rows by UserName
            var users = new System.Collections.Generic.List<string>();
            var byUser = new System.Collections.Generic.Dictionary<string,
                System.Collections.Generic.SortedDictionary<DateTime, decimal>>();

            foreach (DataRow r in dt.Rows)
            {
                string   userName = r["UserName"].ToString();
                DateTime d        = Convert.ToDateTime(r["SaleDate"]).Date;
                decimal  u        = Convert.ToDecimal(r["TotalUnits"]);

                if (!byUser.ContainsKey(userName))
                {
                    byUser[userName] = new System.Collections.Generic.SortedDictionary<DateTime, decimal>();
                    users.Add(userName);
                }
                if (!byUser[userName].ContainsKey(d)) byUser[userName][d] = 0;
                byUser[userName][d] += u;
            }

            // For Manager/Admin: show per-user sections then grand summary
            // For FieldUser: single section
            bool showUserSections = (role == "Admin" || role == "Manager") && users.Count > 1;

            decimal grandTotal = 0;

            if (showUserSections)
            {
                foreach (string userName in users)
                {
                    var dateMap  = byUser[userName];
                    decimal userTotal = 0;
                    foreach (var v in dateMap.Values) userTotal += v;
                    grandTotal += userTotal;

                    sb.Append("<div class='dist-section'>");
                    sb.AppendFormat(
                        "<div class='dist-section-header'>{0} <span>Total: {1:N0} units</span></div>",
                        System.Web.HttpUtility.HtmlEncode(userName), userTotal);

                    sb.Append("<div class='report-table-wrap'><table class='report-tbl'>");
                    sb.Append("<thead><tr><th>Date</th>" +
                              "<th style='text-align:right;'>Units Sold</th></tr></thead><tbody>");

                    foreach (var kvp in dateMap)
                        sb.AppendFormat(
                            "<tr class='row-purchase'><td>{0}</td>" +
                            "<td style='text-align:right;font-weight:600;'>{1:N0}</td></tr>",
                            kvp.Key.ToString("dd MMM yyyy"), kvp.Value);

                    sb.AppendFormat(
                        "<tr class='row-total'><td>User Total</td>" +
                        "<td style='text-align:right;'>{0:N0}</td></tr>",
                        userTotal);

                    sb.Append("</tbody></table></div></div>");
                }

                // Grand total across all users
                sb.Append("<div class='report-card' style='margin-top:4px;'>");
                sb.AppendFormat(
                    "<table class='report-tbl'><tbody>" +
                    "<tr style='background:#1a1a1a;color:#fff;font-weight:700;'>" +
                    "<td>Grand Total — All Users</td>" +
                    "<td style='text-align:right;'>{0:N0} units</td></tr>" +
                    "</tbody></table></div>",
                    grandTotal);
            }
            else
            {
                // Single user view
                sb.Append("<div class='report-table-wrap'><table class='report-tbl'>");
                sb.Append("<thead><tr><th>Date</th>" +
                          "<th style='text-align:right;'>Units Sold</th></tr></thead><tbody>");

                string userName = users.Count > 0 ? users[0] : "";
                if (byUser.ContainsKey(userName))
                    foreach (var kvp in byUser[userName])
                    {
                        sb.AppendFormat(
                            "<tr class='row-purchase'><td>{0}</td>" +
                            "<td style='text-align:right;font-weight:600;'>{1:N0}</td></tr>",
                            kvp.Key.ToString("dd MMM yyyy"), kvp.Value);
                        grandTotal += kvp.Value;
                    }

                sb.AppendFormat(
                    "<tr style='background:#1a1a1a;color:#fff;font-weight:700;'>" +
                    "<td>Total</td><td style='text-align:right;'>{0:N0}</td></tr>",
                    grandTotal);

                sb.Append("</tbody></table></div>");
            }

            sb.Append("</div>"); // report-card
            return sb.ToString();
        }
    }
}
