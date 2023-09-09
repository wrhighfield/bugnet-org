using Ardalis.GuardClauses;

namespace BugNet.Web.Configuration;

internal static class HostEnvironmentEnvExtensions
{
	public static bool IsLocal(this IHostEnvironment hostEnvironment)
	{
		Guard.Against.Null(hostEnvironment, nameof(hostEnvironment));
		return hostEnvironment.IsEnvironment("Local");
	}

	public static bool AreDebugEnvironments(this IHostEnvironment hostEnvironment)
	{
		Guard.Against.Null(hostEnvironment, nameof(hostEnvironment));

		return hostEnvironment.IsLocal() || hostEnvironment.IsDevelopment();
	}
}