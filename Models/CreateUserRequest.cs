using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class CreateUserRequest
    {
        //[Required(ErrorMessage = "EmployeeId is Required")]
        //[Range(1, int.MaxValue, ErrorMessage = "EmployeeId must be greater than 0")]
        public int EmployeeId { get; set; }

        //[Required(ErrorMessage = "UserName is Required")]
        //[StringLength(100, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        //[Required(ErrorMessage = "Password is Required")]
        //[StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role is Required")]
        public string Role { get; set; } = string.Empty;
    }
}
