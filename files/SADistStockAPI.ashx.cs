using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.SessionState;
using MySql.Data.MySqlClient;
using StockApp.DAL;

namespace StockApp
{
    public class SADistStockAPI : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            if (context.Session["UserID"] == null)
            {
                context.Response.StatusCode = 401;
                context.Response.Write("{\"error\":\"Not authenticated\"}");
                return;
            }

            string action = context.Request["action"] ?? "";
            try
            {
                string json;
                switch (action)
                {
                    case "distributors": json = GetDistributors(); break;
                    case "stockData":    json = GetStockData(context); break;
                    default:             json = "{\"error\":\"Unknown action\"}"; break;
                }
                context.Response.Write(json);
            }
            catch (Exception ex)
            {
                context.Response.Write("{\"error\":\"" + EscJson(ex.Message) + "\"}");
            }
        }

        /// <summary>
        /// Returns active distributors (DI/ST) from PK_Customers with State, City.
        /// Same source as StockEntry / Distributor Stock Position Entry.
        /// </summary>
        private string GetDistributors()
        {
            string sql = @"
                SELECT c.CustomerID, c.CustomerName, c.City, c.State
                FROM PK_Customers c
                WHERE c.CustomerType IN ('DI','ST')
                  AND c.IsActive = 1
                  AND c.State IS NOT NULL AND c.State != ''
                  AND c.City IS NOT NULL AND c.City != ''
                ORDER BY c.State, c.City, c.CustomerName";

            DataTable dt = DatabaseHelper.ExecuteQueryPublic(sql);
            var sb = new StringBuilder("[");
            bool first = true;
            foreach (DataRow r in dt.Rows)
            {
                if (!first) sb.Append(",");
                sb.AppendFormat(
                    "{{\"id\":{0},\"name\":\"{1}\",\"city\":\"{2}\",\"state\":\"{3}\"}}",
                    r["CustomerID"],
                    EscJson(r["CustomerName"].ToString()),
                    EscJson(r["City"].ToString()),
                    EscJson(r["State"].ToString())
                );
                first = false;
            }
            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// Returns 90-day data per distributor per day from:
        ///   Stock Sent  -> FIN_SalesInvoice  (TotalQty per CustomerID per InvoiceDate)
        ///   Payment     -> FIN_Receipt       (Amount per CustomerID per ReceiptDate, ReceiptType='CUSTOMER')
        ///   Closing     -> StockPositions    (CurrentStock per DistributorID per EntryDate)
        /// </summary>
        private string GetStockData(HttpContext context)
        {
            int days = 90;
            int.TryParse(context.Request["days"] ?? "90", out days);
            if (days < 1 || days > 365) days = 90;

            DateTime dateTo = DateTime.Today;
            DateTime dateFrom = dateTo.AddDays(-(days - 1));
            string dfStr = dateFrom.ToString("yyyy-MM-dd");
            string dtStr = dateTo.ToString("yyyy-MM-dd");

            // 1) Stock Sent — from FIN_SalesInvoice (units shipped per customer per day)
            string sqlSent = @"
                SELECT si.CustomerID,
                       DATE(si.InvoiceDate) AS ActivityDate,
                       CAST(SUM(si.TotalQty) AS SIGNED) AS Units
                FROM FIN_SalesInvoice si
                WHERE si.CustomerID IS NOT NULL
                  AND si.InvoiceDate BETWEEN ?df AND ?dt
                GROUP BY si.CustomerID, DATE(si.InvoiceDate)";

            DataTable dtSent = DatabaseHelper.ExecuteQueryPublic(sqlSent,
                new MySqlParameter("?df", dfStr),
                new MySqlParameter("?dt", dtStr));

            // 2) Payments — from FIN_Receipt (customer receipts per day)
            string sqlPay = @"
                SELECT r.CustomerID,
                       DATE(r.ReceiptDate) AS ActivityDate,
                       SUM(r.Amount) AS Amount
                FROM FIN_Receipt r
                WHERE r.CustomerID IS NOT NULL
                  AND r.ReceiptType = 'CUSTOMER'
                  AND r.ReceiptDate BETWEEN ?df AND ?dt
                GROUP BY r.CustomerID, DATE(r.ReceiptDate)";

            DataTable dtPay = DatabaseHelper.ExecuteQueryPublic(sqlPay,
                new MySqlParameter("?df", dfStr),
                new MySqlParameter("?dt", dtStr));

            // 3) Closing Stock — from StockPositions (DistributorID = CustomerID)
            string sqlClose = @"
                SELECT sp.DistributorID AS CustomerID,
                       DATE(sp.EntryDate) AS ActivityDate,
                       sp.CurrentStock AS Units
                FROM StockPositions sp
                WHERE sp.EntryDate BETWEEN ?df AND ?dt";

            DataTable dtClose = DatabaseHelper.ExecuteQueryPublic(sqlClose,
                new MySqlParameter("?df", dfStr),
                new MySqlParameter("?dt", dtStr));

            // Build nested dictionary: customerId -> "yyyy-MM-dd" -> {sent, close}
            var data = new Dictionary<int, Dictionary<string, int[]>>();
            var payData = new Dictionary<int, Dictionary<string, decimal>>();

            // Sent data
            foreach (DataRow r in dtSent.Rows)
            {
                int cid = Convert.ToInt32(r["CustomerID"]);
                string dt2 = Convert.ToDateTime(r["ActivityDate"]).ToString("yyyy-MM-dd");
                int units = Convert.ToInt32(r["Units"]);
                if (!data.ContainsKey(cid)) data[cid] = new Dictionary<string, int[]>();
                if (!data[cid].ContainsKey(dt2)) data[cid][dt2] = new int[3];
                data[cid][dt2][0] = units;
            }

            // Payment data
            foreach (DataRow r in dtPay.Rows)
            {
                int cid = Convert.ToInt32(r["CustomerID"]);
                string dt2 = Convert.ToDateTime(r["ActivityDate"]).ToString("yyyy-MM-dd");
                decimal amt = Convert.ToDecimal(r["Amount"]);
                if (!payData.ContainsKey(cid)) payData[cid] = new Dictionary<string, decimal>();
                payData[cid][dt2] = amt;

                if (!data.ContainsKey(cid)) data[cid] = new Dictionary<string, int[]>();
                if (!data[cid].ContainsKey(dt2)) data[cid][dt2] = new int[3];
            }

            // Closing stock data
            foreach (DataRow r in dtClose.Rows)
            {
                int cid = Convert.ToInt32(r["CustomerID"]);
                string dt2 = Convert.ToDateTime(r["ActivityDate"]).ToString("yyyy-MM-dd");
                int units = Convert.ToInt32(r["Units"]);
                if (!data.ContainsKey(cid)) data[cid] = new Dictionary<string, int[]>();
                if (!data[cid].ContainsKey(dt2)) data[cid][dt2] = new int[3];
                data[cid][dt2][2] = units;
            }

            // Build JSON: { "custId": { "2026-04-01": { "s":50, "p":25000, "c":120 }, ... }, ... }
            var sb = new StringBuilder("{");
            bool firstDist = true;
            foreach (var kv in data)
            {
                if (!firstDist) sb.Append(",");
                sb.AppendFormat("\"{0}\":{{", kv.Key);
                bool firstDate = true;
                foreach (var dkv in kv.Value)
                {
                    int sent = dkv.Value[0];
                    decimal pay = 0;
                    if (payData.ContainsKey(kv.Key) && payData[kv.Key].ContainsKey(dkv.Key))
                        pay = payData[kv.Key][dkv.Key];
                    int close = dkv.Value[2];

                    // Skip dates with zero across all three
                    if (sent == 0 && pay == 0 && close == 0) continue;

                    if (!firstDate) sb.Append(",");
                    sb.AppendFormat("\"{0}\":{{", dkv.Key);
                    var parts = new List<string>();
                    if (sent > 0) parts.Add("\"s\":" + sent);
                    if (pay > 0) parts.Add("\"p\":" + pay.ToString("0.##", CultureInfo.InvariantCulture));
                    if (close > 0) parts.Add("\"c\":" + close);
                    sb.Append(string.Join(",", parts));
                    sb.Append("}");
                    firstDate = false;
                }
                sb.Append("}");
                firstDist = false;
            }
            sb.Append("}");
            return sb.ToString();
        }

        private string EscJson(string s) =>
            (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", "");
    }
}
