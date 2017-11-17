using Microsoft.AspNet.Identity;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BExIS.Security.Services.Utilities
{
    public class EmailService : IIdentityMessageService
    {
        private readonly SmtpClient _smtp;

        public EmailService()
        {
            _smtp = new SmtpClient()
            {
                Host = "smtp.uni-jena.de",
                Port = 587,
                EnableSsl = true,
                Credentials =
                    new NetworkCredential(ConfigurationManager.AppSettings["EmailAccount"],
                        ConfigurationManager.AppSettings["EmailPassword"])
            };
        }

        public void Send(string subject, string body, string destination)
        {
            IdentityMessage message = new IdentityMessage()
            {
                Subject = subject,
                Body = body,
                Destination = destination
            };

            var from = new MailAddress("bexis2@uni-jena.de");

            var to = new MailAddress(message.Destination);
            var mail = new MailMessage(from, to)
            {
                Body = message.Body,
                IsBodyHtml = true,
                Subject = message.Subject
            };

            try
            {
                _smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message + " SparkPost probably not configured correctly.");
            }
        }

        public void Send(IdentityMessage message)
        {
            var from = new MailAddress("bexis2@uni-jena.de");

            var to = new MailAddress(message.Destination);
            var mail = new MailMessage(from, to)
            {
                Body = message.Body,
                IsBodyHtml = true,
                Subject = message.Subject
            };

            try
            {
                _smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message + " SparkPost probably not configured correctly.");
            }
        }

        public async Task SendAsync(IdentityMessage message)
        {
            var from = new MailAddress("bexis2@uni-jena.de");

            var to = new MailAddress(message.Destination);
            var mail = new MailMessage(from, to)
            {
                Body = message.Body,
                IsBodyHtml = true,
                Subject = message.Subject
            };

            try
            {
                await _smtp.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message + " SparkPost probably not configured correctly.");
            }
        }

        public void Send(string v1, string v2, object p)
        {
            throw new NotImplementedException();
        }
    }
}