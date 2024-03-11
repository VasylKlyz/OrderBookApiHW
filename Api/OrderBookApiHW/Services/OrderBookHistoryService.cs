using Bitfinex.Client.Websocket.Responses.Books;
using OrderBookApiHW.Logger.Context;
using OrderBookApiHW.Models;
using OrderBookApiHW.Logger.Entity;

namespace OrderBookApiHW.Services
{
    public class OrderBookHistoryService : IOrderBookHistoryService
    {
        private readonly LoggerContext _loggerContext;
        private const int Depth = 25;

        public OrderBookHistoryService(LoggerContext loggerContext)
        {
            _loggerContext = loggerContext;
        }
        
        /// <summary>
        /// Return history of book from start of the day up to chosen date time
        /// </summary>
        /// <param name="upToDateTime"></param>
        /// <returns></returns>
        public async Task<BookHistoryModel> GetDayHistory(DateTime upToDateTime)
        {
            var result = new BookHistoryModel();

            var startOfDay = upToDateTime.Date;
            var endOfDay = upToDateTime.Date.AddDays(1).AddTicks(-1);

            // Get history of day for bids and asks
            var bidsQuery = _loggerContext.OrderBookLogs
                .Where(book => book.Amount > 0 && book.OrderTime >= startOfDay && book.OrderTime <= endOfDay)
                .AsEnumerable();

            var asksQuery = _loggerContext.OrderBookLogs
                .Where(book => book.Amount < 0 && book.OrderTime >= startOfDay && book.OrderTime <= endOfDay)
                .AsEnumerable();

            // Group orders, remove 
            result.Bids = await ProcessBooks(bidsQuery);
            result.Asks = await ProcessBooks(asksQuery);

            return result;
        }

        private async Task<Dictionary<string, Book>> ProcessBooks(IEnumerable<OrderBookLogsEntity> query)
        {
            var filteredGroups = query
                .GroupBy(book => book.Price)
                .Where(group => group.All(book => book.Count > 0)) // if price contains any Count == 0 remove that book
                .Select(group => group
                    .OrderByDescending(book => book.OrderTime)
                    .First()) // Get most recent book for the price point
                .OrderBy(book => book.Price)
                .Take(Depth);

            return filteredGroups.ToDictionary(
                book => book.Price.ToString(),
                book => new Book
                {
                    Amount = book.Amount,
                    Count = book.Count,
                    Pair = book.Pair,
                    Price = book.Price,
                    Period = book.Period,
                    Rate = book.Rate,
                    Symbol = book.Symbol
                });
        }
    }
}