using System;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;
using BugNET.BLL;
using BugNET.Common;
using BugNET.UI;
using log4net;

namespace BugNET.Account
{
    /// <summary>
    /// Password recovery page
    /// </summary>
    public partial class ForgotPassword : BugNetBasePage
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ForgotPassword));

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title =
                $@"{GetLocalString("Page.Title")} - {HostSettingManager.Get(HostSettingNames.ApplicationTitle)}";
        }

        /// <summary>
        /// Handles the Click event of the SubmitButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            var user = Membership.GetUser(UserName.Text.Trim());
            if (user != null && user.IsApproved)
            {
                var profile = new WebProfile().GetProfile(UserName.Text.Trim());
                var token = GenerateToken();
                profile.PasswordVerificationToken = token;
                profile.PasswordVerificationTokenExpirationDate = DateTime.Now.AddMinutes(1440);
                profile.Save();

                // Email the user the password reset token
                UserManager.SendForgotPasswordEmail(user, token);
            }

            forgotPassword.Visible = false;
            successMessage.Visible = true;
        }

        /// <summary>
        /// Generates the token.
        /// </summary>
        /// <returns></returns>
        private static string GenerateToken()
        {
            using (var provider = new RNGCryptoServiceProvider())
            {
                return GenerateToken(provider);
            }
        }

        /// <summary>
        /// Generates the token.
        /// </summary>
        /// <param name="generator">The generator.</param>
        /// <returns></returns>
        private static string GenerateToken(RandomNumberGenerator generator)
        {
            var tokenBytes = new byte[16];
            generator.GetBytes(tokenBytes);
            return HttpServerUtility.UrlTokenEncode(tokenBytes);
        }
    }
}