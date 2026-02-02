// ===========================================================================================
// SERVIÇO DE COMUNICAÇÃO COM DEVICE AGENT - DeviceAgentService.cs
// ===========================================================================================
// Este serviço é responsável por fazer requisições HTTP para o Device Agent Python.
// O Device Agent é quem realmente se comunica com os dispositivos IoT via Telnet.
//
// Arquitetura de comunicação:
// Frontend Angular → .NET API → Device Agent Python → Dispositivo IoT (Telnet)
//
// Este padrão de arquitetura em camadas permite:
// - Separação de responsabilidades
// - Escalabilidade independente
// - Protocolo HTTP mais fácil de debugar que Telnet direto
// ===========================================================================================

using System.Text.Json;
using CIoTDApi.Application.DTOs;
using CIoTDApi.Application.Interfaces;

namespace CIoTDApi.Infrastructure.Http;

/// <summary>
/// Implementação do serviço que comunica com o Device Agent Python
/// </summary>
public class DeviceAgentService : IDeviceAgentService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DeviceAgentService> _logger;
    private readonly string _agentUrl;

    // Construtor - recebe HttpClient configurado via Dependency Injection
    public DeviceAgentService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<DeviceAgentService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        // Lê a URL do Device Agent da configuração (ou usa localhost:8001 como padrão)
        // Em Docker Compose, isso será "http://device-agent:8001"
        _agentUrl = configuration["DEVICE_AGENT_URL"] ?? "http://localhost:8001";
        
        _logger.LogInformation("Device Agent URL configurada: {Url}", _agentUrl);
    }

    // ===========================================================================================
    // MÉTODO: ExecuteCommandAsync
    // ===========================================================================================
    // Envia um comando para o Device Agent Python executar em um dispositivo IoT via Telnet
    // ===========================================================================================
    public async Task<CommandExecutionResultDto> ExecuteCommandAsync(
        string deviceId,
        string deviceHost,
        int devicePort,
        string command,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken = default)
    {
        // PASSO 1: Monta o objeto de requisição para o Device Agent
        var request = new ExecuteCommandRequest
        {
            DeviceId = deviceId,        // ID do dispositivo (para log)
            DeviceHost = deviceHost,    // IP ou hostname do dispositivo IoT
            DevicePort = devicePort,    // Porta Telnet (geralmente 23)
            Command = command,          // Comando a executar (ex: "READ_TEMP")
            Parameters = parameters     // Parâmetros do comando
        };

        // Registra no log a tentativa de execução
        _logger.LogInformation(
            "Enviando comando {Command} para dispositivo {DeviceId} em {Host}:{Port} via Agent",
            command, deviceId, deviceHost, devicePort
        );

        try
        {
            // PASSO 2: Faz uma requisição HTTP POST para o Device Agent
            // Endpoint: POST http://device-agent:8001/api/execute
            var response = await _httpClient.PostAsJsonAsync(
                $"{_agentUrl}/api/execute",  // URL completa do endpoint
                request,                      // Corpo da requisição (será serializado para JSON)
                cancellationToken             // Token para cancelar a operação se necessário
            );

            // Verifica se a resposta HTTP foi bem-sucedida (200-299)
            // Lança exceção se for erro (400-599)
            response.EnsureSuccessStatusCode();

            // PASSO 3: Desserializa a resposta JSON do Agent
            var result = await response.Content.ReadFromJsonAsync<ExecuteCommandResponse>(
                cancellationToken: cancellationToken
            );

            // PASSO 4: Valida se a resposta não é nula
            if (result == null)
            {
                _logger.LogError("Resposta nula do Agent para comando {Command}", command);
                return new CommandExecutionResultDto
                {
                    Success = false,
                    Error = "Resposta nula do Agent"
                };
            }

            // PASSO 5: Registra o resultado e retorna
            _logger.LogInformation(
                "Comando {Command} executado - Success: {Success}",
                command, result.Success
            );

            return new CommandExecutionResultDto
            {
                Success = result.Success,  // true se o comando foi executado com sucesso
                Response = result.Response, // Resposta do dispositivo (ex: "OK TEMP=25.5C")
                Error = result.Error       // Mensagem de erro (se houver)
            };
        }
        catch (HttpRequestException ex)
        {
            // Erro ao comunicar com o Device Agent (Agent offline, timeout, etc)
            _logger.LogError(ex, "Erro ao comunicar com Device Agent: {Message}", ex.Message);
            return new CommandExecutionResultDto
            {
                Success = false,
                Error = $"Erro ao comunicar com Agent: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            // Qualquer outro erro inesperado
            _logger.LogError(ex, "Erro inesperado ao executar comando: {Message}", ex.Message);
            return new CommandExecutionResultDto
            {
                Success = false,
                Error = $"Erro inesperado: {ex.Message}"
            };
        }
    }
}