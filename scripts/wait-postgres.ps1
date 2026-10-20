# PostgreSQL'in hazir olmasini bekler: once docker exec (dotnet-postgres), yoksa port testi
param(
    [string]$ContainerName = 'dotnet-postgres',
    [string]$ComputerName = 'localhost',
    [int]$Port = 5432,
    [int]$TimeoutSec = 120,
    [int]$IntervalSec = 2
)

$ErrorActionPreference = 'Stop'
$deadline = [datetime]::UtcNow.AddSeconds($TimeoutSec)

Write-Host "[wait-postgres] (max ${TimeoutSec}s)..." -ForegroundColor Cyan

while ([datetime]::UtcNow -lt $deadline) {
    try {
        docker exec $ContainerName pg_isready -U ecommerce_user -d ecommerce_db 2>$null | Out-Null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[wait-postgres] PostgreSQL hazir (docker exec)." -ForegroundColor Green
            exit 0
        }
    }
    catch { }

    $tcp = Test-NetConnection -ComputerName $ComputerName -Port $Port -WarningAction SilentlyContinue
    if ($tcp.TcpTestSucceeded) {
        Write-Host "[wait-postgres] Port acik." -ForegroundColor Green
        exit 0
    }

    Start-Sleep -Seconds $IntervalSec
}

Write-Error "[wait-postgres] Zaman asimi."
exit 1
