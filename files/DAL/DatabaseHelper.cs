using System;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

namespace StockApp.DAL
{
    public static class DatabaseHelper
    {
        private static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["StockDBConnection"].ConnectionString;

        // ── CORE PRIVATE HELPERS ──────────────────────────────────────
        private static DataTable ExecuteStoredProcedure(string procedureName, MySqlParameter[] parameters)
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(ConnectionString))
            using (var cmd  = new MySqlCommand(procedureName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                using (var adapter = new MySqlDataAdapter(cmd))
                {
                    conn.Open();
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        private static DataTable ExecuteQuery(string sql, params MySqlParameter[] parms)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    var dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);
                    return dt;
                }
            }
        }

        private static void ExecuteNonQuery(string sql, params MySqlParameter[] parms)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static object ExecuteScalar(string sql, params MySqlParameter[] parms)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    return cmd.ExecuteScalar();
                }
            }
        }

        // ── AUTH ──────────────────────────────────────────────────────
        // ValidateUser - called by Login.aspx.cs, returns DataRow or null
        public static DataRow ValidateUser(string username, string passwordHash)
        {
            try
            {
                var dt = ExecuteQuery(
                    "SELECT UserID, FullName, Username, Role, StateID, IsActive, MustChangePwd " +
                    "FROM Users WHERE LOWER(Username)=LOWER(?u) AND PasswordHash=?p AND IsActive=1 LIMIT 1;",
                    new MySqlParameter("?u", username),
                    new MySqlParameter("?p", passwordHash));
                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
            catch { return null; }
        }

        public static DataRow GetUserByUsername(string username)
        {
            try
            {
                var dt = ExecuteStoredProcedure("usp_GetUserByUsername", new[] {
                    new MySqlParameter("p_Username", MySqlDbType.VarChar) { Value = username }
                });
                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
            catch { return null; }
        }

        public static DataRow GetUserByID(int userId)
        {
            try
            {
                var dt = ExecuteQuery(
                    "SELECT UserID,FullName,Username,Role,StateID,IsActive,MustChangePwd FROM Users WHERE UserID=?id",
                    new MySqlParameter("?id", userId));
                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
            catch { return null; }
        }

        public static bool VerifyPassword(int userId, string passwordHash)
        {
            var dt = ExecuteQuery(
                "SELECT COUNT(*) FROM Users WHERE UserID=?id AND PasswordHash=?p;",
                new MySqlParameter("?id", userId),
                new MySqlParameter("?p",  passwordHash));
            return Convert.ToInt32(dt.Rows[0][0]) > 0;
        }

        public static void ChangePassword(int userId, string newHash)
        {
            ExecuteNonQuery(
                "UPDATE Users SET PasswordHash=?h, MustChangePwd=0 WHERE UserID=?id;",
                new MySqlParameter("?h",  newHash),
                new MySqlParameter("?id", userId));
        }

        public static void UpdateLastLogin(int userId)
        {
            ExecuteNonQuery(
                "UPDATE Users SET LastLogin=NOW() WHERE UserID=?id;",
                new MySqlParameter("?id", userId));
        }

        // ── USERS ─────────────────────────────────────────────────────
        public static DataTable GetAllUsers()
        {
            return ExecuteQuery(
                "SELECT u.UserID, u.FullName, u.Username, u.Role, u.IsActive, " +
                "u.MustChangePwd, u.LastLogin, s.StateName, u.ReportingManagerID, " +
                "m.FullName AS ManagerName " +
                "FROM Users u " +
                "LEFT JOIN States s ON s.StateID=u.StateID " +
                "LEFT JOIN Users m ON m.UserID=u.ReportingManagerID " +
                "ORDER BY u.FullName;");
        }

        public static DataTable GetManagers()
        {
            return ExecuteQuery(
                "SELECT UserID, FullName, Role FROM Users " +
                "WHERE Role IN ('Admin','Manager') AND IsActive=1 ORDER BY FullName;");
        }

        public static DataTable GetStatesForAdmin()
        {
            return ExecuteQuery("SELECT StateID, StateName FROM States ORDER BY StateName;");
        }

        public static bool CreateUser(string fullName, string username, string hash, string role, int? stateId)
        {
            try
            {
                ExecuteNonQuery(
                    "INSERT INTO Users (FullName, Username, PasswordHash, Role, StateID, IsActive, MustChangePwd) " +
                    "VALUES (?fn, ?un, ?pw, ?ro, ?si, 1, 1);",
                    new MySqlParameter("?fn", fullName),
                    new MySqlParameter("?un", username),
                    new MySqlParameter("?pw", hash),
                    new MySqlParameter("?ro", role),
                    new MySqlParameter("?si", stateId.HasValue ? (object)stateId.Value : DBNull.Value));
                return true;
            }
            catch { return false; }
        }

        public static void ResetPassword(int userId, string hash)
        {
            ExecuteNonQuery(
                "UPDATE Users SET PasswordHash=?h, MustChangePwd=1 WHERE UserID=?id;",
                new MySqlParameter("?h",  hash),
                new MySqlParameter("?id", userId));
        }

        public static void ToggleUserActive(int userId)
        {
            ExecuteNonQuery(
                "UPDATE Users SET IsActive = 1 - IsActive WHERE UserID=?id;",
                new MySqlParameter("?id", userId));
        }

        // ── STATES / CITIES / DISTRIBUTORS ────────────────────────────
        public static DataTable GetStates(int days = 30)
            => ExecuteStoredProcedure("usp_GetStates", new[] {
                new MySqlParameter("p_Days", MySqlDbType.Int32) { Value = days }
            });

        public static DataTable GetCitiesByState(int stateId, int days = 30)
            => ExecuteStoredProcedure("usp_GetCitiesByState", new[] {
                new MySqlParameter("p_StateID", MySqlDbType.Int32) { Value = stateId },
                new MySqlParameter("p_Days",    MySqlDbType.Int32) { Value = days }
            });

        public static DataTable GetDistributorsByCity(int cityId, int days = 30)
            => ExecuteStoredProcedure("usp_GetDistributorsByCity", new[] {
                new MySqlParameter("p_CityID", MySqlDbType.Int32) { Value = cityId },
                new MySqlParameter("p_Days",   MySqlDbType.Int32) { Value = days }
            });

        public static DataRow GetDistributorAddress(int distributorId)
        {
            try
            {
                var dt = ExecuteStoredProcedure("usp_GetDistributorAddress", new[] {
                    new MySqlParameter("p_DistributorID", MySqlDbType.Int32) { Value = distributorId }
                });
                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
            catch { return null; }
        }

        // ── STOCK ENTRY ───────────────────────────────────────────────
        public static int SaveStockPosition(int stateId, int cityId, int distributorId, int currentStock)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("usp_SaveStockPosition", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("p_StateID",       MySqlDbType.Int32) { Value = stateId });
                    cmd.Parameters.Add(new MySqlParameter("p_CityID",        MySqlDbType.Int32) { Value = cityId });
                    cmd.Parameters.Add(new MySqlParameter("p_DistributorID", MySqlDbType.Int32) { Value = distributorId });
                    cmd.Parameters.Add(new MySqlParameter("p_CurrentStock",  MySqlDbType.Int32) { Value = currentStock });
                    cmd.ExecuteNonQuery();
                }
                using (var idCmd = new MySqlCommand("SELECT LAST_INSERT_ID();", conn))
                {
                    object result = idCmd.ExecuteScalar();
                    return result != null && result != DBNull.Value ? Convert.ToInt32(result) : -1;
                }
            }
        }

        // ── LAST STOCK ENTRY ─────────────────────────────────────────
        public static DataRow GetLastStockEntry(int distributorId)
        {
            try
            {
                var dt = ExecuteQuery(
                    "SELECT CurrentStock, EntryDate FROM StockPositions " +
                    "WHERE DistributorID=?did ORDER BY EntryDate DESC LIMIT 1;",
                    new MySqlParameter("?did", distributorId));
                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
            catch { return null; }
        }

        // ── DISTRIBUTOR SUMMARIES (return DataRow - single row) ───────
        public static DataRow GetDistributorSalesSummary(int distributorId, int days = 60)
        {
            try
            {
                var dt = ExecuteStoredProcedure("usp_GetDistributorSalesSummary", new[] {
                    new MySqlParameter("p_DistributorID", MySqlDbType.Int32) { Value = distributorId },
                    new MySqlParameter("p_Days",          MySqlDbType.Int32) { Value = days }
                });
                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
            catch { return null; }
        }

        public static DataRow GetDistributorCreditSummary(int distributorId, int days = 60)
        {
            try
            {
                var dt = ExecuteStoredProcedure("usp_GetDistributorCreditSummary", new[] {
                    new MySqlParameter("p_DistributorID", MySqlDbType.Int32) { Value = distributorId },
                    new MySqlParameter("p_Days",          MySqlDbType.Int32) { Value = days }
                });
                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
            catch { return null; }
        }

        // ── DISTRIBUTOR HISTORY (return DataTable - multiple rows) ────
        public static DataTable GetDistributorOrderHistory(int distributorId, int days = 60)
        {
            try
            {
                return ExecuteStoredProcedure("usp_GetDistributorOrderHistory", new[] {
                    new MySqlParameter("p_DistributorID", MySqlDbType.Int32) { Value = distributorId },
                    new MySqlParameter("p_Days",          MySqlDbType.Int32) { Value = days }
                });
            }
            catch { return new DataTable(); }
        }

        public static DataTable GetDistributorPaymentHistory(int distributorId, int days = 60)
        {
            try
            {
                return ExecuteStoredProcedure("usp_GetDistributorPaymentHistory", new[] {
                    new MySqlParameter("p_DistributorID", MySqlDbType.Int32) { Value = distributorId },
                    new MySqlParameter("p_Days",          MySqlDbType.Int32) { Value = days }
                });
            }
            catch { return new DataTable(); }
        }

        // ── AUDIT ─────────────────────────────────────────────────────
        public static void LogAudit(int userId, string action, int? distributorId, int? stockValue, string ip)
        {
            try
            {
                ExecuteNonQuery(
                    "INSERT INTO AuditLog (UserID, Action, DistributorID, StockValue, IPAddress) " +
                    "VALUES (?uid, ?act, ?did, ?sv, ?ip);",
                    new MySqlParameter("?uid", userId),
                    new MySqlParameter("?act", action),
                    new MySqlParameter("?did", distributorId.HasValue ? (object)distributorId.Value : DBNull.Value),
                    new MySqlParameter("?sv",  stockValue.HasValue    ? (object)stockValue.Value    : DBNull.Value),
                    new MySqlParameter("?ip",  ip ?? ""));
            }
            catch { }
        }

        // ── PRODUCTS ──────────────────────────────────────────────────
        public static DataTable GetAllProducts()
        {
            return ExecuteQuery(
                "SELECT ProductID, ProductName, ProductCode, MRP, HSNCode, GSTRate, IsActive, CreatedAt " +
                "FROM Products ORDER BY ProductName;");
        }

        public static DataTable GetActiveProducts()
        {
            return ExecuteQuery(
                "SELECT ProductID, ProductName, MRP, HSNCode, GSTRate " +
                "FROM Products WHERE IsActive=1 ORDER BY ProductName;");
        }

        public static DataRow GetProductById(int productId)
        {
            var dt = ExecuteQuery(
                "SELECT ProductID, ProductName, ProductCode, MRP, HSNCode, GSTRate FROM Products WHERE ProductID=?id;",
                new MySqlParameter("?id", productId));
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public static void UpdateProduct(int productId, string name, string code, decimal mrp, string hsn, decimal gst)
        {
            ExecuteNonQuery(
                "UPDATE Products SET ProductName=?n, ProductCode=?c, MRP=?m, HSNCode=?h, GSTRate=?g WHERE ProductID=?id;",
                new MySqlParameter("?n",  name),
                new MySqlParameter("?c",  string.IsNullOrEmpty(code) ? (object)DBNull.Value : code),
                new MySqlParameter("?m",  mrp),
                new MySqlParameter("?h",  string.IsNullOrEmpty(hsn)  ? (object)DBNull.Value : hsn),
                new MySqlParameter("?g",  gst),
                new MySqlParameter("?id", productId));
        }

        public static DataRow GetUserById(int userId)
        {
            var dt = ExecuteQuery(
                "SELECT UserID, FullName, Username, Role, StateID, ReportingManagerID FROM Users WHERE UserID=?id;",
                new MySqlParameter("?id", userId));
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public static void UpdateUser(int userId, string fullName, string username, string role, int? stateId, int? managerId)
        {
            ExecuteNonQuery(
                "UPDATE Users SET FullName=?fn, Username=?un, Role=?r, StateID=?sid, ReportingManagerID=?mid WHERE UserID=?id;",
                new MySqlParameter("?fn",  fullName),
                new MySqlParameter("?un",  username),
                new MySqlParameter("?r",   role),
                new MySqlParameter("?sid", stateId.HasValue  ? (object)stateId.Value  : DBNull.Value),
                new MySqlParameter("?mid", managerId.HasValue ? (object)managerId.Value : DBNull.Value),
                new MySqlParameter("?id",  userId));
        }

        public static void AddProduct(string name, string code, decimal mrp, string hsn, decimal gstRate)
        {
            ExecuteNonQuery(
                "INSERT INTO Products (ProductName, ProductCode, MRP, HSNCode, GSTRate) VALUES (?n,?c,?mrp,?hsn,?gst);",
                new MySqlParameter("?n",   name),
                new MySqlParameter("?c",   string.IsNullOrEmpty(code) ? (object)DBNull.Value : code),
                new MySqlParameter("?mrp", mrp),
                new MySqlParameter("?hsn", string.IsNullOrEmpty(hsn)  ? (object)DBNull.Value : hsn),
                new MySqlParameter("?gst", gstRate));
        }

        public static void ToggleProductActive(int productId, bool isActive)
        {
            ExecuteNonQuery(
                "UPDATE Products SET IsActive=?a WHERE ProductID=?id;",
                new MySqlParameter("?a",  isActive ? 1 : 0),
                new MySqlParameter("?id", productId));
        }

        // ── DAILY SALES ───────────────────────────────────────────────
        public static void InsertDailySalesEntry(DateTime entryDate, int distributorId,
                                                  int productId, int qty, string remarks, int userId)
        {
            ExecuteNonQuery(
                "INSERT INTO DailySalesEntries (EntryDate,DistributorID,ProductID,QuantitySold,Remarks,SubmittedBy) " +
                "VALUES (?dt,?did,?pid,?qty,?rmk,?uid);",
                new MySqlParameter("?dt",  entryDate),
                new MySqlParameter("?did", distributorId),
                new MySqlParameter("?pid", productId),
                new MySqlParameter("?qty", qty),
                new MySqlParameter("?rmk", string.IsNullOrEmpty(remarks) ? (object)DBNull.Value : remarks),
                new MySqlParameter("?uid", userId));
        }

        // ── REPORTS ───────────────────────────────────────────────────
        public static DataTable GetStockMovementPurchases(DateTime from, DateTime to, int stateId, int cityId, int distId)
        {
            try
            {
                return ExecuteStoredProcedure("usp_GetStockMovementPurchases", new[] {
                    new MySqlParameter("p_From",    MySqlDbType.Date) { Value = from.Date },
                    new MySqlParameter("p_To",      MySqlDbType.Date) { Value = to.Date },
                    new MySqlParameter("p_StateID", MySqlDbType.Int32) { Value = stateId },
                    new MySqlParameter("p_CityID",  MySqlDbType.Int32) { Value = cityId },
                    new MySqlParameter("p_DistID",  MySqlDbType.Int32) { Value = distId }
                });
            }
            catch { return new DataTable(); }
        }

        public static DataTable GetStockMovementClosing(DateTime from, DateTime to, int stateId, int cityId, int distId)
        {
            try
            {
                return ExecuteStoredProcedure("usp_GetStockMovementClosing", new[] {
                    new MySqlParameter("p_From",    MySqlDbType.Date) { Value = from.Date },
                    new MySqlParameter("p_To",      MySqlDbType.Date) { Value = to.Date },
                    new MySqlParameter("p_StateID", MySqlDbType.Int32) { Value = stateId },
                    new MySqlParameter("p_CityID",  MySqlDbType.Int32) { Value = cityId },
                    new MySqlParameter("p_DistID",  MySqlDbType.Int32) { Value = distId }
                });
            }
            catch { return new DataTable(); }
        }

        public static DataTable GetDailySalesReport(DateTime from, DateTime to)
        {
            try
            {
                return ExecuteStoredProcedure("usp_GetDailySalesReport", new[] {
                    new MySqlParameter("p_From", MySqlDbType.Date) { Value = from.Date },
                    new MySqlParameter("p_To",   MySqlDbType.Date) { Value = to.Date }
                });
            }
            catch { return new DataTable(); }
        }

        public static DataTable GetDailySalesReportByUser(DateTime from, DateTime to, int userId, string role)
        {
            try
            {
                return ExecuteStoredProcedure("usp_GetDailySalesReportByUser", new[] {
                    new MySqlParameter("p_From",   MySqlDbType.Date)    { Value = from.Date },
                    new MySqlParameter("p_To",     MySqlDbType.Date)    { Value = to.Date },
                    new MySqlParameter("p_UserID", MySqlDbType.Int32)   { Value = userId },
                    new MySqlParameter("p_Role",   MySqlDbType.VarChar) { Value = role }
                });
            }
            catch { return new DataTable(); }
        }

        // ── DATA IMPORT ───────────────────────────────────────────────
        public static bool SalesOrderExists(string invoiceNo, string productName)
        {
            var dt = ExecuteQuery(
                "SELECT COUNT(*) FROM SalesOrders WHERE InvoiceNo=?inv AND UPPER(ProductName)=UPPER(?pn) LIMIT 1;",
                new MySqlParameter("?inv", invoiceNo),
                new MySqlParameter("?pn",  productName));
            return Convert.ToInt32(dt.Rows[0][0]) > 0;
        }

        public static void InsertSalesOrder(DateTime orderDate, string distributorName,
                                             string invoiceNo, string productName, int qty, decimal value)
        {
            ExecuteNonQuery(
                "INSERT INTO SalesOrders (OrderDate, DistributorName, InvoiceNo, ProductName, NoOfUnits, TotalValue) " +
                "VALUES (?d, ?n, ?inv, ?pn, ?qty, ?val);",
                new MySqlParameter("?d",   orderDate),
                new MySqlParameter("?n",   distributorName),
                new MySqlParameter("?inv", invoiceNo),
                new MySqlParameter("?pn",  string.IsNullOrEmpty(productName) ? (object)DBNull.Value : productName),
                new MySqlParameter("?qty", qty),
                new MySqlParameter("?val", value));
        }

        public static bool ReceiptExists(string vchNo, string particulars)
        {
            var dt = ExecuteQuery(
                "SELECT COUNT(*) FROM ReceiptRegister WHERE VchNo=?v AND UPPER(Particulars)=UPPER(?p) LIMIT 1;",
                new MySqlParameter("?v", vchNo),
                new MySqlParameter("?p", particulars));
            return Convert.ToInt32(dt.Rows[0][0]) > 0;
        }

        public static void InsertReceipt(DateTime receiptDate, string particulars,
                                          string vchNo, decimal credit)
        {
            ExecuteNonQuery(
                "INSERT INTO ReceiptRegister (ReceiptDate, Particulars, VchNo, CreditAmount) VALUES (?d, ?p, ?v, ?c);",
                new MySqlParameter("?d", receiptDate),
                new MySqlParameter("?p", particulars),
                new MySqlParameter("?v", vchNo),
                new MySqlParameter("?c", credit));
        }
    }
}
