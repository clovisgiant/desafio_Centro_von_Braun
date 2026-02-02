// ===========================================================================================
// CONFIGURAÇÃO DA APLICAÇÃO ANGULAR - app.config.ts
// ===========================================================================================
// Este arquivo define a configuração principal da aplicação Angular 17+ (standalone)
// Substitui o antigo app.module.ts em aplicações standalone
// ===========================================================================================

import { ApplicationConfig } from "@angular/core";
import { provideRouter } from "@angular/router";          // Provedor de rotas
import { provideHttpClient } from "@angular/common/http";  // Provedor de HttpClient para requisições HTTP

import { routes } from "./app.routes";  // Importa as rotas definidas

// Configuração da aplicação exportada para uso no bootstrap (main.ts)
export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),      // Registra o sistema de rotas do Angular
    provideHttpClient()         // Registra o HttpClient para fazer requisições HTTP à API
  ],
};