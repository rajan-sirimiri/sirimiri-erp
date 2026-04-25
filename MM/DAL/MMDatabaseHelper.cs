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

        /// <summary>Open a connection and force MySQL session time zone to IST
        /// so NOW() / CURRENT_TIMESTAMP return Indian time regardless of VPS clock.</summary>
        internal static MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            using (var tz = new MySqlCommand("SET time_zone='+05:30';", conn))
                tz.ExecuteNonQuery();
            return conn;
        }

        private static DataTable ExecuteQuery(string sql, params MySqlParameter[] parms)
        {
            using (var conn = OpenConnection())
            using (var cmd = new MySqlCommand(sql, conn))
            {
                if (parms != null) cmd.Parameters.AddRange(parms);
                var dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);
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

        private static DataRow ExecuteQuerySingleRow(string sql, params MySqlParameter[] parms)
        {
            var dt = ExecuteQuery(sql, parms);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public static DataRow ExecuteQuerySingleRowPublic(string sql, params MySqlParameter[] parms)
        {
            return ExecuteQuerySingleRow(sql, parms);
        }

        public static DataTable ExecuteQueryPublic(string sql, params MySqlParameter[] parms)
        {
            return ExecuteQuery(sql, parms);
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
                "FROM MM_Suppliers " +
                "WHERE PartyType='SUPPLIER' " +
                "ORDER BY SupplierName;");
        }

        public static DataTable GetActiveSuppliers()
        {
            return ExecuteQuery(
                "SELECT SupplierID, SupplierCode, SupplierName FROM MM_Suppliers " +
                "WHERE IsActive=1 AND PartyType='SUPPLIER' ORDER BY SupplierName;");
        }

        /// Get suppliers sorted: those who supplied this material first, then the rest.
        /// materialType: 'RM' for MM_RawInward, 'PM' for MM_PackingInward, etc.
        public static DataTable GetSuppliersSortedByMaterial(string materialType, int materialId)
        {
            string inwardTable = "MM_RawInward";
            string matColumn = "RMID";
            if (materialType == "PM") { inwardTable = "MM_PackingInward"; matColumn = "PMID"; }
            else if (materialType == "CM") { inwardTable = "MM_ConsumableInward"; matColumn = "ItemID"; }
            else if (materialType == "ST") { inwardTable = "MM_StationaryInward"; matColumn = "ItemID"; }

            return ExecuteQuery(
                "SELECT s.SupplierID, s.SupplierCode, s.SupplierName," +
                " IFNULL(hist.PurchaseCount, 0) AS PurchaseCount" +
                " FROM MM_Suppliers s" +
                " LEFT JOIN (SELECT SupplierID, COUNT(*) AS PurchaseCount" +
                "   FROM " + inwardTable +
                "   WHERE " + matColumn + "=?mid" +
                "   GROUP BY SupplierID) hist ON hist.SupplierID = s.SupplierID" +
                " WHERE s.IsActive=1 AND s.PartyType='SUPPLIER'" +
                " ORDER BY IFNULL(hist.PurchaseCount,0) DESC, s.SupplierName;",
                new MySqlParameter("?mid", materialId));
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
                "GSTNo, PAN, Address, City, State, PinCode, IsActive, CreatedAt, PartyType) " +
                "VALUES (?code,?name,?cp,?ph,?em,?gst,?pan,?addr,?city,?state,?pin,1,NOW(),'SUPPLIER');",
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
                " IFNULL(latest.LatestRate, IFNULL(os.Rate, 0)) AS LatestCostPerUnit," +
                " IFNULL(avg30.AvgRate, IFNULL(latest.LatestRate, IFNULL(os.Rate, 0))) AS Avg30DayCost," +
                " NULL AS ReconStatus," +
                " NULL AS ReconDate" +
                " FROM MM_RawMaterials r" +
                " JOIN MM_UOM u ON u.UOMID = r.UOMID" +
                " LEFT JOIN MM_OpeningStock os ON os.MaterialType='RM' AND os.MaterialID=r.RMID" +
                " LEFT JOIN (SELECT RMID, SUM(QtyActualReceived) AS TotalReceived" +
                "            FROM MM_RawInward GROUP BY RMID) grn ON grn.RMID = r.RMID" +
                " LEFT JOIN (SELECT RMID, SUM(QtyConsumed) AS TotalConsumed" +
                "            FROM MM_StockConsumption GROUP BY RMID) con ON con.RMID = r.RMID" +
                " LEFT JOIN (SELECT RMID, Rate AS LatestRate FROM MM_RawInward" +
                "            WHERE QtyActualReceived > 0 AND Rate > 0" +
                "            AND InwardID = (SELECT MAX(InwardID) FROM MM_RawInward i2" +
                "                            WHERE i2.RMID = MM_RawInward.RMID AND i2.Rate > 0)" +
                "           ) latest ON latest.RMID = r.RMID" +
                " LEFT JOIN (SELECT RMID," +
                "            SUM(QtyActualReceived * Rate) / NULLIF(SUM(QtyActualReceived), 0) AS AvgRate" +
                "            FROM MM_RawInward WHERE QtyActualReceived > 0 AND Rate > 0" +
                "            AND InwardDate >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)" +
                "            GROUP BY RMID) avg30 ON avg30.RMID = r.RMID" +
                " WHERE r.IsActive = 1 AND LOWER(TRIM(r.RMName)) != 'ro water'" +
                " ORDER BY CurrentStock DESC, r.RMName ASC;");
        }

        public static DataTable GetPMStockReport()
        {
            // Stock model with location split:
            //   StoresQty   = Opening + GRN_Received - PK_Consumption - StoresOut + StoresIn
            //   FloorQty    = FloorIn - FloorOut - FloorConsumed_at_issue
            //   CurrentStock = StoresQty + FloorQty (kept as same column name for backward compat)
            return ExecuteQuery(
                "SELECT p.PMID, p.PMCode, p.PMName, u.Abbreviation AS UOM," +
                " ROUND(" +
                "   IFNULL(os.Quantity, 0)" +
                "   + IFNULL(grn.TotalReceived, 0)" +
                "   - IFNULL(con.TotalConsumed, 0)" +
                "   - IFNULL(trf.StoresOut, 0)" +
                "   + IFNULL(trf.StoresIn, 0)" +
                " , 4) AS StoresQty," +
                " ROUND(" +
                "   IFNULL(trf.FloorIn, 0)" +
                "   - IFNULL(trf.FloorOut, 0)" +
                "   - IFNULL(trf.FloorConsumed, 0)" +
                " , 4) AS FloorQty," +
                " ROUND(" +
                "   IFNULL(os.Quantity, 0)" +
                "   + IFNULL(grn.TotalReceived, 0)" +
                "   - IFNULL(con.TotalConsumed, 0)" +
                "   - IFNULL(trf.FloorConsumed, 0)" +
                " , 4) AS CurrentStock," +
                " IFNULL(os.Quantity, 0) AS OpeningStock," +
                " IFNULL(grn.TotalReceived, 0) AS TotalReceived," +
                " IFNULL(con.TotalConsumed, 0) AS TotalConsumed," +
                " p.ReorderLevel," +
                " IFNULL(latest.LatestRate, IFNULL(os.Rate, 0)) AS LatestCostPerUnit," +
                " IFNULL(avg30.AvgRate, IFNULL(latest.LatestRate, IFNULL(os.Rate, 0))) AS Avg30DayCost" +
                " FROM MM_PackingMaterials p" +
                " JOIN MM_UOM u ON u.UOMID = p.UOMID" +
                " LEFT JOIN MM_OpeningStock os ON os.MaterialType='PM' AND os.MaterialID=p.PMID" +
                " LEFT JOIN (SELECT PMID, SUM(QtyActualReceived) AS TotalReceived" +
                "            FROM MM_PackingInward GROUP BY PMID) grn ON grn.PMID = p.PMID" +
                " LEFT JOIN (SELECT PMID, SUM(QtyUsed) AS TotalConsumed" +
                "            FROM PK_PMConsumption GROUP BY PMID) con ON con.PMID = p.PMID" +
                " LEFT JOIN (" +
                "   SELECT l.MaterialID," +
                "          SUM(CASE WHEN t.FromLocationID=loc_st.LocationID AND t.Status='Issued' THEN l.Quantity ELSE 0 END) AS StoresOut," +
                "          SUM(CASE WHEN t.ToLocationID  =loc_st.LocationID AND t.Status='Issued' THEN l.Quantity ELSE 0 END) AS StoresIn," +
                "          SUM(CASE WHEN t.ToLocationID  =loc_fl.LocationID AND t.Status='Issued' THEN l.Quantity ELSE 0 END) AS FloorIn," +
                "          SUM(CASE WHEN t.FromLocationID=loc_fl.LocationID AND t.Status='Issued' THEN l.Quantity ELSE 0 END) AS FloorOut," +
                "          SUM(CASE WHEN t.ToLocationID  =loc_fl.LocationID AND t.Status='Issued' AND l.ConsumedAtIssue=1 THEN l.Quantity ELSE 0 END) AS FloorConsumed" +
                "   FROM MM_StockTransferLine l" +
                "   JOIN MM_StockTransfer t ON t.TransferID = l.TransferID" +
                "   CROSS JOIN (SELECT LocationID FROM MM_Locations WHERE LocationCode='STORES') loc_st" +
                "   CROSS JOIN (SELECT LocationID FROM MM_Locations WHERE LocationCode='FLOOR')  loc_fl" +
                "   WHERE l.MaterialType='PM'" +
                "   GROUP BY l.MaterialID" +
                " ) trf ON trf.MaterialID = p.PMID" +
                " LEFT JOIN (SELECT PMID, Rate AS LatestRate FROM MM_PackingInward" +
                "            WHERE QtyActualReceived > 0 AND Rate > 0" +
                "            AND InwardID = (SELECT MAX(InwardID) FROM MM_PackingInward i2" +
                "                            WHERE i2.PMID = MM_PackingInward.PMID AND i2.Rate > 0)" +
                "           ) latest ON latest.PMID = p.PMID" +
                " LEFT JOIN (SELECT PMID," +
                "            SUM(QtyActualReceived * Rate) / NULLIF(SUM(QtyActualReceived), 0) AS AvgRate" +
                "            FROM MM_PackingInward WHERE QtyActualReceived > 0 AND Rate > 0" +
                "            AND InwardDate >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)" +
                "            GROUP BY PMID) avg30 ON avg30.PMID = p.PMID" +
                " WHERE p.IsActive = 1" +
                " ORDER BY CurrentStock DESC, p.PMName ASC;");
        }

        public static DataTable GetCNStockReport()
        {
            // Stock model with location split (no PK consumption tracker for CN):
            //   StoresQty   = Opening + GRN_Received - StoresOut + StoresIn
            //   FloorQty    = FloorIn - FloorOut - FloorConsumed_at_issue
            //   CurrentStock = StoresQty + FloorQty
            return ExecuteQuery(
                "SELECT c.ConsumableID, c.ConsumableCode, c.ConsumableName, u.Abbreviation AS UOM," +
                " ROUND(" +
                "   IFNULL(os.Quantity, 0)" +
                "   + IFNULL(grn.TotalReceived, 0)" +
                "   - IFNULL(trf.StoresOut, 0)" +
                "   + IFNULL(trf.StoresIn, 0)" +
                " , 4) AS StoresQty," +
                " ROUND(" +
                "   IFNULL(trf.FloorIn, 0)" +
                "   - IFNULL(trf.FloorOut, 0)" +
                "   - IFNULL(trf.FloorConsumed, 0)" +
                " , 4) AS FloorQty," +
                " ROUND(" +
                "   IFNULL(os.Quantity, 0)" +
                "   + IFNULL(grn.TotalReceived, 0)" +
                "   - IFNULL(trf.FloorConsumed, 0)" +
                " , 4) AS CurrentStock," +
                " c.ReorderLevel," +
                " IFNULL(latest.LatestRate, IFNULL(os.Rate, 0)) AS LatestCostPerUnit," +
                " IFNULL(avg30.AvgRate, IFNULL(latest.LatestRate, IFNULL(os.Rate, 0))) AS Avg30DayCost" +
                " FROM MM_Consumables c" +
                " JOIN MM_UOM u ON u.UOMID = c.UOMID" +
                " LEFT JOIN MM_OpeningStock os ON os.MaterialType='CN' AND os.MaterialID=c.ConsumableID" +
                " LEFT JOIN (SELECT ConsumableID, SUM(QtyActualReceived) AS TotalReceived" +
                "            FROM MM_ConsumableInward GROUP BY ConsumableID) grn ON grn.ConsumableID = c.ConsumableID" +
                " LEFT JOIN (" +
                "   SELECT l.MaterialID," +
                "          SUM(CASE WHEN t.FromLocationID=loc_st.LocationID AND t.Status='Issued' THEN l.Quantity ELSE 0 END) AS StoresOut," +
                "          SUM(CASE WHEN t.ToLocationID  =loc_st.LocationID AND t.Status='Issued' THEN l.Quantity ELSE 0 END) AS StoresIn," +
                "          SUM(CASE WHEN t.ToLocationID  =loc_fl.LocationID AND t.Status='Issued' THEN l.Quantity ELSE 0 END) AS FloorIn," +
                "          SUM(CASE WHEN t.FromLocationID=loc_fl.LocationID AND t.Status='Issued' THEN l.Quantity ELSE 0 END) AS FloorOut," +
                "          SUM(CASE WHEN t.ToLocationID  =loc_fl.LocationID AND t.Status='Issued' AND l.ConsumedAtIssue=1 THEN l.Quantity ELSE 0 END) AS FloorConsumed" +
                "   FROM MM_StockTransferLine l" +
                "   JOIN MM_StockTransfer t ON t.TransferID = l.TransferID" +
                "   CROSS JOIN (SELECT LocationID FROM MM_Locations WHERE LocationCode='STORES') loc_st" +
                "   CROSS JOIN (SELECT LocationID FROM MM_Locations WHERE LocationCode='FLOOR')  loc_fl" +
                "   WHERE l.MaterialType='CN'" +
                "   GROUP BY l.MaterialID" +
                " ) trf ON trf.MaterialID = c.ConsumableID" +
                " LEFT JOIN (SELECT ConsumableID, Rate AS LatestRate FROM MM_ConsumableInward" +
                "            WHERE QtyActualReceived > 0 AND Rate > 0" +
                "            AND InwardID = (SELECT MAX(InwardID) FROM MM_ConsumableInward i2" +
                "                            WHERE i2.ConsumableID = MM_ConsumableInward.ConsumableID AND i2.Rate > 0)" +
                "           ) latest ON latest.ConsumableID = c.ConsumableID" +
                " LEFT JOIN (SELECT ConsumableID," +
                "            SUM(QtyActualReceived * Rate) / NULLIF(SUM(QtyActualReceived), 0) AS AvgRate" +
                "            FROM MM_ConsumableInward WHERE QtyActualReceived > 0 AND Rate > 0" +
                "            AND InwardDate >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)" +
                "            GROUP BY ConsumableID) avg30 ON avg30.ConsumableID = c.ConsumableID" +
                " WHERE c.IsActive = 1" +
                " ORDER BY CurrentStock DESC, c.ConsumableName ASC;");
        }

        public static DataTable GetSTStockReport()
        {
            // Stock model with location split (no PK consumption tracker for ST):
            //   StoresQty   = Opening + GRN_Received - StoresOut + StoresIn
            //   FloorQty    = FloorIn - FloorOut - FloorConsumed_at_issue
            //   CurrentStock = StoresQty + FloorQty
            return ExecuteQuery(
                "SELECT s.StationaryID, s.StationaryCode, s.StationaryName, u.Abbreviation AS UOM," +
                " ROUND(" +
                "   IFNULL(os.Quantity, 0)" +
                "   + IFNULL(grn.TotalReceived, 0)" +
                "   - IFNULL(trf.StoresOut, 0)" +
                "   + IFNULL(trf.StoresIn, 0)" +
                " , 4) AS StoresQty," +
                " ROUND(" +
                "   IFNULL(trf.FloorIn, 0)" +
                "   - IFNULL(trf.FloorOut, 0)" +
                "   - IFNULL(trf.FloorConsumed, 0)" +
                " , 4) AS FloorQty," +
                " ROUND(" +
                "   IFNULL(os.Quantity, 0)" +
                "   + IFNULL(grn.TotalReceived, 0)" +
                "   - IFNULL(trf.FloorConsumed, 0)" +
                " , 4) AS CurrentStock," +
                " s.ReorderLevel," +
                " IFNULL(latest.LatestRate, IFNULL(os.Rate, 0)) AS LatestCostPerUnit," +
                " IFNULL(avg30.AvgRate, IFNULL(latest.LatestRate, IFNULL(os.Rate, 0))) AS Avg30DayCost" +
                " FROM MM_Stationaries s" +
                " JOIN MM_UOM u ON u.UOMID = s.UOMID" +
                " LEFT JOIN MM_OpeningStock os ON os.MaterialType='ST' AND os.MaterialID=s.StationaryID" +
                " LEFT JOIN (SELECT StationaryID, SUM(QtyActualReceived) AS TotalReceived" +
                "            FROM MM_StationaryInward GROUP BY StationaryID) grn ON grn.StationaryID = s.StationaryID" +
                " LEFT JOIN (" +
                "   SELECT l.MaterialID," +
                "          SUM(CASE WHEN t.FromLocationID=loc_st.LocationID AND t.Status='Issued' THEN l.Quantity ELSE 0 END) AS StoresOut," +
                "          SUM(CASE WHEN t.ToLocationID  =loc_st.LocationID AND t.Status='Issued' THEN l.Quantity ELSE 0 END) AS StoresIn," +
                "          SUM(CASE WHEN t.ToLocationID  =loc_fl.LocationID AND t.Status='Issued' THEN l.Quantity ELSE 0 END) AS FloorIn," +
                "          SUM(CASE WHEN t.FromLocationID=loc_fl.LocationID AND t.Status='Issued' THEN l.Quantity ELSE 0 END) AS FloorOut," +
                "          SUM(CASE WHEN t.ToLocationID  =loc_fl.LocationID AND t.Status='Issued' AND l.ConsumedAtIssue=1 THEN l.Quantity ELSE 0 END) AS FloorConsumed" +
                "   FROM MM_StockTransferLine l" +
                "   JOIN MM_StockTransfer t ON t.TransferID = l.TransferID" +
                "   CROSS JOIN (SELECT LocationID FROM MM_Locations WHERE LocationCode='STORES') loc_st" +
                "   CROSS JOIN (SELECT LocationID FROM MM_Locations WHERE LocationCode='FLOOR')  loc_fl" +
                "   WHERE l.MaterialType='ST'" +
                "   GROUP BY l.MaterialID" +
                " ) trf ON trf.MaterialID = s.StationaryID" +
                " LEFT JOIN (SELECT StationaryID, Rate AS LatestRate FROM MM_StationaryInward" +
                "            WHERE QtyActualReceived > 0 AND Rate > 0" +
                "            AND InwardID = (SELECT MAX(InwardID) FROM MM_StationaryInward i2" +
                "                            WHERE i2.StationaryID = MM_StationaryInward.StationaryID AND i2.Rate > 0)" +
                "           ) latest ON latest.StationaryID = s.StationaryID" +
                " LEFT JOIN (SELECT StationaryID," +
                "            SUM(QtyActualReceived * Rate) / NULLIF(SUM(QtyActualReceived), 0) AS AvgRate" +
                "            FROM MM_StationaryInward WHERE QtyActualReceived > 0 AND Rate > 0" +
                "            AND InwardDate >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)" +
                "            GROUP BY StationaryID) avg30 ON avg30.StationaryID = s.StationaryID" +
                " WHERE s.IsActive = 1" +
                " ORDER BY CurrentStock DESC, s.StationaryName ASC;");
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

        // ── CONVERSION LOSS PRICING ─────────────────────────────────────────

        public static void SaveConversionLossSettings(int rmId, int? derivedFromRMID, decimal? conversionLossPct)
        {
            ExecuteNonQuery(
                "UPDATE MM_RawMaterials SET DerivedFromRMID=?dfr, ConversionLossPct=?clp WHERE RMID=?id;",
                new MySqlParameter("?dfr", derivedFromRMID.HasValue ? (object)derivedFromRMID.Value : DBNull.Value),
                new MySqlParameter("?clp", conversionLossPct.HasValue ? (object)conversionLossPct.Value : DBNull.Value),
                new MySqlParameter("?id",  rmId));
        }

        // Overload: save with non-nullable params (used from source RM perspective)
        public static void SaveConversionLossSettings(int derivedRmId, int sourceRmId, decimal lossPct)
        {
            ExecuteNonQuery(
                "UPDATE MM_RawMaterials SET DerivedFromRMID=?src, ConversionLossPct=?pct WHERE RMID=?id;",
                new MySqlParameter("?src", sourceRmId),
                new MySqlParameter("?pct", lossPct),
                new MySqlParameter("?id",  derivedRmId));
        }

        /// <summary>
        /// Find the derived RM that gets its price from this source RM
        /// </summary>
        public static DataRow GetDerivedRMForSource(int sourceRmId)
        {
            return ExecuteQuerySingleRow(
                "SELECT RMID, RMName, ConversionLossPct FROM MM_RawMaterials" +
                " WHERE DerivedFromRMID=?sid AND IsActive=1 LIMIT 1;",
                new MySqlParameter("?sid", sourceRmId));
        }

        /// <summary>
        /// Clear conversion loss settings for any RM that derives from this source
        /// </summary>
        public static void ClearConversionLossForSource(int sourceRmId)
        {
            ExecuteNonQuery(
                "UPDATE MM_RawMaterials SET DerivedFromRMID=NULL, ConversionLossPct=NULL" +
                " WHERE DerivedFromRMID=?sid;",
                new MySqlParameter("?sid", sourceRmId));
        }

        /// <summary>
        /// When a source RM's price changes (via GRN or Opening Stock),
        /// find all RMs that derive from it and update their latest GRN rate.
        /// DerivedRM price = sourcePrice × (1 + ConversionLossPct/100)
        /// </summary>
        public static void UpdateDerivedRMPrices(int sourceRmId, decimal sourceRate)
        {
            if (sourceRate <= 0) return;

            // Find all RMs that derive from this source RM
            var derivedRMs = ExecuteQuery(
                "SELECT RMID, ConversionLossPct FROM MM_RawMaterials" +
                " WHERE DerivedFromRMID=?sid AND ConversionLossPct IS NOT NULL AND IsActive=1;",
                new MySqlParameter("?sid", sourceRmId));

            foreach (DataRow r in derivedRMs.Rows)
            {
                int derivedRmId = Convert.ToInt32(r["RMID"]);
                decimal lossPct = Convert.ToDecimal(r["ConversionLossPct"]);
                decimal derivedRate = Math.Round(sourceRate * (1 + lossPct / 100m), 2);

                // Update or insert opening stock with the calculated rate
                // This ensures the derived RM always has a current price
                ExecuteNonQuery(
                    "INSERT INTO MM_OpeningStock (MaterialType, MaterialID, Quantity, Rate, AsOfDate, Remarks, CreatedBy)" +
                    " VALUES ('RM', ?rmid, 0, ?rate, CURDATE(), ?rem, 1)" +
                    " ON DUPLICATE KEY UPDATE Rate=?rate, Remarks=?rem, UpdatedAt=NOW();",
                    new MySqlParameter("?rmid", derivedRmId),
                    new MySqlParameter("?rate", derivedRate),
                    new MySqlParameter("?rem",  "Auto-priced from source RM#" + sourceRmId +
                        " @ ₹" + sourceRate.ToString("0.00") + " + " + lossPct.ToString("0.##") + "% loss = ₹" + derivedRate.ToString("0.00")));
            }
        }

        // ── SCRAP MATERIALS ──────────────────────────────────────────────────────

        public static DataTable GetAllScrapMaterials()
        {
            return ExecuteQuery(
                "SELECT s.ScrapID, s.ScrapCode, s.ScrapName, s.Description," +
                " s.UOMID, s.CurrentPrice, u.UOMName, u.Abbreviation, s.IsActive, s.CreatedAt" +
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

        // ── SCRAP PRICING ────────────────────────────────────────────────

        public static void SetScrapPrice(int scrapId, decimal price, int userId)
        {
            // Update current price
            ExecuteNonQuery("UPDATE MM_ScrapMaterials SET CurrentPrice=?p WHERE ScrapID=?id;",
                new MySqlParameter("?p", price), new MySqlParameter("?id", scrapId));
            // Log to history
            ExecuteNonQuery(
                "INSERT INTO MM_ScrapPriceHistory (ScrapID, Price, EffectiveDate, CreatedBy)" +
                " VALUES (?id, ?p, CURDATE(), ?by);",
                new MySqlParameter("?id", scrapId), new MySqlParameter("?p", price),
                new MySqlParameter("?by", userId));
        }

        public static DataTable GetScrapPriceHistory(int scrapId)
        {
            return ExecuteQuery(
                "SELECT HistoryID, Price, EffectiveDate, Remarks, CreatedAt" +
                " FROM MM_ScrapPriceHistory WHERE ScrapID=?id ORDER BY EffectiveDate DESC, HistoryID DESC;",
                new MySqlParameter("?id", scrapId));
        }

        public static decimal GetScrapCurrentPrice(int scrapId)
        {
            object val = ExecuteScalar("SELECT CurrentPrice FROM MM_ScrapMaterials WHERE ScrapID=?id;",
                new MySqlParameter("?id", scrapId));
            return val != null && val != DBNull.Value ? Convert.ToDecimal(val) : 0;
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

        // Scrap Material Stock Report
        // StockQty = SUM of GRN receipts with InvoiceNo='SCRAP'
        // LinkedRMs = comma-separated list of RMs that produce this scrap
        public static DataTable GetScrapStockReport()
        {
            return ExecuteQuery(
                "SELECT s.ScrapID, s.ScrapCode, s.ScrapName, u.Abbreviation AS UOM," +
                " ROUND(IFNULL(st.TotalGenerated, 0), 4) AS StockQty," +
                " IFNULL(rms.LinkedRMs, '—') AS LinkedRMs" +
                " FROM MM_ScrapMaterials s" +
                " JOIN MM_UOM u ON u.UOMID = s.UOMID" +
                " LEFT JOIN (" +
                "   SELECT ScrapID, SUM(QtyGenerated) AS TotalGenerated" +
                "   FROM MM_ScrapStock GROUP BY ScrapID" +
                " ) st ON st.ScrapID = s.ScrapID" +
                " LEFT JOIN (" +
                "   SELECT l.ScrapID, GROUP_CONCAT(r.RMName ORDER BY r.RMName SEPARATOR ', ') AS LinkedRMs" +
                "   FROM MM_RMScrapLink l" +
                "   JOIN MM_RawMaterials r ON r.RMID = l.RMID" +
                "   GROUP BY l.ScrapID" +
                " ) rms ON rms.ScrapID = s.ScrapID" +
                " WHERE s.IsActive = 1" +
                " ORDER BY StockQty DESC, s.ScrapName ASC;");
        }

        // ── PACKING MATERIAL ──────────────────────────────────────────
        public static DataTable GetAllPackingMaterials()
        {
            return ExecuteQuery(
                "SELECT p.PMID, p.PMCode, p.PMName, p.PMCategory, p.Description, p.HSNCode, p.GSTRate, p.UOMID, u.UOMName, u.Abbreviation, " +
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

        public static void AddPackingMaterial(string name, string description, string hsnCode, decimal? gstRate, int uomId, decimal reorderLevel, string category = null)
        {
            string code = GeneratePMCode();
            ExecuteNonQuery(
                "INSERT INTO MM_PackingMaterials (PMCode, PMName, PMCategory, Description, HSNCode, GSTRate, UOMID, ReorderLevel, IsActive, CreatedAt) " +
                "VALUES (?code,?name,?cat,?desc,?hsn,?gst,?uom,?reorder,1,NOW());",
                new MySqlParameter("code",   code),
                new MySqlParameter("name",   name),
                new MySqlParameter("cat",    string.IsNullOrEmpty(category) ? (object)DBNull.Value : category),
                new MySqlParameter("desc",   description),
                new MySqlParameter("hsn",    hsnCode ?? (object)DBNull.Value),
                new MySqlParameter("gst",    gstRate.HasValue ? (object)gstRate.Value : DBNull.Value),
                new MySqlParameter("uom",    uomId),
                new MySqlParameter("reorder",reorderLevel));
        }

        public static void UpdatePackingMaterial(int pmId, string code, string name, string description,
            string hsnCode, decimal? gstRate, int uomId, decimal reorderLevel, string category = null)
        {
            ExecuteNonQuery(
                "UPDATE MM_PackingMaterials SET PMCode=?code, PMName=?name, PMCategory=?cat, Description=?desc, " +
                "HSNCode=?hsn, GSTRate=?gst, UOMID=?uom, ReorderLevel=?reorder WHERE PMID=?id;",
                new MySqlParameter("code",   code),
                new MySqlParameter("name",   name),
                new MySqlParameter("cat",    string.IsNullOrEmpty(category) ? (object)DBNull.Value : category),
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
            int next = Convert.ToInt32(Convert.ToString(result));
            return string.Format("S-{0:D4}", next);
        }

        public static string GenerateRMCode()
        {
            var result = ExecuteScalar(
                "SELECT IFNULL(MAX(CAST(SUBSTRING(RMCode, 3) AS UNSIGNED)), 0) + 1 " +
                "FROM MM_RawMaterials WHERE RMCode REGEXP '^R-[0-9]+$';");
            int next = Convert.ToInt32(Convert.ToString(result));
            return string.Format("R-{0:D4}", next);
        }

        public static string GeneratePMCode()
        {
            var result = ExecuteScalar(
                "SELECT IFNULL(MAX(CAST(SUBSTRING(PMCode, 3) AS UNSIGNED)), 0) + 1 " +
                "FROM MM_PackingMaterials WHERE PMCode REGEXP '^P-[0-9]+$';");
            int next = Convert.ToInt32(Convert.ToString(result));
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

        // ── MULTI-ITEM GRN NUMBER GENERATOR ─────────────────────────
        public static string GenerateMultiGRNNumber(string type)
        {
            // type: "RM", "PM", "CN", "ST" -> MGRN-RM-00001
            // Independent counter from single-item GRNs. CounterType stored
            // as "MGRN-RM" / "MGRN-PM" / "MGRN-CN" / "MGRN-ST".
            // All line items in one multi-GRN save share this single number.
            string counterType = "MGRN-" + type;
            ExecuteNonQuery(
                "INSERT IGNORE INTO MM_GRNCounter (CounterType, LastValue) VALUES (?t, 0);",
                new MySqlParameter("t", counterType));
            ExecuteNonQuery(
                "UPDATE MM_GRNCounter SET LastValue = LastValue + 1 WHERE CounterType = ?t;",
                new MySqlParameter("t", counterType));
            var seq = ExecuteScalar(
                "SELECT LastValue FROM MM_GRNCounter WHERE CounterType = ?t;",
                new MySqlParameter("t", counterType));
            return string.Format("MGRN-{0}-{1:D5}", type, Convert.ToInt32(seq));
        }

        // ── MANUAL INVOICE NUMBER GENERATOR (count-based, gap-free) ──
        public static string GenerateManualInvoiceNumber()
        {
            // Returns MN-YYYYMMDD-NNN where NNN = (count of MN-YYYYMMDD-* rows
            // saved across all 4 inward tables today) + 1, zero-padded to 3 digits.
            // Count-based (not counter-table) so deletes don't leave permanent gaps,
            // and no number is ever reserved without being saved.
            string today = DateTime.Now.ToString("yyyyMMdd");
            string pattern = "MN-" + today + "-%";

            // Sum counts across all 4 inward tables for today's MN- invoices.
            string sql =
                "SELECT SUM(c) FROM (" +
                "  SELECT COUNT(*) AS c FROM MM_RawInward       WHERE InvoiceNo LIKE ?p" +
                "  UNION ALL " +
                "  SELECT COUNT(*) AS c FROM MM_PackingInward   WHERE InvoiceNo LIKE ?p" +
                "  UNION ALL " +
                "  SELECT COUNT(*) AS c FROM MM_ConsumableInward WHERE InvoiceNo LIKE ?p" +
                "  UNION ALL " +
                "  SELECT COUNT(*) AS c FROM MM_StationaryInward WHERE InvoiceNo LIKE ?p" +
                ") t;";
            var totalObj = ExecuteScalar(sql, new MySqlParameter("p", pattern));
            long total = (totalObj == null || totalObj == DBNull.Value) ? 0 : Convert.ToInt64(totalObj);
            return string.Format("MN-{0}-{1:D3}", today, total + 1);
        }

        // ── SUPPLIER RECOVERABLES ─────────────────────────────────────
        public static DataTable GetSupplierRecoverables(int supplierId)
        {
            return ExecuteQuery(
                "SELECT i.InwardID, i.GRNNo, i.InwardDate, r.RMName, r.RMName AS MaterialName, r.RMCode, r.RMCode AS MaterialCode, " +
                "u.Abbreviation, i.ShortageQty, i.ShortageValue " +
                "FROM MM_RawInward i " +
                "JOIN MM_RawMaterials r ON r.RMID = i.RMID " +
                "JOIN MM_UOM u ON u.UOMID = r.UOMID " +
                "WHERE i.SupplierID = ?s AND i.ShortageQty > 0 " +
                "ORDER BY i.InwardDate DESC;",
                new MySqlParameter("s", supplierId));
        }

        public static DataTable GetSupplierRecoverablesPM(int supplierId)
        {
            return ExecuteQuery(
                "SELECT i.InwardID, i.GRNNo, i.InwardDate, p.PMName AS MaterialName, p.PMCode AS MaterialCode, " +
                "u.Abbreviation, i.ShortageQty, i.ShortageValue " +
                "FROM MM_PackingInward i " +
                "JOIN MM_PackingMaterials p ON p.PMID = i.PMID " +
                "JOIN MM_UOM u ON u.UOMID = p.UOMID " +
                "WHERE i.SupplierID = ?s AND i.ShortageQty > 0 " +
                "ORDER BY i.InwardDate DESC;",
                new MySqlParameter("s", supplierId));
        }

        public static DataTable GetSupplierRecoverablesCN(int supplierId)
        {
            return ExecuteQuery(
                "SELECT i.InwardID, i.GRNNo, i.InwardDate, c.ConsumableName AS MaterialName, c.ConsumableCode AS MaterialCode, " +
                "u.Abbreviation, i.ShortageQty, i.ShortageValue " +
                "FROM MM_ConsumableInward i " +
                "JOIN MM_Consumables c ON c.ConsumableID = i.ConsumableID " +
                "JOIN MM_UOM u ON u.UOMID = c.UOMID " +
                "WHERE i.SupplierID = ?s AND i.ShortageQty > 0 " +
                "ORDER BY i.InwardDate DESC;",
                new MySqlParameter("s", supplierId));
        }

        public static DataTable GetSupplierRecoverablesST(int supplierId)
        {
            return ExecuteQuery(
                "SELECT i.InwardID, i.GRNNo, i.InwardDate, s.StationaryName AS MaterialName, s.StationaryCode AS MaterialCode, " +
                "u.Abbreviation, i.ShortageQty, i.ShortageValue " +
                "FROM MM_StationaryInward i " +
                "JOIN MM_Stationaries s ON s.StationaryID = i.StationaryID " +
                "JOIN MM_UOM u ON u.UOMID = s.UOMID " +
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
                "  AND s.SupplierCode <> 'INT-PROD' " +
                "  AND i.GRNNo IN (SELECT GRNNo FROM MM_RawInward GROUP BY GRNNo HAVING COUNT(*) = 1) " +
                "ORDER BY i.InwardDate DESC, i.GRNNo DESC;",
                new MySqlParameter("from", from.Date),
                new MySqlParameter("to",   to.Date));
        }

        /// <summary>
        /// Multi-item GRNs only — GRNs with 2+ rows sharing the same GRN No.
        /// Used by MMMultiGRN.aspx history panel so operators only see
        /// their multi-item GRNs (single-item GRNs live on MMRawInward.aspx).
        /// Returns ONE summary row per GRN (grouped), not per line item.
        /// </summary>
        public static DataTable GetMultiItemRawInwardList(DateTime from, DateTime to)
        {
            return ExecuteQuery(
                "SELECT MIN(i.InwardID) AS InwardID, i.GRNNo, MIN(i.InvoiceNo) AS InvoiceNo, " +
                "MIN(i.InwardDate) AS InwardDate, MIN(i.InvoiceDate) AS InvoiceDate, " +
                "MIN(s.SupplierName) AS SupplierName, " +
                "COUNT(*) AS LineCount, " +
                "IFNULL(SUM(i.Amount),0) AS Amount, " +
                "IFNULL(SUM(i.GSTAmount),0) AS GSTAmount, " +
                "IFNULL(SUM(i.TransportCost),0) AS TransportCost, " +
                "IFNULL(SUM(i.ShortageQty),0) AS ShortageQty, " +
                "IFNULL(SUM(i.ShortageValue),0) AS ShortageValue, " +
                "MIN(i.PONo) AS PONo, " +
                "IFNULL(MAX(i.QualityCheck), 0) = 1 AS QualityCheck, " +
                "MIN(i.Status) AS Status, MIN(i.CreatedAt) AS CreatedAt " +
                "FROM MM_RawInward i " +
                "JOIN MM_Suppliers s ON s.SupplierID=i.SupplierID " +
                "WHERE i.InwardDate BETWEEN ?from AND ?to " +
                "  AND s.SupplierCode <> 'INT-PROD' " +
                "GROUP BY i.GRNNo " +
                "HAVING COUNT(*) >= 2 " +
                "ORDER BY MIN(i.InwardDate) DESC, i.GRNNo DESC;",
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

        /// <summary>
        /// Return all rows for a given GRN No. Handles both single-item GRNs (1 row)
        /// and multi-item GRNs (N rows sharing the same GRN No).
        /// </summary>
        public static DataTable GetRawInwardByGRN(string grnNo)
        {
            return ExecuteQuery(
                "SELECT i.*, s.SupplierName, r.RMName, r.RMCode, u.Abbreviation " +
                "FROM MM_RawInward i " +
                "JOIN MM_Suppliers s ON s.SupplierID=i.SupplierID " +
                "JOIN MM_RawMaterials r ON r.RMID=i.RMID " +
                "JOIN MM_UOM u ON u.UOMID=r.UOMID " +
                "WHERE i.GRNNo=?grn " +
                "ORDER BY i.InwardID;",
                new MySqlParameter("grn", grnNo));
        }

        // ── PENDING INVOICE METHODS ──────────────────────────────────

        public static DataTable GetPendingInvoiceRM()
        {
            return ExecuteQuery(
                "SELECT i.InwardID, i.GRNNo, i.InwardDate, i.Amount, s.SupplierName, r.RMName " +
                "FROM MM_RawInward i " +
                "JOIN MM_Suppliers s ON s.SupplierID=i.SupplierID " +
                "JOIN MM_RawMaterials r ON r.RMID=i.RMID " +
                "WHERE UPPER(TRIM(i.InvoiceNo)) = 'PENDING' " +
                "ORDER BY i.InwardDate DESC;");
        }

        public static DataTable GetPendingInvoicePM()
        {
            return ExecuteQuery(
                "SELECT i.InwardID, i.GRNNo, i.InwardDate, i.Amount, s.SupplierName, p.PMName AS MaterialName " +
                "FROM MM_PackingInward i " +
                "JOIN MM_Suppliers s ON s.SupplierID=i.SupplierID " +
                "JOIN MM_PackingMaterials p ON p.PMID=i.PMID " +
                "WHERE UPPER(TRIM(i.InvoiceNo)) = 'PENDING' " +
                "ORDER BY i.InwardDate DESC;");
        }

        public static DataTable GetPendingInvoiceCN()
        {
            return ExecuteQuery(
                "SELECT i.InwardID, i.GRNNo, i.InwardDate, i.Amount, s.SupplierName, c.ConsumableName AS MaterialName " +
                "FROM MM_ConsumableInward i " +
                "JOIN MM_Suppliers s ON s.SupplierID=i.SupplierID " +
                "JOIN MM_Consumables c ON c.ConsumableID=i.ConsumableID " +
                "WHERE UPPER(TRIM(i.InvoiceNo)) = 'PENDING' " +
                "ORDER BY i.InwardDate DESC;");
        }

        public static DataTable GetPendingInvoiceST()
        {
            return ExecuteQuery(
                "SELECT i.InwardID, i.GRNNo, i.InwardDate, i.Amount, s.SupplierName, st.StationaryName AS MaterialName " +
                "FROM MM_StationaryInward i " +
                "JOIN MM_Suppliers s ON s.SupplierID=i.SupplierID " +
                "JOIN MM_Stationaries st ON st.StationaryID=i.StationaryID " +
                "WHERE UPPER(TRIM(i.InvoiceNo)) = 'PENDING' " +
                "ORDER BY i.InwardDate DESC;");
        }

        public static void UpdateInvoiceNumber(string tableName, int inwardId, string invoiceNo, DateTime? invoiceDate)
        {
            string datePart = invoiceDate.HasValue ? ", InvoiceDate=?dt" : "";
            string sql = "UPDATE " + tableName + " SET InvoiceNo=?inv" + datePart + " WHERE InwardID=?id;";
            var parms = new System.Collections.Generic.List<MySqlParameter>();
            parms.Add(new MySqlParameter("?inv", invoiceNo.Trim()));
            if (invoiceDate.HasValue) parms.Add(new MySqlParameter("?dt", invoiceDate.Value));
            parms.Add(new MySqlParameter("?id", inwardId));
            ExecuteNonQuery(sql, parms.ToArray());
        }

        public static int AddRawInward(string grnNo, DateTime inwardDate, DateTime? invoiceDate,
            string invoiceNo, int supplierId, int rmId,
            decimal qtyInvoice, decimal qtyActualReceived, decimal qtyInUOM, decimal rate,
            string hsnCode, decimal? gstRate, decimal gstAmount,
            decimal transportCost, bool transportInInvoice, bool transportInGST,
            decimal loadingCharges, decimal unloadingCharges, bool qtyVerified,
            decimal totalAmount, string poNo, string remarks,
            bool qualityCheck, string status, int createdBy)
        {
            decimal shortageQty   = qtyInvoice - qtyActualReceived;
            if (shortageQty < 0) shortageQty = 0;
            decimal shortageValue = shortageQty * rate;

            ExecuteNonQuery(
                "INSERT INTO MM_RawInward (GRNNo, InwardDate, InvoiceNo, InvoiceDate, SupplierID, RMID, " +
                "Quantity, QtyActualReceived, QtyInUOM, Rate, Amount, HSNCode, GSTRate, GSTAmount, " +
                "TransportCost, TransportInInvoice, TransportInGST, LoadingCharges, UnloadingCharges, QtyVerified, " +
                "ShortageQty, ShortageValue, " +
                "PONo, Remarks, QualityCheck, Status, CreatedBy, CreatedAt) " +
                "VALUES (?grn,?dt,?invno,?invdt,?sup,?rm,?qty,?actqty,?qtyuom,?rate,?amt," +
                "?hsn,?gstr,?gstamt,?trans,?transinv,?transgst,?load,?unload,?qtyv,?shqty,?shval,?po,?rem,?qc,?stat,?by,NOW());",
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
                new MySqlParameter("load",     loadingCharges),
                new MySqlParameter("unload",   unloadingCharges),
                new MySqlParameter("qtyv",     qtyVerified ? 1 : 0),
                new MySqlParameter("shqty",    shortageQty),
                new MySqlParameter("shval",    shortageValue),
                new MySqlParameter("po",       poNo),
                new MySqlParameter("rem",      remarks),
                new MySqlParameter("qc",       qualityCheck ? 1 : 0),
                new MySqlParameter("stat",     status),
                new MySqlParameter("by",       createdBy));
            int newId = Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));

            // Trigger derived RM price update (e.g. Black Sesame → Roasted Black Sesame)
            if (rate > 0)
                UpdateDerivedRMPrices(rmId, rate);

            return newId;
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
                "  AND s.SupplierCode <> 'INT-PROD' " +
                "  AND i.GRNNo IN (SELECT GRNNo FROM MM_PackingInward GROUP BY GRNNo HAVING COUNT(*) = 1) " +
                "ORDER BY i.InwardDate DESC, i.GRNNo DESC;",
                new MySqlParameter("from", from.Date),
                new MySqlParameter("to",   to.Date));
        }

        /// <summary>Multi-item PM GRNs only (2+ rows per GRN).</summary>
        public static DataTable GetMultiItemPackingInwardList(DateTime from, DateTime to)
        {
            return ExecuteQuery(
                "SELECT MIN(i.InwardID) AS InwardID, i.GRNNo, MIN(i.InvoiceNo) AS InvoiceNo, " +
                "MIN(i.InwardDate) AS InwardDate, MIN(i.InvoiceDate) AS InvoiceDate, " +
                "MIN(s.SupplierName) AS SupplierName, " +
                "COUNT(*) AS LineCount, " +
                "IFNULL(SUM(i.Amount),0) AS Amount, " +
                "IFNULL(SUM(i.GSTAmount),0) AS GSTAmount, " +
                "IFNULL(SUM(i.TransportCost),0) AS TransportCost, " +
                "IFNULL(SUM(i.ShortageQty),0) AS ShortageQty, " +
                "IFNULL(SUM(i.ShortageValue),0) AS ShortageValue, " +
                "MIN(i.PONo) AS PONo, " +
                "IFNULL(MAX(i.QualityCheck), 0) = 1 AS QualityCheck, " +
                "MIN(i.Status) AS Status, MIN(i.CreatedAt) AS CreatedAt " +
                "FROM MM_PackingInward i " +
                "JOIN MM_Suppliers s ON s.SupplierID=i.SupplierID " +
                "WHERE i.InwardDate BETWEEN ?from AND ?to " +
                "  AND s.SupplierCode <> 'INT-PROD' " +
                "GROUP BY i.GRNNo " +
                "HAVING COUNT(*) >= 2 " +
                "ORDER BY MIN(i.InwardDate) DESC, i.GRNNo DESC;",
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

        /// <summary>
        /// Return all rows for a given GRN No (single-item or multi-item).
        /// </summary>
        public static DataTable GetPackingInwardByGRN(string grnNo)
        {
            return ExecuteQuery(
                "SELECT i.*, s.SupplierName, p.PMName, p.PMCode, u.Abbreviation " +
                "FROM MM_PackingInward i " +
                "JOIN MM_Suppliers s ON s.SupplierID=i.SupplierID " +
                "JOIN MM_PackingMaterials p ON p.PMID=i.PMID " +
                "JOIN MM_UOM u ON u.UOMID=p.UOMID " +
                "WHERE i.GRNNo=?grn " +
                "ORDER BY i.InwardID;",
                new MySqlParameter("grn", grnNo));
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
            decimal loadingCharges, decimal unloadingCharges, bool qtyVerified,
            decimal totalAmount, string poNo, string remarks,
            bool qualityCheck, string status, int createdBy)
        {
            decimal shortageQty   = qtyInvoice - qtyActualReceived;
            if (shortageQty < 0) shortageQty = 0;
            decimal shortageValue = shortageQty * rate;

            ExecuteNonQuery(
                "INSERT INTO MM_PackingInward (GRNNo, InwardDate, InvoiceNo, InvoiceDate, SupplierID, PMID, " +
                "Quantity, QtyActualReceived, QtyInUOM, Rate, Amount, HSNCode, GSTRate, GSTAmount, " +
                "TransportCost, TransportInInvoice, TransportInGST, LoadingCharges, UnloadingCharges, QtyVerified, ShortageQty, ShortageValue, " +
                "PONo, Remarks, QualityCheck, Status, CreatedBy, CreatedAt) " +
                "VALUES (?grn,?dt,?invno,?invdt,?sup,?pm,?qty,?actqty,?qtyuom,?rate,?amt," +
                "?hsn,?gstr,?gstamt,?trans,?transinv,?transgst,?load,?unload,?qtyv,?shqty,?shval,?po,?rem,?qc,?stat,?by,NOW());",
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
                new MySqlParameter("load",     loadingCharges),
                new MySqlParameter("unload",   unloadingCharges),
                new MySqlParameter("qtyv",     qtyVerified ? 1 : 0),
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
            object val = ExecuteScalar("SELECT IFNULL(MAX(CAST(SUBSTRING(ConsumableCode,3) AS UNSIGNED)), 0) FROM MM_Consumables WHERE ConsumableCode LIKE 'C-%';");
            int next = (val == null || val == DBNull.Value) ? 1 : Convert.ToInt32(Convert.ToString(val)) + 1;
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
            object val = ExecuteScalar("SELECT IFNULL(MAX(CAST(SUBSTRING(StationaryCode,4) AS UNSIGNED)), 0) FROM MM_Stationaries WHERE StationaryCode LIKE 'ST-%';");
            int next = (val == null || val == DBNull.Value) ? 1 : Convert.ToInt32(Convert.ToString(val)) + 1;
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
                "  AND s.SupplierCode <> 'INT-PROD' " +
                "  AND i.GRNNo IN (SELECT GRNNo FROM MM_ConsumableInward GROUP BY GRNNo HAVING COUNT(*) = 1) " +
                "ORDER BY i.InwardDate DESC, i.GRNNo DESC;",
                new MySqlParameter("from", from.Date),
                new MySqlParameter("to",   to.Date));
        }

        /// <summary>Multi-item CN GRNs only (2+ rows per GRN).</summary>
        public static DataTable GetMultiItemConsumableInwardList(DateTime from, DateTime to)
        {
            return ExecuteQuery(
                "SELECT MIN(i.InwardID) AS InwardID, i.GRNNo, MIN(i.InvoiceNo) AS InvoiceNo, " +
                "MIN(i.InwardDate) AS InwardDate, MIN(i.InvoiceDate) AS InvoiceDate, " +
                "MIN(s.SupplierName) AS SupplierName, " +
                "COUNT(*) AS LineCount, " +
                "IFNULL(SUM(i.Amount),0) AS Amount, " +
                "IFNULL(SUM(i.GSTAmount),0) AS GSTAmount, " +
                "IFNULL(SUM(i.TransportCost),0) AS TransportCost, " +
                "IFNULL(SUM(i.ShortageQty),0) AS ShortageQty, " +
                "IFNULL(SUM(i.ShortageValue),0) AS ShortageValue, " +
                "MIN(i.PONo) AS PONo, " +
                "IFNULL(MAX(i.QualityCheck), 0) = 1 AS QualityCheck, " +
                "MIN(i.Status) AS Status, MIN(i.CreatedAt) AS CreatedAt " +
                "FROM MM_ConsumableInward i " +
                "JOIN MM_Suppliers s ON s.SupplierID=i.SupplierID " +
                "WHERE i.InwardDate BETWEEN ?from AND ?to " +
                "  AND s.SupplierCode <> 'INT-PROD' " +
                "GROUP BY i.GRNNo " +
                "HAVING COUNT(*) >= 2 " +
                "ORDER BY MIN(i.InwardDate) DESC, i.GRNNo DESC;",
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

        /// <summary>
        /// Return all rows for a given GRN No (single-item or multi-item).
        /// </summary>
        public static DataTable GetConsumableInwardByGRN(string grnNo)
        {
            return ExecuteQuery(
                "SELECT i.*, s.SupplierName, c.ConsumableName, c.ConsumableCode, u.Abbreviation " +
                "FROM MM_ConsumableInward i " +
                "JOIN MM_Suppliers s ON s.SupplierID=i.SupplierID " +
                "JOIN MM_Consumables c ON c.ConsumableID=i.ConsumableID " +
                "JOIN MM_UOM u ON u.UOMID=c.UOMID " +
                "WHERE i.GRNNo=?grn " +
                "ORDER BY i.InwardID;",
                new MySqlParameter("grn", grnNo));
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
            decimal loadingCharges, decimal unloadingCharges, bool qtyVerified,
            decimal totalAmount, string poNo, string remarks,
            bool qualityCheck, string status, int createdBy)
        {
            decimal shortageQty   = qtyInvoice - qtyActualReceived;
            if (shortageQty < 0) shortageQty = 0;
            decimal shortageValue = shortageQty * rate;

            ExecuteNonQuery(
                "INSERT INTO MM_ConsumableInward (GRNNo, InwardDate, InvoiceNo, InvoiceDate, SupplierID, ConsumableID, " +
                "Quantity, QtyActualReceived, QtyInUOM, Rate, Amount, HSNCode, GSTRate, GSTAmount, " +
                "TransportCost, TransportInInvoice, TransportInGST, LoadingCharges, UnloadingCharges, QtyVerified, ShortageQty, ShortageValue, " +
                "PONo, Remarks, QualityCheck, Status, CreatedBy, CreatedAt) " +
                "VALUES (?grn,?dt,?invno,?invdt,?sup,?con,?qty,?actqty,?qtyuom,?rate,?amt," +
                "?hsn,?gstr,?gstamt,?trans,?transinv,?transgst,?load,?unload,?qtyv,?shqty,?shval,?po,?rem,?qc,?stat,?by,NOW());",
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
                new MySqlParameter("load",     loadingCharges),
                new MySqlParameter("unload",   unloadingCharges),
                new MySqlParameter("qtyv",     qtyVerified ? 1 : 0),
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
                "  AND s.SupplierCode <> 'INT-PROD' " +
                "  AND i.GRNNo IN (SELECT GRNNo FROM MM_StationaryInward GROUP BY GRNNo HAVING COUNT(*) = 1) " +
                "ORDER BY i.InwardDate DESC, i.GRNNo DESC;",
                new MySqlParameter("from", from.Date),
                new MySqlParameter("to",   to.Date));
        }

        /// <summary>Multi-item ST GRNs only (2+ rows per GRN).</summary>
        public static DataTable GetMultiItemStationaryInwardList(DateTime from, DateTime to)
        {
            return ExecuteQuery(
                "SELECT MIN(i.InwardID) AS InwardID, i.GRNNo, MIN(i.InvoiceNo) AS InvoiceNo, " +
                "MIN(i.InwardDate) AS InwardDate, MIN(i.InvoiceDate) AS InvoiceDate, " +
                "MIN(s.SupplierName) AS SupplierName, " +
                "COUNT(*) AS LineCount, " +
                "IFNULL(SUM(i.Amount),0) AS Amount, " +
                "IFNULL(SUM(i.GSTAmount),0) AS GSTAmount, " +
                "IFNULL(SUM(i.TransportCost),0) AS TransportCost, " +
                "IFNULL(SUM(i.ShortageQty),0) AS ShortageQty, " +
                "IFNULL(SUM(i.ShortageValue),0) AS ShortageValue, " +
                "MIN(i.PONo) AS PONo, " +
                "IFNULL(MAX(i.QualityCheck), 0) = 1 AS QualityCheck, " +
                "MIN(i.Status) AS Status, MIN(i.CreatedAt) AS CreatedAt " +
                "FROM MM_StationaryInward i " +
                "JOIN MM_Suppliers s ON s.SupplierID=i.SupplierID " +
                "WHERE i.InwardDate BETWEEN ?from AND ?to " +
                "  AND s.SupplierCode <> 'INT-PROD' " +
                "GROUP BY i.GRNNo " +
                "HAVING COUNT(*) >= 2 " +
                "ORDER BY MIN(i.InwardDate) DESC, i.GRNNo DESC;",
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

        /// <summary>
        /// Return all rows for a given GRN No (single-item or multi-item).
        /// </summary>
        public static DataTable GetStationaryInwardByGRN(string grnNo)
        {
            return ExecuteQuery(
                "SELECT i.*, s.SupplierName, st.StationaryName, st.StationaryCode, u.Abbreviation " +
                "FROM MM_StationaryInward i " +
                "JOIN MM_Suppliers s ON s.SupplierID=i.SupplierID " +
                "JOIN MM_Stationaries st ON st.StationaryID=i.StationaryID " +
                "JOIN MM_UOM u ON u.UOMID=st.UOMID " +
                "WHERE i.GRNNo=?grn " +
                "ORDER BY i.InwardID;",
                new MySqlParameter("grn", grnNo));
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
            decimal loadingCharges, decimal unloadingCharges, bool qtyVerified,
            decimal totalAmount, string poNo, string remarks,
            bool qualityCheck, string status, int createdBy)
        {
            decimal shortageQty   = qtyInvoice - qtyActualReceived;
            if (shortageQty < 0) shortageQty = 0;
            decimal shortageValue = shortageQty * rate;

            ExecuteNonQuery(
                "INSERT INTO MM_StationaryInward (GRNNo, InwardDate, InvoiceNo, InvoiceDate, SupplierID, StationaryID, " +
                "Quantity, QtyActualReceived, QtyInUOM, Rate, Amount, HSNCode, GSTRate, GSTAmount, " +
                "TransportCost, TransportInInvoice, TransportInGST, LoadingCharges, UnloadingCharges, QtyVerified, ShortageQty, ShortageValue, " +
                "PONo, Remarks, QualityCheck, Status, CreatedBy, CreatedAt) " +
                "VALUES (?grn,?dt,?invno,?invdt,?sup,?st,?qty,?actqty,?qtyuom,?rate,?amt," +
                "?hsn,?gstr,?gstamt,?trans,?transinv,?transgst,?load,?unload,?qtyv,?shqty,?shval,?po,?rem,?qc,?stat,?by,NOW());",
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
                new MySqlParameter("load",     loadingCharges),
                new MySqlParameter("unload",   unloadingCharges),
                new MySqlParameter("qtyv",     qtyVerified ? 1 : 0),
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

        // ── PM CATEGORIES ────────────────────────────────────────────────────
        public static DataTable GetPMCategories(bool activeOnly = true)
        {
            if (activeOnly)
                return ExecuteQuery("SELECT CategoryID, CategoryName FROM MM_PMCategories WHERE IsActive=1 ORDER BY CategoryName;");
            else
                return ExecuteQuery("SELECT CategoryID, CategoryName, IsActive FROM MM_PMCategories ORDER BY CategoryName;");
        }

        public static void AddPMCategory(string name)
        {
            ExecuteNonQuery(
                "INSERT INTO MM_PMCategories (CategoryName) VALUES(?n);",
                new MySqlParameter("?n", name));
        }

        // ── STOCK RECONCILIATION ──────────────────────────────────────────────

        /// Get all materials of a given type with current system stock
        public static DataTable GetStockForReconciliation(string materialType)
        {
            string sql = "";
            switch (materialType)
            {
                case "RM":
                    sql = "SELECT t.* FROM (" +
                          "SELECT r.RMID AS MaterialID, r.RMCode AS Code, r.RMName AS Name, u.Abbreviation AS UOM," +
                          " ROUND(IFNULL(os.Quantity,0) + IFNULL(grn.TotalReceived,0) - IFNULL(con.TotalConsumed,0), 4) AS SystemStock" +
                          " FROM MM_RawMaterials r" +
                          " JOIN MM_UOM u ON u.UOMID = r.UOMID" +
                          " LEFT JOIN MM_OpeningStock os ON os.MaterialType='RM' AND os.MaterialID=r.RMID" +
                          " LEFT JOIN (SELECT RMID, SUM(QtyActualReceived) AS TotalReceived FROM MM_RawInward GROUP BY RMID) grn ON grn.RMID=r.RMID" +
                          " LEFT JOIN (SELECT RMID, SUM(QtyConsumed) AS TotalConsumed FROM MM_StockConsumption GROUP BY RMID) con ON con.RMID=r.RMID" +
                          " WHERE r.IsActive=1 AND LOWER(TRIM(r.RMName)) != 'ro water'" +
                          ") t WHERE t.SystemStock > 0 ORDER BY t.Name;";
                    break;
                case "PM":
                    sql = "SELECT t.* FROM (" +
                          "SELECT p.PMID AS MaterialID, p.PMCode AS Code, p.PMName AS Name, u.Abbreviation AS UOM," +
                          " ROUND(IFNULL(os.Quantity,0) + IFNULL(grn.TotalReceived,0) - IFNULL(con.TotalConsumed,0), 4) AS SystemStock" +
                          " FROM MM_PackingMaterials p" +
                          " JOIN MM_UOM u ON u.UOMID = p.UOMID" +
                          " LEFT JOIN MM_OpeningStock os ON os.MaterialType='PM' AND os.MaterialID=p.PMID" +
                          " LEFT JOIN (SELECT PMID, SUM(QtyActualReceived) AS TotalReceived FROM MM_PackingInward GROUP BY PMID) grn ON grn.PMID=p.PMID" +
                          " LEFT JOIN (SELECT PMID, SUM(QtyUsed) AS TotalConsumed FROM PK_PMConsumption GROUP BY PMID) con ON con.PMID=p.PMID" +
                          " WHERE p.IsActive=1" +
                          ") t WHERE t.SystemStock > 0 ORDER BY t.Name;";
                    break;
                case "CM":
                    sql = "SELECT t.* FROM (" +
                          "SELECT c.ConsumableID AS MaterialID, c.ConsumableCode AS Code, c.ConsumableName AS Name, u.Abbreviation AS UOM," +
                          " ROUND(IFNULL(os.Quantity,0) + IFNULL(grn.TotalReceived,0), 4) AS SystemStock" +
                          " FROM MM_Consumables c" +
                          " JOIN MM_UOM u ON u.UOMID = c.UOMID" +
                          " LEFT JOIN MM_OpeningStock os ON os.MaterialType='CM' AND os.MaterialID=c.ConsumableID" +
                          " LEFT JOIN (SELECT ConsumableID, SUM(QtyActualReceived) AS TotalReceived FROM MM_ConsumableInward GROUP BY ConsumableID) grn ON grn.ConsumableID=c.ConsumableID" +
                          " WHERE c.IsActive=1" +
                          ") t WHERE t.SystemStock > 0 ORDER BY t.Name;";
                    break;
                case "ST":
                    sql = "SELECT t.* FROM (" +
                          "SELECT s.StationaryID AS MaterialID, s.StationaryCode AS Code, s.StationaryName AS Name, u.Abbreviation AS UOM," +
                          " ROUND(IFNULL(os.Quantity,0) + IFNULL(grn.TotalReceived,0), 4) AS SystemStock" +
                          " FROM MM_Stationaries s" +
                          " JOIN MM_UOM u ON u.UOMID = s.UOMID" +
                          " LEFT JOIN MM_OpeningStock os ON os.MaterialType='ST' AND os.MaterialID=s.StationaryID" +
                          " LEFT JOIN (SELECT StationaryID, SUM(QtyActualReceived) AS TotalReceived FROM MM_StationaryInward GROUP BY StationaryID) grn ON grn.StationaryID=s.StationaryID" +
                          " WHERE s.IsActive=1" +
                          ") t WHERE t.SystemStock > 0 ORDER BY t.Name;";
                    break;
            }
            return ExecuteQuery(sql);
        }

        /// Save or update a single physical stock entry for today
        public static void SavePhysicalStock(DateTime sessionDate, string materialType, int materialId, decimal physicalQty, int userId)
        {
            ExecuteNonQuery(
                "INSERT INTO MM_PhysicalStock (SessionDate, MaterialType, MaterialID, PhysicalQty, EnteredBy)" +
                " VALUES (?dt, ?mt, ?mid, ?qty, ?uid)" +
                " ON DUPLICATE KEY UPDATE PhysicalQty=?qty2, EnteredBy=?uid2, UpdatedAt=NOW();",
                new MySqlParameter("?dt", sessionDate),
                new MySqlParameter("?mt", materialType),
                new MySqlParameter("?mid", materialId),
                new MySqlParameter("?qty", physicalQty),
                new MySqlParameter("?qty2", physicalQty),
                new MySqlParameter("?uid", userId),
                new MySqlParameter("?uid2", userId));
        }

        /// Get all physical stock entries for a date and material type
        public static DataTable GetPhysicalStock(DateTime sessionDate, string materialType)
        {
            return ExecuteQuery(
                "SELECT MaterialID, PhysicalQty FROM MM_PhysicalStock WHERE SessionDate=?dt AND MaterialType=?mt;",
                new MySqlParameter("?dt", sessionDate),
                new MySqlParameter("?mt", materialType));
        }

        /// Save reconciliation snapshot (comparison results)
        public static void SaveReconciliationSnapshot(DateTime reconDate, string materialType, int materialId,
            decimal physicalQty, decimal systemQty)
        {
            decimal variance = physicalQty - systemQty;
            decimal pct = systemQty != 0 ? (variance / systemQty) * 100 : (physicalQty != 0 ? 100 : 0);
            ExecuteNonQuery(
                "INSERT INTO MM_StockReconciliation (ReconDate, MaterialType, MaterialID, PhysicalQty, SystemQty, Variance, VariancePct)" +
                " VALUES (?dt, ?mt, ?mid, ?phys, ?sys, ?var, ?pct)" +
                " ON DUPLICATE KEY UPDATE PhysicalQty=?phys2, SystemQty=?sys2, Variance=?var2, VariancePct=?pct2;",
                new MySqlParameter("?dt", reconDate),
                new MySqlParameter("?mt", materialType),
                new MySqlParameter("?mid", materialId),
                new MySqlParameter("?phys", physicalQty),
                new MySqlParameter("?phys2", physicalQty),
                new MySqlParameter("?sys", systemQty),
                new MySqlParameter("?sys2", systemQty),
                new MySqlParameter("?var", variance),
                new MySqlParameter("?var2", variance),
                new MySqlParameter("?pct", pct),
                new MySqlParameter("?pct2", pct));
        }

        // ── ROLE-BASED ACCESS CHECK ──────────────────────────────────────
        public static bool RoleHasAppAccess(string roleCode, string appCode)
        {
            if (roleCode == "Super") return true;
            try
            {
                var dt = ExecuteQuery(
                    "SELECT CanAccess FROM ERP_RoleAppAccess WHERE RoleCode=?rc AND AppCode=?ac;",
                    new MySqlParameter("?rc", roleCode),
                    new MySqlParameter("?ac", appCode));
                return dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["CanAccess"]) == 1;
            }
            catch { return true; } // Fail open — if table missing, allow access
        }

        public static bool RoleHasModuleAccess(string roleCode, string appCode, string moduleCode)
        {
            if (roleCode == "Super") return true;
            try
            {
                var dt = ExecuteQuery(
                    "SELECT CanAccess FROM ERP_RoleModuleAccess WHERE RoleCode=?rc AND AppCode=?ac AND ModuleCode=?mc;",
                    new MySqlParameter("?rc", roleCode),
                    new MySqlParameter("?ac", appCode),
                    new MySqlParameter("?mc", moduleCode));
                return dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["CanAccess"]) == 1;
            }
            catch { return true; } // Fail open
        }

        // ═════════════════════════════════════════════════════════════════
        //  STOCK TRANSFER (Store -> Floor)
        // ═════════════════════════════════════════════════════════════════

        // ── Locations master ─────────────────────────────────────────────
        public static DataTable GetActiveLocations()
        {
            return ExecuteQuery(
                "SELECT LocationID, LocationCode, LocationName, LocationType " +
                "FROM MM_Locations WHERE IsActive=1 ORDER BY LocationName;");
        }

        public static int GetLocationIdByCode(string code)
        {
            var v = ExecuteScalar(
                "SELECT LocationID FROM MM_Locations WHERE LocationCode=?c;",
                new MySqlParameter("c", code));
            if (v == null || v == DBNull.Value) return 0;
            return Convert.ToInt32(v);
        }

        // ── Transfer number generator ────────────────────────────────────
        public static string GenerateTransferNumber()
        {
            // TR-YYYYMMDD-NNN where NNN = (today's TR-* count) + 1, zero-padded.
            // Count-based, gap-free, no counter table needed.
            string today = DateTime.Now.ToString("yyyyMMdd");
            string pattern = "TR-" + today + "-%";
            var v = ExecuteScalar(
                "SELECT COUNT(*) FROM MM_StockTransfer WHERE TransferNo LIKE ?p;",
                new MySqlParameter("p", pattern));
            long total = (v == null || v == DBNull.Value) ? 0 : Convert.ToInt64(v);
            return string.Format("TR-{0}-{1:D3}", today, total + 1);
        }

        // ── Save a transfer (header + lines) ─────────────────────────────
        // Returns the new TransferID. Lines is a list of tuples:
        //   (MaterialType: "PM"|"CN"|"ST", MaterialID, Qty, UOMID, ConsumedAtIssue, Remarks)
        // ── Look up ConsumptionMode for a (type, id) ─────────────────────
        // Returns 'AT_ISSUE' or 'IN_PRODUCTION'. Defaults if missing/unknown.
        public static string GetConsumptionMode(string materialType, int materialId)
        {
            string sql, idCol, table, defaultMode;
            switch ((materialType ?? "").ToUpper())
            {
                case "PM":
                    table = "MM_PackingMaterials"; idCol = "PMID"; defaultMode = "IN_PRODUCTION"; break;
                case "CN":
                    table = "MM_Consumables";      idCol = "ConsumableID"; defaultMode = "AT_ISSUE"; break;
                case "ST":
                    table = "MM_Stationaries";     idCol = "StationaryID"; defaultMode = "AT_ISSUE"; break;
                default:
                    return "IN_PRODUCTION";
            }
            sql = "SELECT ConsumptionMode FROM " + table + " WHERE " + idCol + "=?id;";
            try
            {
                var v = ExecuteScalar(sql, new MySqlParameter("id", materialId));
                if (v == null || v == DBNull.Value) return defaultMode;
                string m = v.ToString();
                if (string.IsNullOrEmpty(m)) return defaultMode;
                return (m == "AT_ISSUE" || m == "IN_PRODUCTION") ? m : defaultMode;
            }
            catch
            {
                // Column may not exist yet (pre-migration); fail safe to per-type default.
                return defaultMode;
            }
        }

        // ── Set ConsumptionMode for a (type, id) ─────────────────────────
        public static void SetConsumptionMode(string materialType, int materialId, string mode)
        {
            string table, idCol;
            switch ((materialType ?? "").ToUpper())
            {
                case "PM": table = "MM_PackingMaterials"; idCol = "PMID";        break;
                case "CN": table = "MM_Consumables";      idCol = "ConsumableID"; break;
                case "ST": table = "MM_Stationaries";     idCol = "StationaryID"; break;
                default: return;
            }
            string normalized = (mode == "AT_ISSUE") ? "AT_ISSUE" : "IN_PRODUCTION";
            ExecuteNonQuery(
                "UPDATE " + table + " SET ConsumptionMode=?m WHERE " + idCol + "=?id;",
                new MySqlParameter("m", normalized),
                new MySqlParameter("id", materialId));
        }

        // ── Materials list for the bulk-edit page (PM + CN + ST in one DataTable) ─
        // Columns: MaterialType ('PM'|'CN'|'ST'), MaterialID, Code, Name, UOM, ConsumptionMode, IsActive
        public static DataTable GetMaterialsForBulkEdit()
        {
            return ExecuteQuery(
                "SELECT 'PM' AS MaterialType, p.PMID AS MaterialID, p.PMCode AS Code, p.PMName AS Name, " +
                "       u.Abbreviation AS UOM, p.ConsumptionMode, p.IsActive " +
                "FROM MM_PackingMaterials p LEFT JOIN MM_UOM u ON u.UOMID=p.UOMID WHERE p.IsActive=1 " +
                "UNION ALL " +
                "SELECT 'CN', c.ConsumableID, c.ConsumableCode, c.ConsumableName, " +
                "       u.Abbreviation, c.ConsumptionMode, c.IsActive " +
                "FROM MM_Consumables c LEFT JOIN MM_UOM u ON u.UOMID=c.UOMID WHERE c.IsActive=1 " +
                "UNION ALL " +
                "SELECT 'ST', s.StationaryID, s.StationaryCode, s.StationaryName, " +
                "       u.Abbreviation, s.ConsumptionMode, s.IsActive " +
                "FROM MM_Stationaries s LEFT JOIN MM_UOM u ON u.UOMID=s.UOMID WHERE s.IsActive=1 " +
                "ORDER BY MaterialType, Name;");
        }

        // ── Save a transfer (header + lines) ─────────────────────────────
        // ConsumedAtIssue is now resolved AUTOMATICALLY from each material's
        // master ConsumptionMode flag — line[4] passed by callers is ignored
        // (kept in the signature for backward compatibility).
        public static int AddStockTransfer(
            string transferNo, DateTime transferDate, string transferType,
            int fromLocationId, int toLocationId,
            string requestedBy, int issuedBy, string remarks,
            System.Collections.Generic.List<object[]> lines)
        {
            // Insert header
            ExecuteNonQuery(
                "INSERT INTO MM_StockTransfer " +
                "(TransferNo, TransferDate, TransferType, FromLocationID, ToLocationID, " +
                " RequestedBy, IssuedBy, Remarks, Status) " +
                "VALUES (?tn, ?td, ?tt, ?fl, ?tl, ?rb, ?ib, ?rm, 'Issued');",
                new MySqlParameter("tn", transferNo),
                new MySqlParameter("td", transferDate),
                new MySqlParameter("tt", transferType),
                new MySqlParameter("fl", fromLocationId),
                new MySqlParameter("tl", toLocationId),
                new MySqlParameter("rb", (object)requestedBy ?? DBNull.Value),
                new MySqlParameter("ib", issuedBy),
                new MySqlParameter("rm", (object)remarks ?? DBNull.Value));

            int transferId = Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));

            // Insert each line — ConsumedAtIssue derived from master ConsumptionMode
            foreach (var line in lines)
            {
                string matType   = (string)line[0];
                int    matId     = Convert.ToInt32(line[1]);
                decimal qty      = Convert.ToDecimal(line[2]);
                object uomObj    = line[3];                                  // int or null
                // line[4] (operator-passed consumed flag) IGNORED — sourced from master
                string lineRem   = line.Length > 5 ? (string)line[5] : null;

                string mode = GetConsumptionMode(matType, matId);
                int consumed = (mode == "AT_ISSUE") ? 1 : 0;

                ExecuteNonQuery(
                    "INSERT INTO MM_StockTransferLine " +
                    "(TransferID, MaterialType, MaterialID, Quantity, UOMID, ConsumedAtIssue, Remarks) " +
                    "VALUES (?tid, ?mt, ?mid, ?qty, ?uom, ?ci, ?lr);",
                    new MySqlParameter("tid", transferId),
                    new MySqlParameter("mt", matType),
                    new MySqlParameter("mid", matId),
                    new MySqlParameter("qty", qty),
                    new MySqlParameter("uom", uomObj == null ? (object)DBNull.Value : uomObj),
                    new MySqlParameter("ci", consumed),
                    new MySqlParameter("lr", (object)lineRem ?? DBNull.Value));
            }

            return transferId;
        }

        // ── List of transfers (header view, with line count + total qty) ─
        public static DataTable GetStockTransferList(DateTime fromDate, DateTime toDate)
        {
            return ExecuteQuery(
                "SELECT t.TransferID, t.TransferNo, t.TransferDate, t.TransferType, " +
                "       fl.LocationName AS FromLocation, tl.LocationName AS ToLocation, " +
                "       t.RequestedBy, u.FullName AS IssuedByName, t.Remarks, t.Status, t.CreatedAt, " +
                "       (SELECT COUNT(*) FROM MM_StockTransferLine l WHERE l.TransferID=t.TransferID) AS LineCount " +
                "FROM MM_StockTransfer t " +
                "JOIN MM_Locations fl ON fl.LocationID = t.FromLocationID " +
                "JOIN MM_Locations tl ON tl.LocationID = t.ToLocationID " +
                "LEFT JOIN Users u ON u.UserID = t.IssuedBy " +
                "WHERE t.TransferDate BETWEEN ?fd AND ?td " +
                "ORDER BY t.TransferDate DESC, t.TransferID DESC;",
                new MySqlParameter("fd", fromDate),
                new MySqlParameter("td", toDate));
        }

        // ── Detail of one transfer (lines with material name resolved) ───
        public static DataTable GetStockTransferDetail(int transferId)
        {
            // Polymorphic FK — use UNION ALL across the three master tables to resolve names.
            return ExecuteQuery(
                "SELECT l.LineID, l.MaterialType, l.MaterialID, l.Quantity, l.UOMID, " +
                "       l.ConsumedAtIssue, l.Remarks, " +
                "       COALESCE(pm.PMName, cn.ConsumableName, st.StationaryName) AS MaterialName, " +
                "       COALESCE(pm.PMCode, cn.ConsumableCode, st.StationaryCode) AS MaterialCode, " +
                "       u.Abbreviation AS UOM " +
                "FROM MM_StockTransferLine l " +
                "LEFT JOIN MM_PackingMaterials pm ON l.MaterialType='PM' AND pm.PMID=l.MaterialID " +
                "LEFT JOIN MM_Consumables       cn ON l.MaterialType='CN' AND cn.ConsumableID=l.MaterialID " +
                "LEFT JOIN MM_Stationaries     st ON l.MaterialType='ST' AND st.StationaryID=l.MaterialID " +
                "LEFT JOIN MM_UOM u ON u.UOMID=l.UOMID " +
                "WHERE l.TransferID=?tid;",
                new MySqlParameter("tid", transferId));
        }

        // ── Active material lists for the transfer page (3 separate calls) ─
        // Reuse existing GetActivePackingMaterials / GetActiveConsumables
        // and add the missing GetActiveStationaryForTransfer.
        public static DataTable GetActiveStationaryForTransfer()
        {
            return ExecuteQuery(
                "SELECT s.StationaryID, s.StationaryCode, s.StationaryName, " +
                "       s.UOMID, u.Abbreviation " +
                "FROM MM_Stationaries s LEFT JOIN MM_UOM u ON u.UOMID=s.UOMID " +
                "WHERE s.IsActive=1 ORDER BY s.StationaryName;");
        }

        // ── Stock position at a location (per material type) ─────────────
        // Returns: MaterialID, MaterialName, OpeningQty, ReceivedQty, IssuedQty,
        //          ReceivedReturnQty, ConsumedQty, OnHandQty
        // Rules:
        //   STORES  on-hand = Opening (MM_OpeningStock + MM_LocationOpeningStock@STORES)
        //                   + GRN Received - Consumption (existing Stores logic)
        //                   - Issued OUT to Floor + Returned IN from Floor
        //   FLOOR   on-hand = MM_LocationOpeningStock@FLOOR (always 0 today)
        //                   + Issued IN from Stores - Consumption-at-issue
        //                   - Returned OUT to Stores
        //
        // For Phase A simplicity, we only return Stores impact and Floor impact
        // from transfers. The existing GRN/consumption flows are unchanged.
        public static DataTable GetLocationTransferImpact(string materialType, int locationId)
        {
            // Sums of qty issued INTO and OUT OF this location, per material of the given type
            return ExecuteQuery(
                "SELECT l.MaterialID, " +
                "       SUM(CASE WHEN t.ToLocationID=?lid   AND t.Status='Issued' THEN l.Quantity ELSE 0 END) AS QtyIn, " +
                "       SUM(CASE WHEN t.FromLocationID=?lid AND t.Status='Issued' THEN l.Quantity ELSE 0 END) AS QtyOut, " +
                "       SUM(CASE WHEN t.ToLocationID=?lid   AND t.Status='Issued' AND l.ConsumedAtIssue=1 THEN l.Quantity ELSE 0 END) AS QtyConsumedAtIssue " +
                "FROM MM_StockTransferLine l " +
                "JOIN MM_StockTransfer t ON t.TransferID=l.TransferID " +
                "WHERE l.MaterialType=?mt " +
                "GROUP BY l.MaterialID;",
                new MySqlParameter("lid", locationId),
                new MySqlParameter("mt", materialType));
        }
    }
}
