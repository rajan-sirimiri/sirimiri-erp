<%@ WebHandler Language="C#" Class="KeepAlive" %>
using System.Web;

public class KeepAlive : IHttpHandler, System.Web.SessionState.IRequiresSessionState
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "text/plain";
        context.Response.Write("ok");
    }
    public bool IsReusable { get { return false; } }
}
