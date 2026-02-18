using System.Net;
using System.Net.Http.Json;
using DevicesApi.Api.DTOs;
using FluentAssertions;

namespace DevicesApi.Tests.Integration;

public class DevicesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DevicesControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateDevice_WithValidPayload_ShouldReturn201()
    {
        var request = new CreateDeviceRequest
        {
            Name = "iPhone 15",
            Brand = "Apple",
            State = "Available"
        };

        var response = await _client.PostAsJsonAsync("/api/devices", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var device = await response.Content.ReadFromJsonAsync<DeviceResponse>();
        device.Should().NotBeNull();
        device!.Name.Should().Be("iPhone 15");
        device.Brand.Should().Be("Apple");
        device.State.Should().Be("Available");
        device.Id.Should().NotBeEmpty();
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateDevice_WithMissingName_ShouldReturn400()
    {
        var request = new { Brand = "Apple", State = "Available" };

        var response = await _client.PostAsJsonAsync("/api/devices", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateDevice_WithInvalidState_ShouldReturn400()
    {
        var request = new CreateDeviceRequest
        {
            Name = "Device",
            Brand = "Brand",
            State = "InvalidState"
        };

        var response = await _client.PostAsJsonAsync("/api/devices", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetDevice_WithExistingId_ShouldReturn200()
    {
        var created = await CreateTestDeviceAsync("Test Device", "TestBrand", "Available");

        var response = await _client.GetAsync($"/api/devices/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var device = await response.Content.ReadFromJsonAsync<DeviceResponse>();
        device.Should().NotBeNull();
        device!.Id.Should().Be(created.Id);
        device.Name.Should().Be("Test Device");
    }

    [Fact]
    public async Task GetDevice_WithNonExistingId_ShouldReturn404()
    {
        var response = await _client.GetAsync($"/api/devices/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllDevices_ShouldReturn200WithDevices()
    {
        await CreateTestDeviceAsync("Device A", "Brand A", "Available");
        await CreateTestDeviceAsync("Device B", "Brand B", "InUse");

        var response = await _client.GetAsync("/api/devices");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var devices = await response.Content.ReadFromJsonAsync<List<DeviceResponse>>();
        devices.Should().NotBeNull();
        devices!.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetDevicesByBrand_ShouldReturnFilteredDevices()
    {
        await CreateTestDeviceAsync("Device 1", "FilterBrand", "Available");
        await CreateTestDeviceAsync("Device 2", "FilterBrand", "InUse");
        await CreateTestDeviceAsync("Device 3", "OtherBrand", "Available");

        var response = await _client.GetAsync("/api/devices?brand=FilterBrand");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var devices = await response.Content.ReadFromJsonAsync<List<DeviceResponse>>();
        devices.Should().NotBeNull();
        devices!.Should().OnlyContain(d => d.Brand == "FilterBrand");
    }

    [Fact]
    public async Task GetDevicesByState_ShouldReturnFilteredDevices()
    {
        await CreateTestDeviceAsync("Device X", "Brand", "Inactive");

        var response = await _client.GetAsync("/api/devices?state=Inactive");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var devices = await response.Content.ReadFromJsonAsync<List<DeviceResponse>>();
        devices.Should().NotBeNull();
        devices!.Should().OnlyContain(d => d.State == "Inactive");
    }

    [Fact]
    public async Task PartialUpdateDevice_WithValidPayload_ShouldReturn200()
    {
        var created = await CreateTestDeviceAsync("Original", "OrigBrand", "Available");
        var update = new { Name = "Updated Name" };

        var response = await _client.PatchAsJsonAsync($"/api/devices/{created.Id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var device = await response.Content.ReadFromJsonAsync<DeviceResponse>();
        device.Should().NotBeNull();
        device!.Name.Should().Be("Updated Name");
        device.Brand.Should().Be("OrigBrand");
    }

    [Fact]
    public async Task UpdateDevice_InUse_ShouldRejectNameChange()
    {
        var created = await CreateTestDeviceAsync("InUseDevice", "Brand", "InUse");
        var update = new { Name = "NewName" };

        var response = await _client.PatchAsJsonAsync($"/api/devices/{created.Id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateDevice_InUse_ShouldRejectBrandChange()
    {
        var created = await CreateTestDeviceAsync("Device", "OrigBrand", "InUse");
        var update = new { Brand = "NewBrand" };

        var response = await _client.PatchAsJsonAsync($"/api/devices/{created.Id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateDevice_InUse_ShouldAllowStateChange()
    {
        var created = await CreateTestDeviceAsync("Device", "Brand", "InUse");
        var update = new { State = "Available" };

        var response = await _client.PatchAsJsonAsync($"/api/devices/{created.Id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var device = await response.Content.ReadFromJsonAsync<DeviceResponse>();
        device!.State.Should().Be("Available");
    }

    [Fact]
    public async Task UpdateDevice_CreationTimeShouldNotChange()
    {
        var created = await CreateTestDeviceAsync("Device", "Brand", "Available");
        var update = new { Name = "Updated" };

        var response = await _client.PatchAsJsonAsync($"/api/devices/{created.Id}", update);

        var device = await response.Content.ReadFromJsonAsync<DeviceResponse>();
        device!.CreationTime.Should().BeCloseTo(created.CreationTime, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task FullUpdateDevice_WithAllFields_ShouldReturn200()
    {
        var created = await CreateTestDeviceAsync("Original", "OrigBrand", "Available");
        var update = new { Name = "Replaced", Brand = "NewBrand", State = "Inactive" };

        var response = await _client.PutAsJsonAsync($"/api/devices/{created.Id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var device = await response.Content.ReadFromJsonAsync<DeviceResponse>();
        device!.Name.Should().Be("Replaced");
        device.Brand.Should().Be("NewBrand");
        device.State.Should().Be("Inactive");
    }

    [Fact]
    public async Task FullUpdateDevice_WithMissingFields_ShouldReturn400()
    {
        var created = await CreateTestDeviceAsync("Device", "Brand", "Available");
        var update = new { Name = "Only Name" };

        var response = await _client.PutAsJsonAsync($"/api/devices/{created.Id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteDevice_WithAvailableDevice_ShouldReturn204()
    {
        var created = await CreateTestDeviceAsync("ToDelete", "Brand", "Available");

        var response = await _client.DeleteAsync($"/api/devices/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/devices/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteDevice_WithInUseDevice_ShouldReturn409()
    {
        var created = await CreateTestDeviceAsync("InUseDevice", "Brand", "InUse");

        var response = await _client.DeleteAsync($"/api/devices/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DeleteDevice_WithNonExistingId_ShouldReturn404()
    {
        var response = await _client.DeleteAsync($"/api/devices/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<DeviceResponse> CreateTestDeviceAsync(string name, string brand, string state)
    {
        var request = new CreateDeviceRequest
        {
            Name = name,
            Brand = brand,
            State = state
        };

        var response = await _client.PostAsJsonAsync("/api/devices", request);
        response.EnsureSuccessStatusCode();

        var device = await response.Content.ReadFromJsonAsync<DeviceResponse>();
        return device!;
    }
}
