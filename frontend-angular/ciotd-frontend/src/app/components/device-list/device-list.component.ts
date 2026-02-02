// ===========================================================================================
// COMPONENTE DE LISTA DE DISPOSITIVOS - device-list.component.ts
// ===========================================================================================
// PONTO-CHAVE #3 PARA A ENTREVISTA:
// Este componente demonstra o CICLO COMPLETO de vinculação de dados:
// 1. Declaração de propriedades tipadas
// 2. Injeção de services via constructor
// 3. Inicialização de dados no ngOnInit (lifecycle hook)
// 4. Consumo de dados da API via Observable
// 5. Atualização de propriedades que automaticamente atualizam a view (data binding)
// ===========================================================================================

import { Component, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { Router } from "@angular/router";
import { DeviceService } from "../../services/device.service";
import { AuthService } from "../../services/auth.service";

@Component({
  selector: "app-device-list",
  standalone: true,
  imports: [CommonModule],  // CommonModule fornece *ngFor, *ngIf, etc
  templateUrl: "./device-list.component.html",
  styleUrls: ["./device-list.component.scss"],
})
export class DeviceListComponent implements OnInit {
  // ===========================================================================================
  // PROPRIEDADES DO COMPONENTE (Component State)
  // ===========================================================================================
  // PONTO-CHAVE: Estas propriedades são VINCULADAS ao template HTML
  // Quando você atualiza devices, o Angular automaticamente atualiza a view
  // Isso é chamado de "One-Way Data Binding" (Component -> Template)
  
  devices: string[] = [];  // Array de IDs de dispositivos vindos da API
  loading = true;          // Flag para mostrar loading spinner
  error = "";              // Mensagem de erro para exibir

  // ===========================================================================================
  // INJEÇÃO DE DEPENDÊNCIAS
  // ===========================================================================================
  // PONTO-CHAVE: Angular injeta automaticamente instâncias dos services
  
  constructor(
    private deviceService: DeviceService,  // Service que comunica com API de dispositivos
    private authService: AuthService,      // Service de autenticação
    private router: Router                 // Service de navegação
  ) {}

  // ===========================================================================================
  // LIFECYCLE HOOK: ngOnInit
  // ===========================================================================================
  // PONTO-CHAVE: ngOnInit é executado AUTOMATICAMENTE quando o componente é criado
  // É aqui que normalmente carregamos dados da API
  // Equivalente ao componentDidMount do React
  
  ngOnInit(): void {
    this.loadDevices();  // Carrega os dispositivos assim que a página é aberta
  }

  // ===========================================================================================
  // MÉTODO: loadDevices
  // ===========================================================================================
  // PONTO-CHAVE #4 PARA A ENTREVISTA - VINCULAÇÃO COMPLETA DE DADOS DA API
  // Este método demonstra TODO o fluxo de comunicação com a API:
  // ===========================================================================================
  
  loadDevices(): void {
    // PASSO 1: Chama o método getAllDevices() do DeviceService
    // getAllDevices() faz uma requisição HTTP GET para /api/device
    // Retorna Observable<string[]> - um stream assíncrono de dados
    
    this.deviceService.getAllDevices().subscribe({
      
      // PASSO 2a: CALLBACK next - Executado quando a API retorna sucesso
      // AQUI ACONTECE A VINCULAÇÃO! Os dados da API são atribuídos à propriedade
      next: (devices) => {
        // 'devices' contém a resposta da API (array de strings)
        console.log("Dispositivos carregados:", devices);
        
        // VINCULAÇÃO: Atualiza a propriedade do componente
        // Angular detecta a mudança e AUTOMATICAMENTE atualiza o template HTML
        // Isso dispara o *ngFor no template para renderizar a lista
        this.devices = devices;
        
        // Atualiza flags de estado
        this.loading = false;
      },
      
      // PASSO 2b: CALLBACK error - Executado quando há erro na API
      error: (err) => {
        console.error("Erro ao carregar dispositivos:", err);
        
        // Vincula mensagem de erro ao template
        this.error = "Erro ao carregar dispositivos";
        this.loading = false;
      },
    });
    
    // IMPORTANTE: O fluxo é ASSÍNCRONO
    // O código aqui continua executando enquanto aguarda a resposta da API
    // Quando a API responde, os callbacks (next/error) são executados
  }

  selectDevice(deviceId: string): void {
    console.log("Selecionando device com ID:", deviceId);
    console.log("Tipo do deviceId:", typeof deviceId);
    console.log("DeviceId é undefined?", deviceId === undefined);
    console.log("DeviceId é null?", deviceId === null);
    
    if (!deviceId) {
      console.error("Device ID está vazio ou undefined");
      return;
    }
    
    this.router.navigate(["/device", deviceId]);
  }

  getDeviceIcon(deviceId: string): string {
    if (deviceId.includes('soil')) return '🌱';
    if (deviceId.includes('weather')) return '🌤️';
    if (deviceId.includes('irrigation')) return '💧';
    return '📡';
  }

  getDeviceType(deviceId: string): string {
    if (deviceId.includes('soil')) return 'Sensor de Solo';
    if (deviceId.includes('weather')) return 'Estação Meteorológica';
    if (deviceId.includes('irrigation')) return 'Sistema de Irrigação';
    return 'Dispositivo IoT';
  }

  getDeviceColor(deviceId: string): string {
    if (deviceId.includes('soil')) return 'soil';
    if (deviceId.includes('weather')) return 'weather';
    if (deviceId.includes('irrigation')) return 'irrigation';
    return 'default';
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(["/login"]);
  }
}