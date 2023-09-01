using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Serilog;
using System.Collections.ObjectModel;
using System.Data;
using BugNet.Data;

namespace BugNet.Web.Configuration;

internal static class DependencyInjectionExtensions
{
    public static WebApplicationBuilder RegisterSqlServer(this WebApplicationBuilder builder)
    {
        var bugNetConnectionString =
            builder
                .Configuration
                .GetConnectionString(DataConstants.BugNetConnectionStringName) ??
            throw new InvalidOperationException($"Connection string '{DataConstants.BugNetConnectionStringName}' not found.");

        var identityConnectionString =
            builder
                .Configuration
                .GetConnectionString(DataConstants.IdentityConnectionStringName) ??
            throw new InvalidOperationException($"Connection string '{DataConstants.IdentityConnectionStringName}' not found.");

        builder
            .Services
            .AddDbContext<IdentityDbContext>(options =>
                options.UseSqlServer(identityConnectionString));

        builder
            .Services
            .AddIdentity<ApplicationUser, ApplicationRole>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddDefaultUI()
            .AddEntityFrameworkStores<IdentityDbContext>();

        builder
            .Services
            .AddDbContext<BugNetDbContext>(options =>
            options.UseSqlServer(bugNetConnectionString));

        builder
            .Services
            .AddDatabaseDeveloperPageExceptionFilter();

        return builder;
    }
    public static WebApplicationBuilder RegisterSerilog(this WebApplicationBuilder builder)
    {
        var connectionString =
            builder
                .Configuration
                .GetConnectionString(DataConstants.BugNetConnectionStringName) ??
            throw new InvalidOperationException($"Connection string '{DataConstants.BugNetConnectionStringName}' not found.");

        var switchLogger = new LoggingLevelSwitch(LogEventLevel.Warning);

        var logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(switchLogger)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.MSSqlServer(
                connectionString: connectionString,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = DataConstants.LogsTableName,
                    SchemaName = DataConstants.BugNetSchema,
                    AutoCreateSqlTable = false
                },
                columnOptions: new ColumnOptions
                {
                    AdditionalColumns = new Collection<SqlColumn>
                    {
                        new() { DataType = SqlDbType.NVarChar, ColumnName = nameof(Data.Entities.Log.IpAddress), AllowNull = true, DataLength = 55 },
                        new() { DataType = SqlDbType.NVarChar, ColumnName = nameof(Data.Entities.Log.UserName), AllowNull = true , DataLength = 255 },
                        new() { DataType = SqlDbType.NVarChar, ColumnName = nameof(Data.Entities.Log.Resource), AllowNull = true, DataLength = 1000 }
                    }
                })
            .CreateLogger();

        builder.Host.UseSerilog(logger);
        builder.Services.AddSingleton(logger);
        builder.Services.AddSingleton(switchLogger);

        return builder;
    }
}