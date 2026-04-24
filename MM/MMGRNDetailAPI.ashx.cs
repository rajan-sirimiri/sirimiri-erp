using System;
using System.Data;
using System.Text;
using System.Web;
using System.Web.SessionState;
using MMApp.DAL;

namespace MMApp
{
    /// <summary>
    /// Returns JSON with all line items for a given GRN, for the
    /// inline-expand-row feature on the 4 GRN history lists.
    /// URL: MMGRNDetailAPI.ashx?tab=RM&grn=GRN-RM-00123
    /// tab ∈ { RM, PM, CN, ST }
    /// </summary>
    public class MMGRNDetailAPI : IHttpHandler, IReadOnlySessionState
    {
        public bool IsReusable { get { return false; } }

        public void ProcessRequest(HttpContext ctx)
        {
            ctx.Response.ContentType = "application/json";
            ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            // Auth: same pattern as the aspx pages
            if (ctx.Session == null || ctx.Session["MM_UserID"] == null)
            {
                ctx.Response.StatusCode = 401;
                ctx.Response.Write("{\"error\":\"Not authenticated\"}");
                return;
            }

            string tab = (ctx.Request["tab"] ?? "").Trim().ToUpperInvariant();
            string grn = (ctx.Request["grn"] ?? "").Trim();

            if (string.IsNullOrEmpty(grn))
            {
                ctx.Response.Write("{\"error\":\"Missing grn parameter\"}");
                return;
            }

            DataTable dt;
            string matNameCol, matCodeCol;

            switch (tab)
            {
                case "RM":
                    dt = MMDatabaseHelper.GetRawInwardByGRN(grn);
                    matNameCol = "RMName";   matCodeCol = "RMCode";  break;
                case "PM":
                    dt = MMDatabaseHelper.GetPackingInwardByGRN(grn);
                    matNameCol = "PMName";   matCodeCol = "PMCode";  break;
                case "CN":
                    dt = MMDatabaseHelper.GetConsumableInwardByGRN(grn);
                    matNameCol = "ConsumableName"; matCodeCol = "ConsumableCode"; break;
                case "ST":
                    dt = MMDatabaseHelper.GetStationaryInwardByGRN(grn);
                    matNameCol = "StationaryName"; matCodeCol = "StationaryCode"; break;
                default:
                    ctx.Response.Write("{\"error\":\"Invalid tab (RM|PM|CN|ST required)\"}");
                    return;
            }

            if (dt == null || dt.Rows.Count == 0)
            {
                ctx.Response.Write("{\"lines\":[]}");
                return;
            }

            // Build JSON — header from first row, lines array with per-line fields.
            DataRow first = dt.Rows[0];
            var sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat("\"grnNo\":\"{0}\",",   Esc(S(first, "GRNNo")));
            sb.AppendFormat("\"invoiceNo\":\"{0}\",", Esc(S(first, "InvoiceNo")));
            sb.AppendFormat("\"invoiceDate\":\"{0}\",", D(first, "InvoiceDate"));
            sb.AppendFormat("\"grnDate\":\"{0}\",", D(first, "InwardDate"));
            sb.AppendFormat("\"supplier\":\"{0}\",", Esc(S(first, "SupplierName")));
            sb.AppendFormat("\"poNo\":\"{0}\",",     Esc(S(first, "PONo")));
            sb.AppendFormat("\"createdAt\":\"{0}\",", D(first, "CreatedAt"));
            sb.AppendFormat("\"status\":\"{0}\",",   Esc(S(first, "Status")));
            sb.AppendFormat("\"lineCount\":{0},",    dt.Rows.Count);
            sb.Append("\"lines\":[");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow r = dt.Rows[i];
                if (i > 0) sb.Append(",");
                sb.Append("{");
                sb.AppendFormat("\"matName\":\"{0}\",",  Esc(S(r, matNameCol)));
                sb.AppendFormat("\"matCode\":\"{0}\",",  Esc(S(r, matCodeCol)));
                sb.AppendFormat("\"uom\":\"{0}\",",      Esc(S(r, "Abbreviation")));
                sb.AppendFormat("\"hsn\":\"{0}\",",      Esc(S(r, "HSNCode")));
                sb.AppendFormat("\"qtyInv\":{0},",       N(r, "Quantity"));
                sb.AppendFormat("\"qtyActual\":{0},",    N(r, "QtyActualReceived"));
                sb.AppendFormat("\"qtyBilled\":{0},",    N(r, "QtyInUOM"));
                sb.AppendFormat("\"rate\":{0},",         N(r, "Rate"));
                sb.AppendFormat("\"amount\":{0},",       N(r, "Amount"));
                sb.AppendFormat("\"gstRate\":{0},",      N(r, "GSTRate"));
                sb.AppendFormat("\"gstAmt\":{0},",       N(r, "GSTAmount"));
                sb.AppendFormat("\"transport\":{0},",    N(r, "TransportCost"));
                sb.AppendFormat("\"transInInv\":{0},",   B(r, "TransportInInvoice"));
                sb.AppendFormat("\"transInGst\":{0},",   B(r, "TransportInGST"));
                sb.AppendFormat("\"loading\":{0},",      SafeNum(r, "LoadingCharges"));
                sb.AppendFormat("\"unloading\":{0},",    SafeNum(r, "UnloadingCharges"));
                sb.AppendFormat("\"shortageQty\":{0},",  N(r, "ShortageQty"));
                sb.AppendFormat("\"shortageVal\":{0},",  N(r, "ShortageValue"));
                sb.AppendFormat("\"qc\":{0},",           B(r, "QualityCheck"));
                sb.AppendFormat("\"remarks\":\"{0}\"",   Esc(S(r, "Remarks")));
                sb.Append("}");
            }
            sb.Append("]}");
            ctx.Response.Write(sb.ToString());
        }

