using System;
using System.Web;
using System.Web.UI.WebControls;
using BugNET.BLL;
using BugNET.Common;
using BugNET.Entities;
using BugNET.UI;
using BugNET.UserControls;
using Microsoft.AspNet.FriendlyUrls;

namespace BugNET.Projects
{
    /// <summary>
    /// Summary description for BrowseProject.
    /// </summary>
    public partial class ProjectSummary : BugNetBasePage
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            // Put user code to initialize the page here
            if (Page.IsPostBack) return;

            try
            {
                var segments = Request.GetFriendlyUrlSegments();
                ProjectId = int.Parse(segments[0]);
            }
            catch
            {
                ProjectId = Request.QueryString.Get("pid", 0);
            }

            // BGN-1379
            if (ProjectId.Equals(0))
                ErrorRedirector.TransferToNotFoundPage(Page);

            BindProjectSummary();

            // SiteMap.SiteMapResolve += ExpandPaths;
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            //remove the event handler
            // SiteMap.SiteMapResolve -= ExpandPaths;
        }

        private SiteMapNode ExpandPaths(object sender, SiteMapResolveEventArgs e)
        {
            if (SiteMap.CurrentNode == null) return null;

            var currentNode = SiteMap.CurrentNode.Clone(true);
            var tempNode = currentNode;

            if (null != (tempNode = tempNode.ParentNode)) tempNode.Url = $"~/Issues/IssueList.aspx?pid={ProjectId}";

            return currentNode;
        }

        /// <summary>
        /// Binds the project summary.
        /// </summary>
        private void BindProjectSummary()
        {
            lnkRSSIssuesByCategory.NavigateUrl = $"~/Feed.aspx?pid={ProjectId}&channel=2";
            lnkRSSIssuesByAssignee.NavigateUrl = $"~/Feed.aspx?pid={ProjectId}&channel=6";
            lnkRSSIssuesByStatus.NavigateUrl = $"~/Feed.aspx?pid={ProjectId}&channel=3";
            lnkRSSIssuesByMilestone.NavigateUrl = $"~/Feed.aspx?pid={ProjectId}&channel=1";
            lnkRSSIssuesByPriority.NavigateUrl = $"~/Feed.aspx?pid={ProjectId}&channel=4";
            lnkRSSIssuesByType.NavigateUrl = $"~/Feed.aspx?pid={ProjectId}&channel=5";

            //Milestone
            var lsVersion = IssueManager.GetMilestoneCountByProjectId(ProjectId);

            //Status
            var lsStatus = IssueManager.GetStatusCountByProjectId(ProjectId);

            //Priority
            var lsPriority = IssueManager.GetPriorityCountByProjectId(ProjectId);

            //User
            var lsUser = IssueManager.GetUserCountByProjectId(ProjectId);

            //Type
            var lsType = IssueManager.GetTypeCountByProjectId(ProjectId);

            CategoryTreeView1.ProjectId = ProjectId;
            CategoryTreeView1.BindData();

            rptMilestonesOpenIssues.DataSource = lsVersion;
            rptIssueStatus.DataSource = lsStatus;
            rptPriorityOpenIssues.DataSource = lsPriority;
            rptAssigneeOpenIssues.DataSource = lsUser;
            rptTypeOpenIssues.DataSource = lsType;

            rptMilestonesOpenIssues.DataBind();
            rptIssueStatus.DataBind();
            rptPriorityOpenIssues.DataBind();
            rptAssigneeOpenIssues.DataBind();

            rptTypeOpenIssues.DataBind();

            var p = ProjectManager.GetById(ProjectId);
            litProject.Text = p.Name;
            litProjectCode.Text = p.Code;
        }

        /// <summary>
        /// Binds the data for the versions repeater
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void SummaryItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            switch (e.Item.ItemType)
            {
                case ListItemType.AlternatingItem:
                case ListItemType.Item:
                {
                    if (!(e.Item.DataItem is IssueCount data)) return;

                    var summaryLink = e.Item.FindControl("summaryLink") as HyperLink;
                    var summaryCount = e.Item.FindControl("summaryCount") as Label;
                    var summaryPercent = e.Item.FindControl("summaryPercent") as Label;

                    if (e.Item.FindControl("summaryImage") is TextImage summaryImage)
                    {
                        if (data.ImageUrl.Length > 0)
                            summaryImage.ImageUrl = data.ImageUrl;

                        if (data.Id.ToString().Equals(Globals.NewId.ToString()) ||
                            data.Id.ToString().Equals(Globals.EmptyGuid))
                            summaryImage.Visible = false;
                    }

                    if (summaryLink != null)
                    {
                        summaryLink.Text = data.Name.Trim();

                        // if the item is unassigned then apply the item name from the resource file
                        if (data.Id.Equals(0) || data.Id.Equals(Guid.Empty))
                            summaryLink.Text = GetGlobalString("SharedResources", "Unassigned");
                    }

                    if (summaryCount != null) summaryCount.Text = data.Count.ToString();

                    if (summaryPercent != null) summaryPercent.Text = $@"({data.Percentage}%)";
                }
                    break;
            }
        }
    }
}