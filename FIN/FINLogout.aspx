<%@ Page Language="C#" AutoEventWireup="true" %>
<script runat="server">
protected void Page_Load(object sender, EventArgs e)
{
    Session.Remove("FIN_UserID");
    Session.Remove("FIN_Username");
    Session.Remove("FIN_FullName");
    Session.Remove("FIN_Role");
    Response.Redirect("FINLogin.aspx");
}
</script>
