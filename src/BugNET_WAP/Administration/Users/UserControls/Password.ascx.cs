using System;
using System.Web;
using System.Web.UI;
using BugNET.BLL;
using BugNET.Common;
using BugNET.Providers.MembershipProviders;
using BugNET.UI;
using log4net;

namespace BugNET.Administration.Users.UserControls
{
    public partial class Password : BaseUserControlUserAdmin, IEditUserControl
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public event ActionEventHandler Action;

        /// <summary>
        /// Raises the <see cref="E:Action" /> event.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs"/> instance containing the event data.</param>
        private void OnAction(ActionEventArgs args)
        {
            if (Action != null)
                Action(this, args);
        }

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>
        /// The user id.
        /// </value>
        public Guid UserId
        {
            get => ViewState.Get("UserId", Guid.Empty);
            set => ViewState.Set("UserId", value);
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            GetMembershipData(UserId);
            DataBind();
        }

        /// <summary>
        /// Binds a data source to the invoked server control and all its child controls.
        /// </summary>
        public override void DataBind()
        {
            base.DataBind();

            if (UserId == Guid.Empty) return;

            //get this user and bind the data
            var user = (CustomMembershipUser) MembershipData;

            if (user == null) return;

            PasswordLastChanged.Text = string.Concat(user.LastPasswordChangedDate.ToString("g"), "&nbsp;");
        }

        /// <summary>
        /// Handles the Click event of the cmdChangePassword control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        protected void CmdChangePasswordClick(object sender, EventArgs e)
        {
            if (!cvPasswords.IsValid) return;

            GetMembershipData(UserId);

            if (MembershipData == null) return;

            try
            {
                MembershipData.ChangePassword(MembershipData.ResetPassword(), NewPassword.Text);
                ActionMessage.ShowSuccessMessage(GetLocalString("PasswordChangeSuccess"));
                GetMembershipData(UserId);
                DataBind();
            }
            catch (Exception ex)
            {
                if (Log.IsErrorEnabled)
                {
                    if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
                        MDC.Set("user", HttpContext.Current.User.Identity.Name);

                    Log.Error(LoggingManager.GetErrorMessageResource("PasswordChangeError"), ex);
                }

                ActionMessage.ShowErrorMessage(LoggingManager.GetErrorMessageResource("PasswordChangeError"));
            }
        }

        protected void CmdCancelClick(object sender, ImageClickEventArgs e)
        {
            Response.Redirect("~/Administration/Users/UserList.aspx");
        }
    }
}