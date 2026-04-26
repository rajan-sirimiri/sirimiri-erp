<%@ Page Language="C#" AutoEventWireup="true" %>
<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        // Clear HR-specific session keys (keep StockApp/ERPHome session intact)
        Session["HR_UserID"]   = null;
        Session["HR_Username"] = null;
        Session["HR_FullName"] = null;
        Session["HR_Role"]     = null;

        // Redirect back to HR login
        Response.Redirect("HRLogin.aspx", true);
    }
</script>
