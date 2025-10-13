using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mumii.Discovery.Domain.Interfaces;
using Mumii.Discovery.Infrastructure.Data;
using Mumii.Discovery.Infrastructure.Repositories;
using Mumii.Shared.Common.Data;

namespace Mumii.Discovery.Infrastructure;

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

        // Database - SQLite
        var connectionString = configuration.GetConnectionString("DefaultConnection") ??
            "Data Source=discovery.db";

        services.AddDbContext<DiscoveryDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        // Repositories (Mongo)
        services.AddScoped<IRestaurantRepository, RestaurantRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();

        return services;
    }

    /// <summary>
    /// Ensure database is created
    /// </summary>
    public static async Task EnsureDatabaseCreatedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DiscoveryDbContext>();
        
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
