using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Mumii.Auth.Infrastructure;
using Serilog;
using DotNetEnv;
using Mumii.Auth.Domain.Interfaces;     
using Mumii.Auth.Infrastructure.Services;
using Mumii.Auth.Infrastructure.Settings;

// Load .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Allow environment variables to override configuration (Railway variables)
builder.Configuration.AddEnvironmentVariables();

// Cấu hình Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container
builder.Services.AddControllers();

// Đăng ký dịch vụ cache trong bộ nhớ (cần thiết cho TokenCacheService)
builder.Services.AddMemoryCache();

// Đăng ký ITokenCacheService với triển khai TokenCacheService
builder.Services.AddScoped<ITokenCacheService, TokenCacheService>();

// Cấu hình MailSettings từ appsettings.json
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

// Đăng ký Email Service
builder.Services.AddScoped<IEmailService, SendGridEmailService>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Mumii Auth API",
        Version = "v1",
        Description = "ASP.NET Core Web API with JWT Bearer Authentication"
    });
    
    // JWT Bearer authorization trong Swagger - theo chuẩn OpenAPI
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. " +
                     "Enter 'Bearer' [space] and then your token in the text input below.\\n\\n" +
                     "Example: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// JWT Authentication - chỉ từ environment variable
var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
             builder.Configuration["Jwt:Key"];

if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT_SECRET_KEY là bắt buộc! Vui lòng cấu hình trong file .env hoặc environment variables");
}

// Validate key length for security
if (jwtKey.Length < 32)
{
    throw new InvalidOperationException("JWT Secret Key phải có ít nhất 32 ký tự để đảm bảo bảo mật");
}

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "Mumii",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "Mumii.Client",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// CORS cấu hình theo biến môi trường CORS__AllowedOrigins (phân tách bằng dấu phẩy)
var allowedOrigins = (Environment.GetEnvironmentVariable("CORS__AllowedOrigins") ?? string.Empty)
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
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
    // Set swagger server URL để Try it out đi qua API Gateway
    app.UseSwagger(c =>
    {
        c.PreSerializeFilters.Add((swagger, httpReq) =>
        {
            var scheme = httpReq.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? httpReq.Scheme;
            var host = httpReq.Headers["X-Forwarded-Host"].FirstOrDefault() ?? httpReq.Host.Value;
            var basePath = Environment.GetEnvironmentVariable("SWAGGER_BASE_PATH") ?? string.Empty;
            swagger.Servers = new List<Microsoft.OpenApi.Models.OpenApiServer>
            {
                new() { Url = $"{scheme}://{host}{basePath}" }
            };
        });
    });

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mumii Auth API v1");
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

app.UseCors("default");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

// Root endpoint
app.MapGet("/", () => new { 
    Service = "Mumii Auth API", 
    Version = "1.0.0",
    Status = "Running",
    Timestamp = DateTime.UtcNow
});

try
{
    Log.Information("Starting Mumii Auth API");
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
