using CIoTDApi.Application.DTOs;
using CIoTDApi.Application.Interfaces;

namespace CIoTDApi.Application.Services;

/// <summary>
/// Implementação do serviço de dispositivos com dados mockados
/// </summary>
public class DeviceService : IDeviceService
{
    private readonly ILogger<DeviceService> _logger;

    // Simulação de banco de dados em memória
    private static readonly Dictionary<string, DeviceDto> MockDevices = new();

    public DeviceService(ILogger<DeviceService> logger)
    {
        _logger = logger;
        InitializeMockData();
    }

    public Task<List<string>> GetAllDevicesAsync(CancellationToken cancellationToken = default)
    {
        var deviceIds = MockDevices.Keys.ToList();
        _logger.LogInformation("Retornando {Count} dispositivos", deviceIds.Count);
        return Task.FromResult(deviceIds);
    }

    public Task<DeviceDto?> GetDeviceAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        if (MockDevices.TryGetValue(deviceId, out var device))
        {
            _logger.LogInformation("Dispositivo encontrado: {DeviceId}", deviceId);
            return Task.FromResult<DeviceDto?>(device);
        }

        _logger.LogWarning("Dispositivo não encontrado: {DeviceId}", deviceId);
        return Task.FromResult<DeviceDto?>(null);
    }

    public Task<DeviceDto> CreateDeviceAsync(DeviceDto device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.Identifier))
        {
            device.Identifier = Guid.NewGuid().ToString();
        }

        MockDevices[device.Identifier] = device;
        _logger.LogInformation("Dispositivo criado: {DeviceId}", device.Identifier);
        return Task.FromResult(device);
    }

    public Task<DeviceDto?> UpdateDeviceAsync(string deviceId, DeviceDto device, CancellationToken cancellationToken = default)
    {
        if (!MockDevices.ContainsKey(deviceId))
        {
            _logger.LogWarning("Dispositivo não encontrado para atualização: {DeviceId}", deviceId);
            return Task.FromResult<DeviceDto?>(null);
        }

        device.Identifier = deviceId;
        MockDevices[deviceId] = device;
        _logger.LogInformation("Dispositivo atualizado: {DeviceId}", deviceId);
        return Task.FromResult<DeviceDto?>(device);
    }

    public Task<bool> DeleteDeviceAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        var removed = MockDevices.Remove(deviceId);
        if (removed)
        {
            _logger.LogInformation("Dispositivo removido: {DeviceId}", deviceId);
        }
        else
        {
            _logger.LogWarning("Tentativa de remover dispositivo inexistente: {DeviceId}", deviceId);
        }

        return Task.FromResult(removed);
    }

    private void InitializeMockData()
    {
        // Simulação de dispositivos de agricultura de precisão

        // Sensor de solo
        MockDevices["sensor-soil-001"] = new DeviceDto
        {
            Identifier = "sensor-soil-001",
            Description = "Sensor de umidade e temperatura do solo para monitoramento contínuo",
            Manufacturer = "SoilTech Industries",
            Url = "telnet://192.168.1.100:23",
            Commands = new List<CommandDescriptionDto>
            {
                new()
                {
                    Operation = "READ_HUMIDITY",
                    Description = "Lê o valor de umidade do solo em percentual",
                    Command = new CommandDto
                    {
                        Command = "READ",
                        Parameters = new List<ParameterDto>
                        {
                            new() { Name = "sensor_type", Description = "Tipo de sensor: humidity, temperature" }
                        }
                    },
                    Result = "Valor em percentual (0-100)",
                    Format = new { type = "number", minimum = 0, maximum = 100 }
                },
                new()
                {
                    Operation = "SET_THRESHOLD",
                    Description = "Define o limiar de alerta para umidade",
                    Command = new CommandDto
                    {
                        Command = "CONFIGURE",
                        Parameters = new List<ParameterDto>
                        {
                            new() { Name = "threshold", Description = "Valor do limiar (0-100)" },
                            new() { Name = "unit", Description = "Unidade: percent ou absolute" }
                        }
                    },
                    Result = "OK ou erro",
                    Format = new { type = "string", pattern = "(OK|ERROR)" }
                }
            }
        };

        // Sensor de clima
        MockDevices["sensor-weather-001"] = new DeviceDto
        {
            Identifier = "sensor-weather-001",
            Description = "Estação meteorológica completa com múltiplos sensores",
            Manufacturer = "WeatherPro Systems",
            Url = "telnet://192.168.1.101:23",
            Commands = new List<CommandDescriptionDto>
            {
                new()
                {
                    Operation = "READ_TEMPERATURE",
                    Description = "Lê a temperatura ambiente em graus Celsius",
                    Command = new CommandDto
                    {
                        Command = "READ_TEMP",
                        Parameters = new List<ParameterDto>()
                    },
                    Result = "Temperatura em °C",
                    Format = new { type = "number", minimum = -50, maximum = 50 }
                },
                new()
                {
                    Operation = "READ_HUMIDITY",
                    Description = "Lê a umidade relativa do ar",
                    Command = new CommandDto
                    {
                        Command = "READ_HUM",
                        Parameters = new List<ParameterDto>()
                    },
                    Result = "Umidade relativa (0-100%)",
                    Format = new { type = "number", minimum = 0, maximum = 100 }
                },
                new()
                {
                    Operation = "READ_RAINFALL",
                    Description = "Lê o acumulado de chuva em milímetros",
                    Command = new CommandDto
                    {
                        Command = "READ_RAIN",
                        Parameters = new List<ParameterDto>
                        {
                            new() { Name = "period", Description = "Período: hour, day, week" }
                        }
                    },
                    Result = "Acumulado em mm",
                    Format = new { type = "number", minimum = 0 }
                }
            }
        };

        // Sistema de irrigação
        MockDevices["irrigation-system-001"] = new DeviceDto
        {
            Identifier = "irrigation-system-001",
            Description = "Sistema de irrigação automatizado com múltiplas zonas",
            Manufacturer = "IrriControl Ltd",
            Url = "telnet://192.168.1.102:23",
            Commands = new List<CommandDescriptionDto>
            {
                new()
                {
                    Operation = "START_IRRIGATION",
                    Description = "Inicia a irrigação em uma zona específica",
                    Command = new CommandDto
                    {
                        Command = "START",
                        Parameters = new List<ParameterDto>
                        {
                            new() { Name = "zone", Description = "Número da zona (1-8)" },
                            new() { Name = "duration", Description = "Duração em minutos (1-120)" }
                        }
                    },
                    Result = "Status da operação",
                    Format = new { type = "string", pattern = "(STARTED|ERROR)" }
                },
                new()
                {
                    Operation = "STOP_IRRIGATION",
                    Description = "Para a irrigação em uma zona",
                    Command = new CommandDto
                    {
                        Command = "STOP",
                        Parameters = new List<ParameterDto>
                        {
                            new() { Name = "zone", Description = "Número da zona (1-8)" }
                        }
                    },
                    Result = "Status da operação",
                    Format = new { type = "string", pattern = "(STOPPED|ERROR)" }
                },
                new()
                {
                    Operation = "GET_ZONE_STATUS",
                    Description = "Obtém o status atual de uma zona",
                    Command = new CommandDto
                    {
                        Command = "STATUS",
                        Parameters = new List<ParameterDto>
                        {
                            new() { Name = "zone", Description = "Número da zona (1-8)" }
                        }
                    },
                    Result = "Status: ativo, inativo, erro",
                    Format = new { type = "object", properties = new { status = new { type = "string" }, flow_rate = new { type = "number" } } }
                }
            }
        };

        _logger.LogInformation("Dados mock inicializados com {Count} dispositivos", MockDevices.Count);
    }
}
