using DevicesApi.Application.Commands;
using DevicesApi.Application.Results;

namespace DevicesApi.Application.Interfaces;

public interface IDeviceService
{
    Task<DeviceResult> CreateAsync(CreateDeviceCommand command, CancellationToken cancellationToken = default);
    Task<DeviceResult> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DeviceResult>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<DeviceResult>> GetByBrandAsync(string brand, CancellationToken cancellationToken = default);
    Task<IEnumerable<DeviceResult>> GetByStateAsync(string state, CancellationToken cancellationToken = default);
    Task<DeviceResult> UpdateAsync(Guid id, UpdateDeviceCommand command, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
