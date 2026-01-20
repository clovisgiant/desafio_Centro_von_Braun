# 🧪 Guia de Testes - IoT Challenge CIoTD

## ✅ Verificação de Serviços

### 1. Backend .NET
```powershell
# Status
curl http://localhost:5001/api/health

# Esperado: HTTP 200
```

### 2. Frontend Angular
```powershell
# Status
curl http://localhost:4200

# Esperado: HTTP 200 (página HTML)
```

### 3. Device Agent Python
```powershell
# Health check
curl http://localhost:8001/api/health

# Esperado: {"status": "healthy", "service": "device-agent"}
```

## 🧑‍💻 Testes de Funcionalidade

### Teste 1: Login
1. Abrir http://localhost:4200
2. Espera-se ver formulário de login
3. Inserir:
   - Usuário: `admin`
   - Senha: `admin123`
4. Clicar "Entrar"
5. Espera-se: redirecionamento para http://localhost:4200/devices

**Resultado esperado**: ✅ Login bem-sucedido, token armazenado em localStorage

### Teste 2: Listar Dispositivos
1. Após login bem-sucedido
2. Página deve exibir: "Dispositivos CIoTD"
3. Deve listar 3 dispositivos em grid:
   - sensor-soil-001
   - sensor-weather-001
   - irrigation-system-001

**Resultado esperado**: ✅ Grid com 3 cards de dispositivos

### Teste 3: Visualizar Detalhes
1. Clicar em um dispositivo (ex: sensor-soil-001)
2. Página deve redirecionar para http://localhost:4200/device/sensor-soil-001
3. Deve exibir:
   - Nome do dispositivo
   - ID (sensor-soil-001)
   - Localização
   - Painel de execução de comando

**Resultado esperado**: ✅ Página de detalhes com informações do dispositivo

### Teste 4: Executar Comando
1. Na página de detalhes de um dispositivo
2. Inserir comando: `STATUS`
3. Deixar parâmetros vazios (ou adicionar se necessário)
4. Clicar "Executar"
5. Aguardar processamento

**Resultado esperado**: ✅ Resultado exibido em caixa verde (sucesso) ou vermelha (erro)

### Teste 5: Logout
1. Na página de dispositivos
2. Clicar botão "Sair"
3. Deve redirecionar para http://localhost:4200/login
4. LocalStorage deve estar limpo (token removido)

**Resultado esperado**: ✅ Redirecionado para login, nova tentativa exige autenticação

## 🔧 Troubleshooting

### Problema: Página em branco no http://localhost:4200
**Solução**: 
- Verificar logs do container: `docker compose logs frontend`
- Limpar cache do browser: Ctrl+Shift+Delete
- Recarregar página: Ctrl+F5

### Problema: Erro 401 Unauthorized
**Solução**:
- Fazer login novamente
- Verificar se token está em localStorage (F12 → Application → localStorage)
- Verificar credenciais: admin / admin123

### Problema: Erro de conexão com backend
**Solução**:
- Verificar se backend está rodando: `docker compose ps`
- Verificar logs: `docker compose logs backend`
- Testar conexão: `curl http://localhost:5001/api/device`

### Problema: Componentes não carregam
**Solução**:
- Verificar se routing está correto em app.routes.ts
- Verificar imports nos componentes
- Restartar container frontend: `docker compose restart frontend`

## 📊 Teste de Integração Completa

### Script PowerShell de Teste Automático
```powershell
# 1. Verificar serviços
$services = @(
    "http://localhost:5001",
    "http://localhost:4200",
    "http://localhost:8001/api/health"
)

foreach ($url in $services) {
    $response = Invoke-WebRequest -Uri $url -UseBasicParsing -ErrorAction SilentlyContinue
    Write-Host "[$($response.StatusCode)] $url"
}

# 2. Testar API de dispositivos
$devices = Invoke-RestMethod -Uri http://localhost:5001/api/device -Method GET
Write-Host "Dispositivos encontrados: $($devices.Count)"

# 3. Testar health check do agent
$health = Invoke-RestMethod -Uri http://localhost:8001/api/health -Method GET
Write-Host "Agent status: $($health.status)"
```

## 📋 Checklist Final

- [ ] Backend respondendo em http://localhost:5001
- [ ] Frontend carregando em http://localhost:4200
- [ ] Device Agent saudável em http://localhost:8001
- [ ] Login funciona com admin/admin123
- [ ] Dispositivos são listados após login
- [ ] Clicar em dispositivo abre detalhes
- [ ] Comando pode ser executado
- [ ] Resultado é exibido
- [ ] Logout funciona
- [ ] Re-login é possível

## 🎯 Endpoints Testáveis via cURL/PowerShell

### Login
```powershell
$body = @{
    username = "admin"
    password = "admin123"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri http://localhost:5001/api/auth/login `
    -Method POST `
    -ContentType "application/json" `
    -Body $body

$token = $response.token
```

### Listar Dispositivos
```powershell
$headers = @{
    "Authorization" = "Bearer $token"
}

Invoke-RestMethod -Uri http://localhost:5001/api/device `
    -Method GET `
    -Headers $headers
```

### Executar Comando
```powershell
$body = @{
    operation = "STATUS"
    parameters = @{}
} | ConvertTo-Json

Invoke-RestMethod -Uri http://localhost:5001/api/device/sensor-soil-001/execute `
    -Method POST `
    -Headers $headers `
    -ContentType "application/json" `
    -Body $body
```

---

**Última atualização**: Após implementação completa de componentes Angular
**Status**: ✅ Pronto para testes
