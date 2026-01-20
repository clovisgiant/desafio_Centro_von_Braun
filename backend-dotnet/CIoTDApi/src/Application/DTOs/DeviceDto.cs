namespace CIoTDApi.Application.DTOs;

/// <summary>
/// Representa um dispositivo IoT cadastrado na plataforma
/// </summary>
public class DeviceDto
{
    /// <summary>
    /// Identificador único do dispositivo
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Descrição do dispositivo, incluindo detalhes de seu uso e informações geradas
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Nome do fabricante do dispositivo
    /// </summary>
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// URL de acesso ao dispositivo (ex: telnet://192.168.1.100:23)
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Lista de comandos disponíveis no dispositivo
    /// </summary>
    public List<CommandDescriptionDto> Commands { get; set; } = new();
}
