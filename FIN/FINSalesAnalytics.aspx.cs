using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using FINApp.DAL;

namespace FINApp
{
    public partial class FINSalesAnalytics : Page
    {
        // ── Declarations ──
        protected Label        lblNavUser;
        protected HiddenField  hfSection, hfStateMonthlyData, hfCityMonthlyData, hfProductMonthlyData, hfDistributorChartData;
        protected LinkButton   btnTab1, btnTab2, btnTab3;
        protected Panel        pnlSection1, pnlSection2, pnlSection3;

        // Section 1
        protected Label        lblTotalSales, lblTotalInvoices, lblTotalCustomers, lblTotalStates, lblDataRange;
        protected DropDownList ddlS1State;
        protected Panel        pnlCityChart;
        protected Literal      litStateTable, litCityTable;

        // Section 2
        protected DropDownList ddlS2State, ddlS2City;
        protected Panel        pnlProductResults;
        protected Label        lblProductScope;
        protected Literal      litProductTable, litProductSummary;

        // Section 3
        protected DropDownList ddlS3State, ddlDistributor;
        protected Panel        pnlDistResults, pnlDistDetail;
        protected Label        lblDistScope;
        protected Literal      litDistTable, litDistProducts;

        // ── Page Load ──
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["FIN_UserID"] == null) { Response.Redirect("FINLogin.aspx"); return; }
            lblNavUser.Text = Session["FIN_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
            {
                BindStateDropdowns();
                LoadSection1();
            }
        }

        // ── Tab Switching ──
        protected void btnTab1_Click(object sender, EventArgs e) { SwitchSection(1); LoadSection1(); }
        protected void btnTab2_Click(object sender, EventArgs e) { SwitchSection(2); }
        protected void btnTab3_Click(object sender, EventArgs e) { SwitchSection(3); }

        private void SwitchSection(int section)
        {
            hfSection.Value = section.ToString();
            btnTab1.CssClass = "section-tab" + (section == 1 ? " active" : "");
            btnTab2.CssClass = "section-tab" + (section == 2 ? " active" : "");
            btnTab3.CssClass = "section-tab" + (section == 3 ? " active" : "");
            pnlSection1.Visible = section == 1;
            pnlSection2.Visible = section == 2;
            pnlSection3.Visible = section == 3;

            hfStateMonthlyData.Value = "";
            hfCityMonthlyData.Value = "";
            hfProductMonthlyData.Value = "";
            hfDistributorChartData.Value = "";
        }

        private void BindStateDropdowns()
        {
            var states = FINDatabaseHelper.GetSalesStates();
            foreach (var ddl in new[] { ddlS1State, ddlS2State, ddlS3State })
            {
                ddl.Items.Clear();
                ddl.Items.Add(new ListItem("— Select State —", ""));
                foreach (DataRow r in states.Rows)
                    ddl.Items.Add(new ListItem(r["State"].ToString(), r["State"].ToString()));
            }
        }

        // ══════════════════════════════════════════════════════════
        // SECTION 1: OVERALL SALES GROWTH
        // ══════════════════════════════════════════════════════════

        private void LoadSection1()
        {
            // KPIs from state summary
            var summary = FINDatabaseHelper.GetStateSalesSummary();
            decimal totalSales = 0; int totalInv = 0, totalCust = 0;
            DateTime? minDate = null, maxDate = null;

            foreach (DataRow r in summary.Rows)
            {
                totalSales += Convert.ToDecimal(r["TotalSales"]);
                totalInv += Convert.ToInt32(r["TotalInvoices"]);
                totalCust += Convert.ToInt32(r["TotalCustomers"]);
                DateTime fi = Convert.ToDateTime(r["FirstInvoice"]);
                DateTime li = Convert.ToDateTime(r["LastInvoice"]);
                if (!minDate.HasValue || fi < minDate) minDate = fi;
                if (!maxDate.HasValue || li > maxDate) maxDate = li;
            }

            lblTotalSales.Text = FormatCurrency(totalSales);
            lblTotalInvoices.Text = totalInv.ToString("N0");
            lblTotalCustomers.Text = totalCust.ToString("N0");
            lblTotalStates.Text = summary.Rows.Count.ToString();
            lblDataRange.Text = (minDate.HasValue ? minDate.Value.ToString("MMM yy") : "—") + " — " +
                                (maxDate.HasValue ? maxDate.Value.ToString("MMM yy") : "—");

            // Monthly data by state
            var monthlyData = FINDatabaseHelper.GetMonthlySalesByState();
            BuildPivotChartAndTable(monthlyData, "State", "Month", "SalesValue",
                hfStateMonthlyData, litStateTable, true);
        }

