namespace DevicesApi.Domain.Exceptions;

public class InvalidDeviceStateException : DomainException
{
    public InvalidDeviceStateException(string state)
        : base($"Invalid device state '{state}'. Valid states are: Available, InUse, Inactive.") { }
}
