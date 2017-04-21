using Microsoft.AspNet.Identity;
using System;
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
                Host = "smtp.sparkpostmail.com",
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    "SMTP_Injection", "d0d8887db944fda40063c8d959f9532711fd8230"
                )
            };
        }

        public async Task SendAsync(IdentityMessage message)
        {
            var from = new MailAddress("testing@sparkpostbox.com");

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

        public void SendNotification(IdentityMessage message)
        {
        }
    }
}
