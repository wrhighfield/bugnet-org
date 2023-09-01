using Serilog;
using Serilog.Core;
using BugNet.Web.Configuration.Middleware;
using BugNet.Web.Configuration;
using Serilog.Events;

namespace BugNet.Web;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.RegisterSqlServer();
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
        builder.RegisterSerilog();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
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
            .UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });

        app.MigrateDatabase(app.Services.GetRequiredService<Logger>());
        app.Run();
    }
}