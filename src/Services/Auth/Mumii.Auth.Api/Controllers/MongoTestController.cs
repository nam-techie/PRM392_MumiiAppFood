using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mumii.Auth.Api.Controllers;

[ApiController]
[Route("api/mongo")] 
public class MongoTestController : ControllerBase
{
	private readonly IMongoDatabase _database;

	public MongoTestController(IMongoDatabase database)
	{
		_database = database;
	}

		[HttpGet("ping")]
		public async Task<IActionResult> Ping()
		{
			try
			{
				// Ping the admin database to verify connectivity and auth
				var command = new BsonDocument("ping", 1);
				await _database.RunCommandAsync<BsonDocument>(command);
				return Ok(new { ok = true });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { ok = false, error = ex.Message });
			}
		}

	[HttpPost("seed-user")]
	public async Task<IActionResult> SeedUser()
	{
		var users = _database.GetCollection<BsonDocument>("users");
		var doc = new BsonDocument
		{
			{ "email", "test@mumii.com" },
			{ "password", "hashed_password" },
			{ "fullname", "Test User" },
			{ "role", "User" },
			{ "is_active", true },
			{ "created_at", DateTime.UtcNow },
			{ "updated_at", DateTime.UtcNow }
		};
		await users.InsertOneAsync(doc);
		return Ok(new { insertedId = doc["_id"].ToString() });
	}

	[HttpGet("users")] 
	public async Task<IActionResult> GetUsers()
	{
		var users = _database.GetCollection<BsonDocument>("users");
		var list = await users.Find(FilterDefinition<BsonDocument>.Empty)
			.Sort(Builders<BsonDocument>.Sort.Descending("created_at"))
			.Limit(20)
			.ToListAsync();
		return Ok(list.Select(x => BsonTypeMapper.MapToDotNetValue(x.ToBsonDocument())));
	}
}


