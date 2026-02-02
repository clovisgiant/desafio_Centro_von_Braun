// ===========================================================================================
// COMPONENTE DE LOGIN - login.component.ts
// ===========================================================================================
// PONTO-CHAVE #2 PARA A ENTREVISTA:
// Este componente demonstra o FLUXO BÁSICO de vinculação de dados da API:
// 1. Injeção do Service (AuthService)
// 2. Chamada do método do service
// 3. Subscribe para receber dados assíncronos (Observable pattern)
// 4. Tratamento de sucesso (next) e erro (error)
// ===========================================================================================

import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';  // Necessário para [(ngModel)] - two-way binding
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  // ===========================================================================================
  // PROPRIEDADES DO COMPONENTE
  // ===========================================================================================
  // Estas propriedades são vinculadas ao template HTML via data binding
  
  username = '';    // Vinculado ao input de username no template
  password = '';    // Vinculado ao input de password no template
  error = '';       // Vinculado para exibir mensagens de erro
  loading = false;  // Vinculado para mostrar/ocultar spinner de loading

  // ===========================================================================================
  // INJEÇÃO DE DEPENDÊNCIAS (Constructor Injection)
  // ===========================================================================================
  // PONTO-CHAVE: Angular injeta automaticamente os serviços necessários
  
  constructor(
    private authService: AuthService,  // Serviço que faz a chamada HTTP à API
    private router: Router             // Serviço de navegação entre rotas
  ) {}

  // ===========================================================================================
  // MÉTODO: onSubmit
  // ===========================================================================================
  // PONTO-CHAVE: Aqui acontece a VINCULAÇÃO COM A API!
  // Este método é chamado quando o usuário clica no botão de login
  // ===========================================================================================
  onSubmit(): void {
    // PASSO 1: Prepara o estado visual (loading, limpa erros)
    this.loading = true;
    this.error = '';

    // PASSO 2: Chama o método login do AuthService
    // login() retorna um Observable<LoginResponse>
    // Observable é o padrão do Angular para operações assíncronas (como Promises)
    this.authService.login(this.username, this.password).subscribe({
      
      // PASSO 3a: CALLBACK DE SUCESSO (next)
      // Executado quando a API retorna sucesso (HTTP 200)
      next: () => {
        // Navega para a página de dispositivos
        this.router.navigate(['/devices']);
      },
      
      // PASSO 3b: CALLBACK DE ERRO (error)
      // Executado quando a API retorna erro (HTTP 401, 500, etc)
      error: (err) => {
        // Atualiza a propriedade 'error' que está vinculada ao template
        this.error = 'Usuário ou senha inválidos';
        this.loading = false;
      }
    });
    
    // NOTA: O Subscribe é essencial! Sem ele, a requisição HTTP NÃO é enviada
    // Observables são "lazy" - só executam quando alguém se inscreve (subscribe)
  }
}