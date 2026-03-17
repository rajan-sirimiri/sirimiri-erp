using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using PPApp.DAL;

namespace PPApp
{
    public partial class PPDailyPlan : Page
    {
        private int    UserID   => Convert.ToInt32(Session["PP_UserID"]);
        private string UserRole => Session["PP_Role"]?.ToString() ?? "";

        // Controls declared in ASPX via Inherits — no designer file needed
        protected Label          lblNavUser;
        protected Label          lblPlanDate;
        protected Label          lblPlanStatus;
        protected Label          lblAlert;
        protected Panel          pnlAlert;
        protected HiddenField    hfPlanID;
        protected HiddenField    hfPlanDate;

        // Shift 1
        protected Repeater       rptShift1;
        protected Panel          pnlShift1Empty;
        protected DropDownList   ddlS1Product;
        protected TextBox        txtS1Batches;
        protected Button         btnAddS1;
        protected Label          lblS1Total;

        // Shift 2
        protected Repeater       rptShift2;
        protected Panel          pnlShift2Empty;
        protected DropDownList   ddlS2Product;
        protected TextBox        txtS2Batches;
        protected Button         btnAddS2;
        protected Label          lblS2Total;

        // RM Status
        protected Repeater       rptRM;
        protected Panel          pnlRMEmpty;
        protected Label          lblRMSummary;

        // Action buttons
        protected Button         btnConfirm;
        protected Button         btnDraft;
        protected System.Web.UI.WebControls.HyperLink lnkPDF;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PP_UserID"] == null) { Response.Redirect("PPLogin.aspx"); return; }
            lblNavUser.Text = Session["PP_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
            {
                // Default to today
                DateTime planDate = DateTime.Today;
                if (!string.IsNullOrEmpty(Request.QueryString["date"]))
                    DateTime.TryParse(Request.QueryString["date"], out planDate);

                hfPlanDate.Value = planDate.ToString("yyyy-MM-dd");
                LoadPlanForDate(planDate);
            }
        }

        private void LoadPlanForDate(DateTime planDate)
        {
            lblPlanDate.Text = planDate.ToString("dddd, dd MMM yyyy");

            // Load or create plan
            int planId = PPDatabaseHelper.GetOrCreateDailyPlan(planDate, UserID);
            hfPlanID.Value = planId.ToString();

            // Show status
            DataRow plan = PPDatabaseHelper.GetDailyPlan(planDate);
            string status = plan != null ? plan["Status"].ToString() : "Draft";
            lblPlanStatus.Text    = status;
            lblPlanStatus.CssClass = status == "Confirmed" ? "status-badge confirmed" : "status-badge draft";
            btnConfirm.Visible    = status != "Confirmed";
            btnDraft.Visible      = status == "Confirmed";

            // Load product dropdowns
            LoadProductDropdowns();

            // Load plan rows
            BindShiftRepeaters(planId);

            // Load RM status
            BindRMStatus(planId);

            // Update PDF download link with current date
            if (lnkPDF != null)
                lnkPDF.NavigateUrl = "PPDailyPlanPrint.aspx?date=" + planDate.ToString("yyyy-MM-dd");
        }

        private void LoadProductDropdowns()
        {
            DataTable products = PPDatabaseHelper.GetActiveProducts();

            PopulateProductDropdown(ddlS1Product, products);
            PopulateProductDropdown(ddlS2Product, products);
        }

        private void PopulateProductDropdown(System.Web.UI.WebControls.DropDownList ddl,
            DataTable products)
        {
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem("-- Select Product --", "0"));
            foreach (DataRow row in products.Rows)
            {
                var item = new ListItem(
                    row["ProductName"].ToString(),
                    row["ProductID"].ToString());
                // Embed ProdAbbreviation as data attribute for JS placeholder update
                item.Attributes["data-produom"] = row["ProdAbbreviation"].ToString();
                ddl.Items.Add(item);
            }
        }

        private void BindShiftRepeaters(int planId)
        {
            DataTable rows = PPDatabaseHelper.GetDailyPlanRows(planId);

            // Filter Shift 1
            DataTable s1 = rows.Clone();
            DataTable s2 = rows.Clone();
            foreach (DataRow r in rows.Rows)
            {
                if (Convert.ToInt32(r["Shift"]) == 1) s1.ImportRow(r);
                else s2.ImportRow(r);
            }

            rptShift1.DataSource = s1; rptShift1.DataBind();
            rptShift2.DataSource = s2; rptShift2.DataBind();
            pnlShift1Empty.Visible = s1.Rows.Count == 0;
            pnlShift2Empty.Visible = s2.Rows.Count == 0;

            // Shift totals
            decimal t1 = 0, t2 = 0;
            foreach (DataRow r in s1.Rows) t1 += Convert.ToDecimal(r["Batches"]);
            foreach (DataRow r in s2.Rows) t2 += Convert.ToDecimal(r["Batches"]);
            lblS1Total.Text = t1.ToString("N1") + " scheduled";
            lblS2Total.Text = t2.ToString("N1") + " scheduled";
        }

