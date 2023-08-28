﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using BugNET.BLL;
using BugNET.Common;
using log4net;

namespace BugNET.Install
{
    public partial class Install : System.Web.UI.Page
    {
        private DateTime _startTime;

        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            //Get current Script time-out
            var scriptTimeOut = Server.ScriptTimeout;

            var mode = string.Empty;

            if (Request.QueryString["mode"] != null) mode = Request.QueryString["mode"].ToLower();

            //Disable Client side caching
            Response.Cache.SetCacheability(HttpCacheability.ServerAndNoCache);

            //Check mode is not Nothing
            if (mode == "none")
            {
                NoUpgrade();
            }
            else
            {
                //Set Script timeout to MAX value
                Server.ScriptTimeout = int.MaxValue;
                try
                {
                    var status = UpgradeManager.GetUpgradeStatus();

                    switch (status)
                    {
                        case UpgradeStatus.Install:
                            InstallApplication();
                            break;
                        case UpgradeStatus.Upgrade:
                            UpgradeApplication();
                            break;
                        case UpgradeStatus.None:
                            NoUpgrade();
                            break;
                        case UpgradeStatus.Authenticated:
                            InstallerLogout();
                            break;
                        default:
                            Log.Info($"The current status [{status}] was not handled during the install process.");
                            break;
                    }
                }
                finally
                {
                    //restore Script timeout
                    Server.ScriptTimeout = scriptTimeOut;
                }
            }
        }

        /// <summary>
        /// Logs the user out with a suitable error message.
        /// </summary>
        private void InstallerLogout()
        {
            var tmpuser = HttpContext.Current.User.Identity.Name;

            // Sign out before writing the headers!
            FormsAuthentication.SignOut();

            WriteHeader("logout");
            WriteMessage($"<h3>You were logged in as user '{tmpuser}'</h3>");
            WriteMessage("<h3>You have been logged out of the system automatically.</h3>");
            WriteMessage(
                "<br/><h2>Please make sure you are using Forms authentication and run the install process from a browser that doesn't have your login credentials remembered.</h2>");
            WriteMessage("<br/><h2><a href='../Install/Install.aspx'>Click Here to retry the installation.</a></h2>");
            WriteFooter();
        }


        /// <summary>
        /// Displayed information if no upgrade is necessary
        /// </summary>
        private void NoUpgrade()
        {
            WriteHeader("none");
            WriteMessage($"<h2>Current Database Version: {UpgradeManager.GetInstalledVersion()}</h2>");
            WriteMessage($"<h2>Current Assembly Version: {UpgradeManager.GetCurrentVersion()}</h2>");
            WriteMessage("<h2>No Upgrade needed.</h2>");
            WriteMessage(
                "<br/><br/><h2><a href='../Default.aspx'>Click Here To Access Your BugNET Installation</a></h2>");
            WriteFooter();
        }

        #region Install

        /// <summary>
        /// Installs the application.
        /// </summary>
        /// <returns></returns>
        private void InstallApplication()
        {
            _startTime = DateTime.Now;
            WriteHeader("install");

            WriteMessage($"<h2>Version: {UpgradeManager.GetCurrentVersion()}</h2>");
            WriteMessage("&nbsp;");
            WriteMessage("<h2>Installation Status Report</h2>");

            if (!InstallBugNET())
            {
                WriteMessage("<h2>Installation Failed!</h2>");
            }
            else
            {
                WriteMessage("<h2>Installation Complete</h2>");
                WriteMessage(
                    "<br/><br/><h2><a href='../Default.aspx'>Click Here To Access Your BugNET Installation</a></h2><br/><br/>");
            }

            Response.Flush();
            WriteFooter();
        }

