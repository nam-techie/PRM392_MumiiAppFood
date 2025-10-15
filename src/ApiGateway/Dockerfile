# API Gateway Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

// Copy solution file and global.json to pin SDK
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

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Mumii.ApiGateway.dll"]
