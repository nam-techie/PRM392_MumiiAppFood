# Auth Service Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY ["Mumii.Microservices.sln", "./"]

# Copy project files
COPY ["src/Shared/Common/Mumii.Shared.Common.csproj", "src/Shared/Common/"]
COPY ["src/Services/Auth/Mumii.Auth.Domain/Mumii.Auth.Domain.csproj", "src/Services/Auth/Mumii.Auth.Domain/"]
COPY ["src/Services/Auth/Mumii.Auth.Infrastructure/Mumii.Auth.Infrastructure.csproj", "src/Services/Auth/Mumii.Auth.Infrastructure/"]
COPY ["src/Services/Auth/Mumii.Auth.Api/Mumii.Auth.Api.csproj", "src/Services/Auth/Mumii.Auth.Api/"]

# Restore dependencies
RUN dotnet restore "src/Services/Auth/Mumii.Auth.Api/Mumii.Auth.Api.csproj"

# Copy all source code
COPY . .

# Build application
WORKDIR "/src/src/Services/Auth/Mumii.Auth.Api"
RUN dotnet build "Mumii.Auth.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Mumii.Auth.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8081/health || exit 1

ENTRYPOINT ["dotnet", "Mumii.Auth.Api.dll"]
