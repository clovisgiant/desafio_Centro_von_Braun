namespace CIoTDApi.Application.DTOs;

/// <summary>
/// Credenciais de autenticação
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// Nome de usuário
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Senha do usuário
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Resposta de autenticação com JWT
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// Token JWT para requisições subsequentes
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Tipo do token (Bearer)
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Tempo de expiração em segundos
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Nome do usuário autenticado
    /// </summary>
    public string UserName { get; set; } = string.Empty;
}
