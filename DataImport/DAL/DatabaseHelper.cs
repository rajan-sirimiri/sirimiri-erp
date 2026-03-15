using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace StockApp.DAL
{
    public static class DatabaseHelper
    {
        private static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["StockDBConnection"].ConnectionString;

        // ── PRIVATE HELPERS ──────────────────────────────────────────
        private static DataTable ExecuteQuery(string sql, params MySqlParameter[] parms)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    var da = new MySqlDataAdapter(cmd);
                    var dt = new DataTable();
                    da.Fill(dt);
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

        // ── AUTH ─────────────────────────────────────────────────────
        public static DataRow ValidateUser(string username, string passwordHash)
        {
            string sql = "SELECT UserID, FullName, Username, Role, StateID, IsActive, MustChangePwd " +
                         "FROM Users WHERE LOWER(Username)=LOWER(?u) AND PasswordHash=?p LIMIT 1;";
            DataTable dt = ExecuteQuery(sql,
                new MySqlParameter("?u", username),
                new MySqlParameter("?p", passwordHash));
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public static bool VerifyPassword(int userId, string passwordHash)
        {
            string sql = "SELECT COUNT(*) FROM Users WHERE UserID=?id AND PasswordHash=?p;";
            var dt = ExecuteQuery(sql,
                new MySqlParameter("?id", userId),
                new MySqlParameter("?p",  passwordHash));
            return Convert.ToInt32(dt.Rows[0][0]) > 0;
        }

        public static void ChangePassword(int userId, string newHash)
        {
            string sql = "UPDATE Users SET PasswordHash=?h, MustChangePwd=0 WHERE UserID=?id;";
            ExecuteNonQuery(sql,
                new MySqlParameter("?h",  newHash),
                new MySqlParameter("?id", userId));
        }

        public static void UpdateLastLogin(int userId)
        {
            ExecuteNonQuery("UPDATE Users SET LastLogin=NOW() WHERE UserID=?id;",
                new MySqlParameter("?id", userId));
        }

        public static void LogAudit(int userId, string action, int? distributorId,
                                    int? stockValue, string ip)
        {
            string sql = "INSERT INTO AuditLog (UserID, Action, DistributorID, StockValue, IPAddress, CreatedAt) " +
                         "VALUES (?uid, ?act, ?did, ?sv, ?ip, NOW());";
            ExecuteNonQuery(sql,
                new MySqlParameter("?uid", userId),
                new MySqlParameter("?act", action),
                new MySqlParameter("?did", (object)distributorId ?? DBNull.Value),
                new MySqlParameter("?sv",  (object)stockValue    ?? DBNull.Value),
                new MySqlParameter("?ip",  ip ?? ""));
        }

        // ── SALES IMPORT ─────────────────────────────────────────────
        public static bool SalesOrderExists(string invoiceNo, string productName)
        {
            string sql;
            MySqlParameter[] parms;
            if (string.IsNullOrEmpty(productName))
            {
                sql   = "SELECT COUNT(*) FROM SalesOrders WHERE InvoiceNo=?inv AND (ProductName IS NULL OR ProductName='') LIMIT 1;";
                parms = new[] { new MySqlParameter("?inv", invoiceNo) };
            }
            else
            {
                sql   = "SELECT COUNT(*) FROM SalesOrders WHERE InvoiceNo=?inv AND UPPER(ProductName)=UPPER(?pn) LIMIT 1;";
                parms = new[] { new MySqlParameter("?inv", invoiceNo), new MySqlParameter("?pn", productName) };
            }
            var dt = ExecuteQuery(sql, parms);
            return Convert.ToInt32(dt.Rows[0][0]) > 0;
        }

        public static void InsertSalesOrder(DateTime orderDate, string distributorName,
                                            string invoiceNo, string productName, int qty, decimal value)
        {
            string sql = @"INSERT INTO SalesOrders
                           (OrderDate, DistributorName, InvoiceNo, ProductName, NoOfUnits, TotalValue)
                           VALUES (?d, ?n, ?inv, ?pn, ?qty, ?val);";
            ExecuteNonQuery(sql,
                new MySqlParameter("?d",   orderDate),
                new MySqlParameter("?n",   distributorName),
                new MySqlParameter("?inv", invoiceNo),
                new MySqlParameter("?pn",  string.IsNullOrEmpty(productName) ? (object)DBNull.Value : productName),
                new MySqlParameter("?qty", qty),
                new MySqlParameter("?val", value));
        }

        // ── RECEIPTS IMPORT ──────────────────────────────────────────
        public static bool ReceiptExists(string vchNo, string particulars)
        {
            string sql = "SELECT COUNT(*) FROM ReceiptRegister " +
                         "WHERE VchNo=?v AND UPPER(Particulars)=UPPER(?p) LIMIT 1;";
            var dt = ExecuteQuery(sql,
                new MySqlParameter("?v", vchNo),
                new MySqlParameter("?p", particulars));
            return Convert.ToInt32(dt.Rows[0][0]) > 0;
        }

        public static void InsertReceipt(DateTime receiptDate, string particulars,
                                         string vchNo, decimal credit)
        {
            string sql = @"INSERT INTO ReceiptRegister
                           (ReceiptDate, Particulars, VchNo, CreditAmount)
                           VALUES (?d, ?p, ?v, ?c);";
            ExecuteNonQuery(sql,
                new MySqlParameter("?d", receiptDate),
                new MySqlParameter("?p", particulars),
                new MySqlParameter("?v", vchNo),
                new MySqlParameter("?c", credit));
        }

        // ── IMPORT LOG ───────────────────────────────────────────────
        public static void LogImport(int userId, string fileType,
                                     int inserted, int skipped, int errors, string ip)
        {
            string action = fileType + "_Import|New:" + inserted +
                            "|Skip:" + skipped + "|Err:" + errors;
            LogAudit(userId, action, null, null, ip);
        }
    }
}
