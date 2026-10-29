using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "RefreshToken is Required")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
