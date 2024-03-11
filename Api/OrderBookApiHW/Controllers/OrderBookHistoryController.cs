using Microsoft.AspNetCore.Mvc;
using OrderBookApiHW.Services;

namespace OrderBookApiHW.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderBookHistoryController: ControllerBase
{
    private readonly IOrderBookHistoryService _orderBookHistoryService;
    
    public OrderBookHistoryController(IOrderBookHistoryService orderBookHistoryService)
    {
        _orderBookHistoryService = orderBookHistoryService;
    }
    
    [HttpGet("GetHistory")]
    public async Task<IActionResult> GetHistory([FromQuery]DateTime upToDate)
    {
        var result = await _orderBookHistoryService.GetDayHistory(upToDate);
        
        return Ok(result);
    }
}