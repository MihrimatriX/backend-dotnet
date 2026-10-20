# API saglik endpoint'inin 200 donmesini bekler (varsayilan: docker-compose backend :5000)
param(
    [string]$Url = 'http://localhost:5000/health',
    [int]$TimeoutSec = 180,
    [int]$IntervalSec = 3
)

$ErrorActionPreference = 'Stop'
$deadline = [datetime]::UtcNow.AddSeconds($TimeoutSec)

Write-Host "[wait-api] $Url bekleniyor (max ${TimeoutSec}s)..." -ForegroundColor Cyan

while ([datetime]::UtcNow -lt $deadline) {
    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 10
        if ($response.StatusCode -eq 200) {
            Write-Host "[wait-api] API hazir." -ForegroundColor Green
            exit 0
        }
    }
    catch {
        Write-Host "[wait-api] Bekleniyor..." -ForegroundColor DarkGray
    }
    Start-Sleep -Seconds $IntervalSec
}

Write-Error "[wait-api] Zaman asimi."
exit 1
