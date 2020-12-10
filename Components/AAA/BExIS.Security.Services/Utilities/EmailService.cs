using Microsoft.AspNet.Identity;
using MimeKit;
using System.Collections.Generic;
using System.Configuration;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using Vaiona.Utils.Cfg;
using System.Linq;
using MailKit.Net.Smtp;

namespace BExIS.Security.Services.Utilities
{
    public class EmailService : IIdentityMessageService
    {
        public EmailService()
        {
        }

        public void Send(MimeMessage message)
        {
            using (var client = new SmtpClient())
            {
                client.Connect(ConfigurationManager.AppSettings["Email_Host_Name"], int.Parse(ConfigurationManager.AppSettings["Email_Host_Port"]), MailKit.Security.SecureSocketOptions.Auto);
                client.Authenticate(ConfigurationManager.AppSettings["Email_Account_Name"], ConfigurationManager.AppSettings["Email_Account_Password"]);

                client.Send(message);

                client.Disconnect(true);
            }
        }

        public void Send(string subject, string body, List<string> destinations, List<string> ccs = null, List<string> bccs = null, List<string> replyTos = null)
        {
            var mimeMessage = new MimeMessage();

            mimeMessage.From.Add(new MailboxAddress(ConfigurationManager.AppSettings["Email_From_Name"], ConfigurationManager.AppSettings["Email_From_Address"]));
            if (destinations != null)
                mimeMessage.To.AddRange(destinations.Select(d => new MailboxAddress(d, d)));
            if (ccs != null)
                mimeMessage.Cc.AddRange(ccs.Select(c => new MailboxAddress(c, c)));
            if (bccs != null)
                mimeMessage.Bcc.AddRange(bccs.Select(b => new MailboxAddress(b, b)));
            if (replyTos != null)
                mimeMessage.ReplyTo.AddRange(replyTos.Select(r => new MailboxAddress(r, r)));
            mimeMessage.Subject = AppConfiguration.ApplicationName + " - " + subject;
            mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

            Send(mimeMessage);
        }

        public void Send(string subject, string body, string destination)
        {
            Send(subject, body, new List<string>() { destination }, null, null, null);
        }

        public void Send(IdentityMessage message)
        {
            var mimeMessage = new MimeMessage();

            mimeMessage.From.Add(new MailboxAddress(ConfigurationManager.AppSettings["Email_From_Name"], ConfigurationManager.AppSettings["Email_From_Address"]));
            mimeMessage.To.Add(new MailboxAddress(message.Destination, message.Destination));
            mimeMessage.Subject = AppConfiguration.ApplicationName + " - " + message.Subject;
            mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message.Body };

            Send(mimeMessage);
        }

        public async Task SendAsync(IdentityMessage message)
        {
            var mimeMessage = new MimeMessage();

            mimeMessage.From.Add(new MailboxAddress(ConfigurationManager.AppSettings["Email_From_Name"], ConfigurationManager.AppSettings["Email_From_Address"]));
            mimeMessage.To.Add(new MailboxAddress(message.Destination, message.Destination));
            mimeMessage.Subject = AppConfiguration.ApplicationName + " - " + message.Subject;
            mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message.Body };

            Send(mimeMessage);

            await Task.FromResult(0);
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

            return emails.Count > 0 ? emails : null;
        }
    }
}