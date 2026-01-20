using CIoTDApi.Application.DTOs;
using CIoTDApi.Application.Interfaces;

namespace CIoTDApi.Infrastructure.Http;

/// <summary>
/// Implementação do serviço que se comunica com o Device Agent Python
/// </summary>
public class DeviceAgentService : IDeviceAgentService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DeviceAgentService> _logger;
    private readonly string _deviceAgentBaseUrl;

    public DeviceAgentService(HttpClient httpClient, IConfiguration configuration, ILogger<DeviceAgentService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _deviceAgentBaseUrl = configuration["DeviceAgent:BaseUrl"] ?? "http://localhost:8000";
    }

    public async Task<CommandExecutionResultDto> ExecuteCommandAsync(
        string deviceId,
        string operation,
        Dictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            // Constrói a URL do endpoint do Device Agent
            var url = $"{_deviceAgentBaseUrl}/api/execute";

            // Prepara o payload
            var payload = new
            {
                device_id = deviceId,
                operation = operation,
                parameters = parameters
            };

            // Serializa para JSON
            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json");

            _logger.LogInformation(
                "Enviando comando para Device Agent: device={DeviceId}, operation={Operation}",
                deviceId, operation);

            // Faz a requisição
            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            var executionTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Erro ao executar comando no Device Agent: {StatusCode} - {ErrorContent}",
                    response.StatusCode, errorContent);

                return new CommandExecutionResultDto
                {
                    Success = false,
                    Error = $"Device Agent retornou {response.StatusCode}: {errorContent}",
                    ExecutionTimeMs = executionTime
                };
            }

            // Processa a resposta
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = System.Text.Json.JsonSerializer.Deserialize<CommandExecutionResultDto>(responseContent);

            if (result != null)
            {
                result.ExecutionTimeMs = executionTime;
                _logger.LogInformation(
                    "Comando executado com sucesso em {ExecutionTime}ms",
                    executionTime);
                return result;
            }

            return new CommandExecutionResultDto
            {
                Success = false,
                Error = "Resposta inválida do Device Agent",
                ExecutionTimeMs = executionTime
            };
        }
        catch (HttpRequestException ex)
        {
            var executionTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Erro de comunicação com Device Agent");

            return new CommandExecutionResultDto
            {
                Success = false,
                Error = $"Falha na comunicação com Device Agent: {ex.Message}",
                ExecutionTimeMs = executionTime
            };
        }
        catch (Exception ex)
        {
            var executionTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Erro inesperado ao executar comando");

            return new CommandExecutionResultDto
            {
                Success = false,
                Error = $"Erro inesperado: {ex.Message}",
                ExecutionTimeMs = executionTime
            };
        }
    }
}
