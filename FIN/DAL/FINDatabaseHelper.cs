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
                " i.Quantity, i.Rate, i.HSNCode, i.GSTRate, i.GSTAmount, i.Amount, i.Status, " +
                " bl.ZohoBillID, bl.ZohoBillNo, bl.ZohoStatus, bl.PushStatus, bl.ErrorMessage, " +
                " bl.PushedAt, bl.BillTotal " +
                "FROM mm_rawinward i " +
                "JOIN mm_suppliers s ON s.SupplierID = i.SupplierID " +
                "JOIN mm_rawmaterials rm ON rm.RMID = i.RMID " +
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
                " i.Quantity, i.Rate, i.HSNCode, i.GSTRate, i.GSTAmount, i.Amount, i.Status, " +
                " bl.ZohoBillID, bl.ZohoBillNo, bl.ZohoStatus, bl.PushStatus, bl.ErrorMessage, " +
                " bl.PushedAt, bl.BillTotal " +
                "FROM mm_packinginward i " +
                "JOIN mm_suppliers s ON s.SupplierID = i.SupplierID " +
                "JOIN mm_packingmaterials pm ON pm.PMID = i.PMID " +
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

        /// <summary>Thin wrapper — delegates to the PK helper which owns the Zoho API client
        /// and the auto-create logic for vendors and items.</summary>
        public static FINApp.DAL.ZohoBillPushResult PushGRNToZoho(int grnId, string grnType, int userId)
        {
            return FINApp.DAL.FINZohoHelper.CreateBillFromGRN(grnId, grnType, userId);
        }

        /// <summary>Summary counts for the GRN-to-Zoho dashboard tabs.
        /// Returns a row per GRN type with PendingCount, PushedCount, ErrorCount.</summary>
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

            string sql =
                "SELECT 'RAW' AS GRNType, " +
                "  SUM(CASE WHEN bl.ZohoBillID IS NULL OR bl.ZohoBillID = '' THEN 1 ELSE 0 END) AS PendingCount, " +
                "  SUM(CASE WHEN bl.ZohoBillID IS NOT NULL AND bl.ZohoBillID <> '' THEN 1 ELSE 0 END) AS PushedCount, " +
                "  SUM(CASE WHEN bl.PushStatus = 'Error' THEN 1 ELSE 0 END) AS ErrorCount, " +
                "  COUNT(*) AS TotalCount " +
                "FROM mm_rawinward i " +
                "LEFT JOIN zoho_billlog bl ON bl.GRNID = i.InwardID AND bl.GRNType = 'RAW' " +
                exclFilter + dateFilter +
                "UNION ALL " +
                "SELECT 'PACKING' AS GRNType, " +
                "  SUM(CASE WHEN bl.ZohoBillID IS NULL OR bl.ZohoBillID = '' THEN 1 ELSE 0 END), " +
                "  SUM(CASE WHEN bl.ZohoBillID IS NOT NULL AND bl.ZohoBillID <> '' THEN 1 ELSE 0 END), " +
                "  SUM(CASE WHEN bl.PushStatus = 'Error' THEN 1 ELSE 0 END), " +
                "  COUNT(*) " +
                "FROM mm_packinginward i " +
                "LEFT JOIN zoho_billlog bl ON bl.GRNID = i.InwardID AND bl.GRNType = 'PACKING' " +
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
        /// the supplier dropdown filter on the dashboard.</summary>
        public static DataTable GetSuppliersWithGRNs()
        {
            string sql =
                "SELECT DISTINCT s.SupplierID, s.SupplierName " +
                "FROM mm_suppliers s " +
                "WHERE s.SupplierID <> 306 AND (" +
                "  EXISTS (SELECT 1 FROM mm_rawinward i WHERE i.SupplierID = s.SupplierID " +
                "     AND i.GRNNo NOT LIKE 'INT-%' AND i.GRNNo NOT LIKE 'PREP-%' AND i.GRNNo NOT LIKE 'PRE-%') OR " +
                "  EXISTS (SELECT 1 FROM mm_packinginward i WHERE i.SupplierID = s.SupplierID " +
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
            string sql =
                "SELECT j.JournalID, j.JournalNumber, j.JournalDate, j.Narration, j.Reference, " +
                " j.Status, j.TotalDebit, j.TotalCredit, j.ReversedByJournalID, " +
                " j.CreatedAt, j.PostedAt, j.ReversedAt, " +
                " u.FullName AS CreatedByName " +
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
            var dt = new DataTable();
            using (var conn = new MySqlConnection(ConnectionString))
            using (var cmd = new MySqlCommand(
                "SELECT CONCAT('SUP:', SupplierID) AS PartyKey, 'SUP' AS PartyType, " +
                "  SupplierID AS PartyID, SupplierCode AS PartyCode, SupplierName AS PartyName, " +
                "  GSTNo AS GSTNo, City, State " +
                "FROM MM_Suppliers WHERE IsActive = 1 " +
                "UNION ALL " +
                "SELECT CONCAT('CUS:', CustomerID) AS PartyKey, 'CUS' AS PartyType, " +
                "  CustomerID AS PartyID, CustomerCode AS PartyCode, CustomerName AS PartyName, " +
                "  GSTIN AS GSTNo, City, State " +
                "FROM PK_Customers WHERE IsActive = 1 " +
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
                string sql = parts[0] == "SUP"
                    ? "SELECT SupplierName FROM MM_Suppliers WHERE SupplierID = ?pid"
                    : parts[0] == "CUS"
                        ? "SELECT CustomerName FROM PK_Customers WHERE CustomerID = ?pid"
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
    }
}
