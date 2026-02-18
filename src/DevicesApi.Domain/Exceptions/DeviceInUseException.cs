namespace DevicesApi.Domain.Exceptions;

public class DeviceInUseException : DomainException
{
    public DeviceInUseException(string operation)
        : base($"Cannot {operation} a device that is currently in use.") { }
}
