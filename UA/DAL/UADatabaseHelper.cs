using System;
using System.Configuration;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using MySql.Data.MySqlClient;

namespace UAApp.DAL
{
    public static class UADatabaseHelper
    {
        private static string ConnStr => ConfigurationManager.ConnectionStrings["StockDB"].ConnectionString;

        /// <summary>Open a connection and force MySQL session time zone to IST
        /// so NOW() / CURRENT_TIMESTAMP return Indian time regardless of VPS clock.</summary>
        internal static MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(ConnStr);
            conn.Open();
            using (var tz = new MySqlCommand("SET time_zone='+05:30';", conn))
                tz.ExecuteNonQuery();
            return conn;
        }

        private static DataTable ExecuteQuery(string sql, params MySqlParameter[] prms)
        {
            var dt = new DataTable();
            using (var conn = OpenConnection())
            using (var cmd = new MySqlCommand(sql, conn))
            using (var da = new MySqlDataAdapter(cmd))
            { if (prms != null) cmd.Parameters.AddRange(prms); da.Fill(dt); }
            return dt;
        }

        private static DataRow ExecuteQueryRow(string sql, params MySqlParameter[] prms)
        { var dt = ExecuteQuery(sql, prms); return dt.Rows.Count > 0 ? dt.Rows[0] : null; }

        private static object ExecuteScalar(string sql, params MySqlParameter[] prms)
        {
            using (var conn = OpenConnection())
            using (var cmd = new MySqlCommand(sql, conn))
            { if (prms != null) cmd.Parameters.AddRange(prms); return cmd.ExecuteScalar(); }
        }

        private static void ExecuteNonQuery(string sql, params MySqlParameter[] prms)
        {
            using (var conn = OpenConnection())
            using (var cmd = new MySqlCommand(sql, conn))
            { if (prms != null) cmd.Parameters.AddRange(prms); cmd.ExecuteNonQuery(); }
        }

        public static DataRow ValidateUser(string username, string passwordHash)
        {
            return ExecuteQueryRow(
                "SELECT UserID, FullName, Username, Role, IsActive, MustChangePwd FROM Users WHERE Username=?u AND PasswordHash=?p AND IsActive=1;",
                new MySqlParameter("?u", username), new MySqlParameter("?p", passwordHash));
        }

