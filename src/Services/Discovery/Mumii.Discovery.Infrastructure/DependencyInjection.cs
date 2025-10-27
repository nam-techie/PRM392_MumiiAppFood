using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mumii.Discovery.Domain.Interfaces;
using Mumii.Discovery.Infrastructure.Repositories;
using Mumii.Discovery.Infrastructure.Data;
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
        // Cấu hình MongoDB serialization
        MongoDbConfiguration.ConfigureMongoDb();
        
        // MongoDB
        services.AddMongoDb(configuration);

        // EF DbContext removed; MongoDB only

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
        await Task.CompletedTask;
    }
}
