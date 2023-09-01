﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Security;
using System.Web.UI.WebControls;
using BugNET.BLL;
using BugNET.Common;
using BugNET.Entities;
using BugNET.UI;

namespace BugNET.Issues
{
    public partial class MyIssues : BugNetBasePage
    {
        /// <summary>
        /// Issueses the rebind.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void IssuesRebind(object s, EventArgs e)
        {
            BindIssues();
        }

        /// <summary>
        /// Views the selected index changed.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void MyIssuesFilterChanged(object s, EventArgs e)
        {
            ctlDisplayIssues.CurrentPageIndex = 0;

            ExcludeClosedIssuesFilter.Enabled = ViewIssuesDropDownFilter.SelectedValue != "Closed";

            BindIssues();
        }

        /// <summary>
        /// Returns a list of QueryClauses for items selected in the Project list (will handle the Select All too)
        /// </summary>
        /// <param name="returnAll">When true will return all project id's from the listbox, otherwise only the selected items</param>
        /// <returns></returns>
        private IEnumerable<QueryClause> GetProjectQueryClauses(bool returnAll)
        {
            var queryClauses = new List<QueryClause>();

            var projects = PresentationUtils.GetSelectedItemsIntegerList(ProjectListBoxFilter, returnAll)
                .Where(project => project > Globals.NewId).ToList();

            if (projects.Count > 0)
            {
                var first = true;

                foreach (var project in projects)
                {
                    queryClauses.Add(new QueryClause(first ? "AND (" : "OR", "iv.[ProjectId]", "=", project.ToString(),
                        SqlDbType.NVarChar));
                    first = false;
                }

                queryClauses.Add(new QueryClause(")", "", "", "", SqlDbType.NVarChar));
            }

            return queryClauses;
        }

        /// <summary>
        /// Gets the total assigned issue count.
        /// </summary>
        /// <returns></returns>
        protected string GetTotalAssignedIssueCount()
        {
            var user = ViewIssueMemberDropDown.SelectedValue;

            if (string.IsNullOrEmpty(user)) return "0";

            var queryClauses = new List<QueryClause>
            {
                // do not include disabled projects
                new QueryClause("AND", "iv.[ProjectDisabled]", "=", "0", SqlDbType.Int),

                // do not include disabled issues
                new QueryClause("AND", "iv.[Disabled]", "=", "0", SqlDbType.Int),

                // add the user id to the filtered field
                new QueryClause("AND", "iv.[IssueAssignedUserId]", "=", user, SqlDbType.NVarChar)
            };

            // return the projects in the list box, this represents all the projects the user has access to
            // pre filtered on the page load
            queryClauses.AddRange(GetProjectQueryClauses(true));

            return IssueManager.PerformQuery(queryClauses, null).Count.ToString();
        }

        /// <summary>
        /// Gets the total created issue count.
        /// </summary>
        /// <returns></returns>
        protected string GetTotalCreatedIssueCount()
        {
            var user = ViewIssueMemberDropDown.SelectedValue;

            if (string.IsNullOrEmpty(user)) return "0";

            var queryClauses = new List<QueryClause>
            {
                // do not include disabled projects
                new QueryClause("AND", "iv.[ProjectDisabled]", "=", "0", SqlDbType.Int),

                // do not include disabled issues
                new QueryClause("AND", "iv.[Disabled]", "=", "0", SqlDbType.Int),

                // add the user id to the filtered field
                new QueryClause("AND", "iv.[IssueCreatorUserId]", "=", user, SqlDbType.NVarChar)
            };

            // return the projects in the list box, this represents all the projects the user has access to
            // pre filtered on the page load
            queryClauses.AddRange(GetProjectQueryClauses(true));

            return IssueManager.PerformQuery(queryClauses, null).Count.ToString();
        }

        /// <summary>
        /// Gets the total closed issue count.
        /// </summary>
        /// <returns></returns>
        protected string GetTotalClosedIssueCount()
        {
            var user = ViewIssueMemberDropDown.SelectedValue;

            if (string.IsNullOrEmpty(user)) return "0";

            var queryClauses = new List<QueryClause>
            {
                // do not include disabled projects
                new QueryClause("AND", "iv.[ProjectDisabled]", "=", "0", SqlDbType.Int),

                // do not include disabled issues
                new QueryClause("AND", "iv.[Disabled]", "=", "0", SqlDbType.Int),

                // only closed issue
                new QueryClause("AND", "iv.[IsClosed]", "=", "1", SqlDbType.Int),

                // add the user id to the filtered field
                new QueryClause("AND", "iv.[IssueAssignedUserId]", "=", user, SqlDbType.NVarChar)
            };

            // return the projects in the list box, this represents all the projects the user has access to
            // pre filtered on the page load
            queryClauses.AddRange(GetProjectQueryClauses(true));

            return IssueManager.PerformQuery(queryClauses, null).Count.ToString();
        }

