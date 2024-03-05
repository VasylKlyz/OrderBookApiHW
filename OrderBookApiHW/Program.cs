using Microsoft.EntityFrameworkCore;
using OrderBookApiHW.Data.Context;
using OrderBookApiHW.Hubs;
using OrderBookApiHW.Logger;
using OrderBookApiHW.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddHostedService<OrderBookService>();

builder.Services.AddScoped<IOrderBookLogger, OrderBookLogger>();

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


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseRouting();

app.MapControllers();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<BookOrderHub>("/orderBookHub");
});

var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<LoggerContext>();
    context.Database.Migrate();
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating the database.");
}

app.Run();