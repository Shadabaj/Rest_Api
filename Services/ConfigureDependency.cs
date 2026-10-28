using DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository.Implementation;
using Repository.Interface;
using Services.Implementation;
using Services.Interface;

namespace Services
{
    public class ConfigureDependency
    {
        public static void RegisterService(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(option =>
            {
                option.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            //Repository
            services.AddScoped<IUserRepo, UserRepo>();

            //Services
            services.AddScoped<IUserService, UserService>();
        }
    }
}