        /// <summary>
        /// Gets the total owned issue count.
        /// </summary>
        /// <returns></returns>
        protected string GetTotalOwnedIssueCount()
        {
            var user = ViewIssueMemberDropDown.SelectedValue;

            if (string.IsNullOrEmpty(user)) return "0";

            var queryClauses = new List<QueryClause>
            {
                // do not include disabled projects
                new QueryClause("AND", "iv.[ProjectDisabled]", "=", "0", SqlDbType.Int),

                // do not include disabled issues
                new QueryClause("AND", "iv.[Disabled]", "=", "0", SqlDbType.Int),

                // add the user id to the filtered field
                new QueryClause("AND", "iv.[IssueOwnerUserId]", "=", user, SqlDbType.NVarChar)
            };

            // return the projects in the list box, this represents all the projects the user has access to
            // pre filtered on the page load
            queryClauses.AddRange(GetProjectQueryClauses(true));

            return IssueManager.PerformQuery(queryClauses, null).Count.ToString();
        }

        /// <summary>
        /// Gets the total monitored issues count.
        /// </summary>
        /// <returns></returns>
        protected string GetTotalMonitoredIssuesCount()
        {
            var user = UserManager.GetUser(new Guid(ViewIssueMemberDropDown.SelectedValue));

            return IssueManager.GetMonitoredIssuesByUserName(user.UserName, false).Count.ToString();
        }

        /// <summary>
        /// Binds the issues.
        /// </summary>
        private void BindIssues()
        {
            var userId = ViewIssueMemberDropDown.SelectedValue;
            if (userId == null) return;

            var queryClauses = new List<QueryClause>
            {
                // do not include disabled projects
                new QueryClause("AND", "iv.[ProjectDisabled]", "=", "0", SqlDbType.Int),

                // do not include disabled issues
                new QueryClause("AND", "iv.[Disabled]", "=", "0", SqlDbType.Int)
            };

            // return the projects selected in the list box, this represents all the projects the user has access to
            // pre filtered on the page load
            var selectedProjects = GetProjectQueryClauses(false);

            // hack yes but does the trick to make sure that all projects are loaded when select all is selected
            queryClauses.AddRange(GetProjectQueryClauses(selectedProjects.Count().Equals(0)));

            var sortColumns = new List<KeyValuePair<string, string>>();
            var sorter = ctlDisplayIssues.SortString;

            foreach (var sort in sorter.Split(','))
            {
                var args = sort.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                if (args.Length.Equals(2))
                    sortColumns.Add(new KeyValuePair<string, string>(args[0], args[1]));
            }

            if (ViewIssuesDropDownFilter.SelectedValue == "Monitored")
            {
                var projects = PresentationUtils.GetSelectedItemsIntegerList(ProjectListBoxFilter, false)
                    .Where(project => project > Globals.NewId).ToList();

                ctlDisplayIssues.RssUrl = $"~/Feed.aspx?channel=15&ec={ExcludeClosedIssuesFilter.Checked}";
                ctlDisplayIssues.DataSource = IssueManager.GetMonitoredIssuesByUserName(userId, sortColumns, projects,
                    ExcludeClosedIssuesFilter.Checked);
                ctlDisplayIssues.DataBind();
            }
            else
            {
                switch (ViewIssuesDropDownFilter.SelectedValue)
                {
                    case "Assigned":

                        if (ExcludeClosedIssuesFilter.Checked)
                            queryClauses.Add(new QueryClause("AND", "iv.[IsClosed]", "=", "0", SqlDbType.Int));

                        queryClauses.Add(new QueryClause("AND", "iv.[IssueAssignedUserId]", "=", userId,
                            SqlDbType.NVarChar));

                        ctlDisplayIssues.RssUrl =
                            $"~/Feed.aspx?channel=7&u={Security.GetUserName()}&ec={ExcludeClosedIssuesFilter.Checked}";

                        break;
                    case "Closed":

                        queryClauses.Add(new QueryClause("AND", "iv.[IsClosed]", "=", "1", SqlDbType.Int));

                        queryClauses.Add(new QueryClause("AND", "iv.[IssueAssignedUserId]", "=", userId,
                            SqlDbType.NVarChar));

                        ctlDisplayIssues.RssUrl =
                            $"~/Feed.aspx?channel=7&u={Security.GetUserName()}&ec={bool.FalseString}";

                        break;
                    case "Owned":

                        if (ExcludeClosedIssuesFilter.Checked)
                            queryClauses.Add(new QueryClause("AND", "iv.[IsClosed]", "=", "0", SqlDbType.Int));

                        queryClauses.Add(new QueryClause("AND", "iv.[IssueOwnerUserId]", "=", userId,
                            SqlDbType.NVarChar));
                        ctlDisplayIssues.RssUrl =
                            $"~/Feed.aspx?channel=7&ou={Security.GetUserName()}&ec={ExcludeClosedIssuesFilter.Checked}";
                        break;
                    case "Created":

                        if (ExcludeClosedIssuesFilter.Checked)
                            queryClauses.Add(new QueryClause("AND", "iv.[IsClosed]", "=", "0", SqlDbType.Int));

                        queryClauses.Add(new QueryClause("AND", "iv.[IssueCreatorUserId]", "=", userId,
                            SqlDbType.NVarChar));
                        ctlDisplayIssues.RssUrl =
                            $"~/Feed.aspx?channel=7&ru={Security.GetUserName()}&ec={bool.FalseString}";
                        break;
                    default:

                        if (ExcludeClosedIssuesFilter.Checked)
                            queryClauses.Add(new QueryClause("AND", "iv.[IsClosed]", "=", "0", SqlDbType.Int));
                        break;
                }

                ctlDisplayIssues.DataSource = IssueManager.PerformQuery(queryClauses, sortColumns);
                ctlDisplayIssues.DataBind();
            }
        }

