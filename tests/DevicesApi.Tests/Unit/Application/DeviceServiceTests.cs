using DevicesApi.Application.Commands;
using DevicesApi.Application.Services;
using DevicesApi.Domain.Entities;
using DevicesApi.Domain.Enums;
using DevicesApi.Domain.Exceptions;
using DevicesApi.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace DevicesApi.Tests.Unit.Application;

public class DeviceServiceTests
{
    private readonly IDeviceRepository _repository;
    private readonly DeviceService _service;

    public DeviceServiceTests()
    {
        _repository = Substitute.For<IDeviceRepository>();
        _service = new DeviceService(_repository);
    }

    [Fact]
    public async Task CreateAsync_WithValidCommand_ShouldReturnCreatedDevice()
    {
        var command = new CreateDeviceCommand("iPhone 15", "Apple", "Available");

        var result = await _service.CreateAsync(command);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("iPhone 15");
        result.Brand.Should().Be("Apple");
        result.State.Should().Be("Available");
        result.CreationTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _repository.Received(1).Add(Arg.Any<Device>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithInvalidState_ShouldThrowInvalidDeviceStateException()
    {
        var command = new CreateDeviceCommand("Device", "Brand", "InvalidState");

        var act = () => _service.CreateAsync(command);

        await act.Should().ThrowAsync<InvalidDeviceStateException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldTrimNameAndBrand()
    {
        var command = new CreateDeviceCommand("  iPhone 15  ", "  Apple  ", "Available");

        var result = await _service.CreateAsync(command);

        result.Name.Should().Be("iPhone 15");
        result.Brand.Should().Be("Apple");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingDevice_ShouldReturnDevice()
    {
        var device = Device.Create("Pixel 8", "Google", DeviceState.Available);
        _repository.GetByIdAsync(device.Id, Arg.Any<CancellationToken>()).Returns(device);

        var result = await _service.GetByIdAsync(device.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(device.Id);
        result.Name.Should().Be("Pixel 8");
        result.Brand.Should().Be("Google");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldThrowDeviceNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Device?)null);

        var act = () => _service.GetByIdAsync(id);

        await act.Should().ThrowAsync<DeviceNotFoundException>();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllDevices()
    {
        var devices = new List<Device>
        {
            Device.Create("Device 1", "Brand A", DeviceState.Available),
            Device.Create("Device 2", "Brand B", DeviceState.InUse),
            Device.Create("Device 3", "Brand C", DeviceState.Inactive)
        };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(devices);

        var result = await _service.GetAllAsync();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_WithNoDevices_ShouldReturnEmptyList()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Device>());

        var result = await _service.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByBrandAsync_ShouldReturnMatchingDevices()
    {
        var devices = new List<Device>
        {
            Device.Create("iPhone 14", "Apple", DeviceState.Available),
            Device.Create("iPhone 15", "Apple", DeviceState.InUse)
        };
        _repository.GetByBrandAsync("Apple", Arg.Any<CancellationToken>()).Returns(devices);

        var result = await _service.GetByBrandAsync("Apple");

        result.Should().HaveCount(2);
        result.Should().OnlyContain(d => d.Brand == "Apple");
    }

    [Fact]
    public async Task GetByBrandAsync_WithNonExistingBrand_ShouldReturnEmptyList()
    {
        _repository.GetByBrandAsync("NonExistent", Arg.Any<CancellationToken>()).Returns(new List<Device>());

        var result = await _service.GetByBrandAsync("NonExistent");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByStateAsync_ShouldReturnMatchingDevices()
    {
        var devices = new List<Device>
        {
            Device.Create("Device 1", "Brand A", DeviceState.Available),
            Device.Create("Device 2", "Brand B", DeviceState.Available)
        };
        _repository.GetByStateAsync(DeviceState.Available, Arg.Any<CancellationToken>()).Returns(devices);

        var result = await _service.GetByStateAsync("Available");

        result.Should().HaveCount(2);
        result.Should().OnlyContain(d => d.State == "Available");
    }

    [Fact]
    public async Task GetByStateAsync_WithInvalidState_ShouldThrowInvalidDeviceStateException()
    {
        var act = () => _service.GetByStateAsync("InvalidState");

        await act.Should().ThrowAsync<InvalidDeviceStateException>();
    }

    [Fact]
    public async Task UpdateAsync_WithPartialUpdate_ShouldUpdateOnlyProvidedFields()
    {
        var device = Device.Create("Old Name", "Old Brand", DeviceState.Available);
        _repository.GetByIdAsync(device.Id, Arg.Any<CancellationToken>()).Returns(device);

        var command = new UpdateDeviceCommand("New Name", null, null);

        var result = await _service.UpdateAsync(device.Id, command);

        result.Name.Should().Be("New Name");
        result.Brand.Should().Be("Old Brand");
        result.State.Should().Be("Available");
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WithFullUpdate_ShouldUpdateAllFields()
    {
        var device = Device.Create("Old Name", "Old Brand", DeviceState.Available);
        _repository.GetByIdAsync(device.Id, Arg.Any<CancellationToken>()).Returns(device);

        var command = new UpdateDeviceCommand("New Name", "New Brand", "Inactive");

        var result = await _service.UpdateAsync(device.Id, command);

        result.Name.Should().Be("New Name");
        result.Brand.Should().Be("New Brand");
        result.State.Should().Be("Inactive");
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotUpdateCreationTime()
    {
        var device = Device.Create("Device", "Brand", DeviceState.Available);
        var originalTime = device.CreationTime;
        _repository.GetByIdAsync(device.Id, Arg.Any<CancellationToken>()).Returns(device);

        var command = new UpdateDeviceCommand("Updated", null, null);

        var result = await _service.UpdateAsync(device.Id, command);

        result.CreationTime.Should().Be(originalTime);
    }

    [Fact]
    public async Task UpdateAsync_InUseDevice_ShouldNotAllowNameUpdate()
    {
        var device = Device.Create("Original", "Brand", DeviceState.InUse);
        _repository.GetByIdAsync(device.Id, Arg.Any<CancellationToken>()).Returns(device);

        var command = new UpdateDeviceCommand("Changed", null, null);

        var act = () => _service.UpdateAsync(device.Id, command);

        await act.Should().ThrowAsync<DeviceInUseException>();
    }

    [Fact]
    public async Task UpdateAsync_InUseDevice_ShouldNotAllowBrandUpdate()
    {
        var device = Device.Create("Device", "Original Brand", DeviceState.InUse);
        _repository.GetByIdAsync(device.Id, Arg.Any<CancellationToken>()).Returns(device);

        var command = new UpdateDeviceCommand(null, "New Brand", null);

        var act = () => _service.UpdateAsync(device.Id, command);

        await act.Should().ThrowAsync<DeviceInUseException>();
    }

    [Fact]
    public async Task UpdateAsync_InUseDevice_ShouldAllowStateUpdate()
    {
        var device = Device.Create("Device", "Brand", DeviceState.InUse);
        _repository.GetByIdAsync(device.Id, Arg.Any<CancellationToken>()).Returns(device);

        var command = new UpdateDeviceCommand(null, null, "Available");

        var result = await _service.UpdateAsync(device.Id, command);

        result.State.Should().Be("Available");
    }

    [Fact]
    public async Task UpdateAsync_InUseDevice_ShouldAllowSameNameAndBrand()
    {
        var device = Device.Create("Device", "Brand", DeviceState.InUse);
        _repository.GetByIdAsync(device.Id, Arg.Any<CancellationToken>()).Returns(device);

        var command = new UpdateDeviceCommand("Device", "Brand", null);

        var result = await _service.UpdateAsync(device.Id, command);

        result.Name.Should().Be("Device");
        result.Brand.Should().Be("Brand");
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingId_ShouldThrowDeviceNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Device?)null);

        var command = new UpdateDeviceCommand("New", null, null);

        var act = () => _service.UpdateAsync(id, command);

        await act.Should().ThrowAsync<DeviceNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WithAvailableDevice_ShouldDeleteDevice()
    {
        var device = Device.Create("Device", "Brand", DeviceState.Available);
        _repository.GetByIdAsync(device.Id, Arg.Any<CancellationToken>()).Returns(device);

        await _service.DeleteAsync(device.Id);

        _repository.Received(1).Remove(device);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WithInactiveDevice_ShouldDeleteDevice()
    {
        var device = Device.Create("Device", "Brand", DeviceState.Inactive);
        _repository.GetByIdAsync(device.Id, Arg.Any<CancellationToken>()).Returns(device);

        await _service.DeleteAsync(device.Id);

        _repository.Received(1).Remove(device);
    }

    [Fact]
    public async Task DeleteAsync_WithInUseDevice_ShouldThrowDeviceInUseException()
    {
        var device = Device.Create("Device", "Brand", DeviceState.InUse);
        _repository.GetByIdAsync(device.Id, Arg.Any<CancellationToken>()).Returns(device);

        var act = () => _service.DeleteAsync(device.Id);

        await act.Should().ThrowAsync<DeviceInUseException>();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ShouldThrowDeviceNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Device?)null);

        var act = () => _service.DeleteAsync(id);

        await act.Should().ThrowAsync<DeviceNotFoundException>();
    }
}
