using System.ComponentModel.DataAnnotations;
using DevicesApi.Domain.Enums;

namespace DevicesApi.Api.DTOs;

public class UpdateDeviceRequest
{
    [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
    public string? Name { get; set; }

    [MaxLength(200, ErrorMessage = "Brand cannot exceed 200 characters.")]
    public string? Brand { get; set; }

    [EnumDataType(typeof(DeviceState), ErrorMessage = "State must be one of: Available, InUse, Inactive.")]
    public string? State { get; set; }
}
