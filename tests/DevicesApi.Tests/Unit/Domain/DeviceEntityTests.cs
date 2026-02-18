using DevicesApi.Domain.Entities;
using DevicesApi.Domain.Enums;
using DevicesApi.Domain.Exceptions;
using FluentAssertions;

namespace DevicesApi.Tests.Unit.Domain;

public class DeviceEntityTests
{
    [Fact]
    public void Create_WithValidInputs_ShouldCreateDevice()
    {
        var device = Device.Create("iPhone 15", "Apple", DeviceState.Available);

        device.Id.Should().NotBeEmpty();
        device.Name.Should().Be("iPhone 15");
        device.Brand.Should().Be("Apple");
        device.State.Should().Be(DeviceState.Available);
        device.CreationTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_ShouldTrimNameAndBrand()
    {
        var device = Device.Create("  iPhone 15  ", "  Apple  ", DeviceState.Available);

        device.Name.Should().Be("iPhone 15");
        device.Brand.Should().Be("Apple");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowDomainException(string? name)
    {
        var act = () => Device.Create(name!, "Apple", DeviceState.Available);

        act.Should().Throw<DomainException>().WithMessage("*name*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidBrand_ShouldThrowDomainException(string? brand)
    {
        var act = () => Device.Create("iPhone", brand!, DeviceState.Available);

        act.Should().Throw<DomainException>().WithMessage("*brand*");
    }

    [Fact]
    public void UpdateName_WhenAvailable_ShouldUpdateName()
    {
        var device = Device.Create("Old Name", "Brand", DeviceState.Available);

        device.UpdateName("New Name");

        device.Name.Should().Be("New Name");
    }

    [Fact]
    public void UpdateName_ShouldTrimName()
    {
        var device = Device.Create("Old", "Brand", DeviceState.Available);

        device.UpdateName("  New Name  ");

        device.Name.Should().Be("New Name");
    }

    [Fact]
    public void UpdateName_WhenInUse_WithDifferentName_ShouldThrowDeviceInUseException()
    {
        var device = Device.Create("Original", "Brand", DeviceState.Available);
        device.ChangeState(DeviceState.InUse);

        var act = () => device.UpdateName("Changed");

        act.Should().Throw<DeviceInUseException>();
    }

    [Fact]
    public void UpdateName_WhenInUse_WithSameName_ShouldSucceed()
    {
        var device = Device.Create("SameName", "Brand", DeviceState.Available);
        device.ChangeState(DeviceState.InUse);

        device.UpdateName("SameName");

        device.Name.Should().Be("SameName");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_WithInvalidName_ShouldThrowDomainException(string? name)
    {
        var device = Device.Create("Name", "Brand", DeviceState.Available);

        var act = () => device.UpdateName(name!);

        act.Should().Throw<DomainException>().WithMessage("*name*");
    }

    [Fact]
    public void UpdateBrand_WhenAvailable_ShouldUpdateBrand()
    {
        var device = Device.Create("Name", "Old Brand", DeviceState.Available);

        device.UpdateBrand("New Brand");

        device.Brand.Should().Be("New Brand");
    }

    [Fact]
    public void UpdateBrand_WhenInUse_WithDifferentBrand_ShouldThrowDeviceInUseException()
    {
        var device = Device.Create("Name", "Original", DeviceState.Available);
        device.ChangeState(DeviceState.InUse);

        var act = () => device.UpdateBrand("Changed");

        act.Should().Throw<DeviceInUseException>();
    }

    [Fact]
    public void UpdateBrand_WhenInUse_WithSameBrand_ShouldSucceed()
    {
        var device = Device.Create("Name", "SameBrand", DeviceState.Available);
        device.ChangeState(DeviceState.InUse);

        device.UpdateBrand("SameBrand");

        device.Brand.Should().Be("SameBrand");
    }

    [Theory]
    [InlineData(DeviceState.Available, DeviceState.InUse)]
    [InlineData(DeviceState.InUse, DeviceState.Available)]
    [InlineData(DeviceState.Available, DeviceState.Inactive)]
    [InlineData(DeviceState.Inactive, DeviceState.Available)]
    public void ChangeState_ShouldUpdateState(DeviceState initial, DeviceState target)
    {
        var device = Device.Create("Name", "Brand", initial);

        device.ChangeState(target);

        device.State.Should().Be(target);
    }

    [Fact]
    public void EnsureCanBeDeleted_WhenAvailable_ShouldNotThrow()
    {
        var device = Device.Create("Name", "Brand", DeviceState.Available);

        var act = () => device.EnsureCanBeDeleted();

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCanBeDeleted_WhenInactive_ShouldNotThrow()
    {
        var device = Device.Create("Name", "Brand", DeviceState.Inactive);

        var act = () => device.EnsureCanBeDeleted();

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCanBeDeleted_WhenInUse_ShouldThrowDeviceInUseException()
    {
        var device = Device.Create("Name", "Brand", DeviceState.InUse);

        var act = () => device.EnsureCanBeDeleted();

        act.Should().Throw<DeviceInUseException>();
    }

    [Theory]
    [InlineData("Available", DeviceState.Available)]
    [InlineData("InUse", DeviceState.InUse)]
    [InlineData("Inactive", DeviceState.Inactive)]
    [InlineData("available", DeviceState.Available)]
    [InlineData("inuse", DeviceState.InUse)]
    [InlineData("INACTIVE", DeviceState.Inactive)]
    public void ParseState_WithValidInput_ShouldReturnCorrectState(string input, DeviceState expected)
    {
        var result = Device.ParseState(input);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("NotAState")]
    [InlineData("")]
    public void ParseState_WithInvalidInput_ShouldThrowInvalidDeviceStateException(string input)
    {
        var act = () => Device.ParseState(input);

        act.Should().Throw<InvalidDeviceStateException>();
    }

    [Fact]
    public void CreationTime_ShouldBeImmutable_AfterCreation()
    {
        var device = Device.Create("Name", "Brand", DeviceState.Available);
        var originalTime = device.CreationTime;

        device.UpdateName("Updated");
        device.UpdateBrand("Updated Brand");
        device.ChangeState(DeviceState.InUse);

        device.CreationTime.Should().Be(originalTime);
    }
}
