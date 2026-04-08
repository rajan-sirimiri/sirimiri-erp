-- MySQL dump 10.13  Distrib 8.0.44, for Win64 (x86_64)
--
-- Host: localhost    Database: stockdb
-- ------------------------------------------------------
-- Server version	8.0.44

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `auditlog`
--

DROP TABLE IF EXISTS `auditlog`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `auditlog` (
  `AuditID` int NOT NULL AUTO_INCREMENT,
  `UserID` int NOT NULL,
  `Action` varchar(50) NOT NULL,
  `DistributorID` int DEFAULT NULL,
  `StockValue` int DEFAULT NULL,
  `IPAddress` varchar(45) DEFAULT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`AuditID`),
  KEY `FK_Audit_Users` (`UserID`),
  CONSTRAINT `FK_Audit_Users` FOREIGN KEY (`UserID`) REFERENCES `users` (`UserID`)
) ENGINE=InnoDB AUTO_INCREMENT=89 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cities`
--

DROP TABLE IF EXISTS `cities`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cities` (
  `CityID` int NOT NULL,
  `CityName` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `StateID` int NOT NULL,
  PRIMARY KEY (`CityID`),
  KEY `FK_Cities_States` (`StateID`),
  CONSTRAINT `FK_Cities_States` FOREIGN KEY (`StateID`) REFERENCES `states` (`StateID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `dailysalesentries`
--

DROP TABLE IF EXISTS `dailysalesentries`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `dailysalesentries` (
  `EntryID` int NOT NULL AUTO_INCREMENT,
  `EntryDate` date NOT NULL,
  `DistributorID` int NOT NULL,
  `ProductID` int NOT NULL,
  `QuantitySold` int NOT NULL DEFAULT '0',
  `Remarks` varchar(500) DEFAULT NULL,
  `SubmittedBy` int NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`EntryID`),
  KEY `IX_DailySalesEntries_Date` (`EntryDate`),
  KEY `IX_DailySalesEntries_Distributor` (`DistributorID`),
  KEY `IX_DailySalesEntries_Product` (`ProductID`),
  KEY `FK_DSE_User` (`SubmittedBy`),
  CONSTRAINT `FK_DSE_Distributor` FOREIGN KEY (`DistributorID`) REFERENCES `distributors` (`DistributorID`),
  CONSTRAINT `FK_DSE_Product` FOREIGN KEY (`ProductID`) REFERENCES `products` (`ProductID`),
  CONSTRAINT `FK_DSE_User` FOREIGN KEY (`SubmittedBy`) REFERENCES `users` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `distributors`
--

DROP TABLE IF EXISTS `distributors`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `distributors` (
  `DistributorID` int NOT NULL AUTO_INCREMENT,
  `DistributorName` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CityID` int NOT NULL,
  `PinCode` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `FullAddress` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`DistributorID`),
  KEY `FK_Distributors_Cities` (`CityID`),
  CONSTRAINT `FK_Distributors_Cities` FOREIGN KEY (`CityID`) REFERENCES `cities` (`CityID`)
) ENGINE=InnoDB AUTO_INCREMENT=1209 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `erp_applications`
--

