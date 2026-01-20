using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CIoTDApi.Application.DTOs;
using CIoTDApi.Application.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace CIoTDApi.Infrastructure.Authentication;

/// <summary>
/// Implementação do serviço de autenticação JWT
/// </summary>
public class JwtAuthenticationService : IAuthenticationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtAuthenticationService> _logger;

    // Usuários pré-cadastrados (mock)
    private static readonly Dictionary<string, string> PredefinedUsers = new()
    {
        { "admin", "admin123" },
        { "technician", "tech456" },
        { "researcher", "research789" }
    };

    public JwtAuthenticationService(IConfiguration configuration, ILogger<JwtAuthenticationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<LoginResponseDto?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (!PredefinedUsers.TryGetValue(username, out var storedPassword))
        {
            _logger.LogWarning("Tentativa de login com usuário inexistente: {Username}", username);
            return Task.FromResult<LoginResponseDto?>(null);
        }

        if (storedPassword != password)
        {
            _logger.LogWarning("Tentativa de login com senha incorreta para usuário: {Username}", username);
            return Task.FromResult<LoginResponseDto?>(null);
        }

        var token = GenerateJwtToken(username);
        var response = new LoginResponseDto
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60") * 60,
            UserName = username
        };

        _logger.LogInformation("Usuário autenticado com sucesso: {Username}", username);
        return Task.FromResult<LoginResponseDto?>(response);
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? "");

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Falha ao validar token: {Message}", ex.Message);
            return false;
        }
    }

    public string? GetUsernameFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Falha ao extrair username do token: {Message}", ex.Message);
            return null;
        }
    }

    private string GenerateJwtToken(string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? "");
        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Name, username)
            }),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
