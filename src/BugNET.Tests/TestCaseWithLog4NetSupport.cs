using NUnit.Framework;
using log4net.Config;

namespace BugNET.Tests
{
    public class TestCaseWithLog4NetSupport
    {
        [OneTimeSetUp]
        public void ConfigureLog4Net()
        {
            XmlConfigurator.Configure();
        }
    }
}
