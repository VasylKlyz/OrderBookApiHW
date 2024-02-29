using Bitfinex.Client.Websocket;
using Bitfinex.Client.Websocket.Client;
using Bitfinex.Client.Websocket.Communicator;
using Bitfinex.Client.Websocket.Requests.Subscriptions;
using Bitfinex.Client.Websocket.Websockets;
using Microsoft.AspNetCore.SignalR;
using OrderBookApiHW.Hubs;

namespace OrderBookApiHW.Services;

public class OrderBookService : BackgroundService
{
    private readonly IBitfinexCommunicator _communicator;
    private readonly BitfinexWebsocketClient _client;
    private readonly IHubContext<BookOrderHub> _hubContext;
    private const string Pair = "BTC/EUR";

    public OrderBookService(IHubContext<BookOrderHub> hubContext)
    {
        _hubContext = hubContext;
        var url = BitfinexValues.ApiWebsocketUrl;

        _communicator = new BitfinexWebsocketCommunicator(url);
        _client = new BitfinexWebsocketClient(_communicator);
        
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.Streams.InfoStream.Subscribe(info =>
        {
            _client.Send(new BookSubscribeRequest(Pair));
        });

        _client.Streams.BookStream.Subscribe(async book =>
        {
            var message = $"Book Amount: {book.Amount}, Book Price: {book.Price}, Book Rate: {book.Rate}, {book.Count} {book.Period} ";
            Console.WriteLine(message);

            await _hubContext.Clients.All.SendAsync("ReceiveBookOrder", book);
        });

        await _communicator.Start();
    }
}