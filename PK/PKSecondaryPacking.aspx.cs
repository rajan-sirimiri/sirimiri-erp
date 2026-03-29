using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKApp.DAL;

namespace PKApp
{
    public partial class PKSecondaryPacking : Page
    {
        protected Label lblUser, lblAlert;
        protected Panel pnlAlert, pnlEmpty, pnlTable;
        protected DropDownList ddlProduct, ddlOnlineCarton;
        protected HiddenField hfProductData, hfCasePMID, hfOnlineLines;
        protected System.Web.UI.HtmlControls.HtmlInputGenericControl txtCartons, txtUnitsPerCarton;
        protected System.Web.UI.HtmlControls.HtmlInputText txtOnlineOrderId, txtCustomerName;
        protected System.Web.UI.HtmlControls.HtmlTextArea txtRemarks, txtOnlineRemarks;
        protected Repeater rptLog;
        protected Button btnPack, btnPackOnline;
        protected int UserID => Convert.ToInt32(Session["PK_UserID"]);

        protected void Page_Load(object s, EventArgs e)
        {
            if (Session["PK_UserID"] == null) { Response.Redirect("PKLogin.aspx?ReturnUrl=" + Server.UrlEncode(Request.Url.PathAndQuery)); return; }
            if (lblUser != null) lblUser.Text = Session["PK_FullName"] as string ?? "";
            if (!IsPostBack) { BindProductDropdown(); BindCartonDropdown(); BindLog(); }
        }

        void BindProductDropdown()
        {
            var dt = PKDatabaseHelper.GetFGReadyForSecondary();
            if (ddlProduct != null)
            {
                ddlProduct.Items.Clear();
                ddlProduct.Items.Add(new ListItem("-- Select Product --", "0"));
            }

            var sb = new System.Text.StringBuilder("{");
            bool first = true;
            foreach (DataRow r in dt.Rows)
            {
                string pid = r["ProductID"].ToString();
                string name = r["ProductName"].ToString();
                string code = r["ProductCode"].ToString();
                int availPcs = Convert.ToInt32(r["AvailablePcs"]);
                string unitSizes = r["UnitsPerContainer"] == DBNull.Value ? "" : r["UnitsPerContainer"].ToString();
                string firstUnitSize = "";
                if (!string.IsNullOrEmpty(unitSizes))
                {
                    string[] sizes = unitSizes.Split(',');
                    firstUnitSize = sizes[0].Trim();
                }
                int containersPerCase = Convert.ToInt32(r["ContainersPerCase"]);
                string containerType = r["ContainerType"].ToString();
                int casePMID = Convert.ToInt32(r["CasePMID"]);
                string casePMName = r["CasePMName"].ToString();

                int jarSize = 0;
                int.TryParse(firstUnitSize, out jarSize);
                if (jarSize <= 0) jarSize = 1;
                int availJars = availPcs / jarSize;
                string ctLabel = containerType == "DIRECT" ? "containers" : containerType.ToLower() + "s";

                if (ddlProduct != null)
                    ddlProduct.Items.Add(new ListItem(
                        name + " (" + code + ") — " + availJars + " " + ctLabel,
                        pid));

                if (!first) sb.Append(",");
                sb.Append("\"" + pid + "\":{");
                sb.Append("\"name\":\"" + Esc(name) + "\",");
                sb.Append("\"code\":\"" + Esc(code) + "\",");
                sb.Append("\"availPcs\":" + availPcs + ",");
                sb.Append("\"unitSizes\":\"" + Esc(firstUnitSize) + "\",");
                sb.Append("\"containersPerCase\":" + containersPerCase + ",");
                sb.Append("\"containerType\":\"" + Esc(containerType) + "\",");
                sb.Append("\"casePMID\":\"" + casePMID + "\",");
                sb.Append("\"casePMName\":\"" + Esc(casePMName) + "\"");
                sb.Append("}");
                first = false;
            }
            sb.Append("}");
            if (hfProductData != null) hfProductData.Value = sb.ToString();
        }

