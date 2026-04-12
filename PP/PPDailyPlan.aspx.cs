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

            // Module access check
            string __role = Session["PP_Role"]?.ToString() ?? "";
            if (!PPDatabaseHelper.RoleHasModuleAccess(__role, "PP", "PP_DAILY_PLAN"))
            { Response.Redirect("PPHome.aspx"); return; }
            lblNavUser.Text = Session["PP_FullName"]?.ToString() ?? "";

            if (!IsPostBack)
            {
                // Default to today
                DateTime planDate = PPDatabaseHelper.TodayIST();
                if (!string.IsNullOrEmpty(Request.QueryString["date"]))
                    DateTime.TryParse(Request.QueryString["date"], out planDate);

                // Restrict: no past dates, max 30 days in future
                DateTime today = PPDatabaseHelper.TodayIST();
                if (planDate.Date < today.Date) planDate = today;
                if (planDate.Date > today.AddDays(30).Date) planDate = today.AddDays(30);

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
            {
                lnkPDF.NavigateUrl = "PPDailyPlanPrint.aspx?date=" + planDate.ToString("yyyy-MM-dd");
                lnkPDF.Visible = UserRole == "SuperAdmin";
            }
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

            // Filter to only show materials with shortfall (Required > InStock)
            int totalMaterials = rm.Rows.Count;
            DataTable shortItems = rm.Clone(); // same structure, no rows
            int shortfall = 0;
            foreach (DataRow r in rm.Rows)
            {
                if (Convert.ToDecimal(r["Shortfall"]) > 0)
                {
                    shortItems.ImportRow(r);
                    shortfall++;
                }
            }

            pnlRMEmpty.Visible = shortItems.Rows.Count == 0 && totalMaterials > 0 ? false : shortItems.Rows.Count == 0;
            rptRM.DataSource   = shortItems;
            rptRM.DataBind();

            // Summary count
            if (totalMaterials == 0)
            {
                lblRMSummary.Text = "Add products to see RM requirements";
                lblRMSummary.CssClass = "rm-summary";
            }
            else if (shortfall > 0)
            {
                lblRMSummary.Text = shortfall + " material" + (shortfall == 1 ? "" : "s") + " short — " + (totalMaterials - shortfall) + " sufficient";
                lblRMSummary.CssClass = "rm-summary warn";
            }
            else
            {
                lblRMSummary.Text = "All " + totalMaterials + " materials sufficient";
                lblRMSummary.CssClass = "rm-summary ok";
                pnlRMEmpty.Visible = false;
            }
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
            if (e.CommandName == "Delete") DeletePlanRow(e.CommandArgument);
        }

        protected void rptShift2_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Delete") DeletePlanRow(e.CommandArgument);
        }

        private void DeletePlanRow(object rowIdObj)
        {
            int rowId = Convert.ToInt32(rowIdObj);

            // Block deletion if production order is Initiated or beyond
            string orderStatus = PPDatabaseHelper.GetPlanRowOrderStatus(rowId);
            if (orderStatus != null && orderStatus != "Pending")
            {
                RefreshAll(GetPlanId(), clearAlert: false);
                ShowAlert("This product cannot be removed from the plan because its Production Order is currently <strong>" +
                    orderStatus + "</strong>. To make changes, please go to the Production Order screen and cancel the order first.", false);
                return;
            }

            PPDatabaseHelper.DeleteDailyPlanRow(rowId);
            RefreshAll(GetPlanId());
        }

        // ── CONFIRM / DRAFT ──────────────────────────────────────────────────
        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            int planId = GetPlanId();
            // Block confirm if no products scheduled
            DataTable rows = PPDatabaseHelper.GetDailyPlanRows(planId);
            if (rows == null || rows.Rows.Count == 0)
            {
                ShowAlert("Cannot confirm an empty plan. Please add at least one product.", false);
                return;
            }
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
            hfPlanDate.Value = PPDatabaseHelper.TodayIST().ToString("yyyy-MM-dd");
            LoadPlanForDate(PPDatabaseHelper.TodayIST());
        }

        private void NavigateDay(int delta)
        {
            DateTime current;
            DateTime.TryParse(hfPlanDate.Value, out current);
            DateTime next = current.AddDays(delta);

            // Restrict: no past dates, max 30 days in future
            DateTime today = PPDatabaseHelper.TodayIST();
            if (next.Date < today.Date) next = today;
            if (next.Date > today.AddDays(30).Date) next = today.AddDays(30);

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

        private void RefreshAll(int planId, bool clearAlert = true)
        {
            // Any change to the plan reverts it to Draft
            DataRow plan = PPDatabaseHelper.GetDailyPlan(DateTime.Parse(hfPlanDate.Value));
            string status = plan != null ? plan["Status"].ToString() : "Draft";
            if (status == "Confirmed")
            {
                PPDatabaseHelper.SetDailyPlanStatus(planId, "Draft");
                status = "Draft";
            }
            lblPlanStatus.Text = status;
            lblPlanStatus.CssClass = "status-badge draft";
            btnConfirm.Visible = true;
            btnDraft.Visible = false;

            LoadProductDropdowns();
            BindShiftRepeaters(planId);
            BindRMStatus(planId);
            if (clearAlert) pnlAlert.Visible = false;
        }

        private void ShowAlert(string msg, bool success)
        {
            string icon   = success ? "&#10003;" : "&#9888;";
            string bg     = success ? "#d1f5e0" : "#fdf3f2";
            string color  = success ? "#155724" : "#842029";
            string border = success ? "#a3d9b1" : "#f5c2c7";

            lblAlert.Text = string.Format(
                "<div style='display:flex;align-items:flex-start;gap:10px;padding:12px 16px;" +
                "border-radius:10px;font-size:13px;line-height:1.6;" +
                "background:{0};color:{1};border:1px solid {2};'>" +
                "<span style='font-size:16px;flex-shrink:0;margin-top:1px;'>{3}</span>" +
                "<span>{4}</span></div>",
                bg, color, border, icon, msg);

            pnlAlert.Visible = true;
        }

        // Used in ASPX repeater binding
        protected string FormatDecimal(object val)
        {
            decimal d;
            if (val == null) return "0";
            if (!decimal.TryParse(val.ToString(),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out d)) return "0";
            return d.ToString("N2", System.Globalization.CultureInfo.InvariantCulture)
                    .TrimEnd('0').TrimEnd('.');
        }

        // Normalise quantity to canonical unit ALWAYS (regardless of value size)
        // Required and InStock both come from DB already in RM master UOM
        // So we just display with the canonical version of that UOM label
        protected string FormatQtyWithUOM(object val, object uomObj)
        {
            if (val == null || val == DBNull.Value) return "0";
            decimal d;
            try { d = Convert.ToDecimal(val); }
            catch { return "0"; }
            string uom = uomObj?.ToString().Trim().ToLower() ?? "";

            // Always normalise to canonical unit
            if      (uom == "g")  { d = d / 1000m;    uom = "kg"; }
            else if (uom == "mg") { d = d / 1000000m; uom = "kg"; }
            else if (uom == "ml") { d = d / 1000m;    uom = "l";  }

            return d.ToString("0.###") + " " + uom.ToUpper();
        }

        protected string ShortfallClass(object val)
        {
            if (val == null || val == DBNull.Value) return "";
            try { decimal d = Convert.ToDecimal(val); return d > 0 ? "shortfall" : d < 0 ? "surplus" : ""; }
            catch { return ""; }
        }

        protected string ShortfallDisplay(object val, object uomObj)
        {
            if (val == null || val == DBNull.Value) return "—";
            decimal d;
            try { d = Convert.ToDecimal(val); }
            catch { return "—"; }
            if (d > 0) return FormatQtyWithUOM(d, uomObj) + " SHORT";
            if (d < 0) return FormatQtyWithUOM(Math.Abs(d), uomObj) + " surplus";
            return "OK";
        }
    }
}
