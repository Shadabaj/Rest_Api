using DAL;
using DAL.Entity;
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
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUserRepo, UserRepo>();
            services.AddScoped<IRepository<AuditTrial>, Repository<AuditTrial>>();
            services.AddScoped<IActivityLogRepo, ActivityLogRepo>();

            //Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IService<AuditTrial>, Service<AuditTrial>>();
            services.AddScoped<IActivityLogService, ActivityLogService>();
        }
    }
}
