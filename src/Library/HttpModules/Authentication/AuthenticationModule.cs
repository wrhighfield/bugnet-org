using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using BugNET.BLL;
using BugNET.Common;
using log4net;

namespace BugNET.HttpModules
{
	/// <summary>
	/// BugNET Authentication HttpModule
	/// </summary>
	public class AuthenticationModule : IHttpModule
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(AuthenticationModule));
        private static string _path = string.Empty;
		private const string VALID_EMAIL_REGULAR_EXPRESSION = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";

		/// <summary>
		/// Gets the name of the module.
		/// </summary>
		/// <value>The name of the module.</value>
		public string ModuleName => "AuthenticationModule";

        #region IHttpModule Members

		/// <summary>
		/// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"></see>.
		/// </summary>
		public void Dispose()
		{ }

		/// <summary>
		/// Initializes a module and prepares it to handle requests.
		/// </summary>
		/// <param name="context">An <see cref="T:System.Web.HttpApplication"></see> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application</param>
		public void Init(HttpApplication context)
		{
			context.AuthenticateRequest += ContextAuthenticateRequest;
		}

		/// <summary>
		/// Handles the AuthenticateRequest event of the context control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        private static void ContextAuthenticateRequest(object sender, EventArgs e)
		{
            if (HttpContext.Current.User == null)
            {
                return;
            }

			//check if we are upgrading/installing
			if (HttpContext.Current.Request.Url.LocalPath.ToLower().EndsWith("install.aspx"))
				return;

			//get host settings
			var enabled = HostSettingManager.Get(HostSettingNames.UserAccountSource) == "ActiveDirectory" || HostSettingManager.Get(HostSettingNames.UserAccountSource) == "WindowsSAM";

			//check if windows authentication is enabled in the host settings
            if (!enabled) return;
            if (HttpContext.Current.User != null)
                MDC.Set("user", HttpContext.Current.User.Identity.Name);

            // This was moved from outside "if enabled" to only happen when we need it.
            var request = HttpContext.Current.Request;

            // not needed to be removed
            // HttpResponse response = HttpContext.Current.Response;

            if (!request.IsAuthenticated) return;

            if (HttpContext.Current.User.Identity.AuthenticationType != "NTLM" &&
                HttpContext.Current.User.Identity.AuthenticationType != "Negotiate") return;

            //check if the user exists in the database 
            var user = UserManager.GetUser(HttpContext.Current.User.Identity.Name);

            if (user == null)
            {
                try
                {
                    var userProperties = GetUserProperties(HttpContext.Current.User.Identity.Name);
                    MembershipUser membershipUser;
                    var createStatus = MembershipCreateStatus.Success;

                    //create a new user with the current identity and a random password.
                    membershipUser = Membership.RequiresQuestionAndAnswer
                        ? Membership.CreateUser(HttpContext.Current.User.Identity.Name, Membership.GeneratePassword(7, 2), userProperties.Email, "WindowsAuth", "WindowsAuth", true, out createStatus)
                        : Membership.CreateUser(HttpContext.Current.User.Identity.Name, Membership.GeneratePassword(7, 2), userProperties.Email);

                    if (createStatus != MembershipCreateStatus.Success || membershipUser == null) return;
                    var profile = new WebProfile().GetProfile(HttpContext.Current.User.Identity.Name);
                    profile.DisplayName = !string.IsNullOrWhiteSpace(userProperties.DisplayName)
                        ? userProperties.DisplayName
                        : $"{userProperties.FirstName} {userProperties.LastName}";
                    profile.FirstName = userProperties.FirstName;
                    profile.LastName = userProperties.LastName;
                    profile.Save();

                    //auto assign user to roles
                    var roles = RoleManager.GetAll().FindAll(r => r.AutoAssign);
                    foreach (var r in roles)
                        RoleManager.AddUser(membershipUser.UserName, r.Id);
                }
                catch (Exception ex)
                {
                    if (Log.IsErrorEnabled)
                        Log.Error(
                            $"Unable to add new user '{HttpContext.Current.User.Identity.Name}' to BugNET application. Authentication Type='{HttpContext.Current.User.Identity.AuthenticationType}'.", ex);
                }
            }
            else
            {
                //update the user's last login date.
                user.LastLoginDate = DateTime.Now;
                Membership.UpdateUser(user);
            }
        }

		/// <summary>
		/// Gets the users properties from the specified user store.
		/// </summary>
		/// <param name="identification">The identification.</param>
		/// <returns>
		/// Class of user properties
		/// </returns>
        private static UserProperties GetUserProperties(string identification)
        {
            var userProperties = new UserProperties
            {
                FirstName = identification
            };

            // Determine which method to use to retrieve user information

            switch (HostSettingManager.UserAccountSource)
            {
                // WindowsSAM
                case "WindowsSAM":
                {
                    // Extract the machine or domain name and the user name from the
                    // identification string
                    var samPath = identification.Split(new[] { '\\' });
                    _path = $"WinNT://{samPath[0]}";
                    try
                    {
                        // Find the user
                        var entryRoot = new DirectoryEntry(_path);
                        var userEntry = entryRoot.Children.Find(samPath[1], "user");
                        userProperties.FirstName = userEntry.Properties["FullName"].Value.ToString();
                        userProperties.Email = string.Empty;
                        return userProperties;
                    }
                    catch
                    {
                        return userProperties;
                    }
                }
                // Active Directory
                case "ActiveDirectory":
                {
                    // Setup the filter
                    var username = identification.Substring(identification.LastIndexOf(@"\", StringComparison.Ordinal) + 1, identification.Length - identification.LastIndexOf(@"\", StringComparison.Ordinal) - 1);
                    var domain = HostSettingManager.Get(HostSettingNames.ADPath);

                    var login = HostSettingManager.Get(HostSettingNames.ADUserName);
                    var password = HostSettingManager.Get(HostSettingNames.ADPassword);

                    var domainContext = string.IsNullOrWhiteSpace(login)
                        ? new PrincipalContext(ContextType.Domain, domain)
                        : new PrincipalContext(ContextType.Domain, domain, login, password);

                        var user = UserPrincipal.FindByIdentity(domainContext, username);
                        if (user == null) return userProperties;

                        // extract full name
                        if (!string.IsNullOrWhiteSpace(user.GivenName) || !string.IsNullOrWhiteSpace(user.Surname))
                        {
                            userProperties.FirstName = user.GivenName == null ? "" : user.GivenName.Trim();
                            userProperties.LastName = user.Surname == null ? "" : user.Surname.Trim();
                        }

                        if (!string.IsNullOrWhiteSpace(user.DisplayName))
                        {
                            userProperties.DisplayName = user.DisplayName;
                        }

                        var regexEmail = new Regex(VALID_EMAIL_REGULAR_EXPRESSION, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace);

                        foreach (var email in GetEmails(user))
                        {
                            if (string.IsNullOrWhiteSpace(email))
                                continue;

                            var fixedEmail = email.Trim();
                            //Make it 'case insensitive'
                            if (fixedEmail.StartsWith("smtp:", StringComparison.InvariantCultureIgnoreCase))
                            {
                                //Get the email string from AD
                                fixedEmail = fixedEmail.Substring(5).Trim();
                            }

                            if (!regexEmail.IsMatch(fixedEmail)) continue;
                            userProperties.Email = fixedEmail;
                            break;
                        }

                        //add new properties here to fill the profile.
                        return userProperties;
                }
                default:
                    // The user has not chosen an UserAccountSource or UserAccountSource as None
                    // Usernames will be displayed as "Domain\Username"
                    return userProperties;
            }
        }

        private static IEnumerable<string> GetEmails(UserPrincipal user)
		{
			// Add the "mail" entry
			yield return user.EmailAddress;

			// Add the "proxyaddresses" entries.
			var properties = ((DirectoryEntry) user.GetUnderlyingObject()).Properties;
			foreach (var property in properties["proxyaddresses"])
				yield return property.ToString();

			yield return user.UserPrincipalName;
		}

		#endregion

	}
}
