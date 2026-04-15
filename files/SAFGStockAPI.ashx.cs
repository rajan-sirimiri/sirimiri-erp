using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.SessionState;
using MySql.Data.MySqlClient;
using StockApp.DAL;

namespace StockApp
{
    public class SAFGStockAPI : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            if (context.Session["UserID"] == null)
            {
                context.Response.StatusCode = 401;
                context.Response.Write("Not authenticated");
                return;
            }

            string action = context.Request["action"] ?? "";
            try
            {
                switch (action)
                {
                    case "data":
                        context.Response.ContentType = "application/json";
                        context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                        context.Response.Write(GetFGStockJSON());
                        break;
                    case "pdf":
                        ServePDF(context);
                        break;
                    case "fgOpeningStock":
                        context.Response.ContentType = "application/json";
                        context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                        context.Response.Write(GetFGOpeningStockJSON());
                        break;
                    case "saveFGOpening":
                        context.Response.ContentType = "application/json";
                        context.Response.Write(SaveFGOpeningStock(context));
                        break;
                    default:
                        context.Response.ContentType = "application/json";
                        context.Response.Write("{\"error\":\"Unknown action\"}");
                        break;
                }
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.Write("{\"error\":\"" + Esc(ex.Message) + "\"}");
            }
        }

        /// <summary>
        /// FG Stock = Cases packed (SecondaryPacking, CASE) minus Cases dispatched (DCLines via finalised DCs)
        /// Only products that have at least one CASE packing record are shown.
        /// </summary>
        private DataTable GetFGStockData()
        {
            // FG = Opening Stock + Production − Dispatched
            // Loose JARs = Opening JAR/BOX + (Primary packed JARs − JARs in cases) − Loose in DCs
            // FG Cases = Opening CASE + Cases packed − Cases dispatched (FINALISED)
            // Reserved = DRAFT DC cases
            // Available for DC = FG Cases − Reserved
            string sql = @"
                SELECT p.ProductCode, p.ProductName, p.ContainerType,
                       IFNULL(p.ContainersPerCase, 12) AS ContainersPerCase,
                       CAST(SUBSTRING_INDEX(IFNULL(p.UnitsPerContainer,'1'),',',1) AS UNSIGNED) AS UnitSize,

                       FLOOR(IFNULL(fg.TotalPCS, 0)
                         / GREATEST(CAST(SUBSTRING_INDEX(IFNULL(p.UnitsPerContainer,'1'),',',1) AS UNSIGNED), 1)
                       ) AS TotalJarsPacked,

                       ROUND(IFNULL(sp.JarsUsedInCases, 0), 0) AS JarsUsedInCases,

                       IFNULL(osJar.OpeningJars, 0) AS OpeningJars,
                       IFNULL(osCase.OpeningCases, 0) AS OpeningCases,

                       IFNULL(osJar.OpeningJars, 0)
                         + FLOOR(IFNULL(fg.TotalPCS, 0)
                           / GREATEST(CAST(SUBSTRING_INDEX(IFNULL(p.UnitsPerContainer,'1'),',',1) AS UNSIGNED), 1))
                         - IFNULL(sp.JarsUsedInCases, 0)
                         AS FGLooseJars,

                       ROUND(IFNULL(sp.CasesPacked, 0), 0) AS CasesPacked,
                       IFNULL(dcFinal.CasesDispatched, 0) AS CasesDispatched,

                       IFNULL(osCase.OpeningCases, 0)
                         + IFNULL(sp.CasesPacked, 0)
                         - IFNULL(dcFinal.CasesDispatched, 0)
                         AS FGCases,

                       IFNULL(dcDraft.CasesReserved, 0) AS CasesReserved,

                       IFNULL(osCase.OpeningCases, 0)
                         + IFNULL(sp.CasesPacked, 0)
                         - IFNULL(dcFinal.CasesDispatched, 0)
                         - IFNULL(dcDraft.CasesReserved, 0)
                         AS AvailableForDC

                FROM PP_Products p

                LEFT JOIN (
                    SELECT ProductID, SUM(QtyPacked) AS TotalPCS
                    FROM PK_FGStock GROUP BY ProductID
                ) fg ON fg.ProductID = p.ProductID

                LEFT JOIN (
                    SELECT ProductID,
                      SUM(TotalUnits) AS JarsUsedInCases,
                      SUM(CASE WHEN PackingType='CASE' THEN QtyCartons ELSE 0 END) AS CasesPacked
                    FROM PK_SecondaryPacking GROUP BY ProductID
                ) sp ON sp.ProductID = p.ProductID

                LEFT JOIN (
                    SELECT ProductID, SUM(Quantity) AS OpeningJars
                    FROM PK_FGOpeningStock WHERE StockForm IN ('JAR','BOX')
                    GROUP BY ProductID
                ) osJar ON osJar.ProductID = p.ProductID

                LEFT JOIN (
                    SELECT ProductID, SUM(Quantity) AS OpeningCases
                    FROM PK_FGOpeningStock WHERE StockForm = 'CASE'
                    GROUP BY ProductID
                ) osCase ON osCase.ProductID = p.ProductID

                LEFT JOIN (
                    SELECT dl.ProductID, SUM(dl.Cases) AS CasesDispatched
                    FROM PK_DCLines dl
                    JOIN PK_DeliveryChallans dc ON dc.DCID = dl.DCID
                    WHERE dc.Status = 'FINALISED'
                    GROUP BY dl.ProductID
                ) dcFinal ON dcFinal.ProductID = p.ProductID

                LEFT JOIN (
                    SELECT dl.ProductID, SUM(dl.Cases) AS CasesReserved
                    FROM PK_DCLines dl
                    JOIN PK_DeliveryChallans dc ON dc.DCID = dl.DCID
                    WHERE dc.Status = 'DRAFT'
                    GROUP BY dl.ProductID
                ) dcDraft ON dcDraft.ProductID = p.ProductID

                WHERE p.ProductType = 'Core' AND p.IsActive = 1
                  AND p.ProductCode != 'FG-RETIRED'
                  AND p.ProductName NOT LIKE 'ZOHO %'
                  AND (IFNULL(fg.TotalPCS, 0) > 0 OR IFNULL(sp.CasesPacked, 0) > 0
                       OR IFNULL(osJar.OpeningJars, 0) > 0 OR IFNULL(osCase.OpeningCases, 0) > 0)
                ORDER BY p.ProductName";

            return DatabaseHelper.ExecuteQueryPublic(sql);
        }

        private string GetFGStockJSON()
        {
            DataTable dt = GetFGStockData();
            var sb = new StringBuilder("[");
            bool first = true;
            foreach (DataRow r in dt.Rows)
            {
                if (!first) sb.Append(",");
                sb.AppendFormat(
                    "{{\"code\":\"{0}\",\"name\":\"{1}\",\"ct\":\"{2}\",\"cpc\":{3}," +
                    "\"looseJars\":{4},\"casesPacked\":{5},\"dispatched\":{6}," +
                    "\"fgCases\":{7},\"reserved\":{8},\"availDC\":{9}}}",
                    Esc(r["ProductCode"].ToString()),
                    Esc(r["ProductName"].ToString()),
                    Esc(r["ContainerType"] != DBNull.Value ? r["ContainerType"].ToString() : ""),
                    r["ContainersPerCase"] != DBNull.Value ? Convert.ToInt32(r["ContainersPerCase"]) : 0,
                    Convert.ToInt32(r["FGLooseJars"]),
                    Convert.ToInt32(r["CasesPacked"]),
                    Convert.ToInt32(r["CasesDispatched"]),
                    Convert.ToInt32(r["FGCases"]),
                    Convert.ToInt32(r["CasesReserved"]),
                    Convert.ToInt32(r["AvailableForDC"]));
                first = false;
            }
            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// Serves an HTML page as PDF download using browser print.
        /// Simpler and more reliable than server-side PDF gen on .NET 4.8.
        /// </summary>
        private void ServePDF(HttpContext context)
        {
            DataTable dt = GetFGStockData();
            string dateStr = DateTime.Now.ToString("dd MMM yyyy, hh:mm tt");

            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><head><meta charset='utf-8'/>");
            sb.Append("<title>FG Stock Level</title>");
            sb.Append("<style>");
            sb.Append("@page{size:A4 portrait;margin:15mm}");
            sb.Append("body{font-family:Arial,sans-serif;font-size:11px;color:#1a1a1a;margin:0;padding:20px}");
            sb.Append(".hdr{display:flex;justify-content:space-between;align-items:center;margin-bottom:16px;border-bottom:2px solid #2980b9;padding-bottom:10px}");
            sb.Append(".hdr h1{font-size:18px;color:#2980b9;margin:0}");
            sb.Append(".hdr .dt{font-size:11px;color:#666}");
            sb.Append("table{width:100%;border-collapse:collapse;margin-top:8px}");
            sb.Append("th{background:#2980b9;color:#fff;font-size:10px;font-weight:600;text-transform:uppercase;letter-spacing:.06em;padding:8px 10px;text-align:left}");
            sb.Append("td{padding:7px 10px;border-bottom:1px solid #e0e0e0;font-size:11px}");
            sb.Append("tr:nth-child(even) td{background:#f8f9fb}");
            sb.Append(".num{text-align:right;font-family:'Courier New',monospace;font-weight:600}");
            sb.Append(".avail{color:#2980b9;font-size:13px}");
            sb.Append(".zero{color:#ccc}");
            sb.Append(".footer{margin-top:20px;font-size:9px;color:#999;text-align:center}");
            sb.Append("@media print{body{padding:0}.no-print{display:none}}");
            sb.Append("</style></head><body>");

            sb.Append("<div class='hdr'>");
            sb.Append("<h1>SIRIMIRI — Finished Goods Stock Level</h1>");
            sb.AppendFormat("<span class='dt'>Generated: {0}</span>", dateStr);
            sb.Append("</div>");

            sb.Append("<button class='no-print' onclick='window.print()' style='padding:8px 20px;background:#2980b9;color:#fff;border:none;border-radius:6px;font-size:13px;cursor:pointer;margin-bottom:12px'>Print / Save as PDF</button>");

            sb.Append("<table><thead><tr>");
            sb.Append("<th style='width:25px'>#</th>");
            sb.Append("<th style='width:65px'>Code</th>");
            sb.Append("<th>Product</th>");
            sb.Append("<th style='width:60px;text-align:right'>FG<br/>JARs</th>");
            sb.Append("<th style='width:55px;text-align:right'>Cases<br/>Packed</th>");
            sb.Append("<th style='width:60px;text-align:right'>Disp.</th>");
            sb.Append("<th style='width:55px;text-align:right'>FG<br/>Cases</th>");
            sb.Append("<th style='width:55px;text-align:right'>Rsrvd</th>");
            sb.Append("<th style='width:60px;text-align:right'>Avail<br/>for DC</th>");
            sb.Append("</tr></thead><tbody>");

            int row = 0;
            foreach (DataRow r in dt.Rows)
            {
                row++;
                int looseJars = Convert.ToInt32(r["FGLooseJars"]);
                int casesPacked = Convert.ToInt32(r["CasesPacked"]);
                int dispatched = Convert.ToInt32(r["CasesDispatched"]);
                int fgCases = Convert.ToInt32(r["FGCases"]);
                int reserved = Convert.ToInt32(r["CasesReserved"]);
                int availDC = Convert.ToInt32(r["AvailableForDC"]);

                sb.AppendFormat("<tr><td>{0}</td>", row);
                sb.AppendFormat("<td>{0}</td>", r["ProductCode"]);
                sb.AppendFormat("<td>{0}</td>", r["ProductName"]);
                sb.AppendFormat("<td class='num' style='color:#2980b9;font-weight:700'>{0}</td>", looseJars > 0 ? looseJars.ToString("N0") : "<span class='zero'>0</span>");
                sb.AppendFormat("<td class='num'>{0}</td>", casesPacked > 0 ? casesPacked.ToString("N0") : "<span class='zero'>—</span>");
                sb.AppendFormat("<td class='num'>{0}</td>", dispatched > 0 ? dispatched.ToString("N0") : "<span class='zero'>—</span>");
                sb.AppendFormat("<td class='num' style='color:#2980b9;font-weight:700'>{0}</td>", fgCases > 0 ? fgCases.ToString("N0") : "<span class='zero'>0</span>");
                sb.AppendFormat("<td class='num' style='color:#e67e22'>{0}</td>", reserved > 0 ? reserved.ToString("N0") : "<span class='zero'>—</span>");
                sb.AppendFormat("<td class='num avail'>{0}</td>", availDC > 0 ? availDC.ToString("N0") : (availDC < 0 ? "<span style='color:#e74c3c'>" + availDC.ToString("N0") + "</span>" : "<span class='zero'>0</span>"));
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table>");
            sb.AppendFormat("<div class='footer'>Sirimiri Nutrition Food Products Pvt. Ltd. — FG Stock Report — {0}</div>", dateStr);
            sb.Append("<script>setTimeout(function(){window.print();},500);</script>");
            sb.Append("</body></html>");

            context.Response.ContentType = "text/html";
            context.Response.Write(sb.ToString());
        }

        private string Esc(string s) =>
            (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", "");

        // ── FG OPENING STOCK ─────────────────────────────────────────────

        private string GetFGOpeningStockJSON()
        {
            // Products
            DataTable prods = DatabaseHelper.ExecuteQueryPublic(
                "SELECT p.ProductID, p.ProductCode, p.ProductName," +
                " IFNULL(p.ContainerType,'JAR') AS ContainerType," +
                " IFNULL(p.ContainersPerCase,12) AS ContainersPerCase" +
                " FROM PP_Products p" +
                " WHERE p.ProductType='Core' AND p.IsActive=1" +
                " AND p.ProductCode != 'FG-RETIRED'" +
                " AND p.ProductName NOT LIKE 'ZOHO %'" +
                " ORDER BY p.ProductName");

            // Existing opening stock
            DataTable opening = DatabaseHelper.ExecuteQueryPublic(
                "SELECT ProductID, StockForm, Quantity FROM PK_FGOpeningStock");

            // Build products JSON
            var sb = new StringBuilder("{\"products\":[");
            bool first = true;
            foreach (DataRow r in prods.Rows)
            {
                if (!first) sb.Append(",");
                sb.AppendFormat("{{\"id\":{0},\"code\":\"{1}\",\"name\":\"{2}\",\"ct\":\"{3}\",\"cpc\":{4}}}",
                    r["ProductID"],
                    Esc(r["ProductCode"].ToString()),
                    Esc(r["ProductName"].ToString()),
                    Esc(r["ContainerType"].ToString()),
                    Convert.ToInt32(r["ContainersPerCase"]));
                first = false;
            }
            sb.Append("],\"opening\":{");

            // Build opening stock map: {pid: {jar:X, cs:Y}}
            var openMap = new System.Collections.Generic.Dictionary<int, int[]>();
            foreach (DataRow r in opening.Rows)
            {
                int pid = Convert.ToInt32(r["ProductID"]);
                string form = r["StockForm"].ToString();
                int qty = Convert.ToInt32(r["Quantity"]);
                if (!openMap.ContainsKey(pid)) openMap[pid] = new int[2]; // [0]=jar, [1]=case
                if (form == "CASE") openMap[pid][1] = qty;
                else openMap[pid][0] = qty; // JAR or BOX
            }
            first = true;
            foreach (var kv in openMap)
            {
                if (!first) sb.Append(",");
                sb.AppendFormat("\"{0}\":{{\"jar\":{1},\"cs\":{2}}}", kv.Key, kv.Value[0], kv.Value[1]);
                first = false;
            }
            sb.Append("}}");
            return sb.ToString();
        }

        private string SaveFGOpeningStock(HttpContext context)
        {
            string raw = context.Request["data"] ?? "";
            if (string.IsNullOrEmpty(raw)) return "{\"error\":\"No data\"}";

            int userId = Convert.ToInt32(context.Session["UserID"]);
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            int updated = 0;

            string[] pairs = raw.Split(';');
            foreach (string pair in pairs)
            {
                // format: productId:jars:cases
                string[] parts = pair.Split(':');
                if (parts.Length != 3) continue;
                int pid = 0, jars = 0, cases = 0;
                if (!int.TryParse(parts[0], out pid) || pid <= 0) continue;
                int.TryParse(parts[1], out jars);
                int.TryParse(parts[2], out cases);

                // Upsert JAR/BOX row
                string containerType = "JAR";
                try
                {
                    object ct = DatabaseHelper.ExecuteScalarPublic(
                        "SELECT IFNULL(ContainerType,'JAR') FROM PP_Products WHERE ProductID=" + pid);
                    if (ct != null) containerType = ct.ToString();
                }
                catch { }
                string jarForm = (containerType == "BOX") ? "BOX" : "JAR";

                DatabaseHelper.ExecuteNonQueryPublic(
                    "INSERT INTO PK_FGOpeningStock (ProductID, StockForm, Quantity, AsOfDate, CreatedBy)" +
                    " VALUES(?pid, ?form, ?qty, ?dt, ?by)" +
                    " ON DUPLICATE KEY UPDATE Quantity=?qty, UpdatedAt=NOW()",
                    new MySqlParameter("?pid", pid),
                    new MySqlParameter("?form", jarForm),
                    new MySqlParameter("?qty", jars),
                    new MySqlParameter("?dt", today),
                    new MySqlParameter("?by", userId));

                // Upsert CASE row
                DatabaseHelper.ExecuteNonQueryPublic(
                    "INSERT INTO PK_FGOpeningStock (ProductID, StockForm, Quantity, AsOfDate, CreatedBy)" +
                    " VALUES(?pid, 'CASE', ?qty, ?dt, ?by)" +
                    " ON DUPLICATE KEY UPDATE Quantity=?qty, UpdatedAt=NOW()",
                    new MySqlParameter("?pid", pid),
                    new MySqlParameter("?qty", cases),
                    new MySqlParameter("?dt", today),
                    new MySqlParameter("?by", userId));

                updated++;
            }

            return "{\"ok\":true,\"updated\":" + updated + "}";
        }
    }
}
