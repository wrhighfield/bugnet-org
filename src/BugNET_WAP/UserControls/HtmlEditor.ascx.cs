using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using BugNET.Providers.HtmlEditorProviders;
using log4net;

namespace BugNET.UserControls
{
    [ValidationProperty("Text")]
    public partial class HtmlEditor : UserControl
    {
        private HtmlEditorProvider p;
        private static readonly ILog Log = LogManager.GetLogger(typeof(HtmlEditor));

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            try
            {
                p = HtmlEditorManager.Provider;
                p.ControlId = ID;

                if (Height != Unit.Empty)
                    p.Height = Height;
                if (Width != Unit.Empty)
                    p.Width = Width;

                Controls.Add(p.HtmlEditor);
            }
            catch (Exception ex)
            {
                Log.Error(this, new Exception(
                    $"An error occurred initializing the HtmlEditorProvider: {ex.Message} \n\n {ex.StackTrace}"));
                // Throw an exception now so you don't get exceptions when 
                // other pages try to work with the control.
                Response.Redirect("~/Errors/Error.aspx");
                // throw new Exception("An error occurred initializing the HtmlEditorProvider. See log for details.");
            }
        }


        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text
        {
            get => p.Text;
            set => p.Text = value;
        }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        public Unit Height { get; set; } = Unit.Empty;

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>The width.</value>
        public Unit Width { get; set; } = Unit.Empty;
    }
}