        private void BindMembers()
        {
            if (ProjectListBoxFilter.SelectedValue == "0")
            {
                ViewIssueMemberDropDown.DataSource = UserManager.GetAllUsers();
                ViewIssueMemberDropDown.DataTextField = "DisplayName";
                ViewIssueMemberDropDown.DataValueField = "ProviderUserKey";
            }
            else
            {
                ViewIssueMemberDropDown.DataSource =
                    IssueManager.GetUserCountByProjectId(int.Parse(ProjectListBoxFilter.SelectedValue));
                ViewIssueMemberDropDown.DataTextField = "Name";
                ViewIssueMemberDropDown.DataValueField = "Id";
            }

            ViewIssueMemberDropDown.DataBind();

            var user = Membership.GetUser();
            if (user != null && user.ProviderUserKey != null &&
                ViewIssueMemberDropDown.Items.FindByValue(user.ProviderUserKey.ToString()) != null)
                ViewIssueMemberDropDown.SelectedValue = user.ProviderUserKey.ToString();
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated)
                ErrorRedirectHelper.TransferToLoginPage(Context);

            if (Page.IsPostBack) return;

            ctlDisplayIssues.PageSize = UserManager.GetProfilePageSize();
            ctlDisplayIssues.CurrentPageIndex = 0;

            DisplayNameLabel.Text = string.Format(GetLocalString("MyIssuesPage_Title.Text"),
                Security.GetDisplayName());

            ProjectListBoxFilter.DataSource = ProjectManager.GetByMemberUserName(Context.User.Identity.Name);
            ProjectListBoxFilter.DataTextField = "Name";
            ProjectListBoxFilter.DataValueField = "Id";
            ProjectListBoxFilter.DataBind();
            ProjectListBoxFilter.Items.Insert(0,
                new ListItem(GetLocalString("ProjectListBoxFilter_SelectAll.Text"), "0"));
            ProjectListBoxFilter.SelectedIndex = 0;

            BindMembers();

            ExcludeClosedIssuesFilter.Enabled = ViewIssuesDropDownFilter.SelectedValue != "Closed";
            BindIssues();
        }

        /// <summary>
        /// Handles the Changed event of the ProjectListBoxFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void ProjectListBoxFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindMembers();
            BindIssues();
        }

        protected void ViewIssueMemberDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            var user = UserManager.GetUser(new Guid(ViewIssueMemberDropDown.SelectedValue));
            var name = UserManager.GetUserDisplayName(user.UserName);

            DisplayNameLabel.Text = string.Format(GetLocalString("MyIssuesPage_Title.Text"),
                name);

            BindIssues();
        }
    }
}