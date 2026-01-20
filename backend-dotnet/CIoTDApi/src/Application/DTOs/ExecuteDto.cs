namespace CIoTDApi.Application.DTOs;

/// <summary>
/// Request para executar um comando em um dispositivo
/// </summary>
public class ExecuteCommandDto
{
    /// <summary>
    /// Nome da operação a ser executada
    /// </summary>
    public string Operation { get; set; } = string.Empty;
    
    /// <summary>
    /// Parâmetros do comando (chave: nome do param, valor: valor)
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Resultado da execução de um comando
/// </summary>
public class CommandExecutionResultDto
{
    /// <summary>
    /// Indica se o comando foi executado com sucesso
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Resposta do dispositivo
    /// </summary>
    public string? Response { get; set; }
    
    /// <summary>
    /// Mensagem de erro (se houver)
    /// </summary>
    public string? Error { get; set; }
}