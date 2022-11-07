using BExIS.Security.Services.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

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
            ConfigurationManager.AppSettings["Email_Host_Anonymous"] = "#";
            ConfigurationManager.AppSettings["Email_Host_SecureSocketOptions"] = "#";
            ConfigurationManager.AppSettings["Email_Host_CertificateRevocation"] = "#";
            ConfigurationManager.AppSettings["Email_Account_Name"] = "#";
            ConfigurationManager.AppSettings["Email_Account_Password"] = "#";
            ConfigurationManager.AppSettings["Email_From_Name"] = "#";
            ConfigurationManager.AppSettings["Email_From_Address"] = "#";
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

        //[Test]
        public void Send()
        {
            var file = new FileInfo("c:\\complete\\path\\to\\file.extension");

            var files = new List<FileInfo>();
            files.Add(file);

            EmailService emailService = new EmailService();

            emailService.Send("subject_test", "Hallo again!? Emails are working now!", new List<string>() { "m6thsv2@googlemail.com" }, null, null, null, files);
        }

        /*
         * Further test methods
         */

        //[Test]
        public void Send_EmailWithWhiteSpace_SendSuccess()
        {
            EmailService emailService = new EmailService();
            bool success = true;
            try
            {
                emailService.Send("subject_test", "Hallo again!? Emails are working now!", new List<string>() { " david.blaa@googlemail.com " });
            }
            catch (Exception ex)
            {
                success = false;
            }
            finally
            {
                Assert.IsTrue(success, "send mail was not succesfull");
            }

        }

        //[Test]
        public void Send_EmailWithWhiteSpaceinCCs_SendSuccess()
        {
            EmailService emailService = new EmailService();
            bool success = true;
            try
            {
                emailService.Send("subject_test", "Hallo again!? Emails are working now!", new List<string>() { "david.blaa@googlemail.com" }, new List<string>() { "david.schoene@uni-jena.de " });
            }
            catch (Exception ex)
            {
                success = false;
            }
            finally
            {
                Assert.IsTrue(success, "send mail was not succesfull");
            }
        }


        //[Test]
        public void Send_EmailWithWhiteSpaceinBCCs_SendSuccess()
        {
            EmailService emailService = new EmailService();
            bool success = true;
            try
            {
                emailService.Send("subject_test", "Hallo again!? Emails are working now!", new List<string>() { "david.blaa@googlemail.com" }, null, new List<string>() { "david.schoene@uni-jena.de " });
            }
            catch (Exception ex)
            {
                success = false;
            }
            finally
            {
                Assert.IsTrue(success, "send mail was not succesfull");
            }
        }
    }
}
