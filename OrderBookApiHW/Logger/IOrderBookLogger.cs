using Bitfinex.Client.Websocket.Responses.Books;
using OrderBookApiHW.Data.Entity;

namespace OrderBookApiHW.Logger;

public interface IOrderBookLogger
{
    public void Log(Book entity);
}