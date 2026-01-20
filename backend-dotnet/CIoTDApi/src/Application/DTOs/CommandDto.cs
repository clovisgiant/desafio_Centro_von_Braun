namespace CIoTDApi.Application.DTOs;

/// <summary>
/// Representa um comando executável em um dispositivo IoT
/// </summary>
public class CommandDto
{
    /// <summary>
    /// Sequência de bytes enviada para execução do comando
    /// </summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// Lista de parâmetros aceitos pelo comando
    /// </summary>
    public List<ParameterDto> Parameters { get; set; } = new();
}
