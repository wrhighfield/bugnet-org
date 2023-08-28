using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using BugNET.BLL;
using BugNET.Common;
using BugNET.Entities;
using BugNET.UI;
using Microsoft.AspNet.FriendlyUrls;

namespace BugNET.Projects
{
    /// <summary>
    /// Page that displays a project calendar
    /// </summary>
    public partial class ProjectCalendar : BugNetBasePage
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
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

            BindCalendar();
        }

        /// <summary>
        /// Views the selected index changed.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void ViewSelectedIndexChanged(object s, EventArgs e)
        {
            BindCalendar();
        }

        /// <summary>
        /// Calendars the view selected index changed.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void CalendarViewSelectedIndexChanged(object s, EventArgs e)
        {
            BindCalendar();
        }


        /// <summary>
        /// Binds the calendar.
        /// </summary>
        private void BindCalendar()
        {
            var p = ProjectManager.GetById(ProjectId);
            ltProject.Text = p.Name;
            litProjectCode.Text = p.Code;
            prjCalendar.SelectedDate = DateTime.Today;
            prjCalendar.VisibleDate = DateTime.Today;
        }

        /// <summary>
        /// Handles the DayRender event of the prjCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.DayRenderEventArgs"/> instance containing the event data.</param>
        protected void prjCalendar_DayRender(object sender, DayRenderEventArgs e)
        {
            if (e.Day.IsToday)
            {
                //TODO: If issues are due today in 7 days or less then create as red, else use blue?
            }


            var queryClauses = new List<QueryClause>();
            switch (dropView.SelectedValue)
            {
                case "IssueDueDates":
                    var q = new QueryClause("AND", "iv.[IssueDueDate]", "=", e.Day.Date.ToShortDateString(),
                        SqlDbType.DateTime);
                    queryClauses.Add(q);

                    q = new QueryClause("AND", "iv.[Disabled]", "=", "false", SqlDbType.Bit);
                    queryClauses.Add(q);

                    var issues = IssueManager.PerformQuery(queryClauses, ProjectId);
                    foreach (var issue in issues)
                    {
                        if (issue.Visibility == (int) IssueVisibility.Private &&
                            issue.AssignedDisplayName != Security.GetUserName() &&
                            issue.CreatorDisplayName != Security.GetUserName() && (!UserManager.IsSuperUser() ||
                                !UserManager.IsInRole(issue.ProjectId, Globals.ProjectAdministratorRole)))
                            continue;

                        var cssClass = issue.DueDate <= DateTime.Today ? "calIssuePastDue" : "calIssue";

                        if (issue.Visibility == (int) IssueVisibility.Private)
                            cssClass += " calIssuePrivate";

                        var title = string.Format(
                            @"<div id=""issue"" class=""{3}""><a href=""{4}{2}"">{0} - {1}</a></div>",
                            issue.FullId.ToUpper(), issue.Title, issue.Id, cssClass,
                            Page.ResolveUrl("~/Issues/IssueDetail.aspx?id="));
                        e.Cell.Controls.Add(new LiteralControl(title));
                    }

                    break;
                case "MilestoneDueDates":
                    var milestones = MilestoneManager.GetByProjectId(ProjectId).FindAll(m => m.DueDate == e.Day.Date);
                    foreach (
                        var title in from m in milestones
                        let cssClass = m.DueDate <= DateTime.Today ? "calIssuePastDue" : "calIssue"
                        let projectName = ProjectManager.GetById(ProjectId).Name
                        select string.Format(
                            @"<div id=""issue"" class=""{4}""><a href=""{6}{2}&m={3}"">{1} - {0} </a><br/>{5}</div>",
                            m.Name, projectName, m.ProjectId, m.Id, cssClass, m.Notes,
                            Page.ResolveUrl("~/Issues/IssueDetail.aspx?pid=")))
                        e.Cell.Controls.Add(new LiteralControl(title));
                    break;
            }


            //Set the calendar to week mode only showing the selected week.
            if (dropCalendarView.SelectedValue == "Week")
            {
                if (Week(e.Day.Date) != Week(prjCalendar.VisibleDate)) e.Cell.Visible = false;
                e.Cell.Height = new Unit("300px");
            }
            else
            {
                //e.Cell.Height = new Unit("80px");
                //e.Cell.Width = new Unit("80px");
            }
        }


        /// <summary>
        /// Handles the PreRender event of the prjCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void prjCalendar_PreRender(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Handles the Click event of the JumpButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void JumpButton_Click(object sender, EventArgs e)
        {
            if (!JumpToDate.SelectedValue.HasValue) return;
            prjCalendar.VisibleDate = JumpToDate.SelectedValue.GetValueOrDefault();
            prjCalendar.SelectedDate = JumpToDate.SelectedValue.GetValueOrDefault();
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnNext_Click(object sender, EventArgs e)
        {
            prjCalendar.VisibleDate = dropCalendarView.SelectedValue == "Week"
                ? prjCalendar.VisibleDate.AddDays(7)
                : prjCalendar.VisibleDate.AddMonths(1);
        }

        /// <summary>
        /// Handles the Click event of the btnPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnPrevious_Click(object sender, EventArgs e)
        {
            prjCalendar.VisibleDate = dropCalendarView.SelectedValue == "Week"
                ? prjCalendar.VisibleDate.AddDays(-7)
                : prjCalendar.VisibleDate.AddMonths(-1);
        }

        /// <summary>
        /// Weeks the specified td date.
        /// </summary>
        /// <param name="tdDate">The td date.</param>
        /// <returns></returns>
        private static int Week(DateTime tdDate)
        {
            var ci = System.Threading.Thread.CurrentThread.CurrentCulture;
            var cal = ci.Calendar;
            var cwr = ci.DateTimeFormat.CalendarWeekRule;
            var firstDow = ci.DateTimeFormat.FirstDayOfWeek;
            return cal.GetWeekOfYear(tdDate, cwr, firstDow);
        }
    }
}