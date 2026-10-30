using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Rest_Api.IpAddress;
using Rest_Api.Infrastructure.Otp;
using Services.Interface;

namespace Rest_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IOtpService _otpService;
        private readonly ILogger<LoginController> _logger;
        private readonly IipAddress _ipAddress;

        public LoginController(
            IUserService userService,
            IOtpService otpService,
            ILogger<LoginController> logger,
            IipAddress ipAddress)
        {
            _userService = userService;
            _otpService = otpService;
            _logger = logger;
            _ipAddress = ipAddress;
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

        /// <summary>
        /// Step 1: validate password, then Enqueue OTP email (Hangfire Now). Tokens are not returned yet.
        /// </summary>
        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<ActionResult> ValidateUser([FromBody] LoginModel login)
        {
            _logger.LogInformation("ValidateUser started {@Context}", new { Action = "ValidateUser", UserName = login.UserName });

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ValidateUser validation failed {@Context}", new { Action = "ValidateUser", UserName = login.UserName });
                return BadRequest(ModelState);
            }

            try
            {
                var user = _userService.ValidateUser(login.UserName, login.Password, _ipAddress.GetIpAddress());
                if (user == null)
                {
                    _logger.LogWarning("ValidateUser unauthorized {@Context}", new { Action = "ValidateUser", UserName = login.UserName });
                    return Unauthorized(new { message = "Invalid UserName or password." });
                }

                var otpPending = await _otpService.CreateAndQueueOtpAsync(login.UserName, _ipAddress.GetIpAddress());
                if (otpPending == null)
                    return Unauthorized(new { message = "Unable to start OTP challenge." });

                _logger.LogInformation(
                    "ValidateUser OTP queued {@Context}",
                    new { Action = "ValidateUser", UserName = login.UserName, EmployeeId = user.EmployeeId });

                return Ok(otpPending);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ValidateUser failed {@Context}", new { Action = "ValidateUser", UserName = login.UserName });
                throw;
            }
        }

        /// <summary>Step 2: verify email OTP and issue JWT + refresh token.</summary>
        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<ActionResult<EmployeeLogin>> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _otpService.VerifyOtpAndIssueTokensAsync(
                request.UserName,
                request.Otp,
                _ipAddress.GetIpAddress());

            if (result == null)
                return Unauthorized(new { message = "Invalid or expired OTP." });

            return Ok(result);
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
                var tokens = _userService.RefreshTokens(request.RefreshToken, _ipAddress.GetIpAddress());
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _userService.RevokeRefreshToken(request.RefreshToken);
            return Ok(new { message = "Logged out." });
        }
    }
}
