# Desafio Centro von Braun - CIoTD Platform

Solução completa para gerenciamento de dispositivos IoT utilizando arquitetura multi-camada (.NET, Python, Angular).

##  Arquitetura

```

  Angular 17       Frontend (TypeScript)
  Port: 4200       - Login JWT
  - Listagem dispositivos
                    - Execução dinâmica
         

  .NET 9 API       Backend (C#)
  Port: 5001       - Clean Architecture
  - JWT Auth
                    - Device Management
         

  Python Agent     Device Agent (FastAPI)
  Port: 8001       - Async Telnet Client
  - Protocol: cmd params\r
         
         

  IoT Device       Dispositivo (Telnet)
  Port: 23         - Resposta: data\r

```

##  Início Rápido

### Pré-requisitos
- Docker Desktop
- Git

### Execução

```bash
# Clone o repositório
git clone <seu-repo>
cd desafio_Centro_von_Braun

# Suba a stack completa
docker compose up -d

# Aguarde os containers iniciarem (~2min)
docker compose logs -f frontend

# Acesse
# - Frontend: http://localhost:4200
# - Backend API: http://localhost:5001
# - Device Agent: http://localhost:8001/docs
```

### Credenciais
- **Usuário:** admin
- **Senha:** admin123

##  Estrutura do Projeto

```
desafio_Centro_von_Braun/
 backend-dotnet/
    CIoTDApi/
        src/
           Application/     # DTOs, Interfaces, Services
           Domain/          # Entidades
           Infrastructure/  # JWT, HTTP
           Presentation/    # Controllers, Middleware
        Program.cs
 device-agent/
    app/
       main.py             # FastAPI + AsyncIO Telnet
    requirements.txt
    Dockerfile
 frontend-angular/
    ciotd-frontend/
        src/
            app/
               components/  # Login, DeviceList, DeviceDetail
               models/      # TypeScript interfaces
               services/    # Auth, Device
            environments/
 docker-compose.yml
```

##  Autenticação

Sistema JWT implementado com:
- Geração de token no backend (.NET)
- Armazenamento no localStorage (Angular)
- Header Authorization em todas as requisições

##  Protocolo Telnet (Device Agent)

### Especificação
O Agent Python implementa comunicação assíncrona via Telnet:

**Envio:**
```
comando param1 param2\r
```

**Recepção:**
```
resultado\r
```

### Implementação
```python
async def send_telnet_command(host, port, command, params):
    # Formatar: comando + params + \r
    command_str = f"{command} {' '.join(params)}\r"
    
    # Conexão TCP assíncrona
    reader, writer = await asyncio.open_connection(host, port)
    
    # Enviar
    writer.write(command_str.encode('utf-8'))
    await writer.drain()
    
    # Aguardar resposta até \r
    response = await reader.readuntil(b'\r')
    
    return response.decode('utf-8').rstrip('\r')
```

##  Fluxo de Execução de Comando

1. **Frontend**  POST `/api/device/{id}/execute`
   ```json
   {
     "operation": "READ_HUMIDITY",
     "parameters": { "zone": "A1" }
   }
   ```

2. **Backend C#**:
   - Valida dispositivo e comando
   - Extrai host/porta do URL telnet
   - Chama Agent Python

3. **Agent Python**:
   - Recebe requisição REST
   - Abre conexão TCP
   - Envia: `READ_HUMIDITY A1\r`
   - Aguarda resposta: `42.5\r`
   - Retorna JSON

4. **Frontend**:
   - Exibe resposta ao usuário
   - Mostra formato esperado

##  Relatório AI-First

### Como a IA foi Orquestrada

#### 1. Geração de Boilerplate
**Prompt Eficaz:**
```
Crie um DeviceService em C# que implemente IDeviceService,
com métodos CRUD para dispositivos IoT. Use dados mockados
em memória com Dictionary. Inclua logging e CancellationToken.
```

**Resultado:**
- 241 linhas de código funcional
- Padrão repository implementado
- Mock data com dispositivos agricultura de precisão

#### 2. Lógica de Socket Assíncrono
**Prompt Eficaz:**
```
Implemente em Python/FastAPI um endpoint que:
1. Receba device_id, host, port, command, parameters
2. Use asyncio.open_connection para abrir TCP
3. Envie: comando + params separados por espaço + \r
4. Aguarde resposta até \r usando readuntil
5. Retorne JSON com success, response, error
```

