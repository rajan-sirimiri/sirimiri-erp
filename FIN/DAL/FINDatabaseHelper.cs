// =====================================================================
// FIN/DAL/FINDatabaseHelper.cs  (ADDITIONS)
// Add the methods below into the existing FINDatabaseHelper partial class.
// If FINDatabaseHelper is a single class (not partial), paste the method
// bodies into the class directly and keep the using-directives at top.
// =====================================================================

using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace Sirimiri.FIN.DAL
{
    public partial class FINDatabaseHelper
    {
        // -------------------------------------------------------------
        // POCOs
        // -------------------------------------------------------------
        public class PartyOpeningBalance
        {
            public long OpeningID { get; set; }
            public string PartyType { get; set; }
            public int PartyID { get; set; }
            public string FY { get; set; }
            public DateTime AsOfDate { get; set; }
            public decimal Amount { get; set; }
            public string DrCr { get; set; }
            public string Reason { get; set; }
            public string CreatedBy { get; set; }
            public DateTime CreatedOn { get; set; }
            public string LastModifiedBy { get; set; }
            public DateTime? LastModifiedOn { get; set; }
        }

        public class PartyOpeningAudit
        {
            public long AuditID { get; set; }
            public string ActionType { get; set; }
            public decimal? OldAmount { get; set; }
            public string OldDrCr { get; set; }
            public decimal NewAmount { get; set; }
            public string NewDrCr { get; set; }
            public string Reason { get; set; }
            public string ChangedBy { get; set; }
            public DateTime ChangedOn { get; set; }
        }

        public class PartyInfo
        {
            public int PartyID { get; set; }
            public string PartyType { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
            public string GSTIN { get; set; }
            public bool IsActive { get; set; }
        }

        public class StatementLine
        {
            public DateTime TxnDate { get; set; }
            public string VoucherNo { get; set; }
            public string Particulars { get; set; }
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
            public decimal RunningBalance { get; set; } // populated post-query
            public string RunningDrCr { get; set; }     // Dr|Cr
            public string SourceTable { get; set; }
        }

        // -------------------------------------------------------------
        // Opening Balance: Get / Save
        // -------------------------------------------------------------

        /// <summary>
        /// Returns the opening balance row for (partyType, partyID, fy) or null if none.
        /// </summary>
        public PartyOpeningBalance GetOpeningBalance(string partyType, int partyID, string fy)
        {
            const string sql = @"
                SELECT OpeningID, PartyType, PartyID, FY, AsOfDate, Amount, DrCr,
                       Reason, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn
                FROM FIN_PartyOpeningBalance
                WHERE PartyType = @pt AND PartyID = @pid AND FY = @fy
                LIMIT 1;";

            using (var conn = new MySqlConnection(GetConnectionString()))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@pt", partyType);
                cmd.Parameters.AddWithValue("@pid", partyID);
                cmd.Parameters.AddWithValue("@fy", fy);
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read()) return null;
                    return new PartyOpeningBalance
                    {
                        OpeningID = Convert.ToInt64(rdr["OpeningID"]),
                        PartyType = rdr["PartyType"].ToString(),
                        PartyID = Convert.ToInt32(rdr["PartyID"]),
                        FY = rdr["FY"].ToString(),
                        AsOfDate = Convert.ToDateTime(rdr["AsOfDate"]),
                        Amount = Convert.ToDecimal(rdr["Amount"]),
                        DrCr = rdr["DrCr"].ToString(),
                        Reason = rdr["Reason"] == DBNull.Value ? null : rdr["Reason"].ToString(),
                        CreatedBy = rdr["CreatedBy"].ToString(),
                        CreatedOn = Convert.ToDateTime(rdr["CreatedOn"]),
                        LastModifiedBy = rdr["LastModifiedBy"] == DBNull.Value ? null : rdr["LastModifiedBy"].ToString(),
                        LastModifiedOn = rdr["LastModifiedOn"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["LastModifiedOn"])
                    };
                }
            }
        }

        /// <summary>
        /// Insert or update opening balance. Writes audit row in the same transaction.
        /// Reason is required when ActionType = UPDATE (caller enforces; DAL writes what it's given).
        /// </summary>
        public long SaveOpeningBalance(string partyType, int partyID, string fy,
                                       DateTime asOfDate, decimal amount, string drCr,
                                       string reason, string user)
        {
            using (var conn = new MySqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    // 1) Find existing row
                    long openingID = 0;
                    decimal? oldAmount = null;
                    string oldDrCr = null;

                    using (var cmd = new MySqlCommand(
                        @"SELECT OpeningID, Amount, DrCr FROM FIN_PartyOpeningBalance
                          WHERE PartyType=@pt AND PartyID=@pid AND FY=@fy LIMIT 1;", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@pt", partyType);
                        cmd.Parameters.AddWithValue("@pid", partyID);
                        cmd.Parameters.AddWithValue("@fy", fy);
                        using (var rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                openingID = Convert.ToInt64(rdr["OpeningID"]);
                                oldAmount = Convert.ToDecimal(rdr["Amount"]);
                                oldDrCr = rdr["DrCr"].ToString();
                            }
                        }
                    }

                    string actionType;
                    if (openingID == 0)
                    {
                        // INSERT
                        actionType = "INSERT";
                        using (var cmd = new MySqlCommand(
                            @"INSERT INTO FIN_PartyOpeningBalance
                                (PartyType, PartyID, FY, AsOfDate, Amount, DrCr, Reason, CreatedBy, CreatedOn)
                              VALUES (@pt, @pid, @fy, @asof, @amt, @drcr, @reason, @user, NOW());
                              SELECT LAST_INSERT_ID();", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@pt", partyType);
                            cmd.Parameters.AddWithValue("@pid", partyID);
                            cmd.Parameters.AddWithValue("@fy", fy);
                            cmd.Parameters.AddWithValue("@asof", asOfDate);
                            cmd.Parameters.AddWithValue("@amt", amount);
                            cmd.Parameters.AddWithValue("@drcr", drCr);
                            cmd.Parameters.AddWithValue("@reason", (object)reason ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@user", user);
                            openingID = Convert.ToInt64(cmd.ExecuteScalar());
                        }
                    }
                    else
                    {
                        // UPDATE
                        actionType = "UPDATE";
                        using (var cmd = new MySqlCommand(
                            @"UPDATE FIN_PartyOpeningBalance
                              SET AsOfDate=@asof, Amount=@amt, DrCr=@drcr, Reason=@reason,
                                  LastModifiedBy=@user, LastModifiedOn=NOW()
                              WHERE OpeningID=@oid;", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@asof", asOfDate);
                            cmd.Parameters.AddWithValue("@amt", amount);
                            cmd.Parameters.AddWithValue("@drcr", drCr);
                            cmd.Parameters.AddWithValue("@reason", (object)reason ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@user", user);
                            cmd.Parameters.AddWithValue("@oid", openingID);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // 2) Write audit
                    using (var cmd = new MySqlCommand(
                        @"INSERT INTO FIN_PartyOpeningBalance_Audit
                            (OpeningID, PartyType, PartyID, FY, ActionType,
                             OldAmount, OldDrCr, NewAmount, NewDrCr, Reason, ChangedBy, ChangedOn)
                          VALUES (@oid, @pt, @pid, @fy, @act,
                                  @oldamt, @olddrcr, @newamt, @newdrcr, @reason, @user, NOW());", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@oid", openingID);
                        cmd.Parameters.AddWithValue("@pt", partyType);
                        cmd.Parameters.AddWithValue("@pid", partyID);
                        cmd.Parameters.AddWithValue("@fy", fy);
                        cmd.Parameters.AddWithValue("@act", actionType);
                        cmd.Parameters.AddWithValue("@oldamt", (object)oldAmount ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@olddrcr", (object)oldDrCr ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@newamt", amount);
                        cmd.Parameters.AddWithValue("@newdrcr", drCr);
                        cmd.Parameters.AddWithValue("@reason", (object)reason ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@user", user);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                    return openingID;
                }
            }
        }

        /// <summary>Audit history (latest first) for the given party+FY.</summary>
        public List<PartyOpeningAudit> GetOpeningBalanceAudit(string partyType, int partyID, string fy)
        {
            var list = new List<PartyOpeningAudit>();
            const string sql = @"
                SELECT AuditID, ActionType, OldAmount, OldDrCr, NewAmount, NewDrCr,
                       Reason, ChangedBy, ChangedOn
                FROM FIN_PartyOpeningBalance_Audit
                WHERE PartyType=@pt AND PartyID=@pid AND FY=@fy
                ORDER BY ChangedOn DESC, AuditID DESC;";

            using (var conn = new MySqlConnection(GetConnectionString()))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@pt", partyType);
                cmd.Parameters.AddWithValue("@pid", partyID);
                cmd.Parameters.AddWithValue("@fy", fy);
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new PartyOpeningAudit
                        {
                            AuditID = Convert.ToInt64(rdr["AuditID"]),
                            ActionType = rdr["ActionType"].ToString(),
                            OldAmount = rdr["OldAmount"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["OldAmount"]),
                            OldDrCr = rdr["OldDrCr"] == DBNull.Value ? null : rdr["OldDrCr"].ToString(),
                            NewAmount = Convert.ToDecimal(rdr["NewAmount"]),
                            NewDrCr = rdr["NewDrCr"].ToString(),
                            Reason = rdr["Reason"] == DBNull.Value ? null : rdr["Reason"].ToString(),
                            ChangedBy = rdr["ChangedBy"].ToString(),
                            ChangedOn = Convert.ToDateTime(rdr["ChangedOn"])
                        });
                    }
                }
            }
            return list;
        }

        // -------------------------------------------------------------
        // Party listing (for dropdowns)
        // -------------------------------------------------------------

        /// <summary>All customers or suppliers, active+inactive, for the opening-balance setter.</summary>
        public List<PartyInfo> ListAllParties(string partyType)
        {
            var list = new List<PartyInfo>();
            string sql;
            if (partyType == "CUS")
            {
                sql = @"SELECT CustomerID AS PartyID, CustomerName AS Name,
                               CustomerCode AS Code, GSTIN, IsActive
                        FROM PK_Customers
                        ORDER BY CustomerName;";
            }
            else
            {
                sql = @"SELECT SupplierID AS PartyID, SupplierName AS Name,
                               SupplierCode AS Code, GSTIN, IsActive
                        FROM MM_Suppliers
                        ORDER BY SupplierName;";
            }

            using (var conn = new MySqlConnection(GetConnectionString()))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new PartyInfo
                        {
                            PartyType = partyType,
                            PartyID = Convert.ToInt32(rdr["PartyID"]),
                            Name = rdr["Name"].ToString(),
                            Code = rdr["Code"] == DBNull.Value ? "" : rdr["Code"].ToString(),
                            GSTIN = rdr["GSTIN"] == DBNull.Value ? "" : rdr["GSTIN"].ToString(),
                            IsActive = rdr["IsActive"] != DBNull.Value && Convert.ToBoolean(rdr["IsActive"])
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Parties that had activity in the given FY OR have any opening balance row.
        /// Used by FINAccountStatement dropdown.
        /// </summary>
        public List<PartyInfo> ListPartiesWithActivity(string partyType, DateTime fyStart, DateTime fyEnd)
        {
            var list = new List<PartyInfo>();
            string sql;

            if (partyType == "CUS")
            {
                sql = @"
                    SELECT c.CustomerID AS PartyID, c.CustomerName AS Name,
                           c.CustomerCode AS Code, c.GSTIN, c.IsActive
                    FROM PK_Customers c
                    WHERE c.CustomerID IN (
                        SELECT DISTINCT CustomerID FROM FIN_SalesInvoice
                         WHERE InvoiceDate BETWEEN @s AND @e
                        UNION
                        SELECT DISTINCT CustomerID FROM FIN_Receipt
                         WHERE ReceiptDate BETWEEN @s AND @e
                        UNION
                        SELECT DISTINCT CAST(SUBSTRING(ContactID,5) AS UNSIGNED) FROM FIN_Journal
                         WHERE ContactID LIKE 'CUS:%' AND JournalDate BETWEEN @s AND @e
                        UNION
                        SELECT DISTINCT PartyID FROM FIN_PartyOpeningBalance
                         WHERE PartyType='CUS' AND Amount <> 0
                    )
                    ORDER BY c.CustomerName;";
            }
            else
            {
                sql = @"
                    SELECT s.SupplierID AS PartyID, s.SupplierName AS Name,
                           s.SupplierCode AS Code, s.GSTIN, s.IsActive
                    FROM MM_Suppliers s
                    WHERE s.SupplierID IN (
                        SELECT DISTINCT SupplierID FROM FIN_PurchaseInvoice
                         WHERE InvoiceDate BETWEEN @s AND @e
                        UNION
                        SELECT DISTINCT CAST(SUBSTRING(ContactID,5) AS UNSIGNED) FROM FIN_Journal
                         WHERE ContactID LIKE 'SUP:%' AND JournalDate BETWEEN @s AND @e
                        UNION
                        SELECT DISTINCT PartyID FROM FIN_PartyOpeningBalance
                         WHERE PartyType='SUP' AND Amount <> 0
                    )
                    ORDER BY s.SupplierName;";
            }

            using (var conn = new MySqlConnection(GetConnectionString()))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@s", fyStart);
                cmd.Parameters.AddWithValue("@e", fyEnd);
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new PartyInfo
                        {
                            PartyType = partyType,
                            PartyID = Convert.ToInt32(rdr["PartyID"]),
                            Name = rdr["Name"].ToString(),
                            Code = rdr["Code"] == DBNull.Value ? "" : rdr["Code"].ToString(),
                            GSTIN = rdr["GSTIN"] == DBNull.Value ? "" : rdr["GSTIN"].ToString(),
                            IsActive = rdr["IsActive"] != DBNull.Value && Convert.ToBoolean(rdr["IsActive"])
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>Fetch one party's basic info.</summary>
        public PartyInfo GetParty(string partyType, int partyID)
        {
            string sql = partyType == "CUS"
                ? @"SELECT CustomerID AS PartyID, CustomerName AS Name, CustomerCode AS Code,
                          GSTIN, IsActive FROM PK_Customers WHERE CustomerID=@id LIMIT 1;"
                : @"SELECT SupplierID AS PartyID, SupplierName AS Name, SupplierCode AS Code,
                          GSTIN, IsActive FROM MM_Suppliers WHERE SupplierID=@id LIMIT 1;";

            using (var conn = new MySqlConnection(GetConnectionString()))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", partyID);
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read()) return null;
                    return new PartyInfo
                    {
                        PartyType = partyType,
                        PartyID = Convert.ToInt32(rdr["PartyID"]),
                        Name = rdr["Name"].ToString(),
                        Code = rdr["Code"] == DBNull.Value ? "" : rdr["Code"].ToString(),
                        GSTIN = rdr["GSTIN"] == DBNull.Value ? "" : rdr["GSTIN"].ToString(),
                        IsActive = rdr["IsActive"] != DBNull.Value && Convert.ToBoolean(rdr["IsActive"])
                    };
                }
            }
        }

        // -------------------------------------------------------------
        // Party Statement (transactions only; caller adds opening + running balance)
        // -------------------------------------------------------------

        /// <summary>
        /// Returns union of all ledger rows for the party in the date range,
        /// ordered by date then voucher number. Running balance NOT computed here.
        /// Debit increases AR (customer owes more); Credit decreases AR.
        /// For suppliers: flip convention — supplier credit = we owe more,
        /// but storing raw Dr/Cr keeps math consistent if caller treats opening DrCr accordingly.
        /// </summary>
        public List<StatementLine> GetPartyStatement(string partyType, int partyID,
                                                     DateTime fromDate, DateTime toDate)
        {
            var list = new List<StatementLine>();
            string contactID = partyType + ":" + partyID;
            string sql;

            if (partyType == "CUS")
            {
                sql = @"
                    SELECT InvoiceDate AS TxnDate, InvoiceNo AS VoucherNo,
                           CONCAT('Sales Invoice', IFNULL(CONCAT(' — ', Narration),'')) AS Particulars,
                           GrandTotal AS Debit, 0 AS Credit,
                           'FIN_SalesInvoice' AS SourceTable
                    FROM FIN_SalesInvoice
                    WHERE CustomerID=@pid AND InvoiceDate BETWEEN @s AND @e

                    UNION ALL

                    SELECT ReceiptDate AS TxnDate, VoucherNo AS VoucherNo,
                           CONCAT('Receipt', IFNULL(CONCAT(' — ', ModeOfPayment),'')) AS Particulars,
                           0 AS Debit, Amount AS Credit,
                           'FIN_Receipt' AS SourceTable
                    FROM FIN_Receipt
                    WHERE CustomerID=@pid AND ReceiptDate BETWEEN @s AND @e

                    UNION ALL

                    SELECT JournalDate AS TxnDate, JournalNo AS VoucherNo,
                           IFNULL(Narration,'Journal') AS Particulars,
                           CASE WHEN DrCr='Dr' THEN Amount ELSE 0 END AS Debit,
                           CASE WHEN DrCr='Cr' THEN Amount ELSE 0 END AS Credit,
                           'FIN_Journal' AS SourceTable
                    FROM FIN_Journal
                    WHERE ContactID=@cid AND JournalDate BETWEEN @s AND @e

                    ORDER BY TxnDate, VoucherNo;";
            }
            else
            {
                // Supplier AP: invoices credit us (we owe), payments debit us (we paid)
                sql = @"
                    SELECT InvoiceDate AS TxnDate, InvoiceNo AS VoucherNo,
                           CONCAT('Purchase Invoice', IFNULL(CONCAT(' — ', Narration),'')) AS Particulars,
                           0 AS Debit, GrandTotal AS Credit,
                           'FIN_PurchaseInvoice' AS SourceTable
                    FROM FIN_PurchaseInvoice
                    WHERE SupplierID=@pid AND InvoiceDate BETWEEN @s AND @e

                    UNION ALL

                    SELECT JournalDate AS TxnDate, JournalNo AS VoucherNo,
                           IFNULL(Narration,'Journal') AS Particulars,
                           CASE WHEN DrCr='Dr' THEN Amount ELSE 0 END AS Debit,
                           CASE WHEN DrCr='Cr' THEN Amount ELSE 0 END AS Credit,
                           'FIN_Journal' AS SourceTable
                    FROM FIN_Journal
                    WHERE ContactID=@cid AND JournalDate BETWEEN @s AND @e

                    ORDER BY TxnDate, VoucherNo;";
            }

            using (var conn = new MySqlConnection(GetConnectionString()))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@pid", partyID);
                cmd.Parameters.AddWithValue("@cid", contactID);
                cmd.Parameters.AddWithValue("@s", fromDate);
                cmd.Parameters.AddWithValue("@e", toDate.Date.AddDays(1).AddSeconds(-1));
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new StatementLine
                        {
                            TxnDate = Convert.ToDateTime(rdr["TxnDate"]),
                            VoucherNo = rdr["VoucherNo"].ToString(),
                            Particulars = rdr["Particulars"].ToString(),
                            Debit = Convert.ToDecimal(rdr["Debit"]),
                            Credit = Convert.ToDecimal(rdr["Credit"]),
                            SourceTable = rdr["SourceTable"].ToString()
                        });
                    }
                }
            }
            return list;
        }
    }
}
