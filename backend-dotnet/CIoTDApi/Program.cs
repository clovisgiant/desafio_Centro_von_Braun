using System.IdentityModel.Tokens.Jwt;
using System.Text;
using CIoTDApi.Application.Interfaces;
using CIoTDApi.Application.Services;
using CIoTDApi.Infrastructure.Authentication;
using CIoTDApi.Presentation.Middleware;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? "SuperSecureKeyWith256BitsForJwtTokenSigningPurposes";
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
})
.AddJwtBearer("JwtBearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "CIoTDApi",
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "CIoTDApiUsers",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Dependency Injection
builder.Services.AddScoped<IAuthenticationService, JwtAuthenticationService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddHttpClient<IDeviceAgentService, CIoTDApi.Infrastructure.Http.DeviceAgentService>();
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseJwtMiddleware();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("Health")
    .WithOpenApi();

app.Run();
