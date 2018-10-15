using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Vaiona.Utils.Cfg;

namespace BExIS.Security.Services.Utilities
{
    public class EmailService : IIdentityMessageService
    {
        private readonly SmtpClient _smtp;
        private string AppId = "";

        public EmailService()
        {
            _smtp = new SmtpClient()
            {
                Host = ConfigurationManager.AppSettings["Email_Host"],
                Port = int.Parse(ConfigurationManager.AppSettings["Email_Port"]),
                EnableSsl = bool.Parse(ConfigurationManager.AppSettings["Email_Ssl"])
            };

            if (bool.Parse(ConfigurationManager.AppSettings["Email_Anonymous"]))
            {
                _smtp.UseDefaultCredentials = false;
            }
            else
            {
                _smtp.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["Email_Account"],
                    ConfigurationManager.AppSettings["Email_Password"]);
            }

            if (!string.IsNullOrEmpty(AppConfiguration.ApplicationName) && !string.IsNullOrEmpty(AppConfiguration.ApplicationVersion))
            {
                AppId = AppConfiguration.ApplicationName + " (" + AppConfiguration.ApplicationVersion + ") - ";
            }

        }

        public void Send(string subject, string body, List<string> destinations, List<string> ccs = null, List<string> bccs = null, List<string> replyToLists = null)
        {
            var mail = new MailMessage()
            {
                From = new MailAddress(ConfigurationManager.AppSettings["Email_From"]),
                To = { string.Join(",", destinations) },
                CC = { string.Join(",", ccs) },
                Bcc = { string.Join(",", bccs) },
                ReplyToList = { string.Join(",", replyToLists) },
                Subject = AppId + subject,
                Body = body
            };

            try
            {
                _smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message + " smtp service is probably not configured correctly.");
            }
        }

        public void Send(string subject, string body, string destination)
        {
            if (IsValidEmail(destination))
            {
                IdentityMessage message = new IdentityMessage()
                {
                    Subject = AppId + subject,
                    Body = body,
                    Destination = destination
                };

                var from = new MailAddress(ConfigurationManager.AppSettings["Email_From"]);

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
                    Trace.TraceError(ex.Message + " smtp service is probably not configured correctly.");
                }
            }
        }

        public void Send(IdentityMessage message)
        {
            var from = new MailAddress(ConfigurationManager.AppSettings["Email_From"]);

            var to = new MailAddress(message.Destination);
            var mail = new MailMessage(from, to)
            {
                Body = message.Body,
                IsBodyHtml = true,
                Subject = AppId + message.Subject
            };

            try
            {
                _smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message + " smtp service is probably not configured correctly.");
            }
        }

        public void Send(string v1, string v2, object p)
        {
            throw new NotImplementedException();
        }

        public async Task SendAsync(IdentityMessage message)
        {
            var from = new MailAddress(ConfigurationManager.AppSettings["Email_From"]);

            var to = new MailAddress(message.Destination);
            var mail = new MailMessage(from, to)
            {
                Body = message.Body,
                IsBodyHtml = true,
                Subject = AppId + message.Subject
            };

            try
            {
                await _smtp.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message + " smtp service is probably not configured correctly.");
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}