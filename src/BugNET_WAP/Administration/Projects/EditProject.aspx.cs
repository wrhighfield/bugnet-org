using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.AspNet.FriendlyUrls;
using BugNET.BLL;
using BugNET.Common;
using BugNET.UI;

namespace BugNET.Administration.Projects
{
    /// <summary>
    /// Edit project administration page.
    /// </summary>
    public partial class EditProject : BugNetBasePage
    {
        private Control _contentControl;
        private readonly Dictionary<string, string> _menuItems = new Dictionary<string, string>();

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
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

            if (!UserManager.IsSuperUser())
                if (!UserManager.HasPermission(ProjectId, Permission.AdminEditProject.ToString()))
                    Response.Redirect("~/Errors/AccessDenied");

            if (!Page.IsPostBack)
            {
                litProjectName.Text = ProjectManager.GetById(ProjectId).Name;
                lblExistingProjectName.Text = litProjectName.Text;

                var message = string.Format(GetLocalString("ConfirmDelete"), litProjectName.Text);
                DeleteButton.OnClientClick = $"return confirm('{message}');";

                if (!UserManager.HasPermission(ProjectId, Permission.AdminDeleteProject.ToString()))
                    DeleteButton.Visible = false;

                if (!UserManager.HasPermission(ProjectId, Permission.AdminCloneProject.ToString()))
                    linkCloneProject.Visible = false;
            }

            _menuItems.Add(GetLocalString("Details"), "application_home.png");
            _menuItems.Add(GetLocalString("Categories"), "plugin.gif");
            _menuItems.Add(GetLocalString("Status"), "greencircle.png");
            _menuItems.Add(GetLocalString("Priorities"), "Critical.gif");
            _menuItems.Add(GetLocalString("Milestones"), "package.gif");
            _menuItems.Add(GetLocalString("IssueTypes"), "bug.gif");
            _menuItems.Add(GetLocalString("Resolutions"), "accept.gif");
            _menuItems.Add(GetLocalString("Members"), "users_group.png");
            _menuItems.Add(GetLocalString("SecurityRoles"), "shield.gif");
            _menuItems.Add(GetLocalString("Notifications"), "email_go.gif");
            _menuItems.Add(GetLocalString("CustomFields"), "textfield.gif");
            _menuItems.Add(GetLocalString("Mailboxes"), "email.gif");
            _menuItems.Add(GetLocalString("Subversion"), "svnLogo_sm.jpg");
            _menuItems.Add(GetLocalString("Defaults"), "Default.png");

            AdminMenu.DataSource = _menuItems;
            AdminMenu.DataBind();

            if (TabId != -1)
                LoadTab(TabId);
        }

