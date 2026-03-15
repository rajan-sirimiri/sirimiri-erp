# DataImport — Setup Instructions

## Step 1 — Add Project to Existing Solution in Visual Studio

1. Open your existing solution (StockApp.sln)
2. Right-click the **Solution** in Solution Explorer (top level)
3. Click **Add** → **Existing Project**
4. Browse to the `DataImport` folder → select `DataImport.csproj`
5. Click **Open**

## Step 2 — Install EPPlus NuGet Package

In Visual Studio, right-click the **DataImport** project → **Manage NuGet Packages**
Search for `EPPlus` → Install version **5.8.x** (free non-commercial license)

Or via Package Manager Console:
```
Install-Package EPPlus -Version 5.8.14 -ProjectName DataImport
```

## Step 3 — Copy MySql.Data.dll Reference

The DataImport project references MySql.Data.dll from the StockApp bin folder.
Make sure StockApp is built first so the DLL exists at:
```
..\StockApp\bin\MySql.Data.dll
```

## Step 4 — Build and Publish

1. Right-click **DataImport** project → **Publish**
2. Choose **Folder** as publish target
3. Publish to a local folder e.g. `C:\Publish\DataImport`
4. Copy published files to VPS: `C:\inetpub\vhosts\vimarsa.in\httpdocs\DataImport\`

## Step 5 — Configure IIS on VPS

1. Open IIS Manager
2. Expand Sites → vimarsa.in → httpdocs
3. Right-click **DataImport** folder → **Convert to Application**
4. Application Pool: select the vimarsa.in pool (4.0, Integrated)
5. Click OK

## Step 6 — Test

Open browser and go to:
```
https://vimarsa.in/DataImport/
```

Login with same credentials as StockApp (admin/Admin@123)

## File Structure on VPS
```
C:\inetpub\vhosts\vimarsa.in\httpdocs\
├── StockApp\          ← existing app
│   └── (all StockApp files)
└── DataImport\        ← new app
    ├── bin\
    │   ├── DataImport.dll
    │   ├── MySql.Data.dll
    │   └── EPPlus.dll
    ├── DAL\
    ├── Login.aspx
    ├── ChangePassword.aspx
    ├── Import.aspx
    ├── Logout.aspx
    └── Web.config
```

## Notes
- Uses the same StockDB MySQL database as StockApp
- Uses the same Users table for authentication
- Field Users are blocked — Admin and Manager only
- Duplicate records are automatically skipped on import
- Large files supported (up to 100MB configured in Web.config)
