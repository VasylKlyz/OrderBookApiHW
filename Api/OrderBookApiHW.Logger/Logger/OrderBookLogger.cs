using Bitfinex.Client.Websocket.Responses.Books;
using OrderBookApiHW.Logger.Context;
using OrderBookApiHW.Logger.Entity;

namespace OrderBookApiHW.Logger.Logger;

public class OrderBookLogger : IOrderBookLogger
{
    private readonly LoggerContext _dbContext;

    public OrderBookLogger(LoggerContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Log(Book book)
    {
        _dbContext.OrderBookLogs.Add(new OrderBookLogsEntity
        {
            Amount = book.Amount,
            Count = book.Count,
            Pair = book.Pair,
            Price = book.Price,
            Period = book.Period,
            Rate = book.Rate,
            Symbol = book.Symbol,
            OrderTime = DateTime.UtcNow
        });

        _dbContext.SaveChanges();
    }
}