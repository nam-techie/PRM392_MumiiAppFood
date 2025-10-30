# API Gateway Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Install curl for container healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file and global.json to pin SDK
COPY ["global.json", "./"]
COPY ["Mumii.Microservices.sln", "./"]

# Copy project files
COPY ["src/ApiGateway/Mumii.ApiGateway.csproj", "src/ApiGateway/"]

# Restore dependencies
RUN dotnet restore "src/ApiGateway/Mumii.ApiGateway.csproj"

# Copy all source code
COPY . .

# Build application
WORKDIR "/src/src/ApiGateway"
RUN dotnet build "Mumii.ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Mumii.ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Ensure gateway listens on Render-provided $PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# Health check (fallback to 8080 when running locally)
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:${PORT:-8080}/health || exit 1

ENTRYPOINT ["dotnet", "Mumii.ApiGateway.dll"]
