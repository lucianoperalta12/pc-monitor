#!/bin/bash
# ============================================================
# deploy-server.sh — Deploy de PcMonitorServer en el VPS
# Ejecutar desde el directorio raíz del proyecto (Monitor/)
# Requisitos: .NET 8+ SDK instalado en el VPS, PostgreSQL corriendo
# ============================================================
set -e

# ── Variables — EDITAR ANTES DE CORRER ────────────────────
APP_DIR="/opt/pc-monitor-server"
FRONTEND_DIR="/var/www/pc-monitor"
SERVICE_NAME="pc-monitor"
DOTNET_RUNTIME="linux-x64"
# ──────────────────────────────────────────────────────────

echo "==> Publicando PcMonitorServer..."
cd PcMonitorServer
dotnet publish -c Release -r $DOTNET_RUNTIME --self-contained true -o /tmp/pc-monitor-publish
cd ..

echo "==> Copiando archivos del servidor..."
sudo mkdir -p $APP_DIR
sudo cp -r /tmp/pc-monitor-publish/. $APP_DIR/
sudo chmod +x $APP_DIR/PcMonitorServer

echo "==> Copiando frontend buildado..."
sudo mkdir -p $FRONTEND_DIR
sudo cp -r PcMonitorFrontend/dist/. $FRONTEND_DIR/

echo "==> Configurando systemd..."
sudo tee /etc/systemd/system/${SERVICE_NAME}.service > /dev/null <<EOF
[Unit]
Description=PC Monitor Server
After=network.target postgresql.service

[Service]
Type=notify
WorkingDirectory=$APP_DIR
ExecStart=$APP_DIR/PcMonitorServer
Restart=always
RestartSec=5
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
EOF

sudo systemctl daemon-reload
sudo systemctl enable $SERVICE_NAME
sudo systemctl restart $SERVICE_NAME

echo "==> Verificando nginx..."
sudo nginx -t && sudo systemctl reload nginx

echo ""
echo "✅ Deploy completo."
echo "   API:      https://TU_DOMINIO/api/pc/status?machineId=PC-CASA"
echo "   Frontend: https://TU_DOMINIO/pc/"
echo ""
echo "   Estado del servicio:"
sudo systemctl status $SERVICE_NAME --no-pager -l
