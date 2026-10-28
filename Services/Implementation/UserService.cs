using DAL.Entity;
using Microsoft.Extensions.Configuration;
using Models;
using Repository.Interface;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class UserService :IUserService
    {
        private readonly IUserRepo _UserRepo;
        protected IConfiguration _config;

        public UserService(IUserRepo userRepo)
        {
            _UserRepo = userRepo;
        }

        public string CreateUser(EmployeeLogin user)
        {
            return _UserRepo.CreateUser(user);
        }

        public EmployeeLogin? ValidateUser(string email, string password)
        {
            return _UserRepo.ValidateUser(email, password);
        }

        //private string GenerateJsonWebToken(EmployeeLogin user)
        //{
           
        //}
    }
}
