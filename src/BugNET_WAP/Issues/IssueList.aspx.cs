using System;
using System.Collections.Generic;
using System.Data;
using BugNET.BLL;
using BugNET.Common;
using BugNET.Entities;
using BugNET.UI;

namespace BugNET.Issues
{
    /// <summary>
    /// Summary description for Issue List.
    /// </summary>
    public partial class IssueList : BugNetBasePage
    {
        #region Private Variables

        private const string ISSUELISTSTATE = "IssueListState";

        #endregion

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack) return;

            ctlDisplayIssues.PageSize = UserManager.GetProfilePageSize();
            ctlDisplayIssues.CurrentPageIndex = 0;

            ProjectId = Request.Get("pid", -1);

            // BGN-1379
            if (ProjectId.Equals(-1))
                ErrorRedirectHelper.TransferToNotFoundPage(Context);

            if (!User.Identity.IsAuthenticated)
            {
                dropView.Items.Remove(dropView.Items.FindByValue("Relevant"));
                dropView.Items.Remove(dropView.Items.FindByValue("Assigned"));
                dropView.Items.Remove(dropView.Items.FindByValue("Owned"));
                dropView.Items.Remove(dropView.Items.FindByValue("Created"));
                dropView.SelectedIndex = 1;
            }

            var state = (IssueListState) Session[ISSUELISTSTATE];

            if (state != null)
            {
                if (Request.QueryString.Count == 1 && state.ViewIssues == string.Empty) state.ViewIssues = "Open";

                if (ProjectId > 0 && ProjectId != state.ProjectId)
                {
                    Session.Remove(ISSUELISTSTATE);
                }
                else
                {
                    if (Request.QueryString.Count > 1)
                        state.ViewIssues = string.Empty;

                    dropView.SelectedValue = state.ViewIssues;
                    ProjectId = state.ProjectId;
                    ctlDisplayIssues.CurrentPageIndex = state.IssueListPageIndex;
                    ctlDisplayIssues.SortField = state.SortField;
                    ctlDisplayIssues.SortAscending = state.SortAscending;
                    ctlDisplayIssues.PageSize = state.PageSize;
                }
            }
            else
            {
                if (Request.QueryString.Count > 1) dropView.SelectedValue = string.Empty;
            }

