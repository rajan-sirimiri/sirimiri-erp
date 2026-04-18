using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace PKApp.DAL
{
    public static class PKDatabaseHelper
    {
        private static string ConnStr =>
            ConfigurationManager.ConnectionStrings["StockDB"].ConnectionString;

        public static DateTime NowIST()
        {
            try { return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "India Standard Time"); }
            catch { return DateTime.UtcNow.AddHours(5.5); }
        }
        public static DateTime TodayIST() => NowIST().Date;

        // ── Core DB helpers ─────────────────────────────────────────────────
        public static DataRow ValidateUser(string username, string hash)
        {
            return ExecuteQueryRow(
                "SELECT UserID, Username, FullName, Role, MustChangePwd FROM Users" +
                " WHERE Username=?u AND PasswordHash=?p AND IsActive=1;",
                new MySqlParameter("?u", username),
                new MySqlParameter("?p", hash));
        }

        private static DataTable ExecuteQuery(string sql, params MySqlParameter[] prms)
        {
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd  = new MySqlCommand(sql, conn))
            {
                conn.Open(); foreach (var p in prms) cmd.Parameters.Add(p);
                var dt = new DataTable(); new MySqlDataAdapter(cmd).Fill(dt); return dt;
            }
        }

        private static DataRow ExecuteQueryRow(string sql, params MySqlParameter[] prms)
        {
            var dt = ExecuteQuery(sql, prms);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        private static void ExecuteNonQuery(string sql, params MySqlParameter[] prms)
        {
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd  = new MySqlCommand(sql, conn))
            {
                conn.Open(); foreach (var p in prms) cmd.Parameters.Add(p);
                cmd.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string sql, params MySqlParameter[] prms)
        {
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd  = new MySqlCommand(sql, conn))
            {
                conn.Open(); foreach (var p in prms) cmd.Parameters.Add(p);
                return cmd.ExecuteScalar();
            }
        }

        public static DataTable ExecuteQueryPublic(string sql, params MySqlParameter[] prms)
            => ExecuteQuery(sql, prms);

        // ── CUSTOMERS ────────────────────────────────────────────────────────
        // ── CUSTOMER TYPES ────────────────────────────────────────────────
        public static DataTable GetCustomerTypes()
        {
            return ExecuteQuery(
                "SELECT TypeID, TypeCode, TypeName FROM PK_CustomerTypes WHERE IsActive=1 ORDER BY TypeName;");
        }

        public static void AddCustomerType(string typeCode, string typeName)
        {
            ExecuteNonQuery(
                "INSERT INTO PK_CustomerTypes (TypeCode, TypeName) VALUES(?code, ?name);",
                new MySqlParameter("?code", typeCode),
                new MySqlParameter("?name", typeName));
        }

        // ── CUSTOMERS ────────────────────────────────────────────────────────
        public static DataTable GetAllCustomers()
        {
            return ExecuteQuery(
                "SELECT c.CustomerID, c.CustomerCode, c.CustomerType, c.CustomerName," +
                " c.ContactPerson, c.Phone, c.Email, c.City, c.State, c.PinCode," +
                " c.GSTIN, c.IsActive, c.CreatedAt," +
                " IFNULL(ct.TypeName,'') AS TypeName" +
                " FROM PK_Customers c" +
                " LEFT JOIN PK_CustomerTypes ct ON ct.TypeCode = c.CustomerType" +
                " ORDER BY c.CustomerName;");
        }

        public static DataTable GetActiveCustomers()
        {
            return ExecuteQuery(
                "SELECT c.CustomerID, c.CustomerCode, c.CustomerType, c.CustomerName," +
                " c.ContactPerson, c.Phone, c.Email, c.GSTIN, c.State, c.City," +
                " IFNULL(ct.TypeName,'') AS TypeName" +
                " FROM PK_Customers c" +
                " LEFT JOIN PK_CustomerTypes ct ON ct.TypeCode = c.CustomerType" +
                " WHERE c.IsActive=1 ORDER BY c.CustomerName;");
        }

        /// <summary>Get active customers filtered by type codes (e.g. "RT" or "DI,ST").</summary>
        public static DataTable GetActiveCustomersByType(string typeCodes)
        {
            // Build IN clause from comma-separated type codes
            string[] types = typeCodes.Split(',');
            string inClause = "";
            var prms = new System.Collections.Generic.List<MySqlParameter>();
            for (int i = 0; i < types.Length; i++)
            {
                if (i > 0) inClause += ",";
                inClause += "?t" + i;
                prms.Add(new MySqlParameter("?t" + i, types[i].Trim()));
            }
            return ExecuteQuery(
                "SELECT c.CustomerID, c.CustomerCode, c.CustomerType, c.CustomerName," +
                " c.ContactPerson, c.Phone, c.Email, c.GSTIN, c.State, c.City," +
                " IFNULL(ct.TypeName,'') AS TypeName" +
                " FROM PK_Customers c" +
                " LEFT JOIN PK_CustomerTypes ct ON ct.TypeCode = c.CustomerType" +
                " WHERE c.IsActive=1 AND c.CustomerType IN (" + inClause + ") ORDER BY c.CustomerName;",
                prms.ToArray());
        }

        public static DataRow GetCustomerById(int id)
        {
            return ExecuteQueryRow(
                "SELECT * FROM PK_Customers WHERE CustomerID=?id;",
                new MySqlParameter("?id", id));
        }

        public static int AddCustomer(string customerType, string name, string contact,
            string phone, string email, string address, string city, string state,
            string pinCode, string gstin)
        {
            string code = GenerateCustomerCode(customerType);
            ExecuteNonQuery(
                "INSERT INTO PK_Customers (CustomerCode,CustomerType,CustomerName,ContactPerson,Phone," +
                "Email,Address,City,State,PinCode,GSTIN,IsActive,CreatedAt)" +
                " VALUES(?code,?type,?name,?con,?ph,?em,?addr,?city,?state,?pin,?gst,1,NOW());",
                new MySqlParameter("?code",  code),
                new MySqlParameter("?type",  string.IsNullOrEmpty(customerType) ? (object)DBNull.Value : customerType),
                new MySqlParameter("?name",  name),
                new MySqlParameter("?con",   contact ?? (object)DBNull.Value),
                new MySqlParameter("?ph",    phone   ?? (object)DBNull.Value),
                new MySqlParameter("?em",    email   ?? (object)DBNull.Value),
                new MySqlParameter("?addr",  address ?? (object)DBNull.Value),
                new MySqlParameter("?city",  city    ?? (object)DBNull.Value),
                new MySqlParameter("?state", state   ?? (object)DBNull.Value),
                new MySqlParameter("?pin",   pinCode ?? (object)DBNull.Value),
                new MySqlParameter("?gst",   gstin   ?? (object)DBNull.Value));
            return Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
        }

        public static void UpdateCustomer(int id, string code, string customerType,
            string name, string contact, string phone, string email, string address,
            string city, string state, string pinCode, string gstin)
        {
            ExecuteNonQuery(
                "UPDATE PK_Customers SET CustomerCode=?code,CustomerType=?type,CustomerName=?name," +
                "ContactPerson=?con,Phone=?ph,Email=?em,Address=?addr," +
                "City=?city,State=?state,PinCode=?pin,GSTIN=?gst WHERE CustomerID=?id;",
                new MySqlParameter("?code",  code),
                new MySqlParameter("?type",  string.IsNullOrEmpty(customerType) ? (object)DBNull.Value : customerType),
                new MySqlParameter("?name",  name),
                new MySqlParameter("?con",   contact ?? (object)DBNull.Value),
                new MySqlParameter("?ph",    phone   ?? (object)DBNull.Value),
                new MySqlParameter("?em",    email   ?? (object)DBNull.Value),
                new MySqlParameter("?addr",  address ?? (object)DBNull.Value),
                new MySqlParameter("?city",  city    ?? (object)DBNull.Value),
                new MySqlParameter("?state", state   ?? (object)DBNull.Value),
                new MySqlParameter("?pin",   pinCode ?? (object)DBNull.Value),
                new MySqlParameter("?gst",   gstin   ?? (object)DBNull.Value),
                new MySqlParameter("?id",    id));
        }

        public static void ToggleCustomer(int id, bool active)
        {
            ExecuteNonQuery("UPDATE PK_Customers SET IsActive=?a WHERE CustomerID=?id;",
                new MySqlParameter("?a",  active ? 1 : 0),
                new MySqlParameter("?id", id));
        }

        private static string GenerateCustomerCode(string typeCode)
        {
            string prefix = string.IsNullOrEmpty(typeCode) ? "CU" : typeCode;
            object last = ExecuteScalar(
                "SELECT CustomerCode FROM PK_Customers" +
                " WHERE CustomerCode LIKE ?pat ORDER BY CustomerID DESC LIMIT 1;",
                new MySqlParameter("?pat", prefix + "-%"));
            int n = 1;
            if (last != null && last != DBNull.Value)
            {
                var m = System.Text.RegularExpressions.Regex.Match(last.ToString(), @"\d+$");
                if (m.Success) n = int.Parse(m.Value) + 1;
            }
            return prefix + "-" + n.ToString("D3");
        }


        // ── PRIMARY PACKING EXECUTION ────────────────────────────────────────

        // Products with active (Initiated/InProgress) orders today
        public static DataTable GetProductsInProduction()
        {
            return ExecuteQuery(
                "SELECT DISTINCT p.ProductID, p.ProductCode, p.ProductName," +
                " p.ContainerType, p.UnitsPerContainer, p.ContainersPerCase," +
                " IFNULL(p.HasLanguageLabels,0) AS HasLanguageLabels," +
                " ou.Abbreviation AS OutputUnit" +
                " FROM PP_ProductionOrder po" +
                " JOIN PP_Products p ON p.ProductID = po.ProductID" +
                " JOIN MM_UOM ou ON ou.UOMID = p.OutputUOMID" +
                " WHERE p.ProductType = 'Core'" +
                " AND po.Status IN ('Initiated','InProgress','Completed')" +
                " AND (SELECT COUNT(*) FROM PP_BatchExecution be" +
                "      WHERE be.OrderID=po.OrderID AND be.Status='Completed') > 0" +
                " AND (SELECT COUNT(*) FROM PK_PackingExecution pe" +
                "      WHERE pe.OrderID=po.OrderID AND pe.Status='Completed')" +
                " < (SELECT COUNT(*) FROM PP_BatchExecution be2" +
                "      WHERE be2.OrderID=po.OrderID AND be2.Status='Completed')" +
                " ORDER BY p.ProductName;");
        }


        // Orders with at least 1 produced batch not yet fully packed
        public static DataTable GetPendingPackingOrders(int productId)
        {
            return ExecuteQuery(
                "SELECT po.OrderID, po.OrderDate, po.Shift," +
                " IFNULL(po.RevisedBatches, po.OrderedBatches) AS TotalBatches," +
                " po.Status," +
                " (SELECT COUNT(*) FROM PP_BatchExecution be" +
                "  WHERE be.OrderID=po.OrderID AND be.Status='Completed') AS ProductionDone," +
                " (SELECT COUNT(DISTINCT pe.BatchNo) FROM PK_PackingExecution pe" +
                "  WHERE pe.OrderID=po.OrderID AND pe.Status='Completed' AND pe.BatchNo > 0) AS PackedBatches" +
                " FROM PP_ProductionOrder po" +
                " WHERE po.ProductID=?pid" +
                " AND po.Status IN ('Initiated','InProgress','Completed')" +
                " AND (SELECT COUNT(*) FROM PP_BatchExecution be2" +
                "      WHERE be2.OrderID=po.OrderID AND be2.Status='Completed') > 0" +
                " AND (SELECT COUNT(*) FROM PK_PackingExecution pe2" +
                "      WHERE pe2.OrderID=po.OrderID AND pe2.Status='Completed' AND pe2.BatchNo > 0)" +
                "    < (SELECT COUNT(*) FROM PP_BatchExecution be3" +
                "      WHERE be3.OrderID=po.OrderID AND be3.Status='Completed')" +
                " ORDER BY po.OrderDate DESC;",
                new MySqlParameter("?pid", productId));
        }

        // Get single order by ID with packing counts
        public static DataRow GetPackingOrderById(int orderId)
        {
            return ExecuteQueryRow(
                "SELECT po.OrderID, po.OrderDate, po.Shift," +
                " IFNULL(po.RevisedBatches, po.OrderedBatches) AS TotalBatches," +
                " po.Status, p.ProductID, p.ProductName, p.ProductCode," +
                " p.ContainerType, p.UnitsPerContainer, p.ContainersPerCase," +
                " IFNULL(p.HasLanguageLabels,0) AS HasLanguageLabels," +
                " (SELECT COUNT(*) FROM PP_BatchExecution be" +
                "  WHERE be.OrderID=po.OrderID AND be.Status='Completed') AS ProductionDone," +
                " (SELECT COUNT(DISTINCT pe.BatchNo) FROM PK_PackingExecution pe" +
                "  WHERE pe.OrderID=po.OrderID AND pe.Status='Completed' AND pe.BatchNo > 0) AS PackedBatches" +
                " FROM PP_ProductionOrder po" +
                " JOIN PP_Products p ON p.ProductID = po.ProductID" +
                " WHERE po.OrderID=?oid;",
                new MySqlParameter("?oid", orderId));
        }

        // Legacy — use GetPendingPackingOrders + GetPackingOrderById instead
        public static DataRow GetPackingOrderForProduct(int productId)
        {
            return ExecuteQueryRow(
                "SELECT po.OrderID," +
                " IFNULL(po.RevisedBatches, po.OrderedBatches) AS TotalBatches," +
                " po.Status," +
                " (SELECT COUNT(DISTINCT pe.BatchNo) FROM PK_PackingExecution pe" +
                "  WHERE pe.OrderID=po.OrderID AND pe.Status='Completed') AS PackedBatches," +
                " (SELECT COUNT(*) FROM PP_BatchExecution be" +
                "  WHERE be.OrderID=po.OrderID AND be.Status='Completed') AS ProductionDone" +
                " FROM PP_ProductionOrder po" +
                " WHERE po.ProductID=?pid AND po.Status IN ('Initiated','InProgress','Completed')" +
                " ORDER BY po.OrderDate DESC LIMIT 1;",
                new MySqlParameter("?pid", productId));
        }

        // Active (InProgress) or Ended packing batch for an order
        public static DataRow GetActivePacking(int orderId)
        {
            return ExecuteQueryRow(
                "SELECT * FROM PK_PackingExecution" +
                " WHERE OrderID=?oid AND Status IN ('InProgress','Ended')" +
                " ORDER BY PackingID DESC LIMIT 1;",
                new MySqlParameter("?oid", orderId));
        }

        public static DataRow GetEndedPacking(int orderId)
        {
            return ExecuteQueryRow(
                "SELECT * FROM PK_PackingExecution" +
                " WHERE OrderID=?oid AND Status='Ended'" +
                " ORDER BY PackingID DESC LIMIT 1;",
                new MySqlParameter("?oid", orderId));
        }

        public static DataTable GetPackingHistory(int orderId)
        {
            return ExecuteQuery(
                "SELECT PackingID, BatchNo, StartTime, EndTime," +
                " Cases, Jars, Units, JarSize, TotalUnits, Status, LabelLanguage" +
                " FROM PK_PackingExecution WHERE OrderID=?oid AND BatchNo > 0 ORDER BY BatchNo;",
                new MySqlParameter("?oid", orderId));
        }

        public static int StartPackingBatch(int orderId, int batchNo, int userId, string labelLanguage = null)
        {
            var existing = ExecuteQueryRow(
                "SELECT PackingID FROM PK_PackingExecution WHERE OrderID=?oid AND BatchNo=?bno;",
                new MySqlParameter("?oid", orderId),
                new MySqlParameter("?bno", batchNo));
            if (existing != null) return Convert.ToInt32(existing["PackingID"]);
            ExecuteNonQuery(
                "INSERT INTO PK_PackingExecution (OrderID,BatchNo,StartTime,Status,CreatedBy,LabelLanguage)" +
                " VALUES(?oid,?bno,?now,'InProgress',?by,?lang);",
                new MySqlParameter("?oid",  orderId),
                new MySqlParameter("?bno",  batchNo),
                new MySqlParameter("?now",  NowIST()),
                new MySqlParameter("?by",   userId),
                new MySqlParameter("?lang", string.IsNullOrEmpty(labelLanguage) ? (object)DBNull.Value : labelLanguage));
            return Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
        }

        public static void EndPackingBatch(int packingId)
        {
            ExecuteNonQuery(
                "UPDATE PK_PackingExecution SET Status='Ended', EndTime=?now WHERE PackingID=?id;",
                new MySqlParameter("?now", NowIST()),
                new MySqlParameter("?id",  packingId));
        }

        // Mark batch as Completed directly — used in new flow (output recorded at end of all batches)
        public static void CompletePackingBatch(int packingId)
        {
            ExecuteNonQuery(
                "UPDATE PK_PackingExecution SET Status='Completed', EndTime=?now WHERE PackingID=?id;",
                new MySqlParameter("?now", NowIST()),
                new MySqlParameter("?id",  packingId));
        }

        public static void SavePackingOutput(int packingId, int orderId, int productId,
            int cases, int jars, int units, int jarSize, int userId)
        {
            // Get packing spec
            var spec = ExecuteQueryRow(
                "SELECT IFNULL(ContainersPerCase,12) AS ContainersPerCase," +
                " IFNULL(ContainerType,'DIRECT') AS ContainerType" +
                " FROM PP_Products WHERE ProductID=?pid;",
                new MySqlParameter("?pid", productId));
            int    containersPerCase = spec != null ? Convert.ToInt32(spec["ContainersPerCase"]) : 12;
            string containerType     = spec != null ? spec["ContainerType"].ToString() : "DIRECT";

            int totalUnits;
            if (containerType == "DIRECT")
                totalUnits = cases * jarSize;  // cases × units per case
            else
                totalUnits = (cases * containersPerCase * jarSize) + (jars * jarSize);

            ExecuteNonQuery(
                "UPDATE PK_PackingExecution SET Cases=?c,Jars=?j,Units=?u," +
                "JarSize=?js,TotalUnits=?tot,Status='Completed' WHERE PackingID=?id;",
                new MySqlParameter("?c",   cases),
                new MySqlParameter("?j",   jars),
                new MySqlParameter("?u",   units),
                new MySqlParameter("?js",  jarSize),
                new MySqlParameter("?tot", totalUnits),
                new MySqlParameter("?id",  packingId));

            // Add to FG Stock
            AddFGStock(productId, totalUnits, 0, orderId, 0, userId);

            // Check if all batches packed
            var orderRow = ExecuteQueryRow(
                "SELECT IFNULL(RevisedBatches,OrderedBatches) AS TotalBatches FROM PP_ProductionOrder WHERE OrderID=?oid;",
                new MySqlParameter("?oid", orderId));
            if (orderRow != null)
            {
                int total  = Convert.ToInt32(orderRow["TotalBatches"]);
                int packed = Convert.ToInt32(ExecuteScalar(
                    "SELECT COUNT(*) FROM PK_PackingExecution WHERE OrderID=?oid AND Status='Completed';",
                    new MySqlParameter("?oid", orderId)));
                if (packed >= total)
                    ExecuteNonQuery(
                        "UPDATE PP_ProductionOrder SET Status='Completed' WHERE OrderID=?oid;",
                        new MySqlParameter("?oid", orderId));
            }
        }

        // Save total packing output for an order — inserts/updates summary row
        public static void SaveOrderPackingOutput(int orderId, int productId,
            int cases, int jars, int units, int jarSize, int containersPerCase,
            string containerType, int userId)
        {
            int totalPcs;
            if (containerType == "DIRECT")
                totalPcs = (cases * jarSize) + units;
            else
                totalPcs = (cases * containersPerCase * jarSize) + (jars * jarSize) + units;

            // Check if summary row exists
            var existing = ExecuteQueryRow(
                "SELECT PackingID FROM PK_PackingExecution" +
                " WHERE OrderID=?oid AND BatchNo=0;",
                new MySqlParameter("?oid", orderId));

            if (existing != null)
            {
                ExecuteNonQuery(
                    "UPDATE PK_PackingExecution SET Cases=?c, Jars=?j, Units=?u," +
                    " JarSize=?js, TotalUnits=?tot, Status='Completed', EndTime=?now" +
                    " WHERE OrderID=?oid AND BatchNo=0;",
                    new MySqlParameter("?c",   cases),
                    new MySqlParameter("?j",   jars),
                    new MySqlParameter("?u",   units),
                    new MySqlParameter("?js",  jarSize),
                    new MySqlParameter("?tot", totalPcs),
                    new MySqlParameter("?now", NowIST()),
                    new MySqlParameter("?oid", orderId));
            }
            else
            {
                ExecuteNonQuery(
                    "INSERT INTO PK_PackingExecution" +
                    " (OrderID,BatchNo,StartTime,EndTime,Cases,Jars,Units,JarSize,TotalUnits,Status,CreatedBy)" +
                    " VALUES(?oid,0,?now,?now,?c,?j,?u,?js,?tot,'Completed',?by);",
                    new MySqlParameter("?oid", orderId),
                    new MySqlParameter("?now", NowIST()),
                    new MySqlParameter("?c",   cases),
                    new MySqlParameter("?j",   jars),
                    new MySqlParameter("?u",   units),
                    new MySqlParameter("?js",  jarSize),
                    new MySqlParameter("?tot", totalPcs),
                    new MySqlParameter("?by",  userId));
            }

            // Add to FG Stock
            AddFGStock(productId, totalPcs, 0, orderId, 0, userId);
        }

        // ── FG STOCK ────────────────────────────────────────────────────────
        public static DataTable GetFGStockByProduct(int productId)
        {
            return ExecuteQuery(
                "SELECT FGStockID, QtyPacked, PackedAt, ExecutionID, OrderID, BatchNo" +
                " FROM PK_FGStock WHERE ProductID=?pid ORDER BY PackedAt DESC;",
                new MySqlParameter("?pid", productId));
        }

        public static decimal GetFGAvailable(int productId)
        {
            // FG Available = Total Packed - Total Shipped
            object packed = ExecuteScalar(
                "SELECT IFNULL(SUM(QtyPacked),0) FROM PK_FGStock WHERE ProductID=?pid;",
                new MySqlParameter("?pid", productId));
            object shipped = ExecuteScalar(
                "SELECT IFNULL(SUM(sl.QtyShipped),0) FROM PK_ShipmentLine sl" +
                " JOIN PK_Shipment s ON s.ShipmentID=sl.ShipmentID" +
                " WHERE sl.ProductID=?pid AND s.Status != 'Cancelled';",
                new MySqlParameter("?pid", productId));
            return Convert.ToDecimal(packed) - Convert.ToDecimal(shipped);
        }

        // ── PRIMARY PACKING ──────────────────────────────────────────────────
        public static DataTable GetCompletedBatchesForPacking()
        {
            return ExecuteQuery(
                "SELECT be.ExecutionID, be.OrderID, be.BatchNo, be.ActualOutput," +
                " ou.Abbreviation AS OutputUnit, p.ProductID, p.ProductName, p.ProductCode," +
                " po.OrderDate, IFNULL(pk.PackedQty,0) AS PackedQty," +
                " be.ActualOutput - IFNULL(pk.PackedQty,0) AS PendingQty" +
                " FROM PP_BatchExecution be" +
                " JOIN PP_ProductionOrder po ON po.OrderID = be.OrderID" +
                " JOIN PP_Products p ON p.ProductID = po.ProductID" +
                " JOIN MM_UOM ou ON ou.UOMID = p.OutputUOMID" +
                " LEFT JOIN (SELECT ExecutionID, SUM(QtyPacked) AS PackedQty" +
                "   FROM PK_FGStock GROUP BY ExecutionID) pk ON pk.ExecutionID = be.ExecutionID" +
                " WHERE be.Status = 'Completed' AND be.ActualOutput > 0" +
                " AND (be.ActualOutput - IFNULL(pk.PackedQty,0)) > 0.001" +
                " ORDER BY po.OrderDate DESC, p.ProductName;");
        }

        public static int AddFGStock(int productId, decimal qty, int executionId,
            int orderId, int batchNo, int userId)
        {
            ExecuteNonQuery(
                "INSERT INTO PK_FGStock (ProductID,QtyPacked,PackedAt,ExecutionID,OrderID,BatchNo,CreatedBy)" +
                " VALUES(?pid,?qty,?now,?eid,?oid,?bno,?by);",
                new MySqlParameter("?pid", productId),
                new MySqlParameter("?qty", qty),
                new MySqlParameter("?now", NowIST()),
                new MySqlParameter("?eid", executionId),
                new MySqlParameter("?oid", orderId),
                new MySqlParameter("?bno", batchNo),
                new MySqlParameter("?by",  userId));
            int fgId = Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
            return fgId;
        }

        public static void RecordPMConsumption(int fgStockId, int productId,
            int pmId, decimal qtyUsed, int userId)
        {
            // Record PM usage against FG packing entry
            ExecuteNonQuery(
                "INSERT INTO PK_PrimaryPacking (FGStockID,ProductID,PMID,QtyUsed,PackedAt,CreatedBy)" +
                " VALUES(?fgid,?pid,?pmid,?qty,?now,?by);",
                new MySqlParameter("?fgid", fgStockId),
                new MySqlParameter("?pid",  productId),
                new MySqlParameter("?pmid", pmId),
                new MySqlParameter("?qty",  qtyUsed),
                new MySqlParameter("?now",  NowIST()),
                new MySqlParameter("?by",   userId));
            // Record PM consumption in PK_PMConsumption for stock tracking
            ExecuteNonQuery(
                "INSERT INTO PK_PMConsumption (PMID, QtyUsed, UsedAt, SourceType, SourceID, CreatedBy)" +
                " VALUES(?pmid,?qty,?now,'PRIMARY',?fgid,?by);",
                new MySqlParameter("?pmid", pmId),
                new MySqlParameter("?qty",  qtyUsed),
                new MySqlParameter("?now",  NowIST()),
                new MySqlParameter("?fgid", fgStockId),
                new MySqlParameter("?by",   userId));
        }

        public static DataTable GetPrimaryPackingToday()
        {
            return ExecuteQuery(
                "SELECT fg.FGStockID, p.ProductName, p.ProductCode," +
                " fg.QtyPacked, ou.Abbreviation AS Unit," +
                " fg.PackedAt, fg.BatchNo," +
                " pm.PMName, pp.QtyUsed, pu.Abbreviation AS PMUnit" +
                " FROM PK_FGStock fg" +
                " JOIN PP_Products p  ON p.ProductID = fg.ProductID" +
                " JOIN MM_UOM ou      ON ou.UOMID = p.OutputUOMID" +
                " LEFT JOIN PK_PrimaryPacking pp ON pp.FGStockID = fg.FGStockID" +
                " LEFT JOIN MM_PackingMaterials pm ON pm.PMID = pp.PMID" +
                " LEFT JOIN MM_UOM pu ON pu.UOMID = pm.UOMID" +
                " WHERE DATE(fg.PackedAt) = ?today" +
                " ORDER BY fg.PackedAt DESC;",
                new MySqlParameter("?today", TodayIST()));
        }

        // ── SECONDARY PACKING ────────────────────────────────────────────────
        public static DataTable GetFGReadyForSecondary()
        {
            return ExecuteQuery(
                "SELECT p.ProductID, p.ProductName, p.ProductCode," +
                " p.HSNCode, IFNULL(p.GSTRate, 0) AS GSTRate," +
                " IFNULL(p.ContainerType,'DIRECT') AS ContainerType," +
                " IFNULL(p.ContainersPerCase,12) AS ContainersPerCase," +
                " IFNULL(p.UnitsPerContainer,'') AS UnitsPerContainer," +
                " ou.Abbreviation AS Unit," +
                // FGStock.QtyPacked is in PIECES (jars * unitSize)
                // SecondaryPacking.TotalUnits is in JARS, so multiply by unitSize to get pieces
                " ROUND(IFNULL(fg.TotalPacked,0)" +
                "  - IFNULL(sp.TotalPacked2,0)" +
                "    * CAST(SUBSTRING_INDEX(IFNULL(p.UnitsPerContainer,'1'),',',1) AS UNSIGNED)" +
                ", 0) AS AvailablePcs" +
                " FROM PP_Products p" +
                " JOIN MM_UOM ou ON ou.UOMID = p.OutputUOMID" +
                " LEFT JOIN (SELECT ProductID, SUM(QtyPacked) AS TotalPacked" +
                "   FROM PK_FGStock GROUP BY ProductID) fg ON fg.ProductID = p.ProductID" +
                " LEFT JOIN (SELECT ProductID, SUM(TotalUnits) AS TotalPacked2" +
                "   FROM PK_SecondaryPacking GROUP BY ProductID) sp ON sp.ProductID = p.ProductID" +
                " WHERE p.IsActive=1 AND p.ProductType='Core'" +
                " AND IFNULL(fg.TotalPacked,0)" +
                "  - IFNULL(sp.TotalPacked2,0)" +
                "    * CAST(SUBSTRING_INDEX(IFNULL(p.UnitsPerContainer,'1'),',',1) AS UNSIGNED) > 0" +
                " ORDER BY p.ProductName;");
        }

        /// Get all CASE-level PM mappings for a product (cartons, case labels, tape, etc.)
        public static DataTable GetCasePMsForProduct(int productId)
        {
            return ExecuteQuery(
                "SELECT m.MappingID, m.PMID, pm.PMName, pm.PMCode, m.QtyPerUnit," +
                " u.Abbreviation," +
                " ROUND(IFNULL(os.Quantity,0) + IFNULL(grn.TotalGRN,0) - IFNULL(con.TotalUsed,0), 4) AS CurrentStock" +
                " FROM PK_ProductPMMaster m" +
                " JOIN MM_PackingMaterials pm ON pm.PMID = m.PMID" +
                " JOIN MM_UOM u ON u.UOMID = pm.UOMID" +
                " LEFT JOIN (SELECT MaterialID, Quantity FROM MM_OpeningStock" +
                "   WHERE MaterialType='PM') os ON os.MaterialID = m.PMID" +
                " LEFT JOIN (SELECT PMID, SUM(QtyActualReceived) AS TotalGRN" +
                "   FROM MM_PackingInward GROUP BY PMID) grn ON grn.PMID = m.PMID" +
                " LEFT JOIN (SELECT PMID, SUM(QtyUsed) AS TotalUsed" +
                "   FROM PK_PMConsumption GROUP BY PMID) con ON con.PMID = m.PMID" +
                " WHERE m.ProductID=?pid AND m.ApplyLevel='CASE' AND m.IsActive=1" +
                " ORDER BY pm.PMName;",
                new MySqlParameter("?pid", productId));
        }

        public static void AddSecondaryPacking(int productId, decimal qtyCartons,
            int unitsPerCarton, int pmId, decimal cartonsUsed, string remarks, int userId)
        {
            decimal totalUnits = qtyCartons * unitsPerCarton;
            ExecuteNonQuery(
                "INSERT INTO PK_SecondaryPacking" +
                " (ProductID,QtyCartons,UnitsPerCarton,TotalUnits,PMID,CartonsUsed,PackedAt,Remarks,CreatedBy)" +
                " VALUES(?pid,?qty,?upc,?total,?pmid,?cu,?now,?rem,?by);",
                new MySqlParameter("?pid",   productId),
                new MySqlParameter("?qty",   qtyCartons),
                new MySqlParameter("?upc",   unitsPerCarton),
                new MySqlParameter("?total", totalUnits),
                new MySqlParameter("?pmid",  pmId > 0 ? (object)pmId : DBNull.Value),
                new MySqlParameter("?cu",    cartonsUsed > 0 ? (object)cartonsUsed : DBNull.Value),
                new MySqlParameter("?now",   NowIST()),
                new MySqlParameter("?rem",   remarks ?? (object)DBNull.Value),
                new MySqlParameter("?by",    userId));
            // Record carton PM consumption if specified
            if (pmId > 0 && cartonsUsed > 0)
            {
                int secPackId = Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
                ExecuteNonQuery(
                    "INSERT INTO PK_PMConsumption (PMID, QtyUsed, UsedAt, SourceType, SourceID, CreatedBy)" +
                    " VALUES(?pmid,?qty,?now,'SECONDARY',?sid,?by);",
                    new MySqlParameter("?pmid", pmId),
                    new MySqlParameter("?qty",  cartonsUsed),
                    new MySqlParameter("?now",  NowIST()),
                    new MySqlParameter("?sid",  secPackId),
                    new MySqlParameter("?by",   userId));
            }
        }

        public static void RecordSecondaryPMConsumption(int pmId, decimal qty, int secPackId, int userId)
        {
            ExecuteNonQuery(
                "INSERT INTO PK_PMConsumption (PMID, QtyUsed, UsedAt, SourceType, SourceID, CreatedBy)" +
                " VALUES(?pmid,?qty,?now,'SECONDARY',?sid,?by);",
                new MySqlParameter("?pmid", pmId),
                new MySqlParameter("?qty",  qty),
                new MySqlParameter("?now",  NowIST()),
                new MySqlParameter("?sid",  secPackId),
                new MySqlParameter("?by",   userId));
        }

        public static DataTable GetSecondaryPackingToday()
        {
            return ExecuteQuery(
                "SELECT sp.SecPackID, p.ProductName, p.ProductCode," +
                " sp.QtyCartons, sp.UnitsPerCarton, sp.TotalUnits," +
                " ou.Abbreviation AS Unit, sp.PackedAt, sp.Remarks," +
                " pm.PMName, sp.CartonsUsed," +
                " IFNULL(sp.PackingType,'CASE') AS PackingType," +
                " sp.OnlineOrderID, sp.CustomerName" +
                " FROM PK_SecondaryPacking sp" +
                " JOIN PP_Products p ON p.ProductID = sp.ProductID" +
                " JOIN MM_UOM ou ON ou.UOMID = p.OutputUOMID" +
                " LEFT JOIN MM_PackingMaterials pm ON pm.PMID = sp.PMID" +
                " WHERE DATE(sp.PackedAt) = ?today ORDER BY sp.PackedAt DESC;",
                new MySqlParameter("?today", TodayIST()));
        }

        /// Add an online order packing entry (one line per product in the order).
        public static void AddOnlineOrderPacking(int productId, int qty, int jarSize,
            int pmId, string onlineOrderId, string customerName, string remarks, int userId)
        {
            int totalPcs = qty * jarSize;
            ExecuteNonQuery(
                "INSERT INTO PK_SecondaryPacking" +
                " (ProductID,PackingType,OnlineOrderID,CustomerName," +
                "  QtyCartons,UnitsPerCarton,TotalUnits,PMID,CartonsUsed,PackedAt,Remarks,CreatedBy)" +
                " VALUES(?pid,'ONLINE',?oid,?cust,?qty,1,?total,?pmid,0,?now,?rem,?by);",
                new MySqlParameter("?pid",   productId),
                new MySqlParameter("?oid",   string.IsNullOrEmpty(onlineOrderId) ? (object)DBNull.Value : onlineOrderId),
                new MySqlParameter("?cust",  string.IsNullOrEmpty(customerName)  ? (object)DBNull.Value : customerName),
                new MySqlParameter("?qty",   qty),
                new MySqlParameter("?total", totalPcs),
                new MySqlParameter("?pmid",  pmId > 0 ? (object)pmId : DBNull.Value),
                new MySqlParameter("?now",   NowIST()),
                new MySqlParameter("?rem",   string.IsNullOrEmpty(remarks) ? (object)DBNull.Value : remarks),
                new MySqlParameter("?by",    userId));
        }

        /// Record carton PM consumption for online order (one carton per order).
        public static void RecordOnlineOrderCartonPM(int pmId, int userId)
        {
            if (pmId <= 0) return;
            int secPackId = Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
            ExecuteNonQuery(
                "INSERT INTO PK_PMConsumption (PMID, QtyUsed, UsedAt, SourceType, SourceID, CreatedBy)" +
                " VALUES(?pmid,1,?now,'ONLINE_ORDER',?sid,?by);",
                new MySqlParameter("?pmid", pmId),
                new MySqlParameter("?now",  NowIST()),
                new MySqlParameter("?sid",  secPackId),
                new MySqlParameter("?by",   userId));
        }

        // ── CUSTOMER PO ──────────────────────────────────────────────────────
        public static DataTable GetAllPOs()
        {
            return ExecuteQuery(
                "SELECT po.POID, po.POCode, c.CustomerName, po.PODate," +
                " po.DeliveryDate, po.Status, po.Remarks," +
                " COUNT(pl.LineID) AS LineCount" +
                " FROM PK_CustomerPO po" +
                " JOIN PK_Customers c ON c.CustomerID = po.CustomerID" +
                " LEFT JOIN PK_CustomerPOLine pl ON pl.POID = po.POID" +
                " GROUP BY po.POID ORDER BY po.PODate DESC;");
        }

        public static DataRow GetPOById(int poId)
        {
            return ExecuteQueryRow(
                "SELECT po.*, c.CustomerName FROM PK_CustomerPO po" +
                " JOIN PK_Customers c ON c.CustomerID=po.CustomerID WHERE po.POID=?id;",
                new MySqlParameter("?id", poId));
        }

        public static DataTable GetPOLines(int poId)
        {
            return ExecuteQuery(
                "SELECT pl.LineID, p.ProductName, p.ProductCode," +
                " pl.QtyOrdered, pl.QtyShipped, pl.UnitPrice," +
                " ou.Abbreviation AS Unit" +
                " FROM PK_CustomerPOLine pl" +
                " JOIN PP_Products p ON p.ProductID = pl.ProductID" +
                " JOIN MM_UOM ou ON ou.UOMID = p.OutputUOMID" +
                " WHERE pl.POID=?id ORDER BY p.ProductName;",
                new MySqlParameter("?id", poId));
        }

        public static int AddPO(int customerId, DateTime poDate, DateTime? deliveryDate,
            string remarks, int userId)
        {
            string code = GeneratePOCode();
            ExecuteNonQuery(
                "INSERT INTO PK_CustomerPO (POCode,CustomerID,PODate,DeliveryDate,Status,Remarks,CreatedAt,CreatedBy)" +
                " VALUES(?code,?cid,?dt,?dd,'Open',?rem,NOW(),?by);",
                new MySqlParameter("?code", code),
                new MySqlParameter("?cid",  customerId),
                new MySqlParameter("?dt",   poDate),
                new MySqlParameter("?dd",   deliveryDate.HasValue ? (object)deliveryDate.Value : DBNull.Value),
                new MySqlParameter("?rem",  remarks ?? (object)DBNull.Value),
                new MySqlParameter("?by",   userId));
            return Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
        }

        public static void AddPOLine(int poId, int productId, decimal qty, decimal? unitPrice)
        {
            ExecuteNonQuery(
                "INSERT INTO PK_CustomerPOLine (POID,ProductID,QtyOrdered,QtyShipped,UnitPrice)" +
                " VALUES(?poid,?pid,?qty,0,?price);",
                new MySqlParameter("?poid",  poId),
                new MySqlParameter("?pid",   productId),
                new MySqlParameter("?qty",   qty),
                new MySqlParameter("?price", unitPrice.HasValue ? (object)unitPrice.Value : DBNull.Value));
        }

        public static void DeletePOLine(int lineId)
        {
            ExecuteNonQuery("DELETE FROM PK_CustomerPOLine WHERE LineID=?id;",
                new MySqlParameter("?id", lineId));
        }

        private static string GeneratePOCode()
        {
            object last = ExecuteScalar(
                "SELECT POCode FROM PK_CustomerPO ORDER BY POID DESC LIMIT 1;");
            if (last == null || last == DBNull.Value)
                return "PO-" + DateTime.Now.Year + "-001";
            var m = System.Text.RegularExpressions.Regex.Match(last.ToString(), @"\d+$");
            int n = m.Success ? int.Parse(m.Value) + 1 : 1;
            return "PO-" + DateTime.Now.Year + "-" + n.ToString("D3");
        }

        // ── SHIPMENTS ────────────────────────────────────────────────────────
        public static DataTable GetOpenPOs()
        {
            return ExecuteQuery(
                "SELECT po.POID, po.POCode, c.CustomerName, po.PODate, po.DeliveryDate" +
                " FROM PK_CustomerPO po" +
                " JOIN PK_Customers c ON c.CustomerID=po.CustomerID" +
                " WHERE po.Status='Open' ORDER BY po.PODate DESC;");
        }

        public static DataTable GetAllShipments()
        {
            return ExecuteQuery(
                "SELECT s.ShipmentID, s.ShipmentCode, c.CustomerName," +
                " po.POCode, s.ShipDate, s.VehicleNo, s.Status," +
                " COUNT(sl.ShipLineID) AS Lines" +
                " FROM PK_Shipment s" +
                " JOIN PK_Customers c ON c.CustomerID=s.CustomerID" +
                " JOIN PK_CustomerPO po ON po.POID=s.POID" +
                " LEFT JOIN PK_ShipmentLine sl ON sl.ShipmentID=s.ShipmentID" +
                " GROUP BY s.ShipmentID ORDER BY s.ShipDate DESC;");
        }

        public static DataRow GetShipmentById(int id)
        {
            return ExecuteQueryRow(
                "SELECT s.*, c.CustomerName, po.POCode FROM PK_Shipment s" +
                " JOIN PK_Customers c ON c.CustomerID=s.CustomerID" +
                " JOIN PK_CustomerPO po ON po.POID=s.POID WHERE s.ShipmentID=?id;",
                new MySqlParameter("?id", id));
        }

        public static DataTable GetShipmentLines(int shipmentId)
        {
            return ExecuteQuery(
                "SELECT sl.ShipLineID, p.ProductName, p.ProductCode," +
                " sl.QtyShipped, ou.Abbreviation AS Unit" +
                " FROM PK_ShipmentLine sl" +
                " JOIN PP_Products p ON p.ProductID=sl.ProductID" +
                " JOIN MM_UOM ou ON ou.UOMID=p.OutputUOMID" +
                " WHERE sl.ShipmentID=?id ORDER BY p.ProductName;",
                new MySqlParameter("?id", shipmentId));
        }

        public static int CreateShipment(int poId, int customerId, DateTime shipDate,
            string vehicleNo, string driverName, string remarks, int userId)
        {
            string code = GenerateShipCode();
            ExecuteNonQuery(
                "INSERT INTO PK_Shipment (ShipmentCode,POID,CustomerID,ShipDate," +
                "VehicleNo,DriverName,Remarks,Status,CreatedAt,CreatedBy)" +
                " VALUES(?code,?poid,?cid,?dt,?veh,?drv,?rem,'Dispatched',NOW(),?by);",
                new MySqlParameter("?code", code),
                new MySqlParameter("?poid", poId),
                new MySqlParameter("?cid",  customerId),
                new MySqlParameter("?dt",   shipDate),
                new MySqlParameter("?veh",  vehicleNo  ?? (object)DBNull.Value),
                new MySqlParameter("?drv",  driverName ?? (object)DBNull.Value),
                new MySqlParameter("?rem",  remarks    ?? (object)DBNull.Value),
                new MySqlParameter("?by",   userId));
            return Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
        }

        public static void AddShipmentLine(int shipmentId, int productId, decimal qty)
        {
            ExecuteNonQuery(
                "INSERT INTO PK_ShipmentLine (ShipmentID,ProductID,QtyShipped)" +
                " VALUES(?sid,?pid,?qty);",
                new MySqlParameter("?sid", shipmentId),
                new MySqlParameter("?pid", productId),
                new MySqlParameter("?qty", qty));
            // Update PO line shipped qty
            ExecuteNonQuery(
                "UPDATE PK_CustomerPOLine SET QtyShipped = QtyShipped + ?qty" +
                " WHERE POID = (SELECT POID FROM PK_Shipment WHERE ShipmentID=?sid)" +
                " AND ProductID=?pid;",
                new MySqlParameter("?qty", qty),
                new MySqlParameter("?sid", shipmentId),
                new MySqlParameter("?pid", productId));
        }

        private static string GenerateShipCode()
        {
            object last = ExecuteScalar(
                "SELECT ShipmentCode FROM PK_Shipment ORDER BY ShipmentID DESC LIMIT 1;");
            if (last == null || last == DBNull.Value)
                return "SHIP-" + DateTime.Now.Year + "-001";
            var m = System.Text.RegularExpressions.Regex.Match(last.ToString(), @"\d+$");
            int n = m.Success ? int.Parse(m.Value) + 1 : 1;
            return "SHIP-" + DateTime.Now.Year + "-" + n.ToString("D3");
        }

        // ── REPORTS ──────────────────────────────────────────────────────────
        public static DataTable GetFGStockSummary()
        {
            // Stock Lifecycle:
            //   Opening Stock (JAR/BOX + CASE) from PK_FGOpeningStock
            //   Primary Packing → SFG (PCS in PK_FGStock)
            //   JARs filled     → FG JARs = PK_FGStock PCS / UnitSize (pcs per jar)
            //   Case packing    → consumes JARs, creates Cases
            //
            // FG Loose JARs  = Opening JARs + (Total JARs packed − JARs used in cases)
            // FG Cases        = Opening Cases + Cases packed − Cases dispatched (FINALISED DCs)
            // Reserved Cases  = Cases in DRAFT DCs
            // Avail for DC    = FG Cases − Reserved Cases
            return ExecuteQuery(
                "SELECT p.ProductID, p.ProductCode, p.ProductName," +
                " p.ContainerType, IFNULL(p.ContainersPerCase,12) AS ContainersPerCase," +
                " CAST(SUBSTRING_INDEX(IFNULL(p.UnitsPerContainer,'1'),',',1) AS UNSIGNED) AS UnitSize," +

                // Opening stock
                " IFNULL(osJar.OpeningJars, 0) AS OpeningJars," +
                " IFNULL(osCase.OpeningCases, 0) AS OpeningCases," +

                // Total JARs from primary packing
                " ROUND(IFNULL(fg.TotalPCS, 0), 0) AS TotalPCS," +
                " FLOOR(IFNULL(fg.TotalPCS, 0)" +
                "  / GREATEST(CAST(SUBSTRING_INDEX(IFNULL(p.UnitsPerContainer,'1'),',',1) AS UNSIGNED), 1)" +
                ") AS TotalJarsPacked," +

                // JARs consumed by secondary packing
                " ROUND(IFNULL(sp.JarsUsedInCases, 0), 0) AS JarsUsedInCases," +

                // FG Loose JARs = Opening + Packed − Used in cases
                " IFNULL(osJar.OpeningJars, 0)" +
                " + FLOOR(IFNULL(fg.TotalPCS, 0)" +
                "   / GREATEST(CAST(SUBSTRING_INDEX(IFNULL(p.UnitsPerContainer,'1'),',',1) AS UNSIGNED), 1))" +
                " - IFNULL(sp.JarsUsedInCases, 0)" +
                " AS FGLooseJars," +

                // Cases packed
                " ROUND(IFNULL(sp.CasesPacked, 0), 0) AS CasesPacked," +

                // Cases dispatched
                " IFNULL(dcFinal.CasesDispatched, 0) AS CasesDispatched," +

                // FG Cases = Opening + Packed − Dispatched
                " IFNULL(osCase.OpeningCases, 0) + IFNULL(sp.CasesPacked, 0) - IFNULL(dcFinal.CasesDispatched, 0) AS FGCases," +

                // Reserved
                " IFNULL(dcDraft.CasesReserved, 0) AS CasesReserved," +

                // Available for DC
                " IFNULL(osCase.OpeningCases, 0) + IFNULL(sp.CasesPacked, 0) - IFNULL(dcFinal.CasesDispatched, 0) - IFNULL(dcDraft.CasesReserved, 0) AS AvailableForDC" +

                " FROM PP_Products p" +

                // Opening stock JARs/BOXes
                " LEFT JOIN (" +
                "   SELECT ProductID, SUM(Quantity) AS OpeningJars" +
                "   FROM PK_FGOpeningStock WHERE StockForm IN ('JAR','BOX')" +
                "   GROUP BY ProductID" +
                " ) osJar ON osJar.ProductID = p.ProductID" +

                // Opening stock CASEs
                " LEFT JOIN (" +
                "   SELECT ProductID, SUM(Quantity) AS OpeningCases" +
                "   FROM PK_FGOpeningStock WHERE StockForm = 'CASE'" +
                "   GROUP BY ProductID" +
                " ) osCase ON osCase.ProductID = p.ProductID" +

                // Total PCS from primary packing
                " LEFT JOIN (" +
                "   SELECT ProductID, SUM(QtyPacked) AS TotalPCS" +
                "   FROM PK_FGStock GROUP BY ProductID" +
                " ) fg ON fg.ProductID = p.ProductID" +

                // Secondary packing
                " LEFT JOIN (" +
                "   SELECT ProductID," +
                "     SUM(TotalUnits) AS JarsUsedInCases," +
                "     SUM(CASE WHEN PackingType='CASE' THEN QtyCartons ELSE 0 END) AS CasesPacked" +
                "   FROM PK_SecondaryPacking GROUP BY ProductID" +
                " ) sp ON sp.ProductID = p.ProductID" +

                // FINALISED DC cases
                " LEFT JOIN (" +
                "   SELECT dl.ProductID, SUM(dl.Cases) AS CasesDispatched" +
                "   FROM PK_DCLines dl" +
                "   JOIN PK_DeliveryChallans dc ON dc.DCID = dl.DCID" +
                "   WHERE dc.Status = 'FINALISED'" +
                "   GROUP BY dl.ProductID" +
                " ) dcFinal ON dcFinal.ProductID = p.ProductID" +

                // DRAFT DC cases
                " LEFT JOIN (" +
                "   SELECT dl.ProductID, SUM(dl.Cases) AS CasesReserved" +
                "   FROM PK_DCLines dl" +
                "   JOIN PK_DeliveryChallans dc ON dc.DCID = dl.DCID" +
                "   WHERE dc.Status = 'DRAFT'" +
                "   GROUP BY dl.ProductID" +
                " ) dcDraft ON dcDraft.ProductID = p.ProductID" +

                " WHERE p.IsActive=1 AND p.ProductType='Core'" +
                " ORDER BY p.ProductName;");
        }

        // ── PACKING MATERIALS (from MM) ───────────────────────────────────────
        public static DataTable GetActivePackingMaterials()
        {
            return ExecuteQuery(
                "SELECT pm.PMID, pm.PMCode, pm.PMName, u.Abbreviation," +
                " ROUND(IFNULL(os.Quantity,0) + IFNULL(grn.TotalGRN,0) - IFNULL(con.TotalUsed,0), 4) AS CurrentStock" +
                " FROM MM_PackingMaterials pm" +
                " JOIN MM_UOM u ON u.UOMID=pm.UOMID" +
                " LEFT JOIN (SELECT MaterialID, Quantity FROM MM_OpeningStock" +
                "   WHERE MaterialType='PM') os ON os.MaterialID=pm.PMID" +
                " LEFT JOIN (SELECT PMID, SUM(QtyActualReceived) AS TotalGRN" +
                "   FROM MM_PackingInward GROUP BY PMID) grn ON grn.PMID=pm.PMID" +
                " LEFT JOIN (SELECT PMID, SUM(QtyUsed) AS TotalUsed" +
                "   FROM PK_PMConsumption GROUP BY PMID) con ON con.PMID=pm.PMID" +
                " WHERE pm.IsActive=1 ORDER BY pm.PMName;");
        }

        public static DataTable GetPackingMaterialsByCategory(string category)
        {
            return ExecuteQuery(
                "SELECT pm.PMID, pm.PMCode, pm.PMName, u.Abbreviation," +
                " ROUND(IFNULL(os.Quantity,0) + IFNULL(grn.TotalGRN,0) - IFNULL(con.TotalUsed,0), 4) AS CurrentStock" +
                " FROM MM_PackingMaterials pm" +
                " JOIN MM_UOM u ON u.UOMID=pm.UOMID" +
                " LEFT JOIN (SELECT MaterialID, Quantity FROM MM_OpeningStock" +
                "   WHERE MaterialType='PM') os ON os.MaterialID=pm.PMID" +
                " LEFT JOIN (SELECT PMID, SUM(QtyActualReceived) AS TotalGRN" +
                "   FROM MM_PackingInward GROUP BY PMID) grn ON grn.PMID=pm.PMID" +
                " LEFT JOIN (SELECT PMID, SUM(QtyUsed) AS TotalUsed" +
                "   FROM PK_PMConsumption GROUP BY PMID) con ON con.PMID=pm.PMID" +
                " WHERE pm.IsActive=1 AND pm.PMCategory=?cat ORDER BY pm.PMName;",
                new MySqlParameter("?cat", category));
        }

        /// Get current stock for a specific PM by ID.
        public static decimal GetPMCurrentStock(int pmId)
        {
            object result = ExecuteScalar(
                "SELECT ROUND(" +
                "  IFNULL((SELECT SUM(QtyActualReceived) FROM MM_PackingInward WHERE PMID=?pid), 0)" +
                "  + IFNULL((SELECT Quantity FROM MM_OpeningStock WHERE MaterialType='PM' AND MaterialID=?pid), 0)" +
                "  - IFNULL((SELECT SUM(QtyUsed) FROM PK_PMConsumption WHERE PMID=?pid), 0)" +
                ", 4);",
                new MySqlParameter("?pid", pmId));
            return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        // ── PRODUCTS (from PP) ────────────────────────────────────────────────
        public static DataTable GetActiveProducts()
        {
            return ExecuteQuery(
                "SELECT p.ProductID, p.ProductCode, p.ProductName, ou.Abbreviation AS Unit" +
                " FROM PP_Products p JOIN MM_UOM ou ON ou.UOMID=p.OutputUOMID" +
                " WHERE p.IsActive=1 ORDER BY p.ProductName;");
        }

        // ── PRODUCT PM MAPPING (PK_ProductPMMaster) ──────────────────────

        /// Products list with count of active PM mappings — for the left panel.
        public static DataTable GetProductsWithPMCount()
        {
            return ExecuteQuery(
                "SELECT p.ProductID, p.ProductCode, p.ProductName," +
                " IFNULL(p.ContainerType,'DIRECT') AS ContainerType," +
                " (SELECT COUNT(*) FROM PK_ProductPMMaster m" +
                "  WHERE m.ProductID=p.ProductID AND m.IsActive=1) AS PMCount" +
                " FROM PP_Products p" +
                " WHERE p.IsActive=1 AND p.ProductType='Core'" +
                " ORDER BY p.ProductName;");
        }

        /// Single product info for the mapping page header.
        public static DataRow GetProductForPMMapping(int productId)
        {
            return ExecuteQueryRow(
                "SELECT p.ProductID, p.ProductCode, p.ProductName," +
                " IFNULL(p.ContainerType,'DIRECT') AS ContainerType," +
                " p.UnitsPerContainer, p.ContainersPerCase," +
                " IFNULL(p.HasLanguageLabels,0) AS HasLanguageLabels" +
                " FROM PP_Products p WHERE p.ProductID=?pid;",
                new MySqlParameter("?pid", productId));
        }

        /// All active PM mappings for a product — joined with PM master for display.
        public static DataTable GetProductPMMappings(int productId)
        {
            return GetProductPMMappings(productId, null);
        }

        public static DataTable GetProductPMMappings(int productId, string excludeLevel)
        {
            string levelFilter = "";
            if (!string.IsNullOrEmpty(excludeLevel))
                levelFilter = " AND m.ApplyLevel != '" + excludeLevel + "'";
            return ExecuteQuery(
                "SELECT m.MappingID, m.PMID, m.QtyPerUnit, m.ApplyLevel, m.Language," +
                " pm.PMCode, pm.PMName, u.Abbreviation," +
                " ROUND(IFNULL(os.Quantity,0) + IFNULL(grn.TotalGRN,0) - IFNULL(con.TotalUsed,0), 4) AS CurrentStock" +
                " FROM PK_ProductPMMaster m" +
                " JOIN MM_PackingMaterials pm ON pm.PMID = m.PMID" +
                " JOIN MM_UOM u ON u.UOMID = pm.UOMID" +
                " LEFT JOIN (SELECT MaterialID, Quantity FROM MM_OpeningStock" +
                "   WHERE MaterialType='PM') os ON os.MaterialID = m.PMID" +
                " LEFT JOIN (SELECT PMID, SUM(QtyActualReceived) AS TotalGRN" +
                "   FROM MM_PackingInward GROUP BY PMID) grn ON grn.PMID = m.PMID" +
                " LEFT JOIN (SELECT PMID, SUM(QtyUsed) AS TotalUsed" +
                "   FROM PK_PMConsumption GROUP BY PMID) con ON con.PMID = m.PMID" +
                " WHERE m.ProductID=?pid AND m.IsActive=1" + levelFilter +
                " ORDER BY FIELD(m.ApplyLevel,'CONTAINER','UNIT'), m.Language, pm.PMName;",
                new MySqlParameter("?pid", productId));
        }

        /// Single mapping row for Edit.
        public static DataRow GetProductPMMappingById(int mappingId)
        {
            return ExecuteQueryRow(
                "SELECT * FROM PK_ProductPMMaster WHERE MappingID=?id;",
                new MySqlParameter("?id", mappingId));
        }

        /// Check if a mapping already exists for same product+PM+level+language.
        public static bool ProductPMMappingExists(int productId, int pmId, string level, string language = null)
        {
            if (string.IsNullOrEmpty(language))
            {
                object cnt = ExecuteScalar(
                    "SELECT COUNT(*) FROM PK_ProductPMMaster" +
                    " WHERE ProductID=?pid AND PMID=?pmid AND ApplyLevel=?lvl AND Language IS NULL AND IsActive=1;",
                    new MySqlParameter("?pid",  productId),
                    new MySqlParameter("?pmid", pmId),
                    new MySqlParameter("?lvl",  level));
                return Convert.ToInt32(cnt) > 0;
            }
            else
            {
                object cnt = ExecuteScalar(
                    "SELECT COUNT(*) FROM PK_ProductPMMaster" +
                    " WHERE ProductID=?pid AND PMID=?pmid AND ApplyLevel=?lvl AND Language=?lang AND IsActive=1;",
                    new MySqlParameter("?pid",  productId),
                    new MySqlParameter("?pmid", pmId),
                    new MySqlParameter("?lvl",  level),
                    new MySqlParameter("?lang", language));
                return Convert.ToInt32(cnt) > 0;
            }
        }

        public static void AddProductPMMapping(int productId, int pmId,
            decimal qtyPerUnit, string level, int userId, string language = null)
        {
            ExecuteNonQuery(
                "INSERT INTO PK_ProductPMMaster" +
                " (ProductID, PMID, QtyPerUnit, ApplyLevel, Language, IsActive, CreatedBy, CreatedAt)" +
                " VALUES(?pid, ?pmid, ?qty, ?lvl, ?lang, 1, ?by, NOW());",
                new MySqlParameter("?pid",  productId),
                new MySqlParameter("?pmid", pmId),
                new MySqlParameter("?qty",  qtyPerUnit),
                new MySqlParameter("?lvl",  level),
                new MySqlParameter("?lang", string.IsNullOrEmpty(language) ? (object)DBNull.Value : language),
                new MySqlParameter("?by",   userId));
        }

        public static void UpdateProductPMMapping(int mappingId, int pmId,
            decimal qtyPerUnit, string level, string language = null)
        {
            ExecuteNonQuery(
                "UPDATE PK_ProductPMMaster SET PMID=?pmid, QtyPerUnit=?qty," +
                " ApplyLevel=?lvl, Language=?lang, UpdatedAt=NOW() WHERE MappingID=?id;",
                new MySqlParameter("?pmid", pmId),
                new MySqlParameter("?qty",  qtyPerUnit),
                new MySqlParameter("?lvl",  level),
                new MySqlParameter("?lang", string.IsNullOrEmpty(language) ? (object)DBNull.Value : language),
                new MySqlParameter("?id",   mappingId));
        }

        public static void DeleteProductPMMapping(int mappingId)
        {
            ExecuteNonQuery(
                "UPDATE PK_ProductPMMaster SET IsActive=0, UpdatedAt=NOW() WHERE MappingID=?id;",
                new MySqlParameter("?id", mappingId));
        }

        // ── PM CONSUMPTION CALCULATION ───────────────────────────────────

        /// Calculate PM consumption for a given packing output.
        /// Returns a DataTable with columns: MappingID, PMID, PMName, PMCode,
        ///   ApplyLevel, Language, QtyPerUnit, CalculatedQty, Abbreviation, CurrentStock
        /// Only includes universal PMs (Language IS NULL) and PMs matching selectedLanguage.
        public static DataTable CalculatePMConsumption(int productId,
            int cases, int jars, int loosePcs,
            int unitsPerContainer, int containersPerCase, string containerType,
            string selectedLanguage = null)
        {
            var allMappings = GetProductPMMappings(productId, "CASE");
            allMappings.Columns.Add("CalculatedQty", typeof(decimal));

            // Filter: keep universal PMs (Language IS NULL) + PMs matching selected language
            var rowsToRemove = new System.Collections.Generic.List<DataRow>();
            foreach (DataRow row in allMappings.Rows)
            {
                string pmLang = row["Language"] == DBNull.Value ? null : row["Language"].ToString();
                if (pmLang != null && pmLang != selectedLanguage)
                    rowsToRemove.Add(row);
            }
            foreach (var r in rowsToRemove) allMappings.Rows.Remove(r);

            foreach (DataRow row in allMappings.Rows)
            {
                string level     = row["ApplyLevel"].ToString();
                decimal qtyPer   = Convert.ToDecimal(row["QtyPerUnit"]);
                decimal calcQty  = 0;

                switch (level)
                {
                    case "UNIT":
                        if (containerType == "DIRECT")
                            calcQty = ((decimal)cases * unitsPerContainer + loosePcs) * qtyPer;
                        else
                            calcQty = ((decimal)cases * containersPerCase * unitsPerContainer
                                     + (decimal)jars * unitsPerContainer
                                     + loosePcs) * qtyPer;
                        break;

                    case "CONTAINER":
                        if (containerType == "DIRECT")
                            calcQty = (decimal)cases * qtyPer;
                        else
                            calcQty = ((decimal)cases * containersPerCase + jars) * qtyPer;
                        break;

                    case "CASE":
                        calcQty = (decimal)cases * qtyPer;
                        break;
                }

                row["CalculatedQty"] = Math.Round(calcQty, 4);
            }

            return allMappings;
        }

        /// Record PM consumption entries from the packing output save.
        /// consumptionData: rows with PMID and ActualQty columns.
        public static void RecordPMConsumptionBatch(int orderId, int productId,
            DataTable consumptionData, int userId)
        {
            foreach (DataRow row in consumptionData.Rows)
            {
                int     pmId    = Convert.ToInt32(row["PMID"]);
                decimal qtyUsed = Convert.ToDecimal(row["ActualQty"]);
                if (qtyUsed <= 0) continue;

                ExecuteNonQuery(
                    "INSERT INTO PK_PMConsumption" +
                    " (PMID, QtyUsed, UsedAt, SourceType, SourceID, CreatedBy)" +
                    " VALUES(?pmid, ?qty, ?now, 'PRIMARY_AUTO', ?oid, ?by);",
                    new MySqlParameter("?pmid", pmId),
                    new MySqlParameter("?qty",  qtyUsed),
                    new MySqlParameter("?now",  NowIST()),
                    new MySqlParameter("?oid",  orderId),
                    new MySqlParameter("?by",   userId));
            }
        }

        /// <summary>
        /// Get batch count per language for an order.
        /// Returns rows: Language (nullable), BatchCount.
        /// Used to split language-specific PM consumption proportionally.
        /// </summary>
        public static DataTable GetBatchLanguageSplit(int orderId)
        {
            return ExecuteQuery(
                "SELECT LabelLanguage AS Language, COUNT(*) AS BatchCount" +
                " FROM PK_PackingExecution" +
                " WHERE OrderID=?oid AND BatchNo > 0 AND Status='Completed'" +
                " GROUP BY LabelLanguage;",
                new MySqlParameter("?oid", orderId));
        }

        /// <summary>
        /// Calculate PM consumption for packing output, handling language-specific PMs.
        /// For universal PMs: uses full output.
        /// For language PMs: splits proportionally by batch language ratio.
        /// Returns DataTable with: PMID, PMName, PMCode, ApplyLevel, Language,
        ///   QtyPerUnit, CalculatedQty, Abbreviation, CurrentStock
        /// </summary>
        public static DataTable CalculatePMConsumptionWithLanguage(int productId,
            int orderId, int jars, int loosePcs,
            int unitsPerContainer, int containersPerCase, string containerType)
        {
            var allMappings = GetProductPMMappings(productId, "CASE");
            allMappings.Columns.Add("CalculatedQty", typeof(decimal));

            // Get language split from batch history
            var langSplit = GetBatchLanguageSplit(orderId);
            int totalBatches = 0;
            var batchCounts = new System.Collections.Generic.Dictionary<string, int>();
            foreach (DataRow lr in langSplit.Rows)
            {
                string lang = lr["Language"] == DBNull.Value ? "" : lr["Language"].ToString();
                int cnt = Convert.ToInt32(lr["BatchCount"]);
                batchCounts[lang] = cnt;
                totalBatches += cnt;
            }
            if (totalBatches == 0) totalBatches = 1; // avoid div by zero

            foreach (DataRow row in allMappings.Rows)
            {
                string level   = row["ApplyLevel"].ToString();
                decimal qtyPer = Convert.ToDecimal(row["QtyPerUnit"]);
                string pmLang  = row["Language"] == DBNull.Value ? null : row["Language"].ToString();

                // Base quantity from output
                decimal baseQty = 0;
                switch (level)
                {
                    case "UNIT":
                        baseQty = ((decimal)jars * unitsPerContainer + loosePcs) * qtyPer;
                        break;
                    case "CONTAINER":
                        baseQty = (decimal)jars * qtyPer;
                        break;
                    case "CASE":
                        baseQty = 0; // cases handled in secondary packing
                        break;
                }

                if (pmLang != null)
                {
                    // Language-specific PM: proportion by batch count
                    int langBatches = 0;
                    batchCounts.TryGetValue(pmLang, out langBatches);
                    baseQty = Math.Round(baseQty * langBatches / totalBatches, 4);
                }

                row["CalculatedQty"] = Math.Round(baseQty, 4);
            }

            return allMappings;
        }

        // ── PM STOCK REPORT (for PK module) ──────────────────────────────
        public static DataTable GetPMStockReport()
        {
            return ExecuteQuery(
                "SELECT p.PMID, p.PMCode, p.PMName, u.Abbreviation AS UOM," +
                " ROUND(" +
                "   IFNULL(os.Quantity, 0)" +
                "   + IFNULL(grn.TotalReceived, 0)" +
                "   - IFNULL(con.TotalConsumed, 0)" +
                " , 4) AS CurrentStock," +
                " IFNULL(os.Quantity, 0) AS OpeningStock," +
                " IFNULL(grn.TotalReceived, 0) AS TotalReceived," +
                " IFNULL(con.TotalConsumed, 0) AS TotalConsumed," +
                " p.ReorderLevel" +
                " FROM MM_PackingMaterials p" +
                " JOIN MM_UOM u ON u.UOMID = p.UOMID" +
                " LEFT JOIN MM_OpeningStock os ON os.MaterialType='PM' AND os.MaterialID=p.PMID" +
                " LEFT JOIN (SELECT PMID, SUM(QtyActualReceived) AS TotalReceived" +
                "            FROM MM_PackingInward GROUP BY PMID) grn ON grn.PMID = p.PMID" +
                " LEFT JOIN (SELECT PMID, SUM(QtyUsed) AS TotalConsumed" +
                "            FROM PK_PMConsumption GROUP BY PMID) con ON con.PMID = p.PMID" +
                " WHERE p.IsActive = 1" +
                " ORDER BY CurrentStock DESC, p.PMName ASC;");
        }

        // ── PACKING HISTORY REPORT ───────────────────────────────────────
        public static DataTable GetPackingHistoryReport(int productId, DateTime fromDate, DateTime toDate)
        {
            // Returns order-level summary rows
            string sql =
                "SELECT po.OrderID, po.OrderDate, po.Shift," +
                " p.ProductName, p.ProductCode, p.ContainerType," +
                " IFNULL(summary.Jars, 0) AS OrderJars," +
                " IFNULL(summary.Units, 0) AS OrderPcs," +
                " IFNULL(summary.JarSize, 0) AS JarSize," +
                " IFNULL(summary.TotalUnits, 0) AS OrderTotalUnits," +
                " (SELECT COUNT(*) FROM PK_PackingExecution b" +
                "  WHERE b.OrderID=po.OrderID AND b.BatchNo > 0 AND b.Status='Completed') AS BatchCount," +
                " (SELECT MIN(pe2.StartTime) FROM PK_PackingExecution pe2" +
                "  WHERE pe2.OrderID=po.OrderID AND pe2.BatchNo > 0) AS FirstStart," +
                " (SELECT MAX(pe3.EndTime) FROM PK_PackingExecution pe3" +
                "  WHERE pe3.OrderID=po.OrderID AND pe3.BatchNo > 0) AS LastEnd" +
                " FROM PP_ProductionOrder po" +
                " JOIN PP_Products p ON p.ProductID = po.ProductID" +
                " LEFT JOIN PK_PackingExecution summary" +
                "   ON summary.OrderID = po.OrderID AND summary.BatchNo = 0" +
                " WHERE po.OrderID IN (" +
                "   SELECT DISTINCT pe.OrderID FROM PK_PackingExecution pe" +
                "   WHERE pe.BatchNo > 0 AND pe.Status IN ('Completed','InProgress')" +
                "   AND DATE(pe.StartTime) >= ?from AND DATE(pe.StartTime) <= ?to" +
                " )";

            if (productId > 0)
                sql += " AND po.ProductID = ?pid";

            sql += " ORDER BY po.OrderID DESC;";

            if (productId > 0)
                return ExecuteQuery(sql,
                    new MySqlParameter("?from", fromDate.Date),
                    new MySqlParameter("?to",   toDate.Date),
                    new MySqlParameter("?pid",  productId));
            else
                return ExecuteQuery(sql,
                    new MySqlParameter("?from", fromDate.Date),
                    new MySqlParameter("?to",   toDate.Date));
        }

        public static DataTable GetPackingBatchesByOrder(int orderId)
        {
            return ExecuteQuery(
                "SELECT pe.PackingID, pe.BatchNo, pe.StartTime, pe.EndTime," +
                " pe.Status, pe.LabelLanguage" +
                " FROM PK_PackingExecution pe" +
                " WHERE pe.OrderID=?oid AND pe.BatchNo > 0" +
                " ORDER BY pe.BatchNo ASC;",
                new MySqlParameter("?oid", orderId));
        }

        /// Products that have had packing activity — for the report dropdown.
        public static DataTable GetProductsWithPackingHistory()
        {
            return ExecuteQuery(
                "SELECT DISTINCT p.ProductID, p.ProductCode, p.ProductName" +
                " FROM PK_PackingExecution pe" +
                " JOIN PP_ProductionOrder po ON po.OrderID = pe.OrderID" +
                " JOIN PP_Products p ON p.ProductID = po.ProductID" +
                " WHERE pe.BatchNo > 0" +
                " ORDER BY p.ProductName;");
        }

        // ── DELIVERY CHALLANS ─────────────────────────────────────────────────

        /// Generate DC number: DC-2526-001 (FY year prefix)
        private static string GenerateDCNumber()
        {
            DateTime now = NowIST();
            int fy1 = now.Month >= 4 ? now.Year % 100 : (now.Year - 1) % 100;
            int fy2 = fy1 + 1;
            string prefix = "DC-" + fy1.ToString("D2") + fy2.ToString("D2") + "-";
            object last = ExecuteScalar(
                "SELECT DCNumber FROM PK_DeliveryChallans WHERE DCNumber LIKE ?pat ORDER BY DCID DESC LIMIT 1;",
                new MySqlParameter("?pat", prefix + "%"));
            int n = 1;
            if (last != null && last != DBNull.Value)
            {
                var m = System.Text.RegularExpressions.Regex.Match(last.ToString(), @"\d+$");
                if (m.Success) n = int.Parse(m.Value) + 1;
            }
            return prefix + n.ToString("D3");
        }

        /// Get FG stock available for shipment.
        /// FG = Secondary Packing output (in jars) minus already shipped DC lines (in jars).
        public static DataTable GetFGStockForShipment()
        {
            // FG has two separate pools + opening stock:
            //   1. Loose JARs = Opening JAR/BOX + Primary packed JARs − JARs in cases − Loose in DCs
            //   2. Cases = Opening CASE + Cases packed − Cases in DCs (DRAFT+FINALISED)
            // These are NOT interchangeable — loose JARs cannot become cases without
            // going through secondary packing (which consumes packing materials).
            return ExecuteQuery(
                "SELECT p.ProductID, p.ProductName, p.ProductCode," +
                " p.HSNCode, IFNULL(p.GSTRate, 0) AS GSTRate," +
                " IFNULL(p.ContainersPerCase, 12) AS ContainersPerCase," +
                " IFNULL(p.UnitsPerContainer, '1') AS UnitsPerContainer," +
                " IFNULL(p.ContainerType, 'DIRECT') AS ContainerType," +
                " ou.Abbreviation AS Unit," +

                // Loose JARs available = Opening + (Primary/UnitSize − CasePacked) − DC Loose allocation
                " IFNULL(osJar.OpeningJars, 0)" +
                " + FLOOR(IFNULL(fg.TotalPCS, 0)" +
                "   / GREATEST(CAST(SUBSTRING_INDEX(IFNULL(p.UnitsPerContainer,'1'),',',1) AS UNSIGNED), 1))" +
                " - IFNULL(sp.JarsInCases, 0)" +
                " - IFNULL(dcAlloc.TotalLooseJars, 0)" +
                " AS AvailableLooseJars," +

                // Cases available = Opening + Packed − DC Cases allocation
                " IFNULL(osCase.OpeningCases, 0)" +
                " + IFNULL(sp.CasesPacked, 0)" +
                " - IFNULL(dcAlloc.TotalCases, 0)" +
                " AS AvailableCases," +

                // Combined jar-equivalent for display
                " (IFNULL(osJar.OpeningJars, 0)" +
                "  + FLOOR(IFNULL(fg.TotalPCS, 0)" +
                "    / GREATEST(CAST(SUBSTRING_INDEX(IFNULL(p.UnitsPerContainer,'1'),',',1) AS UNSIGNED), 1))" +
                "  - IFNULL(sp.JarsInCases, 0)" +
                "  - IFNULL(dcAlloc.TotalLooseJars, 0))" +
                " + (IFNULL(osCase.OpeningCases, 0) + IFNULL(sp.CasesPacked, 0) - IFNULL(dcAlloc.TotalCases, 0))" +
                "   * IFNULL(p.ContainersPerCase, 12)" +
                " AS AvailableFGJars" +

                " FROM PP_Products p" +
                " JOIN MM_UOM ou ON ou.UOMID = p.OutputUOMID" +

                // Opening stock JAR/BOX
                " LEFT JOIN (SELECT ProductID, SUM(Quantity) AS OpeningJars" +
                "   FROM PK_FGOpeningStock WHERE StockForm IN ('JAR','BOX')" +
                "   GROUP BY ProductID) osJar ON osJar.ProductID = p.ProductID" +

                // Opening stock CASE
                " LEFT JOIN (SELECT ProductID, SUM(Quantity) AS OpeningCases" +
                "   FROM PK_FGOpeningStock WHERE StockForm = 'CASE'" +
                "   GROUP BY ProductID) osCase ON osCase.ProductID = p.ProductID" +

                // Primary packing total PCS
                " LEFT JOIN (SELECT ProductID, SUM(QtyPacked) AS TotalPCS" +
                "   FROM PK_FGStock GROUP BY ProductID) fg ON fg.ProductID = p.ProductID" +

                // Secondary packing
                " LEFT JOIN (SELECT ProductID," +
                "   SUM(TotalUnits) AS JarsInCases," +
                "   SUM(CASE WHEN PackingType='CASE' THEN QtyCartons ELSE 0 END) AS CasesPacked" +
                "   FROM PK_SecondaryPacking GROUP BY ProductID) sp ON sp.ProductID = p.ProductID" +

                // DC stock allocation based on Source column
                // Source='CASE' → deduct CEIL(TotalPcs/ContainersPerCase) from cases pool
                // Source='LOOSE' → deduct TotalPcs from loose pool
                " LEFT JOIN (SELECT dl.ProductID," +
                "   SUM(CASE WHEN IFNULL(dl.Source,'CASE') = 'CASE'" +
                "     THEN CEIL(dl.TotalPcs / GREATEST(IFNULL(pp.ContainersPerCase,12),1))" +
                "     ELSE 0 END) AS TotalCases," +
                "   SUM(CASE WHEN IFNULL(dl.Source,'CASE') = 'LOOSE'" +
                "     THEN dl.TotalPcs ELSE 0 END) AS TotalLooseJars" +
                "   FROM PK_DCLines dl" +
                "   JOIN PK_DeliveryChallans dch ON dch.DCID = dl.DCID" +
                "   JOIN PP_Products pp ON pp.ProductID = dl.ProductID" +
                "   WHERE dch.Status IN ('DRAFT','FINALISED')" +
                "   GROUP BY dl.ProductID) dcAlloc ON dcAlloc.ProductID = p.ProductID" +

                " WHERE p.IsActive=1 AND p.ProductType IN ('Core','Conversion','Prefilled Conversion')" +
                " AND (IFNULL(fg.TotalPCS, 0) > 0 OR IFNULL(osJar.OpeningJars, 0) > 0 OR IFNULL(osCase.OpeningCases, 0) > 0)" +
                " ORDER BY p.ProductName;");
        }

        /// Create a new DC (DRAFT)
        public static int CreateDeliveryChallan(int customerId, DateTime dcDate, string remarks, int userId, int consignmentId = 0)
        {
            string dcNumber = GenerateDCNumber();
            if (consignmentId > 0)
            {
                ExecuteNonQuery(
                    "INSERT INTO PK_DeliveryChallans (DCNumber, ConsignmentID, CustomerID, DCDate, Status, Remarks, CreatedBy, CreatedAt)" +
                    " VALUES(?num, ?csg, ?cid, ?dt, 'DRAFT', ?rem, ?by, ?now);",
                    new MySqlParameter("?num", dcNumber),
                    new MySqlParameter("?csg", consignmentId),
                    new MySqlParameter("?cid", customerId),
                    new MySqlParameter("?dt",  dcDate),
                    new MySqlParameter("?rem", remarks ?? (object)DBNull.Value),
                    new MySqlParameter("?by",  userId),
                    new MySqlParameter("?now", NowIST()));
            }
            else
            {
                ExecuteNonQuery(
                    "INSERT INTO PK_DeliveryChallans (DCNumber, CustomerID, DCDate, Status, Remarks, CreatedBy, CreatedAt)" +
                    " VALUES(?num, ?cid, ?dt, 'DRAFT', ?rem, ?by, ?now);",
                    new MySqlParameter("?num", dcNumber),
                    new MySqlParameter("?cid", customerId),
                    new MySqlParameter("?dt",  dcDate),
                    new MySqlParameter("?rem", remarks ?? (object)DBNull.Value),
                    new MySqlParameter("?by",  userId),
                    new MySqlParameter("?now", NowIST()));
            }
            return Convert.ToInt32(Convert.ToInt64(ExecuteScalar("SELECT LAST_INSERT_ID();")));
        }

        /// Add a line item to a DC
        public static void AddDCLine(int dcId, int productId, int cases, int looseJars, int jarsPerCase, int totalPcs)
        {
            ExecuteNonQuery(
                "INSERT INTO PK_DCLines (DCID, ProductID, Cases, LooseJars, JarsPerCase, TotalPcs)" +
                " VALUES(?dcid, ?pid, ?cs, ?lj, ?jpc, ?tp);",
                new MySqlParameter("?dcid", dcId),
                new MySqlParameter("?pid",  productId),
                new MySqlParameter("?cs",   cases),
                new MySqlParameter("?lj",   looseJars),
                new MySqlParameter("?jpc",  jarsPerCase),
                new MySqlParameter("?tp",   totalPcs));
        }

        public static void AddDCLineWithPricing(int dcId, int productId, int cases, int looseJars,
            int jarsPerCase, int totalPcs, string hsn, decimal gstRate, decimal mrp, decimal marginPct,
            decimal unitRate, decimal taxableVal, decimal cgst, decimal sgst, decimal igst, decimal lineTotal)
        {
            ExecuteNonQuery(
                "INSERT INTO PK_DCLines (DCID, ProductID, Cases, LooseJars, JarsPerCase, TotalPcs," +
                " HSNCode, GSTRate, MRP, MarginPct, UnitRate, TaxableValue, CGSTAmt, SGSTAmt, IGSTAmt, LineTotal)" +
                " VALUES(?dcid, ?pid, ?cs, ?lj, ?jpc, ?tp, ?hsn, ?gst, ?mrp, ?mgn, ?rate, ?tax, ?cgst, ?sgst, ?igst, ?lt);",
                new MySqlParameter("?dcid", dcId),
                new MySqlParameter("?pid",  productId),
                new MySqlParameter("?cs",   cases),
                new MySqlParameter("?lj",   looseJars),
                new MySqlParameter("?jpc",  jarsPerCase),
                new MySqlParameter("?tp",   totalPcs),
                new MySqlParameter("?hsn",  hsn ?? ""),
                new MySqlParameter("?gst",  gstRate),
                new MySqlParameter("?mrp",  mrp),
                new MySqlParameter("?mgn",  marginPct),
                new MySqlParameter("?rate", unitRate),
                new MySqlParameter("?tax",  taxableVal),
                new MySqlParameter("?cgst", cgst),
                new MySqlParameter("?sgst", sgst),
                new MySqlParameter("?igst", igst),
                new MySqlParameter("?lt",   lineTotal));
        }

        public static void UpdateDCPricing(int dcId, string channel, string custGSTIN, bool isInterState,
            decimal subTotal, decimal totalCGST, decimal totalSGST, decimal totalIGST, decimal grandTotal)
        {
            ExecuteNonQuery(
                "UPDATE PK_DeliveryChallans SET Channel=?ch, CustomerGSTIN=?gstin, IsInterState=?inter," +
                " SubTotal=?sub, TotalCGST=?cgst, TotalSGST=?sgst, TotalIGST=?igst, GrandTotal=?gt" +
                " WHERE DCID=?id;",
                new MySqlParameter("?ch",    channel),
                new MySqlParameter("?gstin", custGSTIN ?? ""),
                new MySqlParameter("?inter", isInterState ? 1 : 0),
                new MySqlParameter("?sub",   subTotal),
                new MySqlParameter("?cgst",  totalCGST),
                new MySqlParameter("?sgst",  totalSGST),
                new MySqlParameter("?igst",  totalIGST),
                new MySqlParameter("?gt",    grandTotal),
                new MySqlParameter("?id",    dcId));
        }

        public static string GenerateInvoiceNumber()
        {
            // Format: SI/2627/0001
            DateTime now = NowIST();
            int fyStartYear = now.Month >= 4 ? now.Year : now.Year - 1;
            string fyCode = (fyStartYear % 100).ToString("D2") + ((fyStartYear + 1) % 100).ToString("D2");

            // Get or create sequence
            ExecuteNonQuery(
                "INSERT INTO PK_InvoiceSequence (FYCode, LastNumber) VALUES(?fy, 0) " +
                "ON DUPLICATE KEY UPDATE FYCode=FYCode;",
                new MySqlParameter("?fy", fyCode));

            // Increment and get
            ExecuteNonQuery(
                "UPDATE PK_InvoiceSequence SET LastNumber=LastNumber+1 WHERE FYCode=?fy;",
                new MySqlParameter("?fy", fyCode));

            object val = ExecuteScalar(
                "SELECT LastNumber FROM PK_InvoiceSequence WHERE FYCode=?fy;",
                new MySqlParameter("?fy", fyCode));

            int seq = Convert.ToInt32(val);
            return "SI/" + fyCode + "/" + seq.ToString("D4");
        }

        public static void SetDCInvoiceNumber(int dcId, string invoiceNumber)
        {
            ExecuteNonQuery(
                "UPDATE PK_DeliveryChallans SET InvoiceNumber=?inv WHERE DCID=?id;",
                new MySqlParameter("?inv", invoiceNumber),
                new MySqlParameter("?id",  dcId));
        }

        /// Delete all lines for a DC (used when re-saving draft)
        public static void DeleteDCLines(int dcId)
        {
            ExecuteNonQuery("DELETE FROM PK_DCLines WHERE DCID=?dcid;",
                new MySqlParameter("?dcid", dcId));
        }

        public static void UpdateDCLineSellingForm(int dcId, int productId, string sellingForm, string source = "CASE")
        {
            ExecuteNonQuery(
                "UPDATE PK_DCLines SET SellingForm=?form, Source=?src WHERE DCID=?dcid AND ProductID=?pid ORDER BY LineID DESC LIMIT 1;",
                new MySqlParameter("?form", sellingForm ?? "JAR"),
                new MySqlParameter("?src", source ?? "CASE"),
                new MySqlParameter("?dcid", dcId),
                new MySqlParameter("?pid", productId));
        }

        /// Update DC header (draft mode)
        public static void UpdateDCHeader(int dcId, int customerId, DateTime dcDate, string remarks)
        {
            ExecuteNonQuery(
                "UPDATE PK_DeliveryChallans SET CustomerID=?cid, DCDate=?dt, Remarks=?rem WHERE DCID=?id AND Status='DRAFT';",
                new MySqlParameter("?cid", customerId),
                new MySqlParameter("?dt",  dcDate),
                new MySqlParameter("?rem", remarks ?? (object)DBNull.Value),
                new MySqlParameter("?id",  dcId));
        }

        /// Finalise a DC — locks it
        public static void FinaliseDeliveryChallan(int dcId, int userId)
        {
            ExecuteNonQuery(
                "UPDATE PK_DeliveryChallans SET Status='FINALISED', FinalisedAt=?now, FinalisedBy=?by WHERE DCID=?id AND Status='DRAFT';",
                new MySqlParameter("?now", NowIST()),
                new MySqlParameter("?by",  userId),
                new MySqlParameter("?id",  dcId));
        }

        /// Get DC header by ID
        public static DataRow GetDCById(int dcId)
        {
            return ExecuteQueryRow(
                "SELECT dc.*, c.CustomerName, c.CustomerCode, c.CustomerType" +
                " FROM PK_DeliveryChallans dc" +
                " JOIN PK_Customers c ON c.CustomerID = dc.CustomerID" +
                " WHERE dc.DCID=?id;",
                new MySqlParameter("?id", dcId));
        }

        /// Get DC lines for a DC
        public static DataTable GetDCLines(int dcId)
        {
            return ExecuteQuery(
                "SELECT dl.LineID, dl.ProductID, dl.SellingForm, dl.Source, p.ProductName, p.ProductCode," +
                " dl.Cases, dl.LooseJars, dl.JarsPerCase, dl.TotalPcs," +
                " dl.HSNCode, dl.GSTRate, dl.MRP, dl.MarginPct, dl.UnitRate," +
                " dl.TaxableValue, dl.CGSTAmt, dl.SGSTAmt, dl.IGSTAmt, dl.LineTotal," +
                " ou.Abbreviation AS Unit" +
                " FROM PK_DCLines dl" +
                " JOIN PP_Products p ON p.ProductID = dl.ProductID" +
                " JOIN MM_UOM ou ON ou.UOMID = p.OutputUOMID" +
                " WHERE dl.DCID=?dcid ORDER BY dl.LineID;",
                new MySqlParameter("?dcid", dcId));
        }

        /// Get recent DCs for listing
        public static DataTable GetRecentDCs(int limit = 50)
        {
            return ExecuteQuery(
                "SELECT dc.DCID, dc.DCNumber, dc.DCDate, dc.Status, dc.Remarks," +
                " c.CustomerName, c.CustomerCode," +
                " IFNULL(ct.TypeName,'') AS TypeName," +
                " IFNULL(csg.ConsignmentCode,'') AS ConsignmentCode," +
                " COUNT(dl.LineID) AS LineCount," +
                " SUM(dl.Cases) AS TotalCases," +
                " SUM(dl.TotalPcs) AS TotalPcs" +
                " FROM PK_DeliveryChallans dc" +
                " JOIN PK_Customers c ON c.CustomerID = dc.CustomerID" +
                " LEFT JOIN PK_CustomerTypes ct ON ct.TypeCode = c.CustomerType" +
                " LEFT JOIN PK_Consignments csg ON csg.ConsignmentID = dc.ConsignmentID" +
                " LEFT JOIN PK_DCLines dl ON dl.DCID = dc.DCID" +
                " GROUP BY dc.DCID" +
                " ORDER BY dc.DCID DESC LIMIT ?lim;",
                new MySqlParameter("?lim", limit));
        }

        /// Delete a DRAFT DC entirely
        public static void DeleteDraftDC(int dcId)
        {
            ExecuteNonQuery("DELETE FROM PK_DCLines WHERE DCID=?id;", new MySqlParameter("?id", dcId));
            ExecuteNonQuery("DELETE FROM PK_DeliveryChallans WHERE DCID=?id AND Status='DRAFT';", new MySqlParameter("?id", dcId));
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

        // ── SA SHIPMENT ORDERS (Sales Force → PK) ─────────────────────────

        /// <summary>Get all SA shipments visible to PK team (Status IN Order, DC, Shipped)</summary>
        public static DataTable GetSAShipmentOrders()
        {
            return ExecuteQuery(
                "SELECT sh.ShipmentID, sh.ShipmentDate, sh.Status, sh.VehicleNo," +
                " IFNULL(cust.CustomerName,'—') AS CustomerName," +
                " IFNULL(ar.AreaName,'—') AS AreaName," +
                " IFNULL(z.ZoneName,'—') AS ZoneName," +
                " IFNULL(r.RegionName,'—') AS RegionName," +
                " c.ChannelName," +
                " IFNULL(tm.ModeName,'—') AS TransportMode," +
                " COUNT(sl.LineID) AS ProductCount," +
                " IFNULL(SUM(sl.ShippedQty),0) AS TotalQty" +
                " FROM SA_Shipments sh" +
                " LEFT JOIN PK_Customers cust ON cust.CustomerID=sh.CustomerID" +
                " LEFT JOIN SA_Areas ar ON ar.AreaID=sh.PositionID" +
                " LEFT JOIN SA_Zones z ON z.ZoneID=sh.ZoneID" +
                " LEFT JOIN SA_Regions r ON r.RegionID=sh.RegionID" +
                " JOIN SA_Channels c ON c.ChannelID=sh.ChannelID" +
                " LEFT JOIN SA_TransportModes tm ON tm.ModeID=sh.TransportModeID" +
                " LEFT JOIN SA_ShipmentLines sl ON sl.ShipmentID=sh.ShipmentID" +
                " WHERE sh.Status IN ('Order','DC','Shipped')" +
                " GROUP BY sh.ShipmentID ORDER BY sh.ShipmentDate DESC;");
        }

        /// <summary>Get SA shipment orders for a specific consignment.</summary>
        public static DataTable GetSAShipmentOrdersByConsignment(int consignmentId)
        {
            return ExecuteQuery(
                "SELECT sh.ShipmentID, sh.ShipmentDate, sh.Status, sh.VehicleNo," +
                " IFNULL(cust.CustomerName,'—') AS CustomerName," +
                " IFNULL(ar.AreaName,'—') AS AreaName," +
                " IFNULL(z.ZoneName,'—') AS ZoneName," +
                " IFNULL(r.RegionName,'—') AS RegionName," +
                " c.ChannelName," +
                " IFNULL(tm.ModeName,'—') AS TransportMode," +
                " COUNT(sl.LineID) AS ProductCount," +
                " IFNULL(SUM(sl.ShippedQty),0) AS TotalQty" +
                " FROM SA_Shipments sh" +
                " LEFT JOIN PK_Customers cust ON cust.CustomerID=sh.CustomerID" +
                " LEFT JOIN SA_Areas ar ON ar.AreaID=sh.PositionID" +
                " LEFT JOIN SA_Zones z ON z.ZoneID=sh.ZoneID" +
                " LEFT JOIN SA_Regions r ON r.RegionID=sh.RegionID" +
                " JOIN SA_Channels c ON c.ChannelID=sh.ChannelID" +
                " LEFT JOIN SA_TransportModes tm ON tm.ModeID=sh.TransportModeID" +
                " LEFT JOIN SA_ShipmentLines sl ON sl.ShipmentID=sh.ShipmentID" +
                " WHERE sh.ConsignmentID=?cid AND sh.Status IN ('Order','DC','Shipped')" +
                " GROUP BY sh.ShipmentID ORDER BY sh.ShipmentDate DESC;",
                new MySqlParameter("?cid", consignmentId));
        }

        /// <summary>Get line items for a SA shipment order</summary>
        public static DataTable GetSAShipmentLines(int shipmentId)
        {
            return ExecuteQuery(
                "SELECT sl.LineID, sl.ProductID, p.ProductName, p.ProductCode, sl.ShippedQty AS Qty" +
                " FROM SA_ShipmentLines sl" +
                " JOIN PP_Products p ON p.ProductID=sl.ProductID" +
                " WHERE sl.ShipmentID=?sid ORDER BY p.ProductName;",
                new MySqlParameter("?sid", shipmentId));
        }

        /// <summary>Check FG stock availability for all line items in a SA shipment</summary>
        public static DataTable CheckFGStockForSAOrder(int shipmentId)
        {
            // Use GetFGStockForShipment as the SINGLE SOURCE OF TRUTH for FG stock
            var fgStock = GetFGStockForShipment();

            // Get SA order lines
            var saLines = ExecuteQuery(
                "SELECT sl.ProductID, p.ProductName, p.ProductCode, sl.ShippedQty AS RequiredQty" +
                " FROM SA_ShipmentLines sl" +
                " JOIN PP_Products p ON p.ProductID=sl.ProductID" +
                " WHERE sl.ShipmentID=?sid ORDER BY p.ProductName;",
                new MySqlParameter("?sid", shipmentId));

            // Add AvailableQty column from FG stock
            saLines.Columns.Add("AvailableQty", typeof(decimal));
            foreach (DataRow sl in saLines.Rows)
            {
                int pid = Convert.ToInt32(sl["ProductID"]);
                decimal avail = 0;
                foreach (DataRow fg in fgStock.Rows)
                {
                    if (Convert.ToInt32(fg["ProductID"]) == pid)
                    {
                        avail = Convert.ToDecimal(fg["AvailableFGJars"]);
                        break;
                    }
                }
                sl["AvailableQty"] = avail;
            }
            return saLines;
        }

        /// <summary>Convert SA shipment to DC status</summary>
        /// <summary>Convert SA shipment to a real Delivery Challan with line items</summary>
        public static int ConvertSAShipmentToDC(int shipmentId, int consignmentId = 0)
        {
            // Get shipment header
            var sh = ExecuteQueryRow(
                "SELECT CustomerID, ShipmentDate, Remarks FROM SA_Shipments WHERE ShipmentID=?sid;",
                new MySqlParameter("?sid", shipmentId));
            if (sh == null) throw new Exception("Shipment not found.");

            int customerId = Convert.ToInt32(sh["CustomerID"]);
            DateTime dcDate = Convert.ToDateTime(sh["ShipmentDate"]);
            string remarks = sh["Remarks"] != DBNull.Value ? sh["Remarks"].ToString() : "";
            string saRef = "SH-" + shipmentId.ToString("D5");

            // Create DC (with consignment if provided)
            string dcNumber = GenerateDCNumber();
            string remText = string.IsNullOrEmpty(remarks) ? saRef : saRef + " — " + remarks;
            if (consignmentId > 0)
            {
                ExecuteNonQuery(
                    "INSERT INTO PK_DeliveryChallans (DCNumber, ConsignmentID, CustomerID, DCDate, Status, Remarks, CreatedBy, CreatedAt)" +
                    " VALUES(?num, ?csg, ?cid, ?dt, 'DRAFT', ?rem, ?by, ?now);",
                    new MySqlParameter("?num", dcNumber),
                    new MySqlParameter("?csg", consignmentId),
                    new MySqlParameter("?cid", customerId),
                    new MySqlParameter("?dt", dcDate),
                    new MySqlParameter("?rem", remText),
                    new MySqlParameter("?by", 1),
                    new MySqlParameter("?now", NowIST()));
            }
            else
            {
                ExecuteNonQuery(
                    "INSERT INTO PK_DeliveryChallans (DCNumber, CustomerID, DCDate, Status, Remarks, CreatedBy, CreatedAt)" +
                    " VALUES(?num, ?cid, ?dt, 'DRAFT', ?rem, ?by, ?now);",
                    new MySqlParameter("?num", dcNumber),
                    new MySqlParameter("?cid", customerId),
                    new MySqlParameter("?dt", dcDate),
                    new MySqlParameter("?rem", remText),
                    new MySqlParameter("?by", 1),
                    new MySqlParameter("?now", NowIST()));
            }
            int dcId = Convert.ToInt32(Convert.ToInt64(ExecuteScalar("SELECT LAST_INSERT_ID();")));

            // Get shipment lines and create DC lines
            var lines = ExecuteQuery(
                "SELECT sl.ProductID, sl.ShippedQty, IFNULL(p.ContainersPerCase, 12) AS JPC," +
                " IFNULL(p.ContainerType, 'JAR') AS ContainerType" +
                " FROM SA_ShipmentLines sl" +
                " JOIN PP_Products p ON p.ProductID = sl.ProductID" +
                " WHERE sl.ShipmentID=?sid AND sl.ShippedQty > 0;",
                new MySqlParameter("?sid", shipmentId));

            foreach (System.Data.DataRow r in lines.Rows)
            {
                int productId = Convert.ToInt32(r["ProductID"]);
                int shippedQty = Convert.ToInt32(r["ShippedQty"]); // in jars/containers
                int jpc = Convert.ToInt32(r["JPC"]);

                // Determine selling form from container type
                string containerType = r.Table.Columns.Contains("ContainerType") && r["ContainerType"] != DBNull.Value
                    ? r["ContainerType"].ToString() : "JAR";
                string sellingForm = containerType == "BOX" ? "BOX" : containerType == "DIRECT" ? "PCS" : "JAR";

                // TotalPcs = shippedQty in the selling form (JARs/BOXes)
                // Source = CASE by default (shipped from cases)
                string source = "CASE";

                ExecuteNonQuery(
                    "INSERT INTO PK_DCLines (DCID, ProductID, Cases, LooseJars, JarsPerCase, TotalPcs, SellingForm, Source)" +
                    " VALUES(?dcid, ?pid, 0, 0, ?jpc, ?tp, ?form, ?src);",
                    new MySqlParameter("?dcid", dcId),
                    new MySqlParameter("?pid", productId),
                    new MySqlParameter("?jpc", jpc),
                    new MySqlParameter("?tp", shippedQty),
                    new MySqlParameter("?form", sellingForm),
                    new MySqlParameter("?src", source));
            }

            // Mark SA shipment as converted
            ExecuteNonQuery("UPDATE SA_Shipments SET Status='DC' WHERE ShipmentID=?sid;",
                new MySqlParameter("?sid", shipmentId));

            return dcId;
        }

        /// <summary>Unconvert DC back to Order status — delete DC and restore SA edit rights</summary>
        public static void UnconvertSAShipmentDC(int shipmentId)
        {
            // Find DC created from this shipment (by remarks containing SH-XXXXX)
            string saRef = "SH-" + shipmentId.ToString("D5");
            var dcRow = ExecuteQueryRow(
                "SELECT DCID FROM PK_DeliveryChallans WHERE Remarks LIKE ?ref AND Status='DRAFT' LIMIT 1;",
                new MySqlParameter("?ref", "%" + saRef + "%"));

            if (dcRow != null)
            {
                int dcId = Convert.ToInt32(dcRow["DCID"]);
                ExecuteNonQuery("DELETE FROM PK_DCLines WHERE DCID=?id;", new MySqlParameter("?id", dcId));
                ExecuteNonQuery("DELETE FROM PK_DeliveryChallans WHERE DCID=?id;", new MySqlParameter("?id", dcId));
            }

            ExecuteNonQuery("UPDATE SA_Shipments SET Status='Order' WHERE ShipmentID=?sid AND Status='DC';",
                new MySqlParameter("?sid", shipmentId));
        }

        /// <summary>Get single SA shipment with full details for editing</summary>
        public static DataRow GetSAShipmentById(int shipmentId)
        {
            return ExecuteQueryRow(
                "SELECT sh.*, IFNULL(cust.CustomerName,'—') AS CustomerName," +
                " IFNULL(ar.AreaName,'—') AS AreaName," +
                " IFNULL(z.ZoneName,'—') AS ZoneName," +
                " IFNULL(r.RegionName,'—') AS RegionName," +
                " IFNULL(c.ChannelName,'—') AS ChannelName," +
                " IFNULL(tm.ModeName,'—') AS TransportMode" +
                " FROM SA_Shipments sh" +
                " LEFT JOIN PK_Customers cust ON cust.CustomerID=sh.CustomerID" +
                " LEFT JOIN SA_Areas ar ON ar.AreaID=sh.PositionID" +
                " LEFT JOIN SA_Zones z ON z.ZoneID=sh.ZoneID" +
                " LEFT JOIN SA_Regions r ON r.RegionID=sh.RegionID" +
                " LEFT JOIN SA_Channels c ON c.ChannelID=sh.ChannelID" +
                " LEFT JOIN SA_TransportModes tm ON tm.ModeID=sh.TransportModeID" +
                " WHERE sh.ShipmentID=?sid;",
                new MySqlParameter("?sid", shipmentId));
        }

        /// <summary>Update SA shipment line items from PK edit</summary>
        public static void UpdateSAShipmentLines(int shipmentId, int[] productIds, int[] quantities)
        {
            ExecuteNonQuery("DELETE FROM SA_ShipmentLines WHERE ShipmentID=?sid;",
                new MySqlParameter("?sid", shipmentId));
            for (int i = 0; i < productIds.Length; i++)
            {
                if (productIds[i] > 0 && quantities[i] > 0)
                {
                    ExecuteNonQuery(
                        "INSERT INTO SA_ShipmentLines (ShipmentID, ProductID, ShippedQty) VALUES (?sid,?pid,?qty);",
                        new MySqlParameter("?sid", shipmentId),
                        new MySqlParameter("?pid", productIds[i]),
                        new MySqlParameter("?qty", quantities[i]));
                }
            }
        }

        /// <summary>Complete shipment dispatch — set status to Shipped and deduct FG stock</summary>
        public static void CompleteSAShipmentDispatch(int shipmentId, int userId)
        {
            // Get line items
            DataTable lines = ExecuteQuery(
                "SELECT sl.ProductID, sl.ShippedQty FROM SA_ShipmentLines sl WHERE sl.ShipmentID=?sid;",
                new MySqlParameter("?sid", shipmentId));

            // Deduct FG stock for each product
            foreach (DataRow r in lines.Rows)
            {
                int productId = Convert.ToInt32(r["ProductID"]);
                int qty = Convert.ToInt32(r["ShippedQty"]);
                if (qty > 0)
                {
                    // Insert negative entry in FG stock to represent deduction
                    ExecuteNonQuery(
                        "INSERT INTO PK_FGStock (ProductID, QtyPacked, PackedAt, CreatedBy)" +
                        " VALUES (?pid, ?qty, NOW(), ?uid);",
                        new MySqlParameter("?pid", productId),
                        new MySqlParameter("?qty", -qty),
                        new MySqlParameter("?uid", userId));
                }
            }

            // Update status
            ExecuteNonQuery("UPDATE SA_Shipments SET Status='Shipped' WHERE ShipmentID=?sid;",
                new MySqlParameter("?sid", shipmentId));
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

        // ── SA PROJECTIONS (Read-only for PK) ─────────────────────────────

        public static DataTable GetSAProjections(int month, int year)
        {
            return ExecuteQuery(
                "SELECT p.ProjectionID, p.ProjectionMonth, p.ProjectionYear, p.Status," +
                " IFNULL(ar.AreaName,'—') AS AreaName," +
                " IFNULL(z.ZoneName,'—') AS ZoneName," +
                " IFNULL(r.RegionName,'—') AS RegionName," +
                " c.ChannelName," +
                " COUNT(pl.LineID) AS ProductCount," +
                " IFNULL(SUM(pl.Quantity),0) AS TotalQty" +
                " FROM SA_Projections p" +
                " LEFT JOIN SA_Areas ar ON ar.AreaID=p.PositionID" +
                " LEFT JOIN SA_Zones z ON z.ZoneID=p.ZoneID" +
                " LEFT JOIN SA_Regions r ON r.RegionID=p.RegionID" +
                " JOIN SA_Channels c ON c.ChannelID=p.ChannelID" +
                " LEFT JOIN SA_ProjectionLines pl ON pl.ProjectionID=p.ProjectionID" +
                " WHERE p.ProjectionMonth=?m AND p.ProjectionYear=?y" +
                " GROUP BY p.ProjectionID ORDER BY ar.AreaName, c.ChannelName;",
                new MySqlParameter("?m", month), new MySqlParameter("?y", year));
        }

        public static DataTable GetSAProjectionLines(int projectionId)
        {
            return ExecuteQuery(
                "SELECT pl.ProductID, pr.ProductName, pr.ProductCode, pl.Quantity," +
                " IFNULL(u.Abbreviation,'') AS UOMAbbrv" +
                " FROM SA_ProjectionLines pl" +
                " JOIN PP_Products pr ON pr.ProductID=pl.ProductID" +
                " LEFT JOIN MM_UOM u ON u.UOMID=pl.UOMID" +
                " WHERE pl.ProjectionID=?pid ORDER BY pr.ProductName;",
                new MySqlParameter("?pid", projectionId));
        }
        // ══════════════════════════════════════════════════════════════
        // MACHINE TRACKING & BATCH COMPLETION
        // ══════════════════════════════════════════════════════════════

        public static DataTable GetActiveMachines()
        {
            return ExecuteQuery("SELECT MachineID, MachineName, MachineCode, Location FROM PK_Machines WHERE IsActive=1 ORDER BY MachineCode;");
        }

        public static DataTable GetAllMachines()
        {
            return ExecuteQuery("SELECT MachineID, MachineName, MachineCode, Location, IsActive FROM PK_Machines ORDER BY MachineCode;");
        }

        public static DataRow GetMachineById(int machineId)
        {
            return ExecuteQueryRow(
                "SELECT * FROM PK_Machines WHERE MachineID=?id;",
                new MySqlParameter("?id", machineId));
        }

        public static void AddMachine(string code, string name, string location)
        {
            ExecuteNonQuery(
                "INSERT INTO PK_Machines (MachineCode, MachineName, Location) VALUES(?code, ?name, ?loc);",
                new MySqlParameter("?code", code),
                new MySqlParameter("?name", name),
                new MySqlParameter("?loc",  string.IsNullOrEmpty(location) ? (object)DBNull.Value : location));
        }

        public static void UpdateMachine(int machineId, string code, string name, string location)
        {
            ExecuteNonQuery(
                "UPDATE PK_Machines SET MachineCode=?code, MachineName=?name, Location=?loc WHERE MachineID=?id;",
                new MySqlParameter("?code", code),
                new MySqlParameter("?name", name),
                new MySqlParameter("?loc",  string.IsNullOrEmpty(location) ? (object)DBNull.Value : location),
                new MySqlParameter("?id",   machineId));
        }

        public static bool MachineCodeExists(string code)
        {
            var row = ExecuteQueryRow(
                "SELECT 1 FROM PK_Machines WHERE MachineCode=?code;",
                new MySqlParameter("?code", code));
            return row != null;
        }

        public static void ToggleMachineActive(int machineId)
        {
            ExecuteNonQuery(
                "UPDATE PK_Machines SET IsActive = IF(IsActive=1, 0, 1) WHERE MachineID=?id;",
                new MySqlParameter("?id", machineId));
        }

        // Start a packing batch with machine ID
        public static int StartPackingBatchWithMachine(int orderId, int batchNo, int userId, int machineId, string labelLanguage = null)
        {
            // Check if THIS machine already has an active batch for this order
            var active = ExecuteQueryRow(
                "SELECT PackingID FROM PK_PackingExecution WHERE OrderID=?oid AND MachineID=?mid AND Status='InProgress';",
                new MySqlParameter("?oid", orderId),
                new MySqlParameter("?mid", machineId));
            if (active != null) return Convert.ToInt32(active["PackingID"]);

            ExecuteNonQuery(
                "INSERT INTO PK_PackingExecution (OrderID,BatchNo,StartTime,Status,CreatedBy,MachineID,LabelLanguage)" +
                " VALUES(?oid,?bno,?now,'InProgress',?by,?mid,?lang);",
                new MySqlParameter("?oid",  orderId),
                new MySqlParameter("?bno",  batchNo),
                new MySqlParameter("?now",  NowIST()),
                new MySqlParameter("?by",   userId),
                new MySqlParameter("?mid",  machineId),
                new MySqlParameter("?lang", string.IsNullOrEmpty(labelLanguage) ? (object)DBNull.Value : labelLanguage));
            return Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID();"));
        }

        // Get active packing for a specific machine on an order
        public static DataRow GetActivePackingForMachine(int orderId, int machineId)
        {
            return ExecuteQueryRow(
                "SELECT * FROM PK_PackingExecution" +
                " WHERE OrderID=?oid AND MachineID=?mid AND Status='InProgress'" +
                " ORDER BY PackingID DESC LIMIT 1;",
                new MySqlParameter("?oid", orderId),
                new MySqlParameter("?mid", machineId));
        }

        /// <summary>Count how many batches THIS machine has completed for an order.</summary>
        public static int GetMachinePackedCount(int orderId, int machineId)
        {
            var row = ExecuteQueryRow(
                "SELECT COUNT(*) AS Cnt FROM PK_PackingExecution" +
                " WHERE OrderID=?oid AND MachineID=?mid AND Status='Completed' AND BatchNo > 0;",
                new MySqlParameter("?oid", orderId),
                new MySqlParameter("?mid", machineId));
            return row != null ? Convert.ToInt32(Convert.ToInt64(row["Cnt"])) : 0;
        }

        /// <summary>Get set of batch numbers this machine has completed for an order.</summary>
        public static System.Collections.Generic.HashSet<int> GetMachineCompletedBatches(int orderId, int machineId)
        {
            var set = new System.Collections.Generic.HashSet<int>();
            var dt = ExecuteQuery(
                "SELECT DISTINCT BatchNo FROM PK_PackingExecution" +
                " WHERE OrderID=?oid AND MachineID=?mid AND Status='Completed' AND BatchNo > 0;",
                new MySqlParameter("?oid", orderId),
                new MySqlParameter("?mid", machineId));
            foreach (DataRow r in dt.Rows)
                set.Add(Convert.ToInt32(r["BatchNo"]));
            return set;
        }

        // Check if all machines have marked themselves done via "All Batches Done" button
        public static bool AreAllBatchesPacked(int orderId)
        {
            // Check no machine still has InProgress batches
            var anyActive = ExecuteQueryRow(
                "SELECT PackingID FROM PK_PackingExecution WHERE OrderID=?oid AND Status='InProgress' LIMIT 1;",
                new MySqlParameter("?oid", orderId));
            if (anyActive != null) return false;

            // Get all machines that participated (have at least one Completed batch)
            var machines = ExecuteQuery(
                "SELECT DISTINCT MachineID FROM PK_PackingExecution WHERE OrderID=?oid AND Status='Completed' AND MachineID IS NOT NULL;",
                new MySqlParameter("?oid", orderId));
            if (machines.Rows.Count == 0) return false;

            // Every participating machine must have a MachineDone marker
            foreach (DataRow r in machines.Rows)
            {
                int mid = Convert.ToInt32(r["MachineID"]);
                var done = ExecuteQueryRow(
                    "SELECT PackingID FROM PK_PackingExecution WHERE OrderID=?oid AND MachineID=?mid AND Status='MachineDone' LIMIT 1;",
                    new MySqlParameter("?oid", orderId),
                    new MySqlParameter("?mid", mid));
                if (done == null) return false;
            }
            return true;
        }

        /// <summary>Mark this machine as done for this order.</summary>
        public static void MarkMachineDone(int orderId, int machineId, int userId)
        {
            var existing = ExecuteQueryRow(
                "SELECT PackingID FROM PK_PackingExecution WHERE OrderID=?oid AND MachineID=?mid AND Status='MachineDone' LIMIT 1;",
                new MySqlParameter("?oid", orderId),
                new MySqlParameter("?mid", machineId));
            if (existing != null) return;
            ExecuteNonQuery(
                "INSERT INTO PK_PackingExecution (OrderID, BatchNo, StartTime, EndTime, Status, CreatedBy, MachineID)" +
                " VALUES(?oid, 0, ?now, ?now, 'MachineDone', ?by, ?mid);",
                new MySqlParameter("?oid", orderId),
                new MySqlParameter("?now", NowIST()),
                new MySqlParameter("?by",  userId),
                new MySqlParameter("?mid", machineId));
        }

        /// <summary>Mark ALL participating machines as done for this order.</summary>
        public static void MarkAllMachinesDone(int orderId, int userId)
        {
            var machines = ExecuteQuery(
                "SELECT DISTINCT MachineID FROM PK_PackingExecution WHERE OrderID=?oid AND Status='Completed' AND MachineID IS NOT NULL;",
                new MySqlParameter("?oid", orderId));
            foreach (DataRow r in machines.Rows)
            {
                int mid = Convert.ToInt32(r["MachineID"]);
                MarkMachineDone(orderId, mid, userId);
            }
        }

        /// <summary>Check if this machine is marked done for this order.</summary>
        public static bool IsMachineDone(int orderId, int machineId)
        {
            var row = ExecuteQueryRow(
                "SELECT PackingID FROM PK_PackingExecution WHERE OrderID=?oid AND MachineID=?mid AND Status='MachineDone' LIMIT 1;",
                new MySqlParameter("?oid", orderId),
                new MySqlParameter("?mid", machineId));
            return row != null;
        }

        // Get batch completion record for an order
        public static DataRow GetBatchCompletion(int orderId)
        {
            return ExecuteQueryRow(
                "SELECT * FROM PK_Batch_Completion WHERE OrderID=?oid;",
                new MySqlParameter("?oid", orderId));
        }

        // Create pending batch completion record when all batches are packed
        public static void CreatePendingBatchCompletion(int orderId)
        {
            var existing = GetBatchCompletion(orderId);
            if (existing != null) return; // already exists
            ExecuteNonQuery(
                "INSERT INTO PK_Batch_Completion (OrderID, Status) VALUES(?oid, 'Pending');",
                new MySqlParameter("?oid", orderId));
        }

        // Complete batch completion with JAR/BOX count and PM verification
        public static void CompleteBatchCompletion(int orderId, int totalJars, int totalLoosePcs,
            int jarSize, int totalUnits, int userId)
        {
            ExecuteNonQuery(
                "UPDATE PK_Batch_Completion SET TotalJars=?jars, TotalLoosePcs=?loose," +
                " JarSize=?js, TotalUnits=?total, PMVerified=1, Status='Completed'," +
                " CompletedBy=?by, CompletedAt=?now WHERE OrderID=?oid;",
                new MySqlParameter("?jars",  totalJars),
                new MySqlParameter("?loose", totalLoosePcs),
                new MySqlParameter("?js",    jarSize),
                new MySqlParameter("?total", totalUnits),
                new MySqlParameter("?by",    userId),
                new MySqlParameter("?now",   NowIST()),
                new MySqlParameter("?oid",   orderId));
        }

        // Check if an order has a pending batch completion (needs completion before new packing)
        public static bool HasPendingBatchCompletion(int orderId)
        {
            var row = GetBatchCompletion(orderId);
            return row != null && row["Status"].ToString() == "Pending";
        }

        // Get packing summary by machine for an order
        public static DataTable GetPackingSummaryByMachine(int orderId)
        {
            return ExecuteQuery(
                "SELECT pe.MachineID, IFNULL(m.MachineName, CONCAT('Machine ', pe.MachineID)) AS MachineName," +
                " m.MachineCode, COUNT(*) AS BatchesPacked," +
                " MIN(pe.StartTime) AS FirstStart, MAX(pe.EndTime) AS LastEnd" +
                " FROM PK_PackingExecution pe" +
                " LEFT JOIN PK_Machines m ON m.MachineID=pe.MachineID" +
                " WHERE pe.OrderID=?oid AND pe.Status='Completed' AND pe.BatchNo > 0" +
                " GROUP BY pe.MachineID, m.MachineName, m.MachineCode" +
                " ORDER BY m.MachineCode;",
                new MySqlParameter("?oid", orderId));
        }

        // Get pending packing orders INCLUDING those with pending batch completion
        public static DataTable GetPendingPackingOrdersWithCompletion(int productId)
        {
            return ExecuteQuery(
                "SELECT po.OrderID, po.OrderDate, po.Shift," +
                " IFNULL(po.RevisedBatches, po.OrderedBatches) AS TotalBatches," +
                " po.Status," +
                " (SELECT COUNT(*) FROM PP_BatchExecution be" +
                "  WHERE be.OrderID=po.OrderID AND be.Status='Completed') AS ProductionDone," +
                " (SELECT COUNT(DISTINCT pe.BatchNo) FROM PK_PackingExecution pe" +
                "  WHERE pe.OrderID=po.OrderID AND pe.Status='Completed' AND pe.BatchNo > 0) AS PackedBatches," +
                " (SELECT bc.Status FROM PK_Batch_Completion bc WHERE bc.OrderID=po.OrderID) AS CompletionStatus" +
                " FROM PP_ProductionOrder po" +
                " WHERE po.ProductID=?pid" +
                " AND po.Status IN ('Initiated','InProgress','Completed')" +
                " AND (SELECT COUNT(*) FROM PP_BatchExecution be2" +
                "      WHERE be2.OrderID=po.OrderID AND be2.Status='Completed') > 0" +
                " AND (" +
                // Either: not all packed yet (normal packing)
                "   (SELECT COUNT(*) FROM PK_PackingExecution pe2" +
                "    WHERE pe2.OrderID=po.OrderID AND pe2.Status='Completed' AND pe2.BatchNo > 0)" +
                "   < (SELECT COUNT(*) FROM PP_BatchExecution be3" +
                "      WHERE be3.OrderID=po.OrderID AND be3.Status='Completed')" +
                // OR: has pending batch completion (needs finishing)
                "   OR EXISTS (SELECT 1 FROM PK_Batch_Completion bc2 WHERE bc2.OrderID=po.OrderID AND bc2.Status='Pending')" +
                " )" +
                " ORDER BY po.OrderDate DESC;",
                new MySqlParameter("?pid", productId));
        }

        // ── CUSTOMER MARGINS ─────────────────────────────────────────────────

        public static void SaveCustomerMargins(int customerId, decimal smPct, decimal gtPct)
        {
            ExecuteNonQuery(
                "INSERT INTO PK_CustomerMargins (CustomerID, SuperMarketPct, GTPct) " +
                "VALUES(?cid, ?sm, ?gt) " +
                "ON DUPLICATE KEY UPDATE SuperMarketPct=?sm, GTPct=?gt;",
                new MySqlParameter("?cid", customerId),
                new MySqlParameter("?sm", smPct),
                new MySqlParameter("?gt", gtPct));
        }

        public static DataRow GetCustomerMargins(int customerId)
        {
            var dt = ExecuteQuery(
                "SELECT * FROM PK_CustomerMargins WHERE CustomerID=?cid;",
                new MySqlParameter("?cid", customerId));
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public static void SaveCustomerShipTo(int customerId, string shipAddr, string shipCity, string shipState, string shipPin)
        {
            ExecuteNonQuery(
                "UPDATE PK_Customers SET ShipToAddress=?addr, ShipToCity=?city, ShipToState=?st, ShipToPinCode=?pin WHERE CustomerID=?cid;",
                new MySqlParameter("?addr", string.IsNullOrEmpty(shipAddr) ? (object)DBNull.Value : shipAddr),
                new MySqlParameter("?city", string.IsNullOrEmpty(shipCity) ? (object)DBNull.Value : shipCity),
                new MySqlParameter("?st", string.IsNullOrEmpty(shipState) ? (object)DBNull.Value : shipState),
                new MySqlParameter("?pin", string.IsNullOrEmpty(shipPin) ? (object)DBNull.Value : shipPin),
                new MySqlParameter("?cid", customerId));
        }

        // ── PRODUCT MRP ──────────────────────────────────────────────────────

        public static DataTable GetProductMRPList()
        {
            return ExecuteQuery(
                "SELECT p.ProductID, p.ProductCode, p.ProductName, p.HSNCode, p.GSTRate, " +
                "p.ContainerType, " +
                "m_pcs.MRP AS MRP_PCS, m_jar.MRP AS MRP_JAR, m_box.MRP AS MRP_BOX, m_case.MRP AS MRP_CASE " +
                "FROM PP_Products p " +
                "LEFT JOIN PK_ProductMRP m_pcs  ON m_pcs.ProductID=p.ProductID  AND m_pcs.SellingForm='PCS' " +
                "LEFT JOIN PK_ProductMRP m_jar  ON m_jar.ProductID=p.ProductID  AND m_jar.SellingForm='JAR' " +
                "LEFT JOIN PK_ProductMRP m_box  ON m_box.ProductID=p.ProductID  AND m_box.SellingForm='BOX' " +
                "LEFT JOIN PK_ProductMRP m_case ON m_case.ProductID=p.ProductID AND m_case.SellingForm='CASE' " +
                "WHERE p.IsActive=1 AND p.ProductType IN ('Core','Conversion','Prefilled Conversion') " +
                "ORDER BY p.ProductName;");
        }

        public static void SaveProductMRP(int productId, string sellingForm, decimal mrp)
        {
            if (mrp > 0)
            {
                ExecuteNonQuery(
                    "INSERT INTO PK_ProductMRP (ProductID, SellingForm, MRP) " +
                    "VALUES(?pid, ?form, ?mrp) " +
                    "ON DUPLICATE KEY UPDATE MRP=?mrp;",
                    new MySqlParameter("?pid", productId),
                    new MySqlParameter("?form", sellingForm),
                    new MySqlParameter("?mrp", mrp));
            }
            else
            {
                ExecuteNonQuery(
                    "DELETE FROM PK_ProductMRP WHERE ProductID=?pid AND SellingForm=?form;",
                    new MySqlParameter("?pid", productId),
                    new MySqlParameter("?form", sellingForm));
            }
        }

        public static decimal GetProductMRP(int productId, string sellingForm)
        {
            var val = ExecuteScalar(
                "SELECT MRP FROM PK_ProductMRP WHERE ProductID=?pid AND SellingForm=?form;",
                new MySqlParameter("?pid", productId),
                new MySqlParameter("?form", sellingForm));
            return val != null && val != DBNull.Value ? Convert.ToDecimal(val) : 0;
        }

        // ══════════════════════════════════════════════════════════════
        // CONSIGNMENT MANAGEMENT
        // ══════════════════════════════════════════════════════════════

        /// <summary>Create a new consignment. Auto-generates ConsignmentCode from date + sequence + userText.</summary>
        public static int CreateConsignment(DateTime consigDate, string userText, string remarks, int userId)
        {
            userText = (userText ?? "").Trim().ToUpper().Replace(" ", "-");
            if (string.IsNullOrEmpty(userText))
                throw new Exception("User text for consignment is required.");

            // Get next sequence for this date
            object seqObj = ExecuteScalar(
                "SELECT IFNULL(MAX(SequenceNo), 0) + 1 FROM PK_Consignments WHERE ConsignmentDate=?dt;",
                new MySqlParameter("?dt", consigDate.ToString("yyyy-MM-dd")));
            int seq = seqObj != null && seqObj != DBNull.Value ? Convert.ToInt32(Convert.ToInt64(seqObj)) : 1;

            string code = "CONSIG-" + consigDate.ToString("ddMMMyy").ToUpper() + "-" + seq.ToString("D2") + "-" + userText;

            ExecuteNonQuery(
                "INSERT INTO PK_Consignments (ConsignmentCode, ConsignmentDate, SequenceNo, UserText, Status, Remarks, CreatedBy, CreatedAt)" +
                " VALUES(?code, ?dt, ?seq, ?txt, 'OPEN', ?rem, ?by, ?now);",
                new MySqlParameter("?code", code),
                new MySqlParameter("?dt", consigDate.ToString("yyyy-MM-dd")),
                new MySqlParameter("?seq", seq),
                new MySqlParameter("?txt", userText),
                new MySqlParameter("?rem", remarks ?? ""),
                new MySqlParameter("?by", userId),
                new MySqlParameter("?now", NowIST()));

            return Convert.ToInt32(Convert.ToInt64(ExecuteScalar("SELECT LAST_INSERT_ID();")));
        }

        /// <summary>Get active consignments (OPEN + READY) for tabs.</summary>
        public static DataTable GetActiveConsignments()
        {
            return ExecuteQuery(
                "SELECT c.ConsignmentID, c.ConsignmentCode, c.ConsignmentDate, c.UserText, c.Status," +
                " c.VehicleNumber," +
                " (SELECT COUNT(*) FROM PK_DeliveryChallans d WHERE d.ConsignmentID=c.ConsignmentID) AS DCCount," +
                " (SELECT IFNULL(SUM(d2.GrandTotal),0) FROM PK_DeliveryChallans d2 WHERE d2.ConsignmentID=c.ConsignmentID) AS TotalAmount" +
                " FROM PK_Consignments c WHERE c.Status IN ('OPEN','READY')" +
                " ORDER BY c.ConsignmentDate DESC, c.SequenceNo DESC;");
        }

        /// <summary>Get all consignments for SA listing.</summary>
        public static DataTable GetAllConsignments(int limit = 50)
        {
            return ExecuteQuery(
                "SELECT c.ConsignmentID, c.ConsignmentCode, c.ConsignmentDate, c.UserText, c.Status, c.Remarks," +
                " (SELECT COUNT(*) FROM PK_DeliveryChallans d WHERE d.ConsignmentID=c.ConsignmentID) AS DCCount," +
                " (SELECT IFNULL(SUM(d2.GrandTotal),0) FROM PK_DeliveryChallans d2 WHERE d2.ConsignmentID=c.ConsignmentID) AS TotalAmount" +
                " FROM PK_Consignments c WHERE c.Status IN ('OPEN','READY','DISPATCHED')" +
                " ORDER BY c.ConsignmentDate DESC, c.SequenceNo DESC LIMIT ?lim;",
                new MySqlParameter("?lim", limit));
        }

        /// <summary>Get DISPATCHED consignments for dropdown.</summary>
        public static DataTable GetDispatchedConsignments()
        {
            return ExecuteQuery(
                "SELECT c.ConsignmentID, c.ConsignmentCode, c.ConsignmentDate, c.VehicleNumber, c.DispatchedAt," +
                " (SELECT COUNT(*) FROM PK_DeliveryChallans d WHERE d.ConsignmentID=c.ConsignmentID) AS DCCount," +
                " (SELECT IFNULL(SUM(d2.GrandTotal),0) FROM PK_DeliveryChallans d2 WHERE d2.ConsignmentID=c.ConsignmentID) AS TotalAmount" +
                " FROM PK_Consignments c WHERE c.Status='DISPATCHED'" +
                " ORDER BY c.DispatchedAt DESC;");
        }

        /// <summary>Get ARCHIVED consignments for dropdown.</summary>
        public static DataTable GetArchivedConsignments(int limit = 50)
        {
            return ExecuteQuery(
                "SELECT c.ConsignmentID, c.ConsignmentCode, c.ConsignmentDate, c.VehicleNumber, c.DispatchedAt, c.ArchivedAt," +
                " (SELECT COUNT(*) FROM PK_DeliveryChallans d WHERE d.ConsignmentID=c.ConsignmentID) AS DCCount," +
                " (SELECT IFNULL(SUM(d2.GrandTotal),0) FROM PK_DeliveryChallans d2 WHERE d2.ConsignmentID=c.ConsignmentID) AS TotalAmount" +
                " FROM PK_Consignments c WHERE c.Status='ARCHIVED'" +
                " ORDER BY c.ArchivedAt DESC LIMIT ?lim;",
                new MySqlParameter("?lim", limit));
        }

        /// <summary>Get consignment by ID.</summary>
        public static DataRow GetConsignmentById(int consignmentId)
        {
            return ExecuteQueryRow(
                "SELECT * FROM PK_Consignments WHERE ConsignmentID=?id;",
                new MySqlParameter("?id", consignmentId));
        }

        /// <summary>Get all DCs for a consignment with transport details.</summary>
        public static DataTable GetDCsByConsignment(int consignmentId)
        {
            return ExecuteQuery(
                "SELECT d.DCID, d.DCNumber, d.DCDate, d.Status, d.GrandTotal, d.InvoiceNumber, d.Channel," +
                " d.TrackingNumber, d.TransportMode, d.CourierName," +
                " c.CustomerName, c.CustomerCode, c.CustomerType," +
                " (SELECT COUNT(*) FROM PK_DCLines dl WHERE dl.DCID=d.DCID) AS LineCount," +
                " (SELECT IFNULL(SUM(dl2.TotalPcs),0) FROM PK_DCLines dl2 WHERE dl2.DCID=d.DCID) AS TotalPcs" +
                " FROM PK_DeliveryChallans d" +
                " JOIN PK_Customers c ON c.CustomerID=d.CustomerID" +
                " WHERE d.ConsignmentID=?cid ORDER BY d.DCNumber;",
                new MySqlParameter("?cid", consignmentId));
        }

        /// <summary>Check if consignment is ready — all DCs finalised + all invoiced.</summary>
        public static string GetConsignmentReadyStatus(int consignmentId)
        {
            var dcs = GetDCsByConsignment(consignmentId);
            if (dcs.Rows.Count == 0) return "OPEN";
            bool allFinalised = true, allInvoiced = true;
            foreach (DataRow dc in dcs.Rows)
            {
                if (dc["Status"].ToString() != "FINALISED") allFinalised = false;
                string invNo = dc["InvoiceNumber"] != DBNull.Value ? dc["InvoiceNumber"].ToString() : "";
                if (string.IsNullOrEmpty(invNo)) allInvoiced = false;
            }
            return (allFinalised && allInvoiced) ? "READY" : "OPEN";
        }

        /// <summary>Auto-update consignment status to READY if conditions met.</summary>
        public static void UpdateConsignmentReadyStatus(int consignmentId)
        {
            string status = GetConsignmentReadyStatus(consignmentId);
            ExecuteNonQuery("UPDATE PK_Consignments SET Status=?st WHERE ConsignmentID=?id AND Status='OPEN';",
                new MySqlParameter("?st", status),
                new MySqlParameter("?id", consignmentId));
        }

        /// <summary>Dispatch a consignment.</summary>
        public static void DispatchConsignment(int consignmentId, string vehicleNumber)
        {
            ExecuteNonQuery(
                "UPDATE PK_Consignments SET Status='DISPATCHED', VehicleNumber=?vn, DispatchedAt=NOW()" +
                " WHERE ConsignmentID=?id AND Status IN ('OPEN','READY');",
                new MySqlParameter("?vn", vehicleNumber ?? ""),
                new MySqlParameter("?id", consignmentId));
        }

        /// <summary>Archive a consignment.</summary>
        public static void ArchiveConsignment(int consignmentId)
        {
            ExecuteNonQuery("UPDATE PK_Consignments SET Status='ARCHIVED', ArchivedAt=NOW() WHERE ConsignmentID=?id;",
                new MySqlParameter("?id", consignmentId));
        }

        /// <summary>Unarchive a consignment back to DISPATCHED.</summary>
        public static void UnarchiveConsignment(int consignmentId)
        {
            ExecuteNonQuery("UPDATE PK_Consignments SET Status='DISPATCHED', ArchivedAt=NULL WHERE ConsignmentID=?id AND Status='ARCHIVED';",
                new MySqlParameter("?id", consignmentId));
        }

        /// <summary>Update DC transport details.</summary>
        public static void UpdateDCTransport(int dcId, string transportMode, string courierName, string trackingNumber)
        {
            ExecuteNonQuery(
                "UPDATE PK_DeliveryChallans SET TransportMode=?tm, CourierName=?cn, TrackingNumber=?tn WHERE DCID=?id;",
                new MySqlParameter("?tm", transportMode ?? ""),
                new MySqlParameter("?cn", courierName ?? ""),
                new MySqlParameter("?tn", trackingNumber ?? ""),
                new MySqlParameter("?id", dcId));
        }

        /// <summary>Get DCs not in any consignment (retail/standalone orders).</summary>
        public static DataTable GetRetailDCs(int limit = 50)
        {
            return ExecuteQuery(
                "SELECT d.DCID, d.DCNumber, d.DCDate, d.Status, d.GrandTotal, d.InvoiceNumber, d.Channel," +
                " d.TrackingNumber, d.TransportMode, d.CourierName," +
                " c.CustomerName, c.CustomerCode, c.CustomerType" +
                " FROM PK_DeliveryChallans d" +
                " JOIN PK_Customers c ON c.CustomerID=d.CustomerID" +
                " WHERE d.ConsignmentID IS NULL" +
                " ORDER BY d.DCID DESC LIMIT ?lim;",
                new MySqlParameter("?lim", limit));
        }

        // ══════════════════════════════════════════════════════════════
        // SALES FORCE CONSIGNMENT
        // ══════════════════════════════════════════════════════════════

        /// <summary>Get shipment orders under a consignment.</summary>
        public static DataTable GetShipmentsByConsignment(int consignmentId)
        {
            return ExecuteQuery(
                "SELECT s.ShipmentID, s.ShipmentDate, s.Status, s.CustomerID, s.Remarks," +
                " c.CustomerName, c.CustomerCode, c.CustomerType," +
                " IFNULL(ct.TypeName,'') AS TypeName," +
                " (SELECT COUNT(*) FROM SA_ShipmentLines sl WHERE sl.ShipmentID=s.ShipmentID) AS LineCount," +
                " (SELECT IFNULL(SUM(sl2.ShippedQty),0) FROM SA_ShipmentLines sl2 WHERE sl2.ShipmentID=s.ShipmentID) AS TotalQty" +
                " FROM SA_Shipments s" +
                " LEFT JOIN PK_Customers c ON c.CustomerID=s.CustomerID" +
                " LEFT JOIN PK_CustomerTypes ct ON ct.TypeCode=c.CustomerType" +
                " WHERE s.ConsignmentID=?cid ORDER BY s.ShipmentID;",
                new MySqlParameter("?cid", consignmentId));
        }

        /// <summary>Link an existing shipment to a consignment.</summary>
        public static void LinkShipmentToConsignment(int shipmentId, int consignmentId)
        {
            ExecuteNonQuery(
                "UPDATE SA_Shipments SET ConsignmentID=?cid WHERE ShipmentID=?sid;",
                new MySqlParameter("?cid", consignmentId),
                new MySqlParameter("?sid", shipmentId));
        }

        /// <summary>Get consignments that have at least one shipment order (for PK visibility).</summary>
        public static DataTable GetConsignmentsWithShipments()
        {
            return ExecuteQuery(
                "SELECT DISTINCT c.ConsignmentID, c.ConsignmentCode, c.ConsignmentDate, c.Status" +
                " FROM PK_Consignments c" +
                " JOIN SA_Shipments s ON s.ConsignmentID=c.ConsignmentID" +
                " WHERE c.Status IN ('OPEN','READY') AND s.Status IN ('Order','Saved')" +
                " ORDER BY c.ConsignmentDate DESC;");
        }

        /// <summary>Create a shipment order under a consignment (from Sales Force).</summary>
        public static int CreateShipmentInConsignment(int consignmentId, int customerId, DateTime shipDate,
            int stateId, int channelId, int userId, string remarks = "")
        {
            ExecuteNonQuery(
                "INSERT INTO SA_Shipments (ConsignmentID, CustomerID, ShipmentDate, StateID, ChannelID," +
                " Status, Remarks, CreatedBy, CreatedAt)" +
                " VALUES(?cid, ?custid, ?dt, ?sid, ?chid, 'Order', ?rem, ?by, NOW());",
                new MySqlParameter("?cid", consignmentId),
                new MySqlParameter("?custid", customerId),
                new MySqlParameter("?dt", shipDate.ToString("yyyy-MM-dd")),
                new MySqlParameter("?sid", stateId),
                new MySqlParameter("?chid", channelId),
                new MySqlParameter("?rem", remarks ?? ""),
                new MySqlParameter("?by", userId));
            return Convert.ToInt32(Convert.ToInt64(ExecuteScalar("SELECT LAST_INSERT_ID();")));
        }
    }
}
