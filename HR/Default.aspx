<%@ Page Language="C#" AutoEventWireup="true" %>
<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        // Always go through HRLogin — it auto-redirects to HREmployee
        // if a valid HR session already exists.
        Response.Redirect("HRLogin.aspx", true);
    }
</script>
