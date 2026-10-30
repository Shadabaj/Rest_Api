using Rest_Api.Infrastructure.Email;

namespace Rest_Api.HangfireJobs
{
    /// <summary>Continuation after recurring cleanup job completes.</summary>
    public class NotifyOtpCleanupDoneJob
    {
        private readonly IEmailService _email;
        private readonly IConfiguration _config;
        private readonly ILogger<NotifyOtpCleanupDoneJob> _logger;

        public NotifyOtpCleanupDoneJob(
            IEmailService email,
            IConfiguration config,
            ILogger<NotifyOtpCleanupDoneJob> logger)
        {
            _email = email;
            _config = config;
            _logger = logger;
        }

        public async Task Execute(string parentJobId)
        {
            _logger.LogInformation(
                "Continuation after cleanup parent job {ParentJobId}",
                parentJobId);

            var to = _config["SmtpSettings:Username"];
            if (string.IsNullOrWhiteSpace(to))
                return;

            var html = $"""
                <p>Hangfire recurring maintenance finished.</p>
                <p>Parent cleanup job id: <strong>{parentJobId}</strong></p>
                <p>UTC: {DateTime.UtcNow:u}</p>
                """;

            await _email.SendAsync(to, "Employee 360 — OTP cleanup completed", html);
        }
    }
}
