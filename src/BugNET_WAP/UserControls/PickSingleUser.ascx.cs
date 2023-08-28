using BugNET.UI;

namespace BugNET.UserControls
{
    using System.Collections.Generic;
    using System.Web.UI.WebControls;
    using Entities;

    /// <summary>
    ///	User control to pick application users.
    /// </summary>
    public partial class PickSingleUser : BugNetUserControl
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, System.EventArgs e)
        {
            // Put user code to initialize the page here
        }

        /// <summary>
        /// Gets or sets a value indicating whether [display default].
        /// </summary>
        /// <value><c>true</c> if [display default]; otherwise, <c>false</c>.</value>
        public bool DisplayDefault { get; set; } = false;

        /// <summary>
        /// Gets or sets the field of the data source that provides the text content of the list items.
        /// </summary>
        public string DataTextField
        {
            get => ddlUsers.DataTextField;
            set => ddlUsers.DataTextField = value;
        }

        /// <summary>
        /// Gets or sets the field of the data source that provides the value of each list item.
        /// </summary>
        public string DataValueField
        {
            get => ddlUsers.DataValueField;
            set => ddlUsers.DataValueField = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [display unassigned].
        /// </summary>
        /// <value><c>true</c> if [display unassigned]; otherwise, <c>false</c>.</value>
        public bool DisplayUnassigned { get; set; } = false;

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>The selected value.</value>
        public string SelectedValue
        {
            get => ddlUsers.SelectedValue;
            set => ddlUsers.SelectedValue = value;
        }

        /// <summary>
        /// Gets the selected text.
        /// </summary>
        /// <value>The selected text.</value>
        public string SelectedText => ddlUsers.SelectedItem.Text;

        /// <summary>
        /// Gets or sets the data source.
        /// </summary>
        /// <value>The data source.</value>
        public List<ITUser> DataSource { get; set; }

        /// <summary>
        /// Binds a data source to the invoked server control and all its child controls.
        /// </summary>
        public override void DataBind()
        {
            ddlUsers.Items.Clear();
            ddlUsers.DataSource = DataSource;
            ddlUsers.DataBind();
            if (DisplayDefault)
                ddlUsers.Items.Insert(0, new ListItem(GetLocalString("SelectUser"), ""));
            if (DisplayDefault && DisplayUnassigned)
                ddlUsers.Items.Insert(1, new ListItem("Unassigned", "-1"));
            else if (DisplayUnassigned)
                ddlUsers.Items.Insert(0, new ListItem("Unassigned", "-1"));
        }

        /// <summary>
        /// Removes the default.
        /// </summary>
        public void RemoveDefault()
        {
            var defaultItem = ddlUsers.Items.FindByValue("-1");
            if (defaultItem != null)
                ddlUsers.Items.Remove(defaultItem);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:PickSingleUser"/> is required.
        /// </summary>
        /// <value><c>true</c> if required; otherwise, <c>false</c>.</value>
        public bool Required
        {
            get => reqVal.Visible;
            set => reqVal.Visible = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:PickSingleUser"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled
        {
            get => ddlUsers.Enabled;
            set => ddlUsers.Enabled = value;
        }
    }
}