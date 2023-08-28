using System;
using System.Web.UI.WebControls;

namespace BugNET.Providers.HtmlEditorProviders
{
    /// <summary>
    /// 
    /// </summary>
    public class TextBoxHtmlEditorProvider : HtmlEditorProvider
    {
        private readonly TextBox textBox = new TextBox();

        /// <summary>
        /// Gets the HTML editor.
        /// </summary>
        /// <value>The HTML editor.</value>
        public override System.Web.UI.Control HtmlEditor => textBox;

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>The width.</value>
        public override Unit Width
        {
            get => textBox.Width;
            set => textBox.Width = value;
        }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        public override Unit Height
        {
            get => textBox.Height;
            set => textBox.Height = value;
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public override string Text
        {
            get => textBox.Text;
            set => textBox.Text = value;
        }

        /// <summary>
        /// Gets or sets the control id.
        /// </summary>
        /// <value>The control id.</value>
        public override string ControlId
        {
            get => textBox.ID;
            set => textBox.ID = value;
        }

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">The name of the provider is null.</exception>
        /// <exception cref="T:System.ArgumentException">The name of the provider has a length of zero.</exception>
        /// <exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.</exception>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            if (config == null || config.Count == 0)
                throw new ArgumentNullException(nameof(config), "You must supply a valid configuration dictionary.");

            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "TextboxHTMLEditorProvider");
            }

            //Let ProviderBase perform the basic initialization
            base.Initialize(name, config);

            //Perform feature-specific provider initialization here
            //A great deal more error checking and handling should exist here

            textBox.TextMode = TextBoxMode.MultiLine;
            textBox.Wrap = true;
            textBox.CssClass = "expanding";

            var text = config["Text"];
            Text = !string.IsNullOrEmpty(text) ? text : "";

            var height = config["Height"];
            Height = !string.IsNullOrEmpty(height) ? Unit.Parse(height) : Unit.Pixel(300);

            var width = config["Width"];
            Width = !string.IsNullOrEmpty(width) ? Unit.Parse(width) : Unit.Pixel(500);
        }

        /// <summary>
        /// 
        /// </summary>
        public override string ProviderPath => throw new NotImplementedException();
    }
}