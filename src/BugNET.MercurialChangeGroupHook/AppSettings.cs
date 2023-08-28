using System.Configuration;

namespace BugNET.MercurialChangeGroupHook
{
    internal static class AppSettings
    {
        public static string BugNetUserName
        {
            get
            {
                var value = ConfigurationManager.AppSettings["BugNetUsername"] ?? "";
                return value;
            }
        }

        public static string BugNetPassword
        {
            get
            {
                var value = ConfigurationManager.AppSettings["BugNetPassword"] ?? "";
                return value;
            }
        }

        public static string IssueIdRegEx
        {
            get
            {
                var value = ConfigurationManager.AppSettings["IssueIdRegEx"] ?? "";
                return value;
            }
        }

        public static string BugNetServicesUrl
        {
            get
            {
                var value = ConfigurationManager.AppSettings["BugNetServicesUrl"] ?? "";
                return value;
            }
        }

        public static bool BugNetWindowsAuthentication
        {
            get
            {
                var value = ConfigurationManager.AppSettings["BugNetWindowsAuthentication"] ?? "";
                return bool.TryParse(value, out var boolean) && boolean;
            }
        }
    }
}