<%@ WebHandler Language="C#" Class="PPApp.KeepAlive" %>
using System.Web;
namespace PPApp
{
    public class KeepAlive : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            // Simply touching the session is enough to reset the timeout
            context.Response.ContentType = "text/plain";
            context.Response.Write("ok");
        }
        public bool IsReusable { get { return false; } }
    }
}