        protected void ddlS1State_Changed(object sender, EventArgs e)
        {
            SwitchSection(1);
            LoadSection1();

            string state = ddlS1State.SelectedValue;
            if (string.IsNullOrEmpty(state)) { pnlCityChart.Visible = false; return; }

            var cityData = FINDatabaseHelper.GetMonthlySalesByCity(state);
            BuildPivotChartAndTable(cityData, "City", "Month", "SalesValue",
                hfCityMonthlyData, litCityTable, true);
            pnlCityChart.Visible = true;
        }

        // ══════════════════════════════════════════════════════════
        // SECTION 2: PRODUCT SALES ANALYSIS
        // ══════════════════════════════════════════════════════════

        protected void ddlS2State_Changed(object sender, EventArgs e)
        {
            SwitchSection(2);
            string state = ddlS2State.SelectedValue;
            if (string.IsNullOrEmpty(state)) { pnlProductResults.Visible = false; return; }

            // Populate city dropdown
            ddlS2City.Items.Clear();
            ddlS2City.Items.Add(new ListItem("— All Cities —", ""));
            var cities = FINDatabaseHelper.GetSalesCities(state);
            foreach (DataRow r in cities.Rows)
                ddlS2City.Items.Add(new ListItem(r["City"].ToString(), r["City"].ToString()));

            LoadProductData(state, "");
        }

        protected void ddlS2City_Changed(object sender, EventArgs e)
        {
            SwitchSection(2);
            string state = ddlS2State.SelectedValue;
            string city = ddlS2City.SelectedValue;
            if (string.IsNullOrEmpty(state)) return;
            LoadProductData(state, city);
        }

        private void LoadProductData(string state, string city)
        {
            DataTable productMonthly;
            string scope;

            if (string.IsNullOrEmpty(city))
            {
                productMonthly = FINDatabaseHelper.GetMonthlyProductSalesByState(state);
                scope = state;
            }
            else
            {
                productMonthly = FINDatabaseHelper.GetMonthlyProductSalesByCity(state, city);
                scope = city + ", " + state;
            }

            lblProductScope.Text = scope;
            BuildPivotChartAndTable(productMonthly, "ProductName", "Month", "SalesValue",
                hfProductMonthlyData, litProductTable, false);

            // Product summary
            var summaryDt = FINDatabaseHelper.GetProductSalesSummary(state);
            var sb = new StringBuilder();
            sb.Append("<table class='data-table'><thead><tr>");
            sb.Append("<th>#</th><th>Product</th><th>Total Sales</th><th>Total Qty</th><th>Invoices</th><th>% Share</th>");
            sb.Append("</tr></thead><tbody>");
            decimal grandTotal = 0;
            foreach (DataRow r in summaryDt.Rows) grandTotal += Convert.ToDecimal(r["TotalSales"]);

            int idx = 0;
            foreach (DataRow r in summaryDt.Rows)
            {
                idx++;
                decimal sales = Convert.ToDecimal(r["TotalSales"]);
                decimal pct = grandTotal > 0 ? (sales / grandTotal * 100) : 0;
                sb.AppendFormat("<tr><td>{0}</td><td style='font-weight:600;'>{1}</td>", idx, r["ProductName"]);
                sb.AppendFormat("<td class='num'>{0}</td>", FormatCurrency(sales));
                sb.AppendFormat("<td class='num'>{0:N0}</td>", Convert.ToDecimal(r["TotalQty"]));
                sb.AppendFormat("<td class='num'>{0}</td>", r["InvoiceCount"]);
                sb.AppendFormat("<td class='num'>{0:0.0}%</td></tr>", pct);
            }
            sb.Append("</tbody></table>");
            litProductSummary.Text = sb.ToString();
            pnlProductResults.Visible = true;
        }

        // ══════════════════════════════════════════════════════════
        // SECTION 3: DISTRIBUTOR PERFORMANCE
        // ══════════════════════════════════════════════════════════

