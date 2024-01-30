using BExIS.App.Testing;
using BExIS.Security.Services.Authentication;
using BExIS.Utils.Config;
using NUnit.Framework;
using System;
using System.Configuration;

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

            ConfigurationManager.AppSettings["Ldap_Host_Name"] = "ldap1.uni-jena.de";
            ConfigurationManager.AppSettings["Ldap_Host_Port"] = "636";
            ConfigurationManager.AppSettings["Ldap_Host_Version"] = "3";
            ConfigurationManager.AppSettings["Ldap_Host_Ssl"] = "true";
            ConfigurationManager.AppSettings["Ldap_Host_AuthType"] = "1";
            ConfigurationManager.AppSettings["Ldap_Host_Scope"] = "0";
            ConfigurationManager.AppSettings["Ldap_Host_BaseDn"] = "ou=users,dc=uni-jena,dc=de";
            ConfigurationManager.AppSettings["Ldap_User_Identifier"] = "uid";
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
            LdapAuthenticationManager ldapAuthenticationManager = new LdapAuthenticationManager();

            var x = ldapAuthenticationManager.ValidateUser("ldap", "username", "password");

            Console.WriteLine(x.ToString());
        }
    }
}