using Microsoft.AspNetCore.Mvc;

namespace OrderBookApiHW.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderBookController: ControllerBase
{
    private readonly ILogger<OrderBookController> _logger;

    public OrderBookController(ILogger<OrderBookController> logger)
    {
        _logger = logger;
    }
    
    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }
}