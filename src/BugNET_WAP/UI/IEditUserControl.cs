using System;
using BugNET.Common;

namespace BugNET.UI
{
    public interface IEditUserControl
    {
        event ActionEventHandler Action;

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>The user id.</value>
        Guid UserId { get; set; }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        void Initialize();

        bool Visible { get; set; }
    }
}