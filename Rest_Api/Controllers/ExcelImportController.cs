using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Rest_Api.Filters;
using Rest_Api.HangfireJobs;

namespace Rest_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizationFilter(Roles = "Admin,HR")]
    public class ExcelImportController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IBackgroundJobClient _jobs;
        private readonly ILogger<ExcelImportController> _logger;

        public ExcelImportController(
            IWebHostEnvironment env,
            IBackgroundJobClient jobs,
            ILogger<ExcelImportController> logger)
        {
            _env = env;
            _jobs = jobs;
            _logger = logger;
        }

        /// <summary>
        /// Upload Excel (.xlsx). Columns: EmployeeCode, FirstName, LastName, EmailAddress, MobileNumber, IsActive.
        /// Hangfire: Enqueue import → Continuation welcome emails → Schedule summary later.
        /// </summary>
        [HttpPost("users")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> UploadUsers(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Excel file is required." });

            var ext = Path.GetExtension(file.FileName);
            if (!string.Equals(ext, ".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Only .xlsx files are supported." });

            var batchId = Guid.NewGuid().ToString("N");
            var dir = Path.Combine(_env.ContentRootPath, "App_Data", "imports");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, $"{batchId}{ext}");

            await using (var stream = System.IO.File.Create(path))
                await file.CopyToAsync(stream);

            // Enqueue = Now
            var importJobId = _jobs.Enqueue<ImportUsersFromExcelJob>(j => j.Execute(path, batchId));

            // Continuation = After Another Job (welcome letter + attachment)
            _jobs.ContinueJobWith<SendWelcomeEmailsJob>(importJobId, j => j.Execute(batchId));

            // Schedule = Later (summary email in 2 minutes)
            _jobs.Schedule<SendImportSummaryJob>(j => j.Execute(batchId), TimeSpan.FromMinutes(2));

            _logger.LogInformation(
                "Excel upload queued batch {BatchId} importJob {JobId}",
                batchId, importJobId);

            return Accepted(new
            {
                message = "Excel import queued.",
                batchId,
                importJobId,
                hangfire = new
                {
                    enqueue = "ImportUsersFromExcelJob (Now)",
                    continuation = "SendWelcomeEmailsJob (After import succeeds)",
                    schedule = "SendImportSummaryJob (Later — 2 minutes)",
                    recurringThenContinue = "Cron '0 * * * *' → RecurringMaintenanceJob → CleanupExpiredOtpsJob → NotifyOtpCleanupDoneJob"
                }
            });
        }
    }
}
