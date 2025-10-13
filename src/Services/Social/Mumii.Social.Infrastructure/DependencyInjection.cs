using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mumii.Social.Domain.Interfaces;
using Mumii.Social.Infrastructure.Data;
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

        // Database - SQLite
        var connectionString = configuration.GetConnectionString("DefaultConnection") ??
            "Data Source=social.db";

        services.AddDbContext<SocialDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        // Repositories
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();

        return services;
    }

    /// <summary>
    /// Ensure database is created
    /// </summary>
    public static async Task EnsureDatabaseCreatedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SocialDbContext>();
        
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
