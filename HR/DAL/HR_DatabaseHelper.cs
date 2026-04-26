using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using MySql.Data.MySqlClient;
using System.Web.Configuration;

namespace HRModule
{
    /// <summary>
    /// HR module data access helper.
    /// Follows the same pattern as MMDatabaseHelper / FINDatabaseHelper:
    ///   - Uses connection string named "StockDB" (NEVER "StockDBConnection")
    ///   - All AUTO_INCREMENT / COUNT reads go through Convert.ToInt64
    ///     first, then Convert.ToInt32 (MySQL driver returns UInt64/Int64)
    ///   - GenerateEmployeeCode uses the same overflow-safe pattern
    ///     established in Session 9
    ///
    /// Session 13 additions (bulk import optimization):
    ///   - GenerateEmployeeCodeBatch: single round-trip for many depts
    ///   - InsertEmployee(MySqlConnection, MySqlTransaction, ...) overload
    ///   - GetOrCreateDepartment(MySqlConnection, MySqlTransaction, ...) overload
    /// These are additive — existing single-row callers are untouched.
    /// </summary>
    public static class HR_DatabaseHelper
    {
        // -----------------------------------------------------------------
        // Connection
        // -----------------------------------------------------------------
        public static MySqlConnection GetConnection()
        {
            string cs = WebConfigurationManager.ConnectionStrings["StockDB"].ConnectionString;
            return new MySqlConnection(cs);
        }

        // -----------------------------------------------------------------
        // Code generation - overflow-safe (Session 9 pattern)
        // -----------------------------------------------------------------
        // -----------------------------------------------------------------
        // EmployeeCode generation
        // -----------------------------------------------------------------

        /// <summary>
        /// Legacy parameterless code generator — generates EMP### using the
        /// global EMP[0-9]+ sequence. Used by founders and any pre-prefix
        /// records. New code paths should call the deptId overload.
        /// </summary>
        public static string GenerateEmployeeCode()
        {
            using (MySqlConnection con = GetConnection())
            {
                con.Open();
                string sql = @"SELECT COALESCE(MAX(CAST(SUBSTRING(EmployeeCode, 4) AS UNSIGNED)), 0)
                               FROM HR_Employee
                               WHERE EmployeeCode REGEXP '^EMP[0-9]+$'";
                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                {
                    object o = cmd.ExecuteScalar();
                    long maxNum = (o == null || o == DBNull.Value) ? 0 : Convert.ToInt64(o);
                    int next = Convert.ToInt32(maxNum) + 1;
                    return "EMP" + next.ToString("D3");
                }
            }
        }

        /// <summary>
        /// Department-aware code generator. Looks up the department's
        /// CodePrefix from HR_Department, finds the highest existing code
        /// matching that prefix, returns the next sequential code.
        ///
        /// Throws InvalidOperationException if the department has no
        /// CodePrefix configured — the caller must surface that error.
        ///
        /// Example: SALES dept has CodePrefix='EMPS'.
        ///   - No existing employees with EMPS prefix -> 'EMPS001'
        ///   - Last is 'EMPS057'                       -> 'EMPS058'
        /// </summary>
        public static string GenerateEmployeeCode(int deptId)
        {
            string prefix = GetDepartmentCodePrefix(deptId);
            if (string.IsNullOrEmpty(prefix))
                throw new InvalidOperationException(
                    "Department (ID " + deptId + ") has no CodePrefix configured. " +
                    "Set HR_Department.CodePrefix before generating employee codes for this department.");

            using (MySqlConnection con = GetConnection())
            {
                con.Open();
                // Match codes like EMPS001, EMPS002 — prefix + 1+ digits, nothing else.
                // CONCAT('^', prefix, '[0-9]+$') keeps EMPS001 in but excludes EMPS01A
                // and rejects any longer-prefix codes like EMPSAB001.
                string sql =
                    @"SELECT COALESCE(MAX(CAST(SUBSTRING(EmployeeCode, @plen) AS UNSIGNED)), 0)
                        FROM HR_Employee
                       WHERE EmployeeCode REGEXP CONCAT('^', @prefix, '[0-9]+$')";
                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@plen", prefix.Length + 1);  // SUBSTRING is 1-indexed
                    cmd.Parameters.AddWithValue("@prefix", prefix);
                    object o = cmd.ExecuteScalar();
                    long maxNum = (o == null || o == DBNull.Value) ? 0 : Convert.ToInt64(o);
                    int next = Convert.ToInt32(maxNum) + 1;
                    return prefix + next.ToString("D3");
                }
            }
        }

