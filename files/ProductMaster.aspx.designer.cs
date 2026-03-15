namespace StockApp
{
    public partial class ProductMaster
    {
        protected global::System.Web.UI.WebControls.TextBox        txtProductName;
        protected global::System.Web.UI.WebControls.TextBox        txtProductCode;
        protected global::System.Web.UI.WebControls.TextBox        txtMRP;
        protected global::System.Web.UI.WebControls.TextBox        txtHSN;
        protected global::System.Web.UI.WebControls.DropDownList   ddlGST;
        protected global::System.Web.UI.WebControls.Button         btnAdd;
        protected global::System.Web.UI.WebControls.GridView       gvProducts;
        protected global::System.Web.UI.WebControls.Panel          pnlMsg;
        protected global::System.Web.UI.WebControls.Label          lblMsg;
        protected global::System.Web.UI.WebControls.Panel          pnlErr;
        protected global::System.Web.UI.WebControls.Label          lblErr;
        protected global::System.Web.UI.WebControls.Panel          pnlEmpty;
        protected global::System.Web.UI.WebControls.Label          lblUserInfo;
        protected global::System.Web.UI.WebControls.LinkButton     btnDownloadTemplate;
        protected global::System.Web.UI.WebControls.FileUpload     fileProducts;
        protected global::System.Web.UI.WebControls.Button         btnBulkUpload;
        protected global::System.Web.UI.WebControls.Panel          pnlBulkResult;
        protected global::System.Web.UI.WebControls.Label          lblAdded;
        protected global::System.Web.UI.WebControls.Label          lblBulkSkipped;
        protected global::System.Web.UI.WebControls.Label          lblBulkErrors;
        protected global::System.Web.UI.WebControls.Label          lblBulkDetail;
        protected global::System.Web.UI.WebControls.Panel          pnlEditProduct;
        protected global::System.Web.UI.WebControls.HiddenField    hfEditProductId;
        protected global::System.Web.UI.WebControls.TextBox        txtEditProductName;
        protected global::System.Web.UI.WebControls.TextBox        txtEditProductCode;
        protected global::System.Web.UI.WebControls.TextBox        txtEditMRP;
        protected global::System.Web.UI.WebControls.TextBox        txtEditHSN;
        protected global::System.Web.UI.WebControls.DropDownList   ddlEditGST;
        protected global::System.Web.UI.WebControls.Button         btnEditSave;
        protected global::System.Web.UI.WebControls.Button         btnEditCancel;
    }
}
