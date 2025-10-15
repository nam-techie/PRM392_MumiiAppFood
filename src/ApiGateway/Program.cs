using Serilog;
using DotNetEnv;

// Load .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add YARP services
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Mumii API Gateway",
        Version = "v1",
        Description = "Centralized API Gateway for Mumii Microservices"
    });
});

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
app.UseSerilogRequestLogging();

app.UseCors("AllowAll");

// Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    var isDev = app.Environment.IsDevelopment();

    // In production, read public HTTPS URLs from environment variables
    // Set on Railway: AUTH_URL, DISCOVERY_URL, SOCIAL_URL, AI_URL (e.g. https://auth.yourapp.railway.app)
    string? authUrl = Environment.GetEnvironmentVariable("AUTH_URL");
    string? discoveryUrl = Environment.GetEnvironmentVariable("DISCOVERY_URL");
    string? socialUrl = Environment.GetEnvironmentVariable("SOCIAL_URL");
    string? aiUrl = Environment.GetEnvironmentVariable("AI_URL");

    if (isDev)
    {
        c.SwaggerEndpoint("http://localhost:8081/swagger/v1/swagger.json", "Auth API v1");
        c.SwaggerEndpoint("http://localhost:8082/swagger/v1/swagger.json", "Discovery API v1");
        c.SwaggerEndpoint("http://localhost:8083/swagger/v1/swagger.json", "Social API v1");
        c.SwaggerEndpoint("http://localhost:8084/swagger/v1/swagger.json", "AI API v1");
    }
    else
    {
        // Single-domain mode: proxy swagger through gateway paths if no external URLs supplied
        if (string.IsNullOrWhiteSpace(authUrl))
            c.SwaggerEndpoint("/swagger/auth/v1/swagger.json", "Auth API v1");
        else
            c.SwaggerEndpoint($"{authUrl.TrimEnd('/')}/swagger/v1/swagger.json", "Auth API v1");

        if (string.IsNullOrWhiteSpace(discoveryUrl))
            c.SwaggerEndpoint("/swagger/discovery/v1/swagger.json", "Discovery API v1");
        else
            c.SwaggerEndpoint($"{discoveryUrl.TrimEnd('/')}/swagger/v1/swagger.json", "Discovery API v1");

        if (string.IsNullOrWhiteSpace(socialUrl))
            c.SwaggerEndpoint("/swagger/social/v1/swagger.json", "Social API v1");
        else
            c.SwaggerEndpoint($"{socialUrl.TrimEnd('/')}/swagger/v1/swagger.json", "Social API v1");

        if (string.IsNullOrWhiteSpace(aiUrl))
            c.SwaggerEndpoint("/swagger/ai/v1/swagger.json", "AI API v1");
        else
            c.SwaggerEndpoint($"{aiUrl.TrimEnd('/')}/swagger/v1/swagger.json", "AI API v1");
    }

    c.RoutePrefix = string.Empty; // Swagger UI tại root "/"
    c.DocumentTitle = "Mumii API Gateway - Swagger UI";
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
    c.EnableFilter();
    c.ShowExtensions();
});

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

// Root endpoint
app.MapGet("/", () => new { 
    Service = "Mumii API Gateway", 
    Version = "1.0.0",
    Status = "Running",
    Timestamp = DateTime.UtcNow,
    Routes = new
    {
        Auth = "/api/auth/*",
        Discovery = "/api/restaurants/*",
        Social = "/api/posts/*",
        AI = "/api/chat/*"
    },
    SwaggerUI = "Available at root path with multiple service definitions"
});

// Map reverse proxy
app.MapReverseProxy();

try
{
    Log.Information("Starting Mumii API Gateway");
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
