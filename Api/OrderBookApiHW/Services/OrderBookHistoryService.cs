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
        /// In case the application is down, our book instantly stays not consistent and to fix that issue we should move to book snapshots.
        /// But with the current implementation to get the correct history we need to run our application nonstop.
        /// </summary>
        /// <param name="upToDateTime"></param>
        /// <returns></returns>
        public async Task<BookHistoryModel> GetDayHistory(DateTime upToDateTime)
        {
            var result = new BookHistoryModel();
            // edge case: We are not sure if all book orders was closed (count == 0) from the previous day
            var startOfDay = upToDateTime.Date.AddDays(-1);

            // Get history of day for bids and asks
            var bidsOfTheDay = _loggerContext.OrderBookLogs
                .Where(book => book.Amount > 0 && book.OrderTime >= startOfDay && book.OrderTime <= upToDateTime)
                .AsEnumerable();

            var asksOfTheDay = _loggerContext.OrderBookLogs
                .Where(book => book.Amount < 0 && book.OrderTime >= startOfDay && book.OrderTime <= upToDateTime)
                .AsEnumerable();

            // Group orders, remove prices where most recent book order was deleted (count == 0) 
            result.Bids = await ProcessBooks(bidsOfTheDay, true);
            result.Asks = await ProcessBooks(asksOfTheDay, false);

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