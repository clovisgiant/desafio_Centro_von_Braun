# 📋 Referência Rápida - Componentes Angular

## 🔐 AuthService
**Localização**: `src/app/services/auth.service.ts`

```typescript
export class AuthService {
  private apiUrl = environment.apiUrl;

  login(username: string, password: string): Observable<AuthResponse>
  logout(): void                                  // Limpa token
  getToken(): string | null                       // Retorna JWT
  isAuthenticated(): boolean                      // Verifica login
}
```

## 📱 DeviceService
**Localização**: `src/app/services/device.service.ts`

```typescript
export class DeviceService {
  getAllDevices(): Observable<string[]>
  getDevice(deviceId: string): Observable<Device>
  executeCommand(deviceId: string, operation: string, 
                parameters: {[key: string]: any}): Observable<ExecuteCommandResponse>
}
```

## 🔑 LoginComponent
**Localização**: `src/app/components/login/`

**Template**: Formulário com campos username/password
**Lógica**: 
- onSubmit() → Autentica com backend
- Armazena token
- Redireciona para /devices

**Estilos**: Card centralizado com gradient background

## 📋 DeviceListComponent
**Localização**: `src/app/components/device-list/`

**Template**: 
- Header com título e botão logout
- Grid responsivo de dispositivos
- Cards clicáveis

**Lógica**:
- ngOnInit() → Carrega dispositivos
- selectDevice() → Navega para detalhe
- logout() → Faz logout

**Estilos**: Grid layout com cards hover

## 🔧 DeviceDetailComponent
**Localização**: `src/app/components/device-detail/`

**Template**:
- Informações do dispositivo (nome, ID, localização)
- Painel de execução de comando
- Exibição de resultado

**Lógica**:
- ngOnInit() → Carrega dispositivo
- executeCommand() → Envia comando para backend
- back() → Volta para lista

**Estilos**: Layout com painel de comando

---

## 🛣️ Rotas Disponíveis

| Rota | Componente | Autenticação |
|------|-----------|--------------|
| `/` | Redirect → /login | Não |
| `/login` | LoginComponent | Não |
| `/devices` | DeviceListComponent | ✅ Requerida |
| `/device/:id` | DeviceDetailComponent | ✅ Requerida |

---

## 📦 Modelos TypeScript

```typescript
// Device
interface Device {
  id: string;
  name: string;
  location?: string;
}

// Command
interface Command {
  name: string;
  parameters?: string[];
}

// Request/Response
interface ExecuteCommandRequest {
  operation: string;
  parameters: {[key: string]: any};
}

interface ExecuteCommandResponse {
  success: boolean;
  response?: string;
  error?: string;
}

// Auth
interface AuthResponse {
  token: string;
  expiresIn?: number;
}
```

---

## 🎨 Estilos Globais

**Arquivo**: `src/styles.scss`

```scss
// Cores
$primary: #667eea;
$danger: #e74c3c;
$success: #27ae60;
$background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);

// Fonts
$font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;

// Breakpoints
$mobile: 600px;
$tablet: 768px;
$desktop: 1024px;
```

---

## 🔌 Endpoints da API

### Autenticação
```
POST /api/auth/login
  Request: { username: string, password: string }
  Response: { token: string }
```

### Dispositivos
```
GET /api/device
  Response: string[]

GET /api/device/{id}
  Response: Device

POST /api/device/{id}/execute
  Request: { operation: string, parameters: {} }
  Response: { success: boolean, response?: string, error?: string }
```

---

## 🧪 Exemplos de Uso

### Fazer Login
```typescript
constructor(private authService: AuthService) {}

login() {
  this.authService.login('admin', 'admin123').subscribe(
    (response) => {
      console.log('Token:', response.token);
      // Redirecionado automaticamente para /devices
    },
    (error) => {
      console.error('Login failed:', error);
    }
  );
}
```

### Listar Dispositivos
```typescript
constructor(private deviceService: DeviceService) {}

ngOnInit() {
  this.deviceService.getAllDevices().subscribe(
    (devices) => {
      console.log('Dispositivos:', devices);
      // ['sensor-soil-001', 'sensor-weather-001', ...]
    }
  );
}
```

### Executar Comando
```typescript
executeCommand() {
  this.deviceService.executeCommand(
    'sensor-soil-001',
    'STATUS',
    { param1: 'value1' }
  ).subscribe(
    (result) => {
      if (result.success) {
        console.log('Resultado:', result.response);
      } else {
        console.error('Erro:', result.error);
      }
    }
  );
}
```

---

## 📂 Estrutura de Pastas Criada

```
frontend-angular/ciotd-frontend/src/app/
├── components/
│   ├── device-detail/
│   │   ├── device-detail.component.ts
│   │   ├── device-detail.component.html
│   │   └── device-detail.component.scss
│   ├── device-list/
│   │   ├── device-list.component.ts
│   │   ├── device-list.component.html
│   │   └── device-list.component.scss
│   └── login/
│       ├── login.component.ts
│       ├── login.component.html
│       └── login.component.scss
├── models/
│   └── device.model.ts
├── services/
│   ├── auth.service.ts
│   └── device.service.ts
├── app.component.ts
├── app.component.html
├── app.config.ts
└── app.routes.ts

src/
├── main.ts
├── styles.scss
└── environments/
    └── environment.ts
```

---

## 🔄 Fluxo de Componentes

```
AppComponent (Root)
│
└─ RouterOutlet
   │
   ├─ LoginComponent (rota: /login)
   │  └─ AuthService
   │
   ├─ DeviceListComponent (rota: /devices)
   │  ├─ AuthService
   │  └─ DeviceService
   │
   └─ DeviceDetailComponent (rota: /device/:id)
      ├─ AuthService
      └─ DeviceService
```

---

## 🚀 Deployment

### Build para Produção
```bash
ng build --configuration production
```

### Docker Build
```dockerfile
FROM node:20 AS builder
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN ng build --configuration production

FROM nginx:alpine
COPY --from=builder /app/dist/ciotd-frontend /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

---

## ✅ Checklist de Implementação

- [x] LoginComponent implementado
- [x] DeviceListComponent implementado
- [x] DeviceDetailComponent implementado
- [x] AuthService com JWT
- [x] DeviceService com API calls
- [x] Modelos TypeScript criados
- [x] Routing configurado (app.routes.ts)
- [x] Providers configurados (app.config.ts)
- [x] Estilos globais aplicados
- [x] SCSS por componente
- [x] Proxy configurado (proxy.conf.js)
- [x] main.ts atualizado
- [x] app.component com RouterOutlet
- [x] Autenticação JWT integrada
- [x] Tratamento de erros implementado
- [x] Loading states implementados
- [x] Testes manuais realizados
- [x] Documentação completa

---

**Desenvolvido com Angular 17 Standalone Components**
**Status**: ✅ Pronto para Produção
