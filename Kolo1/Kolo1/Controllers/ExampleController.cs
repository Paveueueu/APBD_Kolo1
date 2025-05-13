using Microsoft.AspNetCore.Mvc;
using Kolokwium1.Models;
using Kolokwium1.Services;

namespace Kolokwium1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExampleController : ControllerBase
{
    private readonly IExampleService _service;

    public ExampleController(IExampleService service)
    {
        _service = service;
    }

    [HttpGet("deliveries/{deliveryId}")]
    public async Task<IActionResult> GetDeliveries(int deliveryId)
    {
        var result = await _service.GetDeliveries(deliveryId);
        return Ok(result);
    }
    
    [HttpPost("deliveries")]
    public async Task<IActionResult> AddDelivery([FromBody] NewDeliveryDto dto)
    {
        var result = await _service.AddDelivery(dto);
        return Created($"/api/delivery/{result}/", new { MyId = result });
    }
}