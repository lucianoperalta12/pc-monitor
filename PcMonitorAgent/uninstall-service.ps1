# uninstall-service.ps1
# Ejecutar como Administrador

$ServiceName = "PcMonitorAgent"

if (Get-Service -Name $ServiceName -ErrorAction SilentlyContinue) {
    Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
    sc.exe delete $ServiceName
    Write-Host "Servicio '$ServiceName' eliminado."
} else {
    Write-Host "El servicio '$ServiceName' no existe."
}
