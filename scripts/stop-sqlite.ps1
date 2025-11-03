# Mumii Microservices - Stop SQLite Services Script (PowerShell)
# Dừng tất cả services đang chạy

Write-Host "🛑 Stopping Mumii Microservices..." -ForegroundColor Red

# Function to stop processes by port
function Stop-ProcessByPort {
    param([int]$Port)
    
    try {
        $processes = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess
        if ($processes) {
            foreach ($pid in $processes) {
                $process = Get-Process -Id $pid -ErrorAction SilentlyContinue
                if ($process) {
                    Write-Host "🔄 Stopping process $($process.ProcessName) (PID: $pid) on port $Port..." -ForegroundColor Yellow
                    Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
                }
            }
        }
    }
    catch {
        Write-Host "⚠️ Could not stop processes on port $Port" -ForegroundColor Yellow
    }
}

# Stop services by port
$ports = @(8080, 8081, 8082, 8083, 8084)

foreach ($port in $ports) {
    Stop-ProcessByPort -Port $port
}

# Also stop any dotnet processes that might be running
Write-Host "🔄 Stopping any remaining dotnet processes..." -ForegroundColor Yellow
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowTitle -like "*Mumii*" -or $_.CommandLine -like "*Mumii*" } | Stop-Process -Force -ErrorAction SilentlyContinue

Write-Host "✅ All services stopped!" -ForegroundColor Green
Write-Host ""
Write-Host "💡 Database files are preserved:" -ForegroundColor Blue
Write-Host "  📁 auth.db" -ForegroundColor White
Write-Host "  📁 discovery.db" -ForegroundColor White
Write-Host "  📁 social.db" -ForegroundColor White
Write-Host ""
Write-Host "🔄 To start services again, run:" -ForegroundColor Blue
Write-Host "  .\scripts\run-sqlite.ps1" -ForegroundColor White
