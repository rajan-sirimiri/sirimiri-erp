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
        protected Label lblPiName, lblPiCode, lblPiJars, lblPiJarsLabel, lblPiPerCase, lblPiPerCaseLabel, lblPiMaxCases;
        protected Panel pnlAlert, pnlEmpty, pnlTable, pnlProductInfo;
        protected DropDownList ddlProduct, ddlOnlineCarton;
        protected Panel pnlCasePM;
        protected HiddenField hfProductData, hfOnlineLines;
        protected System.Web.UI.HtmlControls.HtmlInputGenericControl txtCartons, txtUnitsPerCarton;
        protected System.Web.UI.HtmlControls.HtmlInputText txtOnlineOrderId, txtCustomerName;
        protected System.Web.UI.HtmlControls.HtmlTextArea txtRemarks, txtOnlineRemarks;
        protected Repeater rptLog, rptCasePM;
        protected Button btnPack, btnPackOnline;
        protected int UserID => Convert.ToInt32(Session["PK_UserID"]);

        protected void Page_Load(object s, EventArgs e)
        {
            if (Session["PK_UserID"] == null) { Response.Redirect("PKLogin.aspx?ReturnUrl=" + Server.UrlEncode(Request.Url.PathAndQuery)); return; }

            // Module access check
            string __role = Session["PK_Role"]?.ToString() ?? "";
            if (!PKDatabaseHelper.RoleHasModuleAccess(__role, "PK", "PK_SECONDARY"))
            { Response.Redirect("PKHome.aspx"); return; }
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

                int jarSize = 0;
                int.TryParse(firstUnitSize, out jarSize);
                if (jarSize <= 0) jarSize = 1;
                int availJars = availPcs / jarSize;
                int maxCases = containersPerCase > 0 ? availJars / containersPerCase : 0;
                string ctLabel = containerType == "DIRECT" ? "containers" : containerType.ToLower() + "s";

                string stockText = availJars + " " + ctLabel;
                if (maxCases > 0)
                    stockText += " (" + maxCases + " cases possible)";

                if (ddlProduct != null)
                    ddlProduct.Items.Add(new ListItem(
                        name + " (" + code + ") — " + stockText,
                        pid));

                if (!first) sb.Append(",");
                sb.Append("\"" + pid + "\":{");
                sb.Append("\"name\":\"" + Esc(name) + "\",");
                sb.Append("\"code\":\"" + Esc(code) + "\",");
                sb.Append("\"availPcs\":" + availPcs + ",");
                sb.Append("\"unitSizes\":\"" + Esc(firstUnitSize) + "\",");
                sb.Append("\"containersPerCase\":" + containersPerCase + ",");
                sb.Append("\"containerType\":\"" + Esc(containerType) + "\"");
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

        protected void ddlProduct_Changed(object s, EventArgs e)
        {
            int productId = 0;
            if (ddlProduct != null && ddlProduct.SelectedValue != null)
                int.TryParse(ddlProduct.SelectedValue, out productId);
            BindCasePMs(productId);
            ShowProductInfo(productId);
        }

        void ShowProductInfo(int productId)
        {
            if (pnlProductInfo == null) return;
            if (productId <= 0) { pnlProductInfo.Visible = false; return; }

            var dt = PKDatabaseHelper.GetFGReadyForSecondary();
            DataRow row = null;
            foreach (DataRow r in dt.Rows)
                if (Convert.ToInt32(r["ProductID"]) == productId) { row = r; break; }
            if (row == null) { pnlProductInfo.Visible = false; return; }

            int availPcs = Convert.ToInt32(row["AvailablePcs"]);
            string unitSizes = row["UnitsPerContainer"] == DBNull.Value ? "1" : row["UnitsPerContainer"].ToString();
            int jarSize = 1;
            if (!string.IsNullOrEmpty(unitSizes))
            {
                string[] sizes = unitSizes.Split(',');
                int.TryParse(sizes[0].Trim(), out jarSize);
            }
            if (jarSize <= 0) jarSize = 1;
            int availJars = availPcs / jarSize;
            int containersPerCase = Convert.ToInt32(row["ContainersPerCase"]);
            int maxCases = containersPerCase > 0 ? availJars / containersPerCase : 0;
            string ct = row["ContainerType"].ToString();
            string ctLabel = ct == "DIRECT" ? "Containers" : ct + "s";

            lblPiName.Text = row["ProductName"].ToString();
            lblPiCode.Text = row["ProductCode"].ToString();
            lblPiJars.Text = availJars.ToString("N0");
            lblPiJarsLabel.Text = ctLabel + " Available";
            lblPiPerCase.Text = containersPerCase.ToString();
            lblPiPerCaseLabel.Text = ctLabel + " per Case";
            lblPiMaxCases.Text = maxCases.ToString("N0");

            // Set jars per case in the input
            if (txtUnitsPerCarton != null) txtUnitsPerCarton.Value = containersPerCase.ToString();

            pnlProductInfo.Visible = true;
        }

        void BindCasePMs(int productId)
        {
            if (pnlCasePM == null) return;
            if (productId <= 0)
            {
                pnlCasePM.Visible = false;
                return;
            }
            var dt = PKDatabaseHelper.GetCasePMsForProduct(productId);
            if (dt.Rows.Count == 0)
            {
                pnlCasePM.Visible = false;
                return;
            }
            pnlCasePM.Visible = true;
            if (rptCasePM != null)
            {
                rptCasePM.DataSource = dt;
                rptCasePM.DataBind();
            }
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

            // Read ALL case PM quantities from form and validate stock
            var casePMs = PKDatabaseHelper.GetCasePMsForProduct(productId);
            var pmShortages = new System.Collections.Generic.List<string>();
            var pmConsumptions = new System.Collections.Generic.List<int[]>(); // pmId, actualQty*100

            foreach (DataRow pmRow in casePMs.Rows)
            {
                int pmId = Convert.ToInt32(pmRow["PMID"]);
                string formKey = "casePmQty_" + pmId;
                string formVal = Request.Form[formKey];
                decimal actualQty = 0;
                if (!string.IsNullOrEmpty(formVal))
                    decimal.TryParse(formVal, out actualQty);
                else
                    actualQty = cases * Convert.ToDecimal(pmRow["QtyPerUnit"]);

                if (actualQty <= 0) continue;

                decimal available = PKDatabaseHelper.GetPMCurrentStock(pmId);
                if (actualQty > available)
                    pmShortages.Add(pmRow["PMName"] + " (need " + actualQty.ToString("0.##") + ", have " + available.ToString("0.##") + ")");

                pmConsumptions.Add(new int[] { pmId, (int)(actualQty * 100) }); // store as int*100 to avoid float issues
            }

            if (pmShortages.Count > 0)
            {
                ShowAlert("Cannot pack — insufficient PM stock: " + string.Join("; ", pmShortages) + ". Please do PM GRN first.", false);
                BindCasePMs(productId);
                return;
            }

            try
            {
                // Save secondary packing record (use first PM for backward compatibility)
                int firstPmId = pmConsumptions.Count > 0 ? pmConsumptions[0][0] : 0;
                decimal firstQty = pmConsumptions.Count > 0 ? pmConsumptions[0][1] / 100m : 0;
                PKDatabaseHelper.AddSecondaryPacking(productId, cases, unitsPerCarton,
                    firstPmId, firstQty, txtRemarks.Value, UserID);

                // Get the SecPackID just inserted
                int secPackId = Convert.ToInt32(PKDatabaseHelper.ExecuteScalar("SELECT LAST_INSERT_ID();"));

                // Record PM consumption for ALL case PMs (skip first — already recorded by AddSecondaryPacking)
                for (int i = 1; i < pmConsumptions.Count; i++)
                {
                    int pmId = pmConsumptions[i][0];
                    decimal qty = pmConsumptions[i][1] / 100m;
                    PKDatabaseHelper.RecordSecondaryPMConsumption(pmId, qty, secPackId, UserID);
                }

                txtCartons.Value = ""; txtUnitsPerCarton.Value = ""; txtRemarks.Value = "";
                if (pnlCasePM != null) pnlCasePM.Visible = false;
                ShowAlert(cases + " cases packed (" + totalJars + " jars). Moved from SFG to FG — ready for dispatch. " + pmConsumptions.Count + " PM(s) consumed.", true);
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
