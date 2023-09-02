using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace BugNet.Web.Providers;

public class ApplicationEmailConfirmationTokenProvider<TUser>
	: DataProtectorTokenProvider<TUser> where TUser : class
{
	public ApplicationEmailConfirmationTokenProvider(
		IDataProtectionProvider dataProtectionProvider,
		IOptions<EmailConfirmationTokenProviderOptions> options,
		ILogger<DataProtectorTokenProvider<TUser>> logger)
		: base(dataProtectionProvider, options, logger)
	{

	}
}