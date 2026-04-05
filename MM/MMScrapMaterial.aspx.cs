using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using MMApp.DAL;

namespace MMApp
{
    public partial class MMScrapMaterial : Page
    {
        protected Label        lblNavUser;
        protected Panel        pnlAlert;
        protected Label        lblAlert;
        protected HiddenField  hfScrapID;
        protected TextBox      txtCode;
        protected TextBox      txtName;
        protected TextBox      txtDesc;
        protected DropDownList ddlUOM;
        protected Button       btnSave;
        protected Button       btnClear;
        protected Button       btnToggle;
        protected Label        lblCount;
        protected Panel        pnlEmpty;
        protected Repeater     rptList;
        protected Panel        pnlPriceHistory, pnlHistoryEmpty;
        protected Repeater     rptPriceHistory;
        protected Label        lblHistoryScrapName;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["MM_UserID"] == null) { Response.Redirect("MMLogin.aspx"); return; }

            // Module access check
            string __role = Session["MM_Role"]?.ToString() ?? "";
            if (!MMDatabaseHelper.RoleHasModuleAccess(__role, "MM", "MM_SCRAP_MASTER"))
            { Response.Redirect("MMHome.aspx"); return; }
            lblNavUser.Text = Session["MM_FullName"] as string ?? "";
            if (!IsPostBack)
            {
                BindUOM();
                BindList();
            }
        }

        private void BindUOM()
        {
            var dt = MMDatabaseHelper.GetActiveUOM();
            ddlUOM.Items.Clear();
            ddlUOM.Items.Add(new ListItem("-- Select UOM --", "0"));
            foreach (DataRow row in dt.Rows)
                ddlUOM.Items.Add(new ListItem(
                    row["UOMName"].ToString() + " (" + row["Abbreviation"].ToString() + ")",
                    row["UOMID"].ToString()));
        }

        private void BindList()
        {
            var dt = MMDatabaseHelper.GetAllScrapMaterials();
            pnlEmpty.Visible = dt.Rows.Count == 0;
            rptList.DataSource = dt;
            rptList.DataBind();
            lblCount.Text = " — " + dt.Rows.Count + " item" + (dt.Rows.Count == 1 ? "" : "s");
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name)) { ShowAlert("Scrap name is required.", false); return; }
            int uomId = Convert.ToInt32(ddlUOM.SelectedValue);
            if (uomId == 0) { ShowAlert("Please select a UOM.", false); return; }
            string desc = txtDesc.Text.Trim();

            int scrapId = Convert.ToInt32(hfScrapID.Value);
            if (scrapId == 0)
            {
                MMDatabaseHelper.AddScrapMaterial(name, desc, uomId);
                ShowAlert("Scrap material '" + name + "' added successfully.", true);
            }
            else
            {
                string code = txtCode.Text.Trim();
                MMDatabaseHelper.UpdateScrapMaterial(scrapId, code, name, desc, uomId);
                ShowAlert("Scrap material updated successfully.", true);
            }
            ClearForm();
            BindList();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            pnlAlert.Visible = false;
        }

        protected void btnToggle_Click(object sender, EventArgs e)
        {
            int scrapId = Convert.ToInt32(hfScrapID.Value);
            if (scrapId == 0) return;
            bool current = btnToggle.Text == "Deactivate";
            MMDatabaseHelper.ToggleScrapMaterialActive(scrapId, !current);
            ShowAlert("Scrap material " + (!current ? "activated" : "deactivated") + ".", true);
            ClearForm();
            BindList();
        }

        protected void rptList_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Edit")
            {
                int scrapId = Convert.ToInt32(e.CommandArgument);
                var row = MMDatabaseHelper.GetScrapMaterialById(scrapId);
                if (row == null) return;
                hfScrapID.Value = scrapId.ToString();
                txtCode.Text    = row["ScrapCode"].ToString();
                txtName.Text    = row["ScrapName"].ToString();
                txtDesc.Text    = row["Description"] == DBNull.Value ? "" : row["Description"].ToString();
                try { ddlUOM.SelectedValue = row["UOMID"].ToString(); } catch { }
                btnToggle.Text    = Convert.ToBoolean(row["IsActive"]) ? "Deactivate" : "Activate";
                btnToggle.Visible = true;
                pnlAlert.Visible  = false;
                if (pnlPriceHistory != null) pnlPriceHistory.Visible = false;
                BindUOM();
                try { ddlUOM.SelectedValue = row["UOMID"].ToString(); } catch { }
            }
            else if (e.CommandName == "SetPrice")
            {
                int scrapId = Convert.ToInt32(e.CommandArgument);
                var txtNewPrice = e.Item.FindControl("txtNewPrice") as TextBox;
                if (txtNewPrice == null || string.IsNullOrEmpty(txtNewPrice.Text.Trim()))
                { ShowAlert("Please enter a price.", false); return; }
                decimal price;
                if (!decimal.TryParse(txtNewPrice.Text.Trim(), out price) || price < 0)
                { ShowAlert("Please enter a valid price.", false); return; }

                int userId = Convert.ToInt32(Session["MM_UserID"]);
                MMDatabaseHelper.SetScrapPrice(scrapId, price, userId);
                ShowAlert("Price updated to ₹" + price.ToString("0.00"), true);
                BindList();
            }
            else if (e.CommandName == "PriceHistory")
            {
                int scrapId = Convert.ToInt32(e.CommandArgument);
                var scrap = MMDatabaseHelper.GetScrapMaterialById(scrapId);
                if (lblHistoryScrapName != null)
                    lblHistoryScrapName.Text = scrap != null ? scrap["ScrapName"].ToString() : "";
                var history = MMDatabaseHelper.GetScrapPriceHistory(scrapId);
                if (rptPriceHistory != null) { rptPriceHistory.DataSource = history; rptPriceHistory.DataBind(); }
                if (pnlHistoryEmpty != null) pnlHistoryEmpty.Visible = history.Rows.Count == 0;
                if (pnlPriceHistory != null) pnlPriceHistory.Visible = true;
            }
        }

        private void ClearForm()
        {
            hfScrapID.Value   = "0";
            txtCode.Text      = "";
            txtName.Text      = "";
            txtDesc.Text      = "";
            ddlUOM.SelectedIndex = 0;
            btnToggle.Visible = false;
        }

        private void ShowAlert(string msg, bool success)
        {
            lblAlert.Text     = msg;
            pnlAlert.CssClass = "alert " + (success ? "alert-success" : "alert-danger");
            pnlAlert.Visible  = true;
        }
    }
}
