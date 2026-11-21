# Crea un paquete reproducible desde archivos versionados, sin binarios ni bases.
$ErrorActionPreference = 'Stop'
$raizProyecto = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$destino = Join-Path $raizProyecto 'output'
New-Item -ItemType Directory -Path $destino -Force | Out-Null
$zip = Join-Path $destino 'APE04_LogiMarket360_DATOS_PENDIENTES.zip'
git -C $raizProyecto archive --format=zip --output=$zip HEAD
if ($LASTEXITCODE -ne 0) { throw 'No se pudo crear el paquete desde Git.' }
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archivo = [IO.Compression.ZipFile]::OpenRead($zip)
try {
    $prohibidos = @($archivo.Entries | Where-Object { $_.FullName -match '(^|/)(bin|obj|\.vs|\.git)/|\.(db|sqlite|suo|user)$' })
    if ($prohibidos.Count -gt 0) { throw 'El paquete contiene archivos no permitidos.' }
    Write-Output "Paquete: $zip"
    Write-Output "Archivos: $($archivo.Entries.Count)"
} finally { $archivo.Dispose() }
