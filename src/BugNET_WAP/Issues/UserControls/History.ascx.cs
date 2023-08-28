using System;
using BugNET.BLL;
using BugNET.Common;
using BugNET.UI;

namespace BugNET.Issues.UserControls
{
    public partial class History : BugNetUserControl, IIssueTab
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
            BindHistory();
        }

        #endregion

        /// <summary>
        /// Binds the history.
        /// </summary>
        private void BindHistory()
        {
            HistoryDataGrid.Columns[0].HeaderText = GetLocalString("HistoryDataGrid.DateModifiedHeader.Text");
            HistoryDataGrid.Columns[1].HeaderText = GetLocalString("HistoryDataGrid.CreatorHeader.Text");
            HistoryDataGrid.Columns[2].HeaderText = GetLocalString("HistoryDataGrid.FieldChangedHeader.Text");
            HistoryDataGrid.Columns[3].HeaderText = GetLocalString("HistoryDataGrid.OldValueHeader.Text");
            HistoryDataGrid.Columns[4].HeaderText = GetLocalString("HistoryDataGrid.NewValueHeader.Text");

            var history = IssueHistoryManager.GetByIssueId(IssueId);

            if (history.Count == 0)
            {
                lblHistory.Text = GetLocalString("NoHistory");
                lblHistory.Visible = true;
            }
            else
            {
                HistoryDataGrid.DataSource = history;
                HistoryDataGrid.DataBind();
            }
        }
    }
}