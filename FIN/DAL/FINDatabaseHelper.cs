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

        /// <summary>Central connection opener. Opens the connection and sets the
        /// MySQL session time zone to IST (+05:30) so NOW() / CURRENT_TIMESTAMP
        /// return Indian time regardless of the VPS server clock.
        /// Use this for any new code — existing scattered `new MySqlConnection`
        /// sites should be migrated over time.</summary>
        internal static MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            using (var tzCmd = new MySqlCommand("SET time_zone='+05:30';", conn))
            {
                tzCmd.ExecuteNonQuery();
            }
            return conn;
        }

        private static DataTable ExecuteQuery(string sql, params MySqlParameter[] parms)
        {
            using (var conn = OpenConnection())
            using (var cmd = new MySqlCommand(sql, conn))
            {
                if (parms != null) cmd.Parameters.AddRange(parms);
                var da = new MySqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        private static void ExecuteNonQuery(string sql, params MySqlParameter[] parms)
        {
            using (var conn = OpenConnection())
            using (var cmd = new MySqlCommand(sql, conn))
            {
                if (parms != null) cmd.Parameters.AddRange(parms);
                cmd.ExecuteNonQuery();
            }
        }

        private static object ExecuteScalar(string sql, params MySqlParameter[] parms)
        {
            using (var conn = OpenConnection())
            using (var cmd = new MySqlCommand(sql, conn))
            {
                if (parms != null) cmd.Parameters.AddRange(parms);
                return cmd.ExecuteScalar();
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

        // ══════════════════════════════════════════════════════════
        // PURCHASE ITEM MAPPING
        // ══════════════════════════════════════════════════════════

        public static DataTable GetAllItemMappings()
        {
            return ExecuteQuery(
                "SELECT m.MapID, m.TallyName, m.MaterialType, m.MaterialID," +
                " CASE m.MaterialType" +
                "   WHEN 'RM' THEN (SELECT CONCAT(r.RMName,' (',r.RMCode,')') FROM MM_RawMaterials r WHERE r.RMID=m.MaterialID)" +
                "   WHEN 'PM' THEN (SELECT CONCAT(p.PMName,' (',p.PMCode,')') FROM MM_PackingMaterials p WHERE p.PMID=m.MaterialID)" +
                "   WHEN 'CN' THEN (SELECT CONCAT(c.ConsumableName,' (',c.ConsumableCode,')') FROM MM_Consumables c WHERE c.ConsumableID=m.MaterialID)" +
                "   WHEN 'ST' THEN (SELECT CONCAT(s.StationaryName,' (',s.StationaryCode,')') FROM MM_Stationaries s WHERE s.StationaryID=m.MaterialID)" +
                "   WHEN 'SCRAP' THEN (SELECT CONCAT(sc.ScrapName,' (',sc.ScrapCode,')') FROM MM_ScrapMaterials sc WHERE sc.ScrapID=m.MaterialID)" +
                "   ELSE CONCAT(m.MaterialType, ' (unmapped)') END AS MaterialLabel" +
                " FROM FIN_TallyItemMap m WHERE m.IsActive=1 ORDER BY m.TallyName;");
        }

        public static bool ItemMappingExists(string tallyName)
        {
            object val = ExecuteScalar(
                "SELECT COUNT(*) FROM FIN_TallyItemMap WHERE TallyName=?tn AND IsActive=1;",
                new MySqlParameter("?tn", tallyName));
            return Convert.ToInt32(val) > 0;
        }

        public static void SaveItemMapping(string tallyName, string materialType, int? materialId)
        {
            ExecuteNonQuery(
                "INSERT INTO FIN_TallyItemMap (TallyName, MaterialType, MaterialID) " +
                "VALUES (?tn, ?mt, ?mid) " +
                "ON DUPLICATE KEY UPDATE MaterialType=VALUES(MaterialType), MaterialID=VALUES(MaterialID), IsActive=1;",
                new MySqlParameter("?tn", tallyName),
                new MySqlParameter("?mt", materialType),
                new MySqlParameter("?mid", materialId.HasValue ? (object)materialId.Value : DBNull.Value));
        }

        public static DataRow GetItemMapping(string tallyName)
        {
            return ExecuteQueryRow(
                "SELECT m.MaterialType, m.MaterialID FROM FIN_TallyItemMap m" +
                " WHERE m.TallyName=?tn AND m.IsActive=1 LIMIT 1;",
                new MySqlParameter("?tn", tallyName));
        }

        // ══════════════════════════════════════════════════════════
        // PURCHASE SUPPLIER MAPPING
        // ══════════════════════════════════════════════════════════

        public static DataTable GetAllSupplierMappings()
        {
            return ExecuteQuery(
                "SELECT m.MapID, m.TallyName, m.SupplierID," +
                " s.SupplierName, s.SupplierCode" +
                " FROM FIN_TallySupplierMap m" +
                " LEFT JOIN MM_Suppliers s ON s.SupplierID=m.SupplierID" +
                " WHERE m.IsActive=1 ORDER BY m.TallyName;");
        }

        public static bool SupplierMappingExists(string tallyName)
        {
            object val = ExecuteScalar(
                "SELECT COUNT(*) FROM FIN_TallySupplierMap WHERE TallyName=?tn AND IsActive=1;",
                new MySqlParameter("?tn", tallyName));
            return Convert.ToInt32(val) > 0;
        }

        public static void SaveSupplierMapping(string tallyName, int supplierId)
        {
            ExecuteNonQuery(
                "INSERT INTO FIN_TallySupplierMap (TallyName, SupplierID) " +
                "VALUES (?tn, ?sid) " +
                "ON DUPLICATE KEY UPDATE SupplierID=VALUES(SupplierID), IsActive=1;",
                new MySqlParameter("?tn", tallyName),
                new MySqlParameter("?sid", supplierId));
        }

        public static DataRow GetSupplierMapping(string tallyName)
        {
            return ExecuteQueryRow(
                "SELECT m.SupplierID FROM FIN_TallySupplierMap m" +
                " WHERE m.TallyName=?tn AND m.IsActive=1 LIMIT 1;",
                new MySqlParameter("?tn", tallyName));
        }

        public static int AutoMatchSuppliers(List<string> tallyNames)
        {
            int matched = 0;
            var erpSuppliers = ExecuteQuery("SELECT SupplierID, SupplierName FROM MM_Suppliers WHERE IsActive=1;");
            foreach (string tally in tallyNames)
            {
                if (SupplierMappingExists(tally)) continue;
                string tNorm = NormalizeName(tally);
                foreach (System.Data.DataRow r in erpSuppliers.Rows)
                {
                    string sNorm = NormalizeName(r["SupplierName"].ToString());
                    if (tNorm == sNorm)
                    {
                        SaveSupplierMapping(tally, Convert.ToInt32(r["SupplierID"]));
                        matched++;
                        break;
                    }
                }
            }
            return matched;
        }

        // ══════════════════════════════════════════════════════════
        // PURCHASE MATERIAL LOOKUPS
        // ══════════════════════════════════════════════════════════

        public static DataTable GetAllRawMaterials()
        {
            return ExecuteQuery(
                "SELECT RMID, RMCode, RMName FROM MM_RawMaterials WHERE IsActive=1 ORDER BY RMName;");
        }

        public static DataTable GetAllPackingMaterials()
        {
            return ExecuteQuery(
                "SELECT PMID, PMCode, PMName FROM MM_PackingMaterials WHERE IsActive=1 ORDER BY PMName;");
        }

        public static DataTable GetAllConsumables()
        {
            return ExecuteQuery(
                "SELECT ConsumableID, ConsumableCode, ConsumableName FROM MM_Consumables WHERE IsActive=1 ORDER BY ConsumableName;");
        }

        public static DataTable GetAllStationaries()
        {
            return ExecuteQuery(
                "SELECT StationaryID, StationaryCode, StationaryName FROM MM_Stationaries WHERE IsActive=1 ORDER BY StationaryName;");
        }

        public static DataTable GetAllSuppliers()
        {
            return ExecuteQuery(
                "SELECT SupplierID, SupplierCode, SupplierName" +
                " FROM MM_Suppliers WHERE IsActive=1 ORDER BY SupplierName;");
        }

        // ══════════════════════════════════════════════════════════
        // PURCHASE IMPORT
        // ══════════════════════════════════════════════════════════

        public static bool PurchaseInvoiceExists(string supplierInvNo)
        {
            object val = ExecuteScalar(
                "SELECT COUNT(*) FROM FIN_PurchaseInvoice WHERE SupplierInvNo=?inv;",
                new MySqlParameter("?inv", supplierInvNo));
            return Convert.ToInt32(val) > 0;
        }

        public static int CreatePurchaseInvoice(string supplierInvNo, DateTime invoiceDate,
            int? supplierId, string tallySupplierName,
            decimal totalQty, decimal totalValue, int batchId)
        {
            ExecuteNonQuery(
                "INSERT INTO FIN_PurchaseInvoice (SupplierInvNo, InvoiceDate, SupplierID," +
                " TallySupplierName, TotalQty, TotalValue, ImportBatchID)" +
                " VALUES (?inv,?dt,?sid,?tsn,?qty,?val,?bid);",
                new MySqlParameter("?inv", supplierInvNo),
                new MySqlParameter("?dt", invoiceDate.Date),
                new MySqlParameter("?sid", supplierId.HasValue ? (object)supplierId.Value : DBNull.Value),
                new MySqlParameter("?tsn", tallySupplierName ?? (object)DBNull.Value),
                new MySqlParameter("?qty", totalQty),
                new MySqlParameter("?val", totalValue),
                new MySqlParameter("?bid", batchId));
            return Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
        }

        public static void AddPurchaseInvoiceLine(int invoiceId, string materialType, int? materialId,
            string tallyItemName, decimal qty, decimal value)
        {
            ExecuteNonQuery(
                "INSERT INTO FIN_PurchaseInvoiceLine (InvoiceID, MaterialType, MaterialID," +
                " TallyProductName, Quantity, Value)" +
                " VALUES (?iid,?mt,?mid,?tin,?qty,?val);",
                new MySqlParameter("?iid", invoiceId),
                new MySqlParameter("?mt", materialType ?? (object)DBNull.Value),
                new MySqlParameter("?mid", materialId.HasValue ? (object)materialId.Value : DBNull.Value),
                new MySqlParameter("?tin", tallyItemName ?? (object)DBNull.Value),
                new MySqlParameter("?qty", qty),
                new MySqlParameter("?val", value));
        }

        // ══════════════════════════════════════════════════════════
        // RECEIPT IMPORT
        // ══════════════════════════════════════════════════════════

        public static bool ReceiptExists(string voucherNo, DateTime receiptDate)
        {
            object val = ExecuteScalar(
                "SELECT COUNT(*) FROM FIN_Receipt WHERE VoucherNo=?vn AND ReceiptDate=?dt;",
                new MySqlParameter("?vn", voucherNo),
                new MySqlParameter("?dt", receiptDate.Date));
            return Convert.ToInt32(val) > 0;
        }

        public static void CreateReceipt(string voucherNo, DateTime receiptDate,
            string tallyName, string receiptType, int? customerId, decimal amount, int batchId)
        {
            ExecuteNonQuery(
                "INSERT INTO FIN_Receipt (VoucherNo, ReceiptDate, TallyName, ReceiptType, CustomerID, Amount, ImportBatchID)" +
                " VALUES (?vn,?dt,?tn,?rt,?cid,?amt,?bid);",
                new MySqlParameter("?vn", voucherNo),
                new MySqlParameter("?dt", receiptDate.Date),
                new MySqlParameter("?tn", tallyName ?? (object)DBNull.Value),
                new MySqlParameter("?rt", receiptType),
                new MySqlParameter("?cid", customerId.HasValue ? (object)customerId.Value : DBNull.Value),
                new MySqlParameter("?amt", amount),
                new MySqlParameter("?bid", batchId));
        }

        /// Retroactively link receipts and invoices that have NULL CustomerID
        /// using existing TallyCustomerMap mappings
        public static int RepairNullCustomerLinks()
        {
            // Count before repair
            object cnt1 = ExecuteScalar(
                "SELECT COUNT(*) FROM FIN_Receipt r" +
                " JOIN FIN_TallyCustomerMap m ON m.TallyName=r.TallyName AND m.IsActive=1" +
                " WHERE r.CustomerID IS NULL AND r.ReceiptType='CUSTOMER' AND r.TallyName IS NOT NULL;");
            object cnt2 = ExecuteScalar(
                "SELECT COUNT(*) FROM FIN_SalesInvoice si" +
                " JOIN FIN_TallyCustomerMap m ON m.TallyName=si.TallyCustomerName AND m.IsActive=1" +
                " WHERE si.CustomerID IS NULL AND si.TallyCustomerName IS NOT NULL;");

            ExecuteNonQuery(
                "UPDATE FIN_Receipt r" +
                " JOIN FIN_TallyCustomerMap m ON m.TallyName=r.TallyName AND m.IsActive=1" +
                " SET r.CustomerID=m.CustomerID" +
                " WHERE r.CustomerID IS NULL AND r.ReceiptType='CUSTOMER' AND r.TallyName IS NOT NULL;");

            ExecuteNonQuery(
                "UPDATE FIN_SalesInvoice si" +
                " JOIN FIN_TallyCustomerMap m ON m.TallyName=si.TallyCustomerName AND m.IsActive=1" +
                " SET si.CustomerID=m.CustomerID" +
                " WHERE si.CustomerID IS NULL AND si.TallyCustomerName IS NOT NULL;");

            int fixed1 = cnt1 != null && cnt1 != DBNull.Value ? Convert.ToInt32(cnt1) : 0;
            int fixed2 = cnt2 != null && cnt2 != DBNull.Value ? Convert.ToInt32(cnt2) : 0;
            return fixed1 + fixed2;
        }

        /// Classify a Tally name as CUSTOMER, BANK, INTERNAL, or OTHER
        public static string ClassifyReceiptType(string tallyName)
        {
            if (string.IsNullOrEmpty(tallyName)) return "OTHER";
            string lower = tallyName.ToLower();

            // Bank entries
            if (lower.Contains("bank") || lower.Contains("a/c") || lower.Contains("current account") ||
                lower.Contains("uco ") || lower.Contains("axis ") || lower.Contains("yes bank") ||
                lower.Contains("hdfc") || lower.Contains("icici") || lower.Contains("sbi ") ||
                lower.Contains("canara") || lower.Contains("indian bank") || lower.Contains("interest on od"))
                return "BANK";

            // Internal entries
            if (lower.Contains("director loan") || lower.Contains("capital account") ||
                lower == "cash" || lower.Contains("petty cash") ||
                lower.Contains("travelling expense") || lower.Contains("travelling payable") ||
                lower.Contains("staffwelfare") || lower.Contains("salary") ||
                lower.Contains("income tax") || lower.Contains("tds"))
                return "INTERNAL";

            // Other income entries
            if (lower.Contains("scrap sales") || lower.Contains("other income") ||
                lower.Contains("razorpay") || lower.Contains("discount"))
                return "OTHER";

            return "CUSTOMER";
        }

        // ══════════════════════════════════════════════════════════
        // SALES ANALYTICS DASHBOARD
        // ══════════════════════════════════════════════════════════

        /// Monthly sales by state (all months available)
        public static DataTable GetMonthlySalesByState()
        {
            return ExecuteQuery(
                "SELECT c.State," +
                " DATE_FORMAT(si.InvoiceDate, '%Y-%m') AS Month," +
                " SUM(si.TotalValue) AS SalesValue," +
                " COUNT(DISTINCT si.InvoiceID) AS InvoiceCount" +
                " FROM FIN_SalesInvoice si" +
                " JOIN PK_Customers c ON c.CustomerID=si.CustomerID" +
                " WHERE c.State IS NOT NULL AND c.State != ''" +
                " GROUP BY c.State, DATE_FORMAT(si.InvoiceDate, '%Y-%m')" +
                " ORDER BY c.State, Month;");
        }

        /// Monthly sales by city/district within a state
        public static DataTable GetMonthlySalesByCity(string state)
        {
            return ExecuteQuery(
                "SELECT c.City," +
                " DATE_FORMAT(si.InvoiceDate, '%Y-%m') AS Month," +
                " SUM(si.TotalValue) AS SalesValue," +
                " COUNT(DISTINCT si.InvoiceID) AS InvoiceCount" +
                " FROM FIN_SalesInvoice si" +
                " JOIN PK_Customers c ON c.CustomerID=si.CustomerID" +
                " WHERE c.State=?state AND c.City IS NOT NULL AND c.City != ''" +
                " GROUP BY c.City, DATE_FORMAT(si.InvoiceDate, '%Y-%m')" +
                " ORDER BY c.City, Month;",
                new MySqlParameter("?state", state));
        }

        /// State-level summary totals
        public static DataTable GetStateSalesSummary()
        {
            return ExecuteQuery(
                "SELECT c.State," +
                " SUM(si.TotalValue) AS TotalSales," +
                " COUNT(DISTINCT si.InvoiceID) AS TotalInvoices," +
                " COUNT(DISTINCT si.CustomerID) AS TotalCustomers," +
                " MIN(si.InvoiceDate) AS FirstInvoice," +
                " MAX(si.InvoiceDate) AS LastInvoice" +
                " FROM FIN_SalesInvoice si" +
                " JOIN PK_Customers c ON c.CustomerID=si.CustomerID" +
                " WHERE c.State IS NOT NULL AND c.State != ''" +
                " GROUP BY c.State ORDER BY TotalSales DESC;");
        }

        /// Monthly product sales by state
        public static DataTable GetMonthlyProductSalesByState(string state)
        {
            return ExecuteQuery(
                "SELECT IFNULL(p.ProductName, sl.TallyProductName) AS ProductName," +
                " IFNULL(p.ProductCode, '') AS ProductCode," +
                " DATE_FORMAT(si.InvoiceDate, '%Y-%m') AS Month," +
                " SUM(sl.Value) AS SalesValue," +
                " SUM(sl.Quantity) AS SalesQty" +
                " FROM FIN_SalesInvoiceLine sl" +
                " JOIN FIN_SalesInvoice si ON si.InvoiceID=sl.InvoiceID" +
                " JOIN PK_Customers c ON c.CustomerID=si.CustomerID" +
                " LEFT JOIN PP_Products p ON p.ProductID=sl.ProductID" +
                " WHERE c.State=?state AND sl.LineType='PRODUCT'" +
                " GROUP BY IFNULL(p.ProductName, sl.TallyProductName), IFNULL(p.ProductCode, ''), DATE_FORMAT(si.InvoiceDate, '%Y-%m')" +
                " ORDER BY IFNULL(p.ProductName, sl.TallyProductName), Month;",
                new MySqlParameter("?state", state));
        }

        /// Monthly product sales by city within a state
        public static DataTable GetMonthlyProductSalesByCity(string state, string city)
        {
            return ExecuteQuery(
                "SELECT IFNULL(p.ProductName, sl.TallyProductName) AS ProductName," +
                " IFNULL(p.ProductCode, '') AS ProductCode," +
                " DATE_FORMAT(si.InvoiceDate, '%Y-%m') AS Month," +
                " SUM(sl.Value) AS SalesValue," +
                " SUM(sl.Quantity) AS SalesQty" +
                " FROM FIN_SalesInvoiceLine sl" +
                " JOIN FIN_SalesInvoice si ON si.InvoiceID=sl.InvoiceID" +
                " JOIN PK_Customers c ON c.CustomerID=si.CustomerID" +
                " LEFT JOIN PP_Products p ON p.ProductID=sl.ProductID" +
                " WHERE c.State=?state AND c.City=?city AND sl.LineType='PRODUCT'" +
                " GROUP BY IFNULL(p.ProductName, sl.TallyProductName), IFNULL(p.ProductCode, ''), DATE_FORMAT(si.InvoiceDate, '%Y-%m')" +
                " ORDER BY IFNULL(p.ProductName, sl.TallyProductName), Month;",
                new MySqlParameter("?state", state),
                new MySqlParameter("?city", city));
        }

        /// Product sales summary for a state (top products)
        public static DataTable GetProductSalesSummary(string state)
        {
            return ExecuteQuery(
                "SELECT IFNULL(p.ProductName, sl.TallyProductName) AS ProductName," +
                " IFNULL(p.ProductCode, '') AS ProductCode," +
                " SUM(sl.Value) AS TotalSales," +
                " SUM(sl.Quantity) AS TotalQty," +
                " COUNT(DISTINCT si.InvoiceID) AS InvoiceCount" +
                " FROM FIN_SalesInvoiceLine sl" +
                " JOIN FIN_SalesInvoice si ON si.InvoiceID=sl.InvoiceID" +
                " JOIN PK_Customers c ON c.CustomerID=si.CustomerID" +
                " LEFT JOIN PP_Products p ON p.ProductID=sl.ProductID" +
                " WHERE c.State=?state AND sl.LineType='PRODUCT'" +
                " GROUP BY IFNULL(p.ProductName, sl.TallyProductName), IFNULL(p.ProductCode, '')" +
                " ORDER BY TotalSales DESC;",
                new MySqlParameter("?state", state));
        }

        /// Distributors in a state with performance metrics
        public static DataTable GetDistributorPerformance(string state)
        {
            return ExecuteQuery(
                "SELECT c.CustomerID, c.CustomerName, c.CustomerType, c.City, c.PinCode," +
                " IFNULL(SUM(si.TotalValue), 0) AS TotalSales," +
                " COUNT(DISTINCT si.InvoiceID) AS TotalOrders," +
                " MIN(si.InvoiceDate) AS FirstOrder," +
                " MAX(si.InvoiceDate) AS LastOrder," +
                " COUNT(DISTINCT DATE_FORMAT(si.InvoiceDate, '%Y-%m')) AS ActiveMonths" +
                " FROM PK_Customers c" +
                " LEFT JOIN FIN_SalesInvoice si ON si.CustomerID=c.CustomerID" +
                " WHERE c.CustomerType IN ('DI','ST') AND c.State=?state AND c.IsActive=1" +
                " GROUP BY c.CustomerID, c.CustomerName, c.CustomerType, c.City, c.PinCode" +
                " ORDER BY TotalSales DESC;",
                new MySqlParameter("?state", state));
        }

        /// Products sold by a specific distributor
        public static DataTable GetDistributorProducts(int customerId)
        {
            return ExecuteQuery(
                "SELECT IFNULL(p.ProductName, sl.TallyProductName) AS ProductName," +
                " SUM(sl.Value) AS TotalSales," +
                " SUM(sl.Quantity) AS TotalQty," +
                " COUNT(DISTINCT si.InvoiceID) AS OrderCount" +
                " FROM FIN_SalesInvoiceLine sl" +
                " JOIN FIN_SalesInvoice si ON si.InvoiceID=sl.InvoiceID" +
                " LEFT JOIN PP_Products p ON p.ProductID=sl.ProductID" +
                " WHERE si.CustomerID=?cid" +
                " GROUP BY IFNULL(p.ProductName, sl.TallyProductName)" +
                " ORDER BY TotalSales DESC;",
                new MySqlParameter("?cid", customerId));
        }

        /// Monthly sales for a specific distributor
        public static DataTable GetDistributorMonthlySales(int customerId)
        {
            return ExecuteQuery(
                "SELECT DATE_FORMAT(si.InvoiceDate, '%Y-%m') AS Month," +
                " SUM(si.TotalValue) AS SalesValue," +
                " COUNT(DISTINCT si.InvoiceID) AS OrderCount" +
                " FROM FIN_SalesInvoice si" +
                " WHERE si.CustomerID=?cid" +
                " GROUP BY DATE_FORMAT(si.InvoiceDate, '%Y-%m')" +
                " ORDER BY Month;",
                new MySqlParameter("?cid", customerId));
        }

        /// Get distinct states that have sales data
        public static DataTable GetSalesStates()
        {
            return ExecuteQuery(
                "SELECT DISTINCT c.State" +
                " FROM FIN_SalesInvoice si" +
                " JOIN PK_Customers c ON c.CustomerID=si.CustomerID" +
                " WHERE c.State IS NOT NULL AND c.State != ''" +
                " ORDER BY c.State;");
        }

        /// Get distinct cities within a state that have sales data
        public static DataTable GetSalesCities(string state)
        {
            return ExecuteQuery(
                "SELECT DISTINCT c.City" +
                " FROM FIN_SalesInvoice si" +
                " JOIN PK_Customers c ON c.CustomerID=si.CustomerID" +
                " WHERE c.State=?state AND c.City IS NOT NULL AND c.City != ''" +
                " ORDER BY c.City;",
                new MySqlParameter("?state", state));
        }

        // ══════════════════════════════════════════════════════════════
        // FIN CONSIGNMENTS — Invoice Processing workflow
        // ══════════════════════════════════════════════════════════════
        // Finance team reviews consignments before dispatch. They can edit any DC
        // regardless of status, approve/unapprove DCs, and dispatch when ready.
        // PK_Consignments and PK_DeliveryChallans are shared with the PK module;
        // these helpers are FIN-specific read/write wrappers.

        /// <summary>Return consignments that finance should see in the Invoice Processing dashboard:
        /// everything except DISPATCHED and ARCHIVED (those live under Historical). Ordered by
        /// most recent first so the active work sits at the top.</summary>
        public static DataTable GetActiveConsignmentsForFIN()
        {
            return ExecuteQuery(
                "SELECT ConsignmentID, ConsignmentCode, ConsignmentDate, Status, UserText," +
                " IFNULL(TransportMode,'') AS TransportMode," +
                " IFNULL(CourierName,'') AS CourierName," +
                " IFNULL(TrackingNumber,'') AS TrackingNumber," +
                " IFNULL(VehicleNumber,'') AS VehicleNumber" +
                " FROM PK_Consignments" +
                " WHERE Status NOT IN ('DISPATCHED','ARCHIVED')" +
                " ORDER BY ConsignmentDate DESC, ConsignmentID DESC;");
        }

        /// <summary>DCs within a consignment with every field finance needs to review: customer,
        /// invoice, grand total, approval state, Zoho sync pointers. Used to render the DC list in
        /// the Invoice Processing dashboard.</summary>
        public static DataTable GetDCsByConsignmentForFIN(int consignmentId)
        {
            return ExecuteQuery(
                "SELECT d.DCID, d.DCNumber, d.DCDate, d.Status, d.GrandTotal, d.InvoiceNumber, d.Channel," +
                " d.ApprovedAt, d.ApprovedBy," +
                " u.FullName AS ApprovedByName," +
                " c.CustomerName, c.CustomerCode, c.CustomerType," +
                " IFNULL(c.GSTIN,'') AS GSTIN," +
                // Active e-invoice/EWB columns from FIN_EInvoiceLog (CancelledAt IS NULL)
                " ei.IRN, ei.AckNo, ei.AckDate, ei.Status AS EInvoiceStatus," +
                " ei.EWBNumber, ei.EWBDate, ei.EWBStatus" +
                " FROM PK_DeliveryChallans d" +
                " JOIN PK_Customers c ON c.CustomerID=d.CustomerID" +
                " LEFT JOIN users u ON u.UserID = d.ApprovedBy" +
                " LEFT JOIN FIN_EInvoiceLog ei ON ei.DCID = d.DCID AND ei.CancelledAt IS NULL" +
                " WHERE d.ConsignmentID=?cid" +
                " ORDER BY d.DCID;",
                new MySqlParameter("?cid", consignmentId));
        }

        /// <summary>Approve a DC (finance sign-off). Stores the approving user + timestamp.
        /// Overwrites any prior approval so the audit trail reflects the most recent approver.</summary>
        public static void ApproveDC(int dcId, int userId)
        {
            ExecuteNonQuery(
                "UPDATE PK_DeliveryChallans SET ApprovedAt=?now, ApprovedBy=?by WHERE DCID=?id;",
                new MySqlParameter("?now", NowIST()),
                new MySqlParameter("?by", userId),
                new MySqlParameter("?id", dcId));
        }

        /// <summary>Clear a DC's approval flag — finance can toggle it off if they approved by
        /// mistake. Also called automatically after any FIN-side edit of the DC so that content
        /// changes force a re-review.</summary>
        public static void UnapproveDC(int dcId)
        {
            ExecuteNonQuery(
                "UPDATE PK_DeliveryChallans SET ApprovedAt=NULL, ApprovedBy=NULL WHERE DCID=?id;",
                new MySqlParameter("?id", dcId));
        }

        /// <summary>Load full DC detail for the FIN edit panel — header + line items with pricing.
        /// Returns a DataSet with two tables: Header and Lines. Used by FINInvoiceProcessing's
        /// expandable detail panel.</summary>
        public static DataSet GetDCDetailForFIN(int dcId)
        {
            var ds = new DataSet();

            var header = ExecuteQuery(
                "SELECT d.DCID, d.DCNumber, d.DCDate, d.Status, d.Channel," +
                " d.SubTotal, d.TotalCGST, d.TotalSGST, d.TotalIGST, d.GrandTotal," +
                " d.IsInterState, d.InvoiceNumber, d.Remarks, d.ApprovedAt, d.ApprovedBy," +
                " c.CustomerName, c.CustomerCode, c.GSTIN, c.State" +
                " FROM PK_DeliveryChallans d" +
                " JOIN PK_Customers c ON c.CustomerID=d.CustomerID" +
                " WHERE d.DCID=?id;",
                new MySqlParameter("?id", dcId));
            header.TableName = "Header";
            ds.Tables.Add(header);

            var lines = ExecuteQuery(
                "SELECT dl.LineID, dl.ProductID, dl.SellingForm, dl.Source," +
                " p.ProductName, p.ProductCode," +
                " dl.TotalPcs AS Qty, dl.JarsPerCase, dl.HSNCode, dl.GSTRate," +
                " dl.MRP, dl.MarginPct, dl.UnitRate," +
                " dl.TaxableValue, dl.CGSTAmt, dl.SGSTAmt, dl.IGSTAmt, dl.LineTotal" +
                " FROM PK_DCLines dl" +
                " JOIN PP_Products p ON p.ProductID = dl.ProductID" +
                " WHERE dl.DCID=?id" +
                " ORDER BY dl.LineID;",
                new MySqlParameter("?id", dcId));
            lines.TableName = "Lines";
            ds.Tables.Add(lines);

            return ds;
        }

        /// <summary>FIN-side override edit of a single DC line. Bypasses the PK state-machine
        /// (DRAFT-only) rule — finance can edit FINALISED or CLOSED DCs too. Recomputes pricing
        /// from scratch using the new qty and given MRP/margin, then rolls the DC header totals
        /// to match. Auto-clears the DC's approval flag since the content changed.
        /// Zoho sync after edit is the user's responsibility (they see a warning banner in UI).</summary>
        public static void UpdateDCLineFromFIN(int lineId, int qty, decimal mrp, decimal marginPct,
            string hsn, decimal gstRate, int userId)
        {
            // Look up parent DC to know interstate flag
            var row = ExecuteQueryRow(
                "SELECT dl.DCID, d.IsInterState FROM PK_DCLines dl" +
                " JOIN PK_DeliveryChallans d ON d.DCID = dl.DCID" +
                " WHERE dl.LineID=?lid;",
                new MySqlParameter("?lid", lineId));
            if (row == null) throw new Exception("DC line not found.");

            int dcId = Convert.ToInt32(row["DCID"]);
            bool isInterState = Convert.ToInt32(row["IsInterState"]) == 1;

            // E-invoice guard: if an active IRN exists, the invoice is locked at NIC IRP and
            // cannot be silently edited. Finance must cancel the e-invoice (within 24 hrs)
            // and re-push after editing. We refuse the edit here with an actionable message.
            string irn = GetActiveIRN(dcId);
            if (!string.IsNullOrEmpty(irn))
                throw new Exception(
                    "Cannot edit — this DC has an active e-invoice (IRN " +
                    (irn.Length > 12 ? irn.Substring(0, 12) + "..." : irn) +
                    "). Cancel the e-invoice in Zoho Books first (within 24 hrs of generation), " +
                    "record the cancellation in ERP, then edit the DC and re-push.");

            // Recompute line pricing from first principles (same formulas PK DC form uses)
            decimal unitRate  = Math.Round(mrp * (1m - marginPct / 100m), 2, MidpointRounding.AwayFromZero);
            decimal lineTotal = Math.Round(unitRate * qty, 2, MidpointRounding.AwayFromZero);
            decimal taxableVal = gstRate > 0
                ? Math.Round(lineTotal / (1m + gstRate / 100m), 2, MidpointRounding.AwayFromZero)
                : lineTotal;
            decimal gstAmt  = Math.Round(lineTotal - taxableVal, 2, MidpointRounding.AwayFromZero);
            decimal cgstAmt = isInterState ? 0m : Math.Round(gstAmt / 2m, 2, MidpointRounding.AwayFromZero);
            decimal sgstAmt = isInterState ? 0m : Math.Round(gstAmt / 2m, 2, MidpointRounding.AwayFromZero);
            decimal igstAmt = isInterState ? gstAmt : 0m;

            ExecuteNonQuery(
                "UPDATE PK_DCLines SET TotalPcs=?q, MRP=?mrp, MarginPct=?mgn, UnitRate=?rate," +
                " HSNCode=?hsn, GSTRate=?gst, TaxableValue=?tax, CGSTAmt=?cgst, SGSTAmt=?sgst," +
                " IGSTAmt=?igst, LineTotal=?lt WHERE LineID=?lid;",
                new MySqlParameter("?q", qty),
                new MySqlParameter("?mrp", mrp),
                new MySqlParameter("?mgn", marginPct),
                new MySqlParameter("?rate", unitRate),
                new MySqlParameter("?hsn", hsn ?? ""),
                new MySqlParameter("?gst", gstRate),
                new MySqlParameter("?tax", taxableVal),
                new MySqlParameter("?cgst", cgstAmt),
                new MySqlParameter("?sgst", sgstAmt),
                new MySqlParameter("?igst", igstAmt),
                new MySqlParameter("?lt", lineTotal),
                new MySqlParameter("?lid", lineId));

            // Roll up DC header totals from all lines (avoid drift)
            RecomputeDCHeaderTotals(dcId);

            // Edit invalidates prior approval — finance must re-review
            UnapproveDC(dcId);
        }

        /// <summary>Delete a DC line (FIN override). Finance can remove a line that shouldn't be
        /// invoiced. Rolls up header totals and clears approval.</summary>
        public static void DeleteDCLineFromFIN(int lineId, int userId)
        {
            var row = ExecuteQueryRow(
                "SELECT DCID FROM PK_DCLines WHERE LineID=?lid;",
                new MySqlParameter("?lid", lineId));
            if (row == null) throw new Exception("DC line not found.");
            int dcId = Convert.ToInt32(row["DCID"]);

            // Same e-invoice guard as edit
            string irn = GetActiveIRN(dcId);
            if (!string.IsNullOrEmpty(irn))
                throw new Exception(
                    "Cannot delete line — DC has an active e-invoice (IRN " +
                    (irn.Length > 12 ? irn.Substring(0, 12) + "..." : irn) +
                    "). Cancel the e-invoice in Zoho first, record the cancellation in ERP, then edit.");

            ExecuteNonQuery("DELETE FROM PK_DCLines WHERE LineID=?lid;",
                new MySqlParameter("?lid", lineId));

            RecomputeDCHeaderTotals(dcId);
            UnapproveDC(dcId);
        }

        /// <summary>Sum up a DC's lines into its header totals. Called after any FIN-side line edit
        /// or delete so SubTotal/CGST/SGST/IGST/GrandTotal reflect actual lines.</summary>
        private static void RecomputeDCHeaderTotals(int dcId)
        {
            ExecuteNonQuery(
                "UPDATE PK_DeliveryChallans dc SET" +
                " SubTotal   = IFNULL((SELECT SUM(TaxableValue) FROM PK_DCLines WHERE DCID=?id), 0)," +
                " TotalCGST  = IFNULL((SELECT SUM(CGSTAmt)      FROM PK_DCLines WHERE DCID=?id), 0)," +
                " TotalSGST  = IFNULL((SELECT SUM(SGSTAmt)      FROM PK_DCLines WHERE DCID=?id), 0)," +
                " TotalIGST  = IFNULL((SELECT SUM(IGSTAmt)      FROM PK_DCLines WHERE DCID=?id), 0)," +
                " GrandTotal = IFNULL((SELECT SUM(LineTotal)    FROM PK_DCLines WHERE DCID=?id), 0)" +
                " WHERE dc.DCID=?id;",
                new MySqlParameter("?id", dcId));
        }

        /// <summary>FIN-side Mark READY — same outcome as the PK-side MarkConsignmentReady.
        /// Requires status=OPEN and every DC to be FINALISED. Throws if any DC is still DRAFT.</summary>
        public static void MarkConsignmentReadyFromFIN(int consignmentId)
        {
            var csg = ExecuteQueryRow(
                "SELECT Status FROM PK_Consignments WHERE ConsignmentID=?id;",
                new MySqlParameter("?id", consignmentId));
            if (csg == null) throw new Exception("Consignment not found.");
            if (csg["Status"].ToString() != "OPEN")
                throw new Exception("Only OPEN consignments can be marked READY (current: " + csg["Status"] + ").");

            var draftCount = ExecuteScalar(
                "SELECT COUNT(*) FROM PK_DeliveryChallans WHERE ConsignmentID=?id AND Status='DRAFT';",
                new MySqlParameter("?id", consignmentId));
            if (draftCount != null && Convert.ToInt64(draftCount) > 0)
                throw new Exception("Cannot mark READY — one or more DCs are still in DRAFT.");

            ExecuteNonQuery(
                "UPDATE PK_Consignments SET Status='READY' WHERE ConsignmentID=?id AND Status='OPEN';",
                new MySqlParameter("?id", consignmentId));
        }

        /// <summary>Dispatch a consignment from the FIN side. Identical outcome to the PK-side
        /// dispatch — both call the shared state-change. Requires consignment=READY AND every
        /// DC=FINALISED (enforced by the shared rule). Finance approval is NOT required — it's
        /// informational per the rules. Cascade: every FINALISED DC in the consignment flips to
        /// CLOSED as part of dispatch.</summary>
        public static void DispatchConsignmentFromFIN(int consignmentId, string vehicleNumber)
        {
            // Precondition check using the same rules as PK
            var csg = ExecuteQueryRow(
                "SELECT Status FROM PK_Consignments WHERE ConsignmentID=?id;",
                new MySqlParameter("?id", consignmentId));
            if (csg == null) throw new Exception("Consignment not found.");
            if (csg["Status"].ToString() != "READY")
                throw new Exception("Consignment must be READY for dispatch (current: " + csg["Status"] + ").");

            var draftCount = ExecuteScalar(
                "SELECT COUNT(*) FROM PK_DeliveryChallans WHERE ConsignmentID=?id AND Status='DRAFT';",
                new MySqlParameter("?id", consignmentId));
            if (draftCount != null && Convert.ToInt64(draftCount) > 0)
                throw new Exception("Dispatch blocked — one or more DCs are still in DRAFT.");

            ExecuteNonQuery(
                "UPDATE PK_Consignments SET Status='DISPATCHED', VehicleNumber=?vn, DispatchedAt=NOW()" +
                " WHERE ConsignmentID=?id AND Status='READY';",
                new MySqlParameter("?vn", vehicleNumber ?? ""),
                new MySqlParameter("?id", consignmentId));

            // Cascade DCs to CLOSED
            ExecuteNonQuery(
                "UPDATE PK_DeliveryChallans SET Status='CLOSED' WHERE ConsignmentID=?id AND Status='FINALISED';",
                new MySqlParameter("?id", consignmentId));
        }

        /// <summary>Historical consignments for the Historical Consignments card — dispatched
        /// and archived, ordered most-recently-dispatched first.</summary>
        public static DataTable GetHistoricalConsignments(int limit = 100)
        {
            return ExecuteQuery(
                "SELECT ConsignmentID, ConsignmentCode, ConsignmentDate, Status, UserText," +
                " IFNULL(VehicleNumber,'') AS VehicleNumber, DispatchedAt, ArchivedAt" +
                " FROM PK_Consignments" +
                " WHERE Status IN ('DISPATCHED','ARCHIVED')" +
                " ORDER BY IFNULL(DispatchedAt, CreatedAt) DESC" +
                " LIMIT ?lim;",
                new MySqlParameter("?lim", limit));
        }

        // ══════════════════════════════════════════════════════════════
        // FIN E-INVOICE / E-WAY BILL — deep-link era
        // ══════════════════════════════════════════════════════════════
        // Phase 1 design: ERP doesn't call Zoho's e-invoicing REST endpoints
        // (paths aren't publicly documented). Instead, finance clicks "Push to
        // IRP" inside Zoho Books (via deep link) and pastes the IRN/ACK/QR
        // back into ERP through this layer. Phase 2 will swap manual entry
        // for automated API calls without changing the table or this surface.

        /// <summary>Return the active e-invoice log row for a DC (CancelledAt IS NULL),
        /// or NULL if none exists. There is at most one active row per DC; cancelled rows
        /// are kept for audit but not returned by this method.</summary>
        public static DataRow GetActiveEInvoice(int dcId)
        {
            return ExecuteQueryRow(
                "SELECT * FROM FIN_EInvoiceLog WHERE DCID=?dc AND CancelledAt IS NULL" +
                " ORDER BY LogID DESC LIMIT 1;",
                new MySqlParameter("?dc", dcId));
        }

        /// <summary>Look up the Zoho invoice id for a DC. Used to build the deep-link
        /// to Zoho Books for the "Push to IRP in Zoho" button.</summary>
        public static string GetZohoInvoiceID(int dcId)
        {
            var row = ExecuteQueryRow(
                "SELECT ZohoInvoiceID FROM zoho_invoicelog WHERE DCID=?dc AND ZohoInvoiceID IS NOT NULL" +
                " ORDER BY LogID DESC LIMIT 1;",
                new MySqlParameter("?dc", dcId));
            return row != null && row["ZohoInvoiceID"] != DBNull.Value
                ? row["ZohoInvoiceID"].ToString() : "";
        }

        /// <summary>Return the Zoho organization ID from Zoho_Config — needed to construct
        /// deep links of the form https://books.zoho.in/app/{orgId}/invoices/{invId}.</summary>
        public static string GetZohoOrgID()
        {
            var row = ExecuteQueryRow(
                "SELECT OrganizationID FROM Zoho_Config WHERE ConfigID=1;");
            return row != null && row["OrganizationID"] != DBNull.Value
                ? row["OrganizationID"].ToString() : "";
        }

        /// <summary>Record an e-invoice generation that finance performed inside Zoho. They
        /// paste the IRN, ACK number, and ACK date back from Zoho's invoice page. This is
        /// the manual fallback for Phase 1 (no REST API yet). If a prior cancelled row
        /// exists for this DC, a new row is created — the old one stays for audit.</summary>
        public static void RecordEInvoiceManual(int dcId, string irn, string ackNo, DateTime? ackDate,
            string zohoInvoiceId, int userId)
        {
            // Defensive: refuse if there's already an active (non-cancelled) row
            var existing = GetActiveEInvoice(dcId);
            if (existing != null)
                throw new Exception("An active e-invoice already exists for this DC. Cancel it first before recording a new one.");

            ExecuteNonQuery(
                "INSERT INTO FIN_EInvoiceLog (DCID, ZohoInvoiceID, IRN, AckNo, AckDate, Status," +
                " GeneratedAt, GeneratedBy)" +
                " VALUES (?dc, ?zid, ?irn, ?ack, ?ackdt, 'GENERATED', ?now, ?by);",
                new MySqlParameter("?dc", dcId),
                new MySqlParameter("?zid", zohoInvoiceId ?? ""),
                new MySqlParameter("?irn", irn ?? ""),
                new MySqlParameter("?ack", ackNo ?? ""),
                new MySqlParameter("?ackdt", (object)ackDate ?? DBNull.Value),
                new MySqlParameter("?now", NowIST()),
                new MySqlParameter("?by", userId));
        }

        /// <summary>Mark an active e-invoice as cancelled (ERP-side log only — finance must
        /// also cancel inside Zoho/IRP, since we don't have API access yet). Stores reason
        /// per NIC's enumeration: 1=Duplicate, 2=Data entry mistake, 3=Order cancelled,
        /// 4=Others. Reason text is also stored verbatim so it shows up in audit reports.</summary>
        public static void CancelEInvoiceManual(int dcId, string reason, int userId)
        {
            var active = GetActiveEInvoice(dcId);
            if (active == null) throw new Exception("No active e-invoice found for this DC.");

            ExecuteNonQuery(
                "UPDATE FIN_EInvoiceLog SET Status='CANCELLED', CancelledAt=?now," +
                " CancelledBy=?by, CancellationReason=?rsn" +
                " WHERE LogID=?id;",
                new MySqlParameter("?now", NowIST()),
                new MySqlParameter("?by", userId),
                new MySqlParameter("?rsn", reason ?? ""),
                new MySqlParameter("?id", Convert.ToInt32(active["LogID"])));
        }

        /// <summary>Record an e-way bill against an active e-invoice. EWB is a separate
        /// NIC artefact (EWB number, date, validity). Lives on the same row as the IRN
        /// since EWB is per-invoice. If no active e-invoice exists yet, refuses — EWB
        /// must be linked to an IRN.</summary>
        public static void RecordEWayBillManual(int dcId, string ewbNo, DateTime ewbDate,
            DateTime? validUpto, int userId)
        {
            var active = GetActiveEInvoice(dcId);
            if (active == null)
                throw new Exception("Generate the e-invoice first — e-way bill must be linked to an IRN.");

            ExecuteNonQuery(
                "UPDATE FIN_EInvoiceLog SET EWBNumber=?ewb, EWBDate=?dt, EWBValidUpto=?valid," +
                " EWBStatus='GENERATED' WHERE LogID=?id;",
                new MySqlParameter("?ewb", ewbNo ?? ""),
                new MySqlParameter("?dt", ewbDate),
                new MySqlParameter("?valid", (object)validUpto ?? DBNull.Value),
                new MySqlParameter("?id", Convert.ToInt32(active["LogID"])));
        }

        /// <summary>Mark an EWB as cancelled (ERP-side). Mirror of CancelEInvoiceManual but
        /// EWB-only — the IRN stays GENERATED.</summary>
        public static void CancelEWayBillManual(int dcId, string reason, int userId)
        {
            var active = GetActiveEInvoice(dcId);
            if (active == null) throw new Exception("No active e-invoice for this DC.");
            if (active["EWBNumber"] == DBNull.Value || string.IsNullOrEmpty(active["EWBNumber"].ToString()))
                throw new Exception("No e-way bill recorded for this DC.");

            ExecuteNonQuery(
                "UPDATE FIN_EInvoiceLog SET EWBStatus='CANCELLED', EWBCancelledAt=?now," +
                " EWBCancellationReason=?rsn WHERE LogID=?id;",
                new MySqlParameter("?now", NowIST()),
                new MySqlParameter("?rsn", reason ?? ""),
                new MySqlParameter("?id", Convert.ToInt32(active["LogID"])));
        }

        /// <summary>Eligibility check: is this DC's customer B2B (has GSTIN) such that
        /// e-invoicing is required? Per current rules, Sirimiri (>5 Cr aggregate turnover)
        /// must e-invoice every B2B sale. B2C (no GSTIN) is exempt.</summary>
        public static bool IsEInvoiceRequired(int dcId)
        {
            var row = ExecuteQueryRow(
                "SELECT IFNULL(c.GSTIN,'') AS GSTIN" +
                " FROM PK_DeliveryChallans d" +
                " JOIN PK_Customers c ON c.CustomerID=d.CustomerID" +
                " WHERE d.DCID=?dc;",
                new MySqlParameter("?dc", dcId));
            if (row == null) return false;
            string gstin = row["GSTIN"].ToString().Trim();
            // GSTIN is 15 chars (2-digit state + 10-digit PAN + 3 alphanumeric).
            // We treat any non-empty GSTIN as B2B; deeper validation is Zoho's job.
            return gstin.Length >= 15;
        }

        /// <summary>Returns the active IRN for a DC, or empty string. Used by the FIN
        /// edit guard ("if IRN exists, refuse line edits and ask finance to cancel first").</summary>
        public static string GetActiveIRN(int dcId)
        {
            var row = GetActiveEInvoice(dcId);
            return row != null && row["IRN"] != DBNull.Value ? row["IRN"].ToString() : "";
        }

        // ══════════════════════════════════════════════════════════════
        // SYNC FROM ZOHO
        // ══════════════════════════════════════════════════════════════
        // Wraps the existing PK-side FINApp.DAL.FINZohoHelper.SyncConsignmentBack so
        // FIN doesn't duplicate the sync logic. The PK helper walks every DC in the
        // consignment, fetches the latest invoice from Zoho, and overwrites ERP
        // header + line item data when it differs (with stock alerts when qty changes
        // would breach FG stock). Returns a structured per-DC result list.
        //
        // What this does NOT sync (yet): IRN / ACK / QR code from Zoho's e-invoicing
        // API. Those endpoints aren't publicly documented; finance must record IRN
        // manually via the Record IRN modal until Phase 2 brings real API coverage.

        /// <summary>Sync every DC in a consignment back from Zoho — header + line items.
        /// Wrapper that delegates to PK's existing implementation.</summary>
        public static System.Collections.Generic.List<FINApp.DAL.ZohoSyncBackResult> SyncConsignmentFromZoho(int consignmentId)
        {
            return FINApp.DAL.FINZohoHelper.SyncConsignmentBack(consignmentId);
        }

        /// <summary>Sync a single DC back from Zoho — header + line items for that one DC.
        /// Used by the per-DC "Sync" button.</summary>
        public static FINApp.DAL.ZohoSyncBackResult SyncSingleDCFromZoho(int dcId)
        {
            return FINApp.DAL.FINZohoHelper.SyncInvoiceBack(dcId);
        }

        // ══════════════════════════════════════════════════════════════
        // GRN → ZOHO BILLS
        // ══════════════════════════════════════════════════════════════
        // Phase 1: Raw and Packing only. List pending GRNs (real vendor
        // purchases), show push status, let finance click to push.
        // Synthetic GRNs (INT-/PREP-/PRE- prefixes, or SupplierID=306) are
        // filtered out of the list at the SQL level.

        /// <summary>List raw-material GRNs that are real vendor purchases, joined with
        /// supplier and material names and any existing zoho_billlog row. Filters out
        /// internal/preprocess/prefilled synthetic entries.</summary>
        public static DataTable GetRawGRNsForBilling(DateTime? fromDate, DateTime? toDate, string pushFilter)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(
                "SELECT i.InwardID, i.GRNNo, i.InvoiceNo, i.InvoiceDate, i.InwardDate, " +
                " i.SupplierID, s.SupplierName, s.State AS SupState, s.GSTNo AS SupGSTIN, " +
                " i.RMID, rm.RMName, rm.RMCode, rm.HSNCode AS RMHSN, " +
                " u.Abbreviation AS Unit, " +
                " i.Quantity, i.QtyActualReceived, i.Rate, i.HSNCode, i.GSTRate, i.GSTAmount, i.Amount, i.Status, " +
                " i.PONo, i.Remarks, i.TransportCost, i.TransportInInvoice, i.TransportInGST, " +
                " i.LoadingCharges, i.UnloadingCharges, i.ShortageQty, i.ShortageValue, " +
                " i.QtyVerified, i.QualityCheck, i.CreatedBy, i.CreatedAt, " +
                " usr.FullName AS CreatedByName, " +
                " bl.ZohoBillID, bl.ZohoBillNo, bl.ZohoStatus, bl.PushStatus, bl.ErrorMessage, " +
                " bl.PushedAt, bl.BillTotal " +
                "FROM mm_rawinward i " +
                "JOIN mm_suppliers s ON s.SupplierID = i.SupplierID " +
                "JOIN mm_rawmaterials rm ON rm.RMID = i.RMID " +
                "LEFT JOIN mm_uom u ON u.UOMID = rm.UOMID " +
                "LEFT JOIN Users usr ON usr.UserID = i.CreatedBy " +
                "LEFT JOIN zoho_billlog bl ON bl.GRNID = i.InwardID AND bl.GRNType = 'RAW' " +
                "WHERE i.SupplierID <> 306 " +
                "  AND i.GRNNo NOT LIKE 'INT-%' " +
                "  AND i.GRNNo NOT LIKE 'PREP-%' " +
                "  AND i.GRNNo NOT LIKE 'PRE-%' ");

            if (fromDate.HasValue) sb.Append(" AND i.InwardDate >= ?fd ");
            if (toDate.HasValue)   sb.Append(" AND i.InwardDate <= ?td ");
            if (pushFilter == "Pending")
                sb.Append(" AND (bl.ZohoBillID IS NULL OR bl.ZohoBillID = '') ");
            else if (pushFilter == "Pushed")
                sb.Append(" AND bl.ZohoBillID IS NOT NULL AND bl.ZohoBillID <> '' ");
            else if (pushFilter == "Error")
                sb.Append(" AND bl.PushStatus = 'Error' ");

            sb.Append("ORDER BY i.InwardDate DESC, i.InwardID DESC;");

            using (var conn = new MySqlConnection(ConnectionString))
            using (var cmd = new MySqlCommand(sb.ToString(), conn))
            {
                if (fromDate.HasValue) cmd.Parameters.AddWithValue("?fd", fromDate.Value);
                if (toDate.HasValue) cmd.Parameters.AddWithValue("?td", toDate.Value);
                var dt = new DataTable();
                conn.Open();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }

        /// <summary>List packing-material GRNs that are real vendor purchases.
        /// Mirrors GetRawGRNsForBilling.</summary>
        public static DataTable GetPackingGRNsForBilling(DateTime? fromDate, DateTime? toDate, string pushFilter)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(
                "SELECT i.InwardID, i.GRNNo, i.InvoiceNo, i.InvoiceDate, i.InwardDate, " +
                " i.SupplierID, s.SupplierName, s.State AS SupState, s.GSTNo AS SupGSTIN, " +
                " i.PMID, pm.PMName, pm.PMCode, pm.PMCategory, pm.HSNCode AS PMHSN, " +
                " u.Abbreviation AS Unit, " +
                " i.Quantity, i.QtyActualReceived, i.Rate, i.HSNCode, i.GSTRate, i.GSTAmount, i.Amount, i.Status, " +
                " i.PONo, i.Remarks, i.TransportCost, i.TransportInInvoice, i.TransportInGST, " +
                " i.LoadingCharges, i.UnloadingCharges, i.ShortageQty, i.ShortageValue, " +
                " i.QtyVerified, i.QualityCheck, i.CreatedBy, i.CreatedAt, " +
                " usr.FullName AS CreatedByName, " +
                " bl.ZohoBillID, bl.ZohoBillNo, bl.ZohoStatus, bl.PushStatus, bl.ErrorMessage, " +
                " bl.PushedAt, bl.BillTotal " +
                "FROM mm_packinginward i " +
                "JOIN mm_suppliers s ON s.SupplierID = i.SupplierID " +
                "JOIN mm_packingmaterials pm ON pm.PMID = i.PMID " +
                "LEFT JOIN mm_uom u ON u.UOMID = pm.UOMID " +
                "LEFT JOIN Users usr ON usr.UserID = i.CreatedBy " +
                "LEFT JOIN zoho_billlog bl ON bl.GRNID = i.InwardID AND bl.GRNType = 'PACKING' " +
                "WHERE i.SupplierID <> 306 " +
                "  AND i.GRNNo NOT LIKE 'INT-%' " +
                "  AND i.GRNNo NOT LIKE 'PREP-%' " +
                "  AND i.GRNNo NOT LIKE 'PRE-%' ");

            if (fromDate.HasValue) sb.Append(" AND i.InwardDate >= ?fd ");
            if (toDate.HasValue)   sb.Append(" AND i.InwardDate <= ?td ");
            if (pushFilter == "Pending")
                sb.Append(" AND (bl.ZohoBillID IS NULL OR bl.ZohoBillID = '') ");
            else if (pushFilter == "Pushed")
                sb.Append(" AND bl.ZohoBillID IS NOT NULL AND bl.ZohoBillID <> '' ");
            else if (pushFilter == "Error")
                sb.Append(" AND bl.PushStatus = 'Error' ");

            sb.Append("ORDER BY i.InwardDate DESC, i.InwardID DESC;");

            using (var conn = new MySqlConnection(ConnectionString))
            using (var cmd = new MySqlCommand(sb.ToString(), conn))
            {
                if (fromDate.HasValue) cmd.Parameters.AddWithValue("?fd", fromDate.Value);
                if (toDate.HasValue) cmd.Parameters.AddWithValue("?td", toDate.Value);
                var dt = new DataTable();
                conn.Open();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }

        /// <summary>List consumable GRNs that are real vendor purchases.
        /// Mirrors GetRawGRNsForBilling. Phase 2 addition.</summary>
        public static DataTable GetConsumableGRNsForBilling(DateTime? fromDate, DateTime? toDate, string pushFilter)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(
                "SELECT i.InwardID, i.GRNNo, i.InvoiceNo, i.InvoiceDate, i.InwardDate, " +
                " i.SupplierID, s.SupplierName, s.State AS SupState, s.GSTNo AS SupGSTIN, " +
                " i.ConsumableID, c.ConsumableName, c.ConsumableCode, c.HSNCode AS ConsumableHSN, " +
                " u.Abbreviation AS Unit, " +
                " i.Quantity, i.QtyActualReceived, i.Rate, i.HSNCode, i.GSTRate, i.GSTAmount, i.Amount, i.Status, " +
                " i.PONo, i.Remarks, i.TransportCost, i.TransportInInvoice, i.TransportInGST, " +
                " i.LoadingCharges, i.UnloadingCharges, i.ShortageQty, i.ShortageValue, " +
                " i.QtyVerified, i.QualityCheck, i.CreatedBy, i.CreatedAt, " +
                " usr.FullName AS CreatedByName, " +
                " bl.ZohoBillID, bl.ZohoBillNo, bl.ZohoStatus, bl.PushStatus, bl.ErrorMessage, " +
                " bl.PushedAt, bl.BillTotal " +
                "FROM mm_consumableinward i " +
                "JOIN mm_suppliers s ON s.SupplierID = i.SupplierID " +
                "JOIN mm_consumables c ON c.ConsumableID = i.ConsumableID " +
                "LEFT JOIN mm_uom u ON u.UOMID = c.UOMID " +
                "LEFT JOIN Users usr ON usr.UserID = i.CreatedBy " +
                "LEFT JOIN zoho_billlog bl ON bl.GRNID = i.InwardID AND bl.GRNType = 'CONSUMABLE' " +
                "WHERE i.SupplierID <> 306 " +
                "  AND i.GRNNo NOT LIKE 'INT-%' " +
                "  AND i.GRNNo NOT LIKE 'PREP-%' " +
                "  AND i.GRNNo NOT LIKE 'PRE-%' ");

            if (fromDate.HasValue) sb.Append(" AND i.InwardDate >= ?fd ");
            if (toDate.HasValue)   sb.Append(" AND i.InwardDate <= ?td ");
            if (pushFilter == "Pending")
                sb.Append(" AND (bl.ZohoBillID IS NULL OR bl.ZohoBillID = '') ");
            else if (pushFilter == "Pushed")
                sb.Append(" AND bl.ZohoBillID IS NOT NULL AND bl.ZohoBillID <> '' ");
            else if (pushFilter == "Error")
                sb.Append(" AND bl.PushStatus = 'Error' ");

            sb.Append("ORDER BY i.InwardDate DESC, i.InwardID DESC;");

            using (var conn = new MySqlConnection(ConnectionString))
            using (var cmd = new MySqlCommand(sb.ToString(), conn))
            {
                if (fromDate.HasValue) cmd.Parameters.AddWithValue("?fd", fromDate.Value);
                if (toDate.HasValue) cmd.Parameters.AddWithValue("?td", toDate.Value);
                var dt = new DataTable();
                conn.Open();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }

        /// <summary>List stationary GRNs that are real vendor purchases.
        /// Mirrors GetRawGRNsForBilling. Phase 2 addition.
        /// Note: underlying table and column use DB spelling "Stationary".</summary>
        public static DataTable GetStationaryGRNsForBilling(DateTime? fromDate, DateTime? toDate, string pushFilter)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(
                "SELECT i.InwardID, i.GRNNo, i.InvoiceNo, i.InvoiceDate, i.InwardDate, " +
                " i.SupplierID, s.SupplierName, s.State AS SupState, s.GSTNo AS SupGSTIN, " +
                " i.StationaryID, st.StationaryName, st.StationaryCode, st.HSNCode AS StationaryHSN, " +
                " u.Abbreviation AS Unit, " +
                " i.Quantity, i.QtyActualReceived, i.Rate, i.HSNCode, i.GSTRate, i.GSTAmount, i.Amount, i.Status, " +
                " i.PONo, i.Remarks, i.TransportCost, i.TransportInInvoice, i.TransportInGST, " +
                " i.LoadingCharges, i.UnloadingCharges, i.ShortageQty, i.ShortageValue, " +
                " i.QtyVerified, i.QualityCheck, i.CreatedBy, i.CreatedAt, " +
                " usr.FullName AS CreatedByName, " +
                " bl.ZohoBillID, bl.ZohoBillNo, bl.ZohoStatus, bl.PushStatus, bl.ErrorMessage, " +
                " bl.PushedAt, bl.BillTotal " +
                "FROM mm_stationaryinward i " +
                "JOIN mm_suppliers s ON s.SupplierID = i.SupplierID " +
                "JOIN mm_stationaries st ON st.StationaryID = i.StationaryID " +
                "LEFT JOIN mm_uom u ON u.UOMID = st.UOMID " +
                "LEFT JOIN Users usr ON usr.UserID = i.CreatedBy " +
                "LEFT JOIN zoho_billlog bl ON bl.GRNID = i.InwardID AND bl.GRNType = 'STATIONARY' " +
                "WHERE i.SupplierID <> 306 " +
                "  AND i.GRNNo NOT LIKE 'INT-%' " +
                "  AND i.GRNNo NOT LIKE 'PREP-%' " +
                "  AND i.GRNNo NOT LIKE 'PRE-%' ");

            if (fromDate.HasValue) sb.Append(" AND i.InwardDate >= ?fd ");
            if (toDate.HasValue)   sb.Append(" AND i.InwardDate <= ?td ");
            if (pushFilter == "Pending")
                sb.Append(" AND (bl.ZohoBillID IS NULL OR bl.ZohoBillID = '') ");
            else if (pushFilter == "Pushed")
                sb.Append(" AND bl.ZohoBillID IS NOT NULL AND bl.ZohoBillID <> '' ");
            else if (pushFilter == "Error")
                sb.Append(" AND bl.PushStatus = 'Error' ");

            sb.Append("ORDER BY i.InwardDate DESC, i.InwardID DESC;");

            using (var conn = new MySqlConnection(ConnectionString))
            using (var cmd = new MySqlCommand(sb.ToString(), conn))
            {
                if (fromDate.HasValue) cmd.Parameters.AddWithValue("?fd", fromDate.Value);
                if (toDate.HasValue) cmd.Parameters.AddWithValue("?td", toDate.Value);
                var dt = new DataTable();
                conn.Open();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }

        /// <summary>Thin wrapper — delegates to the PK helper which owns the Zoho API client
        /// and the auto-create logic for vendors and items.</summary>
        public static FINApp.DAL.ZohoBillPushResult PushGRNToZoho(int grnId, string grnType, int userId)
        {
            return FINApp.DAL.FINZohoHelper.CreateBillFromGRN(grnId, grnType, userId);
        }

        /// <summary>Thin wrapper — delegates to FINZohoHelper which owns the Zoho API client
        /// and party auto-create logic. UI code should call through this wrapper rather than
        /// referencing FINZohoHelper directly (mirrors the PushGRNToZoho pattern).</summary>
        public static FINApp.DAL.ZohoJournalPushResult PushJournalToZoho(int journalId, int userId)
        {
            return FINApp.DAL.FINZohoHelper.PushJournalToZoho(journalId, userId);
        }

        /// <summary>Summary counts for the GRN-to-Zoho dashboard tabs.
        /// Returns a row per GRN type with PendingCount, PushedCount, ErrorCount.
        /// Phase 2: now covers RAW, PACKING, CONSUMABLE, STATIONARY.</summary>
        public static DataTable GetGRNBillingTabSummary(DateTime? fromDate, DateTime? toDate)
        {
            // Union query — one row per type. We keep the filter conditions identical to
            // the listing queries so the tab counts match what the tabs will actually show.
            string dateFilter = "";
            if (fromDate.HasValue) dateFilter += " AND i.InwardDate >= ?fd ";
            if (toDate.HasValue)   dateFilter += " AND i.InwardDate <= ?td ";
            string exclFilter =
                " WHERE i.SupplierID <> 306 " +
                "   AND i.GRNNo NOT LIKE 'INT-%' " +
                "   AND i.GRNNo NOT LIKE 'PREP-%' " +
                "   AND i.GRNNo NOT LIKE 'PRE-%' ";

            // One SELECT per type. Parameters ?fd / ?td are shared across all 4 blocks.
            string countCols =
                "  SUM(CASE WHEN bl.ZohoBillID IS NULL OR bl.ZohoBillID = '' THEN 1 ELSE 0 END) AS PendingCount, " +
                "  SUM(CASE WHEN bl.ZohoBillID IS NOT NULL AND bl.ZohoBillID <> '' THEN 1 ELSE 0 END) AS PushedCount, " +
                "  SUM(CASE WHEN bl.PushStatus = 'Error' THEN 1 ELSE 0 END) AS ErrorCount, " +
                "  COUNT(*) AS TotalCount ";

            string sql =
                "SELECT 'RAW' AS GRNType, " + countCols +
                "FROM mm_rawinward i " +
                "LEFT JOIN zoho_billlog bl ON bl.GRNID = i.InwardID AND bl.GRNType = 'RAW' " +
                exclFilter + dateFilter +
                "UNION ALL " +
                "SELECT 'PACKING' AS GRNType, " + countCols +
                "FROM mm_packinginward i " +
                "LEFT JOIN zoho_billlog bl ON bl.GRNID = i.InwardID AND bl.GRNType = 'PACKING' " +
                exclFilter + dateFilter +
                "UNION ALL " +
                "SELECT 'CONSUMABLE' AS GRNType, " + countCols +
                "FROM mm_consumableinward i " +
                "LEFT JOIN zoho_billlog bl ON bl.GRNID = i.InwardID AND bl.GRNType = 'CONSUMABLE' " +
                exclFilter + dateFilter +
                "UNION ALL " +
                "SELECT 'STATIONARY' AS GRNType, " + countCols +
                "FROM mm_stationaryinward i " +
                "LEFT JOIN zoho_billlog bl ON bl.GRNID = i.InwardID AND bl.GRNType = 'STATIONARY' " +
                exclFilter + dateFilter +
                ";";

            using (var conn = new MySqlConnection(ConnectionString))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                if (fromDate.HasValue) cmd.Parameters.AddWithValue("?fd", fromDate.Value);
                if (toDate.HasValue) cmd.Parameters.AddWithValue("?td", toDate.Value);
                var dt = new DataTable();
                conn.Open();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }

        /// <summary>List distinct suppliers that appear in the filtered GRN lists — for
        /// the supplier dropdown filter on the dashboard. Phase 2: now includes all 4 inward tables.</summary>
        public static DataTable GetSuppliersWithGRNs()
        {
            string sql =
                "SELECT DISTINCT s.SupplierID, s.SupplierName " +
                "FROM mm_suppliers s " +
                "WHERE s.SupplierID <> 306 AND (" +
                "  EXISTS (SELECT 1 FROM mm_rawinward i WHERE i.SupplierID = s.SupplierID " +
                "     AND i.GRNNo NOT LIKE 'INT-%' AND i.GRNNo NOT LIKE 'PREP-%' AND i.GRNNo NOT LIKE 'PRE-%') OR " +
                "  EXISTS (SELECT 1 FROM mm_packinginward i WHERE i.SupplierID = s.SupplierID " +
                "     AND i.GRNNo NOT LIKE 'INT-%' AND i.GRNNo NOT LIKE 'PREP-%' AND i.GRNNo NOT LIKE 'PRE-%') OR " +
                "  EXISTS (SELECT 1 FROM mm_consumableinward i WHERE i.SupplierID = s.SupplierID " +
                "     AND i.GRNNo NOT LIKE 'INT-%' AND i.GRNNo NOT LIKE 'PREP-%' AND i.GRNNo NOT LIKE 'PRE-%') OR " +
                "  EXISTS (SELECT 1 FROM mm_stationaryinward i WHERE i.SupplierID = s.SupplierID " +
                "     AND i.GRNNo NOT LIKE 'INT-%' AND i.GRNNo NOT LIKE 'PREP-%' AND i.GRNNo NOT LIKE 'PRE-%') " +
                ") ORDER BY s.SupplierName;";
            using (var conn = new MySqlConnection(ConnectionString))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                var dt = new DataTable();
                conn.Open();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  JOURNAL ENTRIES (FIN accounting module)
        // ══════════════════════════════════════════════════════════════
        // Double-entry journals with multi-line debits + credits. Accounts
        // come from the locally-cached Zoho chart (FIN_ChartOfAccounts).
        // Status flow: DRAFT → POSTED → REVERSED (via contra journal).

        /// <summary>
        /// List chart of accounts for the dropdown. Active only by default.
        /// Optionally filter by account type (Asset/Liability/Equity/Income/Expense).
        /// </summary>
        public static DataTable GetChartOfAccounts(bool activeOnly = true, string typeFilter = null)
        {
            string sql =
                "SELECT AccountID, ZohoAccountID, AccountName, AccountCode, AccountType, " +
                " AccountTypeName, IsActive, LastSyncAt " +
                "FROM FIN_ChartOfAccounts " +
                "WHERE 1=1 ";
            if (activeOnly) sql += " AND IsActive = 1 ";
            if (!string.IsNullOrEmpty(typeFilter)) sql += " AND AccountType = ?type ";
            sql += "ORDER BY AccountTypeName, AccountName";

            using (var conn = new MySqlConnection(ConnectionString))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                if (!string.IsNullOrEmpty(typeFilter))
                    cmd.Parameters.AddWithValue("?type", typeFilter);
                var dt = new DataTable();
                conn.Open();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }

        /// <summary>
        /// When was the chart of accounts last synced from Zoho? Returns MinValue
        /// if the table is empty.
        /// </summary>
        public static DateTime GetChartOfAccountsLastSync()
        {
            using (var conn = new MySqlConnection(ConnectionString))
            using (var cmd = new MySqlCommand("SELECT MAX(LastSyncAt) FROM FIN_ChartOfAccounts", conn))
            {
                conn.Open();
                object o = cmd.ExecuteScalar();
                if (o == null || o == DBNull.Value) return DateTime.MinValue;
                return Convert.ToDateTime(o);
            }
        }

        /// <summary>
        /// Journal listing for the dashboard. Filter by date range, status, or
        /// journal number substring.
        /// </summary>
        public static DataTable GetJournalList(DateTime? fromDate, DateTime? toDate, string status, string numberSearch)
        {
            // Subqueries add a subtitle-ready "who and what" to each journal row:
            //   PrimaryPartyName — name of the first party-tagged line (supplier or customer)
            //   PrimaryAccountName — name of the first line whose account is NOT a party
            //                        control account (AP/AR).  This is usually the expense
            //                        or income account that "describes" the transaction.
            // Both are LEFT JOINs/subqueries so the list works for journals without parties.
            string sql =
                "SELECT j.JournalID, j.JournalNumber, j.JournalDate, j.Narration, j.Reference, " +
                " j.Status, j.TotalDebit, j.TotalCredit, j.ReversedByJournalID, " +
                " j.CreatedAt, j.PostedAt, j.ReversedAt, " +
                " u.FullName AS CreatedByName, " +
                // Primary party — pick the first party-tagged line, resolve SUP:/CUS: prefix
                " (SELECT CASE " +
                "     WHEN SUBSTRING_INDEX(jl.ContactID, ':', 1) = 'SUP' " +
                "          THEN (SELECT s.SupplierName FROM MM_Suppliers s " +
                "                 WHERE s.SupplierID = CAST(SUBSTRING_INDEX(jl.ContactID, ':', -1) AS UNSIGNED)) " +
                "     WHEN SUBSTRING_INDEX(jl.ContactID, ':', 1) = 'CUS' " +
                "          THEN (SELECT c.CustomerName FROM PK_Customers c " +
                "                 WHERE c.CustomerID = CAST(SUBSTRING_INDEX(jl.ContactID, ':', -1) AS UNSIGNED)) " +
                "     ELSE NULL END " +
                "  FROM FIN_JournalLine jl " +
                "  WHERE jl.JournalID = j.JournalID AND jl.ContactID IS NOT NULL AND jl.ContactID <> '' " +
                "  ORDER BY jl.LineOrder, jl.LineID LIMIT 1) AS PrimaryPartyName, " +
                // Primary account — first line whose account type is NOT payable/receivable
                " (SELECT coa.AccountName FROM FIN_JournalLine jl2 " +
                "  INNER JOIN FIN_ChartOfAccounts coa ON coa.ZohoAccountID = jl2.ZohoAccountID " +
                "  WHERE jl2.JournalID = j.JournalID " +
                "    AND LOWER(COALESCE(coa.AccountTypeName, coa.AccountType, '')) NOT LIKE '%payable%' " +
                "    AND LOWER(COALESCE(coa.AccountTypeName, coa.AccountType, '')) NOT LIKE '%receivable%' " +
                "  ORDER BY jl2.LineOrder, jl2.LineID LIMIT 1) AS PrimaryAccountName " +
                "FROM FIN_Journal j " +
                "LEFT JOIN users u ON u.UserID = j.CreatedBy " +
                "WHERE 1=1 ";
            if (fromDate.HasValue) sql += " AND j.JournalDate >= ?from ";
            if (toDate.HasValue)   sql += " AND j.JournalDate <= ?to ";
            if (!string.IsNullOrEmpty(status)) sql += " AND j.Status = ?status ";
            if (!string.IsNullOrEmpty(numberSearch)) sql += " AND j.JournalNumber LIKE ?numq ";
            sql += "ORDER BY j.JournalDate DESC, j.JournalID DESC LIMIT 500";

            using (var conn = new MySqlConnection(ConnectionString))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                if (fromDate.HasValue) cmd.Parameters.AddWithValue("?from", fromDate.Value.Date);
                if (toDate.HasValue)   cmd.Parameters.AddWithValue("?to",   toDate.Value.Date);
                if (!string.IsNullOrEmpty(status))       cmd.Parameters.AddWithValue("?status", status);
                if (!string.IsNullOrEmpty(numberSearch)) cmd.Parameters.AddWithValue("?numq", "%" + numberSearch + "%");
                var dt = new DataTable();
                conn.Open();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }

        /// <summary>
        /// Load a journal's header + lines for edit or view mode.
        /// DataSet tables: [0]=Header (single row), [1]=Lines.
        /// </summary>
        public static DataSet GetJournalDetail(int journalId)
        {
            var ds = new DataSet();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                // Header
                using (var cmd = new MySqlCommand(
                    "SELECT j.*, u.FullName AS CreatedByName, up.FullName AS PostedByName, ur.FullName AS ReversedByName " +
                    "FROM FIN_Journal j " +
                    "LEFT JOIN users u  ON u.UserID  = j.CreatedBy " +
                    "LEFT JOIN users up ON up.UserID = j.PostedBy " +
                    "LEFT JOIN users ur ON ur.UserID = j.ReversedBy " +
                    "WHERE j.JournalID = ?jid", conn))
                {
                    cmd.Parameters.AddWithValue("?jid", journalId);
                    var dt = new DataTable("Header");
                    dt.Load(cmd.ExecuteReader());
                    ds.Tables.Add(dt);
                }
                // Lines — join to chart to get the human-readable account name
                using (var cmd = new MySqlCommand(
                    "SELECT l.LineID, l.JournalID, l.LineOrder, l.ZohoAccountID, " +
                    " l.Debit, l.Credit, l.LineDescription, l.ContactID, " +
                    " c.AccountName, c.AccountCode, c.AccountType, c.AccountTypeName " +
                    "FROM FIN_JournalLine l " +
                    "LEFT JOIN FIN_ChartOfAccounts c ON c.ZohoAccountID = l.ZohoAccountID " +
                    "WHERE l.JournalID = ?jid " +
                    "ORDER BY l.LineOrder, l.LineID", conn))
                {
                    cmd.Parameters.AddWithValue("?jid", journalId);
                    var dt = new DataTable("Lines");
                    dt.Load(cmd.ExecuteReader());
                    ds.Tables.Add(dt);
                }
            }
            return ds;
        }

        /// <summary>
        /// Compute the current fiscal year code for journal numbering.
        /// India FY runs April–March. e.g. 20 Apr 2026 → "2627" (FY26-27).
        /// </summary>
        public static string GetCurrentFY(DateTime forDate)
        {
            int y = forDate.Year;
            int startYear = (forDate.Month >= 4) ? y : (y - 1);
            int endYear = startYear + 1;
            return (startYear % 100).ToString("D2") + (endYear % 100).ToString("D2");
        }

        /// <summary>
        /// Increment and return the next journal number for the given date.
        /// Uses FIN_JournalSequence (per-FY counter, pattern mirrors PK_DCSequence).
        /// </summary>
        public static string NextJournalNumber(DateTime forDate)
        {
            string fy = GetCurrentFY(forDate);
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                // Ensure row exists
                using (var cmd = new MySqlCommand(
                    "INSERT IGNORE INTO FIN_JournalSequence (FiscalYear, LastSeq) VALUES (?fy, 0)", conn))
                {
                    cmd.Parameters.AddWithValue("?fy", fy);
                    cmd.ExecuteNonQuery();
                }
                // Increment + fetch
                int nextSeq;
                using (var cmd = new MySqlCommand(
                    "UPDATE FIN_JournalSequence SET LastSeq = LastSeq + 1, UpdatedAt = NOW() WHERE FiscalYear = ?fy", conn))
                {
                    cmd.Parameters.AddWithValue("?fy", fy);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new MySqlCommand(
                    "SELECT LastSeq FROM FIN_JournalSequence WHERE FiscalYear = ?fy", conn))
                {
                    cmd.Parameters.AddWithValue("?fy", fy);
                    nextSeq = Convert.ToInt32(cmd.ExecuteScalar());
                }
                return "JV-" + fy + "-" + nextSeq.ToString("D4");
            }
        }

        /// <summary>
        /// Represents one line the UI wants to save. LineOrder is assigned
        /// by the DAL based on position in the list.
        /// </summary>
        public class JournalLineInput
        {
            public string ZohoAccountID;
            public decimal Debit;
            public decimal Credit;
            public string LineDescription;
            public string ContactID;   // stores "SUP:<SupplierID>" or "CUS:<CustomerID>" or NULL
        }

        /// <summary>
        /// Combined party list (MM Suppliers + PK Customers) for the Journal Party picker.
        /// PartyKey is the canonical value stored in FIN_JournalLine.ContactID:
        ///   "SUP:88" for MM_Suppliers.SupplierID=88
        ///   "CUS:793" for PK_Customers.CustomerID=793
        /// Sorted by display name.
        /// Only active parties returned.
        /// </summary>
        public static DataTable GetPartyList()
        {
            // NOTE: columns in WHERE are table-qualified (s.PartyType) to avoid any
            // parser ambiguity with the 'SUP'/'SRV'/'CUS' literal aliases in the SELECT list.
            var dt = new DataTable();
            using (var conn = new MySqlConnection(ConnectionString))
            using (var cmd = new MySqlCommand(
                "SELECT CONCAT('SUP:', s.SupplierID) AS PartyKey, 'SUP' AS PartyType, " +
                "  s.SupplierID AS PartyID, s.SupplierCode AS PartyCode, s.SupplierName AS PartyName, " +
                "  s.GSTNo AS GSTNo, s.City, s.State " +
                "FROM mm_suppliers s WHERE s.IsActive = 1 AND s.PartyType = 'SUPPLIER' " +
                "UNION ALL " +
                "SELECT CONCAT('SRV:', s.SupplierID) AS PartyKey, 'SRV' AS PartyType, " +
                "  s.SupplierID AS PartyID, s.SupplierCode AS PartyCode, s.SupplierName AS PartyName, " +
                "  s.GSTNo AS GSTNo, s.City, s.State " +
                "FROM mm_suppliers s WHERE s.IsActive = 1 AND s.PartyType = 'SERVICE' " +
                "UNION ALL " +
                "SELECT CONCAT('CUS:', c.CustomerID) AS PartyKey, 'CUS' AS PartyType, " +
                "  c.CustomerID AS PartyID, c.CustomerCode AS PartyCode, c.CustomerName AS PartyName, " +
                "  c.GSTIN AS GSTNo, c.City, c.State " +
                "FROM pk_customers c WHERE c.IsActive = 1 " +
                "ORDER BY PartyName", conn))
            {
                conn.Open();
                dt.Load(cmd.ExecuteReader());
            }
            return dt;
        }

        /// <summary>
        /// Look up the display name for a single party key like "SUP:88" or "CUS:793".
        /// Used in view/posted mode to show the party name without the dropdown.
        /// Returns empty string if not found or if key is null/malformed.
        /// </summary>
        public static string GetPartyDisplayName(string partyKey)
        {
            if (string.IsNullOrEmpty(partyKey)) return "";
            var parts = partyKey.Split(':');
            if (parts.Length != 2) return "";
            int pid;
            if (!int.TryParse(parts[1], out pid)) return "";

            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = (parts[0] == "SUP" || parts[0] == "SRV")
                    ? "SELECT SupplierName FROM mm_suppliers WHERE SupplierID = ?pid"
                    : parts[0] == "CUS"
                        ? "SELECT CustomerName FROM pk_customers WHERE CustomerID = ?pid"
                        : null;
                if (sql == null) return "";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("?pid", pid);
                    object r = cmd.ExecuteScalar();
                    return r == null || r == DBNull.Value ? "" : r.ToString();
                }
            }
        }

        /// <summary>
        /// Create a new journal as DRAFT with the given lines. Returns new JournalID.
        /// </summary>
        public static int SaveJournalAsDraft(DateTime journalDate, string narration, string reference,
                                             System.Collections.Generic.List<JournalLineInput> lines, int userId)
        {
            if (lines == null || lines.Count == 0)
                throw new Exception("Journal must have at least one line.");

            decimal totalDebit = 0, totalCredit = 0;
            foreach (var ln in lines)
            {
                if (ln.Debit > 0 && ln.Credit > 0)
                    throw new Exception("A line can have either Debit or Credit, not both.");
                if (ln.Debit == 0 && ln.Credit == 0)
                    throw new Exception("Every line must have a non-zero Debit or Credit.");
                if (string.IsNullOrEmpty(ln.ZohoAccountID))
                    throw new Exception("Every line must have an Account selected.");
                totalDebit  += ln.Debit;
                totalCredit += ln.Credit;
            }

            string journalNo = NextJournalNumber(journalDate);

            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    int journalId;
                    using (var cmd = new MySqlCommand(
                        "INSERT INTO FIN_Journal " +
                        " (JournalNumber, JournalDate, Narration, Reference, Status, " +
                        "  TotalDebit, TotalCredit, CreatedBy, CreatedAt) " +
                        "VALUES " +
                        " (?jn, ?jd, ?narr, ?ref, 'DRAFT', ?td, ?tc, ?uid, NOW()); " +
                        "SELECT LAST_INSERT_ID();", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("?jn", journalNo);
                        cmd.Parameters.AddWithValue("?jd", journalDate.Date);
                        cmd.Parameters.AddWithValue("?narr", string.IsNullOrEmpty(narration) ? (object)DBNull.Value : narration);
                        cmd.Parameters.AddWithValue("?ref", string.IsNullOrEmpty(reference) ? (object)DBNull.Value : reference);
                        cmd.Parameters.AddWithValue("?td", totalDebit);
                        cmd.Parameters.AddWithValue("?tc", totalCredit);
                        cmd.Parameters.AddWithValue("?uid", userId);
                        journalId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    int order = 1;
                    foreach (var ln in lines)
                    {
                        using (var cmd = new MySqlCommand(
                            "INSERT INTO FIN_JournalLine " +
                            " (JournalID, LineOrder, ZohoAccountID, Debit, Credit, LineDescription, ContactID) " +
                            "VALUES (?jid, ?ord, ?acc, ?dr, ?cr, ?desc, ?cid)", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("?jid", journalId);
                            cmd.Parameters.AddWithValue("?ord", order++);
                            cmd.Parameters.AddWithValue("?acc", ln.ZohoAccountID);
                            cmd.Parameters.AddWithValue("?dr", ln.Debit);
                            cmd.Parameters.AddWithValue("?cr", ln.Credit);
                            cmd.Parameters.AddWithValue("?desc", string.IsNullOrEmpty(ln.LineDescription) ? (object)DBNull.Value : ln.LineDescription);
                            cmd.Parameters.AddWithValue("?cid",  string.IsNullOrEmpty(ln.ContactID) ? (object)DBNull.Value : ln.ContactID);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                    return journalId;
                }
            }
        }

        /// <summary>
        /// Update an existing DRAFT journal. Throws if the journal is not DRAFT.
        /// Replaces lines wholesale (delete all + re-insert) since re-ordering
        /// is common and the transaction keeps it atomic.
        /// </summary>
        public static void UpdateJournalDraft(int journalId, DateTime journalDate, string narration, string reference,
                                              System.Collections.Generic.List<JournalLineInput> lines, int userId)
        {
            if (lines == null || lines.Count == 0)
                throw new Exception("Journal must have at least one line.");

            decimal totalDebit = 0, totalCredit = 0;
            foreach (var ln in lines)
            {
                if (ln.Debit > 0 && ln.Credit > 0)
                    throw new Exception("A line can have either Debit or Credit, not both.");
                if (ln.Debit == 0 && ln.Credit == 0)
                    throw new Exception("Every line must have a non-zero Debit or Credit.");
                if (string.IsNullOrEmpty(ln.ZohoAccountID))
                    throw new Exception("Every line must have an Account selected.");
                totalDebit  += ln.Debit;
                totalCredit += ln.Credit;
            }

            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    // Verify it's still DRAFT
                    using (var cmd = new MySqlCommand("SELECT Status FROM FIN_Journal WHERE JournalID = ?jid FOR UPDATE", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("?jid", journalId);
                        object s = cmd.ExecuteScalar();
                        if (s == null) throw new Exception("Journal not found.");
                        if (s.ToString() != "DRAFT")
                            throw new Exception("Only DRAFT journals can be edited. This one is " + s + ".");
                    }

                    using (var cmd = new MySqlCommand(
                        "UPDATE FIN_Journal SET " +
                        " JournalDate=?jd, Narration=?narr, Reference=?ref, " +
                        " TotalDebit=?td, TotalCredit=?tc, UpdatedBy=?uid, UpdatedAt=NOW() " +
                        "WHERE JournalID=?jid", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("?jd", journalDate.Date);
                        cmd.Parameters.AddWithValue("?narr", string.IsNullOrEmpty(narration) ? (object)DBNull.Value : narration);
                        cmd.Parameters.AddWithValue("?ref", string.IsNullOrEmpty(reference) ? (object)DBNull.Value : reference);
                        cmd.Parameters.AddWithValue("?td", totalDebit);
                        cmd.Parameters.AddWithValue("?tc", totalCredit);
                        cmd.Parameters.AddWithValue("?uid", userId);
                        cmd.Parameters.AddWithValue("?jid", journalId);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new MySqlCommand("DELETE FROM FIN_JournalLine WHERE JournalID = ?jid", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("?jid", journalId);
                        cmd.ExecuteNonQuery();
                    }

                    int order = 1;
                    foreach (var ln in lines)
                    {
                        using (var cmd = new MySqlCommand(
                            "INSERT INTO FIN_JournalLine " +
                            " (JournalID, LineOrder, ZohoAccountID, Debit, Credit, LineDescription, ContactID) " +
                            "VALUES (?jid, ?ord, ?acc, ?dr, ?cr, ?desc, ?cid)", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("?jid", journalId);
                            cmd.Parameters.AddWithValue("?ord", order++);
                            cmd.Parameters.AddWithValue("?acc", ln.ZohoAccountID);
                            cmd.Parameters.AddWithValue("?dr", ln.Debit);
                            cmd.Parameters.AddWithValue("?cr", ln.Credit);
                            cmd.Parameters.AddWithValue("?desc", string.IsNullOrEmpty(ln.LineDescription) ? (object)DBNull.Value : ln.LineDescription);
                            cmd.Parameters.AddWithValue("?cid",  string.IsNullOrEmpty(ln.ContactID) ? (object)DBNull.Value : ln.ContactID);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }
            }
        }

        /// <summary>
        /// Post a DRAFT journal. Verifies debit == credit and flips status to POSTED.
        /// </summary>
        public static void PostJournal(int journalId, int userId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    decimal td, tc;
                    string status;
                    using (var cmd = new MySqlCommand(
                        "SELECT Status, TotalDebit, TotalCredit FROM FIN_Journal WHERE JournalID = ?jid FOR UPDATE", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("?jid", journalId);
                        using (var rdr = cmd.ExecuteReader())
                        {
                            if (!rdr.Read()) throw new Exception("Journal not found.");
                            status = rdr["Status"].ToString();
                            td = Convert.ToDecimal(rdr["TotalDebit"]);
                            tc = Convert.ToDecimal(rdr["TotalCredit"]);
                        }
                    }
                    if (status != "DRAFT")
                        throw new Exception("Only DRAFT journals can be posted. This one is " + status + ".");
                    if (td != tc)
                        throw new Exception("Journal is not balanced: debit " + td.ToString("N2") + " vs credit " + tc.ToString("N2") + ".");
                    if (td == 0)
                        throw new Exception("Journal has zero total — add at least one line.");

                    using (var cmd = new MySqlCommand(
                        "UPDATE FIN_Journal SET Status='POSTED', PostedBy=?uid, PostedAt=NOW() WHERE JournalID=?jid", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("?uid", userId);
                        cmd.Parameters.AddWithValue("?jid", journalId);
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
        }

        /// <summary>
        /// Reverse a POSTED journal by creating a contra entry with debits ↔ credits
        /// swapped. Original becomes REVERSED and points to the contra via
        /// ReversedByJournalID. Returns the new contra JournalID.
        /// </summary>
        public static int ReverseJournal(int originalJournalId, DateTime reversalDate, string reason, int userId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    // 1) Load original header + lines, verify POSTED
                    string origNumber, origNarration, status;
                    using (var cmd = new MySqlCommand(
                        "SELECT Status, JournalNumber, Narration FROM FIN_Journal WHERE JournalID = ?jid FOR UPDATE", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("?jid", originalJournalId);
                        using (var rdr = cmd.ExecuteReader())
                        {
                            if (!rdr.Read()) throw new Exception("Journal not found.");
                            status = rdr["Status"].ToString();
                            origNumber = rdr["JournalNumber"].ToString();
                            origNarration = rdr["Narration"] == DBNull.Value ? "" : rdr["Narration"].ToString();
                        }
                    }
                    if (status != "POSTED")
                        throw new Exception("Only POSTED journals can be reversed. This one is " + status + ".");

                    var origLines = new System.Collections.Generic.List<JournalLineInput>();
                    decimal td = 0, tc = 0;
                    using (var cmd = new MySqlCommand(
                        "SELECT ZohoAccountID, Debit, Credit, LineDescription, ContactID, LineOrder " +
                        "FROM FIN_JournalLine WHERE JournalID = ?jid ORDER BY LineOrder, LineID", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("?jid", originalJournalId);
                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                var ln = new JournalLineInput
                                {
                                    ZohoAccountID = rdr["ZohoAccountID"].ToString(),
                                    // SWAP: original debit becomes credit, original credit becomes debit
                                    Debit  = Convert.ToDecimal(rdr["Credit"]),
                                    Credit = Convert.ToDecimal(rdr["Debit"]),
                                    LineDescription = rdr["LineDescription"] == DBNull.Value ? null : rdr["LineDescription"].ToString(),
                                    ContactID = rdr["ContactID"] == DBNull.Value ? null : rdr["ContactID"].ToString()
                                };
                                origLines.Add(ln);
                                td += ln.Debit;
                                tc += ln.Credit;
                            }
                        }
                    }

                    // 2) Create contra journal (new number, same lines swapped, narration prefixed)
                    // NextJournalNumber uses its own connection — close the current tx briefly for it.
                    // Simpler: inline the seq increment here to keep the tx atomic.
                    string fy = GetCurrentFY(reversalDate);
                    using (var cmd = new MySqlCommand(
                        "INSERT IGNORE INTO FIN_JournalSequence (FiscalYear, LastSeq) VALUES (?fy, 0)", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("?fy", fy);
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new MySqlCommand(
                        "UPDATE FIN_JournalSequence SET LastSeq = LastSeq + 1, UpdatedAt = NOW() WHERE FiscalYear = ?fy", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("?fy", fy);
                        cmd.ExecuteNonQuery();
                    }
                    int nextSeq;
                    using (var cmd = new MySqlCommand(
                        "SELECT LastSeq FROM FIN_JournalSequence WHERE FiscalYear = ?fy", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("?fy", fy);
                        nextSeq = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    string contraNumber = "JV-" + fy + "-" + nextSeq.ToString("D4");
                    string contraNarration = "Reversal of " + origNumber
                        + (string.IsNullOrEmpty(reason) ? "" : " — " + reason)
                        + (string.IsNullOrEmpty(origNarration) ? "" : " (orig: " + origNarration + ")");

                    int contraId;
                    using (var cmd = new MySqlCommand(
                        "INSERT INTO FIN_Journal " +
                        " (JournalNumber, JournalDate, Narration, Reference, Status, " +
                        "  TotalDebit, TotalCredit, CreatedBy, CreatedAt, PostedBy, PostedAt) " +
                        "VALUES " +
                        " (?jn, ?jd, ?narr, ?ref, 'POSTED', ?td, ?tc, ?uid, NOW(), ?uid, NOW()); " +
                        "SELECT LAST_INSERT_ID();", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("?jn", contraNumber);
                        cmd.Parameters.AddWithValue("?jd", reversalDate.Date);
                        cmd.Parameters.AddWithValue("?narr", contraNarration);
                        cmd.Parameters.AddWithValue("?ref", origNumber);
                        cmd.Parameters.AddWithValue("?td", td);
                        cmd.Parameters.AddWithValue("?tc", tc);
                        cmd.Parameters.AddWithValue("?uid", userId);
                        contraId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    int order = 1;
                    foreach (var ln in origLines)
                    {
                        using (var cmd = new MySqlCommand(
                            "INSERT INTO FIN_JournalLine " +
                            " (JournalID, LineOrder, ZohoAccountID, Debit, Credit, LineDescription, ContactID) " +
                            "VALUES (?jid, ?ord, ?acc, ?dr, ?cr, ?desc, ?cid)", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("?jid", contraId);
                            cmd.Parameters.AddWithValue("?ord", order++);
                            cmd.Parameters.AddWithValue("?acc", ln.ZohoAccountID);
                            cmd.Parameters.AddWithValue("?dr", ln.Debit);
                            cmd.Parameters.AddWithValue("?cr", ln.Credit);
                            cmd.Parameters.AddWithValue("?desc", string.IsNullOrEmpty(ln.LineDescription) ? (object)DBNull.Value : ln.LineDescription);
                            cmd.Parameters.AddWithValue("?cid",  string.IsNullOrEmpty(ln.ContactID) ? (object)DBNull.Value : ln.ContactID);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // 3) Mark original as REVERSED, link to contra
                    using (var cmd = new MySqlCommand(
                        "UPDATE FIN_Journal SET " +
                        " Status='REVERSED', ReversedByJournalID=?cid, ReversedBy=?uid, ReversedAt=NOW() " +
                        "WHERE JournalID=?jid", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("?cid", contraId);
                        cmd.Parameters.AddWithValue("?uid", userId);
                        cmd.Parameters.AddWithValue("?jid", originalJournalId);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                    return contraId;
                }
            }
        }

        /// <summary>
        /// Delete a DRAFT journal. Throws if status is not DRAFT. Lines cascade.
        /// </summary>
        public static void DeleteJournalDraft(int journalId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string status;
                using (var cmd = new MySqlCommand("SELECT Status FROM FIN_Journal WHERE JournalID = ?jid", conn))
                {
                    cmd.Parameters.AddWithValue("?jid", journalId);
                    object s = cmd.ExecuteScalar();
                    if (s == null) throw new Exception("Journal not found.");
                    status = s.ToString();
                }
                if (status != "DRAFT")
                    throw new Exception("Only DRAFT journals can be deleted. This one is " + status + ".");

                using (var cmd = new MySqlCommand("DELETE FROM FIN_Journal WHERE JournalID = ?jid", conn))
                {
                    cmd.Parameters.AddWithValue("?jid", journalId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>Journal status counts for dashboard badge.</summary>
        public static DataTable GetJournalStatusCounts()
        {
            using (var conn = new MySqlConnection(ConnectionString))
            using (var cmd = new MySqlCommand(
                "SELECT Status, COUNT(*) AS Cnt FROM FIN_Journal GROUP BY Status", conn))
            {
                var dt = new DataTable();
                conn.Open();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        //  JOURNAL → ZOHO PUSH LOG
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Fetch the zoho_journallog row for a given JournalID, or null if
        /// the journal has never been push-attempted.  Used by the UI to
        /// decide whether to show "Push to Zoho" vs "Re-push" vs
        /// "View in Zoho".
        /// </summary>
        public static DataRow GetJournalLog(int journalId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            using (var cmd = new MySqlCommand(
                "SELECT * FROM zoho_journallog WHERE JournalID = ?jid " +
                "ORDER BY LogID DESC LIMIT 1", conn))
            {
                cmd.Parameters.AddWithValue("?jid", journalId);
                var dt = new DataTable();
                conn.Open();
                dt.Load(cmd.ExecuteReader());
                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
        }

        /// <summary>
        /// Batch companion to GetJournalLog — fetches zoho_journallog rows
        /// for a set of JournalIDs in one query, keyed by JournalID.  Used
        /// by the list view to avoid N+1 queries when decorating ~500 rows
        /// with their Zoho push status.
        /// </summary>
        public static System.Collections.Generic.Dictionary<int, DataRow> GetJournalLogsForList(
            System.Collections.Generic.IEnumerable<int> journalIds)
        {
            var result = new System.Collections.Generic.Dictionary<int, DataRow>();
            if (journalIds == null) return result;

            // Build a comma-separated list of validated integers — safe against injection
            // because we parse each one as an int before concatenating.
            var idList = new System.Collections.Generic.List<int>();
            foreach (int id in journalIds)
                if (id > 0) idList.Add(id);
            if (idList.Count == 0) return result;

            string inClause = string.Join(",", idList);
            using (var conn = new MySqlConnection(ConnectionString))
            using (var cmd = new MySqlCommand(
                "SELECT * FROM zoho_journallog WHERE JournalID IN (" + inClause + ")", conn))
            {
                var dt = new DataTable();
                conn.Open();
                dt.Load(cmd.ExecuteReader());
                foreach (DataRow r in dt.Rows)
                {
                    int jid = Convert.ToInt32(r["JournalID"]);
                    // One row per journal — if somehow there are dupes, keep the newest
                    // (unique key on JournalID prevents this in practice).
                    if (!result.ContainsKey(jid))
                        result[jid] = r;
                }
            }
            return result;
        }

        /// <summary>
        /// Mark a journal as successfully pushed.  Upsert: first push
        /// creates the log row; any later push attempt overwrites it.
        /// </summary>
        public static void MarkJournalPushed(
            int journalId, string journalNumber,
            string zohoJournalId, string zohoJournalNo, string zohoStatus,
            int userId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            using (var cmd = new MySqlCommand(
                "INSERT INTO zoho_journallog " +
                " (JournalID, JournalNumber, ZohoJournalID, ZohoJournalNo, ZohoStatus, " +
                "  PushStatus, ErrorMessage, PushedAt, PushedBy) " +
                "VALUES (?jid, ?jno, ?zid, ?zno, ?zst, 'Pushed', NULL, NOW(), ?uid) " +
                "ON DUPLICATE KEY UPDATE " +
                " JournalNumber = VALUES(JournalNumber), " +
                " ZohoJournalID = VALUES(ZohoJournalID), " +
                " ZohoJournalNo = VALUES(ZohoJournalNo), " +
                " ZohoStatus    = VALUES(ZohoStatus), " +
                " PushStatus    = 'Pushed', " +
                " ErrorMessage  = NULL, " +
                " PushedAt      = NOW(), " +
                " PushedBy      = VALUES(PushedBy)", conn))
            {
                cmd.Parameters.AddWithValue("?jid", journalId);
                cmd.Parameters.AddWithValue("?jno", (object)journalNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("?zid", (object)zohoJournalId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("?zno", (object)zohoJournalNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("?zst", (object)zohoStatus ?? DBNull.Value);
                cmd.Parameters.AddWithValue("?uid", userId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Record a push failure.  Flips PushStatus to 'Error' and stores
        /// ErrorMessage.  If there's no prior row, inserts a new Error row.
        /// </summary>
        public static void MarkJournalPushFailed(
            int journalId, string journalNumber, string errorMessage, int userId)
        {
            if (!string.IsNullOrEmpty(errorMessage) && errorMessage.Length > 500)
                errorMessage = errorMessage.Substring(0, 497) + "...";

            using (var conn = new MySqlConnection(ConnectionString))
            using (var cmd = new MySqlCommand(
                "INSERT INTO zoho_journallog " +
                " (JournalID, JournalNumber, PushStatus, ErrorMessage, PushedAt, PushedBy) " +
                "VALUES (?jid, ?jno, 'Error', ?err, NOW(), ?uid) " +
                "ON DUPLICATE KEY UPDATE " +
                " JournalNumber = VALUES(JournalNumber), " +
                " PushStatus    = 'Error', " +
                " ErrorMessage  = VALUES(ErrorMessage), " +
                " PushedAt      = NOW(), " +
                " PushedBy      = VALUES(PushedBy)", conn))
            {
                cmd.Parameters.AddWithValue("?jid", journalId);
                cmd.Parameters.AddWithValue("?jno", (object)journalNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("?err", (object)errorMessage ?? DBNull.Value);
                cmd.Parameters.AddWithValue("?uid", userId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Given a party key like "SUP:88" or "CUS:793", resolve to
        /// (PartyType, PartyID).  Returns (null, 0) on malformed input.
        /// </summary>
        public static Tuple<string, int> ParsePartyKey(string partyKey)
        {
            if (string.IsNullOrEmpty(partyKey)) return Tuple.Create<string, int>(null, 0);
            var parts = partyKey.Split(':');
            if (parts.Length != 2) return Tuple.Create<string, int>(null, 0);
            string ptype = parts[0].ToUpperInvariant();
            if (ptype != "SUP" && ptype != "CUS" && ptype != "SRV") return Tuple.Create<string, int>(null, 0);
            int pid;
            if (!int.TryParse(parts[1], out pid) || pid <= 0)
                return Tuple.Create<string, int>(null, 0);
            return Tuple.Create(ptype, pid);
        }

        // ═══════════════════════════════════════════════════════════════
        // ACCOUNT STATEMENT + PARTY OPENING BALANCE
        // Added 2026-04-21 for FINAccountStatement and FINPartyOpeningBalance pages.
        // ═══════════════════════════════════════════════════════════════

        // ── PARTY OPENING BALANCE ──

        public static DataRow GetOpeningBalance(string partyType, int partyID, string fy)
        {
            return ExecuteQueryRow(
                "SELECT OpeningID, PartyType, PartyID, FY, AsOfDate, Amount, DrCr," +
                " Reason, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn" +
                " FROM fin_partyopeningbalance" +
                " WHERE PartyType=?pt AND PartyID=?pid AND FY=?fy LIMIT 1;",
                new MySqlParameter("?pt", partyType),
                new MySqlParameter("?pid", partyID),
                new MySqlParameter("?fy", fy));
        }

        /// <summary>
        /// Insert or update opening balance for (partyType, partyID, fy).
        /// Writes a before/after audit row in the same transaction.
        /// Caller enforces that Reason is non-empty on update.
        /// </summary>
        public static long SaveOpeningBalance(string partyType, int partyID, string fy,
                                              DateTime asOfDate, decimal amount, string drCr,
                                              string reason, string user)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    long openingID = 0;
                    decimal? oldAmount = null;
                    string oldDrCr = null;

                    using (var cmd = new MySqlCommand(
                        "SELECT OpeningID, Amount, DrCr FROM fin_partyopeningbalance" +
                        " WHERE PartyType=?pt AND PartyID=?pid AND FY=?fy LIMIT 1;", conn, tx))
                    {
                        cmd.Parameters.Add(new MySqlParameter("?pt", partyType));
                        cmd.Parameters.Add(new MySqlParameter("?pid", partyID));
                        cmd.Parameters.Add(new MySqlParameter("?fy", fy));
                        using (var rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                openingID = Convert.ToInt64(rdr["OpeningID"]);
                                oldAmount = Convert.ToDecimal(rdr["Amount"]);
                                oldDrCr = rdr["DrCr"].ToString();
                            }
                        }
                    }

                    string actionType;
                    if (openingID == 0)
                    {
                        actionType = "INSERT";
                        using (var cmd = new MySqlCommand(
                            "INSERT INTO fin_partyopeningbalance" +
                            " (PartyType, PartyID, FY, AsOfDate, Amount, DrCr, Reason, CreatedBy, CreatedOn)" +
                            " VALUES (?pt, ?pid, ?fy, ?asof, ?amt, ?drcr, ?reason, ?user, NOW());" +
                            " SELECT LAST_INSERT_ID();", conn, tx))
                        {
                            cmd.Parameters.Add(new MySqlParameter("?pt", partyType));
                            cmd.Parameters.Add(new MySqlParameter("?pid", partyID));
                            cmd.Parameters.Add(new MySqlParameter("?fy", fy));
                            cmd.Parameters.Add(new MySqlParameter("?asof", asOfDate));
                            cmd.Parameters.Add(new MySqlParameter("?amt", amount));
                            cmd.Parameters.Add(new MySqlParameter("?drcr", drCr));
                            cmd.Parameters.Add(new MySqlParameter("?reason", (object)reason ?? DBNull.Value));
                            cmd.Parameters.Add(new MySqlParameter("?user", user));
                            openingID = Convert.ToInt64(cmd.ExecuteScalar());
                        }
                    }
                    else
                    {
                        actionType = "UPDATE";
                        using (var cmd = new MySqlCommand(
                            "UPDATE fin_partyopeningbalance" +
                            " SET AsOfDate=?asof, Amount=?amt, DrCr=?drcr, Reason=?reason," +
                            "     LastModifiedBy=?user, LastModifiedOn=NOW()" +
                            " WHERE OpeningID=?oid;", conn, tx))
                        {
                            cmd.Parameters.Add(new MySqlParameter("?asof", asOfDate));
                            cmd.Parameters.Add(new MySqlParameter("?amt", amount));
                            cmd.Parameters.Add(new MySqlParameter("?drcr", drCr));
                            cmd.Parameters.Add(new MySqlParameter("?reason", (object)reason ?? DBNull.Value));
                            cmd.Parameters.Add(new MySqlParameter("?user", user));
                            cmd.Parameters.Add(new MySqlParameter("?oid", openingID));
                            cmd.ExecuteNonQuery();
                        }
                    }

                    using (var cmd = new MySqlCommand(
                        "INSERT INTO fin_partyopeningbalance_audit" +
                        " (OpeningID, PartyType, PartyID, FY, ActionType," +
                        "  OldAmount, OldDrCr, NewAmount, NewDrCr, Reason, ChangedBy, ChangedOn)" +
                        " VALUES (?oid, ?pt, ?pid, ?fy, ?act," +
                        "         ?oldamt, ?olddrcr, ?newamt, ?newdrcr, ?reason, ?user, NOW());", conn, tx))
                    {
                        cmd.Parameters.Add(new MySqlParameter("?oid", openingID));
                        cmd.Parameters.Add(new MySqlParameter("?pt", partyType));
                        cmd.Parameters.Add(new MySqlParameter("?pid", partyID));
                        cmd.Parameters.Add(new MySqlParameter("?fy", fy));
                        cmd.Parameters.Add(new MySqlParameter("?act", actionType));
                        cmd.Parameters.Add(new MySqlParameter("?oldamt", (object)oldAmount ?? DBNull.Value));
                        cmd.Parameters.Add(new MySqlParameter("?olddrcr", (object)oldDrCr ?? DBNull.Value));
                        cmd.Parameters.Add(new MySqlParameter("?newamt", amount));
                        cmd.Parameters.Add(new MySqlParameter("?newdrcr", drCr));
                        cmd.Parameters.Add(new MySqlParameter("?reason", (object)reason ?? DBNull.Value));
                        cmd.Parameters.Add(new MySqlParameter("?user", user));
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                    return openingID;
                }
            }
        }

        public static DataTable GetOpeningBalanceAudit(string partyType, int partyID, string fy)
        {
            return ExecuteQuery(
                "SELECT AuditID, ActionType, OldAmount, OldDrCr, NewAmount, NewDrCr," +
                " Reason, ChangedBy, ChangedOn" +
                " FROM fin_partyopeningbalance_audit" +
                " WHERE PartyType=?pt AND PartyID=?pid AND FY=?fy" +
                " ORDER BY ChangedOn DESC, AuditID DESC;",
                new MySqlParameter("?pt", partyType),
                new MySqlParameter("?pid", partyID),
                new MySqlParameter("?fy", fy));
        }

        // ── PARTY DROPDOWNS (used by Account Statement + Opening Balance setter) ──

        /// <summary>
        /// All customers or all suppliers (active + inactive) — used by the
        /// opening-balance setter. PartyType: "CUS" or "SUP".
        /// </summary>
        public static DataTable ListAllParties(string partyType)
        {
            if (partyType == "CUS")
            {
                return ExecuteQuery(
                    "SELECT CustomerID AS PartyID, CustomerName AS Name," +
                    " CustomerCode AS Code, GSTIN, IsActive" +
                    " FROM pk_customers ORDER BY CustomerName;");
            }
            return ExecuteQuery(
                "SELECT SupplierID AS PartyID, SupplierName AS Name," +
                " SupplierCode AS Code, GSTNo AS GSTIN, IsActive" +
                " FROM mm_suppliers WHERE PartyType='SUPPLIER' ORDER BY SupplierName;");
        }

        /// <summary>
        /// Parties with any activity in the selected date range OR any non-zero
        /// opening balance. Used by Account Statement dropdown.
        /// </summary>
        public static DataTable ListPartiesWithActivity(string partyType, DateTime fromDate, DateTime toDate)
        {
            if (partyType == "CUS")
            {
                return ExecuteQuery(
                    "SELECT c.CustomerID AS PartyID, c.CustomerName AS Name," +
                    " c.CustomerCode AS Code, c.GSTIN, c.IsActive" +
                    " FROM pk_customers c" +
                    " WHERE c.CustomerID IN (" +
                    "   SELECT DISTINCT CustomerID FROM fin_salesinvoice" +
                    "    WHERE CustomerID IS NOT NULL AND InvoiceDate BETWEEN ?s AND ?e" +
                    "   UNION" +
                    "   SELECT DISTINCT CustomerID FROM fin_receipt" +
                    "    WHERE CustomerID IS NOT NULL AND ReceiptDate BETWEEN ?s AND ?e" +
                    "   UNION" +
                    "   SELECT DISTINCT CAST(SUBSTRING(jl.ContactID,5) AS UNSIGNED)" +
                    "    FROM fin_journalline jl INNER JOIN fin_journal j ON jl.JournalID=j.JournalID" +
                    "    WHERE jl.ContactID LIKE 'CUS:%'" +
                    "      AND j.Status='POSTED' AND j.JournalDate BETWEEN ?s AND ?e" +
                    "   UNION" +
                    "   SELECT DISTINCT PartyID FROM fin_partyopeningbalance" +
                    "    WHERE PartyType='CUS' AND Amount <> 0" +
                    " )" +
                    " ORDER BY c.CustomerName;",
                    new MySqlParameter("?s", fromDate),
                    new MySqlParameter("?e", toDate));
            }
            return ExecuteQuery(
                "SELECT s.SupplierID AS PartyID, s.SupplierName AS Name," +
                " s.SupplierCode AS Code, s.GSTNo AS GSTIN, s.IsActive" +
                " FROM mm_suppliers s" +
                " WHERE s.PartyType='SUPPLIER' AND s.SupplierID IN (" +
                "   SELECT DISTINCT SupplierID FROM fin_purchaseinvoice" +
                "    WHERE SupplierID IS NOT NULL AND InvoiceDate BETWEEN ?s AND ?e" +
                "   UNION" +
                "   SELECT DISTINCT CAST(SUBSTRING(jl.ContactID,5) AS UNSIGNED)" +
                "    FROM fin_journalline jl INNER JOIN fin_journal j ON jl.JournalID=j.JournalID" +
                "    WHERE jl.ContactID LIKE 'SUP:%'" +
                "      AND j.Status='POSTED' AND j.JournalDate BETWEEN ?s AND ?e" +
                "   UNION" +
                "   SELECT DISTINCT PartyID FROM fin_partyopeningbalance" +
                "    WHERE PartyType='SUP' AND Amount <> 0" +
                " )" +
                " ORDER BY s.SupplierName;",
                new MySqlParameter("?s", fromDate),
                new MySqlParameter("?e", toDate));
        }

        public static DataRow GetParty(string partyType, int partyID)
        {
            if (partyType == "CUS")
            {
                return ExecuteQueryRow(
                    "SELECT CustomerID AS PartyID, CustomerName AS Name," +
                    " CustomerCode AS Code, GSTIN, IsActive" +
                    " FROM pk_customers WHERE CustomerID=?id LIMIT 1;",
                    new MySqlParameter("?id", partyID));
            }
            return ExecuteQueryRow(
                "SELECT SupplierID AS PartyID, SupplierName AS Name," +
                " SupplierCode AS Code, GSTNo AS GSTIN, IsActive" +
                " FROM mm_suppliers WHERE SupplierID=?id LIMIT 1;",
                new MySqlParameter("?id", partyID));
        }

        // ── PARTY STATEMENT ──

        /// <summary>
        /// Returns a DataTable with columns:
        ///   TxnDate (DateTime), VoucherNo (string), Particulars (string),
        ///   Debit (decimal), Credit (decimal), SourceTable (string)
        /// Ordered by date, then voucher. Only POSTED journals are included.
        /// Debit increases the running balance; Credit decreases.
        /// </summary>
        public static DataTable GetPartyStatement(string partyType, int partyID,
                                                  DateTime fromDate, DateTime toDate)
        {
            string contactID = partyType + ":" + partyID;
            DateTime toEnd = toDate.Date.AddDays(1).AddSeconds(-1);

            string sql;
            if (partyType == "CUS")
            {
                sql =
                    "SELECT InvoiceDate AS TxnDate, VoucherNo AS VoucherNo," +
                    " CONCAT('Sales Invoice', IFNULL(CONCAT(' — ', TallyCustomerName),'')) AS Particulars," +
                    " TotalValue AS Debit, 0 AS Credit, 'fin_salesinvoice' AS SourceTable" +
                    " FROM fin_salesinvoice" +
                    " WHERE CustomerID=?pid AND InvoiceDate BETWEEN ?s AND ?e" +

                    " UNION ALL" +

                    " SELECT ReceiptDate AS TxnDate, VoucherNo AS VoucherNo," +
                    " CONCAT('Receipt', IFNULL(CONCAT(' — ', TallyName),''),' [',ReceiptType,']') AS Particulars," +
                    " 0 AS Debit, Amount AS Credit, 'fin_receipt' AS SourceTable" +
                    " FROM fin_receipt" +
                    " WHERE CustomerID=?pid AND ReceiptDate BETWEEN ?s AND ?e" +

                    " UNION ALL" +

                    " SELECT j.JournalDate AS TxnDate, j.JournalNumber AS VoucherNo," +
                    " CONCAT('Journal', IFNULL(CONCAT(' — ', COALESCE(jl.LineDescription, j.Narration)),'')) AS Particulars," +
                    " jl.Debit AS Debit, jl.Credit AS Credit, 'fin_journal' AS SourceTable" +
                    " FROM fin_journalline jl" +
                    " INNER JOIN fin_journal j ON jl.JournalID = j.JournalID" +
                    " WHERE jl.ContactID=?cid" +
                    "   AND j.Status='POSTED'" +
                    "   AND j.JournalDate BETWEEN ?s AND ?e" +

                    " ORDER BY TxnDate, VoucherNo;";
            }
            else
            {
                sql =
                    "SELECT InvoiceDate AS TxnDate, SupplierInvNo AS VoucherNo," +
                    " CONCAT('Purchase Invoice', IFNULL(CONCAT(' — ', TallySupplierName),'')) AS Particulars," +
                    " 0 AS Debit, TotalValue AS Credit, 'fin_purchaseinvoice' AS SourceTable" +
                    " FROM fin_purchaseinvoice" +
                    " WHERE SupplierID=?pid AND InvoiceDate BETWEEN ?s AND ?e" +

                    " UNION ALL" +

                    " SELECT j.JournalDate AS TxnDate, j.JournalNumber AS VoucherNo," +
                    " CONCAT('Journal', IFNULL(CONCAT(' — ', COALESCE(jl.LineDescription, j.Narration)),'')) AS Particulars," +
                    " jl.Debit AS Debit, jl.Credit AS Credit, 'fin_journal' AS SourceTable" +
                    " FROM fin_journalline jl" +
                    " INNER JOIN fin_journal j ON jl.JournalID = j.JournalID" +
                    " WHERE jl.ContactID=?cid" +
                    "   AND j.Status='POSTED'" +
                    "   AND j.JournalDate BETWEEN ?s AND ?e" +

                    " ORDER BY TxnDate, VoucherNo;";
            }

            return ExecuteQuery(sql,
                new MySqlParameter("?pid", partyID),
                new MySqlParameter("?cid", contactID),
                new MySqlParameter("?s", fromDate),
                new MySqlParameter("?e", toEnd));
        }


        // ══════════════════════════════════════════════════════════════
        //  SERVICE PROVIDERS — stored on mm_suppliers with PartyType='SERVICE'
        // ══════════════════════════════════════════════════════════════
        //  Reuses the existing suppliers table so the code generator,
        //  Zoho vendor sync, and party-ledger scaffolding work unchanged.
        //  These parties never appear in GRN flows — all GRN list queries
        //  filter PartyType='SUPPLIER'. Service-provider transactions flow
        //  through Journal Entries (FINJournal).
        // ══════════════════════════════════════════════════════════════

        public static DataTable GetAllServiceProviders()
        {
            return ExecuteQuery(
                "SELECT SupplierID, SupplierCode, SupplierName, ContactPerson, Phone, Email," +
                " GSTNo, PAN, Address, City, State, PinCode, IsActive, CreatedAt, ServiceCategory" +
                " FROM mm_suppliers" +
                " WHERE PartyType='SERVICE'" +
                " ORDER BY SupplierName;");
        }

        public static DataTable GetActiveServiceProviders()
        {
            return ExecuteQuery(
                "SELECT SupplierID, SupplierCode, SupplierName, ServiceCategory" +
                " FROM mm_suppliers" +
                " WHERE PartyType='SERVICE' AND IsActive=1" +
                " ORDER BY SupplierName;");
        }

        public static DataRow GetServiceProviderById(int providerId)
        {
            return ExecuteQueryRow(
                "SELECT * FROM mm_suppliers WHERE SupplierID=?id AND PartyType='SERVICE' LIMIT 1;",
                new MySqlParameter("?id", providerId));
        }

        /// <summary>SRV-0001, SRV-0002, ... — separate sequence from supplier S-0001.
        /// Scans only rows whose code matches the SRV- pattern.</summary>
        public static string GenerateServiceProviderCode()
        {
            var result = ExecuteScalar(
                "SELECT IFNULL(MAX(CAST(SUBSTRING(SupplierCode, 5) AS UNSIGNED)), 0) + 1" +
                " FROM mm_suppliers WHERE SupplierCode REGEXP '^SRV-[0-9]+$';");
            int next = Convert.ToInt32(Convert.ToString(result));
            return string.Format("SRV-{0:D4}", next);
        }

        public static int AddServiceProvider(string name, string contactPerson, string phone,
            string email, string gstNo, string pan, string address, string city,
            string state, string pinCode, string serviceCategory)
        {
            string code = GenerateServiceProviderCode();
            ExecuteNonQuery(
                "INSERT INTO mm_suppliers (SupplierCode, SupplierName, ContactPerson, Phone, Email," +
                " GSTNo, PAN, Address, City, State, PinCode, IsActive, CreatedAt, PartyType, ServiceCategory)" +
                " VALUES (?code, ?name, ?cp, ?ph, ?em, ?gst, ?pan, ?addr, ?city, ?state, ?pin, 1, NOW(), 'SERVICE', ?cat);",
                new MySqlParameter("?code", code),
                new MySqlParameter("?name", name),
                new MySqlParameter("?cp",   contactPerson ?? ""),
                new MySqlParameter("?ph",   phone ?? ""),
                new MySqlParameter("?em",   email ?? ""),
                new MySqlParameter("?gst",  gstNo ?? ""),
                new MySqlParameter("?pan",  pan ?? ""),
                new MySqlParameter("?addr", address ?? ""),
                new MySqlParameter("?city", city ?? ""),
                new MySqlParameter("?state",state ?? ""),
                new MySqlParameter("?pin",  pinCode ?? ""),
                new MySqlParameter("?cat",  serviceCategory ?? ""));
            return Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
        }

        public static void UpdateServiceProvider(int providerId, string name, string contactPerson,
            string phone, string email, string gstNo, string pan, string address,
            string city, string state, string pinCode, string serviceCategory)
        {
            ExecuteNonQuery(
                "UPDATE mm_suppliers SET" +
                " SupplierName=?name, ContactPerson=?cp," +
                " Phone=?ph, Email=?em, GSTNo=?gst, PAN=?pan," +
                " Address=?addr, City=?city, State=?state, PinCode=?pin," +
                " ServiceCategory=?cat" +
                " WHERE SupplierID=?id AND PartyType='SERVICE';",
                new MySqlParameter("?name", name),
                new MySqlParameter("?cp",   contactPerson ?? ""),
                new MySqlParameter("?ph",   phone ?? ""),
                new MySqlParameter("?em",   email ?? ""),
                new MySqlParameter("?gst",  gstNo ?? ""),
                new MySqlParameter("?pan",  pan ?? ""),
                new MySqlParameter("?addr", address ?? ""),
                new MySqlParameter("?city", city ?? ""),
                new MySqlParameter("?state",state ?? ""),
                new MySqlParameter("?pin",  pinCode ?? ""),
                new MySqlParameter("?cat",  serviceCategory ?? ""),
                new MySqlParameter("?id",   providerId));
        }

        public static void ToggleServiceProviderActive(int providerId, bool isActive)
        {
            ExecuteNonQuery(
                "UPDATE mm_suppliers SET IsActive=?a WHERE SupplierID=?id AND PartyType='SERVICE';",
                new MySqlParameter("?a",  isActive ? 1 : 0),
                new MySqlParameter("?id", providerId));
        }


        // ══════════════════════════════════════════════════════════════
        //  SERVICE CATEGORIES (derived from mm_suppliers.ServiceCategory)
        // ══════════════════════════════════════════════════════════════
        //  Categories have no dedicated table — the column itself is the
        //  source of truth. GetServiceCategories() merges a seed list
        //  (for cold starts) with SELECT DISTINCT from mm_suppliers.
        //  A mass-rename on this column is how categories are renamed or
        //  merged from the management UI.
        // ══════════════════════════════════════════════════════════════

        // Seed list used when no providers exist yet. Once providers start
        // picking these, SELECT DISTINCT takes over and seeds become redundant.
        private static readonly string[] SeedServiceCategories = new[]
        {
            "Pest Control", "Security", "Housekeeping", "Maintenance",
            "Transport", "Professional Services", "Utilities", "Other"
        };

        /// <summary>Union of seed categories and distinct categories actually in use.
        /// Sorted alphabetically with "Other" pinned last. Empty / null values skipped.</summary>
        public static System.Collections.Generic.List<string> GetServiceCategories()
        {
            var set = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var seed in SeedServiceCategories)
                set.Add(seed);

            var dt = ExecuteQuery(
                "SELECT DISTINCT TRIM(ServiceCategory) AS cat" +
                " FROM mm_suppliers" +
                " WHERE PartyType='SERVICE'" +
                "   AND ServiceCategory IS NOT NULL" +
                "   AND TRIM(ServiceCategory) <> '';");
            foreach (DataRow r in dt.Rows)
            {
                string cat = r["cat"].ToString();
                if (!string.IsNullOrEmpty(cat)) set.Add(cat);
            }

            var list = new System.Collections.Generic.List<string>(set);
            list.Sort(StringComparer.OrdinalIgnoreCase);
            // Pin "Other" at the end so operators scan the real options first
            if (list.Remove("Other")) list.Add("Other");
            return list;
        }

        /// <summary>For the Manage Categories screen: each category + count of providers
        /// currently using it. Seed categories with zero usage are included too.</summary>
        public static DataTable GetCategoryUsageCounts()
        {
            // Pull actual usage counts from the table
            DataTable used = ExecuteQuery(
                "SELECT TRIM(ServiceCategory) AS Category, COUNT(*) AS ProviderCount" +
                " FROM mm_suppliers" +
                " WHERE PartyType='SERVICE'" +
                "   AND ServiceCategory IS NOT NULL" +
                "   AND TRIM(ServiceCategory) <> ''" +
                " GROUP BY TRIM(ServiceCategory)" +
                " ORDER BY Category;");

            // Build final table: all known categories (seed + used), usage counts filled in
            var result = new DataTable();
            result.Columns.Add("Category",      typeof(string));
            result.Columns.Add("ProviderCount", typeof(int));
            result.Columns.Add("IsSeed",        typeof(bool));

            var usedMap = new System.Collections.Generic.Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow r in used.Rows)
                usedMap[r["Category"].ToString()] = Convert.ToInt32(r["ProviderCount"]);

            var allCategories = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in SeedServiceCategories) allCategories.Add(s);
            foreach (var k in usedMap.Keys)          allCategories.Add(k);

            var sorted = new System.Collections.Generic.List<string>(allCategories);
            sorted.Sort(StringComparer.OrdinalIgnoreCase);
            if (sorted.Remove("Other")) sorted.Add("Other");

            foreach (var cat in sorted)
            {
                bool isSeed = false;
                foreach (var s in SeedServiceCategories)
                    if (string.Equals(s, cat, StringComparison.OrdinalIgnoreCase)) { isSeed = true; break; }

                int count = usedMap.ContainsKey(cat) ? usedMap[cat] : 0;
                result.Rows.Add(cat, count, isSeed);
            }
            return result;
        }

        /// <summary>Mass-rename a category across all service providers.
        /// Also used for merges — pass an existing category as the new name and rows are merged.
        /// Returns number of rows updated.</summary>
        public static int RenameServiceCategory(string oldName, string newName)
        {
            if (string.IsNullOrEmpty(oldName)) throw new ArgumentException("oldName required");
            if (string.IsNullOrEmpty(newName)) throw new ArgumentException("newName required");

            ExecuteNonQuery(
                "UPDATE mm_suppliers SET ServiceCategory=?new" +
                " WHERE PartyType='SERVICE' AND TRIM(ServiceCategory)=?old;",
                new MySqlParameter("?new", newName.Trim()),
                new MySqlParameter("?old", oldName.Trim()));
            // Return count via a separate query — MySqlCommand.ExecuteNonQuery returns row count,
            // but our ExecuteNonQuery helper doesn't expose it. We recompute for the caller.
            var o = ExecuteScalar(
                "SELECT COUNT(*) FROM mm_suppliers" +
                " WHERE PartyType='SERVICE' AND TRIM(ServiceCategory)=?new;",
                new MySqlParameter("?new", newName.Trim()));
            return o == null || o == DBNull.Value ? 0 : Convert.ToInt32(o);
        }


        // ══════════════════════════════════════════════════════════════
        //  SERVICE CATALOG — master list + many-to-many with providers
        //
        //  fin_services: flat list of services a Service Provider can offer.
        //  fin_serviceprovider_services: many-to-many junction (ProviderID ↔ ServiceID).
        //
        //  SupplierID is used as ProviderID because Service Providers live in
        //  mm_suppliers with PartyType='SERVICE'.
        // ══════════════════════════════════════════════════════════════

        public static DataTable GetAllServices()
        {
            return ExecuteQuery(
                "SELECT ServiceID, ServiceCode, ServiceName, Description, HSNCode," +
                " GSTRate, IsActive, CreatedAt FROM fin_services ORDER BY ServiceName;");
        }

        public static DataTable GetActiveServices()
        {
            return ExecuteQuery(
                "SELECT ServiceID, ServiceCode, ServiceName, HSNCode, GSTRate" +
                " FROM fin_services WHERE IsActive = 1 ORDER BY ServiceName;");
        }

        public static DataRow GetServiceById(int serviceId)
        {
            return ExecuteQueryRow(
                "SELECT * FROM fin_services WHERE ServiceID = ?id;",
                new MySqlParameter("?id", serviceId));
        }

        public static string GenerateServiceCode()
        {
            var o = ExecuteScalar(
                "SELECT IFNULL(MAX(CAST(SUBSTRING(ServiceCode, 5) AS UNSIGNED)), 0) + 1" +
                " FROM fin_services WHERE ServiceCode REGEXP '^SVC-[0-9]+$';");
            int next = Convert.ToInt32(Convert.ToString(o));
            return string.Format("SVC-{0:D4}", next);
        }

        public static int AddService(string name, string description, string hsn, decimal? gstRate)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Service name required");
            string code = GenerateServiceCode();
            ExecuteNonQuery(
                "INSERT INTO fin_services (ServiceCode, ServiceName, Description, HSNCode, GSTRate, IsActive)" +
                " VALUES (?code, ?name, ?desc, ?hsn, ?gst, 1);",
                new MySqlParameter("?code", code),
                new MySqlParameter("?name", name.Trim()),
                new MySqlParameter("?desc", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description.Trim()),
                new MySqlParameter("?hsn",  string.IsNullOrEmpty(hsn)         ? (object)DBNull.Value : hsn.Trim()),
                new MySqlParameter("?gst",  (object)gstRate ?? DBNull.Value));
            return Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
        }

        public static void UpdateService(int serviceId, string name, string description, string hsn, decimal? gstRate)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Service name required");
            ExecuteNonQuery(
                "UPDATE fin_services SET ServiceName=?name, Description=?desc, HSNCode=?hsn, GSTRate=?gst" +
                " WHERE ServiceID=?id;",
                new MySqlParameter("?name", name.Trim()),
                new MySqlParameter("?desc", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description.Trim()),
                new MySqlParameter("?hsn",  string.IsNullOrEmpty(hsn)         ? (object)DBNull.Value : hsn.Trim()),
                new MySqlParameter("?gst",  (object)gstRate ?? DBNull.Value),
                new MySqlParameter("?id",   serviceId));
        }

        public static void ToggleServiceActive(int serviceId, bool isActive)
        {
            ExecuteNonQuery(
                "UPDATE fin_services SET IsActive=?a WHERE ServiceID=?id;",
                new MySqlParameter("?a",  isActive ? 1 : 0),
                new MySqlParameter("?id", serviceId));
        }

        public static int GetProviderCountForService(int serviceId)
        {
            var o = ExecuteScalar(
                "SELECT COUNT(*) FROM fin_serviceprovider_services WHERE ServiceID = ?id;",
                new MySqlParameter("?id", serviceId));
            return Convert.ToInt32(o);
        }

        public static DataTable GetServicesWithUsage()
        {
            return ExecuteQuery(
                "SELECT s.ServiceID, s.ServiceCode, s.ServiceName, s.Description," +
                " s.HSNCode, s.GSTRate, s.IsActive," +
                " (SELECT COUNT(*) FROM fin_serviceprovider_services WHERE ServiceID = s.ServiceID) AS ProviderCount" +
                " FROM fin_services s ORDER BY s.ServiceName;");
        }

        // ── Junction (provider ↔ services) ───────────────────────────

        /// <summary>List services linked to a given Service Provider (SupplierID with PartyType='SERVICE').</summary>
        public static DataTable GetServicesForProvider(int providerId)
        {
            return ExecuteQuery(
                "SELECT s.ServiceID, s.ServiceCode, s.ServiceName, s.HSNCode, s.GSTRate" +
                " FROM fin_services s" +
                " JOIN fin_serviceprovider_services j ON j.ServiceID = s.ServiceID" +
                " WHERE j.ProviderID = ?pid" +
                " ORDER BY s.ServiceName;",
                new MySqlParameter("?pid", providerId));
        }

        /// <summary>Replace the junction rows for a provider in one shot.
        /// Uses DELETE+INSERT inside a transaction so the set is atomic.</summary>
        public static void SaveProviderServices(int providerId, System.Collections.Generic.IEnumerable<int> serviceIds)
        {
            using (var conn = OpenConnection())
            using (var tx = conn.BeginTransaction())
            {
                using (var cmd = new MySqlCommand(
                    "DELETE FROM fin_serviceprovider_services WHERE ProviderID = ?pid;", conn, tx))
                {
                    cmd.Parameters.AddWithValue("?pid", providerId);
                    cmd.ExecuteNonQuery();
                }
                if (serviceIds != null)
                {
                    foreach (int sid in serviceIds)
                    {
                        using (var cmd = new MySqlCommand(
                            "INSERT INTO fin_serviceprovider_services (ProviderID, ServiceID) VALUES (?pid, ?sid);", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("?pid", providerId);
                            cmd.Parameters.AddWithValue("?sid", sid);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                tx.Commit();
            }
        }

        /// <summary>Summary of providers with concatenated service names (for a list view).</summary>
        public static DataTable GetProvidersWithServiceSummary()
        {
            return ExecuteQuery(
                "SELECT sp.SupplierID AS ProviderID, sp.SupplierCode AS ProviderCode," +
                " sp.SupplierName AS ProviderName, sp.ContactPerson, sp.Phone, sp.Email," +
                " sp.ServiceCategory, sp.IsActive," +
                " GROUP_CONCAT(s.ServiceName ORDER BY s.ServiceName SEPARATOR ', ') AS Services" +
                " FROM mm_suppliers sp" +
                " LEFT JOIN fin_serviceprovider_services j ON j.ProviderID = sp.SupplierID" +
                " LEFT JOIN fin_services s ON s.ServiceID = j.ServiceID" +
                " WHERE sp.PartyType = 'SERVICE'" +
                " GROUP BY sp.SupplierID, sp.SupplierCode, sp.SupplierName, sp.ContactPerson," +
                " sp.Phone, sp.Email, sp.ServiceCategory, sp.IsActive" +
                " ORDER BY sp.SupplierName;");
        }


        // ══════════════════════════════════════════════════════════════
        //  BANK POSTINGS — bank accounts + layouts + statements + lines
        //
        //  Workflow:
        //    1. User defines a bank in fin_bankaccounts (links to Zoho Chart of Accounts).
        //    2. User configures the XLSX column layout in fin_banklayouts (1:1 per bank).
        //    3. User uploads a bank statement — parsed rows go into
        //       fin_bankstatementlines with a dedup fingerprint.
        //    4. (Phase 2) Each row gets posted as a JV to ERP and Zoho.
        // ══════════════════════════════════════════════════════════════

        // ── Bank account master ───────────────────────────────────────

        public static DataTable GetAllBankAccounts()
        {
            return ExecuteQuery(
                "SELECT BankID, BankCode, BankName, AccountNumber, Branch," +
                " ZohoAccountID, ZohoAccountName, IsActive, CreatedAt" +
                " FROM fin_bankaccounts ORDER BY BankName;");
        }

        public static DataTable GetActiveBankAccounts()
        {
            return ExecuteQuery(
                "SELECT BankID, BankCode, BankName, AccountNumber" +
                " FROM fin_bankaccounts WHERE IsActive = 1 ORDER BY BankName;");
        }

        public static DataRow GetBankAccountById(int bankId)
        {
            return ExecuteQueryRow(
                "SELECT * FROM fin_bankaccounts WHERE BankID=?id;",
                new MySqlParameter("?id", bankId));
        }

        public static string GenerateBankCode()
        {
            var o = ExecuteScalar(
                "SELECT IFNULL(MAX(CAST(SUBSTRING(BankCode, 5) AS UNSIGNED)), 0) + 1" +
                " FROM fin_bankaccounts WHERE BankCode REGEXP '^BNK-[0-9]+$';");
            int next = Convert.ToInt32(Convert.ToString(o));
            return string.Format("BNK-{0:D3}", next);
        }

        public static int AddBankAccount(string bankName, string accountNo, string branch,
                                          string zohoAccountId, string zohoAccountName,
                                          int createdByUserId)
        {
            if (string.IsNullOrEmpty(bankName)) throw new ArgumentException("Bank name required");
            string code = GenerateBankCode();
            ExecuteNonQuery(
                "INSERT INTO fin_bankaccounts" +
                " (BankCode, BankName, AccountNumber, Branch, ZohoAccountID, ZohoAccountName, IsActive, CreatedBy)" +
                " VALUES (?code, ?name, ?acct, ?br, ?zid, ?zname, 1, ?uid);",
                new MySqlParameter("?code", code),
                new MySqlParameter("?name", bankName.Trim()),
                new MySqlParameter("?acct", string.IsNullOrEmpty(accountNo) ? (object)DBNull.Value : accountNo.Trim()),
                new MySqlParameter("?br",   string.IsNullOrEmpty(branch)    ? (object)DBNull.Value : branch.Trim()),
                new MySqlParameter("?zid",  string.IsNullOrEmpty(zohoAccountId) ? (object)DBNull.Value : zohoAccountId.Trim()),
                new MySqlParameter("?zname",string.IsNullOrEmpty(zohoAccountName) ? (object)DBNull.Value : zohoAccountName.Trim()),
                new MySqlParameter("?uid",  createdByUserId));
            var r = ExecuteScalar("SELECT LAST_INSERT_ID();");
            int bankId = Convert.ToInt32(r);

            // Seed an empty layout row so the layout editor can UPDATE it.
            ExecuteNonQuery(
                "INSERT INTO fin_banklayouts (BankID, AmountMode, IsConfigured) VALUES (?bid, 'TWO_COL', 0);",
                new MySqlParameter("?bid", bankId));

            return bankId;
        }

        public static void UpdateBankAccount(int bankId, string bankName, string accountNo, string branch,
                                              string zohoAccountId, string zohoAccountName)
        {
            if (string.IsNullOrEmpty(bankName)) throw new ArgumentException("Bank name required");
            ExecuteNonQuery(
                "UPDATE fin_bankaccounts SET" +
                " BankName=?name, AccountNumber=?acct, Branch=?br," +
                " ZohoAccountID=?zid, ZohoAccountName=?zname" +
                " WHERE BankID=?id;",
                new MySqlParameter("?name", bankName.Trim()),
                new MySqlParameter("?acct", string.IsNullOrEmpty(accountNo) ? (object)DBNull.Value : accountNo.Trim()),
                new MySqlParameter("?br",   string.IsNullOrEmpty(branch)    ? (object)DBNull.Value : branch.Trim()),
                new MySqlParameter("?zid",  string.IsNullOrEmpty(zohoAccountId) ? (object)DBNull.Value : zohoAccountId.Trim()),
                new MySqlParameter("?zname",string.IsNullOrEmpty(zohoAccountName) ? (object)DBNull.Value : zohoAccountName.Trim()),
                new MySqlParameter("?id",   bankId));
        }

        public static void ToggleBankAccountActive(int bankId, bool isActive)
        {
            ExecuteNonQuery(
                "UPDATE fin_bankaccounts SET IsActive=?a WHERE BankID=?id;",
                new MySqlParameter("?a",  isActive ? 1 : 0),
                new MySqlParameter("?id", bankId));
        }

        // ── XLSX column layout ────────────────────────────────────────

        public static DataRow GetBankLayout(int bankId)
        {
            return ExecuteQueryRow(
                "SELECT * FROM fin_banklayouts WHERE BankID=?id;",
                new MySqlParameter("?id", bankId));
        }

        public static void SaveBankLayout(int bankId, int headerRow, int firstDataRow,
                                           string dateCol, string descCol, string refCol,
                                           string amountMode,
                                           string debitCol, string creditCol,
                                           string amountCol, string flagCol,
                                           string balanceCol, string dateFormat)
        {
            ExecuteNonQuery(
                "UPDATE fin_banklayouts SET" +
                " HeaderRow=?h, FirstDataRow=?f," +
                " DateCol=?dc, DescCol=?dsc, RefCol=?rc," +
                " AmountMode=?am, DebitCol=?deb, CreditCol=?crd," +
                " AmountCol=?amt, FlagCol=?flg, BalanceCol=?bal," +
                " DateFormat=?df, IsConfigured=1" +
                " WHERE BankID=?id;",
                new MySqlParameter("?h", headerRow),
                new MySqlParameter("?f", firstDataRow),
                new MySqlParameter("?dc",  string.IsNullOrEmpty(dateCol)   ? (object)DBNull.Value : dateCol.ToUpperInvariant()),
                new MySqlParameter("?dsc", string.IsNullOrEmpty(descCol)   ? (object)DBNull.Value : descCol.ToUpperInvariant()),
                new MySqlParameter("?rc",  string.IsNullOrEmpty(refCol)    ? (object)DBNull.Value : refCol.ToUpperInvariant()),
                new MySqlParameter("?am", amountMode),
                new MySqlParameter("?deb", string.IsNullOrEmpty(debitCol)  ? (object)DBNull.Value : debitCol.ToUpperInvariant()),
                new MySqlParameter("?crd", string.IsNullOrEmpty(creditCol) ? (object)DBNull.Value : creditCol.ToUpperInvariant()),
                new MySqlParameter("?amt", string.IsNullOrEmpty(amountCol) ? (object)DBNull.Value : amountCol.ToUpperInvariant()),
                new MySqlParameter("?flg", string.IsNullOrEmpty(flagCol)   ? (object)DBNull.Value : flagCol.ToUpperInvariant()),
                new MySqlParameter("?bal", string.IsNullOrEmpty(balanceCol)? (object)DBNull.Value : balanceCol.ToUpperInvariant()),
                new MySqlParameter("?df",  string.IsNullOrEmpty(dateFormat)? "dd/MM/yyyy" : dateFormat),
                new MySqlParameter("?id",  bankId));
        }

        // ── Statements ────────────────────────────────────────────────

        public static int CreateBankStatement(int bankId, string fileName,
                                               DateTime? periodStart, DateTime? periodEnd,
                                               decimal? openBal, decimal? closeBal,
                                               int uploadedBy)
        {
            ExecuteNonQuery(
                "INSERT INTO fin_bankstatements" +
                " (BankID, FileName, PeriodStart, PeriodEnd, OpeningBalance, ClosingBalance, RowCount, DuplicateCount, UploadedBy)" +
                " VALUES (?bid, ?fn, ?ps, ?pe, ?ob, ?cb, 0, 0, ?uid);",
                new MySqlParameter("?bid", bankId),
                new MySqlParameter("?fn",  fileName),
                new MySqlParameter("?ps",  (object)periodStart ?? DBNull.Value),
                new MySqlParameter("?pe",  (object)periodEnd   ?? DBNull.Value),
                new MySqlParameter("?ob",  (object)openBal     ?? DBNull.Value),
                new MySqlParameter("?cb",  (object)closeBal    ?? DBNull.Value),
                new MySqlParameter("?uid", uploadedBy));
            return Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
        }

        /// <summary>Insert one parsed line. Uses INSERT IGNORE so exact-duplicate
        /// rows are silently skipped. Returns 1 if inserted, 0 if skipped.</summary>
        public static int InsertBankLine(int statementId, int bankId, int rowSeq,
                                          DateTime txnDate, string description, string reference,
                                          decimal debit, decimal credit, decimal? balance)
        {
            string descClean = string.IsNullOrEmpty(description) ? "" : description.Trim();
            string descHash;
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var bytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(descClean));
                var sb = new System.Text.StringBuilder();
                foreach (var b in bytes) sb.Append(b.ToString("x2"));
                descHash = sb.ToString();
            }

            using (var conn = OpenConnection())
            using (var cmd = new MySqlCommand(
                "INSERT IGNORE INTO fin_bankstatementlines" +
                " (StatementID, BankID, RowSeq, TxnDate, Description, Reference, Debit, Credit, Balance, DescHash)" +
                " VALUES (?sid, ?bid, ?seq, ?dt, ?desc, ?ref, ?deb, ?crd, ?bal, ?dh);", conn))
            {
                cmd.Parameters.AddWithValue("?sid",  statementId);
                cmd.Parameters.AddWithValue("?bid",  bankId);
                cmd.Parameters.AddWithValue("?seq",  rowSeq);
                cmd.Parameters.AddWithValue("?dt",   txnDate);
                cmd.Parameters.AddWithValue("?desc", (object)descClean ?? DBNull.Value);
                cmd.Parameters.AddWithValue("?ref",  string.IsNullOrEmpty(reference) ? (object)DBNull.Value : reference.Trim());
                cmd.Parameters.AddWithValue("?deb",  debit);
                cmd.Parameters.AddWithValue("?crd",  credit);
                cmd.Parameters.AddWithValue("?bal",  (object)balance ?? DBNull.Value);
                cmd.Parameters.AddWithValue("?dh",   descHash);
                return cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateStatementCounts(int statementId, int rowCount, int duplicateCount)
        {
            ExecuteNonQuery(
                "UPDATE fin_bankstatements SET RowCount=?rc, DuplicateCount=?dc WHERE StatementID=?id;",
                new MySqlParameter("?rc", rowCount),
                new MySqlParameter("?dc", duplicateCount),
                new MySqlParameter("?id", statementId));
        }

        public static DataTable ListBankStatements(int? bankId)
        {
            string sql =
                "SELECT s.StatementID, s.BankID, b.BankCode, b.BankName," +
                " s.FileName, s.PeriodStart, s.PeriodEnd," +
                " s.OpeningBalance, s.ClosingBalance," +
                " s.RowCount, s.DuplicateCount, s.UploadedAt," +
                " u.FullName AS UploadedByName" +
                " FROM fin_bankstatements s" +
                " JOIN fin_bankaccounts b ON b.BankID = s.BankID" +
                " LEFT JOIN Users u ON u.UserID = s.UploadedBy" +
                (bankId.HasValue ? " WHERE s.BankID = ?bid" : "") +
                " ORDER BY s.UploadedAt DESC;";
            if (bankId.HasValue)
                return ExecuteQuery(sql, new MySqlParameter("?bid", bankId.Value));
            return ExecuteQuery(sql);
        }

        public static DataTable GetStatementLines(int statementId)
        {
            return ExecuteQuery(
                "SELECT LineID, RowSeq, TxnDate, Description, Reference," +
                " Debit, Credit, Balance, Status, JournalID" +
                " FROM fin_bankstatementlines" +
                " WHERE StatementID = ?sid ORDER BY RowSeq;",
                new MySqlParameter("?sid", statementId));
        }

        public static DataRow GetBankStatementHeader(int statementId)
        {
            return ExecuteQueryRow(
                "SELECT s.*, b.BankCode, b.BankName" +
                " FROM fin_bankstatements s" +
                " JOIN fin_bankaccounts b ON b.BankID = s.BankID" +
                " WHERE s.StatementID = ?id;",
                new MySqlParameter("?id", statementId));
        }

    }
}
