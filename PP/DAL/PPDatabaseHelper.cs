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
                "UPDATE Users SET LastLogin=NOW() WHERE UserID=?id;",
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
                "SELECT MAX(CAST(SUBSTRING(ProductCode,3) AS SIGNED)) " +
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

        public static DataRow GetProductionOrderById(int orderId)
        {
            return ExecuteQueryRow(
                "SELECT po.*, pr.ProductName, pr.ProductCode FROM PP_ProductionOrder po " +
                "JOIN PP_Products pr ON pr.ProductID=po.ProductID " +
                "WHERE po.OrderID=?id;",
                new MySqlParameter("?id", orderId));
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
    }
}
