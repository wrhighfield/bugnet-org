using System.Web.UI;
using System.Web.UI.WebControls;

namespace BugNET.UI.WebControls
{
    /// <summary>
    /// Summary description for ButtonDropDownList
    /// </summary>
    public class ButtonDropDownList : DropDownList, IPostBackEventHandler
    {
        private static readonly object EventCommand = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="ButtonDropDownList"/> class.
        /// </summary>
        public ButtonDropDownList()
        {
            base.AutoPostBack = true;
        }

        /// <summary>
        /// Gets or sets the command argument.
        /// </summary>
        /// <value>The command argument.</value>
        public string CommandArgument
        {
            get
            {
                var str = (string) ViewState["CommandArgument"];
                return str ?? string.Empty;
            }
            set => ViewState["CommandArgument"] = value;
        }

        /// <summary>
        /// Gets or sets the name of the command.
        /// </summary>
        /// <value>The name of the command.</value>
        public string CommandName
        {
            get
            {
                var str = (string) ViewState["CommandName"];
                return str ?? string.Empty;
            }
            set => ViewState["CommandName"] = value;
        }

        #region IPostBackEventHandler implementation

        /// <summary>
        /// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String"/> that represents an optional event argument to be passed to the event handler.</param>
        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            CommandArgument = "0";

            if (base.SelectedItem != null)
                CommandArgument = SelectedItem.Value;

            RaisePostBackEvent();
        }

        #endregion

        /// <summary>
        /// Raises the <see cref="E:Command"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.CommandEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCommand(CommandEventArgs e)
        {
            var handler = (CommandEventHandler) Events[EventCommand];
            handler?.Invoke(this, e);
            //It bubbles the event to the HandleEvent method of the GooglePagerField class.
            RaiseBubbleEvent(this, e);
        }

        /// <summary>
        /// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        protected virtual void RaisePostBackEvent()
        {
            if (CausesValidation) Page.Validate(ValidationGroup);
            OnCommand(new CommandEventArgs(CommandName, CommandArgument));
        }
    }
}