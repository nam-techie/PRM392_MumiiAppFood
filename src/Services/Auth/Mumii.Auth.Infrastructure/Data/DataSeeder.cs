using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mumii.Auth.Domain.Entities;
using Mumii.Auth.Domain.Interfaces;
using Mumii.Auth.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace Mumii.Auth.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAdminUsersAsync(IHost app)
    {
        // Tạo một scope để lấy các services
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        
        try
        {
            var userRepository = services.GetRequiredService<IUserRepository>();
            var idGenerator = services.GetRequiredService<IMongoIdGenerator>();
            var logger = services.GetRequiredService<ILogger<IHost>>();

            logger.LogInformation("Start seeding admin users...");

            // --- TÀI KHOẢN ADMIN 1 ---
            await CreateAdminIfNotExists(
                userRepository, 
                idGenerator, 
                logger,
                email: "admin1@mumii.app",
                password: "AdminPassword1!",
                fullname: "Super Admin 1"
            );

            // --- TÀI KHOẢN ADMIN 2 ---
            await CreateAdminIfNotExists(
                userRepository, 
                idGenerator, 
                logger,
                email: "admin2@mumii.app",
                password: "AdminPassword2!",
                fullname: "Super Admin 2"
            );

            // --- TÀI KHOẢN ADMIN 3 ---
            await CreateAdminIfNotExists(
                userRepository, 
                idGenerator, 
                logger,
                email: "admin3@mumii.app",
                password: "AdminPassword3!",
                fullname: "Super Admin 3"
            );

            logger.LogInformation("Finished seeding admin users.");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<IHost>>();
            logger.LogError(ex, "An error occurred while seeding the admin users.");
        }
    }

    private static async Task CreateAdminIfNotExists(
        IUserRepository userRepository, 
        IMongoIdGenerator idGenerator, 
        ILogger<IHost> logger,
        string email, 
        string password, 
        string fullname)
    {
        // 1. Kiểm tra xem admin đã tồn tại chưa
        var existingAdmin = await userRepository.GetByEmailAsync(email);
        if (existingAdmin == null)
        {
            // 2. Nếu chưa, tạo tài khoản mới
            var newId = await idGenerator.GetNextIdAsync("users");
            var adminUser = User.CreateWithEmail(newId, email, password, fullname);
            
            // 3. Set Role là "Admin"
            adminUser.SetRole("Admin");

            // 4. Thêm vào database
            await userRepository.AddAsync(adminUser);
            logger.LogInformation("Created new admin user: {Email}", email);
        }
        else
        {
            logger.LogInformation("Admin user {Email} already exists. Skipping.", email);
        }
    }
}
