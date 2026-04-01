using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using StockApp.DAL;

namespace StockApp
{
    public partial class DailySales : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Login.aspx"); return; }

            // Module access check
            string __role = Session["Role"]?.ToString() ?? "";
            if (!DatabaseHelper.RoleHasModuleAccess(__role, "SA", "SA_DAILY_SALES"))
            { Response.Redirect("SAHome.aspx"); return; }

            lblUserInfo.Text   = Session["FullName"] + " (" + Session["Role"] + ")";
            pnlAdminMenu.Visible = Session["Role"]?.ToString() == "Admin";

            if (!IsPostBack)
            {
                LoadStates();
                LoadProducts();
            }
        }

        // ── LOAD DROPDOWNS ────────────────────────────────────────────

        private void LoadStates()
        {
            ddlState.Items.Clear();
            ddlState.Items.Add(new ListItem("— Select State —", "0"));
            var dt = DatabaseHelper.GetStates(90);
            foreach (DataRow r in dt.Rows)
                ddlState.Items.Add(new ListItem(r["StateName"].ToString(), r["StateID"].ToString()));
        }

        protected void ddlState_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlCity.Items.Clear();
            lstDistributor.Items.Clear();
            ddlCity.Items.Add(new ListItem("— Select City —", "0"));

            if (!int.TryParse(ddlState.SelectedValue, out int stateId) || stateId == 0) return;

            var dt = DatabaseHelper.GetCitiesByState(stateId, 90);
            foreach (DataRow r in dt.Rows)
                ddlCity.Items.Add(new ListItem(r["CityName"].ToString(), r["CityID"].ToString()));
        }

        protected void ddlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstDistributor.Items.Clear();

            if (!int.TryParse(ddlCity.SelectedValue, out int cityId) || cityId == 0) return;

            var dt = DatabaseHelper.GetDistributorsByCity(cityId, 90);
            foreach (DataRow r in dt.Rows)
                lstDistributor.Items.Add(new ListItem(r["DistributorName"].ToString(), r["DistributorID"].ToString()));
        }

        // ── LOAD PRODUCTS ─────────────────────────────────────────────

        private void LoadProducts()
        {
            var dt = DatabaseHelper.GetActiveProducts();
            rptProducts.DataSource = dt;
            rptProducts.DataBind();
            pnlSalesEntry.Visible = dt.Rows.Count > 0;
        }

        // ── SAVE ──────────────────────────────────────────────────────

        protected void btnSave_Click(object sender, EventArgs e)
        {
            pnlResult.Visible = false;
            pnlError.Visible  = false;

            // Get selected distributors
            var selectedDists = new List<int>();
            foreach (ListItem item in lstDistributor.Items)
                if (item.Selected)
                    selectedDists.Add(Convert.ToInt32(item.Value));

            if (selectedDists.Count == 0)
            { ShowError("Please select at least one distributor."); return; }

            // Collect product quantities
            var entries = new List<(int productId, int totalQty)>();
            foreach (RepeaterItem item in rptProducts.Items)
            {
                var hfId  = (HiddenField)item.FindControl("hfProductId");
                var txtQty = (TextBox)item.FindControl("txtQty");

                if (!int.TryParse(txtQty.Text, out int qty) || qty <= 0) continue;
                entries.Add((Convert.ToInt32(hfId.Value), qty));
            }

            if (entries.Count == 0)
            { ShowError("Please enter quantity for at least one product."); return; }

            // Save with distributor split
            int userId    = Convert.ToInt32(Session["UserID"]);
            DateTime today = DateTime.Today;
            int n          = selectedDists.Count;
            int totalSaved = 0;

            foreach (var (productId, totalQty) in entries)
            {
                // Split quantities: base = floor(total/n), remainder goes to first dists
                int baseQty    = totalQty / n;
                int remainder  = totalQty % n;

                for (int i = 0; i < n; i++)
                {
                    int qty = baseQty + (i < remainder ? 1 : 0);
                    if (qty <= 0) continue;
                    DatabaseHelper.InsertDailySalesEntry(today, selectedDists[i], productId, qty, null, userId);
                    totalSaved++;
                }
            }

            // Build result message
            string distNames = string.Join(", ", lstDistributor.Items
                .Cast<ListItem>()
                .Where(i => i.Selected)
                .Select(i => i.Text));

            lblResultMsg.Text  = $"Saved {entries.Count} product(s) across {n} distributor(s): {distNames}";
            pnlResult.Visible  = true;

            // Reset form
            ResetForm();
        }

        // ── CANCEL ────────────────────────────────────────────────────

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlResult.Visible = false;
            pnlError.Visible  = false;
            ResetForm();
        }

        // ── HELPERS ───────────────────────────────────────────────────

        private void ResetForm()
        {
            ddlState.SelectedIndex = 0;
            ddlCity.Items.Clear();
            ddlCity.Items.Add(new ListItem("— Select City —", "0"));
            lstDistributor.Items.Clear();

            // Reset all qty inputs to 0
            foreach (RepeaterItem item in rptProducts.Items)
            {
                var txtQty = (TextBox)item.FindControl("txtQty");
                if (txtQty != null) txtQty.Text = "0";
            }
        }

        private void ShowError(string msg)
        {
            lblErrorMsg.Text = msg;
            pnlError.Visible = true;
        }
    }
}
