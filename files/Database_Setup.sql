-- ============================================================
-- DATABASE SETUP SCRIPT
-- Stock Position Entry Application
-- ============================================================

USE master;
GO

-- Create the database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'StockDB')
BEGIN
    CREATE DATABASE StockDB;
END
GO

USE StockDB;
GO

-- ============================================================
-- TABLE: States
-- ============================================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'States')
BEGIN
    CREATE TABLE States (
        StateID   INT          IDENTITY(1,1) PRIMARY KEY,
        StateName NVARCHAR(100) NOT NULL
    );
END
GO

-- ============================================================
-- TABLE: Cities
-- ============================================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Cities')
BEGIN
    CREATE TABLE Cities (
        CityID   INT           IDENTITY(1,1) PRIMARY KEY,
        CityName NVARCHAR(100) NOT NULL,
        StateID  INT           NOT NULL,
        CONSTRAINT FK_Cities_States FOREIGN KEY (StateID) REFERENCES States(StateID)
    );
END
GO

-- ============================================================
-- TABLE: Distributors
-- ============================================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Distributors')
BEGIN
    CREATE TABLE Distributors (
        DistributorID   INT           IDENTITY(1,1) PRIMARY KEY,
        DistributorName NVARCHAR(200) NOT NULL,
        CityID          INT           NOT NULL,
        CONSTRAINT FK_Distributors_Cities FOREIGN KEY (CityID) REFERENCES Cities(CityID)
    );
END
GO

-- ============================================================
-- TABLE: StockPositions
-- ============================================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'StockPositions')
BEGIN
    CREATE TABLE StockPositions (
        StockPositionID INT      IDENTITY(1,1) PRIMARY KEY,
        StateID         INT      NOT NULL,
        CityID          INT      NOT NULL,
        DistributorID   INT      NOT NULL,
        CurrentStock    INT      NOT NULL,
        EntryDate       DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Stock_States      FOREIGN KEY (StateID)       REFERENCES States(StateID),
        CONSTRAINT FK_Stock_Cities      FOREIGN KEY (CityID)        REFERENCES Cities(CityID),
        CONSTRAINT FK_Stock_Distributors FOREIGN KEY (DistributorID) REFERENCES Distributors(DistributorID)
    );
END
GO

-- ============================================================
-- SEED DATA: States
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM States)
BEGIN
    INSERT INTO States (StateName) VALUES
        ('Tamil Nadu'),
        ('Karnataka'),
        ('Maharashtra'),
        ('Delhi'),
        ('Gujarat');
END
GO

-- ============================================================
-- SEED DATA: Cities
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Cities)
BEGIN
    -- Tamil Nadu
    INSERT INTO Cities (CityName, StateID) VALUES ('Chennai',    1);
    INSERT INTO Cities (CityName, StateID) VALUES ('Coimbatore', 1);
    INSERT INTO Cities (CityName, StateID) VALUES ('Madurai',    1);

    -- Karnataka
    INSERT INTO Cities (CityName, StateID) VALUES ('Bengaluru',  2);
    INSERT INTO Cities (CityName, StateID) VALUES ('Mysuru',     2);

    -- Maharashtra
    INSERT INTO Cities (CityName, StateID) VALUES ('Mumbai',     3);
    INSERT INTO Cities (CityName, StateID) VALUES ('Pune',       3);
    INSERT INTO Cities (CityName, StateID) VALUES ('Nagpur',     3);

    -- Delhi
    INSERT INTO Cities (CityName, StateID) VALUES ('New Delhi',  4);
    INSERT INTO Cities (CityName, StateID) VALUES ('Dwarka',     4);

    -- Gujarat
    INSERT INTO Cities (CityName, StateID) VALUES ('Ahmedabad',  5);
    INSERT INTO Cities (CityName, StateID) VALUES ('Surat',      5);
END
GO

