# Mumii Microservices - SQLite Quick Start Script (PowerShell)
# Chạy tất cả services với SQLite database

Write-Host "🚀 Starting Mumii Microservices with SQLite..." -ForegroundColor Green

# Function to check if port is in use
function Test-Port {
    param([int]$Port)
    
    try {
        $connection = New-Object System.Net.Sockets.TcpClient
        $connection.Connect("localhost", $Port)
        $connection.Close()
        return $true
    }
    catch {
        return $false
    }
}

# Function to start service
function Start-Service {
    param(
        [string]$ServiceName,
        [int]$Port,
        [string]$Path
    )
    
    Write-Host "🔄 Starting $ServiceName on port $Port..." -ForegroundColor Blue
    
    if (Test-Port -Port $Port) {
        Write-Host "❌ Port $Port is already in use" -ForegroundColor Red
        return $false
    }
    
    try {
        Set-Location $Path
        Start-Process -FilePath "dotnet" -ArgumentList "run", "--urls", "http://localhost:$Port" -WindowStyle Hidden
        Write-Host "✅ $ServiceName started successfully" -ForegroundColor Green
        Start-Sleep -Seconds 2
        return $true
    }
    catch {
        Write-Host "❌ Failed to start $ServiceName" -ForegroundColor Red
        return $false
    }
}

# Check if we're in the right directory
if (-not (Test-Path "Mumii.Microservices.sln")) {
    Write-Host "❌ Please run this script from the project root directory" -ForegroundColor Red
    exit 1
}

# Restore packages
Write-Host "📦 Restoring packages..." -ForegroundColor Yellow
dotnet restore

# Start services
Write-Host "🚀 Starting services..." -ForegroundColor Yellow

# Auth Service
Start-Service "Auth Service" 8081 "src/Services/Auth/Mumii.Auth.Api"

# Discovery Service
Start-Service "Discovery Service" 8082 "src/Services/Discovery/Mumii.Discovery.Api"

# Social Service
Start-Service "Social Service" 8083 "src/Services/Social/Mumii.Social.Api"

# AI Service
Start-Service "AI Service" 8084 "src/Services/AI/Mumii.AI.Api"

# API Gateway
Start-Service "API Gateway" 8080 "src/ApiGateway"

# Wait a moment for all services to start
Write-Host "⏳ Waiting for services to initialize..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Check service health
Write-Host "🔍 Checking service health..." -ForegroundColor Blue

$services = @(
    @{Port=8080; Name="API Gateway"},
    @{Port=8081; Name="Auth Service"},
    @{Port=8082; Name="Discovery Service"},
    @{Port=8083; Name="Social Service"},
    @{Port=8084; Name="AI Service"}
)

foreach ($service in $services) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$($service.Port)/health" -TimeoutSec 5 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ $($service.Name) is healthy" -ForegroundColor Green
        } else {
            Write-Host "❌ $($service.Name) is not responding" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "❌ $($service.Name) is not responding" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "🎉 All services started!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Service URLs:" -ForegroundColor Blue
Write-Host "  🌐 API Gateway:    http://localhost:8080" -ForegroundColor White
Write-Host "  🔐 Auth Service:   http://localhost:8081" -ForegroundColor White
Write-Host "  🏪 Discovery:      http://localhost:8082" -ForegroundColor White
Write-Host "  📝 Social Service: http://localhost:8083" -ForegroundColor White
Write-Host "  🤖 AI Service:     http://localhost:8084" -ForegroundColor White
Write-Host ""
Write-Host "📚 Swagger UI:" -ForegroundColor Blue
Write-Host "  🎯 Centralized:    http://localhost:8080" -ForegroundColor White
Write-Host ""
Write-Host "💡 Database Files:" -ForegroundColor Yellow
Write-Host "  📁 auth.db      - Auth Service database" -ForegroundColor White
Write-Host "  📁 discovery.db - Discovery Service database" -ForegroundColor White
Write-Host "  📁 social.db    - Social Service database" -ForegroundColor White
Write-Host ""
Write-Host "🛑 To stop all services:" -ForegroundColor Blue
Write-Host "  Press Ctrl+C or run: .\scripts\stop-sqlite.ps1" -ForegroundColor White
Write-Host ""

# Keep script running
Write-Host "⏳ Services are running... Press Ctrl+C to stop" -ForegroundColor Yellow
Read-Host "Press Enter to continue"
