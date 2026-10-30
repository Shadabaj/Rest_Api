using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class LoginOtpPendingResponse
    {
        public bool RequiresOtp { get; set; } = true;
        public string UserName { get; set; } = string.Empty;
        public string MaskedEmail { get; set; } = string.Empty;
        public string Message { get; set; } = "OTP sent to your registered email.";
        public int ExpiresInMinutes { get; set; }
    }

    public class VerifyOtpRequest
    {
        [Required]
        [RegularExpression(@"^NEO\d{4}$")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be 6 digits.")]
        public string Otp { get; set; } = string.Empty;
    }

    public class ExcelImportRow
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class ExcelImportResult
    {
        public string BatchId { get; set; } = string.Empty;
        public int UpdatedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<WelcomeRecipient> WelcomeRecipients { get; set; } = new();
    }

    public class WelcomeRecipient
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
