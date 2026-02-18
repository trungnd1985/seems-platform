$ErrorActionPreference = "Stop"

Push-Location $PSScriptRoot

try {
    # Copy .env if not present
    if (-not (Test-Path .env)) {
        Copy-Item .env.example .env
        Write-Host "[info] Created .env from .env.example"
    }

    # Start infrastructure
    Write-Host "[info] Starting PostgreSQL and pgAdmin..."
    docker compose up -d

    # Wait for PostgreSQL to be healthy
    Write-Host "[info] Waiting for PostgreSQL..."
    do {
        Start-Sleep -Seconds 1
        $ready = docker compose exec -T postgres pg_isready -U seems_user -d seems_platform_dev 2>$null
    } until ($LASTEXITCODE -eq 0)
    Write-Host "[info] PostgreSQL is ready."

    # Run backend
    Write-Host "[info] Starting ASP.NET Core API (with auto-migrate & seed)..."
    $backendProcess = Start-Process -FilePath "dotnet" `
        -ArgumentList "run", "--project", "src\backend\Seems.Api", "--launch-profile", "https" `
        -PassThru -NoNewWindow

    Write-Host ""
    Write-Host "=========================================="
    Write-Host "  SEEMS Platform - Dev Environment"
    Write-Host "=========================================="
    Write-Host "  API:      https://localhost:5001"
    Write-Host "  Swagger:  https://localhost:5001/swagger"
    Write-Host "  pgAdmin:  http://localhost:8080"
    Write-Host "    Email:    admin@seems.local"
    Write-Host "    Password: admin"
    Write-Host ""
    Write-Host "  Admin login:"
    Write-Host "    Email:    admin@seems.local"
    Write-Host "    Password: Admin@123"
    Write-Host "=========================================="
    Write-Host ""
    Write-Host "Press Ctrl+C to stop..."

    # Wait for backend process
    $backendProcess.WaitForExit()
}
finally {
    # Cleanup on exit
    if ($backendProcess -and -not $backendProcess.HasExited) {
        Stop-Process -Id $backendProcess.Id -Force -ErrorAction SilentlyContinue
        Write-Host "[info] Backend stopped."
    }
    Pop-Location
}
