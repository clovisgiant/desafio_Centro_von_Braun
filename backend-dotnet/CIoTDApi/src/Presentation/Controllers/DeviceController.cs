// ===========================================================================================
// CONTROLLER DE DISPOSITIVOS - DeviceController.cs
// ===========================================================================================
// Este controller gerencia todas as operações CRUD de dispositivos IoT.
// Também é responsável por executar comandos nos dispositivos através do Device Agent Python.
// ===========================================================================================

using CIoTDApi.Application.DTOs;
using CIoTDApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CIoTDApi.Presentation.Controllers;

/// <summary>
/// Controller para gerenciar dispositivos IoT
/// </summary>
[ApiController]                      // Marca esta classe como um controller de API REST
[Route("api/[controller]")]         // Define a rota base: /api/Device
[Produces("application/json")]      // Indica que este controller retorna JSON
public class DeviceController : ControllerBase
{
    // Serviço que contém a lógica de negócio para gerenciar dispositivos
    private readonly IDeviceService _deviceService;
    
    // Serviço HTTP para se comunicar com o Device Agent Python
    private readonly IDeviceAgentService _deviceAgentService;
    
    // Logger para registrar eventos e erros
    private readonly ILogger<DeviceController> _logger;

    // Construtor - recebe as dependências via Dependency Injection
    public DeviceController(
        IDeviceService deviceService,
        IDeviceAgentService deviceAgentService,
        ILogger<DeviceController> logger)
    {
        _deviceService = deviceService;
        _deviceAgentService = deviceAgentService;
        _logger = logger;
    }

    // ===========================================================================================
    // ENDPOINT: GET /api/Device
    // ===========================================================================================
    // Retorna a lista de todos os IDs de dispositivos cadastrados no sistema
    // ===========================================================================================
    
