using Mumii.Social.Infrastructure;
using Serilog;
using DotNetEnv;

// Load .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Mumii Social API", Version = "v1" });
});

// Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
var enableSwagger = app.Environment.IsDevelopment() ||
                    string.Equals(Environment.GetEnvironmentVariable("SWAGGER_ENABLED"), "true", StringComparison.OrdinalIgnoreCase);

if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mumii Social API v1");
        c.RoutePrefix = string.Empty; // Swagger ở root URL
    });
}

// Ensure database is created
try
{
    await app.Services.EnsureDatabaseCreatedAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An error occurred while creating the database");
    throw;
}

app.UseSerilogRequestLogging();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

// Root endpoint
app.MapGet("/", () => new { 
    Service = "Mumii Social API", 
    Version = "1.0.0",
    Status = "Running",
    Timestamp = DateTime.UtcNow
});

try
{
    Log.Information("Starting Mumii Social API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
