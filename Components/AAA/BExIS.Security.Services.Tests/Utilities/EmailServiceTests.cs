using BExIS.Security.Services.Utilities;
using BExIS.Utils.Config.Configurations;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace BExIS.Security.Services.Tests.Utilities
{
    [TestFixture]
    public class EmailServiceTests
    {
        private SmtpConfiguration _smtpConfiguration;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _smtpConfiguration = new SmtpConfiguration()
            {
                HostName = "smtp.uni-jena.de",
                HostPort = 587,
                HostAnonymous = false,
                HostCertificateRevocation = false,
                HostSecureSocketOptions = 1,
                AccountName = "<account_name>",
                AccountPassword = "<account_password>",
                FromName = "<from_name>",
                FromAddress = "<from_address>"
            };
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
            EmailService emailService = new EmailService(_smtpConfiguration);
            bool success = true;
            try
            {
                emailService.Send("subject_test", "Hallo again!? Emails are working now!", new List<string>() { "m6thsv2@googlemail.com" });
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