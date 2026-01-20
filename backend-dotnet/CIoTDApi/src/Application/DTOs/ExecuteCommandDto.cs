namespace CIoTDApi.Application.DTOs;

/// <summary>
/// Requisição para execução de um comando em um dispositivo
/// </summary>
public class ExecuteCommandDto
{
    /// <summary>
    /// Identificador do dispositivo
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Nome da operação a executar
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Dicionário de parâmetros (nome -> valor)
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; } = new();
}

/// <summary>
/// Resposta da execução de um comando
/// </summary>
public class CommandExecutionResultDto
{
    /// <summary>
    /// Indica se a execução foi bem-sucedida
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Dados retornados pelo dispositivo
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// Mensagem de erro (se houver)
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Tempo de execução em milissegundos
    /// </summary>
    public long ExecutionTimeMs { get; set; }
}
