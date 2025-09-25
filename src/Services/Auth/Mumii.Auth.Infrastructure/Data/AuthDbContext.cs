using Microsoft.EntityFrameworkCore;
using Mumii.Auth.Domain.Entities;
using Mumii.Shared.Common.Enums;

namespace Mumii.Auth.Infrastructure.Data;

/// <summary>
/// DbContext cho Auth Service
/// </summary>
public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Accounts table
    /// </summary>
    public DbSet<Account> Accounts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Account entity configuration
        modelBuilder.Entity<Account>(entity =>
        {
            // Thiết lập primary key
            entity.HasKey(e => e.Id);
            
            // Thiết lập properties
            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.DisplayName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500);

            entity.Property(e => e.Role)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue(UserRole.User);

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Thiết lập indexes
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Accounts_Email");

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_Accounts_IsActive");

            // Thiết lập table name
            entity.ToTable("accounts");

            // Ignore domain events property
            entity.Ignore(e => e.DomainEvents);
        });

        // Seed admin account
        SeedData(modelBuilder);
    }

    /// <summary>
    /// Seed dữ liệu mặc định
    /// </summary>
    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Tạo admin account mặc định
        var adminId = Guid.NewGuid().ToString();
        var adminAccount = new
        {
            Id = adminId,
            Email = "admin@mumii.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), // Password: admin123
            DisplayName = "Admin",
            AvatarUrl = (string?)null,
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        modelBuilder.Entity<Account>().HasData(adminAccount);
    }
}
