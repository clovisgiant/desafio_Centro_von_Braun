// ===========================================================================================
// SERVIÇO DE DISPOSITIVOS - device.service.ts
// ===========================================================================================
// Gerencia toda a comunicação com a API backend relacionada a dispositivos IoT:
// - Listar todos os dispositivos
// - Buscar detalhes de um dispositivo específico
// - Executar comandos em dispositivos
// ===========================================================================================

import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Device, ExecuteCommandRequest, ExecuteCommandResponse } from '../models/device.model';
import { AuthService } from './auth.service';

// Serviço singleton disponível em toda a aplicação
@Injectable({
  providedIn: 'root'
})
export class DeviceService {
  // Construtor: injeta HttpClient e AuthService
  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  // ===========================================================================================
  // MÉTODO PRIVADO: getHeaders
  // ===========================================================================================
  // Cria os headers HTTP com o token JWT para autenticação
  // ===========================================================================================
  private getHeaders(): HttpHeaders {
    // Obtém o token JWT do AuthService
    const token = this.authService.getToken();
    
    // Cria headers com Authorization Bearer e Content-Type JSON
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,    // Token JWT para autenticação
      'Content-Type': 'application/json'     // Formato do corpo da requisição
    });
  }

  // ===========================================================================================
  // MÉTODO: getAllDevices
  // ===========================================================================================
  // Retorna lista de IDs de todos os dispositivos cadastrados
  // GET /api/device
  // ===========================================================================================
  getAllDevices(): Observable<string[]> {
    const url = `${environment.apiUrl}/device`;
    return this.http.get<string[]>(url, { headers: this.getHeaders() });
  }

  // ===========================================================================================
  // MÉTODO: getDevice
  // ===========================================================================================
  // Retorna os detalhes completos de um dispositivo específico
  // GET /api/device/{deviceId}
  // ===========================================================================================
  getDevice(deviceId: string): Observable<Device> {
    // Codifica o ID para tratar caracteres especiais na URL (ex: sensor-soil-001)
    const encodedId = encodeURIComponent(deviceId);
    const url = `${environment.apiUrl}/device/${encodedId}`;
    
    // Log para debug
    console.log('Requesting device from URL:', url);
    
    return this.http.get<Device>(url, { headers: this.getHeaders() });
  }

  // ===========================================================================================
  // MÉTODO: executeCommand
  // ===========================================================================================
  // Executa um comando em um dispositivo IoT
  // POST /api/device/{deviceId}/execute
  // ===========================================================================================
  executeCommand(
    deviceId: string,                         // ID do dispositivo
    operation: string,                        // Nome da operação (ex: "READ_TEMPERATURE")
    parameters: { [key: string]: any }        // Parâmetros do comando
  ): Observable<ExecuteCommandResponse> {
    // Codifica o ID para URL
    const encodedId = encodeURIComponent(deviceId);
    const url = `${environment.apiUrl}/device/${encodedId}/execute`;
    
    // Monta o objeto de requisição
    const request: ExecuteCommandRequest = { 
      operation: operation, 
      parameters: parameters 
    };
    
    // Log para debug
    console.log('Executing command on URL:', url, 'with request:', request);
    
    // Faz POST com o comando e retorna o resultado
    return this.http.post<ExecuteCommandResponse>(url, request, { headers: this.getHeaders() });
  }
}