        /// <summary>
        /// Returns CodePrefix for the given department, or null if not set.
        /// </summary>
        public static string GetDepartmentCodePrefix(int deptId)
        {
            using (MySqlConnection con = GetConnection())
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(
                    "SELECT CodePrefix FROM HR_Department WHERE DeptID = @id", con))
                {
                    cmd.Parameters.AddWithValue("@id", deptId);
                    object o = cmd.ExecuteScalar();
                    if (o == null || o == DBNull.Value) return null;
                    string s = o.ToString().Trim();
                    return s.Length == 0 ? null : s;
                }
            }
        }

        public static string GenerateDeptCode(string deptName)
        {
            // Derive a 3-letter prefix from name; fall back to DPT
            string prefix = "DPT";
            if (!string.IsNullOrEmpty(deptName))
            {
                string cleaned = deptName.Trim().ToUpperInvariant();
                if (cleaned.Length >= 3)
                    prefix = cleaned.Substring(0, 3);
                else
                    prefix = cleaned.PadRight(3, 'X');
            }

            using (MySqlConnection con = GetConnection())
            {
                con.Open();
                string sql = @"SELECT COUNT(*) FROM HR_Department WHERE DeptCode LIKE @p";
                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@p", prefix + "%");
                    long cnt = Convert.ToInt64(cmd.ExecuteScalar());
                    int next = Convert.ToInt32(cnt) + 1;
                    return prefix + next.ToString("D2");
                }
            }
        }

        // =================================================================
        // SESSION 13 — BULK IMPORT OPTIMIZATION HELPERS
        // =================================================================
        // These are designed to be called once before a bulk-insert loop,
        // then drive in-memory code allocation per row, with all inserts
        // sharing one connection + one transaction.
        // =================================================================

        /// <summary>
        /// Holds an in-memory next-suffix counter for one department.
        /// Pre-loaded from the DB once per import so the per-row code
        /// generation is a pure dictionary lookup (no DB round-trip).
        /// </summary>
        public class DeptCodeAllocator
        {
            public int DeptID { get; set; }
            public string Prefix { get; set; }
            public int NextSuffix { get; set; }
            public string DeptName { get; set; }   // for error messages

            public string Allocate()
            {
                string code = Prefix + NextSuffix.ToString("D3");
                NextSuffix++;
                return code;
            }
        }

        /// <summary>
        /// Single-query batch loader: for every department with a CodePrefix,
        /// returns a DeptCodeAllocator pre-set to MAX(suffix)+1.
        ///
        /// Cost: 1 round-trip total (vs. 2 per dept in the per-row path).
        ///
        /// Returns a dict keyed by DeptID. Departments without a CodePrefix
        /// are NOT in the dict — caller must handle missing keys.
        /// </summary>
        public static Dictionary<int, DeptCodeAllocator> GenerateEmployeeCodeBatch(
            MySqlConnection con, MySqlTransaction tx)
        {
            Dictionary<int, DeptCodeAllocator> result = new Dictionary<int, DeptCodeAllocator>();

            // Step 1: pull every dept that has a CodePrefix configured.
            // Cheap (12 rows in prod today) and bounded — fine to do unfiltered.
            using (MySqlCommand cmd = new MySqlCommand(
                @"SELECT DeptID, DeptName, CodePrefix
                    FROM HR_Department
                   WHERE CodePrefix IS NOT NULL AND CodePrefix <> ''", con, tx))
            {
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        int id = Convert.ToInt32(Convert.ToInt64(rdr["DeptID"]));
                        string prefix = rdr["CodePrefix"].ToString().Trim();
                        if (prefix.Length == 0) continue;
                        result[id] = new DeptCodeAllocator
                        {
                            DeptID = id,
                            DeptName = rdr["DeptName"].ToString(),
                            Prefix = prefix,
                            NextSuffix = 1
                        };
                    }
                }
            }

            if (result.Count == 0) return result;

            // Step 2: one grouped query to find MAX(suffix) per prefix in HR_Employee.
            // We do it per-prefix using REGEXP — same shape as the single-row path,
            // just unioned. UNION ALL is faster than building a dynamic IN list and
            // keeps the per-prefix REGEXP local to its branch.
            //
            // Built dynamically because the prefix list is data-driven and short.
            // Parameterized to keep it injection-safe.
            List<string> unions = new List<string>();
            using (MySqlCommand cmd = new MySqlCommand("", con, tx))
            {
                int i = 0;
                foreach (var kvp in result)
                {
                    string p = "@p" + i;
                    string l = "@l" + i;
                    string d = "@d" + i;
                    unions.Add(
                        "SELECT " + d + " AS DeptID, " +
                        "COALESCE(MAX(CAST(SUBSTRING(EmployeeCode, " + l + ") AS UNSIGNED)), 0) AS MaxSfx " +
                        "FROM HR_Employee " +
                        "WHERE EmployeeCode REGEXP CONCAT('^', " + p + ", '[0-9]+$')");
                    cmd.Parameters.AddWithValue(p, kvp.Value.Prefix);
                    cmd.Parameters.AddWithValue(l, kvp.Value.Prefix.Length + 1);
                    cmd.Parameters.AddWithValue(d, kvp.Value.DeptID);
                    i++;
                }
                cmd.CommandText = string.Join(" UNION ALL ", unions);

                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        int deptId = Convert.ToInt32(Convert.ToInt64(rdr["DeptID"]));
                        long max = (rdr["MaxSfx"] == DBNull.Value) ? 0 : Convert.ToInt64(rdr["MaxSfx"]);
                        if (result.TryGetValue(deptId, out DeptCodeAllocator alloc))
                            alloc.NextSuffix = Convert.ToInt32(max) + 1;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Transaction-aware GetOrCreateDepartment used during bulk import.
        /// SELECTs by name (case-insensitive), INSERTs if missing.
        ///
        /// IMPORTANT: when this method creates a new dept, CodePrefix is
        /// left NULL — same behaviour as the legacy single-arg overload.
        /// The caller (import) must therefore handle "dept exists but no
        /// prefix" by erroring out the row gracefully, since the
        /// in-memory allocator dict won't have an entry for that DeptID.
        /// </summary>
        public static int GetOrCreateDepartment(string deptName, string createdBy,
                                                MySqlConnection con, MySqlTransaction tx)
        {
            if (string.IsNullOrWhiteSpace(deptName)) return 0;
            deptName = deptName.Trim();

            using (MySqlCommand find = new MySqlCommand(
                "SELECT DeptID FROM HR_Department WHERE DeptName = @n LIMIT 1", con, tx))
            {
                find.Parameters.AddWithValue("@n", deptName);
                object o = find.ExecuteScalar();
                if (o != null && o != DBNull.Value)
                    return Convert.ToInt32(Convert.ToInt64(o));
            }

            // Not found -> create. Generate a 3-letter dept code by counting
            // existing rows with that prefix WITHIN this transaction so
            // concurrent inserts in this same import don't collide.
            string prefix = "DPT";
            string cleaned = deptName.Trim().ToUpperInvariant();
            if (cleaned.Length >= 3) prefix = cleaned.Substring(0, 3);
            else if (cleaned.Length > 0) prefix = cleaned.PadRight(3, 'X');

            string code;
            using (MySqlCommand cnt = new MySqlCommand(
                "SELECT COUNT(*) FROM HR_Department WHERE DeptCode LIKE @p", con, tx))
            {
                cnt.Parameters.AddWithValue("@p", prefix + "%");
                long c = Convert.ToInt64(cnt.ExecuteScalar());
                code = prefix + (Convert.ToInt32(c) + 1).ToString("D2");
            }

            using (MySqlCommand ins = new MySqlCommand(
                @"INSERT INTO HR_Department (DeptCode, DeptName, IsActive, CreatedBy)
                  VALUES (@c, @n, 1, @u); SELECT LAST_INSERT_ID();", con, tx))
            {
                ins.Parameters.AddWithValue("@c", code);
                ins.Parameters.AddWithValue("@n", deptName);
                ins.Parameters.AddWithValue("@u", createdBy ?? "SYSTEM");
                long id = Convert.ToInt64(ins.ExecuteScalar());
                return Convert.ToInt32(id);
            }
        }

        /// <summary>
        /// Transaction-aware InsertEmployee for bulk paths. Same SQL as
        /// the standalone InsertEmployee — just shares the caller's
        /// connection + transaction so 57 inserts are 1 round-trip
        /// per row instead of 1 connection-open per row.
        /// </summary>
        public static int InsertEmployee(EmployeeRecord e, string createdBy,
                                         MySqlConnection con, MySqlTransaction tx)
        {
            string sql = @"
                INSERT INTO HR_Employee
                  (EmployeeCode, FullName, FatherName, Gender, DOB, DOJ, DOL,
                   DeptID, Designation, ReportingManager, Zone, Region, Area, WorkLocation,
                   EmploymentType,
                   MobileNo, AltMobileNo, Email, AddressLine, City, StateName, Pincode,
                   AadhaarNo, PANNo, UANNo, PFNo, ESINo,
                   BankAccountNo, BankName, IFSCCode,
                   BasicSalary, HRA, ConveyanceAllow, OtherAllow, GrossSalary,
                   IsActive, CreatedBy)
                VALUES
                  (@EmployeeCode, @FullName, @FatherName, @Gender, @DOB, @DOJ, @DOL,
                   @DeptID, @Designation, @ReportingManager, @Zone, @Region, @Area, @WorkLocation,
                   @EmploymentType,
                   @MobileNo, @AltMobileNo, @Email, @AddressLine, @City, @StateName, @Pincode,
                   @AadhaarNo, @PANNo, @UANNo, @PFNo, @ESINo,
                   @BankAccountNo, @BankName, @IFSCCode,
                   @BasicSalary, @HRA, @ConveyanceAllow, @OtherAllow, @GrossSalary,
                   @IsActive, @CreatedBy);
                SELECT LAST_INSERT_ID();";

            using (MySqlCommand cmd = new MySqlCommand(sql, con, tx))
            {
                BindEmployeeParams(cmd, e);
                cmd.Parameters.AddWithValue("@CreatedBy", createdBy ?? "SYSTEM");
                long id = Convert.ToInt64(cmd.ExecuteScalar());
                return Convert.ToInt32(id);
            }
        }

        // -----------------------------------------------------------------
        // Department CRUD
        // -----------------------------------------------------------------
        public static DataTable GetDepartments(bool activeOnly)
        {
            using (MySqlConnection con = GetConnection())
            {
                con.Open();
                string sql = "SELECT DeptID, DeptCode, DeptName, CodePrefix, IsActive, CreatedAt FROM HR_Department";
                if (activeOnly) sql += " WHERE IsActive = 1";
                sql += " ORDER BY DeptName";
                using (MySqlDataAdapter da = new MySqlDataAdapter(sql, con))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public static int GetOrCreateDepartment(string deptName, string createdBy)
        {
            if (string.IsNullOrWhiteSpace(deptName)) return 0;
            deptName = deptName.Trim();

            using (MySqlConnection con = GetConnection())
            {
                con.Open();
                using (MySqlCommand find = new MySqlCommand(
                    "SELECT DeptID FROM HR_Department WHERE DeptName = @n LIMIT 1", con))
                {
                    find.Parameters.AddWithValue("@n", deptName);
                    object o = find.ExecuteScalar();
                    if (o != null && o != DBNull.Value)
                        return Convert.ToInt32(Convert.ToInt64(o));
                }
            }

            // Not found -> create
            string code = GenerateDeptCode(deptName);
            using (MySqlConnection con = GetConnection())
            {
                con.Open();
                using (MySqlCommand ins = new MySqlCommand(
                    @"INSERT INTO HR_Department (DeptCode, DeptName, IsActive, CreatedBy)
                      VALUES (@c, @n, 1, @u); SELECT LAST_INSERT_ID();", con))
                {
                    ins.Parameters.AddWithValue("@c", code);
                    ins.Parameters.AddWithValue("@n", deptName);
                    ins.Parameters.AddWithValue("@u", createdBy ?? "SYSTEM");
                    long id = Convert.ToInt64(ins.ExecuteScalar());
                    return Convert.ToInt32(id);
                }
            }
        }

        public static int InsertDepartment(string code, string name, string createdBy)
        {
            return InsertDepartment(code, name, null, createdBy);
        }

        public static int InsertDepartment(string code, string name, string codePrefix, string createdBy)
        {
            using (MySqlConnection con = GetConnection())
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(
                    @"INSERT INTO HR_Department (DeptCode, DeptName, CodePrefix, IsActive, CreatedBy)
                      VALUES (@c, @n, @p, 1, @u); SELECT LAST_INSERT_ID();", con))
                {
                    cmd.Parameters.AddWithValue("@c", code);
                    cmd.Parameters.AddWithValue("@n", name);
                    cmd.Parameters.AddWithValue("@p",
                        string.IsNullOrWhiteSpace(codePrefix) ? (object)DBNull.Value : codePrefix.Trim().ToUpperInvariant());
                    cmd.Parameters.AddWithValue("@u", createdBy ?? "SYSTEM");
                    long id = Convert.ToInt64(cmd.ExecuteScalar());
                    return Convert.ToInt32(id);
                }
            }
        }

        public static void UpdateDepartment(int deptId, string code, string name, bool isActive, string modifiedBy)
        {
            UpdateDepartment(deptId, code, name, null, isActive, modifiedBy, /*updatePrefix*/ false);
        }

        public static void UpdateDepartment(int deptId, string code, string name, string codePrefix,
                                            bool isActive, string modifiedBy, bool updatePrefix)
        {
            using (MySqlConnection con = GetConnection())
            {
                con.Open();
                string sql = updatePrefix
                    ? @"UPDATE HR_Department
                           SET DeptCode = @c, DeptName = @n, CodePrefix = @p, IsActive = @a,
                               ModifiedAt = NOW(), ModifiedBy = @u
                         WHERE DeptID = @id"
                    : @"UPDATE HR_Department
                           SET DeptCode = @c, DeptName = @n, IsActive = @a,
                               ModifiedAt = NOW(), ModifiedBy = @u
                         WHERE DeptID = @id";
                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@c", code);
                    cmd.Parameters.AddWithValue("@n", name);
                    if (updatePrefix)
                        cmd.Parameters.AddWithValue("@p",
                            string.IsNullOrWhiteSpace(codePrefix) ? (object)DBNull.Value : codePrefix.Trim().ToUpperInvariant());
                    cmd.Parameters.AddWithValue("@a", isActive ? 1 : 0);
                    cmd.Parameters.AddWithValue("@u", modifiedBy ?? "SYSTEM");
                    cmd.Parameters.AddWithValue("@id", deptId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // -----------------------------------------------------------------
        // Employee CRUD
        // -----------------------------------------------------------------
        public static DataTable GetEmployees(string searchTerm, int? deptId, bool activeOnly)
        {
            using (MySqlConnection con = GetConnection())
            {
                con.Open();
                string sql = @"
                    SELECT e.EmployeeID, e.EmployeeCode, e.FullName, e.Gender, e.DOJ, e.DOL,
                           d.DeptName, e.Designation, e.EmploymentType,
                           e.MobileNo, e.AadhaarNo, e.UANNo, e.GrossSalary, e.IsActive
                      FROM HR_Employee e
                      INNER JOIN HR_Department d ON d.DeptID = e.DeptID
                     WHERE 1=1 ";
                if (activeOnly) sql += " AND e.IsActive = 1 ";
                if (deptId.HasValue && deptId.Value > 0) sql += " AND e.DeptID = @d ";
                if (!string.IsNullOrWhiteSpace(searchTerm))
                    sql += " AND (e.EmployeeCode LIKE @s OR e.FullName LIKE @s OR e.MobileNo LIKE @s) ";
                sql += " ORDER BY e.EmployeeCode";

                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                {
                    if (deptId.HasValue && deptId.Value > 0)
                        cmd.Parameters.AddWithValue("@d", deptId.Value);
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                        cmd.Parameters.AddWithValue("@s", "%" + searchTerm.Trim() + "%");

                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public static DataRow GetEmployeeById(int employeeId)
        {
            using (MySqlConnection con = GetConnection())
            {
                con.Open();
                using (MySqlDataAdapter da = new MySqlDataAdapter(
                    "SELECT * FROM HR_Employee WHERE EmployeeID = @id", con))
                {
                    da.SelectCommand.Parameters.AddWithValue("@id", employeeId);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt.Rows.Count > 0 ? dt.Rows[0] : null;
                }
            }
        }

        public static bool EmployeeCodeExists(string code, int? excludeId = null)
        {
            using (MySqlConnection con = GetConnection())
            {
                con.Open();
                string sql = "SELECT COUNT(*) FROM HR_Employee WHERE EmployeeCode = @c";
                if (excludeId.HasValue) sql += " AND EmployeeID <> @id";
                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@c", code);
                    if (excludeId.HasValue) cmd.Parameters.AddWithValue("@id", excludeId.Value);
                    long n = Convert.ToInt64(cmd.ExecuteScalar());
                    return n > 0;
                }
            }
        }

        public static int InsertEmployee(EmployeeRecord e, string createdBy)
        {
            using (MySqlConnection con = GetConnection())
            {
                con.Open();
                string sql = @"
                    INSERT INTO HR_Employee
                      (EmployeeCode, FullName, FatherName, Gender, DOB, DOJ, DOL,
                       DeptID, Designation, ReportingManager, Zone, Region, Area, WorkLocation,
                       EmploymentType,
                       MobileNo, AltMobileNo, Email, AddressLine, City, StateName, Pincode,
                       AadhaarNo, PANNo, UANNo, PFNo, ESINo,
                       BankAccountNo, BankName, IFSCCode,
                       BasicSalary, HRA, ConveyanceAllow, OtherAllow, GrossSalary,
                       IsActive, CreatedBy)
                    VALUES
                      (@EmployeeCode, @FullName, @FatherName, @Gender, @DOB, @DOJ, @DOL,
                       @DeptID, @Designation, @ReportingManager, @Zone, @Region, @Area, @WorkLocation,
                       @EmploymentType,
                       @MobileNo, @AltMobileNo, @Email, @AddressLine, @City, @StateName, @Pincode,
                       @AadhaarNo, @PANNo, @UANNo, @PFNo, @ESINo,
                       @BankAccountNo, @BankName, @IFSCCode,
                       @BasicSalary, @HRA, @ConveyanceAllow, @OtherAllow, @GrossSalary,
                       @IsActive, @CreatedBy);
                    SELECT LAST_INSERT_ID();";

                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                {
                    BindEmployeeParams(cmd, e);
                    cmd.Parameters.AddWithValue("@CreatedBy", createdBy ?? "SYSTEM");
                    long id = Convert.ToInt64(cmd.ExecuteScalar());
                    return Convert.ToInt32(id);
                }
            }
        }

        public static void UpdateEmployee(EmployeeRecord e, string modifiedBy)
        {
            using (MySqlConnection con = GetConnection())
            {
                con.Open();
                string sql = @"
                    UPDATE HR_Employee SET
                       EmployeeCode = @EmployeeCode, FullName = @FullName, FatherName = @FatherName,
                       Gender = @Gender, DOB = @DOB, DOJ = @DOJ, DOL = @DOL,
                       DeptID = @DeptID, Designation = @Designation,
                       ReportingManager = @ReportingManager, Zone = @Zone, Region = @Region,
                       Area = @Area, WorkLocation = @WorkLocation,
                       EmploymentType = @EmploymentType,
                       MobileNo = @MobileNo, AltMobileNo = @AltMobileNo, Email = @Email,
                       AddressLine = @AddressLine, City = @City, StateName = @StateName, Pincode = @Pincode,
                       AadhaarNo = @AadhaarNo, PANNo = @PANNo, UANNo = @UANNo, PFNo = @PFNo, ESINo = @ESINo,
                       BankAccountNo = @BankAccountNo, BankName = @BankName, IFSCCode = @IFSCCode,
                       BasicSalary = @BasicSalary, HRA = @HRA, ConveyanceAllow = @ConveyanceAllow,
                       OtherAllow = @OtherAllow, GrossSalary = @GrossSalary,
                       IsActive = @IsActive,
                       ModifiedAt = NOW(), ModifiedBy = @ModifiedBy
                     WHERE EmployeeID = @EmployeeID";

                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                {
                    BindEmployeeParams(cmd, e);
                    cmd.Parameters.AddWithValue("@EmployeeID", e.EmployeeID);
                    cmd.Parameters.AddWithValue("@ModifiedBy", modifiedBy ?? "SYSTEM");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void BindEmployeeParams(MySqlCommand cmd, EmployeeRecord e)
        {
            cmd.Parameters.AddWithValue("@EmployeeCode", e.EmployeeCode ?? "");
            cmd.Parameters.AddWithValue("@FullName", e.FullName ?? "");
            cmd.Parameters.AddWithValue("@FatherName", (object)e.FatherName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Gender", e.Gender ?? "M");
            cmd.Parameters.AddWithValue("@DOB", (object)e.DOB ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DOJ", e.DOJ);
            cmd.Parameters.AddWithValue("@DOL", (object)e.DOL ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DeptID", e.DeptID);
            cmd.Parameters.AddWithValue("@Designation", (object)e.Designation ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReportingManager", (object)e.ReportingManager ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Zone", (object)e.Zone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Region", (object)e.Region ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Area", (object)e.Area ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@WorkLocation", (object)e.WorkLocation ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EmploymentType", e.EmploymentType ?? "Permanent");
            cmd.Parameters.AddWithValue("@MobileNo", (object)e.MobileNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AltMobileNo", (object)e.AltMobileNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object)e.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AddressLine", (object)e.AddressLine ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@City", (object)e.City ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StateName", (object)e.StateName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Pincode", (object)e.Pincode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AadhaarNo", (object)e.AadhaarNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PANNo", (object)e.PANNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UANNo", (object)e.UANNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PFNo", (object)e.PFNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ESINo", (object)e.ESINo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BankAccountNo", (object)e.BankAccountNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BankName", (object)e.BankName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IFSCCode", (object)e.IFSCCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BasicSalary", e.BasicSalary);
            cmd.Parameters.AddWithValue("@HRA", e.HRA);
            cmd.Parameters.AddWithValue("@ConveyanceAllow", e.ConveyanceAllow);
            cmd.Parameters.AddWithValue("@OtherAllow", e.OtherAllow);
            cmd.Parameters.AddWithValue("@GrossSalary", e.GrossSalary);
            cmd.Parameters.AddWithValue("@IsActive", e.IsActive ? 1 : 0);
        }

        // -----------------------------------------------------------------
        // Validation helpers
        // -----------------------------------------------------------------
        public static bool IsValidAadhaar(string a)
        {
            if (string.IsNullOrWhiteSpace(a)) return true;   // optional
            a = a.Replace(" ", "").Replace("-", "");
            if (a.Length != 12) return false;
            foreach (char c in a) if (!char.IsDigit(c)) return false;
            return true;
        }

        public static bool IsValidPAN(string p)
        {
            if (string.IsNullOrWhiteSpace(p)) return true;
            p = p.Trim().ToUpperInvariant();
            if (p.Length != 10) return false;
            for (int i = 0; i < 5; i++) if (!char.IsLetter(p[i])) return false;
            for (int i = 5; i < 9; i++) if (!char.IsDigit(p[i])) return false;
            if (!char.IsLetter(p[9])) return false;
            return true;
        }

        public static bool IsValidIFSC(string ifsc)
        {
            if (string.IsNullOrWhiteSpace(ifsc)) return true;
            ifsc = ifsc.Trim().ToUpperInvariant();
            if (ifsc.Length != 11) return false;
            for (int i = 0; i < 4; i++) if (!char.IsLetter(ifsc[i])) return false;
            if (ifsc[4] != '0') return false;
            return true;
        }
    }

    // ---------------------------------------------------------------------
    // Transport record for employee rows (used by pages + import)
    // ---------------------------------------------------------------------
    public class EmployeeRecord
    {
        public int EmployeeID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string FatherName { get; set; }
        public string Gender { get; set; } = "M";
        public DateTime? DOB { get; set; }
        public DateTime DOJ { get; set; } = DateTime.Today;
        public DateTime? DOL { get; set; }
        public int DeptID { get; set; }
        public string Designation { get; set; }
        // Organization / Sales territory
        public string ReportingManager { get; set; }
        public string Zone { get; set; }
        public string Region { get; set; }
        public string Area { get; set; }
        public string WorkLocation { get; set; }
        public string EmploymentType { get; set; } = "Permanent";
        public string MobileNo { get; set; }
        public string AltMobileNo { get; set; }
        public string Email { get; set; }
        public string AddressLine { get; set; }
        public string City { get; set; }
        public string StateName { get; set; }
        public string Pincode { get; set; }
        public string AadhaarNo { get; set; }
        public string PANNo { get; set; }
        public string UANNo { get; set; }
        public string PFNo { get; set; }
        public string ESINo { get; set; }
        public string BankAccountNo { get; set; }
        public string BankName { get; set; }
        public string IFSCCode { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal HRA { get; set; }
        public decimal ConveyanceAllow { get; set; }
        public decimal OtherAllow { get; set; }
        public decimal GrossSalary { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
