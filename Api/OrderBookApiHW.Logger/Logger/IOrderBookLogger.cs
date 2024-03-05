using Bitfinex.Client.Websocket.Responses.Books;

namespace OrderBookApiHW.Logger.Logger;

public interface IOrderBookLogger
{
    public void Log(Book entity);
}