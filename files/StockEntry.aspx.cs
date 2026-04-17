using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using StockApp.DAL;

namespace StockApp
{
    public partial class StockEntry : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (Session["UserID"] == null) { Response.Redirect("~/Login.aspx"); return; }
            // Module access check
            string __role = Session["Role"]?.ToString() ?? "";
            if (!DatabaseHelper.RoleHasModuleAccess(__role, "SA", "SA_DIST_STOCK"))
            { Response.Redirect("SAHome.aspx"); return; }
            if (!IsPostBack)
            {
                pnlAdminMenu.Visible = (Session["Role"]?.ToString() == "Admin");
                BindStates();
                ResetCities();
                ResetDistributors();
                ResetPanels();
                pnlSuccess.Visible = false;
                pnlError.Visible   = false;
            }
        }

        protected void ddlState_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetCities(); ResetDistributors(); ResetPanels();
            pnlSuccess.Visible = false; pnlError.Visible = false;
            string state = ddlState.SelectedValue;
            if (!string.IsNullOrEmpty(state) && state != "0")
            { BindCities(state); ddlCity.Enabled = true; }
        }

        protected void ddlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetDistributors(); ResetPanels();
            pnlSuccess.Visible = false; pnlError.Visible = false;
            string city = ddlCity.SelectedValue;
            string state = ddlState.SelectedValue;
            if (!string.IsNullOrEmpty(city) && city != "0")
            { BindDistributors(city, state); ddlDistributor.Enabled = true; }
        }

        protected void ddlDistributor_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetPanels();
            pnlSuccess.Visible = false; pnlError.Visible = false;
            if (int.TryParse(ddlDistributor.SelectedValue, out int distributorId) && distributorId > 0)
            {
                ShowDistributorAddress(distributorId);
                ShowClosingStock(distributorId);
                ShowSalesSummary(distributorId);
                ShowCreditSummary(distributorId);
                ShowOrderChart(distributorId);
                ShowPaymentChart(distributorId);
            }
        }

        protected void rblPeriod_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindStates();
            string state = ddlState.SelectedValue;
            if (!string.IsNullOrEmpty(state) && state != "0")
                BindCities(state);

            ResetPanels();
            pnlSuccess.Visible = false;
            pnlError.Visible   = false;
            if (int.TryParse(ddlDistributor.SelectedValue, out int distributorId) && distributorId > 0)
            {
                ShowDistributorAddress(distributorId);
                ShowClosingStock(distributorId);
                ShowSalesSummary(distributorId);
                ShowCreditSummary(distributorId);
                ShowOrderChart(distributorId);
                ShowPaymentChart(distributorId);
            }
        }

        private int GetSelectedDays()
        {
            int days = 30;
            if (rblPeriod != null && int.TryParse(rblPeriod.SelectedValue, out int d))
                days = d;
            return days;
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            pnlSuccess.Visible = false; pnlError.Visible = false;
            try
            {
                if (string.IsNullOrEmpty(ddlState.SelectedValue) || ddlState.SelectedValue == "0")
                    throw new InvalidOperationException("Please select a valid State.");
                if (string.IsNullOrEmpty(ddlCity.SelectedValue) || ddlCity.SelectedValue == "0")
                    throw new InvalidOperationException("Please select a valid City.");
                if (!int.TryParse(ddlDistributor.SelectedValue, out int distributorId) || distributorId == 0)
                    throw new InvalidOperationException("Please select a valid Distributor.");
                if (!int.TryParse(txtCurrentStock.Text.Trim(),  out int currentStock)  || currentStock < 0)
                    throw new InvalidOperationException("Please enter a valid non-negative stock value.");

                int stateId = 0, cityId = 0;
                int.TryParse(ddlState.SelectedValue, out stateId);
                int.TryParse(ddlCity.SelectedValue, out cityId);

                int newId = DatabaseHelper.SaveStockPosition(distributorId, currentStock, stateId, cityId);
                if (newId > 0)
                {
                    lblSuccess.Text = string.Format(
                        "Stock position saved successfully! (Record ID: <strong>{0}</strong> | " +
                        "Distributor: <strong>{1}</strong> | Units: <strong>{2:N0}</strong>)",
                        newId, ddlDistributor.SelectedItem.Text, currentStock);
                    pnlSuccess.Visible = true;
                    ResetForm();
                }
                else throw new Exception("Record was not saved. Please try again.");
            }
            catch (Exception ex)
            {
                lblError.Text = "Error: " + ex.Message;
                pnlError.Visible = true;
            }
        }

        // ── Bind helpers ──────────────────────────────────────────
        private void BindStates()
        {
            var dt = DatabaseHelper.GetStates(GetSelectedDays());
            ddlState.Items.Clear();
            ddlState.Items.Add(new ListItem("— Select State —", "0"));
            foreach (DataRow r in dt.Rows)
            {
                decimal sales = r["TotalSales"] != DBNull.Value ? Convert.ToDecimal(r["TotalSales"]) : 0;
                string  label = r["StateName"].ToString();
                if (sales > 0) label += "  [₹" + FormatLakh(sales) + "]";
                ddlState.Items.Add(new ListItem(label, r["StateID"].ToString()));
            }
        }

        private void BindCities(string state)
        {
            var dt = DatabaseHelper.GetCitiesByState(state, GetSelectedDays());
            ddlCity.Items.Clear();
            ddlCity.Items.Add(new ListItem("— Select City —", "0"));
            foreach (DataRow r in dt.Rows)
            {
                decimal sales = r["TotalSales"] != DBNull.Value ? Convert.ToDecimal(r["TotalSales"]) : 0;
                string  label = r["CityName"].ToString();
                if (sales > 0) label += "  [₹" + FormatLakh(sales) + "]";
                ddlCity.Items.Add(new ListItem(label, r["CityID"].ToString()));
            }
        }

        private void BindDistributors(string city, string state)
        {
            var dt = DatabaseHelper.GetDistributorsByCity(city, state, GetSelectedDays());
            ddlDistributor.Items.Clear();
            ddlDistributor.Items.Add(new ListItem("— Select Distributor —", "0"));
            foreach (DataRow row in dt.Rows)
            {
                string id   = row["DistributorID"].ToString();
                string name = row["DistributorName"].ToString();
                string type = row["CustomerType"] != DBNull.Value ? row["CustomerType"].ToString() : "";
                decimal val = row["RecentValue"] != DBNull.Value ? Convert.ToDecimal(row["RecentValue"]) : 0;
                string typeTag = !string.IsNullOrEmpty(type) ? " [" + type + "]" : "";
                string display = val > 0
                    ? string.Format("{0}{1}  \u20b9{2}", name, typeTag, val.ToString("N0"))
                    : name + typeTag;
                ddlDistributor.Items.Add(new ListItem(display, id));
            }
            ddlDistributor.Enabled = true;
        }

        // ── Address panel ─────────────────────────────────────────
        private void ShowClosingStock(int distributorId)
        {
            pnlClosingStock.Visible = false;
            DataRow row = DatabaseHelper.GetLastStockEntry(distributorId);
            if (row != null)
            {
                DateTime entryDate  = Convert.ToDateTime(row["EntryDate"]);
                int      units      = Convert.ToInt32(row["CurrentStock"]);
                lblClosingDate.Text  = entryDate.ToString("dd MMM yyyy");
                lblClosingUnits.Text = units.ToString("N0");
                pnlClosingStock.Visible = true;
            }
        }

        private void ShowDistributorAddress(int distributorId)
        {
            DataRow row = DatabaseHelper.GetDistributorAddress(distributorId);
            if (row == null) return;
            string address = row["FullAddress"] != DBNull.Value ? row["FullAddress"].ToString().Trim() : "";
            string pin     = row["PinCode"]     != DBNull.Value ? row["PinCode"].ToString().Trim()     : "";
            string distName = row["DistributorName"] != DBNull.Value ? row["DistributorName"].ToString().Trim() : "";

            if (!string.IsNullOrEmpty(address))
            {
                lblAddress.Text    = address;
                pnlAddress.Visible = true;
                if (!string.IsNullOrEmpty(pin)) { lblPin.Text = pin; pnlPin.Visible = true; }
            }

            // Google Maps link — build from distributor name + address + pin
            string mapQuery = string.Join(" ", new[] { distName, address, pin }.Where(s => !string.IsNullOrEmpty(s)));
            if (!string.IsNullOrEmpty(mapQuery))
            {
                lnkGoogleMap.NavigateUrl = "https://www.google.com/maps/search/?api=1&query=" +
                    Uri.EscapeDataString(mapQuery);
                lnkGoogleMap.Visible = true;
            }
        }

        private string FormatLakh(decimal value)
        {
            if (value >= 10000000) return (value / 10000000m).ToString("0.#") + "Cr";
            if (value >= 100000)   return (value / 100000m).ToString("0.#") + "L";
            if (value >= 1000)     return (value / 1000m).ToString("0.#") + "K";
            return value.ToString("N0");
        }

        // ── 60-day sales summary ──────────────────────────────────
        private void ShowSalesSummary(int distributorId)
        {
            DataRow row = DatabaseHelper.GetDistributorSalesSummary(distributorId, GetSelectedDays());
            if (row == null) return;
            lblSummaryOrders.Text   = (row["TotalOrders"]  != DBNull.Value ? Convert.ToInt32(row["TotalOrders"])   : 0).ToString("N0");
            lblSummaryUnits.Text    = (row["TotalUnits"]   != DBNull.Value ? Convert.ToDecimal(row["TotalUnits"])  : 0).ToString("N0");
            lblSummaryValue.Text    = "&#8377;" + (row["TotalValue"] != DBNull.Value ? Convert.ToDecimal(row["TotalValue"]) : 0).ToString("N2");
            lblLastOrder.Text       = row["LastOrderDate"] != DBNull.Value
                                      ? Convert.ToDateTime(row["LastOrderDate"]).ToString("dd MMM yyyy") : "—";
            divSummary.Visible = true;
            if (lblPeriodTitle  != null) lblPeriodTitle.Text  = GetSelectedDays().ToString();
            if (lblChartDays    != null) lblChartDays.Text    = GetSelectedDays().ToString();
            if (lblPaymentDays  != null) lblPaymentDays.Text  = GetSelectedDays().ToString();
            if (lblCreditDays   != null) lblCreditDays.Text   = GetSelectedDays().ToString();
        }

        // ── Credit / Receipt summary ──────────────────────────────
        private void ShowCreditSummary(int distributorId)
        {
            DataRow row = DatabaseHelper.GetDistributorCreditSummary(distributorId, GetSelectedDays());
            if (row == null) return;
            decimal creditTotal = row["CreditTotal"]    != DBNull.Value ? Convert.ToDecimal(row["CreditTotal"])    : 0;
            decimal credit60    = row["Credit60Days"]   != DBNull.Value ? Convert.ToDecimal(row["Credit60Days"])   : 0;
            decimal salesTotal  = row["SalesTotal"]     != DBNull.Value ? Convert.ToDecimal(row["SalesTotal"])     : 0;
            decimal outstanding = salesTotal - creditTotal;
            string  lastPayDate = row["LastPaymentDate"] != DBNull.Value
                                  ? Convert.ToDateTime(row["LastPaymentDate"]).ToString("dd MMM yy") : "—";
            decimal lastPayAmt  = row["LastPaymentAmt"] != DBNull.Value ? Convert.ToDecimal(row["LastPaymentAmt"]) : 0;

            lblCreditTotal.Text     = "&#8377;" + creditTotal.ToString("N0");
            lblCredit60.Text        = "&#8377;" + credit60.ToString("N0");
            lblLastPayment.Text     = lastPayDate + "<br/><small>&#8377;" + lastPayAmt.ToString("N0") + "</small>";
            lblOutstanding.Text     = "&#8377;" + Math.Abs(outstanding).ToString("N0");
            lblOutstanding.CssClass = outstanding <= 0 ? "out-value credit" : "out-value";
        }

        // ── Order history chart ───────────────────────────────────
        private void ShowOrderChart(int distributorId)
        {
            DataTable dt = DatabaseHelper.GetDistributorOrderHistory(distributorId, GetSelectedDays());
            if (dt == null || dt.Rows.Count == 0) return;
            var labels = new StringBuilder();
            var values = new StringBuilder();
            foreach (DataRow row in dt.Rows)
            {
                string date  = Convert.ToDateTime(row["OrderDate"]).ToString("dd MMM");
                string units = Convert.ToDecimal(row["NoOfUnits"]).ToString("0.##");
                if (labels.Length > 0) { labels.Append(","); values.Append(","); }
                labels.Append("\"").Append(date).Append("\"");
                values.Append(units);
            }
            hfChartData.Value      = "{\"labels\":[" + labels + "],\"values\":[" + values + "]}";
            hfShowOrderChart.Value = "1";
        }

        // ── Payment history chart ─────────────────────────────────
        private void ShowPaymentChart(int distributorId)
        {
            DataTable dt = DatabaseHelper.GetDistributorPaymentHistory(distributorId, GetSelectedDays());
            if (dt == null || dt.Rows.Count == 0) return;
            var labels = new StringBuilder();
            var values = new StringBuilder();
            foreach (DataRow row in dt.Rows)
            {
                string date   = Convert.ToDateTime(row["ReceiptDate"]).ToString("dd MMM");
                string amount = Convert.ToDecimal(row["CreditAmount"]).ToString("0.##");
                if (labels.Length > 0) { labels.Append(","); values.Append(","); }
                labels.Append("\"").Append(date).Append("\"");
                values.Append(amount);
            }
            hfPaymentData.Value        = "{\"labels\":[" + labels + "],\"values\":[" + values + "]}";
            hfShowPaymentChart.Value   = "1";
        }

        // ── Reset helpers ─────────────────────────────────────────
        private void ResetCities()
        {
            ddlCity.Items.Clear();
            ddlCity.Items.Add(new ListItem("— Select State First —", "0"));
            ddlCity.Enabled = false;
        }

        private void ResetDistributors()
        {
            ddlDistributor.Items.Clear();
            ddlDistributor.Items.Add(new ListItem("— Select City First —", "0"));
            ddlDistributor.Enabled = false;
        }

        private void ResetPanels()
        {
            pnlAddress.Visible       = false;
            pnlPin.Visible           = false;
            pnlClosingStock.Visible  = false;
            divSummary.Visible       = false;
            lblAddress.Text          = "";
            lblPin.Text              = "";
            // Reset chart flags — JS will hide the divs
            hfChartData.Value        = "";
            hfPaymentData.Value      = "";
            hfShowOrderChart.Value   = "0";
            hfShowPaymentChart.Value = "0";
        }

        private void ResetForm()
        {
            ddlState.SelectedIndex = 0;
            ResetCities(); ResetDistributors(); ResetPanels();
            txtCurrentStock.Text = string.Empty;
        }
    }
}
