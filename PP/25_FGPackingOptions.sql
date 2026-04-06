-- ═══════════════════════════════════════════════════════════
-- FG Packing Options — Run on stockdb
-- ═══════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS PP_FGPackingOptions (
    OptionID    INT NOT NULL AUTO_INCREMENT,
    ProductID   INT NOT NULL,
    PackForm    VARCHAR(10) NOT NULL COMMENT 'JAR, BOX, CASE, PCS',
    UnitsPerPack INT NOT NULL DEFAULT 1 COMMENT 'Number of units/containers in this pack form',
    Description VARCHAR(100) DEFAULT NULL COMMENT 'e.g. JAR of 50, CASE of 12 JARs',
    IsActive    TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (OptionID),
    UNIQUE KEY uk_product_form_units (ProductID, PackForm, UnitsPerPack),
    KEY idx_productid (ProductID),
    CONSTRAINT FK_FGPack_Product FOREIGN KEY (ProductID) REFERENCES PP_Products(ProductID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

SELECT 'PP_FGPackingOptions table created.' AS Status;
