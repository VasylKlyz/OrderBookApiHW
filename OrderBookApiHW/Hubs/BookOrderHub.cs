using Bitfinex.Client.Websocket.Responses.Books;
using Microsoft.AspNetCore.SignalR;

namespace OrderBookApiHW.Hubs;

public class BookOrderHub: Hub
{
    public async Task SendMessage(Book book)
    {
        await Clients.All.SendAsync("ReceiveBookOrder", book);
    }
}