import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Device, ExecuteCommandRequest, ExecuteCommandResponse } from '../models/device.model';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class DeviceService {
  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  private getHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  getAllDevices(): Observable<string[]> {
    const url = `${environment.apiUrl}/device`;
    return this.http.get<string[]>(url, { headers: this.getHeaders() });
  }

  getDevice(deviceId: string): Observable<Device> {
    // Encode the deviceId to handle special characters
    const encodedId = encodeURIComponent(deviceId);
    const url = `${environment.apiUrl}/device/${encodedId}`;
    console.log('Requesting device from URL:', url);
    return this.http.get<Device>(url, { headers: this.getHeaders() });
  }

  executeCommand(
    deviceId: string,
    operation: string,
    parameters: { [key: string]: any }
  ): Observable<ExecuteCommandResponse> {
    // Encode the deviceId to handle special characters
    const encodedId = encodeURIComponent(deviceId);
    const url = `${environment.apiUrl}/device/${encodedId}/execute`;
    const request: ExecuteCommandRequest = { operation, parameters };
    console.log('Executing command on URL:', url, 'with request:', request);
    return this.http.post<ExecuteCommandResponse>(url, request, { headers: this.getHeaders() });
  }
}