        // ── helpers ──
        private static string S(DataRow r, string col)
        {
            if (!r.Table.Columns.Contains(col)) return "";
            object v = r[col];
            return v == DBNull.Value ? "" : v.ToString();
        }
        private static string D(DataRow r, string col)
        {
            if (!r.Table.Columns.Contains(col)) return "";
            object v = r[col];
            if (v == DBNull.Value) return "";
            try { return Convert.ToDateTime(v).ToString("yyyy-MM-dd"); }
            catch { return ""; }
        }
        private static string N(DataRow r, string col)
        {
            if (!r.Table.Columns.Contains(col)) return "0";
            object v = r[col];
            if (v == DBNull.Value) return "0";
            try { return Convert.ToDecimal(v).ToString(System.Globalization.CultureInfo.InvariantCulture); }
            catch { return "0"; }
        }
        private static string SafeNum(DataRow r, string col)
        {
            // Some tables may not have LoadingCharges/UnloadingCharges on older rows
            if (!r.Table.Columns.Contains(col)) return "0";
            return N(r, col);
        }
        private static string B(DataRow r, string col)
        {
            if (!r.Table.Columns.Contains(col)) return "false";
            object v = r[col];
            if (v == DBNull.Value) return "false";
            try { return Convert.ToBoolean(Convert.ToInt32(v)) ? "true" : "false"; }
            catch { try { return Convert.ToBoolean(v) ? "true" : "false"; } catch { return "false"; } }
        }
        private static string Esc(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder(s.Length + 8);
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"':  sb.Append("\\\""); break;
                    case '\b': sb.Append("\\b");  break;
                    case '\f': sb.Append("\\f");  break;
                    case '\n': sb.Append("\\n");  break;
                    case '\r': sb.Append("\\r");  break;
                    case '\t': sb.Append("\\t");  break;
                    default:
                        if (c < 0x20) sb.AppendFormat("\\u{0:x4}", (int)c);
                        else sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
