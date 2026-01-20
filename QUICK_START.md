# ⚡ QUICK START - CIoTD Platform

## 🚀 Iniciar em 30 segundos

### 1. Verificar Status
```powershell
cd C:\Users\CLOVIS\Documents\desafio_Centro_von_Braun\backend-dotnet
docker compose ps
```

Esperado: 3 containers em "Up" status

### 2. Acessar Frontend
```
http://localhost:4200
```

### 3. Fazer Login
- **Usuário**: admin
- **Senha**: admin123

### 4. Explorar
- Ver dispositivos em grid responsivo
- Clicar em dispositivo para detalhes
- Executar comandos (ex: STATUS, GET, SET)
- Sair quando terminar

---

## 🎯 Funcionalidades Testadas

| Funcionalidade | Status | Endpoint |
|---|---|---|
| Login com JWT | ✅ | POST /api/auth/login |
| Listar Dispositivos | ✅ | GET /api/device |
| Detalhes Dispositivo | ✅ | GET /api/device/{id} |
| Executar Comando | ✅ | POST /api/device/{id}/execute |
| Logout | ✅ | Frontend |

---

## 📊 Serviços

| Serviço | Porta | Status | Detalhes |
|---|---|---|---|
| Frontend (Angular) | 4200 | ✅ | SPA standalone components |
| Backend (.NET) | 5001 | ✅ | Clean Architecture |
| Device Agent | 8001 | ✅ | Telnet async protocol |

---

## 🧪 Comandos Úteis

### Ver logs do frontend
```powershell
docker compose logs frontend -f
```

### Ver logs do backend
```powershell
docker compose logs backend -f
```

### Ver logs do device agent
```powershell
docker compose logs device-agent -f
```

### Parar sistema
```powershell
docker compose down
```

### Reiniciar frontend (hot-reload)
```powershell
docker compose restart frontend
```

---

## 🔑 Credenciais

**Usuário**: `admin`
**Senha**: `admin123`

---

## 📱 Interface

### Página de Login
- Formulário com username/password
- Link de ajuda com credenciais
- Botão "Entrar" com loading state
- Feedback de erro em vermelho

### Página de Dispositivos
- Grid responsivo com 3 dispositivos
- Cards com nome e ID
- Hover effects
- Botão "Sair" no topo

### Página de Detalhes
- Info do dispositivo (nome, ID, localização)
- Painel de execução de comando
- Campo para comando
- Campo para parâmetros (opcionais)
- Resultado em verde (sucesso) ou vermelho (erro)
- Botão "Voltar"

---

## 🎨 Design

- **Cores**: Roxo gradient background (#667eea → #764ba2)
- **Tipografia**: Segoe UI, Geneva, Verdana
- **Responsive**: Grid 3 colunas → 1 coluna em mobile
- **Interativa**: Hover effects, loading states, feedback visual

---

## 🔍 Testes Manuais

### Teste 1: Login
1. Acessar http://localhost:4200
2. Ver formulário de login
3. Entrar com admin/admin123
4. ✅ Deve redirecionar para dispositivos

### Teste 2: Dispositivos
1. Após login
2. Ver 3 dispositivos em grid
3. Clicar em um dispositivo
4. ✅ Deve redirecionar para detalhes

### Teste 3: Comando
1. Na página de detalhes
2. Inserir comando: `STATUS`
3. Deixar parâmetros vazios
4. Clicar "Executar"
5. ✅ Deve exibir resultado

### Teste 4: Logout
1. Na página de dispositivos
2. Clicar "Sair"
3. ✅ Deve redirecionar para login

---

## 🛠️ Troubleshooting

### Erro: Página em branco
→ F12 → Console → Verificar erros
→ Limpar cache: Ctrl+Shift+Delete
→ Recarregar: Ctrl+F5

### Erro: 401 Unauthorized
→ Fazer login novamente
→ Verificar localStorage (F12 → Application)

### Erro: Cannot connect
→ Verificar: `docker compose ps`
→ Logs: `docker compose logs`

### Componentes não aparecem
→ Aguardar build (verificar Console)
→ Recarregar página
→ Reiniciar: `docker compose restart frontend`

---

## 📚 Documentação Completa

- **FRONTEND_COMPLETE.md** - Componentes Angular detalhados
- **TESTING_GUIDE.md** - Guia completo de testes
- **IMPLEMENTATION_COMPLETE.md** - Arquitetura e stack
- **COMPONENTS_REFERENCE.md** - Referência rápida de código

---

## ✅ Checklist

- [ ] Docker Compose rodando (`docker compose ps`)
- [ ] Frontend carregando (http://localhost:4200)
- [ ] Login funciona (admin/admin123)
- [ ] Dispositivos aparecem
- [ ] Pode clicar em dispositivo
- [ ] Pode executar comando
- [ ] Pode sair (logout)

---

## 🎉 Pronto!

Sistema totalmente funcional e testado.

**Acesso**: http://localhost:4200
**Login**: admin / admin123

Aproveite! 🚀
