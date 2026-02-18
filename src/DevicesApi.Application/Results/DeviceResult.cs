namespace DevicesApi.Application.Results;

public record DeviceResult(
    Guid Id,
    string Name,
    string Brand,
    string State,
    DateTime CreationTime);
