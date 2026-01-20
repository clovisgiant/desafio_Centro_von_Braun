using CIoTDApi.Application.DTOs;
using System.Text.Json;

namespace CIoTDApi.Application.Services;

/// <summary>
/// Interface para comunicação com o Device Agent Python
/// </summary>
public interface IDeviceAgentService
{
    Task<ExecuteCommandResponse> ExecuteCommandAsync(
        string deviceId, 
        string deviceHost, 
        int devicePort, 
        string command, 
        Dictionary<string, object> parameters, 
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Implementação do serviço que comunica com o Device Agent Python
/// </summary>
public class DeviceAgentService : IDeviceAgentService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DeviceAgentService> _logger;
    private readonly string _agentUrl;

    public DeviceAgentService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<DeviceAgentService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _agentUrl = configuration["DEVICE_AGENT_URL"] ?? "http://localhost:8001";
        
        _logger.LogInformation("Device Agent URL configurada: {Url}", _agentUrl);
    }

    public async Task<ExecuteCommandResponse> ExecuteCommandAsync(
        string deviceId,
        string deviceHost,
        int devicePort,
        string command,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken = default)
    {
        var request = new ExecuteCommandRequest
        {
            DeviceId = deviceId,
            DeviceHost = deviceHost,
            DevicePort = devicePort,
            Command = command,
            Parameters = parameters
        };

        _logger.LogInformation(
            "Enviando comando {Command} para dispositivo {DeviceId} via Agent",
            command, deviceId
        );

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_agentUrl}/api/execute",
                request,
                cancellationToken
            );

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ExecuteCommandResponse>(
                cancellationToken: cancellationToken
            );

            if (result == null)
            {
                _logger.LogError("Resposta nula do Agent para comando {Command}", command);
                return new ExecuteCommandResponse
                {
                    Success = false,
                    Error = "Resposta nula do Agent"
                };
            }

            _logger.LogInformation(
                "Comando {Command} executado com sucesso: {Response}",
                command, result.Response
            );

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro ao comunicar com Device Agent: {Message}", ex.Message);
            return new ExecuteCommandResponse
            {
                Success = false,
                Error = $"Erro ao comunicar com Agent: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao executar comando: {Message}", ex.Message);
            return new ExecuteCommandResponse
            {
                Success = false,
                Error = $"Erro inesperado: {ex.Message}"
            };
        }
    }
}