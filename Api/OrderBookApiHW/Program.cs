using Microsoft.EntityFrameworkCore;
using OrderBookApiHW.Hubs;
using OrderBookApiHW.Logger.Context;
using OrderBookApiHW.Logger.Logger;
using OrderBookApiHW.Logger.Registration;
using OrderBookApiHW.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure services and policy
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddHostedService<OrderBookService>();
builder.Services.AddScoped<IOrderBookLogger, OrderBookLogger>();
builder.Services.AddScoped<IOrderBookHistoryService, OrderBookHistoryService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", conf =>
        conf.WithOrigins(builder.Configuration.GetValue<string>("FrontendLink"))
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.Services.AddDbContext<LoggerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Setup application
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseRouting();

app.MapControllers();

app.MapHub<BookOrderHub>("/orderBookHub");

// Apply migration 
var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
Registration.Migrate(services);

app.Run();