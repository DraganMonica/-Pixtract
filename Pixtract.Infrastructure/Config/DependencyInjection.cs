using Microsoft.Extensions.DependencyInjection;
using Pixtract.Application.Interfaces;
using Pixtract.Infrastructure.Services;

namespace Pixtract.Infrastructure.Config;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        
        services.AddScoped<IAiService, AiService>();
        services.AddScoped<IExtractionService, ExtractionService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IUsageService, UsageService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddHostedService<PlanExpirationService>();

        return services;
    }
}