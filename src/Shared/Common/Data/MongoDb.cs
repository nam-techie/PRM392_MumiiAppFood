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
		}
		catch
		{
			// ignore initialization race
		}
	}
}


