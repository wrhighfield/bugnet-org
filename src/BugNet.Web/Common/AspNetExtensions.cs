using Microsoft.AspNetCore.Mvc.ModelBinding;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace BugNet.Web.Common;

public static class AspNetExtensions
{
	public static void LogModelErrors(this ModelErrorCollection errors, ILogger logger)
	{
		errors?
			.Where(p => p.Exception != null)
			.ForEach(p => logger.LogWarning(p.Exception, "Model Validation errors"));
	}

	public static void LogIdentityResultErrors(this IEnumerable<IdentityError> errors, ILogger logger)
	{
		errors?
			.Where(p => !string.IsNullOrWhiteSpace(p.Description))
			.ForEach(p => logger.LogWarning("Identity Validation error {Code:l}:{Description:l}", p.Code, p.Description));
	}

	public static string GetModelErrors(this ModelErrorCollection collection)
	{
		if (collection == null || !collection.Any())
		{
			return string.Empty;
		}

		return string.Join(';', collection.Select(p => p.ErrorMessage));
	}

	public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) => source.ForEach((_, item) => action(item));

	public static int ForEach<T>(this IEnumerable<T> source, Action<int, T> action)
	{
		if (action == null) throw new ArgumentNullException(nameof(action));

		var index = 0;

		foreach (var element in source)
		{
			action(index++, element);
		}

		return index;
	}
}