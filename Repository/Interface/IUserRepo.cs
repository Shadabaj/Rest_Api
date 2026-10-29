using DAL.Entity;
using Models;

namespace Repository.Interface
{
    public interface IUserRepo : IRepository<User>
    {
        //bool CreateUser(User user, string roles);
        public string CreateUser(EmployeeLogin employee);
        EmployeeLogin? ValidateUser(string email, string password);
        EmployeeLogin? GetActiveUserById(int userId);
        RefreshToken? GetRefreshTokenByHash(string tokenHash);
        void AddRefreshToken(RefreshToken refreshToken);
        void UpdateRefreshToken(RefreshToken refreshToken);
    }
}

