using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using BugNET.BLL;
using BugNET.Common;
using BugNET.UI;

namespace BugNET.Administration.Host
{
    /// <summary>
    /// Administration page that controls the application configuration
    /// </summary>
    public partial class Settings : BugNetBasePage
    {
        /// <summary>
        /// Message1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        public BugNET.UserControls.Message Message1;

        private Control _ctlHostSettings;
        private readonly Dictionary<string, string> _menuItems = new Dictionary<string, string>();

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UserManager.IsSuperUser())
                Response.Redirect("~/Errors/AccessDenied.aspx");

            _menuItems.Add(GetLocalString("Basic"), "page_white_gear.png");
            _menuItems.Add(GetLocalString("Authentication"), "lock.gif");
            _menuItems.Add(GetLocalString("Mail"), "email.gif");
            _menuItems.Add(GetLocalString("Logging"), "page_white_error.png");
            _menuItems.Add(GetLocalString("Subversion"), "svnLogo_sm.jpg");
            _menuItems.Add(GetLocalString("Notifications"), "email_go.gif");
            _menuItems.Add(GetLocalString("Attachments"), "attach.gif");
            _menuItems.Add(GetLocalString("POP3Mailbox"), "mailbox.png");
            _menuItems.Add(GetLocalString("Languages"), "page_white_world.png");
            _menuItems.Add(GetLocalString("UserCustomFields"), "user_edit.gif");

            AdminMenu.DataSource = _menuItems;
            AdminMenu.DataBind();

            if (!IsPostBack)
            {
                var tabIdStr = Request.QueryString["tid"];
                if (!string.IsNullOrEmpty(tabIdStr))
                {
                    var result = 0;
                    var flag = int.TryParse(tabIdStr, out result);
                    if (flag && result >= 0 && result <= 8)
                        TabId = result;
                }
            }

            if (TabId != -1)
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

            //if (listItem != null)
            //	listItem.Attributes.Add("style", string.Format("background: #C4EFA1 url(../../images/{0}) no-repeat 5px 4px;", dataItem.Value));

            if (lb != null)
                lb.Text = dataItem.Key;
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
        /// Loads the tab.
        /// </summary>
        /// <param name="selectedTab">The selected tab.</param>
        private void LoadTab(int selectedTab)
        {
            var controlName = "BasicSettings.ascx";

            switch (selectedTab)
            {
                case 0:
                    controlName = "BasicSettings.ascx";
                    break;
                case 1:
                    controlName = "AuthenticationSettings.ascx";
                    break;
                case 2:
                    controlName = "MailSettings.ascx";
                    break;
                case 3:
                    controlName = "LoggingSettings.ascx";
                    break;
                case 4:
                    controlName = "SubversionSettings.ascx";
                    break;
                case 5:
                    controlName = "NotificationSettings.ascx";
                    break;
                case 6:
                    controlName = "AttachmentSettings.ascx";
                    break;
                case 7:
                    controlName = "POP3Settings.ascx";
                    break;
                case 8:
                    controlName = "LanguageSettings.ascx";
                    break;
                case 9:
                    controlName = "UserCustomFieldsSettings.ascx";
                    break;
            }

            for (var i = 0; i < _menuItems.Count; i++)
                ((HtmlGenericControl) AdminMenu.Items[i].FindControl("ListItem")).Attributes.Add("class",
                    i == TabId ? "active" : "");


            _ctlHostSettings = Page.LoadControl("~/Administration/Host/UserControls/" + controlName);
            _ctlHostSettings.ID = "ctlHostSetting";
            plhSettingsControl.Controls.Clear();
            plhSettingsControl.Controls.Add(_ctlHostSettings);

            var editHostSettingsControl = (IEditHostSettingControl) _ctlHostSettings;
            editHostSettingsControl.Initialize();
            cmdUpdate.Visible = editHostSettingsControl.ShowSaveButton;
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
        /// Handles the Click event of the cmdUpdate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        protected void cmdUpdate_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            var editHostSettingControl = (IEditHostSettingControl) _ctlHostSettings;

            if (!editHostSettingControl.Update()) return;
            if (Message1.Text.Trim().Length.Equals(0)) Message1.ShowSuccessMessage(GetLocalString("SaveMessage"));
        }
    }
}