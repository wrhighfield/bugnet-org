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
            logger.Information("Attempting to run migrations for BugNet");
            appContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to apply migrations for BugNet");
            throw;
        }

        logger.Information("Migrations for BugNet completed");
		return webApplication;
    }

    public static WebApplication ApplyIdentitySchema(this WebApplication webApplication, Logger logger)
    {
        using var scope = webApplication.Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        try
        {
            logger.Information("Attempting to run migrations for Identity");
            appContext.Database.Migrate();
        }
        catch (Exception ex)
        {
	        logger.Error(ex, "Failed to apply migrations for Identity");
			throw;
        }

        logger.Information("Migrations for BugNet Identity");
		return webApplication;
    }
}