# 🚀 Guia de Setup Local - CIoTD Integration

## Pré-requisitos

### Windows
- [x] Git (https://git-scm.com)
- [x] Docker Desktop (https://www.docker.com/products/docker-desktop)
- [x] Visual Studio Code (https://code.visualstudio.com) - Opcional
- [x] .NET 9 SDK (https://dotnet.microsoft.com) - Para desenvolvimento
- [x] Python 3.11+ (https://www.python.org) - Para desenvolvimento
- [x] PowerShell 5.1+ (Incluído no Windows)

### Linux/Mac
- [x] Git
- [x] Docker & Docker Compose
- [x] .NET 9 SDK (opcional)
- [x] Python 3.11+ (opcional)

## Instalação com Docker (Recomendado)

### Passo 1: Clonar ou fazer download do projeto

```bash
# Se tiver acesso ao Git
git clone <repository-url>
cd desafio_Centro_von_Braun

# Ou fazer download ZIP e extrair
cd desafio_Centro_von_Braun
```

### Passo 2: Iniciar os serviços

**Windows (PowerShell):**
```powershell
# Permitir execução de scripts (se necessário)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Executar
.\run.ps1 up
```

**Linux/Mac (Bash):**
```bash
docker-compose up -d
```

### Passo 3: Verificar status

```bash
docker-compose ps
```

Você deve ver:
```
NAME                 STATUS
ciotd-backend        Up (healthy)
ciotd-device-agent   Up (healthy)
```

### Passo 4: Testar os endpoints

```bash
# Health Check Backend
curl http://localhost:5000/health

# Health Check Device Agent
curl http://localhost:8000/api/health

# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

## Instalação para Desenvolvimento Local

### Backend .NET

```bash
# Navegar até o diretório do backend
cd backend-dotnet/CIoTDApi

# Restaurar dependências
dotnet restore

# Compilar
dotnet build

# Executar
dotnet run

# Ou com reload automático
dotnet watch run
```

Acesso: http://localhost:5000

### Device Agent Python

```bash
# Navegar até o diretório do device agent
cd device-agent

# Criar virtual environment
python -m venv venv

# Ativar virtual environment
# Windows:
venv\Scripts\activate
# Linux/Mac:
source venv/bin/activate

# Instalar dependências
pip install -r requirements.txt

# Executar
python -m app.main

# Ou com reload automático
uvicorn app.main:app --reload
```

Acesso: http://localhost:8000

## Testando a API

### Opção 1: VS Code REST Client

1. Instale a extensão "REST Client" no VS Code
2. Abra `backend-dotnet/CIoTDApi/test.http`
3. Clique em "Send Request" acima de cada requisição
4. Para requisições autenticadas, primeiro execute o Login e copie o token
5. Cole o token nos headers das outras requisições

### Opção 2: Postman

1. Instale Postman (https://www.postman.com)
2. Importe a coleção (ou crie manualmente)
3. Configure a URL base: `http://localhost:5000`
4. Teste os endpoints

### Opção 3: cURL

```bash
# Login
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' \
  | jq -r '.accessToken')

echo "Token: $TOKEN"

# Listar dispositivos
curl -s http://localhost:5000/api/device \
  -H "Authorization: Bearer $TOKEN" | jq .
```

## Estrutura de Diretórios

```
desafio_Centro_von_Braun/
├── README.md                    # Documentação principal
├── ARCHITECTURE.md              # Arquitetura detalhada
├── CHECKLIST.md                 # Checklist de implementação
├── docker-compose.yml           # Orquestração Docker
├── run.ps1                       # Script PowerShell
├── .gitignore
│
├── backend-dotnet/
│   ├── Dockerfile               # Build do backend
│   ├── CIoTDApi/
│   │   ├── src/
│   │   │   ├── Domain/
│   │   │   ├── Application/
│   │   │   ├── Infrastructure/
│   │   │   ├── Presentation/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── CIoTDApi.csproj
│   │   └── test.http
│   │
│   └── obj/ (gerado)
│
├── device-agent/
│   ├── Dockerfile               # Build do device agent
│   ├── app/
│   │   ├── main.py
│   │   ├── api/
│   │   ├── models/
│   │   ├── services/
│   │   └── core/
│   ├── requirements.txt
│   └── __init__.py
│
└── CIoTDApi.http                # Testes globais
```

## Variáveis de Ambiente

### Backend .NET (appsettings.json)

```json
{
  "Jwt": {
    "SecretKey": "SuperSecureKeyWith256BitsForJwtTokenSigningPurposes",
    "Issuer": "CIoTDApi",
    "Audience": "CIoTDApiUsers",
    "ExpirationMinutes": 60
  },
  "DeviceAgent": {
    "BaseUrl": "http://localhost:8000"
  }
}
```

### Device Agent Python

Sem arquivo .env necessário (usa defaults hardcoded).

## Troubleshooting

### Docker não inicia

```bash
# Verificar status do Docker
docker info

# Se não funcionar, reiniciar Docker Desktop
# Windows: Ctrl+Alt+Delete → Task Manager → Docker Desktop
# Fechar e reabrir
```

### Porta já em uso

```bash
# Encontrar processo usando porta 5000
# Windows (PowerShell):
Get-NetTCPConnection -LocalPort 5000

# Linux/Mac:
lsof -i :5000

# Alterar porta no docker-compose.yml
# "5000:5000" → "5001:5000"
```

### Device Agent não se conecta ao Backend

```bash
# Verificar conexão
docker exec ciotd-device-agent curl http://backend:5000/health

# Verificar logs
docker-compose logs device-agent
```

### Token JWT expirado

```bash
# Fazer login novamente
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

## Limpeza

```bash
# Parar serviços
docker-compose down

# Remover volumes (dados)
docker-compose down -v

# Remover tudo (incluindo imagens)
docker-compose down -v --rmi all

# PowerShell
.\run.ps1 clean
```

## Próximas Etapas

1. **Frontend Angular**: Implementar Signals + RxJS
2. **Testes**: Adicionar testes unitários
3. **CI/CD**: GitHub Actions para automação
4. **Banco de Dados**: SQL Server ou PostgreSQL
5. **Monitoramento**: Prometheus + Grafana

## Suporte

- 📖 Consulte [README.md](README.md) para overview
- 🏗️ Consulte [ARCHITECTURE.md](ARCHITECTURE.md) para detalhes técnicos
- ✅ Consulte [CHECKLIST.md](CHECKLIST.md) para status de implementação
- 📝 Consulte [CIoTDApi.http](CIoTDApi.http) para exemplos de API

---

**Desenvolvido com IA-First Mindset** 🤖✨
