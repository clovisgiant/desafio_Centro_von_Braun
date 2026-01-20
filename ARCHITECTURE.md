# Documentação Técnica - Arquitetura e Implementação

## Índice
1. [Arquitetura Geral](#arquitetura-geral)
2. [Backend .NET](#backend-net)
3. [Device Agent Python](#device-agent-python)
4. [Protocolo de Comunicação](#protocolo-de-comunicação)
5. [Fluxo de Dados](#fluxo-de-dados)
6. [Integração Frontend (Próximo)](#integração-frontend)

## Arquitetura Geral

### Padrão: Clean Architecture + Microserviços

```
┌─────────────┐     ┌──────────────┐     ┌─────────────┐
│  Frontend   │────▶│   Backend    │────▶│   Device    │
│  (Angular)  │     │   (.NET 8)   │     │   Agent     │
└─────────────┘     └──────────────┘     │  (Python)   │
                                         └─────────────┘
                                              │
                                              ▼
                                         ┌──────────┐
                                         │ Devices  │
                                         │  (Telnet)│
                                         └──────────┘
```

### Princípios Aplicados
- **Separation of Concerns**: Cada camada tem responsabilidade específica
- **Dependency Injection**: Injeção de dependências em todas as camadas
- **Async/Await**: Operações não-bloqueantes onde necessário
- **Contract First**: DTOs bem definidos entre camadas

## Backend .NET

### Estrutura de Camadas

```
Presentation Layer (Controllers)
    ↓
Application Layer (Services + Interfaces)
    ↓
Infrastructure Layer (HTTP, Auth, Data)
    ↓
Domain Layer (Models - atualmente vazio, pronto para entidades)
```

### Arquivos Criados

#### Camada de Apresentação
```
src/Presentation/
├── Controllers/
│   ├── AuthController.cs        # Login, validação JWT
│   └── DeviceController.cs       # CRUD + execução de comandos
└── Middleware/
    └── JwtMiddleware.cs          # Validação de tokens em requisições
```

**AuthController:**
- `POST /api/auth/login` - Retorna JWT token
- `POST /api/auth/validate` - Valida um token

**DeviceController:**
- `GET /api/device` - Lista IDs de dispositivos
- `GET /api/device/{id}` - Detalhes do dispositivo
- `POST /api/device` - Registrar dispositivo
- `PUT /api/device/{id}` - Atualizar dispositivo
- `DELETE /api/device/{id}` - Remover dispositivo
- `POST /api/device/{id}/execute` - Executar comando

#### Camada de Aplicação
```
src/Application/
├── DTOs/
│   ├── ParameterDto.cs
│   ├── CommandDto.cs
│   ├── CommandDescriptionDto.cs
│   ├── DeviceDto.cs
│   ├── ExecuteCommandDto.cs
│   └── AuthenticationDto.cs
├── Interfaces/
│   ├── IAuthenticationService.cs
│   ├── IDeviceService.cs
│   └── IDeviceAgentService.cs
└── Services/
    └── DeviceService.cs          # Lógica de gerenciamento de dispositivos
```

**Padrão DTO:**
- DTOs são usados para comunicação entre camadas
- Mapeamento automático via C# records quando possível
- Documentação XML para IntelliSense

#### Camada de Infraestrutura
```
src/Infrastructure/
├── Authentication/
│   └── JwtAuthenticationService.cs   # Implementação de autenticação JWT
└── Http/
    └── DeviceAgentService.cs          # Cliente HTTP para Device Agent
```

**JwtAuthenticationService:**
- Geração de tokens HS256
- Validação de claims
- Extração de informações do token
- Usuários mockados em memória

**DeviceAgentService:**
- HttpClient para chamar endpoints do Device Agent
- Tratamento de erros de comunicação
- Logging de operações

### Configuração de Dependências (Program.cs)

```csharp
// JWT Configuration
builder.Services.AddAuthentication("JwtBearer")
    .AddJwtBearer(options => { ... });

// Dependency Injection
builder.Services.AddScoped<IAuthenticationService, JwtAuthenticationService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddHttpClient<IDeviceAgentService, DeviceAgentService>();

// Middleware
app.UseJwtMiddleware();
app.UseAuthentication();
app.UseAuthorization();
```

### Dados Mock

Dispositivos pré-cadastrados em memória:
- `sensor-soil-001` - Sensor de solo
- `sensor-weather-001` - Estação meteorológica
- `irrigation-system-001` - Sistema de irrigação

Cada dispositivo possui:
- Identificador único
- Descrição
- Fabricante
- URL Telnet
- Lista de comandos disponíveis

## Device Agent Python

### Arquitetura

```
FastAPI Application
    ├── Routers (API Endpoints)
    ├── Services (Command Orchestration)
    └── Telnet Client (TCP Communication)
```

### Estrutura de Arquivos

```
app/
├── main.py                      # Aplicação FastAPI
├── api/
│   └── routes.py                # Endpoints REST
├── models/
│   └── schemas.py               # Pydantic models
├── services/
│   ├── command_service.py        # Orquestração de comandos
│   └── telnet_client.py          # Cliente TCP/Telnet
└── core/
    └── __init__.py
```

### Endpoints

```
POST /api/execute
├── Recebe: {"device_id", "operation", "parameters"}
├── Processa: Monta comando com parâmetros
├── Executa: Abre TCP, envia via Telnet, aguarda resposta
└── Retorna: {"success", "data", "error", "execution_time_ms"}

GET /api/health
└── Retorna: {"status", "message"}
```

### Serviço de Orquestração (CommandService)

**Responsabilidades:**
1. Mapear operações para comandos Telnet
2. Validar dispositivo e operação
3. Construir lista de parâmetros ordenada
4. Chamar TelnetDeviceClient
5. Formatar resposta

**Fluxo:**
```python
async def execute_command(device_id, operation, parameters):
    # 1. Localiza dispositivo
    device = self.devices[device_id]
    
    # 2. Obtém comando para operação
    command_info = self._get_command_for_operation(device_id, operation)
    
    # 3. Monta lista de parâmetros
    param_list = self._build_parameter_list(command_info, parameters)
    
    # 4. Executa via Telnet
    success, response = await telnet_client.execute_command(
        device_url, 
        command_string, 
        param_list
    )
    
    # 5. Retorna resultado
    return CommandExecutionResult(success, response, execution_time_ms)
```

### Cliente Telnet/TCP Assíncrono

**Classe: TelnetDeviceClient**

**Método Principal: `execute_command`**

```python
async def execute_command(device_url, command, parameters):
    # 1. Parse URL: telnet://192.168.1.100:23 → (host, port)
    host, port = parse_device_url(device_url)
    
    # 2. Format command: cmd param1 param2\r
    command_string = format_command(command, parameters)
    
    # 3. Open TCP connection (async)
    reader, writer = await asyncio.open_connection(host, port)
    
    # 4. Send command with terminator \r
    writer.write(command_string.encode('utf-8'))
    await writer.drain()
    
    # 5. Read response until \r terminator
    response = await read_until_terminator(reader)
    
    # 6. Close connection
    writer.close()
    await writer.wait_closed()
    
    return (True, response)
```

**Formatação de Comando:**
```
Input:
  command = "READ"
  parameters = ["humidity"]

Output:
  "READ humidity\r"
  
Regras:
  - Separador: espaço (\x20)
  - Terminador: \r (0x0D - Carriage Return)
```

**Leitura de Resposta:**
```python
async def _read_until_terminator(reader, terminator=b'\r'):
    data = b''
    while True:
        chunk = await reader.read(1)
        data += chunk
        
        if data.endswith(terminator):
            return data[:-len(terminator)].decode('utf-8')
```

## Protocolo de Comunicação

### Especificação Telnet/TCP

#### Características
- **Protocolo:** TCP/Telnet (porta 23 por padrão)
- **Encoding:** UTF-8
- **Separador:** Espaço (`\x20`)
- **Terminador:** Carriage Return (`\r`, `\x0D`)
- **Timeout:** 10 segundos (configurável)

#### Exemplo Completo

```
Dispositivo: sensor-soil-001
Operação: READ_HUMIDITY
Parâmetros: {"sensor_type": "humidity"}

┌─────────────────────────────────────────────┐
│        Backend (.NET)                        │
├─────────────────────────────────────────────┤
│ POST /api/device/sensor-soil-001/execute    │
│ Body: {                                     │
│   "operation": "READ_HUMIDITY",             │
│   "parameters": {"sensor_type": "humidity"} │
│ }                                           │
└──────────────────┬──────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────┐
│        Device Agent (Python)                │
├─────────────────────────────────────────────┤
│ POST /api/execute                           │
│ Body: {                                     │
│   "device_id": "sensor-soil-001",           │
│   "operation": "READ_HUMIDITY",             │
│   "parameters": {"sensor_type": "humidity"} │
│ }                                           │
├─────────────────────────────────────────────┤
│ 1. Processa mapeamento de operação          │
│    Operation → Command "READ"               │
│    Parâmetros → ["humidity"]                │
│ 2. Monta string: "READ humidity\r"          │
│ 3. Abre TCP: 192.168.1.100:23               │
│ 4. Envia: bytes("READ humidity\r")          │
│ 5. Aguarda resposta até encontrar \r        │
│ 6. Recebe: "75.5\r"                         │
│ 7. Remove terminador: "75.5"                │
│ 8. Retorna JSON                             │
└──────────────────┬──────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────┐
│        Backend (.NET)                        │
├─────────────────────────────────────────────┤
│ Response: {                                 │
│   "success": true,                          │
│   "data": "75.5",                           │
│   "error": null,                            │
│   "executionTimeMs": 245                    │
│ }                                           │
└─────────────────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────┐
│        Frontend (Angular) - TBD             │
├─────────────────────────────────────────────┤
│ Exibe resultado ao usuário                  │
│ "Umidade do solo: 75.5%"                    │
└─────────────────────────────────────────────┘
```

### Tratamento de Erros

#### Timeouts
```
Se não receber resposta em 10 segundos:
→ ConnectionTimeoutError
→ Retorna: {"success": false, "error": "Timeout..."}
```

#### Conexão Recusada
```
Se não conseguir conectar ao dispositivo:
→ ConnectionRefusedError
→ Retorna: {"success": false, "error": "Connection refused..."}
```

#### Formato Inválido
```
Se o comando não seguir o protocolo:
→ Validado antes do envio
→ Retorna erro de validação
```

## Fluxo de Dados

### Fluxo de Autenticação

```
1. Cliente envia credenciais
   POST /api/auth/login
   {"username": "admin", "password": "admin123"}
   
2. Backend valida credenciais
   - Verifica se usuário existe
   - Verifica se senha está correta
   - Usuários mockados (Dev) ou AD/LDAP (Prod)
   
3. Gera JWT Token
   - Claims: username, iat, exp
   - Assinado com HS256
   - Expira em 60 minutos
   
4. Retorna token ao cliente
   {"accessToken": "eyJhb...", "expiresIn": 3600}
   
5. Cliente armazena token (localStorage/sessionStorage)
   
6. Cliente inclui em requisições subsequentes
   Authorization: Bearer eyJhb...
   
7. Backend valida token em middleware
   - Verifica assinatura
   - Verifica expiração
   - Extrai claims
   
8. Se válido, requisição prossegue com contexto do usuário
```

### Fluxo de Execução de Comando

```
1. Frontend envia intenção de comando
   POST /api/device/{id}/execute
   Authorization: Bearer TOKEN
   Body: {
     "operation": "READ_HUMIDITY",
     "parameters": {"sensor_type": "humidity"}
   }

2. Backend valida autenticação
   - Valida JWT token
   - Extrai informações do usuário
   
3. Backend valida negócio
   - Dispositivo existe?
   - Operação existe?
   - Usuário tem permissão?
   
4. Backend chama Device Agent
   POST http://device-agent:8000/api/execute
   Body: {
     "device_id": "sensor-soil-001",
     "operation": "READ_HUMIDITY",
     "parameters": {"sensor_type": "humidity"}
   }
   
5. Device Agent processa
   a) Mapeia operação para comando: "READ"
   b) Obtém parâmetros: ["humidity"]
   c) Formata string: "READ humidity\r"
   d) Abre TCP: telnet://192.168.1.100:23
   e) Envia comando
   f) Aguarda resposta com timeout
   
6. Dispositivo Telnet responde
   Envia: "75.5\r"
   
7. Device Agent processa resposta
   - Remove terminador \r
   - Formata JSON
   - Retorna ao Backend
   
8. Backend recebe resposta
   {
     "success": true,
     "data": "75.5",
     "executionTimeMs": 245
   }
   
9. Backend retorna ao Frontend
   Mesma resposta + contexto adicional
   
10. Frontend exibe resultado
    "Umidade: 75.5%"
```

## Integração Frontend

### Próximas Etapas

#### 1. Estrutura do Projeto Angular
```
frontend/
├── src/
│   ├── app/
│   │   ├── core/
│   │   │   ├── services/
│   │   │   │   ├── auth.service.ts
│   │   │   │   ├── device.service.ts
│   │   │   │   └── api.service.ts
│   │   │   ├── guards/
│   │   │   │   └── auth.guard.ts
│   │   │   └── interceptors/
│   │   │       ├── auth.interceptor.ts
│   │   │       └── error.interceptor.ts
│   │   ├── features/
│   │   │   ├── login/
│   │   │   ├── devices/
│   │   │   │   ├── device-list/
│   │   │   │   ├── device-detail/
│   │   │   │   └── device-execute/
│   │   │   └── dashboard/
│   │   └── shared/
│   │       ├── components/
│   │       └── pipes/
│   └── environments/
```

#### 2. Signals (Angular 17+)
```typescript
// Auth State usando Signals
export class AuthState {
  token = signal<string | null>(null);
  user = signal<User | null>(null);
  isAuthenticated = computed(() => !!this.token());
}

// Device State usando Signals
export class DeviceState {
  devices = signal<Device[]>([]);
  selectedDevice = signal<Device | null>(null);
  isLoading = signal(false);
}
```

#### 3. RxJS com Signals
```typescript
// Service que combine RxJS e Signals
@Injectable()
export class DeviceService {
  private devicesSubject = new BehaviorSubject<Device[]>([]);
  devices$ = this.devicesSubject.asObservable();
  
  // Convertendo para Signal
  devicesSignal = toSignal(this.devices$, { initialValue: [] });
}
```

#### 4. Componentes com Signals
```typescript
@Component({
  selector: 'app-device-list',
  template: `
    <button *ngFor="let device of devices()">
      {{ device.identifier }}
    </button>
  `
})
export class DeviceListComponent {
  devices = signal<Device[]>([]);
  isLoading = signal(false);
  
  constructor(private deviceService: DeviceService) {
    effect(() => {
      this.deviceService.getDevices().subscribe(
        devices => this.devices.set(devices)
      );
    });
  }
}
```

## Resumo Técnico

| Aspecto | Implementação | Status |
|---------|---------------|--------|
| Backend Framework | .NET 8 | ✅ Completo |
| Padrão Arquitetural | Clean Architecture | ✅ Implementado |
| Autenticação | JWT HS256 | ✅ Completo |
| Device Agent | FastAPI | ✅ Completo |
| Protocolo IoT | Telnet/TCP Assíncrono | ✅ Implementado |
| Docker | Docker Compose | ✅ Pronto |
| Frontend | Angular 17+ (Próximo) | 🔄 Planejado |
| Signals | TBD | 🔄 Em desenvolvimento |
| RxJS | TBD | 🔄 Em desenvolvimento |
| Testes | XUnit/Pytest | 🔄 Planejado |
| CI/CD | GitHub Actions | 🔄 Planejado |

---

Documentação técnica gerada com IA-First mindset.
Validação manual de protocolo Telnet, timeouts e formatação.