        /// <summary>
        /// Changes the Enabled/Disabled Icon
        /// </summary>
        /// <param name="disabled"></param>
        private void ProjectDisableEnable(bool disabled)
        {
            if (disabled)
            {
                DisableButton.Visible = false;
                RestoreButton.Visible = true;
            }
            else
            {
                DisableButton.Visible = true;
                RestoreButton.Visible = false;
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the AdminMenu control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void AdminMenu_ItemCommand(object sender, RepeaterCommandEventArgs e)
        {
            TabId = e.Item.ItemIndex;
            LoadTab(TabId);
        }


        /// <summary>
        /// Handles the ItemDataBound event of the AdminMenu control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void AdminMenu_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var dataItem = (KeyValuePair<string, string>) e.Item.DataItem;
            var listItem = e.Item.FindControl("ListItem") as HtmlGenericControl;
            var lb = e.Item.FindControl("MenuButton") as LinkButton;
            lb.Controls.Add(new LiteralControl("<i class='glyphicon glyphicon-list'></i>"));
            if (lb != null) lb.Text = dataItem.Key;
        }

        /// <summary>
        /// Gets or sets the tab id.
        /// </summary>
        /// <value>The tab id.</value>
        private int TabId
        {
            get => ViewState.Get("TabId", 0);
            set => ViewState.Set("TabId", value);
        }

        /// <summary>
        /// Loads the tab.
        /// </summary>
        /// <param name="selectedTab">The selected tab.</param>
        private void LoadTab(int selectedTab)
        {
            var controlName = "ProjectDescription.ascx";

            switch (selectedTab)
            {
                case 0:
                    controlName = "ProjectDescription.ascx";
                    break;
                case 1:
                    controlName = "ProjectCategories.ascx";
                    break;
                case 2:
                    controlName = "ProjectStatus.ascx";
                    break;
                case 3:
                    controlName = "ProjectPriorities.ascx";
                    break;
                case 4:
                    controlName = "ProjectMilestones.ascx";
                    break;
                case 5:
                    controlName = "ProjectIssueTypes.ascx";
                    break;
                case 6:
                    controlName = "ProjectResolutions.ascx";
                    break;
                case 7:
                    controlName = "ProjectMembers.ascx";
                    break;
                case 8:
                    controlName = "ProjectRoles.ascx";
                    break;
                case 9:
                    controlName = "ProjectNotifications.ascx";
                    break;
                case 10:
                    controlName = "ProjectCustomFields.ascx";
                    break;
                case 11:
                    controlName = "ProjectMailbox.ascx";
                    break;
                case 12:
                    controlName = "ProjectSubversion.ascx";
                    break;
                case 13:
                    controlName = "ProjectDefaultValues.ascx";
                    break;
            }

            for (var i = 0; i < _menuItems.Count; i++)
                if (i == TabId)
                    ((HtmlGenericControl) AdminMenu.Items[i].FindControl("ListItem")).Attributes.Add("class", "active");
                else
                    ((HtmlGenericControl) AdminMenu.Items[i].FindControl("ListItem")).Attributes.Add("class",
                        string.Empty);

            _contentControl = Page.LoadControl("~/Administration/Projects/UserControls/" + controlName);
            ((IEditProjectControl) _contentControl).ProjectId = ProjectId;
            plhContent.Controls.Clear();
            plhContent.Controls.Add(_contentControl);
            _contentControl.ID = "ctlContent";

            SaveButton.Visible = ((IEditProjectControl) _contentControl).ShowSaveButton;


            ((IEditProjectControl) _contentControl).Initialize();
            plhContent.Visible = true;

            if (selectedTab != 0)
            {
                DeleteButton.Visible = false;
                linkCloneProject.Visible = false;
                RestoreButton.Visible = false;
                DisableButton.Visible = false;
            }
            else
            {
                DeleteButton.Visible = true;
                linkCloneProject.Visible = true;
                var p = ProjectManager.GetById(ProjectId);
                ProjectDisableEnable(p.Disabled);
            }
        }

        /// <summary>
        /// Handles the Click event of the DeleteButton control.
        /// </summary>
        /// <param name="s">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        protected void DisableButton_Click(object s, EventArgs e)
        {
            var p = ProjectManager.GetById(ProjectId);
            p.Disabled = true;
            ProjectManager.SaveOrUpdate(p);

            ProjectDisableEnable(true);
        }

        /// <summary>
        /// Handles the Click event of the DeleteButton control.
        /// </summary>
        /// <param name="s">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        protected void DeleteButton_Click(object s, EventArgs e)
        {
            ProjectManager.Delete(ProjectId);
            Response.Redirect("~/Administration/Projects/ProjectList.aspx");
        }

        /// <summary>
        /// Handles the Click event of the RestoreButton control.
        /// </summary>
        /// <param name="s">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void RestoreButton_Click(object s, EventArgs e)
        {
            var p = ProjectManager.GetById(ProjectId);
            p.Disabled = false;
            ProjectManager.SaveOrUpdate(p);

            ProjectDisableEnable(false);
        }

        /// <summary>
        /// Handles the Click event of the SaveButton control.
        /// </summary>
        /// <param name="s">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void SaveButton_Click(object s, EventArgs e)
        {
            var c = plhContent.FindControl("ctlContent");
            if (c == null) return;

            if (((IEditProjectControl) c).Update())
                Message1.ShowInfoMessage(GetLocalString("ProjectUpdated"));
        }

        protected void OkButton_Click(object sender, EventArgs e)
        {
            if (!IsValid) return;

            var newProjectId = ProjectManager.CloneProject(ProjectId, txtNewProjectName.Text);

            if (newProjectId > 0)
                Response.Redirect(FriendlyUrl.Href("~/Administration/Projects/EditProject", newProjectId));
            else
                lblError.Text = LoggingManager.GetErrorMessageResource("CloneProjectError");
        }
    }
}