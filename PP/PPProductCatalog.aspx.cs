using System;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using PPApp.DAL;

namespace PPApp
{
    public partial class PPProductCatalog : Page
    {
        protected Label       lblNavUser, lblCount;
        protected HiddenField hfSelectedProductId;
        protected Repeater    rptProducts;
        protected Panel       pnlEmpty, pnlDetail;

        // Detail labels
        protected Label lblProductName, lblProductCode, lblProductType;
        protected Label lblBatchSize, lblProdUOM, lblOutputUOM, lblUnitWeight;
        protected Label lblProductionLine, lblHSN, lblGST, lblPriceCalc;

        // BOM
        protected Panel   pnlBOM, pnlBOMEmpty, pnlBOMTable;
        protected Repeater rptBOM;
        protected Label   lblBOMTotal;

        // Stages
        protected Panel pnlStages;
        protected Label lblInputRM, lblStageFlow;

        // Params
        protected Panel   pnlParams;
        protected Repeater rptParams;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PP_UserID"] == null) { Response.Redirect("PPLogin.aspx"); return; }

            string role = Session["PP_Role"]?.ToString() ?? "";
            if (!PPDatabaseHelper.RoleHasModuleAccess(role, "PP", "PP_PRODUCT"))
            { Response.Redirect("PPHome.aspx"); return; }
            lblNavUser.Text = Session["PP_FullName"] as string ?? "";

            if (!IsPostBack)
            {
                BindProductList();
            }
            else
            {
                // Handle product selection
                BindProductList();
                int selId = 0;
                if (int.TryParse(hfSelectedProductId.Value, out selId) && selId > 0)
                    LoadProductDetail(selId);
            }
        }

        private void BindProductList()
        {
            var dt = PPDatabaseHelper.GetAllProducts();
            rptProducts.DataSource = dt;
            rptProducts.DataBind();
            lblCount.Text = dt.Rows.Count + " products";
        }

