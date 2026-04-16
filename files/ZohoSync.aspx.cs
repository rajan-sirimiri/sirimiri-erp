using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using StockApp.DAL;

namespace StockApp
{
    public partial class ZohoSync : Page
    {
        protected HiddenField hfActiveTab;
        protected Panel pnlAlert;
        protected Label lblAlert;
        protected Label lblConnStatus, lblOrgId, lblDomain, lblRefreshToken, lblTokenExpiry;
        protected Literal litConnClass;
        protected Button btnTestConnection, btnSyncAllProducts, btnSyncAllCustomers, btnSyncAllSuppliers;
        protected Repeater rptProducts, rptCustomers, rptSuppliers, rptLog;
        protected Panel pnlProductList;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null) { Response.Redirect("Login.aspx"); return; }

            // Only Super role can access Zoho integration
            string role = Session["RoleCode"]?.ToString() ?? "";
            if (role != "Super") { Response.Redirect("ERPHome.aspx"); return; }

            if (!IsPostBack)
            {
                LoadConnectionInfo();
                LoadProducts();
                LoadCustomers();
                LoadSuppliers();
                LoadLog();
            }
        }

        // ── CONNECTION ──

        private void LoadConnectionInfo()
        {
            try
            {
                lblOrgId.Text = ZohoHelper.GetOrgId();
                lblDomain.Text = ZohoHelper.GetApiDomain();

                // Show masked refresh token
                var dt = new DataTable();
                using (var conn = new MySql.Data.MySqlClient.MySqlConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["StockDB"].ConnectionString))
                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(
                    "SELECT RefreshToken, AccessTokenExpiry FROM Zoho_Config WHERE ConfigID=1;", conn))
                {
                    conn.Open(); dt.Load(cmd.ExecuteReader());
                }
                if (dt.Rows.Count > 0)
                {
                    string rt = dt.Rows[0]["RefreshToken"]?.ToString() ?? "";
                    lblRefreshToken.Text = rt.Length > 20 ? rt.Substring(0, 10) + "..." + rt.Substring(rt.Length - 6) : (string.IsNullOrEmpty(rt) ? "NOT SET" : rt);

                    if (dt.Rows[0]["AccessTokenExpiry"] != DBNull.Value)
                    {
                        DateTime exp = Convert.ToDateTime(dt.Rows[0]["AccessTokenExpiry"]);
                        lblTokenExpiry.Text = exp.ToString("dd-MMM-yyyy HH:mm:ss") + " UTC";
                    }
                    else
                    {
                        lblTokenExpiry.Text = "Not yet generated";
                    }
                }
            }
            catch (Exception ex)
            {
                lblOrgId.Text = "Error: " + ex.Message;
            }
        }

        protected void btnTestConnection_Click(object sender, EventArgs e)
        {
            hfActiveTab.Value = "connection";
            try
            {
                string result = ZohoHelper.TestConnection();
                if (result.StartsWith("Connected"))
                {
                    litConnClass.Text = "ok";
                    lblConnStatus.Text = result;
                    ShowAlert(result, "success");
                }
                else
                {
                    litConnClass.Text = "err";
                    lblConnStatus.Text = result;
                    ShowAlert(result, "danger");
                }
                LoadConnectionInfo();
                LoadLog();
            }
            catch (Exception ex)
            {
                litConnClass.Text = "err";
                lblConnStatus.Text = "Error: " + ex.Message;
                ShowAlert("Connection failed: " + ex.Message, "danger");
            }
        }

        // ── PRODUCTS ──

        private void LoadProducts()
        {
            try
            {
                var dt = ZohoHelper.GetProductsForSync();
                rptProducts.DataSource = dt;
                rptProducts.DataBind();
            }
            catch (Exception ex)
            {
                ShowAlert("Error loading products: " + ex.Message, "danger");
            }
        }

        protected void btnSyncAllProducts_Click(object sender, EventArgs e)
        {
            hfActiveTab.Value = "products";
            try
            {
                int count = ZohoHelper.SyncAllProducts();
                ShowAlert("Synced " + count + " product(s) to Zoho Books.", "success");
            }
            catch (Exception ex)
            {
                ShowAlert("Sync error: " + ex.Message, "danger");
            }
            LoadProducts();
            LoadLog();
        }

        protected void rptProducts_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            hfActiveTab.Value = "products";
            if (e.CommandName == "SyncOne")
            {
                int productId = Convert.ToInt32(e.CommandArgument);
                string result = ZohoHelper.SyncProductToZoho(productId);
                if (result == "OK")
                    ShowAlert("Product synced successfully.", "success");
                else
                    ShowAlert(result, "danger");
                LoadProducts();
                LoadLog();
            }
        }

        // ── CUSTOMERS ──

        private void LoadCustomers()
        {
            try
            {
                var dt = ZohoHelper.GetCustomersForSync();
                rptCustomers.DataSource = dt;
                rptCustomers.DataBind();
            }
            catch (Exception ex)
            {
                ShowAlert("Error loading customers: " + ex.Message, "danger");
            }
        }

        protected void btnSyncAllCustomers_Click(object sender, EventArgs e)
        {
            hfActiveTab.Value = "customers";
            try
            {
                int count = ZohoHelper.SyncAllCustomers();
                ShowAlert("Synced " + count + " customer(s) to Zoho Books.", "success");
            }
            catch (Exception ex)
            {
                ShowAlert("Sync error: " + ex.Message, "danger");
            }
            LoadCustomers();
            LoadLog();
        }

        protected void rptCustomers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            hfActiveTab.Value = "customers";
            if (e.CommandName == "SyncOne")
            {
                int customerId = Convert.ToInt32(e.CommandArgument);
                string result = ZohoHelper.SyncCustomerToZoho(customerId);
                if (result == "OK")
                    ShowAlert("Customer synced successfully.", "success");
                else
                    ShowAlert(result, "danger");
                LoadCustomers();
                LoadLog();
            }
        }

        // ── SUPPLIERS ──

        private void LoadSuppliers()
        {
            try
            {
                var dt = ZohoHelper.GetSuppliersForSync();
                rptSuppliers.DataSource = dt;
                rptSuppliers.DataBind();
            }
            catch (Exception ex)
            {
                ShowAlert("Error loading suppliers: " + ex.Message, "danger");
            }
        }

        protected void btnSyncAllSuppliers_Click(object sender, EventArgs e)
        {
            hfActiveTab.Value = "suppliers";
            try
            {
                int count = ZohoHelper.SyncAllSuppliers();
                ShowAlert("Synced " + count + " supplier(s) to Zoho Books.", "success");
            }
            catch (Exception ex)
            {
                ShowAlert("Sync error: " + ex.Message, "danger");
            }
            LoadSuppliers();
            LoadLog();
        }

        protected void rptSuppliers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            hfActiveTab.Value = "suppliers";
            if (e.CommandName == "SyncOne")
            {
                int supplierId = Convert.ToInt32(e.CommandArgument);
                string result = ZohoHelper.SyncSupplierToZoho(supplierId);
                if (result == "OK")
                    ShowAlert("Supplier synced successfully.", "success");
                else
                    ShowAlert(result, "danger");
                LoadSuppliers();
                LoadLog();
            }
        }

        // ── LOG ──

        private void LoadLog()
        {
            try
            {
                var dt = ZohoHelper.GetSyncLog(50);
                rptLog.DataSource = dt;
                rptLog.DataBind();
            }
            catch { }
        }

        // ── HELPERS ──

        protected string GetSyncBadge(object zohoId, object syncStatus)
        {
            if (zohoId == null || zohoId == DBNull.Value)
                return "<span class='status-badge badge-new'>Not Synced</span>";

            string status = syncStatus?.ToString() ?? "";
            switch (status)
            {
                case "Synced": return "<span class='status-badge badge-synced'>Synced</span>";
                case "Error": return "<span class='status-badge badge-error'>Error</span>";
                case "PendingUpdate": return "<span class='status-badge badge-pending'>Pending</span>";
                default: return "<span class='status-badge badge-synced'>Synced</span>";
            }
        }

        private void ShowAlert(string msg, string type)
        {
            pnlAlert.Visible = true;
            lblAlert.Text = msg;
            pnlAlert.CssClass = "alert alert-" + type;
        }
    }
}
