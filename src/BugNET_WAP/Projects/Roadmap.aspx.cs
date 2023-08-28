using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.AspNet.FriendlyUrls;
using BugNET.BLL;
using BugNET.Common;
using BugNET.Entities;
using BugNET.UI;

namespace BugNET.Projects
{
    /// <summary>
    /// Project Road Map
    /// </summary>
    public partial class Roadmap : BugNetBasePage
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
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

                // If don't know project or issue then redirect to something missing page
                if (ProjectId == 0)
                {
                    ErrorRedirector.TransferToSomethingMissingPage(Page);
                    return;
                }

                var p = ProjectManager.GetById(ProjectId);

                if (p == null || p.Disabled)
                {
                    ErrorRedirector.TransferToSomethingMissingPage(Page);
                    return;
                }

                SortHeader = "Id";
                SortAscending = false;
                SortField = "iv.[IssueId]";

                ltProject.Text = p.Name;
                litProjectCode.Text = p.Code;

                Page.Title = $@"{p.Name} ({p.Code}) - {GetLocalString("Roadmap")}";


                BindRoadmap();
            }


            // The ExpandIssuePaths method is called to handle
            // the SiteMapResolve event.
            SiteMap.SiteMapResolve += ExpandProjectPaths;
        }

        /// <summary>
        /// Binds the roadmap.
        /// </summary>
        private void BindRoadmap()
        {
            RoadmapRepeater.DataSource = MilestoneManager.GetByProjectId(ProjectId, false);
            RoadmapRepeater.DataBind();
        }

        /// <summary>
        /// Gets or sets the sort field.
        /// </summary>
        /// <value>The sort field.</value>
        private string SortField
        {
            get => ViewState.Get("SortField", string.Empty);
            set
            {
                if (value == SortField)
                    // same as current sort file, toggle sort direction
                    SortAscending = !SortAscending;

                ViewState.Set("SortField", value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [sort ascending].
        /// </summary>
        /// <value><c>true</c> if [sort ascending]; otherwise, <c>false</c>.</value>
        private bool SortAscending
        {
            get => ViewState.Get("SortAscending", true);
            set => ViewState.Set("SortAscending", value);
        }

        private string SortHeader
        {
            get => ViewState.Get("SortHeader", string.Empty);
            set => ViewState.Set("SortHeader", value);
        }

        protected void SortIssueClick(object sender, EventArgs e)
        {
            if (sender is LinkButton button)
            {
                SortField = button.CommandArgument;
                SortHeader = button.CommandName;
            }

            BindRoadmap();
        }

        /// <summary>
        /// Handles the Unload event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Unload(object sender, EventArgs e)
        {
            //remove the event handler
            SiteMap.SiteMapResolve -= ExpandProjectPaths;
        }

        /// <summary>
        /// Expands the project paths.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Web.SiteMapResolveEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private SiteMapNode ExpandProjectPaths(object sender, SiteMapResolveEventArgs e)
        {
            var currentNode = SiteMap.CurrentNode.Clone(true);
            var tempNode = currentNode;

            // The current node, and its parents, can be modified to include
            // dynamic querystring information relevant to the currently
            // executing request.
            if (ProjectId != 0) tempNode.Url = $"{tempNode.Url}?pid={ProjectId}";

            if (null != (tempNode = tempNode.ParentNode) &&
                ProjectId != 0)
                tempNode.Url = $"{tempNode.Url}?pid={ProjectId}";

            return currentNode;
        }

        /// <summary>
        /// Handles the ItemCreated event of the rptRoadMap control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptRoadMap_ItemCreated(object sender, RepeaterItemEventArgs e)
        {
            switch (e.Item.ItemType)
            {
                case ListItemType.Header:
                    foreach (Control c in e.Item.Controls)
                    {
                        if (c.GetType() != typeof(HtmlTableCell) || c.ID != $"td{SortHeader}") continue;
                        var img = new Image
                        {
                            ImageUrl = $"~/images/{(SortAscending ? "bullet_arrow_up" : "bullet_arrow_down")}.png",
                            CssClass = "icon"
                        };
                        // setting the dynamically URL of the image
                        c.Controls.Add(img);
                    }

                    break;
            }
        }

        /// <summary>
        /// Creates descriptive text for milestone due date
        /// </summary>
        /// <param name="dueDate">Milestone due date</param>
        /// <param name="completed">Milestone completed flag</param>
        /// <returns>Descriptive text</returns>
        private string GetDueDateDescription(DateTime? dueDate, bool completed)
        {
            var response = GetLocalString("NoDueDate");

            if (dueDate == null) return response;
            var date = (DateTime) dueDate;
            if (date.Date.Equals(DateTime.Now.Date))
            {
                response = $"{Resources.SharedResources.Due} <b>{Resources.SharedResources.Today}</b>";
            }
            else if (date.Date.Equals(DateTime.Now.AddDays(1).Date))
            {
                response = $"{Resources.SharedResources.Due} <b>{Resources.SharedResources.Tomorrow}</b>";
            }
            else if (date.Date.Equals(DateTime.Now.AddDays(-1).Date))
            {
                response = $"{Resources.SharedResources.Due} <b>{Resources.SharedResources.Yesterday}</b>";
            }
            else
            {
                string diffName;
                var diffDays = (date.Date - DateTime.Now.Date).Days;

                if (Math.Abs(diffDays) < 14)
                    diffName = $"{Math.Abs(diffDays)} {Resources.SharedResources.Days}";
                else if (Math.Abs(diffDays) < 61)
                    diffName = $"{Math.Abs(Math.Round((decimal) diffDays / 7))} {Resources.SharedResources.Weeks}";
                else if (Math.Abs(diffDays) < 730)
                    diffName =
                        $"{Math.Abs(Math.Round((decimal) diffDays / 30))} {Resources.SharedResources.Months}";
                else
                    diffName =
                        $"{Math.Abs(Math.Round((decimal) diffDays / 365))} {Resources.SharedResources.Years}";

                if (diffDays < 0)
                    response = completed
                        ? Resources.SharedResources.Finished
                        : $"<b>{diffName}</b> {Resources.SharedResources.Late}";
                else
                    response = string.Format("{1} {0}", diffName, Resources.SharedResources.DueIn);
            }

            response += $" ({date.Date.ToShortDateString()})";
            return response;
        }

        /// <summary>
        /// Handles the ItemDataBound event of the RoadmapRepeater control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void RoadmapRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            if (!(e.Item.DataItem is Milestone m)) return;

            if (!string.IsNullOrWhiteSpace(m.Notes))
                ((Label) e.Item.FindControl("MilestoneNotes")).Text = " - " + m.Notes;
            var dueDate = (Label) e.Item.FindControl("lblDueDate");

            if (m.DueDate.HasValue)
            {
                var date = (DateTime) m.DueDate;
                dueDate.Text = GetDueDateDescription(date, false);
            }
            else
            {
                dueDate.Text = GetLocalString("NoReleaseDate");
            }

            if (!(e.Item.FindControl("IssuesList") is Repeater list)) return;

            var queryClauses = new List<QueryClause>
            {
                new QueryClause("AND", "iv.[IssueMilestoneId]", "=", m.Id.ToString(), SqlDbType.Int),
                new QueryClause("AND", "iv.[Disabled]", "=", "0", SqlDbType.Int)
            };

            var sortString = SortAscending ? "ASC" : "DESC";

            var sortList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(SortField, sortString)
            };

            var issueList = IssueManager.PerformQuery(queryClauses, sortList, ProjectId);

            // private issue check
            issueList = IssueManager.StripPrivateIssuesForUser(issueList, Security.GetUserName()).ToList();

            if (issueList.Count > 0)
            {
                list.DataSource = issueList;
                list.DataBind();
            }
            else
            {
                e.Item.Visible = false;
            }

            var nfi = new NumberFormatInfo {PercentDecimalDigits = 0};

            var progressValues = ProjectManager.GetRoadMapProgress(ProjectId, m.Id);
            var issueCount = progressValues[1];
            var resolvedCount = progressValues[0];
            var percent = issueCount.Equals(0) ? 0 : resolvedCount.To<double>() / issueCount.To<double>();
            var pct = percent.ToString("P", nfi);

            var match = Regex.Match(pct, @"\d+").Value.ToOrDefault(0);

            ((Label) e.Item.FindControl("lblProgress")).Text = string.Format(GetLocalString("ProgressMessage"),
                progressValues[0], progressValues[1]);

            ((HtmlControl) e.Item.FindControl("ProgressBar")).Attributes.CssStyle.Add("width", $"{match}%");
            ((HtmlControl) e.Item.FindControl("ProgressBar")).Attributes.Add("aria-valuenow", match.ToString());
            ((HtmlControl) e.Item.FindControl("ProgressBar")).Controls.Add(new LiteralControl($"{match}%"));

            ((HyperLink) e.Item.FindControl("MilestoneLink")).NavigateUrl =
                string.Format(Page.ResolveUrl("~/Issues/IssueList.aspx") + "?pid={0}&m={1}", ProjectId, m.Id);
            ((HyperLink) e.Item.FindControl("MilestoneLink")).Text = m.Name;
        }
    }
}