﻿using BExIS.Security.Services.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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

        [Test]
        public void Send()
        {
            var file = new FileInfo("c:\\complete\\path\\to\\file.extension");

            var files = new List<FileInfo>();
            files.Add(file);

            EmailService emailService = new EmailService();

            emailService.Send("subject_test", "Hallo again!? Emails are working now!", new List<string>(){ "m6thsv2@googlemail.com"} , null, null, null, files);
        }

        /*
         * Further test methods
         */
    }
}
