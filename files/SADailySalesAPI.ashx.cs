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
            { context.Response.StatusCode = 401; context.Response.Write("{\"error\":\"Not authenticated\"}"); return; }

            string action = context.Request["action"] ?? "";
            try
            {
                string json;
                switch (action)
                {
                    case "products":      json = GetProductsWithPackOptions(); break;
                    case "save":          json = SaveEntries(context); break;
                    case "saveDefaults":  json = SaveDefaults(context); break;
                    case "report":        json = GetReport(context); break;
                    default:              json = "{\"error\":\"Unknown action\"}"; break;
                }
                context.Response.Write(json);
            }
            catch (Exception ex)
            { context.Response.Write("{\"error\":\"" + Esc(ex.Message) + "\"}"); }
        }

        // ═══════════════════════════════════════════════
        // PRODUCTS + PACKING OPTIONS + DEFAULT PACK FORM
        // ═══════════════════════════════════════════════
        private string GetProductsWithPackOptions()
        {
            string sqlProd = @"
                SELECT p.ProductID, p.ProductCode, p.ProductName,
                       p.UnitWeightGrams, p.ContainerType, p.HSNCode,
                       IFNULL(dp.DefaultForm, 'PCS') AS DefaultForm
                FROM PP_Products p
                LEFT JOIN SA_ProductDefaultPack dp ON dp.ProductID = p.ProductID
                WHERE p.ProductType='Core' AND p.IsActive=1
                  AND p.ProductCode != 'FG-RETIRED'
                  AND p.ProductName NOT LIKE 'ZOHO %'
                ORDER BY p.ProductName";
            DataTable dtProd = DatabaseHelper.ExecuteQueryPublic(sqlProd);

            string sqlPack = @"
                SELECT fo.ProductID, fo.PackForm, fo.UnitsPerPack, fo.Description
                FROM PP_FGPackingOptions fo
                INNER JOIN PP_Products p ON p.ProductID = fo.ProductID
                WHERE p.ProductType='Core' AND p.IsActive=1 AND fo.IsActive=1
                  AND p.ProductCode != 'FG-RETIRED'
                  AND p.ProductName NOT LIKE 'ZOHO %'
                ORDER BY fo.ProductID, FIELD(fo.PackForm,'PCS','JAR','BOX','CASE'), fo.UnitsPerPack";
            DataTable dtPack = DatabaseHelper.ExecuteQueryPublic(sqlPack);

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
                string df = r["DefaultForm"].ToString();

                sb.AppendFormat("{{\"id\":{0},\"code\":\"{1}\",\"name\":\"{2}\",\"wt\":{3},\"ct\":\"{4}\",\"hsn\":\"{5}\",\"df\":\"{6}\",\"packs\":[",
                    pid, Esc(r["ProductCode"].ToString()), Esc(r["ProductName"].ToString()),
                    wt.ToString("0.##", CultureInfo.InvariantCulture), Esc(ct), Esc(hsn), Esc(df));

                if (packMap.ContainsKey(pid))
                {
                    bool pFirst = true;
                    foreach (DataRow pr in packMap[pid])
                    {
                        if (!pFirst) sb.Append(",");
                        sb.AppendFormat("{{\"form\":\"{0}\",\"units\":{1},\"desc\":\"{2}\"}}",
                            Esc(pr["PackForm"].ToString()),
                            Convert.ToInt32(pr["UnitsPerPack"]),
                            Esc(pr["Description"] != DBNull.Value ? pr["Description"].ToString() : ""));
                        pFirst = false;
                    }
                }
                sb.Append("]}");
                first = false;
            }
            sb.Append("]");
            return sb.ToString();
        }

        // ═══════════════════════════════════════════════
        // SAVE ENTRIES + VISIT LOG
        // POST: customerIds, entries (pid|form|units|qty;...), newShops, repeatShops, date
        // ═══════════════════════════════════════════════
        private string SaveEntries(HttpContext context)
        {
            int userId = Convert.ToInt32(context.Session["UserID"]);
            string dateStr = context.Request["date"] ?? DateTime.Today.ToString("yyyy-MM-dd");
            if (!DateTime.TryParse(dateStr, out DateTime entryDate)) entryDate = DateTime.Today;

            string customerIdsStr = context.Request["customerIds"] ?? "";
            string entriesStr = context.Request["entries"] ?? "";
            int.TryParse(context.Request["newShops"] ?? "0", out int newShops);
            int.TryParse(context.Request["repeatShops"] ?? "0", out int repeatShops);

            if (string.IsNullOrEmpty(customerIdsStr))
                return "{\"error\":\"No distributors selected\"}";

            var customerIds = new List<int>();
            foreach (var s in customerIdsStr.Split(','))
                if (int.TryParse(s.Trim(), out int cid) && cid > 0) customerIds.Add(cid);
            if (customerIds.Count == 0) return "{\"error\":\"No valid distributors\"}";

            // Parse product entries
            var entries = new List<(int pid, string form, int unitsPer, int qty)>();
            if (!string.IsNullOrEmpty(entriesStr))
            {
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
            }

            int n = customerIds.Count;
            int totalSaved = 0;

            // Save product entries
            foreach (var (pid, form, unitsPer, totalQty) in entries)
            {
                int baseQty = totalQty / n;
                int remainder = totalQty % n;
                for (int i = 0; i < n; i++)
                {
                    int qty = baseQty + (i < remainder ? 1 : 0);
                    if (qty <= 0) continue;
                    DatabaseHelper.ExecuteNonQueryPublic(
                        "INSERT INTO SA_DailySalesEntries (EntryDate,CustomerID,ProductID,PackForm,UnitsPerPack,Quantity,TotalUnits,SubmittedBy) " +
                        "VALUES (?dt,?cid,?pid,?form,?upp,?qty,?tu,?uid);",
                        new MySqlParameter("?dt", entryDate),
                        new MySqlParameter("?cid", customerIds[i]),
                        new MySqlParameter("?pid", pid),
                        new MySqlParameter("?form", form),
                        new MySqlParameter("?upp", unitsPer),
                        new MySqlParameter("?qty", qty),
                        new MySqlParameter("?tu", qty * unitsPer),
                        new MySqlParameter("?uid", userId));
                    totalSaved++;
                }
            }

            // Save visit log (new/repeat shops) per distributor
            if (newShops > 0 || repeatShops > 0)
            {
                foreach (int cid in customerIds)
                {
                    DatabaseHelper.ExecuteNonQueryPublic(
                        "INSERT INTO SA_DailyVisitLog (VisitDate,CustomerID,NewShops,RepeatShops,SubmittedBy) " +
                        "VALUES (?dt,?cid,?ns,?rs,?uid);",
                        new MySqlParameter("?dt", entryDate),
                        new MySqlParameter("?cid", cid),
                        new MySqlParameter("?ns", newShops),
                        new MySqlParameter("?rs", repeatShops),
                        new MySqlParameter("?uid", userId));
                }
            }

            string userName = context.Session["FullName"]?.ToString() ?? "Unknown";
            return "{\"ok\":true,\"saved\":" + totalSaved + ",\"user\":\"" + Esc(userName) + "\"}";
        }

        // ═══════════════════════════════════════════════
        // SAVE DEFAULT PACK FORMS
        // POST: defaults=pid:FORM;pid:FORM;...
        // ═══════════════════════════════════════════════
        private string SaveDefaults(HttpContext context)
        {
            string data = context.Request["defaults"] ?? "";
            if (string.IsNullOrEmpty(data)) return "{\"error\":\"No data\"}";

            int count = 0;
            foreach (var seg in data.Split(';'))
            {
                var parts = seg.Split(':');
                if (parts.Length != 2) continue;
                if (!int.TryParse(parts[0], out int pid)) continue;
                string form = parts[1].Trim().ToUpper();
                if (form != "PCS" && form != "JAR" && form != "BOX" && form != "CASE") continue;

                DatabaseHelper.ExecuteNonQueryPublic(
                    "INSERT INTO SA_ProductDefaultPack (ProductID, DefaultForm) VALUES (?pid, ?form) " +
                    "ON DUPLICATE KEY UPDATE DefaultForm=?form2;",
                    new MySqlParameter("?pid", pid),
                    new MySqlParameter("?form", form),
                    new MySqlParameter("?form2", form));
                count++;
            }
            return "{\"ok\":true,\"updated\":" + count + "}";
        }

        // ═══════════════════════════════════════════════
        // REPORT: Sales by User (SO/SR) for date range
        // GET: dateFrom, dateTo, userId (optional)
        // Returns: user summary + per-distributor product breakdown
        // ═══════════════════════════════════════════════
        private string GetReport(HttpContext context)
        {
            string df = context.Request["dateFrom"] ?? DateTime.Today.ToString("yyyy-MM-dd");
            string dt = context.Request["dateTo"] ?? df;
            string uidStr = context.Request["userId"] ?? "0";
            int.TryParse(uidStr, out int filterUserId);

            // User filter clause
            string userFilter = filterUserId > 0 ? " AND e.SubmittedBy=?uid" : "";

            // 1) User-level summary: total new shops, repeat shops, products sold
            string sqlUsers = @"
                SELECT u.UserID, u.FullName,
                       IFNULL(d.DesignName,'') AS Designation,
                       IFNULL(vs.TotalNew, 0) AS TotalNewShops,
                       IFNULL(vs.TotalRepeat, 0) AS TotalRepeatShops,
                       IFNULL(ss.TotalUnits, 0) AS TotalUnitsSold,
                       IFNULL(ss.DistCount, 0) AS DistributorCount
                FROM Users u
                LEFT JOIN SA_OrgPositions op ON op.UserID = u.UserID AND op.IsActive=1
                LEFT JOIN SA_Designations d ON d.DesignationID = op.DesignationID
                LEFT JOIN (
                    SELECT SubmittedBy, SUM(NewShops) AS TotalNew, SUM(RepeatShops) AS TotalRepeat
                    FROM SA_DailyVisitLog WHERE VisitDate BETWEEN ?df AND ?dt
                    GROUP BY SubmittedBy
                ) vs ON vs.SubmittedBy = u.UserID
                LEFT JOIN (
                    SELECT SubmittedBy, SUM(TotalUnits) AS TotalUnits, COUNT(DISTINCT CustomerID) AS DistCount
                    FROM SA_DailySalesEntries WHERE EntryDate BETWEEN ?df AND ?dt
                    GROUP BY SubmittedBy
                ) ss ON ss.SubmittedBy = u.UserID
                WHERE (vs.SubmittedBy IS NOT NULL OR ss.SubmittedBy IS NOT NULL)" +
                (filterUserId > 0 ? " AND u.UserID=?uid" : "") +
                " ORDER BY u.FullName";

            var prms = new List<MySqlParameter> {
                new MySqlParameter("?df", df),
                new MySqlParameter("?dt", dt)
            };
            if (filterUserId > 0) prms.Add(new MySqlParameter("?uid", filterUserId));

            DataTable dtUsers = DatabaseHelper.ExecuteQueryPublic(sqlUsers, prms.ToArray());

            // 2) Detail: per user → per distributor → per product
            string sqlDetail = @"
                SELECT e.SubmittedBy, c.CustomerName AS DistributorName,
                       p.ProductName, e.PackForm,
                       SUM(e.Quantity) AS TotalPacks, SUM(e.TotalUnits) AS TotalUnits
                FROM SA_DailySalesEntries e
                INNER JOIN PK_Customers c ON c.CustomerID = e.CustomerID
                INNER JOIN PP_Products p ON p.ProductID = e.ProductID
                WHERE e.EntryDate BETWEEN ?df AND ?dt" + userFilter + @"
                GROUP BY e.SubmittedBy, c.CustomerName, p.ProductName, e.PackForm
                ORDER BY e.SubmittedBy, c.CustomerName, p.ProductName";

            DataTable dtDetail = DatabaseHelper.ExecuteQueryPublic(sqlDetail, prms.ToArray());

            // Group detail by SubmittedBy
            var detailMap = new Dictionary<int, List<DataRow>>();
            foreach (DataRow r in dtDetail.Rows)
            {
                int uid = Convert.ToInt32(r["SubmittedBy"]);
                if (!detailMap.ContainsKey(uid)) detailMap[uid] = new List<DataRow>();
                detailMap[uid].Add(r);
            }

            // Build JSON
            var sb = new StringBuilder("{\"dateFrom\":\"" + Esc(df) + "\",\"dateTo\":\"" + Esc(dt) + "\",\"users\":[");
            bool firstU = true;
            foreach (DataRow u in dtUsers.Rows)
            {
                if (!firstU) sb.Append(",");
                int uid = Convert.ToInt32(u["UserID"]);
                sb.AppendFormat("{{\"id\":{0},\"name\":\"{1}\",\"desig\":\"{2}\",\"newShops\":{3},\"repeatShops\":{4},\"totalUnits\":{5},\"distCount\":{6},\"lines\":[",
                    uid, Esc(u["FullName"].ToString()), Esc(u["Designation"].ToString()),
                    u["TotalNewShops"], u["TotalRepeatShops"], u["TotalUnitsSold"], u["DistributorCount"]);

                if (detailMap.ContainsKey(uid))
                {
                    bool firstL = true;
                    foreach (DataRow d in detailMap[uid])
                    {
                        if (!firstL) sb.Append(",");
                        sb.AppendFormat("{{\"dist\":\"{0}\",\"product\":\"{1}\",\"form\":\"{2}\",\"packs\":{3},\"units\":{4}}}",
                            Esc(d["DistributorName"].ToString()), Esc(d["ProductName"].ToString()),
                            Esc(d["PackForm"].ToString()), d["TotalPacks"], d["TotalUnits"]);
                        firstL = false;
                    }
                }
                sb.Append("]}");
                firstU = false;
            }
            sb.Append("]}");
            return sb.ToString();
        }

        private string Esc(string s) =>
            (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", "");
    }
}
