using System.Threading;
using System.Web;
using System.Web.UI;

namespace BugNET.UI
{
    /// <summary>
    /// This class gives the programmer access to quick and easy redirects when 
    /// errors happen.
    /// </summary>
    public static class ErrorRedirectHelper
    {
        /// <summary>
        /// Transfers to error page.
        /// </summary>
        public static void TransferToLoginPage(HttpContext httpContext)
        {
            TransferToUrl(httpContext, $"~/Account/Login.aspx?returnurl={httpContext.Request.RawUrl}");
        }

        /// <summary>
        /// Transfers to NotFound page.
        /// </summary>
        public static void TransferToNotFoundPage(HttpContext httpContext)
        {
            TransferToUrl(httpContext, "~/Errors/NotFound");
        }

        /// <summary>
        /// Transfers to SomethingMissing page.
        /// </summary>
        public static void TransferToSomethingMissingPage(HttpContext httpContext)
        {
            TransferToUrl(httpContext, "~/Errors/SomethingMissing");
        }

        /// <summary>
        /// Transfers to SessionExpired page.
        /// </summary>
        public static void TransferToSessionExpiredPage(HttpContext httpContext)
        {
            TransferToUrl(httpContext, "~/Errors/SessionExpired");
        }

        /// <summary>
        /// Transfers to Error page.
        /// Shouldn't really need this.
        /// </summary>
        public static void TransferToErrorPage(HttpContext httpContext)
        {
            TransferToUrl(httpContext, "~/Errors/Error");
        }

        private static void TransferToUrl(HttpContext httpContext, string url)
        {
            try
            {
                httpContext.Response.Redirect(url, false);
            }
            catch (ThreadAbortException)
            {
                httpContext.ApplicationInstance.CompleteRequest();
            }
        }
    }
}