export interface Device {
  id: string;
  name: string;
  location?: string;
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