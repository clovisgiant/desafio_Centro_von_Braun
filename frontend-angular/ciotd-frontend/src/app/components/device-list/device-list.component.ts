import { Component, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { Router } from "@angular/router";
import { DeviceService } from "../../services/device.service";
import { AuthService } from "../../services/auth.service";

@Component({
  selector: "app-device-list",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./device-list.component.html",
  styleUrls: ["./device-list.component.scss"],
})
export class DeviceListComponent implements OnInit {
  devices: string[] = [];
  loading = true;
  error = "";

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
        console.log("Dispositivos carregados:", devices);
        this.devices = devices;
        this.loading = false;
      },
      error: (err) => {
        console.error("Erro ao carregar dispositivos:", err);
        this.error = "Erro ao carregar dispositivos";
        this.loading = false;
      },
    });
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

  logout(): void {
    this.authService.logout();
    this.router.navigate(["/login"]);
  }
}