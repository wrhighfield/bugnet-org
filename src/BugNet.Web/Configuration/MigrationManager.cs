using BugNet.Data;

namespace BugNet.Web.Configuration;

internal static class MigrationManager
{
    public static WebApplication ApplyBugnetSchema(this WebApplication webApplication, Logger logger)
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
            logger.Error(ex, "Failed to apply migrations {context}", nameof(BugNetDbContext));
            throw;
        }

        return webApplication;
    }

    public static WebApplication ApplyIdentitySchema(this WebApplication webApplication, Logger logger)
    {
        using var scope = webApplication.Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        try
        {
            logger.Information("Attempting to run migrations");
            appContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to apply migrations {context}", nameof(IdentityDbContext));
            throw;
        }

        return webApplication;
    }
}