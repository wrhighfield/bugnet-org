﻿using System;
using System.Web;
using System.Web.Security;
using Microsoft.AspNet.Membership.OpenAuth;
using BugNET.BLL;
using BugNET.Common;
using DotNetOpenAuth.GoogleOAuth2;

namespace BugNET.Account
{
    public partial class RegisterExternalLogin : System.Web.UI.Page
    {
        protected string ProviderName
        {
            get => (string) ViewState["ProviderName"] ?? string.Empty;
            private set => ViewState["ProviderName"] = value;
        }

        protected string ProviderDisplayName
        {
            get => (string) ViewState["ProviderDisplayName"] ?? string.Empty;
            private set => ViewState["ProviderDisplayName"] = value;
        }

        protected string ProviderUserId
        {
            get => (string) ViewState["ProviderUserId"] ?? string.Empty;
            private set => ViewState["ProviderUserId"] = value;
        }

        protected string ProviderUserName
        {
            get => (string) ViewState["ProviderUserName"] ?? string.Empty;
            private set => ViewState["ProviderUserName"] = value;
        }

        protected void Page_Load()
        {
            if (!IsPostBack) ProcessProviderResult();
        }

        protected void LogIn_Click(object sender, EventArgs e)
        {
            CreateAndLoginUser();
        }

        protected void cancel_Click(object sender, EventArgs e)
        {
            RedirectToReturnUrl();
        }

        private void ProcessProviderResult()
        {
            // Process the result from an auth provider in the request
            ProviderName = OpenAuth.GetProviderNameFromCurrentRequest();

            if (string.IsNullOrEmpty(ProviderName)) Response.Redirect(FormsAuthentication.LoginUrl);

            // Build the redirect url for OpenAuth verification
            var redirectUrl = "~/Account/RegisterExternalLogin";
            var returnUrl = Request.QueryString["ReturnUrl"];
            if (!string.IsNullOrEmpty(returnUrl)) redirectUrl += "?ReturnUrl=" + HttpUtility.UrlEncode(returnUrl);


            if (ProviderName == "Google") GoogleOAuth2Client.RewriteRequest();

            // Verify the OpenAuth payload
            var authResult = OpenAuth.VerifyAuthentication(redirectUrl);
            ProviderDisplayName = OpenAuth.GetProviderDisplayName(ProviderName);
            if (!authResult.IsSuccessful)
            {
                Title = "External login failed";
                userNameForm.Visible = false;

                providerMessage.Text = $"External login {ProviderDisplayName} failed,";

                // To view this error, enable page tracing in web.config (<system.web><trace enabled="true"/></system.web>) and visit ~/Trace.axd
                Trace.Warn("OpenAuth", $"There was an error verifying authentication with {ProviderDisplayName})",
                    authResult.Error);
                return;
            }

            // User has logged in with provider successfully
            // Check if user is already registered locally
            if (OpenAuth.Login(authResult.Provider, authResult.ProviderUserId, false)) RedirectToReturnUrl();

            // Store the provider details in ViewState
            ProviderName = authResult.Provider;
            ProviderUserId = authResult.ProviderUserId;
            ProviderUserName = authResult.UserName;

            // Strip the query string from action
            Form.Action = ResolveUrl(redirectUrl);

            if (User.Identity.IsAuthenticated)
            {
                // User is already authenticated, add the external login and redirect to return url
                OpenAuth.AddAccountToExistingUser(ProviderName, ProviderUserId, ProviderUserName, User.Identity.Name);
                RedirectToReturnUrl();
            }
            else
            {
                // Check if user registration is enabled
                if (Convert.ToInt32(HostSettingManager.Get(HostSettingNames.UserRegistration)) ==
                    (int) UserRegistration.None) Response.Redirect("~/AccessDenied.aspx", true);

                // Try to get the email from the provider
                string emailResult = null;
                authResult.ExtraData.TryGetValue("email", out emailResult);

                // User is new, ask for their desired membership name and email
                userName.Text = authResult.UserName;
                email.Text = emailResult;
            }
        }

        private void CreateAndLoginUser()
        {
            if (!IsValid) return;

            var createResult = OpenAuth.CreateUser(ProviderName, ProviderUserId, ProviderUserName, userName.Text);

            if (!createResult.IsSuccessful)
            {
                userNameMessage.Text = createResult.ErrorMessage;
            }
            else
            {
                var user = Membership.GetUser(userName.Text);
                user.Email = email.Text;
                Membership.UpdateUser(user);

                // User created & associated OK
                if (OpenAuth.Login(ProviderName, ProviderUserId, false)) RedirectToReturnUrl();
            }
        }

        private void RedirectToReturnUrl()
        {
            var returnUrl = Request.QueryString["ReturnUrl"];
            if (!string.IsNullOrEmpty(returnUrl) && OpenAuth.IsLocalUrl(returnUrl))
                Response.Redirect(returnUrl);
            else
                Response.Redirect("~/");
        }
    }
}