﻿// -----------------------------------------------------------------------
// <copyright file="SmtpMailDeliveryService.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Threading.Tasks;

namespace BugNET.BLL.Notifications
{
    using System;
    using System.Net.Mail;
    using System.Net;
    using Common;
    using log4net;

    public class SmtpMailDeliveryService : IMailDeliveryService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SmtpMailDeliveryService));

        /// <summary>
        /// Sends the specified recipient email.
        /// </summary>
        /// <param name="recipientEmail">The recipient email.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public async Task Send(string recipientEmail, MailMessage message, int? relatedIssueId)
        {
            var allowReplyTo = HostSettingManager.Get<bool>(HostSettingNames.Pop3AllowReplyToEmail, false);
            message.To.Clear();
            message.To.Add(recipientEmail);

            if (allowReplyTo && relatedIssueId.HasValue)
            {
                var at = HostSettingManager.HostEmailAddress.IndexOf("@", StringComparison.Ordinal);
                var issueCode = $"+iid-{relatedIssueId.Value}";
                message.From = new MailAddress(HostSettingManager.HostEmailAddress.Insert(at, issueCode),
                    HostSettingManager.ApplicationTitle);
            }
            else
            {
                message.From = new MailAddress(HostSettingManager.HostEmailAddress,
                    HostSettingManager.ApplicationTitle);
            }


            var smtpServer = HostSettingManager.SmtpServer;
            var smtpPort = int.Parse(HostSettingManager.Get(HostSettingNames.SMTPPort));
            var smtpAuthentication = Convert.ToBoolean(HostSettingManager.Get(HostSettingNames.SMTPAuthentication));
            var smtpUseSsl = bool.Parse(HostSettingManager.Get(HostSettingNames.SMTPUseSSL));

            // Only fetch the password if you need it
            var smtpUsername = string.Empty;
            var smtpPassword = string.Empty;
            var smtpDomain = string.Empty;

            if (smtpAuthentication)
            {
                smtpUsername = HostSettingManager.Get(HostSettingNames.SMTPUsername, string.Empty);
                smtpPassword = HostSettingManager.Get(HostSettingNames.SMTPPassword, string.Empty);
                smtpDomain = HostSettingManager.Get(HostSettingNames.SMTPDomain, string.Empty);
            }

            var client = new SmtpClient {Host = smtpServer, Port = smtpPort, EnableSsl = smtpUseSsl};

            if (smtpAuthentication)
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword, smtpDomain);
            }

            client.SendCompleted += (s, e) =>
            {
                if (e.Error != null)
                    // log the error message
                    Log.Error(e.Error);

                client.Dispose();
                message.Dispose();
            };

            await Task.Run(() =>
            {
                try
                {
                    client.SendAsync(message, null);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            });
        }
    }
}