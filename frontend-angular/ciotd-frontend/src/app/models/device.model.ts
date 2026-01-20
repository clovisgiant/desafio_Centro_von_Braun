export interface DeviceCommand {
  operation: string;
  description: string;
  command: any;
  result: string;
  format: any;
}

export interface Device {
  identifier: string;
  description: string;
  manufacturer: string;
  url: string;
  commands: DeviceCommand[];
}

export interface ExecuteCommandRequest {
  operation: string;
  parameters: { [key: string]: any };
}

export interface ExecuteCommandResponse {
  success: boolean;
  response?: string;
  error?: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  tokenType: string;
  expiresIn: number;
  userName: string;
}