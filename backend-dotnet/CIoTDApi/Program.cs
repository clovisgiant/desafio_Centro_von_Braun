// ===========================================================================================
// ARQUIVO PRINCIPAL DA API - Program.cs
// ===========================================================================================
// Este é o ponto de entrada da aplicação .NET que configura todos os serviços e middleware
// necessários para a API funcionar. Aqui são definidas as configurações de autenticação JWT,
// injeção de dependências, CORS e o pipeline de requisições HTTP.
// ===========================================================================================

using System.IdentityModel.Tokens.Jwt;
using System.Text;
using CIoTDApi.Application.Interfaces;
using CIoTDApi.Application.Services;
using CIoTDApi.Infrastructure.Authentication;
using CIoTDApi.Presentation.Middleware;
using Microsoft.IdentityModel.Tokens;

// Cria o builder da aplicação web - responsável por configurar a aplicação antes de executá-la
var builder = WebApplication.CreateBuilder(args);

// ===========================================================================================
// SEÇÃO 1: CONFIGURAÇÃO DE SERVIÇOS (Dependency Injection Container)
// ===========================================================================================

// Adiciona suporte a Controllers MVC - permite criar endpoints REST usando atributos [Route]
builder.Services.AddControllers();

// Adiciona o explorador de endpoints da API para gerar documentação automática
builder.Services.AddEndpointsApiExplorer();

// Configura o Swagger - ferramenta que gera documentação interativa da API
builder.Services.AddSwaggerGen();

// Configura CORS (Cross-Origin Resource Sharing) - permite que o frontend Angular
// (rodando em outra porta) possa fazer requisições para esta API
builder.Services.AddCors(options =>
{
    // Define a política "AllowAll" que permite qualquer origem, método e cabeçalho
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()    // Permite requisições de qualquer domínio
            .AllowAnyMethod()        // Permite todos os verbos HTTP (GET, POST, PUT, DELETE, etc)
            .AllowAnyHeader();       // Permite todos os cabeçalhos HTTP
    });
});

// ===========================================================================================
// SEÇÃO 2: CONFIGURAÇÃO DE AUTENTICAÇÃO JWT
// ===========================================================================================

// Lê as configurações de JWT do arquivo appsettings.json na seção "Jwt"
var jwtSettings = builder.Configuration.GetSection("Jwt");

// Obtém a chave secreta usada para assinar/validar tokens. Se não houver no config, usa valor padrão
var secretKey = jwtSettings["SecretKey"] ?? "SuperSecureKeyWith256BitsForJwtTokenSigningPurposes";

// Converte a chave de string para bytes - necessário para o algoritmo de criptografia
var key = Encoding.ASCII.GetBytes(secretKey);

// Configura o serviço de autenticação da aplicação
builder.Services.AddAuthentication(options =>
{
    // Define "JwtBearer" como esquema padrão tanto para autenticação quanto para desafio
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
})
// Configura o esquema JWT Bearer que valida tokens em cada requisição
.AddJwtBearer("JwtBearer", options =>
{
    // Define os parâmetros de validação do token JWT
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Valida se o token foi assinado com a chave correta
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        
        // Valida o emissor (issuer) do token
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "CIoTDApi",
        
        // Valida a audiência (audience) - quem pode usar o token
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "CIoTDApiUsers",
        
        // Valida se o token ainda não expirou
        ValidateLifetime = true,
        
        // Remove a tolerância de tempo (clock skew) - token expira exatamente no tempo definido
        ClockSkew = TimeSpan.Zero
    };
});

// ===========================================================================================
// SEÇÃO 3: INJEÇÃO DE DEPENDÊNCIAS (Dependency Injection)
// ===========================================================================================
// Registra os serviços da aplicação no container de DI. O padrão Scoped cria uma instância
// por requisição HTTP, garantindo isolamento entre requisições.

// Serviço de autenticação - responsável por gerar e validar tokens JWT
builder.Services.AddScoped<IAuthenticationService, JwtAuthenticationService>();

// Serviço de dispositivos - contém a lógica de negócio para gerenciar dispositivos IoT
builder.Services.AddScoped<IDeviceService, DeviceService>();

// Serviço HTTP para comunicação com o Device Agent Python
// AddHttpClient cria um cliente HTTP configurado e gerenciado pelo framework
builder.Services.AddHttpClient<IDeviceAgentService, CIoTDApi.Infrastructure.Http.DeviceAgentService>();

// Adiciona sistema de logging da aplicação
builder.Services.AddLogging();

// ===========================================================================================
// SEÇÃO 4: BUILD DA APLICAÇÃO
// ===========================================================================================

// Constrói a aplicação com todas as configurações definidas acima
var app = builder.Build();

// ===========================================================================================
// SEÇÃO 5: CONFIGURAÇÃO DO PIPELINE HTTP (Middleware)
// ===========================================================================================
// Define a ordem de processamento das requisições HTTP. A ordem é importante!

// Se estiver em ambiente de desenvolvimento, habilita o Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();      // Gera o JSON do Swagger
    app.UseSwaggerUI();    // Cria a interface gráfica para testar a API
}

// Redireciona requisições HTTP para HTTPS (segurança)
app.UseHttpsRedirection();

// Aplica a política CORS definida anteriormente
app.UseCors("AllowAll");

// Middleware customizado que extrai e valida o token JWT das requisições
app.UseJwtMiddleware();

// Middleware de autenticação - identifica o usuário baseado no token
app.UseAuthentication();

// Middleware de autorização - verifica se o usuário tem permissão para acessar o endpoint
app.UseAuthorization();

// Mapeia os controllers para seus respectivos endpoints
app.MapControllers();

// ===========================================================================================
// SEÇÃO 6: ENDPOINT DE HEALTH CHECK
// ===========================================================================================

// Cria um endpoint simples para verificar se a API está funcionando
// GET /health retorna { "status": "healthy", "timestamp": "2024-01-30T10:00:00Z" }
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("Health")      // Nome do endpoint para referência
    .WithOpenApi();          // Inclui no Swagger

// ===========================================================================================
// SEÇÃO 7: INICIALIZAÇÃO DA APLICAÇÃO
// ===========================================================================================

// Inicia a aplicação e começa a escutar requisições HTTP na porta configurada (5001)
app.Run();
