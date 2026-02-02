// ===========================================================================================
// ROTAS DA APLICAÇÃO - app.routes.ts
// ===========================================================================================
// Define todas as rotas (URLs) da aplicação e qual componente renderizar em cada uma
// ===========================================================================================

import { Routes } from "@angular/router";
import { LoginComponent } from "./components/login/login.component";
import { DeviceListComponent } from "./components/device-list/device-list.component";
import { DeviceDetailComponent } from "./components/device-detail/device-detail.component";

export const routes: Routes = [
  // Rota padrão: redireciona a página inicial para /login
  { path: "", redirectTo: "/login", pathMatch: "full" },
  
  // Rota /login: exibe o componente de login
  { path: "login", component: LoginComponent },
  
  // Rota /devices: exibe a lista de dispositivos (após login)
  { path: "devices", component: DeviceListComponent },
  
  // Rota /device/:id: exibe os detalhes de um dispositivo específico
  // :id é um parâmetro dinâmico (ex: /device/sensor-soil-001)
  { path: "device/:id", component: DeviceDetailComponent },
];