        private void BindRMStatus(int planId)
        {
            DataTable rm = PPDatabaseHelper.GetRMRequirementVsStock(planId);
            pnlRMEmpty.Visible = rm.Rows.Count == 0;
            rptRM.DataSource   = rm;
            rptRM.DataBind();

            // Summary count
            int shortfall = 0;
            foreach (DataRow r in rm.Rows)
                if (Convert.ToDecimal(r["Shortfall"]) > 0) shortfall++;
            lblRMSummary.Text = rm.Rows.Count + " material" + (rm.Rows.Count == 1 ? "" : "s") +
                (shortfall > 0 ? " — " + shortfall + " shortfall" : " — all sufficient");
            lblRMSummary.CssClass = shortfall > 0 ? "rm-summary warn" : "rm-summary ok";
        }

        // ── ADD ROW SHIFT 1 ──────────────────────────────────────────────────
        protected void btnAddS1_Click(object sender, EventArgs e)
        {
            if (!ValidateRow(ddlS1Product, txtS1Batches, out int prodId, out decimal batches)) return;
            int planId = GetPlanId();
            PPDatabaseHelper.AddDailyPlanRow(planId, 1, prodId, batches);
            ddlS1Product.SelectedIndex = 0;
            txtS1Batches.Text = "";
            RefreshAll(planId);
        }

        // ── ADD ROW SHIFT 2 ──────────────────────────────────────────────────
        protected void btnAddS2_Click(object sender, EventArgs e)
        {
            if (!ValidateRow(ddlS2Product, txtS2Batches, out int prodId, out decimal batches)) return;
            int planId = GetPlanId();
            PPDatabaseHelper.AddDailyPlanRow(planId, 2, prodId, batches);
            ddlS2Product.SelectedIndex = 0;
            txtS2Batches.Text = "";
            RefreshAll(planId);
        }

        // ── DELETE ROW ───────────────────────────────────────────────────────
        protected void rptShift1_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Delete")
            {
                PPDatabaseHelper.DeleteDailyPlanRow(Convert.ToInt32(e.CommandArgument));
                RefreshAll(GetPlanId());
            }
        }

        protected void rptShift2_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Delete")
            {
                PPDatabaseHelper.DeleteDailyPlanRow(Convert.ToInt32(e.CommandArgument));
                RefreshAll(GetPlanId());
            }
        }

        // ── CONFIRM / DRAFT ──────────────────────────────────────────────────
        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            int planId = GetPlanId();
            PPDatabaseHelper.SetDailyPlanStatus(planId, "Confirmed");
            ShowAlert("Plan confirmed successfully.", true);
            DateTime planDate;
            DateTime.TryParse(hfPlanDate.Value, out planDate);
            LoadPlanForDate(planDate);
        }

        protected void btnDraft_Click(object sender, EventArgs e)
        {
            int planId = GetPlanId();
            PPDatabaseHelper.SetDailyPlanStatus(planId, "Draft");
            ShowAlert("Plan moved back to Draft.", true);
            DateTime planDate;
            DateTime.TryParse(hfPlanDate.Value, out planDate);
            LoadPlanForDate(planDate);
        }

        // ── DATE NAVIGATION ──────────────────────────────────────────────────
        protected void btnPrevDay_Click(object sender, EventArgs e) => NavigateDay(-1);
        protected void btnNextDay_Click(object sender, EventArgs e) => NavigateDay(1);
        protected void btnToday_Click(object sender, EventArgs e)
        {
            hfPlanDate.Value = DateTime.Today.ToString("yyyy-MM-dd");
            LoadPlanForDate(DateTime.Today);
        }

        private void NavigateDay(int delta)
        {
            DateTime current;
            DateTime.TryParse(hfPlanDate.Value, out current);
            DateTime next = current.AddDays(delta);
            hfPlanDate.Value = next.ToString("yyyy-MM-dd");
            LoadPlanForDate(next);
        }

        // ── HELPERS ──────────────────────────────────────────────────────────
        private int GetPlanId() => Convert.ToInt32(hfPlanID.Value);

        private bool ValidateRow(DropDownList ddl, TextBox txtBatches,
            out int productId, out decimal batches)
        {
            productId = 0; batches = 0;
            if (ddl.SelectedValue == "0")
            { ShowAlert("Please select a product.", false); return false; }
            if (!decimal.TryParse(txtBatches.Text.Trim(), out batches) || batches <= 0)
            { ShowAlert("Please enter a valid number of batches.", false); return false; }
            productId = Convert.ToInt32(ddl.SelectedValue);
            return true;
        }

        private void RefreshAll(int planId)
        {
            LoadProductDropdowns();
            BindShiftRepeaters(planId);
            BindRMStatus(planId);
            pnlAlert.Visible = false;
        }

        private void ShowAlert(string msg, bool success)
        {
            lblAlert.Text     = msg;
            pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger");
            pnlAlert.Visible  = true;
        }

        // Used in ASPX repeater binding
        protected string FormatDecimal(object val)
        {
            decimal d;
            if (val == null || !decimal.TryParse(val.ToString(), out d)) return "0";
            return d.ToString("N2").TrimEnd('0').TrimEnd('.');
        }

        protected string ShortfallClass(object val)
        {
            decimal d;
            if (val == null || !decimal.TryParse(val.ToString(), out d)) return "";
            return d > 0 ? "shortfall" : d < 0 ? "surplus" : "";
        }

        protected string ShortfallDisplay(object val)
        {
            decimal d;
            if (val == null || !decimal.TryParse(val.ToString(), out d)) return "—";
            if (d > 0) return FormatDecimal(d) + " SHORT";
            if (d < 0) return FormatDecimal(Math.Abs(d)) + " surplus";
            return "OK";
        }
    }
}
