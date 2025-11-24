using Antifraud.Core.Rules;
using Antifraud.Core.Services;
using Antifraud.Infrastructure.Data;
using Antifraud.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Antifraud.Infrastructure.Injection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAntiFraudInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default");

            services.AddDbContext<AntiFraudDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddScoped<Core.Interface.ITransactionReadRepository, TransactionReadRepository>();

          
            services.AddScoped<IAntiFraudRule, AmountLimitRule>();
            services.AddScoped<IAntiFraudRule, DailyLimitRule>();
           
            services.AddScoped<Core.Interface.IAntiFraudService, AntiFraudService>();

            return services;
        }
    }
}