-- ============================================================
-- SEED DATA: Distributors
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Distributors)
BEGIN
    -- Chennai
    INSERT INTO Distributors (DistributorName, CityID) VALUES ('Alpha Distributors',   1);
    INSERT INTO Distributors (DistributorName, CityID) VALUES ('Sri Venkat Traders',   1);
    -- Coimbatore
    INSERT INTO Distributors (DistributorName, CityID) VALUES ('Kovai Agencies',       2);
    INSERT INTO Distributors (DistributorName, CityID) VALUES ('CBE Logistics',        2);
    -- Madurai
    INSERT INTO Distributors (DistributorName, CityID) VALUES ('Madurai Wholesalers',  3);
    -- Bengaluru
    INSERT INTO Distributors (DistributorName, CityID) VALUES ('BLR Supply Co.',       4);
    INSERT INTO Distributors (DistributorName, CityID) VALUES ('Garden City Traders',  4);
    -- Mysuru
    INSERT INTO Distributors (DistributorName, CityID) VALUES ('Royal Distributors',   5);
    -- Mumbai
    INSERT INTO Distributors (DistributorName, CityID) VALUES ('Mumbai Central Dist.', 6);
    INSERT INTO Distributors (DistributorName, CityID) VALUES ('Harbour Traders',      6);
    -- Pune
    INSERT INTO Distributors (DistributorName, CityID) VALUES ('Pune Agencies',        7);
    -- Nagpur
    INSERT INTO Distributors (DistributorName, CityID) VALUES ('Orange City Dist.',    8);
    -- New Delhi
    INSERT INTO Distributors (DistributorName, CityID) VALUES ('Capital Distributors', 9);
    INSERT INTO Distributors (DistributorName, CityID) VALUES ('Delhi Traders Ltd.',   9);
    -- Dwarka
    INSERT INTO Distributors (DistributorName, CityID) VALUES ('Dwarka Agencies',      10);
    -- Ahmedabad
    INSERT INTO Distributors (DistributorName, CityID) VALUES ('Amdavad Distributors', 11);
    INSERT INTO Distributors (DistributorName, CityID) VALUES ('Sabarmati Traders',    11);
    -- Surat
    INSERT INTO Distributors (DistributorName, CityID) VALUES ('Diamond City Dist.',   12);
END
GO

-- ============================================================
-- STORED PROCEDURE: Get All States
-- ============================================================
IF OBJECT_ID('dbo.usp_GetStates', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_GetStates;
GO
CREATE PROCEDURE dbo.usp_GetStates
AS
BEGIN
    SET NOCOUNT ON;
    SELECT StateID, StateName FROM States ORDER BY StateName;
END
GO

-- ============================================================
-- STORED PROCEDURE: Get Cities by State
-- ============================================================
IF OBJECT_ID('dbo.usp_GetCitiesByState', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_GetCitiesByState;
GO
CREATE PROCEDURE dbo.usp_GetCitiesByState
    @StateID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CityID, CityName FROM Cities WHERE StateID = @StateID ORDER BY CityName;
END
GO

-- ============================================================
-- STORED PROCEDURE: Get Distributors by City
-- ============================================================
IF OBJECT_ID('dbo.usp_GetDistributorsByCity', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_GetDistributorsByCity;
GO
CREATE PROCEDURE dbo.usp_GetDistributorsByCity
    @CityID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DistributorID, DistributorName FROM Distributors WHERE CityID = @CityID ORDER BY DistributorName;
END
GO

-- ============================================================
-- STORED PROCEDURE: Save Stock Position
-- ============================================================
IF OBJECT_ID('dbo.usp_SaveStockPosition', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_SaveStockPosition;
GO
CREATE PROCEDURE dbo.usp_SaveStockPosition
    @StateID       INT,
    @CityID        INT,
    @DistributorID INT,
    @CurrentStock  INT,
    @NewID         INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO StockPositions (StateID, CityID, DistributorID, CurrentStock, EntryDate)
    VALUES (@StateID, @CityID, @DistributorID, @CurrentStock, GETDATE());
    SET @NewID = SCOPE_IDENTITY();
END
GO

PRINT 'Database setup complete.';

-- ============================================================
-- STORED PROCEDURE: Get Distributor Address
-- ============================================================
IF OBJECT_ID('dbo.usp_GetDistributorAddress', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_GetDistributorAddress;
GO
CREATE PROCEDURE dbo.usp_GetDistributorAddress
    @DistributorID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        d.DistributorName,
        d.FullAddress,
        d.PinCode,
        c.CityName,
        s.StateName
    FROM Distributors d
    JOIN Cities c ON c.CityID   = d.CityID
    JOIN States s ON s.StateID  = c.StateID
    WHERE d.DistributorID = @DistributorID;
END
GO
