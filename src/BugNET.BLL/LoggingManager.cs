using System;
using System.Configuration;
using System.Web;
using BugNET.Common;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;

namespace BugNET.BLL
{
    /// <summary>
    /// Logging Class
    /// </summary>
    public static class LoggingManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LoggingManager));

        /// <summary>
        /// Configures the logging.
        /// </summary>
        public static void ConfigureLogging()
        {
            ConfigureAdoNetAppender();

            //if email notification of errors are enabled create a SMTP logging appender.
            if (HostSettingManager.Get(HostSettingNames.EmailErrors, false))
                ConfigureEmailLoggingAppender();
        }

        /// <summary>
        /// Configures the ADO net appender.
        /// </summary>
        private static void ConfigureAdoNetAppender()
        {
            // Get the Hierarchy object that organizes the loggers

            if (!(LogManager.GetRepository() is Hierarchy hierarchy)) return;
            //get ADONetAppender
            var adoAppender =
                (AdoNetAppender) hierarchy.Root.GetAppender("AdoNetAppender");

            if (adoAppender == null) return;
            adoAppender.ConnectionString = ConfigurationManager.ConnectionStrings["BugNET"].ConnectionString;
            adoAppender.ActivateOptions(); //refresh settings of appender
        }

        /// <summary>
        /// Adds the email logging appender.
        /// </summary>
        public static void ConfigureEmailLoggingAppender()
        {
            var hierarchy =
                (Hierarchy) LogManager.GetRepository();

            if (hierarchy == null) return;

            var appender = (SmtpAppender) hierarchy.Root.GetAppender("SmtpAppender") ?? new SmtpAppender();

            appender.Name = "SmtpAppender";
            appender.From = HostSettingManager.Get(HostSettingNames.HostEmailAddress, string.Empty);
            appender.To = HostSettingManager.Get(HostSettingNames.ErrorLoggingEmailAddress, string.Empty);
            appender.Subject = "BugNET Error";
            appender.SmtpHost = HostSettingManager.SmtpServer;
            appender.Port = int.Parse(HostSettingManager.Get(HostSettingNames.SMTPPort));
            appender.Authentication = SmtpAppender.SmtpAuthentication.None;
            appender.Username = string.Empty;
            appender.Password = string.Empty;
            appender.EnableSsl = bool.Parse(HostSettingManager.Get(HostSettingNames.SMTPUseSSL));

            if (Convert.ToBoolean(HostSettingManager.Get(HostSettingNames.SMTPAuthentication)))
            {
                appender.Authentication = SmtpAppender.SmtpAuthentication.Basic;
                appender.Username =
                    $"{HostSettingManager.Get(HostSettingNames.SMTPDomain, string.Empty)}\\{HostSettingManager.Get(HostSettingNames.SMTPUsername, string.Empty)}";
                appender.Password = HostSettingManager.Get(HostSettingNames.SMTPPassword, string.Empty);
            }

            appender.Priority = System.Net.Mail.MailPriority.High;
            appender.Threshold = log4net.Core.Level.Error;
            appender.BufferSize = 0;

            //create pattern layout
            var patternLayout = new
                log4net.Layout.PatternLayout(
                    "%newline%date [%thread] %-5level %logger [%property{NDC}] - %message%newline%newline%newline");

            patternLayout.ActivateOptions();
            appender.Layout = patternLayout;
            appender.ActivateOptions();

            //add appender to root logger
            hierarchy.Root.AddAppender(appender);
        }

        /// <summary>
        /// Removes the email logging appender.
        /// </summary>
        public static void RemoveEmailLoggingAppender()
        {
            if (!(LogManager.GetRepository() is Hierarchy hierarchy)) return;

            var appender =
                (SmtpAppender) hierarchy.Root.GetAppender("SmtpAppender");

            if (appender != null)
                hierarchy.Root.RemoveAppender("SmtpAppender");
        }

        /// <summary>
        /// Gets the error message resource.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetErrorMessageResource(string key)
        {
            return HttpContext.GetGlobalResourceObject("Exceptions", key) as string;
        }
    }
}