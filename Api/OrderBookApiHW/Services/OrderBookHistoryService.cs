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

            var startOfDay = upToDateTime.Date.AddDays(-1);

            // Get history of day for bids and asks
            var bidsQuery = _loggerContext.OrderBookLogs
                .Where(book => book.Amount > 0 && book.OrderTime >= startOfDay && book.OrderTime <= upToDateTime)
                .AsEnumerable();

            var asksQuery = _loggerContext.OrderBookLogs
                .Where(book => book.Amount < 0 && book.OrderTime >= startOfDay && book.OrderTime <= upToDateTime)
                .AsEnumerable();

            // Group orders, remove 
            result.Bids = await ProcessBooks(bidsQuery, true);
            result.Asks = await ProcessBooks(asksQuery, false);

            return result;
        }

        
        private async Task<Dictionary<string, Book>> ProcessBooks(IEnumerable<OrderBookLogsEntity> query, bool isBid)
        {
            var filteredGroups = query
                .GroupBy(book => book.Price)
                .Select(group => group.OrderByDescending(book => book.OrderTime).First())
                .Where(book => book.Count != 0);

            var orderedGroups = isBid ? filteredGroups.OrderByDescending(book => book.Price) : filteredGroups.OrderBy(book => book.Price);

            return orderedGroups.Take(Depth).ToDictionary(
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