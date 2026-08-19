# install-service.ps1
# Ejecutar como Administrador

$ServiceName = "PcMonitorAgent"
$DisplayName = "PC Monitor Agent"
$Description = "Envía heartbeats periódicos al servidor de monitoreo."

# Publicar el ejecutable primero
$ProjectPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$PublishPath = Join-Path $ProjectPath "publish"

Write-Host "Publicando el agente en $PublishPath ..."
dotnet publish "$ProjectPath\PcMonitorAgent.csproj" -c Release -r win-x64 --self-contained true -o "$PublishPath"

$ExePath = Join-Path $PublishPath "PcMonitorAgent.exe"

if (-not (Test-Path $ExePath)) {
    Write-Error "No se encontró el ejecutable en $ExePath. Verificá que la compilación haya sido exitosa."
    exit 1
}

# Eliminar servicio si ya existe
if (Get-Service -Name $ServiceName -ErrorAction SilentlyContinue) {
    Write-Host "Deteniendo y eliminando servicio existente..."
    Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
    sc.exe delete $ServiceName
    Start-Sleep -Seconds 2
}

# Crear el servicio
Write-Host "Instalando servicio..."
New-Service -Name $ServiceName `
            -DisplayName $DisplayName `
            -Description $Description `
            -BinaryPathName $ExePath `
            -StartupType Automatic

Start-Service -Name $ServiceName
Write-Host "Servicio '$ServiceName' instalado y en ejecución."
Get-Service -Name $ServiceName | Format-List Name, Status, StartType
