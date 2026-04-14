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
    public class SADailySalesAPI : IHttpHandler, IRequiresSessionState
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
                    case "products":  json = GetProductsWithPackOptions(); break;
                    case "save":      json = SaveEntries(context); break;
                    default:          json = "{\"error\":\"Unknown action\"}"; break;
                }
                context.Response.Write(json);
            }
            catch (Exception ex)
            {
                context.Response.Write("{\"error\":\"" + EscJson(ex.Message) + "\"}");
            }
        }

        /// <summary>
        /// Returns Core active PP_Products with their FG packing options.
        /// </summary>
        private string GetProductsWithPackOptions()
        {
            // Products
            string sqlProd = @"
                SELECT p.ProductID, p.ProductCode, p.ProductName,
                       p.UnitWeightGrams, p.ContainerType, p.HSNCode
                FROM PP_Products p
                WHERE p.ProductType='Core' AND p.IsActive=1
                ORDER BY p.ProductName";
            DataTable dtProd = DatabaseHelper.ExecuteQueryPublic(sqlProd);

            // All packing options for active products
            string sqlPack = @"
                SELECT fo.ProductID, fo.PackForm, fo.UnitsPerPack, fo.Description
                FROM PP_FGPackingOptions fo
                INNER JOIN PP_Products p ON p.ProductID = fo.ProductID
                WHERE p.ProductType='Core' AND p.IsActive=1 AND fo.IsActive=1
                ORDER BY fo.ProductID, FIELD(fo.PackForm,'PCS','JAR','BOX','CASE'), fo.UnitsPerPack";
            DataTable dtPack = DatabaseHelper.ExecuteQueryPublic(sqlPack);

            // Group packing options by ProductID
            var packMap = new Dictionary<int, List<DataRow>>();
            foreach (DataRow r in dtPack.Rows)
            {
                int pid = Convert.ToInt32(r["ProductID"]);
                if (!packMap.ContainsKey(pid)) packMap[pid] = new List<DataRow>();
                packMap[pid].Add(r);
            }

            var sb = new StringBuilder("[");
            bool first = true;
            foreach (DataRow r in dtProd.Rows)
            {
                if (!first) sb.Append(",");
                int pid = Convert.ToInt32(r["ProductID"]);
                decimal wt = r["UnitWeightGrams"] != DBNull.Value ? Convert.ToDecimal(r["UnitWeightGrams"]) : 0;
                string ct = r["ContainerType"] != DBNull.Value ? r["ContainerType"].ToString() : "";
                string hsn = r["HSNCode"] != DBNull.Value ? r["HSNCode"].ToString() : "";

                sb.AppendFormat("{{\"id\":{0},\"code\":\"{1}\",\"name\":\"{2}\",\"wt\":{3},\"ct\":\"{4}\",\"hsn\":\"{5}\",\"packs\":[",
                    pid,
                    EscJson(r["ProductCode"].ToString()),
                    EscJson(r["ProductName"].ToString()),
                    wt.ToString("0.##", CultureInfo.InvariantCulture),
                    EscJson(ct),
                    EscJson(hsn));

                if (packMap.ContainsKey(pid))
                {
                    bool pFirst = true;
                    foreach (DataRow pr in packMap[pid])
                    {
                        if (!pFirst) sb.Append(",");
                        sb.AppendFormat("{{\"form\":\"{0}\",\"units\":{1},\"desc\":\"{2}\"}}",
                            EscJson(pr["PackForm"].ToString()),
                            Convert.ToInt32(pr["UnitsPerPack"]),
                            EscJson(pr["Description"] != DBNull.Value ? pr["Description"].ToString() : ""));
                        pFirst = false;
                    }
                }

                sb.Append("]}");
                first = false;
            }
            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// Saves daily sales entries to SA_DailySalesEntries.
        /// POST: customerIds=1,2,3 & entries=[{pid,form,units,qty},...] & date=2026-04-14
        /// </summary>
        private string SaveEntries(HttpContext context)
        {
            int userId = Convert.ToInt32(context.Session["UserID"]);

            string dateStr = context.Request["date"] ?? DateTime.Today.ToString("yyyy-MM-dd");
            if (!DateTime.TryParse(dateStr, out DateTime entryDate))
                entryDate = DateTime.Today;

            string customerIdsStr = context.Request["customerIds"] ?? "";
            string entriesJson = context.Request["entries"] ?? "[]";

            if (string.IsNullOrEmpty(customerIdsStr))
                return "{\"error\":\"No distributors selected\"}";

            // Parse customer IDs
            var customerIds = new List<int>();
            foreach (var s in customerIdsStr.Split(','))
            {
                if (int.TryParse(s.Trim(), out int cid) && cid > 0)
                    customerIds.Add(cid);
            }
            if (customerIds.Count == 0)
                return "{\"error\":\"No valid distributors\"}";

            // Parse entries — simple manual JSON parse (no external lib needed)
            // Format: pid|form|unitsPerPack|qty;pid|form|unitsPerPack|qty;...
            string entriesStr = context.Request["entries"] ?? "";
            if (string.IsNullOrEmpty(entriesStr))
                return "{\"error\":\"No product entries\"}";

            var entries = new List<(int pid, string form, int unitsPer, int qty)>();
            foreach (var seg in entriesStr.Split(';'))
            {
                var parts = seg.Split('|');
                if (parts.Length != 4) continue;
                if (!int.TryParse(parts[0], out int pid)) continue;
                string form = parts[1];
                if (!int.TryParse(parts[2], out int unitsPer)) continue;
                if (!int.TryParse(parts[3], out int qty) || qty <= 0) continue;
                entries.Add((pid, form, unitsPer, qty));
            }

            if (entries.Count == 0)
                return "{\"error\":\"No quantities entered\"}";

            int n = customerIds.Count;
            int totalSaved = 0;

            foreach (var (pid, form, unitsPer, totalQty) in entries)
            {
                int totalUnits = totalQty * unitsPer;

                // Split across distributors
                int baseQty = totalQty / n;
                int remainder = totalQty % n;

                for (int i = 0; i < n; i++)
                {
                    int qty = baseQty + (i < remainder ? 1 : 0);
                    if (qty <= 0) continue;
                    int units = qty * unitsPer;

                    DatabaseHelper.ExecuteNonQueryPublic(
                        "INSERT INTO SA_DailySalesEntries (EntryDate, CustomerID, ProductID, PackForm, UnitsPerPack, Quantity, TotalUnits, SubmittedBy) " +
                        "VALUES (?dt, ?cid, ?pid, ?form, ?upp, ?qty, ?tu, ?uid);",
                        new MySqlParameter("?dt", entryDate),
                        new MySqlParameter("?cid", customerIds[i]),
                        new MySqlParameter("?pid", pid),
                        new MySqlParameter("?form", form),
                        new MySqlParameter("?upp", unitsPer),
                        new MySqlParameter("?qty", qty),
                        new MySqlParameter("?tu", units),
                        new MySqlParameter("?uid", userId));
                    totalSaved++;
                }
            }

            string userName = context.Session["FullName"]?.ToString() ?? "Unknown";
            return "{\"ok\":true,\"saved\":" + totalSaved + ",\"user\":\"" + EscJson(userName) + "\"}";
        }

        private string EscJson(string s) =>
            (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", "");
    }
}
