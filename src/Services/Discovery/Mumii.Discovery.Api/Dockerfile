# Discovery Service Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8082

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY ["Mumii.Microservices.sln", "./"]

# Copy project files
COPY ["src/Shared/Common/Mumii.Shared.Common.csproj", "src/Shared/Common/"]
COPY ["src/Services/Discovery/Mumii.Discovery.Domain/Mumii.Discovery.Domain.csproj", "src/Services/Discovery/Mumii.Discovery.Domain/"]
COPY ["src/Services/Discovery/Mumii.Discovery.Infrastructure/Mumii.Discovery.Infrastructure.csproj", "src/Services/Discovery/Mumii.Discovery.Infrastructure/"]
COPY ["src/Services/Discovery/Mumii.Discovery.Api/Mumii.Discovery.Api.csproj", "src/Services/Discovery/Mumii.Discovery.Api/"]

# Restore dependencies
RUN dotnet restore "src/Services/Discovery/Mumii.Discovery.Api/Mumii.Discovery.Api.csproj"

# Copy all source code
COPY . .

# Build application
WORKDIR "/src/src/Services/Discovery/Mumii.Discovery.Api"
RUN dotnet build "Mumii.Discovery.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Mumii.Discovery.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Health check
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:${PORT:-8082}/health || exit 1

ENTRYPOINT ["dotnet", "Mumii.Discovery.Api.dll"]
