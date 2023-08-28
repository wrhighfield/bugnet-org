﻿using BugNET.BLL;
using BugNET.Common;
using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using BugNET.UI;
using BugNET.UI.WebControls;

namespace BugNET
{
    public partial class SiteMaster : MasterPage
    {
        private const string AntiXsrfTokenKey = "__AntiXsrfToken";
        private const string AntiXsrfUserNameKey = "__AntiXsrfUserName";
        private string _antiXsrfTokenValue;

        protected void Page_Init(object sender, EventArgs e)
        {
            // The code below helps to protect against XSRF attacks
            var requestCookie = Request.Cookies[AntiXsrfTokenKey];
            Guid requestCookieGuidValue;
            if (requestCookie != null && Guid.TryParse(requestCookie.Value, out requestCookieGuidValue))
            {
                // Use the Anti-XSRF token from the cookie
                _antiXsrfTokenValue = requestCookie.Value;
                Page.ViewStateUserKey = _antiXsrfTokenValue;
            }
            else
            {
                // Generate a new Anti-XSRF token and save to the cookie
                _antiXsrfTokenValue = Guid.NewGuid().ToString("N");
                Page.ViewStateUserKey = _antiXsrfTokenValue;

                var responseCookie = new HttpCookie(AntiXsrfTokenKey)
                {
                    HttpOnly = true,
                    Value = _antiXsrfTokenValue
                };
                if (FormsAuthentication.RequireSSL && Request.IsSecureConnection) responseCookie.Secure = true;
                Response.Cookies.Set(responseCookie);
            }

            Page.PreLoad += master_Page_PreLoad;
        }

        protected void master_Page_PreLoad(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Set Anti-XSRF token
                ViewState[AntiXsrfTokenKey] = Page.ViewStateUserKey;
                ViewState[AntiXsrfUserNameKey] = Context.User.Identity.Name ?? string.Empty;
            }
            else
            {
                // Validate the Anti-XSRF token
                if ((string) ViewState[AntiXsrfTokenKey] != _antiXsrfTokenValue
                    || (string) ViewState[AntiXsrfUserNameKey] != (Context.User.Identity.Name ?? string.Empty))
                    throw new InvalidOperationException("Validation of Anti-XSRF token failed.");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Convert.ToInt32(HostSettingManager.Get(HostSettingNames.UserRegistration)) ==
                (int) UserRegistration.None)
                if (LoginView1.FindControl("RegisterLink") != null)
                    LoginView1.FindControl("RegisterLink").Visible = false;

            var oHelper = new SuckerFishMenuHelper(ProjectId);
            litMenu.Text = oHelper.GetHtml();

            if (HostSettingManager.Get(HostSettingNames.EnableGravatar, true))
            {
                var user = Membership.GetUser(Security.GetUserName());
                if (user != null && user.Email != null)
                {
                    var img = (Image) LoginView1.FindControl("Avatar");
                    img.ImageUrl = PresentationUtils.GetGravatarImageUrl(user.Email, 32);
                }
            }

            ProjectsList.DataTextField = "Name";
            ProjectsList.DataValueField = "Id";

            if (Page.IsPostBack) return;
            var localizedSelectProject = GetGlobalResourceObject("SharedResources", "SelectProject").ToString();
            switch (Page.User.Identity.IsAuthenticated)
            {
                case true:
                    ProjectsList.DataSource = ProjectManager.GetByMemberUserName(Security.GetUserName(), true);
                    ProjectsList.DataBind();
                    ProjectsList.Items.Insert(0, new ListItem(localizedSelectProject));
                    break;
                case false when bool.Parse(HostSettingManager.Get(HostSettingNames.AnonymousAccess)):
                    ProjectsList.DataSource = ProjectManager.GetPublicProjects();
                    ProjectsList.DataBind();
                    ProjectsList.Items.Insert(0, new ListItem(localizedSelectProject));
                    break;
                default:
                    ProjectsList.Visible = false;
                    break;
            }

            var item = ProjectsList.Items.FindByValue(ProjectId.ToString());

            if (item != null)
                ProjectsList.SelectedValue = item.Value;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void ProjectList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ProjectsList.SelectedIndex != 0)
                Response.Redirect($"~/Projects/ProjectSummary/{ProjectsList.SelectedValue}");
        }

        /// <summary>
        /// Retrieves the project Id from the base page class
        /// </summary>
        public int ProjectId
        {
            get
            {
                try
                {
                    // do the as test to to see if the basepage is the same as page
                    // if not the page parameter will be null and no exception will be thrown
                    var page = Page as BugNetBasePage;

                    if (page != null) return page.ProjectId;
                    return -1;
                }
                catch
                {
                    return -1;
                }
            }
        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            int issueId;
            if (int.TryParse(SearchBox.Text, out issueId))
            {
                if (IssueManager.GetById(issueId) != null)
                    Response.Redirect($"~/Issues/IssueDetail.aspx?id={issueId}");
                else
                    Response.Redirect($"~/Issues/IssueSearch.aspx?q={SearchBox.Text}");
            }
            else
            {
                Response.Redirect($"~/Issues/IssueSearch.aspx?q={SearchBox.Text}");
            }
        }
    }
}