# 🎯 Vinculação de Dados da API no Angular - Guia para Entrevista

## 📋 Visão Geral do Fluxo

```
┌─────────────┐      ┌──────────┐      ┌─────────┐      ┌──────────────┐
│  Component  │ ───> │ Service  │ ───> │   API   │ ───> │   Backend    │
│ (View Logic)│ <─── │  (HTTP)  │ <─── │ (.NET)  │ <─── │ (Database)   │
└─────────────┘      └──────────┘      └─────────┘      └──────────────┘
       │                                                         
       ↓                                                         
┌─────────────┐                                                 
│  Template   │  ← Data Binding                                
│   (HTML)    │                                                 
└─────────────┘                                                 
```

## 🔑 Pontos-Chave para a Entrevista

### 1️⃣ **Models/Interfaces** - Estrutura dos Dados
📁 `src/app/models/device.model.ts`

```typescript
// Define a ESTRUTURA dos dados que vêm da API
export interface Device {
  identifier: string;
  description: string;
  manufacturer: string;
  url: string;
  commands: DeviceCommand[];
}
```

**Por quê?**
- TypeScript valida os tipos
- Autocomplete no IDE
- Espelha os DTOs do backend C#
- Previne erros em tempo de desenvolvimento

---

### 2️⃣ **Services** - Comunicação com a API
📁 `src/app/services/device.service.ts`

```typescript
export class DeviceService {
  constructor(private http: HttpClient) {}

  // VINCULAÇÃO: Faz GET para a API
  getAllDevices(): Observable<string[]> {
    const url = `${environment.apiUrl}/device`;
    return this.http.get<string[]>(url, { headers: this.getHeaders() });
  }
}
```

**Pontos importantes:**
- `HttpClient` faz as requisições HTTP
- Retorna `Observable<T>` (padrão RxJS)
- Headers incluem token JWT para autenticação
- URL vem de `environment.apiUrl` (configurável)

---

### 3️⃣ **Components** - Lógica e Consumo de Dados
📁 `src/app/components/device-list/device-list.component.ts`

```typescript
export class DeviceListComponent implements OnInit {
  // PASSO 1: Declarar propriedade tipada
  devices: string[] = [];
  
  constructor(private deviceService: DeviceService) {}
  
  // PASSO 2: Carregar dados no lifecycle hook
  ngOnInit(): void {
    this.loadDevices();
  }
  
  // PASSO 3: AQUI ACONTECE A VINCULAÇÃO!
  loadDevices(): void {
    this.deviceService.getAllDevices().subscribe({
      next: (devices) => {
        // Atribui dados da API à propriedade
        this.devices = devices;  // ← VINCULAÇÃO ACONTECE AQUI
      },
      error: (err) => {
        console.error('Erro:', err);
      }
    });
  }
}
```

**Fluxo detalhado:**
1. Component injeta o Service via constructor
2. `ngOnInit()` é chamado automaticamente quando componente é criado
3. Chama método do service que retorna Observable
4. `.subscribe()` dispara a requisição HTTP
5. Callback `next` recebe dados da API
6. Atribui dados à propriedade do component
7. Angular detecta mudança e atualiza a view automaticamente

---

### 4️⃣ **Templates** - Exibição dos Dados
📁 `src/app/components/device-list/device-list.component.html`

```html
<!-- *ngFor itera sobre o array 'devices' -->
<div *ngFor="let device of devices" class="device-card">
  <h3>{{ device }}</h3>  <!-- {{ }} faz interpolação -->
</div>

<!-- *ngIf mostra/esconde elementos -->
<div *ngIf="loading">Carregando...</div>
<div *ngIf="error">{{ error }}</div>
```

**Data Binding:**
- `{{ device }}` - **Interpolação** (exibe valor)
- `*ngFor` - **Diretiva estrutural** (repete elemento)
- `*ngIf` - **Diretiva estrutural** (condicional)
- `(click)` - **Event binding** (escuta eventos)

---

## 🔄 Fluxo Completo Passo a Passo

### Exemplo: Carregar Lista de Dispositivos

#### 1. **Usuário acessa /devices**
```
Router navega para DeviceListComponent
```

#### 2. **Component é inicializado**
```typescript
constructor(private deviceService: DeviceService) {}
↓
ngOnInit() chamado automaticamente
↓
loadDevices() executado
```

#### 3. **Service faz requisição HTTP**
```typescript
deviceService.getAllDevices()
↓
HttpClient.get('http://localhost:5001/api/device')
↓
Headers: { Authorization: 'Bearer eyJhbGc...' }
```

#### 4. **Backend processa**
```
.NET API recebe GET /api/device
↓
DeviceController.GetAllDevices()
↓
DeviceService.GetAllDevicesAsync()
↓
Retorna: ["sensor-soil-001", "sensor-weather-001", "irrigation-system-001"]
```

#### 5. **Resposta volta para o Angular**
```typescript
.subscribe({
  next: (devices) => {
    // devices = ["sensor-soil-001", "sensor-weather-001", ...]
    this.devices = devices;  // ← VINCULAÇÃO!
    this.loading = false;
  }
})
```

#### 6. **Angular atualiza a View automaticamente**
```html
<!-- Angular detecta mudança em 'devices' -->
<div *ngFor="let device of devices">  ← Renderiza 3 cards
  <h3>{{ device }}</h3>
</div>
```

---

## 💡 Conceitos Importantes para Explicar

