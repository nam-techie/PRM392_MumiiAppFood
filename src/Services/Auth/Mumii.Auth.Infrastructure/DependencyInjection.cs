using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mumii.Auth.Domain.Interfaces;
using Mumii.Auth.Infrastructure.Data;
using Mumii.Auth.Infrastructure.Repositories;
using Mumii.Auth.Infrastructure.Services;
using Mumii.Shared.Common.Data;

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
        // MongoDB
        services.AddMongoDb(configuration);

        // MongoDB Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        
        // MongoDB Services
        services.AddScoped<IMongoIdGenerator, MongoIdGenerator>();

        // Database - SQLite (deprecated - keeping for backward compatibility)
        var connectionString = configuration.GetConnectionString("DefaultConnection") ??
            "Data Source=auth.db";

        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        // Repositories (deprecated - will be removed)
        // services.AddScoped<IAccountRepository, AccountRepository>();

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ITokenCacheService, TokenCacheService>();
        
        // Memory Cache for token storage
        services.AddMemoryCache();

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
            // Ensure MongoDB is initialized (collections + indexes)
            await serviceProvider.EnsureMongoInitializedAsync();
        }
        catch (Exception ex)
        {
            // Log lỗi nếu cần
            Console.WriteLine($"Error creating database: {ex.Message}");
            throw;
        }
    }
}
