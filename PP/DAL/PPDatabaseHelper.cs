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
                "p.BatchSize, p.ProdUOMID, p.OutputUOMID, p.HSNCode, p.GSTRate, p.IsActive, p.ProductionLineID, p.UnitWeightGrams, " +
                "pu.Abbreviation AS ProdAbbreviation, ou.Abbreviation AS OutputAbbreviation, " +
                "pl.LineName AS ProductionLineName " +
                "FROM PP_Products p " +
                "JOIN MM_UOM pu ON pu.UOMID=p.ProdUOMID " +
                "JOIN MM_UOM ou ON ou.UOMID=p.OutputUOMID " +
                "LEFT JOIN PP_ProductionLines pl ON pl.LineID=p.ProductionLineID " +
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
                "WHERE p.IsActive=1 AND p.ProductType IN ('Core','Conversion') ORDER BY p.ProductName;");
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
            string productType = "Core", string imagePath = null, int? productionLineId = null, decimal? unitWeightGrams = null)
        {
            string code = GenerateProductCode();
            using (var conn = OpenConnection())
            using (var cmd  = new MySqlCommand(
                "INSERT INTO PP_Products (ProductCode, ProductName, Description, HSNCode, GSTRate, ProdUOMID, OutputUOMID, BatchSize, IsActive, ProductType, ImagePath, ProductionLineID, UnitWeightGrams) " +
                "VALUES (?code,?name,?desc,?hsn,?gst,?produom,?outuom,?batch,?active,?type,?img,?lineId,?uwg);", conn))
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
                cmd.Parameters.AddWithValue("?lineId",  productionLineId.HasValue ? (object)productionLineId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("?uwg",     unitWeightGrams.HasValue ? (object)unitWeightGrams.Value : DBNull.Value);
                cmd.ExecuteNonQuery();
                using (var idCmd = new MySqlCommand("SELECT LAST_INSERT_ID();", conn))
                    return Convert.ToInt32(idCmd.ExecuteScalar());
            }
        }

        public static void UpdateProduct(int productId, string code, string name,
            string description, string hsnCode, decimal? gstRate,
            int prodUomId, int outputUomId, decimal batchSize, bool isActive,
            string productType = "Core", string imagePath = null, int? productionLineId = null, decimal? unitWeightGrams = null)
        {
            ExecuteNonQuery(
                "UPDATE PP_Products SET ProductCode=?code, ProductName=?name, Description=?desc, " +
                "HSNCode=?hsn, GSTRate=?gst, ProdUOMID=?produom, OutputUOMID=?outuom, BatchSize=?batch, IsActive=?active, " +
                "ProductType=?type, ImagePath=?img, ProductionLineID=?lineId, UnitWeightGrams=?uwg " +
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
                new MySqlParameter("?lineId",  productionLineId.HasValue ? (object)productionLineId.Value : DBNull.Value),
                new MySqlParameter("?uwg",     unitWeightGrams.HasValue ? (object)unitWeightGrams.Value : DBNull.Value),
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

        // ── PACKING SPEC ─────────────────────────────────────────────────────────
        public static void SavePackingSpec(int productId, string containerType,
            string unitSizes, string containersPerCase, bool hasLanguageLabels = false)
        {
            int cpc = 0;
            int.TryParse(containersPerCase, out cpc);
            ExecuteNonQuery(
                "UPDATE PP_Products SET ContainerType=?ct, UnitsPerContainer=?us," +
                " ContainersPerCase=?cpc, HasLanguageLabels=?hll WHERE ProductID=?id;",
                new MySqlParameter("?ct",  string.IsNullOrEmpty(containerType) ? (object)DBNull.Value : containerType.Trim()),
                new MySqlParameter("?us",  string.IsNullOrEmpty(unitSizes)     ? (object)DBNull.Value : unitSizes.Trim()),
                new MySqlParameter("?cpc", cpc > 0 ? (object)cpc : DBNull.Value),
                new MySqlParameter("?hll", hasLanguageLabels ? 1 : 0),
                new MySqlParameter("?id",  productId));
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

            string grnNo   = "INT-" + NowIST().ToString("yyyyMMddHHmmss") + "-" + rmId + "-" + batchNo;
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
                "p.ProductionLineID, p.UnitWeightGrams, " +
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
            // Revert plan to Draft whenever a new row is added
            ExecuteNonQuery(
                "UPDATE PP_DailyPlan SET Status='Draft' WHERE PlanID=?pid;",
                new MySqlParameter("?pid", planId));
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
                " WHEN LOWER(TRIM(ubom.Abbreviation)) IN ('l','ltr','litre','liter','litres','liters') AND LOWER(TRIM(urm.Abbreviation)) IN ('ml','millilitre','milliliter','millilitres','milliliters') THEN 1000" +
                " ELSE 1 END";

            string sql =
                "SELECT r.RMID, r.RMCode, r.RMName, urm.Abbreviation AS RMUnit," +
                " ROUND(SUM(b.Quantity * IFNULL(o.RevisedBatches, o.OrderedBatches) * (" + uom + ")), 4) AS Required," +
                " ROUND(IFNULL(os.Quantity,0) + IFNULL(grn.TotalGRN,0) - IFNULL(con.TotalConsumed,0), 4) AS InStock," +
                " ROUND(SUM(b.Quantity * IFNULL(o.RevisedBatches, o.OrderedBatches) * (" + uom + "))" +
                "   - (IFNULL(os.Quantity,0) + IFNULL(grn.TotalGRN,0) - IFNULL(con.TotalConsumed,0)), 4) AS Shortfall" +
                " FROM PP_ProductionOrder o" +
                " JOIN PP_BOM b ON b.ProductID = o.ProductID AND b.MaterialType = 'RM'" +
                " JOIN MM_UOM ubom ON ubom.UOMID = b.UOMID" +
                " JOIN MM_RawMaterials r ON r.RMID = b.MaterialID" +
                " JOIN MM_UOM urm ON urm.UOMID = r.UOMID" +
                " LEFT JOIN MM_OpeningStock os ON os.MaterialType = 'RM' AND os.MaterialID = r.RMID" +
                " LEFT JOIN (SELECT RMID, SUM(QtyActualReceived) AS TotalGRN FROM MM_RawInward GROUP BY RMID) grn ON grn.RMID = r.RMID" +
                " LEFT JOIN (SELECT RMID, SUM(QtyConsumed) AS TotalConsumed FROM MM_StockConsumption GROUP BY RMID) con ON con.RMID = r.RMID" +
                " WHERE o.OrderID = ?oid" +
                " GROUP BY r.RMID, r.RMCode, r.RMName, urm.Abbreviation, os.Quantity, grn.TotalGRN, con.TotalConsumed" +
                " HAVING Shortfall > 0.001" +
                " ORDER BY r.RMName;";

            return ExecuteQuery(sql, new MySqlParameter("?oid", orderId));
        }


        // ── PREFILLED CONVERSION ─────────────────────────────────────────────────

        public static DataTable GetPrefilledConversionProducts()
        {
            return ExecuteQuery(
                "SELECT p.ProductID, p.ProductCode, p.ProductName, p.BatchSize," +
                " ou.Abbreviation AS OutputUnit" +
                " FROM PP_Products p" +
                " JOIN MM_UOM ou ON ou.UOMID = p.OutputUOMID" +
                " WHERE p.ProductType = 'Prefilled Conversion' AND p.IsActive = 1" +
                " ORDER BY p.ProductName;");
        }

        public static DataTable GetPrefilledEntriesToday(int productId)
        {
            return ExecuteQuery(
                "SELECT i.InwardID, i.GRNNo, i.QtyActualReceived AS Qty, i.CreatedAt," +
                " r.RMName, u.Abbreviation AS Unit" +
                " FROM MM_RawInward i" +
                " JOIN MM_RawMaterials r ON r.RMID = i.RMID" +
                " JOIN MM_UOM u ON u.UOMID = r.UOMID" +
                " WHERE DATE(i.InwardDate) = ?today" +
                " AND i.Remarks LIKE ?pat" +
                " ORDER BY i.InwardID DESC;",
                new MySqlParameter("?today", TodayIST()),
                new MySqlParameter("?pat",   "%ProductID=" + productId + "%"));
        }

        public static void AddPrefilledEntry(int rmId, decimal qty,
            string productName, int productId, int userId)
        {
            string grnNo   = "PRE-" + NowIST().ToString("yyyyMMddHHmmss") + "-" + rmId;
            string remarks = "Prefilled: Product=" + productName + " ProductID=" + productId;
            var supObj = ExecuteScalar(
                "SELECT SupplierID FROM MM_Suppliers WHERE SupplierCode='INT-PROD' LIMIT 1;");
            if (supObj == null || supObj == DBNull.Value)
                throw new Exception("Internal Production supplier (INT-PROD) not found.");
            int supplierId = Convert.ToInt32(supObj);
            ExecuteNonQuery(
                "INSERT INTO MM_RawInward" +
                " (GRNNo, InwardDate, InvoiceNo, InvoiceDate, SupplierID, RMID," +
                "  Quantity, QtyActualReceived, QtyInUOM, Rate, Amount," +
                "  HSNCode, GSTRate, GSTAmount, TransportCost, TransportInInvoice, TransportInGST," +
                "  ShortageQty, ShortageValue, PONo, Remarks, QualityCheck, Status, CreatedBy, CreatedAt)" +
                " VALUES (?grn,?dt,'PREFILLED',NULL,?sup,?rmid," +
                "  ?qty,?qty,?qty,0,0," +
                "  NULL,NULL,0,0,0,0," +
                "  0,0,NULL,?rem,1,'Approved',?by,NOW());",
                new MySqlParameter("?grn",  grnNo),
                new MySqlParameter("?dt",   NowIST().Date),
                new MySqlParameter("?sup",  supplierId),
                new MySqlParameter("?rmid", rmId),
                new MySqlParameter("?qty",  qty),
                new MySqlParameter("?rem",  remarks),
                new MySqlParameter("?by",   userId));
        }

        public static void RecordShiftConsumption(int rmId, decimal qty,
            string rmName, string productName, int userId)
        {
            ExecuteNonQuery(
                "INSERT INTO MM_StockConsumption" +
                " (ExecutionID, OrderID, BatchNo, RMID, SourceType, SourceID, QtyConsumed, ConsumedAt, CreatedBy)" +
                " VALUES (0, 0, 0, ?rmid, 'SHIFT', 0, ?qty, ?now, ?by);",
                new MySqlParameter("?rmid", rmId),
                new MySqlParameter("?qty",  qty),
                new MySqlParameter("?now",  NowIST()),
                new MySqlParameter("?by",   userId));
        }

        public static DataTable GetShiftConsumptionToday(int rmId)
        {
            return ExecuteQuery(
                "SELECT ConsumptionID, QtyConsumed, ConsumedAt" +
                " FROM MM_StockConsumption" +
                " WHERE RMID=?rmid AND SourceType='SHIFT' AND DATE(ConsumedAt)=?today" +
                " ORDER BY ConsumptionID DESC;",
                new MySqlParameter("?rmid",  rmId),
                new MySqlParameter("?today", TodayIST()));
        }

        // Get available stock for an RM (Opening + GRN - Consumed)
        public static decimal GetAvailableStock(int rmId)
        {
            var row = ExecuteQueryRow(
                "SELECT" +
                " ROUND(IFNULL(os.Quantity,0) + IFNULL(grn.TotalGRN,0) - IFNULL(con.TotalConsumed,0), 4) AS Available" +
                " FROM MM_RawMaterials r" +
                " LEFT JOIN MM_OpeningStock os ON os.MaterialType='RM' AND os.MaterialID=r.RMID" +
                " LEFT JOIN (SELECT RMID, SUM(QtyActualReceived) AS TotalGRN FROM MM_RawInward GROUP BY RMID) grn ON grn.RMID=r.RMID" +
                " LEFT JOIN (SELECT RMID, SUM(QtyConsumed) AS TotalConsumed FROM MM_StockConsumption GROUP BY RMID) con ON con.RMID=r.RMID" +
                " WHERE r.RMID=?rmid;",
                new MySqlParameter("?rmid", rmId));
            return row != null ? Convert.ToDecimal(row["Available"]) : 0m;
        }

        // Get scrap materials linked to a Raw Material (via MM_RMScrapLink)
        public static DataTable GetScrapMaterialsForRM(int rmId)
        {
            return ExecuteQuery(
                "SELECT l.LinkID, s.ScrapID, s.ScrapName, u.Abbreviation AS Unit" +
                " FROM MM_RMScrapLink l" +
                " JOIN MM_ScrapMaterials s ON s.ScrapID = l.ScrapID" +
                " JOIN MM_UOM u ON u.UOMID = s.UOMID" +
                " WHERE l.RMID = ?rmid AND s.IsActive = 1" +
                " ORDER BY s.ScrapName;",
                new MySqlParameter("?rmid", rmId));
        }

        // Record scrap qty generated during shift closure
        // Stored in MM_RawInward as a receipt (scrap is a sellable by-product)
        public static void RecordScrapGenerated(int scrapId, decimal qty,
            string scrapName, string rmName, int userId)
        {
            // Insert directly into MM_ScrapStock — dedicated table for scrap by-product tracking
            string remarks = "From: " + rmName;
            ExecuteNonQuery(
                "INSERT INTO MM_ScrapStock (ScrapID, QtyGenerated, GeneratedAt, SourceRMName, Remarks, CreatedBy)" +
                " VALUES (?scid, ?qty, ?now, ?rm, ?rem, ?by);",
                new MySqlParameter("?scid", scrapId),
                new MySqlParameter("?qty",  qty),
                new MySqlParameter("?now",  NowIST()),
                new MySqlParameter("?rm",   rmName),
                new MySqlParameter("?rem",  remarks),
                new MySqlParameter("?by",   userId));
        }

        // ── PRE PROCESSED RM ─────────────────────────────────────────────────────

        public static void SavePreprocessStages(int productId, string inputRMName,
            string stage1, string stage2, string stage3)
        {
            ExecuteNonQuery(
                "INSERT INTO PP_PreprocessStages (ProductID, InputRMName, Stage1Label, Stage2Label, Stage3Label)" +
                " VALUES (?pid,?rm,?s1,?s2,?s3)" +
                " ON DUPLICATE KEY UPDATE InputRMName=?rm, Stage1Label=?s1, Stage2Label=?s2, Stage3Label=?s3;",
                new MySqlParameter("?pid", productId),
                new MySqlParameter("?rm",  inputRMName),
                new MySqlParameter("?s1",  stage1),
                new MySqlParameter("?s2",  stage2),
                new MySqlParameter("?s3",  stage3));
        }

        public static DataRow GetPreprocessStages(int productId)
        {
            return ExecuteQueryRow(
                "SELECT * FROM PP_PreprocessStages WHERE ProductID=?pid;",
                new MySqlParameter("?pid", productId));
        }

        public static DataTable GetPreprocessProducts()
        {
            return ExecuteQuery(
                "SELECT p.ProductID, p.ProductCode, p.ProductName," +
                " ps.InputRMName, ps.Stage1Label, ps.Stage2Label, ps.Stage3Label," +
                " ou.Abbreviation AS OutputUnit" +
                " FROM PP_Products p" +
                " JOIN PP_PreprocessStages ps ON ps.ProductID = p.ProductID" +
                " JOIN MM_UOM ou ON ou.UOMID = p.OutputUOMID" +
                " WHERE p.ProductType = 'Pre processed RM' AND p.IsActive = 1" +
                " ORDER BY p.ProductName;");
        }

        // Add entry for a specific stage — returns new running total for stage
        // Stage 1: deducts from Input RM stock
        // Stage 2: tracking only
        // Stage 3: adds to Output RM stock (product name = RM name), deducts from Stage 2 log
        public static void AddPreprocessEntry(int productId, int stage, decimal qty,
            string productName, string inputRMName, string stageLabel, int userId)
        {
            string grnNo  = "PREP-" + NowIST().ToString("yyyyMMddHHmmss") + "-" + productId + "-S" + stage;
            string remarks = "Preprocess S" + stage + ": " + stageLabel + " | Product=" + productName;

            var supObj = ExecuteScalar(
                "SELECT SupplierID FROM MM_Suppliers WHERE SupplierCode='INT-PROD' LIMIT 1;");
            if (supObj == null || supObj == DBNull.Value)
                throw new Exception("Internal Production supplier (INT-PROD) not found.");
            int supplierId = Convert.ToInt32(supObj);

            if (stage == 1)
            {
                // Deduct from Input RM via MM_StockConsumption
                var rmRow = ExecuteQueryRow(
                    "SELECT RMID FROM MM_RawMaterials" +
                    " WHERE LOWER(TRIM(RMName))=LOWER(TRIM(?name)) AND IsActive=1 LIMIT 1;",
                    new MySqlParameter("?name", inputRMName));
                if (rmRow == null)
                    throw new Exception("Input RM '" + inputRMName + "' not found in Raw Materials.");
                int rmId = Convert.ToInt32(rmRow["RMID"]);

                // Check available stock
                decimal available = GetAvailableStock(rmId);
                if (qty > available)
                    throw new Exception("STOCK_SHORTFALL:" + inputRMName +
                        ": need " + qty.ToString("0.###") +
                        ", available " + available.ToString("0.###"));

                // Record as consumption (FIFO deduction)
                ExecuteNonQuery(
                    "INSERT INTO MM_StockConsumption" +
                    " (ExecutionID, OrderID, BatchNo, RMID, SourceType, SourceID, QtyConsumed, ConsumedAt, CreatedBy)" +
                    " VALUES (0, ?pid, ?stage, ?rmid, 'PREPROCESS', 0, ?qty, ?now, ?by);",
                    new MySqlParameter("?pid",   productId),
                    new MySqlParameter("?stage", stage),
                    new MySqlParameter("?rmid",  rmId),
                    new MySqlParameter("?qty",   qty),
                    new MySqlParameter("?now",   NowIST()),
                    new MySqlParameter("?by",    userId));
            }
            else if (stage == 2)
            {
                // Add to Stage 2 RM stock (stageLabel = RM name, e.g. "Roasted Peanuts")
                var rmRow2 = ExecuteQueryRow(
                    "SELECT RMID FROM MM_RawMaterials" +
                    " WHERE LOWER(TRIM(RMName))=LOWER(TRIM(?name)) AND IsActive=1 LIMIT 1;",
                    new MySqlParameter("?name", stageLabel));
                if (rmRow2 == null)
                    throw new Exception("Stage 2 RM '" + stageLabel + "' not found in Raw Materials.");
                int rmId2 = Convert.ToInt32(rmRow2["RMID"]);

                // Add to stock as internal GRN
                ExecuteNonQuery(
                    "INSERT INTO MM_RawInward" +
                    " (GRNNo, InwardDate, InvoiceNo, InvoiceDate, SupplierID, RMID," +
                    "  Quantity, QtyActualReceived, QtyInUOM, Rate, Amount," +
                    "  HSNCode, GSTRate, GSTAmount, TransportCost, TransportInInvoice, TransportInGST," +
                    "  ShortageQty, ShortageValue, PONo, Remarks, QualityCheck, Status, CreatedBy, CreatedAt)" +
                    " VALUES (?grn,?dt,'PREPROCESS',NULL,?sup,?rmid," +
                    "  ?qty,?qty,?qty,0,0,NULL,NULL,0,0,0,0,0,0,NULL,?rem,1,'Approved',?by,NOW());",
                    new MySqlParameter("?grn",  grnNo),
                    new MySqlParameter("?dt",   NowIST().Date),
                    new MySqlParameter("?sup",  supplierId),
                    new MySqlParameter("?rmid", rmId2),
                    new MySqlParameter("?qty",  qty),
                    new MySqlParameter("?rem",  remarks),
                    new MySqlParameter("?by",   userId));

                // Log for stage tracking
                ExecuteNonQuery(
                    "INSERT INTO PP_PreprocessLog (ProductID, Stage, Qty, Remarks, CreatedAt, CreatedBy)" +
                    " VALUES (?pid, ?stage, ?qty, ?rem, ?now, ?by);",
                    new MySqlParameter("?pid",   productId),
                    new MySqlParameter("?stage", stage),
                    new MySqlParameter("?qty",   qty),
                    new MySqlParameter("?rem",   remarks),
                    new MySqlParameter("?now",   NowIST()),
                    new MySqlParameter("?by",    userId));
            }
            else if (stage == 3)
            {
                // Add to Output RM stock — Stage 3 label name = Output RM name
                // e.g. "Sorted Roasted Peanuts" is the RM that goes into BOM
                var rmRow = ExecuteQueryRow(
                    "SELECT RMID FROM MM_RawMaterials" +
                    " WHERE LOWER(TRIM(RMName))=LOWER(TRIM(?name)) AND IsActive=1 LIMIT 1;",
                    new MySqlParameter("?name", stageLabel));
                if (rmRow == null)
                    throw new Exception("Output RM '" + stageLabel + "' not found in Raw Materials. " +
                        "Ensure an RM named '" + stageLabel + "' exists in MM.");
                int rmId = Convert.ToInt32(rmRow["RMID"]);

                // Check Stage 2 RM available stock
                decimal available = GetAvailableStock(rmId);
                // Look up Stage 2 label from PP_PreprocessStages to find Stage 2 RM
                var stagesRow = ExecuteQueryRow(
                    "SELECT Stage2Label FROM PP_PreprocessStages WHERE ProductID=?pid;",
                    new MySqlParameter("?pid", productId));
                string stage2Label = stagesRow != null ? stagesRow["Stage2Label"].ToString() : "";
                var s2Rm = ExecuteQueryRow(
                    "SELECT RMID FROM MM_RawMaterials" +
                    " WHERE LOWER(TRIM(RMName))=LOWER(TRIM(?name)) AND IsActive=1 LIMIT 1;",
                    new MySqlParameter("?name", stage2Label));
                if (s2Rm == null)
                    throw new Exception("Stage 2 RM '" + stage2Label + "' not found.");
                int s2RmId = Convert.ToInt32(s2Rm["RMID"]);
                decimal s2Available = GetAvailableStock(s2RmId);
                if (qty > s2Available + 0.001m)
                    throw new Exception("STOCK_SHORTFALL:Roasted stock available: " +
                        s2Available.ToString("0.###") + ", requested: " + qty.ToString("0.###"));

                // Add to Output RM stock as internal GRN
                ExecuteNonQuery(
                    "INSERT INTO MM_RawInward" +
                    " (GRNNo, InwardDate, InvoiceNo, InvoiceDate, SupplierID, RMID," +
                    "  Quantity, QtyActualReceived, QtyInUOM, Rate, Amount," +
                    "  HSNCode, GSTRate, GSTAmount, TransportCost, TransportInInvoice, TransportInGST," +
                    "  ShortageQty, ShortageValue, PONo, Remarks, QualityCheck, Status, CreatedBy, CreatedAt)" +
                    " VALUES (?grn,?dt,'PREPROCESS',NULL,?sup,?rmid," +
                    "  ?qty,?qty,?qty,0,0,NULL,NULL,0,0,0,0,0,0,NULL,?rem,1,'Approved',?by,NOW());",
                    new MySqlParameter("?grn",  grnNo),
                    new MySqlParameter("?dt",   NowIST().Date),
                    new MySqlParameter("?sup",  supplierId),
                    new MySqlParameter("?rmid", rmId),
                    new MySqlParameter("?qty",  qty),
                    new MySqlParameter("?rem",  remarks),
                    new MySqlParameter("?by",   userId));

                // Deduct from Stage 2 RM stock (Roasted Peanuts → consumed by sorting)
                ExecuteNonQuery(
                    "INSERT INTO MM_StockConsumption" +
                    " (ExecutionID, OrderID, BatchNo, RMID, SourceType, SourceID, QtyConsumed, ConsumedAt, CreatedBy)" +
                    " VALUES (0, ?pid, 2, ?rmid, 'PREPROCESS', 0, ?qty, ?now, ?by);",
                    new MySqlParameter("?pid",  productId),
                    new MySqlParameter("?rmid", s2RmId),
                    new MySqlParameter("?qty",  qty),
                    new MySqlParameter("?now",  NowIST()),
                    new MySqlParameter("?by",   userId));

                // Log Stage 3 for tracking
                ExecuteNonQuery(
                    "INSERT INTO PP_PreprocessLog (ProductID, Stage, Qty, Remarks, CreatedAt, CreatedBy)" +
                    " VALUES (?pid, ?stage, ?qty, ?rem, ?now, ?by);",
                    new MySqlParameter("?pid",   productId),
                    new MySqlParameter("?stage", stage),
                    new MySqlParameter("?qty",   qty),
                    new MySqlParameter("?rem",   remarks),
                    new MySqlParameter("?now",   NowIST()),
                    new MySqlParameter("?by",    userId));
            }
        }

        public static decimal GetPreprocessStageTotal(int productId, int stage)
        {
            object val = ExecuteScalar(
                "SELECT IFNULL(SUM(Qty),0) FROM PP_PreprocessLog" +
                " WHERE ProductID=?pid AND Stage=?stage AND DATE(CreatedAt)=?today;",
                new MySqlParameter("?pid",   productId),
                new MySqlParameter("?stage", stage),
                new MySqlParameter("?today", TodayIST()));
            return val == null || val == DBNull.Value ? 0m : Convert.ToDecimal(val);
        }

        public static decimal GetPreprocessStage1TotalToday(int productId)
        {
            // Stage 1 is stored in MM_StockConsumption with SourceType='PREPROCESS'
            object val = ExecuteScalar(
                "SELECT IFNULL(SUM(QtyConsumed),0) FROM MM_StockConsumption" +
                " WHERE OrderID=?pid AND BatchNo=1 AND SourceType='PREPROCESS' AND DATE(ConsumedAt)=?today;",
                new MySqlParameter("?pid",   productId),
                new MySqlParameter("?today", TodayIST()));
            return val == null || val == DBNull.Value ? 0m : Convert.ToDecimal(val);
        }

        public static DataTable GetPreprocessLogToday(int productId)
        {
            return ExecuteQuery(
                "SELECT Stage, Qty, CreatedAt FROM PP_PreprocessLog" +
                " WHERE ProductID=?pid AND DATE(CreatedAt)=?today" +
                " ORDER BY Stage, CreatedAt;",
                new MySqlParameter("?pid",   productId),
                new MySqlParameter("?today", TodayIST()));
        }

        public static DataTable GetPreprocessStage1LogToday(int productId)
        {
            return ExecuteQuery(
                "SELECT QtyConsumed AS Qty, ConsumedAt AS CreatedAt FROM MM_StockConsumption" +
                " WHERE OrderID=?pid AND BatchNo=1 AND SourceType='PREPROCESS' AND DATE(ConsumedAt)=?today" +
                " ORDER BY ConsumedAt;",
                new MySqlParameter("?pid",   productId),
                new MySqlParameter("?today", TodayIST()));
        }

        public static DataTable ExecuteQueryPublic(string sql, params MySqlParameter[] prms)
            => ExecuteQuery(sql, prms);


        // ── PRODUCT PARAMS ────────────────────────────────────────────────────

        public static DataTable GetProductParams(int productId)
        {
            try
            {
                return ExecuteQuery(
                    "SELECT ParamID, ParamOrder, ParamType, ParamLabel," +
                    " IFNULL(ParamOptions,'') AS ParamOptions" +
                    " FROM PP_ProductParams WHERE ProductID=?pid ORDER BY ParamOrder;",
                    new MySqlParameter("?pid", productId));
            }
            catch { return new DataTable(); }
        }

        public static DataTable GetRemarkOptions()
        {
            try
            {
                return ExecuteQuery(
                    "SELECT OptionID, OptionText FROM PP_RemarkOptions ORDER BY SortOrder, OptionText;");
            }
            catch { return new DataTable(); }
        }

        public static void SaveRemarkOptions(string[] options)
        {
            ExecuteNonQuery("DELETE FROM PP_RemarkOptions WHERE OptionID > 0;");
            for (int i = 0; i < options.Length; i++)
            {
                if (!string.IsNullOrEmpty(options[i]))
                    ExecuteNonQuery(
                        "INSERT INTO PP_RemarkOptions (OptionText, SortOrder) VALUES(?txt, ?ord);",
                        new MySqlParameter("?txt", options[i].Trim()),
                        new MySqlParameter("?ord", i + 1));
            }
        }

        public static void SaveProductParams(int productId, string[] types, string[] labels, string[] options)
        {
            // Get existing params for this product
            var existing = ExecuteQuery(
                "SELECT ParamID, ParamOrder FROM PP_ProductParams WHERE ProductID=?pid ORDER BY ParamOrder;",
                new MySqlParameter("?pid", productId));

            // Build list of existing ParamIDs
            var existingIds = new System.Collections.Generic.List<int>();
            foreach (DataRow r in existing.Rows)
                existingIds.Add(Convert.ToInt32(r["ParamID"]));

            // Update or insert params
            int usedCount = 0;
            for (int i = 0; i < types.Length; i++)
            {
                if (string.IsNullOrEmpty(types[i])) continue;
                string lbl  = string.IsNullOrEmpty(labels[i]) ? types[i] : labels[i];
                object opts = i < options.Length && !string.IsNullOrEmpty(options[i]) ? (object)options[i] : DBNull.Value;

                if (usedCount < existingIds.Count)
                {
                    // Update existing row
                    ExecuteNonQuery(
                        "UPDATE PP_ProductParams SET ParamOrder=?ord, ParamType=?type, ParamLabel=?lbl, ParamOptions=?opts" +
                        " WHERE ParamID=?id;",
                        new MySqlParameter("?ord",  i + 1),
                        new MySqlParameter("?type", types[i]),
                        new MySqlParameter("?lbl",  lbl),
                        new MySqlParameter("?opts", opts),
                        new MySqlParameter("?id",   existingIds[usedCount]));
                }
                else
                {
                    // Insert new row
                    ExecuteNonQuery(
                        "INSERT INTO PP_ProductParams (ProductID, ParamOrder, ParamType, ParamLabel, ParamOptions)" +
                        " VALUES(?pid, ?ord, ?type, ?lbl, ?opts);",
                        new MySqlParameter("?pid",  productId),
                        new MySqlParameter("?ord",  i + 1),
                        new MySqlParameter("?type", types[i]),
                        new MySqlParameter("?lbl",  lbl),
                        new MySqlParameter("?opts", opts));
                }
                usedCount++;
            }

            // Delete surplus old params that are no longer needed
            // Only delete if no batch params reference them
            for (int j = usedCount; j < existingIds.Count; j++)
            {
                int paramId = existingIds[j];
                object refCount = ExecuteScalar(
                    "SELECT COUNT(*) FROM PP_BatchParams WHERE ParamID=?pid;",
                    new MySqlParameter("?pid", paramId));
                if (Convert.ToInt32(refCount) == 0)
                {
                    ExecuteNonQuery("DELETE FROM PP_ProductParams WHERE ParamID=?pid;",
                        new MySqlParameter("?pid", paramId));
                }
                // If referenced, leave it — it's historical data
            }
        }

        public static void SaveBatchParams(int execId, int[] paramIds, decimal?[] values)
        {
            ExecuteNonQuery("DELETE FROM PP_BatchParams WHERE ExecutionID=?eid;",
                new MySqlParameter("?eid", execId));
            for (int i = 0; i < paramIds.Length; i++)
            {
                ExecuteNonQuery(
                    "INSERT INTO PP_BatchParams (ExecutionID, ParamID, Value)" +
                    " VALUES(?eid, ?pid, ?val);",
                    new MySqlParameter("?eid", execId),
                    new MySqlParameter("?pid", paramIds[i]),
                    new MySqlParameter("?val", values[i].HasValue ? (object)values[i].Value : DBNull.Value));
            }
        }

        public static DataTable GetBatchParams(int execId)
        {
            return ExecuteQuery(
                "SELECT bp.ParamID, pp.ParamLabel, pp.ParamType, bp.Value" +
                " FROM PP_BatchParams bp" +
                " JOIN PP_ProductParams pp ON pp.ParamID = bp.ParamID" +
                " WHERE bp.ExecutionID=?eid ORDER BY pp.ParamOrder;",
                new MySqlParameter("?eid", execId));
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
                "AND p.ProductType != 'Prefilled Conversion' " +
                "ORDER BY p.ProductName;",
                new MySqlParameter("?sh", shift),
                new MySqlParameter("?dt", orderDate.Date));
        }

        // Get initiated orders filtered by shift AND production line
        public static DataTable GetInitiatedOrdersForShiftAndLine(int shift, DateTime orderDate, int lineId)
        {
            return ExecuteQuery(
                "SELECT o.OrderID, o.ProductID, p.ProductName, p.ProductCode, " +
                "IFNULL(o.RevisedBatches, o.OrderedBatches) AS EffectiveBatches, " +
                "p.BatchSize, ou.Abbreviation AS OutputAbbr, o.Status " +
                "FROM PP_ProductionOrder o " +
                "JOIN PP_Products p  ON p.ProductID = o.ProductID " +
                "JOIN MM_UOM ou ON ou.UOMID = p.OutputUOMID " +
                "WHERE o.Shift = ?sh AND o.OrderDate = ?dt " +
                "AND p.ProductionLineID = ?lineId " +
                "AND o.Status IN ('Initiated','InProgress','Stopped') " +
                "AND p.ProductType != 'Prefilled Conversion' " +
                "ORDER BY p.ProductName;",
                new MySqlParameter("?sh", shift),
                new MySqlParameter("?dt", orderDate.Date),
                new MySqlParameter("?lineId", lineId));
        }

        // ── PRODUCTION LINES ─────────────────────────────────────────────
        public static DataTable GetActiveProductionLines()
        {
            return ExecuteQuery("SELECT LineID, LineName, LineCode FROM PP_ProductionLines WHERE IsActive=1 ORDER BY SortOrder;");
        }

        public static DataRow GetProductionLineById(int lineId)
        {
            var dt = ExecuteQuery("SELECT * FROM PP_ProductionLines WHERE LineID=?id;",
                new MySqlParameter("?id", lineId));
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
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
                // 1. Opening stock — subtract already consumed amounts
                var openingRow = ExecuteQueryRow(
                    "SELECT os.OpeningStockID," +
                    " GREATEST(0, os.Quantity - IFNULL(SUM(c.QtyConsumed),0)) AS AvailableQty" +
                    " FROM MM_OpeningStock os" +
                    " LEFT JOIN MM_StockConsumption c ON c.SourceType='OPENING' AND c.SourceID=os.OpeningStockID" +
                    " WHERE os.MaterialType='RM' AND os.MaterialID=?rmid" +
                    " GROUP BY os.OpeningStockID, os.Quantity" +
                    " HAVING AvailableQty > 0;",
                    new MySqlParameter("?rmid", rmId));

                // 2. GRN rows oldest first — subtract already consumed amounts
                // QtyActualReceived is immutable; available = received - consumed per row
                var grnRows = ExecuteQuery(
                    "SELECT i.InwardID," +
                    " GREATEST(0, i.QtyActualReceived - IFNULL(SUM(c.QtyConsumed),0)) AS AvailableQty" +
                    " FROM MM_RawInward i" +
                    " LEFT JOIN MM_StockConsumption c ON c.SourceType='GRN' AND c.SourceID=i.InwardID" +
                    " WHERE i.RMID=?rmid" +
                    " GROUP BY i.InwardID, i.QtyActualReceived, i.InwardDate" +
                    " HAVING AvailableQty > 0" +
                    " ORDER BY i.InwardDate ASC, i.InwardID ASC;",
                    new MySqlParameter("?rmid", rmId));

                // Calculate total available
                decimal available = 0;
                if (openingRow != null)
                    available += Convert.ToDecimal(openingRow["AvailableQty"]);
                foreach (System.Data.DataRow g in grnRows.Rows)
                    available += Convert.ToDecimal(g["AvailableQty"]);

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
                    decimal osQty   = Convert.ToDecimal(openingRow["AvailableQty"]);
                    int     osId    = Convert.ToInt32(openingRow["OpeningStockID"]);
                    decimal consume = Math.Min(osQty, remaining);

                    // Do NOT physically modify MM_OpeningStock.Quantity
                    // Stock position is calculated as: Opening + GRN - Consumption
                    InsertConsumption(executionId, orderId, batchNo, rmId,
                        "OPENING", osId, consume, userId);

                    remaining -= consume;
                }

                // Then: deduct from GRN rows oldest first
                foreach (System.Data.DataRow grn in grnRows.Rows)
                {
                    if (remaining <= 0) break;

                    decimal grnQty  = Convert.ToDecimal(grn["AvailableQty"]);
                    int     grnId   = Convert.ToInt32(grn["InwardID"]);
                    decimal consume = Math.Min(grnQty, remaining);

                    // Do NOT physically modify MM_RawInward.QtyActualReceived
                    // Stock position is calculated as: Opening + GRN - Consumption
                    // Modifying GRN rows would cause double-deduction in stock report
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
    }
}
