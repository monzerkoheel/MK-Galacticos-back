using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MK.Core.Contracts.ServicesContracts;

namespace MK.Service.HelperServices.EmailSevrices
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        public EmailService(IOptions<EmailSettings> options)
        {
            _emailSettings = options.Value;
        }

        public async Task SendEmailAsync(
                    string to,
                    string subject,
                    string body,
                    byte[]? attachment = null,
                    string? attachmentName = null)
        {
            var email = new MimeMessage();

            email.From.Add(
                new MailboxAddress(
                    _emailSettings.FromName,
                    _emailSettings.Username));

            email.To.Add(MailboxAddress.Parse(to));

            email.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };

            if (attachment is not null &&
                !string.IsNullOrWhiteSpace(attachmentName))
            {
                bodyBuilder.Attachments.Add(
                    attachmentName,
                    attachment,
                    new ContentType("application", "pdf"));
            }

            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _emailSettings.Host,
                _emailSettings.Port,
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _emailSettings.Username,
                _emailSettings.Password);

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
        }
    }
}

