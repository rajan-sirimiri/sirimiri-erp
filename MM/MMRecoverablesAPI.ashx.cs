using System;
using System.Data;
using System.Text;
using System.Web;
using System.Web.SessionState;
using MMApp.DAL;

namespace MMApp
{
    public class MMRecoverablesAPI : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                if (context.Session == null || context.Session["MM_UserID"] == null)
                {
                    context.Response.Write("{\"items\":[],\"total\":\"0.00\"}");
                    return;
                }

                int supplierId;
                if (!int.TryParse(context.Request.QueryString["supId"], out supplierId) || supplierId <= 0)
                {
                    context.Response.Write("{\"items\":[],\"total\":\"0.00\"}");
                    return;
                }

                DataTable dt = MMDatabaseHelper.GetSupplierRecoverables(supplierId);
                var sb = new StringBuilder("{\"items\":[");
                decimal total = 0;
                foreach (DataRow r in dt.Rows)
                {
                    decimal shortVal = r["ShortageValue"] == DBNull.Value ? 0 : Convert.ToDecimal(r["ShortageValue"]);
                    total += shortVal;
                    sb.AppendFormat("{{\"grn\":\"{0}\",\"rm\":\"{1}\",\"date\":\"{2}\",\"qty\":\"{3}\",\"uom\":\"{4}\",\"value\":\"{5:N2}\"}},",
                        EscapeJson(r["GRNNo"].ToString()),
                        EscapeJson(r["RMName"].ToString()),
                        Convert.ToDateTime(r["InwardDate"]).ToString("dd-MMM-yy"),
                        r["ShortageQty"],
                        EscapeJson(r["Abbreviation"].ToString()),
                        shortVal);
                }
                if (dt.Rows.Count > 0) sb.Length--;
                sb.AppendFormat("],\"total\":\"{0:N2}\"}}", total);
                context.Response.Write(sb.ToString());
            }
            catch (Exception ex)
            {
                context.Response.Write("{\"items\":[],\"total\":\"0.00\",\"error\":\"" + EscapeJson(ex.Message) + "\"}");
            }
        }

        static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "").Replace("\r", "");
        }

        public bool IsReusable { get { return false; } }
    }
}