        /// <summary>
        /// Installs the BugNET.
        /// </summary>
        /// <returns></returns>
        private bool InstallBugNET()
        {
            try
            {
                var providerPath = UpgradeManager.GetProviderPath();

                if (!providerPath.StartsWith("ERROR"))
                {
                    WriteMessage($"Installing Version: {UpgradeManager.GetCurrentVersion()}<br/>", 0, true);
                    WriteMessage("Installing BugNET Database:<br/>", 0, true);
                    ExecuteSqlInFile($"{providerPath}BugNET.Schema.SqlDataProvider.sql");
                    WriteMessage("Installing BugNET Default Data:<br/>", 0, true);
                    ExecuteSqlInFile($"{providerPath}BugNET.Data.SqlDataProvider.sql");
                    WriteMessage("Creating Administrator Account<br/>", 0, true);

                    //create admin user
                    MembershipCreateStatus status;

                    var newUser = Membership.CreateUser("Admin", "password", "admin@yourdomain.com", "no question",
                        "no answer", true, out status);

                    switch (status)
                    {
                        case MembershipCreateStatus.Success:
                            WriteMessage("Created Administrator Account", 0, true);
                            WriteScriptSuccessError(true);
                            break;
                        case MembershipCreateStatus.InvalidUserName:
                        case MembershipCreateStatus.InvalidPassword:
                        case MembershipCreateStatus.InvalidQuestion:
                        case MembershipCreateStatus.InvalidAnswer:
                        case MembershipCreateStatus.InvalidEmail:
                        case MembershipCreateStatus.DuplicateUserName:
                        case MembershipCreateStatus.DuplicateEmail:
                        case MembershipCreateStatus.UserRejected:
                        case MembershipCreateStatus.InvalidProviderUserKey:
                        case MembershipCreateStatus.DuplicateProviderUserKey:
                        case MembershipCreateStatus.ProviderError:
                            var message = $"Creating Administrator Account Failed, status returned: {status} <br/>";
                            WriteMessage(message, 0, true);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    WriteMessage("Creating Administrator Account default profile <br/>", 0, true);

                    if (status == MembershipCreateStatus.Success)
                    {
                        //add the admin user to the Super Users role.
                        RoleManager.AddUser("Admin", 1);

                        //add user profile information
                        var profile = new WebProfile().GetProfile("Admin");
                        profile.FirstName = "Admin";
                        profile.LastName = "Admin";
                        profile.DisplayName = "Administrator";
                        profile.PasswordVerificationTokenExpirationDate = null;
                        profile.Save();

                        WriteMessage("Created Administrator Account default profile", 0, true);
                        WriteScriptSuccessError(true);
                    }
                    else
                    {
                        WriteMessage(
                            "Created Administrator Account default profile failed, due to status returned from account creation",
                            0, true);
                        WriteScriptSuccessError(false);
                    }

                    UpgradeManager.UpdateDatabaseVersion(UpgradeManager.GetCurrentVersion());
                }
                else
                {
                    //upgrade error
                    Response.Write("<h2>Upgrade Error: " + providerPath + "</h2>");
                    return false;
                }
            }
            catch (Exception e)
            {
                WriteErrorMessage(e.Message);
                return false;
            }

            return true;
        }

        #endregion

        #region Upgrade

        /// <summary>
        /// Upgrades the application.
        /// </summary>
        private void UpgradeApplication()
        {
            _startTime = DateTime.Now;
            WriteHeader("upgrade");
            WriteMessage("<h2>Upgrade Status Report</h2>");
            WriteMessage($"<h2>Current Assembly Version: {UpgradeManager.GetCurrentVersion()}</h2>");
            WriteMessage($"<h2>Current Database Version: {UpgradeManager.GetInstalledVersion()}</h2>");
            WriteMessage($"Upgrading To Version: {UpgradeManager.GetCurrentVersion()}<br/>", 0, true);
            if (UpgradeBugNET())
            {
                WriteMessage("<h2>Upgrade Complete</h2>");
                WriteMessage("<h2><a href='../Default.aspx'>Click Here To Access Your BugNET Installation</a></h2>");

                var currentVersion = UpgradeManager.GetCurrentVersion();
                UpgradeManager.UpdateDatabaseVersion(currentVersion);

                // support for a version file to be loaded to display things like breaking changes or other info 
                // about the upgrade that was done.
                var installPath = Server.MapPath("~/Install");

                var versionFile = Path.Combine(installPath, $"{currentVersion}.htm");

                if (File.Exists(versionFile)) WriteMessage(File.ReadAllText(versionFile));
            }
            else
            {
                WriteMessage("<h2>Upgrade Failed!</h2>");
            }

            WriteFooter();
        }

        /// <summary>
        /// Upgrades the application.
        /// </summary>
        private bool UpgradeBugNET()
        {
            try
            {
                var providerPath = UpgradeManager.GetProviderPath();

                if (!providerPath.StartsWith("ERROR"))
                {
                    //get current App version
                    var assemblyVersion = Convert.ToInt32(UpgradeManager.GetCurrentVersion().Replace(".", ""));
                    var databaseVersion = Convert.ToInt32(UpgradeManager.GetInstalledVersion().Replace(".", ""));

                    //get list of script files
                    var arrScriptFiles = new ArrayList();

                    // wire up the custom field creation here based on the version number supported
                    // from here we need to create the custom field views
                    // doing this will not hurt the code if the code does not support it
                    if (databaseVersion <= 91610)
                    {
                        WriteMessage("Creating Custom Field Views<br/>", 0, true);
                        if (UpgradeManager.CreateCustomFieldViews())
                        {
                            WriteMessage("Custom fields created!<br/>", 0, true);
                        }
                        else
                        {
                            WriteMessage(
                                "There was an issue creating the custom fields views for your project, please view the application log for more details<br/>",
                                0, true);
                            WriteMessage(
                                "You can manually re-generate the custom field views by going to the <a href='../Administration/Projects/ProjectList.aspx'>Project List</a> page and using the generate feature along the top menu<br/>",
                                0, true);
                        }
                    }

                    var arrFiles = Directory.GetFiles(providerPath, "*.sql");

                    foreach (var file in arrFiles)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file);

                        if (string.IsNullOrEmpty(fileName)) continue;

                        fileName = fileName.ToLower().Trim();
                        if (fileName.Length.Equals(0)) continue;
                        if (fileName.StartsWith("install")) continue;
                        if (fileName.StartsWith("bugnet")) continue;
                        if (fileName.StartsWith("latest")) continue;

                        // not a version script
                        if (fileName.LastIndexOf(".").Equals(-1)) continue;

                        var strScriptVersion = fileName.Substring(0, fileName.LastIndexOf("."));
                        var scriptVersion = Convert.ToInt32(strScriptVersion.Replace(".", ""));

                        //check if script file is relevant for upgrade
                        if (scriptVersion > databaseVersion && scriptVersion <= assemblyVersion)
                            arrScriptFiles.Add(file);
                    }

                    arrScriptFiles.Sort();

                    foreach (var scriptFile in arrScriptFiles.Cast<string>()
                                 .Where(strScriptFile => databaseVersion != assemblyVersion))
                        //execute script file (and version upgrades) for version
                        ExecuteSqlInFile(scriptFile);

                    //check if the admin user is in the super users role.
                    var found = false;
                    var roles = RoleManager.GetForUser("Admin");
                    if (roles.Count > 0)
                    {
                        var role = roles.SingleOrDefault(r => r.Name == Globals.SuperUserRole);
                        if (role != null) found = true;
                    }

                    if (!found) RoleManager.AddUser("Admin", 1);

                    UpgradeManager.UpdateDatabaseVersion(UpgradeManager.GetCurrentVersion());

                    return true;
                }

                //upgrade error
                Response.Write("<h2>Upgrade Error: " + providerPath + "</h2>");
                return false;
            }
            catch (Exception e)
            {
                WriteErrorMessage(e.Message);
                return false;
            }
        }

        #endregion

        #region Script Functions

        /// <summary>
        /// Executes the SQL in file.
        /// </summary>
        /// <param name="pathToScriptFile">The path to script file.</param>
        /// <returns></returns>
        private void ExecuteSqlInFile(string pathToScriptFile)
        {
            WriteMessage($"Executing Script: {pathToScriptFile.Substring(pathToScriptFile.LastIndexOf("\\") + 1)}", 2,
                true);

            try
            {
                var statements = new List<string>();

                if (false == File.Exists(pathToScriptFile))
                    throw new Exception($"File {pathToScriptFile} does not exist!");

                using (Stream stream = File.OpenRead(pathToScriptFile))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        string statement;
                        while ((statement = ReadNextStatementFromStream(reader)) != null) statements.Add(statement);
                    }
                }

                UpgradeManager.ExecuteStatements(statements);

                WriteScriptSuccessError(true);
            }
            catch (Exception ex)
            {
                WriteScriptSuccessError(false);
                WriteScriptErrorMessage(pathToScriptFile.Substring(pathToScriptFile.LastIndexOf("\\") + 1), ex.Message);
            }
        }

        /// <summary>
        /// Reads the next statement from stream.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        private static string ReadNextStatementFromStream(StreamReader reader)
        {
            var sb = new StringBuilder();

            while (true)
            {
                var lineOfText = reader.ReadLine();
                if (lineOfText == null) return sb.Length > 0 ? sb.ToString() : null;
                if (lineOfText.TrimEnd().ToUpper() == "GO")
                    break;

                sb.Append(lineOfText + Environment.NewLine);
            }

            return sb.ToString();
        }

        #endregion

        #region Html Utility Functions

        /// <summary>
        /// Writes the footer.
        /// </summary>
        private void WriteFooter()
        {
            Response.Write("</body>");
            Response.Write("</html>");
            Response.Flush();
        }

        /// <summary>
        /// Writes the html header.
        /// </summary>
        /// <param name="mode">The mode.</param>
        private void WriteHeader(string mode)
        {
            //read install page and insert into response stream
            if (File.Exists(HttpContext.Current.Server.MapPath("~/Install/Install.htm")))
            {
                var oStreamReader = File.OpenText(HttpContext.Current.Server.MapPath("~/Install/Install.htm"));
                var sHtml = oStreamReader.ReadToEnd();
                oStreamReader.Close();
                Response.Write(sHtml);
            }

            switch (mode)
            {
                case "install":
                    Response.Write("<h1>Installing BugNET</h1>");
                    break;
                case "upgrade":
                    Response.Write("<h1>Upgrading BugNET</h1>");
                    break;
                case "none":
                    Response.Write("<h1>Nothing To Install At This Time</h1>");
                    break;
                case "logout":
                    Response.Write("<h1>Logged out</h1>");
                    break;
                case "authentication":
                    Response.Write("<h1>Windows Authentication Detected</h1>");
                    break;
            }

            Response.Flush();
        }

        /// <summary>
        /// Writes the error message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void WriteErrorMessage(string message)
        {
            HttpContext.Current.Response.Write($"<br/><br/><font color='red'>Error: {message}</font>");
            HttpContext.Current.Response.Flush();
        }

        /// <summary>
        /// Writes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="indent">How many spaces to indent the text by</param>
        /// <param name="showTime">if set to <c>true</c> [show time].</param>
        private void WriteMessage(string message, int indent = 0, bool showTime = false)
        {
            if (message.Trim().Length.Equals(0)) return;

            var spacer = string.Empty;
            for (var i = 0; i < indent; i++)
                spacer += "&nbsp;";

            if (showTime)
                message = string.Format("{1} - {2} {0} ", message, DateTime.Now.Subtract(_startTime), spacer);

            HttpContext.Current.Response.Write(message);
            HttpContext.Current.Response.Flush();
        }

        /// <summary>
        /// Writes the success error message.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        private void WriteScriptSuccessError(bool success)
        {
            WriteMessage(success ? "<font color='green'>Success</font><br/>" : "<font color='red'>Error!</font><br/>");
        }


        /// <summary>
        /// Writes the error message.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="message">The message.</param>
        private void WriteScriptErrorMessage(string file, string message)
        {
            HttpContext.Current.Response.Write("<h2>Error Details</h2>");
            HttpContext.Current.Response.Write(
                "<table style='color:red;font-size:11px' cellspacing='0' cellpadding='0' border='0'>");
            HttpContext.Current.Response.Write("<tr><td>File</td><td>" + file + "</td></tr>");
            HttpContext.Current.Response.Write($"<tr><td>Error&nbsp;&nbsp;</td><td>{message}</td></tr>");
            HttpContext.Current.Response.Write("</table>");
            HttpContext.Current.Response.Write("<br><br>");
            HttpContext.Current.Response.Flush();
        }

        #endregion
    }
}