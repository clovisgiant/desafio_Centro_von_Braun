# Guia para Completar o Frontend Angular

## Estrutura Criada
-  `src/environments/environment.ts`
-  `src/app/models/device.model.ts`
-  `src/app/services/auth.service.ts`
-  `src/app/services/device.service.ts`

## Próximos Passos

### 1. Configurar HttpClient no app.config.ts
```typescript
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient()
  ]
};
```

### 2. Criar componentes

#### Login Component
```bash
cd C:\Users\CLOVIS\Documents\desafio_Centro_von_Braun\frontend-angular\ciotd-frontend
docker exec -it desafio_centro_von_braun-frontend-1 /bin/bash -c "cd /usr/src/app && npx @angular/cli generate component components/login --skip-tests"
```

**login.component.ts:**
```typescript
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  username = '';
  password = '';
  error = '';
  loading = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onSubmit(): void {
    this.loading = true;
    this.error = '';

    this.authService.login(this.username, this.password).subscribe({
      next: () => {
        this.router.navigate(['/devices']);
      },
      error: (err) => {
        this.error = 'Usuário ou senha inválidos';
        this.loading = false;
      }
    });
  }
}
```

**login.component.html:**
```html
<div class="login-container">
  <div class="login-card">
    <h2>CIoTD Platform</h2>
    <form (ngSubmit)="onSubmit()">
      <div class="form-group">
        <label for="username">Usuário</label>
        <input
          type="text"
          id="username"
          [(ngModel)]="username"
          name="username"
          required
          placeholder="Digite seu usuário"
        />
      </div>
      <div class="form-group">
        <label for="password">Senha</label>
        <input
          type="password"
          id="password"
          [(ngModel)]="password"
          name="password"
          required
          placeholder="Digite sua senha"
        />
      </div>
      <div *ngIf="error" class="error">{{ error }}</div>
      <button type="submit" [disabled]="loading">
        {{ loading ? "Carregando..." : "Entrar" }}
      </button>
    </form>
    <div class="hint">
      <p>Use: admin / admin123</p>
    </div>
  </div>
</div>
```

**login.component.scss:**
```scss
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.login-card {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 400px;

  h2 {
    text-align: center;
    margin-bottom: 2rem;
    color: #333;
  }

  .form-group {
    margin-bottom: 1rem;

    label {
      display: block;
      margin-bottom: 0.5rem;
      color: #555;
      font-weight: 500;
    }

    input {
      width: 100%;
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 1rem;
      box-sizing: border-box;

      &:focus {
        outline: none;
        border-color: #667eea;
      }
    }
  }

  .error {
    color: #e74c3c;
    margin-bottom: 1rem;
    font-size: 0.875rem;
  }

  button {
    width: 100%;
    padding: 0.75rem;
    background: #667eea;
    color: white;
    border: none;
    border-radius: 4px;
    font-size: 1rem;
    cursor: pointer;
    transition: background 0.3s;

    &:hover:not(:disabled) {
      background: #5568d3;
    }

    &:disabled {
      background: #ccc;
      cursor: not-allowed;
    }
  }

  .hint {
    margin-top: 1rem;
    text-align: center;
    color: #777;
    font-size: 0.875rem;
  }
}
```

#### Device List Component
```bash
docker exec -it desafio_centro_von_braun-frontend-1 /bin/bash -c "cd /usr/src/app && npx @angular/cli generate component components/device-list --skip-tests"
```

**device-list.component.ts:**
```typescript
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DeviceService } from '../../services/device.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-device-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './device-list.component.html',
  styleUrl: './device-list.component.scss'
})
export class DeviceListComponent implements OnInit {
  deviceIds: string[] = [];
  loading = true;
  error = '';

  constructor(
    private deviceService: DeviceService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadDevices();
  }

  loadDevices(): void {
    this.deviceService.getAllDevices().subscribe({
      next: (devices) => {
        this.deviceIds = devices;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erro ao carregar dispositivos';
        this.loading = false;
      }
    });
  }

  viewDevice(deviceId: string): void {
    this.router.navigate(['/device', deviceId]);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
```

**device-list.component.html:**
```html
<div class="container">
  <div class="header">
    <h1>Dispositivos IoT</h1>
    <button class="logout-btn" (click)="logout()">Sair</button>
  </div>

  <div *ngIf="loading" class="loading">Carregando...</div>
  <div *ngIf="error" class="error">{{ error }}</div>

  <div class="device-grid" *ngIf="!loading && !error">
    <div
      class="device-card"
      *ngFor="let deviceId of deviceIds"
      (click)="viewDevice(deviceId)"
    >
      <h3>{{ deviceId }}</h3>
      <p>Clique para ver detalhes</p>
    </div>
  </div>
</div>
```

