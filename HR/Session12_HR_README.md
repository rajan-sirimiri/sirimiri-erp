# Session 12 — HR Module Foundation

## What this session delivers

- New **HR** module as a separate VS project + IIS application (parallel to MM/PP/PK/FIN)
- Two master tables: `HR_Department`, `HR_Employee` (with PF/ESI/UAN/Aadhaar/Bank KYC)
- Employee master CRUD with role gate (Super + Admin)
- Department master CRUD with role gate
- Excel bulk import with preview/validate/confirm flow (ClosedXML 0.95.0)
- Auto-generated `EMPnnn` codes (Session 9 overflow-safe pattern)
- Auto-create missing departments during import (mirrors AutoMatchCustomers)

Biometric attendance (device listener, raw punches, daily attendance) will plug
into this foundation in Session 13.

---

## Files in this delivery

```
/HR/
  HR.csproj                       -- new VS project
  web.config                      -- StockDB conn string, auth, 30MB upload
  Default.aspx                    -- redirect to HREmployee
  HR_DatabaseHelper.cs            -- DAL + EmployeeRecord DTO
  HRDepartment.aspx/.cs           -- department master CRUD
  HREmployee.aspx/.cs             -- employee master CRUD (list + edit views)
  HREmployeeImport.aspx/.cs       -- Excel preview + confirm import
  HREmployeeImport.ashx/.ashx.cs  -- template download handler

/SQL/
  Session12_HR_Migration.sql      -- CREATE TABLE for HR_Department, HR_Employee
```

---

## Deployment steps

### 1. Run migration

```bash
mysql -h localhost -P 3308 -u sirimiri_app -p sirimiri_erp < Session12_HR_Migration.sql
```

Confirm `SELECT * FROM HR_Department;` shows the seeded "General" row.

### 2. Add HR project to the solution

In Visual Studio:

1. **File → Add → Existing Project** → pick `HR/HR.csproj`
2. Right-click HR project → **Manage NuGet Packages**
3. Install:
   - `MySql.Data` (match the version used by MM/FIN modules)
   - `ClosedXML` **version 0.95.0** (pinned — later versions require net46+)
4. Build the HR project (Release mode)

### 3. Deploy to VPS

Copy the built HR project folder to the VPS under httpdocs:

| Source                          | Destination on VPS (Plesk path)                 |
|---------------------------------|-------------------------------------------------|
| `HR\bin\*`                      | `C:\Inetpub\vhosts\vimarsa.in\httpdocs\HR\bin\` |
| `HR\*.aspx`, `*.ashx`           | `C:\Inetpub\vhosts\vimarsa.in\httpdocs\HR\`     |
| `HR\web.config`                 | `C:\Inetpub\vhosts\vimarsa.in\httpdocs\HR\`     |

Edit the copied `web.config` and replace `Pwd=REPLACE_ME` with the real MySQL
password (same value as other modules' web.config).

### 4. Convert /HR to an IIS Application

1. Open IIS Manager on VPS
2. Navigate to `Sites → vimarsa.in → httpdocs → HR`
3. Right-click `HR` → **Convert to Application**
4. Application pool: pick the Plesk-managed **vimarsa.in** pool (same as MM/PP/PK)
5. The folder icon should change to the globe icon ✅

### 5. File permissions

The App_Data/ImportTemp folder gets auto-created on first upload, but the IUSR
/ IWPD_n account needs write access to the HR folder. From an elevated cmd:

```cmd
icacls "C:\Inetpub\vhosts\vimarsa.in\httpdocs\HR\App_Data" /grant "IUSR:(OI)(CI)(M)" /T
```

(Replace IUSR with the Plesk-generated username if different — parentheses in
Plesk usernames need quoting, as noted in the Mar 26 notes.)

### 6. Add HR menu entries

Edit the shared `MenuMaster.master` (or equivalent nav partial) and add under
the main nav, visible only when `Session["UserRole"] in ("Super", "Admin")`:

```html
<li class="nav-dropdown">
    <a href="#">HR ▾</a>
    <ul>
        <li><a href="/HR/HREmployee.aspx">Employees</a></li>
        <li><a href="/HR/HRDepartment.aspx">Departments</a></li>
        <li><a href="/HR/HREmployeeImport.aspx">Import Employees</a></li>
    </ul>
</li>
```

### 7. Smoke test

1. Log in as Super
2. Navigate to `/HR/HRDepartment.aspx` → add "Production" and "Admin" depts
3. Navigate to `/HR/HREmployee.aspx` → click "+ New Employee", code should auto-populate as `EMP001`
4. Save one employee; verify it appears in the list
5. Navigate to `/HR/HREmployeeImport.aspx` → "Download Template"
6. Fill 2-3 rows in the template, upload, verify preview shows READY rows
7. Click "Import Ready Rows" → confirm import adds to DB

---

## Design notes worth re-reading before Session 13

- **Aadhaar is VARCHAR(12), not BIGINT** — leading-zero Aadhaars exist, never
  do arithmetic on it. Same rule for Mobile, PAN, IFSC, UAN.
- **No UNIQUE on AadhaarNo/PAN** — intentional. Import handles duplicates as
  warnings, not DB-level rejections.
- **Salary on master is temporary** — when payroll module arrives, introduce
  `HR_EmployeeSalaryHistory(EmployeeID, EffectiveFrom, Basic, HRA, ...)` and
  compute payroll against the dated row.
- **`DOL` (Date of Leaving) nullable** — preserves history for ex-employees.
  `IsActive` stays as a convenience filter but `DOL IS NOT NULL` is the source
  of truth for "left".
- **Role codes in DB are "Super" and "Admin"** (not display names) — all role
  gates match this convention.
- **ASHX pattern**: `.ashx` + `.ashx.cs` pair, never inline — Plesk
  precompiled deploy strips inline code (Session 9 lesson).
- **Postback restore for import panel**: `hfFilePath` check in `Page_Load`
  restores `pnlResults.Visible = true` before controls are needed (Session 9
  lesson on panels with `Visible=false` not instantiating children).
- **EscapeJson / JSON output**: not needed this session (no JSON endpoints),
  but keep the rule in mind when we add the biometric listener next session.

---

## What's next — Session 13 preview (Biometric Attendance)

Once you're comfortable with employee data in the HR tables, the biometric
listener adds:

```
HR_Device           -- registered eSSL devices (SerialNo, Location, LastSeen)
HR_DevicePunch      -- raw push payload, one row per punch
HR_EmployeeDevMap   -- bridges device user ID -> EmployeeID
HR_DailyAttendance  -- derived, rebuildable from raw
HR_DeviceCommand    -- (future) push enroll/clear/reboot

Pages:
  HRDeviceListener.ashx  -- /iclock/cdata + /iclock/getrequest endpoints
  HRDevice.aspx          -- register devices, view heartbeat
  HRPunchLog.aspx        -- raw punch viewer with unmapped highlight
  HRDailyAttendance.aspx -- one row per employee per day
```

Device config on eSSL (in device menu):
- Server URL: `https://vimarsa.in/HR/HRDeviceListener.ashx` (no path component —
  ADMS appends `/iclock/cdata`, so we'll route inside the handler by
  parsing `Request.RawUrl`)
- Port: 443
- Enable ADMS / Cloud Server

We'll get to that when you're ready.
