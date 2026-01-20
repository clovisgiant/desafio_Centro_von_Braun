using System.Text.Json.Serialization;

namespace CIoTDApi.Application.DTOs;

/// <summary>
/// Request para executar comando em dispositivo via Agent
/// </summary>
public class ExecuteCommandRequest
{
    [JsonPropertyName("device_id")]
    public string DeviceId { get; set; } = string.Empty;
    
    [JsonPropertyName("device_host")]
    public string DeviceHost { get; set; } = string.Empty;
    
    [JsonPropertyName("device_port")]
    public int DevicePort { get; set; }
    
    [JsonPropertyName("command")]
    public string Command { get; set; } = string.Empty;
    
    [JsonPropertyName("parameters")]
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Response da execução de comando via Agent
/// </summary>
public class ExecuteCommandResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("response")]
    public string? Response { get; set; }
    
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}