### **Observable vs Promise**
```typescript
// Promise (JavaScript tradicional)
fetch('/api/device').then(data => console.log(data));

// Observable (Angular/RxJS)
this.http.get('/api/device').subscribe(data => console.log(data));
```

**Diferenças:**
- Observable é **lazy** (só executa com subscribe)
- Observable permite **cancelamento** (unsubscribe)
- Observable permite **múltiplos valores** no tempo
- Observable tem **operadores poderosos** (map, filter, debounce, etc)

### **One-Way vs Two-Way Binding**

```typescript
// ONE-WAY: Component → Template
{{ device.name }}  // Exibe valor

// TWO-WAY: Component ↔ Template
[(ngModel)]="username"  // Sincroniza automaticamente
```

### **Lifecycle Hooks**
```typescript
ngOnInit()      // Após criar component (carregar dados aqui)
ngOnDestroy()   // Antes de destruir (limpar subscriptions)
ngOnChanges()   // Quando inputs mudam
```

---

## 📊 Exemplo Completo com Explicação

### Código do Component
```typescript
export class DeviceDetailComponent implements OnInit {
  // 1. PROPRIEDADES (vinculadas ao template)
  device: Device | null = null;
  loading = true;
  
  // 2. INJEÇÃO DE DEPENDÊNCIAS
  constructor(
    private route: ActivatedRoute,        // Pega parâmetros da URL
    private deviceService: DeviceService  // Service HTTP
  ) {}
  
  // 3. LIFECYCLE HOOK
  ngOnInit() {
    // Pega ID da URL (/device/sensor-001)
    this.route.params.subscribe(params => {
      const deviceId = params['id'];
      this.loadDevice(deviceId);
    });
  }
  
  // 4. CARREGA DADOS DA API
  loadDevice(deviceId: string) {
    this.deviceService.getDevice(deviceId).subscribe({
      next: (device) => {
        // VINCULAÇÃO: Dados da API → Propriedade do Component
        this.device = device;
        this.loading = false;
      },
      error: (err) => {
        console.error('Erro:', err);
        this.loading = false;
      }
    });
  }
  
  // 5. EXECUTA COMANDO NO DISPOSITIVO
  executeCommand(operation: string, params: any) {
    this.deviceService
      .executeCommand(this.device!.identifier, operation, params)
      .subscribe({
        next: (response) => {
          console.log('Resposta:', response.response);
        }
      });
  }
}
```

### Template Correspondente
```html
<!-- Mostra loading enquanto carrega -->
<div *ngIf="loading">Carregando...</div>

<!-- Mostra dispositivo quando carregado -->
<div *ngIf="device && !loading">
  <h2>{{ device.description }}</h2>
  <p>Fabricante: {{ device.manufacturer }}</p>
  
  <!-- Lista comandos disponíveis -->
  <div *ngFor="let cmd of device.commands">
    <button (click)="executeCommand(cmd.operation, {})">
      {{ cmd.description }}
    </button>
  </div>
</div>
```

---

## 🎤 Frases para Usar na Entrevista

1. **"No Angular, a vinculação de dados da API segue o padrão Observable do RxJS..."**

2. **"Eu injeto o service via dependency injection no constructor do componente..."**

3. **"Uso o lifecycle hook ngOnInit para carregar dados assim que o componente é criado..."**

4. **"O HttpClient retorna um Observable que precisa ser subscrito para disparar a requisição..."**

5. **"Quando os dados chegam no callback 'next', eu atribuo à propriedade do component e o Angular automaticamente atualiza a view através do change detection..."**

6. **"Uso interfaces TypeScript que espelham os DTOs do backend para garantir type safety..."**

7. **"O data binding do Angular permite que mudanças nas propriedades do component sejam refletidas automaticamente no template..."**

---

## 📝 Checklist para a Entrevista

✅ Explicar a arquitetura em camadas (Component → Service → API)  
✅ Demonstrar conhecimento de Observables e RxJS  
✅ Mencionar lifecycle hooks (especialmente ngOnInit)  
✅ Falar sobre dependency injection  
✅ Explicar data binding (interpolação, diretivas estruturais)  
✅ Mostrar tratamento de erros (callback error)  
✅ Mencionar TypeScript e type safety  
✅ Explicar a diferença entre Observable e Promise  

---

## 🔍 Perguntas Comuns em Entrevistas

**Q: Como você consome uma API REST no Angular?**
> Uso o HttpClient injetado via dependency injection. Crio um service que encapsula as chamadas HTTP e retorna Observables. No component, injeto o service e faço subscribe no Observable para receber os dados.

**Q: O que é um Observable?**
> É um padrão assíncrono do RxJS que representa um stream de dados no tempo. Diferente de Promises, Observables são lazy (só executam com subscribe), podem emitir múltiplos valores, e podem ser cancelados.

**Q: Quando você carrega dados da API?**
> Normalmente no ngOnInit, que é um lifecycle hook executado após o Angular criar o componente. Para dados que dependem de parâmetros da rota, uso ActivatedRoute.params.

**Q: Como você trata erros de API?**
> Uso o callback 'error' do subscribe para capturar erros HTTP. Também posso usar operadores RxJS como catchError para tratamento mais sofisticado.

---

## 🚀 Arquivos-Chave do Projeto

- **Models**: `device.model.ts` - Define estruturas de dados
- **Services**: `device.service.ts`, `auth.service.ts` - Comunicação HTTP
- **Components**: `device-list.component.ts` - Lógica de consumo
- **Templates**: `device-list.component.html` - Exibição dos dados

Boa sorte na entrevista! 🍀
