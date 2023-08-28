using System;
using BugNET.BLL;
using BugNET.Common;
using log4net;
using System.Collections.Generic;
using BugNET.UI;
using Microsoft.AspNet.FriendlyUrls;

namespace BugNET.Queries
{
    /// <summary>
    /// This page displays a list of existing queries
    /// </summary>
    public partial class QueryList : BugNetBasePage
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(QueryList));
        private const string QUERY_LIST_STATE = "QueryListState";

        /// <summary>
        /// Binds the queries.
        /// </summary>
        private void BindQueries()
        {
            dropQueries.DataSource = QueryManager.GetByUsername(User.Identity.Name, ProjectId);
            dropQueries.DataBind();

            pnlDeleteQuery.Visible = dropQueries.DataSource.Count > 0;
            pnlEditQuery.Visible = pnlDeleteQuery.Visible;

            if (!Page.User.Identity.IsAuthenticated ||
                !UserManager.HasPermission(ProjectId, Permission.DeleteQuery.ToString()))
                pnlDeleteQuery.Visible = false;

            if (!Page.User.Identity.IsAuthenticated ||
                !UserManager.HasPermission(ProjectId, Permission.EditQuery.ToString())) pnlEditQuery.Visible = false;
        }

        /// <summary>
        /// Edits the query.
        /// </summary>
        private void EditQuery()
        {
            if (dropQueries.SelectedValue == 0)
                return;

            Response.Redirect($"~/Queries/QueryDetail.aspx?id={dropQueries.SelectedValue}&pid={ProjectId}", true);
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        private void ExecuteQuery()
        {
            if (dropQueries.SelectedValue == 0)
                return;


            try
            {
                var sortColumns = new List<KeyValuePair<string, string>>();

                var sorter = ctlDisplayIssues.SortString;

                if (sorter.Trim().Length.Equals(0)) sorter = "iv.[IssueId] DESC";

                foreach (var sort in sorter.Split(','))
                {
                    var args = sort.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                    if (args.Length.Equals(2))
                        sortColumns.Add(new KeyValuePair<string, string>(args[0], args[1]));
                }

                var colIssues = IssueManager.PerformSavedQuery(ProjectId, dropQueries.SelectedValue, sortColumns);
                ctlDisplayIssues.DataSource = colIssues;
                ctlDisplayIssues.RssUrl = string.Format("~/Feed.aspx?pid={1}&q={0}&channel=13",
                    dropQueries.SelectedValue, ProjectId);

                // Only bind results if there is no error.                
                ctlDisplayIssues.DataBind();

                Results.Visible = true;
            }
            catch (Exception ex)
            {
                lblError.Text = GetLocalString("QueryError");
                if (Log.IsErrorEnabled)
                    Log.Warn($"Error Running Saved Query. Project Id:{ProjectId} Query Id:{dropQueries.SelectedValue}",
                        ex);
            }
        }

        /// <summary>
        /// Adds the query.
        /// </summary>
        private void AddQuery()
        {
            Response.Redirect($"~/Queries/QueryDetail.aspx?pid={ProjectId}");
        }

        /// <summary>
        /// Deletes the query.
        /// </summary>
        private void DeleteQuery()
        {
            if (dropQueries.SelectedValue == 0)
                return;

            QueryManager.Delete(dropQueries.SelectedValue);
            BindQueries();
        }

        #region Web Form Designer generated code

        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ctlDisplayIssues.RebindCommand += new System.EventHandler(IssuesRebind);
        }

        #endregion

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                var segments = Request.GetFriendlyUrlSegments();
                ProjectId = int.Parse(segments[0]);
            }
            catch
            {
                ProjectId = Request.QueryString.Get("pid", 0);
            }


            if (Page.IsPostBack) return;

            // If don't know project or issue then redirect to something missing page
            if (ProjectId == 0)
                ErrorRedirector.TransferToSomethingMissingPage(Page);

            ConfirmDeleteText.Value = GetLocalString("ConfirmDelete").JsEncode();
            var p = ProjectManager.GetById(ProjectId);
            if (p != null)
            {
                ltProject.Text = p.Name;
                litProjectCode.Text = p.Code;
            }

            btnDeleteQuery.OnClientClick = "return confirmDelete();";
            lbDeleteQuery.OnClientClick = "return confirmDelete();";

            ctlDisplayIssues.PageSize = UserManager.GetProfilePageSize();
            ctlDisplayIssues.CurrentPageIndex = 0;
            Results.Visible = false;

            var state = (QueryListState) Session[QUERY_LIST_STATE];

            BindQueries();

            if (state == null) return;

            if (ProjectId > 0 && ProjectId != state.ProjectId)
            {
                Session.Remove(QUERY_LIST_STATE);
            }
            else
            {
                if (state.QueryId != 0)
                    dropQueries.SelectedValue = state.QueryId;
                ProjectId = state.ProjectId;
                ctlDisplayIssues.CurrentPageIndex = state.IssueListPageIndex;
                ctlDisplayIssues.SortField = state.SortField;
                ctlDisplayIssues.SortAscending = state.SortAscending;
                ctlDisplayIssues.PageSize = state.PageSize;
            }

            ExecuteQuery();
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
            var state = (QueryListState) Session[QUERY_LIST_STATE] ?? new QueryListState();
            state.QueryId = dropQueries.SelectedValue;
            state.ProjectId = ProjectId;
            state.IssueListPageIndex = ctlDisplayIssues.CurrentPageIndex;
            state.SortField = ctlDisplayIssues.SortField;
            state.SortAscending = ctlDisplayIssues.SortAscending;
            state.PageSize = ctlDisplayIssues.PageSize;
            Session[QUERY_LIST_STATE] = state;
        }

        /// <summary>
        /// Rebinds the issues
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void IssuesRebind(object s, EventArgs e)
        {
            ExecuteQuery();
        }

        protected void imgPerformQuery_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            ExecuteQuery();
        }

        protected void btnAddQuery_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            AddQuery();
        }

        protected void lbAddQuery_Click(object sender, EventArgs e)
        {
            AddQuery();
        }

        protected void btnDeleteQuery_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            DeleteQuery();
        }

        protected void lbDeleteQuery_Click(object sender, EventArgs e)
        {
            DeleteQuery();
        }

        protected void btnEditQuery_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            EditQuery();
        }

        protected void lbEditQuery_Click(object sender, EventArgs e)
        {
            EditQuery();
        }
    }
}