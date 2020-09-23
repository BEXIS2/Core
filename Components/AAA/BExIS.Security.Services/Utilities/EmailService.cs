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

            if (!string.IsNullOrEmpty(AppConfiguration.ApplicationName))
            {
                AppId = AppConfiguration.ApplicationName + " - ";
            }

        }

        public void Send(string subject, string body, List<string> destinations, List<string> ccs = null, List<string> bccs = null, List<string> replyToLists = null)
        {

            using (var mail = new MailMessage())
            {

                mail.From = new MailAddress(ConfigurationManager.AppSettings["Email_From"]);
                mail.To.Add(string.Join(",", destinations));

                ccs = getValidEmails(ccs);
                if (ccs != null) mail.CC.Add(string.Join(",", ccs));

                bccs = getValidEmails(bccs);
                if (bccs != null) mail.Bcc.Add(string.Join(",", bccs));

                replyToLists = getValidEmails(replyToLists);
                if (replyToLists != null) mail.ReplyToList.Add(string.Join(",", replyToLists));

                mail.Subject = AppId + subject;
                mail.Body = body;
                mail.IsBodyHtml = true;



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

                using (var mail = new MailMessage(from, to))
                {
                    mail.Body = message.Body;
                    mail.IsBodyHtml = true;
                    mail.Subject = message.Subject;

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
        }

        public void Send(IdentityMessage message)
        {
            var from = new MailAddress(ConfigurationManager.AppSettings["Email_From"]);

            var to = new MailAddress(message.Destination);
            using (var mail = new MailMessage(from, to))
            {

                mail.Body = message.Body;
                mail.IsBodyHtml = true;
                mail.Subject = AppId + message.Subject;

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

        public void Send(string v1, string v2, object p)
        {
            throw new NotImplementedException();
        }

        public async Task SendAsync(IdentityMessage message)
        {
            var from = new MailAddress(ConfigurationManager.AppSettings["Email_From"]);

            var to = new MailAddress(message.Destination);
            using (var mail = new MailMessage(from, to))
            {
                mail.Body = message.Body;
                mail.IsBodyHtml = true;
                mail.Subject = AppId + message.Subject;

                try
                {
                    await _smtp.SendMailAsync(mail);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message + " smtp service is probably not configured correctly.");
                }
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

        private List<string> getValidEmails(List<string> emails)
        {
            if (emails == null) return emails;

            for (int i = 0; i < emails.Count; i++)
            {
                if (!IsValidEmail(emails[i])) emails.RemoveAt(i);
            }

            return emails.Count > 0 ? emails : null ;
        }
    }
}