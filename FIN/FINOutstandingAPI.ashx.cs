using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;
using FINApp.DAL;

namespace FINApp
{
    public class FINOutstandingAPI : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            if (context.Session["FIN_UserID"] == null)
            { context.Response.Write("{\"error\":\"Not authenticated\"}"); context.Response.StatusCode = 401; return; }

            try
            {
                // Get all invoices with customer info
                var invoices = FINDatabaseHelper.ExecuteQueryPublic(
                    "SELECT si.InvoiceID, si.VoucherNo, si.InvoiceDate, si.TotalValue," +
                    " si.CustomerID, IFNULL(c.CustomerName, si.TallyCustomerName) AS CustomerName," +
                    " c.CustomerType, c.City, c.State" +
                    " FROM FIN_SalesInvoice si" +
                    " LEFT JOIN PK_Customers c ON c.CustomerID=si.CustomerID" +
                    " ORDER BY si.CustomerID, si.InvoiceDate, si.InvoiceID;");

                // Get total receipts per customer
                var receipts = FINDatabaseHelper.ExecuteQueryPublic(
                    "SELECT CustomerID, SUM(Amount) AS TotalReceived" +
                    " FROM FIN_Receipt" +
                    " WHERE ReceiptType='CUSTOMER' AND CustomerID IS NOT NULL" +
                    " GROUP BY CustomerID;");

                // Build receipt lookup
                var receiptMap = new Dictionary<int, decimal>();
                foreach (DataRow r in receipts.Rows)
                {
                    int cid = Convert.ToInt32(r["CustomerID"]);
                    receiptMap[cid] = Convert.ToDecimal(r["TotalReceived"]);
                }

                // FIFO allocation: for each customer, allocate receipts to invoices oldest first
                var sb = new StringBuilder("{\"invoices\":[");
                bool first = true;
                int prevCustId = -1;
                decimal remainingReceipt = 0;
                decimal grandTotalInvoiced = 0, grandTotalReceived = 0, grandTotalOutstanding = 0;
                int totalInvoices = 0, outstandingCount = 0;

                foreach (DataRow inv in invoices.Rows)
                {
                    int custId = inv["CustomerID"] != DBNull.Value ? Convert.ToInt32(inv["CustomerID"]) : 0;
                    decimal invAmount = Convert.ToDecimal(inv["TotalValue"]);
                    string custName = inv["CustomerName"] != DBNull.Value ? inv["CustomerName"].ToString() : "Unknown";
                    string custType = inv["CustomerType"] != DBNull.Value ? inv["CustomerType"].ToString() : "";
                    string city = inv["City"] != DBNull.Value ? inv["City"].ToString() : "";
                    string state = inv["State"] != DBNull.Value ? inv["State"].ToString() : "";
                    DateTime invDate = Convert.ToDateTime(inv["InvoiceDate"]);
                    int daysSince = (int)(DateTime.Today - invDate).TotalDays;

                    // Reset receipt balance when customer changes
                    if (custId != prevCustId)
                    {
                        remainingReceipt = custId > 0 && receiptMap.ContainsKey(custId) ? receiptMap[custId] : 0;
                        prevCustId = custId;
                    }

                    // Allocate receipt to this invoice (FIFO)
                    decimal allocated = Math.Min(remainingReceipt, invAmount);
                    remainingReceipt -= allocated;
                    decimal balance = invAmount - allocated;

                    grandTotalInvoiced += invAmount;
                    grandTotalReceived += allocated;
                    grandTotalOutstanding += balance;
                    totalInvoices++;
                    if (balance > 0.01m) outstandingCount++;

                    if (!first) sb.Append(",");
                    sb.AppendFormat("{{\"id\":{0},\"vch\":\"{1}\",\"date\":\"{2:yyyy-MM-dd}\",\"days\":{3}," +
                        "\"custId\":{4},\"custName\":\"{5}\",\"custType\":\"{6}\",\"city\":\"{7}\",\"state\":\"{8}\"," +
                        "\"invoiced\":{9},\"received\":{10},\"balance\":{11}}}",
                        inv["InvoiceID"], Esc(inv["VoucherNo"].ToString()), invDate, daysSince,
                        custId, Esc(custName), custType, Esc(city), Esc(state),
                        D(invAmount), D(allocated), D(balance));
                    first = false;
                }

                sb.AppendFormat("],\"summary\":{{\"totalInvoices\":{0},\"outstandingCount\":{1}," +
                    "\"totalInvoiced\":{2},\"totalReceived\":{3},\"totalOutstanding\":{4}}}}}",
                    totalInvoices, outstandingCount,
                    D(grandTotalInvoiced), D(grandTotalReceived), D(grandTotalOutstanding));

                context.Response.Write(sb.ToString());
            }
            catch (Exception ex)
            {
                context.Response.Write("{\"error\":\"" + Esc(ex.Message) + "\"}");
            }
        }

        private string D(decimal v) => v.ToString("0.00", CultureInfo.InvariantCulture);
        private string Esc(string s) => (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", "");
    }
}
