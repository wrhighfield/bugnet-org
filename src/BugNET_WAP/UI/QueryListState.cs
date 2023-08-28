using System;

namespace BugNET.UI
{
    /// <summary>
    /// Class to save the state of the QueryList page. An object of this class is saved in the session
    /// so that the QueryList page state can be restored. 
    /// </summary>
    [Serializable]
    public class QueryListState
    {
        private int queryId;
        private int projectId;
        private int issueListPageIndex;
        private string sortField;
        private bool sortAscending;
        private int pageSize;


        /// <summary>
        /// Gets or sets the query id.
        /// </summary>
        /// <value>The query id.</value>
        public int QueryId
        {
            get => queryId;
            set => queryId = value;
        }

        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        /// <value>The size of the page.</value>
        public int PageSize
        {
            get => pageSize;
            set => pageSize = value;
        }

        /// <summary>
        /// Gets or sets the sort field.
        /// </summary>
        /// <value>The sort field.</value>
        public string SortField
        {
            get => sortField;
            set => sortField = value;
        }


        /// <summary>
        /// Gets or sets a value indicating whether [sort ascending].
        /// </summary>
        /// <value><c>true</c> if [sort ascending]; otherwise, <c>false</c>.</value>
        public bool SortAscending
        {
            get => sortAscending;
            set => sortAscending = value;
        }

        /// <summary>
        /// Gets or sets the project id.
        /// </summary>
        /// <value>The project id.</value>
        public int ProjectId
        {
            get => projectId;
            set => projectId = value;
        }

        /// <summary>
        /// Gets or sets the index of the issue list page.
        /// </summary>
        /// <value>The index of the issue list page.</value>
        public int IssueListPageIndex
        {
            get => issueListPageIndex;
            set => issueListPageIndex = value;
        }
    }
}