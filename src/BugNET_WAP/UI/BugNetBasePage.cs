using System;
using System.Web.UI;
using BugNET.BLL;
using BugNET.Common;
using Microsoft.AspNet.FriendlyUrls;

namespace BugNET.UI
{
    public abstract class BugNetUserControl : UserControl
    {
        protected string GetGlobalString(string className, string resourceKey,
            string defaultValue = "Unset String Resource")
        {
            return GetGlobalResourceObject(className, resourceKey)?.ToString() ?? defaultValue;
        }

        protected string GetLocalString(string resourceKey, string defaultValue = "Unset String Resource")
        {
            return GetLocalResourceObject(resourceKey)?.ToString() ?? defaultValue;
        }
    }

    /// <summary>
    /// Summary description for BasePage.
    /// </summary>
    public abstract class BugNetBasePage : Page
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"></see> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"></see> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Page.Title = $@"{Page.Title} - {HostSettingManager.Get(HostSettingNames.ApplicationTitle)}";
        }

        /// <summary>
        /// Returns to previous page.
        /// </summary>
        public void ReturnToPreviousPage()
        {
            if (Session["ReferrerUrl"] != null)
                Response.Redirect((string) Session["ReferrerUrl"]);
            else
                Response.Redirect($"~/Issues/IssueList.aspx?pid={ProjectId}");
        }

        /// <summary>
        /// Gets or sets the project id.
        /// </summary>
        /// <value>The project id.</value>
        public virtual int ProjectId
        {
            get => ViewState.Get("ProjectId", Globals.NewId);
            set => ViewState.Set("ProjectId", value);
        }

        /// <summary>
        /// Overrides the default OnInit to provide a security check for pages
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Check for session timeouts
            if (Context.Session != null && User.Identity.IsAuthenticated)
                // check whether a new session was generated
                if (Session.IsNewSession)
                {
                    // check whether a cookies had already been associated with this request
                    var sessionCookie = Request.Cookies["ASP.NET_SessionId"];
                    if (sessionCookie != null)
                    {
                        var sessionValue = sessionCookie.Value;
                        if (!string.IsNullOrEmpty(sessionValue))
                            if (Session.SessionID != sessionValue)
                                // we have session timeout condition!
                                ErrorRedirector.TransferToSessionExpiredPage(Page);
                    }
                }

            // Security check using the following rules:
            // 1. Application must allow anonymous identification (DisableAnonymousAccess HostSetting)
            // 2. User must be authenticated if anonymous identification is false
            // 3. Default page is not protected so the unauthenticated user may login
            if (!HostSettingManager.Get(HostSettingNames.AnonymousAccess, false) &&
                !User.Identity.IsAuthenticated &&
                !Request.Url.LocalPath.EndsWith("Default.aspx"))
                ErrorRedirector.TransferToLoginPage(Page);

            int projectId;
            try
            {
                var segments = Request.GetFriendlyUrlSegments();
                projectId = int.Parse(segments[0]);
            }
            catch
            {
                projectId = Request.QueryString.Get("pid", Globals.NewId);
            }

            if (projectId <= Globals.NewId) return;

            // Security check: Ensure the project exists (ie PID is valid project)
            var project = ProjectManager.GetById(projectId);

            if (project == null)
            {
                // If myProj is a null it will cause an exception later on the page anyway, but I want to
                // take extra measures here to prevent leaks of datatypes through exception messages.
                // 
                // This protects against the administrator turning on remote error messages and also 
                // protects the business logic from injection attacks. 
                //
                // If this page is used consistently it will fool a hacker into thinking an actual 
                // DB QUERY executed using the supplied attack. ;)
                ErrorRedirector.TransferToNotFoundPage(Page);
                return;
            }

            // set the project id if we have one
            ProjectId = projectId;

            switch (User.Identity.IsAuthenticated)
            {
                // Security check using the following rules:
                // 1. Anonymous user
                // 2. The project type is private
                case false when project.AccessType == ProjectAccessType.Private:
                    ErrorRedirector.TransferToLoginPage(Page);
                    return;
                // Security check using the following rules:
                // 1. Not Super user
                // 2. Authenticated user
                // 3. The project type is private 
                // 4. The user is not a project member
                case true when !UserManager.IsSuperUser() &&
                               project.AccessType == ProjectAccessType.Private &&
                               !ProjectManager.IsUserProjectMember(User.Identity.Name, projectId):
                    ErrorRedirector.TransferToLoginPage(Page);
                    break;
            }
        }

        protected string GetGlobalString(string className, string resourceKey,
            string defaultValue = "Unset String Resource")
        {
            return GetGlobalResourceObject(className, resourceKey)?.ToString() ?? defaultValue;
        }

        protected string GetLocalString(string resourceKey, string defaultValue = "Unset String Resource")
        {
            return GetLocalResourceObject(resourceKey)?.ToString() ?? defaultValue;
        }
    }
}