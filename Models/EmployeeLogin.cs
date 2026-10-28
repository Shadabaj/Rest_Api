using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class EmployeeLogin
    {
        [Required(ErrorMessage = "FirstName is Required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "LastName is Required")]
        public string LastName { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public DateTime DateOfJoinee { get; set; }

        [Required(ErrorMessage = "Password is Required")]
        public string PassWord { get; set; } = string.Empty;

        public int RoleId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public int EmployeeId { get; set; }

        public  ICollection<string> Roles { get; set; } = new List<string>();

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }


    }
}
