using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OrderBookClientHW;
using OrderBookClientHW.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
{
    var config = sp.GetService<IConfiguration>();
    var url = config["BaseApiUrl"];

    return new HttpClient { BaseAddress = new Uri(url) };
});

builder.Services.AddSingleton<ConnectionStatusService>();
builder.Services.AddSingleton<OrderBookHubClientService>(sp => {
    var config = sp.GetService<IConfiguration>();
    var connectionStatusService = sp.GetRequiredService<ConnectionStatusService>();
    var url = config["OrderBookHub:Url"];
    return new OrderBookHubClientService(connectionStatusService, url);
});

builder.Services.AddScoped<OrderBookHistoryService>();

await builder.Build().RunAsync();