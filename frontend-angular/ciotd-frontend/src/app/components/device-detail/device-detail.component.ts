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
    this.route.params.subscribe((params) => {
      const deviceId = params["id"];
      console.log("Device ID from route:", deviceId);
      this.loadDevice(deviceId);
    });
  }

  loadDevice(deviceId: string) {
    console.log("Loading device:", deviceId);
    this.deviceService.getDevice(deviceId).subscribe({
      next: (device) => {
        console.log("Device loaded:", device);
        this.device = device;
        this.loading = false;
      },
      error: (err) => {
        console.error("Error loading device:", err);
        this.error = "Erro ao carregar dispositivo: " + err.message;
        this.loading = false;
      },
    });
  }

  executeCommand() {
    if (!this.device || !this.command) return;

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

    this.deviceService
      .executeCommand(this.device!.id, this.command, paramPairs)
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.result = response.response || "Comando executado com sucesso";
          } else {
            this.resultError = response.error || "Erro na execução";
          }
          this.executing = false;
        },
        error: (err) => {
          this.resultError = "Erro: " + err.message;
          this.executing = false;
        },
      });
  }

  back() {
    this.router.navigate(["/devices"]);
  }
}