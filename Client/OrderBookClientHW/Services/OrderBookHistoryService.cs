using System.Net.Http.Json;
using OrderBookClientHW.Models;

namespace OrderBookClientHW.Services;

public class OrderBookHistoryService
{
    private readonly HttpClient _httpClient;

    public OrderBookHistoryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BookHistoryModel?> GetHistory(DateTime upToDate)
    {
        var upToDateEncoded = Uri.EscapeDataString(upToDate.ToString("o"));
        var url = $"OrderBookHistory/GetHistory?upToDate={upToDateEncoded}";
        
        var test = await _httpClient.GetFromJsonAsync<BookHistoryModel>(url);

        return test;
    }
}