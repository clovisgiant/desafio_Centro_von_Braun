namespace CIoTDApi.Application.DTOs;

/// <summary>
/// Representa um parâmetro de um comando IoT
/// </summary>
public class ParameterDto
{
    /// <summary>
    /// Nome do parâmetro
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descrição do parâmetro, incluindo detalhes de sua utilização, valores possíveis
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