**device-list.component.scss:**
```scss
.container {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;

  h1 {
    color: #333;
  }

  .logout-btn {
    padding: 0.5rem 1rem;
    background: #e74c3c;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;

    &:hover {
      background: #c0392b;
    }
  }
}

.loading,
.error {
  text-align: center;
  padding: 2rem;
  font-size: 1.2rem;
}

.error {
  color: #e74c3c;
}

.device-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  gap: 1.5rem;
}

.device-card {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s;

  &:hover {
    transform: translateY(-4px);
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
  }

  h3 {
    margin: 0 0 0.5rem 0;
    color: #667eea;
  }

  p {
    margin: 0;
    color: #777;
    font-size: 0.875rem;
  }
}
```

#### Device Detail Component
```bash
docker exec -it desafio_centro_von_braun-frontend-1 /bin/bash -c "cd /usr/src/app && npx @angular/cli generate component components/device-detail --skip-tests"
```

**device-detail.component.ts:**
```typescript
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { DeviceService } from '../../services/device.service';
import { Device, CommandDescription, ExecuteCommandResponse } from '../.. /models/device.model';

@Component({
  selector: 'app-device-detail',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './device-detail.component.html',
  styleUrl: './device-detail.component.scss'
})
export class DeviceDetailComponent implements OnInit {
  device: Device | null = null;
  selectedCommand: CommandDescription | null = null;
  parameters: { [key: string]: any } = {};
  result: ExecuteCommandResponse | null = null;
  loading = true;
  executing = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private deviceService: DeviceService
  ) {}

  ngOnInit(): void {
    const deviceId = this.route.snapshot.paramMap.get('id');
    if (deviceId) {
      this.loadDevice(deviceId);
    }
  }

  loadDevice(deviceId: string): void {
    this.deviceService.getDevice(deviceId).subscribe({
      next: (device) => {
        this.device = device;
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
      }
    });
  }

  selectCommand(command: CommandDescription): void {
    this.selectedCommand = command;
    this.parameters = {};
    this.result = null;

    // Inicializar parâmetros
    command.command.parameters.forEach(param => {
      this.parameters[param.name] = '';
    });
  }

  executeCommand(): void {
    if (!this.device || !this.selectedCommand) return;

    this.executing = true;
    this.result = null;

    this.deviceService.executeCommand(
      this.device.identifier,
      this.selectedCommand.operation,
      this.parameters
    ).subscribe({
      next: (response) => {
        this.result = response;
        this.executing = false;
      },
      error: (err) => {
        this.result = {
          success: false,
          error: 'Erro ao executar comando'
        };
        this.executing = false;
      }
    });
  }

  back(): void {
    this.router.navigate(['/devices']);
  }
}
```

**device-detail.component.html:**
```html
<div class="container" *ngIf="device">
  <button class="back-btn" (click)="back()"> Voltar</button>

  <div class="device-header">
    <h1>{{ device.identifier }}</h1>
    <div class="device-info">
      <p><strong>Fabricante:</strong> {{ device.manufacturer }}</p>
      <p><strong>Descrição:</strong> {{ device.description }}</p>
      <p><strong>URL:</strong> {{ device.url }}</p>
    </div>
  </div>

  <div class="commands-section">
    <h2>Comandos Disponíveis</h2>
    <div class="commands-grid">
      <div
        class="command-card"
        *ngFor="let cmd of device.commands"
        [class.selected]="selectedCommand === cmd"
        (click)="selectCommand(cmd)"
      >
        <h3>{{ cmd.operation }}</h3>
        <p>{{ cmd.description }}</p>
      </div>
    </div>
  </div>

  <div class="execution-section" *ngIf="selectedCommand">
    <h2>Executar: {{ selectedCommand.operation }}</h2>
    <p>{{ selectedCommand.description }}</p>

    <div class="parameters" *ngIf="selectedCommand.command.parameters.length > 0">
      <h3>Parâmetros</h3>
      <div class="param-group" *ngFor="let param of selectedCommand.command.parameters">
        <label>{{ param.name }}</label>
        <input
          type="text"
          [(ngModel)]="parameters[param.name]"
          [placeholder]="param.description"
        />
      </div>
    </div>

    <button class="execute-btn" (click)="executeCommand()" [disabled]="executing">
      {{ executing ? 'Executando...' : 'Executar Comando' }}
    </button>

    <div class="result" *ngIf="result">
      <h3>Resultado</h3>
      <div [class.success]="result.success" [class.error]="!result.success">
        <p *ngIf="result.success"><strong>Sucesso!</strong></p>
        <p *ngIf="result.response">{{ result.response }}</p>
        <p *ngIf="result.error" class="error-msg">{{ result.error }}</p>
      </div>
      <div class="format-info">
        <p><strong>Formato esperado:</strong> {{ selectedCommand.result.format }}</p>
        <p><strong>Descrição:</strong> {{ selectedCommand.result.description }}</p>
      </div>
    </div>
  </div>
</div>

<div *ngIf="loading" class="loading">Carregando...</div>
```

