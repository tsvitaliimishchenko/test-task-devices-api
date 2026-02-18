using DevicesApi.Domain.Entities;
using DevicesApi.Domain.Enums;
using DevicesApi.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DevicesApi.Infrastructure.Persistence.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly AppDbContext _context;

    public DeviceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Devices.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<Device>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .AsNoTracking()
            .OrderByDescending(d => d.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Device>> GetByBrandAsync(string brand, CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .AsNoTracking()
            .Where(d => d.Brand.ToLower() == brand.ToLower())
            .OrderByDescending(d => d.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Device>> GetByStateAsync(DeviceState state, CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .AsNoTracking()
            .Where(d => d.State == state)
            .OrderByDescending(d => d.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public void Add(Device device) => _context.Devices.Add(device);

    public void Remove(Device device) => _context.Devices.Remove(device);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
