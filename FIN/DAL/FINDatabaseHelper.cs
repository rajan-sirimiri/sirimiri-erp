using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

namespace FINApp.DAL
{
    public static class FINDatabaseHelper
    {
        private static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["StockDB"].ConnectionString;

        public static DateTime NowIST()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
        }
        public static DateTime TodayIST() => NowIST().Date;

        // ── PRIVATE HELPERS ──
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

        private static DataRow ExecuteQueryRow(string sql, params MySqlParameter[] parms)
        {
            var dt = ExecuteQuery(sql, parms);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public static DataTable ExecuteQueryPublic(string sql, params MySqlParameter[] parms)
        {
            return ExecuteQuery(sql, parms);
        }

        // ── AUTH ──
        public static DataRow ValidateUser(string username, string passwordHash)
        {
            return ExecuteQueryRow(
                "SELECT UserID, FullName, Username, Role, IsActive, MustChangePwd" +
                " FROM Users WHERE LOWER(Username)=LOWER(?u) AND PasswordHash=?p LIMIT 1;",
                new MySqlParameter("?u", username),
                new MySqlParameter("?p", passwordHash));
        }

        public static void UpdateLastLogin(int userId)
        {
            ExecuteNonQuery("UPDATE Users SET LastLogin=NOW() WHERE UserID=?id;",
                new MySqlParameter("?id", userId));
        }

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

        public static bool RoleHasAppAccess(string roleCode, string appCode)
        {
            if (roleCode == "Super") return true;
            try
            {
                var dt = ExecuteQuery(
                    "SELECT CanAccess FROM ERP_RoleAppAccess WHERE RoleCode=?rc AND AppCode=?ac;",
                    new MySqlParameter("?rc", roleCode), new MySqlParameter("?ac", appCode));
                return dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["CanAccess"]) == 1;
            }
            catch { return true; }
        }

