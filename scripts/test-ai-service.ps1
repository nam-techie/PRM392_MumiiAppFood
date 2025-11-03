# Mumii AI Service - Test Script (PowerShell)
# Test AI Service với Gemini API

Write-Host "🤖 Testing Mumii AI Service..." -ForegroundColor Yellow

# Check if we're in the right directory
if (-not (Test-Path "Mumii.Microservices.sln")) {
    Write-Host "❌ Please run this script from the project root directory" -ForegroundColor Red
    exit 1
}

# Check if .env file exists
if (-not (Test-Path ".env")) {
    Write-Host "⚠️ .env file not found. Creating from template..." -ForegroundColor Yellow
    Copy-Item "env.example" ".env" -ErrorAction SilentlyContinue
    Write-Host "📝 Please edit .env file and add your GEMINI_API_KEY" -ForegroundColor Blue
    Write-Host "   Get API key from: https://makersuite.google.com/app/apikey" -ForegroundColor Blue
    exit 1
}

# Check if GEMINI_API_KEY is set
$envContent = Get-Content ".env" -Raw
if ($envContent -notmatch "GEMINI_API_KEY=" -or $envContent -match "GEMINI_API_KEY=your_gemini_api_key_here") {
    Write-Host "⚠️ GEMINI_API_KEY not configured in .env file" -ForegroundColor Yellow
    Write-Host "📝 Please edit .env file and add your GEMINI_API_KEY" -ForegroundColor Blue
    Write-Host "   Get API key from: https://makersuite.google.com/app/apikey" -ForegroundColor Blue
    exit 1
}

# Start AI Service
Write-Host "🚀 Starting AI Service..." -ForegroundColor Blue
Set-Location "src/Services/AI/Mumii.AI.Api"
Start-Process -FilePath "dotnet" -ArgumentList "run", "--urls", "http://localhost:8084" -WindowStyle Hidden

# Wait for service to start
Write-Host "⏳ Waiting for AI Service to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Test AI Service
Write-Host "🧪 Testing AI Service endpoints..." -ForegroundColor Blue

# Test 1: Health check
Write-Host "1️⃣ Testing health check..." -ForegroundColor Cyan
try {
    $healthResponse = Invoke-WebRequest -Uri "http://localhost:8084/health" -TimeoutSec 10 -UseBasicParsing
    if ($healthResponse.StatusCode -eq 200) {
        Write-Host "✅ Health check passed" -ForegroundColor Green
    } else {
        Write-Host "❌ Health check failed" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Health check failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Root endpoint
Write-Host "2️⃣ Testing root endpoint..." -ForegroundColor Cyan
try {
    $rootResponse = Invoke-WebRequest -Uri "http://localhost:8084/" -TimeoutSec 10 -UseBasicParsing
    if ($rootResponse.StatusCode -eq 200) {
        Write-Host "✅ Root endpoint working" -ForegroundColor Green
        $rootData = $rootResponse.Content | ConvertFrom-Json
        Write-Host "   Service: $($rootData.Service)" -ForegroundColor White
        Write-Host "   Version: $($rootData.Version)" -ForegroundColor White
        Write-Host "   Status: $($rootData.Status)" -ForegroundColor White
    } else {
        Write-Host "❌ Root endpoint failed" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Root endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Food chat
Write-Host "3️⃣ Testing food chat..." -ForegroundColor Cyan
try {
    $chatBody = @{
        message = "Hôm nay tôi muốn ăn gì đó ngon và healthy"
    } | ConvertTo-Json

    $chatResponse = Invoke-WebRequest -Uri "http://localhost:8084/api/Chat/food" -Method POST -Body $chatBody -ContentType "application/json" -TimeoutSec 30 -UseBasicParsing
    if ($chatResponse.StatusCode -eq 200) {
        Write-Host "✅ Food chat working" -ForegroundColor Green
        $chatData = $chatResponse.Content | ConvertFrom-Json
        Write-Host "   Response: $($chatData)" -ForegroundColor White
    } else {
        Write-Host "❌ Food chat failed" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Food chat failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Mood suggestion
Write-Host "4️⃣ Testing mood suggestion..." -ForegroundColor Cyan
try {
    $moodBody = @{
        mood = "HAPPY"
        location = "Hà Nội"
    } | ConvertTo-Json

    $moodResponse = Invoke-WebRequest -Uri "http://localhost:8084/api/Chat/suggest-by-mood" -Method POST -Body $moodBody -ContentType "application/json" -TimeoutSec 30 -UseBasicParsing
    if ($moodResponse.StatusCode -eq 200) {
        Write-Host "✅ Mood suggestion working" -ForegroundColor Green
        $moodData = $moodResponse.Content | ConvertFrom-Json
        Write-Host "   Response: $($moodData)" -ForegroundColor White
    } else {
        Write-Host "❌ Mood suggestion failed" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Mood suggestion failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "🎉 AI Service testing completed!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Service URLs:" -ForegroundColor Blue
Write-Host "  🤖 AI Service: http://localhost:8084" -ForegroundColor White
Write-Host "  📚 Swagger UI: http://localhost:8084/swagger" -ForegroundColor White
Write-Host ""
Write-Host "🛑 To stop AI Service:" -ForegroundColor Blue
Write-Host "  Press Ctrl+C or close the terminal" -ForegroundColor White
