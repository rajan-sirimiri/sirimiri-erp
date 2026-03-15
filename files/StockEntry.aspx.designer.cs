namespace StockApp
{
    public partial class StockEntry
    {
        protected global::System.Web.UI.WebControls.DropDownList    ddlState;
        protected global::System.Web.UI.WebControls.DropDownList    ddlCity;
        protected global::System.Web.UI.WebControls.DropDownList    ddlDistributor;
        protected global::System.Web.UI.WebControls.TextBox         txtCurrentStock;
        protected global::System.Web.UI.WebControls.Button          btnSubmit;
        protected global::System.Web.UI.WebControls.Panel           pnlSuccess;
        protected global::System.Web.UI.WebControls.Panel           pnlError;
        protected global::System.Web.UI.WebControls.Label           lblSuccess;
        protected global::System.Web.UI.WebControls.Label           lblError;
        protected global::System.Web.UI.WebControls.Panel           pnlAddress;
        protected global::System.Web.UI.WebControls.Panel           pnlPin;
        protected global::System.Web.UI.WebControls.Label           lblAddress;
        protected global::System.Web.UI.WebControls.Label           lblPin;
        protected global::System.Web.UI.HtmlControls.HtmlGenericControl divSummary;
        protected global::System.Web.UI.WebControls.Label           lblPeriodTitle;
        protected global::System.Web.UI.WebControls.Label           lblChartDays;
        protected global::System.Web.UI.WebControls.Label           lblPaymentDays;
        protected global::System.Web.UI.WebControls.Label           lblCreditDays;
        protected global::System.Web.UI.WebControls.Label           lblSummaryOrders;
        protected global::System.Web.UI.WebControls.Label           lblSummaryUnits;
        protected global::System.Web.UI.WebControls.Label           lblSummaryValue;
        protected global::System.Web.UI.WebControls.Label           lblLastOrder;
        protected global::System.Web.UI.WebControls.Label           lblCreditTotal;
        protected global::System.Web.UI.WebControls.Label           lblCredit60;
        protected global::System.Web.UI.WebControls.Label           lblLastPayment;
        protected global::System.Web.UI.WebControls.Label           lblOutstanding;
        protected global::System.Web.UI.WebControls.HiddenField     hfChartData;
        protected global::System.Web.UI.WebControls.HiddenField     hfPaymentData;
        protected global::System.Web.UI.WebControls.HiddenField     hfShowOrderChart;
        protected global::System.Web.UI.WebControls.HiddenField     hfShowPaymentChart;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvState;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvCity;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvDistributor;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvStock;
        protected global::System.Web.UI.WebControls.RangeValidator         rvStock;
        protected global::System.Web.UI.WebControls.ValidationSummary      valSummary;
        protected global::System.Web.UI.WebControls.RadioButtonList          rblPeriod;
        protected global::System.Web.UI.WebControls.Panel                    pnlClosingStock;
        protected global::System.Web.UI.WebControls.HyperLink                lnkGoogleMap;
        protected global::System.Web.UI.WebControls.Label                    lblClosingDate;
        protected global::System.Web.UI.WebControls.Label                    lblClosingUnits;
        protected global::System.Web.UI.WebControls.Panel                    pnlAdminMenu;
    }
}