        protected void ddlS3State_Changed(object sender, EventArgs e)
        {
            SwitchSection(3);
            string state = ddlS3State.SelectedValue;
            if (string.IsNullOrEmpty(state)) { pnlDistResults.Visible = false; return; }

            var distData = FINDatabaseHelper.GetDistributorPerformance(state);
            lblDistScope.Text = state;

            // Build ranking chart (top 20)
            var chartJson = new StringBuilder("{\"ranking\":{\"labels\":[");
            var vals = new StringBuilder("],\"values\":[");
            int shown = 0;
            foreach (DataRow r in distData.Rows)
            {
                if (shown >= 20) break;
                decimal sales = Convert.ToDecimal(r["TotalSales"]);
                if (sales <= 0) continue;
                if (shown > 0) { chartJson.Append(","); vals.Append(","); }
                chartJson.AppendFormat("\"{0}\"", EscapeJs(r["CustomerName"].ToString()));
                vals.Append(sales.ToString("0.00", CultureInfo.InvariantCulture));
                shown++;
            }
            chartJson.Append(vals).Append("]}}");
            hfDistributorChartData.Value = chartJson.ToString();

            // Build table
            var sb = new StringBuilder();
            sb.Append("<table class='data-table'><thead><tr>");
            sb.Append("<th>#</th><th>Distributor</th><th>Type</th><th>City</th><th>Total Sales</th><th>Orders</th><th>Active Months</th><th>First Order</th><th>Last Order</th><th>Repeat Rate</th>");
            sb.Append("</tr></thead><tbody>");

            int idx = 0;
            foreach (DataRow r in distData.Rows)
            {
                idx++;
                decimal sales = Convert.ToDecimal(r["TotalSales"]);
                int orders = Convert.ToInt32(r["TotalOrders"]);
                int activeMonths = Convert.ToInt32(r["ActiveMonths"]);
                string firstOrder = r["FirstOrder"] != DBNull.Value ? Convert.ToDateTime(r["FirstOrder"]).ToString("dd-MMM-yy") : "—";
                string lastOrder = r["LastOrder"] != DBNull.Value ? Convert.ToDateTime(r["LastOrder"]).ToString("dd-MMM-yy") : "—";

                // Repeat rate = active months / total months in range
                int totalMonthsAvail = 13; // approx 13 months of data
                double repeatRate = totalMonthsAvail > 0 ? (double)activeMonths / totalMonthsAvail * 100 : 0;
                string repeatClass = repeatRate >= 70 ? "growth-pos" : repeatRate >= 40 ? "" : "growth-neg";

                string typeTag = r["CustomerType"].ToString() == "DI" ? "DI" : "ST";
                string typeCss = typeTag == "DI" ? "dist-tag-di" : "dist-tag-st";

                sb.AppendFormat("<tr><td>{0}</td><td style='font-weight:600;'>{1}</td>", idx, r["CustomerName"]);
                sb.AppendFormat("<td><span class='dist-tag {0}'>{1}</span></td>", typeCss, typeTag);
                sb.AppendFormat("<td>{0}</td>", r["City"]);
                sb.AppendFormat("<td class='num'>{0}</td>", FormatCurrency(sales));
                sb.AppendFormat("<td class='num'>{0}</td>", orders);
                sb.AppendFormat("<td class='num'>{0}</td>", activeMonths);
                sb.AppendFormat("<td>{0}</td><td>{1}</td>", firstOrder, lastOrder);
                sb.AppendFormat("<td class='num {0}'>{1:0}%</td></tr>", repeatClass, repeatRate);
            }
            sb.Append("</tbody></table>");
            litDistTable.Text = sb.ToString();

            // Populate distributor dropdown
            ddlDistributor.Items.Clear();
            ddlDistributor.Items.Add(new ListItem("— Select Distributor —", "0"));
            foreach (DataRow r in distData.Rows)
            {
                decimal sales = Convert.ToDecimal(r["TotalSales"]);
                string label = r["CustomerName"].ToString();
                if (sales > 0) label += " — " + FormatCurrency(sales);
                ddlDistributor.Items.Add(new ListItem(label, r["CustomerID"].ToString()));
            }

            pnlDistResults.Visible = true;
            pnlDistDetail.Visible = false;
        }

