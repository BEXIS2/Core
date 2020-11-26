using BExIS.App.Testing;
using BExIS.Security.Services.Authentication;
using BExIS.Utils.Config;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Security.Services.Tests.Authentication
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]
    [TestFixture]
    public class LdapAuthenticationManagerTests
    {
        private TestSetupHelper helper = null;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);
            ConfigurationManager.AppSettings["Ldap_ConnectionString"] = "ldapHost:ldap1.uni-jena.de;ldapPort:636;ldapBaseDn:ou=users,dc=uni-jena,dc=de;ldapSecure:true;ldapAuthUid:uid;ldapProtocolVersion:3";
        }

        [SetUp]
        protected void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }

        [Test]
        public void Test()
        {
            LdapAuthenticationManager ldapAuthenticationManager = new LdapAuthenticationManager(ConfigurationManager.AppSettings["Ldap_ConnectionString"]);

            var x = ldapAuthenticationManager.ValidateUser("", "");

            Console.WriteLine(x.ToString());
        }
    }
}
