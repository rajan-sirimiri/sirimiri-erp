using System;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

namespace MMApp.DAL
{
    public static class MMDatabaseHelper
    {
        private static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["StockDB"].ConnectionString;

        // ── CORE PRIVATE HELPERS ──────────────────────────────────────
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

        private static DataRow ExecuteQuerySingleRow(string sql, params MySqlParameter[] parms)
        {
            var dt = ExecuteQuery(sql, parms);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

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

        // ── AUTH — reuse Users table from StockDB ─────────────────────
        public static DataRow ValidateUser(string username, string passwordHash)
        {
            return ExecuteQuerySingleRow(
                "SELECT UserID, FullName, Username, Role, IsActive, MustChangePwd " +
                "FROM Users WHERE Username=?u AND PasswordHash=?p AND IsActive=1;",
                new MySqlParameter("u", username),
                new MySqlParameter("p", passwordHash));
        }

        public static void UpdateLastLogin(int userId)
        {
            ExecuteNonQuery("UPDATE Users SET LastLogin=NOW() WHERE UserID=?id;",
                new MySqlParameter("id", userId));
        }

        public static DataRow GetUserByID(int userId)
        {
            return ExecuteQuerySingleRow(
                "SELECT UserID, FullName, Username, Role, StateID, IsActive, MustChangePwd FROM Users WHERE UserID=?id;",
                new MySqlParameter("id", userId));
        }

        public static bool VerifyPassword(int userId, string passwordHash)
        {
            var result = ExecuteScalar(
                "SELECT COUNT(*) FROM Users WHERE UserID=?id AND PasswordHash=?p;",
                new MySqlParameter("id", userId),
                new MySqlParameter("p", passwordHash));
            return Convert.ToInt32(result) > 0;
        }

        public static void ChangePassword(int userId, string newHash)
        {
            ExecuteNonQuery(
                "UPDATE Users SET PasswordHash=?h, MustChangePwd=0 WHERE UserID=?id;",
                new MySqlParameter("h", newHash),
                new MySqlParameter("id", userId));
        }

        // ── ACCESS CONTROL ────────────────────────────────────────────
        public static bool HasAccess(int userId, string module)
        {
            // Admin always has access
            var user = GetUserByID(userId);
            if (user != null && user["Role"].ToString() == "Admin") return true;

            var result = ExecuteScalar(
                "SELECT COUNT(*) FROM MM_UserAccess WHERE UserID=?uid AND Module=?mod AND CanView=1;",
                new MySqlParameter("uid", userId),
                new MySqlParameter("mod", module));
            return Convert.ToInt32(result) > 0;
        }

        public static DataTable GetUserAccessList(int userId)
        {
            return ExecuteQuery(
                "SELECT Module, CanView, CanEdit FROM MM_UserAccess WHERE UserID=?uid;",
                new MySqlParameter("uid", userId));
        }

        // ── UOM MASTER ────────────────────────────────────────────────
        public static DataTable GetAllUOM()
        {
            return ExecuteQuery(
                "SELECT UOMID, UOMName, Abbreviation, IsActive FROM MM_UOM ORDER BY UOMName;");
        }

        public static DataTable GetActiveUOM()
        {
            return ExecuteQuery(
                "SELECT UOMID, UOMName, Abbreviation FROM MM_UOM WHERE IsActive=1 ORDER BY UOMName;");
        }

        public static DataRow GetUOMById(int uomId)
        {
            return ExecuteQuerySingleRow(
                "SELECT UOMID, UOMName, Abbreviation, IsActive FROM MM_UOM WHERE UOMID=?id;",
                new MySqlParameter("id", uomId));
        }

        public static void AddUOM(string name, string abbreviation)
        {
            ExecuteNonQuery(
                "INSERT INTO MM_UOM (UOMName, Abbreviation, IsActive) VALUES (?n, ?a, 1);",
                new MySqlParameter("n", name),
                new MySqlParameter("a", abbreviation));
        }

        public static void UpdateUOM(int uomId, string name, string abbreviation)
        {
            ExecuteNonQuery(
                "UPDATE MM_UOM SET UOMName=?n, Abbreviation=?a WHERE UOMID=?id;",
                new MySqlParameter("n", name),
                new MySqlParameter("a", abbreviation),
                new MySqlParameter("id", uomId));
        }

        public static void ToggleUOMActive(int uomId, bool isActive)
        {
            ExecuteNonQuery(
                "UPDATE MM_UOM SET IsActive=?a WHERE UOMID=?id;",
                new MySqlParameter("a", isActive ? 1 : 0),
                new MySqlParameter("id", uomId));
        }

        // ── SUPPLIER ─────────────────────────────────────────────────
        public static DataTable GetAllSuppliers()
        {
            return ExecuteQuery(
                "SELECT SupplierID, SupplierCode, SupplierName, ContactPerson, Phone, Email, " +
                "GSTNo, PAN, Address, City, State, PinCode, IsActive, CreatedAt " +
                "FROM MM_Suppliers ORDER BY SupplierName;");
        }

        public static DataTable GetActiveSuppliers()
        {
            return ExecuteQuery(
                "SELECT SupplierID, SupplierCode, SupplierName FROM MM_Suppliers WHERE IsActive=1 ORDER BY SupplierName;");
        }

        public static DataRow GetSupplierById(int supplierId)
        {
            return ExecuteQuerySingleRow(
                "SELECT * FROM MM_Suppliers WHERE SupplierID=?id;",
                new MySqlParameter("id", supplierId));
        }

        public static bool SupplierCodeExists(string code, int excludeId = 0)
        {
            var result = ExecuteScalar(
                "SELECT COUNT(*) FROM MM_Suppliers WHERE SupplierCode=?c AND SupplierID<>?id;",
                new MySqlParameter("c", code),
                new MySqlParameter("id", excludeId));
            return Convert.ToInt32(result) > 0;
        }

        public static int AddSupplier(string name, string contactPerson, string phone,
            string email, string gstNo, string pan, string address, string city, string state, string pinCode)
        {
            string code = GenerateSupplierCode();
            ExecuteNonQuery(
                "INSERT INTO MM_Suppliers (SupplierCode, SupplierName, ContactPerson, Phone, Email, " +
                "GSTNo, PAN, Address, City, State, PinCode, IsActive, CreatedAt) " +
                "VALUES (?code,?name,?cp,?ph,?em,?gst,?pan,?addr,?city,?state,?pin,1,NOW());",
                new MySqlParameter("code", code),
                new MySqlParameter("name", name),
                new MySqlParameter("cp",   contactPerson),
                new MySqlParameter("ph",   phone),
                new MySqlParameter("em",   email),
                new MySqlParameter("gst",  gstNo),
                new MySqlParameter("pan",  pan),
                new MySqlParameter("addr", address),
                new MySqlParameter("city", city),
                new MySqlParameter("state",state),
                new MySqlParameter("pin",  pinCode));
            return Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
        }

        public static void UpdateSupplier(int supplierId, string code, string name, string contactPerson,
            string phone, string email, string gstNo, string pan, string address,
            string city, string state, string pinCode)
        {
            ExecuteNonQuery(
                "UPDATE MM_Suppliers SET SupplierCode=?code, SupplierName=?name, ContactPerson=?cp, " +
                "Phone=?ph, Email=?em, GSTNo=?gst, PAN=?pan, Address=?addr, City=?city, " +
                "State=?state, PinCode=?pin WHERE SupplierID=?id;",
                new MySqlParameter("code", code),
                new MySqlParameter("name", name),
                new MySqlParameter("cp",   contactPerson),
                new MySqlParameter("ph",   phone),
                new MySqlParameter("em",   email),
                new MySqlParameter("gst",  gstNo),
                new MySqlParameter("pan",  pan),
                new MySqlParameter("addr", address),
                new MySqlParameter("city", city),
                new MySqlParameter("state",state),
                new MySqlParameter("pin",  pinCode),
                new MySqlParameter("id",   supplierId));
        }

        public static void ToggleSupplierActive(int supplierId, bool isActive)
        {
            ExecuteNonQuery(
                "UPDATE MM_Suppliers SET IsActive=?a WHERE SupplierID=?id;",
                new MySqlParameter("a",  isActive ? 1 : 0),
                new MySqlParameter("id", supplierId));
        }

        // ── RAW MATERIAL ──────────────────────────────────────────────
        // Raw Material Stock Report
        // CurrentStock = OpeningStock + GRN received - Consumed (FIFO deductions)
        public static DataTable GetRMStockReport()
        {
            return ExecuteQuery(
                "SELECT r.RMID, r.RMCode, r.RMName, u.Abbreviation AS UOM," +
                " ROUND(" +
                "   IFNULL(os.Quantity, 0)" +
                "   + IFNULL(grn.TotalReceived, 0)" +
                "   - IFNULL(con.TotalConsumed, 0)" +
                " , 4) AS CurrentStock," +
                " r.ReorderLevel," +
                " NULL AS ReconStatus," +
                " NULL AS ReconDate" +
                " FROM MM_RawMaterials r" +
                " JOIN MM_UOM u ON u.UOMID = r.UOMID" +
                " LEFT JOIN MM_OpeningStock os ON os.MaterialType='RM' AND os.MaterialID=r.RMID" +
                " LEFT JOIN (SELECT RMID, SUM(QtyActualReceived) AS TotalReceived" +
                "            FROM MM_RawInward GROUP BY RMID) grn ON grn.RMID = r.RMID" +
                " LEFT JOIN (SELECT RMID, SUM(QtyConsumed) AS TotalConsumed" +
                "            FROM MM_StockConsumption GROUP BY RMID) con ON con.RMID = r.RMID" +
                " WHERE r.IsActive = 1" +
                " ORDER BY CurrentStock DESC, r.RMName ASC;");
        }

        public static DataTable GetAllRawMaterials()
        {
            return ExecuteQuery(
                "SELECT r.RMID, r.RMCode, r.RMName, r.Description, r.HSNCode, r.GSTRate, r.UOMID, u.UOMName, u.Abbreviation, " +
                "r.ReorderLevel, r.IsActive, r.CreatedAt " +
                "FROM MM_RawMaterials r JOIN MM_UOM u ON u.UOMID=r.UOMID ORDER BY r.RMName;");
        }

        public static DataTable GetActiveRawMaterials()
        {
            return ExecuteQuery(
                "SELECT r.RMID, r.RMCode, r.RMName, u.Abbreviation " +
                "FROM MM_RawMaterials r JOIN MM_UOM u ON u.UOMID=r.UOMID WHERE r.IsActive=1 ORDER BY r.RMName;");
        }

        public static DataRow GetRawMaterialById(int rmId)
        {
            return ExecuteQuerySingleRow(
                "SELECT r.*, u.UOMName, u.Abbreviation FROM MM_RawMaterials r " +
                "JOIN MM_UOM u ON u.UOMID=r.UOMID WHERE r.RMID=?id;",
                new MySqlParameter("id", rmId));
        }

        public static bool RMCodeExists(string code, int excludeId = 0)
        {
            var result = ExecuteScalar(
                "SELECT COUNT(*) FROM MM_RawMaterials WHERE RMCode=?c AND RMID<>?id;",
                new MySqlParameter("c",  code),
                new MySqlParameter("id", excludeId));
            return Convert.ToInt32(result) > 0;
        }

        public static void AddRawMaterial(string name, string description, string hsnCode, decimal? gstRate, int uomId, decimal reorderLevel)
        {
            string code = GenerateRMCode();
            ExecuteNonQuery(
                "INSERT INTO MM_RawMaterials (RMCode, RMName, Description, HSNCode, GSTRate, UOMID, ReorderLevel, IsActive, CreatedAt) " +
                "VALUES (?code,?name,?desc,?hsn,?gst,?uom,?reorder,1,NOW());",
                new MySqlParameter("code",   code),
                new MySqlParameter("name",   name),
                new MySqlParameter("desc",   description),
                new MySqlParameter("hsn",    hsnCode ?? (object)DBNull.Value),
                new MySqlParameter("gst",    gstRate.HasValue ? (object)gstRate.Value : DBNull.Value),
                new MySqlParameter("uom",    uomId),
                new MySqlParameter("reorder",reorderLevel));
        }

        public static void UpdateRawMaterial(int rmId, string code, string name, string description,
            string hsnCode, decimal? gstRate, int uomId, decimal reorderLevel)
        {
            ExecuteNonQuery(
                "UPDATE MM_RawMaterials SET RMCode=?code, RMName=?name, Description=?desc, " +
                "HSNCode=?hsn, GSTRate=?gst, UOMID=?uom, ReorderLevel=?reorder WHERE RMID=?id;",
                new MySqlParameter("code",   code),
                new MySqlParameter("name",   name),
                new MySqlParameter("desc",   description),
                new MySqlParameter("hsn",    hsnCode ?? (object)DBNull.Value),
                new MySqlParameter("gst",    gstRate.HasValue ? (object)gstRate.Value : DBNull.Value),
                new MySqlParameter("uom",    uomId),
                new MySqlParameter("reorder",reorderLevel),
                new MySqlParameter("id",     rmId));
        }

        public static void ToggleRawMaterialActive(int rmId, bool isActive)
        {
            ExecuteNonQuery(
                "UPDATE MM_RawMaterials SET IsActive=?a WHERE RMID=?id;",
                new MySqlParameter("a",  isActive ? 1 : 0),
                new MySqlParameter("id", rmId));
        }

        // ── SCRAP MATERIALS ──────────────────────────────────────────────────────

        public static DataTable GetAllScrapMaterials()
        {
            return ExecuteQuery(
                "SELECT s.ScrapID, s.ScrapCode, s.ScrapName, s.Description," +
                " s.UOMID, u.UOMName, u.Abbreviation, s.IsActive, s.CreatedAt" +
                " FROM MM_ScrapMaterials s JOIN MM_UOM u ON u.UOMID=s.UOMID" +
                " ORDER BY s.ScrapName;");
        }

        public static DataTable GetActiveScrapMaterials()
        {
            return ExecuteQuery(
                "SELECT s.ScrapID, s.ScrapCode, s.ScrapName, u.Abbreviation" +
                " FROM MM_ScrapMaterials s JOIN MM_UOM u ON u.UOMID=s.UOMID" +
                " WHERE s.IsActive=1 ORDER BY s.ScrapName;");
        }

        public static DataRow GetScrapMaterialById(int scrapId)
        {
            return ExecuteQuerySingleRow(
                "SELECT s.*, u.UOMName, u.Abbreviation FROM MM_ScrapMaterials s" +
                " JOIN MM_UOM u ON u.UOMID=s.UOMID WHERE s.ScrapID=?id;",
                new MySqlParameter("id", scrapId));
        }

        public static void AddScrapMaterial(string name, string description, int uomId)
        {
            string code = GenerateScrapCode();
            ExecuteNonQuery(
                "INSERT INTO MM_ScrapMaterials (ScrapCode, ScrapName, Description, UOMID, IsActive, CreatedAt)" +
                " VALUES (?code,?name,?desc,?uom,1,NOW());",
                new MySqlParameter("code", code),
                new MySqlParameter("name", name),
                new MySqlParameter("desc", description ?? (object)DBNull.Value),
                new MySqlParameter("uom",  uomId));
        }

        public static void UpdateScrapMaterial(int scrapId, string code, string name,
            string description, int uomId)
        {
            ExecuteNonQuery(
                "UPDATE MM_ScrapMaterials SET ScrapCode=?code, ScrapName=?name," +
                " Description=?desc, UOMID=?uom WHERE ScrapID=?id;",
                new MySqlParameter("code", code),
                new MySqlParameter("name", name),
                new MySqlParameter("desc", description ?? (object)DBNull.Value),
                new MySqlParameter("uom",  uomId),
                new MySqlParameter("id",   scrapId));
        }

        public static void ToggleScrapMaterialActive(int scrapId, bool isActive)
        {
            ExecuteNonQuery(
                "UPDATE MM_ScrapMaterials SET IsActive=?a WHERE ScrapID=?id;",
                new MySqlParameter("a",  isActive ? 1 : 0),
                new MySqlParameter("id", scrapId));
        }

        private static string GenerateScrapCode()
        {
            object last = ExecuteScalar(
                "SELECT ScrapCode FROM MM_ScrapMaterials ORDER BY ScrapID DESC LIMIT 1;");
            if (last == null || last == DBNull.Value) return "SC-0001";
            string prev = last.ToString();
            int num = 1;
            var m = System.Text.RegularExpressions.Regex.Match(prev, @"\d+$");
            if (m.Success) int.TryParse(m.Value, out num);
            return "SC-" + (num + 1).ToString("D4");
        }

        // ── RM SCRAP LINKS ────────────────────────────────────────────────────────

        public static DataTable GetScrapLinksForRM(int rmId)
        {
            return ExecuteQuery(
                "SELECT l.LinkID, s.ScrapID, s.ScrapCode, s.ScrapName, u.Abbreviation AS Unit" +
                " FROM MM_RMScrapLink l" +
                " JOIN MM_ScrapMaterials s ON s.ScrapID=l.ScrapID" +
                " JOIN MM_UOM u ON u.UOMID=s.UOMID" +
                " WHERE l.RMID=?rmid ORDER BY s.ScrapName;",
                new MySqlParameter("rmid", rmId));
        }

        public static void AddRMScrapLink(int rmId, int scrapId)
        {
            ExecuteNonQuery(
                "INSERT IGNORE INTO MM_RMScrapLink (RMID, ScrapID) VALUES (?rmid, ?scid);",
                new MySqlParameter("rmid", rmId),
                new MySqlParameter("scid", scrapId));
        }

        public static void DeleteRMScrapLink(int linkId)
        {
            ExecuteNonQuery(
                "DELETE FROM MM_RMScrapLink WHERE LinkID=?id;",
                new MySqlParameter("id", linkId));
        }

        // ── PACKING MATERIAL ──────────────────────────────────────────
        public static DataTable GetAllPackingMaterials()
        {
            return ExecuteQuery(
                "SELECT p.PMID, p.PMCode, p.PMName, p.Description, p.HSNCode, p.GSTRate, p.UOMID, u.UOMName, u.Abbreviation, " +
                "p.ReorderLevel, p.IsActive, p.CreatedAt " +
                "FROM MM_PackingMaterials p JOIN MM_UOM u ON u.UOMID=p.UOMID ORDER BY p.PMName;");
        }

        public static DataTable GetActivePackingMaterials()
        {
            return ExecuteQuery(
                "SELECT p.PMID, p.PMCode, p.PMName, u.Abbreviation " +
                "FROM MM_PackingMaterials p JOIN MM_UOM u ON u.UOMID=p.UOMID WHERE p.IsActive=1 ORDER BY p.PMName;");
        }

        public static DataRow GetPackingMaterialById(int pmId)
        {
            return ExecuteQuerySingleRow(
                "SELECT p.*, u.UOMName, u.Abbreviation FROM MM_PackingMaterials p " +
                "JOIN MM_UOM u ON u.UOMID=p.UOMID WHERE p.PMID=?id;",
                new MySqlParameter("id", pmId));
        }

        public static bool PMCodeExists(string code, int excludeId = 0)
        {
            var result = ExecuteScalar(
                "SELECT COUNT(*) FROM MM_PackingMaterials WHERE PMCode=?c AND PMID<>?id;",
                new MySqlParameter("c",  code),
                new MySqlParameter("id", excludeId));
            return Convert.ToInt32(result) > 0;
        }

        public static void AddPackingMaterial(string name, string description, string hsnCode, decimal? gstRate, int uomId, decimal reorderLevel)
        {
            string code = GeneratePMCode();
            ExecuteNonQuery(
                "INSERT INTO MM_PackingMaterials (PMCode, PMName, Description, HSNCode, GSTRate, UOMID, ReorderLevel, IsActive, CreatedAt) " +
                "VALUES (?code,?name,?desc,?hsn,?gst,?uom,?reorder,1,NOW());",
                new MySqlParameter("code",   code),
                new MySqlParameter("name",   name),
                new MySqlParameter("desc",   description),
                new MySqlParameter("hsn",    hsnCode ?? (object)DBNull.Value),
                new MySqlParameter("gst",    gstRate.HasValue ? (object)gstRate.Value : DBNull.Value),
                new MySqlParameter("uom",    uomId),
                new MySqlParameter("reorder",reorderLevel));
        }

        public static void UpdatePackingMaterial(int pmId, string code, string name, string description,
            string hsnCode, decimal? gstRate, int uomId, decimal reorderLevel)
        {
            ExecuteNonQuery(
                "UPDATE MM_PackingMaterials SET PMCode=?code, PMName=?name, Description=?desc, " +
                "HSNCode=?hsn, GSTRate=?gst, UOMID=?uom, ReorderLevel=?reorder WHERE PMID=?id;",
                new MySqlParameter("code",   code),
                new MySqlParameter("name",   name),
                new MySqlParameter("desc",   description),
                new MySqlParameter("hsn",    hsnCode ?? (object)DBNull.Value),
                new MySqlParameter("gst",    gstRate.HasValue ? (object)gstRate.Value : DBNull.Value),
                new MySqlParameter("uom",    uomId),
                new MySqlParameter("reorder",reorderLevel),
                new MySqlParameter("id",     pmId));
        }

        public static void TogglePackingMaterialActive(int pmId, bool isActive)
        {
            ExecuteNonQuery(
                "UPDATE MM_PackingMaterials SET IsActive=?a WHERE PMID=?id;",
                new MySqlParameter("a",  isActive ? 1 : 0),
                new MySqlParameter("id", pmId));
        }

        // ── MASTER CODE GENERATORS ───────────────────────────────────
        public static string GenerateSupplierCode()
        {
            var result = ExecuteScalar(
                "SELECT IFNULL(MAX(CAST(SUBSTRING(SupplierCode, 3) AS UNSIGNED)), 0) + 1 " +
                "FROM MM_Suppliers WHERE SupplierCode REGEXP '^S-[0-9]+$';");
            int next = Convert.ToInt32(result);
            return string.Format("S-{0:D4}", next);
        }

        public static string GenerateRMCode()
        {
            var result = ExecuteScalar(
                "SELECT IFNULL(MAX(CAST(SUBSTRING(RMCode, 3) AS UNSIGNED)), 0) + 1 " +
                "FROM MM_RawMaterials WHERE RMCode REGEXP '^R-[0-9]+$';");
            int next = Convert.ToInt32(result);
            return string.Format("R-{0:D4}", next);
        }

        public static string GeneratePMCode()
        {
            var result = ExecuteScalar(
                "SELECT IFNULL(MAX(CAST(SUBSTRING(PMCode, 3) AS UNSIGNED)), 0) + 1 " +
                "FROM MM_PackingMaterials WHERE PMCode REGEXP '^P-[0-9]+$';");
            int next = Convert.ToInt32(result);
            return string.Format("P-{0:D4}", next);
        }

        // ── GRN NUMBER GENERATOR ─────────────────────────────────────
        public static string GenerateGRNNumber(string type)
        {
            // type: "RM", "PM", "CN", "ST" -> GRN-RM-00001 (5-digit, never resets)
            // Ensure row exists for new types
            ExecuteNonQuery(
                "INSERT IGNORE INTO MM_GRNCounter (CounterType, LastValue) VALUES (?t, 0);",
                new MySqlParameter("t", type));
            ExecuteNonQuery(
                "UPDATE MM_GRNCounter SET LastValue = LastValue + 1 WHERE CounterType = ?t;",
                new MySqlParameter("t", type));
            var seq = ExecuteScalar(
                "SELECT LastValue FROM MM_GRNCounter WHERE CounterType = ?t;",
                new MySqlParameter("t", type));
            return string.Format("GRN-{0}-{1:D5}", type, Convert.ToInt32(seq));
        }

        // ── SUPPLIER RECOVERABLES ─────────────────────────────────────
        public static DataTable GetSupplierRecoverables(int supplierId)
        {
            return ExecuteQuery(
                "SELECT i.InwardID, i.GRNNo, i.InwardDate, r.RMName, r.RMCode, " +
                "u.Abbreviation, i.ShortageQty, i.ShortageValue " +
                "FROM MM_RawInward i " +
                "JOIN MM_RawMaterials r ON r.RMID = i.RMID " +
                "JOIN MM_UOM u ON u.UOMID = r.UOMID " +
                "WHERE i.SupplierID = ?s AND i.ShortageQty > 0 " +
                "ORDER BY i.InwardDate DESC;",
                new MySqlParameter("s", supplierId));
        }

        // ── RAW MATERIAL INWARD (GRN) ─────────────────────────────────
        public static DataTable GetRawInwardList(DateTime from, DateTime to)
        {
            return ExecuteQuery(
                "SELECT i.InwardID, i.GRNNo, i.InvoiceNo, i.InwardDate, i.InvoiceDate, " +
                "s.SupplierName, r.RMName, r.RMCode, u.Abbreviation, " +
                "i.Quantity, i.QtyActualReceived, i.QtyInUOM, i.Rate, i.Amount, i.GSTRate, i.GSTAmount, " +
                "i.TransportCost, i.TransportInInvoice, i.TransportInGST, " +
                "i.ShortageQty, i.ShortageValue, " +
                "i.HSNCode, i.PONo, i.Remarks, i.QualityCheck, i.QualityCheck AS Pass, i.Status, i.CreatedAt " +
                "FROM MM_RawInward i " +
                "JOIN MM_Suppliers s ON s.SupplierID=i.SupplierID " +
                "JOIN MM_RawMaterials r ON r.RMID=i.RMID " +
                "JOIN MM_UOM u ON u.UOMID=r.UOMID " +
                "WHERE i.InwardDate BETWEEN ?from AND ?to " +
                "ORDER BY i.InwardDate DESC, i.GRNNo DESC;",
                new MySqlParameter("from", from.Date),
                new MySqlParameter("to",   to.Date));
        }

        public static DataRow GetRawInwardById(int inwardId)
        {
            return ExecuteQuerySingleRow(
                "SELECT i.*, s.SupplierName, r.RMName, r.RMCode, u.Abbreviation " +
                "FROM MM_RawInward i " +
                "JOIN MM_Suppliers s ON s.SupplierID=i.SupplierID " +
                "JOIN MM_RawMaterials r ON r.RMID=i.RMID " +
                "JOIN MM_UOM u ON u.UOMID=r.UOMID " +
                "WHERE i.InwardID=?id;",
                new MySqlParameter("id", inwardId));
        }

        public static int AddRawInward(string grnNo, DateTime inwardDate, DateTime? invoiceDate,
            string invoiceNo, int supplierId, int rmId,
            decimal qtyInvoice, decimal qtyActualReceived, decimal qtyInUOM, decimal rate,
            string hsnCode, decimal? gstRate, decimal gstAmount,
            decimal transportCost, bool transportInInvoice, bool transportInGST,
            decimal totalAmount, string poNo, string remarks,
            bool qualityCheck, string status, int createdBy)
        {
            decimal shortageQty   = qtyInvoice - qtyActualReceived;
            if (shortageQty < 0) shortageQty = 0;
            decimal shortageValue = shortageQty * rate;

            ExecuteNonQuery(
                "INSERT INTO MM_RawInward (GRNNo, InwardDate, InvoiceNo, InvoiceDate, SupplierID, RMID, " +
                "Quantity, QtyActualReceived, QtyInUOM, Rate, Amount, HSNCode, GSTRate, GSTAmount, " +
                "TransportCost, TransportInInvoice, TransportInGST, ShortageQty, ShortageValue, " +
                "PONo, Remarks, QualityCheck, Status, CreatedBy, CreatedAt) " +
                "VALUES (?grn,?dt,?invno,?invdt,?sup,?rm,?qty,?actqty,?qtyuom,?rate,?amt," +
                "?hsn,?gstr,?gstamt,?trans,?transinv,?transgst,?shqty,?shval,?po,?rem,?qc,?stat,?by,NOW());",
                new MySqlParameter("grn",      grnNo),
                new MySqlParameter("dt",       inwardDate.Date),
                new MySqlParameter("invno",    invoiceNo),
                new MySqlParameter("invdt",    invoiceDate.HasValue ? (object)invoiceDate.Value.Date : DBNull.Value),
                new MySqlParameter("sup",      supplierId),
                new MySqlParameter("rm",       rmId),
                new MySqlParameter("qty",      qtyInvoice),
                new MySqlParameter("actqty",   qtyActualReceived),
                new MySqlParameter("qtyuom",   qtyInUOM),
                new MySqlParameter("rate",     rate),
                new MySqlParameter("amt",      totalAmount),
                new MySqlParameter("hsn",      hsnCode ?? (object)DBNull.Value),
                new MySqlParameter("gstr",     gstRate.HasValue ? (object)gstRate.Value : DBNull.Value),
                new MySqlParameter("gstamt",   gstAmount),
                new MySqlParameter("trans",    transportCost),
                new MySqlParameter("transinv", transportInInvoice ? 1 : 0),
                new MySqlParameter("transgst", transportInGST ? 1 : 0),
                new MySqlParameter("shqty",    shortageQty),
                new MySqlParameter("shval",    shortageValue),
                new MySqlParameter("po",       poNo),
                new MySqlParameter("rem",      remarks),
                new MySqlParameter("qc",       qualityCheck ? 1 : 0),
                new MySqlParameter("stat",     status),
                new MySqlParameter("by",       createdBy));
            return Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
        }

        public static void UpdateRawInward(int inwardId, DateTime inwardDate, DateTime? invoiceDate,
            string invoiceNo, int supplierId, int rmId,
            decimal quantity, decimal qtyInUOM, decimal rate,
            string hsnCode, decimal? gstRate, decimal gstAmount,
            decimal transportCost, bool transportInInvoice, bool transportInGST,
            decimal totalAmount, string poNo, string remarks, bool qualityCheck)
        {
            ExecuteNonQuery(
                "UPDATE MM_RawInward SET InwardDate=?dt, InvoiceNo=?invno, InvoiceDate=?invdt, " +
                "SupplierID=?sup, RMID=?rm, Quantity=?qty, QtyInUOM=?qtyuom, Rate=?rate, Amount=?amt, " +
                "HSNCode=?hsn, GSTRate=?gstr, GSTAmount=?gstamt, " +
                "TransportCost=?trans, TransportInInvoice=?transinv, TransportInGST=?transgst, " +
                "PONo=?po, Remarks=?rem, QualityCheck=?qc WHERE InwardID=?id;",
                new MySqlParameter("dt",       inwardDate.Date),
                new MySqlParameter("invno",    invoiceNo),
                new MySqlParameter("invdt",    invoiceDate.HasValue ? (object)invoiceDate.Value.Date : DBNull.Value),
                new MySqlParameter("sup",      supplierId),
                new MySqlParameter("rm",       rmId),
                new MySqlParameter("qty",      quantity),
                new MySqlParameter("qtyuom",   qtyInUOM),
                new MySqlParameter("rate",     rate),
                new MySqlParameter("amt",      totalAmount),
                new MySqlParameter("hsn",      hsnCode ?? (object)DBNull.Value),
                new MySqlParameter("gstr",     gstRate.HasValue ? (object)gstRate.Value : DBNull.Value),
                new MySqlParameter("gstamt",   gstAmount),
                new MySqlParameter("trans",    transportCost),
                new MySqlParameter("transinv", transportInInvoice ? 1 : 0),
                new MySqlParameter("transgst", transportInGST ? 1 : 0),
                new MySqlParameter("po",       poNo),
                new MySqlParameter("rem",      remarks),
                new MySqlParameter("qc",       qualityCheck ? 1 : 0),
                new MySqlParameter("id",       inwardId));
        }

        // ── PACKING MATERIAL INWARD (GRN) ─────────────────────────────
        public static DataTable GetPackingInwardList(DateTime from, DateTime to)
        {
            return ExecuteQuery(
                "SELECT i.InwardID, i.GRNNo, i.InvoiceNo, i.InwardDate, i.InvoiceDate, " +
                "s.SupplierName, p.PMName, p.PMCode, u.Abbreviation, " +
                "i.Quantity, i.QtyActualReceived, i.QtyInUOM, i.Rate, i.Amount, " +
                "i.GSTRate, i.GSTAmount, i.TransportCost, i.TransportInInvoice, i.TransportInGST, " +
                "i.HSNCode, i.ShortageQty, i.ShortageValue, i.PONo, i.Remarks, i.QualityCheck, i.Status, i.CreatedAt " +
                "FROM MM_PackingInward i " +
                "JOIN MM_Suppliers s ON s.SupplierID=i.SupplierID " +
                "JOIN MM_PackingMaterials p ON p.PMID=i.PMID " +
                "JOIN MM_UOM u ON u.UOMID=p.UOMID " +
                "WHERE i.InwardDate BETWEEN ?from AND ?to " +
                "ORDER BY i.InwardDate DESC, i.GRNNo DESC;",
                new MySqlParameter("from", from.Date),
                new MySqlParameter("to",   to.Date));
        }

        public static DataRow GetPackingInwardById(int inwardId)
        {
            return ExecuteQuerySingleRow(
                "SELECT i.*, s.SupplierName, p.PMName, p.PMCode, u.Abbreviation " +
                "FROM MM_PackingInward i " +
                "JOIN MM_Suppliers s ON s.SupplierID=i.SupplierID " +
                "JOIN MM_PackingMaterials p ON p.PMID=i.PMID " +
                "JOIN MM_UOM u ON u.UOMID=p.UOMID " +
                "WHERE i.InwardID=?id;",
                new MySqlParameter("id", inwardId));
        }

        public static DataTable GetSupplierPackingRecoverables(int supplierId)
        {
            return ExecuteQuery(
                "SELECT i.InwardID, i.GRNNo, i.InwardDate, p.PMName, p.PMCode, " +
                "u.Abbreviation, i.ShortageQty, i.ShortageValue " +
                "FROM MM_PackingInward i " +
                "JOIN MM_PackingMaterials p ON p.PMID = i.PMID " +
                "JOIN MM_UOM u ON u.UOMID = p.UOMID " +
                "WHERE i.SupplierID = ?s AND i.ShortageQty > 0 " +
                "ORDER BY i.InwardDate DESC;",
                new MySqlParameter("s", supplierId));
        }

        public static int AddPackingInward(string grnNo, DateTime inwardDate, DateTime? invoiceDate,
            string invoiceNo, int supplierId, int pmId,
            decimal qtyInvoice, decimal qtyActualReceived, decimal qtyInUOM, decimal rate,
            string hsnCode, decimal? gstRate, decimal gstAmount,
            decimal transportCost, bool transportInInvoice, bool transportInGST,
            decimal totalAmount, string poNo, string remarks,
            bool qualityCheck, string status, int createdBy)
        {
            decimal shortageQty   = qtyInvoice - qtyActualReceived;
            if (shortageQty < 0) shortageQty = 0;
            decimal shortageValue = shortageQty * rate;

            ExecuteNonQuery(
                "INSERT INTO MM_PackingInward (GRNNo, InwardDate, InvoiceNo, InvoiceDate, SupplierID, PMID, " +
                "Quantity, QtyActualReceived, QtyInUOM, Rate, Amount, HSNCode, GSTRate, GSTAmount, " +
                "TransportCost, TransportInInvoice, TransportInGST, ShortageQty, ShortageValue, " +
                "PONo, Remarks, QualityCheck, Status, CreatedBy, CreatedAt) " +
                "VALUES (?grn,?dt,?invno,?invdt,?sup,?pm,?qty,?actqty,?qtyuom,?rate,?amt," +
                "?hsn,?gstr,?gstamt,?trans,?transinv,?transgst,?shqty,?shval,?po,?rem,?qc,?stat,?by,NOW());",
                new MySqlParameter("grn",      grnNo),
                new MySqlParameter("dt",       inwardDate.Date),
                new MySqlParameter("invno",    invoiceNo),
                new MySqlParameter("invdt",    invoiceDate.HasValue ? (object)invoiceDate.Value.Date : DBNull.Value),
                new MySqlParameter("sup",      supplierId),
                new MySqlParameter("pm",       pmId),
                new MySqlParameter("qty",      qtyInvoice),
                new MySqlParameter("actqty",   qtyActualReceived),
                new MySqlParameter("qtyuom",   qtyInUOM),
                new MySqlParameter("rate",     rate),
                new MySqlParameter("amt",      totalAmount),
                new MySqlParameter("hsn",      hsnCode ?? (object)DBNull.Value),
                new MySqlParameter("gstr",     gstRate.HasValue ? (object)gstRate.Value : DBNull.Value),
                new MySqlParameter("gstamt",   gstAmount),
                new MySqlParameter("trans",    transportCost),
                new MySqlParameter("transinv", transportInInvoice ? 1 : 0),
                new MySqlParameter("transgst", transportInGST ? 1 : 0),
                new MySqlParameter("shqty",    shortageQty),
                new MySqlParameter("shval",    shortageValue),
                new MySqlParameter("po",       poNo),
                new MySqlParameter("rem",      remarks),
                new MySqlParameter("qc",       qualityCheck ? 1 : 0),
                new MySqlParameter("stat",     status),
                new MySqlParameter("by",       createdBy));
            return Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
        }

        // ─── CONSUMABLES ────────────────────────────────────────────────────────
        public static DataTable GetAllConsumables()
        {
            return ExecuteQuery(
                "SELECT c.ConsumableID, c.ConsumableCode, c.ConsumableName, c.Description, " +
                "c.HSNCode, c.GSTRate, c.ReorderLevel, c.UOMID, c.IsActive, u.Abbreviation, u.UOMName " +
                "FROM MM_Consumables c JOIN MM_UOM u ON u.UOMID=c.UOMID ORDER BY c.ConsumableName;");
        }

        public static DataTable GetActiveConsumables()
        {
            return ExecuteQuery(
                "SELECT c.ConsumableID, c.ConsumableCode, c.ConsumableName, c.Description, " +
                "c.HSNCode, c.GSTRate, c.ReorderLevel, c.UOMID, u.Abbreviation, u.UOMName " +
                "FROM MM_Consumables c JOIN MM_UOM u ON u.UOMID=c.UOMID " +
                "WHERE c.IsActive=1 ORDER BY c.ConsumableName;");
        }

        public static DataRow GetConsumableById(int consumableId)
        {
            return ExecuteQuerySingleRow(
                "SELECT c.*, u.Abbreviation, u.UOMName FROM MM_Consumables c " +
                "JOIN MM_UOM u ON u.UOMID=c.UOMID WHERE c.ConsumableID=?id;",
                new MySqlParameter("id", consumableId));
        }

        public static string GenerateConsumableCode()
        {
            object val = ExecuteScalar("SELECT MAX(CAST(SUBSTRING(ConsumableCode,3) AS UNSIGNED)) FROM MM_Consumables WHERE ConsumableCode LIKE 'C-%';");
            int next = (val == null || val == DBNull.Value) ? 1 : Convert.ToInt32(val) + 1;
            return "C-" + next.ToString("D4");
        }

        public static void AddConsumable(string name, string description, string hsnCode,
            decimal? gstRate, int uomId, decimal reorderLevel)
        {
            string code = GenerateConsumableCode();
            ExecuteNonQuery(
                "INSERT INTO MM_Consumables (ConsumableCode, ConsumableName, Description, HSNCode, GSTRate, UOMID, ReorderLevel, IsActive) " +
                "VALUES (?code,?name,?desc,?hsn,?gst,?uom,?reorder,1);",
                new MySqlParameter("code",    code),
                new MySqlParameter("name",    name),
                new MySqlParameter("desc",    description),
                new MySqlParameter("hsn",     hsnCode ?? (object)DBNull.Value),
                new MySqlParameter("gst",     gstRate.HasValue ? (object)gstRate.Value : DBNull.Value),
                new MySqlParameter("uom",     uomId),
                new MySqlParameter("reorder", reorderLevel));
        }

        public static void UpdateConsumable(int consumableId, string code, string name,
            string description, string hsnCode, decimal? gstRate, int uomId, decimal reorderLevel)
        {
            ExecuteNonQuery(
                "UPDATE MM_Consumables SET ConsumableCode=?code, ConsumableName=?name, Description=?desc, " +
                "HSNCode=?hsn, GSTRate=?gst, UOMID=?uom, ReorderLevel=?reorder WHERE ConsumableID=?id;",
                new MySqlParameter("code",    code),
                new MySqlParameter("name",    name),
                new MySqlParameter("desc",    description),
                new MySqlParameter("hsn",     hsnCode ?? (object)DBNull.Value),
                new MySqlParameter("gst",     gstRate.HasValue ? (object)gstRate.Value : DBNull.Value),
                new MySqlParameter("uom",     uomId),
                new MySqlParameter("reorder", reorderLevel),
                new MySqlParameter("id",      consumableId));
        }

        public static void ToggleConsumableActive(int consumableId, bool active)
        {
            ExecuteNonQuery("UPDATE MM_Consumables SET IsActive=?a WHERE ConsumableID=?id;",
                new MySqlParameter("a",  active ? 1 : 0),
                new MySqlParameter("id", consumableId));
        }

        // ─── STATIONARIES & OTHER ITEMS ─────────────────────────────────────────
        public static DataTable GetAllStationaries()
        {
            return ExecuteQuery(
                "SELECT s.StationaryID, s.StationaryCode, s.StationaryName, s.Description, " +
                "s.HSNCode, s.GSTRate, s.ReorderLevel, s.UOMID, s.IsActive, u.Abbreviation, u.UOMName " +
                "FROM MM_Stationaries s JOIN MM_UOM u ON u.UOMID=s.UOMID ORDER BY s.StationaryName;");
        }

        public static DataTable GetActiveStationaries()
        {
            return ExecuteQuery(
                "SELECT s.StationaryID, s.StationaryCode, s.StationaryName, s.Description, " +
                "s.HSNCode, s.GSTRate, s.ReorderLevel, s.UOMID, u.Abbreviation, u.UOMName " +
                "FROM MM_Stationaries s JOIN MM_UOM u ON u.UOMID=s.UOMID " +
                "WHERE s.IsActive=1 ORDER BY s.StationaryName;");
        }

        public static DataRow GetStationaryById(int stationaryId)
        {
            return ExecuteQuerySingleRow(
                "SELECT s.*, u.Abbreviation, u.UOMName FROM MM_Stationaries s " +
                "JOIN MM_UOM u ON u.UOMID=s.UOMID WHERE s.StationaryID=?id;",
                new MySqlParameter("id", stationaryId));
        }

        public static string GenerateStationaryCode()
        {
            object val = ExecuteScalar("SELECT MAX(CAST(SUBSTRING(StationaryCode,3) AS UNSIGNED)) FROM MM_Stationaries WHERE StationaryCode LIKE 'ST-%';");
            int next = (val == null || val == DBNull.Value) ? 1 : Convert.ToInt32(val) + 1;
            return "ST-" + next.ToString("D4");
        }

        public static void AddStationary(string name, string description, string hsnCode,
            decimal? gstRate, int uomId, decimal reorderLevel)
        {
            string code = GenerateStationaryCode();
            ExecuteNonQuery(
                "INSERT INTO MM_Stationaries (StationaryCode, StationaryName, Description, HSNCode, GSTRate, UOMID, ReorderLevel, IsActive) " +
                "VALUES (?code,?name,?desc,?hsn,?gst,?uom,?reorder,1);",
                new MySqlParameter("code",    code),
                new MySqlParameter("name",    name),
                new MySqlParameter("desc",    description),
                new MySqlParameter("hsn",     hsnCode ?? (object)DBNull.Value),
                new MySqlParameter("gst",     gstRate.HasValue ? (object)gstRate.Value : DBNull.Value),
                new MySqlParameter("uom",     uomId),
                new MySqlParameter("reorder", reorderLevel));
        }

        public static void UpdateStationary(int stationaryId, string code, string name,
            string description, string hsnCode, decimal? gstRate, int uomId, decimal reorderLevel)
        {
            ExecuteNonQuery(
                "UPDATE MM_Stationaries SET StationaryCode=?code, StationaryName=?name, Description=?desc, " +
                "HSNCode=?hsn, GSTRate=?gst, UOMID=?uom, ReorderLevel=?reorder WHERE StationaryID=?id;",
                new MySqlParameter("code",    code),
                new MySqlParameter("name",    name),
                new MySqlParameter("desc",    description),
                new MySqlParameter("hsn",     hsnCode ?? (object)DBNull.Value),
                new MySqlParameter("gst",     gstRate.HasValue ? (object)gstRate.Value : DBNull.Value),
                new MySqlParameter("uom",     uomId),
                new MySqlParameter("reorder", reorderLevel),
                new MySqlParameter("id",      stationaryId));
        }

        public static void ToggleStationaryActive(int stationaryId, bool active)
        {
            ExecuteNonQuery("UPDATE MM_Stationaries SET IsActive=?a WHERE StationaryID=?id;",
                new MySqlParameter("a",  active ? 1 : 0),
                new MySqlParameter("id", stationaryId));
        }

        // ─── CONSUMABLE INWARD (GRN) ────────────────────────────────────────────
        public static DataTable GetConsumableInwardList(DateTime from, DateTime to)
        {
            return ExecuteQuery(
                "SELECT i.InwardID, i.GRNNo, i.InvoiceNo, i.InwardDate, i.InvoiceDate, " +
                "s.SupplierName, c.ConsumableName, c.ConsumableCode, u.Abbreviation, " +
                "i.Quantity, i.QtyActualReceived, i.QtyInUOM, i.Rate, i.Amount, " +
                "i.GSTRate, i.GSTAmount, i.TransportCost, i.TransportInInvoice, i.TransportInGST, " +
                "i.HSNCode, i.ShortageQty, i.ShortageValue, i.PONo, i.Remarks, i.QualityCheck, i.Status, i.CreatedAt " +
                "FROM MM_ConsumableInward i " +
                "JOIN MM_Suppliers s ON s.SupplierID=i.SupplierID " +
                "JOIN MM_Consumables c ON c.ConsumableID=i.ConsumableID " +
                "JOIN MM_UOM u ON u.UOMID=c.UOMID " +
                "WHERE i.InwardDate BETWEEN ?from AND ?to " +
                "ORDER BY i.InwardDate DESC, i.GRNNo DESC;",
                new MySqlParameter("from", from.Date),
                new MySqlParameter("to",   to.Date));
        }

        public static DataRow GetConsumableInwardById(int inwardId)
        {
            return ExecuteQuerySingleRow(
                "SELECT i.*, s.SupplierName, c.ConsumableName, c.ConsumableCode, u.Abbreviation " +
                "FROM MM_ConsumableInward i " +
                "JOIN MM_Suppliers s ON s.SupplierID=i.SupplierID " +
                "JOIN MM_Consumables c ON c.ConsumableID=i.ConsumableID " +
                "JOIN MM_UOM u ON u.UOMID=c.UOMID " +
                "WHERE i.InwardID=?id;",
                new MySqlParameter("id", inwardId));
        }

        public static DataTable GetSupplierConsumableRecoverables(int supplierId)
        {
            return ExecuteQuery(
                "SELECT i.InwardID, i.GRNNo, i.InwardDate, c.ConsumableName, c.ConsumableCode, " +
                "u.Abbreviation, i.ShortageQty, i.ShortageValue " +
                "FROM MM_ConsumableInward i " +
                "JOIN MM_Consumables c ON c.ConsumableID = i.ConsumableID " +
                "JOIN MM_UOM u ON u.UOMID = c.UOMID " +
                "WHERE i.SupplierID = ?s AND i.ShortageQty > 0 " +
                "ORDER BY i.InwardDate DESC;",
                new MySqlParameter("s", supplierId));
        }

        public static int AddConsumableInward(string grnNo, DateTime inwardDate, DateTime? invoiceDate,
            string invoiceNo, int supplierId, int consumableId,
            decimal qtyInvoice, decimal qtyActualReceived, decimal qtyInUOM, decimal rate,
            string hsnCode, decimal? gstRate, decimal gstAmount,
            decimal transportCost, bool transportInInvoice, bool transportInGST,
            decimal totalAmount, string poNo, string remarks,
            bool qualityCheck, string status, int createdBy)
        {
            decimal shortageQty   = qtyInvoice - qtyActualReceived;
            if (shortageQty < 0) shortageQty = 0;
            decimal shortageValue = shortageQty * rate;

            ExecuteNonQuery(
                "INSERT INTO MM_ConsumableInward (GRNNo, InwardDate, InvoiceNo, InvoiceDate, SupplierID, ConsumableID, " +
                "Quantity, QtyActualReceived, QtyInUOM, Rate, Amount, HSNCode, GSTRate, GSTAmount, " +
                "TransportCost, TransportInInvoice, TransportInGST, ShortageQty, ShortageValue, " +
                "PONo, Remarks, QualityCheck, Status, CreatedBy, CreatedAt) " +
                "VALUES (?grn,?dt,?invno,?invdt,?sup,?con,?qty,?actqty,?qtyuom,?rate,?amt," +
                "?hsn,?gstr,?gstamt,?trans,?transinv,?transgst,?shqty,?shval,?po,?rem,?qc,?stat,?by,NOW());",
                new MySqlParameter("grn",      grnNo),
                new MySqlParameter("dt",       inwardDate.Date),
                new MySqlParameter("invno",    invoiceNo),
                new MySqlParameter("invdt",    invoiceDate.HasValue ? (object)invoiceDate.Value.Date : DBNull.Value),
                new MySqlParameter("sup",      supplierId),
                new MySqlParameter("con",      consumableId),
                new MySqlParameter("qty",      qtyInvoice),
                new MySqlParameter("actqty",   qtyActualReceived),
                new MySqlParameter("qtyuom",   qtyInUOM),
                new MySqlParameter("rate",     rate),
                new MySqlParameter("amt",      totalAmount),
                new MySqlParameter("hsn",      hsnCode ?? (object)DBNull.Value),
                new MySqlParameter("gstr",     gstRate.HasValue ? (object)gstRate.Value : DBNull.Value),
                new MySqlParameter("gstamt",   gstAmount),
                new MySqlParameter("trans",    transportCost),
                new MySqlParameter("transinv", transportInInvoice ? 1 : 0),
                new MySqlParameter("transgst", transportInGST ? 1 : 0),
                new MySqlParameter("shqty",    shortageQty),
                new MySqlParameter("shval",    shortageValue),
                new MySqlParameter("po",       poNo),
                new MySqlParameter("rem",      remarks),
                new MySqlParameter("qc",       qualityCheck ? 1 : 0),
                new MySqlParameter("stat",     status),
                new MySqlParameter("by",       createdBy));
            return Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
        }

        // ─── STATIONARY INWARD (GRN) ────────────────────────────────────────────
        public static DataTable GetStationaryInwardList(DateTime from, DateTime to)
        {
            return ExecuteQuery(
                "SELECT i.InwardID, i.GRNNo, i.InvoiceNo, i.InwardDate, i.InvoiceDate, " +
                "s.SupplierName, st.StationaryName, st.StationaryCode, u.Abbreviation, " +
                "i.Quantity, i.QtyActualReceived, i.QtyInUOM, i.Rate, i.Amount, " +
                "i.GSTRate, i.GSTAmount, i.TransportCost, i.TransportInInvoice, i.TransportInGST, " +
                "i.HSNCode, i.ShortageQty, i.ShortageValue, i.PONo, i.Remarks, i.QualityCheck, i.Status, i.CreatedAt " +
                "FROM MM_StationaryInward i " +
                "JOIN MM_Suppliers s ON s.SupplierID=i.SupplierID " +
                "JOIN MM_Stationaries st ON st.StationaryID=i.StationaryID " +
                "JOIN MM_UOM u ON u.UOMID=st.UOMID " +
                "WHERE i.InwardDate BETWEEN ?from AND ?to " +
                "ORDER BY i.InwardDate DESC, i.GRNNo DESC;",
                new MySqlParameter("from", from.Date),
                new MySqlParameter("to",   to.Date));
        }

        public static DataRow GetStationaryInwardById(int inwardId)
        {
            return ExecuteQuerySingleRow(
                "SELECT i.*, s.SupplierName, st.StationaryName, st.StationaryCode, u.Abbreviation " +
                "FROM MM_StationaryInward i " +
                "JOIN MM_Suppliers s ON s.SupplierID=i.SupplierID " +
                "JOIN MM_Stationaries st ON st.StationaryID=i.StationaryID " +
                "JOIN MM_UOM u ON u.UOMID=st.UOMID " +
                "WHERE i.InwardID=?id;",
                new MySqlParameter("id", inwardId));
        }

        public static DataTable GetSupplierStationaryRecoverables(int supplierId)
        {
            return ExecuteQuery(
                "SELECT i.InwardID, i.GRNNo, i.InwardDate, st.StationaryName, st.StationaryCode, " +
                "u.Abbreviation, i.ShortageQty, i.ShortageValue " +
                "FROM MM_StationaryInward i " +
                "JOIN MM_Stationaries st ON st.StationaryID = i.StationaryID " +
                "JOIN MM_UOM u ON u.UOMID = st.UOMID " +
                "WHERE i.SupplierID = ?s AND i.ShortageQty > 0 " +
                "ORDER BY i.InwardDate DESC;",
                new MySqlParameter("s", supplierId));
        }

        public static int AddStationaryInward(string grnNo, DateTime inwardDate, DateTime? invoiceDate,
            string invoiceNo, int supplierId, int stationaryId,
            decimal qtyInvoice, decimal qtyActualReceived, decimal qtyInUOM, decimal rate,
            string hsnCode, decimal? gstRate, decimal gstAmount,
            decimal transportCost, bool transportInInvoice, bool transportInGST,
            decimal totalAmount, string poNo, string remarks,
            bool qualityCheck, string status, int createdBy)
        {
            decimal shortageQty   = qtyInvoice - qtyActualReceived;
            if (shortageQty < 0) shortageQty = 0;
            decimal shortageValue = shortageQty * rate;

            ExecuteNonQuery(
                "INSERT INTO MM_StationaryInward (GRNNo, InwardDate, InvoiceNo, InvoiceDate, SupplierID, StationaryID, " +
                "Quantity, QtyActualReceived, QtyInUOM, Rate, Amount, HSNCode, GSTRate, GSTAmount, " +
                "TransportCost, TransportInInvoice, TransportInGST, ShortageQty, ShortageValue, " +
                "PONo, Remarks, QualityCheck, Status, CreatedBy, CreatedAt) " +
                "VALUES (?grn,?dt,?invno,?invdt,?sup,?st,?qty,?actqty,?qtyuom,?rate,?amt," +
                "?hsn,?gstr,?gstamt,?trans,?transinv,?transgst,?shqty,?shval,?po,?rem,?qc,?stat,?by,NOW());",
                new MySqlParameter("grn",      grnNo),
                new MySqlParameter("dt",       inwardDate.Date),
                new MySqlParameter("invno",    invoiceNo),
                new MySqlParameter("invdt",    invoiceDate.HasValue ? (object)invoiceDate.Value.Date : DBNull.Value),
                new MySqlParameter("sup",      supplierId),
                new MySqlParameter("st",       stationaryId),
                new MySqlParameter("qty",      qtyInvoice),
                new MySqlParameter("actqty",   qtyActualReceived),
                new MySqlParameter("qtyuom",   qtyInUOM),
                new MySqlParameter("rate",     rate),
                new MySqlParameter("amt",      totalAmount),
                new MySqlParameter("hsn",      hsnCode ?? (object)DBNull.Value),
                new MySqlParameter("gstr",     gstRate.HasValue ? (object)gstRate.Value : DBNull.Value),
                new MySqlParameter("gstamt",   gstAmount),
                new MySqlParameter("trans",    transportCost),
                new MySqlParameter("transinv", transportInInvoice ? 1 : 0),
                new MySqlParameter("transgst", transportInGST ? 1 : 0),
                new MySqlParameter("shqty",    shortageQty),
                new MySqlParameter("shval",    shortageValue),
                new MySqlParameter("po",       poNo),
                new MySqlParameter("rem",      remarks),
                new MySqlParameter("qc",       qualityCheck ? 1 : 0),
                new MySqlParameter("stat",     status),
                new MySqlParameter("by",       createdBy));
            return Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
        }

        // ─── OPENING STOCK ──────────────────────────────────────────────────────
        public static DataRow GetOpeningStock(string materialType, int materialId)
        {
            return ExecuteQuerySingleRow(
                "SELECT OpeningStockID, Quantity, Rate, Value, AsOfDate, Remarks " +
                "FROM MM_OpeningStock WHERE MaterialType=?t AND MaterialID=?id;",
                new MySqlParameter("t",   materialType),
                new MySqlParameter("id",  materialId));
        }

        public static void SaveOpeningStock(string materialType, int materialId,
            decimal quantity, decimal rate, DateTime asOfDate, string remarks, int userId)
        {
            ExecuteNonQuery(
                "INSERT INTO MM_OpeningStock " +
                "(MaterialType, MaterialID, Quantity, Rate, AsOfDate, Remarks, CreatedBy) " +
                "VALUES (?t, ?id, ?qty, ?rate, ?dt, ?rem, ?by) " +
                "ON DUPLICATE KEY UPDATE " +
                "Quantity=?qty, Rate=?rate, AsOfDate=?dt, Remarks=?rem, UpdatedAt=NOW();",
                new MySqlParameter("t",    materialType),
                new MySqlParameter("id",   materialId),
                new MySqlParameter("qty",  quantity),
                new MySqlParameter("rate", rate),
                new MySqlParameter("dt",   asOfDate),
                new MySqlParameter("rem",  string.IsNullOrEmpty(remarks) ? (object)DBNull.Value : remarks),
                new MySqlParameter("by",   userId));
        }

        public static DataTable GetAllOpeningStock(string materialType)
        {
            string sql =
                "SELECT os.OpeningStockID, os.MaterialID, os.Quantity, os.Rate, os.Value, " +
                "os.AsOfDate, os.Remarks, " +
                "CASE os.MaterialType " +
                "  WHEN 'RM' THEN r.RMName   WHEN 'PM' THEN p.PMName " +
                "  WHEN 'CN' THEN c.ConsumableName WHEN 'ST' THEN s.StationaryName END AS MaterialName, " +
                "CASE os.MaterialType " +
                "  WHEN 'RM' THEN r.RMCode   WHEN 'PM' THEN p.PMCode " +
                "  WHEN 'CN' THEN c.ConsumableCode  WHEN 'ST' THEN s.StationaryCode END AS MaterialCode, " +
                "CASE os.MaterialType " +
                "  WHEN 'RM' THEN ur.Abbreviation WHEN 'PM' THEN up.Abbreviation " +
                "  WHEN 'CN' THEN uc.Abbreviation WHEN 'ST' THEN us.Abbreviation END AS UOMAbbrv " +
                "FROM MM_OpeningStock os " +
                "LEFT JOIN MM_RawMaterials    r  ON r.RMID=os.MaterialID AND os.MaterialType='RM' " +
                "LEFT JOIN MM_UOM             ur ON ur.UOMID=r.UOMID " +
                "LEFT JOIN MM_PackingMaterials p  ON p.PMID=os.MaterialID AND os.MaterialType='PM' " +
                "LEFT JOIN MM_UOM             up ON up.UOMID=p.UOMID " +
                "LEFT JOIN MM_Consumables      c  ON c.ConsumableID=os.MaterialID AND os.MaterialType='CN' " +
                "LEFT JOIN MM_UOM             uc ON uc.UOMID=c.UOMID " +
                "LEFT JOIN MM_Stationaries     s  ON s.StationaryID=os.MaterialID AND os.MaterialType='ST' " +
                "LEFT JOIN MM_UOM             us ON us.UOMID=s.UOMID " +
                "WHERE os.MaterialType=?t " +
                "ORDER BY MaterialName;";
            return ExecuteQuery(sql, new MySqlParameter("t", materialType));
        }
    }
}
