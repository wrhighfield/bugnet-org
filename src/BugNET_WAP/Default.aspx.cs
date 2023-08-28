using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using BugNET.BLL;
using BugNET.Common;
using BugNET.Entities;
using BugNET.UI;

namespace BugNET
{
    /// <summary>
    /// Summary description for _Default.
    /// </summary>
    public partial class _Default : BugNetBasePage
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title =
                $@"{GetLocalString("Page.Title")} - {HostSettingManager.Get(HostSettingNames.ApplicationTitle)}";

            if (!Page.IsPostBack)
            {
                lblApplicationTitle.Text = HostSettingManager.Get(HostSettingNames.ApplicationTitle);
                WelcomeMessage.Text = HostSettingManager.Get(HostSettingNames.WelcomeMessage);

                Step1_Localize.Text =
                    GetWelcomeLocalizedText(1, ResolveUrl("~/Administration/Projects/AddProject.aspx"));
                Step2_Localize.Text =
                    GetWelcomeLocalizedText(2, ResolveUrl("~/Administration/Host/Settings.aspx?tid=1"));
                Step3_Localize.Text =
                    GetWelcomeLocalizedText(3, ResolveUrl("~/Administration/Host/Settings.aspx?tid=2"));
            }

            if (ProjectManager.GetAllProjects().Count == 0)
            {
                // the user is logged in and there are no projects, show the "blank slate" help message for new installs
                BlankSlate.Visible = true;
                return;
            }

            if (!Context.User.Identity.IsAuthenticated)
            {
                //get all public available projects here
                if (bool.Parse(HostSettingManager.Get(HostSettingNames.AnonymousAccess)))
                {
                    rptProject.DataSource = ProjectManager.GetPublicProjects();
                }
                else
                {
                    rptProject.Visible = false;
                    lblMessage.Text = GetLocalString("AnonymousAccessDisabled");
                    lblMessage.Visible = true;
                }
            }
            else
            {
                rptProject.DataSource = ProjectManager.GetByMemberUserName(User.Identity.Name);
            }

            rptProject.DataBind();

            if (!UserMessage.Visible)
                // remember that we could have set the message already!
                if (rptProject.Items.Count == 0)
                {
                    if (!Context.User.Identity.IsAuthenticated)
                    {
                        lblMessage.Text = GetLocalString("RegisterAndLoginMessage");
                        UserMessage.Visible = true;
                    }
                    else
                    {
                        lblMessage.Text = GetLocalString("NoProjectsToViewMessage");
                        UserMessage.Visible = true;
                    }
                }
        }

        private string GetWelcomeLocalizedText(int stepNumber, string linkUrl)
        {
            var messageFormat = GetLocalString($"Step{stepNumber}_MessageFormat");
            var linkText = GetLocalString($"Step{stepNumber}_LinkText");
            var link = $"<a href=\"{linkUrl}\">{Server.HtmlEncode(linkText)}</a>";
            return string.Format(messageFormat, link);
        }

        #region Web Form Designer generated code

        /// <summary>
        /// Overrides the default OnInit to provide a security check for pages
        /// </summary>
        /// <param name="e"></param>
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
            this.rptProject.ItemDataBound += new RepeaterItemEventHandler(rptProject_ItemDataBound);
        }

        #endregion

        /// <summary>
        /// Handles the ItemDataBound event of the rptProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        private void rptProject_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            //check permissions
            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                var p = (Project) e.Item.DataItem;

                if (!Context.User.Identity.IsAuthenticated ||
                    !UserManager.HasPermission(p.Id, Common.Permission.AddIssue.ToString()))
                    e.Item.FindControl("ReportIssue").Visible = false;

                if (!Context.User.Identity.IsAuthenticated ||
                    !UserManager.HasPermission(p.Id, Common.Permission.AdminEditProject.ToString()))
                    e.Item.FindControl("Settings").Visible = false;

                var ProjectImage = (Image) e.Item.FindControl("ProjectImage");
                ProjectImage.ImageUrl = $"~/DownloadAttachment.axd?id={p.Id}&mode=project";

                var OpenIssuesLink = (Label) e.Item.FindControl("OpenIssues");
                var NextMilestoneDue = (Label) e.Item.FindControl("NextMilestoneDue");
                var MilestoneComplete = (Label) e.Item.FindControl("MilestoneComplete");

                var milestone = string.Empty;

                var milestoneList = MilestoneManager.GetByProjectId(p.Id);
                milestoneList = milestoneList.FindAll(m => m.DueDate.HasValue && m.IsCompleted != true);

                if (milestoneList.Count > 0)
                {
                    var sortedMilestoneList = milestoneList.Sort<Milestone>("DueDate").ToList();
                    var mileStone = sortedMilestoneList[0];
                    if (mileStone != null)
                    {
                        milestone = ((DateTime) mileStone.DueDate).ToShortDateString();
                        var progressValues = ProjectManager.GetRoadMapProgress(p.Id, mileStone.Id);
                        if (progressValues[0] != 0 || progressValues[1] != 0)
                        {
                            double percent = progressValues[0] * 100 / progressValues[1];
                            MilestoneComplete.Text = $"{percent}%";
                        }
                        else
                        {
                            MilestoneComplete.Text = "0%";
                        }
                    }
                    else
                    {
                        milestone = GetLocalString("None");
                    }

                    NextMilestoneDue.Text = string.Format(GetLocalString("NextMilestoneDue"), milestone);
                }
                else
                {
                    NextMilestoneDue.Text =
                        string.Format(GetLocalString("NextMilestoneDue"), GetLocalString("NoDueDatesSet"));
                }

                var status = StatusManager.GetByProjectId(p.Id);

                if (status.Count > 0)
                {
                    //get total open issues
                    var queryClauses = new List<QueryClause>
                    {
                        new QueryClause("AND", "iv.[IsClosed]", "=", "0", SqlDbType.Int),
                        new QueryClause("AND", "iv.[Disabled]", "=", "0", SqlDbType.Int)
                    };

                    var issueList = IssueManager.PerformQuery(queryClauses, null, p.Id);

                    OpenIssuesLink.Text = string.Format(GetLocalString("OpenIssuesCount"), issueList.Count);

                    var closedStatus = status.FindAll(s => s.IsClosedState);

                    if (closedStatus.Count.Equals(0))
                        // No open issue statuses means there is a problem with the setup of the system.
                        OpenIssuesLink.Text = GetLocalString("NoClosedStatus");
                }
                else
                {
                    // Warn users of a problem
                    OpenIssuesLink.Text = GetLocalString("NoStatusSet");
                }


                var atu = (HyperLink) e.Item.FindControl("AssignedToUser");
                var AssignedUserFilter = e.Item.FindControl("AssignedUserFilter");
                if (Context.User.Identity.IsAuthenticated &&
                    ProjectManager.IsUserProjectMember(User.Identity.Name, p.Id))
                {
                    var user = UserManager.GetUser(User.Identity.Name);
                    atu.NavigateUrl = $"~/Issues/IssueList.aspx?pid={p.Id}&u={user.UserName}";
                }
                else
                {
                    AssignedUserFilter.Visible = false;
                }
            }
        }
    }
}