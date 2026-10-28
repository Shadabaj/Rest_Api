using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "UserName is Required")]
        [RegularExpression(@"^NEO\d{4}$", ErrorMessage = "UserName must start with NEO followed by 4 digits (e.g. NEO1234)")]
        public string UserName { get; set; }

        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",ErrorMessage = "Password must contain at least 8 characters, 1 uppercase letter, 1 number, and 1 special character.")]
        public string Password { get; set; } = string.Empty;
    }
}
