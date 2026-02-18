using DevicesApi.Application.Interfaces;
using DevicesApi.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DevicesApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDeviceService, DeviceService>();
        return services;
    }
}
