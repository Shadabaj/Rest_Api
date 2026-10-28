using DAL.Entity;
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

        public LoginController(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        [HttpPost("create")]
        public ActionResult CreateUser([FromBody] EmployeeLogin request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new EmployeeLogin
            {
                //EmployeeId = request.EmployeeId,
                //UserName = request.UserName.Trim(),
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
                return BadRequest(new { message = "Unable to create user. Check EmployeeId, UserName uniqueness, and that the Role exists." });

            return Ok(new { message = "User created successfully.", userName = created });
        }

        //[HttpPost("validate")]
        //public ActionResult<EmployeeLogin> ValidateUser([FromBody] EmployeeLogin login)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var result = _userRepo.ValidateUser(login.Email, login.Password);
        //    if (result == null)
        //        return Unauthorized(new { message = "Invalid email or password." });

        //    return Ok(result);
        //}
    }
}