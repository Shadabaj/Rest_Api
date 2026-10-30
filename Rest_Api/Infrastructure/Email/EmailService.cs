using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Models;

namespace Rest_Api.Infrastructure.Email
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtp;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<SmtpSettings> smtp, ILogger<EmailService> logger)
        {
            _smtp = smtp.Value;
            _logger = logger;
        }

        public async Task SendAsync(
            string toEmail,
            string subject,
            string htmlBody,
            IEnumerable<(string FileName, byte[] Content, string ContentType)>? attachments = null,
            CancellationToken cancellationToken = default)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_smtp.Username));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            if (attachments != null)
            {
                foreach (var (fileName, content, contentType) in attachments)
                    builder.Attachments.Add(fileName, content, ContentType.Parse(contentType));
            }

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            var secure = _smtp.EnableSSL ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
            await client.ConnectAsync(_smtp.Host, _smtp.Port, secure, cancellationToken);

            var password = (_smtp.Password ?? string.Empty).Replace(" ", string.Empty);
            await client.AuthenticateAsync(_smtp.Username, password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent to {ToEmail} subject {Subject}", toEmail, subject);
        }
    }
}
