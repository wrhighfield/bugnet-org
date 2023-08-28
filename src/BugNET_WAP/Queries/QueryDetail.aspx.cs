using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using BugNET.BLL;
using BugNET.Common;
using BugNET.Entities;
using BugNET.UI;
using BugNET.UserControls;

namespace BugNET.Queries
{
    /// <summary>
    /// This page displays the interface for building a query against the
    /// issues database.
    /// </summary>
    public partial class QueryDetail : BugNetBasePage
    {
        protected DisplayIssues DisplayIssuesControl;

        private int queryId;

        /// <summary>
        ///  The number of query clauses is stored in view state so that the
        /// interface can be recreated on each page request.
        /// </summary>
        /// <value>The clause count.</value>
        private int ClauseCount
        {
            get => ViewState.Get("ClauseCount", 0);
            set => ViewState.Set("ClauseCount", value);
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


        /// <summary>
        /// Builds the user interface for selecting query fields.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            Message1.Visible = false;

            queryId = Request.Get("id", Globals.NewId);
            ProjectId = Request.Get("pid", Globals.NewId);

            // If no project id or query id then redirect away
            if (ProjectId == 0) ErrorRedirector.TransferToSomethingMissingPage(Page);

            if (!Page.User.Identity.IsAuthenticated || (queryId != 0 &&
                                                        !UserManager.HasPermission(ProjectId,
                                                            Common.Permission.EditQuery.ToString())))
                Response.Redirect("~/Errors/AccessDenied");

            if (!Page.User.Identity.IsAuthenticated ||
                !UserManager.HasPermission(ProjectId, Common.Permission.AddQuery.ToString()))
            {
                SaveQueryForm.Visible = false;
                pnlSaveQuery.Visible = false;
            }

            DisplayClauses();

            if (!Page.IsPostBack)
            {
                lblProjectName.Text = ProjectManager.GetById(ProjectId).Name;

                Results.Visible = false;

                if (queryId != 0)
                {
                    //edit query.
                    plhClauses.Controls.Clear();
                    var query = QueryManager.GetById(queryId);
                    txtQueryName.Text = query.Name;
                    chkGlobalQuery.Checked = query.IsPublic;
                    //ClauseCount = 0;

                    foreach (var qc in query.Clauses)
                    {
                        ClauseCount++;
                        AddClause(true, qc);
                    }
                }
                else
                {
                    ClauseCount = 3;
                    DisplayClauses();
                }


                BindQueryFieldTypes();
            }

            // The ExpandIssuePaths method is called to handle
            // the SiteMapResolve event.
            SiteMap.SiteMapResolve += ExpandIssuePaths;
        }

        /// <summary>
        /// Expands the issue paths.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Web.SiteMapResolveEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private SiteMapNode ExpandIssuePaths(object sender, SiteMapResolveEventArgs e)
        {
            if (SiteMap.CurrentNode == null) return null;
            var currentNode = SiteMap.CurrentNode.Clone(true);
            var tempNode = currentNode;

            // The current node, and its parents, can be modified to include
            // dynamic query string information relevant to the currently
            // executing request.
            if (ProjectId != 0) tempNode.Url = $"{tempNode.Url}?id={ProjectId}";

            if (null != (tempNode = tempNode.ParentNode)) tempNode.Url = $"~/Queries/QueryList.aspx?pid={ProjectId}";

            return currentNode;
        }

        /// <summary>
        ///When a user pages or sorts the issues displayed by the DisplayIssues
        /// user control, this method is called. This method simply calls the ExecuteQuery()
        /// method to rebind the DisplayIssues control to its data source.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void IssuesRebind(object s, EventArgs e)
        {
            ExecuteQuery();
        }

        /// <summary>
        /// This method adds the number of clauses stored in the ClauseCount property.
        /// </summary>
        private void DisplayClauses()
        {
            for (var i = 0; i < ClauseCount; i++)
                AddClause();
        }

        /// <summary>
        /// This method iterates through each of the query clauses and binds
        /// the clause to the proper data.
        ///
        /// </summary>
        private void BindQueryFieldTypes()
        {
            foreach (PickQueryField ctlPickQueryField in plhClauses.Controls) ctlPickQueryField.ProjectId = ProjectId;
        }

        /// <summary>
        /// This method adds a new query clause to the user interface.
        /// </summary>
        /// <param name="bindData">if set to <c>true</c> [bind data].</param>
        /// <param name="queryClause"></param>
        private void AddClause(bool bindData = false, QueryClause queryClause = null)
        {
            var ctlPickQueryField = (PickQueryField) Page.LoadControl("~/UserControls/PickQueryField.ascx");

            plhClauses.Controls.Add(ctlPickQueryField);
            ctlPickQueryField.ProjectId = ProjectId;
            if (bindData)
                ctlPickQueryField.QueryClause = queryClause;
        }

