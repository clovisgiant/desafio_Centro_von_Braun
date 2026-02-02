// ===========================================================================================
// SERVIÇO DE AUTENTICAÇÃO - auth.service.ts
// ===========================================================================================
// Gerencia todo o processo de autenticação do usuário:
// - Fazer login (enviar credenciais para a API)
// - Armazenar token JWT no localStorage
// - Fornecer token para outros serviços
// - Fazer logout (remover token)
// ===========================================================================================

import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable, BehaviorSubject } from "rxjs";
import { tap } from "rxjs/operators";
import { environment } from "../../environments/environment";
import { LoginRequest, LoginResponse } from "../models/device.model";

// @Injectable com providedIn: 'root' torna este serviço um singleton disponível em toda a aplicação
@Injectable({
  providedIn: "root",
})
export class AuthService {
  // BehaviorSubject: mantém o estado atual do token e notifica observers quando muda
  // Inicializado com o token do localStorage (se existir)
  private tokenSubject = new BehaviorSubject<string | null>(this.getToken());
  
  // Observable público para outros componentes observarem mudanças no token
  public token$ = this.tokenSubject.asObservable();

  // Construtor: injeta HttpClient para fazer requisições HTTP
  constructor(private http: HttpClient) {}

  // ===========================================================================================
  // MÉTODO: login
  // ===========================================================================================
  // Envia credenciais para a API e armazena o token JWT retornado
  // ===========================================================================================
  login(username: string, password: string): Observable<LoginResponse> {
    // Monta a URL do endpoint de login
    const url = `${environment.apiUrl}/auth/login`;
    
    // Cria o objeto de requisição
    const request: LoginRequest = { username, password };

    // Faz POST para a API e processa a resposta
    return this.http.post<LoginResponse>(url, request).pipe(
      tap((response) => {
        // Quando receber a resposta, salva o token no localStorage
        localStorage.setItem("token", response.accessToken);
        
        // Atualiza o BehaviorSubject, notificando todos os observers
        this.tokenSubject.next(response.accessToken);
      })
    );
  }

  // ===========================================================================================
  // MÉTODO: logout
  // ===========================================================================================
  // Remove o token, efetivamente fazendo logout do usuário
  // ===========================================================================================
  logout(): void {
    // Remove o token do localStorage
    localStorage.removeItem("token");
    
    // Atualiza o BehaviorSubject para null
    this.tokenSubject.next(null);
  }

  // ===========================================================================================
  // MÉTODO: getToken
  // ===========================================================================================
  // Retorna o token atual do localStorage (ou null se não houver)
  // ===========================================================================================
  getToken(): string | null {
    return localStorage.getItem("token");
  }

  // ===========================================================================================
  // MÉTODO: isAuthenticated
  // ===========================================================================================
  // Verifica se o usuário está autenticado (possui token)
  // ===========================================================================================
  isAuthenticated(): boolean {
    // !! converte para boolean: string vira true, null vira false
    return !!this.getToken();
  }
}