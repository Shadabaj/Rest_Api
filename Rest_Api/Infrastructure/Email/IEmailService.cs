namespace Rest_Api.Infrastructure.Email
{
    public interface IEmailService
    {
        Task SendAsync(
            string toEmail,
            string subject,
            string htmlBody,
            IEnumerable<(string FileName, byte[] Content, string ContentType)>? attachments = null,
            CancellationToken cancellationToken = default);
    }
}
