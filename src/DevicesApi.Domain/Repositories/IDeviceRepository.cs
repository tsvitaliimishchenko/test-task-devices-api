using DevicesApi.Domain.Entities;
using DevicesApi.Domain.Enums;

namespace DevicesApi.Domain.Repositories;

public interface IDeviceRepository
{
    Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Device>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Device>> GetByBrandAsync(string brand, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Device>> GetByStateAsync(DeviceState state, CancellationToken cancellationToken = default);
    void Add(Device device);
    void Remove(Device device);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
