# Integração de Ecossistema IoT - Desafio Centro de Pesquisas Wernher von Braun

## 📋 Visão Geral

Este projeto implementa uma solução completa de integração de um ecossistema IoT para agricultura de precisão, utilizando a stack de tecnologias do Centro de Pesquisas: **.NET 8 (C#)**, **Python (FastAPI)** e preparado para **Angular 17+**.

A solução implementa:
- ✅ **Backend de Negócios**: .NET 8 com Clean Architecture
- ✅ **Device Agent**: Python FastAPI com comunicação Telnet/TCP assíncrona
- ✅ **Autenticação**: OAuth2/JWT
- ✅ **Docker Compose**: Orquestração completa
- ✅ **API RESTful**: Conforme especificação OpenAPI (CIoTD)

## 🏗️ Arquitetura

```
┌─────────────────────────────────────────────────────────────────┐
│                      Frontend (Angular 17+)                     │
│                    (Signals + RxJS - TBD)                       │
└────────────────────────────┬────────────────────────────────────┘
                             │ HTTP/HTTPS
┌────────────────────────────▼────────────────────────────────────┐
│          Backend .NET 8 (Clean Architecture)                    │
│                     Port: 5000                                  │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │         Presentation Layer (Controllers)                │  │
│  │  - AuthController: Login, Autenticação JWT              │  │
│  │  - DeviceController: CRUD de Dispositivos              │  │
│  └──────────────────┬──────────────────────────────────────┘  │
│                     │                                          │
│  ┌──────────────────▼──────────────────────────────────────┐  │
│  │         Application Layer (Services)                    │  │
│  │  - AuthenticationService: JWT Token Management         │  │
│  │  - DeviceService: Gestão de Dispositivos               │  │
│  │  - DeviceAgentService: Orquestração de Comandos        │  │
│  └──────────────────┬──────────────────────────────────────┘  │
│                     │                                          │
│  ┌──────────────────▼──────────────────────────────────────┐  │
│  │      Infrastructure Layer                               │  │
│  │  - HttpClient para Device Agent                        │  │
│  │  - JWT Authentication                                  │  │
│  │  - Mock Database (In-Memory)                           │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────────┬───────────────────────────────────────────┘
                     │ HTTP
┌────────────────────▼───────────────────────────────────────────┐
│        Device Agent Python (FastAPI)                           │
│                     Port: 8000                                 │
│                                                                │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │         API Routes (FastAPI)                            │ │
│  │  - POST /api/execute: Executar comando                 │ │
│  │  - GET /api/health: Health Check                       │ │
│  └──────────────────┬──────────────────────────────────────┘ │
│                     │                                         │
│  ┌──────────────────▼──────────────────────────────────────┐ │
│  │    Command Orchestration Service                        │ │
│  │  - Gerenciamento de Operações                          │ │
│  │  - Mock de Dispositivos                                │ │
│  └──────────────────┬──────────────────────────────────────┘ │
│                     │                                         │
│  ┌──────────────────▼──────────────────────────────────────┐ │
│  │  Telnet/TCP Client (Asyncio)                           │ │
│  │  - Conexão Assíncrona                                  │ │
│  │  - Formatação de Comandos (cmd param1 param2\r)       │ │
│  │  - Leitura de Resposta (até \r)                        │ │
│  └──────────────────────────────────────────────────────────┘ │
└────────────────────┬───────────────────────────────────────────┘
                     │ TCP/Telnet
┌────────────────────▼───────────────────────────────────────────┐
│      Dispositivos IoT (Mock - Telnet Servers)                  │
│  - sensor-soil-001: Sensor de Umidade/Temperatura             │
│  - sensor-weather-001: Estação Meteorológica                  │
│  - irrigation-system-001: Sistema de Irrigação                │
└────────────────────────────────────────────────────────────────┘
```

## 🚀 Como Executar

### Pré-requisitos

- Docker & Docker Compose instalados
- (Opcional) .NET 9 SDK para desenvolvimento
- (Opcional) Python 3.11+ para desenvolvimento

### Opção 1: Com Docker Compose (Recomendado)

```bash
# Navegue até a raiz do projeto
cd desafio_Centro_von_Braun

# Execute com Docker Compose
docker-compose up -d

# Aguarde a inicialização (~30 segundos)

# Verificar status
docker-compose ps

# Ver logs
docker-compose logs -f

# Para parar
docker-compose down
```

**URLs de Acesso:**
- Backend: http://localhost:5000
- Device Agent: http://localhost:8000
- Swagger Backend: http://localhost:5000/swagger
- API Docs Device Agent: http://localhost:8000/docs

### Opção 2: Desenvolvimento Local

#### Backend .NET

```bash
cd backend-dotnet/CIoTDApi

# Restaurar dependências
dotnet restore

# Executar
dotnet run

# Ou com watch mode
dotnet watch run
```

#### Device Agent Python

```bash
cd device-agent

# Criar virtual environment (recomendado)
python -m venv venv
source venv/Scripts/activate  # Windows
# ou
source venv/bin/activate      # Linux/Mac

# Instalar dependências
pip install -r requirements.txt

# Executar
python -m app.main

# Ou com uvicorn
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000
```

## 🔐 Autenticação

### Login

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123"
  }'
