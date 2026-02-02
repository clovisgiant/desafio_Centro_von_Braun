// ===========================================================================================
// MODELS (INTERFACES) - device.model.ts
// ===========================================================================================
// PONTO-CHAVE #1 PARA A ENTREVISTA:
// Models definem a ESTRUTURA DOS DADOS que vêm da API
// TypeScript usa essas interfaces para validar tipos e fornecer autocomplete
// Essas estruturas ESPELHAM os DTOs do backend C#
// ===========================================================================================

// Interface que define a estrutura de um comando de dispositivo
// Corresponde ao CommandDescriptionDto do backend
export interface DeviceCommand {
  operation: string;        // Nome da operação (ex: "READ_TEMPERATURE")
  description: string;      // Descrição do que o comando faz
  command: any;            // Detalhes técnicos do comando
  result: string;          // Formato do resultado esperado
  format: any;             // Schema de validação do formato
}

// Interface principal que define a estrutura de um dispositivo IoT
// Corresponde ao DeviceDto do backend
export interface Device {
  identifier: string;       // ID único do dispositivo (ex: "sensor-soil-001")
  description: string;      // Descrição do dispositivo
  manufacturer: string;     // Fabricante
  url: string;             // URL Telnet do dispositivo (ex: "telnet://192.168.1.100:23")
  commands: DeviceCommand[]; // Lista de comandos disponíveis
}

// Interface para requisição de execução de comando
// Usada quando o frontend envia comando para a API
export interface ExecuteCommandRequest {
  operation: string;                    // Nome da operação a executar
  parameters: { [key: string]: any };   // Parâmetros do comando (chave-valor)
}

// Interface para resposta de execução de comando
// Define o que a API retorna após executar um comando
export interface ExecuteCommandResponse {
  success: boolean;      // true se executou com sucesso
  response?: string;     // Resposta do dispositivo (opcional)
  error?: string;        // Mensagem de erro (opcional)
}

// Interface para requisição de login
export interface LoginRequest {
  username: string;      // Nome de usuário
  password: string;      // Senha
}

// Interface para resposta de login
// Define o que a API retorna após autenticação bem-sucedida
export interface LoginResponse {
  accessToken: string;   // Token JWT para autenticação
  tokenType: string;     // Tipo do token ("Bearer")
  expiresIn: number;     // Tempo de expiração em segundos
  userName: string;      // Nome do usuário autenticado
}