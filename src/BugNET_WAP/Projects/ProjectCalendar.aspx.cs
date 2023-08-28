﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using BugNET.BLL;
using BugNET.Common;
using BugNET.Entities;
using Microsoft.AspNet.FriendlyUrls;

namespace BugNET.Projects
{
    /// <summary>
    /// Page that displays a project calendar
    /// </summary>
    public partial class ProjectCalendar : UserInterfaceLayer.BasePage 
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                try
                {
                    IList<string> segments = Request.GetFriendlyUrlSegments();
                    ProjectId = Int32.Parse(segments[0]);
                }
                catch
                {
                    ProjectId = Request.QueryString.Get("pid", 0);
                }

                BindCalendar();
            }
   
        }

        /// <summary>
        /// Views the selected index changed.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void ViewSelectedIndexChanged(Object s, EventArgs e)
        {
            BindCalendar();
        }

        /// <summary>
        /// Calendars the view selected index changed.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void CalendarViewSelectedIndexChanged(Object s, EventArgs e)
        {
            BindCalendar();
        }


        /// <summary>
        /// Binds the calendar.
        /// </summary>
        private void BindCalendar()
        {
            Project p = ProjectManager.GetById(ProjectId);
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
        protected void prjCalendar_DayRender(object sender, System.Web.UI.WebControls.DayRenderEventArgs e)
        {
            if (e.Day.IsToday)
            {
                //TODO: If issues are due today in 7 days or less then create as red, else use blue?
            }
           
           
            List<QueryClause> queryClauses = new List<QueryClause>();
            switch (dropView.SelectedValue)
            {
                case "IssueDueDates":
                    QueryClause q = new QueryClause("AND", "iv.[IssueDueDate]", "=", e.Day.Date.ToShortDateString(), SqlDbType.DateTime);
                    queryClauses.Add(q);

                    q = new QueryClause("AND", "iv.[Disabled]", "=", "false", SqlDbType.Bit);
                    queryClauses.Add(q);

                    List<Issue> issues = IssueManager.PerformQuery(queryClauses, ProjectId);
                    foreach (Issue issue in issues)
                    {
                        if (issue.Visibility == (int)IssueVisibility.Private && issue.AssignedDisplayName != Security.GetUserName() && issue.CreatorDisplayName != Security.GetUserName() && (!UserManager.IsSuperUser() || !UserManager.IsInRole(issue.ProjectId, Globals.ProjectAdministratorRole)))
                            continue;

                        string cssClass = string.Empty;

                        if (issue.DueDate <= DateTime.Today)
                            cssClass = "calIssuePastDue";   
                        else
                            cssClass = "calIssue";

                        if (issue.Visibility == (int)IssueVisibility.Private)
                            cssClass += " calIssuePrivate";

                        string title = string.Format(@"<div id=""issue"" class=""{3}""><a href=""{4}{2}"">{0} - {1}</a></div>", issue.FullId.ToUpper(), issue.Title, issue.Id,cssClass, Page.ResolveUrl("~/Issues/IssueDetail.aspx?id="));
                        e.Cell.Controls.Add(new LiteralControl(title));
                    }
                    break;
                case "MilestoneDueDates":
                    List<Milestone> milestones = MilestoneManager.GetByProjectId(ProjectId).FindAll(m => m.DueDate == e.Day.Date);
                    foreach (Milestone m in milestones)
                    {
                        string cssClass = string.Empty;

                        if (m.DueDate <= DateTime.Today)
                            cssClass = "calIssuePastDue";
                        else
                            cssClass = "calIssue";

                        string projectName = ProjectManager.GetById(ProjectId).Name;
                        string title = string.Format(@"<div id=""issue"" class=""{4}""><a href=""{6}{2}&m={3}"">{1} - {0} </a><br/>{5}</div>", m.Name, projectName, m.ProjectId, m.Id, cssClass, m.Notes, Page.ResolveUrl("~/Issues/IssueDetail.aspx?pid="));
                        e.Cell.Controls.Add(new LiteralControl(title));
                    }
                    break;

            }
           

            //Set the calendar to week mode only showing the selected week.
            if (dropCalendarView.SelectedValue == "Week")
            {
                if (Week(e.Day.Date) != Week(prjCalendar.VisibleDate))
                { 
                    e.Cell.Visible = false;
                }
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
            if (JumpToDate.SelectedValue.HasValue)
            {
                prjCalendar.VisibleDate = JumpToDate.SelectedValue.GetValueOrDefault();
                prjCalendar.SelectedDate = JumpToDate.SelectedValue.GetValueOrDefault();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnNext_Click(object sender, EventArgs e)
        {
            if (dropCalendarView.SelectedValue == "Week")
            {
                prjCalendar.VisibleDate = prjCalendar.VisibleDate.AddDays(7);           
            }
            else
            {
                prjCalendar.VisibleDate = prjCalendar.VisibleDate.AddMonths(1);
            }

        }

        /// <summary>
        /// Handles the Click event of the btnPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnPrevious_Click(object sender, EventArgs e)
        {
            if (dropCalendarView.SelectedValue == "Week")
            {
                prjCalendar.VisibleDate = prjCalendar.VisibleDate.AddDays(-7);
            }
            else
            {
                prjCalendar.VisibleDate = prjCalendar.VisibleDate.AddMonths(-1);
            }

        }
      
        /// <summary>
        /// Weeks the specified td date.
        /// </summary>
        /// <param name="tdDate">The td date.</param>
        /// <returns></returns>
        private static int Week(DateTime tdDate)
        {
            CultureInfo ci = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Globalization.Calendar Cal = ci.Calendar;
            System.Globalization.CalendarWeekRule CWR = ci.DateTimeFormat.CalendarWeekRule;
            DayOfWeek FirstDOW = ci.DateTimeFormat.FirstDayOfWeek;
            return Cal.GetWeekOfYear(tdDate, CWR, FirstDOW);
        }
    }
}
