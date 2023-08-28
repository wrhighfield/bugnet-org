using System.Configuration.Provider;

namespace BugNET.DAL
{
    public class DataProviderCollection : ProviderCollection
    {
        // Return an instance of DataProvider  
        // for a specified provider name  
        public new DataProvider this[string name] => (DataProvider) base[name];
    }
}