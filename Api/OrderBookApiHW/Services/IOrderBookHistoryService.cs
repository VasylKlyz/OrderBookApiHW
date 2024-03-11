using OrderBookApiHW.Models;

namespace OrderBookApiHW.Services;

public interface IOrderBookHistoryService
{
    public Task<BookHistoryModel> GetDayHistory(DateTime upToDateTime);
}