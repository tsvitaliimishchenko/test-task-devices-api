using DevicesApi.Application.Commands;
using DevicesApi.Application.Interfaces;
using DevicesApi.Application.Mappings;
using DevicesApi.Application.Results;
using DevicesApi.Domain.Entities;
using DevicesApi.Domain.Exceptions;
using DevicesApi.Domain.Repositories;

namespace DevicesApi.Application.Services;

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _repository;

    public DeviceService(IDeviceRepository repository)
    {
        _repository = repository;
    }

    public async Task<DeviceResult> CreateAsync(CreateDeviceCommand command, CancellationToken cancellationToken = default)
    {
        var state = Device.ParseState(command.State);
        var device = Device.Create(command.Name, command.Brand, state);

        _repository.Add(device);
        await _repository.SaveChangesAsync(cancellationToken);

        return DeviceMapper.ToResult(device);
    }

    public async Task<DeviceResult> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var device = await GetDeviceOrThrowAsync(id, cancellationToken);
        return DeviceMapper.ToResult(device);
    }

    public async Task<IEnumerable<DeviceResult>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var devices = await _repository.GetAllAsync(cancellationToken);
        return DeviceMapper.ToResultList(devices);
    }

    public async Task<IEnumerable<DeviceResult>> GetByBrandAsync(string brand, CancellationToken cancellationToken = default)
    {
        var devices = await _repository.GetByBrandAsync(brand.Trim(), cancellationToken);
        return DeviceMapper.ToResultList(devices);
    }

    public async Task<IEnumerable<DeviceResult>> GetByStateAsync(string state, CancellationToken cancellationToken = default)
    {
        var parsedState = Device.ParseState(state);
        var devices = await _repository.GetByStateAsync(parsedState, cancellationToken);
        return DeviceMapper.ToResultList(devices);
    }

    public async Task<DeviceResult> UpdateAsync(Guid id, UpdateDeviceCommand command, CancellationToken cancellationToken = default)
    {
        var device = await GetDeviceOrThrowAsync(id, cancellationToken);

        if (command.Name is not null)
            device.UpdateName(command.Name);

        if (command.Brand is not null)
            device.UpdateBrand(command.Brand);

        if (command.State is not null)
            device.ChangeState(Device.ParseState(command.State));

        await _repository.SaveChangesAsync(cancellationToken);

        return DeviceMapper.ToResult(device);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var device = await GetDeviceOrThrowAsync(id, cancellationToken);
        device.EnsureCanBeDeleted();

        _repository.Remove(device);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Device> GetDeviceOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var device = await _repository.GetByIdAsync(id, cancellationToken);
        if (device is null)
            throw new DeviceNotFoundException(id);

        return device;
    }
}