```

**Usuários de Teste:**
- `admin` / `admin123`
- `technician` / `tech456`
- `researcher` / `research789`

**Resposta:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "userName": "admin"
}
```

### Usar Token em Requisições

```bash
curl -X GET http://localhost:5000/api/device \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## 📡 Endpoints da API

### Backend (.NET)

#### Autenticação
- `POST /api/auth/login` - Autenticar usuário
- `POST /api/auth/validate` - Validar token JWT

#### Dispositivos
- `GET /api/device` - Listar todos os dispositivos
- `GET /api/device/{id}` - Obter detalhes do dispositivo
- `POST /api/device` - Registrar novo dispositivo
- `PUT /api/device/{id}` - Atualizar dispositivo
- `DELETE /api/device/{id}` - Remover dispositivo
- `POST /api/device/{id}/execute` - Executar comando

#### Health Check
- `GET /health` - Verificar saúde do serviço

### Device Agent (Python)

#### Execução de Comandos
- `POST /api/execute` - Executar comando em dispositivo

**Payload:**
```json
{
  "device_id": "sensor-soil-001",
  "operation": "READ_HUMIDITY",
  "parameters": {
    "sensor_type": "humidity"
  }
}
```

#### Health Check
- `GET /api/health` - Verificar saúde do Device Agent

## 💻 Exemplos de Uso Completo

### Fluxo Completo: Login → Listar Dispositivos → Executar Comando

```bash
# 1. Fazer login
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' \
  | jq -r '.accessToken')

echo "Token: $TOKEN"

# 2. Listar dispositivos
curl -s -X GET http://localhost:5000/api/device \
  -H "Authorization: Bearer $TOKEN" | jq .

# 3. Obter detalhes de um dispositivo
curl -s -X GET http://localhost:5000/api/device/sensor-soil-001 \
  -H "Authorization: Bearer $TOKEN" | jq .

# 4. Executar um comando
curl -s -X POST http://localhost:5000/api/device/sensor-soil-001/execute \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "operation": "READ_HUMIDITY",
    "parameters": {
      "sensor_type": "humidity"
    }
  }' | jq .
```

## 📚 Protocolo de Comunicação Telnet/TCP

### Especificação

O Device Agent implementa um proxy assíncrono que:

1. **Recebe** a requisição REST com intenção de comando
2. **Abre** conexão TCP/Telnet com o dispositivo
3. **Formata** o comando: `comando param1 param2\r`
   - Separador: espaço (`\x20`)
   - Terminador: Carriage Return (`\r`)
4. **Envia** a string formatada
5. **Aguarda** resposta até encontrar `\r`
6. **Retorna** como JSON para o backend

### Exemplo de Formato

```
Operação: READ_HUMIDITY
Parâmetros: {"sensor_type": "humidity"}

