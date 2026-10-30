using Rest_Api.Infrastructure.Otp;

namespace Rest_Api.HangfireJobs
{
    /// <summary>Recurring = Again and Again — purge expired OTP rows.</summary>
    public class CleanupExpiredOtpsJob
    {
        private readonly IOtpService _otpService;

        public CleanupExpiredOtpsJob(IOtpService otpService)
        {
            _otpService = otpService;
        }

        public Task Execute() => _otpService.CleanupExpiredAsync();
    }
}
