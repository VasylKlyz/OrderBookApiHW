using Bitfinex.Client.Websocket;
using Bitfinex.Client.Websocket.Client;
using Bitfinex.Client.Websocket.Communicator;
using Bitfinex.Client.Websocket.Requests.Subscriptions;
using Bitfinex.Client.Websocket.Utils;
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
    private readonly ILogger<OrderBookService> _logger;

    public OrderBookService(IHubContext<BookOrderHub> hubContext, ILogger<OrderBookService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
        var url = BitfinexValues.ApiWebsocketUrl;

        _communicator = new BitfinexWebsocketCommunicator(url);
        _client = new BitfinexWebsocketClient(_communicator);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.Streams.InfoStream.Subscribe(info =>
        {
            _client.Send(new BookSubscribeRequest(Pair, BitfinexPrecision.P1));
        });

        _client.Streams.BookStream.Subscribe(async book =>
        {
            // Console.WriteLine($"pair {book.Pair} period {book.Period} chanId {book.ChanId} amount {book.Amount} rte {book.Rate} count {book.Count} price {book.Price}");
            _logger.Log(LogLevel.Information, $"Pair {book.Pair} Period {book.Period} ChanId {book.ChanId} Amount {book.Amount} Rate {book.Rate} Count {book.Count} Price {book.Price}");
            await _hubContext.Clients.All.SendAsync("ReceiveBookOrder", book);
        });

        await _communicator.Start();
    }
}