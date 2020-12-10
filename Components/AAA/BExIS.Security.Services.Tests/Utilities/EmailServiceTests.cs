using BExIS.Security.Services.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Security.Services.Tests.Utilities
{
    [TestFixture]
    public class EmailServiceTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ConfigurationManager.AppSettings["Email_Host_Name"] = "#";
            ConfigurationManager.AppSettings["Email_Host_Port"] = "#";
            ConfigurationManager.AppSettings["Email_Account_Name"] = "#";
            ConfigurationManager.AppSettings["Email_Account_Password"] = "#";
            ConfigurationManager.AppSettings["Email_From_Name"] = "Max Mustermann";
            ConfigurationManager.AppSettings["Email_From_Address"] = "max.mustermann@bexis.bexis";
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
        public void Send()
        {
            EmailService emailService = new EmailService();

            emailService.Send("subject_test", "Hallo again!? Emails are working now!", "david.schoene@uni-jena.de");
        }

        /*
         * Further test methods
         */
    }
}
