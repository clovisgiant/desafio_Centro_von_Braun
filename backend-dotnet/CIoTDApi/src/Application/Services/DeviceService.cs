// ===========================================================================================
// SERVIÇO DE DISPOSITIVOS - DeviceService.cs
// ===========================================================================================
// Este serviço contém toda a lógica de negócio para gerenciar dispositivos IoT.
// Atualmente usa dados mockados em memória (Dictionary), mas pode ser facilmente
// adaptado para usar um banco de dados real.
// ===========================================================================================

using CIoTDApi.Application.DTOs;
using CIoTDApi.Application.Interfaces;

namespace CIoTDApi.Application.Services;

/// <summary>
/// Implementação do serviço de dispositivos com dados mockados
/// </summary>
public class DeviceService : IDeviceService
{
    private readonly ILogger<DeviceService> _logger;

    // Simulação de banco de dados em memória usando um Dictionary
    // Em produção, isso seria substituído por um contexto de banco de dados real (Entity Framework, Dapper, etc)
    private static readonly Dictionary<string, DeviceDto> MockDevices = new();

    // Construtor - recebe o logger via Dependency Injection
    public DeviceService(ILogger<DeviceService> logger)
    {
        _logger = logger;
        
        // Inicializa os dados mockados quando o serviço é criado pela primeira vez
        InitializeMockData();
    }

    // ===========================================================================================
    // MÉTODO: GetAllDevicesAsync
    // ===========================================================================================
    // Retorna uma lista com todos os IDs de dispositivos cadastrados
    // ===========================================================================================
    public Task<List<string>> GetAllDevicesAsync(CancellationToken cancellationToken = default)
    {
        // Obtém todas as chaves (IDs) do Dictionary
        var deviceIds = MockDevices.Keys.ToList();
        
        // Registra no log quantos dispositivos foram encontrados
        _logger.LogInformation("Retornando {Count} dispositivos", deviceIds.Count);
        
        // Retorna como Task (async) mesmo sendo operação em memória
        // Em um cenário real com banco de dados, seria await context.Devices.ToListAsync()
        return Task.FromResult(deviceIds);
    }

    // ===========================================================================================
    // MÉTODO: GetDeviceAsync
    // ===========================================================================================
    // Busca um dispositivo específico pelo ID e retorna seus dados completos
    // ===========================================================================================
    public Task<DeviceDto?> GetDeviceAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        // Tenta buscar o dispositivo no Dictionary
        if (MockDevices.TryGetValue(deviceId, out var device))
        {
            // Se encontrou, registra no log e retorna o dispositivo
            _logger.LogInformation("Dispositivo encontrado: {DeviceId}", deviceId);
            return Task.FromResult<DeviceDto?>(device);
        }

