using DAL.Entity;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IUserService
    {
        string CreateUser(EmployeeLogin user);
        EmployeeLogin? ValidateUser(string email, string password);

    }
}
