using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.SessionState;
using MySql.Data.MySqlClient;

namespace HRModule
{
    /// <summary>
    /// JSON endpoint for the org chart page.
    ///
    /// Two actions:
    ///   ?action=tree   -> returns the full reporting graph (all active+inactive
    ///                      employees with mgrId references). Single round-trip,
    ///                      client builds the tree.
    ///   ?action=detail&id=N -> returns the full HR_Employee row for one person,
    ///                          consumed by the side drawer.
    ///
    /// Auth: requires HR_UserID OR UserID in session, role = Super or Admin
    /// (mirrors HREmployee.aspx / HRDepartment.aspx). 401 otherwise.
    ///
    /// Implements IRequiresSessionState so Session is available in this handler
    /// (handlers don't get session by default in IIS).
    ///
    /// JSON output is hand-built (StringBuilder) to avoid pulling in a JSON
    /// library. The shape is small and stable; a CSV-of-fields serializer is
    /// the right size for this job.
    /// </summary>
    public class HROrgChartHandler : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable { get { return false; } }

        public void ProcessRequest(HttpContext ctx)
        {
            // ---- Auth gate (mirrors the .aspx pages) ----
            HttpSessionState s = ctx.Session;
            if (s == null || (s["HR_UserID"] == null && s["UserID"] == null))
            {
                Send401(ctx, "Not signed in.");
                return;
            }
            string role = (s["HR_Role"] as string) ?? (s["UserRole"] as string) ?? (s["Role"] as string);
            if (role != "Super" && role != "Admin")
            {
                Send401(ctx, "Insufficient role.");
                return;
            }

            // ---- Dispatch ----
            string action = (ctx.Request.QueryString["action"] ?? "tree").ToLowerInvariant();
            try
            {
                if (action == "tree")        SendTree(ctx);
                else if (action == "detail") SendDetail(ctx);
                else                         SendError(ctx, 400, "Unknown action: " + action);
            }
            catch (Exception ex)
            {
                SendError(ctx, 500, ex.Message);
            }
        }

        // -------------------------------------------------------------------
        // ?action=tree
        // Returns: { generatedAt: "...", nodes: [ {id, code, name, ...}, ... ] }
        // -------------------------------------------------------------------
        private static void SendTree(HttpContext ctx)
        {
            string sql = @"
                SELECT
                    e.EmployeeID,
                    e.EmployeeCode,
                    e.FullName,
                    e.Designation,
                    d.DeptName,
                    e.ReportingManagerID,
                    e.Zone,
                    e.Region,
                    e.Area,
                    e.WorkLocation,
                    e.MobileNo,
                    e.DOJ,
                    e.IsActive,
                    e.EmploymentType,
                    (SELECT COUNT(*) FROM HR_Employee s WHERE s.ReportingManagerID = e.EmployeeID) AS DirectReports
                  FROM HR_Employee e
                  LEFT JOIN HR_Department d ON d.DeptID = e.DeptID
                 ORDER BY e.EmployeeCode";

            DataTable dt = new DataTable();
            using (MySqlConnection con = HR_DatabaseHelper.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(sql, con))
            {
                con.Open();
                using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    da.Fill(dt);
            }

            StringBuilder sb = new StringBuilder(8192);
            sb.Append("{\"generatedAt\":\"")
              .Append(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"))
              .Append("\",\"nodes\":[");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i > 0) sb.Append(',');
                DataRow r = dt.Rows[i];
                sb.Append('{');
                AppendInt   (sb, "id",            ToInt(r["EmployeeID"]));            sb.Append(',');
                AppendStr   (sb, "code",          AsStr(r["EmployeeCode"]));          sb.Append(',');
                AppendStr   (sb, "name",          AsStr(r["FullName"]));              sb.Append(',');
                AppendStr   (sb, "designation",   AsStr(r["Designation"]));           sb.Append(',');
                AppendStr   (sb, "dept",          AsStr(r["DeptName"]));              sb.Append(',');
                AppendIntOrNull(sb, "mgrId",      r["ReportingManagerID"]);           sb.Append(',');
                AppendStr   (sb, "zone",          AsStr(r["Zone"]));                  sb.Append(',');
                AppendStr   (sb, "region",        AsStr(r["Region"]));                sb.Append(',');
                AppendStr   (sb, "area",          AsStr(r["Area"]));                  sb.Append(',');
                AppendStr   (sb, "location",      AsStr(r["WorkLocation"]));          sb.Append(',');
                AppendStr   (sb, "mobile",        AsStr(r["MobileNo"]));              sb.Append(',');
                AppendDate  (sb, "doj",           r["DOJ"]);                          sb.Append(',');
                AppendBool  (sb, "active",        ToInt(r["IsActive"]) == 1);         sb.Append(',');
                AppendStr   (sb, "empType",       AsStr(r["EmploymentType"]));        sb.Append(',');
                AppendInt   (sb, "reports",       ToInt(r["DirectReports"]));
                sb.Append('}');
            }
            sb.Append("]}");