        // Se não encontrou, registra warning e retorna null
        _logger.LogWarning("Dispositivo não encontrado: {DeviceId}", deviceId);
        return Task.FromResult<DeviceDto?>(null);
    }

    // ===========================================================================================
    // MÉTODO: CreateDeviceAsync
    // ===========================================================================================
    // Cria um novo dispositivo no sistema. Se não houver ID, gera um novo GUID.
    // ===========================================================================================
    public Task<DeviceDto> CreateDeviceAsync(DeviceDto device, CancellationToken cancellationToken = default)
    {
        // Se o ID não foi fornecido, gera um novo GUID automaticamente
        if (string.IsNullOrEmpty(device.Identifier))
        {
            device.Identifier = Guid.NewGuid().ToString();
        }

        // Adiciona (ou substitui) o dispositivo no Dictionary
        MockDevices[device.Identifier] = device;
        
        // Registra a criação no log
        _logger.LogInformation("Dispositivo criado: {DeviceId}", device.Identifier);
        
        // Retorna o dispositivo criado
        return Task.FromResult(device);
    }

    // ===========================================================================================
    // MÉTODO: UpdateDeviceAsync
    // ===========================================================================================
    // Atualiza os dados de um dispositivo existente
    // ===========================================================================================
    public Task<DeviceDto?> UpdateDeviceAsync(string deviceId, DeviceDto device, CancellationToken cancellationToken = default)
    {
        // Verifica se o dispositivo existe no Dictionary
        if (!MockDevices.ContainsKey(deviceId))
        {
            // Se não existe, registra warning e retorna null
            _logger.LogWarning("Dispositivo não encontrado para atualização: {DeviceId}", deviceId);
            return Task.FromResult<DeviceDto?>(null);
        }

        // Garante que o ID do dispositivo permanece o mesmo
        device.Identifier = deviceId;
        
        // Atualiza o dispositivo no Dictionary
        MockDevices[deviceId] = device;
        
        // Registra a atualização no log
        _logger.LogInformation("Dispositivo atualizado: {DeviceId}", deviceId);
        
        // Retorna o dispositivo atualizado
        return Task.FromResult<DeviceDto?>(device);
    }

    // ===========================================================================================
    // MÉTODO: DeleteDeviceAsync
    // ===========================================================================================
    // Remove um dispositivo do sistema
    // ===========================================================================================
    public Task<bool> DeleteDeviceAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        // Tenta remover o dispositivo do Dictionary
        // Remove() retorna true se removeu com sucesso, false se o ID não existia
        var removed = MockDevices.Remove(deviceId);
        
        if (removed)
        {
            // Se removeu, registra sucesso no log
            _logger.LogInformation("Dispositivo removido: {DeviceId}", deviceId);
        }
        else
        {
            // Se não encontrou, registra warning
            _logger.LogWarning("Tentativa de remover dispositivo inexistente: {DeviceId}", deviceId);
        }

        // Retorna true/false indicando se a remoção foi bem-sucedida
        return Task.FromResult(removed);
    }

    // ===========================================================================================
    // MÉTODO: InitializeMockData
    // ===========================================================================================
    // Popula o sistema com dispositivos de exemplo para demonstração
    // Em produção, esses dados viriam de um banco de dados
    // ===========================================================================================
    private void InitializeMockData()
    {
        // ===========================================================================================
        // DISPOSITIVO 1: Sensor de Solo
        // ===========================================================================================
        // Sensor para monitorar umidade e temperatura do solo em lavouras
        
        MockDevices["sensor-soil-001"] = new DeviceDto
        {
            Identifier = "sensor-soil-001",
            Description = "Sensor de umidade e temperatura do solo para monitoramento contínuo",
            Manufacturer = "SoilTech Industries",
            Url = "telnet://192.168.1.100:23",  // Endereço Telnet do dispositivo
            Commands = new List<CommandDescriptionDto>
            {
                // Comando 1: Ler umidade do solo
                new()
                {
                    Operation = "READ_HUMIDITY",
                    Description = "Lê o valor de umidade do solo em percentual",
                    Command = new CommandDto
                    {
                        Command = "READ",  // Comando Telnet real a ser enviado
                        Parameters = new List<ParameterDto>
                        {
                            new() { Name = "sensor_type", Description = "Tipo de sensor: humidity, temperature" }
                        }
                    },
                    Result = "Valor em percentual (0-100)",
                    Format = new { type = "number", minimum = 0, maximum = 100 }
                },
                // Comando 2: Configurar limiar de alerta
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

        // ===========================================================================================
        // DISPOSITIVO 2: Estação Meteorológica
        // ===========================================================================================
        // Sensor de clima que monitora temperatura, umidade e chuva
        
        MockDevices["sensor-weather-001"] = new DeviceDto
        {
            Identifier = "sensor-weather-001",
            Description = "Estação meteorológica completa com múltiplos sensores",
            Manufacturer = "WeatherPro Systems",
            Url = "telnet://192.168.1.101:23",
            Commands = new List<CommandDescriptionDto>
            {
                // Comando 1: Ler temperatura
                new()
                {
                    Operation = "READ_TEMPERATURE",
                    Description = "Lê a temperatura ambiente em graus Celsius",
                    Command = new CommandDto
                    {
                        Command = "READ_TEMP",  // Comando Telnet: "READ_TEMP\r"
                        Parameters = new List<ParameterDto>()  // Sem parâmetros
                    },
                    Result = "Temperatura em °C",
                    Format = new { type = "number", minimum = -50, maximum = 50 }
                },
                // Comando 2: Ler umidade do ar
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
                // Comando 3: Ler índice pluviométrico (chuva)
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

        // ===========================================================================================
        // DISPOSITIVO 3: Sistema de Irrigação
        // ===========================================================================================
        // Controlador de irrigação com 8 zonas independentes
        
        MockDevices["irrigation-system-001"] = new DeviceDto
        {
            Identifier = "irrigation-system-001",
            Description = "Sistema de irrigação automatizado com múltiplas zonas",
            Manufacturer = "IrriControl Ltd",
            Url = "telnet://192.168.1.102:23",
            Commands = new List<CommandDescriptionDto>
            {
                // Comando 1: Iniciar irrigação
                new()
                {
                    Operation = "START_IRRIGATION",
                    Description = "Inicia a irrigação em uma zona específica",
                    Command = new CommandDto
                    {
                        Command = "START",  // Comando Telnet: "START 1 30\r" (zona 1, 30 minutos)
                        Parameters = new List<ParameterDto>
                        {
                            new() { Name = "zone", Description = "Número da zona (1-8)" },
                            new() { Name = "duration", Description = "Duração em minutos (1-120)" }
                        }
                    },
                    Result = "Status da operação",
                    Format = new { type = "string", pattern = "(STARTED|ERROR)" }
                },
                // Comando 2: Parar irrigação
                new()
                {
                    Operation = "STOP_IRRIGATION",
                    Description = "Para a irrigação em uma zona",
                    Command = new CommandDto
                    {
                        Command = "STOP",  // Comando Telnet: "STOP 1\r" (para a zona 1)
                        Parameters = new List<ParameterDto>
                        {
                            new() { Name = "zone", Description = "Número da zona (1-8)" }
                        }
                    },
                    Result = "Status da operação",
                    Format = new { type = "string", pattern = "(STOPPED|ERROR)" }
                },
                // Comando 3: Verificar status da zona
                new()
                {
                    Operation = "GET_ZONE_STATUS",
                    Description = "Obtém o status atual de uma zona",
                    Command = new CommandDto
                    {
                        Command = "STATUS",  // Comando Telnet: "STATUS 1\r"
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

        // Registra no log quantos dispositivos foram inicializados
        _logger.LogInformation("Dados mock inicializados com {Count} dispositivos", MockDevices.Count);
    }
}