            BindIssues();
        }

        /// <summary>
        /// Handles the PreRender event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Page_PreRender(object sender, EventArgs e)
        {
            // Intention is to restore IssueList page state when if it is redirected back to.
            // Put all necessary data in IssueListState object and save it in the session.
            var state = (IssueListState) Session[ISSUELISTSTATE] ?? new IssueListState();

            state.ViewIssues = dropView.SelectedValue;
            state.ProjectId = ProjectId;
            state.IssueListPageIndex = ctlDisplayIssues.CurrentPageIndex;
            state.SortField = ctlDisplayIssues.SortField;
            state.SortAscending = ctlDisplayIssues.SortAscending;
            state.PageSize = ctlDisplayIssues.PageSize;
            Session[ISSUELISTSTATE] = state;
        }

        #region Querystring Properties

        /// <summary>
        /// Returns the component Id from the query string
        /// </summary>
        public string IssueCategoryId => Request.Get("c", string.Empty);

        /// <summary>
        /// Returns the keywords from the query string
        /// </summary>
        public string Key => Request.Get("key", string.Empty).Replace("+", " ");

        /// <summary>
        /// Returns the Milestone Id from the query string
        /// </summary>
        public string IssueMilestoneId => Request.Get("m", string.Empty);

        /// <summary>
        /// Returns the priority Id from the query string
        /// </summary>
        public string IssuePriorityId => Request.Get("p", string.Empty);

        /// <summary>
        /// Returns the Type Id from the query string
        /// </summary>
        public string IssueTypeId => Request.Get("t", string.Empty);

        /// <summary>
        /// Returns the status Id from the query string
        /// </summary>
        public string IssueStatusId => Request.Get("s", string.Empty);

        /// <summary>
        /// Returns the assigned to user Id from the query string
        /// </summary>
        public string AssignedUserId => Request.Get("u", string.Empty);

        /// <summary>
        /// Gets the name of the reporter user.
        /// </summary>
        /// <value>The name of the reporter user.</value>
        public string ReporterUserName => Request.Get("ru", string.Empty);

        /// <summary>
        /// Returns the hardware Id from the query string
        /// </summary>
        public string IssueResolutionId => Request.Get("r", string.Empty);

        /// <summary>
        /// Gets the issue id.
        /// </summary>
        /// <value>The issue id.</value>
        public int IssueId => Request.Get("bid", -1);

        #endregion

        /// <summary>
        /// Views the selected index changed.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void ViewSelectedIndexChanged(object s, EventArgs e)
        {
            ctlDisplayIssues.CurrentPageIndex = 0;

            BindIssues(dropView.SelectedValue);
        }

        /// <summary>
        /// Issues the rebind.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void IssuesRebind(object s, EventArgs e)
        {
            BindIssues();
        }

        /// <summary>
        /// Binds the issues.
        /// </summary>
        private void BindIssues(string issueViewSelectedValue = "")
        {
            var isError = false;

            var queryClauses = new List<QueryClause>();

            // add the disabled field as the first order of business
            var q = new QueryClause("AND", "iv.[Disabled]", "=", "0", SqlDbType.Int);
            queryClauses.Add(q);

            if (Request.QueryString.Count > 1 && issueViewSelectedValue.Equals(""))
            {
                dropView.SelectedIndex = 0;
                var isStatus = false;

                if (!string.IsNullOrEmpty(IssueCategoryId))
                {
                    q = IssueCategoryId == "0"
                        ? new QueryClause("AND", "iv.[IssueCategoryId]", "IS", null, SqlDbType.Int)
                        : new QueryClause("AND", "iv.[IssueCategoryId]", "=", IssueCategoryId, SqlDbType.Int);

                    queryClauses.Add(q);
                }

                if (!string.IsNullOrEmpty(IssueTypeId))
                {
                    q = IssueTypeId == "0"
                        ? new QueryClause("AND", "iv.[IssueTypeId]", "IS", null, SqlDbType.Int)
                        : new QueryClause("AND", "iv.[IssueTypeId]", "=", IssueTypeId, SqlDbType.Int);

                    queryClauses.Add(q);
                }

                if (!string.IsNullOrEmpty(IssuePriorityId))
                {
                    q = IssuePriorityId == "0"
                        ? new QueryClause("AND", "iv.[IssuePriorityId]", "IS", null, SqlDbType.Int)
                        : new QueryClause("AND", "iv.[IssuePriorityId]", "=", IssuePriorityId, SqlDbType.Int);

                    queryClauses.Add(q);
                }

                if (!string.IsNullOrEmpty(IssueMilestoneId))
                {
                    q = IssueMilestoneId == "0"
                        ? new QueryClause("AND", "iv.[IssueMilestoneId]", "IS", null, SqlDbType.Int)
                        : new QueryClause("AND", "iv.[IssueMilestoneId]", "=", IssueMilestoneId, SqlDbType.Int);

                    queryClauses.Add(q);
                }

                if (!string.IsNullOrEmpty(IssueResolutionId))
                {
                    q = IssueResolutionId == "0"
                        ? new QueryClause("AND", "iv.[IssueResolutionId]", "IS", null, SqlDbType.Int)
                        : new QueryClause("AND", "iv.[IssueResolutionId]", "=", IssueResolutionId, SqlDbType.Int);

                    queryClauses.Add(q);
                }

                if (!string.IsNullOrEmpty(AssignedUserId))
                {
                    Guid userId;
                    q = new QueryClause("AND", "iv.[IssueAssignedUserId]", "IS", null, SqlDbType.NVarChar);

                    if (Guid.TryParse(AssignedUserId, out userId))
                        q = AssignedUserId == Globals.EmptyGuid
                            ? new QueryClause("AND", "iv.[IssueAssignedUserId]", "IS", null, SqlDbType.Int)
                            : new QueryClause("AND", "iv.[IssueAssignedUserId]", "=", AssignedUserId,
                                SqlDbType.NVarChar);

                    queryClauses.Add(q);
                }

                if (!string.IsNullOrEmpty(IssueStatusId))
                {
                    if (IssueStatusId == "-2")
                    {
                        // filter by closed issues
                        isStatus = true;
                        queryClauses.Add(new QueryClause("AND", "iv.[IsClosed]", "=", "1", SqlDbType.Int));
                    }
                    else if (IssueStatusId != "-1")
                    {
                        isStatus = true;
                        q = IssueStatusId == "0"
                            ? new QueryClause("AND", "iv.[IssueStatusId]", "IS", null, SqlDbType.Int)
                            : new QueryClause("AND", "iv.[IssueStatusId]", "=", IssueStatusId, SqlDbType.Int);

                        queryClauses.Add(q);
                    }
                    else
                    {
                        isStatus = true;
                        queryClauses.Add(new QueryClause("AND", "iv.[IsClosed]", "=", "0", SqlDbType.Int));
                    }
                }

                // exclude all closed status's
                if (!isStatus) queryClauses.Add(new QueryClause("AND", "iv.[IsClosed]", "=", "0", SqlDbType.Int));

                try
                {
                    //colIssues = IssueManager.PerformQuery(queryClauses, ProjectId);

                    // TODO: WARNING Potential Cross Site Scripting attack
                    // also this code only runs if the previous code does not freak out
                    ctlDisplayIssues.RssUrl = $"~/Feed.aspx?{Request.QueryString}&channel=7";
                }
                catch
                {
                    // BGN-1379
                    // This URL http://localhost/BugNET/Issues/IssueList.aspx?pid=96&c=4471%27;
                    // Generates a Input string was not in a correct format exception in
                    // Source File:  C:\Development\BugNET 0.7.921 SVN Source\branches\BugNET 0.8\src\BugNET_WAP\Old_App_Code\DAL\SqlDataProvider.cs    Line:  4932 
                    // Line 4932:                gcfr(sqlCmd.ExecuteReader(), ref List);
                    isError = true;

                    // perhaps this should rather ErrorRedirector.TransferToErrorPage(Page);
                    // but an empty grid with "There are no issues that match your criteria." looks 
                    // nice too
                }
            }
            else
            {
                var userName = Security.GetUserName();

                switch (dropView.SelectedValue)
                {
                    case "Relevant":
                        queryClauses.Add(new QueryClause("AND", "iv.[IsClosed]", "=", "0", SqlDbType.Int));

                        queryClauses.Add(new QueryClause("AND (", "iv.[AssignedUsername]", "=", userName,
                            SqlDbType.NVarChar));
                        queryClauses.Add(new QueryClause("OR", "iv.[CreatorUsername]", "=", userName,
                            SqlDbType.NVarChar));
                        queryClauses.Add(new QueryClause("OR", "iv.[OwnerUsername]", "=", userName,
                            SqlDbType.NVarChar));
                        queryClauses.Add(new QueryClause(")", "", "", "", SqlDbType.NVarChar));

                        ctlDisplayIssues.RssUrl = $"~/Feed.aspx?pid={ProjectId}&channel=8";
                        break;
                    case "Assigned":
                        queryClauses.Add(new QueryClause("AND", "iv.[IsClosed]", "=", "0", SqlDbType.Int));
                        queryClauses.Add(new QueryClause("AND", "iv.[AssignedUsername]", "=", userName,
                            SqlDbType.NVarChar));

                        ctlDisplayIssues.RssUrl = $"~/Feed.aspx?pid={ProjectId}&channel=9";
                        break;
                    case "Owned":
                        queryClauses.Add(new QueryClause("AND", "iv.[IsClosed]", "=", "0", SqlDbType.Int));
                        queryClauses.Add(
                            new QueryClause("AND", "iv.[OwnerUsername]", "=", userName, SqlDbType.NVarChar));

                        ctlDisplayIssues.RssUrl = $"~/Feed.aspx?pid={ProjectId}&channel=10";
                        break;
                    case "Created":
                        queryClauses.Add(new QueryClause("AND", "iv.[IsClosed]", "=", "0", SqlDbType.Int));
                        queryClauses.Add(new QueryClause("AND", "iv.[CreatorUsername]", "=", userName,
                            SqlDbType.NVarChar));

                        ctlDisplayIssues.RssUrl = $"~/Feed.aspx?pid={ProjectId}&channel=11";
                        break;
                    case "All":
                        ctlDisplayIssues.RssUrl = $"~/Feed.aspx?pid={ProjectId}&channel=12";
                        break;
                    case "Open":
                        queryClauses.Add(new QueryClause("AND", "iv.[IsClosed]", "=", "0", SqlDbType.Int));
                        ctlDisplayIssues.RssUrl = $"~/Feed.aspx?pid={ProjectId}&channel=14";
                        break;
                    case "Closed":
                        queryClauses.Add(new QueryClause("AND", "iv.[IsClosed]", "=", "1", SqlDbType.Int));
                        ctlDisplayIssues.RssUrl = $"~/Feed.aspx?pid={ProjectId}&channel=16";
                        break;
                }
            }

            if (isError) return;

            var sortColumns = new List<KeyValuePair<string, string>>();

            if (Request.QueryString["cr"] != null)
                sortColumns.Add(new KeyValuePair<string, string>("iv.[DateCreated]", "desc"));

            if (Request.QueryString["ur"] != null)
                sortColumns.Add(new KeyValuePair<string, string>("iv.[LastUpdate]", "desc"));

            var sorter = ctlDisplayIssues.SortString;

            foreach (var sort in sorter.Split(','))
            {
                var args = sort.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                if (args.Length.Equals(2))
                    sortColumns.Add(new KeyValuePair<string, string>(args[0], args[1]));
            }

            var colIssues = IssueManager.PerformQuery(queryClauses, sortColumns, ProjectId);

            ctlDisplayIssues.DataSource = colIssues;
            ctlDisplayIssues.DataBind();
        }

        /// <summary>
        /// Adds the issue.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void AddIssue(object s, EventArgs e)
        {
            Response.Redirect($"~/Issues/IssueDetail.aspx?pid={ProjectId}");
        }
    }
}