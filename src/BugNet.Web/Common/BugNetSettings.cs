namespace BugNet.Web.Common
{
	public class BugNetSettings
	{
		public const string SectionName = "BugNet";

		public BugNetTwoFactorAuthenticationSettings TwoFactorAuthentication { get; set; }
	}

	public class BugNetTwoFactorAuthenticationSettings
	{
		public bool EncryptionEnabled { get; set; }
		public string EncryptionKey { get; set; }
	}
}
