using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.AspNet.FriendlyUrls;
using BugNET.BLL;
using BugNET.Common;
using BugNET.UI;

namespace BugNET.Administration.Users
{
    public partial class EditUser : BugNetBasePage
    {
        private static readonly List<AdminMenuItem> MenuItems = new List<AdminMenuItem>();

        /// <summary>
        /// Gets or sets the admin menu id.
        /// </summary>
        /// <value>The admin menu id.</value>
        private int AdminMenuId
        {
            get => ViewState.Get("AdminMenuId", 0);
            set => ViewState.Set("AdminMenuId", value);
        }

        /// <summary>
        /// Gets the tab id.
        /// </summary>
        /// <value>The tab id.</value>
        private int QueryTabId
        {
            get
            {
                var segments = Request.GetFriendlyUrlSegments();
                var queryTabId = 0;
                if (segments.Count > 1) int.TryParse(segments[1], out queryTabId);
                return queryTabId;
            }
        }

        /// <summary>
        /// Gets the user id.
        /// </summary>
        /// <value>The user id.</value>
        private Guid UserId
        {
            get
            {
                var segments = Request.GetFriendlyUrlSegments();
                var userId = segments[0];
                // var userId = Request.QueryString.Get("user", "");

                if (!userId.Equals(""))
                {
                    Guid userGuid;

                    if (Guid.TryParse(userId, out userGuid))
                        return userGuid;

                    throw new Exception(LoggingManager.GetErrorMessageResource("QueryStringError"));
                }

                return Guid.Empty;
            }
        }

        /// <summary>
        /// Loads the admin control.
        /// </summary>
        /// <param name="selectedMenuItem">The selected menu item id.</param>
        /// <param name="loadControl">Flag to indicate if the control should be loaded or not</param>
        private void DisplayAdminControl(int selectedMenuItem, bool loadControl = true)
        {
            AdminMenuId = selectedMenuItem;

            foreach (var adminMenuItem in MenuItems)
            {
                var control = pnlAdminControls.FindControl(adminMenuItem.Argument) as IEditUserControl;

                if (control == null) continue;

                control.Action += EditUserAction;

                if (!loadControl) continue;

                control.Visible = false;
                var htmlControl = AdminMenu.Items[adminMenuItem.Id].FindControl("ListItem") as HtmlGenericControl;

                if (htmlControl != null)
                    htmlControl.Attributes.Add("class", "");

                if (selectedMenuItem != adminMenuItem.Id) continue;

                if (htmlControl != null)
                    htmlControl.Attributes.Add("class", "active");

                control.Visible = true;
                control.UserId = UserId;
                control.Initialize();
            }
        }

        /// <summary>
        /// Events raised from the user specific user controls to the parent
        /// </summary>
        /// <param name="sender">The object sending the event</param>
        /// <param name="args">Arguments sent from the parent</param>
        private void EditUserAction(object sender, ActionEventArgs args)
        {
            switch (args.Trigger)
            {
                case ActionTriggers.Save:
                    if (UserId != Guid.Empty)
                    {
                        var user = UserManager.GetUser(UserId);
                        litUserTitleName.Text = UserManager.GetUserDisplayName(user.UserName);
                    }

                    break;
            }
        }

        private void LoadAdminMenuItems()
        {
            MenuItems.Clear();

            MenuItems.Add(new AdminMenuItem
                {Id = 0, Text = GetLocalString("UserDetails"), Argument = "UserDetails", ImageUrl = "vcard.gif"});
            MenuItems.Add(new AdminMenuItem
                {Id = 1, Text = GetLocalString("UserRoles"), Argument = "UserRoles", ImageUrl = "shield.gif"});
            MenuItems.Add(new AdminMenuItem
                {Id = 2, Text = GetLocalString("UserPassword"), Argument = "UserPassword", ImageUrl = "key.gif"});
            MenuItems.Add(new AdminMenuItem
                {Id = 3, Text = GetLocalString("UserProfile"), Argument = "UserProfile", ImageUrl = "user.gif"});
            MenuItems.Add(new AdminMenuItem
                {Id = 4, Text = GetLocalString("UserDelete"), Argument = "UserDelete", ImageUrl = "user_delete.gif"});

            AdminMenu.DataSource = MenuItems;
            AdminMenu.DataBind();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UserManager.HasPermission(ProjectId, Permission.AdminEditProject.ToString()))
                Response.Redirect("~/Errors/AccessDenied.aspx");

            if (UserId != Guid.Empty)
            {
                var user = UserManager.GetUser(UserId);
                litUserTitleName.Text = UserManager.GetUserDisplayName(user.UserName);
            }

            if (!Page.IsPostBack)
            {
                LoadAdminMenuItems();
                DisplayAdminControl(QueryTabId);
                return;
            }

            DisplayAdminControl(AdminMenuId, false);
        }

        protected void AdminMenuItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var menuItem = e.Item.DataItem as AdminMenuItem;

            var listItem = e.Item.FindControl("ListItem") as HtmlGenericControl;
            var menuButton = e.Item.FindControl("MenuButton") as LinkButton;

            if (menuButton != null)
            {
                menuButton.Text = menuItem.Text;
                menuButton.Attributes.Add("data-menu-id", menuItem.Id.ToString());
            }
        }

        protected void AdminMenuItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var menuButton = e.Item.FindControl("MenuButton") as LinkButton;

            if (menuButton != null) DisplayAdminControl(menuButton.Attributes["data-menu-id"].To<int>());
        }
    }
}