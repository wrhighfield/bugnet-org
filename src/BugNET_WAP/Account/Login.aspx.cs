﻿using BugNET.BLL;
using BugNET.Common;
using System;
using System.Web;
using System.Web.UI;

namespace BugNET.Account
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Register_Localize.Text = GetLocalizedText(ResolveUrl("~/Account/Register.aspx"));
            OpenAuthLogin.ReturnUrl = Request.QueryString["ReturnUrl"];
            this.Form.DefaultButton = this.LoginView.FindControl("LoginButton").UniqueID;

            if (Convert.ToInt32(HostSettingManager.Get(HostSettingNames.UserRegistration)) == (int)UserRegistration.None)
            {
                Register_Localize.Visible = false;
            }

        }

        private string GetLocalizedText(string linkUrl)
        {
            var returnUrl = HttpUtility.UrlEncode(Request.QueryString["ReturnUrl"]);
            if (!String.IsNullOrEmpty(returnUrl))
            {
                linkUrl += "?ReturnUrl=" + returnUrl;
            }
            string messageFormat = GetLocalResourceObject("Register_MessageFormat").ToString();
            string linkText = GetLocalResourceObject("Register_LinkText").ToString();
            string link = String.Format("<a href=\"{0}\">{1}</a>", linkUrl, Server.HtmlEncode(linkText));
            return String.Format(messageFormat, link);
        }
    }
}