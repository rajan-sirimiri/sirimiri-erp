using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;
using FINApp.DAL;
using MySql.Data.MySqlClient;

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
            { context.Response.Write("{\"error\":\"Not authenticated\"}"); context.Response.StatusCode = 401; return; }

            string action = context.Request["action"] ?? "";
            string state = context.Request["state"] ?? "";
            string city = context.Request["city"] ?? "";
            string dateFrom = context.Request["dateFrom"] ?? "";
            string dateTo = context.Request["dateTo"] ?? "";
            string custIdStr = context.Request["customerId"] ?? "0";
            int.TryParse(custIdStr, out int customerId);

            try
            {
                string json;
                switch (action)
                {
                    case "overview":        json = GetOverview(dateFrom, dateTo); break;
                    case "monthlyTrend":    json = GetMonthlyTrend(dateFrom, dateTo); break;
                    case "stateBreakdown":  json = GetStateBreakdown(dateFrom, dateTo); break;
                    case "cityBreakdown":   json = GetCityBreakdown(state, dateFrom, dateTo); break;
                    case "productMix":      json = GetProductMix(state, city, dateFrom, dateTo); break;
                    case "distributors":    json = GetDistributors(state, dateFrom, dateTo); break;
                    case "distDetail":      json = GetDistributorDetail(customerId, dateFrom, dateTo); break;
                    case "topProducts":     json = GetTopProducts(dateFrom, dateTo); break;
                    case "alerts":          json = GetAlerts(); break;
                    case "productView":     json = GetProductView(state, dateFrom, dateTo); break;
                    case "productList":     json = GetProductList(); break;
                    case "distView":        json = GetDistributorView(state, dateFrom, dateTo); break;
                    case "distOrdersByProduct": json = GetDistOrdersByProduct(state, dateFrom, dateTo, context.Request["products"] ?? ""); break;
                    default:                json = "{\"error\":\"Unknown action\"}"; break;
                }
                context.Response.Write(json);
            }
            catch (Exception ex) { context.Response.Write("{\"error\":\"" + EscJson(ex.Message) + "\"}"); }
        }

        // ── Helpers ──
        private string DF(string a, string df, string dt) { string f = ""; if (!string.IsNullOrEmpty(df)) f += " AND " + a + ".InvoiceDate>=?dateFrom"; if (!string.IsNullOrEmpty(dt)) f += " AND " + a + ".InvoiceDate<=?dateTo"; return f; }
        private List<MySqlParameter> DP(string df, string dt) { var l = new List<MySqlParameter>(); if (!string.IsNullOrEmpty(df)) l.Add(new MySqlParameter("?dateFrom", df)); if (!string.IsNullOrEmpty(dt)) l.Add(new MySqlParameter("?dateTo", dt)); return l; }
        private DataTable Q(string sql, params MySqlParameter[] p) => FINDatabaseHelper.ExecuteQueryPublic(sql, p);
        private string D(decimal v) => v.ToString("0.00", CultureInfo.InvariantCulture);
        private string EscJson(string s) => (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", "");
        private string JArr(DataTable dt, Func<DataRow, string> f) { var sb = new StringBuilder("["); bool first = true; foreach (DataRow r in dt.Rows) { if (!first) sb.Append(","); sb.Append(f(r)); first = false; } sb.Append("]"); return sb.ToString(); }
        private string GroupedJson(DataTable data, string gf, string cf, string vf, string an)
        {
            var groups = new Dictionary<string, SortedDictionary<string, decimal>>();
            var totals = new Dictionary<string, decimal>();
            foreach (DataRow r in data.Rows) { string g = r[gf].ToString(), m = r[cf].ToString(); decimal v = Convert.ToDecimal(r[vf]); if (!groups.ContainsKey(g)) { groups[g] = new SortedDictionary<string, decimal>(); totals[g] = 0; } if (groups[g].ContainsKey(m)) groups[g][m] += v; else groups[g][m] = v; totals[g] += v; }
            var ordered = totals.OrderByDescending(x => x.Value).ToList();
            var months = groups.Values.SelectMany(d => d.Keys).Distinct().OrderBy(x => x).ToList();
            var sb = new StringBuilder("{\"months\":["); sb.Append(string.Join(",", months.Select(m => "\"" + m + "\""))); sb.Append("],\"" + an + "\":[");
            bool first = true;
            foreach (var kv in ordered) { if (!first) sb.Append(","); sb.AppendFormat("{{\"name\":\"{0}\",\"total\":{1},\"monthly\":[", EscJson(kv.Key), D(kv.Value)); sb.Append(string.Join(",", months.Select(m => groups[kv.Key].ContainsKey(m) ? D(groups[kv.Key][m]) : "0"))); sb.Append("]}"); first = false; }
            sb.Append("]}"); return sb.ToString();
        }

        // ═══════════════════════════════════════════════
        // EXISTING ENDPOINTS (with date filtering)
        // ═══════════════════════════════════════════════

        private string GetOverview(string df, string dt)
        {
            string dfilt = DF("si", df, dt); var dp = DP(df, dt).ToArray();
            var summary = Q("SELECT c.State, SUM(si.TotalValue) AS TS, COUNT(DISTINCT si.InvoiceID) AS TI, COUNT(DISTINCT si.CustomerID) AS TC FROM FIN_SalesInvoice si JOIN PK_Customers c ON c.CustomerID=si.CustomerID WHERE c.State IS NOT NULL AND c.State!=''" + dfilt + " GROUP BY c.State ORDER BY TS DESC;", dp);
            decimal totalSales = 0; int totalInv = 0, totalCust = 0;
            foreach (DataRow r in summary.Rows) { totalSales += Convert.ToDecimal(r["TS"]); totalInv += Convert.ToInt32(r["TI"]); totalCust += Convert.ToInt32(r["TC"]); }
            var trend = Q("SELECT DATE_FORMAT(si.InvoiceDate,'%Y-%m') AS M, SUM(si.TotalValue) AS S FROM FIN_SalesInvoice si JOIN PK_Customers c ON c.CustomerID=si.CustomerID WHERE c.State IS NOT NULL" + dfilt + " GROUP BY M ORDER BY M;", dp);
            int mc = trend.Rows.Count; decimal tm = mc >= 1 ? Convert.ToDecimal(trend.Rows[mc - 1]["S"]) : 0, lm = mc >= 2 ? Convert.ToDecimal(trend.Rows[mc - 2]["S"]) : 0, g = lm > 0 ? ((tm - lm) / lm * 100) : 0;
            // Total receipts
            string rdfilt = ""; if (!string.IsNullOrEmpty(df)) rdfilt += " AND r.ReceiptDate>=?dateFrom"; if (!string.IsNullOrEmpty(dt)) rdfilt += " AND r.ReceiptDate<=?dateTo";
            var rTotal = Q("SELECT IFNULL(SUM(r.Amount),0) AS TR FROM FIN_Receipt r WHERE r.ReceiptType='CUSTOMER'" + rdfilt + ";", DP(df, dt).ToArray());
            decimal totalReceipts = rTotal.Rows.Count > 0 ? Convert.ToDecimal(rTotal.Rows[0]["TR"]) : 0;
            return string.Format(CultureInfo.InvariantCulture, "{{\"totalSales\":{0},\"totalInvoices\":{1},\"totalCustomers\":{2},\"totalStates\":{3},\"thisMonth\":{4},\"lastMonth\":{5},\"growthPct\":{6},\"monthCount\":{7},\"totalReceipts\":{8}}}", D(totalSales), totalInv, totalCust, summary.Rows.Count, D(tm), D(lm), D(g), mc, D(totalReceipts));
        }

        private string GetMonthlyTrend(string df, string dt)
        {
            string dfilt = DF("si", df, dt);
            var data = Q("SELECT DATE_FORMAT(si.InvoiceDate,'%Y-%m') AS Month, SUM(si.TotalValue) AS Sales, COUNT(DISTINCT si.InvoiceID) AS Invoices FROM FIN_SalesInvoice si JOIN PK_Customers c ON c.CustomerID=si.CustomerID WHERE c.State IS NOT NULL" + dfilt + " GROUP BY Month ORDER BY Month;", DP(df, dt).ToArray());

            // Receipt data by month
            string rdfilt = "";
            if (!string.IsNullOrEmpty(df)) rdfilt += " AND r.ReceiptDate>=?dateFrom";
            if (!string.IsNullOrEmpty(dt)) rdfilt += " AND r.ReceiptDate<=?dateTo";
            var rData = Q("SELECT DATE_FORMAT(r.ReceiptDate,'%Y-%m') AS Month, SUM(r.Amount) AS Receipts FROM FIN_Receipt r WHERE r.ReceiptType='CUSTOMER'" + rdfilt + " GROUP BY Month ORDER BY Month;", DP(df, dt).ToArray());

            // Merge into lookup
            var receiptMap = new Dictionary<string, decimal>();
            foreach (DataRow r in rData.Rows) receiptMap[r["Month"].ToString()] = Convert.ToDecimal(r["Receipts"]);

            return JArr(data, r => {
                string month = r["Month"].ToString();
                decimal receipts = receiptMap.ContainsKey(month) ? receiptMap[month] : 0;
                return string.Format("{{\"month\":\"{0}\",\"sales\":{1},\"invoices\":{2},\"receipts\":{3}}}", month, D(Convert.ToDecimal(r["Sales"])), r["Invoices"], D(receipts));
            });
        }

        private string GetStateBreakdown(string df, string dt)
        {
            var data = Q("SELECT c.State, DATE_FORMAT(si.InvoiceDate,'%Y-%m') AS Month, SUM(si.TotalValue) AS SalesValue FROM FIN_SalesInvoice si JOIN PK_Customers c ON c.CustomerID=si.CustomerID WHERE c.State IS NOT NULL AND c.State!=''" + DF("si", df, dt) + " GROUP BY c.State, Month ORDER BY c.State, Month;", DP(df, dt).ToArray());
            return GroupedJson(data, "State", "Month", "SalesValue", "states");
        }

        private string GetCityBreakdown(string state, string df, string dt)
        {
            if (string.IsNullOrEmpty(state)) return "{\"months\":[],\"cities\":[]}";
            var p = new List<MySqlParameter> { new MySqlParameter("?state", state) }; p.AddRange(DP(df, dt));
            var data = Q("SELECT c.City, DATE_FORMAT(si.InvoiceDate,'%Y-%m') AS Month, SUM(si.TotalValue) AS SalesValue FROM FIN_SalesInvoice si JOIN PK_Customers c ON c.CustomerID=si.CustomerID WHERE c.State=?state AND c.City IS NOT NULL AND c.City!=''" + DF("si", df, dt) + " GROUP BY c.City, Month ORDER BY c.City, Month;", p.ToArray());
            return GroupedJson(data, "City", "Month", "SalesValue", "cities");
        }

        private string GetProductMix(string state, string city, string df, string dt)
        {
            if (string.IsNullOrEmpty(state)) return "{\"months\":[],\"products\":[]}";
            string cf = !string.IsNullOrEmpty(city) ? " AND c.City=?city" : "";
            var p = new List<MySqlParameter> { new MySqlParameter("?state", state) }; if (!string.IsNullOrEmpty(city)) p.Add(new MySqlParameter("?city", city)); p.AddRange(DP(df, dt));
            var data = Q("SELECT IFNULL(p.ProductName,sl.TallyProductName) AS ProductName, DATE_FORMAT(si.InvoiceDate,'%Y-%m') AS Month, SUM(sl.Value) AS SalesValue FROM FIN_SalesInvoiceLine sl JOIN FIN_SalesInvoice si ON si.InvoiceID=sl.InvoiceID JOIN PK_Customers c ON c.CustomerID=si.CustomerID LEFT JOIN PP_Products p ON p.ProductID=sl.ProductID WHERE c.State=?state" + cf + " AND sl.LineType='PRODUCT'" + DF("si", df, dt) + " GROUP BY IFNULL(p.ProductName,sl.TallyProductName), Month ORDER BY IFNULL(p.ProductName,sl.TallyProductName), Month;", p.ToArray());
            return GroupedJson(data, "ProductName", "Month", "SalesValue", "products");
        }

        private string GetTopProducts(string df, string dt)
        {
            var data = Q("SELECT IFNULL(p.ProductName,sl.TallyProductName) AS ProductName, SUM(sl.Value) AS TotalSales, SUM(sl.Quantity) AS TotalQty, COUNT(DISTINCT si.InvoiceID) AS InvoiceCount, COUNT(DISTINCT si.CustomerID) AS CustomerCount FROM FIN_SalesInvoiceLine sl JOIN FIN_SalesInvoice si ON si.InvoiceID=sl.InvoiceID LEFT JOIN PP_Products p ON p.ProductID=sl.ProductID WHERE sl.LineType='PRODUCT'" + DF("si", df, dt) + " GROUP BY IFNULL(p.ProductName,sl.TallyProductName) ORDER BY TotalSales DESC LIMIT 20;", DP(df, dt).ToArray());
            return JArr(data, r => string.Format("{{\"name\":\"{0}\",\"sales\":{1},\"qty\":{2},\"invoices\":{3},\"customers\":{4}}}", EscJson(r["ProductName"].ToString()), D(Convert.ToDecimal(r["TotalSales"])), D(Convert.ToDecimal(r["TotalQty"])), r["InvoiceCount"], r["CustomerCount"]));
        }

        private string GetDistributors(string state, string df, string dt)
        {
            if (string.IsNullOrEmpty(state)) return "[]";
            string dfilt = DF("si", df, dt); var p = new List<MySqlParameter> { new MySqlParameter("?state", state) }; p.AddRange(DP(df, dt));
            var data = Q("SELECT c.CustomerID,c.CustomerName,c.CustomerType,c.City,c.PinCode, IFNULL(SUM(si.TotalValue),0) AS TotalSales, COUNT(DISTINCT si.InvoiceID) AS TotalOrders, MIN(si.InvoiceDate) AS FirstOrder, MAX(si.InvoiceDate) AS LastOrder, COUNT(DISTINCT DATE_FORMAT(si.InvoiceDate,'%Y-%m')) AS ActiveMonths FROM PK_Customers c LEFT JOIN FIN_SalesInvoice si ON si.CustomerID=c.CustomerID" + (string.IsNullOrEmpty(dfilt) ? "" : " AND 1=1" + dfilt) + " WHERE c.CustomerType IN ('DI','ST') AND c.State=?state AND c.IsActive=1 GROUP BY c.CustomerID,c.CustomerName,c.CustomerType,c.City,c.PinCode ORDER BY TotalSales DESC;", p.ToArray());
            return JArr(data, r => { int dsl = r["LastOrder"] != DBNull.Value ? (int)(DateTime.Today - Convert.ToDateTime(r["LastOrder"])).TotalDays : 999; return string.Format("{{\"id\":{0},\"name\":\"{1}\",\"type\":\"{2}\",\"city\":\"{3}\",\"pin\":\"{4}\",\"sales\":{5},\"orders\":{6},\"activeMonths\":{7},\"firstOrder\":\"{8}\",\"lastOrder\":\"{9}\",\"daysSinceLast\":{10}}}", r["CustomerID"], EscJson(r["CustomerName"].ToString()), r["CustomerType"], EscJson(r["City"] != DBNull.Value ? r["City"].ToString() : ""), r["PinCode"] != DBNull.Value ? r["PinCode"].ToString() : "", D(Convert.ToDecimal(r["TotalSales"])), r["TotalOrders"], r["ActiveMonths"], r["FirstOrder"] != DBNull.Value ? Convert.ToDateTime(r["FirstOrder"]).ToString("yyyy-MM-dd") : "", r["LastOrder"] != DBNull.Value ? Convert.ToDateTime(r["LastOrder"]).ToString("yyyy-MM-dd") : "", dsl); });
        }

        private string GetDistributorDetail(int cid, string df, string dt)
        {
            if (cid <= 0) return "{\"monthly\":[],\"products\":[]}";
            string dfilt = DF("si", df, dt);
            var mp = new List<MySqlParameter> { new MySqlParameter("?cid", cid) }; mp.AddRange(DP(df, dt));
            var monthly = Q("SELECT DATE_FORMAT(si.InvoiceDate,'%Y-%m') AS Month, SUM(si.TotalValue) AS SalesValue, COUNT(DISTINCT si.InvoiceID) AS OrderCount FROM FIN_SalesInvoice si WHERE si.CustomerID=?cid" + dfilt + " GROUP BY Month ORDER BY Month;", mp.ToArray());
            var pp = new List<MySqlParameter> { new MySqlParameter("?cid", cid) }; pp.AddRange(DP(df, dt));
            var products = Q("SELECT IFNULL(p.ProductName,sl.TallyProductName) AS ProductName, SUM(sl.Value) AS TotalSales, SUM(sl.Quantity) AS TotalQty, COUNT(DISTINCT si.InvoiceID) AS OrderCount FROM FIN_SalesInvoiceLine sl JOIN FIN_SalesInvoice si ON si.InvoiceID=sl.InvoiceID LEFT JOIN PP_Products p ON p.ProductID=sl.ProductID WHERE si.CustomerID=?cid AND sl.LineType='PRODUCT'" + dfilt + " GROUP BY IFNULL(p.ProductName,sl.TallyProductName) ORDER BY TotalSales DESC;", pp.ToArray());
            var sb = new StringBuilder("{\"monthly\":"); sb.Append(JArr(monthly, r => string.Format("{{\"month\":\"{0}\",\"sales\":{1},\"orders\":{2}}}", r["Month"], D(Convert.ToDecimal(r["SalesValue"])), r["OrderCount"]))); sb.Append(",\"products\":"); sb.Append(JArr(products, r => string.Format("{{\"name\":\"{0}\",\"sales\":{1},\"qty\":{2},\"orders\":{3}}}", EscJson(r["ProductName"].ToString()), D(Convert.ToDecimal(r["TotalSales"])), D(Convert.ToDecimal(r["TotalQty"])), r["OrderCount"]))); sb.Append("}"); return sb.ToString();
        }

        private string GetAlerts()
        {
            var data = Q("SELECT c.CustomerName,c.City,c.State,MAX(si.InvoiceDate) AS LastOrder,DATEDIFF(CURDATE(),MAX(si.InvoiceDate)) AS DaysSilent,SUM(si.TotalValue) AS TotalSales FROM PK_Customers c JOIN FIN_SalesInvoice si ON si.CustomerID=c.CustomerID WHERE c.CustomerType IN ('DI','ST') AND c.IsActive=1 GROUP BY c.CustomerID,c.CustomerName,c.City,c.State HAVING DaysSilent>45 ORDER BY TotalSales DESC LIMIT 20;");
            return "{\"silentDistributors\":" + JArr(data, r => string.Format("{{\"name\":\"{0}\",\"city\":\"{1}\",\"state\":\"{2}\",\"lastOrder\":\"{3:yyyy-MM-dd}\",\"daysSilent\":{4},\"totalSales\":{5}}}", EscJson(r["CustomerName"].ToString()), EscJson(r["City"] != DBNull.Value ? r["City"].ToString() : ""), EscJson(r["State"] != DBNull.Value ? r["State"].ToString() : ""), r["LastOrder"], r["DaysSilent"], D(Convert.ToDecimal(r["TotalSales"])))) + "}";
        }

        private string GetProductView(string state, string df, string dt)
        {
            string dfilt = DF("si", df, dt); string sf = !string.IsNullOrEmpty(state) && state != "ALL" ? " AND c.State=?state" : "";
            var p = new List<MySqlParameter>(); if (!string.IsNullOrEmpty(state) && state != "ALL") p.Add(new MySqlParameter("?state", state)); p.AddRange(DP(df, dt));
            var data = Q("SELECT IFNULL(p.ProductName,sl.TallyProductName) AS ProductName, DATE_FORMAT(si.InvoiceDate,'%Y-%m') AS Month, SUM(sl.Value) AS SalesValue, SUM(sl.Quantity) AS SalesQty FROM FIN_SalesInvoiceLine sl JOIN FIN_SalesInvoice si ON si.InvoiceID=sl.InvoiceID JOIN PK_Customers c ON c.CustomerID=si.CustomerID LEFT JOIN PP_Products p ON p.ProductID=sl.ProductID WHERE sl.LineType='PRODUCT'" + sf + dfilt + " GROUP BY IFNULL(p.ProductName,sl.TallyProductName), Month ORDER BY IFNULL(p.ProductName,sl.TallyProductName), Month;", p.ToArray());
            var p2 = new List<MySqlParameter>(); if (!string.IsNullOrEmpty(state) && state != "ALL") p2.Add(new MySqlParameter("?state", state)); p2.AddRange(DP(df, dt));
            var summ = Q("SELECT IFNULL(p.ProductName,sl.TallyProductName) AS ProductName, SUM(sl.Value) AS TotalSales, SUM(sl.Quantity) AS TotalQty, COUNT(DISTINCT si.InvoiceID) AS InvoiceCount, COUNT(DISTINCT si.CustomerID) AS CustomerCount FROM FIN_SalesInvoiceLine sl JOIN FIN_SalesInvoice si ON si.InvoiceID=sl.InvoiceID JOIN PK_Customers c ON c.CustomerID=si.CustomerID LEFT JOIN PP_Products p ON p.ProductID=sl.ProductID WHERE sl.LineType='PRODUCT'" + sf + dfilt + " GROUP BY IFNULL(p.ProductName,sl.TallyProductName) ORDER BY TotalSales DESC;", p2.ToArray());
            var grouped = GroupedJson(data, "ProductName", "Month", "SalesValue", "products");
            var sb = new StringBuilder(grouped.TrimEnd('}')); sb.Append(",\"summary\":"); sb.Append(JArr(summ, r => string.Format("{{\"name\":\"{0}\",\"sales\":{1},\"qty\":{2},\"invoices\":{3},\"customers\":{4}}}", EscJson(r["ProductName"].ToString()), D(Convert.ToDecimal(r["TotalSales"])), D(Convert.ToDecimal(r["TotalQty"])), r["InvoiceCount"], r["CustomerCount"]))); sb.Append("}"); return sb.ToString();
        }

        private string GetProductList()
        {
            var data = Q("SELECT DISTINCT IFNULL(p.ProductName,sl.TallyProductName) AS ProductName FROM FIN_SalesInvoiceLine sl LEFT JOIN PP_Products p ON p.ProductID=sl.ProductID WHERE sl.LineType='PRODUCT' ORDER BY ProductName;");
            return JArr(data, r => "\"" + EscJson(r["ProductName"].ToString()) + "\"");
        }

        // ═══════════════════════════════════════════════
        // NEW: DISTRIBUTOR VIEW (Tab 4)
        // ═══════════════════════════════════════════════

        private string GetDistributorView(string state, string df, string dt)
        {
            string dfilt = DF("si", df, dt);
            string sf = !string.IsNullOrEmpty(state) && state != "ALL" ? " AND c.State=?state" : "";
            var p = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(state) && state != "ALL") p.Add(new MySqlParameter("?state", state));
            p.AddRange(DP(df, dt));

            // 1. Monthly sales by distributor (top 30 by total sales)
            var monthlyData = Q(
                "SELECT c.CustomerID, c.CustomerName, c.CustomerType, c.City," +
                " DATE_FORMAT(si.InvoiceDate,'%Y-%m') AS Month," +
                " SUM(si.TotalValue) AS SalesValue," +
                " COUNT(DISTINCT si.InvoiceID) AS OrderCount" +
                " FROM FIN_SalesInvoice si" +
                " JOIN PK_Customers c ON c.CustomerID=si.CustomerID" +
                " WHERE c.CustomerType IN ('DI','ST') AND c.IsActive=1" + sf + dfilt +
                " GROUP BY c.CustomerID, c.CustomerName, c.CustomerType, c.City, Month" +
                " ORDER BY c.CustomerName, Month;", p.ToArray());

            // 2. Per-distributor summary + order gap analysis
            var p2 = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(state) && state != "ALL") p2.Add(new MySqlParameter("?state", state));
            p2.AddRange(DP(df, dt));

            var summaryData = Q(
                "SELECT c.CustomerID, c.CustomerName, c.CustomerType, c.City, c.State," +
                " COUNT(DISTINCT si.InvoiceID) AS TotalOrders," +
                " SUM(si.TotalValue) AS TotalSales," +
                " MIN(si.InvoiceDate) AS FirstOrder," +
                " MAX(si.InvoiceDate) AS LastOrder," +
                " COUNT(DISTINCT DATE_FORMAT(si.InvoiceDate,'%Y-%m')) AS ActiveMonths," +
                " DATEDIFF(MAX(si.InvoiceDate), MIN(si.InvoiceDate)) AS DaySpan" +
                " FROM FIN_SalesInvoice si" +
                " JOIN PK_Customers c ON c.CustomerID=si.CustomerID" +
                " WHERE c.CustomerType IN ('DI','ST') AND c.IsActive=1" + sf + dfilt +
                " GROUP BY c.CustomerID, c.CustomerName, c.CustomerType, c.City, c.State" +
                " ORDER BY TotalSales DESC;", p2.ToArray());

            // 3. Individual order dates per distributor (for gap calculation)
            var p3 = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(state) && state != "ALL") p3.Add(new MySqlParameter("?state", state));
            p3.AddRange(DP(df, dt));

            var orderDates = Q(
                "SELECT si.CustomerID, si.InvoiceDate" +
                " FROM FIN_SalesInvoice si" +
                " JOIN PK_Customers c ON c.CustomerID=si.CustomerID" +
                " WHERE c.CustomerType IN ('DI','ST') AND c.IsActive=1" + sf + dfilt +
                " GROUP BY si.CustomerID, si.InvoiceDate" +
                " ORDER BY si.CustomerID, si.InvoiceDate;", p3.ToArray());

            // Build order gap data per customer
            var gapsByCustomer = new Dictionary<int, List<int>>();
            int prevCustId = -1; DateTime prevDate = DateTime.MinValue;
            foreach (DataRow r in orderDates.Rows)
            {
                int cid = Convert.ToInt32(r["CustomerID"]);
                DateTime d = Convert.ToDateTime(r["InvoiceDate"]);
                if (!gapsByCustomer.ContainsKey(cid)) gapsByCustomer[cid] = new List<int>();
                if (cid == prevCustId && prevDate != DateTime.MinValue)
                {
                    int gap = (int)(d - prevDate).TotalDays;
                    if (gap > 0) gapsByCustomer[cid].Add(gap);
                }
                prevCustId = cid; prevDate = d;
            }

            // Build monthly pivot
            var grouped = GroupedJson(monthlyData, "CustomerName", "Month", "SalesValue", "distributors");

            // Build summary array with gap data
            var sb = new StringBuilder(grouped.TrimEnd('}'));
            sb.Append(",\"summary\":[");
            bool first = true;
            foreach (DataRow r in summaryData.Rows)
            {
                if (!first) sb.Append(",");
                int cid = Convert.ToInt32(r["CustomerID"]);
                int orders = Convert.ToInt32(r["TotalOrders"]);
                decimal sales = Convert.ToDecimal(r["TotalSales"]);
                int activeMonths = Convert.ToInt32(r["ActiveMonths"]);
                int daySpan = r["DaySpan"] != DBNull.Value ? Convert.ToInt32(r["DaySpan"]) : 0;
                string fo = r["FirstOrder"] != DBNull.Value ? Convert.ToDateTime(r["FirstOrder"]).ToString("yyyy-MM-dd") : "";
                string lo = r["LastOrder"] != DBNull.Value ? Convert.ToDateTime(r["LastOrder"]).ToString("yyyy-MM-dd") : "";
                int dsl = r["LastOrder"] != DBNull.Value ? (int)(DateTime.Today - Convert.ToDateTime(r["LastOrder"])).TotalDays : 999;

                double avgGap = 0; int minGap = 0, maxGap = 0;
                if (gapsByCustomer.ContainsKey(cid) && gapsByCustomer[cid].Count > 0)
                {
                    var gaps = gapsByCustomer[cid];
                    avgGap = gaps.Average();
                    minGap = gaps.Min();
                    maxGap = gaps.Max();
                }

                sb.AppendFormat("{{\"id\":{0},\"name\":\"{1}\",\"type\":\"{2}\",\"city\":\"{3}\",\"state\":\"{4}\",",
                    cid, EscJson(r["CustomerName"].ToString()), r["CustomerType"],
                    EscJson(r["City"] != DBNull.Value ? r["City"].ToString() : ""),
                    EscJson(r["State"] != DBNull.Value ? r["State"].ToString() : ""));
                sb.AppendFormat("\"orders\":{0},\"sales\":{1},\"activeMonths\":{2},\"daySpan\":{3},",
                    orders, D(sales), activeMonths, daySpan);
                sb.AppendFormat("\"firstOrder\":\"{0}\",\"lastOrder\":\"{1}\",\"daysSinceLast\":{2},",
                    fo, lo, dsl);
                sb.AppendFormat("\"avgGap\":{0},\"minGap\":{1},\"maxGap\":{2}}}",
                    avgGap.ToString("0.0", CultureInfo.InvariantCulture), minGap, maxGap);
                first = false;
            }

            // Regularity buckets
            sb.Append("],\"regularity\":{");
            int[] buckets = { 60, 90, 120, 180, 270, 360 };
            for (int b = 0; b < buckets.Length; b++)
            {
                if (b > 0) sb.Append(",");
                int threshold = buckets[b];
                var regular = new List<string>();
                foreach (DataRow r in summaryData.Rows)
                {
                    int cid = Convert.ToInt32(r["CustomerID"]);
                    if (gapsByCustomer.ContainsKey(cid) && gapsByCustomer[cid].Count >= 2)
                    {
                        double avg = gapsByCustomer[cid].Average();
                        if (avg <= threshold) regular.Add(EscJson(r["CustomerName"].ToString()));
                    }
                    else if (Convert.ToInt32(r["TotalOrders"]) >= 2)
                    {
                        int span = r["DaySpan"] != DBNull.Value ? Convert.ToInt32(r["DaySpan"]) : 0;
                        int ord = Convert.ToInt32(r["TotalOrders"]);
                        if (ord > 1 && span > 0 && (span / (ord - 1)) <= threshold)
                            regular.Add(EscJson(r["CustomerName"].ToString()));
                    }
                }
                sb.AppendFormat("\"d{0}\":{1}", threshold, regular.Count);
            }
            sb.Append("}}");
            return sb.ToString();
        }

        // Distributor orders filtered by selected products
        private string GetDistOrdersByProduct(string state, string df, string dt, string productsParam)
        {
            if (string.IsNullOrEmpty(productsParam)) return "{\"months\":[],\"distributors\":[]}";
            string dfilt = DF("si", df, dt);
            string sf = !string.IsNullOrEmpty(state) && state != "ALL" ? " AND c.State=?state" : "";

            // Build product IN clause
            var prodNames = productsParam.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (prodNames.Length == 0) return "{\"months\":[],\"distributors\":[]}";

            var p = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(state) && state != "ALL") p.Add(new MySqlParameter("?state", state));
            p.AddRange(DP(df, dt));

            var inParts = new List<string>();
            for (int i = 0; i < prodNames.Length; i++)
            {
                string pn = "?pn" + i;
                inParts.Add(pn);
                p.Add(new MySqlParameter(pn, prodNames[i]));
            }
            string inClause = " AND IFNULL(pp.ProductName,sl.TallyProductName) IN (" + string.Join(",", inParts) + ")";

            var data = Q(
                "SELECT c.CustomerName, DATE_FORMAT(si.InvoiceDate,'%Y-%m') AS Month," +
                " SUM(sl.Value) AS SalesValue, COUNT(DISTINCT si.InvoiceID) AS OrderCount" +
                " FROM FIN_SalesInvoiceLine sl" +
                " JOIN FIN_SalesInvoice si ON si.InvoiceID=sl.InvoiceID" +
                " JOIN PK_Customers c ON c.CustomerID=si.CustomerID" +
                " LEFT JOIN PP_Products pp ON pp.ProductID=sl.ProductID" +
                " WHERE c.CustomerType IN ('DI','ST') AND sl.LineType='PRODUCT'" + sf + dfilt + inClause +
                " GROUP BY c.CustomerName, Month ORDER BY c.CustomerName, Month;", p.ToArray());

            return GroupedJson(data, "CustomerName", "Month", "SalesValue", "distributors");
        }
    }
}
