using System;
using System.Web.UI;
using BugNET.BLL;
using BugNET.Common;
using BugNET.Providers.MembershipProviders;
using BugNET.UI;
using log4net;

namespace BugNET.Administration.Users.UserControls
{
    public partial class Membership : BaseUserControlUserAdmin, IEditUserControl
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public event ActionEventHandler Action;

        private void OnAction(ActionEventArgs args)
        {
            if (Action != null)
                Action(this, args);
        }

        public Guid UserId
        {
            get => ViewState.Get("UserId", Guid.Empty);
            set => ViewState.Set("UserId", value);
        }

        public void Initialize()
        {
            BindData();
        }

        /// <summary>
        /// Binds the data.
        /// </summary>
        private void BindData()
        {
            if (UserId == Guid.Empty) return;

            GetMembershipData(UserId);

            //get this user and bind the data
            var user = (CustomMembershipUser) MembershipData;

            if (user == null) return;

            // this is to fix the UI if no data is present
            // stops the ol / li from shifting to the right under IE 
            CreatedDate.Text = string.Concat(user.CreationDate.ToString("g"), "&nbsp;");
            LastActivityDate.Text = string.Concat(user.LastActivityDate.ToString("g"), "&nbsp;");
            LastLoginDate.Text = string.Concat(user.LastLoginDate.ToString("g"), "&nbsp;");

            UserName.Text = user.UserName;
            Email.Text = user.Email;
            LockedOut.Checked = user.IsLockedOut;
            Authorized.Checked = user.IsApproved;
            Online.Checked = user.IsOnline;
            FirstName.Text = user.FirstName;
            LastName.Text = user.LastName;
            DisplayName.Text = user.DisplayName;
            cmdAuthorize.Visible = !user.IsApproved;
            cmdUnAuthorize.Visible = user.IsApproved;
            cmdUnLock.Visible = user.IsLockedOut;
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            ctlUserCustomFields.DataSource = UserCustomFieldManager.GetByUserId(UserId);
            ctlUserCustomFields.DataBind();
        }

        /// <summary>
        /// Handles the Click event of the cmdUpdate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void CmdUpdateClick(object sender, EventArgs e)
        {
            try
            {
                GetMembershipData(UserId);

                var user = (CustomMembershipUser) MembershipData;

                if (user != null)
                {
                    //user.IsApproved = Authorized.Checked;
                    user.Email = Email.Text;
                    user.DisplayName = DisplayName.Text;
                    user.FirstName = FirstName.Text;
                    user.LastName = LastName.Text;
                    UserManager.UpdateUser(user);
                    OnAction(new ActionEventArgs {Trigger = ActionTriggers.Save});

                    UserCustomFieldManager.SaveCustomFieldValues(UserId, ctlUserCustomFields.Values);
                }

                ActionMessage.ShowSuccessMessage(GetLocalString("UpdateUserMessage"));
            }
            catch
            {
                ActionMessage.ShowErrorMessage(LoggingManager.GetErrorMessageResource("UpdateUserError"));
            }
        }

        /// <summary>
        /// Handles the Click event of the AuthorizeUser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void AuthorizeUserClick(object sender, EventArgs e)
        {
            try
            {
                AuthorizeUser(true);
                ActionMessage.ShowSuccessMessage(GetLocalString("UserAuthorizedMessage"));
            }
            catch
            {
                ActionMessage.ShowErrorMessage(GetLocalString("UserAuthorizedError"));
            }
        }

        /// <summary>
        /// Handles the Click event of the UnLockUser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void UnLockUserClick(object sender, EventArgs e)
        {
            GetMembershipData(UserId);

            if (MembershipData != null)
                try
                {
                    MembershipData.UnlockUser();
                    ActionMessage.ShowSuccessMessage(GetLocalString("UpdateUserMessage"));
                    BindData();
                }
                catch
                {
                    ActionMessage.ShowErrorMessage(LoggingManager.GetErrorMessageResource("UpdateUserError"));
                }
        }

        /// <summary>
        /// Handles the Click event of the UnAuthorizeUser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void UnAuthorizeUserClick(object sender, EventArgs e)
        {
            try
            {
                AuthorizeUser(false);
                ActionMessage.ShowSuccessMessage(GetLocalString("UserUnAuthorizedMessage"));
            }
            catch
            {
                ActionMessage.ShowErrorMessage(GetLocalString("UserUnAuthorizedError"));
            }
        }


        /// <summary>
        /// Authorizes the user.
        /// </summary>
        /// <param name="isAuthorized">if set to <c>true</c> [is authorized].</param>
        private void AuthorizeUser(bool isAuthorized)
        {
            GetMembershipData(UserId);

            if (MembershipData == null) return;

            MembershipData.IsApproved = isAuthorized;
            Authorized.Checked = isAuthorized;
            UserManager.UpdateUser(MembershipData);

            cmdAuthorize.Visible = !isAuthorized;
            cmdUnAuthorize.Visible = isAuthorized;
        }

        protected void CmdCancelClick(object sender, ImageClickEventArgs e)
        {
            Response.Redirect("~/Administration/Users/UserList.aspx");
        }
    }
}