DROP TABLE IF EXISTS `erp_applications`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `erp_applications` (
  `AppCode` varchar(10) NOT NULL,
  `AppName` varchar(100) NOT NULL,
  `AppUrl` varchar(200) NOT NULL,
  `SortOrder` int NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`AppCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `erp_modules`
--

DROP TABLE IF EXISTS `erp_modules`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `erp_modules` (
  `ModuleID` int NOT NULL AUTO_INCREMENT,
  `AppCode` varchar(10) NOT NULL,
  `ModuleCode` varchar(30) NOT NULL,
  `ModuleName` varchar(100) NOT NULL,
  `PageUrl` varchar(200) DEFAULT NULL COMMENT 'The ASPX page this module maps to',
  `SortOrder` int NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`ModuleID`),
  UNIQUE KEY `uk_app_module` (`AppCode`,`ModuleCode`),
  KEY `idx_app` (`AppCode`)
) ENGINE=InnoDB AUTO_INCREMENT=41 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `erp_roleappaccess`
--

DROP TABLE IF EXISTS `erp_roleappaccess`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `erp_roleappaccess` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `RoleCode` varchar(30) NOT NULL,
  `AppCode` varchar(10) NOT NULL,
  `CanAccess` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`ID`),
  UNIQUE KEY `uk_role_app` (`RoleCode`,`AppCode`)
) ENGINE=InnoDB AUTO_INCREMENT=56 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `erp_rolemoduleaccess`
--

DROP TABLE IF EXISTS `erp_rolemoduleaccess`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `erp_rolemoduleaccess` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `RoleCode` varchar(30) NOT NULL,
  `AppCode` varchar(10) NOT NULL,
  `ModuleCode` varchar(30) NOT NULL,
  `CanAccess` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`ID`),
  UNIQUE KEY `uk_role_app_mod` (`RoleCode`,`AppCode`,`ModuleCode`)
) ENGINE=InnoDB AUTO_INCREMENT=416 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `erp_roles`
--

DROP TABLE IF EXISTS `erp_roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `erp_roles` (
  `RoleID` int NOT NULL AUTO_INCREMENT,
  `RoleCode` varchar(30) NOT NULL,
  `RoleName` varchar(100) NOT NULL,
  `Description` varchar(300) DEFAULT NULL,
  `SortOrder` int NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`RoleID`),
  UNIQUE KEY `RoleCode` (`RoleCode`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `erp_ssotokens`
--

DROP TABLE IF EXISTS `erp_ssotokens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `erp_ssotokens` (
  `Token` varchar(64) NOT NULL,
  `UserID` int NOT NULL,
  `FullName` varchar(150) NOT NULL,
  `Role` varchar(50) NOT NULL DEFAULT 'Admin',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ExpiresAt` datetime NOT NULL,
  `IsUsed` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Token`),
  KEY `idx_expires` (`ExpiresAt`),
  KEY `idx_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fin_importbatch`
--

DROP TABLE IF EXISTS `fin_importbatch`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fin_importbatch` (
  `BatchID` int NOT NULL AUTO_INCREMENT,
  `ImportType` varchar(20) NOT NULL COMMENT 'SALES, PURCHASE, JOURNAL, RECEIPT',
  `FileName` varchar(200) DEFAULT NULL,
  `ImportedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ImportedBy` int NOT NULL,
  `RowsTotal` int NOT NULL DEFAULT '0',
  `RowsInserted` int NOT NULL DEFAULT '0',
  `RowsSkipped` int NOT NULL DEFAULT '0',
  `RowsError` int NOT NULL DEFAULT '0',
  `Status` varchar(20) NOT NULL DEFAULT 'COMPLETED',
  PRIMARY KEY (`BatchID`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fin_purchaseinvoice`
--

DROP TABLE IF EXISTS `fin_purchaseinvoice`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fin_purchaseinvoice` (
  `InvoiceID` int NOT NULL AUTO_INCREMENT,
  `SupplierInvNo` varchar(100) NOT NULL,
  `InvoiceDate` date NOT NULL,
  `SupplierID` int DEFAULT NULL COMMENT 'FK to MM_Suppliers',
  `TallySupplierName` varchar(300) DEFAULT NULL,
  `TotalQty` decimal(12,2) NOT NULL DEFAULT '0.00',
  `TotalValue` decimal(14,2) NOT NULL DEFAULT '0.00',
  `ImportBatchID` int DEFAULT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`InvoiceID`),
  UNIQUE KEY `uk_supplierinvno` (`SupplierInvNo`),
  KEY `idx_date` (`InvoiceDate`),
  KEY `idx_supplier` (`SupplierID`),
  KEY `idx_batch` (`ImportBatchID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fin_purchaseinvoiceline`
--

DROP TABLE IF EXISTS `fin_purchaseinvoiceline`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fin_purchaseinvoiceline` (
  `LineID` int NOT NULL AUTO_INCREMENT,
  `InvoiceID` int NOT NULL,
  `MaterialType` varchar(10) DEFAULT NULL COMMENT 'RM, PM, CN, ST, SCRAP, CAPEX, OTHER',
  `MaterialID` int DEFAULT NULL,
  `TallyItemName` varchar(300) DEFAULT NULL,
  `Quantity` decimal(12,3) NOT NULL DEFAULT '0.000',
  `Value` decimal(14,2) NOT NULL DEFAULT '0.00',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`LineID`),
  KEY `idx_invoice` (`InvoiceID`),
  KEY `idx_material` (`MaterialType`,`MaterialID`),
  CONSTRAINT `FK_PurchLine_Invoice` FOREIGN KEY (`InvoiceID`) REFERENCES `fin_purchaseinvoice` (`InvoiceID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fin_salesinvoice`
--

DROP TABLE IF EXISTS `fin_salesinvoice`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fin_salesinvoice` (
  `InvoiceID` int NOT NULL AUTO_INCREMENT,
  `VoucherNo` varchar(50) NOT NULL,
  `InvoiceDate` date NOT NULL,
  `CustomerID` int DEFAULT NULL COMMENT 'FK to PK_Customers',
  `TallyCustomerName` varchar(300) DEFAULT NULL COMMENT 'Original name from Tally',
  `BuyerAddress` varchar(500) DEFAULT NULL,
  `TotalQty` decimal(12,2) NOT NULL DEFAULT '0.00',
  `TotalValue` decimal(14,2) NOT NULL DEFAULT '0.00',
  `ImportBatchID` int DEFAULT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`InvoiceID`),
  UNIQUE KEY `uk_voucherno` (`VoucherNo`),
  KEY `idx_date` (`InvoiceDate`),
  KEY `idx_customer` (`CustomerID`),
  KEY `idx_batch` (`ImportBatchID`)
) ENGINE=InnoDB AUTO_INCREMENT=6914 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fin_salesinvoiceline`
--

DROP TABLE IF EXISTS `fin_salesinvoiceline`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fin_salesinvoiceline` (
  `LineID` int NOT NULL AUTO_INCREMENT,
  `InvoiceID` int NOT NULL,
  `ProductID` int DEFAULT NULL COMMENT 'FK to PP_Products (NULL if scrap)',
  `ScrapID` int DEFAULT NULL COMMENT 'FK to MM_ScrapMaterials (NULL if product)',
  `TallyProductName` varchar(300) DEFAULT NULL COMMENT 'Original name from Tally',
  `SellingForm` varchar(10) DEFAULT 'PCS',
  `PiecesPerUnit` int DEFAULT '1',
  `Quantity` decimal(12,2) NOT NULL DEFAULT '0.00',
  `Value` decimal(14,2) NOT NULL DEFAULT '0.00',
  `LineType` varchar(10) NOT NULL DEFAULT 'PRODUCT' COMMENT 'PRODUCT or SCRAP',
  PRIMARY KEY (`LineID`),
  KEY `idx_invoice` (`InvoiceID`),
  KEY `idx_product` (`ProductID`),
  CONSTRAINT `FK_SILine_Invoice` FOREIGN KEY (`InvoiceID`) REFERENCES `fin_salesinvoice` (`InvoiceID`)
) ENGINE=InnoDB AUTO_INCREMENT=28932 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fin_tallycustomermap`
--

DROP TABLE IF EXISTS `fin_tallycustomermap`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fin_tallycustomermap` (
  `MapID` int NOT NULL AUTO_INCREMENT,
  `TallyName` varchar(300) NOT NULL,
  `CustomerID` int NOT NULL COMMENT 'FK to PK_Customers',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`MapID`),
  UNIQUE KEY `uk_tallyname` (`TallyName`(250)),
  KEY `idx_customerid` (`CustomerID`)
) ENGINE=InnoDB AUTO_INCREMENT=1211 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fin_tallyitemmap`
--

DROP TABLE IF EXISTS `fin_tallyitemmap`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fin_tallyitemmap` (
  `MapID` int NOT NULL AUTO_INCREMENT,
  `TallyName` varchar(300) NOT NULL,
  `MaterialType` varchar(10) NOT NULL COMMENT 'RM, PM, CN, ST, SCRAP, CAPEX, OTHER',
  `MaterialID` int DEFAULT NULL COMMENT 'FK to RM/PM/CN/ST/Scrap table based on MaterialType',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`MapID`),
  UNIQUE KEY `uk_tallyname` (`TallyName`(250))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fin_tallyproductmap`
--

DROP TABLE IF EXISTS `fin_tallyproductmap`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fin_tallyproductmap` (
  `MapID` int NOT NULL AUTO_INCREMENT,
  `TallyName` varchar(300) NOT NULL,
  `ProductID` int NOT NULL COMMENT 'FK to PP_Products',
  `SellingForm` varchar(10) NOT NULL DEFAULT 'PCS' COMMENT 'PCS, JAR, CASE, TRAY',
  `PiecesPerUnit` int NOT NULL DEFAULT '1' COMMENT 'How many core pieces in one selling unit',
  `MRP` decimal(10,2) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`MapID`),
  UNIQUE KEY `uk_tallyname` (`TallyName`(250)),
  KEY `idx_productid` (`ProductID`)
) ENGINE=InnoDB AUTO_INCREMENT=83 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fin_tallyscrapmap`
--

DROP TABLE IF EXISTS `fin_tallyscrapmap`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fin_tallyscrapmap` (
  `MapID` int NOT NULL AUTO_INCREMENT,
  `TallyName` varchar(300) NOT NULL,
  `ScrapID` int NOT NULL COMMENT 'FK to MM_ScrapMaterials',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`MapID`),
  UNIQUE KEY `uk_tallyname` (`TallyName`(250)),
  KEY `idx_scrapid` (`ScrapID`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fin_tallysuppliermap`
--

DROP TABLE IF EXISTS `fin_tallysuppliermap`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fin_tallysuppliermap` (
  `MapID` int NOT NULL AUTO_INCREMENT,
  `TallyName` varchar(300) NOT NULL,
  `SupplierID` int DEFAULT NULL COMMENT 'FK to MM_Suppliers',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`MapID`),
  UNIQUE KEY `uk_tallyname` (`TallyName`(250)),
  KEY `idx_supplierid` (`SupplierID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_consumableinward`
--

DROP TABLE IF EXISTS `mm_consumableinward`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_consumableinward` (
  `InwardID` int NOT NULL AUTO_INCREMENT,
  `GRNNo` varchar(25) NOT NULL,
  `InvoiceNo` varchar(50) DEFAULT NULL,
  `InvoiceDate` date DEFAULT NULL,
  `InwardDate` date NOT NULL,
  `SupplierID` int NOT NULL,
  `ConsumableID` int NOT NULL,
  `Quantity` decimal(12,3) NOT NULL,
  `QtyActualReceived` decimal(12,3) DEFAULT NULL,
  `QtyInUOM` decimal(12,3) DEFAULT NULL,
  `Rate` decimal(12,2) NOT NULL DEFAULT '0.00',
  `Amount` decimal(14,2) NOT NULL DEFAULT '0.00',
  `HSNCode` varchar(20) DEFAULT NULL,
  `GSTRate` decimal(5,2) DEFAULT NULL,
  `GSTAmount` decimal(12,2) DEFAULT NULL,
  `TransportCost` decimal(12,2) DEFAULT NULL,
  `TransportInInvoice` tinyint(1) NOT NULL DEFAULT '0',
  `TransportInGST` tinyint(1) NOT NULL DEFAULT '0',
  `LoadingCharges` decimal(12,2) DEFAULT '0.00',
  `UnloadingCharges` decimal(12,2) DEFAULT '0.00',
  `QtyVerified` tinyint(1) DEFAULT '0',
  `ShortageQty` decimal(12,3) DEFAULT NULL,
  `ShortageValue` decimal(12,2) DEFAULT NULL,
  `PONo` varchar(50) DEFAULT NULL,
  `Remarks` varchar(300) DEFAULT NULL,
  `QualityCheck` tinyint(1) NOT NULL DEFAULT '0',
  `Status` varchar(20) NOT NULL DEFAULT 'Received',
  `CreatedBy` int NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`InwardID`),
  UNIQUE KEY `GRNNo` (`GRNNo`),
  KEY `FK_ConsInward_Supplier` (`SupplierID`),
  KEY `FK_ConsInward_Consumable` (`ConsumableID`),
  KEY `FK_ConsInward_User` (`CreatedBy`),
  CONSTRAINT `FK_ConsInward_Consumable` FOREIGN KEY (`ConsumableID`) REFERENCES `mm_consumables` (`ConsumableID`),
  CONSTRAINT `FK_ConsInward_Supplier` FOREIGN KEY (`SupplierID`) REFERENCES `mm_suppliers` (`SupplierID`),
  CONSTRAINT `FK_ConsInward_User` FOREIGN KEY (`CreatedBy`) REFERENCES `users` (`UserID`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_consumables`
--

DROP TABLE IF EXISTS `mm_consumables`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_consumables` (
  `ConsumableID` int NOT NULL AUTO_INCREMENT,
  `ConsumableCode` varchar(20) NOT NULL,
  `ConsumableName` varchar(200) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `HSNCode` varchar(20) DEFAULT NULL,
  `GSTRate` decimal(5,2) DEFAULT NULL,
  `UOMID` int NOT NULL,
  `ReorderLevel` decimal(12,3) NOT NULL DEFAULT '0.000',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`ConsumableID`),
  UNIQUE KEY `ConsumableCode` (`ConsumableCode`),
  KEY `UOMID` (`UOMID`),
  CONSTRAINT `mm_consumables_ibfk_1` FOREIGN KEY (`UOMID`) REFERENCES `mm_uom` (`UOMID`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_grncounter`
--

DROP TABLE IF EXISTS `mm_grncounter`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_grncounter` (
  `CounterType` varchar(10) NOT NULL,
  `LastValue` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`CounterType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_grnsequence`
--

DROP TABLE IF EXISTS `mm_grnsequence`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_grnsequence` (
  `SeqID` int NOT NULL AUTO_INCREMENT,
  `Prefix` varchar(20) NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`SeqID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_openingstock`
--

DROP TABLE IF EXISTS `mm_openingstock`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_openingstock` (
  `OpeningStockID` int NOT NULL AUTO_INCREMENT,
  `MaterialType` varchar(5) NOT NULL,
  `MaterialID` int NOT NULL,
  `Quantity` decimal(12,3) NOT NULL DEFAULT '0.000',
  `Rate` decimal(12,2) NOT NULL DEFAULT '0.00',
  `Value` decimal(14,2) GENERATED ALWAYS AS ((`Quantity` * `Rate`)) STORED,
  `AsOfDate` date NOT NULL,
  `Remarks` varchar(300) DEFAULT NULL,
  `CreatedBy` int NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`OpeningStockID`),
  UNIQUE KEY `uq_opening_stock` (`MaterialType`,`MaterialID`),
  KEY `FK_OS_User` (`CreatedBy`),
  CONSTRAINT `FK_OS_User` FOREIGN KEY (`CreatedBy`) REFERENCES `users` (`UserID`)
) ENGINE=InnoDB AUTO_INCREMENT=532 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_packinginward`
--

DROP TABLE IF EXISTS `mm_packinginward`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_packinginward` (
  `InwardID` int NOT NULL AUTO_INCREMENT,
  `GRNNo` varchar(25) NOT NULL,
  `InvoiceNo` varchar(50) DEFAULT NULL,
  `InvoiceDate` date DEFAULT NULL,
  `InwardDate` date NOT NULL,
  `SupplierID` int NOT NULL,
  `PMID` int NOT NULL,
  `Quantity` decimal(12,3) NOT NULL,
  `QtyInUOM` decimal(12,3) DEFAULT NULL,
  `QtyActualReceived` decimal(12,3) DEFAULT NULL,
  `ShortageQty` decimal(12,3) DEFAULT NULL,
  `ShortageValue` decimal(12,2) DEFAULT NULL,
  `Rate` decimal(12,2) NOT NULL DEFAULT '0.00',
  `HSNCode` varchar(20) DEFAULT NULL,
  `GSTRate` decimal(5,2) DEFAULT NULL,
  `GSTAmount` decimal(12,2) DEFAULT NULL,
  `TransportCost` decimal(12,2) DEFAULT NULL,
  `TransportInInvoice` tinyint(1) NOT NULL DEFAULT '0',
  `TransportInGST` tinyint(1) NOT NULL DEFAULT '0',
  `LoadingCharges` decimal(12,2) DEFAULT '0.00',
  `UnloadingCharges` decimal(12,2) DEFAULT '0.00',
  `QtyVerified` tinyint(1) DEFAULT '0',
  `Amount` decimal(14,2) NOT NULL DEFAULT '0.00',
  `PONo` varchar(50) DEFAULT NULL,
  `Remarks` varchar(300) DEFAULT NULL,
  `QualityCheck` tinyint(1) NOT NULL DEFAULT '0',
  `Status` varchar(20) NOT NULL DEFAULT 'Received',
  `CreatedBy` int NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`InwardID`),
  UNIQUE KEY `UQ_PackingInward_GRN` (`GRNNo`),
  KEY `FK_PackingInward_Supplier` (`SupplierID`),
  KEY `FK_PackingInward_PM` (`PMID`),
  KEY `FK_PackingInward_User` (`CreatedBy`),
  CONSTRAINT `FK_PackingInward_PM` FOREIGN KEY (`PMID`) REFERENCES `mm_packingmaterials` (`PMID`),
  CONSTRAINT `FK_PackingInward_Supplier` FOREIGN KEY (`SupplierID`) REFERENCES `mm_suppliers` (`SupplierID`),
  CONSTRAINT `FK_PackingInward_User` FOREIGN KEY (`CreatedBy`) REFERENCES `users` (`UserID`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_packingmaterials`
--

DROP TABLE IF EXISTS `mm_packingmaterials`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_packingmaterials` (
  `PMID` int NOT NULL AUTO_INCREMENT,
  `PMCode` varchar(20) NOT NULL,
  `PMName` varchar(150) NOT NULL,
  `PMCategory` varchar(20) DEFAULT NULL COMMENT 'CARTON, LABEL, LID, JAR, ROLL, WRAP, OTHER',
  `Description` varchar(300) DEFAULT NULL,
  `HSNCode` varchar(20) DEFAULT NULL,
  `GSTRate` decimal(5,2) DEFAULT NULL,
  `UOMID` int NOT NULL,
  `ReorderLevel` decimal(10,3) NOT NULL DEFAULT '0.000',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`PMID`),
  UNIQUE KEY `UQ_PM_Code` (`PMCode`),
  KEY `FK_PM_UOM` (`UOMID`),
  CONSTRAINT `FK_PM_UOM` FOREIGN KEY (`UOMID`) REFERENCES `mm_uom` (`UOMID`)
) ENGINE=InnoDB AUTO_INCREMENT=76 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_physicalstock`
--

DROP TABLE IF EXISTS `mm_physicalstock`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_physicalstock` (
  `PhysicalID` int NOT NULL AUTO_INCREMENT,
  `SessionDate` date NOT NULL,
  `MaterialType` varchar(5) NOT NULL COMMENT 'RM, PM, CM, ST',
  `MaterialID` int NOT NULL,
  `PhysicalQty` decimal(14,4) NOT NULL DEFAULT '0.0000',
  `EnteredBy` int NOT NULL,
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`PhysicalID`),
  UNIQUE KEY `uk_session` (`SessionDate`,`MaterialType`,`MaterialID`),
  KEY `idx_phys_date` (`SessionDate`)
) ENGINE=InnoDB AUTO_INCREMENT=34 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_pmcategories`
--

DROP TABLE IF EXISTS `mm_pmcategories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_pmcategories` (
  `CategoryID` int NOT NULL AUTO_INCREMENT,
  `CategoryName` varchar(50) NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`CategoryID`),
  UNIQUE KEY `uq_catname` (`CategoryName`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_rawinward`
--

DROP TABLE IF EXISTS `mm_rawinward`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_rawinward` (
  `InwardID` int NOT NULL AUTO_INCREMENT,
  `GRNNo` varchar(25) NOT NULL,
  `InvoiceNo` varchar(50) DEFAULT NULL,
  `InvoiceDate` date DEFAULT NULL,
  `InwardDate` date NOT NULL,
  `SupplierID` int NOT NULL,
  `RMID` int NOT NULL,
  `Quantity` decimal(12,3) NOT NULL,
  `QtyInUOM` decimal(12,3) DEFAULT NULL,
  `QtyActualReceived` decimal(12,3) DEFAULT NULL,
  `ShortageQty` decimal(12,3) DEFAULT NULL,
  `ShortageValue` decimal(12,2) DEFAULT NULL,
  `Rate` decimal(12,2) NOT NULL DEFAULT '0.00',
  `HSNCode` varchar(20) DEFAULT NULL,
  `GSTRate` decimal(5,2) DEFAULT NULL,
  `GSTAmount` decimal(12,2) DEFAULT NULL,
  `TransportCost` decimal(12,2) DEFAULT NULL,
  `TransportInInvoice` tinyint(1) NOT NULL DEFAULT '0',
  `TransportInGST` tinyint(1) NOT NULL DEFAULT '0',
  `LoadingCharges` decimal(12,2) DEFAULT '0.00',
  `UnloadingCharges` decimal(12,2) DEFAULT '0.00',
  `QtyVerified` tinyint(1) DEFAULT '0',
  `Amount` decimal(14,2) NOT NULL DEFAULT '0.00',
  `PONo` varchar(50) DEFAULT NULL,
  `Remarks` varchar(300) DEFAULT NULL,
  `QualityCheck` tinyint(1) NOT NULL DEFAULT '0',
  `Status` varchar(20) NOT NULL DEFAULT 'Received',
  `CreatedBy` int NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`InwardID`),
  UNIQUE KEY `UQ_RawInward_GRN` (`GRNNo`),
  KEY `FK_RawInward_Supplier` (`SupplierID`),
  KEY `FK_RawInward_RM` (`RMID`),
  KEY `FK_RawInward_User` (`CreatedBy`),
  CONSTRAINT `FK_RawInward_RM` FOREIGN KEY (`RMID`) REFERENCES `mm_rawmaterials` (`RMID`),
  CONSTRAINT `FK_RawInward_Supplier` FOREIGN KEY (`SupplierID`) REFERENCES `mm_suppliers` (`SupplierID`),
  CONSTRAINT `FK_RawInward_User` FOREIGN KEY (`CreatedBy`) REFERENCES `users` (`UserID`)
) ENGINE=InnoDB AUTO_INCREMENT=72 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_rawmaterials`
--

DROP TABLE IF EXISTS `mm_rawmaterials`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_rawmaterials` (
  `RMID` int NOT NULL AUTO_INCREMENT,
  `RMCode` varchar(20) NOT NULL,
  `RMName` varchar(150) NOT NULL,
  `Description` varchar(300) DEFAULT NULL,
  `HSNCode` varchar(20) DEFAULT NULL,
  `GSTRate` decimal(5,2) DEFAULT NULL,
  `UOMID` int NOT NULL,
  `ReorderLevel` decimal(10,3) NOT NULL DEFAULT '0.000',
  `ConversionLossPct` decimal(5,2) DEFAULT NULL,
  `DerivedFromRMID` int DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`RMID`),
  UNIQUE KEY `UQ_RM_Code` (`RMCode`),
  KEY `FK_RM_UOM` (`UOMID`),
  CONSTRAINT `FK_RM_UOM` FOREIGN KEY (`UOMID`) REFERENCES `mm_uom` (`UOMID`)
) ENGINE=InnoDB AUTO_INCREMENT=104 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_rmscraplink`
--

DROP TABLE IF EXISTS `mm_rmscraplink`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_rmscraplink` (
  `LinkID` int NOT NULL AUTO_INCREMENT,
  `RMID` int NOT NULL,
  `ScrapID` int NOT NULL,
  PRIMARY KEY (`LinkID`),
  UNIQUE KEY `UQ_RMScrap` (`RMID`,`ScrapID`),
  KEY `FK_RMScrap_Scrap` (`ScrapID`),
  CONSTRAINT `FK_RMScrap_RM` FOREIGN KEY (`RMID`) REFERENCES `mm_rawmaterials` (`RMID`),
  CONSTRAINT `FK_RMScrap_Scrap` FOREIGN KEY (`ScrapID`) REFERENCES `mm_scrapmaterials` (`ScrapID`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_scrapmaterials`
--

DROP TABLE IF EXISTS `mm_scrapmaterials`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_scrapmaterials` (
  `ScrapID` int NOT NULL AUTO_INCREMENT,
  `ScrapCode` varchar(20) NOT NULL,
  `ScrapName` varchar(150) NOT NULL,
  `Description` varchar(300) DEFAULT NULL,
  `UOMID` int NOT NULL,
  `CurrentPrice` decimal(12,2) NOT NULL DEFAULT '0.00',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ScrapID`),
  UNIQUE KEY `ScrapCode` (`ScrapCode`),
  KEY `FK_Scrap_UOM` (`UOMID`),
  CONSTRAINT `FK_Scrap_UOM` FOREIGN KEY (`UOMID`) REFERENCES `mm_uom` (`UOMID`)
) ENGINE=InnoDB AUTO_INCREMENT=20 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_scrappricehistory`
--

DROP TABLE IF EXISTS `mm_scrappricehistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_scrappricehistory` (
  `HistoryID` int NOT NULL AUTO_INCREMENT,
  `ScrapID` int NOT NULL,
  `Price` decimal(12,2) NOT NULL,
  `EffectiveDate` date NOT NULL,
  `Remarks` varchar(300) DEFAULT NULL,
  `CreatedBy` int NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`HistoryID`),
  KEY `FK_SPH_Scrap` (`ScrapID`),
  CONSTRAINT `FK_SPH_Scrap` FOREIGN KEY (`ScrapID`) REFERENCES `mm_scrapmaterials` (`ScrapID`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_scrapstock`
--

DROP TABLE IF EXISTS `mm_scrapstock`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_scrapstock` (
  `ScrapStockID` int NOT NULL AUTO_INCREMENT,
  `ScrapID` int NOT NULL,
  `QtyGenerated` decimal(18,6) NOT NULL,
  `GeneratedAt` datetime NOT NULL,
  `SourceRMName` varchar(150) NOT NULL,
  `Remarks` varchar(300) DEFAULT NULL,
  `CreatedBy` int NOT NULL,
  PRIMARY KEY (`ScrapStockID`),
  KEY `FK_ScrapStock_Scrap` (`ScrapID`),
  CONSTRAINT `FK_ScrapStock_Scrap` FOREIGN KEY (`ScrapID`) REFERENCES `mm_scrapmaterials` (`ScrapID`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_stationaries`
--

DROP TABLE IF EXISTS `mm_stationaries`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_stationaries` (
  `StationaryID` int NOT NULL AUTO_INCREMENT,
  `StationaryCode` varchar(20) NOT NULL,
  `StationaryName` varchar(200) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `HSNCode` varchar(20) DEFAULT NULL,
  `GSTRate` decimal(5,2) DEFAULT NULL,
  `UOMID` int NOT NULL,
  `ReorderLevel` decimal(12,3) NOT NULL DEFAULT '0.000',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`StationaryID`),
  UNIQUE KEY `StationaryCode` (`StationaryCode`),
  KEY `UOMID` (`UOMID`),
  CONSTRAINT `mm_stationaries_ibfk_1` FOREIGN KEY (`UOMID`) REFERENCES `mm_uom` (`UOMID`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_stationaryinward`
--

DROP TABLE IF EXISTS `mm_stationaryinward`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_stationaryinward` (
  `InwardID` int NOT NULL AUTO_INCREMENT,
  `GRNNo` varchar(25) NOT NULL,
  `InvoiceNo` varchar(50) DEFAULT NULL,
  `InvoiceDate` date DEFAULT NULL,
  `InwardDate` date NOT NULL,
  `SupplierID` int NOT NULL,
  `StationaryID` int NOT NULL,
  `Quantity` decimal(12,3) NOT NULL,
  `QtyActualReceived` decimal(12,3) DEFAULT NULL,
  `QtyInUOM` decimal(12,3) DEFAULT NULL,
  `Rate` decimal(12,2) NOT NULL DEFAULT '0.00',
  `Amount` decimal(14,2) NOT NULL DEFAULT '0.00',
  `HSNCode` varchar(20) DEFAULT NULL,
  `GSTRate` decimal(5,2) DEFAULT NULL,
  `GSTAmount` decimal(12,2) DEFAULT NULL,
  `TransportCost` decimal(12,2) DEFAULT NULL,
  `TransportInInvoice` tinyint(1) NOT NULL DEFAULT '0',
  `TransportInGST` tinyint(1) NOT NULL DEFAULT '0',
  `LoadingCharges` decimal(12,2) DEFAULT '0.00',
  `UnloadingCharges` decimal(12,2) DEFAULT '0.00',
  `QtyVerified` tinyint(1) DEFAULT '0',
  `ShortageQty` decimal(12,3) DEFAULT NULL,
  `ShortageValue` decimal(12,2) DEFAULT NULL,
  `PONo` varchar(50) DEFAULT NULL,
  `Remarks` varchar(300) DEFAULT NULL,
  `QualityCheck` tinyint(1) NOT NULL DEFAULT '0',
  `Status` varchar(20) NOT NULL DEFAULT 'Received',
  `CreatedBy` int NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`InwardID`),
  UNIQUE KEY `GRNNo` (`GRNNo`),
  KEY `FK_StatInward_Supplier` (`SupplierID`),
  KEY `FK_StatInward_Stationary` (`StationaryID`),
  KEY `FK_StatInward_User` (`CreatedBy`),
  CONSTRAINT `FK_StatInward_Stationary` FOREIGN KEY (`StationaryID`) REFERENCES `mm_stationaries` (`StationaryID`),
  CONSTRAINT `FK_StatInward_Supplier` FOREIGN KEY (`SupplierID`) REFERENCES `mm_suppliers` (`SupplierID`),
  CONSTRAINT `FK_StatInward_User` FOREIGN KEY (`CreatedBy`) REFERENCES `users` (`UserID`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_stockconsumption`
--

DROP TABLE IF EXISTS `mm_stockconsumption`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_stockconsumption` (
  `ConsumptionID` int NOT NULL AUTO_INCREMENT,
  `ExecutionID` int NOT NULL,
  `OrderID` int NOT NULL,
  `BatchNo` int NOT NULL,
  `RMID` int NOT NULL,
  `SourceType` varchar(10) NOT NULL COMMENT 'OPENING or GRN',
  `SourceID` int NOT NULL COMMENT 'OpeningStockID or InwardID',
  `QtyConsumed` decimal(18,6) NOT NULL,
  `ConsumedAt` datetime NOT NULL,
  `CreatedBy` int NOT NULL,
  PRIMARY KEY (`ConsumptionID`),
  KEY `idx_execution` (`ExecutionID`),
  KEY `idx_order` (`OrderID`),
  KEY `idx_rm` (`RMID`)
) ENGINE=InnoDB AUTO_INCREMENT=93 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_stockreconciliation`
--

DROP TABLE IF EXISTS `mm_stockreconciliation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_stockreconciliation` (
  `ReconID` int NOT NULL AUTO_INCREMENT,
  `ReconDate` date NOT NULL,
  `MaterialType` varchar(5) NOT NULL,
  `MaterialID` int NOT NULL,
  `PhysicalQty` decimal(14,4) NOT NULL DEFAULT '0.0000',
  `SystemQty` decimal(14,4) NOT NULL DEFAULT '0.0000',
  `Variance` decimal(14,4) NOT NULL DEFAULT '0.0000',
  `VariancePct` decimal(8,2) NOT NULL DEFAULT '0.00',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ReconID`),
  KEY `idx_recon_date` (`ReconDate`),
  KEY `idx_recon_material` (`MaterialType`,`MaterialID`)
) ENGINE=InnoDB AUTO_INCREMENT=41 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_suppliers`
--

DROP TABLE IF EXISTS `mm_suppliers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_suppliers` (
  `SupplierID` int NOT NULL AUTO_INCREMENT,
  `SupplierCode` varchar(20) NOT NULL,
  `SupplierName` varchar(150) NOT NULL,
  `ContactPerson` varchar(100) DEFAULT NULL,
  `Phone` varchar(20) DEFAULT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `GSTNo` varchar(20) DEFAULT NULL,
  `PAN` varchar(15) DEFAULT NULL,
  `Address` varchar(300) DEFAULT NULL,
  `City` varchar(80) DEFAULT NULL,
  `State` varchar(80) DEFAULT NULL,
  `PinCode` varchar(10) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`SupplierID`),
  UNIQUE KEY `UQ_Supplier_Code` (`SupplierCode`)
) ENGINE=InnoDB AUTO_INCREMENT=310 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_uom`
--

DROP TABLE IF EXISTS `mm_uom`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_uom` (
  `UOMID` int NOT NULL AUTO_INCREMENT,
  `UOMName` varchar(50) NOT NULL,
  `Abbreviation` varchar(10) NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`UOMID`),
  UNIQUE KEY `UQ_UOM_Name` (`UOMName`)
) ENGINE=InnoDB AUTO_INCREMENT=35 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mm_useraccess`
--

DROP TABLE IF EXISTS `mm_useraccess`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mm_useraccess` (
  `AccessID` int NOT NULL AUTO_INCREMENT,
  `UserID` int NOT NULL,
  `Module` varchar(50) NOT NULL,
  `CanView` tinyint(1) NOT NULL DEFAULT '0',
  `CanEdit` tinyint(1) NOT NULL DEFAULT '0',
  `GrantedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`AccessID`),
  UNIQUE KEY `UQ_UserAccess` (`UserID`,`Module`),
  CONSTRAINT `FK_MM_Access_User` FOREIGN KEY (`UserID`) REFERENCES `users` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pk_customerpo`
--

DROP TABLE IF EXISTS `pk_customerpo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pk_customerpo` (
  `POID` int NOT NULL AUTO_INCREMENT,
  `POCode` varchar(30) NOT NULL,
  `CustomerID` int NOT NULL,
  `PODate` date NOT NULL,
  `DeliveryDate` date DEFAULT NULL,
  `Status` varchar(20) NOT NULL DEFAULT 'Open',
  `Remarks` varchar(300) DEFAULT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `CreatedBy` int NOT NULL,
  PRIMARY KEY (`POID`),
  UNIQUE KEY `POCode` (`POCode`),
  KEY `FK_PO_Customer` (`CustomerID`),
  CONSTRAINT `FK_PO_Customer` FOREIGN KEY (`CustomerID`) REFERENCES `pk_customers` (`CustomerID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pk_customerpoline`
--

DROP TABLE IF EXISTS `pk_customerpoline`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pk_customerpoline` (
  `LineID` int NOT NULL AUTO_INCREMENT,
  `POID` int NOT NULL,
  `ProductID` int NOT NULL,
  `QtyOrdered` decimal(18,3) NOT NULL,
  `QtyShipped` decimal(18,3) NOT NULL DEFAULT '0.000',
  `UnitPrice` decimal(18,2) DEFAULT NULL,
  PRIMARY KEY (`LineID`),
  KEY `FK_POLine_PO` (`POID`),
  KEY `FK_POLine_Product` (`ProductID`),
  CONSTRAINT `FK_POLine_PO` FOREIGN KEY (`POID`) REFERENCES `pk_customerpo` (`POID`),
  CONSTRAINT `FK_POLine_Product` FOREIGN KEY (`ProductID`) REFERENCES `pp_products` (`ProductID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pk_customers`
--

DROP TABLE IF EXISTS `pk_customers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pk_customers` (
  `CustomerID` int NOT NULL AUTO_INCREMENT,
  `CustomerCode` varchar(20) NOT NULL,
  `CustomerType` varchar(5) DEFAULT NULL COMMENT 'ST/DI/RT',
  `CustomerName` varchar(150) NOT NULL,
  `ContactPerson` varchar(100) DEFAULT NULL,
  `Phone` varchar(20) DEFAULT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `Address` varchar(300) DEFAULT NULL,
  `City` varchar(80) DEFAULT NULL,
  `State` varchar(80) DEFAULT NULL,
  `PinCode` varchar(10) DEFAULT NULL,
  `GSTIN` varchar(20) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`CustomerID`),
  UNIQUE KEY `CustomerCode` (`CustomerCode`)
) ENGINE=InnoDB AUTO_INCREMENT=2083 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pk_customertypes`
--

DROP TABLE IF EXISTS `pk_customertypes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pk_customertypes` (
  `TypeID` int NOT NULL AUTO_INCREMENT,
  `TypeCode` varchar(5) NOT NULL COMMENT 'Prefix for customer code: ST, DI, RT',
  `TypeName` varchar(50) NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`TypeID`),
  UNIQUE KEY `uq_typecode` (`TypeCode`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pk_dclines`
--

DROP TABLE IF EXISTS `pk_dclines`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pk_dclines` (
  `LineID` int NOT NULL AUTO_INCREMENT,
  `DCID` int NOT NULL,
  `ProductID` int NOT NULL,
  `Cases` int NOT NULL DEFAULT '0',
  `LooseJars` int NOT NULL DEFAULT '0',
  `JarsPerCase` int NOT NULL DEFAULT '12',
  `TotalPcs` int NOT NULL DEFAULT '0' COMMENT 'Cases*JarsPerCase*UnitSize + LooseJars*UnitSize',
  PRIMARY KEY (`LineID`),
  KEY `ix_dcid` (`DCID`),
  CONSTRAINT `fk_dcline_dc` FOREIGN KEY (`DCID`) REFERENCES `pk_deliverychallans` (`DCID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pk_deliverychallans`
--

DROP TABLE IF EXISTS `pk_deliverychallans`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pk_deliverychallans` (
  `DCID` int NOT NULL AUTO_INCREMENT,
  `DCNumber` varchar(20) NOT NULL,
  `CustomerID` int NOT NULL,
  `DCDate` date NOT NULL,
  `Status` varchar(10) NOT NULL DEFAULT 'DRAFT' COMMENT 'DRAFT or FINALISED',
  `Remarks` varchar(500) DEFAULT NULL,
  `CreatedBy` int DEFAULT NULL,
  `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  `FinalisedAt` datetime DEFAULT NULL,
  `FinalisedBy` int DEFAULT NULL,
  PRIMARY KEY (`DCID`),
  UNIQUE KEY `uq_dcnumber` (`DCNumber`),
  KEY `ix_customer` (`CustomerID`),
  KEY `ix_status` (`Status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pk_fgstock`
--

DROP TABLE IF EXISTS `pk_fgstock`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pk_fgstock` (
  `FGStockID` int NOT NULL AUTO_INCREMENT,
  `ProductID` int NOT NULL,
  `QtyPacked` decimal(18,3) NOT NULL,
  `PackedAt` datetime NOT NULL,
  `ExecutionID` int DEFAULT NULL,
  `OrderID` int DEFAULT NULL,
  `BatchNo` int DEFAULT NULL,
  `CreatedBy` int NOT NULL,
  PRIMARY KEY (`FGStockID`),
  KEY `FK_FGStock_Product` (`ProductID`),
  CONSTRAINT `FK_FGStock_Product` FOREIGN KEY (`ProductID`) REFERENCES `pp_products` (`ProductID`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pk_packingexecution`
--

DROP TABLE IF EXISTS `pk_packingexecution`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pk_packingexecution` (
  `PackingID` int NOT NULL AUTO_INCREMENT,
  `OrderID` int NOT NULL,
  `BatchNo` int NOT NULL,
  `StartTime` datetime NOT NULL,
  `EndTime` datetime DEFAULT NULL,
  `Cases` int DEFAULT NULL,
  `Jars` int DEFAULT NULL,
  `Units` int DEFAULT NULL,
  `JarSize` int DEFAULT NULL,
  `TotalUnits` int DEFAULT NULL,
  `Status` varchar(20) NOT NULL DEFAULT 'InProgress',
  `CreatedBy` int NOT NULL,
  `LabelLanguage` varchar(20) DEFAULT NULL COMMENT 'Language used for labels during this batch: Tamil, Kannada, Telugu, or NULL if not applicable',
  PRIMARY KEY (`PackingID`),
  UNIQUE KEY `UQ_PK_Batch` (`OrderID`,`BatchNo`),
  CONSTRAINT `FK_PKExec_Order` FOREIGN KEY (`OrderID`) REFERENCES `pp_productionorder` (`OrderID`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pk_pmconsumption`
--

DROP TABLE IF EXISTS `pk_pmconsumption`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pk_pmconsumption` (
  `ConsumptionID` int NOT NULL AUTO_INCREMENT,
  `PMID` int NOT NULL,
  `QtyUsed` decimal(18,3) NOT NULL,
  `UsedAt` datetime NOT NULL,
  `SourceType` varchar(20) NOT NULL,
  `SourceID` int NOT NULL,
  `CreatedBy` int NOT NULL,
  PRIMARY KEY (`ConsumptionID`),
  KEY `FK_PMCon_PM` (`PMID`),
  CONSTRAINT `FK_PMCon_PM` FOREIGN KEY (`PMID`) REFERENCES `mm_packingmaterials` (`PMID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pk_primarypacking`
--

DROP TABLE IF EXISTS `pk_primarypacking`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pk_primarypacking` (
  `PackingID` int NOT NULL AUTO_INCREMENT,
  `FGStockID` int NOT NULL,
  `ProductID` int NOT NULL,
  `PMID` int NOT NULL,
  `QtyUsed` decimal(18,3) NOT NULL,
  `PackedAt` datetime NOT NULL,
  `CreatedBy` int NOT NULL,
  PRIMARY KEY (`PackingID`),
  KEY `FK_PK_Primary_FG` (`FGStockID`),
  KEY `FK_PK_Primary_PM` (`PMID`),
  CONSTRAINT `FK_PK_Primary_FG` FOREIGN KEY (`FGStockID`) REFERENCES `pk_fgstock` (`FGStockID`),
  CONSTRAINT `FK_PK_Primary_PM` FOREIGN KEY (`PMID`) REFERENCES `mm_packingmaterials` (`PMID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pk_productpmmaster`
--

DROP TABLE IF EXISTS `pk_productpmmaster`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pk_productpmmaster` (
  `MappingID` int NOT NULL AUTO_INCREMENT,
  `ProductID` int NOT NULL,
  `PMID` int NOT NULL,
  `QtyPerUnit` decimal(12,4) NOT NULL DEFAULT '1.0000',
  `ApplyLevel` varchar(20) NOT NULL DEFAULT 'UNIT' COMMENT 'UNIT = per individual piece, CONTAINER = per jar/bottle, CASE = per case',
  `Language` varchar(20) DEFAULT NULL COMMENT 'NULL = universal (always consumed), Tamil/Kannada/Telugu = language-specific PM',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedBy` int DEFAULT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` datetime DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`MappingID`),
  UNIQUE KEY `uq_product_pm_level_lang` (`ProductID`,`PMID`,`ApplyLevel`,`Language`),
  KEY `idx_product` (`ProductID`),
  KEY `idx_pm` (`PMID`)
) ENGINE=InnoDB AUTO_INCREMENT=77 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pk_secondarypacking`
--

DROP TABLE IF EXISTS `pk_secondarypacking`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pk_secondarypacking` (
  `SecPackID` int NOT NULL AUTO_INCREMENT,
  `ProductID` int NOT NULL,
  `PackingType` varchar(10) NOT NULL DEFAULT 'CASE' COMMENT 'CASE = regular case packing, ONLINE = online order packing',
  `OnlineOrderID` varchar(50) DEFAULT NULL COMMENT 'External order ID for online orders',
  `CustomerName` varchar(200) DEFAULT NULL COMMENT 'Customer name for online orders',
  `QtyCartons` decimal(18,3) NOT NULL,
  `UnitsPerCarton` int NOT NULL DEFAULT '1',
  `TotalUnits` decimal(18,3) NOT NULL,
  `PMID` int DEFAULT NULL,
  `CartonsUsed` decimal(18,3) DEFAULT NULL,
  `PackedAt` datetime NOT NULL,
  `Remarks` varchar(300) DEFAULT NULL,
  `CreatedBy` int NOT NULL,
  PRIMARY KEY (`SecPackID`),
  KEY `FK_PK_Sec_Product` (`ProductID`),
  CONSTRAINT `FK_PK_Sec_Product` FOREIGN KEY (`ProductID`) REFERENCES `pp_products` (`ProductID`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pk_shipment`
--

DROP TABLE IF EXISTS `pk_shipment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pk_shipment` (
  `ShipmentID` int NOT NULL AUTO_INCREMENT,
  `ShipmentCode` varchar(30) NOT NULL,
  `POID` int NOT NULL,
  `CustomerID` int NOT NULL,
  `ShipDate` date NOT NULL,
  `VehicleNo` varchar(20) DEFAULT NULL,
  `DriverName` varchar(100) DEFAULT NULL,
  `Remarks` varchar(300) DEFAULT NULL,
  `Status` varchar(20) NOT NULL DEFAULT 'Dispatched',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `CreatedBy` int NOT NULL,
  PRIMARY KEY (`ShipmentID`),
  UNIQUE KEY `ShipmentCode` (`ShipmentCode`),
  KEY `FK_Ship_PO` (`POID`),
  KEY `FK_Ship_Customer` (`CustomerID`),
  CONSTRAINT `FK_Ship_Customer` FOREIGN KEY (`CustomerID`) REFERENCES `pk_customers` (`CustomerID`),
  CONSTRAINT `FK_Ship_PO` FOREIGN KEY (`POID`) REFERENCES `pk_customerpo` (`POID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pk_shipmentline`
--

DROP TABLE IF EXISTS `pk_shipmentline`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pk_shipmentline` (
  `ShipLineID` int NOT NULL AUTO_INCREMENT,
  `ShipmentID` int NOT NULL,
  `ProductID` int NOT NULL,
  `QtyShipped` decimal(18,3) NOT NULL,
  PRIMARY KEY (`ShipLineID`),
  KEY `FK_ShipLine_Ship` (`ShipmentID`),
  KEY `FK_ShipLine_Product` (`ProductID`),
  CONSTRAINT `FK_ShipLine_Product` FOREIGN KEY (`ProductID`) REFERENCES `pp_products` (`ProductID`),
  CONSTRAINT `FK_ShipLine_Ship` FOREIGN KEY (`ShipmentID`) REFERENCES `pk_shipment` (`ShipmentID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pp_batchexecution`
--

DROP TABLE IF EXISTS `pp_batchexecution`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pp_batchexecution` (
  `ExecutionID` int NOT NULL AUTO_INCREMENT,
  `OrderID` int NOT NULL,
  `BatchNo` int NOT NULL,
  `StartTime` datetime NOT NULL,
  `EndTime` datetime DEFAULT NULL,
  `ActualOutput` decimal(12,3) DEFAULT NULL,
  `Remarks` varchar(300) DEFAULT NULL,
  `Status` varchar(20) NOT NULL DEFAULT 'InProgress',
  `CreatedBy` int NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ExecutionID`),
  UNIQUE KEY `uq_batch` (`OrderID`,`BatchNo`),
  KEY `FK_BE_User` (`CreatedBy`),
  CONSTRAINT `FK_BE_Order` FOREIGN KEY (`OrderID`) REFERENCES `pp_productionorder` (`OrderID`),
  CONSTRAINT `FK_BE_User` FOREIGN KEY (`CreatedBy`) REFERENCES `users` (`UserID`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pp_batchparams`
--

DROP TABLE IF EXISTS `pp_batchparams`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pp_batchparams` (
  `BatchParamID` int NOT NULL AUTO_INCREMENT,
  `ExecutionID` int NOT NULL,
  `ParamID` int NOT NULL,
  `Value` decimal(18,4) DEFAULT NULL,
  PRIMARY KEY (`BatchParamID`),
  KEY `FK_BPParam_Exec` (`ExecutionID`),
  KEY `FK_BPParam_Param` (`ParamID`),
  CONSTRAINT `FK_BPParam_Exec` FOREIGN KEY (`ExecutionID`) REFERENCES `pp_batchexecution` (`ExecutionID`),
  CONSTRAINT `FK_BPParam_Param` FOREIGN KEY (`ParamID`) REFERENCES `pp_productparams` (`ParamID`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pp_bom`
--

DROP TABLE IF EXISTS `pp_bom`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pp_bom` (
  `BOMID` int NOT NULL AUTO_INCREMENT,
  `ProductID` int NOT NULL,
  `MaterialType` varchar(5) NOT NULL,
  `MaterialID` int NOT NULL,
  `Quantity` decimal(12,3) NOT NULL,
  `UOMID` int NOT NULL,
  PRIMARY KEY (`BOMID`),
  KEY `FK_BOM_Product` (`ProductID`),
  KEY `FK_BOM_UOM` (`UOMID`),
  CONSTRAINT `FK_BOM_Product` FOREIGN KEY (`ProductID`) REFERENCES `pp_products` (`ProductID`),
  CONSTRAINT `FK_BOM_UOM` FOREIGN KEY (`UOMID`) REFERENCES `mm_uom` (`UOMID`)
) ENGINE=InnoDB AUTO_INCREMENT=166 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pp_dailyplan`
--

DROP TABLE IF EXISTS `pp_dailyplan`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pp_dailyplan` (
  `PlanID` int NOT NULL AUTO_INCREMENT,
  `PlanDate` date NOT NULL,
  `Status` varchar(20) NOT NULL DEFAULT 'Draft',
  `CreatedBy` int NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`PlanID`),
  UNIQUE KEY `PlanDate` (`PlanDate`),
  KEY `FK_DPlan_User` (`CreatedBy`),
  CONSTRAINT `FK_DPlan_User` FOREIGN KEY (`CreatedBy`) REFERENCES `users` (`UserID`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pp_dailyplanrow`
--

DROP TABLE IF EXISTS `pp_dailyplanrow`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pp_dailyplanrow` (
  `RowID` int NOT NULL AUTO_INCREMENT,
  `PlanID` int NOT NULL,
  `Shift` tinyint NOT NULL,
  `ProductID` int NOT NULL,
  `Batches` decimal(8,2) NOT NULL DEFAULT '1.00',
  `SortOrder` int NOT NULL DEFAULT '0',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`RowID`),
  KEY `FK_DPlanRow_Plan` (`PlanID`),
  KEY `FK_DPlanRow_Product` (`ProductID`),
  CONSTRAINT `FK_DPlanRow_Plan` FOREIGN KEY (`PlanID`) REFERENCES `pp_dailyplan` (`PlanID`),
  CONSTRAINT `FK_DPlanRow_Product` FOREIGN KEY (`ProductID`) REFERENCES `pp_products` (`ProductID`)
) ENGINE=InnoDB AUTO_INCREMENT=19 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pp_fgpackingoptions`
--

DROP TABLE IF EXISTS `pp_fgpackingoptions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pp_fgpackingoptions` (
  `OptionID` int NOT NULL AUTO_INCREMENT,
  `ProductID` int NOT NULL,
  `PackForm` varchar(10) NOT NULL COMMENT 'JAR, BOX, CASE, PCS',
  `UnitsPerPack` int NOT NULL DEFAULT '1' COMMENT 'Number of units/containers in this pack form',
  `Description` varchar(100) DEFAULT NULL COMMENT 'e.g. JAR of 50, CASE of 12 JARs',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`OptionID`),
  UNIQUE KEY `uk_product_form_units` (`ProductID`,`PackForm`,`UnitsPerPack`),
  KEY `idx_productid` (`ProductID`),
  CONSTRAINT `FK_FGPack_Product` FOREIGN KEY (`ProductID`) REFERENCES `pp_products` (`ProductID`)
) ENGINE=InnoDB AUTO_INCREMENT=98 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pp_preprocesslog`
--

DROP TABLE IF EXISTS `pp_preprocesslog`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pp_preprocesslog` (
  `LogID` int NOT NULL AUTO_INCREMENT,
  `ProductID` int NOT NULL,
  `Stage` int NOT NULL,
  `Qty` decimal(18,6) NOT NULL,
  `Remarks` varchar(300) DEFAULT NULL,
  `CreatedAt` datetime NOT NULL,
  `CreatedBy` int NOT NULL,
  PRIMARY KEY (`LogID`),
  KEY `idx_product_stage` (`ProductID`,`Stage`),
  CONSTRAINT `FK_PPL_Product` FOREIGN KEY (`ProductID`) REFERENCES `pp_products` (`ProductID`)
) ENGINE=InnoDB AUTO_INCREMENT=41 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pp_preprocessstages`
--

DROP TABLE IF EXISTS `pp_preprocessstages`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pp_preprocessstages` (
  `ProductID` int NOT NULL,
  `Stage1Label` varchar(100) NOT NULL DEFAULT 'Dispensed for Processing',
  `Stage2Label` varchar(100) NOT NULL DEFAULT 'Processed Qty',
  `Stage3Label` varchar(100) NOT NULL DEFAULT 'Final Sorted Qty',
  `Stage4Label` varchar(100) DEFAULT NULL,
  `InputRMName` varchar(150) NOT NULL,
  PRIMARY KEY (`ProductID`),
  CONSTRAINT `FK_PPS_Product` FOREIGN KEY (`ProductID`) REFERENCES `pp_products` (`ProductID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pp_productionlines`
--

DROP TABLE IF EXISTS `pp_productionlines`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pp_productionlines` (
  `LineID` int NOT NULL AUTO_INCREMENT,
  `LineName` varchar(100) NOT NULL,
  `LineCode` varchar(30) NOT NULL,
  `SortOrder` int NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`LineID`),
  UNIQUE KEY `LineCode` (`LineCode`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pp_productionorder`
--

DROP TABLE IF EXISTS `pp_productionorder`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pp_productionorder` (
  `OrderID` int NOT NULL AUTO_INCREMENT,
  `PlanID` int NOT NULL,
  `PlanRowID` int NOT NULL,
  `Shift` tinyint NOT NULL,
  `ProductionLineID` int DEFAULT NULL,
  `ProductID` int NOT NULL,
  `OrderedBatches` decimal(8,2) NOT NULL,
  `RevisedBatches` decimal(8,2) DEFAULT NULL,
  `Status` varchar(20) NOT NULL DEFAULT 'Pending',
  `ExecutionPriority` int DEFAULT NULL,
  `OrderDate` date NOT NULL,
  `InitiatedAt` datetime DEFAULT NULL,
  `CompletedAt` datetime DEFAULT NULL,
  `CreatedBy` int NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`OrderID`),
  UNIQUE KEY `uq_order_planrow` (`PlanRowID`),
  KEY `FK_PO_Plan` (`PlanID`),
  KEY `FK_PO_Product` (`ProductID`),
  KEY `FK_PO_User` (`CreatedBy`),
  CONSTRAINT `FK_PO_Plan` FOREIGN KEY (`PlanID`) REFERENCES `pp_dailyplan` (`PlanID`),
  CONSTRAINT `FK_PO_PlanRow` FOREIGN KEY (`PlanRowID`) REFERENCES `pp_dailyplanrow` (`RowID`) ON DELETE CASCADE,
  CONSTRAINT `FK_PO_Product` FOREIGN KEY (`ProductID`) REFERENCES `pp_products` (`ProductID`),
  CONSTRAINT `FK_PO_User` FOREIGN KEY (`CreatedBy`) REFERENCES `users` (`UserID`)
) ENGINE=InnoDB AUTO_INCREMENT=107 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pp_productionplan`
--

DROP TABLE IF EXISTS `pp_productionplan`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pp_productionplan` (
  `PlanID` int NOT NULL AUTO_INCREMENT,
  `PlanNo` varchar(20) NOT NULL,
  `PlanDate` date NOT NULL,
  `PlanMonth` tinyint NOT NULL,
  `PlanYear` smallint NOT NULL,
  `ProductID` int NOT NULL,
  `PlannedQty` decimal(12,3) NOT NULL,
  `Status` varchar(20) NOT NULL DEFAULT 'Planned',
  `Remarks` varchar(300) DEFAULT NULL,
  `CreatedBy` int NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`PlanID`),
  UNIQUE KEY `PlanNo` (`PlanNo`),
  KEY `FK_Plan_Product` (`ProductID`),
  KEY `FK_Plan_User` (`CreatedBy`),
  CONSTRAINT `FK_Plan_Product` FOREIGN KEY (`ProductID`) REFERENCES `pp_products` (`ProductID`),
  CONSTRAINT `FK_Plan_User` FOREIGN KEY (`CreatedBy`) REFERENCES `users` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pp_productparams`
--

DROP TABLE IF EXISTS `pp_productparams`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pp_productparams` (
  `ParamID` int NOT NULL AUTO_INCREMENT,
  `ProductID` int NOT NULL,
  `ParamOrder` int NOT NULL DEFAULT '1',
  `ParamType` varchar(20) NOT NULL,
  `ParamLabel` varchar(100) NOT NULL,
  `ParamOptions` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`ParamID`),
  KEY `FK_PPParam_Product` (`ProductID`),
  CONSTRAINT `FK_PPParam_Product` FOREIGN KEY (`ProductID`) REFERENCES `pp_products` (`ProductID`)
) ENGINE=InnoDB AUTO_INCREMENT=46 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pp_products`
--

DROP TABLE IF EXISTS `pp_products`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pp_products` (
  `ProductID` int NOT NULL AUTO_INCREMENT,
  `ProductCode` varchar(20) NOT NULL,
  `ProductName` varchar(200) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `ProductType` varchar(20) NOT NULL DEFAULT 'Core',
  `ProductionLineID` int DEFAULT NULL,
  `UnitWeightGrams` decimal(10,2) DEFAULT NULL,
  `HSNCode` varchar(20) DEFAULT NULL,
  `GSTRate` decimal(5,2) DEFAULT NULL,
  `ProdUOMID` int NOT NULL DEFAULT '1',
  `OutputUOMID` int NOT NULL DEFAULT '1',
  `BatchSize` decimal(12,3) NOT NULL DEFAULT '1.000',
  `ImagePath` varchar(300) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ContainerType` varchar(20) DEFAULT NULL,
  `ContainersPerCase` int DEFAULT '12',
  `UnitsPerContainer` varchar(200) DEFAULT NULL,
  `HasLanguageLabels` tinyint(1) NOT NULL DEFAULT '0' COMMENT 'Set to 1 if this product has language-specific labels (Tamil, Kannada, Telugu)',
  `IsPriceCalcProduct` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`ProductID`),
  UNIQUE KEY `ProductCode` (`ProductCode`),
  KEY `FK_PP_Product_ProdUOM` (`ProdUOMID`),
  KEY `FK_PP_Product_OutputUOM` (`OutputUOMID`),
  CONSTRAINT `FK_PP_Product_OutputUOM` FOREIGN KEY (`OutputUOMID`) REFERENCES `mm_uom` (`UOMID`),
  CONSTRAINT `FK_PP_Product_ProdUOM` FOREIGN KEY (`ProdUOMID`) REFERENCES `mm_uom` (`UOMID`)
) ENGINE=InnoDB AUTO_INCREMENT=46 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pp_remarkoptions`
--

DROP TABLE IF EXISTS `pp_remarkoptions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pp_remarkoptions` (
  `OptionID` int NOT NULL AUTO_INCREMENT,
  `OptionText` varchar(200) NOT NULL,
  `SortOrder` int NOT NULL DEFAULT '1',
  PRIMARY KEY (`OptionID`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `products`
--

DROP TABLE IF EXISTS `products`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `products` (
  `ProductID` int NOT NULL AUTO_INCREMENT,
  `ProductName` varchar(200) NOT NULL,
  `ProductCode` varchar(50) DEFAULT NULL,
  `MRP` decimal(10,2) NOT NULL DEFAULT '0.00',
  `HSNCode` varchar(20) DEFAULT NULL,
  `GSTRate` decimal(5,2) NOT NULL DEFAULT '0.00',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ProductID`),
  UNIQUE KEY `uq_ProductName` (`ProductName`)
) ENGINE=InnoDB AUTO_INCREMENT=24 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `receiptregister`
--

DROP TABLE IF EXISTS `receiptregister`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `receiptregister` (
  `ReceiptID` int NOT NULL AUTO_INCREMENT,
  `ReceiptDate` date NOT NULL,
  `Particulars` varchar(255) NOT NULL,
  `DistributorID` int DEFAULT NULL,
  `VchType` varchar(50) NOT NULL DEFAULT 'Receipt',
  `VchNo` int NOT NULL,
  `CreditAmount` decimal(12,2) NOT NULL DEFAULT '0.00',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ReceiptID`),
  KEY `IX_Receipt_DistributorID` (`DistributorID`),
  KEY `IX_Receipt_Date` (`ReceiptDate`),
  CONSTRAINT `FK_Receipt_Distributor` FOREIGN KEY (`DistributorID`) REFERENCES `distributors` (`DistributorID`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sa_areas`
--

DROP TABLE IF EXISTS `sa_areas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sa_areas` (
  `AreaID` int NOT NULL AUTO_INCREMENT,
  `RegionID` int NOT NULL,
  `AreaName` varchar(100) NOT NULL,
  `AreaCode` varchar(20) NOT NULL,
  `SortOrder` int NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`AreaID`),
  UNIQUE KEY `AreaCode` (`AreaCode`),
  KEY `idx_region` (`RegionID`)
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sa_channels`
--

DROP TABLE IF EXISTS `sa_channels`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sa_channels` (
  `ChannelID` int NOT NULL AUTO_INCREMENT,
  `ChannelName` varchar(100) NOT NULL,
  `SortOrder` int NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`ChannelID`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sa_designations`
--

DROP TABLE IF EXISTS `sa_designations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sa_designations` (
  `DesignationID` int NOT NULL AUTO_INCREMENT,
  `DesignCode` varchar(20) NOT NULL,
  `DesignName` varchar(100) NOT NULL,
  `HierarchyLevel` int NOT NULL COMMENT '1=MD, 2=ZSM, 3=RSM, 4=ASM/ASE, 5=Sr SO',
  `SortOrder` int NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`DesignationID`),
  UNIQUE KEY `DesignCode` (`DesignCode`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sa_orgpositions`
--

DROP TABLE IF EXISTS `sa_orgpositions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sa_orgpositions` (
  `PositionID` int NOT NULL AUTO_INCREMENT,
  `UserID` int DEFAULT NULL COMMENT 'Linked user account (NULL = vacant)',
  `DesignationID` int NOT NULL,
  `EmployeeID` varchar(30) DEFAULT NULL COMMENT 'Company employee ID',
  `EmployeeName` varchar(150) DEFAULT NULL COMMENT 'Display name (can differ from Users.FullName)',
  `ZoneID` int DEFAULT NULL COMMENT 'Applicable for ZSM and below',
  `RegionID` int DEFAULT NULL COMMENT 'Applicable for RSM and below',
  `ReportsToID` int DEFAULT NULL COMMENT 'PositionID of the reporting manager',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`PositionID`),
  KEY `idx_user` (`UserID`),
  KEY `idx_desig` (`DesignationID`),
  KEY `idx_zone` (`ZoneID`),
  KEY `idx_region` (`RegionID`),
  KEY `idx_reports` (`ReportsToID`)
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sa_projectionlines`
--

DROP TABLE IF EXISTS `sa_projectionlines`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sa_projectionlines` (
  `LineID` int NOT NULL AUTO_INCREMENT,
  `ProjectionID` int NOT NULL,
  `ProductID` int NOT NULL,
  `Quantity` int NOT NULL DEFAULT '0' COMMENT 'Number of units (jars/boxes)',
  `UOMID` int DEFAULT NULL,
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`LineID`),
  UNIQUE KEY `uk_proj_product` (`ProjectionID`,`ProductID`),
  KEY `idx_proj` (`ProjectionID`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sa_projections`
--

DROP TABLE IF EXISTS `sa_projections`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sa_projections` (
  `ProjectionID` int NOT NULL AUTO_INCREMENT,
  `ProjectionMonth` int NOT NULL COMMENT '1-12',
  `ProjectionYear` int NOT NULL,
  `StateID` int NOT NULL,
  `ChannelID` int NOT NULL,
  `ZoneID` int DEFAULT NULL,
  `RegionID` int DEFAULT NULL,
  `PositionID` int DEFAULT NULL,
  `Status` varchar(20) NOT NULL DEFAULT 'Draft' COMMENT 'Draft, Confirmed',
  `CreatedBy` int DEFAULT NULL,
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ProjectionID`),
  UNIQUE KEY `uk_month_channel_position` (`ProjectionMonth`,`ProjectionYear`,`ChannelID`,`PositionID`),
  KEY `idx_month_year` (`ProjectionMonth`,`ProjectionYear`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sa_regions`
--

DROP TABLE IF EXISTS `sa_regions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sa_regions` (
  `RegionID` int NOT NULL AUTO_INCREMENT,
  `ZoneID` int NOT NULL,
  `RegionName` varchar(100) NOT NULL,
  `RegionCode` varchar(20) NOT NULL,
  `SortOrder` int NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`RegionID`),
  UNIQUE KEY `RegionCode` (`RegionCode`),
  KEY `idx_zone` (`ZoneID`)
) ENGINE=InnoDB AUTO_INCREMENT=27 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sa_shipmentlines`
--

DROP TABLE IF EXISTS `sa_shipmentlines`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sa_shipmentlines` (
  `LineID` int NOT NULL AUTO_INCREMENT,
  `ShipmentID` int NOT NULL,
  `ProductID` int NOT NULL,
  `ProjectedQty` int NOT NULL DEFAULT '0' COMMENT 'From projection',
  `ShippedQty` int NOT NULL DEFAULT '0' COMMENT 'Actual shipped (user can modify)',
  PRIMARY KEY (`LineID`),
  KEY `idx_ship` (`ShipmentID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sa_shipments`
--

DROP TABLE IF EXISTS `sa_shipments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sa_shipments` (
  `ShipmentID` int NOT NULL AUTO_INCREMENT,
  `ProjectionID` int DEFAULT NULL COMMENT 'Linked projection (can be NULL for ad-hoc)',
  `CustomerID` int DEFAULT NULL,
  `ShipmentDate` date NOT NULL,
  `StateID` int NOT NULL,
  `ChannelID` int NOT NULL,
  `ZoneID` int DEFAULT NULL,
  `RegionID` int DEFAULT NULL,
  `PositionID` int DEFAULT NULL,
  `TransportModeID` int DEFAULT NULL,
  `VehicleNo` varchar(50) DEFAULT NULL,
  `Status` varchar(20) NOT NULL DEFAULT 'Draft' COMMENT 'Draft, Shipped, Delivered',
  `Remarks` varchar(500) DEFAULT NULL,
  `CreatedBy` int DEFAULT NULL,
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ShipmentID`),
  KEY `idx_date` (`ShipmentDate`),
  KEY `idx_proj` (`ProjectionID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sa_transportmodes`
--

DROP TABLE IF EXISTS `sa_transportmodes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sa_transportmodes` (
  `ModeID` int NOT NULL AUTO_INCREMENT,
  `ModeName` varchar(100) NOT NULL,
  `SortOrder` int NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`ModeID`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sa_zones`
--

DROP TABLE IF EXISTS `sa_zones`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sa_zones` (
  `ZoneID` int NOT NULL AUTO_INCREMENT,
  `ZoneName` varchar(100) NOT NULL,
  `ZoneCode` varchar(20) NOT NULL,
  `SortOrder` int NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ZoneID`),
  UNIQUE KEY `ZoneCode` (`ZoneCode`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `salesorders`
--

DROP TABLE IF EXISTS `salesorders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `salesorders` (
  `OrderID` int NOT NULL AUTO_INCREMENT,
  `DistributorID` int DEFAULT NULL,
  `DistributorName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `OrderDate` date NOT NULL,
  `InvoiceNo` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductName` varchar(255) DEFAULT NULL,
  `NoOfUnits` decimal(10,2) NOT NULL DEFAULT '0.00',
  `TotalValue` decimal(12,2) NOT NULL DEFAULT '0.00',
  `CreatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `CreditAmount` decimal(12,2) DEFAULT NULL,
  `CreditDate` date DEFAULT NULL,
  `ReceiptVchNo` int DEFAULT NULL,
  PRIMARY KEY (`OrderID`),
  UNIQUE KEY `UX_SalesOrders_Invoice_Product` (`InvoiceNo`,`ProductName`(100)),
  KEY `IX_SalesOrders_DistributorID` (`DistributorID`),
  KEY `IX_SalesOrders_OrderDate` (`OrderDate`),
  CONSTRAINT `FK_SalesOrders_Distributors` FOREIGN KEY (`DistributorID`) REFERENCES `distributors` (`DistributorID`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `states`
--

DROP TABLE IF EXISTS `states`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `states` (
  `StateID` int NOT NULL,
  `StateName` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`StateID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `stockpositions`
--

DROP TABLE IF EXISTS `stockpositions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `stockpositions` (
  `StockPositionID` int NOT NULL AUTO_INCREMENT,
  `StateID` int NOT NULL,
  `CityID` int NOT NULL,
  `DistributorID` int NOT NULL,
  `CurrentStock` int NOT NULL,
  `EntryDate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`StockPositionID`),
  KEY `FK_Stock_States` (`StateID`),
  KEY `FK_Stock_Cities` (`CityID`),
  KEY `FK_Stock_Distributors` (`DistributorID`),
  CONSTRAINT `FK_Stock_Cities` FOREIGN KEY (`CityID`) REFERENCES `cities` (`CityID`),
  CONSTRAINT `FK_Stock_States` FOREIGN KEY (`StateID`) REFERENCES `states` (`StateID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `UserID` int NOT NULL AUTO_INCREMENT,
  `FullName` varchar(100) NOT NULL,
  `Username` varchar(50) NOT NULL,
  `PasswordHash` varchar(64) NOT NULL,
  `Role` varchar(30) NOT NULL DEFAULT 'FieldUser',
  `StateID` int DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `MustChangePwd` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `LastLogin` datetime DEFAULT NULL,
  `ReportingManagerID` int DEFAULT NULL,
  PRIMARY KEY (`UserID`),
  UNIQUE KEY `Username` (`Username`),
  KEY `FK_Users_States` (`StateID`),
  KEY `fk_reporting_manager` (`ReportingManagerID`),
  CONSTRAINT `fk_reporting_manager` FOREIGN KEY (`ReportingManagerID`) REFERENCES `users` (`UserID`) ON DELETE SET NULL,
  CONSTRAINT `FK_Users_States` FOREIGN KEY (`StateID`) REFERENCES `states` (`StateID`)
) ENGINE=InnoDB AUTO_INCREMENT=90 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-04-07 22:01:28
