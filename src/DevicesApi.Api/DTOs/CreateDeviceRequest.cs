using System.ComponentModel.DataAnnotations;
using DevicesApi.Domain.Enums;

namespace DevicesApi.Api.DTOs;

public class CreateDeviceRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Brand is required.")]
    [MaxLength(200, ErrorMessage = "Brand cannot exceed 200 characters.")]
    public string Brand { get; set; } = string.Empty;

    [Required(ErrorMessage = "State is required.")]
    [EnumDataType(typeof(DeviceState), ErrorMessage = "State must be one of: Available, InUse, Inactive.")]
    public string State { get; set; } = string.Empty;
}
