using System.Web.UI;

namespace BugNET.UI
{
    /// <summary>
    /// This class gives the programmer access to quick and easy redirects when 
    /// errors happen.
    /// </summary>
    public static class ErrorRedirector
    {
        /// <summary>
        /// Transfers to error page.
        /// </summary>
        public static void TransferToLoginPage(Page webPage)
        {
            webPage.Response.Redirect($"~/Account/Login.aspx?returnurl={webPage.Request.RawUrl}", true);
        }

        /// <summary>
        /// Transfers to NotFound page.
        /// </summary>
        public static void TransferToNotFoundPage(Page webPage)
        {
            webPage.Response.Redirect("~/Errors/NotFound", true);
        }

        /// <summary>
        /// Transfers to SomethingMissing page.
        /// </summary>
        public static void TransferToSomethingMissingPage(Page webPage)
        {
            webPage.Response.Redirect("~/Errors/SomethingMissing", true);
        }

        /// <summary>
        /// Transfers to SessionExpired page.
        /// </summary>
        public static void TransferToSessionExpiredPage(Page webPage)
        {
            webPage.Response.Redirect("~/Errors/SessionExpired", true);
        }

        /// <summary>
        /// Transfers to Error page.
        /// Shouldn't really need this.
        /// </summary>
        public static void TransferToErrorPage(Page webPage)
        {
            webPage.Response.Redirect("~/Errors/Error", true);
        }
    }
}