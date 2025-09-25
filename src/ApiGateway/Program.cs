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
    // Load Swagger JSON trực tiếp từ các services
    c.SwaggerEndpoint("http://localhost:8081/swagger/v1/swagger.json", "Auth API v1");
    c.SwaggerEndpoint("http://localhost:8082/swagger/v1/swagger.json", "Discovery API v1");
    c.SwaggerEndpoint("http://localhost:8083/swagger/v1/swagger.json", "Social API v1");
    c.SwaggerEndpoint("http://localhost:8084/swagger/v1/swagger.json", "AI API v1");
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
