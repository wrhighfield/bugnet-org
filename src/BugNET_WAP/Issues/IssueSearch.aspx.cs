﻿using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.WebControls;
using BugNET.BLL;
using BugNET.Entities;
using System.Data;
using System.Linq;
using BugNET.BLL.Comparers;
using BugNET.UI;

namespace BugNET.Issues
{
    /// <summary>
    /// 
    /// </summary>
    public partial class SearchResults : BugNetBasePage
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                pnlResultsMessage.Visible = true;
                pnlSearchResults.Visible = false;

                if (Request.QueryString["q"] != null)
                {
                    txtSearch.Text = Request.QueryString["q"];
                    BindIssues();
                }
            }


            // The ExpandIssuePaths method is called to handle
            // the SiteMapResolve event.
            SiteMap.SiteMapResolve += ExpandIssuePaths;
        }

        /// <summary>
        /// Handles the Unload event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Unload(object sender, EventArgs e)
        {
            //remove the event handler
            SiteMap.SiteMapResolve -= ExpandIssuePaths;
        }

        private static SiteMapNode ExpandIssuePaths(object sender, SiteMapResolveEventArgs e)
        {
            if (SiteMap.CurrentNode == null) return null;

            var currentNode = SiteMap.CurrentNode.Clone(true);
            var tempNode = currentNode;

            if (null != (tempNode = tempNode.ParentNode)) tempNode.Url = string.Empty;

            return currentNode;
        }

        //  private const string ISSUELISTSTATE = "SearchIssueListState";

        /// <summary>
        /// Handles the PreRender event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Page_PreRender(object sender, EventArgs e)
        {
            // Intention is to restore IssueList page state when if it is redirected back to.
            // Put all necessary data in IssueListState object and save it in the session.

            //IssueListState state = (IssueListState)Session[ISSUELISTSTATE];
            //if (state == null) state = new IssueListState();            
            //state.IssueListPageIndex = ctlBugs.CurrentPageIndex;
            //state.IssueListPageIndex = ctlBugs.CurrentPageIndex;
            //state.SortField = ctlBugs.SortField;
            //state.SortAscending = ctlBugs.SortAscending;
            //state.PageSize = ctlBugs.PageSize;
            //Session[ISSUELISTSTATE] = state;
        }

        private List<Issue> _mainIssues;
        private List<IssueComment> _mainComments;

        /// <summary>
        /// Handles the Click event of the Button1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Button1_Click(object sender, EventArgs e)
        {
            //don't search on empty string
            if (string.IsNullOrEmpty(txtSearch.Text))
                return;

            BindIssues();
        }

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
        /// Binds the issues.
        /// </summary>
        private void BindIssues()
        {
            _mainIssues = new List<Issue>();
            _mainComments = new List<IssueComment>();

            // The problem is in Version 0.8, global searches accross all projects for the same integer 
            // codes (for things like status and priority) are meaningless. 
            // So you have to search only with wildcards and keywords across all projects.

            // ---------------------------------------------------------------
            // Prepare a list of projects which the user has access to.             
            //
            // ---------------------------------------------------------------

            // are we logged in ?
            var searchProjects = string.IsNullOrEmpty(Context.User.Identity.Name)
                ? ProjectManager.GetPublicProjects()
                : ProjectManager.GetByMemberUserName(Context.User.Identity.Name);

            searchProjects.Sort(new ProjectComparer("Name", false));

            // ---------------------------------------------------------------
            //
            // Perform the actual search using a method which populates mainIssues
            // and mainComment.
            //
            // ---------------------------------------------------------------

            PerformIssueSearch(searchProjects);

            // ---------------------------------------------------------------
            // 
            // Bind the UI controls
            //
            // ---------------------------------------------------------------
            SearchProjectRepeater.DataSource = searchProjects;
            SearchProjectRepeater.DataBind();

            if (_mainIssues.Count.Equals(0))
            {
                pnlResultsMessage.Visible = true;
                pnlSearchResults.Visible = false;
                litResultsMessage.Text = GetLocalString("SearchNoResults");
            }
            else
            {
                pnlResultsMessage.Visible = false;
                pnlSearchResults.Visible = true;
            }

            if (_mainComments.Count > 0)
                lblSearchSummary.Text = string.Format(GetLocalString("IssuesAndCommentsFound"), _mainIssues.Count,
                    _mainComments.Count);
            else
                lblSearchSummary.Text = string.Format(GetLocalString("IssuesFound"), _mainIssues.Count);
        }

        /// <summary>
        /// Performs the issue search and populates mainIssues and mainComment.
        /// </summary>
        /// <param name="searchProjects">A List of projects to search through.</param>
        private void PerformIssueSearch(IEnumerable<Project> searchProjects)
        {
            var foundComments = new List<IssueComment>();
            var issueComments = new List<IssueComment>();

            // Our search strings on normal and "like" comparators
            // Note: these are deliberately not trimmed!
            // to the users, "test" might be different from "test "
            var strSearch = txtSearch.Text;
            var strLike = "%" + strSearch + "%";
            var strHtmlSearch = Server.HtmlEncode(strSearch);
            var strHtmlLike = "%" + strHtmlSearch + "%";

            // if the two strings are equal srchHtmlcode is false
            // If they are not equal, then I need to search for the HTML encoded 
            // variants later on.
            var srchHtmlcode = strHtmlSearch != strSearch;

            var srchComments = chkComments.Checked;

            // Sort the projects using LINQ
            foreach (var p in searchProjects)
            {
                // now search each project with wildcard parameters 
                // (except for the search string)            

                // ---------------------------------------------------------------
                // Normal Search
                //
                // Searches Description, Issue Title using a LIKE query
                // If you are searching username it adds the LastUpdateUsername,
                // AssignedUsername, CreatorUserName, OwnerUserName to the list.
                //
                // ---------------------------------------------------------------

                var queryClauses = new List<QueryClause>();

                // filter out disabled issues
                queryClauses.Add(new QueryClause("AND", "iv.[Disabled]", "=", "0", SqlDbType.Int));

                // if the user wants to exclude closed issues then filter the closed flag otherwise don't bother
                if (chkExcludeClosedIssues.Checked)
                    queryClauses.Add(new QueryClause("AND", "iv.[IsClosed]", "=", "0", SqlDbType.Int));

                queryClauses.Add(new QueryClause("AND (", "1", "=", "2", SqlDbType.NVarChar));
                queryClauses.Add(new QueryClause("OR", "iv.[IssueId]", "LIKE", strLike, SqlDbType.NVarChar));

                if (chkSearchTitle.Checked || chkSearchDesc.Checked)
                {
                    if (chkSearchTitle.Checked)
                    {
                        queryClauses.Add(new QueryClause("OR", "iv.[IssueTitle]", "LIKE", strLike, SqlDbType.NVarChar));
                        if (srchHtmlcode)
                            queryClauses.Add(new QueryClause("OR", "iv.[IssueTitle]", "LIKE", strHtmlLike,
                                SqlDbType.NVarChar));
                    }

                    if (chkSearchDesc.Checked)
                    {
                        queryClauses.Add(new QueryClause("OR", "iv.[IssueDescription]", "LIKE", strLike,
                            SqlDbType.NVarChar));
                        if (srchHtmlcode)
                            queryClauses.Add(new QueryClause("OR", "iv.[IssueDescription]", "LIKE", strHtmlLike,
                                SqlDbType.NVarChar));
                    }
                }

                queryClauses.Add(new QueryClause(")", "", "", "", SqlDbType.NVarChar));

                // Use the new Generic way to search with those QueryClauses
                var issues = IssueManager.PerformQuery(queryClauses, null, p.Id);

                queryClauses.Clear();

                _mainIssues.AddRange(issues);

                // ---------------------------------------------------------------
                // Search Comments
                //
                // ---------------------------------------------------------------

                if (!srchComments) continue;

                issues.Clear();
                issueComments.Clear();
                foundComments.Clear();

                queryClauses.Add(new QueryClause("AND", "iv.[Disabled]", "=", "0", SqlDbType.Int));

                // if the user wants to exclude closed issues then filter the closed flag otherwise don't bother
                // stuff the criteria into the first spot because we have an open nested criteria going on
                if (chkExcludeClosedIssues.Checked)
                    queryClauses.Insert(0, new QueryClause("AND", "iv.[IsClosed]", "=", "0", SqlDbType.Int));

                // Get ALL issues
                issues = IssueManager.PerformQuery(queryClauses, null, p.Id);

                // to the private check on all issues
                issues = IssueManager.StripPrivateIssuesForUser(issues, Security.GetUserName()).ToList();

                foreach (var iss in issues)
                {
                    // New Way
                    // Using the Generic Interface
                    var qryComment = new List<QueryClause>
                    {
                        new QueryClause("AND (", "Comment", "LIKE", strLike, SqlDbType.NVarChar)
                    };
                    if (srchHtmlcode)
                        qryComment.Add(new QueryClause("OR", "Comment", "LIKE", strHtmlLike, SqlDbType.NVarChar));

                    // close parenthesis 
                    qryComment.Add(new QueryClause(")", "", "", "", SqlDbType.NVarChar));

                    issueComments = IssueCommentManager.PerformQuery(iss.Id, qryComment);

                    // Did we find anything?
                    if (issueComments.Count <= 0) continue;

                    _mainComments.AddRange(issueComments);
                    _mainIssues.Add(iss);
                    // make sure we record the parent issue of the comment(s)
                }
            }

            // ---------------------------------------------------------------
            // Clean up duplicates and sort
            // 
            // mainIssues and mainComments
            // Sorry for the horrible variable names
            //
            // --------------------------------------------------------------- 


            var tmpIss = (from iss1 in _mainIssues
                orderby iss1.ProjectId, iss1.Id descending
                select iss1).Distinct(new DistinctIssueComparer());


            var tmpIssues1 = new List<Issue>();
            tmpIssues1.AddRange(tmpIss);
            _mainIssues.Clear();
            _mainIssues.AddRange(tmpIssues1);

            // to the private check on all issues
            _mainIssues = IssueManager.StripPrivateIssuesForUser(_mainIssues, Security.GetUserName()).ToList();

            // mainIssues list should be pure now
            var tmpComm = (from comm in _mainComments
                    orderby comm.IssueId, comm.Id
                    select comm)
                .Distinct();

            var tmpComm1 = new List<IssueComment>();
            tmpComm1.AddRange(tmpComm);
            _mainComments.Clear();
            _mainComments.AddRange(tmpComm1);
        }

        protected void chkHistory_CheckedChanged(object sender, EventArgs e)
        {
        }

        protected void chkCommentUsername_CheckedChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Handles the ItemDataBound event of the SearchProjectRepeater control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void SearchProjectRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var p = (Project) e.Item.DataItem;
            var rptr = e.Item.FindControl("IssuesList") as Repeater;

            if (rptr == null)
            {
                e.Item.Visible = false;
                return;
            }

            ((HyperLink) e.Item.FindControl("ProjectLink")).Text = $"{p.Name} ({p.Code})";

            // Chop description at 50 chars
            ((Label) e.Item.FindControl("ProjectDescription")).Text = p.Description.Length > 100
                ? p.Description.Substring(0, 100) + "..."
                : p.Description;


            // Only get this projects issues using LINQ
            var filteredIssues = new List<Issue>(from iss in _mainIssues
                where p.Id == iss.ProjectId
                select iss);

            // Are there any results
            if (filteredIssues.Count > 0)
            {
                ((HyperLink) e.Item.FindControl("IssuesCount")).Text =
                    string.Format(GetLocalString("IssuesFound"), filteredIssues.Count);

                rptr.DataSource = filteredIssues;
                rptr.DataBind();
            }
            else
            {
                e.Item.Visible = false;
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the IssuesCommentListRepeater control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void IssuesCommentListRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var lblcomm = (Label) e.Item.FindControl("lblComment");
            var ic = (IssueComment) e.Item.DataItem;

            // Prevent XSS
            lblcomm.Text = Server.HtmlEncode(IssueCommentManager.GetShortTextComment(ic.Comment));
        }


        /// <summary>
        /// Handles the ItemDataBound event of the IssuesListRepeater control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void IssuesListRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;
            var i = (Issue) e.Item.DataItem;
            // Only get this projects issues using LINQ
            var filteredComm = new List<IssueComment>(from comm in _mainComments
                where i.Id == comm.IssueId
                select comm);

            var pnl1 = (Panel) e.Item.FindControl("pnlIssueComments");
            var row = e.Item.FindControl("CommentsRow");

            // Are there any results
            if (filteredComm.Count > 0)
            {
                var rptr = (Repeater) e.Item.FindControl("IssuesCommentList");

                var lbl1 = (Label) pnl1.FindControl("lblCommentCount");
                var linkText = $"<a href='../Issues/IssueDetail.aspx?id={i.Id}'>{i.FullId}</a>";
                var fullText = string.Format(GetLocalString("MatchingCommentsFound"), filteredComm.Count, linkText);
                lbl1.Text = $"<em>{fullText}.</em>";

                rptr.DataSource = filteredComm;
                rptr.DataBind();

                row.Visible = true;
            }
            else
            {
                row.Visible = false;
                //var rptr = ((Repeater)e.Item.FindControl("IssuesCommentList"));
                //rptr.Visible = false;
                //pnl1.Visible = false;
            }
        }
    }
}