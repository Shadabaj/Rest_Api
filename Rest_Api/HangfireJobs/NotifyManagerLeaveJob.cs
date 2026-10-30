using DAL;
using Microsoft.EntityFrameworkCore;
using Rest_Api.Infrastructure.Email;

namespace Rest_Api.HangfireJobs
{
    /// <summary>Enqueue when leave is requested — email reporting manager.</summary>
    public class NotifyManagerLeaveJob
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _email;
        private readonly ILogger<NotifyManagerLeaveJob> _logger;

        public NotifyManagerLeaveJob(AppDbContext db, IEmailService email, ILogger<NotifyManagerLeaveJob> logger)
        {
            _db = db;
            _email = email;
            _logger = logger;
        }

        public async Task Execute(long leaveRequestId)
        {
            var leave = await _db.LeaveRequests.AsNoTracking()
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync(l => l.LeaveRequestId == leaveRequestId);

            if (leave?.Employee == null)
                return;

            if (leave.Employee.ReportingManagerId == null)
            {
                _logger.LogWarning("Leave {Id}: no reporting manager configured", leaveRequestId);
                return;
            }

            var manager = await _db.EmployeeMasters.AsNoTracking()
                .FirstOrDefaultAsync(e => e.RowId == leave.Employee.ReportingManagerId && !e.IsDeleted);

            if (manager == null || string.IsNullOrWhiteSpace(manager.EmailAddress))
            {
                _logger.LogWarning("Leave {Id}: manager email missing", leaveRequestId);
                return;
            }

            var empName = $"{leave.Employee.FirstName} {leave.Employee.LastName}".Trim();
            var html = $"""
                <p>Hello {manager.FirstName},</p>
                <p><strong>{empName}</strong> ({leave.Employee.EmployeeCode}) has applied for leave.</p>
                <ul>
                  <li>Type: {leave.LeaveType?.LeaveName}</li>
                  <li>From: {leave.StartDate:yyyy-MM-dd}</li>
                  <li>To: {leave.EndDate:yyyy-MM-dd}</li>
                  <li>Days: {leave.RequestedDays}</li>
                  <li>Reason: {leave.Reason}</li>
                  <li>Status: {leave.RequestStatus}</li>
                </ul>
                <p>Please review this request in Employee 360.</p>
                """;

            await _email.SendAsync(
                manager.EmailAddress,
                $"Leave request from {empName}",
                html);

            _logger.LogInformation("Manager leave email sent for leave {Id} to {Email}", leaveRequestId, manager.EmailAddress);
        }
    }
}