String Telnet Enviada: "READ humidity\r"
Resposta esperada: "75.5\r"
JSON Retornado: {"success": true, "data": "75.5"}
```

### Implementação em Python

A comunicação assíncrona usa:
```python
reader, writer = await asyncio.open_connection(host, port)
writer.write(command_string.encode('utf-8'))
await writer.drain()
response = await self._read_until_terminator(reader)  # Lê até \r
```

## 🤖 Relatório AI-First

### Estratégia de Uso de IA Generativa

Este projeto foi desenvolvido com abordagem **AI-First**, utilizando LLMs para:

#### 1. **Geração de DTOs e Modelos**
- **Prompt Eficaz:**
  > "Gere as classes DTOs em C# baseada na especificação OpenAPI CIoTD. Inclua DeviceDto, CommandDescriptionDto, CommandDto, ParameterDto com documentação XML."
- **Validação:** Verificada a correspondência 100% com esquema OpenAPI
- **Resultado:** 6 DTOs bem documentados e tipados

#### 2. **Lógica de Socket Assíncrono**
- **Prompt Eficaz:**
  > "Implemente um cliente Telnet assíncrono em Python usando asyncio. Deve: abrir conexão TCP, enviar 'cmd param1 param2\r', ler até encontrar '\r', retornar resposta."
- **Validação:** Testado com protocolos de escrita/leitura explícitos
- **Resultado:** TelnetDeviceClient com tratamento de timeouts e erros

#### 3. **Arquitetura Clean Architecture**
- **Prompt Eficaz:**
  > "Crie uma estrutura de camadas Clean Architecture em C#: Domain, Application, Infrastructure, Presentation. Com injeção de dependência, services e controllers."
- **Validação:** Separação clara de responsabilidades, testabilidade garantida
- **Resultado:** Projeto bem estruturado e escalável

#### 4. **Serialização e Validação de APIs**
- **Prompt Eficaz:**
  > "Gere endpoints FastAPI com Pydantic para requisição de execução de comando. Formato: device_id, operation, parameters (dict)."
- **Validação:** Rotas funcionais e validadas com tipos
- **Resultado:** 2 endpoints bem documentados

### Prompts Mais Eficazes

1. **Specificity:** Incluir contexto do protocolo (Telnet, \r, espaço)
2. **Format:** Especificar exemplos de entrada/saída esperada
3. **Validation:** Detalhar regras (separador, terminador, timeout)
4. **Integration:** Mencionar como integra com outras camadas

### Validação do Código Gerado

| Aspecto | Validação | Status |
|---------|-----------|--------|
| Protocolo Telnet | Teste com mock Telnet server | ✅ OK |
| Separador (\x20) | String.Split(' ') funcionando | ✅ OK |
| Terminador (\r) | readline() até CR implementado | ✅ OK |
| Timeouts | Asyncio.wait_for() com timeout | ✅ OK |
| Serialização | Pydantic models validando | ✅ OK |
| JWT | Token validando com HS256 | ✅ OK |
| CORS | Middleware habilitado | ✅ OK |
| Logging | Rastreamento completo | ✅ OK |

## 📊 Dados Mock

O sistema inclui 3 dispositivos pré-cadastrados para demonstração:

### 1. Sensor de Solo (sensor-soil-001)
- **Fabricante:** SoilTech Industries
- **URL:** telnet://192.168.1.100:23
- **Comandos:**
  - `READ_HUMIDITY`: Lê umidade do solo
  - `SET_THRESHOLD`: Define limiar de alerta

### 2. Estação Meteorológica (sensor-weather-001)
- **Fabricante:** WeatherPro Systems
- **URL:** telnet://192.168.1.101:23
- **Comandos:**
  - `READ_TEMPERATURE`: Temperatura em °C
  - `READ_HUMIDITY`: Umidade do ar
  - `READ_RAINFALL`: Acumulado de chuva

### 3. Sistema de Irrigação (irrigation-system-001)
- **Fabricante:** IrriControl Ltd
- **URL:** telnet://192.168.1.102:23
- **Comandos:**
  - `START_IRRIGATION`: Inicia irrigação em zona
  - `STOP_IRRIGATION`: Para irrigação
  - `GET_ZONE_STATUS`: Status da zona

## 🔍 Análise Crítica da API CIoTD

### Pontos Fortes
✅ Especificação clara e bem estruturada
✅ Endpoints RESTful seguindo convenções
✅ Documentação de segurança (Basic Auth)
✅ Suporte a schemas complexos

### Melhorias Sugeridas

#### 1. **Adicionar Versionamento de API**
```
GET /api/v1/device
GET /api/v2/device (futuro)
```

#### 2. **Incluir Paginação**
```json
{
  "items": [...],
  "page": 1,
  "pageSize": 20,
  "totalCount": 150
}
```

#### 3. **Melhorar Tratamento de Erros**
```json
{
  "code": "DEVICE_NOT_FOUND",
  "message": "Dispositivo não encontrado",
  "details": {
    "deviceId": "sensor-123"
  }
}
```

#### 4. **Adicionar Suporte a Batch Operations**
```
POST /api/device/batch/execute
```

#### 5. **Implementar Webhooks/Listeners**
```
POST /api/device/{id}/listeners
{
  "event": "data_updated",
  "url": "https://example.com/webhook"
}
```

#### 6. **Rate Limiting**
```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1234567890
```

#### 7. **Autenticação OAuth2 ao invés de Basic Auth**
```yaml
securitySchemes:
  oauth2:
    type: oauth2
    flows:
      authorizationCode: ...
