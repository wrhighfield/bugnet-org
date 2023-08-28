using BugNET.UI;

namespace BugNET.UserControls
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI.WebControls;
    using Entities;

    /// <summary>
    ///		This user control displays a dropdown list of projects.
    /// </summary>
    public partial class PickProject : BugNetUserControl
    {
        #region Web Form Designer generated code

        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        ///		Required method for Designer support - do not modify
        ///		the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }

        #endregion


        public event EventHandler SelectedIndexChanged;


        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>The selected value.</value>
        public int SelectedValue
        {
            get
            {
                if (dropProjects.SelectedValue == string.Empty)
                    return 0;
                return int.Parse(dropProjects.SelectedValue);
            }
            set => dropProjects.SelectedValue = value.ToString();
        }


        /// <summary>
        /// Gets the selected item.
        /// </summary>
        /// <value>The selected item.</value>
        public ListItem SelectedItem => dropProjects.SelectedItem;


        /// <summary>
        /// Gets or sets a value indicating whether [display default].
        /// </summary>
        /// <value><c>true</c> if [display default]; otherwise, <c>false</c>.</value>
        public bool DisplayDefault { get; set; } = false;


        /// <summary>
        /// Gets or sets the CSS class.
        /// </summary>
        /// <value>The CSS class.</value>
        public string CssClass
        {
            get => dropProjects.CssClass;
            set => dropProjects.CssClass = value;
        }


        /// <summary>
        /// Gets or sets a value indicating whether [auto post back].
        /// </summary>
        /// <value><c>true</c> if [auto post back]; otherwise, <c>false</c>.</value>
        public bool AutoPostBack
        {
            get => dropProjects.AutoPostBack;
            set => dropProjects.AutoPostBack = value;
        }


        /// <summary>
        /// Gets the selected text.
        /// </summary>
        /// <value>The selected text.</value>
        public string SelectedText => dropProjects.SelectedItem.Text;

        /// <summary>
        /// Gets or sets the data source.
        /// </summary>
        /// <value>The data source.</value>
        public List<Project> DataSource { get; set; }

        /// <summary>
        /// Binds a data source to the invoked server control and all its child controls.
        /// </summary>
        public override void DataBind()
        {
            dropProjects.DataSource = DataSource;
            dropProjects.DataTextField = "Name";
            dropProjects.DataValueField = "Id";
            dropProjects.DataBind();
            if (DisplayDefault)
                dropProjects.Items.Insert(0, new ListItem(GetLocalString("SelectProject"), "0"));
        }


        /// <summary>
        /// Raises the <see cref="E:SelectedIndexChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void OnSelectedIndexChanged(EventArgs e)
        {
            if (SelectedIndexChanged != null) SelectedIndexChanged(this, e);
        }

        /// <summary>
        /// Projects the selected index changed.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void ProjectSelectedIndexChanged(object s, EventArgs e)
        {
            OnSelectedIndexChanged(e);
        }

        /// <summary>
        /// Removes the default.
        /// </summary>
        public void RemoveDefault()
        {
            var defaultItem = dropProjects.Items.FindByValue("0");
            if (defaultItem != null)
                dropProjects.Items.Remove(defaultItem);
        }
    }
}