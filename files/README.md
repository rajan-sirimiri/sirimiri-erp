# Stock Position Entry — ASP.NET Web Forms (.NET Framework 4.8)

## Project Structure

```
StockApp/
│
├── StockApp.csproj          ← Visual Studio project file
├── Web.config               ← Connection string & app settings
├── Database_Setup.sql       ← Full SQL script (tables, seed data, SPs)
│
├── StockEntry.aspx          ← Web Form (UI)
├── StockEntry.aspx.cs       ← Code-behind (cascading logic + save)
│
└── DAL/
    └── DatabaseHelper.cs    ← Data Access Layer (all SQL calls)
```

---

## Quick Start

### Step 1 — Create the Database

1. Open **SQL Server Management Studio (SSMS)**.
2. Open `Database_Setup.sql`.
3. Run the entire script.
   - Creates database `StockDB`
   - Creates tables: `States`, `Cities`, `Distributors`, `StockPositions`
   - Inserts sample data (5 states, 12 cities, 18 distributors)
   - Creates stored procedures used by the app

### Step 2 — Update the Connection String

Open `Web.config` and change `Data Source` to your SQL Server instance:

```xml
<add name="StockDBConnection"
     connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=StockDB;Integrated Security=True"
     providerName="System.Data.SqlClient" />
```

Common values for `Data Source`:
| Instance | Value |
|---|---|
| SQL Server Express (default) | `.\SQLEXPRESS` or `(local)\SQLEXPRESS` |
| SQL Server Developer/Standard | `.\` or `(local)` or `localhost` |
| Named instance | `SERVER_NAME\INSTANCE_NAME` |

### Step 3 — Open in Visual Studio

1. Open Visual Studio 2019 or 2022.
2. **File → Open → Project/Solution** → select `StockApp.csproj`.
3. Build the solution (**Ctrl+Shift+B**).
4. Press **F5** to run.

---

## How It Works

### Cascading Dropdowns
| User Action | What Happens |
|---|---|
| Page loads | State dropdown is populated; City & Distributor are disabled |
| Select a State | AutoPostBack fires → Cities for that state are loaded |
| Select a City | AutoPostBack fires → Distributors for that city are loaded |
| Select a Distributor | Form is ready for stock entry |

### Form Submission
1. Client-side ASP.NET validators run first (required fields, integer check).
2. On valid submit, `btnSubmit_Click` calls `DatabaseHelper.SaveStockPosition()`.
3. The record is inserted via stored procedure `usp_SaveStockPosition`.
4. A success banner shows the new Record ID; the form resets for the next entry.
5. On error, a red error banner displays the exception message.

---

## Database Tables

### States
| Column | Type | Notes |
|---|---|---|
| StateID | INT IDENTITY | PK |
| StateName | NVARCHAR(100) | |

### Cities
| Column | Type | Notes |
|---|---|---|
| CityID | INT IDENTITY | PK |
| CityName | NVARCHAR(100) | |
| StateID | INT | FK → States |

### Distributors
| Column | Type | Notes |
|---|---|---|
| DistributorID | INT IDENTITY | PK |
| DistributorName | NVARCHAR(200) | |
| CityID | INT | FK → Cities |

### StockPositions
| Column | Type | Notes |
|---|---|---|
| StockPositionID | INT IDENTITY | PK |
| StateID | INT | FK → States |
| CityID | INT | FK → Cities |
| DistributorID | INT | FK → Distributors |
| CurrentStock | INT | Stock count entered |
| EntryDate | DATETIME | Auto-set to GETDATE() |

---

## Adding Your Own Data

To add more states, cities, or distributors:

```sql
-- Add a state
INSERT INTO States (StateName) VALUES ('Rajasthan');

-- Add cities to that state (StateID = the new ID, e.g. 6)
INSERT INTO Cities (CityName, StateID) VALUES ('Jaipur', 6);
INSERT INTO Cities (CityName, StateID) VALUES ('Jodhpur', 6);

-- Add distributors to a city (CityID = the new city's ID)
INSERT INTO Distributors (DistributorName, CityID) VALUES ('Pink City Traders', 13);
```

---

## Requirements

- Visual Studio 2019 / 2022
- .NET Framework 4.8
- SQL Server 2014 or later (Express / Developer / Standard)
