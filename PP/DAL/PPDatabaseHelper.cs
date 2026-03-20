using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

namespace PPApp.DAL
{
    public static class PPDatabaseHelper
    {
        // ── CONNECTION ────────────────────────────────────────────────────────
        private static string ConnStr =>
            ConfigurationManager.ConnectionStrings["StockDB"].ConnectionString;

        // ── IST HELPER ───────────────────────────────────────────────────────
        // Always use this instead of DateTime.Now to ensure IST regardless of server timezone
        public static DateTime NowIST()
        {
            try
            {
                var ist = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ist);
            }
            catch
            {
                // Fallback for Linux servers where TZ id is different
                try
                {
                    var ist = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ist);
                }
                catch
                {
                    return DateTime.UtcNow.AddHours(5).AddMinutes(30);
                }
            }
        }

        public static DateTime TodayIST() => NowIST().Date;

        private static MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(ConnStr);
            conn.Open();
            return conn;
        }

        // ── GENERIC HELPERS ───────────────────────────────────────────────────
        private static DataTable ExecuteQuery(string sql, params MySqlParameter[] prms)
        {
            using (var conn = OpenConnection())
            using (var cmd  = new MySqlCommand(sql, conn))
            {
                if (prms != null) cmd.Parameters.AddRange(prms);
                var dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }

        private static DataRow ExecuteQueryRow(string sql, params MySqlParameter[] prms)
        {
            var dt = ExecuteQuery(sql, prms);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        private static int ExecuteNonQuery(string sql, params MySqlParameter[] prms)
        {
            using (var conn = OpenConnection())
            using (var cmd  = new MySqlCommand(sql, conn))
            {
                if (prms != null) cmd.Parameters.AddRange(prms);
                return cmd.ExecuteNonQuery();
            }
        }

        private static object ExecuteScalar(string sql, params MySqlParameter[] prms)
        {
            using (var conn = OpenConnection())
            using (var cmd  = new MySqlCommand(sql, conn))
            {
                if (prms != null) cmd.Parameters.AddRange(prms);
                return cmd.ExecuteScalar();
            }
        }

        // ── USERS / AUTH (shared Users table) ─────────────────────────────────
        public static DataRow ValidateUser(string username, string passwordHash)
        {
            return ExecuteQueryRow(
                "SELECT UserID, Username, FullName, Role, IsActive, MustChangePwd " +
                "FROM Users WHERE Username=?u AND PasswordHash=?p LIMIT 1;",
                new MySqlParameter("?u", username),
                new MySqlParameter("?p", passwordHash));
        }

        public static void UpdateLastLogin(int userId)
        {
            ExecuteNonQuery(
                "UPDATE Users SET LastLogin=?now WHERE UserID=?id;",
                new MySqlParameter("?now", NowIST()),
                new MySqlParameter("?id", userId));
        }

        // ── UOM (read-only — maintained in MM) ────────────────────────────────
        public static DataTable GetActiveUOM()
        {
            return ExecuteQuery(
                "SELECT UOMID, UOMName, Abbreviation FROM MM_UOM WHERE IsActive=1 ORDER BY UOMName;");
        }

        // ── RAW MATERIALS (read-only) ──────────────────────────────────────────
        public static DataTable GetActiveRawMaterials()
        {
            return ExecuteQuery(
                "SELECT r.RMID, r.RMCode, r.RMName, r.HSNCode, r.GSTRate, " +
                "u.Abbreviation, u.UOMName " +
                "FROM MM_RawMaterials r JOIN MM_UOM u ON u.UOMID=r.UOMID " +
                "WHERE r.IsActive=1 ORDER BY r.RMName;");
        }

        // ── PACKING MATERIALS (read-only) ─────────────────────────────────────
        public static DataTable GetActivePackingMaterials()
        {
            return ExecuteQuery(
                "SELECT p.PMID, p.PMCode, p.PMName, p.HSNCode, p.GSTRate, " +
                "u.Abbreviation, u.UOMName " +
                "FROM MM_PackingMaterials p JOIN MM_UOM u ON u.UOMID=p.UOMID " +
                "WHERE p.IsActive=1 ORDER BY p.PMName;");
        }

        // ── CONSUMABLES (read-only) ───────────────────────────────────────────
        public static DataTable GetActiveConsumables()
        {
            return ExecuteQuery(
                "SELECT c.ConsumableID, c.ConsumableCode, c.ConsumableName, " +
                "u.Abbreviation, u.UOMName " +
                "FROM MM_Consumables c JOIN MM_UOM u ON u.UOMID=c.UOMID " +
                "WHERE c.IsActive=1 ORDER BY c.ConsumableName;");
        }

        // ── PRODUCT MODEL ─────────────────────────────────────────────────────
        public static DataTable GetAllProducts()
        {
            return ExecuteQuery(
                "SELECT p.ProductID, p.ProductCode, p.ProductName, p.Description, p.ProductType, p.ImagePath, " +
                "p.BatchSize, p.ProdUOMID, p.OutputUOMID, p.HSNCode, p.GSTRate, p.IsActive, " +
                "pu.Abbreviation AS ProdAbbreviation, ou.Abbreviation AS OutputAbbreviation " +
                "FROM PP_Products p " +
                "JOIN MM_UOM pu ON pu.UOMID=p.ProdUOMID " +
                "JOIN MM_UOM ou ON ou.UOMID=p.OutputUOMID " +
                "ORDER BY p.ProductName;");
        }

        public static DataTable GetActiveProducts()
        {
            return ExecuteQuery(
                "SELECT p.ProductID, p.ProductCode, p.ProductName, p.BatchSize, " +
                "pu.Abbreviation AS ProdAbbreviation, ou.Abbreviation AS OutputAbbreviation " +
                "FROM PP_Products p " +
                "JOIN MM_UOM pu ON pu.UOMID=p.ProdUOMID " +
                "JOIN MM_UOM ou ON ou.UOMID=p.OutputUOMID " +
                "WHERE p.IsActive=1 ORDER BY p.ProductName;");
        }

        public static DataRow GetProductById(int productId)
        {
            return ExecuteQueryRow(
                "SELECT p.*, pu.Abbreviation AS ProdAbbreviation, pu.UOMName AS ProdUOMName, " +
                "ou.Abbreviation AS OutputAbbreviation, ou.UOMName AS OutputUOMName FROM PP_Products p " +
                "JOIN MM_UOM pu ON pu.UOMID=p.ProdUOMID " +
                "JOIN MM_UOM ou ON ou.UOMID=p.OutputUOMID " +
                "WHERE p.ProductID=?id;",
                new MySqlParameter("?id", productId));
        }

        public static string GenerateProductCode()
        {
            object val = ExecuteScalar(
                "SELECT MAX(CAST(SUBSTRING(ProductCode,4) AS SIGNED)) " +
                "FROM PP_Products WHERE ProductCode LIKE 'FG-%';");
            int next = (val == null || val == DBNull.Value) ? 1 : Convert.ToInt32(val) + 1;
            return "FG-" + next.ToString("D4");
        }

        public static int AddProduct(string name, string description, string hsnCode,
            decimal? gstRate, int prodUomId, int outputUomId, decimal batchSize, bool isActive,
            string productType = "Core", string imagePath = null)
        {
            string code = GenerateProductCode();
            using (var conn = OpenConnection())
            using (var cmd  = new MySqlCommand(
                "INSERT INTO PP_Products (ProductCode, ProductName, Description, HSNCode, GSTRate, ProdUOMID, OutputUOMID, BatchSize, IsActive, ProductType, ImagePath) " +
                "VALUES (?code,?name,?desc,?hsn,?gst,?produom,?outuom,?batch,?active,?type,?img);", conn))
            {
                cmd.Parameters.AddWithValue("?code",    code);
                cmd.Parameters.AddWithValue("?name",    name);
                cmd.Parameters.AddWithValue("?desc",    (object)description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("?hsn",     (object)hsnCode     ?? DBNull.Value);
                cmd.Parameters.AddWithValue("?gst",     (object)gstRate     ?? DBNull.Value);
                cmd.Parameters.AddWithValue("?produom", prodUomId);
                cmd.Parameters.AddWithValue("?outuom",  outputUomId);
                cmd.Parameters.AddWithValue("?batch",   batchSize);
                cmd.Parameters.AddWithValue("?active",  isActive ? 1 : 0);
                cmd.Parameters.AddWithValue("?type",    productType ?? "Core");
                cmd.Parameters.AddWithValue("?img",     (object)imagePath ?? DBNull.Value);
                cmd.ExecuteNonQuery();
                using (var idCmd = new MySqlCommand("SELECT LAST_INSERT_ID();", conn))
                    return Convert.ToInt32(idCmd.ExecuteScalar());
            }
        }

        public static void UpdateProduct(int productId, string code, string name,
            string description, string hsnCode, decimal? gstRate,
            int prodUomId, int outputUomId, decimal batchSize, bool isActive,
            string productType = "Core", string imagePath = null)
        {
            ExecuteNonQuery(
                "UPDATE PP_Products SET ProductCode=?code, ProductName=?name, Description=?desc, " +
                "HSNCode=?hsn, GSTRate=?gst, ProdUOMID=?produom, OutputUOMID=?outuom, BatchSize=?batch, IsActive=?active, " +
                "ProductType=?type, ImagePath=?img " +
                "WHERE ProductID=?id;",
                new MySqlParameter("?code",    code),
                new MySqlParameter("?name",    name),
                new MySqlParameter("?desc",    (object)description ?? DBNull.Value),
                new MySqlParameter("?hsn",     (object)hsnCode     ?? DBNull.Value),
                new MySqlParameter("?gst",     (object)gstRate     ?? DBNull.Value),
                new MySqlParameter("?produom", prodUomId),
                new MySqlParameter("?outuom",  outputUomId),
                new MySqlParameter("?batch",   batchSize),
                new MySqlParameter("?active",  isActive ? 1 : 0),
                new MySqlParameter("?type",    productType ?? "Core"),
                new MySqlParameter("?img",     (object)imagePath ?? DBNull.Value),
                new MySqlParameter("?id",      productId));
        }

        public static void ToggleProductActive(int productId, bool active)
        {
            ExecuteNonQuery(
                "UPDATE PP_Products SET IsActive=?a WHERE ProductID=?id;",
                new MySqlParameter("?a",  active ? 1 : 0),
                new MySqlParameter("?id", productId));
        }

        // ── BILL OF MATERIALS ─────────────────────────────────────────────────
        public static DataTable GetBOMByProduct(int productId)
        {
            return ExecuteQuery(
                "SELECT b.BOMID, b.MaterialType, b.MaterialID, b.Quantity, b.UOMID, " +
                "u.Abbreviation, " +
                "CASE b.MaterialType " +
                "  WHEN 'RM' THEN r.RMName " +
                "  WHEN 'PM' THEN p.PMName " +
                "  WHEN 'CN' THEN c.ConsumableName " +
                "  ELSE '?' END AS MaterialName, " +
                "CASE b.MaterialType " +
                "  WHEN 'RM' THEN r.RMCode " +
                "  WHEN 'PM' THEN p.PMCode " +
                "  WHEN 'CN' THEN c.ConsumableCode " +
                "  ELSE '?' END AS MaterialCode, " +
                "CASE b.MaterialType " +
                "  WHEN 'RM' THEN " +
                "    IFNULL(( " +
                "      SELECT SUM(g.QtyActualReceived * g.Rate) / NULLIF(SUM(g.QtyActualReceived), 0) " +
                "      FROM (SELECT QtyActualReceived, Rate FROM MM_RawInward " +
                "            WHERE RMID=b.MaterialID AND QtyActualReceived>0 AND Rate>0 " +
                "            ORDER BY InwardDate DESC, InwardID DESC LIMIT 3) g " +
                "    ), IFNULL(os.Rate, 0)) " +
                "  WHEN 'PM' THEN " +
                "    IFNULL(( " +
                "      SELECT SUM(g.QtyActualReceived * g.Rate) / NULLIF(SUM(g.QtyActualReceived), 0) " +
                "      FROM (SELECT QtyActualReceived, Rate FROM MM_PackingInward " +
                "            WHERE PMID=b.MaterialID AND QtyActualReceived>0 AND Rate>0 " +
                "            ORDER BY InwardDate DESC, InwardID DESC LIMIT 3) g " +
                "    ), IFNULL(os.Rate, 0)) " +
                "  WHEN 'CN' THEN " +
                "    IFNULL(( " +
                "      SELECT SUM(g.QtyActualReceived * g.Rate) / NULLIF(SUM(g.QtyActualReceived), 0) " +
                "      FROM (SELECT QtyActualReceived, Rate FROM MM_ConsumableInward " +
                "            WHERE ConsumableID=b.MaterialID AND QtyActualReceived>0 AND Rate>0 " +
                "            ORDER BY InwardDate DESC, InwardID DESC LIMIT 3) g " +
                "    ), IFNULL(os.Rate, 0)) " +
                "  ELSE 0 END AS UnitRate, " +
                "CASE b.MaterialType " +
                "  WHEN 'RM' THEN " +
                "    CASE WHEN EXISTS(SELECT 1 FROM MM_RawInward " +
                "         WHERE RMID=b.MaterialID AND QtyActualReceived>0 AND Rate>0) THEN 1 " +
                "         WHEN os.Rate IS NOT NULL THEN 2 ELSE 0 END " +
                "  WHEN 'PM' THEN " +
                "    CASE WHEN EXISTS(SELECT 1 FROM MM_PackingInward " +
                "         WHERE PMID=b.MaterialID AND QtyActualReceived>0 AND Rate>0) THEN 1 " +
                "         WHEN os.Rate IS NOT NULL THEN 2 ELSE 0 END " +
                "  WHEN 'CN' THEN " +
                "    CASE WHEN EXISTS(SELECT 1 FROM MM_ConsumableInward " +
                "         WHERE ConsumableID=b.MaterialID AND QtyActualReceived>0 AND Rate>0) THEN 1 " +
                "         WHEN os.Rate IS NOT NULL THEN 2 ELSE 0 END " +
                "  ELSE 0 END AS HasRate, " +
                "CASE b.MaterialType " +
                "  WHEN 'RM' THEN (SELECT COUNT(*) FROM " +
                "    (SELECT 1 FROM MM_RawInward WHERE RMID=b.MaterialID AND QtyActualReceived>0 AND Rate>0 " +
                "     ORDER BY InwardDate DESC LIMIT 3) g) " +
                "  WHEN 'PM' THEN (SELECT COUNT(*) FROM " +
                "    (SELECT 1 FROM MM_PackingInward WHERE PMID=b.MaterialID AND QtyActualReceived>0 AND Rate>0 " +
                "     ORDER BY InwardDate DESC LIMIT 3) g) " +
                "  WHEN 'CN' THEN (SELECT COUNT(*) FROM " +
                "    (SELECT 1 FROM MM_ConsumableInward WHERE ConsumableID=b.MaterialID AND QtyActualReceived>0 AND Rate>0 " +
                "     ORDER BY InwardDate DESC LIMIT 3) g) " +
                "  ELSE 0 END AS GRNCount " +
                "FROM PP_BOM b " +
                "JOIN MM_UOM u ON u.UOMID=b.UOMID " +
                "LEFT JOIN MM_RawMaterials    r ON b.MaterialType='RM' AND r.RMID=b.MaterialID " +
                "LEFT JOIN MM_PackingMaterials p ON b.MaterialType='PM' AND p.PMID=b.MaterialID " +
                "LEFT JOIN MM_Consumables      c ON b.MaterialType='CN' AND c.ConsumableID=b.MaterialID " +
                "LEFT JOIN MM_OpeningStock     os ON os.MaterialType=b.MaterialType AND os.MaterialID=b.MaterialID " +
                "WHERE b.ProductID=?pid ORDER BY b.MaterialType, MaterialName;",
                new MySqlParameter("?pid", productId));
        }

        public static void AddBOMLine(int productId, string materialType, int materialId,
            decimal quantity, int uomId)
        {
            ExecuteNonQuery(
                "INSERT INTO PP_BOM (ProductID, MaterialType, MaterialID, Quantity, UOMID) " +
                "VALUES (?pid,?type,?mid,?qty,?uom);",
                new MySqlParameter("?pid",  productId),
                new MySqlParameter("?type", materialType),
                new MySqlParameter("?mid",  materialId),
                new MySqlParameter("?qty",  quantity),
                new MySqlParameter("?uom",  uomId));
        }

        public static void DeleteBOMLine(int bomId)
        {
            ExecuteNonQuery("DELETE FROM PP_BOM WHERE BOMID=?id;",
                new MySqlParameter("?id", bomId));
        }

        public static void DeleteBOMByProduct(int productId)
        {
            ExecuteNonQuery("DELETE FROM PP_BOM WHERE ProductID=?pid;",
                new MySqlParameter("?pid", productId));
        }

        // ── PRODUCTION PLAN ───────────────────────────────────────────────────
        public static DataTable GetProductionPlans(DateTime from, DateTime to)
        {
            return ExecuteQuery(
                "SELECT pl.PlanID, pl.PlanNo, pl.PlanDate, pl.PlanMonth, pl.PlanYear, " +
                "pl.ProductID, pr.ProductName, pr.ProductCode, pl.PlannedQty, " +
                "pl.Status, pl.Remarks, pl.CreatedAt " +
                "FROM PP_ProductionPlan pl " +
                "JOIN PP_Products pr ON pr.ProductID=pl.ProductID " +
                "WHERE pl.PlanDate BETWEEN ?f AND ?t " +
                "ORDER BY pl.PlanDate DESC;",
                new MySqlParameter("?f", from),
                new MySqlParameter("?t", to));
        }

        public static DataRow GetProductionPlanById(int planId)
        {
            return ExecuteQueryRow(
                "SELECT pl.*, pr.ProductName, pr.ProductCode FROM PP_ProductionPlan pl " +
                "JOIN PP_Products pr ON pr.ProductID=pl.ProductID " +
                "WHERE pl.PlanID=?id;",
                new MySqlParameter("?id", planId));
        }

        public static string GeneratePlanNo()
        {
            object val = ExecuteScalar(
                "SELECT MAX(CAST(SUBSTRING(PlanNo,5) AS SIGNED)) FROM PP_ProductionPlan WHERE PlanNo LIKE 'PLN-%';");
            int next = (val == null || val == DBNull.Value) ? 1 : Convert.ToInt32(val) + 1;
            return "PLN-" + next.ToString("D5");
        }

        public static int AddProductionPlan(DateTime planDate, int planMonth, int planYear,
            int productId, decimal plannedQty, string remarks, int createdBy)
        {
            string planNo = GeneratePlanNo();
            using (var conn = OpenConnection())
            using (var cmd  = new MySqlCommand(
                "INSERT INTO PP_ProductionPlan " +
                "(PlanNo, PlanDate, PlanMonth, PlanYear, ProductID, PlannedQty, Status, Remarks, CreatedBy) " +
                "VALUES (?no,?dt,?mon,?yr,?pid,?qty,'Planned',?rem,?by);", conn))
            {
                cmd.Parameters.AddWithValue("?no",  planNo);
                cmd.Parameters.AddWithValue("?dt",  planDate);
                cmd.Parameters.AddWithValue("?mon", planMonth);
                cmd.Parameters.AddWithValue("?yr",  planYear);
                cmd.Parameters.AddWithValue("?pid", productId);
                cmd.Parameters.AddWithValue("?qty", plannedQty);
                cmd.Parameters.AddWithValue("?rem", (object)remarks ?? DBNull.Value);
                cmd.Parameters.AddWithValue("?by",  createdBy);
                cmd.ExecuteNonQuery();
                using (var idCmd = new MySqlCommand("SELECT LAST_INSERT_ID();", conn))
                    return Convert.ToInt32(idCmd.ExecuteScalar());
            }
        }

        public static void UpdateProductionPlanStatus(int planId, string status)
        {
            ExecuteNonQuery(
                "UPDATE PP_ProductionPlan SET Status=?s WHERE PlanID=?id;",
                new MySqlParameter("?s",  status),
                new MySqlParameter("?id", planId));
        }

        // ── PRODUCTION ORDER ──────────────────────────────────────────────────
        public static DataTable GetProductionOrders(DateTime from, DateTime to)
        {
            return ExecuteQuery(
                "SELECT po.OrderID, po.OrderNo, po.OrderDate, po.ProductID, " +
                "pr.ProductName, pr.ProductCode, po.OrderQty, po.TargetDate, " +
                "po.Status, po.Remarks, po.CreatedAt " +
                "FROM PP_ProductionOrder po " +
                "JOIN PP_Products pr ON pr.ProductID=po.ProductID " +
                "WHERE po.OrderDate BETWEEN ?f AND ?t " +
                "ORDER BY po.OrderDate DESC;",
                new MySqlParameter("?f", from),
                new MySqlParameter("?t", to));
        }

        // ── CONVERSION PRODUCT ────────────────────────────────────────────────────

        public static DataRow GetRMByName(string productName)
        {
            return ExecuteQueryRow(
                "SELECT RMID, RMName, UOMID FROM MM_RawMaterials " +
                "WHERE LOWER(TRIM(RMName)) = LOWER(TRIM(?name)) AND IsActive = 1 LIMIT 1;",
                new MySqlParameter("?name", productName));
        }

        public static void AddInternalGRN(int rmId, decimal qty, string productName, int orderNo, int batchNo, int userId)
        {
            // Get or create the Internal Production supplier
            object supObj = ExecuteScalar(
                "SELECT SupplierID FROM MM_Suppliers WHERE SupplierCode = 'INT-PROD' LIMIT 1;");
            if (supObj == null || supObj == DBNull.Value)
                throw new Exception("Internal Production supplier not found. Please run: " +
                    "INSERT INTO MM_Suppliers (SupplierCode, SupplierName, ContactPerson, Phone, Email, " +
                    "Address, City, State, PinCode, GSTIN, IsActive, CreatedAt) " +
                    "VALUES ('INT-PROD','Internal Production','System','','','','','','','',1,NOW());");
            int supplierId = Convert.ToInt32(supObj);

            string grnNo   = "INT-" + NowIST().ToString("yyyyMMdd") + "-" + rmId + "-" + batchNo;
            string remarks = "Internal production: " + productName + " | Order #" + orderNo + " Batch #" + batchNo;
            ExecuteNonQuery(
                "INSERT INTO MM_RawInward " +
                "(GRNNo, InwardDate, InvoiceNo, InvoiceDate, SupplierID, RMID," +
                " Quantity, QtyActualReceived, QtyInUOM, Rate, Amount," +
                " HSNCode, GSTRate, GSTAmount, TransportCost, TransportInInvoice, TransportInGST," +
                " ShortageQty, ShortageValue, PONo, Remarks, QualityCheck, Status, CreatedBy, CreatedAt)" +
                " VALUES (?grn,?dt,'INTERNAL',NULL,?sup,?rmid," +
                " ?qty,?qty,?qty,0,0," +
                " NULL,NULL,0,0,0,0," +
                " 0,0,NULL,?rem,1,'Approved',?by,NOW());",
                new MySqlParameter("?grn",  grnNo),
                new MySqlParameter("?dt",   NowIST().Date),
                new MySqlParameter("?sup",  supplierId),
                new MySqlParameter("?rmid", rmId),
                new MySqlParameter("?qty",  qty),
                new MySqlParameter("?rem",  remarks),
                new MySqlParameter("?by",   userId));
        }

        public static DataRow GetProductionOrderById(int orderId)
        {
            return ExecuteQueryRow(
                "SELECT o.OrderID, o.ProductID, o.Shift, o.Status, " +
                "IFNULL(o.RevisedBatches, o.OrderedBatches) AS EffectiveBatches, " +
                "p.ProductName, p.ProductCode, p.BatchSize, p.ProductType, " +
                "ou.Abbreviation AS OutputAbbr, pu.Abbreviation AS ProdAbbr " +
                "FROM PP_ProductionOrder o " +
                "JOIN PP_Products p  ON p.ProductID = o.ProductID " +
                "JOIN MM_UOM ou ON ou.UOMID = p.OutputUOMID " +
                "JOIN MM_UOM pu ON pu.UOMID = p.ProdUOMID " +
                "WHERE o.OrderID = ?oid;",
                new MySqlParameter("?oid", orderId));
        }

        public static string GenerateOrderNo()
        {
            object val = ExecuteScalar(
                "SELECT MAX(CAST(SUBSTRING(OrderNo,5) AS SIGNED)) FROM PP_ProductionOrder WHERE OrderNo LIKE 'PRO-%';");
            int next = (val == null || val == DBNull.Value) ? 1 : Convert.ToInt32(val) + 1;
            return "PRO-" + next.ToString("D5");
        }

        public static int AddProductionOrder(DateTime orderDate, int productId,
            decimal orderQty, DateTime targetDate, string remarks, int createdBy)
        {
            string orderNo = GenerateOrderNo();
            using (var conn = OpenConnection())
            using (var cmd  = new MySqlCommand(
                "INSERT INTO PP_ProductionOrder " +
                "(OrderNo, OrderDate, ProductID, OrderQty, TargetDate, Status, Remarks, CreatedBy) " +
                "VALUES (?no,?dt,?pid,?qty,?tgt,'Open',?rem,?by);", conn))
            {
                cmd.Parameters.AddWithValue("?no",  orderNo);
                cmd.Parameters.AddWithValue("?dt",  orderDate);
                cmd.Parameters.AddWithValue("?pid", productId);
                cmd.Parameters.AddWithValue("?qty", orderQty);
                cmd.Parameters.AddWithValue("?tgt", targetDate);
                cmd.Parameters.AddWithValue("?rem", (object)remarks ?? DBNull.Value);
                cmd.Parameters.AddWithValue("?by",  createdBy);
                cmd.ExecuteNonQuery();
                using (var idCmd = new MySqlCommand("SELECT LAST_INSERT_ID();", conn))
                    return Convert.ToInt32(idCmd.ExecuteScalar());
            }
        }

        public static void UpdateProductionOrderStatus(int orderId, string status)
        {
            ExecuteNonQuery(
                "UPDATE PP_ProductionOrder SET Status=?s WHERE OrderID=?id;",
                new MySqlParameter("?s",  status),
                new MySqlParameter("?id", orderId));
        }

        // ── MATERIAL STOCK POSITION ───────────────────────────────────────────
        public static DataTable GetRawMaterialStockPosition()
        {
            return ExecuteQuery(
                "SELECT r.RMCode, r.RMName, u.Abbreviation, " +
                "COALESCE(SUM(i.QtyActualReceived), 0) AS TotalReceived " +
                "FROM MM_RawMaterials r " +
                "JOIN MM_UOM u ON u.UOMID=r.UOMID " +
                "LEFT JOIN MM_RawInward i ON i.RMID=r.RMID " +
                "WHERE r.IsActive=1 " +
                "GROUP BY r.RMID, r.RMCode, r.RMName, u.Abbreviation " +
                "ORDER BY r.RMName;");
        }

        public static DataTable GetPackingMaterialStockPosition()
        {
            return ExecuteQuery(
                "SELECT p.PMCode, p.PMName, u.Abbreviation, " +
                "COALESCE(SUM(i.QtyActualReceived), 0) AS TotalReceived " +
                "FROM MM_PackingMaterials p " +
                "JOIN MM_UOM u ON u.UOMID=p.UOMID " +
                "LEFT JOIN MM_PackingInward i ON i.PMID=p.PMID " +
                "WHERE p.IsActive=1 " +
                "GROUP BY p.PMID, p.PMCode, p.PMName, u.Abbreviation " +
                "ORDER BY p.PMName;");
        }

        public static DataTable GetConsumableStockPosition()
        {
            return ExecuteQuery(
                "SELECT c.ConsumableCode, c.ConsumableName, u.Abbreviation, " +
                "COALESCE(SUM(i.QtyActualReceived), 0) AS TotalReceived " +
                "FROM MM_Consumables c " +
                "JOIN MM_UOM u ON u.UOMID=c.UOMID " +
                "LEFT JOIN MM_ConsumableInward i ON i.ConsumableID=c.ConsumableID " +
                "WHERE c.IsActive=1 " +
                "GROUP BY c.ConsumableID, c.ConsumableCode, c.ConsumableName, u.Abbreviation " +
                "ORDER BY c.ConsumableName;");
        }

        // ── DAILY PRODUCTION PLAN ─────────────────────────────────────────────

        // Get or create the plan record for a given date; returns PlanID
        public static int GetOrCreateDailyPlan(DateTime planDate, int userId)
        {
            var existing = ExecuteQueryRow(
                "SELECT PlanID FROM PP_DailyPlan WHERE PlanDate=?dt;",
                new MySqlParameter("?dt", planDate.Date));
            if (existing != null)
                return Convert.ToInt32(existing["PlanID"]);

            ExecuteNonQuery(
                "INSERT INTO PP_DailyPlan (PlanDate, Status, CreatedBy) VALUES (?dt,'Draft',?by);",
                new MySqlParameter("?dt", planDate.Date),
                new MySqlParameter("?by", userId));
            return Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
        }

        public static DataRow GetDailyPlan(DateTime planDate)
        {
            return ExecuteQueryRow(
                "SELECT PlanID, PlanDate, Status FROM PP_DailyPlan WHERE PlanDate=?dt;",
                new MySqlParameter("?dt", planDate.Date));
        }

        public static void SetDailyPlanStatus(int planId, string status)
        {
            ExecuteNonQuery(
                "UPDATE PP_DailyPlan SET Status=?s WHERE PlanID=?id;",
                new MySqlParameter("?s",  status),
                new MySqlParameter("?id", planId));
        }

        // Get all rows for a plan, for both shifts
        public static DataTable GetDailyPlanRows(int planId)
        {
            return ExecuteQuery(
                "SELECT r.RowID, r.PlanID, r.Shift, r.ProductID, r.Batches, r.SortOrder, " +
                "p.ProductName, p.ProductCode, p.BatchSize, " +
                "ou.Abbreviation AS OutputAbbr, " +
                "pu.Abbreviation AS ProdAbbr " +
                "FROM PP_DailyPlanRow r " +
                "JOIN PP_Products p  ON p.ProductID=r.ProductID " +
                "JOIN MM_UOM ou ON ou.UOMID=p.OutputUOMID " +
                "JOIN MM_UOM pu ON pu.UOMID=p.ProdUOMID " +
                "WHERE r.PlanID=?pid " +
                "ORDER BY r.Shift, r.SortOrder, r.RowID;",
                new MySqlParameter("?pid", planId));
        }

        // Add a new row to a plan
        public static int AddDailyPlanRow(int planId, int shift, int productId, decimal batches)
        {
            // SortOrder = next available within this shift
            object maxOrder = ExecuteScalar(
                "SELECT IFNULL(MAX(SortOrder),0)+1 FROM PP_DailyPlanRow " +
                "WHERE PlanID=?pid AND Shift=?sh;",
                new MySqlParameter("?pid", planId),
                new MySqlParameter("?sh",  shift));
            int sortOrder = Convert.ToInt32(maxOrder);

            ExecuteNonQuery(
                "INSERT INTO PP_DailyPlanRow (PlanID, Shift, ProductID, Batches, SortOrder) " +
                "VALUES (?pid,?sh,?prod,?bat,?so);",
                new MySqlParameter("?pid",  planId),
                new MySqlParameter("?sh",   shift),
                new MySqlParameter("?prod", productId),
                new MySqlParameter("?bat",  batches),
                new MySqlParameter("?so",   sortOrder));
            return Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
        }

        // Update batches for an existing row
        public static void UpdateDailyPlanRowBatches(int rowId, decimal batches)
        {
            ExecuteNonQuery(
                "UPDATE PP_DailyPlanRow SET Batches=?bat WHERE RowID=?id;",
                new MySqlParameter("?bat", batches),
                new MySqlParameter("?id",  rowId));
        }

        // Delete a row
        // Returns the order status for a plan row, or null if no order exists
        public static string GetPlanRowOrderStatus(int rowId)
        {
            var row = ExecuteQueryRow(
                "SELECT Status FROM PP_ProductionOrder WHERE PlanRowID=?id LIMIT 1;",
                new MySqlParameter("?id", rowId));
            return row != null ? row["Status"].ToString() : null;
        }

        public static void DeleteDailyPlanRow(int rowId)
        {
            // Delete dependent production order only if still Pending
            ExecuteNonQuery(
                "DELETE FROM PP_ProductionOrder WHERE PlanRowID=?id AND Status='Pending';",
                new MySqlParameter("?id", rowId));
            // Delete the plan row
            ExecuteNonQuery(
                "DELETE FROM PP_DailyPlanRow WHERE RowID=?id;",
                new MySqlParameter("?id", rowId));
        }

        // RM Requirement vs Stock for a given plan
        // Required = SUM(BOM.Quantity * DailyPlanRow.Batches) across all shifts
        // Stock    = OpeningStock.Quantity + SUM(MM_RawInward.QtyActualReceived)
        // Shortfall = Required - Stock (negative means surplus)
        public static DataTable GetRMRequirementVsStock(int planId)
        {
            string uom =
                "CASE" +
                " WHEN LOWER(TRIM(ubom.Abbreviation)) = LOWER(TRIM(urm.Abbreviation)) THEN 1" +
                " WHEN LOWER(TRIM(ubom.Abbreviation)) IN ('g','gm','gram','grams','grm') AND LOWER(TRIM(urm.Abbreviation)) IN ('kg','kgs','kilo','kilogram','kilograms') THEN 0.001" +
                " WHEN LOWER(TRIM(ubom.Abbreviation)) IN ('mg','milligram','milligrams') AND LOWER(TRIM(urm.Abbreviation)) IN ('kg','kgs','kilo','kilogram','kilograms') THEN 0.000001" +
                " WHEN LOWER(TRIM(ubom.Abbreviation)) IN ('mg','milligram','milligrams') AND LOWER(TRIM(urm.Abbreviation)) IN ('g','gm','gram','grams','grm') THEN 0.001" +
                " WHEN LOWER(TRIM(ubom.Abbreviation)) IN ('kg','kgs','kilo','kilogram','kilograms') AND LOWER(TRIM(urm.Abbreviation)) IN ('g','gm','gram','grams','grm') THEN 1000" +
                " WHEN LOWER(TRIM(ubom.Abbreviation)) IN ('kg','kgs','kilo','kilogram','kilograms') AND LOWER(TRIM(urm.Abbreviation)) IN ('mg','milligram','milligrams') THEN 1000000" +
                " WHEN LOWER(TRIM(ubom.Abbreviation)) IN ('g','gm','gram','grams','grm') AND LOWER(TRIM(urm.Abbreviation)) IN ('mg','milligram','milligrams') THEN 1000" +
                " WHEN LOWER(TRIM(ubom.Abbreviation)) IN ('ml','millilitre','milliliter','millilitres','milliliters') AND LOWER(TRIM(urm.Abbreviation)) IN ('l','ltr','litre','liter','litres','liters') THEN 0.001" +
                " WHEN LOWER(TRIM(ubom.Abbreviation)) IN ('l','ltr','litre','liter','litres','liters') AND LOWER(TRIM(urm.Abbreviation)) IN ('ml','millilitre','milliliter','millilitres','milliliters') THEN 1000" +
                " ELSE 1 END";

            string sql =
                "SELECT r.RMCode, r.RMName, urm.Abbreviation," +
                " ROUND(SUM(b.Quantity * pr.Batches * (" + uom + ")), 4) AS Required," +
                " ROUND(IFNULL(os.Quantity,0) + IFNULL(grn.TotalGRN,0), 4) AS InStock," +
                " ROUND(SUM(b.Quantity * pr.Batches * (" + uom + ")) - (IFNULL(os.Quantity,0) + IFNULL(grn.TotalGRN,0)), 4) AS Shortfall" +
                " FROM PP_DailyPlanRow pr" +
                " JOIN PP_BOM b ON b.ProductID = pr.ProductID AND b.MaterialType = 'RM'" +
                " JOIN MM_UOM ubom ON ubom.UOMID = b.UOMID" +
                " JOIN MM_RawMaterials r ON r.RMID = b.MaterialID" +
                " JOIN MM_UOM urm ON urm.UOMID = r.UOMID" +
                " LEFT JOIN MM_OpeningStock os ON os.MaterialType = 'RM' AND os.MaterialID = r.RMID" +
                " LEFT JOIN (SELECT RMID, SUM(QtyActualReceived) AS TotalGRN FROM MM_RawInward GROUP BY RMID) grn ON grn.RMID = r.RMID" +
                " WHERE pr.PlanID = ?pid" +
                " GROUP BY r.RMID, r.RMCode, r.RMName, urm.Abbreviation, os.Quantity, grn.TotalGRN" +
                " ORDER BY r.RMName;";

            return ExecuteQuery(sql, new MySqlParameter("?pid", planId));
        }

        // Check RM stock availability for a specific production order
        // Required = BOM qty x EffectiveBatches, converted to RM native UOM
        // InStock  = OpeningStock + GRN received (in RM native UOM)
        // Returns only rows where Shortfall > 0.001
        public static DataTable CheckStockForOrder(int orderId)
        {
            string uom =
                "CASE" +
                " WHEN LOWER(TRIM(ubom.Abbreviation)) = LOWER(TRIM(urm.Abbreviation)) THEN 1" +
                " WHEN LOWER(TRIM(ubom.Abbreviation)) IN ('g','gm','gram','grams','grm') AND LOWER(TRIM(urm.Abbreviation)) IN ('kg','kgs','kilo','kilogram','kilograms') THEN 0.001" +
                " WHEN LOWER(TRIM(ubom.Abbreviation)) IN ('mg','milligram','milligrams') AND LOWER(TRIM(urm.Abbreviation)) IN ('kg','kgs','kilo','kilogram','kilograms') THEN 0.000001" +
                " WHEN LOWER(TRIM(ubom.Abbreviation)) IN ('mg','milligram','milligrams') AND LOWER(TRIM(urm.Abbreviation)) IN ('g','gm','gram','grams','grm') THEN 0.001" +
                " WHEN LOWER(TRIM(ubom.Abbreviation)) IN ('kg','kgs','kilo','kilogram','kilograms') AND LOWER(TRIM(urm.Abbreviation)) IN ('g','gm','gram','grams','grm') THEN 1000" +
                " WHEN LOWER(TRIM(ubom.Abbreviation)) IN ('kg','kgs','kilo','kilogram','kilograms') AND LOWER(TRIM(urm.Abbreviation)) IN ('mg','milligram','milligrams') THEN 1000000" +
                " WHEN LOWER(TRIM(ubom.Abbreviation)) IN ('g','gm','gram','grams','grm') AND LOWER(TRIM(urm.Abbreviation)) IN ('mg','milligram','milligrams') THEN 1000" +
                " WHEN LOWER(TRIM(ubom.Abbreviation)) IN ('ml','millilitre','milliliter','millilitres','milliliters') AND LOWER(TRIM(urm.Abbreviation)) IN ('l','ltr','litre','liter','litres','liters') THEN 0.001" +
                " WHEN LOWER(TRIM(ubom.Abbreviation)) IN ('l','ltr','litre','liter','litres','liters') AND LOWER(TRIM(ubom.Abbreviation)) IN ('ml','millilitre','milliliter','millilitres','milliliters') THEN 1000" +
                " ELSE 1 END";

            string sql =
                "SELECT r.RMID, r.RMCode, r.RMName, urm.Abbreviation AS RMUnit," +
                " ROUND(SUM(b.Quantity * IFNULL(o.RevisedBatches, o.OrderedBatches) * (" + uom + ")), 4) AS Required," +
                " ROUND(IFNULL(os.Quantity,0) + IFNULL(grn.TotalGRN,0), 4) AS InStock," +
                " ROUND(SUM(b.Quantity * IFNULL(o.RevisedBatches, o.OrderedBatches) * (" + uom + "))" +
                "   - (IFNULL(os.Quantity,0) + IFNULL(grn.TotalGRN,0)), 4) AS Shortfall" +
                " FROM PP_ProductionOrder o" +
                " JOIN PP_BOM b ON b.ProductID = o.ProductID AND b.MaterialType = 'RM'" +
                " JOIN MM_UOM ubom ON ubom.UOMID = b.UOMID" +
                " JOIN MM_RawMaterials r ON r.RMID = b.MaterialID" +
                " JOIN MM_UOM urm ON urm.UOMID = r.UOMID" +
                " LEFT JOIN MM_OpeningStock os ON os.MaterialType = 'RM' AND os.MaterialID = r.RMID" +
                " LEFT JOIN (SELECT RMID, SUM(QtyActualReceived) AS TotalGRN FROM MM_RawInward GROUP BY RMID) grn ON grn.RMID = r.RMID" +
                " WHERE o.OrderID = ?oid" +
                " GROUP BY r.RMID, r.RMCode, r.RMName, urm.Abbreviation, os.Quantity, grn.TotalGRN" +
                " HAVING Shortfall > 0.001" +
                " ORDER BY r.RMName;";

            return ExecuteQuery(sql, new MySqlParameter("?oid", orderId));
        }


        // ── PRODUCTION ORDER ──────────────────────────────────────────────────

        // Get or create order rows for today from the daily plan
        // Returns all order rows for the given plan + shift
        public static DataTable GetOrCreateProductionOrders(int planId, int shift, 
            DateTime orderDate, int userId)
        {
            // Create missing order rows from plan rows
            ExecuteNonQuery(
                "INSERT IGNORE INTO PP_ProductionOrder " +
                "(PlanID, PlanRowID, Shift, ProductID, OrderedBatches, Status, OrderDate, CreatedBy) " +
                "SELECT r.PlanID, r.RowID, r.Shift, r.ProductID, r.Batches, 'Pending', ?dt, ?by " +
                "FROM PP_DailyPlanRow r " +
                "WHERE r.PlanID = ?pid AND r.Shift = ?sh;",
                new MySqlParameter("?dt",  orderDate.Date),
                new MySqlParameter("?by",  userId),
                new MySqlParameter("?pid", planId),
                new MySqlParameter("?sh",  shift));

            return ExecuteQuery(
                "SELECT o.OrderID, o.PlanRowID, o.Shift, o.ProductID, " +
                "o.OrderedBatches, o.RevisedBatches, o.Status, o.InitiatedAt, " +
                "p.ProductName, p.ProductCode, p.BatchSize, " +
                "ou.Abbreviation AS OutputAbbr, pu.Abbreviation AS ProdAbbr, " +
                "IFNULL(o.RevisedBatches, o.OrderedBatches) AS EffectiveBatches, " +
                "IFNULL((SELECT COUNT(*) FROM PP_BatchExecution be " +
                "  WHERE be.OrderID=o.OrderID AND be.Status='Completed'),0) AS CompletedBatches " +
                "FROM PP_ProductionOrder o " +
                "JOIN PP_Products p  ON p.ProductID  = o.ProductID " +
                "JOIN MM_UOM ou ON ou.UOMID = p.OutputUOMID " +
                "JOIN MM_UOM pu ON pu.UOMID = p.ProdUOMID " +
                "WHERE o.PlanID = ?pid AND o.Shift = ?sh " +
                "ORDER BY o.OrderID;",
                new MySqlParameter("?pid", planId),
                new MySqlParameter("?sh",  shift));
        }

        public static DataRow GetProductionOrder(int orderId)
        {
            return ExecuteQueryRow(
                "SELECT o.*, p.ProductName, p.ProductCode, p.BatchSize, " +
                "ou.Abbreviation AS OutputAbbr " +
                "FROM PP_ProductionOrder o " +
                "JOIN PP_Products p  ON p.ProductID = o.ProductID " +
                "JOIN MM_UOM ou ON ou.UOMID = p.OutputUOMID " +
                "WHERE o.OrderID = ?id;",
                new MySqlParameter("?id", orderId));
        }

        // Update revised batches (only if not yet Initiated)
        public static bool UpdateRevisedBatches(int orderId, decimal revisedBatches)
        {
            ExecuteNonQuery(
                "UPDATE PP_ProductionOrder " +
                "SET RevisedBatches = ?rb " +
                "WHERE OrderID = ?id AND Status != 'Completed';",
                new MySqlParameter("?rb", revisedBatches),
                new MySqlParameter("?id", orderId));
            // Check if update took effect
            var row = ExecuteQueryRow(
                "SELECT Status FROM PP_ProductionOrder WHERE OrderID=?id;",
                new MySqlParameter("?id", orderId));
            return row != null && row["Status"].ToString() == "Pending";
        }

        // Stop a production order
        public static bool StopOrder(int orderId)
        {
            ExecuteNonQuery(
                "UPDATE PP_ProductionOrder SET Status='Stopped' " +
                "WHERE OrderID=?id AND Status IN ('Initiated','InProgress');",
                new MySqlParameter("?id", orderId));
            var row = ExecuteQueryRow("SELECT Status FROM PP_ProductionOrder WHERE OrderID=?id;",
                new MySqlParameter("?id", orderId));
            return row != null && row["Status"].ToString() == "Stopped";
        }

        // Resume a stopped order
        public static bool ResumeOrder(int orderId)
        {
            ExecuteNonQuery(
                "UPDATE PP_ProductionOrder SET Status='InProgress' " +
                "WHERE OrderID=?id AND Status='Stopped';",
                new MySqlParameter("?id", orderId));
            var row = ExecuteQueryRow("SELECT Status FROM PP_ProductionOrder WHERE OrderID=?id;",
                new MySqlParameter("?id", orderId));
            return row != null && row["Status"].ToString() == "InProgress";
        }

        // Initiate a production order
        public static bool InitiateOrder(int orderId)
        {
            ExecuteNonQuery(
                "UPDATE PP_ProductionOrder " +
                "SET Status = 'Initiated', InitiatedAt = ?now " +
                "WHERE OrderID = ?id AND Status = 'Pending';",
                new MySqlParameter("?now", NowIST()),
                new MySqlParameter("?id", orderId));
            // Verify status changed
            var row = ExecuteQueryRow(
                "SELECT Status FROM PP_ProductionOrder WHERE OrderID=?id;",
                new MySqlParameter("?id", orderId));
            return row != null && row["Status"].ToString() == "Initiated";
        }

        // Get progress for all initiated/in-progress orders for today
        public static DataTable GetTodayOrderProgress(DateTime orderDate)
        {
            return ExecuteQuery(
                "SELECT o.OrderID, o.Shift, o.Status, o.InitiatedAt, " +
                "o.ProductID, p.ProductName, p.ProductCode, p.BatchSize, " +
                "o.OrderedBatches, " +
                "IFNULL(o.RevisedBatches, o.OrderedBatches) AS EffectiveBatches, " +
                "ou.Abbreviation AS OutputAbbr, pu.Abbreviation AS ProdAbbr, " +
                "IFNULL((SELECT COUNT(*) FROM PP_BatchExecution be " +
                "  WHERE be.OrderID=o.OrderID AND be.Status='Completed'),0) AS CompletedBatches, " +
                "IFNULL((SELECT SUM(be.ActualOutput) FROM PP_BatchExecution be " +
                "  WHERE be.OrderID=o.OrderID AND be.Status='Completed'),0) AS ActualOutput, " +
                "IFNULL((SELECT be.BatchNo FROM PP_BatchExecution be " +
                "  WHERE be.OrderID=o.OrderID AND be.Status='InProgress' LIMIT 1),0) AS RunningBatchNo " +
                "FROM PP_ProductionOrder o " +
                "JOIN PP_Products p  ON p.ProductID = o.ProductID " +
                "JOIN MM_UOM ou ON ou.UOMID = p.OutputUOMID " +
                "JOIN MM_UOM pu ON pu.UOMID = p.ProdUOMID " +
                "WHERE o.OrderDate = ?dt " +
                "AND o.Status IN ('Initiated','InProgress','Completed','Stopped') " +
                "ORDER BY o.Shift, o.InitiatedAt;",
                new MySqlParameter("?dt", orderDate.Date));
        }

        // Get RM consumption estimate for an order (BOM x effective batches)
        public static DataTable GetOrderRMEstimate(int orderId)
        {
            return ExecuteQuery(
                "SELECT r.RMName, r.RMCode, " +
                "urm.Abbreviation AS RM_UOM, ubom.Abbreviation AS BOM_UOM, " +
                "b.Quantity * IFNULL(o.RevisedBatches, o.OrderedBatches) * " +
                "CASE " +
                "  WHEN LOWER(TRIM(ubom.Abbreviation)) = LOWER(TRIM(urm.Abbreviation)) THEN 1 " +
                "  WHEN LOWER(TRIM(ubom.Abbreviation)) IN ('g','gm','gram','grams') " +
                "       AND LOWER(TRIM(urm.Abbreviation)) IN ('kg','kgs','kilo','kilogram') THEN 0.001 " +
                "  WHEN LOWER(TRIM(ubom.Abbreviation)) IN ('mg','milligram') " +
                "       AND LOWER(TRIM(urm.Abbreviation)) IN ('kg','kgs','kilo','kilogram') THEN 0.000001 " +
                "  WHEN LOWER(TRIM(ubom.Abbreviation)) IN ('ml','millilitre','milliliter') " +
                "       AND LOWER(TRIM(urm.Abbreviation)) IN ('l','ltr','litre','liter') THEN 0.001 " +
                "  ELSE 1 " +
                "END AS EstimatedQty " +
                "FROM PP_ProductionOrder o " +
                "JOIN PP_BOM b ON b.ProductID = o.ProductID AND b.MaterialType = 'RM' " +
                "JOIN MM_RawMaterials r  ON r.RMID = b.MaterialID " +
                "JOIN MM_UOM urm ON urm.UOMID = r.UOMID " +
                "JOIN MM_UOM ubom ON ubom.UOMID = b.UOMID " +
                "WHERE o.OrderID = ?id " +
                "ORDER BY r.RMName;",
                new MySqlParameter("?id", orderId));
        }

        // ── BATCH EXECUTION ───────────────────────────────────────────────────

        // Get initiated orders for today for a given shift — for the product dropdown
        public static DataTable GetInitiatedOrdersForShift(int shift, DateTime orderDate)
        {
            return ExecuteQuery(
                "SELECT o.OrderID, o.ProductID, p.ProductName, p.ProductCode, " +
                "IFNULL(o.RevisedBatches, o.OrderedBatches) AS EffectiveBatches, " +
                "p.BatchSize, ou.Abbreviation AS OutputAbbr, o.Status " +
                "FROM PP_ProductionOrder o " +
                "JOIN PP_Products p  ON p.ProductID = o.ProductID " +
                "JOIN MM_UOM ou ON ou.UOMID = p.OutputUOMID " +
                "WHERE o.Shift = ?sh AND o.OrderDate = ?dt " +
                "AND o.Status IN ('Initiated','InProgress','Stopped') " +
                "ORDER BY p.ProductName;",
                new MySqlParameter("?sh", shift),
                new MySqlParameter("?dt", orderDate.Date));
        }

        // Get full execution state for an order
        public static DataTable GetBatchHistory(int orderId)
        {
            return ExecuteQuery(
                "SELECT ExecutionID, BatchNo, StartTime, EndTime, " +
                "ActualOutput, Remarks, Status " +
                "FROM PP_BatchExecution " +
                "WHERE OrderID = ?oid " +
                "ORDER BY BatchNo;",
                new MySqlParameter("?oid", orderId));
        }

        // Get the current in-progress batch for an order (if any)
        public static DataRow GetActiveBatch(int orderId)
        {
            return ExecuteQueryRow(
                "SELECT ExecutionID, BatchNo, StartTime, Status " +
                "FROM PP_BatchExecution " +
                "WHERE OrderID = ?oid AND Status = 'InProgress' " +
                "ORDER BY BatchNo DESC LIMIT 1;",
                new MySqlParameter("?oid", orderId));
        }

        // Get batch that has been ended but output not yet saved
        public static DataRow GetEndedBatch(int orderId)
        {
            return ExecuteQueryRow(
                "SELECT ExecutionID, BatchNo, StartTime, EndTime, Status " +
                "FROM PP_BatchExecution " +
                "WHERE OrderID = ?oid AND Status = 'Ended' " +
                "ORDER BY BatchNo DESC LIMIT 1;",
                new MySqlParameter("?oid", orderId));
        }

        // Start a new batch
        public static int StartBatch(int orderId, int batchNo, int userId)
        {
            // Check for duplicate before inserting — unique key (OrderID, BatchNo)
            var existing = ExecuteQueryRow(
                "SELECT ExecutionID FROM PP_BatchExecution " +
                "WHERE OrderID=?oid AND BatchNo=?bno;",
                new MySqlParameter("?oid", orderId),
                new MySqlParameter("?bno", batchNo));
            if (existing != null)
                return Convert.ToInt32(existing["ExecutionID"]); // already started

            ExecuteNonQuery(
                "INSERT INTO PP_BatchExecution " +
                "(OrderID, BatchNo, StartTime, Status, CreatedBy) " +
                "VALUES (?oid, ?bno, ?now, 'InProgress', ?by);",
                new MySqlParameter("?oid", orderId),
                new MySqlParameter("?bno", batchNo),
                new MySqlParameter("?now", NowIST()),
                new MySqlParameter("?by",  userId));
            return Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
        }

        // End a batch — marks EndTime, updates order to InProgress
        public static void EndBatch(int executionId, int orderId)
        {
            // Mark as 'Ended' — awaiting output entry. GetActiveBatch looks for 'InProgress' only.
            ExecuteNonQuery(
                "UPDATE PP_BatchExecution SET EndTime = ?now, Status = 'Ended' " +
                "WHERE ExecutionID = ?eid;",
                new MySqlParameter("?now", NowIST()),
                new MySqlParameter("?eid", executionId));
            // Update order status to InProgress immediately
            ExecuteNonQuery(
                "UPDATE PP_ProductionOrder SET Status = 'InProgress' " +
                "WHERE OrderID = ?oid AND Status = 'Initiated';",
                new MySqlParameter("?oid", orderId));
        }

        // Save actual output for a completed batch
        // ── FIFO STOCK DEDUCTION ──────────────────────────────────────────────────────
        // Called after each batch is saved. Deducts BOM qty × 1 batch from stock FIFO.
        // Returns list of shortfall messages if any RM has insufficient stock.
        // Throws if stock is insufficient — caller should catch and block save.
        public static List<string> DeductStockFIFO(int executionId, int orderId, int batchNo,
            int productId, int userId)
        {
            // Get BOM — RM lines only
            var bom = ExecuteQuery(
                "SELECT b.MaterialID AS RMID, b.Quantity AS BOMQty," +
                " ubom.Abbreviation AS BOMUnit, urm.Abbreviation AS RMUnit, urm.UOMID AS RMUomID" +
                " FROM PP_BOM b" +
                " JOIN MM_UOM ubom ON ubom.UOMID = b.UOMID" +
                " JOIN MM_RawMaterials r ON r.RMID = b.MaterialID" +
                " JOIN MM_UOM urm ON urm.UOMID = r.UOMID" +
                " WHERE b.ProductID = ?pid AND b.MaterialType = 'RM';",
                new MySqlParameter("?pid", productId));

            if (bom.Rows.Count == 0)
                return new List<string>();

            var shortfalls = new List<string>();

            foreach (System.Data.DataRow bomRow in bom.Rows)
            {
                int     rmId    = Convert.ToInt32(bomRow["RMID"]);
                decimal bomQty  = Convert.ToDecimal(bomRow["BOMQty"]);
                string  bomUnit = bomRow["BOMUnit"].ToString().Trim().ToLower();
                string  rmUnit  = bomRow["RMUnit"].ToString().Trim().ToLower();

                // Convert BOM qty to RM native UOM
                decimal needed = bomQty * GetUOMConversionFactor(bomUnit, rmUnit);

                // ── Pull stock sources in FIFO order ─────────────────────────
                // 1. Opening stock (treated as oldest)
                var openingRow = ExecuteQueryRow(
                    "SELECT OpeningStockID, Quantity FROM MM_OpeningStock" +
                    " WHERE MaterialType='RM' AND MaterialID=?rmid AND Quantity > 0;",
                    new MySqlParameter("?rmid", rmId));

                // 2. GRN rows oldest first (only those with remaining qty)
                var grnRows = ExecuteQuery(
                    "SELECT InwardID, QtyActualReceived FROM MM_RawInward" +
                    " WHERE RMID=?rmid AND QtyActualReceived > 0" +
                    " ORDER BY InwardDate ASC, InwardID ASC;",
                    new MySqlParameter("?rmid", rmId));

                // Calculate total available
                decimal available = 0;
                if (openingRow != null)
                    available += Convert.ToDecimal(openingRow["Quantity"]);
                foreach (System.Data.DataRow g in grnRows.Rows)
                    available += Convert.ToDecimal(g["QtyActualReceived"]);

                if (available < needed)
                {
                    // Get RM name for message
                    var rmRow = ExecuteQueryRow(
                        "SELECT RMName FROM MM_RawMaterials WHERE RMID=?id;",
                        new MySqlParameter("?id", rmId));
                    string rmName = rmRow != null ? rmRow["RMName"].ToString() : "RM#" + rmId;
                    shortfalls.Add(rmName + ": need " + needed.ToString("0.###") + " " + rmUnit +
                        ", available " + available.ToString("0.###") + " " + rmUnit);
                    continue; // collect all shortfalls before throwing
                }

                // ── FIFO deduction ───────────────────────────────────────────
                decimal remaining = needed;

                // First: deduct from Opening Stock
                if (openingRow != null && remaining > 0)
                {
                    decimal osQty   = Convert.ToDecimal(openingRow["Quantity"]);
                    int     osId    = Convert.ToInt32(openingRow["OpeningStockID"]);
                    decimal consume = Math.Min(osQty, remaining);

                    ExecuteNonQuery(
                        "UPDATE MM_OpeningStock SET Quantity = Quantity - ?q" +
                        " WHERE OpeningStockID = ?id;",
                        new MySqlParameter("?q",  consume),
                        new MySqlParameter("?id", osId));

                    InsertConsumption(executionId, orderId, batchNo, rmId,
                        "OPENING", osId, consume, userId);

                    remaining -= consume;
                }

                // Then: deduct from GRN rows oldest first
                foreach (System.Data.DataRow grn in grnRows.Rows)
                {
                    if (remaining <= 0) break;

                    decimal grnQty  = Convert.ToDecimal(grn["QtyActualReceived"]);
                    int     grnId   = Convert.ToInt32(grn["InwardID"]);
                    decimal consume = Math.Min(grnQty, remaining);

                    ExecuteNonQuery(
                        "UPDATE MM_RawInward SET QtyActualReceived = QtyActualReceived - ?q" +
                        " WHERE InwardID = ?id;",
                        new MySqlParameter("?q",  consume),
                        new MySqlParameter("?id", grnId));

                    InsertConsumption(executionId, orderId, batchNo, rmId,
                        "GRN", grnId, consume, userId);

                    remaining -= consume;
                }
            }

            if (shortfalls.Count > 0)
                throw new Exception("STOCK_SHORTFALL:" + string.Join("|", shortfalls));

            return shortfalls;
        }

        private static void InsertConsumption(int executionId, int orderId, int batchNo,
            int rmId, string sourceType, int sourceId, decimal qty, int userId)
        {
            ExecuteNonQuery(
                "INSERT INTO MM_StockConsumption" +
                " (ExecutionID, OrderID, BatchNo, RMID, SourceType, SourceID, QtyConsumed, ConsumedAt, CreatedBy)" +
                " VALUES (?eid, ?oid, ?bno, ?rmid, ?stype, ?sid, ?qty, ?now, ?by);",
                new MySqlParameter("?eid",   executionId),
                new MySqlParameter("?oid",   orderId),
                new MySqlParameter("?bno",   batchNo),
                new MySqlParameter("?rmid",  rmId),
                new MySqlParameter("?stype", sourceType),
                new MySqlParameter("?sid",   sourceId),
                new MySqlParameter("?qty",   qty),
                new MySqlParameter("?now",   NowIST()),
                new MySqlParameter("?by",    userId));
        }

        private static decimal GetUOMConversionFactor(string fromUnit, string toUnit)
        {
            if (fromUnit == toUnit) return 1m;
            string f = fromUnit.Trim().ToLower();
            string t = toUnit.Trim().ToLower();
            string[] kg  = {"kg","kgs","kilo","kilogram","kilograms"};
            string[] g   = {"g","gm","gram","grams","grm"};
            string[] mg  = {"mg","milligram","milligrams"};
            string[] l   = {"l","ltr","litre","liter","litres","liters"};
            string[] ml  = {"ml","millilitre","milliliter","millilitres","milliliters"};
            bool In(string v, string[] arr) { foreach (var a in arr) if (v==a) return true; return false; }
            if (In(f,g)  && In(t,kg))  return 0.001m;
            if (In(f,mg) && In(t,kg))  return 0.000001m;
            if (In(f,mg) && In(t,g))   return 0.001m;
            if (In(f,kg) && In(t,g))   return 1000m;
            if (In(f,kg) && In(t,mg))  return 1000000m;
            if (In(f,g)  && In(t,mg))  return 1000m;
            if (In(f,ml) && In(t,l))   return 0.001m;
            if (In(f,l)  && In(t,ml))  return 1000m;
            return 1m; // same family or unknown — no conversion
        }


        public static void SaveBatchOutput(int executionId, decimal actualOutput,
            string remarks, int orderId, int totalBatches)
        {
            ExecuteNonQuery(
                "UPDATE PP_BatchExecution " +
                "SET ActualOutput = ?ao, Remarks = ?rem, Status = 'Completed' " +
                "WHERE ExecutionID = ?eid AND Status = 'Ended';",
                new MySqlParameter("?ao",  actualOutput),
                new MySqlParameter("?rem", string.IsNullOrEmpty(remarks) ? (object)DBNull.Value : remarks),
                new MySqlParameter("?eid", executionId));

            // Check if all batches are done — auto-complete the order
            object completedCount = ExecuteScalar(
                "SELECT COUNT(*) FROM PP_BatchExecution " +
                "WHERE OrderID = ?oid AND Status = 'Completed';",
                new MySqlParameter("?oid", orderId));
            if (Convert.ToInt32(completedCount) >= totalBatches)
            {
                ExecuteNonQuery(
                    "UPDATE PP_ProductionOrder " +
                    "SET Status = 'Completed', CompletedAt = ?nowc " +
                    "WHERE OrderID = ?oid;",
                    new MySqlParameter("?nowc", NowIST()),
                    new MySqlParameter("?oid", orderId));
            }
        }
    }
}
