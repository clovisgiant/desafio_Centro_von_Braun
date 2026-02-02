// ===========================================================================================
// SERVIÇO DE AUTENTICAÇÃO JWT - JwtAuthenticationService.cs
// ===========================================================================================
// Este serviço é responsável por autenticar usuários e gerar tokens JWT (JSON Web Tokens).
// O JWT é usado para manter a sessão do usuário sem necessidade de cookies ou estado no servidor.
//
// Fluxo:
// 1. Usuário envia username/password
// 2. Serviço valida as credenciais
// 3. Se válidas, gera um token JWT assinado
// 4. Frontend envia este token em cada requisição no header Authorization
// 5. Middleware valida o token e identifica o usuário
// ===========================================================================================

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

    // ===========================================================================================
    // USUÁRIOS MOCKADOS
    // ===========================================================================================
    // Em produção, isso seria substituído por consulta ao banco de dados com senhas hashadas
    // Atualmente usa usuários em memória apenas para demonstração
    
    private static readonly Dictionary<string, string> PredefinedUsers = new()
    {
        { "admin", "admin123" },          // Usuário administrador
        { "technician", "tech456" },      // Técnico
        { "researcher", "research789" }   // Pesquisador
    };

    // Construtor - recebe configurações e logger via Dependency Injection
    public JwtAuthenticationService(IConfiguration configuration, ILogger<JwtAuthenticationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    // ===========================================================================================
    // MÉTODO: AuthenticateAsync
    // ===========================================================================================
    // Autentica um usuário verificando username e password. Se válido, gera um token JWT.
    // ===========================================================================================
    public Task<LoginResponseDto?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        // PASSO 1: Verifica se o usuário existe no "banco de dados" mockado
        if (!PredefinedUsers.TryGetValue(username, out var storedPassword))
        {
            // Usuário não encontrado - registra warning e retorna null
            _logger.LogWarning("Tentativa de login com usuário inexistente: {Username}", username);
            return Task.FromResult<LoginResponseDto?>(null);
        }

        // PASSO 2: Verifica se a senha está correta
        // NOTA: Em produção, usar BCrypt ou similar para comparar hashes
        if (storedPassword != password)
        {
            // Senha incorreta - registra warning e retorna null
            _logger.LogWarning("Tentativa de login com senha incorreta para usuário: {Username}", username);
            return Task.FromResult<LoginResponseDto?>(null);
        }

        // PASSO 3: Credenciais válidas - gera o token JWT
        var token = GenerateJwtToken(username);
        
        // PASSO 4: Cria o objeto de resposta com o token e metadados
        var response = new LoginResponseDto
        {
            AccessToken = token,                                                             // Token JWT assinado
            TokenType = "Bearer",                                                           // Tipo do token (padrão HTTP)
            ExpiresIn = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60") * 60,   // Tempo de expiração em segundos
            UserName = username                                                             // Nome do usuário autenticado
        };

        // PASSO 5: Registra o sucesso e retorna a resposta
        _logger.LogInformation("Usuário autenticado com sucesso: {Username}", username);
        return Task.FromResult<LoginResponseDto?>(response);
    }

    // ===========================================================================================
    // MÉTODO: ValidateToken
    // ===========================================================================================
    // Valida se um token JWT é válido (assinatura correta, não expirado, etc)
    // ===========================================================================================
    public bool ValidateToken(string token)
    {
        try
        {
            // Cria um handler para processar tokens JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Obtém a chave secreta da configuração e converte para bytes
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? "");

            // Valida o token usando os mesmos parâmetros definidos no Program.cs
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,                        // Verifica se foi assinado com a chave correta
                IssuerSigningKey = new SymmetricSecurityKey(key),       // Chave de validação
                ValidateIssuer = true,                                  // Verifica o emissor
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,                                // Verifica a audiência
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,                                // Verifica se não expirou
                ClockSkew = TimeSpan.Zero                               // Sem tolerância de tempo
            }, out SecurityToken validatedToken);

            // Se chegou aqui, o token é válido
            return true;
        }
        catch (Exception ex)
        {
            // Token inválido, expirado ou com assinatura incorreta
            _logger.LogWarning("Falha ao validar token: {Message}", ex.Message);
            return false;
        }
    }

    // ===========================================================================================
    // MÉTODO: GetUsernameFromToken
    // ===========================================================================================
    // Extrai o nome de usuário de dentro de um token JWT (dos claims)
    // ===========================================================================================
    public string? GetUsernameFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Lê o token sem validar (apenas extrai os claims)
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            // Busca o claim NameIdentifier que contém o username
            // Claims são pares chave-valor armazenados dentro do token
            return jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        }
        catch (Exception ex)
        {
            // Token malformado ou inválido
            _logger.LogWarning("Falha ao extrair username do token: {Message}", ex.Message);
            return null;
        }
    }

    // ===========================================================================================
    // MÉTODO PRIVADO: GenerateJwtToken
    // ===========================================================================================
    // Gera um novo token JWT assinado com os dados do usuário
    // ===========================================================================================
    private string GenerateJwtToken(string username)
    {
        // Handler responsável por criar e escrever tokens JWT
        var tokenHandler = new JwtSecurityTokenHandler();
        
        // Obtém a chave secreta da configuração
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? "");
        
        // Tempo de expiração do token (padrão 60 minutos)
        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

        // Define o descritor do token - contém todas as informações que irão dentro do JWT
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            // Subject: identidade do usuário - conjunto de claims (dados)
            Subject = new ClaimsIdentity(new[]
            {
                // Claim NameIdentifier: identificador único do usuário
                new Claim(ClaimTypes.NameIdentifier, username),
                // Claim Name: nome amigável do usuário
                new Claim(ClaimTypes.Name, username)
            }),
            
            // Data de expiração do token
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            
            // Emissor: quem criou o token
            Issuer = _configuration["Jwt:Issuer"],
            
            // Audiência: para quem o token é destinado
            Audience = _configuration["Jwt:Audience"],
            
            // Credenciais de assinatura: chave secreta + algoritmo HMAC SHA256
            // A assinatura garante que o token não foi adulterado
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        // Cria o token baseado no descritor
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        // Converte o token para string (formato JWT: header.payload.signature)
        return tokenHandler.WriteToken(token);
    }
}