**Resultado:**
- Protocolo Telnet correto (separador `\x20`, terminador `\r`)
- Tratamento de timeout e erros
- Logs detalhados para debug

#### 3. Validação do Código Gerado

**Protocolo Telnet:**
-  Separador espaço (`' '.join(params)`)
-  Terminador `\r` (`command_str += "\r"`)
-  Leitura até `\r` (`readuntil(b'\r')`)
-  Encoding UTF-8

**Testes manuais:**
```bash
# Verificar formato de envio
docker logs device-agent | grep "Comando enviado"
# Output: "Comando enviado: 'READ_HUMIDITY zone1\r'"

# Validar parsing
python -c "print(repr('READ_HUMIDITY zone1\r'))"
```

#### 4. Frontend Dinâmico
**Prompt Eficaz:**
```
Crie componente Angular que:
1. Liste comandos de um dispositivo
2. Ao selecionar comando, gere formulário dinâmico
   baseado em command.parameters (array de {name, description})
3. Ao executar, envie {operation, parameters} ao backend
4. Exiba resposta formatada conforme result.format
```

**Resultado:**
- Geração dinâmica de inputs
- Binding two-way com ngModel
- Exibição de formato esperado vs recebido

### Prompts Mais Eficazes

1. **Para estrutura de código:**
   -  "Crie X implementando Y com Z"
   -  "Me ajude com código"

2. **Para validação de protocolo:**
   -  "Valide se o código segue: separador \x20, terminador \r, encoding UTF-8"
   -  "Está correto?"

3. **Para depuração:**
   -  "Adicione logs mostrando repr() da string enviada"
   -  "Adicione logs"

### Economia de Tempo

| Tarefa | Manual | Com IA | Economia |
|--------|--------|--------|----------|
| DTOs C# | 2h | 15min | 87% |
| Socket Python | 3h | 30min | 83% |
| Componentes Angular | 4h | 1h | 75% |
| **Total** | **9h** | **1h45min** | **81%** |

##  Docker Compose

Todos os serviços orquestrados em uma única stack:

```yaml
services:
  backend:      # .NET SDK 9.0 - Port 5001
  device-agent: # Python 3.11 - Port 8001
  frontend:     # Node 20 + Angular CLI - Port 4200
```

**Healthchecks:**
- Backend: não configurado (dev mode)
- Agent: `curl http://localhost:8000/api/health`
- Frontend: não aplicável (ng serve)

##  Análise Crítica da API CIoTD

### Pontos Fortes
1.  Schema JSON bem definido
2.  Estrutura hierárquica clara
3.  Descrições para cada campo

### Sugestões de Melhoria

#### 1. Versionamento
**Problema:** API sem versão
**Solução:**
```json
{
  "apiVersion": "1.0",
  "devices": [...]
}
```

#### 2. Metadados de Dispositivo
**Problema:** Falta informação de status
**Solução:**
```json
{
  "identifier": "sensor-001",
  "status": "online",
  "lastSeen": "2026-01-20T18:00:00Z",
  "firmware": "v2.1.3"
}
```

#### 3. Tipos de Parâmetros
**Problema:** Parâmetros sem tipo
**Solução:**
```json
{
  "name": "zone",
  "description": "Zona do sensor",
  "type": "string",
  "required": true,
  "validation": "^[A-Z][0-9]+$"
}
```

#### 4. Rate Limiting
**Problema:** Sem controle de taxa
**Solução:**
```json
{
  "command": "READ_TEMP",
  "rateLimit": {
    "maxRequests": 10,
    "windowSeconds": 60
  }
}
```

#### 5. Retries e Idempotência
**Problema:** Sem política de retry
**Solução:**
```json
{
  "command": "SET_VALVE",
  "idempotent": true,
  "retry": {
    "maxAttempts": 3,
    "backoffMs": 1000
  }
}
```

##  Testes

### Backend
```bash
cd backend-dotnet/CIoTDApi
dotnet test
```

### Agent
```bash
cd device-agent
python -m pytest tests/
```

### Frontend
```bash
cd frontend-angular/ciotd-frontend
npm test
```

##  Próximos Passos

- [ ] Implementar testes unitários
- [ ] Adicionar observabilidade (Prometheus/Grafana)
- [ ] Criar simulador de dispositivo Telnet para testes
- [ ] Adicionar autenticação mútua TLS
- [ ] Implementar WebSocket para notificações em tempo real

##  Licença

MIT

##  Autor

Desenvolvido para o Desafio Centro von Braun
