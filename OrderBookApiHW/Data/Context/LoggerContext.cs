using Microsoft.EntityFrameworkCore;
using OrderBookApiHW.Data.Entity;

namespace OrderBookApiHW.Data.Context;

public class LoggerContext: DbContext
{
    public LoggerContext(DbContextOptions<LoggerContext> options)
        : base(options)
    {
    }

    public DbSet<OrderBookLogsEntity> OrderBookLogs { get; set; }
}