        public static bool RoleHasModuleAccess(string roleCode, string appCode, string moduleCode)
        {
            if (roleCode == "Super") return true;
            try
            {
                var dt = ExecuteQuery(
                    "SELECT CanAccess FROM ERP_RoleModuleAccess WHERE RoleCode=?rc AND AppCode=?ac AND ModuleCode=?mc;",
                    new MySqlParameter("?rc", roleCode), new MySqlParameter("?ac", appCode),
                    new MySqlParameter("?mc", moduleCode));
                return dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["CanAccess"]) == 1;
            }
            catch { return true; }
        }

        // ══════════════════════════════════════════════════════════
        // TALLY MAPPING — PRODUCTS
        // ══════════════════════════════════════════════════════════

        public static DataTable GetAllProductMappings()
        {
            return ExecuteQuery(
                "SELECT m.MapID, m.TallyName, m.ProductID, m.SellingForm, m.PiecesPerUnit, m.MRP," +
                " p.ProductName, p.ProductCode" +
                " FROM FIN_TallyProductMap m" +
                " JOIN PP_Products p ON p.ProductID=m.ProductID" +
                " WHERE m.IsActive=1 ORDER BY m.TallyName;");
        }

        public static bool ProductMappingExists(string tallyName)
        {
            var dt = ExecuteQuery(
                "SELECT 1 FROM FIN_TallyProductMap WHERE TallyName=?tn AND IsActive=1 LIMIT 1;",
                new MySqlParameter("?tn", tallyName));
            return dt.Rows.Count > 0;
        }

        public static void SaveProductMapping(string tallyName, int productId, string sellingForm, int piecesPerUnit, decimal? mrp)
        {
            ExecuteNonQuery(
                "INSERT INTO FIN_TallyProductMap (TallyName, ProductID, SellingForm, PiecesPerUnit, MRP)" +
                " VALUES (?tn, ?pid, ?sf, ?ppu, ?mrp)" +
                " ON DUPLICATE KEY UPDATE ProductID=?pid2, SellingForm=?sf2, PiecesPerUnit=?ppu2, MRP=?mrp2;",
                new MySqlParameter("?tn", tallyName),
                new MySqlParameter("?pid", productId),
                new MySqlParameter("?sf", sellingForm),
                new MySqlParameter("?ppu", piecesPerUnit),
                new MySqlParameter("?mrp", mrp.HasValue ? (object)mrp.Value : DBNull.Value),
                new MySqlParameter("?pid2", productId),
                new MySqlParameter("?sf2", sellingForm),
                new MySqlParameter("?ppu2", piecesPerUnit),
                new MySqlParameter("?mrp2", mrp.HasValue ? (object)mrp.Value : DBNull.Value));
        }

        // ══════════════════════════════════════════════════════════
        // TALLY MAPPING — SCRAP
        // ══════════════════════════════════════════════════════════

        public static DataTable GetAllScrapMappings()
        {
            return ExecuteQuery(
                "SELECT m.MapID, m.TallyName, m.ScrapID, s.ScrapName, s.ScrapCode" +
                " FROM FIN_TallyScrapMap m" +
                " JOIN MM_ScrapMaterials s ON s.ScrapID=m.ScrapID" +
                " WHERE m.IsActive=1 ORDER BY m.TallyName;");
        }

        public static bool ScrapMappingExists(string tallyName)
        {
            var dt = ExecuteQuery(
                "SELECT 1 FROM FIN_TallyScrapMap WHERE TallyName=?tn AND IsActive=1 LIMIT 1;",
                new MySqlParameter("?tn", tallyName));
            return dt.Rows.Count > 0;
        }

        public static void SaveScrapMapping(string tallyName, int scrapId)
        {
            ExecuteNonQuery(
                "INSERT INTO FIN_TallyScrapMap (TallyName, ScrapID)" +
                " VALUES (?tn, ?sid)" +
                " ON DUPLICATE KEY UPDATE ScrapID=?sid2;",
                new MySqlParameter("?tn", tallyName),
                new MySqlParameter("?sid", scrapId),
                new MySqlParameter("?sid2", scrapId));
        }

        // ══════════════════════════════════════════════════════════
        // TALLY MAPPING — CUSTOMERS
        // ══════════════════════════════════════════════════════════

        public static DataTable GetAllCustomerMappings()
        {
            return ExecuteQuery(
                "SELECT m.MapID, m.TallyName, m.CustomerID, c.CustomerName, c.CustomerCode, c.CustomerType" +
                " FROM FIN_TallyCustomerMap m" +
                " JOIN PK_Customers c ON c.CustomerID=m.CustomerID" +
                " WHERE m.IsActive=1 ORDER BY m.TallyName;");
        }

        public static bool CustomerMappingExists(string tallyName)
        {
            var dt = ExecuteQuery(
                "SELECT 1 FROM FIN_TallyCustomerMap WHERE TallyName=?tn AND IsActive=1 LIMIT 1;",
                new MySqlParameter("?tn", tallyName));
            return dt.Rows.Count > 0;
        }

        public static void SaveCustomerMapping(string tallyName, int customerId)
        {
            ExecuteNonQuery(
                "INSERT INTO FIN_TallyCustomerMap (TallyName, CustomerID)" +
                " VALUES (?tn, ?cid)" +
                " ON DUPLICATE KEY UPDATE CustomerID=?cid2;",
                new MySqlParameter("?tn", tallyName),
                new MySqlParameter("?cid", customerId),
                new MySqlParameter("?cid2", customerId));
        }

        // ══════════════════════════════════════════════════════════
        // MASTER DATA LOOKUPS
        // ══════════════════════════════════════════════════════════

        public static DataTable GetAllProducts()
        {
            return ExecuteQuery(
                "SELECT ProductID, ProductCode, ProductName, ContainerType FROM PP_Products" +
                " WHERE IsActive=1 ORDER BY ProductName;");
        }

        /// Get all products with their FG packing options as a combined list
        public static DataTable GetProductsWithFGOptions()
        {
            return ExecuteQuery(
                "SELECT p.ProductID, p.ProductCode, p.ProductName," +
                " f.PackForm, f.UnitsPerPack, f.Description," +
                " CONCAT(p.ProductName, ' (', p.ProductCode, ') — ', f.PackForm, ' of ', f.UnitsPerPack) AS DisplayLabel" +
                " FROM PP_Products p" +
                " JOIN PP_FGPackingOptions f ON f.ProductID=p.ProductID AND f.IsActive=1" +
                " WHERE p.IsActive=1" +
                " ORDER BY p.ProductName, f.PackForm, f.UnitsPerPack;");
        }

        /// Get distinct selling forms: PCS + container types from PP_Products + CASE
        public static List<string> GetSellingForms()
        {
            var forms = new List<string>();
            forms.Add("PCS");
            var dt = ExecuteQuery(
                "SELECT DISTINCT ContainerType FROM PP_Products" +
                " WHERE ContainerType IS NOT NULL AND ContainerType != ''" +
                " ORDER BY ContainerType;");
            foreach (DataRow r in dt.Rows)
            {
                string ct = r["ContainerType"].ToString().Trim().ToUpper();
                if (!string.IsNullOrEmpty(ct) && !forms.Contains(ct))
                    forms.Add(ct);
            }
            if (!forms.Contains("CASE")) forms.Add("CASE");
            return forms;
        }

        public static DataTable GetAllScrapMaterials()
        {
            return ExecuteQuery(
                "SELECT ScrapID, ScrapCode, ScrapName FROM MM_ScrapMaterials" +
                " WHERE IsActive=1 ORDER BY ScrapName;");
        }

        public static DataTable GetAllCustomers()
        {
            return ExecuteQuery(
                "SELECT CustomerID, CustomerCode, CustomerName, CustomerType, City, PinCode FROM PK_Customers" +
                " WHERE IsActive=1 ORDER BY CustomerName;");
        }

        /// <summary>
        /// Auto-match Tally customer names to ERP customers.
        /// Step 1: Exact name match (case-insensitive, trimmed)
        /// Step 2: Normalized match (strip special chars, lowercase)
        /// Step 3: If multiple matches, disambiguate by pincode
        /// Returns count of auto-matched customers.
        /// </summary>
        public static int AutoMatchCustomers(List<string> tallyNames, Dictionary<string, string> tallyPincodes)
        {
            var erpCustomers = GetAllCustomers();
            int matched = 0;

            // Build ERP lookup: normalized name → list of (CustomerID, PinCode)
            var exactMap = new Dictionary<string, List<int[]>>(StringComparer.OrdinalIgnoreCase);
            var normMap = new Dictionary<string, List<int[]>>();

            foreach (DataRow r in erpCustomers.Rows)
            {
                int cid = Convert.ToInt32(r["CustomerID"]);
                string name = r["CustomerName"].ToString().Trim();
                string pin = r["PinCode"] != DBNull.Value ? r["PinCode"].ToString().Trim() : "";
                string norm = NormalizeName(name);

                // Exact (case-insensitive)
                if (!exactMap.ContainsKey(name)) exactMap[name] = new List<int[]>();
                exactMap[name].Add(new int[] { cid, pin.Length == 6 ? int.Parse(pin) : 0 });

                // Normalized
                if (!normMap.ContainsKey(norm)) normMap[norm] = new List<int[]>();
                normMap[norm].Add(new int[] { cid, pin.Length == 6 ? int.Parse(pin) : 0 });
            }

            foreach (string tallyName in tallyNames)
            {
                // Skip if already mapped
                if (CustomerMappingExists(tallyName)) continue;

                string tallyPin = tallyPincodes != null && tallyPincodes.ContainsKey(tallyName)
                    ? tallyPincodes[tallyName] : "";

                int matchedId = 0;

                // Step 1: Exact name match
                if (exactMap.ContainsKey(tallyName.Trim()))
                {
                    var candidates = exactMap[tallyName.Trim()];
                    if (candidates.Count == 1)
                        matchedId = candidates[0][0];
                    else if (candidates.Count > 1 && tallyPin.Length == 6)
                    {
                        // Multiple matches — try pincode
                        int pin = int.Parse(tallyPin);
                        foreach (var c in candidates)
                            if (c[1] == pin) { matchedId = c[0]; break; }
                        // If still no pin match, take first
                        if (matchedId == 0) matchedId = candidates[0][0];
                    }
                    else
                        matchedId = candidates[0][0];
                }

                // Step 2: Normalized match
                if (matchedId == 0)
                {
                    string norm = NormalizeName(tallyName);
                    if (normMap.ContainsKey(norm))
                    {
                        var candidates = normMap[norm];
                        if (candidates.Count == 1)
                            matchedId = candidates[0][0];
                        else if (candidates.Count > 1 && tallyPin.Length == 6)
                        {
                            int pin = int.Parse(tallyPin);
                            foreach (var c in candidates)
                                if (c[1] == pin) { matchedId = c[0]; break; }
                            if (matchedId == 0) matchedId = candidates[0][0];
                        }
                        else
                            matchedId = candidates[0][0];
                    }
                }

                if (matchedId > 0)
                {
                    SaveCustomerMapping(tallyName, matchedId);
                    matched++;
                }
            }

            return matched;
        }

        private static string NormalizeName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            var sb = new System.Text.StringBuilder();
            foreach (char c in name.ToLower())
                if (char.IsLetterOrDigit(c)) sb.Append(c);
            return sb.ToString();
        }

        // ══════════════════════════════════════════════════════════
        // SALES IMPORT
        // ══════════════════════════════════════════════════════════

        public static bool SalesInvoiceExists(string voucherNo)
        {
            var dt = ExecuteQuery(
                "SELECT 1 FROM FIN_SalesInvoice WHERE VoucherNo=?vn LIMIT 1;",
                new MySqlParameter("?vn", voucherNo));
            return dt.Rows.Count > 0;
        }

        public static int CreateSalesInvoice(string voucherNo, DateTime invoiceDate,
            int? customerId, string tallyCustomerName, string buyerAddress,
            decimal totalQty, decimal totalValue, int importBatchId)
        {
            ExecuteNonQuery(
                "INSERT INTO FIN_SalesInvoice (VoucherNo, InvoiceDate, CustomerID, TallyCustomerName," +
                " BuyerAddress, TotalQty, TotalValue, ImportBatchID)" +
                " VALUES (?vn, ?dt, ?cid, ?tcn, ?addr, ?qty, ?val, ?bid);",
                new MySqlParameter("?vn", voucherNo),
                new MySqlParameter("?dt", invoiceDate),
                new MySqlParameter("?cid", customerId.HasValue ? (object)customerId.Value : DBNull.Value),
                new MySqlParameter("?tcn", tallyCustomerName),
                new MySqlParameter("?addr", (object)buyerAddress ?? DBNull.Value),
                new MySqlParameter("?qty", totalQty),
                new MySqlParameter("?val", totalValue),
                new MySqlParameter("?bid", importBatchId));
            return Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
        }

        public static void AddSalesInvoiceLine(int invoiceId, int? productId, int? scrapId,
            string tallyProductName, string sellingForm, int piecesPerUnit,
            decimal qty, decimal value, string lineType)
        {
            ExecuteNonQuery(
                "INSERT INTO FIN_SalesInvoiceLine (InvoiceID, ProductID, ScrapID, TallyProductName," +
                " SellingForm, PiecesPerUnit, Quantity, Value, LineType)" +
                " VALUES (?iid, ?pid, ?sid, ?tpn, ?sf, ?ppu, ?qty, ?val, ?lt);",
                new MySqlParameter("?iid", invoiceId),
                new MySqlParameter("?pid", productId.HasValue ? (object)productId.Value : DBNull.Value),
                new MySqlParameter("?sid", scrapId.HasValue ? (object)scrapId.Value : DBNull.Value),
                new MySqlParameter("?tpn", tallyProductName),
                new MySqlParameter("?sf", sellingForm ?? "PCS"),
                new MySqlParameter("?ppu", piecesPerUnit),
                new MySqlParameter("?qty", qty),
                new MySqlParameter("?val", value),
                new MySqlParameter("?lt", lineType));
        }

        public static int CreateImportBatch(string importType, string fileName, int userId)
        {
            ExecuteNonQuery(
                "INSERT INTO FIN_ImportBatch (ImportType, FileName, ImportedBy) VALUES (?t, ?f, ?u);",
                new MySqlParameter("?t", importType),
                new MySqlParameter("?f", fileName),
                new MySqlParameter("?u", userId));
            return Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
        }

        public static void UpdateImportBatch(int batchId, int total, int inserted, int skipped, int errors)
        {
            ExecuteNonQuery(
                "UPDATE FIN_ImportBatch SET RowsTotal=?t, RowsInserted=?i, RowsSkipped=?s, RowsError=?e WHERE BatchID=?id;",
                new MySqlParameter("?t", total),
                new MySqlParameter("?i", inserted),
                new MySqlParameter("?s", skipped),
                new MySqlParameter("?e", errors),
                new MySqlParameter("?id", batchId));
        }

        public static DataTable GetImportBatches(string importType)
        {
            return ExecuteQuery(
                "SELECT b.*, u.FullName AS ImportedByName FROM FIN_ImportBatch b" +
                " LEFT JOIN Users u ON u.UserID=b.ImportedBy" +
                " WHERE b.ImportType=?t ORDER BY b.ImportedAt DESC LIMIT 20;",
                new MySqlParameter("?t", importType));
        }

        // ── Mapping lookup helpers for import ──

        public static DataRow GetProductMapping(string tallyName)
        {
            return ExecuteQueryRow(
                "SELECT m.ProductID, m.SellingForm, m.PiecesPerUnit FROM FIN_TallyProductMap m" +
                " WHERE m.TallyName=?tn AND m.IsActive=1 LIMIT 1;",
                new MySqlParameter("?tn", tallyName));
        }

        public static DataRow GetScrapMapping(string tallyName)
        {
            return ExecuteQueryRow(
                "SELECT m.ScrapID FROM FIN_TallyScrapMap m" +
                " WHERE m.TallyName=?tn AND m.IsActive=1 LIMIT 1;",
                new MySqlParameter("?tn", tallyName));
        }

        public static DataRow GetCustomerMapping(string tallyName)
        {
            return ExecuteQueryRow(
                "SELECT m.CustomerID FROM FIN_TallyCustomerMap m" +
                " WHERE m.TallyName=?tn AND m.IsActive=1 LIMIT 1;",
                new MySqlParameter("?tn", tallyName));
        }
    }
}
