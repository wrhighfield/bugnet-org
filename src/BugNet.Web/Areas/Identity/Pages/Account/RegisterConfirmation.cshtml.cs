// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BugNet.Data;

namespace BugNet.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterConfirmationModel : PageModel
{
    private readonly UserManager<ApplicationUser> userManager;

    public RegisterConfirmationModel(UserManager<ApplicationUser> userManager) => this.userManager = userManager;

	public async Task<IActionResult> OnGetAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToPage("/Index");
        }

        var user = await userManager.FindByEmailAsync(email);

		if (user == null)
        {
            return NotFound($"Unable to load user with email '{email}'.");
        }

        if (!userManager.Options.SignIn.RequireConfirmedEmail)
        {
            return RedirectToPage("/Login");
        }

        if (user.EmailConfirmed && userManager.Options.SignIn.RequireConfirmedEmail)
        {
            return RedirectToPage("/Index");
        }

        return Page();
    }
}