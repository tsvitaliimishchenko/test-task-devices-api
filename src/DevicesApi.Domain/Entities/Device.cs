using DevicesApi.Domain.Enums;
using DevicesApi.Domain.Exceptions;

namespace DevicesApi.Domain.Entities;

public class Device
{
    private Device() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Brand { get; private set; } = string.Empty;
    public DeviceState State { get; private set; }
    public DateTime CreationTime { get; private set; }

    public static Device Create(string name, string brand, DeviceState state)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Device name is required.");

        if (string.IsNullOrWhiteSpace(brand))
            throw new DomainException("Device brand is required.");

        return new Device
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Brand = brand.Trim(),
            State = state,
            CreationTime = DateTime.UtcNow
        };
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("Device name is required.");

        var trimmed = newName.Trim();

        if (State == DeviceState.InUse && trimmed != Name)
            throw new DeviceInUseException("update the name of");

        Name = trimmed;
    }

    public void UpdateBrand(string newBrand)
    {
        if (string.IsNullOrWhiteSpace(newBrand))
            throw new DomainException("Device brand is required.");

        var trimmed = newBrand.Trim();

        if (State == DeviceState.InUse && trimmed != Brand)
            throw new DeviceInUseException("update the brand of");

        Brand = trimmed;
    }

    public void ChangeState(DeviceState newState)
    {
        State = newState;
    }

    public void EnsureCanBeDeleted()
    {
        if (State == DeviceState.InUse)
            throw new DeviceInUseException("delete");
    }

    public static DeviceState ParseState(string state)
    {
        if (Enum.TryParse<DeviceState>(state.Trim(), ignoreCase: true, out var parsed))
            return parsed;

        throw new InvalidDeviceStateException(state);
    }
}
