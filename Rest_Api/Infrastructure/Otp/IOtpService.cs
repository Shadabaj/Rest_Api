using Models;

namespace Rest_Api.Infrastructure.Otp
{
    public interface IOtpService
    {
        Task<LoginOtpPendingResponse?> CreateAndQueueOtpAsync(string userName, string? ipAddress);
        Task<EmployeeLogin?> VerifyOtpAndIssueTokensAsync(string userName, string otp, string? ipAddress);
        Task CleanupExpiredAsync();
    }
}
