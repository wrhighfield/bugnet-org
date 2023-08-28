using BugNET.BLL;
using BugNET.Common;
using System;
using System.Web;
using BugNET.UI;

namespace BugNET.Account
{
    public partial class Login : BugNetBasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Register_Localize.Text = GetLocalizedText(ResolveUrl("~/Account/Register.aspx"));
            OpenAuthLogin.ReturnUrl = Request.QueryString["ReturnUrl"];
            Form.DefaultButton = LoginView.FindControl("LoginButton").UniqueID;

            if (Convert.ToInt32(HostSettingManager.Get(HostSettingNames.UserRegistration)) ==
                (int) UserRegistration.None) Register_Localize.Visible = false;
        }

        private string GetLocalizedText(string linkUrl)
        {
            var returnUrl = HttpUtility.UrlEncode(Request.QueryString["ReturnUrl"]);
            if (!string.IsNullOrEmpty(returnUrl)) linkUrl += "?ReturnUrl=" + returnUrl;
            var messageFormat = GetLocalString("Register_MessageFormat");
            var linkText = GetLocalString("Register_LinkText");
            var link = $"<a href=\"{linkUrl}\">{Server.HtmlEncode(linkText)}</a>";
            return string.Format(messageFormat, link);
        }
    }
}