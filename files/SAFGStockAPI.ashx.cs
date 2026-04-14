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
            string sql = @"
                SELECT p.ProductCode, p.ProductName, p.ContainerType,
                       p.ContainersPerCase,
                       IFNULL(packed.TotalCases, 0) AS CasesPacked,
                       IFNULL(dispatched.TotalCases, 0) AS CasesDispatched,
                       IFNULL(packed.TotalCases, 0) - IFNULL(dispatched.TotalCases, 0) AS AvailableCases
                FROM PP_Products p
                INNER JOIN (
                    SELECT ProductID, SUM(QtyCartons) AS TotalCases
                    FROM PK_SecondaryPacking
                    WHERE PackingType = 'CASE'
                    GROUP BY ProductID
                ) packed ON packed.ProductID = p.ProductID
                LEFT JOIN (
                    SELECT dl.ProductID, SUM(dl.Cases) AS TotalCases
                    FROM PK_DCLines dl
                    INNER JOIN PK_DeliveryChallans dc ON dc.DCID = dl.DCID
                    WHERE dc.Status IN ('DRAFT','FINALISED')
                    GROUP BY dl.ProductID
                ) dispatched ON dispatched.ProductID = p.ProductID
                WHERE p.ProductType = 'Core' AND p.IsActive = 1
                  AND p.ProductCode != 'FG-RETIRED'
                  AND p.ProductName NOT LIKE 'ZOHO %'
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
                int avail = Convert.ToInt32(r["AvailableCases"]);
                sb.AppendFormat(
                    "{{\"code\":\"{0}\",\"name\":\"{1}\",\"ct\":\"{2}\",\"cpc\":{3},\"packed\":{4},\"dispatched\":{5},\"available\":{6}}}",
                    Esc(r["ProductCode"].ToString()),
                    Esc(r["ProductName"].ToString()),
                    Esc(r["ContainerType"] != DBNull.Value ? r["ContainerType"].ToString() : ""),
                    r["ContainersPerCase"] != DBNull.Value ? Convert.ToInt32(r["ContainersPerCase"]) : 0,
                    Convert.ToInt32(r["CasesPacked"]),
                    Convert.ToInt32(r["CasesDispatched"]),
                    avail);
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
            sb.Append("<h1>SIRIMIRI — Finished Goods Stock Level (Cases)</h1>");
            sb.AppendFormat("<span class='dt'>Generated: {0}</span>", dateStr);
            sb.Append("</div>");

            sb.Append("<button class='no-print' onclick='window.print()' style='padding:8px 20px;background:#2980b9;color:#fff;border:none;border-radius:6px;font-size:13px;cursor:pointer;margin-bottom:12px'>Print / Save as PDF</button>");

            sb.Append("<table><thead><tr>");
            sb.Append("<th style='width:40px'>#</th>");
            sb.Append("<th style='width:90px'>Product Code</th>");
            sb.Append("<th>Product Name</th>");
            sb.Append("<th style='width:90px;text-align:right'>Cases Packed</th>");
            sb.Append("<th style='width:90px;text-align:right'>Dispatched</th>");
            sb.Append("<th style='width:100px;text-align:right'>Available Cases</th>");
            sb.Append("</tr></thead><tbody>");

            int row = 0;
            foreach (DataRow r in dt.Rows)
            {
                row++;
                int avail = Convert.ToInt32(r["AvailableCases"]);
                int packed = Convert.ToInt32(r["CasesPacked"]);
                int dispatched = Convert.ToInt32(r["CasesDispatched"]);

                sb.AppendFormat("<tr><td>{0}</td>", row);
                sb.AppendFormat("<td>{0}</td>", r["ProductCode"]);
                sb.AppendFormat("<td>{0}</td>", r["ProductName"]);
                sb.AppendFormat("<td class='num'>{0}</td>", packed > 0 ? packed.ToString("N0") : "<span class='zero'>—</span>");
                sb.AppendFormat("<td class='num'>{0}</td>", dispatched > 0 ? dispatched.ToString("N0") : "<span class='zero'>—</span>");
                sb.AppendFormat("<td class='num avail'>{0}</td>", avail > 0 ? avail.ToString("N0") : (avail == 0 ? "<span class='zero'>0</span>" : "<span style='color:#e74c3c'>" + avail.ToString("N0") + "</span>"));
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
    }
}
