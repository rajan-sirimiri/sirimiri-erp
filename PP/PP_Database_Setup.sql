-- ============================================================
-- Production & Planning Module — Database Setup
-- Run ONCE on StockDB (local and VPS)
-- Pre-requisites: MM_UOM, MM_RawMaterials, MM_PackingMaterials,
--                 MM_Consumables, Users tables must exist
-- ============================================================

USE StockDB;

-- ── 1. FINISHED GOODS / PRODUCTS MASTER ──────────────────
CREATE TABLE IF NOT EXISTS PP_Products (
    ProductID    INT            NOT NULL AUTO_INCREMENT PRIMARY KEY,
    ProductCode  VARCHAR(20)    NOT NULL UNIQUE,
    ProductName  VARCHAR(200)   NOT NULL,
    Description  VARCHAR(500)   NULL,
    HSNCode      VARCHAR(20)    NULL,
    GSTRate      DECIMAL(5,2)   NULL,
    UOMID        INT            NOT NULL,
    BatchSize    DECIMAL(12,3)  NOT NULL DEFAULT 1,
    IsActive     TINYINT(1)     NOT NULL DEFAULT 1,
    CreatedAt    DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_PP_Product_UOM FOREIGN KEY (UOMID) REFERENCES MM_UOM(UOMID)
);

-- ── 2. BILL OF MATERIALS (BOM) ────────────────────────────
-- MaterialType: 'RM' = Raw Material, 'PM' = Packing Material, 'CN' = Consumable
CREATE TABLE IF NOT EXISTS PP_BOM (
    BOMID        INT            NOT NULL AUTO_INCREMENT PRIMARY KEY,
    ProductID    INT            NOT NULL,
    MaterialType VARCHAR(5)     NOT NULL,   -- RM / PM / CN
    MaterialID   INT            NOT NULL,
    Quantity     DECIMAL(12,3)  NOT NULL,
    UOMID        INT            NOT NULL,
    CONSTRAINT FK_BOM_Product FOREIGN KEY (ProductID) REFERENCES PP_Products(ProductID),
    CONSTRAINT FK_BOM_UOM     FOREIGN KEY (UOMID)     REFERENCES MM_UOM(UOMID)
);

-- ── 3. PRODUCTION PLAN ────────────────────────────────────
-- Status: Planned → Released → Completed → Cancelled
CREATE TABLE IF NOT EXISTS PP_ProductionPlan (
    PlanID      INT            NOT NULL AUTO_INCREMENT PRIMARY KEY,
    PlanNo      VARCHAR(20)    NOT NULL UNIQUE,
    PlanDate    DATE           NOT NULL,
    PlanMonth   TINYINT        NOT NULL,
    PlanYear    SMALLINT       NOT NULL,
    ProductID   INT            NOT NULL,
    PlannedQty  DECIMAL(12,3)  NOT NULL,
    Status      VARCHAR(20)    NOT NULL DEFAULT 'Planned',
    Remarks     VARCHAR(300)   NULL,
    CreatedBy   INT            NOT NULL,
    CreatedAt   DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Plan_Product FOREIGN KEY (ProductID)  REFERENCES PP_Products(ProductID),
    CONSTRAINT FK_Plan_User    FOREIGN KEY (CreatedBy)  REFERENCES Users(UserID)
);

-- ── 4. PRODUCTION ORDER ───────────────────────────────────
-- Status: Open → In Progress → Completed → Cancelled
CREATE TABLE IF NOT EXISTS PP_ProductionOrder (
    OrderID     INT            NOT NULL AUTO_INCREMENT PRIMARY KEY,
    OrderNo     VARCHAR(20)    NOT NULL UNIQUE,
    OrderDate   DATE           NOT NULL,
    ProductID   INT            NOT NULL,
    OrderQty    DECIMAL(12,3)  NOT NULL,
    TargetDate  DATE           NOT NULL,
    Status      VARCHAR(20)    NOT NULL DEFAULT 'Open',
    Remarks     VARCHAR(300)   NULL,
    CreatedBy   INT            NOT NULL,
    CreatedAt   DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Order_Product FOREIGN KEY (ProductID) REFERENCES PP_Products(ProductID),
    CONSTRAINT FK_Order_User    FOREIGN KEY (CreatedBy) REFERENCES Users(UserID)
);

-- ── VERIFY ────────────────────────────────────────────────
SELECT 'PP_Products'        AS TableName, COUNT(*) AS RowCount FROM PP_Products
UNION ALL
SELECT 'PP_BOM',             COUNT(*) FROM PP_BOM
UNION ALL
SELECT 'PP_ProductionPlan',  COUNT(*) FROM PP_ProductionPlan
UNION ALL
SELECT 'PP_ProductionOrder', COUNT(*) FROM PP_ProductionOrder;
