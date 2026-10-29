using Models;

namespace Services.Interface
{
    public interface IUserService
    {
        string CreateUser(EmployeeLogin user);
        EmployeeLogin? ValidateUser(string email, string password, string? ipAddress = null);
        TokenResponse? RefreshTokens(string refreshToken, string? ipAddress = null);
        bool RevokeRefreshToken(string refreshToken);
    }
}
