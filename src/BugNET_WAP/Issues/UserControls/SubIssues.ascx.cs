using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using BugNET.BLL;
using BugNET.Common;
using BugNET.Entities;
using BugNET.UI;

namespace BugNET.Issues.UserControls
{
    /// <summary>
    ///		Summary description for SubIssues.
    /// </summary>
    public partial class SubIssues : BugNetUserControl, IIssueTab
    {
        /// <summary>
        /// 
        /// </summary>
        protected SubIssues()
        {
            ProjectId = 0;
            IssueId = 0;
        }

        /// <summary>
        /// Binds the related.
        /// </summary>
        private void BindRelated()
        {
            var issues = RelatedIssueManager.GetChildIssues(IssueId);

            if (issues.Count == 0)
            {
                NoIssuesLabel.Text = GetLocalString("NoSubIssues");
                NoIssuesLabel.Visible = true;
                grdIssues.Visible = false;
            }
            else
            {
                grdIssues.DataSource = issues;
                grdIssues.DataKeyField = "IssueId";
                grdIssues.DataBind();
                NoIssuesLabel.Visible = false;
                grdIssues.Visible = true;
            }
        }


        /// <summary>
        /// GRDs the bugs item data bound.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.DataGridItemEventArgs"/> instance containing the event data.</param>
        protected void GrdIssuesItemDataBound(object s, DataGridItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var cmdDelete = e.Item.FindControl("cmdDelete") as ImageButton;

            if (cmdDelete == null) return;
            cmdDelete.Visible = false;

            var entity = e.Item.DataItem as RelatedIssue;

            if (entity == null) return;

            // allow delete if user had the permission, the project admin or a super user trying to delete the comment.
            if (!UserManager.IsInRole(ProjectId, Common.Permission.DeleteSubIssue.ToString()) &&
                !UserManager.IsSuperUser() &&
                !UserManager.IsInRole(ProjectId, Globals.ProjectAdministratorRole)) return;

            cmdDelete.Visible = true;
            cmdDelete.OnClientClick = $"return confirm('{GetLocalString("RemoveSubIssue")}');";
        }

        /// <summary>
        /// GRDs the bugs item command.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.DataGridCommandEventArgs"/> instance containing the event data.</param>
        protected void GrdIssuesItemCommand(object s, DataGridCommandEventArgs e)
        {
            var commandArgument = e.CommandArgument.ToString();
            var commandName = e.CommandName.ToLower().Trim();
            var currentIssueId = Globals.NewId;

            switch (commandName)
            {
                case "delete":
                    currentIssueId = int.Parse(commandArgument);
                    RelatedIssueManager.DeleteChildIssue(IssueId, currentIssueId);
                    break;
            }

            if (currentIssueId > Globals.NewId)
            {
                var history = new IssueHistory
                {
                    IssueId = IssueId,
                    DateChanged = DateTime.Now,
                    CreatedUserName = Security.GetUserName(),
                    FieldChanged =
                        ResourceStrings.GetGlobalResource(GlobalResources.SharedResources, "ChildIssue", "Child Issue"),
                    OldValue = string.Empty,
                    NewValue = ResourceStrings.GetGlobalResource(GlobalResources.SharedResources, "Deleted", "Deleted"),
                    TriggerLastUpdateChange = true
                };

                IssueHistoryManager.SaveOrUpdate(history);

                var changes = new List<IssueHistory> {history};

                IssueNotificationManager.SendIssueNotifications(IssueId, changes);
            }

            BindRelated();
        }

        /// <summary>
        /// Adds the related issue.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void AddRelatedIssue(object s, EventArgs e)
        {
            if (IssueIdTextBox.Text == string.Empty) return;

            if (!Page.IsValid) return;

            SubIssuesMessage.Visible = false;

            var issueId = Utilities.ParseFullIssueId(IssueIdTextBox.Text.Trim());

            if (issueId <= Globals.NewId) return;

            RelatedIssueManager.CreateNewChildIssue(IssueId, issueId);

            var history = new IssueHistory
            {
                IssueId = IssueId,
                DateChanged = DateTime.Now,
                CreatedUserName = Security.GetUserName(),
                FieldChanged =
                    ResourceStrings.GetGlobalResource(GlobalResources.SharedResources, "ChildIssue", "Child Issue"),
                OldValue = string.Empty,
                NewValue = ResourceStrings.GetGlobalResource(GlobalResources.SharedResources, "Added", "Added"),
                TriggerLastUpdateChange = true
            };

            IssueHistoryManager.SaveOrUpdate(history);

            var changes = new List<IssueHistory> {history};

            IssueNotificationManager.SendIssueNotifications(IssueId, changes);

            IssueIdTextBox.Text = string.Empty;
            BindRelated();
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
            BindRelated();

            if (!Page.User.Identity.IsAuthenticated ||
                !UserManager.HasPermission(ProjectId, Common.Permission.DeleteSubIssue.ToString()))
                grdIssues.Columns[4].Visible = false;

            //check users role permission for adding a related issue
            if (!Page.User.Identity.IsAuthenticated ||
                !UserManager.HasPermission(ProjectId, Common.Permission.AddSubIssue.ToString()))
                AddSubIssuePanel.Visible = false;
        }

        #endregion
    }
}