using Hangfire;

namespace Rest_Api.HangfireJobs
{
    /// <summary>
    /// Recurring (Cron) entry point.
    /// Pattern: Recurring fires → Enqueue primary job → Continuation runs after it succeeds.
    /// </summary>
    public class RecurringMaintenanceJob
    {
        private readonly IBackgroundJobClient _jobs;
        private readonly ILogger<RecurringMaintenanceJob> _logger;

        public RecurringMaintenanceJob(IBackgroundJobClient jobs, ILogger<RecurringMaintenanceJob> logger)
        {
            _jobs = jobs;
            _logger = logger;
        }

        public void Execute()
        {
            _logger.LogInformation("Recurring cron triggered — starting cleanup then continuation");

            // 1) Primary job (runs now from this recurring tick)
            var cleanupJobId = _jobs.Enqueue<CleanupExpiredOtpsJob>(job => job.Execute());

            // 2) Continuation = after the cleanup job succeeds
            _jobs.ContinueJobWith<NotifyOtpCleanupDoneJob>(
                cleanupJobId,
                job => job.Execute(cleanupJobId));

            _logger.LogInformation(
                "Queued cleanup {CleanupJobId} with continuation NotifyOtpCleanupDoneJob",
                cleanupJobId);
        }
    }
}
