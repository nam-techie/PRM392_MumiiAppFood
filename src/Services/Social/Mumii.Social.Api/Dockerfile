# Social Service Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY ["Mumii.Microservices.sln", "./"]

# Copy project files
COPY ["src/Shared/Common/Mumii.Shared.Common.csproj", "src/Shared/Common/"]
COPY ["src/Services/Social/Mumii.Social.Domain/Mumii.Social.Domain.csproj", "src/Services/Social/Mumii.Social.Domain/"]
COPY ["src/Services/Social/Mumii.Social.Infrastructure/Mumii.Social.Infrastructure.csproj", "src/Services/Social/Mumii.Social.Infrastructure/"]
COPY ["src/Services/Social/Mumii.Social.Api/Mumii.Social.Api.csproj", "src/Services/Social/Mumii.Social.Api/"]

# Restore dependencies
RUN dotnet restore "src/Services/Social/Mumii.Social.Api/Mumii.Social.Api.csproj"

# Copy all source code
COPY . .

# Build application
WORKDIR "/src/src/Services/Social/Mumii.Social.Api"
RUN dotnet build "Mumii.Social.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Mumii.Social.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Mumii.Social.Api.dll"]
