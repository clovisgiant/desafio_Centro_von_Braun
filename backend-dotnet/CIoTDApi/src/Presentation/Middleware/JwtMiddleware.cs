using CIoTDApi.Application.Interfaces;

namespace CIoTDApi.Presentation.Middleware;

/// <summary>
/// Middleware para validar o token JWT em requisições
/// </summary>
public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtMiddleware> _logger;

    public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuthenticationService authService)
    {
        var token = ExtractTokenFromHeader(context);

        if (!string.IsNullOrEmpty(token))
        {
            if (authService.ValidateToken(token))
            {
                var username = authService.GetUsernameFromToken(token);
                if (!string.IsNullOrEmpty(username))
                {
                    context.Items["User"] = username;
                    _logger.LogDebug("Token validado para usuário: {Username}", username);
                }
            }
            else
            {
                _logger.LogWarning("Token inválido recebido");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Token inválido ou expirado" });
                return;
            }
        }

        await _next(context);
    }

    private string? ExtractTokenFromHeader(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var auth = authHeader.ToString();
            if (auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return auth.Substring("Bearer ".Length).Trim();
            }
        }

        return null;
    }
}

/// <summary>
/// Extensão para adicionar o middleware JWT ao pipeline
/// </summary>
public static class JwtMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtMiddleware>();
    }
}
