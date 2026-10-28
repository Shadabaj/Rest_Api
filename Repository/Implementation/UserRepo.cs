using DAL;
using DAL.Entity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Models;
using Repository.Interface;
using System.Data;

namespace Repository.Implementation
{
    public class UserRepo : Repository<User>, IUserRepo
    {
        private readonly IConfiguration _configuration;
        public UserRepo(AppDbContext db, IConfiguration configuration) : base(db) 
        { 
            _configuration = configuration;
        }

        public string CreateUser(EmployeeLogin employee)
        {
            using (SqlConnection con= new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandText = "dbo.usp_CreateUserAccount";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.Add("@FirstName", SqlDbType.VarChar, 100).Value = employee.FirstName;
                    cmd.Parameters.Add("@LastName", SqlDbType.VarChar, 100).Value = employee.LastName;
                    cmd.Parameters.Add("@DateOfBirth", SqlDbType.DateTime).Value = employee.DateOfBirth;
                    cmd.Parameters.Add("@DateOfJoinee", SqlDbType.DateTime).Value = employee.DateOfJoinee;
                    cmd.Parameters.Add("@PassWord", SqlDbType.NVarChar, 1000).Value = BCrypt.Net.BCrypt.HashPassword(employee.PassWord);
                    cmd.Parameters.Add("@UserRoleId", SqlDbType.Int).Value = employee.RoleId;

                    SqlParameter output= cmd.Parameters.Add("@EmployeeCode", SqlDbType.VarChar, 100);
                    output.Direction = ParameterDirection.Output;

                    con.Open();
                    cmd.ExecuteNonQuery();

                    return output.Value.ToString() ?? string.Empty;


                }
            }
        }

        //public bool CreateUser(User user, string roles)
        //{
        //    try
        //    {
        //        Role? role = _db.Roles.FirstOrDefault(r => r.Name == roles);
        //        if (role == null)
        //            return false;


        //        //user.EmployeeId=
        //        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        //        user.UserRoleUsers.Add(new UserRole
        //        {
        //            RoleId = role.Id,
        //            AssignedAt = DateTime.UtcNow
        //        });

        //        _db.Users.Add(user);
        //        _db.SaveChanges();
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        public EmployeeLogin? ValidateUser(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            var normalizedEmail = email.Trim();

            var user = _db.Users.Include(u => u.Employee).Include(u => u.UserRoleUsers).ThenInclude(ur => ur.Role).FirstOrDefault(u =>
                    u.Employee != null
                    && u.Employee.EmailAddress == normalizedEmail
                    && u.IsActive
                    && u.Employee.IsActive
                    && !u.Employee.IsDeleted);

            if (user == null)
                return null;

            if (user.IsLockedOut && user.LockoutEndAt.HasValue && user.LockoutEndAt.Value > DateTime.UtcNow)
                return null;

            if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return new EmployeeLogin
            {
                UserName = user.UserName,
                EmployeeId = user.EmployeeId,
                Roles = user.UserRoleUsers
                    .Where(ur => ur.Role != null && ur.Role.IsActive)
                    .Select(ur => ur.Role.Name)
                    .ToList()
            };
        }


       
    }
}