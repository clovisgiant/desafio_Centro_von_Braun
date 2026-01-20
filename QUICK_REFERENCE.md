# 🔥 Quick Reference - CIoTD Integration

## ⚡ Iniciar em 30 Segundos

```bash
cd desafio_Centro_von_Braun
docker-compose up -d
```

URLs:
- Backend: http://localhost:5000
- Device Agent: http://localhost:8000

## 🔐 Login Rápido

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

**Copiar Token** e usar em:
```bash
curl -H "Authorization: Bearer TOKEN" http://localhost:5000/api/device
```

## 📝 Documentos Importantes

| Documento | Propósito |
|-----------|----------|
| [README.md](README.md) | Overview, arquitetura, exemplos |
| [ARCHITECTURE.md](ARCHITECTURE.md) | Detalhes técnicos |
| [SETUP.md](SETUP.md) | Instruções de instalação |
| [CHECKLIST.md](CHECKLIST.md) | Status de implementação |
| [FLUXOS.md](FLUXOS.md) | Diagramas de fluxo |
| [RESUMO_EXECUTIVO.md](RESUMO_EXECUTIVO.md) | Visão geral executiva |

## 🔌 Endpoints Principais

### Autenticação
```
POST /api/auth/login
POST /api/auth/validate
```

### Dispositivos
```
GET  /api/device              # Lista IDs
GET  /api/device/{id}         # Detalhes
POST /api/device              # Criar
PUT  /api/device/{id}         # Atualizar
DELETE /api/device/{id}       # Deletar
POST /api/device/{id}/execute # Executar comando
```

### Health
```
GET /health                   # Backend
GET /api/health               # Device Agent
```

## 📦 Usuários de Teste

| Username | Password |
|----------|----------|
| admin | admin123 |
| technician | tech456 |
| researcher | research789 |

## 🎯 Dispositivos Pré-cadastrados

| ID | Tipo | Operações |
|----|------|-----------|
| sensor-soil-001 | Solo | READ_HUMIDITY, SET_THRESHOLD |
| sensor-weather-001 | Meteorológica | READ_TEMPERATURE, READ_HUMIDITY, READ_RAINFALL |
| irrigation-system-001 | Irrigação | START_IRRIGATION, STOP_IRRIGATION, GET_ZONE_STATUS |

## 💻 Desenvolvimento Local

### Backend .NET
```bash
cd backend-dotnet/CIoTDApi
dotnet run
# http://localhost:5000
```

### Device Agent Python
```bash
cd device-agent
python -m venv venv
source venv/bin/activate  # Linux/Mac
venv\Scripts\activate     # Windows
pip install -r requirements.txt
python -m app.main
# http://localhost:8000
```

## 🧪 Testar no VS Code

1. Instale extensão "REST Client"
2. Abra `backend-dotnet/CIoTDApi/test.http`
3. Clique em "Send Request"
4. Copie o token do login e use nas outras requisições

## 🐛 Troubleshooting Rápido

| Problema | Solução |
|----------|---------|
| Porta 5000 ocupada | Alterar em docker-compose.yml: "5001:5000" |
| Docker não inicia | Reiniciar Docker Desktop |
| Token expirado | Fazer login novamente |
| Device Agent não conecta | Verificar logs: `docker-compose logs device-agent` |

## 📊 Estrutura de Projeto

```
desafio_Centro_von_Braun/
├── backend-dotnet/          # .NET Backend
│   └── CIoTDApi/
│       └── src/             # Clean Architecture
├── device-agent/            # Python Device Agent
│   └── app/                 # FastAPI Application
└── Documentação (*.md)      # Guides & References
```

## 🔄 Fluxo de Comando Completo

```
Frontend
   ↓
POST /api/device/{id}/execute
{operation, parameters}
   ↓
Backend (.NET)
   ↓
POST http://device-agent:8000/api/execute
   ↓
Device Agent (Python)
   ↓
TCP/Telnet: cmd param1\r
   ↓
Dispositivo IoT
   ↓
Resposta: data\r
   ↓
Device Agent JSON
   ↓
Backend JSON
   ↓
Frontend Exibe
```

## 🤖 AI-First Highlights

- ✅ DTOs gerados com IA
- ✅ Lógica Telnet/TCP assíncrona
- ✅ Clean Architecture estruturada
- ✅ Documentação completa
- ✅ Validação manual do protocolo

## 🚀 Próximas Etapas

1. **Frontend Angular**: Signals + RxJS
2. **Banco de Dados**: SQL Server/PostgreSQL
3. **Testes**: xUnit + Pytest
4. **CI/CD**: GitHub Actions
5. **Monitoramento**: Prometheus + Grafana

## 📞 Suporte Rápido

```bash
# Ver status dos serviços
docker-compose ps

# Ver logs
docker-compose logs -f

# Parar serviços
docker-compose down

# Limpar tudo
docker-compose down -v
```

## ✅ Checklist Rápido

- [x] Backend .NET pronto
- [x] Device Agent Python pronto
- [x] Docker Compose funcional
- [x] Documentação completa
- [x] Exemplos de teste
- [x] Relatório AI-First
- [x] Pronto para apresentação

---

**Desenvolvido com AI-First Mindset** 🤖✨