        protected void ddlDistributor_Changed(object sender, EventArgs e)
        {
            SwitchSection(3);
            if (!int.TryParse(ddlDistributor.SelectedValue, out int custId) || custId <= 0)
            { pnlDistDetail.Visible = false; return; }

            // Reload the state-level chart
            string state = ddlS3State.SelectedValue;
            if (!string.IsNullOrEmpty(state)) ddlS3State_Changed(null, EventArgs.Empty);

            // Monthly sales for this distributor
            var monthly = FINDatabaseHelper.GetDistributorMonthlySales(custId);
            var products = FINDatabaseHelper.GetDistributorProducts(custId);

            // Build combined chart JSON
            var json = new StringBuilder();
            json.Append("{");

            // Ranking — keep from existing hfDistributorChartData
            string existing = hfDistributorChartData.Value;
            if (existing.StartsWith("{\"ranking\""))
            {
                json.Append(existing.Substring(1, existing.Length - 2)); // strip outer {}
                json.Append(",");
            }

            // Monthly
            json.Append("\"monthly\":{\"labels\":[");
            var mVals = new StringBuilder("],\"values\":[");
            bool first = true;
            foreach (DataRow r in monthly.Rows)
            {
                if (!first) { json.Append(","); mVals.Append(","); }
                json.AppendFormat("\"{0}\"", FormatMonth(r["Month"].ToString()));
                mVals.Append(Convert.ToDecimal(r["SalesValue"]).ToString("0.00", CultureInfo.InvariantCulture));
                first = false;
            }
            json.Append(mVals).Append("]},");

            // Products (doughnut)
            json.Append("\"products\":{\"labels\":[");
            var pVals = new StringBuilder("],\"values\":[");
            first = true;
            int pCount = 0;
            foreach (DataRow r in products.Rows)
            {
                if (pCount >= 10) break; // top 10
                if (!first) { json.Append(","); pVals.Append(","); }
                json.AppendFormat("\"{0}\"", EscapeJs(r["ProductName"].ToString()));
                pVals.Append(Convert.ToDecimal(r["TotalSales"]).ToString("0.00", CultureInfo.InvariantCulture));
                first = false;
                pCount++;
            }
            json.Append(pVals).Append("]}");
            json.Append("}");
            hfDistributorChartData.Value = json.ToString();

            // Products table
            var sb = new StringBuilder();
            sb.Append("<table class='data-table'><thead><tr>");
            sb.Append("<th>#</th><th>Product</th><th>Total Sales</th><th>Qty</th><th>Orders</th>");
            sb.Append("</tr></thead><tbody>");
            int idx = 0;
            foreach (DataRow r in products.Rows)
            {
                idx++;
                sb.AppendFormat("<tr><td>{0}</td><td style='font-weight:600;'>{1}</td>", idx, r["ProductName"]);
                sb.AppendFormat("<td class='num'>{0}</td>", FormatCurrency(Convert.ToDecimal(r["TotalSales"])));
                sb.AppendFormat("<td class='num'>{0:N0}</td>", Convert.ToDecimal(r["TotalQty"]));
                sb.AppendFormat("<td class='num'>{0}</td></tr>", r["OrderCount"]);
            }
            sb.Append("</tbody></table>");
            litDistProducts.Text = sb.ToString();
            pnlDistDetail.Visible = true;
        }

        // ══════════════════════════════════════════════════════════
        // SHARED: PIVOT TABLE + CHART BUILDER
        // ══════════════════════════════════════════════════════════

