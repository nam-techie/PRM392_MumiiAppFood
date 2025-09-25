using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mumii.Auth.Domain.Interfaces;
using Mumii.Auth.Infrastructure.Data;
using Mumii.Auth.Infrastructure.Repositories;
using Mumii.Auth.Infrastructure.Services;

namespace Mumii.Auth.Infrastructure;

/// <summary>
/// Extension methods để đăng ký Infrastructure services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký Infrastructure services
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Database - SQLite
        var connectionString = configuration.GetConnectionString("DefaultConnection") ??
            "Data Source=auth.db";

        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        // Repositories
        services.AddScoped<IAccountRepository, AccountRepository>();

        // Services
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }

    /// <summary>
    /// Ensure database is created
    /// </summary>
    public static async Task EnsureDatabaseCreatedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        
        try
        {
            await context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            // Log lỗi nếu cần
            Console.WriteLine($"Error creating database: {ex.Message}");
            throw;
        }
    }
}
