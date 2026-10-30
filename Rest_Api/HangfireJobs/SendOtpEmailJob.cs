using DAL;
using Microsoft.EntityFrameworkCore;
using Rest_Api.Infrastructure.Email;

namespace Rest_Api.HangfireJobs
{
    /// <summary>Enqueue = Now — send login OTP email immediately.</summary>
    public class SendOtpEmailJob
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _email;
        private readonly ILogger<SendOtpEmailJob> _logger;

        public SendOtpEmailJob(AppDbContext db, IEmailService email, ILogger<SendOtpEmailJob> logger)
        {
            _db = db;
            _email = email;
            _logger = logger;
        }

        public async Task Execute(long otpId, string plainCode)
        {
            var otp = await _db.LoginOtps.AsNoTracking().FirstOrDefaultAsync(o => o.Id == otpId);
            if (otp == null)
            {
                _logger.LogWarning("OTP {OtpId} not found for email job", otpId);
                return;
            }

            var html = $"""
                <p>Hello {otp.UserName},</p>
                <p>Your Employee 360 login OTP is:</p>
                <h2 style="letter-spacing:4px;">{plainCode}</h2>
                <p>This code expires at <strong>{otp.ExpiresAt:u} UTC</strong>.</p>
                <p>If you did not request this, ignore this email.</p>
                """;

            await _email.SendAsync(otp.Email, "Employee 360 — Login OTP", html);
        }
    }
}
