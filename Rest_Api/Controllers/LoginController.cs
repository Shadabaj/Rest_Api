using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services.Interface;

namespace Rest_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<LoginController> _logger;

        public LoginController(IUserService userService, ILogger<LoginController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("create")]
        [AllowAnonymous]
        public ActionResult CreateUser([FromBody] EmployeeLogin request)
        {
            _logger.LogInformation("CreateUser started {@Context}", new { Action = "CreateUser", RoleId = request.RoleId });

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "CreateUser validation failed {@Context}",
                    new { Action = "CreateUser", Errors = ModelState.Keys });
                return BadRequest(ModelState);
            }

            try
            {
                var user = new EmployeeLogin
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    DateOfBirth = request.DateOfBirth,
                    DateOfJoinee = request.DateOfJoinee,
                    PassWord = request.PassWord,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    RoleId = request.RoleId
                };

                var created = _userService.CreateUser(user);
                if (string.IsNullOrEmpty(created))
                {
                    _logger.LogWarning(
                        "CreateUser failed {@Context}",
                        new { Action = "CreateUser", Reason = "DuplicateOrInvalidRole" });
                    return BadRequest(new { message = "Unable to create user. Check EmployeeId, UserName uniqueness, and that the Role exists." });
                }

                _logger.LogInformation(
                    "CreateUser succeeded {@Context}",
                    new { Action = "CreateUser", UserName = created });
                return Ok(new { message = "User created successfully.", userName = created });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "CreateUser failed {@Context}",
                    new { Action = "CreateUser", RoleId = request.RoleId });
                throw;
            }
        }

        [HttpPost("validate")]
        [AllowAnonymous]
        public ActionResult<EmployeeLogin> ValidateUser([FromBody] LoginModel login)
        {
            _logger.LogInformation("ValidateUser started {@Context}", new { Action = "ValidateUser", UserName = login.UserName });

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ValidateUser validation failed {@Context}", new { Action = "ValidateUser", UserName = login.UserName });
                return BadRequest(ModelState);
            }

            try
            {
                var result = _userService.ValidateUser(login.UserName, login.Password, HttpContext.Connection.RemoteIpAddress?.ToString());
                if (result == null)
                {
                    _logger.LogWarning("ValidateUser unauthorized {@Context}", new { Action = "ValidateUser", UserName = login.UserName });
                    return Unauthorized(new { message = "Invalid UserName or password." });
                }

                _logger.LogInformation("ValidateUser succeeded {@Context}", new { Action = "ValidateUser", UserName = login.UserName, EmployeeId = result.EmployeeId });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ValidateUser failed {@Context}", new { Action = "ValidateUser", UserName = login.UserName });
                throw;
            }
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public ActionResult<TokenResponse> Refresh([FromBody] RefreshTokenRequest request)
        {
            _logger.LogInformation("RefreshToken started {@Context}", new { Action = "RefreshToken" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var tokens = _userService.RefreshTokens(request.RefreshToken, HttpContext.Connection.RemoteIpAddress?.ToString());
                if (tokens == null)
                {
                    _logger.LogWarning("RefreshToken rejected {@Context}", new { Action = "RefreshToken", Reason = "InvalidOrExpired" });
                    return Unauthorized(new { message = "Invalid or expired refresh token." });
                }

                _logger.LogInformation("RefreshToken succeeded {@Context}", new { Action = "RefreshToken" });
                return Ok(tokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RefreshToken failed {@Context}", new { Action = "RefreshToken" });
                throw;
            }
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public ActionResult Logout([FromBody] RefreshTokenRequest request)
        {
            _logger.LogInformation("Logout started {@Context}", new { Action = "Logout" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var revoked = _userService.RevokeRefreshToken(request.RefreshToken);
            if (!revoked)
            {
                _logger.LogWarning("Logout token not found {@Context}", new { Action = "Logout" });
                return Ok(new { message = "Logged out." });
            }

            _logger.LogInformation("Logout succeeded {@Context}", new { Action = "Logout" });
            return Ok(new { message = "Logged out." });
        }
    }
}
