using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class ProvisionEmployeeRequest
    {
        [Required] public string FirstName { get; set; } = string.Empty;
        [Required] public string LastName { get; set; } = string.Empty;
        [Required, EmailAddress] public string EmailAddress { get; set; } = string.Empty;
        public string? MobileNumber { get; set; }
        [Required] public string Password { get; set; } = string.Empty;
        [Required, Range(1, int.MaxValue)] public int RoleId { get; set; }
        public int? CountryId { get; set; }
        public int? StateId { get; set; }
        public int? CityId { get; set; }
        public int? DepartmentId { get; set; }
        public int? DesignationId { get; set; }
        public int? WorkLocationId { get; set; }
        public int? ReportingManagerId { get; set; }
        public DateTime DateOfBirth { get; set; } = new(1995, 1, 1);
        public DateTime DateOfJoinee { get; set; } = DateTime.UtcNow.Date;
        public bool IsActive { get; set; } = true;
    }

    public class SubmitLeaveRequest
    {
        [Required, Range(1, int.MaxValue)] public int LeaveTypeId { get; set; }
        [Required] public DateTime StartDate { get; set; }
        [Required] public DateTime EndDate { get; set; }
        public bool IsHalfDay { get; set; }
        public string? HalfDayPart { get; set; }
        public string? Reason { get; set; }
    }

    public class TripPlanRequest
    {
        [Required] public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required] public DateTime StartDate { get; set; }
        [Required] public DateTime EndDate { get; set; }
        [Range(1, double.MaxValue)] public decimal TargetAmount { get; set; }
    }

    public class TripPaymentRequest
    {
        [Range(0.01, double.MaxValue)] public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "UPI";
    }
}
