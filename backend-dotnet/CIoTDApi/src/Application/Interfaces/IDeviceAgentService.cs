using CIoTDApi.Application.DTOs;

namespace CIoTDApi.Application.Interfaces;

/// <summary>
/// Interface para executar comandos em dispositivos via Device Agent
/// </summary>
public interface IDeviceAgentService
{
    /// <summary>
    /// Executa um comando em um dispositivo via Device Agent Python
    /// </summary>
    Task<CommandExecutionResultDto> ExecuteCommandAsync(
        string deviceId,
        string deviceHost,
        int devicePort,
        string command,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken = default);
}