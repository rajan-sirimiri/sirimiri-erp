using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PPApp
{
    public partial class PPBatchCostReport : Page
    {
        protected Label lblNavUser;
        protected TextBox txtDateFrom, txtDateTo;
        protected DropDownList ddlProduct;
        protected Button btnApply;
        protected Panel pnlAlert, pnlSummary, pnlReport, pnlEmpty;
        protected Label lblAlert, lblTotalBatches, lblTotalProducts, lblBOMCost, lblActualCost;
        protected Literal litTable;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PP_UserID"] == null) { Response.Redirect("PPLogin.aspx"); return; }
            lblNavUser.Text = Session["PP_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
            {
                // Default: last 30 days
                txtDateFrom.Text = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
                txtDateTo.Text = DateTime.Today.ToString("yyyy-MM-dd");
                LoadProductDropdown();
            }
        }

        private void LoadProductDropdown()
        {
            DataTable dt = PPDatabaseHelper.GetAllProducts();
            ddlProduct.Items.Clear();
            ddlProduct.Items.Add(new ListItem("All Products", "0"));
            if (dt != null)
            {
                foreach (DataRow r in dt.Rows)
                {
                    if (Convert.ToBoolean(r["IsActive"]))
                        ddlProduct.Items.Add(new ListItem(r["ProductName"].ToString(), r["ProductID"].ToString()));
                }
            }
        }

        protected void btnApply_Click(object sender, EventArgs e)
        {
            DateTime dateFrom, dateTo;
            if (!DateTime.TryParse(txtDateFrom.Text, out dateFrom) || !DateTime.TryParse(txtDateTo.Text, out dateTo))
            {
                ShowAlert("Please select valid dates.", false);
                return;
            }

            int productId = Convert.ToInt32(ddlProduct.SelectedValue);
            DataTable batches = PPDatabaseHelper.GetBatchCostReport(dateFrom, dateTo, productId);

            if (batches == null || batches.Rows.Count == 0)
            {
                pnlEmpty.Visible = true;
                pnlSummary.Visible = false;
                pnlReport.Visible = false;
                ShowAlert("No completed batches found for the selected period.", false);
                return;
            }

            // Build the report
            BuildReport(batches, dateFrom, dateTo, productId);
        }

        private void BuildReport(DataTable batches, DateTime dateFrom, DateTime dateTo, int productId)
        {
            // Group by OrderID + BatchNo to get unique batches
            // Each row in batches has: OrderID, BatchNo, ProductName, ProductCode, OrderDate,
            //   MaterialName, MaterialCode, MaterialType, BOMQtyPerBatch, BOMUom,
            //   ActualConsumed, ActualUom, UnitRate, BOMCost, ActualCost

            var sb = new StringBuilder();
            sb.Append("<table><thead><tr>");
            sb.Append("<th>Batch</th>");
            sb.Append("<th>Product</th>");
            sb.Append("<th>Date</th>");
            sb.Append("<th>Material</th>");
            sb.Append("<th>Type</th>");
            sb.Append("<th class='num'>BOM Qty</th>");
            sb.Append("<th>UOM</th>");
            sb.Append("<th class='num'>Actual Consumed</th>");
            sb.Append("<th>UOM</th>");
            sb.Append("<th class='num'>Unit Rate</th>");
            sb.Append("<th class='num'>BOM Cost</th>");
            sb.Append("<th class='num'>Actual Cost</th>");
            sb.Append("<th class='num'>Variance</th>");
            sb.Append("</tr></thead><tbody>");

            string prevBatchKey = "";
            decimal grandBOMCost = 0, grandActualCost = 0;
            decimal batchBOMCost = 0, batchActualCost = 0;
            int totalBatches = 0;
            var products = new System.Collections.Generic.HashSet<string>();

            for (int i = 0; i < batches.Rows.Count; i++)
            {
                DataRow r = batches.Rows[i];
                string batchKey = r["OrderID"].ToString() + "-" + r["BatchNo"].ToString();
                string productName = r["ProductName"].ToString();
                string orderDate = r["OrderDate"] != DBNull.Value ? Convert.ToDateTime(r["OrderDate"]).ToString("dd-MMM-yy") : "";

                // New batch header
                if (batchKey != prevBatchKey)
                {
                    // Close previous batch total
                    if (prevBatchKey != "")
                    {
                        sb.AppendFormat("<tr class='batch-total'><td colspan='10' style='text-align:right;'>Batch Total</td>" +
                            "<td class='num'>{0}</td><td class='num'>{1}</td><td class='num' style='color:{4};'>{2}{3}</td></tr>",
                            FmtCurrency(batchBOMCost), FmtCurrency(batchActualCost),
                            batchActualCost - batchBOMCost >= 0 ? "+" : "",
                            FmtCurrency(batchActualCost - batchBOMCost),
                            batchActualCost > batchBOMCost ? "#e74c3c" : "#2ecc71");
                        grandBOMCost += batchBOMCost;
                        grandActualCost += batchActualCost;
                    }

                    totalBatches++;
                    products.Add(productName);
                    batchBOMCost = 0;
                    batchActualCost = 0;

                    sb.AppendFormat("<tr class='batch-header'><td class='bold'>B{0}</td><td class='bold'>{1}</td><td>{2}</td>" +
                        "<td colspan='10'></td></tr>",
                        r["BatchNo"], Esc(productName), orderDate);
                    prevBatchKey = batchKey;
                }

                // Material row
                decimal bomQty = r["BOMQtyPerBatch"] != DBNull.Value ? Convert.ToDecimal(r["BOMQtyPerBatch"]) : 0;
                decimal actualQty = r["ActualConsumed"] != DBNull.Value ? Convert.ToDecimal(r["ActualConsumed"]) : 0;
                decimal unitRate = r["UnitRate"] != DBNull.Value ? Convert.ToDecimal(r["UnitRate"]) : 0;
                decimal bomCost = r["BOMCost"] != DBNull.Value ? Convert.ToDecimal(r["BOMCost"]) : 0;
                decimal actualCost = r["ActualCost"] != DBNull.Value ? Convert.ToDecimal(r["ActualCost"]) : 0;
                decimal variance = actualCost - bomCost;
                string bomUom = r["BOMUom"] != DBNull.Value ? r["BOMUom"].ToString() : "";
                string actualUom = r["ActualUom"] != DBNull.Value ? r["ActualUom"].ToString() : bomUom;

                batchBOMCost += bomCost;
                batchActualCost += actualCost;

                sb.AppendFormat("<tr><td></td><td></td><td></td>" +
                    "<td>{0}</td><td>{1}</td>" +
                    "<td class='num'>{2}</td><td>{3}</td>" +
                    "<td class='num'>{4}</td><td>{5}</td>" +
                    "<td class='num'>{6}</td><td class='num'>{7}</td><td class='num'>{8}</td>" +
                    "<td class='num' style='color:{11};'>{9}{10}</td></tr>",
                    Esc(r["MaterialName"].ToString()), r["MaterialType"],
                    FmtQty(bomQty), bomUom,
                    FmtQty(actualQty), actualUom,
                    FmtCurrency(unitRate), FmtCurrency(bomCost), FmtCurrency(actualCost),
                    variance >= 0 ? "+" : "", FmtCurrency(variance),
                    variance > 0 ? "#e74c3c" : "#2ecc71");
            }

            // Last batch total
            if (prevBatchKey != "")
            {
                sb.AppendFormat("<tr class='batch-total'><td colspan='10' style='text-align:right;'>Batch Total</td>" +
                    "<td class='num'>{0}</td><td class='num'>{1}</td><td class='num' style='color:{4};'>{2}{3}</td></tr>",
                    FmtCurrency(batchBOMCost), FmtCurrency(batchActualCost),
                    batchActualCost - batchBOMCost >= 0 ? "+" : "",
                    FmtCurrency(batchActualCost - batchBOMCost),
                    batchActualCost > batchBOMCost ? "#e74c3c" : "#2ecc71");
                grandBOMCost += batchBOMCost;
                grandActualCost += batchActualCost;
            }

            // Grand total
            decimal grandVariance = grandActualCost - grandBOMCost;
            sb.AppendFormat("<tr style='background:#e8e5e0;font-weight:700;'><td colspan='10' style='text-align:right;font-family:\"Bebas Neue\",sans-serif;font-size:14px;letter-spacing:.06em;'>Grand Total</td>" +
                "<td class='num' style='font-size:12px;'>{0}</td><td class='num' style='font-size:12px;'>{1}</td>" +
                "<td class='num' style='font-size:12px;color:{4};'>{2}{3}</td></tr>",
                FmtCurrency(grandBOMCost), FmtCurrency(grandActualCost),
                grandVariance >= 0 ? "+" : "", FmtCurrency(grandVariance),
                grandVariance > 0 ? "#e74c3c" : "#2ecc71");

            sb.Append("</tbody></table>");
            litTable.Text = sb.ToString();

            // Summary cards
            lblTotalBatches.Text = totalBatches.ToString();
            lblTotalProducts.Text = products.Count.ToString();
            lblBOMCost.Text = FmtCurrencyShort(grandBOMCost);
            lblActualCost.Text = FmtCurrencyShort(grandActualCost);

            pnlSummary.Visible = true;
            pnlReport.Visible = true;
            pnlEmpty.Visible = false;
            pnlAlert.Visible = false;
        }

        private string FmtQty(decimal v) => v == 0 ? "—" : v.ToString("0.###", CultureInfo.InvariantCulture);

        private string FmtCurrency(decimal v) => v == 0 ? "—" : "₹" + v.ToString("#,##0.00", new CultureInfo("en-IN"));

        private string FmtCurrencyShort(decimal v)
        {
            if (v >= 1e7m) return "₹" + (v / 1e7m).ToString("0.#") + "Cr";
            if (v >= 1e5m) return "₹" + (v / 1e5m).ToString("0.#") + "L";
            if (v >= 1e3m) return "₹" + (v / 1e3m).ToString("0.#") + "K";
            return "₹" + Math.Round(v).ToString("#,##0", new CultureInfo("en-IN"));
        }

        private string Esc(string s) => System.Web.HttpUtility.HtmlEncode(s ?? "");

        private void ShowAlert(string msg, bool success)
        {
            string bg = success ? "#d1f5e0" : "#fdf3f2";
            string color = success ? "#155724" : "#842029";
            string border = success ? "#a3d9b1" : "#f5c2c7";
            string icon = success ? "&#10003;" : "&#9888;";
            lblAlert.Text = string.Format(
                "<div style='background:{0};color:{1};border:1px solid {2};padding:12px 18px;border-radius:8px;font-size:13px;'>" +
                "<strong>{3}</strong> {4}</div>", bg, color, border, icon, msg);
            pnlAlert.Visible = true;
        }
    }
}
