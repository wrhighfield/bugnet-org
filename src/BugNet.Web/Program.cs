using BugNet.Web.Configuration.Middleware;
using BugNet.Web.Configuration;
using BugNet.Web.Common;

namespace BugNet.Web;

public static class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);
		var configuration = builder.Configuration;

		builder
			.RegisterIdentity()
			.RegisterBugNetDbContext()
			.RegisterSerilog()
			.Services.AddControllersWithViews()
			.Services.AddLocalization(options =>
			{
				options.ResourcesPath = "Resources";
			})
			.AddAuthentication()
			.AddMicrosoftAccount(microsoftOptions =>
			{
				// this is configured using user secrets
				// aspnet-BugNet.Web-7714fa3b-8b44-4408-aa71-6a98962aa118
				// https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=windows#enable-secret-storage
				microsoftOptions.ClientId = configuration["Authentication:Microsoft:ClientId"]!;
				microsoftOptions.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"]!;
			});

		builder.Services.Configure<BugNetSettings>(configuration.GetSection(BugNetSettings.SectionName));

		builder.Services.ConfigureApplicationCookie(options =>
		{
			options.Cookie.Name = ".BugNet.Identity";
			options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
			options.SlidingExpiration = true;
		});

		ConfigureLocalizationServices(builder.Services);

		var app = builder.Build();

		// Configure the HTTP request pipeline.

		if (!app.Environment.AreDebugEnvironments())
		{
			app.UseExceptionHandler("/Home/Error");
			app.UseHsts();
		}
		else
		{
			var loggingLevelSwitch = app.Services.GetRequiredService<LoggingLevelSwitch>();
			loggingLevelSwitch.MinimumLevel = LogEventLevel.Information;
			Serilog.Debugging.SelfLog.Enable(Console.WriteLine);
		}

		// the order of these is important for authorization to work properly
		app
			.UseMiddleware<RequestMiddleware>()
			.UseSerilogRequestLogging()
			.UseHttpsRedirection()
			.UseRouting()
			.UseStaticFiles()
			.UseCookiePolicy()
			.UseAuthentication()
			.UseAuthorization()
			.UseRequestLocalization()
			.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
				endpoints.MapControllers();
				endpoints.MapRazorPages();
			});

		app
			.ApplyIdentitySchema(app.Services.GetRequiredService<Logger>())
			.ApplyBugnetSchema(app.Services.GetRequiredService<Logger>())
			.Run();
	}

	private static void ConfigureLocalizationServices(IServiceCollection services)
	{
		services.AddLocalization(options =>
		{
			options.ResourcesPath = "Resources";
		}).AddSingleton<CommonLocalizationService>();

		services.Configure<RequestLocalizationOptions>(options =>
		{
			options.SetDefaultCulture("en-Us");
			options.FallBackToParentUICultures = true;

			options
				.RequestCultureProviders
				.Remove(new AcceptLanguageHeaderRequestCultureProvider());

			//todo: need to create a custom provider to load the languages from the database
			options
				.RequestCultureProviders
				.Insert(0, new CustomRequestCultureProvider(_ => Task.FromResult(new ProviderCultureResult("en")))); ;
		});

		services
			.AddRazorPages()
			.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
			.AddDataAnnotationsLocalization();
	}
}
