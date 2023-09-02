using Serilog.Sinks.MSSqlServer;
using System.Collections.ObjectModel;
using System.Data;
using BugNet.Data;
using BugNet.Web.Providers;

namespace BugNet.Web.Configuration;

internal static class WebApplicationBuilderExtensions
{
	public static WebApplicationBuilder RegisterIdentity(this WebApplicationBuilder builder)
	{
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
			.AddIdentity<ApplicationUser, ApplicationRole>(options =>
			{
				options.SignIn.RequireConfirmedAccount = true;
				options.SignIn.RequireConfirmedEmail = true;
				options.Tokens.ProviderMap.Add("ApplicationEmailConfirmationTokenProvider",
					new TokenProviderDescriptor(
						typeof(ApplicationEmailConfirmationTokenProvider<ApplicationUser>)));
				options.Tokens.EmailConfirmationTokenProvider = "ApplicationEmailConfirmationTokenProvider";
			})
			.AddDefaultUI()
			.AddEntityFrameworkStores<IdentityDbContext>()
			.AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider);

		builder.Services.Configure<DataProtectionTokenProviderOptions>(o =>
			o.TokenLifespan = TimeSpan.FromHours(3));

		builder.Services.AddTransient<ApplicationEmailConfirmationTokenProvider<ApplicationUser>>();
		builder.Services.AddTransient<IEmailSender, IdentityEmailService>();

		return builder;
	}

	public static WebApplicationBuilder RegisterBugNetDbContext(this WebApplicationBuilder builder)
    {
        var bugNetConnectionString =
            builder
                .Configuration
                .GetConnectionString(DataConstants.BugNetConnectionStringName) ??
            throw new InvalidOperationException($"Connection string '{DataConstants.BugNetConnectionStringName}' not found.");

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