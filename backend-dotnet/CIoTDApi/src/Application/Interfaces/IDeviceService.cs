using CIoTDApi.Application.DTOs;

namespace CIoTDApi.Application.Interfaces;

/// <summary>
/// Interface para gerenciar dispositivos IoT
/// </summary>
public interface IDeviceService
{
    /// <summary>
    /// Obtém a lista de todos os dispositivos cadastrados
    /// </summary>
    Task<List<string>> GetAllDevicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém os detalhes de um dispositivo específico
    /// </summary>
    Task<DeviceDto?> GetDeviceAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra um novo dispositivo
    /// </summary>
    Task<DeviceDto> CreateDeviceAsync(DeviceDto device, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza os dados de um dispositivo
    /// </summary>
    Task<DeviceDto?> UpdateDeviceAsync(string deviceId, DeviceDto device, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um dispositivo
    /// </summary>
    Task<bool> DeleteDeviceAsync(string deviceId, CancellationToken cancellationToken = default);
}
