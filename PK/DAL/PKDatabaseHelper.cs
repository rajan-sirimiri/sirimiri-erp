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
                "SELECT UserID, Username, FullName, Role FROM Users" +
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

        private static object ExecuteScalar(string sql, params MySqlParameter[] prms)
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
        public static DataTable GetAllCustomers()
        {
            return ExecuteQuery(
                "SELECT CustomerID, CustomerCode, CustomerName, ContactPerson," +
                " Phone, Email, City, State, GSTIN, IsActive, CreatedAt" +
                " FROM PK_Customers ORDER BY CustomerName;");
        }

        public static DataTable GetActiveCustomers()
        {
            return ExecuteQuery(
                "SELECT CustomerID, CustomerCode, CustomerName, City" +
                " FROM PK_Customers WHERE IsActive=1 ORDER BY CustomerName;");
        }

        public static DataRow GetCustomerById(int id)
        {
            return ExecuteQueryRow(
                "SELECT * FROM PK_Customers WHERE CustomerID=?id;",
                new MySqlParameter("?id", id));
        }

        public static void AddCustomer(string name, string contact, string phone,
            string email, string address, string city, string state, string gstin)
        {
            string code = GenerateCustomerCode();
            ExecuteNonQuery(
                "INSERT INTO PK_Customers (CustomerCode,CustomerName,ContactPerson,Phone," +
                "Email,Address,City,State,GSTIN,IsActive,CreatedAt)" +
                " VALUES(?code,?name,?con,?ph,?em,?addr,?city,?state,?gst,1,NOW());",
                new MySqlParameter("?code",  code),
                new MySqlParameter("?name",  name),
                new MySqlParameter("?con",   contact ?? (object)DBNull.Value),
                new MySqlParameter("?ph",    phone   ?? (object)DBNull.Value),
                new MySqlParameter("?em",    email   ?? (object)DBNull.Value),
                new MySqlParameter("?addr",  address ?? (object)DBNull.Value),
                new MySqlParameter("?city",  city    ?? (object)DBNull.Value),
                new MySqlParameter("?state", state   ?? (object)DBNull.Value),
                new MySqlParameter("?gst",   gstin   ?? (object)DBNull.Value));
        }

        public static void UpdateCustomer(int id, string code, string name,
            string contact, string phone, string email, string address,
            string city, string state, string gstin)
        {
            ExecuteNonQuery(
                "UPDATE PK_Customers SET CustomerCode=?code,CustomerName=?name," +
                "ContactPerson=?con,Phone=?ph,Email=?em,Address=?addr," +
                "City=?city,State=?state,GSTIN=?gst WHERE CustomerID=?id;",
                new MySqlParameter("?code",  code),
                new MySqlParameter("?name",  name),
                new MySqlParameter("?con",   contact ?? (object)DBNull.Value),
                new MySqlParameter("?ph",    phone   ?? (object)DBNull.Value),
                new MySqlParameter("?em",    email   ?? (object)DBNull.Value),
                new MySqlParameter("?addr",  address ?? (object)DBNull.Value),
                new MySqlParameter("?city",  city    ?? (object)DBNull.Value),
                new MySqlParameter("?state", state   ?? (object)DBNull.Value),
                new MySqlParameter("?gst",   gstin   ?? (object)DBNull.Value),
                new MySqlParameter("?id",    id));
        }

        public static void ToggleCustomer(int id, bool active)
        {
            ExecuteNonQuery("UPDATE PK_Customers SET IsActive=?a WHERE CustomerID=?id;",
                new MySqlParameter("?a",  active ? 1 : 0),
                new MySqlParameter("?id", id));
        }

        private static string GenerateCustomerCode()
        {
            object last = ExecuteScalar(
                "SELECT CustomerCode FROM PK_Customers ORDER BY CustomerID DESC LIMIT 1;");
            if (last == null || last == DBNull.Value) return "CUST-001";
            var m = System.Text.RegularExpressions.Regex.Match(last.ToString(), @"\d+$");
            int n = m.Success ? int.Parse(m.Value) + 1 : 1;
            return "CUST-" + n.ToString("D3");
        }


        // ── PRIMARY PACKING EXECUTION ────────────────────────────────────────

        // Products with active (Initiated/InProgress) orders today
        public static DataTable GetProductsInProduction()
        {
            return ExecuteQuery(
                "SELECT DISTINCT p.ProductID, p.ProductCode, p.ProductName," +
                " p.ContainerType, p.UnitsPerContainer, p.ContainersPerCase," +
                " ou.Abbreviation AS OutputUnit" +
                " FROM PP_ProductionOrder po" +
                " JOIN PP_Products p ON p.ProductID = po.ProductID" +
                " JOIN MM_UOM ou ON ou.UOMID = p.OutputUOMID" +
                " WHERE po.Status IN ('Initiated','InProgress') AND p.ProductType = 'Core'" +
                " ORDER BY p.ProductName;");
        }

        // Get order details for a product — total batches and packed batches
        public static DataRow GetPackingOrderForProduct(int productId)
        {
            return ExecuteQueryRow(
                "SELECT po.OrderID," +
                " IFNULL(po.RevisedBatches, po.OrderedBatches) AS TotalBatches," +
                " po.Status," +
                " (SELECT COUNT(*) FROM PK_PackingExecution pe" +
                "  WHERE pe.OrderID=po.OrderID AND pe.Status='Completed') AS PackedBatches" +
                " FROM PP_ProductionOrder po" +
                " WHERE po.ProductID=?pid AND po.Status IN ('Initiated','InProgress')" +
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
                " Cases, Jars, Units, JarSize, TotalUnits, Status" +
                " FROM PK_PackingExecution WHERE OrderID=?oid ORDER BY BatchNo;",
                new MySqlParameter("?oid", orderId));
        }

        public static int StartPackingBatch(int orderId, int batchNo, int userId)
        {
            var existing = ExecuteQueryRow(
                "SELECT PackingID FROM PK_PackingExecution WHERE OrderID=?oid AND BatchNo=?bno;",
                new MySqlParameter("?oid", orderId),
                new MySqlParameter("?bno", batchNo));
            if (existing != null) return Convert.ToInt32(existing["PackingID"]);
            ExecuteNonQuery(
                "INSERT INTO PK_PackingExecution (OrderID,BatchNo,StartTime,Status,CreatedBy)" +
                " VALUES(?oid,?bno,?now,'InProgress',?by);",
                new MySqlParameter("?oid", orderId),
                new MySqlParameter("?bno", batchNo),
                new MySqlParameter("?now", NowIST()),
                new MySqlParameter("?by",  userId));
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
                " ou.Abbreviation AS Unit," +
                " ROUND(IFNULL(fg.TotalPacked,0) - IFNULL(sp.TotalPacked2,0), 3) AS AvailableQty" +
                " FROM PP_Products p" +
                " JOIN MM_UOM ou ON ou.UOMID = p.OutputUOMID" +
                " LEFT JOIN (SELECT ProductID, SUM(QtyPacked) AS TotalPacked" +
                "   FROM PK_FGStock GROUP BY ProductID) fg ON fg.ProductID = p.ProductID" +
                " LEFT JOIN (SELECT ProductID, SUM(TotalUnits) AS TotalPacked2" +
                "   FROM PK_SecondaryPacking GROUP BY ProductID) sp ON sp.ProductID = p.ProductID" +
                " WHERE p.IsActive=1 AND IFNULL(fg.TotalPacked,0) - IFNULL(sp.TotalPacked2,0) > 0.001" +
                " ORDER BY p.ProductName;");
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

        public static DataTable GetSecondaryPackingToday()
        {
            return ExecuteQuery(
                "SELECT sp.SecPackID, p.ProductName, p.ProductCode," +
                " sp.QtyCartons, sp.UnitsPerCarton, sp.TotalUnits," +
                " ou.Abbreviation AS Unit, sp.PackedAt, sp.Remarks," +
                " pm.PMName, sp.CartonsUsed" +
                " FROM PK_SecondaryPacking sp" +
                " JOIN PP_Products p ON p.ProductID = sp.ProductID" +
                " JOIN MM_UOM ou ON ou.UOMID = p.OutputUOMID" +
                " LEFT JOIN MM_PackingMaterials pm ON pm.PMID = sp.PMID" +
                " WHERE DATE(sp.PackedAt) = ?today ORDER BY sp.PackedAt DESC;",
                new MySqlParameter("?today", TodayIST()));
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
            return ExecuteQuery(
                "SELECT p.ProductCode, p.ProductName, ou.Abbreviation AS Unit," +
                " ROUND(IFNULL(fg.TotalPacked,0),3) AS TotalPacked," +
                " ROUND(IFNULL(sp.TotalSecondary,0),3) AS TotalSecondary," +
                " ROUND(IFNULL(sh.TotalShipped,0),3) AS TotalShipped," +
                " ROUND(IFNULL(fg.TotalPacked,0) - IFNULL(sh.TotalShipped,0),3) AS FGAvailable" +
                " FROM PP_Products p" +
                " JOIN MM_UOM ou ON ou.UOMID=p.OutputUOMID" +
                " LEFT JOIN (SELECT ProductID, SUM(QtyPacked) AS TotalPacked" +
                "   FROM PK_FGStock GROUP BY ProductID) fg ON fg.ProductID=p.ProductID" +
                " LEFT JOIN (SELECT ProductID, SUM(TotalUnits) AS TotalSecondary" +
                "   FROM PK_SecondaryPacking GROUP BY ProductID) sp ON sp.ProductID=p.ProductID" +
                " LEFT JOIN (SELECT sl.ProductID, SUM(sl.QtyShipped) AS TotalShipped" +
                "   FROM PK_ShipmentLine sl JOIN PK_Shipment s ON s.ShipmentID=sl.ShipmentID" +
                "   WHERE s.Status!='Cancelled' GROUP BY sl.ProductID) sh ON sh.ProductID=p.ProductID" +
                " WHERE p.IsActive=1" +
                " ORDER BY p.ProductName;");
        }

        // ── PACKING MATERIALS (from MM) ───────────────────────────────────────
        public static DataTable GetActivePackingMaterials()
        {
            return ExecuteQuery(
                "SELECT pm.PMID, pm.PMCode, pm.PMName, u.Abbreviation," +
                " ROUND(IFNULL(grn.TotalGRN,0) - IFNULL(con.TotalUsed,0), 4) AS CurrentStock" +
                " FROM MM_PackingMaterials pm" +
                " JOIN MM_UOM u ON u.UOMID=pm.UOMID" +
                " LEFT JOIN (SELECT PMID, SUM(QtyActualReceived) AS TotalGRN" +
                "   FROM MM_PackingInward GROUP BY PMID) grn ON grn.PMID=pm.PMID" +
                " LEFT JOIN (SELECT PMID, SUM(QtyUsed) AS TotalUsed" +
                "   FROM PK_PMConsumption GROUP BY PMID) con ON con.PMID=pm.PMID" +
                " WHERE pm.IsActive=1 ORDER BY pm.PMName;");
        }

        // ── PRODUCTS (from PP) ────────────────────────────────────────────────
        public static DataTable GetActiveProducts()
        {
            return ExecuteQuery(
                "SELECT p.ProductID, p.ProductCode, p.ProductName, ou.Abbreviation AS Unit" +
                " FROM PP_Products p JOIN MM_UOM ou ON ou.UOMID=p.OutputUOMID" +
                " WHERE p.IsActive=1 ORDER BY p.ProductName;");
        }
    }
}
