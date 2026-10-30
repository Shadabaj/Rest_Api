using Rest_Api.Infrastructure.Email;
using Rest_Api.Infrastructure.Excel;

namespace Rest_Api.HangfireJobs
{
    /// <summary>Schedule = Later — email import summary a few minutes after upload.</summary>
    public class SendImportSummaryJob
    {
        private readonly IExcelUserImportService _import;
        private readonly IEmailService _email;
        private readonly IConfiguration _config;
        private readonly ILogger<SendImportSummaryJob> _logger;

        public SendImportSummaryJob(
            IExcelUserImportService import,
            IEmailService email,
            IConfiguration config,
            ILogger<SendImportSummaryJob> logger)
        {
            _import = import;
            _email = email;
            _config = config;
            _logger = logger;
        }

        public async Task Execute(string batchId)
        {
            var result = _import.ReadBatchResult(batchId);
            if (result == null)
                return;

            var to = _config["SmtpSettings:Username"];
            if (string.IsNullOrWhiteSpace(to))
                return;

            var html = $"""
                <p>Excel import summary for batch <strong>{batchId}</strong></p>
                <ul>
                  <li>Updated: {result.UpdatedCount}</li>
                  <li>Skipped: {result.SkippedCount}</li>
                  <li>Welcome recipients: {result.WelcomeRecipients.Count}</li>
                  <li>Errors: {result.Errors.Count}</li>
                </ul>
                """;

            await _email.SendAsync(to, $"Excel import summary — {batchId}", html);
            _logger.LogInformation("Import summary emailed for batch {BatchId}", batchId);
        }
    }
}
