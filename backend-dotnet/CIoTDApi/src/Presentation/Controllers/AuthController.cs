using CIoTDApi.Application.DTOs;
using CIoTDApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CIoTDApi.Presentation.Controllers;

/// <summary>
/// Controller para autenticação e login
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthenticationService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Autentica um usuário e retorna um token JWT
    /// </summary>
    /// <remarks>
    /// Usuários de teste:
    /// - admin / admin123
    /// - technician / tech456
    /// - researcher / research789
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username e password são obrigatórios" });
        }

        var result = await _authService.AuthenticateAsync(request.Username, request.Password, cancellationToken);

        if (result == null)
        {
            _logger.LogWarning("Falha de autenticação para usuário: {Username}", request.Username);
            return Unauthorized(new { message = "Credenciais inválidas" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Valida um token JWT
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public IActionResult Validate([FromHeader(Name = "Authorization")] string? authorization)
    {
        if (string.IsNullOrWhiteSpace(authorization))
        {
            return Unauthorized(new { message = "Authorization header não fornecido" });
        }

        var token = authorization.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

        if (!_authService.ValidateToken(token))
        {
            return Unauthorized(new { message = "Token inválido ou expirado" });
        }

        var username = _authService.GetUsernameFromToken(token);
        return Ok(new { username, valid = true });
    }
}
