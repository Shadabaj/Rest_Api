using Models;

namespace Services.Interface
{
    public interface IUserService
    {
        string CreateUser(EmployeeLogin user);
        /// <summary>Validates credentials only (no JWT). Used before OTP.</summary>
        EmployeeLogin? ValidateUser(string email, string password, string? ipAddress = null);
        /// <summary>Issues access + refresh tokens after successful OTP.</summary>
        EmployeeLogin? IssueTokensForUser(int userId, string? ipAddress = null);
        TokenResponse? RefreshTokens(string refreshToken, string? ipAddress = null);
        bool RevokeRefreshToken(string refreshToken);
    }
}
