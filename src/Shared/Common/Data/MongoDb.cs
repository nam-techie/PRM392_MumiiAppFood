using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Security.Authentication;

namespace Mumii.Shared.Common.Data;

public class MongoDbSettings
{
	public string ConnectionString { get; set; } = string.Empty;
	public string DatabaseName { get; set; } = string.Empty;
}

public static class MongoDbRegistration
{
	public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
	{
		var mongoSection = configuration.GetSection("MongoDB");
		services.Configure<MongoDbSettings>(mongoSection);
        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            var url = new MongoUrl(settings.ConnectionString);
            var clientSettings = MongoClientSettings.FromUrl(url);

            // Force TLS 1.2 and relax revocation checks (common issue on Windows networks)
            clientSettings.SslSettings = new SslSettings
            {
                EnabledSslProtocols = SslProtocols.Tls12,
                CheckCertificateRevocation = false
            };

            // Optional: allow insecure TLS for local dev if explicitly enabled via env
            // DO NOT enable in production
            var allowInsecure = Environment.GetEnvironmentVariable("MONGO_ALLOW_INSECURE_TLS");
            if (!string.IsNullOrWhiteSpace(allowInsecure) &&
                (allowInsecure.Equals("1") || allowInsecure.Equals("true", StringComparison.OrdinalIgnoreCase)))
            {
                clientSettings.AllowInsecureTls = true;
            }

            // Conservative timeouts to avoid long hangs
            clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(20);
            clientSettings.ConnectTimeout = TimeSpan.FromSeconds(15);
            clientSettings.SocketTimeout = TimeSpan.FromSeconds(60);

            return new MongoClient(clientSettings);
        });
		services.AddSingleton<IMongoDatabase>(sp =>
		{
			var cfg = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
			var client = sp.GetRequiredService<IMongoClient>();
			return client.GetDatabase(cfg.DatabaseName);
		});
		return services;
	}
}

public static class MongoDbInitializer
{
	public static async Task EnsureMongoInitializedAsync(this IServiceProvider services)
	{
		using var scope = services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();

			// Ensure collections & indexes for Auth service
		var userCollection = db.GetCollection<MongoDB.Bson.BsonDocument>("users");
		var profileCollection = db.GetCollection<MongoDB.Bson.BsonDocument>("profiles");
		var notificationCollection = db.GetCollection<MongoDB.Bson.BsonDocument>("notifications");

			// Ensure collections for Discovery service
			var restaurants = db.GetCollection<MongoDB.Bson.BsonDocument>("restaurants");
			var restaurantImages = db.GetCollection<MongoDB.Bson.BsonDocument>("restaurant_images");
			var reviews = db.GetCollection<MongoDB.Bson.BsonDocument>("reviews");
			var favorites = db.GetCollection<MongoDB.Bson.BsonDocument>("favorites");

			// Ensure collections for Social service
			var posts = db.GetCollection<MongoDB.Bson.BsonDocument>("posts");
			var moods = db.GetCollection<MongoDB.Bson.BsonDocument>("moods");
			var postMoods = db.GetCollection<MongoDB.Bson.BsonDocument>("post_moods");

		// Create indexes if not exist
			try
		{
			await userCollection.Indexes.CreateOneAsync(
				new CreateIndexModel<MongoDB.Bson.BsonDocument>(
					Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("email"),
					new CreateIndexOptions { Unique = true, Name = "idx_users_email_unique" }
				)
			);

			await profileCollection.Indexes.CreateOneAsync(
				new CreateIndexModel<MongoDB.Bson.BsonDocument>(
					Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("user_id"),
					new CreateIndexOptions { Name = "idx_profiles_user_id" }
				)
			);

			await notificationCollection.Indexes.CreateOneAsync(
				new CreateIndexModel<MongoDB.Bson.BsonDocument>(
					Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("user_id").Ascending("is_read"),
					new CreateIndexOptions { Name = "idx_notifications_user_read" }
				)
			);

				// Discovery: restaurants indexes
				await restaurants.Indexes.CreateManyAsync(new[]
				{
					new CreateIndexModel<MongoDB.Bson.BsonDocument>(
						Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("partner_id"),
						new CreateIndexOptions { Name = "idx_restaurants_partner" }
					),
					new CreateIndexModel<MongoDB.Bson.BsonDocument>(
						Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("status"),
						new CreateIndexOptions { Name = "idx_restaurants_status" }
					),
					new CreateIndexModel<MongoDB.Bson.BsonDocument>(
						Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Descending("rating").Descending("created_at"),
						new CreateIndexOptions { Name = "idx_restaurants_rating_created" }
					)
				});

				// Discovery: restaurant_images
				await restaurantImages.Indexes.CreateOneAsync(
					new CreateIndexModel<MongoDB.Bson.BsonDocument>(
						Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("restaurant_id"),
						new CreateIndexOptions { Name = "idx_restaurant_images_restaurant" }
					)
				);

				// Discovery: reviews
				await reviews.Indexes.CreateManyAsync(new[]
				{
					new CreateIndexModel<MongoDB.Bson.BsonDocument>(
						Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("restaurant_id"),
						new CreateIndexOptions { Name = "idx_reviews_restaurant" }
					),
					new CreateIndexModel<MongoDB.Bson.BsonDocument>(
						Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("user_id"),
						new CreateIndexOptions { Name = "idx_reviews_user" }
					)
				});

				// Discovery: favorites (unique pair user+restaurant)
				await favorites.Indexes.CreateOneAsync(
					new CreateIndexModel<MongoDB.Bson.BsonDocument>(
						Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("user_id").Ascending("restaurant_id"),
						new CreateIndexOptions { Name = "uq_favorites_user_restaurant", Unique = true }
					)
				);

				// Social: posts indexes
				await posts.Indexes.CreateManyAsync(new[]
				{
					new CreateIndexModel<MongoDB.Bson.BsonDocument>(
						Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("partner_id"),
						new CreateIndexOptions { Name = "idx_posts_partner" }
					),
					new CreateIndexModel<MongoDB.Bson.BsonDocument>(
						Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("restaurant_id"),
						new CreateIndexOptions { Name = "idx_posts_restaurant" }
					),
					new CreateIndexModel<MongoDB.Bson.BsonDocument>(
						Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Descending("created_at"),
						new CreateIndexOptions { Name = "idx_posts_created" }
					)
				});

				// Social: moods unique name, post_moods unique pair
				await moods.Indexes.CreateOneAsync(
					new CreateIndexModel<MongoDB.Bson.BsonDocument>(
						Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("name"),
						new CreateIndexOptions { Name = "uq_moods_name", Unique = true }
					)
				);

				await postMoods.Indexes.CreateOneAsync(
					new CreateIndexModel<MongoDB.Bson.BsonDocument>(
						Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("post_id").Ascending("mood_id"),
						new CreateIndexOptions { Name = "uq_post_moods_post_mood", Unique = true }
					)
				);
		}
		catch
		{
			// ignore initialization race
		}
	}
}


