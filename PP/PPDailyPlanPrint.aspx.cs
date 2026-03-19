using System;
using System.Data;
using System.Text;
using System.Web.UI;
using PPApp.DAL;

namespace PPApp
{
    public partial class PPDailyPlanPrint : Page
    {
        // Control declarations — match IDs in PPDailyPlanPrint.aspx exactly
        protected System.Web.UI.WebControls.Literal litTitle;
        protected System.Web.UI.WebControls.Literal litPlanDate;
        protected System.Web.UI.WebControls.Literal litGenerated;
        protected System.Web.UI.WebControls.Literal litStatus;
        protected System.Web.UI.WebControls.Literal litUser;
        protected System.Web.UI.WebControls.Literal litS1Summary;
        protected System.Web.UI.WebControls.Literal litS2Summary;
        protected System.Web.UI.WebControls.Literal litShift1Table;
        protected System.Web.UI.WebControls.Literal litShift2Table;
        protected System.Web.UI.WebControls.Literal litRMTable;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PP_UserID"] == null) { Response.Redirect("PPLogin.aspx"); return; }

            // Read date from query string
            DateTime planDate = PPDatabaseHelper.TodayIST();
            if (!string.IsNullOrEmpty(Request.QueryString["date"]))
                DateTime.TryParse(Request.QueryString["date"], out planDate);

            // Get plan
            DataRow plan = PPDatabaseHelper.GetDailyPlan(planDate);
            if (plan == null)
            {
                // No plan exists for this date
                litTitle.Text     = planDate.ToString("dd MMM yyyy");
                litPlanDate.Text  = planDate.ToString("dddd, dd MMMM yyyy").ToUpper();
                litGenerated.Text = PPDatabaseHelper.NowIST().ToString("dd MMM yyyy, hh:mm tt");
                litStatus.Text    = "<span class='plan-status status-draft'>DRAFT</span>";
                litS1Summary.Text = "No products scheduled.";
                litS2Summary.Text = "No products scheduled.";
                litShift1Table.Text = "<div class='empty-msg'>No products added to Shift 1.</div>";
                litShift2Table.Text = "<div class='empty-msg'>No products added to Shift 2.</div>";
                litRMTable.Text     = "<div class='empty-msg'>No RM requirements calculated.</div>";
                litUser.Text        = Session["PP_FullName"]?.ToString() ?? "";
                return;
            }

            int    planId = Convert.ToInt32(plan["PlanID"]);
            string status = plan["Status"].ToString();

            // Header
            litTitle.Text     = planDate.ToString("dd MMM yyyy");
            litPlanDate.Text  = planDate.ToString("dddd, dd MMMM yyyy").ToUpper();
            litGenerated.Text = PPDatabaseHelper.NowIST().ToString("dd MMM yyyy, hh:mm tt");
            litUser.Text      = Session["PP_FullName"]?.ToString() ?? "";
            litStatus.Text    = status == "Confirmed"
                ? "<span class='plan-status status-confirmed'>CONFIRMED</span>"
                : "<span class='plan-status status-draft'>DRAFT</span>";

            // Load plan rows
            DataTable rows = PPDatabaseHelper.GetDailyPlanRows(planId);
            DataTable s1   = rows.Clone();
            DataTable s2   = rows.Clone();
            foreach (DataRow r in rows.Rows)
            {
                if (Convert.ToInt32(r["Shift"]) == 1) s1.ImportRow(r);
                else s2.ImportRow(r);
            }

            // Build shift tables
            litShift1Table.Text = BuildShiftTable(s1);
            litShift2Table.Text = BuildShiftTable(s2);

            // Shift summaries
            litS1Summary.Text = BuildShiftSummary(s1);
            litS2Summary.Text = BuildShiftSummary(s2);

            // RM table
            DataTable rm = PPDatabaseHelper.GetRMRequirementVsStock(planId);
            litRMTable.Text = BuildRMTable(rm);
        }

