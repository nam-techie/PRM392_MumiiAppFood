#!/usr/bin/env bash
set -euo pipefail

# Ensure dotnet is available
dotnet --info

# Restore solution packages
dotnet restore Mumii.Microservices.sln

# Publish ApiGateway as entrypoint
dotnet publish src/ApiGateway/Mumii.ApiGateway.csproj -c Release -o /app/out

# Bind to the Railway-provided port (default 8080)
export ASPNETCORE_URLS="http://0.0.0.0:${PORT:-8080}"

exec dotnet /app/out/Mumii.ApiGateway.dll


