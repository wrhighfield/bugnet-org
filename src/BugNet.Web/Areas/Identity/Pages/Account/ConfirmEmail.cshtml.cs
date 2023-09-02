// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BugNet.Data;

namespace BugNet.Web.Areas.Identity.Pages.Account;

public class ConfirmEmailModel : PageModel
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IStringLocalizer<ConfirmEmailModel> pageStrings;

	public ConfirmEmailModel(
		UserManager<ApplicationUser> userManager,
		IStringLocalizer<ConfirmEmailModel> pageStrings)
	{
		this.userManager = userManager;
		this.pageStrings = pageStrings;
	}

    [TempData]
    public string Message { get; set; }

	public async Task<IActionResult> OnGetAsync(string userId, string code)
    {
        if (userId == null || code == null)
        {
            return RedirectToPage("/Index");
        }

        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        if (user.EmailConfirmed)
        {
	        return RedirectToPage("/Login");
		}

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await userManager.ConfirmEmailAsync(user, code);
        Message = result.Succeeded ? "Confirmation.Success" : "Confirmation.Error";
        return Page();
    }
}