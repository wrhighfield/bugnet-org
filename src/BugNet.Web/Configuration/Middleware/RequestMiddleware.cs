using System.Net.Sockets;
using Serilog.Context;

namespace BugNet.Web.Configuration.Middleware;

internal class RequestMiddleware
{
    private readonly RequestDelegate next;

	public RequestMiddleware(RequestDelegate next)
	{
		this.next = next;
	}

	public async Task Invoke(HttpContext context)
    {
		using (LogContext.PushProperty("IpAddress", GetValidIpAddress(context)))
        using (LogContext.PushProperty("Resource", context.Request.Path.ToString()))
        {
            await next(context);
        }
    }

    private static string GetValidIpAddress(HttpContext context)
    {
        var ipAddress = "localhost";

        if (context.Connection.RemoteIpAddress == null ||
            context.Connection.RemoteIpAddress.IsIPv6LinkLocal ||
            context.Connection.RemoteIpAddress.IsIPv6UniqueLocal ||
            context.Connection.RemoteIpAddress.IsIPv6SiteLocal)
        {
            return ipAddress;
        }


        switch (context.Connection.RemoteIpAddress.AddressFamily)
        {
            case AddressFamily.InterNetwork:
                return context.Connection.RemoteIpAddress.ToString();
            case AddressFamily.InterNetworkV6:
                ipAddress = context.Connection.RemoteIpAddress.ToString().Equals("::1")
                    ? ipAddress
                    : context.Connection.RemoteIpAddress.ToString();
                return ipAddress;
            default:
                return ipAddress;
        }
    }
}