        private string BuildShiftTable(DataTable dt)
        {
            if (dt.Rows.Count == 0)
                return "<div class='empty-msg'>No products scheduled for this shift.</div>";

            var sb = new StringBuilder();
            sb.Append("<table>");
            sb.Append("<tr>");
            sb.Append("<th>Product</th>");
            sb.Append("<th class='num'>Qty</th>");
            sb.Append("<th class='num'>Expected Output</th>");
            sb.Append("</tr>");

            foreach (DataRow r in dt.Rows)
            {
                decimal batches   = Convert.ToDecimal(r["Batches"]);
                decimal batchSize = Convert.ToDecimal(r["BatchSize"]);
                decimal output    = batches * batchSize;
                string  prodAbbr  = r["ProdAbbr"].ToString();
                string  outAbbr   = r["OutputAbbr"].ToString();

                sb.Append("<tr>");
                sb.AppendFormat("<td><div class='prod-name'>{0}</div><div class='prod-code'>{1}</div></td>",
                    Encode(r["ProductName"].ToString()),
                    Encode(r["ProductCode"].ToString()));
                sb.AppendFormat("<td class='num'><span class='batch-big'>{0}</span></td>",
                    FormatDecimal(batches));
                sb.AppendFormat("<td class='num'>{0}</td>", Encode(prodAbbr));
                sb.AppendFormat("<td class='num'><span class='output-val'>{0} {1}</span></td>",
                    FormatDecimal(output), Encode(outAbbr));
                sb.Append("</tr>");
            }
            sb.Append("</table>");
            return sb.ToString();
        }

        private string BuildShiftSummary(DataTable dt)
        {
            if (dt.Rows.Count == 0) return "No products scheduled.";
            decimal total = 0;
            foreach (DataRow r in dt.Rows) total += Convert.ToDecimal(r["Batches"]);
            return string.Format("{0} product{1} — {2} total scheduled",
                dt.Rows.Count,
                dt.Rows.Count == 1 ? "" : "s",
                FormatDecimal(total));
        }

        private string BuildRMTable(DataTable dt)
        {
            if (dt.Rows.Count == 0)
                return "<div class='empty-msg'>No RM requirements — add products to shifts first.</div>";

            var sb = new StringBuilder();
            sb.Append("<table>");
            sb.Append("<tr>");
            sb.Append("<th>Raw Material</th>");
            sb.Append("<th class='num'>Required</th>");
            sb.Append("<th class='num'>In Stock</th>");
            sb.Append("<th class='num'>Shortfall / Surplus</th>");
            sb.Append("</tr>");

            foreach (DataRow r in dt.Rows)
            {
                decimal required  = Convert.ToDecimal(r["Required"]);
                decimal inStock   = Convert.ToDecimal(r["InStock"]);
                decimal shortfall = Convert.ToDecimal(r["Shortfall"]);
                string  sfClass   = shortfall > 0 ? "shortfall" : shortfall < 0 ? "surplus" : "";
                string  sfText    = shortfall > 0
                    ? FormatDecimal(shortfall) + " SHORT"
                    : shortfall < 0
                        ? FormatDecimal(Math.Abs(shortfall)) + " surplus"
                        : "OK";

                sb.Append("<tr>");
                sb.AppendFormat("<td><div class='prod-name'>{0}</div><div class='prod-code'>{1}</div></td>",
                    Encode(r["RMName"].ToString()),
                    Encode(r["RMCode"].ToString()));
                string abbr = r["Abbreviation"].ToString();
                sb.AppendFormat("<td class='num'>{0}</td>", FormatQtyWithUOM(required, abbr));
                sb.AppendFormat("<td class='num'>{0}</td>", FormatQtyWithUOM(inStock, abbr));
                string sfFormatted = shortfall > 0
                    ? FormatQtyWithUOM(shortfall, abbr) + " SHORT"
                    : shortfall < 0
                        ? FormatQtyWithUOM(Math.Abs(shortfall), abbr) + " surplus"
                        : "OK";
                sb.AppendFormat("<td class='num {0}'>{1}</td>", sfClass, sfFormatted);
                sb.Append("</tr>");
            }
            sb.Append("</table>");
            return sb.ToString();
        }

        private string FormatDecimal(decimal val)
        {
            return val.ToString("N2").TrimEnd('0').TrimEnd('.');
        }

        private string FormatQtyWithUOM(decimal val, string uom)
        {
            string u = (uom ?? "").Trim().ToLower();
            // Always normalise — no threshold check
            if      (u == "g")  { val = val / 1000m;    u = "kg"; }
            else if (u == "mg") { val = val / 1000000m; u = "kg"; }
            else if (u == "ml") { val = val / 1000m;    u = "l";  }
            return val.ToString("N3").TrimEnd('0').TrimEnd('.') + " " + u.ToUpper();
        }

        private string Encode(string s)
        {
            return System.Web.HttpUtility.HtmlEncode(s);
        }
    }
}
