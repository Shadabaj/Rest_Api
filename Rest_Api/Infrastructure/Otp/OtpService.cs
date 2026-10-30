using System.Security.Cryptography;
using System.Text;
using DAL;
using DAL.Entity;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models;
using Rest_Api.HangfireJobs;
using Services.Interface;

namespace Rest_Api.Infrastructure.Otp
{
    public class OtpService : IOtpService
    {
        private readonly AppDbContext _db;
        private readonly IUserService _userService;
        private readonly OtpExpirationSettings _otpSettings;
        private readonly IBackgroundJobClient _jobs;
        private readonly ILogger<OtpService> _logger;

        public OtpService(
            AppDbContext db,
            IUserService userService,
            IOptions<OtpExpirationSettings> otpSettings,
            IBackgroundJobClient jobs,
            ILogger<OtpService> logger)
        {
            _db = db;
            _userService = userService;
            _otpSettings = otpSettings.Value;
            _jobs = jobs;
            _logger = logger;
        }

        public async Task<LoginOtpPendingResponse?> CreateAndQueueOtpAsync(string userName, string? ipAddress)
        {
            var user = await _db.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u =>
                    u.UserName == userName
                    && u.IsActive
                    && u.Employee != null
                    && u.Employee.IsActive
                    && !u.Employee.IsDeleted);

            if (user?.Employee == null)
                return null;

            var email = user.Employee.EmailAddress?.Trim();
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException("No email is registered for this user. Contact Admin.");

            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            var otp = new LoginOtp
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = email,
                CodeHash = Hash(code),
                ExpiresAt = DateTime.UtcNow.AddMinutes(_otpSettings.ExpireMinutes),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            _db.LoginOtps.Add(otp);
            await _db.SaveChangesAsync();

            // Enqueue = Now — send OTP email immediately via Hangfire
            _jobs.Enqueue<SendOtpEmailJob>(j => j.Execute(otp.Id, code));

            _logger.LogInformation("OTP queued for {UserName} otpId {OtpId}", userName, otp.Id);

            return new LoginOtpPendingResponse
            {
                RequiresOtp = true,
                UserName = user.UserName,
                MaskedEmail = MaskEmail(email),
                ExpiresInMinutes = _otpSettings.ExpireMinutes,
                Message = $"OTP sent to {MaskEmail(email)}. Valid for {_otpSettings.ExpireMinutes} minutes."
            };
        }

        public async Task<EmployeeLogin?> VerifyOtpAndIssueTokensAsync(string userName, string otp, string? ipAddress)
        {
            var hash = Hash(otp.Trim());
            var now = DateTime.UtcNow;

            var record = await _db.LoginOtps
                .Where(o =>
                    o.UserName == userName
                    && o.CodeHash == hash
                    && o.ConsumedAt == null
                    && o.ExpiresAt >= now)
                .OrderByDescending(o => o.Id)
                .FirstOrDefaultAsync();

            if (record == null)
                return null;

            record.ConsumedAt = now;
            await _db.SaveChangesAsync();

            return _userService.IssueTokensForUser(record.UserId, ipAddress);
        }

        public async Task CleanupExpiredAsync()
        {
            var cutoff = DateTime.UtcNow.AddDays(-1);
            var old = await _db.LoginOtps
                .Where(o => o.ExpiresAt < cutoff || o.ConsumedAt != null && o.ConsumedAt < cutoff)
                .ToListAsync();

            if (old.Count == 0)
                return;

            _db.LoginOtps.RemoveRange(old);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Cleaned {Count} expired/consumed OTP rows", old.Count);
        }

        private static string Hash(string value)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(bytes);
        }

        private static string MaskEmail(string email)
        {
            var at = email.IndexOf('@');
            if (at <= 1)
                return "***";
            return email[0] + new string('*', Math.Max(1, at - 2)) + email[(at - 1)..];
        }
    }
}
