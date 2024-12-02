using BExIS.Utils.Config;
using BExIS.Utils.Config.Configurations;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNet.Identity;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Utilities
{
    public class EmailService : IIdentityMessageService, IDisposable
    {
        private readonly IUnitOfWork _guow = null;
        private SmtpConfiguration _smtpConfiguration;
        private bool isDisposed = false;

        public EmailService(SmtpConfiguration smtpConfiguration)
        {
            _smtpConfiguration = smtpConfiguration;
        }

        public EmailService()
        {
            _smtpConfiguration = GeneralSettings.SmtpConfiguration;
        }

        ~EmailService()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (_guow != null)
                        _guow.Dispose();
                    isDisposed = true;
                }
            }
        }

        public void Send(MimeMessage message)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    // 2021-03-16 by Sven
                    // Because of trouble for specific server certificates, the system rejects handshakes.
                    client.CheckCertificateRevocation = _smtpConfiguration.HostCertificateRevocation;

                    // @2020-01-18 by Sven
                    // With this line, the service should accept every certificate.
                    client.ServerCertificateValidationCallback = (s, c, h, e) => { return true; };

                    client.Connect(_smtpConfiguration.HostName, _smtpConfiguration.HostPort, (SecureSocketOptions)_smtpConfiguration.HostSecureSocketOptions);

                    if (!_smtpConfiguration.HostAnonymous)
                    {
                        client.Authenticate(_smtpConfiguration.AccountName, _smtpConfiguration.AccountPassword);
                    }

                    client.Send(message);

                    client.Disconnect(true);
                }
            }
            catch (Exception exception)
            {
                //throw exception;
            }
        }

        public void Send(string subject, string body, List<string> destinations, List<string> ccs = null, List<string> bccs = null, List<string> replyTos = null, List<FileInfo> attachments = null)
        {
            List<string> _destinations = new List<string>();
            List<string> _ccs = new List<string>();
            List<string> _bccs = new List<string>();
            List<string> _replyTos = new List<string>();
            string _emailFromName = _smtpConfiguration.FromName;
            string _emailFromAddress = _smtpConfiguration.FromAddress;

            // clear all mails from white space

            #region clear emails

            if (destinations != null)
                _destinations = destinations.Select(innerItem => innerItem != null ? innerItem.Trim() : null).ToList();

            if (ccs != null)
                _ccs = ccs.Select(innerItem => innerItem != null ? innerItem.Trim() : null).ToList();

            if (bccs != null)
                _bccs = bccs.Select(innerItem => innerItem != null ? innerItem.Trim() : null).ToList();

            if (replyTos != null)
                _replyTos = replyTos.Select(innerItem => innerItem != null ? innerItem.Trim() : null).ToList();

            #endregion clear emails

            using (var mimeMessage = new MimeMessage())
            {
                mimeMessage.From.Add(new MailboxAddress(_emailFromName, _emailFromAddress));
                if (destinations != null)
                    mimeMessage.To.AddRange(_destinations.Select(d => new MailboxAddress(d, d)));
                if (ccs != null)
                    mimeMessage.Cc.AddRange(_ccs.Select(c => new MailboxAddress(c, c)));
                if (bccs != null)
                    mimeMessage.Bcc.AddRange(_bccs.Select(b => new MailboxAddress(b, b)));
                if (replyTos != null)
                    mimeMessage.ReplyTo.AddRange(_replyTos.Select(r => new MailboxAddress(r, r)));
                mimeMessage.Subject = GeneralSettings.ApplicationName + " - " + subject;

                var builder = new BodyBuilder();
                builder.HtmlBody = body;

                if (attachments != null)
                {
                    foreach (var attachment in attachments)
                    {
                        if (attachment.Length > 0)
                        {
                            using (FileStream inFile = attachment.OpenRead())
                            {
                                string fileName = Path.GetFileName(attachment.Name);
                                builder.Attachments.Add(fileName, inFile);
                            }
                        }
                    }
                }

                mimeMessage.Body = builder.ToMessageBody();

                Send(mimeMessage);
            }
        }

        public void Send(string subject, string body, string destination)
        {
            Send(subject, body, new List<string>() { destination.Trim() }, null, null, null, null);
        }

        public void Send(IdentityMessage message)
        {
            using (var mimeMessage = new MimeMessage())
            {
                mimeMessage.From.Add(new MailboxAddress(_smtpConfiguration.FromName, _smtpConfiguration.FromAddress));
                mimeMessage.To.Add(new MailboxAddress(message.Destination.Trim(), message.Destination.Trim()));
                mimeMessage.Subject = GeneralSettings.ApplicationName + " - " + message.Subject;
                mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message.Body };

                Send(mimeMessage);
            }
        }

        public async Task SendAsync(IdentityMessage message)
        {
            using (var mimeMessage = new MimeMessage())
            {
                mimeMessage.From.Add(new MailboxAddress(_smtpConfiguration.FromName, _smtpConfiguration.FromAddress));
                mimeMessage.To.Add(new MailboxAddress(message.Destination, message.Destination));
                mimeMessage.Subject = GeneralSettings.ApplicationName + " - " + message.Subject;
                mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message.Body };

                Send(mimeMessage);
                await Task.FromResult(0);
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

            return emails.Count > 0 ? emails : null;
        }
    }
}