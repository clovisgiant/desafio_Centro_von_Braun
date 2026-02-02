// ===========================================================================================
// CONTROLLER DE AUTENTICAÇÃO - AuthController.cs
// ===========================================================================================
// Este controller gerencia todas as operações relacionadas à autenticação de usuários.
// Responsável por gerar tokens JWT e validar credenciais.
// ===========================================================================================

using CIoTDApi.Application.DTOs;
using CIoTDApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CIoTDApi.Presentation.Controllers;

/// <summary>
/// Controller para autenticação e login
/// </summary>
[ApiController]                      // Marca esta classe como um controller de API REST
[Route("api/[controller]")]         // Define a rota base: /api/Auth
[Produces("application/json")]      // Indica que este controller retorna JSON
public class AuthController : ControllerBase
{
    // Serviço que contém a lógica de autenticação (geração e validação de tokens)
    private readonly IAuthenticationService _authService;
    
    // Logger para registrar eventos e erros
    private readonly ILogger<AuthController> _logger;

    // Construtor - recebe as dependências via Dependency Injection (configurado no Program.cs)
    public AuthController(IAuthenticationService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    // ===========================================================================================
    // ENDPOINT: POST /api/Auth/login
    // ===========================================================================================
    // Este método recebe credenciais de usuário (username e password) e retorna um token JWT
    // se as credenciais forem válidas. O token JWT é usado para autenticar requisições futuras.
    // ===========================================================================================
    
    /// <summary>
    /// Autentica um usuário e retorna um token JWT
    /// </summary>
    /// <remarks>
    /// Usuários de teste:
    /// - admin / admin123
    /// - technician / tech456
    /// - researcher / research789
    /// </remarks>
    [HttpPost("login")]  // Define que este método responde a POST em /api/Auth/login
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]         // Sucesso (200)
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]        // Credenciais inválidas (401)
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        // Valida se username e password foram fornecidos
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            // Retorna HTTP 400 (Bad Request) se os dados estiverem incompletos
            return BadRequest(new { message = "Username e password são obrigatórios" });
        }

        // Chama o serviço de autenticação para verificar as credenciais
        var result = await _authService.AuthenticateAsync(request.Username, request.Password, cancellationToken);

        // Se result for null, significa que as credenciais são inválidas
        if (result == null)
        {
            // Registra a tentativa de login falha no log
            _logger.LogWarning("Falha de autenticação para usuário: {Username}", request.Username);
            
            // Retorna HTTP 401 (Unauthorized)
            return Unauthorized(new { message = "Credenciais inválidas" });
        }

        // Retorna HTTP 200 (OK) com o token JWT e informações do usuário
        return Ok(result);
    }

    // ===========================================================================================
    // ENDPOINT: POST /api/Auth/validate
    // ===========================================================================================
    // Este método valida se um token JWT ainda é válido (não expirou e tem assinatura correta)
    // Útil para verificar a sessão do usuário sem fazer login novamente
    // ===========================================================================================
    
    /// <summary>
    /// Valida um token JWT
    /// </summary>
    [HttpPost("validate")]  // Define que este método responde a POST em /api/Auth/validate
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]          // Token válido (200)
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)] // Token inválido (401)
    public IActionResult Validate([FromHeader(Name = "Authorization")] string? authorization)
    {
        // Verifica se o header Authorization foi enviado
        if (string.IsNullOrWhiteSpace(authorization))
        {
            return Unauthorized(new { message = "Authorization header não fornecido" });
        }

        // Remove o prefixo "Bearer " do token (formato padrão: "Bearer <token>")
        var token = authorization.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

        // Chama o serviço para validar o token (verifica assinatura, expiração, etc)
        if (!_authService.ValidateToken(token))
        {
            // Retorna HTTP 401 se o token for inválido ou expirado
            return Unauthorized(new { message = "Token inválido ou expirado" });
        }

        // Extrai o nome de usuário do token
        var username = _authService.GetUsernameFromToken(token);
        
        // Retorna HTTP 200 com o username e confirmação de que o token é válido
        return Ok(new { username, valid = true });
    }
}
