using System;
using BugNET.BLL;
using BugNET.Common;
using BugNET.UI;

namespace BugNET.Issues.UserControls
{
    public partial class Revisions : BugNetUserControl, IIssueTab
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        #region IIssueTab Members

        /// <summary>
        /// Gets or sets the issue id.
        /// </summary>
        /// <value>The issue id.</value>
        public int IssueId
        {
            get => ViewState.Get("IssueId", 0);
            set => ViewState.Set("IssueId", value);
        }

        /// <summary>
        /// Gets or sets the project id.
        /// </summary>
        /// <value>The project id.</value>
        public int ProjectId
        {
            get => ViewState.Get("ProjectId", 0);
            set => ViewState.Set("ProjectId", value);
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            //IssueRevisionsDataGrid.Columns[0].HeaderText = GetLocalString("IssueRevisionsDataGrid.RevisionHeader.Text").ToString();
            //IssueRevisionsDataGrid.Columns[1].HeaderText = GetLocalString("IssueRevisionsDataGrid.AuthorHeader.Text").ToString();
            //IssueRevisionsDataGrid.Columns[2].HeaderText = GetLocalString("IssueRevisionsDataGrid.RevisionDateHeader.Text").ToString();
            //IssueRevisionsDataGrid.Columns[3].HeaderText = GetLocalString("IssueRevisionsDataGrid.RepositoryHeader.Text").ToString();
            //IssueRevisionsDataGrid.Columns[4].HeaderText = GetLocalString("IssueRevisionsDataGrid.MessageHeader.Text").ToString();

            BindIssueRevisions();
        }

        #endregion

        private void BindIssueRevisions()
        {
            var revisions = IssueRevisionManager.GetByIssueId(IssueId);
            if (revisions.Count == 0)
            {
                IssueRevisionsLabel.Text = GetLocalString("NoRevisions");
                IssueRevisionsLabel.Visible = true;
                IssueRevisionsDataGrid.Visible = false;
            }
            else
            {
                IssueRevisionsDataGrid.DataSource = revisions;
                IssueRevisionsDataGrid.DataKeyField = "IssueId";
                IssueRevisionsDataGrid.DataBind();
                IssueRevisionsLabel.Visible = false;
                IssueRevisionsDataGrid.Visible = true;
            }
        }
    }
}