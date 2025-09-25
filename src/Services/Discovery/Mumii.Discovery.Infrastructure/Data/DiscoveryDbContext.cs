using Microsoft.EntityFrameworkCore;
using Mumii.Discovery.Domain.Entities;
using System.Text.Json;

namespace Mumii.Discovery.Infrastructure.Data;

/// <summary>
/// DbContext cho Discovery Service
/// </summary>
public class DiscoveryDbContext : DbContext
{
    public DiscoveryDbContext(DbContextOptions<DiscoveryDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Restaurants table
    /// </summary>
    public DbSet<Restaurant> Restaurants { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Restaurant entity configuration
        modelBuilder.Entity<Restaurant>(entity =>
        {
            // Thiết lập primary key
            entity.HasKey(e => e.Id);
            
            // Thiết lập properties
            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.Address)
                .HasColumnType("TEXT")
                .IsRequired();

            entity.Property(e => e.Latitude)
                .HasColumnType("DECIMAL(10,8)");

            entity.Property(e => e.Longitude)
                .HasColumnType("DECIMAL(11,8)");

            entity.Property(e => e.Region)
                .HasMaxLength(100);

            entity.Property(e => e.AvgPrice)
                .HasColumnType("DECIMAL(10,2)");

            entity.Property(e => e.Rating)
                .HasColumnType("DECIMAL(2,1)")
                .HasDefaultValue(0);

            entity.Property(e => e.Description)
                .HasColumnType("TEXT");

            // JSON columns for MySQL
            entity.Property(e => e.ImageUrls)
                .HasColumnType("JSON")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                );

            entity.Property(e => e.Tags)
                .HasColumnType("JSON")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                );

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Thiết lập indexes
            entity.HasIndex(e => new { e.Latitude, e.Longitude })
                .HasDatabaseName("IX_Restaurants_Location");

            entity.HasIndex(e => e.Region)
                .HasDatabaseName("IX_Restaurants_Region");

            entity.HasIndex(e => e.Rating)
                .HasDatabaseName("IX_Restaurants_Rating");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_Restaurants_CreatedAt");

            entity.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_Restaurants_IsDeleted");

            // Thiết lập table name
            entity.ToTable("restaurants");

            // Ignore domain events property
            entity.Ignore(e => e.DomainEvents);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    /// <summary>
    /// Seed dữ liệu mẫu
    /// </summary>
    private static void SeedData(ModelBuilder modelBuilder)
    {
        var restaurants = new[]
        {
            new
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Phở Hà Nội",
                Address = "123 Phố Cổ, Hoàn Kiếm, Hà Nội",
                Latitude = 21.0285m,
                Longitude = 105.8542m,
                Region = "HaNoi",
                AvgPrice = 50000m,
                Rating = 4.5m,
                Description = "Phở bò truyền thống Hà Nội",
                ImageUrls = "[\"https://example.com/pho1.jpg\"]",
                Tags = "[\"vietnamese\", \"pho\", \"beef\"]",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            },
            new
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Bún Chả Obama",
                Address = "1 Lê Văn Hưu, Hai Bà Trưng, Hà Nội",
                Latitude = 21.0285m,
                Longitude = 105.8542m,
                Region = "HaNoi",
                AvgPrice = 80000m,
                Rating = 4.8m,
                Description = "Bún chả nổi tiếng từ chuyến thăm của Obama",
                ImageUrls = "[\"https://example.com/buncha1.jpg\"]",
                Tags = "[\"vietnamese\", \"buncha\", \"grilled\"]",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            },
            new
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Cơm Tấm Sài Gòn",
                Address = "123 Nguyễn Văn Cừ, Quận 1, TP.HCM",
                Latitude = 10.7769m,
                Longitude = 106.7009m,
                Region = "HoChiMinh",
                AvgPrice = 45000m,
                Rating = 4.3m,
                Description = "Cơm tấm sườn nướng truyền thống",
                ImageUrls = "[\"https://example.com/comtam1.jpg\"]",
                Tags = "[\"vietnamese\", \"rice\", \"grilled\"]",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            },
            new
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Bánh Mì Huynh Hoa",
                Address = "26 Lê Thị Riêng, Quận 1, TP.HCM",
                Latitude = 10.7769m,
                Longitude = 106.7009m,
                Region = "HoChiMinh",
                AvgPrice = 25000m,
                Rating = 4.7m,
                Description = "Bánh mì thập cẩm nổi tiếng",
                ImageUrls = "[\"https://example.com/banhmi1.jpg\"]",
                Tags = "[\"vietnamese\", \"sandwich\", \"street_food\"]",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            },
            new
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Mì Quảng Bà Mua",
                Address = "45 Trần Cao Vân, Đà Nẵng",
                Latitude = 16.0471m,
                Longitude = 108.2068m,
                Region = "DaNang",
                AvgPrice = 35000m,
                Rating = 4.4m,
                Description = "Mì quảng đặc sản Đà Nẵng",
                ImageUrls = "[\"https://example.com/miquang1.jpg\"]",
                Tags = "[\"vietnamese\", \"noodles\", \"seafood\"]",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        };

        modelBuilder.Entity<Restaurant>().HasData(restaurants);
    }
}
