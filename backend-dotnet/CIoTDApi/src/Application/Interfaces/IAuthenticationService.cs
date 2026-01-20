using CIoTDApi.Application.DTOs;

namespace CIoTDApi.Application.Interfaces;

/// <summary>
/// Interface para gerenciar autenticação
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Autentica um usuário e retorna um token JWT
    /// </summary>
    Task<LoginResponseDto?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida um token JWT
    /// </summary>
    bool ValidateToken(string token);

    /// <summary>
    /// Extrai o nome de usuário de um token JWT
    /// </summary>
    string? GetUsernameFromToken(string token);
}
