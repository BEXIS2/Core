using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using BExIS.Utils.NH.Querying;
using System.Web;
using System.Security.Principal;
using BExIS.Utils.Config;
using BExIS.App.Testing;

namespace BExIS.Modules.RPM.UI.Tests
{
    public class IOTests
    {
        private TestSetupHelper helper = null;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);

           
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            helper.Dispose();
        }

        [Test]
        public void test()
        {

        }
    }
}