**device-detail.component.scss:**
```scss
.container {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.back-btn {
  padding: 0.5rem 1rem;
  background: #667eea;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  margin-bottom: 1rem;

  &:hover {
    background: #5568d3;
  }
}

.device-header {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  margin-bottom: 2rem;

  h1 {
    margin: 0 0 1rem 0;
    color: #333;
  }

  .device-info {
    p {
      margin: 0.5rem 0;
      color: #555;
    }
  }
}

.commands-section {
  margin-bottom: 2rem;

  h2 {
    margin-bottom: 1rem;
    color: #333;
  }
}

.commands-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 1rem;
}

.command-card {
  background: white;
  padding: 1rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  cursor: pointer;
  transition: all 0.2s;

  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
  }

  &.selected {
    border: 2px solid #667eea;
    background: #f0f4ff;
  }

  h3 {
    margin: 0 0 0.5rem 0;
    color: #667eea;
    font-size: 1rem;
  }

  p {
    margin: 0;
    color: #777;
    font-size: 0.875rem;
  }
}

.execution-section {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);

  h2 {
    margin: 0 0 1rem 0;
    color: #333;
  }

  .parameters {
    margin: 1.5rem 0;

    h3 {
      margin-bottom: 1rem;
      color: #555;
    }

    .param-group {
      margin-bottom: 1rem;

      label {
        display: block;
        margin-bottom: 0.5rem;
        color: #555;
        font-weight: 500;
      }

      input {
        width: 100%;
        padding: 0.75rem;
        border: 1px solid #ddd;
        border-radius: 4px;
        font-size: 1rem;
        box-sizing: border-box;

        &:focus {
          outline: none;
          border-color: #667eea;
        }
      }
    }
  }

  .execute-btn {
    padding: 0.75rem 1.5rem;
    background: #27ae60;
    color: white;
    border: none;
    border-radius: 4px;
    font-size: 1rem;
    cursor: pointer;

    &:hover:not(:disabled) {
      background: #229954;
    }

    &:disabled {
      background: #ccc;
      cursor: not-allowed;
    }
  }

  .result {
    margin-top: 1.5rem;
    padding: 1rem;
    background: #f8f9fa;
    border-radius: 4px;

    h3 {
      margin: 0 0 1rem 0;
      color: #333;
    }

    .success {
      color: #27ae60;
    }

    .error {
      color: #e74c3c;
    }

    .error-msg {
      font-weight: bold;
    }

    .format-info {
      margin-top: 1rem;
      padding-top: 1rem;
      border-top: 1px solid #ddd;

      p {
        margin: 0.5rem 0;
        color: #555;
        font-size: 0.875rem;
      }
    }
  }
}

.loading {
  text-align: center;
  padding: 2rem;
  font-size: 1.2rem;
}
```

### 3. Configurar Rotas (app.routes.ts)
```typescript
import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { DeviceListComponent } from './components/device-list/device-list.component';
import { DeviceDetailComponent } from './components/device-detail/device-detail.component';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'devices', component: DeviceListComponent },
  { path: 'device/:id', component: DeviceDetailComponent }
];
```

### 4. Atualizar app.component.html
```html
<router-outlet />
```

### 5. Atualizar app.component.ts
```typescript
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'ciotd-frontend';
}
```

### 6. Limpar estilos globais (src/styles.scss)
```scss
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
  background: #f5f5f5;
}
```

## Teste da Aplicação

1. Reiniciar frontend:
```powershell
docker compose restart frontend
```

2. Acessar: http://localhost:4200

3. Login: `admin` / `admin123`

4. Navegar pelos dispositivos e executar comandos

## Observações
- Todos os arquivos TypeScript foram criados com encoding UTF-8 sem BOM
- O proxy `/api` está configurado para rotear ao backend
- JWT é armazenado no localStorage
- Componentes Angular 17 standalone (sem módulos)
