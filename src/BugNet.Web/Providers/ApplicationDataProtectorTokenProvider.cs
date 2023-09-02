namespace BugNet.Web.Providers;

public class EmailConfirmationTokenProviderOptions : DataProtectionTokenProviderOptions
{
	public EmailConfirmationTokenProviderOptions()
	{
		Name = "EmailDataProtectorTokenProvider";
		TokenLifespan = TimeSpan.FromHours(3);
	}
}