using System.Web;
using BugNET.BLL;
using BugNET.Common;

namespace BugNET.UI.WebControls
{
    public class SuckerFishMenuHelper : MenuHelperRoot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SuckerFishMenuHelper"/> class.
        /// </summary>
        /// <param name="projectId">The project id.</param>
        public SuckerFishMenuHelper(int projectId)
        {
            //Setup menu... 
            Items.Add(new SuckerMenuItem("~/Default", Resources.SharedResources.Home, this));

            if (HttpContext.Current.User.Identity.IsAuthenticated)
                Items.Add(new SuckerMenuItem("~/Issues/MyIssues", Resources.SharedResources.MyIssues, this));

            if (projectId > Globals.NewId)
            {
                var oItemProject = new SuckerMenuItem("#", Resources.SharedResources.Project, this, "dropdown");

                Items.Insert(1, oItemProject);
                oItemProject.Items.Add(new SuckerMenuItem($"~/Projects/ProjectSummary/{projectId}",
                    Resources.SharedResources.ProjectSummary, this));
                oItemProject.Items.Add(new SuckerMenuItem($"~/Projects/Roadmap/{projectId}",
                    Resources.SharedResources.Roadmap, this));
                oItemProject.Items.Add(new SuckerMenuItem($"~/Projects/ChangeLog/{projectId}",
                    Resources.SharedResources.ChangeLog, this));
                oItemProject.Items.Add(new SuckerMenuItem($"~/Projects/ProjectCalendar/{projectId}",
                    Resources.SharedResources.Calendar, this));

                if (!string.IsNullOrEmpty(ProjectManager.GetById(projectId).SvnRepositoryUrl))
                    oItemProject.Items.Add(new SuckerMenuItem($"~/SvnBrowse/SubversionBrowser.aspx?pid={projectId}",
                        Resources.SharedResources.Repository, this));

                var oItemIssues = new SuckerMenuItem("#", Resources.SharedResources.Issues, this, "dropdown");

                oItemIssues.Items.Add(new SuckerMenuItem($"~/Issues/IssueList.aspx?pid={projectId}",
                    Resources.SharedResources.Issues, this));
                oItemIssues.Items.Add(new SuckerMenuItem($"~/Queries/QueryList.aspx?pid={projectId}",
                    Resources.SharedResources.Queries, this));
                Items.Insert(2, oItemIssues);

                if (HttpContext.Current.User.Identity.IsAuthenticated)
                    //check add issue permission
                    if (UserManager.HasPermission(projectId, Permission.AddIssue.ToString()))
                        Items.Add(new SuckerMenuItem($"~/Issues/CreateIssue/{projectId}",
                            Resources.SharedResources.NewIssue, this));
            }

            if (!HttpContext.Current.User.Identity.IsAuthenticated) return;

            var oItemAdmin = new SuckerMenuItem("#", Resources.SharedResources.Admin, this, "navbar-admin");

            if (projectId > Globals.NewId && (UserManager.IsInRole(projectId, Globals.ProjectAdministratorRole) ||
                                              UserManager.IsSuperUser()))
                oItemAdmin.Items.Add(new SuckerMenuItem($"~/Administration/Projects/EditProject/{projectId}",
                    Resources.SharedResources.EditProject, this, "admin"));

            if (UserManager.IsSuperUser())
            {
                oItemAdmin.Items.Add(new SuckerMenuItem("~/Administration/Projects/ProjectList",
                    Resources.SharedResources.Projects, this));
                oItemAdmin.Items.Add(new SuckerMenuItem("~/Administration/Users/UserList",
                    Resources.SharedResources.UserAccounts, this));
                oItemAdmin.Items.Add(new SuckerMenuItem("~/Administration/Host/Settings",
                    Resources.SharedResources.ApplicationConfiguration, this));
                oItemAdmin.Items.Add(new SuckerMenuItem("~/Administration/Host/LogViewer",
                    Resources.SharedResources.LogViewer, this));
            }

            if (oItemAdmin.Items.Count > 0) Items.Add(oItemAdmin);
        }
    }
}