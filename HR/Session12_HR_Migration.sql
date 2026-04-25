-- =====================================================================
-- Session 12: HR Module Foundation
-- Creates HR_Department and HR_Employee with KYC fields
-- Target: MySQL 5.7+ / 8.x  (Sirimiri ERP on port 3308)
-- =====================================================================

SET FOREIGN_KEY_CHECKS = 0;

-- ---------------------------------------------------------------------
-- HR_Department
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS HR_Department (
    DeptID        INT AUTO_INCREMENT PRIMARY KEY,
    DeptCode      VARCHAR(20)  NOT NULL,
    DeptName      VARCHAR(100) NOT NULL,
    IsActive      TINYINT(1)   NOT NULL DEFAULT 1,
    CreatedAt     DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedBy     VARCHAR(50),
    ModifiedAt    DATETIME     NULL,
    ModifiedBy    VARCHAR(50),
    UNIQUE KEY uk_dept_code (DeptCode)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------
-- HR_Employee
-- NOTE: Aadhaar/PAN are stored as VARCHAR (leading zeros, never arithmetic).
-- NOTE: Salary fields belong on the master for now; in future sessions
--       we will introduce HR_EmployeeSalaryHistory for dated payroll.
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS HR_Employee (
    EmployeeID      INT AUTO_INCREMENT PRIMARY KEY,
    EmployeeCode    VARCHAR(20)  NOT NULL,
    FullName        VARCHAR(150) NOT NULL,
    FatherName      VARCHAR(150),
    Gender          ENUM('M','F','O') NOT NULL DEFAULT 'M',
    DOB             DATE         NULL,
    DOJ             DATE         NOT NULL,
    DOL             DATE         NULL,

    DeptID          INT          NOT NULL,
    Designation     VARCHAR(100),
    EmploymentType  ENUM('Permanent','Contract','Trainee','Apprentice','Temporary') NOT NULL DEFAULT 'Permanent',

    -- Contact
    MobileNo        VARCHAR(15),
    AltMobileNo     VARCHAR(15),
    Email           VARCHAR(100),
    AddressLine     VARCHAR(500),
    City            VARCHAR(80),
    StateName       VARCHAR(80),
    Pincode         VARCHAR(10),

    -- KYC / Statutory
    AadhaarNo       VARCHAR(12),
    PANNo           VARCHAR(10),
    UANNo           VARCHAR(12),
    PFNo            VARCHAR(30),
    ESINo           VARCHAR(20),
    BankAccountNo   VARCHAR(30),
    BankName        VARCHAR(100),
    IFSCCode        VARCHAR(11),

    -- Salary master (no payroll logic yet)
    BasicSalary     DECIMAL(12,2) NOT NULL DEFAULT 0,
    HRA             DECIMAL(12,2) NOT NULL DEFAULT 0,
    ConveyanceAllow DECIMAL(12,2) NOT NULL DEFAULT 0,
    OtherAllow      DECIMAL(12,2) NOT NULL DEFAULT 0,
    GrossSalary     DECIMAL(12,2) NOT NULL DEFAULT 0,

    IsActive        TINYINT(1)   NOT NULL DEFAULT 1,
    CreatedAt       DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedBy       VARCHAR(50),
    ModifiedAt      DATETIME     NULL,
    ModifiedBy      VARCHAR(50),

    UNIQUE KEY uk_emp_code (EmployeeCode),
    KEY idx_emp_dept (DeptID),
    KEY idx_emp_active (IsActive),
    KEY idx_emp_name (FullName),
    CONSTRAINT fk_emp_dept FOREIGN KEY (DeptID) REFERENCES HR_Department(DeptID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------
-- Seed a default department so first imports have something to map to
-- ---------------------------------------------------------------------
INSERT IGNORE INTO HR_Department (DeptCode, DeptName, IsActive, CreatedBy)
VALUES ('GEN', 'General', 1, 'SYSTEM');

SET FOREIGN_KEY_CHECKS = 1;
