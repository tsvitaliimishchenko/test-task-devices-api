namespace DevicesApi.Domain.Exceptions;

public class DeviceNotFoundException : DomainException
{
    public DeviceNotFoundException(Guid id)
        : base($"Device with identifier '{id}' was not found.") { }
}
