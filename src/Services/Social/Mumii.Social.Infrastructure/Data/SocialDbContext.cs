using Microsoft.EntityFrameworkCore;
using Mumii.Social.Domain.Entities;
using System.Text.Json;

namespace Mumii.Social.Infrastructure.Data;

/// <summary>
/// DbContext cho Social Service
/// </summary>
public class SocialDbContext : DbContext
{
    public SocialDbContext(DbContextOptions<SocialDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Posts table
    /// </summary>
    public DbSet<Post> Posts { get; set; } = null!;

    /// <summary>
    /// Comments table
    /// </summary>
    public DbSet<Comment> Comments { get; set; } = null!;

    /// <summary>
    /// Reactions table
    /// </summary>
    public DbSet<Reaction> Reactions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Post entity configuration
        modelBuilder.Entity<Post>(entity =>
        {
            // Thiết lập primary key
            entity.HasKey(e => e.Id);
            
            // Thiết lập properties
            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(e => e.AccountId)
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(e => e.Content)
                .HasColumnType("TEXT")
                .IsRequired();

            entity.Property(e => e.Mood)
                .HasMaxLength(50);

            entity.Property(e => e.RestaurantId)
                .HasMaxLength(36);

            // JSON column for image URLs
            entity.Property(e => e.ImageUrls)
                .HasColumnType("JSON")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                );

            entity.Property(e => e.ReactionCount)
                .HasDefaultValue(0);

            entity.Property(e => e.CommentCount)
                .HasDefaultValue(0);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Thiết lập relationships
            entity.HasMany(e => e.Comments)
                .WithOne(e => e.Post)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Reactions)
                .WithOne(e => e.Post)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Thiết lập indexes
            entity.HasIndex(e => e.AccountId)
                .HasDatabaseName("IX_Posts_AccountId");

            entity.HasIndex(e => e.RestaurantId)
                .HasDatabaseName("IX_Posts_RestaurantId");

            entity.HasIndex(e => e.Mood)
                .HasDatabaseName("IX_Posts_Mood");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_Posts_CreatedAt");

            entity.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_Posts_IsDeleted");

            // Thiết lập table name
            entity.ToTable("posts");

            // Ignore domain events property
            entity.Ignore(e => e.DomainEvents);
        });

        // Comment entity configuration
        modelBuilder.Entity<Comment>(entity =>
        {
            // Thiết lập primary key
            entity.HasKey(e => e.Id);
            
            // Thiết lập properties
            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(e => e.PostId)
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(e => e.AccountId)
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(e => e.Content)
                .HasColumnType("TEXT")
                .IsRequired();

            entity.Property(e => e.ParentCommentId)
                .HasMaxLength(36);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Thiết lập relationships
            entity.HasOne(e => e.ParentComment)
                .WithMany(e => e.Replies)
                .HasForeignKey(e => e.ParentCommentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Thiết lập indexes
            entity.HasIndex(e => e.PostId)
                .HasDatabaseName("IX_Comments_PostId");

            entity.HasIndex(e => e.AccountId)
                .HasDatabaseName("IX_Comments_AccountId");

            entity.HasIndex(e => e.ParentCommentId)
                .HasDatabaseName("IX_Comments_ParentCommentId");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_Comments_CreatedAt");

            // Thiết lập table name
            entity.ToTable("comments");
        });

        // Reaction entity configuration
        modelBuilder.Entity<Reaction>(entity =>
        {
            // Thiết lập primary key
            entity.HasKey(e => e.Id);
            
            // Thiết lập properties
            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(e => e.PostId)
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(e => e.AccountId)
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Thiết lập unique constraint
            entity.HasIndex(e => new { e.PostId, e.AccountId })
                .IsUnique()
                .HasDatabaseName("UX_Reactions_PostId_AccountId");

            // Thiết lập indexes
            entity.HasIndex(e => e.PostId)
                .HasDatabaseName("IX_Reactions_PostId");

            entity.HasIndex(e => e.AccountId)
                .HasDatabaseName("IX_Reactions_AccountId");

            entity.HasIndex(e => e.Type)
                .HasDatabaseName("IX_Reactions_Type");

            // Thiết lập table name
            entity.ToTable("reactions");
        });
    }
}