        public static string HashPassword(string plain)
        {
            using (var sha = SHA256.Create())
            { var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(plain)); var sb = new StringBuilder(); foreach (var b in bytes) sb.Append(b.ToString("x2")); return sb.ToString(); }
        }

        public static bool RoleHasAppAccess(string roleCode, string appCode)
        {
            if (roleCode == "Super") return true;
            object val = ExecuteScalar("SELECT CanAccess FROM ERP_RoleAppAccess WHERE RoleCode=?rc AND AppCode=?ac;",
                new MySqlParameter("?rc", roleCode), new MySqlParameter("?ac", appCode));
            return val != null && val != DBNull.Value && Convert.ToInt32(val) == 1;
        }

        public static bool RoleHasModuleAccess(string roleCode, string appCode, string moduleCode)
        {
            if (roleCode == "Super") return true;
            object val = ExecuteScalar("SELECT CanAccess FROM ERP_RoleModuleAccess WHERE RoleCode=?rc AND AppCode=?ac AND ModuleCode=?mc;",
                new MySqlParameter("?rc", roleCode), new MySqlParameter("?ac", appCode), new MySqlParameter("?mc", moduleCode));
            return val != null && val != DBNull.Value && Convert.ToInt32(val) == 1;
        }

        public static DataTable GetAllRoles()
        { return ExecuteQuery("SELECT * FROM ERP_Roles WHERE IsActive=1 ORDER BY SortOrder;"); }

        public static DataRow GetRoleByCode(string roleCode)
        { return ExecuteQueryRow("SELECT * FROM ERP_Roles WHERE RoleCode=?rc;", new MySqlParameter("?rc", roleCode)); }

        public static DataTable GetRoleAppAccess(string roleCode)
        {
            return ExecuteQuery(
                "SELECT a.AppCode, a.AppName, IFNULL(ra.CanAccess, 0) AS CanAccess FROM ERP_Applications a" +
                " LEFT JOIN ERP_RoleAppAccess ra ON ra.AppCode = a.AppCode AND ra.RoleCode=?rc WHERE a.IsActive=1 ORDER BY a.SortOrder;",
                new MySqlParameter("?rc", roleCode));
        }

        public static DataTable GetRoleModuleAccess(string roleCode, string appCode)
        {
            return ExecuteQuery(
                "SELECT m.AppCode, m.ModuleCode, m.ModuleName, IFNULL(rm.CanAccess, 0) AS CanAccess FROM ERP_Modules m" +
                " LEFT JOIN ERP_RoleModuleAccess rm ON rm.AppCode = m.AppCode AND rm.ModuleCode = m.ModuleCode AND rm.RoleCode=?rc" +
                " WHERE m.AppCode=?ac AND m.IsActive=1 ORDER BY m.SortOrder;",
                new MySqlParameter("?rc", roleCode), new MySqlParameter("?ac", appCode));
        }

        public static void SaveRoleAppAccess(string roleCode, string appCode, bool canAccess)
        {
            ExecuteNonQuery("INSERT INTO ERP_RoleAppAccess (RoleCode, AppCode, CanAccess) VALUES (?rc,?ac,?ca) ON DUPLICATE KEY UPDATE CanAccess=?ca2;",
                new MySqlParameter("?rc", roleCode), new MySqlParameter("?ac", appCode),
                new MySqlParameter("?ca", canAccess ? 1 : 0), new MySqlParameter("?ca2", canAccess ? 1 : 0));
        }

        public static void SaveRoleModuleAccess(string roleCode, string appCode, string moduleCode, bool canAccess)
        {
            ExecuteNonQuery("INSERT INTO ERP_RoleModuleAccess (RoleCode, AppCode, ModuleCode, CanAccess) VALUES (?rc,?ac,?mc,?ca) ON DUPLICATE KEY UPDATE CanAccess=?ca2;",
                new MySqlParameter("?rc", roleCode), new MySqlParameter("?ac", appCode), new MySqlParameter("?mc", moduleCode),
                new MySqlParameter("?ca", canAccess ? 1 : 0), new MySqlParameter("?ca2", canAccess ? 1 : 0));
        }

        public static void ClearRoleAccess(string roleCode)
        {
            ExecuteNonQuery("DELETE FROM ERP_RoleAppAccess WHERE RoleCode=?rc;", new MySqlParameter("?rc", roleCode));
            ExecuteNonQuery("DELETE FROM ERP_RoleModuleAccess WHERE RoleCode=?rc;", new MySqlParameter("?rc", roleCode));
        }

        public static DataTable GetAllApplications()
        { return ExecuteQuery("SELECT * FROM ERP_Applications WHERE IsActive=1 ORDER BY SortOrder;"); }

        public static DataTable GetModulesByApp(string appCode)
        { return ExecuteQuery("SELECT * FROM ERP_Modules WHERE AppCode=?ac AND IsActive=1 ORDER BY SortOrder;", new MySqlParameter("?ac", appCode)); }

        public static DataTable GetAllUsers()
        {
            return ExecuteQuery(
                "SELECT u.UserID, u.FullName, u.Username, u.Role, IFNULL(r.RoleName, u.Role) AS RoleName, u.IsActive, u.LastLogin" +
                " FROM Users u LEFT JOIN ERP_Roles r ON r.RoleCode = u.Role ORDER BY u.FullName;");
        }

        public static DataRow GetUserById(int userId)
        { return ExecuteQueryRow("SELECT UserID, FullName, Username, Role, IsActive FROM Users WHERE UserID=?id;", new MySqlParameter("?id", userId)); }

        public static void CreateUser(string fullName, string username, string passwordHash, string roleCode)
        {
            ExecuteNonQuery("INSERT INTO Users (FullName, Username, PasswordHash, Role, IsActive, MustChangePwd) VALUES (?fn,?un,?pw,?ro,1,1);",
                new MySqlParameter("?fn", fullName), new MySqlParameter("?un", username),
                new MySqlParameter("?pw", passwordHash), new MySqlParameter("?ro", roleCode));
        }

        public static int GetLastInsertId()
        { object val = ExecuteScalar("SELECT LAST_INSERT_ID();"); return val != null ? Convert.ToInt32(Convert.ToInt64(val)) : 0; }

        public static void UpdateUser(int userId, string fullName, string username, string roleCode)
        {
            ExecuteNonQuery("UPDATE Users SET FullName=?fn, Username=?un, Role=?ro WHERE UserID=?id;",
                new MySqlParameter("?fn", fullName), new MySqlParameter("?un", username),
                new MySqlParameter("?ro", roleCode), new MySqlParameter("?id", userId));
        }

        public static void ToggleUserActive(int userId)
        { ExecuteNonQuery("UPDATE Users SET IsActive = IF(IsActive=1, 0, 1) WHERE UserID=?id;", new MySqlParameter("?id", userId)); }

        public static void ResetPassword(int userId, string newHash)
        { ExecuteNonQuery("UPDATE Users SET PasswordHash=?pw, MustChangePwd=1 WHERE UserID=?id;", new MySqlParameter("?pw", newHash), new MySqlParameter("?id", userId)); }

        public static bool UsernameExists(string username, int excludeUserId = 0)
        {
            object val = ExecuteScalar("SELECT COUNT(*) FROM Users WHERE LOWER(Username)=LOWER(?u) AND UserID!=?eid;",
                new MySqlParameter("?u", username), new MySqlParameter("?eid", excludeUserId));
            return Convert.ToInt32(val) > 0;
        }

        public static string GetRoleAppsString(string roleCode)
        {
            if (roleCode == "Super") return "ALL";
            DataTable dt = ExecuteQuery("SELECT AppCode FROM ERP_RoleAppAccess WHERE RoleCode=?rc AND CanAccess=1 ORDER BY AppCode;",
                new MySqlParameter("?rc", roleCode));
            var sb = new StringBuilder();
            foreach (DataRow r in dt.Rows) { if (sb.Length > 0) sb.Append(","); sb.Append(r["AppCode"]); }
            return sb.ToString();
        }

        // ── ZONES ─────────────────────────────────────────────────────────
        public static DataTable GetAllZones()
        { return ExecuteQuery("SELECT * FROM SA_Zones WHERE IsActive=1 ORDER BY SortOrder, ZoneName;"); }

        public static string GenerateZoneCode()
        {
            object val = ExecuteScalar("SELECT MAX(CAST(SUBSTRING(ZoneCode,3) AS SIGNED)) FROM SA_Zones WHERE ZoneCode LIKE 'ZN%';");
            int next = (val == null || val == DBNull.Value) ? 1 : Convert.ToInt32(Convert.ToInt64(val)) + 1;
            return "ZN" + next.ToString("D3");
        }

        public static void SaveZone(int zoneId, string zoneName, string zoneCode = null)
        {
            if (zoneId == 0)
            {
                if (string.IsNullOrEmpty(zoneCode)) zoneCode = GenerateZoneCode();
                ExecuteNonQuery("INSERT INTO SA_Zones (ZoneName, ZoneCode) VALUES (?n,?c);",
                    new MySqlParameter("?n", zoneName), new MySqlParameter("?c", zoneCode));
            }
            else
                ExecuteNonQuery("UPDATE SA_Zones SET ZoneName=?n WHERE ZoneID=?id;",
                    new MySqlParameter("?n", zoneName), new MySqlParameter("?id", zoneId));
        }

        public static void ToggleZoneActive(int zoneId)
        { ExecuteNonQuery("UPDATE SA_Zones SET IsActive=IF(IsActive=1,0,1) WHERE ZoneID=?id;", new MySqlParameter("?id", zoneId)); }

        // ── REGIONS ───────────────────────────────────────────────────────
        public static DataTable GetAllRegions()
        {
            return ExecuteQuery(
                "SELECT r.*, z.ZoneName FROM SA_Regions r JOIN SA_Zones z ON z.ZoneID=r.ZoneID WHERE r.IsActive=1 ORDER BY z.SortOrder, r.SortOrder, r.RegionName;");
        }

        public static DataTable GetRegionsByZone(int zoneId)
        { return ExecuteQuery("SELECT * FROM SA_Regions WHERE ZoneID=?zid AND IsActive=1 ORDER BY SortOrder, RegionName;", new MySqlParameter("?zid", zoneId)); }

        public static string GenerateRegionCode()
        {
            object val = ExecuteScalar("SELECT MAX(CAST(SUBSTRING(RegionCode,3) AS SIGNED)) FROM SA_Regions WHERE RegionCode LIKE 'RG%';");
            int next = (val == null || val == DBNull.Value) ? 1 : Convert.ToInt32(Convert.ToInt64(val)) + 1;
            return "RG" + next.ToString("D3");
        }

        public static void SaveRegion(int regionId, int zoneId, string regionName, string regionCode = null)
        {
            if (regionId == 0)
            {
                if (string.IsNullOrEmpty(regionCode)) regionCode = GenerateRegionCode();
                ExecuteNonQuery("INSERT INTO SA_Regions (ZoneID, RegionName, RegionCode) VALUES (?z,?n,?c);",
                    new MySqlParameter("?z", zoneId), new MySqlParameter("?n", regionName), new MySqlParameter("?c", regionCode));
            }
            else
                ExecuteNonQuery("UPDATE SA_Regions SET ZoneID=?z, RegionName=?n WHERE RegionID=?id;",
                    new MySqlParameter("?z", zoneId), new MySqlParameter("?n", regionName), new MySqlParameter("?id", regionId));
        }

        // ── AREAS ─────────────────────────────────────────────────────────
        public static DataTable GetAllAreas()
        {
            return ExecuteQuery(
                "SELECT a.*, r.RegionName, z.ZoneName FROM SA_Areas a" +
                " JOIN SA_Regions r ON r.RegionID=a.RegionID" +
                " JOIN SA_Zones z ON z.ZoneID=r.ZoneID" +
                " WHERE a.IsActive=1 ORDER BY z.SortOrder, r.SortOrder, a.SortOrder, a.AreaName;");
        }

        public static DataTable GetAreasByRegion(int regionId)
        { return ExecuteQuery("SELECT * FROM SA_Areas WHERE RegionID=?rid AND IsActive=1 ORDER BY SortOrder, AreaName;", new MySqlParameter("?rid", regionId)); }

        public static string GenerateAreaCode()
        {
            object val = ExecuteScalar("SELECT MAX(CAST(SUBSTRING(AreaCode,3) AS SIGNED)) FROM SA_Areas WHERE AreaCode LIKE 'AR%';");
            int next = (val == null || val == DBNull.Value) ? 1 : Convert.ToInt32(Convert.ToInt64(val)) + 1;
            return "AR" + next.ToString("D3");
        }

        public static void SaveArea(int areaId, int regionId, string areaName, string areaCode = null)
        {
            if (areaId == 0)
            {
                if (string.IsNullOrEmpty(areaCode)) areaCode = GenerateAreaCode();
                ExecuteNonQuery("INSERT INTO SA_Areas (RegionID, AreaName, AreaCode) VALUES (?r,?n,?c);",
                    new MySqlParameter("?r", regionId), new MySqlParameter("?n", areaName), new MySqlParameter("?c", areaCode));
            }
            else
                ExecuteNonQuery("UPDATE SA_Areas SET RegionID=?r, AreaName=?n WHERE AreaID=?id;",
                    new MySqlParameter("?r", regionId), new MySqlParameter("?n", areaName), new MySqlParameter("?id", areaId));
        }

        // ── DESIGNATIONS ──────────────────────────────────────────────────
        public static DataTable GetAllDesignations()
        { return ExecuteQuery("SELECT * FROM SA_Designations WHERE IsActive=1 ORDER BY SortOrder;"); }

        // ── ORG POSITIONS ─────────────────────────────────────────────────
        public static DataTable GetAllOrgPositions()
        {
            return ExecuteQuery(
                "SELECT p.PositionID, p.EmployeeID, p.EmployeeName, p.IsActive," +
                " d.DesignName, d.HierarchyLevel, d.DesignCode," +
                " z.ZoneName, r.RegionName," +
                " u.Username, u.FullName AS UserFullName," +
                " mgr.EmployeeName AS ReportsToName, mgrd.DesignName AS ReportsToDesign" +
                " FROM SA_OrgPositions p" +
                " JOIN SA_Designations d ON d.DesignationID=p.DesignationID" +
                " LEFT JOIN SA_Zones z ON z.ZoneID=p.ZoneID" +
                " LEFT JOIN SA_Regions r ON r.RegionID=p.RegionID" +
                " LEFT JOIN Users u ON u.UserID=p.UserID" +
                " LEFT JOIN SA_OrgPositions mgr ON mgr.PositionID=p.ReportsToID" +
                " LEFT JOIN SA_Designations mgrd ON mgrd.DesignationID=mgr.DesignationID" +
                " WHERE p.IsActive=1 ORDER BY d.SortOrder, z.SortOrder, r.SortOrder, p.EmployeeName;");
        }

        public static DataRow GetOrgPositionById(int positionId)
        {
            return ExecuteQueryRow(
                "SELECT * FROM SA_OrgPositions WHERE PositionID=?id;", new MySqlParameter("?id", positionId));
        }

        public static DataTable GetPositionsByDesignation(int hierarchyLevel)
        {
            return ExecuteQuery(
                "SELECT p.PositionID, p.EmployeeName, d.DesignName" +
                " FROM SA_OrgPositions p JOIN SA_Designations d ON d.DesignationID=p.DesignationID" +
                " WHERE d.HierarchyLevel=?lvl AND p.IsActive=1 ORDER BY p.EmployeeName;",
                new MySqlParameter("?lvl", hierarchyLevel));
        }

        public static void SaveOrgPosition(int positionId, int? userId, int designationId,
            string employeeId, string employeeName, int? zoneId, int? regionId, int? reportsToId)
        {
            if (positionId == 0)
                ExecuteNonQuery(
                    "INSERT INTO SA_OrgPositions (UserID, DesignationID, EmployeeID, EmployeeName, ZoneID, RegionID, ReportsToID)" +
                    " VALUES (?uid,?did,?eid,?ename,?zid,?rid,?rtid);",
                    new MySqlParameter("?uid", userId.HasValue ? (object)userId.Value : DBNull.Value),
                    new MySqlParameter("?did", designationId),
                    new MySqlParameter("?eid", (object)employeeId ?? DBNull.Value),
                    new MySqlParameter("?ename", (object)employeeName ?? DBNull.Value),
                    new MySqlParameter("?zid", zoneId.HasValue ? (object)zoneId.Value : DBNull.Value),
                    new MySqlParameter("?rid", regionId.HasValue ? (object)regionId.Value : DBNull.Value),
                    new MySqlParameter("?rtid", reportsToId.HasValue ? (object)reportsToId.Value : DBNull.Value));
            else
                ExecuteNonQuery(
                    "UPDATE SA_OrgPositions SET UserID=?uid, DesignationID=?did, EmployeeID=?eid," +
                    " EmployeeName=?ename, ZoneID=?zid, RegionID=?rid, ReportsToID=?rtid WHERE PositionID=?id;",
                    new MySqlParameter("?uid", userId.HasValue ? (object)userId.Value : DBNull.Value),
                    new MySqlParameter("?did", designationId),
                    new MySqlParameter("?eid", (object)employeeId ?? DBNull.Value),
                    new MySqlParameter("?ename", (object)employeeName ?? DBNull.Value),
                    new MySqlParameter("?zid", zoneId.HasValue ? (object)zoneId.Value : DBNull.Value),
                    new MySqlParameter("?rid", regionId.HasValue ? (object)regionId.Value : DBNull.Value),
                    new MySqlParameter("?rtid", reportsToId.HasValue ? (object)reportsToId.Value : DBNull.Value),
                    new MySqlParameter("?id", positionId));
        }

        public static void TogglePositionActive(int positionId)
        { ExecuteNonQuery("UPDATE SA_OrgPositions SET IsActive=IF(IsActive=1,0,1) WHERE PositionID=?id;", new MySqlParameter("?id", positionId)); }

        public static void ExecuteNonQueryDirect(string sql, params MySqlParameter[] prms)
        { ExecuteNonQuery(sql, prms); }

        public static bool VerifyPassword(int userId, string passwordHash)
        {
            var dt = ExecuteQuery("SELECT UserID FROM Users WHERE UserID=?id AND PasswordHash=?h;",
                new MySqlParameter("?id", userId), new MySqlParameter("?h", passwordHash));
            return dt.Rows.Count > 0;
        }

        public static void ChangePassword(int userId, string newHash)
        {
            ExecuteNonQuery("UPDATE Users SET PasswordHash=?h, MustChangePwd=0 WHERE UserID=?id;",
                new MySqlParameter("?h", newHash), new MySqlParameter("?id", userId));
        }
    }
}