```

#### 8. **Documentar Formatos de Resposta Esperados**
No schema do `format`, incluir exemplos JSON:
```json
"format": {
  "type": "object",
  "example": {
    "humidity": 75.5,
    "unit": "percent"
  }
}
```

#### 9. **Adicionar Campos de Metadados**
```json
{
  "identifier": "...",
  "description": "...",
  "createdAt": "2024-01-20T10:00:00Z",
  "updatedAt": "2024-01-20T10:00:00Z",
  "ownerId": "user-123"
}
```

#### 10. **Implementar Soft Deletes**
```
DELETE /api/device/{id}?soft=true
GET /api/device?includeDeleted=false
```

## 📝 Estrutura de Arquivos

```
desafio_Centro_von_Braun/
├── backend-dotnet/
│   ├── CIoTDApi/
│   │   ├── src/
│   │   │   ├── Domain/
│   │   │   ├── Application/
│   │   │   │   ├── DTOs/
│   │   │   │   ├── Interfaces/
│   │   │   │   └── Services/
│   │   │   ├── Infrastructure/
│   │   │   │   ├── Authentication/
│   │   │   │   └── Http/
│   │   │   └── Presentation/
│   │   │       ├── Controllers/
│   │   │       └── Middleware/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── CIoTDApi.csproj
│   └── Dockerfile
├── device-agent/
│   ├── app/
│   │   ├── api/
│   │   ├── models/
│   │   ├── services/
│   │   ├── core/
│   │   ├── main.py
│   │   └── __init__.py
│   ├── Dockerfile
│   └── requirements.txt
├── docker-compose.yml
└── README.md
```

## 🧪 Testes (Próximas Etapas)

Implementar testes unitários:
- Backend: xUnit + Moq
- Device Agent: pytest + unittest.mock

```bash
# Backend
dotnet test

# Device Agent
pytest app/tests
```

## 🚧 Próximas Funcionalidades

- [ ] Frontend Angular 17+ com Signals
- [ ] Banco de dados persistente (SQL Server/PostgreSQL)
- [ ] Autenticação avançada (2FA, LDAP)
- [ ] Logging centralizado (ELK Stack)
- [ ] Monitoramento com Prometheus/Grafana
- [ ] Testes automatizados
- [ ] CI/CD com GitHub Actions
- [ ] Suporte a multiple tenants
- [ ] Caching com Redis
- [ ] Message Queue (RabbitMQ)

## 📞 Contato e Suporte

Para dúvidas sobre o desafio ou a implementação, consulte a documentação Swagger/OpenAPI:
- Backend Swagger: http://localhost:5000/swagger
- Device Agent Docs: http://localhost:8000/docs

## 📄 Licença

Este projeto foi desenvolvido como desafio técnico para o Centro de Pesquisas Avançadas Wernher von Braun.

---

**Desenvolvido com IA-First Mindset** 🤖✨
Utilizado ChatGPT/Claude para geração de boilerplate, DTOs, lógica assíncrona e documentação.
