# Mumii Microservices - Run All Services Script (PowerShell)
# Chạy tất cả services và mở Swagger UI tập trung

Write-Host "🚀 Starting all Mumii Microservices..." -ForegroundColor Green

# Colors for output
$RED = 'Red'
$GREEN = 'Green'
$YELLOW = 'Yellow'
$BLUE = 'Blue'
$CYAN = 'Cyan'

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
    
    Write-Host "🔄 Starting $ServiceName on port $Port..." -ForegroundColor $BLUE
    
    if (Test-Port -Port $Port) {
        Write-Host "❌ Port $Port is already in use" -ForegroundColor $RED
        return $false
    }
    
    try {
        Set-Location $Path
        Start-Process -FilePath "dotnet" -ArgumentList "run", "--urls", "http://localhost:$Port" -WindowStyle Hidden
        Write-Host "✅ $ServiceName started successfully" -ForegroundColor $GREEN
        Start-Sleep -Seconds 3
        return $true
    }
    catch {
        Write-Host "❌ Failed to start $ServiceName" -ForegroundColor $RED
        return $false
    }
}

# Check if we're in the right directory
if (-not (Test-Path "Mumii.Microservices.sln")) {
    Write-Host "❌ Please run this script from the project root directory" -ForegroundColor $RED
    exit 1
}

# Stop any running services first
Write-Host "🛑 Stopping any running services..." -ForegroundColor $YELLOW
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowTitle -like "*Mumii*" -or $_.CommandLine -like "*Mumii*" } | Stop-Process -Force -ErrorAction SilentlyContinue

# Start services in order
Write-Host "🚀 Starting services..." -ForegroundColor $YELLOW

$services = @(
    @{Name="Auth Service"; Port=8081; Path="src/Services/Auth/Mumii.Auth.Api"},
    @{Name="Discovery Service"; Port=8082; Path="src/Services/Discovery/Mumii.Discovery.Api"},
    @{Name="Social Service"; Port=8083; Path="src/Services/Social/Mumii.Social.Api"},
    @{Name="AI Service"; Port=8084; Path="src/Services/AI/Mumii.AI.Api"},
    @{Name="API Gateway"; Port=8080; Path="src/ApiGateway"}
)

$successCount = 0
foreach ($service in $services) {
    if (Start-Service $service.Name $service.Port $service.Path) {
        $successCount++
    }
}

# Wait for all services to start
Write-Host "⏳ Waiting for all services to initialize..." -ForegroundColor $YELLOW
Start-Sleep -Seconds 10

# Check service health
Write-Host "🔍 Checking service health..." -ForegroundColor $BLUE

foreach ($service in $services) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$($service.Port)/health" -TimeoutSec 5 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ $($service.Name) is healthy" -ForegroundColor $GREEN
        } else {
            Write-Host "❌ $($service.Name) is not responding" -ForegroundColor $RED
        }
    }
    catch {
        Write-Host "❌ $($service.Name) is not responding" -ForegroundColor $RED
    }
}

Write-Host ""
Write-Host "🎉 All services started! ($successCount/$($services.Count) successful)" -ForegroundColor $GREEN
Write-Host ""
Write-Host "📋 Service URLs:" -ForegroundColor $BLUE
Write-Host "  🌐 API Gateway:    http://localhost:8080" -ForegroundColor White
Write-Host "  🔐 Auth Service:   http://localhost:8081" -ForegroundColor White
Write-Host "  🏪 Discovery:      http://localhost:8082" -ForegroundColor White
Write-Host "  📝 Social Service: http://localhost:8083" -ForegroundColor White
Write-Host "  🤖 AI Service:     http://localhost:8084" -ForegroundColor White
Write-Host ""
Write-Host "📚 Swagger UI (Centralized):" -ForegroundColor $BLUE
Write-Host "  🎯 http://localhost:8080" -ForegroundColor $CYAN
Write-Host "     ↓ Dropdown để chọn service" -ForegroundColor White
Write-Host ""
Write-Host "💡 Database Files:" -ForegroundColor $YELLOW
Write-Host "  📁 auth.db      - Auth Service database" -ForegroundColor White
Write-Host "  📁 discovery.db - Discovery Service database" -ForegroundColor White
Write-Host "  📁 social.db    - Social Service database" -ForegroundColor White
Write-Host ""
Write-Host "🛑 To stop all services:" -ForegroundColor $BLUE
Write-Host "  Press Ctrl+C or run: .\scripts\stop-sqlite.ps1" -ForegroundColor White
Write-Host ""

# Open Swagger UI in browser
Write-Host "🌐 Opening Swagger UI in browser..." -ForegroundColor $CYAN
Start-Process "http://localhost:8080"

# Keep script running
Write-Host "⏳ Services are running... Press Ctrl+C to stop" -ForegroundColor $YELLOW
Read-Host "Press Enter to continue"
