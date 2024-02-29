using Bitfinex.Client.Websocket;
using Bitfinex.Client.Websocket.Client;
using Bitfinex.Client.Websocket.Communicator;
using Bitfinex.Client.Websocket.Requests.Subscriptions;
using Bitfinex.Client.Websocket.Responses.Books;
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
    private List<Book> _books;
    private const int Depth = 15;

    public OrderBookService(IHubContext<BookOrderHub> hubContext, ILogger<OrderBookService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
        _books = new List<Book>();
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
            _books.Add(book);

            if (book.Count == 0)
            {
                if (book.Amount == 1)
                {
                    _books.RemoveAll(x => x.Price == book.Price && x.Amount > 0);
                }
                else if (book.Amount == -1)
                {
                    _books.RemoveAll(x => x.Price == book.Price && x.Amount < 0);
                }
            }

            // Console.WriteLine($"pair {book.Pair} period {book.Period} chanId {book.ChanId} amount {book.Amount} rte {book.Rate} count {book.Count} price {book.Price}");
            //_logger.Log(LogLevel.Information, $"Pair {book.Pair} Period {book.Period} ChanId {book.ChanId} Amount {book.Amount} Rate {book.Rate} Count {book.Count} Price {book.Price} ServerTimestamp {book.ServerTimestamp}");
            else
            {
                if (book.Amount > 0)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveBookOrderBid", GetBid(), cancellationToken: stoppingToken);
                }
                else if (book.Amount < 0)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveBookOrderAsk", GetAsk(), cancellationToken: stoppingToken);
                }
            }
        });

        await _communicator.Start();
    }

    private Dictionary<string, List<Book>> GetAsk()
    {
        var result = _books
            .Where(book => book.Amount < 0)
            .OrderBy(book => book.Price)
            .GroupBy(book => book.Price)
            .Take(Depth)
            .ToDictionary(
                group => group.Key.ToString(),
                group => group.ToList()
            );
        
        return result;
    }
    
    private Dictionary<string, List<Book>> GetBid()
    {
        var result = _books
            .Where(book => book.Amount > 0)
            .OrderByDescending(book => book.Price)
            .GroupBy(book => book.Price)
            .Take(Depth)
            .ToDictionary(
                group => group.Key.ToString(),
                group => group.ToList()
            );
        
        return result;
    }
}