        void BindCartonDropdown()
        {
            if (ddlOnlineCarton == null) return;
            var dt = PKDatabaseHelper.GetPackingMaterialsByCategory("Carton / Master Box");
            ddlOnlineCarton.Items.Clear();
            ddlOnlineCarton.Items.Add(new ListItem("-- Select Carton --", "0"));
            foreach (DataRow r in dt.Rows)
            {
                string stock = Convert.ToDecimal(r["CurrentStock"]).ToString("0.##");
                ddlOnlineCarton.Items.Add(new ListItem(
                    r["PMName"] + " (Stock: " + stock + " " + r["Abbreviation"] + ")",
                    r["PMID"].ToString()));
            }
        }

        string Esc(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "").Replace("\r", "");
        }

        void BindLog()
        {
            var dt = PKDatabaseHelper.GetSecondaryPackingToday();
            if (pnlEmpty != null) pnlEmpty.Visible = dt.Rows.Count == 0;
            if (pnlTable != null) pnlTable.Visible = dt.Rows.Count > 0;
            if (rptLog != null) { rptLog.DataSource = dt; rptLog.DataBind(); }
        }

        // ── CASE PACKING SAVE ──
        protected void btnPack_Click(object s, EventArgs e)
        {
            int productId = Convert.ToInt32(ddlProduct.SelectedValue);
            if (productId == 0) { ShowAlert("Please select a product.", false); return; }

            int cases, unitsPerCarton;
            if (!int.TryParse(txtCartons.Value, out cases) || cases <= 0)
            { ShowAlert("Enter a valid number of cases.", false); return; }
            if (!int.TryParse(txtUnitsPerCarton.Value, out unitsPerCarton) || unitsPerCarton <= 0)
            { ShowAlert("Enter valid jars per case.", false); return; }

            int totalJars = cases * unitsPerCarton;

            var products = PKDatabaseHelper.GetFGReadyForSecondary();
            DataRow productRow = null;
            foreach (DataRow r in products.Rows)
                if (Convert.ToInt32(r["ProductID"]) == productId) { productRow = r; break; }
            if (productRow == null) { ShowAlert("Product not found or no stock.", false); return; }

            int availPcs = Convert.ToInt32(productRow["AvailablePcs"]);
            string unitSizes = productRow["UnitsPerContainer"] == DBNull.Value ? "" : productRow["UnitsPerContainer"].ToString();
            int jarSize = 0;
            if (!string.IsNullOrEmpty(unitSizes))
            {
                string[] sizes = unitSizes.Split(',');
                int.TryParse(sizes[0].Trim(), out jarSize);
            }
            if (jarSize <= 0) jarSize = 1;
            int availJars = availPcs / jarSize;

            if (totalJars > availJars)
            { ShowAlert("Need " + totalJars + " jars but only " + availJars + " available.", false); return; }

            int pmId = 0;
            int.TryParse(hfCasePMID.Value, out pmId);
            decimal cartonsUsed = pmId > 0 ? (decimal)cases : 0;

            // Check ALL CASE-level PM stock for this product
            var casePMs = PKDatabaseHelper.GetProductPMMappings(productId);
            var pmShortages = new System.Collections.Generic.List<string>();
            foreach (DataRow pmRow in casePMs.Rows)
            {
                string level = pmRow["ApplyLevel"].ToString();
                if (level != "CASE") continue;
                int mappedPmId = Convert.ToInt32(pmRow["PMID"]);
                decimal qtyPerCase = Convert.ToDecimal(pmRow["QtyPerUnit"]);
                decimal needed = cases * qtyPerCase;
                decimal available = PKDatabaseHelper.GetPMCurrentStock(mappedPmId);
                if (needed > available)
                    pmShortages.Add(pmRow["PMName"] + " (need " + needed.ToString("0.##") + ", have " + available.ToString("0.##") + ")");
            }
            // Also check the auto-detected carton PM if not in mappings
            if (pmId > 0 && pmShortages.Count == 0)
            {
                decimal pmStock = PKDatabaseHelper.GetPMCurrentStock(pmId);
                if (pmStock < cases)
                    pmShortages.Add("Carton (need " + cases + ", have " + pmStock.ToString("0.##") + ")");
            }
            if (pmShortages.Count > 0)
            {
                ShowAlert("Cannot pack — insufficient PM stock: " + string.Join("; ", pmShortages) + ". Please do PM GRN first.", false);
                return;
            }

            try
            {
                PKDatabaseHelper.AddSecondaryPacking(productId, cases, unitsPerCarton,
                    pmId, cartonsUsed, txtRemarks.Value, UserID);

                txtCartons.Value = ""; txtUnitsPerCarton.Value = ""; txtRemarks.Value = "";
                ShowAlert(cases + " cases packed (" + totalJars + " jars). Moved from SFG to FG — ready for dispatch.", true);
                BindProductDropdown(); BindLog();
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        // ── ONLINE ORDER SAVE ──
        protected void btnPackOnline_Click(object s, EventArgs e)
        {
            string orderId = txtOnlineOrderId != null ? txtOnlineOrderId.Value.Trim() : "";
            string customer = txtCustomerName != null ? txtCustomerName.Value.Trim() : "";
            if (string.IsNullOrEmpty(orderId))
            { ShowAlert("Please enter an Order ID.", false); return; }
            if (string.IsNullOrEmpty(customer))
            { ShowAlert("Please enter a Customer Name.", false); return; }

            string linesRaw = hfOnlineLines != null ? hfOnlineLines.Value : "";
            if (string.IsNullOrEmpty(linesRaw))
            { ShowAlert("Please add at least one product to the order.", false); return; }

            string[] lineParts = linesRaw.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (lineParts.Length == 0)
            { ShowAlert("Please add at least one product to the order.", false); return; }

            int cartonPmId = 0;
            if (ddlOnlineCarton != null)
                int.TryParse(ddlOnlineCarton.SelectedValue, out cartonPmId);

            // Check carton PM stock for online order (1 carton per order)
            if (cartonPmId > 0)
            {
                decimal pmStock = PKDatabaseHelper.GetPMCurrentStock(cartonPmId);
                if (pmStock < 1)
                {
                    ShowAlert("Insufficient shipping carton stock — 0 available. Please do PM GRN first.", false);
                    return;
                }
            }

            string remarks = txtOnlineRemarks != null ? txtOnlineRemarks.Value.Trim() : "";

            try
            {
                // Validate stock
                var products = PKDatabaseHelper.GetFGReadyForSecondary();
                foreach (string part in lineParts)
                {
                    string[] fields = part.Split(':');
                    if (fields.Length < 3) continue;
                    int pid = int.Parse(fields[0]);
                    int qty = int.Parse(fields[1]);
                    int js  = int.Parse(fields[2]);

                    DataRow pr = null;
                    foreach (DataRow r in products.Rows)
                        if (Convert.ToInt32(r["ProductID"]) == pid) { pr = r; break; }
                    if (pr == null)
                    { ShowAlert("Product ID " + pid + " not found or no stock.", false); return; }

                    int availPcs = Convert.ToInt32(pr["AvailablePcs"]);
                    int availJars = availPcs / (js > 0 ? js : 1);
                    if (qty > availJars)
                    { ShowAlert("Insufficient stock for " + pr["ProductName"] + ": need " + qty + ", have " + availJars, false); return; }
                }

                // Save each line
                bool cartonRecorded = false;
                foreach (string part in lineParts)
                {
                    string[] fields = part.Split(':');
                    if (fields.Length < 3) continue;
                    int pid = int.Parse(fields[0]);
                    int qty = int.Parse(fields[1]);
                    int js  = int.Parse(fields[2]);

                    PKDatabaseHelper.AddOnlineOrderPacking(pid, qty, js,
                        cartonPmId, orderId, customer, remarks, UserID);

                    if (!cartonRecorded && cartonPmId > 0)
                    {
                        PKDatabaseHelper.RecordOnlineOrderCartonPM(cartonPmId, UserID);
                        cartonRecorded = true;
                    }
                }

                if (txtOnlineOrderId != null) txtOnlineOrderId.Value = "";
                if (txtCustomerName != null) txtCustomerName.Value = "";
                if (txtOnlineRemarks != null) txtOnlineRemarks.Value = "";
                if (hfOnlineLines != null) hfOnlineLines.Value = "";

                ShowAlert("Online order " + orderId + " packed — " + lineParts.Length + " product(s) for " + customer + ".", true);
                BindProductDropdown(); BindCartonDropdown(); BindLog();
            }
            catch (Exception ex) { ShowAlert("Error: " + ex.Message, false); }
        }

        void ShowAlert(string m, bool ok)
        {
            if (lblAlert != null) lblAlert.Text = m;
            if (pnlAlert != null)
            {
                pnlAlert.CssClass = "alert " + (ok ? "alert-success" : "alert-danger");
                pnlAlert.Visible = true;
            }
        }
    }
}
