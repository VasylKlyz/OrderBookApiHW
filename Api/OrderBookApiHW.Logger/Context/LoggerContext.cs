using Microsoft.EntityFrameworkCore;
using OrderBookApiHW.Logger.Entity;

namespace OrderBookApiHW.Logger.Context;

public class LoggerContext: DbContext
{
    public LoggerContext(DbContextOptions<LoggerContext> options)
        : base(options)
    {
    }

    public DbSet<OrderBookLogsEntity> OrderBookLogs { get; set; }
}
