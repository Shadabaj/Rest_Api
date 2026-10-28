using Microsoft.AspNetCore.Mvc;
using Models;
using Repository.Interface;

namespace Rest_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IUserRepo _userRepo;
        private readonly ILogger<LoginController> _logger;

        public LoginController(IUserRepo userRepo, ILogger<LoginController> logger)
        {
            _userRepo = userRepo;
            _logger = logger;
        }

        [HttpPost("create")]
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

                var created = _userRepo.CreateUser(user);
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
                // Cogitate-style: error first, then message with context
                _logger.LogError(
                    ex,
                    "CreateUser failed {@Context}",
                    new { Action = "CreateUser", RoleId = request.RoleId });
                throw;
            }
        }

        [HttpPost("validate")]
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
                var result = _userRepo.ValidateUser(login.UserName, login.Password);
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
                _logger.LogError(ex,"ValidateUser failed {@Context}",new { Action = "ValidateUser", UserName = login.UserName });
                throw;
            }
        }


        //[HttpGet("get-value")]
        //public ActionResult<int> GetValue()
        //{
        //    int a = 10;int b = 0;
        //    return Ok(a/b);
        //}
    }
}