    /// <summary>
    /// Retorna uma lista contendo os identificadores dos dispositivos cadastrados
    /// </summary>
    [HttpGet]  // Responde a GET em /api/Device
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]  // Retorna lista de strings (200)
    public async Task<IActionResult> GetAllDevices(CancellationToken cancellationToken)
    {
        // Busca todos os dispositivos através do serviço
        var devices = await _deviceService.GetAllDevicesAsync(cancellationToken);
        
        // Retorna HTTP 200 com a lista de IDs
        return Ok(devices);
    }

    // ===========================================================================================
    // ENDPOINT: GET /api/Device/{id}
    // ===========================================================================================
    // Retorna os detalhes completos de um dispositivo específico, incluindo seus comandos
    // ===========================================================================================
    
    /// <summary>
    /// Retorna os detalhes de um dispositivo
    /// </summary>
    [HttpGet("{id}")]  // Responde a GET em /api/Device/sensor-01 (por exemplo)
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status200OK)]     // Dispositivo encontrado (200)
    [ProducesResponseType(StatusCodes.Status404NotFound)]                  // Não encontrado (404)
    public async Task<IActionResult> GetDevice(string id, CancellationToken cancellationToken)
    {
        // Busca o dispositivo pelo ID
        var device = await _deviceService.GetDeviceAsync(id, cancellationToken);

        // Se não encontrou, retorna HTTP 404
        if (device == null)
        {
            return NotFound(new { message = $"Dispositivo {id} não encontrado" });
        }

        // Retorna HTTP 200 com os dados completos do dispositivo
        return Ok(device);
    }

    // ===========================================================================================
    // ENDPOINT: POST /api/Device
    // ===========================================================================================
    // Cadastra um novo dispositivo no sistema. Se o ID não for fornecido, gera um GUID.
    // ===========================================================================================
    
    /// <summary>
    /// Cadastra um novo dispositivo
    /// </summary>
    [HttpPost]  // Responde a POST em /api/Device
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status201Created)]  // Criado com sucesso (201)
    [ProducesResponseType(StatusCodes.Status400BadRequest)]                  // Dados inválidos (400)
    public async Task<IActionResult> CreateDevice([FromBody] DeviceDto device, CancellationToken cancellationToken)
    {
        // Se não foi fornecido um ID, gera um novo GUID automaticamente
        if (string.IsNullOrWhiteSpace(device.Identifier))
        {
            device.Identifier = Guid.NewGuid().ToString();
        }

        // Cria o dispositivo no sistema
        var created = await _deviceService.CreateDeviceAsync(device, cancellationToken);
        
        // Retorna HTTP 201 (Created) com o dispositivo criado
        // O header Location conterá a URL para acessar o novo dispositivo
        return CreatedAtAction(nameof(GetDevice), new { id = created.Identifier }, created);
    }

    // ===========================================================================================
    // ENDPOINT: PUT /api/Device/{id}
    // ===========================================================================================
    // Atualiza os dados de um dispositivo existente (nome, descrição, comandos, etc)
    // ===========================================================================================
    
    /// <summary>
    /// Atualiza os dados de um dispositivo
    /// </summary>
    [HttpPut("{id}")]  // Responde a PUT em /api/Device/sensor-01
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status200OK)]  // Atualizado com sucesso (200)
    [ProducesResponseType(StatusCodes.Status404NotFound)]               // Dispositivo não existe (404)
    public async Task<IActionResult> UpdateDevice(string id, [FromBody] DeviceDto device, CancellationToken cancellationToken)
    {
        // Tenta atualizar o dispositivo
        var updated = await _deviceService.UpdateDeviceAsync(id, device, cancellationToken);

        // Se retornou null, significa que o dispositivo não existe
        if (updated == null)
        {
            return NotFound(new { message = $"Dispositivo {id} não encontrado" });
        }

        // Retorna HTTP 200 com os dados atualizados
        return Ok(updated);
    }

    // ===========================================================================================
    // ENDPOINT: DELETE /api/Device/{id}
    // ===========================================================================================
    // Remove um dispositivo do sistema permanentemente
    // ===========================================================================================
    
    /// <summary>
    /// Remove um dispositivo
    /// </summary>
    [HttpDelete("{id}")]  // Responde a DELETE em /api/Device/sensor-01
    [ProducesResponseType(StatusCodes.Status200OK)]        // Removido com sucesso (200)
    [ProducesResponseType(StatusCodes.Status404NotFound)]  // Dispositivo não existe (404)
    public async Task<IActionResult> DeleteDevice(string id, CancellationToken cancellationToken)
    {
        // Tenta remover o dispositivo
        var deleted = await _deviceService.DeleteDeviceAsync(id, cancellationToken);

        // Se retornou false, significa que o dispositivo não existe
        if (!deleted)
        {
            return NotFound(new { message = $"Dispositivo {id} não encontrado" });
        }

        // Retorna HTTP 200 confirmando a remoção
        return Ok(new { message = $"Dispositivo {id} removido com sucesso" });
    }

    // ===========================================================================================
    // ENDPOINT: POST /api/Device/{id}/execute
    // ===========================================================================================
    // Executa um comando em um dispositivo IoT via Telnet através do Device Agent Python
    // Fluxo: Frontend → .NET API → Python Agent → Dispositivo (Telnet)
    // ===========================================================================================
    
    /// <summary>
    /// Executa um comando em um dispositivo
    /// </summary>
    [HttpPost("{id}/execute")]  // Responde a POST em /api/Device/sensor-01/execute
    [ProducesResponseType(typeof(CommandExecutionResultDto), StatusCodes.Status200OK)]  // Comando executado (200)
    [ProducesResponseType(StatusCodes.Status400BadRequest)]                             // Comando inválido (400)
    [ProducesResponseType(StatusCodes.Status404NotFound)]                               // Dispositivo não encontrado (404)
    public async Task<IActionResult> ExecuteCommand(
        string id,                                    // ID do dispositivo (da URL)
        [FromBody] ExecuteCommandDto request,         // Dados do comando (do corpo da requisição)
        CancellationToken cancellationToken)
    {
        // PASSO 1: Valida se o dispositivo existe no sistema
        var device = await _deviceService.GetDeviceAsync(id, cancellationToken);
        if (device == null)
        {
            // Retorna HTTP 404 se o dispositivo não foi encontrado
            return NotFound(new { message = $"Dispositivo {id} não encontrado" });
        }

        // PASSO 2: Valida se a operação solicitada existe para este dispositivo
        var command = device.Commands.FirstOrDefault(c => c.Operation == request.Operation);
        if (command == null)
        {
            // Retorna HTTP 400 se o comando não existe para este dispositivo
            return BadRequest(new { message = $"Operação {request.Operation} não existe para o dispositivo {id}" });
        }

        // Registra no log a execução do comando
        _logger.LogInformation(
            "Executando comando no dispositivo {DeviceId}: operação {Operation}",
            id, request.Operation);

        // PASSO 3: Extrai o host e porta do URL do dispositivo
        // URL exemplo: telnet://192.168.1.100:23
        var uri = new Uri(device.Url);
        var host = uri.Host;                              // Ex: 192.168.1.100
        var port = uri.Port > 0 ? uri.Port : 23;         // Porta Telnet padrão é 23

        // PASSO 4: Envia o comando para o Device Agent Python executar via Telnet
        var result = await _deviceAgentService.ExecuteCommandAsync(
            deviceId: id,                                 // ID do dispositivo
            deviceHost: host,                             // Endereço IP/host do dispositivo
            devicePort: port,                             // Porta Telnet
            command: command.Command.Command,             // Comando a ser executado (ex: READ_TEMP)
            parameters: request.Parameters,               // Parâmetros do comando
            cancellationToken: cancellationToken);

        // PASSO 5: Retorna o resultado da execução
        return Ok(new CommandExecutionResultDto
        {
            Success = result.Success,      // Indica se o comando foi executado com sucesso
            Response = result.Response,    // Resposta do dispositivo
            Error = result.Error          // Mensagem de erro (se houver)
        });
    }
}
