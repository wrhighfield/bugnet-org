using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NETCore.Encrypt;
using static System.Boolean;

namespace BugNet.Data;

public class ApplicationRole : IdentityRole<Guid>
{
	public override string ToString() => $"AspNetRole: [{Name}:{Id}]";
}

public class ApplicationUser : IdentityUser<Guid>
{
	public override string ToString() => $"AspNetUser: [{Email}:{Id}]";
}

public class ApplicationUserManager : UserManager<ApplicationUser>
{
	private readonly IConfiguration configuration;

	public ApplicationUserManager(
		IUserStore<ApplicationUser> store,
		IOptions<IdentityOptions> optionsAccessor,
		IPasswordHasher<ApplicationUser> passwordHasher,
		IEnumerable<IUserValidator<ApplicationUser>> userValidators,
		IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
		ILookupNormalizer keyNormalizer,
		IdentityErrorDescriber errors,
		IServiceProvider services,
		ILogger<ApplicationUserManager> logger,
		IConfiguration configuration)
		: base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
		=> this.configuration = configuration;

	public override string GenerateNewAuthenticatorKey()
	{
		var originalAuthenticatorKey = base.GenerateNewAuthenticatorKey();

		var encryptedKey = IsTwoFactorAuthenticationEncryptionEnabled
			? EncryptProvider.AESEncrypt(originalAuthenticatorKey, EncryptionKey)
			: originalAuthenticatorKey;

		return encryptedKey;
	}

	public override async Task<string> GetAuthenticatorKeyAsync(ApplicationUser user)
	{
		var databaseKey = await base.GetAuthenticatorKeyAsync(user);

		if (databaseKey == null)
		{
			return null;
		}

		var originalAuthenticatorKey = IsTwoFactorAuthenticationEncryptionEnabled
			? EncryptProvider.AESDecrypt(databaseKey, EncryptionKey)
			: databaseKey;

		return originalAuthenticatorKey;
	}

	protected override string CreateTwoFactorRecoveryCode()
	{
		var originalRecoveryCode = base.CreateTwoFactorRecoveryCode();

		var encryptedRecoveryCode = IsTwoFactorAuthenticationEncryptionEnabled
			? EncryptProvider.AESEncrypt(originalRecoveryCode, EncryptionKey)
			: originalRecoveryCode;

		return encryptedRecoveryCode;
	}

	public override async Task<IEnumerable<string>> GenerateNewTwoFactorRecoveryCodesAsync(
		ApplicationUser user,
		int number)
	{
		var tokens = await base.GenerateNewTwoFactorRecoveryCodesAsync(user, number);

		var generatedTokens = tokens as string[] ?? tokens.ToArray();
		if (!generatedTokens.Any())
		{
			return generatedTokens;
		}

		return IsTwoFactorAuthenticationEncryptionEnabled
			? generatedTokens
				.Select(token =>
					EncryptProvider.AESDecrypt(token, EncryptionKey))
			: generatedTokens;

	}

	public override Task<IdentityResult> RedeemTwoFactorRecoveryCodeAsync(ApplicationUser user, string code)
	{
		if (IsTwoFactorAuthenticationEncryptionEnabled && !string.IsNullOrEmpty(code))
		{
			code = EncryptProvider.AESEncrypt(code, EncryptionKey);
		}

		return base.RedeemTwoFactorRecoveryCodeAsync(user, code);
	}

	private bool IsTwoFactorAuthenticationEncryptionEnabled
	{
		get
		{
			var success = TryParse(configuration["BugNet:TwoFactorAuthentication:EncryptionEnabled"], out var encryptionEnabled);
			return success && encryptionEnabled;
		}
	}

	private string EncryptionKey => configuration["BugNet:TwoFactorAuthentication:EncryptionKey"] ?? string.Empty;
}