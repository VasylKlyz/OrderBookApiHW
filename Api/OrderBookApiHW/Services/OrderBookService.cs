using Bitfinex.Client.Websocket;
using Bitfinex.Client.Websocket.Client;
using Bitfinex.Client.Websocket.Communicator;
using Bitfinex.Client.Websocket.Requests.Subscriptions;
using Bitfinex.Client.Websocket.Responses.Books;
using Bitfinex.Client.Websocket.Websockets;
using Microsoft.AspNetCore.SignalR;
using OrderBookApiHW.Hubs;
using OrderBookApiHW.Logger.Logger;

namespace OrderBookApiHW.Services;

public class OrderBookService : BackgroundService
{
    private readonly IBitfinexCommunicator _communicator;
    private readonly BitfinexWebsocketClient _client;
    private readonly IHubContext<BookOrderHub> _hubContext;
    private const string Pair = "BTC/EUR";
    private List<Book> _bookBid;
    private List<Book> _booksAsk;
    
    // Amount of taken books for both bids and asks
    private const int Depth = 15;
    private readonly IServiceScopeFactory _scopeFactory;
    
    public OrderBookService(IHubContext<BookOrderHub> hubContext, IServiceScopeFactory scopeFactory)
    {
        _hubContext = hubContext;
        _scopeFactory = scopeFactory;
        _bookBid = new List<Book>();
        _booksAsk = new List<Book>();
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
            await using var scope = _scopeFactory.CreateAsyncScope();
            var logger = scope.ServiceProvider.GetRequiredService<IOrderBookLogger>();
            
            logger.Log(book);
            
            // Those logic goes from the documentation to keep trading book instance updated
            /*
             * Algorithm to create and keep a trading book instance updated
             *   when count > 0 then you have to add or update the price level
             *   3.1 if amount > 0 then add/update bids
             *   3.2 if amount < 0 then add/update asks
             *   when count = 0 then you have to delete the price level.
             *   4.1 if amount = 1 then remove from bids
             *   4.2 if amount = -1 then remove from asks
             */
            if (book.Count == 0)
            {
                if (book.Amount == 1)
                {
                    _bookBid.RemoveAll(x => x.Price == book.Price);
                }
                else if (book.Amount == -1)
                {
                    _booksAsk.RemoveAll(x => x.Price == book.Price);
                }
            }
            else
            {
                if (book.Amount > 0)
                {
                    AddOrUpdate(book, _bookBid);
                }
                else if (book.Amount < 0)
                {
                    AddOrUpdate(book, _booksAsk);
                }
            }
            
            // Notify all subscribers with current bid or ask
            if (book.Amount > 0)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveBookOrderBid", GetGrouped(_bookBid), cancellationToken: stoppingToken);
            }
            else if (book.Amount < 0)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveBookOrderAsk", GetGrouped(_booksAsk), cancellationToken: stoppingToken);
            }
        });

        await _communicator.Start();
    }

    /// <summary>
    /// Add to the book new value in case we do not have it. Update current if we have.
    /// </summary>
    /// <param name="newBook"></param>
    /// <param name="books"></param>
    private void AddOrUpdate(Book newBook, List<Book> books)
    {
        var existing = books.SingleOrDefault(x => x.Price == newBook.Price);
                    
        if (existing is not null)
        {
            existing.Amount = newBook.Amount;
            existing.Count = newBook.Count;
        }
        else
        {
            books.Add(newBook);
        }
    }
    
    // Group Book by prices
    private Dictionary<string, Book> GetGrouped(List<Book> books)
    {
        var result = books
            .OrderBy(book => book.Price)
            .Take(Depth)
            .ToDictionary(
                book => book.Price.ToString(),
                book => book
            );
        
        return result;
    }
}