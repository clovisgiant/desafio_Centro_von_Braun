// ===========================================================================================
// ARQUIVO DE INICIALIZAÇÃO - main.ts
// ===========================================================================================
// Este é o ponto de entrada da aplicação Angular. Faz o bootstrap (inicialização) do
// componente raiz (AppComponent) com a configuração definida.
// ===========================================================================================

import { bootstrapApplication } from "@angular/platform-browser";
import { AppComponent } from "./app/app.component";
import { appConfig } from "./app/app.config";

// Inicializa a aplicação Angular com o componente raiz e configuração
// Se houver erro, exibe no console
bootstrapApplication(AppComponent, appConfig).catch((err) =>
  console.error(err)
);