        private void LoadProductDetail(int productId)
        {
            DataRow dr = PPDatabaseHelper.GetProductById(productId);
            if (dr == null) return;

            pnlEmpty.Visible  = false;
            pnlDetail.Visible = true;

            lblProductName.Text = dr["ProductName"].ToString();
            lblProductCode.Text = dr["ProductCode"].ToString();
            string ptype = dr["ProductType"].ToString();
            lblProductType.Text = ptype;

            lblBatchSize.Text = Convert.ToDecimal(dr["BatchSize"]).ToString("0.###") + " " +
                dr["ProdAbbreviation"].ToString();
            lblProdUOM.Text   = dr["ProdAbbreviation"].ToString();
            lblOutputUOM.Text = dr["OutputAbbreviation"].ToString();

            decimal unitWt = dr["UnitWeightGrams"] != DBNull.Value ? Convert.ToDecimal(dr["UnitWeightGrams"]) : 0;
            lblUnitWeight.Text = unitWt > 0 ? unitWt.ToString("0.##") + " g" : "—";

            string lineName = "—";
            if (dr.Table.Columns.Contains("ProductionLineName") && dr["ProductionLineName"] != DBNull.Value)
                lineName = dr["ProductionLineName"].ToString();
            else if (dr.Table.Columns.Contains("ProductionLineID") && dr["ProductionLineID"] != DBNull.Value)
            {
                int lineId = Convert.ToInt32(dr["ProductionLineID"]);
                if (lineId > 0)
                {
                    var lineRow = PPDatabaseHelper.ExecuteQueryPublic(
                        "SELECT LineName FROM PP_ProductionLines WHERE LineID=?id;",
                        new MySql.Data.MySqlClient.MySqlParameter("?id", lineId));
                    if (lineRow.Rows.Count > 0) lineName = lineRow.Rows[0]["LineName"].ToString();
                }
            }
            lblProductionLine.Text = lineName;
            lblHSN.Text = dr["HSNCode"] != DBNull.Value && dr["HSNCode"].ToString() != ""
                ? dr["HSNCode"].ToString() : "—";
            lblGST.Text = dr["GSTRate"] != DBNull.Value
                ? Convert.ToDecimal(dr["GSTRate"]).ToString("0.##") + "%" : "—";

            bool isPriceCalc = dr.Table.Columns.Contains("IsPriceCalcProduct") &&
                dr["IsPriceCalcProduct"] != DBNull.Value && Convert.ToInt32(dr["IsPriceCalcProduct"]) == 1;
            lblPriceCalc.Text = isPriceCalc
                ? "<span class='tag tag-yes'>Yes</span>"
                : "<span class='tag tag-no'>No</span>";

            // BOM
            bool showBOM = (ptype == "Core" || ptype == "Conversion" || ptype == "Prefilled Conversion");
            pnlBOM.Visible = showBOM;
            if (showBOM)
            {
                var bom = PPDatabaseHelper.GetBOMByProduct(productId);
                if (bom.Rows.Count == 0)
                {
                    pnlBOMEmpty.Visible = true;
                    pnlBOMTable.Visible = false;
                }
                else
                {
                    pnlBOMEmpty.Visible = false;
                    pnlBOMTable.Visible = true;
                    rptBOM.DataSource   = bom;
                    rptBOM.DataBind();

                    decimal totalCost = 0;
                    foreach (DataRow r in bom.Rows)
                    {
                        decimal q = Convert.ToDecimal(r["Quantity"]);
                        decimal u = r["UnitRate"] != DBNull.Value ? Convert.ToDecimal(r["UnitRate"]) : 0;
                        totalCost += q * u;
                    }
                    lblBOMTotal.Text = totalCost.ToString("N2");
                }
            }

            // Pre-Process Stages
            bool showStages = (ptype == "Pre processed RM");
            pnlStages.Visible = showStages;
            if (showStages)
            {
                DataRow stages = PPDatabaseHelper.GetPreprocessStages(productId);
                if (stages != null)
                {
                    lblInputRM.Text = stages["InputRMName"] != DBNull.Value
                        ? stages["InputRMName"].ToString() : "—";

                    var sb = new StringBuilder();
                    string[] stageLabels = { "Stage1Label", "Stage2Label", "Stage3Label", "Stage4Label" };
                    int stageNum = 0;
                    foreach (string col in stageLabels)
                    {
                        if (stages[col] != DBNull.Value && stages[col].ToString() != "")
                        {
                            stageNum++;
                            if (stageNum > 1) sb.Append("<span class='stage-arrow'>→</span>");
                            sb.Append("<div class='stage-pill'>S" + stageNum + ": " + stages[col].ToString() + "</div>");
                        }
                    }
                    lblStageFlow.Text = sb.ToString();
                }
            }

            // Batch Parameters
            var prms = PPDatabaseHelper.GetProductParams(productId);
            pnlParams.Visible = prms.Rows.Count > 0;
            if (prms.Rows.Count > 0)
            {
                rptParams.DataSource = prms;
                rptParams.DataBind();
            }
        }

        // ── Helper methods for ASPX ──

        protected string GetTypeClass(object productType)
        {
            string t = (productType ?? "").ToString().ToLower().Replace(" ", "");
            if (t == "core") return "core";
            if (t == "conversion") return "conversion";
            if (t.Contains("prefilled")) return "prefilled";
            if (t.Contains("preprocess") || t.Contains("preprocessed")) return "preprocess";
            return "core";
        }

        protected string GetTypeIcon(object productType)
        {
            string t = (productType ?? "").ToString().ToLower();
            if (t == "core") return "&#x2699;";
            if (t == "conversion") return "&#x1F504;";
            if (t.Contains("prefilled")) return "&#x1F4E6;";
            if (t.Contains("preprocess")) return "&#x1F525;";
            return "&#x2699;";
        }

        protected string FormatRate(object val)
        {
            if (val == null || val == DBNull.Value) return "—";
            decimal d = Convert.ToDecimal(val);
            return d > 0 ? d.ToString("N2") : "—";
        }

        protected string FormatBOMCost(object qty, object rate)
        {
            decimal q = (qty != null && qty != DBNull.Value) ? Convert.ToDecimal(qty) : 0;
            decimal r = (rate != null && rate != DBNull.Value) ? Convert.ToDecimal(rate) : 0;
            decimal cost = q * r;
            return cost > 0 ? cost.ToString("N2") : "—";
        }
    }
}
