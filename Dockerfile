# Root Dockerfile for Railway
# Builds and runs the ApiGateway using .NET 8 stable images

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Pin SDK version inside container
COPY ["global.json", "./"]

# Copy solution and project files first for better layer caching
COPY ["Mumii.Microservices.sln", "./"]
COPY ["src/ApiGateway/Mumii.ApiGateway.csproj", "src/ApiGateway/"]

# Restore dependencies
RUN dotnet restore "src/ApiGateway/Mumii.ApiGateway.csproj"

# Copy all source
COPY . .

# Build
WORKDIR "/src/src/ApiGateway"
RUN dotnet build "Mumii.ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Mumii.ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:${PORT:-8080}/health || exit 1

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
ENTRYPOINT ["dotnet", "Mumii.ApiGateway.dll"]


