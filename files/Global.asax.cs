using System;
using System.Web;
using System.Web.UI;

namespace StockApp
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Disable unobtrusive validation so ASP.NET validators
            // work without requiring jQuery on the page.
            ValidationSettings.UnobtrusiveValidationMode = UnobtrusiveValidationMode.None;
        }
    }
}