        /// <summary>
        ///This method is called when a user clicks the Add Clause button.
        /// </summary>
        private void AddClauseClick()
        {
            ClauseCount++;
            AddClause(true);
            pnlRemoveClause.Enabled = true;
        }

        /// <summary>
        /// This method is called when a user clicks the Remove Clause button.
        /// </summary>
        private void RemoveClause()
        {
            if (ClauseCount > 1)
            {
                ClauseCount--;
                plhClauses.Controls.RemoveAt(plhClauses.Controls.Count - 1);
            }

            if (ClauseCount < 2)
                pnlRemoveClause.Enabled = false;
        }

        /// <summary>
        /// This method is called when a user clicks the Remove Clause button.
        /// </summary>
        private void PerformQuery()
        {
            DisplayIssuesControl.CurrentPageIndex = 0;
            ExecuteQuery();
        }

        /// <summary>
        /// This method is called when a user clicks the Save Query button.
        /// The method saves the query to a database table.
        /// </summary>
        private void SaveQuery()
        {
            if (!Page.IsValid) return;

            var queryName = txtQueryName.Text.Trim();
            var userName = Security.GetUserName();

            if (queryName == string.Empty) return;

            var queryClauses = BuildQuery();

            if (queryClauses.Count == 0) return;

            var query = new Query
            {
                Id = queryId,
                Name = queryName,
                IsPublic = chkGlobalQuery.Checked,
                Clauses = queryClauses
            };

            var success = QueryManager.SaveOrUpdate(userName, ProjectId, query);

            if (success)
                Response.Redirect($"QueryList.aspx?pid={ProjectId}");
            else
                Message1.ShowErrorMessage(GetLocalString("SaveQueryError"));
        }


        /// <summary>
        /// This method executes a query and displays the results.
        /// </summary>
        private void ExecuteQuery()
        {
            var queryClauses = BuildQuery();

            if (queryClauses.Count > 0)
                try
                {
                    var sorter = DisplayIssuesControl.SortString;

                    if (sorter.Trim().Length.Equals(0)) sorter = "iv.[IssueId] DESC";

                    var sortColumns = (
                        from sort in sorter.Split(',')
                        select sort.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries)
                        into args
                        where args.Length.Equals(2)
                        select new KeyValuePair<string, string>(args[0], args[1])).ToList();

                    // add the disabled query filter since the UI cannot add this
                    queryClauses.Insert(0, new QueryClause("AND", "iv.[Disabled]", "=", "0", SqlDbType.Int));

                    var colIssues = IssueManager.PerformQuery(queryClauses, sortColumns, ProjectId);
                    DisplayIssuesControl.DataSource = colIssues;
                    Results.Visible = true;
                    DisplayIssuesControl.DataBind();
                }
                catch
                {
                    Message1.ShowErrorMessage(GetLocalString("RunQueryError"));
                }
            else
                Message1.ShowWarningMessage(GetLocalString("SelectOneQueryClause"));
        }

        /// <summary>
        /// This method builds a database query by iterating through each query clause.
        /// </summary>
        /// <returns></returns>
        private List<QueryClause> BuildQuery()
        {
            return (
                from PickQueryField ctlPickQuery in plhClauses.Controls
                select ctlPickQuery.QueryClause
                into objQueryClause
                where objQueryClause != null
                select objQueryClause).ToList();
        }

        private void CancelQuery()
        {
            Response.Redirect($"~/Queries/QueryList.aspx?pid={ProjectId}", true);
        }

        protected void btnAddClause_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            AddClauseClick();
        }

        protected void lbAddClause_Click(object sender, EventArgs e)
        {
            AddClauseClick();
        }

        protected void btnRemoveClause_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            RemoveClause();
        }

        protected void lbRemoveClause_Click(object sender, EventArgs e)
        {
            RemoveClause();
        }

        protected void btnPerformQuery_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            PerformQuery();
        }

        protected void lbPerformQuery_Click(object sender, EventArgs e)
        {
            PerformQuery();
        }

        protected void btnSaveQuery_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            SaveQuery();
        }

        protected void lbSaveQuery_Click(object sender, EventArgs e)
        {
            SaveQuery();
        }

        protected void btnCancel_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            CancelQuery();
        }

        protected void lbCancel_Click(object sender, EventArgs e)
        {
            CancelQuery();
        }
    }
}