using Bitfinex.Client.Websocket.Responses.Books;

namespace OrderBookApiHW.Models;

public class BookHistoryModel
{
    public Dictionary<string, Book> Bids { get; set; }
    public Dictionary<string, Book> Asks { get; set; }
}