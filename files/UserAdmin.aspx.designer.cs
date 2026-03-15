namespace StockApp
{
    public partial class UserAdmin
    {
        protected global::System.Web.UI.WebControls.Panel              pnlMsg;
        protected global::System.Web.UI.WebControls.Label              lblMsg;
        protected global::System.Web.UI.WebControls.Panel              pnlAddUser;
        protected global::System.Web.UI.WebControls.TextBox            txtFullName;
        protected global::System.Web.UI.WebControls.TextBox            txtUsername;
        protected global::System.Web.UI.WebControls.TextBox            txtTempPwd;
        protected global::System.Web.UI.WebControls.DropDownList       ddlRole;
        protected global::System.Web.UI.HtmlControls.HtmlGenericControl divState;
        protected global::System.Web.UI.WebControls.DropDownList       ddlState;
        protected global::System.Web.UI.WebControls.Button             btnAdd;
        protected global::System.Web.UI.WebControls.GridView           gvUsers;
        protected global::System.Web.UI.WebControls.Panel             pnlBulkUpload;
        protected global::System.Web.UI.WebControls.FileUpload        fileUsers;
        protected global::System.Web.UI.WebControls.Button            btnBulkUpload;
        protected global::System.Web.UI.WebControls.LinkButton        btnDownloadTemplate;
        protected global::System.Web.UI.WebControls.Panel             pnlBulkResult;
        protected global::System.Web.UI.WebControls.Label             lblCreated;
        protected global::System.Web.UI.WebControls.Label             lblSkipped;
        protected global::System.Web.UI.WebControls.Label             lblBulkErrors;
        protected global::System.Web.UI.WebControls.Label             lblBulkDetail;
        protected global::System.Web.UI.WebControls.Panel              pnlEditUser;
        protected global::System.Web.UI.WebControls.HiddenField        hfEditUserId;
        protected global::System.Web.UI.WebControls.TextBox            txtEditFullName;
        protected global::System.Web.UI.WebControls.TextBox            txtEditUsername;
        protected global::System.Web.UI.WebControls.DropDownList       ddlEditRole;
        protected global::System.Web.UI.HtmlControls.HtmlGenericControl divEditState;
        protected global::System.Web.UI.WebControls.DropDownList       ddlEditState;
        protected global::System.Web.UI.WebControls.Button             btnEditUserSave;
        protected global::System.Web.UI.WebControls.Button             btnEditUserCancel;
        protected global::System.Web.UI.WebControls.DropDownList       ddlEditManager;
    }
}
