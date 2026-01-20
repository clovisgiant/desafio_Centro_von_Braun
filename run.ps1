# Script de Inicialização - CIoTD Integration
# Uso: .\run.ps1 [up|down|restart|logs|clean]

param(
    [string]$Command = "up"
)

$projectRoot = Get-Location
$dockerComposePath = Join-Path $projectRoot "docker-compose.yml"

function Show-Menu {
    Write-Host "╔════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║  CIoTD - IoT Integration                ║" -ForegroundColor Cyan
    Write-Host "║  Centro Wernher von Braun              ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
}

function Check-Docker {
    try {
        docker --version | Out-Null
        Write-Host "✓ Docker detectado" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ Docker não encontrado. Instale Docker Desktop primeiro." -ForegroundColor Red
        exit 1
    }
}

function Start-Services {
    Show-Menu
    Write-Host "🚀 Iniciando serviços..." -ForegroundColor Yellow
    docker-compose up -d
    
    Write-Host ""
    Write-Host "⏳ Aguardando inicialização (30 segundos)..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5
    
    # Verificar saúde dos serviços
    Write-Host ""
    Write-Host "📊 Status dos serviços:" -ForegroundColor Cyan
    docker-compose ps
    
    Write-Host ""
    Write-Host "✓ Serviços iniciados com sucesso!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📋 URLs de Acesso:" -ForegroundColor Cyan
    Write-Host "  • Backend:        http://localhost:5000" -ForegroundColor White
    Write-Host "  • Swagger UI:     http://localhost:5000/swagger" -ForegroundColor White
    Write-Host "  • Device Agent:   http://localhost:8000" -ForegroundColor White
    Write-Host "  • API Docs:       http://localhost:8000/docs" -ForegroundColor White
    Write-Host ""
    Write-Host "🔐 Usuários de Teste:" -ForegroundColor Cyan
    Write-Host "  • admin       / admin123" -ForegroundColor White
    Write-Host "  • technician  / tech456" -ForegroundColor White
    Write-Host "  • researcher  / research789" -ForegroundColor White
}

function Stop-Services {
    Show-Menu
    Write-Host "🛑 Parando serviços..." -ForegroundColor Yellow
    docker-compose down
    Write-Host "✓ Serviços parados." -ForegroundColor Green
}

function Restart-Services {
    Stop-Services
    Write-Host ""
    Start-Services
}

function Show-Logs {
    Show-Menu
    Write-Host "📖 Exibindo logs (Ctrl+C para sair)..." -ForegroundColor Yellow
    docker-compose logs -f
}

function Clean-All {
    Show-Menu
    Write-Host "🗑️  Removendo containers e volumes..." -ForegroundColor Yellow
    docker-compose down -v
    Write-Host "✓ Limpeza concluída." -ForegroundColor Green
}

# Main
Check-Docker

switch ($Command.ToLower()) {
    "up" { Start-Services }
    "down" { Stop-Services }
    "restart" { Restart-Services }
    "logs" { Show-Logs }
    "clean" { Clean-All }
    default {
        Write-Host "Uso: .\run.ps1 [up|down|restart|logs|clean]" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Comandos disponíveis:" -ForegroundColor Cyan
        Write-Host "  up       - Inicia os serviços" -ForegroundColor White
        Write-Host "  down     - Para os serviços" -ForegroundColor White
        Write-Host "  restart  - Reinicia os serviços" -ForegroundColor White
        Write-Host "  logs     - Mostra os logs em tempo real" -ForegroundColor White
        Write-Host "  clean    - Remove containers e volumes" -ForegroundColor White
    }
}
