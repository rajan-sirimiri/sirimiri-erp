using System; using System.Data; using System.Text;
using System.Web.UI; using System.Web.UI.WebControls; using PKApp.DAL;
namespace PKApp {
    public partial class PKShipment : Page {
        protected Label lblUser, lblAlert;
        protected Panel pnlAlert, pnlPO, pnlShip, pnlPOEmpty, pnlPOTable;
        protected Panel pnlShipEmpty, pnlShipTable, pnlPOLines, pnlSelectPO;
        protected HiddenField hfTab, hfPOID;
        protected Button btnTabPO, btnTabShip, btnSavePO, btnCreateShipment;
        protected DropDownList ddlPOCustomer, ddlShipPO;
        protected System.Web.UI.HtmlControls.HtmlInputGenericControl txtPODate, txtDeliveryDate, txtPORemarks;
        protected System.Web.UI.HtmlControls.HtmlInputGenericControl txtShipDate, txtVehicle, txtDriver, txtShipRemarks;
        protected Repeater rptPOs, rptShipments, rptShipLines;
        protected Literal litProducts;
        protected int UserID => Convert.ToInt32(Session["PK_UserID"]);

        protected void Page_Load(object s, EventArgs e) {
            if (Session["PK_UserID"] == null) { Response.Redirect("PKLogin.aspx"); return; }
            lblUser.Text = Session["PK_FullName"] as string ?? "";
            if (!IsPostBack) {
                BindCustomerDropdown(); BindPODropdown(); BuildProductLiteral();
                BindPOList(); BindShipmentList();
                SetTab("po");
            }
        }

        void BindCustomerDropdown() {
            var dt = PKDatabaseHelper.GetActiveCustomers();
            ddlPOCustomer.Items.Clear();
            ddlPOCustomer.Items.Add(new ListItem("-- Select Customer --", "0"));
            foreach (DataRow r in dt.Rows)
                ddlPOCustomer.Items.Add(new ListItem(r["CustomerName"].ToString(), r["CustomerID"].ToString()));
        }

        void BindPODropdown() {
            var dt = PKDatabaseHelper.GetOpenPOs();
            ddlShipPO.Items.Clear();
            ddlShipPO.Items.Add(new ListItem("-- Select PO --", "0"));
            foreach (DataRow r in dt.Rows)
                ddlShipPO.Items.Add(new ListItem(
                    r["POCode"] + " — " + r["CustomerName"] + " (" + Convert.ToDateTime(r["PODate"]).ToString("dd MMM") + ")",
                    r["POID"].ToString() + "|" + r["CustomerID"].ToString()));
        }

        void BuildProductLiteral() {
            var dt = PKDatabaseHelper.GetActiveProducts();
            var sb = new StringBuilder();
            sb.Append("<option value='0'>-- Select Product --</option>");
            foreach (DataRow r in dt.Rows)
                sb.Append("<option value='" + r["ProductID"] + "'>" + r["ProductName"] + " (" + r["Unit"] + ")</option>");
            litProducts.Text = sb.ToString();
        }

        void BindPOList() {
            var dt = PKDatabaseHelper.GetAllPOs();
            pnlPOEmpty.Visible = dt.Rows.Count == 0;
            pnlPOTable.Visible = dt.Rows.Count > 0;
            rptPOs.DataSource = dt; rptPOs.DataBind();
        }

        void BindShipmentList() {
            var dt = PKDatabaseHelper.GetAllShipments();
            pnlShipEmpty.Visible = dt.Rows.Count == 0;
            pnlShipTable.Visible = dt.Rows.Count > 0;
            rptShipments.DataSource = dt; rptShipments.DataBind();
        }

        protected void btnTabPO_Click(object s, EventArgs e) { SetTab("po"); BindPOList(); }
        protected void btnTabShip_Click(object s, EventArgs e) { SetTab("ship"); BindShipmentList(); }

        void SetTab(string tab) {
            hfTab.Value = tab;
            pnlPO.Visible   = tab == "po";
            pnlShip.Visible = tab == "ship";
            btnTabPO.CssClass   = "tab" + (tab == "po" ? " active" : "");
            btnTabShip.CssClass = "tab" + (tab == "ship" ? " active" : "");
        }

        protected void ddlShipPO_Changed(object s, EventArgs e) {
            string val = ddlShipPO.SelectedValue;
            if (val == "0") { pnlPOLines.Visible = false; pnlSelectPO.Visible = true; return; }
            int poId = int.Parse(val.Split('|')[0]);
            hfPOID.Value = poId.ToString();
            var dt = PKDatabaseHelper.GetPOLines(poId);
            pnlSelectPO.Visible = false;
            pnlPOLines.Visible  = true;
            rptShipLines.DataSource = dt; rptShipLines.DataBind();
        }

        protected void btnSavePO_Click(object s, EventArgs e) {
            int custId = Convert.ToInt32(ddlPOCustomer.SelectedValue);
            if (custId == 0) { ShowAlert("Please select a customer.", false); return; }
            DateTime poDate;
            if (!DateTime.TryParse(txtPODate.Value, out poDate)) { ShowAlert("Please enter a valid PO date.", false); return; }
            DateTime? delivDate = null;
            DateTime d;
            if (DateTime.TryParse(txtDeliveryDate.Value, out d)) delivDate = d;
            try {
                int poId = PKDatabaseHelper.AddPO(custId, poDate, delivDate, txtPORemarks.Value, UserID);
                // Process line items from Request.Form
                int lineNum = 1;
                while (Request.Form["po_product_" + lineNum] != null) {
                    int pid; decimal qty; decimal price;
                    int.TryParse(Request.Form["po_product_" + lineNum], out pid);
                    decimal.TryParse(Request.Form["po_qty_" + lineNum], out qty);
                    decimal.TryParse(Request.Form["po_price_" + lineNum], out price);
                    if (pid > 0 && qty > 0) PKDatabaseHelper.AddPOLine(poId, pid, qty, price > 0 ? (decimal?)price : null);
                    lineNum++;
                }
                ShowAlert("PO created successfully.", true);
                BindPOList(); BindPODropdown();
            } catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        protected void btnCreateShipment_Click(object s, EventArgs e) {
            string poVal = ddlShipPO.SelectedValue;
            if (poVal == "0") { ShowAlert("Please select a PO.", false); return; }
            string[] parts = poVal.Split('|');
            int poId = int.Parse(parts[0]), custId = int.Parse(parts[1]);
            DateTime shipDate;
            if (!DateTime.TryParse(txtShipDate.Value, out shipDate)) { ShowAlert("Please enter a valid ship date.", false); return; }
            try {
                int shipId = PKDatabaseHelper.CreateShipment(poId, custId, shipDate,
                    txtVehicle.Value, txtDriver.Value, txtShipRemarks.Value, UserID);
                // Process ship lines from Request.Form
                foreach (string key in Request.Form.AllKeys) {
                    if (key == null || !key.StartsWith("ship_qty_")) continue;
                    string lineId = key.Substring(9);
                    decimal qty; int pid;
                    if (!decimal.TryParse(Request.Form[key], out qty) || qty <= 0) continue;
                    if (!int.TryParse(Request.Form["ship_pid_" + lineId], out pid)) continue;
                    PKDatabaseHelper.AddShipmentLine(shipId, pid, qty);
                }
                ShowAlert("Shipment created successfully.", true);
                BindShipmentList(); BindPODropdown();
            } catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        void ShowAlert(string m, bool ok) { lblAlert.Text = m; pnlAlert.CssClass = "alert " + (ok ? "alert-success" : "alert-danger"); pnlAlert.Visible = true; }
    }
}
