# Ejecutar desde PowerShell con .NET 8 instalado.
$ErrorActionPreference = 'Stop'
$raizProyecto = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Push-Location $raizProyecto
try {
    dotnet restore LogiMarket360.sln
    if ($LASTEXITCODE -ne 0) { throw 'Fallo la restauracion.' }
    dotnet build LogiMarket360.sln --no-restore -m:1
    if ($LASTEXITCODE -ne 0) { throw 'Fallo la compilacion.' }
    dotnet test tests/LogiMarket.Tests --no-build --no-restore --logger 'trx;LogFileName=aceptacion.trx' --results-directory docs/evidencias
    if ($LASTEXITCODE -ne 0) { throw 'Hay pruebas fallidas.' }
} finally { Pop-Location }
