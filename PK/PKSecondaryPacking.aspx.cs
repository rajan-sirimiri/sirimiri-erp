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
        protected DropDownList ddlProduct;
        protected HiddenField hfProductData, hfCasePMID;
        protected System.Web.UI.HtmlControls.HtmlInputGenericControl txtCartons, txtUnitsPerCarton;
        protected System.Web.UI.HtmlControls.HtmlTextArea txtRemarks;
        protected Repeater rptLog;
        protected Button btnPack;
        protected int UserID => Convert.ToInt32(Session["PK_UserID"]);

        protected void Page_Load(object s, EventArgs e)
        {
            if (Session["PK_UserID"] == null) { Response.Redirect("PKLogin.aspx"); return; }
            if (lblUser != null) lblUser.Text = Session["PK_FullName"] as string ?? "";
            if (!IsPostBack) { BindProductDropdown(); BindLog(); }
        }

        void BindProductDropdown()
        {
            var dt = PKDatabaseHelper.GetFGReadyForSecondary();
            ddlProduct.Items.Clear();
            ddlProduct.Items.Add(new ListItem("-- Select Product --", "0"));

            // Build JSON for client-side product info
            var sb = new System.Text.StringBuilder("{");
            bool first = true;
            foreach (DataRow r in dt.Rows)
            {
                string pid = r["ProductID"].ToString();
                string name = r["ProductName"].ToString();
                string code = r["ProductCode"].ToString();
                int availPcs = Convert.ToInt32(r["AvailablePcs"]);
                string unitSizes = r["UnitsPerContainer"] == DBNull.Value ? "" : r["UnitsPerContainer"].ToString();
                // Get first unit size for jar calculation
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

                // Calculate jars for dropdown display
                int jarSize = 0;
                int.TryParse(firstUnitSize, out jarSize);
                if (jarSize <= 0) jarSize = 1;
                int availJars = availPcs / jarSize;
                string ctLabel = containerType == "DIRECT" ? "containers" : containerType.ToLower() + "s";

                ddlProduct.Items.Add(new ListItem(
                    name + " (" + code + ") — " + availJars + " " + ctLabel + " available",
                    pid));

                if (!first) sb.Append(",");
                sb.Append("\"" + pid + "\":{");
                sb.Append("\"name\":\"" + EscapeJson(name) + "\",");
                sb.Append("\"code\":\"" + EscapeJson(code) + "\",");
                sb.Append("\"availPcs\":" + availPcs + ",");
                sb.Append("\"unitSizes\":\"" + EscapeJson(firstUnitSize) + "\",");
                sb.Append("\"containersPerCase\":" + containersPerCase + ",");
                sb.Append("\"containerType\":\"" + EscapeJson(containerType) + "\",");
                sb.Append("\"casePMID\":\"" + casePMID + "\",");
                sb.Append("\"casePMName\":\"" + EscapeJson(casePMName) + "\"");
                sb.Append("}");
                first = false;
            }
            sb.Append("}");
            hfProductData.Value = sb.ToString();
        }

        string EscapeJson(string s)
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

            // Get available — need to compute in jars
            var products = PKDatabaseHelper.GetFGReadyForSecondary();
            DataRow productRow = null;
            foreach (DataRow r in products.Rows)
            {
                if (Convert.ToInt32(r["ProductID"]) == productId) { productRow = r; break; }
            }
            if (productRow == null) { ShowAlert("Product not found or no stock available.", false); return; }

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
            {
                ShowAlert("Need " + totalJars + " jars but only " + availJars + " available.", false);
                return;
            }

            // The existing AddSecondaryPacking expects TotalUnits in individual pieces
            int totalPcs = totalJars * jarSize;

            int pmId = 0;
            int.TryParse(hfCasePMID.Value, out pmId);
            decimal cartonsUsed = pmId > 0 ? (decimal)cases : 0;

            try
            {
                PKDatabaseHelper.AddSecondaryPacking(productId, cases, unitsPerCarton,
                    pmId, cartonsUsed, txtRemarks.Value, UserID);

                string ctLabel = productRow["ContainerType"].ToString();
                ctLabel = ctLabel == "DIRECT" ? "containers" : ctLabel.ToLower() + "s";

                txtCartons.Value = ""; txtUnitsPerCarton.Value = ""; txtRemarks.Value = "";
                ShowAlert(cases + " cases packed (" + totalJars + " " + ctLabel + " → " + cases + " master cartons). Added to FG stock.", true);
                BindProductDropdown();
                BindLog();
            }
            catch (Exception ex)
            {
                ShowAlert("Error: " + ex.Message, false);
            }
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
