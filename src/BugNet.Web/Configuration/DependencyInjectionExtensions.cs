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
        var connectionString =
            builder
                .Configuration
                .GetConnectionString("BugNetConnection") ??
            throw new InvalidOperationException("Connection string 'BugNetConnection' not found.");

        builder
            .Services
            .AddDbContext<BugNetDbContext>(options =>
            options.UseSqlServer(connectionString));

        builder
            .Services
            .AddDatabaseDeveloperPageExceptionFilter();

        builder
            .Services
            .AddIdentity<BugNetUser, BugNetRole>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddDefaultUI()
            .AddEntityFrameworkStores<BugNetDbContext>();

        return builder;
    }
    public static WebApplicationBuilder RegisterSerilog(this WebApplicationBuilder builder)
    {
        var connectionString =
            builder
                .Configuration
                .GetConnectionString("BugNetConnection") ??
            throw new InvalidOperationException("Connection string 'BugNetConnection' not found.");

        var switchLogger = new LoggingLevelSwitch(LogEventLevel.Warning);

        var logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(switchLogger)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.MSSqlServer(
                connectionString: connectionString,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = "Logs",
                    SchemaName = "BugNet",
                    AutoCreateSqlTable = false
                },
                columnOptions: new ColumnOptions
                {
                    AdditionalColumns = new Collection<SqlColumn>
                    {
                        new() { DataType = SqlDbType.NVarChar, ColumnName = "IpAddress", AllowNull = true, DataLength = 55 },
                        new() { DataType = SqlDbType.NVarChar, ColumnName = "UserName", AllowNull = true , DataLength = 255 },
                        new() { DataType = SqlDbType.NVarChar, ColumnName = "Resource", AllowNull = true, DataLength = 1000 }
                    }
                })
            .CreateLogger();

        builder.Host.UseSerilog(logger);
        builder.Services.AddSingleton(logger);
        builder.Services.AddSingleton(switchLogger);

        return builder;
    }
}