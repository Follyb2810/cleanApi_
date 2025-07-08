using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleanConnect.Infrastructure.Data;
using CleanConnect.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace cleanApi.Extensions
{
    public class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            return services;
        }
    }
}

// ???

// using Microsoft.Extensions.DependencyInjection;
// using YourProject.Modules.User.Interfaces;
// using YourProject.Modules.User.Repositories;
// using YourProject.Common.Interfaces;
// using YourProject.Common.Repositories;

// namespace YourProject.Extensions
// {
//     public static class ServiceCollectionExtensions
//     {
//         public static IServiceCollection AddApplicationServices(this IServiceCollection services)
//         {
//             // Common generic repository
//             services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

//             // Module-specific services
//             services.AddScoped<IUserRepository, UserRepository>();

//             // Add other module services...
//             return services;
//         }
//     }
// }
// builder.Services.AddApplicationServices();
