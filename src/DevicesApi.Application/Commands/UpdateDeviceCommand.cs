namespace DevicesApi.Application.Commands;

public record UpdateDeviceCommand(string? Name, string? Brand, string? State);
