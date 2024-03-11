using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderBookApiHW.Logger.Context;

namespace OrderBookApiHW.Logger.Registration;

public static class Registration
{
    public static void Migrate(IServiceProvider services)
    {
        try
        {
            var context = services.GetRequiredService<LoggerContext>();
            context.Database.Migrate();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<LoggerContext>>();
            logger.LogError(ex, "An error occurred while migrating the database.");
        }
    }
}