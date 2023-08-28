using System;
using System.Web;
using BugNET.UI;

namespace BugNET.Errors
{
    public partial class Error : BugNetBasePage
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            Label2.Text = string.Format(GetLocalString("Message1"), Page.ResolveUrl("~/Default.aspx"));
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.TemplateControl.Error"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnError(EventArgs e)
        {
            //// At this point we have information about the error
            var ctx = HttpContext.Current;

            var exception = ctx.Server.GetLastError();

            var errorInfo =
                "<br>Offending URL: " + ctx.Request.Url +
                "<br>Source: " + exception.Source +
                "<br>Message: " + exception.Message; // +
            //"<br>Stack trace: " + exception.StackTrace;

            ctx.Response.Write(errorInfo);

            //// --------------------------------------------------
            //// To let the page finish running we clear the error
            //// --------------------------------------------------
            ctx.Server.ClearError();

            base.OnError(e);
        }
    }
}