            WriteJson(ctx, sb.ToString());
        }

        // -------------------------------------------------------------------
        // ?action=detail&id=N
        // Returns the full HR_Employee row for the side drawer.
        // -------------------------------------------------------------------
        private static void SendDetail(HttpContext ctx)
        {
            string idStr = ctx.Request.QueryString["id"];
            int id;
            if (!int.TryParse(idStr, out id) || id <= 0)
            { SendError(ctx, 400, "Missing or invalid id."); return; }

            string sql = @"
                SELECT
                    e.EmployeeID, e.EmployeeCode, e.FullName, e.FatherName, e.Gender,
                    e.DOB, e.DOJ, e.DOL,
                    d.DeptName, e.Designation, e.EmploymentType,
                    e.ReportingManager, e.ReportingManagerID,
                    mgr.FullName     AS MgrName,
                    mgr.EmployeeCode AS MgrCode,
                    e.Zone, e.Region, e.Area, e.WorkLocation,
                    e.MobileNo, e.AltMobileNo, e.Email,
                    e.AddressLine, e.City, e.StateName, e.Pincode,
                    e.AadhaarNo, e.PANNo, e.UANNo, e.PFNo, e.ESINo,
                    e.BankAccountNo, e.BankName, e.IFSCCode,
                    e.BasicSalary, e.HRA, e.ConveyanceAllow, e.OtherAllow, e.GrossSalary,
                    e.IsActive,
                    e.CreatedAt, e.CreatedBy, e.ModifiedAt, e.ModifiedBy,
                    (SELECT COUNT(*) FROM HR_Employee s WHERE s.ReportingManagerID = e.EmployeeID) AS DirectReports
                  FROM HR_Employee e
                  LEFT JOIN HR_Department d  ON d.DeptID     = e.DeptID
                  LEFT JOIN HR_Employee   mgr ON mgr.EmployeeID = e.ReportingManagerID
                 WHERE e.EmployeeID = @id
                 LIMIT 1";

            DataTable dt = new DataTable();
            using (MySqlConnection con = HR_DatabaseHelper.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    da.Fill(dt);
            }

            if (dt.Rows.Count == 0)
            { SendError(ctx, 404, "Employee not found."); return; }

            DataRow r = dt.Rows[0];
            StringBuilder sb = new StringBuilder(2048);
            sb.Append('{');

            AppendInt   (sb, "id",            ToInt(r["EmployeeID"]));            sb.Append(',');
            AppendStr   (sb, "code",          AsStr(r["EmployeeCode"]));          sb.Append(',');
            AppendStr   (sb, "name",          AsStr(r["FullName"]));              sb.Append(',');
            AppendStr   (sb, "fatherName",    AsStr(r["FatherName"]));            sb.Append(',');
            AppendStr   (sb, "gender",        AsStr(r["Gender"]));                sb.Append(',');
            AppendDate  (sb, "dob",           r["DOB"]);                          sb.Append(',');
            AppendDate  (sb, "doj",           r["DOJ"]);                          sb.Append(',');
            AppendDate  (sb, "dol",           r["DOL"]);                          sb.Append(',');
            AppendStr   (sb, "dept",          AsStr(r["DeptName"]));              sb.Append(',');
            AppendStr   (sb, "designation",   AsStr(r["Designation"]));           sb.Append(',');
            AppendStr   (sb, "empType",       AsStr(r["EmploymentType"]));        sb.Append(',');
            AppendStr   (sb, "managerText",   AsStr(r["ReportingManager"]));      sb.Append(',');
            AppendIntOrNull(sb, "mgrId",      r["ReportingManagerID"]);           sb.Append(',');
            AppendStr   (sb, "mgrName",       AsStr(r["MgrName"]));               sb.Append(',');
            AppendStr   (sb, "mgrCode",       AsStr(r["MgrCode"]));               sb.Append(',');
            AppendStr   (sb, "zone",          AsStr(r["Zone"]));                  sb.Append(',');
            AppendStr   (sb, "region",        AsStr(r["Region"]));                sb.Append(',');
            AppendStr   (sb, "area",          AsStr(r["Area"]));                  sb.Append(',');
            AppendStr   (sb, "location",      AsStr(r["WorkLocation"]));          sb.Append(',');
            AppendStr   (sb, "mobile",        AsStr(r["MobileNo"]));              sb.Append(',');
            AppendStr   (sb, "altMobile",     AsStr(r["AltMobileNo"]));           sb.Append(',');
            AppendStr   (sb, "email",         AsStr(r["Email"]));                 sb.Append(',');
            AppendStr   (sb, "address",       AsStr(r["AddressLine"]));           sb.Append(',');
            AppendStr   (sb, "city",          AsStr(r["City"]));                  sb.Append(',');
            AppendStr   (sb, "state",         AsStr(r["StateName"]));             sb.Append(',');
            AppendStr   (sb, "pincode",       AsStr(r["Pincode"]));               sb.Append(',');
            AppendStr   (sb, "aadhaar",       AsStr(r["AadhaarNo"]));             sb.Append(',');
            AppendStr   (sb, "pan",           AsStr(r["PANNo"]));                 sb.Append(',');
            AppendStr   (sb, "uan",           AsStr(r["UANNo"]));                 sb.Append(',');
            AppendStr   (sb, "pfNo",          AsStr(r["PFNo"]));                  sb.Append(',');
            AppendStr   (sb, "esiNo",         AsStr(r["ESINo"]));                 sb.Append(',');
            AppendStr   (sb, "bankAcNo",      AsStr(r["BankAccountNo"]));         sb.Append(',');
            AppendStr   (sb, "bankName",      AsStr(r["BankName"]));              sb.Append(',');
            AppendStr   (sb, "ifsc",          AsStr(r["IFSCCode"]));              sb.Append(',');
            AppendDec   (sb, "basic",         r["BasicSalary"]);                  sb.Append(',');
            AppendDec   (sb, "hra",           r["HRA"]);                          sb.Append(',');
            AppendDec   (sb, "conveyance",    r["ConveyanceAllow"]);              sb.Append(',');
            AppendDec   (sb, "other",         r["OtherAllow"]);                   sb.Append(',');
            AppendDec   (sb, "gross",         r["GrossSalary"]);                  sb.Append(',');
            AppendBool  (sb, "active",        ToInt(r["IsActive"]) == 1);         sb.Append(',');
            AppendDate  (sb, "createdAt",     r["CreatedAt"]);                    sb.Append(',');
            AppendStr   (sb, "createdBy",     AsStr(r["CreatedBy"]));             sb.Append(',');
            AppendDate  (sb, "modifiedAt",    r["ModifiedAt"]);                   sb.Append(',');
            AppendStr   (sb, "modifiedBy",    AsStr(r["ModifiedBy"]));            sb.Append(',');
            AppendInt   (sb, "directReports", ToInt(r["DirectReports"]));

            sb.Append('}');
            WriteJson(ctx, sb.ToString());
        }

        // ===================================================================
        // JSON / response helpers
        // ===================================================================
        private static void WriteJson(HttpContext ctx, string body)
        {
            ctx.Response.Clear();
            ctx.Response.ContentType = "application/json; charset=utf-8";
            ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            ctx.Response.Write(body);
        }

        private static void Send401(HttpContext ctx, string msg)
        {
            ctx.Response.Clear();
            ctx.Response.StatusCode = 401;
            ctx.Response.ContentType = "application/json; charset=utf-8";
            ctx.Response.Write("{\"error\":" + JsonString(msg) + "}");
        }

        private static void SendError(HttpContext ctx, int code, string msg)
        {
            ctx.Response.Clear();
            ctx.Response.StatusCode = code;
            ctx.Response.ContentType = "application/json; charset=utf-8";
            ctx.Response.Write("{\"error\":" + JsonString(msg) + "}");
        }

        // ===================================================================
        // Field appenders — keep all serialization in one place
        // ===================================================================
        private static void AppendStr(StringBuilder sb, string key, string value)
        {
            sb.Append('"').Append(key).Append("\":");
            if (value == null) sb.Append("null");
            else sb.Append(JsonString(value));
        }

        private static void AppendInt(StringBuilder sb, string key, int value)
        {
            sb.Append('"').Append(key).Append("\":").Append(value.ToString(CultureInfo.InvariantCulture));
        }

        private static void AppendIntOrNull(StringBuilder sb, string key, object value)
        {
            sb.Append('"').Append(key).Append("\":");
            if (value == null || value == DBNull.Value) { sb.Append("null"); return; }
            try { sb.Append(Convert.ToInt32(Convert.ToInt64(value)).ToString(CultureInfo.InvariantCulture)); }
            catch { sb.Append("null"); }
        }

        private static void AppendBool(StringBuilder sb, string key, bool value)
        {
            sb.Append('"').Append(key).Append("\":").Append(value ? "true" : "false");
        }

        private static void AppendDate(StringBuilder sb, string key, object value)
        {
            sb.Append('"').Append(key).Append("\":");
            if (value == null || value == DBNull.Value) { sb.Append("null"); return; }
            try
            {
                DateTime d = Convert.ToDateTime(value);
                if (d == DateTime.MinValue) { sb.Append("null"); return; }
                // Serialize as YYYY-MM-DD (no time / no timezone) to match how
                // HR pages display dates everywhere else in the module.
                sb.Append('"').Append(d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append('"');
            }
            catch { sb.Append("null"); }
        }

        private static void AppendDec(StringBuilder sb, string key, object value)
        {
            sb.Append('"').Append(key).Append("\":");
            if (value == null || value == DBNull.Value) { sb.Append("0"); return; }
            try { sb.Append(Convert.ToDecimal(value).ToString("0.##", CultureInfo.InvariantCulture)); }
            catch { sb.Append("0"); }
        }

        private static int ToInt(object o)
        {
            if (o == null || o == DBNull.Value) return 0;
            try { return Convert.ToInt32(Convert.ToInt64(o)); }
            catch { return 0; }
        }

        private static string AsStr(object o)
        {
            if (o == null || o == DBNull.Value) return null;
            string s = o.ToString();
            return string.IsNullOrEmpty(s) ? null : s;
        }

        // Minimal JSON string escaper. Handles the cases that actually appear
        // in HR data: quotes, backslashes, and a few control chars.
        private static string JsonString(string s)
        {
            if (s == null) return "null";
            StringBuilder sb = new StringBuilder(s.Length + 8);
            sb.Append('"');
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
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
                        if (c < 0x20)
                            sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else
                            sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
            return sb.ToString();
        }
    }
}
