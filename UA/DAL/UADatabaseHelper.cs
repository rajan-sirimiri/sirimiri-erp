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

        private static DataTable ExecuteQuery(string sql, params MySqlParameter[] prms)
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(sql, conn))
            using (var da = new MySqlDataAdapter(cmd))
            { if (prms != null) cmd.Parameters.AddRange(prms); conn.Open(); da.Fill(dt); }
            return dt;
        }

        private static DataRow ExecuteQueryRow(string sql, params MySqlParameter[] prms)
        { var dt = ExecuteQuery(sql, prms); return dt.Rows.Count > 0 ? dt.Rows[0] : null; }

        private static object ExecuteScalar(string sql, params MySqlParameter[] prms)
        {
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(sql, conn))
            { if (prms != null) cmd.Parameters.AddRange(prms); conn.Open(); return cmd.ExecuteScalar(); }
        }

        private static void ExecuteNonQuery(string sql, params MySqlParameter[] prms)
        {
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(sql, conn))
            { if (prms != null) cmd.Parameters.AddRange(prms); conn.Open(); cmd.ExecuteNonQuery(); }
        }

        public static DataRow ValidateUser(string username, string passwordHash)
        {
            return ExecuteQueryRow(
                "SELECT UserID, FullName, Username, Role, IsActive FROM Users WHERE Username=?u AND PasswordHash=?p AND IsActive=1;",
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
    }
}
