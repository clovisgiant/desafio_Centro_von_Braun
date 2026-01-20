namespace CIoTDApi.Application.DTOs;

/// <summary>
/// Descreve um comando disponível em um dispositivo IoT
/// </summary>
public class CommandDescriptionDto
{
    /// <summary>
    /// Nome da operação executada pelo dispositivo
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Descrição e detalhes adicionais sobre a operação/comando
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Comando a ser executado
    /// </summary>
    public CommandDto Command { get; set; } = new();

    /// <summary>
    /// Descrição do resultado esperado da execução do comando
    /// </summary>
    public string Result { get; set; } = string.Empty;

    /// <summary>
    /// Definição (OpenAPI) do formato dos dados retornados pelo comando
    /// </summary>
    public object? Format { get; set; }
}
