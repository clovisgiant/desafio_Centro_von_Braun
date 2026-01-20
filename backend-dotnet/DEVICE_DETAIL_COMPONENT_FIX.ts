// ===============================================
// DEVICE-DETAIL.COMPONENT.TS - CÓDIGO CORRETO
// ===============================================
// Copie TUDO isto e cole no seu arquivo:
// frontend-angular/ciotd-frontend/src/app/components/device-detail/device-detail.component.ts

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';

interface Device {
  identifier: string;
  description: string;
  manufacturer: string;
  url: string;
  commands: any[];
}

interface CommandExecutionResultDto {
  success: boolean;
  response?: string;
  error?: string;
}

@Component({
  selector: 'app-device-detail',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './device-detail.component.html',
  styleUrls: ['./device-detail.component.css']
})
export class DeviceDetailComponent implements OnInit {
  deviceId: string | null = null;
  device: Device | null = null;
  selectedOperation: string = '';
  parameters: { [key: string]: string } = {};
  executionResult: CommandExecutionResultDto | null = null;
  errorMessage: string = '';
  successMessage: string = '';
  loading: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    // AQUI É O PONTO CRÍTICO: capturar o 'id' da rota
    this.route.paramMap.subscribe(params => {
      this.deviceId = params.get('id');
      console.log('🔍 Device ID capturado da rota:', this.deviceId);
      
      if (this.deviceId) {
        this.loadDeviceDetails();
      } else {
        this.errorMessage = 'ID do dispositivo não encontrado na URL';
        console.error('❌ Device ID é nulo!');
      }
    });
  }

  loadDeviceDetails(): void {
    if (!this.deviceId) {
      console.error('Não é possível carregar: deviceId é nulo');
      return;
    }

    this.http.get<Device>(`/api/device/${this.deviceId}`).subscribe({
      next: (data) => {
        this.device = data;
        console.log('✅ Dispositivo carregado:', data);
        this.errorMessage = '';
      },
      error: (err) => {
        this.errorMessage = `Erro ao carregar dispositivo: ${err.statusText}`;
        console.error('❌ Erro ao carregar device:', err);
      }
    });
  }

  // ESTE É O MÉTODO CRÍTICO QUE ESTAVA FALHANDO
  executeCommand(): void {
    // Validação: deviceId DEVE existir
    if (!this.deviceId) {
      this.errorMessage = '❌ Erro: Device ID não definido. Verifique a URL ou recarregue a página.';
      console.error('Device ID é nulo em executeCommand()');
      alert(this.errorMessage);
      return;
    }

    if (!this.selectedOperation) {
      this.errorMessage = 'Selecione uma operação';
      return;
    }

    this.loading = true;
    this.executionResult = null;
    this.successMessage = '';

    // CONSTRUIR URL COM deviceId VÁLIDO (não será undefined)
    const url = `/api/device/${this.deviceId}/execute`;
    console.log('📤 Enviando POST para:', url);
    console.log('📋 Operação:', this.selectedOperation);
    console.log('📝 Parâmetros:', this.parameters);

    const payload = {
      operation: this.selectedOperation,
      parameters: this.parameters
    };

    this.http.post<CommandExecutionResultDto>(url, payload).subscribe({
      next: (result) => {
        this.executionResult = result;
        this.loading = false;
        
        if (result.success) {
          this.successMessage = `✅ Comando executado com sucesso!\nResposta: ${result.response}`;
          this.errorMessage = '';
        } else {
          this.errorMessage = `❌ Erro na execução: ${result.error}`;
        }
        console.log('✅ Resultado:', result);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = `❌ Erro HTTP ${err.status}: ${err.statusText}\n${err.error?.message || err.message}`;
        console.error('❌ Erro na requisição:', err);
      }
    });
  }

  onParameterChange(paramName: string, value: string): void {
    this.parameters[paramName] = value;
    console.log(`Parâmetro '${paramName}' atualizado para:`, value);
  }

  onOperationChange(operation: string): void {
    this.selectedOperation = operation;
    this.parameters = {};
    console.log('Operação selecionada:', operation);
  }
}
