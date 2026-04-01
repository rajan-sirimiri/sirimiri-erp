using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using MMApp.DAL;

namespace MMApp
{
    public partial class MMPMStockReport : Page
    {
        protected Label    lblNavUser;
        protected Label    lblReportDate;
        protected Label    lblPrintDate;
        protected Repeater rptStock;
        protected Label    lblTotal;
        protected Label    lblInStock;
        protected Label    lblLow;
        protected Label    lblZero;
        protected Button   btnRefresh;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["MM_UserID"] == null) { Response.Redirect("MMLogin.aspx"); return; }

            // Module access check
            string __role = Session["MM_Role"]?.ToString() ?? "";
            if (!MMDatabaseHelper.RoleHasModuleAccess(__role, "MM", "MM_PM_REPORT"))
            { Response.Redirect("MMHome.aspx"); return; }
            lblNavUser.Text = Session["MM_FullName"] as string ?? "";

            if (!IsPostBack)
                BindReport();
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            BindReport();
        }

        private void BindReport()
        {
            string now = DateTime.Now.ToString("dd MMM yyyy, hh:mm tt");
            lblReportDate.Text = now;
            lblPrintDate.Text  = now;

            var dt = MMDatabaseHelper.GetPMStockReport();

            rptStock.DataSource = dt;
            rptStock.DataBind();

            int total = dt.Rows.Count;
            int inStock = 0, low = 0, zero = 0;
            foreach (DataRow row in dt.Rows)
            {
                decimal stock = Convert.ToDecimal(row["CurrentStock"]);
                decimal reorder = row["ReorderLevel"] == DBNull.Value ? 0 : Convert.ToDecimal(row["ReorderLevel"]);
                if (stock <= 0)           zero++;
                else if (reorder > 0 && stock <= reorder) low++;
                else                      inStock++;
            }

            lblTotal.Text   = total.ToString();
            lblInStock.Text = inStock.ToString();
            lblLow.Text     = low.ToString();
            lblZero.Text    = zero.ToString();
        }

        protected string FormatQty(object val)
        {
            if (val == null || val == DBNull.Value) return "0";
            decimal d = Convert.ToDecimal(val);
            return d.ToString("0.###");
        }

        protected string GetStockClass(object stockVal, object reorderVal)
        {
            try
            {
                decimal stock   = Convert.ToDecimal(stockVal);
                decimal reorder = reorderVal == DBNull.Value ? 0 : Convert.ToDecimal(reorderVal);
                if (stock <= 0)           return "stock-zero";
                if (reorder > 0 && stock <= reorder) return "stock-low";
                return "stock-ok";
            }
            catch { return "stock-ok"; }
        }
    }
}
