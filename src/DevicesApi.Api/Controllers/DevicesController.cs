using DevicesApi.Api.DTOs;
using DevicesApi.Application.Commands;
using DevicesApi.Application.Interfaces;
using DevicesApi.Application.Results;
using Microsoft.AspNetCore.Mvc;

namespace DevicesApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;

    public DevicesController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDeviceRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateDeviceCommand(request.Name, request.Brand, request.State);
        var result = await _deviceService.CreateAsync(command, cancellationToken);
        var response = ToResponse(result);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deviceService.GetByIdAsync(id, cancellationToken);
        return Ok(ToResponse(result));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DeviceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? brand = null,
        [FromQuery] string? state = null,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<DeviceResult> results;

        if (!string.IsNullOrWhiteSpace(brand))
            results = await _deviceService.GetByBrandAsync(brand, cancellationToken);
        else if (!string.IsNullOrWhiteSpace(state))
            results = await _deviceService.GetByStateAsync(state, cancellationToken);
        else
            results = await _deviceService.GetAllAsync(cancellationToken);

        return Ok(results.Select(ToResponse));
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDeviceRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateDeviceCommand(request.Name, request.Brand, request.State);
        var result = await _deviceService.UpdateAsync(id, command, cancellationToken);

        return Ok(ToResponse(result));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> FullUpdate(Guid id, [FromBody] UpdateDeviceRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Name is required for a full update." });

        if (string.IsNullOrWhiteSpace(request.Brand))
            return BadRequest(new { message = "Brand is required for a full update." });

        if (string.IsNullOrWhiteSpace(request.State))
            return BadRequest(new { message = "State is required for a full update." });

        var command = new UpdateDeviceCommand(request.Name, request.Brand, request.State);
        var result = await _deviceService.UpdateAsync(id, command, cancellationToken);

        return Ok(ToResponse(result));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _deviceService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private static DeviceResponse ToResponse(DeviceResult result)
    {
        return new DeviceResponse
        {
            Id = result.Id,
            Name = result.Name,
            Brand = result.Brand,
            State = result.State,
            CreationTime = result.CreationTime
        };
    }
}
