$ErrorActionPreference = "Stop"

Push-Location $PSScriptRoot

try {
    Write-Host "[info] Stopping containers..."
    docker compose down

    Write-Host "[info] Dev environment stopped."
    Write-Host ""
    Write-Host "To also remove data volumes: docker compose down -v"
}
finally {
    Pop-Location
}
