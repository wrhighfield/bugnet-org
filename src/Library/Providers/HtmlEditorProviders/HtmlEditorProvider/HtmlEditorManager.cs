using System.Configuration;
using System.Configuration.Provider;
using System.Web.Configuration;

namespace BugNET.Providers.HtmlEditorProviders
{
    /// <summary>
    /// 
    /// </summary>
    public class HtmlEditorManager
    {
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private static void Initialize()
        {
            //Get the feature's configuration info
            var qc = ConfigurationManager.GetSection("HtmlEditorProvider") as HtmlEditorConfiguration;

            if (qc != null && (qc.DefaultProvider == null || qc.Providers == null || qc.Providers.Count < 1))
                throw new ProviderException("You must specify a valid default provider.");

            //Instantiate the providers
            Providers = new HtmlEditorProviderCollection();
            ProvidersHelper.InstantiateProviders(qc.Providers, Providers, typeof(HtmlEditorProvider));
            Providers.SetReadOnly();
            _defaultProvider = Providers[qc.DefaultProvider];

            if (_defaultProvider == null)
            {
                throw new ConfigurationErrorsException(
                    "You must specify a default provider for the feature.",
                    qc.ElementInformation.Properties["defaultProvider"]?.Source,
                    qc.ElementInformation.Properties["defaultProvider"].LineNumber);
            }
        }

        //Public feature API
        private static HtmlEditorProvider _defaultProvider;

        /// <summary>
        /// Gets the provider.
        /// </summary>
        /// <value>The provider.</value>
        public static HtmlEditorProvider Provider
        {
            get
            {
                Initialize();
                return _defaultProvider;
            }
        }

        /// <summary>
        /// Gets the providers.
        /// </summary>
        /// <value>The providers.</value>
        public static HtmlEditorProviderCollection Providers { get; private set; }
    }
}
