using System;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace BugNET.Common
{
    public static class Utilities
    {
        /// <summary>
        /// Validates an string for a valid email
        /// </summary>
        /// <param name="email">The string to validate</param>
        /// <returns></returns>
        public static bool IsValidEmail(string email)
        {
            var validator = new IsEMail();
            return validator.IsEmailValid(email);

            //uses regex from the asp.net MVC email address attribute
            // Regex regex = new Regex(@"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // return regex.Match(email).Length > 0;
        }

        /// <summary>
        /// Parses a BugNet status code from a database raise error exception
        /// </summary>
        /// <param name="errorMessage">The error message from the database</param>
        /// <returns></returns>
        public static int ParseDatabaseStatusCode(string errorMessage)
        {
            if (!errorMessage.StartsWith("BNCode")) return 0;

            // at this point we have a code so we have to parse it out
            var parts = errorMessage.Split(' ');

            if (parts.Length <= 0) return DownloadAttachmentStatusCodes.NoAccess.To<int>();
            var statusCodeParts = parts[0].Split(':');

            if (!statusCodeParts.Length.Equals(2)) return DownloadAttachmentStatusCodes.NoAccess.To<int>();
            var statusCode = statusCodeParts[1];

            // if we cannot convert the code to a proper status code then do the safe thing and not allow access
            return statusCode.ToOrDefault(DownloadAttachmentStatusCodes.NoAccess.To<int>());

            // if we cannot parse the code out then do the safe thing and not allow access
        }

        /// <summary>
        /// Parses the full issue id.
        /// </summary>
        /// <param name="fullId">The full id.</param>
        /// <returns></returns>
        public static int ParseFullIssueId(string fullId)
        {
            var lastDashPos = fullId.LastIndexOf("-", StringComparison.Ordinal);

            if (lastDashPos > 0) 
                fullId = fullId.Substring(lastDashPos+1);

            return fullId.ToOrDefault(-1);
        }

        /// <summary>
        /// This checks the Project upload path within the context of the 
        /// BugNET application.
        /// 
        /// Plugs numerous security holes.
        /// 
        /// BGN-1909
        /// BGN-1905
        /// BGN-1904
        /// </summary>
        /// <param name="sPath"></param>
        /// <returns></returns>
        public static bool CheckUploadPath(string sPath)
        {
            var isPathValid = false;
            var tmpPath = sPath; // don't even trim it!

            // BGN-1904
            // Check the length of the upload path
            // 64 characters are allows            
            if ((tmpPath.Length > Globals.UploadFolderLimit))
            {
                isPathValid = true;
            }

            // Now check for funny characters but there is a slight problem.

            // The string paths are "~\Uploads\Project1\"
            // The "\\" is seen as a UNC path and marked invalid
            // However our encoding defines a UNC path as "\\"
            // So we have to do some magic first

            // Reject any UNC paths
            if (tmpPath.Contains(@"\\"))
            {
                isPathValid = true;
            }

            // Reject attempts to traverse directories
            if ((tmpPath.Contains(@"\..")) ||
                (tmpPath.Contains(@"..\")) || (tmpPath.Contains(@"\.\")))
            {
                isPathValid = true;
            }

            // Now that there are just folders left, remove the "\" character
            tmpPath = tmpPath.Replace(@"\", " ");

            //check for illegal filename characters
            if (tmpPath.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                isPathValid = true;
            }

            // Return the opposite of norty
            return !isPathValid;
        }

        public enum ApplicationSettingKeys
        {
                InstallationDate,
                NotFoundUrl,
                SomethingMissingUrl,
                SessionExpiredUrl,
                ErrorUrl,
                AccessDeniedUrl
        }

        /// <summary>
        /// Gets the app setting.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static string GetApplicationSetting(ApplicationSettingKeys key, string defaultValue) =>
            ConfigurationManager.AppSettings.Get(key.ToString(), defaultValue);

        /// <summary>
        /// Gets the boolean as string.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <returns></returns>
        public static string GetBooleanAsString(bool value)
        {
            var boolString = (value) ? bool.TrueString : bool.FalseString;
            return ResourceStrings.GetGlobalResource(GlobalResources.SharedResources, boolString);
        }

        /// <summary>
        /// Estimations to string.
        /// </summary>
        /// <param name="estimation">The estimation.</param>
        /// <returns></returns>
        public static string EstimationToString(decimal estimation)
        {
            return estimation >= 0
                ? estimation.ToString(CultureInfo.InvariantCulture)
                : ResourceStrings.GetGlobalResource(GlobalResources.SharedResources, "Empty", "Empty").ToLower();
        }

        /// <summary>
        /// Strips the HTML. BGN-1732
        /// 
        /// This should be in a helper class
        /// 
        /// From http://www.codeproject.com/Articles/68222/Strip-HTML-Tags-from-Text.aspx
        /// Copyright Raymund Macaalay | 25 Mar 2010
        /// http://nz.linkedin.com/in/macaalay
        /// http://anyrest.wordpress.com/
        /// </summary>
        /// <param name="sInputString">The s input string.</param>
        /// <returns></returns>
        public static string StripHtml(string sInputString)
        {
            try
            {
                var sOutputString = sInputString;
                //Initial Cleaning Step
                //Replace new line and carriage return with Spaces
                sOutputString = sOutputString.Replace("\r", " ");
                sOutputString = sOutputString.Replace("\n", " ");
                // Remove sTabs
                sOutputString = sOutputString.Replace("\t", string.Empty);

                //Tag Removal
                var myDataTable = GetTableDefinition();
                myDataTable.DefaultView.Sort = "iID ASC";
                foreach (DataRow drCleaningItem in myDataTable.Rows)
                {
                    var sOriginalString = (drCleaningItem["sOriginalString"]).ToString();
                    var sReplacementString = (drCleaningItem["sReplacementString"]).ToString();
                    sOutputString = Regex.Replace
                        (sOutputString, sOriginalString, sReplacementString, RegexOptions.IgnoreCase);
                }

                //Initial replacement target string for line-breaks
                var sBreaks = "\r\r\r";

                // Initial replacement target string for sTabs
                var sTabs = "\t\t\t\t\t";
                for (var x = 0; x < sOutputString.Length; x++)
                {
                    sOutputString = sOutputString.Replace(sBreaks, "\r\r");
                    sOutputString = sOutputString.Replace(sTabs, "\t\t\t\t");
                    sBreaks += "\r";
                    sTabs += "\t";
                }

                return sOutputString;
            }
            catch
            {
                return sInputString;
            }
        }

        /// <summary>
        /// Gets the table definition. BGN-1732
        /// 
        /// Needs System.Data :(
        /// 
        /// This should be in a helper class
        /// 
        /// From http://www.codeproject.com/Articles/68222/Strip-HTML-Tags-from-Text.aspx
        /// Copyright Raymund Macaalay | 25 Mar 2010
        /// http://nz.linkedin.com/in/macaalay
        /// http://anyrest.wordpress.com/
        /// </summary>
        /// <returns></returns>
        private static DataTable GetTableDefinition()
        {
            var table = new DataTable();
            table.Columns.Add("iID", typeof(int));
            table.Columns.Add("sOriginalString", typeof(string));
            table.Columns.Add("sReplacementString", typeof(string));

            // Replace repeating spaces with single space
            table.Rows.Add(1, @"( )+", " ");

            // Prepare and clean Header Tag
            table.Rows.Add(2, @"<( )*head([^>])*>", "<head>");
            table.Rows.Add(3, @"(<( )*(/)( )*head( )*>)", "</head>");
            table.Rows.Add(4, "(<head>).*(</head>)", string.Empty);

            // Prepare and clean Script Tag
            table.Rows.Add(5, @"<( )*script([^>])*>", "<script>");
            table.Rows.Add(6, @"(<( )*(/)( )*script( )*>)", "</script>");
            table.Rows.Add(7, @"(<script>).*(</script>)", string.Empty);

            // Prepare and clean Style Tag
            table.Rows.Add(8, @"<( )*style([^>])*>", "<style>");
            table.Rows.Add(9, @"(<( )*(/)( )*style( )*>)", "</style>");
            table.Rows.Add(10, "(<style>).*(</style>)", string.Empty);

            // Replace <td> with sTabs
            table.Rows.Add(11, @"<( )*td([^>])*>", "\t");

            // Replace <BR> and <LI> with Line sBreaks
            table.Rows.Add(12, @"<( )*br( )*>", "\r");
            table.Rows.Add(13, @"<( )*li( )*>", "\r");

            // Replace <P>, <DIV> and <TR> with Double Line sBreaks
            table.Rows.Add(14, @"<( )*div([^>])*>", "\r\r");
            table.Rows.Add(15, @"<( )*tr([^>])*>", "\r\r");
            table.Rows.Add(16, @"<( )*p([^>])*>", "\r\r");

            // Remove Remaining tags enclosed in < >
            table.Rows.Add(17, @"<[^>]*>", string.Empty);

            // Replace special characters:
            table.Rows.Add(18, @" ", " ");
            table.Rows.Add(19, @"&bull;", " * ");
            table.Rows.Add(20, @"&lsaquo;", "<");
            table.Rows.Add(21, @"&rsaquo;", ">");
            table.Rows.Add(22, @"&trade;", "(tm)");
            table.Rows.Add(23, @"&frasl;", "/");
            table.Rows.Add(24, @"&lt;", "<");
            table.Rows.Add(25, @"&gt;", ">");
            table.Rows.Add(26, @"&copy;", "(c)");
            table.Rows.Add(27, @"&reg;", "(r)");
            table.Rows.Add(28, @"&frac14;", "1/4");
            table.Rows.Add(29, @"&frac12;", "1/2");
            table.Rows.Add(30, @"&frac34;", "3/4");
            table.Rows.Add(31, @"&lsquo;", "'");
            table.Rows.Add(32, @"&rsquo;", "'");
            table.Rows.Add(33, @"&ldquo;", "\"");
            table.Rows.Add(34, @"&rdquo;", "\"");

            // Remove all others remaining special characters
            // you don't want to replace with another string
            table.Rows.Add(35, @"&(.{2,6});", string.Empty);

            // Remove extra line sBreaks and sTabs
            table.Rows.Add(36, "(\r)( )+(\r)", "\r\r");
            table.Rows.Add(37, "(\t)( )+(\t)", "\t\t");
            table.Rows.Add(38, "(\t)( )+(\r)", "\t\r");
            table.Rows.Add(39, "(\r)( )+(\t)", "\r\t");
            table.Rows.Add(40, "(\r)(\t)+(\r)", "\r\r");
            table.Rows.Add(41, "(\r)(\t)+", "\r\t");

            return table;
        }
    }
}
