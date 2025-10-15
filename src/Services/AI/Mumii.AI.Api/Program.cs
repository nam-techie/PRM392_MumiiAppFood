using Mumii.AI.Domain.Interfaces;
using Mumii.AI.Infrastructure.Services;
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
    c.SwaggerDoc("v1", new() { Title = "Mumii AI API", Version = "v1" });
});

// AI Services
builder.Services.AddScoped<IGeminiService, GeminiService>();

// Configuration
builder.Configuration.AddEnvironmentVariables();

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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mumii AI API v1");
        c.RoutePrefix = string.Empty; // Swagger ở root URL
    });
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
    Service = "Mumii AI API", 
    Version = "1.0.0",
    Status = "Running",
    Features = new[]
    {
        "Food Chat with Gemini AI",
        "Mood-based Food Suggestions",
        "Food Image Analysis",
        "Restaurant Recommendations"
    },
    Timestamp = DateTime.UtcNow
});

try
{
    Log.Information("Starting Mumii AI API");
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
