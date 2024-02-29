using OrderBookApiHW.Hubs;
using OrderBookApiHW.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddHostedService<OrderBookService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", conf =>
        conf.WithOrigins("https://localhost:44354")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

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

app.Run();