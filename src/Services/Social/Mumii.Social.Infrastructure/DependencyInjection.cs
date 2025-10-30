using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mumii.Social.Domain.Interfaces;
using Mumii.Social.Infrastructure.Repositories;
using Mumii.Shared.Common.Data;

namespace Mumii.Social.Infrastructure;

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

        // EF DbContext removed; MongoDB only

        // Repositories (Mongo)
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IMoodRepository, MoodRepository>();
        services.AddScoped<IPostMoodRepository, PostMoodRepository>();
        services.AddHttpClient<IUserRepository, UserRepository>();
        services.AddHttpClient<IRestaurantRepository, RestaurantRepository>();

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
