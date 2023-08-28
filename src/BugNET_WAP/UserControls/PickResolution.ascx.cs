using BugNET.UI;

namespace BugNET.UserControls
{
    using System.Collections.Generic;
    using System.Web.UI.WebControls;
    using Entities;

    /// <summary>
    ///		Summary description for PickResolution.
    /// </summary>
    public partial class PickResolution : BugNetUserControl
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, System.EventArgs e)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether [display default].
        /// </summary>
        /// <value><c>true</c> if [display default]; otherwise, <c>false</c>.</value>
        public bool DisplayDefault { get; set; } = false;

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>The selected value.</value>
        public int SelectedValue
        {
            get => int.Parse(ddlResolution.SelectedValue);
            set => ddlResolution.SelectedValue = value.ToString();
        }

        /// <summary>
        /// Gets the selected text.
        /// </summary>
        /// <value>The selected text.</value>
        public string SelectedText => ddlResolution.SelectedItem.Text;

        /// <summary>
        /// Gets or sets the data source.
        /// </summary>
        /// <value>The data source.</value>
        public List<Resolution> DataSource { get; set; }

        /// <summary>
        /// Binds a data source to the invoked server control and all its child controls.
        /// </summary>
        public override void DataBind()
        {
            ddlResolution.Items.Clear();
            ddlResolution.DataSource = DataSource;
            ddlResolution.DataTextField = "Name";
            ddlResolution.DataValueField = "Id";
            ddlResolution.DataBind();

            if (DisplayDefault)
                ddlResolution.Items.Insert(0, new ListItem(GetLocalString("SelectResolution"), "0"));
        }

        /// <summary>
        /// Removes the default.
        /// </summary>
        public void RemoveDefault()
        {
            var defaultItem = ddlResolution.Items.FindByValue("0");
            if (defaultItem != null)
                ddlResolution.Items.Remove(defaultItem);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PickResolution"/> is required.
        /// </summary>
        /// <value><c>true</c> if required; otherwise, <c>false</c>.</value>
        public bool Required
        {
            get => reqVal.Visible;
            set => reqVal.Visible = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PickResolution"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled
        {
            get => ddlResolution.Enabled;
            set => ddlResolution.Enabled = value;
        }
    }
}