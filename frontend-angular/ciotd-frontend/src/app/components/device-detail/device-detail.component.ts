import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { DeviceService } from "../../services/device.service";
import { Device } from "../../models/device.model";

@Component({
  selector: "app-device-detail",
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: "./device-detail.component.html",
  styleUrls: ["./device-detail.component.scss"],
})
export class DeviceDetailComponent implements OnInit {
  device: Device | null = null;
  loading = true;
  error = "";
  command = "";
  parameters = "";
  executing = false;
  result = "";
  resultError = "";

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private deviceService: DeviceService
  ) {}

  ngOnInit() {
    console.log('DeviceDetailComponent initialized');
    this.route.params.subscribe((params) => {
      const deviceId = params["id"];
      console.log("Device ID from route params:", deviceId);
      console.log("All route params:", params);
      
      if (!deviceId) {
        console.error('No device ID found in route params');
        this.error = "ID do dispositivo não fornecido na URL";
        this.loading = false;
        return;
      }
      
      this.loadDevice(deviceId);
    });
  }

  loadDevice(deviceId: string) {
    console.log("Loading device:", deviceId);
    this.loading = true;
    this.error = "";
    
    this.deviceService.getDevice(deviceId).subscribe({
      next: (device) => {
        console.log("Device loaded - Raw response:", device);
        console.log("Device type:", typeof device);
        console.log("Device keys:", Object.keys(device || {}));
        console.log("Device.identifier:", device?.identifier);
        console.log("Device.description:", device?.description);
        console.log("Device.manufacturer:", device?.manufacturer);
        
        this.device = device;
        console.log("this.device after assignment:", this.device);
        console.log("this.device.identifier after assignment:", this.device?.identifier);
        this.loading = false;
      },
      error: (err) => {
        console.error("Error loading device:", err);
        console.error("Error status:", err.status);
        console.error("Error details:", err.error);
        
        let errorMessage = "Erro ao carregar dispositivo";
        if (err.status === 404) {
          errorMessage = `Dispositivo "${deviceId}" não encontrado`;
        } else if (err.status === 401 || err.status === 403) {
          errorMessage = "Não autorizado. Faça login novamente.";
        } else if (err.error && err.error.message) {
          errorMessage = err.error.message;
        } else if (err.message) {
          errorMessage = err.message;
        }
        
        this.error = errorMessage;
        this.loading = false;
      },
    });
  }

  executeCommand() {
    if (!this.device || !this.command) {
      console.error('Cannot execute command: device or command is missing', {
        device: this.device,
        command: this.command
      });
      return;
    }

    const deviceId = this.device.identifier;
    if (!deviceId) {
      console.error('Device identifier is missing');
      this.resultError = "Erro: ID do dispositivo não encontrado";
      return;
    }

    this.executing = true;
    this.result = "";
    this.resultError = "";

    // Parse parameters from space-separated string into object
    const paramPairs: { [key: string]: string } = {};
    if (this.parameters.trim()) {
      const parts = this.parameters.split(" ").filter((p) => p.trim() !== "");
      parts.forEach((part, index) => {
        paramPairs[`param${index + 1}`] = part;
      });
    }

    console.log('Executing command:', {
      deviceId,
      command: this.command,
      parameters: paramPairs
    });

    this.deviceService
      .executeCommand(deviceId, this.command, paramPairs)
      .subscribe({
        next: (response) => {
          console.log('Command response:', response);
          if (response.success) {
            this.result = response.response || "Comando executado com sucesso";
          } else {
            this.resultError = response.error || "Erro na execução";
          }
          this.executing = false;
        },
        error: (err) => {
          console.error('Command execution error:', err);
          this.resultError = "Erro: " + (err.error?.message || err.message || 'Erro desconhecido');
          this.executing = false;
        },
      });
  }

  selectCommand(cmd: any) {
    this.command = cmd.operation;
    this.parameters = "";
    this.result = "";
    this.resultError = "";
    console.log('Comando selecionado:', cmd.operation);
  }

  back() {
    this.router.navigate(["/devices"]);
  }
}
