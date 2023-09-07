using BugNet.Data;

namespace BugNet.Web.Common.Bases
{
	public class BugNetPageModeBase<T> : PageModel
	{
		protected readonly ILogger<T> Logger;

		public BugNetPageModeBase(ILogger<T> logger) => Logger = logger;
		
		protected bool IsModelValid
		{
			get
			{
				if (!ModelState.IsValid)
				{
					LogModelErrors();
				}

				return ModelState.IsValid;
			}
		}

		protected void LogModelErrors()
		{
			ModelState.Root.Errors.LogModelErrors(Logger);
		}

		protected void LogIdentityErrors(IEnumerable<IdentityError> errors)
		{
			errors.LogIdentityResultErrors(Logger);
		}

		protected void LogInformation(string message) => Logger.LogInformation(message);

		protected void LogWarning(string message) => Logger.LogWarning(message);

		protected void LogError(string message) => Logger.LogError(message);

		/// <summary>
		/// Mostly used for Identity lookups
		/// </summary>
		/// <param name="email">The email for the user that could not be located.</param>
		protected void LogUserNotFoundByEmail(string email) =>
			LogWarning($"AspNetNet user could not be found by email [{email}]");

		protected void LogUserWarning(ApplicationUser user, string message) =>
			LogWarning(string.Concat($"{user} ", message));

		protected void LogUserInformation(ApplicationUser user, string message) =>
			LogInformation(string.Concat($"{user} ", message));
	}
}
