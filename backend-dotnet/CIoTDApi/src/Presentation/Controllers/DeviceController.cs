using CIoTDApi.Application.DTOs;
using CIoTDApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CIoTDApi.Presentation.Controllers;

/// <summary>
/// Controller para gerenciar dispositivos IoT
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DeviceController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly IDeviceAgentService _deviceAgentService;
    private readonly ILogger<DeviceController> _logger;

    public DeviceController(
        IDeviceService deviceService,
        IDeviceAgentService deviceAgentService,
        ILogger<DeviceController> logger)
    {
        _deviceService = deviceService;
        _deviceAgentService = deviceAgentService;
        _logger = logger;
    }

    /// <summary>
    /// Retorna uma lista contendo os identificadores dos dispositivos cadastrados
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllDevices(CancellationToken cancellationToken)
    {
        var devices = await _deviceService.GetAllDevicesAsync(cancellationToken);
        return Ok(devices);
    }

    /// <summary>
    /// Retorna os detalhes de um dispositivo
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDevice(string id, CancellationToken cancellationToken)
    {
        var device = await _deviceService.GetDeviceAsync(id, cancellationToken);

        if (device == null)
        {
            return NotFound(new { message = $"Dispositivo {id} não encontrado" });
        }

        return Ok(device);
    }

    /// <summary>
    /// Cadastra um novo dispositivo
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDevice([FromBody] DeviceDto device, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(device.Identifier))
        {
            device.Identifier = Guid.NewGuid().ToString();
        }

        var created = await _deviceService.CreateDeviceAsync(device, cancellationToken);
        return CreatedAtAction(nameof(GetDevice), new { id = created.Identifier }, created);
    }

    /// <summary>
    /// Atualiza os dados de um dispositivo
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDevice(string id, [FromBody] DeviceDto device, CancellationToken cancellationToken)
    {
        var updated = await _deviceService.UpdateDeviceAsync(id, device, cancellationToken);

        if (updated == null)
        {
            return NotFound(new { message = $"Dispositivo {id} não encontrado" });
        }

        return Ok(updated);
    }

    /// <summary>
    /// Remove um dispositivo
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDevice(string id, CancellationToken cancellationToken)
    {
        var deleted = await _deviceService.DeleteDeviceAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound(new { message = $"Dispositivo {id} não encontrado" });
        }

        return Ok(new { message = $"Dispositivo {id} removido com sucesso" });
    }

    /// <summary>
    /// Executa um comando em um dispositivo
    /// </summary>
    [HttpPost("{id}/execute")]
    [ProducesResponseType(typeof(CommandExecutionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExecuteCommand(
        string id,
        [FromBody] ExecuteCommandDto request,
        CancellationToken cancellationToken)
    {
        // Valida se o dispositivo existe
        var device = await _deviceService.GetDeviceAsync(id, cancellationToken);
        if (device == null)
        {
            return NotFound(new { message = $"Dispositivo {id} não encontrado" });
        }

        // Valida se a operação existe
        var command = device.Commands.FirstOrDefault(c => c.Operation == request.Operation);
        if (command == null)
        {
            return BadRequest(new { message = $"Operação {request.Operation} não existe para o dispositivo {id}" });
        }

        _logger.LogInformation(
            "Executando comando no dispositivo {DeviceId}: operação {Operation}",
            id, request.Operation);

        // Parse do URL telnet (ex: telnet://192.168.1.100:23)
        var uri = new Uri(device.Url);
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 23; // Default Telnet port

        // Executa o comando via Device Agent
        var result = await _deviceAgentService.ExecuteCommandAsync(
            deviceId: id,
            deviceHost: host,
            devicePort: port,
            command: command.Command.Command,
            parameters: request.Parameters,
            cancellationToken: cancellationToken);

        return Ok(new CommandExecutionResultDto
        {
            Success = result.Success,
            Response = result.Response,
            Error = result.Error
        });
    }
}
