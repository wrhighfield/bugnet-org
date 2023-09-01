using BugNet.Data;
using Serilog.Core;

namespace BugNet.Web.Configuration;

internal static class MigrationManager
{
    public static WebApplication MigrateDatabase(this WebApplication webApplication, Logger logger)
    {
        using var scope = webApplication.Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<BugNetDbContext>();

        try
        {
            logger.Information("Attempting to run migrations");
            appContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to apply migrations");
            throw;
        }

        return webApplication;
    }
}