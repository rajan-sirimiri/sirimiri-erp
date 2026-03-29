using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKApp.DAL;

namespace PKApp
{
    public partial class PKPMStockReport : Page
    {
        protected Label    lblUser, lblReportDate, lblPrintDate;
        protected Repeater rptStock;
        protected Label    lblTotal, lblInStock, lblLow, lblZero;
        protected Button   btnRefresh;

        protected void Page_Load(object s, EventArgs e)
        {
            if (Session["PK_UserID"] == null) { Response.Redirect("PKLogin.aspx?ReturnUrl=" + Server.UrlEncode(Request.Url.PathAndQuery)); return; }
            lblUser.Text = Session["PK_FullName"] as string ?? "";
            if (!IsPostBack) BindReport();
        }

        protected void btnRefresh_Click(object s, EventArgs e) { BindReport(); }

        void BindReport()
        {
            string now = PKDatabaseHelper.NowIST().ToString("dd MMM yyyy, hh:mm tt");
            lblReportDate.Text = now;
            lblPrintDate.Text  = now;

            var dt = PKDatabaseHelper.GetPMStockReport();
            rptStock.DataSource = dt;
            rptStock.DataBind();

            int total = dt.Rows.Count, inStock = 0, low = 0, zero = 0;
            foreach (DataRow row in dt.Rows)
            {
                decimal stock   = Convert.ToDecimal(row["CurrentStock"]);
                decimal reorder = row["ReorderLevel"] == DBNull.Value ? 0 : Convert.ToDecimal(row["ReorderLevel"]);
                if (stock <= 0)                       zero++;
                else if (reorder > 0 && stock <= reorder) low++;
                else                                  inStock++;
            }
            lblTotal.Text   = total.ToString();
            lblInStock.Text = inStock.ToString();
            lblLow.Text     = low.ToString();
            lblZero.Text    = zero.ToString();
        }

        protected string FormatQty(object val)
        {
            if (val == null || val == DBNull.Value) return "0";
            return Convert.ToDecimal(val).ToString("0.###");
        }

        protected string GetStockClass(object stockVal, object reorderVal)
        {
            try
            {
                decimal stock   = Convert.ToDecimal(stockVal);
                decimal reorder = reorderVal == DBNull.Value ? 0 : Convert.ToDecimal(reorderVal);
                if (stock <= 0)                       return "stock-zero";
                if (reorder > 0 && stock <= reorder)  return "stock-low";
                return "stock-ok";
            }
            catch { return "stock-ok"; }
        }
    }
}
