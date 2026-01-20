import { Routes } from "@angular/router";
import { LoginComponent } from "./components/login/login.component";
import { DeviceListComponent } from "./components/device-list/device-list.component";
import { DeviceDetailComponent } from "./components/device-detail/device-detail.component";

export const routes: Routes = [
  { path: "", redirectTo: "/login", pathMatch: "full" },
  { path: "login", component: LoginComponent },
  { path: "devices", component: DeviceListComponent },
  { path: "device/:id", component: DeviceDetailComponent },
];