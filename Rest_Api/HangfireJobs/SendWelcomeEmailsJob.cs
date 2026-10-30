using System.Text;
using Rest_Api.Infrastructure.Email;
using Rest_Api.Infrastructure.Excel;

namespace Rest_Api.HangfireJobs
{
    /// <summary>Continuation = After Another Job — welcome email + attachment after Excel import.</summary>
    public class SendWelcomeEmailsJob
    {
        private readonly IExcelUserImportService _import;
        private readonly IEmailService _email;
        private readonly ILogger<SendWelcomeEmailsJob> _logger;

        public SendWelcomeEmailsJob(
            IExcelUserImportService import,
            IEmailService email,
            ILogger<SendWelcomeEmailsJob> logger)
        {
            _import = import;
            _email = email;
            _logger = logger;
        }

        public async Task Execute(string batchId)
        {
            var result = _import.ReadBatchResult(batchId);
            if (result == null)
            {
                _logger.LogWarning("No import result for batch {BatchId}", batchId);
                return;
            }

            foreach (var person in result.WelcomeRecipients.Where(r => !string.IsNullOrWhiteSpace(r.Email)))
            {
                var letter = BuildWelcomeLetter(person.FullName, person.EmployeeCode);
                var bytes = Encoding.UTF8.GetBytes(letter);

                var html = $"""
                    <p>Dear {person.FullName},</p>
                    <p>Welcome to <strong>Employee 360</strong>.</p>
                    <p>Your employee code is <strong>{person.EmployeeCode}</strong>.</p>
                    <p>Please find your welcome letter attached.</p>
                    """;

                await _email.SendAsync(
                    person.Email,
                    "Welcome to Employee 360",
                    html,
                    new[]
                    {
                        ($"WelcomeLetter_{person.EmployeeCode}.txt", bytes, "text/plain")
                    });

                _logger.LogInformation("Welcome email sent to {Email} for {Code}", person.Email, person.EmployeeCode);
            }
        }

        private static string BuildWelcomeLetter(string fullName, string employeeCode) =>
            $"""
            EMPLOYEE 360 — WELCOME LETTER
            =============================

            Dear {fullName},

            We are delighted to welcome you to Employee 360.
            Your employee code is: {employeeCode}

            Use this code to sign in to the portal. After password login you will
            receive a one-time password (OTP) on your registered email.

            Regards,
            HR Team — Employee 360
            Generated (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}
            """;
    }
}
