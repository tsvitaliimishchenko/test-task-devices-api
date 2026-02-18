using DevicesApi.Application.Results;
using DevicesApi.Domain.Entities;

namespace DevicesApi.Application.Mappings;

public static class DeviceMapper
{
    public static DeviceResult ToResult(Device device)
    {
        return new DeviceResult(
            device.Id,
            device.Name,
            device.Brand,
            device.State.ToString(),
            device.CreationTime);
    }

    public static IEnumerable<DeviceResult> ToResultList(IEnumerable<Device> devices)
    {
        return devices.Select(ToResult);
    }
}
