using DAL.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Models;
using Repository.Interface;
using Services.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepo _userRepo;
        private readonly IConfiguration _config;

        public UserService(IUserRepo userRepo, IConfiguration configuration)
        {
            _userRepo = userRepo;
            _config = configuration;
        }

        public string CreateUser(EmployeeLogin user)
        {
            return _userRepo.CreateUser(user);
        }

        public EmployeeLogin? ValidateUser(string email, string password, string? ipAddress = null)
        {
            var user = _userRepo.ValidateUser(email, password);
            if (user == null)
                return null;

            var tokens = IssueTokenPair(user, ipAddress);
            _userRepo.SaveChanges();

            user.Token = tokens.AccessToken;
            user.RefreshToken = tokens.RefreshToken;
            user.ExpiresIn = tokens.ExpiresIn;
            return user;
        }

        public TokenResponse? RefreshTokens(string refreshToken, string? ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return null;

            var tokenHash = HashToken(refreshToken);
            var stored = _userRepo.GetRefreshTokenByHash(tokenHash);
            if (stored == null || stored.RevokedAt != null || stored.ExpiresAt <= DateTime.UtcNow)
                return null;

            var user = _userRepo.GetActiveUserById(stored.UserId);
            if (user == null)
                return null;

            var tokens = IssueTokenPair(user, ipAddress);

            stored.RevokedAt = DateTime.UtcNow;
            stored.ReplacedByTokenHash = HashToken(tokens.RefreshToken);
            _userRepo.UpdateRefreshToken(stored);
            _userRepo.SaveChanges();

            return tokens;
        }

        public bool RevokeRefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var stored = _userRepo.GetRefreshTokenByHash(HashToken(refreshToken));
            if (stored == null || stored.RevokedAt != null)
                return false;

            stored.RevokedAt = DateTime.UtcNow;
            _userRepo.UpdateRefreshToken(stored);
            _userRepo.SaveChanges();
            return true;
        }

        private TokenResponse IssueTokenPair(EmployeeLogin user, string? ipAddress)
        {
            var accessMinutes = GetAccessTokenMinutes();
            var accessToken = GenerateAccessToken(user, accessMinutes);
            var refreshToken = GenerateRefreshTokenValue();

            _userRepo.AddRefreshToken(new RefreshToken
            {
                UserId = user.UserId,
                TokenHash = HashToken(refreshToken),
                ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenDays()),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            });

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = accessMinutes * 60
            };
        }

        private string GenerateAccessToken(EmployeeLogin user, int accessMinutes)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.UserName),
                new("name", string.Concat(user.FirstName, " ", user.LastName).Trim()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("userId", user.UserId.ToString()),
                new("employeeId", user.EmployeeId.ToString())
            };

            foreach (var role in user.Roles.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct())
                claims.Add(new Claim("role", role.Trim()));

            var handler = new JwtSecurityTokenHandler();
            handler.OutboundClaimTypeMap.Clear();

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],_config["Jwt:Audience"],claims,expires: DateTime.UtcNow.AddMinutes(accessMinutes),signingCredentials: credentials);

            return handler.WriteToken(token);
        }

        private static string GenerateRefreshTokenValue()
        {
            var bytes = new byte[64];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }

        private static string HashToken(string token)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(hash);
        }

        private int GetAccessTokenMinutes()
        {
            return int.TryParse(_config["Jwt:AccessTokenMinutes"], out var minutes) && minutes > 0 ? minutes: 15;
        }

        private int GetRefreshTokenDays()
        {
            return int.TryParse(_config["Jwt:RefreshTokenDays"], out var days) && days > 0? days :7;
        }
    }
}
