using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using Mumii.Discovery.Domain.Entities;

namespace Mumii.Discovery.Infrastructure.Data;

/// <summary>
/// Cấu hình MongoDB serialization cho Discovery service
/// </summary>
public static class MongoDbConfiguration
{
    /// <summary>
    /// Cấu hình MongoDB conventions và serialization
    /// </summary>
    public static void ConfigureMongoDb()
    {
        // Cấu hình convention để ignore các field không có trong entity
        var conventionPack = new ConventionPack
        {
            new IgnoreExtraElementsConvention(true) // Ignore các field không có trong entity
        };
        
        ConventionRegistry.Register("MumiiDiscoveryConventions", conventionPack, type => true);

        // Cấu hình serialization cho Restaurant entity
        if (!BsonClassMap.IsClassMapRegistered(typeof(Restaurant)))
        {
            BsonClassMap.RegisterClassMap<Restaurant>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true); // Ignore các field không có trong entity
            });
        }
    }
}