        private void BuildPivotChartAndTable(DataTable data, string rowField, string colField, string valueField,
            HiddenField chartHf, Literal tableLit, bool showGrowth)
        {
            if (data.Rows.Count == 0) { chartHf.Value = ""; tableLit.Text = "<p style='color:#999;'>No data available.</p>"; return; }

            // Get unique rows and columns
            var rows = new List<string>();
            var cols = new List<string>();
            var dict = new Dictionary<string, Dictionary<string, decimal>>();

            foreach (DataRow r in data.Rows)
            {
                string rowVal = r[rowField].ToString();
                string colVal = r[colField].ToString();
                decimal val = Convert.ToDecimal(r[valueField]);

                if (!rows.Contains(rowVal)) rows.Add(rowVal);
                if (!cols.Contains(colVal)) cols.Add(colVal);
                if (!dict.ContainsKey(rowVal)) dict[rowVal] = new Dictionary<string, decimal>();
                dict[rowVal][colVal] = val;
            }

            cols.Sort();

            // Sort rows by total descending
            rows = rows.OrderByDescending(rv => dict.ContainsKey(rv) ? dict[rv].Values.Sum() : 0).ToList();

            // Build chart JSON
            var chartJson = new StringBuilder("{\"labels\":[");
            for (int i = 0; i < cols.Count; i++)
            {
                if (i > 0) chartJson.Append(",");
                chartJson.AppendFormat("\"{0}\"", FormatMonth(cols[i]));
            }
            chartJson.Append("],\"datasets\":[");

            int dsIdx = 0;
            foreach (string rowVal in rows.Take(10)) // Top 10 for chart
            {
                if (dsIdx > 0) chartJson.Append(",");
                chartJson.AppendFormat("{{\"label\":\"{0}\",\"data\":[", EscapeJs(rowVal));
                for (int c = 0; c < cols.Count; c++)
                {
                    if (c > 0) chartJson.Append(",");
                    decimal v = dict.ContainsKey(rowVal) && dict[rowVal].ContainsKey(cols[c]) ? dict[rowVal][cols[c]] : 0;
                    chartJson.Append(v.ToString("0.00", CultureInfo.InvariantCulture));
                }
                chartJson.Append("]}");
                dsIdx++;
            }
            chartJson.Append("]}");
            chartHf.Value = chartJson.ToString();

            // Build HTML table
            var sb = new StringBuilder();
            sb.Append("<table class='data-table'><thead><tr><th>#</th><th>" + rowField + "</th><th>Total</th>");
            foreach (var col in cols)
                sb.AppendFormat("<th>{0}</th>", FormatMonth(col));
            if (showGrowth && cols.Count >= 2)
                sb.Append("<th>Growth (Last 2 Mo)</th>");
            sb.Append("</tr></thead><tbody>");

            int rowIdx = 0;
            foreach (string rowVal in rows)
            {
                rowIdx++;
                decimal total = dict.ContainsKey(rowVal) ? dict[rowVal].Values.Sum() : 0;
                sb.AppendFormat("<tr><td>{0}</td><td style='font-weight:600;'>{1}</td>", rowIdx, rowVal);
                sb.AppendFormat("<td class='num' style='font-weight:700;'>{0}</td>", FormatCurrency(total));

                foreach (var col in cols)
                {
                    decimal v = dict.ContainsKey(rowVal) && dict[rowVal].ContainsKey(col) ? dict[rowVal][col] : 0;
                    sb.AppendFormat("<td class='num'>{0}</td>", v > 0 ? FormatCurrency(v) : "—");
                }

                if (showGrowth && cols.Count >= 2)
                {
                    string lastCol = cols[cols.Count - 1];
                    string prevCol = cols[cols.Count - 2];
                    decimal last = dict.ContainsKey(rowVal) && dict[rowVal].ContainsKey(lastCol) ? dict[rowVal][lastCol] : 0;
                    decimal prev = dict.ContainsKey(rowVal) && dict[rowVal].ContainsKey(prevCol) ? dict[rowVal][prevCol] : 0;

                    if (prev > 0)
                    {
                        decimal growth = (last - prev) / prev * 100;
                        string cls = growth >= 0 ? "growth-pos" : "growth-neg";
                        string arrow = growth >= 0 ? "▲" : "▼";
                        sb.AppendFormat("<td class='num {0}'>{1}{2:0.0}%</td>", cls, arrow, Math.Abs(growth));
                    }
                    else
                    {
                        sb.Append("<td class='num'>—</td>");
                    }
                }
                sb.Append("</tr>");
            }
            sb.Append("</tbody></table>");
            tableLit.Text = sb.ToString();
        }

        // ── Helpers ──
        private string FormatCurrency(decimal val)
        {
            if (val >= 10000000) return "₹" + (val / 10000000m).ToString("0.0") + "Cr";
            if (val >= 100000) return "₹" + (val / 100000m).ToString("0.0") + "L";
            if (val >= 1000) return "₹" + (val / 1000m).ToString("0.#") + "K";
            return "₹" + val.ToString("N0");
        }

        private string FormatMonth(string yyyymm)
        {
            if (string.IsNullOrEmpty(yyyymm) || yyyymm.Length < 7) return yyyymm;
            try
            {
                var dt = DateTime.ParseExact(yyyymm, "yyyy-MM", CultureInfo.InvariantCulture);
                return dt.ToString("MMM yy");
            }
            catch { return yyyymm; }
        }

        private string EscapeJs(string s)
        {
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("'", "\\'");
        }
    }
}
