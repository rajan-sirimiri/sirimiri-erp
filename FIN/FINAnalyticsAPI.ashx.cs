using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;
using FINApp.DAL;

namespace FINApp
{
    public class FINAnalyticsAPI : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            if (context.Session["FIN_UserID"] == null)
            {
                context.Response.Write("{\"error\":\"Not authenticated\"}");
                context.Response.StatusCode = 401;
                return;
            }

            string action = context.Request["action"] ?? "";
            string state = context.Request["state"] ?? "";
            string city = context.Request["city"] ?? "";
            string custIdStr = context.Request["customerId"] ?? "0";
            int.TryParse(custIdStr, out int customerId);

            try
            {
                string json;
                switch (action)
                {
                    case "overview":       json = GetOverview(); break;
                    case "monthlyTrend":   json = GetMonthlyTrend(); break;
                    case "stateBreakdown": json = GetStateBreakdown(); break;
                    case "cityBreakdown":  json = GetCityBreakdown(state); break;
                    case "productMix":     json = GetProductMix(state, city); break;
                    case "distributors":   json = GetDistributors(state); break;
                    case "distDetail":     json = GetDistributorDetail(customerId); break;
                    case "topProducts":    json = GetTopProducts(); break;
                    case "alerts":         json = GetAlerts(); break;
                    default:               json = "{\"error\":\"Unknown action\"}"; break;
                }
                context.Response.Write(json);
            }
            catch (Exception ex)
            {
                context.Response.Write("{\"error\":\"" + EscJson(ex.Message) + "\"}");
            }
        }

        private string GetOverview()
        {
            var summary = FINDatabaseHelper.GetStateSalesSummary();
            decimal totalSales = 0, thisMonth = 0, lastMonth = 0;
            int totalInv = 0, totalCust = 0;

            foreach (DataRow r in summary.Rows)
            {
                totalSales += Convert.ToDecimal(r["TotalSales"]);
                totalInv += Convert.ToInt32(r["TotalInvoices"]);
                totalCust += Convert.ToInt32(r["TotalCustomers"]);
            }

            var trend = FINDatabaseHelper.GetMonthlySalesByState();
            var monthTotals = new Dictionary<string, decimal>();
            foreach (DataRow r in trend.Rows)
            {
                string m = r["Month"].ToString();
                decimal v = Convert.ToDecimal(r["SalesValue"]);
                if (!monthTotals.ContainsKey(m)) monthTotals[m] = 0;
                monthTotals[m] += v;
            }
            var months = monthTotals.Keys.OrderBy(x => x).ToList();
            if (months.Count >= 1) thisMonth = monthTotals[months.Last()];
            if (months.Count >= 2) lastMonth = monthTotals[months[months.Count - 2]];

            decimal growth = lastMonth > 0 ? ((thisMonth - lastMonth) / lastMonth * 100) : 0;

            var sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat("\"totalSales\":{0},", D(totalSales));
            sb.AppendFormat("\"totalInvoices\":{0},", totalInv);
            sb.AppendFormat("\"totalCustomers\":{0},", totalCust);
            sb.AppendFormat("\"totalStates\":{0},", summary.Rows.Count);
            sb.AppendFormat("\"thisMonth\":{0},", D(thisMonth));
            sb.AppendFormat("\"lastMonth\":{0},", D(lastMonth));
            sb.AppendFormat("\"growthPct\":{0},", D(growth));
            sb.AppendFormat("\"currentMonthLabel\":\"{0}\",", months.Count > 0 ? months.Last() : "");
            sb.AppendFormat("\"monthCount\":{0}", months.Count);
            sb.Append("}");
            return sb.ToString();
        }

        private string GetMonthlyTrend()
        {
            var data = FINDatabaseHelper.GetMonthlySalesByState();
            var monthTotals = new SortedDictionary<string, decimal>();
            var monthInvoices = new SortedDictionary<string, int>();

            foreach (DataRow r in data.Rows)
            {
                string m = r["Month"].ToString();
                decimal v = Convert.ToDecimal(r["SalesValue"]);
                int ic = Convert.ToInt32(r["InvoiceCount"]);
                if (!monthTotals.ContainsKey(m)) { monthTotals[m] = 0; monthInvoices[m] = 0; }
                monthTotals[m] += v;
                monthInvoices[m] += ic;
            }

            var sb = new StringBuilder("[");
            bool first = true;
            foreach (var kv in monthTotals)
            {
                if (!first) sb.Append(",");
                sb.AppendFormat("{{\"month\":\"{0}\",\"sales\":{1},\"invoices\":{2}}}",
                    kv.Key, D(kv.Value), monthInvoices[kv.Key]);
                first = false;
            }
            sb.Append("]");
            return sb.ToString();
        }

        private string GetStateBreakdown()
        {
            var data = FINDatabaseHelper.GetMonthlySalesByState();
            var states = new Dictionary<string, SortedDictionary<string, decimal>>();
            var stateTotals = new Dictionary<string, decimal>();

            foreach (DataRow r in data.Rows)
            {
                string st = r["State"].ToString();
                string m = r["Month"].ToString();
                decimal v = Convert.ToDecimal(r["SalesValue"]);
                if (!states.ContainsKey(st)) { states[st] = new SortedDictionary<string, decimal>(); stateTotals[st] = 0; }
                states[st][m] = v;
                stateTotals[st] += v;
            }

            var ordered = stateTotals.OrderByDescending(x => x.Value).ToList();
            var allMonths = states.Values.SelectMany(d => d.Keys).Distinct().OrderBy(x => x).ToList();

            var sb = new StringBuilder("{\"months\":[");
            sb.Append(string.Join(",", allMonths.Select(m => "\"" + m + "\"")));
            sb.Append("],\"states\":[");

            bool first = true;
            foreach (var kv in ordered)
            {
                if (!first) sb.Append(",");
                sb.AppendFormat("{{\"name\":\"{0}\",\"total\":{1},\"monthly\":[", EscJson(kv.Key), D(kv.Value));
                sb.Append(string.Join(",", allMonths.Select(m =>
                    states[kv.Key].ContainsKey(m) ? D(states[kv.Key][m]) : "0")));
                sb.Append("]}");
                first = false;
            }
            sb.Append("]}");
            return sb.ToString();
        }

        private string GetCityBreakdown(string state)
        {
            if (string.IsNullOrEmpty(state)) return "{\"months\":[],\"cities\":[]}";
            var data = FINDatabaseHelper.GetMonthlySalesByCity(state);
            var cities = new Dictionary<string, SortedDictionary<string, decimal>>();
            var cityTotals = new Dictionary<string, decimal>();

            foreach (DataRow r in data.Rows)
            {
                string c = r["City"].ToString();
                string m = r["Month"].ToString();
                decimal v = Convert.ToDecimal(r["SalesValue"]);
                if (!cities.ContainsKey(c)) { cities[c] = new SortedDictionary<string, decimal>(); cityTotals[c] = 0; }
                cities[c][m] = v;
                cityTotals[c] += v;
            }

            var ordered = cityTotals.OrderByDescending(x => x.Value).ToList();
            var allMonths = cities.Values.SelectMany(d => d.Keys).Distinct().OrderBy(x => x).ToList();

            var sb = new StringBuilder("{\"months\":[");
            sb.Append(string.Join(",", allMonths.Select(m => "\"" + m + "\"")));
            sb.Append("],\"cities\":[");

            bool first = true;
            foreach (var kv in ordered)
            {
                if (!first) sb.Append(",");
                sb.AppendFormat("{{\"name\":\"{0}\",\"total\":{1},\"monthly\":[", EscJson(kv.Key), D(kv.Value));
                sb.Append(string.Join(",", allMonths.Select(m =>
                    cities[kv.Key].ContainsKey(m) ? D(cities[kv.Key][m]) : "0")));
                sb.Append("]}");
                first = false;
            }
            sb.Append("]}");
            return sb.ToString();
        }

        private string GetProductMix(string state, string city)
        {
            DataTable data;
            if (!string.IsNullOrEmpty(city))
                data = FINDatabaseHelper.GetMonthlyProductSalesByCity(state, city);
            else if (!string.IsNullOrEmpty(state))
                data = FINDatabaseHelper.GetMonthlyProductSalesByState(state);
            else
                return "{\"months\":[],\"products\":[]}";

            var prods = new Dictionary<string, SortedDictionary<string, decimal>>();
            var prodTotals = new Dictionary<string, decimal>();

            foreach (DataRow r in data.Rows)
            {
                string p = r["ProductName"] != DBNull.Value ? r["ProductName"].ToString() : "Unknown";
                string m = r["Month"].ToString();
                decimal v = Convert.ToDecimal(r["SalesValue"]);
                if (!prods.ContainsKey(p)) { prods[p] = new SortedDictionary<string, decimal>(); prodTotals[p] = 0; }
                prods[p][m] = v;
                prodTotals[p] += v;
            }

            var ordered = prodTotals.OrderByDescending(x => x.Value).ToList();
            var allMonths = prods.Values.SelectMany(d => d.Keys).Distinct().OrderBy(x => x).ToList();

            var sb = new StringBuilder("{\"months\":[");
            sb.Append(string.Join(",", allMonths.Select(m => "\"" + m + "\"")));
            sb.Append("],\"products\":[");

            bool first = true;
            foreach (var kv in ordered)
            {
                if (!first) sb.Append(",");
                sb.AppendFormat("{{\"name\":\"{0}\",\"total\":{1},\"monthly\":[", EscJson(kv.Key), D(kv.Value));
                sb.Append(string.Join(",", allMonths.Select(m =>
                    prods[kv.Key].ContainsKey(m) ? D(prods[kv.Key][m]) : "0")));
                sb.Append("]}");
                first = false;
            }
            sb.Append("]}");
            return sb.ToString();
        }

        private string GetTopProducts()
        {
            var data = FINDatabaseHelper.ExecuteQueryPublic(
                "SELECT IFNULL(p.ProductName, sl.TallyProductName) AS ProductName," +
                " SUM(sl.Value) AS TotalSales, SUM(sl.Quantity) AS TotalQty," +
                " COUNT(DISTINCT si.InvoiceID) AS InvoiceCount," +
                " COUNT(DISTINCT si.CustomerID) AS CustomerCount" +
                " FROM FIN_SalesInvoiceLine sl" +
                " JOIN FIN_SalesInvoice si ON si.InvoiceID=sl.InvoiceID" +
                " LEFT JOIN PP_Products p ON p.ProductID=sl.ProductID" +
                " WHERE sl.LineType='PRODUCT'" +
                " GROUP BY IFNULL(p.ProductName, sl.TallyProductName)" +
                " ORDER BY TotalSales DESC LIMIT 15;");

            var sb = new StringBuilder("[");
            bool first = true;
            foreach (DataRow r in data.Rows)
            {
                if (!first) sb.Append(",");
                sb.AppendFormat("{{\"name\":\"{0}\",\"sales\":{1},\"qty\":{2},\"invoices\":{3},\"customers\":{4}}}",
                    EscJson(r["ProductName"].ToString()),
                    D(Convert.ToDecimal(r["TotalSales"])),
                    D(Convert.ToDecimal(r["TotalQty"])),
                    r["InvoiceCount"], r["CustomerCount"]);
                first = false;
            }
            sb.Append("]");
            return sb.ToString();
        }

        private string GetDistributors(string state)
        {
            if (string.IsNullOrEmpty(state)) return "[]";
            var data = FINDatabaseHelper.GetDistributorPerformance(state);

            var sb = new StringBuilder("[");
            bool first = true;
            foreach (DataRow r in data.Rows)
            {
                if (!first) sb.Append(",");
                decimal sales = Convert.ToDecimal(r["TotalSales"]);
                int orders = Convert.ToInt32(r["TotalOrders"]);
                int activeMonths = Convert.ToInt32(r["ActiveMonths"]);
                string fo = r["FirstOrder"] != DBNull.Value ? Convert.ToDateTime(r["FirstOrder"]).ToString("yyyy-MM-dd") : "";
                string lo = r["LastOrder"] != DBNull.Value ? Convert.ToDateTime(r["LastOrder"]).ToString("yyyy-MM-dd") : "";
                int daysSinceLast = 0;
                if (r["LastOrder"] != DBNull.Value)
                    daysSinceLast = (int)(DateTime.Today - Convert.ToDateTime(r["LastOrder"])).TotalDays;

                sb.AppendFormat("{{\"id\":{0},\"name\":\"{1}\",\"type\":\"{2}\",\"city\":\"{3}\",\"pin\":\"{4}\",",
                    r["CustomerID"], EscJson(r["CustomerName"].ToString()), r["CustomerType"],
                    EscJson(r["City"] != DBNull.Value ? r["City"].ToString() : ""),
                    r["PinCode"] != DBNull.Value ? r["PinCode"].ToString() : "");
                sb.AppendFormat("\"sales\":{0},\"orders\":{1},\"activeMonths\":{2},",
                    D(sales), orders, activeMonths);
                sb.AppendFormat("\"firstOrder\":\"{0}\",\"lastOrder\":\"{1}\",\"daysSinceLast\":{2}}}",
                    fo, lo, daysSinceLast);
                first = false;
            }
            sb.Append("]");
            return sb.ToString();
        }

        private string GetDistributorDetail(int customerId)
        {
            if (customerId <= 0) return "{\"monthly\":[],\"products\":[]}";
            var monthly = FINDatabaseHelper.GetDistributorMonthlySales(customerId);
            var products = FINDatabaseHelper.GetDistributorProducts(customerId);

            var sb = new StringBuilder("{\"monthly\":[");
            bool first = true;
            foreach (DataRow r in monthly.Rows)
            {
                if (!first) sb.Append(",");
                sb.AppendFormat("{{\"month\":\"{0}\",\"sales\":{1},\"orders\":{2}}}",
                    r["Month"], D(Convert.ToDecimal(r["SalesValue"])), r["OrderCount"]);
                first = false;
            }
            sb.Append("],\"products\":[");
            first = true;
            foreach (DataRow r in products.Rows)
            {
                if (!first) sb.Append(",");
                sb.AppendFormat("{{\"name\":\"{0}\",\"sales\":{1},\"qty\":{2},\"orders\":{3}}}",
                    EscJson(r["ProductName"].ToString()),
                    D(Convert.ToDecimal(r["TotalSales"])),
                    D(Convert.ToDecimal(r["TotalQty"])),
                    r["OrderCount"]);
                first = false;
            }
            sb.Append("]}");
            return sb.ToString();
        }

        private string GetAlerts()
        {
            var silent = FINDatabaseHelper.ExecuteQueryPublic(
                "SELECT c.CustomerName, c.City, c.State, MAX(si.InvoiceDate) AS LastOrder," +
                " DATEDIFF(CURDATE(), MAX(si.InvoiceDate)) AS DaysSilent," +
                " SUM(si.TotalValue) AS TotalSales" +
                " FROM PK_Customers c" +
                " JOIN FIN_SalesInvoice si ON si.CustomerID=c.CustomerID" +
                " WHERE c.CustomerType IN ('DI','ST') AND c.IsActive=1" +
                " GROUP BY c.CustomerID, c.CustomerName, c.City, c.State" +
                " HAVING DaysSilent > 45" +
                " ORDER BY TotalSales DESC LIMIT 20;");

            var sb = new StringBuilder("{\"silentDistributors\":[");
            bool first = true;
            foreach (DataRow r in silent.Rows)
            {
                if (!first) sb.Append(",");
                sb.AppendFormat("{{\"name\":\"{0}\",\"city\":\"{1}\",\"state\":\"{2}\"," +
                    "\"lastOrder\":\"{3:yyyy-MM-dd}\",\"daysSilent\":{4},\"totalSales\":{5}}}",
                    EscJson(r["CustomerName"].ToString()),
                    EscJson(r["City"] != DBNull.Value ? r["City"].ToString() : ""),
                    EscJson(r["State"] != DBNull.Value ? r["State"].ToString() : ""),
                    r["LastOrder"], r["DaysSilent"],
                    D(Convert.ToDecimal(r["TotalSales"])));
                first = false;
            }
            sb.Append("]}");
            return sb.ToString();
        }

        private string D(decimal v) => v.ToString("0.00", CultureInfo.InvariantCulture);
        private string EscJson(string s) => (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", "");
    }
}
