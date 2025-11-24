using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction.Core.Interface;
using Transaction.Infrastructure.Data;
using Transaction.Infrastructure.Repositories;

namespace Transaction.Infrastructure.Injection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTransactionInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default");

            services.AddDbContext<TransactionDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddScoped<ITransactionRepository, TransactionRepository>();

            return services;
        }
    }
}
