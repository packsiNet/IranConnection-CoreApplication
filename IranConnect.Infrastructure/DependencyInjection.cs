using IranConnect.Application.Common.Interfaces;
using IranConnect.Infrastructure.BackgroundServices;
using IranConnect.Infrastructure.Persistence;
using IranConnect.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IranConnect.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(
                    typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IWireGuardService, WireGuardService>();
        services.AddScoped<IVpnConfigService, VpnConfigService>();
        // Seeds the Iranian app catalog into the DB on first run (if empty).
        services.AddHostedService<IranianAppSeeder>();
        // Startup resync runs first so wg0 matches the DB before stats polling.
        services.AddHostedService<WireGuardSyncService>();
        services.AddHostedService<WireGuardStatsService>();
        services.AddHttpClient<ICountryLookupService, HttpCountryLookupService>();

        return services;
    }
}
